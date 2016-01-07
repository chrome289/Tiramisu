using AE.Net.Mail;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Tiramisu.Pages {
    /// <summary>
    /// Interaction logic for mailCredentials.xaml
    /// </summary>
    public partial class mailCredentials : UserControl {
        public mailCredentials() {
            InitializeComponent();
        }

        private void bt1_Click(object sender, RoutedEventArgs e) {
            mail.login();
        }
    }
}