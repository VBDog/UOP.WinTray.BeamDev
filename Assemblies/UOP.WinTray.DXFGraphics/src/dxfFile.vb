
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfFile
        Inherits List(Of dxfFileEntry)

        Public Event StatusChange(aStatus As String)

        Public Sub New()
            MyBase.New()
            Clear()
        End Sub


        Public Sub New(aFileSpec As String)

            MyBase.New()
            Clear()
            FileSpec = aFileSpec

        End Sub


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

        Private _SectionProperties As dxoPropertyArray
        Public ReadOnly Property SectionProperties As dxoPropertyArray
            Get
                Return _SectionProperties
            End Get
        End Property


        Public Property ErrorStrings As List(Of String)

        Private _ErrorString As String = String.Empty

        Public Property ErrorString As String
            Get
                Return _ErrorString
            End Get
            Set(value As String)
                If value Is Nothing Then value = String.Empty
                _ErrorString = value.Trim()
                If Not String.IsNullOrWhiteSpace(_ErrorString) And ErrorStrings IsNot Nothing Then
                    If Not ErrorStrings.Contains(_ErrorString) Then ErrorStrings.Add(_ErrorString)
                End If
            End Set
        End Property

        Private _BlockNames As List(Of String)
        Public ReadOnly Property BlockNames As List(Of String)
            Get
                Return _BlockNames
            End Get
        End Property

        Private _TableNames As List(Of String)
        Public ReadOnly Property TableNames As List(Of String)
            Get
                Return _TableNames
            End Get
        End Property

        Private _SectionNames As List(Of String)
        Public ReadOnly Property SectionNames As List(Of String)
            Get
                Return _SectionNames
            End Get
        End Property

        Private _FileSpec As String
        Public Property FileSpec As String
            Get
                Return _FileSpec
            End Get
            Private Set(value As String)
                _FileSpec = value
                _FileName = ""
                _Folder = ""
                If Not String.IsNullOrWhiteSpace(_FileSpec) Then
                    _Folder = IO.Path.GetDirectoryName(_FileSpec)
                    _FileName = IO.Path.GetFileNameWithoutExtension(_FileSpec)
                End If
            End Set
        End Property

        Private _FileName As String
        Public ReadOnly Property FileName As String
            Get
                Return _FileName
            End Get
        End Property

        Private _Folder As String
        Public ReadOnly Property Folder As String
            Get
                Return _Folder
            End Get
        End Property

        Private _Header As dxfFileSection
        Friend ReadOnly Property Header As dxfFileSection
            Get
                If _Header Is Nothing Then
                    If _SectionNames.FindIndex(Function(x) x.ToUpper() = "HEADER") >= 0 Then
                        xtract_Header()

                    Else
                        Return Nothing
                    End If

                End If
                Return _Header
            End Get
        End Property

        Private _Classes As dxfFileSection
        Friend ReadOnly Property Classes As dxfFileSection
            Get
                If _Classes Is Nothing Then
                    If _SectionNames.FindIndex(Function(x) x.ToUpper() = "CLASSES") >= 0 Then
                        xtract_Classes()
                        Return _Classes

                    Else
                        Return Nothing
                    End If

                End If
                Return _Classes
            End Get
        End Property

        Private _Tables As dxfFileSection
        Friend ReadOnly Property Tables As dxfFileSection
            Get
                If _Tables Is Nothing Then
                    If _SectionNames.FindIndex(Function(x) x.ToUpper() = "TABLES") >= 0 Then
                        xtract_Tables()

                    Else
                        Return New dxfFileSection(dxxFileSections.Classes)
                    End If

                End If
                Return _Tables
            End Get
        End Property

        Private _Blocks As dxfFileSection
        Friend ReadOnly Property Blocks As dxfFileSection
            Get
                If _Blocks Is Nothing Then
                    If _SectionNames.FindIndex(Function(x) x.ToUpper() = "BLOCKS") >= 0 Then
                        xtract_Blocks()

                    Else
                        Return Nothing
                    End If

                End If
                Return _Blocks
            End Get
        End Property

        Private _Entities As dxfFileSection
        Friend ReadOnly Property Entities As dxfFileSection
            Get
                If _Entities Is Nothing Then
                    If _SectionNames.FindIndex(Function(x) x.ToUpper() = "ENTITIES") >= 0 Then
                        xtract_Entities()

                    Else
                        Return Nothing
                    End If

                End If
                Return _Entities
            End Get
        End Property

        Private _ThumbNail As dxfFileSection
        Friend ReadOnly Property ThumbNail As dxfFileSection
            Get
                If _ThumbNail Is Nothing Then
                    If _SectionNames.FindIndex(Function(x) x.ToUpper() = "THUMBNAILIMAGE") >= 0 Then
                        xtract_ThumbNail()

                    Else
                        Return Nothing

                    End If
                End If

                Return _ThumbNail
            End Get
        End Property

        Private _Objects As dxfFileSection
        Friend ReadOnly Property Objects As dxfFileSection
            Get
                If _Objects Is Nothing Then
                    If _SectionNames.FindIndex(Function(x) x.ToUpper() = "OBJECTS") >= 0 Then
                        xtract_Objects()

                    Else
                        Return New dxfFileSection(dxxFileSections.Entities)
                    End If

                End If
                Return _Objects
            End Get
        End Property

        Private _Loaded As Boolean
        Public ReadOnly Property Loaded As Boolean
            Get
                Return _Loaded
            End Get
        End Property


        Friend Function ReadToImage(aFileSpec As String, Optional aSuppressString As String = "", Optional aTableList As String = "", Optional aImage As dxfImage = Nothing) As dxfImage

            If aFileSpec Is Nothing Then aFileSpec = ""
            If String.IsNullOrWhiteSpace(aFileSpec) Then aFileSpec = FileSpec

            Dim _rVal As dxfImage
            Dim merging As Boolean = aImage IsNot Nothing
            If Not merging Then
                _rVal = New dxfImage(IO.Path.GetFileName(aFileSpec))
            Else
                _rVal = aImage
            End If


            '#1the path to the file to read
            '#2a list of file sections and table names to skip during the read
            '#3flag to return all objects with the handles removed
            '^reads the drawing information form the passed drawing file name.
            '~supports DXF,DWG and DXT files.
            If aSuppressString Is Nothing Then aSuppressString = ""
            aSuppressString = aSuppressString.Trim().Replace(" ", "")

            Dim bNoHeader As Boolean = TLISTS.Contains("HEADER", aSuppressString)
            Dim bNoClasses As Boolean = TLISTS.Contains("CLASSES", aSuppressString)
            Dim bNoTables As Boolean = TLISTS.Contains("TABLES", aSuppressString)
            Dim bNoBlocks As Boolean = TLISTS.Contains("BLOCKS", aSuppressString)
            Dim bNoEntities As Boolean = TLISTS.Contains("ENTITIES", aSuppressString)
            Dim bNoObjects As Boolean = TLISTS.Contains("OBJECTS", aSuppressString)
            Dim bNoThumb As Boolean = TLISTS.Contains("THUMBNAILIMAGE", aSuppressString) Or bNoObjects

            Try



                'read in the file
                If Not Loaded Then

                    Read(aFileSpec, True)
                    If ErrorStrings.Count > 0 Then
                        Return _rVal
                    End If
                End If



                If bNoHeader Then
                    Status = $"Skipping DXF File Section - HEADER"

                Else

                    Status = $"Extracting DXF File Section - HEADER"
                    TransferToImage_Header(aImage:=_rVal)
                End If

                If bNoClasses Then
                    Status = $"Skipping DXF File Section - CLASSES"
                Else

                    Status = $"Extracting DXF File Section - CLASS"
                    TransferToImage_Classes(aImage:=_rVal, bNewMembersOnly:=merging)

                End If

                If bNoTables Then
                    Status = $"Skipping DXF File Section - TABLES"
                Else

                    Status = $"Extracting DXF File TABLES - TABLES"
                    TransferToImage_Tables(aImage:=_rVal, bNewMembersOnly:=merging, aTableList)

                End If

                If bNoBlocks Then
                    Status = $"Skipping DXF File Section - BLOCKS"

                Else

                    Status = $"Extracting DXF File Section - BLOCKS"
                    TransferToImage_Blocks(aImage:=_rVal, bNewMembersOnly:=merging, bSuppressReferences:=Not bNoTables)
                End If
                If bNoEntities Then
                    Status = $"Skipping DXF File Section - ENTITIES"

                Else

                    Status = $"Extracting DXF File Section - ENTITIES"
                    TransferToImage_Entities(aImage:=_rVal, bNewMembersOnly:=merging, bSuppressReferences:=Not bNoTables)
                End If

                If bNoObjects Then
                    Status = $"Skipping DXF File Section - OBJECTS"

                Else

                    Status = $"Extracting DXF File Section - OBJECTS"
                    TransferToImage_Objects(aImage:=_rVal, bNewMembersOnly:=merging)
                End If
                If bNoThumb Then
                    Status = $"Skipping DXF File Section - THUMBNAIL"

                Else

                    Status = $"Extracting DXF File Section - THUMBNAIL"
                    TransferToImage_ThumbNail(aImage:=_rVal)
                End If


            Catch ex As Exception

                ErrorString = ex.Message
            Finally



            End Try
            Return _rVal
        End Function

        Friend Function GetTableEntry(aRefType As dxxReferenceTypes, aNameOrHandle As String) As dxfFileObject

            Dim table As dxfFileObject = Tables.SubObjects.Find(Function(x) x.ReferenceType = aRefType)

            If table Is Nothing Then table = Tables.SubObjects.Find(Function(x) String.Compare(x.Name, dxfEnums.Description(aRefType), True) = 0)
            If table Is Nothing Then Return Nothing

            Return table.SubObjects.Find(Function(x) String.Compare(x.Handle, aNameOrHandle, True) = 0 Or String.Compare(x.Name, aNameOrHandle, True) = 0)

        End Function

        Public Overloads Sub Clear()
            MyBase.Clear()
            _TableNames = New List(Of String)()
            _SectionNames = New List(Of String)()
            _BlockNames = New List(Of String)()
            ErrorString = ""
            FileSpec = ""
            _Header = Nothing
            _Classes = Nothing
            _Tables = Nothing
            _Blocks = Nothing
            _SectionProperties = New dxoPropertyArray
            _Loaded = False
            ErrorStrings = New List(Of String)
        End Sub

        Public Function Read(aFileSpec As String, bSuppressErr As Boolean) As String
            ErrorString = ""
            ErrorStrings = New List(Of String)
            Clear()
            FileSpec = aFileSpec
            If String.IsNullOrWhiteSpace(aFileSpec) Then Return String.Empty


            Dim fin As IO.StreamReader = Nothing

            Dim lno As Integer = 0
            Try
                If Not IO.File.Exists(aFileSpec) Then
                    ErrorString = $"File Not Found '{aFileSpec}'"
                    Return ErrorString
                End If
                If String.Compare(IO.Path.GetExtension(aFileSpec), ".dxf", True) <> 0 Then
                    ErrorString = $"File '{aFileSpec}' Is Not a DXF File"
                    Return ErrorString
                End If
                Dim lastentry As dxfFileEntry = Nothing
                fin = New IO.StreamReader(aFileSpec)
                Dim section As String = String.Empty
                Dim table As String = String.Empty
                Dim block As String = String.Empty
                Dim line1 As String = String.Empty
                Dim line2 As String = String.Empty
                Status = $"Reading File '{ aFileSpec}' To Buffer"
                Do
                    If Not ReadTwoLines(fin, line1, line2, lno) Then
                        Return ErrorString
                    End If

                    If Count > 0 Then lastentry = Item(Count - 1)
                    Dim entry As New dxfFileEntry(lno - 1, TVALUES.To_INT(line1), line2)
                    If lastentry IsNot Nothing Then
                        If entry.GroupCode = 2 Then
                            If lastentry.GroupCode = 0 And String.Compare(lastentry.Value, "SECTION", True) = 0 Then

                                section = entry.Value.ToUpper()
                                lastentry.FileSection = section
                                _SectionNames.Add(section)
                                SectionProperties.Add(New dxoProperties(section))
                                SectionProperties.Item(section).Add(New dxoProperty(lastentry))
                            ElseIf lastentry.GroupCode = 0 And String.Compare(lastentry.Value, "TABLE", True) = 0 Then

                                table = entry.Value.ToUpper()
                                lastentry.Table = table
                                _TableNames.Add(table)
                            ElseIf section = "BLOCKS" And block = "" Then
                                block = entry.Value
                                _BlockNames.Add(block)
                                For i As Integer = Count To 1 Step -1
                                    Dim e As dxfFileEntry = Item(i - 1)
                                    e.BlockName = block
                                    If e.GroupCode = 0 Then Exit For
                                Next
                            End If

                        End If

                    End If
                    entry.FileSection = section
                    entry.Table = table
                    entry.BlockName = block
                    If entry.GroupCode = 5 And table <> "DIMSTYLE" Then
                        entry.PropertyType = dxxPropertyTypes.Handle
                    ElseIf entry.GroupCode = 105 And table = "DIMSTYLE" Then
                        entry.PropertyType = dxxPropertyTypes.Handle
                    Else
                        If (entry.GroupCode >= 320 And entry.GroupCode <= 369) Or (entry.GroupCode >= 390 And entry.GroupCode <= 399) Or entry.GroupCode = 1005 Then
                            entry.PropertyType = dxxPropertyTypes.Pointer
                        End If
                    End If

                    Add(entry)
                    If section <> "" Then
                        SectionProperties.Item(section).Add(New dxoProperty(entry))
                    End If

                    If String.Compare(entry.Value, "ENDTAB", True) = 0 Then table = ""
                    If String.Compare(entry.Value, "ENDBLK", True) = 0 Then block = ""
                    If String.Compare(entry.Value, "ENDSEC", True) = 0 Then section = ""

                    If fin.EndOfStream Then Exit Do
                Loop
                fin.Close()
                _Loaded = True
            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                fin?.Close()
                If ErrorString <> "" And Not bSuppressErr Then Throw New Exception(ErrorString)

                Status = ""
            End Try
            Return ErrorString

        End Function

        Private Function ReadTwoLines(aStream As IO.StreamReader, ByRef rLine1 As String, ByRef rLine2 As String, ByRef LineNo As Integer) As Boolean
            rLine1 = ""
            rLine2 = ""
            Do Until rLine1 <> "" ' skip blank lines
                If aStream.EndOfStream Then
                    ErrorString = "Premature End Of File Detected"
                    Return False
                End If
                LineNo += 1
                rLine1 = aStream.ReadLine.Trim()
            Loop
            If aStream.EndOfStream Then
                ErrorString = "Premature End Of File Detected"
                Return False
            End If
            rLine2 = aStream.ReadLine.Trim()
            If Not TVALUES.IsNumber(rLine1) Then
                ErrorString = "Invalid File Format Detected"
                Return ErrorString
            End If
            LineNo += 1
            Return True
        End Function

        Friend Function GetBlock(aBlockNameOrHandle As String, ByRef rErrorString As String) As dxfFileObject

            rErrorString = ""
            If Not Loaded And Not String.IsNullOrWhiteSpace(FileSpec) Then
                rErrorString = Read(FileSpec, False)
                If rErrorString <> "" Then Return Nothing
            End If

            Dim _rVal As dxfFileObject = Blocks.SubObjects.Find(Function(x) String.Compare(x.Name, aBlockNameOrHandle, True) = 0 Or String.Compare(x.Handle, aBlockNameOrHandle, True) = 0)
            If _rVal Is Nothing Then
                ErrorString = $"Block '{aBlockNameOrHandle}' Was Not Found In File '{FileName}'"
                rErrorString = ErrorString
                Return _rVal
            End If



            Return _rVal

        End Function

        Friend Function GetBlocks(aBlockNameOrHandle As String, ByRef rErrorString As String) As List(Of dxfFileObject)
            Dim _rVal As New List(Of dxfFileObject)
            rErrorString = ""
            If Not Loaded And Not String.IsNullOrWhiteSpace(FileSpec) Then
                rErrorString = Read(FileSpec, False)
                If rErrorString <> "" Then Return _rVal
            End If
            Dim fblock As dxfFileObject = GetBlock(aBlockNameOrHandle, rErrorString)
            If rErrorString <> "" Then Return _rVal

            Dim fblk As dxfFileObject = Blocks.SubObjects.Find(Function(x) String.Compare(x.Name, aBlockNameOrHandle, True) = 0 Or String.Compare(x.Handle, aBlockNameOrHandle, True) = 0)
            If ErrorString <> "" Then
                rErrorString = ErrorString
                Return _rVal
            End If
            _rVal.Add(fblock)
            Dim inserts As List(Of dxfFileSubObject) = fblk.SubObjects.FindAll(Function(x) String.Compare(x.Name, "INSERT", True) = 0)


            For Each ent As dxfFileSubObject In inserts
                Dim fblocks As List(Of dxfFileObject) = GetBlocks(ent.Properties.ValueS(2), rErrorString)
                If rErrorString <> "" Then Return _rVal
                _rVal.AddRange(fblocks)

            Next
            Return _rVal

        End Function

        Friend Function GetSection(aSectionName) As TOBJECT
            Return New TOBJECT(aSectionName, ToProperties(FindAll(Function(x) x.FileSection = aSectionName), aSectionName))

        End Function

        Friend Shared Function ToProperties(aEntries As List(Of dxfFileEntry), Optional aName As String = "") As TPROPERTIES
            Dim _rVal As New TPROPERTIES(aName)
            If aEntries Is Nothing Then Return _rVal
            For Each entry As dxfFileEntry In aEntries
                _rVal.Add(entry.Prop)
            Next
            Return _rVal
        End Function

        Friend Function Entry(aIndex As Integer, Optional aMembers As List(Of dxfFileEntry) = Nothing)
            If aMembers Is Nothing Then
                If aIndex < 1 Or aIndex > Count Then Return New dxfFileEntry(-1, -1, "")
                Return Item(aIndex - 1)
            Else
                If aIndex < 1 Or aIndex > aMembers.Count Then Return New dxfFileEntry(-1, -1, "")
                Return aMembers.Item(aIndex - 1)

            End If

        End Function

        Private Function xtract_Properties(aSection As dxxFileSections) As dxoProperties

            Dim _rVal As dxoProperties = SectionProperties.Item(dxfEnums.Description(aSection))


            Dim props As dxoProperties = SectionProperties.Item(dxfEnums.Description(aSection))
            If props Is Nothing Then

                Return _rVal
            End If
            Dim groupname As String
            If aSection = dxxFileSections.Header Then
                For i As Integer = 1 To props.Count
                    Dim iskip As Integer = 0
                    Dim prop1 As dxoProperty = props.Item(i)
                    Dim pname As String = prop1.ValueS
                    If pname.StartsWith("$", comparisonType:=StringComparison.OrdinalIgnoreCase) Then
                        groupname = pname
                        prop1.Name = $"{groupname}_NAME"
                    Else
                        groupname = ""

                    End If
                    prop1.GroupName = groupname

                    Dim prop2 As dxoProperty = props.Item(i + 1)
                    Dim prop3 As dxoProperty = props.Item(i + 2)
                    Dim prop4 As dxoProperty = props.Item(i + 3)


                    If pname.StartsWith("$") Then
                        If prop2 IsNot Nothing Then
                            If Not prop2.ValueS.StartsWith("$") Then
                                iskip = 1
                                prop2.GroupName = groupname
                                prop2.Key = groupname
                                prop2.Name = $"{groupname}_VALUE"
                            Else
                                Continue For
                            End If
                        End If
                        If prop3 IsNot Nothing Then
                            If Not prop3.ValueS.StartsWith("$") Then
                                prop2.IsOrdinate = True
                                prop3.IsOrdinate = True
                                iskip = 2
                                prop3.GroupName = groupname
                                prop3.Name = $"{groupname}_Y"
                            Else
                                Continue For
                            End If

                        End If
                        If prop4 IsNot Nothing Then
                            If Not prop4.ValueS.StartsWith("$") Then
                                prop4.IsOrdinate = True
                                iskip = 3
                                prop4.GroupName = groupname
                                prop4.Name = $"{groupname}_Z"
                            Else
                                Continue For
                            End If

                        End If

                    End If


                    i += iskip
                    If i + 1 > props.Count Then Exit For
                Next




            End If


            Return _rVal


        End Function

        Private Sub xtract_Header()
            ErrorString = ""

            Try
                Dim props As dxoProperties = xtract_Properties(aSection:=dxxFileSections.Header)
                If props.Count <= 0 Then
                    _Header = Nothing
                    Return
                End If

                Dim stndprops As New dxoProperties(dxpProperties.GetReferenceProps(dxxSettingTypes.HEADER))


                stndprops.CopyVals(props, bCopyNewMembers:=True, bByName:=True)
                _Header = New dxfFileSection(dxxFileSections.Header, stndprops)

            Catch ex As Exception
                ErrorString = ex.Message
                _Header = Nothing
            End Try


        End Sub

        Private Sub xtract_Classes()
            ErrorString = ""
            Try

                Dim props As dxoProperties = xtract_Properties(aSection:=dxxFileSections.Classes)
                If props.Count <= 0 Then
                    _Classes = Nothing
                    Return
                End If
                _Classes = New dxfFileSection(dxxFileSections.Classes) With {.Properties = props}
                Dim prop As dxoProperty = Nothing
                If props.TryGet(0, prop, 2) Then
                    If String.Compare(prop.Value.ToString(), "ENDSEC", True) <> 0 Then

                        Dim subset As dxoProperties = props.GetSubSet(0, aStartIndex:=props.IndexOf(prop))
                        Do Until subset.Count <= 0
                            If subset.TryGet(2, prop) Then
                                _Classes.AddSubObject(subset)

                            End If
                            subset = props.GetSubSet(0, aStartIndex:=props.IndexOf(subset.Last) + 1)
                        Loop
                    End If



                End If
            Catch ex As Exception
                ErrorString = ex.Message
                _Classes = Nothing
            End Try



        End Sub

        Private Sub xtract_Tables()
            ErrorString = ""
            Try


                Dim props As dxoProperties = xtract_Properties(aSection:=dxxFileSections.Tables)
                If props.Count <= 0 Then
                    _Tables = Nothing
                    Return
                End If


                _Tables = New dxfFileSection(dxxFileSections.Tables) With {.Properties = props}
                Dim tprops As dxoProperties = props.Members(0, "TABLE")
                Dim eprops As dxoProperties = props.Members(0, "ENDTAB")
                If tprops.Count <> eprops.Count Then
                    Throw New Exception("Invalid Table Section Properties Detected")

                End If
                Dim i As Integer = 0
                For Each tprop As dxoProperty In tprops
                    i += 1
                    Dim eprop As dxoProperty = eprops.Item(i)

                    Dim subset As dxoProperties = props.SubSet(aStartIndex:=props.IndexOf(tprop), aEndIndex:=props.IndexOf(eprop), True)
                    If subset.Count > 2 Then
                        Dim prop As dxoProperty = Nothing

                        If subset.TryGet(2, prop) Then ' be sure the name property is present
                            Dim tname As String = prop.StringValue
                            Dim reftype As dxxReferenceTypes = dxfEnums.ReferenceTypeByName(tname)
                            Dim stndprops As New dxoProperties(dxpProperties.Get_TableProps(reftype))
                            Dim prop1 As dxoProperty = Nothing
                            If subset.TryGet(0, tname, prop) Then ' find the start of the tabel members (0: {TABLE TYPE NAME}
                                Dim i1 As Integer = 1
                                Dim i2 As Integer = subset.IndexOf(prop) - 1

                                stndprops.CopyVals(subset.SubSet(aStartIndex:=i1, aEndIndex:=i2, bRemove:=True), bCopyNewMembers:=True)
                                'create the table with named and standardize properties
                                Dim table As dxfFileObject = _Tables.AddSubObject(stndprops)
                                table.ReferenceType = reftype


                                'get the table members
                                Dim memprops As dxoProperties = subset.GetSubSet(0, 1)

                                Do Until memprops.Count <= 0
                                    If memprops.Count > 1 Then
                                        stndprops = New dxoProperties(dxpProperties.GetReferenceProps(reftype))
                                        stndprops.CopyValues(memprops, bCopyNewMembers:=True)

                                        If reftype = dxxReferenceTypes.STYLE Then
                                            Dim entryname = memprops.ValueS(2)
                                        End If

                                        'create the table member with named and standardize properties
                                        Dim sobj As dxfFileSubObject = table.AddSubObject(stndprops)
                                        sobj.ReferenceType = reftype
                                    Else
                                        Exit Do
                                    End If

                                    memprops = subset.GetSubSet(0, aStartIndex:=subset.IndexOf(memprops.Last) + 1)


                                Loop
                            Else


                                'create the table with named and standardize properties (NO MEMBERS)
                                stndprops.CopyValues(subset, bCopyNewMembers:=True)
                                Dim table As dxfFileObject = _Tables.AddSubObject(stndprops)
                                table.ReferenceType = reftype

                            End If

                        End If

                    End If



                Next


            Catch ex As Exception
                ErrorString = ex.Message
                _Tables = Nothing
            End Try



        End Sub

        Private Sub xtract_Entities()
            ErrorString = ""
            Try


                Dim props As dxoProperties = xtract_Properties(aSection:=dxxFileSections.Entities)
                If props.Count <= 0 Then
                    _Entities = Nothing
                    Return
                End If


                _Entities = New dxfFileSection(dxxFileSections.Entities) With {.Properties = props}
                Dim tprops As dxoProperties = props.GroupCodeMembers(0)
                tprops.RemoveAll(Function(x) String.Compare(x.StringValue, "SECTION") = 0 Or String.Compare(x.StringValue, "ENDSEC") = 0)

                Dim i As Integer = 0
                For Each tprop As dxoProperty In tprops
                    i += 1
                    Dim i1 As Integer = props.IndexOf(tprop)
                    Dim i2 As Integer = props.Count

                    If i < tprops.Count Then
                        i2 = props.IndexOf(tprops.Item(i + 1)) - 1
                    End If


                    Dim subset As dxoProperties = props.SubSet(aStartIndex:=i1, aEndIndex:=i2, bGetClones:=True)

                    Dim prop As dxoProperty = Nothing
                    If subset.TryGet(0, prop) Then ' be sure the name property is present
                        Dim ename As String = prop.StringValue


                        Dim gtype As dxxGraphicTypes = dxfEnums.GraphicTypeByEntityTypeName(ename)
                        Dim stndprops As New dxoProperties(dxpProperties.Get_EntityProps(gtype, ""))
                        stndprops.CopyValues(subset, bCopyNewMembers:=True)
                        Dim sobj As dxfFileObject = _Entities.AddSubObject(stndprops)
                        sobj.GraphicType = gtype
                    End If


                Next


            Catch ex As Exception
                ErrorString = ex.Message
                _Entities = Nothing
            End Try


        End Sub
        Private Sub xtract_ThumbNail()
            ErrorString = ""
            Try


                Dim props As dxoProperties = xtract_Properties(aSection:=dxxFileSections.Thumbnail)
                If props.Count <= 0 Then
                    _ThumbNail = Nothing
                    Return
                End If


                _ThumbNail = New dxfFileSection(dxxFileSections.Thumbnail) With {.Properties = props}



            Catch ex As Exception
                ErrorString = ex.Message
                _ThumbNail = Nothing
            End Try


        End Sub

        Private Sub xtract_Objects()
            ErrorString = ""
            Try


                Dim props As dxoProperties = xtract_Properties(aSection:=dxxFileSections.Objects)
                If props.Count <= 0 Then
                    _Objects = Nothing
                    Return
                End If


                _Objects = New dxfFileSection(dxxFileSections.Objects) With {.Properties = props}
                Dim tprops As dxoProperties = props.GroupCodeMembers(0)

                Dim i As Integer = 0
                For Each tprop As dxoProperty In tprops
                    i += 1
                    Dim i1 As Integer = props.IndexOf(tprop)
                    Dim i2 As Integer = props.Count

                    If i < tprops.Count Then
                        i2 = props.IndexOf(tprops.Item(i + 1)) - 1
                    End If


                    Dim subset As dxoProperties = props.SubSet(aStartIndex:=i1, aEndIndex:=i2, bGetClones:=True)

                    Dim prop As dxoProperty = Nothing
                    If subset.TryGet(0, prop) Then ' be sure the name property is present
                        Dim ename As String = prop.StringValue


                        Dim otype As dxxObjectTypes = dxfEnums.ObjectTypeByObjectTypeName(ename)
                        Dim stndprops As New dxoProperties(dxpProperties.Get_ObjectProperties(otype, ""))
                        stndprops.CopyValues(subset, bCopyNewMembers:=True)
                        Dim sobj As dxfFileObject = _Objects.AddSubObject(stndprops)
                        sobj.DXFObjectType = otype
                    End If


                Next


            Catch ex As Exception
                ErrorString = ex.Message
                _Objects = Nothing
            End Try


        End Sub

        Private Sub xtract_Blocks()
            ErrorString = ""
            Try


                Dim props As dxoProperties = xtract_Properties(aSection:=dxxFileSections.Blocks)
                If props.Count <= 0 Then
                    _Blocks = Nothing
                    Return
                End If


                _Blocks = New dxfFileSection(dxxFileSections.Blocks) With {.Properties = props}
                Dim tprops As dxoProperties = props.Members(0, "BLOCK")
                Dim eprops As dxoProperties = props.Members(100, "AcDbBlockEnd")
                If tprops.Count <> eprops.Count Then
                    Throw New Exception("Invalid Block Section Properties Detected")

                End If
                Dim i As Integer = 0
                For Each tprop As dxoProperty In tprops
                    i += 1
                    Dim eprop As dxoProperty = eprops.Item(i)

                    Dim subset As dxoProperties = props.SubSet(aStartIndex:=props.IndexOf(tprop), aEndIndex:=props.IndexOf(eprop), bGetClones:=True)
                    If subset.Count > 2 Then
                        Dim prop As dxoProperty = Nothing
                        If subset.TryGet(2, prop) Then ' be sure the name property is present
                            Dim bname As String = prop.StringValue
                            If subset.TryGet(0, prop, 2) Then
                                Dim i1 As Integer = 1
                                Dim i2 As Integer = subset.IndexOf(prop) - 1
                                Dim stndprops As New dxoProperties(dxpProperties.Get_BlockProps(bname))
                                Dim blkprops As dxoProperties = subset.SubSet(aStartIndex:=i1, aEndIndex:=i2, bRemove:=True)
                                stndprops.CopyValues(blkprops, bCopyNewMembers:=True)
                                'create the block with named and standardize properties

                                Dim block As dxfFileObject = _Blocks.AddSubObject(stndprops)
                                'get the entites defined in the block
                                Dim memprops As dxoProperties = subset.GetSubSet(0, 1)
                                Do Until memprops.Count <= 0
                                    If memprops.Count > 1 Then
                                        Dim ename As String = memprops.ValueS(0)
                                        Dim gtype As dxxGraphicTypes = dxfEnums.GraphicTypeByEntityTypeName(ename)
                                        'If gtype = dxxGraphicTypes.Ellipse Then
                                        '    Beep()
                                        'End If
                                        stndprops = New dxoProperties(dxpProperties.Get_EntityProps(gtype, ""))
                                        stndprops.CopyValues(memprops, bCopyNewMembers:=True)
                                        Dim sobj As dxfFileSubObject = block.AddSubObject(memprops)
                                        sobj.GraphicType = gtype
                                    Else
                                        Exit Do
                                    End If

                                    memprops = subset.GetSubSet(0, aStartIndex:=subset.IndexOf(memprops.Last) + 1)


                                Loop

                            End If

                        End If

                    End If



                Next


            Catch ex As Exception
                ErrorString = ex.Message
                _Blocks = Nothing
            End Try


        End Sub

        Friend Function TransferToImage_Header(aImage As dxfImage) As List(Of String)
            ErrorStrings = New List(Of String)

            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Null"
                Return ErrorStrings
            End If
            If Header IsNot Nothing Then
                If ErrorStrings.Count > 0 Then Return ErrorStrings
            Else
                ErrorString = $"File '{FileName}' Does Not Contain a Header Section"
                Return ErrorStrings
            End If
            Try

                aImage.Header.Properties.CopyVals(Header.Properties, bCopyNewMembers:=True, bSkipHandles:=False, bSkipPointers:=False)
            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                If ErrorStrings.Count > 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType().Name, ErrorStrings)
            End Try

            Return ErrorStrings
        End Function

        Friend Function TransferToImage_Classes(aImage As dxfImage, Optional bNewMembersOnly As Boolean = True, Optional aNamesList As String = "") As List(Of String)
            ErrorStrings = New List(Of String)

            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Null"
                Return ErrorStrings
            End If
            If Classes IsNot Nothing Then
                If ErrorStrings.Count > 0 Then Return ErrorStrings
            Else
                ErrorString = $"File '{FileName}' Does Not Contain a Classes Section"
                Return ErrorStrings
            End If
            Try

                Dim targets As dxfFileObjects = Classes.GetTargets(aNamesList, ErrorStrings)
                If targets.Count <= 0 Then Return ErrorStrings

                Dim iclasses As colDXFClasses = aImage.Classes
                For Each fclass As dxfFileObject In targets
                    If Not TLISTS.Contains(fclass.Name, aNamesList, bReturnTrueForNullList:=True) Then Continue For
                    Dim iclass As dxfClass = iclasses.Find(Function(x) String.Compare(x.Name, fclass.Name, True) = 0)
                    If iclass Is Nothing Then
                        iclass = New dxfClass(fclass.Name)
                        iclass.Properties.CopyValues(fclass.Properties, bCopyNewMembers:=True)
                        aImage.HandleGenerator.AssignTo(iclass)
                        iclasses.Add(iclass)
                    Else
                        If bNewMembersOnly Then Continue For
                        iclass.Properties.CopyValues(fclass.Properties, bCopyNewMembers:=True, "5")

                    End If
                Next


            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                If ErrorStrings.Count > 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, ErrorStrings)
            End Try

            Return ErrorStrings
        End Function

        Friend Function TransferToImage_Tables(aImage As dxfImage, Optional bNewMembersOnly As Boolean = True, Optional aTableNamesList As String = "", Optional aMembersNamesList As String = "") As List(Of String)
            ErrorStrings = New List(Of String)

            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Null"
                Return ErrorStrings
            End If
            If Tables IsNot Nothing Then
                If ErrorStrings.Count > 0 Then Return ErrorStrings
            Else
                ErrorString = $"File '{FileName}' Does Not Contain a Tables Section"
                Return ErrorStrings
            End If
            Try
                Dim ttypes As List(Of Integer) = dxfEnums.EnumValueList(GetType(dxxTableTypes))
                For Each item As Integer In ttypes
                    Dim tbtype As dxxTableTypes = DirectCast(item, dxxTableTypes)
                    If tbtype = dxxTableTypes.Unknown Then Continue For
                    Dim tname As String = dxfEnums.Description(tbtype)
                    If Not TLISTS.Contains(tname, aTableNamesList, bReturnTrueForNullList:=True) Then Continue For

                    Dim reftype As dxxReferenceTypes = dxfEnums.ReferenceTypeByName(tname)
                    Dim itable As dxfTable = aImage.Table(reftype)
                    Dim ftable As dxfFileObject = Tables.SubObjects.Find(Function(x) x.ReferenceType = reftype)
                    If ftable Is Nothing Then
                        ErrorString = $"File {FileName} Does Not Contain Table '{tname}'"
                        Continue For
                    Else
                        For Each fentry As dxfFileObject In ftable.SubObjects
                            If Not TLISTS.Contains(fentry.Name, aMembersNamesList, bReturnTrueForNullList:=True) Then Continue For
                            Dim ientry As dxfTableEntry = itable.Entry(fentry.Name)
                            If ientry Is Nothing Then
                                ientry = dxfTableEntry.CreateNewReference(reftype, fentry.Name)
                                Dim edata As dxoPropertyArray = fentry.Properties.ExtractExtendedData(True)
                                ientry.Properties = fentry.Properties
                                ientry.ExtendedData = edata
                                If reftype = dxxReferenceTypes.STYLE Then
                                    Dim tstyle As dxoStyle = ientry
                                    Dim fntname As String = fentry.Properties.ValueS(3)
                                    Dim fntstyl As String = fentry.Properties.ValueS(1000)
                                    If fntname.EndsWith("TTF", comparisonType:=StringComparison.OrdinalIgnoreCase) Then
                                        Dim i As Integer = InStrRev(fntstyl, " ")
                                        If i > 0 Then
                                            fntname = $"{fntstyl.Substring(0, i - 1)}"
                                            fntstyl = fntstyl.Substring(i - 1, fntstyl.Length - fntname.Length)
                                            fntname = $"{fntname.Trim}.ttf"
                                        End If
                                    Else
                                        fntstyl = "Shape"
                                    End If
                                    If ientry.EntryType = dxxReferenceTypes.STYLE Then

                                        tstyle.UpdateFontName(fntname, fntstyl)
                                    End If
                                End If
                                itable.Add(ientry)
                            Else
                                If bNewMembersOnly Then Continue For
                                ientry.Properties.CopyValues(fentry.Properties, bCopyNewMembers:=True)
                            End If


                        Next
                    End If


                Next
            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                If ErrorStrings.Count > 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, ErrorStrings)
            End Try

            Return ErrorStrings
        End Function

        ''' <summary>
        ''' transfer the requested block from this file top the passed image
        ''' </summary>
        ''' <param name="aImage">the target image</param>
        ''' <param name="bNewMembersOnly">flag to only transfer block that don't already exist in the images blocks collection</param>
        ''' <param name="aNamesList">the comma delimited list of block names to transfer</param>
        ''' <param name="bConvertAttribsToText">a flag to convert any attributes definitions in the text</param>
        ''' <param name="bSuppressReferences">a flag to prevent the transfering layers etc. that the block references from the fime to the passed image</param>
        ''' <param name="bIncludeSubBlocks">a flag controling if any nested blocks in the tartget blocks are also transfered</param>
        ''' <returns></returns>
        Friend Function TransferToImage_Blocks(aImage As dxfImage, Optional bNewMembersOnly As Boolean = True, Optional aNamesList As String = "", Optional bConvertAttribsToText As Boolean = False, Optional bSuppressReferences As Boolean = False, Optional bIncludeSubBlocks As Boolean = False) As List(Of dxfBlock)

            Dim _rVal As New List(Of dxfBlock)()
            ErrorStrings = New List(Of String)

            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Null"
                Return _rVal
            End If
            If Blocks IsNot Nothing Then
                If ErrorStrings.Count > 0 Then Return _rVal
            Else
                ErrorString = $"File '{FileName}' Does Not Contain a Blocks Section"
                Return _rVal
            End If


            Dim targets As dxfFileObjects = Blocks.GetTargets(aNamesList, ErrorStrings, bCollectReferences:=Not bSuppressReferences)
            If targets.Count <= 0 Then Return _rVal
            Dim iblocks As colDXFBlocks = aImage.Blocks
            Try

                Dim serr As String = String.Empty
                For Each fblock As dxfFileObject In targets
                    Dim addit As Boolean = True
                    Dim btargets As dxfFileObjects = Nothing
                    Dim iblock As dxfBlock = iblocks.GetByName(fblock.Name)
                    If iblock Is Nothing Then
                        iblock = xObj_ToBlock(fblock, btargets, bConvertAttribsToText, bSuppressReferences, Nothing, aImage.HandleGenerator)
                        If iblock Is Nothing Then
                            ErrorString = serr
                            addit = False
                            Continue For
                        End If
                    Else
                        If bNewMembersOnly Then
                            _rVal.Add(iblock)
                            addit = False
                            iblock = Nothing
                            Continue For
                        End If
                        iblock = xObj_ToBlock(fblock, btargets, bConvertAttribsToText, bSuppressReferences, Nothing, aImage.HandleGenerator)
                        If iblock Is Nothing Then

                            addit = False
                            Continue For
                        End If
                        iblocks.Remove(iblock)
                    End If
                    If iblock IsNot Nothing And addit Then
                        iblock.ImageGUID = aImage.GUID
                        aImage.HandleGenerator.AssignTo(iblock)
                        iblock = iblocks.AddToCollection(iblock, bSuppressEvnts:=True, aImage:=aImage)
                        If iblock IsNot Nothing Then
                            _rVal.Add(iblock)
                        End If
                        If Not bSuppressReferences Then btargets.TranserferReferences(Me, aImage)

                        If bIncludeSubBlocks Then
                            Dim inserts As List(Of dxfFileObject) = fblock.SubObjects.FindAll(Function(x) String.Compare(x.Name, "INSERT", True) = 0).OfType(Of dxfFileObject)().ToList
                            For Each insert As dxfFileObject In inserts
                                Dim bname As String = insert.Properties.ValueS(2)
                                If targets.FindIndex(Function(x) String.Compare(x.Name, bname, True) = 0) < 0 Then
                                    Dim newblock As dxfFileObject = Blocks.SubObjects.Find(Function(x) String.Compare(x.Name, bname, True) = 0)
                                    If newblock IsNot Nothing Then targets.Add(newblock)
                                End If
                            Next
                        End If
                    End If
                Next
            Catch ex As Exception
                ErrorString = ex.Message
            Finally

                If ErrorStrings.Count > 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, ErrorStrings)
            End Try

            Return _rVal
        End Function

        Friend Function TransferToImage_Entities(aImage As dxfImage, Optional bNewMembersOnly As Boolean = False, Optional aNamesList As String = "", Optional bConvertAttribsToText As Boolean = False, Optional bSuppressReferences As Boolean = False) As List(Of String)
            ErrorStrings = New List(Of String)

            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Null"
                Return ErrorStrings
            End If
            If Entities IsNot Nothing Then
                If ErrorStrings.Count > 0 Then Return ErrorStrings
            Else
                ErrorString = $"File '{FileName}' Does Not Contain a Entities Section"
                Return ErrorStrings
            End If
            Dim ientities As colDXFEntities = aImage.Entities
            Dim sewuz As Boolean = ientities.SuppressEvents
            ientities.SuppressEvents = True
            Dim bjustadd As Boolean = ientities.Count <= 0


            Try

                Dim targets As dxfFileObjects = Entities.GetTargets(aNamesList, ErrorStrings, Not bSuppressReferences)
                If targets.Count <= 0 Then Return ErrorStrings
                Dim ients As colDXFEntities = xObjs_ToEntities(targets, bConvertAttribsToText, aImage.Entities, aImage.HandleGenerator)
                If targets.CollectReferenceNames Then targets.TranserferReferences(Me, aImage)

            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                ientities.SuppressEvents = sewuz
                If ErrorStrings.Count > 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, ErrorStrings)
            End Try

            Return ErrorStrings
        End Function

        Friend Function TransferToImage_ThumbNail(aImage As dxfImage) As List(Of String)
            ErrorStrings = New List(Of String)

            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Null"
                Return ErrorStrings
            End If
            If ThumbNail IsNot Nothing Then
                If ErrorStrings.Count > 0 Then Return ErrorStrings
            Else
                ' ErrorString = $"File '{FileName}' Does Not Contain a Thumbnail Section"
                Return ErrorStrings
            End If
            Try
                aImage.obj_THUMBNAIL = New TPROPERTIES(ThumbNail.Properties)

            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                If ErrorStrings.Count > 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, ErrorStrings)
            End Try

            Return ErrorStrings
        End Function
        Friend Function TransferToImage_Objects(aImage As dxfImage, Optional bNewMembersOnly As Boolean = True) As List(Of String)
            ErrorStrings = New List(Of String)

            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Null"
                Return ErrorStrings
            End If
            If Objects IsNot Nothing Then
                If ErrorStrings.Count > 0 Then Return ErrorStrings
            Else
                ErrorString = $"File '{FileName}' Does Not Contain a Objects Section"
                Return ErrorStrings
            End If
            Try

                Dim targets As dxfFileObjects = Objects.GetTargets("", ErrorStrings)
                If targets.Count <= 0 Then Return ErrorStrings
                Dim tobject As TDXFOBJECT
                Dim iobjects As colDXFObjects = aImage.Objex
                For Each fobject As dxfFileObject In targets

                    Dim iobject As dxfObject = iobjects.Find(Function(x) fobject.DXFObjectType = x.ObjectType And (String.Compare(x.Name, fobject.Name, True) = 0 Or String.Compare(x.Handle, fobject.Handle, True) = 0))
                    If iobject Is Nothing Then
                        tobject = New TDXFOBJECT(fobject.DXFObjectType)
                        tobject.Props.CopyValues(fobject.Properties, bCopyNewMembers:=True)
                        iobject = dxfObject.Create(tobject, aImage.GUID)
                        aImage.HandleGenerator.AssignTo(iobject)
                        iobjects.Add(iobject)
                    Else
                        If bNewMembersOnly Then Continue For
                        tobject = iobject.Strukture
                        tobject.Props.CopyValues(fobject.Properties, bCopyNewMembers:=True, "5")
                        iobject.Strukture = tobject

                    End If
                Next



            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                If ErrorStrings.Count > 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, ErrorStrings)
            End Try

            Return ErrorStrings
        End Function


        Friend Function TransferToImage_References(aImage As dxfImage,
                                              Optional aLTNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional aStyleNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional aDimStyleNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional aLayersNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional aUCSNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional aViewNamesOrHandlesList As List(Of String) = Nothing,
                                               Optional aVPortNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional aAPPIDNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional aBRecordNamesOrHandlesList As List(Of String) = Nothing,
                                              Optional bOverrideExising As Boolean = False) As List(Of String)

            If ErrorStrings Is Nothing Then ErrorStrings = New List(Of String)
            ErrorString = ""
            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Nothing"
                Return ErrorStrings
            End If
            'order matters here

            If aAPPIDNamesOrHandlesList IsNot Nothing Then
                If aAPPIDNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.APPID, aAPPIDNamesOrHandlesList, bOverrideExising)
                End If

            End If
            If aLTNamesOrHandlesList IsNot Nothing Then
                If aLTNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.LTYPE, aLTNamesOrHandlesList, bOverrideExising)
                End If

            End If

            If aLayersNamesOrHandlesList IsNot Nothing Then
                If aLayersNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.LAYER, aLayersNamesOrHandlesList, bOverrideExising)
                End If

            End If

            If aViewNamesOrHandlesList IsNot Nothing Then
                If aViewNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.VIEW, aViewNamesOrHandlesList, bOverrideExising)
                End If

            End If
            If aVPortNamesOrHandlesList IsNot Nothing Then
                If aVPortNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.VPORT, aVPortNamesOrHandlesList, bOverrideExising)
                End If

            End If
            If aStyleNamesOrHandlesList IsNot Nothing Then
                If aStyleNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.STYLE, aStyleNamesOrHandlesList, bOverrideExising)
                End If

            End If
            If aDimStyleNamesOrHandlesList IsNot Nothing Then
                If aDimStyleNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.DIMSTYLE, aDimStyleNamesOrHandlesList, bOverrideExising)
                End If

            End If
            If aUCSNamesOrHandlesList IsNot Nothing Then
                If aUCSNamesOrHandlesList.Count > 0 Then
                    TransferToImage_References(aImage, dxxReferenceTypes.UCS, aUCSNamesOrHandlesList, bOverrideExising)
                End If

            End If
            Return ErrorStrings

        End Function


        Friend Sub TransferToImage_References(aImage As dxfImage, aRefType As dxxReferenceTypes, aNamesOrHandlesList As List(Of String), Optional bOverrideExising As Boolean = False)
            If ErrorStrings Is Nothing Then ErrorStrings = New List(Of String)
            ErrorString = ""
            If aImage Is Nothing Then
                ErrorString = "The Passed Image Is Nothing"
                Return
            End If
            If aNamesOrHandlesList Is Nothing Then
                Return
            End If
            aNamesOrHandlesList.RemoveAll(Function(x) String.IsNullOrWhiteSpace(x))

            If aNamesOrHandlesList.Count <= 0 Then Return
            Dim tname As String = dxfEnums.Description(aRefType)

            Dim ftable As dxfFileObject = Tables.SubObjects.Find(Function(x) x.ReferenceType = aRefType Or String.Compare(x.Name, tname, True) = 0)
            If ftable Is Nothing Then
                ErrorString = $"Table {tname} Was not Found in File '{FileName}'"
                Return
            End If

            Dim itable As dxfTable = aImage.Table(aRefType)
            If itable Is Nothing Then
                ErrorString = $"Table {tname} Was not Found in Image '{aImage.Name}'"
                Return

            End If

            Try
                For Each refname As String In aNamesOrHandlesList.Distinct()

                    refname = refname.Trim()
                    Dim fobj As dxfFileObject = ftable.SubObjects.Find(Function(x) String.Compare(refname, x.Name, True) = 0 Or String.Compare(refname, x.Handle, True) = 0)
                    If fobj Is Nothing Then Continue For
                    Dim tEntry As dxfTableEntry = Nothing
                    Dim errstr As String = String.Empty
                    Dim newentry As dxfTableEntry = Nothing

                    If itable.TryGetEntry(refname, tEntry) Then
                        If bOverrideExising Then
                            newentry = FileObject_ToTableEntry(fobj, errstr)
                            If errstr = "" And newentry IsNot Nothing Then
                                newentry.Handle = tEntry.Handle
                                itable.UpdateEntry = newentry
                            Else
                                ErrorString = errstr
                            End If
                        End If
                    Else
                        newentry = FileObject_ToTableEntry(fobj, errstr)
                        If errstr = "" And newentry IsNot Nothing Then
                            aImage.HandleGenerator.AssignTo(newentry)
                            itable.AddToCollection(newentry)
                        Else
                            ErrorString = errstr
                        End If

                    End If

                Next

            Catch ex As Exception
                ErrorString = ex.Message
            End Try

            Return

        End Sub

        Private Function xObj_ToBlock(aFileObject As dxfFileObject, ByRef rTargets As dxfFileObjects, bConvertAttribsToText As Boolean, bSuppressReferences As Boolean,
                                          Optional aCollector As colDXFEntities = Nothing, Optional aHG As dxoHandleGenerator = Nothing,
                                          Optional aFilterList As List(Of String) = Nothing) As dxfBlock
            rTargets = Nothing
            ErrorString = ""
            If aFileObject Is Nothing Then
                ErrorString = "Invalid Input Detected"
                Return Nothing
            End If
            If aFileObject.Properties.Count <= 0 Then
                ErrorString = $"Invalid Input Detected - The Passed Block '{aFileObject.Name} ' Has No Properties"
                Return Nothing
            End If
            Dim bname As String = aFileObject.Properties.ValueS(2)
            If String.IsNullOrWhiteSpace(bname) Then
                ErrorString = "Invalid Input Detected - The Passed Block Has No Name"
                Return Nothing
            End If


            Dim _rVal As dxfBlock = Nothing
            Try
                _rVal = New dxfBlock(bname)
                Dim bprops As dxoProperties = _rVal.Properties
                bprops.CopyValues(aFileObject.Properties, bCopyNewMembers:=True)
                _rVal.Properties = bprops
                _rVal.Handle = aFileObject.Properties.ValueS(5)



                rTargets = New dxfFileObjects(aFileObject.SubObjects.OfType(Of dxfFileObject)().ToList(), dxxFileSections.Blocks, bname, Not bSuppressReferences)
                If Not bSuppressReferences Then
                    rTargets.SaveRefNames(_rVal)
                End If

                Dim eblk As dxfFileObject = rTargets.LastOrDefault
                If eblk IsNot Nothing Then
                    rTargets.Remove(eblk)
                    _rVal.EndBlockHandle = eblk.Handle
                End If

                Dim ients As colDXFEntities = xObjs_ToEntities(rTargets, bConvertAttribsToText, aCollector:=_rVal.Entities)
            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                If _rVal IsNot Nothing Then
                    If aHG IsNot Nothing Then
                        aHG.AssignTo(_rVal)
                    End If
                End If
            End Try
            Return _rVal

        End Function


        Friend Function FileObject_ToTableEntry(aFileObject As dxfFileObject, ByRef rErrorString As String) As dxfTableEntry
            rErrorString = ""
            Dim _rVal As dxfTableEntry = Nothing

            If aFileObject Is Nothing Then
                rErrorString = "Invalid Input Detected"
                Return Nothing
            End If
            If aFileObject.Properties.Count <= 0 Then
                rErrorString = "Invalid Input Detected - The Passed Object Has No Properties"
                Return Nothing
            End If
            Dim tname As String = aFileObject.Properties.ValueS(2)

            If aFileObject.ReferenceType = dxxReferenceTypes.UNDEFINED Then aFileObject.ReferenceType = dxfEnums.ReferenceTypeByName(tname)

            If aFileObject.ReferenceType = dxxReferenceTypes.UNDEFINED Then
                rErrorString = "Invalid Input Detected - The Passed File Object Is Not of a Recogized Type"
                Return Nothing
            End If

            Try
                Dim entry As New TTABLEENTRY(aFileObject.ReferenceType)

                entry.Props.CopyValues(aFileObject.Properties, bCopyNewMembers:=True)
                entry.Reactors = entry.Props.ExtractReactorGroups

                Select Case aFileObject.ReferenceType
                    Case dxxReferenceTypes.APPID
                        _rVal = New dxoAPPID()
                    Case dxxReferenceTypes.BLOCK_RECORD
                        _rVal = New dxoBlockRecord()
                    Case dxxReferenceTypes.DIMSTYLE
                        _rVal = New dxoDimStyle()
                    Case dxxReferenceTypes.LAYER
                        _rVal = New dxoLayer()
                    Case dxxReferenceTypes.LTYPE
                        _rVal = New dxoLinetype()
                    Case dxxReferenceTypes.STYLE
                        _rVal = New dxoStyle()
                    Case dxxReferenceTypes.UCS
                        _rVal = New dxoUCS()
                    Case dxxReferenceTypes.VIEW
                        _rVal = New dxoView()
                    Case dxxReferenceTypes.VPORT
                        _rVal = New dxoViewPort()

                End Select
                _rVal.Properties.CopyValues(aFileObject.Properties, bCopyNewMembers:=True)
                _rVal.Reactors = aFileObject.Properties.ExtractReactorGroups()


            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then
                    _rVal.Handle = aFileObject.Handle
                Else
                    rErrorString = $"Invalid Input Detected - The Passed File Entity Type '{tname}' Cannot Be Converted to a New Table Entry"
                End If
            End Try

            Return _rVal

        End Function

        Private Function xObj_ToEntity(aFileObject As dxfFileObject, aTargets As dxfFileObjects, Optional bConvertAttribsToText As Boolean = False, Optional bDontErrorOnUnsupported As Boolean = False) As dxfEntity
            Dim rUnsupportedType As Boolean
            Return xObj_ToEntity(aFileObject, aTargets, rUnsupportedType, bConvertAttribsToText, bDontErrorOnUnsupported)
        End Function

        Private Function xObj_ToEntity(aFileObject As dxfFileObject, aTargets As dxfFileObjects, ByRef rUnsupportedType As Boolean, Optional bConvertAttribsToText As Boolean = False, Optional bDontErrorOnUnsupported As Boolean = False) As dxfEntity
            ErrorString = ""
            Dim _rVal As dxfEntity = Nothing
            Dim ename As String = String.Empty
            Dim gtype As dxxGraphicTypes = dxxGraphicTypes.Undefined

            Dim eType As dxxEntityTypes = dxxEntityTypes.Undefined
            Try
                Dim sErr As String = String.Empty

                If Not xValidate_Entity(aFileObject, sErr, ename, gtype, eType, rUnsupportedType) Then
                    If rUnsupportedType And bDontErrorOnUnsupported Then
                        sErr = ""
                    End If
                    ErrorString = sErr
                    Return Nothing
                End If

                Select Case gtype

                    Case dxxGraphicTypes.Arc
                        _rVal = xObj_ToArc(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Line
                        _rVal = xObj_ToLine(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Ellipse
                        _rVal = xObj_ToEllipse(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Polyline
                        _rVal = xObj_ToPolyline(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Point
                        _rVal = xObj_ToPoint(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Hatch
                        _rVal = xObj_ToShape(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Shape
                        _rVal = xObj_ToShape(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Solid
                        _rVal = xObj_ToSolid(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Insert
                        _rVal = xObj_ToInsert(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.MText, dxxGraphicTypes.Text
                        _rVal = xObj_ToText(aFileObject, rErrorString:=sErr, bConvertAttribsToText)
                    Case dxxGraphicTypes.Dimension
                        _rVal = xObj_ToDim(aFileObject, eType, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Leader
                        _rVal = xObj_ToLeader(aFileObject, rErrorString:=sErr, aTargets:=aTargets)
                    Case dxxGraphicTypes.Hatch
                        _rVal = xObj_ToHatch(aFileObject, rErrorString:=sErr, aTargets:=aTargets)

                End Select

                ErrorString = sErr

            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                If _rVal IsNot Nothing Then
                    _rVal.SuppressEvents = False
                    _rVal.Handle = _rVal.Properties().ValueS(5)
                Else
                    If gtype <> dxxGraphicTypes.SequenceEnd Then
                        ErrorString = $"Invalid Input Detected - The Passed File Entity Type '{ename}' Cannot Be Converted to a New Enity"
                    End If

                End If
            End Try

            Return _rVal
        End Function


        Private Function xValidate_Entity(aFileObject As dxfFileObject, ByRef rErrorString As String, ByRef rName As String, ByRef rGraphicType As dxxGraphicTypes, ByRef rEntityType As dxxEntityTypes, ByRef rUnsupportedType As Boolean) As Boolean
            rName = ""
            rGraphicType = dxxGraphicTypes.Undefined
            rUnsupportedType = False
            rEntityType = dxxEntityTypes.Undefined
            If aFileObject Is Nothing Then
                rErrorString = "Invalid Input Detected"
                Return False
            End If


            If aFileObject.Properties.Count <= 0 Then
                rErrorString = "Invalid Input Detected - The Passed Entity Has No Properties"
                Return False
            End If


            rName = aFileObject.Properties.ValueS(0).ToUpper()
            If String.IsNullOrWhiteSpace(rName) Then
                rErrorString = "Invalid Input Detected - The Passed Entity Has No Enitity Type Name"
                Return False
            End If
            rGraphicType = dxfEnums.GraphicTypeByEntityTypeName(rName)
            If rGraphicType = dxxGraphicTypes.Undefined Then
                rErrorString = $"Invalid Input Detected - The Passed File Object Type '{rName}' Cannot Be Converted to a New Enity"
                rUnsupportedType = True
                Return False
            End If

            Select Case rGraphicType
                Case dxxGraphicTypes.Arc
                    If rName = "CIRCLE" Then rEntityType = dxxEntityTypes.Circle Else rEntityType = dxxEntityTypes.Arc
                Case dxxGraphicTypes.Line
                    rEntityType = dxxEntityTypes.Line
                Case dxxGraphicTypes.Ellipse
                    rEntityType = dxxEntityTypes.Ellipse
                Case dxxGraphicTypes.Polyline
                    rEntityType = dxxEntityTypes.Polyline
                Case dxxGraphicTypes.Point
                    rEntityType = dxxEntityTypes.Point
                Case dxxGraphicTypes.Shape
                    rEntityType = dxxEntityTypes.Shape
                Case dxxGraphicTypes.Solid
                    If rName = "TRACE" Then rEntityType = dxxEntityTypes.Trace Else rEntityType = dxxEntityTypes.Solid

                Case dxxGraphicTypes.Insert
                    rEntityType = dxxEntityTypes.Insert
                Case dxxGraphicTypes.MText
                    rEntityType = dxxEntityTypes.MText
                Case dxxGraphicTypes.Text
                    Select Case rName
                        Case "TEXT"
                            rEntityType = dxxEntityTypes.Text
                        Case "ATTDEF"
                            rEntityType = dxxEntityTypes.Attdef
                        Case "ATTRIB"
                            rEntityType = dxxEntityTypes.Attribute
                    End Select
                Case dxxGraphicTypes.Hatch
                    rEntityType = dxxEntityTypes.Hatch

                Case dxxGraphicTypes.Dimension
                    rEntityType = dxfUtils.DecodeDimensionType(aFileObject.Properties.ValueI(70, 1), aFileObject.Properties.ValueD(50))
                    If rEntityType < dxxEntityTypes.DimLinearH Or rEntityType > dxxEntityTypes.DimAngular3P Then
                        rErrorString = "The Passed File Object Is Not a Recognized Dimension"
                        rUnsupportedType = True
                        Return False
                    End If

                Case dxxGraphicTypes.Leader
                    If aFileObject.Properties.ValueI(72) = 1 Then
                        rErrorString = "Spline Leaders are Not Supported"
                        Return False 'spline leaders
                    End If
            End Select

            Return True
        End Function


        Private Function xObj_ToDim(aObj As dxfFileObject, eType As dxxEntityTypes, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeDimension
            rErrorString = ""


            Dim _rVal As New dxeDimension()


            Try


                Dim dstyle As dxfFileObject = GetTableEntry(dxxReferenceTypes.DIMSTYLE, aObj.Properties.ValueS(3))
                Dim block As dxfFileObject = Blocks.SubObjects.Find(Function(x) String.Compare(x.Name, aObj.Properties.ValueS(2), True) = 0)
                Dim brecord As dxfFileObject = GetTableEntry(dxxReferenceTypes.BLOCK_RECORD, aObj.Properties.ValueS(3))


                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)

                Dim bUserTxt As Boolean

                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                Dim aStr As String = String.Empty
                'determine the dimension type
                _rVal.SuppressEvents = True
                _rVal.Handle = aObj.Handle
                '_rVal.DisplayStructure = aObj.DisplayVars
                'get the dim properties
                _rVal.PlaneV = aPlane

                _rVal.SetPropVal("*UserPositionedText", bUserTxt, False)

                _rVal.SetPropVal("*EntityType", eType, False)
                _rVal.TextLineSpacingFactor = aObj.Properties.ValueD(41)
                aStr = aObj.Properties.ValueS(1).Trim()
                If aStr <> "" And aStr <> "<>" Then _rVal.OverideText = aStr
                _rVal.DefPt10V = aObj.Properties.ValueV(10) 'defpt 10
                _rVal.DefPt11V = aPlane.WorldVector(aObj.Properties.ValueV(11)) 'defpt 11
                _rVal.DefPt13V = aObj.Properties.ValueV(13)
                _rVal.DefPt14V = aObj.Properties.ValueV(14)
                _rVal.DefPt15V = aObj.Properties.ValueV(15)
                _rVal.DefPt16V = aPlane.WorldVector(aObj.Properties.ValueV(15))
                _rVal.PlaneV = aPlane

                Dim style As dxoDimStyle = _rVal.DimStyle


                If dstyle IsNot Nothing Then

                    style.Properties.CopyVals(dstyle.Properties, bSkipHandles:=False, bSkipPointers:=False)
                End If
                Dim stypeprops As dxoProperties = style.Properties

                For Each eprops As dxoProperties In aObj.ExtendedData
                    If String.Compare(eprops.ValueS(1000), "DSTYLE", True) = 0 Then
                        Dim eprop As dxoProperty
                        For i = 1 To eprops.Count
                            eprop = eprops.Item(i)
                            If eprop.GroupCode = 1070 Then
                                Dim gc As Integer = TVALUES.ToInteger(eprop.Value)
                                i += 1
                                eprop = eprops.Item(i)
                                If eprop Is Nothing Then Exit For
                                Dim eval As Object = eprop.Value
                                Dim dprop As dxoProperty = Nothing
                                If stypeprops.TryGet(gc, dprop) Then
                                    dprop.CopyValue(eprop)
                                End If

                            End If
                        Next
                        Exit For
                    End If
                Next
                style.Properties = stypeprops
                _rVal.DimStyle = style

                _rVal.SetPropVal("*BlockName", "")
                _rVal.BlockGUID = ""
                Dim sents As New dxfEntities()
                If block IsNot Nothing Then
                    If block.Name <> "" Then
                        If block.Name.ToUpper.StartsWith("*D") Then
                            _rVal.SetPropVal("*BlockName", block.Name)

                            Dim targets As New dxfFileObjects(block.SubObjects.OfType(Of dxfFileObject)().ToList(), dxxFileSections.Blocks, block.Name, False)
                            Do Until targets.EOF
                                Dim fent As dxfFileObject = targets.NextObject()
                                Dim gtype As dxxGraphicTypes = dxfEnums.GraphicTypeByEntityTypeName(fent.Properties.ValueS(0))
                                If gtype <> dxxGraphicTypes.EndBlock Then

                                    Dim ent As dxfEntity = xObj_ToEntity(fent, targets)
                                    If ent IsNot Nothing Then
                                        sents.Add(ent)
                                    Else

                                        sents.Clear()
                                        Exit Do
                                    End If
                                End If


                            Loop

                        End If
                    End If
                End If
                If sents.Count > 0 Then
                    _rVal.PathEntities = sents
                    _rVal.IsDirty = False
                Else
                    _rVal.IsDirty = True
                End If


            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToLeader(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeLeader
            rErrorString = ""
            Dim ltype As dxxLeaderTypes = aObj.Properties.ValueI(73)
            Dim _rVal As New dxeLeader(ltype)

            Try

                '#1the DXF object that carries the properties to define a new dxeDimension
                '#2the file to use for style and block references
                '^Defines a new dxeLeader based on the properties of the passed objects.
                Dim dstyle As dxfFileObject = GetTableEntry(dxxReferenceTypes.DIMSTYLE, aObj.Properties.ValueS(3))
                Dim block As dxfFileObject = Blocks.SubObjects.Find(Function(x) String.Compare(x.Name, aObj.Properties.ValueS(2), True) = 0)
                Dim brecord As dxfFileObject = GetTableEntry(dxxReferenceTypes.BLOCK_RECORD, aObj.Properties.ValueS(3))

                ' _rVal.DefineByFileObject(aObj, False, dstyle, block)

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)


                'Dim bUserTxt As Boolean

                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))

                'determine the dimension type
                _rVal.SuppressEvents = True
                _rVal.Handle = aObj.Handle
                '_rVal.DisplayStructure = aObj.DisplayVars
                'get the dim properties
                _rVal.PlaneV = aPlane

                Dim aStr As String = String.Empty
                ''Dim bUserTxt As Boolean
                'determine the dimension type
                _rVal.SuppressEvents = True

                '_rVal.DisplayStructure = aObj.DisplayVars
                'get the dim properties
                _rVal.PlaneV = aPlane
                _rVal.SetPropVal("Leader Type", ltype, False)
                _rVal.SuppressArrowHead = aObj.Properties.ValueB(71)
                Dim v1 As dxfVector = aObj.Properties.Vector(213)
                If v1 IsNot Nothing Then
                    _rVal.XOffset = v1.X
                    _rVal.YOffset = v1.Y

                End If
                _rVal.HasHook = aObj.Properties.ValueB(75)
                _rVal.SuppressHook = (_rVal.EntityType = dxxEntityTypes.LeaderBlock) Or Not _rVal.HasHook
                'get the vertices
                Dim verts As New colDXFVectors(aObj.Properties.VerticesV(10))
                _rVal.Vertices = verts
                Dim style As dxoDimStyle = _rVal.DimStyle

                If dstyle IsNot Nothing Then
                    style.Properties.CopyVals(dstyle.Properties, bSkipHandles:=False, bSkipPointers:=False)
                End If
                Dim stypeprops As dxoProperties = style.Properties

                For Each eprops As dxoProperties In aObj.ExtendedData
                    If String.Compare(eprops.ValueS(1000), "DSTYLE", True) = 0 Then
                        Dim eprop As dxoProperty
                        For i = 1 To eprops.Count
                            eprop = eprops.Item(i)
                            If eprop.GroupCode = 1070 Then
                                Dim gc As Integer = eprop.ValueI
                                i += 1
                                If i > eprops.Count Then Exit For

                                eprop = eprops.Item(i, bSuppressIndexError:=True)
                                If eprop Is Nothing Then Exit For
                                Dim eval As Object = eprop.Value
                                Dim dprop As dxoProperty = Nothing
                                If stypeprops.TryGet(gc, dprop) Then
                                    dprop.CopyValue(eprop)
                                End If

                            End If
                        Next
                        Exit For
                    End If
                Next
                style.Properties = stypeprops
                _rVal.DimStyle = style

                If aTargets IsNot Nothing Then
                    If _rVal.LeaderType = dxxLeaderTypes.LeaderText Then
                        'look for the mtext of the leader and give it t the leader
                        Dim thndl = aObj.Properties.ValueS(340)
                        If Not String.IsNullOrWhiteSpace(thndl) Then
                            Dim ftxt As dxfFileObject = aTargets.Find(Function(x) x.Handle = aObj.Properties.ValueS(340))
                            If ftxt IsNot Nothing Then
                                ftxt.Ignore = True 'tell the main loop to skip this one (if it occured prior to the leader in the file it will already be sent to the requestor)
                                If ftxt.GraphicType = dxxGraphicTypes.MText Then
                                    _rVal.TextString = ftxt.Properties.ValueS(1)
                                    _rVal.MText = xObj_ToText(ftxt, "", False)
                                End If

                            End If

                        End If

                    End If
                Else

                End If


            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObjs_ToEntities(aTargets As dxfFileObjects, bConvertAttribsToText As Boolean,
                                          Optional aCollector As colDXFEntities = Nothing, Optional aHG As dxoHandleGenerator = Nothing,
                                          Optional aFilterList As List(Of String) = Nothing) As colDXFEntities


            If aTargets Is Nothing Then Return Nothing

            Dim ientities As colDXFEntities = aCollector
            Dim sewuz As Boolean
            Dim bjustadd As Boolean
            If ientities Is Nothing Then ientities = New colDXFEntities
            If aTargets.Count <= 0 Then Return ientities

            sewuz = ientities.SuppressEvents
            ientities.SuppressEvents = True
            bjustadd = ientities.Count <= 0

            Dim forents As Boolean = False
            If aTargets.FileSection = dxxFileSections.Entities Then
                forents = True
            End If
            Try

                aTargets.Pointer = 0
                Do Until aTargets.EOF

                    Try


                        Dim fent As dxfFileObject = aTargets.NextObject
                        If fent Is Nothing Then Exit Do
                        If fent.Ignore Then Continue Do

                        If aFilterList IsNot Nothing Then
                            If aFilterList.FindIndex(Function(x) String.Compare(x, fent.Name, True) = 0) >= 0 Then Continue Do
                        End If
                        Dim ient As dxfEntity = xObj_ToEntity(fent, aTargets, bConvertAttribsToText, False)
                        If ient Is Nothing Then Continue Do


                        Dim exst As dxfEntity = Nothing
                        If Not bjustadd And aCollector IsNot Nothing Then
                            exst = aCollector.Find(Function(x) x.EntityType = ient.EntityType And x.Handle = ient.Handle)
                        End If

                        If exst Is Nothing Then
                            aHG?.AssignTo(ient)
                            ientities.AddToCollection(ient)

                        Else
                            exst.Properties.CopyValues(fent.Properties, bCopyNewMembers:=True, aGCsToSkip:="5")
                            ient = exst
                        End If




                        If aTargets.CollectReferenceNames Then
                            aTargets.SaveRefNames(ient)

                        End If

                    Catch ex As Exception
                        ErrorString = ex.Message
                        Continue Do
                    End Try



                Loop



            Catch ex As Exception
                ErrorString = ex.Message
            Finally
                ientities.SuppressEvents = sewuz

            End Try

            Return ientities
        End Function
        Private Function xObj_ToArc(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeArc
            rErrorString = ""


            Dim _rVal As New dxeArc

            Try

                _rVal.Reactors = New dxoPropertyArray(aObj.Reactors)
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)

                '    Structure = ent_DefineProperties(Structure, props, bNoHandle)
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane

                _rVal.CenterV = aPlane.WorldVector(aObj.Properties.ValueV(10))
                If String.Compare(aObj.Properties.ValueS(0), "ARC", True) = 0 Then
                    _rVal.StartAngle = aObj.Properties.ValueD(50, aDefault:=_rVal.StartAngle)
                    _rVal.EndAngle = aObj.Properties.ValueD(51, aDefault:=_rVal.EndAngle)
                End If

            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try

            Return _rVal
        End Function

        Private Function xObj_ToLine(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeLine
            rErrorString = ""

            Dim _rVal As New dxeLine


            Try

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)



                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane
                _rVal.StartPtV = aObj.Properties.ValueV(10)
                _rVal.EndPtV = aObj.Properties.ValueV(11)

            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToEllipse(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeEllipse
            rErrorString = ""
            Dim _rVal As New dxeEllipse

            Try

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)



                Dim cp As TVECTOR = aObj.Properties.ValueV(10)
                Dim xep As TVECTOR = cp + aObj.Properties.ValueV(11)
                Dim bFlag As Boolean
                Dim xDir As TVECTOR = cp.DirectionTo(xep, False, bFlag)
                Dim zDir As TVECTOR = aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ)
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(zDir)
                If bFlag Then xDir = aPlane.XDirection
                Dim yDir As TVECTOR = zDir.CrossProduct(xDir, True)
                aPlane = aPlane.ReDefined(cp, xDir, yDir)

                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane
                _rVal.CenterV = cp
                Dim rad1 As Double = dxfProjections.DistanceTo(cp, xep)
                Dim rat As Double = aObj.Properties.ValueD(40)
                Dim rad2 As Double = rat * rad1
                TVALUES.SortTwoValues(True, rad1, rad2)

                _rVal.Properties.SetVal("*MinorRadius", rad1)
                _rVal.Properties.SetVal("*MajorRadius", rad2)
                Dim ang1 As Double = aObj.Properties.ValueD(41, aDefault:=0)
                Dim ang2 As Double = aObj.Properties.ValueD(42, aDefault:=2 * Math.PI)
                Do While ang1 > 2 * Math.PI
                    ang1 -= (2 * Math.PI)
                Loop
                Do While ang2 > 2 * Math.PI
                    ang2 -= (2 * Math.PI)
                Loop
                Dim va As Double = -_rVal.Radius
                Dim vb As Double = -_rVal.MinorRadius


                Dim v1 As New TVECTOR(aPlane, va * Math.Cos(ang1), vb * Math.Cos(ang1))
                Dim aDir As TVECTOR = cp.DirectionTo(v1)
                Dim vVal As Double = aPlane.XDirection.AngleTo(aDir, aPlane.ZDirection)
                If ang1 > 0 And ang1 < 0.5 * Math.PI Then
                    _rVal.StartAngle = vVal - 180
                ElseIf ang1 > 0.5 * Math.PI And ang1 < Math.PI Then
                    _rVal.StartAngle = vVal - 180
                ElseIf ang1 > Math.PI And ang1 < 1.5 * Math.PI Then
                    _rVal.StartAngle = 180 + vVal
                ElseIf ang1 > 1.5 * Math.PI And ang1 < 2 * Math.PI Then
                    _rVal.StartAngle = 180 + vVal
                Else
                    _rVal.StartAngle = ang1 * 180 / Math.PI
                End If
                v1.X = va * Math.Cos(ang2)
                v1.Y = vb * Math.Sin(ang2)
                v1 = New TVECTOR(aPlane, v1.X, v1.Y)
                aDir = cp.DirectionTo(v1)
                vVal = aPlane.XDirection.AngleTo(aDir, aPlane.ZDirection)
                If ang2 > 0 And ang2 < 0.5 * Math.PI Then
                    _rVal.EndAngle = vVal - 180
                ElseIf ang2 > 0.5 * Math.PI And ang2 < Math.PI Then
                    _rVal.EndAngle = vVal - 180
                ElseIf ang2 > Math.PI And ang2 < 1.5 * Math.PI Then
                    _rVal.EndAngle = 180 + vVal
                ElseIf ang2 > 1.5 * Math.PI And ang2 < 2 * Math.PI Then
                    _rVal.EndAngle = 180 + vVal
                Else
                    _rVal.EndAngle = ang2 * 180 / Math.PI
                End If

            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToPolyline(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxePolyline
            rErrorString = ""


            Dim _rVal As New dxePolyline
            Dim oname As String = aObj.Properties.ValueS(0)
            Try

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Handle = aObj.Handle
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))

                If oname = "POLYLINE" Then
                    Dim elev As Double = aObj.Properties.ValueD(30)
                    If elev <> 0 Then aPlane.Origin += aPlane.ZDirection * -elev
                    _rVal.PlaneV = aPlane
                    Dim aVal As Integer = aObj.Properties.ValueI(70)
                    Dim bitcodes As TVALUES = TVALUES.BitCode_Decompose(aVal)
                    _rVal.Closed = bitcodes.ContainsString("1")


                    If aTargets IsNot Nothing Then

                        Dim verts As New colDXFVectors
                        For j As Integer = aTargets.Pointer + 1 To aTargets.Count
                            Dim nextfent As dxfFileObject = aTargets.NextObject(False)
                            Dim vname As String = nextfent.Properties.ValueS(0).Trim().ToUpper()
                            If vname = "SEQEND" Then
                                aTargets.Pointer = j
                                Exit For
                            ElseIf vname = "VERTEX" Then
                                verts.Add(nextfent.Properties.Vector(10))

                            Else
                                aTargets.Pointer = j - 1
                                Exit For
                            End If
                        Next
                        _rVal.Vertices = verts
                    Else
                        rErrorString = $"Unable To Extact {oname}[{aObj.Properties.ValueS(5)}] Vertices"
                    End If
                Else

                    _rVal.Reactors = aObj.Reactors
                    _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                    _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)

                    Dim elev As Double = _rVal.Properties.ValueD(38)
                    If elev <> 0 Then aPlane.Origin += aPlane.ZDirection * -elev
                    '_rVal.DisplayStructure = aObj.DisplayVars

                    _rVal.PlaneV = aPlane
                    Dim aVal As Integer = _rVal.Properties.ValueI(70) '
                    Dim bVal As Integer = _rVal.Properties.ValueI(90) 'vertext count
                    If aVal > 128 Then
                        _rVal.PlineGen = True
                        aVal -= 128
                    End If
                    _rVal.Closed = aVal = 1


                    If bVal > 0 Then

                        Dim gWd As Double = _rVal.Properties.ValueD(43, aDefault:=-1)
                        If gWd < 0 Then gWd = -1
                        'get the vertices

                        _rVal.Vertices = New colDXFVectors(aObj.Properties.VerticesV(10, gWd, True, New dxfPlane(aPlane)))

                    End If
                End If


            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToHatch(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeHatch
            rErrorString = ""


            Dim _rVal As New dxeHatch


            Try

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)


                '    Structure = ent_DefineProperties(Structure, props, bNoHandle)
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane


            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function


        Private Function xObj_ToPoint(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxePoint
            rErrorString = ""


            Dim _rVal As New dxePoint


            Try

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)

                '    Structure = ent_DefineProperties(Structure, props, bNoHandle)
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane
                _rVal.Vector = aObj.Properties.ValueV(10)

            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToText(aObj As dxfFileObject, ByRef rErrorString As String, Optional bConvertAttribsToText As Boolean = False) As dxeText
            rErrorString = ""
            Dim aTextType As dxxTextTypes
            Dim oprops As dxoProperties = aObj.Properties
            Dim ename As String = oprops.ValueS(0)

            Select Case ename.ToUpper
                Case "MTEXT"
                    aTextType = dxxTextTypes.Multiline
                Case "ATTDEF"
                    If bConvertAttribsToText Then aTextType = dxxTextTypes.DText Else aTextType = dxxTextTypes.AttDef
                Case "ATTRIB"
                    If bConvertAttribsToText Then aTextType = dxxTextTypes.DText Else aTextType = dxxTextTypes.Attribute
                Case Else
                    aTextType = dxxTextTypes.DText
            End Select


            Dim _rVal As New dxeText(aTextType)


            Try
                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)


                '    Structure = ent_DefineProperties(Structure, props, bNoHandle)
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(oprops.ValueV(210, aDefault:=dxfVector.WorldZ))
                ''_rVal.DisplayStructure = aObj.DisplayVars


                Dim v1 As dxfVector
                Dim v2 As dxfVector
                Dim txt As String
                Dim txt1 As String
                Dim txt2 As String
                Dim txt3 As String
                Dim aLm As dxxMTextAlignments
                Dim ang As Double
                Dim vAlign As dxxTextJustificationsVertical
                Dim hAlign As dxxTextJustificationsHorizontal
                Dim styname As String = oprops.ValueS(7, aDefault:="Standard")
                Dim tstyle As dxfFileObject = GetTableEntry(dxxReferenceTypes.STYLE, styname)

                Dim k As Integer
                Select Case aTextType
                    Case dxxTextTypes.Multiline
                    Case Else
                        hAlign = oprops.ValueI(72, aDefault:=dxxTextJustificationsHorizontal.Left)
                        vAlign = oprops.ValueI(73, aDefault:=dxxTextJustificationsVertical.Baseline)
                End Select

                v1 = oprops.Vector(10, aDefault:=dxfVector.Zero)

                '=============================================
                If aTextType = dxxTextTypes.Multiline Then
                    '=============================================
                    _rVal.AlignmentPt1 = v1
                    _rVal.TextHeight = oprops.ValueD(40, aDefault:=0.2)
                    _rVal.Alignment = oprops.ValueI(71, aDefault:=dxxMTextAlignments.TopLeft)
                    'get the text angle
                    v2 = oprops.Vector(11)
                    If v2 IsNot Nothing Then
                        aPlane.AlignTo(v2, dxxAxisDescriptors.X)
                    Else
                        ang = oprops.ValueD(50)
                        _rVal.Rotation = ang * 180 / Math.PI
                    End If
                    _rVal.LineSpacingFactor = oprops.ValueD(44, aDefault:=1)
                    k = 1
                    txt1 = oprops.ValueS(1)
                    txt3 = oprops.ValueS(3)
                    If txt3 = "" Then
                        txt = txt1
                    Else
                        txt2 = "X"
                        k = 0
                        Do Until txt2 = ""
                            k += 1
                            txt2 = oprops.ValueS(3, k)
                            If txt2 <> "" Then txt3 += txt2
                        Loop
                        txt = txt3 & txt1
                    End If
                    txt = TFONT.CADTextToScreenText(txt)
                    If String.Compare(Left(txt, 1), "{", True) = 0 And String.Compare(Right(txt, 1), "}", True) = 0 Then
                        txt = Mid(txt, 2, txt.Length - 2)
                    End If
                    _rVal.TextString = txt
                    '=============================================
                Else
                    '=============================================
                    aLm = TFONT.DecodeAlignment(vAlign, hAlign)
                    _rVal.Alignment = aLm
                    _rVal.TextHeight = oprops.ValueD(40)
                    _rVal.WidthFactor = oprops.ValueD(41, aDefault:=1)
                    _rVal.ObliqueAngle = oprops.ValueD(51)
                    _rVal.Rotation = oprops.ValueD(50)
                    _rVal.LineSpacingFactor = oprops.ValueD(44, aDefault:=1)
                    txt = oprops.ValueS(1)
                    txt = TFONT.CADTextToScreenText(txt)
                    _rVal.TextString = txt
                    '           pln_Revolve aPlane, -ang
                    v1 = oprops.Vector(10)

                    v1 = New dxfVector(aPlane.WorldVector(v1.Strukture))
                    v2 = oprops.Vector(11)
                    If v2 Is Nothing Then
                        v2 = v1
                    Else

                        v2 = New dxfVector(aPlane.WorldVector(v2.Strukture))
                    End If
                    If aLm <> dxxMTextAlignments.Fit And aLm <> dxxMTextAlignments.Aligned Then
                        _rVal.AlignmentPt1V = v2.Strukture
                        _rVal.AlignmentPt2V = v1.Strukture
                    Else
                        _rVal.AlignmentPt1V = v1.Strukture
                        _rVal.AlignmentPt2V = v2.Strukture
                    End If
                    If aTextType = dxxTextTypes.AttDef Or aTextType = dxxTextTypes.Attribute Then
                        _rVal.Prompt = oprops.ValueS(3)
                        _rVal.AttributeTag = oprops.ValueS(2)
                        _rVal.AttributeType = oprops.ValueS(70)
                    End If
                End If
                _rVal.PlaneV = aPlane
            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToShape(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeShape
            rErrorString = ""

            Dim _rVal As New dxeShape


            Try

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)

                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane
                _rVal.InsertionPtV = aPlane.WorldVector(aObj.Properties.ValueV(10))
                _rVal.ShapeName = aObj.Properties.ValueS(2)

            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToSolid(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeSolid
            rErrorString = ""

            Dim _rVal As New dxeSolid


            Try

                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)

                '    Structure = ent_DefineProperties(Structure, props, bNoHandle)
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane

                Dim idx As Integer
                Dim v3 As dxfVector = aObj.Properties.Vector(12)
                Dim v4 As dxfVector = aObj.Properties.Vector(13, aDefault:=v3)
                Dim v1 As dxfVector = aObj.Properties.Vector(10)
                Dim v2 As dxfVector = aObj.Properties.Vector(11)

                _rVal.Vertex1V = New TVECTOR(aPlane, v1.X, v1.Y, v1.Z)
                _rVal.Vertex2V = New TVECTOR(aPlane, v2.X, v2.Y, v1.Z)
                _rVal.Vertex4V = New TVECTOR(aPlane, v4.X, v4.Y, v1.Z)
                If String.Compare(aObj.Properties.ValueS(0), "TRACE", True) = 0 Then
                    _rVal.Vertex3V = New TVECTOR(aPlane, v3.X, v3.Y)
                Else
                    If idx = 0 Then v3 = v4
                    _rVal.Vertex3V = New TVECTOR(aPlane, v3.X, v3.Y, v1.Z)
                    _rVal.Vertex4V = New TVECTOR(aPlane, v4.X, v4.Y, v1.Z)
                    _rVal.Triangular = v3.IsEqual(v4, 4)
                End If
                aPlane.Origin = v1.Strukture

            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Private Function xObj_ToInsert(aObj As dxfFileObject, ByRef rErrorString As String, Optional aTargets As dxfFileObjects = Nothing) As dxeInsert
            rErrorString = ""


            Dim _rVal As New dxeInsert


            Try
                _rVal.Reactors = aObj.Reactors
                _rVal.ExtendedData.Append(aObj.ExtendedData, bClear:=True)
                _rVal.Properties.CopyVals(aObj.Properties, bSkipHandles:=False, bSkipPointers:=False)


                '    Structure = ent_DefineProperties(Structure, props, bNoHandle)
                Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.ValueV(210, aDefault:=dxfVector.WorldZ))
                '_rVal.DisplayStructure = aObj.DisplayVars
                _rVal.PlaneV = aPlane

                Dim blok As dxfFileObject = GetBlock(aObj.Properties.ValueS(2), rErrorString)
                If rErrorString <> "" Then
                    Return Nothing
                End If
                _rVal.InsertionPtV = aPlane.WorldVector(aObj.Properties.ValueV(10))
                _rVal.XScaleFactor = aObj.Properties.ValueD(41, aDefault:=1)
                _rVal.YScaleFactor = aObj.Properties.ValueD(42, aDefault:=1)
                _rVal.ZScaleFactor = aObj.Properties.ValueD(43, aDefault:=1)
                _rVal.RotationAngle = aObj.Properties.ValueD(50)
                If aTargets IsNot Nothing Then

                    Dim attribs As New dxfAttributes

                    Dim fatribs As New List(Of dxfFileObject)
                    'get the attributes by reading forward to the sequence end
                    For j As Integer = aTargets.Pointer + 1 To aTargets.Count
                        Dim nextfent As dxfFileObject = aTargets.NextObject()
                        If nextfent.GraphicType = dxxGraphicTypes.SequenceEnd Then
                            aTargets.Pointer = j
                            Exit For
                        ElseIf nextfent.GraphicType = dxxGraphicTypes.Text Then
                            Dim converr As String = String.Empty
                            Dim nextent As dxeText = xObj_ToText(nextfent, converr)
                            If nextent IsNot Nothing Then
                                attribs.Add(New dxfAttribute(nextent))


                            End If
                        Else
                            aTargets.Pointer = j - 1
                            Exit For
                        End If
                    Next
                    _rVal.Attributes = attribs
                Else
                    rErrorString = $"Unable To Read Attributes for INSERT[{aObj.Properties.ValueS(5)}] "
                End If
            Catch ex As Exception
                rErrorString = ex.Message
                Return Nothing
            Finally
                If _rVal IsNot Nothing Then _rVal.SuppressEvents = False
            End Try



            Return _rVal
        End Function

        Friend Shared Function SubObjectType(aFileSection As dxxFileSections) As dxxFileObjectTypes

            Select Case aFileSection
                Case dxxFileSections.Blocks
                    Return dxxFileObjectTypes.Block
                Case dxxFileSections.Classes
                    Return dxxFileObjectTypes.DXFClass
                Case dxxFileSections.Entities
                    Return dxxFileObjectTypes.Entity
                Case dxxFileSections.Objects
                    Return dxxFileObjectTypes.DXFObject
                Case dxxFileSections.Tables
                    Return dxxFileObjectTypes.Table
                Case Else
                    Return dxxFileObjectTypes.Undefined
            End Select

        End Function


    End Class


    Public Class dxfFileEntry
        Public Property LineNo As Integer = -1
        Public Property GroupCode As Integer = -1
        Public Property Value As String = String.Empty
        Public Property PropertyType As dxxPropertyTypes = dxxPropertyTypes.Undefined
        Public Property FileSection As String = String.Empty
        Public Property Table As String = String.Empty
        Public Property BlockName As String = String.Empty

        Public Sub New(aLineNo As Integer, aGC As Integer, aValue As String, Optional aPropertyType As dxxPropertyTypes = dxxPropertyTypes.Undefined)
            LineNo = aLineNo
            GroupCode = aGC
            Value = aValue
            PropertyType = aPropertyType
        End Sub
        Public Overrides Function ToString() As String
            Dim _rVal As String = $"{GroupCode} : {Value} : {Path}"

            Return _rVal
        End Function
        Public ReadOnly Property Path As String
            Get
                Dim _rVal As String = FileSection
                If _rVal = "" Then Return String.Empty
                If FileSection = "TABLES" And Table <> "" Then
                    If _rVal <> "" Then _rVal += $".{Table}"
                End If
                If FileSection = "BLOCKS" And BlockName <> "" Then
                    If _rVal <> "" Then _rVal += $".{BlockName}"
                End If
                Return _rVal

            End Get
        End Property

        Friend ReadOnly Property Prop As TPROPERTY
            Get
                Dim gc As Integer = GroupCode
                Dim val As Object = Value
                Dim ptype As dxxPropertyTypes = PropertyType
                Dim lnno As Integer = LineNo
                Return New TPROPERTY(aCode:=gc, aValue:=val, aName:=$"Line {lnno}", aPropType:=ptype, aLastVal:=Nothing) With {.LineNo = lnno}

            End Get
        End Property
    End Class

    Friend Class dxfFileObjects
        Inherits List(Of dxfFileObject)

        Public Property Properties As dxoProperties
        Public Property Reactors As dxoPropertyArray
        Public Property Name As String

        Public Property Pointer As Integer

        Friend Property AppIds As List(Of String)
        Friend Property LayerNames As List(Of String)
        Friend Property StyleNames As List(Of String)
        Friend Property DStyleNames As List(Of String)
        Friend Property LTypeNames As List(Of String)

        Public Sub New()
            MyBase.New()
            Properties = New dxoProperties("")
            Reactors = New dxoPropertyArray("Reactors")
            FileSection = dxxFileSections.Unknown
            Name = ""
            Pointer = 1
            CollectReferenceNames = False
        End Sub

        Public Sub New(aName As String)
            MyBase.New()
            Properties = New dxoProperties("")
            Reactors = New dxoPropertyArray("Reactors")
            FileSection = dxxFileSections.Unknown
            Name = aName
            Pointer = 1
            CollectReferenceNames = False
        End Sub

        Public Sub New(aMembers As List(Of dxfFileObject), aFileSection As dxxFileSections, Optional aName As String = "", Optional bCollectReferenceNames As Boolean = False)
            MyBase.New()
            Properties = New dxoProperties("")
            Reactors = New dxoPropertyArray("Reactors")
            FileSection = aFileSection
            Pointer = 1
            Name = aName
            If aMembers IsNot Nothing Then AddRange(aMembers)
            CollectReferenceNames = bCollectReferenceNames
        End Sub

        Private _CollectReferenceNames As Boolean
        Friend Property CollectReferenceNames As Boolean
            Get
                Return _CollectReferenceNames
            End Get
            Set(value As Boolean)
                _CollectReferenceNames = value
                If Not value Then
                    AppIds = Nothing
                    LTypeNames = Nothing
                    LayerNames = Nothing
                    StyleNames = Nothing
                    DStyleNames = Nothing
                Else
                    AppIds = New List(Of String)
                    LTypeNames = New List(Of String)
                    LayerNames = New List(Of String)
                    StyleNames = New List(Of String)
                    DStyleNames = New List(Of String)

                End If
            End Set
        End Property

        Friend Sub SaveRefNames(aBlock As dxfBlock)
            If aBlock Is Nothing Or Not CollectReferenceNames Then Return
            If Not String.IsNullOrWhiteSpace(aBlock.LayerName) And Not LayerNames.Contains(aBlock.LayerName) Then LayerNames.Add(aBlock.LayerName)

        End Sub

        Friend Function TranserferReferences(aFile As dxfFile, aImage As dxfImage) As List(Of String)


            If Not CollectReferenceNames Or aFile Is Nothing Or aImage Is Nothing Then Return Nothing

            Return aFile.TransferToImage_References(aImage, LTypeNames, StyleNames, DStyleNames, LayerNames, aAPPIDNamesOrHandlesList:=AppIds)


        End Function

        Friend Sub SaveRefNames(aEntity As dxfEntity)
            If aEntity Is Nothing Or Not CollectReferenceNames Then Return
            'save extended data appids
            Dim xData As dxoPropertyArray = aEntity.ExtendedData
            For k As Integer = 1 To xData.Count
                If Not AppIds.Contains(xData.Item(k).Name) Then AppIds.Add(xData.Item(k).Name)
            Next k


            If Not String.IsNullOrWhiteSpace(aEntity.LayerName) And Not LayerNames.Contains(aEntity.LayerName) Then LayerNames.Add(aEntity.LayerName)
            If Not String.IsNullOrWhiteSpace(aEntity.Linetype) And Not LTypeNames.Contains(aEntity.Linetype) Then LTypeNames.Add(aEntity.Linetype)
            If Not String.IsNullOrWhiteSpace(aEntity.DimStyleName) And Not DStyleNames.Contains(aEntity.DimStyleName) Then DStyleNames.Add(aEntity.DimStyleName)
            If Not String.IsNullOrWhiteSpace(aEntity.TextStyleName) And Not StyleNames.Contains(aEntity.TextStyleName) Then StyleNames.Add(aEntity.TextStyleName)
        End Sub

        Public Function NextObject(Optional bDontAdvancePointer As Boolean = False) As dxfFileObject
            If EOF Then Return Nothing
            Dim _rVal As dxfFileObject = Item(Pointer - 1)
            If Not bDontAdvancePointer Then Pointer += 1
            Return _rVal
        End Function
        Public ReadOnly Property EOF As Boolean
            Get
                If Pointer < 1 Then Pointer = 1
                Return Count <= 0 Or Pointer > Count
            End Get
        End Property

        Public Property SubObjects As List(Of dxfFileObject) = Nothing

        Public Overrides Function ToString() As String
            Return $"dxfFileObjects [{Name}] ({Count})"
        End Function

        Public Function Names() As String
            If SubObjects Is Nothing Then Return String.Empty
            Dim _rVal As String = String.Empty

            For Each obj As dxfFileObject In SubObjects
                TLISTS.Add(_rVal, obj.Name)
            Next
            Return _rVal
        End Function

        Public Property FileSection As dxxFileSections

        Public Function Structure_Get() As TOBJECTS
            Dim _rVal As New TOBJECTS(Name) With
                {
                .Properties = New TPROPERTIES(Properties),
                .Reactors = Reactors.Structure_Get,
                .Name = Name
            }
            If SubObjects Is Nothing Then Return _rVal
            For Each sobj As dxfFileObject In SubObjects
                TLISTS.Add(_rVal.Name, sobj.Name)
                _rVal.Add(sobj.Structure_Get())
            Next

            Return _rVal
        End Function


        Public ReadOnly Property SubObjectType As dxxFileObjectTypes
            Get
                Return dxfFile.SubObjectType(FileSection)
            End Get
        End Property
    End Class

    Friend Class dxfFileSubObject
        Inherits dxfFileObject


        Public Overloads ReadOnly Property Name As String
            Get
                If FileSection <> dxxFileSections.Blocks Then
                    Return Properties.ValueS(2)
                Else
                    Return Properties.ValueS(0)
                End If

            End Get
        End Property


        Public Overloads ReadOnly Property Handle As String
            Get
                If FileSection <> dxxFileSections.Tables Then
                    Return Properties.ValueS(5)
                Else
                    If ReferenceType = dxxReferenceTypes.DIMSTYLE Then
                        Return Properties.ValueS(105)
                    Else
                        Return Properties.ValueS(5)
                    End If
                End If
            End Get
        End Property


        Public Sub New()
            MyBase.New()

            Structure_Set(New TOBJECT(""))
            FileSection = dxxFileSections.Unknown
        End Sub

        Public Sub New(aName As String)
            MyBase.New()

            Structure_Set(New TOBJECT(aName))
            FileSection = dxxFileSections.Unknown
        End Sub

        Public Sub New(aFileSection As dxxFileSections, aProperties As dxoProperties)


            FileSection = aFileSection

            Properties = New dxoProperties
            If aProperties IsNot Nothing Then
                Properties = aProperties
                Reactors = Properties.ExtractReactorGroups()
                ExtendedData = Properties.ExtractExtendedData()
            End If


        End Sub


        Public Overrides Function ToString() As String
            Return $"dxfFileSubObject [{Name }]"
        End Function



        Public Overloads Function Structure_Get() As TOBJECT
            Dim _rVal As New TOBJECT(Name) With
                {
                .Properties = New TPROPERTIES(Properties),
                .Reactors = Reactors.Structure_Get,
                .Name = Name
            }


            Return _rVal
        End Function
        Public Overloads Sub Structure_Set(aMember As TOBJECT)

            Properties = New dxoProperties(aMember.Properties, Name)

            Reactors.Structure_Set(aMember.Reactors)

        End Sub

        Public ReadOnly Property SubObjectType As dxxFileObjectTypes
            Get
                Select Case FileSection
                    Case dxxFileSections.Blocks
                        Return dxxFileObjectTypes.Block
                    Case dxxFileSections.Classes
                        Return dxxFileObjectTypes.DXFClass
                    Case dxxFileSections.Entities
                        Return dxxFileObjectTypes.Entity
                    Case dxxFileSections.Objects
                        Return dxxFileObjectTypes.DXFObject
                    Case dxxFileSections.Tables
                        Return dxxFileObjectTypes.Table
                    Case Else
                        Return dxxFileObjectTypes.Undefined
                End Select
            End Get
        End Property

        Public Overloads Function PropValueString(aName As String, Optional aOccur As Integer = 1)
            Dim prop As dxoProperty = Nothing
            If Properties.TryGet(aName, prop, aOccur) Then Return prop.ValueString Else Return String.Empty
        End Function
        Public Overloads Function PropValueString(aGroupCode As Integer, Optional aOccur As Integer = 1)
            Dim prop As dxoProperty = Nothing
            If Properties.TryGet(aGroupCode, prop, aOccur) Then Return prop.ValueString Else Return String.Empty
        End Function
    End Class
    Friend Class dxfFileSection
        Public Property Properties As dxoProperties


        Public Sub New(aFileSection As dxxFileSections)


            FileSection = aFileSection
            Properties = New dxoProperties

        End Sub


        Public Sub New(aFileSection As dxxFileSections, aProperties As dxoProperties)


            FileSection = aFileSection
            If aProperties Is Nothing Then Properties = New dxoProperties Else Properties = aProperties
        End Sub

        Private _SubObjects As New dxfFileObjects
        Public ReadOnly Property SubObjectType As dxxFileObjectTypes
            Get
                Return dxfFile.SubObjectType(FileSection)
            End Get
        End Property

        Public Property SubObjects As dxfFileObjects
            Get

                Return _SubObjects
            End Get
            Set(value As dxfFileObjects)
                If value Is Nothing Then _SubObjects.Clear() Else _SubObjects = value
            End Set
        End Property


        Public Property FileSection As dxxFileSections


        Public ReadOnly Property Name As String
            Get
                Return dxfEnums.Description(FileSection)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"dxfFileSection - {Name}"
        End Function

        Public ReadOnly Property ObjectType As dxxFileObjectTypes = dxfFile.SubObjectType(FileSection)

        Public Function AddSubObject(aProperties As dxoProperties) As dxfFileObject
            If aProperties Is Nothing Then Return Nothing
            Dim obj As New dxfFileObject(FileSection, aProperties)
            SubObjects.Add(obj)
            Return obj

        End Function

        Friend Function GetSubObjects(aNamesList As String, ByRef rErrorCollector As List(Of String)) As List(Of dxfFileObject)
            If String.IsNullOrWhiteSpace(aNamesList) Then Return SubObjects
            Dim names As TVALUES = TVALUES.FromList(aNamesList, bTrimmed:=True, bNoNulls:=True, bUnique:=True)
            If names.Count <= 0 Then Return New List(Of dxfFileObject)

            Dim _rVal As List(Of dxfFileObject) = SubObjects.FindAll(Function(x) names.ContainsString(x.Name) Or names.ContainsString(x.Handle))
            If rErrorCollector IsNot Nothing And names.Count <> _rVal.Count Then
                For i As Integer = 1 To names.Count
                    Dim sval As String = names.StringVal(i)
                    If _rVal.FindIndex(Function(x) String.Compare(x.Name, sval, True) = 0 Or String.Compare(x.Handle, sval, True) = 0) < 0 Then
                        rErrorCollector.Add($"Sub-Object '{sval}' Was Not Found")
                    End If
                Next

            End If
            Return _rVal
        End Function

        Friend Function GetTargets(aNamesList As String, ByRef rErrorCollector As List(Of String), Optional bCollectReferences As Boolean = False) As dxfFileObjects
            If String.IsNullOrWhiteSpace(aNamesList) Then Return New dxfFileObjects(SubObjects, FileSection, bCollectReferenceNames:=bCollectReferences)

            Dim names As TVALUES = TVALUES.FromList(aNamesList, bTrimmed:=True, bNoNulls:=True, bUnique:=True)
            If names.Count <= 0 Then Return New List(Of dxfFileObject)

            Dim _rVal As New dxfFileObjects(SubObjects.FindAll(Function(x) names.ContainsString(x.Name) Or names.ContainsString(x.Handle)), FileSection, bCollectReferenceNames:=bCollectReferences)
            If rErrorCollector IsNot Nothing And names.Count <> _rVal.Count Then
                For i As Integer = 1 To names.Count
                    Dim sval As String = names.StringVal(i)
                    If _rVal.FindIndex(Function(x) String.Compare(x.Name, sval, True) = 0 Or String.Compare(x.Handle, sval, True) = 0) < 0 Then
                        rErrorCollector.Add($"Sub-Object '{sval}' Was Not Found")
                    End If
                Next

            End If
            Return _rVal
        End Function

    End Class

    Friend Class dxfFileObject


        Public Sub New()
            GraphicType = dxxGraphicTypes.Undefined
            ReferenceType = dxxReferenceTypes.UNDEFINED
            _SubObjects = New List(Of dxfFileSubObject)
            FileSection = dxxFileSections.Unknown
            Properties = New dxoProperties
            Reactors = New dxoPropertyArray("Reactors")
            ExtendedData = New dxoPropertyArray("ExtentedData")
        End Sub

        Public Sub New(aFileSection As dxxFileSections)
            GraphicType = dxxGraphicTypes.Undefined
            ReferenceType = dxxReferenceTypes.UNDEFINED
            _SubObjects = New List(Of dxfFileSubObject)
            FileSection = aFileSection
            Properties = New dxoProperties
            Reactors = New dxoPropertyArray("Reactors")
            ExtendedData = New dxoPropertyArray("ExtentedData")
            _ObjectType = dxfFile.SubObjectType(FileSection)
        End Sub

        Friend Sub New(aSubObject As dxfFileSubObject)
            GraphicType = dxxGraphicTypes.Undefined
            ReferenceType = dxxReferenceTypes.UNDEFINED
            _SubObjects = New List(Of dxfFileSubObject)
            FileSection = aSubObject?.FileSection
            Properties = New dxoProperties
            Reactors = New dxoPropertyArray("Reactors")
            ExtendedData = New dxoPropertyArray("ExtentedData")
            _ObjectType = dxfFile.SubObjectType(FileSection)
            If aSubObject IsNot Nothing Then
                Properties = aSubObject.Properties
                Reactors = aSubObject.Reactors
            End If
        End Sub

        Public ReadOnly Property DisplayVars As TDISPLAYVARS
            Get
                Return TDISPLAYVARS.FromEntityProperties(Properties)
            End Get
        End Property

        Public Sub New(aFileSection As dxxFileSections, aProperties As dxoProperties)


            FileSection = aFileSection

            Properties = New dxoProperties
            Reactors = New dxoPropertyArray("Reactors")
            ExtendedData = New dxoPropertyArray("ExtentedData")
            If aProperties IsNot Nothing Then
                Properties = aProperties
                Reactors = Properties.ExtractReactorGroups()
                ExtendedData = Properties.ExtractExtendedData()
            End If
            _SubObjects = New List(Of dxfFileSubObject)
            _ObjectType = dxfFile.SubObjectType(FileSection)

        End Sub


        Public Property ImageGUID As String



        Public Property ExtendedData As dxoPropertyArray
        Public Property Properties As dxoProperties
        Public Property Reactors As dxoPropertyArray
        Public Property SectionName As String = dxfEnums.Description(FileSection)
        Public Property ReferenceType As dxxReferenceTypes
        Public Property GraphicType As dxxGraphicTypes
        Public Property DXFObjectType As dxxObjectTypes
        Public Property FileSection As dxxFileSections

        Public Property Ignore As Boolean = False

        Private _ObjectType As dxxFileObjectTypes = dxxFileObjectTypes.Undefined
        Public Property ObjectType As dxxFileObjectTypes
            Get
                Return _ObjectType
            End Get
            Set(value As dxxFileObjectTypes)
                _ObjectType = value
            End Set
        End Property

        Public ReadOnly Property HandleGroupCode As Integer
            Get
                If ReferenceType = dxxReferenceTypes.DIMSTYLE Then Return 105 Else Return 5
            End Get
        End Property


        Public ReadOnly Property Name As String
            Get
                If FileSection <> dxxFileSections.Entities Then
                    Return Properties.ValueS(2)
                Else
                    Return Properties.ValueS(0)
                End If

            End Get
        End Property

        Public ReadOnly Property Handle As String
            Get

                Return Properties.ValueS(HandleGroupCode)

            End Get
        End Property


        Private _SubObjects As New List(Of dxfFileSubObject)
        Public ReadOnly Property SubObjects As List(Of dxfFileSubObject)
            Get
                Return _SubObjects
            End Get
        End Property




        Public Function Structure_Get_SubObjects() As TOBJECTS
            Dim _rVal As New TOBJECTS(Name) With
            {
            .Properties = New TPROPERTIES(Properties),
           .Reactors = Reactors.Structure_Get()
            }
            For Each sobj As dxfFileSubObject In SubObjects
                _rVal.Add(sobj.Structure_Get())
            Next

            Return _rVal
        End Function

        Public Function Structure_Get() As TOBJECT
            Dim _rVal As New TOBJECT(Name) With
            {
  .DisplayVars = DisplayVars,
            .ExtendedData = ExtendedData.Structure_Get,
            .ImageGUID = ImageGUID,
            .Properties = New TPROPERTIES(Properties),
            .Reactors = Reactors.Structure_Get,
            .Section = SectionName
            }
            Return _rVal
        End Function



        Public Function AddSubObject(aProperties As dxoProperties) As dxfFileSubObject
            If aProperties Is Nothing Then Return Nothing
            Dim obj As New dxfFileSubObject(FileSection, aProperties)

            SubObjects.Add(obj)
            Return obj

        End Function

        Public Function PropValueString(aName As String, Optional aOccur As Integer = 1)
            Dim prop As dxoProperty = Nothing
            If Properties.TryGet(aName, prop, aOccur) Then Return prop.ValueString Else Return String.Empty
        End Function
        Public Function PropValueString(aGroupCode As Integer, Optional aOccur As Integer = 1)
            Dim prop As dxoProperty = Nothing
            If Properties.TryGet(aGroupCode, prop, aOccur) Then Return prop.ValueString Else Return String.Empty
        End Function

        Public Overrides Function ToString() As String
            Return $"dxfFileObject [{dxfEnums.Description(ObjectType) }] - {Name}"
        End Function
    End Class

    Public Class dxfFiles
        Inherits List(Of dxfFile)

        Friend Sub New()
            MyBase.New()
        End Sub

        Friend Function GetByFileName(aFileName As String, ByRef rIndex As Integer) As dxfFile
            rIndex = -1


            If String.IsNullOrWhiteSpace(aFileName) Then Return Nothing
            For Each file As dxfFile In Me
                If String.Compare(file.FileName, aFileName, ignoreCase:=True) = 0 Or
               String.Compare(IO.Path.GetFileName(file.FileSpec), aFileName, ignoreCase:=True) = 0 Or
               String.Compare(IO.Path.GetFileNameWithoutExtension(file.FileSpec), aFileName, ignoreCase:=True) = 0 Then
                    rIndex = MyBase.IndexOf(file) + 1
                    Return file
                End If
            Next

            Return Nothing
        End Function

        Public Shadows Function Item(aIndex As Integer) As dxfFile
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            Return MyBase.Item(aIndex - 1)
        End Function
    End Class
End Namespace

