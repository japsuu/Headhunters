namespace Headhunters.Networking
{
    public struct PlayerData
    {
        public string Name;

        public bool IsHeadhunter;

        public PlayerData(string name, bool isHeadhunter)
        {
            Name = name;
            IsHeadhunter = isHeadhunter;
        }
    }
}