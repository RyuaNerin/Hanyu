using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Security.Cryptography;
using PnPeople.Security;

namespace hanyu
{
    internal class Cert : IDisposable
    {
        public static Cert[] GetCerts()
        {
            var lst = new List<Cert>();

            var paths = new List<string>
            {
                Path.Combine(SystemDirectory.LocalNow, "NPKI")
            };
            foreach (var drive in DriveInfo.GetDrives())
                if (drive.IsReady)
                    paths.Add(Path.Combine(drive.Name, "NPKI"));

            foreach (var dirPath in paths)
                GetCerts(lst, dirPath);
            
            return lst.ToArray();
        }

        private static void GetCerts(IList<Cert> lst, string dirPath)
        {
            if (!Directory.Exists(dirPath))
                return;

            try
            {
                foreach (var ca in Directory.GetDirectories(dirPath))
                {
                    var caPath = Path.Combine(ca, "User");
                    if (!Directory.Exists(caPath))
                        continue;

                    foreach (var user in Directory.GetDirectories(caPath))
                    {
                        try
                        {
                            lst.Add(new Cert(user));
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public Cert(string path)
        {
            this.m_certDir = path;
            this.m_userName = Path.GetFileName(path);
            this.m_caName   = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
            this.m_npkiDir  = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path)));

            var files = Directory.GetFiles(path);
            this.m_certFilePath = files.First(e => Path.GetExtension(e) == ".der");
            this.m_keyFilePath  = files.First(e => Path.GetExtension(e) == ".key");

            try
            {
                this.m_streamCert = new FileStream(this.m_certFilePath, FileMode.Open, FileAccess.ReadWrite);
                this.m_streamKey  = new FileStream(this.m_keyFilePath,  FileMode.Open, FileAccess.ReadWrite);

                this.m_editable = true;
            }
            catch
            {
                try
                {
                    this.m_streamCert = new FileStream(this.m_certFilePath, FileMode.Open, FileAccess.Read);
                    this.m_streamKey  = new FileStream(this.m_keyFilePath,  FileMode.Open, FileAccess.Read);

                    this.m_editable = false;
                }
                catch
                {

                    throw;
                }
            }

            var cert = new X509Certificate2(this.ReadCert());
            var name = cert.Subject;
            var m = Regex.Match(name, @"CN=([^,]+)\(\)[^,]+,");
            if (m.Success)
                name = m.Groups[1].Value;

            this.m_name = name;
            this.m_notBefore = cert.NotBefore;
            this.m_notAfter = cert.NotAfter;
            this.m_drive = Path.GetPathRoot(path);
        }
        ~Cert()
        {
            this.Dispose(false);
            GC.SuppressFinalize(this);
        }
        public void Dispose()
        {
            this.Dispose(true);
        }
        private bool m_disposed = false;
        private void Dispose(bool disposing)
        {
            if (this.m_disposed)
                return;

            this.m_disposed = true;

            this.m_streamCert.Dispose();
            this.m_streamKey.Dispose();
        }

        private readonly string m_certDir;
        private readonly string m_npkiDir;
        private readonly string m_caName;
        private readonly string m_userName;

        private readonly string m_certFilePath;
        private readonly string m_keyFilePath;
        private readonly Stream m_streamCert;
        private readonly Stream m_streamKey;

        private readonly bool m_editable;
        public bool Editable => this.m_editable;

        private readonly string m_name;
        public string Name => this.m_name;

        private readonly DateTime m_notBefore;
        public DateTime NotBefore => this.m_notBefore;

        private readonly DateTime m_notAfter;
        public DateTime NotAfter => this.m_notAfter;

        private readonly string m_drive;
        public string Drive => this.m_drive;

        private byte[] ReadCert()
        {
            var data = new byte[this.m_streamCert.Length];
            this.m_streamCert.Position = 0;
            this.m_streamCert.Read(data, 0, data.Length);

            return data;
        }
        private byte[] ReadKey()
        {
            var data = new byte[this.m_streamKey.Length];
            this.m_streamKey.Position = 0;
            this.m_streamKey.Read(data, 0, data.Length);

            return data;
        }
        private void WriteKey(byte[] data)
        {
            this.m_streamKey.SetLength(0);
            this.m_streamKey.Write(data, 0, data.Length);
        }

        public bool CheckPassword(string password)
        {
            var pkInfo = new PKCS8.EncryptedPrivateKeyInfo(this.ReadKey());

            return PrivateKeyDataDecrypt(password, pkInfo.Algorithm, pkInfo.Salt, pkInfo.IterationCount, pkInfo.EncryptedData) != null;
        }
        public bool ChangePassword(string oldPassword, string newPassword)
        {
            var pkInfo = new PKCS8.EncryptedPrivateKeyInfo(this.ReadKey());

            var privateKey  = PrivateKeyDataDecrypt(oldPassword, pkInfo.Algorithm, pkInfo.Salt, pkInfo.IterationCount, pkInfo.EncryptedData);
            if (privateKey == null)
                return false;
            
            var newSalt = new byte[8];
            var rnd = new Random(DateTime.Now.Millisecond);
            rnd.NextBytes(newSalt);

            pkInfo.Salt = newSalt;
            pkInfo.EncryptedData = PrivateKeyDataEncrypt(newPassword, pkInfo.Algorithm, pkInfo.Salt, pkInfo.IterationCount, privateKey);
            
            this.WriteKey(pkInfo.GetBytes());

            return true;
        }
        public void CopyTo(string newDrive)
        {            
            var localnow = SystemDirectory.LocalNow;
            var newPath = Path.Combine(newDrive == Path.GetPathRoot(localnow) ? localnow : newDrive, "NPKI", this.m_caName, "User", this.m_userName);

            Directory.CreateDirectory(newPath);

            File.Copy(this.m_certFilePath, Path.Combine(newPath, Path.GetFileName(this.m_certFilePath)));
            File.Copy(this.m_keyFilePath, Path.Combine(newPath, Path.GetFileName(this.m_keyFilePath)));
        }

        internal void Remove()
        {
            this.m_streamCert.Dispose();
            this.m_streamKey .Dispose();

            File.Delete(this.m_certFilePath);
            File.Delete(this.m_keyFilePath);

            Directory.Delete(this.m_certDir);

            bool existsCerts = false;
            foreach (var caDir in Directory.GetDirectories(this.m_npkiDir))
            {
                var userDir = Path.Combine(caDir, "User");
                foreach (var certDir in Directory.GetDirectories(userDir))
                {
                    if (Directory.GetFiles(certDir).Length != 0)
                    {
                        existsCerts = true;
                        break;
                    }
                }
                if (existsCerts)
                    break;
            }

            if (!existsCerts)
            {
                Directory.Delete(this.m_npkiDir, true);
            }
        }

        private static byte[] PrivateKeyDataDecrypt(string password, string algorithmOid, byte[] salt, int iterationCount, byte[] encryptedData)
        {
            if (algorithmOid != "1.2.410.200004.1.15") return null;

            try
            {
                return CreateSeed(Encoding.Default.GetBytes(password), salt, iterationCount).Decrypt(encryptedData);
            }
            catch
            {
                return null;
            }            
        }

        private static byte[] PrivateKeyDataEncrypt(string password, string algorithmOid, byte[] salt, int iterationCount, byte[] decryptedData)
        {
            if (algorithmOid != "1.2.410.200004.1.15") return null;

            try
            {
                return CreateSeed(Encoding.Default.GetBytes(password), salt, iterationCount).Encrypt(decryptedData);
            }
            catch
            {
                return null;
            }
        }

        private static SEED CreateSeed(byte[] password, byte[] salt, int iterationCount)
        {
            var pdbBytes = new PasswordDeriveBytes(password, salt, "SHA1", iterationCount).GetBytes(20);

            var seed = new SEED
            {
                ModType  = SEED.MODE.AI_CBC,
                KeyBytes = new byte[16],
                IV       = new byte[16]
            };

            Buffer.BlockCopy(pdbBytes, 0, seed.KeyBytes, 0, 16);

            using (var sha1 = new SHA1CryptoServiceProvider())
                Buffer.BlockCopy(sha1.ComputeHash(pdbBytes, 16, 4), 0, seed.IV, 0, 16);

            return seed;
        }
    }
}
