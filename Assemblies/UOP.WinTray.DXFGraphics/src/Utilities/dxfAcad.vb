
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfAcad
#Region "Constants"

#End Region 'Constants
#Region "Members"

#End Region 'Members
#Region "Properties"

#End Region 'Properties
#Region "Methods"
        Friend Shared Function OpenInACAD(aImage As dxfImage, aFileType As dxxFileTypes, aFileName As String, aCallingForm As Long, bSuppressGUI As Boolean, bShowWorking As Boolean, bAllowVersionChange As Boolean, aAutoCADtoUse As String, aAttributes As dxfAttributes, aBlockNames As String, aSuppressAttributes As Boolean, ByRef rErrorString As String, ByRef rErrNum As Long, ByRef rCanceled As Boolean) As Boolean
            Dim _rVal As Boolean
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            '       '#1the subject image
            '       '#1the file type to write (DWG or DWF)
            '       '#2a reference to the calling form
            '       '#3the file name to write to
            '       '#4flag to show or bypass the Working screen shown while the DXF file is written and openened in AutoCAD
            '       '#5flag to control if the client can change the DXF file version
            '       '#6optional string to tell which AutoCAD version to start
            '       '#7a collection of attributes
            '       '^used to open a DXF File in an AutoCAD session
            '       '~an Error is raised if the host system doesn't have AutoCAD installed
            '       rCanceled = False
            'rErrorString = ""
            'rErrNum = 0
            'If aimage Is Nothing Then Return _rVal
            'On Error GoT2 Err:
            'Dim frm As frmToAutoCad
            'Dim Errs As String
            ''**UNUSED VAR** Dim ErrD As String
            'Dim bRefreshit As Boolean
            'Dim i As Integer
            'Dim aAtt As New TPROPERTY
            ''**UNUSED VAR** Dim j As Long
            'Dim aExt As String
            'Dim aHdr As New TTABLEENTRY
            'If Not goACAD.UserHasAutoCAD Then Throw New Exception("AutoCAD was not found on the host sytem"))
            'If aFileType <> dxxFileTypes.DWG And aFileType <> dxxFileTypes.DXF Then aFileType = aImage.FileType
            'If Not goACAD.ConverterPresent Then aFileType = dxxFileTypes.DXF
            'aExt = img_FileExtension(aFileType)
            'aHdr = aImage.SET_HEADER
            'If aFileName = "" Then aFileName = img_OutputFileName(aImage, aFileType) Else aImage.FileName = aFileName
            'aFileName = img_OutputFileName(aImage, aFileType)
            'If bSuppressGUI Then
            '    If aFileName = "" Then
            '        aFileName = dxfUtils.TempPath
            '        If aFileName = "" Then Throw New Exception("The passed DXF File has no File Name"))
            '        If Right(aFileName, 1) <> "\" Then aFileName += "\"
            '        aFileName += gsTemDXFFileName$ & "." & aExt
            '    End If
            '    If aAttributes IsNot Nothing Then
            '        For i = 1 To aAttributes.Count
            '            aAtt = aAttributes.Member(i)
            '            dxfUtils.SetAttributeValue(aImage, aAtt.Name, aAtt.Value)
            '        Next i
            '    End If
            '    acad_WriteAndOpenFile(aImage, aAutoCADtoUse, aCallingForm, aFileType = dxxFileTypes.DWG, aFileName, rErrNum)
            'Else
            '    frm = New frmToAutoCad
            '        frm.OpenDrawing(aImage, aCallingForm, aAttributes, bRefreshit, aBlockNames, bAllowVersionChange, Not bShowWorking, aAutoCADtoUse, aSuppressAttributes, rErrNum, rCanceled)
            '        If frm.ErrString <> "" Then Throw New Exception( .ErrString)
            '    If bRefreshit Then aImage.Render
            '    _rVal = bRefreshit
            'End If
            Return _rVal
