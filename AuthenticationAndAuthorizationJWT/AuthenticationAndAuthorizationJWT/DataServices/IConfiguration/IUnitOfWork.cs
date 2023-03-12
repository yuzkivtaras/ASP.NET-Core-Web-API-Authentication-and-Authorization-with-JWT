using AuthenticationAndAuthorizationJWT.DataServices.IRepository;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.DataServices.IConfiguration
{
    public interface IUnitOfWork
    {
        IUsersRepository Users { get; }
        IRefreshTokensRepository RefreshTokens { get; }

        Task ComplateAsync();
    }
}
