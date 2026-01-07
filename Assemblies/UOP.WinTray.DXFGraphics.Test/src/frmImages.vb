
Imports UOP.DXFGraphics
Imports System.Drawing
Imports System.Drawing.Text
Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports UOP.DXFGraphicsControl
Imports UOP.DXFGraphics.Utilities

Public Class frmImages
    Private WithEvents _Image As dxfImage
    Private _lasso As Boolean
    Private _Loading As Boolean
    Private _Drag As Boolean
    Private _Pan1 As Point
    Private _Pan2 As Point
    Private _RunningCode As Boolean
    Private _CheckBoxes As Dictionary(Of String, CheckBox)
    Dim _ZoomPt As Point
    Dim _MousePt As Point
    Private _WorkingFolder As String = ""  '"C:\Users\E342367\Documents\Junk\"
    Private _WorkingVectors As colDXFVectors
    Private _Filename As String = ""

    Public Sub New()

        ' This call is required by the designer.
        _Loading = True
        InitializeComponent()
        _Loading = False
        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Function GetAppPath() As String
        Dim i As Integer
        Dim rPath As String = System.Reflection.Assembly.GetExecutingAssembly.Location()

        i = InStrRev(rPath, "\", , vbTextCompare)

        If i > 0 Then rPath = Microsoft.VisualBasic.Left(rPath, i - 1)

        Return rPath

    End Function


    Private Property WorkingFolder() As String
        Get
            If _WorkingFolder = "" Then
                _WorkingFolder = My.Settings.WorkingFolder
                If _WorkingFolder <> "" Then
                    If Not System.IO.Directory.Exists(_WorkingFolder) Then _WorkingFolder = ""
                End If
            End If

            If _WorkingFolder = "" Then
                If System.IO.Directory.Exists("C:\Users\E342367\Documents\Junk") Then
                    _WorkingFolder = "C:\Users\E342367\Documents\Junk"
                Else
                    _WorkingFolder = GetAppPath()




                End If
                My.Settings.WorkingFolder = _WorkingFolder
            End If

            Return _WorkingFolder
        End Get
        Set(value As String)
            _WorkingFolder = value
            My.Settings.WorkingFolder = _WorkingFolder


        End Set
    End Property


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click


        RunCode(1)



    End Sub

    Private Property StatusText(aIndex As Integer) As String
        Get
            Select Case aIndex
                Case 1
                    Return Status1.Text
                Case 2
                    Return Status2.Text
                Case 3
                    Return Status3.Text
                Case Else
                    Return ""
            End Select



        End Get
        Set(value As String)
            Select Case aIndex
                Case 1
                    Status1.Text = value
                Case 2
                    Status2.Text = value
                Case 3
                    Status3.Text = value
                Case Else

            End Select


        End Set
    End Property

    Private _SuppressZoomExtents As Boolean = False

    Private Sub RunCode(aIndex As Integer)

        Panel1.Enabled = False
        Me.Cursor = System.Windows.Forms.Cursors.WaitCursor
        Dim start_time As DateTime
        Dim stop_time As DateTime
        Dim elapsed_time As TimeSpan


        Try
            txtErrors.Text = "" : txtSettings.Text = "" : txtRender.Text = "" : txtTables.Text = "" : txtImage.Text = "" : txtEntities.Text = "" : txtBlocks.Text = "" : txtScreen.Text = ""

            UserControl12.viewer.SetImage(Nothing)
            UserControl12.viewer.Clear()
            _SuppressZoomExtents = False
            '_Image.Display.SetDevice(Nothing)
            If aIndex > 0 Then
                If Not IsNothing(_Image) Then _Image.Dispose()
                _Image = New dxfImage() ' _Image.Clear()
            Else
                _Image = New dxfImage()
                udUCSSize.Value = _Image.Header.UCSSize
                _MousePt = New System.Drawing.Point(0, 0)
                If _Image.Screen.PointSize >= 1 Then udPsize.Value = _Image.Screen.PointSize

            End If

            _Image.Display.SetDevice(Me.pnlOutput)

            '_Image.Dispose()
            '_Image = New dxfImage
            _Image.CollectErrors = True
            _Image.ResetErrors()
            SetScreenProperties()

            _Image.FolderPath = WorkingFolder
            lblZoomFactor.Text = Format(_Image.Display.ZoomFactor, "0.00")

            _Filename = "Code_" & aIndex
            _Image.Name = _Filename
            If aIndex <> 0 And CheckVal("AddCodeLabels") Then
                _Image.Screen.Entities.aScreenText(_Filename, dxxRectangularAlignments.TopLeft, aScreenFraction:=0.025, bPersist:=True)
            End If
            _RunningCode = True

            start_time = Now
            lblElapsed.Text = "0.000000"
            'Application.DoEvents()

            Dim hdr As dxsHeader = _Image.Header
            Dim dimorides As dxsDimOverrides = _Image.DimStyleOverrides
            dimorides.LinearPrecision = 3

            Select Case aIndex
                Case 0
                    _Image.Display.Redraw()
                Case 1

                    Code_1()
                Case 2
                    Code_2()
                Case 3
                    Code_3()
                Case 4
                    Code_4()
                Case 5
                    Code_5()
                Case 6
                    Code_6()
                Case 7
                    Code_7()
                Case 8
                    Code_8()
                Case 9
                    Code_9()
                Case 10
                    Code_10()
                Case 11
                    Code_11()
                Case 12
                    Code_12()
                Case 13
                    Code_13()
                Case 14
                    Code_14()
                Case 15
                    Code_15()
                Case 16
                    Code_16()
                Case 17
                    Code_17()

                Case 18
                    Code_18()
                Case 19
                    Code_19()
                Case 20
                    Code_20()
                Case 21
                    Code_21()
                Case 100
                    Code_100()
                Case 101
                    Code_101()
            End Select

        Catch ex As Exception
            MsgBox("Error-" & ex.Message)


        Finally

            Try
                Dim sTime As String = "0.00"
                If aIndex <> 0 Then
                    stop_time = Now
                    elapsed_time = stop_time.Subtract(start_time)
                    sTime = elapsed_time.TotalSeconds.ToString("0.000000")

                End If
                Me.Cursor = System.Windows.Forms.Cursors.Default
                If Not _SuppressZoomExtents Then
                    _Image.Display.ZoomExtents(1.15)
                End If

                UpdateScreenBitmap()


                lblElapsed.Text = sTime

                _RunningCode = False
                Panel1.Enabled = True
                Me.Cursor = System.Windows.Forms.Cursors.Default

            Catch ex As Exception

            Finally
                Panel1.Enabled = True
                Me.Visible = True
                Me.WindowState = FormWindowState.Normal
                Try
                    UserControl12.viewer.SetImage(_Image)
                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                End Try



            End Try
        End Try
    End Sub

    Private Sub UpdateScreenBitmap(Optional aDXFBitmap As dxfBitmap = Nothing)
        If IsNothing(_Image) Then Return
        Try
            If picScreen.Size = Panel1.Size Then
                Dim aImage As Bitmap
                If Not IsNothing(aDXFBitmap) Then aImage = aDXFBitmap.Image Else aImage = _Image.GetImage(True)
                picScreen.Image = aImage
            Else
                If IsNothing(aDXFBitmap) Then aDXFBitmap = _Image.Bitmap(True)
                aDXFBitmap.StretchToControl(picScreen.Handle)
            End If
            picScreen.Refresh()
        Catch ex As Exception

        End Try


    End Sub

    Private Sub SetScreenProperties()
        If _Image Is Nothing Then Return
        _Image.Screen.BoundingRectangles = CheckVal("EntityBounds")
        _Image.Screen.ExtentPts = CheckVal("ExtentPts")
        _Image.Screen.TextBoxes = CheckVal("TextBoxes")
        _Image.Screen.OCSs = CheckVal("EntityOCS")
        _Image.Display.BackgroundColor = PictureBox2.BackColor
        _Image.Header.UCSMode = cboUCSMode.SelectedIndex
        _Image.Screen.Suppressed = CheckVal("SuppressScreen")
        _Image.Screen.PointSize = udPsize.Value
        _Image.Screen.ExtentRectangle = CheckVal("ExtentRectangle")
        _Image.Header.UCSSize = udUCSSize.Value
        _Image.Header.LineWeightDisplay = CheckVal("DisplayLineweights")
    End Sub

    Private Sub Code_1()
        Dim v1 As dxfVector = dxfVector.Zero
        Dim v2 As New dxfVector(100, 100)
        Dim i As Integer
        Dim k As Integer
        Dim zRec As New dxfRectangle(New dxfVector(0, 0), 100, 100)
        Dim c1 As dxxColors
        Dim EntCount As Integer = 100 * 3
        Dim draw As dxoDrawingTool = _Image.Draw
        Dim dims As dxoDimTool = _Image.DimTool
        Try

            'dxfUtils.TestIT()


            '_Image.Display.ZoomOnRectangle(zRec)
            'c1 = dxfColors.RandomACIColor
            ''Debug.Print(_Image.Layers.NameList)
            'lyr = _Image.Layers.Add("Circles", c1, aLineWeight:=dxxLineWeights.ByDefault)

            ''MsgBox(_Image.Layer("Circles").Color)
            ''lyr.Color = dxxColors.Blue
            ''_Image.Linetypes.AddLinetype("Hidden") 
            'c1 = dxfColors.RandomACIColor(False, c1)

            'lyr = _Image.Layers.Add("Ellipses", c1, aLineWeight:=dxxLineWeights.ByDefault)
            'c1 = dxfColors.RandomACIColor(False, c1)
            'lyr = _Image.Layers.Add("Rectangles", c1, aLineWeight:=dxxLineWeights.ByDefault)



            'draw.aLine(zRec.BottomLeft, zRec.TopRight, aColor:=dxxColors.Orange, bSuppressUCS:=True)
            'Dim limline As dxeLine = draw.aLine(new dxfVector(-3, 63.0625), new dxfVector(3, 53.59375), aColor:=dxxColors.Orange, bSuppressUCS:=True)
            'Dim leg1_outside As dxeLine = draw.aLine(new dxfVector(3, 53.59375), new dxfVector(3, 56.90625), aColor:=dxxColors.Orange, bSuppressUCS:=True)
            ''_Image.Display.ZoomExtents()
            '_Image.Display.ZoomExtents()
            'Dim thk As Double = 0.105
            'draw.aLine(zRec.BottomLeft, zRec.TopRight, aColor:=dxxColors.Orange, bSuppressUCS:=True)
            ''_Image.Display.ZoomExtents()
            'Dim serr As String = ""
            'Dim ar2 As dxeArc = dxfPrimatives.FilletArc(limline, leg1_outside, 2 * thk, serr)
            '_Image.SaveEntity(ar2)
            '_SuppressZoomExtents = True

            Dim vers As New colDXFVectors(New dxfVector(-10, 10), New dxfVector(-10, -10), New dxfVector(10, -10), New dxfVector(10, 10))
            vers.Item(3).Color = dxxColors.Blue
            vers.Item(3).Linetype = "Hidden"
            vers.Item(1).VertexRadius = 15
            Dim v0 As New dxfVector(5, 5)
            vers.Translate(v0)



            Dim gname As String = "MIKES_GROUP"
            _Image.GroupName = gname
            Dim pg As New dxePolygon(vers, dxfVector.Zero, True, aDisplaySettings:=New dxfDisplaySettings("0", dxxColors.Red, dxfLinetypes.Continuous))
            _Image.Entities.Add(pg)
            Dim paperscale As Double = _Image.Display.ZoomExtents(bSetFeatureScale:=True)
            _Image.GroupName = String.Empty

            'dims.Stack_Ordinate(dxxOrdinateDimTypes.OrdVertical, vers.GetVector(dxxPointFilters.GetBottomLeft), vers, -0.5 * paperscale)
            v0 = dxfVector.Zero ' pg.BoundingRectangle().Center
            _Image.LinetypeLayers.Add("Center", "CLINES", dxxColors.Yellow)


            draw.aCenterlines(pg, 0.5 * paperscale)

            v1 = vers.GetVector(dxxPointFilters.GetLeftTop)
            v2 = vers.GetVector(dxxPointFilters.GetTopRight)
            Dim d1 As dxeDimension

            ' d1 = dims.OrdinateV(v0, v1, -0.5)

            'draw.aCircle(d1.DimensionPt1, 0.5, dxfDisplaySettings.Null(aColor:=dxxColors.Magenta))
            'draw.aCircle(d1.DimensionPt2, 0.25, dxfDisplaySettings.Null(aColor:=dxxColors.LightCyan))

            d1 = dims.OrdinateH(v0, v1, 0.5)
            draw.aCircle(d1.DimensionPt1, 0.5, dxfDisplaySettings.Null(aColor:=dxxColors.Magenta))
            draw.aCircle(d1.DimensionPt2, 0.25, dxfDisplaySettings.Null(aColor:=dxxColors.LightCyan))


            'd1 = dims.Horizontal(v1, v2, 0.3)
            'draw.aCircle(d1.DimensionPt1, 0.5, dxfDisplaySettings.Null(aColor:=dxxColors.Magenta))
            'draw.aCircle(d1.DimensionPt2, 0.25, dxfDisplaySettings.Null(aColor:=dxxColors.LightCyan))

            ' draw.aInsert(pg, new dxfVector(0, 0, 0), aRotationAngle:=0)

            '_Image.GroupName = gname
            'Dim rec As dxfRectangle = pg.BoundingRectangle()
            'v1 = rec.Center
            'Dim ar1 As dxeArc = draw.aCircle(v1, 0.1 * rec.Diagonal)
            'Dim ar2 As dxeArc = draw.aCircle(v1.Moved(0.25 * rec.Diagonal, -0.25 * rec.Diagonal), 0.1 * rec.Diagonal)


            'Dim block1 As dxfBlock = _Image.Blocks.Add(New dxfBlock("BLOCK1", aEntities:=New List(Of dxfEntity)({New dxePolyline(pg.Vertices, True), ar1.Clone(), ar2.Clone()})))

            'v1 = rec.TopRight + New dxfVector(3, -3)

            'Dim insert1 As dxeInsert = draw.aInsert(block1.Name, v1)
            'v1 = rec.BottomLeft + New dxfVector(3, -3)
            'Dim insert2 As dxeInsert = insert1.Clone()
            'insert2.ScaleFactor = 0.5
            'Dim block2 As dxfBlock = _Image.Blocks.Add(New dxfBlock("BLOCK2", aEntities:=New List(Of dxfEntity)({New dxePolyline(pg.Vertices, True), ar1.Clone(), ar2.Clone(), insert2})))
            'Dim insert3 As dxeInsert = draw.aInsert(block2.Name, v1)
            ''_Image.GroupName = String.Empty
            Return
            Dim s1 As Single
            Dim s2 As Single
            Dim a1 As Single

            c1 = dxxColors.ByLayer
            k = 0

            For i = 1 To EntCount
                v1 = dxfUtils.RandomPoint(zRec.Left, zRec.Right, zRec.Bottom, zRec.Top)
                k += 1
                s1 = dxfUtils.RandomSingle(0.5, 6)
                s2 = dxfUtils.RandomSingle(0.1, 0.5) * s1
                a1 = dxfUtils.RandomSingle(0, 360)
                c1 = dxxColors.ByLayer ' dxfUtils.Colors.RandomACIColor

                If k = 1 Then

                    draw.aCircle(v1, s1, aDisplaySettings:=dxfDisplaySettings.Null("Circles", c1))

                ElseIf k = 2 Then

                    draw.aRectangle(v1, s1, s2, aRotation:=a1, aDisplaySettings:=dxfDisplaySettings.Null("Rectangles", c1))

                ElseIf k >= 3 Then

                    draw.aEllipse(v1, s1 * 2, s2 * 2, aRotation:=a1, aDisplaySettings:=dxfDisplaySettings.Null(aLayer:="Ellipses", aColor:=c1))
                    k = 0

                End If

            Next
            _Image.Entities.SetDisplayVariable(dxxDisplayProperties.Color, dxxColors.Blue,, dxxEntityTypes.Circle)

        Catch ex As Exception
            _Image.HandleError("Code_1", "frmImages", ex.Message)
        End Try




        '_Image.Screen.Entities.aRectangle(New dxfVector(0, 0), 20, 20,,,, dxxColors.Cyan, True)
        '_Image.Bitmap.CopyToClipBoard()

    End Sub


    Private Sub Code_2()

        Dim v1 As dxfVector = dxfVector.Zero
        Dim txt As String

        Dim aArc As New dxeArc
        'Dim aFnts As New dxoFonts

        Dim aLayer As dxoLayer = _Image.Layers.Add("TEXT", dxxColors.Blue, dxfLinetypes.Continuous, bMakeCurrent:=True)

        _Image.Layers.Add("MARKERS", dxxColors.BlackWhite, dxfLinetypes.Continuous, bMakeCurrent:=False)
        aLayer.Description = "Mike's layer to test"
        Dim canc As Boolean = True
        Dim sty As dxoStyle = _Image.TextStyle
        'sty.SelectFont(Me)
        sty.FontName = "RomanD.shx" '"Arial.ttf" 
        'sty.FontName = "Arial NArrow.ttf"
        'aFnts.WriteToFile("")
        Dim fontst As dxoFontStyle = sty.GetFontStyle()
        Dim draw As dxoDrawingTool = _Image.Draw
        Dim aTxt As dxeText
        Dim v2 As dxfVector = Nothing
        '_Image.Header.QuickText = True

        ' _Image.Styles.AddTextStyle("ARIAL", "Arial.ttf",,,,,,, True)
        'txt = "\O\T2;Agy\T1;\o True TYPE FONTS - ARIAL.TTF"


        Dim aln As dxxMTextAlignments = dxxMTextAlignments.TopLeft

        txt = dxfEnums.Description(aln) ' "Ag"
        txt = "Ag...^!@$#j%ASZX\Paa\C6;KIL\PThr\H2x;eey"
        txt = "\A1;1{\H0.5x;2}3\PjiK" '\H200;j\H100;3" '\P\H300;3\H25;4j\H50;56y7\P\H300;678"
        txt = "\A2;{\H0.35x;y\H0.25x;\S234/aayAaa;}j\A0;{\H0.5x;C\P\C4;\FRomanS;1ABC}"

        ''txt = "Code_2"
        Dim ang As Single = 0
        Dim tht As Double = 1

        sty.Vertical = False
        'sty.ObliqueAngle = 25
        sty.LineSpacingStyle = dxxLineSpacingStyles.AtLeast
        sty.LineSpacingFactor = 1
        sty.Backwards = False
        sty.UpsideDown = False

        v1.SetCoordinates(0, 0)

        Dim ttype As dxxTextTypes = dxxTextTypes.Multiline
        Dim dims As dxoDimTool = _Image.DimTool

        txt = "\LTHIS Is RomanS\l\P\FRomanD.shx;This Is RomanD.shx\P\FRomanT.shx;\C6;This Is RomanT.shx\P\O\T2;COLOR 6"
        txt = "84 - %%C19 HOLES\PNEXT LINE"
        txt = fontst.FontName & "\P" & fontst.StyleName & "\P1234567890"
        'txt = "1234567890"
        'txt = "QI2n3R"
        txt = "\A0;A\H1.0x;BC\P\H1.5x;\W1.0;DEF" '\PDEFG"
        txt = "A  BC\P123X"
        txt = $"\A2;Aa BbJj\P1\H0.5x;23\C{CInt(dxxColors.Red)};X"

        txt = "\A1;1{\H0.5x;\S^2;} 2{\H0.5x;\S1^;}\P\H0.5x;SubScript and SuperScript j"  'superscript & sub script
        'txt = "\A1;1{\H0.25x;\S7/2;}\P\H0.5x;Vertical Stack j"  'horizontal stack fraction
        'txt = "\A1;1{\H0.25x;\S7#2;}\P\H0.5x;Diagonal Stack j"  'diagonal fraction
        'txt = "\A1;1{\H0.25x;\S7~22;}\P\H0.5x;Decimal Stack j"  'diagonal fraction
        txt = "\A1;1{\H0.5x;\S^2;} 2{\H0.5x;\S1^;}"
        txt += $"\P{dxfEnums.Description(sty.LineSpacingStyle)}"
        'txt = "X{\H0.5x;X}X"
        'txt = "\T2;1234567890AaBbCcDdEeFfGgHhIiJj"
        'txt = "\T1;XXXXXXjabcq1234567"

        txt = "AaBbCcQqJj"
        txt = "A"
        txt = "{\A1;100.0000}\PABCD"
        ang = 0
        ttype = dxxTextTypes.Multiline
        v1.SetCoordinates(0, 0)
        tht = 100
        Dim algnm As dxxMTextAlignments = dxxMTextAlignments.BaselineLeft

        'Return
        aTxt = _Image.EntityTool.Create_Text(v1, txt, tht, aAlignment:=algnm, aTextAngle:=ang, aTextType:=ttype, aLayer:="TEXT", aAlignmentPt2XY:=v2)

        'aTxt.UpdatePath()
        'aTxt.Translate(10, -5)
        'aTxt.UpdatePath(True, _Image)
        _Image.SaveEntity(aTxt)

        Dim markDsp As New dxfDisplaySettings(aLayer:="MARKERS")
        Dim chr As dxoCharacter = aTxt.Character(1)
        Dim rec As dxfRectangle = aTxt.BoundingRectangle
        Dim aPln As New dxfPlane(aTxt.Plane)

        DrawCharBoxes(aTxt, bSuppressGrid:=True, bSuppressCharBoxes:=True, bSuppressBounds:=False, bSuppressChars:=True, bSuppressStrings:=True, bSuppressString:=False, bCirclesOnPathPts:=False, bCirclesOnExtentPts:=True, bDrawBaseLines:=True)

        Dim paperscale As Double = _Image.Display.ZoomExtents(bSetFeatureScale:=True)
        draw.aDots(dxxDotShapes.Square, aTxt.BoundingRectangle().Corners(), 0.055 * paperscale, New dxfDisplaySettings(aLayer:="MARKERS", aColor:=dxxColors.Blue))
        draw.aDot(dxxDotShapes.Square, v1, 0.025 * paperscale, New dxfDisplaySettings(aLayer:="MARKERS", aColor:=dxxColors.Red))
        If Not IsNothing(v2) Then draw.aCircle(v2, 0.085 * paperscale, aDisplaySettings:=dxfDisplaySettings.Null(aLayer:="MARKERS", aColor:=dxxColors.Red))

        Dim cbox As dxoCharBox = aTxt.Character(1).CharacterBox

        _Image.DimStyleOverrides.ShowTextBoxes = True
        _Image.DimStyleOverrides.FeatureScaleFactor *= 0.5
        draw.aDim.Vertical(cbox.BasePt, cbox.TopLeft, -0.2)
        ' draw.aDim.Horizontal(cbox.BasePt, cbox.BaselineRight, -0.2)
        If aTxt.LineCount > 1 And Not aTxt.Vertical Then
            'Dim v0 As dxfVector
            'If aTxt.LineSpacingStyle = dxxLineSpacingStyles.Exact Then
            '    v0 = aTxt.Strings.Item(0).Bounds.BottomLeft
            'Else
            '    v0 = aTxt.Strings.Item(0).BaseLine().StartPt
            'End If


            '
            'dims.OrdinateV(v0, v0, -0.25)
            'For i As Integer = 2 To aTxt.Strings.Count
            '    Dim dp As dxfVector
            '    If aTxt.LineSpacingStyle = dxxLineSpacingStyles.Exact Then
            '        dp = aTxt.Strings.Item(i - 1).Bounds.BottomLeft
            '    Else
            '        dp = aTxt.Strings.Item(i - 1).BaseLine().StartPt
            '    End If

            '    dims.OrdinateV(v0, dp, -0.25)

            '    dims.Vertical(aTxt.Strings.Item(i - 2).Bounds.BottomRight, aTxt.Strings.Item(i - 1).Bounds.TopRight, 0.75)
            'Next i

        End If

        'For i As Integer = 1 To aTxt.Strings.Count
        '    Dim str As dxoString = aTxt.Strings.Item(i - 1)
        '    Dim vischars As List(Of dxoCharacter) = str.Characters.VisibleCharacters()
        '    For Each schr As dxoCharacter In vischars
        '        If schr.StackID <= 0 Then
        '            _Image.Entities.Add(schr.CharacterBox.MidLine)
        '        End If
        '    Next


        'Next i
        'Dim algnmentrecs As List(Of dxfRectangle) = aTxt.Strings.GetAlignmentBoundaries()
        'For Each arec As dxfRectangle In algnmentrecs
        '    draw.aRectangle(arec, True)
        'Next
        draw.aRectangle(aTxt.Strings.GetAlignmentBounds(), True, aLayer:="MARKERS")

        'draw.aRectangle(aTxt.BoundingRectangle(), True, aColor:=dxxColors.Cyan)
        Return





    End Sub

    Private Sub DrawCharBoxes(aTxt As dxeText, Optional bSuppressGrid As Boolean = True, Optional bSuppressCharBoxes As Boolean = False, Optional bSuppressBounds As Boolean = True,
                                                Optional bSuppressChars As Boolean = False, Optional bSuppressStrings As Boolean = False, Optional bSuppressString As Boolean = True,
                                                Optional bCirclesOnPathPts As Boolean = False, Optional bCirclesOnExtentPts As Boolean = False, Optional bDrawBaseLines As Boolean = False)
        Dim i As Integer
        Dim j As Integer

        Dim aRec As dxfRectangle
        Dim apl As dxePolyline
        Dim v1 As dxfVector
        Dim v2 As dxfVector
        Dim v3 As dxfVector
        Dim v4 As New dxfVector
        Dim aWCS As New dxfPlane
        Dim d1 As Double
        Dim d2 As Double
        Dim d3 As Double
        Dim draw As dxoDrawingTool = _Image.Draw
        Dim markDsp As New dxfDisplaySettings(aLayer:="MARKERS")
        Dim lcolor As dxxColors = _Image.Layer("TEXT").Color
        Dim brec As dxfRectangle
        Dim evecs As colDXFVectors
        aRec = aTxt.BoundingRectangle
        v1 = aRec.TopLeft
        v2 = aRec.BottomRight

        d1 = aRec.Height
        d2 = d1 / 4
        d3 = d2 / 5
        If Not bSuppressGrid Then
            _Image.Layers.Add("GRID", dxxColors.Orange, dxfLinetypes.Hidden2)
            _Image.Layers.Add("GRIDM", dxxColors.Black, "Dot")
            v3 = v1.Clone
            For i = 1 To 3
                v2 = v3.Clone
                For j = 1 To 4
                    v2.Project(aRec.YDirection, -d3)
                    draw.aLine(v2, aRec.XDirection, aRec.Width, "GridM")

                Next

                v3.Project(aRec.YDirection, -d2)
                draw.aLine(v3, aRec.XDirection, aRec.Width, "Grid")
            Next
            v2 = v3.Clone
            For j = 1 To 4
                v2.Project(aRec.YDirection, -d3)
                draw.aLine(v2, aRec.XDirection, aRec.Width, "GridM")

            Next


            d1 = aRec.Width
            d2 = d1 / 4
            d3 = d2 / 5

            v3 = v1.Clone
            For i = 1 To 3
                v2 = v3.Clone
                For j = 1 To 4
                    v2.Project(aRec.XDirection, d3)
                    draw.aLine(v2, aRec.YDirection, -aRec.Height, "GridM")

                Next

                v3.Project(aRec.XDirection, d2)
                draw.aLine(v3, aRec.YDirection, -aRec.Height, "Grid")
            Next
            v2 = v3.Clone
            For j = 1 To 4
                v2.Project(aRec.XDirection, d3)
                draw.aLine(v2, aRec.YDirection, -aRec.Height, "GridM")

            Next
        End If
        Dim tstrings As dxfStrings = aTxt.Strings
        For Each tstring As dxoString In tstrings

            If bDrawBaseLines Then _Image.Entities.Add(tstring.BaseLine(aTxt.DisplaySettings.Copy(aLayer:=markDsp.LayerName)))
            Dim schars As List(Of dxoCharacter) = tstring.Characters.VisibleCharacters
            For Each schar As dxoCharacter In schars
                If Not bSuppressBounds And Not bSuppressChars Then

                    apl = draw.aRectangle(schar.GetBounds(True), False, aLayer:=markDsp.LayerName, aColor:=dxxColors.Cyan)
                End If
                If Not bSuppressCharBoxes And Not bSuppressChars Then
                    apl = draw.aRectangle(schar.CharacterBox, True, aLayer:=markDsp.LayerName, aColor:=dxxColors.LightCyan)
                End If

                If bCirclesOnPathPts Then
                    evecs = schar.PathPoints

                    draw.aCircles(evecs, 0.01 * aTxt.TextHeight, schar.DisplaySettings.Copy(aLayer:=markDsp.LayerName, aColor:=lcolor))
                End If

                If bCirclesOnExtentPts Then
                    draw.aCircles(schar.ExtentPoints(), 0.02 * aTxt.TextHeight, schar.DisplaySettings.Copy(aLayer:=markDsp.LayerName, aColor:=lcolor))
                End If
            Next


            If Not bSuppressBounds And Not bSuppressStrings Then
                apl = draw.aRectangle(tstring.Bounds, False, aLayer:=markDsp.LayerName, aColor:=dxxColors.Green)
            End If
            If Not bSuppressCharBoxes And Not bSuppressStrings Then
                apl = draw.aRectangle(tstring.CharacterBox, True, aLayer:=markDsp.LayerName, aColor:=dxxColors.LightGreen)
            End If


            If bCirclesOnExtentPts Then
                brec = tstring.GetBounds()
                evecs = brec.Corners
                draw.aCircles(evecs, 0.025 * aTxt.TextHeight, New dxfDisplaySettings(markDsp.LayerName, aColor:=dxxColors.Orange))
            End If

        Next

        brec = tstrings.BoundingRectangle
        evecs = brec.Corners
        If bCirclesOnExtentPts Then
            draw.aCircles(evecs, 0.03 * aTxt.TextHeight, New dxfDisplaySettings(markDsp.LayerName, aColor:=dxxColors.Yellow))
        End If

        If Not bSuppressString Then

            If Not bSuppressBounds Then
                apl = draw.aRectangle(brec, False, aLayer:=markDsp.LayerName, aColor:=dxxColors.Yellow)
            End If
            If Not bSuppressCharBoxes Then
                apl = draw.aRectangle(tstrings.CharacterBox, True, aLayer:=markDsp.LayerName, aColor:=dxxColors.LightYellow)
            End If


        End If



        '        Dim cBoxes As colDXFEntities = aTxt.CharacterBoxes(bTextRectangles:=True, aColor:=dxxColors.LightBlue)
        '        _Image.Entities.Append(cBoxes)
        '_Image.Screen.NumberVectors(apl.Vertices, 2)

    End Sub

    Private Sub Code_3()
        'Dim aShapes As New dxoShapes("C:\Junk\Fonts\txt.shp")
        Dim aRec As dxePolyline
        Dim aArc2 As dxeArc
        Dim gname As String = ""
        gname = "MIKES_GROUP"
        _Image.GroupName = gname

        aRec = _Image.Draw.aRectangle(New dxfVector(0, 0), 100, 100)
        aArc2 = _Image.Draw.aCircle(aRec.Vertex(1), 10)

        aArc2.Radius = 20

        _Image.DimStyleOverrides.DimLineColor = dxxColors.Red

        'aRec.FilletAtVertex(2, 10)
        Dim v1 As New dxfVector("0,75")
        Dim v2 As dxfVector
        Dim vrts As New colDXFVectors ' = dxfUtilities.RandomPoints(2, -20, 20, -20, 20)
        Dim aArc As dxeArc
        Dim aDim As dxeDimension
        vrts.Add(-5, -5, 0)
        vrts.Add(10, 10, 0)
        _Image.DimStyleOverrides.AutoReset = True
        _Image.DimStyleOverrides.SuppressExtLine1 = True
        _Image.DimStyleOverrides.ArrowHead1 = dxxArrowHeadTypes.BoxBlank
        ''_Image.Screen.ExtentPts = True
        'aArc = _Image.Draw.aCircle(vrts, 1.5)
        _Image.Display.ZoomExtents(1.05, True)

        _Image.Header.LineTypeScale = 12
        '_Image.Screen.NumberVectors(vrts, 2)
        'vrts = aArc.ExtentPoints
        '_Image.Screen.NumberVectors(vrts, 1.2, dxxColors.Grey)

        v1 = aRec.Vertices.Add(v1, aAfterIndex:=2)
        v2 = aRec.Vertices.Item(3)
        v1 = aRec.Vertices.Item(3)


        Dim a3 As dxeArc
        a3 = New dxeArc(New dxfVector(0, 0, 0), 1,, 50) With {.Color = dxxColors.Green}


        'v1.Y = 25

        '_Image.Screen.ExtentPts = True

        _Image.Screen.NumberVectors(aRec.Vertices, 2)

        Dim l1 As New dxeLine(New dxfVector(-75, -50), New dxfVector(75, 50)) With {
        .Color = dxxColors.Orange,
        .Linetype = "Phantom"}

        l1.Instances.CreateRectangularArray(1, 5, 0, 25)
        _Image.Entities.Add(l1)
        _Image.Display.ZoomExtents()
        'Dim l1 As dxeLine = _Image.Draw.aLine(New dxfVector(-75, -50), New dxfVector(75, 50),, dxxColors.Orange, "Phantom")


        Dim ipts As colDXFVectors = l1.Intersections(aRec)
        Return

        'For i As Integer = 1 To ipts.Count
        '    v2 = ipts.Item(i)
        '    Debug.Print(i & ")" & v2.ToString)

        '    _Image.Draw.aCircle(v2, 1)
        'Next

        aArc = _Image.Draw.aCircle(ipts.FirstOrDefault, 2, dxfDisplaySettings.Null(aColor:=dxxColors.Blue))

        ipts = aArc.Intersections(l1)
        _Image.Header.PointSize = -0.5
        ' _Image.Header.PointMode = dxxPointModes.dxfPDCircCross

        _Image.Draw.aCircles(ipts, 0.5, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Green))

        ' _Image.Draw.aPoint(aArc.PathPoints)
        'v1.Y = 50

        Dim aSld As New dxeSolid(New dxfVector(0, 0, 0), New dxfVector(-20, 20, 0), New dxfVector(20, 20, 0))

        _Image.Entities.Add(aSld)

        'MsgBox(_Image.DimStyle.ArrowHeadBlock & vbLf & _Image.DimStyle.ArrowHeadBlock1 & vbLf & _Image.DimStyle.ArrowHeadBlock2)

        _Image.GroupName = ""

        aDim = _Image.DimTool.Horizontal(aRec.Vertex(1), aRec.Vertex(5), 0.5, 0)

        _Image.DimStyleOverrides.ReName("MIKES_DIMSTYLE")
        _Image.DimStyleOverrides.AutoReset = False
        _Image.DimStyleOverrides.ExtLineColor = dxxColors.Blue
        _Image.DimStyleOverrides.SetArrowHeads(dxxArrowHeadTypes.Open90, dxxArrowHeadTypes.Integral, dxxArrowHeadTypes.Open90)

        aDim = _Image.DimTool.Vertical(aRec.Vertex(4), aRec.Vertex(5), 0.75)

        Dim ldr As dxeLeader = _Image.LeaderTool.Text(aRec.Vertex(1), aRec.Vertex(1) + New dxfVector(-50, 20), "TEST")

        '' _Image.Entities.Add(ldr.MText.Clone)
        'Dim prop As dxxDimStyleProperties
        'prop = dxxDimStyleProperties.DIMAUNIT
        '_Image.DimStyle.SetProperty(prop, dxxAngularUnits.DegreesMinSec)


        '_Image.Draw.aEllipse(New dxfVector, 100, 50,,,,,, dxxColors.Green)
        ' _Image.Display.ZoomExtents()
        'aShp = aShapes.Item("T")
        'Debug.Print(Asc("T") & " // " & aShp.ToString)

        'For i As Integer = 1 To aShapes.Count
        '    aShp = aShapes.Item(i)
        '    Debug.Print(aShp.ToString)
        '    If aShp.Name = "T" Then MsgBox(aShp.ToString)
        'Next



    End Sub

    Private Sub Code_4()

        Dim v1 As dxfVector

        Dim v2 As dxfVector
        Dim aDim As dxeDimension
        Dim aRec As dxePolyline = _Image.Draw.aRectangle(New dxfVector, 20, 10)
        Dim dstyle As dxoDimStyle = _Image.DimStyle
        dstyle.ExtLineColor = dxxColors.Green
        'dstyle.ArrowHead1 = dxxArrowHeadTypes.ClosedFilled
        'dstyle.ArrowHead2 = dxxArrowHeadTypes.Integral
        dstyle.DimLineColor = dxxColors.Blue
        dstyle.TextColor = dxxColors.Magenta
        dstyle.ExtLinetype1 = "Hidden"
        dstyle.SuppressDimLine1 = False

        _Image.Display.ZoomExtents(2, bSetFeatureScale:=True)

        v1 = aRec.BoundingRectangle.TopLeft
        v2 = aRec.BoundingRectangle.TopRight

        aDim = _Image.DimTool.Linear(dxxLinearDimTypes.LinearHorizontal, v2, v1, -0.5)
        _Image.Styles.Add("RomanD", "RomanD.shx")


        dstyle.FeatureScaleFactor *= 2
        dstyle.ExtLineColor = dxxColors.Red
        'dstyle.ArrowHead1 = dxxArrowHeadTypes.BoxBlank
        'dstyle.ArrowHead2 = dxxArrowHeadTypes.ClosedBlank
        dstyle.DimLineColor = dxxColors.Magenta
        dstyle.TextColor = dxxColors.Orange
        dstyle.LinearPrecision = 1
        dstyle.ExtLinetype1 = "ByBlock"
        dstyle.SuppressDimLine1 = False
        dstyle.TextStyleName = "RomanD"

        v1 = aRec.BoundingRectangle.TopRight
        v2 = aRec.BoundingRectangle.BottomRight

        aDim = _Image.DimTool.Linear(dxxLinearDimTypes.LinearVertical, v1, v2, 0.5)

        v1 = aRec.BoundingRectangle.BottomLeft
        v2 = aRec.BoundingRectangle.TopRight

        Dim v3 As dxfVector = aRec.BoundingRectangle.BottomRight

        Dim aL As dxeLine = _Image.Draw.aLine(v1, v2,, dxxColors.Grey)
        v2 = v1.FractionPoint(v2, 0.25)
        v3 = v1.FractionPoint(v3, 0.25)


        _Image.DimTool.Angular3P(v1, v2, v3, 0.5 * aL.Length)

        Dim aCirc As dxeArc = _Image.Draw.aCircle(aRec.Center, 5)
        _Image.Draw.aDim.RadialD(aCirc, 60, 1.25 * aCirc.Radius)


        '_Image.Draw.aInsert("_ClosedFilled", v1,, 1)


    End Sub

    Private Sub Code_5()

        Dim v1 As dxfVector = dxfVector.Zero

        Dim v2 As New dxfVector(2, 0, 0)


        _Image.Draw.aInsert("_ClosedFilled", v1, 90, 1)
        _Image.Draw.aInsert("_ClosedBlank", v2, 90, 1)

        _Image.Display.ZoomExtents()
    End Sub

    Private Sub Code_6()

        Dim v1 As New dxfVector(10, 5, 0)

        Dim v2 As New dxfVector(v1, _Image.UCS, -45, 10)
        Dim aRec As New dxfRectangle(10, 10)
        Dim aPline As New dxePolyline(aRec.Corners, True)

        aPline.FilletAtVertex(1, 1.5)

        _Image.Entities.Add(aPline)

        _Image.Display.ZoomExtents()

        '_Image.Screen.NumberVectors(aPline.Vertices)

        aPline.FilletAtVertex(4, 2.5, True)
        aPline.UpdateImage()

        Dim aPlane As New dxfPlane(aPline.Vertex(1))
        aPlane.Tip(25)
        _Image.Screen.Entities.aAxis(aPlane)

        Return

        Dim rad As Double = 12
        Dim l1 As dxeLine = _Image.Draw.aLine(v1, v2)
        Dim aArcs As List(Of dxeArc) = dxfPrimatives.ArcsBetweenPoints(rad, l1.StartPt, l1.EndPt)
        Dim aAr As dxeArc
        Dim bAr As dxeArc

        aAr = _Image.Draw.aArc(l1.StartPt, rad, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Blue, aLinetype:="Hidden"))
        bAr = _Image.Draw.aArc(l1.EndPt, rad, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Green, aLinetype:="Hidden"))
        _Image.Display.ZoomExtents()
        _Image.Header.PointMode = dxxPointModes.CircCross
        Dim ips As colDXFVectors = aAr.Intersections(bAr)

        _Image.Draw.aPoint(ips)

        For i As Integer = 1 To aArcs.Count

            aAr = aArcs.Item(i)
            aAr.Color = i
            _Image.Entities.Add(aAr)

            'Debug.Print(i & ") " & aAr.Descriptor)
        Next

    End Sub

    Private Sub Code_7()

        Dim v1 As New dxfVector(10, 5, 0)
        Dim v2 As New dxfVector(v1, _Image.UCS, -45, 10)
        Dim aRec As New dxfRectangle(100, 10)
        Dim aPl As New dxfPlane()
        aRec.Y = 50
        Dim aPline As New dxePolyline(aRec.Corners)
        aPline.Vertex(1).Move(-10)
        aPline.Vertex(4).Move(10)
        aPline.Closed = True

        _Image.Draw.aLine(New dxfVector(-100, 0), New dxfVector(100, 0),, dxxColors.Red, "Center")

        '_Image.Entities.Add(aPline,,, True)

        'aPline = aPline.Clone()

        'aPline.MirrorAboutLine(aPl.XAxis)

        aPline.Instances.Add(0, -100, bInverted:=True)

        _Image.Entities.Add(aPline)
        _Image.Display.ZoomExtents()

        Return


    End Sub

    Private Sub Code_8()

        Dim v1 As New dxfVector(10, 5, 0)
        Dim v2 As New dxfVector(v1, _Image.UCS, -45, 10)
        Dim aPg As New dxePolygon
        Dim aRec As New dxfRectangle(100, 10)
        Dim aPl As New dxfPlane()
        Dim aEnts As colDXFEntities
        aRec.Y = 50

        aPg.Vertices.Append(aRec.Corners)
        aPg.Closed = True

        aPg.Vertex(2).Linetype = "Hidden"
        aPg.Vertex(2).Color = dxxColors.Blue



        aPg.AdditionalSegments.Add(New dxeArc(aRec.Center, 3))
        _Image.Entities.Add(aPg)

        aEnts = aPg.SubEntities(bIncludeInstances:=False)
        aEnts.Move(0, -50)
        For Each ent As dxfEntity In aEnts
            _Image.Entities.Add(ent)
        Next


        _Image.Display.ZoomExtents()

    End Sub

    Private Sub Code_9()


        Dim P1 As New dxfVector
        '**UNUSED VAR** Dim P2 As New dxfVector
        '**UNUSED VAR** Dim aLine As dxeLine
        Dim l1 As dxeLine
        '**UNUSED VAR** Dim aDir As New dxfDirection
        Dim aDim As dxeDimension
        '**UNUSED VAR** Dim aAr As dxeArc
        '**UNUSED VAR** Dim aH As dxeHole
        '**UNUSED VAR** Dim aLd As dxeLeader
        Dim lng As Double
        Dim ang As Double
        Dim toset As Double
        Dim txt As String = ""
        Dim oset As Double

        With _Image.DimSettings
            .DimLayer = "DIMS"
            .DimLayerColor = dxxColors.Cyan
        End With
        Dim draw As dxoDrawingTool = _Image.Draw
        With draw

            _Image.Display.SetDisplayWindow(120, P1, , True, True)
            _Image.Display.SetFeatureScales(2 * _Image.Display.PaperScale)
            '    _Image.Header.LineWeightDisplay = True

            _Image.Styles.Add("MIKE", "RomanS.shx")
            _Image.Screen.PointSize = 4
            P1 = New dxfVector
            _Image.Layers.Add("MIKE", dxxColors.Blue)
            _Image.DimSettings.DimTickLength = 0.15

            '        _Image.Header.LineWeightDefault = LW_025
            '        _Image.Header.LineWeightScale = 1
            '        .TextStyle.FontName = "Arial"

            '        _Image.DimSettings.DimLayer = "DIMS"
            '        _Image.DimSettings.DimLayerColor = dxfCyan

            '        _Image.TextSettings.LayerColor = dxfGreen

            '        Set p2 = l1.MidPt.Projected(.Utilities.CreateDirection(-1, 1), 0.4 * l1.Length)
            '        Set aLd = draw.aLeader.Text(l1.MidPt, p2, "MIKE1")
            '        aLd.DimStyle.DimLineColor = dxfBlue
            '        _Image.Display.Redraw

            '        Set aDim = draw.aDimension_L(LinearHorizontal, l1.StartPt, l1.EndPt, 0.5, 0, "", "", False, "", 0, 0, "STANDARD")
            'GoTo Done:

            '        _Image.DimStyles.AddDimStyle "MIKE", "Bill", True

            With _Image.DimStyleOverrides
                '            .FeatureScaleFactor = _Image.Display.ZoomFactor
                .TextAngle = -25
                '            .TextInsideHorizontal = False
                '            .TextOutsideHorizontal = False
                '            .LinearFormatType = Architectural
                '            .LinearPrecision = 3
                .ArrowHeads = dxxArrowHeadTypes.Open90
                '            .ArrowHead1 = BoxBlank
                '            .FractionStackType = Diagonal
                '.ArrowHeadBlock = "ArchTick"
                '.ArrowHeadBlock1 = "DotBlank"
                '            .SuppressExtLine1 = True
                '            .DimLineColor = dxfBlue
                .TextColor = dxxColors.LightMagenta
                .TextStyleName = "MIKE"
                .TextGap = - .TextGap
                .ExtLinetype1 = "Hidden"
                .TextInsideHorizontal = False
                .TextOutsideHorizontal = False
                '.DimLinetype = "Phantom"
                '            .SetColors dxfBlue, dxfBlackWhite
                '            .TextPositionH = Ext1
                '            .TextFit = MoveArrowsFirst
                '            .TextPositionV = Above
                '            .TextPositionH = Ext2
                '            .SuppressDimLine1 = True
                '            .SuppressDimLine2 = True
                '            .ForceDimLines = True
                '            .ArrowSize = 0
                '           .ArrowTickSize = 0.09
                '            .Name = "MIKE"
            End With
            '        With _Image.DimStyleOverrides
            '            .ArrowHeadBlock1 = "DotBlank"
            '        End With

            lng = 25
            ang = -22.5
            toset = 0
            oset = 1
            '        txt = "XYXXX"
            l1 = draw.aLine(P1, ang + 90, 0.5 * lng, aLayer:="MIKE")

            l1 = draw.aLine(P1, ang, lng, aLayer:="MIKE")
            _Image.Display.ZoomExtents(1.1, True)
            '_Image.Display.SetDisplayWindow(50, l1.MidPt, 50, True)
            '_Image.DimStyle.FeatureScaleFactor = 0.5 * _Image.Display.PaperScale


            aDim = draw.aDim.Horizontal(l1.StartPt, l1.EndPt, -oset, toset, "", "", False, txt)

            Dim aRec As dxfRectangle = aDim.TextPrimary.BoundingRectangle
            Dim epts As colDXFVectors = aDim.TextPrimary.ExtentPoints

            aRec.Stretch(0.36000001430511475)
            _Image.Draw.aRectangle(aRec, aColor:=dxxColors.Green)
            Dim blines As colDXFEntities = aRec.Borders(New dxfDisplaySettings("", dxxColors.LightBlue))
            '_Image.Entities.Append(blines)
            Dim l3 As dxeLine = aDim.DimensionLine1

            Dim ipts As colDXFVectors = l3.Intersections(blines, True, False)
            _Image.Draw.aCircles(ipts, 0.05 * aRec.Height)
            'Return
            aDim = draw.aDim.Vertical(l1.StartPt, l1.EndPt, -oset, toset, "", "", False, txt)
            aDim = draw.aDim.Aligned(l1.StartPt, l1.EndPt, oset, toset, "", "", False, txt)
            .aDim.TickLine(aDim, 1, -0.2, aColor:=dxxColors.Red, aLineType:=dxfLinetypes.Continuous)
            .aDim.TickLine(aDim, 2, -0.2, aLineType:=dxfLinetypes.Continuous)

            Return

            '    _Image.AutoRedraw = False
            '    With _Image.TextStyle("MIKE")
            '        .WidthFactor = 0.5
            '        .FontName = "Arial"
            '    End With
            '    _Image.AutoRedraw = True

            '        Set aDim = aDim.OrdinateV(  Nothing, l1.StartPt, -0.75, 0.1)

            '        _Image.DimStyle.Name = "JIM"

            '        Set aDim = draw.aDimension_L(LinearVertical, l1.EndPt, l1.StartPt, -0.3, 0, "", "", False, "", 1, -0.5, "STANDARD")
            .aCircle(l1.StartPt, 0.3, , dxxColors.Green)
            'aDim.ReScale 2, aDim.DefPt13
            aDim = draw.aDim.Aligned(l1.StartPt, l1.EndPt, oset, toset, "", "", False, txt)

            aDim = draw.aDim.Horizontal(l1.StartPt, l1.EndPt, -oset, toset, "", "", False, txt)

            _Image.Entities.GetByGraphicType(dxxGraphicTypes.Dimension).Move(4, 4)
            '
            '        aDim.InstancesAdd 35, 0, 2

            '        aDim.Rescale 2
            '        aDim.UpdateImage
            '        aDim.DefPt14.Move 14, 8
            '        aDim.DimStyle.Update
            '        aDim.UpdateImage True

            '
            '
            '        Set aDim = draw.aDimension_L(LinearAligned, l1.StartPt, l1.EndPt, 0.5, 0, "", "", False, "", 0, 0, "STANDARD")
            '
            '        aDim.DimStyle.ExtLineExtend = 4 * aDim.DimStyle.ExtLineExtend

            '        .aText Nothing, "Firsty" & utils_CreateStackedText(1, "2", dxfCharacterStack_Vertical, dxfCharAlign_Center, 0) & "X"

            '        aDim.DimStyle.ExtLineColor = dxfRed
            '       aDim.DimStyle.DimLineColor = dxfBlue
            '       aDim.DimStyle.TextColor = dxfLightGreen
            '        aDim.DimStyle.SuppressExtLine1 = True

            '        Set aDim = aDim.Clone
            '        aDim.DimStyle.LinearScaleFactor = 0.5
            '        aDim.ReScale 2, aDim.DimensionPt2
            '       oIMage.Entities.Add aDim

            '        MsgBox aDim.MText.Alignmentname
            '        .aCircle aDim.MText.InsertionPt, 0.25
            '        .aCircle aDim.MText.TextRectangle.Center, 0.125
            '        .aPolyline aDim.MText.TextRectangle.Corners, True
            '        .aPolyline aDim.MText.Rectangle.Corners, True

            '    aDim.DimStyle.ArrowHeadBlock1 = "Integral"
            '
            '        aDim.DimStyle.SuppressExtLine1 = True
            '
            '        .Layers.Item("MIKE").Linetype = dxfLinetypes.Continuous

