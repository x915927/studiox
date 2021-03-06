using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudioX.Application.Services;
using StudioX.Application.Services.Dto;
using StudioX.Authorization;
using StudioX.Boilerplate.Authorization;
using StudioX.Boilerplate.Authorization.Roles;
using StudioX.Boilerplate.Authorization.Users;
using StudioX.Boilerplate.Users.Dto;
using StudioX.Collections.Extensions;
using StudioX.Domain.Repositories;
using StudioX.Domain.Uow;
using StudioX.IdentityFramework;
using StudioX.Linq.Extensions;
using StudioX.UI;

namespace StudioX.Boilerplate.Users
{
    [StudioXAuthorize(PermissionNames.System.Administration.Users.MainMenu)]
    public class UserAppService :
        AsyncCrudAppService<User, UserDto, long, PagedResultRequestDto, CreateUserInput, UpdateUserInput>,
        IUserAppService
    {
        private readonly UserManager userManager;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly RoleManager roleManager;

        public UserAppService(IRepository<User, long> userRepository,
            UserManager userManager,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher) : base(userRepository)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.passwordHasher = passwordHasher;

            GetAllPermissionName = PermissionNames.System.Administration.Users.MainMenu;
            GetPermissionName = PermissionNames.System.Administration.Users.MainMenu;
            CreatePermissionName = PermissionNames.System.Administration.Users.Create;
            UpdatePermissionName = PermissionNames.System.Administration.Users.Edit;
            DeletePermissionName = PermissionNames.System.Administration.Users.Delete;
        }

        protected override IQueryable<User> CreateFilteredQuery(PagedResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles).OrderByDescending(x => x.Id);
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            return await CreateFilteredQuery(null)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        protected override UserDto MapToEntityDto(User user)
         {
             var roles = roleManager.Roles.Where(r => user.Roles.Any(ur => ur.RoleId == r.Id)).Select(r => r.NormalizedName);
             var userDto = base.MapToEntityDto(user);
             userDto.RoleNames = roles.ToArray();
             return userDto;
         }

    protected override User MapToEntity(CreateUserInput createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();

            return user;
        }

        protected override void MapToEntity(UpdateUserInput input, User role)
        {
            ObjectMapper.Map(input, role);
            role.SetNormalizedNames();
        }

        protected override IQueryable<User> ApplyPaging(IQueryable<User> query, PagedResultRequestDto input)
        {
            // Try to use paging if available
            var pagedInput = input as IPagedResultRequest;
            if (pagedInput != null)
            {
                // Sort again after paging // .take resets the orderby
                query = query.PageBy(pagedInput);
                query = ApplySorting(query, input);
                return query;
            }

            // Try to limit query result if available
            var limitedInput = input as ILimitedResultRequest;
            if (limitedInput != null)
            {
                query = query.Take(limitedInput.MaxResultCount);
                query = ApplySorting(query, input);
                return query;
            }

            // No paging
            return query;
        }

        [UnitOfWork]
        public override async Task<UserDto> Create(CreateUserInput input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);
            user.TenantId = StudioXSession.TenantId;
            user.Password = passwordHasher.HashPassword(user, input.Password);
            user.IsEmailConfirmed = true;
            CheckErrors(await userManager.CreateAsync(user));
            CheckErrors(await userManager.SetRoles(user, input.RoleNames));

            CurrentUnitOfWork.SaveChanges();

            return MapToEntityDto(user);
        }

        [UnitOfWork]
        public override async Task<UserDto> Update(UpdateUserInput input)
        {
            CheckUpdatePermission();

            var user = await userManager.GetUserByIdAsync(input.Id);

            MapToEntity(input, user);

            CheckErrors(await userManager.UpdateAsync(user));
            CheckErrors(await userManager.SetRoles(user, input.RoleNames));

            // Get the user again after updating the roles
            return await Get(input);
        }

        [UnitOfWork]
        public override async Task Delete(EntityDto<long> input)
        {
            var user = await userManager.GetUserByIdAsync(input.Id);
            await userManager.DeleteAsync(user);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        [StudioXAuthorize(PermissionNames.System.Administration.Users.ChangePermission)]
        public async Task ProhibitPermission(ProhibitPermissionInput input)
        {
            var user = await userManager.GetUserByIdAsync(input.UserId);
            var permission = PermissionManager.GetPermission(input.PermissionName);

            await userManager.ProhibitPermissionAsync(user, permission);
        }

        [StudioXAuthorize(PermissionNames.System.Administration.Users.ChangePermission)]
        public async Task ResetUserSpecificPermissions(long id)
        {
            var user = await userManager.GetUserByIdAsync(id);
            await userManager.ResetAllPermissionsAsync(user);
        }

        [StudioXAuthorize(PermissionNames.System.Administration.Users.Edit)]
        public async Task Unlock(long id)
        {
            var user = await userManager.GetUserByIdAsync(id);
            await userManager.ResetAccessFailedCountAsync(user);
        }

        [StudioXAuthorize(PermissionNames.System.Administration.Users.ChangePermission)]
        public async Task UpdateUserPermissions(UserPermissionsInput input)
        {
            var user = await userManager.GetUserByIdAsync(input.Id);

            var grantedPermissions = PermissionManager
                .GetAllPermissions()
                .Where(p => input.GrantedPermissionNames.Contains(p.Name))
                .ToList();

            await userManager.SetGrantedPermissionsAsync(user, grantedPermissions);
        }

        [StudioXAuthorize(PermissionNames.System.Administration.Users.Edit)]
        public async Task ResetPassword(ChangeUserPasswordInput input)
        {
            var user = await userManager.GetUserByIdAsync(input.Id);
            var identityResult = await userManager.ChangePasswordAsync(user, input.Password);

            if (!identityResult.Succeeded)
            {
                throw new UserFriendlyException(identityResult.Errors.JoinAsString(", "));
            }
        }
    }
}