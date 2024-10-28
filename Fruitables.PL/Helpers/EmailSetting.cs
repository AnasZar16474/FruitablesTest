using Fruitables.DAL.Models;
using System.Net;
using System.Net.Mail;

namespace Fruitables.PL.Helpers
{
    public class EmailSetting
    {
        public static void SendEmail(Email email)
        {
            var client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("tariqshreem00@gmail.com", "rphu xuiv pzqt rvvv");
            client.Send("tariqshreem00@gmail.com", email.Recivers, email.Subject, email.Body);
        }
    }
}
