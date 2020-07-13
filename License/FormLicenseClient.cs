using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Smartline.License.Common;
using Smartline.License.Communication;

namespace Smartline.License.PreDefine {
    public partial class FormLicenseClient : Form {
        private readonly BindingList<PropertyValue> _licenseProperties = new BindingList<PropertyValue>();

        public FormLicenseClient() {
            InitializeComponent();

            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = _licenseProperties;
        }

        private void FormLicenseLoad(object sender, EventArgs e) {
            var templateLicense = new LicenseData {
                PCCode = ComputerInfo.GetCodePc(),
                ProgramVersion = Application.ProductVersion
            };
            foreach (PropertyValue item in PropertyOfClass<LicenseData>.GetProperties(templateLicense).
                Where(a => a.ShowOnlyAdmin == false))
                _licenseProperties.Add(item);
        }

        private void DataGridView1DataError(object sender, DataGridViewDataErrorEventArgs e) {
            // leave as is            
        }

        private void ButtonSendClick(object sender, EventArgs e) {
            dataGridView1.EndEdit();

            if (_licenseProperties.Any() == false) {
                MessageBox.Show("Заполните поля", "Заполните поля", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            var license = new LicenseData();
            PropertyOfClass<LicenseData>.SetValues(_licenseProperties.ToArray(), ref license);

            if (license == null) {
                MessageBox.Show("Произошла ошибка, проверьте данные и повторите попытку", "Произошла ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            license.ProgramID =
                new Guid(
                    ((GuidAttribute)
                     Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value);

            string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".key");
            if (new LicenseGen().Save(fileName, license) == LicenseStatus.OK)
                new Mail().SendWithAttachment("Запрос на получение лицензии", "См. вложение", fileName);
        }
    }
}