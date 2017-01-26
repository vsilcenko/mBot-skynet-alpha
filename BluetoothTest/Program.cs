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


                var isPaired = BluetoothSecurity.PairRequest(mbot.DeviceAddress, "694395");
                Console.WriteLine(isPaired ? "Paired!" : "There was a problem pairing.");

                // check if device is paired
                if (isPaired && mbot.Authenticated)
                {
                    // set pin of device to connect with
                    Client.SetPin("694395");
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
            var buffer = new byte[20];
            
            if (result.IsCompleted)
            {
                using (var streamWriter = new StreamWriter(Client.GetStream()))
                {
                    while (true)
                    {
                        streamWriter.Write(9);
                        //streamWriter.Write(-1);
                        streamWriter.Flush();
                        Thread.Sleep(1000);


                        Client.GetStream().Read(buffer, 0, 20);
                    }
                }

                // client is connected now :)
            }
        }
    }
}
