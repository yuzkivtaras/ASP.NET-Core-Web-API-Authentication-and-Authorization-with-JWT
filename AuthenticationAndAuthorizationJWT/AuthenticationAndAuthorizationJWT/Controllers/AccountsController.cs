using AuthenticationAndAuthorizationJWT.Authentication.Configuration;
using AuthenticationAndAuthorizationJWT.Authentication.Models.DTO.Incoming;
using AuthenticationAndAuthorizationJWT.Authentication.Models.DTO.Outgoing;
using AuthenticationAndAuthorizationJWT.Authentication.Models.Generic;
using AuthenticationAndAuthorizationJWT.DataServices.IConfiguration;
using AuthenticationAndAuthorizationJWT.Models;
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
using AuthResult = AuthenticationAndAuthorizationJWT.Authentication.Models.DTO.Outgoing.AuthResult;

namespace AuthenticationAndAuthorizationJWT.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig;

        public AccountsController(
            IUnitOfWork unitOfWork, 
            UserManager<IdentityUser> userManager, 
            TokenValidationParameters tokenValidationParameters, 
            IOptionsMonitor<JwtConfig> otionMonitor)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtConfig = otionMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto registrationRequestDto)
        {
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

                var token = await GenerateJwtToken(newUser);

                return Ok(new UserRegistrationResponseDto()
                {
                    Success = true,
                    Token = token.JwtToken,
                    RefreshToken = token.RefreshToken
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
                    var jwtToken = await GenerateJwtToken(userExist);

                    return Ok(new UserLoginResponseDto()
                    {
                        Success = true,
                        Token= jwtToken.JwtToken,
                        RefreshToken = jwtToken.RefreshToken
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

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyToken(tokenRequestDto);

                if (result == null)
                {
                    return BadRequest(new UserRegistrationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>() { "Token validation failed" }
                    });
                }

                return Ok(result);
            }
            return BadRequest(new UserRegistrationResponseDto
            {
                Success = false,
                Errors = new List<string>() { "Invalid payload" }
            });
        }

        private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var pricipal = tokenHandler.ValidateToken(tokenRequestDto.Token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (!result)
                    {
                        return null;
                    }
                }
                var utcExpiryDate = long.Parse(pricipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Jwt token has not expired" }
                    };
                }

                var refreshTokenExist = await _unitOfWork.RefreshTokens.GetByRefreshToken(tokenRequestDto.RefreshToken);

                if (refreshTokenExist == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Invalid refresh token" }
                    };
                }

                if (refreshTokenExist.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh token has expired, please login again" }
                    };
                }

                if (refreshTokenExist.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh token has been used, it cannot be reused" }
                    };
                }

                if (refreshTokenExist.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh token has been revoked, it cannot be eused" }
                    };
                }

                var jti = pricipal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (refreshTokenExist.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh token reference does not match  the jwt token" }
                    };
                }

                refreshTokenExist.IsUsed = true;

                var updateRusult = await _unitOfWork.RefreshTokens.MarkRefreshTokenAsUsed(refreshTokenExist);

                if (updateRusult)
                {
                    await _unitOfWork.ComplateAsync();

                    var dbUser = await _userManager.FindByIdAsync(refreshTokenExist.UserId);

                    if (dbUser == null)
                    {
                        return new AuthResult()
                        {
                            Success = false,
                            Errors = new List<string>() { "Error processing request" }
                        };
                    }

                    var tokens = await GenerateJwtToken(dbUser);

                    return new AuthResult
                    {
                        Token = tokens.JwtToken, 
                        Success = true, 
                        RefreshToken = tokens.RefreshToken
                    };
                }

                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>() { "Error processing request" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixDate)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixDate).ToUniversalTime();
            return dateTime;
        }

        private async Task<TokenData> GenerateJwtToken(IdentityUser user)
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
                //Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            //generate the security obj token 
            var token = jwtHandler.CreateToken(tokenDescriptor);

            //converte the security obj into a atring
            var jwtToken = jwtHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                //AddedDate = DateTime.UtcNow,
                Token = $"{RandomStringGenerator(25)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsRevoked = false,
                IsUsed = false,
                Status = 1,
                JwtId = token.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
            };

            await _unitOfWork.RefreshTokens.Add(refreshToken);
            await _unitOfWork.ComplateAsync();

            var tokenData = new TokenData
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            };

            return tokenData;
        }

        private string RandomStringGenerator(int lenght)
        {
            var random = new Random();
            const string chars = "QWERTYUIOPASDFGHJKLZXCVBNM";

            return new string(Enumerable.Repeat(chars, lenght).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
