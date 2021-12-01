using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace GhostNetwork.AspNetCore.Identity.Mongo
{
    internal class UserStore<TUser, TRole, TKey, TUserRole, TUserClaim, TUserLogin, TUserToken> :
        UserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>,
        IUserRoleStore<TUser>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
    {
        private readonly IdentityDbContext<TKey> context;

        public UserStore(IdentityDbContext<TKey> context)
            : base(new IdentityErrorDescriber())
        {
            this.context = context;
        }

        public override IQueryable<TUser> Users => UsersCollection.AsQueryable();

        private IMongoCollection<TUser> UsersCollection => context.Collection<TUser>("users");

        private IMongoCollection<TRole> Roles => context.Collection<TRole>("roles");

        private IMongoCollection<TUserRole> UserRoles => context.Collection<TUserRole>("userRoles");

        private IMongoCollection<TUserClaim> UserClaims => context.Collection<TUserClaim>("userClaims");

        private IMongoCollection<TUserLogin> UserLogins => context.Collection<TUserLogin>("userLogins");

        private IMongoCollection<TUserToken> UserTokens => context.Collection<TUserToken>("userTokens");

        public override async Task AddClaimsAsync(
            TUser user,
            IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default)
        {
            await UserClaims.InsertManyAsync(
                claims
                    .Select(c => new TUserClaim
                    {
                        UserId = user.Id,
                        ClaimType = c.Type,
                        ClaimValue = c.Value
                    }),
                cancellationToken: cancellationToken);
        }

        public override async Task AddLoginAsync(
            TUser user,
            UserLoginInfo login,
            CancellationToken cancellationToken = default)
        {
            await UserLogins.InsertOneAsync(
                new TUserLogin
                {
                    UserId = user.Id,
                    LoginProvider = login.LoginProvider,
                    ProviderDisplayName = login.ProviderDisplayName,
                    ProviderKey = login.ProviderKey
                },
                cancellationToken: cancellationToken);
        }

        public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
        {
            await UsersCollection.InsertOneAsync(user, cancellationToken: cancellationToken);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default)
        {
            await UsersCollection.DeleteOneAsync(Builders<TUser>.Filter.Eq(u => u.Id, user.Id), cancellationToken);

            return IdentityResult.Success;
        }

        public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            return await UsersCollection
                .Find(Builders<TUser>.Filter.Eq(u => u.NormalizedEmail, normalizedEmail))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await UsersCollection
                .Find(Builders<TUser>.Filter.Eq(u => u.Id, ConvertIdFromString(userId)))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            return await UsersCollection
                .Find(Builders<TUser>.Filter.Eq(u => u.NormalizedUserName, normalizedUserName))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            var ucs = await UserClaims
                .Find(Builders<TUserClaim>.Filter.Eq(u => u.UserId, user.Id))
                .ToListAsync(cancellationToken);

            return ucs.Select(cs => cs.ToClaim()).ToList();
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            var uls = await UserLogins
                .Find(Builders<TUserLogin>.Filter.Eq(u => u.UserId, user.Id))
                .ToListAsync(cancellationToken);

            return uls.Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName)).ToList();
        }

        public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            var claimFilter = Builders<TUserClaim>.Filter.And(
                Builders<TUserClaim>.Filter.Eq(uc => uc.ClaimValue, claim.Value),
                Builders<TUserClaim>.Filter.Eq(uc => uc.ClaimType, claim.Type));

            var ucs = await UserClaims
                .Find(claimFilter)
                .ToListAsync(cancellationToken);

            return await UsersCollection
                .Find(Builders<TUser>.Filter.In(u => u.Id, ucs.Select(uc => uc.UserId)))
                .ToListAsync(cancellationToken);
        }

        public override async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            var claimsFilter = Builders<TUserClaim>.Filter.Or(claims.Select(c => Builders<TUserClaim>.Filter.And(
                Builders<TUserClaim>.Filter.Eq(uc => uc.ClaimValue, c.Value),
                Builders<TUserClaim>.Filter.Eq(uc => uc.ClaimType, c.Type))));

            var filter = Builders<TUserClaim>.Filter.And(
                Builders<TUserClaim>.Filter.Eq(uc => uc.UserId, user.Id),
                claimsFilter);

            await UserClaims.DeleteManyAsync(filter, cancellationToken);
        }

        public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TUserLogin>.Filter.And(
                Builders<TUserLogin>.Filter.Eq(ul => ul.UserId, user.Id),
                Builders<TUserLogin>.Filter.Eq(ul => ul.LoginProvider, loginProvider),
                Builders<TUserLogin>.Filter.Eq(ul => ul.ProviderKey, providerKey));

            await UserLogins.DeleteOneAsync(filter, cancellationToken);
        }

        public override async Task ReplaceClaimAsync(
            TUser user,
            Claim claim,
            Claim newClaim,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<TUserClaim>.Filter.And(
                Builders<TUserClaim>.Filter.Eq(uc => uc.UserId, user.Id),
                Builders<TUserClaim>.Filter.Eq(uc => uc.ClaimValue, claim.Value),
                Builders<TUserClaim>.Filter.Eq(uc => uc.ClaimType, claim.Type));

            await UserClaims.ReplaceOneAsync(
                filter,
                new TUserClaim
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                },
                cancellationToken: cancellationToken);
        }

        public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TUser>.Filter.Eq(u => u.Id, user.Id);

            await UsersCollection.ReplaceOneAsync(filter, user, cancellationToken: cancellationToken);

            return IdentityResult.Success;
        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var role = await Roles
                .Find(Builders<TRole>.Filter.Eq(r => r.NormalizedName, roleName))
                .FirstAsync(cancellationToken);

            await UserRoles.InsertOneAsync(
                new TUserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                },
                cancellationToken: cancellationToken);
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            var urs = await UserRoles.Find(Builders<TUserRole>.Filter.Eq(ur => ur.UserId, user.Id)).ToListAsync(cancellationToken);
            var rs = await Roles.Find(Builders<TRole>.Filter.In(r => r.Id, urs.Select(ur => ur.RoleId))).ToListAsync(cancellationToken);

            return rs.Select(r => r.Name).ToList();
        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = await Roles.Find(Builders<TRole>.Filter.Eq(r => r.NormalizedName, roleName)).FirstAsync(cancellationToken);
            var urs = await UserRoles
                .Find(Builders<TUserRole>.Filter.Eq(ur => ur.RoleId, role.Id))
                .ToListAsync(cancellationToken);

            return await UsersCollection.Find(Builders<TUser>.Filter.In(r => r.Id, urs.Select(ur => ur.UserId))).ToListAsync(cancellationToken);
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var role = await Roles.Find(Builders<TRole>.Filter.Eq(r => r.NormalizedName, roleName))
                .FirstAsync(cancellationToken);

            var filter = Builders<TUserRole>.Filter.And(
                Builders<TUserRole>.Filter.Eq(ur => ur.UserId, user.Id),
                Builders<TUserRole>.Filter.Eq(ur => ur.RoleId, role.Id));

            return await UserRoles
                .Find(filter)
                .AnyAsync(cancellationToken);
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var role = await Roles.Find(Builders<TRole>.Filter.Eq(r => r.NormalizedName, roleName)).FirstAsync(cancellationToken);

            var filter = Builders<TUserRole>.Filter.And(
                Builders<TUserRole>.Filter.Eq(ur => ur.UserId, user.Id),
                Builders<TUserRole>.Filter.Eq(ur => ur.RoleId, role.Id));
            await UserRoles.DeleteOneAsync(filter, cancellationToken);
        }

        protected override async Task RemoveUserTokenAsync(TUserToken token)
        {
            var filter = Builders<TUserToken>.Filter.And(
                Builders<TUserToken>.Filter.Eq(ul => ul.UserId, token.UserId),
                Builders<TUserToken>.Filter.Eq(ul => ul.LoginProvider, token.LoginProvider));

            await UserTokens.DeleteOneAsync(filter);
        }

        protected override async Task AddUserTokenAsync(TUserToken token)
        {
            await UserTokens.InsertOneAsync(token);
        }

        protected override async Task<TUserToken> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var filter = Builders<TUserToken>.Filter.And(
                Builders<TUserToken>.Filter.Eq(u => u.LoginProvider, loginProvider),
                Builders<TUserToken>.Filter.Eq(u => u.Name, name),
                Builders<TUserToken>.Filter.Eq(u => u.UserId, user.Id));

            return await UserTokens.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        protected override async Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken)
        {
            var filter = Builders<TUser>.Filter.Eq(u => u.Id, userId);

            return await UsersCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        protected override async Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var filter = Builders<TUserLogin>.Filter.And(
                Builders<TUserLogin>.Filter.Eq(u => u.LoginProvider, loginProvider),
                Builders<TUserLogin>.Filter.Eq(u => u.ProviderKey, providerKey));

            return await UserLogins.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        protected override async Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var filter = Builders<TUserLogin>.Filter.And(
                Builders<TUserLogin>.Filter.Eq(u => u.UserId, userId),
                Builders<TUserLogin>.Filter.Eq(u => u.LoginProvider, loginProvider),
                Builders<TUserLogin>.Filter.Eq(u => u.ProviderKey, providerKey));

            return await UserLogins.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
