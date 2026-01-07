Imports Vanara.PInvoke
Imports System.IO
Imports System.Reflection

Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports System.Windows.Input
Imports System.Windows.Controls

Namespace UOP.DXFGraphics.Fonts
    Public Class dxfFonts
#Region "Members"
        Private Shared _Bitmap As dxfBitmap
#End Region 'Members
#Region "Structures"
        Friend Structure TFONTINFO
            Dim Family As String
            Dim Style As String
            Dim Weight As SharpDX.DirectWrite.FontWeight
            Dim Metrics As SharpDX.DirectWrite.FontMetrics
            Dim IsSymbol As Boolean
            Public Sub New(aFamily As String, aStyle As String, aWeight As SharpDX.DirectWrite.FontWeight, aMetrix As SharpDX.DirectWrite.FontMetrics, bIsSymbol As Boolean)
                Family = aFamily
                Style = aStyle
                Weight = aWeight
                Metrics = aMetrix
                IsSymbol = bIsSymbol
            End Sub

            Public ReadOnly Property TTFStyle As dxxTextStyleFontSettings
                Get
                    Dim sty As String = Style.ToUpper()
                    If String.IsNullOrWhiteSpace(sty) Then Return dxxTextStyleFontSettings.Undefined

                    If Weight = SharpDX.DirectWrite.FontWeight.Bold Then
                        If sty = "NORMAL" Or sty = "REGULAR" Then Return dxxTextStyleFontSettings.Bold
                        If sty = "ITALIC" Then Return dxxTextStyleFontSettings.BoldItalic
                    ElseIf Weight = SharpDX.DirectWrite.FontWeight.Normal Or Weight = SharpDX.DirectWrite.FontWeight.Regular Then
                        If sty = "NORMAL" Or sty = "REGULAR" Then
                            Return dxxTextStyleFontSettings.Regular
                        ElseIf sty = "ITALIC" Then
                            Return dxxTextStyleFontSettings.Italic
                        End If
                    End If

                    Return dxxTextStyleFontSettings.Undefined
                End Get

            End Property


            Public Overrides Function ToString() As String
                Return $"{Family} - {Style} - {Weight}"
            End Function
        End Structure

        Friend Structure TFONTSTYLE
            Implements ICloneable
#Region "Members"
            Public Characters As TCHARARRAY
            Public NullChar As TCHAR
            Public FontIndex As Integer
            Public IsShape As Boolean
            Public StyleIndex As Integer
            Public TypeFaceName As String
            Public Ascent As Integer
            Public Descent As Integer
            Public CellHeight As Integer
            Public TTFStyle As dxxTextStyleFontSettings
            Private _FontName As String
            Private _FileName As String
            Private _XCorrection As Double
            Private _YCorrection As Double
            Private _FamilyName As String
            Private _DescentFactor As Double
#End Region 'Members
#Region "Constructors"
            Public Sub New(aFamilyName As String, aFontStyle As FontStyle)
                'init -------------------------------------------------
                Characters = New TCHARARRAY(0)
                NullChar = New TCHAR("")
                FontIndex = 0
                StyleIndex = 0
                IsShape = False
                TypeFaceName = ""
                Ascent = 0
                Descent = 0
                CellHeight = 0
                TTFStyle = dxxTextStyleFontSettings.Regular
                _FamilyName = IIf(Not String.IsNullOrWhiteSpace(aFamilyName), aFamilyName.Trim(), "")
                _FontName = ""
                _FileName = ""
                _XCorrection = 0
                _YCorrection = 1
                _DescentFactor = 0
                'init -------------------------------------------------

                Dim sname As String = aFontStyle.ToString().ToUpper()

                If aFontStyle = FontStyle.Regular Then
                    TTFStyle = dxxTextStyleFontSettings.Regular
                ElseIf aFontStyle = FontStyle.Bold Then
                    TTFStyle = dxxTextStyleFontSettings.Bold
                ElseIf aFontStyle = FontStyle.Italic Then
                    TTFStyle = dxxTextStyleFontSettings.Italic
                ElseIf aFontStyle = FontStyle.Bold Or FontStyle.Italic Then
                    TTFStyle = dxxTextStyleFontSettings.BoldItalic
                Else
                    TTFStyle = dxxTextStyleFontSettings.Regular
                End If


                If Not String.IsNullOrWhiteSpace(aFamilyName) Then FileName = $"{aFamilyName}.ttf"
                'If aFontData isNot Nothing Then
                '    'LogicalFont = aFontData.LogicalFont

                '    TTFStyle = aFontData.TTFStyle
                '    'Metrics = aFontData.TextMetrics

                'End If
            End Sub
            Public Sub New(aFamilyName As String, Optional aCharCount As Integer = 0)
                'init -------------------------------------------------
                Characters = New TCHARARRAY(aCharCount)
                NullChar = New TCHAR("")
                FontIndex = 0
                StyleIndex = 0
                IsShape = False
                TypeFaceName = ""
                Ascent = 0
                Descent = 0
                CellHeight = 0
                TTFStyle = dxxTextStyleFontSettings.Regular
                _FamilyName = IIf(Not String.IsNullOrWhiteSpace(aFamilyName), aFamilyName.Trim(), "")
                _FontName = ""
                _FileName = ""
                _XCorrection = 0
                _YCorrection = 1
                _DescentFactor = 0
                'init -------------------------------------------------



            End Sub
            Public Sub New(aFamilyName As String, aStyle As dxxTextStyleFontSettings, bIsShape As Boolean)

                'init -------------------------------------------------
                Characters = New TCHARARRAY(0)
                NullChar = New TCHAR("")
                FontIndex = 0
                StyleIndex = 0
                IsShape = bIsShape
                TypeFaceName = ""
                Ascent = 0
                Descent = 0
                CellHeight = 0
                TTFStyle = aStyle
                _FamilyName = IIf(Not String.IsNullOrWhiteSpace(aFamilyName), aFamilyName.Trim(), "")
                _FontName = ""
                _FileName = ""
                _XCorrection = 0
                _YCorrection = 1
                _DescentFactor = 0
                'init -------------------------------------------------


            End Sub

            Public Sub New(aFontStyle As TFONTSTYLE)
                'init -------------------------------------------------
                Characters = New TCHARARRAY(aFontStyle.Characters)
                NullChar = New TCHAR(aFontStyle.NullChar)
                FontIndex = aFontStyle.FontIndex
                StyleIndex = aFontStyle.StyleIndex
                IsShape = aFontStyle.IsShape
                TypeFaceName = aFontStyle.TypeFaceName
                Ascent = aFontStyle.Ascent
                Descent = aFontStyle.Descent
                CellHeight = aFontStyle.CellHeight
                TTFStyle = aFontStyle.TTFStyle
                _FamilyName = aFontStyle._FamilyName
                _FontName = aFontStyle._FontName
                _FileName = aFontStyle._FileName
                _XCorrection = aFontStyle._XCorrection
                _YCorrection = aFontStyle._YCorrection
                _DescentFactor = aFontStyle._DescentFactor
                'init -------------------------------------------------
            End Sub

#End Region 'Constructors
#Region "Properties"

            Public ReadOnly Property StyleName As String
                Get
                    Return dxfEnums.Description(TTFStyle)
                End Get
            End Property

            Public Property FileName As String
                Get
                    Return _FileName
                End Get
                Set(value As String)
                    If Not String.IsNullOrWhiteSpace(value) Then

                        _FileName = value.Trim
                        If _FileName.ToUpper().EndsWith(".SHP") Then
                            _FileName = _FileName.Substring(0, _FileName.Length - 4)
                            _FileName += ".shx"
                        End If
                        _FileName = Path.GetFileName(_FileName)
                        If Path.GetExtension(_FileName).ToUpper() = ".TTF" Then
                            IsShape = False
                        ElseIf Path.GetExtension(_FileName).ToUpper() = ".SHX" Then
                            IsShape = True
                        End If
                    Else
                        _FileName = ""
                    End If
                End Set
            End Property

            Public Property DescentFactor As Double
                Get
                    Return _DescentFactor
                End Get
                Set(value As Double)
                    _DescentFactor = value
                End Set
            End Property

            Public Property FontName As String
                Get
                    Return _FontName
                End Get
                Set(value As String)
                    If Not value Is Nothing Then _FontName = value.Trim Else _FontName = ""
                End Set
            End Property

            'Public WriteOnly Property Metrics As Gdi32.NEWTEXTMETRICEX
            '    Set(value As Gdi32.NEWTEXTMETRICEX)
            '        Ascent = value.ntmTm.tmAscent
            '        Descent = value.ntmTm.tmDescent
            '        CellHeight = value.ntmTm.ntmCellHeight

            '        If Ascent <> 0 And Ascent <> 0 And DescentFactor <= 0 Then
            '            DescentFactor = Ascent / Descent

            '            'DescentFactor = 1 / 3
            '        End If

            '    End Set
            'End Property

            Public Property FamilyName As String
                Get
                    Return _FamilyName
                End Get
                Set(value As String)
                    If Not String.IsNullOrWhiteSpace(value) Then
                        _FamilyName = value.Trim()
                    Else
                        _FamilyName = ""
                    End If
                End Set
            End Property

            Public Property XCorrection As Double
                Get
                    If _XCorrection <= 0 Then _XCorrection = 0
                    Return _XCorrection
                End Get
                Set(value As Double)
                    _XCorrection = value
                End Set
            End Property

            Public Property YCorrection As Double
                Get
                    If _YCorrection <= 0 Then _YCorrection = 1
                    Return _YCorrection
                End Get
                Set(value As Double)
                    _YCorrection = value
                End Set
            End Property
            Public ReadOnly Property FontStyle As System.Drawing.FontStyle
                Get
                    If TTFStyle = dxxTextStyleFontSettings.Bold Or String.Compare(StyleName, "Bold", True) = 0 Then
                        Return FontStyle.Bold
                    ElseIf TTFStyle = dxxTextStyleFontSettings.Italic Or String.Compare(StyleName, "Italic", True) = 0 Then
                        Return FontStyle.Italic
                    ElseIf TTFStyle = dxxTextStyleFontSettings.BoldItalic Or String.Compare(StyleName, "Bold Italic", True) = 0 Then
                        Return FontStyle.Bold + FontStyle.Italic
                    Else
                        Return FontStyle.Regular
                    End If
                End Get
            End Property
            Public ReadOnly Property NullCharacter As TCHAR
                '^creates the world vectors of the null char
                Get
                    Dim _rVal As TCHAR = NullChar
                    If Not _rVal.PathDefined Then
                        Dim aFont As TFONT = dxoFonts.Member(FontIndex)
                        If aFont.Index > 0 Then
                            _rVal = aFont.CreateNullChar(StyleIndex, Me, True, _YCorrection)
                        End If
                    End If
                    NullChar = _rVal
                    Return _rVal
                End Get
            End Property
