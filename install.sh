#!/bin/bash
echo "za68fix install"
chmod +x za68fix
sudo cp za68fix /usr/local/bin/
echo "za68fix installed!"
echo "you can use fix: sudo za68fix"
echo "wait 10s for auto run za68fix..."
sleep 10
clear
sudo za68fix
