using AuthenticationAndAuthorizationJWT.DataServices.IConfiguration;
using AuthenticationAndAuthorizationJWT.DataServices.IRepository;
using AuthenticationAndAuthorizationJWT.DataServices.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.DataServices.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public IUsersRepository Users { get; private set; }

        public UnitOfWork(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("db_logs");

            Users = new UsersRepository(context, _logger);
        }

        public async Task ComplateAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose() 
        {
            _context.Dispose();
        }
    }
}
