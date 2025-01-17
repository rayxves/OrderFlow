using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using OrderAPI.Mappers;

namespace OrderAPI.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public string GenerateEmailHtml(string userName, int orderId, DateTime orderDate, string paymentStatus, decimal totalAmount, string deliveryStatus, DateTime deliveryDate)
        {
            string headerColor = paymentStatus == "Aprovado" ? "#2ecc71" : "#e74c3c";
            string headerText = paymentStatus == "Aprovado" ? "Pagamento Aprovado!" : "Pagamento Recusado!";
            string bodyMessage = paymentStatus == "Aprovado"
                ? "Seu pagamento foi aprovado e o pedido está sendo processado."
                : "Infelizmente, o pagamento do seu pedido foi recusado. Por favor, entre em contato para resolver o problema.";
            string deliveryInfo = paymentStatus == "Aprovado"
                ? $@"<p><strong>Status da Entrega:</strong> {deliveryStatus}</p>
             <p><strong>Data Estimada de Entrega:</strong> {deliveryDate:dd/MM/yyyy}</p>"
                : "";

            return $@"<html>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; background-color: #f9f9f9; margin: 0; padding: 20px;'>
            <div style='max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);'>
                <!-- Cabeçalho -->
                <div style='background-color: {headerColor}; color: #ffffff; padding: 20px; border-radius: 8px 8px 0 0; text-align: center;'>
                    <h2 style='margin: 0;'>{headerText}</h2>
                </div>
                <!-- Corpo do Email -->
                <div style='padding: 20px;'>
                    <p><strong>Olá, {userName}!</strong></p>
                    <p>{bodyMessage}</p>
                    <p><strong>ID do Pedido:</strong> {orderId}</p>
                    <p><strong>Data do Pedido:</strong> {orderDate:dd/MM/yyyy}</p>
                    <p><strong>Total:</strong> R$ {totalAmount:F2}</p>
                    {deliveryInfo}
                    <br />
                    <p style='color: #555555; font-size: 14px;'>Atenciosamente,</p>
                    <p><strong>OrderAPI</strong></p>
                </div>
                <!-- Rodapé -->
                <div style='background-color: #f4f4f4; color: #777777; padding: 10px; text-align: center; font-size: 12px; border-radius: 0 0 8px 8px;'>
                    <p>© 2025 OrderAPI. Todos os direitos reservados.</p>
                </div>
            </div>
        </body>
        </html>";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.SmtpPort,
                Credentials = new NetworkCredential(_emailSettings.EmailFrom, _emailSettings.EmailPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.EmailFrom),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"E-mail enviado com sucesso para {toEmail}");
            }
            catch (SmtpFailedRecipientException ex)
            {
                Console.WriteLine($"Falha ao enviar o e-mail para {ex.FailedRecipient}: {ex.Message}");
                throw new Exception($"Falha no envio do e-mail para o destinatário {ex.FailedRecipient}", ex);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine("Erro SMTP: " + ex.Message);
                Console.WriteLine("Código de status: " + ex.StatusCode);

                if (ex.StatusCode == SmtpStatusCode.GeneralFailure)
                {
                    throw new Exception("Falha na conexão SMTP. Verifique o servidor e as credenciais.", ex);
                }
                else if (ex.StatusCode == SmtpStatusCode.MailboxUnavailable)
                {
                    throw new Exception("A caixa de e-mail do destinatário está indisponível.", ex);
                }
                else if (ex.StatusCode == SmtpStatusCode.ClientNotPermitted)
                {
                    throw new Exception("O cliente não tem permissão para enviar e-mails. Verifique as configurações do servidor SMTP.", ex);
                }
                else
                {
                    throw new Exception("Ocorreu um erro SMTP ao enviar o e-mail.", ex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro inesperado ao enviar o e-mail: " + ex.Message);
                throw new Exception("Erro inesperado ao enviar o e-mail.", ex);
            }
        }
    }
}
