using System.Runtime.CompilerServices;
using System.Security.Claims;
using AutoMapper;
using Cursus.DTO;
using Cursus.DTO.User;
using Cursus.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Cursus.Constants;
using Cursus.Data;

namespace Cursus.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly MyDbContext _context;
        private readonly IMapper _mapper;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public UserService(UserManager<User> userManager, MyDbContext context, IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
        }

        public async Task<User> GetCurrentUser()
        {
            var userIdClaim = _claimsPrincipal.Claims
                .FirstOrDefault(c => c.Type == "Id");

            if (userIdClaim is null)
                return null;
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userIdClaim.Value);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<ResultDTO<UserProfileDTO>> GetUserProfile()
        {
            var user = await GetCurrentUser();
            if (user is not null)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                return ResultDTO<UserProfileDTO>.Success(new UserProfileDTO()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Image = user.Image,
                    Gender = user.Gender,
                    Role = role
                });
            }

            return ResultDTO<UserProfileDTO>.Fail("Not Found", 404);
        }

        public async Task<ResultDTO<List<UserDTO>>> GetAll()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userRoles = await _context.UserRoles.Join(
                    _context.Roles.AsQueryable(),
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new
                    {
                        UserId = ur.UserId,
                        RoleName = r.Name
                    }).ToListAsync();

                var userDTOs = users.Select(u =>
                    new UserDTO()
                    {
                        Id = Guid.Parse(u.Id),
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Username = u.UserName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        Image = u.Image,
                        Gender = u.Gender,
                        Role = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleName,
                        Status = u.Status,
                        CreatedDate = u.CreatedDate,
                        UpdatedDate = u.UpdatedDate
                    }
                ).ToList();

                return ResultDTO<List<UserDTO>>.Success(userDTOs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<List<UserDTO>>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<string>> UpdateUserProfile(UserProfileUpdateDTO updateUser)
        {
            try
            {
                const string PHONE_NUMBER_PATTERN = @"^(\+84|0)(3|5|7|8|9)([0-9]{8})$";

                // Check all fields are valid
                if (string.IsNullOrEmpty(updateUser.FirstName) ||
                    string.IsNullOrEmpty(updateUser.LastName) ||
                    string.IsNullOrEmpty(updateUser.PhoneNumber) ||
                    string.IsNullOrEmpty(updateUser.Address) ||
                    string.IsNullOrEmpty(updateUser.Image)
                   )
                {
                    return ResultDTO<string>.Fail("All fields are empty!", 400);
                }

                if (updateUser.FirstName.Length < 3 || updateUser.FirstName.Length > 50)
                    return ResultDTO<string>.Fail("First name must be between 3 and 50 characters!", 400);

                if (updateUser.LastName.Length < 3 || updateUser.LastName.Length > 50)
                    return ResultDTO<string>.Fail("Last name must be between 3 and 50 characters!", 400);

                if (!Regex.IsMatch(updateUser.PhoneNumber, PHONE_NUMBER_PATTERN))
                    return ResultDTO<string>.Fail("Invalid phone number format!", 400);

                if (!Enum.TryParse<UserGender>(updateUser.Gender, out var userGender))
                    return ResultDTO<string>.Fail("Invalid gender!", 400);

                // Get user by ID
                var currentUser = await GetCurrentUser();

                if (currentUser is null)
                    return ResultDTO<string>.Fail("Not Found", 404);

                // Update user information
                currentUser.FirstName = updateUser.FirstName;
                currentUser.LastName = updateUser.LastName;
                currentUser.PhoneNumber = updateUser.PhoneNumber;
                currentUser.Address = updateUser.Address;
                currentUser.Image = updateUser.Image;
                currentUser.Gender = Enum.GetName(userGender);

                var updateResult = await _userManager.UpdateAsync(_mapper.Map<User>(currentUser));
                if (!updateResult.Succeeded)
                {
                    return ResultDTO<string>.Fail(updateResult.Errors.Select(err => err.Description));
                }

                return ResultDTO<string>.Success("");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<string>> UpdateUserStatus(Guid id, string status)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id.ToString());
                if (user is null)
                    return ResultDTO<string>.Fail("User is not found", 404);
                
                if (!Enum.TryParse<UserStatus>(status, out var userStatus))
                    return ResultDTO<string>.Fail("Invalid user status", 400);

                user.Status = Enum.GetName(userStatus);
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return ResultDTO<string>.Fail(updateResult.Errors.Select(err => err.Description));
                }

                return ResultDTO<string>.Success("");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }
    }
}