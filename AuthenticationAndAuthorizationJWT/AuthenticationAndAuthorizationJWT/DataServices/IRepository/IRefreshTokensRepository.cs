using AuthenticationAndAuthorizationJWT.Models;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.DataServices.IRepository
{
    public interface IRefreshTokensRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetByRefreshToken(string refreshToken);
        Task<bool> MarkRefreshTokenAsUsed(RefreshToken refreshToken);
    }
}
