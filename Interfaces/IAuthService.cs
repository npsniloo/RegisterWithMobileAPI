

namespace RegisterApp.Interfaces
{
    public interface IAuthService
    {
        public Task<(bool ok, string? message)> RegisterAsync(string mobileNumber, CancellationToken cancellationToken);
        public Task<(bool ok, string? message)> LoginAsync(string mobileNumber, CancellationToken cancellationToken);
        public Task<(bool ok, string? token, string? tokenType, string? error)> VerifyRegisterAsync(string mobileNumber, string code, CancellationToken cancellationToken);
        public Task<(bool ok, string? token, string? tokenType, string? error)> VerifyLoginAsync(string mobileNumber, string code, CancellationToken cancellationToken);
    }
}
