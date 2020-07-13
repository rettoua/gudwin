using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace Smartline.License.Communication {
    public class Mail {
        #region Учетная запись для работы с почтой

        private const SmtpDeliveryMethod DeliveryMethod = SmtpDeliveryMethod.Network;
        private const bool EnableSsl = false;
        private const string Host = "mx1.mirohost.net";
        private const int Port = 587;
        private const bool UseDefaultCredentials = false;

        #endregion

        private readonly SmtpClient _smtp;
        private MailMessage _message;
        private Thread _thread;

        public Mail() {
            _smtp = new SmtpClient {
                Host = Host,
                Port = Port,
                EnableSsl = EnableSsl,
                DeliveryMethod = DeliveryMethod,
                UseDefaultCredentials = UseDefaultCredentials,
                Credentials = new NetworkCredential(new MailAddress(Login, Login).Address, Password)
            };
        }

        private static string Login {
            get { return ""; }
        }

        private static string ToMail {
            get { return ""; }
        }

        private static string Password {
            get { return ""; }
        }

        public event Action SendSuccess;
        public event Action<string> SendFailure;

        /// <summary>
        /// Отправка сообщения, используя системного адресата
        /// </summary>
        /// <param name="subject">Тема</param>
        /// <param name="body">Сообщение</param>
        /// <param name="path">Вложение, полный путь к файлу</param>
        public void SendWithAttachments(string subject, string body, string[] path) {
            SendWithAddresseeAttachments(ToMail, subject, body, path);
        }

        /// <summary>
        /// Отправка сообщения, используя системного адресата
        /// </summary>
        /// <param name="subject">Тема</param>
        /// <param name="body">Сообщение</param>
        /// <param name="path">Вложение, полный путь к файлу (null нет файла)</param>
        public void SendWithAttachment(string subject, string body, string path) {
            SendWithAttachments(subject, body, new[] { path });
        }

        /// <summary>
        /// Отправка сообщения, используя системного адресата
        /// </summary>
        /// <param name="subject">Тема</param>
        /// <param name="body">Сообщение</param>
        /// <param name="path">Вложение, полный путь к файлу (null нет файла)</param>
        public void SendWithOutAttachment(string subject, string body) {
            SendWithAttachments(subject, body, null);
        }

        /// <summary>
        /// Отправка сообщения, используя указанного адресата
        /// </summary>
        /// <param name="addressee">Адресат</param>
        /// <param name="subject">Тема</param>
        /// <param name="body">Сообщение</param>
        /// <param name="path">Вложение полный путь к файлу</param>
        public void SendWithAddresseeAttachments(string addressee, string subject, string body, string[] path) {
            _message = new MailMessage(new MailAddress(Login, Login), new MailAddress(addressee, addressee)) {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            Send(_message, path);
        }

        /// <summary>
        /// Отправка сообщения, используя указанного адресата
        /// </summary>
        /// <param name="addressee">Адресат</param>
        /// <param name="subject">Тема</param>
        /// <param name="body">Сообщение</param>
        /// <param name="path">Вложение полный путь к файлу</param>
        public void SendWithAddresseeAttachment(string addressee, string subject, string body, string path) {
            SendWithAddresseeAttachments(addressee, subject, body, new[] { path });
        }

        /// <summary>
        /// Отправка сообщения, используя указанного адресата
        /// </summary>
        /// <param name="toMail">Адресат</param>
        /// <param name="subject">Тема</param>
        /// <param name="body">Сообщение</param>
        /// <param name="path">Вложение полный путь к файлу</param>
        public void SendWithAddressee(string addressee, string subject, string body) {
            SendWithAddresseeAttachments(addressee, subject, body, null);
        }

        private void Send(MailMessage message, IEnumerable<string> path = null) {
            if (path != null) {
                foreach (string item in path) {
                    if (File.Exists(item)) {
                        // Попытка упаковать файл
                        //string compressedFile_path = Tool.ZipPack.Archive_File(item);
                        //SIM_Files.Compress.CAB(item);
                        //if (string.IsNullOrEmpty(compressedFile_path))
                        message.Attachments.Add(new Attachment(item));
                        //else
                        //    message.Attachments.Add(new Attachment(compressedFile_path));
                    }
                }
            }

            _thread = new Thread(SendPrepared);
            _thread.Start();
        }

        public void SendAbort() {
            if (_thread != null) {
                try {
                    _thread.Abort();
                } catch (Exception ex) {
#if DEBUG
                    throw new Exception(ex.Message);
#else 
                    //SIM_Journal.Log_Exception.ToLog(ex);
#endif
                }
            }
        }

        private void SendPrepared() {
            try {
                _smtp.Send(_message);
                _message = null;

                if (SendSuccess != null)
                    SendSuccess();
            } catch (Exception ex) {
                if (!(ex is ThreadAbortException)) {
#if !DEBUG                    
                    //SIM_Journal.Log_Exception.ToLog(ex);
#endif
                    if (SendFailure != null)
                        SendFailure(ex.Message);
                }
            }
        }
    }
}