using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardSquares.Models
{
    [Table("FP_EventLogging")]
    public class EventLog
    {
        [Key]
        [Column("LoggingID")]
        public int LoggingId { get; set; }

        [Column("UserID")]
        public int UserId { get; set; }

        public string EventDescription { get; set; }
 
        public string EventSource { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}