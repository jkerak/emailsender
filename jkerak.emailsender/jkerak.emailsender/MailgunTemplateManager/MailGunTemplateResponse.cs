using System.Collections.Generic;

namespace jkerak.emailsender.MailgunTemplateManager
{
    public class Item
    {
        public string createdAt { get; set; }
        public string description { get; set; }
        public string name { get; set; }
    }

    public class Paging
    {
        public string first { get; set; }
        public string last { get; set; }
        public string next { get; set; }
        public string prev { get; set; }
    }

    public class MailGunTemplateResponse
    {
        public List<Item> items { get; set; }
        public Paging paging { get; set; }
    }
}