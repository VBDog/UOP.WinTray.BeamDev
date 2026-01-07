
Imports UOP.DXFGraphics
Imports UOP.DXFGraphics.Structures
Public Class frmColorPicker
    Private SelectedColor As Integer
    Private OKSelected As Boolean
    Dim tBoxes As System.Collections.Generic.Dictionary(Of String, TextBox)
    Private eInitColor As String
    Private Sub SetTabOrder(f As Form)
        'Sets the tab order according to
        'vertical position. Higher controls
        'go first
        Dim controls = GetAllControls(f).OrderBy(Function(c) GetLocationRelativeToForm(c).Y)
        For i = 0 To controls.Count - 1
            controls.ElementAt(i).TabIndex = i
        Next
    End Sub
    Public Function GetAllControls(control As Control) As List(Of Control)
        Dim ret As New List(Of Control)
        For Each C As Control In control.Controls
            ret.Add(C)
            ret.AddRange(GetAllControls(C))
        Next
        Return ret
    End Function
    Public Function GetLocationRelativeToForm(control As Control) As Point
        Dim curControl As Control = control
        Dim curPt As New Point(0, 0)
        Do Until curControl Is Nothing
            curPt += curControl.Location
            If Not (TypeOf curControl.Parent Is Form) Then
                curControl = curControl.Parent
            Else
                Exit Do
            End If
        Loop
        Return curPt
    End Function
    Private Sub ShowColors()
        '^executed  load to set the text box's background colors
        Dim i As Integer
        Dim base As Integer
        Dim tBox As TextBox
        Dim tidx As Integer = 1
        Dim arT() As Control
        Dim idxBoxes As New Dictionary(Of String, TextBox)
        tBoxes = New System.Collections.Generic.Dictionary(Of String, TextBox)
        SetTag(StdColors0, dxfColors.Color(dxxColors.Red), True, tidx)
        SetTag(StdColors1, dxfColors.Color(dxxColors.Yellow), True, tidx)
        SetTag(StdColors2, dxfColors.Color(dxxColors.Green), True, tidx)
        SetTag(StdColors3, dxfColors.Color(dxxColors.Cyan), True, tidx)
        SetTag(StdColors4, dxfColors.Color(dxxColors.Blue), True, tidx)
        SetTag(StdColors5, dxfColors.Color(dxxColors.Magenta), True, tidx)
        SetTag(StdColors6, dxfColors.Color(dxxColors.BlackWhite), True, tidx)
        SetTag(StdColors7, dxfColors.Color(dxxColors.Grey), True, tidx)
        SetTag(StdColors8, dxfColors.Color(dxxColors.LightGrey), True, tidx)
        SetTag(Grays0, dxfColors.Color(dxxColors.Black), True, tidx)
        SetTag(Grays1, dxfColors.Color(251), True, tidx)
        SetTag(Grays2, dxfColors.Color(252), True, tidx)
        SetTag(Grays3, dxfColors.Color(253), True, tidx)
        SetTag(Grays4, dxfColors.Color(254), True, tidx)
        SetTag(Grays5, dxfColors.Color(dxxColors.White), True, tidx)
        SetTag(LogColor0, dxfColors.Color(dxxColors.ByLayer), True, tidx)
        SetTag(LogColor1, dxfColors.Color(dxxColors.ByBlock), True, tidx)
        For i = 0 To 239
            arT = Controls.Find("TextBox" & i.ToString, True)
            If arT.Length > 0 Then
                tBox = arT(0)
                idxBoxes.Add(i.ToString, tBox)
                tBox.TabIndex = 0
            End If
        Next i
        base = 18
        For i = 216 To 216 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 16
        For i = 192 To 192 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 14
        For i = 168 To 168 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 12
        For i = 144 To 144 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 10
        For i = 120 To 120 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 11
        For i = 96 To 96 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 13
        For i = 72 To 72 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 15
        For i = 48 To 48 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 17
        For i = 24 To 24 + 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
        base = 19
        For i = 0 To 23
            tBox = idxBoxes.Item(i.ToString)
            SetTag(tBox, dxfColors.Color(base), True, tidx)
            base += 10
        Next i
    End Sub
    Private Sub SetTag(aTextBox As TextBox, aColor As dxfColor, Optional bSaveBox As Boolean = False, Optional aTabIndex As Integer = -1)
        Dim txt As String = aColor.ToString
        aTextBox.Tag = txt
        ToolTip1.SetToolTip(aTextBox, txt)
        If aColor.IsLogical Or aColor.ACLNumber = 7 Then
            aTextBox.BackColor = Color.White
        Else
            aTextBox.BackColor = aColor.ToWin64
        End If
        'If aTabIndex >= 0 Then
        '    aTextBox.TabIndex = aTabIndex
        '    aTabIndex += 1
        'End If
        AssignValidation(aTextBox)
        If bSaveBox Then
            Dim tbox As TextBox = Nothing
            Dim hndl As String = aColor.ACLValue.ToString
            If Not tBoxes.TryGetValue(hndl, tbox) Then
                tBoxes.Add(hndl, aTextBox)
            End If
        End If
    End Sub
    Private Sub SetSelectedColor(aColor As String)
        'If Not TVALUES.IsNumber(aColor) Then Return
        Dim iVal As Long = TVALUES.To_LNG(aColor)
        Dim tbox As TextBox = Nothing
        If tBoxes.TryGetValue(iVal.ToString, tbox) Then
            If Visible Then tbox.Select()
            HandleClicks(tbox, New EventArgs)
        Else
            ShowColor("Windows", dxfColors.Win32ToWin64(TValues.To_INT(iVal)), False)
        End If
        If Visible Then txtSelColor.Select()
    End Sub
    Public Function SelectColor(aOwner As IWin32Window, ByRef rCanceled As Boolean, Optional InitColor As dxxColors = dxxColors.Undefined, Optional bNoLogical As Boolean = False, Optional bNoWindows As Boolean = False, Optional aWin64Color As Color? = Nothing) As dxfColor
        Dim _rVal As dxfColor = Nothing
        Try
            rCanceled = True
            fraLogicals.Enabled = Not bNoLogical
            cmdWindows.Enabled = Not bNoWindows
            ShowColors()
            Dim dxClr As dxxColors = InitColor
            dxClr = InitColor
            If bNoLogical And dxClr = 0 Or dxClr = 256 Then dxClr = dxxColors.BlackWhite
            If bNoWindows And dxClr <> dxxColors.Undefined And (dxClr < -1 Or dxClr > 256) Then
                If Not dxfColors.Color(dxClr).IsACL Then dxClr = dxxColors.BlackWhite
            End If
            eInitColor = dxfColors.Color(dxClr).ACLNumber.ToString
            If aWin64Color.HasValue Then
                Dim idx As dxxColors
                Dim aClr As dxfColor = dxfColors.NearestACLColor(aWin64Color.Value, idx, True)
                If aClr.ACLNumber <> -1 And idx > 0 And idx < 256 Then
                    eInitColor = aClr.ACLValue.ToString
                Else
                    eInitColor = dxfColors.Win64ToWin32(aWin64Color).ToString
                End If
            End If
            OKSelected = False
            ShowDialog(aOwner)
            If OKSelected Then
                rCanceled = False
                If IsNumeric(lblIndex.Text) Then
                    _rVal = dxfColors.Color(TVALUES.To_INT(lblIndex.Text))
                Else
                    _rVal = New dxfColor(-1, dxfColors.Win64ToARGB(txtSelColor.BackColor))  ' dxfColors.color(Me.txtSelColor.BackColor).Clone
                End If
            Else
                _rVal = dxfColors.Color(TVALUES.To_INT(dxClr))
            End If
            Return _rVal
        Catch ex As Exception
            Throw ex
            Return _rVal
        End Try
    End Function
    Private Sub frmColorPicker_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblRGB.Text = ""
        lblIndex.Text = ""
        lblWin32.Text = ""
        ShowColors()
        SetTabOrder(Me)
    End Sub
    Private Sub cmdWindows_Click(sender As Object, e As EventArgs) Handles cmdWindows.Click
        ColorDialog1.Color = txtSelColor.BackColor
        If ColorDialog1.ShowDialog(Me) <> Windows.Forms.DialogResult.Cancel Then
            ShowColor("Windows", ColorDialog1.Color)
        End If
    End Sub
    Private Sub cmdOK_Click(sender As Object, e As EventArgs) Handles cmdOK.Click
        OKSelected = True
        Hide()
    End Sub
    Private Sub cmdCancel_Click(sender As Object, e As EventArgs) Handles cmdCancel.Click
        OKSelected = False
        Hide()
    End Sub
    Public Sub AssignValidation(ByRef CTRL As Windows.Forms.TextBox)
        Dim txt As Windows.Forms.TextBox = CTRL
        AddHandler txt.DoubleClick, AddressOf HandleDoubleClicks
        AddHandler txt.Click, AddressOf HandleClicks
        AddHandler txt.KeyPress, AddressOf HandleKeyPress
    End Sub
    Private Sub HandleClicks(sender As Object, e As EventArgs)
        Dim tbox As TextBox = sender
        Dim clr As Color = tbox.BackColor
        ShowColor(tbox.Tag.ToString, clr)
    End Sub
    Private Sub HandleDoubleClicks(sender As Object, e As EventArgs)
        Dim tbox As TextBox = sender
        Dim clr As Color = tbox.BackColor
        ShowColor(tbox.Tag.ToString, clr)
        txtSelColor.Select()
        OKSelected = True
        Hide()
    End Sub
    Private Sub ShowColor(aIndex As String, aColor As Color, Optional bFindACL As Boolean = False)
        Dim bCLr As Color = Color.FromArgb(TValues.To_INT(aColor.A), TValues.To_INT(aColor.R), TValues.To_INT(aColor.G), TValues.To_INT(aColor.B))
        If aColor <> Color.Transparent Then txtSelColor.BackColor = bCLr
        lblWin32.Text = New COLOR_ARGB(aColor.R, aColor.G, aColor.B).ToWin32.ToString
        lblRGB.Text = $"{aColor.R },{ aColor.G },{aColor.G}"
        lblIndex.Text = aIndex
        If aIndex.IndexOf(dxfLinetypes.ByLayer, StringComparison.OrdinalIgnoreCase) >= 0 Then
            txtSelColor.Text = dxfLinetypes.ByLayer
        ElseIf aIndex.IndexOf(dxfLinetypes.ByBlock, StringComparison.OrdinalIgnoreCase) >= 0 Then
            txtSelColor.Text = dxfLinetypes.ByBlock
        Else
            txtSelColor.Text = ""
        End If
        If bFindACL And aIndex = "Windows" Then
            Dim idx As dxxColors = dxxColors.Undefined
            Dim tCol As dxfColor = dxfColors.NearestACLColor(aColor, idx, True)
            If idx <> dxxColors.Undefined Then
                lblWin32.Text = tCol.RGB.ToWin32.ToString
                lblRGB.Text = aColor.R & "," & aColor.G & "," & aColor.G
                lblIndex.Text = idx
            End If
        End If
    End Sub
    Private Sub HandleKeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs)
        e.KeyChar = Chr(0)
        e.Handled = True
    End Sub
    Private Sub txtSelColor_TextChanged(sender As Object, e As EventArgs) Handles txtSelColor.TextChanged
    End Sub
    Private Sub txtSelColor_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtSelColor.KeyPress
        e.KeyChar = Chr(0)
        e.Handled = True
    End Sub
    Private Sub cmdWindows_DockChanged(sender As Object, e As EventArgs) Handles cmdWindows.DockChanged
    End Sub
    Private Sub frmColorPicker_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Static bDone As Boolean
        If bDone Then Return
        bDone = True
        SetSelectedColor(eInitColor)
    End Sub
End Class

