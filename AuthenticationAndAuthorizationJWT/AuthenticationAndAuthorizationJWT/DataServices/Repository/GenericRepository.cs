using AuthenticationAndAuthorizationJWT.DataServices.Data;
using AuthenticationAndAuthorizationJWT.DataServices.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.DataServices.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected ApplicationDbContext _context;
        internal DbSet<T> dbSet;
        protected readonly ILogger _logger;

        public GenericRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            dbSet = context.Set<T>();
            _logger = logger;
        }

        public virtual async Task<bool> Add(T entity)
        {
            await dbSet.AddAsync(entity);
            return true;
        }

        public virtual async Task<IEnumerable<T>> All()
        {
           return await dbSet.ToListAsync();
        }

        public virtual async Task<bool> Delete(Guid id, string userId)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> GetById(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public Task<bool> Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