#End Region 'Properties
#Region "Methods"
            Public Function Clone() As TFONTSTYLE
                Return New TFONTSTYLE(Me)
            End Function
            Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
                Return New TFONTSTYLE(Me)
            End Function

            Public Overrides Function ToString() As String
                Dim _rVal As String = FontName
                If Not String.IsNullOrWhiteSpace(StyleName) Then
                    If Not String.IsNullOrWhiteSpace(_rVal) Then _rVal += $" - { StyleName}" Else _rVal = StyleName
                End If
                _rVal = $"TFONT [{ _rVal }]"
                Return _rVal
            End Function
            Public Function PrintToDevice(aPixelSize As Integer, aBitmap As dxfBitmap, aCharString As String, Optional aXOffset As Integer = 0, Optional aYOffset As Integer = 0, Optional aXScale As Double = 1, Optional aYScale As Double = -1) As TPOINTS
                Dim rLimits As TLIMITS = Nothing
                Return PrintToDevice(aPixelSize, aBitmap, aCharString, aXOffset, aYOffset, aXScale, aYScale, rLimits)
            End Function
            Public Function PrintToDevice(aPixelSize As Integer, aBitmap As dxfBitmap, aCharString As String, aXOffset As Integer, aYOffset As Integer, aXScale As Double, aYScale As Double, ByRef rLimits As TLIMITS) As TPOINTS
                '^prints the passed string to the device context bassed of the passed font style
                If aBitmap Is Nothing Then
                    'create a 1000 x 1000 bitmap
                    aBitmap = New dxfBitmap(2 * dxfGlobals.CharPatternHt, 2 * dxfGlobals.CharPatternHt)
                End If
                Dim rPath As TPOINTS = Nothing
                Dim g As Graphics = Graphics.FromImage(aBitmap.Bitmap)
                Dim aHndl As Gdi32.SafeHFONT



                If aPixelSize <= 0 Then
                    aPixelSize = dxfGlobals.CharPatternHt
                End If

                Dim aFnt As New Font(FamilyName, aPixelSize, FontStyle, GraphicsUnit.Pixel)
                Dim lfnt As New LOGFONT()
                'create the logical font
                aFnt.ToLogFont(lfnt, g)
                Dim aDC As IntPtr = g.GetHdc
                'set the map mode so the coordinate axis is proper Y up
                APIWrapper.SetMapMode(aDC, Gdi32.MapMode.MM_TEXT)
                'set to baseline alignment
                APIWrapper.SetTextAlign(aDC, dxfGlobals.TA_BASELINE Or dxfGlobals.TA_NOUPDATECP)
                APIWrapper.SetTextColor(aDC, 0) 'vbBlack)
                APIWrapper.MoveToEx(aDC, aXOffset, aXOffset)

                aHndl = APIWrapper.CreateFontIndirect(lfnt)
                If aHndl <> 0 Then
                    'select the font to the device and get the handle of the current font
                    Dim bHndl As Vanara.PInvoke.HGDIOBJ = APIWrapper.SelectObject(aDC, aHndl)
                    'print it to device
                    Dim flag As Boolean = APIWrapper.BeginPath(aDC)
                    Dim stat As Long = APIWrapper.TextOut(aDC, 0, 0, aCharString, 1)
                    flag = APIWrapper.EndPath(aDC)
                    rPath = APIWrapper.GetPath(aDC, aXOffset, aYOffset, aXScale, aYScale, rLimits)
                    'set the old font back to the device
                    APIWrapper.SelectObject(aDC, bHndl)
                    'release the created font handle
                    aHndl.Dispose()
                End If
                g.Dispose()
                g = Nothing
                Return rPath
            End Function
            Public Function CreateChar_TTF(ByRef aPattern As TCHAR, aBitmap As dxfBitmap) As TCHAR

                '^creates the world vectors of the requested character of the passed font 500 pixel high character
                aPattern.IsShape = False
                Dim aAsciiIndex As Integer = aPattern.AsciiIndex
                Dim PointCoords(0) As System.Drawing.Point
                Dim PTypes(0) As Gdi32.VertexType
                Dim nullChr As TCHAR = NullCharacter
                Dim aCharStr As String = Chr(aAsciiIndex)
                Dim patHt As Integer = dxfGlobals.CharPatternHt

                If aAsciiIndex < 0 Or aAsciiIndex > 255 Then Return nullChr 'ASCII chars only!!
                aPattern.Charr = aCharStr
                aPattern.AsciiIndex = aAsciiIndex

                If aBitmap Is Nothing Then
                    'create a bitmap
                    aBitmap = New dxfBitmap(2 * dxfGlobals.CharPatternHt, 2 * dxfGlobals.CharPatternHt)
                End If
                Dim aPl As TPLANE = TPLANE.World
                Dim pLims As New TLIMITS(False)
                'select the font to the bitmap and print it baseline left on the device
                Dim pPth As TPOINTS = PrintToDevice(patHt * YCorrection, aBitmap, aCharStr, 0, 0, 1, -1, rLimits:=pLims)

                'the first four are the text rectangle corners
                Dim recPts As TPOINTS = pPth.First(4, bRemove:=True)
                Dim tLims As TLIMITS = recPts.Limits
                Dim charLims As TLIMITS = pPth.Limits



                If charLims.Top > tLims.Top Then
                    Dim dY As Double = charLims.Top - tLims.Top

                    charLims.Translate(0, dY)
                    pPth.Translate(0, dY)
                End If
                tLims.Top = patHt
                If charLims.Right > tLims.Right Then tLims.Right = charLims.Right

                If charLims.Width = 0 And pPth.Count <= 0 Then
                    'a space or character with no visbile paths
                    charLims.Update(tLims.Left + 0.125 * tLims.Width, tLims.Right - 0.125 * tLims.Width, 0, tLims.Top)
                    pPth.AddLine(New TPOINT(0, 0), New TPOINT(0, tLims.Right), bJustMoveTo:=True)

                End If

                aPattern.CharBox.Define(TVECTOR.Zero, TVECTOR.WorldX, TVECTOR.WorldY, patHt, Math.Abs(tLims.Bottom), aWidth:=tLims.Right)
                aPattern.ExtentPts = charLims.CornerPts()
                aPattern.PathDefined = True
                aPattern.Shape.Path = pPth
                aPattern.CharHeight = patHt

                Dim _rVal As New TCHAR(aPattern) With
                {
                    .StyleIndex = StyleIndex,
                    .FontIndex = FontIndex,
                    .CharHeight = patHt,
                    .Ascent = patHt
                    }



                Return _rVal
            End Function
            Public Function CreateChar_SHP(ByRef aPattern As TCHAR, ByRef ioShapes As TSHAPES) As TCHAR

                '^computes the un-transform path of the passed shape based character
                Dim pExents As TPOINTS = New TPOINTS(0)

                Dim _rVal As TCHAR = NullCharacter
                Dim aLims As TLIMITS = Nothing
                Dim sScale As Double = 1
                Try

                    _rVal.PathDefined = True
                    If aPattern.Shape.ByteCount <= 0 Then
                        aPattern.Shape = ioShapes.Member(aPattern.Charr)
                    End If
                    If aPattern.Shape.ByteCount <= 0 Then
                        Return _rVal
                    End If

                    If ioShapes.Ascent <> 0 Then
                        If ioShapes.Ascent <> dxfGlobals.CharPatternHt Then
                            sScale = dxfGlobals.CharPatternHt / ioShapes.Ascent
                        End If
                    End If
                    aPattern.ExtentPts = New TPOINTS(0)
                    aPattern.CharBox.Define(TVECTOR.Zero, New TVECTOR(1, 0, 0), New TVECTOR(0, 1, 0), ioShapes.Ascent, ioShapes.Descent, 0)
                    'compute the path of the character based on it's shape structure
                    Dim segs As TSEGMENTS = TSHAPES.ComputePath(ioShapes, aPattern.Shape, pExents, aLims, sScale)
                    'return the pattern defined
                    _rVal = New TCHAR(aPattern) With
                    {
                    .IsShape = True,
                    .FontIndex = FontIndex,
                    .StyleIndex = StyleIndex,
                    .Ascent = ioShapes.Ascent * sScale,
                    .Descent = ioShapes.Descent * sScale,
                    .Width = aLims.Right,
                    .PathDefined = True
                    }
                    Dim aRec As TPLANE

                    If segs.Count > 0 Then
                        aRec = segs.Bounds(TPLANE.World, True)
                        _rVal.ExtentPts.Append(aRec.Corners)
                    Else
                        Dim dx As Double = 0.125 * _rVal.Width
                        _rVal.ExtentPts.Add(dx, 0, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        _rVal.ExtentPts.Add(_rVal.Width - dx, 0, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        _rVal.ExtentPts.Add(_rVal.Width - dx, _rVal.Ascent, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        _rVal.ExtentPts.Add(dx, _rVal.Ascent, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    End If


                    Return _rVal
                Catch ex As Exception
                    _rVal.PathDefined = False
                    Return _rVal
                End Try
            End Function
            Friend Function GetCharacter(aCharCode As Integer, ByRef rFound As Boolean) As TCHAR

                'get the character
                Dim rChar As TCHAR = NullCharacter
                rChar.Index = -1
                rFound = Characters.TryGet(aCharCode, rChar)
                If rFound Then
                    If rChar.AsciiIndex < 0 Or rChar.AsciiIndex > 255 Then
                        rChar = NullCharacter
                    Else
                        rChar.Charr = Chr(rChar.AsciiIndex)
                    End If

                End If
                rChar.IsShape = IsShape

                Return rChar
            End Function
#End Region 'Methods
#Region "Shared Members"
            Public Shared Function GetTTFStyle(aBold As Boolean, aItalic As Boolean) As dxxTextStyleFontSettings
                If aItalic And aBold Then
                    Return dxxTextStyleFontSettings.BoldItalic
                ElseIf aItalic Then
                    Return dxxTextStyleFontSettings.Italic
                ElseIf aBold Then
                    Return dxxTextStyleFontSettings.Bold
                Else
                    Return dxxTextStyleFontSettings.Regular
                End If
            End Function
#End Region 'Shared Members
        End Structure 'TFONTSTYLE
        Friend Structure TFONTSTYLES
            Implements ICloneable
#Region "Members"
            Private _FamilyName As String
            Private _Init As Boolean
            Private _Members() As TFONTSTYLE
            Private _FontIndex As Integer
            Private _FontName As String
#End Region 'Members
#Region "Constructors"
            Public Sub New(aFamilyName As String)
                'init -----------------------------------------
                _Init = True
                ReDim _Members(-1)
                _FamilyName = IIf(Not String.IsNullOrWhiteSpace(aFamilyName), aFamilyName.Trim(), "")
                _FontIndex = 0
                _FontName = ""
                'init -----------------------------------------
            End Sub

            Public Sub New(aStyles As TFONTSTYLES)
                'init -----------------------------------------
                _Init = True
                ReDim _Members(-1)
                _FamilyName = aStyles._FamilyName
                _FontIndex = aStyles._FontIndex
                _FontName = aStyles._FontName
                'init -----------------------------------------
                If aStyles._Members IsNot Nothing Then _Members = aStyles._Members.Clone()

            End Sub

#End Region 'Constructors
#Region "Properties"
            Public Property FontIndex As Integer
                Get
                    Return _FontIndex
                End Get
                Set(value As Integer)
                    _FontIndex = value
                End Set
            End Property
            Public Property FontName As String
                Get
                    Return _FontName
                End Get
                Set(value As String)
                    If Not String.IsNullOrWhiteSpace(value) Then _FontName = value.Trim Else _FontName = ""
                End Set
            End Property
            Public Property FamilyName As String
                Get
                    Return _FamilyName
                End Get
                Set(value As String)
                    If Not String.IsNullOrWhiteSpace(value) Then _FamilyName = value.Trim Else _FamilyName = ""
                End Set
            End Property
            Public ReadOnly Property Count As Integer
                Get
                    If Not _Init Then Clear()
                    Return _Members.Count
                End Get
            End Property
#End Region 'Properties
#Region "Methods"
            Public Function Item(aIndex As Integer) As TFONTSTYLE
                If aIndex = 1 And Count <= 0 Then
                    ReDim _Members(0)
                    _Members(0) = New TFONTSTYLE
                End If
                If aIndex < 1 Or aIndex > Count Then Return New TFONTSTYLE("", 0)
                _Members(aIndex - 1).StyleIndex = aIndex
                Return _Members(aIndex - 1)
            End Function
            Public Sub SetItem(aIndex As Integer, value As TFONTSTYLE)

                If aIndex < 1 Or aIndex > Count Then Return
                _Members(aIndex - 1) = value
                _Members(aIndex - 1).FontIndex = FontIndex
                _Members(aIndex - 1).FontName = FontName
                _Members(aIndex - 1).StyleIndex = aIndex
            End Sub
            Public Function Item(aName As String) As TFONTSTYLE
                For i = 1 To Count
                    If String.Compare(_Members(i - 1).StyleName, aName, ignoreCase:=True) = 0 Then
                        _Members(i - 1).StyleIndex = i
                        _Members(i - 1).FontIndex = FontIndex
                        _Members(i - 1).FontName = FontName
                        Return _Members(i - 1)
                        Exit For
                    End If
                Next
                Return Nothing
            End Function
            Public Sub SetItem(aName As String, value As TFONTSTYLE)
                For i = 1 To Count
                    If String.Compare(_Members(i - 1).StyleName, value.StyleName, ignoreCase:=True) = 0 Then
                        _Members(i - 1) = value
                        _Members(i - 1).StyleIndex = i
                        Exit For
                    End If
                Next
            End Sub
            Public Sub Add(aStyle As TFONTSTYLE)
                If String.IsNullOrWhiteSpace(aStyle.StyleName) Then Return
                System.Array.Resize(_Members, Count + 1)
                _Members(_Members.Count - 1) = aStyle  '.Clone ' Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TFONTSTYLE)(aStyle)
                _Members(_Members.Count - 1).StyleIndex = _Members.Count
                _Members(_Members.Count - 1).FamilyName = FamilyName
                _Members(_Members.Count - 1).FontName = FontName
                _Members(_Members.Count - 1).FontIndex = FontIndex
            End Sub
            Public Sub Clear()
                _Init = True
                ReDim _Members(-1)
            End Sub
            Public Function Clone() As TFONTSTYLES

                Return New TFONTSTYLES(Me)
            End Function
            Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
                Return New TFONTSTYLES(Me)
            End Function
            Public Overrides Function ToString() As String
                Return $"TFONTSTYLES [{ Count }]"
            End Function
#End Region 'Methods
        End Structure 'TFONTSTYLES
        Friend Structure TFONTSTYLEINFO
            Implements ICloneable
#Region "Members"
            Friend NullChar As TCHAR
            Public FaceName As String
            Public FileName As String
            Public FontIndex As Integer
            Public FontName As String
            Public IsShape As Boolean
            Public NotFound As Boolean

            Public StyleIndex As Integer
            Public StyleName As String
            Public TTFStyle As dxxTextStyleFontSettings
            Public TypeFaceName As String
            'Private _TypoFont As TypographicFont
#End Region 'Members
#Region "Constructors"
            Public Sub New(aFont As TFONT, aFontStyle As TFONTSTYLE)
                'init -------------------------------------------------
                NullChar = aFontStyle.NullChar
                FaceName = aFont.FaceName
                FileName = aFontStyle.FileName
                FontIndex = aFont.Index
                FontName = aFont.Name
                IsShape = aFont.IsShape
                NotFound = FontIndex < 0
                StyleIndex = 0
                StyleName = aFontStyle.StyleName
                TTFStyle = aFontStyle.TTFStyle
                TypeFaceName = aFontStyle.TypeFaceName
                'init -------------------------------------------------

                If IsShape Then
                    StyleIndex = 1
                    StyleName = dxfEnums.Description(dxxTextStyleFontSettings.Regular)
                Else
                    StyleIndex = aFontStyle.StyleIndex
                    StyleName = aFontStyle.StyleName
                End If
            End Sub
            Public Sub New(aFontStyleInfo As TFONTSTYLEINFO)
                'init -------------------------------------------------
                NullChar = aFontStyleInfo.NullChar
                FaceName = aFontStyleInfo.FaceName
                FileName = aFontStyleInfo.FileName
                FontIndex = aFontStyleInfo.FontIndex
                FontName = aFontStyleInfo.FontName
                IsShape = aFontStyleInfo.IsShape
                IsShape = aFontStyleInfo.IsShape
                NotFound = aFontStyleInfo.NotFound
                StyleIndex = aFontStyleInfo.StyleIndex
                StyleName = aFontStyleInfo.StyleName
                TTFStyle = aFontStyleInfo.TTFStyle
                TypeFaceName = aFontStyleInfo.TypeFaceName
                'init -------------------------------------------------
            End Sub

#End Region 'Constructors
#Region "Methods"

            Public Function Clone() As TFONTSTYLEINFO
                Return New TFONTSTYLEINFO(Me)
            End Function
            Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
                Return New TFONTSTYLEINFO(Me)
            End Function
            Friend Function GetNullChar(aCharHeight As Double, aCharacterBox As dxoCharBox, aFormats As dxoCharFormat) As TCHAR

                If Not NullChar.PathDefined Then
                    'get the style
                    NullChar = dxoFonts.GetNullCharacter(FontIndex, StyleIndex)
                End If

                Dim _rVal As New TCHAR(NullChar) With {.FontStyleInfo = Me}
                If aCharacterBox IsNot Nothing Then
                    _rVal.CharBox.CopyDirections(aCharacterBox, bMoveTo:=True)
                End If
                If aCharHeight > 0 And _rVal.CharHeight <> 0 Then
                    Dim chrScale As Double = aCharHeight / _rVal.CharHeight
                    _rVal.Scale(chrScale, chrScale)

                End If
                _rVal.Formats = New TCHARFORMAT(aFormats)
                _rVal.FormatCode = dxxCharFormatCodes.Base
                Return _rVal
            End Function
            Public Overrides Function ToString() As String
                Dim _rVal As String = "TFONTSTYLEINFO ["
                If Not String.IsNullOrEmpty(FontName) Then
                    If Not String.IsNullOrEmpty(StyleName) Then _rVal += $"{FontName } - { StyleName}"
                End If
                _rVal += "]"
                Return _rVal
            End Function
            Public Function DXFProperties(aStyleName As String) As TPROPERTIES
                Dim _rVal As New TPROPERTIES(aStyleName)
                '^the SHX or TTF file name associated to the font
                '~based on the current Font Style
                'On Error Resume Next
                If IsShape Then
                    _rVal.Add(New TPROPERTY(3, FaceName, "Font Family Name", dxxPropertyTypes.dxf_String))
                    _rVal.Add(New TPROPERTY(4, "", "Big Font Name", dxxPropertyTypes.dxf_String))
                Else
                    If FileName = "" Then
                        _rVal.Add(New TPROPERTY(3, FaceName & ".ttf", "TTF Font File Name", dxxPropertyTypes.dxf_String))
                    Else
                        _rVal.Add(New TPROPERTY(3, FileName, "TTF Font File Name", dxxPropertyTypes.dxf_String))
                    End If
                    _rVal.Add(New TPROPERTY(4, "", "Big Font Name", dxxPropertyTypes.dxf_String))
                    _rVal.Add(New TPROPERTY(1001, "ACAD", "Extended Data", dxxPropertyTypes.Undefined))
                    _rVal.Add(New TPROPERTY(1000, TypeFaceName, "Font Type Face Name", dxxPropertyTypes.dxf_String))
                    If StyleName.IndexOf("BOLD", StringComparison.OrdinalIgnoreCase) + 1 > 0 And StyleName.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) + 1 > 0 Then
                        _rVal.Add(New TPROPERTY(1071, dxxTextStyleFontSettings.BoldItalic, "Font Style Code", dxxPropertyTypes.dxf_Integer))
                    Else
                        If StyleName.IndexOf("BOLD", StringComparison.OrdinalIgnoreCase) + 1 > 0 Then
                            _rVal.Add(New TPROPERTY(1071, dxxTextStyleFontSettings.Bold, "Font Style Code", dxxPropertyTypes.dxf_Integer))
                        ElseIf StyleName.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) + 1 > 0 Then
                            _rVal.Add(New TPROPERTY(1071, dxxTextStyleFontSettings.Italic, "Font Style Code", dxxPropertyTypes.dxf_Integer))
                        Else
                            _rVal.Add(New TPROPERTY(1071, dxxTextStyleFontSettings.Regular, "Font Style Code", dxxPropertyTypes.dxf_Integer))
                        End If
                    End If
                    'props_Add _rVal, 1071, LogicalFont.elfLogFont.lfPitchAndFamily, "Font Style Code")
                End If
                Return _rVal
            End Function
#End Region 'Methods
        End Structure 'TFONTSTYLEINFO
        Friend Structure TFONT
            Implements ICloneable
#Region "Members"
            Friend _Shapes As TSHAPES
            Private _Styles As TFONTSTYLES
            Private _Key As String
            Private _FontType As dxxFontTypes
            Private _Name As String
            Private _FamilyName As String
            Private _Index As Integer
#End Region 'Members
#Region "Constructors"
            Public Sub New(Optional aFamilyName As String = "", Optional aName As String = "", Optional aFontType As dxxFontTypes = dxxFontTypes.Undefined, Optional aFamily As System.Drawing.FontFamily = Nothing)
                'init -------------------------------------------------
                _Name = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), "")
                _FamilyName = IIf(Not String.IsNullOrWhiteSpace(aFamilyName), aFamilyName.Trim(), "")
                _Shapes = TSHAPES.Null
                _Styles = New TFONTSTYLES(_FamilyName)
                _Key = ""
                _FontType = aFontType
                _Index = -1
                'init -------------------------------------------------
                If IsShape Then

                    _Styles.Add(New TFONTSTYLE(FamilyName, dxxTextStyleFontSettings.Regular, True))
                Else


                    If aFamily Is Nothing Then Return

                    If aFamily.IsStyleAvailable(FontStyle.Regular) Then
                        AddStyle(New TFONTSTYLE(aFamilyName, FontStyle.Regular))
                    End If

                    If aFamily.IsStyleAvailable(FontStyle.Bold) Then
                        AddStyle(New TFONTSTYLE(aFamilyName, FontStyle.Bold))
                    End If
                    If aFamily.IsStyleAvailable(FontStyle.Italic) Then
                        AddStyle(New TFONTSTYLE(aFamilyName, FontStyle.Italic))
                    End If
                    If aFamily.IsStyleAvailable(FontStyle.Bold Or FontStyle.Italic) Then
                        AddStyle(New TFONTSTYLE(aFamilyName, FontStyle.Bold Or FontStyle.Italic))

                    End If

                End If

            End Sub

            Public Sub New(aFont As TFONT)
                'init -------------------------------------------------
                _Name = aFont._Name
                _FamilyName = aFont._FamilyName
                _Shapes = New TSHAPES(aFont._Shapes)
                _Styles = New TFONTSTYLES(aFont._Styles)
                _Key = aFont._Key
                _FontType = aFont._FontType
                _Index = aFont._Index
                'init -------------------------------------------------
            End Sub

            Public Sub New(aFont As dxoFont)
                'init -------------------------------------------------
                _Name = ""
                _FamilyName = ""
                _Shapes = New TSHAPES("")
                _Styles = New TFONTSTYLES("")
                _Key = ""
                _FontType = dxxFontTypes.Undefined
                _Index = -1
                'init -------------------------------------------------
                If aFont Is Nothing Then Return
                Dim t As TFONT = aFont.FontStructure
                'init -------------------------------------------------
                _Name = t._Name
                _FamilyName = t._FamilyName
                _Shapes = New TSHAPES(t._Shapes)
                _Styles = New TFONTSTYLES(t._Styles)
                _Key = t._Key
                _FontType = t._FontType
                _Index = t._Index
                'init -------------------------------------------------


            End Sub

