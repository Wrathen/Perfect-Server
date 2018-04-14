using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Server
    {
        static IPAddress ServerIP = IPAddress.Parse("127.0.0.1");
        static int ServerPort = 8080;
        static TcpListener Listener = null;
        public static List<Player> Players = new List<Player>();
        static int maxPlayers = 2;

        static void Main(string[] args)
        {
            Listener = new TcpListener(ServerIP, ServerPort);
            Thread Command = new Thread(new ThreadStart(OnCommand));
            Thread Listen = new Thread(new ThreadStart(OnConnect));

            Listener.Start();
            Command.Start();
            Listen.Start();

            Console.WriteLine("Server Created!");
        }

        static void OnConnect()
        {
            TcpClient client = Listener.AcceptTcpClient();
            Player player = new Player(client);

            if (Players.Count > maxPlayers - 1) // If the Server is Full decline the Incoming Connection
            {
                SendMessage(player, "Server Is Full!"); // Send a Message To Incoming Client.
                client.Close(); // Close The Connection.
            }
            else
            {
                Console.WriteLine("Player " + player.ID + " Has Connected!");
                Players.Add(player);
                new Thread(new ThreadStart(() => OnListen(player))).Start();
            }

            OnConnect();
        }
        static void OnListen(Player player)
        {
            NetworkStream stream = player.Client.GetStream();
            byte[] receivedBuffer = new byte[49152];

            try { stream.Read(receivedBuffer, 0, receivedBuffer.Length); }
            catch
            {
                OnDisconnect(player);
                return;
            }

            ParsePlayerMessage(player, Util.ByteArrayToString(receivedBuffer));
            if (player.Username == null) return; // If The Player Isn't Still Logged In Then Disconnect That Player.e
            OnListen(player);
        }        
        static void OnDisconnect(Player player)
        {
            Players.Remove(player);
            player.Client.Close();
            Console.WriteLine("Player " + player.ID + " Has Disconnected!");
        }
        static void OnCommand()
        {
            List<string> command = Util.FetchRequest(Console.ReadLine(), " ");
            switch (command[0])
            {
                case "send": // send all msg || send id msg || send username msg
                    if (command.Count < 3)
                    {
                        Console.WriteLine("Invalid Syntax!");
                        break;
                    }
                    SendMessage(command);
                    break;
                case "create": // create username password
                    if (command.Count < 3)
                    {
                        Console.WriteLine("Invalid Syntax!");
                        break;
                    }
                    Account.CreateAccount(command[1], command[2]);
                    break;
                case "clear":
                    Console.Clear();
                    break;
                default:
                    Console.WriteLine("Unknown Command: " + command[0]);
                    break;
            }
            OnCommand();
        }

        static void KickPlayer(Player player, string msg)
        {
            SendMessage(player, "You Have Been Kicked From The Server: " + msg);
            OnDisconnect(player);
        }     
        static void ParsePlayerMessage(Player player, string message)
        {
            if(player.Username == null)
            {
                List<string> request = Util.FetchRequest(message, ":");
                if (request.Count < 3)
                {
                    KickPlayer(player, "Invalid Request!");
                    return;
                }

                request[1] = Util.MakeStringCooler(request[1]); // adMiN -> Admin
                switch (request[0])
                {
                    case "Login":                       
                        if (Account.GetAccount(request[1], request[2]) == "Success!")
                        {
                            if (Player.IsPlayerAlreadyInGame(request[1])) // If Player Is Already In The Game Then Dont Let New Player Join The Game.
                            {
                                KickPlayer(player, "Disconnected! You are already in the Game!");
                                break;
                            }
                            Console.WriteLine(request[1] + " Has Logged In! [Player " + player.ID + "]");
                            SendMessage(player, "Welcome Mr. " + request[1] + "! You Are Now Logged In!");
                            player.Username = request[1];
                            player.Password = request[2];
                        }
                        else KickPlayer(player, "Account Not Found Or Wrong Password");
                        break;
                    case "Register":
                        if (Account.CreateAccount(request[1], request[2])) KickPlayer(player, "Account Created!");
                        else KickPlayer(player, "Account Exists!");
                        break;
                }
                return;
            }

            if (message.Substring(0, 1) == "/")
            {
                List<string> request = Util.FetchRequest(message.Substring(1, message.Length - 1), " ");
                switch (request[0])
                {
                    case "hi":
                        SendMessage(player, "Hey!");
                        break;
                    case "commands":
                        SendMessage(player, "/Hi and /Commands are Current At The Moment.");
                        break;
                    default:
                        SendMessage(player, "Command Not Found!");
                        break;
                }
                Console.WriteLine("Player " + player.ID + " Tried to Execute This Command: /" + request.ToString());
                return;
            }

            Console.WriteLine(player.Username + ": " + message);
        }
    
        static void SendMessage(Player player, string message)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
            try { player.Client.GetStream().Write(msg, 0, msg.Length); }
            catch { OnDisconnect(player); }
        }
        static void SendMessage(List<string> command)
        {
            string message = "";
            // Command's message is seperate. Like command[2]=Hello command[3]=World command[4]=!
            // We Make message = "Hello World !";
            for (int i = 2; i < command.Count; i++) message += command[i] + " ";

            if (command[1] == "all") // send all msg
            {
                foreach (Player player in Players)
                {
                    SendMessage(player, message);
                    Console.WriteLine("Message Sent to Player " + player.ID + "");
                }
                return;
            }
            try { SendMessage(Convert.ToInt32(command[1]), message); } // send <id> msg
            catch { SendMessage(command[1], message); } // send <username> msg
        }
        static void SendMessage(int ID, string message)
        {
            Player player = Player.GetPlayerByID(ID);
            if (player == null)
            {
                Console.WriteLine("Player " + ID + " Not Found!");
                return;
            }
            SendMessage(player, message);
        }
        static void SendMessage(string Username, string message)
        {
            Player player = Player.GetPlayerByUsername(Username);
            if (player == null)
            {
                Console.WriteLine(Username + " Not Found!");
                return;
            }
            SendMessage(player, message);
        }
    }
}