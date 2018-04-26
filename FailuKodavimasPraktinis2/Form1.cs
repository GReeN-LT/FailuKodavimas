using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Threading;


namespace FailuKodavimasPraktinis2
{
    public partial class Form1 : Form
    {

        private ManualResetEvent mre;
        private Thread thread;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Visible = false;
            label3.Visible = false;
            textBox1.UseSystemPasswordChar = true;

        }


        private void button1_Click(object sender, EventArgs e)
        {
            //pasirinkt kataloga
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
               
                string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
               
                label2.Visible = true;
                label3.Visible = true;
                label2.Text = "Pasirinkta: " + folderBrowserDialog1.SelectedPath;
                label3.Text = "Failų rasta: " + files.Length.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //koduot failus
            // zip iskviest

            if (thread == null || thread?.IsAlive == false)
            {
                thread = new Thread(new ThreadStart(zipKodavimas));
                thread.Start();
                thread.IsBackground = true;
                mre = new ManualResetEvent(true);
               
            }

        }


    

        private void button3_Click(object sender, EventArgs e)
        {
            //atkoduoti failus

            if (thread == null || thread?.IsAlive == false)
            {
                thread = new Thread(new ThreadStart(unZipAtodavimas));
                thread.Start();
                thread.IsBackground = true;
                mre = new ManualResetEvent(true);
               
            }

        }

        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

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

        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

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

        public void zipKodavimas()
        {

            this.BeginInvoke((MethodInvoker)delegate
            {
                progressBar1.Value = 0;
            });

            string startPath = folderBrowserDialog1.SelectedPath;

            mre.WaitOne();


            string zipPath = folderBrowserDialog1.SelectedPath + ".zip";

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            mre.WaitOne();

            ZipFile.CreateFromDirectory(startPath, zipPath);

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            string file = folderBrowserDialog1.SelectedPath + ".zip";

            mre.WaitOne();

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });


            

            DirectoryInfo di = new DirectoryInfo(folderBrowserDialog1.SelectedPath);


            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            //delete files after zip 
            foreach (FileInfo files in di.GetFiles())
            {
                files.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            //delete files
            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            mre.WaitOne();

            string password = textBox1.Text;

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });


            byte[] bytesToBeEncrypted = File.ReadAllBytes(file); //turi rast kodavimas.zip desktope
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            mre.WaitOne();

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            passwordBytes = MD5.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            string hashPassword = BitConverter.ToString(passwordBytes).Replace("-", ""); //hash reiksme
            //MessageBox.Show(hashPassword);

            using (StreamWriter writer =
            new StreamWriter("hash.txt"))
            {

                writer.WriteLine(hashPassword);

            }

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            mre.WaitOne();

            //MessageBox.Show(MD5.Create().ComputeHash(passwordBytes).ToString());

            //deletint specifini faila
            File.Delete(folderBrowserDialog1.SelectedPath + ".zip");

            //deletint specifini faila

            string fileEncrypted = folderBrowserDialog1.SelectedPath + "Coded.zip"; //uzkoduotas failas

            File.WriteAllBytes(fileEncrypted, bytesEncrypted);

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            MessageBox.Show("Failai užkoduoti!");
        }

        void unZipAtodavimas()
        {

            this.BeginInvoke((MethodInvoker)delegate
            {
                progressBar1.Value = 0;
            });


            string fileEncrypted = folderBrowserDialog1.SelectedPath + "Coded.zip"; //uzkoduotas failas

            string password = textBox1.Text;

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            textBox1.UseSystemPasswordChar = true;

            mre.WaitOne();

            byte[] bytesToBeDecrypted = File.ReadAllBytes(fileEncrypted);


            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            passwordBytes = MD5.Create().ComputeHash(passwordBytes);

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            mre.WaitOne();

            string file = folderBrowserDialog1.SelectedPath + "UnCoded.zip"; //atkoduot ir išzipint 

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            File.WriteAllBytes(file, bytesDecrypted);

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            mre.WaitOne();

            //unzip
            // ZipFile.ExtractToDirectory(folderBrowserDialog1.SelectedPath + "UnCoded.zip", "C:\\Users\\Mindaugas Laboga\\Desktop\\atkoduotiFailai"); //alredy exist 
            ZipFile.ExtractToDirectory(folderBrowserDialog1.SelectedPath + "UnCoded.zip", folderBrowserDialog1.SelectedPath); //alredy exist 

            //unzip
            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });


            File.Delete(folderBrowserDialog1.SelectedPath + "UnCoded.zip");

            mre.WaitOne();

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            File.Delete(folderBrowserDialog1.SelectedPath + "Coded.zip");

            this.BeginInvoke((MethodInvoker)delegate
            {
                updateProgressBar();
            });

            MessageBox.Show("Failai atkoduoti!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //isejimas

            Application.Exit();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            //pauze

            mre.Reset(); 


        }

        private void button6_Click(object sender, EventArgs e)
        {
            //testi

            mre.Set();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //stabdyti
            thread.Abort();
            
        }

        void updateProgressBar()
        {
            progressBar1.Value += 10;

        }

    }
}
