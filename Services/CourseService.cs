using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Course;
using Cursus.DTO.CourseCatalog;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Identity;

namespace Cursus.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICatalogService _catalogService;
        private readonly ICourseCatalogService _courseCatalogService;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly ICartItemRepository _cartItemRepository;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper, ICourseCatalogService courseCatalogService,
            ICatalogService catalogService, UserManager<User> userManager, IUserService userService, ICartItemRepository cartItemRepository
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _catalogService = catalogService;
            _courseCatalogService = courseCatalogService;
            _userManager = userManager;
            _userService = userService;
            _cartItemRepository = cartItemRepository;
        }

        private IEnumerable<CourseDTO> GetCourses()
        {
            var courseCatalogs = _unitOfWork.CourseCatalogRepository.GetAll();

            var courseFeedbacks = _unitOfWork.CourseFeedbackRepository.GetQueryable()
                .GroupBy(cf => cf.CourseID)
                .Select(g => new
                {
                    CourseID = g.Key,
                    AvgRate = g.Average(cf => cf.Rate)
                }).ToList();

            var registrations = _unitOfWork.OrderDetailRepository.GetQueryable()
                .GroupBy(r => r.CourseID)
                .Select(g => new
                {
                    CourseID = g.Key,
                    LearnerQuanity = g.Count()
                })
                .ToList();

            var instructors = _unitOfWork.InstructorRepository.GetQueryable()
                .Join(
                    _userManager.Users.AsQueryable(),
                    i => i.UserID.ToString(),
                    u => u.Id,
                    (i, u) => new InstructorDTO()
                    {
                        ID = i.ID,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Image = u.Image
                    })
                .ToList();

            var courseDTOs = _unitOfWork.CourseRepository.GetAll(c => !c.IsDeleted)
                .Select(c => new CourseDTO()
                {
                    ID = c.ID,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    Image = c.Image,
                    VideoIntroduction = c.VideoIntroduction,
                    Outcome = c.Outcome,
                    CatalogIDs = courseCatalogs
                        .Where(cc => cc.CourseID.Equals(c.ID))
                        .Select(cc => cc.CatalogID).ToList(),
                    AvgRate = courseFeedbacks
                        .FirstOrDefault(cf => cf.CourseID.Equals(c.ID))?.AvgRate ?? 0,
                    LearnerQuantity = registrations
                        .FirstOrDefault(r => r.CourseID.Equals(c.ID))?.LearnerQuanity ?? 0,
                    Instructor = instructors.FirstOrDefault(i => i.ID.Equals(c.InstructorID)),
                    CreatedDate = c.CreatedDate
                });

            return courseDTOs;
        }

        public ResultDTO<CourseListDTO> GetCoursesByFilter(
            int offset, int limit,
            double minPrice, double maxPrice, List<Guid> catalogIDs,
            CourseSort courseSort
        )
        {
            if (offset < 0 || limit <= 0)
            {
                return ResultDTO<CourseListDTO>.Fail(
                    "offset must be greater or equal to 0 and limit must be greater than 0");
            }

            if (minPrice < 0 || maxPrice <= 0)
            {
                return ResultDTO<CourseListDTO>.Fail(
                    "minimum price must be greater or equal to 0 and maximum price must be greater than 0"
                );
            }

            try
            {
                var courseDTOs = GetCourses()
                    .Where(
                        c => c.Price >= minPrice &&
                             c.Price <= maxPrice
                    );
                if (catalogIDs.Any())
                    courseDTOs = courseDTOs.Where(c => c.CatalogIDs.Intersect(catalogIDs).Any());


                switch (courseSort)
                {
                    case CourseSort.TopRate:
                        courseDTOs = courseDTOs.OrderByDescending(c => c.AvgRate);
                        break;
                    case CourseSort.AscName:
                        courseDTOs = courseDTOs.OrderBy(c => c.Name);
                        break;
                    case CourseSort.DscName:
                        courseDTOs = courseDTOs.OrderByDescending(c => c.Name);
                        break;
                    case CourseSort.Newest:
                        courseDTOs = courseDTOs.OrderByDescending(c => c.CreatedDate);
                        break;
                    case CourseSort.Oldest:
                        courseDTOs = courseDTOs.OrderBy(c => c.CreatedDate);
                        break;
                }

                var result = new CourseListDTO()
                {
                    List = courseDTOs.Skip(offset).Take(limit).ToList(),
                    Total = courseDTOs.Count(),
                    SortBy = Enum.GetName(courseSort)
                };

                return ResultDTO<CourseListDTO>.Success(result);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<CourseListDTO>.Fail("service is not available");
            }
        }

        public async Task<ResultDTO<CourseListDTO>> GetCoursesByInstructor(
            int offset, int limit
        )
        {
            if (offset < 0 || limit <= 0)
            {
                return ResultDTO<CourseListDTO>.Fail(
                    "offset must be greater or equal to 0 and limit must be greater than 0");
            }

            try
            {
                var currentUser = await _userService.GetCurrentUser();

                if (currentUser is null)
                    return ResultDTO<CourseListDTO>.Fail("Instructor is not found");

                var instructor =
                    await _unitOfWork.InstructorRepository.GetAsync(i => i.UserID == Guid.Parse(currentUser.Id));
                if (instructor is null)
                    return ResultDTO<CourseListDTO>.Fail("Instructor is not found");
                
                var courseDTOs = GetCourses().Where(c => c.Instructor is not null && c.Instructor.ID == instructor.ID);

                var result = new CourseListDTO()
                {
                    List = courseDTOs.Skip(offset).Take(limit).ToList(),
                    Total = courseDTOs.Count()
                };

                return ResultDTO<CourseListDTO>.Success(result);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<CourseListDTO>.Fail("service is not available");
            }
        }

        public async Task<ResultDTO<CreateCourseResDTO>> AddCourse(CreateCourseReqDTO courseRequest)
            {
                if (string.IsNullOrEmpty(courseRequest.Outcome))
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Outcome is required.");
                }

                if (string.IsNullOrEmpty(courseRequest.Image))
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Image is required.");
                }

                if (string.IsNullOrEmpty(courseRequest.VideoIntroduction))
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Video introduction is required.");
                }

                if (courseRequest.Price < 0 || courseRequest.Price > 500)
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Price should be between 0 and 500.");
                }

                if (string.IsNullOrEmpty(courseRequest.Description))
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Description is required.");
                }

                if (string.IsNullOrEmpty(courseRequest.Name))
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Name is required.");
                }

                if (courseRequest.CatalogIDs.Count <= 0)
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Catalog ID is required.");
                }

                var currentUser = await _userService.GetCurrentUser();

                if (currentUser is null)
                    return ResultDTO<CreateCourseResDTO>.Fail("Instructor is not found");

                var instructor =
                    await _unitOfWork.InstructorRepository.GetAsync(i => i.UserID == Guid.Parse(currentUser.Id));
                if (instructor is null)
                    return ResultDTO<CreateCourseResDTO>.Fail("Instructor is not found");

                var _course = new Course
                {
                    ID = Guid.NewGuid(),
                    Name = courseRequest.Name,
                    Description = courseRequest.Description,
                    Price = courseRequest.Price,
                    Outcome = courseRequest.Outcome,
                    Image = courseRequest.Image,
                    VideoIntroduction = courseRequest.VideoIntroduction,
                    InstructorID = instructor.ID,
                };
                if (courseRequest.CatalogIDs != null)
                {
                    var hasErrors = false;
                    var distinctCatalogIDs = courseRequest.CatalogIDs.Distinct().ToList();
                    if (distinctCatalogIDs.Count < courseRequest.CatalogIDs.Count)
                    {
                        hasErrors = true;
                        return ResultDTO<CreateCourseResDTO>.Fail("Duplicate catalog IDs found.");
                    }

                    foreach (var catalogID in distinctCatalogIDs)
                    {
                        var catalogExists = await _catalogService.CatalogExists(catalogID);
                        if (!catalogExists._isSuccess)
                        {
                            hasErrors = true;
                            return ResultDTO<CreateCourseResDTO>.Fail("Catalogs does not exist:" + catalogID);
                        }
                    }

                    if (!hasErrors)
                    {
                        foreach (var catalogID in distinctCatalogIDs)
                        {
                            var catalogExists = await _catalogService.CatalogExists(catalogID);
                            var courseCatalog = new CourseCatalogReqDTO
                            {
                                CatalogID = catalogExists._data.ID,
                                CourseID = _course.ID
                            };

                            var result = await _courseCatalogService.AddCourseCatalog(courseCatalog);
                            if (!result._isSuccess)
                            {
                                return ResultDTO<CreateCourseResDTO>.Fail("Failed to add course catalog: " + result);
                            }
                        }
                    }
                }

                _unitOfWork.CourseRepository.Add(_course);

                try
                {
                    await _unitOfWork.CommitAsync();
                    var course = _mapper.Map<CreateCourseResDTO>(_course);
                    return ResultDTO<CreateCourseResDTO>.Success(course);
                }

                catch (Exception ex)
                {
                    return ResultDTO<CreateCourseResDTO>.Fail("Failed to add course: " + ex.Message);
                }
            }

            public async Task Update(UpdateCourseDTO updateCourse)
            {
                try
                {
                    //check model is valid
                    if (updateCourse == null)
                    {
                        throw new Exception("Course not found");
                    }

                    string message = "";
                    //check all fields are valid
                    if (updateCourse.ID == null || string.IsNullOrEmpty(updateCourse.Name) ||
                        string.IsNullOrEmpty(updateCourse.Description) ||
                        updateCourse.Price == null || updateCourse.CatalogIDs == null ||
                        string.IsNullOrEmpty(updateCourse.Outcome) ||
                        string.IsNullOrEmpty(updateCourse.Image) ||
                        string.IsNullOrEmpty(updateCourse.VideoIntroduction))
                    {
                        message = "All fields is empty!";
                    }
                    else if (updateCourse.Name.Length < 3 || updateCourse.Name.Length > 50)
                    {
                        message = "Name must be between 3 and 50 characters!";
                    }
                    else if (updateCourse.Description.Length < 3 || updateCourse.Description.Length > 50)
                    {
                        message = "Description must be between 3 and 50 characters!";
                    }
                    else if (updateCourse.Price < 0)
                    {
                        message = "Price must be greater than 0!";
                    }
                    else if (await _unitOfWork.CourseRepository.GetAsync(x => x.ID.Equals(updateCourse.ID)) == null)
                    {
                        message = "Course not found!";
                    }

                    //if List catalog name is null
                    if (updateCourse.CatalogIDs == null)
                    {
                        message = "List catalog ID is null!";
                    }
                    else if (updateCourse.CatalogIDs.Count == 0)
                    {
                        message = "List catalog ID is empty!";
                    }
                    else
                    {
                        foreach (var catalogID in updateCourse.CatalogIDs)
                        {
                            //check exist catalog name
                            if (!(await _catalogService.CatalogExists(catalogID))._isSuccess)
                            {
                                message = "Catalog ID not found!";
                            }
                        }
                    }

                    //if any field is invalid, throw exception
                    if (message != "")
                    {
                        throw new ExceptionError(400, message);
                    }

                    //Get course by id and catalogCourse by course id
                    var course = await _unitOfWork.CourseRepository.GetAsync(x => x.ID.Equals(updateCourse.ID));
                    var catalogCourses = await _unitOfWork.CourseCatalogRepository
                        .GetAllAsync(x => x.CourseID.Equals(updateCourse.ID));
                    //update course and catalogCourse
                    course.Name = updateCourse.Name;
                    course.Description = updateCourse.Description;
                    course.Price = updateCourse.Price;
                    course.Outcome = updateCourse.Outcome;
                    course.Image = updateCourse.Image;
                    course.VideoIntroduction = updateCourse.VideoIntroduction;
                    foreach (var catalogCourse in catalogCourses)
                    {
                        _unitOfWork.CourseCatalogRepository.Remove(catalogCourse);
                    }

                    //add new catalogCourse
                    foreach (var catalogID in updateCourse.CatalogIDs)
                    {
                        var catalog = (await _catalogService.CatalogExists(catalogID))._data;
                        var catalogCourse = new CourseCatalog
                        {
                            CourseID = updateCourse.ID,
                            CatalogID = catalog.ID
                        };
                        _unitOfWork.CourseCatalogRepository.Add(catalogCourse);
                    }

                    //save change
                    await _unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public async Task<ResultDTO<string>> DeleteCourse(Guid id)
            {
                try
                {
                    var course = await _unitOfWork.CourseRepository.GetAsync(
                        c => c.ID.Equals(id)
                    );

                    if (course is null)
                        return ResultDTO<string>.Fail("Course is not found", 404);

                    course.IsDeleted = true;
                    _unitOfWork.CourseRepository.Update(course);
                    await _unitOfWork.CommitAsync();
                    var users = await _userManager.GetUsersInRoleAsync("User");
                    foreach (var user in users)
                    {
                        _cartItemRepository.RemoveItem(user.Id, id.ToString());                        
                    }

                    return ResultDTO<string>.Success("", "Course is deleted");
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    return ResultDTO<string>.Fail("Service is not available");
                }
            }
        }
    }