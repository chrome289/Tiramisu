using AE.Net.Mail;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Tiramisu {
    /// <summary>
    /// Interaction logic for mail.xaml
    /// </summary>
    public class resulr {

        public PlusService service;
        public UserCredential credential;
    }
    public partial class mail : UserControl {
        private static ImapClient imapClient;
        public static resulr result = new resulr();
        public static AE.Net.Mail.MailMessage[] mm;
        public static void login() {
            result.credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                   new ClientSecrets
                                   {
                                       ClientId = "316924129116-efm2iopj3strkrpbjkf7oeqmptn8ph27.apps.googleusercontent.com",
                                       ClientSecret = "MAxnno9YGxGTdtNiEqItt8-n"
                                   },
                                   new[] { "https://mail.google.com/ email" },
                                   "Tiramisu",
                                   CancellationToken.None,
                                   new FileDataStore("Analytics.Auth.Store")).Result;

            if (result.credential != null) {
                result.service = new PlusService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = result.credential,
                    ApplicationName = "Google mail",
                });

                Google.Apis.Plus.v1.Data.Person me = result.service.People.Get("me").Execute();
                Google.Apis.Plus.v1.Data.Person.EmailsData myAccountEmail = me.Emails.Where(a => a.Type == "account").FirstOrDefault();

                imapClient = new ImapClient("imap.gmail.com", myAccountEmail.Value, result.credential.Token.AccessToken, AuthMethods.SaslOAuth, 993, true);
                //MessageBox.Show(" " + imapClient.GetMessageCount());

            }
            else {
                MessageBox.Show("Result.credential is null");
            }
        }

        public mail() {
            InitializeComponent();
        }

        private void bt1_Click(object sender, RoutedEventArgs e) {

            login();
            MessageBox.Show(" " + imapClient.IsConnected);
            imapClient.SelectMailbox("INBOX");
            mm = imapClient.GetMessages(0, 19);
            int i = 0;
            foreach (AE.Net.Mail.MailMessage m in mm) {
                i++;
                dataGrid.Items.Add(new { Col1 = i, Col2 = m.Date.ToString(), Col3=m.From.User ,Col4 = m.Subject });
            }
        }

        
        private void datagridOnSelected(object sender, RoutedEventArgs e) {
            readMail rM = new readMail();
            AE.Net.Mail.MailMessage temp = imapClient.GetMessage(dataGrid.SelectedIndex,false);
            rM.lb1.Content += temp.Date.ToString();
            rM.lb2.Content += temp.Subject;
            rM.lb3.Content += temp.From.User+" <"+temp.From.Address+"> ";
            var uri = new Uri("data:text/html,"+temp.Body, UriKind.Absolute);

            rM.wb.Source = uri; 
            rM.Show();
        }
    }
}
