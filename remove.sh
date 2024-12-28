#!/bin/bash

echo "Starting cleanup process..."

# Remove fix-related files
echo "Removing fix files..."
if [ -f /usr/local/bin/za68fix ]; then
    sudo rm /usr/local/bin/za68fix
    echo "- za68fix removed"
else
    echo "- za68fix not found"
fi

if [ -f /usr/local/bin/restore_GRUB.sh ]; then
    sudo rm /usr/local/bin/restore_GRUB.sh
    echo "- restore_GRUB.sh removed"
else
    echo "- restore_GRUB.sh not found"
fi

echo "Cleanup completed successfully"
