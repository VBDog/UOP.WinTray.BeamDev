
Imports UOP.DXFGraphics.dxfGlobals
Imports System.IO
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics

    Public Class dxoFileTool
#Region "Members"
        Private Enum BufferMode
            Idle = 0
            Reading = 1
            Writing = 2
        End Enum
        Private Shared _ErrorString As String = String.Empty
        Private _ImageObj As dxfImage
        Private _Stream_Out As IO.StreamWriter

        Private _SkipList As New TLIST(".")
        Private _PathBuilder As TLIST
        Private _BufferBlocks As colDXFBlocks
        Private _BufferEnts As colDXFEntities
        'Private _Stream_Log As TextStream
        Private _BlockMatrices As System.Collections.Generic.Dictionary(Of String, TPROPERTYMATRIXARRAY)
        Private _EntityMatrices As List(Of dxoPropertyMatrix)
        Private _Image As TIMAGE
        Private _HandleGenerator As dxoHandleGenerator
        Private _Buffer As TBUFFER
        Private _Handles As TPROPERTIES
        Private _HandleProps As System.Collections.Generic.Dictionary(Of String, TPROPERTY)
        Private _PointerProps As List(Of TPROPERTY)
        Private _BufferGroups As TOBJECTS
        Private _lastProp As TPROPERTY
        Private _iGUID As String = String.Empty
        Private _FileType As dxxFileTypes
        Private _AcadVersion As dxxACADVersions
        Private _IncludedSuppressed As Boolean
        Private _IncludedHidden As Boolean
        Private _Mode As BufferMode = BufferMode.Idle
        Private _OutputToFile As Boolean
        Private _SuppressInstances As Boolean
        Private _Invisible As Boolean
        Private _SuppressDXTNames As Boolean
        Private _NumericHandles As Boolean
        Private _ModelSpaceHandle As String = String.Empty
        Private _PaperSpaceHandle As String = String.Empty
        Private _ReactorHandle As String = String.Empty
        Private _PostScript As String = String.Empty
        Private _ByBlockLTHandle As String = String.Empty

        Private _iInstance As Integer
        Private _iProperty As Integer
        Private _iPropIdx As Integer
        Private _SectionName As String = String.Empty
        Private _TableName As String = String.Empty
        Private _BlockName As String = String.Empty
        Private _Object As String = String.Empty
        Private _NullObjects As String = String.Empty
        Private _NullRefs As String = String.Empty
        Private _ThrowHandleErrors As Boolean
        Private _CancelBuffer As Boolean
        Private _Buffering As Boolean
        Private _IDE As Boolean
        Private _PropIndex As Long
        Private _PropLineNo As Long
        Private _TableCount As Integer = 0
        Private WithEvents _File As dxfFile
        Private _ErrorList As List(Of Exception)
        Private _ShowHiddenObjects As Boolean
        Private _LastProperty As TPROPERTY
#End Region 'Members
#Region "Events"
        Public Event PropertySaved(aProperty As dxoProperty, aInstance As Integer, aPostScript As String, aParentPath As String, bSuppressed As Boolean, bHidden As Boolean, bInvisble As Boolean)
        Public Event StatusChange(aStatus As String)
        Public Event HandleError(aErrorString As String, aErrorType As String, aProperty As dxoProperty, bProperty As dxoProperty)
        Public Event BufferTerminated()
        Public Event PathChangeEvent(e As dxoFileHandlerEventArg)
