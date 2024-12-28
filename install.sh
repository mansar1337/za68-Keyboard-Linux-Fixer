#!/bin/bash
echo "za68fix install"
chmod +x za68fix
sudo cp za68fix /usr/local/bin/
echo "za68fix installed!"
echo "grub restore install"
chmod +x restore_GRUB.sh
sudo cp restore_GRUB.sh /usr/local/bin/
echo "grub restore installed!"
echo "you can use fix: sudo za68fix"
echo "you can restore grub after fix: sudo restore_GRUB.sh"
echo "wait 10s for auto run za68fix..."
sleep 10
clear
sudo za68fix
