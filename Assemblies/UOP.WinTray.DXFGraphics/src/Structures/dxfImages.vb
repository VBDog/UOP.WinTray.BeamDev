
Imports UOP.DXFGraphics.dxfGlobals


Namespace UOP.DXFGraphics.Structures

#Region "Structures"
    Friend Structure TIMAGE
            Implements ICloneable
#Region "Members"
            Public Clears As Long
            Public CollectErrors As Boolean
            Public Erasing As Boolean

            Public FileType As dxxFileTypes
            Public FolderPath As String
            Public GroupName As String
            Private _GUID As String
            Public IsDirty As Boolean
            Private _Name As String

            Public RaiseErrors As Boolean
            Public RegenList As String
            Public Rendering As Boolean
            Public SelectionStartID As Long
            Public Status As String
            Public SuppressFromFile As Boolean
            Public Tag As String
            Public Errors As TVALUES
            Public obj_CLASSES As TDXFCLASSES
            Public obj_DISPLAY As TDISPLAY
            Public obj_OBJECTS As TDXFOBJECTS
            Public obj_SCREEN As TSCREEN
            Public obj_SHAPES As TSHAPEARRAY
            Public obj_THUMBNAIL As TPROPERTIES
            Public obj_UCS As TPLANE
            Public pth_UCSICON As TPATHS
        Public set_HEADER As TTABLEENTRY
        Public set_DIM As TTABLEENTRY
        Public set_DIMOVERRIDES As TTABLEENTRY
        Public set_LTYPES As TTABLEENTRY
        Public set_SYMBOL As TTABLEENTRY
            Public set_TABLE As TTABLEENTRY
            Public set_TEXT As TTABLEENTRY
            Public tbl_APPID As TTABLE
            Public tbl_BLOCKRECORD As TTABLE
            Public tbl_DIMSTYLE As TTABLE
            Public tbl_LAYER As TTABLE
            Public tbl_LTYPE As TTABLE
            Public tbl_STYLE As TTABLE
            Public tbl_UCS As TTABLE
            Public tbl_VIEW As TTABLE
            Public tbl_VPORT As TTABLE
