using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BoardSquares.Models
{
    public class News
    {
        [Key]
        public int NewsID { get; set; }

        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}