Imports System.Net.NetworkInformation
Imports System.Reflection
Imports System.IO
Public Class FrmMain
    Implements IDisposable
    Private fileReader As String
    Private IP_LIST As Array
    Dim _th As Threading.Thread
    Private Property _str As String
    Private DateTemp As Date
    Public Property FileName As String
    Public Property DblTimeKind As Decimal = 1.0
    Public Property DblTimeLoop As Decimal = 1.0
    Public _PathFileLog As String
    Private _SenderValid As Boolean
    Private Ontime As DateTime
    Private _DoEvent As Boolean = False
    Private _TimeSetClear As DateTime
    Sub FrmMain_Close(sender As Object, e As EventArgs) Handles MyBase.Closing
        GCDispose(sender, e)
    End Sub
    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        fileReader = My.Computer.FileSystem.ReadAllText(My.Application.Info.DirectoryPath & "\" & IIf(FileName = "", "IP-LIST.txt", FileName & ".txt"))
        Me.Text = "By _Josky [" & IIf(FileName = "", "IP-LIST", FileName) & "]"
        IP_LIST = (fileReader.Split(vbNewLine))
        _th = New Threading.Thread(AddressOf Ping) With {.IsBackground = True}
        _th.Start()
        Label3.Text = Format(Now(), "dd-MM-yyyy HH:mm:ss")
        Clearlbox(ListBox1)
        Clearlbox(ListBox2)
        _PathFileLog = Nothing
        btnStart.Enabled = False
    End Sub
    Private Sub Ping()
        While _th.IsAlive
            Try
                DateTemp = Now
                For Each _IP As String In IP_LIST
                    Dim IpFill As String = New String(_IP.ToString.Where(Function(x) Not Char.IsWhiteSpace(x)).ToArray())
                    If IpFill = "" Then Continue For
                    If My.Computer.Network.Ping(IpFill) = True Then
                        Addlbox(ListBox1, IpFill & " - " & Format(Now(), "dd-MM-yy HH:mm:ss"))
                    Else
                        Addlbox(ListBox2, IpFill & " - " & Format(Now(), "dd-MM-yy HH:mm:ss"))
                        _str &= IpFill & " - " & Format(Now(), "dd-MM-yy HH:mm:ss") & vbNewLine
                    End If
                    If Array.LastIndexOf(IP_LIST, _IP) <> (IP_LIST.Length - 1) Then
                        Threading.Thread.Sleep(DblTimeKind * 1000) ' Interval time wait move next 
                    End If
                Next
                ChngeTxtObj(Label1, "[ " & Math.Round((Now - DateTemp).TotalSeconds, 2) & " s ] " & " [ loop : " & DblTimeLoop & " s ]")
                Threading.Thread.Sleep(DblTimeLoop * 1000) ' Interval time wait loop
            Catch ex As Exception
            End Try
            Clearlbox(ListBox1)
        End While
    End Sub
    Private Delegate Sub delAddlbox(ByRef lbox As ListBox, ByVal _str As String)
    Public Sub Addlbox(ByRef lbox As ListBox, ByVal _str As String)
        If Me.InvokeRequired Then
            Me.Invoke(New delAddlbox(AddressOf Addlbox), lbox, _str)
        Else
            lbox.Items.Add(_str)
        End If
    End Sub
    Private Delegate Sub delClearlbox(ByRef lbox As ListBox)
    Public Sub Clearlbox(ByRef lbox As ListBox)
        If Me.InvokeRequired Then
            Me.Invoke(New delClearlbox(AddressOf Clearlbox), lbox)
        Else
            lbox.Items.Clear()
        End If
    End Sub
    Private Delegate Sub delChngeTxtObj(ByRef _item As Object, ByVal _txt As String)
    Public Sub ChngeTxtObj(ByRef _item As Object, ByVal _txt As String)
        If Me.InvokeRequired Then
            Me.Invoke(New delChngeTxtObj(AddressOf ChngeTxtObj), _item, _txt)
        Else
            _item.text = _txt
        End If
    End Sub
    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        GCDispose(sender, e)
        _str = Nothing
        btnStart.Enabled = True
    End Sub
    Private Sub GCDispose(sender As Object, e As EventArgs)
        If Not _th Is Nothing Then
            If _th.IsAlive Then _th.Abort()
        End If
    End Sub
    Private Sub FrmMain_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        tboxSender.Text = GetSetting("Sender_Default")
        tboxReceiver.Text = GetSetting("Receiver_Default")
    End Sub
    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each foundFile As String In My.Computer.FileSystem.GetFiles(My.Application.Info.DirectoryPath, Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories, "IP-LIST*.txt")
            cboxIplist.Items.Add(Path.GetFileNameWithoutExtension(foundFile))
        Next
        dtPicker.Value = Now
    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If _str = "" Then Exit Sub
        Dim file As IO.StreamWriter
        _PathFileLog = My.Application.Info.DirectoryPath & "\Log\[" & Me.Text & "] " & Now.ToString("dd-MM-yyyy") & "_" & Now.ToString("HH_mm_ss") & ".txt"
        file = My.Computer.FileSystem.OpenTextFileWriter(_PathFileLog, True)
        file.WriteLine(_str)
        file.Close()
    End Sub
    Private Sub btnSetting_Click(sender As Object, e As EventArgs) Handles btnSetting.Click '
        Me.AutoSize = Not Me.AutoSize
        'Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        GroupBox3.Visible = Not GroupBox3.Visible
        GroupBox4.Visible = Not GroupBox4.Visible
    End Sub
    Private Sub SchTimer_Tick(ByVal Sender As Object, ByVal e As EventArgs) Handles SchTimer.Tick
        Ontime = dtPicker.Value.ToString("hh:mm ttt")
        If (Ontime = Now.ToString("hh:mm tt")) AndAlso Not (_DoEvent) Then
            _TimeSetClear = Now
            btnSave_Click(Sender, e)
            If SendMail(tboxSender.Text, tboxSender_pass.Text, tboxReceiver.Text, "Looping service report", IIf(String.IsNullOrEmpty(_PathFileLog), True, False)) Then
                _DoEvent = True
                lbelTimeNow.BackColor = Color.Green
                btnStop_Click(Sender, e)
                btnStart_Click(Sender, e)
            End If
            Threading.Thread.Sleep(2000)
        End If
        If (_TimeSetClear.AddMinutes(1).ToString("hh:mm") = Now.ToString("hh:mm")) AndAlso _DoEvent Then
            _DoEvent = False
        End If
        lbelTimeNow.Text = Now.ToString("hh:mm:ss ttt")
    End Sub
    Private Sub cboxIplist_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboxIplist.SelectedIndexChanged
        FileName = sender.selecteditem
    End Sub
    Private Sub tboxNexKind_TextChanged(sender As Object, e As EventArgs) Handles tboxNexKind.TextChanged
        If Not (Double.TryParse(sender.text, "0.0")) Then Exit Sub
        DblTimeKind = CDbl(sender.text)
    End Sub
    Private Sub tboxNextLoop_TextChanged(sender As Object, e As EventArgs) Handles tboxNextLoop.TextChanged
        If Not (Double.TryParse(sender.text, "0.0")) Then Exit Sub
        DblTimeLoop = IIf(String.IsNullOrEmpty(sender.text), 0, CDbl(sender.text))
    End Sub
    Private Sub tboxSender_TextChanged(sender As Object, e As EventArgs) Handles tboxSender.TextChanged
        If EmailValid(sender.text) Then
            tboxSender.ForeColor = Color.Green
            _SenderValid = True
        Else
            tboxSender.ForeColor = Color.Red
            _SenderValid = False
        End If
    End Sub
    Private Sub tboxReceiver_TextChanged(sender As Object, e As EventArgs) Handles tboxReceiver.TextChanged
        If EmailValid(sender.text) Then
            tboxReceiver.ForeColor = Color.Green
            _SenderValid = True
        Else
            tboxReceiver.ForeColor = Color.Red
            _SenderValid = False
        End If
    End Sub
    Private Sub cboxEmailNoti_CheckedChanged(sender As Object, e As EventArgs) Handles cboxEmailNoti.CheckedChanged
        SchTimer.Interval = 5000
        ' Enable timer.
        SchTimer.Enabled = sender.checked
    End Sub
End Class

