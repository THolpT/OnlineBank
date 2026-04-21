using WebApplication1.Domains.Enums;

namespace WebApplication1.DTO;

public class UpdateUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string Phone { get; set; }
    public UserStatus Status { get; set; }
}