Done:

            '_Image.DisplayErrors()

            'MsgBox .DimStyle.SuppressDimLine1
        End With
    End Sub

    Private Sub Code_10()
        Dim P1 As New dxfVector
        Dim P2 As New dxfVector
        Dim aLine As dxeLine
        Dim aDir As New dxfDirection
        Dim d1 As dxeDimension
        Dim dims As dxoDimTool = _Image.DimTool


        With _Image.Draw

            _Image.Header.LineWeightDefault = dxxLineWeights.LW_005

            P1.SetCoordinates(2, -0.1225526)
            _Image.Display.SetDisplayWindow(10, P1)
            P1.SetCoordinates(0.2, 0.2)
            aDir.SetComponents(1, 1, 0)
            P2.MoveToAndProject(P1, aDir, 3)
            .aCircle(P1, 0.02, dxfDisplaySettings.Null(aColor:=dxxColors.Cyan))
            '_Image.TextStyle.SelectFont(Me)

            With _Image.DimStyleOverrides
                '            .Color = dxfByLayer
                '            .DimPrecision = 2
                .DimLineColor = dxxColors.Cyan
                .ExtLineColor = dxxColors.Blue
                .TextGap = - .TextGap
                .TextPositionV = dxxDimTadSettings.Above

                '            .DimTextColor = dxfGreen
                '            .DimExtLineExtend = 0.09
                '            .DimExtLineOffset = 0.09

                '            .DimTextPositionV = Above
            End With
            '        .Layers.Add "MIKE", vbBlue + 100, , ,, True
            '        .SetTextStyleProperty "", "FontName", "Arial"

            aLine = .aLine(P1, P2, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.DarkPurple))
            '        .d1ension_O OrdHorizontal, p1, p2, 0.5, -0.2, , , , , , , , , , , dxfGrey, dxfLightCyan
            '        Set d1 = d1.OrdinateH(  p1, p2, -1.5, -0.4)
            '        Set d1 = d1.OrdinateH(  p1, p2, -1.5, 0.4)
            '
            _Image.Display.ZoomExtents(4.5)
            '        .DimStyleOverrides.TextGap = -1 * .DimStyleOverrides.TextGap

            d1 = dims.OrdinateH(aLine.StartPt, aLine.EndPt, 1.5)
            dims.TickLine(d1, 1, 0.25)


            d1.DimensionPt1.Move(3, 3)
            d1.DimStyle.ExtLineColor = dxxColors.Grey
            d1.DimStyle.DimLineColor = dxxColors.Red
            d1.DimStyle.TextColor = dxxColors.Orange
            d1.DimStyle.TextGap = Math.Abs(d1.DimStyle.TextGap)
            d1.DimStyle.FeatureScaleFactor = 2
            d1.UpdateImage()


            d1 = dims.OrdinateH(aLine.StartPt, aLine.EndPt, 1.5, -0.4)

            '        Set d1 = dims.OrdinateV(  p1, p2, 1.5, -0.4)
            '        Set d1 = dims.OrdinateV(  p1, p2, 1.5, 0.4)
            d1 = dims.OrdinateV(P1, P2, -1.5, -0.4)
            d1 = dims.OrdinateV(P1, P2, -1.5, 0.4)
            d1.DimStyle.ExtLineColor = dxxColors.Blue
            d1.DimStyle.TextColor = dxxColors.LightYellow
            d1.UpdateImage()
            '        d1.Delete

            '        d1.Move 2, 2

            'Dim aAxis As dxeLine
            'aAxis = _Image.UCS.ZAxis(10, aLine.StartPt)

            '        d1.RotateAboutAxis aAxis, 2

            '       .d1ension_O OrdVertical, p1, p1, 3.5, -3
            '        .d1ension_O OrdVertical, p1, p1, -1.5, -0.4

