namespace BoardSquares.Models
{
    public class PlayerScoringSummary
    {
        public int PlayerID { get; set; }
        public string PlayerPosition { get; set; }
        public string PlayerFirstName { get; set; }
        public string PlayerLastName { get; set; }
        public string PlayerTeam { get; set; }
        public int WildCard { get; set; }
        public int Divisional { get; set; }
        public int Conference { get; set; }
        public int Superbowl { get; set; }
        public int PlayerTotal { get; set; }
        public int TieBreakerSort { get; set; }
    }
}