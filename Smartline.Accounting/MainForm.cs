using System;
using System.Windows.Forms;

namespace Smartline.Accounting {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e) {
            var accountingController = new AccountingController();
            accountingController.Start();
        }
    }
}
