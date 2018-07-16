using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using hanyu.PrivKey;

namespace hanyu
{
    internal class Cert
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

            using (var cert = new X509Certificate2(File.ReadAllBytes(this.m_certFilePath)))
            {
                this.Name = cert.Subject;
                var m = Regex.Match(this.Name, @"CN=([^,\(]+)");
                if (m.Success)
                    this.Name = m.Groups[1].Value;

                this.NotAfter = cert.NotAfter;
                this.Drive = Path.GetPathRoot(path);

                string oid = null;
                var policy = cert.Extensions.Cast<X509Extension>().FirstOrDefault(e => e.Oid.Value == "2.5.29.32");
                if (policy != null)
                {
                    m = Regex.Match(policy.Format(false), "Policy Identifier=([^,]+),");
                    if (m.Success)
                        oid = m.Groups[1].Value;
                }

                if (!string.IsNullOrWhiteSpace(oid) && OIDs.ContainsKey(oid))
                {
                    var v = OIDs[oid];

                    this.Ca = v.Ca;
                    this.Type = v.Desc;
                }
                else
                {
                    this.Ca = this.m_caName;
                    this.Type = "알수없음";
                }
            }

            try
            {
                using (var file = new FileStream(this.m_keyFilePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    file.Position = file.Length - 1;
                }

                this.Editable = true;
            }
            catch
            {
                this.Editable = false;
            }
        }

        private readonly string m_certDir;
        private readonly string m_npkiDir;
        private readonly string m_caName;
        private readonly string m_userName;

        private readonly string m_certFilePath;
        private readonly string m_keyFilePath;

        public bool     Editable  { get; }
        public string   Name      { get; }
        public string   Type      { get; }
        public string   Ca        { get; }
        public DateTime NotAfter  { get; }
        public string   Drive     { get; }

        public void CopyTo(string newDrive)
        {
            var localnow = SystemDirectory.LocalNow;
            var newPath = Path.Combine(newDrive == Path.GetPathRoot(localnow) ? localnow : newDrive, "NPKI", this.m_caName, "User", this.m_userName);

            Directory.CreateDirectory(newPath);

            File.Copy(this.m_certFilePath, Path.Combine(newPath, Path.GetFileName(this.m_certFilePath)));
            File.Copy(this.m_keyFilePath, Path.Combine(newPath, Path.GetFileName(this.m_keyFilePath)));
        }

        public void MoveTo(string newDrive)
        {
            var localnow = SystemDirectory.LocalNow;
            var newPath = Path.Combine(newDrive == Path.GetPathRoot(localnow) ? localnow : newDrive, "NPKI", this.m_caName, "User", this.m_userName);

            Directory.CreateDirectory(newPath);

            File.Move(this.m_certFilePath, Path.Combine(newPath, Path.GetFileName(this.m_certFilePath)));
            File.Move(this.m_keyFilePath, Path.Combine(newPath, Path.GetFileName(this.m_keyFilePath)));
            
            this.AutoRemoveNpki();
        }

        public void Remove()
        {
            File.Delete(this.m_certFilePath);
            File.Delete(this.m_keyFilePath);

            Directory.Delete(this.m_certDir);

            this.AutoRemoveNpki();
        }

        private void AutoRemoveNpki()
        {
            Directory.Delete(this.m_certDir);

            bool existsCerts = false;
            foreach (var caDir in Directory.GetDirectories(this.m_npkiDir))
            {
                foreach (var certDir in Directory.GetDirectories(caDir))
                {
                    if (Directory.GetFiles(certDir, "*", SearchOption.AllDirectories).Length != 0)
                    {
                        existsCerts = true;
                        break;
                    }
                }
                if (existsCerts)
                    break;
            }

            if (!existsCerts)
                Directory.Delete(this.m_npkiDir, true);
        }

