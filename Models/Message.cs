namespace BoardSquares.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Body { get; set; }
        public int BatchID { get; set; }
    }
}