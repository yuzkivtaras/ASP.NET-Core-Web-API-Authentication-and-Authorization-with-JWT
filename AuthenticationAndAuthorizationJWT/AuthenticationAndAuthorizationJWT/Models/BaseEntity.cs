using System;

namespace AuthenticationAndAuthorizationJWT.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Status { get; set; } = 1;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateTime { get; set; }
    }
}
