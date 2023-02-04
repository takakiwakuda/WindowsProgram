using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace WindowsProgram;

/// <summary>
/// Provides information about a program installed in Windows on the local computer.
/// </summary>
public sealed class ProgramInfo : IDisposable
{
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
    private readonly Lazy<Uri?> _helpLink;
    private readonly Lazy<string?> _installLocation;
    private readonly Lazy<string?> _uninstallString;
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
    internal ProgramInfo(string name, RegistryKey key)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _key = key;
        _name = name;

        _publisher = new Lazy<string?>(() => (string?)_key.GetValue("Publisher"));
        _uninstallString = new Lazy<string?>(() => (string?)_key.GetValue("UninstallString"));
        _installLocation = new Lazy<string?>(() => (string?)_key.GetValue("InstallLocation"));
        _installDate = new Lazy<DateTime>(GetInstallDate);
        _size = new Lazy<int?>(() => (int?)_key.GetValue("EstimatedSize"));

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
