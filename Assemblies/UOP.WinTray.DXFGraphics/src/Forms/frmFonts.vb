Imports UOP.DXFGraphics
Imports UOP.DXFGraphics.Fonts.dxfFonts
Public Class frmFonts
        Private oFontNames As List(Of String)
        Private oFontIndices As List(Of Integer)
        Private bPreviewing As Boolean
        Private bOpCanceled As Boolean = True
        Private bOpComplete As Boolean = False
        Private bShowShapes As Boolean = True
        Private sSelectedFont As String = String.Empty
        Private sSelectedStyle As String = String.Empty
        Private bShowTrueTypes As Boolean = True
        Private bAllowStyles As Boolean = True
        Private bLoading As Boolean = False
        Private sLastStyle As String = String.Empty
        Private sLastFont As String = String.Empty
        Private _AllFonts As List(Of dxoFont)
        Private _Image As dxfImage
        Public Sub New()
            bLoading = True
            ' This call is required by the designer.
            InitializeComponent()
            bLoading = False
            ' Add any initialization after the InitializeComponent() call.
            NamesClear()
            cboStyles.Items.Clear()
            cboStyles.Text = ""
            cboFonts.Items.Clear()
            cboFonts.Text = ""
            sLastStyle = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
            bShowShapes = True
            bShowTrueTypes = True
            bAllowStyles = True
            sLastFont = ""
            bOpComplete = False
            bOpCanceled = True
            'picPreview.AutoRedraw = True
        End Sub
        Private Function GetDataControl(aTag As String) As Label
            If String.Compare(lblFData_0.Tag, aTag, True) = 0 Then Return lblFData_0
            If String.Compare(lblFData_1.Tag, aTag, True) = 0 Then Return lblFData_1
            If String.Compare(lblFData_2.Tag, aTag, True) = 0 Then Return lblFData_2
            Return Nothing
        End Function
        Public Function SelectFont(Optional aInitFont As String = "", Optional aInitStyle As String = "", Optional bNoShapes As Boolean = False, Optional bNoTrueType As Boolean = False, Optional bNoStyles As Boolean = False, Optional rCanceled As Boolean? = Nothing, Optional aOwner As IWin32Window = Nothing, Optional bReloadFonts As Boolean = False) As String
        If rCanceled IsNot Nothing Then rCanceled = False
        If String.IsNullOrWhiteSpace(aInitFont) Then aInitFont = ""
        If String.IsNullOrWhiteSpace(aInitStyle) Then aInitStyle = ""
        Dim sInitFont As String = aInitFont.Trim
        Dim sInitStyle As String = aInitStyle.Trim
        If sInitFont = "" Then sInitFont = SelectedFont
        If sInitStyle = "" Then sInitStyle = SelectedStyle
        ShowTrueTypes = Not bNoTrueType
        ShowShapes = Not bNoShapes
        bOpComplete = False
        bOpCanceled = False
        bAllowStyles = Not bNoStyles
        bLoading = True
        chkSHX.Checked = ShowShapes
        chkTTF.Checked = ShowTrueTypes
        bLoading = False
        If sInitFont = "" Then
            sInitFont = "Arial.ttf"
            sInitStyle = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
        End If
        bLoading = True
        SelectedFont = sInitFont
        SelectedStyle = sInitStyle
        bLoading = False
        LoadFontCombo(bReloadFonts)
        sInitFont = cboFonts.Text
        sInitStyle = cboStyles.Text
        ShowDialog(aOwner)
        Dim _rVal As String
        Dim canceld = bOpCanceled
        SelectedFont = cboFonts.Text
        SelectedStyle = cboStyles.Text
        If rCanceled Then
            SelectedStyle = sInitStyle
            SelectedFont = sInitFont
        End If
        Dim fnt As String = SelectedFont
        Dim sty As String = SelectedStyle
        If String.IsNullOrEmpty(fnt) Then canceld = True
        If Not rCanceled Then
            If sty = "" Then sty = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
            If Not fnt.ToUpper.EndsWith(".SHX") Then
                If Not fnt.ToUpper.EndsWith(".TTF") Then
                    fnt += ".ttf"
                End If
            End If
        Else
            fnt = sInitStyle
            fnt = sInitFont
        End If
        If rCanceled IsNot Nothing Then rCanceled = canceld
        If _Image IsNot Nothing Then _Image.Dispose()
        _Image = Nothing
        _rVal = fnt & ";" & sty
        Return _rVal
    End Function
    Public Property SelectedFont As String
        Get
            If sSelectedFont Is Nothing Then sSelectedFont = ""
            Return sSelectedFont
        End Get
        Set(value As String)
            Dim newval As Boolean = sSelectedFont <> value
            sSelectedFont = value
            If Not bLoading And newval Then
                LoadStyleCombo()
            End If
        End Set
    End Property
    Public Property SelectedStyle As String
        Get
            If sSelectedStyle Is Nothing Then sSelectedStyle = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
            Return sSelectedStyle
        End Get
        Set(value As String)
            Dim newval As Boolean = sSelectedStyle <> value
            sSelectedStyle = value
            If Not bLoading And newval Then
                ShowSelectedFont()
            End If
        End Set
    End Property
    Private Sub LoadStyleCombo()
        Dim wuz As Boolean = bLoading
        Dim fntname As String
        Dim i As Integer
        Dim idx As Integer
        Dim sNames As List(Of String)
        Dim bttf As Boolean
        Dim stylewas = SelectedStyle
        Try
            bLoading = True
            fntname = SelectedFont
            cboStyles.Items.Clear()
            bttf = Not fntname.Contains(".sh")
            If fntname = "" Then
                bLoading = wuz
                Return
            End If
            If Not bttf Then
                If Not fntname.ToLower.EndsWith(".shx") Then fntname += ".shx"
                stylewas = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
                cboStyles.Items.Add(stylewas)
                cboStyles.SelectedItem = 0
                cboStyles.Text = stylewas
                bLoading = wuz
                Return
            Else
                If fntname.ToLower.EndsWith(".ttf") Then fntname = fntname.Substring(0, fntname.Length - 4)
            End If
            Dim font As dxoFont = dxoFonts.Find(_AllFonts, fntname)
            If font.Found Then
                sNames = font.StyleNames
            Else
                sNames = New List(Of String)
            End If
            stylewas = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
            idx = -1
            For i = 1 To sNames.Count
                If sNames.Item(i - 1) IsNot Nothing Then
                    cboStyles.Items.Add(sNames.Item(i - 1))
                    If stylewas <> "" And String.Compare(sNames.Item(i - 1), stylewas, True) = 0 Then
                        idx = i - 1
                    End If
                End If
            Next i
            'If cboStyles.Items.Count <= 0 Then
            '    cboStyles.Items.Add(dxfEnums.Description(dxxTextStyleFontSettings.Regular))
            '    idx = 0
            'End If
            If idx < 0 Then idx = 0
            If idx > cboStyles.Items.Count - 1 Then idx = 0
            If cboStyles.Items.Count > 0 Then
                cboStyles.SelectedItem = cboStyles.Items.Item(idx)
            End If
            cboStyles.Enabled = bAllowStyles
        Catch ex As Exception
        Finally
            bLoading = wuz
            If Not bLoading Then
                ShowSelectedFont()
            End If
        End Try
    End Sub
    Private Sub LoadFontCombo(bReloadFontList As Boolean)
        If _AllFonts Is Nothing Then bReloadFontList = True
        If bReloadFontList Then
            _AllFonts = dxoFonts.GetAvailableFonts(bReloadFontList)
        End If
        bLoading = True
        Dim fntwuz As String = SelectedFont
        Dim stywuz As String = SelectedStyle
        Dim idx As Integer = -1
        cboFonts.Text = ""
        cboFonts.Items.Clear()
        Dim i As Integer = -1
        For Each f In _AllFonts
            If (ShowTrueTypes And Not f.IsShape) Or (ShowShapes And f.IsShape) Then
                cboFonts.Items.Add(f.Name)
                i += 1
                If fntwuz <> "" And String.Compare(f.Name, fntwuz, True) = 0 Then
                    idx = i
                End If
            End If
        Next
        If cboFonts.Items.Count <= 0 Then
            SelectedFont = ""
            SelectedStyle = ""
        Else
            If idx < 0 Then idx = 0 Else SelectedFont = fntwuz
            cboFonts.SelectedItem = cboFonts.Items(idx)
        End If
        LoadStyleCombo()
        bLoading = False
        ShowSelectedFont()
    End Sub
    Private Sub ShowSelectedFont()
        Try
            Dim fname As String = SelectedFont
            Dim sname As String = SelectedStyle
            Dim wuz As Boolean
            If fname = "" Then
                If ShowShapes Then
                    fname = "txt.shx"
                Else
                    fname = "Arial.ttf"
                End If
                sname = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
            Else
                If Not fname.ToLower.EndsWith(".shx") Then fname += ".ttf"
            End If
            wuz = bLoading
            bLoading = True
            NamesClear()
            Dim font As dxoFont = dxoFonts.Find(_AllFonts, fname)
            ShowFontData(font)
            ShowPreview()
            bLoading = wuz
        Catch ex As Exception
        End Try
        If Visible And Enabled Then
            'If picPreview.Visible Then picPreview.SetFocus
        End If
    End Sub
    Private Sub ShowPreview()
        '    'On Error Resume Next
        If bPreviewing Or bOpCanceled Or bOpComplete Then Exit Sub
        bPreviewing = True
        Dim aStr As String
        Dim fnt As String = SelectedFont
        Dim styl As String = SelectedStyle
        If String.IsNullOrEmpty(fnt) Then
            bPreviewing = False
            Return
        End If
        Dim g As Graphics = picPreview.CreateGraphics
        g.Clear(Color.White)
        If _Image Is Nothing Then
            _Image = New dxfImage
            ' _Image.Display.Size = New System.Drawing.Size(picPreview.Width, picPreview.Height)
            _Image.Display.SetDevice(picPreview, dxxColors.White)
        End If
        _Image.AutoRedraw = False
        _Image.Display.BackgroundColor = Color.White
        _Image.Entities.Clear(True)
        _Image.Header.UCSMode = dxxUCSIconModes.None
        If Not fnt.Contains(".") Then fnt += ".ttf"
        Try
            Dim font As dxoFont = dxfGlobals.GetFont(fnt, fnt) ' dxoFonts.Find(_AllFonts, fnt)
            If font Is Nothing OrElse font.Styles.Count <= 0 Then
                Return
            End If
            Dim style As dxoFontStyle = font.Style(styl)
            If style Is Nothing Then
                style = font.Style(cboStyles.SelectedIndex + 1)
            End If
            If style Is Nothing Then Return
            Dim tstyle As dxoStyle = _Image.TextStyle
            tstyle.UpdateFontName(fnt, style.StyleName)
            aStr = "abcdefghijklmnopqrstuvwxyz 1234567890"
            aStr += "\P" & UCase("abcdefghijklmnopqrstuvwxyz")
            'Dim gfont As Font = New Font(font.Name, )
            _Image.Draw.aText(dxfVector.Zero, aStr, 5, aAlignment:=dxxMTextAlignments.BaselineLeft)
            _Image.Display.ZoomExtents()
            '_Image.Bitmap().DrawToDevice(g.GetHdc)
            'g.DrawImage(_Image.Bitmap().Bitmap, New Point(0, 0))
        Catch ex As Exception
        Finally
            bPreviewing = False
            g.Dispose()
            '_Image.Dispose()
            '_Image = Nothing
        End Try
        '    Dim aStr As String
        '    Dim pntrWuz As Long
        '    Dim wuz As Boolean
        '    Dim aBMP As TBITMAP
        '    Dim aFC As TFONTCONTROL
        '    wuz = Enabled
        '    pntrWuz = Screen.MousePointer
        '    Enabled = False
        '    Screen.MousePointer = vbHourglass
        '    bmp_Create aBMP, picPreview.hdc
        'If Not cboFonts.SelectedItem Is Nothing Then
        '        aStr = "abcdefghijklmnopqrstuvwxyz 1234567890"
        '        aStr = aStr & "\P" & UCase("abcdefghijklmnopqrstuvwxyz")
        '            aFC.FontStyle = cboStyles.Text
        '            aFC.Alignment = dxfTopLeft
        '            aFC.FontName = cboFonts.SelectedItem.Text
        '            aFC.Height = 0.23 * ScaleY(picPreview.Height, ScaleMode, vbPixels)
        '            aFC.FontIndex = oFontIndices.Item(cboFonts.SelectedItem.Index)
        '        bmp_DrawString aBMP, aStr, aFC, , , dxfAlign_TopLeft, 10, 10
        'End If
        '    bmp_DrawTo aBMP, picPreview.hdc, dxfAlign_MiddleCenter
        'picPreview.Refresh
        '    bmp_Destroy aBMP
        'bPreviewing = False
        '    Enabled = wuz
        '    Screen.MousePointer = pntrWuz
    End Sub
    Private Sub ShowFontData(aFont As dxoFont)
        NamesClear()
        If aFont Is Nothing Then Return
        Dim font As TFONT = New TFONT(aFont)
        Dim styl As TFONTSTYLE
        If font.IsShape Then
                styl = font.GetStyle(dxfEnums.Description(dxxTextStyleFontSettings.Regular))
            Else
                styl = font.GetStyle(SelectedStyle)
            End If
            Dim fname As String = styl.FileName
            Dim i As Integer = InStrRev(fname, "/")
            If i > 0 Then
                fname = Microsoft.VisualBasic.Right(fname, Len(fname) - i)
            End If
            GetDataControl("TypeFace").Text = font.FamilyName
            GetDataControl("FaceName").Text = font.FaceName
            GetDataControl("FileName").Text = fname
        End Sub
        Public Property ShowTrueTypes As Boolean
            Get
                Return bShowTrueTypes
            End Get
            Set(value As Boolean)
                bShowTrueTypes = value
            End Set
        End Property
        Public Property ShowShapes As Boolean
            Get
                Return bShowShapes
            End Get
            Set(value As Boolean)
                bShowShapes = value
            End Set
        End Property
        Private Sub cmdClose_Click(sender As Object, e As EventArgs) Handles cmdClose.Click
            bOpCanceled = True
            Hide()
        End Sub
        Private Sub cmdSave_Click(sender As Object, e As EventArgs) Handles cmdSave.Click
            bOpCanceled = False
            bOpComplete = True
            Hide()
        End Sub
        Private Sub NamesClear()
            lblFData_0.Text = ""
            lblFData_1.Text = ""
            lblFData_2.Text = ""
        End Sub
        Private Sub cboFonts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFonts.SelectedIndexChanged
            SelectedFont = cboFonts.Text
            SelectedStyle = cboStyles.Text
        End Sub
        Private Sub cboStyles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboStyles.SelectedIndexChanged
            SelectedFont = cboFonts.Text
            SelectedStyle = cboStyles.Text
        End Sub
        Private Sub cboFonts_KeyPress(sender As Object, e As KeyPressEventArgs) Handles cboFonts.KeyPress, cboStyles.KeyPress
            e.KeyChar = ""
            e.Handled = True
        End Sub
        Private Sub cboFonts_KeyDown(sender As Object, e As KeyEventArgs) Handles cboFonts.KeyDown, cboStyles.KeyDown
            e.Handled = True
        End Sub
        Private Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
            LoadFontCombo(True)
        End Sub
        Private Sub chkTTF_CheckedChanged(sender As Object, e As EventArgs) Handles chkTTF.CheckedChanged
            ShowTrueTypes = chkTTF.Checked
            If Not bLoading Then
                LoadFontCombo(False)
            End If
        End Sub
        Private Sub chkSHX_CheckedChanged(sender As Object, e As EventArgs) Handles chkSHX.CheckedChanged
            ShowShapes = chkSHX.Checked
            If Not bLoading Then
                LoadFontCombo(False)
            End If
        End Sub
        Private Sub cboFonts_SelectedValueChanged(sender As Object, e As EventArgs) Handles cboFonts.SelectedValueChanged
            SelectedFont = cboFonts.Text
            SelectedStyle = cboStyles.Text
        End Sub
        Private Sub cboStyles_SelectedValueChanged(sender As Object, e As EventArgs) Handles cboStyles.SelectedValueChanged
            SelectedFont = cboFonts.Text
            SelectedStyle = cboStyles.Text
        End Sub
    End Class

