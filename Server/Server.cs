using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Server
    {
        private static TcpListener Listener = null;
        public static List<Player> Players = new List<Player>();

        private static void Main(string[] args)
        {
            Listener = new TcpListener(Constants.SERVER_IP, Constants.SERVER_PORT);
            Thread Command = new Thread(new ThreadStart(OnCommand));
            Thread Listen = new Thread(new ThreadStart(OnConnect));

            Listener.Start();
            Command.Start();
            Listen.Start();

            Console.WriteLine("Server Created!");
        }
        private static void OnConnect()
        {
            TcpClient client = Listener.AcceptTcpClient();
            Player player = new Player(client);

            if (Players.Count > Constants.SERVER_MAXPLAYERS - 1) // If the Server is Full decline the Incoming Connection
            {
                SendMessage(player, "Server Is Full!"); // Send a Message To Incoming Client.
                client.Close(); // Close The Connection.
            }
            else
            {
                Console.WriteLine("Player " + player.ID + " Has Connected!");
                Players.Add(player);
                player.Client.GetStream().BeginRead(player.receivedBuffer, 0, player.receivedBuffer.Length, OnRead, player);
            }

            OnConnect();
        }
        private static void OnRead(IAsyncResult AR)
        {
            Player player = (Player) AR.AsyncState;
            NetworkStream stream = player.Client.GetStream();
            int readBytes = 0;
            try
            {
                readBytes = stream.EndRead(AR);
                if (readBytes == 0)
                {
                    OnDisconnect(player);
                    return;
                }
            }
            catch
            {
                OnDisconnect(player);
                return;
            }
            Array.Resize(ref player.receivedBuffer, readBytes);
            string message = Util.ByteArrayToString(player.receivedBuffer);
            ParsePlayerMessage(player, message);
            if (player.Username == null) return; // If The Player Isn't Still Logged In Then Stop Listening.
            player.receivedBuffer = new byte[512];
            stream.BeginRead(player.receivedBuffer, 0, player.receivedBuffer.Length, OnRead, player);
        }
        private static void OnDisconnect(Player player)
        {
            Players.Remove(player);
            player.Client.Close();
            Console.WriteLine("Player " + player.ID + " Has Disconnected!");
        }
        private static void OnCommand()
        {
            List<string> command = Util.FetchString(Console.ReadLine(), " ");
            switch (command[0])
            {
                case "/send": // send all msg || send id msg || send username msg
                    if (command.Count < 3)
                    {
                        Console.WriteLine("Invalid Syntax!");
                        break;
                    }
                    SendMessage(command);
                    break;
                case "/create": // create username password
                    if (command.Count < 4)
                    {
                        Console.WriteLine("Invalid Syntax!");
                        break;
                    }
                    if (command[1] == "account")
                        Account.CreateAccount(command[2], command[3]);
                    if (command[1] == "character" && command.Count == 5)
                        Console.WriteLine(Character.CreateCharacter(command[2], command[3], command[4]));
                    else Console.WriteLine("Invalid Syntax! Usage: /create character <username> <charName> <charClass>");
                    break;
                case "/delete":
                    if (command[1] == "character" && command.Count == 4) Console.WriteLine(Character.DeleteCharacter(command[2], Convert.ToInt32(command[3])));
                    else Console.WriteLine("Invalid Syntax! Usage: /delete character <username> <charindex>");
                    break;
                case "/clear":
                    Console.Clear();
                    break;
                default:
                    Console.WriteLine("Unknown Command: " + command[0]);
                    break;
            }
            OnCommand();
        }
        private static void KickPlayer(Player player, string msg)
        {
            SendMessage(player, "You Have Been Kicked From The Server: " + msg);
            OnDisconnect(player);
        }
        private static void ParsePlayerMessage(Player player, string message)
        {
            Console.WriteLine("Player " + player.ID + ": " + message);
            if (player.Username == null)
            {
                List<string> request = Util.FetchString(message, ":");
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
                            SendMessage(player, "*Welcome");
                            player.Username = request[1];
                            player.Password = request[2];
                        }
                        else KickPlayer(player, "Account Not Found Or Wrong Password");
                        break;
                    case "Register":
                        if (Account.CreateAccount(request[1], request[2])) SendMessage(player, "Account Created!");
                        else KickPlayer(player, "Account Exists!");
                        break;
                }
                return;
            }

            if (message.Substring(0, 1) == "/")
            {
                List<string> request = Util.FetchString(message.Substring(1, message.Length - 1), " ");
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
            }
            else if (message.Substring(0, 1) == "*")
            {
                List<string> request = Util.FetchString(message.Substring(1, message.Length - 1), " ");
                switch (request[0])
                {
                    case "Create":
                        if (request.Count != 5) return;
                        if (request[1] == "Character") SendMessage(player, "*CCreateResult:" + Character.CreateCharacter(request[2], request[3], request[4]));
                        break;
                    case "Delete":
                        if (request.Count != 4) return;
                        if (request[1] == "Character") Character.DeleteCharacter(request[2], Convert.ToInt16(request[3]));
                        break;
                    case "CharacterList":
                        if (request.Count != 3) return;
                        if (Account.GetAccount(request[1], request[2]) == "Success!") SendMessage(player, "*CList:" + Character.RetrieveShortenedCharacterList(request[1]));
                        break;
                    case "EnterWorld":
                        if (request.Count != 3) return;
                        SendMessage(player, "*CharacterData:" + Character.LoadCharacterAsString(request[1], Convert.ToInt16(request[2])));
                        break;
                }
            }
            else Console.WriteLine(player.Username + ": " + message);
        }
        private static void SendMessage(Player player, string message)
        {
            Console.WriteLine("[Server]: " + message);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
            try { player.Client.GetStream().Write(msg, 0, msg.Length); }
            catch { OnDisconnect(player); }
        }
        private static void SendMessage(List<string> command)
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
        private static void SendMessage(int ID, string message)
        {
            Player player = Player.GetPlayerByID(ID);
            if (player == null)
            {
                Console.WriteLine("Player " + ID + " Not Found!");
                return;
            }
            SendMessage(player, message);
        }
        private static void SendMessage(string Username, string message)
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