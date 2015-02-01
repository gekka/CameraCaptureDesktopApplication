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

Public Class Form1

    Dim pictureBox1 As PictureBox
    Dim listBox1 As ListBox

    Sub New()
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        Me.Width = 600
        Me.Height = 600

        Dim panel1 = New System.Windows.Forms.Panel()
        panel1.Width = 100
        panel1.Dock = DockStyle.Left
        Me.Controls.Add(panel1)

        Dim label1 = New Label()
        label1.Text = "カメラ一覧"
        panel1.Controls.Add(label1)

        listBox1 = New ListBox()
        listBox1.Width = 100
        listBox1.Top = label1.Height
        listBox1.DisplayMember = "Name"
        AddHandler listBox1.SelectedIndexChanged, Sub(s, e)
                                                      CaptureImage()
                                                  End Sub
        panel1.Controls.Add(listBox1)

        Dim button1 = New Button()
        button1.Text = "Capture"
        button1.Top = listBox1.Top + listBox1.Height + 5
        button1.AutoSize = True
        AddHandler button1.Click, Sub(s, e)
                                      CaptureImage()
                                  End Sub
        panel1.Controls.Add(button1)

        pictureBox1 = New PictureBox()
        pictureBox1.Dock = DockStyle.Fill
        pictureBox1.BackColor = Color.White
        Me.Controls.Add(pictureBox1)

        AddHandler Me.Shown, AddressOf Form1_Shown
    End Sub

    Private Async Sub Form1_Shown(sender As Object, e As EventArgs)
        '利用可能なビデオデバイス(カメラ)を探す
        Dim devices = Await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)
        If (devices.Count = 0) Then

            MessageBox.Show("カメラが見つかりませんでした", "Error", MessageBoxButtons.OK)
            Return
        End If
        Me.listBox1.DataSource = devices.ToArray()

    End Sub


    Private Async Sub CaptureImage()
        Me.Enabled = False
        Try

            Dim di = DirectCast(listBox1.SelectedItem, DeviceInformation)

            Using mediaCapture1 As New MediaCapture()

                AddHandler mediaCapture1.Failed, Sub(s, errorEventArgs)
                                                     MessageBox.Show("キャプチャできませんでした:" + errorEventArgs.Message, "Error", MessageBoxButtons.OK)
                                                 End Sub

                Dim setting As New MediaCaptureInitializationSettings()
                setting.VideoDeviceId = di.Id 'カメラ選択
                setting.StreamingCaptureMode = StreamingCaptureMode.Video
                Await mediaCapture1.InitializeAsync(setting)

                Dim pngProperties = ImageEncodingProperties.CreatePng()
                pngProperties.Width = CType(pictureBox1.Width, UInteger)
                pngProperties.Height = CType(pictureBox1.Height, UInteger)

                Using randomAccessStream As New InMemoryRandomAccessStream()

                    Await mediaCapture1.CapturePhotoToStreamAsync(pngProperties, randomAccessStream)

                    randomAccessStream.Seek(0)

                    'ビットマップにして表示
                    Dim stream = System.IO.WindowsRuntimeStreamExtensions.AsStream(randomAccessStream)
                    Dim img = System.Drawing.Image.FromStream(stream)
                    Me.pictureBox1.Image = img
                End Using
            End Using

        Catch ex As Exception

            MessageBox.Show(ex.Message)
        End Try
        Me.Enabled = True
    End Sub
End Class
