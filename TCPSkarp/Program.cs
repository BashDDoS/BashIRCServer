using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPSkarp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 845.
                Int32 port = 845;
                IPAddress localAddr = IPAddress.Parse("0.0.0.0");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();



                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();



                    int i;
                    int j = 0;
                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        j++;
                        // Translate data bytes to a UTF8 string.
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);

                        Console.WriteLine("Received: {0} from {1}", data, client.Client.LocalEndPoint);
                        Package recievedPack = PackageManagement.PackageParser(data);

                        PackageManagement.HandlePackage(recievedPack);

                        data = data.ToUpper();
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);


                    }
                    if (j == 0)
                    {
                        Console.WriteLine("pinged by " + client.Client.RemoteEndPoint);
                    }
                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                Console.WriteLine("Stopping server...");
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }

    public static class Commands
    {
        public static void RunCommand(string cmd)
        {
            TestCommand();
        }

        public static bool TestCommand()
        {
            Console.WriteLine("Test Command received!");
            return true;
        }
    }

    public static class Authentication{
        public static void Authenticate(string data)
        {
            Console.Write("Should authenticate!");
        }
    }

    public static class PackageManagement
    {
        public static Package PackageParser(string pkg)
        {
            Package returnPack;

            string data = pkg.Substring(4, pkg.Length - 4);
            switch (pkg.Substring(0, 3).ToLower())
            {
                case "msg": returnPack = new Package(CmdType.message, data); break;
                case "cmd": returnPack = new Package(CmdType.command, data); break;
                case "aut": returnPack = new Package(CmdType.authentication, data); break;
                default: returnPack = new Package(CmdType.error, ""); break;
            }
            return returnPack;
        }

        public static void HandlePackage(Package pack)
        {
            switch (pack.type)
            {
                case CmdType.message: MessageManager.HandleMessage(pack.data); break;
                case CmdType.command: Commands.RunCommand(pack.data); break;
                case CmdType.authentication: Authentication.Authenticate(pack.data); break;
                case CmdType.error: Console.Write("ERROR IN PACK"); break;
            }
        }
    }

    public static class MessageManager
    {
        public static void HandleMessage(string msg)
        {

        }
    }

    public struct Package
    {
        public CmdType type;
        public string data;

        public Package(CmdType _type, string _data)
        {
            type = _type;
            data = _data;
        }
    }

    public enum CmdType
    {
        message,
        command,
        authentication,
        error,
        ping
    }

    public class User
    {
        private int _id;
        private string _username;
        private string _password;
        private string _ip;
        private int _lastSeen = 0;
        
        public User(int idVal, string usernameVal, string passwordVal, string ipVal)
        {
            _id = idVal;
            _username = usernameVal;
            _password = passwordVal;
            _ip = ipVal;
            _lastSeen = (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public bool IsValid()
        {
            // Check if ID, Username and Password exist in DB
            return true;
        }

        public int GetRank()
        {
            // Check IRC rank in DB
            return 1;
        }

        public bool IsSessionActive(string ipVal)
        {
            return this._lastSeen + 1800 > (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds && this.IsValid();
        }
    }
}
