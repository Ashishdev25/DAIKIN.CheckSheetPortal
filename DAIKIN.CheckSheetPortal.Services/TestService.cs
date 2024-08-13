using AutoMapper;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class TestService : ITestService
    {
        private readonly IOptions<AppSettings> _options;
        private readonly ILogger _logger;
        private readonly IMasterSettingsService _masterSettingsService;
        public TestService(IMapper mapper, ICacheService cache, IOptions<AppSettings> options, 
            ILogger<CheckSheetVersionService> logger, IMasterSettingsService masterSettingsService)
        {
            _options = options;
            _logger = logger;
            _masterSettingsService = masterSettingsService;
        }
        public bool SendMail(string subject, string body, string env, string emailTo, string emailCC)
        {
            var settings = ((OperationResponse<MasterSettingsDTO>)(_masterSettingsService.GetSettingsAsync().Result)).Payload;
            int maxRetries = 3;
            int retryDelay = 5000;
            bool sentSuccessfully = false;
            int retries = 0;

            while (!sentSuccessfully && retries < maxRetries)
            {
                try
                {
                    var message = new MailMessage();
                    var smtpClient = new SmtpClient();
                    var userid = settings.SMTPUserId;
                    var password = settings.SMTPPassword;
                    var fromAddress = new MailAddress(settings.SenderEmailAddress, $"[{env}] Digital Check Sheet");
                    message.From = fromAddress;
                    message.To.Add(emailTo);
                    message.CC.Add(emailCC);
                    message.Subject = subject;
                    message.IsBodyHtml = true;
                    message.Body = body;
                    smtpClient.Host = settings.SMTPHost;
                    smtpClient.Port = settings.SMTPPort;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(userid, password);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = settings.SMTPEnableSSL;
                    smtpClient.Timeout = 30000;
                    smtpClient.Send(message);
                    sentSuccessfully = true;
                    _logger.LogInformation("SendMail:Successful");
                }
                catch (TimeoutException)
                {
                    if (retries < maxRetries - 1)
                    {
                        retries++;
                        _logger.LogInformation($"SendMail:Timeout occurred. Retrying attempt {retries + 1} in {retryDelay / 1000} seconds...");
                        System.Threading.Thread.Sleep(retryDelay);
                    }
                    else
                    {
                        retries = 5;
                        _logger.LogError("SendMail:Failed to send email: Maximum retries exceeded.");
                    }
                }
                catch (SmtpException ex)
                {
                    retries = 5;
                    _logger.LogError(ex, "SendMail:Failed to send email: " + ex.Message);
                }
                catch (Exception ex)
                {
                    retries = 5;
                    _logger.LogError(ex, $"SendMail - TO: {emailTo} CC: {emailCC}");
                }
            }
            return sentSuccessfully;
        }
    }
}
