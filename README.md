# CableBanisher
Sets up a network adapter for use with Meta Airlink or Virtual Desktop.

HOW TO USE:

Download the file "Configurator.ps1"

If you have yet to do so, open a Powershell window as Administrator.

Run the following commands (one at a time):

  Set-ExecutionPolicy Unrestricted
  Set-ExecutionPolicy Unrestricted -Scope CurrentUser

This allows the script to run (Windows disallows this by default).

SETUP NETWORK ADAPTER:
In an Administrator Powershell window, do:

  netsh wlan show settings

This will display your wifi config. Note the name at the end of the line 'Auto configuration logic is enabled on interface "Wi-Fi"'. The word Wi-Fi may be different, and if it is, you'll need to use the actual name later.

  cd {directory of wherever you downloaded the script to}
  ./Configurator.ps1 1 {"Name from previous command if applicable, otherwise ignore this" (put it in quotes)}

Give it a few seconds, and your computer's hotspot should enable.
NOTE: If you are experiencing slower than expected speeds, it may be necessary to connect another Wi-Fi 6 capable device to the hotspot first. I'm not entirely sure why this is, but it's a relatively simple if annoying fix.

RESET NETWORK ADAPTER:
In an Administrator Powershell Window, do:

  
  cd {directory of wherever you downloaded the script to}
  ./Configurator.ps1 0 {"Name from previous command if applicable, otherwise ignore this" (put it in quotes)}
