using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.DataServices.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> All();

        Task<T> GetById(Guid id);

        Task<bool> Add(T entity);

        Task<bool> Delete(Guid id, string userId);

        Task<bool> Update(T entity);
    }
}
