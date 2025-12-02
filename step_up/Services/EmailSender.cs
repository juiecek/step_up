//using Microsoft.AspNetCore.Identity.UI.Services;
//using MimeKit;
//using MailKit.Net.Smtp;
//using MailKit.Security;
//using System.Threading.Tasks;

//public class EmailSender : IEmailSender
//{
//    private readonly IConfiguration _configuration;

//    public EmailSender(IConfiguration configuration)
//    {
//        _configuration = configuration;
//    }

//    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
//    {
//        var emailSettings = _configuration.GetSection("EmailSettings");

//        var message = new MimeMessage();

//        // Вместо одного аргумента добавляем имя отправителя и email
//        message.From.Add(new MailboxAddress("NoReply", emailSettings["FromAddress"]));  // 'NoReply' — это имя, которое будет отображаться
//        message.To.Add(new MailboxAddress("", email));  // Пустое имя для получателя
//        message.Subject = subject;

//        var bodyBuilder = new BodyBuilder
//        {
//            HtmlBody = htmlMessage
//        };
//        message.Body = bodyBuilder.ToMessageBody();

//        using (var client = new SmtpClient())
//        {
//            // Подключаемся к SMTP-серверу
//            await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), SecureSocketOptions.StartTls);
//            await client.AuthenticateAsync(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);

//            // Отправляем сообщение
//            await client.SendAsync(message);

//            await client.DisconnectAsync(true);
//        }
//    }
//}
