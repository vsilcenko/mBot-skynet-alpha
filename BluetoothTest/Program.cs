using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BluetoothTest
{
    class Program
    {
        private static string MBotName { get; set; }
        private static BluetoothClient Client { get; set; }

        static void Main(string[] args)
        {
            MBotName = "Makeblock";

            Console.WriteLine("Hello World!");

            BluetoothClient bc = new BluetoothClient();
            BluetoothDeviceInfo[] devices = bc.DiscoverDevices(8);

            var mbot = devices.FirstOrDefault(device => device.DeviceName.Equals(MBotName));

            if (mbot != null && !mbot.Connected)
            {
                Client = new BluetoothClient();


                var isPaired = BluetoothSecurity.PairRequest(mbot.DeviceAddress, "325870");
                Console.WriteLine(isPaired ? "Paired!" : "There was a problem pairing.");

                // check if device is paired
                if (mbot.Authenticated)
                {
                    // set pin of device to connect with
                    Client.SetPin("325870");
                    // async connection method
                    Client.BeginConnect(mbot.DeviceAddress, BluetoothService.SerialPort, Connect, mbot);
                }

// callback
        
    }

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void Connect(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                while (true)
                {
                    using (var streamWriter = new StreamWriter(Client.GetStream()))
                    {
                        streamWriter.Write(1);
                    }
                    Thread.Sleep(1000);
                }

                // client is connected now :)
            }
        }
    }
}
