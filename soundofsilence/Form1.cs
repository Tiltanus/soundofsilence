using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;
using NAudio;
using System.IO;

namespace soundofsilence
{
    public partial class Form1 : Form
    {
        //WaveIn - thread for record
        WaveIn waveIn;
        //Class for record in file
        WaveFileWriter writer;
        //files
        string outputFileName = "C:/demo.wav";
        string inputFileName = "C:/demo2.wav";
        //device number
        int deviceNumber = 0;
        //output device
        WaveOutEvent outputDevice;
        //output stream
        AudioFileReader audioFile;
        //recording flag
        bool isRecording;
        //array of max samples
        float[] maxsamples;


        public Form1()
        {
            InitializeComponent();
        }

        //insert data from in-buffer
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                if (isRecording)
                {
                    writer.Write(e.Buffer, 0, e.BytesRecorded);
                }
                

                float max = 0;
                //interpret as 16 bit audio
                maxsamples = new float[e.BytesRecorded];
                for (int index = 0; index< e.BytesRecorded; index += 2)
                {
                    short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
                    //to floating point
                    var sample32 = sample / 32768f;
                    //absolute value
                    if (sample32 < 0) sample32 = -sample32;
                    //is this the max value?
                    if (sample32 > max) max = sample32;
                    volumeMeter1.Amplitude = 100 * max;
                    maxsamples[index] = max;
                }
                var byteArray = new byte[maxsamples.Length * 4];
                Buffer.BlockCopy(maxsamples, 0, byteArray, 0, byteArray.Length);
                using (FileStream fstream = new FileStream("C:/demo.txt", FileMode.Open))
                {
                    fstream.Write(byteArray, 0, byteArray.Length);
                }
            }
        }

        void StopRecording()
        {
            label1.BackColor = Color.Red;
            label1.Text = "Stopped";
            waveIn.StopRecording();
        }

        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                isRecording = false;
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }

        private void OnPlayBackStopped(object sender, StoppedEventArgs e)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            
            try
            {
                isRecording = true;
                label1.BackColor = Color.Lime;
                label1.Text = "Recording";
                waveIn = new WaveIn();
                //Дефолтное устройство для записи (если оно имеется)
                //встроенный микрофон ноутбука имеет номер 0
                waveIn.DeviceNumber = deviceNumber;
                //Прикрепляем к событию DataAvailable обработчик, возникающий при наличии записываемых данных
                waveIn.DataAvailable += waveIn_DataAvailable;
                //Прикрепляем обработчик завершения записи
                waveIn.RecordingStopped += waveIn_RecordingStopped;
                //Формат wav-файла - принимает параметры - частоту дискретизации и количество каналов(здесь mono)
                waveIn.WaveFormat = new WaveFormat(44100, 1);
                //Инициализируем объект WaveFileWriter
                writer = new WaveFileWriter(outputFileName, waveIn.WaveFormat);
                //Начало записи
                waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (waveIn != null)
            {
                StopRecording();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            deviceNumber = 0;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            deviceNumber = 1;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            outputFileName = textBox1.Text;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlayBackStopped;
            }
            if (audioFile == null)
            {
                audioFile = new AudioFileReader(inputFileName);
                outputDevice.Init(audioFile);
            }
            outputDevice.Play();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            outputDevice?.Stop();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            outputDevice?.Pause();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            inputFileName = textBox2.Text;
        }
    }
}
