//http://blogs.msdn.com/b/eternalcoding/archive/2013/10/29/how-to-use-specific-winrt-api-from-desktop-apps-capturing-a-photo-using-your-webcam-into-a-wpf-app.aspx
//を参考にしてにある以下のDLLを参照する
//  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll
//
//  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.Runtime.dll
//  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.Runtime.InteropServices.WindowsRuntime.dll
//  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.IO.dll
//
//  %ProgramFiles%\Windows Kits\8.1\References\CommonConfiguration\Neutral\にWindows.winmd
//
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

using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using System.IO;

namespace CSWPFCamera
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //利用可能なビデオデバイス(カメラ)を探す
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (devices.Count == 0)
            {
                MessageBox.Show("カメラが見つかりませんでした", "Error", MessageBoxButton.OK);
                return;
            }
            this.listBox1.ItemsSource = devices.ToArray();
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CaptureImage();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CaptureImage();
        }

        async void CaptureImage()
        {
            this.IsEnabled = false;
            try
            {
                var di = (DeviceInformation)listBox1.SelectedItem;
                if(di != null)
                {
                    using (MediaCapture mediaCapture = new MediaCapture())
                    {
                        mediaCapture.Failed += (s, e) =>
                        {
                            MessageBox.Show("キャプチャできませんでした:" + e.Message, "Error", MessageBoxButton.OK);
                        };

                        MediaCaptureInitializationSettings setting = new MediaCaptureInitializationSettings();
                        setting.VideoDeviceId = di.Id;//カメラ選択
                        setting.StreamingCaptureMode = StreamingCaptureMode.Video;
                        await mediaCapture.InitializeAsync(setting);

                        //調整しないと暗い場合があるので
                        var vcon = mediaCapture.VideoDeviceController;
                        vcon.Brightness.TrySetAuto(true);
                        vcon.Contrast.TrySetAuto(true);

                        var pngProperties = ImageEncodingProperties.CreatePng();
                        pngProperties.Width = (uint)image1.ActualWidth;
                        pngProperties.Height = (uint)image1.ActualHeight;

                        using (var randomAccessStream = new InMemoryRandomAccessStream())
                        {
                            await mediaCapture.CapturePhotoToStreamAsync(pngProperties, randomAccessStream);

                            randomAccessStream.Seek(0);

                            //ビットマップにして表示
                            var bmp = new BitmapImage();
                            using (System.IO.Stream stream = System.IO.WindowsRuntimeStreamExtensions.AsStream(randomAccessStream))
                            {
                                bmp.BeginInit();
                                bmp.CacheOption = BitmapCacheOption.OnLoad;
                                bmp.StreamSource = stream;
                                bmp.EndInit();
                            }

                            this.image1.Source = bmp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.IsEnabled = true;
        }
    }
}