#End Region 'Constructors
#Region "Properties"
            Public Property StyleArray As TFONTSTYLES
                Get
                    _Styles.FontIndex = Index
                    _Styles.FontName = Name
                    Return _Styles
                End Get
                Set(value As TFONTSTYLES)
                    _Styles = value
                    _Styles.FontIndex = Index
                    _Styles.FontName = Name
                End Set
            End Property
            Public Property FontType As dxxFontTypes
                Get
                    Return _FontType
                End Get
                Friend Set(value As dxxFontTypes)
                    _FontType = value
                End Set
            End Property
            Public Property Key As String
                Get
                    Return _Key
                End Get
                Friend Set(value As String)
                    _Key = value
                End Set
            End Property
            Friend Property Shapes As TSHAPES
                Get
                    Return _Shapes
                End Get
                Set(value As TSHAPES)
                    _Shapes = value
                End Set
            End Property
            Public Property Name As String
                Get
                    Return _Name
                End Get
                Friend Set(value As String)
                    _Name = value
                    _Styles.FontName = Name
                End Set
            End Property
            Public Property FamilyName As String
                Get
                    Return _FamilyName
                End Get
                Friend Set(value As String)
                    _FamilyName = value
                End Set
            End Property
            Public Property Index As Integer
                Get
                    Return _Index
                End Get
                Friend Set(value As Integer)
                    _Index = value
                    _Styles.FontIndex = _Index
                End Set
            End Property
            Public ReadOnly Property FaceName As String
                '^the short name of the Font (less the file extension)
                Get
                    Dim _rVal As String = FamilyName
                    If String.IsNullOrWhiteSpace(_rVal) Then Return String.Empty
                    _rVal = Name

                    If _rVal.Contains(".") Then
                        _rVal = dxfUtils.LeftOf(_rVal, ".", bFromEnd:=True)
                    End If
                    _FamilyName = _rVal

                    Return _rVal
                End Get
            End Property
            Public ReadOnly Property Bitmap As dxfBitmap
                Get
                    If _Bitmap Is Nothing Then
                        'create a 500 x 500 bitmap
                        _Bitmap = New dxfBitmap(2 * dxfGlobals.CharPatternHt, 2 * dxfGlobals.CharPatternHt)
                    End If
                    Return _Bitmap
                End Get
            End Property
            Public ReadOnly Property IsShape As Boolean
                Get
                    Return FontType = dxxFontTypes.Shape Or FontType = dxxFontTypes.Embedded
                End Get
            End Property
            Public ReadOnly Property Embedded As Boolean
                Get
                    Return FontType = dxxFontTypes.Embedded
                End Get
            End Property
