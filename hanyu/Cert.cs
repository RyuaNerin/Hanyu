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
            this.m_certDir     = path;
            this.m_certDirName = Path.GetFileName(path);
            this.m_caDirName   = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
            this.m_npkiDir     = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path)));

            var der = new List<string>
            {
                Path.Combine(path, "signCert.der"),
                Path.Combine(path, "kmCert.der"),
            };
            var key = new List<string>
            {
                Path.Combine(path, "signPri.key"),
                Path.Combine(path, "kmPri.key")
            };

            this.m_der = der.Where(e => File.Exists(e)).ToArray();
            this.m_key = key.Where(e => File.Exists(e)).ToArray();
            
            using (var cert = new X509Certificate2(File.ReadAllBytes(this.m_der[0])))
            {
                this.Name = cert.Subject;
                var m = Regex.Match(this.Name, @"CN=([^,\(\d]+)");
                if (m.Success)
                    this.Name = m.Groups[1].Value;

                this.NotAfter = cert.NotAfter;
                this.Drive = Path.GetPathRoot(path);

                string oid = null;
                var policy = cert.Extensions.Cast<X509Extension>().FirstOrDefault(e => e.Oid.Value == "2.5.29.32");
                if (policy != null)
                {
                    m = Regex.Match(policy.Format(false), "Policy Identifier=([^,]+)");
                    if (m.Success)
                        oid = m.Groups[1].Value;
                }

                var ca = OidCa.FirstOrDefault(e => oid.StartsWith(e.Key)).Value;
                this.Ca = !string.IsNullOrWhiteSpace(ca) ? ca : this.m_caDirName;

                var ty = OidTypes.FirstOrDefault(e => oid.StartsWith(e.Key)).Value;
                this.Type = !string.IsNullOrWhiteSpace(ty) ? ty : "-";
            }

            try
            {
                using (var file = new FileStream(this.m_key[0], FileMode.Open, FileAccess.ReadWrite))
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
        private readonly string m_caDirName;
        private readonly string m_certDirName;

        private readonly string[] m_der;
        private readonly string[] m_key;

        public bool     Editable  { get; }
        public string   Name      { get; }
        public string   Type      { get; }
        public string   Ca        { get; }
        public DateTime NotAfter  { get; }
        public string   Drive     { get; }

        public void CopyTo(string newDrive)
        {
            var localnow = SystemDirectory.LocalNow;
            var newPath = Path.Combine(newDrive == Path.GetPathRoot(localnow) ? localnow : newDrive, "NPKI", this.m_caDirName, "User", this.m_certDirName);

            Directory.CreateDirectory(newPath);

            foreach (var der in this.m_der) File.Copy(der, Path.Combine(newPath, Path.GetFileName(der)));
            foreach (var key in this.m_key) File.Copy(key, Path.Combine(newPath, Path.GetFileName(key)));
        }

        public void MoveTo(string newDrive)
        {
            var localnow = SystemDirectory.LocalNow;
            var newPath = Path.Combine(newDrive == Path.GetPathRoot(localnow) ? localnow : newDrive, "NPKI", this.m_caDirName, "User", this.m_certDirName);

            Directory.CreateDirectory(newPath);

            foreach (var der in this.m_der) File.Move(der, Path.Combine(newPath, Path.GetFileName(der)));
            foreach (var key in this.m_key) File.Move(key, Path.Combine(newPath, Path.GetFileName(key)));
            
            this.AutoRemoveNpki();
        }

        public void Remove()
        {
            foreach (var der in this.m_der) File.Delete(der);
            foreach (var key in this.m_key) File.Delete(key);

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
            using (var file = new FileStream(this.m_key[0], FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buff = new byte[file.Length];
                file.Read(buff, 0, buff.Length);

                return IPrivateKey.CheckPassword(buff, password);
            }
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            int i;

            var keyStream = new FileStream[this.m_key.Length];
            var keyData = new byte[this.m_key.Length][];

            var succ = true;

            for (i = 0; i < this.m_key.Length; ++i)
                keyStream[i] = new FileStream(this.m_key[i], FileMode.Open, FileAccess.ReadWrite);

            for (i = 0; i < this.m_key.Length; ++i)
            {
                var buff = new byte[keyStream[i].Length];
                keyStream[i].Read(buff, 0, buff.Length);

                var newData = IPrivateKey.ChangePassword(buff, oldPassword, newPassword);
                if (newData == null)
                {
                    succ = false;
                    break;
                }

                keyData[i] = newData;
            }

            if (succ)
            {
                for (i = 0; i < this.m_key.Length; ++i)
                {
                    keyStream[i].SetLength(0);
                    keyStream[i].Position = 0;
                    keyStream[i].Write(keyData[i], 0, keyData[i].Length);
                }
            }

            for (i = 0; i < this.m_key.Length; ++i)
            {
                keyStream[i].Flush();
                keyStream[i].Dispose();
            }

            return succ;
        }

        private static readonly IDictionary<string, string> OidCa = new Dictionary<string, string>
        {
            { "1.2.410.100001.5.7", "행정안전부"       },
            { "1.2.410.100001.5.3", "교육부"           },
            { "1.2.410.100001.5.5", "대검찰청"         },
            { "1.2.410.100001.5.6", "병무청"           },
            { "1.2.410.100001.5.8", "대법원"           },
            
            { "1.2.410.200004.5.1", "한국증권전산"     }, // signkorea / 코스콤
            { "1.2.410.200004.5.2", "한국정보인증"     }, // signgate
            { "1.2.410.200004.5.3", "한국정보화진흥원" }, // niasign
            { "1.2.410.200004.5.4", "한국전자인증"     }, // crosscert
            { "1.2.410.200004.5.5", "이니텍"           }, // inipass
            { "1.2.410.200005",     "금융결제원"       }, // kftc (yessign)
            { "1.2.410.200012",     "한국무역정보통신" }, // ktnet
        }.OrderByDescending(e => e.Key.Length).ToDictionary(e => e.Key, e => e.Value);

        private static readonly IDictionary<string, string> OidTypes = new Dictionary<string, string>
        {
             { "1.2.410.100001.2.1.1"  , "전자관인" },
             { "1.2.410.100001.2.1.2"  , "컴퓨터" },
             { "1.2.410.100001.2.1.3"  , "전자목적용" },
             { "1.2.410.100001.2.1.4"  , "전자관인(공공/민간)" },
             { "1.2.410.100001.2.1.5"  , "컴퓨터(공공/민간)" },
             { "1.2.410.100001.2.1.6"  , "특수목적용(공공/민간)" },
             { "1.2.410.100001.2.2.1"  , "공무원용" },
             { "1.2.410.100001.2.2.2"  , "개인용" },
             { "1.2.410.100001.5.3.1.1", "전자관인용" },
             { "1.2.410.100001.5.3.1.3", "개인용" },
             { "1.2.410.100001.5.3.1.5", "특수목적용" },
             { "1.2.410.100001.5.3.1.7", "컴퓨터용" },
             { "1.2.410.100001.5.3.1.9", "SSL용" },

             { "1.2.410.200004.2.1	"      , "공인인증기관" },
             { "1.2.410.200004.5.1.1.1"    , "용도제한(개인)" },
             { "1.2.410.200004.5.1.1.2"    , "용도제한(개인 서버)" },
             { "1.2.410.200004.5.1.1.3"    , "용도제한(법인)" },
             { "1.2.410.200004.5.1.1.4"    , "용도제한(법인 서버)" },
             { "1.2.410.200004.5.1.1.5"    , "범용(개인)" },
             { "1.2.410.200004.5.1.1.6"    , "범용(개인서버)" },
             { "1.2.410.200004.5.1.1.7"    , "범용(법인)" },
             { "1.2.410.200004.5.1.1.8"    , "범용(법인서버)" },
             { "1.2.410.200004.5.1.1.9"    , "용도제한(골드, 증권/보험)" },
             { "1.2.410.200004.5.1.1.9.2"  , "용도제한(골드, 신용카드용)" },
             { "1.2.410.200004.5.1.1.10"   , "용도제한(골드, 개인서버)" },
             { "1.2.410.200004.5.1.1.11"   , "국세청용(개인)" },
             { "1.2.410.200004.5.1.1.12"   , "국세청용(법인)" },
             { "1.2.410.200004.5.2.1.1"    , "범용(법인)" },
             { "1.2.410.200004.5.2.1.2"    , "범용(개인)" },
             { "1.2.410.200004.5.2.1.3"    , "특별등급(전자입찰)" },
             { "1.2.410.200004.5.2.1.4"    , "1등급인증서(서버)" },
             { "1.2.410.200004.5.2.1.5"    , "특별등급 법인" },
             { "1.2.410.200004.5.2.1.5.140", "용도제한(NEIS)" },
             { "1.2.410.200004.5.2.1.5001" , "국세청용(개인,법인)" },
             { "1.2.410.200004.5.2.1.6"    , "용도제한(법인)" },
             { "1.2.410.200004.5.2.1.7.1"  , "금융(개인)" },
             { "1.2.410.200004.5.2.1.7.2"  , "증권(개인)" },
             { "1.2.410.200004.5.2.1.7.3"  , "신용(개인)" },

             { "1.2.410.200004.5.3.1.1", "범용(기관)" },
             { "1.2.410.200004.5.3.1.2", "범용(법인)" },
             { "1.2.410.200004.5.3.1.3", "범용(서버)" },
             { "1.2.410.200004.5.3.1.4", "용도제한(개인)" },
             { "1.2.410.200004.5.3.1.5", "용도제한(기관)" },
             { "1.2.410.200004.5.3.1.6", "용도제한(법인)" },
             { "1.2.410.200004.5.3.1.7", "용도제한(서버)" },
             { "1.2.410.200004.5.3.1.8", "용도제한(개인)" },
             { "1.2.410.200004.5.3.1.9", "범용(개인)" },

             { "1.2.410.200004.5.4.1.1"  , "범용(개인)" },
             { "1.2.410.200004.5.4.1.2"  , "범용(법인)" },
             { "1.2.410.200004.5.4.1.3"  , "범용(서버)" },
             { "1.2.410.200004.5.4.1.4"  , "용도제한(개인)" },
             { "1.2.410.200004.5.4.1.5"  , "용도제한(법인)" },
             { "1.2.410.200004.5.4.1.74" , "국세청용(개인)" },
             { "1.2.410.200004.5.4.1.101", "은행(개인)" },
             { "1.2.410.200004.5.4.1.102", "증권(개인)" },
             { "1.2.410.200004.5.4.1.103", "신용카드(개인)" },
             { "1.2.410.200004.5.4.1.104", "전자민원(개인)" },
             { "1.2.410.200004.5.4.1.105", "전자민원(개인)" },
             { "1.2.410.200004.5.4.1.106", "은행/전자민원(개인)" },
             { "1.2.410.200004.5.4.1.107", "증권/전자민원(개인)" },
             { "1.2.410.200004.5.4.1.108", "보험/전자민원(개인)" },
             { "1.2.410.200004.5.4.1.109", "신용카드/전자민원(개인)" },
             { "1.2.410.200004.5.4.2.52" , "국세청용" },

             { "1.2.410.200004.5.5.1.1", "범용(법인)" },
             { "1.2.410.200004.5.5.1.2", "범용(개인)" },

             { "1.2.410.200005.1.1.1"  , "범용(개인)" },
             { "1.2.410.200005.1.1.2"  , "은행/신용카드/보험(법인/단체)" },
             { "1.2.410.200005.1.1.3"  , "서버(법인/단체)" },
             { "1.2.410.200005.1.1.4"  , "은행/신용카드/보험(개인)" },
             { "1.2.410.200005.1.1.5"  , "범용(법인/단체)" },
             { "1.2.410.200005.1.1.6"  , "용도제한" },
             { "1.2.410.200005.1.1.6.1", "기업뱅킹(법인/단체)" },
             { "1.2.410.200005.1.1.6.2", "신용카드(개인)" },
             { "1.2.410.200005.1.1.6.3", "조달청 원클릭용(법인/단체)" },
             { "1.2.410.200005.1.1.6.4", "퇴직연금" },
             { "1.2.410.200005.1.1.6.5", "재정경제부 CTR용(법인/단체)" },

             { "1.2.410.200006.2.1", "국방공무원" },
             
             { "1.2.410.200012.1.1.1"  , "전자거래 서명용(개인)" },
             { "1.2.410.200012.1.1.2"  , "전자거래 암호용(개인)" },
             { "1.2.410.200012.1.1.3"  , "전자거래 서명용(법인)" },
             { "1.2.410.200012.1.1.4"  , "전자거래 암호용(법인)" },
             { "1.2.410.200012.1.1.5"  , "전자거래 서명용(서버)" },
             { "1.2.410.200012.1.1.6"  , "전자거래 암호용(서버)" },
             { "1.2.410.200012.1.1.7"  , "전자무역 서명용(개인)" },
             { "1.2.410.200012.1.1.8"  , "전자무역 암호용(개인)" },
             { "1.2.410.200012.1.1.9"  , "전자무역 서명용(법인)" },
             { "1.2.410.200012.1.1.10" , "전자무역 암호용(법인)" },
             { "1.2.410.200012.1.1.11" , "전자무역 서명용(서버)" },
             { "1.2.410.200012.1.1.12" , "전자무역 암호용(서버)" },
             { "1.2.410.200012.1.1.101", "은행/보험용" },
             { "1.2.410.200012.1.1.103", "증권/보험용" },
             { "1.2.410.200012.1.1.105", "신용카드용" },
             { "1.2.410.200012.11.31"  , "금융(서명용)" },
             { "1.2.410.200012.11.32"  , "금융(암호용)" },
             { "1.2.410.200012.11.35"  , "증권(서명용)" },
             { "1.2.410.200012.11.36"  , "증권(암호용)" },
             { "1.2.410.200012.11.39"  , "보험(서명용)" },
             { "1.2.410.200012.11.40"  , "보험(암호용)" },
             { "1.2.410.200012.11.43"  , "신용(서명용)" },
             { "1.2.410.200012.11.44"  , "신용(암호용)" },
        }.OrderByDescending(e => e.Key.Length).ToDictionary(e => e.Key, e => e.Value);
    }
}
