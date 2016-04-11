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
        static BigQServer server;

        static void Main(string[] args)
        {
            server = new BigQServer(null, port, true, false, true, true);

            server.MessageReceived = MessageReceived;
            server.ServerStopped = ServerStopped;
            server.ClientConnected = ClientConnected;
            server.ClientLogin = ClientLogin;
            server.ClientDisconnected = ClientDisconnected;

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
            Console.WriteLine("Listening on TCP/" + port);
            Console.WriteLine("Press ENTER to exit");
            Console.WriteLine("");

            Console.ReadLine();
        }

        static bool MessageReceived(BigQMessage msg)
        {
            Console.WriteLine(msg.SenderGuid + " -> " + msg.RecipientGuid + ": " + msg.Data.ToString());
            return true;
        }

        static bool ServerStopped()
        {
            // restart
            Console.WriteLine("***");
            Console.WriteLine("*** Server stopped, attempting to restart ***");
            Console.WriteLine("***");

            server = new BigQServer(null, 8000, false, true, true, true);
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
    }
}