Done:

            '        _Image.TextStyle.SelectFont Me
            '
            '        _Image.TextStyle.FontStyle = "Bold Italic"

            '============================================================================


        End With
    End Sub




    Private Sub Code_11()
        Dim p0 As New dxfVector
        Dim P1 As New dxfVector
        Dim P2 As New dxfVector
        Dim p3 As New dxfVector
        '**UNUSED VAR** Dim p4 As New dxfVector
        '**UNUSED VAR** Dim Pts As colDXFVectors
        '**UNUSED VAR** Dim aEl As dxeEllipse

        '**UNUSED VAR** Dim aA As dxeArc
        '**UNUSED VAR** Dim bA As dxeArc
        '**UNUSED VAR** Dim ac As dxeArc
        '**UNUSED VAR** Dim bC As dxeArc
        '**UNUSED VAR** Dim iPts As colDXFVectors
        '**UNUSED VAR** Dim aE As dxeEllipse
        Dim bLn As dxeLine
        Dim gLn As dxeLine
        '**UNUSED VAR** Dim i As Long
        '**UNUSED VAR** Dim aP As dxePolyline
        '**UNUSED VAR** Dim d1 As Double
        Dim aDim As dxeDimension




        With _Image.Draw

            P1.SetCoordinates(2, -0.1225526)
            _Image.Display.SetDisplayWindow(10, P1)
            '_Image.TextStyle.FontName = "RomanD"

            With _Image.DimSettings
                .DimLayerColor = dxxColors.Cyan
                .DimLayer = "DIMS"

            End With

            With _Image.DimStyle

                .LinearPrecision = 2
                .DimLineColor = dxxColors.LightRed
                .ExtLineColor = dxxColors.Cyan
                .SetGapsAndOffsets(0.09, 0.09)
                .TextColor = dxxColors.Orange
                '            .TextInsideHorizontal = False
                '            .ArrowTickSize = 0.2
                '            .TextGap = -1 * .TextGap
                '            .DimPrefix = "zxx "
                .ArrowHeads = dxxArrowHeadTypes.ByStyle
                '            .ArrowHeadBlock = "DAtumFilled"
                '            .AngUnits = DegreesDecimal
                '.Prefix = "XX "
                '            .TextOutsideHorizontal = False
                '            .TextPositionH = Ext2
                '            .TextPositionV = Above
                '            .ArrowSize = 0.18
                '            .ArrowTickSize = 0.1
                '            .SuppressDimLine1 = True
                '.ForceTextBetweenExtLine = True
                '            .ForceDimLines = True
                '.TextFit = MoveArrowsFirst
                '            .SuppressDimLine1 = True
                .AngularPrecision = 2
                .SetProperty(dxxDimStyleProperties.DIMTIH, False)
                .SetProperty(dxxDimStyleProperties.DIMTOH, False)
            End With

            _Image.UCS.Translate(20, 20, 0)
            Dim bSupUCS As Boolean = True

            p0 = _Image.CreateVector(0, 0, 0, 0)  'create a vector relative to the current UCS

            bLn = .aLine(p0, 10, -3, aSegmentPtType:=dxxSegmentPointTypes.StartPt, aColor:=dxxColors.Blue, bSuppressUCS:=bSupUCS)
            gLn = .aLine(p0, -30, -3, aSegmentPtType:=dxxSegmentPointTypes.StartPt, aColor:=dxxColors.Green, bSuppressUCS:=bSupUCS)

            _Image.Display.ZoomExtents(6, True)

            '        _Image.DimStyleOverrides.SuppressDimLine2 = True

            '   aDim = .aDim.Angular3P(gLn.StartPt, gLn.EndPt, bLn.EndPt, 1.25 * bLn.Length, bSuppressUCS:=bSupUCS)

            '        Set aDim = _Image.DimTool.Angular(bLn, gLn, , , p3, bSuppressUCS:=bSupUCS)

            '_Image.DimStyleOverrides.SuppressExtLine1 = True
            '_Image.DimStyleOverrides.SuppressExtLine2 = True


            _Image.Screen.PointSize = 4
            P2 = bLn.Point(50, bPercentPassed:=True)

            P1 = gLn.Point(120, bPercentPassed:=True)
            p3 = gLn.Point(1.25 * gLn.Length, bPercentPassed:=False)
            'p3.Rotate(gLn.StartPt, -45)
            aDim = .aDim.Angular3P(aCenterXY:=bLn.StartPt, aDimPT1XY:=bLn.EndPt, aDimPT2XY:=gLn.EndPt, aPlacementPointXY:=p3, bSuppressUCS:=bSupUCS) ' , ,aOffsetRadius:=1.25 * gLn.Length )

            _Image.Display.ZoomExtents()


            '.aCircle(aDim.DefPt13, 0.05, aColor:=dxxColors.Blue, bSuppressUCS:=bSupUCS)
            '.aCircle(aDim.DefPt14, 0.05, aColor:=dxxColors.Green, bSuppressUCS:=bSupUCS)
            '.aCircle(aDim.DefPt15, 0.05, aColor:=dxxColors.Green, bSuppressUCS:=bSupUCS)
            '.aCircle(aDim.DefPt10, 0.025, aColor:=dxxColors.Red, bSuppressUCS:=bSupUCS)


            'aDir = bLn.Direction
            'aDir.RotateAbout(Nothing, 12)
            'p3 = bLn.IntersectPoint(gLn)
            'p3.Project(aDir, 2.5)
            '_Image.Draw.aCircle(p3, 0.05, , dxxColors.Red)

            '_Image.DimStyleOverrides.SuppressExtLine1 = True
            '_Image.DimStyleOverrides.SuppressExtLine2 = True


            '' aDim = .aDim.Angular3P(p0, P1, P2, ,, p3)

            'aDir = gLn.Direction
            'aDir.RotateAbout(Nothing, -3)
            'p3 = bLn.IntersectPoint(gLn)
            'If Not IsNothing(p3) Then
            '    p3.Project(aDir, -5.5)
            '    _Image.Draw.aCircle(p3, 0.05, , dxxColors.Red, bSuppressUCS:=True)
            '    aDim = _Image.DimTool.Angular(bLn, gLn, , , p3, "BY LINES " & bLn.AngleTo(gLn), bSuppressUCS:=True)
            'End If

            _Image.Layer("DefPoints").Visible = True

            _Image.UCS.Reset()

        End With
    End Sub


    Private Sub Code_12()
        Dim p0 As New dxfVector
        Dim P1 As New dxfVector
        Dim P2 As New dxfVector
        '**UNUSED VAR** Dim p3 As New dxfVector
        '**UNUSED VAR** Dim p4 As New dxfVector
        '**UNUSED VAR** Dim Pts As colDXFVectors
        '**UNUSED VAR** Dim aD As dxfDirection
        Dim ac As dxeArc
        '**UNUSED VAR** Dim bC As dxeArc
        Dim aDim As dxeDimension
        Dim tp As dxxRadialDimensionTypes

        p0 = New dxfVector


        With _Image.Draw


            P1.SetCoordinates(2, -0.1225526)


            _Image.Display.SetDisplayWindow(10, P1)

            _Image.Header.UCSMode = dxxUCSIconModes.Origin

            With _Image.DimStyleOverrides
                .LinearPrecision = 2
                .DimLineColor = dxxColors.Red
                .ExtLineColor = dxxColors.Blue
                .TextColor = dxxColors.Green
                .ExtLineExtend = 0.09
                .ExtLineOffset = 0.09
                .TextGap = -1 * .TextGap
                '.CenterMarkSize = -1.0#
                '            .Prefix = "zxx "
                '            .ArrowBLock = "ArchTick"
                .TextOutsideHorizontal = False
                '            .TextInsideHorizontal = False
                '            .TextPositionH = AlignExt2
                '            .TextPositionV = Above
                '            .ArrowSize = 0
                '            .ArrowTickSize = 0.1
                '            .SuppressDimLine2 = True
                '.ForceTextBetweenExtLine = True
                '            .ForceDimLines = True
                '            .TextFit = MoveArrowsFirst
                '.AngularPrecision = 2
            End With
            P1.SetCoordinates(20, 3)
            P2.SetCoordinates(1.5, 2)

            tp = dxxRadialDimensionTypes.Radial
            ac = _Image.Draw.aCircle(P1, 4, , dxxColors.Green)

            '_Image.DimStyleOverrides.CenterMarkSize = -0.09

            aDim = .aDim.Radial(tp, ac, 45, 7)


            _Image.DimStyleOverrides.CenterMarkSize = 0.09
            _Image.DimStyleOverrides.TextOutsideHorizontal = True

            _Image.DimStyleOverrides.TextGap *= -1

            aDim = .aDim.Radial(tp, ac, 35, 7)
            _Image.DimStyleOverrides.CenterMarkSize = -0.09
            tp = dxxRadialDimensionTypes.Diametric
            _Image.DimStyleOverrides.TextOutsideHorizontal = False
            _Image.DimStyleOverrides.TextGap *= -1
            aDim = .aDim.Radial(tp, ac, -45, 2)


            '        aDim.DimStyle.ExtLineColor = dxfBlue
            '        aDim.DimStyle.DimLineColor = dxfGreen
            '        Set aDim = .aDim.Radial( tp, aC, 215, 0.05)

            '        Set aDim = .aDim.Radial( tp, aC, 315, 2, 0)
            '        Set aDim = .aDim.Radial( tp, aC, 130, 2.7, 0)


            '============================================================================
