using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

            StartServer();
            
            bool runForever = true;
            List<BigQClient> clients;
            List<BigQUser> users;
            List<BigQPermission> perms;

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
                        Console.WriteLine("  q       quit");
                        Console.WriteLine("  cls     clear screen");
                        Console.WriteLine("  who     list connected users");
                        Console.WriteLine("  count   show server connection count");
                        Console.WriteLine("  debug   enable/disable console debug (currently " + server.ConsoleDebug + ")");
                        Console.WriteLine("  auth    display users.json and permissions.json authorization");
                        Console.WriteLine("");
                        break;

                    case "q":
                        runForever = false;
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

                    case "debug":
                        server.ConsoleDebug = !server.ConsoleDebug;
                        break;

                    case "auth":
                        Console.WriteLine("users.json");
                        users = server.ListCurrentUsersFile();
                        if (users != null && users.Count > 0)
                        {
                            foreach (BigQUser curr in users)
                            {
                                string userEntry = "  " + curr.Email + " " + curr.Password + " " + curr.Permission + " IP: ";
                                if (curr.IPWhiteList != null && curr.IPWhiteList.Count > 0)
                                {
                                    foreach (string currIP in curr.IPWhiteList) userEntry += currIP + " ";
                                }
                                else userEntry += "*";
                                Console.WriteLine(userEntry);
                            }
                        }
                        else Console.WriteLine("(null)");

                        Console.WriteLine("permissions.json");
                        perms = server.ListCurrentPermissionsFile();
                        if (perms != null && perms.Count > 0)
                        {
                            foreach (BigQPermission curr in perms)
                            {
                                string permEntry = "  " + curr.Name + " Login: " + curr.Login + " Allowed: ";
                                if (curr.Permissions != null && curr.Permissions.Count > 0)
                                {
                                    foreach (string currPerm in curr.Permissions) permEntry += currPerm + " ";
                                }
                                else permEntry += "*";
                                Console.WriteLine(permEntry);
                            }
                        }
                        else Console.WriteLine("(null)");
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

        static bool StartServer()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Attempting to start server...");

                    server = new BigQServer(null, port, null, 8223, debug, true, true, true, heartbeat);
                    server.LogLockMethodResponseTime = false;
                    server.LogMessageResponseTime = false;

                    server.MessageReceived = MessageReceived;
                    server.ServerStopped = StartServer;
                    server.ClientConnected = ClientConnected;
                    server.ClientLogin = ClientLogin;
                    server.ClientDisconnected = ClientDisconnected;
                    // server.LogMessage = LogMessage;

                    Console.WriteLine("Listening on TCP/" + port + " (heartbeat " + heartbeat + ", debug " + debug + ")");

                    return true;
                }
                catch (Exception EOuter)
                {
                    Console.WriteLine("*** Exception while attempting to start server: " + EOuter.Message);
                    Console.WriteLine("*** Retrying in five seconds");
                    Thread.Sleep(5000);
                }
            }
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
