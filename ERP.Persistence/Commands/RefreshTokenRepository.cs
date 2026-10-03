using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        // Un token vencido se guarda un día más: si alguien presenta uno copiado, todavía se reconoce como robo.
        private static readonly TimeSpan KeepExpired = TimeSpan.FromDays(1);

        private readonly ErpDbContext _context;

        public RefreshTokenRepository(ErpDbContext context) => _context = context;

        public void Add(RefreshToken refreshToken) =>
            _context.RefreshTokens.Add(refreshToken);

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
            await _context.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);

        public async Task RevokeFamilyAsync(Guid familyId, DateTime now)
        {
            foreach (var token in await _context.RefreshTokens.Where(t => t.FamilyId == familyId && t.RevokedAt == null).ToListAsync())
                token.Revoke(now);
        }

        public async Task RevokeAllForUserAsync(Guid userId, DateTime now)
        {
            foreach (var token in await _context.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync())
                token.Revoke(now);
        }

        public async Task RemoveExpiredForUserAsync(Guid userId, DateTime now)
        {
            var limit = now - KeepExpired;
            _context.RefreshTokens.RemoveRange(
                await _context.RefreshTokens.Where(t => t.UserId == userId && t.ExpiresAt < limit).ToListAsync());
        }
    }
}
