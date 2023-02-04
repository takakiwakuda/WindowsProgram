using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace WindowsProgram;

/// <summary>
/// Provides information about a program installed in Windows on the local computer.
/// </summary>
public sealed class ProgramInfo : IDisposable
{
    private const string UninstallKeyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

    /// <summary>Gets the name of the program.</summary>
    /// <remarks>This value is equal to DisplayName.</remarks>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public string Name
    {
        get
        {
            ThrowIfDisposed();
            return _name;
        }
    }

    /// <summary>Gets the publisher for the program.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public string? Publisher
    {
        get
        {
            ThrowIfDisposed();
            return _publisher.Value;
        }
    }

    /// <summary>Gets the date when the program was installed.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public DateTime InstallDate
    {
        get
        {
            ThrowIfDisposed();
            return _installDate.Value;
        }
    }

    /// <summary>Gets the size of the program.</summary>
    /// <remarks>This value is equal to EstimatedSize.</remarks>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public int? Size
    {
        get
        {
            ThrowIfDisposed();
            return _size.Value;
        }
    }

    /// <summary>Gets the installed version of the program.</summary>
    /// <remarks>This value is equal to DisplayVersion.</remarks>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public Version? Version
    {
        get
        {
            ThrowIfDisposed();
            return _version.Value;
        }
    }

    /// <summary>Gets comments of the program.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public string? Comments
    {
        get
        {
            ThrowIfDisposed();
            return _comments.Value;
        }
    }

    /// <summary>Gets a string representing the installation location of the program.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public string? InstallLocation
    {
        get
        {
            ThrowIfDisposed();
            return _installLocation.Value;
        }
    }

    /// <summary>Gets a string representing the installation source of the program.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public string? InstallSource
    {
        get
        {
            ThrowIfDisposed();
            return _installSource.Value;
        }
    }

    /// <summary>Gets a string representation of the command to modify the program.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public string? ModifyPath
    {
        get
        {
            ThrowIfDisposed();
            return _modifyPath.Value;
        }
    }

    /// <summary>Gets a string to uninstall the program.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public string? UninstallString
    {
        get
        {
            ThrowIfDisposed();
            return _uninstallString.Value;
        }
    }

    /// <summary>Gets a value that indicates whether the program can be modified.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public bool NoModify
    {
        get
        {
            ThrowIfDisposed();
            return _noModify.Value;
        }
    }

    /// <summary>Gets a value that indicates whether the program can be uninstalled.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public bool NoRemove
    {
        get
        {
            ThrowIfDisposed();
            return _noRemove.Value;
        }
    }

    /// <summary>Gets a value that indicates whether the program can be repaired.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public bool NoRepair
    {
        get
        {
            ThrowIfDisposed();
            return _noRepair.Value;
        }
    }

    /// <summary>Gets a URL for the link to the program's help.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public Uri? HelpLink
    {
        get
        {
            ThrowIfDisposed();
            return _helpLink.Value;
        }
    }

    /// <summary>Gets a URL for the link to the program's website.</summary>
    /// <remarks>This value is equal to URLInfoAbout.</remarks>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public Uri? UrlInfoAbout
    {
        get
        {
            ThrowIfDisposed();
            return _urlInfoAbout.Value;
        }
    }

    /// <summary>Gets a URL for the link to the program's website.</summary>
    /// <remarks>This value is equal to URLUpdateInfo.</remarks>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public Uri? UrlUpdateInfo
    {
        get
        {
            ThrowIfDisposed();
            return _urlUpdateInfo.Value;
        }
    }

    /// <summary>Gets the registry key that contains a program information.</summary>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public RegistryKey RegistryKey
    {
        get
        {
            ThrowIfDisposed();
            return _key;
        }
    }

    private RegistryKey _key;
    private readonly string _name;
    private readonly Lazy<string?> _publisher;
    private readonly Lazy<DateTime> _installDate;
    private readonly Lazy<int?> _size;
    private readonly Lazy<Version?> _version;
    private readonly Lazy<string?> _comments;
    private readonly Lazy<string?> _installLocation;
    private readonly Lazy<string?> _installSource;
    private readonly Lazy<string?> _modifyPath;
    private readonly Lazy<string?> _uninstallString;
    private readonly Lazy<bool> _noModify;
    private readonly Lazy<bool> _noRemove;
    private readonly Lazy<bool> _noRepair;
    private readonly Lazy<Uri?> _helpLink;
    private readonly Lazy<Uri?> _urlInfoAbout;
    private readonly Lazy<Uri?> _urlUpdateInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramInfo"/> class with the specified registry key.
    /// </summary>
    /// <param name="name">The name of the program.</param>
    /// <param name="key">The registry key that contains information about an installed program.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    private ProgramInfo(string name, RegistryKey key)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _key = key;
        _name = name;

        _publisher = new Lazy<string?>(() => (string?)_key.GetValue("Publisher"));
        _comments = new Lazy<string?>(() => (string?)_key.GetValue("Comments"));
        _installLocation = new Lazy<string?>(() => (string?)_key.GetValue("InstallLocation"));
        _installSource = new Lazy<string?>(() => (string?)_key.GetValue("InstallSource"));
        _modifyPath = new Lazy<string?>(() => (string?)_key.GetValue("ModifyPath"));
        _uninstallString = new Lazy<string?>(() => (string?)_key.GetValue("UninstallString"));
        _installDate = new Lazy<DateTime>(GetInstallDate);
        _size = new Lazy<int?>(() => (int?)_key.GetValue("EstimatedSize"));
        _noModify = new Lazy<bool>(() => (int)_key.GetValue("NoModify", 0) == 1);
        _noRemove = new Lazy<bool>(() => (int)_key.GetValue("NoRemove", 0) == 1);
        _noRepair = new Lazy<bool>(() => (int)_key.GetValue("NoRepair", 0) == 1);

