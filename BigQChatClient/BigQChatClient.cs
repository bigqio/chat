using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BigQ;

namespace BigQChatClient
{
    class BigQChatClient
    {
        static int port;
        static string server;
        static string name = "";
        static BigQClient client;
        static BigQMessage response;
        static List<BigQClient> users;
        const bool DEBUG = false;

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
            Console.WriteLine("BigQ Chat Client");
            Console.WriteLine("");
            
            Console.Write("Server IP or hostname [ENTER for chat.bigq.io]: ");
            server = Console.ReadLine();
            if (String.IsNullOrEmpty(server)) server = "chat.bigq.io";

            while (true)
            {
                Console.Write("Port number [ENTER for 8222]: ");
                string portString = Console.ReadLine();
                if (String.IsNullOrEmpty(portString))
                {
                    port = 8222;
                    break;
                }

                if (!Int32.TryParse(portString, out port))
                {
                    Console.WriteLine("Positive numbers only, please");
                    continue;
                }

                if (port < 1)
                {
                    Console.WriteLine("Positive numbers only, please");
                    continue;
                }

                break;
            }

            while (true)
            {
                Console.Write("Nickname [no spaces please]: ");
                name = Console.ReadLine();
                if (String.IsNullOrEmpty(name)) continue;
                break;
            }

            ConnectToServer();

            bool runForever = true;
            while (runForever)
            {
                if (client == null) Console.Write("[OFFLINE] ");
                Console.Write("Command [? for help]: ");

                string input = Console.ReadLine();
                if (String.IsNullOrEmpty(input)) continue;
                
                if (input.StartsWith("/"))
                {
                    if (client == null) continue;

                    string msg = input.Substring(1);
                    string recipient = "";
                    string recipientMessage = "";

                    int currPosition = 0;
                    while (true)
                    {
                        if (currPosition >= msg.Length) break;
                        if (msg[currPosition] != ' ')
                        {
                            recipient += msg[currPosition];
                            currPosition++;
                        }
                        else
                        {
                            currPosition++;
                            break;
                        }
                    }

                    recipientMessage = input.Substring(currPosition).Trim();

                    if (String.IsNullOrEmpty(recipient))
                    {
                        Console.WriteLine("*** No recipient specified");
                        continue;
                    }

                    if (String.IsNullOrEmpty(recipientMessage))
                    {
                        Console.WriteLine("*** No message specified");
                        continue;
                    }

                    Console.WriteLine("Sending message to " + recipient + ": " + recipientMessage);
                    if (!client.SendPrivateMessageAsync(recipient, recipientMessage))
                    {
                        Console.WriteLine("*** Unable to send message to " + recipient);
                    }

                    continue;
                }
                else
                {
                    switch (input.ToLower())
                    {
                        case "?":
                            Console.WriteLine("");
                            Console.WriteLine("Available Commands:");
                            Console.WriteLine("  q                  quit");
                            Console.WriteLine("  cls                clear the screen");
                            Console.WriteLine("  whoami             show my TCP endpoint");
                            Console.WriteLine("  who                list all connected users");
                            Console.WriteLine("  /(handle) (msg)    send message (msg) to user with handle (handle)");
                            Console.WriteLine("                     leave parentheses off for both handle and message data");
                            Console.WriteLine("");
                            break;

                        case "q":
                        case "quit":
                            runForever = false;
                            break;

                        case "c":
                        case "cls":
                            Console.Clear();
                            break;

                        case "login":
                            if (client == null) break;
                            if (!client.Login(out response))
                            {
                                Console.WriteLine("*** Login failed");
                            }
                            else
                            {
                                Console.WriteLine("Login succeeded");
                            }
                            break;

                        case "whoami":
                            if (client == null) break;
                            Console.Write(client.IpPort());
                            if (!String.IsNullOrEmpty(client.ClientGuid)) Console.WriteLine("  GUID " + client.ClientGuid);
                            else Console.WriteLine("[not logged in]");
                            break;

                        case "who":
                            if (client == null) break;
                            if (!client.ListClients(out response, out users))
                            {
                                Console.WriteLine("*** Unable to retrieve user list");
                            }
                            else
                            {
                                if (users == null || users.Count < 1)
                                {
                                    Console.WriteLine("*** Null or empty user list retrieved");
                                }
                                else
                                {
                                    List<BigQClient> deduped = users.Distinct().ToList();

                                    Console.WriteLine("Connected users:");
                                    foreach (BigQClient curr in deduped)
                                    {
                                        Console.WriteLine("  " + curr.ClientGuid);
                                    }
                                }
                            }
                            break;

                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }
            }
        }
        
        static bool AsyncMessageReceived(BigQMessage msg)
        {
            if (msg == null) return false;
            if (msg.Data == null) return false;

            Console.WriteLine(msg.SenderGuid + " -> " + msg.RecipientGuid + ": " + msg.Data.ToString());
            return true;
        }

        static object SyncMessageReceived(BigQMessage msg)
        {
            if (msg == null) return null;
            if (msg.Data == null) return null;

            Console.WriteLine("Received synchronous message from " + msg.SenderGuid);
            Console.WriteLine(msg.Data.ToString());
            Console.WriteLine("Press ENTER to take control of the console and then type your response");
            Console.Write("Response [ENTER for 'received!']: ");
            string resp = Console.ReadLine();
            return resp;
        }

        static bool ConnectToServer()
        {
            try
            {
                Console.WriteLine("Attempting to connect to " + server + ":" + port);
                if (client != null) client.Close();
                client = null;
                client = new BigQClient(name, name, server, port, 5000, 0, DEBUG);

                client.AsyncMessageReceived = AsyncMessageReceived;
                client.SyncMessageReceived = SyncMessageReceived;
                client.ServerDisconnected = ConnectToServer;
                // client.LogMessage = LogMessage;

                BigQMessage response;
                if (!client.Login(out response))
                {
                    Console.WriteLine("Unable to login, retrying in five seconds");
                    Thread.Sleep(5000);
                    return ConnectToServer();
                }

                Console.WriteLine("Successfully connected to " + server + ":" + port);
                return true;
            }
            catch (SocketException)
            {
                Console.WriteLine("*** Unable to connect to " + server + ":" + port + " (port not reachable)");
                Console.WriteLine("*** Retrying in five seconds");
                Thread.Sleep(5000);
                return ConnectToServer();
            }
            catch (TimeoutException)
            {
                Console.WriteLine("*** Timeout connecting to " + server + ":" + port);
                Console.WriteLine("*** Retrying in five seconds");
                Thread.Sleep(5000);
                return ConnectToServer();
            }
            catch (Exception e)
            {
                Console.WriteLine("*** Unable to connect to " + server + ":" + port + " due to the following exception:");
                PrintException("ConnectToServer", e);
                Console.WriteLine("*** Retrying in five seconds");
                Thread.Sleep(5000);
                return ConnectToServer();                
            }
        }

        static bool LogMessage(string msg)
        {
            Console.WriteLine("BigQClient message: " + msg);
            return true;
        }

        static void PrintException(string method, Exception e)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine(" = Method: " + method);
            Console.WriteLine(" = Exception Type: " + e.GetType().ToString());
            Console.WriteLine(" = Exception Data: " + e.Data);
            Console.WriteLine(" = Inner Exception: " + e.InnerException);
            Console.WriteLine(" = Exception Message: " + e.Message);
            Console.WriteLine(" = Exception Source: " + e.Source);
            Console.WriteLine(" = Exception StackTrace: " + e.StackTrace);
            Console.WriteLine("================================================================================");
        }
    }
}
