namespace BaseCore.Entities
{
    public class UserRoleAssignment
    {
        public int UserId { get; set; }
        public byte RoleId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public SystemRole? Role { get; set; }
    }
}
