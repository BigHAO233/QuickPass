using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickPass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string key;
        string lastContent;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            key = keyBox.Text;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Clipboard.SetData(DataFormats.Text, "``" + DESEncryption.Encrypt(GetRtf(), key));
                contentBox.Document.Blocks.Clear();
            }

            /*if (e.Key == Key.F6)
            {
                string content = Clipboard.GetText();
                if (content[0] == '`' && content[1] == '`')
                {
                    content = content.Substring(2);
                    contentBox.Document.Blocks.Clear();
                    SetRtf(DESEncryption.Decrypt(content, key));
                }
            }*/
        }

        string GetRtf()
        {
            string rtf = string.Empty;
            TextRange textRange = new TextRange(contentBox.Document.ContentStart, contentBox.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                textRange.Save(ms, System.Windows.DataFormats.Rtf);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(ms);
                rtf = sr.ReadToEnd();
            }

            return rtf;
        }

        void SetRtf(string rtf)
        {
            if (string.IsNullOrEmpty(rtf))
            {
                throw new ArgumentNullException();
            }
            TextRange textRange = new TextRange(contentBox.Document.ContentStart, contentBox.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.Write(rtf);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    textRange.Load(ms, DataFormats.Rtf);
                }
            }
        }

        private void contentBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Clipboard.GetDataObject().GetFormats().Length != 0 && Clipboard.GetDataObject().GetFormats()[0] == DataFormats.Text)
            {
                string content = Clipboard.GetText();
                if (content[0] == '`' && content[1] == '`')
                {
                    if (content == lastContent)
                        return;
                    lastContent = content;
                    content = content.Substring(2);
                    contentBox.Document.Blocks.Clear();
                    SetRtf(DESEncryption.Decrypt(content, key));
                }
            }
        }
    }

    /// <summary>
    /// DES加密解密类
    /// </summary>
    public class DESEncryption
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="content">加密内容</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string Encrypt(string content, string key)
        {
            byte[] inputByteArray = Encoding.UTF8.GetBytes(content);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            MemoryStream mStream = new MemoryStream();
            des.Key = ASCIIEncoding.UTF8.GetBytes(key);
            des.IV = ASCIIEncoding.UTF8.GetBytes(key);
            CryptoStream cStream = new CryptoStream(mStream, des.CreateEncryptor(), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="content">解密内容</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string Decrypt(string content, string key)
        {
            byte[] inputByteArray = Convert.FromBase64String(content);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            MemoryStream mStream = new MemoryStream();
            des.Key = ASCIIEncoding.UTF8.GetBytes(key);
            des.IV = ASCIIEncoding.UTF8.GetBytes(key);
            CryptoStream cStream = new CryptoStream(mStream, des.CreateDecryptor(), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            try
            {
                cStream.FlushFinalBlock();
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(mStream.ToArray());
        }
    }
}