Err:
            'Errs = "img_OpenInACAD." & ex.Message
            'If rErrNum <= 0 Then rErrNum = 1000
            'rErrorString = Errs
            'Return _rVal
        End Function


        Friend Shared Function OptionHasFont(aOption As TACADOPTION, aFontName As String, ByRef rPath As String, Optional bTransferFonts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            rPath = ""
            Dim pth As String
            Dim fname As String

            fname = aFontName.Trim
            If fname = "" Then Return _rVal
            pth = aOption.Path
            If pth = "" Then Return _rVal
            pth = IO.Path.GetDirectoryName(pth)
            If Not IO.Directory.Exists(pth) Then Return _rVal

            If fname.Contains("\") Then
                fname = dxfUtils.RightOf(fname, "\", bFromEnd:=True)
            End If

            If fname.Contains(".") Then
                fname = dxfUtils.LeftOf(fname, ".", bFromEnd:=True)
            End If
            If fname = "" Then Return _rVal
            fname += ".shx"
            If IO.File.Exists($"{pth }\{ fname}") Then
                _rVal = True
                rPath = pth & "\" & fname
                Return _rVal
            End If
            If IO.Directory.Exists($"{pth }\Fonts") Then
                If IO.File.Exists(pth & "\Fonts\" & fname) Then
                    _rVal = True
                    rPath = pth & "\Fonts\" & fname
                    Return _rVal
                Else
                    If aFontName.Contains("\") And bTransferFonts Then
                        If IO.File.Exists(aFontName) Then
                            IO.File.Copy(aFontName, $"{pth}\Fonts\{ fname}", False)
                            If IO.File.Exists($"{pth}\Fonts\{ fname}") Then
                                _rVal = True
                                rPath = $"{pth}\Fonts\{ fname}"
                                Return _rVal
                            End If
                        End If
                    End If
                End If
            End If
            If IO.Directory.Exists($"{pth}\Support\") Then
                If IO.File.Exists($"{pth}\Support\{ fname}") Then
                    _rVal = True
                    rPath = $"{pth}\Support\{ fname}"
                    Return _rVal
                End If
            End If
            Return _rVal
        End Function
        Friend Shared Function OptionYearName(aOption As TACADOPTION) As String
            Dim _rVal As String = String.Empty
            If aOption.Name <> "" Then
                Dim i As Integer = aOption.Name.LastIndexOf(" ")
                If i > 0 Then
                    _rVal = aOption.Name.Substring(i + 1, aOption.Name.Length - (i + 1))
                End If
            End If
            Return _rVal
        End Function
        Friend Shared Function Sessions() As Object
            Throw New NotImplementedException()
            'Dim _rVal As New TACADSESSIONS
            '^collection of AutoCAD sessions running on the host machine
            '   'On Error Resume Next
            '
            '    Dim cadProcesses As TPROCESSES
            '    Dim aPrc As TPROCESS
            '    Dim Caption As String
            '    Dim nSes As TACADSESSION
            '    Dim aSes As TACADSESSION
            '    Dim cap As String
            '    Dim bKeep As Boolean
            '    Dim i As Integer
            '    Dim yName As String
            '    Dim pid As Long
            '    Dim prc As Integer
            '    Dim chr As String
            '    Dim tSes As TACADSESSION
            '    Dim aOpt As TACADOPTION
            '    Dim aOPTs As TACADOPTIONS
            '
            '    aOPTs = goACAD.Options
            '    acad_Sessions.Count = 0
            '    ReDim acad_Sessions.Members(0)
            '
            '
            '    cadProcesses = procs_GetByName("ACAD,ACLT", False)
            '    For prc = 0 To cadProcesses.Count - 1
            '        aPrc = cadProcesses.Members(prc)
            '
            '
            '        If aPrc.EXEPath <> "" Then
            '            If IO.File.Exists(aPrc.EXEPath) Then
            '                tSes = nSes
            '                    tSes.Index = dxfACAD.Sessions.Count + 1
            '                    tSes.hWnd = aPrc.WindowHandle
            '                    tSes.Caption = aPrc.WindowCaption
            '                    tSes.ProcessID = aPrc.ProcessID
            '                    cap = tSes.Caption
            '                
            '                    If cal.contains("-") Then cap = dxfUtils.LeftOf(cal,"-")
            '
            '                    For i = 1 To aOPTs.Count
            '                        If String.Compare(aOPTs.Members(i - 1).Path, aPrc.EXEPath, True) = 0 Then
            '                            aOpt = aOPTs.Members(i - 1)
            '                            Exit For
            '                        End If
            '                    Next i
            '
            '                    tSes.IsMechanical =  cap.Contains( "MECHANICAL", Stringcomparer.OrdinalIgnoreCase)
            '                    tSes.Version = aOpt.Version
            '                    tSes.Release = aOpt.Release
            '                    tSes.EXEPath = aOpt.Path
            '                    tSes.IsLite = aOpt.IsLite
            '                    tSes.OptionName = aOpt.Name
            '                    tSes.YearName = dxfACAD.OptionYearName(aOpt)
            '                    tSes.Process = aPrc
            '
            '                acad_Sessions.Count = dxfACAD.Sessions.Count + 1
            '                System.Array.Resize( acad_Sessions.Members,acad_Sessions.Count)
            '                acad_Sessions.Members(acad_Sessions.Count - 1) = tSes
            '
            '            End If
            '        End If
            '
            '    Next prc
            '  Return _rVal
        End Function
        Friend Shared Function TranslateError(aErrorCode As Long) As String
            Dim _rVal As String = String.Empty
            Select Case aErrorCode
                Case 0
                    Return _rVal
                Case -1
                    _rVal = "Could not open source file"
                Case -2
                    _rVal = "Could not open destination file"
                Case -3
                    _rVal = "Unlicensed DLL"
                Case -5
                    _rVal = "Wrong flag value for init_dwgconvert"
                Case -6
                    _rVal = "Parameters are blank"
                Case -7
                    _rVal = "/HELP /? not supported for DLL command params"
                Case -9
                    _rVal = "Could not locate DWGCONV.INI file(not in the application folder, inform location using init_dwgconvert) "
                Case -16
                    _rVal = "Output file did not have a .DXF or .DWG extension"
                Case -19
                    _rVal = "File does not exist in folder specified"
                Case -20
                    _rVal = "File size exceeded in trial version"
                Case -21
                    _rVal = "Identical input and output file names(not allowed) "
                Case -27
                    _rVal = "Specified output folder does not exist"
                Case -33
                    _rVal = "Wildcard pattern must have a .DXF or .DWG extension, e.g. *.DXF"
                Case -34
                    _rVal = "Source file should have a .DXF or .DWG extension."
                Case -35
                    _rVal = "Wildcard specification not supported in trial version"
                Case -36
                    _rVal = "Output folder is not writable"
                Case -53
                    _rVal = "Missing unzip32.dll(should be in the same folder as dwgconvert.dll) "
                Case -54
                    _rVal = "Missing stdfonts.csz(should be in the same folder as dwgconvert.dll) "
                Case -77
                    _rVal = "Instance of the DLL already running"
                Case -100
                    _rVal = "Error saving file - disk full?"
                Case -101
                    _rVal = "Could not make proxy"
                Case -102
                    _rVal = "Invalid Drawing"
                Case -103
                    _rVal = "Not implemented yet"
                Case -104
                    _rVal = "Not applicable"
                Case -105
                    _rVal = "Invalid input"
                Case -106
                    _rVal = "Invalid Filer"
                Case -107
                    _rVal = "Ambiguous input"
                Case -108
                    _rVal = "Ambiguous output"
                Case -109
                    _rVal = "Out of memory"
                Case -110
                    _rVal = "No interface"
                Case -111
                    _rVal = "Buffer too small"
                Case -112
                    _rVal = "Invalid open state"
                Case -113
                    _rVal = "Unsupported Method"
                Case -114
                    _rVal = "Entity in inactive layout"
                Case -115
                    _rVal = "Handle already exists"
                Case -116
                    _rVal = "Null handle"
                Case -117
                    _rVal = "Broken handle"
                Case -118
                    _rVal = "Unknown handle"
                Case -119
                    _rVal = "Handle in use"
                Case -120
                    _rVal = "Null object pointer"
                Case -121
                    _rVal = "Null object Id"
                Case -122
                    _rVal = "Null block name"
                Case -123
                    _rVal = "Container is not empty"
                Case -124
                    _rVal = "Null entity pointer"
                Case -125
                    _rVal = "Illegal entity type"
                Case -126
                    _rVal = "Key Not Found"
                Case -127
                    _rVal = "Duplicate Key"
                Case -128
                    _rVal = "Invalid index"
                Case -129
                    _rVal = "Character Not Found"
                Case -130
                    _rVal = "Duplicate index"
                Case -131
                    _rVal = "Already in database"
                Case -132
                    _rVal = "Out of disk"
                Case -133
                    _rVal = "Deleted entry"
                Case -134
                    _rVal = "Negative value not allowed"
                Case -135
                    _rVal = "Invalid Extents"
                Case -136
                    _rVal = "Invalid ads name"
                Case -137
                    _rVal = "Invalid Symbol Table Name"
                Case -138
                    _rVal = "Invalid Key"
                Case -139
                    _rVal = "Wrong Object Type"
                Case -140
                    _rVal = "Wrong Database"
                Case -141
                    _rVal = "Object to be deleted"
                Case -142
                    _rVal = "Invalid Dwg Version"
                Case -143
                    _rVal = "Anonymous entry"
                Case -144
                    _rVal = "Illegal Replacement"
                Case -145
                    _rVal = "End of oject"
                Case -146
                    _rVal = "Unexpected end of file"
                Case -147
                    _rVal = "File exists"
                Case -148
                    _rVal = "Can't open file"
                Case -149
                    _rVal = "File close error"
                Case -150
                    _rVal = "File write error"
                Case -151
                    _rVal = "No filename"
                Case -152
                    _rVal = "Filer error"
                Case -153
                    _rVal = "File access error"
                Case -154
                    _rVal = "File system error"
                Case -155
                    _rVal = "File internal error"
                Case -156
                    _rVal = "Too many open files"
                Case -157
                    _rVal = "File not found"
                Case -158
                    _rVal = "Unknown File Type"
                Case -159
                    _rVal = "Is reading"
                Case -160
                    _rVal = "Is writing"
                Case -161
                    _rVal = "Not opened for read"
                Case -162
                    _rVal = "Not opened for write"
                Case -163
                    _rVal = "Not that kind of class"
                Case -164
                    _rVal = "Invalid block name"
                Case -165
                    _rVal = "Missing dxf field"
                Case -166
                    _rVal = "Duplicate dxf field"
                Case -167
                    _rVal = "Invalid group code"
                Case -168
                    _rVal = "Invalid ResBuf"
                Case -169
                    _rVal = "Bad Dxf sequence"
                Case -170
                    _rVal = "Invalid RoundTripR14 data"
                Case -171
                    _rVal = "Polyface Mesh vertex after face"
                Case -172
                    _rVal = "Invalid vertex index"
                Case -173
                    _rVal = "Other objects busy"
                Case -174
                    _rVal = "The invoked BTR is not database-resident yet"
                Case -175
                    _rVal = "Cannot nest block definitions"
                Case -176
                    _rVal = "Dwg recovered OK"
                Case -177
                    _rVal = "Dwg not recoverable"
                Case -178
                    _rVal = "Dxf partially read"
                Case -179
                    _rVal = "Dxf read aborted"
                Case -180
                    _rVal = "Dxb partially read"
                Case -181
                    _rVal = "CRC does not match"
                Case -182
                    _rVal = "Dwg sentinel does not match"
                Case -183
                    _rVal = "Dwg object improperly read"
                Case -184
                    _rVal = "No input filer"
                Case -185
                    _rVal = "Dwg needs a full save"
                Case -186
                    _rVal = "Dxb read aborted"
                Case -187
                    _rVal = "Dwk lock file found"
                Case -188
                    _rVal = "Object was erased"
                Case -189
                    _rVal = "Object was permanently erased"
                Case -190
                    _rVal = "Was open for read"
                Case -191
                    _rVal = "Was open for write"
                Case -192
                    _rVal = "Was open for undo"
                Case -193
                    _rVal = "Was notifying"
                Case -194
                    _rVal = "Was open for notify"
                Case -195
                    _rVal = "On locked layer"
                Case -196
                    _rVal = "Must open thru owner"
                Case -197
                    _rVal = "Subentities still open"
                Case -198
                    _rVal = "At max readers"
                Case -199
                    _rVal = "Is write protected"
                Case -200
                    _rVal = "Is XRef object"
                Case -201
                    _rVal = "An object in entitiesToMove is not an entity"
                Case -202
                    _rVal = "Had multiple readers"
                Case -203
                    _rVal = "Invalid Block table record name"
                Case -204
                    _rVal = "Duplicate record name"
                Case -205
                    _rVal = "Block is not an external reference definition"
                Case -206
                    _rVal = "Empty record name"
                Case -207
                    _rVal = "Block depend on other XRefs"
                Case -208
                    _rVal = "Entity references itself"
                Case -209
                    _rVal = "Missing symbol table"
                Case -210
                    _rVal = "Missing symbol table record"
                Case -211
                    _rVal = "Was not open for write"
                Case -212
                    _rVal = "Close was notifying"
                Case -213
                    _rVal = "Close nodify aborted"
                Case -214
                    _rVal = "Close partial failure"
                Case -215
                    _rVal = "Close fail object damaged"
                Case -216
                    _rVal = "Object can't be erased"
                Case -217
                    _rVal = "Cannot be resurrected"
                Case -218
                    _rVal = "Insert after"
                Case -219
                    _rVal = "Fixed all errors"
                Case -220
                    _rVal = "Left errors unfixed"
                Case -221
                    _rVal = "Unrecoverable errors"
                Case -222
                    _rVal = "No Database"
                Case -223
                    _rVal = "XData size exceeded"
                Case -224
                    _rVal = "Cannot save hatch roundtrip data due to format limitations(they are too large) "
                Case -225
                    _rVal = "Hatch is gradient"
                Case -226
                    _rVal = "Invalid RegApp"
                Case -227
                    _rVal = "Repeat entity"
                Case -228
                    _rVal = "Record not in table"
                Case -229
                    _rVal = "Iterator done"
                Case -230
                    _rVal = "Null iterator"
                Case -231
                    _rVal = "Not in block"
                Case -232
                    _rVal = "Owner not in database"
                Case -233
                    _rVal = "Owner not open for read"
                Case -234
                    _rVal = "Owner not open for write"
                Case -235
                    _rVal = "Explode before transform"
                Case -236
                    _rVal = "Cannot transform by non-ortho matrix"
                Case -237
                    _rVal = "Cannot transform by non-uniform scaling matrix"
                Case -238
                    _rVal = "Object not in database"
                Case -239
                    _rVal = "Not current database"
                Case -240
                    _rVal = "Is an entity"
                Case -241
                    _rVal = "Cannot change properties of active viewport!"
                Case -242
                    _rVal = "No active viewport in paperspace"
                Case -243
                    _rVal = "Command was in progress"
                Case -244
                    _rVal = "General modeling failure"
                Case -245
                    _rVal = "Out of range"
                Case -246
                    _rVal = "Non coplanar geometry"
                Case -247
                    _rVal = "Degenerate geometry"
                Case -248
                    _rVal = "Invalid axis"
                Case -249
                    _rVal = "Point not on entity"
                Case -250
                    _rVal = "Singular point"
                Case -251
                    _rVal = "Invalid offset"
                Case -252
                    _rVal = "Non planar entity"
                Case -253
                    _rVal = "Can not explode entity"
                Case -254
                    _rVal = "String too long"
                Case -255
                    _rVal = "Invalid sym table flag"
                Case -256
                    _rVal = "Undefined linetype"
                Case -257
                    _rVal = "Text style is invalid"
                Case -258
                    _rVal = "Too few Linetype elements"
                Case -259
                    _rVal = "Too many Linetype elements"
                Case -260
                    _rVal = "Excessive item count"
                Case -261
                    _rVal = "Ignored Linetype redefinition"
                Case -262
                    _rVal = "Bad UCS"
                Case -263
                    _rVal = "Bad Paperspace view"
                Case -264
                    _rVal = "Some input data left unread"
                Case -265
                    _rVal = "No internal space"
                Case -266
                    _rVal = "Invalid dimension style"
                Case -267
                    _rVal = "Invalid layer"
                Case -268
                    _rVal = "Multiline style is invalid"
                Case -269
                    _rVal = "Dwg file needs recovery"
                Case -270
                    _rVal = "Recovery failed"
                Case -271
                    _rVal = "Delete entity"
                Case -272
                    _rVal = "Invalid fix"
                Case -273
                    _rVal = "Bad layer name"
                Case -274
                    _rVal = "Layer group code missing"
                Case -275
                    _rVal = "Bad color index"
                Case -276
                    _rVal = "Bad linetype name"
                Case -277
                    _rVal = "Bad linetype scale"
                Case -278
                    _rVal = "Bad visibility value"
                Case -279
                    _rVal = "Proper class separator expected"
                Case -280
                    _rVal = "Bad lineweight value"
                Case -281
                    _rVal = "Pager error"
                Case -282
                    _rVal = "Out of pager memory"
                Case -283
                    _rVal = "Pager write error"
                Case -284
                    _rVal = "Was not forwarding"
                Case -285
                    _rVal = "Invalid id map"
                Case -286
                    _rVal = "Invalid Owner Object"
                Case -287
                    _rVal = "Owner Not Set"
                Case -288
                    _rVal = "Wrong subentity type"
                Case -289
                    _rVal = "Too many vertices"
                Case -290
                    _rVal = "Too few vertices"
                Case -291
                    _rVal = "No Active Transactions"
                Case -292
                    _rVal = "Transaction Is Active"
                Case -293
                    _rVal = "Not top transaction"
                Case -294
                    _rVal = "Transaction open while command ended"
                Case -295
                    _rVal = "In process of committing"
                Case -296
                    _rVal = "Not newly created object"
                Case -297
                    _rVal = "Entity is excluded from long transaction"
                Case -298
                    _rVal = "No work set"
                Case -299
                    _rVal = "Entity already in group"
                Case -300
                    _rVal = "There is no entity with this id in group"
                Case -301
                    _rVal = "Bad Dwg File"
                Case -302
                    _rVal = "Invalid REFIID"
                Case -303
                    _rVal = "Invalid normal"
                Case -304
                    _rVal = "Invalid style"
                Case -305
                    _rVal = "Cannot restore from Acis file"
                Case -306
                    _rVal = "NLS file not available"
                Case -307
                    _rVal = "Not allowed for this proxy"
                Case -308
                    _rVal = "Not supported in dwg api"
                Case -309
                    _rVal = "Poly width lost"
                Case -310
                    _rVal = "Null Extents"
                Case -311
                    _rVal = "Explode again"
                Case -312
                    _rVal = "Bad dwg header"
                Case -313
                    _rVal = "Lock violation"
                Case -314
                    _rVal = "Lock conflict"
                Case -315
                    _rVal = "Database objects open"
                Case -316
                    _rVal = "Lock change in progress"
                Case -317
                    _rVal = "Vetoed"
                Case -318
                    _rVal = "No document"
                Case -319
                    _rVal = "Not from this document"
                Case -320
                    _rVal = "LISP active"
                Case -321
                    _rVal = "Target doc not quiescent"
                Case -322
                    _rVal = "Document switch disabled"
                Case -323
                    _rVal = "Invalid context of execution"
                Case -324
                    _rVal = "Create failed"
                Case -325
                    _rVal = "Create invalid name"
                Case -326
                    _rVal = "Seting active layout failed"
                Case -327
                    _rVal = "Does not exist"
                Case -328
                    _rVal = "Model Space layout can't be deleted"
                Case -329
                    _rVal = "Last Paper Space layout can't be deleted"
                Case -330
                    _rVal = "Unable to delete current"
                Case -331
                    _rVal = "Unable to find to delete"
                Case -332
                    _rVal = "Cannot rename non-existing"
                Case -333
                    _rVal = "Model Space layout can't be renamed"
                Case -334
                    _rVal = "Invalid layout name"
                Case -335
                    _rVal = "Layout already exists"
                Case -336
                    _rVal = "Cannot rename: the name is invalid"
                Case -337
                    _rVal = "Copy failed: object does not exist"
                Case -338
                    _rVal = "Cannot copy Model Space"
                Case -339
                    _rVal = "Copy failed"
                Case -340
                    _rVal = "Copy failed: invalid name"
                Case -341
                    _rVal = "Copy failed: such name already exists"
                Case -342
                    _rVal = "Profile does not exist"
                Case -343
                    _rVal = "Invalid profile name"
                Case -344
                    _rVal = "Profile is in use"
                Case -345
                    _rVal = "Registry access error"
                Case -346
                    _rVal = "Registry create error"
                Case -347
                    _rVal = "Bad Dxf file"
                Case -348
                    _rVal = "Unknown Dxf file format"
                Case -349
                    _rVal = "Missing Dxf section"
                Case -350
                    _rVal = "Invalid Dxf section name"
                Case -351
                    _rVal = "Not Dxf header group code"
                Case -352
                    _rVal = "Undefined Dxf group code"
                Case -353
                    _rVal = "Not initialized yet"
                Case -354
                    _rVal = "Invalid Dxf 2d point"
                Case -355
                    _rVal = "Invalid Dxf 3d point"
                Case -356
                    _rVal = "Badly nested AppData"
                Case -357
                    _rVal = "Incomplete block definition"
                Case -358
                    _rVal = "Incomplete complex object"
                Case -359
                    _rVal = "Block definition in entity section"
                Case -360
                    _rVal = "No block begin"
                Case -361
                    _rVal = "Duplicate layer name"
                Case -362
                    _rVal = "Bad plotstyle name"
                Case -363
                    _rVal = "Duplicate block name"
                Case -364
                    _rVal = "Bad plotstyle type"
                Case -365
                    _rVal = "Bad plotstyle name handle"
                Case -366
                    _rVal = "Undefined shape name"
                Case -367
                    _rVal = "Duplicate block definition"
                Case -368
                    _rVal = "Missing block name"
                Case -369
                    _rVal = "Binary data size exceeded"
                Case -370
                    _rVal = "Object is referenced"
                Case -371
                    _rVal = "Invalid thumbnail bitmap"
                Case -372
                    _rVal = "eGuidNoAddress"
                Case -373
                    _rVal = "Must be 0 to 2"
                Case -374
                    _rVal = "Must be 0 to 3"
                Case -375
                    _rVal = "Must be 0 to 4"
                Case -376
                    _rVal = "Must be 0 to 5"
                Case -377
                    _rVal = "Must be 0 to 8"
                Case -378
                    _rVal = "Must be 1 to 8"
                Case -379
                    _rVal = "Must be 1 to 15"
                Case -380
                    _rVal = "Must be positive"
                Case -381
                    _rVal = "Must be non negative"
                Case -382
                    _rVal = "Must be non zero"
                Case -383
                    _rVal = "Must be 1 to 6"
                Case -384
                    _rVal = "No plotstyle translation table"
                Case -385
                    _rVal = "Plot style is in color dependent mode"
                Case -386
                    _rVal = "Max layouts"
                Case -387
                    _rVal = "No ClassId"
                Case -388
                    _rVal = "Undo operation is not available"
                Case -389
                    _rVal = "No undo group begin"
                Case -390
                    _rVal = "Hatch is too dense - ignoring"
                Case -391
                    _rVal = "File open cancelled"
                Case -392
                    _rVal = "Not Handled"
                Case -393
                    _rVal = "Library integrity is broken"
                Case -394
                    _rVal = "Already active"
                Case -395
                    _rVal = "Already inactive"
                Case -396
                    _rVal = "Codepage not found"
                Case -397
                    _rVal = "Incorrect init file version"
                Case -398
                    _rVal = "Internal error in Freetype font library"
                Case -399
                    _rVal = "No UCS present in object"
                Case -400
                    _rVal = "Object has wrong type"
                Case -401
                    _rVal = "Protocol extension object is bad"
                Case -402
                    _rVal = "Bad name for hatch pattern"
                Case -403
                    _rVal = "Object is not transaction resident"
                Case -404
                    _rVal = "Dwg file is encrypted"
                Case -405
                    _rVal = "The password is incorrect"
                Case -406
                    _rVal = "HostApp cannot decrypt data"
                Case -407
                    _rVal = "An arithmetic overflow"
                Case -408
                    _rVal = "Paging skips the object"
                Case -409
                    _rVal = "Paging is stoped"
                Case -410
                    _rVal = "Invalid ResBuf with DimStyle data"
                Case -411
                    _rVal = "Extended error"
                Case -416
                    _rVal = "Unsupported early DWG version"
                Case -899 To -600
                    _rVal = "error saving intermediate file."
                Case -897
                    _rVal = "Error saving intermediate file - is output file open in another application(e.g. AutoCAD?) "
                Case -900
                    _rVal = "Error saving intermediate file - disk full?"
                Case -1001
                    _rVal = "Could not open HGPL source file"
                Case -1002
                    _rVal = "Could not open intermediate destination file"
                Case -1003
                    _rVal = "Unlicensed DLL"
                Case -1005
                    _rVal = "Wrong flag value for init_hpgl2cad"
                Case -1006
                    _rVal = "Parameters are blank"
                Case -1007
                    _rVal = "/HELP /? not supported for DLL command params"
                Case -1011
                    _rVal = "Source file is not an HPGL file - Unrecognized format"
                Case -1012
                    _rVal = "Source file is not an HPGL file - it's a DMP plot file"
                Case -1013
                    _rVal = "Source file is not an HPGL file - it's a CALCOMP plot file"
                Case -1014
                    _rVal = "Source file is not an HPGL file - it's a DXF file"
                Case -1015
                    _rVal = "Source file is not an HPGL file - it's a DWG file"
                Case -1016
                    _rVal = "Output file did not have a .DXF or .DWG extension"
                Case -1017
                    _rVal = "Source file is not an HPGL file - it's a Postscript file"
                Case -1018
                    _rVal = "Source file is not an HPGL file - it's a PCL(Printer Command Language) file"
                Case -1020
                    _rVal = "File size exceeded in trial version"
                Case -1021
                    _rVal = "Identical input and output file names(not allowed) "
                Case -1027
                    _rVal = "Specified output folder does not exist"
                Case -1031
                    _rVal = "Wildcard pattern must specify an extension.  e.g. *.PLT.  *.* pattern not allowed"
                Case -1032
                    _rVal = "Wildcard pattern must specify an extension.  e.g. *.PLT"
                Case -1035
                    _rVal = "Wildcard specification not supported in trial version"
                Case -1036
                    _rVal = "Output folder is not writable"
                Case -1051
                    _rVal = "Init library allocation error"
                Case -1052
                    _rVal = "Init library error - could not open initialization file(init file will have name adinitXX.dat, where 'XX ' is variable, dependant on the version of the software. This error will usually be because the file cannot be found. However it could also be the result of having an incorrect version or corrupted copy of the init file.)"
                Case -1053
                    _rVal = "Unable to create output file"
                Case -1077
                    _rVal = "Instance of the DLL already running"
                Case Else
                    If aErrorCode < 0 Then _rVal = "Unspecified Translation Error"
            End Select
            If _rVal <> "" And aErrorCode <> 0 Then
                _rVal = "(" & aErrorCode & ")" & _rVal
            End If
            Return _rVal
        End Function
        Friend Shared Function UserHasAutoCAD() As Boolean
            '^flag which tells the client if the host system has AutoCAD installed
            Return goACAD.UserHasAutoCAD
        End Function
        Friend Shared Function WriteAndOpenFile(aImage As dxfImage, aAutoCADName As String) As Object
            Return Nothing
            ' Optional aCallingForm As Long = 0, Optional aWait As Long = 0, Optional bSaveAsDWG As Boolean = False, Optional bSuppressDimOverrides As Boolean = False, Optional bActivateACAD As Boolean = True, Optional bUseDefaultIfNotFound As Boolean = True, Optional aFileName As String = "",  ByRef rACADHWND As Long = 0, Optional aGenerator As dxoFileTool = Nothing, ByRef rErrNo As Long = 0
            'Dim _rVal As Object = Nothing
            'Dim rErrString As String = String.Empty
            'Try
            '    rACADHWND = 0
            '    rErrNo = 0
            '    If aImage Is Nothing Then Throw New Exception("The Passed Image Is Undefined")
            '    Dim fname As String
            '    Dim aExt As String
            '    Dim ftype As dxxFileTypes
            '    Dim curOption As New TACADOPTION
            '    If bSaveAsDWG Then ftype = dxxFileTypes.DWG Else ftype = dxxFileTypes.DXF
            '    aExt = dxfEnums.PropertyName(ftype).ToLower
            '        goACAD.CurrentFileName = ""
            '        goACAD.SetAutoCADToUse(aAutoCADName, bUseDefaultIfNotFound, curOption)
            '        'wite the file to disc
            '        If aFileName = "" Then
            '            fname = aImage.FileName(ftype)
            '            If fname = "" Then Throw New Exception("The Passed File Has No File Name Assigned")
            '        Else
            '            fname = aFileName
            '        End If
            '        goACAD.KillOpenDXF(aCallingForm, fname, bSaveAsDWG)
            '        'write the file to disc
            '        'make sure  we write the dxo file in a version compatible with the requested autocad
            '        goACAD.CurrentFileName = fname
            '        If Not bSaveAsDWG Then ftype = dxxFileTypes.DXF Else ftype = dxxFileTypes.DWG
            '        goACAD.Status = "Writing AutoCAD " & acad_OptionYearName(curOption) & " " & UCase(aExt) & " File to Disc"
            '        fname = aImage.WriteToFile(aGenerator, ftype, fname, curOption.Version, bSuppressDimOverrides, True, rErrString)
            '        goACAD.CurrentFileName = ""
            '        If rErrString <> "" Then
            '            Throw New Exception(rErrString)
            '        Else
            '            If fname <> "" Then
            '                rACADHWND = goACAD.OpenCADFile(fname, aCallingForm, aWait)
            '                If rACADHWND <> 0 Then
            '                    goACAD.ActivateWindow(rACADHWND, False)
            '                End If
            '                '                    If bActivateACAD Then goACAD.Activate False
            '            End If
            '        End If
            'Catch ex As Exception
            '    rErrString = ex.Message
            '    If rErrNo = 0 Then rErrNo = 1000
            '    Throw New Exception("dxfAcad.acad_WriteAndOpenFile - " & rErrString)
            '    goACAD.CurrentFileName = ""
            'End Try
            'Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxfAcad
End Namespace