#End Region 'Events
#Region "Constructors"
        Public Sub New()
            _PathBuilder = New TLIST(PathDelimitor)
            _SkipList = New TLIST(",")
            _ModelSpaceHandle = "0"
            _PaperSpaceHandle = "0"
            _FileType = dxxFileTypes.DXF
            _Mode = BufferMode.Idle
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend Property AcadVersion As dxxACADVersions
            '^the autoCAD version to generate DXF for
            Get
                Return _AcadVersion
            End Get
            Set(value As dxxACADVersions)
                _AcadVersion = value
            End Set
        End Property

        Friend ReadOnly Property CurrentBuffer
            Get
                Return _Buffer
            End Get
        End Property

        Friend ReadOnly Property Buffering As Boolean
            Get
                Return _Buffering
            End Get
        End Property
        Public Property CancelBuffer As Boolean
            Get
                Return _CancelBuffer
            End Get
            Set(value As Boolean)
                If _CancelBuffer <> value Then
                    _CancelBuffer = value
                    If value Then
                        If _Buffering Then
                            Status = "Buffer Canceled"
                            RaiseEvent BufferTerminated()
                        End If
                    End If
                End If
            End Set
        End Property
        Friend Property ImageStructure As TIMAGE
            Get
                Return _Image
            End Get
            Set(value As TIMAGE)
                _Image = value
            End Set
        End Property
        Friend ReadOnly Property ObjectTypeName As String
            Get
                Return _Object
            End Get
        End Property
        Public ReadOnly Property PathDelimitor As String
            Get
                Return dxfGlobals.Delim
            End Get
        End Property
        Friend ReadOnly Property PathBuilder As TLIST
            Get
                Return _PathBuilder
            End Get
        End Property
        Friend Property SectionName As String
            Get
                Return _SectionName
            End Get
            Set(value As String)
                value = value.ToUpper().Trim()
                If value = _SectionName Then Return
                Dim pathWas As String = String.Empty
                Dim pChange As Boolean = False
                Dim newPath As String
                _iProperty = 0
                _Object = ""
                _TableName = ""
                _PropIndex = 0
                If value = String.Empty And _SectionName <> "" Then value = "ENDSEC"
                newPath = _PathBuilder.ReduceTo(1, pathWas, pChange)
                If pChange Then
                    RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathDecrease, dxxFileHandlerEvents.EndSection, pathWas, newPath))
                End If
                If value <> "ENDSEC" Then
                    _SectionName = value
                    pChange = _PathBuilder.Add(_SectionName)
                    If pChange Then
                        newPath = CurrentPath()
                        RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathIncrease, dxxFileHandlerEvents.BeginSection, pathWas, newPath))
                        SaveProperty(0, "SECTION", dxxPropertyTypes.dxf_String, aPS:=_SectionName, aName:=_SectionName)
                        If _SectionName <> "SETTINGS" Then SaveProperty(2, value, dxxPropertyTypes.dxf_String, aName:="SectionName")
                    End If
                ElseIf value = "ENDSEC" Then
                    _SectionName = ""
                    SaveProperty("0", "ENDSEC", dxxPropertyTypes.dxf_String)
                End If
            End Set
        End Property

        Private _Status As String = String.Empty
        Public Property Status As String
            Get
                Return _Status
            End Get
            Set(value As String)
                If value <> _Status Then
                    _Status = value
                    RaiseEvent StatusChange(_Status)
                    'Application.DoEvents()
                End If
            End Set
        End Property
        Friend Property BlockName As String
            Get
                Return _BlockName
            End Get
            Set(value As String)
                value = value.Trim
                If value = _BlockName Then Return
                Dim pathWas As String = CurrentPath()
                Dim pChange As Boolean
                Dim newPath As String
                _iProperty = 0
                _Object = ""
                _SectionName = "BLOCKS"
                'If value = String.Empty And _BlockName <> "" Then value = "ENDBLK"
                newPath = _PathBuilder.ReduceTo(_SectionName, pathWas, pChange)
                If pChange Then
                    RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathDecrease, dxxFileHandlerEvents.EndTable, pathWas, newPath))
                End If
                If value <> "" Then
                    _BlockName = value
                    pChange = _PathBuilder.Add(_BlockName)
                    If pChange Then
                        newPath = CurrentPath()
                        RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathIncrease, dxxFileHandlerEvents.BeginBlock, pathWas, newPath))
                        SaveProperty(0, "BLOCK", dxxPropertyTypes.dxf_String, aName:=_BlockName, aPS:=_BlockName)
                    End If
                Else
                    _BlockName = ""
                    SaveProperty("0", "END BLOCK", dxxPropertyTypes.dxf_String, aName:="END BLOCK", bNonDXF:=True)
                End If
            End Set
        End Property
        Friend Property TableName As String
            Get
                Return _TableName
            End Get
            Set(value As String)
                value = Trim(value.ToUpper)
                If value = _TableName Then Return
                Dim pathWas As String = CurrentPath()
                Dim pChange As Boolean = False
                Dim newPath As String
                _iProperty = 0
                _Object = ""
                _SectionName = "TABLES"
                If value = String.Empty And _TableName <> "" Then value = "ENDTAB"
                newPath = _PathBuilder.ReduceTo(_SectionName, pathWas, pChange, Nothing)
                If pChange Then
                    RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathDecrease, dxxFileHandlerEvents.EndTable, pathWas, newPath))
                End If
                If value <> "ENDTAB" Then
                    _TableName = value
                    pChange = _PathBuilder.Add(_TableName)
                    If pChange Then
                        newPath = _PathBuilder.StringValue
                        RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathIncrease, dxxFileHandlerEvents.BeginTable, pathWas, newPath))
                        SaveProperty(0, "TABLE", dxxPropertyTypes.dxf_String, aName:=_TableName, aPS:=_TableName)
                    End If
                Else
                    _TableName = ""
                    'newPath = PathBuilder.ReduceTo(_SectionName, pathWas, pChange)
                    'If pChange Then
                    '    newPath = _PathBuilder.StringValue
                    '    RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathDecrease, dxxFileHandlerEvents.EndTable, pathWas, newPath))
                    SaveProperty("0", "ENDTAB", dxxPropertyTypes.dxf_String, aName:="EndTable")
                    'End If
                End If
            End Set
        End Property
        Public ReadOnly Property Writing As Boolean
            Get
                Return _Mode = BufferMode.Writing
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function CurrentPath(Optional aAdder As String = "") As String

            Dim _rVal As String = _PathBuilder.StringValue
            If aAdder = "" Then Return _rVal
            If _rVal <> "" Then _rVal += PathDelimitor
            _rVal += aAdder
            Return _rVal
        End Function
        Private Sub SaveException(aException As Exception)
            If aException Is Nothing Or _ErrorList Is Nothing Then Return
            Dim msg As String = Status
            If Not String.IsNullOrEmpty(msg) Then msg += "."
            msg += aException.Message
            Dim ex As New Exception(msg) With {.Source = "dxoFileTool"}
            _ErrorList.Add(ex)
        End Sub
        Friend Function Buffer_CREATE(aImage As dxfImage, bIncludeSup As Boolean, bIncludeHidn As Boolean,
                                      bSuppressDimOverrides As Boolean, aVersion As dxxACADVersions, Optional bShowHiddenObjects As Boolean = False,
                                      Optional aSuppressList As String = "", Optional aThrowHandleErrors As Boolean = False,
                                      Optional aSettingsString As String = "", Optional bForFileOutput As Boolean = False) As TBUFFER
            Dim _rVal As New TBUFFER("")
            _ErrorList = New List(Of Exception)
            If aImage Is Nothing AndAlso aImage.Disposed Then
                _ErrorList.Add(New Exception("The Passed Image Is Nothing or Is Dosposed"))
                Return _rVal
            End If
            _CancelBuffer = False
            _PathBuilder = New TLIST(PathDelimitor, aImage.GUID)
            _SkipList = New TLIST(",", aSuppressList.ToUpper)
            _ShowHiddenObjects = bShowHiddenObjects
            _Buffering = True
            _OutputToFile = bForFileOutput
            If _Stream_Out Is Nothing Then _OutputToFile = False
            Dim initBlocks As colDXFBlocks = aImage.Blocks
            Dim initStruc As TIMAGE = aImage.Strukture
            'reset the collecting object buffer structure
            _Buffer = New TBUFFER(aImage.FileName)
            _HandleProps = New System.Collections.Generic.Dictionary(Of String, TPROPERTY)
            _PointerProps = New List(Of TPROPERTY)
            If _OutputToFile Then
                _SkipList.Add("SETTINGS")
                _ShowHiddenObjects = False
            End If
            _iPropIdx = 1
            Status = "Creating Image Buffer"
            _Image = aImage.Strukture
            _IDE = dxfUtils.RunningInIDE()
            _ThrowHandleErrors = aThrowHandleErrors
            If Not dxfEnums.Validate(GetType(dxxACADVersions), aVersion, bSkipNegatives:=True) Then aVersion = aImage.FileVersion
            aImage.FileVersion = aVersion
            RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathIncrease, dxxFileHandlerEvents.BeginFile, "", aImage.GUID))
            'make sure all defaults are in place and handled
            Try
                dxfImageTool.VerifyDefaultMembers(aImage, True, True)
            Catch ex As Exception
                SaveException(ex)
            End Try
            Try
                If _CancelBuffer Then
                    _Buffering = False
                    Return _rVal
                End If
                'clear the group
                aImage.Objex.ClearGroups()
                _Image = aImage.Strukture
                initBlocks = aImage.Blocks
                _Image = aImage.Strukture
                aImage.HandleGenerator.SaveState() 'mark the end of the handles genertor so we can set it back at the end
                aImage.HandleGenerator.Verbose = True
                aImage.HandleGenerator.TempHandles = True
                initStruc = _Image
                _ImageObj = aImage
                _iGUID = aImage.GUID
                AcadVersion = aVersion
                _IncludedSuppressed = bIncludeSup
                _IncludedHidden = bIncludeHidn
                If Writing Then
                    _IncludedSuppressed = _FileType = dxxFileTypes.DXT
                End If
            Catch ex As Exception
                SaveException(ex)
            End Try
            'build the collection of objects whose properties will be written to the file
            If Not _CancelBuffer Then CreateOutputCollections(aImage)
            Dim sectionEnums As System.Collections.Generic.Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxFileSections))
            Dim section As dxxFileSections
            Dim secname As String
            For s As Integer = 1 To sectionEnums.Count
                section = sectionEnums.ElementAt(s - 1).Value
                If section <> dxxFileSections.Unknown Then
                    secname = dxfEnums.Description(section)
                    If Not _SkipList.Contains(secname) Then
                        SaveSection(section, aImage, bSuppressDimOverrides, aSettingsString)
                    End If
                    If section = dxxFileSections.Thumbnail Then
                        SaveProperty(0, "EOF", dxxPropertyTypes.dxf_String)
                        If aSettingsString = "" Then
                            Exit For
                        End If
                    End If
                    If _CancelBuffer Then Exit For
                End If
            Next s
            'reset the blocks and handles
            Try
                aImage.HandleGenerator.ReleaseTempHandles()
                aImage.HandleGenerator.SaveState(True)  'delete borrowed handles
                aImage.HandleGenerator.Verbose = False
                aImage.HandleGenerator.TempHandles = False
                aImage.SetBlocks(initBlocks)
                '  aImage.Strukture = initStruc
            Catch ex As Exception
                SaveException(ex)
            Finally
                If Not _CancelBuffer Then
                    RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathDecrease, dxxFileHandlerEvents.EndFile, aImage.GUID, ""))
                End If
                aImage.HandleGenerator.SaveState(True)  'delete borrowed handles
                initBlocks = Nothing
                _Buffering = False
                _rVal = _Buffer
                If _ThrowHandleErrors And Not _CancelBuffer Then xThrowPointerErrors()
                zTerminateObjects()
                Status = ""
            End Try
            Return _rVal
        End Function

        Public Shared ReadOnly Property ErrorString As String
            Get
                Return _ErrorString
            End Get
        End Property
        Private Sub CreateOutputCollections(aImage As dxfImage)
            Status = "Creating Output Collections"
            Try

                Dim vBlocks As colDXFBlocks
                Dim BlockEnts As New TPROPERTYMATRIXARRAY("BlockEnts")

                Dim eMatrix As dxoPropertyMatrix = Nothing
                Dim aMatrix As New TPROPERTYMATRIX("aMatrix")
                Dim bMatrix As New TPROPERTYMATRIX("bMatrix")
                Dim eBlk As dxfBlock = Nothing
                Dim aEnts As colDXFEntities

                Dim bKeep As Boolean
                Dim aProps As TPROPERTIES = TPROPERTIES.Null
                Dim brTable As TTABLE = TTABLE.Null
                Dim brEntry As TTABLEENTRY = TTABLEENTRY.Null
                Dim reactEnt As dxfEntity = Nothing
                Dim rMatrix As dxoPropertyMatrix
                Dim iblocks As colDXFBlocks = aImage.Blocks

                _BlockMatrices = New System.Collections.Generic.Dictionary(Of String, TPROPERTYMATRIXARRAY)
                _EntityMatrices = New List(Of dxoPropertyMatrix)()
                'clear the dimstyle reactors
                _Image.tbl_DIMSTYLE.ClearEntryReactors("{ACAD_REACTORS", True)
                Dim aDimStyles As dxoDimStyles = aImage.DimStyles
                _ModelSpaceHandle = iblocks.BlockRecordHandle("*Model_Space")
                _PaperSpaceHandle = iblocks.BlockRecordHandle("*Paper_Space")
                _ByBlockLTHandle = aImage.Styles.EntryHandle(dxfLinetypes.ByBlock)
                'build the working blocks collection and the record table

                vBlocks = New colDXFBlocks(aImage, "") With {.Handle = iblocks.Handle}

                aImage.HandleGenerator.AssignTo(vBlocks)
                For Each iblock In iblocks
                    vBlocks.AddToCollection(iblock, bSuppressEvnts:=True)
                    If _CancelBuffer Then Return
                Next
                'replace then images blocks temporarily
                aImage.SetBlocks(vBlocks)
                If _CancelBuffer Then Return
                'create the collection of entity property array matrices
                'that will be written to the file
                aEnts = aImage.Entities
                For Each ent In aEnts
                    'determine if the current entity should be written to file and get it's property matrix
                    If KeepEntity(aImage, ent, eMatrix, eBlk, reactEnt) Then
                        _EntityMatrices.Add(eMatrix)
                        If eBlk IsNot Nothing Then
                            If Not vBlocks.BlockExists(eBlk.Name) Then vBlocks.AddToCollection(eBlk, bSuppressEvnts:=True)
                        End If
                        'reactor entity
                        If reactEnt IsNot Nothing And eMatrix IsNot Nothing Then
                            rMatrix = reactEnt.DXFFileProperties(Nothing, aImage, eBlk, _SuppressInstances, False)
                            _EntityMatrices.Add(rMatrix)
                            If eBlk IsNot Nothing Then
                                vBlocks.AddToCollection(eBlk, bSuppressEvnts:=True)
                            End If
                        End If
                    End If
                    'Application.DoEvents()
                    If _CancelBuffer Then Return
                Next
                'create the block file properties
                For Each block In vBlocks
                    bKeep = _ShowHiddenObjects Or (Not _ShowHiddenObjects And Not block.Suppressed)
                    If bKeep Then
                        aImage.HandleGenerator.AssignTo(block)
                        'add the block record
                        If Not _Image.tbl_BLOCKRECORD.TryGet(block.Name, rEntry:=brEntry) Then

                            _Image.tbl_BLOCKRECORD.Add(New TTABLEENTRY(block.BlockRecord))
                            aImage.Tables.UpdateTable(_Image.tbl_BLOCKRECORD)
                        End If
                        block.ResetReferences()
                        BlockEnts = New TPROPERTYMATRIXARRAY(block.Name)
                        aMatrix = New TPROPERTYMATRIX(block.GUID) With {.Block = block.Strukture}
                        bMatrix = New TPROPERTYMATRIX(block.GUID) With {.Block = aMatrix.Block}
                        'add the blocks properties to the matrix
                        aProps = dxpProperties.Get_BlockProps(aBlock:=block)
                        aMatrix.Add(aProps, block.Name, "PROPERTIES")
                        BlockEnts.Add(aMatrix)
                        aEnts = block.Entities
                        For Each ent In aEnts
                            ent.Suppressed = False
                            KeepEntity(aImage, ent, eMatrix, eBlk, reactEnt)  'to get the entities properties
                            BlockEnts.Add(eMatrix.Strukture)
                            If eBlk IsNot Nothing Then
                                vBlocks.AddToCollection(eBlk, bSuppressEvnts:=True)
                            End If
                            'reactor entity
                            If reactEnt IsNot Nothing And eMatrix IsNot Nothing Then
                                rMatrix = reactEnt.DXFFileProperties(Nothing, aImage, eBlk, _SuppressInstances, False)
                                BlockEnts.Add(rMatrix.Strukture)
                                If eBlk IsNot Nothing Then
                                    vBlocks.AddToCollection(eBlk, bSuppressEvnts:=True)
                                End If
                            End If
                            'Application.DoEvents()
                            If _CancelBuffer Then Return
                        Next
                        aProps = dxpProperties.Get_EntityProps(dxxGraphicTypes.EndBlock, aGUID:="", aHandle:=block.EndBlockHandle, aOwnerHandle:=block.BlockRecordHandle, aLayerName:=block.LayerName, block.Domain = dxxDrawingDomains.Paper)
                        bMatrix.Add(aProps, block.Name, "ENDBLK")
                        BlockEnts.Add(bMatrix)
                        _BlockMatrices.Add(block.GUID, BlockEnts)
                    End If
                    'Application.DoEvents()
                    If _CancelBuffer Then Return
                Next
                If _CancelBuffer Then Return
                'update the tables that have references
                _Image = aImage.Strukture


            Catch ex As Exception
                SaveException(ex)
            Finally
                'aImage.Strukture = _Image
            End Try
        End Sub


        Private Function GroupReactors(aEntProps As TPROPERTIES, aGroupName As String, ByRef rGroupIndex As Integer, ByRef rGroupHandle As String) As TPROPERTIES
            Dim _rVal As New TPROPERTIES
            Dim rGUID As String = aEntProps.ValueStr("*ReactorHandle")
            aGroupName = aGroupName.Trim()
            If aGroupName <> "" Then dxfUtils.ValidateGroupName(aGroupName, "", True)
            Dim aProp As TPROPERTY = aEntProps.GetByGC(5, 1)
            Dim strReactors = aProp.Reactors
            Dim dxfGrp As dxfoGroup = _ImageObj.Objex.GetObject(aGroupName, dxxObjectTypes.Group)
            _rVal.Add(New TPROPERTY(102, "{ACAD_REACTORS", "Reactors Begin", dxxPropertyTypes.dxf_String))
            If rGUID <> "" And rGUID <> "0" Then
                _rVal.Add(New TPROPERTY(330, rGUID, "Reactor", dxxPropertyTypes.Pointer))
            End If
            Dim aGrp As dxfoGroup
            rGroupIndex = -1
            rGroupHandle = ""
            If aGroupName <> "" Then
                aGrp = dxfImageTool.GroupGet(_ImageObj, aGroupName, True)
                If aGrp IsNot Nothing Then
                    rGroupIndex = aGrp.Index
                    rGroupHandle = aGrp.Handle
                    If rGroupHandle <> "" Then
                        _rVal.Add(New TPROPERTY(330, rGroupHandle, "Group Reactor", dxxPropertyTypes.Pointer))
                    Else
                        rGroupIndex = -1
                    End If
                End If

            End If
            _rVal.Add(New TPROPERTY(102, "}", "Reactors End", dxxPropertyTypes.dxf_String))
            Return _rVal
        End Function

        Friend Function Image_WRITE(aFileType As dxxFileTypes, aImage As dxfImage, aFileSpec As String, bSuppressDimOverrides As Boolean, aVersion As dxxACADVersions, bNoErrors As Boolean, ByRef rErrString As String, bSuppressDXTNames As Boolean, bNumericHandles As Boolean, ByRef rErrNo As Long, Optional bNoConverter As Boolean = False) As String
            Dim _rVal As String = String.Empty
            rErrNo = 0
            rErrString = ""
            If aImage Is Nothing Then
                rErrString = "No Image Passed"
                Return String.Empty
            End If
            _ImageObj = aImage
            Dim tmpfile As String = String.Empty
            Dim outfile As String
            Dim bIsCAD As Boolean
            _CancelBuffer = False
            Try
                If Not dxfEnums.Validate(GetType(dxxACADVersions), aVersion, bSkipNegatives:=True) Then aVersion = aImage.FileVersion
                Dim ext As String = String.Empty
                aFileType = dxfUtils.ValidateFileType(aFileType, ext, bIsCAD)
                If aFileType <> dxxFileTypes.DXF Then bNoConverter = False
                If aFileType = dxxFileTypes.DWG Then
                    If Not goACAD.ConverterPresent Then Throw New Exception("DXF File Converter Not Found")
                End If
                aFileSpec = Trim(aFileSpec)
                If aFileSpec = "" Then aFileSpec = aImage.FileName(aFileType)
                If aFileSpec = "" Then Throw New Exception("Invalid File Path Detected")
                Dim fldr As String = Path.GetDirectoryName(aFileSpec)
                If fldr = "" Then Throw New Exception($"Folder Not Found '{ fldr }'")
                If Not IO.Directory.Exists(fldr) Then Throw New Exception($"Folder Not Found '{ fldr }'")

                Dim fname As String = Path.GetFileNameWithoutExtension(aFileSpec)
                If fname = "" Then Throw New Exception("Invalid File Name")
                outfile = Path.Combine(fldr, $"{fname}.{ext}")

                'open the tempory file and start the stream
                tmpfile = zStartStream(aFileType, aFileSpec)
                If tmpfile = "" Then Throw New Exception("Unable To Create Tempory Text File For Output Operations")
                Status = $"Writing File - {tmpfile}"
                _Mode = BufferMode.Writing
                _SuppressDXTNames = bSuppressDXTNames
                _NumericHandles = bNumericHandles
                'all is good so write to temporary file
                Buffer_CREATE(aImage, bIncludeSup:=False, bIncludeHidn:=False, bSuppressDimOverrides:=bSuppressDimOverrides, aVersion:=aVersion, bForFileOutput:=True)
