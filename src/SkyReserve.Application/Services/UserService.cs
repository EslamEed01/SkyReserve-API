using AutoMapper;
using Learnova.Business.DTOs.Contract.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkyReserve.Application.Consts;
using SkyReserve.Application.Contract.Users;
using SkyReserve.Application.Interfaces;
using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users
                .Where(u => !u.IsDisabled)
                .ToListAsync(cancellationToken);

            var userResponses = new List<UserResponse>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userResponses.Add(new UserResponse(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email!,
                    user.RoleType,
                    user.IsDisabled,
                    roles.ToList()
                ));
            }

            return userResponses;
        }

        public async Task<Result<UserResponse>> GetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Result.Failure<UserResponse>(UserErrors.UserNotFound);

            var roles = await _userManager.GetRolesAsync(user);
            var response = new UserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                user.RoleType,
                user.IsDisabled,
                roles.ToList()
            );

            return Result.Success(response);
        }

        public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.email);
            if (existingUser != null)
                return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

            var user = new ApplicationUser
            {
                Email = request.email,
                UserName = request.email,
                FirstName = request.firstName,
                LastName = request.lastName,
                EmailConfirmed = true,
                RoleType = request.Roles.Contains("SuperAdmin") ? "SuperAdmin" : "User"
            };

            var result = await _userManager.CreateAsync(user, request.password);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                return Result.Failure<UserResponse>(new Error(error.Code, error.Description, 400));
            }

            // Assign roles
            if (request.Roles.Any())
            {
                await _userManager.AddToRolesAsync(user, request.Roles);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var response = new UserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.RoleType,
                user.IsDisabled,
                roles.ToList()
            );

            return Result.Success(response);
        }

        public async Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Result.Failure(UserErrors.UserNotFound);

            user.FirstName = request.firstName;
            user.LastName = request.lastName;
            

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                return Result.Failure(new Error(error.Code, error.Description, 400));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(request.Roles);
            var rolesToAdd = request.Roles.Except(currentRoles);

            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(user, rolesToAdd);

            return Result.Success();
        }

        public async Task<Result> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Result.Failure(UserErrors.UserNotFound);

            user.IsDisabled = !user.IsDisabled;
            await _userManager.UpdateAsync(user);

            return Result.Success();
        }

        public async Task<Result> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Result.Failure(UserErrors.UserNotFound);

            await _userManager.SetLockoutEndDateAsync(user, null);
            return Result.Success();
        }

        public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

            var response = new UserProfileResponse(
                user.Email!,
                user.UserName!,
                user.FirstName,
                user.LastName,
                user.PhoneNumber ?? string.Empty,
                user.CreatedAt
            );

            return Result.Success(response);
        }

        public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(UserErrors.UserNotFound);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                return Result.Failure(new Error(error.Code, error.Description, 400));
            }

            return Result.Success();
        }

        public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure(UserErrors.UserNotFound);

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                return Result.Failure(new Error(error.Code, error.Description, 400));
            }

            return Result.Success();
        }
    }
}
