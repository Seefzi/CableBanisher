# Takes adapter name (wn) and action (a) as arguments

param (
	[int]$a = 1,
	[string]$wn = "Wi-Fi"
)

$connectionProfile = [Windows.Networking.Connectivity.NetworkInformation,Windows.Networking.Connectivity,ContentType=WindowsRuntime]::GetInternetConnectionProfile()
$tetheringManager = [Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager,Windows.Networking.NetworkOperators,ContentType=WindowsRuntime]::CreateFromConnectionProfile($connectionProfile)

Write-Output "Script started."
Write-Output "Wireless Adapter Name: $wn"
Write-Output "Action: $a"

if ($a -eq 1)
{
	Start-Sleep -Seconds 2
	netsh wlan set autoconfig enabled=yes interface=$wn
	$tetheringManager.StartTetheringAsync()
	Start-Sleep -Seconds 2
	netsh wlan set autoconfig enabled=no interface=$wn
} 
elseif ($a -eq 0)
{
	Start-Sleep -Seconds 2
	netsh wlan set autoconfig enabled=yes interface=$wn
	$tetheringManager.StopTetheringAsync()
}