Done:


        End With
    End Sub


    Private Sub Code_13()
        Dim dPts As colDXFVectors
        '**UNUSED VAR** Dim i As Long
        Dim P1 As dxfVector
        Dim P2 As dxfVector

        '**UNUSED VAR** Dim aL As dxeLine
        Dim iPts As colDXFVectors
        '**UNUSED VAR** Dim bL As dxeLine
        '**UNUSED VAR** Dim aH As dxeHole
        '**UNUSED VAR** Dim aP As dxePolyline
        '**UNUSED VAR** Dim bP As dxePolyline
        '**UNUSED VAR** Dim bPGon As dxePolygon
        '**UNUSED VAR** Dim aPg As dxePolygon
        Dim aPGon As dxePolygon
        Dim xscal As Double
        Dim oset As Double
        Dim shft As Double
        Dim bInv As Boolean
        Dim bcws As Boolean
        Dim bnobase As Boolean
        Dim fctr As Double
        Dim cLns As colDXFEntities


        With _Image.Draw

            With _Image.DimSettings
                .DimLayer = "DIMS"
                .DimLayerColor = dxxColors.Cyan
            End With

            _Image.DimStyles.Add("MIKES_DIMSTYLE", bMakeCurrent:=True)

            _Image.Display.SetDisplayWindow(15, dxfVector.Zero, 15, True)

            '        .UCS.Tip 45

            With _Image.DimStyle 'Overrides

                .LinearPrecision = 2
                .DimLineColor = dxxColors.Cyan
                .ExtLineColor = dxxColors.Cyan
                .TextColor = dxxColors.Green
                .ExtLineExtend = 0.09
                .ExtLineOffset = 0.09
                .TextInsideHorizontal = False
                '            .TextGap = -1 * .TextGap
                .ArrowSize = 0.18
                .AngularPrecision = 2

            End With
            '    _Image.UCS.Tip 35

            _Image.LinetypeLayers.Add("Center", "Center", dxxColors.Red)
            _Image.LinetypeLayers.Add("Hidden", "", dxxColors.Blue)
            _Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.ForceToLayer


            aPGon = _Image.EntityTool.CreateShape_Polygon(6, 25, New dxfVector(1, 1), 0, True, 0, , dxxColors.Blue, "Hidden", bReturnPolygon:=True)
            aPGon.Vertex(2).Linetype = "Invisible"
            _Image.LinetypeLayers.ApplyTo(aPGon, dxxLinetypeLayerFlag.ForceToColor)
            '        Set aPGon = primative_Rectangle(vec_Null, True, 12, 12)

            '    aPGon.GroupName = "MIEKS"
            _Image.Entities.Add(aPGon)

            '_Image.Display.ZoomExtents(2.25, True)
            Dim paperscale As Double = _Image.Display.ZoomExtents(1.25, True) ' _Image.ComputeFeatureScale(0.125)
            '_Image.SetFeatureScales(paperscale)
            '    oimage.Draw.aDimension_L LinearAligned, aPGon.Vertex(1), aPGon.Vertex(2), 0.5

            iPts = aPGon.PhantomPoints(5, 6)

            dPts = iPts '.Jumbled
            '    dPts.Item(7).Flag = "NODIM"
            '    dPts.Item(8).Flag = "NODIM"
            '    dPts.Item(9).Flag = "NODIM"
            '    dPts.Item(10).Flag = "NODIM"
            '    dPts.SubSet(14, 17).SetTagsAndFlags , "V"
            'dPts.SubSet(15, 23).SetTagsAndFlags , "NODIM"

            '    _Image.UCS.Tip -35

            '    dPts.Item(23).Flag = "V"
            '    dPts.Item(24).Flag = "NODIM"
            '    dPts.Item(14).Flag = "NODIM"

            P1 = dPts.GetVector(dxxPointFilters.GetTopLeft)
            P2 = New dxfVector

            .aCircle(P1, 0.1, , dxxColors.Blue)

            xscal = _Image.Display.PaperScale
            oset = -0.45 * xscal
            shft = 0 * xscal
            bInv = False
            bcws = False
            fctr = 0# * xscal
            bnobase = False
            '    Set ipts = dPts.GetAtCoordinate(, dPts.GetOrdinate(dxfMaxY), , , , True)
            '    ipts.Move 0.25
            '    dPts.Append ipts
            .aCircles(dPts, 0.05, , dxxColors.Yellow)
            Dim ent As dxfEntity = aPGon
            cLns = New colDXFEntities(.aCenterlines(ent, -0.125 * xscal, , False))
            dPts.Append(cLns.EndPoints)
            P2 = Nothing
            P1 = Nothing


            Dim dims As List(Of dxeDimension) = _Image.DimTool.DimensionVertices(True, P1, dPts, P2, oset, bInv)

            Dim dents As New colDXFEntities(dims.OfType(Of dxfEntity)().ToList())
            Dim extpts As colDXFVectors = dents.ExtentPoints
            '_Image.Draw.aCircles(extpts, 0.07)
            _Image.Draw.aRectangle(extpts.BoundingRectangle(), aColor:=dxxColors.Yellow)


            '       _Image.EntityTool.Create_DimStack_VerticesHV OrdHorizontal, True, P1, dPts, P2, oset, bInv, fctr, bcws, False, 3.7
            '       .aDimensionStackOrdinate OrdVertical, p1, dPts, oset, shft, bInv, fctr
            '       .aDimensionStackOrdinate OrdHorizontal, p1, dPts, oset, shft, bInv, fctr

            '        _Image.Entities.GetByGraphicType(gdo_Dimension).Move 3, 3
            '        _Image.Display.ZoomExtents

            '_Image.NumberVectors dPts

            '============================================================================
