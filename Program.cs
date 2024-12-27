using System;
using System.IO;
using System.Diagnostics;

class Program
{
    private const string GRUB_CONFIG_PATH = "/etc/default/grub";
    private const string BACKUP_SUFFIX = ".backup";
    private const string VENDOR_ID = "5566";
    private const string PRODUCT_ID = "0008";

    static void Main(string[] args)
    {
        Console.WriteLine("ZA68 Keyboard Linux Fix (GRUB Configuration)");

        try
        {
            // Check root privileges
            if (Environment.UserName != "root")
            {
                throw new Exception("This program must be run with root privileges (sudo)");
            }

            // Create backup
            CreateBackup();

            // Modify GRUB configuration
            ModifyGrubConfig();

            // Update GRUB
            UpdateGrub();

            Console.WriteLine("\nConfiguration completed successfully!");
            Console.WriteLine("Please restart your computer to apply the changes.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine("Changes were not applied.");
            Environment.Exit(1);
        }
    }

    static void CreateBackup()
    {
        Console.WriteLine("Creating GRUB configuration backup...");
        string backupPath = GRUB_CONFIG_PATH + BACKUP_SUFFIX;
        File.Copy(GRUB_CONFIG_PATH, backupPath, true);
        Console.WriteLine($"Backup created: {backupPath}");
    }

    static void ModifyGrubConfig()
    {
        Console.WriteLine("Modifying GRUB configuration...");
        
        string[] lines = File.ReadAllLines(GRUB_CONFIG_PATH);
        bool found = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("GRUB_CMDLINE_LINUX_DEFAULT="))
            {
                string quirksParameter = $"usbcore.quirks={VENDOR_ID}:{PRODUCT_ID}:gki";
                string currentLine = lines[i].Trim('"');
                
                if (!currentLine.Contains(quirksParameter))
                {
                    // Remove quotes and add parameter
                    string[] parts = lines[i].Split('"');
                    if (parts.Length >= 2)
                    {
                        string parameters = parts[1];
                        parameters = parameters.Trim();
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            parameters += " ";
                        }
                        parameters += quirksParameter;
                        lines[i] = $"GRUB_CMDLINE_LINUX_DEFAULT=\"{parameters}\"";
                    }
                }
                found = true;
                break;
            }
        }

        if (!found)
        {
            // If line not found, add new one
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = $"GRUB_CMDLINE_LINUX_DEFAULT=\"usbcore.quirks={VENDOR_ID}:{PRODUCT_ID}:gki\"";
        }

        File.WriteAllLines(GRUB_CONFIG_PATH, lines);
        Console.WriteLine("GRUB configuration updated");
    }

    static void UpdateGrub()
    {
        Console.WriteLine("Updating GRUB configuration...");
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "grub-mkconfig",
                Arguments = "-o /boot/grub/grub.cfg",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Error updating GRUB: {error}");
        }

        Console.WriteLine("GRUB successfully updated");
    }
}
