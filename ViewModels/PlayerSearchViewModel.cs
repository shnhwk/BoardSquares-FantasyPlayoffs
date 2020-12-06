using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class PlayerSearchViewModel
    {
        public PlayerSearchViewModel()
        {
            Years = Enumerable.Range(DateTime.Now.Year - 5, 11).ToList();
            SelectedYear = DateTime.Now.Year;
            Message = "";
            Teams = new Dictionary<string, string> {{"All", "All"}};
            Players = new List<Player>();
        }

        public List<Player> Players { get; set; }
        public Dictionary<string, string> Teams { get; set; }
        public List<int> Years { get; set; }
        public string Message { get; set; }

        [DisplayName("Year")]
        public int SelectedYear { get; set; }

        [DisplayName("Team")]
        public string SelectedTeam { get; set; }
    }
}