Done:

        End With
    End Sub


    Private Sub Code_14()
        Dim iPts As colDXFVectors
        '**UNUSED VAR** Dim i As Long
        Dim P1 As dxfVector
        Dim P2 As dxfVector

        Dim aEl As dxeEllipse
        Dim aL As dxeLine
        Dim aB As dxeBezier
        Dim aDir As dxfDirection
        Dim ac As dxeArc

        P1 = New dxfVector



        iPts = New colDXFVectors
        iPts.Add(0, 0)
        iPts.Add(1.5, 1)
        iPts.Add(3, -1)
        iPts.Add(4.5, 0)

        With _Image.Draw

            _Image.Display.SetDisplayWindow(8, New dxfVector, 8)

            P1.SetCoordinates(1, 0.5)
            aEl = .aEllipse(P1, 6, 2, 25)
            aDir = New dxfDirection(1, 0.1, 0)

            P2 = aEl.AnglePoint(25)
            .aCircle(P2, 0.05, , dxxColors.Cyan)
            '.aText(P2, P2.CoordinatesR)

            aL = New dxeLine(P1, aDir, 4)
            _Image.Draw.aLine(aL.StartPt, aL.EndPt, , dxxColors.Blue)

            ac = .aCircle(aEl.Center, aEl.MinorRadius + 0.1, , dxxColors.Grey)


            aB = _Image.Draw.aBezier(iPts.Item(1), iPts.Item(2), iPts.Item(3), iPts.Item(4), aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Cyan))

            iPts = aL.Intersections(aEl, True, True)
            .aCircles(iPts, 0.05, , dxxColors.Green)

            _Image.Entities.Add(aEl.MajorAxis)


            ' .aText(aL.EndPt, aL.EndPoints.CoordinatesR(2, ":"))


            iPts = aL.Intersections(ac, True)
            .aCircles(iPts, 0.1, , dxxColors.Blue)

            iPts = aEl.Intersections(ac, True)
            .aCircles(iPts, 0.075, , dxxColors.LightCyan)


            iPts = ac.Intersections(aB, True)
            .aCircles(iPts, 0.04, , dxxColors.LightBlue)


            '
            '        .aCircle aL.MidPt, 0.1

            '        Set p1 = New dxfVector
            '        p1.Move 3, 3
            '        .aCircle p1, 0.03, , dxfBlue
            '        Set p2 = p1.Projected(.Utilities.CreateDirection(1, 0, 0), 3)
            '        .aCircle p2, 0.03, , dxfGreen
            '
            '        oimage.Draw.aText p2, p1.AngleTo(p2)
            '

            _Image.Display.Refresh()

            '============================================================================
Done:


        End With
    End Sub


    Private Sub Code_15()
        '**UNUSED VAR** Dim i As Long
        Dim P1 As dxfVector
        Dim P2 As dxfVector
        Dim iPts As colDXFVectors
        '**UNUSED VAR** Dim aDT As dxeText
        '**UNUSED VAR** Dim aMT As dxeText
        Dim aDims As colDXFEntities
        Dim aDim As dxeDimension


        Static Pts As String

        '**UNUSED VAR** Dim aLdr As dxeLeader
        Dim xscal As Double
        P1 = New dxfVector
        P2 = New dxfVector

        With _Image.Draw
            _Image.Header.UCSMode = dxxUCSIconModes.Origin

            '        .UCS.Spin 45

            '        Set p2 = p1.Projected(.Utilities.CreateDirection(1, 1), 0.75)
            '        .DimStyleOverrides.Save
            '        .DimStyleOverrides.ArrowHeadBlockLeader = "Origin2"
            '        .DimStyleOverrides.TextGap = -.DimStyleOverrides.TextGap
            '        .DimStyleOverrides.DimLineColor = dxfBlue
            '        .DimStyleOverrides.TextPositionV = Centered
            '        Set aLdr = .aLeader.Text(p1, p2, "TEST\PTWO LINES")

            '============================================================================
            '        .SetTextStyleProperty "Standard", "Width Factor", 0.4
            With _Image.DimSettings
                .DimLayer = "DIMS"
                .DimLayerColor = dxxColors.Grey

            End With

            With _Image.TextStyle
                '            .Backwards = True
                '            .UpsideDown = True
                '            .ObliqueAngle = 25
                .WidthFactor = 1
                .FontName = "RomanS"
                '            .Vertical = True
            End With
            With _Image.DimStyle
                .LinearPrecision = 2
                .SetColors(dxxColors.Grey, dxxColors.LightBlue)
                '            .TextGap = -0.09
            End With

            '        .DimStyleOverrides.TextGap = -0.09
            _Image.TextSettings.LayerColor = dxxColors.Cyan
            '        Set iPts = New colDXFVectors
            '        iPts.Add -2, 0
            'If CheckVal Then Pts = ""
            'If Pts = "" Then
            iPts = dxfUtils.RandomPoints(5, -0.75, 0.75, -0.75, 0.75)

            'Else
            'iPts = New colDXFVectors
            '    iPts.Coordinates = Pts
            'End If

            Pts = iPts.Coordinates_Get

            '            iPts.AddPair -1, 1, 0, 1, -1, 0, , " OD", "X"
            '            iPts.AddPair -3, 2, 0, 3, -2, 0, , , "Y"
            '            iPts.AddPair -4, 0.5, 0, 4, -0.5, 0, , , "Z"
            '            iPts.RotateAbout .UCS.ZAxis, 90

            '         For i = 1 To 3
            '
            '            ipts.AddRelative 2
            '        Next i

            .aCircles(iPts, 0.03, , , , dxxColors.Yellow)

            _Image.Display.ZoomExtents(2.5, True)
            xscal = _Image.Display.PaperScale

            '        .aDimensionStack LinearVertical, iPts, 0.2, 0.3, True, , , False
            '    iPts.Sort LeftToRight
            aDims = _Image.DimTool.Stack_Vertical(iPts, -0.2 * xscal, 0.125 * xscal, True, , , False)
            aDim = aDims.LastEntity
            '        MsgBox aDim.SubEntities.Identifiers
            '        .aPolyline aDim.TextBox, True, , , dxfCyan
            '
            '        .aPolyline aDim.DimEntities(True, True, False, True, True, False).BoundingRectangle, True

            _Image.Display.ZoomExtents()
            _Image.Screen.NumberVectors(iPts, 2)

            '        .aDimensionStackConcentric LinearVertical, ipts, 0.2, False, True, False, False
            '    .aDimension_V ipts.Item(ipts.Count - 1), ipts.Item(2), -0.5, 0.2

            '
            '        .Layers.Add "MIKE", dxfBlue

            '        p1.SetCoordinates 1, 1
            '        Set p2 = p1.Projected(.Utilities.CreateDirection(1, 2, 0), 2)
            '        Set aDT = .aText(p1, "LMOy", 1, dxfFit, , , , dxfByLayer, p2, 1, DText)
            '
            '
            '        aDT.Color = dxfLightYellow
            '        aDT.Project aDT.XDirection, 3 * aDT.Length

            '        cmdDisplay_Click 0

            '        Set aDT = .aText(Nothing, "KLbq", 1, dxfBottomCenter, , "MIKE", , dxfByLayer, p2, 1, DText)

            '        If Not aDT Is Nothing Then
            '        .aCircle aDT.AlignmentPoint1, 0.03, , dxfOrange
            '        .aCircle aDT.AlignmentPoint2, 0.02, , dxfBlue
            '        End If
            '
            '        .aCircle New dxfVector, 0.08, , dxfGreen
            ''        .aCircle p2, 0.08, , dxfBlue
            '        p2.SetCoordinates 1, 0
            '        .aLine p1, p2, , dxfLightCyan

            '============================================================================
Done:
            'cmdDisplay_Click 0
            _Image.Display.Refresh()
        End With
    End Sub

    Private Sub Code_16()
        '**UNUSED VAR** Dim i As Long
        Dim P1 As dxfVector
        Dim P2 As dxfVector
        Dim p3 As dxfVector
        '**UNUSED VAR** Dim arwSty As Integer
        Dim iPts As colDXFVectors
        '**UNUSED VAR** Dim aL As dxeLine
        '**UNUSED VAR** Dim aPl As dxePolyline
        '**UNUSED VAR** Dim aMT As dxeText
        '**UNUSED VAR** Dim aArw As dxeSymbol
        '**UNUSED VAR** Dim aArw1 As dxeInsert
        '**UNUSED VAR** Dim d1 As Double
        Dim aP As dxePolyline
        Dim aSym As dxeSymbol
        Dim lPts As colDXFVectors
        Dim aRec As dxfRectangle


        '**UNUSED VAR** Dim ac As dxeArc

        '**UNUSED VAR** Dim aAttribs As colDXFEntities
        '**UNUSED VAR** Dim aBl As dxfBlock
        '**UNUSED VAR** Dim aIns As dxeInsert
        '**UNUSED VAR** Dim aTxt As dxeText
        Dim T1 As String
        Dim t2 As String
        Dim t3 As String
        Dim t4 As String

        On Error GoTo Err

        P1 = New dxfVector
        P2 = New dxfVector
        p3 = New dxfVector

        With _Image

            .Display.SetDisplayWindow(8, New dxfVector, 8)

            '============================================================================

            lPts = New colDXFVectors
            iPts = New colDXFVectors
            iPts.Add(-3, 0)
            iPts.AddRelative(6)
            iPts.AddRelative(, 2)
            iPts.AddRelative(-1)
            iPts.AddRelative(, 1)
            iPts.AddRelative(-4)
            iPts.AddRelative(, -1)
            iPts.AddRelative(, -1)

            '        .InstancesAdd 20, -10
            '

            aP = .Draw.aPolyline(iPts, bClosed:=True)
            .Draw.aLine(iPts.Item(4), iPts.Item(7))

            aRec = aP.BoundingRectangle

            P1 = iPts.Item(4)
            P2 = P1.Projected(_Image.UCS.AngularDirection(45), 1)
            .Display.ZoomExtents(2)

            .Styles.Add("MIKE", "RomanD.shx", , 0.8)

            With .SymbolSettings
                .TextStyleName = "MIKE"
                .LineColor = dxxColors.ByLayer
                .TextColor = dxxColors.LightGrey
                '            .AlignText = False
                .TextGap = 0.035
                .BoxText = True
                .TextHeight = 0.09
                .ArrowHead = dxxArrowHeadTypes.Open30
                .AxisStyle = 1
                .ArrowSize = 0.15
                .ArrowStyle = dxxArrowStyles.AngledFullOpen
                .LayerColor = dxxColors.Blue
                .ArrowTextAlignment = dxxRectangularAlignments.TopLeft
                .LayerName = "SYMBOLS"
                .ArrowTails = dxxArrowTails.Open
            End With

            P1 = aP.Vertex(5, True)
            lPts.Add(P1)

            P1 = P1.Projected(New dxfDirection(1, 0.3), 1.2)
            lPts.Add(P1, , , True)

            P1 = P1.Projected(New dxfDirection(-1, 1), 1.2)
            lPts.Add(P1, , , True)

            P1 = P1.Projected(New dxfDirection(1, 1), 1.75)
            lPts.Add(P1, , , True)
            T1 = " Is REALLY(WAY TOOOO LONG  0.75\PLINe2\PTHREE"
            t2 = " x ( 0.1\PLine2)" '"0.25
            t3 = "TRAILS\PWITH SEVERAL" '\PLINES"
            '        Set aSym = .Draw.aSymbol_Weld(lPts, Fillet, , True, True, T1, T2, t3, False, 0, , , "Standard")
            '
            P1 = aRec.Center

            aSym = .Draw.aSymbol.ViewArrow(P1, 0.5 * aRec.Height, 0.5, "A1", 300)

            aSym = .Draw.aSymbol.Arrow(P1, 0, 0.5, "Trail", , , "Above", "Below", "Lead\pTwo\PLines")
            '
            '        Set P1 = aRec.BottomLeft.Moved(0.2, 0.2)
            '        Set aSym = .Draw.aSymbol_Axis(P1, 1, -30, , , 0.125, , , , StdBlocks)
            '
            '
            '        iPts.Clear
            '        iPts.Add aRec.TopCenter.Moved(0.35, 0.2)
            '        iPts.Add aRec.TopCenter.Moved(-0.35, 0.2)
            '        iPts.Add aRec.Center.Moved(-0.35)
            '        iPts.Add aRec.Center.Moved(0.35)
            '
            '        iPts.Add aRec.BottomCenter.Moved(0.35, -0.2)
            '        Set aSym = .Draw.aSymbol_SectionArrow(iPts, , "S1", 45, , MiddleRight)
            '


            Return
            '
            '        lPts.Clear
            '        Set p1 = aRec.TopCenter
            '        p1.Move , 0.5 * .PaperScale
            '         lPts.Add p1
            '
            '        Set p1 = aRec.Center
            '         lPts.Add p1, , , True
            '
            '        p1.Move 1
            '         lPts.Add p1, , , True
            '
            '        p1.Y = aRec.BottomCenter.Y - 0.5 * .PaperScale
            '        lPts.Add p1, , , True
            '        lPts.Move -0.5, 0.2
            '        lPts.Rotate aP.Plane.ZAxis, -10
            '
            '        Set aSym = .aSymbol_SectionArrow(lPts, 0.5, "SECTION\P1", -10, , , , , "Hidden")
            '
            '
            '        Set p1 = aP.Vertex(1, True)
            '        Set aSym = .aSymbol_DetailBubble(p1, 0.5, "DETAIL C", 0.5, 360 - 25, , , , , False, "Standard", Integral)
            '
            P1 = aP.Vertex(2, True)
            lPts.Clear()
            lPts.Add(P1)
            lPts.Add(P1.Projected(dxfUtils.CreateDirectionAngular(-60), 0.35))
            aSym = .Draw.aSymbol.Bubble(dxxBubbleTypes.Pill, lPts, "BUBBLEjy", , 0.85, 25, "A1", "Trailer Text\PWith Many Lines\PCan Be Tricky")
            '
            '

            '        Set aSym = .Draw.aSymbol_Axis(aP.BoundingRectangle.Center, 0.25, 90, "X", "Y", , , True, , , "Standard")

            P1 = aP.Vertex(6, True)
            T1 = "ARROW"
            t2 = "ABOVE TEXT\PREALLY REALLY TO LONG"
            t3 = "BELOW TEXT YUP"
            t4 = "LEAD"
            '        Set aSym = .EntityTool.CreateSymbol_ArrowPointer(p1, 45, 0.75, T1, , , T2, t3, t4, , Small, , 0.02, 1)
            aSym = .Draw.aSymbol.Arrow(P1, 45, 6, T1, 0.25, 0.15, t2, t3, t4, , , , 0.02, 2)

            '        aSym.Rescale 2
            '
            '        aBl.Name = "BILL"
            '        .Blocks.Add aBl
            '
            '        p1.SetCoordinates 0, 0
            '
            '        .Draw.aInsert aBl.Name, p1

            '        .Entities.Append aBl.Entities

            '        p1.Move -4
            '        Set aIns = .Draw.aInsert(aBl.Name, p1)
            '
            '        Set aTxt = aSym.Entities.Graphic(gdo_Text, 1)
            '        aTxt.UpdatePath True, _Image
            ''        .Entities.Append aTxt.TextBoxes(False)
            ''        aTxt.UpdatePath True, _Image
            '        .Entities.Add aTxt
            '        .Entities.Append aTxt.SubStrings(DText)
            '        .Entities.Add aSym.Entities.Graphic(gdo_Polyline, 1)
            '        .Entities.Append aTxt.TextBoxes(False, , , dxfCyan)

            Return

            '============================================================================
