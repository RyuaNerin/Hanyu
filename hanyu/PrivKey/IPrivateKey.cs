using System;
using System.Security.Cryptography;
using System.Text;
using Asn1;
using PnPeople.Security;

namespace hanyu.PrivKey
{
    internal abstract class IPrivateKey
    {
        public static bool CheckPassword(byte[] data, string password)
        {
            var pwd = Encoding.ASCII.GetBytes(password);

            return ParseToIKey(data).Decrypt(pwd);
        }
        public static byte[] ChangePassword(byte[] data, string oldPassword, string newPassword)
        {
            var opwd = Encoding.ASCII.GetBytes(oldPassword);
            var npwd = Encoding.ASCII.GetBytes(newPassword);

            var pk = ParseToIKey(data);

            if (!pk.Decrypt(opwd))
                return null;

            pk.RandomizeSalt();

            if (!pk.Encrypt(npwd))
                return null;

            return pk.GetBytes();
        }

        protected readonly Asn1Node m_origNode;
        protected IPrivateKey(Asn1Node node)
        {
            this.m_origNode = node;
        }

        protected byte[] Data { get; set; }
        protected byte[] Salt { get; set; }
        protected int Iter { get; set; }

        protected virtual Random RandomizeSalt()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            rnd.NextBytes(this.Salt);

            return rnd;
        }

        public abstract byte[] GetBytes();

        protected abstract SEED CreateSeed(byte[] password);

        private bool Encrypt(byte[] password)
        {
            try
            {
                var newData = CreateSeed(password).Encrypt(this.Data);
                if (newData != null)
                {
                    this.Data = newData;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
        private bool Decrypt(byte[] password)
        {
            try
            {
                var newData = CreateSeed(password).Decrypt(this.Data);
                if (newData != null)
                {
                    this.Data = newData;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static IPrivateKey ParseToIKey(byte[] data)
        {
            /*
            node        30
            node[0]     | 30
            node[0][0]  | | 06 - Algorithm
            node[0][1]  | | ...
            node[1][0]  | 04 DATA
            */
            var node = Asn1Node.ReadNode(data);

            var alro = node.Nodes[0].Nodes[0] as Asn1ObjectIdentifier;
            if (alro == null) return null;

            if (alro.Value == "1.2.410.200004.1.15"  ) return new SeedCBCWithSHA1(node);
            if (alro.Value == "1.2.840.113549.1.5.13") return new PKCS5PBES2(node);

            return null;
        }
    }

    internal static class Asn1Extension
    {
        public static T Get<T>(this Asn1Node node, params int[] indexes)
            where T : Asn1Node
        {
            for (int i = 0; i < indexes.Length; ++i)
                node = node.Nodes[indexes[i]];

            return node as T;
        }
        public static void Set(this Asn1Node node, Asn1Node item, params int[] indexes)
        {
            for (int i = 0; i < indexes.Length - 1; ++i)
                node = node.Nodes[indexes[i]];

            node.Nodes[indexes[indexes.Length - 1]] = item;
        }

        public static int ToInt32(this Asn1Integer node)
            => (int)node.ToUInt64();
    }
}
