using Microsoft.Win32;
using System;
using System.Security.Principal;
using System.Text;

namespace HWIDSpoofer
{
    class Program
    {
        static string originalHwProfileGUID;
        static string originalGUID;
        static string originalPCName;
        static string originalProductID;
        static string originalInstallTime;
        static string originalInstallDate;

        static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("Please run the program as administrator.");
                return;
            }

            Console.WriteLine("Press 'n' and Enter to spoof a new HWID. Press 'k' and Enter to revert to original HWID. Press any other key to exit.");

            while (true)
            {
                string input = Console.ReadLine();
                if (input.ToLower() == "n")
                {
                    SpoofHWID();
                }
                else if (input.ToLower() == "k")
                {
                    RestoreOriginalHWID();
                }
                else
                {
                    break;
                }
            }
        }

        public static void SpoofHWID()
        {
            originalHwProfileGUID = CurrentHwProfileGUID();
            originalGUID = CurrentGUID();
            originalPCName = CurrentPCName();
            originalProductID = CurrentProductID();
            originalInstallTime = CurrentInstallTime();
            originalInstallDate = CurrentInstallDate();

            // Generate spoofed values
            string spoofedHwProfileGUID = GenerateGUID();
            string spoofedGUID = GenerateGUID();
            string spoofedPCName = GeneratePCName();
            string spoofedProductID = GenerateProductID();
            string spoofedInstallTime = GenerateDate(15);
            string spoofedInstallDate = GenerateDate(8);

            // Apply spoofed values to registry
            SetRegistryValue("SYSTEM\\CurrentControlSet\\Control\\IDConfigDB\\Hardware Profiles\\0001", "HwProfileGUID", spoofedHwProfileGUID);
            SetRegistryValue("SOFTWARE\\Microsoft\\Cryptography", "MachineGuid", spoofedGUID);
            SetRegistryValue("SYSTEM\\CurrentControlSet\\Control\\ComputerName\\ActiveComputerName", "ComputerName", spoofedPCName);
            SetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductID", spoofedProductID);
            SetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallTime", spoofedInstallTime);
            SetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallDate", spoofedInstallDate);

            Console.WriteLine("HWID spoofed successfully.");
        }

        public static void RestoreOriginalHWID()
        {
            // Restore original values
            SetRegistryValue("SYSTEM\\CurrentControlSet\\Control\\IDConfigDB\\Hardware Profiles\\0001", "HwProfileGUID", originalHwProfileGUID);
            SetRegistryValue("SOFTWARE\\Microsoft\\Cryptography", "MachineGuid", originalGUID);
            SetRegistryValue("SYSTEM\\CurrentControlSet\\Control\\ComputerName\\ActiveComputerName", "ComputerName", originalPCName);
            SetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductID", originalProductID);
            SetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallTime", originalInstallTime);
            SetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallDate", originalInstallDate);

            Console.WriteLine("Original HWID restored successfully.");
        }

        public static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void SetRegistryValue(string keyPath, string valueName, string value)
        {
            if (OperatingSystem.IsWindows())
            {
                using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey subKey = registryKey.OpenSubKey(keyPath, true))
                    {
                        if (subKey != null)
                        {
                            subKey.SetValue(valueName, value);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: Registry manipulation is only supported on Windows platforms.");
            }
        }

        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GeneratePCName()
        {
            return "DESKTOP-" + GenerateString(8); // Change the length to 8 characters
        }

        public static string GenerateProductID()
        {
            return GenerateString(20); // Change the length to 20 characters
        }

        public static string GenerateDate(int size)
        {
            const string Alphabet = "abcdef0123456789";
            StringBuilder builder = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < size; i++)
            {
                builder.Append(Alphabet[rand.Next(Alphabet.Length)]);
            }
            return builder.ToString();
        }

        public static string GenerateString(int size)
        {
            const string Alphabet = "ABCDEF0123456789";
            StringBuilder builder = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < size; i++)
            {
                builder.Append(Alphabet[rand.Next(Alphabet.Length)]);
            }
            return builder.ToString();
        }

        public static string CurrentHwProfileGUID()
        {
            return GetRegistryValue("SYSTEM\\CurrentControlSet\\Control\\IDConfigDB\\Hardware Profiles\\0001", "HwProfileGUID");
        }

        public static string CurrentGUID()
        {
            return GetRegistryValue("SOFTWARE\\Microsoft\\Cryptography", "MachineGuid");
        }

        public static string CurrentPCName()
        {
            return GetRegistryValue("SYSTEM\\CurrentControlSet\\Control\\ComputerName\\ActiveComputerName", "ComputerName");
        }

        public static string CurrentProductID()
        {
            return GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductID");
        }

        public static string CurrentInstallTime()
        {
            return GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallTime");
        }

        public static string CurrentInstallDate()
        {
            return GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallDate");
        }

        public static string GetRegistryValue(string keyPath, string valueName)
        {
            using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey subKey = registryKey.OpenSubKey(keyPath))
                {
                    if (subKey != null)
                    {
                        var value = subKey.GetValue(valueName);
                        if (value != null)
                        {
                            return value.ToString();
                        }
                    }
                }
            }
            return null;
        }
    }
}
