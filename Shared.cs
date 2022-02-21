using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sleepeye.MVC
{
    public class Shared
    {
        /// <summary>
        /// Convert String to  Base64 
        /// </summary>
        /// <param name="strEncrypted"></param>
        /// <returns></returns>
        public static string EnryptBase64String(string strEncrypted)
        {
            // return strEncrypted;
            byte[] b = ASCIIEncoding.UTF8.GetBytes(strEncrypted);
            string encryptedPassword = Convert.ToBase64String(b);
            return encryptedPassword;
        }

        /// <summary>
        /// Convert Base64 to String
        /// </summary>
        /// <param name="encrString"></param>
        /// <returns></returns>
        public static string DecryptBase64String(string encrString)
        {
            // return encrString;
            byte[] b = Convert.FromBase64String(encrString);
            string decryptedPassword = ASCIIEncoding.UTF8.GetString(b);
            return decryptedPassword;
        }

        /// <summary>
        /// Encript and Password string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encrypt(string key, string data)
        {
            string encData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                encData = EncryptStringToBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }

        /// <summary>
        /// Decrypt and Password string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decrypt(string key, string data)
        {
            string decData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }

        private static byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        private static string EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                            new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] Key, byte[] IV)
        {
            byte[] cipherText = Convert.FromBase64String(cipherTextString);

            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                            new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

        //public void FileToByte(HttpPostedFile Files)
        //{
        //    using (var binaryReader = new BinaryReader(Files.InputStream))
        //    {
        //        var Bytes = binaryReader.ReadBytes(Files.ContentLength);
        //    }
        //}
        /// <summary>
        /// แปลงตัวเลขเป็นตัวอักษรไทย 
        /// </summary>
        /// <param name="strNumber"></param>
        /// <param name="IsTrillion"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string ThaiBahtText(string strNumber, bool IsTrillion = false)
        {
            string BahtText = "";
            string strTrillion = "";
            string[] strThaiNumber = { "ศูนย์", "หนึ่ง", "สอง", "สาม", "สี่", "ห้า", "หก", "เจ็ด", "แปด", "เก้า", "สิบ" };
            string[] strThaiPos = { "", "สิบ", "ร้อย", "พัน", "หมื่น", "แสน", "ล้าน" };

            decimal decNumber = 0;
            decimal.TryParse(strNumber, out decNumber);

            if (decNumber == 0)
            {
                return "ศูนย์บาทถ้วน";
            }

            strNumber = decNumber.ToString("0.00");
            string strInteger = strNumber.Split('.')[0];
            string strSatang = strNumber.Split('.')[1];

            if (strInteger.Length > 13)
                throw new Exception("รองรับตัวเลขได้เพียง ล้านล้าน เท่านั้น!");

            bool _IsTrillion = strInteger.Length > 7;
            if (_IsTrillion)
            {
                strTrillion = strInteger.Substring(0, strInteger.Length - 6);
                BahtText = ThaiBahtText(strTrillion, _IsTrillion);
                strInteger = strInteger.Substring(strTrillion.Length);
            }

            int strLength = strInteger.Length;
            for (int i = 0; i < strInteger.Length; i++)
            {
                string number = strInteger.Substring(i, 1);
                if (number != "0")
                {
                    if (i == strLength - 1 && number == "1" && strLength != 1)
                    {
                        BahtText += "เอ็ด";
                    }
                    else if (i == strLength - 2 && number == "2" && strLength != 1)
                    {
                        BahtText += "ยี่";
                    }
                    else if (i != strLength - 2 || number != "1")
                    {
                        BahtText += strThaiNumber[int.Parse(number)];
                    }

                    BahtText += strThaiPos[(strLength - i) - 1];
                }
            }

            if (IsTrillion)
            {
                return BahtText + "ล้าน";
            }

            if (strInteger != "0")
            {
                BahtText += "บาท";
            }

            if (strSatang == "00")
            {
                BahtText += "ถ้วน";
            }
            else
            {
                strLength = strSatang.Length;
                for (int i = 0; i < strSatang.Length; i++)
                {
                    string number = strSatang.Substring(i, 1);
                    if (number != "0")
                    {
                        if (i == strLength - 1 && number == "1" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "เอ็ด";
                        }
                        else if (i == strLength - 2 && number == "2" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "ยี่";
                        }
                        else if (i != strLength - 2 || number != "1")
                        {
                            BahtText += strThaiNumber[int.Parse(number)];
                        }

                        BahtText += strThaiPos[(strLength - i) - 1];
                    }
                }

                BahtText += "สตางค์";
            }

            return BahtText;
        }

        /// <summary>
        /// แปลงตัวเลขเป็น ตัวเลขไทย
        /// </summary>
        /// <param name="strNumber"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string NumberThaiBaht(string strNumber)
        {
            string BahtText = "";
            string strTrillion = "";
            string[] strThaiNumber = { "๐", "๑", "๒", "๓", "๔", "๕", "๖", "๗", "๘", "๙" };


            decimal decNumber = 0;
            decimal.TryParse(strNumber, out decNumber);



            strNumber = decNumber.ToString("0.00");
            string strInteger = strNumber.Split('.')[0];
            string strSatang = strNumber.Split('.')[1];

            if (strInteger.Length > 13)
                throw new Exception("รองรับตัวเลขได้เพียง ล้านล้าน เท่านั้น!");

            //bool _IsTrillion = strInteger.Length > 7;
            //if (_IsTrillion)
            //{
            //    strTrillion = strInteger.Substring(0, strInteger.Length - 6);
            //    BahtText = NumberThaiBaht(strTrillion, _IsTrillion);
            //    strInteger = strInteger.Substring(strTrillion.Length);
            //}

            int strLength = strInteger.Length;
            for (int i = 0; i < strInteger.Length; i++)
            {
                string number = strInteger.Substring(i, 1);
                //if (number != "0")
                //{
                //if (i == strLength - 1 && number == "1" && strLength != 1)
                //{
                //    BahtText += "เอ็ด";
                //}
                //else if (i == strLength - 2 && number == "2" && strLength != 1)
                //{
                //    BahtText += "ยี่";
                //}
                //else if (i != strLength - 2 || number != "1")
                //{
                BahtText += strThaiNumber[int.Parse(number)];
                //}


                //}
            }

            //if (IsTrillion)
            //{
            //    return BahtText + "ล้าน";
            //}

            if (strInteger != "0")
            {
                //BahtText += "บาท";
            }

            if (strSatang == "00")
            {
                //BahtText += "ถ้วน";
            }
            else
            {
                BahtText += ".";
                strLength = strSatang.Length;
                for (int i = 0; i < strSatang.Length; i++)
                {
                    string number = strSatang.Substring(i, 1);
                    //if (number != "0")
                    //{
                    //if (i == strLength - 1 && number == "1" && strSatang[0].ToString() != "0")
                    //{
                    //    BahtText += "เอ็ด";
                    //}
                    //else if (i == strLength - 2 && number == "2" && strSatang[0].ToString() != "0")
                    //{
                    //    BahtText += "ยี่";
                    //}
                    //else if (i != strLength - 2 || number != "1")
                    //{
                    BahtText += strThaiNumber[int.Parse(number)];
                    //}

                    //}
                }


            }

            return BahtText;
        }

        /// <summary>
        /// SentEmail
        /// </summary>
        /// <param name="HostMail"></param>
        /// <param name="Port"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="email"></param>
        /// <param name="FromMail"></param>
        /// <param name="MailCC"></param>
        /// <param name="DisplayFromMail"></param>
        /// <param name="PathFile"></param>
        /// <param name="PathFile2"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static JsonResponse SentEmail(string HostMail,string Port, string Subject, string Body, string email, string FromMail, List<string> MailCC, string DisplayFromMail, string PathFile=null, string PathFile2=null,string[] FilePath=null)
        {
            var jsonReturn = new JsonResponse();
            try
            {
                using (System.Net.Mail.MailMessage Mail = new System.Net.Mail.MailMessage())
                {
                    Mail.To.Add(email);
                    Mail.From = new MailAddress(FromMail, DisplayFromMail);
                    if (MailCC != null)
                    {
                        foreach (var cc in MailCC)
                        {
                            MailAddress copy = new MailAddress(cc);
                            //MailAddress copy = new MailAddress(cc, "Pink");
                            Mail.CC.Add(copy);
                        }
                    }


                    Mail.BodyEncoding = System.Text.Encoding.UTF8;
                    Mail.Subject = Subject;
                    Mail.Body = Body;
                    Mail.IsBodyHtml = true;
                    Mail.Priority = MailPriority.High;
                    var filename = PathFile;
                    if (filename != null)
                    {
                        Mail.Attachments.Add(new Attachment(filename));
                    }
                    var filename2 = PathFile2;
                    if (filename2 != null)
                    {
                        Mail.Attachments.Add(new Attachment(filename2));
                    }

                    foreach(var r in FilePath)
                    {
                        if (r != null)
                        {
                            Mail.Attachments.Add(new Attachment(r));
                        }
                    }

                    Mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = HostMail;
                    smtp.Port = Convert.ToInt32(Port);
                    smtp.Send(Mail);

                }

                jsonReturn = new JsonResponse { status = true, message = "ok", data = null };

            }
            catch (Exception ex)
            {
                string err = ex.InnerException?.Message ?? ex.Message;
                Log.Error(MethodBase.GetCurrentMethod().Name + " Error -> " + err);
                jsonReturn = new JsonResponse { status = false, message = err };
            }

            return jsonReturn;


        }

    }
    public class JsonResponse
    {
        public bool status { get; set; }
        public object data { get; set; }
        public int total { get; set; }
        public string message { get; set; }
    }

    public class SelectOption
    {
        public object Value { get; set; }
        public string Text { get; set; }
    }

    public class JsonDataTable
    {
        public bool status { get; set; }
        public string message { get; set; }
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public object data { get; set; }
    }

    public class FilterModel
    {
        public int CurrentPage { get; set; }
        public string Query { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Database { get; set; }
        public List<string> Status { get; set; }
        public int Docnum { get; set; }
        public string Disable { get; set; }
        public List<string> ProvinceCode { get; set; }
        public List<string> UserGroupCode { get; set; }//pink20171120
        public List<string> DocType { get; set; }
        public List<string> Product { get; set; }
        public List<string> Size { get; set; }
        public List<string> OrderNo { get; set; }

        public List<string> Supplier { get; set; }
        public string Query2 { get; set; }
        public string InOut { get; set; }
        public DateTime? ExpiredDateStart { get; set; }
        public DateTime? ExpiredDateEnd { get; set; }
        public string DealerTypecode { get; set; }
        public List<int> Year { get; set; }
        public List<int> Month { get; set; }
        public string Branch_Code { get; set; }
        public string Company_Code { get; set; }
        public string StockCode { get; set; }
        public string Color_Code { get; set; }
        public List<string> ColorCode { get; set; }

        public FilterModel()
        {
            CurrentPage = 1;
            Status = new List<string>();
            ProvinceCode = new List<string>();
            UserGroupCode = new List<string>();//pink20171120
            DocType = new List<string>();
            Product = new List<string>();
            ColorCode = new List<string>();
            Supplier = new List<string>();
            OrderNo = new List<string>();
            Size = new List<string>();
            Year = new List<int>();
            Month = new List<int>();



        }
    }

    public class DatableOption : FilterModel
    {
        public int start { get; set; }
        public int length { get; set; }
        public int draw { get; set; }
        public List<Dictionary<string, string>> order { get; set; }
        private int sorting => int.Parse(order[0].FirstOrDefault(r => r.Key == "column").Value);
        private string dir => order[0].FirstOrDefault(r => r.Key == "dir").Value;

        public string ApplySorting(string[] column)
        {
            if (length == -1)
                length = 100;

            var s = sorting;
            if (s >= column.Length)
                s = column.Length - 1;
            return $"{column[s]} {dir}";
        }
    }

    public class NumberToEnglish
    {
        public String changeNumericToWords(double numb)
        {
            String num = numb.ToString();
            return changeToWords(num, false);
        }
        public String changeCurrencyToWords(String numb)
        {
            return changeToWords(numb, true);
        }
        public String changeNumericToWords(String numb)
        {
            return changeToWords(numb, false);
        }
        public String changeCurrencyToWords(double numb)
        {
            return changeToWords(Math.Round(numb, 2).ToString("0.00"), true);
        }
        private String changeToWords(String numb, bool isCurrency)
        {
            String val = "", wholeNo = numb, points = string.Empty, andStr = string.Empty, pointStr = string.Empty;
            String endStr = (isCurrency) ? ("Only") : ("");
            try
            {
                int decimalPlace = numb.IndexOf(".");
                int negative = numb.IndexOf("-");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = (isCurrency) ? ("and") : ("point");// just to separate whole numbers from points/cents
                        endStr = (isCurrency) ? ("Stang  " + endStr) : ("");
                        pointStr = tens(points);
                    }
                }
                if (negative > -1)
                {
                    wholeNo = numb.Substring(negative + 1);
                }
                val = String.Format("{0} {1} {2} {3} {4}", translateWholeNumber(wholeNo).Trim(), "Baht", andStr, pointStr, endStr);
            }
            catch {; }
            return val;
        }
        private String translateWholeNumber(String number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX
                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = number.StartsWith("0");
                    int numDigits = number.Length;
                    int pos = 0;//store digit grouping
                    String place = "";//digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {
                        case 1://ones' range
                            word = ones(number);
                            isDone = true;
                            break;
                        case 2://tens' range
                            word = tens(number);
                            isDone = true;
                            break;
                        case 3://hundreds' range
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range
                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)
                        word = translateWholeNumber(number.Substring(0, pos)) + place + translateWholeNumber(number.Substring(pos));
                        //check for trailing zeros
                        if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch {; }
            return word.Trim();
        }
        private String tens(String digit)
        {
            int digt = Convert.ToInt32(digit);
            String name = null;
            switch (digt)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (digt > 0)
                    {
                        name = tens(digit.Substring(0, 1) + "0") + " " + ones(digit.Substring(1));
                    }
                    break;
            }
            return name;
        }
        private String ones(String digit)
        {
            int digt = Convert.ToInt32(digit);
            String name = "";
            switch (digt)
            {
                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }
        private String translateCents(String cents)
        {
            String cts = "", digit = "", engOne = "";
            for (int i = 0; i < cents.Length; i++)
            {
                digit = cents[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ones(digit);
                }
                cts += " " + engOne;
            }
            return cts;
        }
    }

}
