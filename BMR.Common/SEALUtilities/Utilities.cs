using Microsoft.Research.SEAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BMR.Common.SEALUtilities
{
    public class Utilities
    {
        private readonly Encryptor encryptor;
        private readonly Evaluator evaluator;
        private readonly Decryptor decryptor;
        private readonly CKKSEncoder encoder;
        private readonly SEALContext context;
        public Utilities()
        {
            EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS);
            ulong polyModulusDegree = 8192;
            parms.PolyModulusDegree = polyModulusDegree;
            parms.CoeffModulus = CoeffModulus.Create(polyModulusDegree, new int[] { 60, 40, 40, 60 });
            double scale = Math.Pow(2.0, 40);
            context = new SEALContext(parms);
            KeyGenerator keygen = new KeyGenerator(context);
            SecretKey secretKey = keygen.SecretKey;
            keygen.CreatePublicKey(out PublicKey publicKey);
            encryptor = new Encryptor(context, publicKey);
            evaluator = new Evaluator(context);
            decryptor = new Decryptor(context, secretKey);
            encoder = new CKKSEncoder(context);
        }
        public Encryptor Encryptor 
        {
            get
            {
                return encryptor;
            } 
        }
        public Evaluator Evaluator
        {
            get
            {
                return evaluator;
            }
        }
        public Decryptor Decryptor
        {
            get
            {
                return decryptor;
            }
        }
        public CKKSEncoder CKKSEncoder
        {
            get
            {
                return encoder;
            }
        }
        public SEALContext SEALContext
        {
            get
            {
                return context;
            }
        }
        public string CiphertextToBase64String(Ciphertext ciphertext)
        {
            using (var ms = new MemoryStream())
            {
                ciphertext.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        public Ciphertext BuildCiphertextFromBase64String(string base64, SEALContext context)
        {
            var payload = Convert.FromBase64String(base64);

            using (var ms = new MemoryStream(payload))
            {
                var ciphertext = new Ciphertext();
                ciphertext.Load(context, ms);

                return ciphertext;
            }
        }
    }
}
