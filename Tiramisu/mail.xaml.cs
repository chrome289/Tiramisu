using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
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
        private static ImapClient imapClient = new ImapClient();
        public static resulr result = new resulr();
        public static MailMessage[] mm;
        public static string mailboxName, lastUid, lastDate;
        private static IMailFolder fol = null;



        public void login() {
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
                imapClient = new MailKit.Net.Imap.ImapClient();
                var credentials = new NetworkCredential(myAccountEmail.Value, result.credential.Token.AccessToken);

                imapClient.Connect("imap.gmail.com", 993, true, CancellationToken.None);
                imapClient.Authenticate(credentials, CancellationToken.None);
            }
        }

        /*private void Sent_OnNewMessagesArrived(object sender, IdleEventArgs e) {
            MessageBox.Show("You got mail"); Dispatcher.Invoke(() => dataGrid.Items.Clear(), DispatcherPriority.Send);
            //Dispatcher.Invoke(() => imapClient.Folders.Find ("[Gmail]/"+mailboxName").EmptyFolder(), DispatcherPriority.Send);

            Dispatcher.Invoke(() => imapClient.Folders.Find("[Gmail]/" + mailboxName).Messages.Download("ALL", ImapX.Enums.MessageFetchMode.Minimal, 5), DispatcherPriority.Send);
            int i = 0;
            foreach (Message m in imapClient.Folders.Find("[Gmail]/" + mailboxName).Messages) {
                i++;
                Dispatcher.Invoke(() => dataGrid.Items.Add(new { Col1 = i, Col2 = m.Date.ToString(), Col3 = m.From.DisplayName, Col4 = m.Subject, Col5 = m.UId }), DispatcherPriority.Send);
            }
        }*/

        public mail() {
            InitializeComponent();
        }

        private async Task exIntialize() {
            await Task.Run(() => intialize());
        }

        private void intialize() {

            if (Properties.Settings.Default.firstUse) {
                pb1.Visibility = Visibility.Visible;
                bt1.IsEnabled = false;
                folder.IsEnabled = false;
                exRefresh();
            }
            else {
                int i = 0;
                try {
                    for (long x = 0; x <= (long)Properties.Settings.Default[lastUid]; x++) {
                        if (File.Exists("D:\\emails\\" + mailboxName + "\\" + x + ".eml")) {
                            Envelope m = new Envelope(); string temp;
                            temp = new StreamReader("D:\\emails\\" + mailboxName + "\\" + x + ".eml").ReadToEnd();
                            Envelope.TryParse(temp, out m);
                            Dispatcher.Invoke(() => dataGrid.Items.Add(new { Col1 = i + 1, Col2 = String.Format("{0:yyyy/MM/dd HH:mm:ss}", m.Date.Value.LocalDateTime).ToString(), Col3 = m.From.ToString().Replace('"', ' '), Col4 = m.Subject, Col5 = temp.Split('\n')[1] }));
                            i++;
                        }
                    }
                }
                catch (Exception e) {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        private void bt1_Click(object sender, RoutedEventArgs e) {
            //dataGrid.Items.Clear();
            pb1.Visibility = Visibility.Visible;
            bt1.IsEnabled = false;
            folder.IsEnabled = false;
            exRefresh();
        }
        private async Task exRefresh() {
            await Task.Run(() => refresh());
            pb1.Visibility = Visibility.Hidden; bt1.IsEnabled = true; folder.IsEnabled = true;
            //imapClient.Folders.Find("[Gmail]/" + mailboxName).OnNewMessagesArrived += Sent_OnNewMessagesArrived;
            //imapClient.Folders.Find("[Gmail]/" + mailboxName).StartIdling();
            Properties.Settings.Default.firstUse = false;
            Properties.Settings.Default.Save();
        }

        private void intializeFolder() {
            var personal = imapClient.GetFolder(imapClient.PersonalNamespaces[0]);
            foreach (var folder in personal.GetSubfolders(false)) {
                if (folder.Name == "[Gmail]") {
                   // MessageBox.Show("fgtythth" + folder.Name);
                    //folder.Open(MailKit.FolderAccess.ReadWrite);
                    foreach (var folder2 in folder.GetSubfolders(false)) {
                        if (folder2.Name == mailboxName) {
                            MessageBox.Show("loco" + folder2.Name);
                            fol = folder2;
                            break;
                        }
                    }
                }
            }
            fol.Open(FolderAccess.ReadWrite);
        }

        private void folder_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                dataGrid.Items.Clear();
            }
            catch { }
            mailboxName = ((ComboBoxItem)folder.SelectedItem).Content.ToString();
            lastUid = "lastDownload" + mailboxName.Replace(" ", "") + "UID";
            lastDate = "lastDownload" + mailboxName.Replace(" ", "") + "Date";
            exIntialize();
        }

        private void refresh() {

            login();
            intializeFolder();
            System.Collections.Generic.IList<IMessageSummary> summaries;
            MessageBox.Show(mailboxName);
            MessageBox.Show("!!" + fol.Count.ToString());

            if (!Directory.Exists(@"D:\emails\" + mailboxName)) {
                Directory.CreateDirectory(@"D:\emails\" + mailboxName);
                int i = 0;
                summaries = fol.Fetch(0, 1000000, MessageSummaryItems.Envelope);
                var summaries2 = fol.Fetch(0, 1000000, MessageSummaryItems.UniqueId);
                StreamWriter sw;
                foreach (var message in summaries) {
                    sw = new StreamWriter("D:\\emails\\" + mailboxName + "\\" + i + ".eml", false);
                    sw.Write(message.Envelope.ToString() + "\n" + summaries2[i].UniqueId.ToString());
                    sw.Close();
                    Dispatcher.Invoke(() => dataGrid.Items.Add(new { Col1 = i + 1, Col2 = String.Format("{0:yyyy/MM/dd HH:mm:ss}", message.Date.LocalDateTime).ToString(), Col3 = message.Envelope.From, Col4 = message.Envelope.Subject, Col5 = summaries2[i].UniqueId.ToString() }), DispatcherPriority.Send);
                    if (i > (long)Properties.Settings.Default[lastUid]) {
                        Properties.Settings.Default[lastUid] = (long)i;
                        Properties.Settings.Default[lastDate] = String.Format("{0:yyyy/MM/dd HH:mm:ss}", message.Date.LocalDateTime).ToString();
                        Properties.Settings.Default.Save();
                    }
                    i++;
                }
            }
            else {
                int i = Convert.ToInt32(Properties.Settings.Default[lastUid]) + 1;
                MessageBox.Show(Properties.Settings.Default[lastDate] + "");
                var query = SearchQuery.DeliveredAfter(DateTime.ParseExact(Properties.Settings.Default[lastDate].ToString(), "yyyy/MM/dd HH:mm:ss", null));
                StreamWriter sw;
                var url = fol.Search(query);
                if (url.Count > 0) {
                    summaries = fol.Fetch(url, MessageSummaryItems.Envelope);
                    var summaries2 = fol.Fetch(0, 1000000, MessageSummaryItems.UniqueId);
                    foreach (var message in summaries) {
                        sw = new StreamWriter("D:\\emails\\" + mailboxName + "\\" + i + ".eml", false);
                        sw.Write(message.Envelope.ToString() + "\n" + summaries2[i].UniqueId.ToString());
                        sw.Close();
                        Dispatcher.Invoke(() => dataGrid.Items.Add(new { Col1 = i + 1, Col2 = String.Format("{0:yyyy/MM/dd HH:mm:ss}", message.Date.LocalDateTime).ToString(), Col3 = message.Envelope.From, Col4 = message.Envelope.Subject, Col5 = summaries2[i].UniqueId.ToString() }), DispatcherPriority.Send);

                        MessageBox.Show(":");
                        if (i > (long)Properties.Settings.Default[lastUid]) {
                            Properties.Settings.Default[lastUid] = (long)i;
                            Properties.Settings.Default[lastDate] = String.Format("{0:yyyy/MM/dd HH:mm:ss}", message.Date.LocalDateTime).ToString();
                            Properties.Settings.Default.Save();
                        }
                        i++;
                    }
                }
            }
        }

        private void datagridOnSelected(object sender, RoutedEventArgs e) {
            if (!imapClient.IsAuthenticated)
                login();
            readMail rM = new readMail();
            MessageBox.Show("ww" + dataGrid.SelectedItem.ToString().Split('=')[5].Substring(1, dataGrid.SelectedItem.ToString().Split('=')[5].Length - 3) + "ww");
            string[] x = { dataGrid.SelectedItem.ToString().Split('=')[5].Substring(1, dataGrid.SelectedItem.ToString().Split('=')[5].Length - 3) };
            intializeFolder();
            var temp = fol.GetMessage(UniqueId.Parse(x[0]));
            rM.Title = temp.Subject;
            rM.lb1.Content += temp.Date.ToString();
            rM.lb2.Content += temp.Subject;
            rM.lb3.Content += temp.From.ToString();
            Uri uri;
            if (temp.HtmlBody != null) {
                StreamWriter sw = new StreamWriter("d:\\temp.html", false); sw.Write(temp.HtmlBody); sw.Close();
                uri = new Uri(@"d:\temp.html", UriKind.Absolute);
            }
            else
                uri = new Uri("data:text/plain," + temp.TextBody, UriKind.Absolute);
            rM.wb.Source = uri;
            rM.Show();
        }
    }
}