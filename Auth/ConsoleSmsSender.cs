using RegisterApp.Interfaces;

namespace RegisterApp.Auth
{
    public class ConsoleSmsSender : ISmsSender
    {
        public Task SendAsync(string toE164, string message, CancellationToken ct = default)
        {
            Console.WriteLine($"[SMS to {toE164}] {message}");
            return Task.CompletedTask;
        }
    }
}