        public bool CheckPassword(string password)
        {
            using (var file = new FileStream(this.m_keyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buff = new byte[file.Length];
                file.Read(buff, 0, buff.Length);

                return IPrivateKey.CheckPassword(buff, password);
            }
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            using (var file = new FileStream(this.m_keyFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                var buff = new byte[file.Length];
                file.Read(buff, 0, buff.Length);

                var newData = IPrivateKey.ChangePassword(buff, oldPassword, newPassword);
                if (newData == null)
                    return false;

                file.SetLength(0);
                file.Position = 0;
                file.Write(newData, 0, newData.Length);
                return true;
            }
        }

        private static readonly IDictionary<string, (string Ca, string Desc)> OIDs = new SortedDictionary<string, (string, string)>
        {
            { "1.2.410.200004.5.2.1.1"  , ("한국정보인증"    , "범용(법인)"  ) },
            { "1.2.410.200004.5.2.1.2"  , ("한국정보인증"    , "범용(개인)"  ) },
            { "1.2.410.200004.5.1.1.7"  , ("한국증권전산"    , "범용(법인)"  ) },
            { "1.2.410.200004.5.1.1.5"  , ("한국증권전산"    , "범용(개인)"  ) },
            { "1.2.410.200005.1.1.5"    , ("금융결제원"      , "범용(법인)"  ) },
            { "1.2.410.200005.1.1.1"    , ("금융결제원"      , "범용(개인)"  ) },
            { "1.2.410.200004.5.4.1.2"  , ("한국전자인증"    , "범용(법인)"  ) },
            { "1.2.410.200004.5.4.1.1"  , ("한국전자인증"    , "범용(개인)"  ) },
            { "1.2.410.200012.1.1.3"    , ("한국무역정보통신", "범용(법인)"  ) },
            { "1.2.410.200012.1.1.1"    , ("한국무역정보통신", "범용(개인)"  ) },
            { "1.2.410.200004.5.5.1.1"  , ("이니텍"          , "범용(법인)"  ) },
            { "1.2.410.200004.5.5.1.2"  , ("이니텍"          , "범용(개인)"  ) },
            { "1.2.410.200004.5.2.1.7.1", ("한국정보인증"    , "금융(개인)"  ) },
            { "1.2.410.200004.5.2.1.7.2", ("한국정보인증"    , "증권(개인)"  ) },
            { "1.2.410.200004.5.2.1.7.3", ("한국정보인증"    , "신용(개인)"  ) },
            { "1.2.410.200004.5.1.1.9"  , ("한국증권전산"    , "용도제한"    ) },
            { "1.2.410.200005.1.1.4"    , ("금융결제원"      , "금융(개인)"  ) },
            { "1.2.410.200005.1.1.6.2"  , ("금융결제원"      , "신용(개인)"  ) },
            { "1.2.410.200004.5.4.1.101", ("한국전자인증"    , "금융(개인)"  ) },
            { "1.2.410.200004.5.4.1.102", ("한국전자인증"    , "증권(개인)"  ) },
            { "1.2.410.200004.5.4.1.103", ("한국전자인증"    , "보험(개인)"  ) },
            { "1.2.410.200004.5.4.1.104", ("한국전자인증"    , "신용(개인)"  ) },
            { "1.2.410.200004.5.4.1.105", ("한국전자인증"    , "민원(개인)"  ) },
            { "1.2.410.200004.5.4.1.106", ("한국전자인증"    , "금융(전자)"  ) },
            { "1.2.410.200004.5.4.1.107", ("한국전자인증"    , "증권(전자)"  ) },
            { "1.2.410.200004.5.4.1.108", ("한국전자인증"    , "보험(전자)"  ) },
            { "1.2.410.200004.5.4.1.109", ("한국전자인증"    , "신용(전자)"  ) },
            { "1.2.410.200012.11.31"    , ("한국무역정보통신", "금융(서명용)") },
            { "1.2.410.200012.11.32"    , ("한국무역정보통신", "금융(암호용)") },
            { "1.2.410.200012.11.35"    , ("한국무역정보통신", "증권(서명용)") },
            { "1.2.410.200012.11.36"    , ("한국무역정보통신", "증권(암호용)") },
            { "1.2.410.200012.11.39"    , ("한국무역정보통신", "보험(서명용)") },
            { "1.2.410.200012.11.40"    , ("한국무역정보통신", "보험(암호용)") },
            { "1.2.410.200012.11.43"    , ("한국무역정보통신", "신용(서명용)") },
            { "1.2.410.200012.11.44"    , ("한국무역정보통신", "신용(암호용)") },
        };
    }
}
