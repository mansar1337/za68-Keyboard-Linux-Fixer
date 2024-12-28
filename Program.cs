using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

class Program
{
    private const string GRUB_CONFIG_PATH = "/etc/default/grub";
    private const string BACKUP_SUFFIX = ".backup";
    private const string VENDOR_ID = "5566";
    private const string PRODUCT_ID = "0008";
    private const string QUIRKS_PARAMETER = "usbcore.quirks=5566:0008:gki";
    private const string SYSTEMD_CONFIG_PATH = "/etc/modules-load.d/za68.conf";
    private const string MODPROBE_CONFIG_PATH = "/etc/modprobe.d/za68.conf";
    private const string MODULE_NAME = "usbhid";
    private const string MODULE_PARAMS = "options usbhid quirks=0x5566:0x0008:0x0004";
    private const string LEGACY_MODULES_PATH = "/etc/modules";

    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║    ZA68 Keyboard Linux Configuration   ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        try
        {
            await ValidateEnvironment();
            var bootSystem = DetectBootSystem();

            if (args.Length > 0 && args[0] == "--remove")
            {
                switch (bootSystem)
                {
                    case BootSystem.Grub:
                        await RemoveFix();
                        break;
                    case BootSystem.Systemd:
                        await RemoveSystemdFix();
                        break;
                    case BootSystem.Legacy:
                        await RemoveLegacyFix();
                        break;
                }
            }
            else
            {
                switch (bootSystem)
                {
                    case BootSystem.Grub:
                        await CreateBackup();
                        await ModifyGrubConfig();
                        await UpdateGrub();
                        break;
                    case BootSystem.Systemd:
                        await ApplySystemdFix();
                        break;
                    case BootSystem.Legacy:
                        await ApplyLegacyFix();
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ Configuration completed successfully using {bootSystem} method!");
                Console.WriteLine("✓ Please restart your computer to apply the changes.");
                Console.WriteLine("\nTo remove the fix later, run: sudo za68fix --remove");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine("✗ Changes were not applied.");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }

    static Task ValidateEnvironment()
    {
        Console.WriteLine("Checking environment...");
        
        if (Environment.UserName != "root")
        {
            throw new UnauthorizedAccessException("This program must be run with root privileges (sudo)");
        }

        if (!File.Exists(GRUB_CONFIG_PATH))
        {
            throw new FileNotFoundException($"GRUB configuration file not found at {GRUB_CONFIG_PATH}");
        }

        Console.WriteLine("✓ Environment check passed");
        return Task.CompletedTask;
    }

    static Task CreateBackup()
    {
        Console.WriteLine("\nCreating GRUB configuration backup...");
        string backupPath = $"{GRUB_CONFIG_PATH}{BACKUP_SUFFIX}";
        
        try
        {
            File.Copy(GRUB_CONFIG_PATH, backupPath, true);
            Console.WriteLine($"✓ Backup created: {backupPath}");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to create backup: {ex.Message}");
        }
    }

    static Task ModifyGrubConfig()
    {
        Console.WriteLine("\nModifying GRUB configuration...");
        
        try
        {
            var lines = File.ReadAllLines(GRUB_CONFIG_PATH);
            bool configUpdated = false;
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("GRUB_CMDLINE_LINUX_DEFAULT="))
                {
                    lines[i] = UpdateGrubCommandLine(lines[i]);
                    configUpdated = true;
                    break;
                }
            }

            if (!configUpdated)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[^1] = $"GRUB_CMDLINE_LINUX_DEFAULT=\"{QUIRKS_PARAMETER}\"";
            }

            File.WriteAllLines(GRUB_CONFIG_PATH, lines);
            Console.WriteLine("✓ GRUB configuration updated");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to modify GRUB configuration: {ex.Message}");
        }
    }

    static string UpdateGrubCommandLine(string line)
    {
        char quoteType = line.Contains('\'') ? '\'' : '"';
        string[] parts = line.Split(quoteType);
        
        if (parts.Length < 2)
        {
            return $"GRUB_CMDLINE_LINUX_DEFAULT=\"{QUIRKS_PARAMETER}\"";
        }

        string parameters = parts[1].Trim();
        if (!parameters.Contains(QUIRKS_PARAMETER))
        {
            parameters = string.IsNullOrEmpty(parameters) 
                ? QUIRKS_PARAMETER 
                : $"{parameters} {QUIRKS_PARAMETER}";
        }

        return $"GRUB_CMDLINE_LINUX_DEFAULT={quoteType}{parameters}{quoteType}";
    }

    static async Task UpdateGrub()
    {
        Console.WriteLine("\nUpdating GRUB configuration...");
        
        using var process = new Process
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

        try
        {
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"GRUB update failed: {error}");
            }

            Console.WriteLine("✓ GRUB successfully updated");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to execute grub-mkconfig: {ex.Message}");
        }
    }

    static async Task RemoveFix()
    {
        Console.WriteLine("Removing ZA68 keyboard fix...");
        
        try
        {
            var lines = File.ReadAllLines(GRUB_CONFIG_PATH);
            bool configUpdated = false;
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("GRUB_CMDLINE_LINUX_DEFAULT="))
                {
                    lines[i] = RemoveQuirksParameter(lines[i]);
                    configUpdated = true;
                    break;
                }
            }

            if (configUpdated)
            {
                await CreateBackup();
                File.WriteAllLines(GRUB_CONFIG_PATH, lines);
                await UpdateGrub();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ ZA68 keyboard fix removed successfully!");
                Console.WriteLine("✓ Please restart your computer to apply the changes.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("ℹ No ZA68 keyboard fix found in GRUB configuration.");
            }
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to remove fix: {ex.Message}");
        }
    }

    static string RemoveQuirksParameter(string line)
    {
        char quoteType = line.Contains('\'') ? '\'' : '"';
        string[] parts = line.Split(quoteType);
        
        if (parts.Length < 2)
        {
            return line;
        }

        string parameters = parts[1].Trim();
        parameters = parameters.Replace(QUIRKS_PARAMETER, "").Trim();
        
        // Clean up multiple spaces
        while (parameters.Contains("  "))
        {
            parameters = parameters.Replace("  ", " ");
        }

        return $"GRUB_CMDLINE_LINUX_DEFAULT={quoteType}{parameters}{quoteType}";
    }

    static bool IsGrubAvailable()
    {
        return File.Exists(GRUB_CONFIG_PATH) && File.Exists("/boot/grub/grub.cfg");
    }

    static async Task ApplySystemdFix()
    {
        Console.WriteLine("Applying systemd-based fix...");
        
        try
        {
            // Create modules-load.d config
            await File.WriteAllTextAsync(SYSTEMD_CONFIG_PATH, $"{MODULE_NAME}\n");
            Console.WriteLine($"✓ Created {SYSTEMD_CONFIG_PATH}");

            // Create modprobe config
            await File.WriteAllTextAsync(MODPROBE_CONFIG_PATH, $"{MODULE_PARAMS}\n");
            Console.WriteLine($"✓ Created {MODPROBE_CONFIG_PATH}");

            // Update initramfs
            await UpdateInitramfs();
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to apply systemd fix: {ex.Message}");
        }
    }

    static async Task RemoveSystemdFix()
    {
        Console.WriteLine("Removing systemd-based fix...");
        
        try
        {
            if (File.Exists(SYSTEMD_CONFIG_PATH))
            {
                File.Delete(SYSTEMD_CONFIG_PATH);
                Console.WriteLine($"✓ Removed {SYSTEMD_CONFIG_PATH}");
            }

            if (File.Exists(MODPROBE_CONFIG_PATH))
            {
                File.Delete(MODPROBE_CONFIG_PATH);
                Console.WriteLine($"✓ Removed {MODPROBE_CONFIG_PATH}");
            }

            await UpdateInitramfs();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Systemd fix removed successfully!");
            Console.WriteLine("✓ Please restart your computer to apply the changes.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to remove systemd fix: {ex.Message}");
        }
    }

    static async Task UpdateInitramfs()
    {
        Console.WriteLine("\nUpdating initramfs...");
        
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "update-initramfs",
                Arguments = "-u",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"initramfs update failed: {error}");
            }

            Console.WriteLine("✓ initramfs successfully updated");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to execute update-initramfs: {ex.Message}");
        }
    }

    enum BootSystem
    {
        Grub,
        Systemd,
        Legacy
    }

    static BootSystem DetectBootSystem()
    {
        Console.WriteLine("Detecting system configuration...");
        
        if (IsGrubAvailable())
        {
            Console.WriteLine("✓ GRUB bootloader detected");
            return BootSystem.Grub;
        }
        
        if (Directory.Exists("/etc/modules-load.d"))
        {
            Console.WriteLine("✓ Systemd configuration detected");
            return BootSystem.Systemd;
        }

        Console.WriteLine("✓ Using legacy modules configuration");
        return BootSystem.Legacy;
    }

    static async Task ApplyLegacyFix()
    {
        Console.WriteLine("Applying legacy fix...");
        
        try
        {
            // Add module to /etc/modules if not present
            var moduleLines = await File.ReadAllLinesAsync(LEGACY_MODULES_PATH);
            if (!moduleLines.Contains(MODULE_NAME))
            {
                await File.AppendAllTextAsync(LEGACY_MODULES_PATH, $"\n{MODULE_NAME}\n");
                Console.WriteLine($"✓ Added {MODULE_NAME} to {LEGACY_MODULES_PATH}");
            }

            // Create modprobe config
            await File.WriteAllTextAsync(MODPROBE_CONFIG_PATH, $"{MODULE_PARAMS}\n");
            Console.WriteLine($"✓ Created {MODPROBE_CONFIG_PATH}");

            // Load module immediately
            await LoadModule();
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to apply legacy fix: {ex.Message}");
        }
    }

    static async Task RemoveLegacyFix()
    {
        Console.WriteLine("Removing legacy fix...");
        
        try
        {
            // Remove module from /etc/modules
            var moduleLines = (await File.ReadAllLinesAsync(LEGACY_MODULES_PATH))
                .Where(line => !line.Trim().Equals(MODULE_NAME))
                .ToArray();
            await File.WriteAllLinesAsync(LEGACY_MODULES_PATH, moduleLines);
            Console.WriteLine($"✓ Removed {MODULE_NAME} from {LEGACY_MODULES_PATH}");

            // Remove modprobe config
            if (File.Exists(MODPROBE_CONFIG_PATH))
            {
                File.Delete(MODPROBE_CONFIG_PATH);
                Console.WriteLine($"✓ Removed {MODPROBE_CONFIG_PATH}");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Legacy fix removed successfully!");
            Console.WriteLine("✓ Please restart your computer to apply the changes.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to remove legacy fix: {ex.Message}");
        }
    }

    static async Task LoadModule()
    {
        Console.WriteLine("\nLoading module...");
        
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "modprobe",
                Arguments = MODULE_NAME,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"Module loading failed: {error}");
            }

            Console.WriteLine("✓ Module loaded successfully");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load module: {ex.Message}");
        }
    }
}
