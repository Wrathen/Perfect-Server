using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    class Character
    {
        public byte Index;
        public string Name = "";
        public uint Level = Constants.CHARACTER_STARTING_LEVEL;
        public string Class = "Warrior";
        public uint ZoneID = Constants.CHARACTER_STARTING_ZONEID;
        public float CoordinateX = 0;
        public float CoordinateY = 0;
        public float CoordinateZ = 0;
        public float RotationX = 0;
        public float RotationY = 0;
        public float RotationZ = 0;
        public uint Money = Constants.CHARACTER_STARTING_MONEY;
        public List<string> EquipmentIDs;
        public List<string> InventoryIDs;
        public List<string> Stats;

        public Character(string _Index, string _Name, string _Level, string _Class, string _ZoneID, string _CoordinateX, string _CoordinateY, string _CoordinateZ, string _RotationX, string _RotationY, string _RotationZ, string _Money, string _EquipmentIDs, string _InventoryIDs, string _Stats)
        {
            Index = Convert.ToByte(_Index);
            Name = _Name;
            Level = Convert.ToUInt16(_Level);
            Class = _Class;
            ZoneID = Convert.ToUInt16(_ZoneID);
            CoordinateX = float.Parse(_CoordinateX, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            CoordinateY = float.Parse(_CoordinateY, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            CoordinateZ = float.Parse(_CoordinateZ, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            RotationX = float.Parse(_RotationX, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            RotationY = float.Parse(_RotationY, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            RotationZ = float.Parse(_RotationZ, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            Money = Convert.ToUInt32(_Money);
            EquipmentIDs = Util.FetchString(_EquipmentIDs, ",");
            InventoryIDs = Util.FetchString(_InventoryIDs, ",");
            Stats = Util.FetchString(_Stats, ",");
        }
        public Character(int _Index, string _Name, string _Class)
        {
            Index = Convert.ToByte(_Index);
            Name = _Name;
            Class = _Class;
            EquipmentIDs = Util.FetchString(Constants.CHARACTER_STARTING_EQUIPMENTIDS, ",");
            InventoryIDs = Util.FetchString(Constants.CHARACTER_STARTING_INVENTORYIDS, ",");
            Stats = Util.FetchString(Constants.CHARACTER_STARTING_STATS, ",");
        }

        public static string RetrieveShortenedCharacterList(string username)
        {
            List<Character> characterList = new List<Character>();
            string characters = "";

            for (int i = 1; i < Constants.ACCOUNT_MAXCHARACTER + 1; i++)
            {
                if (File.Exists(Constants.PATH_DB + "Characters/" + username + "/" + i + ".txt")) characterList.Add(LoadCharacter(username, i));
            }
            foreach (Character c in characterList)
            {
                characters +=  c.Index + ":" + c.Name + ":" + c.Level + ":" + c.Class + ":" + Util.GetZoneName(c.ZoneID) + "&";
            }
            if (characters.Length > 0) characters = characters.Substring(0, characters.Length - 1);
            return characters;
        }
        public static string RetrieveCharacterList(string username)
        {
            List<Character> characterList = new List<Character>();
            string characters = "";

            for (int i = 1; i < Constants.ACCOUNT_MAXCHARACTER + 1; i++)
            {
                if (File.Exists(Constants.PATH_DB + "Characters/" + username + "/" + i + ".txt")) characterList.Add(LoadCharacter(username, i));
                else break;
            }
            foreach(Character c in characterList)
            {
                characters += DataListToString(AssembleData(c)) + "&";
            }
            if (characters.Length > 0) characters = characters.Substring(0, characters.Length - 1);
            return characters;
        }
        public static string DataListToString(List<string> data)
        {
            string str = "";
            foreach (string s in data) str += s + ":";
            str = str.Substring(0, str.Length - 1);
            return str;
        }
        public static List<string> DataStringToList(string data)
        {
            return Util.FetchString(data, ":");
        }
        public static List<string> AssembleData(Character character)
        {
            List<string> data = new List<string>();
            data.Add(character.Index.ToString());
            data.Add(character.Name);
            data.Add(character.Level.ToString());
            data.Add(character.Class);
            data.Add(character.ZoneID.ToString());
            data.Add(character.CoordinateX.ToString());
            data.Add(character.CoordinateY.ToString());
            data.Add(character.CoordinateZ.ToString());
            data.Add(character.RotationX.ToString());
            data.Add(character.RotationY.ToString());
            data.Add(character.RotationZ.ToString());
            data.Add(character.Money.ToString());
            string IDs = "";
            foreach (string i in character.EquipmentIDs) IDs += i + ",";
            if (IDs.Length > 1) IDs = IDs.Substring(0, IDs.Length - 1); // Remove Last Comma At The End. Ex: 3,4,5,6, needs to be 3,4,5,6
            data.Add(IDs);
            IDs = "";
            foreach (string i in character.InventoryIDs) IDs += i + ",";
            if (IDs.Length > 1) IDs = IDs.Substring(0, IDs.Length - 1); // Remove Last Comma At The End. Ex: 3,4,5,6, needs to be 3,4,5,6
            data.Add(IDs);
            IDs = "";
            foreach (string i in character.Stats) IDs += i + ",";
            if (IDs.Length > 1) IDs = IDs.Substring(0, IDs.Length - 1); // Remove Last Comma At The End. Ex: 3,4,5,6, needs to be 3,4,5,6
            data.Add(IDs);
            return data;
        }
        public static Character DisAssembleData(List<string> data)
        {
            return new Character(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12], data[13], data[14]);
        }
        public static void SaveCharacter(string username, Character character)
        {
            File.WriteAllText(Constants.PATH_DB + "Characters/" + username + "/" + character.Index + ".txt", DataListToString(AssembleData(character)));
        }
        public static Character LoadCharacter(string username, int charIndex)
        {
            return DisAssembleData(DataStringToList(File.ReadAllText(Constants.PATH_DB + "Characters/" + username + "/" + charIndex + ".txt")));
        }
        public static string LoadCharacterAsString(string username, int charIndex)
        {
            return File.ReadAllText(Constants.PATH_DB + "Characters/" + username + "/" + charIndex + ".txt");
        }
        public static string CreateCharacter(string username, string characterName, string characterClass)
        {
            characterName = Util.MakeStringCooler(characterName);
            characterClass = Util.MakeStringCooler(characterClass);

            string[] AllCharacters = File.ReadAllLines(Constants.PATH_DB + "Characters/Characters.txt");
            foreach(string c in AllCharacters)
            {
                if (c == characterName) return "Name Is Not Available!";
            }
            if (!Directory.Exists(Constants.PATH_DB + "Characters/" + username)) return "Account Not Found!";
            for (int i = 1; i < Constants.ACCOUNT_MAXCHARACTER + 1; i++)
            {
                if (!File.Exists(Constants.PATH_DB + "Characters/" + username + "/" + i + ".txt"))
                {
                    File.WriteAllText(Constants.PATH_DB + "Characters/" + username + "/" + i + ".txt", DataListToString(AssembleData(new Character(i, characterName, characterClass))));
                    File.AppendAllLines(Constants.PATH_DB + "Characters/Characters.txt", new string[] { characterName });
                    return "Success!";
                }
            }
            return "You Have No Slot Available!";
        }
        public static string DeleteCharacter(string username, int charIndex)
        {
            string characterName = LoadCharacter(username, charIndex).Name;
            try { File.Delete(Constants.PATH_DB + "Characters/" + username + "/" + charIndex + ".txt"); }
            catch { return "Something Went Wrong!"; }

            string[] OldAllNames = File.ReadAllLines(Constants.PATH_DB + "Characters/Characters.txt");
            List<string> NewAllNames = new List<string>();          

            for (int i = 0; i < OldAllNames.Length; i++) if (OldAllNames[i] != characterName) NewAllNames.Add(OldAllNames[i]);

            File.WriteAllLines(Constants.PATH_DB + "Characters/Characters.txt", NewAllNames);
            return "Success!";
        }
    }
}