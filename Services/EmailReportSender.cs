using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailReportSender : IReportSender
{
    public async Task SendAsync(string content)
    {
        using var client = new SmtpClient("smtp.server.com")
        {
            Port = 587,
            EnableSsl = true,
            Credentials = new NetworkCredential("user", "password")
        };

        using var msg = new MailMessage("from@example.com", "to@example.com")
        {
            Subject = "Process Report (Boot Delivery)",
            Body = content
        };

        await client.SendMailAsync(msg);
    }
}