#End Region 'Members
#Region "Constructors"
            Public Sub New(Optional aName As String = "")
                'init ------------------------
                Clears = 0
                CollectErrors = False
                Erasing = False
                _GUID = ""
                Tag = ""
                Status = ""
                FileType = dxxFileTypes.DXF
                FolderPath = ""
                GroupName = ""
                IsDirty = False
                _Name = ""
                RaiseErrors = False
                RegenList = ""
                Rendering = False
                SelectionStartID = 0
                Status = ""
                SuppressFromFile = False
                Tag = ""

                Errors = New TVALUES(0)
                obj_CLASSES = New TDXFCLASSES(_GUID)
                obj_DISPLAY = TDISPLAY.NullDisplay(New TDISPLAY(_GUID))
                obj_OBJECTS = New TDXFOBJECTS(_GUID)
                obj_SCREEN = New TSCREEN(_GUID)
                obj_SHAPES = New TSHAPEARRAY(_GUID)
                obj_THUMBNAIL = New TPROPERTIES("Thumbnail")
                obj_UCS = TPLANE.World
                pth_UCSICON = New TPATHS(dxxDrawingDomains.Model)
            set_DIMOVERRIDES = New TTABLEENTRY(dxxReferenceTypes.DIMSTYLE)
            set_DIM = New TTABLEENTRY(dxxSettingTypes.DIMSETTINGS)
            set_TEXT = New TTABLEENTRY(dxxSettingTypes.TEXTSETTINGS)
            set_TABLE = New TTABLEENTRY(dxxSettingTypes.TABLESETTINGS)
            set_SYMBOL = New TTABLEENTRY(dxxSettingTypes.SYMBOLSETTINGS)
            set_LTYPES = New TTABLEENTRY(dxxSettingTypes.LINETYPESETTINGS)
            tbl_APPID = New TTABLE(dxxReferenceTypes.APPID)
            tbl_BLOCKRECORD = New TTABLE(dxxReferenceTypes.BLOCK_RECORD)
            tbl_DIMSTYLE = New TTABLE(dxxReferenceTypes.DIMSTYLE)
            tbl_LAYER = New TTABLE(dxxReferenceTypes.LAYER)
            tbl_LTYPE = New TTABLE(dxxReferenceTypes.LTYPE)
            tbl_STYLE = New TTABLE(dxxReferenceTypes.STYLE)
            tbl_UCS = New TTABLE(dxxReferenceTypes.UCS)
            tbl_VIEW = New TTABLE(dxxReferenceTypes.VIEW)
            tbl_VPORT = New TTABLE(dxxReferenceTypes.VPORT)
            'init ------------------------

            _Name = aName.Trim()
        End Sub
        Public Sub New(ByRef ioDisplay As TDISPLAY, Optional aName As String = "")
            'init ------------------------
            Clears = 0
            CollectErrors = False
            Erasing = False
            _GUID = ""
            Tag = ""
            Status = ""
            FileType = dxxFileTypes.DXF
            FolderPath = ""
            GroupName = ""
            IsDirty = False
            _Name = ""
            RaiseErrors = False
            RegenList = ""
            Rendering = False
            SelectionStartID = 0
            Status = ""
            SuppressFromFile = False
            Tag = ""
            Errors = New TVALUES(0)
            obj_CLASSES = New TDXFCLASSES(_GUID)
            obj_DISPLAY = TDISPLAY.NullDisplay(New TDISPLAY(_GUID))
            obj_OBJECTS = New TDXFOBJECTS(_GUID)
            obj_SCREEN = New TSCREEN(_GUID)
            obj_SHAPES = New TSHAPEARRAY(_GUID)
            obj_THUMBNAIL = New TPROPERTIES("Thumbnail")
            obj_UCS = TPLANE.World
            pth_UCSICON = New TPATHS(dxxDrawingDomains.Model)
            set_DIMOVERRIDES = New TTABLEENTRY(dxxReferenceTypes.DIMSTYLE)
            set_DIM = New TTABLEENTRY(dxxSettingTypes.DIMSETTINGS)
            set_TEXT = New TTABLEENTRY(dxxSettingTypes.TEXTSETTINGS)
            set_TABLE = New TTABLEENTRY(dxxSettingTypes.TABLESETTINGS)
            set_SYMBOL = New TTABLEENTRY(dxxSettingTypes.SYMBOLSETTINGS)
            set_LTYPES = New TTABLEENTRY(dxxSettingTypes.LINETYPESETTINGS)
            tbl_APPID = New TTABLE(dxxReferenceTypes.APPID)
            tbl_BLOCKRECORD = New TTABLE(dxxReferenceTypes.BLOCK_RECORD)
            tbl_DIMSTYLE = New TTABLE(dxxReferenceTypes.DIMSTYLE)
            tbl_LAYER = New TTABLE(dxxReferenceTypes.LAYER)
            tbl_LTYPE = New TTABLE(dxxReferenceTypes.LTYPE)
            tbl_STYLE = New TTABLE(dxxReferenceTypes.STYLE)
            tbl_UCS = New TTABLE(dxxReferenceTypes.UCS)
            tbl_VIEW = New TTABLE(dxxReferenceTypes.VIEW)
            tbl_VPORT = New TTABLE(dxxReferenceTypes.VPORT)
            'init ------------------------

            obj_DISPLAY = TDISPLAY.NullDisplay(ioDisplay) 'one time only
            obj_UCS = New TPLANE("World", obj_DISPLAY.Units)

        End Sub

        Public Sub New(aImage As TIMAGE, Optional bNewGUIDS As Boolean = False)
            'init ------------------------
            Clears = aImage.Clears
            CollectErrors = aImage.CollectErrors
            Erasing = aImage.Erasing
            If Not bNewGUIDS Then _GUID = dxfEvents.NextImageGUID() Else _GUID = aImage.GUID
            Tag = aImage.Tag
            Status = aImage.Status
            FolderPath = aImage.FolderPath
            GroupName = aImage.GroupName
            FileType = aImage.FileType
            IsDirty = aImage.IsDirty
            _Name = aImage.Name
            RaiseErrors = aImage.RaiseErrors
            RegenList = aImage.RegenList
            Rendering = aImage.Rendering
            SelectionStartID = aImage.SelectionStartID
            Status = aImage.Status
            SuppressFromFile = aImage.SuppressFromFile
            Tag = aImage.Tag
            Errors = New TVALUES(aImage.Errors)
            obj_CLASSES = New TDXFCLASSES(aImage.obj_CLASSES, aImageGUID:=_GUID)
            obj_DISPLAY = TDISPLAY.NullDisplay(aImage.obj_DISPLAY)
            obj_OBJECTS = New TDXFOBJECTS(aImage.obj_OBJECTS)
            obj_SCREEN = New TSCREEN(aImage.obj_SCREEN)
            obj_SHAPES = New TSHAPEARRAY(aImage.obj_SHAPES)
            obj_THUMBNAIL = New TPROPERTIES(aImage.obj_THUMBNAIL)
            obj_UCS = New TPLANE(aImage.obj_UCS, aImageGUID:=_GUID)
            pth_UCSICON = New TPATHS(dxxDrawingDomains.Model)
            set_DIMOVERRIDES = New TTABLEENTRY(aImage.set_DIMOVERRIDES, aImageGUID:=_GUID)
            set_DIM = New TTABLEENTRY(aImage.set_DIM, aImageGUID:=_GUID)
            set_TEXT = New TTABLEENTRY(aImage.set_TEXT, aImageGUID:=_GUID)
            set_TABLE = New TTABLEENTRY(aImage.set_TABLE, aImageGUID:=_GUID)
            set_SYMBOL = New TTABLEENTRY(aImage.set_SYMBOL, aImageGUID:=_GUID)
            set_LTYPES = New TTABLEENTRY(aImage.set_LTYPES, aImageGUID:=_GUID)
            tbl_APPID = New TTABLE(aImage.tbl_APPID, aImageGUID:=_GUID)
            tbl_BLOCKRECORD = New TTABLE(aImage.tbl_BLOCKRECORD, aImageGUID:=_GUID)
            tbl_DIMSTYLE = New TTABLE(aImage.tbl_DIMSTYLE, aImageGUID:=_GUID)
            tbl_LAYER = New TTABLE(aImage.tbl_LAYER, aImageGUID:=_GUID)
            tbl_LTYPE = New TTABLE(aImage.tbl_LTYPE, aImageGUID:=_GUID)
            tbl_STYLE = New TTABLE(aImage.tbl_STYLE, aImageGUID:=_GUID)
            tbl_UCS = New TTABLE(aImage.tbl_UCS, aImageGUID:=_GUID)
            tbl_VIEW = New TTABLE(aImage.tbl_VIEW, aImageGUID:=_GUID)
            tbl_VPORT = New TTABLE(aImage.tbl_VPORT, aImageGUID:=_GUID)
            'init ------------------------
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public Property AutoRedraw As Boolean
            Get
                Return obj_DISPLAY.AutoRedraw
            End Get
            Set(value As Boolean)
                obj_DISPLAY.AutoRedraw = value
            End Set
        End Property

        Public Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value.Trim()

                If _Name.Length <= 0 Then Return
                Dim i As Integer = _Name.LastIndexOf("\")
                If i > 0 Then
                    _Name = _Name.Substring(0, _Name.Length - i).Trim()
                End If
                If _Name.Length >= 4 Then
                    If _Name.ToUpper().EndsWith(".DXF") Then
                        FileType = dxxFileTypes.DXF
                    ElseIf _Name.ToUpper().EndsWith(".DWG") Then
                        FileType = dxxFileTypes.DWG
                    ElseIf _Name.ToUpper().EndsWith(".DXT") Then
                        FileType = dxxFileTypes.DXF
                    End If
                    If FileType <> dxxFileTypes.DWG And FileType <> dxxFileTypes.DXF And FileType <> dxxFileTypes.DXT Then
                        FileType = dxxFileTypes.DWG
                    End If

                End If

            End Set
        End Property

        Public Sub Copy(aImage As TIMAGE)
            'Dim aDisp As TDISPLAY = _DISPLAY
            Errors = aImage.Errors
            obj_THUMBNAIL = aImage.obj_THUMBNAIL
            '_DISPLAY = aDisp
            pth_UCSICON = aImage.pth_UCSICON

            set_DIMOVERRIDES = aImage.set_DIMOVERRIDES
            set_DIM = aImage.set_DIM
            set_TEXT = aImage.set_TEXT
            set_TABLE = aImage.set_TABLE
            set_SYMBOL = aImage.set_SYMBOL
            set_LTYPES = aImage.set_LTYPES
            tbl_APPID = aImage.tbl_APPID
            tbl_BLOCKRECORD = aImage.tbl_BLOCKRECORD
            tbl_DIMSTYLE = aImage.tbl_DIMSTYLE
            tbl_LAYER = aImage.tbl_LAYER
            tbl_LTYPE = aImage.tbl_LTYPE
            tbl_STYLE = aImage.tbl_STYLE
            tbl_UCS = aImage.tbl_UCS
            tbl_VIEW = aImage.tbl_VIEW
            tbl_VPORT = aImage.tbl_VPORT
            obj_OBJECTS = aImage.obj_OBJECTS
            obj_CLASSES = aImage.obj_CLASSES

            obj_SHAPES = aImage.obj_SHAPES
            obj_SCREEN = aImage.obj_SCREEN
            obj_UCS = aImage.obj_UCS
        End Sub

        Public Function Clone() As TIMAGE
            Return New TIMAGE(Me)

        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TIMAGE(Me)
        End Function
        Public Property GUID As String
            Get
                '^the unique identify string of the image
                '~the image is renamed after every clear
                Return _GUID
            End Get
            Set(value As String)
                '^the unique identify string of the image
                '~the image is renamed after every clear
                If value Is Nothing Then value = String.Empty
                If _GUID <> value Then
                    _GUID = value
                    set_DIM.ImageGUID = _GUID
                    set_DIMOVERRIDES.ImageGUID = _GUID
                    set_LTYPES.ImageGUID = _GUID
                    set_SYMBOL.ImageGUID = _GUID
                    set_TABLE.ImageGUID = _GUID
                    obj_SCREEN.ImageGUID = _GUID
                    obj_OBJECTS.ImageGUID = _GUID
                    obj_DISPLAY.ImageGUID = _GUID
                    'obj_HANDLES.ImageGUID = _GUID
                    obj_CLASSES.ImageGUID = _GUID
                    tbl_LAYER.ImageGUID = _GUID
                    tbl_APPID.ImageGUID = _GUID
                    tbl_VIEW.ImageGUID = _GUID
                    tbl_UCS.ImageGUID = _GUID
                    tbl_VPORT.ImageGUID = _GUID
                    tbl_LTYPE.ImageGUID = _GUID
                    tbl_STYLE.ImageGUID = _GUID
                    tbl_DIMSTYLE.ImageGUID = _GUID
                End If
            End Set
        End Property

#End Region
#Region "Methods"



        Friend Function BaseSettings(aSettingsType As dxxSettingTypes) As TTABLEENTRY

            Dim _rVal As TTABLEENTRY = TTABLEENTRY.Null
            Select Case aSettingsType
                Case dxxSettingTypes.DIMOVERRIDES
                    If set_DIMOVERRIDES.Props.Count <= 0 Then
                        set_DIMOVERRIDES = New TTABLEENTRY(dxxSettingTypes.DIMOVERRIDES)
                    End If
                    set_DIMOVERRIDES.IsGlobal = True
                    _rVal = set_DIMOVERRIDES
                Case dxxSettingTypes.DIMSETTINGS
                    If set_DIM.Props.Count <= 0 Then set_DIM = New TTABLEENTRY(dxxSettingTypes.DIMSETTINGS)
                    _rVal = set_DIM
                Case dxxSettingTypes.TABLESETTINGS
                    If set_TABLE.Props.Count <= 0 Then set_TABLE = New TTABLEENTRY(dxxSettingTypes.TABLESETTINGS)
                    _rVal = set_TABLE
                Case dxxSettingTypes.HEADER

                Case dxxSettingTypes.LINETYPESETTINGS
                    If set_LTYPES.Props.Count <= 0 Then set_LTYPES = New TTABLEENTRY(dxxSettingTypes.LINETYPESETTINGS)
                    _rVal = set_LTYPES
                Case dxxSettingTypes.SYMBOLSETTINGS
                    If set_SYMBOL.Props.Count <= 0 Then set_SYMBOL = New TTABLEENTRY(dxxSettingTypes.SYMBOLSETTINGS)
                    _rVal = set_SYMBOL
                Case dxxSettingTypes.TEXTSETTINGS
                    If set_TEXT.Props.Count <= 0 Then set_TEXT = New TTABLEENTRY(dxxSettingTypes.TEXTSETTINGS)
                    _rVal = set_TEXT
                Case Else
                    Return Nothing
            End Select
            _rVal.ImageGUID = GUID
            _rVal.IsCopied = False
            Return _rVal


        End Function

        Friend Sub BaseSettings_Set(value As TTABLEENTRY)
            If value.Props.Count <= 0 Then Return
            Select Case value.SettingType
                Case dxxSettingTypes.DIMOVERRIDES
                    set_DIMOVERRIDES = value
                    set_DIMOVERRIDES.IsGlobal = True
                Case dxxSettingTypes.DIMSETTINGS
                    set_DIM = value
                Case dxxSettingTypes.TABLESETTINGS
                    set_TABLE = value
                Case dxxSettingTypes.LINETYPESETTINGS
                    set_LTYPES = value
                Case dxxSettingTypes.SYMBOLSETTINGS
                    set_SYMBOL = value
                Case dxxSettingTypes.TEXTSETTINGS
                    set_TEXT = value
                    Case Else
                        Return
                End Select
            End Sub

            Friend Sub BaseTable_Set(aTable As TTABLE)
            If aTable.Props.Count <= 0 Then Return
            Select Case aTable.TableType
                Case dxxReferenceTypes.DIMSTYLE
                    tbl_DIMSTYLE = aTable
                Case dxxReferenceTypes.STYLE
                    tbl_STYLE = aTable
                Case dxxReferenceTypes.LAYER
                    tbl_LAYER = aTable
                Case dxxReferenceTypes.UCS
                    tbl_UCS = aTable
                Case dxxReferenceTypes.VIEW
                    tbl_VIEW = aTable
                Case dxxReferenceTypes.LTYPE
                    tbl_LTYPE = aTable
                Case dxxReferenceTypes.BLOCK_RECORD
                    tbl_BLOCKRECORD = aTable
                Case dxxReferenceTypes.APPID
                    tbl_APPID = aTable
                Case dxxReferenceTypes.VPORT
                    tbl_VPORT = aTable
                Case Else
                    Return
            End Select
        End Sub

        Public Function BaseTable(aReferenceType As dxxReferenceTypes) As TTABLE
            Dim _rVal As TTABLE
            Select Case aReferenceType
                Case dxxReferenceTypes.DIMSTYLE
                    If tbl_DIMSTYLE.Props.Count <= 0 Then tbl_DIMSTYLE = New TTABLE(dxxReferenceTypes.DIMSTYLE)
                    _rVal = tbl_DIMSTYLE
                Case dxxReferenceTypes.STYLE
                    If tbl_STYLE.Props.Count <= 0 Then tbl_STYLE = New TTABLE(dxxReferenceTypes.STYLE)
                    _rVal = tbl_STYLE
                Case dxxReferenceTypes.LAYER
                    If tbl_LAYER.Props.Count <= 0 Then tbl_LAYER = New TTABLE(dxxReferenceTypes.LAYER)
                    _rVal = tbl_LAYER
                Case dxxReferenceTypes.UCS
                    If tbl_UCS.Props.Count <= 0 Then tbl_UCS = New TTABLE(dxxReferenceTypes.UCS)
                    _rVal = tbl_UCS
                Case dxxReferenceTypes.VIEW
                    If tbl_VIEW.Props.Count <= 0 Then tbl_VIEW = New TTABLE(dxxReferenceTypes.VIEW)
                    _rVal = tbl_VIEW
                Case dxxReferenceTypes.LTYPE
                    If tbl_LTYPE.Props.Count <= 0 Then tbl_LTYPE = New TTABLE(dxxReferenceTypes.LTYPE)
                    _rVal = tbl_LTYPE
                Case dxxReferenceTypes.BLOCK_RECORD
                    If tbl_BLOCKRECORD.Props.Count <= 0 Then tbl_BLOCKRECORD = New TTABLE(dxxReferenceTypes.BLOCK_RECORD)
                    _rVal = tbl_BLOCKRECORD
                Case dxxReferenceTypes.APPID
                    If tbl_APPID.Props.Count <= 0 Then tbl_APPID = New TTABLE(dxxReferenceTypes.APPID)
                    _rVal = tbl_APPID
                Case dxxReferenceTypes.VPORT
                    If tbl_VPORT.Props.Count <= 0 Then tbl_VPORT = New TTABLE(dxxReferenceTypes.VPORT)
                    _rVal = tbl_VPORT
                Case Else
                    Return New TTABLE(dxxReferenceTypes.UNDEFINED)
            End Select
            _rVal.ImageGUID = GUID
            _rVal.TableType = aReferenceType
            Return _rVal


        End Function



        Friend Sub Clear()
            'On Error Resume Next
            obj_OBJECTS.Clear()
            obj_SCREEN.Clear()
            obj_SHAPES.Clear()
            obj_THUMBNAIL.Clear()
            Errors.Clear()
            pth_UCSICON.Clear()
            set_DIM.Clear()

            set_SYMBOL.Clear()
            set_TABLE.Clear()
            set_TEXT.Clear()
            tbl_APPID.Clear()
            tbl_BLOCKRECORD.Clear()
            tbl_DIMSTYLE.Clear()
            tbl_LAYER.Clear()
            tbl_LTYPE.Clear()
            tbl_STYLE.Clear()
            tbl_UCS.Clear()
            tbl_VIEW.Clear()
            tbl_VPORT.Clear()
            obj_DISPLAY.Dispose()
            obj_CLASSES.Clear()

        End Sub

        Friend Function ReferenceExists(aRefType As dxxReferenceTypes, aName As String) As Boolean
            '#1the type of reference object to look for
            '#2the name of the refrence to look for
            '^returns true if the passed name exists in the refrenced table
            Dim aTable As TTABLE
            Select Case aRefType
                Case dxxReferenceTypes.APPID
                    aTable = tbl_APPID
                Case dxxReferenceTypes.BLOCK_RECORD
                    aTable = tbl_BLOCKRECORD
                Case dxxReferenceTypes.DIMSTYLE
                    aTable = tbl_DIMSTYLE
                Case dxxReferenceTypes.LAYER
                    aTable = tbl_LAYER
                Case dxxReferenceTypes.LTYPE
                    aTable = tbl_LTYPE
                Case dxxReferenceTypes.STYLE
                    aTable = tbl_STYLE
                Case dxxReferenceTypes.UCS
                    aTable = tbl_UCS
                Case dxxReferenceTypes.VIEW
                    aTable = tbl_VIEW
                Case dxxReferenceTypes.VPORT
                    aTable = tbl_VPORT
                Case Else
                    Return False
            End Select
            Return aTable.Contains(aName)
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Sub GetTables(aImage As TIMAGE, ByRef rTables() As TTABLE)
            ReDim rTables(0 To 8)
            'If aImage.tbl_VPORT.Props.Count <= 0 Then aImage.tbl_VPORT = dxfImageTool.TableCreate New TTABLE(dxxReferenceTypes.VPORT)
            'If aImage.tbl_LTYPE.Props.Count <= 0 Then aImage.tbl_LTYPE = New TTABLE(dxxReferenceTypes.LTYPE)
            'If aImage.tbl_LAYER.Props.Count <= 0 Then aImage.tbl_LAYER = New TTABLE(dxxReferenceTypes.LAYER)
            'If aImage.tbl_STYLE.Props.Count <= 0 Then aImage.tbl_STYLE = New TTABLE(dxxReferenceTypes.STYLE)
            'If aImage.tbl_VIEW.Props.Count <= 0 Then aImage.tbl_VIEW = New TTABLE(dxxReferenceTypes.VIEW)
            'If aImage.tbl_UCS.Props.Count <= 0 Then aImage.tbl_UCS = New TTABLE(dxxReferenceTypes.UCS)
            'If aImage.tbl_APPID.Props.Count <= 0 Then aImage.tbl_APPID = New TTABLE(dxxReferenceTypes.APPID)
            'If aImage.tbl_DIMSTYLE.Props.Count <= 0 Then aImage.tbl_DIMSTYLE = New TTABLE(dxxReferenceTypes.DIMSTYLE)
            'If aImage.tbl_BLOCKRECORD.Props.Count <= 0 Then
            '    aImage.tbl_BLOCKRECORD = New TTABLE(dxxReferenceTypes.BLOCK_RECORD)
            'End If
            rTables(0) = aImage.tbl_VPORT
            rTables(1) = aImage.tbl_LTYPE
            rTables(2) = aImage.tbl_LAYER
            rTables(3) = aImage.tbl_STYLE
            rTables(4) = aImage.tbl_VIEW
            rTables(5) = aImage.tbl_UCS
            rTables(6) = aImage.tbl_APPID
            rTables(7) = aImage.tbl_DIMSTYLE
            rTables(8) = aImage.tbl_BLOCKRECORD
        End Sub
        Public Shared Function GetReference(aImage As TIMAGE, aRefType As dxxReferenceTypes, aName As String) As TTABLEENTRY
            Dim _rVal As TTABLEENTRY = TTABLEENTRY.Null
            Dim aStyle As TTABLEENTRY = TTABLEENTRY.Null
            Dim tStyle As TTABLEENTRY = TTABLEENTRY.Null
            Dim sname As String
            Select Case aRefType
                Case dxxReferenceTypes.LAYER
                    If aImage.tbl_LAYER.Count <= 0 Then aImage.tbl_LAYER = New TTABLE(dxxReferenceTypes.LAYER, True)
                    _rVal = aImage.tbl_LAYER.Entry(aName)
                    If _rVal.Index < 0 Then _rVal = aImage.tbl_LAYER.Item(1)
                Case dxxReferenceTypes.DIMSTYLE
                    If aImage.tbl_DIMSTYLE.Count <= 0 Then aImage.tbl_DIMSTYLE = New TTABLE(dxxReferenceTypes.DIMSTYLE, True)
                    _rVal = aImage.tbl_DIMSTYLE.Entry(aName)
                    If _rVal.Index < 0 Then _rVal = aImage.tbl_DIMSTYLE.Item(1)
                    If aImage.tbl_STYLE.Count <= 0 Then aImage.tbl_STYLE = New TTABLE(dxxReferenceTypes.STYLE, True)
                    _rVal = aImage.tbl_DIMSTYLE.Entry(aName)
                    sname = _rVal.Props.ValueStr("*DIMTXSTY_NAME", "Standard")
                    tStyle = aImage.tbl_STYLE.Entry(sname)
                    If tStyle.Props.Count <= 0 Then tStyle = aImage.tbl_STYLE.Item(1)
                    _rVal.Props.SetVal("*DIMTXSTY_NAME", tStyle.Name)
                        aStyle = aImage.tbl_DIMSTYLE.Entry(aName)
                        aStyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, tStyle.Props.GCValueStr(2))
                        aStyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY, tStyle.Props.GCValueStr(5))
                        aImage.tbl_DIMSTYLE.UpdateEntry = aStyle
                        _rVal = aImage.tbl_DIMSTYLE.Entry(aName)
                    Case dxxReferenceTypes.STYLE
                        If aImage.tbl_STYLE.Count <= 0 Then aImage.tbl_STYLE = New TTABLE(dxxReferenceTypes.STYLE, True)
                        _rVal = aImage.tbl_STYLE.Entry(aName)
                        If _rVal.Index < 0 Then _rVal = aImage.tbl_STYLE.Item(1)
                End Select
                Return _rVal
            End Function
#End Region 'Shared Methods
        End Structure 'TIMAGE
#End Region 'Structures

End Namespace
