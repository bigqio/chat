using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            
            while (true)
            {
                Console.Write("Server IP or hostname: ");
                server = Console.ReadLine();
                if (String.IsNullOrEmpty(server)) continue;
                break;
            }

            while (true)
            {
                Console.Write("Port number: ");
                string portString = Console.ReadLine();
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

            Console.WriteLine("Attempting connection to server: " + server + ":" + port);

            try
            {
                client = new BigQClient(name, name, server, port, 20000, false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while connecting: " + e.Message);
                return;
            }

            client.AsyncMessageReceived = AsyncMessageReceived;
            client.SyncMessageReceived = SyncMessageReceived;
            client.ServerDisconnected = ServerDisconnected;

            Console.WriteLine("Attempting login to server: " + server + ":" + port);
            if (!client.Login(out response))
            {
                Console.WriteLine("*** Unable to login to server, exiting");
                return;
            }
            else
            {
                Console.WriteLine("Login successful");
            }

            bool runForever = true;
            while (runForever)
            {
                Console.Write("Command [? for help]: ");
                string input = Console.ReadLine();
                if (String.IsNullOrEmpty(input)) continue;
                
                if (input.StartsWith("/"))
                {
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
                            Console.WriteLine("Menu");
                            Console.WriteLine("  q                  quit");
                            Console.WriteLine("  who                list all connected users");
                            Console.WriteLine("  /(handle) (msg)    send message (msg) to user with handle (handle)");
                            Console.WriteLine("                     leave parentheses off for both handle and message data");
                            Console.WriteLine("");
                            break;

                        case "q":
                        case "quit":
                            runForever = false;
                            break;

                        case "who":
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

        static bool ServerDisconnected()
        {
            Console.WriteLine("***");
            Console.WriteLine("*** Disconnected, attempting to reconnect ***");
            Console.WriteLine("***");
            client = new BigQClient(name, name, server, port, 5000, false);
            return true;
        }

        static bool LogMessage(string msg)
        {
            Console.WriteLine("BigQClient message: " + msg);
            return true;
        }
    }
}
