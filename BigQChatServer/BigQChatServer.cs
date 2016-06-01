using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigQ;

namespace BigQChat
{
    class BigQChatServer
    {
        static int port = 8222;
        static int heartbeat = 0;
        static bool debug = false;
        static BigQServer server;
        
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(@" $$\       $$\                      ");
            Console.WriteLine(@" $$ |      \__|                     ");
            Console.WriteLine(@" $$$$$$$\  $$\  $$$$$$\   $$$$$$\   ");
            Console.WriteLine(@" $$  __$$\ $$ |$$  __$$\ $$  __$$\  ");
            Console.WriteLine(@" $$ |  $$ |$$ |$$ /  $$ |$$ /  $$ | ");
            Console.WriteLine(@" $$ |  $$ |$$ |$$ |  $$ |$$ |  $$ | ");
            Console.WriteLine(@" $$$$$$$  |$$ |\$$$$$$$ |\$$$$$$$ | ");
            Console.WriteLine(@" \_______/ \__| \____$$ | \____$$ | ");
            Console.WriteLine(@"               $$\   $$ |      $$ | ");
            Console.WriteLine(@"               \$$$$$$  |      $$ | ");
            Console.WriteLine(@"                \______/       \__| ");
            Console.ResetColor();

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("BigQ Chat Server");
            Console.WriteLine("");

            if (!GetArguments(args, out port, out heartbeat, out debug))
            {
                port = 8222;
                heartbeat = 0;
                debug = false;
            }

            server = new BigQServer(null, port, null, 8223, debug, false, true, true, heartbeat);

            server.MessageReceived = MessageReceived;
            server.ServerStopped = ServerStopped;
            server.ClientConnected = ClientConnected;
            server.ClientLogin = ClientLogin;
            server.ClientDisconnected = ClientDisconnected;

            Console.WriteLine("Listening on TCP/" + port + " (heartbeat " + heartbeat + ", debug " + debug + ")");

            bool runForever = true;
            List<BigQClient> clients;

            while (runForever)
            {
                Console.Write("Command [? for help] > ");
                string input = Console.ReadLine();
                if (String.IsNullOrEmpty(input)) continue;

                switch (input.ToLower().Trim())
                {
                    case "?":
                        Console.WriteLine("-------------------------------------------------------------------------------");
                        Console.WriteLine("Menu");
                        Console.WriteLine("  q      quit");
                        Console.WriteLine("  cls    clear screen");
                        Console.WriteLine("  who    list connected users");
                        Console.WriteLine("  count  show server connection count");
                        Console.WriteLine("");
                        break;

                    case "c":
                    case "cls":
                        Console.Clear();
                        break;

                    case "who":
                        clients = server.ListClients();
                        if (clients == null) Console.WriteLine("(null)");
                        else if (clients.Count < 1) Console.WriteLine("(empty)");
                        else
                        {
                            Console.WriteLine(clients.Count + " clients connected");
                            foreach (BigQClient curr in clients)
                            {
                                Console.WriteLine("  " + curr.IpPort() + "  " + curr.ClientGuid + "  " + curr.Email);
                            }
                        }
                        break;

                    case "count":
                        Console.WriteLine("Server connection count: " + server.ConnectionCount());
                        break;

                    case "q":
                        runForever = false;
                        break;

                    default:
                        break;
                }
            }

            Console.ReadLine();
        }

        static bool MessageReceived(BigQMessage msg)
        {
            Console.WriteLine(msg.SenderGuid + " -> " + msg.RecipientGuid + ": " + Encoding.UTF8.GetString(msg.Data));
            return true;
        }

        static bool ServerStopped()
        {
            // restart
            Console.WriteLine("*** Server stopped, attempting to restart ***");
            
            server = new BigQServer(null, port, null, 8223, debug, true, true, true, heartbeat);
            server.MessageReceived = MessageReceived;
            server.ServerStopped = ServerStopped;
            server.ClientConnected = ClientConnected;
            server.ClientLogin = ClientLogin;
            server.ClientDisconnected = ClientDisconnected;
            // server.LogMessage = LogMessage;
            return true;
        }

        static bool ClientConnected(BigQClient client)
        {
            // client connected
            Console.WriteLine("*** Client connected: " + client.IpPort() + " " + client.ClientGuid);
            return true;
        }

        static bool ClientLogin(BigQClient client)
        {
            // client login
            Console.WriteLine("*** Client login: " + client.IpPort() + " " + client.ClientGuid);
            return true;
        }

        static bool ClientDisconnected(BigQClient client)
        {
            // client disconnected
            Console.WriteLine("*** Client disconnected: " + client.IpPort() + " " + client.ClientGuid);
            return true;
        }

        static bool LogMessage(string msg)
        {
            Console.WriteLine("BigQServer message: " + msg);
            return true;
        }

        static bool GetArguments(string[] args, out int port, out int heartbeat, out bool debug)
        {
            port = 0;
            heartbeat = 0;
            debug = false;

            if (args == null || args.Length != 3) return false;

            try
            {
                port = Convert.ToInt32(args[0]);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid port number specified on command line");
                return false;
            }

            try
            {
                heartbeat = Convert.ToInt32(args[1]);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid heartbeat interval specified on command line");
                return false;
            }

            try
            {
                debug = Convert.ToBoolean(args[2]);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid value for debug");
                return false;
            }

            return true;
        }
    }
}
