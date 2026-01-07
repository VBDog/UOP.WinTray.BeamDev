
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Drawing.Text
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports SharpDX.Direct2D1.Effects
Imports System.Windows
Imports System.Windows.Documents

Namespace UOP.DXFGraphics
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
    Public Class LOGFONT
        Public lfHeight As Integer
        Public lfWidth As Integer
        Public lfEscapement As Integer
        Public lfOrientation As Integer
        Public lfWeight As Integer
        Public lfItalic As Byte
        Public lfUnderline As Byte
        Public lfStrikeOut As Byte
        Public lfCharSet As Byte
        Public lfOutPrecision As Byte
        Public lfClipPrecision As Byte
        Public lfQuality As Byte
        Public lfPitchAndFamily As Byte
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=32)>
        Public lfFaceName As String

    End Class
    Public Class dxoFonts
#Region "Properties"
#End Region 'Properties
        Public Shared ReadOnly Property Count As Integer
            Get
                Return dxfGlobals.Fonts.Count
            End Get
        End Property

#Region "Methods"

        Friend Shared Function Member(aIndex As Integer) As TFONT
            '^creates the world vectors of the null char

            Return dxfGlobals.Fonts.Item(aIndex)

        End Function
        Friend Shared Function Member(aFontName As String) As TFONT
            '^creates the world vectors of the null char

            Return dxfGlobals.Fonts.Member(aFontName)

        End Function
        Public Shared Function GetAvailableFonts() As List(Of dxoFont)
            Return GetAvailableFonts(False)
        End Function
        Public Shared Function GetShapeFonts(Optional aSHPFolderSpec As String = "", Optional aLoadSHPFiles As Boolean = False) As List(Of dxoFont)
            Dim rError As String = String.Empty
            Return GetShapeFonts(aSHPFolderSpec, aLoadSHPFiles, rError)
        End Function
        Public Shared Function GetShapeFonts(aSHPFolderSpec As String, aLoadSHPFiles As Boolean, ByRef rError As String) As List(Of dxoFont)
            Dim _rVal As New List(Of dxoFont)
            '#1the folder to load the fonts from
            '#2flag to overwrite the definitions of shape fonts that are already defined
            '#3flag to load files with the SHP extension also
            '^loads all the fonts from .SHP files found in the passed folder
            '~errors are ignored
            Dim fspec As String
            Dim readerr As String = String.Empty
            'load the fonts in the font directory if it is found
            'load the fonts in the font directory if it is found
            If String.IsNullOrWhiteSpace(aSHPFolderSpec) Then
                fspec = $"{Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) }\Fonts"
            Else
                fspec = aSHPFolderSpec
            End If
            If Not Directory.Exists(fspec) Then Return _rVal
            Dim di As New DirectoryInfo(fspec)
            Dim aryFi As FileInfo() = di.GetFiles("*.shx")
            Dim font As dxoFont
            Dim finfo As FileInfo
            For Each finfo In aryFi
                font = dxoFonts.ReadShapeFont(finfo.FullName, readerr)
                If font IsNot Nothing And String.IsNullOrWhiteSpace(readerr) Then
                    _rVal.Add(font)
                End If
            Next
            If aLoadSHPFiles Then
                aryFi = di.GetFiles("*.shp")
                For Each finfo In aryFi
                    font = dxoFonts.ReadShapeFont(finfo.FullName, readerr)
                    If font IsNot Nothing And String.IsNullOrWhiteSpace(readerr) Then
                        _rVal.Add(font)
                    End If
                Next
            End If
            Return _rVal
        End Function
        Public Shared Function ReadShapeFont(aFileName As String) As dxoFont
            Dim rError As String = String.Empty
            Return ReadShapeFont(aFileName, rError)
        End Function
        Public Shared Function ReadShapeFont(aFileName As String, ByRef rError As String) As dxoFont
            rError = String.Empty
            '#1the path to the target .SHP file
            '#2flag to overwrite the definitions of shape fonts that are already defined
            '^loads a font from the target .SHP file
            '~errors are ignored
            Dim _rVal As dxoFont = Nothing
            If String.IsNullOrWhiteSpace(aFileName) Then
                rError = "Invalid Filename Passed"
                Return _rVal
            End If
            aFileName = aFileName.Trim
            If aFileName.Length < 5 Then
                rError = "Invalid Filename Passed"
                Return _rVal
            End If
            Dim ext As String = IO.Path.GetExtension(aFileName)
            If String.Compare(ext, ".SHX", True) <> 0 And String.Compare(ext, ".SHP", True) <> 0 Then
                rError = "Invalid Filename Passed"
                Return _rVal
            End If
            If Not IO.File.Exists(aFileName) Then
                rError = "File Not Found"
                Return _rVal
            End If
            Try
                Dim aFont As TFONT
                Dim bBadRead As Boolean
                Dim fname As String
                fname = IO.Path.GetFileNameWithoutExtension(aFileName)
                aFont = TFONT.LoadFromFile(aFileName, bBadRead, False, rError)
                If bBadRead Then Return _rVal
                _rVal = New dxoFont(aFont)
                Return _rVal
            Catch ex As Exception
                rError = ex.Message
            End Try
            Return _rVal
        End Function
        Public Shared Sub LoadSystemShapeFonts(aFonts As List(Of dxoFont))
            '^ load all shape files in the executable path folder and any subfolder called "FONTS"
            If aFonts Is Nothing Then Return
            Dim aFlrs As New List(Of String)
            Dim serror As String = String.Empty
            Dim fspec As String = IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)
            If Directory.Exists(fspec) Then
                aFlrs.Add(fspec)
                If Directory.Exists(fspec & "\Fonts") Then
                    aFlrs.Add(fspec & "\Fonts")
                End If
            End If
            aFlrs.AddRange(goACAD.FontFolders(True))
            Dim di As DirectoryInfo
            Dim aryFi As FileInfo()
            Dim font As dxoFont
            Dim finfo As FileInfo
            Dim readerr As String = String.Empty
            Dim fntname As String
            For Each fspec In aFlrs
                Try
                    di = New DirectoryInfo(fspec)
                    aryFi = di.GetFiles("*.shx")
                    For Each finfo In aryFi
                        fntname = IO.Path.GetFileName(finfo.FullName)
                        If aFonts.FindIndex(Function(x) String.Compare(x.Name, fntname, True) = 0) < 0 Then
                            font = dxoFonts.ReadShapeFont(finfo.FullName, readerr)
                            If font IsNot Nothing And String.IsNullOrWhiteSpace(readerr) Then aFonts.Add(font)
                        End If
                    Next
                Catch ex As Exception
                End Try
                Try
                    di = New DirectoryInfo(fspec)
                    aryFi = di.GetFiles("*.shp")
                    For Each finfo In aryFi
                        fntname = IO.Path.GetFileNameWithoutExtension(finfo.FullName) & ".shx"
                        If aFonts.FindIndex(Function(x) String.Compare(x.Name, fntname, True) = 0) < 0 Then
                            font = dxoFonts.ReadShapeFont(finfo.FullName, readerr)
                            If font IsNot Nothing And String.IsNullOrWhiteSpace(readerr) Then aFonts.Add(font)
                        End If
                    Next
                Catch ex As Exception
                End Try
            Next
        End Sub
        Public Shared Function GetEmbeddedFonts(ByRef rError As String) As List(Of dxoFont)
            '^Loads the default shape fonts that are emmbed in the assembly
            Dim _rVal As New List(Of dxoFont)

            Try

                'loop on emmbeded resources
                Dim assy As Assembly = Assembly.GetExecutingAssembly
                Dim rnames() As String = assy.GetManifestResourceNames
                For i As Integer = 0 To rnames.Length - 1
                    Dim rname As String = rnames(i)
                    If rname.EndsWith(".SHP", comparisonType:=StringComparison.OrdinalIgnoreCase) Or rname.EndsWith(".SHX", comparisonType:=StringComparison.OrdinalIgnoreCase) Then
                        Try
                            Dim aFont As TFONT = TFONT.LoadEmbeddedShapeFont(rname)
                            If Not String.IsNullOrWhiteSpace(aFont.Name) Then
                                Dim font As dxoFont = New dxoFont(aFont)
                                _rVal.Add(font)
                            End If
                        Catch ex As Exception
                            'just skip it
                        End Try
                    End If
                Next
            Catch ex As Exception
                rError = ex.Message
            End Try
            Return _rVal
        End Function

        Friend Shared Function GetSystemFontLibrary(Optional aCollector As List(Of dxoFont) = Nothing) As List(Of TFONT)
            Dim _rVal As New List(Of TFONT)
            Try
                Dim installed_fonts As New InstalledFontCollection
                Dim famname As String

                For Each font_family As FontFamily In installed_fonts.Families
                    Dim keep As Boolean = True
                    famname = font_family.Name

                    Dim newFont As New TFONT(famname, $"{famname}.ttf", aFontType:=dxxFontTypes.TTF, font_family)
                    If newFont.StyleArray.Count <= 0 Then Continue For
                    _rVal.Add(newFont)
                    If aCollector IsNot Nothing Then
                        aCollector.Add(New dxoFont(newFont))
                    End If
                Next


            Catch ex As Exception

            End Try

            Return _rVal


        End Function

        Friend Shared Function GetAvailableFonts(bReloadFontList As Boolean) As List(Of dxoFont)
            If bReloadFontList Then
                dxfGlobals.AvailableFonts = Nothing
            End If
            Dim _rVal As List(Of dxoFont) = dxfGlobals.AvailableFonts
            If _rVal Is Nothing Then _rVal = New List(Of dxoFont)
            If _rVal.Count > 0 Then Return _rVal
            Dim err As String = String.Empty
            'first get the shape fonts
            _rVal.AddRange(dxoFonts.GetEmbeddedFonts(err))
            dxoFonts.LoadSystemShapeFonts(_rVal)

            'add the available true types on the system
            Dim sysfonts As List(Of TFONT) = dxoFonts.GetSystemFontLibrary(aCollector:=_rVal)
            _rVal.Sort(Function(font1 As dxoFont, font2 As dxoFont) font1.Name.CompareTo(font2.Name))
            dxfGlobals.AvailableFonts = _rVal
            Return _rVal
        End Function


        Public Shared Function Find(aFontName As String, Optional bSearchLoadedFonts As Boolean = False) As dxoFont
            Dim _rVal As New dxoFont()
            If String.IsNullOrWhiteSpace(aFontName) Then
                Return New dxoFont() With {.ErrorString = "Null Font Name Passed"}
            End If
            Dim ext As String = IO.Path.GetExtension(aFontName)
            Dim typepassed As Boolean = Not String.IsNullOrWhiteSpace(ext)
            Dim fnt As String = IO.Path.GetFileNameWithoutExtension(aFontName)
            Dim truetype As Boolean = True
            Dim fam As String = fnt

            If typepassed Then
                If String.Compare(ext, ".shp", True) = 0 Then ext = ".shx"
                If String.Compare(ext, ".shx", True) = 0 Then truetype = False
            Else
                ext = ".ttf"
            End If
            Dim styinfo As New List(Of APIWrapper.FontData)
            Try
                If Not truetype Then _rVal.FontType = dxxFontTypes.Shape Else _rVal.FontType = dxxFontTypes.TTF
                'first see if it is a loaded font
                If bSearchLoadedFonts Then
                    Dim fonts As TFONTS = dxfGlobals.Fonts
                    _rVal.FontStructure = fonts.Member($"{fam}{ ext}")
                    _rVal.Found = _rVal.Index > 0
                    If _rVal.Found Then Return _rVal
                End If
                Dim allfonts As List(Of dxoFont) = dxoFonts.GetAvailableFonts()
                'get the font from the available fonts
                _rVal = dxoFonts.Find(allfonts, aFontName)
                If _rVal.Found And Not _rVal.IsShape Then
                    'try to get the font metrics for each style
                    Dim styleinfos As List(Of TFONTINFO) = APIWrapper.FontInfoArray()
                    If styleinfos IsNot Nothing Then
                        styleinfos = styleinfos.FindAll(Function(x) String.Compare(x.Family, fnt, True) = 0)
                        If (styleinfos.Count) <= 0 Then
                            Dim i As Integer = fnt.LastIndexOf(" ")
                            If i > 0 Then
                                Dim sfnt = fnt.Substring(0, i + 1).Trim()
                                styleinfos = styleinfos.FindAll(Function(x) String.Compare(x.Family, sfnt, True) = 0)
                            End If
                        End If
                        For Each style As dxoFontStyle In _rVal.Styles
                            Dim styleinfo As TFONTINFO = styleinfos.Find(Function(x) x.TTFStyle = style.TTFStyle)
                            If Not String.IsNullOrWhiteSpace(styleinfo.Family) Then
                                style.FontInfo = styleinfo
                            End If

                        Next


                    End If

                End If
                'If _rVal.Found And Not _rVal.IsShape Then
                'Dim keep As Boolean
                '    keep = False
                '    Dim styles As List(Of dxoFontStyle) = _rVal.Styles
                '    Dim newstyles As New List(Of dxoFontStyle)
                '    APIWrapper.GetFontLibrary()
                '    styinfo = APIWrapper.GetFontStyeList(_rVal.Name)
                '    Dim fdata As APIWrapper.FontData
                '    'If String.Compare(_rVal.Name, "Cambria", True) = 0 Then
                '    '    Beep()
                '    'End If
                '    Dim idx As Integer
                '    Dim styl As dxoFontStyle
                '    If styles.Count = 1 And styinfo.Count = 1 Then
                '        fdata = styinfo.Item(0)
                '        styl = styles.Item(0)
                '        'styl.FontData = fdata
                '        newstyles.Add(styl)
                '        styinfo.Remove(fdata)
                '    Else
                '        For Each styl In styles
                '            idx = styinfo.FindIndex(Function(x) String.Compare(x.StyleName, styl.StyleName, True) = 0)
                '            If idx >= 0 Then
                '                fdata = styinfo.Item(idx)

                '                newstyles.Add(styl)
                '                styinfo.Remove(fdata)
                '            Else
                '                Debug.Print($"Font Style Info For '{ styl.StyleName }' Was Not Found")
                '            End If
                '        Next
                '    End If
                '    If newstyles.Count <= 0 And styles.Count > 0 And styinfo.Count > 0 Then
                '        fdata = styinfo.Item(0)
                '        styl = styles.Item(0)
                '        'styl.FontData = fdata
                '        newstyles.Add(styl)
                '    End If
                '    If newstyles.Count <= 0 Then
                '        _rVal.ErrorString = $"Style Data for True Type Font '{ aFontName }' was not found"
                '    Else
                '        _rVal.Found = True
                '        _rVal.Styles = newstyles
                '    End If

                'End If
            Catch ex As Exception
                Debug.WriteLine($"dxoFonts.Find ERROR: { ex.Message}")
            End Try
            Return _rVal
        End Function
        Public Shared Function Find(aFonts As List(Of dxoFont), aFontName As String) As dxoFont
            If String.IsNullOrEmpty(aFontName) Then aFontName = "Arial.ttf"
            Dim ext As String = IO.Path.GetExtension(aFontName)
            Dim fam As String = IO.Path.GetFileNameWithoutExtension(aFontName)
            If String.IsNullOrWhiteSpace(ext) Then ext = ".ttf"
            If String.Compare(ext, ".shp", True) = 0 Then ext = ".shx"
            Dim truetype = String.Compare(ext, ".shx", True) <> 0
            If truetype Then ext = ".ttf"
            Dim fnt As String = fam & ext
            Dim font As dxoFont = Nothing
            If Not truetype Then
                font = aFonts.Find(Function(x) String.Compare(x.Name, fnt, True) = 0)
                If font Is Nothing Then font = aFonts.Find(Function(x) String.Compare(x.Name, fam & ".shx", True) = 0)
            Else
                font = aFonts.Find(Function(x) String.Compare(x.Name, fnt, True) = 0)
                If font Is Nothing Then font = aFonts.Find(Function(x) String.Compare(x.Name, fam & ".ttf", True) = 0)
            End If
            If font Is Nothing Then
                font = New dxoFont With {
                    .Found = False,
                    .ErrorString = $"Style Data for True Type Font '{ aFontName }' was not found"
                }
            Else
                font.Found = True
            End If
            Return font
        End Function
        Public Shared Function GetFontStyleNames(aFontName As String, Optional aFontIndex As Integer = 0) As List(Of String)
            '#1the subject fonts
            '#2the font name to search for
            '^retrieves a font from the array based on the passed font name
            Dim _rVal = New List(Of String)
            Try
                Dim allfonts As TFONTS = dxfGlobals.Fonts
                If String.IsNullOrWhiteSpace(aFontName) And (aFontIndex <= 0 Or aFontIndex > allfonts.Count) Then Return _rVal
                aFontName = aFontName.Trim.ToLower
                Dim fname As String
                Dim font As TFONT = TFONT.Null
                If aFontIndex > 0 And aFontIndex <= allfonts.Count Then
                    font = allfonts.Item(aFontIndex)
                Else
                    Dim bExt As Boolean = aFontName.Contains(".")
                    Dim fmem As TFONT
                    For i = 1 To allfonts.Count
                        fmem = allfonts.Item(i)
                        fname = fmem.Name
                        If Not bExt Then fname = fmem.FaceName.ToLower
                        If String.Compare(aFontName, fname, True) = 0 Then
                            font = fmem
                            Exit For
                        End If
                    Next i
                End If
                For j As Integer = 1 To font.StyleArray.Count
                    _rVal.Add(font.StyleArray.Item(j).StyleName)
                Next j
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Public Shared Function GetSystemTrueTypeFonts() As List(Of dxoFont)
            Dim _rVal As New List(Of dxoFont)
            Dim allfonts As List(Of dxoFont) = dxoFonts.GetAvailableFonts()
            Dim font As dxoFont
            For Each font In allfonts
                If font.FontType = dxxFontTypes.TTF Then
                    _rVal.Add(font)
                End If
            Next
            Return _rVal
        End Function
        Friend Shared Function UpdateMember(aFont As TFONT) As Boolean
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Dim _rVal As Boolean = fnts.UpdateMember(aFont)
            dxfGlobals.Fonts = fnts
            Return _rVal
        End Function
        Friend Shared Function UpdateMember(aFont As dxoFont) As Boolean
            If aFont Is Nothing Then Return False
            Return UpdateMember(aFont.FontStructure)
        End Function

        Friend Shared Sub ParseFontName(ByRef aFontName As String, ByRef rStyleName As String, ByRef rTTFStyle As dxxTextStyleFontSettings)

            Dim fstyl As String
            If String.IsNullOrWhiteSpace(rStyleName) Then fstyl = String.Empty Else fstyl = rStyleName.Trim()

            rStyleName = "Normal"
            rTTFStyle = dxxTextStyleFontSettings.Regular
            If String.IsNullOrWhiteSpace(aFontName) Then Return
            aFontName = aFontName.Trim()
            Dim sVals() As String
            Dim bBold As Boolean
            Dim bItalics As Boolean

            If aFontName.Contains(";") Then
                sVals = aFontName.Split(";")
                aFontName = sVals(0).Trim
                If fstyl = String.Empty Then
                    fstyl = sVals(1).Trim()
                End If
            End If
            If fstyl.IndexOf("Bold", StringComparison.OrdinalIgnoreCase) + 1 > 0 Then bBold = True
            If fstyl.IndexOf("|b1|", StringComparison.OrdinalIgnoreCase) + 1 > 0 Then bBold = True
            If fstyl.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) + 1 > 0 Then bItalics = True
            If fstyl.IndexOf("|i1|", StringComparison.OrdinalIgnoreCase) + 1 > 0 Then bItalics = True
            If bBold And bItalics Then
                rTTFStyle = dxxTextStyleFontSettings.BoldItalic
            ElseIf bBold Then
                rTTFStyle = dxxTextStyleFontSettings.Bold
            ElseIf bItalics Then
                rTTFStyle = dxxTextStyleFontSettings.Italic
            Else
                rTTFStyle = dxxTextStyleFontSettings.Regular
            End If
        End Sub

        Friend Shared Function GetFontStyleInfo(Optional aFontName As String = "", Optional aIndex As Integer = 0, Optional aStyleName As String = "", Optional aStyleIndex As Integer = 0, Optional bReturnDefault As Boolean = True, Optional aTTFStyle As dxxTextStyleFontSettings = dxxTextStyleFontSettings.Regular) As TFONTSTYLEINFO
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Dim _rVal As TFONTSTYLEINFO = fnts.GetFontStyleInfo(aFontName, aIndex, aStyleIndex, bReturnDefault, aTTFStyle)
            dxfGlobals.Fonts = fnts
            Return _rVal
        End Function
        Friend Shared Function GetFontStyleInfoByString(aFontString As String) As TFONTSTYLEINFO
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Dim _rVal As TFONTSTYLEINFO = fnts.GetFontStyleInfoByString(aFontString)
            dxfGlobals.Fonts = fnts
            Return _rVal
        End Function
        Friend Shared Function GetCharacter(ByRef ioChar As TCHAR, aPlane As TPLANE, Optional bRecomputePath As Boolean = False, Optional bDoPlane As Boolean = False) As TCHAR
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Dim _rVal As TCHAR = fnts.GetCharacter(ioChar, aPlane, bRecomputePath, bDoPlane)
            dxfGlobals.Fonts = fnts
            Return _rVal
        End Function
        Friend Shared Function GetNullCharacter(aFontIndex As Integer, aStyleIndex As Integer) As TCHAR
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Dim _rVal As TCHAR = fnts.GetNullCharacter(aFontIndex, aStyleIndex)

            dxfGlobals.Fonts = fnts
            Return _rVal
        End Function

        Friend Shared Sub GetCharacterPath(ioChar As dxoCharacter, Optional bRecomputePath As Boolean = False, Optional bDontFormat As Boolean = False)
            Dim fnts As TFONTS = dxfGlobals.Fonts
            fnts.GetCharacterPath(ioChar, bRecomputePath, bDontFormat)
            dxfGlobals.Fonts = fnts
        End Sub
        Public Shared Sub WriteToFile(aFilename As String)
            dxfGlobals.Fonts.WriteToDebug(True)
        End Sub
        Public Shared Function GetFontNames(Optional aSuppressShapes As Boolean = False, Optional aSuppressTrueTypes As Boolean = False, Optional aSorted As Boolean = False, Optional aSuppressExtenstions As Boolean = False) As List(Of String)
            '#1flag to not return shape fonts in the list
            '#2flag to not return true type fonts
            '#3flag sort the returned collection alphabetically
            '#4flag to include the fonts file extension in the name
            '^returns the names of the members in the global font array
            Return dxfGlobals.Fonts.GetNames(aSuppressShapes, aSuppressTrueTypes, aSorted, aSuppressExtenstions)
        End Function
        Public Shared Function LoadShapeFont(aFileName As String, Optional bReplaceExisting As Boolean = False) As Boolean
            '#1the path to the target .SHP file
            '#2flag to overwrite the definitions of shape fonts that are already defined
            '^loads a font from the target .SHP file
            '~errors are ignored
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Dim _rVal As Boolean = fnts.LoadShapeFont(aFileName, bReplaceExisting)
            dxfGlobals.Fonts = fnts
            Return _rVal
        End Function
        Friend Shared Function GetFontName(aFontIndex As Integer) As String
            Return dxfGlobals.Fonts.GetFontName(aFontIndex)
        End Function
        Public Shared Function LoadShapeFonts(aFolder As String, Optional aOverrideExisting As Boolean = False) As Integer
            '#1the folder to load the fonts from
            '#2flag to overwrite the definitions of shape fonts that are already defined
            '^loads all the fonts from .SHP files found in the passed folder
            '~errors are ignored
            Return dxfGlobals.Fonts.LoadShapeFonts(aFolder, aOverrideExisting, True)
        End Function
        Public Shared Function Initialize() As Integer
            '^loads the default fonts
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Return fnts.Count
        End Function
        Public Shared Function LoadTrueTypeFont(aFontFamilyName As String, Optional bReplaceExisting As Boolean = False) As Boolean
            Dim rIndex As Integer = 0
            Dim rErrorString As String = String.Empty
            Return LoadTrueTypeFont(aFontFamilyName, bReplaceExisting, rIndex, rErrorString)
        End Function
        Public Shared Function LoadTrueTypeFont(aFontFamilyName As String, bReplaceExisting As Boolean, ByRef rIndex As Integer, ByRef rErrorString As String) As Boolean
            '^loads the passed true type font if if can be validated
            Dim fnts As TFONTS = dxfGlobals.Fonts
            Dim _rVal As Boolean = fnts.LoadTrueTypeFont(aFontFamilyName, bReplaceExisting, rIndex, rErrorString)
            dxfGlobals.Fonts = fnts
            Return _rVal
        End Function
        Public Shared Function SelectFont(Optional aInitFont As String = "", Optional aInitStyle As String = "", Optional bNoShapes As Boolean = False, Optional bNoTrueType As Boolean = False, Optional bNoStyles As Boolean = False, Optional rCanceled As Boolean? = Nothing, Optional aOwnerForm As IWin32Window = Nothing) As String
            Return goFontForm.SelectFont(aInitFont, aInitStyle, bNoShapes, bNoTrueType, bNoStyles, rCanceled, aOwnerForm)
        End Function
#End Region 'Methods
    End Class 'dxoFonts
End Namespace
