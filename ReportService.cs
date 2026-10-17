using EmailSender;
using ReportService.Models.Domains;
using ReportService.Repositories;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;

namespace ReportService
{
    public partial class ReportService : ServiceBase
    {
        private static readonly NLog.Logger Logger =
            NLog.LogManager.GetCurrentClassLogger();
        private readonly bool _sendReports = SendReports;
        private readonly int _sendHour = SendHour;
        private readonly int _intervalInMinutes = IntervalInMinutes;
        private readonly Timer _timer;
        private ErrorRepository _errorRepository = new ErrorRepository();
        private ReportRepository _reportRepository = new ReportRepository();
        private Email _email;
        private GenerateHtmlEmail _htmlEmail = new GenerateHtmlEmail();
        private string _emailReceiver; //odbiorca emaili
        private StringCipher _stringCipher = 
            new StringCipher("7EE5345D-D5BB-4F21-96E5-07BAA21CA040");

        public ReportService()
        {
            InitializeComponent();


            try
            {
                _sendReports = bool.Parse
            (ConfigurationManager.AppSettings["SendReports"])
            _sendHour = int.Parse
            (ConfigurationManager.AppSettings["SendHour"])
            _intervalInMinutes = int.Parse
            (ConfigurationManager.AppSettings["IntervalInMinutes"])
            _timer = new Timer(_intervalInMinutes * 60000);

                _emailReceiver = ConfigurationManager.AppSettings["ReciverEmail"];

                var encryptedPassword = ConfigurationManager.AppSettings
                    ["SenderEmailPassword"];

                if (encryptedPassword.StartsWith("encrypt:"))
                {
                    encryptedPassword = _stringCipher
                        .Encrypt(encryptedPassword.Replace("encrypt:", ""));

                    var configFile = ConfigurationManager.OpenExeConfiguration
                        (ConfigurationUserLevel.None);

                    configFile.AppSettings.Settings["SenderEmailPassword"].Value =
                        encryptedPassword;
                    configFile.Save();
                }

                _email = new Email(new EmailParams
                {
                    HostSmtp = ConfigurationManager.AppSettings["HostSmtp"],
                    Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]),
                    EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]),
                    SenderName = ConfigurationManager.AppSettings["SenderName"],
                    SenderEmail = ConfigurationManager.AppSettings["SenderEmail"], //adres email
                    SenderEmailPassword =  DecryptSenderEmailPassword() //haslo do emaila z ktorego wysylamy raport
                });
            }
            catch(Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private string DecryptSenderEmailPassword()
        {
            var encryptedPassword = ConfigurationManager.AppSettings
                    ["SenderEmailPassword"];

            if (encryptedPassword.StartsWith("encrypt:"))
            {
                encryptedPassword = _stringCipher
                    .Encrypt(encryptedPassword.Replace("encrypt:", ""));

                var configFile = ConfigurationManager.OpenExeConfiguration
                    (ConfigurationUserLevel.None);

                configFile.AppSettings.Settings["SenderEmailPassword"].Value =
                    encryptedPassword;
                configFile.Save();
            }

            return _stringCipher.Decrypt(encryptedPassword);
        }

        protected override void OnStart(string[] args)
        {
            _timer.Elapsed += DoWork;
            _timer.Start();
            Logger.Info("Service started...");
        }

        private async void DoWork(object sender, ElapsedEventArgs e)
        {
            try
            {
                await SendError();
                await SendReport();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }
        private async Task SendError()
        {
            var errors = _errorRepository.GetLastErrors(_intervalInMinutes);

            if (errors == null || !errors.Any())
                return;

           await _email.Send("Błędy w aplikacji", _htmlEmail.GenerateErrors(errors,
                _intervalInMinutes), _emailReceiver);

            Logger.Info("Error sent...");
        }
        private async Task SendReport()
        {
            if (!_sendReports)
                return;

            var actualHour = DateTime.Now.Hour;

            if (actualHour < _sendHour)
                return;

            var report = _reportRepository.GetLastNotSendReport();
            if (report == null)
                return;

            await _email.Send("Raport dobowy", _htmlEmail.GenerateReport(report), 
                _emailReceiver);
            _reportRepository.ReportSent(report);

            Logger.Info("Report sent");

        }

        protected override void OnStop()
        {
            Logger.Info("Service stopped...");
        }
    }
}
