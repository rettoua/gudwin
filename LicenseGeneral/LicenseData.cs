using System;
using System.Reflection;
using System.Security.Cryptography;

namespace Smartline.License.Common {
    public class LicenseData {
        /// <summary>
        /// Цифровая подпись. Шифровать все данные нельзя, ограничение
        /// </summary>
        public byte[] SignedHash;

        /// <summary>
        /// Версия лицензии
        /// </summary>
        private Version _licVersion; // XmlSerializer not work with Version Type

        private Version _programVersion; // XmlSerializer not work with Version Type

        public LicenseData() {
            _licVersion = new Version();
            _programVersion = new Version();
            CompanyName = "";
            Contacts = "";
        }

        [UserDescription("Срок годности файла-ключа лицензии истекает", true)]
        public DateTime Expires { get; set; }

        [UserDescription("Код компьютера")]
        public long PCCode { get; set; }

        [UserDescription("Версия файла-ключа лицензии", true)]
        public string LicVersion {
            get { return _licVersion.ToString(); }
            set { _licVersion = Version.Parse(value); }
        }

        [UserDescription("Дата создания файла-ключа лицензии", true)]
        public DateTime Created { get; set; }

        [UserDescription("Идентификатор программного продукта", true)]
        public Guid ProgramID { get; set; }

        [UserDescription("Версия программного продукта")]
        public string ProgramVersion {
            get { return _programVersion.ToString(); }
            set { _programVersion = Version.Parse(value); }
        }

        [UserDescription("Название организации")]
        public string CompanyName { get; set; }

        [UserDescription("Номер телефона и другая контактная информация")]
        public string Contacts { get; set; }

        public byte[] GetSignHash() {
            string hash = "";
            foreach (PropertyInfo item in GetType().GetProperties()) {
                hash += item.GetValue(this, null).ToString();
            }
            var md5 = new MD5CryptoServiceProvider();
            byte[] hashBytes = GetBytes(hash);
            return md5.ComputeHash(hashBytes, 0, hashBytes.Length);
        }

        private byte[] GetBytes(string str) {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}