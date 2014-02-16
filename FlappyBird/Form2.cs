using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

namespace FlappyBird
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            ReadRecord();
        }

        void ReadRecord()
        {
            if (!FTP.FTPAvailable())
            {
                MessageBox.Show("服务器好像出问题了哦，请稍后再查看排名~");
                return;
            }
            FTP.Download(Directory.GetCurrentDirectory(), "flappy bird/records.txt", "records.txt", "59.66.133.208", "FlappyBird", "flappybird");
            StreamReader sr = new StreamReader(@".\records.txt");
            string[] record = new string[6];
            for (int i = 0; i < 6; i++)
                record[i] = sr.ReadLine();
            sr.Close();
            label2.Text = record[0];
            label3.Text = record[2];
            label4.Text = record[4];
            label5.Text = record[1];
            label6.Text = record[3];
            label7.Text = record[5];
            File.Delete(@".\records.txt");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            GraphicsPath oPath = new GraphicsPath();
            int x = 0;
            int y = 0;
            int w = Width;
            int h = Height;
            int a = 40;
            Graphics g = CreateGraphics();
            oPath.AddArc(x, y, a, a, 180, 90);
            oPath.AddArc(w - a, y, a, a, 270, 90);
            oPath.AddArc(w - a, h - a, a, a, 0, 90);
            oPath.AddArc(x, h - a, a, a, 90, 90);
            oPath.CloseAllFigures();
            Region = new Region(oPath);
        }
    }
}
