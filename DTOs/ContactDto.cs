namespace Bookify_API.DTOs
{
    public class ContactDto
    {
        public int id { get; set; }
        public int? providerId { get; set; }
        public string name { get; set; }
        public string specialty { get; set; }
        public string avatar { get; set; }
        public int unread { get; set; }
        public bool isOnline { get; set; }
        public string lastMessage { get; set; }
        public string time { get; set; }
    }
}