Done:

        End With

        Return
Err:
        _Image.HandleError("my Sub", Err.Source, Err.Description)
    End Sub


    Private Sub Code_17()

        Dim P1 As dxfVector

        Dim tbl As New List(Of String)
        Dim alnm As New List(Of String)
        Dim iRs As Integer
        Dim iCs As Integer

        Dim aStr As String
        Dim bStr As String
        Dim nStr As String
        Dim pStr As String
        Dim k As dxxRectangularAlignments

        Dim talm As dxxRectangularAlignments
        Dim cNames As New Collection
        Dim cPrompts As New Collection
        Dim txt As String
        Dim aChr As String
        Dim tlen As Integer
        Dim lns As Integer
        Dim n As Integer
        Dim m As Integer
        Dim P As Integer




        '         _Image.Display.SetDisplayWindow 14, New dxfVector, 14


        '============================================================================


        Try
            _Image.TextStyle().FontName = "Arial Narrow.ttf"

            With _Image.TableSettings
                .LayerName = "TABLES"
                .LayerColor = dxxColors.Blue
            End With


            _Image.Styles.Add("ROMAND", "RomanD.shx")

            iRs = 3
            iCs = 3
            aStr = " COL 1 | COL 2 | Col 3 "
            tbl.Add(aStr)
            For iR As Integer = 2 To iRs
                aStr = ""
                bStr = ""
                nStr = ""
                pStr = ""

                For iC As Integer = 1 To iCs

                    If iC = 1 Then
                        k = dxxRectangularAlignments.MiddleRight
                    ElseIf iC = 2 Then
                        k = dxxRectangularAlignments.MiddleLeft
                    Else
                        k = dxxRectangularAlignments.MiddleCenter
                    End If



                    If aStr <> "" Then aStr &= "|"
                    If bStr <> "" Then bStr &= "|"
                    If nStr <> "" Then nStr &= "|"
                    If pStr <> "" Then pStr &= "|"

                    txt = ""
                    lns = 1 ' dxfUtils.RandomInteger(1, 5)
                    For n = 1 To lns
                        If txt <> "" Then txt &= "\P"
                        tlen = dxfUtils.RandomInteger(3, 8)
                        For m = 1 To tlen
                            P = dxfUtils.RandomInteger(1, 3)
                            If P = 1 Then
                                aChr = Chr(dxfUtils.RandomInteger(65, 90))
                            ElseIf P = 2 Then
                                aChr = Chr(dxfUtils.RandomInteger(97, 122))
                            Else
                                aChr = Chr(dxfUtils.RandomInteger(48, 51))
                            End If
                            txt &= aChr
                        Next m
                    Next n

                    aStr &= txt

                    bStr &= k

                    nStr &= "Cell " & iC

                    pStr &= "Cell " & iC & " :"
                Next

                tbl.Add(aStr)
                'alnm.Add(bStr)
                cNames.Add(nStr)
                cPrompts.Add(pStr)
            Next
            '

            '        Set alnm = Nothing
            '        p1.SetComponents 20, 20

            With _Image.TableSettings
                .GridColor = dxxColors.Grey
                .ColumnGap = 0
                .RowGap = 0
                .TextGap = 0.2
                .FeatureScale = 1
                .HeaderRow = 1
                .SaveAsBlock = True
                .HeaderTextStyle = "RomanD"
            End With
            alnm.Add(dxxRectangularAlignments.MiddleRight.ToString & "|" & dxxRectangularAlignments.MiddleLeft & "|" & dxxRectangularAlignments.BottomRight)
            talm = dxxRectangularAlignments.TopLeft
            P1 = New dxfVector(0, 0)
            _Image.Draw.aTableBlk("TABLE1", P1, tbl, alnm, aTableAlign:=talm)

        Catch ex As Exception
            _Image.HandleError("my Sub", Err.Source, Err.Description)
        End Try



    End Sub

    Public Sub Code_18()

        Dim aRec As New dxfRectangle(101.625, 102.25)
        Dim aPl As dxePolyline = _Image.Draw.aPolyline(aRec, dxfDisplaySettings.Null(aColor:=dxxColors.Red))

        _Image.Header.UCSMode = dxxUCSIconModes.Origin
        _Image.UCS.MoveTo(aRec.BottomLeft)
        _Image.DimStyleOverrides.TextGap *= -1  'negative text gap turns on text boxes
        _Image.DimStyleOverrides.LinearPrecision = 1
        _Image.DimStyleOverrides.LinearFormatType = dxxLinearUnitFormats.Architectural
        Dim SF As Double = _Image.Display.ZoomExtents(1.25, True)
        Dim v0 As dxfVector = _Image.UCS.Origin
        Dim v1 As dxfVector = aPl.Vertex(1)
        Dim v2 As dxfVector = aPl.Vertex(2)
        Dim v3 As dxfVector = aPl.Vertex(3)

        Dim v4 As dxfVector = aPl.Vertex(4)
        _Image.Layer("Defpoints").Visible = True
        _Image.Styles.Add("RomanS", "RomanS.shx",, 0.6)

        _Image.DimTool.OrdinateH(Nothing, v1, 0.5)
        _Image.DimTool.OrdinateH(v0, v3, -0.5)
        _Image.DimStyleOverrides.LinearFormatType = dxxLinearUnitFormats.Engineering
        _Image.DimStyleOverrides.LinearPrecision = 5
        _Image.DimTool.OrdinateV(v0, v4, 0.5)

        _Image.DimStyleOverrides.LinearFormatType = dxxLinearUnitFormats.Decimals
        _Image.DimStyleOverrides.LinearPrecision = 3
        _Image.DimTool.OrdinateV(v0, v1, -0.5)

        _Image.DimStyleOverrides.TextGap *= -1
        _Image.DimStyleOverrides.LinearFormatType = dxxLinearUnitFormats.Scientific
        _Image.DimStyleOverrides.LinearPrecision = 3
        _Image.DimStyleOverrides.TextStyleName = "RomanS"

        _Image.DimTool.OrdinateV(v0, aPl.Center, -0.5)



    End Sub

    Private Sub Code_19()



        Dim DPI As Integer = DeviceDpi
        '_Image.Draw.aCircle(New dxfVector, 100 / dpi)

        Dim sz As Drawing.Size = Panel1.Size
        Dim rad As Double = sz.Width / 8 / DPI
        Dim v2 As New dxfVector(sz.Width / 2 / DPI, -sz.Height / 2 / DPI)
        Dim bLine As dxeLine
        Dim aPts As New colDXFVectors()
        Dim v1 As New dxfVector
        Dim ang As Double = 0

        _Image.DimStyle.TextColor = dxxColors.Cyan
        Dim learTxt As New List(Of String)({"sdsdffd", "kuoiyoy", "bcnvbmui", "sdfdfzxcz"})

        Dim shape As dxePolyline = _Image.Primatives.Polygon(New dxfVector, bReturnAsPolygon:=False, aSideCnt:=10, aRadius:=rad, aRotation:=ang)


        shape = _Image.Draw.aPolyline(shape.Vertices, bClosed:=True, aDisplaySettings:=New dxfDisplaySettings(aColor:=dxxColors.Red))

        For i As Integer = 1 To shape.Vertices.Count
            shape.Vertex(i).Tag = "VERT" & i

        Next

        _Image.Display.ZoomExtents(2.0, bSetFeatureScale:=True)

        _Image.Screen.NumberVectors(shape.Vertices)

        Dim paperScl As Double = _Image.Display.PaperScale

        _Image.DimSettings.LeaderLayer = "LEADERS"
        _Image.DimSettings.LeaderLayerColor = dxxColors.LightCyan

        _Image.DimStyle.SetColors(aTextColor:=dxxColors.Orange)
        v1 = _Image.UCS.PolarVector(2 * rad, 45)

        _Image.LeaderTool.Stack_Vertical(New colDXFVectors(shape.Vertices.GetVectors(aFilter:=dxxPointFilters.GreaterThanX)), aLeaderStrings:=Nothing, aFirstLeaderPointXY:=v1, aSpaceAdder:=0.125 * paperScl)

        '_Image.Draw.aRectangle(New dxfVector, 2 * rad, 2 * rad)



        Return

        Dim aLine As dxeLine = _Image.Draw.aLine(v1, v2, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Blue, aLinetype:="Hidden"))
        v2 = New dxfVector(-sz.Width / 2 / DPI, sz.Height / 2 / DPI)
        bLine = _Image.Draw.aLine(v1, v2, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.BlackWhite, aLinetype:="Hidden"))

        bLine.LTScale = 0.5
        bLine.UpdateImage(True)

        _Image.Draw.aEllipse(v1, 4 * rad, rad, 45, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Green))

        aPts.Add(New dxfVector(0, 0))
        aPts.Add(New dxfVector(-10 / DPI, -20 / DPI))
        aPts.Add(New dxfVector(10 / DPI, -20 / DPI))

        _Image.Draw.aSolid(aPts,, dxxColors.Orange)

        '_Image.Display.ZoomExtents()

        If _Image Is Nothing Then Return





    End Sub

    Private Sub Code_20()

        Dim vecs As colDXFVectors = dxfUtils.RandomPoints(10, -10 + 20, 10 + 20, -10 + 20, 10 + 20)
        vecs.Sort(dxxSortOrders.TopToBottom)
        Dim v1 As dxfVector = vecs.BoundingRectangle.MiddleLeft
        Dim v2 As dxfVector = v1.Moved(10)
        v2.RotateAbout(v1, 45)
        _Image.Draw.aCircle(v1, 0.375, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Blue))
        'Dim l1 As dxeLine = _Image.Draw.aLine(v1, v2, aColor:=dxxColors.Blue)
        'vecs.Sort(aOrder:=dxxSortOrders.LeftToRight, v1)
        'vecs.Clockwise(l1.StartPt, l1.AngleOfInclination, bReverseSort:=True)
        Dim cnt As Integer = vecs.Count
        'For i As Integer = 1 To cnt
        '    v2 = vecs.Item(i).Clone
        '    vecs.Add(v2.Moved(-2))
        '    vecs.Add(v2.Moved(2))
        '    vecs.Add(v2.Moved(0, 2))
        '    vecs.Add(v2.Moved(0, -2))

        'Next

        ' vecs.Sort(dxxSortOrders.NearestToFarthest, v1)
        Dim ldrtext As New List(Of String)
        Dim v3 As dxfVector

        For i As Integer = 1 To vecs.Count
            v3 = vecs.Item(i)
            _Image.Draw.aText(v3, i.ToString, aTextHeight:=0.75, aAlignment:=dxxMTextAlignments.MiddleCenter)
            ldrtext.Add($"LDRPT_{i}")
            v3.Tag = $"LDRPT_{i}"
        Next
        _Image.Display.ZoomExtents(2.5, bSetFeatureScale:=False)
        _Image.Display.SetFeatureScales(0.5 / _Image.Display.ZoomFactor)
        v1 = vecs.BoundingRectangle.Center
        _Image.Draw.aLeader.Stacks(vecs, v1, 2, 2)


    End Sub

    Private Sub Code_21()

        Dim vecs As colDXFVectors
        Dim cnt As Integer = 4
        If IsNothing(_WorkingVectors) Then _WorkingVectors = New colDXFVectors

        If _WorkingVectors.Count <> cnt Then
            _WorkingVectors = dxfUtils.RandomPoints(cnt, -10 + 10, 10 + 10, -10 + 10, 10 + 10)
        End If
        Dim circs As New colDXFEntities

        vecs = _WorkingVectors

        Dim v1 As dxfVector

        For i As Integer = 1 To vecs.Count
            v1 = vecs.Item(i)
            circs.Add(_Image.Draw.aCircle(v1, 0.25, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Cyan)))


        Next
        Dim bnd As dxfRectangle = circs.BoundingRectangle

        v1 = bnd.MiddleRight.Moved(10)
        _Image.Draw.aRectangle(bnd)
        _Image.Draw.aCircle(v1, 0.125, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Red))

        circs = New colDXFEntities(circs)
        Dim axis As New dxeLine(v1, v1.Moved(0, 0, 10))
        ' circs.RotateAbout(axis, 180)
        circs.SetDisplayVariable(dxxDisplayProperties.Color, dxxColors.Blue)
        _Image.Entities.Append(circs)

        v1 = New dxfVector(0, 0)
        Dim algn As dxxRectangularAlignments = dxxRectangularAlignments.BottomRight
        _Image.Screen.Entities.aScreenText("SCREEN", aAlignment:=algn)


    End Sub

    Private Sub Code_100()
        'Dim aShapes As New dxoShapes("C:\Junk\Fonts\txt.shp")
        Dim aRec As dxePolyline

        aRec = _Image.Draw.aRectangle(New dxfVector(0, 0), 100, 100)

        _Image.DimStyle.DimLineColor = dxxColors.Red

        'aRec.FilletAtVertex(2, 10)
        Dim v1 As New dxfVector("0,75")
        Dim v2 As dxfVector
        Dim vrts As New colDXFVectors ' = dxfUtilities.RandomPoints(2, -20, 20, -20, 20)
        Dim aArc As dxeArc

        vrts.Add(-5, -5, 0)
        vrts.Add(10, 10, 0)


        ''_Image.Screen.ExtentPts = True
        'aArc = _Image.Draw.aCircle(vrts, 1.5)
        _Image.Display.ZoomExtents(1.05, True)

        '_Image.Screen.NumberVectors(vrts, 2)
        'vrts = aArc.ExtentPoints
        '_Image.Screen.NumberVectors(vrts, 1.2, dxxColors.Grey)

        v1 = aRec.Vertices.Add(v1,, 2)
        v2 = aRec.Vertices.Item(3)
        v1 = aRec.Vertices.Item(3)

        'v1.Y = 25

        '_Image.Screen.ExtentPts = True

        _Image.Screen.NumberVectors(aRec.Vertices, 2)

        Dim l1 As dxeLine = _Image.Draw.aLine(New dxfVector(-75, -50), New dxfVector(75, 50),, dxxColors.Orange, "Phantom")

        Dim ipts As colDXFVectors = l1.Intersections(aRec)
        For i As Integer = 1 To ipts.Count
            aArc = _Image.Draw.aCircle(ipts.Item(i), 2, , dxxColors.Blue)
        Next

        Dim aArcs As List(Of dxeArc) = _Image.Draw.aCircles(ipts, 2, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Blue))
        aArc = aArcs.Item(0)
        ipts = aArc.Intersections(l1)
        _Image.Header.PointSize = -0.1
        _Image.Header.PointMode = dxxPointModes.CircCross

        _Image.Draw.aCircles(ipts, 0.5, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Green))

        _Image.Draw.aPoint(aArc.PathPoints)
        'v1.Y = 50

        Dim aSld As New dxeSolid(New dxfVector(0, 0, 0), New dxfVector(-20, 20, 0), New dxfVector(20, 20, 0))

        _Image.Entities.Add(aSld)
        _Image.DimTool.Horizontal(aRec.Vertex(1), aRec.Vertex(5), 0.5)
        _Image.DimTool.Aligned(aRec.Vertex(1), aRec.Vertex(5), 1.5)

        _Image.DimTool.Vertical(aRec.Vertex(2), aRec.Vertex(1), 0.5)
        _Image.DimTool.Aligned(aRec.Vertex(2), aRec.Vertex(1), 1.5)

        _Image.DimTool.Horizontal(aRec.Vertex(4), aRec.Vertex(2), 0.5)
        _Image.DimTool.Aligned(aRec.Vertex(4), aRec.Vertex(2), 1.5)

        _Image.DimTool.Aligned(aRec.Vertex(2), aRec.Vertex(3), 0.5)

        Dim al1 As dxeLine = Nothing
        Dim al2 As dxeLine = Nothing
        al1 = New dxeLine(aRec.Vertex(3), aRec.Vertex(2))
        al2 = New dxeLine(aRec.Vertex(3), aRec.Vertex(4))
        _Image.DimTool.Angular(al1, al2, 90)



        '_Image.Draw.aEllipse(New dxfVector, 100, 50,,,,,, dxxColors.Green)
        _Image.Display.ZoomExtents()
        'aShp = aShapes.Item("T")
        'Debug.Print(Asc("T") & " // " & aShp.ToString)

        'For i As Integer = 1 To aShapes.Count
        '    aShp = aShapes.Item(i)
        '    Debug.Print(aShp.ToString)
        '    If aShp.Name = "T" Then MsgBox(aShp.ToString)
        'Next



    End Sub

    Private Sub Code_101()
        'Dim aShapes As New dxoShapes("C:\Junk\Fonts\txt.shp")
        Dim aRec As dxePolyline

        aRec = _Image.Draw.aRectangle(New dxfVector(0, 0), 100, 100)

        _Image.DimStyle.DimLineColor = dxxColors.Red

        'aRec.FilletAtVertex(2, 10)
        Dim v1 As New dxfVector("0,75")
        Dim v2 As dxfVector
        Dim vrts As New colDXFVectors ' = dxfUtilities.RandomPoints(2, -20, 20, -20, 20)
        Dim aArc As dxeArc
        Dim acirc As dxeArc

        vrts.Add(-5, -5, 0)
        vrts.Add(10, 10, 0)


        ''_Image.Screen.ExtentPts = True
        acirc = _Image.Draw.aCircle(New dxfVector(0, 0, 0), 30)
        _Image.Display.ZoomExtents(1.05, True)

        '_Image.Screen.NumberVectors(vrts, 2)
        'vrts = aArc.ExtentPoints
        '_Image.Screen.NumberVectors(vrts, 1.2, dxxColors.Grey)

        v1 = aRec.Vertices.Add(v1,, 2)
        v2 = aRec.Vertices.Item(3)
        v1 = aRec.Vertices.Item(3)

        'v1.Y = 25

        '_Image.Screen.ExtentPts = True

        _Image.Screen.NumberVectors(aRec.Vertices, 2)

        Dim l1 As dxeLine = _Image.Draw.aLine(New dxfVector(-75, -50), New dxfVector(75, 50),, dxxColors.Orange, "Phantom")

        Dim ipts As colDXFVectors = l1.Intersections(aRec)
        For i As Integer = 1 To ipts.Count
            aArc = _Image.Draw.aCircle(ipts.Item(i), 2, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Blue))
        Next

        Dim arcs As List(Of dxeArc) = _Image.Draw.aCircles(ipts, 2, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Blue))
        aArc = arcs.Item(0)
        ipts = aArc.Intersections(l1)
        _Image.Header.PointSize = -0.1
        _Image.Header.PointMode = dxxPointModes.CircCross

        _Image.Draw.aCircles(ipts, 0.5, aDisplaySettings:=dxfDisplaySettings.Null(aColor:=dxxColors.Green))

        _Image.Draw.aPoint(aArc.PathPoints)
        'v1.Y = 50

        'Dim aSld As New dxeSolid(New dxfVector(0, 0, 0), New dxfVector(-20, 20, 0), New dxfVector(20, 20, 0))

        '_Image.Entities.Add(aSld)

        _Image.DimTool.OrdinateH(New dxfVector(0, 0, 0), aRec.Vertex(1), -0.5, -0.5, , , True)
        _Image.DimTool.OrdinateH(New dxfVector(0, 0, 0), aRec.Vertex(2), 0.5, 0.5, , , True)

        Dim od As dxeDimension
        _Image.DimTool.OrdinateV(New dxfVector(0, 0, 0), aRec.Vertex(1), -0.5, -0.5, , , True)
        od = _Image.DimTool.OrdinateV(New dxfVector(0, 0, 0), aRec.Vertex(2), 0.5, 0.5, , , True)

        '_Image.DimTool.RadialD(acirc, 0.0)
        '_Image.DimTool.RadialR(acirc, 45.0)


        '_Image.Draw.aEllipse(New dxfVector, 100, 50,,,,,, dxxColors.Green)
        _Image.Display.ZoomExtents()
        'aShp = aShapes.Item("T")
        'Debug.Print(Asc("T") & " // " & aShp.ToString)

        'For i As Integer = 1 To aShapes.Count
        '    aShp = aShapes.Item(i)
        '    Debug.Print(aShp.ToString)
        '    If aShp.Name = "T" Then MsgBox(aShp.ToString)
        'Next



        '        .aDimensionStackConcentric LinearVertical, ipts, 0.2, False, True, False, False
        '    .aDimension_V ipts.Item(ipts.Count - 1), ipts.Item(2), -0.5, 0.2

        '
        '        .Layers.Add "MIKE", dxfBlue

        '        p1.SetCoordinates 1, 1
        '        Set p2 = p1.Projected(.Utilities.CreateDirection(1, 2, 0), 2)
        '        Set aDT = .aText(p1, "LMOy", 1, dxfFit, , , , dxfByLayer, p2, 1, DText)
        '
        '
        '        aDT.Color = dxfLightYellow
        '        aDT.Project aDT.XDirection, 3 * aDT.Length

        '        cmdDisplay_Click 0

        '        Set aDT = .aText(Nothing, "KLbq", 1, dxfBottomCenter, , "MIKE", , dxfByLayer, p2, 1, DText)

        '        If Not aDT Is Nothing Then
        '        .aCircle aDT.AlignmentPoint1, 0.03, , dxfOrange
        '        .aCircle aDT.AlignmentPoint2, 0.02, , dxfBlue
        '        End If
        '
        '        .aCircle New dxfVector, 0.08, , dxfGreen
        ''        .aCircle p2, 0.08, , dxfBlue
        '        p2.SetCoordinates 1, 0
        '        .aLine p1, p2, , dxfLightCyan

        '============================================================================
