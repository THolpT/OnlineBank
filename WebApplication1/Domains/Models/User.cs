using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string PasswordHash { get; set; }
    public UserStatus Status { get; set; }
    public DateOnly CreatedAt { get; set; }
}