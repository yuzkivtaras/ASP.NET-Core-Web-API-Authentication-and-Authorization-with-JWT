﻿using System.ComponentModel.DataAnnotations;

namespace AuthenticationAndAuthorizationJWT.Authentication.Models.DTO.Incoming
{
    public class TokenRequestDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
