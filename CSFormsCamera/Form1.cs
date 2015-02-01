//http://blogs.msdn.com/b/eternalcoding/archive/2013/10/29/how-to-use-specific-winrt-api-from-desktop-apps-capturing-a-photo-using-your-webcam-into-a-wpf-app.aspx
//を参考にしてにある以下のDLLを参照する
//  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll
//
//  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.Runtime.dll
//  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.Runtime.InteropServices.WindowsRuntime.dll
//
//  %ProgramFiles%\Windows Kits\8.1\References\CommonConfiguration\Neutral\にWindows.winmd
//も参照する。

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace CSFormsCamera
{
    public partial class Form1 : Form
    {
        PictureBox pictureBox1;
        ListBox listBox1;

        public Form1()
        {
            //InitializeComponent();

            this.Width = 600;
            this.Height = 600;

            var panel1 = new System.Windows.Forms.Panel();
            panel1.Width = 100;
            panel1.Dock = DockStyle.Left;
            this.Controls.Add(panel1);

            Label label1 = new Label();
            label1.Text = "カメラ一覧";
            panel1.Controls.Add(label1);

            listBox1 = new ListBox();
            listBox1.Width = 100;
            listBox1.Top = label1.Height;
            listBox1.DisplayMember = "Name";
            listBox1.SelectedIndexChanged += (s, e) => { CaptureImage(); };
            panel1.Controls.Add(listBox1);

            Button button1 = new Button();
            button1.Text = "Capture";
            button1.Top = listBox1.Top + listBox1.Height + 5;
            button1.AutoSize = true;
            button1.Click += (s, e) => { CaptureImage(); };
            panel1.Controls.Add(button1);

            pictureBox1 = new PictureBox();
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.BackColor = Color.White;
            this.Controls.Add(pictureBox1);

            this.Shown += Form1_Shown;
        }

        async void Form1_Shown(object sender, EventArgs e)
        {
            //利用可能なビデオデバイス(カメラ)を探す
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (devices.Count == 0)
            {
                MessageBox.Show("カメラが見つかりませんでした", "Error", MessageBoxButtons.OK);
                return;
            }
            this.listBox1.DataSource = devices.ToArray();
        }

        async void CaptureImage()
        {
            this.Enabled = false;
            try
            {
                var di = (DeviceInformation)listBox1.SelectedItem;

                using (MediaCapture mediaCapture = new MediaCapture())
                {
                    mediaCapture.Failed += (s, e) =>
                    {
                        MessageBox.Show("キャプチャできませんでした:" + e.Message, "Error", MessageBoxButtons.OK);
                    };

                    MediaCaptureInitializationSettings setting = new MediaCaptureInitializationSettings();
                    setting.VideoDeviceId = di.Id;//カメラ選択
                    setting.StreamingCaptureMode = StreamingCaptureMode.Video;
                    await mediaCapture.InitializeAsync(setting);

                    var pngProperties = ImageEncodingProperties.CreatePng();
                    pngProperties.Width = (uint)pictureBox1.Width; ;
                    pngProperties.Height = (uint)pictureBox1.Height;

                    using (var randomAccessStream = new InMemoryRandomAccessStream())
                    {
                        await mediaCapture.CapturePhotoToStreamAsync(pngProperties, randomAccessStream);

                        randomAccessStream.Seek(0);

                        //ビットマップにして表示
                        System.IO.Stream stream = System.IO.WindowsRuntimeStreamExtensions.AsStream(randomAccessStream);
                        var img = System.Drawing.Image.FromStream(stream);
                        this.pictureBox1.Image = img;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Enabled = true;
        }
    }
}
