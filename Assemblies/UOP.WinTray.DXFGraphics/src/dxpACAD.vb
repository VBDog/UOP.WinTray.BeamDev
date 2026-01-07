Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Friend Class dxpACAD
#Region "Members"
        Private _Struc As TACAD
#End Region 'Members
#Region "Events"
        Friend Event ACADStatusChange(aStatus As String, aFileName As String)
#End Region 'Events
#Region "Constructors"
        Public Sub New()
            _Struc = New TACAD("")
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend ReadOnly Property Versions As TACADVERSIONS
            Get
                Return _Struc.Versions
            End Get
        End Property
        Friend ReadOnly Property AutoCADName As String
            Get
                '^the name of the current AutoCAD option being used
                '~a string like  "AutoCAD LT 98" or  "AutoCAD 2000"
                If _Struc.CurrentOption.Index > 0 Then Return _Struc.CurrentOption.Name Else Return String.Empty
            End Get
        End Property
        Friend ReadOnly Property AutoCADToOpen As dxxACADVersions
            Get
                '^returns the version of autoCAD that will be started if OpenDXFFileINAutoCAD is called
                If _Struc.CurrentOption.Index > 0 Then AutoCADToOpen = _Struc.CurrentOption.Version
                Return AutoCADToOpen
            End Get
        End Property
        Friend ReadOnly Property ConverterPresent As Boolean
            Get
                Dim _rVal As Boolean = False
                Return _rVal
                Dim retVal As Long
                Try
                    '1 is to unlock
                    'retVal = init_dwgconvert(_Struc.ConverterCode, 1)
                    If (retVal = 0) Then _rVal = True
                Catch ex As Exception
                    If ex.HResult = 53 Then
                        ''On Error GoT2 0
                        If IO.File.Exists(IO.Path.Combine(Application.ExecutablePath, "dwgconvert.dll")) Then
                            '2 is to set the path
                            'retVal = init_dwgconvert(IO.Path.Combine(Application.ExecutablePath, "dwgconvert.dll"), 2)
                            _rVal = (retVal = 0)
                        End If
                    End If
                End Try