Done:
                If Not _CancelBuffer Then
                    _Mode = BufferMode.Idle
                    aImage.FileType = aFileType
                    If aFileType = dxxFileTypes.DXF Or aFileType = dxxFileTypes.DWG Then
                        If _IDE Then
                            If IO.Directory.Exists("c:\Junk") Then
                                IO.File.Copy(tmpfile, Path.Combine("c:\Junk", "LastDXF.DXF"), True)
                            End If
                        End If
                        If aFileType = dxxFileTypes.DWG Then
                            tmpfile = goACAD.ConvertDXF_To_DWG(tmpfile, aVersion)
                        ElseIf aFileType = dxxFileTypes.DXF Then
                            'If Not bNoConverter Then
                            '    tmpfile = goACAD.ConvertDXF_To_DXF(tmpfile, aVersion, aSuppressExplode:=True)
                            'End If
                        End If
                    End If
                    'copy the temp file to the destination
                    If IO.File.Exists(tmpfile) Then
                        If Not IO.File.Exists(outfile) Then
                            Dim astrm As New IO.StreamWriter(outfile)
                            astrm.Close()
                        End If
                        IO.File.Copy(tmpfile, outfile, True)
                        _rVal = outfile
                    End If
                    Status = $"File = {outfile}"
                Else
                    Throw New Exception("File Generation Prematurely Terminated")
                End If
            Catch ex As Exception
                rErrString = Err.Description
                rErrNo = 1000
            Finally
                Try
                    If IO.File.Exists(tmpfile) Then
                        IO.File.Delete(tmpfile)
                    End If
                Catch ex As Exception
                    'ow well we tried
                End Try
                zTerminateObjects()
                If Not bNoErrors Then aImage.HandleError("Image_Write", "dxoFileTool", rErrString)
            End Try
            Return _rVal
        End Function
        Public Function WriteImageToFile(aImage As dxfImage, aFileType As dxxFileTypes, aFileSpec As String, aVersion As dxxACADVersions, bSuppressDimOverrides As Boolean, bNoErrors As Boolean, Optional bSuppressDXTNames As Boolean = False, Optional bNumericHandles As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1the type of file to write
            '#2the filename to write to
            '#3the AutCAD version to write the DWG to
            '^used to write the current file to disk
            '~returns True if the file was successfully written.
            '~all errors are raised to the caller.
            Dim rErrString As String = String.Empty
            If aImage IsNot Nothing Then
                _rVal = aImage.WriteToFile(Me, aFileType, aFileSpec, aVersion, bSuppressDimOverrides, bNoErrors, rErrString:=rErrString, bSuppressDXTNames:=bSuppressDXTNames, bNumericHandles:=bNumericHandles)
            End If
            Return _rVal
        End Function
        Private Function KeepEntity(aImage As dxfImage, ByRef aEntity As dxfEntity, ByRef rMatrix As dxoPropertyMatrix, ByRef rBlock As dxfBlock, ByRef reactEnt As dxfEntity) As Boolean
            'determine if the current entity should be written to file
            'don't write an entity that is ...
            '  a)marked to be ommmited from the file
            '  b)marked as a member of the screen domain
            '  c)has the invisible linetype
            rBlock = Nothing
            Dim aTxt As dxeText
            Dim _rVal As Boolean = aEntity.SaveToFile
            'only write suppressed entities if we are not writing to the file
            If Not _IncludedSuppressed Or _OutputToFile Then
                If aEntity.Suppressed Then _rVal = False
            End If
            If aEntity.GraphicType = dxxGraphicTypes.Text Then
                aTxt = aEntity
                If aTxt.TextType = dxxTextTypes.Attribute Then
                    aTxt = aTxt.Clone
                    aTxt.ImageGUID = aImage.GUID
                    aTxt.TextType = dxxTextTypes.AttDef
                    aEntity = aTxt
                Else
                End If
            End If
            aEntity.UpdatePath(False, aImage)
            aImage.HandleGenerator.AssignTo(aEntity)
            reactEnt = aEntity.ReactorEntity
            If reactEnt IsNot Nothing Then
                'aEntity.ReactorHandle = reactEnt.Handle
                reactEnt.ReactorHandle = aEntity.Handle
                'aEntity.ReactorGUID = reactEnt.GUID
                reactEnt.ReactorGUID = aEntity.GUID
            End If
            rMatrix = aEntity.DXFFileProperties(Nothing, aImage, rBlock, _SuppressInstances, False)
            If rMatrix Is Nothing Then
                rMatrix = New dxoPropertyMatrix
                _rVal = False
            Else
                If rMatrix.Count <= 0 Then _rVal = False
            End If
            If _rVal Then
                If aEntity.GroupName <> "" Then
                    aImage.Objex.AddGroupEntry(aEntity.GroupName, aEntity)
                End If
            End If
            Return _rVal
        End Function
        Private Sub ObjectName(aType As Object, Optional aName As Object = Nothing, Optional aColor As Integer = 0, Optional bSuppressed As Boolean = False, Optional bNONDXF As Boolean = False, Optional bKeepCurrentPath As Boolean = False, Optional aPS As Object = Nothing, Optional sReducePathTo As String = Nothing)
            Dim oVal As String = TVALUES.To_STR(aType, bTrim:=True)
            Dim pname As String = TVALUES.To_STR(aName, oVal, True)
            Dim pathWas As String = CurrentPath()
            If oVal.ToUpper = _Object.ToUpper Then Return
            _iProperty = 0
            _PropIndex = 0
            If _Object <> "" And Not bKeepCurrentPath Then
                If _PathBuilder.Remove(_Object) Then
                    Dim newPath As String = _PathBuilder.StringValue
                    RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathDecrease, dxxFileHandlerEvents.EndObject, pathWas, newPath))
                End If
            End If
            If oVal <> "" Then
                If _PathBuilder.Add(pname) Then
                    Dim newPath As String = _PathBuilder.StringValue
                    pname = _PathBuilder.LastValue
                    _Object = pname
                    RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathIncrease, dxxFileHandlerEvents.BeginObject, pathWas, newPath))
                End If
            Else
                _Object = ""
            End If
            If _Object <> "" Then
                SaveProperty(0, oVal, dxxPropertyTypes.dxf_String, aPS:=TVALUES.To_STR(aPS, pname), bSuppressed:=bSuppressed, bNonDXF:=bNONDXF)
            End If
            If Not String.IsNullOrEmpty(sReducePathTo) Then _PathBuilder.ReduceTo(sReducePathTo)
        End Sub
        Friend Function PrepareForReading(aFileSpec As String, ByRef rFileType As dxxFileTypes, ByRef rExtension As String, ByRef rTempFileCreated As Boolean, ByRef rFileVersion As dxxACADVersions) As String
            Dim _rVal As String
            '#1the file name to read DXF,DWG or DXT
            '#2returns the dxf file type of the passed file
            '#3returns the path to a temporary file
            rFileType = dxxFileTypes.Undefined
            rTempFileCreated = False
            rExtension = ""
            Try
                aFileSpec = aFileSpec.Trim
                If aFileSpec = "" Then Throw New Exception("No File Name Passed")
                If Not IO.File.Exists(aFileSpec) Then Throw New Exception($"File Not Found({ aFileSpec })")
                Dim fspec As String
                Dim bIsCAD As Boolean
                Dim fExts As String = String.Empty
                Dim ftpt As String = String.Empty
                If Not dxfUtils.ValidateFileName(aFileSpec, rFileType, rExtension, bIsCAD, fExts) Then
                    Throw New Exception($"Only File Extensions [{ fExts }] Can Be Read")
                End If
                If bIsCAD Then
                    rFileVersion = goACAD.GetCADFileVersion(aFileSpec, ftpt)
                    If rFileType = dxxFileTypes.DWG Then
                        'convert to r2000 DXF!!! in the temp folder
                        fspec = dxfUtils.TempFileSpec(rExtension)
                        'convert a dwg file to a temp dxf file
                        Status = $"Converting '{ aFileSpec }' { ftpt } To DXF"
                        fspec = goACAD.ConvertDWG_To_DXF(aFileSpec, rFileVersion, IO.Path.GetDirectoryName(fspec), False, IO.Path.GetFileNameWithoutExtension(fspec))
                        rTempFileCreated = True
                    Else
                        'convert a dxf file to a temp dxf file
                        rTempFileCreated = False
                        fspec = aFileSpec
                    End If

                Else
                    'just read the the file cause it's a DXT file
                    fspec = aFileSpec
                End If
                _rVal = fspec
            Catch ex As Exception
                Throw New Exception($"dxoFileTool.PrepareForReading - {ex.Message}")
            End Try
            Return _rVal
        End Function
        Private Sub SaveSection(aSection As dxxFileSections, aImage As dxfImage, bSuppressDimOverrides As Boolean, aIncludeList As String)
            If Not dxfEnums.Validate(GetType(dxxFileSections), aSection, "0") Then Return
            Dim sectName As String = dxfEnums.Description(aSection)
            Dim memName As String
            If aSection = dxxFileSections.Thumbnail And _Image.obj_THUMBNAIL.Count <= 0 Then Return
            Try
                SectionName = sectName
                Status = $"Writing: {SectionName}"
                '=============== save to the buffer ======================
                Select Case aSection
                    Case dxxFileSections.Header
#Region "HEADER"
                        Try
                            SaveProperties(zUpdateHeaderProps(bSuppressDimOverrides))
                        Catch ex As Exception
                            Throw ex
                        End Try
#End Region 'HEADER
                    Case dxxFileSections.Classes
#Region "CLASSES"
                        Try
                            Dim aCls As TDXFCLASS
                            For i As Integer = 1 To _Image.obj_CLASSES.Count
                                aCls = _Image.obj_CLASSES.Item(i)
                                If aCls._Props.Count > 1 Then
                                    memName = aCls.Name
                                    Status = $"Writing: CLASS({ memName })"
                                    ObjectName("CLASS", memName)
                                    SaveProperties(aCls._Props, aStartID:=2)
                                    ObjectName("")
                                End If
                                If _CancelBuffer Then Exit For
                            Next i
                        Catch ex As Exception
                            Throw ex
                        Finally
                            Status = "Writing: CLASSES"
                        End Try
#End Region 'CLASSES
                    Case dxxFileSections.Tables
#Region "TABLES"
                        Try
                            Dim aTbls(0) As TTABLE
                            Dim aTbl As TTABLE
                            TIMAGE.GetTables(_Image, aTbls)
                            For i As Integer = 1 To aTbls.Length
                                aTbl = aTbls(i - 1)
                                memName = aTbl.TableTypeName
                                If Not _SkipList.Contains(memName, bReturnTrueForNullList:=False) Then
                                    Status = "Writing: TABLE -" & memName
                                    Try
                                        SaveTable(aTbl, memName)
                                    Catch ex As Exception
                                        SaveException(ex)
                                    Finally
                                        Status = "Writing: TABLES"
                                        _ImageObj.Tables.UpdateTable(aTbl)
                                    End Try
                                End If
                                If _CancelBuffer Then Return
                            Next i
                        Catch ex As Exception
                            Throw ex
                        Finally
                            Status = "Writing: " & SectionName
                        End Try
#End Region 'TABLES
                    Case dxxFileSections.Blocks
