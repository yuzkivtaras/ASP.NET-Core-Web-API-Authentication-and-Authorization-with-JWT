﻿using AuthenticationAndAuthorizationJWT.DataServices.Data;
using AuthenticationAndAuthorizationJWT.DataServices.IRepository;
using AuthenticationAndAuthorizationJWT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.DataServices.Repository
{
    public class UsersRepository : GenericRepository<User>, IUsersRepository
    {
        public UsersRepository(ApplicationDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public override async Task<IEnumerable<User>> All()
        {
            try
            {
                return await dbSet.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "{Repo} All method has generated an error", typeof(UsersRepository));
                return new List<User>();
            }
        }
    }
}
