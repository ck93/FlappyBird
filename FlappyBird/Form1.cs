using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Media;

namespace FlappyBird
{
    public partial class Form1 : Form
    {
        const int TIME_INTERVAL = 2;
        const int GRAVITY = 1;
        const int BIRD_X = 119;
        const int BIRD_WIDTH = 30;
        const int BIRD_HEIGHT = 30;
        const int SCENE_HEIGHT = 350;
        const int SCENE_WIDTH = 350;
        const int PIPE_WIDTH = 30;
        const int PIPE_INTERVAL = 105;
        const int PIPE_IMG_HEIGHT = 250;
        static int[] GAP = new int[3] { 100, 90, 80 };
        static int[] MAX = new int[3] { 210, 220, 230 };
        static int[] MIN = new int[3] { 80, 70, 60 };
        Bitmap[] bird = new Bitmap[6];
        Bitmap pipe_up = new Bitmap(Properties.Resources.pipe_up);
        Bitmap pipe_down = new Bitmap(Properties.Resources.pipe_down);
        Bitmap ground = new Bitmap(Properties.Resources.ground);
        Bitmap background = new Bitmap(Properties.Resources.background);
        Bitmap scene;
        int gap = GAP[1];
        int max = MAX[1];
        int min = MIN[1];
        int bird_y = SCENE_HEIGHT / 2;        
        int v = 0;
        bool ready = false;
        bool valid = true;
        bool gameOver = false;
        bool setting = false;
        bool nameChanged = false;
        List<int> pipeHeight = new List<int>();
        int pipe_x = 350;
        int groundShift = 0;
        int score = 0;
        int count = 0;        
        string name = "Player";
        string configFile = @".\config.bin";
        SoundPlayer sound1 = new SoundPlayer(@".\sounds\1.wav");
        SoundPlayer sound2 = new SoundPlayer(@".\sounds\2.wav");
        SoundPlayer sound3 = new SoundPlayer(@".\sounds\3.wav");
        SoundPlayer sound4 = new SoundPlayer(@".\sounds\4.wav");

        public Form1()
        {
            InitializeComponent();
            ReadResources();
            GenerateHeight();           
            label1.Parent = pictureBox1;
            panel1.Parent = pictureBox1;
            panel2.Parent = pictureBox1;
            panel3.Parent = pictureBox1;
            panel1.Visible = false;
            panel3.Visible = false;
            radioButton2.Checked = true;
            StreamReader sr = new StreamReader(configFile);
            sr.ReadLine();
            name = Encoding.Default.GetString(Convert.FromBase64String(sr.ReadLine()));
            textBox1.Text = name;
            sr.Close();
            timer2.Start();
            timer3.Start();
            
        }

        private void GenerateHeight()
        {
            pipeHeight.Clear();
            Random rd = new Random();
            for (int i = 0; i < 4; i++)
            {
                pipeHeight.Add(rd.Next(100, 210));
                Thread.Sleep(50);
            }
        }

        private void ReadResources()
        {
            bird[0] = new Bitmap(Properties.Resources.red_bird_up_2);
            bird[1] = new Bitmap(Properties.Resources.red_bird_up);
            bird[2] = new Bitmap(Properties.Resources.red_bird);
            bird[3] = new Bitmap(Properties.Resources.red_bird_down_1);
            bird[4] = new Bitmap(Properties.Resources.red_bird_down_2);
            bird[5] = new Bitmap(Properties.Resources.red_bird_down_3);
        }
        private void DrawBird()
        {            
            Graphics g = Graphics.FromImage(scene);
            if (v < -1)
                g.DrawImage(bird[0], BIRD_X, bird_y);
            else if (v < 3)
                g.DrawImage(bird[1], BIRD_X, bird_y);
            else if (v < 6)
                g.DrawImage(bird[2], BIRD_X, bird_y);
            else if (v < 9)
                g.DrawImage(bird[3], BIRD_X, bird_y);
            else if (v < 12)
                g.DrawImage(bird[4], BIRD_X, bird_y);
            else
                g.DrawImage(bird[5], BIRD_X, bird_y);
            pictureBox1.Image = scene;
            g.Dispose();
        }