#Region "BLOCKS"
                        Dim bKeep As Boolean
                        Dim bRecHandle As String
                        Dim aMatrix As TPROPERTYMATRIX
                        Dim bMatrix As TPROPERTYMATRIX
                        Dim aBlk As TBLOCK
                        Dim aBlockCol As TPROPERTYMATRIXARRAY
                        Dim blkProps As TPROPERTIES
                        Dim endblkProps As TPROPERTIES
                        Try
                            If _BlockMatrices Is Nothing Then Throw New Exception("Block Property MAtrix Array is not defined.")
                            _Invisible = False
                            For bi As Integer = 1 To _BlockMatrices.Count
                                aBlockCol = _BlockMatrices.ElementAt(bi - 1).Value
                                _PropIndex = 0
                                bKeep = aBlockCol.Count >= 2
                                aMatrix = aBlockCol.Item(1)
                                bMatrix = aBlockCol.Item(aBlockCol.Count)
                                aBlk = aMatrix.Block
                                _Invisible = aBlk.Suppressed
                                blkProps = aMatrix.Item(1).Item(1)
                                endblkProps = bMatrix.Item(1).Item(1)
                                _Invisible = aBlk.Suppressed
                                If blkProps.Count <= 0 Or endblkProps.Count <= 0 Then bKeep = False
                                If bKeep Then
                                    Status = "Writing: BLOCK(" & aBlk.Name & ")"
                                    bRecHandle = aBlk.BLKRECORD.Props.GCValueStr(5, "0", bReturnDefaultIfNullString:=True)
                                    Try
                                        BlockName = aBlk.Name
                                        ObjectName("Block Properties", "BlockProperties", aPS:="Block Properties", bNONDXF:=True)
                                        If BlockName.ToUpper.StartsWith("*PAPER_") Then
                                            blkProps.SetValGC(67, 1, aOccurance:=1)
                                        Else
                                            blkProps.SetValGC(67, 0, aOccurance:=1)
                                        End If
                                        SaveProperties(blkProps, bSuppressed:=aBlk.Suppressed, aStartID:=2)
                                        SaveProperty(New TPROPERTY(0, "End Block Properties", "End Block Properties", dxxPropertyTypes.dxf_String, bNonDXF:=True), False, False)
                                        ObjectName("")
                                        If aBlockCol.Count > 2 Then
                                            For ei As Integer = 2 To aBlockCol.Count - 1
                                                aMatrix = aBlockCol.Item(ei)
                                                'every matrix represents an entity
                                                SaveEntityPropertyMatrix(aImage, New dxoPropertyMatrix(aMatrix), bRecHandle, False)
                                                'Application.DoEvents()
                                                If _CancelBuffer Then Return
                                            Next ei
                                        End If
                                        'the end block
                                        ObjectName("ENDBLK", "ENDBLK")
                                        If aBlockCol.Count > 2 Then
                                            If _IDE Then
                                                'Beep()
                                            End If
                                        End If
                                        SaveProperties(endblkProps, bSuppressed:=aBlk.Suppressed, aStartID:=2)
                                        ObjectName("")
                                    Catch ex As Exception
                                        SaveException(ex)
                                    Finally
                                        BlockName = ""
                                    End Try
                                End If
                                'Application.DoEvents()
                                If _CancelBuffer Then Return
                            Next bi
                        Catch ex As Exception
                            Throw ex
                        Finally
                            BlockName = ""
                            Status = "Writing: " & SectionName
                        End Try
#End Region 'BLOCKS
                    Case dxxFileSections.Entities
#Region "ENTITIES"
                        Dim aMatrix As dxoPropertyMatrix
                        If _EntityMatrices Is Nothing Then Throw New Exception("The Internal Entity Matrix Is Not Defned")
                        Try
                            For Each aMatrix In _EntityMatrices
                                SaveEntityPropertyMatrix(aImage, aMatrix, "")
                                'Application.DoEvents()
                                If _CancelBuffer Then Return
                            Next
                        Catch ex As Exception
                            Throw ex
                        Finally
                            Status = $"Writing: {SectionName}"
                        End Try
#End Region 'ENTITIES
                    Case dxxFileSections.Objects
#Region "OBJECTS"
                        Dim aObj As dxfObject
                        Dim aProp As TPROPERTY
                        Dim rHandles As TPROPERTIES
                        Dim bKeep As Boolean
                        Dim bSaveIt As Boolean
                        Dim otname As String = String.Empty
                        Dim si As Integer
                        Dim otype As dxxObjectTypes
                        Dim bReacts As Boolean
                        Dim aObjs As colDXFObjects = aImage.Objex
                        Dim aBlk As dxfBlock
                        Dim aryProps As TPROPERTYARRAY
                        Dim aPl As New TPLANE("")
                        Dim oProps As TPROPERTIES
                        Dim hprops As New List(Of TPROPERTY)
                        Try
                            For i As Integer = 1 To aObjs.Count
                                aObj = aObjs.Item(i)
                                otname = aObj.ObjectName
                                aryProps = aObj.DXFProps(Nothing, 1, aPl, otname, aImage)
                                oProps = aryProps.Item(1, True)
                                bKeep = aObj.Props.Count > 2
                                hprops.Clear()
                                otype = aObj.ObjectType
                                bReacts = False
                                If otname = "" Then bKeep = False
                                If aObj.Suppressed Then
                                    If Not _IncludedSuppressed Then bKeep = False
                                End If
                                If bKeep Then
                                    'object name
                                    Status = "Writing: OBJECT(" & aObj.ToString & ")"
                                    ObjectName(otname, aObj.GUID, bSuppressed:=aObj.Suppressed, aPS:=aObj.ToString)
                                    If otype = dxxObjectTypes.Layout Then
                                        aBlk = aImage.Blocks.GetByName(aObj.Props.ValueStr("*REFERENCE"))
                                        If aBlk IsNot Nothing Then
                                            aObj.AddReactorHandle(aBlk.GUID, aBlk.Handle)
                                            aObj.ReactorGUID = aBlk.GUID
                                        End If
                                    End If
                                    For j As Integer = 2 To oProps.Count
                                        aProp = oProps.Item(j)
                                        bSaveIt = j <> si
                                        If otype = dxxObjectTypes.Layout Then
                                            If aProp.GroupCode = 331 Or aProp.GroupCode = 345 Or aProp.GroupCode = 346 Then
                                                bSaveIt = aProp.Value.ToString() <> "" And aProp.Value.ToString() <> "0"
                                            End If
                                        End If
                                        If bSaveIt Then
                                            If aProp.Hidden Then
                                                hprops.Add(aProp)
                                            Else
                                                SaveProperty(aProp, aObj.Suppressed)
                                            End If
                                        End If
                                        If aProp.GroupCode = 5 And Not bReacts Then
                                            'rHandles = aObj.Reactors.GetProps("{ACAD_REACTORS")
                                            'If rHandles.Count > 0 Then
                                            '    SaveProperties(rHandles.Wrapped(102, rHandles.Name, "Start Reactors", 102, "}", "End Reactors"), bSuppressed:=aObj.Suppressed)
                                            'End If
                                            rHandles = aObj.Reactors.GetProps("{ACAD_XDICTIONARY")
                                            If rHandles.Count > 0 Then
                                                SaveProperties(rHandles.Wrapped(102, rHandles.Name, "Start Dictionary Reactors", 102, "}", "End Reactors"), bSuppressed:=aObj.Suppressed)
                                            End If
                                            bReacts = True
                                        End If
                                        If otype = dxxObjectTypes.Dictionary And aProp.GroupCode = 281 Then
                                            'Dim aDic As dxfoDictionary = aObj
                                            'Dim eProps As TPROPERTIES = aDic.Entries.Properties
                                            'SaveProperties(eProps)
                                        ElseIf otype = dxxObjectTypes.DictionaryWDFLT And aProp.GroupCode = 281 Then
                                            'SaveProperties(aObj.Reactors.GetProps("Members"), bSuppressed:=aObj.Suppressed)
                                            'Dim aDic As dxfoDictionary = aObj
                                            'SaveProperties(aDic.Entries.Properties)
                                        ElseIf otype = dxxObjectTypes.Group And aProp.GroupCode = 71 Then
                                            'SaveProperties(aObj.Reactors.GetProps("Members"), bSuppressed:=aObj.Suppressed)
                                        ElseIf otype = dxxObjectTypes.MLineStyle And aProp.GroupCode = 71 Then
                                            SaveProperties(aObj.Reactors.GetProps("Elements"), bSuppressed:=aObj.Suppressed)
                                        ElseIf otype = dxxObjectTypes.TableStyle And aProp.GroupCode = 281 Then
                                            SaveProperties(aObj.Reactors.GetProps("Cell Settings"), bSuppressed:=aObj.Suppressed)
                                        End If
                                        'Application.DoEvents()
                                        If _CancelBuffer Then Return
                                    Next j
                                    If otype = dxxObjectTypes.SortEntsTable Then
                                        SaveProperties(aObj.Reactors.GetProps("Members"), bSuppressed:=aObj.Suppressed)
                                    End If
                                    AddExtendedData(aObj.ExtendedData)
                                    If _IncludedHidden Then SaveHiddenProps(hprops)
                                    ObjectName("")
                                End If
                                'Application.DoEvents()
                                If _CancelBuffer Then Exit For
                            Next i
                        Catch ex As Exception
                            Throw ex
                        Finally
                            Status = "Writing: " & SectionName
                        End Try
#End Region 'OBJECTS
                    Case dxxFileSections.Thumbnail
#Region "THUMBNAIL"
                        If _Image.obj_THUMBNAIL.Count > 0 Then
                            Try
                                SaveProperties(_Image.obj_THUMBNAIL, aStartID:=2)
                            Catch ex As Exception
                                Throw ex
                            End Try
                        End If
#End Region 'THUMBNAIL
                    Case dxxFileSections.Settings
#Region "SETTINGS"
                        Dim KeepList As New TLIST(",", Trim(aIncludeList.ToUpper))
                        Dim entry As TTABLEENTRY
                        If KeepList.Count <= 0 Then Return
                        Try
                            If KeepList.Contains("SCREEN") Then
                                ObjectName("SCREEN", bNONDXF:=True)
                                SaveProperties(_Image.obj_SCREEN.Props)
                                ObjectName("TEXTSTYLE", bNONDXF:=True, bKeepCurrentPath:=True)
                                SaveProperties(_Image.obj_SCREEN.TextStyle.Props, aStartID:=6)
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                            If KeepList.Contains("DISPLAY") Then
                                ObjectName("DISPLAY", bNONDXF:=True)
                                SaveProperties(_Image.obj_DISPLAY.Properties)
                                ObjectName("BITMAP", bNONDXF:=True, bKeepCurrentPath:=True)
                                SaveProperties(_ImageObj.bmp_Display.Properties)
                                ObjectName("", sReducePathTo:="DISPLAY")
                                ObjectName("VIEWPLANE", bNONDXF:=True)
                                SaveProperties(_Image.obj_DISPLAY.pln_VIEW.Properties)
                                ObjectName("", sReducePathTo:="DISPLAY")
                                ObjectName("DEVICEPLANE", bNONDXF:=True, bKeepCurrentPath:=True)
                                SaveProperties(_Image.obj_DISPLAY.pln_DEVICE.Properties)
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                            If _CancelBuffer Then Return
                            If KeepList.Contains("DIMSETTINGS") Then
                                ObjectName("DIMSETTINGS", bNONDXF:=True)
                                SaveProperties(_Image.set_DIM.Props)
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                            If _CancelBuffer Then Return
                            If KeepList.Contains("DIMOVERRIDES") Then
                                Dim aProps As TPROPERTIES
                                Dim bProps As TPROPERTIES
                                Dim aProp As TPROPERTY
                                Dim ename As String
                                Dim difCnt As Integer = 0
                                ObjectName("DIMOVERRIDES", bNONDXF:=True)
                                entry = _Image.set_DIMOVERRIDES

                                aProps = entry.Props
                                ename = aProps.GCValueStr(2)
                                bProps = _Image.tbl_DIMSTYLE.Entry(ename).Props
                                aProps = aProps.GetDifferences(bProps, difCnt, aGCSkipList:="0, 2, 105, 100", aNameSkipList:="*GUID", bSuppressHiddenProps:=False)
                                SaveProperty(2, ename, dxxPropertyTypes.dxf_String, aName:="Name")
                                For i As Integer = 1 To aProps.Count
                                    aProp = aProps.Item(i)
                                    aProp.NonDXF = True
                                    aProp.SuppressedValue = Nothing
                                    aProp.Suppressed = False
                                    If _CancelBuffer Then Return
                                    SaveProperty(aProp)
                                Next i
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                            If _CancelBuffer Then Return
                            If KeepList.Contains("SymbolSettings") Then
                                ObjectName("SYMBOLSETTINGS", bNONDXF:=True)
                                SaveProperties(_Image.set_SYMBOL.Props)
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                            If KeepList.Contains("LinetypeSettings") Then
                                ObjectName("LINETYPESETTINGS", bNONDXF:=True)
                                SaveProperties(_Image.set_LTYPES.Props)
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                            If _CancelBuffer Then Return
                            If KeepList.Contains("TableSettings") Then
                                ObjectName("TABLESETTINGS", bNONDXF:=True)
                                SaveProperties(_Image.set_TABLE.Props)
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                            If _CancelBuffer Then Return
                            If KeepList.Contains("TextSettings") Then
                                ObjectName("TEXTSETTINGS", bNONDXF:=True)
                                SaveProperties(_Image.set_TEXT.Props)
                                ObjectName("", sReducePathTo:=_SectionName)
                            End If
                        Catch ex As Exception
                            Throw ex
                        End Try
