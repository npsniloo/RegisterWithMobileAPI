namespace RegisterApp.Interfaces
{
    public interface ISmsSender
    {
        Task SendAsync(string toE164, string message, CancellationToken ct = default);
    }

}
