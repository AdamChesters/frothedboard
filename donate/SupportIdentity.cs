namespace Frothedboard.App;

internal sealed record SupportIdentity(string AppName, string AppLogo, string AppVersion, string AppId, Func<Task<string>> CheckForUpdates);
