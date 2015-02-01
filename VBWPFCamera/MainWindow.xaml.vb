'http://blogs.msdn.com/b/eternalcoding/archive/2013/10/29/how-to-use-specific-winrt-api-from-desktop-apps-capturing-a-photo-using-your-webcam-into-a-wpf-app.aspx
'を参考にしてにある以下のDLLを参照する
'  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll
'
'  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.Runtime.dll
'  %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.Runtime.InteropServices.WindowsRuntime.dll
'
'  %ProgramFiles%\Windows Kits\8.1\References\CommonConfiguration\Neutral\にWindows.winmd
'も参照する。
' %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.Threading.Tasks.dll
' %ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Facades\System.IO.dll

Imports Windows.Devices.Enumeration
Imports Windows.Media.Capture
Imports Windows.Media.MediaProperties
Imports Windows.Storage.Streams

Class MainWindow
    Sub New()

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        AddHandler Me.Loaded, AddressOf MainWindow_Loaded
    End Sub


    Private Async Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        '利用可能なビデオデバイス(カメラ)を探す
        Dim devices = Await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)
        If (devices.Count = 0) Then

            MessageBox.Show("カメラが見つかりませんでした", "Error", MessageBoxButton.OK)
            Return
        End If
        Me.listBox1.ItemsSource = devices.ToArray()

    End Sub

    Private Sub listBox1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        CaptureImage()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        CaptureImage()
    End Sub

    Private Async Sub CaptureImage()
        Me.IsEnabled = False
        Try

            Dim di = DirectCast(listBox1.SelectedItem, DeviceInformation)

            Using mediaCapture1 As New MediaCapture()

                AddHandler mediaCapture1.Failed, Sub(s, errorEventArgs)
                                                     MessageBox.Show("キャプチャできませんでした:" + errorEventArgs.Message, "Error", MessageBoxButton.OK)
                                                 End Sub

                Dim setting As New MediaCaptureInitializationSettings()
                setting.VideoDeviceId = di.Id 'カメラ選択
                setting.StreamingCaptureMode = StreamingCaptureMode.Video
                Await mediaCapture1.InitializeAsync(setting)

                Dim pngProperties = ImageEncodingProperties.CreatePng()
                pngProperties.Width = CType(image1.ActualWidth, UInteger)
                pngProperties.Height = CType(image1.ActualHeight, UInteger)

                Using randomAccessStream As New InMemoryRandomAccessStream()

                    Await mediaCapture1.CapturePhotoToStreamAsync(pngProperties, randomAccessStream)

                    randomAccessStream.Seek(0)

                    'ビットマップにして表示
                    Dim bmp As New BitmapImage()
                    Using stream = System.IO.WindowsRuntimeStreamExtensions.AsStream(randomAccessStream)
                        bmp.BeginInit()
                        bmp.CacheOption = BitmapCacheOption.OnLoad
                        bmp.StreamSource = stream
                        bmp.EndInit()
                    End Using
                    Me.image1.Source = bmp
                End Using
            End Using
        Catch ex As Exception

            MessageBox.Show(ex.Message)
        End Try
        Me.IsEnabled = True
    End Sub
End Class
