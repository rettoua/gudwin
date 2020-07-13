using System;
using System.Windows.Forms;

namespace Smartline.Compacting {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void OnStartButtonClick(object sender, EventArgs e) {
            var reportingController = new CompactingController();
            reportingController.Start();
        }
    }
}