using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;


namespace Tikkit_SolpacWeb.Services.Email
{
    public class EmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            // Thiết lập người gửi và người nhận
            emailMessage.From.Add(MailboxAddress.Parse("vnsolpac@gmail.com"));
            emailMessage.To.Add(MailboxAddress.Parse(email));

            // Thiết lập tiêu đề và nội dung email
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            // Kết nối và gửi email thông qua SMTP
            using (var client = new SmtpClient())
            {
                // Kết nối với máy chủ SMTP (thay đổi thông tin máy chủ SMTP và cổng tùy thuộc vào dịch vụ bạn đang sử dụng)
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                // Xác thực tài khoản người gửi (thay đổi thông tin tài khoản và mật khẩu tùy thuộc vào dịch vụ bạn đang sử dụng)
                await client.AuthenticateAsync("vnsolpac@gmail.com", "xytbriqdobdyxdwr");

                // Gửi email
                await client.SendAsync(emailMessage);

                // Ngắt kết nối
                await client.DisconnectAsync(true);
            }
        }
    }
}