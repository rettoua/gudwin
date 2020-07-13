using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace Smartline.License.Common {
    public class LicenseGen {
        public LicenseStatus Save(string fileName, LicenseData license, string privateKey = null) {
            if (license == null)
                return LicenseStatus.CorruptData;

            if (string.IsNullOrEmpty(privateKey) == false) {
                if (SignHash(ref license, privateKey) == false)
                    return LicenseStatus.ErrSign;
            }

            byte[] serializedLicense = Serialize(license);
            if (serializedLicense == null)
                return LicenseStatus.CorruptData;

            if (SaveFile(fileName, serializedLicense) == false)
                return LicenseStatus.ErrFile;

            return LicenseStatus.OK;
        }

        private bool SaveFile(string fileName, byte[] serializedLicense) {
            try {
                File.WriteAllBytes(fileName, serializedLicense);
                return true;
            } catch {
            }
            return false;
        }

        private bool SignHash(ref LicenseData license, string keyPrivate) {
            using (var RSA = new RSACryptoServiceProvider()) {
                try {
                    RSA.FromXmlString(keyPrivate);
                    var rsaFormatter = new RSAPKCS1SignatureFormatter(RSA);
                    rsaFormatter.SetHashAlgorithm("MD5");
                    license.SignedHash = rsaFormatter.CreateSignature(license.GetSignHash());
                    return true;
                } catch {
                }
            }
            return false;
        }

        private byte[] Serialize(LicenseData license) {
            using (var stream = new MemoryStream()) {
                try {
                    var xmlSer = new XmlSerializer(typeof(LicenseData));
                    xmlSer.Serialize(XmlWriter.Create(stream), license);
                    stream.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream.ToArray();
                } catch {
                }
            }
            return null;
        }
    }
}