using System;
using System.Windows.Forms;

namespace Smartline.Reporting {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void Button1Click(object sender, EventArgs e) {
            var reportingController = new ReportingController();
            reportingController.Start();
        }
    }
}