Done:
        'cmdDisplay_Click 0
        _Image.Display.Refresh()
        ' End With
    End Sub

    Private Sub LoadSettings()
        Dim wuz As Boolean = _Loading
        _Loading = True
        _CheckBoxes = New System.Collections.Generic.Dictionary(Of String, CheckBox)
        _CheckBoxes.Add(chkBox_1.Tag, chkBox_1)
        _CheckBoxes.Add(chkBox_2.Tag, chkBox_2)
        _CheckBoxes.Add(chkBox_3.Tag, chkBox_3)
        _CheckBoxes.Add(chkBox_4.Tag, chkBox_4)
        _CheckBoxes.Add(chkBox_5.Tag, chkBox_5)
        _CheckBoxes.Add(chkBox_6.Tag, chkBox_6)
        _CheckBoxes.Add(chkBox_7.Tag, chkBox_7)
        _CheckBoxes.Add(chkBox_8.Tag, chkBox_8)
        _CheckBoxes.Add(chkBox_9.Tag, chkBox_9)

        CheckVal("EntityBounds") = My.Settings.EntityBounds
        CheckVal("ExtentPts") = My.Settings.ExtentPts
        CheckVal("TextBoxes") = My.Settings.TextBoxes
        CheckVal("EntityOCS") = My.Settings.EntityOCS
        CheckVal("SuppressScreen") = My.Settings.SuppressScreen
        CheckVal("ExtentRectangle") = My.Settings.ExtentRectangle
        CheckVal("DisplayLineweights") = My.Settings.DisplayLineweights
        CheckVal("AddCodeLabels") = My.Settings.AddCodeLabels
        CheckVal("LocalView") = My.Settings.LocalView
        _Loading = wuz
    End Sub

    Private Property CheckVal(aTag As String) As Boolean
        Get
            If IsNothing(_CheckBoxes) Then Return False
            Dim ckbox As CheckBox = Nothing
            If _CheckBoxes.TryGetValue(aTag, ckbox) Then Return ckbox.Checked
            Return False
        End Get
        Set(value As Boolean)
            If IsNothing(_CheckBoxes) Then Return
            Dim ckbox As CheckBox = Nothing
            If _CheckBoxes.TryGetValue(aTag, ckbox) Then ckbox.Checked = value

        End Set
    End Property

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LoadSettings()

        ToolTip1.SetToolTip(txtErrors, txtErrors.Text)
        ToolTip1.SetToolTip(txtSettings, txtSettings.Text)
        ToolTip1.SetToolTip(txtTables, txtTables.Text)
        ToolTip1.SetToolTip(txtEntities, txtEntities.Text)
        ToolTip1.SetToolTip(txtImage, txtImage.Text)
        ToolTip1.SetToolTip(txtBlocks, txtBlocks.Text)
        ToolTip1.SetToolTip(txtScreen, txtBlocks.Text)

        cboUCSMode.SelectedIndex = 2

        lblElapsed.Text = "0.00000"
        lblZoomFactor.Text = "1.0"

        pnlZoom.Visible = False
        lblCoords.Text = "0,0"
        txtFolder1.Text = Me.WorkingFolder

        RunCode(0)

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        RunCode(2)
    End Sub
    Private Sub ZoomBox(X As Integer, Y As Integer)
        If IsNothing(_Image) Then Return
        If _Image.Display.DeviceHwnd <> pnlOutput.Handle Then Return

        If Not _lasso Then
            _lasso = True
            _ZoomPt = New System.Drawing.Point(X, Y)

            pnlZoom.Width = 0
            pnlZoom.Top = Y
            pnlZoom.Left = X
            pnlZoom.Height = 0
            pnlZoom.Visible = True
        Else
            If pnlZoom.Visible Then
                pnlZoom.Width = Math.Abs(_ZoomPt.X - X)
                pnlZoom.Height = Math.Abs(_ZoomPt.Y - Y)
                If _ZoomPt.X > X Then
                    pnlZoom.Left = X
                Else
                    pnlZoom.Left = X - pnlZoom.Width
                End If

                If _ZoomPt.Y > Y Then
                    pnlZoom.Top = Y

                Else
                    pnlZoom.Top = Y - pnlZoom.Height

                End If
                pnlZoom.BorderStyle = BorderStyle.FixedSingle

            End If


        End If

    End Sub

    Private Sub Drag(X As Integer, Y As Integer)
        If IsNothing(_Image) Then Return
        If _Image.Display.DeviceHwnd <> pnlOutput.Handle Then Return

        If Not _Drag Then
            _Drag = True
            _Pan1 = New Point(X, Y)
            _Pan2 = New Point(X, Y)
            Panel1.Cursor = Cursors.Hand
            Me.Cursor = Cursors.Hand
        Else

            _Pan2 = New Point(X, Y)
            Me.Cursor = Cursors.Default



            _Drag = False

            Dim p2 As New dxfVector(X, Y)
            Dim p1 As New dxfVector(_ZoomPt.X, _ZoomPt.Y)

            Dim dX As Double = -Math.Round((_Pan2.X - _Pan1.X) / Panel1.Width, 1)

            Dim dy As Double = Math.Round((_Pan2.Y - _Pan1.Y) / Panel1.Height, 1)
            If dX <> 0 Or dy <> 0 Then
                Panel1.Enabled = False
                SetScreenProperties()
                _Image.Display.Pan(dX, dy)
                Panel1.Enabled = True
            End If





        End If


    End Sub

    Private ReadOnly Property MousePt() As dxfVector
        Get
            If _Image.Display.DeviceHwnd <> pnlOutput.Handle Then
                Return Nothing
            Else
                Dim p1 = New dxfVector(_MousePt.X, _MousePt.Y)
                Return _Image.Display.ConvertVector(p1, dxxDomains.Device, dxxDomains.World)
            End If
        End Get
    End Property


    Private Sub ZoomIt(X As Integer, Y As Integer)
        If _Image.Display.DeviceHwnd <> pnlOutput.Handle Then Return

        If _lasso Then
            SetScreenProperties()

            With _Image.Display
                Panel1.Enabled = False
                _lasso = False
                pnlZoom.Visible = False
                Dim p2 As New dxfVector(X, Y)
                Dim p1 As New dxfVector(_ZoomPt.X, _ZoomPt.Y)

                'p1 = _Image.Display.ConvertVector(p1, dxxDomains.Device, dxxDomains.World)
                'p2 = _Image.Display.ConvertVector(p2, dxxDomains.Device, dxxDomains.World)

                .ZoomBox(p1.X, p1.Y, p2.X, p2.Y)

                '.ZoomDiagonal = New dxeLine(p1, p2)

                Panel1.Enabled = True
            End With


        End If



    End Sub
    Private Sub zoomExt_Click(sender As Object, e As EventArgs) Handles zoomExt.Click
        If IsNothing(_Image) Then Return

        SetScreenProperties()
        _Image.Display.ZoomExtents(1.15)
        Panel1.Enabled = True
    End Sub

    Private Sub pnlOutput_MouseDown(sender As Object, e As MouseEventArgs) Handles pnlOutput.MouseDown
        If e.Button = MouseButtons.Right Then
            If Not _lasso Then ZoomBox(e.X, e.Y)

        ElseIf e.Button = MouseButtons.Left Then
            If Not _Drag Then Drag(e.X, e.Y)

        End If

    End Sub

    Private Sub pnlOutput_MouseMove(sender As Object, e As MouseEventArgs) Handles pnlOutput.MouseMove
        If _Image.Display.DeviceHwnd <> pnlOutput.Handle Then Return

        _MousePt.X = e.X
        _MousePt.Y = e.Y
        Dim vW As dxfVector = MousePt
        Dim vd As dxfVector
        Dim vv As dxfVector

        lblWorld.Text = vW.CoordinatesR(2, bSuppressZ:=True, bSuppressParens:=True)
        vv = _Image.Display.ConvertVector(vW, dxxDomains.World, dxxDomains.Viewport)
        vd = _Image.Display.ConvertVector(vv, dxxDomains.World, dxxDomains.Device)

        lblView.Text = vv.CoordinatesR(2, bSuppressZ:=True, bSuppressParens:=True)
        lblCoords.Text = vd.CoordinatesR(2, bSuppressZ:=True, bSuppressParens:=True)

        If _lasso Then ZoomBox(e.X, e.Y)
    End Sub

    Private Sub pnlOutput_MouseUp(sender As Object, e As MouseEventArgs) Handles pnlOutput.MouseUp
        If e.Button = MouseButtons.Right Then
            If _lasso Then ZoomIt(e.X, e.Y)
        ElseIf e.Button = MouseButtons.Left Then
            If _Drag Then Drag(e.X, e.Y)

        End If
    End Sub

    Private Sub frmImages_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        _Image.Dispose()

    End Sub



    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim aFrm As New frmBitmaps
        aFrm.Show(Me)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        RunCode(3)
    End Sub

    Private Sub zoomOut_Click(sender As Object, e As EventArgs) Handles zoomOut.Click
        _Image.Display.ZoomFactor = 1.2 * _Image.Display.ZoomFactor
        Panel1.Enabled = True
    End Sub

    Private Sub ZoomIn_Click(sender As Object, e As EventArgs) Handles ZoomIn.Click
        _Image.Display.ZoomFactor = 0.8 * _Image.Display.ZoomFactor
        Panel1.Enabled = True
    End Sub

    Private Sub displayRedraw_Click(sender As Object, e As EventArgs) Handles displayRedraw.Click
        SetScreenProperties()
        Static start_time As DateTime = Now
        Static stop_time As DateTime
        Dim elapsed_time As TimeSpan

        start_time = Now
        lblElapsed.Text = "0.000000"

        _Image.Display.Redraw()
        stop_time = Now
        elapsed_time = stop_time.Subtract(start_time)
        lblElapsed.Text = elapsed_time.TotalSeconds.ToString("0.000000")
        Panel1.Enabled = True


    End Sub

    Private Sub displayRegen_Click(sender As Object, e As EventArgs) Handles displayRegen.Click
        SetScreenProperties()
        Static start_time As DateTime = Now
        Static stop_time As DateTime
        Dim elapsed_time As TimeSpan

        start_time = Now
        lblElapsed.Text = "0.000000"

        _Image.Display.Regen()
        stop_time = Now
        elapsed_time = stop_time.Subtract(start_time)
        lblElapsed.Text = elapsed_time.TotalSeconds.ToString("0.000000")
        Panel1.Enabled = True

    End Sub

    Private Sub cmdBuffer_Click(sender As Object, e As EventArgs) Handles cmdBuffer.Click
        If Not IsNothing(_Image) Then
            dxfUtils.ShowImageProperties(_Image, Me)
        End If
    End Sub


    Private Sub btnTip_Forward_Click(sender As Object, e As EventArgs) Handles btnTip_Forward.Click
        If Not IsNothing(_Image) Then
            _Image.Display.Tip(udTip.Value, False, CheckVal("LocalView"))

        End If
    End Sub

    Private Sub btnTip_Back_Click(sender As Object, e As EventArgs) Handles btnTip_Back.Click
        If Not IsNothing(_Image) Then
            _Image.Display.Tip(-1 * udTip.Value, False, CheckVal("LocalView"))

        End If
    End Sub

    Private Sub btnSpin_Forward_Click(sender As Object, e As EventArgs) Handles btnSpin_Forward.Click
        If Not IsNothing(_Image) Then
            _Image.Display.Spin(udTip.Value, False, CheckVal("LocalView"))

        End If
    End Sub

    Private Sub btnSpin_Back_Click(sender As Object, e As EventArgs) Handles btnSpin_Back.Click
        If Not IsNothing(_Image) Then
            _Image.Display.Spin(-1 * udTip.Value, False, CheckVal("LocalView"))

        End If
    End Sub
    Private Sub btnRoll_Forward_Click(sender As Object, e As EventArgs) Handles btnRoll_Forward.Click
        If Not IsNothing(_Image) Then
            _Image.Display.Rotate(udTip.Value, False, CheckVal("LocalView"))

        End If
    End Sub

    Private Sub btnRoll_Back_Click(sender As Object, e As EventArgs) Handles btnRoll_Back.Click
        If Not IsNothing(_Image) Then
            _Image.Display.Rotate(-1 * udTip.Value, False, CheckVal("LocalView"))

        End If
    End Sub



    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        RunCode(4)
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        RunCode(5)
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        RunCode(6)
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        RunCode(7)
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        RunCode(8)
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        RunCode(9)
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        RunCode(10)
    End Sub
    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        RunCode(11)
    End Sub
    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        RunCode(12)
    End Sub

    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        RunCode(13)
    End Sub

    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
        RunCode(14)
    End Sub

    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click
        RunCode(15)
    End Sub

    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click
        RunCode(16)
    End Sub
    Private Sub Button19_Click(sender As Object, e As EventArgs) Handles Button19.Click
        RunCode(17)
    End Sub

    Private Sub Button20_Click(sender As Object, e As EventArgs) Handles Button20.Click
        RunCode(18)
    End Sub

    Private Sub Button201_Click(sender As Object, e As EventArgs) Handles Button201.Click
        RunCode(100)
    End Sub

    Private Sub Button202_Click(sender As Object, e As EventArgs) Handles Button202.Click
        RunCode(101)
    End Sub

    Private Sub _Image_ScreenRender(aBitmap As dxfBitmap) Handles _Image.ScreenRender
        UpdateScreenBitmap(aBitmap)

        Dim sb As New System.Text.StringBuilder(txtScreen.Text)


        Dim txt As String = "_Image.ScreenRenderEvent:" & "Bitmap=" & aBitmap.Width & " x " & aBitmap.Height
        'sb.AppendLine(txt)
        'tabsInfo.SelectedIndex = 3
        'txtScreen.Text = sb.ToString()
        'txtScreen.Refresh()

        StatusText(2) = txt

    End Sub



    Private Sub Button22_Click(sender As Object, e As EventArgs) Handles Button22.Click

        RunCode(19)




    End Sub


    Private Sub txtFolder1_DoubleClick(sender As Object, e As EventArgs) Handles txtFolder1.DoubleClick



        FolderBrowserDialog1.SelectedPath = WorkingFolder
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            WorkingFolder = FolderBrowserDialog1.SelectedPath
            txtFolder1.Text = WorkingFolder
        End If


    End Sub


    Private Sub txtFolder1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtFolder1.KeyPress
        e.KeyChar = ""
    End Sub

    Private Sub _Image_RenderEvent(e As dxfImageRenderEventArg) Handles _Image.RenderEvent

        Try

            Dim sb As New System.Text.StringBuilder(txtRender.Text)

            Dim txt As String = e.ToString
            sb.AppendLine(txt)

            txtRender.Text = sb.ToString()
            StatusText(1) = "_Image.RenderEvent:" & "Begin=" & e.Begin.ToString
            Static start_time As DateTime = Now
            Static stop_time As DateTime
            Dim elapsed_time As TimeSpan

            If e.Begin Then
                Panel1.Enabled = False
                start_time = Now
                lblElapsed.Text = "0.000000"
            Else
                If Not _RunningCode Then
                    Panel1.Enabled = True
                    stop_time = Now
                    elapsed_time = stop_time.Subtract(start_time)
                    lblElapsed.Text = elapsed_time.TotalSeconds.ToString("0.000000")
                    lblZoomFactor.Text = Format(_Image.Display.ZoomFactor, "0.00")
                    'Dim aBMP As dxfBitmap = _Image.Screen.Bitmap()

                    'If aBMP IsNot Nothing Then
                    '    PictureBox1.Image = aBMP.Image

                    'End If
                End If

            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Sub Button21_Click(sender As Object, e As EventArgs) Handles Button21.Click
        If _Filename = "" Then Return
        If txtFolder1.Text = "" Or Not System.IO.Directory.Exists(txtFolder1.Text) Then
            txtFolder1_DoubleClick(sender, e)
            Return
        End If

        Dim fname As String = Trim(txtFolder1.Text) & "\" & _Filename & ".dwg"
        If MsgBox("Save File '" & fname & "' ?", MsgBoxStyle.YesNo + MsgBoxStyle.DefaultButton1 + MsgBoxStyle.Question, "Save Drawing To File?") = MsgBoxResult.No Then
            Return
        End If

        Dim msg As String = ""
        UserControl12.viewer.SaveDwg(fname, msg)

        _Image.SaveToFile(aSuppressUI:=True, aFileName:=fname, aFileType:=dxxFileTypes.DXF)
    End Sub

    Private Sub cboUCSMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboUCSMode.SelectedIndexChanged
        If Not IsNothing(_Image) Then
            SetScreenProperties()

            _Image.Display.Redraw()

        End If
    End Sub



    Private Sub btnBrowseFolder_Click(sender As Object, e As EventArgs) Handles btnBrowseFolder.Click
        txtFolder1_DoubleClick(sender, e)
    End Sub

    Private Sub frmImages_DoubleClick(sender As Object, e As EventArgs) Handles Me.DoubleClick
        Panel1.Enabled = True
    End Sub


    Private Sub PictureBox2_DoubleClick(sender As Object, e As EventArgs) Handles PictureBox2.DoubleClick
        Dim bCanc As Boolean
        Dim aclr As dxfColor = dxfUtils.SelectColor(Me, dxxColors.Undefined, True, False, bCanc, PictureBox2.BackColor)
        If Not bCanc Then
            PictureBox2.BackColor = aclr.ToWin64
            _Image.Display.BackgroundColor = PictureBox2.BackColor
            If Not _Image.Display.AutoRedraw Then _Image.Display.Redraw()
        End If
    End Sub

    Private Sub btnResetView_Click(sender As Object, e As EventArgs) Handles btnResetView.Click
        _Image.Display.Reset(False, True)

    End Sub


    Private Sub _Image_ViewRegenerate(aImage As dxfImage) Handles _Image.ViewRegenerate

    End Sub

    Private Sub _Image_ErrorRecieved(aFunction As String, aErrSource As String, aErrDescription As String, ByRef arIgnore As Boolean) Handles _Image.ErrorRecieved

        Dim sb As New System.Text.StringBuilder(txtErrors.Text)


        Dim txt As String = "[" & aErrSource & "]"
        txt &= "[" & aFunction & "] "

        txt &= aErrDescription

        sb.AppendLine(txt)
        tabsInfo.SelectedIndex = 0
        txtErrors.Text = sb.ToString()
        txtErrors.Refresh()

    End Sub

    Private Sub _Image_SettingChange(aSource As String, aProperty As dxoProperty) Handles _Image.SettingChange

        Dim sb As New System.Text.StringBuilder(txtSettings.Text)
        Dim aOldValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=True)
        Dim aNewValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=False)
        Dim txt As String = "Setting " & aSource & "." & aProperty.Name & " Changed From '" & aOldValue & "' To '" & aNewValue & "'"
        sb.AppendLine(txt)
        Me.txtSettings.Text = sb.ToString()
        tabsInfo.SelectedIndex = 1
        txtSettings.Refresh()
    End Sub

    Private Sub _Image_PropertyChange(aSource As String, aProperty As dxoProperty) Handles _Image.PropertyChange

        Dim sb As New System.Text.StringBuilder(txtImage.Text)
        Dim aOldValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=True)
        Dim aNewValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=False)
        Dim txt As String = "Property " & aSource & "." & aProperty.Name & " Changed From '" & aOldValue & "' To '" & aNewValue & "'"
        sb.AppendLine(txt)
        Me.txtImage.Text = sb.ToString()
        'tabsInfo.SelectedIndex = 4
        'txtImage.Refresh()
    End Sub
    Private Sub _Image_TableEvent(TableName As String, EventType As dxxCollectionEventTypes, EventDescription As String) Handles _Image.TableEvent

        Dim sb As New System.Text.StringBuilder(txtTables.Text)

        Dim txt As String = "Table Event:[" & TableName & "] [" & Replace(EventType.ToString, "dxfCollectionEvent_", "",,, vbTextCompare) & "] " & EventDescription
        sb.AppendLine(txt)

        txtTables.Text = sb.ToString()
    End Sub

    Private Sub _Image_TableMemberEvent(TableName As String, MemberName As String, aProperty As dxoProperty) Handles _Image.TableMemberEvent
        Dim sb As New System.Text.StringBuilder(txtTables.Text)
        Dim aOldValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=True)
        Dim aNewValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=False)
        Dim txt As String = "Table Member Event:[" & TableName & "." & MemberName & "].'" & aProperty.Name & "' Changed from '" & aOldValue & "' To '" & aNewValue & "'"
        sb.AppendLine(txt)
        txtTables.Text = sb.ToString()
        tabsInfo.SelectedIndex = 4
        txtTables.Refresh()
    End Sub

    Private Sub cmdLayers_Click(sender As Object, e As EventArgs) Handles cmdLayers.Click
        If IsNothing(_Image) Then Return
        'MsgBox(_Image.ShowSelectForm(Me, dxxReferenceTypes.LAYER, _Image.Layers.Layer(_Image.Layers.Count).Name, False))
        _Image.ShowFormatForm(Me, dxxReferenceTypes.LAYER)

        'Dim enumnmames As Dictionary(Of String, String) = dxfEnums.GetEnumNames(GetType(dxxHeaderVars), True, "$")
        'Dim aStr As String
        'For Each aStr In enumnmames.Values
        '    Debug.Print(aStr & "=" & _Image.Header.GetPropertyValue(aStr).ToString)
        'Next
    End Sub

    Private Sub CheckedChanged(sender As Object, e As EventArgs) Handles chkBox_1.CheckedChanged, chkBox_2.CheckedChanged, chkBox_3.CheckedChanged,
    chkBox_4.CheckedChanged, chkBox_5.CheckedChanged, chkBox_6.CheckedChanged, chkBox_7.CheckedChanged, chkBox_8.CheckedChanged, chkBox_9.CheckedChanged
        If _Loading Then Return
        My.Settings.EntityBounds = CheckVal("EntityBounds")
        My.Settings.ExtentPts = CheckVal("ExtentPts")
        My.Settings.TextBoxes = CheckVal("TextBoxes")
        My.Settings.EntityOCS = CheckVal("EntityOCS")
        My.Settings.SuppressScreen = CheckVal("SuppressScreen")
        My.Settings.ExtentRectangle = CheckVal("ExtentRectangle")
        My.Settings.DisplayLineweights = CheckVal("DisplayLineweights")
        My.Settings.AddCodeLabels = CheckVal("AddCodeLabels")
        My.Settings.LocalView = CheckVal("LocalView")
    End Sub

    Private Sub _Image_EntitiesEvent(Added As Boolean, aEntity As dxfEntity) Handles _Image.EntitiesEvent
        Dim sb As New System.Text.StringBuilder(txtEntities.Text)
        Dim txt As String
        If Added Then
            txt = "Entity " & aEntity.ToString & " Was Added to the entities collection"
        Else
            txt = "Entity " & aEntity.ToString & " Was Removed from the entities collection"
        End If

        sb.AppendLine(txt)
        Me.txtEntities.Text = sb.ToString()
        tabsInfo.SelectedIndex = 5
        txtEntities.Refresh()

    End Sub

    Private Sub txtEntities_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtEntities.KeyPress, txtImage.KeyPress, txtSettings.KeyPress, txtRender.KeyPress, txtTables.KeyPress, txtBlocks.KeyPress, txtScreen.KeyPress
        e.KeyChar = ""

    End Sub

    Private Sub _Image_BlocksEvent(Added As Boolean, aBlock As dxfBlock) Handles _Image.BlocksEvent
        Dim sb As New System.Text.StringBuilder(Me.txtBlocks.Text)
        Dim txt As String
        If Added Then
            txt = "Block " & aBlock.ToString & " Was Added to the blocks collection"
        Else
            txt = "Block " & aBlock.ToString & " Was Removed from the blocks collection"
        End If

        sb.AppendLine(txt)
        Me.txtBlocks.Text = sb.ToString()
        tabsInfo.SelectedIndex = 7
        txtEntities.Refresh()

    End Sub

    Private Sub _Image_ScreenDrawingEvent(aEventType As dxxScreenEventTypes, e As dxfImageScreenEventArg) Handles _Image.ScreenDrawingEvent
        UpdateScreenBitmap(e.Bitmap)
        Dim sb As New System.Text.StringBuilder(txtScreen.Text)


        Dim txt As String = "_Image.ScreenDrawingEvent:" & "[" & dxfEnums.Description(aEventType) & "{ "
        If Not IsNothing(e.ScreenEntity) Then
            'the screen enttiy caries all th format info of the drawin screen enity

            txt &= "SCREEN ENT [" & e.ScreenEntity.EntityTypeName & "]"
            If e.ScreenEntity.ScreenFraction <> 0 Then
                txt &= " [" & Format(e.ScreenEntity.ScreenFraction, "0.00#") & "%]"
            End If

        End If

        If Not IsNothing(e.ImageEntity) Then
            txt &= "Image Entity [" & e.ImageEntity.ToString & "]"


        End If
        txt &= "}"


        sb.AppendLine(txt)
        tabsInfo.SelectedIndex = 3
        txtScreen.Text = sb.ToString()
        txtScreen.Refresh()

        StatusText(2) = txt
    End Sub

    Private Sub cmdSetLimits_Click(sender As Object, e As EventArgs) Handles cmdSetLimits.Click
        If IsNothing(_Image) Then Return
        If MsgBox("Define Limits Based On Current View ?", MsgBoxStyle.YesNo, "Set Limits") = MsgBoxResult.Yes Then
            _Image.Display.SetLimits(_Image.Display.ViewRectangle)
        End If
    End Sub

    Private Sub displayZoomLimits_Click(sender As Object, e As EventArgs) Handles displayZoomLimits.Click
        If IsNothing(_Image) Then Return
        If Not _Image.Display.HasLimits Then
            MsgBox("Display Limits Are Currently Undefined!")
            Return

        End If
        _Image.Display.ZoomToLimits()
        Panel1.Enabled = True
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        RunCode(20)
    End Sub

    Private Sub Button23_Click(sender As Object, e As EventArgs) Handles Button23.Click
        RunCode(21)
    End Sub
End Class