# Takes adapter name (wn) and action (a) as arguments

param (
	[string]$wn 
	[int]$a 
)

$WirelessAdapterName = $wn
$connectionProfile = [Windows.Networking.Connectivity.NetworkInformation,Windows.Networking.Connectivity,ContentType=WindowsRuntime]::GetInternetConnectionProfile()
$tetheringManager = [Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager,Windows.Networking.NetworkOperators,ContentType=WindowsRuntime]::CreateFromConnectionProfile($connectionProfile)

if (a -eq 1)
{
	Start-Sleep -Seconds 2
	netsh wlan set autoconfig enabled=yes interface=$WirelessAdapterName

	$tetheringManager.StartTetheringAsync()
	Start-Sleep -Seconds 2

	netsh wlan set autoconfig enabled=no interface=$WirelessAdapterName
} 
else if (a -eq 0)
{
	Start-Sleep -Seconds 2
	netsh wlan set autoconfig enabled=yes interface=$WirelessAdapterName
	$tetheringManager.StopTetheringAsync()
}

