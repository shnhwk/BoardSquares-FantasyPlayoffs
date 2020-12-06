using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardSquares.Models
{
    public class ScoringPoints
    {
        [Key]
        public int ID { get; set; }
        public int PlayerID { get; set; }
        public int ScoringEventID { get; set; }
        public int RoundID { get; set; }
        public int Value { get; set; }
        public int Points { get; set; }
        public int Year { get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public string ScoringEventName { get; set; }
        [NotMapped]
        public string RoundName { get; set; }
    }
}