using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;
using System.Diagnostics;


class Program
{
    static async Task Main()
    {
        string input = "i";
        NetworkInterface[] networkDevices = ScanDevices();
        PrintTable(networkDevices);
        
        // Gets user input and follows the switch statement to determine next action
        while (input != null && (input.Length == 0 || char.ToLower(input[0]) != 'q'))
        {
            switch (char.ToLower(input[0]))
            {
                case 'r': // Refresh
                    networkDevices = ScanDevices();
                    break;
                case 's': // Setup Network Device
                    await SetupAdapter(networkDevices, input);
                    break;
                case 'd': // Reset Network Device
                    await RestoreAdapter(networkDevices, input);
                    break;
                case 'q': // Quit
                default:
                    break;
            }
            
            PrintTable(networkDevices);

            // Get input, tries to avoid null
            input = Console.ReadLine()?.Trim() ?? "i";
            if (string.IsNullOrEmpty(input))
                input = "i";
        }
        
    }

    

    // Finds any network adapters with the type Wireless80211 and returns it as an array. I don't know what the Local Area Connection things are but they're not the right ones so they're also excluded.
    static NetworkInterface[] ScanDevices()
    {
        List<NetworkInterface> networkDevices = new List<NetworkInterface>();
        foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces())
            if (iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && !iface.Name.Contains("Local Area Connection"))
                networkDevices.Add(iface);
        
        return networkDevices.ToArray();
    }


    // Sets an adapter up for VR hotspot use
    static async Task SetupAdapter(NetworkInterface[] networkDevices, string input)
    {
        int i = NumberExtractor(networkDevices.Length, input);

        if (i < 0)
        {
            Console.WriteLine("Check syntax and try again.\nAny key to continue...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"Starting network card setup and hotspot...\n");

        RunNetshCommand($"wlan set autoconfig enabled=yes interface=\"{networkDevices[i].Name}\"");

        ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();

        if (connectionProfile == null)
        {
            Console.WriteLine("Error: Unable to get your connection profile. Setup cannot be completed. Default settings remain applied.");
            Console.WriteLine("Any key to continue...");
            Console.ReadKey();
            return;
        }

        NetworkOperatorTetheringManager tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionProfile);

        if (tetheringManager == null)
        {
            Console.WriteLine("Something went wrong. Does your network adapter and OS version support tethering?");
            Console.WriteLine("Any key to continue...");
            Console.ReadKey();
            return;
        }

        await StartHotspot(tetheringManager, networkDevices[i].Name);
    }

    // Restores autoconfig and *attempts to* disable tethering.

    static async Task RestoreAdapter(NetworkInterface[] networkDevices, string input)
    {
        int i = NumberExtractor(networkDevices.Length, input);
        if (i >= 0)
        {
            Console.WriteLine($"Resetting network card settings and disabling hotspot...");

            RunNetshCommand($"wlan set autoconfig enabled=yes interface=\"{networkDevices[i].Name}\"");

            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (connectionProfile != null)
            {
                NetworkOperatorTetheringManager tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionProfile);

                if (tetheringManager != null)
                {
                    try
                    {
                        await tetheringManager.StopTetheringAsync();
                        Console.WriteLine("Hotspot stopped and network settings restored.\nAny key to continue...");
                    } 
                    catch
                    {
                        Console.WriteLine("Network settings restored. Hotspot timed out on shutdown, though it may still have been disabled. Your network card will still work regardless.\nAny key to continue...");
                    }
                    
                    Console.ReadKey();
                    
                }
            }
            else
            {
                Console.WriteLine("Unable to get your connection profile. Autoconfig left enabled.");
            }
        }
        else
        {
            Console.WriteLine("Check syntax and try again.\nAny key to continue...");
            Console.ReadKey();
        }
    }


    // Searches user input for the number they were supposed to provide and determines if it exists and is under the maximum
    static int NumberExtractor(int max, string input)
    {
        string pattern = @"\d+";

        Match match = Regex.Match(input, pattern);

        if (match.Success && int.Parse(match.Value) - 1 < max)
        {
            return int.Parse(match.Value) - 1;
        }
        else
            return -1;
    }


    // Prints the interface
    static void PrintTable(NetworkInterface[] interfaces)
    {
        Console.Clear();
        Console.WriteLine("Cable Banisher version 1.3");
        Console.Write("\n\n");
        Console.WriteLine("* This program fixes major stuttering issues when using your PC's wireless card as a hotspot to enable wireless VR.");
        Console.WriteLine("* For the best experience, use ethernet to connect your PC to the internet.");
        Console.WriteLine("* While set up for VR, the wireless card won't be available for internet connections.");
        Console.WriteLine("* Find your desired network card below. It is usually called something like \"Wi-Fi\".");
        Console.WriteLine("* Use the 's' command to setup the network card for VR.");
        Console.WriteLine("* Once setup is complete, connect your VR headset to your PC's wireless hotspot (it should enable itself automatically).");
        Console.WriteLine("* For the Quest 2 and other Meta headsets, you may need to connect then disconnect another Wi-Fi 6 enabled device, such as a modern smartphone, in order to achieve best performance. For some reason, it doesn't start with Wi-Fi 6 when only those headsets are connected.");
        Console.WriteLine("* If you don't know your PC hotspot's SSID and password, you can find it in your PC settings (just search 'hotspot').");
        Console.WriteLine("* If you need to use the network card for internet access, you can change it back with the 'd' command.");
        Console.Write("\n\n");
        int i = 1;
        foreach (NetworkInterface iface in interfaces)
        {
            Console.WriteLine($"{i}: {iface.Name}");
            i++;
        }

        // Warns user if no wifi card was found
        if (interfaces == null)
        {
            Console.WriteLine("Warning: No compatible network adapter detected.");
        }

        Console.WriteLine("\n\nr: refresh list of network adapters\ns (plus line number): setup network adapter as VR hotspot\nd (plus line number): restore network adapter to default settings\nq: quit\n\n");
        Console.Write("$: ");
    }

    // Simple method for running the necessary networking commands
    static void RunNetshCommand(string arguments)
    {
        try
        {
            Process netshProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            netshProcess.Start();
            string output = netshProcess.StandardOutput.ReadToEnd();
            netshProcess.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running netsh command: {ex.Message}");
        }
    }

    // On some machines, starting the hotspot programmatically isn't consistent. Here's some error handling for that case
    static async Task StartHotspot(NetworkOperatorTetheringManager tetheringManager, string netName)
    {
        while (!GetHotspotEnabled())
        {
            try
            {
                var result = await tetheringManager.StartTetheringAsync();
                if (result.Status == TetheringOperationStatus.Success)
                {
                    break;
                }
                else
                    throw new Exception("StartTetheringAsync operation failed.");

            }
            catch (Exception ex)
            {
                Console.WriteLine("It seems the Windows API may have timed out. Checking if the hotspot is ready...");
                if (GetHotspotEnabled())
                {
                    Console.WriteLine("The hotspot is ready. Proceeding...");
                    break;
                }
                else
                {
                    Console.WriteLine("The hotspot didn't successfully enable. If this continues to fail, please enable your hotspot in Settings. Any key to try again...");
                    Console.ReadKey();
                }
            }
        }
        RunNetshCommand($"wlan set autoconfig enabled=no interface=\"{netName}\"");

        Console.WriteLine("Setup complete.\nAny key to continue...");
        Console.ReadKey();
    }

    static bool GetHotspotEnabled()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = "wlan show hostednetwork",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            using (var reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                process.WaitForExit();

                if (result.Contains("Status") && result.Contains("Started"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}