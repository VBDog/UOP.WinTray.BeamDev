

Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Windows.Controls
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfEntity
        Inherits dxfHandleOwner
        Implements IDisposable
#Region "Events"
        Public Event PropertyChangeEvent(aEvent As dxfEntityEvent)
#End Region 'Events

#Region "Members"

        Private _Components As TCOMPONENTS

        Private _Style As TTABLEENTRY

        Private _DimStyleStructure As TTABLEENTRY
        Friend _TextStyle As TTABLEENTRY
        Friend _PathEntities As dxfEntities

        Friend _Insert As dxeInsert
        Friend _MText As dxeText

#End Region 'Members

#Region "Constructors"
        Friend Sub New(aGraphicType As dxxGraphicTypes, Optional aEntityToCopy As dxfEntity = Nothing, Optional bCloneInstances As Boolean = False)
            MyBase.New("", aGraphicType)
            Try
                Initialize(aGraphicType, aEntityToCopy:=aEntityToCopy, bCloneInstances:=bCloneInstances)
            Catch
            End Try
        End Sub
        Friend Sub New(aGraphicType As dxxGraphicTypes, aDisplaySettings As dxfDisplaySettings, Optional aGUIDPrefix As String = Nothing, Optional aVertices As IEnumerable(Of iVector) = Nothing, Optional aEntityToCopy As dxfEntity = Nothing, Optional bCloneInstances As Boolean = False)
            MyBase.New("", aGraphicType, aGUIDPrefix)
            Try

                Initialize(aGraphicType, aDisplaySettings:=aDisplaySettings, aVertices:=aVertices, aEntityToCopy:=aEntityToCopy, bCloneInstances:=bCloneInstances)
            Catch
            End Try
        End Sub
        Friend Sub New(aTextType As dxxTextTypes, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aGUIDPrefix As String = Nothing, Optional aEntityToCopy As dxfEntity = Nothing, Optional bCloneInstances As Boolean = False)
            MyBase.New("", IIf(aTextType = dxxTextTypes.Multiline, dxxGraphicTypes.MText, dxxGraphicTypes.Text), aGUIDPrefix)
            Try

                If aTextType = dxxTextTypes.Undefined Then aTextType = dxxTextTypes.Multiline

                _GraphicType = IIf(aTextType = dxxTextTypes.Multiline, dxxGraphicTypes.MText, dxxGraphicTypes.Text)

                ''SetStructure(New TENTITY(_GraphicType, GUID, aTextType:=aTextType))
                Initialize(_GraphicType, aDisplaySettings:=aDisplaySettings, aEntityToCopy:=aEntityToCopy, aTextType:=aTextType, bCloneInstances:=bCloneInstances)

            Catch
            End Try
        End Sub
        Friend Sub New(aSubEntity As TENTITY, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bNewGUID As Boolean = False)
            MyBase.New("", aSubEntity.GraphicType)
            Try
                _GraphicType = aSubEntity.GraphicType

                If Not bNewGUID Then
                    If Not String.IsNullOrWhiteSpace(aSubEntity.GUID) Then MyBase.GUID = aSubEntity.GUID
                End If


                Initialize(_GraphicType, aDisplaySettings:=aDisplaySettings)
                Properties.CopyValues(aSubEntity.Props, True)
                Properties.GUID = GUID
                DefPts.Copy(aSubEntity.DefPts)
                TagFlagValue = New TTAGFLAGVALUE(aSubEntity.TagFlagValue)
                ExtendedData = New dxoPropertyArray(aSubEntity.ExtendedData)
                Components = New TCOMPONENTS(aSubEntity.Components, GUID)

            Catch
            End Try
        End Sub
        Private Sub Initialize(aGraphicType As dxxGraphicTypes, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aVertices As IEnumerable(Of iVector) = Nothing, Optional aDefPts As dxpDefPoints = Nothing, Optional aEntityToCopy As dxfEntity = Nothing, Optional aTextType As dxxTextTypes? = Nothing, Optional bCloneInstances As Boolean = False)
            _GraphicType = aGraphicType
            _State = dxxEntityStates.Steady
            _SuppressEvents = True
            If _GraphicType = dxxGraphicTypes.Text Then
                If Not aTextType.HasValue Then aTextType = dxxTextTypes.DText
                _Properties = dxpProperties.Get_EntityProperties(GraphicType, GUID, Handle, aTextType:=aTextType.Value)
            ElseIf _GraphicType = dxxGraphicTypes.MText Then
                _Properties = dxpProperties.Get_EntityProperties(GraphicType, GUID, Handle, aTextType:=dxxTextTypes.Multiline)
            Else
                _Properties = dxpProperties.Get_EntityProperties(GraphicType, GUID, Handle)
            End If

            If _PathEntities Is Nothing Then _PathEntities = New dxfEntities()
            _PathEntities.Owner = GUID
            _PathEntities.ImageGUID = ImageGUID


            ''If Not bDontSetStructure Then SetStructure(New TENTITY(GraphicType, GUID))
            If _DefPts Is Nothing Then
                _DefPts = New dxpDefPoints(GraphicType, Me)
            End If
            SetPropVal("*GUID", GUID)
            Select Case GraphicType
                Case dxxGraphicTypes.Dimension
                    DimStyle = New dxoDimStyle With {.IsCopied = True}
                Case dxxGraphicTypes.Leader
                    DimStyle = New dxoDimStyle With {.IsCopied = True}
                    _Attributes = New dxfAttributes()
                Case dxxGraphicTypes.Polygon
                    _AddSegs = New colDXFEntities
                Case dxxGraphicTypes.Hatch
                    _AddSegs = New colDXFEntities
                Case dxxGraphicTypes.Table
                    _Cells = New dxoTableCells()

                Case dxxGraphicTypes.Symbol
                    Style = New TTABLEENTRY(dxxSettingTypes.SYMBOLSETTINGS) With {.IsCopied = True}
                    SetPropVal("ArrowHead", dxxArrowHeadTypes.ByStyle, False)
                Case dxxGraphicTypes.Text, dxxGraphicTypes.MText
                    _Strings = New dxfStrings()
                    If Not aTextType.HasValue Then aTextType = dxxTextTypes.Multiline
                    If aTextType.Value = dxxTextTypes.Undefined Then aTextType = dxxTextTypes.Multiline
                    _TextStyle = New TTABLEENTRY(dxxReferenceTypes.STYLE, "Standard") With {.IsCopied = True}
                Case dxxGraphicTypes.Insert
                    _Attributes = New dxfAttributes()

            End Select
            MyBase.OwnerPtr = New WeakReference(Me)


            If aDisplaySettings IsNot Nothing Then

                Properties.CopyDisplayProperties(aDisplaySettings)
            End If
            If aVertices IsNot Nothing And _DefPts.HasVertices Then
                _DefPts.VerticesSet(aVertices)
            End If

            If aEntityToCopy IsNot Nothing Then
                Copy(aEntityToCopy, bCloneInstances:=bCloneInstances)
                SetPropVal("*SourceGUID", aEntityToCopy.GUID)

            End If

            _SuppressEvents = False

            '' Console.WriteLine(Me.Descriptor)

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property DxfHandles As ULong()
        Public Property DxfGroupHandle As ULong?

        Public Overrides Property ImageGUID As String
            '^the GUID of the image that owns this object
            Get
                Return MyBase.ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                If String.Compare(value, MyBase.ImageGUID, ignoreCase:=True) <> 0 Then
                    MyBase.ImageGUID = value
                    If _AddSegs IsNot Nothing Then _AddSegs.ImageGUID = value
                    If _MText IsNot Nothing Then _MText.ImageGUID = value
                    If _Insert IsNot Nothing Then _Insert.ImageGUID = value
                    If _DimStyle IsNot Nothing Then _DimStyle.ImageGUID = value
                End If
            End Set
        End Property


        Public ReadOnly Property Area As Double
            Get
                Return dxfEntity.GetArea(Me)
            End Get
        End Property
        Friend Property ArcStructure As TARC
            Get
                If GraphicType <> dxxGraphicTypes.Arc And GraphicType <> dxxGraphicTypes.Ellipse Then
                    Return New TARC(0)
                Else
                    Return ArcLineStructure.ArcStructure
                End If
            End Get
            Set(value As TARC)
                If GraphicType <> dxxGraphicTypes.Arc And GraphicType <> dxxGraphicTypes.Ellipse Then Return
                Dim bDirt As Boolean = Not TPLANES.Compare(PlaneV, value.Plane, 5, True, True)
                PlaneV = value.Plane
                If SetPropVal("*Clockwise", value.ClockWise) Then
                    bDirt = True
                End If
                If Not value.Plane.Origin <> DefPts.VectorGet(1) Then bDirt = True
                DefPts.VectorSet(1, value.Plane.Origin)
                Select Case GraphicType
                    Case dxxGraphicTypes.Arc
                        If SetPropVal("Radius", value.Radius) Then bDirt = True
                        If SetPropVal("Start Angle", value.StartAngle) Then bDirt = True
                        If SetPropVal("End Angle", value.EndAngle) Then bDirt = True
                        If SetPropVal("*StartWidth", value.StartWidth) Then bDirt = True
                        If SetPropVal("*EndWidth", value.EndWidth) Then bDirt = True
                    Case dxxGraphicTypes.Ellipse
                        If SetPropVal("*MajorRadius", value.Radius) Then bDirt = True
                        If SetPropVal("*MinorRadius", value.MinorRadius) Then bDirt = True
                        If SetPropVal("*StartAngle", value.StartAngle) Then bDirt = True
                        If SetPropVal("*EndAngle", value.EndAngle) Then bDirt = True
                End Select
                If bDirt Then IsDirty = True
            End Set
        End Property

        Friend _Cells As dxoTableCells
        Friend Property TableCells As dxoTableCells
            Get
                If GraphicType <> dxxGraphicTypes.Table Then
                    _Cells = Nothing
                    Return Nothing
                Else
                    If _Cells Is Nothing Then _Cells = New dxoTableCells()
                End If
                Return _Cells
            End Get
            Set(value As dxoTableCells)
                If GraphicType <> dxxGraphicTypes.Table Then
                    _Cells = Nothing
                    Return
                Else
                    If _Cells Is Nothing Then _Cells = New dxoTableCells()
                End If
                If value Is Nothing Then _Cells = New dxoTableCells Else _Cells = value
            End Set
        End Property

        Friend WriteOnly Property LineStructure As TLINE
            'Get
            '    If GraphicType <> dxxGraphicTypes.Line Then Return New TLINE Else Return ArcLineStructure.LineStructure
            'End Get
            Set(value As TLINE)
                If GraphicType <> dxxGraphicTypes.Line Then Return
                Dim bDirt As Boolean = DefPts.VectorSet(1, value.SPT)
                If DefPts.VectorSet(2, value.EPT) Then bDirt = True
                If SetPropVal("*StartWidth", value.StartWidth) Then bDirt = True
                If SetPropVal("*EndWidth", value.EndWidth) Then bDirt = True
                If bDirt Then IsDirty = True
            End Set
        End Property



        Private _DefPts As dxpDefPoints
        ''' <summary>
        ''' the object the contains the defined definition poits or vertices of the entity
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property DefPts As dxpDefPoints
            Get
                If _DefPts IsNot Nothing Then

                    _DefPts.SetGUIDS(ImageGUID, GUID, BlockGUID, SuppressEvents, Me)
                End If
                Return _DefPts
            End Get
            'Set(value As dxpDefPoints)
            '    _DefPts = value
            'End Set
        End Property
        Public Property Boundless As Boolean
            Get
                Return PropValueB("*Boundless")
            End Get
            Friend Set(value As Boolean)
                SetPropVal("*Boundless", value)
            End Set
        End Property
        Friend Property BoundaryLoops As TBOUNDLOOPS
            Get
                UpdatePath()
                Return _Components.BoundaryLoops
            End Get
            Set(value As TBOUNDLOOPS)
                _Components.BoundaryLoops = value
            End Set
        End Property
        Friend ReadOnly Property Bounds As TPLANE
            Get
                Return Paths.Bounds(PlaneV)
            End Get
            'Set(value As TPLANE)
            '    _Components.Paths.Bounds = Bounds
            'End Set
        End Property
        Public Property Col As Integer
            Get
                Return _TagFlagValue.Col
            End Get
            Set(value As Integer)
                _TagFlagValue.Col = value
            End Set
        End Property
        Public Property Color As dxxColors
            '^the entity's color
            '~default = dxxColors.ByLayer
            Get
                Return PropValueI("Color")
            End Get
            Set(value As dxxColors)
                If value = dxxColors.Undefined Then Return
                If SetPropVal("Color", value, False) Then
                    If Not IsDirty Then
                        _Components.Paths.SetDisplayVariable("COLOR", value)
                    Else
                        IsDirty = True
                    End If
                End If
            End Set
        End Property
        Friend Property Components As TCOMPONENTS
            Get
                Return _Components
            End Get
            Set(value As TCOMPONENTS)
                _Components = value
            End Set
        End Property

        Public ReadOnly Property HasVertices As Boolean
            Get
                Return DefPts.HasVertices
            End Get
        End Property
        Public Overridable ReadOnly Property DefPtCount As Integer
            Get
                Return DefPts.DefPtCnt
            End Get
        End Property
        Public Overridable ReadOnly Property Descriptor As String
            Get
                Dim _rVal As String = String.Empty
                'On Error Resume Next
                Select Case GraphicType
                    Case dxxGraphicTypes.Arc
                        Dim v1 As TVECTOR = DefPts.VectorGet(1)
                        If EntityType = dxxEntityTypes.Circle Then
                            _rVal = $"CIRCLE - CP:{v1.Coordinates(4)} /RAD:{ PropValueD("Radius"):0.000}"
                        Else
                            _rVal = $"ARC - CP:{v1.Coordinates(4)} /RAD:{ PropValueD("Radius"):0.000}/SA:{ PropValueD("Start Angle"):0.0} /EA:{PropValueD("End Angle"):0.0} /CLKWS:{PropValueB("*Clockwise")}"
                        End If

                    Case dxxGraphicTypes.Bezier
                        _rVal = DefiningVectors.CoordinatesR(3)
                    Case dxxGraphicTypes.Dimension
                    Case dxxGraphicTypes.Ellipse
                        Dim v1 As TVECTOR = DefPts.VectorGet(1)
                        _rVal = $"{v1.Coordinates(0) }/Rad-{ PropValueD("*MajorRadius"):0.000#}/{ PropValueD("*StartAngle"):0.0#}/{ PropValueD("*EndAngle"):0.0#}/{ PropValueB("*Clockwise")}"
                    Case dxxGraphicTypes.Hatch
                    Case dxxGraphicTypes.Hole
                        'Dim Ent As dxeHole = Me
                        '_rVal = dxfUtils.HoleDescriptor(Ent)
                    Case dxxGraphicTypes.Insert
                        Dim v1 As TVECTOR = DefPts.VectorGet(1)
                        _rVal = $"{PropValueStr("Block Name") },{ v1.Coordinates(0)}"
                    Case dxxGraphicTypes.Leader
                        _rVal = Vertices.Coordinates_Get
                    Case dxxGraphicTypes.Line
                        Dim v1 As TVECTOR = DefPts.VectorGet(1)
                        Dim v2 As TVECTOR = DefPts.VectorGet(2)
                        _rVal = $"{v1.Coordinates(0) },{ v2.Coordinates(0)}"
                    Case dxxGraphicTypes.Point
                        _rVal = DefPts.Vector1.Coordinates(0)
                    Case dxxGraphicTypes.Polygon
                        _rVal = Vertices.CoordinatesV
                    Case dxxGraphicTypes.Polyline
                        _rVal = Vertices.CoordinatesV
                    Case dxxGraphicTypes.Solid
                        _rVal = DefiningVectors.CoordinatesR(3)
                    Case dxxGraphicTypes.Symbol
                        _rVal = String.Empty
                    Case dxxGraphicTypes.Table
                        UpdatePath()
                        _rVal = String.Empty
                    Case dxxGraphicTypes.Text
                        _rVal = $"{PropValueStr("Text String")} : {DefPts.Vector1.Coordinates(0)}"
                    Case dxxGraphicTypes.Shape
                        Dim aShp As dxeShape
                        aShp = Me
                        _rVal = $"{PropValueStr("*FileName")}({ PropValueI("*ShapeNumber") }"
                End Select
                If _rVal = String.Empty Then
                    _rVal = GraphicTypeName
                Else
                    _rVal = $"{GraphicTypeName}:{ _rVal}"
                End If
                Return _rVal
            End Get
        End Property

        Private _DimStyle As dxoDimStyle
        ''' <summary>
        ''' the dxoDimStyle that governs the geometry of the entity
        ''' </summary>
        ''' <remarks>returns null if the entity is not have a dimstyle</remarks>
        ''' <returns></returns>
        Public Overridable Property DimStyle As dxoDimStyle
            Get
                If Not HasDimStyle Then
                    If _DimStyle IsNot Nothing Then _DimStyle.ReleaseReferences()
                    _DimStyle = Nothing
                    Return Nothing
                End If
                If _DimStyle Is Nothing Then _DimStyle = New dxoDimStyle
                _DimStyle.SetGUIDS(ImageGUID, GUID, BlockGUID, dxxFileObjectTypes.Entity, Me)
                _DimStyle.IsCopied = True
                Return _DimStyle
            End Get
            Friend Set(value As dxoDimStyle)
                If Not HasDimStyle Or value Is Nothing Then
                    If _DimStyle IsNot Nothing Then _DimStyle.ReleaseReferences()
                    _DimStyle = Nothing
                    Return
                End If
                If _DimStyle IsNot Nothing Then
                    If Not _DimStyle.IsEqual(value) Then
                        _DimStyle.Properties.CopyVals(value.SettingProperties(), bSkipHandles:=False, bSkipPointers:=False)
                        _DimStyle.IsDirty = True
                    End If
                Else
                    _DimStyle = value
                End If
                _DimStyle.IsCopied = True
                _DimStyle.IsGlobal = False
                SetPropVal("DimStyle Name", _DimStyle.Name)
                _Style = New TTABLEENTRY(_DimStyle)
                _DimStyleStructure = _Style
            End Set
        End Property
        Public Property DimStyleName As String
            Get
                Dim _rVal As String
                If HasDimStyle Then
                    _rVal = DimStyle.Name

                Else
                    _rVal = _DimStyleStructure.Name
                End If
                Return _rVal
            End Get
            Friend Set(value As String)
                If HasDimStyle Then
                    DimStyle.Name = value
                    SetPropVal("DimStyle Name", DimStyle.Name)
                    _DimStyleStructure = New TTABLEENTRY(DimStyle)
                Else
                    _DimStyleStructure.Name = value
                End If
            End Set
        End Property
        Friend Property DimStyleStructure As TTABLEENTRY
            Get
                If _DimStyle IsNot Nothing Then _DimStyleStructure = New TTABLEENTRY(_DimStyle)
                Return _DimStyleStructure
            End Get
            Set(value As TTABLEENTRY)

                If Not HasDimStyle Then Return
                '^the style (with overides) that applies to this dimension
                DimStyle.Properties = New dxoProperties(value.Props)
                SetPropVal("DimStyle Name", DimStyle.Name)
                _DimStyleStructure = New TTABLEENTRY(DimStyle)
                _Style = New TTABLEENTRY(DimStyle)
            End Set
        End Property
        Friend Property TextStyleStructure As TTABLEENTRY
            Get
                Return _TextStyle
            End Get
            Set(value As TTABLEENTRY)
                _TextStyle = value
            End Set
        End Property
        Public Overloads Property Domain As dxxDrawingDomains
            Get
                Return PropValueI("*Domain")
            End Get
            Set(value As dxxDrawingDomains)
                If value = dxxDrawingDomains.Screen Or value = dxxDrawingDomains.Model Or value = dxxDrawingDomains.Paper Then

                    SetPropVal("*Domain", value)
                    MyBase.Domain = value
                End If
            End Set
        End Property
        Public Overridable Property TextStyleName As String
            Get
                If _MText IsNot Nothing Then Return _MText.TextStyleName
                Dim _rVal As String = String.Empty
                If HasDimStyle Then
                    _rVal = DimStyle.TextStyleName
                Else
                    If IsText Then
                        _rVal = PropValueStr(7, "Standard")
                    End If
                End If
                Return _rVal
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                If HasDimStyle Then
                    DimStyle.TextStyleName = value
                End If
                If IsText Then
                    If SetPropVal(7, value, True) Then
                        _TextStyle.Name = value
                    End If
                End If
            End Set
        End Property
        Public ReadOnly Property TextStyleNames As String
            Get
                Dim _rVal As String
                UpdatePath()
                Select Case GraphicType
                    Case dxxGraphicTypes.Table
                        Dim aTbl As dxeTable = Me
                        _rVal = aTbl.Entities.TextStyleNames
                    Case dxxGraphicTypes.Symbol
                        Dim aSym As dxeSymbol = Me
                        _rVal = aSym.Entities.TextStyleNames
                    Case Else
                        _rVal = TextStyleName
                End Select
                Return _rVal
            End Get
        End Property
        Public Overridable Property DisplaySettings As dxfDisplaySettings
            '^the object which carries display style information for an entity
            Get
                Return New dxfDisplaySettings(DisplayStructure)
            End Get
            Set(value As dxfDisplaySettings)
                If value IsNot Nothing Then CopyDisplayValues(value)
            End Set
        End Property
        Friend Property DisplayStructure As TDISPLAYVARS
            Get
                Dim _rVal As New TDISPLAYVARS("0", dxfLinetypes.ByLayer, dxxColors.ByLayer, dxxLineWeights.ByLayer, 1)

                If Properties.Count <= 0 Then Return _rVal
                _rVal.Color = PropValueI("Color", aDefault:=dxxColors.ByLayer)
                _rVal.LayerName = PropValueStr("Layername", "0")
                _rVal.Linetype = PropValueStr("Linetype", dxfLinetypes.ByLayer)
                _rVal.LTScale = PropValueD("LT Scale", 1)
                _rVal.LineWeight = PropValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer)
                _rVal.Suppressed = PropValueB(dxfLinetypes.Invisible)
                If IsText Then
                    _rVal.TextStyle = PropValueStr("Text Style Name", "Standard")
                End If
                Return _rVal
            End Get
            Set(value As TDISPLAYVARS)
                CopyDisplayValues(value)
            End Set
        End Property
        Friend Property DrawnPaths As TPATHS
            Get
                _Components.DrawnPaths.EntityGUID = GUID
                Return _Components.DrawnPaths
            End Get
            Set(value As TPATHS)
                _Components.DrawnPaths = value
                _Components.DrawnPaths.EntityGUID = GUID
            End Set
        End Property
        Public Overridable ReadOnly Property EntityType As dxxEntityTypes
            Get

                Select Case GraphicType
                    Case dxxGraphicTypes.Arc  ' = 4
                       'overrided by dxeArc  could be circle or arc
                    Case dxxGraphicTypes.Solid  ' = 32
                        'overrided by dxeSolid could be solid or trace
                    Case dxxGraphicTypes.Line  ' = 2
                        'overrided by dxeLine could be a polyline if the start or end width is defined
                    Case dxxGraphicTypes.Text  ' = 512
                         'overrided by dxeText   follows text type
                    Case dxxGraphicTypes.Dimension  ' = 4096
                        'overrided by dxeDimension  depends on dim type
                    Case dxxGraphicTypes.Hole  ' = 16384
                        'overrided by dxeHole  could be slot or hole

                    Case Else
                        Return GraphicType.EntityType()

                End Select
                Return dxxEntityTypes.Undefined
            End Get
            'Friend Set(value As dxxEntityTypes)
            '    SetPropVal("*EntityType", value, True)
            'End Set
        End Property
        Public ReadOnly Property EntityTypeName As String
            Get
                Return dxfEnums.Description(EntityType)
            End Get
        End Property

        Public ReadOnly Property ExtentPoints As colDXFVectors
            '^the points used to define the extents of an entity on its plane
            Get
                UpdatePath()
                Return New colDXFVectors(ExtentPts())  '  CType(ExtentPts(), colDXFVectors)
            End Get
        End Property
        Public ReadOnly Property ExtrusionDirection As dxfDirection
            Get
                '^returnd the z-axis if the entities plane
                Return Plane.ZDirection
            End Get
        End Property

        Private _Disposed As Boolean
        Public ReadOnly Property Disposed As dxxGraphicTypes
            Get
                Return _Disposed
            End Get
        End Property


        Private _GraphicType As dxxGraphicTypes
        Public ReadOnly Property GraphicType As dxxGraphicTypes
            Get
                Return _GraphicType
            End Get
        End Property

        Public Property TextType As dxxTextTypes
            Get

                If GraphicType = dxxGraphicTypes.Text Then
                    Dim otype As String = Properties.ValueS(0).Trim().ToUpper()
                    If otype = "ATTDEF" Then
                        Return dxxTextTypes.AttDef
                    ElseIf otype = "ATTRIB" Then
                        Return dxxTextTypes.Attribute
                    Else
                        Return dxxTextTypes.DText
                    End If
                ElseIf GraphicType = dxxGraphicTypes.MText Then
                    Return dxxTextTypes.Multiline
                Else
                    Return dxxTextTypes.Undefined
                End If


            End Get
            Set(value As dxxTextTypes)
                Dim gType As dxxGraphicTypes = GraphicType
                If gType <> dxxGraphicTypes.Text Then Return
                If Not dxfEnums.Validate(GetType(dxxTextTypes), TVALUES.To_INT(value), TVALUES.To_INT(dxxTextTypes.Multiline), bSkipNegatives:=True) Then Return
                If value = TextType Then Return

                Dim newProps As dxoProperties = IIf(value = dxxTextTypes.AttDef, dxpProperties.Get_EntityProperties(GraphicType, GUID, Handle, aTextType:=dxxTextTypes.AttDef), dxpProperties.Get_EntityProperties(GraphicType, GUID, Handle, aTextType:=dxxTextTypes.Attribute))
                newProps.CopyVals(Properties, bSkipHandles:=False, bSkipPointers:=False)
                newProps.GUID = GUID
                newProps.Handle = Handle
                'mtext cannot me converted to DTEXT type (atribs etc.) direction Use SubText
                'DText can be converted to atributes text types
                _Properties = newProps
            End Set
        End Property

        Public ReadOnly Property GraphicTypeName As String
            Get
                Return dxfEnums.Description(GraphicType)
            End Get
        End Property
        Public Overrides Property GUID As String
            Get
                Dim _rVal As String = MyBase.GUID
                If String.IsNullOrWhiteSpace(_rVal) Then
                    _rVal = dxfEvents.NextEntityGUID(GraphicType)
                    MyBase.GUID = _rVal
                    Properties.GUID = _rVal

                End If
                Return _rVal
                ''Return Props.GUID
            End Get
            Friend Set(value As String)
                Properties.GUID = value
                MyBase.GUID = value
            End Set
        End Property
        Public ReadOnly Property HasDimStyle As Boolean
            Get
                Select Case GraphicType
                    Case dxxGraphicTypes.Dimension, dxxGraphicTypes.Leader
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        Public Overridable ReadOnly Property HasSubEntities As Boolean
            Get
                Select Case GraphicType

                    Case dxxGraphicTypes.Dimension, dxxGraphicTypes.Leader, dxxGraphicTypes.Polygon, dxxGraphicTypes.Symbol, dxxGraphicTypes.Table, dxxGraphicTypes.Insert
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        Public ReadOnly Property PaperSpace As Boolean
            Get
                Return (Domain = dxxDrawingDomains.Paper)
            End Get
        End Property
        Friend ReadOnly Property PathIsImageDependant As Boolean
            Get
                Select Case GraphicType
         '--------------------- PRIMATIVES -----------------------------------------------------
                    Case dxxGraphicTypes.Arc
                        Return False
                    Case dxxGraphicTypes.Line
                        Return False
                    Case dxxGraphicTypes.Bezier
                        Return False
                    Case dxxGraphicTypes.Ellipse
                        Return False
                        ''--------------------- COMPOSITES -----------------------------------------------------
                    Case dxxGraphicTypes.Polygon
                        Return False
                    Case dxxGraphicTypes.Polyline
                        Return False
                    Case dxxGraphicTypes.Text, dxxGraphicTypes.MText
                        Return False
                    Case dxxGraphicTypes.Dimension
                        Return True
                    Case dxxGraphicTypes.Hatch
                        Return True
                    Case dxxGraphicTypes.Hole
                        Return False
                    Case dxxGraphicTypes.Insert
                        Return True
                    Case dxxGraphicTypes.Leader
                        Return True
                    Case dxxGraphicTypes.Point
                        Return False
                    Case dxxGraphicTypes.Solid
                        Return False
                    Case dxxGraphicTypes.Symbol
                        Return False
                    Case dxxGraphicTypes.Table
                        Return True
                    Case dxxGraphicTypes.Shape
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property
        Public Overloads Property Identifier As String
            Get
                Return PropValueStr("*Identifier")
            End Get
            Friend Set(value As String)
                SetPropVal("*Identifier", value)
                MyBase.Identifier = PropValueStr("*Identifier")
            End Set
        End Property
        Public Overloads Property Index As Integer
            Get
                Return MyBase.Index
            End Get
            Friend Set(value As Integer)
                MyBase.Index = value
            End Set
        End Property

        Private _Instances As dxoInstances

        ''' <summary>
        ''' instances causes the entity to be multiplied in it's image reducing the total number of entities and increasing the speed of the redraw of the image.
        ''' </summary>
        ''' <remarks>if the instances get changed, the view of the entity is marked as dirty and will be regenerated on the next request</remarks>
        ''' <returns></returns>
        Public Property Instances As dxoInstances
            Get
                If _Instances Is Nothing Then
                    _Instances = New dxoInstances(Me)
                Else
                    _Instances.Owner = Me
                End If
                Return _Instances
            End Get
            Set(value As dxoInstances)
                If value IsNot Nothing Then
                    If Instances.Copy(value, True) Then IsDirty = True

                Else
                    Instances.Clear()
                End If
            End Set
        End Property
        Public ReadOnly Property TypeName As String
            Get
                '^the entity DXF type name
                Return GraphicTypeName.ToUpper
            End Get
        End Property
        Public ReadOnly Property IsText As Boolean
            Get
                Return GraphicType = dxxGraphicTypes.MText Or GraphicType = dxxGraphicTypes.Text
            End Get
        End Property
        Public Overridable Property IsDirty As Boolean
            Get
                If _DefPts Is Nothing Then _DefPts = New dxpDefPoints(GraphicType, Me)
                Dim _rVal As Boolean = Properties.IsDirty Or DefPts.IsDirty Or _Components.Paths.Count <= 0
                If HasSubEntities And _PathEntities Is Nothing Then _rVal = True
                If IsText And _Strings Is Nothing Then _rVal = True
                If _Instances IsNot Nothing Then
                    If _Instances.IsDirty Then _rVal = True
                End If
                Return _rVal
            End Get
            Friend Set(value As Boolean)

                If _DefPts Is Nothing Then _DefPts = New dxpDefPoints(GraphicType, Me)
                If value <> (Properties.IsDirty Or DefPts.IsDirty Or Instances.IsDirty) Then
                    DefPts.IsDirty = value
                    Properties.IsDirty = value
                    Instances.IsDirty = value
                    If value Then
                        If BelongsToBlock Then  'raise change to a block Entity
                            Dim block As dxfBlock = MyBlock
                            If block IsNot Nothing Then block.IsDirty = True
                        End If
                    End If
                End If
            End Set
        End Property
        Public Property LayerName As String
            '^the name of the layer that the entity resides on
            '~default = "0"
            Get
                Return PropValueStr(8, "0")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim()
                If value = String.Empty Then value = "0"
                If SetPropVal(8, value, False) Then
                    If Not IsDirty Then
                        _Components.Paths.SetDisplayVariable("LAYERNAME", value)
                    End If
                End If
            End Set
        End Property

        Friend Function PropValueStr(aName As String, Optional aDefault As String = Nothing, Optional aOccur As Integer = 1) As String
            Return Properties.ValueS(aName, aOccur:=aOccur, aDefault:=aDefault)
        End Function

        Friend Function PropValueD(aName As String, Optional aDefault As Double = 0.0, Optional aOccur As Integer = 1) As Double
            Return Properties.ValueD(aName, aOccur:=aOccur, aDefault:=aDefault)
        End Function
        Friend Function PropValueI(aName As String, Optional aDefault As Integer = 0, Optional aOccur As Integer = 1) As Integer
            Return Properties.ValueI(aName, aOccur:=aOccur, aDefault:=aDefault)
        End Function
        Friend Function PropValueB(aName As String, Optional aDefault As Boolean = False, Optional aOccur As Integer = 1) As Boolean
            Return Properties.ValueB(aName, aOccur:=aOccur, aDefault:=aDefault)
        End Function

        Friend Function PropValueStr(aGroupCode As Integer, Optional aDefault As String = Nothing, Optional aOccur As Integer = 1) As String
            Return Properties.ValueS(aGroupCode, aOccur:=aOccur, aDefault:=aDefault)
        End Function

        Friend Function PropValueD(aGroupCode As Integer, Optional aDefault As Double = 0.0, Optional aOccur As Integer = 1) As Double
            Return Properties.ValueD(aGroupCode, aOccur:=aOccur, aDefault:=aDefault)
        End Function
        Friend Function PropValueI(aGroupCode As Integer, Optional aDefault As Integer = 0, Optional aOccur As Integer = 1) As Integer
            Return Properties.ValueI(aGroupCode, aOccur:=aOccur, aDefault:=aDefault)
        End Function
        Friend Function PropValueB(aGroupCode As Integer, Optional aDefault As Boolean = False, Optional aOccur As Integer = 1) As Boolean
            Return Properties.ValueB(aGroupCode, aOccur:=aOccur, aDefault:=aDefault)
        End Function

        Public ReadOnly Property LayerNames As String
            Get
                Dim _rVal As String = LayerName
                Select Case GraphicType
                    Case dxxGraphicTypes.Hole, dxxGraphicTypes.Insert, dxxGraphicTypes.Leader, dxxGraphicTypes.Polygon, dxxGraphicTypes.Symbol, dxxGraphicTypes.Table, dxxGraphicTypes.Dimension
                        UpdatePath()
                        _rVal = Paths.LayerNames(LayerName)
                End Select
                Return _rVal
            End Get
        End Property
        Public Overridable Property Length As Double
            Get
                Return dxfEntity.EntityLength(Me)
            End Get
            Set(value As Double)
                SetPropVal("Length", value)
            End Set
        End Property
        Public Property Linetype As String
            Get
                '^the linetype assigned to the entity
                '~default = dxfLinetypes.ByLayer
                Return PropValueStr("Linetype")
            End Get
            Set(value As String)
                '^the linetype assigned to the entity
                '~default = dxfLinetypes.ByLayer
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty
                value = value.Trim
                If value = String.Empty Then value = dxfLinetypes.ByLayer
                If SetPropVal("LineType", value, False) Then
                    If Not IsDirty Then
                        If String.Compare(value, dxfLinetypes.ByBlock, True) <> 0 Then
                            _Components.Paths.SetDisplayVariable("LINETYPE", value)
                        Else
                            IsDirty = True
                        End If
                    End If
                End If
            End Set
        End Property
        Public ReadOnly Property LineTypes As String
            Get
                Dim _rVal As String = Linetype
                Select Case GraphicType
                    Case dxxGraphicTypes.Hole, dxxGraphicTypes.Insert, dxxGraphicTypes.Leader, dxxGraphicTypes.Polygon, dxxGraphicTypes.Symbol, dxxGraphicTypes.Table, dxxGraphicTypes.Dimension
                        UpdatePath()
                        _rVal = Paths.Linetypes(_rVal)
                End Select
                Return _rVal
            End Get
        End Property
        Public Property LTScale As Double
            '^the scale to apply to the line type pattern of the entity
            '~default = 1
            Get
                Return PropValueD("LT Scale")
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                If SetPropVal("LT Scale", value, False) And Not IsDirty Then
                    _Components.Paths.SetDisplayVariable("LTSCALE", value)
                End If
            End Set
        End Property
        Public Property LineWeight As dxxLineWeights
            Get
                '^the with of the pen to render the entity with
                '~default = ByLayer
                Return PropValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer)
            End Get
            Set(value As dxxLineWeights)
                '^the with of the pen to render the entity with
                '~default = ByLayer
                If value < dxxLineWeights.ByBlock Then value = PropValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer)
                If value > dxxLineWeights.LW_211 Then value = PropValueI(dxxLineWeights.LW_211)
                If SetPropVal("LineWeight", value, False) And Not IsDirty Then
                    If value <> dxxLineWeights.ByBlock Then
                        _Components.Paths.SetDisplayVariable("LINEWEIGHT", value)
                    Else
                        IsDirty = True
                    End If
                End If
            End Set
        End Property
        Friend Property ReactorEntity As dxfEntity
            Get
                If GraphicType = dxxGraphicTypes.Leader Then
                    Dim lType As dxxLeaderTypes = PropValueI("Leader Type")
                    If lType = dxxLeaderTypes.LeaderText Then
                        _Insert = Nothing
                        If _MText Is Nothing Then
                            _MText = New dxeText(aTextType:=dxxTextTypes.Multiline, aGUIDPrefix:=GUID & ".")
                        End If
                        _MText.GroupName = GroupName
                        _MText.Instances = Instances
                        _MText.SetGUIDS(ImageGUID, GUID, BlockGUID, dxxFileObjectTypes.Entity, Me)
                        SetPropVal("*ReactorHandle", _MText.Handle)

                        Return _MText
                    ElseIf lType = dxxLeaderTypes.LeaderBlock Then
                        _MText = Nothing
                        If _Insert Is Nothing Then
                            _Insert = New dxeInsert(GUID & ".")
                        End If
                        _Insert.Instances = Instances
                        _Insert.GroupName = GroupName
                        _Insert.SetGUIDS(ImageGUID, GUID, BlockGUID, dxxFileObjectTypes.Entity, Me)
                        SetPropVal("*ReactorHandle", _Insert.Handle)

                        Return _Insert
                    Else
                        _MText = Nothing
                        _Insert = Nothing
                        SetPropVal(340, "0", aOccurance:=1)
                        Return Nothing
                    End If
                End If
                Return Nothing
            End Get
            Set(value As dxfEntity)
                If value Is Nothing Then
                    If _MText IsNot Nothing Then
                        _MText.SetGUIDS(ImageGUID, "", "", dxxFileObjectTypes.Undefined)
                        _MText = Nothing
                    End If
                    If _Insert IsNot Nothing Then
                        _Insert.SetGUIDS(ImageGUID, "", "", dxxFileObjectTypes.Undefined)
                        _Insert = Nothing
                    End If
                    SetPropVal(340, "0", aOccurance:=1)
                    Return
                End If
                value.Instances = Instances
                If GraphicType = dxxGraphicTypes.Leader Then
                    PropVectorSet("*Vector1", value.HandlePt)
                    If _Insert IsNot Nothing Then
                        _Insert.SetGUIDS(ImageGUID, "", "", dxxFileObjectTypes.Undefined)
                        _Insert = Nothing
                    End If
                    If _MText IsNot Nothing Then
                        _MText.SetGUIDS(ImageGUID, "", "", dxxFileObjectTypes.Undefined)
                        _MText = Nothing
                    End If
                    If value.GraphicType = dxxGraphicTypes.MText Or value.GraphicType = dxxGraphicTypes.Text Then
                        SetPropVal("Leader Type", dxxLeaderTypes.LeaderText)
                    ElseIf value.GraphicType = dxxGraphicTypes.Insert Then
                        SetPropVal("Leader Type", dxxLeaderTypes.LeaderBlock)
                    Else
                        SetPropVal("Leader Type", dxxLeaderTypes.NoReactor)
                    End If
                    Dim lType As dxxLeaderTypes = PropValueI("Leader Type")
                    SetPropVal(340, value.Handle, aOccurance:=1)
                    If lType = dxxLeaderTypes.LeaderText Then
                        If value.GraphicType = dxxGraphicTypes.MText Then
                            _MText = value
                            If _MText.GUID.ToUpper.Contains("LEADER") Then
                                _MText.GUID = $"{GUID }.{ _MText.GUID}"
                            End If
                            _MText.SetGUIDS(ImageGUID, GUID, BlockGUID, dxxFileObjectTypes.Entity)
                            _TextStyle.Name = _MText.TextStyleName
                            DimStyle.TextStyleName = _MText.TextStyleName
                        End If
                        Return
                    ElseIf lType = dxxLeaderTypes.LeaderBlock Then
                        If value.GraphicType = dxxGraphicTypes.Insert Then
                            _Insert = value
                            _Insert.SetGUIDS(ImageGUID, GUID, BlockGUID, dxxFileObjectTypes.Entity)
                            If _Insert.GUID.ToUpper.Contains("LEADER") Then
                                _Insert.GUID = $"{GUID}.{ _Insert.GUID}"
                            End If
                        End If
                        Return
                    End If
                Else
                    If _MText IsNot Nothing Then
                        _MText.SetGUIDS(ImageGUID, "", "", dxxFileObjectTypes.Undefined)
                        _MText = Nothing
                    End If
                    If _Insert IsNot Nothing Then
                        _Insert.SetGUIDS(ImageGUID, "", "", dxxFileObjectTypes.Undefined)
                        _Insert = Nothing
                    End If
                    Return
                End If
                Return
            End Set
        End Property
        Public Overrides ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.Entity
            End Get
        End Property
        Public ReadOnly Property PathPoints As colDXFVectors
            '^returns the points that are used to draw the entity to a graphic device
            Get
                UpdatePath()
                Return New colDXFVectors(CType(_Components.Paths, TVECTORS))
            End Get
        End Property
        Friend Property Paths As TPATHS
            Get
                UpdatePath()
                _Components.Paths.EntityGUID = GUID
                Return _Components.Paths
            End Get
            Set(value As TPATHS)

                _Components.Paths = value
                _Components.Paths.EntityGUID = GUID
            End Set
        End Property
        Public Overridable Property Plane As dxfPlane
            Get
                Return New dxfPlane(PlaneV)
            End Get
            Set(value As dxfPlane)
                DefPts.PlaneObj = value
            End Set
        End Property
        Friend Overridable Property PlaneV As TPLANE
            Get

                Dim _rVal As TPLANE = DefPts.Plane
                If IsText Then
                    Dim ang As Double = PropValueD(50)
                    If TextType = dxxTextTypes.Multiline Then ang *= 180 / Math.PI

                    If ang <> 0 Then
                        _rVal.Rotate(ang)
                    End If
                End If

                Return _rVal
            End Get
            Set(value As TPLANE)
                DefPts.Plane = value
            End Set
        End Property

        Friend ReadOnly Property Props As TPROPERTIES
            Get
                Properties.Handle = Handle
                Dim _rVal As TPROPERTIES = New TPROPERTIES(Properties)
                _rVal.Handle = Handle
                Return _rVal
            End Get

        End Property



        Private _Properties As dxoProperties
        Friend Overridable Property Properties As dxoProperties
            Get
                If _Properties Is Nothing Then
                    If GraphicType <> dxxGraphicTypes.Undefined Then Return New dxoProperties()
                    _Properties = dxpProperties.Get_EntityProperties(GraphicType, GUID, Handle, aTextType:=TextType)
                End If

                Return _Properties
            End Get
            Set(value As dxoProperties)
                If value Is Nothing Then Return
                _Properties.CopyVals(value, bSkipHandles:=True, bSkipPointers:=True)
            End Set

        End Property
        Public ReadOnly Property Rectangle As dxfRectangle
            '^returns a copy of the entities current bounding rectangle
            Get
                Return BoundingRectangle()
            End Get
        End Property
        Public Function ReferencesLayer(aLayerName As String) As Boolean
            Return TLISTS.Contains(LayerNames, aLayerName)
        End Function
        Public Function ReferencesLinetype(aLineType As String) As Boolean
            Return TLISTS.Contains(LineTypes, aLineType)
        End Function

        Private _Reactors As dxoPropertyArray
        Friend Property Reactors As dxoPropertyArray
            Get
                If _Reactors Is Nothing Then _Reactors = New dxoPropertyArray
                _Reactors.Name = "Reactors"
                Return _Reactors
            End Get
            Set(value As dxoPropertyArray)
                If _Reactors Is Nothing Then _Reactors = New dxoPropertyArray
                _Reactors.Append(value, True, True)

            End Set
        End Property

        Private _ExtendedData As dxoPropertyArray
        Friend Property ExtendedData As dxoPropertyArray
            Get
                If _ExtendedData Is Nothing Then _ExtendedData = New dxoPropertyArray
                _ExtendedData.Name = "ExtendedData"
                Return _ExtendedData
            End Get
            Set(value As dxoPropertyArray)
                If _ExtendedData Is Nothing Then _ExtendedData = New dxoPropertyArray
                _ExtendedData.Append(value, True, True)

            End Set
        End Property


        Friend ReadOnly Property ReactorProperties As dxoProperties
            Get
                Return Reactors.Item("{ACAD_REACTORS")
            End Get
        End Property

        Public Property ReactorHandle As String
            '^the handle of another entity that this entity reacts to
            Get
                Return PropValueStr("*ReactorHandle", "0")
            End Get
            Friend Set(value As String)
                '^the handle of another entity that this entity reacts to
                SetPropVal("*ReactorHandle", value)
            End Set
        End Property
        Public Property Row As Integer
            Get
                Return _TagFlagValue.Row
            End Get
            Set(value As Integer)
                _TagFlagValue.Row = value
            End Set
        End Property
        Public Overrides Property Name As String
            Get
                Dim _rVal As String = MyBase.Name
                If String.IsNullOrEmpty(_rVal) Then _rVal = Identifier
                If String.IsNullOrEmpty(_rVal) Then _rVal = Tag
                If String.IsNullOrEmpty(_rVal) Then _rVal = String.Empty
                Return _rVal
            End Get
            Friend Set(value As String)
                MyBase.Name = value
                SetPropVal("*Name", value, False)
            End Set
        End Property
        Public Property SaveToFile As Boolean
            '^controls if the entity will be written to file if a write request is made
            '~False means the entity only appears in the image but not in the written file
            Get
                Dim _rVal As Boolean = PropValueB("*SaveToFile") And Domain <> dxxDrawingDomains.Screen
                If _rVal Then _rVal = String.Compare(Linetype, dxfLinetypes.Invisible, True) <> 0
                Return _rVal
            End Get
            Set(value As Boolean)
                SetPropVal("*SaveToFile", value)
            End Set
        End Property
        Public Overloads Property SourceGUID As String
            Get
                Return PropValueStr("*SourceGUID")
            End Get
            Friend Set(value As String)
                SetPropVal("*SourceGUID", value)
                MyBase.SourceGUID = value
            End Set
        End Property

        Private _State As dxxEntityStates
        Public Property State As dxxEntityStates
            Get
                Return _State
            End Get
            Friend Set(value As dxxEntityStates)
                _State = value
            End Set
        End Property
        Friend Property PathSegments As TSEGMENTS
            Get
                UpdatePath()
                Return _Components.Segments
            End Get
            Set(value As TSEGMENTS)
                _Components.Segments = value
            End Set
        End Property
        Friend Property Style As TTABLEENTRY
            Get
                If HasDimStyle Then
                    _Style = New TTABLEENTRY(DimStyle)
                    _DimStyleStructure = _Style
                End If
                Return _Style
            End Get
            Set(value As TTABLEENTRY)
                If value.Props.Count <= 0 Then Return
                _Style = value
                _Style.IsCopied = True
                _Style.ImageGUID = ImageGUID
                If HasDimStyle And value.EntryType = dxxReferenceTypes.DIMSTYLE Then
                    _DimStyleStructure = _Style
                    DimStyle.Properties.CopyVals(_DimStyleStructure, bSkipHandles:=False, bSkipPointers:=False)
                ElseIf value.EntryType = dxxReferenceTypes.STYLE Then
                    _TextStyle = _Style
                End If

            End Set
        End Property




        Private _SubProperties As dxoProperties
        ''' <summary>
        ''' used to store attribute properties
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property SubProperties As dxoProperties
            Get
                If _SubProperties IsNot Nothing Then
                    _SubProperties.Handle = Handle
                    _SubProperties.GUID = GUID
                    _SubProperties.Name = "Attributes"
                End If
                Return _SubProperties

            End Get
            Set(value As dxoProperties)
                _SubProperties = value
                _SubProperties.Handle = Handle
                _SubProperties.GUID = GUID
            End Set
        End Property

        ''' <summary>
        ''' flag indicating if the entity should be rendered or saved
        ''' </summary>
        ''' <returns></returns>
        Friend Overrides Property Suppressed As Boolean
            Get
                Return PropValueB(dxfLinetypes.Invisible)
            End Get
            Set(value As Boolean)
                If SetPropVal(dxfLinetypes.Invisible, value, False) Then
                    _Components.Paths.Suppressed = value
                End If
            End Set
        End Property


        Private _SuppressEvents As Boolean
        ''' <summary>
        ''' flag indicating if the entity should stop throwing change events
        ''' </summary>
        ''' <returns></returns>
        Friend Property SuppressEvents As Boolean
            Get
                Return _SuppressEvents Or State <> dxxEntityStates.Steady
            End Get
            Set(value As Boolean)
                If _SuppressEvents <> value Then
                    DefPts.SuppressEvents = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' provided to carry additional textual info about the entity
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property Tag As String
            Get
                Return _TagFlagValue.Tag
            End Get
            Set(value As String)
                If value Is Nothing Then value = String.Empty
                _TagFlagValue.Tag = value
            End Set
        End Property

        ''' <summary>
        ''' provided to carry additional numeric info about the entity
        ''' </summary>
        ''' <returns></returns>
        Public Property Factor As Double
            Get
                Return _TagFlagValue.Factor
            End Get
            Set(value As Double)
                _TagFlagValue.Factor = value
            End Set
        End Property
        ''' <summary>
        ''' provided to carry additional textual info about the entity
        ''' </summary>
        ''' <returns></returns>
        Public Property Flag As String
            Get
                Return _TagFlagValue.Flag
            End Get
            Set(value As String)
                If value Is Nothing Then value = String.Empty
                _TagFlagValue.Flag = value
            End Set
        End Property
        ''' <summary>
        ''' the dxf group that this entity is associated to
        ''' </summary>
        ''' <returns></returns>

        Public Property GroupName As String
            Get

                Return PropValueStr("*GroupName")
            End Get
            Set(value As String)
                dxfUtils.ValidateGroupName(value, bFixIt:=True)
                SetPropVal("*GroupName", value)

            End Set
        End Property
        Public ReadOnly Property HandlePt As dxfVector
            Get
                Select Case GraphicType
                    Case dxxGraphicTypes.Polyline
                        Return BoundingRectangle.Center
                    Case dxxGraphicTypes.Leader
                        Return Vertices.Item(1, True)
                    Case dxxGraphicTypes.Dimension  ' = 4096
                        Return DefPts.Vector4
                    Case Else
                        Return DefPts.Vector1
                End Select
                Return DefPts.Vector1
            End Get
        End Property
        Public Overrides Property Handle As String
            Get
                '^the handle assigned to the entity for unique identification in DXF code generation
                Return MyBase.Handle
            End Get
            Friend Set(value As String)
                '^the handle assigned to the entity for unique identification in DXF code generation
                MyBase.Handle = value
                Properties.Handle = value

            End Set
        End Property
        Public ReadOnly Property PolylineSegments As colDXFEntities
            Get
                Return New colDXFEntities(PathSegments)
            End Get
        End Property
        Public Property Value As Double
            Get
                '^a user assignable value holder for the entity
                Return _TagFlagValue.Value
            End Get
            Set(value As Double)
                '^a user assignable value holder for the entity
                _TagFlagValue.Value = value
            End Set
        End Property
        Public Overridable Property Vertices As colDXFVectors
            '^the defining vertices of the entity
            Get
                If DefPts.HasVertices Then
                    Dim _rVal As colDXFVectors = DefPts.Vertices
                    _rVal.SetGUIDS(ImageGUID, GUID, BlockGUID, SuppressEvents)
                    _rVal.OwnerPtr = New WeakReference(Me)
                    Return _rVal
                Else
                    UpdatePath()
                    Return BoundingRectangle.Corners()
                End If
            End Get
            Friend Set(value As colDXFVectors)
                If DefPts.HasVertices Then Vertices.Populate(value, True)
            End Set
        End Property
        Public Property X As Double
            '^the world X ordinate of the entity
            Get
                Return HandlePt.X
            End Get
            Set(value As Double)
                Dim curval As Double = X
                If curval = value Then Return
                Translate(New TVECTOR(value - curval, 0, 0))
            End Set
        End Property
        Public ReadOnly Property XDirection As dxfDirection
            Get
                Return New dxfDirection(XDirectionV)
            End Get
        End Property
        Friend ReadOnly Property XDirectionV As TVECTOR
            Get
                Return PlaneV.XDirection
            End Get
        End Property
        Public Property Y As Double
            '^the world Y ordinate of the entity
            Get
                Return HandlePt.Y
            End Get
            Set(value As Double)
                Dim curval As Double = Y
                If curval = value Then Return
                Translate(New TVECTOR(0, value - curval, 0))
            End Set
        End Property
        Public ReadOnly Property YDirection As dxfDirection
            Get
                Return New dxfDirection(YDirectionV)
            End Get
        End Property
        Friend ReadOnly Property YDirectionV As TVECTOR
            Get
                Return PlaneV.YDirection
            End Get
        End Property
        Public Property Z As Double
            '^the world Z ordinate of the entity
            Get
                Return HandlePt.Z
            End Get
            Set(value As Double)
                Dim curval As Double = Z
                If curval = value Then Return
                Translate(New TVECTOR(value - curval, 0, 0))
            End Set
        End Property
        Public ReadOnly Property ZDirection As dxfDirection
            Get
                Return New dxfDirection(ZDirectionV)
            End Get
        End Property
        Friend ReadOnly Property ZDirectionV As TVECTOR
            Get
                Return PlaneV.ZDirection
            End Get
        End Property

        Friend _TagFlagValue As TTAGFLAGVALUE
        Friend Property TagFlagValue As TTAGFLAGVALUE
            Get
                Return _TagFlagValue
            End Get
            Set(value As TTAGFLAGVALUE)
                _TagFlagValue = value
            End Set
        End Property

        Friend _Strings As dxfStrings
        Public Overridable Property Strings As dxfStrings
            Get
                Return _Strings
            End Get
            Friend Set(value As dxfStrings)
                _Strings = value
                If IsText Then
                    _Components.SubStrings = New TSTRINGS(_Strings)
                End If
            End Set
        End Property

        Friend Property Text_Alignment As dxxMTextAlignments
            Get
                Try
                    If GraphicType <> dxxGraphicTypes.MText And GraphicType <> dxxGraphicTypes.Text Then Return dxxMTextAlignments.BaselineLeft
                    If GraphicType = dxxGraphicTypes.Text Then
                        Dim vAlign As dxxTextJustificationsVertical = DirectCast(PropValueI(74), dxxTextJustificationsVertical)
                        Dim hAlign As dxxTextJustificationsHorizontal = DirectCast(PropValueI(72), dxxTextJustificationsHorizontal)
                        Return TFONT.DecodeAlignment(vAlign, hAlign)
                    Else
                        Return DirectCast(PropValueI(71), dxxMTextAlignments)
                    End If
                Catch
                    Return dxxMTextAlignments.BaselineLeft

                End Try

            End Get
            Set(value As dxxMTextAlignments)
                Try
                    If GraphicType <> dxxGraphicTypes.MText And GraphicType <> dxxGraphicTypes.Text Then Return
                    If value < 1 Or value > 14 Then Return

                    If GraphicType = dxxGraphicTypes.Text Then
                        Dim vAlign As dxxTextJustificationsVertical
                        Dim hAlign As dxxTextJustificationsHorizontal

                        TFONT.EncodeAlignment(value, vAlign, hAlign)
                        SetPropVal(74, vAlign, True)
                        SetPropVal(72, hAlign, True)
                    Else
                        SetPropVal(71, value, True)
                    End If
                Catch
                    Return

                End Try
            End Set
        End Property

        Friend _Attributes As dxfAttributes
        Public Overridable Property Attributes As dxfAttributes
            Get
                Return _Attributes
            End Get

            Friend Set(value As dxfAttributes)
                _Attributes = value
            End Set
        End Property

        Friend Property PathEntities As dxfEntities
            Get
                If HasSubEntities Then UpdatePath()

                If _PathEntities IsNot Nothing Then
                    _PathEntities.Owner = GUID
                    _PathEntities.ImageGUID = ImageGUID
                    _PathEntities.Name = "Path Entities"
                End If

                If HasDimStyle Then
                End If
                Return _PathEntities

            End Get
            Set(value As dxfEntities)
                _PathEntities = value
            End Set
        End Property

        Friend _AddSegs As colDXFEntities
        ''' <summary>
        ''' a collection of entities that are included as part of the entities geometry
        ''' </summary>
        ''' <returns></returns>
        ''' 
        Friend Overridable Property AddSegs As colDXFEntities
            Get
                If _AddSegs Is Nothing Then Return Nothing

                _AddSegs.MonitorMembers = True
                _AddSegs.SetGUIDS(aImageGUID:=ImageGUID, aOwnerGUID:=GUID, aBlockGUID:=BlockGUID, aOwnerType:=dxxFileObjectTypes.Entity, aOwner:=Me)
                Return _AddSegs

            End Get
            Set(value As colDXFEntities)
                If value Is Nothing And _AddSegs IsNot Nothing Then
                    _AddSegs.MonitorMembers = False
                    _AddSegs.SetGUIDS("", "", "", dxxFileObjectTypes.Undefined)
                End If
                _AddSegs = value
                If _AddSegs IsNot Nothing Then
                    _AddSegs.MonitorMembers = True
                    _AddSegs.Filter = New List(Of dxxEntityTypes)({dxxGraphicTypes.Arc, dxxGraphicTypes.Line, dxxGraphicTypes.Polyline, dxxGraphicTypes.Solid, dxxGraphicTypes.Point})
                    _AddSegs.SetGUIDS(ImageGUID, GUID, BlockGUID, dxxFileObjectTypes.Entity)
                End If

            End Set
        End Property

#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend MustOverride Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
        Public Overridable Function Clone(Optional bCloneInstances As Boolean = False) As dxfEntity

            '^returns a new object with properties matching those of the cloned object
            Dim _rVal As dxfEntity = Nothing

            Dim statewuz As dxxEntityStates = State

            State = dxxEntityStates.Cloning

            Try

                Select Case GraphicType
                    Case dxxGraphicTypes.Arc
                        _rVal = New dxeArc(DirectCast(Me, dxeArc), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Bezier
                        _rVal = New dxeBezier(DirectCast(Me, dxeBezier), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Dimension
                        _rVal = New dxeDimension(DirectCast(Me, dxeDimension), bCloneInstances:=bCloneInstances)

                    Case dxxGraphicTypes.Ellipse
                        _rVal = New dxeEllipse(DirectCast(Me, dxeEllipse), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.EndBlock
                        _rVal = Nothing
                    Case dxxGraphicTypes.Hatch
                        _rVal = New dxeHatch(DirectCast(Me, dxeHatch), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Hole
                        _rVal = New dxeHole(DirectCast(Me, dxeHole), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Hole
                    Case dxxGraphicTypes.Insert
                        _rVal = New dxeInsert(DirectCast(Me, dxeInsert), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Leader
                        _rVal = New dxeLeader(DirectCast(Me, dxeLeader), bCloneInstances:=bCloneInstances)

                    Case dxxGraphicTypes.Line
                        _rVal = New dxeLine(DirectCast(Me, dxeLine), bCloneInstances:=bCloneInstances)

                    Case dxxGraphicTypes.MText, dxxGraphicTypes.Text
                        _rVal = New dxeText(DirectCast(Me, dxeText), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Point
                        _rVal = New dxePoint(DirectCast(Me, dxePoint), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Polygon
                        _rVal = New dxePolygon(DirectCast(Me, dxePolygon), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Polyline
                        _rVal = New dxePolyline(DirectCast(Me, dxePolyline), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.SequenceEnd
                        _rVal = Nothing
                    Case dxxGraphicTypes.Shape
                        _rVal = New dxeShape(DirectCast(Me, dxeShape), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Solid
                        _rVal = New dxeSolid(DirectCast(Me, dxeSolid), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Symbol
                        _rVal = New dxeSymbol(DirectCast(Me, dxeSymbol), bCloneInstances:=bCloneInstances)
                    Case dxxGraphicTypes.Table
                        _rVal = New dxeTable(DirectCast(Me, dxeTable), bCloneInstances:=bCloneInstances)
                    Case Else
                        _rVal = Nothing
                End Select
            Catch ex As Exception
                Throw ex
            Finally
                If _rVal IsNot Nothing Then
                    _rVal.State = dxxEntityStates.Cloning

                    _rVal.CopyDisplayProps(Me)
                    _rVal.SetPropVal("*SaveToFile", True)
                    _rVal.SetPropVal("*GroupName", "")
                    _rVal.SetPropVal("*GUID", _rVal.GUID)
                    _rVal.SetPropVal("*SourceGUID", GUID)

                    If _MText IsNot Nothing Then
                        _rVal._MText = _MText.Clone()
                    End If
                    If _Insert IsNot Nothing Then
                        _rVal._Insert = New dxeInsert(_Insert)
                    End If

                    If _SubProperties IsNot Nothing Then
                        _rVal.SubProperties = New dxoProperties(_SubProperties)
                    End If

                    _rVal.State = statewuz
                    _rVal.DefPts.IsDirty = True
                    _rVal.SuppressEvents = False

                End If

                State = statewuz
            End Try

            Return _rVal
        End Function

        Public Overridable Sub UpdateProperties()

            '^returns a new object with properties matching those of the cloned object

            If _Properties Is Nothing Then Return
            _Properties.SetDirection(210, ExtrusionDirection)
            Try

                Select Case GraphicType
                    Case dxxGraphicTypes.Arc
                        _Properties.SetVector(10, DefPts.Vector1)
                    Case dxxGraphicTypes.Bezier
                        _Properties.SetVector(10, DefPts.Vector1, aOccur:=1)
                        _Properties.SetVector(10, DefPts.Vector2, aOccur:=2)
                        _Properties.SetVector(10, DefPts.Vector3, aOccur:=3)
                        _Properties.SetVector(10, DefPts.Vector4, aOccur:=4)
                    Case dxxGraphicTypes.Dimension
                    Case dxxGraphicTypes.Ellipse
                        _Properties.SetVector(10, DefPts.Vector1)
                        Dim rad1 As Double = Properties.ValueD("*MajorRadius")
                        Dim rad2 As Double = Properties.ValueD("*MinorRadius")
                        Dim ratio As Double = 1
                        If rad2 <> 0 Then ratio = rad1 / rad2
                        _Properties.SetVector(11, DefPts.Vector1 + Plane.XDirection * rad1)
                        _Properties.SetVal("Ratio", ratio)
                    Case dxxGraphicTypes.EndBlock
                    Case dxxGraphicTypes.Hatch
                    Case dxxGraphicTypes.Hole
                        _Properties.SetVector(10, DefPts.Vector1)
                        _Properties.SetDirection(211, XDirection)
                    Case dxxGraphicTypes.Insert
                    Case dxxGraphicTypes.Leader
                    Case dxxGraphicTypes.Line
                        _Properties.SetVector(10, DefPts.Vector1)
                        _Properties.SetVector(11, DefPts.Vector2)
                    Case dxxGraphicTypes.MText
                        _Properties.SetVector(10, DefPts.Vector1)
                        _Properties.SetVector(11, DefPts.Vector2)
                    Case dxxGraphicTypes.Text
                        _Properties.SetVector(10, DefPts.Vector1)
                        _Properties.SetVector(11, DefPts.Vector2)
                    Case dxxGraphicTypes.Point
                        _Properties.SetVector(10, DefPts.Vector1)
                    Case dxxGraphicTypes.Polygon
                        _Properties.SetVector(10, DefPts.Vector1)
                    Case dxxGraphicTypes.Polyline
                    Case dxxGraphicTypes.SequenceEnd
                    Case dxxGraphicTypes.Shape
                        _Properties.SetVector(10, DefPts.Vector1)
                    Case dxxGraphicTypes.Solid
                        _Properties.SetVector(10, DefPts.Vector1)
                        _Properties.SetVector(11, DefPts.Vector2)
                        _Properties.SetVector(12, DefPts.Vector3)
                        _Properties.SetVector(13, DefPts.Vector4)

                    Case dxxGraphicTypes.Symbol
                        _Properties.SetVector(10, DefPts.Vector1)
                    Case dxxGraphicTypes.Table
                        _Properties.SetVector(10, DefPts.Vector1)
                    Case Else
                End Select
            Catch ex As Exception
                Throw ex
            Finally
                If _MText IsNot Nothing Then _MText.UpdateProperties()
                If _Insert IsNot Nothing Then _Insert.UpdateProperties()
            End Try

        End Sub

#End Region 'MustOverride Entity Methods
#Region "Methods"

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        ''' <returns></returns>
        Public Function TransferedToPlane(aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing) As dxfEntity
            Dim _rVal As dxfEntity = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal

        End Function
        ''' <summary>
        ''' redefines the entity defining vectors using the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane">the destination plane</param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        Public Overridable Sub TransferToPlane(aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing)
            If dxfPlane.IsNull(aPlane) Then Return
            If aFromPlane Is Nothing Then
                aFromPlane = Plane
                If GraphicType = dxxGraphicTypes.Polyline Then
                    aFromPlane.Origin = Vertices.PlanarCenter(aFromPlane)
                End If

            End If
            DefPts.TransferToPlane(aPlane, aFromPlane)

            Dim persistents As List(Of dxfEntity) = PersistentSubEntities()
            If persistents IsNot Nothing Then
                If persistents.Count > 0 Then
                    dxfEntities.TransferEntitiesToPlane(persistents, aPlane, aFromPlane)
                End If

            End If
            If _AddSegs IsNot Nothing And persistents IsNot _AddSegs Then
                If _AddSegs.Count > 0 Then
                    _AddSegs.TransferToPlane(aPlane, aFromPlane)
                End If

            End If
                If _PathEntities IsNot Nothing Then _PathEntities.TransferToPlane(aPlane, aFromPlane)
            Dim reactor As dxfEntity = ReactorEntity
            If reactor IsNot Nothing Then ReactorEntity.TransferToPlane(aPlane, aFromPlane)
            IsDirty = True
        End Sub

        Friend Sub Copy(aEntity As dxfEntity, Optional bCloneInstances As Boolean = False)
            If aEntity Is Nothing Then Return
            If aEntity.GraphicType <> GraphicType Then Return
            State = dxxEntityStates.Cloning
            Try
                Properties.CopyVals(aEntity.Properties, aNamesToSkip:=New List(Of String)({"*GUID", "SaveToFile", "*GroupName"}), bSkipHandles:=True, bSkipPointers:=True)
                Properties.GUID = GUID
                DefPts.Copy(aEntity)
                TagFlagValue = New TTAGFLAGVALUE(aEntity.TagFlagValue)
                ExtendedData = New dxoPropertyArray(aEntity.ExtendedData)
                Components = New TCOMPONENTS(aEntity.Components, GUID)

                _PathEntities = New dxfEntities(aEntity._PathEntities, True)
                If aEntity.SubProperties IsNot Nothing Then SubProperties = New dxoProperties(aEntity.SubProperties)
                If aEntity.Attributes IsNot Nothing Then _Attributes = New dxfAttributes(aEntity._Attributes)
                If aEntity._Cells IsNot Nothing Then _Cells = New dxoTableCells(aEntity._Cells)
                If aEntity._Strings IsNot Nothing Then _Strings = New dxfStrings(aEntity._Strings)
                If aEntity._AddSegs IsNot Nothing Then
                    _AddSegs = New colDXFEntities(aEntity._AddSegs)
                End If
                If aEntity._DimStyle IsNot Nothing Then _DimStyle = New dxoDimStyle(aEntity._DimStyle)

                If bCloneInstances Then Instances.Copy(aEntity.Instances, True)
            Finally
                State = dxxEntityStates.Steady
            End Try





        End Sub

        Friend Sub SetProps(aProps As TPROPERTIES, Optional bCopyNewMembers As Boolean = True)
            _Properties.CopyValues(aProps, bCopyNewMembers)
        End Sub

        ''' <summary>
        ''' returns a properties object loaded with the entities current properties
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function ActiveProperties() As dxoProperties

            Properties.Handle = Handle
            UpdateCommonProperties(bUpdateProperties:=True)

            Return New dxoProperties(Properties)

        End Function

        Friend Sub UpdateStructure()
            If HasDimStyle Then
                _Style = New TTABLEENTRY(DimStyle)
                _DimStyleStructure = _Style
                _TextStyle.Name = DimStyle.TextStyleName
            End If
            If GraphicType = dxxGraphicTypes.Insert Or GraphicType = dxxGraphicTypes.Leader Or GraphicType = dxxGraphicTypes.Polygon Then
                SetPropVal("*SourceBlockGUID", BlockGUID)
            End If

            _Instances.Plane = PlaneV
        End Sub


        Friend Overridable Sub SavePaths(aPaths As TPATHS, aSegments As TSEGMENTS, Optional aReactorEntity As dxfEntity = Nothing, Optional aPathEntities As dxfEntities = Nothing)

            State = dxxEntityStates.GeneratingPath
            aPaths.EntityGUID = GUID
            aPaths.GraphicType = GraphicType
            aPaths.Identifier = Identifier
            'If Me.Instances.Count > 0 Then
            '    aPaths = Instances.Apply(aPaths)
            'End If

            _Components.Paths = aPaths
            _Components.Segments = aSegments


            State = dxxEntityStates.Steady
            If HasSubEntities Then
                PathEntities = aPathEntities
            End If
            IsDirty = False
        End Sub
        Friend Function GetStructure() As TENTITY
            UpdateStructure()
            Return New TENTITY(Me)
        End Function
        Friend Sub SetStructure(aStructure As TENTITY)


            If aStructure.GraphicType <> GraphicType Then Return
            If aStructure.Props.Count <= 0 Then Return
            MyBase.HStrukture = New THANDLES(aStructure.Handlez, GUID)

            Instances.Copy(aStructure.Instances, bSuppressPlane:=True)

            If HasDimStyle Then
                If _DimStyle Is Nothing Then
                    _DimStyle = New dxoDimStyle(aStructure.DimStyle)

                Else
                    _DimStyle.Properties.CopyVals(aStructure.DimStyle)
                End If

            End If

            _ExtendedData = New dxoPropertyArray(aStructure.ExtendedData)
            _Components = New TCOMPONENTS(aStructure.Components)
            _Properties = New dxoProperties(aStructure.Props, "Properties")
            _Reactors = New dxoPropertyArray(aStructure.Reactors)
            _Style = New TTABLEENTRY(aStructure.Style)
            _TextStyle = New TTABLEENTRY(aStructure.TextStyle)
            If aStructure.SubProps.Count > 0 Then SubProperties = New dxoProperties(aStructure.SubProps)


            _TagFlagValue = New TTAGFLAGVALUE(aStructure.TagFlagValue)


            Dim aGUID As String = GUID

            MyBase.GUID = aGUID
            Properties.GUID = aGUID

            If HasDimStyle Then
                If _DimStyle Is Nothing Then _DimStyle = New dxoDimStyle(_DimStyleStructure) Else _DimStyle.Properties.CopyVals(_DimStyleStructure)
            End If
            _DefPts = New dxpDefPoints(aStructure.DefPts)

        End Sub

        Public Overridable Function DefiningVectors() As colDXFVectors

            Return DefPts.DefiningVectors()

        End Function




        Public Overridable Function DefinitionPoint(aVectorType As dxxEntDefPointTypes) As dxfVector
            Dim _rVal As dxfVector
            If aVectorType > 100 Then
                Dim ival As Integer = Convert.ToInt32(aVectorType) - 100
                Dim algn As dxxRectangularAlignments = CType(ival, dxxRectangularAlignments)
                _rVal = BoundingRectangle.GripPoint(algn)
            Else
                Select Case GraphicType
                    Case dxxGraphicTypes.Point  ' = 1
                        _rVal = DefPts.Vector1
                    Case dxxGraphicTypes.Line  ' = 2
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = DefPts.Vector2
                            Case Else  'retrun the midpt
                                _rVal = DefPts.Vector1
                                _rVal = _rVal.MidPoint(DefPts.Vector2)
                        End Select
                    Case dxxGraphicTypes.Arc  ' = 4
                        Dim rad As Double = PropValueD("Radius")
                        Dim sa As Double = PropValueD("Start Angle")
                        Dim ea As Double = PropValueD("End Angle")
                        Dim sp As TVECTOR = PlaneV.AngleVector(sa, rad, False)
                        Dim ep As TVECTOR = PlaneV.AngleVector(ea, rad, False)
                        Dim cw As Boolean = PropValueB("*Clockwise")
                        Dim ctr As TVECTOR = DefPts.VectorGet(1)
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = New dxfVector(sp)
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = New dxfVector(ep)
                            Case dxxEntDefPointTypes.MidPt
                                _rVal = New dxfVector(ArcStructure.MidPoint)
                            Case Else
                                _rVal = DefPts.Vector1  'the center
                        End Select
                    Case dxxGraphicTypes.Ellipse  ' = 8
                        Dim ctr As TVECTOR = DefPts.VectorGet(1)
                        Dim mjrad As Double = PropValueD("*MajorRadius")
                        Dim mnrad As Double = PropValueD("*MinorRadius")
                        Dim cw As Boolean = PropValueB("*Clockwise")
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = New dxfVector(dxfUtils.EllipsePoint(ctr, 2 * mjrad, 2 * mnrad, PropValueD("Start Angle"), PlaneV))
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = New dxfVector(dxfUtils.EllipsePoint(ctr, 2 * mjrad, 2 * mnrad, PropValueD("End Angle"), PlaneV))
                            Case dxxEntDefPointTypes.MidPt
                                _rVal = New dxfVector(Me.ArcStructure.Plane.Origin)
                            Case Else
                                _rVal = DefPts.Vector1  'the center
                        End Select
                    Case dxxGraphicTypes.Bezier  ' = 16
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = DefPts.Vector4
                            Case Else
                                Dim aB As dxeBezier = Me
                                _rVal = aB.MidPt   'midpoint or center
                        End Select
                    Case dxxGraphicTypes.Solid  ' = 32
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = DefPts.Vector4
                            Case Else
                                _rVal = BoundingRectangle.Center
                        End Select
                    Case dxxGraphicTypes.Shape  ' = 128
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.MidPt, dxxEntDefPointTypes.Center
                                _rVal = BoundingRectangle.Center
                            Case Else
                                _rVal = DefPts.Vector1
                        End Select
                    Case dxxGraphicTypes.Polyline
                        Dim aSeg As dxfEntity
                        Dim aSegs As New colDXFEntities(PathSegments)
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                If Vertices.Count > 0 Then _rVal = Vertices.Item(1) Else _rVal = HandlePt
                            Case dxxEntDefPointTypes.EndPt
                                If aSegs.Count > 0 Then
                                    aSeg = aSegs.LastMember
                                    _rVal = aSeg.DefinitionPoint(dxxEntDefPointTypes.EndPt)
                                Else
                                    _rVal = HandlePt
                                End If
                            Case dxxEntDefPointTypes.Center
                                _rVal = BoundingRectangle.Center
                            Case Else
                                _rVal = BoundingRectangle.Center
                        End Select
                    Case dxxGraphicTypes.Insert  ' = 1024
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.MidPt, dxxEntDefPointTypes.Center
                                _rVal = BoundingRectangle.Center
                            Case Else
                                _rVal = DefPts.Vector1
                        End Select
                    Case dxxGraphicTypes.MText, dxxGraphicTypes.Text


                        UpdatePath()
                        Dim trec As dxoCharBox = Strings.CharBox

                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = New dxfVector(trec.BasePtV)
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = New dxfVector(trec.PointV(dxxRectanglePts.BaselineRight))
                            Case dxxEntDefPointTypes.MidPt
                                _rVal = New dxfVector(trec.PointV(dxxRectanglePts.BaselineCenter))
                            Case dxxEntDefPointTypes.Center
                                _rVal = BoundingRectangle.Center
                            Case Else
                                _rVal = DefPts.Vector1
                        End Select
                    Case dxxGraphicTypes.Leader
                        UpdatePath()
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = Vertices.FirstVector
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = Vertices.LastVector
                            Case dxxEntDefPointTypes.MidPt
                                _rVal = Vertices.MidPoint
                            Case dxxEntDefPointTypes.Center
                                _rVal = BoundingRectangle.Center
                            Case Else
                                _rVal = Vertices.FirstVector
                        End Select
                    Case dxxGraphicTypes.Polygon
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                If Vertices.Count > 0 Then _rVal = Vertices.Item(1) Else _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.EndPt
                                Dim aSegs As New colDXFEntities(PathSegments)
                                Dim aSeg As dxfEntity
                                aSeg = aSegs.LastMember
                                If aSeg IsNot Nothing Then _rVal = aSeg.DefinitionPoint(dxxEntDefPointTypes.EndPt) Else _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.MidPt
                                Dim aSegs As New colDXFEntities(PathSegments)
                                If aSegs.Count > 0 Then
                                    _rVal = New dxfVector With {.Strukture = dxfEntity.SegmentPoint(aSegs, 0.5)}
                                Else
                                    _rVal = DefPts.Vector1
                                End If
                            Case dxxEntDefPointTypes.Center
                                _rVal = BoundingRectangle.Center
                            Case Else
                                _rVal = DefPts.Vector1
                        End Select
                    Case dxxGraphicTypes.Dimension  ' = 4096
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = DefPts.Vector2
                            Case Else
                                _rVal = BoundingRectangle.Center
                        End Select
                        'Case dxxGraphicTypes.Hatch  ' = 819
                        'Case dxxGraphicTypes.Hole  ' = 16384
                        'Case dxxGraphicTypes.Symbol  ' = 32768
                        'Case dxxGraphicTypes.Table  ' = 65536
                    Case Else
                        Select Case aVectorType
                            Case dxxEntDefPointTypes.StartPt
                                _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.EndPt
                                _rVal = DefPts.Vector1
                            Case dxxEntDefPointTypes.MidPt
                                _rVal = BoundingRectangle.Center
                            Case dxxEntDefPointTypes.Center
                                _rVal = BoundingRectangle.Center
                            Case Else
                                _rVal = DefPts.Vector1
                        End Select
                End Select
            End If
            If _rVal IsNot Nothing Then
                _rVal.Tag = Tag
                _rVal.Flag = Flag
                _rVal.Value = Value
                _rVal.Row = Row
                _rVal.Col = Col
                _rVal.GUID = GUID
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the width of the entity's bound rectangle
        ''' </summary>
        ''' '''<remarks>if the plane is not passed the entity's definition plane is assumed</remarks>
        ''' <param name="aPlane">an optional plane to get the rectangle on</param>
        ''' <param name="bIncludeInstances">flag to include all the instances defined for then entity in the returned rectangle</param>
        ''' <returns></returns>
        Public Overridable Function Width(Optional aPlane As dxfPlane = Nothing, Optional bIncludeInstances As Boolean = False) As Double
            Return BoundingRectangle(aPlane, bIncludeInstances).Width
        End Function

        ''' <summary>
        ''' returns the height of the entity's bound rectangle
        ''' </summary>
        ''' '''<remarks>if the plane is not passed the entity's definition plane is assumed</remarks>
        ''' <param name="aPlane">an optional plane to get the rectangle on</param>
        ''' <param name="bIncludeInstances">flag to include all the instances defined for then entity in the returned rectangle</param>
        ''' <returns></returns>
        Public Overridable Function Height(Optional aPlane As dxfPlane = Nothing, Optional bIncludeInstances As Boolean = False) As Double
            Return BoundingRectangle(aPlane, bIncludeInstances).Height
        End Function

        ''' <summary>
        ''' returns the  entity's bound rectangle
        ''' </summary>
        ''' '''<remarks>if the plane is not passed the entity's definition plane is assumed</remarks>
        ''' <param name="aPlane">an optional plane to get the rectangle on</param>
        ''' <param name="bIncludeInstances">flag to include all the instances defined for then entity in the returned rectangle</param>
        Public Overridable Function BoundingRectangle(Optional aPlane As dxfPlane = Nothing, Optional bIncludeInstances As Boolean = False) As dxfRectangle
            Dim aPl As TPLANE = PlaneV
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane)
            Return New dxfRectangle(BoundingRectangle(aPl, bIncludeInstances))
        End Function

        ''' <summary>
        ''' returns the  entity's bound rectangle
        ''' </summary>
        ''' '''<remarks>if the plane is not passed the entity's definition plane is assumed</remarks>
        ''' <param name="aPlane">an optional plane to get the rectangle on</param>
        ''' <param name="bIncludeInstances">flag to include all the instances defined for then entity in the returned rectangle</param>
        Friend Overridable Function BoundingRectangle(aPlane As TPLANE, Optional bIncludeInstances As Boolean = False) As TPLANE
            If TPLANE.IsNull(aPlane) Then aPlane = PlaneV
            Dim extpts As TVECTORS = ExtentPts(Not bIncludeInstances)
            Return extpts.Bounds(aPlane)
        End Function

        Friend Function PropVector(aName As String) As TVECTOR
            Return Properties.ValueV(aName)
        End Function
        Friend Sub PropVectorSet(aName As String, value As TVECTOR)
            Properties.SetVector(aName, value)
        End Sub

        Friend Sub PropVectorSet(aName As String, value As iVector)
            Properties.SetVector(aName, value)
        End Sub

        Public Function Segmented(Optional aCurveDivisions As Integer = 0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            Try
                Dim dsp As dxfDisplaySettings
                If aDisplaySettings Is Nothing Then dsp = DisplaySettings Else dsp = aDisplaySettings
                Dim eSegs As TSEGMENTS = IntersectionSegments(aCurveDivisions)
                For i As Integer = 1 To eSegs.Count
                    _rVal.AddArcLineV(eSegs.Item(i), dsp)
                Next i
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Public Overridable Function SetPropVal(aPropName As String, aValue As Object, Optional bDirtyOnChange As Boolean = False, Optional aOccurance As Integer = 1) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(aPropName, prop, aOccurance) Then

                If dxfUtils.RunningInIDE Then
                    ' If String.Compare(aPropName, "LTScale", True) <> 0 And String.Compare(aPropName, "LineWeight", True) <> 0 Then
                    If aOccurance <= 1 Then Debug.WriteLine($"Property '{aPropName}' Not Found") Else Debug.WriteLine($"Property '{aPropName}' OCCR:{aOccurance} Not Found")
                    'End If

                End If
                Return False
            Else
                Return SetPropVal(prop, aValue, bDirtyOnChange)
            End If
        End Function
        Public Overridable Function SetPropVal(aGroupCode As Integer, aValue As Object, Optional bDirtyOnChange As Boolean = False, Optional aOccurance As Integer = 1) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(aGroupCode, prop, aOccurance) Then
                If dxfUtils.RunningInIDE Then
                    If aOccurance <= 1 Then Debug.WriteLine($"Property '{aGroupCode}' Not Found") Else Debug.WriteLine($"Property '{aGroupCode}' OCCR:{aOccurance} Not Found")
                End If
                Return False
            Else
                Return SetPropVal(prop, aValue, bDirtyOnChange)
            End If

        End Function

        Friend Overridable Function SetPropVal(aProp As dxoProperty, aValue As Object, Optional bDirtyOnChange As Boolean = False) As Boolean
            If aProp Is Nothing Then Return False

            Dim _rVal As Boolean = aProp.SetVal(aValue)
            If _rVal Then
                Dim evnt As New dxfEntityEvent(Me, dxxEntityEventTypes.PropertyValue, CollectionGUID, ImageGUID, BlockGUID, GUID, New dxoProperty(aProp)) With {.DirtyOnChange = bDirtyOnChange}
                RaisePropertyChangeEvent(evnt)
                SetDependantPropVal(aProp)


            End If
            Return _rVal
        End Function

        Private Sub SetDependantPropVal(prop As dxoProperty)
            If prop Is Nothing Then Return

            Select Case prop.Name.ToUpper()
                Case "*GROUPNAME", "GROUPNAME"
                    Dim gname As String = prop.ValueS
                    If _MText IsNot Nothing Then _MText.GroupName = gname
                    If _Insert IsNot Nothing Then _Insert.GroupName = gname
                    If _AddSegs IsNot Nothing Then
                        For Each ent As dxfEntity In _AddSegs
                            ent.GroupName = gname
                        Next
                    End If
            End Select
            If IsText Then
                If prop.GroupCode = 7 Then ' text style name (7 for MText or DText
                    _TextStyle.Name = prop.ValueS
                    If State = dxxEntityStates.Steady Then
                        Dim myImage As dxfImage = Image
                        If myImage IsNot Nothing Then
                            _TextStyle = New TTABLEENTRY(myImage.TextStyle(prop.ValueS, True)) With {.IsCopied = True}
                        End If
                    End If
                End If

            End If
        End Sub


        Private Function RaisePropertyChangeEvent(aEvent As dxfEntityEvent) As Boolean
            If SuppressEvents Or aEvent Is Nothing Or State <> dxxEntityStates.Steady Then Return False
            Dim prop As dxoProperty = aEvent.SourceProperty
            If String.IsNullOrWhiteSpace(prop.Name) Then Return False
            aEvent.Entity = Me
            RaiseEvent PropertyChangeEvent(aEvent)
            Dim rUndo As Boolean = aEvent.Undo
            If Not rUndo And Not String.IsNullOrWhiteSpace(aEvent.CollectionGUID) Then
                Dim mycol As colDXFEntities = myCollection
                If mycol IsNot Nothing Then
                    mycol.RaiseEntitiesMemberChange(aEvent)
                    rUndo = aEvent.Undo
                End If
            End If
            If rUndo Then
                Properties.SetVal(prop.GroupCode, prop.LastValue)
            Else
                If aEvent.DirtyOnChange Then IsDirty = True


            End If
            Return rUndo
        End Function
        Public Function SetDisplayProperty(aVariableType As dxxDisplayProperties, aNewValue As Object, Optional aMatchValue As String = Nothing) As Boolean
            Dim _rVal As Boolean = False
            '#1the name of the display variable to affect (LayerName, Color, Linetype etc.)
            '#2the new value for the  display variable
            '#3a variable value to match
            '#4flag to set any undefined variable to the new value
            '^sets the members indicated display variable to the new value
            '~if a seach value is passed then only members with a current value equal to the search value will be affected.
            '~returns the affected members.
            Dim sValue As String
            Dim sngValue As Double
            Dim bValue As Boolean
            'Dim bNoPaths As Boolean
            Dim lastVal As Object
            'Dim nval As Object
            Dim iVal As Integer
            Dim slist As New TLIST
            If aMatchValue IsNot Nothing Then slist = New TLIST(",", aMatchValue)
            Select Case aVariableType
                Case dxxDisplayProperties.Color
                    If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                    iVal = TVALUES.To_INT(aNewValue)
                    If iVal = -1 Then Return _rVal
                    'nval = iVal
                    lastVal = Color
                    If slist.ContainsNumber(lastVal, True, 0) Then
                        If Color <> iVal Then
                            _rVal = True
                            Color = iVal
                        End If
                    End If
                Case dxxDisplayProperties.LineWeight
                    If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                    iVal = TVALUES.To_INT(aNewValue)
                    'nval = iVal
                    If iVal < dxxLineWeights.ByBlock Or iVal > dxxLineWeights.LW_211 Then Return _rVal
                    lastVal = LineWeight
                    If slist.ContainsNumber(lastVal, True, 0) Then
                        If LineWeight <> iVal Then
                            _rVal = True
                            LineWeight = iVal
                        End If
                    End If
                Case dxxDisplayProperties.LTScale
                    If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                    sngValue = TVALUES.To_DBL(aNewValue, False, 6)
                    'nval = sngValue
                    If sngValue < 0 Then Return _rVal
                    lastVal = LTScale
                    If slist.ContainsNumber(lastVal, True, 6) Then
                        If Math.Round(LTScale, 6) <> sngValue Then
                            _rVal = True
                            LTScale = sngValue
                        End If
                    End If
                Case dxxDisplayProperties.LayerName
                    sValue = aNewValue.ToString().Trim()
                    If sValue = String.Empty Then sValue = "0"
                    'nval = sValue
                    lastVal = LayerName
                    If slist.Contains(lastVal, True) Then
                        If String.Compare(lastVal, sValue, True) <> 0 Then
                            _rVal = True
                            LayerName = sValue
                        End If
                    End If
                Case dxxDisplayProperties.Linetype
                    'bNoPaths = GraphicType = dxxGraphicTypes.Text Or GraphicType = dxxGraphicTypes.Point
                    sValue = aNewValue.ToString().Trim()
                    If sValue = String.Empty Then sValue = dxfLinetypes.ByLayer
                    'nval = sValue
                    lastVal = Linetype
                    If slist.Contains(lastVal, True) Then
                        If String.Compare(lastVal, sValue, True) <> 0 Then
                            _rVal = True
                            Linetype = sValue
                        End If
                    End If
                Case dxxDisplayProperties.Suppressed
                    If Not TVALUES.IsBoolean(aNewValue) Then Return _rVal
                    bValue = TVALUES.ToBoolean(aNewValue)
                    'nval = bValue
                    lastVal = Suppressed
                    If slist.Contains(lastVal, True) Then
                        If lastVal <> bValue Then
                            _rVal = True
                            Suppressed = bValue
                        End If
                    End If
                Case dxxDisplayProperties.TextStyle
                    If Not IsText Or HasDimStyle Then Return _rVal
                    sValue = aNewValue.ToString().Trim()
                    'bNoPaths = True
                    If sValue = String.Empty Then sValue = "Standard"
                    'nval = sValue
                    lastVal = TextStyleName
                    If lastVal <> "" Then
                        If slist.Contains(lastVal, True) Then
                            If String.Compare(lastVal, sValue, True) <> 0 Then
                                _rVal = True
                                IsDirty = True
                                TextStyleName = sValue
                            End If
                        End If
                    End If
                Case dxxDisplayProperties.DimStyle
                    If Not HasDimStyle Then Return _rVal
                    sValue = aNewValue.ToString().Trim()
                    If sValue = String.Empty Then sValue = "Standard"
                    'nval = sValue
                    lastVal = DimStyleName
                    If lastVal <> "" Then
                        If slist.Contains(lastVal, True) Then
                            If String.Compare(lastVal, sValue, True) <> 0 Then
                                _rVal = True
                                IsDirty = True
                                DimStyleName = sValue
                            End If
                        End If
                    End If
            End Select
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Dim hp As dxfVector = HandlePt
            Dim _rVal As String = $"dxe{EntityTypeName} - [{hp.X:#,0.000#},{hp.Y:#,0.000#}"
            If hp.Z <> 0 Then _rVal += $",{hp.Z:#,0.000#}]" Else _rVal += $"]"
            If GraphicType = dxxGraphicTypes.Line Then
                hp = DefPts.Vector2
                _rVal = $"{_rVal} => [{hp.X:#,0.000#},{hp.Y:#,0.000#}"
                If hp.Z <> 0 Then _rVal += $",{hp.Z:#,0.000#}]" Else _rVal += $"]"
            ElseIf GraphicType = dxxGraphicTypes.Polygon Or GraphicType = dxxGraphicTypes.Symbol Then
                Dim bname As String = PropValueStr("*BlockName")
                If String.IsNullOrEmpty(bname) Then bname = Name
                If Not String.IsNullOrWhiteSpace(bname) Then _rVal += $" - {bname}"
            ElseIf GraphicType = dxxGraphicTypes.Insert Then
                Dim bname As String = Properties.ValueS(2)
                If Not String.IsNullOrWhiteSpace(bname) Then _rVal += $" - {bname}"
            End If
            _rVal += $" {_TagFlagValue.TagAndFlag(":").Trim()}"


            Return _rVal
        End Function

        Public Function TagFlag() As String
            Return _TagFlagValue.TagAndFlag(":")

        End Function

        Public Function SetCoordinates(Optional aNewX As Double? = Nothing, Optional aNewY As Double? = Nothing, Optional aNewZ As Double? = Nothing, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the value to set the X coordinate to
            '#2the value to set the Y coordinate to
            '#3the value to set the Z coordinate to
            '^sets the X,Y and Z components of the vector to the passed values
            '~unpassed or non-numeric values are ignored
            Dim vOld As New TVECTOR(X, Y, Z)
            Dim vNew As TVECTOR = vOld.Clone
            _rVal = vNew.Update(aNewX, aNewY, aNewZ)
            If Not _rVal Then Return False
            Return Translate(vNew - vOld)
        End Function
        Friend Function IntersectionSegments(aCurveDivisions As Integer) As TSEGMENTS
            Return dxfIntersections.IntersectionSegments(Me, aCurveDivisions, False)
        End Function
        Friend Function ArcLineStructure(Optional bReturnInfinite As Boolean = False) As TSEGMENT
            Dim _rVal As New TSEGMENT
            If Properties.Count < dxfGlobals.CommonProps Then Return _rVal
            Dim gType As dxxGraphicTypes = GraphicType
            If gType <> dxxGraphicTypes.Arc And gType <> dxxGraphicTypes.Ellipse And gType <> dxxGraphicTypes.Line Then Return _rVal
            _rVal.INFINITE = bReturnInfinite
            _rVal.Flag = Flag
            _rVal.Tag = Tag
            _rVal.Value = Value
            _rVal.DisplayStructure = DisplayStructure
            _rVal.Identifier = Identifier
            _rVal.OwnerGUID = OwnerGUID
            Select Case gType
                Case dxxGraphicTypes.Arc
                    _rVal.IsArc = True
                    _rVal.ArcStructure.ClockWise = PropValueB("*Clockwise")
                    _rVal.ArcStructure.Elliptical = False
                    _rVal.ArcStructure.Radius = PropValueD("Radius", 1)
                    _rVal.ArcStructure.Plane = PlaneV
                    If bReturnInfinite Then
                        _rVal.ArcStructure.StartAngle = 0
                        _rVal.ArcStructure.EndAngle = 360
                        '    _rVal.ArcStructure.SpannedAngle = 360
                        '    _rVal.ArcStructure.StartPt = .PLane.AngleVector( 0, _rVal.ArcStructure.Radius, False)
                        '    _rVal.ArcStructure.EndPt = _rVal.ArcStructure.StartPt
                    Else
                        _rVal.ArcStructure.StartAngle = PropValueD("Start Angle")
                        _rVal.ArcStructure.EndAngle = PropValueD("End Angle")
                        _rVal.ArcStructure.StartWidth = PropValueD("*StartWidth")
                        _rVal.ArcStructure.EndWidth = PropValueD("*EndWidth")
                        If _rVal.ArcStructure.SpannedAngle >= 359.99 Then
                            _rVal.INFINITE = True
                        End If
                    End If
                Case dxxGraphicTypes.Ellipse
                    _rVal.IsArc = True
                    _rVal.ArcStructure.Plane = PlaneV
                    _rVal.ArcStructure.ClockWise = PropValueB("*Clockwise")
                    _rVal.ArcStructure.Elliptical = True
                    _rVal.ArcStructure.Radius = PropValueD("*MajorRadius")
                    _rVal.ArcStructure.MinorRadius = PropValueD("*MinorRadius")
                    If bReturnInfinite Then
                        _rVal.ArcStructure.StartAngle = 0
                        _rVal.ArcStructure.EndAngle = 360
                        '    _rVal.ArcStructure.SpannedAngle = 360
                        '    _rVal.ArcStructure.StartPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, 0, _rVal.ArcStructure.Plane)
                        '    _rVal.ArcStructure.EndPt = _rVal.ArcStructure.StartPt
                    Else
                        _rVal.ArcStructure.StartAngle = PropValueD("*StartAngle")
                        _rVal.ArcStructure.EndAngle = PropValueD("*EndAngle")
                        '_rVal.ArcStructure.SpannedAngle = dxfMath.SpannedAngle(_rVal.ArcStructure.ClockWise, _rVal.ArcStructure.StartAngle, _rVal.ArcStructure.EndAngle)
                        '_rVal.ArcStructure.StartPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, _rVal.ArcStructure.StartAngle, _rVal.ArcStructure.Plane)
                        '_rVal.ArcStructure.EndPt = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, _rVal.ArcStructure.EndAngle, _rVal.ArcStructure.Plane)
                        If _rVal.ArcStructure.SpannedAngle >= 359.99 Then
                            _rVal.INFINITE = True
                        End If
                    End If
                Case dxxGraphicTypes.Line
                    _rVal.IsArc = False
                    _rVal.INFINITE = bReturnInfinite
                    _rVal.LineStructure.SPT = DefPts.VectorGet(1)
                    _rVal.LineStructure.EPT = DefPts.VectorGet(2)
                    _rVal.LineStructure.StartWidth = PropValueD("*StartWidth")
                    _rVal.LineStructure.EndWidth = PropValueD("*EndWidth")
            End Select
            Return _rVal
        End Function
        Friend Sub SetArcLineStructure(value As TSEGMENT)
            Dim gType As dxxGraphicTypes = GraphicType
            If gType <> dxxGraphicTypes.Arc And gType <> dxxGraphicTypes.Ellipse And gType <> dxxGraphicTypes.Line Then Return
            'bReturnInfinite = value.INFINITE
            Flag = value.Flag
            Tag = value.Tag
            Me.Value = value.Value
            DisplayStructure = value.DisplayStructure
            Identifier = value.Identifier
            OwnerGUID = value.OwnerGUID
            Select Case gType
                Case dxxGraphicTypes.Arc
                    If Not value.IsArc Then Return
                    If value.ArcStructure.Elliptical Then Return
                    ArcStructure = value.ArcStructure
                Case dxxGraphicTypes.Ellipse
                    If Not value.IsArc Then Return
                    If Not value.ArcStructure.Elliptical Then Return
                    ArcStructure = value.ArcStructure
                Case dxxGraphicTypes.Line
                    If value.IsArc Then Return
                    LineStructure = value.LineStructure
            End Select
        End Sub
        Friend Function Handlez_Get() As THANDLES

            Return MyBase.HStrukture
        End Function
        Friend Sub Handlez_Set(aHandles As THANDLES)
            MyBase.HStrukture = New THANDLES(aHandles, GUID)

        End Sub
        Public Overridable Function GetVertices(Optional bReturnClones As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            Select Case GraphicType
                Case dxxGraphicTypes.Arc
                    _rVal = New colDXFVectors(ArcStructure.QuadrantPoints)
                    bReturnClones = False
                Case dxxGraphicTypes.Bezier
                    _rVal.Add(DefPts.Vector1)
                    _rVal.Add(DefPts.Vector2)
                    _rVal.Add(DefPts.Vector3)
                    _rVal.Add(DefPts.Vector4)
                Case dxxGraphicTypes.Dimension
                    _rVal.Add(DefPts.Vector1)
                    _rVal.Add(DefPts.Vector2)
                    _rVal.Add(DefPts.Vector3)
                    _rVal.Add(DefPts.Vector4)
                    _rVal.Add(DefPts.Vector5)
                    _rVal.Add(DefPts.Vector6)
                    _rVal.Add(DefPts.Vector7)
                Case dxxGraphicTypes.Ellipse
                    _rVal = New colDXFVectors(ArcStructure.QuadrantPoints)
                    bReturnClones = False
                'Case dxxGraphicTypes.Hatch
                '    _rVal = BoundingRectangle.Corners()
                '    bReturnClones = False
                'Case dxxGraphicTypes.Hole
                '    _rVal = BoundingRectangle.Corners()
                '    bReturnClones = False
                'Case dxxGraphicTypes.Insert
                '    _rVal = BoundingRectangle.Corners()
                '    bReturnClones = False
                Case dxxGraphicTypes.Leader
                    If bReturnClones Then _rVal = Vertices.Clone Else _rVal = Vertices
                    Dim subents As List(Of dxfEntity) = PersistentSubEntities()
                    If subents IsNot Nothing Then
                        If subents.Count > 0 Then
                            Dim aEnt As dxfEntity = subents.Item(0)
                            _rVal.Append(subents.Item(0).GetVertices(bReturnClones))
                        End If
                    End If
                    bReturnClones = False
                Case dxxGraphicTypes.Line
                    _rVal.Add(DefPts.Vector1)
                    _rVal.Add(DefPts.Vector2)
                Case dxxGraphicTypes.MText, dxxGraphicTypes.Text, dxxGraphicTypes.Insert, dxxGraphicTypes.Hole, dxxGraphicTypes.Hatch, dxxGraphicTypes.Shape
                    _rVal = BoundingRectangle.Corners()
                    bReturnClones = False
                Case dxxGraphicTypes.Point
                    _rVal.Add(DefPts.Vector1)
                Case dxxGraphicTypes.Polygon
                    If bReturnClones Then _rVal = Vertices.Clone Else _rVal = Vertices
                    Dim subents As List(Of dxfEntity) = PersistentSubEntities()
                    If subents IsNot Nothing Then
                        If subents.Count > 0 Then
                            For Each aEnt As dxfEntity In subents
                                _rVal.Append(aEnt.GetVertices(bReturnClones))
                            Next
                        End If
                    End If
                    bReturnClones = False
                Case dxxGraphicTypes.Polyline
                    _rVal = Vertices
                'Case dxxGraphicTypes.Shape
                '    _rVal = BoundingRectangle.Corners()
                '    bReturnClones = False
                Case dxxGraphicTypes.Solid
                    If EntityType = dxxEntityTypes.Trace Then
                        _rVal.Add(DefPts.Vector1)
                        _rVal.Add(DefPts.Vector2)
                        _rVal.Add(DefPts.Vector3)
                        _rVal.Add(DefPts.Vector4)
                    Else
                        If Not PropValueB("*Triangular") Then
                            _rVal.Add(DefPts.Vector1)
                            _rVal.Add(DefPts.Vector2)
                            _rVal.Add(DefPts.Vector3)
                            _rVal.Add(DefPts.Vector4)
                        Else
                            _rVal.Add(DefPts.Vector1)
                            _rVal.Add(DefPts.Vector2)
                            _rVal.Add(DefPts.Vector3)
                        End If
                    End If
                Case dxxGraphicTypes.Symbol

                    For Each ent As dxfEntity In PathEntities

                    Next
                    Dim mySubEnts As dxfEntities = PathEntities
                    For Each ent As dxfEntity In mySubEnts
                        _rVal.Append(ent.GetVertices(bReturnClones))
                    Next
                    bReturnClones = False
                Case dxxGraphicTypes.SequenceEnd, dxxGraphicTypes.EndBlock
                    Return _rVal
                Case dxxGraphicTypes.Table
                    Dim mySubEnts As dxfEntities = PathEntities
                    For Each ent As dxfEntity In mySubEnts
                        _rVal.Append(ent.GetVertices(bReturnClones))
                    Next
                    bReturnClones = False
                    'Case dxxGraphicTypes.Text
                    '    _rVal = BoundingRectangle.Corners()
                    '    bReturnClones = False
                Case Else
                    _rVal = BoundingRectangle.Corners()
                    bReturnClones = False
            End Select
            If Not bReturnClones Then Return _rVal Else Return _rVal.Clone
        End Function
        Friend Function IntersectionSegments(aCurveDivisions As Integer, arInfinite As Boolean?) As TSEGMENTS
            Return dxfIntersections.IntersectionSegments(Me, aCurveDivisions, arInfinite)
        End Function
        '^If the Entity has dependent entities it will return a list containing them other wise an empty lists should be retured
        Public Overridable Function PersistentSubEntities() As List(Of dxfEntity)
            Return New List(Of dxfEntity)
        End Function
        Friend Function AddReactorHandle(aReactorGUID As String, aHandle As String, Optional aPropGC As Integer = 5) As Boolean
            If String.IsNullOrEmpty(aHandle) Then aHandle = "0"
            aHandle = aHandle.Trim
            If aHandle = String.Empty Then aHandle = "0"
            Return Properties.ReactorAdd(aPropGC, aReactorGUID, aHandle)
        End Function


        Friend Function ALSStructure(ByRef rInvalid As Boolean, Optional bNullLengthInvalid As Boolean = False) As TSEGMENT
            Dim rSuppress As Boolean = False
            Return ALSStructure(rInvalid, bNullLengthInvalid, rSuppress)
        End Function
        Friend Function ALSStructure(ByRef rInvalid As Boolean, bNullLengthInvalid As Boolean, ByRef rSuppress As Boolean) As TSEGMENT

            rSuppress = False

            Dim aEnt As dxfEntity = Me
            Dim bPl As Boolean
            If GraphicType = dxxGraphicTypes.Polyline Then
                rInvalid = True
                Dim aPl As dxfPolyline = Me
                bPl = True
                aEnt = aPl.Segment(1)
                rSuppress = Suppressed
                If aEnt Is Nothing Then Return New TSEGMENT()
            End If
            Select Case aEnt.GraphicType


                Case dxxGraphicTypes.Line, dxxGraphicTypes.Arc, dxxGraphicTypes.Ellipse
                    rInvalid = False

                    If Not bPl Then rSuppress = aEnt.Suppressed
                    Dim _rVal As TSEGMENT = aEnt.ArcLineStructure()

                    If bNullLengthInvalid Then
                        If _rVal.LineStructure.Length <= 0 Then
                            rInvalid = True
                        End If
                    End If
                    Return _rVal
                Case Else
                    Return New TSEGMENT()
                    rInvalid = True
            End Select

        End Function
        Friend Function AddReactor(aGC As Integer, aHandle As String, aReactorGroup As String, bAddIfNotFound As Boolean) As Boolean
            Dim _rVal As Boolean = False
            If aGC <= 0 Or String.IsNullOrWhiteSpace(aHandle) Or String.IsNullOrWhiteSpace(aReactorGroup) Then Return False
            Return Reactors.AddReactor(aReactorGroup, aGC, aHandle, bDontAddArray:=Not bAddIfNotFound)
        End Function
        Friend Function CloneAll(Optional aImage As dxfImage = Nothing, Optional bSuppressGUIDsUpdate As Boolean = False) As dxfEntity
            Dim _rVal As dxfEntity = Clone()
            _rVal.Handlez_Set(Handlez_Get)
            If aImage IsNot Nothing Then
                _rVal.ImageGUID = aImage.GUID
                If Not bSuppressGUIDsUpdate Then
                    aImage.HandleGenerator.UpdateGUIDS(Me, _rVal)
                End If
            End If
            Return _rVal
        End Function
        Public Function ContainsPoint(aPointXY As iVector, Optional aFudgeFactor As Double = 0.001, Optional bTreatAsInfinite As Boolean = False) As Boolean
            If aPointXY Is Nothing Then Return False
            Return ContainsVector(New TVECTOR(aPointXY), aFudgeFactor, False, False, bTreatAsInfinite)
        End Function
        Public Function ContainsPoint(aPointXY As iVector, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, Optional bTreatAsInfinite As Boolean = False) As Boolean
            If aPointXY Is Nothing Then Return False
            Return ContainsVector(New TVECTOR(aPointXY), aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite)
        End Function
        Friend Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional bTreatAsInfinite As Boolean = False) As Boolean
            Dim rIsStartPt As Boolean
            Dim rIsEndPt As Boolean
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite)
        End Function
        Friend Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, Optional bTreatAsInfinite As Boolean = False) As Boolean
            '#1the vector to test
            '#2a fudge factor to apply
            '#3returns true if the passed vector is the start vector of the entity
            '#4returns true if the passed vector is the end vector of the entity
            '^returns true if the passed vector lies on the entity
            Select Case GraphicType
                Case dxxGraphicTypes.Arc, dxxGraphicTypes.Ellipse, dxxGraphicTypes.Line
                    Dim within As Boolean = False
                    Return ArcLineStructure.ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, False, within)
                Case dxxGraphicTypes.Bezier
                    Dim aB As dxeBezier = Me
                    Return aB.BezierStructure.ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt)
                Case dxxGraphicTypes.Polyline, dxxGraphicTypes.Polygon
                    Return dxfEntities.ContainsVector(PolylineSegments, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, False)
                Case Else
                    Return False
            End Select
        End Function
        Friend Sub ClearHandles()
            Handle = String.Empty

        End Sub
        Public Function CopyDisplayValues(aDisplaySettings As dxfDisplaySettings) As Boolean
            If aDisplaySettings Is Nothing Then Return False
            '#1the entity settings to copy

            '^copies the values of the passed display settings to this entities display settings

            Dim _rVal As Boolean = Properties.CopyDisplayProperties(aDisplaySettings)
            If _rVal And BelongsToBlock And Not SuppressEvents Then
                dxfEntity.DirtyInserts(ImageGUID, BlockGUID)
            End If
            Return _rVal
        End Function
        Friend Function CopyDisplayValues(aEntitySet As TDISPLAYVARS) As Boolean

            '#1the entity settings to copy

            '^copies the values of the passed display settings to this entities display settings

            Dim _rVal As Boolean = Properties.CopyDisplayProps(aEntitySet)
            If _rVal And BelongsToBlock And Not SuppressEvents Then
                dxfEntity.DirtyInserts(ImageGUID, BlockGUID)
            End If
            Return _rVal
        End Function
        Friend Shared Function SetDisplayProperties(aEntity As dxfEntity, Optional aLayerName As Object = Nothing, Optional aColor As Object = Nothing, Optional aLineType As Object = Nothing, Optional aLTScale As Object = Nothing) As Boolean
            Dim _rVal As Boolean = False
            '^sets the entities primary display settings in a single call
            If aEntity Is Nothing Then Return _rVal
            Dim bChanged As Boolean
            Dim aStr As String
            Dim aclr As dxxColors
            Dim aVal As Double
            Dim bNoPaths As Boolean
            Dim aDSPV As TDISPLAYVARS
            Dim origDSP As TDISPLAYVARS
            Dim bLyr As Boolean
            Dim bLt As Boolean
            Dim bClr As Boolean
            Dim bLts As Boolean
            aDSPV = TENTITY.DisplayVarsFromProperties(aEntity.Properties)
            origDSP = aDSPV
            If aLayerName IsNot Nothing Then
                aStr = Trim(aLayerName)
                If aStr <> "" Then
                    bLyr = String.Compare(aDSPV.LayerName, aStr, True) <> 0
                    If bLyr Then
                        bChanged = True
                        aDSPV.LayerName = aStr
                    End If
                End If
            End If
            If aLineType IsNot Nothing Then
                aStr = Replace(Trim(aLineType), " ", "")
                If aStr <> "" Then
                    bLt = String.Compare(aDSPV.Linetype, aStr, True) <> 0
                    If bLt Then
                        bChanged = True
                        If aEntity.GraphicType = dxxGraphicTypes.Text Or aEntity.GraphicType = dxxGraphicTypes.Point Then bLt = False
                        aDSPV.Linetype = aStr
                    End If
                End If
            End If
            If aColor IsNot Nothing Then
                If TVALUES.IsNumber(aColor) Then
                    aclr = TVALUES.To_INT(aColor)
                    If aclr <> dxxColors.Undefined Then
                        bClr = aclr <> aDSPV.Color
                        If bClr Then
                            bChanged = True
                            aDSPV.Color = aclr
                        End If
                    End If
                End If
            End If
            If aLTScale IsNot Nothing Then
                If TVALUES.IsNumber(aLTScale) Then
                    aVal = TVALUES.To_DBL(aLTScale)
                    If aVal > 0 Then
                        bLts = aDSPV.LTScale <> aVal
                        If bLts Then
                            If aEntity.GraphicType = dxxGraphicTypes.Text Or aEntity.GraphicType = dxxGraphicTypes.Point Then bLts = False
                            bChanged = True
                            aDSPV.LTScale = aVal
                        End If
                    End If
                End If
            End If
            _rVal = bChanged
            If bChanged Then
                aEntity.DisplayStructure = aDSPV
                Dim aPaths As TPATHS
                If Not bNoPaths Then
                    If Not aEntity.IsDirty Then
                        aPaths = aEntity.Paths
                        If bLyr Then
                            aPaths.SetDisplayVariable("LAYERNAME", aDSPV.LayerName, origDSP.LayerName)
                        End If
                        If bLt Then
                            aPaths.SetDisplayVariable("LINETYPE", aDSPV.Linetype, origDSP.Linetype)
                        End If
                        If bClr Then
                            aPaths.SetDisplayVariable("COLOR", aDSPV.Color, origDSP.Color)
                        End If
                        If bLts Then
                            aPaths.SetDisplayVariable("LTSCALE", aDSPV.LTScale, origDSP.LTScale)
                        End If
                        aEntity.Paths = aPaths
                    End If
                End If
                If aEntity.BelongsToBlock Then
                    dxfEntity.DirtyInserts(aEntity.ImageGUID, aEntity.BlockGUID)
                End If
            End If
            Return _rVal
        End Function
        Friend Function CopyDisplayProps(aFromEntity As TENTITY, Optional aMatchLayer As Object = Nothing, Optional aMatchColor As dxxColors = dxxColors.Undefined, Optional aMatchLineType As Object = Nothing) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If Properties.Count < dxfGlobals.CommonProps Then Return _rVal
            If aFromEntity.Props.Count < dxfGlobals.CommonProps Then Return _rVal
            Dim mly As String = String.Empty
            Dim mlt As String = String.Empty
            _rVal = False
            If PropValueI("Color") = dxxColors.Undefined Then SetPropVal("Color", dxxColors.ByLayer)
            If aFromEntity.Props.ValueI("Color") = dxxColors.Undefined Then aFromEntity.Props.SetVal("Color", dxxColors.ByLayer)
            SetPropVal("LineType", PropValueStr("LineType").Trim)
            SetPropVal("LayerName", PropValueStr("LayerName").Trim)
            aFromEntity.Props.SetVal("LineType", aFromEntity.Props.ValueStr("LineType").Trim)
            aFromEntity.Props.SetVal("LayerName", aFromEntity.Props.ValueStr("LayerName").Trim)
            If PropValueStr("LayerName") = String.Empty Then SetPropVal("LayerName", "0")
            If PropValueStr("LineType") = String.Empty Then SetPropVal("LayerName", dxfLinetypes.ByLayer)
            If aFromEntity.Props.ValueStr("LayerName") = String.Empty Then aFromEntity.Props.SetVal("LayerName", "0")
            If aFromEntity.Props.ValueStr("LineType") = String.Empty Then aFromEntity.Props.SetVal("LineType", dxfLinetypes.ByLayer)
            If aMatchLayer IsNot Nothing Then mly = aMatchLayer.ToString().Trim()
            If aMatchLineType IsNot Nothing Then mlt = aMatchLineType.ToString().Trim()
            If Properties.CopyValue(aFromEntity.Props, dxfLinetypes.Invisible) Then _rVal = True
            If aMatchColor = dxxColors.Undefined Then
                If Properties.CopyValue(aFromEntity.Props, "Color") Then _rVal = True
            Else
                If PropValueI("Color") = aMatchColor Then
                    If Properties.CopyValue(aFromEntity.Props, "Color") Then _rVal = True
                End If
            End If
            If mly = String.Empty Then
                If Properties.CopyValue(aFromEntity.Props, "LayerName", aDefStringVal:="0") Then _rVal = True
            Else
                If String.Compare(PropValueStr("LayerName"), mly, True) = 0 Then
                    If Properties.CopyValue(aFromEntity.Props, "LayerName", aDefStringVal:="0") Then _rVal = True
                End If
            End If
            If mlt = String.Empty Then
                If Properties.CopyValue(aFromEntity.Props, "LineType", aDefStringVal:=dxfLinetypes.ByLayer) Then _rVal = True
            Else
                If String.Compare(PropValueStr("LayerName"), mlt, True) = 0 Then
                    If Properties.CopyValue(aFromEntity.Props, "LineType", aDefStringVal:=dxfLinetypes.ByLayer) Then _rVal = True
                End If
            End If
            If aFromEntity.Props.ValueD("LT Scale") > 0 Then
                If Properties.CopyValue(aFromEntity.Props, "LT Scale") Then _rVal = True
            End If
            If aFromEntity.Props.ValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer) >= dxxLineWeights.ByBlock Then
                If Properties.CopyValue(aFromEntity.Props, "LineWeight") Then _rVal = True
            End If
            Return _rVal
        End Function
        Friend Function CopyDisplayProps(aFromEntity As dxfEntity, Optional aMatchLayer As Object = Nothing, Optional aMatchColor As dxxColors = dxxColors.Undefined, Optional aMatchLineType As Object = Nothing) As Boolean
            If aFromEntity Is Nothing Then Return False


            Dim _rVal As Boolean
            'On Error Resume Next
            If Properties.Count < dxfGlobals.CommonProps Then Return _rVal
            If aFromEntity.Properties.Count < dxfGlobals.CommonProps Then Return _rVal
            Dim mly As String = String.Empty
            Dim mlt As String = String.Empty
            _rVal = False
            If PropValueI("Color") = dxxColors.Undefined Then SetPropVal("Color", dxxColors.ByLayer)
            If aFromEntity.Properties.ValueI("Color") = dxxColors.Undefined Then aFromEntity.SetPropVal("Color", dxxColors.ByLayer)
            SetPropVal("LineType", PropValueStr("LineType").Trim)
            SetPropVal("LayerName", PropValueStr("LayerName").Trim)
            aFromEntity.SetPropVal("LineType", aFromEntity.PropValueStr("LineType").Trim)
            aFromEntity.SetPropVal("LayerName", aFromEntity.PropValueStr("LayerName").Trim)
            If PropValueStr("LayerName") = String.Empty Then SetPropVal("LayerName", "0")
            If PropValueStr("LineType") = String.Empty Then SetPropVal("LayerName", dxfLinetypes.ByLayer)
            If aFromEntity.PropValueStr("LayerName") = String.Empty Then aFromEntity.SetPropVal("LayerName", "0")
            If aFromEntity.PropValueStr("LineType") = String.Empty Then aFromEntity.SetPropVal("LineType", dxfLinetypes.ByLayer)
            If aMatchLayer IsNot Nothing Then mly = aMatchLayer.ToString().Trim()
            If aMatchLineType IsNot Nothing Then mlt = aMatchLineType.ToString().Trim()
            If Properties.CopyValue(aFromEntity.Properties, dxfLinetypes.Invisible) Then _rVal = True
            If aMatchColor = dxxColors.Undefined Then
                If Properties.CopyValue(aFromEntity.Properties, "Color") Then _rVal = True
            Else
                If PropValueI("Color") = aMatchColor Then
                    If Properties.CopyValue(aFromEntity.Properties, "Color") Then _rVal = True
                End If
            End If
            If mly = String.Empty Then
                If Properties.CopyValue(aFromEntity.Properties, "LayerName", aDefStringVal:="0") Then _rVal = True
            Else
                If String.Compare(PropValueStr("LayerName"), mly, True) = 0 Then
                    If Properties.CopyValue(aFromEntity.Properties, "LayerName", aDefStringVal:="0") Then _rVal = True
                End If
            End If
            If mlt = String.Empty Then
                If Properties.CopyValue(aFromEntity.Properties, "LineType", aDefStringVal:=dxfLinetypes.ByLayer) Then _rVal = True
            Else
                If String.Compare(PropValueStr("LayerName"), mlt, True) = 0 Then
                    If Properties.CopyValue(aFromEntity.Properties, "LineType", aDefStringVal:=dxfLinetypes.ByLayer) Then _rVal = True
                End If
            End If
            If aFromEntity.Properties.ValueD("LT Scale") > 0 Then
                If Properties.CopyValue(aFromEntity.Properties, "LT Scale") Then _rVal = True
            End If
            If aFromEntity.Properties.ValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer) >= dxxLineWeights.ByBlock Then
                If Properties.CopyValue(aFromEntity.Properties, "LineWeight") Then _rVal = True
            End If
            Return _rVal

        End Function
        Public Sub Delete()
            '^removes the entity from an image's entities collection and bitmap
            dxfGlobals.goEvents.DeleteGraphic(Handlez_Get)
        End Sub
        Friend Function ExtentPts(Optional bSuppressInstances As Boolean = False) As TVECTORS
            UpdatePath()
            Dim _rVal As TVECTORS = _Components.Paths.ExtentVectors
            If Not bSuppressInstances And Instances.Count >= 1 Then _rVal = Instances.Apply(_rVal)
            Return _rVal
        End Function



        Public Function PhantomPoints(Optional aCurveDivisions As Integer = 20, Optional aLineDivision As Integer = 1) As colDXFVectors
            '^a collection of phantom vertices along the entity
            Return dxfUtils.PhantomPoints(Me, aCurveDivisions, aLineDivision)
        End Function

        Friend Sub UpdatePath(bRegen As Boolean, bResetReactors As Boolean, Optional aImage As dxfImage = Nothing)
            '#1flag to force a regeneration of the entities path
            '^updates the entities basic path object to reflect the current properties
            '~the path is recomputed if it is dirty or the passed flag is True
            If State <> dxxEntityStates.Steady Then Return
            If bResetReactors Or bRegen Or _Components.Paths.Count = 0 Then
                _MText = Nothing
                _Insert = Nothing
            End If
            UpdatePath(bRegen, aImage)
        End Sub

        Public Sub UpdatePath(Optional bRegen As Boolean = False, Optional aImage As dxfImage = Nothing)
            '#1flag to force a regeneration of the entities path
            '^updates the entities basic path object to reflect the current properties
            '~the path is recomputed if it is dirty or the passed flag is True
            If State <> dxxEntityStates.Steady Then Return
            If bRegen Or _Components.Paths.Count = 0 Then
                _MText = Nothing
                _Insert = Nothing
            End If
            If IsDirty Or _Components.Paths.Count = 0 Then bRegen = True


            If GetImage(aImage) Then
                If aImage.RegenList.IndexOf(GraphicTypeName, StringComparison.OrdinalIgnoreCase) >= 0 Then bRegen = True
            End If
            If Not bRegen Then Return
            Try
                State = dxxEntityStates.GeneratingPath

                dxfPaths.Paths_Entity(Me, aImage)
                IsDirty = False
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType(), ex)
            Finally
                State = dxxEntityStates.Steady
            End Try
        End Sub
        Public Function SetDisplayProperties(Optional aLayerName As Object = Nothing, Optional aColor As Object = Nothing, Optional aLineType As Object = Nothing, Optional aLTScale As Object = Nothing) As Boolean
            '^sets the entities primary display settings in a single call\
            Return dxfEntity.SetDisplayProperties(Me, aLayerName, aColor, aLineType, aLTScale)
        End Function
        Public Sub LCLGet(ByRef rLayerName As String, ByRef rColor As dxxColors, ByRef rLinetype As String, ByRef rLineWeight As dxxLineWeights, ByRef rLTScale As Double)
            'returns the Layer, Color, Linetype and lineweight of the entity
            rLayerName = LayerName
            rColor = Color
            rLinetype = Linetype
            rLineWeight = LineWeight
            rLTScale = LTScale
        End Sub
        Public Sub LCLSet(Optional aLayerName As String = Nothing, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = Nothing)
            'sets the Layer, Color and Linetype of the entity
            If Not String.IsNullOrWhiteSpace(aLayerName) Then LayerName = aLayerName.Trim()
            If aColor <> dxxColors.Undefined Then
                Color = aColor
            End If
            If Not String.IsNullOrWhiteSpace(aLineType) Then Linetype = aLineType.Trim()
        End Sub
        Public Sub TFVCopy(aEntity As dxfEntity)
            If aEntity Is Nothing Then Return
            Dim aTg As String = String.Empty
            Dim aFg As String = String.Empty
            Dim aVal As Object = Nothing
            aEntity.TFVGet(aTg, aFg, aVal)
            TFVSet(aTg, aFg, aVal)
        End Sub
        Public Sub TFVGet(ByRef rTag As String, ByRef rFlag As String)
            rTag = Tag : rFlag = Flag
        End Sub
        Public Sub TFVGet(ByRef rTag As String, ByRef rFlag As String, ByRef rValue As Object)
            rTag = Tag : rFlag = Flag : rValue = Value
        End Sub
        Public Sub TFVSet(Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing)
            If aTag IsNot Nothing Then Tag = aTag.ToString()
            If aFlag IsNot Nothing Then Flag = aFlag.ToString()
            If aValue IsNot Nothing Then
                If aValue.HasValue Then Value = aValue.Value
            End If
        End Sub
        Public Function UpdateImage(Optional bErase As Boolean = True) As Boolean
            '#1flag to erase the entity befor re-rendering it
            '^re-renders the entity in its current image
            If dxfGlobals.goEvents.UpdateEntityGraphic(Me, bErase) Then
                IsDirty = False
                Return True
            Else
                Return False
            End If
        End Function
        Public Overridable Sub UpdateCommonProperties(Optional aCadTypeName As String = Nothing, Optional bUpdateProperties As Boolean = False)
            Dim aVal As Object
            Dim aclr As dxfColor
            Properties.Handle = Handle

            'layer
            aVal = PropValueStr("LayerName").Trim()
            If aVal = String.Empty Then aVal = "0"
            SetPropVal("LayerName", aVal)
            'linetype
            aVal = PropValueStr("LineType").Trim()
            If aVal = String.Empty Then aVal = dxfLinetypes.ByLayer
            SetPropVal("LineType", aVal)


            'color
            aVal = PropValueI("Color")
            If aVal = dxxColors.Undefined Then aVal = dxxColors.ByLayer
            aclr = dxfColors.Color(aVal)
            aVal = aclr.ACLValue
            SetPropVal("Color", aVal)
            If aclr.IsACL Then
                If aVal <> 0 And aVal <> 256 Then SetPropVal("Color Long Value", aclr.RGB.ToWin32(True))
                Properties.SetSuppressed("Color Long Value", True)
            Else
                SetPropVal("Color Long Value", aclr.RGB.ToWin32(True))
                Properties.SetSuppressed("Color Long Value", False)
            End If
            If Not String.IsNullOrWhiteSpace(aCadTypeName) Then SetPropVal("Entity Type", aCadTypeName)

            'lnot all entities have these properties
            'linetype scale
            Dim prop As dxoProperty = Nothing
            If Properties.TryGet("LT Scale", prop) Then
                Dim dval As Double = prop.ValueD
                If dval <= 0 Then dval = 1
                prop.SetVal(dval)
            End If
            'line weight
            If Properties.TryGet("LineWeight", prop) Then
                Dim ival As Integer = prop.ValueI
                If ival < dxxLineWeights.ByBlock Or ival > dxxLineWeights.LW_211 Then ival = dxxLineWeights.ByLayer
                prop.SetVal(ival)
            End If



            Properties.SetVal("*EntityType", EntityType)

            If HasDimStyle Then
                SetPropVal("DimStyle Name", DimStyle.Name)
            End If
            If bUpdateProperties Then UpdateProperties()
        End Sub
        Protected Overrides Sub Finalize()
            _MText = Nothing
            _Insert = Nothing
            _DimStyle = Nothing
            MyBase.Finalize()
        End Sub



        Public Function Intersections(aEntity As dxfEntity, Optional thisEntity_IsInfinite As Boolean = False, Optional theEntity_IsInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1a entity  of entity objects to get the intersection points for
            '#2flag to treat this entity as infinite
            '#3flag to treat the passed entity as infinite
            '#4flag indicated all returned points must lie on both this entity and the passed entity
            '^returns the intersection points of the passed entity (line,line,polyline etc.) with this entity
            Dim alist As New List(Of dxfEntity)({Me})
            Dim blist As New List(Of dxfEntity)({aEntity})
            dxfIntersections.Points(alist, blist, thisEntity_IsInfinite, theEntity_IsInfinite, bMustBeOnBoth, aCollector:=_rVal)
            Return _rVal
        End Function
        Public Function Intersections(aEntities As IEnumerable(Of dxfEntity), Optional thisEntity_IsInfinite As Boolean = False, Optional theEntities_AreInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1a  collection of entity objects to get the intersection points for
            '#2flag to treat this entity as infinite
            '#3flag to treat the passed entity as infinite
            '#4flag indicated all returned points must lie on both this entity and the passed entity
            '^returns the intersection points of the passed entity (line,line,polyline etc.) with this entity
            Dim alist As New List(Of dxfEntity)({Me})
            dxfIntersections.Points(alist, aEntities, thisEntity_IsInfinite, theEntities_AreInfinite, bMustBeOnBoth, aCollector:=_rVal)
            Return _rVal
        End Function

#End Region 'Methods
#Region "Transformation Methods"
        Public Function ProjectionToPlane(aPlane As dxfPlane) As colDXFEntities
            '^the entities created when this entity is projected to a plane
            If dxfPlane.IsNull(aPlane) Then Return New colDXFEntities(Me)
            Return dxfEntity.ProjectToPlane(Me, New TPLANE(aPlane))
        End Function
        Public Function Mirror(aLineXY As iLine) As Boolean
            Return Transform(TTRANSFORM.CreateMirror(aLineXY, SuppressEvents))
        End Function

        Public Function MirrorPlanar(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the x coordinate to mirror across
            '#2the y coordinate to mirror across
            '#3the plane to use to create the mirror axes
            '^moves the current coordinates to a vector mirrored across the passed values
            '~only allows orthogonal mirroring.
            Dim aPl As New TPLANE(aPlane)
            Dim aLn As TLINE
            Dim aTrs As New TTRANSFORMS

            If aMirrorX.HasValue Then
                aLn = aPl.LineV(aMirrorX.Value, 10)
                aTrs.Add(TTRANSFORM.CreateMirror(aLn, SuppressEvents))
            End If
            If aMirrorY.HasValue Then
                aLn = aPl.LineH(aMirrorY.Value, 10)
                aTrs.Add(TTRANSFORM.CreateMirror(aLn, SuppressEvents))
            End If
            If aTrs.Count > 0 Then
                Return Transform(aTrs)
            Else
                Return False
            End If
        End Function
        Public Function Move(Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Return Transform(TTRANSFORM.CreateTranslation(aXChange, aYChange, aZChange, aPlane, HandlePt, SuppressEvents))
        End Function

        Public Function Moved(Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxfEntity
            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^returns a copy of the enttity moved by the passed displacement info
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Dim _rVal As dxfEntity = Clone()
            _rVal.Move(aXChange, aYChange, aZChange, aPlane)
            Return _rVal
        End Function

        Public Function MoveFromTo(aBasePointXY As iVector, aDestinationPointXY As iVector, Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0) As Boolean
            '^used to move the object from one reference vector to another
            Return Transform(TTRANSFORM.CreateFromTo(aBasePointXY, aDestinationPointXY, aXChange, aYChange, aZChange, SuppressEvents))
        End Function
        Public Function MovePolar(aBasePt As iVector, aAngle As Double, aDistance As Double, Optional aPlane As dxfPlane = Nothing, Optional bInRadians As Boolean = False) As Boolean
            '#1the base point to use as the center
            '#2the direction angle to move in
            '#3the distance to move
            '#4the coordinate system to use to determine the direction based on the angle
            '^moves the entity to a point on a plane aligned with the XY plane of the passed coordinate system and centered at the base point.
            '~if the base point is nothing passed the center of the entities coordinate system is used.
            '~if the coordinate system is not passed the world coordinate system is used.
            Return Transform(TTRANSFORM.CreatePolarTranslation(aBasePt, aAngle, aDistance, HandlePt, aPlane, bInRadians, SuppressEvents))
        End Function
        Friend Function MoveTo(aDestinationVector As TVECTOR, Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0) As Boolean
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the entity from its current primary definition point to the passed point
            '~returns True if the entity actually moves from this process
            Return Transform(TTRANSFORM.CreateFromTo(HandlePt, New dxfVector(aDestinationVector), aXChange, aYChange, aZChange, SuppressEvents))
        End Function
        Public Function MoveTo(aDestinationXY As iVector, Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0) As Boolean
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the entity from its current primary definition point to the passed point
            '~returns True if the entity actually moves from this process
            Return Transform(TTRANSFORM.CreateFromTo(HandlePt, aDestinationXY, aXChange, aYChange, aZChange, SuppressEvents))
        End Function
        Public Function MoveTo(aDestinationEnt As dxfEntity, Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0) As Boolean
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the entity from its current primary definition point to the passed point
            '~returns True if the entity actually moves from this process
            If aDestinationEnt Is Nothing Then Return False

            Return Transform(TTRANSFORM.CreateFromTo(HandlePt, aDestinationEnt.HandlePt, aXChange, aYChange, aZChange, SuppressEvents))
        End Function
        Public Function Project(aDirection As dxfDirection, aDistance As Double) As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the entity in the passed direction the requested distance
            If aDirection Is Nothing Then Return False
            Return Transform(TTRANSFORM.CreateProjection(aDirection, aDistance, SuppressEvents))
        End Function
        Public Function Project(aVector As dxfVector, aDistance As Double) As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the entity in the passed direction the requested distance
            If aVector Is Nothing Then Return False
            Return Transform(TTRANSFORM.CreateProjection(aVector.Direction, aDistance, SuppressEvents))
        End Function
        Friend Function Project(aDirection As TVECTOR, aDistance As Double) As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^translates the entity in the passed direction the passed distance
            Return Transform(TTRANSFORM.CreateProjection(aDirection, aDistance, SuppressEvents))
        End Function
        Public Function Rescale(aScaleFactor As Double, Optional aReference As iVector = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the factor to scale the entity by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the entity in space and dimension by the passed factor
            Dim pln As dxfPlane = aPlane
            If dxfPlane.IsNull(aPlane) Then pln = Plane
            If aReference IsNot Nothing Then pln.OriginV = New TVECTOR(aReference)
            Return Transform(TTRANSFORM.CreateScale(pln.OriginV, aScaleFactor, aPlane:=pln, bSuppressEvents:=SuppressEvents))

            'If aReference Is Nothing Then
            '    Return Transform(TTRANSFORM.CreateScale(HandlePt.Strukture, aScaleFactor, aPlane:=aPlane, bSuppressEvents:=SuppressEvents))
            'Else
            '    Return Transform(TTRANSFORM.CreateScale(New TVECTOR(aReference), aScaleFactor, aPlane:=aPlane, bSuppressEvents:=SuppressEvents))
            'End If
        End Function
        Public Function Rotate(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Rotate(aAngle, rAxis, bInRadians, aPlane)
        End Function
        Public Function Rotate(aAngle As Double, ByRef rAxis As dxeLine, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '#3if passed returns the line used for the rotation
            '#4the coordinate system to use to get the rotation axis
            '^rotates the entity about a line through the entities reference point and aligned with the current Z axis of the entities OCS
            Return Transform(TTRANSFORM.CreatePlanarRotation(HandlePt.Strukture, PlaneV, aAngle, bInRadians, aPlane, dxxAxisDescriptors.Z, rAxis, SuppressEvents))
        End Function

        Public Function RotateAbout(aPlane As dxfPlane, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPoint As dxfVector = Nothing, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z) As Boolean
            '#1the plane which is used to obtaint the axis of rotation. If null, the world plane is assumed.
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the point to rotate about
            '#5an optional axis descriptor to selected a planes primary axis to use other than the Z axis.
            '^rotates the members about the an axis the requested  starting at the passed point and aligned with the X axis of the passef plane
            '~if the passed point is null the enity is rotated about the origin of the passed coordinated system
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane
            If aPoint Is Nothing Then aPoint = aPlane.Origin.Clone
            Return Transform(TTRANSFORM.CreateRotation(aPoint, aPlane, aAngle, bInRadians, Nothing, True, aAxisDescriptor), aEventName:="RotateAbout-Point")
        End Function
        Public Function RotateAbout(aAxis As iLine, aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            If aAxis Is Nothing Then Return False
            '#1the line to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '^rotates the entity about the passed axis the requested angle
            '~if the passed line is nothing no action is taken
            Return Transform(TTRANSFORM.CreateRotation(aAxis, Nothing, aAngle, bInRadians, Nothing, SuppressEvents, False), aEventName:="RotateAbout-Line")
        End Function
        Public Function RotateAbout(aPoint As iVector, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z) As Boolean
            '#1the point to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the plane which is used to obtaint the axis of rotation. If null, the world plane is assumed.
            '#5an optional axis descriptor to selected a planes primary axis to use other than the Z axis.
            '^rotates the entity about an axis starting at the passed point and aligned with the Z axis of the passed plane
            '~if the passed point is null the members is rotated about the origin of the passed coordinated system
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane
            If aPoint Is Nothing Then aPoint = aPlane.Origin.Clone
            Return Transform(TTRANSFORM.CreateRotation(aPoint, aPlane, aAngle, bInRadians, Nothing, True, aAxisDescriptor), aEventName:="RotateAbout-Point")
        End Function
        Public Function Spin(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Spin(aAngle, rAxis, bInRadians, aPlane)
        End Function
        Public Function Spin(aAngle As Double, ByRef rAxis As dxeLine, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '#3returns the line used for the rotation
            '#4the coordinate system to use to get the rotation axis
            '^rotates the entity about a line through the entities reference point and aligned with the current Y axis of the entities OCS
            Return Transform(TTRANSFORM.CreatePlanarRotation(HandlePt.Strukture, PlaneV, aAngle, bInRadians, aPlane, dxxAxisDescriptors.Y, rAxis, SuppressEvents))
        End Function
        Public Function Tip(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Tip(aAngle, rAxis, bInRadians, aPlane)
        End Function
        Public Function Tip(aAngle As Double, ByRef rAxis As dxeLine, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '#3returns the line used for the rotation
            '#4the coordinate system to use to get the rotation axis
            '^rotates the entity about a line through the entities reference point and aligned with the current X axis of the entities OCS
            Return Transform(TTRANSFORM.CreatePlanarRotation(HandlePt.Strukture, PlaneV, aAngle, bInRadians, aPlane, dxxAxisDescriptors.Z, rAxis, SuppressEvents))
        End Function
        Friend Function Transform(aTransforms As TTRANSFORMS, Optional bSuppressEvnts As Boolean = False) As Boolean

            If aTransforms.Count <= 0 Then Return False
            Return TTRANSFORMS.Apply(aTransforms, Me, bSuppressEvnts)
        End Function
        Friend Function Transform(aTransform As TTRANSFORM, Optional bSuppressEvnts As Boolean = False, Optional aEventName As String = Nothing) As Boolean
            Return TTRANSFORM.Apply(aTransform, Me, bSuppressEvnts)
        End Function
        Public Function Translate(aTranslation As iVector, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1a vector with X,Y, and Z translations
            '#2the coordinate system to use for direction
            '^translates the entity the passed displacements based on directions the subject coordinate system
            '~if no system is passed then entities current coordinate system is used

            Return Translate(New TVECTOR(aTranslation), aPlane)
        End Function
        Public Function Translate(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1a vector with X,Y, and Z translations
            '#2the coordinate system to use for direction
            '^translates the entity the passed displacements based on directions the subject coordinate system
            '~if no system is passed then entities current coordinate system is used
            Return Translate(New TVECTOR(aX, aY, aZ), aPlane)
        End Function
        Friend Function Translate(aTranslation As TVECTOR, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1a vector with X,Y, and Z translations
            '^translates the entity with the passed X,Y and Z displacements
            'If  Not dxfPLane.IsNull(aPlane) Then aPlane = Plane

            If TVECTOR.IsNull(aTranslation) Then Return False
            Return Transform(TTRANSFORM.CreateTranslation(aTranslation, aPlane:=aPlane, bSuppressEvents:=SuppressEvents))
        End Function


#End Region 'Transformation Methods
#Region "Instancing Methods"
        Public Property Centers As colDXFVectors
            'gets and sets the instanceing points
            Get

                Return Instances.Centers()
            End Get
            Set(value As colDXFVectors)
                If value Is Nothing Then
                    HandlePt.SetCoordinates(0, 0, 0)
                Else
                    Dim aVectors As New TVERTICES(value)
                    If aVectors.Count > 0 Then
                        HandlePt.MoveTo(aVectors.Vector(1))
                        Instances.FromVertices(aVectors, PlaneV, 1, 0)
                    Else
                        HandlePt.SetCoordinates(0, 0, 0)
                    End If
                End If
            End Set
        End Property
        'Friend Property InstancesV As TINSTANCES
        '    Get
        '        _Instances.Plane = PlaneV
        '        Return _Instances
        '    End Get
        '    Set(value As TINSTANCES)
        '        _Instances = value
        '    End Set
        'End Property


#End Region 'Instancing Methods
#Region "DimStyle_EventHandlers"
        Friend Overrides Sub RespondToDimStylePropertyChange(aEvent As dxfPropertyChangeEvent)
            If aEvent Is Nothing Or Not HasDimStyle Then Return
            aEvent.OwnerNotified = True
            _Style = New TTABLEENTRY(DimStyle)
            _DimStyleStructure = _Style
        End Sub
#End Region 'DimStyle_EventHandlers
#Region "DefPts_EventHandlers"
        Friend Overrides Sub RespondToDefPtChange(aEvent As dxfDefPtEvent)
            If aEvent Is Nothing AndAlso aEvent.Vertex Is Nothing Then Return
            If aEvent.Vertex.DefPntIndex < 1 Or aEvent.Vertex.DefPntIndex > DefPtCount Then Return
            aEvent.OwnerNotified = True
            If IsDirty Then Return
            Dim bDontDirty As Boolean = False
            Try
                If aEvent.EventType = dxxVertexEventTypes.Position Then
                    If GraphicType = dxxGraphicTypes.Line And Not IsDirty Then
                        Dim aLn As dxeLine = Me
                        Dim aPth As TPATH
                        Dim aLoop As TVECTORS
                        If aEvent.Vertex.DefPntIndex = 1 Then
                            bDontDirty = Not aLn.HasWidth And _Components.Paths.Count > 0
                            If bDontDirty Then
                                If _Components.Paths.Count > 0 Then
                                    aPth = _Components.Paths.Item(1)
                                    If aPth.LoopCount > 0 Then
                                        aLoop = aPth.Looop(1)
                                        If aLoop.Count >= 1 Then
                                            aLoop.SetItem(1, aEvent.Vertex.Strukture)
                                            aPth.SetLoop(1, aLoop)
                                            _Components.Paths.SetItem(1, aPth)
                                        Else
                                            bDontDirty = False
                                        End If
                                    Else
                                        bDontDirty = False
                                    End If
                                Else
                                    bDontDirty = False
                                End If
                            End If
                        ElseIf aEvent.Vertex.DefPntIndex = 2 Then
                            bDontDirty = Not aLn.HasWidth And _Components.Paths.Count > 0
                            If bDontDirty Then
                                If _Components.Paths.Count > 0 Then
                                    aPth = _Components.Paths.Item(1)
                                    If aPth.LoopCount > 0 Then
                                        aLoop = aPth.Looop(1)
                                        If aLoop.Count >= 2 Then
                                            aLoop.SetItem(2, aEvent.Vertex.Strukture)
                                            aPth.SetLoop(1, aLoop)
                                            _Components.Paths.SetItem(1, aPth)
                                            '_Struc.Components.Paths.Members(0).Pen.Defined = False
                                        End If
                                    Else
                                        bDontDirty = False
                                    End If
                                Else
                                    bDontDirty = False
                                End If
                            End If
                        End If
                        Return
                    End If
                    If Not bDontDirty Then IsDirty = True
                End If
            Catch
            Finally
                DefPts.RespondToDefPtChange(aEvent, bDontDirty)
            End Try
        End Sub
        Friend Overrides Sub RespondToVectorsMemberChange(aEvent As dxfVertexEvent)
            If aEvent Is Nothing Then Return
            If String.Compare(aEvent.OwnerGUID, GUID, ignoreCase:=True) Then Return
            aEvent.OwnerNotified = True
            If Not DefPts.HasVertices Then Return
            DefPts.RespondToVectorsMemberChange(aEvent)
            If GraphicType = dxxGraphicTypes.Polyline Or GraphicType = dxxGraphicTypes.Polygon Then
                If aEvent.PropertyType = dxxVectorProperties.EndWidth Or aEvent.PropertyType = dxxVectorProperties.StartWidth Then
                    SetPropVal("Constant Width", -1, True)
                End If
            End If
        End Sub
        Friend Overrides Sub RespondToVectorsChange(aEvent As dxfVectorsEvent)
            If aEvent Is Nothing Then Return
            If String.Compare(aEvent.OwnerGUID, GUID, ignoreCase:=True) Then Return
            aEvent.OwnerNotified = True
            DefPts.RespondToVectorsChange(aEvent)
        End Sub
#End Region 'DefPts_EventHandlers
#Region "oEvents_EventHandlers"
        Private Sub _Events_EntityRequest(aGUID As String, ByRef rEntity As dxfEntity)
            If aGUID = GUID Then rEntity = Me
        End Sub
#End Region 'oEvents_EventHandlers
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _Disposed Then
                If disposing Then
                    Try
                        ReleaseReferences()
                        SetGUIDS("", "", "", dxxFileObjectTypes.Undefined)

                        If _DefPts IsNot Nothing Then
                            '' RemoveHandler _DefPts.VectorChange, AddressOf _DefPts_VectorChange
                            'RemoveHandler _DefPts.VertexChange, AddressOf _DefPts_VertexChange
                            _DefPts.SetGUIDS("", "", "", True)
                            _DefPts.Dispose()
                        End If

                        If _DimStyle IsNot Nothing Then
                            'RemoveHandler _DimStyle.DimStylePropertyChange, AddressOf _DimStyle_DimStylePropertyChange
                            _DimStyle.SetGUIDS("", "", "", dxxFileObjectTypes.Undefined)
                            _DimStyle.Dispose()
                        End If

                        If _Insert IsNot Nothing Then
                            _Insert.SetGUIDS("", "", "", dxxFileObjectTypes.Undefined)
                            _Insert.Dispose()
                        End If

                        If _MText IsNot Nothing Then
                            _MText.SetGUIDS("", "", "", dxxFileObjectTypes.Undefined)
                            _MText.Dispose()
                        End If

                        If _AddSegs IsNot Nothing Then
                            _AddSegs.SetGUIDS("", "", "", dxxFileObjectTypes.Undefined)
                            'RemoveHandler _AddSegs.EntitiesChange, AddressOf _AddSegs_EntitiesChange
                            'RemoveHandler _AddSegs.EntitiesMemberChange, AddressOf _AddSegs_EntitiesMemberChange
                            If _AddSegs.Count > 0 Then
                                _AddSegs.Dispose()
                            End If
                        End If

                        Dim subsents As List(Of dxfEntity) = Me.PersistentSubEntities()
                        If subsents IsNot Nothing Then
                            For Each ent As dxfEntity In subsents
                                If ent IsNot Nothing Then ent.Dispose()
                            Next
                            subsents.Clear()
                        End If
                    Catch ex As Exception

                    Finally
                        _DefPts = Nothing
                        _DimStyle = Nothing
                        _Insert = Nothing
                        _MText = Nothing
                        _AddSegs = Nothing

                        _Disposed = True
                    End Try


                End If

            End If
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#Region "Shared Methods"
        Friend Shared Function ArcLine(aEntity As dxfEntity) As TSEGMENT

            Return TPROPERTIES.ArcLineStructure(aEntity, Nothing)
        End Function
        Friend Shared Function ArcLine(aEntity As dxfEntity, Optional arInfinite As Boolean? = Nothing) As TSEGMENT

            Return TPROPERTIES.ArcLineStructure(aEntity, arInfinite)
        End Function
        Friend Shared Sub DirtyInserts(aImageGUID As String, aBlockGUID As String)
            If aImageGUID = String.Empty Or aBlockGUID = String.Empty Then Return
            Dim aImg As dxfImage = dxfEvents.GetImage(aImageGUID)
            If aImg Is Nothing Then Return
            Dim aBl As dxfBlock = Nothing
            If Not aImg.Blocks.TryGet(aBlockGUID, aBl, dxxBlockReferenceTypes.GUID) Then Return


            Dim aIns As dxeInsert
            Dim aEnts As colDXFEntities = aImg.Entities
            For Each aEnt As dxfEntity In aEnts
                If aEnt.GraphicType = dxxGraphicTypes.Insert Then

                    aIns = DirectCast(aEnt, dxeInsert)
                    If aIns.SourceBlockGUID = aBlockGUID Then
                        aIns.IsDirty = True
                    End If
                End If
            Next

        End Sub
        Friend Shared Function Text_DXF(aText As dxeText, aImage As dxfImage, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aInstances As dxoInstances = Nothing) As TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            If aText Is Nothing Then Return New TPROPERTYARRAY("", aInstance)
            Dim _rVal As New TPROPERTYARRAY(aText.GUID, aInstance)
            aText.GetImage(aImage)
            If aInstances Is Nothing Then aInstances = aText.Instances
            'On Error Resume Next
            Dim eProps As New TPROPERTIES(aText.Properties)
            Dim rProps As TPROPERTIES = eProps.Clone()
            Dim aTrs As New TTRANSFORMS
            Dim scl As Double
            Dim ang As Double
            Dim bInv As Boolean
            Dim bLft As Boolean
            Dim tType As dxxTextTypes = aText.TextType
            rTypeName = dxfEnums.CodeValue(tType)
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim wf As Double
            Dim algn As dxxMTextAlignments = aText.Alignment
            Dim hAlg As dxxTextJustificationsHorizontal
            Dim vAlg As dxxTextJustificationsVertical
            Dim tht As Double = aText.TextHeight
            Dim tang As Double = aText.Rotation
            Dim aRec As TPLANE
            Dim ff As Double

            Dim txt As String
            Dim aTxt As dxeText
            Dim bReturnDTextCol As Boolean
            Dim subText As Collection
            Dim tstrings As dxfStrings = aText.GetStrings(False, aImage)


            Dim aPl As TPLANE = aText.PlaneV
            Dim txtPln As TPLANE = aPl.Clone
            Dim xtrus As String = "Extrusion Direction"
            'Dim vjust As String = "Vertical text justification type"
            'If tang <> 0 Then txtPln.Revolve(tang, False)
            eProps.Handle = aText.Handle
            aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
            TENTITY.UpdateCommonProps(eProps, rTypeName)
            eProps.SetVector(xtrus, New dxfVector(aOCS.ZDirection), bSuppressed:=Math.Round(aOCS.ZDirection.Z, 6) = 1, bDirection:=True)
            aText.SetProps(eProps)
            rProps.SetVal("Entity Type", rTypeName)
            wf = eProps.ValueD("Width Factor") 'WidthFactor
            txt = eProps.ValueStr("Text String") 'TextString
            hAlg = TFONT.AlignmentH(algn)
            vAlg = TFONT.AlignmentV(algn)
            aRec = aText.Bounds
            If algn = dxxMTextAlignments.Fit Then
                ff = eProps.ValueD("*FitFactor") 'FitFactor
                If ff <> 0 Then wf *= ff
                algn = dxxMTextAlignments.Aligned
            End If
            If wf <= 0 Then wf = 1
            If tType = dxxTextTypes.Multiline Then
                'If tang <> 0 Then aPl.Revolve(tang, False)

                v1 = tstrings.MTextAlignmentPt(v1, algn)
                rProps.SetVector("AligmentPt 1", New dxfVector(v1))
                rProps.SetVal("Text Height", tht)
                rProps.SetVal("Reference Rectangle Width", 0) 'aRec.Width
                rProps.SetVal("Alignment", algn)
                rProps.SetVal("Drawing Direction", eProps.ValueI("*Drawing Direction"))
                rProps.SetVal("Text Style Name", eProps.ValueStr("Text Style Name", "Standard")) 'TextStyleName)
                rProps.SetVector("Extrusion Direction", eProps.Vector("Extrusion Direction").Normalized)
                rProps.SetVector("X-Axis Direction", aPl.XDirection.Normalized)
                rProps.SetVal("Horizontal Rectangle Width", aRec.Width)
                rProps.SetVal("Vertical Rectangle Width", aRec.Height)
                rProps.SetVal("Text Angle(radians)", 0)
                rProps.SetVal("Line Spacing Style", eProps.ValueI("*Line Spacing Style")) 'LineSpacingStyle
                rProps.SetVal("Line Spacing Factor", eProps.ValueI("*Line Spacing Factor", True, True, 1)) 'LineSpacingFactor
                rProps = rProps.ReplaceByGC(dxfUtils.TextOutputProps(txt), 1)
            Else
                '            tang = aOCS.XDirection.AngleTo( aRec.XDirection, aOCS.ZDirection)
                Dim vecs As TVECTORS = tstrings.AlignmentVectors(aOCS, v1, v2, algn, wf, aTrs)
                rProps.SetVector("First alignment point(in OCS)", vecs.Item(1))
                rProps.SetVector("Second alignment point(in OCS)", vecs.Item(2))
                rProps.SetVal("Width Factor", wf)
                rProps.SetVal("Horizontal text justification type", hAlg)
                rProps.SetVal("Vertical text justification type", vAlg)
                rProps.SetVal("Attribute flags", 0)
                rProps.SetVal("Rotation", TVALUES.NormAng(tang + ang, False, True, True))
                Dim aflg As Integer = 0
                If eProps.ValueB("*Invisible") Then aflg += 1
                If eProps.ValueB("*Constant") Then aflg += 2
                If eProps.ValueB("*Verify") Then aflg += 4
                If eProps.ValueB("*Preset") Then aflg += 8
                rProps.SetVal("Attribute flags", aflg)
                rProps.SetSuppressed("Attribute Prompt", tType = dxxTextTypes.DText)
                rProps.SetSuppressed("Attribute Tag", tType = dxxTextTypes.DText)
                rProps.SetSuppressed("Attribute flags", tType = dxxTextTypes.DText)
                rProps.SetSuppressed("Lock position flag", tType = dxxTextTypes.DText)
                If tType = dxxTextTypes.AttDef Then
                    rProps.GroupCodeSet("Vertical text justification type", 74)
                    rProps.SetVal("Entity Sub Class Marker_2", "AcDbAttributeDefinition")
                ElseIf tType = dxxTextTypes.Attribute Then
                    rProps.GroupCodeSet("Vertical text justification type", 74)
                    rProps.SetVal("Entity Sub Class Marker_2", "AcDbAttribute")
                Else
                    rProps.GroupCodeSet("Vertical text justification type", 73)
                    rProps.SetVal("Entity Sub Class Marker_2", "AcDbText")
                End If
            End If
            eProps = rProps 'save these for use in subsequent instances
            scl = 1
            ang = 0
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = aText.Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl)
                TTRANSFORMS.Apply(aTrs, txtPln)
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
            End If
            '===============================================================================
            'loop on sub text
            aTxt = aText
            If tType = dxxTextTypes.Multiline Then
                If (aText.UpsideDown Or aText.Backwards) Or aText.Alignment >= dxxMTextAlignments.Fit Then
                    aTxt = aText.Clone
                    tType = aTxt.TextType
                End If
            End If
            'If tType <> dxxTextTypes.Multiline Then
            '    If subStrs.Count > 1 Then bReturnDTextCol = True
            '    If subStrs.HasFormats Then bReturnDTextCol = True
            'End If
            'If Not bReturnDTextCol Then
            subText = New Collection
            If aInstance > 1 Then subText.Add(aTxt.Clone) Else subText.Add(aTxt)
            'Else
            '    subText = aTxt.ToDText(tType, aInstances.Strukture, aImage)
            'End If
            eProps.SetVector(xtrus, aOCS.ZDirection, bSuppressed:=Math.Round(aOCS.ZDirection.Z, 6) = 1, bDirection:=True)
            If tType <> dxxTextTypes.Multiline Then
                tang = aOCS.XDirection.AngleTo(txtPln.XDirection, aOCS.ZDirection)
                aTxt.SetPropVal("Rotation", tang)
            End If
            For ti As Integer = 1 To subText.Count
                aTxt = subText.Item(ti)
                If aInstance > 1 Then
                    TTRANSFORMS.Apply(aTrs, aTxt, True)
                End If
                tstrings = aTxt.GetStrings(bReturnDTextCol, aImage)
                rProps = eProps
                If tType = dxxTextTypes.AttDef Or tType = dxxTextTypes.Attribute Then
                    rProps.SetVal("Attribute Tag", aTxt.AttributeTag)
                End If
                v1 = aTxt.AlignmentPt1V
                v2 = aTxt.AlignmentPt2V
                If scl <> 1 Then rProps.Scale(scl)
                rProps.SetVector(xtrus, aOCS.ZDirection, bSuppressed:=Math.Round(aOCS.ZDirection.Z, 6) = 1, bDirection:=True)
                If tType = dxxTextTypes.Multiline Then
                    v1 = aTxt.AlignmentPt1V
                    algn = aTxt.Alignment
                    v1 = tstrings.MTextAlignmentPt(v1, algn)
                    'rProps.Value("Text String") = algn
                    rProps.SetVector("AligmentPt 1", v1)
                    rProps.SetVector("X-Axis Direction", aPl.XDirection.Normalized)
                Else
                    If subText.Count > 1 Then rProps.SetVal("Text String", aTxt.TextString)
                    Dim vecs As TVECTORS = tstrings.AlignmentVectors(aOCS, v1, v2, algn, wf, New TTRANSFORMS)
                    rProps.SetVector("First alignment point(in OCS)", vecs.Item(1))
                    rProps.SetVector("Second alignment point(in OCS)", vecs.Item(2))
                End If
                If subText.Count > 1 Then
                    _rVal.Add(rProps, "SUBSTRING" & ti)
                Else
                    _rVal.Add(rProps, rTypeName)
                End If
            Next ti
            Return _rVal
        End Function
        Friend Shared Function GetArea(aEntity As dxfEntity) As Double
            ' rWellFormed = False
            If aEntity Is Nothing Then Return 0
            Try
                Select Case aEntity.GraphicType
                    Case dxxGraphicTypes.Arc
                        Dim aA As dxeArc = aEntity
                        Return dxfUtils.ArcArea(aA.Radius, aA.SpannedAngle, False)
                    Case dxxGraphicTypes.Bezier
                        Return 0
                    Case dxxGraphicTypes.Dimension
                        Return 0
                    Case dxxGraphicTypes.Ellipse
                        Dim aE As dxeEllipse = aEntity
                        If aE.SpannedAngle < 359.99 Then
                            Return aE.ArcStructure.PhantomPoints(200).AreaSummation(aE.Plane)
                        Else
                            Return Math.PI * aE.MinorRadius * aE.Radius
                        End If
                    Case dxxGraphicTypes.Hatch
                        Dim aHtch As dxeHatch = aEntity
                        aHtch.UpdatePath()
                        Return aHtch.BoundingEntities.TotalArea
                    Case dxxGraphicTypes.Hole
                        Dim aH As dxeHole = aEntity
                        Return dxfMath.HoleArea(aH.Radius, aH.MinorRadius, aH.Length, aH.IsSquare)
                    Case dxxGraphicTypes.Insert
                        Return 0
                    Case dxxGraphicTypes.Leader
                        Return 0
                    Case dxxGraphicTypes.Line
                        Return 0
                    Case dxxGraphicTypes.Point
                        Return 0
                    Case dxxGraphicTypes.Polygon
                        Dim aPg As dxePolygon = aEntity
                        Return dxfMath.PolylineArea(aPg)
                    Case dxxGraphicTypes.Polyline
                        Dim aPl As dxePolyline = aEntity
                        Return dxfMath.PolylineArea(aPl)
                    Case dxxGraphicTypes.Solid
                        Dim aSl As dxeSolid = aEntity
                        Return aSl.Vertices.AreaSummation
                    Case dxxGraphicTypes.Symbol
                        Return aEntity.BoundingRectangle.Area
                    Case dxxGraphicTypes.Table
                        Return aEntity.BoundingRectangle.Area
                    Case dxxGraphicTypes.Text
                        Return aEntity.BoundingRectangle.Area
                    Case dxxGraphicTypes.Shape
                        Return aEntity.BoundingRectangle.Area
                    Case Else
                        Return 0
                End Select
            Catch ex As Exception
                Return 0
            End Try
        End Function

        Public Shared Function DefPointCount(aGraphicType As dxxGraphicTypes) As Integer

            Select Case aGraphicType
                Case dxxGraphicTypes.Arc, dxxGraphicTypes.Ellipse, dxxGraphicTypes.Hatch, dxxGraphicTypes.Hole, dxxGraphicTypes.Insert, dxxGraphicTypes.Point, dxxGraphicTypes.Table, dxxGraphicTypes.Shape
                    Return 1
                Case dxxGraphicTypes.Line, dxxGraphicTypes.Text, dxxGraphicTypes.MText
                    Return 2
                Case dxxGraphicTypes.Bezier, dxxGraphicTypes.Solid
                    Return 4
                Case dxxGraphicTypes.Dimension
                    Return 7
                Case dxxGraphicTypes.Symbol
                    Return 6
                Case dxxGraphicTypes.Polygon
                    Return 1
                Case dxxGraphicTypes.Leader, dxxGraphicTypes.Polyline
                    Return 0
            End Select
            Return 0
        End Function
        Public Shared Function CreateAxis(aAxisDescriptor As dxxAxisDescriptors, Optional aPlane As dxfPlane = Nothing, Optional aLength As Double = 10) As dxeLine
            '^ returns a Line aligined with the Z axis of thw world coordinate systen or the passed coordinate system
            If aLength = 0 Then aLength = 1
            Dim sp As dxfVector
            If dxfPlane.IsNull(aPlane) Then sp = dxfVector.Zero Else sp = New dxfVector(aPlane.Origin)
            Dim aDir As dxfDirection
            Select Case aAxisDescriptor
                Case dxxAxisDescriptors.X
                    If dxfPlane.IsNull(aPlane) Then aDir = dxfDirection.WorldX Else aDir = aPlane.XDirection
                Case dxxAxisDescriptors.Y
                    If dxfPlane.IsNull(aPlane) Then aDir = dxfDirection.WorldY Else aDir = aPlane.YDirection
                Case Else
                    If dxfPlane.IsNull(aPlane) Then aDir = dxfDirection.WorldZ Else aDir = aPlane.ZDirection
            End Select
            Return New dxeLine(sp, sp + aDir * aLength)
        End Function

        Public Function IntersectionSegments() As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            Dim iSegs As TSEGMENTS = dxfIntersections.IntersectionSegments(Me, -1)
            Dim dsp As dxfDisplaySettings = DisplaySettings
            For i As Integer = 1 To iSegs.Count
                Dim iseg As TSEGMENT = iSegs.Item(i)
                If iseg.IsArc Then
                    _rVal.Add(New dxeArc(iseg.ArcStructure, dsp))
                Else
                    _rVal.Add(New dxeLine(iseg.LineStructure, aDisplaySettings:=dsp))
                End If

            Next
            Return _rVal
        End Function
        Friend Shared Function ProjectToPlane(aEntity As dxfEntity, aPlane As TPLANE, Optional bMaintainPlines As Boolean = False) As colDXFEntities
            Dim rOrthog As Boolean = False
            Dim rPlanar As Boolean = False
            Return ProjectToPlane(aEntity, aPlane, rOrthog, rPlanar, bMaintainPlines)
        End Function
        Friend Shared Function ProjectToPlane(aEntity As dxfEntity, aPlane As TPLANE, ByRef rOrthog As Boolean, ByRef rPlanar As Boolean, Optional bMaintainPlines As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            rOrthog = False
            rPlanar = False
            If aEntity Is Nothing Or TPLANE.IsNull(aPlane) Then Return _rVal
            Try
                Dim pPln As TPLANE = aPlane.Clone
                Dim ang As Double
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim aRec As TPLANE
                Dim aPths As TPATHS = aEntity.Paths
                Dim aPln As TPLANE = aEntity.Bounds
                Dim bPths As New TPATHS(dxxDrawingDomains.Model)
                Dim i As Integer
                Dim j As Integer
                Dim sEnts As colDXFEntities = Nothing
                Dim gType As dxxGraphicTypes = aEntity.GraphicType
                Dim vPts As TVECTORS
                Dim aVrts As TVERTICES
                Dim bLn As dxeLine
                Dim aDsp As dxfDisplaySettings = aEntity.DisplaySettings
                rOrthog = aPln.IsOrthogonalTo(pPln, ang, 3, rPlanar)
                If rOrthog And gType <> dxxGraphicTypes.Hole Then
                    'just return a line
                    aRec = aPths.ExtentVectors.Bounds(pPln)
                    v1 = aRec.Point(dxxRectanglePts.TopLeft)
                    v2 = aRec.Point(dxxRectanglePts.BottomRight)
                    bLn = _rVal.AddLineV(v1, v2, aMinLength:=0.001)
                    bLn.DisplaySettings = aDsp
                Else
                    Select Case gType
                        Case dxxGraphicTypes.Arc
                            Dim aA As dxeArc = aEntity
                            If rPlanar Then
                                aA = aA.Clone
                                aA.CenterV = aA.CenterV.ProjectedTo(pPln)
                                _rVal.Add(aA)
                            Else
                                bPths = aPths.ProjectedToPlane(pPln)
                            End If
                        Case dxxGraphicTypes.Bezier
                            Dim aB As dxeBezier = aEntity
                            bPths = aPths.ProjectedToPlane(pPln)
                        Case dxxGraphicTypes.Dimension
                            Dim aD As dxeDimension = aEntity
                            sEnts = aD.Entities
                        Case dxxGraphicTypes.Ellipse
                            Dim aE As dxeEllipse = aEntity
                            If rPlanar Then
                                aE = aE.Clone
                                aE.CenterV = aE.CenterV.ProjectedTo(pPln)
                                _rVal.Add(aE)
                            Else
                                bPths = aPths.ProjectedToPlane(pPln)
                            End If
                        Case dxxGraphicTypes.Hatch
                            Dim aH As dxeHatch
                            aH = aEntity
                            bPths = aPths.ProjectedToPlane(pPln)
                        Case dxxGraphicTypes.Hole
                            Dim aHl As dxeHole
                            aHl = aEntity
                            If rOrthog Then
                                aRec = aPths.ExtentVectors.Bounds(pPln)
                                If aHl.Depth <= 0 Then
                                    v1 = aRec.Point(dxxRectanglePts.TopLeft)
                                    v2 = aRec.Point(dxxRectanglePts.BottomRight)
                                    bLn = _rVal.AddLineV(v1, v2, aMinLength:=0.001)
                                    bLn.DisplaySettings = aDsp
                                Else
                                    v1 = aRec.Point(dxxRectanglePts.TopLeft)
                                    v2 = aRec.Point(dxxRectanglePts.BottomLeft)
                                    bLn = _rVal.AddLineV(v1, v2, aMinLength:=0.001)
                                    bLn.DisplaySettings = aDsp
                                    bLn.Identifier = "DEPTH LINE"
                                    v1 = v2
                                    v2 = aRec.Point(dxxRectanglePts.BottomRight)
                                    bLn = _rVal.AddLineV(v1, v2, aMinLength:=0.001)
                                    bLn.DisplaySettings = aDsp
                                    v1 = v2
                                    v2 = aRec.Point(dxxRectanglePts.TopRight)
                                    bLn = _rVal.AddLineV(v1, v2, aMinLength:=0.001)
                                    bLn.DisplaySettings = aDsp
                                    bLn.Identifier = "DEPTH LINE"
                                    v1 = v2
                                    v2 = aRec.Point(dxxRectanglePts.TopLeft)
                                    bLn = _rVal.AddLineV(v1, v2, aMinLength:=0.001)
                                    bLn.DisplaySettings = aDsp
                                End If
                            Else
                                If rPlanar Then
                                    sEnts = New colDXFEntities(aHl.BoundingEntity)
                                Else
                                    sEnts = New colDXFEntities(dxfPrimatives.CreateHoleSegments(aHl))
                                End If
                            End If
                        Case dxxGraphicTypes.Insert
                            Dim aI As dxeInsert
                            aI = aEntity
                            bPths = aPths.ProjectedToPlane(pPln)
                        Case dxxGraphicTypes.Leader
                            Dim aL As dxeLeader = aEntity
                            bPths = aPths.ProjectedToPlane(pPln)
                            sEnts = New colDXFEntities
                            If aL.LeaderType = dxxLeaderTypes.LeaderText Then
                                sEnts = New colDXFEntities(aL.MText)
                            ElseIf aL.EntityType = dxxEntityTypes.LeaderBlock Then
                                sEnts = New colDXFEntities(aL.Insert)
                            End If
                        Case dxxGraphicTypes.Line
                            Dim aLn As dxeLine = aEntity.Clone
                            aLn.StartPtV = aLn.StartPtV.ProjectedTo(aPlane)
                            aLn.EndPtV = aLn.EndPtV.ProjectedTo(aPlane)
                            aLn.PlaneV = aPlane
                            _rVal.Add(aLn)
                        Case dxxGraphicTypes.Point
                            Dim aP As dxePoint = aEntity.Clone
                            aP.Vector = aP.Vector.ProjectedTo(pPln)
                            _rVal.Add(aP)
                        Case dxxGraphicTypes.Polygon
                            Dim aPg As dxePolygon = aEntity.Clone
                            aPg.InsertionPtV = aPg.InsertionPtV.ProjectedTo(pPln)
                            If rPlanar Then
                                aPg.Vertices = aPg.Vertices.ProjectedToPlane(pPln)
                                _rVal.Add(aPg)
                            Else
                                sEnts = aPg.Segments
                            End If
                            If Not aPg.SuppressAdditionalSegments Then
                                For i = 1 To aPg.AdditionalSegments.Count
                                    _rVal.Append(dxfEntity.ProjectToPlane(aPg.AdditionalSegments.Item(i), aPlane, bMaintainPlines:=bMaintainPlines))
                                Next i
                            End If
                        Case dxxGraphicTypes.Polyline
                            Dim aPl As dxePolyline = aEntity.Clone
                            If rPlanar Then
                                aVrts = aPl.VerticesV.Clone
                                aVrts.ProjectTo(aPlane)
                                aPl.VerticesV = aVrts
                                _rVal.Add(aPl)
                            Else
                                If bMaintainPlines Then
                                    vPts = dxfProjections.ToPlane(New TVECTORS(aPl.PhantomPoints), aPlane)
                                    aPl.VectorsV = vPts
                                    aPl.PlaneV = aPlane
                                    _rVal.Add(aPl)
                                Else
                                    sEnts = aPl.Segments
                                End If
                            End If
                        Case dxxGraphicTypes.Solid
                            Dim aSl As dxeSolid = aEntity.Clone
                            aSl.PlaneV = aPlane
                            aSl.Vertex1V = aSl.Vertex1V.ProjectedTo(aPlane)
                            aSl.Vertex2V = aSl.Vertex2V.ProjectedTo(aPlane)
                            aSl.Vertex3V = aSl.Vertex3V.ProjectedTo(aPlane)
                            aSl.Vertex4V = aSl.Vertex4V.ProjectedTo(aPlane)
                            _rVal.Add(aSl)
                        Case dxxGraphicTypes.Symbol
                            Dim aSy As dxeSymbol = aEntity
                            sEnts = aSy.Entities
                        Case dxxGraphicTypes.Table
                            Dim aTB As dxeTable = aEntity
                            sEnts = aTB.Entities
                        Case dxxGraphicTypes.Text
                            Dim aTx As dxeText = aEntity
                            If rPlanar Then
                                aTx = aTx.Clone
                                aTx.AlignmentPt1V = aTx.AlignmentPt1V.ProjectedTo(aPlane)
                                aTx.AlignmentPt2V = aTx.AlignmentPt2V.ProjectedTo(aPlane)
                                _rVal.Add(aTx)
                            Else
                                bPths = aPths.ProjectedToPlane(aPlane)
                            End If
                        Case dxxGraphicTypes.Shape
                            Dim aShp As dxeShape = aEntity.Clone
                            aShp.PlaneV = aPlane
                            aShp.InsertionPtV = aShp.InsertionPtV.ProjectedTo(aPlane)
                    End Select
                End If
                If bPths.Count > 0 Then
                    For i = 1 To bPths.Count
                        TPATH.ToEntities(bPths.Item(i), _rVal)
                    Next i
                End If
                If sEnts IsNot Nothing Then
                    For j = 1 To sEnts.Count
                        _rVal.Append(dxfEntity.ProjectToPlane(sEnts.Item(j), aPlane, bMaintainPlines:=bMaintainPlines))
                    Next j
                End If
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Friend Shared Function SegmentPoint(aEntCol As colDXFEntities, aLengthFraction As Double) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            If aEntCol Is Nothing Then Return _rVal
            If aEntCol.Count <= 0 Then Return _rVal
            Dim aL As dxeLine
            Dim aA As dxeArc
            Dim totLeng As Double
            Dim aEnt As dxfEntity
            Dim Segs As Collection
            Dim aLng As Double
            Dim blng As Double
            '**UNUSED VAR** Dim cLeng As Double
            Dim aFrac As Double
            Dim bFrac As Double
            Dim idx As Long
            Dim eLng As Double
            If aLengthFraction < 0 Then aLengthFraction = 0
            If aLengthFraction > 1 Then aLengthFraction = 1
            aLengthFraction = Math.Round(aLengthFraction, 5)
            Segs = New Collection
            For i As Integer = 1 To aEntCol.Count
                'For Each aEnt In aEntCol.Values
                aEnt = aEntCol.Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Arc Or aEnt.GraphicType = dxxGraphicTypes.Line Then
                    If aLengthFraction = 0 Then
                        _rVal = aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt).Strukture
                        Return _rVal
                    Else
                        eLng = aEnt.Length
                        If eLng > 0 Then
                            Segs.Add(aEnt)
                            totLeng += eLng
                        End If
                    End If
                End If
            Next
            If aLengthFraction = 1 Then
                If Segs.Count > 0 Then
                    aEnt = Segs.Item(Segs.Count)
                    _rVal = aEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt).Strukture
                End If
            Else
                For i = 1 To Segs.Count
                    aEnt = Segs.Item(i)
                    eLng = aEnt.Length
                    aFrac = Math.Round((blng + eLng) / totLeng, 5)
                    If aFrac >= aLengthFraction And idx = 0 Then
                        idx = i
                    Else
                        If idx = 0 Then blng += eLng Else aLng += eLng
                    End If
                Next i
                If idx > 0 Then
                    aEnt = Segs.Item(idx)
                    eLng = aEnt.Length
                    bFrac = ((totLeng * aLengthFraction) - blng) / eLng
                    If aEnt.GraphicType = dxxGraphicTypes.Arc Then
                        aA = aEnt
                        _rVal = aA.AngleVector(bFrac * aA.SpannedAngle)
                    Else
                        aL = aEnt
                        _rVal = aL.StartPtV.Interpolate(aL.EndPtV, bFrac)
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function EntityLength(aEntity As dxfEntity, Optional aCurveDivisions As Integer = 100) As Double
            Dim _rVal As Double
            If aEntity Is Nothing Then Return 0

            Dim entdefpts As New TDEFPOINTS(aEntity.DefPts)
            'On Error Resume Next
            Select Case aEntity.GraphicType
                Case dxxGraphicTypes.Arc
                    Dim sa As Double = aEntity.Properties.ValueD("Start Angle")
                    Dim ea As Double = aEntity.Properties.ValueD("End Angle")
                    Dim cw As Boolean = aEntity.Properties.ValueB("*Clockwise")
                    Dim span As Double = dxfMath.SpannedAngle(cw, sa, ea)
                    Return (span * Math.PI) / 180 * aEntity.Properties.ValueD("Radius")
                Case dxxGraphicTypes.Bezier
                    Dim aB As New TBEZIER(entdefpts.DefPt1, entdefpts.DefPt2, entdefpts.DefPt3, entdefpts.DefPt4, entdefpts.Plane)
                    aB.PhantomLines(True, _rVal, aCurveDivisions, bComputLength:=True)
                    Return _rVal
                Case dxxGraphicTypes.Dimension
                    Return 0
                Case dxxGraphicTypes.Ellipse
                    Dim mjrad As Double = aEntity.Properties.ValueD("*MajorRadius")
                    Dim mnrad As Double = aEntity.Properties.ValueD("*MinorRadius")
                    Dim sa As Double = aEntity.Properties.ValueD("Start Angle")
                    Dim ea As Double = aEntity.Properties.ValueD("End Angle")
                    Dim cw As Boolean = aEntity.Properties.ValueB("*Clockwise")
                    Dim span As Double = dxfMath.SpannedAngle(cw, sa, ea)
                    If span >= 359.99 Then
                        Return 2 * Math.PI * Math.Sqrt(0.5 * (mnrad ^ 2 + mjrad ^ 2))  'EULERS APPROXIMATION
                    Else
                        Return aEntity.ArcStructure.PhantomPoints(aCurveDivisions).LengthSummation(False)
                    End If
                Case dxxGraphicTypes.Hatch
                    Return 0
                Case dxxGraphicTypes.Hole
                    Return aEntity.Properties.ValueD("Length")
                Case dxxGraphicTypes.Insert
                    Return 0
                Case dxxGraphicTypes.Leader
                    aEntity.UpdatePath()
                    Return aEntity.Vertices.LengthSummation(False)
                Case dxxGraphicTypes.Line
                    Return dxfProjections.DistanceTo(entdefpts.DefPt1, entdefpts.DefPt2)
                Case dxxGraphicTypes.Point
                    Return 0
                Case dxxGraphicTypes.Polygon
                    aEntity.UpdatePath()

                    Return New colDXFEntities(aEntity.Components.Segments).TotalLength
                Case dxxGraphicTypes.Polyline
                    aEntity.UpdatePath()

                    Return New colDXFEntities(aEntity.Components.Segments).TotalLength
                Case dxxGraphicTypes.Solid
                    Return aEntity.DefiningVectors.LengthSummation(True)
                Case dxxGraphicTypes.Symbol
                    Return aEntity.Properties.ValueD("Length")
                Case dxxGraphicTypes.Table
                    Return aEntity.BoundingRectangle.Width
                Case dxxGraphicTypes.Text, dxxGraphicTypes.MText
                    Return aEntity.BoundingRectangle.Width
                Case dxxGraphicTypes.Shape
                    Return aEntity.BoundingRectangle.Width
            End Select
            Return 0
        End Function
        Friend Overrides Sub RespondToEntitiesChangeEvent(aEvent As dxfEntitiesEvent)
            If _AddSegs Is Nothing Or aEvent Is Nothing Then Return
            aEvent.OwnerNotified = True
            If aEvent.EventType = dxxCollectionEventTypes.PreAdd Then
                If aEvent.Member Is Nothing Then aEvent.Undo = True
                If Not aEvent.Undo Then
                    Dim aEnt As dxfEntity
                    aEnt = aEvent.Member
                    aEnt.IsDirty = True
                    If aEnt.GraphicType <> dxxGraphicTypes.Arc And
aEnt.GraphicType <> dxxGraphicTypes.Line And
aEnt.GraphicType <> dxxGraphicTypes.Polyline And
             aEnt.GraphicType <> dxxGraphicTypes.Solid And aEnt.GraphicType <> dxxGraphicTypes.Point Then aEvent.Undo = True
                End If
            Else
                IsDirty = True
            End If
        End Sub
        Friend Sub RespondToEntitiesMemberChangeEvent(aEvent As dxfEntityEvent)
            IsDirty = True
        End Sub
        Friend Overrides Sub RespondToReactorChangeEvent(aEvent As dxfEntityEvent)
            aEvent.OwnerNotified = True
            IsDirty = True
        End Sub
        Public Shared Function SwapEntities(ByRef aEntity As dxfEntity, ByRef bEntity As dxfEntity, Optional aBooleanCondition As Boolean? = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the first vector
            '#2the second vector
            '#3a flag when evaluated as a boolean equals False will prevent the swap from being made
            '^swaps the two entities references if the third argument is not passed or if it evaluates to True
            '~Returns True if the swap was made
            If Not aBooleanCondition.HasValue Then
                _rVal = True
            Else
                _rVal = aBooleanCondition.Value
            End If
            If Not _rVal Then Return _rVal
            Dim aObj As dxfEntity
            aObj = aEntity
            aEntity = bEntity
            bEntity = aObj
            Return _rVal
        End Function

        Public Shared Function CloneCopy(aEntity As dxfEntity) As dxfEntity
            If aEntity Is Nothing Then Return Nothing
            Return aEntity.Clone
        End Function
#End Region 'Shared Methods
    End Class
End Namespace