#End Region 'Properties
#Region "Methods"
            Public Function AddStyle(aStyle As TFONTSTYLE) As TFONTSTYLE
                _Styles.Add(aStyle)
                Return StyleArray.Item(StyleArray.Count)
            End Function
            Public Function UpdateStyle(aIndex As Integer, aStyle As TFONTSTYLE) As Boolean
                If aIndex < 1 Or aIndex > StyleArray.Count Then Return False
                _Styles.SetItem(aIndex, aStyle)
                Return True
            End Function
            Public Sub Clear()
                _Styles.Clear()
            End Sub
            Public Function Clone() As TFONT
                Return New TFONT(Me)

            End Function
            Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
                Return New TFONT(Me)
            End Function
            Public Function ConfirmStyle(aStyleName As String, ByRef rTTFStyle As dxxTextStyleFontSettings) As Integer
                Dim _rVal As Integer
                Dim aStyle As TFONTSTYLE
                Dim i As Integer
                _rVal = -1
                rTTFStyle = dxxTextStyleFontSettings.Regular
                If IsShape Then
                    _rVal = 1
                    aStyleName = StyleArray.Item(1).StyleName
                Else
                    If aStyleName <> "" Then
                        For i = 1 To _Styles.Count
                            aStyle = _Styles.Item(i)
                            If String.Compare(aStyle.StyleName, aStyleName, True) = 0 Then
                                _rVal = i
                                aStyleName = aStyle.StyleName
                                rTTFStyle = aStyle.TTFStyle
                                Exit For
                            End If
                        Next i
                    Else
                        For i = 1 To _Styles.Count
                            aStyle = _Styles.Item(i)
                            If aStyle.TTFStyle = rTTFStyle Then
                                _rVal = i
                                aStyleName = aStyle.StyleName
                                Exit For
                            End If
                        Next i
                    End If
                End If
                If _rVal = -1 Then _rVal = 1
                aStyle = StyleArray.Item(_rVal)
                aStyleName = aStyle.StyleName
                rTTFStyle = aStyle.TTFStyle
                aStyle.NullChar.FontIndex = Index
                aStyle.NullChar.StyleIndex = _rVal
                aStyle.NullChar.IsShape = IsShape
                _Styles.SetItem(_rVal, aStyle)
                Return _rVal
            End Function
            Friend Function CreateNullChar(aStyleIndex As Integer, Optional aStyle As TFONTSTYLE = Nothing, Optional bUpdateArray As Boolean = False) As TCHAR
                Dim rYCorrections As Double = 1
                Return CreateNullChar(aStyleIndex, aStyle, bUpdateArray, rYCorrections)
            End Function
            Friend Function CreateNullChar(aStyleIndex As Integer, aStyle As TFONTSTYLE, bUpdateArray As Boolean, ByRef rYCorrections As Double) As TCHAR
                '^creates the world vectors of the capital X of the  font 1000 pixel high character
                rYCorrections = 1
                Dim _rVal As New TCHAR(-1, 0)

                Dim myStyle As Boolean = aStyle.FontIndex = Index

                Dim patHt As Integer = dxfGlobals.CharPatternHt


                If aStyle.IsShape Then
                    Dim rCF As Double = 0
                    If Shapes.Ascent > 0 Then rCF = patHt / Shapes.Ascent
                    aStyle.YCorrection = rCF
                    Dim fMat As TCHARFORMAT = _rVal.Formats
                    fMat.FontIndex = Index
                    If myStyle Then fMat.FontIndex = Index
                    fMat.StyleIndex = 1
                    fMat.IsShape = True
                    fMat.CharHeight = Shapes.Ascent * rCF
                    fMat.Tracking = 1
                    _rVal.Formats = fMat
                    Dim trk As Double = 0.125 * Shapes.Ascent
                    Dim wd As Double = (Shapes.Ascent * rCF) - 2 * trk


                    _rVal.CharBox = New TCHARBOX(TVECTOR.Zero, wd + 2 * trk, Shapes.Ascent * rCF, Shapes.Descent * rCF)
                    _rVal.PathDefined = True
                    _rVal.Shape.Path.Append(_rVal.CharBox.Corners(True))
                Else

                    aStyle.NullChar = New TCHAR("")
                    Dim aPl As TPLANE = TPLANE.World
                    aStyle.YCorrection = 1
                    aStyle.XCorrection = 0
                    'print the X to the device
                    Dim pPth As TPOINTS = aStyle.PrintToDevice(patHt, Bitmap, "X")
                    'Get the path data from the DC
                    'the first four are the text rectangle corners
                    Dim recPts As TPOINTS = pPth.First(4, True)
                    Dim txtRec As TPLANE = recPts.Bounds(aPl)
                    Dim tLims As TLIMITS = txtRec.Limits

                    If tLims.Left <> 0 Then
                        recPts.Add(-tLims.Left, 0)
                        recPts.Add(-tLims.Left, 0)
                        txtRec = recPts.Bounds(aPl)
                        tLims = txtRec.Limits
                    End If
                    Dim bndRec As TPLANE = pPth.Bounds(aPl)
                    Dim bLims As TLIMITS = bndRec.Limits
                    If bLims.Top <> 0 Then rYCorrections = patHt / bLims.Top
                    If rYCorrections <> 1 Then
                        aStyle.YCorrection = rYCorrections
                        pPth.Scale(rYCorrections, rYCorrections)
                        bndRec = pPth.Bounds(aPl)
                        bLims = bndRec.Limits
                        recPts.Scale(rYCorrections, rYCorrections)
                        txtRec = recPts.Bounds(aPl)
                        tLims = txtRec.Limits
                    End If
                    _rVal.Tracking = tLims.Right - bLims.Right
                    _rVal.Width = bLims.Right
                    _rVal.Ascent = patHt
                    _rVal.Descent = aStyle.DescentFactor * _rVal.Ascent
                    _rVal.Index = -1
                    _rVal.Charr = ""
                    Dim shp As New TSHAPE
                    shp.Path.Add(0.125 * bLims.Right, aCode:=dxxVertexStyles.MOVETO)
                    shp.Path.Add(0.125 * bLims.Right, 0.75 * patHt, aCode:=dxxVertexStyles.LINETO)
                    shp.Path.Add(0.875 * bLims.Right, 0.75 * patHt, aCode:=dxxVertexStyles.LINETO)
                    shp.Path.Add(0.875 * bLims.Right, aCode:=dxxVertexStyles.LINETO)
                    shp.Path.Add(0.125 * bLims.Right, aCode:=dxxVertexStyles.LINETO)
                    _rVal.Shape = shp
                    Dim fMat As TCHARFORMAT = _rVal.Formats
                    fMat.StyleIndex = aStyle.StyleIndex
                    fMat.FontIndex = aStyle.FontIndex
                    fMat.IsShape = False
                    fMat.CharHeight = patHt
                    fMat.Tracking = 1
                    _rVal.Formats = fMat
                    _rVal.PathDefined = True
                End If
                aStyle.NullChar = _rVal
                If myStyle And bUpdateArray Then
                    UpdateStyle(aStyleIndex, aStyle)  '.Clone ' Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TFONTSTYLE)(aStyle)
                    dxoFonts.UpdateMember(Me)
                End If
                Return _rVal
            End Function
            Friend Function GetStyle(aStyleName As String, Optional bBold As Boolean = False, Optional bItalics As Boolean = False) As TFONTSTYLE
                Dim rStyleIndex As Integer = 0
                Return GetStyle(aStyleName, bBold, bItalics, rStyleIndex)
            End Function
            Friend Function GetStyle(aStyleName As String, bBold As Boolean, bItalics As Boolean, ByRef rStyleIndex As Integer) As TFONTSTYLE
                Dim _rVal As New TFONTSTYLE
                rStyleIndex = -1
                If String.IsNullOrWhiteSpace(aStyleName) Then aStyleName = "" Else aStyleName = aStyleName.Trim
                If IsShape Then
                    rStyleIndex = 1
                    _rVal = StyleArray.Item(1)
                    _rVal.IsShape = True
                Else
                    If aStyleName <> "" Then
                        _rVal = StyleArray.Item(aStyleName)
                        If _rVal.StyleIndex > 0 Then
                            rStyleIndex = _rVal.StyleIndex
                        End If
                    End If
                    If rStyleIndex = -1 Then
                        rStyleIndex = GetStyleID(bBold, bItalics)
                    End If
                    _rVal = StyleArray.Item(rStyleIndex)
                End If
                _rVal.StyleIndex = rStyleIndex
                _rVal.FontIndex = Index
                _rVal.IsShape = IsShape
                _rVal.NullChar.FontIndex = Index
                _rVal.NullChar.StyleIndex = rStyleIndex
                _rVal.NullChar.IsShape = IsShape
                Return _rVal
            End Function
            Friend Function GetStyleID(bBold As Boolean, bItalics As Boolean) As Integer
                Dim _rVal As Integer
                If (Not bBold And Not bItalics) Or IsShape Then
                    _rVal = 0
                Else
                    Dim i As Integer
                    Dim aStyle As New TFONTSTYLE
                    Dim sid As Integer
                    Dim aTTFStyle As dxxTextStyleFontSettings
                    sid = -1
                    If bBold And bItalics Then
                        aTTFStyle = dxxTextStyleFontSettings.BoldItalic
                    Else
                        If bBold Then aTTFStyle = dxxTextStyleFontSettings.Bold Else aTTFStyle = dxxTextStyleFontSettings.Italic
                    End If
                    For i = 1 To StyleArray.Count
                        aStyle = _Styles.Item(i)
                        If aStyle.TTFStyle = aTTFStyle Then
                            sid = i - 1
                            Exit For
                        End If
                    Next i
                    If sid < 0 And (bBold And bItalics) Then
                        aTTFStyle = dxxTextStyleFontSettings.Bold
                        For i = 1 To StyleArray.Count
                            aStyle = _Styles.Item(i)
                            If aStyle.TTFStyle = aTTFStyle Then
                                sid = i - 1
                                Exit For
                            End If
                        Next i
                    End If
                    If sid < 0 And (bBold And bItalics) Then
                        aTTFStyle = dxxTextStyleFontSettings.Italic
                        For i = 1 To StyleArray.Count
                            aStyle = _Styles.Item(i)
                            If aStyle.TTFStyle = aTTFStyle Then
                                sid = i - 1
                                Exit For
                            End If
                        Next i
                    End If
                    If sid >= 0 Then _rVal = sid
                End If
                Return _rVal
            End Function
            Friend Sub LoadShapes(Optional bUpdateFont As Boolean = True)
                '^reads in a shape fonts shape definitions
                If Not IsShape Or (IsShape And Shapes.Loaded) Then Return
                Dim aErr As String = String.Empty
                If Not Embedded Then
                    Dim aShapes As TSHAPES = TSHAPES.ReadFromFile(Me.Shapes.FileName, aErr, True)
                    aShapes.Loaded = True
                    _Shapes = aShapes
                End If
                Dim i As Integer
                Dim aShp As TSHAPE
                Dim aNC As TCHAR
                Dim aChar As TCHAR
                Dim idx As Long
                Dim ascnt As Double = TVALUES.To_DBL(Me.Shapes.Ascent)
                Dim dscnt As Double = TVALUES.To_DBL(Me.Shapes.Descent)
                Dim aStyle As New TFONTSTYLE With {
                    .FileName = FamilyName & ".shx",
                     .StyleIndex = 1,
                    .IsShape = True
                }
                Clear()
                AddStyle(aStyle)
                If ascnt <> 0 Then aStyle.DescentFactor = ascnt / dscnt
                aNC = CreateNullChar(aStyle.StyleIndex, aStyle, True)
                aNC.FontIndex = Index
                aNC.StyleIndex = 1
                aNC.IsShape = True
                aStyle.NullChar = aNC.Clone
                'cnt = 255
                aStyle.Characters = New TCHARARRAY(0)
                For i = 1 To Shapes.Count
                    aShp = _Shapes.Item(i)
                    'If aShp.ShapeNumber <= 255 Then
                    idx = aShp.ShapeNumber
                    aChar = aNC.Clone
                    aChar.FontIndex = Index
                    aChar.StyleIndex = 1
                    If aShp.ShapeNumber <= 255 Then
                        aChar.Charr = Chr(aShp.ShapeNumber)
                    End If
                    aChar.AsciiIndex = aShp.ShapeNumber
                    aChar.Ascent = ascnt
                    aChar.Descent = dscnt
                    aChar.PathDefined = False
                    aChar.Shape = aShp
                    aStyle.Characters.Add(aChar) ' .UpdateMember(idx + 1, aChar)
                Next i
                aStyle.FontIndex = Index
                aStyle.StyleIndex = 1
                aStyle.IsShape = True
                _Styles.SetItem(1, aStyle)
                _Shapes.Loaded = True
                If bUpdateFont Then dxoFonts.UpdateMember(Me)
            End Sub
            Public Overrides Function ToString() As String
                Return "TFONT [" & Name & "]"
            End Function
