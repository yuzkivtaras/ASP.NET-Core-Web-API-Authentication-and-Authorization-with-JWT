using AuthenticationAndAuthorizationJWT.Data;
using AuthenticationAndAuthorizationJWT.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace AuthenticationAndAuthorizationJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUsers() 
        {
            var users = _context.Users.Where(x => x.Status == 1).ToList();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult AddUsers(User user) 
        {
            var _user = new User();
            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Email = user.Email;
            _user.DateOfBirth = Convert.ToDateTime(user.DateOfBirth);
            user.Status = 1;

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok();
        }
        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            return Ok(user);
        }
    }
}
