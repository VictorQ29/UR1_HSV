using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CountPixel2
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Thread _captureThread;
        private int _threshold = 150;
        private int hMin, hMax, sMin, sMax, vMin, vMax = 100;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(1);
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();
            thresholdTrackBar.Value = _threshold;
        }

        private void DisplayWebcam()
        {
            while (_capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();

                int newHeight = (frame.Size.Height * emguPictureBox.Size.Width) / frame.Size.Width;
                Size newSize = new Size(emguPictureBox.Size.Width, newHeight);
                CvInvoke.Resize(frame, frame, newSize);
                emguPictureBox.Image = frame.Bitmap;


                Mat grayFrame = new Mat();
                Mat binaryFrame = new Mat();
                CvInvoke.CvtColor(frame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayFrame, binaryFrame, _threshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                binaryPictureBox.Image = binaryFrame.Bitmap;


                Image<Gray, byte> binaryImage = binaryFrame.ToImage<Gray, byte>();

                List<int> PixelCount = new List<int>(); //list for the 7 slices
                int SliceWidth = binaryFrame.Width / 7;


                for (int slice = 0; slice < 7; slice++)
                {
                    int SliceXcoordinate = SliceWidth * slice;
                    PixelCount.Add(0);
                    for (int x = SliceXcoordinate; x < (SliceXcoordinate) + SliceWidth; x++)
                    {
                        for (int y = 0; y < binaryImage.Height; y++)
                        {
                            if (binaryImage.Data[y, x, 0] == 255)
                                PixelCount[slice]++;
                        }
                    }
                }

                Invoke(new Action(() =>
                {
                    PixelCountLabel1.Text = $"{PixelCount[0]}";
                    PixelCountLabel2.Text = $"{PixelCount[1]}";
                    PixelCountLabel3.Text = $"{PixelCount[2]}";
                    PixelCountLabel4.Text = $"{PixelCount[3]}";
                    PixelCountLabel5.Text = $"{PixelCount[4]}";
                    PixelCountLabel6.Text = $"{PixelCount[5]}";
                    PixelCountLabel7.Text = $"{PixelCount[6]}";
                }));

                //HSV code
                Mat HsvFrame = new Mat();
                CvInvoke.CvtColor(frame, HsvFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                Mat[] HsvChannels = HsvFrame.Split();

                Mat HueFilter = new Mat();
                CvInvoke.InRange(HsvChannels[0], new ScalarArray(hMin), new ScalarArray(hMax), HueFilter);
                Invoke(new Action(() => { HuePictureBox.Image = HueFilter.Bitmap; }));

                Mat SaturationFilter = new Mat();
                CvInvoke.InRange(HsvChannels[1], new ScalarArray(sMin), new ScalarArray(sMax), SaturationFilter);
                Invoke(new Action(() => { SaturationPictureBox.Image = SaturationFilter.Bitmap; }));

                Mat ValueFilter = new Mat();
                CvInvoke.InRange(HsvChannels[2], new ScalarArray(vMin), new ScalarArray(vMax), ValueFilter);
                Invoke(new Action(() => { ValuePictureBox.Image = ValueFilter.Bitmap; }));

                Mat MergedImage = new Mat();
                CvInvoke.BitwiseAnd(HueFilter, SaturationFilter, MergedImage);
                CvInvoke.BitwiseAnd(MergedImage, ValueFilter, MergedImage);
                Invoke(new Action(() => { RedLinePictureBox.Image = MergedImage.Bitmap; }));

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }

        private void thresholdTrackBar_Scroll(object sender, EventArgs e)
        {
            _threshold = thresholdTrackBar.Value;
        }

        private void HueTrackBar1_Scroll(object sender, EventArgs e)
        {
            hMin = HueTrackBar1.Value;
        }

        private void HueTrackBar2_Scroll(object sender, EventArgs e)
        {
            hMax = HueTrackBar2.Value;
        }

        private void SaturationTrackBar1_Scroll(object sender, EventArgs e)
        {
            sMin = SaturationTrackBar1.Value;
        }

        private void SaturationTrackBar2_Scroll(object sender, EventArgs e)
        {
            sMax = SaturationTrackBar2.Value;
        }

        private void ValueTrackBar1_Scroll(object sender, EventArgs e)
        {
            vMin = ValueTrackBar1.Value;
        }

        private void ValueTrackBar2_Scroll(object sender, EventArgs e)
        {
            vMax = ValueTrackBar2.Value;
        }
    }
}