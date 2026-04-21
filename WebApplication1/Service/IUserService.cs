using WebApplication1.Domains.Enums;
using WebApplication1.Domains.Models;
using WebApplication1.DTO;

namespace WebApplication1.Service
{
    public interface IUserService
    {
        Task<User> CreateAsync(CreateUserDto dto);
        Task<User?> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> UpdateAsync(Guid id, UpdateUserDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
