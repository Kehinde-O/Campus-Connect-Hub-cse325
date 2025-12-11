using Microsoft.AspNetCore.Components;

namespace CampusConnectHub.Client.Services;

public class NotificationService
{
    public event Action<NotificationMessage>? OnNotification;

    public void ShowSuccess(string message, int duration = 3000)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Success,
            Duration = duration
        });
    }

    public void ShowError(string message, int duration = 5000)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Error,
            Duration = duration
        });
    }

    public void ShowInfo(string message, int duration = 3000)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Info,
            Duration = duration
        });
    }

    public void ShowWarning(string message, int duration = 4000)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Warning,
            Duration = duration
        });
    }
}

public class NotificationMessage
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int Duration { get; set; } = 3000;
    public Guid Id { get; set; } = Guid.NewGuid();
}

public enum NotificationType
{
    Success,
    Error,
    Info,
    Warning
}

