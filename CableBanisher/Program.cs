﻿using System.IO;
using System;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Management;

class Program
{
    static void Main()
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
                    SetupAdapter(networkDevices, input);
                    PrintTable(networkDevices);
                    break;
                case 'd':
                    RestoreAdapter(networkDevices, input);
                    PrintTable(networkDevices);
                    break;
                case 'q':
                default:
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
    static void SetupAdapter(NetworkInterface[] networkDevices, string input)
    {
        int i = NumberExtractor(networkDevices.Length, input);
        string query = $"SELECT * FROM Win32_NetworkAdapter WHERE Name = '{networkDevices[i].Name}'";
        ManagementObjectSearcher search = new ManagementObjectSearcher(query);
        ManagementObjectCollection collection = search.Get();
        Console.WriteLine(collection);

        foreach (ManagementObject item in collection)
        {
            try
            { 
                object result = item.InvokeMethod("Disable", null);
                if ((uint)result == 0)
                {
                    Console.WriteLine("Success!");
                }
                else
                {
                    Console.WriteLine($"Failed to disable network adapter. Error code: {(uint)result}");
                }
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                Console.ReadKey();
            }
        }
        Console.ReadKey();

    }


    // Sets an adapter up for regular use
    static void RestoreAdapter(NetworkInterface[] networkDevices, string input)
    {
        int i = NumberExtractor(networkDevices.Length, input);

        if (i >= 0)
        {

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

        Console.WriteLine("\n\nr: refresh list of network adapters\ns (plus line number): setup network adapter as VR hotspot\nd: restore network adapter to default settings\nq: quit\n\n");
        Console.Write("$: ");
    }
}