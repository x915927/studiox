﻿using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using StudioX.Authorization.Users;
using StudioX.Dependency;
using StudioX.Extensions;
using StudioX.MultiTenancy;
using StudioX.Zero.Ldap.Configuration;

namespace StudioX.Zero.Ldap.Authentication
{
    /// <summary>
    /// Implements <see cref="IExternalAuthenticationSource{TTenant,TUser}"/> to authenticate users from LDAP.
    /// Extend this class using application's User and Tenant classes as type parameters.
    /// Also, all needed methods can be overridden and changed upon your needs.
    /// </summary>
    /// <typeparam name="TTenant">Tenant type</typeparam>
    /// <typeparam name="TUser">User type</typeparam>
    public abstract class LdapAuthenticationSource<TTenant, TUser> : DefaultExternalAuthenticationSource<TTenant, TUser>, ITransientDependency
        where TTenant : StudioXTenant<TUser>
        where TUser : StudioXUserBase, new()
    {
        /// <summary>
        /// LDAP
        /// </summary>
        public const string SourceName = "LDAP";

        public override string Name => SourceName;

        private readonly ILdapSettings settings;
        private readonly IStudioXZeroLdapModuleConfig ldapModuleConfig;

        protected LdapAuthenticationSource(ILdapSettings settings, IStudioXZeroLdapModuleConfig ldapModuleConfig)
        {
            this.settings = settings;
            this.ldapModuleConfig = ldapModuleConfig;
        }

        /// <inheritdoc/>
        public override async Task<bool> TryAuthenticateAsync(string userNameOrEmailAddress, string plainPassword, TTenant tenant)
        {
            if (!ldapModuleConfig.IsEnabled || !(await settings.GetIsEnabled(GetIdOrNull(tenant))))
            {
                return false;
            }

            using (var principalContext = await CreatePrincipalContext(tenant))
            {
                return ValidateCredentials(principalContext, userNameOrEmailAddress, plainPassword);
            }
        }

        /// <inheritdoc/>
        public async override Task<TUser> CreateUserAsync(string userNameOrEmailAddress, TTenant tenant)
        {
            await CheckIsEnabled(tenant);

            var user = await base.CreateUserAsync(userNameOrEmailAddress, tenant);

            using (var principalContext = await CreatePrincipalContext(tenant))
            {
                var userPrincipal = UserPrincipal.FindByIdentity(principalContext, userNameOrEmailAddress);

                if (userPrincipal == null)
                {
                    throw new StudioXException("Unknown LDAP user: " + userNameOrEmailAddress);
                }

                UpdateUserFromPrincipal(user, userPrincipal);

                user.IsEmailConfirmed = true;
                user.IsActive = true;

                return user;
            }
        }

        public async override Task UpdateUserAsync(TUser user, TTenant tenant)
        {
            await CheckIsEnabled(tenant);

            await base.UpdateUserAsync(user, tenant);

            using (var principalContext = await CreatePrincipalContext(tenant))
            {
                var userPrincipal = UserPrincipal.FindByIdentity(principalContext, user.UserName);

                if (userPrincipal == null)
                {
                    throw new StudioXException("Unknown LDAP user: " + user.UserName);
                }

                UpdateUserFromPrincipal(user, userPrincipal);
            }
        }

        protected virtual bool ValidateCredentials(PrincipalContext principalContext, string userNameOrEmailAddress, string plainPassword)
        {
            return principalContext.ValidateCredentials(userNameOrEmailAddress, plainPassword, ContextOptions.Negotiate);
        }

        protected virtual void UpdateUserFromPrincipal(TUser user, UserPrincipal userPrincipal)
        {
            user.UserName = userPrincipal.SamAccountName;
            user.FirstName = userPrincipal.GivenName;
            user.LastName = userPrincipal.Surname;
            user.EmailAddress = userPrincipal.EmailAddress;

            if (userPrincipal.Enabled.HasValue)
            {
                user.IsActive = userPrincipal.Enabled.Value;
            }
        }

        protected virtual async Task<PrincipalContext> CreatePrincipalContext(TTenant tenant)
        {
            var tenantId = GetIdOrNull(tenant);
            
            return new PrincipalContext(
                await settings.GetContextType(tenantId),
                ConvertToNullIfEmpty(await settings.GetDomain(tenantId)),
                ConvertToNullIfEmpty(await settings.GetContainer(tenantId)),
                ConvertToNullIfEmpty(await settings.GetUserName(tenantId)),
                ConvertToNullIfEmpty(await settings.GetPassword(tenantId))
                );
        }

        private async Task CheckIsEnabled(TTenant tenant)
        {
            if (!ldapModuleConfig.IsEnabled)
            {
                throw new StudioXException("Ldap Authentication module is disabled globally!");                
            }

            var tenantId = GetIdOrNull(tenant);
            if (!await settings.GetIsEnabled(tenantId))
            {
                throw new StudioXException("Ldap Authentication is disabled for given tenant (id:" + tenantId + ")! You can enable it by setting '" + LdapSettingNames.IsEnabled + "' to true");
            }
        }

        private static int? GetIdOrNull(TTenant tenant)
        {
            return tenant == null
                ? (int?)null
                : tenant.Id;
        }

        private static string ConvertToNullIfEmpty(string str)
        {
            return str.IsNullOrWhiteSpace()
                ? null
                : str;
        }
    }
}
