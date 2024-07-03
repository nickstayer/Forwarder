
namespace Forwarder
{
    public class Message
    {
        public string Id { get; internal set; }
        public string Subject { get; internal set; }
        public string Body { get; internal set; }
        public string From { get; internal set; }
        public List<string> Attachments { get; internal set; }
    }
}