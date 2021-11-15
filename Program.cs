using System;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;

namespace PingSweep
{
    public class MainClass
    {
        public string portRange, portScanType;
        private bool ping(string ipAddress)
        {
            Ping PingObj = new Ping();

            PingOptions PingOptionObj = new PingOptions();
            PingOptionObj.DontFragment = true;

            byte[] msgBuffer = Encoding.UTF8.GetBytes("Hello");

            PingReply Reply = PingObj.Send(
                                            ipAddress, // ipaddress
                                            1, // timeout
                                            msgBuffer, // message
                                            PingOptionObj); // ping options

            if ((Reply.Status == IPStatus.Success) && (IPAddress.Parse(ipAddress).AddressFamily == AddressFamily.InterNetwork))
            {
                return true;
            }

            return false;
        }

        private string GetMachineNameFromIPAddress(string ipAddress)
        {
            string machineName = string.Empty;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
                machineName = hostEntry.HostName;
            }
            catch (Exception)
            {
                return "Not Available";
            }
            Console.WriteLine("{0}", machineName);
            return machineName;
        }

        private string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                System.Console.WriteLine(macAddress);
                return macAddress;
            }

            else
            {
                return "own Machine";
            }
        }

        private bool CheckIpValid(string ipAddress)
        {
            try
            {
                // null and empty check
                if (string.IsNullOrEmpty(ipAddress))
                {
                    return false;
                }

                // check no of blocks
                string[] ipBlocks = ipAddress.Split('.');
                if (ipBlocks.Length != 4)
                    return false;

                // check ipaddress format
                IPAddress.Parse(ipAddress);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void IPCompare(string startIP, string endIP, out int[] startIPBlock, out int[] endIPBlock)
        {
            string[] startIPBlockStr, endIPBlockStr;
            bool needToSwap = false;
            startIPBlockStr = startIP.Split('.');
            endIPBlockStr = endIP.Split('.');

            startIPBlock = Array.ConvertAll(startIPBlockStr, int.Parse);
            endIPBlock = Array.ConvertAll(endIPBlockStr, int.Parse);

            for (int iter = 0; iter <= 3; iter++) // ip address only has 4 block
            {
                if (startIPBlock[iter] > endIPBlock[iter])
                {
                    needToSwap = true;
                    break;
                }
                else if (startIPBlock[iter] < endIPBlock[iter])
                {
                    needToSwap = false;
                    break;
                }
                else if (startIPBlock[iter] == endIPBlock[iter])
                {
                    continue;
                }
            }
            if (needToSwap)
            {
                startIPBlock = Array.ConvertAll(endIPBlockStr, int.Parse);
                endIPBlock = Array.ConvertAll(startIPBlockStr, int.Parse);
                System.Console.WriteLine("since the start ip is greater than end ip....Need to swap\n");
            }
            else
            {
                System.Console.WriteLine("Start IP is lesser than End IP\n");
            }
        }

        private void PlanOfAction(int[] startIpBlocks, int[] endIpBlocks)
        {
            int[] endPoint = { 255, 255, 255, 255 };
            int[] startPoint = { 0, 0, 0, 0 };

            for (startPoint[0] = startIpBlocks[0]; startPoint[0] <= endIpBlocks[0]; startPoint[0]++)
            {
                endPoint[1] = endIpBlocks[0] == startPoint[0] ? endIpBlocks[1] : 255;
                startPoint[1] = startPoint[0] == startIpBlocks[0] ? startIpBlocks[1] : 0;
                for (; startPoint[1] <= endPoint[1]; startPoint[1]++)
                {
                    endPoint[2] = (endIpBlocks[0] == startPoint[0]) && (endIpBlocks[1] == startPoint[1]) ? endIpBlocks[2] : 255;
                    startPoint[2] = (startIpBlocks[0] == startPoint[0]) && (startIpBlocks[1] == startPoint[1]) ? startIpBlocks[2] : 0;
                    for (; startPoint[2] <= endPoint[2]; startPoint[2]++)
                    {
                        endPoint[3] = (endIpBlocks[0] == startPoint[0]) && (endIpBlocks[1] == startPoint[1]) && (endIpBlocks[2] == startPoint[2]) ? endIpBlocks[3] : 255;
                        startPoint[3] = (startIpBlocks[0] == startPoint[0]) && (startIpBlocks[1] == startPoint[1]) && (startIpBlocks[2] == startPoint[2]) ? startIpBlocks[3] : 0;
                        for (; startPoint[3] <= endPoint[3]; startPoint[3]++)
                        {
                            string ip = startPoint[0].ToString() + '.' + startPoint[1].ToString() + '.' + startPoint[2].ToString() + '.' + startPoint[3].ToString();
                            System.Console.WriteLine("{0}", ip);
                            if (ping(ip))
                            {
                                Console.WriteLine("machine reachable {0}", ip);
                                Console.WriteLine("fetching machine info");
                                GetMachineNameFromIPAddress(ip);
                                GetMacAddress(ip);
                                PortScan(ip, portRange, portScanType);
                            }
                            else
                            {
                                System.Console.WriteLine("machine not reachable {0}", ip);
                            }
                        }
                    }
                }
            }
        }
        public void IpScan(string startIpAddress, string endIpAddress)
        {
            int[] start, end;
            IPCompare(startIpAddress, endIpAddress, out start, out end);
            PlanOfAction(start, end);
        }
        public void ProcessSubnet(IPAddress ip, int subnetMask)
        {
            try
            {
                double NoOfUsableHost;
                NoOfUsableHost = Math.Pow(2, (32 - subnetMask)) - 2;
                Console.WriteLine("no of host " + NoOfUsableHost + "\n");

                uint mask = ~(uint.MaxValue >> subnetMask);

                // Convert the IP address to bytes.
                byte[] ipBytes = ip.GetAddressBytes();

                // BitConverter gives bytes in opposite order to GetAddressBytes().
                byte[] maskBytes = BitConverter.GetBytes(mask).Reverse().ToArray();

                byte[] startIPBytes = new byte[ipBytes.Length];
                byte[] endIPBytes = new byte[ipBytes.Length];

                // Calculate the bytes of the start and end IP addresses.
                for (int i = 0; i < ipBytes.Length; i++)
                {
                    startIPBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                    endIPBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
                }

                // Convert the bytes to IP addresses.
                IPAddress startIP = new IPAddress(startIPBytes);
                IPAddress endIP = new IPAddress(endIPBytes);

                Console.WriteLine("start ip address {0}\nend ip address {1}\n", startIP.ToString(), endIP.ToString());
                IpScan(startIP.ToString(), endIP.ToString());

            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }

        public void PortScan(string ip, string portRange, string scantechnique)
        {
            string pythonScriptDir = "\"E:\\projects\\Subnet Scanner\\PortScanner.py\"";
            string args = pythonScriptDir + " " + ip + " " + portRange + " " + scantechnique;
            string pythonExePath = "C:\\Users\\ashwin-8919\\AppData\\Local\\Programs\\Python\\Python38\\python.exe";
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExePath;
            start.Arguments = args;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }

        }
        public bool setPortScanParams()
        {
            try
            {

                Console.WriteLine("Port scanner range(ex- 1-100, 15-1000, etc)");
                string[] scantype = { "connect", "half", "fin", "xmas", "null" };
                portRange = Convert.ToString(Console.ReadLine());
                Console.WriteLine("Port scanner type(half, connect, fin, xmas, null)");
                portScanType = (Convert.ToString(Console.ReadLine())).ToLower();
                if(!scantype.Contains(portScanType))
                {
                    return false;
                }

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            try
            {
                MainClass Main = new MainClass();
                int choice;
                Console.WriteLine("IP SCANNER");
                Console.WriteLine("1. subnet scan");
                Console.WriteLine("2. ip range scan");
                Console.WriteLine("Enter the choice");

                choice = int.Parse(Console.ReadLine());
                if( !Main.setPortScanParams())
                {
                    Console.ReadKey();
                    return;
                }
                if (choice == 1)
                {
                    string ipAddress;
                    int subnetMask;
                    Console.WriteLine("Enter the any ip address within the subnet");
                    ipAddress = Console.ReadLine();
                    Console.WriteLine("Enter the subnet mask");
                    subnetMask = int.Parse(Console.ReadLine());
                    if (Main.CheckIpValid(ipAddress) && (subnetMask <= 32))
                        Main.ProcessSubnet(IPAddress.Parse(ipAddress), subnetMask);
                    else
                        Console.WriteLine("invalid data");
                }
                else if (choice == 2)
                {
                    string startIpAddress, endIpAddress;
                    System.Console.WriteLine("enter start ip address\n");
                    startIpAddress = Console.ReadLine();
                    System.Console.WriteLine("enter end ip address\n");
                    endIpAddress = Console.ReadLine();
                    if (Main.CheckIpValid(startIpAddress) && Main.CheckIpValid(endIpAddress))
                    {
                        Main.IpScan(startIpAddress, endIpAddress);
                    }
                    else
                    {
                        Console.WriteLine("invalid data");
                    }
                }
                else
                {
                    Console.WriteLine("incorrect choice");
                }
                //pause
                Console.ReadKey();
            }
            catch (FormatException Ex)
            {
                Console.WriteLine(Ex.Message);
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }

        }
    }
}
