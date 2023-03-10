using AuthenticationAndAuthorizationJWT.Authentication.Configuration;
using AuthenticationAndAuthorizationJWT.Authentication.Models.DTO.Incoming;
using AuthenticationAndAuthorizationJWT.Authentication.Models.DTO.Outgoing;
using AuthenticationAndAuthorizationJWT.DataServices.IConfiguration;
using AuthenticationAndAuthorizationJWT.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationAndAuthorizationJWT.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AccountsController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> otionMonitor)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _jwtConfig = otionMonitor.CurrentValue;
        }

        //Register Action
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto registrationRequestDto)
        {
            //Check the model or obj we are recieving is valid
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.FindByEmailAsync(registrationRequestDto.Email); 
                if (userExist != null)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>() { "Email already in use" }
                    });
                }

                var newUser = new IdentityUser()
                {
                    Email = registrationRequestDto.Email,
                    UserName = registrationRequestDto.Email,
                    EmailConfirmed = true
                };

                var isCreated = await _userManager.CreateAsync(newUser, registrationRequestDto.Password);
                if (!isCreated.Succeeded)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = isCreated.Succeeded,
                        Errors = isCreated.Errors.Select(x => x.Description).ToList()
                    });
                }

                var _user = new User();
                _user.IdentityId = new Guid(newUser.Id);
                _user.FirstName = registrationRequestDto.FirstName;
                _user.LastName = registrationRequestDto.LastName;
                _user.Email = registrationRequestDto.Email;
                _user.Phone = "";
                _user.Country = "";
                _user.Status = 1;

                await _unitOfWork.Users.Add(_user);
                await _unitOfWork.ComplateAsync();

                var token = GenerateJwtToken(newUser);

                return Ok(new UserRegistrationResponseDto()
                {
                    Success = true,
                    Token = token
                });
            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto
                {
                    Success = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginDto)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.FindByEmailAsync(loginDto.Email);

                if (userExist == null)
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>() { "Invalid authentication request" }
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(userExist, loginDto.Password);

                if (isCorrect)
                {
                    var jwtToken = GenerateJwtToken(userExist);

                    return Ok(new UserLoginResponseDto()
                    {
                        Success = true,
                        Token= jwtToken
                    });
                }
                else
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>() { "Invalid authentication request" }
                    });
                }
            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto
                {
                    Success = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email), //uniqe id
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                //Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            //generate the security obj token 
            var token = jwtHandler.CreateToken(tokenDescriptor);

            //converte the security obj into a atring
            var jwtToken = jwtHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
