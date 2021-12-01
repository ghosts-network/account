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
    internal class RoleStore<TRole, TKey, TUserRole, TRoleClaim> :
        RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserRole : IdentityUserRole<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new()
    {
        private readonly IdentityDbContext<TKey> context;

        public RoleStore(IdentityDbContext<TKey> context)
            : base(new IdentityErrorDescriber())
        {
            this.context = context;
        }

        public override IQueryable<TRole> Roles => RolesCollection.AsQueryable();

        private IMongoCollection<TRole> RolesCollection => context.Collection<TRole>("roles");

        private IMongoCollection<TRoleClaim> RoleClaims => context.Collection<TRoleClaim>("roleClaims");

        public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            await RolesCollection.InsertOneAsync(role, null, cancellationToken);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
        {
            await RemoveClaimsAsync(role, cancellationToken);
            await RolesCollection.DeleteOneAsync(Builders<TRole>.Filter.Eq(r => r.Id, role.Id), cancellationToken);

            return IdentityResult.Success;
        }

        public override Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return RolesCollection.Find(Builders<TRole>.Filter.Eq(r => r.Id, ConvertIdFromString(id))).FirstOrDefaultAsync(cancellationToken);
        }

        public override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        {
            return RolesCollection.Find(Builders<TRole>.Filter.Eq(r => r.NormalizedName, normalizedName)).FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            await RolesCollection.ReplaceOneAsync(Builders<TRole>.Filter.Eq(r => r.Id, role.Id), role, cancellationToken: cancellationToken);

            return IdentityResult.Success;
        }

        public override async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            await RoleClaims.InsertOneAsync(
                new TRoleClaim
                {
                    RoleId = role.Id,
                    ClaimValue = claim.Value,
                    ClaimType = claim.Type
                },
                cancellationToken: cancellationToken);
        }

        public override async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
            var rcs = await RoleClaims
                .Find(Builders<TRoleClaim>.Filter.Eq(rc => rc.RoleId, role.Id))
                .ToListAsync(cancellationToken);

            return rcs.Select(rc => rc.ToClaim()).ToList();
        }

        public override async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TRoleClaim>.Filter.And(
                Builders<TRoleClaim>.Filter.Eq(rc => rc.RoleId, role.Id),
                Builders<TRoleClaim>.Filter.Eq(rc => rc.ClaimType, claim.Type),
                Builders<TRoleClaim>.Filter.Eq(rc => rc.ClaimValue, claim.Value));

            await RoleClaims.DeleteOneAsync(filter, cancellationToken);
        }

        public async Task RemoveClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
            var filter = Builders<TRoleClaim>.Filter.Eq(rc => rc.RoleId, role.Id);

            await RoleClaims.DeleteOneAsync(filter, cancellationToken);
        }
    }
}
