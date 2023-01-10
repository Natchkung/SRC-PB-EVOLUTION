using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;

namespace PointBlank.AntiCheat
{
    internal class Program
    {
        private static Socket server;

        private static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];


            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(epoch.AddSeconds(secondsSince1970), target ?? TimeZoneInfo.Local);
        }
        private static string getOSName()
        {
            OperatingSystem os = Environment.OSVersion;
            ComputerInfo comp = new ComputerInfo();
            string operatingSystem = comp.OSFullName;
            if (os.ServicePack != "")
                operatingSystem += " " + os.ServicePack;
            operatingSystem += " (" + ((Environment.Is64BitOperatingSystem ? "64" : "32") + " bits)");
            return operatingSystem;
        }
        /*private static DateTime GetDate()
        {
            try
            {
                using (var response = WebRequest.Create("http://www.google.com").GetResponse())
                    return DateTime.ParseExact(response.Headers["date"],
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeUniversal).ToUniversalTime();
            }
            catch
            {
                return new DateTime();
            }
        }*/
        protected static void Main(string[] args)
        {
            string Date = GetLinkerTime(Assembly.GetExecutingAssembly(), null).ToString("dd/MM/yyyy HH:mm");
            Logger.checkDirectory();
            Console.Title = "Point Blank Server By";
            Logger.info("________________________________________________________________________________");
            Logger.info("                                                                               ");
            Logger.info("                                                                               ");
            Logger.info("                             POINT BLANK ANTICHEAT                             ");
            Logger.info("                                                                               ");
            Logger.info("                                                                               ");
            Logger.info("_______________________________ " + Date + " _______________________________");
            Logger.warning("[Status] Server AntiCheat is Runing...");
            bool check = true;
            if (check)
            {
                Start();
                Logger.warning(StartSuccess());
                updateRAM2();
            }
            Process.GetCurrentProcess().WaitForExit();
        }

        private static string StartSuccess()
        {
            return "[Status] Active Server (" + DateTime.Now.ToString("yy/MM/dd HH:mm:ss") + ")";
        }

        public static async void updateRAM2()
        {
            while (true)
            {
                try
                {
                    Console.Title = "Point Blank - NProject! [Used RAM: " + (GC.GetTotalMemory(true) / 1024) + " KB]";
                    ///
                    Socket Packet = server.Accept();
                    byte[] buffer;

                    // Receive

                    buffer = new byte[1024];
                    int rec = Packet.Receive(buffer, 0, buffer.Length, 0);
                    Array.Resize(ref buffer, rec);
                    string msg = Encoding.Default.GetString(buffer);
                    string Sub = msg.Substring(0, 15);
                    string SubID = msg.Substring(16, msg.Length - 16);
                    if (Sub == "send screenshot")
                    {
                        string a = SendReportRequest("http://localhost:8080/launcher/api.php?act=report", SubID);
                        Console.WriteLine(a);
                    }
                }
                catch
                {
                    // Skip
                }
                await Task.Delay(1000);
            }
        }

        public static bool Start()
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint Local = new IPEndPoint(IPAddress.Parse("103.233.195.182"), 25009);
                server.Bind(Local);
                server.Listen((int)SocketOptionName.MaxConnections);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string SendReportRequest(string url, string token)
        {
            CookieContainer Cookie = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "NPBProjectV2";
            request.CookieContainer = Cookie; // use the global cookie variable
            string postData = "token=" + token;
            byte[] data = Encoding.UTF8.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            request.ContentLength = data.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            WebResponse response = (HttpWebResponse)request.GetResponse();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
    }
}