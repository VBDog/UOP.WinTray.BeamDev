Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports Vanara.PInvoke

Namespace UOP.DXFGraphics.Structures

    Friend Structure TDEFPOINTS
        Implements ICloneable
#Region "Members"
        Public DefPt1 As TVECTOR
        Public DefPt2 As TVECTOR
        Public DefPt3 As TVECTOR
        Public DefPt4 As TVECTOR
        Public DefPt5 As TVECTOR
        Public DefPt6 As TVECTOR
        Public DefPt7 As TVECTOR
        Public IsDirty As Boolean
        Public OwnerGUID As String
        Public Plane As TPLANE
        Public SuppressEvents As Boolean
        Public Verts As TVERTICES

        Private _GraphicType As dxxGraphicTypes
        Private _DefPtCnt As Integer
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aUnits As dxxDeviceUnits = dxxDeviceUnits.Inches, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined)
            'init ----------------------------------------------
            DefPt1 = TVECTOR.Zero
            DefPt2 = TVECTOR.Zero
            DefPt3 = TVECTOR.Zero
            DefPt4 = TVECTOR.Zero
            DefPt5 = TVECTOR.Zero
            DefPt6 = TVECTOR.Zero
            DefPt7 = TVECTOR.Zero
            IsDirty = False
            OwnerGUID = ""
            Plane = TPLANE.World
            SuppressEvents = False
            Verts = New TVERTICES(0)
            _GraphicType = dxxGraphicTypes.Undefined
            _DefPtCnt = 0
            'init ----------------------------------------------
            Plane.Units = aUnits
            GraphicType = aGraphicType
        End Sub
        Public Sub New(aDefPts As TDEFPOINTS)
            'init ----------------------------------------------
            _GraphicType = aDefPts.GraphicType
            _DefPtCnt = aDefPts._DefPtCnt
            DefPt1 = New TVECTOR(aDefPts.DefPt1)
            DefPt2 = New TVECTOR(aDefPts.DefPt2)
            DefPt3 = New TVECTOR(aDefPts.DefPt3)
            DefPt4 = New TVECTOR(aDefPts.DefPt4)
            DefPt5 = New TVECTOR(aDefPts.DefPt5)
            DefPt6 = New TVECTOR(aDefPts.DefPt6)
            DefPt7 = New TVECTOR(aDefPts.DefPt7)
            IsDirty = aDefPts.IsDirty
            OwnerGUID = aDefPts.OwnerGUID
            Plane = New TPLANE(aDefPts.Plane)
            SuppressEvents = aDefPts.SuppressEvents
            Verts = New TVERTICES(aDefPts.Verts)

            'init ----------------------------------------------

        End Sub
        Public Sub New(aDefPts As dxpDefPoints)
            'init ----------------------------------------------
            DefPt1 = TVECTOR.Zero
            DefPt2 = TVECTOR.Zero
            DefPt3 = TVECTOR.Zero
            DefPt4 = TVECTOR.Zero
            DefPt5 = TVECTOR.Zero
            DefPt6 = TVECTOR.Zero
            DefPt7 = TVECTOR.Zero
            IsDirty = False
            OwnerGUID = IIf(aDefPts Is Nothing, "", aDefPts.OwnerGUID)
            Plane = TPLANE.World
            SuppressEvents = False
            Verts = New TVERTICES(0)
            _GraphicType = dxxGraphicTypes.Undefined
            _DefPtCnt = 0
            'init ----------------------------------------------
            If aDefPts Is Nothing Then Return
            GraphicType = aDefPts.GraphicType
            DefPt1 = New TVECTOR(aDefPts.GetVector(1))
            DefPt2 = New TVECTOR(aDefPts.GetVector(2))
            DefPt3 = New TVECTOR(aDefPts.GetVector(3))
            DefPt4 = New TVECTOR(aDefPts.GetVector(4))
            DefPt5 = New TVECTOR(aDefPts.GetVector(5))
            DefPt6 = New TVECTOR(aDefPts.GetVector(6))
            DefPt7 = New TVECTOR(aDefPts.GetVector(7))
            IsDirty = aDefPts.IsDirty
            Plane = New TPLANE(aDefPts.Plane)
            SuppressEvents = aDefPts.SuppressEvents
            Verts = New TVERTICES(aDefPts.Vertices)
        End Sub
#End Region 'Constructors
        Public ReadOnly Property DefPtCnt As Integer
            Get
                Return _DefPtCnt
            End Get
        End Property
        Public Property GraphicType As dxxGraphicTypes
            Get
                Return _GraphicType
            End Get
            Private Set(value As dxxGraphicTypes)
                If value <> _GraphicType Then
                    _DefPtCnt = dxfEntity.DefPointCount(value)
                    If HasVertices And Not TENTITY.HasVertices(value) Then
                        Verts = New TVERTICES(0)
                    End If

                End If
                _GraphicType = value
            End Set
        End Property
