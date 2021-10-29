using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WindowsProgram
{
    /// <summary>
    /// Get-WindowsProgram cmdlet
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "WindowsProgram", HelpUri = "https://github.com/takakiwakuda/WindowsProgram/blob/main/doc/Get-WindowsProgram.md")]
    [OutputType(typeof(WindowsProgramInfo))]
    public class GetWindowsProgramCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            List<WindowsProgramInfo> infos = new();
            infos.AddRange(GetPrograms(RegistryHive.CurrentUser, RegistryView.Registry64));
            infos.AddRange(GetPrograms(RegistryHive.CurrentUser, RegistryView.Registry32));
            infos.AddRange(GetPrograms(RegistryHive.LocalMachine, RegistryView.Registry64));
            infos.AddRange(GetPrograms(RegistryHive.LocalMachine, RegistryView.Registry32));

            WriteObject(infos.OrderBy(i => i.Name), true);
        }

        private IEnumerable<WindowsProgramInfo> GetPrograms(RegistryHive hKey, RegistryView view)
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hKey, view);
            using RegistryKey uninstallKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", false);

            foreach (string subKeyName in uninstallKey.GetSubKeyNames())
            {
                using RegistryKey programKey = uninstallKey.OpenSubKey(subKeyName, false);

                if ((int)programKey.GetValue("SystemComponent", 0) == 1)
                {
                    continue;
                }

                if (programKey.GetValue("ParentKeyName") != null)
                {
                    continue;
                }

                string name = (string)programKey.GetValue("DisplayName", string.Empty);
                if (name.Length == 0)
                {
                    continue;
                }

                if (!Version.TryParse((string)programKey.GetValue("DisplayVersion"), out Version version))
                {
                    version = null;
                }

                yield return new WindowsProgramInfo()
                {
                    InstalledDate = GetInstalledDate(programKey),
                    Name = name,
                    Publisher = (string)programKey.GetValue("Publisher"),
                    Size = (int?)programKey.GetValue("EstimatedSize"),
                    Version = version
                };
            }
        }

        private DateTime GetInstalledDate(RegistryKey key)
        {
            string datetime = (string)key.GetValue("InstallDate");
            if (datetime == null || !Regex.IsMatch(datetime, "^[0-9]{8}$"))
            {
                return NativeMethod.GetLastWriteTime(key);
            }

            int year = int.Parse(datetime.Substring(0, 4));
            int month = int.Parse(datetime.Substring(4, 2));
            int day = int.Parse(datetime.Substring(6, 2));

            try
            {
                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
            }
            catch (ArgumentOutOfRangeException)
            {
                WriteDebug($"Datetime {datetime} is in an invalid.");
            }

            return NativeMethod.GetLastWriteTime(key);
        }
    }

    public class WindowsProgramInfo
    {
        public DateTime InstalledDate { get; internal set; }

        public string Name { get; internal set; }

        public string Publisher { get; internal set; }

        public int? Size { get; internal set; }

        public Version Version { get; internal set; }
    }
}
