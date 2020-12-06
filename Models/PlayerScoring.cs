namespace BoardSquares.Models
{
    public class PlayerScoring
    {
        public int RoundID { get; set; }
        public string RoundName { get; set; }
        public int PlayerID { get; set; }
        public string PlayerTeam { get; set; }
        public string PlayerPosition { get; set; }
        public string PlayerFirstName { get; set; }
        public string PlayerLastName { get; set; }
        public int ScoringEventID { get; set; }
        public string EventName { get; set; }
        public int YardsOrCount { get; set; }
        public int Points { get; set; }
    }
}