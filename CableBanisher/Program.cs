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
            /* 
             * r = refresh
             * s = setup for VR hotspot
             * d = restore network adapter to default settings
             * q = quit
             */
            switch (char.ToLower(input[0]))
            {
                case 'r':
                    networkDevices = ScanDevices();
                    PrintTable(networkDevices);
                    break;
                case 's':
                    await SetupAdapter(networkDevices, input);
                    PrintTable(networkDevices);
                    break;
                case 'd':
                    await RestoreAdapter(networkDevices, input);
                    PrintTable(networkDevices);
                    break;
                case 'q':
                default:
                    PrintTable(networkDevices);
                    break;
            }
            
            // Get input, tries to avoid null
            input = Console.ReadLine()?.Trim() ?? "i";
            if (string.IsNullOrEmpty(input))
                input = "i";
        }
        
    }

    

    // Finds any network adapters with the type Wireless80211 and returns it as an array
    static NetworkInterface[] ScanDevices()
    {
        List<NetworkInterface> networkDevices = new List<NetworkInterface>();
        foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                networkDevices.Add(iface);
            }
        }
        
        return networkDevices.ToArray();
    }


    // Sets an adapter up for VR hotspot use
    static async Task SetupAdapter(NetworkInterface[] networkDevices, string input)
    {
        int i = NumberExtractor(networkDevices.Length, input);
        if (i >= 0)
        {
            Console.WriteLine($"Disabling Wi-Fi scanning on designated network interface...");

            RunNetshCommand($"wlan set autoconfig enabled=yes interface=\"{networkDevices[i].Name}\"");

            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (connectionProfile != null)
            {
                NetworkOperatorTetheringManager tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionProfile);

                if (tetheringManager != null)
                {
                    var result = await tetheringManager.StartTetheringAsync();

                    if (result.Status == TetheringOperationStatus.Success)
                    {
                        RunNetshCommand($"wlan set autoconfig enabled=no interface=\"{networkDevices[i].Name}\"");

                        Console.WriteLine("Setup complete. Keep in mind, you may need to connect your smartphone to the hotspot network for full Wifi 6 speeds to be available. Press any key to continue.");
                        Console.ReadKey();
                    }
                }
            }
            else
            {
                Console.WriteLine("Unable to get your connection profile. Autoconfig left enabled.");
            }
        }
        else
        {
            Console.WriteLine("Syntax: no line number selected.\nAny key to continue...");
            Console.ReadKey();
        }
    }

    // Restores autoconfig and *attempts to* disable tethering.

    static async Task RestoreAdapter(NetworkInterface[] networkDevices, string input)
    {
        int i = NumberExtractor(networkDevices.Length, input);
        if (i >= 0)
        {
            Console.WriteLine($"Re-enabling Wi-Fi scanning on designated network interface...");

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
                        Console.WriteLine("Tethering timed out on shutdown, though it may still have been disabled. Your network card will still work regardless. Press any key to continue.");
                    } 
                    catch
                    {
                        Console.WriteLine("Tethering stopped and network settings restored. Press any key to continue.");
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
            Console.WriteLine("Syntax: no line number selected.\nAny key to continue...");
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

    // Simple class for running the necessary networking commands
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

            Console.WriteLine($"Netsh Output: {output}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running netsh command: {ex.Message}");
        }
    }

    static bool GetHotspotEnabled()
    {
        // Create a process to run the command
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = "wlan show hostednetwork",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process and read the output
        using (Process process = Process.Start(startInfo))
        {
            using (var reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                process.WaitForExit();

                // Check for "Status" in the output
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