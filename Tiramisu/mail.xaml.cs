using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using ImapX;
using ImapX.Authentication;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
        public static MailMessage[] mm;
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

                imapClient = new ImapClient("imap.gmail.com", 993, true);
                if (imapClient.Connect()) {

                    var credentials = new OAuth2Credentials(myAccountEmail.Value, result.credential.Token.AccessToken);

                    if (imapClient.Login(credentials)) {
                        MessageBox.Show("snoop d o double g");
                    }

                }
            }
            else {
                MessageBox.Show("Result.credential is null");
            }
        }

        public mail() {
            InitializeComponent();
        }

        private void bt1_Click(object sender, RoutedEventArgs e) {
            pb1.Visibility = Visibility.Visible;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            pb1.Visibility = Visibility.Hidden;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e) {
            login();
            imapClient.Folders.Inbox.Messages.Download("ALL", ImapX.Enums.MessageFetchMode.Minimal, 10);
            int i = 0;
            foreach (Message m in imapClient.Folders.Inbox.Messages) {
                i++;
                Dispatcher.Invoke(() => dataGrid.Items.Add(new { Col1 = i, Col2 = m.Date.ToString(), Col3 = m.From.DisplayName, Col4 = m.Subject }), DispatcherPriority.Send);
            }
        }

        private void datagridOnSelected(object sender, RoutedEventArgs e) {
            readMail rM = new readMail();
            Message temp = imapClient.Folders.Inbox.Messages[dataGrid.SelectedIndex];
            rM.Title = temp.Subject;
            rM.lb1.Content += temp.Date.ToString();
            rM.lb2.Content += temp.Subject;
            rM.lb3.Content += temp.From.DisplayName + " <" + temp.From.Address + "> ";
            //MessageBox.Show("6666" + temp.ContentType + "66666" + "666666");
            temp.Body.Download(); MessageBox.Show(temp.Body.HasText.ToString()); MessageBox.Show(temp.Body.HasHtml.ToString());
            Uri uri;
            if (!temp.Body.HasHtml)
                uri = new Uri("data:text/plain," + temp.Body.Text, UriKind.Absolute);
            else {
                StreamWriter sw = new StreamWriter("d:\\temp.html",false); sw.Write(temp.Body.Html); sw.Close();
                uri = new Uri(@"d:\temp.html", UriKind.Absolute);
            }

            rM.wb.Source = uri;
            rM.Show();
        }
    }
}