#End Region 'SETTINGS
                End Select
            Catch ex As Exception
                SaveException(ex)
            Finally
                _Invisible = False
                If aSection < dxxFileSections.Settings Then
                    SectionName = "ENDSEC"
                Else
                    _PathBuilder.ReduceTo(1)
                End If
            End Try
        End Sub
        Private Sub SaveTable(aTable As TTABLE, Optional aTableName As String = "")
            '^adds the properties required to write the table to a dxf file
            Dim aEntry As TTABLEENTRY
            Dim aReactors As TPROPERTIES
            Dim aProp As TPROPERTY
            Dim wTable As TTABLE
            Dim sclass As String
            Dim ename As String
            Dim cnt As Integer
            Dim tcnt As Integer
            Dim primeNames As String = String.Empty
            Dim hProps As New List(Of TPROPERTY)
            Dim tblHndl As String = aTable.Handle
            Try
                _Invisible = False
                If aTableName = "" Then aTableName = aTable.TableType.ToString
                If aTableName = "" Then Return
                sclass = aTable.SubClassName
                wTable = aTable.Clone(bDontCloneEntries:=True, bCloneHandles:=True)
                'collect the table entries that will be written
                cnt = 0
                For i As Integer = 1 To aTable.Count
                    aEntry = aTable.Item(i)
                    If _ShowHiddenObjects Or (Not _ShowHiddenObjects And Not aEntry.Invisible) Then
                        ename = aEntry.Name '.Props.GCValueStr(2)
                        If aTable.TableType = dxxReferenceTypes.LTYPE Then
                            If ename.ToUpper = "BYBLOCK" Then _ByBlockLTHandle = aEntry.Handle
                        End If
                        If _IDE Then
                            If String.Compare(sclass, aEntry.Props.GCValueStr(100, aOccurance:=2), True) <> 0 Then
                                MessageBox.Show($"ERR:   { sclass}")
                                aEntry.Props.Print()
                            End If
                        End If
                        If aTable.TableType = dxxReferenceTypes.DIMSTYLE Then
                            If aEntry.Domain = dxxDrawingDomains.Model Then
                                cnt += 1
                                If ename.Contains("$") Then
                                    TLISTS.Add(primeNames, ename)
                                    tcnt += 1
                                End If
                            End If
                        Else
                            If Not aEntry.Invisible Then cnt += 1
                        End If
                        wTable.Add(aEntry)
                    End If
                    If _CancelBuffer Then Return
                Next i
                'set the count properties (70 & 71)
                aTable.MemberCount = cnt
                If aTable.TableType = dxxReferenceTypes.DIMSTYLE Then
                    aTable.Props.SetValGC(71, tcnt, aOccurance:=1)
                End If
                'add to the current path
                _TableCount = aTable.Count
                TableName = aTableName
                'write the table Properties
                hProps.Clear()
                Dim pname As String = "Properties"
                ObjectName("Properties", "Table Properties", bNONDXF:=True, aPS:="Table Properties")
                ' _PathBuilder.Add(pname)
                ' SaveProperty(New TPROPERTY("Table Properties", 0, "Table Properties", bNonDXF:=True), False, False, aPostScript:="Table Properties")
                For i As Integer = 2 To aTable.Props.Count
                    aProp = aTable.Props.Item(i)
                    If aProp.PropType = dxxPropertyTypes.Handle Then
                        tblHndl = TVALUES.To_STR(aProp.Value)
                    End If
                    If Not aProp.Hidden Then SaveProperty(aProp) Else hProps.Add(aProp)
                    'prop_Print aTable.Props.Members(i - 1)
                    If aProp.PropType = dxxPropertyTypes.Handle Then
                        aReactors = aTable.Reactors.GetProps("{ACAD_XDICTIONARY")
                        If aReactors.Count > 0 Then
                            SaveProperties(aReactors.Wrapped(102, aReactors.Name, "Start Dictionary Reactors", 102, "}", "End Reactors"))
                        End If
                    End If
                    If _CancelBuffer Then Return
                Next i
                If aTable.TableType = dxxReferenceTypes.DIMSTYLE Then
                    For i As Integer = 1 To wTable.Count
                        aEntry = wTable.Item(i)
                        If TLISTS.Contains(aEntry.Name, primeNames) Then
                            SaveProperty(340, aEntry.Handle, dxxPropertyTypes.Pointer, aName:="Primary Member Handle")
                        End If
                    Next i
                    If _CancelBuffer Then Return
                End If
                SaveProperty(New TPROPERTY(0, "ENDPROPS", "End Properties", dxxPropertyTypes.dxf_String, bNonDXF:=True), False, True)
                If _CancelBuffer Then Return
                If _IncludedHidden Then SaveHiddenProps(hProps)
                ObjectName("")
                ' _PathBuilder.Remove(pname)
                '    If _Stream_Out Is Nothing Then ObjectName ""
                'loop on table entries
                For i As Integer = 1 To wTable.Count
                    Try
                        aEntry = wTable.Item(i)
                        _Invisible = aEntry.Invisible
                        ename = aEntry.Name
                        aEntry.Props.GCValueSet(330, tblHndl)
                        ObjectName(aTableName, ename)
                        If wTable.TableType = dxxReferenceTypes.DIMSTYLE Then
                            aEntry.PropValueSet(dxxDimStyleProperties.DIMPOST, dxfUtils.DimPrefixSuffix(aEntry.PropValueStr(dxxDimStyleProperties.DIMPREFIX), aEntry.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)))
                            aEntry.PropValueSet(dxxDimStyleProperties.DIMAPOST, dxfUtils.DimPrefixSuffix(aEntry.PropValueStr(dxxDimStyleProperties.DIMAPREFIX), aEntry.PropValueStr(dxxDimStyleProperties.DIMASUFFIX)))
                        End If
                        Try
                            hProps.Clear()
                            For j As Integer = 2 To aEntry.Props.Count
                                Try
                                    aProp = aEntry.Props.Item(j)
                                    If wTable.TableType = dxxReferenceTypes.DIMSTYLE Then
                                        If aProp.GroupCode >= 345 And aProp.GroupCode <= 347 Then
                                            If aProp.Value = _ByBlockLTHandle Then aProp.Suppressed = True
                                        End If
                                    End If
                                    If Not aProp.Hidden Then SaveProperty(aProp) Else hProps.Add(aProp)
                                    If aProp.PropType = dxxPropertyTypes.Handle Then
                                        aReactors = aEntry.Reactors.GetProps("{ACAD_XDICTIONARY")
                                        If aReactors.Count > 0 Then
                                            SaveProperties(aReactors.Wrapped(102, aReactors.Name, "Start Dictionary Reactors", 102, "}", "End Reactors"))
                                        End If
                                        If aTable.TableType = dxxReferenceTypes.DIMSTYLE Then
                                            aReactors = aEntry.Reactors.GetProps("{ACAD_REACTORS")
                                            If aReactors.Count > 0 Then
                                                SaveProperties(aReactors.Wrapped(102, aReactors.Name, "Start Dictionary Reactors", 102, "}", "End Reactors"))
                                            End If
                                        End If
                                    End If
                                    If aTable.TableType = dxxReferenceTypes.BLOCK_RECORD Then
                                        If aProp.GroupCode = 340 Then
                                            SaveProperties(aEntry.BinaryData)
                                            aReactors = aEntry.Reactors.GetProps("{BLKREFS")
                                            If aReactors.Count > 0 Then
                                                SaveProperties(aReactors.Wrapped(102, aReactors.Name, "Start Block Reactors", 102, "}", "End Reactors"))
                                            End If
                                        End If
                                    End If
                                Catch ex As Exception
                                    SaveException(ex)
                                End Try
                                'Application.DoEvents()
                                If _CancelBuffer Then Return
                            Next j
                        Catch ex As Exception
                            SaveException(ex)
                        End Try
                        AddExtendedData(aEntry.ExtendedData)
                        If _IncludedHidden Then SaveHiddenProps(hProps)
                    Catch ex As Exception
                        SaveException(ex)
                    Finally
                        ObjectName("")
                        _Invisible = False
                    End Try
                    'Application.DoEvents()
                    If _CancelBuffer Then Return
                Next i
            Catch ex As Exception
                SaveException(ex)
            Finally
                _Invisible = False
                _TableCount = 0
                TableName = "ENDTAB"
            End Try
        End Sub
        Private Sub SaveEntityPropertyMatrix(aImage As dxfImage, aMatrix As dxoPropertyMatrix, aOwnerHandle As String, Optional bSuppress As Object = Nothing)
            Dim ohndl As String
            Dim eProps As TPROPERTIES
            Dim eGUID As String
            Dim ehndl As String = String.Empty
            Dim gType As dxxGraphicTypes
            Dim oname As String
            Dim pArray As TPROPERTYARRAY
            Dim bsup As Boolean
            Dim dmn As dxxDrawingDomains
            Dim bReacts As TPROPERTIES
            Dim aProp As TPROPERTY
            Dim bToFile As Boolean
            Dim eType As dxxGraphicTypes
            Dim spchndl As String
            Dim hndl1 As String = String.Empty
            Dim bEntSec As Boolean = _SectionName = "ENTITIES"
            Dim ename As String
            Dim hProps As New List(Of TPROPERTY)
            Dim newHndl As Boolean
            Dim subGUID As String
            Dim agrp As dxfoGroup = Nothing
            Dim bgroupit As Boolean
            Try
                If aMatrix.Count <= 0 Then Return
                bToFile = Writing Or _OutputToFile
                aOwnerHandle = Trim(aOwnerHandle)
                'the first member of the matrix is the source
                'off all the subsequent members. if it is supressed
                'then whole set is suppressed
                pArray = aMatrix.Item(1)
                eProps = pArray.Item(1, True)
                'If it gets here then we assume it should be written
                Dim gname As String = aMatrix.GroupName
                If gname = "" Then gname = eProps.ValueStr("*GroupName")
                If gname <> "" Then
                    agrp = aImage.Objex.GetObject(gname, dxxObjectTypes.Group)
                    bgroupit = agrp IsNot Nothing
                End If
                'this is the graphic type of the primary entity
                eType = eProps.Value("*GraphicType")
                eGUID = eProps.GUID
                subGUID = eGUID
                'set the space handle
                dmn = eProps.ValueI("*Domain")
                spchndl = Trim(aOwnerHandle)
                If spchndl = "" Then
                    If dmn = dxxDrawingDomains.Model Then
                        spchndl = _ModelSpaceHandle
                    Else
                        spchndl = _PaperSpaceHandle
                    End If
                End If
                'get the base reactors
                'aReacts = GroupReactors(eProps, aMatrix.GroupName, grpidx, ghndl)
                For ei As Integer = 1 To aMatrix.Count
                    'each array in the matrix represents an instance of an entity
                    pArray = aMatrix.Item(ei)
                    eProps = pArray.Item(1, True)
                    subGUID = eGUID
                    If eProps.Count >= dxfGlobals.CommonProps Then
                        bReacts = New TPROPERTIES("Reactors")
                        ohndl = spchndl
                        gType = eType
                        For si As Integer = 1 To pArray.Count
                            eProps = pArray.Item(si, True)
                            If ei > 1 Then
                                If si = 1 Then
                                    If eProps.GUID = eGUID Then
                                        subGUID = eGUID & $"Instance-{ ei}"
                                    Else
                                        If dxfUtils.RunningInIDE Then Beep()
                                    End If
                                End If
                            End If
                            If si > 1 Or ei > 1 Then
                                'all instance and subentities get temp ahandles
                                ehndl = eProps.ValueStr("Handle", "0", bReturnDefaultForNullString:=True)
                                ehndl = aImage.HandleGenerator.NextHandle(subGUID, ehndl, rNewHandleCreated:=newHndl, aOwnerName:=subGUID)
                                If bgroupit Then
                                    agrp.AddEntry(subGUID, ehndl)
                                End If
                                If newHndl Then
                                    eProps.Handle = ehndl
                                End If
                            Else
                                bsup = TVALUES.ToBoolean(bSuppress, eProps.ValueB("*Invisible"))
                                'the primary instance
                                If ehndl = "0" Then
                                    If Not bsup Then ehndl = aImage.HandleGenerator.NextHandle(subGUID, "", rNewHandleCreated:=newHndl)
                                    If newHndl Then eProps.SetVal("Handle", ehndl)
                                End If
                            End If
                            pArray.SetItem(si, eProps)
                            'the first properties are the primary entities properties
                            'the rest are followers like attributes etc. and are considered virtual
                            oname = Trim(eProps.Item(1).Value).ToUpper
                            ehndl = eProps.ValueStr("Handle", "0", bReturnDefaultForNullString:=True)
                            If si > 1 Then
                                gType = eProps.ValueI("*GraphicType")
                            End If
                            If si = 1 Then
                                hndl1 = ehndl
                                eProps.SetVal("Owner Handle", spchndl)
                                If eType = dxxGraphicTypes.Leader Then
                                    'If pArray.Count > 1 Then
                                    '    eProps.Value("Annotation Handle") = pArray.Member(1).Handle ' .ValueStr("Handle")
                                    'End If
                                ElseIf eType = dxxGraphicTypes.Insert Then
                                    _PostScript = eProps.ValueStr("2")  'blockname
                                End If
                            Else
                                If eType = dxxGraphicTypes.Insert Then
                                    ohndl = hndl1
                                ElseIf eType = dxxGraphicTypes.Leader Then
                                    Dim ptype As dxxPropertyTypes = dxxPropertyTypes.Pointer
                                    bReacts.Add(330, hndl1, "Leader Reactor", "", ptype)
                                End If
                            End If
                            eProps.SetVal("Owner Handle", ohndl)
                            Try
                                ename = oname
                                If ename = "INSERT" Then
                                    ename += "{" & eProps.ValueStr("Block Name") & "}"  'block name
                                End If
                                ename += "(" & ehndl & ")"
                                If ei > 1 Then
                                    ename += ".Instance[" & ei & "]"
                                End If
                                ObjectName(oname, ename, bSuppressed:=bsup, bKeepCurrentPath:=ei > 1)
                                If bEntSec Then
                                    Status = "Writing: ENTITY-" & ename
                                End If
                                hProps.Clear()
                                For ki As Integer = 2 To eProps.Count
                                    aProp = eProps.Item(ki)
                                    If aProp.Hidden Then
                                        If si = 1 And ei = 1 Then hProps.Add(aProp)
                                    Else
                                        aProp.Suppressed = TPROPERTY.IsSuppressed(aProp)
                                        ' If Not aProp.Suppressed Or (aProp.Suppressed And Not bToFile And ei = 1 And si = 1) Then
                                        SaveProperty(aProp, Nothing)
                                        'End If
                                        If aProp.PropType = dxxPropertyTypes.Handle And bReacts.Count > 2 Then
                                            SaveProperties(bReacts)
                                        End If
                                    End If
                                    If _CancelBuffer Then Exit For
                                Next ki
                                If si = 1 And ei = 1 Then
                                    SaveHiddenProps(hProps)
                                End If
                            Catch ex As Exception
                                SaveException(ex)
                            Finally
                                'eGUID = _Object
                                ObjectName("")
                            End Try
                            'Application.DoEvents()
                            If _CancelBuffer Then Exit For
                        Next si
                    End If
                    'Application.DoEvents()
                    If _CancelBuffer Then Exit For
                    If aMatrix.ExtendedData.Count > 0 Then
                        AddExtendedData(aMatrix.ExtendedData)
                    End If
                Next ei
            Catch ex As Exception
                SaveException(ex)
            Finally
                'close the wrapper
                _Image = aImage.Strukture
            End Try
        End Sub
        Private Sub SaveHiddenProps(aProperties As TPROPERTIES)
            If Not _IncludedHidden Then Return
            Dim hProps As TPROPERTIES = aProperties.HiddenMembers
            If hProps.Count <= 0 Then Return
            For i As Integer = 1 To hProps.Count
                SaveProperty(hProps.Item(i), True)
            Next i
        End Sub
        Private Sub SaveHiddenProps(aProperties As List(Of TPROPERTY))
            If aProperties Is Nothing Or Not _IncludedHidden Or _OutputToFile Then Return
            If aProperties.Count <= 0 Then Return
            Dim pname As String = "HiddenProperties"
            Dim aProp As TPROPERTY
            Dim pathWas As String = CurrentPath()
            _PathBuilder.Add(pname)
            RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathIncrease, dxxFileHandlerEvents.BeginPropertyGroup, pathWas, CurrentPath()))
            SaveProperty(New TPROPERTY(0, "Hidden Properties", "Hidden Properties", dxxPropertyTypes.dxf_String, bNonDXF:=True), False, True, aPostScript:="Hidden Properties")
            For i As Integer = 1 To aProperties.Count
                aProp = aProperties.Item(i - 1)
                SaveProperty(aProp, False, True)
            Next i
            SaveProperty(New TPROPERTY(0, "ENDPROPS", "End Hidden Properties", dxxPropertyTypes.dxf_String, bNonDXF:=True), False, True)
            pathWas = CurrentPath()
            _PathBuilder.Remove(pname)
            RaiseEvent PathChangeEvent(New dxoFileHandlerEventArg(dxxFileHandlerEvents.PathDecrease, dxxFileHandlerEvents.EndPropertyGroup, pathWas, CurrentPath()))
        End Sub
        Private Sub SaveProperty(aGroupCode As Integer, aValue As Object, aPropType As dxxPropertyTypes, Optional aIgnore As Object = Nothing,
                                 Optional aName As String = "", Optional bSuppressed As Boolean = False, Optional bIsHandle As Boolean = False,
                                 Optional bIsPointer As Boolean = False, Optional aPS As Object = Nothing, Optional aColor As Integer = 0, Optional bNonDXF As Boolean = False)
            If aIgnore IsNot Nothing Then
                aIgnore = TPROPERTY.SetTypeByGroupCode(aGroupCode, aIgnore)
            End If
            aValue = TPROPERTY.SetTypeByGroupCode(aGroupCode, aValue)
            'If String.IsNullOrEmpty(aName) Then aProp.Name = TVALUES.To_STR(aValue)
            Dim aProp As New TPROPERTY With {
                .NonDXF = bNonDXF,
                .GroupCode = aGroupCode,
                .Value = aValue,
                .Name = aName,
                .PropType = aPropType
            }
            If bIsHandle Then
                aProp.PropType = dxxPropertyTypes.Handle
            ElseIf bIsPointer Then
                aProp.PropType = dxxPropertyTypes.Pointer
            End If
            aProp.Suppressed = bSuppressed
            If aIgnore IsNot Nothing Then
                aProp.SuppressedValue = aIgnore
                If aProp.Value = aProp.SuppressedValue Then aProp.Suppressed = True
            End If
            If aGroupCode < 0 Then aProp.Suppressed = True
            If aPS IsNot Nothing Then _PostScript = aPS.ToString()
            aProp.Color = aColor
            SaveProperty(aProp)
        End Sub
        Private Sub SaveProperties(aProps As TPROPERTIES, Optional aSkipList As String = "", Optional bSuppressed As Object = Nothing, Optional bRaiseObjectName As Boolean = False, Optional aStartID As Integer = 0)
            aSkipList = Trim(aSkipList)
            Dim bDoIt As Boolean
            'Dim bListPassed As Boolean = aSkipList <> ""
            Dim si As Integer = 1
            Dim skipVals As New TLIST(",", aSkipList)
            Dim aProp As TPROPERTY
            Dim hProps As New List(Of TPROPERTY)
            If aStartID > 0 Then si = aStartID
            For i As Integer = si To aProps.Count
                aProp = aProps.Item(i)
                If Not aProp.Hidden Then
                    bDoIt = Not skipVals.Contains(aProp.GroupCode)
                    If bRaiseObjectName And i = 1 Then
                        ObjectName(aProp.Value, Trim(aProp.Value))
                        bDoIt = False
                    End If
                    If bDoIt Then
                        SaveProperty(aProp, bSuppressed)
                    End If
                Else
                    hProps.Add(aProp)
                End If
                If _CancelBuffer Then Exit For
            Next i
            If _CancelBuffer Then Return
            If _IncludedHidden Then SaveHiddenProps(hProps)
        End Sub
        Private Sub SaveProperty(aProp As TPROPERTY, Optional bSuppressed As Object = Nothing, Optional bHidden As Object = Nothing, Optional aPostScript As String = Nothing)
            Dim bSaveIt As Boolean
            Dim txt As String
            Dim bProp As TPROPERTY = TPROPERTY.Null
            If Not String.IsNullOrEmpty(aPostScript) Then _PostScript = aPostScript
            If Not aProp.NonDXF Then
                If aProp.PropType <= dxxPropertyTypes.dxf_Variant Then
                    aProp.Value = TPROPERTY.SetTypeByGroupCode(aProp.GroupCode, aProp.Value)
                    aProp.Suppressed = TPROPERTY.IsSuppressed(aProp)
                Else
                    aProp.Suppressed = TPROPERTY.IsSuppressed(aProp)
                End If
            Else
                If _SectionName <> "SETTINGS" Then aProp.Suppressed = True
            End If
            'If aProp.Name = "IsPaperSpace" Then
            '    Beep()
            'End If
            Dim bSupr As Boolean = TVALUES.ToBoolean(bSuppressed, aProp.Suppressed)
            Dim bHidn As Boolean = TVALUES.ToBoolean(bHidden, aProp.Hidden)
            If _Invisible Then
                bSaveIt = _ShowHiddenObjects
            Else
                bSaveIt = Not bSupr Or (bSupr And _IncludedSuppressed)
            End If
            AssignPropertyPath(aProp)
            If bSaveIt Then
                aProp.Suppressed = bSupr
                _lastProp = aProp
                'we are creating a buffer
                If (aProp.PropType = dxxPropertyTypes.Handle Or aProp.PropType = dxxPropertyTypes.Pointer) And _HandleProps IsNot Nothing Then
                    Dim sval As String = TVALUES.To_STR(aProp.Value).Trim
                    If aProp.PropType = dxxPropertyTypes.Handle Then
                        If sval = "" Then aProp.Value = "0"
                        If sval = "0" Then
                            txt = $"Undefined Handle[{ sval}] On Line { aProp.LineNo} Is Undefined"
                            If _ThrowHandleErrors Then RaiseEvent HandleError(txt, "Handles", New dxoProperty(aProp), Nothing)
                            SaveException(New Exception(txt))
                        End If
                        If _HandleProps.TryGetValue(sval, bProp) Then
                            txt = $"Handle[{ sval}] On Line{ aProp.LineNo}"
                            txt += $" Is Used Again at Line { bProp.LineNo}"
                            If _ThrowHandleErrors Then RaiseEvent HandleError(txt, "Handles", New dxoProperty(aProp), New dxoProperty(bProp))
                            txt += vbLf & "    " & aProp.Key
                            txt += vbLf & "    " & bProp.Key
                            SaveException(New Exception(txt))
                        Else
                            _HandleProps.Add(sval, aProp)
                        End If
                    Else
                        'Pointers
                        If sval = "" Then aProp.Value = "0"
                        _PointerProps.Add(aProp)
                    End If
                End If
            End If
            If bSaveIt Then
                RaiseEvent PropertySaved(New dxoProperty(aProp), _iInstance, _PostScript, CurrentPath, bSupr, bHidn, _Invisible)
                If _OutputToFile Then
                    If Not bSupr Or bHidn Then WriteProperty(aProp)
                End If
                _PostScript = ""
                If aProp.Reactors.Count > 0 Then 'recursion!
                    Dim reacts As New TPROPERTIES("ACAD_REACTORS")
                    reacts.Add(New TPROPERTY(102, "{ACAD_REACTORS", "Start Reactors", dxxPropertyTypes.dxf_String))
                    reacts.Append(aProp.Reactors.Properties(True))
                    reacts.Add(New TPROPERTY(102, "}", "End Reactors", dxxPropertyTypes.dxf_String))
                    SaveProperties(reacts)
                End If
            End If
        End Sub
        Private Sub WriteProperty(aProp As TPROPERTY)
            'On Error Resume Next
            If aProp.NonDXF Then Return
            If _Stream_Out Is Nothing Then Return
            Dim aStr As String
            Dim sVal As String
            Dim hVal As Integer
            If _FileType = dxxFileTypes.DXT Then
                If aProp.PropType <> dxxPropertyTypes.Switch Then
                    sVal = TVALUES.To_STR(aProp.Value)
                Else
                    If aProp.Value = 1 Then sVal = "True" Else sVal = "False"
                End If
                If _NumericHandles Then
                    If aProp.PropType = dxxPropertyTypes.Handle Or aProp.PropType = dxxPropertyTypes.Pointer Then
                        hVal = TVALUES.HexToInteger(sVal)
                        sVal = hVal.ToString
                    End If
                End If
                If aProp.GroupCode = 0 Then
                    _Stream_Out.WriteLine("OBJECT=" & sVal)
                Else

                    If Not _SuppressDXTNames Then
                        _Stream_Out.WriteLine("[" & aProp.Name & "]" & aProp.GroupCode & "=" & sVal)
                    Else
                        _Stream_Out.WriteLine(aProp.GroupCode & "=" & sVal)
                    End If
                End If
            Else
                If aProp.IsHeader Then
                    aStr = aProp.Name
                    If aStr <> "" Then
                        _Stream_Out.WriteLine("9")
                        _Stream_Out.WriteLine(aStr)
                    End If
                End If
                _Stream_Out.WriteLine(aProp.GroupCode)
                _Stream_Out.WriteLine(aProp.StringValue(, False))

            End If
        End Sub
        Private Sub AddExtendedData(aExtendedDateArray As TPROPERTYARRAY, Optional bSuppressed As Boolean = False)

            If aExtendedDateArray.Count <= 0 Then Return
            Dim aProps As TPROPERTIES
            For i As Integer = 1 To aExtendedDateArray.Count
                aProps = aExtendedDateArray.Item(i)
                If aProps.Count > 0 Then
                    SaveProperty(1001, aProps.Name, dxxPropertyTypes.dxf_String, bSuppressed:=bSuppressed)
                    For j As Integer = 1 To aProps.Count
                        SaveProperty(aProps.Item(j), bSuppressed)
                    Next j
                End If
            Next i
        End Sub
        Private Function xExtractDimStyleOverrides(aDimStyle As TTABLEENTRY, aObj As TOBJECT) As Boolean
            Dim _rVal As Boolean
            '#1the DXF object that carries the properties to define a new dxeDimension
            '#2the the base dim style for the dimension
            '#3the the block that was defined for the dimension
            '^Defines a new dxeDimension based on the properties of the passed objects.
            Dim oRides As TPROPERTIES
            Dim gcProp As TPROPERTY
            Dim valProp As TPROPERTY
            Dim oProp As New TPROPERTY
            Dim dsProp As New TPROPERTY

            Dim ridx As Integer
            Dim j As Integer
            Dim bChange As Boolean
            Dim aRef As TTABLEENTRY
            Dim nProp As TPROPERTY = TPROPERTY.Null
            aDimStyle.IsCopied = True
            aDimStyle.IsGlobal = False
            oRides = aObj.ExtendedData.GetProps("ACAD", True)
            aObj.ExtendedData.ClearMember("ACAD")
            If oRides.Count > 0 Then
                Dim prop = oRides.GetByStringValue("{")
                If prop.Index > 0 Then
                    For j = prop.Index + 1 To oRides.Count
                        gcProp = oRides.Item(j)
                        If j + 1 > oRides.Count Then Exit For
                        valProp = oRides.Item(j)
                        j += 1
                        oProp.GroupCode = TVALUES.To_INT(gcProp.Value)
                        oProp.Value = valProp.Value
                        If oProp.IsPointer Then oProp.PropType = dxxPropertyTypes.Pointer
                        If oProp.PropType = dxxPropertyTypes.Pointer Then oProp.Value = Trim(oProp.Value)
                        If aDimStyle.Props.TryGet(oProp.GroupCode, nProp) Then
                            bChange = nProp.StringValue(1, False) <> oProp.StringValue(1, False)
                            aDimStyle.Props.SetVal(nProp.Name, oProp.Value)
                            If oProp.PropType = dxxPropertyTypes.Pointer Then
                                If bChange Then _rVal = True
                                nProp = aDimStyle.Props.GetMember($"*{ dsProp.Name}")
                                If nProp.Name <> "" Then
                                    Select Case dsProp.Name.ToUpper()
                                        Case "DIMTXSTY"
                                            aRef = _Image.tbl_STYLE.GetByHandle(oProp.StringValue, ridx)
                                        Case "DIMLTYPE", "DIMLTEX1", "DIMLTEX2"
                                            aRef = _Image.tbl_LTYPE.GetByHandle(oProp.StringValue, ridx)
                                        Case Else
                                            aRef = _Image.tbl_BLOCKRECORD.GetByHandle(oProp.StringValue, ridx)
                                    End Select
                                    aDimStyle.Props.SetVal(nProp.Name, aRef.Props.GCValueStr(2))
                                End If
                            End If
                        End If
                    Next j
                End If
            End If
            Return _rVal
        End Function
        Private Function xGetBufferGroups(aBuffer As TBUFFER) As TOBJECTS
            Dim rGrps As New TOBJECTS("Groups")
            '^read the current buffer objects section and pick out group object
            '~the groups object as stored in _BufferGroups
            'On Error Resume Next
            Dim aObjs As TOBJECTS = aBuffer.Objects
            Dim aObj As TOBJECT
            Dim bObj As TOBJECT
            Dim gNames As New TPROPERTIES("group names")
            Dim aProp As New TPROPERTY
            Dim aHndl As String
            Dim bProp As TPROPERTY
            Dim i As Integer
            Dim j As Integer
            'find the group name object dictionary
            aHndl = ""
            For i = 1 To aObjs.Count
                aObj = aObjs.Item(i)
                If UCase(aObj.Properties.GCValueStr(0)) = "DICTIONARY" Then
                    For j = 1 To aObj.Properties.Count
                        aProp = aObj.Properties.Item(j)
                        If aProp.GroupCode = 3 Then
                            If String.Compare(aProp.Value, "ACAD_GROUP", True) = 0 Then
                                If j + 1 <= aObj.Properties.Count Then
                                    aProp = aObj.Properties.Item(j + 1)
                                    aHndl = Trim(aProp.Value)
                                End If
                                Exit For
                            End If
                        End If
                    Next j
                End If
                If aHndl <> "" Then Exit For
            Next i
            If aHndl <> "" Then
                aObj = aObjs.GetByPropertyStringValue(aHndl, 5, i)
                If i > 0 Then
                    If aObj.Properties.TryGet(3, aProp) Then
                        j = aProp.Index + 1
                        For i = j To aObj.Properties.Count
                            aProp = aObj.Properties.Item(i)
                            If aProp.GroupCode = 3 Then
                                If i + 1 <= aObj.Properties.Count Then
                                    bProp = aObj.Properties.Item(i + 1)
                                    aHndl = Trim(bProp.Value)
                                    If aHndl <> "" Then
                                        bObj = aObjs.GetByPropertyStringValue(aHndl, 5, j)
                                        If j > 0 Then
                                            bObj.Name = aProp.Value.ToString()
                                            bObj.Properties.RemoveByGroupCode(340)
                                            rGrps.Add(bObj)
                                            aObjs.SetItem(j, bObj)
                                        End If
                                    End If
                                Else
                                    Exit For
                                End If
                            Else
                                Exit For
                            End If
                        Next i
                    End If
                End If
            End If
            Return rGrps
        End Function
        Private Sub xMultiLineAttrib(aAttrib As dxeText)
            Return
            '! I TRIED ! ..... but failed ;o(
            '^add the objects to define a multiline attribute
            Dim aDic As dxfoDictionary

            Dim xRec As dxfoXRecord
            Dim i As Integer
            Dim tg As String
            Dim vPts As TVECTORS
            Dim wf As Double
            vPts = xxTextAligmentVectors(aAttrib, wf)
            tg = aAttrib.AttributeTag

            If tg.Contains("_") Then tg = dxfUtils.LeftOf(tg, "_", bFromEnd:=True)
            aDic = _ImageObj.Objex.VerifyNamedDictionary("ACAD_XDICTIONARY")
            Dim reacts As dxoPropertyArray = aAttrib.Reactors
            reacts.ClearMember("{ACAD_XDICTIONARY", True)
            aAttrib.Reactors = reacts
            aAttrib.AddReactor(360, aDic.Handle, "{ACAD_XDICTIONARY", True)
            aDic.AddEntry(aAttrib.Name, aAttrib.Handle)
            aDic.SetProperty("Lock position flag", 1)
            xRec = New dxfoXRecord("ACAD_MLATT")
            _HandleGenerator.AssignTo(xRec)
            'aDic.AddReferenceTo(_HandleGenerator, xRec, bAddReactor:=True, aPointerGC:=360, aRefName:="ACAD_MLATT")
            Dim xprops As TPROPERTIES = xRec.Props
            If aAttrib.LineNo = 1 Then
                xprops.Add(New TPROPERTY(70, 4, "Mtext Flag", dxxPropertyTypes.dxf_Integer))
                xprops.Add(New TPROPERTY(70, 0, "Is Really Locked", dxxPropertyTypes.Switch))
            Else
                xprops.Add(New TPROPERTY(70, 2, "Mtext Flag", dxxPropertyTypes.dxf_Integer))
                xprops.Add(New TPROPERTY(70, 1, "Is Really Locked", dxxPropertyTypes.Switch))
            End If
            xprops.Add(New TPROPERTY(70, aAttrib.SourceCount - 1, "Sub Attribute Count", dxxPropertyTypes.dxf_Integer))
            xprops.Add(New TPROPERTY(340, aAttrib.Handle, "Attrib Entity Handle", dxxPropertyTypes.Pointer))
            xprops.Add(New TPROPERTY(40, 1, "Annotation Scale", dxxPropertyTypes.dxf_Double))
            xprops.Add(New TPROPERTY(2, tg, "Attribute Tag", dxxPropertyTypes.dxf_String))
            xprops.Add(New TPROPERTY(1, "Embedded Object", "Obj Type", dxxPropertyTypes.dxf_String))
            xprops.AddVector(10, vPts.Item(1, True), "Alignment Pt")
            xprops.Add(New TPROPERTY(40, aAttrib.TextHeight, "Text Height", dxxPropertyTypes.dxf_Double))
            xprops.Add(New TPROPERTY(41, 0, "Ref. Rectangle Width", dxxPropertyTypes.dxf_Double))
            xprops.Add(New TPROPERTY(46, 0, "Ref. Rectangle Height", dxxPropertyTypes.dxf_Double))
            xprops.Add(New TPROPERTY(72, aAttrib.DrawingDirection, "Drawing Direction", dxxPropertyTypes.dxf_Integer))
            xprops.Add(New TPROPERTY(1, aAttrib.SourceString, "Source String", dxxPropertyTypes.dxf_String))
            xprops.Add(New TPROPERTY(73, aAttrib.LineSpacingStyle, "Spacing Style", dxxPropertyTypes.dxf_Integer))
            xprops.Add(New TPROPERTY(44, aAttrib.LineSpacingFactor, "Spacing Factor", dxxPropertyTypes.dxf_Double))
            xRec.Props = xprops
            _ImageObj.Objex.Add(xRec)
            xRec = _ImageObj.Objex.GetObject("ACDB_RECOMPOSE_DATA", dxxObjectTypes.XRecord)
            If xRec.Index > 0 Then
                xprops = xRec.Props
                xprops.GetByStringValue(aAttrib.Handle, 1, i, 330)
                If i <= 0 Then
                    xprops.Add(New TPROPERTY(330, aAttrib.Handle, "Attrib Handle", dxxPropertyTypes.Pointer))
                    _ImageObj.Objex.SetItem(xRec.Index, xRec)
                End If
                xRec.Props = xprops
            End If
            _Image = _ImageObj.Strukture
        End Sub

        Private Sub xSetGroupName(aEnt As dxfEntity)
            '#1 the subject entity
            '^searchs the entity's reactor handles and the
            '^_BufferGroups members to find the group name the entity is associated
            '^to. If the group is found the entity's group name is assigned to the name
            '^of the matching group.
            aEnt.GroupName = ""
            If _BufferGroups.Count <= 0 Then Return
            Dim rHandles As dxoProperties = aEnt.ReactorProperties
            Dim aProp As dxoProperty
            Dim aHndl As String
            Dim aObj As TOBJECT
            Dim gname As String = String.Empty
            For i As Integer = 1 To rHandles.Count
                aProp = rHandles.Item(i)
                aHndl = TVALUES.To_STR(aProp.Value, bTrim:=True)
                If aHndl <> "" Then
                    For j As Integer = 1 To _BufferGroups.Count
                        aObj = _BufferGroups.Item(j)
                        If String.Compare(aObj.Properties.GCValueStr(5), aHndl, ignoreCase:=True) = 0 Then
                            gname = aObj.Name
                            Exit For
                        End If
                    Next j
                End If
                If gname <> "" Then
                    aEnt.GroupName = gname
                    Exit For
                End If
            Next i
        End Sub
        Private Sub xThrowPointerErrors()
            'On Error Resume Next
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY = TPROPERTY.Null
            Dim txt As String
            For i As Integer = 1 To _PointerProps.Count
                aProp = _PointerProps.Item(i - 1)
                txt = TVALUES.To_STR(aProp.Value).Trim
                If txt <> "" And txt <> "0" And Not aProp.Suppressed Then
                    If Not _HandleProps.TryGetValue(txt, bProp) Then
                        txt = "Pointer Property [" & txt & "] Refers To an unknown handle"
                        RaiseEvent HandleError(txt, "Pointers", New dxoProperty(aProp), Nothing)
                    End If
                End If
                'Application.DoEvents()
            Next i
        End Sub



        Private Function xxTextAligmentVectors(aText As dxeText, ByRef rWidthFactor As Double) As TVECTORS
            Dim _rVal As TVECTORS
            '#1the text to get the dxf properties for
            '^returns the properties required to write the object to a dxf file
            '~the DXF code of an Entity is it's entry in the Entities section in the DXF file
            Dim aOCS As TPLANE
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim aStrs As dxfStrings
            Dim tAlign As dxxMTextAlignments
            _rVal = New TVECTORS
            rWidthFactor = 1
            If aText Is Nothing Then Return _rVal
            aText.ImageGUID = _iGUID

            aStrs = aText.Strings
            rWidthFactor = aText.WidthFactor
            aOCS = TPLANE.ArbitraryCS(aText.PlaneV.ZDirection)
            tAlign = aText.Alignment
            v1 = aText.AlignmentPt1V
            v2 = v1
            If tAlign <> dxxMTextAlignments.BaselineLeft Then
                If tAlign = dxxMTextAlignments.Aligned Or tAlign = dxxMTextAlignments.Fit Then
                    v2 = aText.AlignmentPt2V
                Else
                    v3 = v1
                    v1 = aText.TextRectangleV.Point(dxxRectanglePts.BaselineLeft)
                    v2 = v3
                End If
            End If
            v1 = v1.WithRespectTo(aOCS, 6)
            v2 = v2.WithRespectTo(aOCS, 6)
            If tAlign = dxxMTextAlignments.Fit Then
                rWidthFactor *= aText.FitFactor
            End If
            _rVal.Add(v1)
            _rVal.Add(v2)
            Return _rVal
        End Function
        Private Sub xxUpdateDimstyleNames(ByRef aImage As TIMAGE)
            Dim hProp As TPROPERTY
            Dim bEntry As TTABLEENTRY = TTABLEENTRY.Null
            Dim xRec As New TTABLEENTRY(dxxReferenceTypes.BLOCK_RECORD, "_ClosedFilled")
            Dim idx As Integer
            Dim nidx As Integer
            For i As Integer = 1 To aImage.tbl_DIMSTYLE.Count
                Dim aEntry As TTABLEENTRY = aImage.tbl_DIMSTYLE.Item(i)
                Dim hProps As TPROPERTIES = aEntry.Props.GetGroupCodeRange(340, 347, True)
                For j As Integer = 1 To hProps.Count
                    hProp = hProps.Item(j)
                    idx = -1
                    If Trim(hProp.Value) <> "" Then
                        Dim nProp As TPROPERTY = aEntry.Props.GetMember($"*{ hProp.Name}")
                        Select Case UCase(hProp.Name)
                            Case "DIMTXSTY"
                                bEntry = aImage.tbl_STYLE.GetByHandle(hProp.Value, idx)
                                If bEntry.Index <= 0 Then bEntry = aImage.tbl_STYLE.Entry("Standard")
                            Case "DIMLTYPE", "DIMLTEX1", "DIMLTEX2"
                                bEntry = aImage.tbl_LTYPE.GetByHandle(hProp.Value, idx)
                                If bEntry.Index < 0 Then bEntry = aImage.tbl_LTYPE.Entry(dxfLinetypes.ByBlock)
                            Case Else
                                bEntry = aImage.tbl_BLOCKRECORD.GetByHandle(hProp.Value, idx)
                                If bEntry.Index < 0 Then
                                    bEntry = xRec
                                End If
                        End Select
                    End If
                    If bEntry.Index >= 0 Then
                        aEntry.Props.SetVal(hProp.Name, bEntry.Props.GCValueStr(5))
                        If nidx > 0 Then
                            aEntry.Props.SetVal(nidx, bEntry.Props.GCValueStr(2))
                        End If
                    End If
                Next j
                aImage.tbl_DIMSTYLE.SetItem(i, aEntry)
            Next i
        End Sub
        Private Sub AssignPropertyPath(aProp As TPROPERTY)
            If aProp.GroupCode = 0 Then
                aProp.Mark = True
                aProp.Path = CurrentPath()
                aProp.Key = CurrentPath()
                _iPropIdx = 0
            Else
                _iPropIdx += 1
                If aProp.Name = "" Then aProp.Name = "Property(" & _iPropIdx & ")"
                aProp.Path = CurrentPath(aProp.Name)
                aProp.Key = CurrentPath(aProp.Name)
                aProp.Mark = False
            End If
            _PropLineNo += 1
            aProp.LineNo = _PropLineNo
        End Sub

        Friend Function zStartStream(aFileType As dxxFileTypes, aFileSpec As String) As String
            Dim rOutputStream As StreamWriter = Nothing
            Return zStartStream(aFileType, aFileSpec, rOutputStream)
        End Function

        Friend Function zStartStream(aFileType As dxxFileTypes, aFileSpec As String, ByRef rOutputStream As StreamWriter) As String
            Dim _rVal As String = String.Empty
            If aFileType <> dxxFileTypes.DWG And aFileType <> dxxFileTypes.DXF And aFileType <> dxxFileTypes.DXT Then aFileType = dxxFileTypes.DXF

            If aFileType = dxxFileTypes.DXT Then _FileType = dxxFileTypes.DXT Else _FileType = dxxFileTypes.DXF

            Dim ID As Integer = 0
            Dim aExt As String
            Dim outfolder As String = IO.Path.GetTempPath

            If outfolder = "" Then outfolder = IO.Path.GetDirectoryName(aFileSpec)
            If aFileType = dxxFileTypes.DXT Then aExt = "dxt" Else aExt = "dxf"
            Dim outfile As String = IO.Path.Combine(outfolder, $"dxfGraphics[{ ID }].Buffer.{ aExt}")
            Do Until Not IO.File.Exists(outfile)
                IO.File.Delete(outfile)
                If IO.File.Exists(outfile) Then
                    ID += 1
                    If ID > 10 Then Return _rVal
                    outfile = IO.Path.Combine(outfolder, $"dxfGraphics[{ ID }].Buffer.{ aExt}")
                Else
                    Exit Do
                End If
            Loop
            _rVal = outfile
            _Stream_Out = New StreamWriter(outfile)
            rOutputStream = _Stream_Out
            Return _rVal
        End Function
        Private Sub zTerminateObjects()
            'really important
            'On Error Resume Next
            Status = "Terminating Object References"
            _HandleProps = Nothing
            _PointerProps = Nothing
            _PathBuilder = Nothing
            _HandleGenerator = Nothing
            If _OutputToFile Then
                If _Stream_Out IsNot Nothing Then _Stream_Out.Close()
            End If
            _Stream_Out = Nothing

            _ErrorList = Nothing
            _ImageObj = Nothing
            _iGUID = ""
            _File = Nothing
        End Sub
        Private Function zUpdateHeaderProps(bSuppressDimOverrides As Boolean) As TPROPERTIES
            Dim aHeader As TTABLEENTRY = _Image.set_HEADER
            Dim oRides As TTABLEENTRY = _Image.set_DIMOVERRIDES
            Dim rProps As TPROPERTIES = aHeader.Props
            Dim aPlane As TPLANE = _Image.obj_UCS
            Try
                Dim aProp As TPROPERTY
                Dim bProp As TPROPERTY
                Dim cProp As TPROPERTY
                Dim aEntry As TTABLEENTRY
                Dim pname As String
                Dim sval As String
                Dim ptype As dxxHeaderVars
                aEntry = TIMAGE.GetReference(_Image, dxxReferenceTypes.DIMSTYLE, rProps.ValueStr("$DIMSTYLE"))
                aEntry.PropValueSet(dxxDimStyleProperties.DIMPOST, dxfUtils.DimPrefixSuffix(aEntry.PropValueStr(dxxDimStyleProperties.DIMPREFIX), aEntry.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)))
                aEntry.PropValueSet(dxxDimStyleProperties.DIMAPOST, dxfUtils.DimPrefixSuffix(aEntry.PropValueStr(dxxDimStyleProperties.DIMAPREFIX), aEntry.PropValueStr(dxxDimStyleProperties.DIMASUFFIX)))
                If Not bSuppressDimOverrides Then
                    oRides.PropValueSet(dxxDimStyleProperties.DIMPOST, dxfUtils.DimPrefixSuffix(oRides.PropValueStr(dxxDimStyleProperties.DIMPREFIX), oRides.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)))
                    oRides.PropValueSet(dxxDimStyleProperties.DIMAPOST, dxfUtils.DimPrefixSuffix(oRides.PropValueStr(dxxDimStyleProperties.DIMAPREFIX), oRides.PropValueStr(dxxDimStyleProperties.DIMASUFFIX)))
                End If
                For i As Integer = 1 To rProps.Count
                    aProp = rProps.Item(i)
                    sval = TVALUES.To_STR(aProp.Value, bTrim:=True)
                    If aProp.GroupCode < 0 Then aProp.Suppressed = True
                    ptype = aProp.EnumName
                    pname = aProp.Name.ToUpper
                    If pname.StartsWith("$DIM") And pname <> "$DIMSTYLE" And pname <> "$DIMASSOC" And pname <> "$DIMASO" And pname <> "$DIMSHO" Then
                        'map header props to related dim style Prop
                        pname = Right(pname, Len(pname) - 1)
                        Select Case ptype
                            Case dxxHeaderVars.DIMBLK, dxxHeaderVars.DIMBLK1, dxxHeaderVars.DIMBLK2, dxxHeaderVars.DIMLDRBLK
                                pname = "*" & pname & "_NAME"
                                If sval = "" Then aProp.Value = "_ClosedFilled"
                            Case dxxHeaderVars.DIMTXSTY
                                pname = "*" & pname & "_NAME"
                                If sval = "" Then aProp.Value = "Standard"
                            Case dxxHeaderVars.DIMLTYPE, dxxHeaderVars.DIMLTEX1, dxxHeaderVars.DIMLTEX2
                                pname = "*" & pname & "_NAME"
                                If sval = "" Then aProp.Value = dxfLinetypes.ByBlock
                        End Select
                        If bSuppressDimOverrides Then
                            bProp = aEntry.Props.Item(pname)
                        Else
                            bProp = oRides.Props.Item(pname)
                        End If
                        If bProp.Index <> 0 Then
                            aProp.PropType = bProp.PropType
                            If bSuppressDimOverrides Then
                                aProp.Suppressed = True
                                cProp = aProp
                            Else
                                cProp = aEntry.Props.Item(pname)
                                aProp.Suppressed = bProp.Compare(cProp) ' False 'prop_Compare(bProp, cProp)
                            End If
                            aProp.SetVal(bProp.Value)
                            Select Case pname
                                Case "DIMSAH", "DIMLFAC"
                                    aProp.Suppressed = False
                                Case "DIMSCALE"
                                    aProp.Suppressed = False
                                Case "DIMJOGANG"
                                    If Not bSuppressDimOverrides Then
                                        aProp.Suppressed = cProp.Value = bProp.Value
                                    End If
                                    aProp.PropType = dxxPropertyTypes.dxf_Double
                                    aProp.SetVal(bProp.Value * Math.PI / 180)
                                Case "*DIMBLK_NAME", "*DIMBLK1_NAME", "*DIMBLK2_NAME", "*DIMLDRBLK_NAME"
                                    If String.Compare(sval, "_ClosedFilled", True) = 0 Then aProp.Suppressed = True
                             'props_Print oRides.Props
                                Case "*DIMLTYPE_NAME", "*DIMLTEX1_NAME", "*DIMLTEX2"
                                    If String.Compare(sval, dxfLinetypes.ByBlock, True) = 0 Then aProp.Suppressed = True
                                    '
                            End Select
                        End If
                    Else
                        Select Case pname
                            Case "$VERSIONGUID"
                 '                aProp.Value = ""
                            Case "$FINGERPRINTGUID"
                                aProp.Value = "{1EA0BA20-4020-11D7-B716-0002A559B82A}"
                            Case "$HANDLING"
                                aProp.Value = 1&
                            Case "$HANDSEED"
                                aProp.Value = TVALUES.To_Handle(_ImageObj.HandleGenerator.MaxHandle + 1000)
                            Case "$UCSORG"
                                rProps.SetVector(aProp.Index, aPlane.Origin)
                                Continue For
                            Case "$UCSXDIR"

                                rProps.SetVector(aProp.Index, TVECTOR.ToDirection(aPlane.XDirection))
                                Continue For

                            Case "$UCSYDIR"
                                rProps.SetVector(aProp.Index, TVECTOR.ToDirection(aPlane.YDirection))
                                Continue For


                        End Select
                    End If
                    rProps.SetItem(i, aProp)
                Next i
                Return rProps
            Catch ex As Exception
                Return rProps
            End Try
        End Function

        Private Sub _File_StatusChange(aStatus As String) Handles _File.StatusChange
            Status = aStatus
        End Sub
#End Region 'Methods
    End Class 'dxoFileTool
End Namespace
