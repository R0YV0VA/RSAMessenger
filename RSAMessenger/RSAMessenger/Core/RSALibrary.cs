using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

using CSharp_easy_RSA_PEM;

namespace RSAMessenger.Core
{
    internal class RSALibrary
    {
        private string PEMPublicHeader = "-----BEGIN PUBLIC KEY-----\n";
        private string PEMPrivateHeader = "-----BEGIN RSA PRIVATE KEY-----\n";
        private string PEMPublicFooter = "\n-----END PUBLIC KEY-----";
        private string PEMPrivateFooter = "\n-----END RSA PRIVATE KEY-----";
        public RSAKeys createRSAKeys()
        {
            var rsa = new RSACryptoServiceProvider(2048);
            var rsaKeys = new RSAKeys
            {
                Private = Crypto.ExportPrivateKeyToRSAPEM(rsa),
                Public = Crypto.ExportPublicKeyToX509PEM(rsa)
            };
            return rsaKeys;
        }

        public string Encrypt(string textToEncrypt, string publicKeyString)
        {
            var publicKeyStringHF = PEMPublicHeader + publicKeyString + PEMPublicFooter;
            var RSApublicKey = ImportPublicKey(publicKeyStringHF);
            var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
            var bytesCypherText = RSApublicKey.Encrypt(bytesToEncrypt, false);
            var cypherText = Convert.ToBase64String(bytesCypherText);
            return cypherText;
        }

        public string Decrypt(string textToDecrypt, string privateKeyString)
        {
            var privateKeyStringHF = PEMPrivateHeader + privateKeyString + PEMPrivateFooter;
            var RSAprivateKey = ImportPrivateKey(privateKeyStringHF);
            var bytesToDescrypt = Convert.FromBase64String(textToDecrypt);
            var bytesPlainTextData = RSAprivateKey.Decrypt(bytesToDescrypt, false);
            var plainTextData = System.Text.Encoding.UTF8.GetString(bytesPlainTextData);
            return plainTextData;
        }
        private RSACryptoServiceProvider ImportPrivateKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }

        private RSACryptoServiceProvider ImportPublicKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }
    }
}
