﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BigQ.Core;
using BigQ.Server;

namespace BigQChat
{
    class BigQChatServer
    {
        static string config;
        static Server server;
        static List<ServerClient> clients;
        static List<User> users;
        static List<Permission> perms;

        static void Main(string[] args)
        {
            if (args != null && args.Length == 1) config = args[0];
            else config = null;

            StartServer();
            bool runForever = true;
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
                        Console.WriteLine("  debug   enable/disable console debug (currently " + server.Config.Debug.Enable + ")");
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
                            foreach (ServerClient curr in clients)
                            {
                                Console.WriteLine("  " + curr.IpPort + "  " + curr.ClientGUID + "  " + curr.Email);
                            }
                        }
                        break;
                         
                    case "debug":
                        server.Config.Debug.Enable = !server.Config.Debug.Enable;
                        break;

                    case "auth":
                        Console.WriteLine("Users");
                        users = server.ListCurrentUsersFile();
                        if (users != null && users.Count > 0)
                        {
                            foreach (User curr in users)
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

                        Console.WriteLine("Permissions");
                        perms = server.ListCurrentPermissionsFile();
                        if (perms != null && perms.Count > 0)
                        {
                            foreach (Permission curr in perms)
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

        static bool MessageReceived(Message msg)
        {
            Console.WriteLine(msg.SenderGUID + " -> " + msg.RecipientGUID + ": " + Encoding.UTF8.GetString(msg.Data));
            return true;
        }
        
        static bool StartServer()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Attempting to start server...");

                    server = new Server(config);
                    server.Callbacks.MessageReceived = MessageReceived;
                    server.Callbacks.ServerStopped = StartServer;
                    server.Callbacks.ClientConnected = ClientConnected;
                    server.Callbacks.ClientLogin = ClientLogin;
                    server.Callbacks.ClientDisconnected = ClientDisconnected;
                    // server.Callbacks.LogMessage = LogMessage;

                    Console.WriteLine("Server started");

                    return true;
                }
                catch (Exception EOuter)
                {
                    Console.WriteLine("*** Exception while attempting to start server: " + EOuter.Message);
                    Console.WriteLine("*** Retrying in five seconds");
                    Task.Delay(5000).Wait();
                }
            }
        }

        static bool ClientConnected(ServerClient client)
        {
            // client connected
            Console.WriteLine("*** Client connected: " + client.IpPort + " " + client.ClientGUID);
            return true;
        }

        static bool ClientLogin(ServerClient client)
        {
            // client login
            Console.WriteLine("*** Client login: " + client.IpPort + " " + client.ClientGUID);
            return true;
        }

        static bool ClientDisconnected(ServerClient client)
        {
            // client disconnected
            Console.WriteLine("*** Client disconnected: " + client.IpPort + " " + client.ClientGUID);
            return true;
        }

        static bool LogMessage(string msg)
        {
            Console.WriteLine("BigQServer message: " + msg);
            return true;
        }
    }
}
