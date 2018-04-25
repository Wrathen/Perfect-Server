using System.Net;

namespace Server
{
    class Constants
    {
        public static IPAddress SERVER_IP = IPAddress.Parse("127.0.0.1");
        public const int SERVER_PORT = 8080;
        public const int SERVER_MAXPLAYERS = 2;

        public const string PATH_SERVER = "C:/Users/Kullanici/Desktop/Server/";
        public const string PATH_DB = "C:/Users/Kullanici/Desktop/Server/Database/";

        public const int ACCOUNT_MAXCHARACTER = 3;

        public const int CHARACTER_STARTING_LEVEL = 1;
        public const int CHARACTER_STARTING_ZONEID = 1;
        public const int CHARACTER_STARTING_MONEY = 0;
        public const string CHARACTER_STARTING_EQUIPMENTIDS = "";
        public const string CHARACTER_STARTING_INVENTORYIDS = "";
        public const string CHARACTER_STARTING_STATS = "";
    }
}
