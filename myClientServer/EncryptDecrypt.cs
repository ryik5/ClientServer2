using System;
using System.Security.Cryptography;

namespace myClientServer
{
    public class EncryptDecrypt
    {
        //Cryption Area. Start Block
        public string EncryptStringToBase64Text(string plainText, byte[] Key, byte[] IV) //Encrypt variables PlainText Data. Use EncryptStringToBytes()
        {
            string sBase64Test;
            sBase64Test = Convert.ToBase64String(EncryptStringToBytes(plainText, Key, IV));
            return sBase64Test;
        }

        public static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV) //Encrypt variables PlainText Data
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
            { throw new ArgumentNullException("plainText"); }
            if (Key == null || Key.Length <= 0)
            { throw new ArgumentNullException("Key"); }
            if (IV == null || IV.Length <= 0)
            { throw new ArgumentNullException("IV"); }
            byte[] encrypted;

            using (RijndaelManaged rijAlg = new RijndaelManaged())        // Create an RijndaelManaged object with the specified key and IV. 
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);    // Create a decrytor to perform the stream transform.

                using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())   // Create the streams used for encryption. 
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);   //Write all data to the stream.
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;    // Return the encrypted bytes from the memory stream. 
        }

        #region NewRijndaelManaged
        /// Create a new RijndaelManaged class and initialize it
        /// <param name="text" />The text to encrypt
        /// <param name="salt" />The pasword salt

        /// Generate new cryption and decryption keys
        public string EncryptRijndael(string text, string salt)
        {
            if (string.IsNullOrEmpty(text))
            { throw new ArgumentNullException("text"); }

            var aesAlg = NewRijndaelManaged(salt);

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var msEncrypt = new System.IO.MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
            {
                swEncrypt.Write(text);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public string DecryptRijndael(string cipherText, string salt)
        {
            string text="";
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                { text = ""; throw new ArgumentNullException("cipherText"); }

                if (!IsBase64String(cipherText))
                {
                    text = "The cipherText input parameter is not base64 encoded";
                    throw new Exception("The cipherText input parameter is not base64 encoded");
                }

                var aesAlg = NewRijndaelManaged(salt);
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                var cipher = Convert.FromBase64String(cipherText);

                using (var msDecrypt = new System.IO.MemoryStream(cipher))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            text = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            catch { }
            return text;
        }

        internal const string Inputkey = "f803e097-d7c2-4cc2-a1ba-b167e44d7331";
        private RijndaelManaged NewRijndaelManaged(string salt)
        {
            if (salt == null) { throw new ArgumentNullException("salt"); }
            var saltBytes = System.Text.Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes(Inputkey, saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);  //     btsMess1 = aesAlg.Key; //Key Encrypt
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);  //    btsMess2 = aesAlg.IV; //Key Decrypt

            return aesAlg;
        }
        #endregion

        /// <param name="base64String" />The base64 encoded string
        /// <returns>Base64 encoded stringt</returns>
        private static bool IsBase64String(string base64String)
        {
            base64String = base64String.Trim();
            return (base64String.Length % 4 == 0) &&
                   System.Text.RegularExpressions.Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,3}$",
                   System.Text.RegularExpressions.RegexOptions.None);
        }

        public string DecryptBase64ToString(string sBase64Text, byte[] Key, byte[] IV) //Encrypt variables PlainText Data. Use DecryptStringFromBytes()
        {
            byte[] bBase64Test;
            bBase64Test = Convert.FromBase64String(sBase64Text);
            return DecryptStringFromBytes(bBase64Test, Key, IV);
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV) //Decrypt PlainText Data to variables
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
            { throw new ArgumentNullException("cipherText"); }
            if (Key == null || Key.Length <= 0)
            { throw new ArgumentNullException("Key"); }
            if (IV == null || IV.Length <= 0)
            { throw new ArgumentNullException("IV"); }

            string plaintext = null;   // Declare the string used to hold the decrypted text.

            using (RijndaelManaged rijAlg = new RijndaelManaged())  // Create an RijndaelManaged object  with the specified key and IV.
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);  // Create a decrytor to perform the stream transform.                              

                using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(cipherText))  // Create the streams used for decryption. 
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (System.IO.StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();  // Read the decrypted bytes from the decrypting stream and place them in a string. 
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
