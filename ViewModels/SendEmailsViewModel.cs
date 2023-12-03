using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using BoardSquares.Models;

namespace BoardSquares.ViewModels
{
    public class SendEmailsViewModel
    {
        public Dictionary<string, string> RoundsDictionary { get; set; }
        [Display(Name = "Message Type")]
        public string SelectedRound { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public bool IsValid { get; set; }

        public SendEmailsViewModel()
        {
            RoundsDictionary = new Dictionary<string, string>();
            SelectedRound = "";
            ErrorMessage = "";
            SuccessMessage = "";
            IsValid = true;
            GetRounds();
        }
        
        private void GetRounds()
        {
            var rounds = new Dictionary<string, string>();
            rounds.Add("4", "Weekly Recap - Wild Card");
            rounds.Add("5", "Weekly Recap - Divisional");
            rounds.Add("6", "Weekly Recap - Conference");
            rounds.Add("7", "Final Results");
            RoundsDictionary = rounds;
        }

        public void HandleRequest()
        {
            var db = new BoardSquaresRepository();
            var messages = db.AttemptGetEmails(Convert.ToInt32(SelectedRound));
            if (!messages.Any())
            {
                ErrorMessage = "Emails already sent for selcted week";
                IsValid = false;
                return;
            }
            foreach (var message in messages)
            {
                var mailClient = new SmtpClient();
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("admin@boardsquares.com"),
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(new MailAddress(message.Email));
                mailClient.Send(mailMessage);
                mailClient.Dispose();
            }
            db.AttemptMarkEmailsSent(messages.FirstOrDefault().BatchID);
            SuccessMessage = messages.Count + " Messages Sent!";
        }
    }
}