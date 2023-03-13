using AuthenticationAndAuthorizationJWT.DataServices.Data;
using AuthenticationAndAuthorizationJWT.DataServices.IConfiguration;
using AuthenticationAndAuthorizationJWT.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]   
    public class UsersController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _unitOfWork.Users.All();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> AddUsers(User user)
        {
            var _user = new User();
            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Email = user.Email;
            _user.Phone = user.Phone;
            _user.DateOfBirth = user.DateOfBirth;
            _user.Country = user.Country;
            _user.Status = 1;

            await _unitOfWork.Users.Add(_user);
            await _unitOfWork.ComplateAsync();

            return CreatedAtRoute("GetUser", new { id = _user.Id }, user);
        }
        [HttpGet]
        [Route("GetUser", Name = "GetUser")]
        public IActionResult GetUser(Guid id)
        {
            var user = _unitOfWork.Users.GetById(id);

            return Ok(user);
        }
    }
}
