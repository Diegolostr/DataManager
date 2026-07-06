using Microsoft.AspNetCore.SignalR;

namespace UnityDataImporter.Hubs;

public class DataHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var httpCtx = Context.GetHttpContext();
        var expectedKey = Environment.GetEnvironmentVariable("API_KEY");
        if (!string.IsNullOrWhiteSpace(expectedKey))
        {
            var key = httpCtx?.Request.Query["api_key"].ToString()
                   ?? httpCtx?.Request.Headers["X-Api-Key"].ToString();
            if (key != expectedKey)
            {
                Context.Abort();
                return Task.CompletedTask;
            }
        }
        Console.WriteLine($"[SignalR] Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[SignalR] Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}
