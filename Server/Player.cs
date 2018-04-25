using System;
using System.Net.Sockets;

namespace Server
{
    class Player
    {
        public TcpClient Client { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int ID { get; set; }
        public byte[] receivedBuffer = new byte[512];

        public Player(TcpClient _Client)
        {
            Client = _Client;
            FindAnEmptyID();
        }
        public static bool IsPlayerAlreadyInGame(string Username)
        {
            foreach(Player p in Server.Players)
            {
                if (!String.IsNullOrEmpty(p.Username) && p.Username == Username) return true;
            }
            return false;
        }
        public static Player GetPlayerByID(int ID)
        {
            foreach(Player p in Server.Players)
            {
                if (p.ID == ID) return p;
            }
            return null;
        }
        public static Player GetPlayerByUsername(string Username)
        {
            foreach (Player p in Server.Players)
            {
                if (p.Username == Username) return p;
            }
            return null;
        }
        private void FindAnEmptyID()
        {
            int i = 1;
            foreach (Player player in Server.Players)
            {
                if (player.ID != i) break;
                i++;
            }
            ID = i;
        }       
    }
}
