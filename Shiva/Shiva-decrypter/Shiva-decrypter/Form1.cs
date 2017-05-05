
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Management;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace shiva_decrypter
{
    public partial class Form1 : Form
    {
        string userName = Environment.UserName;
        string userDir = "C:\\Users\\";
        

        public Form1()
        {
            InitializeComponent();
        }

        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                     
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                        
                  
                }
            }

            return decryptedBytes;
        }

        public void DecryptFile(string file,string password)
        {

            byte[] bytesToBeDecrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            File.WriteAllBytes(file, bytesDecrypted);
            string extension = System.IO.Path.GetExtension(file);
            string result = file.Substring(0, file.Length - extension.Length);
            System.IO.File.Move(file, result);

        }

        public static List<string> GetNetworkShareFoldersList(string serverName)
        {
            List<string> shares = new List<string>();
            ConnectionOptions connectionOptions = new ConnectionOptions();
            ManagementScope scope = new ManagementScope("\\\\" + serverName + "\\root\\CIMV2", connectionOptions);
            scope.Connect();

            ManagementObjectSearcher worker = new ManagementObjectSearcher(scope, new ObjectQuery("select * from win32_mappedlogicaldisk"));

            foreach (ManagementObject share in worker.Get())
            {
                shares.Add(share["Name"].ToString());

            }
            return shares;
        }

        public void DecryptDirectory(string location)
        {
            
                string password = textBox1.Text;
                string[] files = Directory.GetFiles(location);
                string[] childDirectories = Directory.GetDirectories(location);
            try
            {    
                for (int i = 0; i < files.Length; i++)
                {
                    string extension = Path.GetExtension(files[i]);
                    if (extension.Length == 7)
                    {
                        DecryptFile(files[i], password);
                    }
                }
                for (int i = 0; i < childDirectories.Length; i++)
                {
                    DecryptDirectory(childDirectories[i]);
                    File.Delete(childDirectories[i] + "\\READ_IT.html");
                    
                }
                label3.Visible = true;
            }
            catch (Exception e) { 
            
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
             //Decrypt Local disk
            string path = "\\Desktop\\test";
            string fullpath = userDir + userName + path;
            DecryptDirectory(fullpath);
            try
            {
                File.Delete(fullpath + "\\READ_IT.html");
            }
            catch (Exception exc) { }

            //Decrypt network disks
            string serverName = Environment.GetEnvironmentVariable("computername");
            List<string> shares = GetNetworkShareFoldersList(serverName);
            foreach (string share in shares)
            {
                DecryptDirectory(share + "\\");

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
