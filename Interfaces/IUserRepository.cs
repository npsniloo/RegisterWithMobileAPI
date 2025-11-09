using RegisterApp.Entities;

namespace RegisterApp.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExist(string phoneNumber, CancellationToken cancellationToken);
        Task<AppUser?> GetUser(string phoneNumber, CancellationToken cancellationToken);
        Task<bool> AddUser(AppUser user, CancellationToken cancellationToken);
        Task<bool> UpdateUser(AppUser user, CancellationToken cancellationToken);
    }
}
