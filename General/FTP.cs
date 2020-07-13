using System;
using System.IO;
using System.Net;
using System.Text;

namespace Smartline.License.Communication {
    public class FTP {
        private static string _login = "";
        private static string _password = "";
        private static string _ftpAddressRoot = "";

        /// <summary>
        /// Размер загружаемого файла определен. Если -1, то размер определить нельзя
        /// </summary>
        public Action<long> DefDataLength;

        /// <summary>
        /// Загрузка завершена
        /// </summary>
        public Action<int> DownloadCompleate;

        /// <summary>
        /// Загрузка прервана по причине возникновения исключения
        /// </summary>
        public Action<Exception> DownloadError;

        /// <summary>
        /// Процесс загрузки, процент загруженного
        /// </summary>
        public Action<int> DownloadProgress;

        /// <summary>
        /// Work with FTP server
        /// </summary>
        /// <param name="login">Login (encoded DES): null - default, "" - anonym</param>
        /// <param name="Password">Password (encoded DES) on FTP account</param>
        /// <param name="FTPAddressRoot">FTP root address (encoded DES)</param>        
        public FTP(string login = null, string password = null, string ftpAddressRoot = null) {
            if (login != null) {
                _login = login;
                _password = password;
            }
            if (ftpAddressRoot != null) {
                _ftpAddressRoot = ftpAddressRoot;
            }
        }

        /// <summary>
        /// Загружает указанный файл в память
        /// </summary>
        /// <param name="ftpAddress">Путь к файлу (без root)</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Значение сохраненного потока в памяти</returns>
        public string DownloadFile(string ftpAddress, string fileName) {
            return downloadFile(ftpAddress, fileName, false);
        }

        /// <summary>
        /// Загружает указанный файл на диск
        /// </summary>
        /// <param name="ftpAddress">Путь к файлу (без root)</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="filePathSave">Путь для сохранения загруженного файла</param>
        /// <param name="fileNameNew">Новое имя файла для загружаемого файла</param>
        public void DownloadFile(string ftpAddress, string fileName, string filePathSave, string fileNameNew = null) {
            downloadFile(ftpAddress, fileName, true, filePathSave, fileNameNew);
        }

        /// <summary>
        /// Загружает указанный файл в заданый поток
        /// </summary>
        /// <param name="ftpAddress">Путь к файлу</param>
        /// <param name="filename">Имя файла</param>
        /// <param name="streamFile">Сохранять файл в поток - файл на диске, если нет, то в память</param>
        /// <param name="fileNameNew">Новое имя файла для загружаемого файла</param>
        /// <param name="filePathSave">Путь для загрузки нового файла</param>
        /// <returns>Значение сохраненного потока в памяти</returns>
        private string downloadFile(string ftpAddress, string filename, bool streamFile, string filePathSave = null,
                                    string fileNameNew = null) {
            //Create FTP request
            //Note: format is ftp://server.com/file.ext
            var request = WebRequest.Create(_ftpAddressRoot + "/" + ftpAddress + "/" + filename) as FtpWebRequest;

            //Get the file size first (for progress bar)
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(_login, _password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true; //don't close the connection

            long dataLength = -1;
            try {
                dataLength = request.GetResponse().ContentLength;
            } catch {
            }
            if (DefDataLength != null)
                DefDataLength(dataLength);

            //Now get the actual data
            request = WebRequest.Create(_ftpAddressRoot + "/" + ftpAddress + "/" + filename) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_login, _password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false; //close the connection when done

            //Streams
            FtpWebResponse response = null;
            try {
                response = request.GetResponse() as FtpWebResponse;
            } catch (Exception ex) {
                if (DownloadError != null)
                    DownloadError(ex);
                return "";
            }

            Stream reader = response.GetResponseStream();
            //Download to memory
            //Note: adjust the streams here to download directly to the hard drive
            Stream memStream;
            if (streamFile) {
                memStream = new FileStream(Path.Combine(filePathSave, // Путь к файлу
                                                        (fileNameNew ?? filename)), // имя файла
                                           FileMode.Create); // метод создания файла
            } else
                memStream = new MemoryStream();
            var buffer = new byte[10240]; //downloads in chuncks

            long bytesRead = 0;
            while (true) {
                //Try to read the data
                int byteRead;
                try {
                    byteRead = reader.Read(buffer, 0, buffer.Length);
                } catch (Exception ex) {
                    if (DownloadError != null)
                        DownloadError(ex);
                    return "";
                }

                if (byteRead == 0) {
                    break;
                }
                //Write the downloaded data
                memStream.Write(buffer, 0, byteRead);
                if (dataLength > 0) {
                    if ((DownloadProgress != null)) {
                        bytesRead += byteRead;
                        DownloadProgress(Convert.ToInt32(Math.Round(Convert.ToDecimal(100 / dataLength * bytesRead), 0)));
                    }
                }
            }

            memStream.Position = 0;
            string tReturn = "";
            if (!streamFile)
                tReturn = new StreamReader(memStream, Encoding.Default, true).ReadToEnd();

            try {
                reader.Close();
            } catch {
            }
            try {
                memStream.Close();
            } catch {
            }
            try {
                response.Close();
            } catch {
            }

            //Nothing was read, finished downloading
            if (DownloadCompleate != null)
                DownloadCompleate(100);
            return tReturn;
        }

        /// <summary>
        /// for license generator
        /// </summary>
        /// <param name="filePath">to sending file</param>
        /// <param name="pathTarget">to target file path</param>
        /// <returns></returns>
        public bool SendFile(string filePath, string pathTarget) {
            if (File.Exists(filePath)) {
                var fileInf = new FileInfo(filePath);

                // Create FtpWebRequest object from the Uri provided
                var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(pathTarget));
                // Provide the WebPermission Credintials
                reqFtp.Credentials = new NetworkCredential(_login, _password);
                // By default KeepAlive is true, where the control connection is 
                // not closed after a command is executed.
                reqFtp.KeepAlive = false;
                // Specify the command to be executed.
                reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
                // Specify the data transfer type.
                reqFtp.UseBinary = true;
                // Notify the server about the size of the uploaded file
                reqFtp.ContentLength = fileInf.Length;
                // The buffer size is set to 2kb
                const int buffLength = 2048;
                var buff = new byte[buffLength];
                // Opens a file stream (System.IO.FileStream) to read 
                //the file to be uploaded
                FileStream fs = fileInf.OpenRead();
                // Stream to which the file to be upload is written
                Stream strm = reqFtp.GetRequestStream();

                try {
                    // Read from the file stream 2kb at a time
                    int contentLen = fs.Read(buff, 0, buffLength);
                    // Till Stream content ends
                    while (contentLen != 0) {
                        // Write Content from the file stream to the 
                        // FTP Upload Stream
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }
                } catch {
                    return false;
                }

                // Close the file stream and the Request Stream
                try {
                    fs.Close();
                } catch {
                }
                try {
                    strm.Close();
                } catch {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}