        _version = new Lazy<Version?>(() =>
        {
            _ = Version.TryParse((string?)_key.GetValue("DisplayVersion"), out Version? version);
            return version;
        });

        _helpLink = new Lazy<Uri?>(() => GetUri("HelpLink"));
        _urlInfoAbout = new Lazy<Uri?>(() => GetUri("URLInfoAbout"));
        _urlUpdateInfo = new Lazy<Uri?>(() => GetUri("URLUpdateInfo"));
    }

    /// <summary>
    /// Returns programs on the local computer.
    /// </summary>
    /// <returns>An array of the <see cref="ProgramInfo"/>.</returns>
    public static ProgramInfo[] GetProgramInfos()
    {
        List<ProgramInfo> programInfos = new();
        programInfos.AddRange(GetProgramInfos(RegistryHive.CurrentUser, RegistryView.Default));
        programInfos.AddRange(GetProgramInfos(RegistryHive.LocalMachine, RegistryView.Registry64));
        programInfos.AddRange(GetProgramInfos(RegistryHive.LocalMachine, RegistryView.Registry32));

        return programInfos.OrderBy(p => p.Name).ToArray();
    }

    /// <summary>
    /// Returns programs from the specified registry.
    /// </summary>
    /// <param name="hKey">The registry hive to use.</param>
    /// <param name="view">The registry hive to view.</param>
    /// <returns>An array of the <see cref="ProgramInfo"/>.</returns>
    private static ProgramInfo[] GetProgramInfos(RegistryHive hKey, RegistryView view)
    {
        List<ProgramInfo> programInfos = new();

        using RegistryKey baseKey = RegistryKey.OpenBaseKey(hKey, view);
        using RegistryKey uninstallKey = baseKey.OpenSubKey(UninstallKeyName, false)!;

        foreach (string keyName in uninstallKey.GetSubKeyNames())
        {
            RegistryKey? targetKey = uninstallKey.OpenSubKey(keyName, false);
            if (targetKey is not null)
            {
                if ((int)targetKey.GetValue("SystemComponent", 0) == 1)
                {
                    targetKey.Dispose();
                    continue;
                }

                string? name = (string?)targetKey.GetValue("DisplayName");
                if (name is null)
                {
                    targetKey.Dispose();
                    continue;
                }

                programInfos.Add(new ProgramInfo(name, targetKey));
            }
        }

        return programInfos.ToArray();
    }

    /// <summary>
    /// Release all resources used by the current instance of the <see cref="ProgramInfo"/> class.
    /// </summary>
    public void Dispose()
    {
        if (_key is not null)
        {
            _key.Dispose();
            _key = null!;
        }
    }

    /// <summary>
    /// Returns a string representing the name of the current program.
    /// </summary>
    /// <returns>A string representing the name of the current program.</returns>
    /// <exception cref="ObjectDisposedException">
    /// The current object was disposed.
    /// </exception>
    public override string ToString()
    {
        ThrowIfDisposed();
        return _name;
    }

    /// <summary>
    /// Retrieves the date when the program was installed from the registry.
    /// </summary>
    /// <returns>
    /// The date if the InstallDate exists in the registry;
    /// otherwise, the last time the registry was written.
    /// </returns>
    private DateTime GetInstallDate()
    {
        string? installDate = (string?)_key.GetValue("InstallDate");
        if (installDate is not null && installDate.Length == 8)
        {
            try
            {
#if NET7_0_OR_GREATER
                int year = int.Parse(installDate.AsSpan(0, 4));
                int month = int.Parse(installDate.AsSpan(4, 2));
                int day = int.Parse(installDate.AsSpan(6, 2));
#else
                int year = int.Parse(installDate.AsSpan(0, 4).ToString());
                int month = int.Parse(installDate.AsSpan(4, 2).ToString());
                int day = int.Parse(installDate.AsSpan(6, 2).ToString());
#endif

                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
            }
            catch (Exception e)
            {
                // Ignore errors...
                Debug.WriteLine(e);
            }
        }

        return RegistryHelper.GetLastWriteTime(_key);
    }

    /// <summary>
    /// Retrieves the URI with the specified name from the registry.
    /// </summary>
    /// <param name="name">The name to retrieve from the registry.</param>
    /// <returns>
    /// An instance of the <see cref="Uri"/> class if the URL is found by <paramref name="name"/>;
    /// <see langword="null"/> otherwise.
    /// </returns>
    private Uri? GetUri(string name)
    {
        if (Uri.TryCreate((string?)_key.GetValue(name), UriKind.Absolute, out Uri? uri))
        {
            return uri;
        }
        return null;
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if the current instance has already been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// <see cref="_key"/> is <see langword="null"/>.
    /// </exception>
    private void ThrowIfDisposed()
    {
        if (_key is null)
        {
            throw new ObjectDisposedException(nameof(ProgramInfo));
        }
    }
}
