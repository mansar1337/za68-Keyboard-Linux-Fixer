#!/bin/bash

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     ZA68 Keyboard Fixer Installation   ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"

# Download required file
echo -e "\n${YELLOW}[1/3]${NC} Downloading required files..."
FILE_URL="https://drive.usercontent.google.com/download?id=1k2a2ej5NMpmjVsnO4U3ZN3deFh4xWNCZ&export=download&authuser=0&confirm=t&uuid=c5cd170d-e6c2-4ffd-aa39-b5f9e70a69c4"
if wget --progress=bar:force -O Za68Fix "$FILE_URL" 2>&1; then
    echo -e "${GREEN}✓ File successfully downloaded${NC}"
else
    echo -e "${RED}✗ Failed to download file${NC}"
    exit 1
fi

# Install za68fix
echo -e "\n${YELLOW}[2/3]${NC} Installing za68fix utility..."
chmod +x za68fix
if sudo cp za68fix /usr/local/bin/; then
    echo -e "${GREEN}✓ za68fix successfully installed${NC}"
else
    echo -e "${RED}✗ Failed to install za68fix${NC}"
    exit 1
fi


# Install za68fix_GUI
echo -e "\n${YELLOW}[2/3]${NC} Installing Za68Fix_GUI utility..."
chmod +x Za68Fix
if sudo cp Za68Fix /usr/local/bin/; then
    echo -e "${GREEN}✓ Za68Fix_GUI successfully installed${NC}"
else
    echo -e "${RED}✗ Failed to install Za68Fix_GUI${NC}"
    exit 1
fi

# Install GRUB restore script
echo -e "\n${YELLOW}[3/3]${NC} Installing GRUB restore utility..."
chmod +x restore_GRUB.sh
if sudo cp restore_GRUB.sh /usr/local/bin/; then
    echo -e "${GREEN}✓ GRUB restore utility successfully installed${NC}"
else
    echo -e "${RED}✗ Failed to install GRUB restore utility${NC}"
    exit 1
fi

echo -e "\n${BLUE}╔════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║          Installation Complete          ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"

echo -e "\n${YELLOW}Usage:${NC}"
echo -e "  ${GREEN}•${NC} To fix keyboard:  ${BLUE}sudo za68fix${NC}"
echo -e "  ${GREEN}•${NC} To restore GRUB:  ${BLUE}sudo restore_GRUB.sh${NC}"
echo -e "  ${GREEN}•${NC} To fix keyboard with GUI:  ${BLUE}sudo Za68Fix${NC}"
echo -e "\n${YELLOW}Automatic keyboard fix gui will start in 10 seconds...${NC}"
echo -e "${RED}Press Ctrl+C to cancel${NC}"
sleep 10
clear
sudo Za68Fix
