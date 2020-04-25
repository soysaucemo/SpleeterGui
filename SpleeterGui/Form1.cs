using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

// 2019 Maken it so.
// https://www.youtube.com/c/makenitso

namespace SpleeterGui
{
    public partial class Form1 : Form
    {
        private string stem_count = "2";
        private string mask_extension = "zeros";
        private string storage = "";
        private int files_remain = 0;
        private List<string> files_to_process = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        public void LoadStuff()
        {
            txt_output_directory.Text = Properties.Settings.Default.output_location;
            storage = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpleeterGUI";
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (files_remain == 0)
            {
                if (txt_output_directory.Text == "")
                {
                    if (Translate_zhcn.Checked == false)
                    {
                        MessageBox.Show("Please select an output directory");
                    }
                    else {
                        MessageBox.Show("请选择保存路径");
                    }
                    return;
                }

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                files_remain = 0;
                foreach (string file in files)
                {
                    files_to_process.Add(file);
                    files_remain++;
                }

                progressBar1.Maximum = files_remain + 1;
                progressBar1.Value = 0;

                if (Translate_zhcn.Checked == false)
                {
                    textBox1.AppendText("Starting processing of all songs\r\n");
                    progress_txt.Text = "Starting..." + files_remain + " songs remaining";
                }
                else
                {
                    textBox1.AppendText("开始处理全部音乐\r\n");
                    progress_txt.Text = "开始处理，还有 " + files_remain + " 份文件未处理";
                }
                next_song();
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
        }


        private void next_song()
        {
            if (files_remain > 0)
            {
                //string pyPath = storage + @"\python\python.exe";
                string pyPath = @"python";
                progressBar1.Value = progressBar1.Value + 1;
                Console.WriteLine("starting " + files_to_process[0]);
                //System.IO.File.WriteAllText(storage + @"\config.json", get_config_string());
                if (Translate_zhcn.Checked == false)
                {
                    textBox1.AppendText("Processing " + files_to_process[0] + "\r\n");
                    progress_txt.Text = "Working..." + files_remain + " songs remaining";
                }
                else
                {
                    textBox1.AppendText("正在处理 " + files_to_process[0] + "\r\n");
                    progress_txt.Text = "正在处理，还有 " + files_remain + " 份文件未处理";
                }
                //ProcessStartInfo processStartInfo = new ProcessStartInfo(pyPath, @" -W ignore -m spleeter separate -i " + (char)34 + files_to_process[0] + (char)34 + " -o " + (char)34 + txt_output_directory.Text + (char)34 + " -p " + (char)34 + storage + @"\config.json" + (char)34);
                ProcessStartInfo processStartInfo = new ProcessStartInfo(pyPath, @" -m spleeter separate -i " + (char)34 + files_to_process[0] + (char)34 + " -o " + (char)34 + txt_output_directory.Text + (char)34 + " -p spleeter:" + stem_count + "stems" + (char)34);
                //processStartInfo.WorkingDirectory = storage;

                processStartInfo.UseShellExecute = false;
                processStartInfo.ErrorDialog = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;

                files_to_process.Remove(files_to_process[0]);

                Process process = new Process();
                process.StartInfo = processStartInfo;
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(ProcessExited);
                process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
                bool processStarted = process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            else
            {
                if (Translate_zhcn.Checked == false)
                {
                    progress_txt.Text = "idle";
                    textBox1.AppendText("Finished processing all songs\r\n");
                }
                else
                {
                    progress_txt.Text = "就绪";
                    textBox1.AppendText("已完成全部音乐分离处理\r\n");
                }
                progressBar1.Value = progressBar1.Maximum;
                System.Media.SystemSounds.Beep.Play();
            }
        }

        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    textBox1.AppendText(e.Data.TrimEnd('\r', '\n') + "\r\n");
                }
            }));
        }

        void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    textBox1.AppendText(e.Data.TrimEnd('\r', '\n') + "\r\n");
                }
            }));
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            Invoke((Action)(() =>
            {
                files_remain--;
                next_song();
            }));
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txt_output_directory.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.output_location = txt_output_directory.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                txt_output_directory.Text = "";
            }
        }

        public void runCmd(String command)
        {
            ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = command;
            Process cmd = Process.Start(cmdsi);
            while (!cmd.HasExited)
            {
                Application.DoEvents();
            }
            cmd.Close();
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private string get_config_string()
        {
            string readText = File.ReadAllText(storage + @"\" + stem_count + "stems.json");
            if (mask_extension == "average")
            {
                readText = readText.Replace("zeros", "average");
            }
            return readText;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            mask_extension = checkBox1.Checked ? "average" : "zeros";
        }



        private void spleeterGithubPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/deezer/spleeter");
        }

        private void makenItSoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/user/mitchellcj/videos");
        }

        private void stems2_Click(object sender, EventArgs e)
        {
            stem_count = "2";
        }

        private void stems4_Click(object sender, EventArgs e)
        {
            stem_count = "4";
        }

        private void stems5_Click(object sender, EventArgs e)
        {
            stem_count = "5";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        private void helpFAQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://makenweb.com/spleeter_help.php");
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadStuff();
        }

        private void parts_btn2_Click(object sender, EventArgs e)
        {
            if (Translate_zhcn.Checked == false)
            { 
                parts_label.Text = "Vocal + Accompaniment";
            }
            else
            { 
                parts_label.Text = "人声 + 伴奏"; 
            }
            parts_btn2.UseVisualStyleBackColor = false;
            parts_btn4.UseVisualStyleBackColor = true;
            parts_btn5.UseVisualStyleBackColor = true;
            stem_count = "2";
        }

        private void parts_btn4_Click(object sender, EventArgs e)
        {
            if (Translate_zhcn.Checked == false)
            {
                parts_label.Text = "Vocal + Bass + Drums + Other";
            }
            else
            {
                parts_label.Text = "人声 + 低音 + 打击乐 + 其他";
            }
            parts_btn2.UseVisualStyleBackColor = true;
            parts_btn4.UseVisualStyleBackColor = false;
            parts_btn5.UseVisualStyleBackColor = true;
            stem_count = "4";
        }

        private void parts_btn5_Click(object sender, EventArgs e)
        {
            if (Translate_zhcn.Checked == false)
            {
                parts_label.Text = "Vocal + Bass + Drums + Piano + Other";
            }
            else
            {
                parts_label.Text = "人声 + 低音 + 打击乐 + 钢琴 + 其他";
            }
            parts_btn2.UseVisualStyleBackColor = true;
            parts_btn4.UseVisualStyleBackColor = true;
            parts_btn5.UseVisualStyleBackColor = false;
            stem_count = "5";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (files_remain == 0)
            {
                openFileDialog2.ShowDialog();
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            if (files_remain == 0)
            {
                if (txt_output_directory.Text == "")
                {
                    if (Translate_zhcn.Checked == false)
                    {
                        MessageBox.Show("Please select an output directory");
                    }
                    else
                    {
                        MessageBox.Show("请选择保存路径");
                    }
                    return;
                }
                files_remain = 0;
                foreach (String file in openFileDialog2.FileNames)
                {
                    Debug.WriteLine("BBB" + file);
                    files_to_process.Add(file);
                    files_remain++;
                }
                
                progressBar1.Maximum = files_remain + 1;
                progressBar1.Value = 0;
                if (Translate_zhcn.Checked == false)
                {
                    textBox1.AppendText("Starting processing of all songs\r\n");
                    progress_txt.Text = "Starting..." + files_remain + " songs remaining";
                }
                else
                {
                    textBox1.AppendText("开始处理全部音乐\r\n");
                    progress_txt.Text = "开始处理，还有 " + files_remain + " 份文件未处理";
                }
                next_song();
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }


        private void 切换中文界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem.Text="文件";
            exitToolStripMenuItem.Text = "退出";
            helpToolStripMenuItem.Text = "帮助";
            label5.Text = "分离声部数量";
            checkBox1.Text = "启用全带宽（高质量）-此功能已屏蔽";
            button2.Text = "保存路径";
            label7.Text = "处理进度";
            label2.Text = "拖放文件至此处以开始处理（支持多文件）";
            label1.Text = "或";
            button1.Text = "选择文件";
            label3.Text = "分离音乐声部";
            label4.Text = "Windows桌面版";
            progress_txt.Text = "就绪";
            parts_label.Text = "人声 + 伴奏";
            Translate_zhcn.Checked = true;
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void modByAcFun我就是来打酱油的ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("感谢来自deezer的Spleeter!   https://github.com/deezer/spleeter \n\r感谢来自boy1dr的SpleeterGui！ https://github.com/boy1dr/SpleeterGui \n\r修改内容：屏蔽json文件读写，新增中文界面 \n\rmodByAcFun我就是来打酱油的", "关于");
        }
    }
}