#End Region 'Methods
#Region "Shared Methods"
            Public Shared ReadOnly Property Null As TFONT
                Get
                    Return New TFONT("")
                End Get

            End Property
            Public Shared Function AlignmentH(aAlignment As dxxMTextAlignments) As dxxTextJustificationsHorizontal
                Dim _rVal As dxxTextJustificationsHorizontal
                '^returns DXF horizontal and vertical text alignment codes for the passed text alignment
                _rVal = dxxTextJustificationsHorizontal.Left
                Select Case aAlignment
                    Case dxxMTextAlignments.Fit
                        _rVal = dxxTextJustificationsHorizontal.Fit
                    Case dxxMTextAlignments.Aligned
                        _rVal = dxxTextJustificationsHorizontal.Align
                    Case dxxMTextAlignments.TopLeft, dxxMTextAlignments.MiddleLeft, dxxMTextAlignments.BottomLeft, dxxMTextAlignments.BaselineLeft
                        _rVal = dxxTextJustificationsHorizontal.Left
                    Case dxxMTextAlignments.TopCenter, dxxMTextAlignments.MiddleCenter, dxxMTextAlignments.BottomCenter, dxxMTextAlignments.BaselineMiddle
                        _rVal = dxxTextJustificationsHorizontal.Center
                    Case dxxMTextAlignments.TopRight, dxxMTextAlignments.MiddleRight, dxxMTextAlignments.BottomRight, dxxMTextAlignments.BaselineRight
                        _rVal = dxxTextJustificationsHorizontal.Right
                    Case Else
                        _rVal = dxxTextJustificationsHorizontal.Left
                End Select
                Return _rVal
            End Function
            Public Shared Function AlignmentV(aAlignment As dxxMTextAlignments) As dxxTextJustificationsVertical
                Dim _rVal As dxxTextJustificationsVertical
                '^returns the vertical text alignment codes for the passed text alignment
                Select Case aAlignment
                    Case dxxMTextAlignments.Fit
                        _rVal = dxxTextJustificationsVertical.Baseline
                    Case dxxMTextAlignments.Aligned
                        _rVal = dxxTextJustificationsVertical.Baseline
                    Case dxxMTextAlignments.TopLeft, dxxMTextAlignments.TopCenter, dxxMTextAlignments.TopRight
                        _rVal = dxxTextJustificationsVertical.Top
                    Case dxxMTextAlignments.MiddleLeft, dxxMTextAlignments.MiddleCenter, dxxMTextAlignments.MiddleRight
                        _rVal = dxxTextJustificationsVertical.Middle
                    Case dxxMTextAlignments.BottomLeft, dxxMTextAlignments.BottomCenter, dxxMTextAlignments.BottomRight
                        _rVal = dxxTextJustificationsVertical.Bottom
                    Case dxxMTextAlignments.BaselineLeft, dxxMTextAlignments.BaselineMiddle, dxxMTextAlignments.BaselineRight
                        _rVal = dxxTextJustificationsVertical.Baseline
                    Case Else

                        _rVal = dxxTextJustificationsVertical.Baseline
                End Select
                Return _rVal
            End Function


            Public Shared Function DecodeAlignment(vAlign As dxxTextJustificationsVertical, hAlign As dxxTextJustificationsHorizontal) As dxxMTextAlignments
                Dim _rVal As dxxMTextAlignments
                '^returns DXF mtext alignment based on the passed vertical and horizontal alignment codes
                If vAlign = dxxTextJustificationsVertical.Baseline Then
                    If hAlign = dxxTextJustificationsHorizontal.Left Then
                        _rVal = dxxMTextAlignments.BaselineLeft
                    ElseIf hAlign = dxxTextJustificationsHorizontal.Center Or hAlign = dxxTextJustificationsHorizontal.HMiddle Then
                        _rVal = dxxMTextAlignments.BaselineMiddle
                    ElseIf hAlign = dxxTextJustificationsHorizontal.Right Then
                        _rVal = dxxMTextAlignments.BaselineRight
                    ElseIf hAlign = dxxTextJustificationsHorizontal.Fit Then
                        _rVal = dxxMTextAlignments.Fit
                    ElseIf hAlign = dxxTextJustificationsHorizontal.Align Then
                        _rVal = dxxMTextAlignments.Aligned
                    Else
                        _rVal = dxxMTextAlignments.BaselineLeft
                    End If
                Else
                    If vAlign = dxxTextJustificationsVertical.Bottom Then
                        If hAlign = dxxTextJustificationsHorizontal.Center Or hAlign = dxxTextJustificationsHorizontal.HMiddle Then
                            _rVal = dxxMTextAlignments.BottomCenter
                        ElseIf hAlign = dxxTextJustificationsHorizontal.Left Then
                            _rVal = dxxMTextAlignments.BottomLeft
                        ElseIf hAlign = dxxTextJustificationsHorizontal.Right Then
                            _rVal = dxxMTextAlignments.BottomRight
                        End If
                    ElseIf vAlign = dxxTextJustificationsVertical.Middle Then
                        If hAlign = dxxTextJustificationsHorizontal.Center Or hAlign = dxxTextJustificationsHorizontal.HMiddle Then
                            _rVal = dxxMTextAlignments.MiddleCenter
                        ElseIf hAlign = dxxTextJustificationsHorizontal.Left Then
                            _rVal = dxxMTextAlignments.MiddleLeft
                        ElseIf hAlign = dxxTextJustificationsHorizontal.Right Then
                            _rVal = dxxMTextAlignments.MiddleRight
                        End If
                    ElseIf vAlign = dxxTextJustificationsVertical.Top Then
                        If hAlign = dxxTextJustificationsHorizontal.Center Or hAlign = dxxTextJustificationsHorizontal.HMiddle Then
                            _rVal = dxxMTextAlignments.TopCenter
                        ElseIf hAlign = dxxTextJustificationsHorizontal.Left Then
                            _rVal = dxxMTextAlignments.TopLeft
                        ElseIf hAlign = dxxTextJustificationsHorizontal.Right Then
                            _rVal = dxxMTextAlignments.TopRight
                        End If
                    End If
                End If
                Return _rVal
            End Function
            Public Shared Sub EncodeAlignment(aAlignment As dxxMTextAlignments, ByRef vAlign As dxxTextJustificationsVertical, ByRef hAlign As dxxTextJustificationsHorizontal)
                '^returns DXF horizontal and vertical text alignment codes for the passed text alignment
                vAlign = TFONT.AlignmentV(aAlignment)
                hAlign = TFONT.AlignmentH(aAlignment)
            End Sub

            Private Shared _PercentCodes As List(Of String)
            Public Shared ReadOnly Property PercentCodes As List(Of String)
                Get
                    If _PercentCodes Is Nothing Then
                        _PercentCodes = New List(Of String)({"%%D", "%%C", "%%P"})
                    End If
                    Return _PercentCodes
                End Get
            End Property

            Public Shared Function CADTextToScreenText(txt As String) As String
                Dim _rVal As String = txt
                '#1the string to convert to screen text
                '^used to replace AutoCAD "%%" character codes found in the passed string to their ascii symbol equivalent
                Dim Codes As List(Of String) = PercentCodes


                For Each cd As String In Codes
                    Dim str As String = String.Empty
                    Select Case UCase(cd)
             'degree symbol
                        Case "%%D"
                            str = Chr(186)
             'diameter symbol
                        Case "%%C"
                            str = Chr(216)
             'plus minus
                        Case "%%P"
                            str = Chr(177)
                    End Select
                    If str <> "" Then
                        If _rVal.Contains(cd) Then
                            _rVal = _rVal.Replace(cd.ToUpper(), str)
                            _rVal = _rVal.Replace(cd.ToLower(), str)
                        End If
                    End If
                Next


                Return _rVal
            End Function
            Public Shared Function LoadEmbeddedShapeFont(aFilename As String, Optional bReplaceExisting As Boolean = False) As TFONT
                '#1the path to the target .SHP file
                '#2flag to overwrite the definitions of shape fonts that are already defined
                '^loads a font from the target .SHP file
                '~errors are ignored
                Dim aFont As TFONT
                Dim bBadRead As Boolean

                Dim s As Stream = Nothing

                Try
                    If Not String.IsNullOrEmpty(aFilename) Then aFilename = aFilename.Trim Else aFilename = ""
                    If aFilename.Length < 5 Then Return Nothing
                    s = Assembly.GetExecutingAssembly.GetManifestResourceStream(aFilename)
                    If s Is Nothing Then Return Nothing

                    Dim ext As String = Path.GetExtension(aFilename).ToUpper()
                    If ext <> ".SHX" And ext <> ".SHP" Then Return Nothing
                    Dim fname As String = Path.GetFileNameWithoutExtension(aFilename)

                    If ext = ".SHP" Then
                        aFont = TFONT.ReadFromResourceSHP(s, $"{fname}.shx", bBadRead)
                    Else
                        aFont = TFONT.ReadFromResourceSHX(s, $"{fname}.shx", bBadRead)
                    End If
                    If bBadRead Then
                        Return TFONT.Null
                    Else
                        Return aFont
                    End If
                Catch ex As Exception
                    Return Nothing
                Finally
                    If s IsNot Nothing Then
                        s.Close()
                        s.Dispose()
                    End If

                End Try
            End Function
            Friend Shared Function ReadFromResourceSHP(aStream As Stream, aFilename As String, ByRef rErrFlag As Boolean) As TFONT
                Dim _rVal As TFONT = Nothing
                If String.IsNullOrWhiteSpace(aFilename) Or aStream Is Nothing Then Return _rVal
                Dim fname As String
                fname = Replace(aFilename, ".shp", ".shx", Compare:=vbTextCompare)
                Dim fam As String = Path.GetFileNameWithoutExtension(fname)
                fname = Path.GetFileName(fname)
                Dim aFont As New TFONT(fam, fname, dxxFontTypes.Embedded)

                Dim aErr As String = String.Empty
                Dim aReader As StreamReader = Nothing
                Dim aStyle As TFONTSTYLE
                '#1the file name to read the shape characters from
                '^returns true if the characters were read succesfully
                Try
                    aReader = New StreamReader(aStream)
                    'read thew shapes
                    Dim aShapes As TSHAPES = TSHAPES.ReadSHP(aReader, aFilename, True, aErr, False)
                    aStream.Close()
                    aStream.Dispose()
                    aReader.Dispose()
                    If Not aFilename.Contains(".") Then aFilename += ".SHX"
                    If aErr <> "" Then
                        rErrFlag = True
                        Return Nothing
                    Else
                        If aShapes.Ascent > 0 And aShapes.Descent > 0 Then
                            aFont.Shapes = aShapes
                            aStyle = aFont.StyleArray.Item(1)
                            aStyle.StyleIndex = 1
                            aStyle.TTFStyle = dxxTextStyleFontSettings.Regular
                            aStyle.FileName = aFilename
                            If aShapes.Ascent <> 0 Then
                                aStyle.DescentFactor = Math.Abs(aShapes.Descent / aShapes.Ascent)
                            End If
                            aFont.UpdateStyle(1, aStyle)
                            _rVal = aFont
                        Else
                            rErrFlag = True
                        End If
                        rErrFlag = False
                        Return _rVal
                    End If
                Catch ex As Exception
                    If aStream IsNot Nothing Then aStream.Dispose()
                    If aReader IsNot Nothing Then aReader.Dispose()
                    rErrFlag = True
                    aErr = ex.Message
                    Return Nothing
                End Try
            End Function
            Friend Shared Function ReadFromResourceSHX(aStream As Stream, aFilename As String, ByRef rErrFlag As Boolean) As TFONT
                If String.IsNullOrWhiteSpace(aFilename) Then Return Nothing
                aFilename = aFilename.Trim
                Dim _rVal As TFONT = Nothing
                Dim ext As String = Path.GetExtension(aFilename)
                Dim fname As String = Path.GetFileNameWithoutExtension(aFilename)
                If String.Compare(ext, ".shp", True) Then ext = ".shx"
                Dim aFont As New TFONT(fname, fname & ext, dxxFontTypes.Embedded)
                Dim aShapes As New TSHAPES
                Dim aErr As String = String.Empty
                Dim Bytes(0) As Byte
                Dim nStyle As New TFONTSTYLE
                '#1the file name to read the shape characters from
                '^returns true if the characters were read succesfully
                Try
                    ReDim Bytes(0 To aStream.Length - 1)
                    aStream.Read(Bytes, 0, Bytes.Length)
                    aShapes = TSHAPES.ReadSHX(Bytes, aFilename, True, aErr, False)
                    aStream.Close()
                    aStream.Dispose()
                    If aErr <> "" Then
                        rErrFlag = True
                        Return Nothing
                    Else
                        If Not aShapes.IsFont Then
                            rErrFlag = True
                            Return Nothing
                        Else
                            If aShapes.Ascent > 0 And aShapes.Descent > 0 Then
                                aFont.Shapes = aShapes
                                nStyle.StyleIndex = 1
                                nStyle.TTFStyle = dxxTextStyleFontSettings.Regular
                                nStyle.FileName = aFilename
                                If aShapes.Ascent <> 0 Then
                                    nStyle.DescentFactor = Math.Abs(aShapes.Descent / aShapes.Ascent)
                                End If
                                aFont.AddStyle(nStyle)
                                _rVal = aFont
                            Else
                                rErrFlag = True
                            End If
                            rErrFlag = False
                            Return _rVal
                        End If
                    End If
                Catch ex As Exception
                    rErrFlag = True
                    Return Nothing
                End Try
            End Function
            Public Shared Function LoadFromFile(aFileName As String, ByRef rErrFlag As Boolean, bJustHeaders As Boolean, ByRef rError As String) As TFONT
                '#1the file name to read the shape characters from
                '^returns true if the characters were read succesfully
                rError = string.Empty
                rErrFlag = False
                Dim aFont As TFONT = TFONT.Null
                Try
                    Dim fname As String = Path.GetFileNameWithoutExtension(aFileName)
                    aFont = New TFONT(fname, fname, dxxFontTypes.Shape)
                    fname += ".shx"
                    Dim aShapes As TSHAPES = TSHAPES.ReadFromFile(aFileName, rError, True, True, bJustHeaders)
                    Dim aStyle As TFONTSTYLE
                    If rError <> "" Then
                        rErrFlag = True
                    Else
                        If aShapes.Ascent > 0 And aShapes.Descent > 0 Then
                            aFont.Name = fname
                            aFont.Shapes = aShapes
                            aStyle = aFont.StyleArray.Item(1)
                            aStyle.StyleIndex = 1
                            aStyle.TTFStyle = dxxTextStyleFontSettings.Regular
                            aStyle.FileName = aFileName
                            If aShapes.Ascent <> 0 Then
                                aStyle.DescentFactor = Math.Abs(aShapes.Descent / aShapes.Ascent)
                            End If
                            aFont.UpdateStyle(1, aStyle)
                        Else
                            rErrFlag = True
                            rError = "Shape Ascent And Descent Undefined"
                            aFont = Nothing
                        End If
                    End If
                Catch ex As Exception
                    rError = True
                    rError = ex.Message
                End Try
                Return aFont
            End Function
