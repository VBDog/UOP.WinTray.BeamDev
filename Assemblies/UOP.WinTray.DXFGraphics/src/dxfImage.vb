'Imports Microsoft.VisualBasic.PowerPacks.Printing.Compatibility.VB6.ColorConstants
Imports System.Linq
Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.dxfGlobals

Imports UOP.DXFGraphics.Structures
Imports System.Windows.Controls
Imports SharpDX.Direct2D1
Imports System.Windows

Namespace UOP.DXFGraphics
    Public Class dxfImage
        Implements IDisposable
        Implements IEventSubscriber(Of Message_ImageRequest)
#Region "Members"
        'Private _Events As dxpEventHandler
        Private _Structure As TIMAGE
        Private _Blocks As colDXFBlocks
        Private _Entities As colDXFEntities
        Private _Objects As colDXFObjects
        Private _Screen As dxoScreen
        'the bitmaps
        Private _Bitmap As dxfBitmap
        Private _ScreenBitmap As dxfBitmap
        Private _BackGround As dxfBitmap
        Private _Disposed As Boolean
        Private _UsingDXFViewer As Boolean
        Private _HandleGenerator As dxoHandleGenerator
#End Region 'Members
#Region "Events"
        Public Event RenderEvent(e As dxfImageRenderEventArg)
        Public Event ImageError(aType As dxxFileErrorTypes, aErrorString As String)
        Public Event PropertyChange(aSource As String, aProperty As dxoProperty)
        Public Event SettingChange(aSource As String, aProperty As dxoProperty)
        Public Event StatusChange(StatusDescription As String)
        Public Event WriteToDisc(aBegin As Boolean, aFileType As dxxFileTypes, aFileName As String)
        Public Event TableEvent(TableName As String, EventType As dxxCollectionEventTypes, EventDescription As String)
        Public Event TableMemberEvent(TableName As String, MemberName As String, aProperty As dxoProperty)
        Public Event EntitiesEvent(Added As Boolean, aEntity As dxfEntity)
        Public Event EntitiesMemberEvent(aEntity As dxfEntity, aDescription As String)
        Public Event BlocksEvent(Added As Boolean, aBlock As dxfBlock)
        Public Event BlocksMemberEvent(aBlock As dxfBlock, aDescription As String)
        Public Event ObjectEvent(ObjectTypeName As String, ObjectName As String, PropertyName As String, NewValue As Object, LastValue As Object)
        Public Event ObjectsEvent(ObjectTypeName As String, EventType As dxxCollectionEventTypes, EventDescription As String)
        Public Event ErrorRecieved(aFunction As String, aErrSource As String, aErrDescription As String, ByRef ioIgnore As Boolean)
        'new
        Public Event ScreenRender(aBitmap As dxfBitmap)
        Public Event ScreenDrawingEvent(aEventType As dxxScreenEventTypes, e As dxfImageScreenEventArg)
        'added by CADFX
        Public Event ZoomEvent(aExtents As Boolean, aZoomFactor As Double)
        Public Event ViewChangeEvent(e As dxfViewChangeEventArg)
        Public Event ViewRotateEvent(aRotation As Double)
        Public Event ViewRegenerate(aImage As dxfImage)
        Public Event SaveToFileEvent(aFileName As String)
        Public Event OverlayEvent(added As Boolean, ByRef Entity As dxfEntity)
        Public Event OverlayBmpEvent(added As Boolean, bmp As System.Drawing.Bitmap)
        Public Event EntitiesUpdateEvent(ByRef Remove As Boolean, ByRef Entity As dxfEntity)
