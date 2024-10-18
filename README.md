# CableBanisher
Sets up a network adapter for use with Meta Airlink or Virtual Desktop.
Only works for Windows PCs.

HOW TO USE:

Download and extract the .zip file from the latest release. 
Extract it to its own directory, preferably.
Run CableBanisher.exe.
Find the network adapter you want to use for Airlink/Virtual Desktop.
Type 's', the number next to the adapter of choice, then hit enter.
Wait a bit. It'll tell you when it's done.

NOTE: If you are experiencing slower than expected speeds, it may be necessary to connect another Wi-Fi 6 capable device to the hotspot first. I'm not entirely sure why this is, but it's a relatively simple if annoying fix.

RESET NETWORK ADAPTER:

Run CableBanisher.exe.
Find the network adapter from before.
Type 'd', the number next to your adapter, then hit enter.
Wait a bit, don't close the program right away. It'll tell you when it's done.
It may or may not successfully disable your hotspot. The Windows API for it times out a lot. Your network adapter will be useable regardless.

# Old Script Version:

I highly recommend the C# app version over the script version. It doesn't require the extra work the PowerShell script does. If you used the PowerShell script to set up a network adapter, you can still use the script to reset it.

If you still decide to use the script version:

1) Unrestrict your ExecutionPolicy

```Set-ExecutionPolicy Unrestricted```

```Set-ExecutionPolicy Unrestricted -Scope CurrentUser```

3) Open an ADMIN PowerShell and CD to the path of the script

```CD /<wherever you put it>```

5) Run the script with a 1 to setup the network card or 0 to restore the network card

```./Configurator.ps1 <0 or 1> "<name of your network adapter ONLY IF it is not called 'Wi-Fi', this parameter must be in quotes.>"```

7) Restore your ExecutionPolicy

```Set-ExecutionPolicy Default```

```Set-ExecutionPolicy Default -Scope CurrentUser```
