using System.IO;
using System.Net;
using System.Net.NetworkInformation;

class Program
{
    static void Main()
    {
        int i;
        string input = "i";
        string[] networkDeviceNames = { };
        NetworkInterface[] networkDevices = NetworkInterface.GetAllNetworkInterfaces();

        for (i = 0; i < networkDevices.Length; i++)
        {
            networkDeviceNames[i] = networkDevices[i].Name;
        }
        
        while (input != null && (input.Length == 0 || char.ToLower(input[0]) != 'q'))
        {
            Console.Clear();

            switch (char.ToLower(input[0]))
            {
                case 's':
                    break;
                default:
                    break;
            }


        }
    }

    static void PrintTable(NetworkInterface[] interfaces)
    {

    }
}