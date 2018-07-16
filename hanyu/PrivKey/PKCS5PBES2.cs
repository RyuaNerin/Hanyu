using System;
using System.Security.Cryptography;
using Asn1;
using PnPeople.Security;

namespace hanyu.PrivKey
{
    internal class PKCS5PBES2 : IPrivateKey
    {
        /*
        node                30
        node 0              | 30
        node 0 0            | | 06 : 1.2.840.113549.1.5.13
        node 0 1            | | 30
        node 0 1 0          | | | 30
        node 0 1 0 0        | | | | 06 : 1.2.840.113549.1.5.12
        node 0 1 0 1        | | | | 30
        node 0 1 0 1 0      | | | | | 04 : Salt
        node 0 1 0 1 1      | | | | | 02 : Iter
        node 0 1 1          | | | 30
        node 0 1 1 0        | | | | 06 : 1.2.410.200004.1.4
        node 0 1 1 1        | | | | 04 : IV
        node 1              | 04 : DATA
        */
        public PKCS5PBES2(Asn1Node node)
            : base(node)
        {
            this.Salt = this.m_origNode.Get<Asn1OctetString>(0, 1, 0, 1, 0).Data;
            this.Iter = this.m_origNode.Get<Asn1Integer    >(0, 1, 0, 1, 1).ToInt32();
            this.m_iv = this.m_origNode.Get<Asn1OctetString>(0, 1, 1, 1   ).Data;
            this.Data = this.m_origNode.Get<Asn1OctetString>(1            ).Data;

        }

        public override byte[] GetBytes()
        {
            this.m_origNode.Get<Asn1OctetString>(0, 1, 0, 1, 0).Data  = this.Salt;
            this.m_origNode.Get<Asn1Integer    >(0, 1, 0, 1, 1).Value = new Asn1Integer(this.Iter).Value;
            this.m_origNode.Get<Asn1OctetString>(0, 1, 1, 1   ).Data  = this.m_iv;
            this.m_origNode.Get<Asn1OctetString>(1            ).Data  = this.Data;

            return this.m_origNode.GetBytes();
        }

        private readonly byte[] m_iv;

        protected override Random RandomizeSalt()
        {
            var rnd = base.RandomizeSalt();
            rnd.NextBytes(this.m_iv);

            return rnd;
        }

        protected override SEED CreateSeed(byte[] password)
        {
            byte[] pdbBytes;

            using (var pdb = new Rfc2898DeriveBytes(password, this.Salt, this.Iter))
                pdbBytes = pdb.GetBytes(20);

            var seed = new SEED
            {
                ModType  = SEED.MODE.AI_CBC,
                KeyBytes = new byte[16],
                IV       = new byte[16]
            };

            Buffer.BlockCopy(pdbBytes, 0, seed.KeyBytes, 0, 16);
            Buffer.BlockCopy(this.m_iv, 0, seed.IV, 0, 16);

            return seed;
        }
    }
}
