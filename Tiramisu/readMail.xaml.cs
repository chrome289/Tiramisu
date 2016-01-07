using FirstFloor.ModernUI.Windows.Controls;

namespace Tiramisu {
    /// <summary>
    /// Interaction logic for readMail.xaml
    /// </summary>
    public partial class readMail : ModernWindow {
        public string mailBody,mailSubject,mailDate,mailSender,title="";
        public readMail() {
            InitializeComponent();
        }
    }
}
