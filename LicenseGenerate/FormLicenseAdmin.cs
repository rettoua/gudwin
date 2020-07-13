using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Smartline.License.Common;
using Smartline.License.PreDefine;

namespace Smartline.License.Generator {
    public partial class FormLicenseAdmin : Form {
        private readonly BindingList<PropertyValue> _licenseProperties = new BindingList<PropertyValue>();

        private readonly OpenFileDialog _oFileDialog = new OpenFileDialog();
        private readonly SaveFileDialog _sFileDialog = new SaveFileDialog();

        public FormLicenseAdmin() {
            InitializeComponent();

            _oFileDialog.AddExtension = _sFileDialog.AddExtension = true;
            _oFileDialog.DefaultExt = _sFileDialog.DefaultExt = "key";
            _oFileDialog.Filter = _sFileDialog.Filter = "Файл лицензии|*.key";

            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = _licenseProperties;
            NewToolStripMenuItemClick(null, null);
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e) {
            if (_oFileDialog.ShowDialog() == DialogResult.OK) {
                LicenseData license;
                LicenseStatus status;
                var licOpen = new LicenseOpen();
                if ((status = licOpen.OpenLicenseFile(_oFileDialog.FileName, out license)) != LicenseStatus.OK) {
                    MessageText.Text = UserDescriptionAttribute.GetStatusText<LicenseStatus>(status);
                    return;
                }
                if (license == null) {
                    MessageText.Text = UserDescriptionAttribute.GetStatusText<LicenseStatus>(LicenseStatus.CorruptData);
                    return;
                }
                MessageText.Text =
                    UserDescriptionAttribute.GetStatusText<LicenseStatus>(licOpen.Check(license, GetProgramId(),
                                                                                         GetProgramVersion()));
                OpenLicense(license);
            } else
                MessageText.Text = "";
        }

        private void SaveToolStripMenuItemClick(object sender, EventArgs e) {
            dataGridView1.EndEdit();

            if (_licenseProperties.Any() == false) {
                MessageText.Text = "Нет данных";
                return;
            }

            var license = new LicenseData();
            PropertyOfClass<LicenseData>.SetValues(_licenseProperties.ToArray(), ref license);

            if (license == null) {
                MessageText.Text = UserDescriptionAttribute.GetStatusText<LicenseStatus>(LicenseStatus.CorruptData);
                return;
            }

            if (_sFileDialog.ShowDialog() == DialogResult.OK) {
                LicenseStatus status;
                if ((status = new LicenseGen().Save(_sFileDialog.FileName, license, KeyPrivate.Key)) != LicenseStatus.OK) {
                    MessageText.Text = UserDescriptionAttribute.GetStatusText<LicenseStatus>(status);
                    return;
                }
                MessageText.Text = "Saved";
            } else
                MessageText.Text = "";
        }

        private void DataGridView1DataError(object sender, DataGridViewDataErrorEventArgs e) {
            // leave as is
        }

        private void NewToolStripMenuItemClick(object sender, EventArgs e) {
            OpenLicense(new LicenseData {
                LicVersion = Application.ProductVersion,
                Created = DateTime.Now,
                Expires = DateTime.Now.AddYears(1),
                ProgramID = GetProgramId(),
                ProgramVersion = GetProgramVersion().ToString()
            });
            MessageText.Text = "Created";
        }

        private Version GetProgramVersion() {
            return typeof(FormLicenseClient).Assembly.GetName().Version;
        }

        private Guid GetProgramId() {
            return
                Guid.Parse(
                    ((GuidAttribute)
                     typeof(FormLicenseClient).Assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value);
        }

        private void OpenLicense(LicenseData value) {
            _licenseProperties.Clear();
            foreach (PropertyValue item in PropertyOfClass<LicenseData>.GetProperties(value))
                _licenseProperties.Add(item);
        }
    }
}