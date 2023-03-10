﻿using System.ComponentModel.DataAnnotations;

namespace AuthenticationAndAuthorizationJWT.Authentication.Models.DTO.Incoming
{
    public class UserLoginRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
