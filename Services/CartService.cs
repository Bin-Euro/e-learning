using Amazon.Runtime.Internal;
using AutoMapper;
using Cursus.DTO;
using Cursus.DTO.Cart;
using Cursus.DTO.Course;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;

namespace Cursus.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public CartService(IUnitOfWork unitOfWork, ICartRepository repository, ICartItemRepository cartItemRepository,
            IUserService userService, IMapper mapper)
        {
            _repository = repository;
            _cartItemRepository = cartItemRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<ResultDTO<CartResponse>> GetByUserID()
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                if (user is null)
                    return ResultDTO<CartResponse>.Fail("User is not found");
                var CartByID = _repository.GetByUserID(user.Id);
                if (CartByID == null)
                {
                    return ResultDTO<CartResponse>.Fail("Cart is not Exist", 404);
                }

                var cartItemsDTO = (await _unitOfWork.CourseRepository
                        .GetAllAsync())
                    .Join(
                        CartByID.Items,
                        c => c.ID.ToString(),
                        item => item.CourseID,
                        (c, item) => new CartItemsDTO
                        {
                            CourseID = item.CourseID,
                            Description = c.Description,
                            Price = c.Price,
                            CourseName = c.Name,
                            CreateDate = item.CreatedDate,
                            InstructorID = c.InstructorID
                        });

                var cartResponse = new CartResponse()
                {
                    Id = CartByID.Id,
                    Items = cartItemsDTO.ToList(),
                    TotalPrice = cartItemsDTO.Sum(item => item.Price),
                    UserID = CartByID.UserID
                };
                return ResultDTO<CartResponse>.Success(cartResponse);
            }
            catch (Exception ex)
            {
                return ResultDTO<CartResponse>.Fail("Failed to add course: " + ex.Message);
            }
        }

        public async Task<ResultDTO<CartResponse>> ConfirmCart(string userId, List<string> courseIds)
        {
            try
            {
                var CartByID = _repository.GetByUserID(userId);
                if (CartByID == null)
                {
                    return ResultDTO<CartResponse>.Fail("Cart is not Exist");
                }

                var cartItems = new List<CartItem>();

                foreach (var courseId in courseIds)
                {
                    var cartItem = CartByID.Items.FirstOrDefault(item => item.CourseID == courseId);
                    if (cartItem != null)
                    {
                        cartItems.Add(cartItem);
                    }
                    else
                    {
                        return ResultDTO<CartResponse>.Fail("Some Course don't have in Cart");
                    }
                }

                if (cartItems != null)
                {
                    CartByID.Items = cartItems;
                }

                return ResultDTO<CartResponse>.Success(_mapper.Map<CartResponse>(CartByID));
            }
            catch (Exception ex)
            {
                return ResultDTO<CartResponse>.Fail("Failed to add course: " + ex.Message);
            }
        }

        public async Task<ResultDTO<CreateCart>> CreateCart(CreateCart request)
        {
            try
            {
                if (request.UserID == null)
                {
                    return ResultDTO<CreateCart>.Fail("Invalid input data");
                }

                var checkCart = _repository.GetByUserID(request.UserID);
                if (checkCart != null)
                {
                    return ResultDTO<CreateCart>.Fail("Cart is already existed!");
                }

                var newCart = new Cart
                {
                    UserID = request.UserID,
                    Items = new List<CartItem>
                    {
                    }
                };

                // Lưu đối tượng Cart mới vào cơ sở dữ liệu
                _repository.Create(newCart);

                return ResultDTO<CreateCart>.Success(request);
            }
            catch (Exception ex)
            {
                return ResultDTO<CreateCart>.Fail("Failed to add course: " + ex.Message);
            }
        }

        public async Task<ResultDTO<AddOrRemoveCartRequest>> AddToCart(AddOrRemoveCartRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CourseID))
                {
                    return ResultDTO<AddOrRemoveCartRequest>.Fail("Course ID is required", 400);
                }

                var checkCourse = await _unitOfWork.CourseRepository.GetAsync(c => c.ID.ToString() == request.CourseID);

                if (checkCourse == null)
                {
                    return ResultDTO<AddOrRemoveCartRequest>.Fail("Course is not Existed");
                }

                var user = await _userService.GetCurrentUser();

                if (user is null)
                    return ResultDTO<AddOrRemoveCartRequest>.Fail("User is not found");

                bool courseExistsInCart = _repository.GetByUserID(user.Id).Items
                    .Any(item => item.CourseID == request.CourseID);

                if (courseExistsInCart)
                {
                    return ResultDTO<AddOrRemoveCartRequest>.Fail("Course already exists in the cart");
                }

                var cartItem = new CartItem
                {
                    CourseID = request.CourseID,
                    CreatedDate = DateTime.UtcNow
                };

                _cartItemRepository.AddToCart(user.Id, cartItem);

                return ResultDTO<AddOrRemoveCartRequest>.Success(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<AddOrRemoveCartRequest>.Fail("Failed to add to cart");
            }
        }

        public async Task<ResultDTO<AddOrRemoveCartRequest>> RemoveItem(AddOrRemoveCartRequest request)
        {
            try
            {
                if (request.CourseID == null)
                {
                    return ResultDTO<AddOrRemoveCartRequest>.Fail("Invalid input data");
                }

                var checkCourse = await _unitOfWork.CourseRepository.GetAsync(c => c.ID.ToString() == request.CourseID);

                if (checkCourse == null)
                {
                    return ResultDTO<AddOrRemoveCartRequest>.Fail("Course is not Existed");
                }

                var user = await _userService.GetCurrentUser();

                var remove = _cartItemRepository.RemoveItem(user?.Id, request.CourseID);
                if (remove)
                {
                    return ResultDTO<AddOrRemoveCartRequest>.Success(request);
                }

                return ResultDTO<AddOrRemoveCartRequest>.Fail("Have some problem when removing!");
            }
            catch (Exception ex)
            {
                return ResultDTO<AddOrRemoveCartRequest>.Fail("Failed to remove: " + ex.Message);
            }
        }
    }
}