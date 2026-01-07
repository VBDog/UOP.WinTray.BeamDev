Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxePolygon
        Inherits dxfPolyline
        Implements ICloneable
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxPolylineTypes.Polygon)
            Init()
        End Sub


        Public Sub New(aEntity As dxePolygon, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxPolylineTypes.Polygon, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)

        End Sub

        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(dxxPolylineTypes.Polygon, aSubEntity, bNewGUID:=bNewGUID)
            Init()
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxPolylineTypes.Polygon)
            DefineByObject(aObject)
        End Sub
        Public Sub New(aVertices As IEnumerable(Of iVector), Optional aInsertPt As iVector = Nothing, Optional bClosed As Boolean = False, Optional aName As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aSegWidth As Double = 0)
            MyBase.New(dxxPolylineTypes.Polygon, aDisplaySettings, aVertices)
            Init()
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
            Vertices = New colDXFVectors(aVertices, bAddClones:=True)
            Closed = bClosed

            If aInsertPt IsNot Nothing Then InsertionPtV = New TVECTOR(aInsertPt) Else InsertionPtV = New TVECTOR(Vertices.ItemVector(1, True))
            If aSegWidth > 0 Then SegmentWidth = aSegWidth
            Name = aName
            BlockName = aName
        End Sub
        Friend Sub New(aVertices As TVECTORS, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aSegWidth As Double = 0)
            MyBase.New(dxxPolylineTypes.Polygon, aDisplaySettings, New colDXFVectors(aVertices))
            Init()
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)

            Closed = bClosed

            If aSegWidth > 0 Then SegmentWidth = aSegWidth
        End Sub
        Friend Sub New(aVertices As TVERTICES, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aSegWidth As Double = 0)
            MyBase.New(dxxPolylineTypes.Polygon, aDisplaySettings, New colDXFVectors(aVertices))
            Init()
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)

            Closed = bClosed

            If aSegWidth > 0 Then SegmentWidth = aSegWidth
        End Sub
        Public Sub New(aPolyline As dxePolyline)
            MyBase.New(dxxPolylineTypes.Polygon, aEntityToCopy:=aPolyline)
            Init()

        End Sub

        Public Sub New(aCoordinates As String, bClosed As Boolean, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "�")
            MyBase.New(dxxPolylineTypes.Polygon, aDisplaySettings, New colDXFVectors(aCoordinates))
            Init()
            '^a concantonated string of all the vertex coordinates of the polyline
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "�" (char 184))
            SegmentWidth = aSegmentWidth
            Closed = bClosed
            PlaneV = New TPLANE(aPlane)


        End Sub

        Private Overloads Sub Init()
            'MyBase.Init()
            AdditionalSegments = New colDXFEntities With {
            .Filter = New List(Of dxxEntityTypes)({dxxGraphicTypes.Arc, dxxGraphicTypes.Line, dxxGraphicTypes.Polyline, dxxGraphicTypes.Solid, dxxGraphicTypes.Point}),
            .OwnerGUID = GUID
            }
        End Sub