#End Region 'Shared Methods
        End Structure 'TFONT
        Friend Structure TFONTS
#Region "Members"
            Friend _DefaultIndex As Integer

            Private _Init As Boolean
            Private _Members() As TFONT
#End Region 'Members
#Region "Constructors"

            Public Sub New(aDummy As String)
                _DefaultIndex = 0
                _Init = True

                ReDim _Members(-1)
            End Sub

#End Region 'Constructors
#Region "Properties"

            Public ReadOnly Property IsEmpty As Boolean
                Get
                    If Not _Init Then Return True Else Return Count <= 0
                End Get
            End Property
            Public ReadOnly Property Count As Integer
                Get
                    If Not _Init Then
                        ReDim _Members(-1)
                        _Init = True
                        Return 0
                    End If
                    Return _Members.Count
                End Get
            End Property
            Public ReadOnly Property DefaultIndex As Integer
                Get
                    Return _DefaultIndex
                End Get
            End Property
            Public ReadOnly Property DefaultMember
                Get
                    If IsEmpty Then Return TFONT.Null
                    If DefaultIndex > 0 And DefaultIndex <= Count Then Return _Members(DefaultIndex - 1) Else Return TFONT.Null
                End Get
            End Property
#End Region 'Properties
#Region "Methods"


            Public Function TryGet(aName As String, ByRef rMember As TFONT) As Boolean
                rMember = TFONT.Null
                If IsEmpty Or String.IsNullOrWhiteSpace(aName) Then Return False
                Dim mems = _Members.Where(Function(x) String.Compare(x.Name, aName, True) = 0)
                If mems.Count > 0 Then
                    Dim idx = Array.IndexOf(Of TFONT)(_Members, mems(0))
                    _Members(idx).Index = idx + 1
                    rMember = _Members(idx)
                    Return True
                Else
                    Return False
                End If
            End Function
            Public Function Contains(aName As String) As Boolean
                If IsEmpty Or String.IsNullOrWhiteSpace(aName) Then Return False
                Dim mem As TFONT = TFONT.Null
                Return TryGet(aName, mem)
            End Function
            Friend Sub SetDefaultIndex(aIndex As Integer)
                If aIndex > 0 And aIndex <= Count Then
                    _DefaultIndex = aIndex
                End If
            End Sub
            Friend Sub Clear()
                _Init = True
                ReDim _Members(-1)

            End Sub
            Public Function Item(aIndex As Integer, Optional getDefault As Boolean = False) As TFONT
                If IsEmpty Then Return TFONT.Null
                If aIndex < 1 Or aIndex > Count And getDefault Then aIndex = DefaultIndex
                If aIndex < 1 Or aIndex > Count Then Return TFONT.Null
                Return _Members(aIndex - 1)
            End Function
            Public Function Member(aKeyOrName As String) As TFONT
                If String.IsNullOrWhiteSpace(aKeyOrName) Or IsEmpty Then Return TFONT.Null
                Dim sKey As String = dxfUtils.LeftOf(aKeyOrName.ToLower(), ".")
                Dim ext As String = $".{dxfUtils.RightOf(aKeyOrName.ToLower(), ".", bFromEnd:=True)}"
                If ext = "." Then ext += ".ttf"
                If ext = ".shp" Then ext = ".shx"
                If ext <> ".shx" And ext <> ".ttf" Then ext = ".ttf"

                sKey += ext
                Dim _rVal As TFONT = TFONT.Null
                TryGet(sKey, _rVal)
                Return _rVal

            End Function
            Public Function MemberSHX(aKeyOrName As String) As TFONT
                If String.IsNullOrWhiteSpace(aKeyOrName) Or IsEmpty Then Return TFONT.Null

                Dim sKey As String = dxfUtils.LeftOf(aKeyOrName.ToLower(), ".")
                Dim ext As String = $".{dxfUtils.RightOf(aKeyOrName.ToLower(), ".", bFromEnd:=True)}"
                If ext <> ".shx" Then ext = ".shx"

                sKey += ext
                Dim idx As Integer = 0
                Dim _rVal As TFONT = TFONT.Null
                TryGet(sKey, _rVal)
                Return _rVal

            End Function
            Public Function MemberTTF(aKeyOrName As String) As TFONT
                If String.IsNullOrWhiteSpace(aKeyOrName) Or IsEmpty Then Return TFONT.Null


                Dim sKey As String = dxfUtils.LeftOf(aKeyOrName.ToLower(), ".")
                Dim ext As String = $".{dxfUtils.RightOf(aKeyOrName.ToLower(), ".", bFromEnd:=True)}"
                If ext <> ".ttf" Then ext = ".ttf"

                sKey += ext
                Dim idx As Integer = 0
                Dim _rVal As TFONT = TFONT.Null
                TryGet(sKey, _rVal)
                Return _rVal


            End Function
            Friend Function GetByFaceName(aFontName As String, bShapes As Boolean) As Integer
                If String.IsNullOrWhiteSpace(aFontName) Or IsEmpty Then Return 0

                '#1the font name to search for
                '^retrieves a font from the array based on the passed font name

                aFontName = aFontName.Trim.ToUpper
                Dim _rVal As Integer = 0



                For i As Integer = 1 To _Members.Count
                    Dim aFnt As TFONT = _Members(i - 1)
                    Dim ftest As String = aFnt.FamilyName.ToUpper
                    If (bShapes And aFnt.IsShape) Or (Not bShapes And Not aFnt.IsShape) Then
                        If ftest = aFontName Then
                            _rVal = i
                            Exit For
                        End If
                    End If
                Next
                Return _rVal
            End Function
            Friend Function UpdateMember(aIndex As Integer, aFont As TFONT) As Boolean
                If IsEmpty Then Return False

                If aIndex <= 0 Or aIndex > Count Then Return False

                aFont.Index = aIndex
                _Members(aIndex - 1) = aFont
                Return True
            End Function
            Friend Function UpdateMember(aFont As TFONT) As Boolean
                If String.IsNullOrWhiteSpace(aFont.Name) Or IsEmpty Then Return False
                Dim mname As String = aFont.Name.ToLower
                Dim ext As String = Path.GetExtension(mname)
                If String.IsNullOrWhiteSpace(ext) Then
                    mname += ".ttf"
                End If
                Dim mem As TFONT = TFONT.Null
                If Not TryGet(mname, mem) Then Return False
                aFont.Index = mem.Index
                _Members(mem.Index - 1) = aFont
                Return True
            End Function

            Private Sub Repopulate(aMems As List(Of TFONT), Optional bAddClones As Boolean = False)
                ReDim _Members(-1)
                _Init = True
                If aMems Is Nothing Then Return
                If aMems.Count <= 0 Then Return
                Dim j As Integer
                System.Array.Resize(_Members, aMems.Count)
                For Each mem As TFONT In aMems
                    Dim newmem As TFONT = mem
                    If bAddClones Then newmem = New TFONT(mem)
                    j += 1
                    newmem.Index = j
                    _Members(j - 1) = newmem
                Next

            End Sub

            Friend Function Remove(aName As String) As Boolean
                If String.IsNullOrWhiteSpace(aName) Or IsEmpty Then Return False
                Dim mname As String = aName.ToLower().Trim()
                Dim ext As String = Path.GetExtension(mname)
                If String.IsNullOrWhiteSpace(ext) Then
                    mname += ".ttf"
                End If
                Dim allmems As List(Of TFONT) = _Members.ToList()
                Dim idx As Integer = allmems.FindIndex(Function(x) String.Compare(x.Name, aName, True) = 0)
                If idx < 0 Then Return False
                allmems.RemoveAt(idx)
                Repopulate(allmems)
                Return True
            End Function
            Friend Function Remove(aIndex As Integer) As Boolean
                If IsEmpty Then Return False
                If aIndex < 1 Or aIndex > Count Then Return False
                Dim allmems As List(Of TFONT) = _Members.ToList()
                allmems.RemoveAt(aIndex - 1)
                Return True

            End Function
            Friend Function Add(aNewMem As TFONT, Optional bReplaceExisting As Boolean = True) As Boolean
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If

                Dim sKey As String = aNewMem.Name
                If Not sKey.Contains(".") Then
                    If aNewMem.IsShape Then sKey += ".SHX" Else sKey += ".TTF"
                End If
                sKey = sKey.ToLower
                aNewMem.Key = sKey
                If aNewMem.Name = "" Then Return False
                Dim existing As TFONT = TFONT.Null
                If TryGet(aName:=aNewMem.Name, existing) Then
                    If bReplaceExisting Then
                        aNewMem.Index = existing.Index
                        _Members(aNewMem.Index - 1) = aNewMem
                    Else
                        Return False
                    End If

                Else
                    System.Array.Resize(_Members, _Members.Count + 1)
                    aNewMem.Index = _Members.Count
                    _Members(_Members.Count - 1) = aNewMem
                End If

                Return True
            End Function
            Friend Sub Append(aFonts As TFONTS, Optional bReplaceExisting As Boolean = False)
                For i As Integer = 1 To aFonts.Count
                    Add(aFonts.Item(i), bReplaceExisting)
                Next
            End Sub
            Public Function Contains(aFontKey As String, Optional bDefautlToShape As Boolean = True) As Boolean
                Dim rIndex As Integer = 0
                Return Contains(aFontKey, rIndex, bDefautlToShape)
            End Function
            Public Function Contains(aFontKey As String, ByRef rIndex As Integer, Optional bDefautlToShape As Boolean = True) As Boolean

                rIndex = 0
                If IsEmpty Or String.IsNullOrWhiteSpace(aFontKey) Then Return False

                aFontKey = aFontKey.Trim()
                If Not aFontKey.EndsWith(".shx", comparisonType:=StringComparison.OrdinalIgnoreCase) And Not aFontKey.EndsWith(".ttf", comparisonType:=StringComparison.OrdinalIgnoreCase) Then

                    If bDefautlToShape Then aFontKey += ".shx" Else aFontKey += ".ttf"
                End If
                Dim existing As TFONT = TFONT.Null
                Dim _rVal As Boolean = TryGet(aFontKey, existing)
                If _rVal Then rIndex = existing.Index
                Return _rVal
            End Function
            Public Sub LoadEmbeddedFonts(Optional bOverwriteExisting As Boolean = False)
                '^Loads the default fonts that are emmbed in the assembly
                Dim rnames() As String
                Dim rname As String
                Dim assy As Assembly
                Dim bJustHeaders As Boolean = True
                Dim sErr As String = String.Empty
                Dim aFont As TFONT = Nothing
                Dim ext As String
                Try
                    bJustHeaders = False
                    'loop on emmbeded resources
                    assy = Assembly.GetExecutingAssembly
                    rnames = assy.GetManifestResourceNames
                    For Each rname In rnames '  i As Integer = 0 To rnames.Length - 1
                        'rname = rnames(i)
                        ext = Path.GetExtension(rname)
                        If String.Compare(ext, ".shp", True) = 0 Or String.Compare(ext, ".shx", True) = 0 Then
                            Try
                                aFont = TFONT.LoadEmbeddedShapeFont(rname)
                                '  If aFont.Index <> -1 Then
                                If bOverwriteExisting Or (Not bOverwriteExisting And Not Contains(aFont.Name)) Then
                                    Add(aFont)
                                    If DefaultIndex <= 0 Then
                                            If Item(Count).Key = "txt.shx" Then
                                                SetDefaultIndex(Count)
                                            End If
                                        End If
                                    End If
                                ' End If
                            Catch ex As Exception
                                'just skip it
                            End Try
                        End If
                    Next
                    If Count > 0 And DefaultIndex <= 0 Then
                        SetDefaultIndex(1)
                    End If
                Catch ex As Exception
                    If dxfUtils.RunningInIDE Then MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name} -{ex.Message }")

                End Try
            End Sub
            Public Function LoadShapeFonts(Optional aSHPFolderSpec As String = "", Optional aOverrideExisting As Boolean = False, Optional aLoadSHPFiles As Boolean = False) As Integer
                '#1the folder to load the fonts from
                '#2flag to overwrite the definitions of shape fonts that are already defined
                '#3flag to load files with the SHP extension also
                '^loads all the fonts from .SHP files found in the passed folder
                '~errors are ignored
                Dim fspec As String
                'load the fonts in the font directory if it is found
                If String.IsNullOrWhiteSpace(aSHPFolderSpec) Then
                    fspec = Path.GetDirectoryName(Application.ExecutablePath) & "\Fonts"
                Else
                    fspec = aSHPFolderSpec
                End If
                If Not Directory.Exists(fspec) Then Return 0
                Dim _rVal As Integer = 0
                Dim di As New DirectoryInfo(fspec)
                Dim aryFi As FileInfo() = di.GetFiles("*.shx")
                Dim finfo As FileInfo
                For Each finfo In aryFi
                    If LoadShapeFont(finfo.FullName, aOverrideExisting, False) Then
                        _rVal += 1
                    End If
                Next
                If aLoadSHPFiles Then
                    aryFi = di.GetFiles("*.shp")
                    For Each finfo In aryFi
                        If LoadShapeFont(finfo.FullName, False, False) Then
                            _rVal += 1
                        End If
                    Next
                End If
                Return _rVal
            End Function
            Public Sub LoadSystemSHXFonts(Optional bReplaceExisting As Boolean = False)
                '^ load all shape files in the executable path folder and any subfolder called "FONTS"
                Dim fspec As String = Path.GetDirectoryName(Application.ExecutablePath)
                LoadShapeFonts(fspec, aOverrideExisting:=bReplaceExisting, aLoadSHPFiles:=True)
                LoadShapeFonts(fspec & "\Fonts", aOverrideExisting:=bReplaceExisting, aLoadSHPFiles:=True)
                Dim aFlrs As List(Of String) = goACAD.FontFolders(True)
                For Each fldr As String In aFlrs
                    LoadShapeFonts(fspec, aOverrideExisting:=bReplaceExisting, aLoadSHPFiles:=False)
                Next
            End Sub
            Public Function LoadShapeFont(aFileName As String, Optional bReplaceExisting As Boolean = False, Optional bJustHeaders As Boolean = False) As Boolean
                Dim rError As String = string.Empty
                Return LoadShapeFont(aFileName, bReplaceExisting, bJustHeaders, rError)
            End Function
            Public Function LoadShapeFont(aFileName As String, bReplaceExisting As Boolean, bJustHeaders As Boolean, ByRef rError As String) As Boolean
                Dim _rVal As Boolean
                rError = string.Empty
                '#1the path to the target .SHP file
                '#2flag to overwrite the definitions of shape fonts that are already defined
                '^loads a font from the target .SHP file
                '~errors are ignored
                If String.IsNullOrWhiteSpace(aFileName) Then
                    rError = "Invalid Filename Passed"
                    Return False
                End If
                aFileName = aFileName.Trim
                If aFileName.Length < 5 Then
                    rError = "Invalid Filename Passed"
                    Return False
                End If
                Dim ext As String = Path.GetExtension(aFileName)
                If String.Compare(ext, ".SHX", True) <> 0 And String.Compare(ext, ".SHP", True) <> 0 Then
                    rError = "Invalid Filename Passed"
                    Return _rVal
                End If
                If Not File.Exists(aFileName) Then
                    rError = "File Not Found"
                    Return _rVal
                End If
                Dim font As dxoFont
                Dim idx As Integer
                Dim fname As String
                Try
                    fname = Path.GetFileNameWithoutExtension(aFileName)
                    idx = GetByFaceName(fname, True)
                    If idx > 0 Then
                        If Not bReplaceExisting Then
                            Return _rVal
                        End If
                    End If
                    Dim readerr As String = String.Empty
                    font = dxoFonts.ReadShapeFont(aFileName, readerr)
                    If readerr <> "" Then
                        rError = readerr
                        Return _rVal
                    End If
                    If idx > 0 Then
                        UpdateMember(font.FontStructure)
                        _rVal = True
                    Else
                        Add(font.FontStructure)
                        _rVal = True
                    End If
                    Return _rVal
                Catch ex As Exception
                    rError = ex.Message
                    Return False
                End Try
            End Function
            Public Sub LoadTrueTypeFonts(Optional aFamilyName As String = Nothing, Optional bReplaceExisting As Boolean = False)
                '^obtains the font metrics of all the available true type fonts on the current system
                Dim loaded As Boolean
                Dim err As String = String.Empty
                Dim idx As Integer

                'get the available fonts that will be loaded
                Dim subset As List(Of dxoFont) = dxoFonts.GetAvailableFonts()
                If Not String.IsNullOrWhiteSpace(aFamilyName) Then
                    subset = New List(Of dxoFont)(From mem In subset Where String.Compare(mem.Family, aFamilyName, True) = 0)
                End If
                'loop on the installed fonts families and add one font for each family and the styles below each
                For Each f As dxoFont In subset
                    loaded = LoadTrueTypeFont($"{f.Name}", False, idx, err)
                    If Not loaded Then
                        Debug.WriteLine($"{f.Name}.ttf Was Not Loaded - {err}")
                    End If
                Next
            End Sub
            Public Function LoadTrueTypeFont(aFontFamilyName As String, bReplaceExisting As Boolean, ByRef rIndex As Integer, ByRef rErrorString As String) As Boolean
                '^loads the requested true type font form the available ttf fonts to the global fonts lists
                rErrorString = ""
                rIndex = 0
                If String.IsNullOrWhiteSpace(aFontFamilyName) Then Return False
                aFontFamilyName = aFontFamilyName.Trim()
                'Dim f As TypographicFontFamily
                ' Dim s As TypographicFont = Nothing

                Dim fidx As Integer = Count + 1
                Dim fam As String = Path.GetFileNameWithoutExtension(aFontFamilyName)
                Dim fntName As String = $"{fam}.ttf"
                'see if the font is already loaded
                Dim isMember As Boolean = Contains(fam, rIndex, bDefautlToShape:=False)
                If isMember Then
                    If Not bReplaceExisting Then
                        Return False
                    Else
                        fidx = rIndex
                    End If
                End If
                'get the font from the available fonts
                Dim ttffont As dxoFont = dxoFonts.Find(fntName)
                If Not ttffont.Found Then
                    rErrorString = ttffont.ErrorString
                    Return False
                End If

                Dim fname As String = $"{fam}.ttf"
                Try
                    Dim ffam As Drawing.FontFamily = New System.Drawing.FontFamily(ttffont.Family)

                Catch ex As Exception
                    rErrorString = $"Family '{ ttffont.Family }' Was Not Found"
                    Return False

                End Try

                Dim fnt As TFONT = ttffont.FontStructure
                For i As Integer = 1 To fnt.StyleArray.Count
                    Dim style As TFONTSTYLE = fnt.StyleArray.Item(i)
                    fnt.CreateNullChar(i, style, True, style.YCorrection)
                    style.Characters = New TCHARARRAY(256)

                    fnt.UpdateStyle(i, style)
                Next

                Add(fnt)
                Return True


            End Function
            Public Function GetNames(Optional aSuppressShapes As Boolean = False, Optional aSuppressTrueTypes As Boolean = False, Optional aSorted As Boolean = False, Optional aSuppressExtenstions As Boolean = False, Optional rIndices As List(Of Integer) = Nothing) As List(Of String)
                Dim _rVal As New List(Of String)
                '#1flag to not return shape fonts in the list
                '#2flag to not return true type fonts
                '#3flag sort the returned collection alphabetically
                '#4flag to include the fonts file extension in the name
                '^returns the names of the members in the global font array
                If aSuppressShapes And aSuppressTrueTypes Then aSuppressTrueTypes = False
                Dim arNames As New TVALUES
                Dim arIndices As New TVALUES
                Dim nm As String
                Dim cnt As Integer
                Dim bKeep As Boolean
                Dim fnt As TFONT
                If Count <= 0 Then Return _rVal
                Dim srt As New List(Of Tuple(Of String, Integer))
                cnt = -1
                For i As Integer = 1 To Count
                    fnt = Item(i)
                    nm = fnt.FaceName
                    bKeep = True
                    If aSuppressShapes And fnt.IsShape Then bKeep = False
                    If aSuppressTrueTypes And Not fnt.IsShape Then bKeep = False
                    If bKeep Then
                        If Not aSuppressExtenstions Then
                            If fnt.IsShape Then nm += ".shx" Else nm += ".ttf"
                        End If
                        srt.Add(New Tuple(Of String, Integer)(nm, i))
                    End If
                Next i
                If aSorted Then srt.Sort(Function(tupl1 As Tuple(Of String, Integer), tupl2 As Tuple(Of String, Integer)) tupl1.Item1.CompareTo(tupl2.Item1))
                For i As Integer = 1 To srt.Count
                    _rVal.Add(srt.Item(i - 1).Item1)
                    If rIndices IsNot Nothing Then rIndices.Add(srt.Item(i - 1).Item2)
                Next
                Return _rVal
            End Function
            Friend Function GetFontName(aFontIndex As Integer) As String
                '#1the index of the font to search for
                '^retrieves the name of the font at the passed index
                If aFontIndex >= 1 And aFontIndex <= Count Then
                    Return Item(aFontIndex).Name
                Else
                    Return String.Empty
                End If
            End Function
            Public Function GetIndexByName(aFontName As String, ByRef rIsShape As Boolean, ByRef rFound As Boolean) As Integer
                Dim _rVal As Integer = 0
                Dim truetype As Boolean
                Dim shapetype As Boolean
                '#1the font name to search for
                '^retrieves a font from the array based on the passed font name
                rFound = False
                rIsShape = False

                If String.IsNullOrWhiteSpace(aFontName) Then Return _rVal
                aFontName = aFontName.Trim.ToUpper
                truetype = aFontName.EndsWith(".TTF")
                shapetype = aFontName.EndsWith(".SHP") Or aFontName.EndsWith(".SHX")
                _rVal = 0
                Try
                    Dim fam As String = aFontName
                    If fam.Contains(".") Then fam = dxfUtils.LeftOf(fam, ".", bFromEnd:=True)
                    Dim bExtensionPassed As Boolean = truetype Or shapetype
                    Dim bKeep As Boolean
                    If aFontName.EndsWith(".SHP") Then
                        aFontName = fam & ".SHX"
                    End If
                    If Not bExtensionPassed Then
                        'look for a shape
                        bKeep = Contains($"{fam}.SHX", _rVal)
                        If Not bKeep Then
                            truetype = True
                            aFontName += ".TTF"
                        End If
                    End If
                    bKeep = Me.Contains(aFontName, _rVal)
                    Dim serr As String = String.Empty
                    If truetype And Not bKeep Then
                        bKeep = LoadTrueTypeFont(aFontName, False, rIndex:=_rVal, serr)
                    End If
                    If Not bKeep Then
                        _rVal = DefaultIndex
                        If _rVal <= 0 Then _rVal = 1
                        rFound = False
                    End If
                    If _rVal > 0 Then
                        rFound = True
                        rIsShape = Item(_rVal).IsShape
                    Else
                        System.Diagnostics.Debug.WriteLine($"FONT Not FOUND - { aFontName}")
                    End If
                    Return _rVal
                Catch ex As Exception
                    Return _rVal
                End Try
            End Function
            Friend Function GetCharacter(ByRef ioChar As TCHAR, aPlane As TPLANE, Optional bRecomputePath As Boolean = False, Optional bDoPlane As Boolean = False) As TCHAR
                Dim _rVal As TCHAR = ioChar
                Dim fMats As TCHARFORMAT = ioChar.Formats
                Dim cScl As Single
                Dim fid As Integer
                Try
                    'get the font
                    If fMats.FontIndex > 0 And fMats.FontIndex <= Count Then fid = fMats.FontIndex Else fid = DefaultIndex
                    Dim sid As Integer = fMats.StyleIndex
                    If fMats.Tracking <= 0 Then fMats.Tracking = 1
                    If fMats.WidthFactor <= 0 Then fMats.WidthFactor = 1
                    If fMats.CharHeight <= 0 Then fMats.CharHeight = 1

                    'get the font style
                    Dim tfont As TFONT = Item(fid, True)
                    Dim tstyle As TFONTSTYLE
                    If sid < 0 Then sid = 1
                    If sid > tfont.StyleArray.Count Then sid = 1
                    tstyle = tfont.StyleArray.Item(sid)
                    If tstyle.Characters.Count <= 0 Or tfont.IsShape Then sid = 1
                    tstyle = tfont.StyleArray.Item(sid)
                    tstyle.FontIndex = fid
                    tfont.UpdateStyle(sid, tstyle)
                    'make sure the shape fonts have been loaded
                    If tfont.IsShape And Not tfont.Shapes.Loaded Then
                        tfont.LoadShapes()
                    End If
                    Dim font As dxoFont
                    If tfont.IsShape Then
                        font = New dxoFont(tfont)
                    Else
                        font = dxfGlobals.GetFont(tfont.Name)
                        tstyle = font.GetStyleStructure(tstyle.StyleName)
                    End If
                    'make sure the null character has been defined

                    Dim sChar As TCHAR = tstyle.NullCharacter
                    Dim bNullC As Boolean = ioChar.AsciiIndex < 0
                    Dim bFound As Boolean = False
                    'get the character
                    If Not bNullC Then
                        sChar = tstyle.GetCharacter(aCharCode:=ioChar.AsciiIndex, bFound)
                        If Not bFound Then
                            bNullC = True
                        Else
                            '=============================================================
                            'compute or recompute the character path vectors
                            '=============================================================
                            'bRecomputePath = True


                            If bRecomputePath Or Not sChar.PathDefined Then
                                If Not ioChar.IsFormatCode Then
                                    If tfont.IsShape Then
                                        sChar = tstyle.CreateChar_SHP(sChar, tfont.Shapes)
                                    Else
                                        sChar = tstyle.CreateChar_TTF(sChar, tfont.Bitmap)
                                    End If
                                    'save the character to the style for later use
                                    tstyle.Characters.UpdateMember(sChar)
                                    'save the updated style back to the font
                                    tfont.UpdateStyle(sid, tstyle)
                                    font.FontStructure = tfont
                                    'save the font back to
                                    dxoFonts.UpdateMember(tfont)
                                End If
                            End If

                        End If
                    End If
                    'set the return as a copy of the global font styles character
                    _rVal = New TCHAR(sChar) With
                    {
                      .Formats = fMats,
                    .GroupIndex = ioChar.GroupIndex,
                    .LineNo = ioChar.LineNo,
                    .StringIndex = ioChar.LineNo,
                    .LineIndex = ioChar.LineIndex,
                    .ReplacedChar = ioChar.ReplacedChar,
                    .FormatCode = ioChar.FormatCode
                    }

                    _rVal.GroupIndex = 0

                    'scale for Height and width
                    cScl = _rVal.CharHeight / sChar.CharHeight ' dxfGlobals.CharPatternHt
                    _rVal.Scale(aXScale:=cScl * fMats.WidthFactor, aYScale:=cScl * fMats.HeightFactor)


                    If Not _rVal.IsFormatCode Then

                        'format the character

                        If fMats.Tracking <> 1 Then
                            _rVal.Width *= fMats.Tracking
                        End If
                        'apply formats
                        If fMats.Overline Or fMats.Underline Or fMats.StrikeThru Or fMats.Backwards Or fMats.UpsideDown Or fMats.ObliqueAngle <> 0 Then
                            _rVal.ApplyFormats()
                        End If
                    End If

                    'apply plane rotations
                    If bDoPlane Then
                        _rVal.TransferToPlane(aPlane)
                    Else
                        _rVal.CharBox.CopyDirections(aPlane)
                    End If
                    'Application.DoEvents()
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine($"fnt_GetCharacter ERROR: { ex.Message}")
                End Try
                Return _rVal
            End Function

            Friend Function GetNullCharacter(aFontIndex As Integer, aStyleIndex As Integer) As TCHAR
                Dim _rVal As New TCHAR(" ")
                Dim fid As Integer
                Try
                    'get the font
                    If aFontIndex > 0 And aFontIndex <= Count Then fid = aFontIndex Else fid = DefaultIndex

                    Dim tfont As TFONT = Item(fid, True)

                    Dim sid As Integer = aStyleIndex

                    Dim tstyle As TFONTSTYLE
                    If sid < 0 Then sid = 1
                    If sid > tfont.StyleArray.Count Then sid = 1
                    tstyle = tfont.StyleArray.Item(sid)
                    If tstyle.Characters.Count <= 0 Or tfont.IsShape Then sid = 1
                    tstyle = tfont.StyleArray.Item(sid)
                    tstyle.FontIndex = fid
                    tfont.UpdateStyle(sid, tstyle)
                    'make sure the shape fonts have been loaded
                    If tfont.IsShape And Not tfont.Shapes.Loaded Then
                        tfont.LoadShapes()
                    End If
                    Dim font As dxoFont
                    If tfont.IsShape Then
                        font = New dxoFont(tfont)
                    Else
                        font = dxfGlobals.GetFont(tfont.Name)
                        tstyle = font.GetStyleStructure(tstyle.StyleName)
                    End If

                    _rVal = tstyle.NullChar

                    If Not _rVal.PathDefined Then

                        _rVal = tfont.CreateNullChar(tstyle.StyleIndex, tstyle, True, tstyle.YCorrection)
                        tfont.UpdateStyle(sid, tstyle)
                        font.FontStructure = tfont
                        'save the font back to
                        dxoFonts.UpdateMember(tfont)


                    End If

                    Return _rVal




                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine($"fnt_GetCharacter ERROR: { ex.Message}")
                    Return _rVal
                End Try

            End Function

            Friend Sub GetCharacterPath(ioChar As dxoCharacter, Optional bRecomputePath As Boolean = False, Optional bDontFormat As Boolean = False)
                If ioChar Is Nothing Then Return

                Dim fMats As TCHARFORMAT = New TCHARFORMAT(ioChar.Formats)
                Dim cScl As Single
                Dim fid As Integer
                Try
                    'get the font
                    If fMats.FontIndex > 0 And fMats.FontIndex <= Count Then fid = fMats.FontIndex Else fid = DefaultIndex
                    Dim sid As Integer = fMats.StyleIndex
                    If fMats.Tracking <= 0 Then fMats.Tracking = 1
                    If fMats.WidthFactor <= 0 Then fMats.WidthFactor = 1
                    If fMats.CharHeight <= 0 Then fMats.CharHeight = 1

                    'get the font style
                    Dim tfont As TFONT = Item(fid, getDefault:=True)
                    Dim tstyle As TFONTSTYLE
                    If sid < 0 Then sid = 1
                    If sid > tfont.StyleArray.Count Then sid = 1
                    tstyle = tfont.StyleArray.Item(sid)
                    If tstyle.Characters.Count <= 0 Or tfont.IsShape Then sid = 1
                    tstyle = tfont.StyleArray.Item(sid)
                    tstyle.FontIndex = fid
                    'make sure the shape fonts have been loaded
                    If Not tstyle.NullChar.PathDefined Then
                        tstyle.NullChar = GetNullCharacter(fid, sid)
                    End If


                    tfont.UpdateStyle(sid, tstyle)

                    If tfont.IsShape And Not tfont.Shapes.Loaded Then
                        tfont.LoadShapes()
                    End If
                    Dim font As dxoFont
                    If tfont.IsShape Then
                        font = New dxoFont(tfont)
                    Else
                        font = dxfGlobals.GetFont(tfont.Name)
                        tstyle = font.GetStyleStructure(tstyle.StyleName)
                    End If
                    'make sure the null character has been defined
                    Dim bFound As Boolean = False

                    Dim asciicode As Integer = ioChar.AsciiIndex
                    If Not String.IsNullOrWhiteSpace(ioChar.ReplacedChar) Then
                        asciicode = Asc(ioChar.ReplacedChar.Chars(0))
                    End If


                    Dim fontChar As TCHAR = tstyle.GetCharacter(aCharCode:=asciicode, bFound)

                    Dim bNullC As Boolean = Not bFound

                    'get the character
                    If bFound Then
                        '=============================================================
                        'compute or recompute the character path vectors
                        '=============================================================
                        'bRecomputePath = True

                        If bRecomputePath Or Not fontChar.PathDefined Then

                            If tfont.IsShape Then
                                fontChar = tstyle.CreateChar_SHP(fontChar, tfont.Shapes)
                            Else
                                fontChar = tstyle.CreateChar_TTF(fontChar, tfont.Bitmap)
                            End If
                            'save the character to the style for later use
                            tstyle.Characters.UpdateMember(fontChar)
                            'save the updated style back to the font
                            tfont.UpdateStyle(sid, tstyle)
                            font.FontStructure = tfont
                            'save the font back to
                            dxoFonts.UpdateMember(tfont)

                        End If


                    End If

                    Dim sChar As TCHAR = New TCHAR(fontChar)

                    'scale for Height and width
                    cScl = ioChar.CharHeight / fontChar.CharHeight ' dxfGlobals.CharPatternHt
                    If bDontFormat Or (fMats.WidthFactor = 1 And fMats.HeightFactor = 1) Then
                        sChar.Scale(aXScale:=cScl, aYScale:=cScl)

                    Else
                        sChar.Scale(aXScale:=cScl * fMats.WidthFactor, aYScale:=cScl * fMats.HeightFactor)
                    End If
                    'set the return as a copy of the global font styles character
                    ioChar.Copy(sChar)
                    If bDontFormat Then Return


                    'format the character

                    If fMats.Tracking <> 1 Then

                        ioChar.Width *= fMats.Tracking
                    End If
                    'apply formats
                    If fMats.Overline Or fMats.Underline Or fMats.StrikeThru Or fMats.Backwards Or fMats.UpsideDown Or fMats.ObliqueAngle <> 0 Then
                        ioChar.ApplyFormats()
                    End If
                    'Application.DoEvents()
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine($"fnt_GetCharacter ERROR: { ex.Message}")
                End Try

            End Sub

            Friend Function GetFontStyleInfo(Optional aFontName As String = "", Optional aIndex As Integer = 0, Optional aStyleIndex As Integer = 0, Optional bReturnDefault As Boolean = True, Optional aTTFStyle As dxxTextStyleFontSettings = dxxTextStyleFontSettings.Regular) As TFONTSTYLEINFO
                Dim _rVal As TFONTSTYLEINFO
                If String.IsNullOrWhiteSpace(aFontName) Then aFontName = "" Else aFontName = aFontName.Trim

                Dim bFound As Boolean
                Dim aFnt As TFONT = TFONT.Null
                Dim bisShape As Boolean = False
                Dim aStyle As New TFONTSTYLE(aFontName, 0)
                If aIndex < 1 Or aIndex > Count Then
                    aIndex = GetIndexByName(aFontName, bisShape, bFound)
                End If
                bFound = aIndex > 0
                If Not bFound And bReturnDefault Then
                    If DefaultIndex > 0 Then aIndex = DefaultIndex Else aIndex = 1
                End If
                bFound = aIndex > 0
                If Not bFound Then aStyleIndex = -1
                If bFound Then
                    aFnt = Item(aIndex)
                    bisShape = aFnt.IsShape
                    If aFnt.IsShape Then aStyleIndex = 1
                    If aStyleIndex < 1 Or aStyleIndex > aFnt.StyleArray.Count Then
                        aStyle = aFnt.StyleArray.Item(dxfEnums.Description(aTTFStyle))
                        aStyleIndex = aStyle.StyleIndex
                    End If
                    If aStyleIndex < 1 Or aStyleIndex > aFnt.StyleArray.Count Then
                        aStyleIndex = 1
                    End If
                    aStyle = aFnt.StyleArray.Item(aStyleIndex)
                    If bisShape Then
                        If Not aFnt.Shapes.Loaded Then
                            aFnt.LoadShapes()
                        End If
                    End If
                End If
                _rVal = New TFONTSTYLEINFO(aFnt, aStyle)
                Return _rVal
            End Function
            Friend Function GetFontStyleInfoByString(aFontString As String) As TFONTSTYLEINFO
                Dim _rVal As New TFONTSTYLEINFO With {.NotFound = True}
                If Not String.IsNullOrWhiteSpace(aFontString) Then aFontString = aFontString.Trim Else Return _rVal
                If Count <= 0 Then Return _rVal
                Dim aFontName As String



                Dim aFntStyle As dxxTextStyleFontSettings
                'extract the font name from the passed format string
                'i.e. \fArial.ttf|b1|i0|
                If String.Compare(aFontString.Substring(0, 2), "\f", True) = 0 Then
                    aFontString = aFontString.Substring(2, aFontString.Length - 2)
                End If

                If aFontString.Contains("|") Then
                    aFontName = dxfUtils.LeftOf(aFontString, "|")
                Else
                    aFontName = aFontString
                End If
                Dim bBold As Boolean = aFontString.IndexOf("|b1|", StringComparison.OrdinalIgnoreCase) + 1 > 0
                Dim bItalics As Boolean = aFontString.IndexOf("|i1|", StringComparison.OrdinalIgnoreCase) + 1 > 0
                If bBold And bItalics Then
                    aFntStyle = dxxTextStyleFontSettings.BoldItalic
                ElseIf bBold Then
                    aFntStyle = dxxTextStyleFontSettings.Bold
                ElseIf bItalics Then
                    aFntStyle = dxxTextStyleFontSettings.Italic
                Else
                    aFntStyle = dxxTextStyleFontSettings.Regular
                End If
                _rVal = GetFontStyleInfo(aFontName, 0, 0, False, aFntStyle)
                Return _rVal
            End Function
            Friend Sub WriteToDebug(Optional bShowIndices As Boolean = False)
                Dim i As Integer
                Dim j As Integer
                Dim aFnt As TFONT
                Try
                    For i = 1 To Count
                        aFnt = Item(i)
                        If bShowIndices Then
                            Debug.WriteLine(i & " - " & aFnt.Name)
                        Else
                            Debug.WriteLine(aFnt.Name)
                        End If
                        If aFnt.FontType = dxxFontTypes.TTF Then
                            For j = 1 To aFnt.StyleArray.Count
                                Debug.WriteLine("    " & aFnt.StyleArray.Item(j).StyleName)
                            Next
                        End If
                    Next i
                Catch ex As Exception
                End Try
            End Sub
#End Region 'Methods
#Region "Shared Methods"

            Friend Shared ReadOnly Property Null As TFONTS
                Get
                    Return New TFONTS("")
                End Get
            End Property
#End Region 'Shared Methods

        End Structure 'TFONTS
#End Region 'Structures
#Region "Shared Methods"
        Private Shared _FontForm As frmFonts
        Friend Shared ReadOnly Property goFontForm As frmFonts
            Get
                If _FontForm Is Nothing Then _FontForm = New frmFonts
                Return _FontForm
            End Get
        End Property
#End Region 'Shared Methods
    End Class 'dxfFonts
End Namespace