#End Region 'Events
#Region "Constructors"
        Public Sub New()
            Try
                Init(-100, Color.White)
            Catch ex As Exception
                If dxfUtils.RunningInIDE Then MessageBox.Show(ex.Message)
            End Try
        End Sub
        Public Sub New(aBackColor As Color, Optional aImageSize As System.Drawing.Size? = Nothing)
            Try
                Init(-100, aBackColor)
                If aImageSize.HasValue Then Display.Size = aImageSize.Value
            Catch ex As Exception
                If dxfUtils.RunningInIDE Then MessageBox.Show(ex.Message)
            End Try
        End Sub
        Public Sub New(Optional aName As String = "", Optional aBackColor As Color = Nothing, Optional aImageSize As System.Drawing.Size? = Nothing)
            Try
                Init(-100, aBackColor)
                Name = aName.Trim
                If aImageSize.HasValue Then Display.Size = aImageSize.Value
            Catch ex As Exception
                If dxfUtils.RunningInIDE Then MessageBox.Show(ex.Message)
            End Try
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Disposed As Boolean
            Get
                Return _Disposed
            End Get
        End Property
        Public Property UsingDxfViewer As Boolean
            Get
                Return _UsingDXFViewer
            End Get
            Set(value As Boolean)
                _UsingDXFViewer = value
            End Set
        End Property
        Friend Property bmp_Screen As dxfBitmap
            Get
                Return _ScreenBitmap
            End Get
            Set(value As dxfBitmap)
                _ScreenBitmap = value
            End Set
        End Property
        Friend Property bmp_Display As dxfBitmap
            Get
                Return _Bitmap
            End Get
            Set(value As dxfBitmap)
                _Bitmap = value
            End Set
        End Property
        Friend Property bmp_Background As dxfBitmap
            Get
                Return _BackGround
            End Get
            Set(value As dxfBitmap)
                _BackGround = value
            End Set
        End Property
        Friend Property Background As dxfBitmap
            Get
                Return _BackGround
            End Get
            Set(value As dxfBitmap)
                If value Is Nothing Then
                    If _BackGround IsNot Nothing Then _BackGround.Dispose()
                    _BackGround = Nothing
                Else
                    _BackGround = value.Clone
                End If
            End Set
        End Property
        Public ReadOnly Property APPIDS As dxoAPPIDs
            Get
                '^provides  access to the images APPID table
                Return Tables.APPIDs
            End Get
        End Property
        Public ReadOnly Property UCSS As dxoUCSs
            Get
                '^provides  access to the images UCS table
                Return Tables.UCSs
            End Get
        End Property

        Public ReadOnly Property BlockRecords As dxoBlockRecords
            Get
                '^provides  access to the images Block Record table
                Return Tables.BlockRecords
            End Get
        End Property
        '^controls how the image responds to changes to table entries, and display settings
        '~true results in redraws after each event that affects the current output image
        Public Property AutoRedraw As Boolean
            Get
                Return _Structure.obj_DISPLAY.AutoRedraw
            End Get
            Set(value As Boolean)
                Dim wuz As Boolean = _Structure.obj_DISPLAY.AutoRedraw
                If wuz = value Then Return
                _Structure.obj_DISPLAY.AutoRedraw = value
                RespondToPropertyChange("", New TPROPERTY(70, value, "AutoRedraw", dxxPropertyTypes.dxf_Boolean, wuz))
            End Set
        End Property
        Public ReadOnly Property Blocks As colDXFBlocks
            Get
                If _Blocks Is Nothing And Not Disposed Then
                    _Blocks = New colDXFBlocks(Me)
                End If
                '^the current blocks collection of the image
                Return _Blocks
            End Get
        End Property
        '^flag to raise or collect drawing errors
        '~default = False
        Public Property CollectErrors As Boolean
            Get
                Return _Structure.CollectErrors
            End Get
            Set(value As Boolean)
                Dim wuz As Boolean = _Structure.CollectErrors
                If wuz = value Then Return
                If _Structure.CollectErrors <> value Then ResetErrors()
                _Structure.CollectErrors = value
                RespondToPropertyChange("", New TPROPERTY(70, value, "CollectErrors", dxxPropertyTypes.dxf_Boolean, wuz))
            End Set
        End Property
        '^the dimsettings structure
        Public ReadOnly Property DimSettings As dxoSettingsDim
            Get
                Return New dxoSettingsDim(BaseSettings(dxxSettingTypes.DIMSETTINGS))
            End Get
        End Property
        '^the dimension style which carries the current override settings
        Public ReadOnly Property DimStyleOverrides As dxsDimOverrides
            Get
                Dim curname As String = Header.DimStyleName
                Dim _rVal As dxsDimOverrides = _Settings.DimStyleOverrides
                _rVal.SetImage(Me, False)
                If String.Compare(_rVal.Name, curname) <> 0 Then

                    _rVal.UpdateToImage(curname, aImage:=Me)
                End If
                Return _rVal
            End Get
        End Property
        '^the collection of all dim styles defined in the oimages DimStyle table
        Public ReadOnly Property DimStyles As dxoDimStyles
            Get
                Return Tables.DimStyles
            End Get
        End Property
        '^a tool supplied by the image that is used to create dimensions and dimension stacks
        Public ReadOnly Property DimTool As dxoDimTool
            Get
                Return New dxoDimTool(Me)
            End Get
        End Property
        '^a tool supplied by the image that is used to create symbols
        Public ReadOnly Property SymbolTool As dxoSymbolTool
            Get
                Return New dxoSymbolTool(Me)
            End Get
        End Property
        '^the object used to control the current display of the image
        Public ReadOnly Property Display As dxoDisplay
            Get
                Return New dxoDisplay(Me)
            End Get
        End Property
        Public ReadOnly Property Draw As dxoDrawingTool
            Get
                '^the object which offers the user extended function for adding entities to the image
                Return New dxoDrawingTool(Me)
            End Get
        End Property
        Public Overridable Property Entities As colDXFEntities
            '^the collection of dxfEntities defined for the image
            Get
                If _Entities IsNot Nothing Then
                    _Entities.SetGUIDS(GUID, "", "", dxxFileObjectTypes.Entities, aImage:=Me)
                End If
                Return _Entities
            End Get
            Set(value As colDXFEntities)
                If _Entities IsNot Nothing Then
                    _Entities.SetGUIDS(GUID, "", "", dxxFileObjectTypes.Entities, aImage:=Me)
                    _Entities.Populate(value, True)
                End If
            End Set
        End Property
        Public ReadOnly Property EntityTool As dxoEntityTool
            Get
                '^a tool supplied by the image that is used to create complex
                '^entities like dimensions, leaders, tables and symbols etc.
                Return New dxoEntityTool(Me)
            End Get
        End Property
        Friend Property Erasing As Boolean
            Get
                '^flag indicating that the image is currently erasing an entity
                Return _Structure.Erasing
            End Get
            Set(value As Boolean)
                '^flag indicating that the image is currently erasing an entity
                _Structure.Erasing = value
            End Set
        End Property
        Public Property ErrorCol As List(Of String)
            '^a collection of strings which are the current Errors
            Get
                Return _Structure.Errors.ToStringList
            End Get
            Set(value As List(Of String))
                _Structure.Errors = New TVALUES
                If value Is Nothing Then Return
                For i As Integer = 0 To value.Count - 1
                    If Not String.IsNullOrWhiteSpace(value.Item(i)) Then
                        _Structure.Errors.Add(Trim(value.Item(i)))
                    End If
                Next i
            End Set
        End Property
        Public ReadOnly Property ErrorCount As Integer
            '^the number of currently stored error strings
            Get
                Return _Structure.Errors.Count
            End Get
        End Property

        Public Property FileType As dxxFileTypes
            Get
                '^the file type last requested top read or write
                If Not dxfEnums.Validate(GetType(dxxFileTypes), TVALUES.To_INT(_Structure.FileType), bSkipNegatives:=True) Then _Structure.FileType = dxxFileTypes.DWG
                Return _Structure.FileType
            End Get
            Set(value As dxxFileTypes)
                '^the file type last requested top read or write
                If Not dxfEnums.Validate(GetType(dxxFileTypes), TVALUES.To_INT(value), TVALUES.To_INT(dxxFileTypes.Undefined)) Then Return
                Dim wuz As dxxFileTypes = _Structure.FileType
                If wuz = value Then Return
                _Structure.FileType = value
                Dim aProp As New TPROPERTY(2, value, "FileType", dxxPropertyTypes.dxf_Integer, wuz) With {.EnumValueType = GetType(dxxFileTypes), .DecodeString = dxfEnums.ValueNameList(GetType(dxxFileTypes))}
                RespondToPropertyChange("Image", aProp)
            End Set
        End Property
        Public Property FolderPath As String
            Get
                '^the folder where the image was last written to or read from
                Dim _rVal As String = _Structure.FolderPath
                If _rVal = "" Then _rVal = "C:\"
                Return _rVal
            End Get
            Set(value As String)
                '^the folder where the image was last written to or read from
                value = Trim(value)
                If value <> "" Then
                    If Right(value, 1) <> "\" Then value += "\"
                End If
                Dim wuz As String = _Structure.FolderPath
                If String.Compare(wuz, value, True) = 0 Then Return
                _Structure.FolderPath = value
                Dim aProp As New TPROPERTY(2, value, "FolderPath", dxxPropertyTypes.dxf_String, wuz)
                RespondToPropertyChange("Image", aProp)
            End Set
        End Property
        Public Property GroupName As String
            '^the currently active group name
            Get
                Return _Structure.GroupName
            End Get
            Set(value As String)
                Dim aErrStr As String = String.Empty
                value = value.Trim
                Try
                    If value <> "" Then dxfUtils.ValidateGroupName(value, aErrStr, False)
                    If aErrStr <> "" Then Throw New Exception(aErrStr)
                    Dim wuz As String = _Structure.GroupName
                    If wuz.ToUpper = value.ToUpper Then Return
                    _Structure.GroupName = value
                    RespondToPropertyChange("", New TPROPERTY(3, value, "GroupName", dxxPropertyTypes.dxf_String, wuz))
                Catch ex As Exception
                    HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfImage", ex)
                    Return
                End Try
            End Set
        End Property
        Public Property GUID As String
            Get
                '^the unique identify string of the image
                '~the image is renamed after every clear
                Return _Structure.GUID
            End Get
            Set(value As String)
                '^the unique identify string of the image
                '~the image is renamed after every clear
                If _Structure.GUID <> value Then
                    _Structure.GUID = value
                    If _Blocks IsNot Nothing Then _Blocks.ImageGUID = value
                    If _Entities IsNot Nothing Then _Entities.SetGUIDS(value, "", "", dxxFileObjectTypes.Entities)
                End If
            End Set
        End Property
        Friend Property Objex As colDXFObjects
            Get
                If Disposed Then Return Nothing
                If _Objects Is Nothing Then _Objects = New colDXFObjects(Me) Else _Objects.ImageGUID = GUID
                Return _Objects
            End Get
            Set(value As colDXFObjects)
                _Objects = value
            End Set
        End Property

        Public ReadOnly Property Groups As List(Of dxfoGroup)
            Get
                Return Objex.GetObjects(dxxObjectTypes.Group).OfType(Of dxfoGroup)().ToList()

            End Get
        End Property


        Public Property HandleGenerator As dxoHandleGenerator
            Get
                If _HandleGenerator Is Nothing And Not Disposed Then
                    _HandleGenerator = New dxoHandleGenerator(Me)
                End If
                If _HandleGenerator IsNot Nothing Then _HandleGenerator.ImageGUID = GUID
                Return _HandleGenerator
            End Get
            Friend Set(value As dxoHandleGenerator)
                If value IsNot Nothing Then
                    _HandleGenerator = value
                    _HandleGenerator.ImageGUID = GUID
                End If
            End Set
        End Property

        ''' <summary>
        ''' the object which carries the current dxf header properties
        ''' </summary>

        Public ReadOnly Property Header As dxsHeader
            Get
                If (Settings_Objects Is Nothing) Then
                    Return Nothing
                End If
                Return Settings_Objects.Header
            End Get
        End Property

        Public Property ImageName As String
            '^User assignable string name for the image
            Get
                Return _Bitmap.Name
            End Get
            Set(value As String)
                If _Bitmap Is Nothing Then Return
                Dim wuz As String = _Bitmap.Name
                If wuz = value Then Return
                _Bitmap.Name = value
                RespondToPropertyChange("", New TPROPERTY(2, value, "ImageName", dxxPropertyTypes.dxf_String, wuz))
            End Set
        End Property
        Public Property FileVersion As dxxACADVersions
            Get
                Return goACAD.Versions.GetVersionByName(dxfEnums.MemberName(Header.AcadVersion), True).Version
            End Get
            Set(value As dxxACADVersions)
                If Not dxfEnums.Validate(GetType(dxxACADVersions), TVALUES.To_INT(value), bSkipNegatives:=True) Then Return
                Header.PropValueSet(dxxHeaderVars.ACADVER, dxfEnums.CodeValue(value))
            End Set
        End Property

        Public Property IsDirty As Boolean
            Get
                '^a user assignable dirty flag for the image
                Return _Structure.IsDirty
            End Get
            Set(value As Boolean)
                '^a user assignable dirty flag for the image
                Dim wuz As Boolean = _Structure.obj_DISPLAY.IsDirty
                If wuz = value Then Return
                _Structure.IsDirty = value
                RespondToPropertyChange("", New TPROPERTY(70, value, "IsDirty", dxxPropertyTypes.dxf_Boolean, wuz))
            End Set
        End Property

        Public ReadOnly Property Layers As dxoLayers
            Get
                '^provides user access to the images layer table
                Return Tables.Layers
            End Get
        End Property

        Public ReadOnly Property Layouts As List(Of dxfObject)
            Get
                '^provides access to the collection of LAYOUT objects defined for the image
                Return Objex.GetObjects(dxxObjectTypes.Layout)
            End Get
        End Property
        Public ReadOnly Property LeaderTool As dxoLeaderTool
            Get
                '^a tool supplied by the image that is used to create leaders and leader stacks
                Return New dxoLeaderTool(Me)
            End Get
        End Property

        ''' <summary>
        ''' provides user access to the images linetype layer settings
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LinetypeLayers As dxsLinetypes
            Get

                Return _Settings.LinetypeLayers
            End Get
        End Property
        Public ReadOnly Property Linetypes As dxoLineTypes
            Get
                '^provides user access to the images linetype linetype table
                Return Tables.LineTypes
            End Get
        End Property
        Public ReadOnly Property TextStyles As dxoStyles
            Get
                '^provides user access to the images text style table
                Return Tables.Styles
            End Get
        End Property
        Public Property Name As String
            Get
                '^a user assignable name for the image
                If Not String.IsNullOrEmpty(_Structure.Name) Then Return _Structure.Name Else Return GUID
            End Get
            Set(value As String)
                '^a user assignable name for the image
                Dim wuz As String = _Structure.Name
                If wuz = value Then Return
                _Structure.Name = value
                Dim aProp As New TPROPERTY(2, value, "Name", dxxPropertyTypes.dxf_String, wuz)
                RespondToPropertyChange("Image", aProp)
            End Set
        End Property

        Private _Classes As colDXFClasses
        Public Property Classes As colDXFClasses
            Get
                If _Classes Is Nothing Then _Classes = New colDXFClasses
                _Classes.ImageGUID = GUID
                Return _Classes
            End Get
            Friend Set(value As colDXFClasses)
                _Classes = value
            End Set
        End Property


        Friend Property obj_DISPLAY As TDISPLAY
            Get
                '^the display object of the image
                Dim dsp As TDISPLAY = _Structure.obj_DISPLAY
                dsp.ImageGUID = _Structure.GUID
                If Not Disposed AndAlso _Bitmap IsNot Nothing Then
                    dsp.DPI = _Bitmap.DPI
                    dsp.pln_DEVICE = _Bitmap.Plane
                    _Structure.obj_DISPLAY = dsp
                End If
                Return _Structure.obj_DISPLAY
            End Get
            Set(value As TDISPLAY)
                '^the display object of the image
                value.ImageGUID = _Structure.GUID
                If Not Disposed AndAlso _Bitmap IsNot Nothing Then
                    value.DPI = _Bitmap.DPI
                    value.pln_DEVICE = _Bitmap.Plane
                End If
                _Structure.obj_DISPLAY = value
            End Set
        End Property
        Friend Property obj_SCREEN As TSCREEN
            Get
                '^the screen object of the image
                If _Screen IsNot Nothing Then _Structure.obj_SCREEN = _Screen.Strukture
                _Structure.obj_SCREEN.ImageGUID = _Structure.GUID
                _Structure.obj_SCREEN.TextStyle.ImageGUID = _Structure.GUID
                _Structure.obj_SCREEN.TextStyle.IsGlobal = True
                Return _Structure.obj_SCREEN
            End Get
            Set(value As TSCREEN)
                '^the screen object of the image
                _Structure.obj_SCREEN = value
                If _Screen IsNot Nothing Then _Screen.Strukture = _Structure.obj_SCREEN
            End Set
        End Property
        Friend Property obj_SCREENTEXTSTYLE As TTABLEENTRY
            Get
                '^the text style for the virtual screen
                Return obj_SCREEN.TextStyle
            End Get
            Set(value As TTABLEENTRY)
                '^the text style for the virtual screen
                If value.Props.Count > 0 Then
                    Dim Scrn As TSCREEN = obj_SCREEN
                    Scrn.TextStyle = value
                    obj_SCREEN = Scrn
                End If
            End Set
        End Property
        Friend Property obj_SHAPES As TSHAPEARRAY
            Get
                _Structure.obj_SHAPES.ImageGUID = _Structure.GUID
                Return _Structure.obj_SHAPES
            End Get
            Set(value As TSHAPEARRAY)
                _Structure.obj_SHAPES = value
            End Set
        End Property
        Friend Property obj_THUMBNAIL As TPROPERTIES
            Get
                Return _Structure.obj_THUMBNAIL
            End Get
            Set(value As TPROPERTIES)
                _Structure.obj_THUMBNAIL = value
            End Set
        End Property
        Friend Property obj_UCS As TPLANE
            Get
                '^the ucs defined for the image
                _Structure.obj_UCS.Units = _Structure.obj_DISPLAY.Units
                _Structure.obj_UCS.Name = "UCS"
                _Structure.obj_UCS.ImageGUID = GUID
                Return _Structure.obj_UCS
            End Get
            Set(value As TPLANE)
                '^the ucs defined for the image
                _Structure.obj_UCS = value
                _Structure.obj_UCS.ImageGUID = GUID
            End Set
        End Property
        Public ReadOnly Property Picture As System.Drawing.Image
            Get
                '^returns then current bitmap as a stdPicture
                Try
                    Return _Bitmap.Bitmap.Clone
                Catch ex As Exception
                    Return Nothing
                End Try
            End Get
        End Property
        Public ReadOnly Property PlotSettings As List(Of dxfObject)
            Get
                '^provides access to the collection of PLOTSETTINGS objects defined for the image
                Return Objex.GetObjects(dxxObjectTypes.PlotSetting)
            End Get
        End Property
        Public ReadOnly Property Primatives As dxfPrimatives
            Get
                Return New dxfPrimatives(GUID)
            End Get
        End Property
        Friend Property pth_UCSICON As TPATHS
            '^the last drawn path of the current ucs icon
            Get
                Return _Structure.pth_UCSICON
            End Get
            Set(value As TPATHS)
                _Structure.pth_UCSICON = value
            End Set
        End Property
        Public Property RaiseErrors As Boolean
            Get
                '^flag to raise errors
                '~default = False. Only valid if CollectErrors = False.
                Return _Structure.RaiseErrors
            End Get
            Set(value As Boolean)
                '^flag to raise errors
                '~default = False. Only valid if CollectErrors = False
                Dim wuz As Boolean = _Structure.RaiseErrors
                If wuz = value Then Return
                _Structure.RaiseErrors = value
                RespondToPropertyChange("", New TPROPERTY(70, value, "RaiseErrors", dxxPropertyTypes.dxf_Boolean, wuz))
            End Set
        End Property
        Friend ReadOnly Property RegenList As String
            Get
                '^the lsit of entity types to be forced to regenerae their paths
                Return _Structure.RegenList
            End Get
        End Property
        Public Property Rendering As Boolean
            Get
                Return _Structure.Rendering
            End Get
            Friend Set(value As Boolean)
                If _Structure.Rendering = value Then Return
                _Structure.Rendering = value
            End Set
        End Property
        Public ReadOnly Property Screen As dxoScreen
            Get
                Return _Screen
                'Return New dxoScreen(obj_SCREEN)
            End Get
        End Property
        Friend Property BaseSettingsCol As List(Of TTABLEENTRY)
            Get
                Return New List(Of TTABLEENTRY) From {
                    BaseSettings(dxxReferenceTypes.VPORT),
                    BaseSettings(dxxReferenceTypes.LTYPE),
                    BaseSettings(dxxReferenceTypes.LAYER),
                    BaseSettings(dxxReferenceTypes.STYLE),
                    BaseSettings(dxxReferenceTypes.VIEW),
                    BaseSettings(dxxReferenceTypes.UCS),
                    BaseSettings(dxxReferenceTypes.APPID),
                    BaseSettings(dxxReferenceTypes.DIMSTYLE),
                    BaseSettings(dxxReferenceTypes.BLOCK_RECORD)
                }
            End Get
            Set(value As List(Of TTABLEENTRY))
                If value Is Nothing Then Return
                If value.Count <= 0 Then Return
                Dim aSetngs As TTABLEENTRY
                For Each aSetngs In value
                    BaseSettings_Set(aSetngs)
                Next
            End Set
        End Property

        Public ReadOnly Property Shapes As dxoShapeArray
            Get
                Return New dxoShapeArray(obj_SHAPES)
            End Get
        End Property
        Public Property Status As String
            Get
                '^a user assignable status string which is stored and raised as an event when it changes
                Status = _Structure.Status
                Return Status
            End Get
            Set(value As String)
                '^a user assignable status string which is stored and raised as an event when it changes
                If _Structure.Status <> value Then
                    Try
                        RaiseEvent StatusChange(value)
                    Catch ex As Exception
                    End Try
                    _Structure.Status = value
                End If
            End Set
        End Property

        Friend Property SelectionStartID As Long

        Friend ReadOnly Property Strukture As TIMAGE
            Get

                For Each tbl As dxfTable In Tables
                    _Structure.BaseTable_Set(New TTABLE(tbl))
                Next

                _Structure.set_HEADER = New TTABLEENTRY(Header)
                _Structure.set_DIMOVERRIDES = New TTABLEENTRY(DimStyleOverrides)

                Return _Structure


            End Get
            'Set(value As TIMAGE)

            '    IsDirty = value.IsDirty  'to be sure to throw the event
            '    _Structure.Copy(value)
            '    _Structure.obj_OBJECTS = Objex.Structure_Get
            '    _Structure.obj_CLASSES = Classes.Structure_Get

            '    _Tables = New dxoTables()
            '    _Tables.Clear()
            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_APPID, Me))
            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_BLOCKRECORD, Me))
            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_LAYER, Me))

            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_LTYPE, Me))
            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_STYLE, Me))
            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_DIMSTYLE, Me))

            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_UCS, Me))
            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_VIEW, Me))
            '    _Tables.Add(dxfTable.FromTTABLE(_Structure.tbl_VPORT, Me))

            '    _Settings = New dxsSettings()
            '    _Settings.Clear()
            '    _Settings.Add(New dxsHeader(Me))

            'End Set
        End Property
        Public ReadOnly Property Styles As dxoStyles
            Get
                '^the collection of all text styles defined by the current collection of entities
                Return Tables.Styles
            End Get
        End Property



        Public Property SuppressFromFile As Boolean
            Get
                '^if set to True new entities added th the image will be marked
                '^so they will not be written to the DXF File if it is saved to disk
                Return _Structure.SuppressFromFile
            End Get
            Set(value As Boolean)
                '^if set to True new entities added th the image will be marked
                '^so they will not be written to the DXF File if it is saved to disk
                Dim wuz As Boolean = _Structure.SuppressFromFile
                If wuz = value Then Return
                _Structure.SuppressFromFile = value
                RespondToPropertyChange("", New TPROPERTY(70, value, "SuppressFromFile", dxxPropertyTypes.dxf_Boolean, wuz))
            End Set
        End Property
        Public ReadOnly Property SymbolSettings As dxoSettingsSymbol
            Get
                '^the object which carries the settings inherited when new symbols are created by the entity tool
                Return New dxoSettingsSymbol(BaseSettings(dxxSettingTypes.SYMBOLSETTINGS))
            End Get
        End Property
        Public ReadOnly Property TableSettings As dxoSettingsTable
            Get
                '^the object which carries the settings inherited when new tables are created by the entity tool
                Return New dxoSettingsTable(BaseSettings(dxxSettingTypes.TABLESETTINGS))
            End Get
        End Property
        Public Property Tag As String
            Get
                '^a user assignable string for the image
                Return _Structure.Tag
            End Get
            Set(value As String)
                '^a user assignable string for the image
                _Structure.Tag = value
            End Set
        End Property
        Public ReadOnly Property TextSettings As dxoSettingsText
            Get
                Return New dxoSettingsText(BaseSettings(dxxSettingTypes.TEXTSETTINGS))
            End Get
        End Property
        Public ReadOnly Property UCS As dxfPlane
            Get
                '^the current user coordinate system
                Return New dxfPlane(obj_UCS, GUID)
            End Get
        End Property



        Public ReadOnly Property ViewPorts As dxoViewPorts
            Get
                '^provides  access to the images ViewPorts table
                Return Tables.ViewPorts
            End Get
        End Property
        Public ReadOnly Property Views As dxoViews
            Get
                '^provides  access to the images view table
                Return Tables.Views
            End Get
        End Property

        ''' <summary>
        ''' the world X coordinate of the current UCS
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Double
            Get
                Return obj_UCS.Origin.X
            End Get
            Set(value As Double)
                UCS.X = value
            End Set
        End Property
        ''' <summary>
        ''' the world Y coordinate of the current UCS
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Double
            Get
                Return obj_UCS.Origin.Y
            End Get
            Set(value As Double)
                UCS.Y = value
            End Set
        End Property

        ''' <summary>
        ''' the world Z coordinate of the current UCS
        ''' </summary>
        ''' <returns></returns>
        Public Property Z As Double
            Get
                Return obj_UCS.Origin.Z
            End Get
            Set(value As Double)
                UCS.Z = value
            End Set
        End Property

        ''' <summary>
        ''' the X Direction of the current UCS
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property XDirection As dxfDirection
            Get
                Return New dxfDirection(obj_UCS.XDirection)
            End Get
        End Property

        ''' <summary>
        ''' the Y Direction of the current UCS
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property YDirection As dxfDirection
            Get
                Return New dxfDirection(obj_UCS.YDirection)
            End Get
        End Property

        ''' <summary>
        ''' the Z Direction of the current UCS
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ZDirection As dxfDirection
            Get
                Return New dxfDirection(obj_UCS.ZDirection)
            End Get
        End Property

        Private _Tables As dxoTables
        Public ReadOnly Property Tables As dxoTables
            Get
                If Disposed Then Return Nothing
                If _Tables Is Nothing Then _Tables = New dxoTables(True, Me)
                _Tables.ImageGUID = GUID
                Return _Tables
            End Get
        End Property

        Private _Settings As dxsSettings
        Public ReadOnly Property Settings_Objects As dxsSettings
            Get
                If Disposed Then Return Nothing
                If _Settings Is Nothing Then _Settings = New dxsSettings(Me)
                _Settings.ImageGUID = GUID
                Return _Settings
            End Get
        End Property


#End Region 'Properties
#Region "Rendering Methods"
        Public Function Settings(aSettingsType As dxxSettingTypes) As idxfSettingsObject
            Select Case aSettingsType
                Case dxxSettingTypes.DIMOVERRIDES
                    Return DimStyleOverrides
                Case dxxSettingTypes.DIMSETTINGS
                    Return DimSettings
                Case dxxSettingTypes.TABLESETTINGS
                    Return TableSettings
                Case dxxSettingTypes.HEADER
                    Return Header
                Case dxxSettingTypes.LINETYPESETTINGS
                    Return LinetypeLayers
                Case dxxSettingTypes.SYMBOLSETTINGS
                    Return SymbolSettings
                Case dxxSettingTypes.TEXTSETTINGS
                    Return TextSettings
                Case Else
                    Return Nothing
            End Select
        End Function
        Friend Sub AddDimStyleReference(aEntity As dxfEntity)
            If aEntity Is Nothing Then Return
            Dim aTbl As dxoDimStyles = DimStyles
            Dim sName As String
            sName = aEntity.DimStyleName
            If sName = "" Then Return
            Dim aEntry As dxoDimStyle = Nothing
            If Not aTbl.TryGetEntry(sName, rEntry:=aEntry) Then Return
            aEntry.ReferenceADD(aEntity.Handle)

        End Sub
        Friend Function ReCalculateExtents(ByRef ioDisplay As TDISPLAY, ByRef ioPens As TPENS, uStep As Integer, bRegeneratePaths As Boolean, bJustDraw As Boolean, bSuppressDeviceUpdate As Boolean, ByRef ioTransFormB As TTRANSFORMATION, ByRef ioTransFormS As TTRANSFORMATION) As Double
            Dim _rVal As Double
            Dim aEnts As colDXFEntities
            Dim aEnt As dxfEntity
            Dim entPaths As TPATHS
            Dim drnPaths As TPATHS
            Dim oldExtents As New TPLANE("OLD EXTENTS")
            Dim newExtents As New TPLANE("EXTENTS")
            Dim vwPlane As TPLANE
            Dim aScrn As TSCREEN = obj_SCREEN
            Dim vwDiag As Double
            Dim drwnCnt As Long
            Dim bOffScreen As Boolean
            Dim bDrawit As Boolean
            Dim bRegen As Boolean
            Dim dmn As dxxDrawingDomains
            Dim bDrawUCS As Object = Nothing
            Dim cnt As Long
            Dim bSymbols As Boolean
            Dim bDrawRecs As Object = Nothing
            Dim bDrawExtPts As Object = Nothing
            Dim pxSz As Integer = -1
            Dim recClr As dxxColors = dxxColors.Undefined
            Dim axSz As Double = 0
            Dim bUdateDev As Boolean = ioDisplay.OutputDeviceIsDefined And Not bSuppressDeviceUpdate
            Dim sRegenLst As String = _Structure.RegenList
            Dim bNoScreenEnts As Boolean = obj_SCREEN.Suppressed

            If ioPens.Count <= 0 Then ioPens = New TPENS(Me)
            'If ioTransFormB Is Nothing Then arTransFormB = obj_DISPLAY.ViewTransform
            'If arTransFormS Is Nothing Then arTransFormS = aScrn.ViewTransform
            vwPlane = ioDisplay.pln_VIEW
            vwDiag = vwPlane.Diagonal / 2
            aEnts = Entities
            If Not bJustDraw Then
                oldExtents = ioDisplay.rec_EXTENTS
                If oldExtents.Diagonal = 0 Then oldExtents = ioDisplay.pln_DEVICE.Clone
                'newExtents = oldExtents.Clone(True, "EXTENTS")
                'ioDisplay.rec_EXTENTS = newExtents
            End If
            For i As Integer = 1 To aEnts.Count
                aEnt = aEnts.Item(i)
                bRegen = bRegeneratePaths
                'force path regeneration if the graphic type is in the regen list
                If Not bRegen And sRegenLst <> "" Then
                    bRegen = TLISTS.Contains(aEnt.GraphicType, sRegenLst)
                End If
                dmn = aEnt.Domain
                bDrawit = (dmn <> dxxDrawingDomains.Screen) Or (dmn = dxxDrawingDomains.Screen And Not bNoScreenEnts)
                If bDrawit Then
                    'make sure the entity's work paths are up to date
                    aEnt.UpdatePath(bRegen, Me)
                    'get the entity definition for access to generic path data etc.
                    'exand the extents to include the extent poitns of the entity
                    If Not bJustDraw Then
                        newExtents.ExpandToVectors(aEnt.ExtentPts(False))
                    End If
                    'get the entity's paths
                    entPaths = aEnt.Paths
                    bOffScreen = False
                    'we could see if the entity paths are completely off the device screen but .... nothing yet
                    If Not bOffScreen And entPaths.Count > 0 Then
                        If Not UsingDxfViewer And aEnt.Instances.Count >= 1 Then
                            Dim instPaths As TPATHS = aEnt.Instances.Apply(entPaths)
                            entPaths = instPaths
                        End If

                        'render the paths to the image bitmap
                        drnPaths = Render_Paths(ioPens, entPaths, bIsDeviceCoords:=False, bBitmapOutput:=False, bIgnoreVisibility:=False, bScreenPath:=dmn = dxxDrawingDomains.Screen, ioTransFormB:=ioTransFormB, ioTransFormS:=ioTransFormS)
                        'aEnt.Paths = entPaths
                        If drnPaths.Count > 0 Then
                            cnt += 1
                            drwnCnt += 1 + aEnt.Instances.Count
                            If Not bNoScreenEnts Then
                                If cnt = 1 Then
                                    'see if we are drawing any entity symbols to the screen and draw them for the first drawn entity
                                    bSymbols = aScrn.DrawEntitySymbols(Me, ioDisplay, aEnt, bDrawRecs, bDrawExtPts, bDrawUCS, pxSz, recClr, axSz)
                                Else
                                    If bSymbols Then
                                        'draw subsequent entity symboles
                                        aScrn.DrawEntitySymbols(Me, ioDisplay, aEnt, bDrawRecs, bDrawExtPts, bDrawUCS, pxSz, recClr, axSz)
                                    End If
                                End If
                            End If
                        End If
                        aEnt.DrawnPaths = drnPaths
                        If drwnCnt >= uStep Then
                            'update the output device (if defined)
                            If bUdateDev Then ioDisplay.DrawBitmap(Me)
                            drwnCnt = 0
                        End If
                        'Application.DoEvents()
                    End If
                End If
            Next i
            If bUdateDev Then ioDisplay.DrawBitmap(Me)
            'clear the regenlist
            AddToRegenList(dxxGraphicTypes.Undefined)
            _rVal = oldExtents.Diagonal
            If _rVal <> 0 Then
                _rVal = newExtents.Diagonal / _rVal
            Else
                _rVal = 1
            End If
            If Not bJustDraw Then
                newExtents.Units = ioDisplay.Units
                ioDisplay.rec_EXTENTS = newExtents
            End If
            'save
            obj_SCREEN = aScrn
            obj_DISPLAY = ioDisplay
            Return _rVal
        End Function
        Friend Function Render(Optional aMargin As Double = 0, Optional bRegeneratePaths As Boolean = False, Optional bZoomExtents As Boolean = False, Optional bSetFeatureScale As Boolean = False, Optional bSuppressEvnts As Boolean = False, Optional bSuppressDeviceUpdate As Boolean = False) As Double
            Dim aDisplay As TDISPLAY = obj_DISPLAY
            If Rendering Then Return aDisplay.ZoomFactor
            Dim lZF As Double
            Dim lstView As TPLANE = aDisplay.pln_VIEW
            Dim aPens As New TPENS(0)
            RaiseRenderEvent(True, bZoomExtents, False, bRegeneratePaths, bSuppressEvnts, aMargin)
            Try

                aDisplay.ImageGUID = ""
                Dim viewRec As TPLANE
                Dim extviewRec As TPLANE

                Dim aProps As TPROPERTIES = TPROPERTIES.Null

                Dim fW As Double
                Dim fH As Double
                Dim ZF As Double
                Dim eZF As Double

                Dim iRun As Integer
                Dim bNewExtents As Boolean
                Dim bGoAgain As Boolean
                Dim noextents As Boolean
                Dim aBitmap As dxfBitmap = bmp_Display
                Dim sBitmap As dxfBitmap = bmp_Screen
                aMargin = TVALUES.LimitedValue(aMargin, 0, 100)

                If aBitmap Is Nothing Then
                    aBitmap = New dxfBitmap(aDisplay.WidthI, aDisplay.HeightI, aDisplay.BackGroundColor, "DISPLAY")
                Else
                    aBitmap.Resize(aDisplay.WidthI, aDisplay.HeightI)
                    aBitmap.SetBackColor(aDisplay.BackGroundColor, True)
                End If
                bmp_Display = aBitmap

                If aDisplay.pln_DEVICE.Diagonal <> aBitmap.Diagonal Then
                    aDisplay.Resize(Me, aBitmap.Width, aBitmap.Height)
                End If

                If sBitmap Is Nothing Then
                    sBitmap = New dxfBitmap(aDisplay.WidthI, aDisplay.HeightI, aDisplay.BackGroundColor, "SCREEN")
                Else
                    sBitmap.Resize(aDisplay.WidthI, aDisplay.HeightI)
                    sBitmap.SetBackColor(aDisplay.BackGroundColor, True)
                End If
                bmp_Screen = sBitmap

                aDisplay.ImageGUID = GUID
                lstView = New TPLANE(aDisplay.pln_VIEW)
                lZF = aDisplay.ZoomLast
                aDisplay = TDISPLAY.UpdateStructure(aDisplay, lstView, rChangeProps:=aProps, aImage:=Me, bSuppressEvnts:=True, bNoRedraw:=True)
                obj_DISPLAY = aDisplay
                viewRec = New TPLANE(aDisplay.pln_VIEW)
                If bZoomExtents Then
                    extviewRec = aDisplay.ExtentViewRectangle(Me, aMargin, noextents)
                End If
                ZF = aDisplay.ZoomFactor
                fW = viewRec.Width * ZF
                fH = viewRec.Height * ZF
                'set the focal width and height and center
                If bZoomExtents And Not noextents Then
                    fW = extviewRec.Width
                    fH = extviewRec.Height
                    viewRec.Origin = extviewRec.Origin
                End If
                '================== RENDERING =========================================
                If fW = 0 Then fW = aDisplay.pln_VIEW.Width * ZF
                If fH = 0 Then fH = aDisplay.pln_VIEW.Height * ZF
                If noextents And bZoomExtents And Entities.Count <= 0 Then viewRec.Origin = New TVECTOR
                viewRec.Width = fW
                viewRec.Height = fH
                Dim TransFormB As TTRANSFORMATION = aDisplay.ViewTransform
                Dim TransFormS As TTRANSFORMATION = obj_SCREEN.ViewTransform

                'get the pens
                aPens = New TPENS(Me)
                'clear the current display
                'calculate the extents and draw as you go
                '================== RENDERING TO BITMAP =========================================
                iRun = 0
                bNewExtents = True
                Do While bNewExtents
                    iRun += 1
                    aBitmap.FloodFill(aDisplay.BackGroundColor)
                    viewRec.Name = "VIEW"
                    If bZoomExtents Then
                        ZF = aDisplay.FactorToView(viewRec)
                    End If
                    aDisplay = TDISPLAY.UpdateStructure(aDisplay, viewRec, aProps, ZF, aImage:=Me, bSuppressEvnts:=True, bNoRedraw:=True, bSuppressImageUpdate:=True)
                    If bmp_Background IsNot Nothing Then
                        If bmp_Background.ImageScale <= 0 Then bmp_Background.ImageScale = aDisplay.ZoomFactor
                        'eZF = TVALUES.To_DBL(ZF / bmp_Background.ImageScale)
                        bmp_Background.DrawToBitmap(aBitmap)
                        'bmp_StretchTo(aDisplay.BACKGRND, aDisplay.BMAP.hDC, eZF, eZF, dxxRectangularAlignments.MiddleCenter)
                    End If
                    obj_DISPLAY = aDisplay
                    'aPens = GetPens()
                    If Not bSuppressDeviceUpdate Then aDisplay.DrawBitmap(Me, aBitmap)
                    eZF = ReCalculateExtents(aDisplay, aPens, 10, bRegeneratePaths, iRun > 1, bSuppressDeviceUpdate, TransFormB, TransFormS)
                    bNewExtents = eZF <> 1
                    If iRun > 4 Then bNewExtents = False
                    aDisplay = obj_DISPLAY
                    If bZoomExtents And bNewExtents Then
                        If aDisplay.rec_EXTENTS.Diagonal > 0 Then
                            extviewRec = aDisplay.ExtentViewRectangle(Me, aMargin)
                            eZF = extviewRec.Diagonal / viewRec.Diagonal  'aDisplay.FactorToView(extviewRec)
                            bGoAgain = Math.Round(eZF, 4) <> Math.Round(aDisplay.ZoomFactor, 4)
                            If bGoAgain Then
                                aDisplay.ZoomFactor = eZF
                                viewRec = extviewRec.Clone
                                obj_DISPLAY = aDisplay
                                TransFormB = obj_DISPLAY.ViewTransform
                            Else
                                bNewExtents = False
                            End If
                        End If
                    Else
                        Exit Do
                    End If
                Loop
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType(), ex)
            Finally
                '================== RENDERING TO BITMAP =========================================
                'Bitmap.WriteBMP "C:\JunK\Render.BMP"
                '+++++++++++++++++++++ make sure the current view rectangle matches the extent rectangle with the buffer ++++++++++
Done:
                aDisplay.pln_VIEWLAST = lstView
                aDisplay.ZoomLast = lZF
                obj_DISPLAY = aDisplay
                Rendering = False
                IsDirty = False
                'add the ucs icon
                If Header.UCSMode > 0 Then
                    Render_UCSICon(True, False)
                End If
                'draw the screen domain
                obj_SCREEN.Refresh(Me, aPens, True)
                'update the features scales if requested
                If bSetFeatureScale Then
                    SetFeatureScales(1 / aDisplay.ZoomFactor, True, True)
                End If
                'update the output device if it is defined
                If Not bSuppressDeviceUpdate Then
                    aDisplay.DrawBitmap(Me)
                End If
                'tell the image ti raise it's render event
                RaiseRenderEvent(False, bZoomExtents, False, bRegeneratePaths, bSuppressEvnts, aMargin)
            End Try
            'return the current zoom factor
            Return aDisplay.ZoomFactor
        End Function
        Friend Function Render_Entity(aEntity As dxfEntity, Optional bExcludeFromExtents As Boolean = False, Optional bSuppressRefresh As Boolean = False, Optional bSuppressRectangles As Boolean = False) As Integer
            Dim _rVal As Integer
            '#1the subject entity
            '#2flag to exclude then entity for then current extents calculations
            '#3flag to suppress the refresh of the current images display
            '#4flag to suppress the drawing of the entities bounding rectangle
            '^used to draw an entity to the images display
            If aEntity Is Nothing Then Return _rVal


            Dim aDisplay As TDISPLAY = obj_DISPLAY
            Dim entRecs As Boolean
            Dim aScrn As TSCREEN = obj_SCREEN
            Dim bScrnRefrs As Boolean
            Dim aPens As New TPENS(0)
            Try
                'draw to the bitmap
                If bSuppressRectangles Then entRecs = False
                aEntity.UpdatePath(False, Me)
                Dim ePaths As TPATHS = aEntity.Paths


                Dim dPaths As TPATHS
                If Not Erasing Then
                    aEntity.ImageGUID = GUID
                    '        ePaths.PixelSize = ascrn.GetPropVal( scrn_PointSize)

                    dPaths = Render_Paths(aPens, ePaths)
                    If dPaths.Count > 0 And ePaths.Domain <> dxxDrawingDomains.Screen Then
                        If Not Erasing Then
                            aScrn.DrawEntitySymbols(Me, aDisplay, aEntity, Nothing, Nothing, Nothing)
                        End If
                    End If
                    _rVal = dPaths.Count
                    'aEntity.Paths = ePaths
                    aEntity.DrawnPaths = dPaths
                    If Not bExcludeFromExtents Then
                        aDisplay.rec_EXTENTS = TVECTORS.Appended(ePaths.ExtentVectors, aDisplay.rec_EXTENTS.Corners).Bounds(aDisplay.rec_EXTENTS)
                        aDisplay.UpdateImage(Me, TPROPERTIES.Null, aRedraw:=False, bSuppressViewChange:=True)
                    End If
                Else
                    dPaths = aEntity.DrawnPaths
                    Dim nPaths As TPATHS = Render_Paths(aPens, dPaths, bIgnoreVisibility:=True)
                    _rVal = nPaths.Count
                    aScrn.DrawEntitySymbols(Me, aDisplay, aEntity, Nothing, Nothing, Nothing, 0, aDisplay.BackColor, 0)
                End If
                'paste the image to the output device if there is one
                If Not bSuppressRefresh Then
                    obj_DISPLAY.DrawBitmap(Me)
                    If bScrnRefrs Then
                        obj_SCREEN.Refresh(Me, aPens, False)
                    End If
                End If

                Return _rVal
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("img_Entity_Render ERROR: " & ex.Message)
                Return _rVal
            End Try
        End Function
        Friend Function Render_Paths(ByRef ioPens As TPENS, aPaths As TPATHS, Optional bIsDeviceCoords As Boolean = False, Optional bBitmapOutput As Boolean = False, Optional bIgnoreVisibility As Boolean = False, Optional bScreenPath As Boolean = False) As TPATHS
            Dim ioTransFormB As TTRANSFORMATION = Nothing
            Dim ioTransFormS As TTRANSFORMATION = Nothing
            Return Render_Paths(ioPens, aPaths, bIsDeviceCoords, bBitmapOutput, bIgnoreVisibility, bScreenPath, ioTransFormB, ioTransFormS)
        End Function
        Friend Function Render_Paths(ByRef ioPens As TPENS, aPaths As TPATHS, bIsDeviceCoords As Boolean, bBitmapOutput As Boolean, bIgnoreVisibility As Boolean, bScreenPath As Boolean, ByRef ioTransFormB As TTRANSFORMATION, ByRef ioTransFormS As TTRANSFORMATION) As TPATHS
            Dim _rVal As New TPATHS(aPaths, bNoMembers:=True)
            If aPaths.Count <= 0 Or aPaths.Suppressed Then Return _rVal
            If aPaths.Domain = dxxDrawingDomains.Screen And obj_SCREEN.Suppressed Then Return _rVal

            If ioPens.Count <= 0 Then ioPens = New TPENS(Me)
            'If ioTransFormB Is Nothing Then ioTransFormB = obj_DISPLAY.ViewTransform
            'If ioTransFormS Is Nothing Then ioTransFormS = obj_SCREEN.ViewTransform
            '_rVal = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TPATHS)(aPaths)
            Dim dPath As TPATH = New TPATH("")
            For i As Integer = 1 To aPaths.Count
                If Render_Path(ioPens, aPaths, i, bIgnoreVisibility, bIsDeviceCoords, bScreenPath, ioTransFormB, ioTransFormS, rPath:=dPath) Then
                    _rVal.Add(dPath)
                End If
            Next i
            If bBitmapOutput Then
                obj_DISPLAY.DrawBitmap(Me)
            End If
            Return _rVal
        End Function
        Friend Function Render_Path(ioPens As TPENS, aPaths As TPATHS, aPathIndex As Integer, bIgnoreVisibility As Boolean, bIsDeviceCoords As Boolean, Optional bForceScreenPath As Boolean = False) As Boolean
            Dim ioTransFormB As TTRANSFORMATION = Nothing
            Dim ioTransFormS As TTRANSFORMATION = Nothing
            Dim rPath As TPATH = Nothing
            Return Render_Path(ioPens, aPaths, aPathIndex, bIgnoreVisibility, bIsDeviceCoords, bForceScreenPath, ioTransFormB, ioTransFormS, rPath)
        End Function
        Friend Function Render_Path(ByRef ioPens As TPENS, aPaths As TPATHS, aPathIndex As Integer, bIgnoreVisibility As Boolean, bIsDeviceCoords As Boolean, bForceScreenPath As Boolean, ByRef ioTransFormB As TTRANSFORMATION, ByRef ioTransFormS As TTRANSFORMATION, ByRef rPath As TPATH) As Boolean
            Dim _rVal As Boolean
            '#1the subject Image
            '#2the pens defined on the subject images device
            '#3the paths array to get the subject path from
            '#4the subject device context
            '#5the index of the path to draw
            '#6flag to to draw the path regardless of its current visibility
            '#7flag idicating that the path is a screen entity not a real entity
            '^renders the passed path to the current bitmap
            '    If aViewExtentPt Is Nothing Then Set aViewExtentPt = New colDXFVectors
            'Dim rPath As TPATH
            Dim vWorld As TVECTOR
            Dim vDevice As TVECTOR
            Dim vView As TVECTOR
            Dim pPen As TPEN
            Dim li As Integer
            Dim vi As Integer
            Dim bVisbl As Boolean
            Dim lname As String
            Dim bScreenPath As Boolean
            Dim pxSize As Integer = aPaths.PixelSize
            Dim iDisplay As TDISPLAY = obj_DISPLAY

            Dim gPath As Drawing2D.GraphicsPath
            Dim TForm As TTRANSFORMATION
            Dim vPath As TVECTORS
            'If ioTransFormB Is Nothing Then ioTransFormB = obj_DISPLAY.ViewTransform
            'If ioTransFormS Is Nothing Then ioTransFormS = obj_SCREEN.ViewTransform
            Try
                rPath = New TPATH(aPaths.Item(aPathIndex))
                'no loops means nothing to draw
                If rPath.LoopCount <= 0 Then Return False
                'If rPath.GraphicType = dxxGraphicTypes.MText Then
                'End If
                'get the pens collection
                If ioPens.Count <= 0 Then ioPens = New TPENS(Me)
                If aPaths.Domain <> dxxDrawingDomains.Screen Then
                    'get the layer visiblity for world (image entity paths)
                    lname = aPaths.LayerName(aPathIndex)
                    If bIgnoreVisibility Then
                        bVisbl = True
                    Else
                        bVisbl = Not rPath.Suppressed
                        'the image pens has a list of the currently invisible layers
                        If bVisbl Then bVisbl = Not ioPens.InvisibleLayers.Contains(lname)
                    End If
                Else
                    'don't draw if it's a screen path or the path is suppressed
                    bVisbl = Not obj_SCREEN.Suppressed And Not rPath.Suppressed
                    pxSize = obj_SCREEN.PointSize
                End If
                If aPaths.Domain = dxxDrawingDomains.Screen Or bForceScreenPath Then
                    TForm = ioTransFormS
                    If bVisbl Then
                        bScreenPath = True
                    End If
                Else
                    TForm = ioTransFormB
                End If
                'determime if we will render this path
                _rVal = bVisbl Or bIgnoreVisibility Or Erasing
                'bail out as we are not drawing this path
                If Not _rVal Then Return _rVal
                'get the image and screen bitmaps
                Dim aBMP As dxfBitmap = GetBitmap(False)
                Dim sBMP As dxfBitmap = GetBitmap(True)


                'convert world points to device points
                For li = 1 To rPath.LoopCount
                    Dim vWorldPts As TVECTORS = rPath.Looop(li)
                    If Not bIsDeviceCoords Or rPath.Relative Then
                        vPath = New TVECTORS(vWorldPts.Count)
                        For vi = 1 To vWorldPts.Count
                            'render the path in the image
                            vWorld = vWorldPts.Item(vi)
                            'if the path points are relative to the path coordinate system then we convert to world
                            If rPath.Relative Then
                                vWorld = rPath.Plane.WorldVector(vWorld)
                            End If

                            If Not bIsDeviceCoords Then
                                'convert world points to view points
                                vView = iDisplay.WorldToView(vWorld)
                                'convert view points to device points
                                vDevice = iDisplay.ViewToDevice(vView)
                                'v1 = TMATRIX4.PlanarTransformMatrix(iDisplay.pln_VIEW).Multiply(vWorld)
                                'v2 = TMATRIX4.PlanarTransformMatrix(iDisplay.pln_DEVICE).Multiply(v1)
                            Else
                                vView = New TVECTOR(vWorld)
                                vDevice = New TVECTOR(vWorld)
                            End If
                            'save the device point to the path
                            vPath.SetItem(vi, vDevice)
                        Next vi
                    Else
                        'the path is already in device coordinates to just use them as they are
                        vPath = vWorldPts
                    End If
                    If Not UsingDxfViewer Then
                        'define the pen
                        pPen = ioPens.GetPathPen(Me, aPaths, aPathIndex)
                        'convert the dxfPath to a GraphicsPath
                        gPath = vPath.ToGraphicPath(pxSize, aBMP.Size)
                        'rend to the bitmap
                        aBMP.Render(gPath, rPath.Filled, pPen)
                        If bScreenPath Then
                            'also add it to the screen bitmap
                            sBMP.Render(gPath, rPath.Filled, pPen)
                        End If
                    End If
                Next li
                'aPaths.Item(aPathIndex) = rPath
                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                Return _rVal
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
                Return _rVal
            End Try
        End Function
        Friend Function RenderToDevice(aDevice As Object, aViewPlane As TPLANE, aIconMode As dxxUCSIconModes, bShowLineWeight As Boolean, aColorMode As dxxColorModes, bZoomExtents As Boolean, aZoomFactor As Double, aLWTScale As Double, aZoomBuffer As Double, aBackColor As Integer, ByRef rBitmap As dxfBitmap, Optional bSuppressNoPlotLayers As Boolean = False, Optional bSuppressScreenEnts As Boolean = True) As TDISPLAY
            'On Error Resume Next
            '#1the subject image
            '#2the device to draw the current image to
            '#3the view plane to display
            '#4the icon mode to apply
            '#5flag to show line weights
            '#6a color mode to apply
            '#7flag to zoom the image to the extents of the device
            '#8a zoom factor to use
            '#9a line weight scale to apply
            '#10a zoom buffer factor to apply
            '#11a background color to use
            '#12returns the bitmap drawn to the device
            Dim deType As String
            Dim bExts As Boolean
            Dim aBuf As Double
            Dim aScrn As TSCREEN = obj_SCREEN
            Dim bScrn As New TSCREEN(aScrn)
            Dim aProps As New TPROPERTIES
            Dim ZF As Double
            Dim oldLayers As dxfTable = Layers
            Header.StoreProperties()

            Dim aBMP As dxfBitmap = GetBitmap(False)
            If bSuppressNoPlotLayers Then
                For Each layer As dxoLayer In oldLayers

                    If Not layer.PropValueB(dxxLayerProperties.PlotFlag) Then
                        layer.PropValueSet(dxxLayerProperties.Visible, False, True)
                    End If
                Next
            End If

            If bSuppressScreenEnts Then
                bScrn.BoundingRectangles = False
                bScrn.ExtentPts = False
                bScrn.OCSs = False
                bScrn.SetValue(dxxScreenProperties.ExtentRectangle, False)
                '        bScrn.Entities.Count = 0
            End If
            aBuf = 1
            Dim aDisplay As TDISPLAY = obj_DISPLAY
            Dim bDisplay As TDISPLAY = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TDISPLAY)(aDisplay)
            bDisplay.AutoRedraw = False
            If aColorMode >= 0 And aColorMode <= 2 Then bDisplay.ColorMode = aColorMode
            If aIconMode >= 0 And aIconMode <= 2 Then
                Header.Properties.SetVal("$UCSMODE", aIconMode, bSuppressEvents:=True)
            End If
            If bShowLineWeight Then
                Header.Properties.SetVal("$LWDISPLAY", 1, bSuppressEvents:=True)
            Else
                Header.Properties.SetVal("$LWDISPLAY", 0, bSuppressEvents:=True)
            End If
            If aLWTScale > 0 Then
                Header.Properties.SetVal("$LWSCALE", aLWTScale, bSuppressEvents:=True)
            End If
            bScrn.Suppressed = bSuppressScreenEnts
            obj_SCREEN = bScrn
            ZF = bDisplay.ZoomFactor
            If aZoomFactor > 0 And Not bZoomExtents Then
                ZF = aZoomFactor
            ElseIf bZoomExtents Then
                aBuf = aZoomBuffer
                bExts = True
            End If
            bDisplay.AutoRedraw = False
            bDisplay.ImageGUID = GUID
            deType = TypeName(aDevice).Trim().ToUpper()
            bDisplay.SetDevice(aDevice, aBackColor, True, Me)
            bDisplay = TDISPLAY.UpdateStructure(bDisplay, ioViewPlane:=aViewPlane, rChangeProps:=aProps, aZoomFactor:=ZF, aBackColor:=aBackColor, aImage:=Me, bSuppressEvnts:=True)
            Render(aBuf, bZoomExtents:=bExts, bSuppressEvnts:=True, bSuppressDeviceUpdate:=deType = "DXFBITMAP")
            bDisplay = obj_DISPLAY
            rBitmap.Bitmap = aBMP.Bitmap.Clone
            obj_DISPLAY = bDisplay
            obj_SCREEN = aScrn
            Header.RestoreProperties()
            Return obj_DISPLAY
        End Function
        Friend Sub Render_UCSICon(bSuppressBitmapOutput As Boolean, bEraseLast As Boolean)
            Dim aUCS As TPLANE = obj_UCS.Clone
            Dim aDisplay As TDISPLAY = obj_DISPLAY
            Dim aHdr As dxsHeader = Header
            Dim devPln As TPLANE = aDisplay.pln_DEVICE
            Dim aIcon As TPATHS = pth_UCSICON
            Dim bIcon As TPATHS
            Dim bRec As TPLANE
            Dim pVecs As TVECTORS
            Dim lng As Double
            Dim icMode As dxxUCSIconModes = aHdr.UCSMode
            Dim aPth As TPATH
            Try
                If bEraseLast Then
                    If aIcon.Count > 0 Then
                        Erasing = True
                        Render_Paths(New TPENS(0), aIcon, True, bIgnoreVisibility:=True)
                        Erasing = False
                    End If
                End If
                bIcon = New TPATHS(dxxDrawingDomains.Model)
                If icMode <> dxxUCSIconModes.None Then
                    lng = (aHdr.UCSSize / 100 * devPln.Height) / aDisplay.PixelsPerUnit / aDisplay.ZoomFactor
                    bIcon.Clear()
                    aPth = TPATH.UCS(aUCS, aColor:=aHdr.UCSColor, aLength:=lng)
                    bIcon.Add(aPth)
                    pVecs = bIcon.Item(1).Looop(1)
                    pVecs = aDisplay.WorldsToDevice(pVecs)
                    'pVecs.Append(TPATH.RECTANGLE(bRec, False).Looop(1))
                    bRec = pVecs.Bounds(devPln)
                    'see if the icon is off the screen
                    If icMode = dxxUCSIconModes.LowerLeft Then
                        bRec.Expand(20, 20)
                        If Not devPln.Limits.Contains(bRec.Limits) Then
                            Dim v1 As TVECTOR = devPln.Point(dxxRectanglePts.BottomLeft)
                            Dim v2 As TVECTOR = bRec.Point(dxxRectanglePts.BottomLeft)
                            pVecs.Translate(v1 - v2)
                        End If
                    End If
                    aPth.SetLoop(1, pVecs)
                    bIcon.SetItem(1, aPth)
                    'bIcon.Print()
                    'aIcon.Members(0).Loops(0).Print
                    bIcon.Domain = dxxDrawingDomains.Screen
                    Render_Paths(New TPENS(0), bIcon, bIsDeviceCoords:=True, bBitmapOutput:=Not bSuppressBitmapOutput, bIgnoreVisibility:=True)
                End If
                pth_UCSICON = bIcon
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Render_UCSICon ERROR: " & ex.Message)
            End Try
        End Sub
#End Region 'Rendering Methods
#Region "Protected Methods"
        Friend Function GroupNamesDictionary(ByRef rIndex As Integer) As dxfoDictionary
            Dim _rVal As dxfoDictionary
            _rVal = Objex.VerifyNamedDictionary("ACAD_GROUP", False, Me)
            rIndex = _rVal.Index
            Return _rVal
        End Function
        Friend Function UpdateViewPort(aViewPort As TTABLEENTRY) As TTABLEENTRY
            Dim _rVal As TTABLEENTRY
            If aViewPort.EntryType <> dxxReferenceTypes.VPORT Then
                _rVal = New TTABLEENTRY(dxxReferenceTypes.VPORT, "*Active")
            Else
                _rVal = aViewPort
            End If
            Dim aPlane As TPLANE = obj_DISPLAY.pln_VIEW
            _rVal.Props.SetValueGC(16, aPlane.ZDirection)
            _rVal.Props.SetVectorGC(12, aPlane.Origin)
            _rVal.Props.SetValueGC(146, aPlane.Origin.Z)
            _rVal.Props.SetVectorGC(110, obj_UCS.Origin)
            _rVal.Props.SetVectorGC(111, obj_UCS.XDirection)
            _rVal.Props.SetVectorGC(112, obj_UCS.YDirection)
            _rVal.Props.SetVectorGC(113, obj_UCS.ZDirection)
            _rVal.Props.SetValueGC(40, aPlane.Height)
            _rVal.Props.SetVal("*Width", aPlane.Width)
            _rVal.Props.SetValueGC(51, -aPlane.Yaw)
            _rVal.Props.SetValueGC(41, aPlane.AspectRatio(True))
            Return _rVal
        End Function
        Friend Sub Init(Optional aRefsToRetain As dxxReferenceTypes = dxxReferenceTypes.UNDEFINED, Optional aBackColor As Color? = Nothing)
            '#1addative enum indicating the references to retain
            '^intitialize the image to empty
            'make sure the available fonts are loaded

            Dim oldStruc As TIMAGE
            Dim bFirstTime As Boolean = aRefsToRetain = -100
            Dim oldTbl As dxfTable
            Dim newTbl As dxfTable
            Dim bKeepBlocks As Boolean
            Dim bResetView As Boolean
            Dim sRetain As New TLIST(",")
            Dim aVers As dxxACADVersions
            Dim aRefType As dxxReferenceTypes
            Dim iGUID As String = GUID
            Dim bKeep As Boolean
            Dim oldSettings As New List(Of TTABLEENTRY)
            Dim oldBlks As New List(Of dxfBlock)
            Dim tables_Old As dxoTables = Nothing
            Dim oldObjs As New List(Of dxfObject)
            Dim settings_Old As dxsSettings = Nothing
            Dim keepNames As List(Of String)
            Dim dsp As TDISPLAY = _Structure.obj_DISPLAY
            'On Error Resume Next
            If bFirstTime Then
                iGUID = dxfEvents.NextImageGUID()
            Else
                _Structure.Clears += 1
                iGUID = dxfEvents.NextImageGUID(iGUID)
                dxfGlobals.Aggregator.Unsubscribe(Me)
            End If
            dxfGlobals.Aggregator.Subscribe(Me)
            'If _Events IsNOt Nothing Then RemoveHandler _Events.ImageRequest, AddressOf _Events_ImageRequest
            '_Events = Nothing
            If Not aBackColor.HasValue And _Bitmap IsNot Nothing Then aBackColor = _Bitmap.BackgroundColor
            If Not aBackColor.HasValue Then aBackColor = Color.White
            If aBackColor.Value.Name = "0" Then aBackColor = Color.White
            Try
                If _Bitmap IsNot Nothing Then _Bitmap.FloodFill(aBackColor.Value) Else _Bitmap = New dxfBitmap(aBackColor)
            Catch ex As Exception

            End Try
            Try
                If _ScreenBitmap IsNot Nothing Then _ScreenBitmap.FloodFill(aBackColor.Value) Else _ScreenBitmap = New dxfBitmap(aBackColor)
            Catch ex As Exception

            End Try


            _Bitmap.IMageGUID = GUID
            If bFirstTime Then
                _Tables = New dxoTables(True, Me)
                _Settings = New dxsSettings(Me)
                Name = String.Empty

                FileType = dxxFileTypes.DXF
                UsingDxfViewer = False
                _Structure.obj_DISPLAY.BackGroundColor = aBackColor
                _Structure = New TIMAGE(_Structure.obj_DISPLAY, "") With {.GUID = iGUID}
                oldStruc = _Structure
                aVers = dxxACADVersions.R2007
                _HandleGenerator = New dxoHandleGenerator(Me, iGUID)
                _Objects = New colDXFObjects(Me, iGUID)
                _Blocks = New colDXFBlocks(Me, iGUID)
            Else
                'retain these
                tables_Old = Tables
                settings_Old = Settings_Objects
                oldSettings = BaseSettingsCol
                'Dim oBlk As dxfBlock
                'Dim oObj As dxfObject
                'For b As Integer = 1 To Blocks.Count
                '    oBlk = Blocks.Item(b)
                '    oldBlks.Add(oBlk.Name, oBlk.Clone)
                'Next
                oldBlks = Blocks.Clone()
                'For b As Integer = 1 To Objex.Count
                '    oObj = Objex.Item(b)
                '    oldObjs.Add(oObj.Name, oObj.Clone)
                'Next
                oldObjs = Objex.Clone()
                oldStruc = _Structure
                bResetView = True
                aVers = FileVersion
                _Blocks = New colDXFBlocks(Me, iGUID)
                _Entities.Clear(bDestroy:=True, Me)
                _Structure = New TIMAGE(oldStruc.obj_DISPLAY, "")
                '_Blocks.SwapCollection(oldBlks)
                sRetain.AddList(TVALUES.BitCode_Decompose(aRefsToRetain).ToList())
                _HandleGenerator.Clear()
                _Objects.Dispose()
                _Objects = New colDXFObjects(Me, iGUID)
            End If
            _Structure.GUID = iGUID
            _HandleGenerator.AssignTo(_Blocks)
            _Entities = New colDXFEntities(Me)
            _HandleGenerator.AssignTo(_Entities)
            FileVersion = aVers
            If Not bFirstTime Then
                bKeepBlocks = sRetain.Contains(dxxReferenceTypes.BLOCK_RECORD)
                Dim aBlk As dxfBlock
                Dim aEntry As dxfTableEntry
                keepNames = dxfTable.DefaultMemberNames(dxxReferenceTypes.BLOCK_RECORD, bReturnUpperCase:=True)
                For Each aBlk In oldBlks
                    If bKeepBlocks Or keepNames.IndexOf(aBlk.Name.ToUpper) >= 0 Then
                        _Blocks.AddToCollection(aBlk, bSuppressEvnts:=True, aImage:=Me)
                    End If
                Next
                _HandleGenerator.SaveUsedHandles(_Blocks)
                For Each oldTbl In tables_Old
                    keepNames = dxfTable.DefaultMemberNames(oldTbl.TableType, True)
                    aRefType = oldTbl.TableType
                    If aRefType <> dxxReferenceTypes.BLOCK_RECORD Then
                        If sRetain.Contains(aRefType) Then keepNames.AddRange(oldTbl.GetNames(bReturnUpperCase:=True))
                    Else
                        keepNames.AddRange(_Blocks.GetNames(bReturnUpperCase:=True))
                    End If
                    newTbl = dxfTable.CreateTable(oldTbl.TableType)
                    For Each aEntry In oldTbl
                        If keepNames.IndexOf(aEntry.Name.ToUpper) >= 0 Then
                            If aEntry.EntryType = dxxReferenceTypes.STYLE Then
                                If Not sRetain.Contains(aRefType) Then
                                    Dim style As dxoStyle = DirectCast(aEntry, dxoStyle)
                                    style.UpdateFontName("Arial.ttf", dxfEnums.Description(dxxTextStyleFontSettings.Regular))
                                End If
                            ElseIf aEntry.EntryType = dxxReferenceTypes.DIMSTYLE Then
                                If Not sRetain.Contains(aRefType) Then
                                    aEntry.PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, "Standard")
                                End If
                            End If
                            newTbl.Add(aEntry)
                        End If
                    Next
                    _HandleGenerator.SaveUsedHandles(newTbl)

                    oldTbl.Clear()
                Next
                Dim oldSets As TTABLEENTRY
                For Each oldSets In oldSettings
                    aRefType = oldSets.EntryType
                    bKeep = sRetain.Contains(aRefType)
                    If bKeep Then
                        _Structure.BaseSettings_Set(oldSets)
                    End If
                Next
            End If  'not first time
            If Not bFirstTime Then
                Dim objs As New List(Of dxfObject)
                Dim aobj As dxfObject = Nothing
                Dim nmDic As dxfoDictionary = Nothing
                Dim grpDic As dxfoDictionary
                keepNames = New List(Of String)({"DICTIONARY NAMES", "ACAD_GROUP"})
                For Each aoj In oldObjs
                    bKeep = False
                    If aobj.ObjectType = dxxObjectTypes.Dictionary Then
                        If String.Compare(aobj.Name, "DICTIONARY NAMES", ignoreCase:=True) = 0 Then
                            bKeep = True
                            nmDic = aobj
                            nmDic.Entries = New TDICTIONARYENTRIES(nmDic.NameGroupCode, nmDic.HandleGroupCode)
                            '_HandleGenerator.AssignTo(nmDic)
                        ElseIf String.Compare(aobj.Name, "ACAD_GROUP", ignoreCase:=True) = 0 Then
                            bKeep = True
                            grpDic = aobj
                            '_HandleGenerator.AssignTo(grpDic)
                            grpDic.Entries = New TDICTIONARYENTRIES(grpDic.NameGroupCode, grpDic.HandleGroupCode)
                            If nmDic IsNot Nothing Then nmDic.AddEntry(grpDic.Name, grpDic.Handle)
                        End If
                    End If
                    If bKeep Then
                        objs.Add(aobj)
                    End If
                Next
                _Objects.AddRange(objs)
                _HandleGenerator.SaveUsedHandles(_Objects)
            End If
            dxfImageTool.VerifyDefaultMembers(Me)

            '
            If bFirstTime Then oldStruc = _Structure
            If bResetView Then
                dsp.pln_VIEWLAST = oldStruc.obj_DISPLAY.pln_VIEW
                dsp.ZoomLast = oldStruc.obj_DISPLAY.ZoomFactor
                dsp.pln_VIEW = New TPLANE("VIEW", dxxDeviceUnits.Inches)
                dsp.ZoomLast = dsp.ZoomFactor
                dsp.ZoomFactor = 1
            Else
                dsp = oldStruc.obj_DISPLAY
            End If
            dsp.DPI = _Bitmap.DPI
            dsp.BackGroundColor = _Bitmap.BackgroundColor
            dsp.pln_DEVICE = _Bitmap.Plane
            _Structure.obj_DISPLAY = dsp
            If bKeepBlocks Then
                '        For k = 1 To oldBlks.Count
                '            _Blocks.Add
                '        Next k
            End If
            If Not bFirstTime Then
                'reset the dim overrides
                Dim curStye As dxoDimStyle = DimStyle()
                DimStyleOverrides.UpdateToImage(curStye.Name, aImage:=Me)

            End If


            '_Events = goEvents()
            'AddHandler _Events.ImageRequest, AddressOf _Events_ImageRequest
            _Screen = New dxoScreen(Me)
            _Entities.SetGUIDS(iGUID, "", "", dxxFileObjectTypes.Entities, aImage:=Me)
            _Settings.SetImage(Me, True)
        End Sub
        Public Function ReferenceCanBeDeleted(aRefType As dxxReferenceTypes, aHandle As String, bTestHeaderVar As Boolean, ByRef rError As String) As Boolean
            Dim rIsStandardObject As Boolean = False
            Dim rReferencedByRefs As Boolean = False
            Dim rReferencedByBlocks As Boolean = False
            Dim rReferencedByEnts As Boolean = False
            Dim rReferencedByHeader As Boolean = False
            Return ReferenceCanBeDeleted(aRefType, aHandle, bTestHeaderVar, rIsStandardObject, rReferencedByRefs, rReferencedByBlocks, rReferencedByEnts, rReferencedByHeader, rError)
        End Function
        Public Function ReferenceCanBeDeleted(aRefType As dxxReferenceTypes, aHandle As String, bTestHeaderVar As Boolean, ByRef rIsStandardObject As Boolean, ByRef rReferencedByRefs As Boolean, ByRef rReferencedByBlocks As Boolean, ByRef rReferencedByEnts As Boolean, ByRef rReferencedByHeader As Boolean, ByRef rError As String) As Boolean
            rError = $""
            Dim bOKToDelete As Boolean = True
            aHandle = Trim(aHandle)
            rReferencedByEnts = False
            rReferencedByBlocks = False
            rReferencedByRefs = False
            rIsStandardObject = False
            rReferencedByHeader = False
            If aHandle = "" Then Return True
            Dim lVal As Integer
            Dim idx As Integer
            Dim aHdr As dxsHeader = Header
            Dim aTbl As dxfTable = Table(aRefType)
            If aTbl.TableType <> aRefType Then Return True
            Dim aMem As dxfTableEntry = Nothing
            If Not aTbl.TryGetEntry(aHandle, aMem) Then
                Return bOKToDelete
            End If
            Dim aName As String = aMem.Name
            Dim uName As String = UCase(aName)
            Dim tname As String = aTbl.Name
            Dim refName As String = String.Empty
            Select Case aRefType
                Case dxxReferenceTypes.DIMSTYLE
                    rIsStandardObject = (uName = "STANDARD")
                    bOKToDelete = Not rIsStandardObject
                    If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a standard entry in table '{ tname }'"
                    If bOKToDelete Then
                        bOKToDelete = Not aMem.PropValueB(dxxDimStyleProperties.XREFDEPENDANT) 'xRef dependant
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a has XRef Dependancy"
                    End If
                    If bOKToDelete And bTestHeaderVar Then
                        If String.Compare(aHdr.DimStyleName, aName, True) = 0 Then
                            rReferencedByHeader = True
                            bOKToDelete = Not rReferencedByHeader
                            If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the current header '{ tname }'"
                        End If
                    End If
                    If bOKToDelete Then
                        rReferencedByBlocks = Blocks.ReferencesDimStyle(aName, refName)
                        bOKToDelete = Not rReferencedByBlocks
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing block '{ refName }'"
                    End If
                    If bOKToDelete Then
                        rReferencedByEnts = Entities.ReferencesDimStyle(aName)
                        bOKToDelete = Not rReferencedByEnts
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing Entity"
                    End If
                Case dxxReferenceTypes.STYLE
                    rIsStandardObject = (uName = "STANDARD")
                    bOKToDelete = Not rIsStandardObject
                    If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a standard entry in table '{ tname }'"
                    If bOKToDelete Then
                        bOKToDelete = Not aMem.PropValueB(dxxStyleProperties.XREFDEPENANT) 'xRef dependant
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a has XRef Dependancy"
                    End If
                    If bOKToDelete And bTestHeaderVar Then
                        If String.Compare(aHdr.TextStyleName, aName, True) = 0 Then
                            rReferencedByHeader = True
                            bOKToDelete = Not rReferencedByHeader
                            If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the current header '{ tname }'"
                        End If
                    End If
                    If bOKToDelete Then
                        idx = DimStyles.FindIndex(Function(x) String.Compare(x.PropValueStr(dxxDimStyleProperties.DIMTXSTY_NAME), aName, True) = 0)
                        If idx >= 0 Then
                            rReferencedByRefs = True
                        End If
                        bOKToDelete = Not rReferencedByRefs
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing dimstyle '{DimStyles(idx).Name }'"
                    End If
                    If bOKToDelete Then
                        rReferencedByBlocks = Blocks.ReferencesStyle(aName, refName)
                        bOKToDelete = Not rReferencedByBlocks
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing block '{ refName}'"
                    End If
                    If bOKToDelete Then
                        rReferencedByEnts = Entities.ReferencesStyle(aName)
                        bOKToDelete = Not rReferencedByEnts
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing Entity"
                    End If
                Case dxxReferenceTypes.LAYER
                    rIsStandardObject = (uName = "0" Or uName = "DEFPOINTS")
                    bOKToDelete = Not rIsStandardObject
                    If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a standard entry in table '{ tname }'"
                    If bOKToDelete Then
                        bOKToDelete = Not aMem.PropValueB(dxxLayerProperties.XRefDependant) 'xRef dependant
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a has XRef Dependancy"
                    End If
                    If bOKToDelete And bTestHeaderVar Then
                        If String.Compare(aHdr.LayerName, aName, True) = 0 Then
                            rReferencedByHeader = True
                            bOKToDelete = Not rReferencedByHeader
                            If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the current header '{ tname }'"
                        End If
                    End If
                    If bOKToDelete Then
                        rReferencedByBlocks = Blocks.ReferencesLayer(aName, rMemberName:=refName)
                        bOKToDelete = Not rReferencedByBlocks
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing block '{ refName}'"
                    End If
                    If bOKToDelete Then
                        rReferencedByEnts = Entities.ReferencesLayer(aName)
                        bOKToDelete = Not rReferencedByEnts
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing Entity"
                    End If
                Case dxxReferenceTypes.LTYPE
                    rIsStandardObject = (uName = "BYLAYER" Or uName = "BYBLOCK" Or uName = "CONTINUOUS")
                    bOKToDelete = Not rIsStandardObject
                    If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a standard entry in table '{ tname }'"
                    If bOKToDelete Then
                        bOKToDelete = Not aMem.PropValueB(dxxLinetypeProperties.XRefDependant) 'xRef dependant
                    End If
                    If bOKToDelete Then
                        lVal = aMem.Properties.ValueI(70)
                        Do While lVal >= 64
                            lVal -= 64
                        Loop
                        If lVal >= 48 Then bOKToDelete = False 'xRef dependant
                    End If
                    If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is a has XRef Dependancy"
                    If bOKToDelete And bTestHeaderVar Then
                        If String.Compare(aHdr.Linetype, aName, True) = 0 Then
                            rReferencedByHeader = True
                            bOKToDelete = Not rReferencedByHeader
                            If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the current header '{ tname }'"
                        End If
                    End If
                    If bOKToDelete Then
                        Dim layer As dxfTableEntry = Layers.GetByPropertyValue(6, aName, aStringCompare:=True)
                        rReferencedByRefs = layer IsNot Nothing
                        bOKToDelete = Not rReferencedByRefs
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing Layer '{ layer.Name }'"
                    End If
                    If bOKToDelete Then
                        rReferencedByBlocks = Blocks.ReferencesLinetype(aName, rMemberName:=refName)
                        bOKToDelete = Not rReferencedByBlocks
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing Block '{ refName}'"
                    End If
                    If bOKToDelete Then
                        rReferencedByEnts = Entities.ReferencesLinetype(aName)
                        bOKToDelete = Not rReferencedByEnts
                        If Not bOKToDelete Then rError = $"Member '{ aName }' cannot be deleted because it is the currently referenced by an existing Entity"
                    End If
                Case dxxReferenceTypes.UCS
                    rError = $"Deleting '{ tname }' Table Entries is Not currently allowed"
                    Return False
                Case dxxReferenceTypes.VIEW
                    rError = $"Deleting '{ tname }' Table Entries is Not currently allowed"
                    Return False
                Case dxxReferenceTypes.BLOCK_RECORD
                    rError = $"Deleting '{ tname }' Table Entries is Not currently allowed"
                    Return False
                Case dxxReferenceTypes.APPID
                    rError = $"Deleting '{ tname }' Table Entries is Not currently allowed"
                    Return False
                Case dxxReferenceTypes.VPORT
                    rError = $"Deleting '{ tname }' Table Entries is Not currently allowed"
                    Return False
                Case Else
                    Return False
            End Select
            Return bOKToDelete
        End Function
        Friend Function GetPens() As TPENS

            Return New TPENS(Me)

        End Function
        Friend Function GetBitmap(bScreen As Boolean) As dxfBitmap
            If _Disposed Then Return Nothing
            If Not bScreen Then
                Return _Bitmap
            Else
                If _ScreenBitmap.Size <> _Bitmap.Size Then _ScreenBitmap.Resize(_Bitmap.Width, _Bitmap.Height)
                _ScreenBitmap.IMageGUID = GUID
                Return _ScreenBitmap
            End If
        End Function
        Friend Sub SetBlocks(ByRef newobj As colDXFBlocks)
            _Blocks = newobj
            _Blocks.ImageGUID = GUID
        End Sub
        Friend Sub SetEntities(ByRef newobj As colDXFEntities)
            If newobj IsNot Nothing Then _Entities = newobj Else _Entities.Clear()
            _Entities.SetGUIDS(GUID, "", "", dxxFileObjectTypes.Entities, aImage:=Me)
            _Entities.MonitorMembers = True
        End Sub
        Friend Function WriteToFile(aGenerator As dxoFileTool, aFileType As dxxFileTypes, aFileSpec As String, aVersion As dxxACADVersions, bSuppressDimOverrides As Boolean, bNoErrors As Boolean, ByRef rErrString As String, Optional bSuppressDXTNames As Boolean = False, Optional bNumericHandles As Boolean = False, Optional bNoConverter As Boolean = False) As String
            Dim _rVal As String
            '#1the type of file to write
            '#2the filename to write to
            '#3the AutCAD version to write the DWG to
            '^used to write the current file to disk
            '~returns True if the file was successfully written.
            '~all errors are raised to the caller.
            Dim fspec As String

            Dim outspec As String
            Dim aExts As String
            Dim fname As String
            Dim fpath As String
            If Not String.IsNullOrWhiteSpace(aFileSpec) Then
                fname = IO.Path.GetFileNameWithoutExtension(aFileSpec)
                fpath = IO.Path.GetDirectoryName(aFileSpec)
            Else
                aFileSpec = FileName(aFileType)
                fname = IO.Path.GetFileNameWithoutExtension(aFileSpec)
                fpath = IO.Path.GetDirectoryName(aFileSpec)
            End If
            rErrString = ""
            If String.IsNullOrWhiteSpace(fpath) Then
                rErrString = "Output path is not defined"
                Return fname
            End If
            If Not IO.Directory.Exists(fpath) Then
                rErrString = $"Output folder '{fpath}' does not exist "
                Return fname
            End If
            If aFileType <> dxxFileTypes.DWG And aFileType <> dxxFileTypes.DXF And aFileType <> dxxFileTypes.DXT Then aFileType = dxxFileTypes.DXF
            If aFileType <> dxxFileTypes.DXF Then bNoConverter = False
            'If aFileType = dxxFileTypes.DWG Then
            '    If Not goACAD.ConverterPresent Then Throw New Exception("File Converter Not Found")
            'End If
            'If Not goACAD.ConverterPresent Then bNoConverter = True
            aExts = dxfEnums.PropertyName(aFileType).ToLower
            outspec = IO.Path.Combine(fpath, $"{fname}.{aExts}")

            Try
                RaiseEvent WriteToDisc(True, aFileType, outspec)
            Catch ex As Exception
            End Try



            If aGenerator Is Nothing Then aGenerator = New dxoFileTool
            Dim eStr As String = String.Empty
            Dim eNo As Long
            fspec = aGenerator.Image_WRITE(aFileType, Me, outspec, bSuppressDimOverrides, aVersion, bNoErrors:=True, rErrString:=eStr, bSuppressDXTNames:=bSuppressDXTNames, bNumericHandles:=bNumericHandles, rErrNo:=eNo, bNoConverter:=bNoConverter)
            'eStr = dxoFileTool.ErrorString
            'bail if an error is returned
            If eStr <> "" Then Throw New Exception(eStr)
            FileName_Set(fspec)
            _rVal = FileName()

            Try
                RaiseEvent WriteToDisc(False, dxxFileTypes.DWG, fspec)
            Catch ex As Exception
                rErrString = ex.Message
            Finally
                If Not bNoErrors And rErrString <> "" Then HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", rErrString)
            End Try
            Return _rVal
        End Function
        Friend Sub AddToRegenList(aGraphicType As dxxGraphicTypes)
            If aGraphicType = dxxGraphicTypes.Undefined Then
                _Structure.RegenList = ""
            Else
                Dim wuz As String = _Structure.RegenList
                If TLISTS.Add(_Structure.RegenList, dxfEnums.Description(aGraphicType), bAllowNulls:=False) Then
                    RespondToPropertyChange("", New TPROPERTY(2, _Structure.RegenList, "RegenList", dxxPropertyTypes.dxf_String, wuz))
                End If
            End If
        End Sub



        Friend Sub BaseSettings_Set(value As TTABLEENTRY)
            _Structure.BaseSettings_Set(value)
            Select Case value.EntryType
                Case dxxReferenceTypes.HEADER
                    Header.Properties.CopyVals(value, bSkipHandles:=False, bSkipPointers:=False)
            End Select

        End Sub

        Friend Function BaseSettings(aSettingsType As dxxSettingTypes) As TTABLEENTRY

            Return _Structure.BaseSettings(aSettingsType)

        End Function

#End Region 'Protected Methods
#Region "Event Methods"
        Friend Sub RaiseZoomEvent(aExtents As Boolean, aZoomFactor As Double)
            Try
                RaiseEvent ZoomEvent(aExtents, aZoomFactor)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseViewChangeEvent()
            Try
                RaiseEvent ViewChangeEvent(New dxfViewChangeEventArg(GUID, Display.ViewRectangle))
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseErrorRecieved(aFunction As String, aErrSource As String, aErrDescription As String, ByRef ioIgnore As Boolean)
            Try
                RaiseEvent ErrorRecieved(aFunction, aErrSource, aErrDescription, ioIgnore) ', aThrowList, aIgnoreList, rIgnore, rDontLog)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseEntitiesEvent(aAdded As Boolean, aEntity As dxfEntity)
            Try
                RaiseEvent EntitiesEvent(aAdded, aEntity)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseBlocksEvent(aAdded As Boolean, aBlock As dxfBlock)
            Try
                RaiseEvent BlocksEvent(aAdded, aBlock)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseEntitiesUpdateEvent(aRemove As Boolean, aEntity As dxfEntity)
            Try
                RaiseEvent EntitiesUpdateEvent(aRemove, aEntity)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseOverlayEvent(aAdded As Boolean, aEntity As dxfEntity)
            Try
                RaiseEvent OverlayEvent(aAdded, aEntity)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseOverlayBMPEvent(aAdded As Boolean, bmp As System.Drawing.Bitmap)
            Try
                RaiseEvent OverlayBmpEvent(aAdded, bmp)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseObjectEvent(aObjectTypeName As String, aObjectName As String, aPropertyName As String, aNewValue As Object, aLastValue As Object)
            Try
                RaiseEvent ObjectEvent(aObjectTypeName, aObjectName, aPropertyName, aNewValue, aLastValue)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseObjectsEvent(aObjectTypeName As String, aEventType As dxxCollectionEventTypes, aEventDescription As String)
            Try
                RaiseEvent ObjectsEvent(aObjectTypeName, aEventType, aEventDescription)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseRenderEvent(aBegin As Boolean, bZoomExtents As Boolean, bSetFeatureScales As Boolean, bRegeneratePaths As Boolean, Optional bSuppressEvnts As Boolean = False, Optional aExtentBufferPercentage As Double = 0)
            '^causes the image to raise the render event with thte passed arguments
            _Structure.Rendering = aBegin
            If Not bSuppressEvnts Then
                Try
                    RaiseEvent RenderEvent(New dxfImageRenderEventArg(GUID, aBegin, bZoomExtents, bSetFeatureScales, bRegeneratePaths, aExtentBufferPercentage))
                    If bZoomExtents And Not aBegin Then
                        RaiseZoomEvent(True, obj_DISPLAY.ZoomFactor)
                    End If
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine(ex.Message)
                End Try
                'If aBegin Then
                '    RaiseScreenEvent(dxxScreenEventTypes.Clear)
                'Else
                '    RaiseScreenEvent(dxxScreenEventTypes.Refresh)
                'End If
            End If
        End Sub
        Friend Sub RaiseScreenEvent(aEventType As dxxScreenEventTypes, Optional aScreenEntity As dxoScreenEntity = Nothing, Optional aImageEntity As dxfEntity = Nothing)
            Dim aBMP As dxfBitmap = GetBitmap(True)
            If aBMP Is Nothing Then Return
            Dim e As New dxfImageScreenEventArg(GUID, aEventType, aScreenEntity, aImageEntity) With {.Bitmap = aBMP}
            Try
                If aEventType = dxxScreenEventTypes.Clear Or aEventType = dxxScreenEventTypes.Refresh Then
                    RaiseEvent ScreenRender(aBMP)
                End If
                RaiseEvent ScreenDrawingEvent(aEventType, e)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaisePropertyChange(aSource As String, aProperty As TPROPERTY)
            Try
                If String.IsNullOrEmpty(aSource) Then aSource = "Image"
                Dim aOldValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=True)
                Dim aNewValue As String = aProperty.StringValue(bSwitchesAsBooleans:=True, bDecoded:=True, bReturnLastValue:=False)
                RaiseEvent PropertyChange(aSource, New dxoProperty(aProperty))
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseSettingChange(aSource As idxfSettingsObject, aProperty As dxoProperty)
            Try
                If aSource Is Nothing Then Return
                Dim sVal As String = dxfEnums.DisplayName(aSource.SettingType)
                Dim name As String = aSource.Name
                If name <> "" And name.ToUpper <> sVal.ToUpper Then sVal += " [" & name & "]"
                RaiseEvent SettingChange(sVal, New dxoProperty(aProperty))
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseSettingChange(aSource As dxfSettingObject, aProperty As dxoProperty)
            Try
                If aSource Is Nothing Then Return
                Dim sVal As String = dxfEnums.DisplayName(aSource.SettingType)
                Dim name As String = aSource.Name
                If name <> "" And name.ToUpper <> sVal.ToUpper Then sVal += " [" & name & "]"
                RaiseEvent SettingChange(sVal, New dxoProperty(aProperty))
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub

        Friend Sub RaiseTableEvent(aTableName As String, aEventType As dxxCollectionEventTypes, aEventDescription As String)
            Try
                RaiseEvent TableEvent(aTableName, aEventType, aEventDescription)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub

        Friend Sub RaiseTableMemberEvent(aMember As dxfTableEntry, aProperty As dxoProperty)
            Try
                If aMember Is Nothing Or aProperty Is Nothing Then Return
                RaiseEvent TableMemberEvent(aMember.EntryTypeName.ToUpper & "S", aMember.Name, New dxoProperty(aProperty))
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseViewRegenerateEvent()
            Try
                RaiseEvent ViewRegenerate(Me)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RaiseViweRotateEvent(rotation As Double)
            Try
                RaiseEvent ViewRotateEvent(rotation)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
#End Region 'Event Methods
#Region "Reactions"
        Friend Function ConfirmNewEntity(aEntity As dxfEntity) As Boolean
            Dim rErrorString As String = String.Empty
            Return ConfirmNewEntity(aEntity, rErrorString)
        End Function
        Friend Function ConfirmNewEntity(aEntity As dxfEntity, ByRef rErrorString As String) As Boolean
            rErrorString = ""
            If aEntity Is Nothing Then Return False
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            Try
                'give the block a reference to the Insert and give the insert the block info
                If gType = dxxGraphicTypes.Insert Then
                    Dim aInsert As dxeInsert = DirectCast(aEntity, dxeInsert)
                    Dim aBlock As dxfBlock = Blocks.GetByName(aInsert.BlockName, True)
                    If aBlock IsNot Nothing Then
                        aInsert.Block = aBlock
                        Return True
                    Else
                        Throw New Exception($"Non Existant Block Reference [{ aInsert.BlockName }]")
                    End If
                End If
                Dim styName As String = aEntity.TextStyleName
                If styName <> "" Then aEntity.TextStyleName = GetOrAdd(dxxReferenceTypes.STYLE, styName)
                If (aEntity.HasDimStyle) Then

                    styName = aEntity.DimStyleName
                    If styName <> "" Then aEntity.DimStyleName = GetOrAdd(dxxReferenceTypes.STYLE, styName)
                End If

                aEntity.Linetype = GetOrAdd(dxxReferenceTypes.LTYPE, aEntity.Linetype)
                aEntity.LayerName = GetOrAdd(dxxReferenceTypes.LAYER, aEntity.LayerName)
                Return True
            Catch ex As Exception
                rErrorString = ex.Message
                Return False
            End Try
        End Function
        Friend Sub ProcessNewEntity(aEntity As dxfEntity)
            'On Error Resume Next
            If aEntity Is Nothing Then Return
            Dim gname As String
            Dim aTxt As dxeText
            Dim aDimen As dxeDimension
            Dim aLeader As dxeLeader
            Dim aTbl As dxeTable
            Dim aEnts As colDXFEntities
            Dim i As Integer
            gname = aEntity.GroupName
            If Not String.IsNullOrWhiteSpace(gname) Then
                dxfUtils.ValidateGroupName(gname.Trim(), True)
                aEntity.GroupName = gname
            End If
            If Not aEntity.IsDirty Then
                'Dim aPaths As TPATHSv
                'aPaths = aEntity.Components.Paths
                'aEntity.Components.Paths = aPaths
                'aEntity.Definition = aEntity
            End If
            If Not String.IsNullOrWhiteSpace(gname) Then
                Objex.AddGroupEntry(gname, aEntity)
            End If
            'save the blokc references
            Select Case aEntity.GraphicType
         '--------------------------------------------------------------------------------
                Case dxxGraphicTypes.Insert
                    '--------------------------------------------------------------------------------
                    Dim aInsert As dxeInsert = aEntity
                    Dim aBlock As dxfBlock = Blocks.Item(aInsert.BlockName)
                    If aBlock IsNot Nothing Then aBlock.ReferenceAdd(aInsert.Handle)
         '--------------------------------------------------------------------------------
                Case dxxGraphicTypes.Table
                    '--------------------------------------------------------------------------------
                    aTbl = aEntity
                    aTbl.UpdatePath(False, Me)
                    aEnts = aTbl.Entities
                    For i = 1 To aEnts.Count
                        ConfirmNewEntity(aEnts.Item(i))
                    Next i
         '--------------------------------------------------------------------------------
                Case dxxGraphicTypes.Text
                    '--------------------------------------------------------------------------------
                    aTxt = aEntity
                    aTxt.TextStyleName = GetOrAdd(dxxReferenceTypes.STYLE, aTxt.TextStyleName)
                    Styles.ReferenceADD(aTxt.TextStyleName, aTxt.Handle)

         '--------------------------------------------------------------------------------
                Case dxxGraphicTypes.Dimension
                    '--------------------------------------------------------------------------------
                    aDimen = aEntity
                    aDimen.SuppressEvents = True
                    aDimen.DimStyleName = GetOrAdd(dxxReferenceTypes.DIMSTYLE, aDimen.DimStyleName)
                    aDimen.DimStyle.ImageGUID = GUID
                    aDimen.SuppressEvents = False
                    aDimen.UpdatePath(False, Me)
                    aEnts = aDimen.Entities
                    For i = 1 To aEnts.Count
                        ConfirmNewEntity(aEnts.Item(i))
                    Next i
         '--------------------------------------------------------------------------------
                Case dxxGraphicTypes.Leader
                    '--------------------------------------------------------------------------------
                    aLeader = aEntity
                    aLeader.SuppressEvents = True
                    aLeader.DimStyleName = GetOrAdd(dxxReferenceTypes.DIMSTYLE, aLeader.DimStyleName)
                    aLeader.DimStyle.ImageGUID = GUID
                    aLeader.SuppressEvents = False
                Case dxxGraphicTypes.Polygon
                    'Dim aPg As dxePolygon = aEntity
            End Select
        End Sub
        Friend Sub RespondToCollectionEvent(aCollection As Object, aEventType As dxxCollectionEventTypes, aMemberOrSubSet As Object, ByRef rCancel As Boolean, Optional bOverrideExisting As Boolean = False, Optional bNoRedraw As Boolean = False)
            rCancel = False
            If aCollection Is Nothing Then Return
            Dim bRedraw As Boolean
            Dim bAdded As Boolean
            Dim rBlock As dxfBlock = Nothing
            Try
                Select Case aCollection.GetType
                    Case GetType(colDXFEntities)
#Region "ENTITIES"
                        Dim aEnt As dxfEntity = Nothing
                        Dim aEnts As colDXFEntities = Nothing
                        Dim aErr As String = String.Empty
                        Dim tsName As String
                        Dim dsName As String
                        Dim remvdGUIDS As New TVALUES(0)
                        Dim bColPassed As Boolean = TypeOf (aMemberOrSubSet) Is colDXFEntities
                        If bColPassed Then aEnts = aMemberOrSubSet
                        If TypeOf (aMemberOrSubSet) Is dxfEntity Then aEnt = aMemberOrSubSet
                        Select Case aEventType
                                '--------------------------------------------------------------------------------
                            Case dxxCollectionEventTypes.PreAdd
                                '--------------------------------------------------------------------------------
                                If aEnt Is Nothing Then
                                    rCancel = True
                                    Return
                                End If
                                'add a clone cause we alreay have this member
                                If (aEnt.ImageGUID = GUID And aEnt.Handle <> "") Or aEnt.BlockGUID <> "" Then
                                    aEnt = aEnt.Clone
                                    aMemberOrSubSet = aEnt
                                End If
                                rCancel = Not ConfirmNewEntity(aEnt, aErr)
                                If aErr <> "" Then HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", aErr)
                                If Not rCancel Then
                                    aEnt.ImageGUID = ""
                                    HandleGenerator.AssignTo(aEnt)
                                End If
                                '--------------------------------------------------------------------------------
                            Case dxxCollectionEventTypes.Add, dxxCollectionEventTypes.Append
                                '--------------------------------------------------------------------------------
                                If aEnt IsNot Nothing Then
                                    aEnts = New colDXFEntities(aEnt)
                                Else
                                    Return
                                End If
                                Dim blockEnts As Boolean = aEnts.IsBlock
                                For i As Integer = 1 To aEnts.Count
                                    aEnt = aEnts.Item(i)
                                    If aEnt Is Nothing Then Continue For
                                    tsName = aEnt.TextStyleName
                                    If aEnt.HasDimStyle Then dsName = aEnt.DimStyleName

                                    If Not blockEnts Then aEnt.SaveToFile = Not SuppressFromFile
                                    aEnt.ImageGUID = GUID
                                    If String.IsNullOrWhiteSpace(aEnt.GroupName) Then
                                        aEnt.GroupName = GroupName  'assign a group name if we have a live group name
                                    End If
                                    ProcessNewEntity(aEnt)
                                    tsName = aEnt.TextStyleName
                                    If Not blockEnts Then
                                        Render_Entity(aEnt, bSuppressRefresh:=bColPassed)
                                        bAdded = True
                                        RaiseEntitiesEvent(True, aEnt)
                                    End If

                                Next
                                If bColPassed Then bRedraw = AutoRedraw
                                '-------------------------------------------------------------------------------------------------
                            Case dxxCollectionEventTypes.PreRemove
                                '-------------------------------------------------------------------------------------------------
                                rCancel = False
                                Return
                                '--------------------------------------------------------------------------------
                            Case dxxCollectionEventTypes.Remove, dxxCollectionEventTypes.RemoveSet, dxxCollectionEventTypes.Clear
                                '--------------------------------------------------------------------------------
                                If aEnt IsNot Nothing Then
                                    aEnts = New colDXFEntities(aEnt)

                                Else
                                    Return
                                End If

                                Dim blockEnts As Boolean = aEnts.IsBlock
                                For i As Integer = 1 To aEnts.Count
                                    aEnt = aEnts.Item(i)
                                    If aEnt IsNot Nothing Then
                                        tsName = aEnt.TextStyleName
                                        dsName = aEnt.DimStyleName
                                        aEnt.ImageGUID = ""
                                        If Not blockEnts Then
                                            If AutoRedraw And Not bColPassed Then
                                                dxfImageTool.Entity_Erase(Me, aEnt)
                                                bNoRedraw = True
                                            Else
                                                IsDirty = True
                                            End If
                                        Else
                                            IsDirty = True
                                        End If
                                        If tsName <> "" Then
                                            TextStyles.ReferenceRemove(tsName, aEnt.Handle)
                                        End If
                                        If dsName <> "" Then
                                            DimStyles.ReferenceRemove(dsName, aEnt.Handle)
                                        End If
                                        remvdGUIDS.Add(aEnt.GUID)
                                        Select Case aEnt.GraphicType
                                            Case dxxGraphicTypes.Insert
                                                Dim aInsert As dxeInsert = aEnt
                                                Dim aBlock As dxfBlock = Blocks.GetByName(aInsert.BlockName)
                                                If aBlock IsNot Nothing Then aBlock.ReferenceRemove(aEnt.Handle)
                                        End Select
                                        If Not blockEnts Then
                                            bAdded = False
                                            RaiseEntitiesEvent(False, aEnt)
                                        End If
                                        _HandleGenerator.Release(aEnt)
                                    End If
                                Next
                                IsDirty = True
                                If bColPassed Then bRedraw = AutoRedraw
                        End Select
                        If remvdGUIDS.Count > 0 Then
                            obj_SCREEN.RemoveEntsByGUID(Me, remvdGUIDS)
                        End If
                        If aEventType < 0 Then
                            IsDirty = True
                            '        bRedraw = True
                        End If
#End Region 'ENTITIES
                    Case GetType(colDXFBlocks)
#Region "BLOCKS"
                        Dim aBlock As dxfBlock = aMemberOrSubSet
                        If aBlock Is Nothing Then Return
                        Dim aErr As String = String.Empty
                        Select Case aEventType
                                '-------------------------------------------------------
                            Case dxxCollectionEventTypes.PreAdd
                                '-------------------------------------------------------
                                aBlock.ImageGUID = ""
                                Dim aName As String = aBlock.Name
                                dxfUtils.ValidateBlockName(aName, aErr, bFixIt:=False)
                                If aErr <> "" Then
                                    rCancel = True
                                    aErr = $"An Attempt Was Made To Add a Block With An Invalid Name. { aErr}"
                                    Throw New Exception(aErr)
                                End If
                                If Not rCancel Then
                                    If Blocks.BlockExists(aName) Then
                                        'Throw New Exception("An Attempt Was Made To Add a Block With a Name That Already Existists In the Collection [" & aName & "]" & aErr)
                                        '                        aImage.HandleError "RespondToCollectionEvent_BLOCK", "dxfImage", aErr
                                    End If
                                End If
                                If Not rCancel Then
                                    HandleGenerator.AssignTo(aBlock)
                                    aBlock.Name = aName
                                End If
                                '-------------------------------------------------------
                            Case dxxCollectionEventTypes.PreRemove
                                '-------------------------------------------------------
                                If TLISTS.Contains(aBlock.Name, "*Model_Space,*Paper_Space,_ClosedFilled") Then
                                    rCancel = True
                                End If
                                If Not rCancel Then rCancel = DimStyleOverrides.ReferencesBlock(aBlock.Name, aBlock.BlockRecordHandle)
                                If Not rCancel Then rCancel = DimStyles.ReferencesBlock(aBlock.Name, True, aBlock.BlockRecordHandle)
                                If Not rCancel Then rCancel = Entities.ReferencesBlock(aBlock.Name, True)
                                If Not rCancel Then rCancel = Blocks.ReferencesBlock(aBlock.Name, True)
                                If Not rCancel Then
                                    HandleGenerator.ReleaseHandle(aBlock.Handle)
                                    HandleGenerator.ReleaseHandle(aBlock.BlockRecordHandle)
                                    HandleGenerator.ReleaseHandle(aBlock.EndBlockHandle)
                                End If
                                '-------------------------------------------------------
                            Case dxxCollectionEventTypes.Add
                                '-------------------------------------------------------
                                aBlock.SuppressEvents = True
                                HandleGenerator.AssignTo(aBlock)
                                Dim aBr As dxoBlockRecord = Nothing
                                Dim brTBL As dxfTable = BlockRecords
                                If Not brTBL.TryGetEntry(aBlock.Name, aBr) Then
                                    aBr = New dxoBlockRecord(aBlock.BlockRecord)
                                    brTBL.Add(aBr)
                                    RespondToTableEvent(brTBL, dxxCollectionEventTypes.Add, aBr, False, False, aErr)
                                End If

                                Dim aEnts As colDXFEntities = aBlock.Entities
                                For Each aEnt As dxfEntity In aEnts
                                    ConfirmNewEntity(aEnt)
                                    ProcessNewEntity(aEnt)
                                    'aEnt.SetPropVal("*BlockName", aBlock.Name)
                                    aEnt.BlockGUID = aBlock.GUID
                                Next
                                aBlock.SuppressEvents = False
                                rBlock = aBlock
                                bAdded = True
                                '-------------------------------------------------------
                            Case dxxCollectionEventTypes.Remove
                                '-------------------------------------------------------
                                aBlock.ImageGUID = ""
                                HandleGenerator.ReleaseHandle(aBlock.Handle)
                                HandleGenerator.ReleaseHandle(aBlock.BlockRecordHandle)
                                HandleGenerator.ReleaseHandle(aBlock.EndBlockHandle)
                                If BlockRecords.Remove(aBlock.Name).Count <= 0 Then BlockRecords.Remove(aBlock.BlockRecordHandle)
                                rBlock = aBlock
                                bAdded = False
                        End Select
#End Region 'BLOCKS
                End Select
            Catch ex As Exception
                rCancel = True
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
            Finally
                If rBlock IsNot Nothing Then
                    RaiseBlocksEvent(bAdded, rBlock)
                End If
                If bRedraw And Not bNoRedraw Then
                    Render()
                End If
            End Try
        End Sub
        Friend Sub RespondToBlockChange(aMember As dxfBlock, aName As String, aOldValue As Object, aNewValue As Object, ByRef rDontChange As Boolean, ByRef rError As String)
            rDontChange = False
            rError = String.Empty
            If aMember Is Nothing Then Return
            Dim aBlk As dxfBlock = Blocks.GetByGUID(aMember.GUID)
            If aBlk Is Nothing Then Return
            'On Error Resume Next
            Dim wuz As Boolean
            Dim aMem As dxfEntity
            Dim aIns As dxeInsert
            Dim aEnts As colDXFEntities
            Dim tBlk As TBLOCK = aBlk.Strukture
            wuz = AutoRedraw
            AutoRedraw = False
            aEnts = Entities
            For i As Integer = 1 To aEnts.Count
                aMem = aEnts.Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Insert Then
                    aIns = aMem
                    If String.Compare(aIns.BlockName, tBlk.Name, True) = 0 Or
                 String.Compare(aIns.BlockGUID, aBlk.GUID, True) = 0 Then
                        aIns.Block = aBlk
                        aIns.IsDirty = True
                    End If
                End If
            Next i
            AutoRedraw = wuz
            If rError <> "" Then
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", rError)
            End If
        End Sub
        Friend Sub RespondToObjectEvent(aMember As dxfObject, aGroupCode As Integer, aOccurance As Integer, aOldValue As Object, aNewValue As Object, ByRef rDontChange As Boolean, ByRef rProperty As TPROPERTY, ByRef rMemberIndex As Integer, ByRef rError As String)
            rDontChange = False
            rMemberIndex = 0
            rError = String.Empty
            If aMember Is Nothing Then Return
            Dim bRedraw As Boolean = False
            Dim hndl As String = aMember.Handle
            Dim aObjs As colDXFObjects = Objex
            Dim mName As String = aMember.Name
            Dim aObj As dxfObject = aObjs.GetObject(aMember.GUID, aMember.ObjectType)
            If aObj Is Nothing Then
                rDontChange = True
                Return
            End If
            rDontChange = False
            Try
            Catch ex As Exception
            Finally
                If bRedraw Then
                    If AutoRedraw Then Render() Else IsDirty = True
                End If
                If rError <> "" Then
                    If rProperty.Name <> "" Then
                        RaiseObjectEvent(aMember.ObjectName, aMember.Name, rProperty.Name, rProperty.Value, rProperty.LastValue)
                    End If
                    HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", rError)
                End If
            End Try
        End Sub
        Friend Sub RespondToObjectsEvent(aObjects As colDXFObjects, aEventType As dxxCollectionEventTypes, aObject As dxfObject, ByRef rCancel As Boolean, Optional aMakeCurrent As Boolean = False)
            rCancel = True
            If aObjects Is Nothing Or aObject Is Nothing Then Return
            Dim aNm As String = aObject.Name
            rCancel = False
            If aObject.Props.Count <= 0 Then
                rCancel = True
                Return
            End If
            Dim evntMsg As String = String.Empty
            Dim objTyp As dxxObjectTypes = aObject.ObjectType
            Dim objType As String = dxfEnums.Description(objTyp)
            Try
                Select Case aEventType
                      '-----------------------------------------------------------------------------
                    Case dxxCollectionEventTypes.PreAdd
                        '-----------------------------------------------------------------------------
                        If aNm = "" Then
                            rCancel = True
                            Return
                        End If
                        If aObjects.GetObject(aNm, aObject.ObjectType) IsNot Nothing Then
                            rCancel = True
                            Return
                        End If

                        Dim existing As dxfObject = Nothing
                        If Not aObjects.TryGet(aObject.Name, aObject.ObjectType, existing) Then
                            If Not aObjects.TryGet(aObject.GUID, aObject.ObjectType, existing) Then
                                aObjects.TryGet(aObject.Handle, aObject.ObjectType, existing)
                            End If
                        End If
                        rCancel = existing IsNot Nothing
                        If Not rCancel Then HandleGenerator.AssignTo(aObject)
                    Case dxxCollectionEventTypes.Add
                        evntMsg = $"'{aObject}' Added"
                        Select Case aObject.ObjectType
                         '=====================================================
                            Case dxxObjectTypes.Layout
                                '=====================================================
                        End Select

                    Case dxxCollectionEventTypes.Remove
                        HandleGenerator.ReleaseHandle(aObject.Handle)
                        evntMsg = $"'{ aObject}' Removed"
                End Select
                '                Select Case aObject.ObjectType
                '         '=====================================================
                '                    Case dxxObjectTypes.Layout
                '                        '=====================================================
                '#Region "LAYOUT"
                '                        Select Case aEventType
                '             '-----------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.PreAdd
                '                                '-----------------------------------------------------------------------------
                '                                aObjs.GetByName(aNm, dxxObjectTypes.Layout, idx)
                '                                If idx > 0 Then
                '                                    rCancel = True
                '                                Else
                '                                    HandleGenerator.AssignTo(aObject)
                '                                    aDict = aObjs.GetByName("ACAD_LAYOUT", dxxObjectTypes.Dictionary, didx)
                '                                    If didx <= 0 Then
                '                                        _Structure.VerifyNamedDictionary(HandleGenerator, aDictionaryName:="ACAD_LAYOUT", bWDFlt:=False, aNamedDicIndex:=0)
                '                                        aObjs = _Structure.obj_OBJECTS
                '                                    End If
                '                                End If
                '             '-----------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.PreRemove
                '                                '-----------------------------------------------------------------------------
                '                                If String.Compare(aNm, "Model", True) = 0 Or String.Compare(aNm, "Layout1", True) = 0 Then
                '                                    rCancel = True
                '                                    Throw New Exception("An Attempt Was Made To Remove Layout '" & aNm & "'. Layout '" & aNm & "' Cannot Be Removed")
                '                                End If
                '             '-----------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.Remove
                '                                '-----------------------------------------------------------------------------
                '                                evntMsg = "Layout '" & aNm & "' Removed"
                '             '-------------------------------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.Add
                '                                '-------------------------------------------------------------------------------------------------
                '                                evntMsg = "Layout '" & aNm & "' Added"
                '                        End Select
                '#End Region 'LAYOUT
                '        '=====================================================
                '                    Case dxxObjectTypes.PlotSetting
                '#Region "PLOTSETTINGS"
                '                        Select Case aEventType
                '             '-----------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.PreAdd
                '                                '-----------------------------------------------------------------------------
                '                                If aNm = "" Or aObject.Props.Count <= 0 Then
                '                                    rCancel = True
                '                                    Return
                '                                End If
                '                                aObject.name= aNm
                '                                aObjs.GetByName(aNm, dxxObjectTypes.PlotSetting, idx)
                '                                If idx > 0 Then
                '                                    rCancel = True
                '                                Else
                '                                    HandleGenerator.AssignTo(aObject)
                '                                    aDict = aObjs.GetByName("ACAD_PLOTSETTINGS", dxxObjectTypes.Dictionary, didx)
                '                                    If didx <= 0 Then
                '                                        _Structure.VerifyNamedDictionary(HandleGenerator, aDictionaryName:="ACAD_PLOTSETTINGS", bWDFlt:=False, aNamedDicIndex:=0)
                '                                        aObjs = _Structure.obj_OBJECTS
                '                                    End If
                '                                End If
                '             '-----------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.PreRemove
                '             '-----------------------------------------------------------------------------
                '             '-----------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.Remove
                '                                '-----------------------------------------------------------------------------
                '                                aObject.ImageGUID = ""
                '                                aObjs.ReplaceMembers(aObjects.Strukture, dxxObjectTypes.PlotSetting)
                '             '-------------------------------------------------------------------------------------------------
                '                            Case dxxCollectionEventTypes.Add
                '                                '-------------------------------------------------------------------------------------------------
                '                                RaiseObjectsEvent("PLOTSETTINGS", dxxCollectionEventTypes.Add, "Plotsettings '" & aNm & "' Added")
                '                                aObjs = aObjs.ReplaceMembers(aObjects.Strukture, dxxObjectTypes.PlotSetting)
                '                        End Select
                '#End Region 'PLOTSETINGS
                '                End Select
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
            Finally
                If evntMsg <> "" Then
                    'obj_OBJECTS = aObjs
                    RaiseObjectsEvent(objType, aEventType, evntMsg)
                End If
            End Try
        End Sub
        Friend Sub RespondToPropertyChange(aSource As String, aProperty As TPROPERTY)
            Try
                RaisePropertyChange(aSource, aProperty)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
            End Try
        End Sub
        Friend Sub RespondToSettingChange(aSettingObj As idxfSettingsObject, aProp As dxoProperty, Optional bSuppressEvents As Boolean = False)
            If aSettingObj Is Nothing Then Return
            Dim bUCS As Boolean
            Dim bRedraw As Boolean
            Dim refType As dxxReferenceTypes = aSettingObj.SettingType

            Select Case refType

         '
         '============================================================
                Case dxxSettingTypes.DIMSETTINGS
                    '============================================================
                    Dim aDimSettings As dxoSettingsDim = aSettingObj
                    If aDimSettings.IsCopied Then Return
                    BaseSettings_Set(aDimSettings.Strukture)
         '============================================================
                Case dxxSettingTypes.TEXTSETTINGS
                    '============================================================
                    Dim aTextSettings As dxoSettingsText = aSettingObj
                    If aTextSettings.IsCopied Then Return
                    BaseSettings_Set(aTextSettings.Strukture)
         '============================================================
                Case dxxSettingTypes.TABLESETTINGS
                    '============================================================
                    Dim aTableSettings As dxoSettingsTable = aSettingObj
                    If aTableSettings.IsCopied Then Return
                    BaseSettings_Set(aTableSettings.Strukture)
         '============================================================
                Case dxxSettingTypes.SYMBOLSETTINGS
                    '============================================================
                    Dim aSymbolSettings As dxoSettingsSymbol = aSettingObj
                    If aSymbolSettings.IsCopied Then Return
                    BaseSettings_Set(aSymbolSettings.Strukture)
                    Dim setlayer As dxoLayer = Nothing
                    Select Case aProp.Name.ToUpper
                        Case "LAYERNAME"
                            Dim lname As String = aProp.ValueS.Trim()
                            If String.IsNullOrWhiteSpace(lname) Then Return
                            If Layers.TryGet(aProp.LastValue.ToString(), setlayer) Then
                                setlayer.Name = lname
                                setlayer.Color = aSymbolSettings.LayerColor
                            End If
                            If Layers.TryGet(lname, setlayer) Then
                                setlayer.Color = aSymbolSettings.LayerColor
                                Return
                            Else
                                setlayer = Layers.Add(lname, aSymbolSettings.LayerColor)
                            End If

                        Case "LAYERCOLOR"
                            Dim lname As String = aSymbolSettings.LayerName
                            If Layers.TryGet(lname, setlayer) Then
                                setlayer.Color = aProp.Value
                            Else
                                setlayer = Layers.Add(lname, aSymbolSettings.LayerColor)
                            End If
                    End Select
                       '============================================================
                Case dxxReferenceTypes.STYLE
                    '============================================================
                    'screen text style
                    Dim aStyle As dxoStyle = aSettingObj
                    If Not aStyle.IsGlobal Then Return
                    _Structure.obj_SCREEN.TextStyle = New TTABLEENTRY(aStyle)


         '============================================================
                Case dxxSettingTypes.SCREENSETTINGS
                    '============================================================
                    bRedraw = False
                    Dim aScreen As dxoScreen = aSettingObj
                    If TLISTS.Contains(aProp.Name.ToUpper, "DRAWOCS,DRAWBOUNDINGRECTANGLES,DRAWEXTENTPTS,SUPPRESSED,TEXTBOXES") Then
                        bRedraw = Entities.Count > 0
                    ElseIf aProp.Name.ToUpper = "DRAWEXTENTRECTANGLE" Then
                        bRedraw = True
                    End If
                    obj_SCREEN = aScreen.Strukture
                     '============================================================
                Case dxxReferenceTypes.UCS
                    '============================================================
                    'Dim aUCS As dxoUCS = aSettingObj
                    If Header.UCSMode <> dxxUCSIconModes.None Then bRedraw = True
                Case Else
                    'Return
            End Select
            If Not bSuppressEvents Then RaiseSettingChange(aSettingObj, aProp)
            If bRedraw Then
                If AutoRedraw Then
                    Render()
                Else
                    IsDirty = True
                End If
            Else
                If bUCS Then
                    Render_UCSICon(False, True)
                End If
            End If
        End Sub

        Friend Sub RespondToSettingChange(aSettingObj As dxfSettingObject, aProp As dxoProperty, Optional bSuppressEvents As Boolean = False)
            If aSettingObj Is Nothing Or _Settings Is Nothing Then Return
            Dim idx As Integer = 0
            Dim mySettings As dxfSettingObject = _Settings.GetMatchingEntry(aSettingObj, idx)

            If idx <= 0 Then Return

            Dim bUCS As Boolean
            Dim bRedraw As Boolean
            Dim refType As dxxSettingTypes = aSettingObj.SettingType
            Dim aName As String = aProp.Name.ToUpper()

            Select Case True
                Case TypeOf mySettings Is dxsDimOverrides
                    If aName.StartsWith("DIM") Then
                        Dim hprop As dxoProperty = Header.Properties.Find(Function(x) String.Compare(x.Key, aProp.Key, True) = 0)
                        If hprop IsNot Nothing Then
                            Header.Properties.SetVal(aName, aProp.Value, bSuppressEvents:=True)
                        End If

                    End If

                Case TypeOf mySettings Is dxsHeader
#Region "HEADER"

                    bSuppressEvents = True
                    Select Case aName
             '========================================================================================
                        Case "$LTSCALE"
                            '========================================================================================
                            bRedraw = Entities.Count > 0
             '========================================================================================
                        Case "*UCSMODE"
                            '========================================================================================
                            bUCS = True
             '========================================================================================
                        Case "*UCSCOLOR", "*UCSSIZE"
                            '========================================================================================
                            bUCS = Header.UCSMode <> dxxUCSIconModes.Origin
             '========================================================================================
                        Case "$PDMODE"
                            '========================================================================================
                            AddToRegenList(dxxGraphicTypes.Point)
                            bRedraw = Entities.Count > 0
             '========================================================================================
                        Case "$PDSIZE"
                            '========================================================================================
                            AddToRegenList(dxxGraphicTypes.Point)
                            bRedraw = Entities.Count > 0
             '========================================================================================
                        Case "$QTEXTMODE"
                            '========================================================================================
                            AddToRegenList(dxxGraphicTypes.Text)
                            bRedraw = Entities.Count > 0
             '========================================================================================
                        Case "$STYLESHEET"
                            '========================================================================================
                            Dim Objs As List(Of dxfObject) = Layouts
                            For Each aObj As dxfoLayout In Objs
                                aObj.SetProperty(7, aProp.Value, aOccurance:=1)
                            Next
                    End Select

#End Region 'Header




            End Select

            '   Select Case refType
            ''============================================================
            '       Case dxxSettingTypes.LINETYPESETTINGS
            '           '============================================================
            '           Dim aLinetypeSettings As dxsLinetypes = aSettingObj
            '           If aLinetypeSettings.IsCopied Then Return
            '           BaseSettings_Set(aLinetypeSettings.Strukture)
            ''============================================================
            '       Case dxxSettingTypes.DIMSETTINGS
            '           '============================================================
            '           Dim aDimSettings As dxoSettingsDim = aSettingObj
            '           If aDimSettings.IsCopied Then Return
            '           BaseSettings_Set(aDimSettings.Strukture)
            ''============================================================
            '       Case dxxSettingTypes.TEXTSETTINGS
            '           '============================================================
            '           Dim aTextSettings As dxoSettingsText = aSettingObj
            '           If aTextSettings.IsCopied Then Return
            '           BaseSettings_Set(aTextSettings.Strukture)
            ''============================================================
            '       Case dxxSettingTypes.TABLESETTINGS
            '           '============================================================
            '           Dim aTableSettings As dxoSettingsTable = aSettingObj
            '           If aTableSettings.IsCopied Then Return
            '           BaseSettings_Set(aTableSettings.Strukture)
            ''============================================================
            '       Case dxxSettingTypes.SYMBOLSETTINGS
            '           '============================================================
            '           Dim aSymbolSettings As dxoSettingsSymbol = aSettingObj
            '           If aSymbolSettings.IsCopied Then Return
            '           BaseSettings_Set(aSymbolSettings.Strukture)
            '           Dim setlayer As dxoLayer = Nothing
            '           Select Case aProp.Name.ToUpper
            '               Case "LAYERNAME"
            '                   Dim lname As String = aProp.ValueS.Trim()
            '                   If String.IsNullOrWhiteSpace(lname) Then Return
            '                   If Layers.TryGet(aProp.LastValue.ToString(), setlayer) Then
            '                       setlayer.Name = lname
            '                       setlayer.Color = aSymbolSettings.LayerColor
            '                   End If
            '                   If Layers.TryGet(lname, setlayer) Then
            '                       setlayer.Color = aSymbolSettings.LayerColor
            '                       Return
            '                   Else
            '                       setlayer = Layers.Add(lname, aSymbolSettings.LayerColor)
            '                   End If

            '               Case "LAYERCOLOR"
            '                   Dim lname As String = aSymbolSettings.LayerName
            '                   If Layers.TryGet(lname, setlayer) Then
            '                       setlayer.Color = aProp.Value
            '                   Else
            '                       setlayer = Layers.Add(lname, aSymbolSettings.LayerColor)
            '                   End If
            '           End Select
            '              '============================================================
            '       Case dxxReferenceTypes.STYLE
            '           '============================================================
            '           'screen text style
            '           Dim aStyle As dxoStyle = aSettingObj
            '           If Not aStyle.IsGlobal Then Return
            '           _Structure.obj_SCREEN.TextStyle = aStyle.Structure_Get()
            ''============================================================
            '       Case dxxReferenceTypes.DIMSTYLE, dxxSettingTypes.DIMOVERRIDES
            '           '============================================================
            '           'dimorverrides
            '           Dim aDimStyle As dxoDimStyle = aSettingObj
            '           If Not aDimStyle.IsGlobal Then
            '               RespondToTableMemberEvent(aDimStyle, False, aProp)
            '               Return
            '           End If
            '           If aDimStyle.IsCopied Then Return
            '           'update the overides

            '           Dim pname As String = aProp.Name.ToUpper
            '           If aProp.Hidden Then pname = Right(pname, pname.Length - 1)
            '           If pname.StartsWith("DIMBLK") Or pname.StartsWith("DIMLDRBLK") Then
            '               If Not aProp.Hidden Then
            '                   'the handle has changed
            '                   Dim aBlk As dxfBlock = Blocks.GetByBlockRecordHandle(aProp.Value.ToString)
            '                   If aBlk IsNot Nothing Then
            '                       pname = $"*{pname }_NAME"
            '                       DimStyle.PropValueSetByName(pname, aBlk.Name, True)
            '                   End If
            '               Else
            '                   'the name has changed
            '                   Dim aBlk As dxfBlock = Blocks.GetByName(aProp.Value.ToString(), True)
            '                   If aBlk IsNot Nothing Then
            '                       pname = dxfUtils.LeftOf(pname, "_", bFromEnd:=True)
            '                       aDimStyle.PropValueSetByName(pname, aBlk.BlockRecordHandle, True)
            '                   End If
            '               End If
            '           End If
            '           BaseSettings_Set(New TTABLEENTRY(aDimStyle))
            ''============================================================
            '       Case dxxSettingTypes.SCREENSETTINGS
            '           '============================================================
            '           bRedraw = False
            '           Dim aScreen As dxoScreen = aSettingObj
            '           If TLISTS.Contains(aProp.Name.ToUpper, "DRAWOCS,DRAWBOUNDINGRECTANGLES,DRAWEXTENTPTS,SUPPRESSED,TEXTBOXES") Then
            '               bRedraw = Entities.Count > 0
            '           ElseIf aProp.Name.ToUpper = "DRAWEXTENTRECTANGLE" Then
            '               bRedraw = True
            '           End If
            '           obj_SCREEN = aScreen.Strukture
            '            '============================================================
            '       Case dxxReferenceTypes.UCS
            '           '============================================================
            '           'Dim aUCS As dxoUCS = aSettingObj
            '           If Header.UCSMode <> dxxUCSIconModes.None Then bRedraw = True
            '       Case Else
            '           'Return
            '   End Select
            If Not bSuppressEvents Then RaiseSettingChange(mySettings, aProp)
            If bRedraw Then
                If AutoRedraw Then
                    Render()
                Else
                    IsDirty = True
                End If
            Else
                If bUCS Then
                    Render_UCSICon(False, True)
                End If
            End If
        End Sub


        Friend Sub RespondToTableEvent(aTable As dxfTable, aEventType As dxxCollectionEventTypes, aEntry As dxfTableEntry, ByRef rCancel As Boolean, aMakeCurrent As Boolean, ByRef rError As String, Optional bSuppressErrors As Boolean = False, Optional bSuppressEvents As Boolean = False)
            rError = String.Empty
            rCancel = True
            If aTable Is Nothing Then Return
            If aTable.ImageGUID <> GUID Then Return
            Try
                Dim refType As dxxReferenceTypes = aTable.TableType
                Dim refTypeName As String = aTable.TableTypeName
                Dim evntMsg As String = String.Empty
                If aEntry Is Nothing Then Throw New Exception($"The passed { refTypeName } is undefined")
                If aEntry.Properties.Count <= 0 Then
                    Throw New Exception($"The passed { refTypeName } is undefined")
                End If
                If aEntry.EntryType <> refType Then Throw New Exception($"The passed entry in not a {refTypeName}")
                Dim aNm As String = aEntry.Name
                rCancel = False
                Select Case aEventType
                    Case dxxCollectionEventTypes.PreAdd
#Region "PREADD"
                        rCancel = aTable.MemberExists(aNm)
                        If rCancel Then
                            rError = $"{refTypeName.ToUpper()} '{aNm}' Cannot be added because a { refTypeName.ToLower()} with that name already exists in the table"
                            Throw New Exception(rError)
                        End If
#End Region 'PREADD
                    Case dxxCollectionEventTypes.Add
#Region "ADD"
                        HandleGenerator?.AssignTo(aEntry)
                        evntMsg = $"{refTypeName} '{aNm}' Added"
                        If aMakeCurrent Then 'make the new entry the current refernece
                            Header.SetCurrentReferenceName(refType, aNm)
                        End If
#End Region 'ADD
                    Case dxxCollectionEventTypes.Remove
#Region "REMOVE"

                        aEntry.ImageGUID = ""
                        HandleGenerator.ReleaseHandle(aEntry.Handle)
                        evntMsg = $"{refTypeName} '{aNm}' Removed"
#End Region 'REMOVE
                    Case dxxCollectionEventTypes.PreRemove
#Region "PREREMOVE"
                        rCancel = Not ReferenceCanBeDeleted(dxxReferenceTypes.LAYER, aEntry.Handle, False, rError:=rError)
                        If rCancel Then
                            Throw New Exception(rError)
                        Else
                            'reset the header reference if one being removed is the current ref.
                            Select Case refType
                                Case dxxReferenceTypes.LAYER
                                    Header.SetCurrentReferenceName(refType, "0")
                                Case dxxReferenceTypes.DIMSTYLE, dxxReferenceTypes.STYLE
                                    Header.SetCurrentReferenceName(refType, "Standard")
                            End Select
                            Return
                        End If
#End Region 'PREREMOVE
                End Select
                If Not rCancel Then
                    ' Type Specific responses
                    Select Case refType
                        Case dxxReferenceTypes.LAYER
                            Dim layer As dxoLayer = DirectCast(aEntry, dxoLayer)
#Region "LAYER"
                            Select Case aEventType
                 '-----------------------------------------------------------------------------
                                Case dxxCollectionEventTypes.PreAdd
                                    layer.PropValueSet(dxxLayerProperties.Linetype, GetOrAdd(dxxReferenceTypes.LTYPE, layer.Linetype), True)
                            End Select
#End Region 'LAYER
                        Case dxxReferenceTypes.LTYPE
                            Dim linetype As dxoLinetype = DirectCast(aEntry, dxoLinetype)
#Region "LTYPE"
                            Select Case aEventType
                                '-----------------------------------------------------------------------------
                            End Select
#End Region 'LTYPE
                        Case dxxReferenceTypes.STYLE
#Region "STYLE"
                            Dim style As dxoStyle = DirectCast(aEntry, dxoStyle)
                            Select Case aEventType
                                '-----------------------------------------------------------------------------
                            End Select
#End Region 'STYLE
                        Case dxxReferenceTypes.DIMSTYLE
#Region "DSTYLE"
                            Dim dstyle As dxoDimStyle = DirectCast(aEntry, dxoDimStyle)
                            Select Case aEventType
                                Case dxxCollectionEventTypes.PreAdd
                                    'confirm the text style
                                    'get by name
                                    Dim aStr As String = dstyle.TextStyleName
                                    If String.IsNullOrWhiteSpace(aStr) Then
                                        aStr = "Standard"
                                        dstyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, aStr, True)
                                    End If
                                    Dim tstyle As dxoStyle = Styles.Member(aStr)
                                    If tstyle Is Nothing Then aStr = GetOrAdd(dxxReferenceTypes.STYLE, aStr, rEntry:=tstyle)
                                    If tstyle Is Nothing Then aStr = GetOrAdd(dxxReferenceTypes.STYLE, "Standard", rEntry:=tstyle)

                                    dstyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, tstyle.Name)
                                    dstyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY, tstyle.Handle)
                                Case Else
                            End Select
#End Region 'DSTYLE
                    End Select
                End If

                If rError <> "" And Not bSuppressErrors Then HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType(), New Exception(rError))
                If Not bSuppressEvents And evntMsg <> "" And Not rCancel Then
                    RaiseTableEvent(refTypeName, aEventType, evntMsg)
                End If
            Catch ex As Exception
                If Not bSuppressErrors Then HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType(), ex)
            End Try
        End Sub

        Friend Sub RespondToTableMemberEvent(aMember As dxfTableEntry, bSuppressEvents As Boolean, aProperty As dxoProperty)
            Dim rDontChange As Boolean = False
            Dim rMemberIndex As Integer = -1
            Dim rError As String = String.Empty
            Dim rRedraw As Boolean = False
            Dim rChangedProps As TPROPERTIES = TPROPERTIES.Null
            RespondToTableMemberEvent(aMember, aProperty, rDontChange, rMemberIndex, rError, rRedraw, bSuppressEvents, rChangedProps)
        End Sub
        Friend Sub RespondToTableMemberEvent(aMember As dxfTableEntry, bSuppressEvents As Boolean, aProperty As dxoProperty, ByRef rDontChange As Boolean)
            Dim rMemberIndex As Integer = -1
            Dim rError As String = String.Empty
            Dim rRedraw As Boolean = False
            Dim rChangedProps As TPROPERTIES = TPROPERTIES.Null
            RespondToTableMemberEvent(aMember, aProperty, rDontChange, rMemberIndex, rError, rRedraw, bSuppressEvents, rChangedProps)
        End Sub
        Friend Sub RespondToTableMemberEvent(aMember As dxfTableEntry, aProperty As dxoProperty, ByRef rDontChange As Boolean, ByRef rMemberIndex As Integer, ByRef rError As String, ByRef rRedraw As Boolean, bSuppressEvents As Boolean, ByRef rChangedProps As TPROPERTIES?)


            rDontChange = False
            rMemberIndex = -1
            rError = String.Empty
            rRedraw = False
            rChangedProps = TPROPERTIES.Null
            If aMember Is Nothing Or aProperty Is Nothing Then Return
            If aProperty.Name = "" Then Return
            'If aMember.IsGlobal Then
            '    RespondToSettingChange(aMember, aProperty, bSuppressEvents)
            '    Return
            'End If

            'get the table and the entry in the table & make sure it is actually a member of one on my tables!
            Dim aTable As dxfTable = Nothing
            Dim aEntry As dxfTableEntry = Tables.GetMatchingEntry(aMember, aTable, rMemberIndex)
            If aEntry Is Nothing Or rMemberIndex <= 0 Then Return

            If Not rChangedProps.HasValue Then rChangedProps = New TPROPERTIES("Changes")
            If Not rChangedProps.Value.ContainsKey(aProperty.Name) Then rChangedProps.Value.Add(aProperty)

            Dim aNm As String = aProperty.Name.ToUpper
            Dim mName As String = aEntry.Name
            Dim hndl As String = aEntry.Handle
            Dim refTypeName As String = aEntry.EntryTypeName
            Dim fname As String = $"RespondToTableMemberEvent_{refTypeName}"
            Try

                Select Case True
                    Case TypeOf aEntry Is dxoAPPID

                    Case TypeOf aEntry Is dxoBlockRecord
                    Case TypeOf aEntry Is dxoDimStyle
#Region "DSTYLE"
                        Dim aDStyle As dxoDimStyle = aEntry
                        If aProperty.GroupCode = 2 Then  'Name
                            'name change
                            Blocks.SetDisplayVariable(dxxDisplayProperties.DimStyle, aProperty.ValueS, TVALUES.To_STR(aProperty.LastValue))
                            Entities.SetDisplayVariable(dxxDisplayProperties.DimStyle, aProperty.ValueS, TVALUES.To_STR(aProperty.LastValue))
                        End If

                        Dim pname As String = aProperty.Name.ToUpper
                        If aProperty.Hidden Then pname = Right(pname, pname.Length - 1)
                        If pname.StartsWith("DIMBLK") Or pname.StartsWith("DIMLDRBLK") Then
                            If Not aProperty.Hidden Then
                                'the handle has changed
                                Dim aBlk As dxfBlock = Blocks.GetByBlockRecordHandle(aProperty.Value.ToString)
                                If aBlk IsNot Nothing Then
                                    pname = $"*{ pname }_NAME"
                                    aEntry.Properties.SetVal(pname, aBlk.Name)
                                End If
                            Else
                                'the name has changed
                                Dim aBlk As dxfBlock = Blocks.GetByName(aProperty.Value.ToString, True)
                                If aBlk IsNot Nothing Then
                                    pname = dxfUtils.LeftOf(pname, "_", bFromEnd:=True)
                                    aEntry.Properties.SetVal(pname, aBlk.BlockRecordHandle)
                                End If
                            End If
                        End If
                        'update the overrides
                        If aEntry.Name.ToUpper() = Header.DimStyleName.ToUpper Then
                            Dim aOride As dxsDimOverrides = _Settings.DimStyleOverrides
                            If aOride.Properties.SetVal(aProperty.Name, aProperty.Value) Then
                                RespondToSettingChange(aOride, aOride.Properties.Item(aProperty.Name), bSuppressEvents)
                            End If


                        End If
#End Region 'DSTYLE
                    Case TypeOf aEntry Is dxoLayer

#Region "LAYERS"
                        Dim aNewValue As Object = aProperty.Value
                        Dim aOldValue As Object = aProperty.LastValue
                        If aNewValue Is Nothing Then Return
                        If aOldValue Is Nothing Then Return
                        Select Case aProperty.GroupCode
             '------------------------------------------------------
                            Case 2 ' "NAME"
                                '------------------------------------------------------
                                aNewValue = TVALUES.To_STR(aNewValue, bTrim:=True)
                                aOldValue = TVALUES.To_STR(aOldValue, bTrim:=True)
                                If aOldValue = "0" Then
                                    rDontChange = True
                                    Throw New Exception($"An Attempt Was Made To Rename Layer '0' To '{ aNewValue }'")
                                Else
                                    rDontChange = aTable.MemberExists(TVALUES.To_STR(aNewValue))
                                    If rDontChange Then
                                        Throw New Exception($"An Attempt Was Made To Rename Layer '{ mName}' To '{ aNewValue}' Which Already Exists")
                                    End If
                                End If
                                If Not rDontChange Then
                                    'aEntry.Properties.SetVal(dxxLayerProperties.Name, aNewValue)
                                    Entities.SetDisplayVariable(dxxDisplayProperties.LayerName, aNewValue, aOldValue)
                                    Blocks.SetDisplayVariable(dxxDisplayProperties.LayerName, aNewValue, aOldValue)
                                End If
             '------------------------------------------------------
                            Case 6 '"LINETYPE"
                                '------------------------------------------------------
                                aNewValue = TVALUES.To_STR(aNewValue, bTrim:=True)
                                rDontChange = aNewValue = ""
                                If Not rDontChange Then
                                    rDontChange = String.Compare(aNewValue, dxfLinetypes.ByLayer, ignoreCase:=True) = 0 Or String.Compare(aNewValue, dxfLinetypes.ByBlock, ignoreCase:=True) = 0
                                End If
                                If Not rDontChange Then
                                    aNewValue = GetOrAdd(dxxReferenceTypes.LTYPE, aNewValue)
                                    ' If aEntry.Properties.SetVal(dxxLayerProperties.Linetype, aNewValue) Then
                                    rRedraw = True
                                    IsDirty = True
                                    'End If
                                Else
                                    Throw New Exception("Layer Linetype values cannot be logical linetypes")
                                End If
             '------------------------------------------------------
                            Case 62 '"COLOR"
                                '------------------------------------------------------
                                Dim aclr As dxxColors = TVALUES.To_INT((aNewValue))
                                rDontChange = Math.Abs(aclr) = dxxColors.Undefined Or Math.Abs(aclr) = dxxColors.ByLayer Or Math.Abs(aclr) = dxxColors.ByBlock
                                If Not rDontChange Then
                                    IsDirty = True
                                    'If aEntry.Properties.SetVal(dxxLayerProperties.Color, aclr) Then
                                    rRedraw = True
                                    'IsDirty = True
                                    'End If
                                    If Math.Abs(aProperty.Value) = Math.Abs(aProperty.LastValue) Then
                                        'visibility change
                                        rChangedProps.Value.Add(New TPROPERTY(aProperty.GroupCode, TVALUES.To_INT(aProperty.Value) > 0, "Visible", dxxPropertyTypes.dxf_Boolean, TVALUES.To_INT(aProperty.Value) <= 0) With {.Path = aTable.Name & ".Item[" & aMember.Name & "]"})
                                    End If
                                Else
                                    Throw New Exception("Invalid Layer Color Detect. Layers cannot have logical colors")
                                End If
                            '------------------------------------------------------'
                            Case 70 '"Bit Code"
                                '------------------------------------------------------
                                rRedraw = True
                                'aEntry.Properties.SetVal(aProperty.GroupCode, aNewValue)'
                        '------------------------------------------------------
                            Case 370 '"LINE WEIGHT"
                                '------------------------------------------------------
                                Dim aLWt As dxxLineWeights = TVALUES.To_INT(aNewValue)
                                If aLWt <> dxxLineWeights.ByDefault Then
                                    rDontChange = aLWt < dxxLineWeights.LW_000 Or aLWt > dxxLineWeights.LW_211
                                End If
                                If Not rDontChange Then
                                    IsDirty = True
                                    'If aEntry.Properties.SetVal(dxxLayerProperties.LineWeight, aLWt) Then IsDirty = True
                                    rRedraw = Header.LineWeightDisplay
                                Else
                                    Throw New Exception("Invalid Lineweight Detect. Layers cannot have logical lineweights")
                                End If
                            Case Else
                                'aEntry.Properties.SetVal(aProperty.GroupCode, aNewValue)
                        End Select
                        If Not rDontChange Then
                            IsDirty = True
                            'aEntry.Properties.SetVal(aProperty.Name, aProperty.Value)  'probably don't need this
                            aTable.UpdateEntry = aEntry
                            Entities.RespondToLayerChange(Me, aEntry, aProperty)
                            Blocks.RespondToLayerChange(Me, aEntry, aProperty)
                        End If
#End Region 'LAYERS                
                    Case TypeOf aEntry Is dxoLinetype
                    Case TypeOf aEntry Is dxoStyle
#Region "STYLE"
                        Dim aTStyle As dxoStyle = aEntry
                        Dim aNewValue As Object = aProperty.Value
                        Dim aOldValue As Object = aProperty.LastValue
                        If aNewValue Is Nothing Then Return
                        If aOldValue Is Nothing Then Return
                        If Not rDontChange Then
                            If String.Compare(aProperty.Name, "FONT", True) <> 0 Then
                                If Entities.zDirtyText(aTStyle, Me) Then IsDirty = True
                                rRedraw = True
                            End If


                        End If
#End Region 'STYLE
                    Case TypeOf aEntry Is dxoUCS

                    Case TypeOf aEntry Is dxoView
                    Case TypeOf aEntry Is dxoViewPort

                End Select




                If Not rDontChange Then
                    If aProperty.PropertyType = dxxPropertyTypes.BitCode And aProperty._EnumName IsNot Nothing Then
                        Dim pname As String = aProperty.DisplayName
                        Dim vlast As Object = aProperty.LastValue
                        Dim vnew As Object = aProperty.Value
                        vnew = TVALUES.BitCode_FindSubCode(TVALUES.To_INT(aProperty.Value), aProperty._EnumName)

                        rChangedProps.Value.Add(New TPROPERTY(aProperty.GroupCode, vnew, pname, dxxPropertyTypes.dxf_Boolean, vlast))
                    End If
                End If
            Catch ex As Exception
                rError = ex.Message
            Finally

                If rError <> "" Then
                    If Not bSuppressEvents Then HandleError(fname, "dxfImage", rError)
                End If
                If Not bSuppressEvents And rChangedProps.Value.Count > 0 Then
                    For i As Integer = 1 To rChangedProps.Value.Count
                        RaiseTableMemberEvent(aEntry, rChangedProps.Value.Item(i))
                    Next
                End If
                If rRedraw Then
                    If AutoRedraw And Not bSuppressEvents Then Render() Else IsDirty = True
                End If
            End Try
        End Sub
#End Region 'Reactions
#Region "Methods"
        Public Function FileName(Optional aFileType As dxxFileTypes = dxxFileTypes.Undefined, Optional bSuppressPath As Boolean = False) As String
            '^the filename that is used when a request is made to write the file to disk

            Dim path As String = String.Empty

            If Not bSuppressPath Then path = FolderPath
            If String.IsNullOrWhiteSpace(path) Then bSuppressPath = True

            If Not dxfEnums.Validate(GetType(dxxFileTypes), aFileType, bSkipNegatives:=True) Then aFileType = FileType
            Dim fname As String = Name.Replace(".", "_")
            If Not bSuppressPath Then
                Return IO.Path.Combine(path, $"{fname}.{FileExtension()}")
            Else
                Return $"{fname}.{FileExtension()}"
            End If



        End Function

        Friend Sub FileName_Set(aFullPath As String, Optional aFileType As dxxFileTypes = dxxFileTypes.Undefined, Optional bSuppressPath As Boolean = False)
            '^the filename that is used when a request is made to write the file to disk

            '^the filename that is used when a request is made to write the file to disk

            Dim pth As String = String.Empty
            aFullPath = aFullPath.Trim()
            If aFullPath = "" Then
                FolderPath = ""
            Else
                Dim i As Integer = aFullPath.LastIndexOf("\")
                If i > 0 Then
                    FolderPath = IO.Path.GetDirectoryName(aFullPath)
                    Name = IO.Path.GetFileName(aFullPath)
                    pth = aFullPath.Substring(0, i + 1).Trim()

                    aFullPath = aFullPath.Substring(i + 1, aFullPath.Length - i - 1).Trim()
                End If

            End If

        End Sub




        Public Overrides Function ToString() As String
            If Not Disposed Then
                Return $"dxfImage[{ GUID }]"
            Else
                Return "dxfImage[Disposed]"
            End If
        End Function
        Public Function GetImage(bScreen As Boolean) As System.Drawing.Bitmap
            If _Disposed Then Return Nothing
            If Not bScreen Then
                If Not _Bitmap.IsDefined Then Return Nothing
                Return _Bitmap.Bitmap.Clone
            Else
                If Not _ScreenBitmap.IsDefined Then Return Nothing
                Return _ScreenBitmap.Bitmap.Clone
            End If
        End Function
        Public Function Bitmap(Optional bReturnScreen As Boolean = False) As dxfBitmap
            Dim _rVal As dxfBitmap = GetBitmap(bReturnScreen)
            '^returns a copy of the current Image bitmap or that of the passed device contex
            If _rVal Is Nothing Then Return Nothing
            If _rVal.Disposed Then Return Nothing
            Return _rVal.Clone
        End Function
        Public Sub Clear(Optional aRefsToRetain As dxxReferenceTypes = dxxReferenceTypes.UNDEFINED, Optional bSuppressRedraw As Boolean = False)
            '#1an addative argument to indicate the things to retain
            '#2flag to suppress a redraw after the clearing operation is complete
            '^clears the entities, blocks and table entities (layers, linetype, styles etc.) defined in the image
            '~if the VPORT reference is passed the current view is retained
            GUID = ""
            Dim cntWuz As Integer = 0
            If _Entities IsNot Nothing Then cntWuz = Entities.Count
            If Disposed Then
                _Structure.GUID = ""
                _Structure.Clears = 0
                Init(-100)
                _Disposed = False
            Else
                Init(aRefsToRetain)
            End If
            'System.Diagnostics.Debug.WriteLine(_Structure.GUID & " Reset(" &_Structure & ")")
            If Not bSuppressRedraw And cntWuz > 0 Then Render()
        End Sub
        Public Function CopyBlockFromFile(aFileSpec As String, aBlockName As String, bOverwriteExistingReferences As Boolean, bSuppressErrors As Boolean) As Boolean
            Dim rErrorString As String = String.Empty

            Return CopyBlockFromFile(aFileSpec, aBlockName, bOverwriteExistingReferences, bSuppressErrors, rErrorString)
        End Function
        Public Function CopyBlockFromFile(aFileSpec As String, aBlockName As String, bOverwriteExistingReferences As Boolean, bSuppressErrors As Boolean, ByRef rErrorString As String) As Boolean
            rErrorString = ""
            aFileSpec = Trim(aFileSpec)
            aBlockName = Trim(aBlockName)

            If aFileSpec = "" Or aBlockName = "" Then Return False 'no error
            Dim dfile As New dxfFile(aFileSpec)
            Try
                If Not IO.File.Exists(aFileSpec) Then
                    rErrorString = $"File Not Found '{ aFileSpec }'"
                    Return False
                End If


                dfile.ErrorStrings = New List(Of String)
                dfile.Read(aFileSpec, True)
                If dfile.ErrorStrings.Count > 0 Then

                    Return False
                End If


                dfile.TransferToImage_Blocks(Me, Not bOverwriteExistingReferences, aBlockName, False, False, True)

            Catch ex As Exception
                dfile.ErrorStrings.Add(ex.Message)
                Return False
            Finally
                If dfile.ErrorStrings.Count > 0 Then
                    rErrorString = dfile.ErrorStrings.LastOrDefault
                    If Not bSuppressErrors Then HandleError(Reflection.MethodBase.GetCurrentMethod, $"dxfImage {Name}", dfile.ErrorStrings)

                End If

            End Try
            Return String.IsNullOrWhiteSpace(rErrorString)
        End Function
        Public Function CopyBlockFromImage(aImage As dxfImage, aBlockName As String, Optional bOverwriteExistingReferences As Boolean = False, Optional bSuppressErrors As Boolean = False) As Boolean
            Dim rErrorString As String = String.Empty

            If aImage Is Nothing Then Return False
            Return CopyBlockFromImage(aImage, aBlockName, bOverwriteExistingReferences, bSuppressErrors, rErrorString)
        End Function
        Public Function CopyBlockFromImage(aImage As dxfImage, aBlockName As String, bOverwriteExistingReferences As Boolean, bSuppressErrors As Boolean, ByRef rErrorString As String) As Boolean
            If aImage Is Nothing Then Return False
            Dim _rVal As Boolean = False
            rErrorString = ""

            If String.IsNullOrWhiteSpace(aBlockName) Then Return _rVal 'no error
            aBlockName = Trim(aBlockName)
            Try
                If Not TLISTS.Contains(aBlockName, Blocks.NamesList) Then
                    Throw New Exception("Block '" & aBlockName & "' Was Not Found In The Passed Image")
                End If
                _rVal = dxfImageTool.TransferBlockToImage(Me, aImage, aBlockName, bOverwriteExistingReferences)
            Catch ex As Exception
                rErrorString = HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message, bSuppressErrors)
            End Try
            Return _rVal
        End Function


        Friend Function CreateText(aStyle As dxoStyle, aTextString As String, aTextType As dxxTextTypes, aDirectionFlag As dxxTextDrawingDirections, Optional aTextHeight As Double = 0.0, Optional aAlignment As dxxMTextAlignments = dxxMTextAlignments.AlignUnknown, Optional aWidthFactor As Double = 0.0, Optional aAngle As Double = 0.0, Optional aObliqueAngle As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aInsertPt As iVector = Nothing) As dxeText
            If aStyle Is Nothing Then aStyle = TextStyle()
            If aTextType < dxxTextTypes.DText Or aTextType > dxxTextTypes.Multiline Then aTextType = dxxTextTypes.Multiline
            Dim _rVal As New dxeText(aTextType)
            If aTextType = dxxTextTypes.Multiline Then
                If aDirectionFlag <> dxxTextDrawingDirections.Horizontal And aDirectionFlag <> dxxTextDrawingDirections.Vertical And aDirectionFlag <> dxxTextDrawingDirections.ByStyle Then
                    aDirectionFlag = dxxTextDrawingDirections.ByStyle
                End If
            Else
                aDirectionFlag = dxxTextDrawingDirections.ByStyle
            End If
            Dim tht As Double = Math.Abs(Math.Round(aTextHeight, 8))
            If tht <= 0 Then tht = aStyle.PropValueD(dxxStyleProperties.TEXTHT)
            If tht <= 0 Then tht = Header.TextSize
            If Not dxfPlane.IsNull(aPlane) Then _rVal.Plane = aPlane Else _rVal.Plane = UCS
            If aInsertPt Is Nothing Then aInsertPt = New dxfVector

            _rVal.AlignmentPt1V = New TVECTOR(aPlane:=_rVal.Plane, aVector:=aInsertPt)

            If aDirectionFlag = dxxTextDrawingDirections.ByStyle Then
                _rVal.Vertical = aStyle.PropValueB(dxxStyleProperties.VERTICAL)
            Else
                _rVal.Vertical = aDirectionFlag = dxxTextDrawingDirections.Vertical
            End If
            _rVal.ObliqueAngle = aStyle.PropValueD(dxxStyleProperties.OBLIQUE)
            _rVal.WidthFactor = aStyle.PropValueD(dxxStyleProperties.WIDTHFACTOR)
            _rVal.LineSpacingFactor = aStyle.PropValueD(dxxStyleProperties.LINESPACING)
            _rVal.LineSpacingStyle = aStyle.PropValueD(dxxStyleProperties.LINESPACINGSTYLE)
            _rVal.TextHeight = tht
            _rVal.TextString = aTextString
            If aAlignment >= 1 And aAlignment <= 14 Then _rVal.Alignment = aAlignment
            _rVal.Rotation = aAngle
            _rVal.TextStyleName = aStyle.PropValueStr(dxxStyleProperties.NAME)
            If _rVal.TextType <> dxxTextTypes.Multiline Then
                If aWidthFactor > 0 Then _rVal.WidthFactor = aWidthFactor
            End If
            If aObliqueAngle IsNot Nothing Then
                If aObliqueAngle.HasValue Then _rVal.ObliqueAngle = aObliqueAngle.Value
            End If
            _rVal.ImageGUID = GUID
            Return _rVal
        End Function

        Friend Function CreateUCSVector(aVectorXY As iVector, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As TVECTOR
            '#1the X,Y,Z vector object passng the coordinates for the new vector
            '#2If True is passed the world coordinate system otherwise the current UCS is used
            '#3If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#4 an optional plane to use instead of the current UCS or the world plane.
            '^used to create a new vector with respect to the origin of the system
            Dim rPlane As dxfPlane = Nothing
            Return CreateUCSVector(aVectorXY, bSuppressUCS, bSuppressElevation, rPlane:=rPlane)
        End Function
        Friend Function CreateUCSVector(aVectorXY As iVector, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane) As TVECTOR
            Dim _rVal As New TVECTOR(aVectorXY)
            '#1the X,Y,Z vector object passng the coordinates for the new vector
            '#2If True is passed the world coordinate system otherwise the current UCS is used
            '#3If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#4 an optional plane to use instead of the current UCS or the world plane.
            '^used to create a new vector with respect to the origin of the system
            Dim aPl As TPLANE
            Dim bNoPlane As Boolean = dxfPlane.IsNull(rPlane)
            If bNoPlane Then
                If bSuppressUCS Then
                    aPl = New TPLANE("World")
                    bSuppressElevation = True
                Else
                    aPl = New TPLANE(obj_UCS)
                End If

            Else
                aPl = New TPLANE(rPlane)
                bSuppressElevation = True
            End If
            If Not bSuppressElevation Then
                aPl.Origin += aPl.ZDirection * Header.Elevation
            End If

            _rVal = aPl.Vector(_rVal.X, _rVal.Y, _rVal.Z)
            If bNoPlane Then rPlane = New dxfPlane(aPl)
            Return _rVal

        End Function

        Friend Function CreateUCSVector(aVectorXY As TVECTOR, bSuppressUCS As Boolean, Optional bSuppressElevation As Boolean = False) As TVECTOR
            '#1the X,Y,Z vector object passng the coordinates for the new vector
            '#2If True is passed the world coordinate system otherwise the current UCS is used
            '#3If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#4 an optional plane to use instead of the current UCS or the world plane.
            '^used to create a new vector with respect to the origin of the system
            Dim rPlane As dxfPlane = Nothing

            Return CreateUCSVector(aVectorXY, bSuppressUCS, bSuppressElevation, rPlane:=rPlane)
        End Function
        Friend Function CreateUCSVector(aVectorXY As TVECTOR, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane) As TVECTOR
            Dim _rVal As New TVECTOR(aVectorXY)
            '#1the X,Y,Z vector object passng the coordinates for the new vector
            '#2If True is passed the world coordinate system otherwise the current UCS is used
            '#3If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#4 an optional plane to use instead of the current UCS or the world plane.
            '^used to create a new vector with respect to the origin of the system
            Dim aPl As TPLANE

            Dim bNoPlane As Boolean = dxfPlane.IsNull(rPlane)
            If bNoPlane Then
                If bSuppressUCS Then
                    aPl = New TPLANE("World")
                    bSuppressElevation = True
                Else
                    aPl = New TPLANE(obj_UCS)
                End If

            Else
                aPl = New TPLANE(rPlane)
                bSuppressElevation = True
            End If
            If Not bSuppressElevation Then
                aPl.Origin += aPl.ZDirection * Header.Elevation
            End If
            _rVal = aPl.Vector(_rVal.X, _rVal.Y, _rVal.Z)
            If bNoPlane Then rPlane = New dxfPlane(aPl)
            Return _rVal

        End Function



        Friend Function CreateUCSVector(aVectorX As Double, aVectorY As Double, aVectorZ As Double, bSuppressUCS As Boolean, Optional bSuppressElevation As Boolean = False) As TVECTOR
            '#1the X coordinate for the new vertex
            '#2the Y coordinate for the new vertex
            '#3the Z coordinate for the new vertex
            '#4If True is passed the world coordinate system otherwise the uccrent UCS is used
            '#5If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#6 an optional plane to use instead of the current UCs or the world plane.
            '^used to create a new vertex with respect to the origin of the system
            Dim rPlane As dxfPlane = Nothing
            Return CreateUCSVector(New TVECTOR(aVectorX, aVectorY, aVectorZ), bSuppressUCS, bSuppressElevation, rPlane:=rPlane)
        End Function


        Friend Function CreateUCSVertex(Optional aVectorX As Double = 0, Optional aVectorY As Double = 0, Optional aVectorZ As Double = 0, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As TVERTEX
            Dim rPlane As dxfPlane = Nothing
            Return CreateUCSVertex(aVectorX, aVectorY, aVectorZ, bSuppressUCS, bSuppressElevation, rPlane)
        End Function
        Friend Function CreateUCSVertex(aVectorX As Double, aVectorY As Double, aVectorZ As Double, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane) As TVERTEX
            Dim _rVal As New TVERTEX(aVectorX, aVectorY, aVectorZ)
            '#1the X coordinate for the new vertex
            '#2the Y coordinate for the new vertex
            '#3the Z coordinate for the new vertex
            '#4If True is passed the world coordinate system otherwise the uccrent UCS is used
            '#5If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#6 an optional plane to use instead of the current UCs or the world plane.
            '^used to create a new vertex with respect to the origin of the system
            Dim aPl As TPLANE
            Dim bNoPlane As Boolean = dxfPlane.IsNull(rPlane)
            If bNoPlane Then
                If bSuppressUCS Then
                    aPl = New TPLANE("World")
                    bSuppressElevation = True
                Else
                    aPl = New TPLANE(obj_UCS)
                End If

            Else
                aPl = New TPLANE(rPlane)
                bSuppressElevation = True
            End If
            If Not bSuppressElevation Then
                aPl.Origin += aPl.ZDirection * Header.Elevation
            End If
            _rVal.Vector = aPl.Vector(_rVal.X, _rVal.Y, _rVal.Z)
            If bNoPlane Then rPlane = New dxfPlane(aPl)
            Return _rVal
        End Function
        Friend Function CreateUCSVertex(aVectorXY As iVector, bSuppressUCS As Boolean, Optional bSuppressElevation As Boolean = False) As TVERTEX
            Dim rPlane As dxfPlane = Nothing
            Return CreateUCSVertex(aVectorXY, bSuppressUCS, bSuppressElevation, rPlane)
        End Function
        Friend Function CreateUCSVertex(aVectorXY As iVector, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane) As TVERTEX
            '#1the vector to convert to  UCS vector
            '#4If True is passed the world coordinate system otherwise the uccrent UCS is used
            '#5If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#6 an optional plane to use instead of the current UCs or the world plane.
            '^used to create a new vertex with respect to the origin of the system
            Dim _rVal As New TVERTEX(aVectorXY)
            Dim aPl As TPLANE
            Dim bNoPlane As Boolean = TPLANE.IsNull(rPlane)
            If bNoPlane Then

                If bSuppressUCS Then aPl = New TPLANE("World") Else aPl = New TPLANE(obj_UCS)
                bSuppressElevation = True
            Else
                aPl = New TPLANE(rPlane)
                bSuppressElevation = True
            End If
            If Not bSuppressElevation Then
                aPl.Origin += obj_UCS.ZDirection * Header.Elevation
            End If
            _rVal.Vector = aPl.Vector(_rVal.X, _rVal.Y, _rVal.Z)
            If bNoPlane Then rPlane = New dxfPlane(aPl)
            Return _rVal
        End Function
        Public Function CreateVector(aVectorXY As iVector, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxfVector
            '#1the object to use to get the XYZ coordinates from
            '^used to create a new vector with respect to the origin of the system based on the passed object coordinates
            Return New dxfVector(CreateUCSVertex(aVectorXY, bSuppressUCS, bSuppressElevation))

        End Function
        Public Function CreateVector(aVectorXY As iVector, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane) As dxfVector
            '#1the object to use to get the XYZ coordinates from
            '^used to create a new vector with respect to the origin of the system based on the passed object coordinates
            Return New dxfVector(CreateUCSVertex(aVectorXY, bSuppressUCS, bSuppressElevation, rPlane))
        End Function
        Public Function CreateVector(aVectorX As Double, aVectorY As Double, Optional aVectorZ As Double = 0, Optional aRotation As Double = 0, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxfVector
            Return New dxfVector(CreateUCSVector(New TVECTOR(aVectorX, aVectorY, aVectorZ), bSuppressUCS, bSuppressElevation, Nothing)) With {.Rotation = aRotation}
        End Function
        Public Function CreateVector(aVectorX As Double, aVectorY As Double, aVectorZ As Double, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane, Optional aRotation As Double = 0) As dxfVector
            Return New dxfVector(CreateUCSVector(New TVECTOR(aVectorX, aVectorY, aVectorZ), bSuppressUCS, bSuppressElevation, rPlane)) With {.Rotation = aRotation}
            '#1the X coordinate for the new vector
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '#4If True is passed the world coordinate system otherwise the uccrent UCS is used
            '#5If True the elevation of the return point with respect to the operational plane will be 0 otherwise the current global elevation is applied
            '#6 an optional plane to use instead of the current UCs or the world plane.
            '^used to create a new vector with respect to the origin of the system
        End Function
        Public Function CreateVectors(aVectorsXYZ As IEnumerable(Of iVector), Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bErrorOnEmpty As Boolean = False, Optional bReturnEmpty As Boolean = False) As colDXFVectors
            Dim arPlane As dxfPlane = Nothing
            Return CreateVectors(aVectorsXYZ, bSuppressUCS, bSuppressElevation, arPlane, bErrorOnEmpty)
        End Function
        Public Function CreateVectors(aVectorsXYZ As IEnumerable(Of iVector), bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane, Optional bErrorOnEmpty As Boolean = False, Optional bReturnEmpty As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors

            '#1the vector or vectors objects (anything with XYZ Properties)
            '#2if True the vectors are treated as world vectors otherwise their ordinates are used to create world vectors with respect to the current UCS
            '#3if True and the SuppressUCS argument is True then the vectors z is used otherwise the current elevation is used
            '#4flag to raise an Error if the passed vector source will return an empty collection otherwise a single 0,0,0 vector is returned
            '#5returns a copy of the current UCS
            '^used to create a collection of dxoVectors based on the passed collection or single xyz object(s)
            '~clones are returned if coldxfVectors is passed
            Try
                'to get the coorediante system
                CreateUCSVector(TVECTOR.Zero, bSuppressUCS, bSuppressElevation, rPlane)
                Dim bPlane As TPLANE = rPlane.Strukture
                Dim aVrts As New TVERTICES(aVectorsXYZ)
                Dim vrt As TVERTEX
                If aVrts.Count <= 0 Then
                    If bErrorOnEmpty Then
                        Throw New Exception("Empty Collection Passed")
                    Else
                        If Not bReturnEmpty Then _rVal.AddV(bPlane.Origin)
                    End If
                Else
                    For i As Integer = 1 To aVrts.Count
                        vrt = aVrts.Item(i)
                        vrt.Vector = bPlane.Vector(vrt.Vector.X, vrt.Vector.Y, aRotation:=vrt.Rotation)
                        _rVal.Add(New dxfVector(vrt))
                    Next i
                End If
                Return _rVal
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
                Return _rVal
            End Try
        End Function
        Friend Function CreateUCSVectors(aVectorsXYZ As IEnumerable(Of iVector), Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bErrorOnEmpty As Boolean = False, Optional bReturnEmpty As Boolean = False, Optional aMaxReturn As Integer? = Nothing) As TVECTORS
            Dim arPlane As dxfPlane = Nothing
            Return CreateUCSVectors(aVectorsXYZ, bSuppressUCS, bSuppressUCS, arPlane, bErrorOnEmpty, bReturnEmpty, aMaxReturn)
        End Function
        Friend Function CreateUCSVectors(aVectorsXYZ As IEnumerable(Of iVector), bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane, Optional bErrorOnEmpty As Boolean = False, Optional bReturnEmpty As Boolean = False, Optional aMaxReturn As Integer? = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '#1the vector or vectors objects (anything with XYZ Properties)
            '#2if True the vectors are treated as world vectors otherwise their ordinates are used to create world vectors with respect to the current UCS
            '#3if True and the SuppressUCS argument is True then the vectors z is used otherwise the current elevation is used
            '#4flag to raise an Error if the passed vector source will return an empty collection otherwise a single 0,0,0 vector is returned
            '#5returns a copy of the current UCS
            '^used to create a collection of dxoVectors based on the passed collection or single xyz object(s)
            '~clones are returned if real vectors are passed
            Try
                'to get the coorediante system
                CreateUCSVector(TVECTOR.Zero, bSuppressUCS, bSuppressElevation, rPlane)
                Dim bPlane As TPLANE = rPlane.Strukture
                Dim aVrts As New TVERTICES(aVectorsXYZ)
                Dim vrt As TVERTEX
                If aVrts.Count <= 0 Then
                    If bErrorOnEmpty Then
                        Throw New Exception("Empty Collection Passed")
                    Else
                        If Not bReturnEmpty Then

                            _rVal.Add(bPlane.Origin)
                        End If
                    End If
                Else
                    For i As Integer = 1 To aVrts.Count
                        If aMaxReturn.HasValue AndAlso i > aMaxReturn.Value Then Exit For
                        vrt = aVrts.Item(i)
                        vrt.Vector = bPlane.Vector(vrt.Vector.X, vrt.Vector.Y, aVectorRotation:=vrt.Vector.Rotation)

                        _rVal.Add(vrt.Vector)
                    Next i
                End If
                Return _rVal
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
                Return _rVal
            End Try
        End Function
        Friend Function CreateUCSVertices(aVectorsXYZ As IEnumerable(Of iVector), Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bErrorOnEmpty As Boolean = False, Optional bReturnEmpty As Boolean = False) As TVERTICES
            Dim arPlane As dxfPlane = Nothing
            Return CreateUCSVertices(aVectorsXYZ, bSuppressUCS, bSuppressElevation, arPlane, bErrorOnEmpty, bReturnEmpty)
        End Function
        Friend Function CreateUCSVertices(aVectorsXYZ As IEnumerable(Of iVector), bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane, Optional bErrorOnEmpty As Boolean = False, Optional bReturnEmpty As Boolean = False) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            '#1the vector or vectors objects (anything with XYZ Properties)
            '#2if True the vectors are treated as world vectors otherwise their ordinates are used to create world vectors with respect to the current UCS
            '#3if True and the SuppressUCS argument is True then the vectors z is used otherwise the current elevation is used
            '#4flag to raise an Error if the passed vector source will return an empty collection otherwise a single 0,0,0 vector is returned
            '#5returns a copy of the current UCS
            '^used to create a collection of dxoVectors based on the passed collection or single xyz object(s)
            '~clones are returned if real vectors are passed
            Try
                'to get the coorediante system
                CreateUCSVector(TVECTOR.Zero, bSuppressUCS, bSuppressElevation, rPlane)
                Dim bPlane As TPLANE = rPlane.Strukture
                Dim aVrts As New TVERTICES(aVectorsXYZ)
                Dim vrt As TVERTEX
                If aVrts.Count <= 0 Then
                    If bErrorOnEmpty Then
                        Throw New Exception("Empty Collection Passed")
                    Else
                        If Not bReturnEmpty Then

                            _rVal.Add(bPlane.Origin)
                        End If
                    End If
                Else
                    For i As Integer = 1 To aVrts.Count
                        vrt = aVrts.Item(i)
                        vrt.Vector = bPlane.Vector(vrt.Vector.X, vrt.Vector.Y, aVectorRotation:=vrt.Rotation)

                        _rVal.Add(vrt)
                    Next i
                End If
                Return _rVal
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
                Return _rVal
            End Try
        End Function


        Friend Function DeleteGraphicEntity(aHandles As THANDLES) As Boolean
            Dim _rVal As Boolean = False
            If aHandles.Handle = "" Then Return False
            If aHandles.ImageGUID <> GUID Then Return False
            Dim aEnt As dxfEntity = Nothing
            Dim aBlock As dxfBlock = Nothing
            If aHandles.BlockGUID <> "" Then

                If Blocks.TryGet(aHandles.BlockGUID, aBlock) Then
                    If aBlock.Entities.TryGet(aHandles.GUID, aEnt) Then
                        _rVal = True
                        aBlock.Entities.Remove(aEnt)
                    End If
                End If
            End If

            If Not _rVal Then
                If Entities.TryGet(aHandles.GUID, aEnt) Then
                    Entities.Remove(aEnt)
                    _rVal = True
                End If
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the requested or current dimension style
        ''' </summary>
        ''' <remarks>If the name doesn't exist he current dimstyle listed in the header constant $DIMSTYLE is returned</remarks>
        ''' <param name="aName">the name of the table entry to retrieve</param>
        ''' <param name="bReturnCurrentIfNotFound">flag to return the current entry if the passed name is not the name of an existing entry </param>
        ''' <returns></returns>
        Public Function DimStyle(Optional aName As String = "", Optional bReturnCurrentIfNotFound As Boolean = True) As dxoDimStyle

            Dim getdefault As Boolean = String.IsNullOrWhiteSpace(aName)
            Dim aEntry As dxoDimStyle = Nothing
            If Not getdefault Then
                If Not DimStyles.TryGet(aName.Trim(), aEntry) Then
                    If bReturnCurrentIfNotFound Then
                        getdefault = True
                    Else
                        Return Nothing
                    End If
                Else
                    Return aEntry
                End If
            End If

            If getdefault Then
                Dim curname As String = Header.DimStyleName
                If String.IsNullOrWhiteSpace(curname) Then curname = "Standard"
                If Not DimStyles.TryGet(curname, aEntry) Then
                    aEntry = DirectCast(GetOrAddReference(curname, dxxReferenceTypes.DIMSTYLE), dxoDimStyle)

                End If
                Header.PropValueSet(dxxHeaderVars.DIMSTYLE, aEntry.Name, bSuppressEvnts:=True)
            End If
            Return aEntry

        End Function

        ''' <summary>
        ''' returns the requested or current layer
        ''' </summary>
        ''' <remarks>If the name doesn't exist he current layer listed in the header constant $CLAYER is returned</remarks>
        ''' <param name="aName">the name of the table entry to retrieve</param>
        ''' <param name="bReturnCurrentIfNotFound">flag to return the current entry if the passed name is not the name of an existing entry </param>
        ''' <returns></returns>
        Public Function Layer(Optional aName As String = "", Optional bReturnCurrentIfNotFound As Boolean = True) As dxoLayer

            Dim getdefault As Boolean = String.IsNullOrWhiteSpace(aName)
            Dim aEntry As dxoLayer = Nothing
            If Not getdefault Then
                If Not Layers.TryGet(aName.Trim(), aEntry) Then
                    If bReturnCurrentIfNotFound Then
                        getdefault = True
                    Else
                        Return Nothing
                    End If
                Else
                    Return aEntry
                End If
            End If

            If getdefault Then
                Dim curname As String = Header.LayerName
                If String.IsNullOrWhiteSpace(curname) Then curname = "0"
                If Not Layers.TryGet(curname, aEntry) Then
                    aEntry = DirectCast(GetOrAddReference(curname, dxxReferenceTypes.LAYER), dxoLayer)

                End If
                Header.PropValueSet(dxxHeaderVars.CLAYER, aEntry.Name, bSuppressEvnts:=True)
            End If
            Return aEntry

        End Function

        ''' <summary>
        ''' returns the requested or current text style
        ''' </summary>
        ''' <remarks>If the name doesn't exist he current layer listed in the header constant $TEXTSTYLE is returned</remarks>
        ''' <param name="aName">the name of the table entry to retrieve</param>
        ''' <param name="bReturnCurrentIfNotFound">flag to return the current entry if the passed name is not the name of an existing entry </param>
        ''' <returns></returns>
        Public Function TextStyle(Optional aName As String = "", Optional bReturnCurrentIfNotFound As Boolean = True) As dxoStyle
            Dim getdefault As Boolean = String.IsNullOrWhiteSpace(aName)
            Dim aEntry As dxoStyle = Nothing
            If Not getdefault Then
                If Not Styles.TryGet(aName.Trim(), aEntry) Then
                    If bReturnCurrentIfNotFound Then
                        getdefault = True
                    Else
                        Return Nothing
                    End If
                Else
                    Return aEntry
                End If
            End If

            If getdefault Then
                Dim curname As String = Header.TextStyleName
                If String.IsNullOrWhiteSpace(curname) Then curname = "Standard"
                If Not Styles.TryGet(curname, aEntry) Then
                    aEntry = DirectCast(GetOrAddReference(curname, dxxReferenceTypes.STYLE), dxoStyle)

                End If
                Header.PropValueSet(dxxHeaderVars.TEXTSTYLE, aEntry.Name, bSuppressEvnts:=True)
            End If
            Return aEntry


        End Function

        Public Function DimTextStyle(Optional aDimStyleName As String = "", Optional bReturnCurrentIfNotFound As Boolean = True) As dxoStyle
            Dim _rVal As dxoStyle = Nothing
            '#1 then desired dimension style to return
            '^returns text style of the requested or current dimension style
            Dim aDimStyle As dxoDimStyle
            aDimStyle = DimStyle(aDimStyleName, bReturnCurrentIfNotFound)
            If aDimStyle IsNot Nothing Then _rVal = TextStyle(aDimStyle.TextStyleName)
            Return _rVal
        End Function
        Public Sub DisplayErrors(Optional aCaption As String = "", Optional bRetainErrors As Boolean = False, Optional aOwner As IWin32Window = Nothing)
            '#1a caption for the error form
            '#2flag to retain the current errors after they are displayed
            '^displays the current errors in a form
            '~the current errors are cleared after they are displayed
            Try
                If ErrorCount <= 0 Then Return
                If Not String.IsNullOrWhiteSpace(aCaption) Then aCaption = Trim(aCaption) Else aCaption = aCaption = Name & " Errors"
                dxfUtils.DisplayErrors(ErrorCol, aCaption, aOwner)
            Catch ex As Exception
            Finally
                If Not bRetainErrors Then ErrorCol = Nothing
            End Try
        End Sub
        Public Function FileExtension(Optional aFileType As dxxFileTypes = dxxFileTypes.Undefined) As String
            '^the filename extension that is used when a request is made to write the file to disk
            If Not dxfEnums.Validate(GetType(dxxFileTypes), aFileType, bSkipNegatives:=True) Then aFileType = FileType
            Return dxfEnums.PropertyName(aFileType).ToLower

        End Function

        Public Function FormatAngle(aNum As Object, Optional aDimStyle As String = "") As String
            '#2the number to format
            '#3the dimstyle to get the formats control from
            '^used to apply the current angle format to the passed number
            If Not TVALUES.IsNumber(aNum) Then Return TVALUES.To_STR(aNum)
            Dim dVal As Double = TVALUES.To_DBL(aNum)
            Return DimStyle(aDimStyle, True).FormatAngle(dVal)
        End Function
        Public Function FormatNumber(aNum As Object, Optional bAddUnitLabel As Boolean = False, Optional bUseInchTicks As Boolean = True, Optional bApplyLinearMultiplier As Boolean = True, Optional aDimStyle As String = "", Optional aPrecision As Integer = -1) As String
            '#1the number to format
            '#2flag to include the units label in the returned string (in. or mm)
            '#3flag to use inch tick marks ('') insted of (in.)
            '#4flag to convert the passed value from english units the the current drawing units
            '^used to apply the current linear format to the passed number
            If Not TVALUES.IsNumber(aNum) Then Return TVALUES.To_STR(aNum)
            Dim dVal As Double = TVALUES.To_DBL(aNum)
            Dim _rVal As String = DimStyle(aDimStyle, True).FormatNumber(dVal, bApplyLinearMultiplier:=bApplyLinearMultiplier, aPrecision:=aPrecision)
            If bAddUnitLabel Then
                If _Structure.set_DIM.Props.ValueI("DrawingUnits") = dxxDrawingUnits.English Then
                    If bUseInchTicks Then
                        _rVal += "''"
                    Else
                        _rVal += " in."
                    End If
                Else
                    _rVal += " mm"
                End If
            End If
            Return _rVal
        End Function
        Public Function GetDisplaySettingsNR(aEntityType As dxxEntityTypes, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLayerName As String = "", Optional aLineType As String = "", Optional aStyleName As String = "") As dxfDisplaySettings
            '#2an overriding linetype layer setting
            '#3an override stylename
            '^returns the display settings that would be applied to a new entity added to the image
            Return GetDisplaySettings(aEntityType:=aEntityType, aLayerName:=aLayerName, aColor:=aColor, aLineType:=aLineType, aLTLFlag:=aLTLFlag, bSuppressed:=False, aStyleName:=aStyleName)
        End Function
        Public Function GetDisplaySettings(aEntityType As dxxEntityTypes, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bSuppressed As Boolean = False, Optional aStyleName As String = "", Optional aSettingsToCopy As dxfDisplaySettings = Nothing) As dxfDisplaySettings
            '#2an overriding layername
            '#3an overriding color
            '#4an overriding linetype
            '#5an overriding linetype layer setting
            '^returns the display settings that would be applied to a new entity added to the image
            Dim _rVal As New dxfDisplaySettings

            'rTextStyle = Nothing
            'rDimStyle = Nothing
            If aLayerName Is Nothing Then aLayerName = ""
            If aLineType Is Nothing Then aLineType = ""
            If aStyleName Is Nothing Then aStyleName = ""
            If aSettingsToCopy IsNot Nothing Then
                If Not String.IsNullOrWhiteSpace(aSettingsToCopy.LayerName) And String.IsNullOrWhiteSpace(aLayerName) Then aLayerName = aSettingsToCopy.LayerName
                If Not String.IsNullOrWhiteSpace(aSettingsToCopy.Linetype) And String.IsNullOrWhiteSpace(aLineType) Then aLineType = aSettingsToCopy.Linetype
                If aEntityType > 0 And aEntityType < dxxEntityTypes.DimLinearH Then
                    If Not String.IsNullOrWhiteSpace(aSettingsToCopy.TextStyleName) And String.IsNullOrWhiteSpace(aStyleName) Then aStyleName = aSettingsToCopy.TextStyleName
                Else
                    If Not String.IsNullOrWhiteSpace(aSettingsToCopy.DimStyleName) And String.IsNullOrWhiteSpace(aStyleName) Then aStyleName = aSettingsToCopy.DimStyleName
                End If
                If aSettingsToCopy.Color <> dxxColors.Undefined Then aColor = aSettingsToCopy.Color

            End If
            Dim tDSP As TDISPLAYVARS
            Dim tSets As TTABLEENTRY = TTABLEENTRY.Null

            Dim aInput As New TDIMINPUT With
            {
            .LayerName = aLayerName.Trim,
            .Color = aColor,
            .Linetype = aLineType.Trim,
            .DimStyleName = aStyleName.Trim
            }
            Dim rTextStyle As dxoStyle = Nothing
            Dim rDimStyle As dxoDimStyle = Nothing
            If aEntityType >= dxxEntityTypes.DimLinearH And aEntityType <= dxxEntityTypes.DimAngular3P Then
                'dimensions
                tDSP = dxfImageTool.DisplayStructure_DIM(Me, False, aInput, rDStyle:=rDimStyle, rTStyle:=rTextStyle)

            ElseIf aEntityType = dxxEntityTypes.LeaderText Or aEntityType = dxxEntityTypes.LeaderTolerance Or aEntityType = dxxEntityTypes.LeaderBlock Or aEntityType = dxxEntityTypes.Leader Then
                'leaders
                tDSP = dxfImageTool.DisplayStructure_DIM(Me, True, aInput, rDStyle:=rDimStyle, rTStyle:=rTextStyle)

            ElseIf aEntityType = dxxEntityTypes.Text Or aEntityType = dxxEntityTypes.Attdef Or aEntityType = dxxEntityTypes.Attribute Or aEntityType = dxxEntityTypes.MText Or aEntityType = dxxEntityTypes.Character Then
                'text
                tDSP = dxfImageTool.DisplayStructure_Text(Me, aLayerName, aColor, aStyleName)
                rTextStyle = New dxoStyle(_Structure.tbl_STYLE.Entry(tDSP.TextStyle))
                tDSP.TextStyle = rTextStyle.Name
            ElseIf aEntityType = dxxEntityTypes.Table Then
                'tables
                tSets = BaseSettings(dxxSettingTypes.TABLESETTINGS)
                tSets.Props.SetVal("TextStyleName", aStyleName)
                tDSP = dxfImageTool.DisplayStructure_Table(Me, tSets)
            ElseIf aEntityType = dxxEntityTypes.Symbol Then
                'symbols
                rTextStyle = Nothing
                tDSP = dxfImageTool.DisplayStructure_Symbol(Me, aStyleName, rSettings:=tSets, aLayerName:="", aColor:=dxxColors.Undefined, rTStyle:=rTextStyle)
            Else
                tDSP = dxfImageTool.DisplayStructure(Me, aLayerName, aColor, aLineType, aLTLFlag)
            End If

            If aSettingsToCopy IsNot Nothing Then
                If aSettingsToCopy.LineWeight <> dxxLineWeights.Undefined Then _rVal.LineWeight = aSettingsToCopy.LineWeight
            End If

            _rVal.Strukture = tDSP
            _rVal.Suppressed = bSuppressed
            Return _rVal
        End Function
        Public Function GetDisplaySettingsWithStyles(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aStyleName As String = "", Optional bSuppressed As Boolean = False) As dxfDisplaySettings
            Dim rTextStyle As dxoStyle = Nothing
            Dim rDimStyle As dxoDimStyle = Nothing
            Return GetDisplaySettingsWithStyles(aEntityType, aLayerName, aColor, aLayerName, aLTLFlag, aStyleName, rTextStyle, rDimStyle, bSuppressed)
        End Function
        Public Function GetDisplaySettingsWithStyles(aEntityType As dxxEntityTypes, aLayerName As String, aColor As dxxColors, aLineType As String, aLTLFlag As dxxLinetypeLayerFlag, aStyleName As String, ByRef rTextStyle As dxoStyle, ByRef rDimStyle As dxoDimStyle, Optional bSuppressed As Boolean = False) As dxfDisplaySettings
            Dim _rVal As New dxfDisplaySettings
            '#2an overriding layername
            '#3an overriding color
            '#4an overriding linetype
            '#5an overriding linetype layer setting
            '^returns the display settings that would be applied to a new entity added to the image
            rTextStyle = Nothing
            rDimStyle = Nothing
            Dim tDSP As TDISPLAYVARS
            Dim tSets As TTABLEENTRY = TTABLEENTRY.Null

            Dim aInput As New TDIMINPUT With
            {
            .LayerName = Trim(aLayerName),
            .Color = aColor,
            .Linetype = Trim(aLineType),
            .DimStyleName = Trim(aStyleName)
            }
            If aEntityType >= dxxEntityTypes.DimLinearH And aEntityType <= dxxEntityTypes.DimAngular3P Then
                'dimensions
                tDSP = dxfImageTool.DisplayStructure_DIM(Me, False, aInput, rDStyle:=rDimStyle, rTStyle:=rTextStyle)

            ElseIf aEntityType = dxxEntityTypes.LeaderText Or aEntityType = dxxEntityTypes.LeaderTolerance Or aEntityType = dxxEntityTypes.LeaderBlock Or aEntityType = dxxEntityTypes.Leader Then
                'leaders
                tDSP = dxfImageTool.DisplayStructure_DIM(Me, False, aInput, rDStyle:=rDimStyle, rTStyle:=rTextStyle)

            ElseIf aEntityType = dxxEntityTypes.Text Or aEntityType = dxxEntityTypes.Attdef Or aEntityType = dxxEntityTypes.Attribute Or aEntityType = dxxEntityTypes.MText Or aEntityType = dxxEntityTypes.Character Then
                'text
                tDSP = dxfImageTool.DisplayStructure_Text(Me, aLayerName, aColor, aStyleName)
                rTextStyle = New dxoStyle(_Structure.tbl_STYLE.Entry(tDSP.TextStyle))
                tDSP.TextStyle = rTextStyle.Name
            ElseIf aEntityType = dxxEntityTypes.Table Then
                'tables
                tSets = BaseSettings(dxxSettingTypes.TABLESETTINGS)
                tSets.Props.SetVal("TextStyleName", aStyleName)
                tDSP = dxfImageTool.DisplayStructure_Table(Me, tSets)
            ElseIf aEntityType = dxxEntityTypes.Symbol Then
                'symbols
                tDSP = dxfImageTool.DisplayStructure_Symbol(Me, aStyleName, rSettings:=tSets, aLayerName:="", aColor:=dxxColors.Undefined, rTStyle:=rTextStyle)
            Else
                tDSP = dxfImageTool.DisplayStructure(Me, aLayerName, aColor, aLineType, aLTLFlag)
            End If
            _rVal.Strukture = tDSP
            _rVal.Suppressed = bSuppressed
            Return _rVal
        End Function

        ''' <summary>
        ''' allows image sub objects and users to raise errors via the image
        ''' </summary>
        ''' <param name="aMethod">the method where the error occured</param>
        ''' <param name="aErrSource">the source of the error (class that threw the error)</param>
        ''' <param name="aException"> the exception</param>
        ''' <returns></returns>
        Public Function HandleError(aMethod As Reflection.MethodBase, aErrSource As Type, aException As Exception) As String
            If aException Is Nothing Or aMethod Is Nothing Or aErrSource Is Nothing Then Return String.Empty

            Return HandleError(aMethod.Name, aErrSource.ToString(), aException.Message, False)
        End Function

        ''' <summary>
        ''' allows image sub objects and users to raise errors via the image
        ''' </summary>
        ''' <param name="aMethod">the method where the error occured</param>
        ''' <param name="aErrSource">the source of the error (class that threw the error)</param>
        ''' <param name="aException"> the exception</param>
        ''' <returns></returns>
        Public Function HandleError(aMethod As Reflection.MethodBase, aErrSource As String, aException As Exception) As String
            If aException Is Nothing Or aMethod Is Nothing Then Return String.Empty

            Return HandleError(aMethod.Name, aErrSource, aException.Message, False)
        End Function

        ''' <summary>
        ''' alows image sub objects and users to raise errors via the image
        ''' </summary>
        ''' <param name="aMethod">the method where the error occured</param>
        ''' <param name="aErrSource">the source of the error (class that threw the error)</param>
        ''' <param name="aException"> the exception</param>
        ''' <returns></returns>
        Public Function HandleError(aMethod As Reflection.MethodBase, aErrSource As String, aException As String) As String
            If String.IsNullOrWhiteSpace(aException) Or aMethod Is Nothing Then Return String.Empty

            Return HandleError(aMethod.Name, aErrSource, aException, False)
        End Function
        Public Function HandleError(aMethod As Reflection.MethodBase, aErrSource As String, aExceptions As List(Of String)) As String ', Optional aErrNumber As Long = 0, Optional bIgnore As Boolean = False, Optional aThrowList As Object = Nothing) As String
            If aExceptions Is Nothing Or aMethod Is Nothing Then Return String.Empty
            '#1the the function which is raising the error
            '#2the name of the object which is raising the error
            '#3the error description
            '^allows image sub objects and users to raise errors via the image
            '~if CollectErrors = True the error is stored in then images error array and can be displayed
            '~using DisplayErrors.
            '~if CollectErrors = False then if RaiseErrors = True the error is raised otherwise
            '~an error message box is displayed
            Dim fname As String = aMethod.Name
            Dim _rVal As String = String.Empty
            For Each estr As String In aExceptions
                If Not String.IsNullOrWhiteSpace(estr) Then
                    _rVal = HandleError(aMethod.Name, aErrSource, estr, False)
                End If
            Next
            Return _rVal
        End Function
        Public Function HandleError(aFunction As String, aErrSource As String, aException As Exception) As String ', Optional aErrNumber As Long = 0, Optional bIgnore As Boolean = False, Optional aThrowList As Object = Nothing) As String
            If aException Is Nothing Then Return String.Empty
            '#1the name of the function which is raising the error
            '#2the name of the object which is raising the error
            '#3the error description
            '^allows image sub objects and users to raise errors via the image
            '~if CollectErrors = True the error is stored in then images error array and can be displayed
            '~using DisplayErrors.
            '~if CollectErrors = False then if RaiseErrors = True the error is raised otherwise
            '~an error message box is displayed
            Return HandleError(aFunction, aErrSource, aException.Message, False)
        End Function
        Public Function HandleError(aFunction As String, aErrSource As String, aErrDescription As String) As String
            '#1the name of the function which is raising the error
            '#2the name of the object which is raising the error
            '#3the error description
            '^allows image sub objects and users to raise errors via the image
            '~if CollectErrors = True the error is stored in then images error array and can be displayed
            '~using DisplayErrors.
            '~if CollectErrors = False then if RaiseErrors = True the error is raised otherwise
            '~an error message box is displayed
            Return HandleError(aFunction, aErrSource, aErrDescription, False)
        End Function

        Public Function HandleError(aFunction As String, aErrSource As String, aErrDescriptions As List(Of String)) As String ', Optional aErrNumber As Long = 0, Optional bIgnore As Boolean = False, Optional aThrowList As Object = Nothing) As String
            '#1the name of the function which is raising the error
            '#2the name of the object which is raising the error
            '#3the error description
            '^allows image sub objects and users to raise errors via the image
            '~if CollectErrors = True the error is stored in then images error array and can be displayed
            '~using DisplayErrors.
            '~if CollectErrors = False then if RaiseErrors = True the error is raised otherwise
            '~an error message box is displayed
            If aErrDescriptions Is Nothing Then Return String.Empty
            Dim _rVal As String = String.Empty
            Dim wuz As Boolean = _Structure.CollectErrors
            _Structure.CollectErrors = True
            For i As Integer = 1 To aErrDescriptions.Count

                _rVal = HandleError(aFunction, aErrSource, aErrDescriptions.Item(i - 1), False)
            Next
            _Structure.CollectErrors = wuz
            Return _rVal
        End Function

        Public Function HandleError(aFunction As String, aErrSource As String, aErrDescription As String, ByRef ioIgnore As Boolean) As String ', Optional aErrNumber As Long = 0, Optional bIgnore As Boolean = False, Optional aThrowList As Object = Nothing, Optional aIgnoreList As Object = Nothing) As String
            If String.IsNullOrWhiteSpace(aErrDescription) Then
                Return String.Empty
            End If
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            'If _rVal = "" Then bIgnore = True
            'If aIgnoreList IsNot Nothing Then
            '    If TLISTS.Contains(aErrNumber, aIgnoreList).ToString() Then bIgnore = True
            'End If
            If Not String.IsNullOrWhiteSpace(aErrSource) Then _rVal += $"[Source: {aErrSource.Trim()} ]"
            If Not String.IsNullOrWhiteSpace(aFunction) Then _rVal += $" [Function: { aFunction.Trim() }]"
            _rVal += $" Error: { aErrDescription.Trim()}"
            If String.IsNullOrWhiteSpace(_rVal) Then Return String.Empty
            Dim iDontLog As Boolean
            RaiseErrorRecieved(aFunction, aErrSource, _rVal, ioIgnore) ', aThrowList, aIgnoreList, iIgnore, iDontLog)
            If ioIgnore Then
                _rVal = ""
                Return _rVal
            End If
            Dim bThrowit As Boolean
            If _Structure.CollectErrors Then
                If Not iDontLog Then _Structure.Errors.Add(_rVal)
                bThrowit = False
            Else
                bThrowit = _Structure.RaiseErrors
            End If
            'If aThrowList IsNot Nothing Then
            '    If TLISTS.Contains(aErrNumber, aThrowList.ToString()) Then bThrowit = True
            'End If
            If Not bThrowit Then
                If Not _Structure.CollectErrors Then
                    Forms.MessageBox.Show(text:=$"Image Event Error:{ _rVal}", caption:="dxfImage Error", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Exclamation)
                End If
            Else
                Throw New Exception(_rVal)
            End If
            Return _rVal
        End Function
        Public Function LabelPathVectors(Optional aEntity As dxfEntity = Nothing, Optional aTextScaleFactor As Double = 0.0, Optional aColor As dxxColors = dxxColors.Blue, Optional bSaveToFile As Boolean = False) As Long
            '#1the subject entity
            '#2a scale factor to apply to the labels text height
            '#3a color to apply to the labels
            '#4flag to mark the labels to be saved to file if the image is saved
            '^labels the vectors that define the screen path of the subject entity
            Return dxfImageTool.LabelPathVectors(Me, aEntity, aTextScaleFactor, aColor, bSaveToFile)
        End Function



        Public Sub LayerColorAndLineType_Set(Optional aLayer As String = Nothing, Optional aColor As dxxColors? = Nothing, Optional aLineType As String = Nothing, Optional bAddNewLayers As Boolean = True)
            '#1the Layer name to use
            '#2the Color to use
            '#3the LineType to use
            '#4flag to either set the layer, color and linetype or get the layer, color and linetype
            '#5flag to add a new layer if the passed layer doesn't already exist
            '^shorthand method to set a dxoDrawingTool objects Layer, Color and Linetype Properties
            Dim aHdr As dxsHeader = Header

            If Not String.IsNullOrWhiteSpace(aLayer) Then
                aLayer = GetOrAdd(dxxReferenceTypes.LAYER, aLayer.Trim(), bSuppressNew:=Not bAddNewLayers)
                aHdr.LayerName = aLayer
            End If
            If aColor.HasValue Then
                If aColor.Value <> dxxColors.Undefined Then
                    aHdr.Color = aColor
                End If

            End If
            If Not String.IsNullOrWhiteSpace(aLineType) Then
                aLineType = GetOrAdd(dxxReferenceTypes.LTYPE, aLineType.Trim())
                If Not String.IsNullOrWhiteSpace(aLineType) Then aHdr.Linetype = aLineType
            End If


        End Sub

        Public Function Layout(Optional aName As String = "") As dxfObject
            If String.IsNullOrWhiteSpace(aName) Then aName = "Model" Else aName = aName.Trim()
            Return Objex.GetObject(aName, dxxObjectTypes.Layout)
        End Function
        Public Function OpenInACAD(Optional aFileType As dxxFileTypes = dxxFileTypes.Undefined, Optional aFileName As String = "", Optional aCallingForm As Long = 0, Optional bSuppressGUI As Boolean = False, Optional bShowWorking As Boolean = True, Optional bAllowVersionChange As Boolean = True, Optional aAutoCADtoUse As String = "", Optional aAttributes As dxfAttributes = Nothing, Optional aBlockNames As String = "", Optional aSuppressAttributes As Boolean = False) As Boolean
            Dim rCanceled As Boolean = False
            Return OpenInACAD(aFileType, aFileName, aCallingForm, bSuppressGUI, bShowWorking, bAllowVersionChange, aAutoCADtoUse, aAttributes, aBlockNames, aSuppressAttributes, rCanceled)
        End Function
        Public Function OpenInACAD(aFileType As dxxFileTypes, aFileName As String, aCallingForm As Long, bSuppressGUI As Boolean, bShowWorking As Boolean, bAllowVersionChange As Boolean, aAutoCADtoUse As String, aAttributes As dxfAttributes, aBlockNames As String, aSuppressAttributes As Boolean, ByRef rCanceled As Boolean) As Boolean
            '#1the file type to write (DWG or DWF)
            '#2a reference to the calling form
            '#3the file name to write to
            '#4flag to show or bypass the Working screen shown while the DXF file is written and openened in AutoCAD
            '#5flag to control if the client can change the DXF file version
            '#6optional string to tell which AutoCAD version to start
            '#7a collection of attributes
            '^used to open a DXF File in an AutoCAD session
            '~an Error is raised if the host system doesn't have AutoCAD installed
            Dim sErrStr As String = String.Empty
            Dim errNo As Long
            Dim _rVal As Boolean = dxfAcad.OpenInACAD(Me, aFileType, aFileName, aCallingForm, bSuppressGUI, bShowWorking, bAllowVersionChange, aAutoCADtoUse, aAttributes, aBlockNames, aSuppressAttributes, sErrStr, errNo, rCanceled)
            If sErrStr <> "" Then
                If Not bSuppressGUI Then
                    If errNo <> 1001 Then
                        Forms.MessageBox.Show($"An Error Occured Opening '{ FileName(aFileType) }' In AutoCAD{ vbLf }{ vbLf}ERROR: { sErrStr}", "ERROR", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Exclamation)
                    Else
                        Forms.MessageBox.Show($"File Write Canceled", "Cancel Selected", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Information)


                    End If
                Else
                    Throw New Exception("dxfImage.OpenInACAD - " & sErrStr)
                End If
            End If
            Return _rVal
        End Function
        Public Function Purge(Optional bAll As Boolean = False, Optional bBlocks As Boolean = False, Optional bLayers As Boolean = False, Optional bLineTypes As Boolean = False, Optional bStyles As Boolean = False, Optional bDimStyles As Boolean = False) As Long
            '#1Flag to purge all
            '#2Flag to purge blocks
            '#3Flag to purge layers
            '#4Flag to purge linetypes
            '#5Flag to purge styles
            '#6Flag to purge dimstyles
            '^removes unreferenced objects from the image
            Return dxfImageTool.Purge(Me, bAll, bBlocks, bLayers, bLineTypes, bStyles, bDimStyles)
        End Function
        Public Function ReadFromFile(aFileSpec As String, Optional aCallingForm As Long = 0, Optional bShowGUI As Boolean = True, Optional aSuppressString As String = "") As Boolean
            Dim _rVal As Boolean = False
            If String.IsNullOrWhiteSpace(aFileSpec) Then Return _rVal
            aFileSpec = aFileSpec.Trim()

            Dim file As New dxfFile()

            Dim aDisplay As TDISPLAY = obj_DISPLAY
            '#1the path to the dxf file to read
            '#2the calling form
            '#3flag to show the file selection box
            '^reads the passed DXF file and sets the files properties to those read in
            Try


                '**UNUSED VAR** Dim bRedrawWuz As Boolean
                If bShowGUI Then
                    If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
                    'Dim aFrm As frmReadWrite
                    'aFrm = New frmReadWrite
                    'Dim bCanc As Boolean
                    '_rVal = aFrm.ShowRead(aCallingForm, Me, aFileSpec, bCanc)
                    'aFrm = Nothing
                Else

                    _Structure.AutoRedraw = False
                    file = New dxfFile(aFileSpec)
                    file.ReadToImage(aFileSpec, aImage:=Me)

                End If

            Catch ex As Exception
                file.ErrorStrings.Add(ex.Message)

                Reset()
            Finally
                If file.ErrorStrings.Count > 0 Then HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfImage", file.ErrorStrings)
                _Structure.AutoRedraw = aDisplay.AutoRedraw
            End Try
            Return _rVal
        End Function
        Public Function ReferenceExists(aRefType As dxxReferenceTypes, aName As String) As Boolean
            '#1the type of reference object to look for
            '#2the name of the refrence to look for
            '^returns true if the passed name exists in the refrenced table
            Return Tables.ReferenceExists(aRefType, aName)
        End Function
        Public Sub ResetErrors()
            _Structure.Errors = New TVALUES(0)
        End Sub
        Public Function SaveEntity(Optional aDXFEntity As dxfEntity = Nothing, Optional aEntCol As colDXFEntities = Nothing, Optional bEntCol As List(Of dxfEntity) = Nothing, Optional aGroupName As String = Nothing, Optional bSuppressFromFile As Boolean? = Nothing, Optional aTag As String = Nothing) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '#1a single dxf entity to add to the current image
            '#2a dxf collection of entities to add to the current image
            '#3a vb collection of entities to add to the current image
            '#4an override group name to apply to the entity
            '#5a flag to mark the new entities for suppression from the file when the image is written to file
            '^used to save an entity to the current image
            Dim aEnt As dxfEntity

            Dim etag As String = String.Empty
            Dim bsv2file As Boolean
            Dim gnamewuz As String = String.Empty

            If aTag IsNot Nothing Then etag = aTag

            Try
                If bSuppressFromFile Is Nothing Then
                    bsv2file = Not _Structure.SuppressFromFile
                Else
                    bsv2file = IIf(bSuppressFromFile.HasValue, Not bSuppressFromFile.Value, Not _Structure.SuppressFromFile)
                End If
                If aGroupName IsNot Nothing Then
                    gnamewuz = GroupName
                    If Not String.IsNullOrWhiteSpace(aGroupName) Then GroupName = aGroupName.Trim()
                End If
                If aDXFEntity IsNot Nothing Then
                    aEnt = aDXFEntity
                    If aDXFEntity.ImageGUID = GUID Then
                        If aDXFEntity.Handle <> "" Then aEnt = aDXFEntity.Clone
                    End If
                    If etag <> "" Then aEnt.Tag = etag
                    _rVal = Entities.Add(aEnt)
                    _rVal.SaveToFile = bsv2file
                End If
                If aEntCol IsNot Nothing Then
                    For i As Integer = 1 To aEntCol.Count
                        aEnt = aEntCol.Item(i)
                        If aEnt.ImageGUID = GUID Then
                            If aEnt.Handle <> "" Then aEnt = aEnt.Clone
                        End If
                        If etag <> "" Then aEnt.Tag = etag
                        _rVal = Entities.Add(aEnt)
                        _rVal.SaveToFile = bsv2file
                    Next i
                End If
                If bEntCol IsNot Nothing Then
                    For i = 1 To bEntCol.Count
                        aEnt = bEntCol.Item(i - 1)
                        If aEnt.ImageGUID = GUID Then
                            If aEnt.Handle <> "" Then aEnt = aEnt.Clone
                        End If
                        If etag <> "" Then aEnt.Tag = etag
                        _rVal = Entities.Add(aEnt)
                        _rVal.SaveToFile = bsv2file
                    Next i
                End If
                If aGroupName IsNot Nothing Then GroupName = gnamewuz
                aDXFEntity = _rVal
                Return _rVal
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
                Return _rVal
            End Try
        End Function
        Public Function SaveToFile(aSuppressUI As Boolean, Optional aFileName As String = "", Optional aVersion As dxxACADVersions = dxxACADVersions.DefaultVersion, Optional aFileType As dxxFileTypes = dxxFileTypes.Undefined, Optional aCallingForm As Long = 0, Optional bConfirmOverwrite As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1flag to show the file writing form
            '#2a filename to use (if not passed the current file name is used)
            '#3the AutoCAD version to save the File as
            '#4the type of file to write
            '#5handle to the calling form
            '#6flag to confirm prior to overwriting an existing file
            '^prompts the user for a file name and file and writes the file to disk
            '^in the requested version format.
            '~returns True if no write errors were encountered
            Dim aExt As String = String.Empty
            Dim fname As String = String.Empty
            Dim aErrStr As String = String.Empty
            If aFileName <> "" Then FileName_Set(aFileName)
            If Not aSuppressUI Then
                If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
                'Dim aFrm As frmReadWrite
                'aFrm = New frmReadWrite
                '_rVal = aFrm.ShowWrite(aCallingForm, Me, aVersion, bConfirmOverwrite, aFileType)
                'aFrm = Nothing
            Else
                Try
                    If aFileType <> dxxFileTypes.DWG And aFileType <> dxxFileTypes.DXF Then aFileType = dxxFileTypes.DWG
                    'If Not goACAD.ConverterPresent And aFileType <> dxxFileTypes.DXT Then aFileType = dxxFileTypes.DXF
                    aExt = dxfEnums.PropertyName(aFileType).ToLower

                    fname = FileName(aFileType)
                    If fname = "" Then Throw New Exception("A File Name Must Be Assigned Before The File Can Be Written.")
                    fname = WriteToFile(aFileType, fname, aVersion, False, True, aErrStr)
                    If aErrStr <> "" Then Throw New Exception(aErrStr)
                    RaiseEvent SaveToFileEvent(fname)
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine(ex.Message)
                    Forms.MessageBox.Show($"Error Encounter Saving { aExt } File '{aFileName }' ERROR - { ex.Message}", $"{aExt} Save Error", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Exclamation)
                End Try
            End If
            _rVal = IO.File.Exists(fname)
            Return _rVal
        End Function

        Public Sub SetFeatureScales(aPaperScale As Double, Optional bSetDimStyle As Boolean = True, Optional bSetTextHeight As Boolean = True)
            '#1the subject image
            '#2the feature scale to apply
            '#3flag to pass the feature scale to all the current dim styles
            '#4flag to pass the feature scale to all the current text styles
            '^assigns the passed feature scale to all the scaled settings objects
            Dim SF As Double

            Dim aVal As Object = Nothing
            Dim sVal As Double
            Dim aTable As dxfTable
            Dim aDsp As TDISPLAY = obj_DISPLAY
            Dim pname As String = String.Empty
            Dim oldVal As Double
            Dim lastVal As Double
            SF = Math.Abs(aPaperScale)
            If SF = 0 Then
                If aDsp.ZoomFactor = 0 Then SF = 1 / aDsp.ZoomFactor
            End If
            If SF = 0 Then Return
            aDsp.PaperScale = SF
            obj_DISPLAY = aDsp
            oldVal = _Structure.set_TABLE.Props.ValueD("FeatureScale")
            'settings
            If _Structure.set_TABLE.Props.SetVal("FeatureScale", SF) Then
                RespondToSettingChange(TableSettings, _Structure.set_TABLE.Props.Item("FeatureScale"))
            End If
            If _Structure.set_SYMBOL.Props.SetVal("FeatureScale", SF) Then
                RespondToSettingChange(SymbolSettings, _Structure.set_SYMBOL.Props.Item("FeatureScale"))
            End If
            Header.LineTypeScale = SF * 0.25
            DimStyleOverrides.FeatureScaleFactor = SF

            aTable = DimStyles
            For Each dstyle In aTable
                Dim featr As dxoProperty = dstyle.PropByEnum(dxxDimStyleProperties.DIMSCALE)
                If featr.SetVal(SF) Then
                    RaiseTableMemberEvent(dstyle, featr)
                End If
            Next

            If bSetTextHeight And SF <> 1 Then
                lastVal = Header.TextSize / oldVal
                sVal = SF * lastVal
                Header.TextSize = sVal

                aTable = Styles
                For Each style In aTable
                    Dim tht As dxoProperty = style.PropByEnum(dxxStyleProperties.TEXTHT)
                    lastVal = tht.ValueD / oldVal
                    If lastVal > 0 Then
                        sVal = SF * lastVal
                        If tht.SetVal(sVal) Then
                            RaiseTableMemberEvent(style, tht)
                        End If
                    End If

                Next


            End If
        End Sub
        Public Function ScreenTextRectangle(aString As String, Optional aTextHeight As Double = 0.0, Optional aStyleName As String = "", Optional aDirectionFlag As dxxTextDrawingDirections = dxxTextDrawingDirections.ByStyle, Optional aWidthFactor As Double = 0.0, Optional aAngle As Double = 0.0, Optional aObliqueAngle As Object = Nothing) As dxfRectangle
            '^returns the bounding rectangle of the passsed string
            'On Error Resume Next
            Dim aSty As dxoStyle = TextStyle(aStyleName)
            If aSty Is Nothing Then aSty = TextStyle()
            Return aSty.CreateText(aString, dxxTextTypes.Multiline, aDirectionFlag, aTextHeight, aWidthFactor:=aWidthFactor, aAngle:=aAngle, aObliqueAngle:=aObliqueAngle, aImage:=Me).BoundingRectangle
        End Function
        Public Function SelectionSet(Optional aSelectionType As dxxSelectionTypes = dxxSelectionTypes.CurrentSet, Optional aSelectionCriteria As Object = Nothing, Optional aSelectType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGetClones As Boolean = False, Optional aRemove As Boolean = False) As colDXFEntities
            '#1the type of selection set to return
            '#2an optional value to use in conjuction with the selection type
            '#3an entity type filter value
            '#4flag to return clones of the set rather than the actual enities
            '#4flag to remove the set from the current image
            '^provides extended methods for obtaining subsets of the images current entities collection
            Return dxfImageTool.SelectionSet(Me, aSelectionType, aSelectionCriteria, aSelectType, aGetClones, aRemove)
        End Function
        Public Function SelectionSetInit(aEnd As Boolean, Optional bMaintainStartPoint As Boolean = False, Optional aGetClones As Boolean = False, Optional aRemove As Boolean = False) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '#1flag indicating if the call is to begin or end the current select set
            '#2flag to to leave the starting pointer where it is rather than moving it to the end of the current entities collection (valid if Ending)
            '#3flag to remove the return a clone of the current selection set (valid if Ending)
            '#4flag to remove the current selections set fron the current entitites collection (valid if Ending)
            '^used to start or end the current selection set. If ending then current selections set is returned
            '~moves the current selection set starting point to the end of the current entities collection
            If aEnd Then
                _rVal = SelectionSet(dxxSelectionTypes.CurrentSet, aGetClones:=aGetClones, aRemove:=aRemove)
                If Not bMaintainStartPoint Or aRemove Then SelectionStartID = Entities.Count + 1
            Else
                SelectionStartID = Entities.Count + 1
            End If
            Return _rVal
        End Function
        Public Sub SendToPrinter(Optional aShowDialogue As Boolean = True, Optional aPaperOrientation As dxxPaperOrientations = dxxPaperOrientations.ByAspect, Optional aColorMode As dxxColorModes = dxxColorModes.ByImage, Optional aScaleMode As dxxPrinterScaleModes = dxxPrinterScaleModes.CurrentView)
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            '#1flag to show the printer selection dialogue
            '#2indicates the desired paper orientation
            '#3the desired color mode
            '^intiates a print job based ont the current image
            '     On Error GoT2 Err:
            '     Dim eStr As String
            '     If Printers.Count = 0 Then
            '         Throw New Exception( "No System Printers Detected.")
            '         Return
            '     End If
            '     Load(frmPrint)
            '     frmPrint.ShowPrintImage(Me, aShowDialogue, aPaperOrientation, aColorMode, aScaleMode)
            '     frmPrint = Nothing
            '     Return
            'Err:
            '     eStr = Ex.Message
            '     frmPrint = Nothing
            '     If aShowDialogue Then
            '   MessageBox.Show($"Printer Error -  { eStr}", "Printer Error", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Exclamation)
            '     Else
            '         Throw New Exception( "dxfImage.SendToPrinter - " & eStr)
            '     End If
        End Sub
        Public Function SetTextStyleProperty(aStyleName As String, aProperty As String, aValue As Object) As Boolean
            Dim _rVal As Boolean
            '#1the style to set a property for
            '#2the name of the property to set
            '#3the new value for the property
            '^used to set the text height,font name or width factor of a text style
            Try
                _rVal = dxfImageTool.SetTextStyleProperty(Me, aStyleName, aProperty, aValue)
            Catch ex As Exception
                _rVal = False
                Forms.MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name} -  { ex.Message}", "Text Style Property Error", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Exclamation)

            End Try
            Return _rVal
        End Function


        Public Function ShowFormatForm(aOwner As IWin32Window, aReferenceType As dxxReferenceTypes) As Boolean
            Dim _rVal As Boolean = False
            Dim wuz As Boolean = _Structure.obj_DISPLAY.AutoRedraw
            Dim bRedraw As Boolean
            Dim tbleForm = New frmTables
            _Structure.AutoRedraw = False
            Try
                If aReferenceType = dxxReferenceTypes.LAYER Then
                    _rVal = tbleForm.ShowFormat(aOwner, aReferenceType, Me, True, bRedraw)
                ElseIf aReferenceType = dxxReferenceTypes.LTYPE Then
                    '         lForm = New frmEdit_LTYPE
                    '         _rVal = lForm.ShowFormat(Me)
                    '         lForm = Nothing
                ElseIf aReferenceType = dxxReferenceTypes.STYLE Then
                    '         aCap = "Style Formating"
                    '         lForm = New frmEdit_STYLE
                    '         _rVal = lForm.ShowFormat(Me)
                    '         lForm = Nothing
                    '         If _rVal Then
                    '             zAddToRegenList(dxxGraphicTypes.Text)
                    '             bRedraw = True
                    '         End If
                End If
                Return _rVal
                Return _rVal
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
                Return _rVal
            Finally
                tbleForm.Dispose()
                _Structure.AutoRedraw = wuz
                If bRedraw Then Render()
            End Try
            Return _rVal
        End Function
        Public Function ShowSelectForm(aOwner As IWin32Window, aReferenceType As dxxReferenceTypes, Optional aInitName As String = "", Optional bComboStyle As Boolean = False, Optional bAllowLogicals As Boolean = False, Optional bAllowAdd As Boolean = False) As String
            Dim _rVal As String = String.Empty
            Dim lForm As idxfReferenceForm = Nothing
            Try
                If aReferenceType = dxxReferenceTypes.LAYER Then
                    Dim tblForm = New frmTables
                    _rVal = tblForm.ShowSelect(aOwner, dxxReferenceTypes.LAYER, Me, aInitName, bComboStyle)
                ElseIf aReferenceType = dxxReferenceTypes.LTYPE Then
                    Dim tblForm = New frmTables
                    _rVal = tblForm.ShowSelect(aOwner, dxxReferenceTypes.LTYPE, Me, aInitName, bComboStyle, bAllowLogicals, False)
                ElseIf aReferenceType = dxxReferenceTypes.STYLE Then
                    Dim tblForm = New frmTables
                    _rVal = tblForm.ShowSelect(aOwner, dxxReferenceTypes.STYLE, Me, aInitName, bComboStyle, bAllowLogicals, False)
                End If
                Return _rVal
                Return _rVal
            Catch ex As Exception
                HandleError(Reflection.MethodBase.GetCurrentMethod.Name, "dxfImage", ex.Message)
            Finally
                If lForm IsNot Nothing Then lForm.Dispose()
            End Try
            '     'On Error Resume Next
            '     Dim lForm As idxfReferenceForm
            '     Dim eStr As String
            '     Dim aCap As String
            '     If aReferenceType = dxxReferenceTypes.LAYER Then
            '         aCap = "Layer Selecting"
            '         lForm = New frmEdit_LAYERS
            '         _rVal = lForm.ShowSelect(Me, aInitName, bAllowLogicals, bAllowAdd)
            '         lForm = Nothing
            '     ElseIf aReferenceType = dxxReferenceTypes.LTYPE Then
            '         aCap = "Linetype Selecting"
            '         lForm = New frmEdit_LTYPE
            '         _rVal = lForm.ShowSelect(Me, aInitName, bAllowLogicals, bAllowAdd)
            '         lForm = Nothing
            '     ElseIf aReferenceType = dxxReferenceTypes.STYLE Then
            '         aCap = "Style Selecting"
            '         lForm = New frmEdit_STYLE
            '         _rVal = lForm.ShowSelect(Me, aInitName, bAllowLogicals, bAllowAdd)
            '         lForm = Nothing
            '     End If
            '     Return _rVal
            'Err:
            '     eStr = Ex.Message
            'MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name} -  { ex.Message}", "Unexpected Error EnCounted", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Exclamation)

            Return _rVal
        End Function
        Public Function TextLineSpacing(Optional aStyleName As String = "", Optional aTextHeight As Double = 0.0) As Double
            '^the distance being used to seperate lines of text if there are multiple lines
            '~this value is in drawing inches
            Dim aText As dxeText = EntityTool.Create_Text(New dxfVector, "XXXX", aTextHeight, aStyleName:=aStyleName)
            Return aText.LineStep
        End Function

        Friend Sub VerifyDefaultMembers()
            dxfImageTool.VerifyDefaultMembers(Me)
        End Sub
        Public Function WriteToFile(aFileType As dxxFileTypes, aFileSpec As String, aVersion As dxxACADVersions, bSuppressDimOverrides As Boolean, bNoErrors As Boolean, Optional bSuppressDXTNames As Boolean = False, Optional bNumericHandles As Boolean = False) As String
            Dim rErrString As String = String.Empty
            Return WriteToFile(aFileType, aFileSpec, aVersion, bSuppressDimOverrides, bNoErrors, rErrString, bSuppressDXTNames, bNumericHandles)
        End Function
        Public Function WriteToFile(aFileType As dxxFileTypes, aFileSpec As String, aVersion As dxxACADVersions, bSuppressDimOverrides As Boolean, bNoErrors As Boolean, ByRef rErrString As String, Optional bSuppressDXTNames As Boolean = False, Optional bNumericHandles As Boolean = False) As String
            '#1the type of file to write
            '#2the filename to write to
            '#3the AutCAD version to write the DWG to
            '^used to write the current file to disk
            '~returns True if the file was successfully written.
            '~all errors are raised to the caller.
            Return WriteToFile(Nothing, aFileType, aFileSpec, aVersion, bSuppressDimOverrides, bNoErrors, rErrString:=rErrString, bSuppressDXTNames:=bSuppressDXTNames, bNumericHandles:=bNumericHandles)
        End Function

        Friend Function Table(aTableType As dxxReferenceTypes) As dxfTable
            Return Tables.Table(aTableType, Me)
        End Function
        Friend Function TableEntry(aTableType As dxxReferenceTypes, aEntryName As String) As dxfTableEntry
            Return Tables.Entry(aTableType, aEntryName, Me)
        End Function

        Friend Function Settings_Object(aSettingsType As dxxReferenceTypes) As dxfSettingObject
            Return Settings_Objects.Setting(aSettingsType, Me)
        End Function

#End Region 'Methods
#Region "GetOrAdd"
        '^used to get or add references.
        '~returns the new or existing entry
        '~Only Layer, Linetype, Style and DimStyle reference types are supported
        Public Function GetOrAddReference(aName As String, aRefType As dxxReferenceTypes, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLineWeigth As dxxLineWeights = dxxLineWeights.Undefined, Optional bSuppressNew As Boolean = False) As dxfTableEntry
            If (aRefType <> dxxReferenceTypes.LAYER And
                aRefType <> dxxReferenceTypes.LTYPE And
                aRefType <> dxxReferenceTypes.STYLE And
                aRefType <> dxxReferenceTypes.DIMSTYLE) Then
                Return Nothing
            End If
            Dim rFound As Boolean = False
            Dim rAdded As Boolean = False
            Dim newEntry As dxfTableEntry = Nothing
            aName = GetOrAdd(aRefType, aName, newEntry, aColor, aLineType, aLineWeigth, bSuppressNew:=False, bSuppressEvnt:=False, rFound, rAdded)
            If rFound Or rAdded Then Return newEntry Else Return Nothing
        End Function
        '^used to get or add references.
        '~returns the new or existing entry
        '~Only Layer, Linetype, Style and DimStyle reference types are supported
        Public Function GetOrAdd(aRefType As dxxReferenceTypes, aName As String) As String
            Dim rEntry As dxfTableEntry = Nothing
            Dim rFound As Boolean = False
            Dim rAdded As Boolean = False
            Return GetOrAdd(aRefType, aName, rEntry, dxxColors.Undefined, "", dxxLineWeights.Undefined, False, False, rFound, rAdded)
        End Function
        '^used to get or add references.
        '~returns the new or existing entry
        '~Only Layer, Linetype, Style and DimStyle reference types are supported
        Friend Function GetOrAdd(aRefType As dxxReferenceTypes, aName As String, ByRef rEntry As dxfTableEntry) As String
            Dim rFound As Boolean = False
            Dim rAdded As Boolean = False
            Return GetOrAdd(aRefType, aName, rEntry, dxxColors.Undefined, "", dxxLineWeights.Undefined, False, False, rFound, rAdded)
        End Function
        '^used to get or add references.
        '~returns the new or existing entry
        '~Only Layer, Linetype, Style and DimStyle reference types are supported
        Friend Function GetOrAdd(aRefType As dxxReferenceTypes, aName As String, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLineWeigth As dxxLineWeights = dxxLineWeights.Undefined, Optional bSuppressNew As Boolean = False, Optional bSuppressEvnt As Boolean = False) As String
            Dim rEntry As dxfTableEntry = Nothing
            Dim rFound As Boolean = False
            Dim rAdded As Boolean = False
            Return GetOrAdd(aRefType, aName, rEntry, aColor, aLineType, aLineWeigth, bSuppressNew, bSuppressEvnt, rFound, rAdded)
        End Function
        '^used to get or add references.
        '~returns the new or existing entry
        '~Only Layer, Linetype, Style and DimStyle reference types are supported
        Friend Function GetOrAdd(aRefType As dxxReferenceTypes, ByRef ioName As String, ByRef rEntry As dxfTableEntry, aColor As dxxColors, aLineType As String, aLineWeigth As dxxLineWeights, bSuppressNew As Boolean, bSuppressEvnt As Boolean, ByRef rFound As Boolean, ByRef rAdded As Boolean) As String
            rAdded = False
            rFound = False
            If (aRefType <> dxxReferenceTypes.LAYER And
                aRefType <> dxxReferenceTypes.LTYPE And
                aRefType <> dxxReferenceTypes.STYLE And
                aRefType <> dxxReferenceTypes.DIMSTYLE) Then
                Return String.Empty
            End If
            rEntry = Nothing
            'get the table by refrence type
            Dim baseTBL As dxfTable = Nothing
            If Not Tables.TryGetTable(aRefType, baseTBL, True) Then Return String.Empty

            'bail if the table is found
            If String.IsNullOrWhiteSpace(ioName) Then ioName = "" Else ioName = ioName.Trim()

            'set the default if no name was passed
            If ioName = "" Then
                Dim hpname As String = String.Empty
                Dim defname As String = String.Empty
                Dim hpenum As dxxHeaderVars? = Nothing
                Select Case aRefType
                    Case dxxReferenceTypes.LAYER
                        hpname = "$CLAYER" : defname = "0" : hpenum = dxxHeaderVars.CLAYER
                    Case dxxReferenceTypes.DIMSTYLE
                        hpname = "$DIMSTYLE" : defname = "Standard" : hpenum = dxxHeaderVars.DIMSTYLE
                    Case dxxReferenceTypes.LTYPE
                        hpname = "$CELTYPE" : defname = dxfLinetypes.ByLayer : hpenum = dxxHeaderVars.CELTYPE
                    Case dxxReferenceTypes.STYLE
                        hpname = "$TEXTSTYLE" : defname = "Standard" : hpenum = dxxHeaderVars.TEXTSTYLE

                End Select
                If hpenum.HasValue Then ioName = Header.PropValueStr(hpenum.Value, aDefault:=defname, bReturnDefaultForNullString:=True)
            End If
            'see if the requested name is a member of the table
            rFound = baseTBL.TryGetEntry(ioName, rEntry:=rEntry)
            If rFound Then
                'the requested name was found in the table
                ioName = rEntry.Name
            Else
                If bSuppressNew Then
                    Return String.Empty
                Else
                    'create the new entry
                    rEntry = dxfTableEntry.CreateNewReference(aRefType, ioName)
                    If (rEntry.Properties.Count <= 0) Then
                        rEntry = Nothing
                        Return String.Empty
                    End If
                End If


            End If
            Dim aNewEntry As dxfTableEntry = Nothing
            Select Case aRefType
                Case dxxReferenceTypes.LAYER
#Region "LAYER"

                    Dim prop As dxoProperty = Nothing
                    ' Dim bUpdate As Boolean
                    'set some props if they were passed
                    If aColor <> dxxColors.Undefined Then
                        If rEntry.PropValueSet(dxxLayerProperties.Color, aColor) Then
                            'bUpdate = True
                        End If
                    End If
                    If aLineType <> "" Then
                        If String.Compare(aLineType, rEntry.PropValueStr(dxxLayerProperties.Linetype), True) <> 0 Then
                            aLineType = GetOrAdd(dxxReferenceTypes.LTYPE, aLineType)
                            If rEntry.PropValueSet(dxxLayerProperties.Linetype, aLineType) Then
                                'bUpdate = True
                            End If
                        End If
                    End If
                    If Not rFound Then
                        'add this one unplottable and hidden
                        If String.Compare(ioName, "DefPoints", ignoreCase:=True) = 0 Then
                            rEntry.PropValueSet(dxxLayerProperties.PlotFlag, False)
                            rEntry.PropValueSet(dxxLayerProperties.Visible, False)
                        Else
                            Dim ltlayers As dxsLinetypes = LinetypeLayers
                            If ltlayers.Count > 0 Then
                                prop = ltlayers.Properties.GetByPropertyValue(ioName, prop, True)
                                If prop IsNot Nothing Then
                                    aLineType = GetOrAdd(dxxReferenceTypes.LTYPE, prop.Name)
                                    rEntry.PropValueSet(dxxLayerProperties.Linetype, aLineType)

                                End If
                            End If

                        End If

                    End If
#End Region 'LAYER
                Case dxxReferenceTypes.DIMSTYLE
#Region "DIMSTYLE"
                    'update the dimstyles references
                    rEntry.UpdateProperties(Me)
                    'create the new entry

#End Region 'DIMSTYLE
                Case dxxReferenceTypes.STYLE
#Region "STYLE"
                    'create the new entry
                    If Not rFound Then aNewEntry = New dxoStyle(rEntry)
#End Region 'STYLE
                Case dxxReferenceTypes.LTYPE
#Region "LTYPE"
                    'see if the linetype is loaded in the images linetype table
                    If Not rFound Then
                        'check the default names that should be in the table
                        rEntry = dxfLinetypes.DefaultDef(ioName, bNoLogicals:=True)

                    End If
#End Region 'LTYPE
                Case Else
                    'these are the only reference types that are supported by this function
                    Return String.Empty
            End Select
            If Not rFound Then
                baseTBL.Add(rEntry)
                rAdded = True
                If rEntry IsNot Nothing And Not bSuppressEvnt Then
                    Dim aErr As String = String.Empty
                    'to assign handles
                    RespondToTableEvent(baseTBL, dxxCollectionEventTypes.Add, rEntry, False, False, aErr)
                End If
            End If

            Return rEntry.Name
        End Function
#End Region 'GetorAdd
#Region "_Events_EventHandlers"
        'Private Sub _Events_ImageRequest(aGUID As String, ByRef rImage As dxfImage)
        '    If aGUID = GUID And Not Disposed Then rImage = Me
        'End Sub
#End Region '_Events_EventHandlers
#Region "IDisposable Implementation"
        'Protected Overrides Sub Finalize()
        '    MyBase.Finalize()
        'End Sub
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _Disposed Then
                Try
                    If disposing Then
                        '  _Structure.Destroy()
                        'If _Events IsNOt Nothing Then RemoveHandler _Events.ImageRequest, AddressOf _Events_ImageRequest
                        If _Screen IsNot Nothing Then _Screen.Dispose()
                        If _HandleGenerator IsNot Nothing Then _HandleGenerator.Dispose()
                        If _Entities IsNot Nothing Then _Entities.Dispose()
                        If _Blocks IsNot Nothing Then _Blocks.Dispose()
                        If _Bitmap IsNot Nothing Then _Bitmap.Dispose()
                        If _ScreenBitmap IsNot Nothing Then _ScreenBitmap.Dispose()
                        If _Objects IsNot Nothing Then _Objects.Dispose()
                        If _Tables IsNot Nothing Then _Tables.Dispose()
                        If _Settings IsNot Nothing Then _Settings.Dispose()
                    End If

                Catch ex As Exception
                Finally
                    '_Events = Nothing
                    _Screen = Nothing
                    _Structure = New TIMAGE("")
                    _HandleGenerator = Nothing
                    _Entities = Nothing
                    _Blocks = Nothing
                    _Bitmap = Nothing
                    _ScreenBitmap = Nothing
                    _Objects = Nothing
                    _Tables = Nothing
                    _Settings = Nothing
                    ' free unmanaged resources (unmanaged objects) and override finalizer
                    ' set large fields to null
                    _Disposed = True
                    dxfGlobals.Aggregator.Unsubscribe(Me)
                    'GC.Collect()
                End Try
            End If
        End Sub
        Protected Overrides Sub Finalize()
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=False)
            MyBase.Finalize()
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region 'IDisposable Implementation
#Region "IEventSubScriber Implementation"
        Protected Friend Sub OnAggregateEvent(message As Object) Implements IEventSubscriber(Of Message_ImageRequest).OnAggregateEvent
            Dim msg As Message_ImageRequest = message ' = TryCast(message, Message_ImageRequest)
            If String.Compare(msg.ImageGUID, GUID, ignoreCase:=True) = 0 Then
                If Not Disposed Then msg.Image = Me
            End If
        End Sub
#End Region 'IEventSubScriber Implementation
    End Class 'dxfImage

End Namespace
