using System.Collections.Generic;
using System.Linq;

namespace Server
{
    class Util
    {
        public static List<string> FetchString(string str, string seperator)
        {         
            List<string> request = new List<string>();
            if (string.IsNullOrEmpty(str)) return request;
            int iTemp = 0;
            int i = 0;
            for (i = 0; i < str.Length; i++) // for example --> login:user:pass --> returns --> login, user and pass
            {
                if (str.Substring(i, 1) == seperator)
                {
                    request.Add(str.Substring(iTemp, i - iTemp));
                    iTemp = i + 1;
                }
            }
            request.Add(str.Substring(iTemp, i - iTemp));
            return request;
        }
        public static string ByteArrayToString(byte[] array)
        {
            string message = "";
            foreach (byte b in array)
            {
                if (b != 00) message += System.Convert.ToChar(b);
                else break;
            }
            return message;
        }
        public static string MakeStringCooler(string str)
        {
            return str.First().ToString().ToUpperInvariant() + str.Substring(1).ToLowerInvariant();
        }
        public static string GetZoneName(uint id)
        {
            string[] zoneNames = System.IO.File.ReadAllLines(Constants.PATH_DB + "Zones/" + "Zones.txt");
            List<string> zoneData; // zoneData[0] = zoneID and zoneData[1] = zoneName
            foreach(string zone in zoneNames)
            {
                zoneData = FetchString(zone, ":");
                if (System.Convert.ToUInt16(zoneData[0]) == id) return zoneData[1];
            }
            return null;
        }
    }
}
