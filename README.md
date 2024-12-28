# ZA68 Keyboard Linux Fix - How It Works

## Overview
This program fixes the ZA68 keyboard functionality on Linux systems by modifying the GRUB bootloader configuration. It adds specific USB quirks parameters that help the system properly recognize and handle the keyboard.

## Technical Details

### 1. Initial Checks
- Verifies if the program is running with root privileges (required for GRUB modification)
- Checks for the existence of the GRUB configuration file at `/etc/default/grub`

### 2. Backup Process
- Creates a backup of the original GRUB configuration
- Saves it as `/etc/default/grub.backup`
- Ensures system safety in case of any issues

### 3. GRUB Configuration Modification
- Adds USB quirks parameter: `usbcore.quirks=5566:0008:gki`
  - Vendor ID: 5566
  - Product ID: 0008
  - Parameter: gki (Generic Keyboard Interface)
- Modifies the `GRUB_CMDLINE_LINUX_DEFAULT` line
- If the line doesn't exist, creates it with the required parameters
- Preserves any existing bootloader parameters

### 4. GRUB Update
- Executes `grub-mkconfig -o /boot/grub/grub.cfg`
- Generates new GRUB configuration
- Applies changes to the system bootloader

### 5. Error Handling
- Implements comprehensive error checking
- Provides clear error messages
- Maintains system stability by rolling back changes if errors occur

## Program Flow
1. Check root privileges
2. Create backup
3. Modify GRUB configuration
4. Update GRUB
5. Prompt for system restart

## Technical Requirements
- Linux operating system
- GRUB bootloader
- Root access

### Restore Backup
 If you have broken GRUB, you can restore GRUB.
 (If you used fix)
 1) Run restore GRUB script: `sudo restore_GRUB.sh`
 2) Reboot pc

### Install
 1) Clone repo: `git clone https://github.com/mansar1337/za68-Keyboard-Linux-Fixer.git`
 2) Go to repo folder: `cd za68-Keyboard-Linux-Fixer`
 3) Set permission to run the script: `chmod +x install.sh`
 4) Run script at root: `sudo ./install.sh`
 5) After run fix reboot pc


