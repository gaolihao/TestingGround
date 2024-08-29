using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using MyApi.Contract;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace MyApi;

/// <summary>
/// Sets up Kestrel on unix socket.
/// </summary>
public static class SocketSetup
{
    /// <summary>
    /// Configures Kestrel.
    /// </summary>
    /// <param name="options">Kestrel options to configure.</param>
    public static void Execute(KestrelServerOptions options)
    {
        /*
        var section = options.ApplicationServices.GetRequiredService<IOptions<SocketConfiguration>>().Value;

        if (!CanUseUnixSockets)
        {
            options.ListenLocalhost(section.HttpPort);
            return;
        }

        var socketPath = GetSocketPath(section.Filename);

        var parentDir = new FileInfo(socketPath).Directory ?? throw new InvalidOperationException("Parent directory could not be found");
        if (!parentDir.Exists)
        {
            parentDir = Directory.CreateDirectory(parentDir.FullName);
        }

        if (OperatingSystem.IsWindows())
        {
            SetPermissions(parentDir);
        }

        if (File.Exists(socketPath))
        {
            File.Delete(socketPath);
        }

        options.ListenUnixSocket(socketPath);
        */
    }

    [SupportedOSPlatform("windows")]
    private static void SetPermissions(DirectoryInfo directory)
    {
        var users = new NTAccount(@"builtin\users");

        var security = directory.GetAccessControl(AccessControlSections.Access);

        var authorizationRules = security.GetAccessRules(true, true, typeof(NTAccount));

        bool hasRequiredPermissions = false;
        foreach (AuthorizationRule rule in authorizationRules)
        {
            if (rule is not FileSystemAccessRule fileRule)
            {
                continue;
            }

            if (fileRule.IdentityReference != users)
            {
                continue;
            }

            if (fileRule.FileSystemRights.HasFlag(FileSystemRights.Modify))
            {
                hasRequiredPermissions = true;
                break;
            }
        }

        if (hasRequiredPermissions)
        {
            return;
        }

        _ = security.ModifyAccessRule(
            AccessControlModification.Add,
            new FileSystemAccessRule(
                users,
                FileSystemRights.Modify,
                InheritanceFlags.ObjectInherit,
                PropagationFlags.InheritOnly | PropagationFlags.NoPropagateInherit,
                AccessControlType.Allow),
            out var _);

        directory.SetAccessControl(security);
    }
}