Err:
                Return _rVal
                If _rVal Then
                    '1 is to unlock
                    'retVal = init_dwgconvert(_Struc.ConverterCode, 1)
                    _rVal = (retVal = 0)
                End If
                Return _rVal
            End Get
        End Property
        Friend ReadOnly Property CurrentCADOption As TACADOPTION
            Get
                Dim aOpt As New TACADOPTION
                If _Struc.CurrentOption.Index <= 0 Then SetAutoCADToUse("", True, aOpt)
                CurrentCADOption = _Struc.CurrentOption
                Return CurrentCADOption
            End Get
        End Property
        Friend ReadOnly Property CurrentExeFolder As String
            Get
                Dim cadop As New TACADOPTION
                cadop = CurrentCADOption
                If cadop.Index > 0 Then Return IO.Path.GetDirectoryName(cadop.Path) Else Return String.Empty
            End Get
        End Property
        Friend ReadOnly Property CurrentExePath As String
            Get
                Dim cadop As New TACADOPTION
                cadop = CurrentCADOption
                If cadop.Index > 0 Then Return cadop.Path Else Return String.Empty
            End Get
        End Property
        Friend Property CurrentFileName As String
            Get
                '^the current dxf file path
                'Application.DoEvents()
                CurrentFileName = _Struc.CurrentFileName
                Return CurrentFileName
            End Get
            Set(value As String)
                '^the current dxf file path
                _Struc.CurrentFileName = value
            End Set
        End Property
        Friend ReadOnly Property FontPath As String
            Get
                '^the search path for AutoCAD SHX font files
                '~like "c:\fonts;c:\program files\AutoCAD\Fonts"
                FontPath = _Struc.FontPath
                Return FontPath
            End Get
        End Property
        Friend Property OpCanceled As Boolean
            Get
                OpCanceled = _Struc.OpCanceled
                Return OpCanceled
            End Get
            Set(value As Boolean)
                _Struc.OpCanceled = value
            End Set
        End Property
        Friend ReadOnly Property OptionNames As TVALUES
            Get
                Dim _rVals As New TVALUES(0)
                Dim i As Integer
                For i = 1 To _Struc.Options.Count
                    _rVals.Add(_Struc.Options.Members(i - 1).Name)
                Next i
                Return _rVals
            End Get
        End Property
        Friend ReadOnly Property Options As TACADOPTIONS
            Get
                Return _Struc.Options
            End Get
        End Property
        Friend Property Status As String
            Get
                '^the current dxpACAD object status string
                '~set during various operations so the client can tell whats going on at different vectors during long running requests.
                'Application.DoEvents()
                Status = _Struc.Status
                Return Status
            End Get
            Set(value As String)
                '^used internal to set the dxpACAD objects status string
                '~set during various operations so the client can tell whats going on at different vectors during long running requests.
                If _Struc.Status <> value Then
                    _Struc.Status = value
                    RaiseEvent ACADStatusChange(_Struc.Status, _Struc.CurrentFileName)
                    'Application.DoEvents()
                End If
            End Set
        End Property
        Friend ReadOnly Property UserHasAutoCAD As Boolean
            Get
                '^flag which tells the client if the host system has AutoCAD installed
                UserHasAutoCAD = (_Struc.Options.Count > 0)
                Return UserHasAutoCAD
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Sub ActivateWindow(hWnd As Integer, Optional bMaximize As Boolean = False)
            Throw New NotImplementedException()
            '#1the handle of the window to activate
            '#2flag to maximize the subject window
            '^activates and brings the requested window to the foreground
            'On Error Resume Next
            'If APIWrapper.SetForegroundWindow(hWnd) Then
            '    If bMaximize Then
            '        APIWrapper.ShowWindow(hWnd, ShowWindowCommand.SW_SHOWMAXIMIZED)
            '    End If
            '    APIWrapper.SetForegroundWindow(hWnd)
            'End If
        End Sub
        Friend Function AutoCADIsRunning() As Boolean

            Throw New NotImplementedException()
            'Dim _rVal As Boolean
            ''#1returns the running session
            ''^returns True if AutoCAD is currently running on the host system
            ''~if both a full version and a lite session is running the full version is returned
            'Dim aSessions As New TACADSESSIONS
            'Dim i As Long
            'Dim aSes As New TACADSESSION
            'Dim fSes As New TACADSESSION
            'Dim lSes As New TACADSESSION
            'aSessions = dxfAcad.Sessions()
            '_rVal = aSessions.Count > 0
            'For i = aSessions.Count To 1 Step -1
            '    aSes = aSessions.Members(i - 1)
            '    If aSes.IsLite Then
            '        If lSes.Index = 0 Then lSes = aSes
            '    Else
            '        If fSes.Index = 0 Then fSes = aSes
            '    End If
            'Next i
            'If _rVal Then
            '    If fSes.Index > 0 Then rRunningSession = fSes Else rRunningSession = lSes
            'End If
            'Return _rVal
        End Function
        Friend Function ConvertDWG_To_DXF(aDWGFileSpec As String, Optional aToVersion As dxxACADVersions = dxxACADVersions.DefaultVersion, Optional aOutputFolder As String = "", Optional bDeleteDWG As Boolean = False, Optional aFileName As String = "") As String
            Dim rInputVersion As dxxACADVersions = dxxACADVersions.UnknownVersion
            Return ConvertDWG_To_DXF(aDWGFileSpec, aToVersion, aOutputFolder, bDeleteDWG, aFileName, rInputVersion)
        End Function
        Friend Function ConvertDWG_To_DXF(aDWGFileSpec As String, aToVersion As dxxACADVersions, aOutputFolder As String, bDeleteDWG As Boolean, aFileName As String, ByRef rInputVersion As dxxACADVersions) As String
            Dim _rVal As String = String.Empty
            '#1the file path of the DWG file to convert
            '#2the version to convert the DWG file to
            '#3the folder to place the output file in
            '#4flag to delete the source file after the conversion
            '#5the filename to assign to the output file. If not passed the filename of the input file is used with the extension changed to .DXF.
            '#6returns the file version of th input file
            '^converts the input file to DXF format
            Dim retVal As Long
            Dim sParam As String
            Dim infldr As String
            rInputVersion = dxxACADVersions.UnknownVersion
            Try
                aDWGFileSpec = aDWGFileSpec.Trim
                aFileName = aFileName.Trim
                If aDWGFileSpec = "" Then Return _rVal
                If Not IO.File.Exists(aDWGFileSpec) Then
                    Throw New Exception("File Not Found '" & aDWGFileSpec & "'")
                Else
                    rInputVersion = GetCADFileVersion(aDWGFileSpec)
                    If rInputVersion = dxxACADVersions.UnknownVersion Then
                        Throw New Exception("Unknown AutoCAD Version Detected '" & aDWGFileSpec & "'")
                    End If
                End If
                If aToVersion = dxxACADVersions.DefaultVersion Then aToVersion = rInputVersion
                '1 is to unlock
                sParam = _Struc.ConverterCode
                ' retVal = init_dwgconvert(sParam, 1)
                If retVal <> 0 Then Throw New Exception(dxfAcad.TranslateError(retVal))
                '2 is to SET THE INI PATH
                sParam = Application.ExecutablePath & "\dwgConv.ini"
                If IO.File.Exists(sParam) Then
                    'retVal = init_dwgconvert(sParam, 2)
                End If
                Dim fin As String
                Dim fOUT As String
                '**UNUSED VAR** Dim i As Integer
                fin = aDWGFileSpec
                aOutputFolder = Trim(aOutputFolder)
                If aOutputFolder <> "" Then
                    If Not IO.Directory.Exists(aOutputFolder) Then aOutputFolder = ""
                End If
                infldr = IO.Path.GetDirectoryName(fin)
                If aOutputFolder = "" Then aOutputFolder = infldr
                fOUT = aOutputFolder
                If Right(fOUT, 1) <> "\" Then fOUT += "\"
                If aFileName = "" Then
                    fOUT += IO.Path.GetFileNameWithoutExtension(fin) & ".dxf"
                Else
                    fOUT += aFileName
                    If String.Compare(Right(fOUT, 4), ".dxf", True) <> 0 Then fOUT += ".dxf"
                End If
                sParam = """" & fin & """ """ & fOUT & """"
                sParam += xExplodeArguments() & xConvertVersion(aToVersion)
                sParam += " /NDEC16"
                'retVal = exec_dwgconvert(sParam, 0)
                If retVal <> 0 Then Throw New Exception(dxfAcad.TranslateError(retVal))
                If IO.File.Exists(fOUT) Then
                    _rVal = fOUT
                End If
                'On Error Resume Next
                If bDeleteDWG Then IO.File.Delete(fin)
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
        Friend Function ConvertDXF_To_DWG(aDXFFileSpec As String, Optional aToVersion As dxxACADVersions = dxxACADVersions.DefaultVersion, Optional aOutputFolder As String = "", Optional bDeleteDXF As Boolean = False, Optional aFileName As String = "") As String
            Dim rInputVersion As dxxACADVersions = dxxACADVersions.UnknownVersion
            Return ConvertDXF_To_DWG(aDXFFileSpec, aToVersion, aOutputFolder, bDeleteDXF, rInputVersion)
        End Function
        Friend Function ConvertDXF_To_DWG(aDXFFileSpec As String, aToVersion As dxxACADVersions, aOutputFolder As String, bDeleteDXF As Boolean, ByRef rInputVersion As dxxACADVersions) As String
            Dim _rVal As String = String.Empty
            '#1the file path of the DXF file to convert
            '#2the version to convert the DXF file to
            '#3the folder to place the output file in
            '#4flag to delete the source file after the conversion
            '#5the filename to assign to the output file. If not passed the filename of the input file is used with the extension changed to .DWG.
            '#6returns the file version of th input file
            '^converts the input file to DWG format
            rInputVersion = dxxACADVersions.UnknownVersion
            Dim retVal As Long
            Dim sParam As String
            Try
                aOutputFolder = Trim(aOutputFolder)
                aDXFFileSpec = Trim(aDXFFileSpec)
                If aDXFFileSpec = "" Then Return _rVal
                If Not IO.File.Exists(aDXFFileSpec) Then
                    Throw New Exception("File Not Found '" & aDXFFileSpec & "'")
                Else
                    rInputVersion = GetCADFileVersion(aDXFFileSpec)
                    If rInputVersion = dxxACADVersions.UnknownVersion Then
                        Throw New Exception("Unknown AutoCAD Version Detected '" & aDXFFileSpec & "'")
                    End If
                End If
                If aToVersion = dxxACADVersions.DefaultVersion Then aToVersion = rInputVersion
                '1 is to unlock
                sParam = _Struc.ConverterCode
                'retVal = init_dwgconvert(sParam, 1)
                If retVal <> 0 Then Throw New Exception(dxfAcad.TranslateError(retVal))
                '2 is to SET THE INI PATH
                sParam = Application.ExecutablePath & "\"
                If IO.File.Exists(sParam & "dwgConv.ini") Then
                    'retVal = init_dwgconvert(sParam, 2)
                End If
                Dim fin As String
                Dim fOUT As String
                Dim infldr As String
                fin = aDXFFileSpec
                aOutputFolder = Trim(aOutputFolder)
                If aOutputFolder <> "" Then
                    If Not IO.Directory.Exists(aOutputFolder) Then aOutputFolder = ""
                End If
                infldr = IO.Path.GetDirectoryName(fin)
                If aOutputFolder = "" Then aOutputFolder = infldr
                fOUT = aOutputFolder
                If Right(fOUT, 1) <> "\" Then fOUT += "\"
                fOUT += IO.Path.GetFileNameWithoutExtension(fin) & ".DWG"
                sParam = """" & fin & """ """ & fOUT & """"
                sParam += " /AUDIT"
                sParam += xConvertVersion(aToVersion)
                sParam += " /NDEC16"
                sParam += " /X=0"
                'retVal = exec_dwgconvert(sParam, 0)
                If retVal <> 0 Then
                    Throw New Exception(dxfAcad.TranslateError(retVal))
                End If
                'On Error Resume Next
                If IO.File.Exists(fOUT) Then _rVal = fOUT
                If IO.File.Exists(fin) Then
                    If bDeleteDXF Then IO.File.Delete(fin)
                End If
            Catch ex As Exception
                Throw New Exception("dxpACAD.ConvertDXF_To_DWG - " & ex.Message)
            End Try
            Return _rVal
        End Function
        Friend Function ConvertDXF_To_DXF(aFileSpec As String, Optional aToVersion As dxxACADVersions = dxxACADVersions.DefaultVersion, Optional aOutputFolder As String = "", Optional aFileName As String = "", Optional aSuppressExplode As Boolean = False) As String
            Dim _rVal As String = String.Empty
            Dim rInputVersion As dxxACADVersions = dxxACADVersions.UnknownVersion
            '#1the file path of the DXF file to convert
            '#2the version to convert the DXF file to
            '#3the folder to place the output file in
            '#4flag to delete the source file after the conversion
            '#5the filename to assign to the output file. If not passed the filename of the input file is used.
            '#6returns the file version of th input file
            '^converts the input file to DXF format in the requested version
            Dim retVal As Long
            Dim sParam As String
            Dim bSameFolder As Boolean
            Try
                aOutputFolder = Trim(aOutputFolder)
                aFileSpec = Trim(aFileSpec)
                rInputVersion = dxxACADVersions.UnknownVersion
                If aFileSpec = "" Then Return _rVal
                aFileName = Trim(aFileName)
                If Not IO.File.Exists(aFileSpec) Then
                    Throw New Exception("File Not Found '" & aFileSpec & "'")
                Else
                    rInputVersion = GetCADFileVersion(aFileSpec)
                    If rInputVersion = dxxACADVersions.UnknownVersion Then
                        Throw New Exception("Unknown AutoCAD Version Detected '" & aFileSpec & "'")
                    End If
                End If
                If aToVersion = dxxACADVersions.DefaultVersion Then aToVersion = rInputVersion
                '1 is to unlock
                sParam = _Struc.ConverterCode
                'retVal = init_dwgconvert(sParam, 1)
                If retVal <> 0 Then Throw New Exception(dxfAcad.TranslateError(retVal))
                '2 is to SET THE INI PATH
                sParam = Application.ExecutablePath & "\"
                If IO.File.Exists(sParam & "dwgConv.ini") Then
                    'retVal = init_dwgconvert(sParam, 2)
                End If
                Dim fin As String
                Dim fOUT As String
                Dim infldr As String
                fin = aFileSpec
                aOutputFolder = Trim(aOutputFolder)
                If aOutputFolder <> "" Then
                    If Not IO.Directory.Exists(aOutputFolder) Then aOutputFolder = ""
                End If
                infldr = IO.Path.GetDirectoryName(fin)
                If aOutputFolder = "" Then aOutputFolder = infldr
                fOUT = aOutputFolder
                If Right(fOUT, 1) <> "\" Then fOUT += "\"
                If aFileName = "" Then
                    fOUT += IO.Path.GetFileNameWithoutExtension(fin) & ".dxf"
                Else
                    fOUT += aFileName
                    If String.Compare(Right(fOUT, 4), ".dxf", True) <> 0 Then fOUT += ".dxf"
                End If
                bSameFolder = String.Compare(fin, fOUT, True) = 0
                If bSameFolder Then
                    fOUT = aOutputFolder
                    If Right(fOUT, 1) <> "\" Then fOUT += "\"
                    fOUT += $"{dxfGlobals.TemDXFFileName}.DXF"
                End If
                sParam = """" & fin & """ """ & fOUT & """"
                sParam += " /AUDIT"
                sParam += xConvertVersion(aToVersion)
                If Not aSuppressExplode Then
                    sParam += xExplodeArguments()
                Else
                    sParam += " /X=0"
                End If
                sParam += " /NDEC16"
                'retVal = exec_dwgconvert(sParam, 0)
                If retVal <> 0 Then
                    Throw New Exception(dxfAcad.TranslateError(retVal))
                End If
                If IO.File.Exists(fOUT) Then
                    If bSameFolder Then
                        IO.File.Copy(fOUT, fin, True)
                        IO.File.Delete(fOUT)
                        fOUT = fin
                    End If
                    _rVal = fOUT
                End If
            Catch ex As Exception
                Throw New Exception($"{Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
            Return _rVal
        End Function
        Friend Function FontFolders(Optional bJustOne As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            Dim aMem As New TACADOPTION
            Dim i As Integer
            Dim fspec As String
            '**UNUSED VAR** Dim fldr As Folder
            Dim aOPTs As New TACADOPTIONS
            aOPTs = _Struc.Options
            For i = aOPTs.Count To 1 Step -1
                aOPTs.Members(i - 1).Index = i
                aMem = aOPTs.Members(i - 1)
                If Not aMem.IsLite Then
                    fspec = IO.Path.GetDirectoryName(aMem.Path)
                    If IO.Directory.Exists(fspec & "\Fonts") Then
                        _rVal.Add(fspec & "\Fonts")
                        If bJustOne Then Exit For
                    End If
                End If
            Next i
            If _rVal.Count <= 0 Then
                For i = aOPTs.Count To 1 Step -1
                    aOPTs.Members(i - 1).Index = i
                    aMem = aOPTs.Members(i - 1)
                    If aMem.IsLite Then
                        fspec = IO.Path.GetDirectoryName(aMem.Path)
                        If IO.Directory.Exists(fspec) Then
                            If IO.Directory.Exists(fspec & "\Fonts") Then
                                _rVal.Add(fspec & "\Fonts")
                                If bJustOne Then Exit For
                            Else
                                _rVal.Add(fspec)
                                If bJustOne Then Exit For
                            End If
                        End If
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Friend Function GetCADFileVersion(aFileSpec As String) As dxxACADVersions
            Dim rFileTypeName As String = String.Empty
            Return GetCADFileVersion(aFileSpec, rFileTypeName)
        End Function
        Friend Function GetCADFileVersion(aFileSpec As String, ByRef rFileTypeName As String) As dxxACADVersions
            Dim _rVal As dxxACADVersions
            Dim aStream As IO.StreamReader = Nothing
            Dim sErr As String
            Dim aStr As String = String.Empty
            Dim vStr As String = String.Empty
            Dim Bytes() As Byte
            Dim aVers As New TACADVERSION
            rFileTypeName = ""
            Try
                If String.Compare(IO.Path.GetExtension(aFileSpec), ".DXF", True) = 0 Then
                    aStream = New IO.StreamReader(aFileSpec)
                    Do Until aStream.EndOfStream
                        aStr = Trim(aStream.ReadLine)
                        If String.Compare(aStr, "$ACADVER", True) = 0 Then
                            aStream.ReadLine()
                            vStr = Trim(aStream.ReadLine)
                            Exit Do
                        End If
                    Loop
                    aStream.Close()
                    aStream = Nothing
                Else
                    Using reader As New IO.BinaryReader(IO.File.Open(aFileSpec, IO.FileMode.Open))
                        Bytes = reader.ReadBytes(5)
                        reader.Close()
                    End Using
                    vStr = Chr(Bytes(0)) & Chr(Bytes(1)) & Chr(Bytes(2)) & Chr(Bytes(3)) & Chr(Bytes(4)) '& Chr(Bytes(5))
                End If
                aVers = goACAD.Versions.GetVersionByName(vStr, True)
                _rVal = aVers.Version
                rFileTypeName = aVers.Name
            Catch ex As Exception
                sErr = ex.Message
                If aStream IsNot Nothing Then aStream.Close()
                Throw New Exception($"dxpACAD.GetCADFileVersion - {sErr}")
            End Try
            Return _rVal
        End Function
        Friend Function GetOption(aName As String) As TACADOPTION
            Dim _rVal As New TACADOPTION
            Dim i As Integer
            Dim aOpt As New TACADOPTION
            If aName <> "" Then
                For i = 1 To _Struc.Options.Count
                    _Struc.Options.Members(i - 1).Index = i
                    aOpt = _Struc.Options.Members(i - 1)
                    If String.Compare(aOpt.Name, aName, True) = 0 Then
                        _rVal = aOpt
                        Exit For
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Friend Function HighestOption(bFull As Boolean) As String
            Dim rIndex As Integer = 0
            Return HighestOption(bFull, rIndex)
        End Function
        Friend Function HighestOption(bFull As Boolean, ByRef rIndex As Integer) As String
            Dim _rVal As String = String.Empty
            '^Returns the dxpACADOption object with the highest non-Lite version number.
            rIndex = -1
            Dim valu As Integer
            Dim maxname As String = String.Empty
            Dim aOpt As New TACADOPTION
            Dim i As Integer
            valu = 0
            For i = 1 To _Struc.Options.Count
                _Struc.Options.Members(i - 1).Index = i
                aOpt = _Struc.Options.Members(i - 1)
                If bFull Then
                    If Not aOpt.IsLite Then
                        If aOpt.Version >= valu Then
                            rIndex = i - 1
                            valu = aOpt.Version
                            maxname = aOpt.Name
                        End If
                    End If
                Else
                    If aOpt.IsLite Then
                        If aOpt.Version >= valu Then
                            rIndex = i - 1
                            valu = aOpt.Version
                            maxname = aOpt.Name
                        End If
                    End If
                End If
            Next i
            _rVal = maxname
            Return _rVal
        End Function

        Friend Function OpenCADFile(aFileName As String, Optional aCallingForm As Long = 0, Optional aWait As Long = 0) As Long
            Dim _rVal As Long = 0
            '#1the filname to open in AutoCAD
            '#2the calling form hWnd
            '#3time to wait in milliseconds for AutoCAD to open completely
            '^used to open the passed filename in AutoCAD
            'caller should be a form
            '**UNUSED VAR** Dim docs As Object
            '**UNUSED VAR** Dim doc As Object
            '**UNUSED VAR** Dim cmd As String
            '**UNUSED VAR** Dim scriptname As String
            '**UNUSED VAR** Dim FileWasWritten As Boolean
            '**UNUSED VAR** Dim ver As String
            Dim DWGName As String
            Dim curOption As New TACADOPTION
            '**UNUSED VAR** Dim aExt As String
            '**UNUSED VAR** Dim retVal As Long
            Dim aErr As String = String.Empty
            Try
                If Not IO.File.Exists(aFileName) Then Throw New Exception("Can't Find File) '" & aFileName & "'")
                OpCanceled = False
                'get the drawing name if we are going to save as DWG
                DWGName = IO.Path.GetDirectoryName(aFileName) & "\" & IO.Path.GetFileNameWithoutExtension(aFileName) & ".dwg"
                curOption = CurrentCADOption
                If curOption.Index <= 0 Then Throw New Exception("Can't Find AutoCAD")

                Dim bIsRunning As Boolean
                bIsRunning = AutoCADIsRunning()
                Status = "Opening " & IO.Path.GetFileNameWithoutExtension(aFileName) & " In AutoCAD"
                If Not bIsRunning Then
                    'pass the executable path to start the right version
                    aErr = dxfUtils.OpenFileInSystemApp(aCallingForm, aFileName, False, curOption.Path, True)
                Else
                    'pass just the file name to open it in the running version
                    aErr = dxfUtils.OpenFileInSystemApp(aCallingForm, aFileName, False, "", True)
                End If
                If aErr <> "" Then
                    Throw New Exception(aErr)
                Else
                    bIsRunning = AutoCADIsRunning()
                    If bIsRunning Then _rVal = 0 'aSession.hWnd
                End If
            Catch ex As Exception
                Throw New Exception("dxpACAD.OpenCADFile - " & ex.Message)
            End Try
            Return _rVal
        End Function
        Friend Function SearchPaths(Optional bIncludeFontFolders As Boolean = False) As TVALUES
            Dim _rVal As New TVALUES
            _rVal = New TVALUES
            Dim aMem As New TACADOPTION
            Dim i As Integer
            Dim fspec As String
            '**UNUSED VAR** Dim fldr As Folder
            Dim aOPTs As New TACADOPTIONS
            aOPTs = _Struc.Options
            For i = aOPTs.Count To 1 Step -1
                aOPTs.Members(i - 1).Index = i
                aMem = aOPTs.Members(i - 1)
                If Not aMem.IsLite Then
                    fspec = IO.Path.GetDirectoryName(aMem.Path)
                    If IO.Directory.Exists(fspec) Then
                        _rVal.Add(fspec)
                        If IO.Directory.Exists(fspec & "\Support") Then
                            _rVal.Add(fspec & "\Support")
                        End If
                        If bIncludeFontFolders Then
                            If IO.Directory.Exists(fspec & "\Fonts") Then
                                _rVal.Add(fspec & "\Fonts")
                            End If
                        End If
                    End If
                End If
            Next i
            If _rVal.Count <= 0 Then
                For i = aOPTs.Count To 1 Step -1
                    aMem = aOPTs.Members(i - 1)
                    If aMem.IsLite Then
                        fspec = IO.Path.GetDirectoryName(aMem.Path)
                        If IO.Directory.Exists(fspec) Then
                            _rVal.Add(fspec)
                            If IO.Directory.Exists(fspec & "\Support") Then
                                _rVal.Add(fspec & "\Support")
                            End If
                            If bIncludeFontFolders Then
                                If IO.Directory.Exists(fspec & "\Fonts") Then
                                    _rVal.Add(fspec & "\Fonts")
                                End If
                            End If
                        End If
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Friend Sub SetAutoCADToUse(aAutoCADName As String, bUseDefaultIfNotFound As Boolean, ByRef rOption As TACADOPTION)
            '#1the AutoCAD option name to use (i.e. "AutoCAD LT 2000")
            '#2a flag to allow or prevent the function from using the default cad option if the passed name isn't found
            '^used by clients to set which cad option to use.
            '~client can display the available AutoCAD options by accessing the Options collection of this class
            Dim cadname As String
            Dim aOpt As New TACADOPTION
            Try
                cadname = Trim(aAutoCADName)
                rOption = aOpt
                If cadname <> "" Then
                    aOpt = GetOption(cadname)
                    If aOpt.Index <= 0 Then cadname = ""
                End If
                'get the requested option
                If cadname = "" Then
                    aOpt = GetOption(dxfUtils.ReadINI_String("", "ACAD", "CurrentACAD"))
                    If aOpt.Index > 0 Then
                        cadname = aOpt.Name
                    Else
                        cadname = ""
                    End If
                End If
                dxfUtils.WriteINIString("", "ACAD", "CurrentACAD", cadname)
                If cadname = "" Then
                    If bUseDefaultIfNotFound Then
                        cadname = HighestOption(True)
                        If cadname = "" Then cadname = HighestOption(False)
                    End If
                End If
                _Struc.CurrentOption = GetOption(cadname)
                rOption = _Struc.CurrentOption
                If _Struc.CurrentOption.Index <= 0 Then Throw New Exception(cadname & " Not Found")
                If Not IO.File.Exists(_Struc.CurrentOption.Path) Then Throw New Exception(_Struc.CurrentOption.Path & " Not Found")
                _Struc.AcadPath = _Struc.CurrentOption.Path
                _Struc.AutoCADName = _Struc.CurrentOption.Name
            Catch ex As Exception
                Throw New Exception("dxpACAD.SetAutoCADToUse - " & ex.Message)
            End Try
        End Sub
        Private Function StartAutoCADSession(aCallingForm As IntPtr, aAutoCADName As String, bUseDefaultIfNotFound As Boolean) As Object
            Throw New NotImplementedException()
            'Dim _rVal As New TACADSESSION
            ''start it or get it
            'Dim aOpt As New TACADOPTION
            'SetAutoCADToUse(aAutoCADName, bUseDefaultIfNotFound, aOpt)
            'If aOpt.Index <= 0 Then Throw New Exception("Can't Find AutoCAD")
            'Dim aErr As String = String.Empty
            'Dim aSessions As New TACADSESSIONS
            'aSessions = dxfAcad.Sessions()
            'If Not _Struc.CurrentOption.IsLite Then
            '    aErr = dxfUtils.OpenFileInSystemApp(aCallingForm, "", False, _Struc.CurrentOption.Path, True)
            '    'ShellExecute aCallingForm, "Open", tstruc.CurrentOption.Path, "/nologo", "C:\", SW_SHOWNORMAL
            'Else
            '    aErr = dxfUtils.OpenFileInSystemApp(aCallingForm, "", False, _Struc.CurrentOption.Path, True)
            '    'ShellExecute aCallingForm, "Open", tstruc.CurrentOption.Path, "/nologo", "C:\", SW_SHOWNORMAL
            'End If
            'APIWrapper.Sleep(2000)
            'If AutoCADIsRunning(_rVal) Then _rVal = aSessions.Members(aSessions.Count - 1)
            'ActivateWindow(aCallingForm, False)
            'If aErr <> "" Then Throw New Exception(aErr)
            'Return _rVal
        End Function
        Private Function xConvertVersion(aVersion As dxxACADVersions) As String
            Dim _rVal As String = String.Empty
            Select Case aVersion
                Case dxxACADVersions.R9
                    _rVal = " /9"
                Case dxxACADVersions.R10
                    _rVal = " /10"
                Case dxxACADVersions.R11
                    _rVal = " /12"
                Case dxxACADVersions.R13
                    _rVal = " /13"
                Case dxxACADVersions.R14
                    _rVal = " /14"
                Case dxxACADVersions.R2000
                    _rVal = " /2000"
                Case dxxACADVersions.R2004
                    _rVal = " /2004"
                Case dxxACADVersions.R2007
                    _rVal = " /2007"
                Case Is > dxxACADVersions.R2007
                    _rVal = " /2007"
                Case Else
                    _rVal = " /2000"
            End Select
            Return _rVal
        End Function
        Private Function xExplodeArguments() As String
            Dim _rVal As String = String.Empty
            'explode splines
            _rVal += " /X=128 /XSR=15"
            'explode hatches
            _rVal += " /X=512"
            'explode tables
            _rVal += " /X=1024"
            'explode mlines
            _rVal += " /X=8192"
            'explode 2D/3d polylines to lwpolylines
            _rVal += " /X=16384"
            Return _rVal
        End Function
        Private Sub xGetCADOptions()
            '^searches the registry for AutoCAD install information and populates the CADOptions collection.
            '~executed when the class is initialized.

            _Struc.Options = New TACADOPTIONS(0)
            Dim SubKey As String ' name of the subkey to open
            Dim root As mzRegistryRoots
            Dim hasFull As Boolean
            Dim HasLite As Boolean
            Dim nOpt As New TACADOPTION
            Dim CADOption As New TACADOPTION
            Dim spath As String
            Dim fpath As String
            Dim cadKeys As ArrayList
            Dim found As Boolean
            Dim i As Integer
            Dim subKeys As ArrayList
            Dim j As Integer
            Dim reles As String
            Dim rName As String
            Dim cnt As Integer
            Dim keyFull As String
            Dim keyLite As String
            Dim b64 As Boolean = False
            'On Error Resume Next
            root = mzRegistryRoots.HKEY_LOCAL_MACHINE
            keyFull = "Software\AutoDesk\AutoCAD"
            keyLite = "Software\AutoDesk\AutoCAD LT"
            SubKey = keyFull
            _Struc.FontPath = ""
            cnt = 0
            'first look for the full versions
            cadKeys = dxfUtils.Registry_GetSubKeys(root, SubKey, found, b64)
            hasFull = found
            If hasFull Then
                'loop on the versions found
                For i = 1 To cadKeys.Count
                    SubKey = "Software\AutoDesk\AutoCAD\" & cadKeys.Item(i - 1).ToString
                    subKeys = dxfUtils.Registry_GetSubKeys(root, SubKey, found, b64)
                    For j = 1 To subKeys.Count
                        If String.Compare(Left(subKeys.Item(j - 1).ToString, 5), "ACAD-", True) = 0 Then
                            CADOption = nOpt
                            SubKey += "\" & subKeys.Item(j - 1).ToString
                            fpath = dxfUtils.Registry_ReadKey(root, SubKey, "AcadLocation") & "\acad.exe"
                            'save the option if th executable is found
                            If IO.File.Exists(fpath) Then
                                reles = cadKeys.Item(i - 1).ToString 'Registry_ReadKey(Root, Subkey, "RELEASE")
                                rName = dxfUtils.Registry_ReadKey(root, SubKey, "ProductName")
                                spath = dxfUtils.Registry_ReadKey(root, SubKey, "ACAD")
                                CADOption.SubKey = SubKey
                                CADOption.Path = fpath
                                CADOption.SearchPath = spath
                                CADOption.IsLite = False
                                CADOption.Name = rName
                                If TVALUES.IsNumber(Right(reles, reles.Length - 1)) Then
                                    CADOption.Release = TVALUES.To_DBL(Right(reles, reles.Length - 1))
                                End If
                                Select Case dxfAcad.OptionYearName(CADOption)
                                    Case "2000", "2002"
                                        CADOption.Version = dxxACADVersions.R2000
                                    Case "2004", "2005", "2006"
                                        CADOption.Version = dxxACADVersions.R2004
                                    Case "2007", "2008", "2009"
                                        CADOption.Version = dxxACADVersions.R2007
                                    Case "2010", "2011"
                                        CADOption.Version = dxxACADVersions.R2010
                                End Select
                                CADOption.Index = _Struc.Options.Count + 1
                                If _Struc.FontPath = "" Then _Struc.FontPath = spath
                                _Struc.Options.Add(CADOption)
                            End If
                        End If
                    Next j
                Next i
            End If
            cnt = _Struc.Options.Count
            If cnt = 0 Then hasFull = False
            SubKey = keyLite
            Dim unsupported As Boolean = False
            'first look for the full versions
            cadKeys = dxfUtils.Registry_GetSubKeys(root, SubKey, found, b64)
            HasLite = found
            If HasLite Then
                'loop on the versions found
                For i = 1 To cadKeys.Count
                    SubKey = keyLite & "\" & cadKeys.Item(i - 1).ToString
                    subKeys = dxfUtils.Registry_GetSubKeys(root, SubKey, found, b64)
                    For j = 1 To subKeys.Count
                        If String.Compare(Left(subKeys.Item(j - 1).ToString, 5), "ACLT-", True) = 0 Then
                            CADOption = nOpt
                            SubKey += "\" & subKeys.Item(j - 1).ToString
                            If String.Compare(cadKeys.Item(i - 1).ToString, "R5.0", True) = 0 Then
                                'R14
                                fpath = dxfUtils.Registry_ReadKey(root, SubKey, "AcltLocation") & "\aclt.exe"
                            Else
                                fpath = dxfUtils.Registry_ReadKey(root, SubKey, "Location") & "\aclt.exe"
                            End If
                            'save the option if th executable is found
                            If IO.File.Exists(fpath) Then
                                If String.Compare(cadKeys.Item(i - 1).ToString, "R5.0", True) = 0 Then
                                    reles = "R14"
                                Else
                                    reles = dxfUtils.Registry_ReadKey(root, SubKey, "RELEASE")
                                End If
                                rName = dxfUtils.Registry_ReadKey(root, SubKey, "ProductName")
                                spath = dxfUtils.Registry_ReadKey(root, SubKey, "ACLT")
                                CADOption.Path = fpath
                                CADOption.SearchPath = spath
                                CADOption.IsLite = True
                                CADOption.Name = rName
                                If reles.Contains(".") Then
                                    reles = dxfUtils.RightOf(reles, ".")

                                End If
                                Select Case reles.ToUpper
                                    Case "R9"
                                        CADOption.Version = dxxACADVersions.R14
                                    Case "R14"
                                        CADOption.Version = dxxACADVersions.R14
                                    Case "R2000"
                                        CADOption.Version = dxxACADVersions.R2000
                                    Case Else
                                        unsupported = True
                                End Select
                                If Not unsupported Then
                                    If _Struc.FontPath = "" Then _Struc.FontPath = spath
                                    CADOption.Index = _Struc.Options.Count + 1
                                    _Struc.Options.Add(CADOption)
                                End If
                            End If
                        End If
                    Next j
                Next i
            End If
            If _Struc.Options.Count = cnt Then HasLite = False
            If HasLite And hasFull Then _Struc.HasLiteAndFullVersion = True
        End Sub
#End Region 'Methods
    End Class 'dxpACAD
End Namespace
