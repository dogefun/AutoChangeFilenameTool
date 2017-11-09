using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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


namespace 自动改名
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            FastFilesChangeName();
        }
        

        private void FastFilesChangeName()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            Regex regex = new Regex("^(\\s|\\S)+\\.(jpg|png|JPG|PNG)+$");

            int i = 1;

            if(dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            Hashtable hashtable = new Hashtable();
            List<long> problemSize = new List<long>();
            HashSet<string> md5Set = new HashSet<string>();

            foreach (string file in Directory.GetFiles(dialog.SelectedPath))
            {

                if (!regex.IsMatch(file))
                {
                    continue;
                }
                var t_f = File.OpenRead(file);
                var size = t_f.Length;
                t_f.Close();
                if (hashtable.Contains(size))
                {
                    problemSize.Add(size);
                    var list = (List<string>)hashtable[size];
                    list.Add(file);
                    continue;
                }
                hashtable.Add(size,new List<string>() { file });
            }

            foreach(var size in problemSize)
            {
                foreach(var name in (List<string>)hashtable[size])
                {
                    if (!md5Set.Add(MD5File(name)))
                    {
                        File.Delete(name);
                        continue;
                    }
                }
            }

            foreach (string file in Directory.GetFiles(dialog.SelectedPath))
            {
                if (!regex.IsMatch(file))
                {
                    continue;
                }

                var fnS = file.Split('\\');
                var filename = fnS[fnS.Length - 1];
                var backname = filename.Split('.')[1];
                var s = file.Replace(filename, "tmpFileByACN_" + i++.ToString() + "." + backname);
                File.Move(file, s);
            }

            foreach (string file in Directory.GetFiles(dialog.SelectedPath))
            {
                File.Move(file, file.Replace("tmpFileByACN_", ""));
            }

            MessageBox.Show("修改了" + i.ToString() + "个数据");
        }

        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="fileName">要计算 MD5 值的文件名和路径</param>
        /// <returns>MD5 值16进制字符串</returns>
        private string MD5File(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash_byte = md5.ComputeHash(file);
            string str = System.BitConverter.ToString(hash_byte);
            str = str.Replace("-", "");
            file.Close();
            return str;
        }



    }
}
