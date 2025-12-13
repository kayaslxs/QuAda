// Dosya Adý: CipherService.cs (V3 - 4 Basamaklý Kodlama ve Dosya Ýþlemleri)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;
using System.IO;

namespace QuAdaCryption
{
    public class CipherService
    {
        // Karakterden Koda (Þifreleme) Sözlüðü - 4 Basamaklý Kod
        private readonly Dictionary<char, string> encryptionKey = new Dictionary<char, string>
        {
            // BÜYÜK HARFLER (00XX, 12XX, 41XX...)
            {'A',"0000"},{'B',"1200"},{'C',"4100"},{'Ç',"1400"},{'D',"9200"},{'E',"8100"},
            {'F',"5200"},{'G',"4400"},{'Ð',"5400"},{'H',"1500"},{'I',"2000"},{'Ý',"2100"},
            {'J',"5000"},{'K',"3100"},{'L',"0400"},{'M',"4300"},{'N',"7200"},{'O',"7000"},
            {'Ö',"7100"},{'P',"6900"},{'R',"6300"},{'S',"8000"},{'Þ',"8200"},{'T',"1100"},
            {'U',"1000"},{'Ü',"1600"},{'V',"0100"},{'Y',"0500"},{'Z',"0800"},{'X',"9400"},
            {'W',"9900"},{'Q',"5500"},

            // KÜÇÜK HARFLER (09XX, 02XX, 13XX...)
            {'a',"0900"},{'b',"0200"},{'c',"1300"},{'ç',"4600"},{'d',"8900"},{'e',"0600"},
            {'f',"4800"},{'g',"3300"},{'ð',"2500"},{'h',"7400"},{'ý',"8800"},{'i',"9600"},
            {'j',"6200"},{'k',"4000"},{'l',"1800"},{'m',"0700"},{'n',"8400"},{'o',"3700"},
            {'ö',"4200"},{'p',"6700"},{'r',"2900"},{'s',"3400"},{'þ',"7700"},{'t',"8600"},
            {'u',"1900"},{'ü',"3000"},{'v',"9100"},{'y',"2700"},{'z',"5800"},{'x',"2400"},
            {'w',"1700"},{'q',"9300"},

            // SAYILAR 
            {'0',"0300"},{'1',"0301"},{'2',"0302"},{'3',"0303"},{'4',"0304"},
            {'5',"0305"},{'6',"0306"},{'7',"0307"},{'8',"0308"},{'9',"0309"},

            // NOKTALAMA & DÝÐERLERÝ
            {'.',"2200"}, {',',"2300"}, {'!',"2600"}, {'?',"2800"}, {':',"3200"}, {';',"9700"},
            {'\'',"3500"},{'"',"3600"},{'(',"3800"},{')',"3900"},{'-',"4500"},{'_',"4700"},
            {'*',"4900"},{'/',"5600"},{'@',"5700"},{'#',"5900"},{'$',"6000"},{'%',"6100"},
            {'&',"9500"},{'=',"6400"},{'+',"6500"},{'<',"6600"},{'>',"6800"},{'[',"7300"},
            {']',"9000"},{'{',"7500"},{'}',"7600"},
            
            // PROGRAMLAMA OPERATÖRLERÝ
            {'|',"0311"},
            {'~',"0310"},
            {'^',"0312"},
            {'\\',"0313"},
            {'`',"0314"}, 

            // BOÞLUK & FORMATLAMA KARAKTERLERÝ
            {' ',"9800"},
            {'\n',"8700"},
            {'\r',"8500"},
            {'\t',"5100"}
        };

        private readonly Dictionary<string, char> decryptionKey;

        public CipherService()
        {
            decryptionKey = encryptionKey.ToDictionary(pair => pair.Value, pair => pair.Key);
        }

        public Dictionary<char, string> GetEncryptionKey()
        {
            return encryptionKey;
        }

        // ---------------------------------------------------------------------
        // TEMEL ÞÝFRELEME/ÇÖZME METOTLARI (4 BASAMAKLI)
        // ---------------------------------------------------------------------

        public string Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return string.Empty;

            var result = new StringBuilder();
            foreach (char c in plaintext)
            {
                if (encryptionKey.TryGetValue(c, out string? code))
                {
                    result.Append(code);
                }
            }
            return result.ToString();
        }

        public string Decrypt(string ciphertext)
        {
            ciphertext = ciphertext.Replace(" ", "");

            if (string.IsNullOrEmpty(ciphertext) || ciphertext.Length % 4 != 0)
            {
                return "ERROR: The encryption code must consist of a multiple of four digits.";
            }

            var result = new StringBuilder();
            for (int i = 0; i < ciphertext.Length; i += 4)
            {
                string code = ciphertext.Substring(i, 4);

                if (decryptionKey.TryGetValue(code, out char character))
                {
                    result.Append(character);
                }
                else
                {
                    result.Append('?');
                }
            }
            return result.ToString();
        }

        // ---------------------------------------------------------------------
        // DOSYA ÝÞLEMLERÝ
        // ---------------------------------------------------------------------

        public string EncryptFile(string filePath)
        {
            try
            {
                string plaintext = File.ReadAllText(filePath, Encoding.UTF8);
                string ciphertext = Encrypt(plaintext);

                string fileName = Path.GetFileName(filePath);
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string newFilePath = Path.Combine(documentsPath, fileName + ".KAYA");

                File.WriteAllText(newFilePath, ciphertext, Encoding.UTF8);

                return newFilePath;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public string DecryptFile(string filePath)
        {
            const string KAYA_EXTENSION = ".KAYA";
            try
            {
                string ciphertext = File.ReadAllText(filePath, Encoding.UTF8);
                string plaintext = Decrypt(ciphertext);

                if (plaintext.StartsWith("ERROR:"))
                {
                    return plaintext;
                }

                string fileNameWithExt = Path.GetFileName(filePath);

                string decryptedFileName;
                if (fileNameWithExt.EndsWith(KAYA_EXTENSION, StringComparison.OrdinalIgnoreCase))
                {
                    // .KAYA uzantýsýný kaldýr
                    decryptedFileName = fileNameWithExt.Substring(0, fileNameWithExt.Length - KAYA_EXTENSION.Length);
                }
                else
                {
                    // Uzantý .KAYA deðilse güvenli olmasý için öne ekleme yap
                    decryptedFileName = "DEC_UNSAFE_" + fileNameWithExt;
                }

                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string newFilePath = Path.Combine(documentsPath, decryptedFileName);

                File.WriteAllText(newFilePath, plaintext, Encoding.UTF8);

                return newFilePath;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        // ---------------------------------------------------------------------
        // QR KOD ÝÞLEMLERÝ
        // ---------------------------------------------------------------------

        public string DecodeQRImage(string imagePath)
        {
            try
            {
                using (var barcodeBitmap = (Bitmap)System.Drawing.Image.FromFile(imagePath))
                {
                    var reader = new ZXing.Windows.Compatibility.BarcodeReader();

                    reader.Options = new ZXing.Common.DecodingOptions
                    {
                        PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE }
                    };

                    var result = reader.Decode(barcodeBitmap);

                    if (result != null)
                    {
                        return result.Text;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR Code Decoding Error: {ex.Message}");
                return "ERROR";
            }
        }
    }
}