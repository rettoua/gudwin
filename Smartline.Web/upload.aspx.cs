using System;
using System.IO;
using System.Web;
using Ext.Net;

namespace Smartline.Web {
    public partial class upload : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {

        }

        protected void UploadClick(object sender, DirectEventArgs e) {
            if (!FileUploadField1.HasFile) {
                X.Msg.Alert("Ошибка загрузки файла", "Файл для загрузки не выбран.").Show();
                return;
            }
            FileUploadField1.Text = Path.GetFileName(FileUploadField1.PostedFile.FileName);
            Stream stream = FileUploadField1.PostedFile.InputStream;
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            File.WriteAllBytes(HttpContext.Current.Server.MapPath("~") + GetFileName(), buffer);
            X.Msg.Alert("Загрузка", "Файл успешно загружен.").Show();
        }

        private string GetFileName() {
            if (string.IsNullOrWhiteSpace(txtFileName.Text)) {
                return "uploaded.hex";
            }
            return txtFileName.Text;
        }
    }
}