#Region "Methods"

        Public ReadOnly Property HasVertices As Boolean
            Get
                Return TENTITY.HasVertices(GraphicType)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"TDEFPOINTS[{ dxfEnums.Description(GraphicType) }]"
        End Function
        Public Sub Clear()
            Verts = TVERTICES.Zero
            DefPt1 = TVECTOR.Zero
            DefPt2 = TVECTOR.Zero
            DefPt3 = TVECTOR.Zero
            DefPt4 = TVECTOR.Zero
            DefPt5 = TVECTOR.Zero
            DefPt6 = TVECTOR.Zero
        End Sub
        Public Function Clone() As TDEFPOINTS
            Return New TDEFPOINTS(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDEFPOINTS(Me)
        End Function
#End Region 'Methods
    End Structure 'TDEFPOINTS
    Friend Structure TENTITY
        Implements ICloneable
        Implements IDisposable
#Region "Members"
        Public GraphicType As dxxGraphicTypes
        Public Components As TCOMPONENTS
        Public DefPts As TDEFPOINTS
        Public ExtendedData As TPROPERTYARRAY
        Public Handlez As THANDLES
        Public Instances As TINSTANCES
        Public Props As TPROPERTIES
        Public Reactors As TPROPERTYARRAY
        Public Style As TTABLEENTRY
        Public SubEntities As TENTITYARRAY
        Public SubProps As TPROPERTIES
        Public DimStyle As TTABLEENTRY
        Public TextStyle As TTABLEENTRY
        Public TagFlagValue As TTAGFLAGVALUE
        Public Strings As TSTRINGS
        Public Index As Integer
        Private disposedValue As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(aGraphicType As dxxGraphicTypes, Optional arGUID As String = "", Optional aDefPts As dxpDefPoints = Nothing, Optional aTextType As dxxTextTypes = dxxTextTypes.Undefined)
            'init ------------------------------------------------
            GraphicType = aGraphicType
            Handlez = New THANDLES(aGUID:=IIf(String.IsNullOrWhiteSpace(arGUID), dxfEvents.NextEntityGUID(GraphicType), arGUID))
            Components = New TCOMPONENTS(GraphicType, GUID)
            ExtendedData = New TPROPERTYARRAY(aOwner:=GUID)
            Instances = New TINSTANCES(GraphicType)
            DefPts = New TDEFPOINTS(aGraphicType:=GraphicType) With {.OwnerGUID = GUID}
            Props = dxpProperties.Get_EntityProps(GraphicType, GUID, aTextType:=aTextType)
            Reactors = New TPROPERTYARRAY(aOwner:=GUID)
            Style = New TTABLEENTRY(dxxReferenceTypes.UNDEFINED, "")
            TextStyle = New TTABLEENTRY(dxxReferenceTypes.STYLE, "")
            SubProps = New TPROPERTIES("SUBPROPS") With {.Owner = GUID}
            DimStyle = New TTABLEENTRY(dxxReferenceTypes.DIMSTYLE, "")
            SubEntities = New TENTITYARRAY("", "")
            TagFlagValue = New TTAGFLAGVALUE("")
            Strings = New TSTRINGS(DefPts.Plane, Handlez.Domain)
            disposedValue = False
            Index = -1
            'init ------------------------------------------------

            arGUID = GUID

            If aDefPts IsNot Nothing Then
                If aDefPts.GraphicType = aGraphicType Then DefPts = New TDEFPOINTS(aDefPts) With {.OwnerGUID = GUID}
            End If

            If aGraphicType = dxxGraphicTypes.Dimension Then
                ExtendedData.Add(New TPROPERTIES("ACAD"))
            End If
            If aGraphicType = dxxGraphicTypes.Insert Then
                SubEntities.Add(New TENTITIES(arGUID, "", "ATTRIBUTES"), "", True, False, True)
            End If
        End Sub

        Public Sub New(aEntity As TENTITY, Optional bCloneHandles As Boolean = True)
            'init ------------------------------------------------
            GraphicType = aEntity.GraphicType
            Handlez = IIf(bCloneHandles, New THANDLES(aEntity.Handlez), New THANDLES(dxfEvents.NextEntityGUID(GraphicType)))
            Components = New TCOMPONENTS(aEntity.Components, Handlez.GUID)
            DefPts = New TDEFPOINTS(aEntity.DefPts)
            ExtendedData = New TPROPERTYARRAY(aEntity.ExtendedData)
            Instances = New TINSTANCES(aEntity.Instances)
            Props = New TPROPERTIES(aEntity.Props) With {.Handle = Handle}
            Reactors = IIf(bCloneHandles, New TPROPERTYARRAY(aEntity.Reactors), New TPROPERTYARRAY("Reactors"))
            Style = New TTABLEENTRY(aEntity.Style)
            TextStyle = New TTABLEENTRY(aEntity.TextStyle)
            SubProps = New TPROPERTIES(aEntity.SubProps)
            DimStyle = New TTABLEENTRY(aEntity.DimStyle)

            SubEntities = New TENTITYARRAY(aEntity.SubEntities)
            TagFlagValue = New TTAGFLAGVALUE(aEntity.TagFlagValue)
            Strings = New TSTRINGS(aEntity.Strings)
            Index = aEntity.Index
            disposedValue = False
            'init ------------------------------------------------


            Props.SetVal("*GUID", GUID)
        End Sub
        Public Sub New(aEntity As dxfEntity, Optional bCloneHandles As Boolean = True, Optional bDontCloneParts As Boolean = True)
            'init ------------------------------------------------
            GraphicType = IIf(aEntity Is Nothing, dxxGraphicTypes.Undefined, aEntity.GraphicType)
            Handlez = New THANDLES(aGUID:=IIf(aEntity IsNot Nothing And bCloneHandles, aEntity.GUID, dxfEvents.NextEntityGUID(GraphicType)))
            Components = New TCOMPONENTS(GraphicType, GUID)
            ExtendedData = New TPROPERTYARRAY(aOwner:=GUID)
            Instances = New TINSTANCES(GraphicType)
            DefPts = New TDEFPOINTS(aGraphicType:=GraphicType) With {.OwnerGUID = GUID}
            Props = dxpProperties.Get_EntityProps(GraphicType, GUID)
            Reactors = IIf(bCloneHandles And Not aEntity Is Nothing, New TPROPERTYARRAY(aEntity.Reactors), New TPROPERTYARRAY(aOwner:=GUID))
            Style = New TTABLEENTRY(dxxReferenceTypes.UNDEFINED, "")
            TextStyle = New TTABLEENTRY(dxxReferenceTypes.STYLE, "")
            SubProps = New TPROPERTIES("SUBPROPS") With {.Owner = GUID}
            DimStyle = New TTABLEENTRY(dxxReferenceTypes.DIMSTYLE, "")
            SubEntities = New TENTITYARRAY("", "")
            TagFlagValue = New TTAGFLAGVALUE("")
            Strings = New TSTRINGS(DefPts.Plane, Handlez.Domain)
            Index = -1
            disposedValue = False
            'init ------------------------------------------------

            If aEntity IsNot Nothing Then
                Handlez.Domain = aEntity.Domain
                Dim statewuz As dxxEntityStates = aEntity.State
                aEntity.State = dxxEntityStates.Cloning

                'init ------------------------------------------------
                If bDontCloneParts Then

                    Handlez = aEntity.HStrukture
                    Components = aEntity.Components
                    DefPts = New TDEFPOINTS(aEntity.DefPts)
                    ExtendedData = New TPROPERTYARRAY(aEntity.ExtendedData)
                    Instances = New TINSTANCES(aEntity.Instances)
                    Props = New TPROPERTIES(aEntity.ActiveProperties())
                    Reactors = New TPROPERTYARRAY(aEntity.Reactors)
                    Style = aEntity.Style
                    TextStyle = aEntity.TextStyleStructure
                    SubProps = New TPROPERTIES(aEntity.SubProperties)
                    DimStyle = aEntity.DimStyleStructure

                    TagFlagValue = aEntity.TagFlagValue
                    Strings = New TSTRINGS(aEntity.Strings)
                    Index = aEntity.Index
                Else
                    Handlez = New THANDLES(aEntity)
                    Components = New TCOMPONENTS(aEntity.Components)
                    DefPts = New TDEFPOINTS(aEntity.DefPts)
                    ExtendedData = New TPROPERTYARRAY(aEntity.ExtendedData)
                    Instances = New TINSTANCES(aEntity.Instances)
                    Props = New TPROPERTIES(aEntity.ActiveProperties())
                    Reactors = New TPROPERTYARRAY(aEntity.Reactors)
                    Style = New TTABLEENTRY(aEntity.Style)
                    TextStyle = New TTABLEENTRY(aEntity.TextStyleStructure)
                    SubProps = New TPROPERTIES(aEntity.SubProperties)
                    DimStyle = New TTABLEENTRY(aEntity.DimStyleStructure)

                    TagFlagValue = New TTAGFLAGVALUE(aEntity.TagFlagValue)
                    Strings = New TSTRINGS(aEntity.Strings)
                    Index = aEntity.Index
                End If
                aEntity.State = statewuz
                'init ------------------------------------------------
            End If

            If Not bCloneHandles Then


                Handlez = New THANDLES(GUID, "")

            End If
            Props.Handle = Handle
            ExtendedData.Owner = GUID
            Reactors.Owner = GUID
            SubProps.Owner = GUID

            Props.SetVal("*GUID", GUID)
        End Sub
#End Region 'Constructors
#Region "Properties"

        Public ReadOnly Property IsText As Boolean
            Get
                Return GraphicType = dxxGraphicTypes.Text Or GraphicType = dxxGraphicTypes.MText
            End Get
        End Property

        Public Property Handle As String
            Get
                Return Handlez.Handle
            End Get
            Set(value As String)
                Handlez.Handle = value
            End Set
        End Property
        Public Property GUID As String
            Get
                Return Handlez.GUID
            End Get
            Set(value As String)
                Handlez.GUID = value
            End Set
        End Property
        Public Property ImageGUID As String
            Get
                Return Handlez.ImageGUID
            End Get
            Set(value As String)
                Handlez.ImageGUID = value
            End Set
        End Property
        Public Property OwnerGUID As String
            Get
                Return Handlez.OwnerGUID
            End Get
            Set(value As String)
                Handlez.OwnerGUID = value
            End Set
        End Property
        Public Property BlockGUID As String
            Get
                Return Handlez.BlockGUID
            End Get
            Set(value As String)
                Handlez.BlockGUID = value
            End Set
        End Property
        Public ReadOnly Property DisplayVars As TDISPLAYVARS
            Get
                Dim _rVal As New TDISPLAYVARS("0", dxfLinetypes.ByLayer, dxxColors.ByLayer, dxxLineWeights.ByLayer, 1)

                If Props.Count <= 0 Then Return _rVal
                _rVal.Color = Props.ValueI("Color", aDefault:=dxxColors.ByLayer)
                _rVal.LayerName = Props.ValueStr("Layername", "0")
                _rVal.Linetype = Props.ValueStr("Linetype", dxfLinetypes.ByLayer, True)
                _rVal.LTScale = Props.ValueD("LT Scale", True, True, 1)
                _rVal.LineWeight = Props.ValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer)
                _rVal.Suppressed = Props.ValueB(dxfLinetypes.Invisible)
                If IsText Then
                    _rVal.TextStyle = Props.ValueStr("Text Style Name", "Standard", True)
                End If
                Return _rVal
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"TENTITY[{ dxfEnums.Description(GraphicType) }]"
        End Function

        Public Sub UpdateCommonProperties(Optional aCadTypeName As String = "")
            TENTITY.UpdateCommonProps(Props, aCadTypeName)
        End Sub
        Public Function Clone(Optional aGUID As String = "", Optional bCloneHandles As Boolean = False, Optional aSourceGUID As String = "", Optional aImageGUID As String = "") As TENTITY
            Dim _rVal As New TENTITY(Me, bCloneHandles)

            _rVal.Props.SetVal("*SaveToFile", True)
            _rVal.Props.SetVal("*GroupName", "")
            _rVal.Props.SetVal("*GUID", aGUID)
            _rVal.Props.SetVal("*SourceGUID", aSourceGUID)

            Return _rVal
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TENTITY(Me)
        End Function

        Friend Function ExtentPts(Optional bSuppressInstances As Boolean = False) As TVECTORS
            Dim _rVal As TVECTORS = Components.Paths.ExtentVectors
            If Not bSuppressInstances And Instances.Count >= 1 Then _rVal = Instances.Apply(_rVal)
            Return _rVal
        End Function

        Friend Function Bounds(Optional aPlane As dxfPlane = Nothing, Optional bIncludeInstances As Boolean = False) As TPLANE
            '#1the subject entity
            '#2an optional plane to get the rectangle on
            '#3flag to include all the instances defined for then entity in the returned rectangle
            '^a rectangle that encompasses the entity
            '~if the plane is not passed the entity's definition plane is assumed
            Dim extpts As TVECTORS = ExtentPts(Not bIncludeInstances)
            If dxfPlane.IsNull(aPlane) Then
                Return extpts.Bounds(DefPts.Plane)
            Else
                Return extpts.Bounds(aPlane.Strukture)
            End If

        End Function
        Public Function GetEntity(Optional bNewGUID As Boolean = False) As dxfEntity
            Select Case GraphicType
                Case dxxGraphicTypes.Point
                    Return New dxePoint(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Line
                    Return New dxeLine(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Arc
                    Return New dxeArc(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Ellipse
                    Return New dxeEllipse(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Bezier
                    Return New dxeBezier(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Solid
                    Return New dxeSolid(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Polyline
                    Return New dxePolyline(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Shape
                    Return New dxeShape(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Polygon
                    Return New dxePolygon(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.MText
                    Return New dxeText(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Text
                    Return New dxeText(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Insert
                    Return New dxeInsert(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Leader
                    Return New dxeLeader(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Dimension
                    Return New dxeDimension(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Hatch
                    Return New dxeHatch(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Hole
                    Return New dxeHole(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Symbol
                    Return New dxeSymbol(Me, bNewGUID:=bNewGUID)
                Case dxxGraphicTypes.Table
                    Return New dxeTable(Me, bNewGUID:=bNewGUID)
                Case Else
                    Return Nothing
            End Select
        End Function

        Friend Function GetEntities() As TENTITIES
            Dim _rVal As New TENTITIES("", "")

            Dim bEnt As TENTITY = New TENTITY(Me)
            bEnt.Instances.Clear()
            _rVal.Add(bEnt)
            If Instances.Count > 0 Then
                For i As Integer = 1 To Instances.Count
                    _rVal.Add(Instances.Apply(i, bEnt))
                Next i
            End If
            Return _rVal
        End Function

#End Region 'Methods
#Region "Shared Methods"



        Public Shared Function DisplayVarsFromProperties(aProperties As TPROPERTIES) As TDISPLAYVARS
            Dim _rVal As New TDISPLAYVARS("0", dxfLinetypes.ByLayer, dxxColors.ByLayer, dxxLineWeights.ByLayer, 1)
            If aProperties.Count <= 0 Then Return _rVal
            _rVal.Color = aProperties.ValueI("Color", aDefault:=dxxColors.ByLayer)
            _rVal.LayerName = aProperties.ValueStr("Layername", aDefault:="0", bReturnDefaultForNullString:=True)
            _rVal.Linetype = aProperties.ValueStr("Linetype", aDefault:=dxfLinetypes.ByLayer, bReturnDefaultForNullString:=True)
            _rVal.LTScale = aProperties.ValueD("LT Scale", True, True, aDefault:=1)
            _rVal.LineWeight = aProperties.ValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer)
            _rVal.Suppressed = aProperties.ValueB(dxfLinetypes.Invisible)
            Return _rVal
        End Function

        Public Shared Function DisplayVarsFromProperties(aProperties As dxoProperties) As TDISPLAYVARS
            Dim _rVal As New TDISPLAYVARS("0", dxfLinetypes.ByLayer, dxxColors.ByLayer, dxxLineWeights.ByLayer, 1)
            If aProperties Is Nothing Then Return _rVal
            If aProperties.Count <= 0 Then Return _rVal
            _rVal.Color = aProperties.ValueI("Color", aDefault:=dxxColors.ByLayer)
            _rVal.LayerName = aProperties.ValueS("Layername", aDefault:="0")
            _rVal.Linetype = aProperties.ValueS("Linetype", aDefault:=dxfLinetypes.ByLayer)
            _rVal.LTScale = aProperties.ValueD("LT Scale", aDefault:=1)
            _rVal.LineWeight = aProperties.ValueI("LineWeight", aDefault:=dxxLineWeights.ByLayer)
            _rVal.Suppressed = aProperties.ValueB(dxfLinetypes.Invisible)
            Return _rVal
        End Function

        Public Shared Sub UpdateCommonProps(aEntProps As TPROPERTIES, Optional aCadTypeName As String = "")
            Dim aVal As Object
            Dim aclr As dxfColor
            aVal = aEntProps.ValueStr("Handle").Trim
            aEntProps.Handle = aVal
            'linetype scale
            aVal = aEntProps.ValueD("LT Scale")
            If aVal <= 0 Then aVal = 1
            aEntProps.SetVal("LTScale", aVal)
            'layer
            aVal = aEntProps.ValueStr("LayerName").Trim
            If aVal = "" Then aVal = "0"
            aEntProps.SetVal("LayerName", aVal)
            'linetype
            aVal = aEntProps.ValueStr("LineType").Trim
            If aVal = "" Then aVal = dxfLinetypes.ByLayer
            aEntProps.SetVal("LineType", aVal)
            'lineweight
            aVal = aEntProps.ValueI("LineWeight")
            If aVal < dxxLineWeights.ByBlock Or aVal > dxxLineWeights.LW_211 Then aVal = dxxLineWeights.ByLayer
            aEntProps.SetVal("LineWeight", aVal)
            'color
            aVal = aEntProps.ValueI("Color")
            If aVal = dxxColors.Undefined Then aVal = dxxColors.ByLayer
            aclr = dxfColors.Color(aVal)
            aVal = aclr.ACLValue
            aEntProps.SetVal("Color", aVal)
            If aclr.IsACL Then
                If aVal <> 0 And aVal <> 256 Then aEntProps.SetVal("Color Long Value", aclr.RGB.ToWin32(True))
                aEntProps.SetSuppressed("Color Long Value", True)
            Else
                aEntProps.SetVal("Color Long Value", aclr.RGB.ToWin32(True))
                aEntProps.SetSuppressed("Color Long Value", False)
            End If
            If aCadTypeName <> "" Then aEntProps.SetVal("Entity Type", aCadTypeName)
        End Sub
        Friend Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    Components.Clear()
                    DefPts.Clear()
                    ExtendedData.Clear()
                    Instances.Clear()
                    Props.Clear()
                    Reactors.Clear()
                    Style.Clear()
                    SubEntities.Clear()
                    SubProps.Clear()
                    DimStyle.Clear()
                    TextStyle.Clear()

                End If
                disposedValue = True
            End If
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        Public Shared Function HasVertices(aGraphicType As dxxGraphicTypes) As Integer

            Select Case aGraphicType

                Case dxxGraphicTypes.Symbol
                    Return True
                Case dxxGraphicTypes.Polygon
                    Return True
                Case dxxGraphicTypes.Leader, dxxGraphicTypes.Polyline
                    Return True
                Case Else
                    Return False
            End Select

        End Function

        Public Shared ReadOnly Property Null
            Get
                Return New TENTITY(aGraphicType:=dxxGraphicTypes.Undefined)
            End Get
        End Property
#End Region 'Shared MEthods"
    End Structure 'TENTITY

    Friend Structure TENTITIES
        Implements ICloneable
#Region "Members"
        Public Filter As String
        Public Handle As String
        Public ImageGUID As String
        Public Name As String
        Public OwnerGUID As String
        Private _Init As Boolean
        Private _Members() As TENTITY
#End Region 'Members
#Region "Constructors"

        Public Sub New(aCount As Integer)
            'init ----------------------------
            Filter = ""
            Handle = ""
            Name = ""
            OwnerGUID = ""
            _Init = True
            ReDim _Members(-1)
            'init ----------------------------
            For i As Integer = 1 To aCount
                Add(TENTITY.Null)
            Next i

        End Sub
        Public Sub New(Optional aOwnerGUID As String = "", Optional aImageGUID As String = "", Optional aName As String = "")
            'init ----------------------------
            Filter = ""
            Handle = ""
            Name = ""
            OwnerGUID = ""
            _Init = True
            ReDim _Members(-1)
            'init ----------------------------
            OwnerGUID = aOwnerGUID
            ImageGUID = aImageGUID
            Name = aName

        End Sub

        Public Sub New(aEntities As TENTITIES, Optional bClear As Boolean = False)
            'init ----------------------------
            Filter = aEntities.Filter
            Handle = aEntities.Handle
            Name = aEntities.Name
            OwnerGUID = aEntities.OwnerGUID
            _Init = True
            ReDim _Members(-1)
            'init ----------------------------
            If aEntities._Init And Not bClear Then _Members = aEntities._Members.Clone()

        End Sub
        Public Sub New(aEntities As IEnumerable(Of dxfEntity))
            'init ----------------------------
            Filter = ""
            Handle = ""
            Name = ""
            OwnerGUID = ""
            _Init = True
            ReDim _Members(-1)

            'init ----------------------------
            If aEntities Is Nothing Then Return
            For Each ent In aEntities
                Add(ent.GetStructure())
            Next

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As TENTITY
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TENTITY.Null
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TENTITY)
            If aIndex < 0 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Overrides Function ToString() As String
            Return $"TENTITIES[{ Count }]"
        End Function
        Public Sub Add(aMem As TENTITY, Optional bAssignIndices As Boolean = False)

            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aMem
            If bAssignIndices Then _Members(_Members.Count - 1).Index = _Members.Count
        End Sub
        Public Sub Add(aMem As dxfEntity, Optional bAssignIndices As Boolean = False)
            If aMem IsNot Nothing Then Add(aMem.GetStructure, bAssignIndices)
        End Sub
        Public Sub Clear()
            ReDim _Members(-1)
        End Sub

        Public Function Clone(Optional bClear As Boolean = False) As TENTITIES
            Return New TENTITIES(Me, bClear)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TENTITIES(Me)
        End Function

        Public Sub Append(bEnts As TENTITIES, Optional bClear As Boolean = False)
            If bEnts.Count <= 0 Then Return

            If bClear Then
                _Init = True
                ReDim _Members(-1)
            End If
            For i As Integer = 1 To bEnts.Count
                Add(bEnts.Item(i))
            Next i
        End Sub
        Public Function AttributesGet() As TPROPERTIES
            Dim _rVal As New TPROPERTIES("Attributes") With {.Delimiter = "~", .Owner = OwnerGUID}
            Dim aEnt As TENTITY
            Dim aTag As String
            _rVal = New TPROPERTIES("Attributes") With {.Delimiter = "~", .Owner = OwnerGUID}
            For i As Integer = 1 To Count
                aEnt = _Members(i - 1)
                If aEnt.Props.Value("*GraphicType") = dxxGraphicTypes.Text Then
                    If aEnt.Props.Count >= dxfGlobals.CommonProps Then
                        aTag = aEnt.Props.GCValueStr(2)
                        If Not String.IsNullOrWhiteSpace(aTag) Then
                            dxfAttributes.AddTagValue(_rVal, aTag, aEnt.Props.GCValueStr(1), aEnt.Props.GCValueStr(3))
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetByStringProperty(aGroupCode As Integer, aValue As String, bAssignIndices As Boolean) As TENTITIES
            Dim _rVal As TENTITIES = Clone(True)
            For i As Integer = 1 To Count
                Dim prop As New TPROPERTY
                If _Members(i - 1).Props.TryGet(aGroupCode, prop) Then
                    If String.Compare(prop.StringValue, aValue, True) = 0 Then
                        _rVal.Add(_Members(i - 1).Clone, bAssignIndices)
                    End If
                End If
            Next i
            Return _rVal
        End Function



        Public Function GetByIdentifier(aIdentifier As String, ByRef rIndex As Integer) As TENTITY
            rIndex = 0
            Dim _rVal As TENTITY = TENTITY.Null

            For i As Integer = 1 To Count
                Dim aMem As TENTITY = _Members(i - 1)
                If String.Compare(aMem.Props.ValueStr("*Identifier"), aIdentifier, ignoreCase:=True) = 0 Then
                    _rVal = aMem
                    rIndex = i
                    Exit For
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetByIdentifiers(aIdentifiers As String, Optional aDelimiter As String = ",", Optional bContainsString As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional bIgnoreSuppressed As Boolean = False) As TENTITIES
            Dim _rVal As New TENTITIES(0)

            Dim idents As TVALUES = TLISTS.ToValues(aIdentifiers, aDelimiter, False)

            Dim bTest As Boolean
            Dim aMem As TENTITY
            _rVal = Clone(True)

            For i As Integer = 1 To Count
                aMem = _Members(i - 1)
                bTest = aGraphicType = dxxGraphicTypes.Undefined Or aMem.Props.Value("*GraphicType") = aGraphicType
                If bTest And bIgnoreSuppressed Then
                    If aMem.Props.ValueB(dxfLinetypes.Invisible) Then
                        bTest = False
                    End If
                End If
                If bTest Then
                    For j As Integer = 1 To idents.Count
                        If Not bContainsString Then
                            If String.Compare(aMem.Props.ValueStr("*Identifier"), idents.Item(j), True) = 0 Then
                                _rVal.Add(aMem)
                            End If
                        Else
                            If aMem.Props.ValueStr("*Identifier").IndexOf(idents.Item(j), StringComparison.OrdinalIgnoreCase) + 1 > 0 Then
                                _rVal.Add(aMem)
                            End If
                        End If
                    Next j
                End If
            Next i
            Return _rVal
        End Function

        Public Function ToList() As List(Of TENTITY)
            If Count <= 0 Then Return New List(Of TENTITY)
            Return _Members.ToList()
        End Function
#End Region 'Methods
    End Structure 'TENTITIES
    Friend Structure TENTITYARRAY
        Implements ICloneable
#Region "Members"
        Public ImageGUID As String
        Private _Members() As TENTITIES
        Private _Init As Boolean
        Public OwnerGUID As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aOwnerGUID As String = "", Optional aImageGUID As String = "")
            'init -----------------------------
            OwnerGUID = aOwnerGUID
            ImageGUID = aImageGUID
            _Init = True
            ReDim _Members(-1)
            'init -----------------------------

        End Sub

        Public Sub New(aArray As TENTITYARRAY, Optional bClear As Boolean = False)
            'init -----------------------------
            OwnerGUID = aArray.OwnerGUID
            ImageGUID = aArray.ImageGUID
            _Init = True
            ReDim _Members(-1)
            'init -----------------------------
            If aArray.Count > 0 And Not bClear Then
                _Members = aArray._Members.Clone()
            End If
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then Return 0
                If _Members Is Nothing Then
                    _Init = True
                    ReDim _Members(-1)

                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"TENTITYARRAY[{ Count }]"
        End Function
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub
        Public Function Item(aIndex As Integer) As TENTITIES
            If aIndex < 1 Or aIndex > Count Then Return New TENTITIES(OwnerGUID, ImageGUID)
            If OwnerGUID <> "" Then _Members(aIndex - 1).OwnerGUID = OwnerGUID
            If ImageGUID <> "" Then _Members(aIndex - 1).ImageGUID = ImageGUID
            Return _Members(aIndex - 1)
        End Function
        Public Sub Add(aEnts As TENTITIES, Optional aName As String = "", Optional bSuppressSearch As Boolean = False, Optional bDontAddEmpty As Boolean = False, Optional bClearExisting As Boolean = False)
            Dim rIndex As Integer = 0

            Dim cnt As Integer = Count
            aName = IIf(Not String.IsNullOrWhiteSpace(aName), aName.Trim(), "")

            If aName = "" Then
                aName = IIf(Not String.IsNullOrWhiteSpace(aEnts.Name), aEnts.Name.Trim(), "")
            End If
            If bSuppressSearch Then
                rIndex = -1
            Else
                GetByName(aName, rIndex)
            End If
            If rIndex < 0 Then
                If Count >= Integer.MaxValue Then Return
                If bDontAddEmpty And aEnts.Count <= 0 Then Return
                cnt += 1
                rIndex = cnt - 1
                System.Array.Resize(_Members, cnt)
                _Members(rIndex) = aEnts
                _Members(rIndex).Name = aName.ToString()
            Else
                If bClearExisting Then
                    _Members(rIndex).Clear()
                End If
                If bDontAddEmpty And aEnts.Count <= 0 Then Return
                _Members(rIndex).Append(aEnts)
            End If
            If OwnerGUID <> "" Then _Members(rIndex).OwnerGUID = OwnerGUID
            If ImageGUID <> "" Then _Members(rIndex).ImageGUID = ImageGUID
        End Sub
        Public Function GetByName(aName As String, Optional bAddIfNotFound As Boolean = False, Optional aOwnerGUID As String = Nothing, Optional aImageGUID As String = Nothing) As TENTITIES
            Return GetByName(aName, 0, bAddIfNotFound, aOwnerGUID, aImageGUID)
        End Function
        Public Function GetByName(aName As String, ByRef rIndex As Integer, Optional bAddIfNotFound As Boolean = False, Optional aOwnerGUID As String = Nothing, Optional aImageGUID As String = Nothing) As TENTITIES
            Dim oGUID As String = IIf(aOwnerGUID IsNot Nothing, aOwnerGUID, OwnerGUID)
            Dim iGUID As String = IIf(aImageGUID IsNot Nothing, aImageGUID, ImageGUID)

            Dim _rVal As New TENTITIES(oGUID, iGUID) With {.Name = aName}
            Dim aMem As TENTITIES
            rIndex = -1
            For i As Integer = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.Name, aName, ignoreCase:=True) = 0 Then
                    _rVal = aMem
                    _rVal.ImageGUID = iGUID
                    _rVal.OwnerGUID = oGUID
                    rIndex = i - 1
                    Exit For
                End If
            Next i
            If rIndex = -1 And bAddIfNotFound And aName <> "" Then
                Add(_rVal, aName)
                rIndex = Count - 1
            End If
            Return _rVal
        End Function
        Public Function Clone(Optional bReturnEmpty As Boolean = False) As TENTITYARRAY
            Return New TENTITYARRAY(Me, bReturnEmpty)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TENTITYARRAY(Me)
        End Function

        Public Function Remove(aName As String) As TENTITIES
            Dim _rVal As New TENTITIES(OwnerGUID, ImageGUID) With {.Name = aName}
            'On Error Resume Next
            Dim rIndex As Integer = -1

            Dim rArray As TENTITYARRAY = Clone(True)

            For i As Integer = 1 To Count
                Dim aMem As TENTITIES = Item(i)
                If String.Compare(aMem.Name, aName, ignoreCase:=True) <> 0 Then
                    rArray.Add(aMem.Clone, aMem.Name, True, False, False)
                Else
                    _rVal = aMem
                    rIndex = i - 1
                End If
            Next i
            _Members = rArray._Members
            Return _rVal
        End Function
        Public Sub AddEnt(aEnt As TENTITY, aName As String, Optional bSuppressSearch As Boolean = False, Optional bClearExisting As Boolean = False)

            aName = aName.Trim()
            If aName = "" Or aEnt.Props.Count <= 0 Then Return
            Dim idx As Integer
            If bSuppressSearch Then
                idx = -1
            Else
                GetByName(aName, idx)
            End If
            If idx < 0 Then
                If Count >= Integer.MaxValue Then Return
                Dim nMem As New TENTITIES(OwnerGUID, ImageGUID) With {.Name = aName}
                nMem.Add(aEnt)
                Add(nMem)
                idx = Count
            Else
                If bClearExisting Then _Members(idx - 1).Clear()
                _Members(idx - 1).Add(aEnt)
            End If
            If idx > 0 Then
                If OwnerGUID <> "" Then _Members(idx - 1).OwnerGUID = OwnerGUID
                If ImageGUID <> "" Then _Members(idx - 1).ImageGUID = ImageGUID
            End If
        End Sub
        Friend Function GetEntity(aArrayName As String, aEntityIndex As Integer, ByRef rArrayIndex As Integer) As TENTITY
            Dim sEnts As TENTITIES = GetByName(aArrayName, rArrayIndex, False)
            If rArrayIndex >= 0 Then
                If aEntityIndex >= 0 And aEntityIndex <= sEnts.Count - 1 Then
                    Return sEnts.Item(aEntityIndex + 1)
                End If
            End If
            Return Nothing
        End Function
        Public Sub Update(aIndex As Integer, aMem As TENTITIES)

            If aIndex <= 0 Or aIndex < Count Then Return
            _Members(aIndex - 1) = aMem
            If OwnerGUID <> "" Then _Members(aIndex - 1).OwnerGUID = OwnerGUID
            If ImageGUID <> "" Then _Members(aIndex - 1).ImageGUID = ImageGUID
        End Sub
#End Region 'Methods
    End Structure 'TENTITYARRAY

End Namespace

Namespace UOP.DXFGraphics
    Public Class dxfEntities
        Inherits List(Of dxfEntity)
        Implements IEnumerable(Of dxfEntity)
        Implements ICloneable
#Region "Members"
        Public Instance As Integer
        Public Name As String
        Public Owner As String
        Public ImageGUID As String
#End Region 'Members

#Region "Constructors"

        Public Sub New()
            Instance = 0
            Name = String.Empty
            Owner = String.Empty
            ImageGUID = String.Empty
        End Sub

        Public Sub New(aName As String)
            Instance = 0
            Name = IIf(aName Is Nothing, String.Empty, aName)
            Owner = String.Empty
            ImageGUID = String.Empty
        End Sub

        Public Sub New(aEntities As IEnumerable(Of dxfEntity), Optional bAddClones As Boolean = True)
            Instance = 0
            Name = String.Empty
            Owner = String.Empty
            ImageGUID = String.Empty
            If aEntities Is Nothing Then Return
            If TypeOf aEntities Is colDXFEntities Then
                Dim dxf As colDXFEntities = DirectCast(aEntities, colDXFEntities)
                Owner = dxf.OwnerGUID
                ImageGUID = dxf.ImageGUID
                Name = dxf.Name
            ElseIf TypeOf aEntities Is dxfEntities Then
                Dim dxf As dxfEntities = DirectCast(aEntities, dxfEntities)
                Owner = dxf.Owner
                ImageGUID = dxf.ImageGUID
                Name = dxf.Name
                Instance = dxf.Instance
            End If


            For Each ent As dxfEntity In aEntities
                Add(ent, bAddClone:=bAddClones)
            Next

        End Sub

        Friend Sub New(aEntities As TENTITIES)
            Instance = 0
            Name = String.Empty
            Owner = String.Empty
            ImageGUID = String.Empty

            Owner = aEntities.OwnerGUID
            ImageGUID = aEntities.ImageGUID
            Name = aEntities.Name

            For i As Integer = 1 To aEntities.Count
                Add(aEntities.Item(i).GetEntity())
            Next

        End Sub

#End Region 'Constructors

#Region "Methods"

        Public Function TransferedToPlane(aPlane As dxfPlane) As dxfEntities
            Dim _rVal As dxfEntities = Me.Clone()
            dxfEntities.TransferEntitiesToPlane(_rVal, aPlane)
            Return _rVal
        End Function
        Public Sub TransferToPlane(aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing)
            dxfEntities.TransferEntitiesToPlane(Me, aPlane, aFromPlane)
        End Sub

        Public Function Clone() As dxfEntities
            Return New dxfEntities(Me, True)
        End Function
        Public Overloads Sub Add(aEntity As dxfEntity)
            If aEntity Is Nothing Then Return

            If Not String.IsNullOrWhiteSpace(ImageGUID) Then aEntity.ImageGUID = ImageGUID
            If Not String.IsNullOrWhiteSpace(Owner) Then aEntity.OwnerGUID = Owner
            MyBase.Add(aEntity)

        End Sub

        Public Function GetHandles(Optional aEntType As dxxEntityTypes? = Nothing) As List(Of String)
            Dim _rVal As New List(Of String)
            For Each ent As dxfEntity In Me
                If Not aEntType.HasValue Then
                    _rVal.Add(ent.Handle)
                Else
                    If ent.EntityType = aEntType.Value Then _rVal.Add(ent.Handle)
                End If
            Next
            Return _rVal
        End Function
        Public Overloads Sub Add(aEntity As dxfEntity, bAddClone As Boolean)
            If aEntity Is Nothing Then Return
            Dim newent As dxfEntity = IIf(bAddClone, aEntity.Clone(), aEntity)
            If Not String.IsNullOrWhiteSpace(ImageGUID) Then newent.ImageGUID = ImageGUID
            If Not String.IsNullOrWhiteSpace(Owner) Then newent.OwnerGUID = Owner
            MyBase.Add(newent)

        End Sub

        Public Function GetByIdentifiers(aIdentifiers As String, Optional aDelimiter As String = ",", Optional bContainsString As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional bIgnoreSuppressed As Boolean = False) As dxfEntities
            Return dxfEntities.GetByIdentifiers(Me, aIdentifiers, aDelimiter, bContainsString, aGraphicType,)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxfEntities(Me, True)
        End Function

#End Region 'Methods

#Region "Shared Methods"
        ''' <summary>
        ''' redefines the entity defining vectors using the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aEntities">the subject entities</param>
        ''' <param name="aPlane">the destination plane</param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        Public Shared Sub TransferEntitiesToPlane(aEntities As IEnumerable(Of dxfEntity), aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing)
            If aEntities Is Nothing Then Return
            If dxfPlane.IsNull(aPlane) Or aEntities.Count <= 0 Then Return
            For Each aEnt As dxfEntity In aEntities
                aEnt.TransferToPlane(aPlane, aFromPlane)
            Next
        End Sub
        ''' <summary>
        ''' returns a contiguous subset of the collection
        ''' </summary>
        ''' <remarks>if the ending index is null it is assumed to be the end of the collection</remarks>
        ''' <param name="aStartID">the starting index</param>
        ''' <param name="aEndID">the ending index</param>
        ''' <param name="aGraphicType">a filter for graphic type</param>
        ''' <returns></returns>
        Public Shared Function SubSet(aEntities As IEnumerable(Of dxfEntity), aStartID As Integer, aEndID As Integer?, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)

            If aEntities Is Nothing Then Return New List(Of dxfEntity)
            If aEntities.Count <= 0 Then Return New List(Of dxfEntity)
            Dim sid As Integer
            Dim eid As Integer
            If Not dxfUtils.LoopIndices(aEntities.Count, aStartID, aEndID, sid, eid) Then Return New List(Of dxfEntity)

            Dim elist As List(Of dxfEntity) = aEntities.ToList()
            Dim _rVal As List(Of dxfEntity) = elist.FindAll(Function(x) elist.IndexOf(x) + 1 >= sid And elist.IndexOf(x) + 1 <= eid)
            If aGraphicType <> dxxGraphicTypes.Undefined Then _rVal.RemoveAll(Function(x) x.GraphicType <> aGraphicType)

            Return _rVal
        End Function

        Public Shared Function TranslateEntities(aEntities As IEnumerable(Of dxfEntity), aTranslation As iVector, Optional aPlane As dxfPlane = Nothing) As Boolean
            If aEntities Is Nothing Then Return False
            Return TransformEntities(aEntities, TTRANSFORM.CreateTranslation(aTranslation.X, aTranslation.Y, aTranslation.Z, aPlane))

        End Function

        Friend Shared Function TransformEntities(aEntities As IEnumerable(Of dxfEntity), aTransform As TTRANSFORM) As Boolean
            Dim _rVal As Boolean = False
            If aEntities Is Nothing Then Return False
            If aEntities.Count <= 0 Or aTransform.IsUndefined Then Return _rVal
            For Each ent In aEntities
                If TTRANSFORM.Apply(aTransform, ent) Then _rVal = True
            Next

            Return _rVal
        End Function

        Public Shared Function GetByIdentifiers(aEntities As IEnumerable(Of dxfEntity), aIdentifiers As String, Optional aDelimiter As String = ",", Optional bContainsString As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional bIgnoreSuppressed As Boolean = False) As dxfEntities
            If aEntities Is Nothing Then Return New dxfEntities()

            Dim _rVal As New dxfEntities

            Dim idents As List(Of String) = TLISTS.ToStringList(aIdentifiers, aDelimiter, False)
            If idents.Count <= 0 Then Return _rVal


            For Each aMem As dxfEntity In aEntities
                If aMem Is Nothing Then Continue For
                If aGraphicType <> dxxGraphicTypes.Undefined And aMem.GraphicType <> aGraphicType Then Continue For
                If bIgnoreSuppressed Then
                    If aMem.Suppressed Then Continue For
                End If
                If Not bContainsString Then
                    If idents.FindIndex(Function(x) String.Compare(x, aMem.Identifier, True) = 0) >= 0 Then

                        _rVal.Add(aMem)
                    End If
                    Continue For
                End If
                For Each ident In idents
                    If aMem.Identifier.IndexOf(ident, StringComparison.OrdinalIgnoreCase) >= 0 Then
                        _rVal.Add(aMem)
                    End If

                Next

            Next
            Return _rVal
        End Function

        Friend Shared Function GetInstanceEntities(aEnt As dxfEntity) As dxfEntities
            If aEnt Is Nothing Then Return New dxfEntities()

            Dim _rVal As New dxfEntities() With {.Owner = aEnt.GUID, .ImageGUID = aEnt.ImageGUID}

            Dim bEnt As dxfEntity = aEnt.Clone
            bEnt.Instances.Clear()
            _rVal.Add(bEnt)
            Dim inst As dxoInstances = aEnt.Instances
            If inst.Count > 0 Then
                For i As Integer = 1 To inst.Count
                    _rVal.Add(inst.Apply(i, bEnt))
                Next i
            End If
            Return _rVal
        End Function

        Friend Shared Function ExtractAttDefs(aEntities As IEnumerable(Of dxfEntity), bReturnAsAttributes As Boolean, Optional aOwnerGUID As String = "", Optional aImageGUID As String = "") As TENTITIES
            Dim _rVal As New TENTITIES(aOwnerGUID, aImageGUID)
            If aEntities Is Nothing Then Return _rVal
            Dim elist As List(Of dxfEntity) = aEntities.ToList()
            Dim atdefs As List(Of dxfEntity) = elist.FindAll(Function(x) x.EntityType = dxxEntityTypes.Attdef)
            If atdefs.Count <= 0 Then Return _rVal

            For Each ent As dxfEntity In atdefs
                Dim atdef As dxeText = DirectCast(ent, dxeText)
                Dim txt As String = atdef.TextString
                If bReturnAsAttributes Then
                    atdef = atdef.Clone(Nothing, aNewTextType:=dxxTextTypes.Attribute)
                Else
                    atdef = atdef.Clone()
                End If
                atdef.TextString = txt
                Dim sEnt As TENTITY = atdef.GetStructure()
                sEnt.OwnerGUID = _rVal.OwnerGUID
                _rVal.Add(sEnt)
            Next

            If aImageGUID IsNot Nothing Then _rVal.ImageGUID = aImageGUID.Trim()
            Return _rVal
        End Function
        Friend Shared Function ExtractAttributeDefs(aEntities As IEnumerable(Of dxfEntity), bReturnAsAttributes As Boolean, Optional aOwnerGUID As String = Nothing, Optional aImageGUID As String = Nothing) As List(Of dxeText)
            Dim _rVal As New List(Of dxeText)
            If aEntities Is Nothing Then Return _rVal
            Dim elist As List(Of dxfEntity) = aEntities.ToList()
            Dim atdefs As List(Of dxfEntity) = elist.FindAll(Function(x) x.EntityType = dxxEntityTypes.Attdef)
            If atdefs.Count <= 0 Then Return _rVal

            For Each ent As dxfEntity In atdefs
                Dim atdef As dxeText = DirectCast(ent, dxeText)
                Dim txt As String = atdef.TextString
                If bReturnAsAttributes Then
                    atdef = atdef.Clone(Nothing, aNewTextType:=dxxTextTypes.Attribute)
                Else
                    atdef = atdef.Clone()
                End If

                If Not String.IsNullOrWhiteSpace(aOwnerGUID) Then atdef.OwnerGUID = aOwnerGUID
                If Not String.IsNullOrWhiteSpace(aImageGUID) Then atdef.ImageGUID = aImageGUID
                atdef.TextString = txt
                _rVal.Add(atdef)
            Next


            Return _rVal
        End Function

        Public Shared Function ExtractAttributes(aEntities As IEnumerable(Of dxfEntity), Optional aTextType As dxxTextTypes = dxxTextTypes.Undefined, Optional aOwnerGUID As String = Nothing) As dxfAttributes
            Dim _rVal As New dxfAttributes
            If aEntities Is Nothing Then Return _rVal
            Dim elist As List(Of dxfEntity) = aEntities.ToList()
            Dim txtents As List(Of dxeText) = elist.FindAll(Function(x) x.GraphicType = dxxGraphicTypes.Text).OfType(Of dxeText)().ToList()
            txtents.RemoveAll(Function(x) x.TextType <> dxxTextTypes.Attribute And x.TextType <> dxxTextTypes.AttDef)

            If aTextType <> dxxTextTypes.Undefined Then
                txtents.RemoveAll(Function(x) x.TextType <> aTextType)

            End If
            If txtents.Count <= 0 Then Return _rVal

            For Each ent As dxeText In txtents

                _rVal.Add(New dxfAttribute(ent, aOwnerGUID:=aOwnerGUID))
            Next


            Return _rVal
        End Function

        Friend Shared Function GetStructures(aEntities As IEnumerable(Of dxfEntity), bIncludeInstances As Boolean, Optional aOwnerGUID As String = "", Optional aImageGUID As String = "") As TENTITIES
            Dim _rVal As New TENTITIES(aOwnerGUID, aImageGUID)

            For Each ent As dxfEntity In aEntities
                Dim aEnt As TENTITY = ent.GetStructure()
                If Not bIncludeInstances Then
                    _rVal.Add(aEnt)
                Else
                    Dim sEnts As TENTITIES = aEnt.GetEntities
                    For j As Integer = 1 To sEnts.Count
                        _rVal.Add(sEnts.Item(j))
                    Next j
                End If
            Next

            Return _rVal
        End Function


        Public Shared Function DefiningVectors(aEntities As IEnumerable(Of dxfEntity)) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aEntities Is Nothing Then Return _rVal

            For Each ent As dxfEntity In aEntities
                _rVal.Append(ent.DefiningVectors)
            Next
            Return _rVal
        End Function

        Public Shared Function EncloseVector(aBounds As IEnumerable(Of dxfEntity), aVector As iVector, Optional aFudgeFactor As Double = 0.001, Optional aPlane As dxfPlane = Nothing, Optional bOnBoundIsIn As Boolean = True, Optional bUsedClosedSegments As Boolean = True) As Boolean
            Dim rIsonBound As Boolean = False
            Dim rIsEndPoint As Boolean = False
            Return EncloseVector(aVector, aBounds, aFudgeFactor, rIsonBound, rIsEndPoint, aPlane, bOnBoundIsIn, bUsedClosedSegments)
        End Function

        ''' <summary>
        ''' returns true if the passed vector is enclosed by the passed collection of entities
        ''' </summary>
        ''' <remarks>all entities are assumed to lie on the working plane</remarks>
        ''' <param name="aVector">the vector to search</param>
        ''' <param name="aBounds">the bounding segments to search</param>
        ''' <param name="aFudgeFactor">a fudge factor to apply</param>
        ''' <param name="rIsonBound">returns true if the passed vector is the start vector of the a member of the collection</param>
        ''' <param name="rIsEndPoint">returns true if the passed vector is the end vector of the a member of the collection</param>
        ''' <param name="aPlane">the plane to use</param>
        ''' <param name="bOnBoundIsIn">flag to treat a vector on the boundary as within the boundary</param>
        ''' <param name="bUsedClosedSegments">flag to treat gaps in the segments as lines</param>
        ''' <returns></returns>
        Public Shared Function EncloseVector(aVector As iVector, aBounds As IEnumerable(Of dxfEntity), aFudgeFactor As Double, ByRef rIsonBound As Boolean, ByRef rIsEndPoint As Boolean, Optional aPlane As dxfPlane = Nothing, Optional bOnBoundIsIn As Boolean = True, Optional bUsedClosedSegments As Boolean = True) As Boolean
            rIsonBound = False
            rIsEndPoint = False
            Return dxfEntities.EncloseVector(aBounds, New TVECTOR(aVector), aFudgeFactor, rIsonBound, rIsEndPoint, aPlane, bOnBoundIsIn, bUsedClosedSegments)
        End Function
        ''' <summary>
        ''' returns true if the passed vector is enclosed by the passed collection of entities
        ''' </summary>
        ''' <remarks>all entities are assumed to lie on the working plane</remarks>
        ''' <param name="aVector">the vector to search</param>
        ''' <param name="aBounds">the bounding segments to search</param>
        ''' <param name="aFudgeFactor">a fudge factor to apply</param>
        ''' <param name="rIsonBound">returns true if the passed vector is the start vector of the a member of the collection</param>
        ''' <param name="rIsEndPoint">returns true if the passed vector is the end vector of the a member of the collection</param>
        ''' <param name="aPlane">the plane to use</param>
        ''' <param name="bOnBoundIsIn">flag to treat a vector on the boundary as within the boundary</param>
        ''' <param name="bUsedClosedSegments">flag to treat gaps in the segments as lines</param>
        Friend Shared Function EncloseVector(aBounds As IEnumerable(Of dxfEntity), aVector As TVECTOR, aFudgeFactor As Double, rIsonBound As Boolean, rIsEndPoint As Boolean, Optional aPlane As dxfPlane = Nothing, Optional bOnBoundIsIn As Boolean = True, Optional bUsedClosedSegments As Boolean = True) As Boolean


            rIsonBound = False
            rIsEndPoint = False
            If aBounds Is Nothing Then Return False

            If aBounds.Count <= 0 Then Return False

            Dim d1 As Double
            Dim b1 As Boolean
            Dim B2 As Boolean
            Dim f1 As Double



            Dim aCS As dxfPlane
            Dim _rVal As Boolean
            If TPLANE.IsNull(aPlane) Then
                aCS = aBounds(0).Plane
            Else
                aCS = New dxfPlane(aPlane)
            End If
            f1 = TVALUES.LimitedValue(Math.Abs(aFudgeFactor), aMin:=0.000001, aMax:=0.1, 0.001)

            'chech the bounds
            Dim Segs As colDXFEntities


            If bUsedClosedSegments Then
                Segs = dxfEntities.GetClosedSegments(aBounds, 4, True)
            Else
                Segs = aBounds
            End If
            Dim bRect As dxfRectangle = Segs.BoundingRectangle(aCS)
            'see if its on my plane
            If Not aVector.LiesOn(bRect.Strukture, f1) Then Return False

            rIsonBound = dxfEntities.ContainsVector(Segs, aVector, f1, b1, B2)
            rIsEndPoint = b1 Or B2

            Dim Diagonal As Double = bRect.Diagonal
            If rIsonBound Then
                If bOnBoundIsIn Then
                    _rVal = True
                End If
            Else
                'gross check
                d1 = bRect.CenterV.DistanceTo(aVector)
                If d1 > (Diagonal / 2) + (3 * f1) Then
                    Return False
                End If
                Dim aLine As TLINE
                Dim iPts As New colDXFVectors
                Dim v1 As TVECTOR
                Dim aFlg As Boolean
                Dim bFlg As Boolean
                Dim cFlg As Boolean

                Dim coinc As Boolean
                Dim parl As Boolean
                aLine = New TLINE(aVector, aVector + bRect.XDirectionV * (3 * Diagonal))
                For i As Integer = 1 To Segs.Count
                    Dim aEnt As dxfEntity = Segs.Item(i)
                    If aEnt.GraphicType = dxxGraphicTypes.Line Then
                        Dim bL As dxeLine = DirectCast(aEnt, dxeLine)
                        v1 = aLine.IntersectionPt(New TLINE(bL), parl, coinc, aFlg, bFlg, cFlg)
                        If aFlg And bFlg And cFlg Then iPts.AddV(v1)
                    ElseIf aEnt.GraphicType = dxxGraphicTypes.Arc Then
                        Dim bA As dxeArc = DirectCast(aEnt, dxeArc)
                        aLine.IntersectionPts(bA.ArcStructure, True, False, False, 1000, iPts)
                    Else
                        Dim isegs As TSEGMENTS = aEnt.IntersectionSegments(1000)
                        For j As Integer = 1 To isegs.Count
                            Dim iseg As TSEGMENT = isegs.Item(j)
                            If iseg.IsArc Then
                                aLine.IntersectionPts(iseg.ArcStructure, True, False, False, 1000, iPts)
                            Else
                                v1 = aLine.IntersectionPt(iseg.LineStructure, parl, coinc, aFlg, bFlg, cFlg)
                                If aFlg And bFlg And cFlg Then iPts.Add(New dxfVector(v1))
                            End If
                        Next j


                    End If
                Next i


                If iPts.Count > 0 Then
                    _rVal = (iPts.Count Mod 2 <> 0)
                End If
            End If
            Return _rVal
        End Function

        Public Shared Function AddEntity(aEntCol As List(Of dxfEntity), aEnt As dxfEntity, Optional bAddClone As Boolean = False) As dxfEntity
            If aEntCol Is Nothing Or aEnt Is Nothing Then Return Nothing
            Dim _rVal As dxfEntity
            If bAddClone Then _rVal = aEnt.Clone() Else _rVal = aEnt
            aEntCol.Add(_rVal)
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the definition points of the entities in the array
        ''' </summary>
        ''' <param name="aEntities">the subject entities</param>
        ''' <param name="aPointType">a filter that can be applied for specific definition point type</param>
        ''' <param name="aEntityType">a filter for specific entity types</param>
        ''' <param name="aSearchTag">a filter to narrow the search by entity tag</param>
        ''' <param name="aSearchFlag">a filter to narrow the search by entity flag</param>
        ''' <param name="bReturnClones">a flag to return clones of the points</param>
        ''' <param name="aSuppressedVal">a filter to narrow the search to the enities current suppressed value</param>
        ''' <param name="aPlane"> the plane to use</param>
        ''' <returns></returns>
        Public Shared Function GetDefinitionPoints(aEntities As IEnumerable(Of dxfEntity), aPointType As dxxEntDefPointTypes, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aSearchTag As String = Nothing, Optional aSearchFlag As String = Nothing, Optional bReturnClones As Boolean = False, Optional aSuppressedVal As Boolean? = Nothing, Optional aPlane As dxfPlane = Nothing) As List(Of dxfVector)
            If aEntities Is Nothing Then Return New List(Of dxfVector)

            Dim source As List(Of dxfEntity) = aEntities.ToList()
            Dim srch As List(Of dxfEntity) = source

            Dim _rVal As New colDXFVectors With {.MaintainIndices = False}

            Dim noplane As Boolean = dxfPlane.IsNull(aPlane)

            Dim bTestSup As Boolean
            Dim bSupVal As Boolean
            If aEntityType <> dxxEntityTypes.Undefined Then srch = srch.FindAll(Function(mem) mem.EntityType = aEntityType)
            If aSuppressedVal IsNot Nothing Then
                bTestSup = aSuppressedVal.HasValue
                If bTestSup Then bSupVal = aSuppressedVal.Value
            End If
            If aSearchTag IsNot Nothing Then
                srch = srch.FindAll(Function(mem) String.Compare(mem.Tag, aSearchTag, ignoreCase:=True) = 0)
            End If
            If aSearchFlag IsNot Nothing Then
                srch = srch.FindAll(Function(mem) String.Compare(mem.Flag, aSearchFlag, ignoreCase:=True) = 0)
            End If
            For Each aEnt As dxfEntity In srch

                If bTestSup Then
                    If aEnt.Suppressed <> bSupVal Then Continue For
                End If

                Dim v1 As dxfVector = aEnt.DefinitionPoint(aPointType)
                If v1 Is Nothing Then Continue For
                If bReturnClones Then v1 = New dxfVector(v1)
                If Not noplane Then v1 = v1.WithRespectToPlane(aPlane)
                v1.Index = source.IndexOf(aEnt)
                v1.OwnerGUID = aEnt.GUID
                _rVal.Add(v1)

            Next

            Return _rVal
        End Function


        ''' <summary>
        ''' returns the definition points of the entities in the array
        ''' </summary>
        ''' <param name="aEntities">the subject entities</param>
        ''' <param name="aEntityType">a filter for specific entity types</param>
        ''' <param name="aSearchTag">a filter to narrow the search by entity tag</param>
        ''' <param name="aSearchFlag">a filter to narrow the search by entity flag</param>
        ''' <returns></returns>
        Public Shared Function GetInstanceMemberPoints(aEntities As IEnumerable(Of dxfEntity), Optional bReturnBasePts As Boolean = True, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aSearchTag As String = Nothing, Optional aSearchFlag As String = Nothing) As List(Of dxfVector)
            If aEntities Is Nothing Then Return New List(Of dxfVector)

            Dim source As List(Of dxfEntity) = aEntities.ToList()
            Dim srch As List(Of dxfEntity) = source

            Dim _rVal As New List(Of dxfVector)
            If aSearchTag IsNot Nothing Then
                srch = srch.FindAll(Function(mem) String.Compare(mem.Tag, aSearchTag, ignoreCase:=True) = 0)
            End If
            If aSearchFlag IsNot Nothing Then
                srch = srch.FindAll(Function(mem) String.Compare(mem.Flag, aSearchFlag, ignoreCase:=True) = 0)
            End If
            For Each aEnt As dxfEntity In srch

                Dim entpts As List(Of dxfVector) = aEnt.Instances.MemberPoints(aReturnBasePt:=bReturnBasePts)
                If entpts.Count > 0 Then _rVal.AddRange(entpts)

            Next

            Return _rVal
        End Function

        ''' <summary>
        '''  returns the segment entities
        ''' </summary>
        ''' <param name="aEntities"></param>
        ''' <param name="aPrecis"></param>
        ''' <param name="bContiguous"></param>
        ''' <returns></returns>
        Public Shared Function GetClosedSegments(aEntities As IEnumerable(Of dxfEntity), Optional aPrecis As Integer = 4, Optional bContiguous As Boolean = True) As colDXFEntities

            Dim _rVal As New colDXFEntities
            If aEntities Is Nothing Then Return _rVal
            Dim elist As List(Of dxfEntity) = aEntities.ToList()
            Dim ePts As List(Of dxfVector) = dxfEntities.GetDefinitionPoints(aEntities, aPointType:=dxxEntDefPointTypes.EndPt)
            Dim sPts As List(Of dxfVector) = dxfEntities.GetDefinitionPoints(aEntities, aPointType:=dxxEntDefPointTypes.StartPt)
            Dim ep As TVECTOR
            Dim sp As TVECTOR
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            For Each endpt As dxfVector In ePts
                _rVal.Add(elist.Find(Function(x) x.OwnerGUID = endpt.OwnerGUID))


                ep = New TVECTOR(endpt)
                Dim startpt As dxfVector = sPts.Find(Function(x) x.OwnerGUID = endpt.OwnerGUID)
                If Not bContiguous Then
                    If Not dxfVectors.ContainsVector(sPts, endpt, aPrecis:=aPrecis) Then
                        sp = New TVECTOR(dxfVectors.GetRelativeMember(True, sPts, endpt))

                        _rVal.Add(New dxeLine(ep, sp))
                    End If
                Else
                    If startpt IsNot Nothing Then
                        sp = New TVECTOR(startpt)
                        If Not ep.Equals(sp, aPrecis) Then _rVal.Add(New dxeLine(ep, sp))
                    End If
                End If

            Next

            If _rVal.Count > 1 Then
                ep = New TVECTOR(_rVal.LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt))
                sp = New TVECTOR(_rVal.FirstMember.DefinitionPoint(dxxEntDefPointTypes.StartPt))
                If Not ep.Equals(sp, aPrecis) Then _rVal.Add(New dxeLine(ep, sp))
            End If
            Return _rVal
        End Function

        Public Function ConvertArcsToLines(aEntities As IEnumerable(Of dxfEntity), Optional aCurveDivisions As Integer = 20) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '^returns a copy of the segments in the collection with the curved entities (ars,ellipses & beziers) converted to lines
            If aEntities Is Nothing Then Return _rVal
            For Each aEnt As dxfEntity In aEntities
                Select Case aEnt.GraphicType
                    Case dxxGraphicTypes.Arc
                        'convert ars to lines
                        Dim aA As dxeArc = DirectCast(aEnt, dxeArc)
                        _rVal.AddRange(aA.PhantomPoints(aCurveDivisions).ConnectingLines)
                    Case dxxGraphicTypes.Ellipse
                        'convert ars to lines
                        Dim aEl As dxeEllipse = DirectCast(aEnt, dxeEllipse)
                        _rVal.AddRange(aEl.PhantomPoints(aCurveDivisions).ConnectingLines)
                    Case dxxGraphicTypes.Bezier
                        'convert ars to lines
                        Dim aBz As dxeBezier = DirectCast(aEnt, dxeBezier)
                        _rVal.AddRange(aBz.PhantomPoints(aCurveDivisions).ConnectingLines)
                    Case Else
                        _rVal.Add(aEnt.Clone)
                End Select
            Next


            Return _rVal
        End Function

        ''' <summary>
        ''' returns true if the passed vector lies on the a member of the collection
        ''' </summary>
        ''' <param name="aEntities"></param>
        ''' <param name="aVector">the vector to test</param>
        ''' <param name="aFudgeFactor">a fudge factor to apply</param>
        ''' <param name="bTreatAsInfinite">flag to treat the members as infinite</param>
        ''' <param name="rSegIndices">returns the indices of the members that contains the vector</param>
        ''' <returns></returns>
        Friend Shared Function ContainsVector(aEntities As IEnumerable(Of dxfEntity), aVector As iVector, Optional aFudgeFactor As Double = 0.001, Optional bTreatAsInfinite As Boolean = False, Optional rSegIndices As List(Of Integer) = Nothing) As Boolean
            Dim rIsStartPt As Boolean
            Dim rIsEndPt As Boolean
            If aEntities Is Nothing Then Return False

            If aEntities.Count <= 0 Or aVector Is Nothing Then Return False

            Return dxfEntities.ContainsVector(aEntities, New TVECTOR(aVector), aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, rSegIndices)
        End Function

        ''' <summary>
        ''' returns true if the passed vector lies on the a member of the collection
        ''' </summary>
        ''' <param name="aEntities"></param>
        ''' <param name="aVector">the vector to test</param>
        ''' <param name="aFudgeFactor">a fudge factor to apply</param>
        ''' <param name="rIsStartPt">returns true if the passed vector is the start vector of the a member of the collection</param>
        ''' <param name="rIsEndPt">returns true if the passed vector is the end vector of the a member of the collection</param>
        ''' <param name="bTreatAsInfinite">flag to treat the members as infinite</param>
        ''' <param name="rSegIndices">returns the indices of the members that contains the vector</param>
        Friend Shared Function ContainsVector(aEntities As IEnumerable(Of dxfEntity), aVector As iVector, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, Optional bTreatAsInfinite As Boolean = False, Optional rSegIndices As List(Of Integer) = Nothing) As Boolean
            If aEntities Is Nothing Then Return False
            If aEntities.Count <= 0 Or aVector Is Nothing Then Return False
            Return dxfEntities.ContainsVector(aEntities, New TVECTOR(aVector), aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, rSegIndices)
        End Function

        ''' <summary>
        ''' returns true if the passed vector lies on the a member of the collection
        ''' </summary>
        ''' <param name="aEntities"></param>
        ''' <param name="aVector">the vector to test</param>
        ''' <param name="aFudgeFactor">a fudge factor to apply</param>
        ''' <param name="rIsStartPt">returns true if the passed vector is the start vector of the a member of the collection</param>
        ''' <param name="rIsEndPt">returns true if the passed vector is the end vector of the a member of the collection</param>
        ''' <param name="bTreatAsInfinite">flag to treat the members as infinite</param>
        ''' <param name="rSegIndices">returns the indices of the members that contains the vector</param>
        Friend Shared Function ContainsVector(aEntities As IEnumerable(Of dxfEntity), aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, Optional bTreatAsInfinite As Boolean = False, Optional rSegIndices As List(Of Integer) = Nothing) As Boolean
            Dim _rVal As Boolean = False

            rIsStartPt = False
            rIsEndPt = False
            If aEntities Is Nothing Then Return False

            If aEntities.Count <= 0 Then Return False
            'Returns True if the passed vector lies on this a member of the collection



            Dim v1 As New dxfVector(aVector)
            Dim i As Integer = 0
            For Each aEnt As dxfEntity In aEntities
                i += 1
                Dim bFlag1 As Boolean
                Dim bFlag2 As Boolean

                If aEnt.ContainsPoint(v1, aFudgeFactor, bFlag1, bFlag2, bTreatAsInfinite) Then
                    _rVal = True
                    If rSegIndices IsNot Nothing Then rSegIndices.Add(i)
                End If
                If bFlag1 Then rIsStartPt = True
                If bFlag2 Then rIsEndPt = True
            Next
            Return _rVal
        End Function

        Friend Shared Function Paths(aEntities As IEnumerable(Of dxfEntity), Optional bRegen As Boolean = False, Optional aImage As dxfImage = Nothing) As TPATHS


            Dim _rVal As New TPATHS(dxxDrawingDomains.Model)
            If aEntities Is Nothing Then Return _rVal

            If aEntities.Count <= 0 Then Return _rVal

            For Each aMem As dxfEntity In aEntities

                aMem.UpdatePath(bRegen, aImage)
                Dim aPths As TPATHS = aMem.Paths
                _rVal.ExtentVectors.Append(aPths.ExtentVectors)
                _rVal.Append(aPths, True)
            Next
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the shortest member in the collection 
        ''' </summary>
        ''' <param name="aEntities"></param>
        ''' <param name="bGetClone"></param>
        ''' <param name="aType"></param>
        ''' <returns></returns>
        Public Shared Function ShortestMember(aEntities As IEnumerable(Of dxfEntity), Optional bGetClone As Boolean = False, Optional aType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As dxfEntity
            If aEntities Is Nothing Then Return Nothing
            If aEntities.Count <= 0 Then Return Nothing
            Dim cval As Double = Double.MaxValue


            Dim idx As Integer = 0
            Dim i As Integer = 1
            For Each aEnt As dxfEntity In aEntities

                If aType = dxxGraphicTypes.Undefined Or (aType <> dxxGraphicTypes.Undefined And aEnt.GraphicType = aType) Then
                    Dim lVal As Double = aEnt.Length
                    If lVal < cval Then
                        cval = lVal
                        idx = i
                    End If
                End If
                i += 1
            Next
            Dim _rVal As dxfEntity = Nothing

            If idx > 0 Then
                _rVal = aEntities(idx - 1)
                If bGetClone Then _rVal = _rVal.Clone()
            End If

            Return _rVal

        End Function

        ''' <summary>
        ''' returns the longest member in the collection 
        ''' </summary>
        ''' <param name="aEntities"></param>
        ''' <param name="bGetClone"></param>
        ''' <param name="aType"></param>
        ''' <returns></returns>
        Public Shared Function LongestMember(aEntities As IEnumerable(Of dxfEntity), Optional bGetClone As Boolean = False, Optional aType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As dxfEntity
            If aEntities Is Nothing Then Return Nothing
            If aEntities.Count <= 0 Then Return Nothing
            Dim cval As Double = Double.MinValue


            Dim idx As Integer = 0
            Dim i As Integer = 1
            For Each aEnt As dxfEntity In aEntities

                If aType = dxxGraphicTypes.Undefined Or (aType <> dxxGraphicTypes.Undefined And aEnt.GraphicType = aType) Then
                    Dim lVal As Double = aEnt.Length
                    If lVal > cval Then
                        cval = lVal
                        idx = i
                    End If
                End If
                i += 1
            Next
            Dim _rVal As dxfEntity = Nothing

            If idx > 0 Then
                _rVal = aEntities(idx - 1)
                If bGetClone Then _rVal = _rVal.Clone()
            End If

            Return _rVal

        End Function

        Public Shared Function GetNestedInserts(aEntities As IEnumerable(Of dxfEntity), aImage As dxfImage) As List(Of dxeInsert)
            If aEntities Is Nothing Or aImage Is Nothing Then Return New List(Of dxeInsert)
            Dim _rVal As New List(Of dxeInsert)()

            Dim iserts As List(Of dxeInsert) = aEntities.ToList().FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Insert).ConvertAll(Function(ent) CType(ent, dxeInsert))
            For Each isert As dxeInsert In iserts
                _rVal.Add(isert)
                Dim iblock As dxfBlock = Nothing
                If isert.GetBlock(aImage, iblock) Then
                    _rVal.AddRange(dxfEntities.GetNestedInserts(iblock.Entities, aImage))   'recursion
                End If
            Next

            Return _rVal

        End Function

        ''' <summary>
        ''' returns the bounding rectangle of the passed entities on the passed plane
        ''' </summary>
        ''' <remarks>the bounding rectangle encloses all the defined Extent Points of each entity projected to te subject plane</remarks>
        ''' <param name="aEntities">the subject entities</param>
        ''' <param name="aPlane">the plane to define the rectangle on. if null, the global XY plane is assumed.</param>
        ''' <param name="bIncludeSuppressed">flag to include the extent points of the entites that are marked as suppressed </param>
        ''' <param name="bSuppressInstances">flag to exclude the extent points of the entites instances if any are defined</param>
        ''' <param name="aWidthAdder">a fixed distance to add to the returned rectangles width</param>
        ''' <param name="aHeightAdder">a fixed distance to add to the returned rectangles height</param>
        ''' <param name="aScaleFactor">a factor to scale the size of the returned rectangle</param>
        ''' <returns></returns>
        Public Shared Function BoundingRectangle(aEntities As IEnumerable(Of dxfEntity), Optional aPlane As dxfPlane = Nothing, Optional bIncludeSuppressed As Boolean = False, Optional bSuppressInstances As Boolean = False, Optional aWidthAdder As Double = 0, Optional aHeightAdder As Double = 0, Optional aScaleFactor As Double? = Nothing) As dxfRectangle
            Dim _rVal As New dxfRectangle(ExtentPoints(aEntities, bIncludeSuppressed, bSuppressInstances), aPlane, False)
            If aWidthAdder <> 0 Then _rVal.Width += aWidthAdder
            If aHeightAdder <> 0 Then _rVal.Height += aHeightAdder
            If aScaleFactor.HasValue Then
                _rVal.Rescale(aScaleFactor.Value)
            End If
            Return _rVal

        End Function

        ''' <summary>
        ''' ^the points used to define the entities bounding rectangle
        ''' </summary>
        ''' <param name="aEntities">the subject entities</param>
        ''' <param name="bIncludeSuppressed">flag to include suppressed entities in the return</param>
        ''' <param name="bSuppressInstances">flag to exclude the extent points of the entities defined instances</param>
        ''' <returns></returns>
        Public Shared Function ExtentPoints(aEntities As IEnumerable(Of dxfEntity), Optional bIncludeSuppressed As Boolean = False, Optional bSuppressInstances As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors()
            If aEntities Is Nothing Then Return _rVal

            For Each ent As dxfEntity In aEntities
                If bIncludeSuppressed Or (Not bIncludeSuppressed And Not ent.Suppressed) Then
                    _rVal.Append(ent.ExtentPts(bSuppressInstances), ent.Handle)
                End If

            Next

            Return _rVal

        End Function


        ''' <summary>
        ''' returns the members whose specified definition point match the passed location
        ''' </summary>
        ''' <param name="aEntities">the set to search</param>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="aVectorType">the entity defintion point type to seach by</param>
        ''' <param name="bReturnClones">flag to return clone</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="bGetJustOne">flag to return the first match only</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Shared Function GetByDefPoint(aEntities As IEnumerable(Of dxfEntity), aMatchPoint As iVector, Optional aVectorType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bGetJustOne As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aEntities Is Nothing Then Return _rVal
            If aMatchPoint Is Nothing Then aMatchPoint = dxfVector.Zero
            For Each aEnt As dxfEntity In aEntities
                If aEnt Is Nothing Then Continue For
                If aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType <> aEntityType Then Continue For
                If aGraphicType <> dxxGraphicTypes.Undefined And aEnt.GraphicType <> aGraphicType Then Continue For
                Dim P1 As dxfVector = aEnt.DefinitionPoint(aVectorType)
                If P1 IsNot Nothing Then
                    If P1.DistanceTo(aMatchPoint, aPrecis) = 0 Then
                        If Not bReturnClones Then _rVal.Add(aEnt) Else _rVal.Add(aEnt.Clone())
                        If bGetJustOne Then Exit For
                    End If

                End If

            Next
            Return _rVal
        End Function
        ''' <summary>
        ''' returns the members whose specified definition point is nearest to the passed location
        ''' </summary>
        ''' <param name="aEntities">the set to search</param>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="aVectorType">the entity defintion point type to seach by</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Shared Function GetByNearestDefPoint(aEntities As IEnumerable(Of dxfEntity), aMatchPoint As iVector, Optional aVectorType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPrecis As Integer = 3, Optional bReturnClone As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            If aEntities Is Nothing Then Return _rVal
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If aMatchPoint Is Nothing Then aMatchPoint = dxfVector.Zero

            Dim d1 As Double = Double.MaxValue
            Dim d2 As Double = 0
            For Each aEnt As dxfEntity In aEntities
                If aEnt Is Nothing Then Continue For
                If aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType <> aEntityType Then Continue For
                If aGraphicType <> dxxGraphicTypes.Undefined And aEnt.GraphicType <> aGraphicType Then Continue For

                Dim P1 As dxfVector = aEnt.DefinitionPoint(aVectorType)
                If P1 Is Nothing Then Continue For

                d2 = P1.DistanceTo(aMatchPoint, aPrecis)
                If d2 <= d1 Then
                    d1 = d2
                    _rVal = aEnt
                End If

            Next

            If _rVal IsNot Nothing Then
                If bReturnClone Then _rVal = _rVal.Clone
            End If
            Return _rVal
        End Function

#End Region 'Shared Methods'
    End Class

End Namespace