        private void DrawPipes()
        {
            scene = (Bitmap)background.Clone();
            if (pipe_x < -PIPE_WIDTH)
            {
                Random rd = new Random();
                pipeHeight.Add(rd.Next(min, max));
                pipeHeight.RemoveAt(0);
                pipe_x += PIPE_INTERVAL;
            }            
            Graphics g = Graphics.FromImage(scene);
            for (int i = 0; i < 4; i++)
            {
                g.DrawImage(pipe_up, pipe_x + i * PIPE_INTERVAL, pipeHeight[i] - PIPE_IMG_HEIGHT);
                g.DrawImage(pipe_down, pipe_x + i * PIPE_INTERVAL, pipeHeight[i] + gap);
            }          
            g.Dispose();
        }

        private void DrawGround()
        {
            Graphics g = pictureBox2.CreateGraphics();
            g.DrawImage(ground, groundShift, 0);
        }

        private void DrawCollision()
        {
            scene = (Bitmap)background.Clone();
            Graphics g = Graphics.FromImage(scene);
            g.Clear(Color.White);
            pictureBox1.Image = scene;
            g.Dispose();
        }

        private bool CheckCollision()
        {
            if (bird_y + BIRD_HEIGHT > SCENE_HEIGHT)
            {
                return true;
            }
            for (int i = 0; i < 3; i++)
            {
                if (pipe_x + i * PIPE_INTERVAL <= BIRD_X + BIRD_WIDTH && pipe_x + i * PIPE_INTERVAL + PIPE_WIDTH >= BIRD_X)
                {
                    if (pipeHeight[i] > bird_y || pipeHeight[i] + gap < bird_y + BIRD_HEIGHT)
                        return true;
                }
            }
            return false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {           
            bird_y += v * TIME_INTERVAL;
            if (bird_y > SCENE_HEIGHT - BIRD_HEIGHT)
            {
                bird_y = SCENE_HEIGHT - BIRD_HEIGHT;
                sound1.Play();                
                timer1.Stop();
                string highestScore = ReadTxt();
                if (score > Convert.ToInt32(highestScore))
                {
                    WriteTxt(score.ToString(), name);
                    Thread th = new Thread(Record);
                    th.Start();
                }
                label4.Text = score.ToString();
                label5.Text = ReadTxt();
                panel1.Visible = true;
                textBox1.ReadOnly = false;
                panel2.Visible = true;
            }
            else if (bird_y < 0)
                bird_y = 0;
            v += GRAVITY;
            if (valid)
                pipe_x -= 3;
            DrawPipes();
            DrawBird();
            count++;
            if (count == (SCENE_WIDTH - BIRD_X) / 3)
            {
                score = 1;
                Thread th = new Thread(PlayCoinSound);
                th.Start();
                label1.Text = score.ToString();
                count = 0;
            }
            if (count == PIPE_INTERVAL / 3 && score > 0)
            {
                score++;
                Thread th = new Thread(PlayCoinSound);
                th.Start();
                label1.Text = score.ToString();
                count = 0;
            }
            if (!gameOver)
            {                
                if (CheckCollision())
                {
                    valid = false;
                    gameOver = true;
                    DrawCollision();
                    Thread th = new Thread(PlayDropSound);
                    th.Start();
                }
            }
        }

        private void PlayDropSound()
        {
            sound1.PlaySync();
            sound4.Play();
        }

        private void PlayCoinSound()
        {
            sound2.PlaySync();
        }        

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && valid)
            {                
                if (!ready)
                {
                    ready = true;
                    panel2.Visible = false;
                    timer1.Start();
                    timer3.Stop();
                }
                v = -5;
                sound3.Play();
            }
            else if (e.KeyCode == Keys.R && !textBox1.Focused)
                Restart();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Restart();
        }

