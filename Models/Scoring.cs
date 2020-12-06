using System.ComponentModel.DataAnnotations;

namespace BoardSquares.Models
{
    public class Scoring
    {
        [Key]
        public int ID { get; set; }
        public string EventName { get; set; }
        public bool OnlyDEF { get; set; }
        public string EventCategory { get; set; }
    }
}