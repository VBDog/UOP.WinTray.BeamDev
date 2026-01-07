
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Friend NotInheritable Class dxfGlobals
#Region "Constants"

        Public Shared ReadOnly Property Delim As Char = "¸" ' Chr(184)
        Public Shared ReadOnly Property subDelim As Char = "¤" 'Chr(164) 
        Public Shared ReadOnly Property breaker As Char = "«" 'Chr(171)
        Public Shared ReadOnly Property endbreaker As Char = "»" 'Chr(187)
        Public Shared ReadOnly Property MultiDelim As Char = "§" ' Chr(167)

        Public Shared ReadOnly Property BadBlockChars As String = "<>/\:;?*|,=`"""

        Public Shared ReadOnly Property InvalidBlockCharacters As List(Of Char) = New List(Of Char)({"<", ">", ":", ";", "?", "|", "=", "'", """"})

        Public Shared ReadOnly Property CharCodes As String = ":A:C:F:f:T:Q:W:H:O:o:L:l:S:"
        Public Shared ReadOnly Property TemDXFFileName As String = "TempDXF"


        Public Shared ReadOnly Property ArrowDecode As String = "-3=Various,-2=Suppressed,-1=ByStyle,0=ClosedFilled,1=ClosedBlank,2=Closed,3=Dot,4=ArchTick,5=Oblique,6=Open,7=Origin,8=Origin2,9=Open90,10=Open30,11=DotSmall,12=Small,13=DotBlank,14=BoxBlank,15=BoxFilled,16=DatumBlank,17=DatumFilled,18=Integral,19=None,20=UserDefined"
        Public Shared ReadOnly Property TextAlignDecode As String = "1=Top Left,2=Top Center,3=Top Right,4=Middle Left,5=Middle Center,6=Middle Right,7=Bottom Left,8=Bottom Center,9=Bottom Right,10=Baseline Left,11=Baseline Center,12=Baseline Right,13=Fit,14=Aligned"
        Public Shared ReadOnly Property ColorDecode As String = "-1=Undefined,0=ByBlock,1=Red,2=Yellow,3=Green,4=Cyan,5=Blue,6=Magenta,7=BlackWhite,8=Grey,30=Orange,151=Light Blue,81=Light Green,131=Light Cyan,11=Light Red,211=Light Magenta,51=Light Yellow,9=Light Grey,255=White,250=Black,256=ByLayer"

        Public Shared ReadOnly Property GraphictypeDecode As String = "-2=EndBlock,-1=SequenceEnd,0=Undefined,1=Point,2=Line,4=Arc,8=Ellipse,16=Bezier,32=Solid,64=Polyline,128=Shape,256=Polygon,512=Text,1024=Insert,2048=Leader,4096=Dimension,8192=Hatch,16384=Hole,32768=Symbol,65536=Table"


        Public Shared ReadOnly Property CharPatternHt As Integer = 500
        Public Shared ReadOnly Property CommonProps As Integer = 18
        Public Shared ReadOnly Property BlackWhiteBrightness As Integer = 35

        Public Shared ReadOnly Property kappa As Double = 0.55228474983079
        Public Shared ReadOnly Property TA_BASELINE As UInteger = 24
        Public Shared ReadOnly Property TA_BOTTOM As UInteger = 8
        Public Shared ReadOnly Property TA_CENTER As UInteger = 6
        Public Shared ReadOnly Property TA_LEFT As UInteger = 0
        Public Shared ReadOnly Property TA_NOUPDATECP As UInteger = 0
        Public Shared ReadOnly Property TA_RIGHT As UInteger = 2
        Public Shared ReadOnly Property TA_TOP As UInteger = 0
        Public Shared ReadOnly Property TA_UPDATECP As UInteger = 1
        Public Shared ReadOnly Property TA_MASK As UInteger = (TA_BASELINE + TA_CENTER + TA_UPDATECP)
        Public Shared ReadOnly Property PT_BEZIERTO As Int32 = &H4
        Public Shared ReadOnly Property PT_CLOSEFIGURE As Int32 = &H1
        Public Shared ReadOnly Property PT_LINETO As Int32 = &H2
        Public Shared ReadOnly Property PT_MOVETO As Int32 = &H6
        Public Shared ReadOnly Property PT_PIXELTO As Int32 = &H10

        Public Shared ReadOnly Property GDI_ERROR As Int32 = &HFFFF
#End Region 'Constants
#Region "Members"
        Private Shared _Init As Boolean
        Private Shared _Fonts As TFONTS

        Private Shared _WCS As dxfPlane
        Private Shared _INIFile As String

        Private Shared _AvailableFonts As List(Of dxoFont)
        Private Shared _ACAD As dxpACAD
        Private Shared _DefaultLineWeights As TTABLE

        Private Shared _Events As dxpEventHandler
        Private Shared _HatchPatterns As dxpHatchPatterns
        Private Shared _newImg As dxfImage
        Private Shared _FontsLoaded As Boolean
        Private Shared _PixelsPerInch As Double

        Private Shared _Aggregator As EventAggregator
#Region "Constructors"

#End Region 'Constructors
#Region "Properties"
        Friend Shared ReadOnly Property Aggregator As EventAggregator
            Get
                If Not _Init Then Init()
                Return _Aggregator
            End Get
        End Property
        Friend Shared Property FontsAreLoaded As Boolean
            Get
                Return _FontsLoaded
            End Get
            Set(value As Boolean)

            End Set
        End Property
        Friend Shared ReadOnly Property goEvents As dxpEventHandler
            Get
                If Not _Init Then Init()
                Return _Events
            End Get

        End Property
        Friend Shared ReadOnly Property goHatchPatterns As dxpHatchPatterns
            Get
                If Not _Init Then Init()
                Return _HatchPatterns
            End Get

        End Property
        Friend Shared ReadOnly Property goACAD As dxpACAD
            Get
                If Not _Init Then Init()
                Return _ACAD
            End Get
        End Property


#End Region 'Properties
#End Region 'Members
#Region "Methods"
        Private Shared Sub Init()
            If _Init Then Return
            _Init = True
            Try

                If _Aggregator Is Nothing Then _Aggregator = New EventAggregator

                If _ACAD Is Nothing Then _ACAD = New dxpACAD

                If _Events Is Nothing Then _Events = New dxpEventHandler
                If _HatchPatterns Is Nothing Then
                    _HatchPatterns = New dxpHatchPatterns
                    If IO.File.Exists(IO.Path.Combine(Application.ExecutablePath, "acad.pat")) Then
                        _HatchPatterns.LoadFromFile(IO.Path.Combine(Application.ExecutablePath, "acad.pat"))
                    End If
                End If
                If Not dxfGlobals.FontsAreLoaded And dxfUtils.RunningInIDE Then
                    dxoFonts.Initialize()
                End If
            Catch ex As Exception
                _Init = False
                If dxfUtils.RunningInIDE Then
                    MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name} - {ex.Message }")
                End If
            End Try

        End Sub
        Friend Shared Function DefaultLineWeights_Get() As TTABLE
            Return _DefaultLineWeights
        End Function

        Friend Shared Sub DefaultLineWeights_Set(aTable As TTABLE)
            _DefaultLineWeights = aTable
        End Sub






        Friend Shared Function New_Image(Optional aTemplate As String = "") As dxfImage

            If aTemplate Is Nothing Then aTemplate = ""
            aTemplate = aTemplate.Trim()
            If _newImg IsNot Nothing AndAlso _newImg.Disposed Then
                _newImg = Nothing
            End If


            If aTemplate <> "" Then
                If dxfUtils.ValidateFileName(aTemplate) Then
                    If IO.File.Exists(aTemplate) Then

                        Dim aFile As New dxfFile(aTemplate)
                        _newImg = aFile.ReadToImage(aTemplate, aImage:=_newImg)
                    End If
                End If
            End If

            'Return New dxfImage

            If _newImg Is Nothing Then _newImg = New dxfImage

            Return _newImg
        End Function

#End Region 'Members
#Region "Methods"
        Friend Shared Function PixelsPerInch() As Integer
            'the screen
            'If aDC = 0 Then
            If _PixelsPerInch = 0 Then
                Dim aBmp As New Bitmap(100, 100, Imaging.PixelFormat.Format32bppArgb)
                Dim g As Graphics = Graphics.FromImage(aBmp)
                _PixelsPerInch = g.DpiX
                g.Dispose() ' APIWrapper.GetDeviceCaps(aDC, Gdi32.DeviceCap.LOGPIXELSX)
                'APIWrapper.ReleaseDC(lHd, aDC)
            End If
            Return _PixelsPerInch
            'Else
            '_rVal = APIWrapper.GetDeviceCaps(aDC, Gdi32.DeviceCap.LOGPIXELSX)
            'If _rVal = 0 Then _rVal = 96
            'End If
        End Function

        Friend Shared Function GetFont(aFontName As String, Optional aDefaultReturn As String = "txt.shx", Optional bAttemptLoadIfNotFound As Boolean = True, Optional bReturnDefault As Boolean = True) As dxoFont
            If String.IsNullOrWhiteSpace(aDefaultReturn) Then
                If bReturnDefault Then aDefaultReturn = "txt.shx" Else aDefaultReturn = ""
            End If
            If String.IsNullOrWhiteSpace(aFontName) Then aFontName = aDefaultReturn
            Dim fnt As String = aFontName.Trim().ToLower()


            If fnt.Contains(";") Then
                fnt = dxfUtils.LeftOf(fnt, ";", bFromEnd:=True)
            End If
            Dim fam As String = fnt
            Dim truetype As Boolean = fnt.EndsWith(".ttf")
            Dim shapetype As Boolean = fnt.EndsWith(".shx") Or fnt.EndsWith(".shp")
            Dim ext As String
            Dim typepassed As Boolean = truetype Or shapetype
            Dim _rVal As dxoFont = Nothing
            Dim idx As Integer
            If typepassed Then
                ext = IO.Path.GetExtension(fnt).ToLower()
                If ext = ".shp" Then ext = ".shx"
                fam = IO.Path.GetFileNameWithoutExtension(fnt)
            Else

                If fam.Contains(".") Then fam = dxfUtils.LeftOf(fam, ".", bFromEnd:=True)
                ext = ".ttf"
                truetype = True
            End If
            'first see if it is a loaded font
            Dim allfonts As TFONTS = dxfGlobals.Fonts
            Dim loadedfnt As TFONT = allfonts.Member($"{fam}.{ ext}")
            If loadedfnt.Index > 0 Then
                'just return it as it has been loaded to the global fonts array
                _rVal = New dxoFont(loadedfnt)
            Else
                'see if we can load the
                If bAttemptLoadIfNotFound And truetype Then
                    Dim serr As String = String.Empty
                    dxoFonts.LoadTrueTypeFont(fam, False, idx, serr)
                    If idx > 0 Then
                        loadedfnt = dxfGlobals.Fonts.Item(idx)
                        _rVal = New dxoFont(loadedfnt)
                    End If
                End If
            End If
            If _rVal Is Nothing Then
                If Not bReturnDefault Then Return _rVal
                'if the default return is different than the requested name return the default font is available
                If String.Compare(aFontName, aDefaultReturn, ignoreCase:=True) <> 0 Then
                    aFontName = aDefaultReturn
                    fnt = aFontName.Trim.ToLower

                    If fnt.Contains(";") Then
                        fnt = dxfUtils.LeftOf(fnt, ";", bFromEnd:=True)
                    End If
                    fam = fnt
                    truetype = fnt.EndsWith(".ttf")
                    shapetype = fnt.EndsWith(".shx") Or fnt.EndsWith(".shp")
                    loadedfnt = dxfGlobals.Fonts.Member($"{fam}{ext}")
                    If loadedfnt.Index > 0 Then
                        'just return it as it has been loaded to the global fonts array
                        _rVal = New dxoFont(loadedfnt)
                    Else
                        'see if we can load the
                        If truetype Then
                            Dim serr As String = String.Empty
                            dxoFonts.LoadTrueTypeFont(fam, False, idx, serr)
                            If idx > 0 Then
                                If dxoFonts.LoadTrueTypeFont(fam, False) Then
                                    Fonts = dxfGlobals.Fonts
                                    loadedfnt = dxfGlobals.Fonts.Item(idx)
                                    _rVal = New dxoFont(loadedfnt)
                                End If
                            End If

                        End If
                    End If
                End If
            End If
            If _rVal Is Nothing Then _rVal = New dxoFont() With {.FontStructure = dxfGlobals.Fonts.DefaultMember}
            Return _rVal
        End Function
        Friend Shared Property Fonts As TFONTS
            Get
                Try
                    If Not _FontsLoaded Then
                        _FontsLoaded = True
                        Dim aFonts As TFONTS = TFONTS.Null
                        _Fonts = TFONTS.Null
                        'dxfGlobals.SetFonts(Nothing)
                        '-------------- LOAD THE EMBEDDED SHAPE FONTS -------------------
                        aFonts.LoadEmbeddedFonts(False)
                        '-------------- LOAD THE AVAILABLE SHAPE FONTS -------------------
                        aFonts.LoadSystemSHXFonts()
                        '-------------- LOAD THE TRUE TYPE FONTS -------------------
                        'get the basic true type font structures with names defined (no Styles) listed in the registry
                        aFonts.LoadTrueTypeFonts("Arial")
                        _Fonts = aFonts
                    End If
                    Return _Fonts
                Catch ex As Exception
                    _FontsLoaded = False
                    Return TFONTS.Null
                End Try
            End Get
            Set(value As TFONTS)
                _Fonts = value
            End Set
        End Property

        Friend Shared Function g_WCS() As dxfPlane
            If _WCS Is Nothing Then _WCS = New dxfPlane
            Return _WCS
        End Function
        Friend Shared Function gsINIFilePath() As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            If String.IsNullOrWhiteSpace(_INIFile) Then _INIFile = ""
            If _INIFile = "" Then
                _INIFile = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                _INIFile = IO.Path.Combine(_INIFile, "dxfGraphics.INI")
                If Not IO.File.Exists(_INIFile) Then
                    Dim ifile As New IO.StreamWriter(_INIFile)
                    ifile.Close()
                End If
                If Not IO.File.Exists(_INIFile) Then _INIFile = "X"
            End If
            If _INIFile <> "X" Then _rVal = _INIFile
            Return _rVal
        End Function
        Friend Shared Property AvailableFonts As List(Of dxoFont)
            Get

                Return _AvailableFonts
            End Get
            Set(value As List(Of dxoFont))
                _AvailableFonts = value
            End Set
        End Property
#End Region 'Methods
    End Class 'dxfGlobals
End Namespace
