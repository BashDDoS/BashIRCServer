using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
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
                IPAddress localAddr = IPAddress.Parse("169.254.247.82");

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
                    
                    Console.WriteLine("Client with IP: " + client.Client.RemoteEndPoint);

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();
                    
                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a UTF8 string.
                        data = Encoding.UTF8.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);
                        
                        // Response for all data
                        if (data.FirstOrDefault().ToString() == "!")
                        {
                            // Handle commands
                            switch (data)
                            {
                                case "!TestCommand":
                                    if (!Commands.TestCommand())
                                        Console.WriteLine("Command error: " + data);
                                    break;
                                default:
                                    // Should log attempt and IP
                                    break;
                            }
                        }
                        
                        byte[] msg = Encoding.UTF8.GetBytes(data);
                        
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);

                        i = stream.Read(bytes, 0, bytes.Length);
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
        public static bool TestCommand()
        {
            Console.WriteLine("Test Command received!");
            return true;
        }
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