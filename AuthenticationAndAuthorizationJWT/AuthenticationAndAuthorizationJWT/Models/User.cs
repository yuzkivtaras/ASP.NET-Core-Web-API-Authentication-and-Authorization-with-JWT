﻿using System;

namespace AuthenticationAndAuthorizationJWT.Models
{
    public class User : BaseEntity
    {
        public Guid IdentityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Country { get; set; }
    }
}