        private void Restart()
        {
            panel1.Visible = false;
            textBox1.ReadOnly = true;
            this.Focus();
            panel2.Visible = false;
            if (nameChanged)
            {
                ChangeName();
            }
            gameOver = false;
            valid = true;
            bird_y = SCENE_HEIGHT / 2;
            v = -2;
            pipe_x = SCENE_WIDTH;
            score = 0;
            count = 0;
            label1.Text = "0";
            timer1.Start();
            timer3.Stop();
        }

        private void ChangeName()
        {
            nameChanged = false;
            name = textBox1.Text;
            string highest = ReadTxt();
            WriteTxt(highest, name);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            groundShift -= 3;
            if (groundShift < -16)
                groundShift = 0;
            DrawGround();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Restart();
        }

        private void WriteTxt(string txt)
        {
            StreamWriter sw = new StreamWriter(configFile);
            sw.WriteLine(Convert.ToBase64String(Encoding.Default.GetBytes(txt)));
            sw.Close();
        }

        private void WriteTxt(string txt1, string txt2)
        {
            StreamWriter sw = new StreamWriter(configFile);
            sw.WriteLine(Convert.ToBase64String(Encoding.Default.GetBytes(txt1)));
            sw.WriteLine(Convert.ToBase64String(Encoding.Default.GetBytes(txt2)));
            sw.Close();
        }

        private string ReadTxt()
        {
            StreamReader sr = new StreamReader(configFile);
            string txt = Encoding.Default.GetString(Convert.FromBase64String(sr.ReadLine()));
            sr.Close();
            return txt;
        }

        #region 设置
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (setting)
            {
                setting = false;
                panel3.Visible = false;
            }
            else
            {
                setting = true;
                panel3.Visible = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                gap = GAP[0];
                min = MIN[0];
                max = MAX[0];
                setting = false;
                panel3.Visible = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                gap = GAP[1];
                min = MIN[1];
                max = MAX[1];
                setting = false;
                panel3.Visible = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                gap = GAP[2];
                min = MIN[2];
                max = MAX[2];
                setting = false;
                panel3.Visible = false;
            }
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            textBox1.ReadOnly = true;
            this.Focus();
            if (nameChanged)
            {
                ChangeName();
            }
        }
        #endregion

        private void timer3_Tick(object sender, EventArgs e)
        {
            scene = (Bitmap)background.Clone();
            DrawBird();
            bird_y += v * TIME_INTERVAL;
            v += GRAVITY;
            if (bird_y > 180)
                v = -5;
        }

        private void Record()
        {
            if (!FTP.FTPAvailable())
                return;
            FTP.Download(Directory.GetCurrentDirectory(), "flappy bird/records.txt", "records.txt", "59.66.133.208", "FlappyBird", "flappybird");
            StreamReader sr = new StreamReader(@".\records.txt");
            string[] record = new string[6];
            for (int i = 0; i < 6; i++)
                record[i] = sr.ReadLine();
            sr.Close();
            if (score > Convert.ToInt32(record[5]))
            {
                StreamWriter sw = new StreamWriter(@".\records.txt");
                if (score > Convert.ToInt32(record[1]))
                {
                    sw.WriteLine(name);
                    sw.WriteLine(score.ToString());
                    for (int i = 0; i < 4; i++)
                        sw.WriteLine(record[i]);
                }
                else if (score > Convert.ToInt32(record[3]))
                {
                    sw.WriteLine(record[0]);
                    sw.WriteLine(record[1]);
                    sw.WriteLine(name);
                    sw.WriteLine(score.ToString());
                    sw.WriteLine(record[2]);
                    sw.WriteLine(record[3]);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        sw.WriteLine(record[i]);
                    sw.WriteLine(name);
                    sw.WriteLine(score.ToString());
                }
                sw.Close();
                FTP.Delete("flappy bird", "records.txt", "59.66.133.208", "FlappyBird", "flappybird");
                FTP.Upload(@".\records.txt", "flappy bird", "59.66.133.208", "FlappyBird", "flappybird");
            }
            File.Delete(@".\records.txt");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            nameChanged = true;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
        }
    }
}
