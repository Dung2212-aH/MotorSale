namespace BaseCore.Entities
{
    public class SystemRole
    {
        public byte Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }

        public List<UserRoleAssignment> UserAssignments { get; set; } = new();
    }
}
