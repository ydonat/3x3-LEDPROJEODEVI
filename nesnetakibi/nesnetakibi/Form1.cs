using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using System.IO;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.IO.Ports;

namespace nesnetakibi
{
    public partial class Form1 : Form
    {
        string[] portlar = SerialPort.GetPortNames();
        public Form1()
        {
            InitializeComponent();
        }
        Graphics g;
        Bitmap video, video2;
        int mode;
        int yes, mav, kir;
        bool OnOff = false;
        int derlenmezamanı = 5;
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;

        private void Form1_Load(object sender, EventArgs e)
        {

            foreach (string port in portlar)
            {
                comboBox2.Items.Add(port);
                comboBox2.SelectedIndex = 0;
            }
            label4.Text = "Bağlantı kapalı!";
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = 0;
            FinalFrame = new VideoCaptureDevice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += FinalFrame_NewFrame;
            FinalFrame.Start();
        }

        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video = (Bitmap)eventArgs.Frame.Clone();
            video2 = (Bitmap)eventArgs.Frame.Clone();
            Mirror filter = new Mirror(false, true);
            filter.ApplyInPlace(video);
            filter.ApplyInPlace(video2);

            switch (mode)
            {
                case 2:
                    {
                        g = Graphics.FromImage(video2);
                        g.DrawString(derlenmezamanı.ToString(), new Font("Arial", 20), new SolidBrush(Color.White), new PointF(2, 2));
                        g.Dispose();
                    }
                    break;
                case 1:
                    {



                        ColorFiltering ColorFilter = new ColorFiltering();
                        ColorFilter.ApplyInPlace(video2);

                        ColorFilter.Red = new IntRange(kir, (int)numericUpDown2Red.Value);
                        ColorFilter.Green = new IntRange(yes, (int)numericUpDown3Green.Value);
                        ColorFilter.Blue = new IntRange(mav, (int)numericUpDown1Blue.Value);
                        ColorFilter.ApplyInPlace(video2);
                        ColorFilter.ApplyInPlace(video);


                        BlobCounter blobcounter = new BlobCounter();
                        blobcounter.MinHeight = 10;
                        blobcounter.MinWidth = 10;
                        blobcounter.ObjectsOrder = ObjectsOrder.Size;
                        blobcounter.ProcessImage(video2);
                        Rectangle[] rect = blobcounter.GetObjectsRectangles();

                        if (rect.Length > 0)
                        {
                            Rectangle Object = rect[0];
                            Graphics graphics = Graphics.FromImage(video2);

                            using (Pen pen = new Pen(Color.White, 3))
                            {
                                graphics.DrawRectangle(pen, Object);
                            }


                            int uzunluk = video.Width;
                            int a = video.Height;
                            int ObjectX = Object.X + (Object.Width / 2);
                            int ObjectY = Object.Y + (Object.Height / 2);
                            
                            if (ObjectX > 0 && ObjectX < 213 && ObjectY > 0 && ObjectY < 160)
                            {

                                Console.WriteLine("nesne sol köşede" + ObjectX + "," + ObjectY);
                                serialPort1.Write("1");

                            }

                            if (ObjectX > 213 && ObjectX < 426 && ObjectY > 0 && ObjectY < 160)
                            {
                                Console.WriteLine("nesne ortada kor" + ObjectX + "," + ObjectY);
                                serialPort1.Write("2");

                            }
                            if (ObjectX > 426 && ObjectX < 640 && ObjectY > 0 && ObjectY < 160  )
                            {
                                Console.WriteLine("nesne sağ kösede" + ObjectX + "," + ObjectY);
                                serialPort1.Write("3");

                            }
                            if (ObjectX > 0 && ObjectX < 213 && Object.Y > 160 && ObjectY < 320)
                            {
                                Console.WriteLine("nesne sol ortada" + ObjectX + "," + ObjectY);
                                serialPort1.Write("4");

                            }
                            if (ObjectX > 213 && ObjectX < 426 && Object.Y > 160 && ObjectY < 320)
                            {
                                Console.WriteLine("nesne  ortada" + ObjectX + "," + ObjectY);
                                serialPort1.Write("5");

                            }
                            if (ObjectX > 426 && ObjectX < 640 && ObjectY > 160 && ObjectY < 320)
                            {
                                Console.WriteLine("nesne  sağ ortada" + ObjectX + "," + ObjectY);
                                serialPort1.Write("6");

                            }
                            if (ObjectX > 0 && ObjectX < 213 && ObjectY > 320 && ObjectY < 480)
                            {
                                Console.WriteLine("nesne sol altta" + ObjectX + "," + ObjectY);
                                serialPort1.Write("7");

                            }
                            if (ObjectX > 213 && ObjectX < 426 && ObjectY > 320 && ObjectY < 480)
                            {
                                Console.WriteLine("nesne alt ortada" + ObjectX + "," + ObjectY);
                                serialPort1.Write("8");

                            }
                            if (ObjectX > 426 && ObjectX < 640 && ObjectY > 320 && Object.Y < 480)
                            {
                                Console.WriteLine("sağ alt köşede" + ObjectX + "," + ObjectY);
                                serialPort1.Write("9");

                            }
                            graphics.Dispose();
                        }
                        
                    }
                    pictureBox2.Image = video2;
                    break;
                                      
            }
             pictureBox1.Image = video;
    
        }
       

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FinalFrame.IsRunning == true)
            {
                FinalFrame.Stop();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = (Bitmap)pictureBox1.Image.Clone();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            derlenmezamanı--;
            if (derlenmezamanı == 0)
            {
                timer1.Enabled = false;
                OnOff = false;
                pictureBox2.Image = video;
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            serialPort1.BaudRate = 9600;
            serialPort1.PortName = comboBox2.Text;
            serialPort1.Open();
            label4.Text="Bağlantı açık !";
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen==true)
            {
                serialPort1.Close();
                label5.Text = "Bağlantı kapalı !";
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            serialPort1.Write("1");
            label4.Text = "LED YANDI";
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            kir = (int)trackBar1.Value;
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            yes = (int)trackBar2.Value;
        }
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            mav = (int)trackBar3.Value;
        }
        private void numericUpDown3Green_ValueChanged(object sender, EventArgs e)
        {

        }
        private void numericUpDown1Blue_ValueChanged(object sender, EventArgs e)
        {

        }
        private void button8_Click(object sender, EventArgs e)
        {
            serialPort1.Write("0");
            label5.Text = "LED SÖNDÜ";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            mode = 2;
        }
        private void btnTrackingObject_Click(object sender, EventArgs e)
        {
            mode = 1;
        }
    }
}
