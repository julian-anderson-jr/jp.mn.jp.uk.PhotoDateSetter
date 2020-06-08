using System;
using System.Collections.Generic;
using System.Linq;
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

namespace jp.mn.uk.PhotoDateSetter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string getPath = string.Empty;
        private string setPath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();

            string c1 = System.Environment.CurrentDirectory;
            string c2 = System.IO.Directory.GetCurrentDirectory();
            string c3 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string c4 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            System.Environment.CurrentDirectory = c4;
            System.IO.Directory.SetCurrentDirectory(c4);
            getPath = c4;
            setPath = c3;
            this.txtTarget.Text = getPath;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtTarget.Text) || !System.IO.Directory.Exists(this.txtTarget.Text))
            {
                MessageBox.Show("Not find the folder at '" + this.txtTarget.Text + "'", "Error");
                return;
            }
            if (string.IsNullOrEmpty(this.txtDest.Text) || !System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(this.txtDest.Text)))
            {
                MessageBox.Show("Not find the folder at '" + this.txtDest.Text + "'", "Error");
                return;
            }
            if (string.IsNullOrEmpty(this.txtDest.Text) || System.IO.File.Exists(this.txtDest.Text))
            {
                if (MessageBox.Show("Are you the File overwrite OK? '" + this.txtDest.Text + "'", "Warning", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return;
                }
            }
            try
            {
                using (System.IO.StreamWriter wr = new System.IO.StreamWriter(this.txtDest.Text, false))
                {
                    wr.WriteLine("Path,FileName,update,shotdate");
                    SetFileList(this.txtTarget.Text, wr);

                    MessageBox.Show("File list completed!", "Info");
                }
            }
            catch
            {
                MessageBox.Show("File list about!", "Error");
            }
        }

        /// <summary>
        /// フォルダ選択(Selectボタン)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fdg = new System.Windows.Forms.FolderBrowserDialog();

            string path = string.Empty;

            fdg.Description = "Select folder!";
            fdg.SelectedPath = getPath;
            fdg.ShowNewFolderButton = true;

            if (fdg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                getPath = fdg.SelectedPath;
                txtTarget.Text = getPath;
                DateTime now1 = DateTime.Now;
                this.txtDest.Text = System.IO.Path.Combine(setPath, now1.Year.ToString()
                    + (now1.Month < 10 ? "0" : "") + (now1.Month.ToString())
                    + (now1.Day < 10 ? "0" : "") + (now1.Day.ToString())
                    + (now1.Hour < 10 ? "0" : "") + (now1.Hour.ToString())
                    + (now1.Minute < 10 ? "0" : "") + (now1.Minute.ToString())
                    + (now1.Second < 10 ? "0" : "") + (now1.Second.ToString())
                    + ".csv");
            }

        }

        /// <summary>
        /// 画像パラメータ取得
        /// </summary>
        /// <param name="path">ファイルフルパス</param>
        /// <returns>撮影日時(DateTime)</returns>
        private DateTime GetPhotoDate(string path)
        {
            DateTime dt = DateTime.Now;
            try
            {
                //読み込む
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(path))
                {
                    //Exif情報を列挙する
                    foreach (System.Drawing.Imaging.PropertyItem item in bmp.PropertyItems)
                    {
                        //データの型を判断
                        if (item.Type == 2)
                        {
                            //ASCII文字の場合は、文字列に変換する
                            string val = System.Text.Encoding.ASCII.GetString(item.Value);
                            val = val.Trim(new char[] { '\0' });
                            if (item.Id == 0x0132)
                            {
                                string[] dttmp = Microsoft.VisualBasic.Strings.Split(val, " ");
                                string[] tm = { "00", "00", "00" };
                                if (dttmp.Length == 2)
                                {
                                    tm = Microsoft.VisualBasic.Strings.Split(dttmp[1], ":");
                                }
                                string[] day = Microsoft.VisualBasic.Strings.Split(dttmp[0], ":");
                                dt = DateTime.Parse(day[0] + "/" + day[1] + "/" + day[2] + " " + tm[0] + ":" + tm[1] + ":" + tm[2] + "");
                                break;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err;
                err = ex.Message;
                dt = DateTime.Now;
            }
            return dt;
        }

        /// <summary>
        /// ファイル取得
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="sw"></param>
        /// <returns></returns>
        private string SetFileList(string path1, System.IO.StreamWriter sw)
        {
            try
            {
                if (System.IO.Directory.Exists(path1))
                {
                    string[] subdir = System.IO.Directory.GetDirectories(path1);
                    if (subdir != null)
                    {
                        for (int i = 0; i < subdir.Length; i++)
                        {
                            System.IO.FileAttributes fas = System.IO.File.GetAttributes(subdir[i]);
                            bool bSystem =
                              ((fas & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) ||
                              ((fas & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden);
                            if (bSystem == false)
                            {
                                SetFileList(subdir[i], sw);
                            }
                        }
                    }
                    string[] files = System.IO.Directory.GetFiles(path1);
                    if (files != null)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            System.IO.FileAttributes fas = System.IO.File.GetAttributes(files[i]);
                            bool bSystem =
                              ((fas & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) ||
                              ((fas & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden);
                            if (bSystem == false && Microsoft.VisualBasic.Strings.UCase(System.IO.Path.GetExtension(files[i])) == ".JPG")
                            {
                                DateTime update = System.IO.File.GetLastWriteTime(files[i]);
                                DateTime shotupdate = GetPhotoDate(files[i]);
                                if (update != shotupdate)
                                {
                                    System.IO.File.SetLastWriteTime(files[i], shotupdate);
                                }
                                System.IO.FileInfo fi = new System.IO.FileInfo(files[i]);
                                string filesize = fi.Length.ToString();
                                fi = null;
                                string updatestr = "";
                                if (update != null)
                                {
                                    updatestr = update.Year.ToString() + "/" + update.Month.ToString() + "/" + update.Day.ToString() + " " + update.Hour.ToString() + ":" + (update.Minute < 10 ? "0" : "") + update.Minute.ToString() + ":" + (update.Second < 10 ? "0" : "") + update.Second.ToString();
                                }
                                string shotupdatestr = "";
                                if (shotupdate != null)
                                {
                                    shotupdatestr = shotupdate.Year.ToString() + "/" + shotupdate.Month.ToString() + "/" + shotupdate.Day.ToString() + " " + shotupdate.Hour.ToString() + ":" + (shotupdate.Minute < 10 ? "0" : "") + shotupdate.Minute.ToString() + ":" + (shotupdate.Second < 10 ? "0" : "") + shotupdate.Second.ToString();
                                }
                                string pathfile = System.IO.Path.GetDirectoryName(files[i]);
                                string filename = "";
                                string[] filepaths = files[i].Split(new Char[] { '\\' });
                                if (filepaths != null)
                                {
                                    filename = filepaths[filepaths.Length - 1];
                                }
                                sw.WriteLine(pathfile + "," + filename + "," + updatestr + "," + shotupdatestr);

                            }
                        }
                    }
                }
            }
            catch { }
            return path1;
        }

    }
}