#End Region 'Constructors
#Region "Properties"

        Public Property BlockName As String
            Get
                Dim _rVal As String = PropValueStr("*BlockName")
                If _rVal = "" Then _rVal = Name
                Return _rVal
            End Get
            Set(value As String)
                Name = value
            End Set
        End Property
        Public Overrides Property Name As String
            Get
                Dim _rVal As String = MyBase.Name
                If String.IsNullOrEmpty(_rVal) Then _rVal = Identifier
                If String.IsNullOrEmpty(_rVal) Then _rVal = Tag
                If String.IsNullOrEmpty(_rVal) Then _rVal = GUID
                Return _rVal
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                MyBase.Name = value
                SetPropVal("*Name", value, False)
                SetPropVal("*BlockName", value)
            End Set
        End Property


        Public Overrides Function DefiningVectors() As colDXFVectors
            Dim _rVal As colDXFVectors = DefPts.DefiningVectors
            _rVal.Append(AdditionalSegments.DefiningVectors, False)
            Return _rVal

        End Function




        Public Property SuppressAdditionalSegments As Boolean
            Get
                Return PropValueB("*SuppressAdditionalSegments")
            End Get
            Set(value As Boolean)
                SetPropVal("*SuppressAdditionalSegments", value, AddSegs.Count > 0)
            End Set
        End Property


        ''' <summary>
        ''' a collection of entities that are included as part of the entities geometry
        ''' </summary>
        ''' <returns></returns>
        ''' 
        Friend Overrides Property AddSegs As colDXFEntities
            Get
                If MyBase.AddSegs Is Nothing Then MyBase.AddSegs = New colDXFEntities()
                Return MyBase.AddSegs

            End Get
            Set(value As colDXFEntities)
                MyBase.AddSegs = value

            End Set
        End Property

#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Throw New Exception("dxePolygons cannot be defined by Object")
        End Sub
        '^returns the dxePolygons holes and additional segments
        Public Overrides Function PersistentSubEntities() As List(Of dxfEntity)
            Return AdditionalSegments
        End Function

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing) As dxePolygon
            Dim _rVal As dxePolygon = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxePolygon
            Return New dxePolygon(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#End Region 'MustOverride Entity Methods
#Region "Methods"

        Public Function ToPolyline() As dxePolyline

            '^a dxePolyline that is the perimeter of the Polygon defined with the polycons vertices
            Return New dxePolyline(Vertices.Clone, Closed, DisplaySettings)

        End Function


        Public Function AddAdditionalSegment(aAddSeg As dxfEntity, Optional bAddClone As Boolean = False, Optional bAddCenterPt As Boolean = False, Optional aTag As Object = Nothing, Optional aFlag As Object = Nothing) As dxfEntity


            If aAddSeg Is Nothing Then Return Nothing
            Dim _rVal As dxfEntity = AdditionalSegments.Add(aAddSeg, bAddClone:=bAddClone, aTag:=aTag, aFlag:=aFlag)
            If _rVal Is Nothing Then Return Nothing
            If bAddCenterPt And _rVal.GraphicType <> dxxGraphicTypes.Point Then
                Dim aPt As New dxePoint(aAddSeg.DefinitionPoint(dxxEntDefPointTypes.Center), _rVal.DisplaySettings)
                aPt.TFVCopy(_rVal)
                AdditionalSegments.Add(aPt)
            End If
            Return _rVal


        End Function

        Public Function AddRelation(aIndex1 As Integer, aIndex2 As Integer, Optional aLineType As String = "", Optional aRelationship As dxxSegmentTypes = dxxSegmentTypes.Line, Optional aRadius As Double = 0.0, Optional aColor As dxxColors = dxxColors.Undefined, Optional bInverted As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "", Optional bRemoveVerts As Boolean = False, Optional aLayerName As String = "") As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '^used to add an additional segment based on two of the polygons existing boundiong vertices
            If aRelationship <> dxxSegmentTypes.Arc And aRelationship <> dxxSegmentTypes.Line Then aRelationship = dxxSegmentTypes.Line
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            If aIndex1 <= 0 Or aIndex1 > Vertices.Count Then Return _rVal
            If aIndex2 <= 0 Or aIndex2 > Vertices.Count Then Return _rVal
            aLineType = Trim(aLineType)
            If aLineType = "" Then aLineType = Linetype
            aLayerName = Trim(aLayerName)
            If aLayerName = "" Then aLayerName = LayerName
            If aColor = dxxColors.Undefined Then aColor = Color
            v1 = Vertices.Item(aIndex1)
            v2 = Vertices.Item(aIndex2)
            If aRelationship = dxxSegmentTypes.Arc Then
                _rVal = AdditionalSegments.AddArcPointToPoint(v1, v2, aRadius, aTag, aFlag, bInverted, aLineType, aColor, aLayerName)
            Else
                _rVal = AdditionalSegments.AddLine(v1, v2, New dxfDisplaySettings(aLayerName, aColor, aLineType), aTag, aFlag)
            End If
            If bRemoveVerts Then
                Vertices.RemoveMember(v1)
                Vertices.RemoveMember(v2)
            End If
            Return _rVal
        End Function
        Public Function AddRelationExt(ByRef Index1 As Integer, ByRef Index2 As Integer, Optional aLineType As String = "", Optional aRelationship As dxxSegmentTypes = dxxSegmentTypes.Line, Optional aRadius As Double = 0.0, Optional aColor As dxxColors = dxxColors.Undefined, Optional bClockwise As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "", Optional aLayerName As String = "") As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '^used to add an additional segment based on two of the polygons existing boundiong vertices
            If aRelationship <> dxxSegmentTypes.Arc And aRelationship <> dxxSegmentTypes.Line Then aRelationship = dxxSegmentTypes.Line
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            aLineType = Trim(aLineType)
            If aLineType = "" Then aLineType = Linetype
            aLayerName = Trim(aLayerName)
            If aLayerName = "" Then aLayerName = LayerName
            If aColor = dxxColors.Undefined Then aColor = Color
            If Index1 <= 0 Or Index1 > Vertices.Count Then Return _rVal
            If Index2 <= 0 Or Index2 > Vertices.Count Then Return _rVal
            v1 = Vertices.Item(Index1)
            v2 = Vertices.Item(Index2)
            If aRelationship = dxxSegmentTypes.Arc Then
                _rVal = AdditionalSegments.AddArcPointToPoint(v1, v2, aRadius, aTag, aFlag, bClockwise, aLineType, aColor, aLayerName)
            Else
                _rVal = AdditionalSegments.AddLine(v1, v2, New dxfDisplaySettings(aLayerName, aColor, aLineType), aTag, aFlag)
            End If
            Vertices.RemoveMember(v1)
            Vertices.RemoveMember(v2)
            Return _rVal
        End Function
        Public Function AddRelationX(Index1 As Integer, Index2 As Integer, Optional aLineType As String = "", Optional aRelationship As dxxSegmentTypes = dxxSegmentTypes.Line, Optional aRadius As Double = 0.0, Optional aColor As dxxColors = dxxColors.Undefined, Optional bClockwise As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "", Optional bContiguous As Boolean = False, Optional aLayerName As String = "") As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '^used to add an additional segment based on two of the polygons existing bounding vertices
            If aRelationship <> dxxSegmentTypes.Arc And aRelationship <> dxxSegmentTypes.Line Then aRelationship = dxxSegmentTypes.Line
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            If Not bContiguous Then
                If Index1 <= 0 Or Index1 > Vertices.Count Then Return _rVal
                If Index2 <= 0 Or Index2 > Vertices.Count Then Return _rVal
                v1 = Vertices.Item(Index1)
                v2 = Vertices.Item(Index2)
                If aRelationship = dxxSegmentTypes.Arc Then
                    _rVal = AdditionalSegments.AddArcPointToPoint(v1, v2, aRadius, aTag, aFlag, bClockwise, Linetype, Color, LayerName, Plane)
                Else
                    _rVal = AdditionalSegments.AddLine(v1, v2, New dxfDisplaySettings(LayerName, Color, Linetype), aTag, aFlag)
                End If
                If _rVal IsNot Nothing Then
                    _rVal.SuppressEvents = True
                    If aColor <> dxxColors.Undefined Then _rVal.Color = aColor Else _rVal.Color = Color
                    If aLineType <> "" Then _rVal.Linetype = aLineType Else _rVal.Linetype = Linetype
                    If aLayerName <> "" Then _rVal.LayerName = aLayerName Else _rVal.LayerName = LayerName
                    _rVal.SuppressEvents = False
                End If
            Else
                If Index1 <= 0 Then Index1 = 1
                If Index2 <= 0 Then Index2 = Vertices.Count
                If Index2 > Vertices.Count Then Index2 = Vertices.Count
                TVALUES.SortTwoValues(True, Index1, Index2)
                For i As Integer = Index1 To Index2 Step 2
                    v1 = Vertices.Item(i)
                    v2 = Vertices.Item(i + 1)
                    _rVal = Nothing
                    If aRelationship = dxxSegmentTypes.Arc Then
                        _rVal = AdditionalSegments.AddArcPointToPoint(v1, v2, aRadius, aTag, aFlag, bClockwise, Linetype, Color, LayerName, Plane)
                    Else
                        _rVal = AdditionalSegments.AddLine(v1, v2, DisplaySettings, aTag, aFlag)
                    End If
                    If _rVal IsNot Nothing Then
                        _rVal.SuppressEvents = True
                        If aColor <> dxxColors.Undefined Then _rVal.Color = aColor Else _rVal.Color = Color
                        If aLineType <> "" Then _rVal.Linetype = aLineType Else _rVal.Linetype = Linetype
                        _rVal.SuppressEvents = False
                    End If
                Next i
            End If
            _rVal.Color = aColor
            Return _rVal
        End Function
        Public Function AddRelationsByTag(aTagVal As String, RemoveVerts As Boolean, Optional aLineType As String = "", Optional aRelationship As dxxSegmentTypes = dxxSegmentTypes.Line, Optional aRadius As Double = 0.0, Optional aColor As dxxColors = dxxColors.Undefined, Optional bInverted As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "", Optional aLayerName As String = "") As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            If aRelationship <> dxxSegmentTypes.Arc And aRelationship <> dxxSegmentTypes.Line Then aRelationship = dxxSegmentTypes.Line
            '^used to add an additional segmenst based on two or more of the polygons existing boundiong vertices
            _rVal = New colDXFEntities
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            Dim verts As colDXFVectors
            Dim i As Integer
            verts = Vertices.GetByTag(aTagVal, bRemove:=RemoveVerts)
            If verts.Count <= 1 Then Return _rVal
            aLineType = Trim(aLineType)
            If aLineType = "" Then aLineType = Linetype
            aLayerName = Trim(aLayerName)
            If aLayerName = "" Then aLayerName = LayerName
            If aColor = dxxColors.Undefined Then aColor = Color
            For i = 1 To verts.Count Step 2
                If i + 1 > verts.Count Then Exit For
                v1 = verts.Item(i)
                v2 = verts.Item(i + 1)
                If aRelationship = dxxSegmentTypes.Arc Then
                    _rVal.Add(AdditionalSegments.AddArcPointToPoint(v1, v2, aRadius, aTag, aFlag, bInverted, aLineType, aColor, aLayerName))
                Else
                    _rVal.Add(AdditionalSegments.AddLine(v1, v2, New dxfDisplaySettings(aLayerName, aColor, aLineType), aTag, aFlag))
                End If
            Next i
            Return _rVal
        End Function


        Public Function Block(aBlockName As String, Optional bIncludesSubEntityInstances As Boolean = False, Optional aImage As dxfImage = Nothing, Optional aLTLSettings As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bCenterAtIP As Boolean = False, Optional aLayerName As String = "") As dxfBlock
            If String.IsNullOrWhiteSpace(aBlockName) Then aBlockName = BlockName
            Dim _rVal As New dxfBlock(aBlockName, Domain, Plane)
            UpdatePath(bRegen:=True, aImage:=aImage)
            Dim aLTL As dxsLinetypes = Nothing
            Dim aEnts As colDXFEntities
            Dim v1 As TVECTOR
            Dim bTrans As Boolean
            Dim aEnt As dxfEntity
            Dim lname As String = LayerName
            Dim iGUID As String = ImageGUID
            'Dim bEnts As New colDXFEntities
            Dim dolayers As Boolean = Not String.IsNullOrWhiteSpace(aLayerName)
            If aImage IsNot Nothing Then iGUID = aImage.GUID

            aEnts = New colDXFEntities(New dxfEntities(PathEntities), aImageGUID:=iGUID, bIncludeInstances:=bIncludesSubEntityInstances, bNoHandles:=True) With {.Suppressed = Suppressed} ' SubEntities(bIncludesSubEntityInstances)
            If bCenterAtIP Then
                v1 = InsertionPtV
            Else
                v1 = InsertionPtV * -1
                bTrans = Not TVECTOR.IsNull(v1)
            End If
            If aImage IsNot Nothing Then

                aLTL = aImage.LinetypeLayers
                If aLTLSettings = dxxLinetypeLayerFlag.Undefined Then
                    aLTLSettings = aLTL.Setting
                End If
                If aLTLSettings = dxxLinetypeLayerFlag.Undefined Or aLTLSettings = dxxLinetypeLayerFlag.Suppressed Then aLTL = Nothing
            End If
            If bTrans Or aLTL IsNot Nothing Or dolayers Then
                For i As Integer = 1 To aEnts.Count
                    aEnt = aEnts.Item(i)
                    If bTrans Then aEnt.Translate(v1)
                    If dolayers Then
                        If String.Compare(lname, aEnt.LayerName, ignoreCase:=True) = 0 Then
                            aEnt.LayerName = aLayerName
                        End If
                    End If

                    'bEnts.Add(aEnt)
                Next i
            End If
            If aLTL IsNot Nothing Then
                aLTL.ApplyTo(aEnts, aLTLSettings, aImage)
            End If
            _rVal.LayerName = TVALUES.To_STR(aLayerName, LayerName)
            _rVal.SetEntities(aEnts)
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aSubEnts As colDXFEntities
            If aInstances Is Nothing Then
                aInstances = Instances
                aInstances.IsParent = True
                aInstances.ParentPlane = Bounds
            End If
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            Dim bname As String = BlockName
            Dim aIns As dxeInsert
            Dim bSupr As Boolean
            Dim iProps As TPROPERTYARRAY
            Dim bnameerr As String = String.Empty
            If Components.Paths.Count <= 0 Then Return _rVal
            dxfUtils.ValidateBlockName(bname, bnameerr, bFixIt:=True, bAllowNull:=False, aBlocks:=aImage.Blocks)
            aSubEnts = SubEntities(True)
            bSupr = aSubEnts.Suppressed
            _rVal.Name = $"POLYGON_INSERT[{ GUID }]"
            For i = 1 To iCnt
                If i = 1 Then
                    rBlock = New dxfBlock(bname) With {
                .Suppressed = bSupr,
                .ImageGUID = aImage.GUID}
                    aSubEnts.TranslateV((InsertionPtV * -1), bSuppressEvnts:=True)
                    rBlock.SetEntities(aSubEnts)
                    aImage.HandleGenerator.AssignTo(rBlock)
                    aImage.Blocks.AddToCollection(rBlock, bSuppressEvnts:=True, bOverrideExisting:=True)
                End If
                aIns = New dxeInsert(rBlock) With {.PlaneV = PlaneV, .InsertionPtV = InsertionPtV, .GroupName = GroupName, .Suppressed = bSupr, .ImageGUID = aImage.GUID}
                aImage.HandleGenerator.AssignTo(aIns)
                'aIns.Instances = Instances.Clone
                Dim tname As String = String.Empty
                iProps = aIns.DXFProps(Instances, aInstance, PlaneV, tname, aImage:=aImage) ' .DXFFileProperties(Nothing, aImage, aInstance:=aInstance)
                If i = 1 Then
                    iProps.Name = "POLYGON_INSERT"
                Else
                    iProps.Name = $"POLYGON_INSERT - {i} INSTANCES"
                End If
                _rVal.Add(iProps)
            Next i

            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            If aInstances Is Nothing Then
                aInstances = Instances
                aInstances.IsParent = True
                aInstances.ParentPlane = PlaneV
            End If
            GetImage(aImage)
            Return New TPROPERTYARRAY(SubEntities(True).DXFProps(aInstances, aInstance, New dxfPlane(aOCS), rTypeName, aImage))
        End Function
        Public Function Divide(aSplitType As dxxSplitTypes, aSplitCenter As dxfVector, Optional aGap As Double = 0.0, Optional aAngle As Double = 0.0, Optional bReturnGap As Boolean = False) As colDXFEntities
            Return dxfBreakTrimExtend.split_Polygon(Me, aSplitType, aSplitCenter, aGap, aAngle, bReturnGap)
        End Function
        Public Function Divide(aSplitType As dxxSplitTypes, aSplitCenter As dxfVector, aGap As Double, aAngle As Double, bReturnGap As Boolean, ByRef rSplitOccured As Boolean) As colDXFEntities
            Return dxfBreakTrimExtend.split_Polygon(Me, aSplitType, aSplitCenter, aGap, aAngle, bReturnGap, rSplitOccured)
        End Function



        Public Function Hatch(aHatchType As dxxHatchStyle, aPatternName As String, Optional aScaleFactor As Double = 0.0, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLayerName As String = "", Optional aLineType As String = "") As dxeHatch
            Dim _rVal As dxeHatch
            Try
                If aHatchType < dxxHatchStyle.dxfHatchUserDefined Or aHatchType > dxxHatchStyle.dxfHatchPreDefined Then Throw New Exception("Un-Known Hatch Type Passed")
                _rVal = New dxeHatch With {
                    .Boundary = Me
                }
                If aColor <> dxxColors.Undefined Then _rVal.Color = aColor
                If aLayerName <> "" Then _rVal.LayerName = aLayerName
                If aLineType <> "" Then _rVal.Linetype = aLineType
                If aHatchType = dxxHatchStyle.dxfHatchPreDefined Then
                    _rVal.HatchStyle = aHatchType
                    _rVal.PatternName = aPatternName
                    _rVal.ScaleFactor = aScaleFactor
                End If
            Catch ex As Exception
                _rVal = Nothing
                Throw New Exception("dxePolygon.Hatch - " & ex.Message)
            End Try
            Return _rVal
        End Function

        Public Function Invert(Optional KeepChange As Boolean = False) As dxePolygon
            Dim _rVal As dxePolygon
            '#1flag to permanently invert the Polygon or to just return an inverted copy
            '^used to invert the Polygon
            '~returns a polygon with it's vertices swapped across a horizontal line thru it's center of area
            Dim Vertex As dxfVector
            Dim NextVertex As dxfVector
            Dim ydif As Double
            Dim Reversed As colDXFVectors
            Dim coa As dxfVector
            Reversed = New colDXFVectors
            coa = Center()
            'On Error Resume Next
            _rVal = Clone()
            'swap the y values around the center
            For i As Integer = 1 To _rVal.Vertices.Count
                Vertex = _rVal.Vertices.Item(i)
                ydif = Vertex.Y - coa.Y
                Vertex.Y = coa.Y - ydif
            Next i
            'now reverse the order
            Reversed.Add(_rVal.Vertices.Item(1, True))
            For i As Integer = _rVal.Vertices.Count To 2 Step -1
                Vertex = _rVal.Vertices.Item(i)
                Reversed.Add(Vertex, bAddClone:=True)
            Next i
            _rVal.Vertices.Clear()
            _rVal.Vertices = Reversed
            'now move the begin arc vertex back by one
            For i = _rVal.Vertices.Count To 1 Step -1
                Vertex = _rVal.Vertices.Item(i)
                If Vertex.Radius <> 0 Then
                    If i > 2 Then
                        NextVertex = _rVal.Vertices.Item(i - 1)
                    Else
                        NextVertex = _rVal.Vertices.Item(_rVal.Vertices.Count)
                    End If
                    NextVertex.Radius = Vertex.Radius
                    Vertex.Radius = 0
                    i -= 1
                End If
            Next i
            If KeepChange Then
                Vertices.Clear()
                For i As Integer = 1 To _rVal.Vertices.Count
                    Vertices.Add(_rVal.Vertices.Item(i, True))
                Next i
            End If
            Return _rVal
        End Function

        Public Sub Merge(aPolyEnt As dxfPolyline)
            If aPolyEnt Is Nothing Then Return
            AdditionalSegments.Append(aPolyEnt.Segments)
            AdditionalSegments.Append(aPolyEnt.AdditionalSegments, True)
        End Sub

        Public Function Perimeter(Optional bClosed As Boolean = False) As dxePolyline
            '^returns the bounding segments as a Polyline
            '^returns the entity as a Polyline
            Dim _rVal As New dxePolyline With {
                .SuppressEvents = True,
                .Closed = Closed Or bClosed,
                .DisplayStructure = DisplayStructure,
                .PlaneV = PlaneV,
                .Vertices = Vertices.Clone,
                .Identifier = "Polygon"
                }
            _rVal.SuppressEvents = False
            Return _rVal
        End Function

        Public Function SetColors(newval As dxxColors, Optional aSearchColor As dxxColors = dxxColors.Undefined, Optional DoAdditionalSegs As Boolean = True) As Boolean
            Dim _rVal As Boolean = False
            '^sets all segments to the passed color
            If newval = dxxColors.Undefined Then Return _rVal
            Dim sVal As String = String.Empty
            Dim cnt As Integer
            If aSearchColor <> dxxColors.Undefined Then
                TLISTS.Add(sVal, TVALUES.To_INT(aSearchColor))
            Else
                TLISTS.Add(sVal, TVALUES.To_INT(Color))
            End If
            TLISTS.Add(sVal, TVALUES.To_INT(dxxColors.ByBlock))
            cnt = Vertices.SetDisplayVariable(dxxDisplayProperties.Color, newval, sVal).Count
            If DoAdditionalSegs Then
                cnt += AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Color, newval, sVal).Count
            End If
            _rVal = cnt > 0
            If aSearchColor <> dxxColors.Undefined Then
                If Components.Paths.UpdateColors(aSearchColor, newval) Then _rVal = True
            Else
                If Components.Paths.UpdateColors(Color, newval) Then _rVal = True
            End If
            If TLISTS.Contains(Color, sVal) Then SetPropVal("Color", newval, True)
            '    If SetColors Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetLayerNames(newval As String, Optional aSearchType As Object = Nothing, Optional DoAdditionalSegs As Boolean = True) As Boolean
            Dim _rVal As Boolean = False
            '^sets all segments to the passed layername
            newval = Trim(newval)
            If newval = "" Then newval = "0"
            Dim sVal As String = String.Empty
            Dim cnt As Integer
            If aSearchType IsNot Nothing Then sVal = aSearchType.ToString().Trim()
            If sVal = "" Then sVal = LayerName
            cnt = Vertices.SetDisplayVariable(dxxDisplayProperties.LayerName, newval, sVal).Count
            If DoAdditionalSegs Then
                cnt += AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.LayerName, newval, sVal).Count
            End If
            If cnt > 0 Then _rVal = True
            If Components.Paths.UpdateLayers(sVal, newval) Then _rVal = True
            '    If SetLayerNames Then
            '        IsDirty = True
            '    End If
            If TLISTS.Contains(LayerName, sVal) Then SetPropVal("Layer", newval, True)
            Return _rVal
        End Function
        Public Function SetLineTypes(newval As String, Optional aSearchType As Object = Nothing, Optional DoAdditionalSegs As Boolean = True) As Boolean
            Dim _rVal As Boolean = False
            '^sets all segments to the passed linetype
            newval = newval.Trim()
            If String.IsNullOrWhiteSpace(newval) Then newval = dxfLinetypes.ByLayer
            Dim sVal As String = String.Empty
            Dim cnt As Integer
            If aSearchType IsNot Nothing Then sVal = aSearchType.ToString().Trim()
            If sVal = "" Then sVal = Linetype
            TLISTS.Add(sVal, "ByBLock")
            cnt = Vertices.SetDisplayVariable(dxxDisplayProperties.Linetype, newval, sVal).Count
            If DoAdditionalSegs Then
                cnt += AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, newval, sVal).Count
            End If
            _rVal = cnt > 0
            If Not IsDirty Then
                If Components.Paths.UpdateLinetypes(sVal, newval) Then _rVal = True
            End If
            If TLISTS.Contains(Linetype, sVal) Then SetPropVal("Linetype", newval, True)
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SubEntities() As colDXFEntities
            '^returns a VB collection of polylines, lines and arcs that are defined by the polygons vertices
            '^and vertex associations of the polygon.
            '~the dxePolygon is added to the dxf file as the sum of all its parts
            '^returns the individual entities that make up the polygon
            'If bIncludeInstances Then sEnts
            Return New colDXFEntities(PathEntities, aImageGUID:=ImageGUID, bIncludeInstances:=True, bNoHandles:=True)
        End Function
        Public Function SubEntities(bIncludeInstances As Boolean) As colDXFEntities
            '^returns a VB collection of polylines, lines and arcs that are defined by the polygons vertices
            '^and vertex associations of the polygon.
            '~the dxePolygon is added to the dxf file as the sum of all its parts
            '^returns the individual entities that make up the polygon
            Return New colDXFEntities(PathEntities, aImageGUID:=ImageGUID, bIncludeInstances) With {.Suppressed = Suppressed}
        End Function
        Public Function TransferedToImage(aImage As dxfImage, Optional aBlockName As String = "", Optional aLayer As String = "*", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "*", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bSuppressAddSegs As Object = Nothing) As dxePolygon
            '#1the subject image
            '#2the layer name to assign to the new polygon. if a null string is passed the layer name of the passed polygon is retained. if '*' is passed the new polygon is put on the current layer
            '#3the color to assign to the new polygon. if dxxColors.Undefined is passed the new polygon is assigned the current color. if dxxColors.ByBlock is passed the new polygons color is retained.
            '#4the linetype name to assign to the new polygon. if a null string is passed the linetype name of the passed polygon is retained. if '*' is passed the new polygon is assigned the current linetype
            '#5flag suppress defining the new polygon with respect to the current UCS
            '^used to copy the passed polygon to the images UCS and apply the images current settings
            Dim _rVal As dxePolygon
            Try
                _rVal = Clone()
                If aImage Is Nothing Then Return _rVal
                dxfImageTool.TransferPolygon(aImage, _rVal, aBlockName, aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation, aLTLSetting, bSuppressAddSegs)
                Return _rVal
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
        Public Function TrimWithArc(aTrimArc As dxeArc, Optional aKeepPoint As iVector = Nothing, Optional bTrimmerIsInfinite As Boolean = False, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False) As Boolean
            '#1the arc to trim with
            '#2a point to determine which side of the arc to keep
            '#3flag indicating the passed arc should be treated as infinite (360 degree span)
            '^trims the polyline with the passed arc
            Return dxfBreakTrimExtend.trim_Polyline_Arc(Me, aTrimArc, aKeepPoint, bTrimmerIsInfinite, bDoAddSegs, bDoSubPGons)
        End Function
        Public Function TrimWithLine(aTrimLine As iLine, aKeepPoint As iVector, Optional bTrimmerIsInfinite As Boolean = False, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False) As Boolean
            '#1line to trim with
            '#2a point to determine which side of the line to keep
            '#3flag indicating the passed line should be treated as infinite
            '^trims the polyline with the passed line
            Return dxfBreakTrimExtend.trim_Polyline_Line(Me, aTrimLine, New TVECTOR(aKeepPoint), bTrimmerIsInfinite, bDoAddSegs, bDoSubPGons)
        End Function
        Public Sub TrimWithOrthoLine(aTrimType As dxxTrimTypes, ByRef TrimCoordinate As Double, Optional rTrimPerformed As Boolean? = Nothing, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False)
            '#1the type of trim to perform
            '#2the x or y coordinate to trim at
            '#3return flag indicating if any actual trimming was performed
            '^trims the current polygon at the indicted coordinate
            '~only allows vertical or horizontal line trimming.
            Dim newPgon As dxePolygon
            Try
                Dim trimmed As Boolean? = False
                newPgon = dxfBreakTrimExtend.trim_Polygon_Ortho(Me, aTrimType, TrimCoordinate, trimmed, bDoAddSegs, bDoSubPGons)
                If rTrimPerformed IsNot Nothing Then rTrimPerformed = trimmed
                If trimmed Then
                    IsDirty = True
                    VerticesV = newPgon.VerticesV
                    Components = newPgon.Components
                End If
                newPgon = Nothing
            Catch ex As Exception
            End Try
        End Sub
        Public Sub TrimWithSegments(ByRef TrimSegments As colDXFEntities, Optional SegmentsAreInfinite As Boolean = False, Optional refPt As dxfVector = Nothing, Optional rTrimPerformed As Boolean? = Nothing)
            '#1a collection of segments to trim then polygon with
            '#2flag to treat the passed segments as infinite
            '^trims the polygon with the passed collection of polygon segments (lines & arcs)
            If TrimSegments Is Nothing Then Return
            If TrimSegments.Count <= 0 Then Return
            If refPt Is Nothing Then refPt = Center()
            Dim aSeg As dxfEntity
            Dim bWasTrimmed As Boolean
            If rTrimPerformed IsNot Nothing Then rTrimPerformed = False
            For i As Integer = 1 To TrimSegments.Count
                aSeg = TrimSegments.Item(i)
                If aSeg.GraphicType = dxxGraphicTypes.Arc Or aSeg.GraphicType = dxxGraphicTypes.Line Then
                    If aSeg.GraphicType = dxxGraphicTypes.Arc Then
                        bWasTrimmed = TrimWithArc(aSeg, refPt, SegmentsAreInfinite)
                    Else
                        bWasTrimmed = TrimWithLine(aSeg, refPt, SegmentsAreInfinite)
                    End If
                    If bWasTrimmed And rTrimPerformed IsNot Nothing Then rTrimPerformed = True
                End If
            Next i
        End Sub

        ''' <summary>
        ''' returns a properties object loaded with the entities current properties
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ActiveProperties() As dxoProperties
            Properties.Handle = Handle
            UpdateCommonProperties(bUpdateProperties:=True)

            Dim _rVal As New dxoProperties(Properties)
            _rVal.Append(Components.Segments.PolylineVertices(PlaneV, PropValueB("*Closed"), PropValueD("Constant Width")))

            Return _rVal
        End Function

#End Region 'Methods




    End Class 'dxePolygon
End Namespace
