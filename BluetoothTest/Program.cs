using System;
using System.IO;
using System.Linq;
using System.Threading;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BluetoothTest
{
    class Program
    {
        private static string MBotName { get; set; }
        private static BluetoothClient Client { get; set; }
        private static readonly AutoResetEvent StopWaitHandle = new AutoResetEvent(false);
        private static byte[] Command { get; set; }


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
                
                var isPaired = BluetoothSecurity.PairRequest(mbot.DeviceAddress, "682011");
                Console.WriteLine(isPaired ? "Paired!" : "There was a problem pairing.");

                // check if device is paired
                if (isPaired && mbot.Authenticated)
                {
                    // set pin of device to connect with
                    Client.SetPin("682011");
                    // async connection method
                    Client.BeginConnect(mbot.DeviceAddress, BluetoothService.SerialPort, Connect, mbot);

                    StopWaitHandle.WaitOne();
                }
            }

            while (true)
            {
            }
        }

        private static void Connect(IAsyncResult result)
        {
            StopWaitHandle.Set();

            if (!result.IsCompleted)
            {
                return;
            }

            Command = new byte[] {0, 0};

            var buffer = new byte[20];

            // get commands from keyboard and send them to the robot
            using (var streamWriter = Client.GetStream())
            {
                while (true)
                {
                    var isValidCommand = GetCommand();
                    if (!isValidCommand)
                    {
                        continue;
                    }
                    streamWriter.Write(Command, 0, 2);

                    //Client.GetStream().Read(buffer, 0, 20);
                    //Console.WriteLine($"{buffer[0]} {buffer[1]}");
                }
            }
        }

        private static bool GetCommand()
        {
            var keyInfo = Console.ReadKey();
            switch (keyInfo.Key)
            {
                case ConsoleKey.W:
                {
                    SetCommand(Constants.ForwardDirection);
                    return true;
                }
                case ConsoleKey.S:
                    {
                    SetCommand(Constants.BackwardsDirection);
                    return true;
                    }
                case ConsoleKey.D:
                    {
                    SetCommand(Constants.RightDirection);
                    return true;
                    }
                case ConsoleKey.A:
                    {
                    SetCommand(Constants.LeftDirection);
                    return true;
                    }
                case ConsoleKey.Spacebar:
                {
                    Command[1] = 0;
                    return true;
                    }
            }
            return false;

        }

        private static void SetCommand(byte direction)
        {
            if (Command[0] == direction || Command[1] == 0)
            {
                IncreaseSpeed();
            }
            else if (direction <= 2 && Command[0] <= 2)
            {
                if (Command[1]  <= Constants.SpeadIncrease)
                {
                    // reset speed and increase to move backwards
                    Command[1] = 0;
                }
                else
                {
                    Command[1] -= Constants.SpeadIncrease;
                    return;
                }
            }
            Command[0] = direction;
        }

        private static void IncreaseSpeed()
        {
            if (Command[1] + Constants.SpeadIncrease > Constants.MaxSpeed)
            {
                Command[1] = Constants.MaxSpeed;
            }
            else
            {
                Command[1] += Constants.SpeadIncrease;
            }
        }
    }
}
