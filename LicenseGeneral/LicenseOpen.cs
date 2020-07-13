using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace Smartline.License.Common {
    public class LicenseOpen {
        public LicenseStatus Check(string licenseFilePath, Guid programId, Version programVersion) {
            LicenseData license;

            LicenseStatus fileState = OpenLicenseFile(licenseFilePath, out license);
            if (fileState != LicenseStatus.OK)
                return fileState;

            return Check(license, programId, programVersion);
        }

        public LicenseStatus Check(LicenseData license, Guid programId, Version programVersion) {
            if (license == null)
                return LicenseStatus.ErrOther;

            if (license.ProgramID != programId)
                return LicenseStatus.AnotherProgram;

            if (Version.Parse(license.ProgramVersion).Major != programVersion.Major)
                return LicenseStatus.AnotherProgramVersion;

            if (license.Expires <= DateTime.Now)
                return LicenseStatus.LicenseExpired;

            if (license.PCCode != ComputerInfo.GetCodePc())
                return LicenseStatus.AnotherPC;

            if (CheckSighHash(license) == false)
                return LicenseStatus.ErrSign;

            return LicenseStatus.OK;
        }

        public LicenseStatus OpenLicenseFile(string licenseFilePath, out LicenseData license) {
            license = null;

            byte[] encodedLicense = OpenFile(licenseFilePath);
            if (encodedLicense == null)
                return LicenseStatus.ErrFile;

            license = Deserialize(encodedLicense);
            if (license == null)
                return LicenseStatus.CorruptData;

            return LicenseStatus.OK;
        }

        private LicenseData Deserialize(byte[] encodedLicense) {
            using (var memStr = new MemoryStream(encodedLicense)) {
                var xmlSer = new XmlSerializer(typeof(LicenseData));
                try {
                    return (LicenseData)xmlSer.Deserialize(XmlReader.Create(memStr));
                } catch {
                }
            }
            return null;
        }

        private bool CheckSighHash(LicenseData license) {
            using (var RSA = new RSACryptoServiceProvider()) {
                try {
                    RSA.FromXmlString(KeyPublic.Key);
                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(RSA);
                    rsaDeformatter.SetHashAlgorithm("MD5");
                    return rsaDeformatter.VerifySignature(license.GetSignHash(), license.SignedHash);
                } catch {
                }
            }
            return false;
        }

        private byte[] OpenFile(string licenseFilePath) {
            if (File.Exists(licenseFilePath)) {
                try {
                    return File.ReadAllBytes(licenseFilePath);
                } catch {
                }
            }
            return null;
        }
    }
}