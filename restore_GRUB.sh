#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Backup file path
GRUB_CONFIG="/etc/default/grub"
BACKUP_FILE="/etc/default/grub.backup"

# Print banner
echo -e "${YELLOW}================================${NC}"
echo -e "${YELLOW}    GRUB Restore Utility${NC}"
echo -e "${YELLOW}================================${NC}"

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo -e "${RED}Error: Please run as root (sudo)${NC}"
    exit 1
fi

# Check if backup exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo -e "${RED}Error: Backup file not found at $BACKUP_FILE${NC}"
    exit 1
fi

# Confirm restoration
echo -e "${YELLOW}Warning: This will restore GRUB to previous configuration.${NC}"
read -p "Do you want to continue? (y/n): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Operation cancelled.${NC}"
    exit 1
fi

# Restore backup
echo -e "\n${YELLOW}Restoring GRUB configuration...${NC}"
if cp "$BACKUP_FILE" "$GRUB_CONFIG"; then
    echo -e "${GREEN}Backup restored successfully.${NC}"
else
    echo -e "${RED}Failed to restore backup.${NC}"
    exit 1
fi

# Update GRUB
echo -e "\n${YELLOW}Updating GRUB...${NC}"
if grub-mkconfig -o /boot/grub/grub.cfg > /dev/null 2>&1; then
    echo -e "${GREEN}GRUB updated successfully.${NC}"
else
    echo -e "${RED}Failed to update GRUB.${NC}"
    exit 1
fi

echo -e "\n${GREEN}Restoration completed successfully!${NC}"
echo -e "${YELLOW}Please restart your computer to apply the changes.${NC}"
