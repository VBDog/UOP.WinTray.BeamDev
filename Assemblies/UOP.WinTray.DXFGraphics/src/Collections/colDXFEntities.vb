Imports System.Reflection
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke

Namespace UOP.DXFGraphics
    Public Class colDXFEntities
        Inherits List(Of dxfEntity)  'Inherits dxfHandleOwner
        Implements IDisposable
        Implements IEnumerable(Of dxfEntity)
        Implements iHandleOwner
#Region "Members"
        'Private _Events As dxpEventHandler
        Private _ErrorString As String

        Private _SuppressEvents As Boolean
        Private _MaintainIndices As Boolean

        Private _BlockName As String
        Private _Invalid As Boolean
        Private _Suppressed As Boolean
        Private disposedValue As Boolean
#End Region 'Members
#Region "Events"
        Public Event EntitiesMemberChange(aEvent As dxfEntityEvent)
        Public Event EntitiesChange(aEvent As dxfEntitiesEvent)
#End Region 'Events
#Region "Constructors"
        Private Overloads Sub Init()
            ' MyBase.Init("")
            _ErrorString = String.Empty

            _SuppressEvents = False
            _MaintainIndices = False
            _BlockName = String.Empty
            _Suppressed = False
            disposedValue = False
        End Sub

        Public Sub New()
            '  MyBase.New("")
            Init()

        End Sub
        Friend Sub New(aSegments As TSEGMENTS)
            'MyBase.New("")
            Init()
            ArcLineStructures_Set(aSegments)
        End Sub

        Public Sub New(aSegments As List(Of iPolylineSegment))
            'MyBase.New("")
            Init()
            If aSegments Is Nothing Then Return
            For Each seg As iPolylineSegment In aSegments
                If seg Is Nothing Then Continue For
                If seg.SegmentType = dxxSegmentTypes.Arc Then
                    Add(DirectCast(seg, dxeArc))
                ElseIf seg.SegmentType = dxxSegmentTypes.Line Then
                    Add(DirectCast(seg, dxeLine))
                End If
            Next
        End Sub
        Friend Sub New(aImage As dxfImage, Optional aGUID As String = "")
            ' MyBase.New("")
            Init()
            If aImage IsNot Nothing Then
                GUID = dxfEvents.NextEntitiesGUID(aGUID)
                SetImage(aImage, False)
                MonitorMembers = True
                MaintainIndices = True
            End If
        End Sub
        Friend Sub New(aLines As TLINES, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            'MyBase.New("")
            Init()
            If aLines.Count <= 0 Then Return
            Dim aLn As New dxeLine(New dxfVector, New dxfVector, aDisplaySettings)
            For i As Integer = 1 To aLines.Count
                aLn.LineStructure = aLines.Item(i)
                Add(aLn.Clone)
            Next
        End Sub
        Friend Sub New(aSubEnts As TENTITIES, bIncludeInstances As Boolean, Optional bNoHandles As Boolean = False, Optional aImageGUID As String = Nothing)
            ' MyBase.New("")
            Init()
            Dim iGUID As String = TVALUES.To_STR(aImageGUID, ImageGUID)
            SubEntities_Set(aSubEnts, bIncludeInstances, bNoHandles, iGUID)
        End Sub
        Public Sub New(aEntity As dxfEntity)
            'MyBase.New("")
            Init()
            If aEntity IsNot Nothing Then
                If aEntity.HasReferenceTo_Image Then SetImage(aEntity.Image, False)
                MyBase.Add(aEntity)
            End If
        End Sub
        Public Sub New(aEntity As dxfEntity, bEntity As dxfEntity)
            'MyBase.New("")
            Init()
            If aEntity IsNot Nothing Then
                If aEntity.HasReferenceTo_Image Then SetImage(aEntity.Image, False)
                MyBase.Add(aEntity)
            End If
            If bEntity IsNot Nothing Then MyBase.Add(bEntity)
        End Sub
        Public Sub New(aEntity As dxfEntity, bEntity As dxfEntity, cEntity As dxfEntity)
            'MyBase.New("")
            Init()
            If aEntity IsNot Nothing Then
                If aEntity.HasReferenceTo_Image Then SetImage(aEntity.Image, False)
                MyBase.Add(aEntity)
            End If
            If bEntity IsNot Nothing Then MyBase.Add(bEntity)
            If cEntity IsNot Nothing Then MyBase.Add(cEntity)
        End Sub
        Public Sub New(aEntity As dxfEntity, bEntity As dxfEntity, cEntity As dxfEntity, dEntity As dxfEntity)
            ' MyBase.New("")
            Init()
            If aEntity IsNot Nothing Then
                If aEntity.HasReferenceTo_Image Then SetImage(aEntity.Image, False)
                MyBase.Add(aEntity)
            End If
            If bEntity IsNot Nothing Then MyBase.Add(bEntity)
            If cEntity IsNot Nothing Then MyBase.Add(cEntity)
            If dEntity IsNot Nothing Then MyBase.Add(dEntity)
        End Sub

        Friend Sub New(aEntities As IEnumerable(Of dxfEntity), aImageGUID As String, bIncludeInstances As Boolean, Optional bNoHandles As Boolean = False, Optional aSuppressedValue As Boolean? = Nothing, Optional bGetClones As Boolean = True)
            'MyBase.New("")
            Init()
            If aEntities Is Nothing Then Return
            Dim iGUID As String = TVALUES.To_STR(aImageGUID, ImageGUID)
            SubEntities_Set(aEntities, bIncludeInstances, bNoHandles, iGUID, aSuppressedVal:=aSuppressedValue, bGetClones:=bGetClones)
        End Sub


        Public Sub New(aEntities As IEnumerable(Of dxfEntity), Optional bReturnEmpty As Boolean = False, Optional bDontCloneMembers As Boolean = False, Optional bCopyInstances As Boolean = False)
            ' MyBase.New("")
            Init()
            If aEntities Is Nothing Then Return
            SuppressEvents = True

            If TypeOf aEntities Is colDXFEntities Then
                Dim dxfEnts As colDXFEntities = DirectCast(aEntities, colDXFEntities)
                _MaintainIndices = dxfEnts.MaintainIndices
                _BlockName = dxfEnts.BlockName
                _Suppressed = dxfEnts.Suppressed
                _Invalid = dxfEnts.Invalid
                _Filter = dxfEnts.Filter
                _ErrorString = dxfEnts.ErrorString
            End If
            If bReturnEmpty Then Return

            For Each mem As dxfEntity In aEntities
                If mem Is Nothing Then Continue For
                Dim newmem As dxfEntity = Nothing
                If bDontCloneMembers Then newmem = mem Else newmem = mem.Clone(bCopyInstances)
                If bCopyInstances And Not bDontCloneMembers Then
                    newmem.Instances.Copy(mem.Instances)
                End If
                AddToCollection(newmem)
            Next
            SuppressEvents = False
        End Sub

#End Region 'Constructors

#Region "Properties"
        Friend Property MonitorMembers As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(GUID)
            End Get
            Set(value As Boolean)
                If value Then
                    If String.IsNullOrWhiteSpace(GUID) Then
                        MaintainIndices = True
                        GUID = dxfEvents.NextEntitiesGUID
                        For Each mem As dxfEntity In Me
                            SetMemberInfo(mem)
                        Next
                    End If
                    'If _Events IS Nothing Then _Events = goEvents()
                    'AddHandler _Events.EntititiesRequest, AddressOf _Events_EntitiesRequest
                Else
                    If Not String.IsNullOrWhiteSpace(GUID) Then
                        For Each mem As dxfEntity In Me
                            mem.SetCollection(Nothing, False)
                        Next
                    End If
                    GUID = ""
                    'If _Events IsNOt Nothing Then RemoveHandler _Events.EntititiesRequest, AddressOf _Events_EntitiesRequest
                    '_Events = Nothing
                End If
            End Set
        End Property
        Public Property ErrorString As String
            Get
                Return _ErrorString = ""
            End Get
            Set(value As String)
                _ErrorString = value
            End Set
        End Property

        Friend Property BlockName As String
            Get
                Return _BlockName
            End Get
            Set(value As String)
                _BlockName = value
            End Set
        End Property
        Public Property CollectionObj As List(Of dxfEntity)
            Get
                Return MyBase.ToList()
            End Get
            Set(value As List(Of dxfEntity))
                If value Is Nothing Then Return
                MyBase.Clear()
                MyBase.AddRange(value)
                If MaintainIndices Then
                    ReIndex()
                End If
            End Set
        End Property
        Public Overloads ReadOnly Property Count As Integer
            Get
                If disposedValue Then Return 0
                Return MyBase.Count
            End Get
        End Property
        Public Function DefiningVectors(Optional bGetClones As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                _rVal.Append(aMem.DefiningVectors, bGetClones)
            Next i
            Return _rVal
        End Function
        Private _Filter As List(Of dxxEntityTypes) = Nothing
        Friend Property Filter As List(Of dxxEntityTypes)
            Get
                Return _Filter
            End Get
            Set(value As List(Of dxxEntityTypes))
                _Filter = value
            End Set
        End Property

        Public ReadOnly Property Handlez As Dictionary(Of String, String)
            Get
                Dim _rVal = New Dictionary(Of String, String)
                Dim aMem As dxfEntity
                For i As Integer = 1 To Count
                    aMem = Item(i)
                    _rVal.Add(aMem.GUID, aMem.Handle)
                Next i
                Return _rVal
            End Get
        End Property
        Public Property Invalid As Boolean
            Get
                Return _Invalid
            End Get
            Set(value As Boolean)
                If _Invalid <> value Then
                    Dim bRaise As Boolean
                    bRaise = Not _Invalid
                    _Invalid = value
                    If bRaise Then RaiseEntitiesChange(dxxCollectionEventTypes.Invalidate, Nothing, False, False, False)
                End If
            End Set
        End Property
        Friend ReadOnly Property IsBlock As Boolean
            Get
                Return BlockGUID <> ""
            End Get
        End Property
        Public Property MaintainIndices As Boolean
            Get
                Return _MaintainIndices Or MonitorMembers
            End Get
            Set(value As Boolean)
                If _MaintainIndices <> value Then
                    _MaintainIndices = value
                    If MaintainIndices Then ReIndex()
                End If
            End Set
        End Property


        Friend Property SuppressEvents As Boolean
            Get
                Return _SuppressEvents
            End Get
            Set(value As Boolean)
                _SuppressEvents = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function HandleList(Optional aDelimiter As String = ",") As String

            Dim aMem As dxfEntity
            Dim _rVal As String = String.Empty
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.Handle <> "" Then
                    If _rVal <> "" Then _rVal += aDelimiter
                    _rVal += aMem.Handle
                End If
            Next i
            Return _rVal

        End Function

        Public Overrides Function ToString() As String
            Return "colDXFEntities[" & Count & "]"
        End Function
        Public Function Above(aYValue As Double, Optional bOnisIn As Boolean = True, Optional aPrecis As Integer = 3, Optional aCS As dxfPlane = Nothing, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Return GetByPoints(dxxPointFilters.GreaterThanY, aYValue, bOnisIn, aCS, bReturnClones:=bReturnClones, bRemove:=bRemove, aPrecis:=aPrecis, aEntPointType:=aDefPtType)
        End Function
        Public Overloads Sub AddRange(aMembers As List(Of dxfEntity), Optional bAddClones As Boolean = False)
            If aMembers Is Nothing OrElse aMembers.Count <= 0 Then Return
            If Not bAddClones Then
                MyBase.AddRange(aMembers)
            Else
                For Each ent As dxfEntity In aMembers
                    If ent IsNot Nothing Then MyBase.Add(ent.Clone)
                Next
            End If
            If MaintainIndices Then ReIndex()
        End Sub
        Public Overloads Function Add(aEntity As dxfEntity, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aFactor As Double? = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfEntity
            Return AddToCollection(aEntity, aBeforeIndex, aAfterIndex, aAddClone:=bAddClone, aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aFactor:=aFactor, aInstances:=aInstances, aDisplaySettings:=aDisplaySettings)
        End Function

        Public Function AddShape(aShape As iShape, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aFactor As Double? = Nothing) As dxePolyline

            If aShape Is Nothing Then Return Nothing
            If aShape.Vertices.Count <= 1 Then Return Nothing
            Return DirectCast(AddToCollection(New dxePolyline(aShape, aDisplaySettings:=aDisplaySettings), aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aFactor:=aFactor, aInstances:=aInstances), dxePolyline)
        End Function

        Public Function AddShapes(aShapes As IEnumerable(Of iShape), Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aFactor As Double? = Nothing) As List(Of dxePolyline)
            Dim _rVal As New List(Of dxePolyline)
            If aShapes Is Nothing Then Return _rVal
            For Each shape As iShape In aShapes
                Dim pl As dxePolyline = AddShape(shape, aDisplaySettings, aInstances:=Nothing, aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aFactor:=aFactor)
                If pl IsNot Nothing Then _rVal.Add(pl)
            Next shape

            Return _rVal
        End Function
        Public Function AddLines(aLines As IEnumerable(Of iLine), Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aFactor As Double? = Nothing) As List(Of dxeLine)

            Dim _rVal As New List(Of dxeLine)
            If aLines Is Nothing Then Return _rVal
            For Each line As iLine In aLines
                If line IsNot Nothing Then _rVal.Add(AddLine(line, aDisplaySettings, aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aFactor:=aFactor))
            Next
            Return _rVal
        End Function
        Public Function AddLine(aLine As iLine, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aFactor As Double? = Nothing) As dxeLine

            If aLine Is Nothing Then Return Nothing
            Return DirectCast(AddToCollection(New dxeLine(aLine, aDisplaySettings:=aDisplaySettings), aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aFactor:=aFactor, aInstances:=aInstances), dxeLine)
        End Function
        Public Function AddLine(aStartPtX As Double, aStartPtY As Double, Optional aEndPtX As Double = 0.0, Optional aEndPtY As Double = 0.0, Optional aTag As String = "", Optional aFlag As String = "", Optional aPlane As dxfPlane = Nothing) As dxeLine
            Dim _rVal As New dxeLine
            '#1the X coordinate of the start point of the new line to add
            '#2the Y coordinate of the start point of the new line to add
            '#3the X coordinate of the end point of the new line to add
            '#4the Y coordinate of the end point of the new line to add
            '#5a tag to assign to the new line
            '#6a flag to assign to the new line
            '#7the plane to use. if nothing passed the world XY plane is assumed
            '^adds a new line based on the passed info
            '~a shorthand way to add a segment to the collection without all the code to create one and add it conventionally
            ', Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = String.Empty

            Return AddLine(New dxfVector(aStartPtX, aStartPtY), New dxfVector(aEndPtX, aEndPtY), dxfDisplaySettings.Null(), aTag, aFlag, aPlane)
        End Function

        Public Function AddLine(aStartPtX As Double, aStartPtY As Double, aEndPtX As Double, aEndPtY As Double, aDisplaySettings As dxfDisplaySettings, Optional aTag As String = "", Optional aFlag As String = "", Optional aPlane As dxfPlane = Nothing) As dxeLine

            '#1the X coordinate of the start point of the new line to add
            '#2the Y coordinate of the start point of the new line to add
            '#3the X coordinate of the end point of the new line to add
            '#4the Y coordinate of the end point of the new line to add
            '#5a tag to assign to the new line
            '#6a flag to assign to the new line
            '#7the plane to use. if nothing passed the world XY plane is assumed
            '^adds a new line based on the passed info
            '~a shorthand way to add a segment to the collection without all the code to create one and add it conventionally

            Return AddLine(New dxfVector(aStartPtX, aStartPtY), New dxfVector(aEndPtX, aEndPtY), aDisplaySettings, aTag, aFlag, aPlane)
        End Function

        Public Function AddLine(aStartPt As iVector, aEndPt As iVector, aDisplaySettings As dxfDisplaySettings, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aPlane As dxfPlane = Nothing) As dxeLine

            '#1the new lines startPt
            '#2the new lines end point
            '#3the display settings to apply
            '#4a tag to assign to the new line
            '#5a flag to assign to the new line
            '#7the plane to use. if nothing passed the world XY plane is assumed
            '^adds a new line based on the passed info
            '~a shorthand way to add a segment to the collection without all the code to create one and add it conventionally
            Dim sp As New dxfVector(aStartPt)
            Dim ep As New dxfVector(aEndPt)
            If aTag Is Nothing Then aTag = sp.Tag
            If aFlag Is Nothing Then aFlag = sp.Flag

            Dim plane As New TPLANE("")
            If aPlane IsNot Nothing Then
                plane = New TPLANE(aPlane)
                sp.ProjectTo(plane)
                ep.ProjectTo(plane)
            End If
            Dim _rVal As New dxeLine(sp, ep, aDisplaySettings) With
            {
                .PlaneV = plane,
                .Tag = aTag,
                .Flag = aFlag
            }

            Return Add(_rVal)
        End Function



        Friend Function AddLine(aStartPt As TPOINT, aEndPt As TPOINT) As dxeLine
            Return New dxeLine(CType(aStartPt, dxfVector), CType(aEndPt, dxfVector))
            '#1the start point of the new line to add
            '#2the end point of the new line to add
            '^adds a new line based on the passed info
            '~a shorthand way to add a segment to the collection without all the code to create one and add it conventionally
        End Function

        Public Function AddArc(aX As Double, aY As Double, aRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bClockwise As Boolean = False, Optional aStartWidth As Double = 0.0, Optional aEndWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aZ As Double = 0.0, Optional aTag As String = "") As dxeArc


            Return AddArc(New dxfVector(aX, aY), aRadius, aStartAngle, aEndAngle, bClockwise, aStartWidth, aEndWidth, aDisplaySettings:=aDisplaySettings, aOCS:=aPlane)
        End Function

        Public Function AddArc(aCenter As iVector, aRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockwise As Boolean = False, Optional aStartWidth As Double = 0.0, Optional aEndWidth As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aOCS As dxfPlane = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxeArc
            Dim ctr As New dxfVector(aCenter)
            If aTag Is Nothing Then aTag = ctr.Tag
            If aFlag Is Nothing Then aFlag = ctr.Flag

            Dim _rVal As New dxeArc(ctr, aRadius, aStartAngle, aEndAngle, bClockwise, aOCS, aDisplaySettings) With {
                .SuppressEvents = True,
            .EndWidth = aEndWidth,
            .StartWidth = aStartWidth,
            .Tag = aTag,
            .Flag = aFlag,
            .Value = ctr.Value
            }
            _rVal.SuppressEvents = False
            Return Add(_rVal)
        End Function
        Friend Function AddArcLineV(aSegment As TSEGMENT, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfEntity
            Dim _rVal As dxfEntity
            Dim aE As dxeEllipse
            If aSegment.IsArc Then
                If aSegment.ArcStructure.Elliptical Then
                    aE = New dxeEllipse With {.ArcStructure = aSegment.ArcStructure, .DisplaySettings = aDisplaySettings}
                    _rVal = Add(aE)
                Else
                    _rVal = AddArc(aSegment.ArcStructure)
                End If
            Else
                _rVal = AddLineV2(aSegment.LineStructure)
            End If
            If _rVal IsNot Nothing Then
                If aDisplaySettings IsNot Nothing Then _rVal.DisplaySettings = aDisplaySettings
                _rVal.Identifier = aSegment.Identifier
            End If
            Return _rVal
        End Function
        Public Function AddArcPointToPoint(aStartPoint As iVector, aEndPoint As iVector, aRadius As Double, Optional aTag As String = "", Optional aFlag As String = "", Optional bClockwise As Boolean = False, Optional aLineType As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLayerName As String = "", Optional aPlane As dxfPlane = Nothing) As dxeArc
            Dim _rVal As dxeArc = Nothing
            '#1the start vertex of the new arc segment to add
            '#2the end vertex of the new arc segment to add
            '#3the radius of the new arc segment
            '#4the tag to apply to the new segment
            '#5the flag to apply to the new segment
            '#6flag to add the smaller of the two arcs posible between the two passed points
            '^adds a new arc segment based on the passed info if it is geometrically possible
            '~a shorthand way to add a segment to the collection without all the code to create one and add it conventionally.
            '~an error will be raised by the utilities object if the requested segment can't be defined
            Dim aMem As dxeArc = dxfUtils.ArcBetweenPoints(aRadius, New TVECTOR(aStartPoint), New TVECTOR(aEndPoint), bClockwise, False, aPlane:=aPlane)
            If aMem IsNot Nothing Then
                aMem.Tag = aTag
                aMem.Flag = aFlag
                aMem.LCLSet(aLayerName, aColor, aLineType)
                _rVal = Add(aMem)
            End If
            Return _rVal
        End Function
        Public Function AddArcSegment(aStartPoint As iVector, aEndPoint As iVector, Radius As Double, Optional Tag As String = "", Optional Flag As String = "", Optional ClockWise As Boolean = False, Optional Linetype As String = "") As dxeArc
            Dim _rVal As dxeArc = Nothing
            '#1the start vertex of the new arc segment to add
            '#2the end vertex of the new arc segment to add
            '#3the radius of the new arc segment
            '#4the tag to apply to the new segment
            '#5the flag to apply to the new segment
            '#6flag to add the smaller of the two arcs posible between the two passed points
            '^adds a new arc segment based on the passed info if it is geometrically possible
            '~a shorthand way to add a segment to the collection without all the code to create one and add it conventionally.
            '~an error will be raised by the utilities object if the requested segment can't be defined
            Dim vrt As New TVERTEX(aStartPoint)
            Dim Segment As dxeArc = dxfUtils.ArcBetweenPoints(Radius, New TVECTOR(aStartPoint), New TVECTOR(aEndPoint), ClockWise, False)
            If Segment IsNot Nothing Then
                If Not String.IsNullOrWhiteSpace(Tag) Then Segment.Tag = Tag Else Segment.Tag = vrt.Tag
                If Not String.IsNullOrWhiteSpace(Flag) Then Segment.Flag = Flag Else Segment.Flag = vrt.Flag
                If Not String.IsNullOrWhiteSpace(Linetype) Then Segment.Linetype = Linetype
                _rVal = Add(Segment)
            End If
            Return _rVal
        End Function
        Public Function AddArcThreePoint(StartPointXY As Object, ArcPoint As Object, EndPointXY As Object, Optional aTag As String = "", Optional aFlag As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLayerName As String = "", Optional aPlane As dxfPlane = Nothing) As dxeArc
            Dim _rVal As dxeArc = Nothing
            If StartPointXY Is Nothing Or ArcPoint Is Nothing Or ArcPoint Is Nothing Then Return _rVal
            '#1the start vertex of the new arc entity to add
            '#2a point on the new arc(not the start or end point)
            '#3the end vertex of the new arc entity to add
            '#4the tag to apply to the new entity
            '#5the flag to apply to the new entity
            '#6flag return then clockwise arc
            '#7a color to assign to the arc
            '#8a linetype to assign to the arc
            '^adds a new arc entity based on the passed info if it is geometrically possible
            '~a shorthand way to add a entity to the collection without all the code to create one and add it conventionally.
            '~an error will be raised by the utilities object if the requested entity can't be defined
            Dim plane As New TPLANE(aPlane)
            Dim err As String = String.Empty
            Try

                _rVal = dxfPrimatives.CreateArcThreePoint(StartPointXY, ArcPoint, EndPointXY, False, plane, err)
                If _rVal IsNot Nothing Then
                    _rVal.LCLSet(aLayerName, aColor, aLineType)
                    Add(_rVal, aTag:=aTag, aFlag:=aFlag)
                Else
                    If Not String.IsNullOrWhiteSpace(err) Then Throw New Exception(err)
                End If
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function

        Public Function AddArc(aArc As iArc, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aFactor As Double? = Nothing) As dxeArc

            If aArc Is Nothing Then Return Nothing
            Return DirectCast(AddToCollection(New dxeArc(aArc, aDisplaySettings:=aDisplaySettings), aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aFactor:=aFactor, aInstances:=aInstances), dxeArc)
        End Function
        Friend Function AddArc(aArc As TARC, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeArc
            Dim _rVal As New dxeArc With {.ArcStructure = aArc, .DisplaySettings = aDisplaySettings}
            Return Add(_rVal)
        End Function



        Public Function AddCircles(aCenters As IEnumerable(Of iVector), aRadius As Double, Optional bUseVectorsForRadius As Boolean = False, Optional aCS As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As Integer
            Dim _rVal As Integer = 0
            Try
                If aCenters Is Nothing Then Return _rVal
                Dim ctrs As TVERTICES = dxfVectors.GetTVERTICES(aCenters)
                Dim v1 As New TVERTEX
                Dim rad As Double = Math.Abs(aRadius)
                Dim rad2 As Double
                Dim aCirc As dxeArc
                Dim bCS As dxfPlane
                Dim dSets As New TDISPLAYVARS
                If aCS Is Nothing Then bCS = New dxfPlane Else bCS = aCS
                If rad = 0 Then rad = 1
                If aDisplaySettings Is Nothing Then
                    dSets.Color = dxxColors.ByLayer
                    dSets.LayerName = "0"
                    dSets.Linetype = dxfLinetypes.ByLayer
                    dSets.LTScale = 1
                    dSets.LineWeight = dxxLineWeights.ByLayer
                Else
                    dSets = aDisplaySettings.Strukture
                End If
                For i As Integer = 1 To ctrs.Count
                    v1 = ctrs.Item(i)
                    rad2 = rad
                    If bUseVectorsForRadius Then
                        rad2 = v1.Radius
                        If rad2 <= 0 Then rad2 = rad
                    End If
                    aCirc = New dxeArc With {
                        .Plane = bCS,
                        .DisplayStructure = dSets,
                        .CenterV = v1.Vector,
                        .Radius = rad2
                    }
                    Add(aCirc)
                Next i
            Catch ex As Exception
                Throw New Exception("colDXFEntities.AddCircles - " & ex.Message)
            End Try
            Return _rVal
        End Function
        Public Function AddEllipse(aCenter As iVector, aRadius As Double, aMinorRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockwise As Boolean = False, Optional aRotation As Double = 0.0, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLayerName As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As dxeEllipse
            Dim _rVal As New dxeEllipse
            Dim aPl As New TPLANE(aPlane)
            If aRotation <> 0 Then aPl.Revolve(aRotation, False)
            aPl.Origin = New TVECTOR(aCenter)
            _rVal.PlaneV = aPl
            _rVal.CenterV = aPl.Origin
            If aDisplaySettings IsNot Nothing Then _rVal.DisplayStructure = aDisplaySettings.Strukture
            _rVal.Radius = aRadius
            _rVal.MinorRadius = aMinorRadius
            _rVal.StartAngle = aStartAngle
            _rVal.EndAngle = aEndAngle
            _rVal.ClockWise = bClockwise
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            Return Add(_rVal)
        End Function
        Public Function AddHole(aCenterX As Double, aCenterY As Double, aDiameter As Double, Optional aLength As Double = 0.0, Optional aDepth As Double = 0.0, Optional aIsSquare As Boolean = False, Optional aCenterZ As Double = 0.0, Optional aRotation As Double = 0.0, Optional aStdPlane As dxxStandardPlanes = dxxStandardPlanes.XY, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing) As dxeHole
            Dim _rVal As dxeHole = Nothing
            '#1the center X ordinate of the hole
            '#2the center Y ordinate of the hole
            '#3the height/diameter of the hole
            '#4the length of the hole
            '#5the depth of the hole
            '#6flag indicating if the hole should be round or square
            '#7the center Z ordinate of the hole
            '#8the angle to apply to the hole
            '#9a standard plane to assign to the new hole
            '#10the layer to assign to the new hole
            '#11the color to assign to the new hole
            '#12the linetype to assign to the new hole
            '#13the coordinate system that the passed standard plane applies to
            '^shorthand method for adding a hole to the collection
            If aDiameter <= 0 Then Return _rVal
            Dim bPlane As New TPLANE(aPlane)
            Dim v1 As TVECTOR = bPlane.Vector(aCenterX, aCenterY, aCenterZ)
            If aStdPlane = dxxStandardPlanes.XZ Then
                bPlane = bPlane.StandardPlane(dxxStandardPlanes.XZ, New dxfVector(v1))
            ElseIf aStdPlane = dxxStandardPlanes.YZ Then
                bPlane = bPlane.StandardPlane(dxxStandardPlanes.YZ, New dxfVector(v1))
            End If
            _rVal = New dxeHole
            bPlane.Origin = v1
            _rVal.SuppressEvents = True
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.PlaneV = bPlane
            _rVal.Height = aDiameter
            _rVal.Length = aLength
            _rVal.IsSquare = aIsSquare
            _rVal.CenterV = v1
            _rVal.Rotation = aRotation
            _rVal.Depth = aDepth
            _rVal.SuppressEvents = False
            Return Add(_rVal)
        End Function
        Public Function AddHolesByString(sDescriptors As String, Optional bSimple As Boolean = False) As List(Of dxfEntity)
            '#1the descriptor string of a hole collection to extract the collection member properties from
            '^used to populate the collection based on the values in the passed comma delimated string
            '~see colUOPHoles.Descriptor
            Dim _rVal As New List(Of dxfEntity)
            sDescriptors = Trim(sDescriptors)
            Dim vals() As String

            Dim hstr As String
            Dim nHole As dxeHole
            vals = sDescriptors.Split(dxfGlobals.Delim)
            For i As Integer = 0 To vals.Length - 1
                hstr = vals(i)
                nHole = New dxeHole
                If Not bSimple Then nHole.DefineByString(hstr) Else nHole.SimpleDescriptor = hstr
                nHole = Add(nHole)
                nHole.Index = i + 1
                _rVal.Add(nHole)
            Next i
            Return _rVal
        End Function
        Public Function AddLinePlanar(aStartPtX As Double, aStartPtY As Double, Optional aEndPtX As Double = 0.0, Optional aEndPtY As Double = 0.0, Optional aTag As String = "", Optional aFlag As String = "", Optional aLineType As String = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As dxeLine
            Dim _rVal As dxeLine = dxeLine.PlanarLine(aStartPtX, aStartPtY, aEndPtX, aEndPtY, aTag, aFlag, aLineType, aDisplaySettings, aPlane)
            '#1the X coordinate of the start point of the new line
            '#2the Y coordinate of the start point of the new line
            '#3the X coordinate of the end point of the new line
            '#4the Y coordinate of the end point of the new line
            '#5a tag to assign to the line
            '#6a flag to assign to the line
            '#7an overriding linetype assign to the line
            '#8the display settings for the line
            '#9the coordinate system to use
            '^adds a new line based on the passed info
            '~a shorthand way to add a entity to the collection without all the code to create one and add it conventionally
            _rVal = Add(_rVal)
            Return _rVal
        End Function
        Friend Function AddLineV(aSP As TVECTOR, aEP As TVECTOR, Optional aTag As String = "", Optional aFlag As String = "", Optional aMinLength As Double = -1, Optional aPlane As dxfPlane = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.ByLayer, Optional aLineType As String = "") As dxeLine
            '#1the start vertex of the new entity to add
            '#2the end vertex of the new entity to add
            '^adds a new entity based on the passed info
            '~a shorthand way to add a entity to the collection without all the code to create one and add it conventionally
            'Optional aBeforeIndex As INteger, Optional aAfterIndex As INteger
            Dim _rVal As New dxeLine(aSP, aEP)

            _rVal.TFVSet(aTag, aFlag)
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.Identifier = _rVal.Tag
            _rVal.Plane = aPlane
            If aMinLength <= 0 Then
                Add(_rVal)
            Else
                If dxfProjections.DistanceTo(aSP, aEP) >= aMinLength Then
                    Add(_rVal)
                End If
            End If
            Return _rVal
        End Function
        Friend Function AddLineV2(aLine As TLINE, Optional aTag As String = "", Optional aFlag As String = "", Optional aMinLength As Double = -1, Optional aPlane As dxfPlane = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.ByLayer, Optional aLineType As String = "") As dxeLine
            '#1the start vertex of the new entity to add
            '#2the end vertex of the new entity to add
            '^adds a new entity based on the passed info
            '~a shorthand way to add a entity to the collection without all the code to create one and add it conventionally
            'Optional aBeforeIndex As INteger, Optional aAfterIndex As INteger
            Return AddLineV(aLine.SPT, aLine.EPT, aTag, aFlag, aMinLength, aPlane, aLayerName, aColor, aLineType)
        End Function
        Public Function AddPlanarArc(aPlane As dxfPlane, aRadius As Double, Optional aCPX As Double = 0.0, Optional aCPY As Double = 0.0, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockwise As Boolean = False, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "") As dxeArc
            Dim _rVal As New dxeArc
            If dxfPlane.IsNull(aPlane) Then
                _rVal.MoveTo(New dxfVector(aCPX, aCPY, 0))
            Else
                _rVal.Plane = aPlane
                _rVal.Center = New dxfVector(aPlane, aCPX, aCPY)
            End If
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.TFVSet(aTag, aFlag)
            _rVal.StartAngle = aStartAngle
            _rVal.EndAngle = aEndAngle
            _rVal.Radius = aRadius
            If bClockwise Then _rVal.ClockWise = bClockwise
            Return Add(_rVal)
        End Function
        Public Function AddPlanarLine(aPlane As dxfPlane, aSPX As Double, aSPY As Double, aEPX As Double, aEPY As Double, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "") As dxeLine
            Dim _rVal As New dxeLine
            If dxfPlane.IsNull(aPlane) Then
                _rVal.StartPt.SetCoordinates(aSPX, aSPY)
                _rVal.EndPt.SetCoordinates(aEPX, aEPY)
            Else
                _rVal.Plane = aPlane
                _rVal.StartPt = New dxfVector(aPlane, aSPX, aSPY)
                _rVal.EndPt = New dxfVector(aPlane, aEPX, aEPY)
            End If
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.TFVSet(aTag, aFlag)
            Return Add(_rVal)
        End Function
        Public Function AddPoint(aCenterEnt As dxfEntity, Optional aHandlePt As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxePoint
            If aCenterEnt Is Nothing Then Return Nothing
            If aDisplaySettings Is Nothing Then aDisplaySettings = aCenterEnt.DisplaySettings
            Dim _rVal As New dxePoint With
            {
             .PlaneV = aCenterEnt.PlaneV,
             .Center = aCenterEnt.DefinitionPoint(aHandlePt),
             .DisplaySettings = aDisplaySettings,
             .Tag = aCenterEnt.Tag,
             .Flag = aCenterEnt.Flag,
             .Value = aCenterEnt.Value
            }
            Return Add(_rVal)
        End Function
        Public Function AddPoint(aCenter As iVector, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing) As dxePoint
            Dim _rVal As New dxePoint(aCenter, New dxfDisplaySettings(aLayerName, aColor, aLineType), aPlane)

            _rVal.TFVSet(aTag, aFlag, aValue)
            Return Add(_rVal)
        End Function
        Public Function AddPoint(aX As Double, aY As Double, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing, Optional aZ As Double? = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing) As dxePoint
            Dim _rVal = New dxePoint(aX, aY, 0) With {.Plane = aPlane}
            If aZ.HasValue Then _rVal.Z = aZ.Value
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.TFVSet(aTag, aFlag, aValue)
            Return Add(_rVal)
        End Function

        Public Function AddPolyline(aPolyline As dxfPolyline, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aFactor As Double? = Nothing) As dxePolyline
            If aPolyline Is Nothing Then Return Nothing
            Return AddToCollection(aPolyline, aBeforeIndex, aAfterIndex, aAddClone:=bAddClone, aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aFactor:=aFactor)
        End Function


        Public Function AddPolyline(aVertices As IEnumerable(Of iVector), bClosed As Boolean, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aCS As dxfPlane = Nothing, Optional aTag As String = "", Optional aSegmentWidth As Double = 0.0) As dxePolyline

            Return AddToCollection(New dxePolyline(aVertices, bClosed, aDisplaySettings, aCS, aSegmentWidth), aTag:=aTag)

        End Function



        Public Function AddPolyline(aCoordinates As String, bClosed As Boolean, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸") As dxePolyline
            '^a concantonated string of all the vertex coordinates of the polyline
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)

            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane()
            Dim verts As New colDXFVectors()
            verts.VertexCoordinatesSet(aCoordinates, aPlane, aDelimiter:=aDelimiter)
            Return AddPolyline(verts, bClosed, aDisplaySettings, aPlane, aSegmentWidth:=aSegmentWidth)
        End Function

        Public Function AddRectangle(aCenter As dxfVector, aWidth As Double, aHeight As Double, Optional aFillet As Double = 0.0, Optional aTag As String = "", Optional aFlag As String = "", Optional bReturnAsPolygon As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aCS As dxfPlane = Nothing) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '#1the center of the new rectangular polygon
            '#2the width of the new rectangular polygon
            '#3the height of the new rectangular polygon
            '#4the tag to assign to the new rectangular polygon
            '#5the flag to assign to the new rectangular polygon
            '^short hand method used to add a rectangular polygon to the collection
            If aWidth <= 0 And aHeight <= 0 Then Return _rVal
            _rVal = dxfPrimatives.CreateRectanglularPerimeter(aCenter, bReturnAsPolygon, aHeight, aWidth, aFillet, False, aPlane:=aCS)
            If aDisplaySettings IsNot Nothing Then
                _rVal.SetDisplayProperties(aDisplaySettings.LayerName, aDisplaySettings.Color, aDisplaySettings.Linetype, aDisplaySettings.LTScale)
            End If
            _rVal.Tag = aTag
            _rVal.Flag = aFlag
            Return Add(_rVal)
        End Function
        Public Function AddRectangle(aLeft As Double, aRight As Double, aTop As Double, aBottom As Double, Optional aTag As String = "", Optional aFlag As String = "", Optional aPlane As dxfPlane = Nothing, Optional bReturnAsPolygon As Boolean = False) As dxfPolyline
            Dim _rVal As dxfPolyline = Nothing
            '#1the left edge of the new rectangular polyline
            '#2the right edge of the new rectangular polyline
            '#3the top edge of the new rectangular polyline
            '#4the bottom edge of the new rectangular polyline
            '^short hand method used to add a rectangular polygon or polyline to the collection
            Dim aPl As New TPLANE(aPlane)
            Dim x1 As Double
            Dim x2 As Double
            Dim Y1 As Double
            Dim Y2 As Double
            Dim cp As TVECTOR
            Dim aPg As dxePolygon
            Dim aPe As dxePolyline
            '**UNUSED VAR** Dim aRec As dxfRectangle
            x1 = aLeft
            x2 = aRight
            Y1 = aBottom
            Y2 = aTop
            TVALUES.SortTwoValues(True, x1, x2)
            TVALUES.SortTwoValues(True, Y1, Y2)
            If x2 - x1 <= 0 And Y2 - Y1 <= 0 Then Return _rVal

            cp = aPl.Vector(x1 + (x2 - x1) / 2, Y1 + (Y2 - Y1) / 2)
            aPl.Origin = cp
            If bReturnAsPolygon Then
                aPg = New dxePolygon With {
                    .Closed = True,
                    .PlaneV = aPl,
                    .Tag = aTag,
                    .Flag = aFlag
                }
                aPg.Vertices.AddV(aPl.Vector(-(x2 - x1) / 2, (Y2 - Y1) / 2))
                aPg.Vertices.AddV(aPl.Vector(-(x2 - x1) / 2, -(Y2 - Y1) / 2))
                aPg.Vertices.AddV(aPl.Vector((x2 - x1) / 2, -(Y2 - Y1) / 2))
                aPg.Vertices.AddV(aPl.Vector((x2 - x1) / 2, (Y2 - Y1) / 2))
                _rVal = aPg
            Else
                aPe = New dxePolyline With {
                    .Closed = True,
                    .PlaneV = aPl,
                    .Tag = aTag,
                    .Flag = aFlag
                }
                aPe.Vertices.AddV(aPl.Vector(-(x2 - x1) / 2, (Y2 - Y1) / 2))
                aPe.Vertices.AddV(aPl.Vector(-(x2 - x1) / 2, -(Y2 - Y1) / 2))
                aPe.Vertices.AddV(aPl.Vector((x2 - x1) / 2, -(Y2 - Y1) / 2))
                aPe.Vertices.AddV(aPl.Vector((x2 - x1) / 2, (Y2 - Y1) / 2))
                _rVal = aPe
            End If
            Return Add(_rVal)
        End Function

        Public Function AddSolid(aCoordinates As String, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸") As dxeSolid
            Dim _rVal As New dxeSolid() With {.PlaneV = New TPLANE(aPlane), .DisplaySettings = aDisplaySettings}
            '^a concantonated string of all the vertex coordinates of the solid
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)
            _rVal.VertexCoordinatesSet(aCoordinates, _rVal.Plane, aDelimiter)
            Return Add(_rVal)
        End Function

        Public Function AddSolid(aVertex1 As iVector, aVertex2 As iVector, aVertex3 As iVector, Optional aVertex4 As iVector = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸") As dxeSolid
            '^adds a solid to the collection
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane
            If aVertex1 Is Nothing AndAlso aVertex2 Is Nothing AndAlso aVertex3 Is Nothing Then Return Nothing
            Dim _rVal As New dxeSolid(aVertex1, aVertex2, aVertex3, aVertex4, aPlane, aDisplaySettings)
            _rVal = Add(_rVal)
            Return _rVal
        End Function
        Public Function AddTrace(aVertex1 As iVector, aVertex2 As iVector, aVertex3 As iVector, aVertex4 As iVector, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸") As dxeSolid
            If aVertex1 Is Nothing AndAlso aVertex2 Is Nothing AndAlso aVertex3 Is Nothing AndAlso aVertex4 Is Nothing Then Return Nothing
            '^adds a solid as a trace to the collection
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane
            Dim _rVal As New dxeSolid(aVertex1, aVertex2, aVertex3, aVertex4, aPlane, aDisplaySettings) With {.IsTrace = True}
            _rVal = Add(_rVal)
            Return _rVal
        End Function
        Friend Function AddToCollection(aEntity As dxfEntity, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bSuppressReindex As Boolean = False, Optional bSuppressEvnts As Boolean = False, Optional aAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional bDontSave As Boolean = False, Optional aFactor As Double? = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfEntity
            If aEntity Is Nothing Then Return Nothing
            Dim _rVal As dxfEntity = Nothing
            Dim aImage As dxfImage = Nothing
            Dim bBail As Boolean
            Dim cnt As Integer = Count
            Dim idx As Integer
            Try
                If _Filter IsNot Nothing Then
                    If _Filter.FindIndex(Function(x) x = aEntity.GraphicType) < 0 Then
                        Return Nothing
                    End If
                End If
                Dim newEnt As dxfEntity = aEntity
                If GetByGUID(newEnt.GUID) IsNot Nothing Then aAddClone = True
                Dim listnenerWantsClone As Boolean
                RaiseEntitiesChange(dxxCollectionEventTypes.PreAdd, newEnt, bBail, listnenerWantsClone, False, aImage, bSuppressEvnts)
                If bBail Then Return _rVal
                If listnenerWantsClone Then aAddClone = True
                If aAddClone Then newEnt = aEntity.Clone
                If aTag IsNot Nothing Then newEnt.Tag = aTag
                If aFlag IsNot Nothing Then newEnt.Flag = aFlag
                If aValue IsNot Nothing Then
                    If aValue.HasValue Then newEnt.Value = aValue.Value
                End If
                If aFactor IsNot Nothing Then
                    If aFactor.HasValue Then newEnt.Factor = aFactor.Value
                End If
                If aDisplaySettings IsNot Nothing Then
                    newEnt.DisplaySettings = aDisplaySettings
                End If
                If aInstances IsNot Nothing Then
                    newEnt.Instances = aInstances
                End If

                If Not bDontSave Then
                    If cnt = 0 Then
                        aBeforeIndex = 0
                        aAfterIndex = 0
                        idx = 1
                    Else
                        If aBeforeIndex < 1 Then aBeforeIndex = 0
                        If aAfterIndex < 1 Then aAfterIndex = 0
                        If aBeforeIndex > 0 Then
                            aAfterIndex = 0
                            If aBeforeIndex >= cnt Then aBeforeIndex = 0
                            If aBeforeIndex > 0 Then idx = aBeforeIndex - 1
                        ElseIf aAfterIndex > 0 Then
                            aBeforeIndex = 0
                            If aAfterIndex >= cnt Then aAfterIndex = 0
                            If aAfterIndex > 0 Then idx = aAfterIndex + 1
                        End If
                    End If
                    If aBeforeIndex = 0 And aAfterIndex = 0 Then
                        idx = cnt + 1
                        If MaintainIndices Then newEnt.Index = idx
                        MyBase.Add(newEnt)
                    Else
                        If aBeforeIndex <> 0 Then
                            MyBase.Insert(idx - 1, newEnt)
                        Else
                            MyBase.Insert(idx - 1, newEnt)
                        End If
                    End If
                End If
                idx = MyBase.IndexOf(newEnt) + 1
                If MaintainIndices Then newEnt.Index = idx
                _rVal = Item(idx)
                'Application.DoEvents()
                RaiseEntitiesChange(dxxCollectionEventTypes.Add, newEnt, bBail, listnenerWantsClone, False, aImage, bSuppressEvnts)
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
            Return _rVal
        End Function
        'Public Function Append(aList As List(Of dxeDimension), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxeDimension In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxeLine), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxeLine In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxeLeader), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxeLeader In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxeArc), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxeArc In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxePolyline), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxePolyline In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxePolygon), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxePolygon In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxeSolid), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxeSolid In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxeText), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxeText In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxeSymbol), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    For Each ent As dxeSymbol In aList
        '        _rVal.Add(AddToCollection(ent, aAddClone:=bAddClones))
        '    Next
        '    Return _rVal
        'End Function
        'Public Function Append(aList As List(Of dxfEntity), Optional bAddClones As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)
        '    If aList Is Nothing Then Return _rVal
        '    If aList.Count <= 0 Then Return _rVal
        '    _rVal = Append(New colDXFEntities(aList), bAddClones:=bAddClones)
        '    Return _rVal
        'End Function
        'Friend Function Append(aLines As TLINES, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressEvnts As Boolean = False) As List(Of dxfEntity)
        '    Dim _rVal As New List(Of dxfEntity)

        '    If aLines.Count <= 0 Then Return _rVal
        '    For i As Integer = 1 To aLines.Count
        '        _rVal.Add(AddToCollection(New dxeLine(aLines.Item(i), aDisplaySettings:=aDisplaySettings), bSuppressEvnts:=bSuppressEvnts))
        '    Next i
        '    Return _rVal
        'End Function


        Public Function Append(aEntities As IEnumerable(Of dxfEntity), Optional bAddClones As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional bReverseOrder As Boolean = False, Optional aValue As Double? = Nothing, Optional bCloneInstances As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aEntities Is Nothing Then Return _rVal
            If aEntities.Count <= 0 Then Return _rVal


            Dim si As Integer
            Dim ei As Integer
            Dim astp As Integer
            Dim bSA As Boolean
            bSA = ImageGUID <> "" Or BlockGUID <> ""
            If BlockGUID <> "" Then
                bAddClones = True
            End If
            If dxfUtils.LoopIndices(aEntities.Count, 1, aEntities.Count, si, ei, bReverseOrder, astp) Then
                For i As Integer = si To ei Step astp
                    Dim aEntity As dxfEntity = aEntities(i - 1)
                    Dim aEnt As dxfEntity = Nothing
                    If bAddClones Then
                        aEnt = aEntity.Clone(bCloneInstances)
                    Else
                        aEnt = aEntity
                    End If
                    aEnt = AddToCollection(aEnt, bSuppressEvnts:=Not bSA, aTag:=aTag, aFlag:=aFlag, aValue:=aValue)
                    If aEnt IsNot Nothing Then
                        _rVal.Add(aEnt)
                    End If
                Next i
            End If

            If _rVal.Count > 0 And Not SuppressEvents And Not bSA Then
                RaiseEntitiesChange(dxxCollectionEventTypes.Append, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function

        Public Function AppendMirrors(aEntities As IEnumerable(Of dxfEntity), aMirrorLine As dxeLine, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional bReverseOrder As Boolean = False, Optional aValue As Double? = Nothing, Optional bAddClones As Boolean = True) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aEntities Is Nothing Then Return _rVal
            If aEntities.Count <= 0 Then Return _rVal
            Dim aEntity As dxfEntity
            Dim aEnt As dxfEntity
            Dim si As Integer
            Dim ei As Integer
            Dim astp As Integer
            Dim bSA As Boolean
            bSA = ImageGUID <> "" Or BlockGUID <> ""
            Dim aTr As TTRANSFORM = TTRANSFORM.CreateMirror(aMirrorLine, bAddClones)
            If aTr.TransformType = dxxTransformationTypes.Undefined Then Return _rVal
            If BlockGUID <> "" Then bAddClones = True
            If dxfUtils.LoopIndices(aEntities.Count, 1, aEntities.Count, si, ei, bReverseOrder, astp) Then
                For i As Integer = si To ei Step astp
                    aEntity = aEntities(i - 1)
                    If bAddClones Then
                        aEnt = aEntity.Clone
                    Else
                        aEnt = aEntity
                    End If
                    TTRANSFORM.Apply(aTr, aEnt, bSuppressEvnts:=bAddClones)
                    aEnt = AddToCollection(aEnt, bSuppressEvnts:=Not bSA, aTag:=aTag, aFlag:=aFlag, aValue:=aValue)
                    If aEnt IsNot Nothing Then _rVal.Add(aEnt)
                Next i

            End If
            If _rVal.Count > 0 And Not SuppressEvents And Not bSA Then
                RaiseEntitiesChange(dxxCollectionEventTypes.Append, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Friend Function ArcLineStructures_Get() As TSEGMENTS


            Dim _rVal As New TSEGMENTS(0)
            For Each ent As dxfEntity In Me
                If ent.GraphicType = dxxGraphicTypes.Arc Or ent.GraphicType = dxxGraphicTypes.Ellipse Or ent.GraphicType = dxxGraphicTypes.Line Then
                    _rVal.Add(ent.ArcLineStructure)
                End If
            Next
            Return _rVal

        End Function
        Friend Sub ArcLineStructures_Set(aSegments As TSEGMENTS)

            Clear()

            For i As Integer = 1 To aSegments.Count
                Dim aSeg As TSEGMENT = aSegments.Item(i)
                Add(CType(aSeg, dxfEntity))
            Next i

        End Sub

        Public Function ArcItem(aIndex As Integer, Optional bIncludeEllipses As Boolean = False, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxeArc
            Dim _rVal As dxeArc = Nothing
            Dim cnt As Integer
            For i As Integer = 1 To Count
                Dim aEnt = Item(i)
                If (aEnt.GraphicType = dxxGraphicTypes.Arc) Or (bIncludeEllipses And aEnt.GraphicType = dxxGraphicTypes.Ellipse) Then
                    cnt += 1
                    If cnt = aIndex Then
                        _rVal = aEnt
                        Exit For
                    End If
                End If
            Next i
            If _rVal IsNot Nothing Then
                If bReturnClone Then
                    _rVal = _rVal.Clone
                Else
                    If bRemove Then RemoveMember(_rVal)
                End If
            End If
            Return _rVal
        End Function
        Public Function GetArcs(Optional aRadius As Double = 0.0, Optional aPrecis As Integer = 4, Optional bIncludeEllipses As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            '#1a radius to filter for
            '#2the precision to apply to the radius test (if any)
            '#3flag to return ellipses as well as arc entities
            '#4flag to return copies
            '#5flag to remove the returned set frm the current collection
            '^returns all the arc entities in this collection
            Dim _rVal As New colDXFEntities(Me, True, True)

            Dim aArc As dxfArc
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            aRadius = Math.Round(Math.Abs(aRadius), aPrecis)
            For i As Integer = 1 To Count
                Dim aMem = Item(i)
                If (aMem.GraphicType = dxxGraphicTypes.Arc) Or (bIncludeEllipses And aMem.GraphicType = dxxGraphicTypes.Ellipse) Then
                    aArc = aMem
                    If aRadius > 0 Then
                        If Math.Round(aArc.Radius, aPrecis) = aRadius Then _rVal.Add(aMem, bAddClone:=bReturnClones)
                    Else
                        _rVal.Add(aMem, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Public Function ArcsAndLines(Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bIncludePolylines As Boolean = False, Optional bIncludeTextBoxes As Boolean = False, Optional bIncludeBeziers As Boolean = False, Optional aBezierSegments As Integer = 20, Optional bIncludeInvisibles As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)

            For i As Integer = 1 To Count
                Dim aEnt = Item(i)
                If bIncludeInvisibles Or (Not bIncludeInvisibles And Not aEnt.Suppressed And String.Compare(aEnt.Linetype, dxfLinetypes.Invisible, ignoreCase:=True) <> 0) Then

                    Select Case aEnt.GraphicType
                        Case dxxGraphicTypes.Arc, dxxGraphicTypes.Line
                            dxfEntities.AddEntity(_rVal, aEnt, bReturnClones)
                        Case dxxGraphicTypes.Polygon, dxxGraphicTypes.Polyline
                            If bIncludePolylines Then
                                If aEnt.GraphicType = dxxGraphicTypes.Polyline Or aEnt.GraphicType = dxxGraphicTypes.Polygon Then
                                    Dim aPl As dxfPolyline = aEnt
                                    aPl.UpdatePath()
                                    Dim segs As colDXFEntities = aPl.Segments
                                    For Each seg As dxfEntity In segs
                                        dxfEntities.AddEntity(_rVal, seg)
                                    Next


                                End If
                            End If
                        Case dxxGraphicTypes.Bezier
                            If bIncludeBeziers Then
                                Dim aBz As dxeBezier = aEnt
                                Dim segs As colDXFEntities = aBz.LineSegments(aBezierSegments)
                                For Each seg As dxfEntity In segs
                                    dxfEntities.AddEntity(_rVal, seg)
                                Next

                            End If
                        Case dxxGraphicTypes.MText, dxxGraphicTypes.Text
                            If bIncludeTextBoxes Then
                                Dim segs As colDXFEntities = aEnt.BoundingRectangle().BorderLines()
                                For Each seg As dxfEntity In segs
                                    dxfEntities.AddEntity(_rVal, seg)
                                Next

                            End If
                    End Select

                End If

            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Public Sub AssignRowsAndColumns(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.Center, Optional aPlane As dxfPlane = Nothing, Optional bBottomToTop As Boolean = False, Optional bRightToLeft As Boolean = False, Optional aPrecis As Integer = 4)
            Dim rYOrds As List(Of Double) = Nothing
            Dim rXOrds As List(Of Double) = Nothing
            Dim rRowCount As Integer = 0
            Dim rColCount As Integer = 0
            AssignRowsAndColumns(aEntityType, aDefPtType, aPlane, bBottomToTop, bRightToLeft, aPrecis, rYOrds, rXOrds, rRowCount, rColCount)
        End Sub
        Public Sub AssignRowsAndColumns(aEntityType As dxxEntityTypes, aDefPtType As dxxEntDefPointTypes, aPlane As dxfPlane, bBottomToTop As Boolean, bRightToLeft As Boolean, aPrecis As Integer, ByRef rYOrds As List(Of Double), ByRef rXOrds As List(Of Double), ByRef rRowCount As Integer, ByRef rColCount As Integer)
            Dim P1 As dxfVector
            Dim v1 As TVECTOR
            Dim j As Integer
            Dim bPln As Boolean
            Dim aPl As New TPLANE(aPlane, bPln)
            Dim aMem As dxfEntity
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If aDefPtType < dxxEntDefPointTypes.StartPt Or aDefPtType > dxxEntDefPointTypes.EndPt Then aDefPtType = dxxEntDefPointTypes.HandlePt
            rYOrds = Ordinates(dxxOrdinateDescriptors.Y, aPrecis, aEntityType, aDefPtType, aPlane)
            rXOrds = Ordinates(dxxOrdinateDescriptors.X, aPrecis, aEntityType, aDefPtType, aPlane)
            rYOrds.Sort()
            If bBottomToTop Then rYOrds.Reverse()
            rXOrds.Sort()
            If bRightToLeft Then rXOrds.Reverse()
            rRowCount = rYOrds.Count
            rColCount = rXOrds.Count
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.EntityType = aEntityType Or aEntityType = dxxEntityTypes.Undefined Then
                    P1 = aMem.DefinitionPoint(aDefPtType)
                    If P1 Is Nothing Then P1 = New dxfVector
                    v1 = P1.Strukture
                    If bPln Then v1 = v1.WithRespectTo(aPl)
                    v1.X = Math.Round(v1.X, aPrecis)
                    v1.Y = Math.Round(v1.Y, aPrecis)
                    aMem.Row = 0
                    aMem.Col = 0
                    For j = 1 To rYOrds.Count
                        If v1.Y = rYOrds.Item(j - 1) Then
                            aMem.Row = j
                            Exit For
                        End If
                    Next j
                    For j = 1 To rXOrds.Count
                        If v1.X = rXOrds.Item(j - 1) Then
                            aMem.Col = j
                            Exit For
                        End If
                    Next j
                End If
            Next i
        End Sub


        Public Function ContainsEntityType(aEntityType As dxxEntityTypes) As Boolean
            If Count <= 0 Then Return False
            Return FindIndex(Function(x) x.EntityType = aEntityType) >= 0
        End Function

        Public Function Below(aYValue As Double, Optional bOnisIn As Boolean = True, Optional aPrecis As Integer = 3, Optional aCS As dxfPlane = Nothing, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Return GetByPoints(dxxPointFilters.LessThanY, aYValue, bOnisIn, aCS, bReturnClones:=bReturnClones, bRemove:=bRemove, aPrecis:=aPrecis, aEntPointType:=aDefPtType)
        End Function
        Public Function BottomMost(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPrecis As Integer = 3, Optional bReturnClone As Boolean = False, Optional aCS As dxfPlane = Nothing) As dxfEntity
            Return GetByPoint(dxxPointFilters.AtMinY, aPlane:=aCS, aPrecis:=aPrecis, bReturnClone:=bReturnClone, bRemove:=False, aEntPointType:=aDefPtType, aEntityType:=aEntityType)
        End Function

        ''' <summary>
        ''' returns the bounding rectangle of the entities on the passed plane
        ''' </summary>
        ''' <remarks>the bounding rectangle encloses all the defined Extent Points of each entity projected to te subject plane</remarks>
        ''' <param name="aPlane">the plane to define the rectangle on. if null, the global XY plane is assumed.</param>
        ''' <param name="bIncludeSuppressed">flag to include the extent points of the entites that are marked as suppressed </param>
        ''' <param name="bSuppressInstances">flag to exclude the extent points of the entites instances if any are defined</param>
        ''' <param name="aWidthAdder">a fixed distance to add to the returned rectangles width</param>
        ''' <param name="aHeightAdder">a fixed distance to add to the returned rectangles height</param>
        ''' <param name="aScaleFactor">a factor to scale the size of the returned rectangle</param>
        ''' <returns></returns>

        Public Function BoundingRectangle(Optional aPlane As dxfPlane = Nothing, Optional bIncludeSuppressed As Boolean = False, Optional bSuppressInstances As Boolean = False, Optional aWidthAdder As Double = 0, Optional aHeightAdder As Double = 0, Optional aScaleFactor As Double? = Nothing) As dxfRectangle

            Return dxfEntities.BoundingRectangle(Me, aPlane, bIncludeSuppressed, bSuppressInstances, aWidthAdder, aHeightAdder, aScaleFactor)

        End Function
        ''' <summary>
        ''' returns the bounding rectangle of the entities on the passed plane
        ''' </summary>
        ''' <remarks>the bounding rectangle encloses all the defined Extent Points of each entity projected to te subject plane</remarks>
        ''' <param name="aPlane">the plane to define the rectangle on. if null, the global XY plane is assumed.</param>
        ''' <returns></returns>
        Friend Function BoundingRectangleV(aPlane As TPLANE) As TPLANE
            Return ExtentPointsV.Bounds(aPlane)
        End Function

        Friend Function BoundingRectangles(Optional aPlane As dxfPlane = Nothing) As List(Of dxfRectangle)
            Dim _rVal As New List(Of dxfRectangle)
            For i As Integer = 1 To Count
                Dim aEnt As dxfEntity = Item(i)
                Dim aRec As dxfRectangle = aEnt.BoundingRectangle(aPlane)
                If aRec IsNot Nothing Then _rVal.Add(aRec)
            Next i
            Return _rVal
        End Function

        Public Function Centers(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnClones As Boolean = False, Optional aSearchTag As String = Nothing, Optional aSuppressedVal As Boolean? = Nothing) As colDXFVectors
            '#1a entity type filter
            '#2flag to return copies of the points
            '#3a tag to apply to the search
            '^returns the center points of all the entities in the collection
            Return DefinitionPoints(dxxEntDefPointTypes.Center, aEntityType, aSearchTag, bReturnClones:=bReturnClones, aSuppressedVal:=aSuppressedVal)
        End Function
        Public Function Properties(aName As String, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aOccur As Integer = 1, Optional bGetClones As Boolean = True) As dxoProperties
            'On Error Resume Next
            Dim _rVal As New dxoProperties
            For Each aMem As dxfEntity In Me

                If aGraphicType = aMem.GraphicType Or aGraphicType = dxxGraphicTypes.Undefined Then
                    If aEntityType = aMem.EntityType Or aEntityType = dxxEntityTypes.Undefined Then
                        Dim aProps As dxoProperties = aMem.ActiveProperties()
                        If aProps IsNot Nothing Then
                            Dim aProp As dxoProperty = Nothing
                            If aProps.TryGet(aName, aProp, aOccur) Then
                                If bGetClones Then _rVal.Add(New dxoProperty(aProp)) Else _rVal.Add(aProp)
                            End If
                        End If
                    End If

                End If
            Next
            Return _rVal

        End Function

        Public Function Properties(aGroupCode As Integer, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aOccur As Integer = 1, Optional bGetClones As Boolean = True) As dxoProperties
            'On Error Resume Next
            Dim _rVal As New dxoProperties



            For Each aMem As dxfEntity In Me

                If aGraphicType = aMem.GraphicType Or aGraphicType = dxxGraphicTypes.Undefined Then
                    If aEntityType = aMem.EntityType Or aEntityType = dxxEntityTypes.Undefined Then
                        Dim aProps As dxoProperties = aMem.ActiveProperties()
                        If aProps IsNot Nothing Then
                            Dim aProp As dxoProperty = Nothing
                            If aProps.TryGet(aGroupCode, aProp, aOccur) Then
                                If bGetClones Then _rVal.Add(New dxoProperty(aProp)) Else _rVal.Add(aProp)
                            End If
                        End If
                    End If

                End If
            Next
            Return _rVal
        End Function

        Friend Overloads Sub Clear(bDestroy As Boolean, aImage As dxfImage, Optional bReleaseHandles As Boolean = True)
            If Not bDestroy Then
                Clear(bSuppressEvnts:=True)
                Return
            End If
            Dim bhndls As Boolean = aImage IsNot Nothing And Count > 0
            Dim HG As dxoHandleGenerator = Nothing
            If bhndls And bReleaseHandles Then
                HG = aImage.HandleGenerator
                bhndls = HG IsNot Nothing
            End If
            Try

                If bhndls Then
                    For Each ent As dxfEntity In Me
                        HG.ReleaseHandle(ent.Handle)
                        ent.ReleaseReferences()
                    Next
                End If
                ReleaseReferences()
                Clear()

                disposedValue = True
            Catch ex As Exception
            End Try
        End Sub
        Public Overloads Sub Clear()
            Clear(False)
        End Sub
        Friend Overloads Sub Clear(bSuppressEvnts As Boolean)
            If Count <= 0 Then Return
            If Not SuppressEvents And Not bSuppressEvnts Then 'And cnt <> 0 Then
                Dim bCanc As Boolean
                RaiseEntitiesChange(dxxCollectionEventTypes.Clear, MyBase.ToList(), bCanc, bCountChange:=True)
                If bCanc Then Return
            End If
            If MonitorMembers Then
                For Each ent As dxfEntity In Me
                    ent.ReleaseCollectionReference()
                    ent.ImageGUID = ""
                Next
            End If
            MyBase.Clear()
        End Sub
        Public Function Clone(Optional bReturnEmpty As Boolean = False, Optional bCopyInstances As Boolean = False) As colDXFEntities
            Return New colDXFEntities(Me, bReturnEmpty, False, bCopyInstances)
            '^returns a new object with properties matching those of the cloned object

        End Function
        Friend Function CloneAll(Optional aImage As dxfImage = Nothing, Optional bSuppressGUIDsUpdate As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities(Me, True)
            '^returns a new object with properties matching those of the cloned object

            For Each aEntity As dxfEntity In Me
                Dim bEntity As dxfEntity = aEntity.CloneAll(aImage, bSuppressGUIDsUpdate)
                _rVal.AddToCollection(bEntity, bSuppressEvnts:=True)
            Next


            Return _rVal
        End Function


        Public Function Copy(Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aTag As String = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1a layer to assign to the copies
            '#2a color to assign to the copies
            '#3a linetype to assign to the copies
            '#4entity type filter
            '#5tag filter
            '^returns a new object with the members assigned the passed display settings

            Dim bTestTag As Boolean

            Dim bAddit As Boolean
            bTestTag = aTag IsNot Nothing

            _rVal.MaintainIndices = _MaintainIndices
            For i As Integer = 1 To Count
                Dim aMem As dxfEntity = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or aMem.EntityType = aEntityType Then
                    bAddit = True
                    If bTestTag Then
                        If String.Compare(aMem.Tag, aTag, ignoreCase:=True) <> 0 Then bAddit = False
                    End If
                    If bAddit Then
                        aMem = aMem.Clone
                        aMem.LCLSet(aLayerName, aColor, aLineType)
                        _rVal.Add(aMem)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function CopyDisplayValues(aEntitySet As dxfDisplaySettings, Optional aLTScale As Double = 0.0, Optional rChangedEnts As colDXFEntities = Nothing) As Boolean
            Dim _rVal As Boolean = False
            Dim aEnt As dxfEntity
            Dim bFlag As Boolean
            For i As Integer = 1 To Count
                aEnt = Item(i)
                bFlag = False
                If aEnt.CopyDisplayValues(aEntitySet) Then bFlag = True
                If aLTScale > 0 Then
                    If aEnt.LTScale <> aLTScale Then
                        bFlag = True
                        aEnt.LTScale = aLTScale
                    End If
                End If
                If bFlag Then
                    _rVal = True
                    If rChangedEnts IsNot Nothing Then rChangedEnts.Add(aEnt)
                End If
            Next i
            Return _rVal
        End Function
        Public Sub CopyEntitySettings(aEnt As dxfEntity)
            If aEnt Is Nothing Then Return
            'On Error Resume Next
            '^applies the basic entity properties of the passed entity to all the members
            Dim bEnt As dxfEntity
            For i As Integer = 1 To Count
                bEnt = Item(i)
                bEnt.CopyDisplayProps(aEnt)
            Next i
        End Sub
        Public Function CopyMembers(aEntities As IEnumerable(Of dxfEntity), Optional aLayerName As String = Nothing, Optional aColor As dxxColors? = Nothing, Optional aLineType As String = Nothing, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional bReverseOrder As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}
            '#1a collection containting idxfEntities
            '#2an optional layer name to assign to the new members
            '#3an optional color to assign to the new members
            '#4an optional linetype to assign to the new members
            '#5an optional tag to assign to the new members
            '#6an optional flag to assign to the new members
            '#7an optional value to assign to the new members
            '#8flag to add the new members in the reverse order of the passed collection
            '^adds copies of the passed entities to this collection
            If aEntities Is Nothing Then Return _rVal
            If aEntities.Count <= 0 Then Return _rVal


            Dim aEnt As dxfEntity
            Dim si As Integer
            Dim ei As Integer
            Dim astp As Integer
            Dim lname As String = String.Empty
            Dim clr As dxxColors
            Dim lType As String = String.Empty
            Dim bSA As Boolean
            bSA = ImageGUID <> "" Or BlockGUID <> ""
            If Not String.IsNullOrWhiteSpace(aLayerName) Then
                lname = aLayerName.Trim()
            End If
            clr = dxxColors.Undefined
            If aColor.HasValue Then
                clr = aColor.Value
            End If
            If Not String.IsNullOrWhiteSpace(aLineType) Then
                lType = aLineType.Trim()
            End If
            If Not dxfUtils.LoopIndices(aEntities.Count, 1, aEntities.Count, si, ei, bReverseOrder, astp) Then Return _rVal

            For i As Integer = si To ei Step astp
                Dim aEntity As dxfEntity = aEntities(i)
                If aEntity Is Nothing Then Continue For
                aEnt = aEntity.Clone
                If lname <> "" Then aEnt.LayerName = lname
                If lType <> "" Then aEnt.Linetype = lType
                If clr <> dxxColors.Undefined Then aEnt.Color = clr
                aEnt = AddToCollection(aEnt, bSuppressEvnts:=Not bSA, aAddClone:=False, aTag:=aTag, aFlag:=aFlag, aValue:=aValue)
                If aEnt IsNot Nothing Then _rVal.AddToCollection(aEnt, bSuppressEvnts:=True)

            Next i

            If _rVal.Count > 0 And Not SuppressEvents And Not bSA Then
                RaiseEntitiesChange(dxxCollectionEventTypes.Append, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Public Function DefinitionPoints(aPointType As dxxEntDefPointTypes, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aSearchTag As String = Nothing, Optional aSearchFlag As String = Nothing, Optional bReturnClones As Boolean = False, Optional aSuppressedVal As Boolean? = Nothing, Optional aSearchCol As List(Of dxfEntity) = Nothing, Optional aPlane As dxfPlane = Nothing) As colDXFVectors


            Dim srch As List(Of dxfEntity) = Me
            If aSearchCol IsNot Nothing Then srch = aSearchCol

            Dim eppts As List(Of dxfVector) = dxfEntities.GetDefinitionPoints(srch, aPointType, aEntityType, aSearchTag, aSearchFlag, bReturnClones, aSuppressedVal, aPlane)


            Return New colDXFVectors(eppts, bAddClones:=False) With {.MaintainIndices = False}


        End Function
        Public Function Descriptors(Optional aDelimitor As String = "¸") As String
            Dim _rVal As String = String.Empty
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If _rVal <> "" Then _rVal += aDelimitor
                _rVal += aEnt.Descriptor
            Next i
            Return _rVal
        End Function

        Friend Function EnclosesVector(aVector As dxfVector, Optional aFudgeFactor As Double = 0.001, Optional rIsonBound As Boolean? = Nothing, Optional rIsEndPoint As Boolean? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional bOnBoundIsIn As Boolean = True, Optional bUsedClosedSegments As Boolean = True) As Boolean
            '#1the vector to test
            '#2a fudge factor to apply
            '#3returns true if the passed vector is the start vector of the a member of the collection
            '#4returns true if the passed vector is the end vector of the a member of the collection
            '#5the plane to use
            '#6flag to treat a vector on the bounday as within the boundary
            '#7flag to treat gaps in the segments as lines
            '^returns true if the passed vector is enclosed by the passed collection of entities
            '~all entities are assumed to lie on the working plane
            If rIsonBound IsNot Nothing Then rIsonBound = False
            If rIsEndPoint IsNot Nothing Then rIsEndPoint = False
            If aVector Is Nothing Then Return False
            Return dxfEntities.EncloseVector(Me, aVector.Strukture, aFudgeFactor, rIsonBound, rIsEndPoint, aPlane, bOnBoundIsIn, bUsedClosedSegments)
        End Function

        Public Function EndPoints(Optional aSearchTag As String = Nothing, Optional bIncludeStartPts As Boolean = True, Optional bReturnClones As Boolean = False, Optional bUnSuppressOnly As Boolean = False, Optional aEntityType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As colDXFVectors
            Dim _rVal As New colDXFVectors
            Dim aEnt As dxfEntity
            Dim bTest As Boolean = aSearchTag IsNot Nothing
            Dim aStr As String = String.Empty
            Dim bKeep As Boolean
            Dim v1 As dxfVector
            If bTest Then aStr = aSearchTag
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxGraphicTypes.Undefined Or (aEntityType <> dxxGraphicTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    bKeep = True
                    If bTest Then
                        If String.Compare(aEnt.Tag, aStr, ignoreCase:=True) <> 0 Then bKeep = False
                    End If
                    If Not bUnSuppressOnly Then
                        If aEnt.Suppressed Then bKeep = False
                    End If
                    If bKeep Then
                        v1 = aEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt)
                        _rVal.Add(v1, bAddClone:=bReturnClones)
                        If bIncludeStartPts Then
                            v1 = aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt)
                            _rVal.Add(v1, bAddClone:=bReturnClones)
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function EndPt() As dxfVector
            If Count > 0 Then Return LastMember.DefinitionPoint(dxxEntDefPointTypes.EndPt)
            Return Nothing
        End Function
        Public Function EndPts(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnClones As Boolean = False, Optional aSearchTag As String = Nothing, Optional aSuppressedVal As Boolean? = Nothing) As colDXFVectors
            '#1a entity type filter
            '#2flag to return copies of the points
            '#3a tag to apply to the search
            '#4a suppression flag to match
            '^returns the end points of all the entities in the collection
            Return DefinitionPoints(dxxEntDefPointTypes.EndPt, aEntityType, aSearchTag, bReturnClones, aSuppressedVal)
        End Function
        Public Function Entity(aEntityType As dxxEntityTypes, aIndex As Integer, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '#1the entity type to search for
            '#2the position of the desired entity with respect to it's entity type in the collection.
            '#3returns the collection index of the retirn entity
            '#4flag to return a copy of the desired member
            '#5flag to return remove the desired member
            '^retrieves and returns the entity in the collection of the passed entity type
            '^whose index in the subset of members with the passed entity type matches the passed index.
            '~i.e. Entity(3,dxxEntityTypes.Line) returns the third line in the collection.
            '~a negative index causes the search to be performed from the end of the collection.
            Dim cnt As Integer = Count
            Dim si As Integer = 1
            Dim ei As Integer = cnt
            Dim stp As Integer = 1
            Dim aMem As dxfEntity
            Dim idx As Integer = 0
            If aIndex = 0 Or Math.Abs(aIndex) > cnt Or cnt = 0 Then Return Nothing
            If aIndex < 0 Then
                si = cnt
                ei = 1
                stp = -1
                aIndex = Math.Abs(aIndex)
            End If
            For i As Integer = si To ei Step stp
                aMem = Item(i)
                If aMem.EntityType = aEntityType Then
                    cnt += 1
                    If cnt = aIndex Then
                        _rVal = aMem
                        idx = i
                        Exit For
                    End If
                End If
            Next i
            If idx > 0 Then
                If bReturnClone Then _rVal = _rVal.Clone
                If bRemove Then
                    RemoveV(idx, True)
                    If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, _rVal, False, bCountChange:=True)
                End If
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' ^the points used to define the entities bounding rectangle
        ''' </summary>
        ''' <param name="bIncludeSuppressed">flag to include suppressed entities in the return</param>
        ''' <param name="bSuppressInstances">flag to exclude the extent points of the entities defined instances</param>
        ''' <returns></returns>
        Public Function ExtentPoints(Optional bIncludeSuppressed As Boolean = False, Optional bSuppressInstances As Boolean = False) As colDXFVectors


            Return dxfEntities.ExtentPoints(Me, bIncludeSuppressed, bSuppressInstances:=bSuppressInstances)

        End Function
        Friend Function ExtentPointsV(Optional bIncludeSuppressed As Boolean = False) As TVECTORS

            '    ^the points used to determine the entities bounding rectangle

            Dim _rVal As New TVECTORS
            For i As Integer = 1 To Count
                Dim aMem As dxfEntity = Item(i)
                If Not aMem.Suppressed Or (aMem.Suppressed And bIncludeSuppressed) Then
                    aMem.UpdatePath()
                    Dim eExts As TVECTORS = aMem.ExtentPts

                    _rVal.Append(eExts)
                End If
            Next i
            Return _rVal
        End Function

        Public Function ExtractByFlag(aFlag As String, Optional aTag As String = "", Optional bContainsString As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnJustOne As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFEntities
            '#1the flag to search for
            '#2an optional tag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4a entity type to match
            '^returns and removes all the entities that match the search criteria
            Return GetByFlag(aFlag, aTag, bContainsString, aEntityType, bReturnJustOne, False, True, bReturnInverse)
        End Function
        Public Function ExtractByPoints(aFilter As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional bOnisIn As Boolean = True, Optional aCS As dxfPlane = Nothing, Optional rIndices As List(Of Integer) = Nothing, Optional aPrecis As Integer = 3, Optional aEntPointType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt) As colDXFEntities
            '#1point type type parameter
            '#2search type parameter
            '#3the ordinate to search for if the search is ordinate specific
            '#4flag indicating if equal values should be returned
            '#5an optional coordinate system to use
            '#6returns the indices of the matches
            '#7a precision for numerical comparison (1 to 8)
            '^returns the members from the collection whose specified point match the passed search criteria
            Return GetByPoints(aFilter, aOrdinate, bOnisIn, aCS, rIndices, False, True, aPrecis, aEntPointType)
        End Function
        Public Function ExtractByTag(aTag As String, Optional aFlag As String = "", Optional bContainsString As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnJustOne As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFEntities
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4a entity type to match
            '#5flag to stop searching when the first match is found
            '^returns and removes all the entities that match the search criteria
            Return GetByTag(aTag, aFlag, bContainsString, aEntityType, bReturnJustOne, False, True, bReturnInverse)
        End Function

        Friend Function FindByGUID(aGUID As String) As dxfEntity
            If String.IsNullOrWhiteSpace(aGUID) Then Return Nothing
            Return MyBase.Find(Function(mem) mem.GUID = aGUID)
        End Function

        Public Function First(aCount As Integer, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the number of entities to Return
            '#2flag to return copies
            '#3flag to remove the subset from the collection
            '^returns the first members of the collection up to the passed count
            '~i.e. First(4) returns the first 4 members
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aCount <= 0 Then Return _rVal
            If aCount > Count Then aCount = Count
            Dim aEnt As dxfEntity
            For i As Integer = 1 To aCount
                aEnt = Item(i)
                _rVal.Add(aEnt, bAddClone:=bReturnClones)
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Public Function FirstMember(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As dxfEntity
            If Count < 1 Then Return Nothing
            If aEntityType = dxxEntityTypes.Undefined Then Return Item(1)
            Dim emems As List(Of dxfEntity) = FindAll(Function(x) x.EntityType = aEntityType)
            Return emems.FirstOrDefault()
        End Function
        Public Function FlagList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty

            For i As Integer = 1 To Count
                Dim aMem As dxfEntity = Item(i)
                TLISTS.Add(_rVal, aMem.Flag, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function FlaggedCount(Optional aFlagValue As String = Nothing, Optional aType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bContaining As Boolean = False) As Integer
            Dim _rVal As Integer = 0
            '#1the flag to count
            '^returns the number of members in the collection that have their flag property defined
            '^or have a flag that matches the passed value

            Dim bTest As Boolean
            Dim aStr As String = String.Empty
            bTest = aFlagValue IsNot Nothing
            If bTest Then aStr = aFlagValue
            For i As Integer = 1 To Count
                Dim aEnt As dxfEntity = Item(i)
                If aType = dxxEntityTypes.Undefined Or aEnt.EntityType = aType Then
                    If Not bTest Then
                        If aEnt.Flag <> "" Then _rVal += 1
                    Else
                        If Not bContaining Then
                            If String.Compare(aEnt.Flag, aStr, ignoreCase:=True) = 0 Then _rVal += 1
                        Else
                            If aEnt.Flag.IndexOf(aStr, StringComparison.OrdinalIgnoreCase) + 1 > 0 Then _rVal += 1
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function Flags(Optional bUniqueOnly As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            '#1flag to return only the unique flags
            '^returns a collection of strings containing the flags of the members
            Dim aMem As dxfEntity
            Dim bKeep As Boolean
            For i As Integer = 1 To Count
                aMem = Item(i)
                bKeep = True
                If bUniqueOnly Then
                    bKeep = _rVal.FindIndex(Function(mem) String.Compare(mem, aMem.Flag, True) = 0) < 0
                End If
                If bKeep Then _rVal.Add(aMem.Flag)
            Next i
            Return _rVal
        End Function
        Public Function GetAbove(Yord As Double, Optional IncludeCrossers As Boolean = True, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aCS As dxfPlane = Nothing, Optional bReturnInverse As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '#1the Y ordinate to test
            '#2flag to include the arcs that cross this Y ordinate but are not complete above of it
            '^returns the arcs that are completely above of the passed Y ordinate

            Dim aPl As New TPLANE(aCS)
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            Dim Y1 As Double = Math.Round(Yord, 6)
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = srch.FindAll(Function(mem) mem.EntityType = aEntityType)
            End If

            For Each aEnt As dxfEntity In srch
                Dim ep As TVECTOR = aEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt).Strukture.WithRespectTo(aPl)
                Dim sp As TVECTOR = aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt).Strukture.WithRespectTo(aPl)
                Dim bKeep As Boolean = False
                Dim y3 As Double = Math.Round(ep.Y, 6)
                Dim Y2 As Double = Math.Round(sp.Y, 6)
                If Not bReturnInverse Then
                    If Y2 >= Y1 And y3 >= Y1 Then bKeep = True
                Else
                    If Y2 <= Y1 And y3 <= Y1 Then bKeep = True
                End If
                If IncludeCrossers And Not bKeep Then
                    If (Y2 >= Y1 And y3 <= Y1) Or (Y2 <= Y1 And y3 >= Y1) Then bKeep = True
                End If
                If bKeep Then _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
            Next
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Public Function GetAtCoordinate(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '#1the X coordinate to match
            '#2the Y coordinate to match
            '#3the Z coordinate to match
            '#4an optional coordinate system to use
            '#5an entity type filter
            '#6a precision for the comparison (1 to 8)
            '#7flag to return copies
            '#8flag to remove the return
            '^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
            '~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
            '~say ony an X value is passed, then all the vectors with the same X ordinate are returned regarless of their
            '~respective Y and Z ordinate values.
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            If aDefPtType < dxxEntDefPointTypes.StartPt Or aDefPtType > dxxEntDefPointTypes.EndPt Then aDefPtType = dxxEntDefPointTypes.HandlePt
            Dim DoX As Boolean = aX IsNot Nothing And aX.HasValue
            Dim doY As Boolean = aY IsNot Nothing And aY.HasValue
            Dim doZ As Boolean = aZ IsNot Nothing And aZ.HasValue
            Dim isMatchX As Boolean
            Dim isMatchY As Boolean
            Dim isMatchZ As Boolean
            Dim xx As Double
            Dim yy As Double
            Dim zz As Double
            Dim v1 As TVECTOR
            Dim bPLn As Boolean
            Dim aPl As New TPLANE(aPlane, bPLn)
            Dim P1 As dxfVector
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = srch.FindAll(Function(mem) mem.EntityType = aEntityType)
            End If
            If DoX Then xx = Math.Round(aX.Value, aPrecis)
            If doY Then yy = Math.Round(aY.Value, aPrecis)
            If doZ Then zz = Math.Round(aZ.Value, aPrecis)
            If Not DoX And Not doY And Not doZ Then Return _rVal
            For Each aEnt As dxfEntity In srch
                P1 = aEnt.DefinitionPoint(aDefPtType)
                If P1 Is Nothing Then P1 = New dxfVector
                v1 = P1.Strukture.Rounded(aPrecis)
                If bPLn Then v1 = v1.WithRespectTo(aPl)
                isMatchX = True
                isMatchY = True
                isMatchZ = True
                If DoX Then
                    isMatchX = (v1.X = xx)
                End If
                If doY And isMatchX Then
                    isMatchY = (v1.Y = yy)
                End If
                If doZ And isMatchX And isMatchY Then
                    isMatchZ = (v1.Z = zz)
                End If
                If isMatchX And isMatchY And isMatchZ Then
                    _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
                End If
            Next
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Public Function GetBetweenOrdinates(aOrd1 As Double, aOrd2 As Double, Optional aOrdinateType As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X, Optional aPrecision As Integer = 4, Optional bOnisIn As Boolean = True, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.Undefined, Optional bReturnClones As Boolean = False, Optional aCS As dxfPlane = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '#1then first ordinate to search for
            '#2the second ordinate to search for
            '#3the ordinate type to use in the search
            '#4the precision to use for the comparison
            '#5flag indicating if on a bound is to be considered in
            '#6if the definition point type is passed the search is based soly on the indicated entity point otherwise the bounding rectangle of the entity is used
            '^returns the entities from the collection that lie completely between the passed ordinates
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            If aEntityType <> dxxEntityTypes.Undefined Then srch = srch.FindAll(Function(mem) mem.EntityType = aEntityType)
            aPrecision = TVALUES.LimitedValue(aPrecision, 0, 8)
            If aDefPtType > dxxEntDefPointTypes.StartPt And aDefPtType < dxxEntDefPointTypes.EndPt Then
                Dim bIds As New List(Of Integer)
                Dim ePts As colDXFVectors = DefinitionPoints(aDefPtType, aSearchCol:=srch)
                Dim mPts As colDXFVectors = ePts.GetBetweenOrdinates(aOrdinateType, aOrd1, aOrd2, bOnisIn, aCS, aPrecision, bIds)
                For Each index As Integer In bIds
                    _rVal.Add(SetMemberInfo(srch.Item(index - 1), bReturnClones))
                Next
            Else
                Dim o1 As Double
                Dim o2 As Double
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim aPlane As New TPLANE("")
                Dim lim1 As Double
                Dim lim2 As Double
                Dim aBnd As TPLANE
                If aCS IsNot Nothing Then aPlane = New TPLANE(aCS)
                lim1 = Math.Round(aOrd1, aPrecision)
                lim2 = Math.Round(aOrd2, aPrecision)
                TVALUES.SortTwoValues(True, lim1, lim2)
                For Each aEnt As dxfEntity In srch
                    aBnd = aEnt.Bounds
                    v1 = aBnd.Point(dxxRectanglePts.TopLeft, True)
                    v2 = aBnd.Point(dxxRectanglePts.BottomRight, True)
                    v1 = v1.WithRespectTo(aPlane)
                    v2 = v1.WithRespectTo(aPlane)
                    If aOrdinateType = dxxOrdinateDescriptors.Z Then
                        o1 = Math.Round(v1.Z, aPrecision)
                        o2 = Math.Round(v2.Z, aPrecision)
                    ElseIf aOrdinateType = dxxOrdinateDescriptors.Y Then
                        o1 = Math.Round(v1.Y, aPrecision)
                        o2 = Math.Round(v2.Y, aPrecision)
                    Else
                        o1 = Math.Round(v1.X, aPrecision)
                        o2 = Math.Round(v2.X, aPrecision)
                    End If
                    TVALUES.SortTwoValues(True, o1, o2)
                    If (bOnisIn And (o1 >= lim1) And (o2 >= lim1)) Or (Not bOnisIn And (o1 > lim1) And (o2 > lim1)) Then
                        If (bOnisIn And (o1 <= lim2) And (o2 <= lim2)) Or (Not bOnisIn And (o1 < lim2) And (o2 < lim2)) Then
                            _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
                        End If
                    End If
                Next
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the members whose center point matches the passed location
        ''' </summary>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="bReturnClones">flag to return clone</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="bRemove">flag to remove the return</param>
        ''' <param name="bGetJustOne">flag to return the first match only</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Function GetByCenter(aMatchPoint As iVector, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bGetJustOne As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)
            Return GetByDefPoint(aMatchPoint, dxxEntDefPointTypes.Center, aPrecis:=aPrecis, bReturnClones:=bReturnClones, bRemove:=bReturnClones, bGetJustOne:=bGetJustOne, aEntityType:=aEntityType, aGraphicType:=aGraphicType)
        End Function

        ''' <summary>
        ''' returns the members whose end point matches the passed location
        ''' </summary>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="bReturnClones">flag to return clone</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="bRemove">flag to remove the return</param>
        ''' <param name="bGetJustOne">flag to return the first match only</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Function GetByEndPt(aMatchPoint As iVector, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bGetJustOne As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)
            Return GetByDefPoint(aMatchPoint, dxxEntDefPointTypes.EndPt, aPrecis:=aPrecis, bReturnClones:=bReturnClones, bRemove:=bReturnClones, bGetJustOne:=bGetJustOne, aEntityType:=aEntityType, aGraphicType:=aGraphicType)
        End Function

        ''' <summary>
        ''' returns the members whose start point matches the passed location
        ''' </summary>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="bReturnClones">flag to return clone</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="bRemove">flag to remove the return</param>
        ''' <param name="bGetJustOne">flag to return the first match only</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Function GetByStartPt(aMatchPoint As iVector, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bGetJustOne As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)
            Return GetByDefPoint(aMatchPoint, dxxEntDefPointTypes.StartPt, aPrecis:=aPrecis, bReturnClones:=bReturnClones, bRemove:=bReturnClones, bGetJustOne:=bGetJustOne, aEntityType:=aEntityType, aGraphicType:=aGraphicType)
        End Function
        ''' <summary>
        ''' returns the members whose handle point matches the passed location
        ''' </summary>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="bReturnClones">flag to return clone</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="bRemove">flag to remove the return</param>
        ''' <param name="bGetJustOne">flag to return the first match only</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Function GetByHandlePt(aMatchPoint As iVector, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bGetJustOne As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)
            Return GetByDefPoint(aMatchPoint, dxxEntDefPointTypes.HandlePt, aPrecis:=aPrecis, bReturnClones:=bReturnClones, bRemove:=bReturnClones, bGetJustOne:=bGetJustOne, aEntityType:=aEntityType, aGraphicType:=aGraphicType)
        End Function

        ''' <summary>
        ''' returns the members whose mid point matches the passed location
        ''' </summary>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="bReturnClones">flag to return clone</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="bRemove">flag to remove the return</param>
        ''' <param name="bGetJustOne">flag to return the first match only</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Function GetByMidPt(aMatchPoint As iVector, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bGetJustOne As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)
            Return GetByDefPoint(aMatchPoint, dxxEntDefPointTypes.MidPt, aPrecis:=aPrecis, bReturnClones:=bReturnClones, bRemove:=bReturnClones, bGetJustOne:=bGetJustOne, aEntityType:=aEntityType, aGraphicType:=aGraphicType)
        End Function
        Public Function GetByColumn(aCol As Integer, Optional aRow As Integer? = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional rMaxRow As Integer? = Nothing, Optional rMaxCol As Integer? = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the column to search for
            '#2an optional row that all returns must match
            '#3a entity type filter
            '#4flag to return clones
            '#5returns then highest column number in the collection
            '#6returns then highest row number in the collection
            '#7flag to remove the return set from this collection
            If Not bRemove And Not bReturnClones Then _rVal.MaintainIndices = False
            Dim aMem As dxfEntity
            Dim bTest As Boolean
            Dim arw As Integer
            If rMaxRow IsNot Nothing Then rMaxRow = 0
            If rMaxCol IsNot Nothing Then rMaxCol = 0
            If aRow IsNot Nothing Then
                If aRow.HasValue Then
                    bTest = True
                    arw = aRow.Value
                End If
            End If
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or aMem.EntityType = aEntityType Then
                    If rMaxRow IsNot Nothing Then
                        If aMem.Row > rMaxRow Then rMaxRow = aMem.Row
                    End If
                    If rMaxCol IsNot Nothing Then
                        If aMem.Col > rMaxCol Then rMaxCol = aMem.Col
                    End If
                    If aMem.Col = aCol Then
                        If Not bTest Then
                            _rVal.Add(aMem, bAddClone:=bReturnClones)
                        Else
                            If aMem.Row = arw Then
                                _rVal.Add(aMem, bAddClone:=bReturnClones)
                            End If
                        End If
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then
                    RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, bCountChange:=True)
                End If
            End If
            Return _rVal
        End Function
        Public Function GetByCrossesX(Optional CrossesVal As Boolean = True, Optional aCS As dxfPlane = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '^returns the entities that have a crosses X value that matches the passed value
            Dim aEnt As dxfEntity
            Dim aLine As dxeLine
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.EntityType = dxxEntityTypes.Line Then
                    aLine = aEnt
                    If aLine.CrossesX(aCS) = CrossesVal Then _rVal.Add(aLine, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, bCountChange:=True)
            End If
            Return _rVal
        End Function
        Public Function GetByCrossesY(Optional CrossesVal As Boolean = True, Optional aCS As dxfPlane = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '^'^returns the entities that have a crosses Y value that matches the passed value
            Dim aLine As dxeLine
            Dim aEnt As dxfEntity
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.EntityType = dxxEntityTypes.Line Then
                    aLine = aEnt
                    If aLine.CrossesY(aCS) = CrossesVal Then _rVal.Add(aLine, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the members whose specified definition point match the passed location
        ''' </summary>
        ''' <param name="aMatchPoint">the point to match</param>
        ''' <param name="aVectorType">the entity defintion point type to seach by</param>
        ''' <param name="bReturnClones">flag to return clone</param>
        ''' <param name="aPrecis">the precision for the comparison</param>
        ''' <param name="bRemove">flag to remove the return</param>
        ''' <param name="bGetJustOne">flag to return the first match only</param>
        ''' <param name="aEntityType">a entity type filter</param>
        ''' <param name="aGraphicType">a graphic type filter</param>
        ''' <returns></returns>
        Public Function GetByDefPoint(aMatchPoint As iVector, Optional aVectorType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bGetJustOne As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As List(Of dxfEntity)
            Dim mems As List(Of dxfEntity) = dxfEntities.GetByDefPoint(Me, aMatchPoint, aVectorType, aPrecis, bReturnClones:=False, bGetJustOne:=bGetJustOne, aEntityType:=aEntityType, aGraphicType:=aGraphicType)
            Dim _rVal As New List(Of dxfEntity)
            For Each mem As dxfEntity In mems

                _rVal.Add(SetMemberInfo(mem, bReturnClones))
            Next
            If bRemove Then
                If RemoveMembersV(mems, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, mems, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByNearestDefPoint(aMatchPoint As iVector, Optional aVectorType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPrecis As Integer = 3, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As dxfEntity
            Dim mem As dxfEntity = dxfEntities.GetByNearestDefPoint(Me, aMatchPoint, aVectorType, aPrecis, bReturnClone:=False, aEntityType:=aEntityType, aGraphicType:=aGraphicType)
            If mem Is Nothing Then Return Nothing
            Dim _rVal As dxfEntity = SetMemberInfo(mem, bReturnClone)
            If bRemove Then
                If RemoveV(IndexOf(mem), True) IsNot Nothing And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, mem, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByDisplayVariableValue(aVarType As dxxDisplayProperties, aValue As Object, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bCaseSensitive As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the variable to search on
            '#2the value to search for
            '#3flag to return clones
            '#4flag to remove the results
            '^searchs for entities with matching values to the passed variable name
            If bRemove Then bReturnClones = False
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aEnt As dxfEntity
            Dim cStruc As TDISPLAYVARS
            Dim bVal As Object
            Dim bStrComp As Boolean
            Dim bKeep As Boolean
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            For Each aEnt In srch
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    cStruc = aEnt.DisplaySettings.Strukture
                    bKeep = False
                    Select Case aVarType
                        Case dxxDisplayProperties.Color
                            bVal = cStruc.Color
                        Case dxxDisplayProperties.LayerName
                            bStrComp = True
                            bVal = cStruc.LayerName
                        Case dxxDisplayProperties.TextStyle
                            bStrComp = True
                            bVal = cStruc.TextStyle
                        Case dxxDisplayProperties.DimStyle
                            bStrComp = True
                            bVal = cStruc.DimStyle
                        Case dxxDisplayProperties.Linetype
                            bStrComp = True
                            bVal = cStruc.Linetype
                        Case dxxDisplayProperties.LTScale
                            bVal = cStruc.LTScale
                        Case dxxDisplayProperties.Suppressed
                            bVal = cStruc.Suppressed
                        Case Else
                            Return _rVal
                    End Select
                    If bStrComp Then
                        If bCaseSensitive Then
                            bKeep = (aValue = bVal)
                        Else
                            bKeep = String.Compare(aValue, bVal, ignoreCase:=True) = 0
                        End If
                    Else
                        bKeep = aValue = bVal
                    End If
                    If Not bReturnInverse Then
                        If bKeep Then _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
                    Else
                        If Not bKeep Then _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
                    End If
                End If
            Next
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function

        Public Function GetByEntityType(aEntityType As dxxEntityTypes, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aEntityType = dxxGraphicTypes.Undefined Or Count <= 0 Then Return New colDXFEntities
            Dim ret As List(Of dxfEntity) = FindAll(Function(mem) mem.EntityType = aEntityType)
            _rVal.AddRange(ret, bReturnClones)
            If bRemove Then
                If RemoveMembersV(ret, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function

        Public Function GetEntities(aEntityType As dxxEntityTypes, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False) As List(Of dxfEntity)

            If aEntityType = dxxGraphicTypes.Undefined Or Count <= 0 Then Return New List(Of dxfEntity)
            Dim ret As List(Of dxfEntity) = FindAll(Function(mem) mem.EntityType = aEntityType)

            If bRemove Then
                If RemoveMembersV(ret, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, ret, False, False, True)
            End If
            Return ret
        End Function

        Public Function ConvertText(aTextType As dxxTextTypes, bTextType As dxxTextTypes) As List(Of dxfEntity)
            If aTextType = bTextType Or Count <= 0 Then Return New List(Of dxfEntity)()
            Dim tmems As List(Of dxfEntity) = FindAll(Function(x) x.TextType = aTextType)
            If tmems.Count <= 0 Then Return New List(Of dxfEntity)()
            If RemoveMembersV(tmems, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, tmems, False, False, True)

            Dim ret As New List(Of dxfEntity)
            For Each tmem As dxfEntity In tmems
                Dim txt As dxeText = tmem

                Dim mtxt As List(Of dxeText) = txt.ConvertToTextType(bTextType)
                For Each subtxt As dxeText In mtxt
                    ret.Add(Add(subtxt))
                Next
            Next
            Return ret

        End Function

        Public Function GetByExtrusionDirection(aDirection As dxfDirection, Optional bReturnClones As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aPrecis As Integer = 3, Optional bRemove As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aDirection Is Nothing Then Return _rVal
            Dim aEnt As dxfEntity
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim bKeep As Boolean
            v1 = aDirection.Strukture
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aGraphicType = dxxGraphicTypes.Undefined Or aEnt.GraphicType = aGraphicType Then
                    If aEntityType = dxxEntityTypes.Undefined Or aEnt.EntityType = aEntityType Then
                        v2 = aEnt.Plane.ZDirectionV
                        bKeep = v1.Equals(v2, True, aPrecis)
                        If (Not bReturnInverse And bKeep) Or (bReturnInverse And Not bKeep) Then _rVal.Add(aEnt, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns the entities that match the passed search criteria
        ''' </summary>
        ''' <remarks>
        ''' The search is based on the entity flag and an optional tag string.
        ''' </remarks>
        ''' <param name="aFlag">the flag to search for</param>
        ''' <param name="aTag">an optional tag to include in the search criteria</param>
        ''' <param name="bContainsString">flag to include any member whose flag string contains the passed search strings instead of a full string match</param>
        ''' <param name="aEntityType">a entity type to filter for</param>
        ''' <param name="bReturnJustOne">flag to stop searching when the first match is found</param>
        ''' <param name="bReturnClones">flag to return clones of the matches</param>
        ''' <param name="bRemove">flag to to remove the match from the collection</param>
        ''' <param name="bReturnInverse">flag to return the members that don't match the search criteria</param>
        ''' <param name="aValue">an optional value to include in the search criteria</param>
        ''' <param name="aDelimitor">if passed, the the search value is treated as  a delimited list of individual values to search for </param>
        ''' <param name="bIgnoreCase">flag to conduct the search without regard to case</param>
        ''' <returns></returns>
        Public Function GetByFlag(aFlag As String, Optional aTag As String = Nothing, Optional bContainsString As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aValue As Double? = Nothing, Optional aDelimitor As String = Nothing, Optional bIgnoreCase As Boolean = True) As colDXFEntities
            Dim _rVal As New colDXFEntities

            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aStr As String = String.Empty
            Dim bTest As Boolean = aTag IsNot Nothing
            Dim bTestVal As Boolean = aValue IsNot Nothing
            If bTestVal Then bTestVal = aValue.HasValue
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            If bTest Then aStr = aTag
            Dim ret As New List(Of dxfEntity)
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = srch.FindAll(Function(mem) mem.EntityType = aEntityType)
            End If
            Dim srchvals() As String
            If String.IsNullOrWhiteSpace(aDelimitor) Then
                srchvals = New String() {aFlag}
            Else
                srchvals = aFlag.Split(aDelimitor)
            End If
            If bTestVal Then srch = srch.FindAll(Function(x) x.Value = aValue.Value)
            For Each flag As String In srchvals
                If Not bContainsString Then
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) = 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Tag, aStr, ignoreCase:=bIgnoreCase) = 0))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) < 0 And String.Compare(mem.Tag, aStr, ignoreCase:=bIgnoreCase) < 0))
                        End If
                    End If
                Else
                    Dim comp As StringComparison = StringComparison.OrdinalIgnoreCase
                    If Not bIgnoreCase Then comp = StringComparison.Ordinal
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(flag, comparisonType:=comp) > -1))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(flag, comparisonType:=comp) > -1 And mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aStr, comparisonType:=comp) > -1))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(flag, comparisonType:=comp) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Tag.IndexOf(flag, comparisonType:=comp) < 0 And mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aStr, comparisonType:=comp) < 0))
                        End If
                    End If
                End If
            Next
            If ret.Count > 0 Then
                If bReturnJustOne Then
                    _rVal.Add(Item(IndexOf(ret.Item(0))), bAddClone:=bReturnClones)
                Else
                    _rVal.CollectionObj = ret
                End If
            Else
                Return _rVal
            End If
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByGUID(aGUID As String, Optional bRemove As Boolean = False, Optional bReturnClone As Boolean = False) As dxfEntity
            If String.IsNullOrWhiteSpace(aGUID) Then Return Nothing
            Dim _rVal As dxfEntity = Find(Function(mem) String.Compare(mem.GUID, aGUID, ignoreCase:=True) = 0)
            If _rVal Is Nothing Then Return Nothing
            If bRemove Then Return Remove(_rVal) Else Return SetMemberInfo(_rVal, bReturnClone)
        End Function
        Public Function GetByGraphicType(aGraphicType As dxxGraphicTypes, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aSearchTag As String = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the graphic types to return
            '#2flag to return copies of the matching members
            '#3flag to remove the matching members
            '#4flag to return members that don't match the passed graphic type
            '^returns members from the collection whose graphic type is matchs the passed graphic type(s)
            '~graphic type is an addative bit code so passing  'dxxGraphicTypes.Line + dxxGraphicTypes.Dimension'
            '~returns only the lines and dimensions
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aGraphicType = dxxGraphicTypes.Undefined Then Return _rVal
            Dim aMem As dxfEntity
            Dim gTypes As String = String.Empty
            Dim gCnt As Integer
            Dim bList As Boolean
            Dim tgmatch As String = String.Empty
            Dim testtag As Boolean = Not String.IsNullOrEmpty(aSearchTag)

            If testtag Then
                tgmatch = aSearchTag
            End If

            If aGraphicType > 0 Then
                gCnt = TVALUES.BitCode_Decompose(aGraphicType, gTypes).Count
                If gCnt = 0 Then Return _rVal
            End If
            For i As Integer = 1 To Count
                aMem = Item(i)
                If testtag Then
                    If String.Compare(aMem.Tag, tgmatch, True) <> 0 Then Continue For
                End If

                If gCnt > 1 Then bList = TLISTS.Contains(aMem.GraphicType, gTypes)
                If (gCnt = 1 And aMem.GraphicType = aGraphicType) Or aGraphicType <= 0 Or bList Then
                    If Not bReturnInverse Then
                        _rVal.Add(aMem, bAddClone:=bReturnClones)
                    Else
                        _rVal.Add(aMem, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByGroupName(aGroupName As String, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the group name to search for
            '^returns the members whose group name matches the passed string
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aEnt As dxfEntity
            Dim bKeep As Boolean
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    bKeep = String.Compare(aEnt.GroupName, aGroupName, ignoreCase:=True) = 0
                    If (Not bReturnInverse And bKeep) Or (bReturnInverse And Not bKeep) Then
                        _rVal.Add(aEnt, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByHandle(aHandle As String, Optional bRemove As Boolean = False, Optional bReturnClone As Boolean = False) As dxfEntity
            '#1the handle to search for
            '^returns the first member whose handle matches the passed string
            If String.IsNullOrWhiteSpace(aHandle) Then Return Nothing
            Dim _rVal As dxfEntity = Find(Function(mem) mem.Handle = aHandle)
            If _rVal Is Nothing Then Return Nothing
            If bRemove Then Return Remove(_rVal) Else Return SetMemberInfo(_rVal, bReturnClone)
        End Function

        Public Function GetByHandles(aEntHandles As List(Of String), Optional bReturnTheInverse As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional rIndices As List(Of Integer) = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If aEntHandles Is Nothing Then Return _rVal
            If aEntHandles.Count <= 0 Then Return _rVal
            '^returns the listed listed in the passed collection of handles
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim bAddit As Boolean
            Dim aEnt As dxfEntity
            Dim aStr As String
            For j As Integer = 1 To Count
                aEnt = Item(j)
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                ''CheckHandle:
                bAddit = False
                For i As Integer = 1 To aEntHandles.Count
                    aStr = aEntHandles.Item(i)
                    If aStr <> "" Then
                        If String.Compare(aStr, "*,*", ignoreCase:=True) = 0 Then
                            bAddit = True
                            Exit For
                        End If
                        If String.Compare(aStr, aEnt.Handle, ignoreCase:=True) = 0 Then
                            bAddit = True
                            Exit For
                        End If
                        If Right(aStr, 2) = ",*" Then
                            If String.Compare(Left(aStr, aStr.Length - 2), aEnt.Tag, ignoreCase:=True) = 0 Then
                                bAddit = True
                                Exit For
                            End If
                        End If
                        If Left(aStr, 2) = "*," Then
                            If String.Compare(Right(aStr, aStr.Length - 2), aEnt.Flag, ignoreCase:=True) = 0 Then
                                bAddit = True
                                Exit For
                            End If
                        End If
                    End If
                Next i
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                If Not bReturnTheInverse Then
                    If bAddit Then
                        _rVal.Add(aEnt, bAddClone:=bReturnClones)
                        If rIndices IsNot Nothing Then rIndices.Add(j)
                    End If
                Else
                    If Not bAddit Then
                        _rVal.Add(aEnt, bAddClone:=bReturnClones)
                        If rIndices IsNot Nothing Then rIndices.Add(j)
                    End If
                End If
            Next j
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function GetByIdentifier(aIdentifier As String, Optional bContainsString As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False, Optional bReturnJustOne As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aIdentifier Is Nothing Then Return _rVal
            '#1the identifier to search for
            '#2flag to include any member whose identifier string contains the passed search strings instead of a full string match
            '^returns all the entities that match the search criteria
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = srch.FindAll(Function(mem) mem.EntityType = aEntityType)
            End If
            If Not bContainsString Then
                srch = srch.FindAll(Function(mem) mem.Identifier.ToUpper = aIdentifier.ToUpper)
            Else
                srch = srch.FindAll(Function(mem) mem.Identifier.ToUpper.Contains(aIdentifier.ToUpper))
            End If
            For Each aEnt As dxfEntity In srch
                _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
            Next
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByLayer(aValue As String, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False) As colDXFEntities
            '#1the layer name to search for
            '#2flag to return clones
            '#3flag to remove the results
            '#4a entity type to search for
            '#5flag to return the inverse of the request
            '^searchs for entities with matching values to the passed variable name
            Return GetByDisplayVariableValue(dxxDisplayProperties.LayerName, aValue, bReturnClones, bRemove, aEntityType:=aEntityType, bReturnInverse:=bReturnInverse)
        End Function
        Public Function GetByLayerColorAndLinetype(Optional aLayer As String = Nothing, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False, Optional bReturnJustOne As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '#1the layer name to search for
            '#2the color to search for
            '#3the linetype to search for
            '#4flag to return clones
            '#5flag to remove the results
            '#6a entity type to search for
            '#7flag to return the inverse of the request
            '^searchs for entities with matching values to the passed criteria
            Dim bTest1 As Boolean = Not String.IsNullOrWhiteSpace(aLayer)
            Dim bTest2 As Boolean = aColor <> dxxColors.Undefined
            Dim bTest3 As Boolean = Not String.IsNullOrWhiteSpace(aLineType)
            Dim comp1 As String = String.Empty
            Dim comp2 As Integer
            Dim comp3 As String = String.Empty
            Dim aEnt As dxfEntity
            Dim bKeep As Boolean
            If bTest1 Then
                comp1 = Trim(aLayer)
                bTest1 = comp1 <> ""
            End If
            If bTest2 Then comp2 = aColor
            If bTest3 Then
                comp3 = Trim(aLineType)
                bTest3 = comp3 <> ""
            End If
            For i As Integer = 1 To Count
                aEnt = Item(i)
                bKeep = True
                If aEntityType <> dxxEntityTypes.Undefined Then
                    If aEnt.EntityType <> aEntityType Then bKeep = False
                End If
                If bKeep And bTest1 Then
                    If String.Compare(aEnt.LayerName, comp1, ignoreCase:=True) <> 0 Then bKeep = False
                End If
                If bKeep And bTest2 Then
                    If aEnt.Color <> comp2 Then bKeep = False
                End If
                If bKeep And bTest3 Then
                    If String.Compare(aEnt.Linetype, comp3, ignoreCase:=True) <> 0 Then bKeep = False
                End If
                If Not bReturnInverse Then
                    If bKeep Then
                        If Not bReturnClones Then _rVal.Add(aEnt) Else _rVal.Add(aEnt.Clone())
                        If bReturnJustOne Then Exit For
                    End If
                Else
                    If Not bKeep Then
                        If Not bReturnClones Then _rVal.Add(aEnt) Else _rVal.Add(aEnt.Clone())
                        If bReturnJustOne Then Exit For
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByLength(aLength As Double, Optional bReturnClones As Boolean = False, Optional aPrecis As Integer = 3, Optional bRemove As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False, Optional aOthers As colDXFEntities = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the length to search for
            '#2flag to return copies
            '#3the pricision for comparision
            '#4flag to remove the return
            '#5a type to search for
            '#6flag to return the members not equal to the passed length
            '#7returns the anti-return if a intitialized collection is passed
            '^returns the members whose length is eqaul to the passed length with the passed precision
            '~if the passed length is less than 0 the entity with the max length is returns.
            '~if the passed length is less than 0 and bReturnInverse is True the entity with the smallest length is returns.
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If Count <= 0 Then Return _rVal
            Dim aEnt As dxfEntity
            Dim dif As Double
            Dim bKeep As Boolean
            Dim aMax As Double
            Dim aMin As Double
            Dim imax As Integer
            Dim imin As Integer
            Dim eLen As Double
            aMax = 2.6E+30
            aMin = -10
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    eLen = aEnt.Length
                    dif = Math.Round(Math.Abs(eLen - aLength), aPrecis)
                    If aLength >= 0 Then
                        bKeep = dif <= 0
                        If Not bReturnInverse Then
                            If bKeep Then
                                _rVal.Add(aEnt, bAddClone:=bReturnClones)
                            Else
                                If aOthers IsNot Nothing Then aOthers.Add(aEnt, bAddClone:=bReturnClones)
                            End If
                        Else
                            If Not bKeep Then
                                _rVal.Add(aEnt, bAddClone:=bReturnClones)
                            Else
                                If aOthers IsNot Nothing Then aOthers.Add(aEnt, bAddClone:=bReturnClones)
                            End If
                        End If
                    Else
                        If eLen < aMax Then
                            aMax = eLen
                            imax = i
                        End If
                        If eLen > aMin Then
                            aMin = eLen
                            imin = i
                        End If
                    End If
                End If
            Next i
            If aLength < 0 Then
                If Not bReturnInverse Then
                    _rVal.Add(Item(imax), bAddClone:=bReturnClones)
                    If aOthers IsNot Nothing Then aOthers.Add(Item(imin), bAddClone:=bReturnClones)
                Else
                    _rVal.Add(Item(imin), bAddClone:=bReturnClones)
                    If aOthers IsNot Nothing Then aOthers.Add(Item(imax), bAddClone:=bReturnClones)
                End If
            End If
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByLineType(aValue As String, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False) As colDXFEntities
            '#1the linetype to search for
            '#2flag to return clones
            '#3flag to remove the results
            '#4a entity type to search for
            '#5flag to return the inverse of the request
            '^searchs for entities with matching values to the passed variable name
            Return GetByDisplayVariableValue(dxxDisplayProperties.Linetype, aValue, bReturnClones, bRemove, aEntityType:=aEntityType, bReturnInverse:=bReturnInverse)
        End Function

        Public Function GetByName(aName As String, Optional aTag As String = Nothing, Optional bContainsString As Boolean = False,
                                  Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnJustOne As Boolean = False,
                                  Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aName Is Nothing Then Return _rVal
            '#1the name to search for
            '#2an optional tag to include in the search criteria
            '#3flag to include any member whose name string contains the passed search strings instead of a full string match
            '#4flag to stop searching when the first match is found
            '#5flag to return clones of the matches
            '#6flag to to remove the match from the collection
            '^returns all the vectors that match the search criteria
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = srch.FindAll(Function(mem) mem.EntityType = aName)
            End If
            If Not bContainsString Then
                srch = srch.FindAll(Function(mem) mem.Name.ToUpper = aName.ToUpper)
            Else
                srch = srch.FindAll(Function(mem) mem.Name.ToUpper.Contains(aName.ToUpper))
            End If
            If aTag IsNot Nothing Then
                srch = srch.FindAll(Function(mem) mem.Tag.ToUpper = aTag.ToUpper)
            End If
            For Each aEnt As dxfEntity In srch
                _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
            Next
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function

        Public Function GetByPlane(aPlane As dxfPlane, Optional bReturnClones As Boolean = False, Optional aPrecis As Integer = 1, Optional bRemove As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If dxfPlane.IsNull(aPlane) Then Return _rVal
            Dim aEnt As dxfEntity
            Dim aPl As New TPLANE(aPlane)
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aGraphicType = dxxGraphicTypes.Undefined Or aEnt.GraphicType = aGraphicType Then
                    If aEntityType = dxxEntityTypes.Undefined Or aEnt.EntityType = aEntityType Then
                        If aPl.IsCoplanar(aEnt.Plane.Strukture, aPrecis) Then _rVal.Add(aEnt, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByPlaneDirection(aDirectionObj As iVector, aDirToCompare As dxxOrdinateDescriptors, Optional bCompareInverseDirection As Boolean = True, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1an object to extract a direction vector from
            '#2indicates what primary direction to compare
            '#3flag indicating that the inverse of the passed direction should also be treated as equal
            '#4the precision to apply
            '#5flag to return clones of the matches rather that the matches themselves
            '#6flag to remove the matches from this collection
            '^returns the members whose plane directions are equal to the passed direction withing the passed precision
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aDirectionObj Is Nothing Then Return _rVal
            Dim aMem As dxfEntity
            Dim aPl As TPLANE
            Dim aDir As New TVECTOR(aDirectionObj)
            Dim bDir As TVECTOR
            Dim aFlg As Boolean
            aDir.Normalize(rVectorIsNull:=aFlg)
            If aFlg Then Return _rVal
            If aDirToCompare < dxxOrdinateDescriptors.X Or aDirToCompare > dxxOrdinateDescriptors.Z Then aDirToCompare = dxxOrdinateDescriptors.Z
            For i As Integer = 1 To Count
                aMem = Item(i)
                aPl = aMem.PlaneV
                If aDirToCompare = dxxOrdinateDescriptors.X Then
                    bDir = aPl.XDirection
                ElseIf aDirToCompare = dxxOrdinateDescriptors.Y Then
                    bDir = aPl.YDirection
                Else
                    bDir = aPl.ZDirection
                End If
                If aDir.Equals(bDir, bCompareInverseDirection, aPrecis) Then
                    _rVal.Add(aMem, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByPoint(aFilter As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False, Optional aEntPointType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As dxfEntity
            If aEntPointType < dxxEntDefPointTypes.StartPt Then aEntPointType = dxxEntDefPointTypes.StartPt
            If aEntPointType > dxxEntDefPointTypes.EndPt Then aEntPointType = dxxEntDefPointTypes.EndPt
            '#1the entity point type to use
            '#2flag indicating what type of vector to search for
            '#3the ordinate to search for if the search is ordinate specific
            '#4an optional coordinate systemt o use
            '#5a precision for numerical comparison (1 to 8)
            '^returns a entity from the collection whose properties or position in the collection match the passed control flag
            Dim v1 As dxfVector = DefinitionPoints(aEntPointType, aEntityType).GetVector(aFilter, aOrdinate, aPlane, aPrecis:=aPrecis)
            If v1 Is Nothing Then Return Nothing

            Return GetByGUID(v1.GUID, bRemove, bReturnClone)

        End Function

        Public Function Nearest(aVector As dxfVector, Optional aEntPointType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aPlane As dxfPlane = Nothing, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfEntity

            Dim v0 As New dxfVector(aVector)

            If Not dxfPlane.IsNull(aPlane) Then v0 = v0.WithRespectToPlane(aPlane)

            If aEntPointType < dxxEntDefPointTypes.StartPt Then aEntPointType = dxxEntDefPointTypes.StartPt
            If aEntPointType > dxxEntDefPointTypes.EndPt Then aEntPointType = dxxEntDefPointTypes.EndPt
            '#1the search vector to test the distnace to
            '#2the enitiy point type to search for
            '#3an enitity type to filter
            '#4 if passed, the comparision will be againsts points with respect to the plane
            '#5a flag to returns a clone
            '#6a flag to remove the enitiy from the collection
            '^returns an entity from the collection whose position is closest to the passed vector
            Dim v1 As dxfVector = DefinitionPoints(aEntPointType, aEntityType, aPlane:=aPlane).NearestVector(v0)
            If v1 Is Nothing Then Return Nothing

            Return GetByGUID(v1.GUID, bRemove, bReturnClone)

        End Function

        Public Function Farthest(aVector As dxfVector, Optional aEntPointType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aPlane As dxfPlane = Nothing, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfEntity

            Dim v0 As New dxfVector(aVector)
            If Not dxfPlane.IsNull(aPlane) Then v0 = v0.WithRespectToPlane(aPlane)

            If aEntPointType < dxxEntDefPointTypes.StartPt Then aEntPointType = dxxEntDefPointTypes.StartPt
            If aEntPointType > dxxEntDefPointTypes.EndPt Then aEntPointType = dxxEntDefPointTypes.EndPt
            '#1the search vector to test the distnace to
            '#2the enitiy point type to search for
            '#3an enitity type to filter
            '#4 if passed, the comparision will be againsts points with respect to the plane
            '#5a flag to returns a clone
            '#6a flag to remove the enitiy from the collection
            '^returns an entity from the collection whose position is closest to the passed vector
            Dim d1 As Double
            Dim v1 As dxfVector = DefinitionPoints(aEntPointType, aEntityType, aPlane:=aPlane).FarthestVector(v0, d1)
            If v1 Is Nothing Then Return Nothing

            Return GetByGUID(v1.GUID, bRemove, bReturnClone)

        End Function


        Public Function GetByPoints(aFilter As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional bOnisIn As Boolean = True, Optional aCS As dxfPlane = Nothing, Optional rIndices As List(Of Integer) = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aPrecis As Integer = 3, Optional aEntPointType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If bRemove Then bReturnClones = False
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aEntPointType < dxxEntDefPointTypes.StartPt Then aEntPointType = dxxEntDefPointTypes.StartPt
            If aEntPointType > dxxEntDefPointTypes.EndPt Then aEntPointType = dxxEntDefPointTypes.EndPt
            '#1point type type parameter
            '#2search type parameter
            '#3the ordinate to search for if the search is ordinate specific
            '#4flag indicating if equal values should be returned
            '#5an optional coordinate system to use
            '#6returns the indices of the matches
            '#7flag to return copies
            '#8flag to remove the matching set
            '#9a precision for numerical comparison (1 to 8)
            '^returns the members from the collection whose specified point match the passed search criteria
            Dim i As Integer
            Dim ePts As List(Of dxfVector) = DefinitionPoints(aEntPointType).GetVectors(aFilter:=aFilter, aOrdinate:=aOrdinate, bOnIsIn:=bOnisIn, aPlane:=aCS, aPrecis:=aPrecis)
            For i = 1 To ePts.Count
                _rVal.Add(Item(ePts.Item(i - 1).Index, bReturnClone:=bReturnClones))
            Next i
            If bRemove And _rVal.Count > 0 Then
                RemoveMembersV(_rVal, True)
                RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByRow(aRow As Integer, Optional aCol As Integer? = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnClones As Boolean = False, Optional rMaxRow As Integer? = 0, Optional rMaxCol As Integer? = 0, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the row to search for
            '#2an optional column that all returns must match
            '#3a entity type filter
            '#4flag to return clones
            '#5returns then highest row number in the collection
            '#6returns then highest col number in the collection
            '#7flag to remove the return set from this collection
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim bTest As Boolean
            Dim aCl As Integer
            Dim aMem As dxfEntity
            If rMaxRow IsNot Nothing Then rMaxRow = 0
            If rMaxCol IsNot Nothing Then rMaxCol = 0
            If aCol IsNot Nothing Then
                If aCol.HasValue Then
                    bTest = True
                    aCl = aCol.Value
                End If
            End If
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or aMem.EntityType = aEntityType Then
                    If rMaxRow IsNot Nothing Then
                        If aMem.Row > rMaxRow Then rMaxRow = aMem.Row
                    End If
                    If rMaxCol IsNot Nothing Then
                        If aMem.Col > rMaxCol Then rMaxCol = aMem.Col
                    End If
                    If aMem.Row = aRow Then
                        If Not bTest Then
                            _rVal.Add(aMem, bAddClone:=bReturnClones)
                        Else
                            If aMem.Col = aCl Then _rVal.Add(aMem, bAddClone:=bReturnClones)
                        End If
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then
                    RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
                End If
            End If
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns the entities that match the passed search criteria
        ''' </summary>
        ''' <remarks>
        ''' The search is based on the entity flag and an optional tag string.
        ''' </remarks>
        ''' <param name="aTag">the tag to search for</param>
        ''' <param name="aFlag">an optional flag to include in the search criteria</param>
        ''' <param name="bContainsString">flag to include any member whose tag string contains the passed search strings instead of a full string match</param>
        ''' <param name="aEntityType">a entity type to filter for</param>
        ''' <param name="bReturnJustOne">flag to stop searching when the first match is found</param>
        ''' <param name="bReturnClones">flag to return clones of the matches</param>
        ''' <param name="bRemove">flag to to remove the match from the collection</param>
        ''' <param name="bReturnInverse">flag to return the members that don't match the search criteria</param>
        ''' <param name="aValue">an optional value to include in the search criteria</param>
        ''' <param name="aDelimitor">if passed, the the search value is treated as  a delimited list of individual values to search for </param>
        ''' <param name="bIgnoreCase">flag to conduct the search without regard to case</param>
        ''' <returns></returns>
        Public Function GetByTag(aTag As String, Optional aFlag As String = Nothing, Optional bContainsString As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aValue As Double? = Nothing, Optional aDelimitor As String = Nothing, Optional bIgnoreCase As Boolean = True) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If aTag Is Nothing Then Return _rVal
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4a entity type to match
            '#5flag to stop searching when the first match is found
            '#6flag to return clones of the matches
            '#7flag to to remove the match from the collection
            '#8flag to return the members that don't match the search criteria
            '#9an optional value to include in the search criteria
            '^returns all the entities that match the search criteria
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False

            Dim bTest As Boolean = aFlag IsNot Nothing
            Dim bTestVal As Boolean = aValue IsNot Nothing
            If bTestVal Then bTestVal = aValue.HasValue
            Dim srch As List(Of dxfEntity) = MyBase.ToList()

            Dim ret As New List(Of dxfEntity)
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = srch.FindAll(Function(mem) mem.EntityType = aEntityType)
            End If
            Dim srchvals() As String
            If String.IsNullOrWhiteSpace(aDelimitor) Then
                srchvals = New String() {aTag}
            Else
                srchvals = aTag.Split(aDelimitor)
            End If
            For Each tag As String In srchvals
                If Not bContainsString Then
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) = 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) = 0))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) < 0 And String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) < 0))
                        End If
                    End If
                Else
                    Dim comp As StringComparison = StringComparison.OrdinalIgnoreCase
                    If Not bIgnoreCase Then comp = StringComparison.Ordinal
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) > -1))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) > -1 And mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(aFlag, comparisonType:=comp) > -1))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) < 0 And mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(aFlag, comparisonType:=comp) < 0))
                        End If
                    End If
                End If
            Next
            If bTestVal Then
                ret = ret.FindAll(Function(mem) mem.Value = aValue.Value)
            End If
            If ret.Count > 0 Then
                If bReturnJustOne Then
                    _rVal.Add(Item(IndexOf(ret.Item(0))), bAddClone:=bReturnClones)
                Else
                    _rVal.CollectionObj = ret
                End If
            Else
                Return _rVal
            End If
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns the entities that match the passed search criteria
        ''' </summary>
        ''' <param name="aTagList">the list of tags to search for</param>
        ''' <param name="aFlagList">an optional list of flags to include in the search criteria</param>
        ''' <param name="bReturnClones">flag to return clones of the matches</param>
        ''' <param name="bRemove">flag to to remove the match from the collection</param>
        ''' <param name="bIgnoreCase">flag to conduct the search without regard to case</param>
        ''' <returns></returns>
        Public Function GetByTags(aTagList As List(Of String), Optional aFlagList As List(Of String) = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bIgnoreCase As Boolean = True) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)

            If aTagList Is Nothing Or aTagList.Count <= 0 Then Return _rVal

            Dim matches As New List(Of dxfEntity)
            For Each ent As dxfEntity In Me

                If aTagList.FindIndex(Function(x) String.Compare(x, ent.Tag, ignoreCase:=bIgnoreCase)) >= 0 Then
                    If aFlagList Is Nothing Then
                        If Not bReturnClones Then _rVal.Add(ent) Else _rVal.Add(ent.Clone())
                        matches.Add(ent)
                    Else
                        If aFlagList.FindIndex(Function(x) String.Compare(x, ent.Flag, ignoreCase:=bIgnoreCase)) >= 0 Then
                            If Not bReturnClones Then _rVal.Add(ent) Else _rVal.Add(ent.Clone())
                            matches.Add(ent)

                        End If
                    End If
                End If

            Next


            If bRemove And matches.Count > 0 Then
                If RemoveMembersV(matches, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, matches, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByType(aType As dxxEntityTypes, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aType = dxxGraphicTypes.Undefined Then Return _rVal
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If Not bReturnInverse Then
                    If aMem.EntityType = aType Then _rVal.Add(aMem, bAddClone:=bReturnClones)
                Else
                    If aMem.EntityType <> aType Then _rVal.Add(aMem, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByValue(aValue As Double, Optional aTag As String = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aPrecis As Integer = 6, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the value to search for
            '#2an optional tag to include in the search criteria
            '#3a entity type to match
            '#4flag to stop searching when the first match is found
            '#5flag to return clones of the matches
            '#6flag to to remove the match from the collection
            '^returns all the entities that match the search criteria
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Dim aEnt As dxfEntity
            Dim aStr As String = aValue.ToString()
            Dim src As List(Of dxfEntity) = MyBase.ToList()
            If aEntityType <> dxxEntityTypes.Undefined Then
                src = (From ent In src Where ent.EntityType = aEntityType).ToList()
            End If
            If aTag IsNot Nothing Then
                src = (From ent In src Where String.Compare(ent.Tag, aTag, ignoreCase:=True) >= 0).ToList()
            End If
            If Not bReturnInverse Then

                src = (From ent In src Where Math.Round(ent.Value, aPrecis) = Math.Round(aValue, aPrecis)).ToList()
            Else
                src = (From ent In src Where Math.Round(ent.Value, aPrecis) <> Math.Round(aValue, aPrecis)).ToList()
            End If


            For Each aEnt In src
                _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
            Next
            If bRemove Then
                If RemoveMembersV(src, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetDimensionEntities(Optional aSuppressLeaders As Boolean = False, Optional aDimensionType As dxxDimTypes = dxxDimTypes.Undefined, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aMem As dxfEntity
            Dim aDim As dxeDimension
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Leader Then
                    If Not aSuppressLeaders Then _rVal.Add(aMem, bAddClone:=bReturnClones)
                ElseIf aMem.GraphicType = dxxGraphicTypes.Dimension Then
                    aDim = aMem
                    If aDimensionType < 0 Or aDim.DimType = aDimensionType Then _rVal.Add(aMem, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetEntities(aFilter As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional bOnisIn As Boolean = True, Optional aCS As dxfPlane = Nothing, Optional rIndices As List(Of Integer) = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aPrecis As Integer = 3) As colDXFEntities
            '#1search type parameter
            '#2the ordinate to search for if the search is ordinate specific
            '#3flag indicating if equal values should be returned
            '#4an optional coordinate system to use
            '#5returns the indices of the matches
            '#6flag to return copies
            '#7flag to remove the matching set
            '#8a precision for numerical comparison (1 to 8)
            '^returns the members from the collection whose CENTER match the passed search criteria
            '~same as by GetByPoints but only the center is used
            Return GetByPoints(aFilter, aOrdinate, bOnisIn, aCS, rIndices, bReturnClones, bRemove, aPrecis)
        End Function
        Public Function GetFirst(aType As dxxGraphicTypes) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.GraphicType = aType Then
                    _rVal = aEnt
                    Exit For
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetFlagged(aFlag As String, Optional aTag As String = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aOccurance As Integer = 0, Optional bReturnClone As Boolean = False, Optional bIgnoreCase As Boolean = True) As dxfEntity
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            If aFlag Is Nothing Then Return Nothing
            Dim member As dxfEntity = Nothing
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = FindAll(Function(mem) mem.EntityType = aEntityType)
            End If
            If aTag Is Nothing Then
                srch = srch.FindAll(Function(mem) String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) = 0)
            Else
                srch = srch.FindAll(Function(mem) String.Compare(mem.Tag, aTag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) = 0)
            End If
            If srch.Count <= 0 Then Return Nothing
            If aOccurance >= 1 And aOccurance > srch.Count Then Return Nothing
            member = srch.Item(0)
            If aOccurance > 1 Then member = srch.Item(aOccurance - 1)
            Return SetMemberInfo(member, bReturnClone)
        End Function
        Public Function GetHoleData(aHoleProperty As dxxHoleProperties, Optional bUniqueValues As Boolean = False, Optional aHoleType As dxxHoleTypes = dxxHoleTypes.Undefined, Optional aExtrusionDirection As dxfDirection = Nothing) As List(Of Object)
            Dim _rVal As New List(Of Object)
            If aHoleProperty < 1 Or aHoleProperty > 9 Then Return _rVal
            Dim aMem As dxfEntity
            Dim aHole As dxeHole

            Dim aVal As Object
            Dim bKeep As Boolean
            Dim tDir As TVECTOR
            Dim bTestDir As Boolean
            Dim pidx As Integer
            pidx = dxfGlobals.CommonProps + aHoleProperty
            If aExtrusionDirection IsNot Nothing Then
                tDir = aExtrusionDirection.Strukture
                bTestDir = True
            End If
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Hole Then
                    aHole = aMem

                    aVal = aHole.ActiveProperties().Value(pidx)
                    bKeep = aHoleType = dxxHoleTypes.Undefined Or (aHoleType <> dxxHoleTypes.Undefined And aHole.HoleType = aHoleType)
                    If bKeep And bTestDir Then
                        If Not tDir.Equals(aHole.DefPts.Plane.ZDirection, True, 3) Then bKeep = False
                    End If
                    If bKeep And bUniqueValues Then
                        For j As Integer = 1 To _rVal.Count
                            If _rVal.Item(j) = aVal Then
                                bKeep = False
                                Exit For
                            End If
                        Next j
                    End If
                    If bKeep Then _rVal.Add(aVal)
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetHoleSets(Optional aExtrusionDirection As dxfDirection = Nothing, Optional aHoleType As dxxHoleTypes = dxxHoleTypes.Undefined, Optional bReturnClones As Boolean = False) As List(Of colDXFEntities)
            Dim _rVal As New List(Of colDXFEntities)
            '#1a extrusion to filter the return with
            '#2a hole type to filter the return with
            '^returns a collection of hole collections separated by type and size
            '~the comparison is based on type, width and height and does not include depth.
            Dim aSet As colDXFEntities
            Dim bSet As colDXFEntities
            Dim cSet As colDXFEntities
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim cnt As Integer
            Dim aTypes() As Integer
            Dim aType As dxxHoleTypes
            Dim aData As List(Of Object)
            Dim bData As List(Of Object)
            If aHoleType = dxxHoleTypes.Undefined Then
                ReDim aTypes(0 To 3)
                aTypes(0) = dxxHoleTypes.Round
                aTypes(1) = dxxHoleTypes.Square
                aTypes(2) = dxxHoleTypes.RoundSlot
                aTypes(3) = dxxHoleTypes.SquareSlot
                cnt = 3
            Else
                ReDim aTypes(0)
                aTypes(0) = aHoleType
            End If
            For i = 0 To cnt
                aType = aTypes(i)
                aSet = GetHoles(aType, aExtrusionDirection:=aExtrusionDirection, bReturnClones:=bReturnClones)
                If aSet.Count > 0 Then
                    aData = aSet.GetHoleData(dxxHoleProperties.Radius, True)
                    For j = 1 To aData.Count
                        bSet = aSet.GetHoles(aType, dxxHoleProperties.Radius, aData.Item(j - 1))
                        If aType = dxxHoleTypes.Round Or aType = dxxHoleTypes.Square Then
                            If bSet.Count > 0 Then _rVal.Add(bSet)
                        Else
                            bData = bSet.GetHoleData(dxxHoleProperties.Length, True)
                            For k = 1 To bData.Count
                                cSet = bSet.GetHoles(, dxxHoleProperties.Length, bData.Item(k - 1))
                                If cSet.Count > 0 Then _rVal.Add(cSet)
                            Next k
                        End If
                    Next j
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetHoles(Optional aHoleType As dxxHoleTypes = dxxHoleTypes.Undefined, Optional aPropertyType As dxxHoleProperties = dxxHoleProperties.PropUndefined, Optional aPropValue As Object = Nothing, Optional aExtrusionDirection As dxfDirection = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aPropertyType < 0 Or aPropertyType > 9 Then aPropertyType = dxxHoleProperties.PropUndefined
            If aPropertyType <> dxxHoleProperties.PropUndefined Then
                If aPropValue Is Nothing Then Return _rVal
            End If
            Dim aMem As dxfEntity
            Dim aHole As dxeHole
            Dim bKeep As Boolean
            Dim bTestDir As Boolean
            Dim tDir As TVECTOR
            Dim pidx As Integer
            pidx = dxfGlobals.CommonProps + aPropertyType
            If aExtrusionDirection IsNot Nothing Then
                tDir = aExtrusionDirection.Strukture
                bTestDir = True
            End If

            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Hole Then
                    aHole = aMem

                    bKeep = aHoleType = dxxHoleTypes.Undefined Or (aHoleType <> dxxHoleTypes.Undefined And aHole.HoleType = aHoleType)
                    If bKeep And aPropertyType <> dxxHoleProperties.PropUndefined Then

                        If aHole.ActiveProperties().Value(pidx) <> aPropValue Then bKeep = False
                    End If
                    If bKeep And bTestDir Then
                        If Not tDir.Equals(aHole.DefPts.Plane.ZDirection, True, 3) Then bKeep = False
                    End If
                    If bKeep Then _rVal.Add(aHole, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetLast(aType As dxxGraphicTypes) As dxfEntity
            Dim aEnt As dxfEntity
            For i As Integer = Count To 1 Step -1
                aEnt = Item(i)
                If aEnt.GraphicType = aType Then
                    Return aEnt
                End If
            Next i
            Return Nothing
        End Function
        Public Function GetOrdinate(aSearchParam As dxxOrdinateTypes, Optional aDefPointType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPlane As dxfPlane = Nothing) As Double
            Dim _rVal As Double = 0.0
            '#1parameter controling the value returned
            '#2the entity definition point to use for the comparison
            '#3an optional plane to define the member ordinates against
            '^returns the requested ordinate based on the search parameter and the members of the current collection
            Dim aMem As dxfEntity
            Dim P1 As dxfVector
            Dim v1 As TVECTOR
            Dim vMax As TVECTOR
            Dim vMin As TVECTOR
            Dim bPln As Boolean
            Dim imaxX As Integer
            Dim iminX As Integer
            Dim imaxY As Integer
            Dim iminY As Integer
            Dim imaxZ As Integer
            Dim iminZ As Integer
            Dim aPl As New TPLANE(aPlane, bPln)
            vMax = New TVECTOR(-3.6E+36, -3.6E+36, -3.6E+36)
            vMin = New TVECTOR(3.6E+36, 3.6E+36, 3.6E+36)
            For i As Integer = 1 To Count
                aMem = Item(i)
                P1 = aMem.DefinitionPoint(aDefPointType)
                If P1 IsNot Nothing Then
                    v1 = P1.Strukture
                    If bPln Then v1 = v1.WithRespectTo(aPl)
                    If v1.X > vMax.X Then
                        imaxX = i
                        vMax.X = v1.X
                    End If
                    If v1.X < vMin.X Then
                        iminX = i
                        vMin.X = v1.X
                    End If
                    If v1.Y > vMax.Y Then
                        imaxY = i
                        vMax.Y = v1.Y
                    End If
                    If v1.Y < vMin.Y Then
                        iminY = i
                        vMin.Y = v1.Y
                    End If
                    If v1.Z > vMax.Z Then
                        imaxZ = i
                        vMax.Z = v1.Z
                    End If
                    If v1.Z < vMin.Z Then
                        iminZ = i
                        vMin.Z = v1.Z
                    End If
                End If
            Next i
            Select Case aSearchParam
                Case dxxOrdinateTypes.MinX
                    If iminX > 0 Then
                        _rVal = vMin.X
                    End If
                Case dxxOrdinateTypes.MinY
                    If iminY > 0 Then
                        _rVal = vMin.Y
                    End If
                Case dxxOrdinateTypes.MinZ
                    If iminZ > 0 Then
                        _rVal = vMin.Z
                    End If
                Case dxxOrdinateTypes.MaxX
                    If imaxX > 0 Then
                        _rVal = vMax.X
                    End If
                Case dxxOrdinateTypes.MaxY
                    If imaxY > 0 Then
                        _rVal = vMax.Y
                    End If
                Case dxxOrdinateTypes.MaxZ
                    If imaxZ > 0 Then
                        _rVal = vMax.Z
                    End If
                Case dxxOrdinateTypes.MidX
                    If imaxX > 0 And iminX > 0 Then
                        _rVal = vMin.X + (vMax.X - vMin.X) / 2
                    End If
                Case dxxOrdinateTypes.MidY
                    If imaxY > 0 And iminY > 0 Then
                        _rVal = vMin.Y + (vMax.Y - vMin.Y) / 2
                    End If
                Case dxxOrdinateTypes.MidZ
                    If imaxZ > 0 And iminZ > 0 Then
                        _rVal = vMin.Z + (vMax.Z - vMin.Z) / 2
                    End If
            End Select
            Return _rVal
        End Function
        Public Function GetParallelLines(aBaseLine As dxeLine, Optional aPrecis As Integer = 3, Optional bMustBeDirectionEqual As Boolean = False, Optional bReturnClones As Boolean = False) As colDXFEntities
            '#1the line to compare to
            '#2the precision for the comparison
            '#3flag to only return lines that are parrallel and have the same direction as the base line
            '#4flag to return clones rather than the actual members
            '^returns the lines from the collection that are parrallel to the passed base line
            Return dxfUtils.Lines_Parallel(Me, aBaseLine, aPrecis, bMustBeDirectionEqual, bReturnClones)
        End Function
        Public Function GetRightOf(ByRef XOrd As Double, Optional IncludeCrossers As Boolean = True, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aCS As dxfPlane = Nothing, Optional bReturnInverse As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the Y ordinate to test
            '#2flag to include the entities that cross this Y ordinate but are not complete above of it
            '^returns the entities that are completely above of the passed Y ordinate
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aEnt As dxfEntity
            Dim x1 As Double
            Dim x2 As Double
            Dim x3 As Double
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim aPl As New TPLANE("")
            Dim bKeep As Boolean
            If aCS IsNot Nothing Then aPl = New TPLANE(aCS)
            x1 = Math.Round(XOrd, 6)
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    ep = aEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt).Strukture.WithRespectTo(aPl)
                    sp = aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt).Strukture.WithRespectTo(aPl)
                    bKeep = False
                    x3 = Math.Round(ep.Y, 6)
                    x2 = Math.Round(sp.Y, 6)
                    If Not bReturnInverse Then
                        If x2 >= x1 And x3 >= x1 Then bKeep = True
                    Else
                        If x2 <= x1 And x3 <= x1 Then bKeep = True
                    End If
                    If IncludeCrossers And Not bKeep Then
                        If (x2 >= x1 And x3 <= x1) Or (x2 <= x1 And x3 >= x1) Then bKeep = True
                    End If
                    If bKeep Then _rVal.Add(aEnt, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function GetSubEntities(Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bConvertAttDefsToAttribs As Boolean = False, Optional aOwnerGUID As String = Nothing, Optional aImageGUID As String = "") As TENTITIES
            Dim _rVal As New TENTITIES(OwnerGUID, ImageGUID)
            Dim aMem As dxfEntity
            Dim sEnt As TENTITY
            Dim aTxt As dxeText
            If aOwnerGUID IsNot Nothing Then _rVal.OwnerGUID = aOwnerGUID.Trim()
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aGraphicType <> dxxGraphicTypes.Undefined And aMem.GraphicType <> aGraphicType Then Continue For
                If aEntityType <> dxxEntityTypes.Undefined And aMem.EntityType <> aEntityType Then Continue For
                If bConvertAttDefsToAttribs And aMem.GraphicType = dxxGraphicTypes.Text Then
                    aTxt = DirectCast(aMem, dxeText)
                    If aTxt.TextType = dxxTextTypes.AttDef Then
                        aTxt = aTxt.Clone(Nothing, aNewTextType:=dxxTextTypes.Attribute)
                        aMem = aTxt
                    End If
                End If

                sEnt = aMem.GetStructure()
                sEnt.OwnerGUID = _rVal.OwnerGUID
                _rVal.Add(sEnt)
            Next i
            If aImageGUID IsNot Nothing Then _rVal.ImageGUID = aImageGUID.Trim()
            Return _rVal
        End Function
        Friend Function GetSymsAndDims() As colDXFEntities
            Dim _rVal As New colDXFEntities
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Symbol Or aEnt.GraphicType = dxxGraphicTypes.Dimension Then
                    _rVal.Add(aEnt)
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetTagged(aTag As String, Optional aFlag As String = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aOccurance As Integer = 0, Optional bReturnClone As Boolean = False, Optional bContainsString As Boolean = False, Optional bIgnoreCase As Boolean = True) As dxfEntity
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            If aTag Is Nothing Then Return Nothing
            Dim member As dxfEntity = Nothing
            Dim srch As List(Of dxfEntity) = MyBase.ToList()
            If aEntityType <> dxxEntityTypes.Undefined Then
                srch = FindAll(Function(mem) mem.EntityType = aEntityType)
            End If
            If Not bContainsString Then
                If aFlag Is Nothing Then
                    srch = srch.FindAll(Function(mem) String.Compare(mem.Tag, aTag, ignoreCase:=bIgnoreCase) = 0)
                Else
                    srch = srch.FindAll(Function(mem) String.Compare(mem.Tag, aTag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) = 0)
                End If
            Else
                Dim comp As StringComparison = StringComparison.OrdinalIgnoreCase
                If Not bIgnoreCase Then comp = StringComparison.Ordinal
                If aFlag Is Nothing Then
                    srch = srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aTag, comparisonType:=comp) > -1)
                Else
                    srch = srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aTag, comparisonType:=comp) > -1 And mem.Flag IsNot Nothing AndAlso mem.Tag.IndexOf(aFlag, comparisonType:=comp) > -1)
                End If
            End If
            If srch.Count <= 0 Then Return Nothing
            If aOccurance >= 1 And aOccurance > srch.Count Then Return Nothing
            member = srch.Item(0)
            If aOccurance > 1 Then member = srch.Item(aOccurance - 1)
            Return SetMemberInfo(member, bReturnClone)
        End Function
        Private Function SetMemberInfo(aMember As dxfEntity, Optional bReturnClone As Boolean = False) As dxfEntity
            If aMember Is Nothing Then Return Nothing
            If MonitorMembers Then
                aMember.SetCollection(Me, False)
            End If
            'If _Suppressed Then aMember.Suppressed = True
            'If ImageGUID <> "" Then aMember.ImageGUID = ImageGUID
            'If OwnerGUID <> "" Then aMember.OwnerGUID = OwnerGUID
            'If BlockGUID <> "" Then aMember.BlockGUID = BlockGUID
            If MaintainIndices Then aMember.Index = MyBase.IndexOf(aMember) + 1
            'aMember.CollectionGUID = CollectionGUID
            If bReturnClone Then Return aMember.Clone Else Return aMember
        End Function
        Friend Sub SetGUIDS(aImageGUID As String, aOwnerGUID As String, aBlockGUID As String, aOwnerType As dxxFileObjectTypes, Optional aOwner As dxfHandleOwner = Nothing, Optional aBlock As dxfBlock = Nothing, Optional aImage As dxfImage = Nothing)
            ImageGUID = aImageGUID : OwnerGUID = aOwnerGUID : BlockGUID = aBlockGUID : OwnerType = aOwnerType
            If aOwner IsNot Nothing Then
                OwnerType = aOwner.FileObjectType
                OwnerGUID = aOwner.GUID
                _OwnerPointer = New WeakReference(Of dxfHandleOwner)(aOwner)
            End If
            If aBlock IsNot Nothing Then
                BlockGUID = aBlock.GUID
                _BlockPtr = New WeakReference(Of dxfBlock)(aBlock)
            End If
            If aImage IsNot Nothing Then
                ImageGUID = aImage.GUID
                _ImagePtr = New WeakReference(Of dxfImage)(aImage)
            ElseIf Not String.IsNullOrWhiteSpace(ImageGUID) And Not HasReferenceTo_Image Then
                Dim img As dxfImage = dxfEvents.GetImage(ImageGUID)
                SetImage(img, False)
            End If
            If aBlock IsNot Nothing Or aImage IsNot Nothing Then
                If Not MonitorMembers Then
                    MonitorMembers = True
                    GetImage(Nothing)
                End If
            End If
        End Sub


        Public Function GetTextEntities(Optional bIncludeMText As Boolean = False, Optional bReturnClones As Boolean = False, Optional aTextType As dxxTextTypes = dxxTextTypes.Undefined) As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}


            For i As Integer = 1 To Count
                Dim aEnt As dxfEntity = Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Text Or aEnt.GraphicType = dxxGraphicTypes.MText Then
                    Dim aText As dxeText = DirectCast(aEnt, dxeText)
                    If aTextType = dxxTextTypes.Undefined Then
                        If bIncludeMText Then
                            _rVal.Add(aText, bAddClone:=bReturnClones)
                        Else
                            If aText.TextType <> dxxTextTypes.Multiline Then _rVal.Add(aText, bAddClone:=bReturnClones)
                        End If
                    Else
                        If aText.TextType = aTextType Then _rVal.Add(aText, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function Graphic(aGraphicType As dxxGraphicTypes, aIndex As Integer, Optional rIndex As Integer? = Nothing, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '#1the graphic type to search for
            '#2the position of the desired entity with respect to it's entity type in the collection.
            '#3returns the collection index of the retirn entity
            '#4flag to return a copy of the desired member
            '#5flag to return remove the desired member
            '^retrieves and returns the entity in the collection of the passed graphic type
            '^whose index in the subset of members with the passed graphic type matches the passed index.
            '~i.e. Graphic(3,dxxGraphicTypes.Line) returns the third line in the collection.
            '~a negative index causes the search to be performed from the end of the collection.
            Dim cnt As Integer = Count
            Dim si As Integer = 1
            Dim ei As Integer = cnt
            Dim stp As Integer = 1
            Dim i As Integer
            Dim aMem As dxfEntity
            Dim ridx As Integer = 0
            If rIndex IsNot Nothing Then rIndex = 0
            If aIndex = 0 Or Math.Abs(aIndex) > cnt Or cnt = 0 Then Return _rVal
            If aIndex < 0 Then
                si = cnt
                ei = 1
                stp = -1
                aIndex = Math.Abs(aIndex)
            End If
            For i = si To ei Step stp
                aMem = Item(i)
                If aMem.GraphicType = aGraphicType Then
                    cnt += 1
                    If cnt = aIndex Then
                        _rVal = aMem
                        ridx = i
                        Exit For
                    End If
                End If
            Next i
            If ridx > 0 Then
                If bReturnClone Then _rVal = _rVal.Clone
                If bRemove Then
                    RemoveV(ridx, True)
                    If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, _rVal, False, False, True)
                End If
            End If
            If rIndex IsNot Nothing Then rIndex = ridx
            Return _rVal
        End Function
        Public Function GraphicTypeCount(aGraphicType As dxxGraphicTypes, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False) As Integer
            If aGraphicType = dxxGraphicTypes.Undefined Then Return 0
            Dim rMembers As List(Of dxfEntity) = FindAll(Function(mem) mem.GraphicType = aGraphicType)
            If Not bReturnInverse Then Return Count Else Return Count - bReturnInverse
        End Function
        Public Function HandlePts(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnClones As Boolean = False, Optional aSearchTag As String = Nothing, Optional aSuppressedVal As Boolean? = Nothing) As colDXFVectors
            '#1a entity type filter
            '#2flag to return copies of the points
            '#3a tag to apply to the search
            '^returns the handle points of all the entities in the collection
            Return DefinitionPoints(dxxEntDefPointTypes.HandlePt, aEntityType, aSearchTag, bReturnClones:=bReturnClones, aSuppressedVal:=aSuppressedVal)
        End Function
        Public Function HaveWidth() As Boolean
            Dim _rVal As Boolean = False
            Dim aEnt As dxfEntity
            Dim aL As dxeLine
            Dim aA As dxeArc
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Line Or aEnt.GraphicType = dxxGraphicTypes.Arc Then
                    If aEnt.EntityType = dxxEntityTypes.Line Then
                        aL = aEnt
                        If aL.StartWidth > 0 Or aL.EndWidth > 0 Then
                            _rVal = True
                            Exit For
                        End If
                    Else
                        aA = aEnt
                        If aA.StartWidth > 0 Or aA.EndWidth > 0 Then
                            _rVal = True
                            Exit For
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function Highest(Optional aCS As dxfPlane = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.Center) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            If Count <= 0 Then Return _rVal
            Dim aEnt As dxfEntity
            Dim comp As Double
            Dim aPl As New TPLANE("")
            Dim v1 As TVECTOR
            Dim idx As Integer
            If aCS IsNot Nothing Then aPl = New TPLANE(aCS)
            comp = -2.6E+30
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    v1 = aEnt.DefinitionPoint(aDefPtType).Strukture
                    If aCS IsNot Nothing Then v1 = v1.WithRespectTo(aPl)
                    If v1.Y > comp Then
                        comp = v1.Y
                        idx = i
                    End If
                End If
            Next i
            If idx > 0 Then _rVal = Item(idx)
            Return _rVal
        End Function
        Public Function Hole(aIndex As Integer) As dxeHole
            Dim _rVal As dxeHole = Nothing
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            Dim aMem As dxfEntity
            Dim cnt As Integer
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Hole Then
                    cnt += 1
                    If cnt = aIndex Then
                        _rVal = aMem
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function HoleItem(aIndex As Integer, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxeHole
            Dim _rVal As dxeHole = Nothing
            Dim aEnt As dxfEntity
            Dim cnt As Integer
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If (aEnt.GraphicType = dxxGraphicTypes.Hole) Then
                    cnt += 1
                    If cnt = aIndex Then
                        _rVal = aEnt
                        Exit For
                    End If
                End If
            Next i
            If _rVal IsNot Nothing Then
                If bReturnClone Then
                    _rVal = _rVal.Clone
                Else
                    If bRemove Then RemoveMember(_rVal)
                End If
            End If
            Return _rVal
        End Function
        Public Function HorizontalLines(Optional aTag As String = "", Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 4) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1a tag to filter for
            '^returns only the horizontal lines in the collection
            Dim aEnt As dxfEntity
            Dim aLine As dxeLine
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Line Then
                    aLine = aEnt
                    If aLine.IsHorizontal(aCS, aPrecis) Then
                        If aTag = "" Or (aTag <> "" And String.Compare(aLine.Tag, aTag, ignoreCase:=True) = 0) Then
                            _rVal.Add(aLine)
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Friend Function Identified(aIdentifier As String, Optional bGetClone As Boolean = False, Optional bRemove As Boolean = False, Optional aImage As dxfImage = Nothing, Optional rIndex As Integer? = Nothing) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '#1the identifier to search for
            '#2flag to return a copy
            '#3flag to include any member whose identifier string contains the passed search strings instead of a full string match
            '^returns all the entities that match the search criteria
            Dim aMem As dxfEntity
            Dim rid As Integer = -1
            For i As Integer = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.Identifier, aIdentifier, ignoreCase:=True) = 0 Then
                    rid = i
                    _rVal = aMem
                    Exit For
                End If
            Next i
            If bRemove And rid > 0 Then RemoveV(rid, True)
            If bGetClone And rid > 0 Then
                If aImage IsNot Nothing Then
                    _rVal = _rVal.CloneAll(aImage, False)
                Else
                    _rVal = _rVal.Clone
                End If
            End If
            If rIndex IsNot Nothing Then rIndex = rid
            Return _rVal
        End Function
        Friend Function IdentifierCount(aIdentifier As String) As Integer
            Dim _rVal As Integer = 0
            '#1the identifier to search for
            '#2flag to include any member whose identifier string contains the passed search strings instead of a full string match
            '^returns all the entities that match the search criteria
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If String.Compare(aEnt.Identifier, aIdentifier, ignoreCase:=True) = 0 Then _rVal += 1
            Next i
            Return _rVal
        End Function
        Public Function Identifiers(Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional bUnique As Boolean = False, Optional bGetNulls As Boolean = False, Optional aDelimiter As String = ",") As String
            Dim _rVal As String = String.Empty
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aGraphicType = dxxGraphicTypes.Undefined Or aMem.GraphicType = aGraphicType Then
                    TLISTS.Add(_rVal, aMem.Identifier, bAllowDuplicates:=Not bUnique, aDelimitor:=aDelimiter, bAllowNulls:=bGetNulls)
                End If
            Next i
            Return _rVal
        End Function
        Public Overloads Function IndexOf(aMember As dxfEntity) As Integer
            If aMember Is Nothing Or Count <= 0 Then Return 0
            Return MyBase.IndexOf(aMember) + 1
        End Function
        Public Overloads Function Insert(aNewEnts As IEnumerable(Of dxfEntity), Optional aAfterIndex As Integer = 0, Optional aBeforeIndex As Integer = 0, Optional bAddClones As Boolean = False, Optional bReverseOrder As Boolean = False) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            If aNewEnts Is Nothing Then Return _rVal
            If aNewEnts.Count <= 0 Then Return _rVal
            '#1a collection of entities to insert into this collection
            '#2The index position to insert the passed entities after
            '#3The index position to insert the passed entities before
            '^used to insert many entities into the collection at the requested index.
            '~returns the last inserted entity.
            Dim i As Integer
            Dim aEnt As dxfEntity
            Dim idx As Integer
            Dim si As Integer
            Dim ei As Integer
            Dim aStep As Integer
            Dim bSA As Boolean
            Dim newmems As List(Of dxfEntity) = aNewEnts.ToList()
            Dim cnt As Integer = Count
            bSA = ImageGUID <> "" Or BlockGUID <> ""
            If aBeforeIndex < 1 Then aBeforeIndex = 0
            If aAfterIndex < 1 Then aAfterIndex = 0
            If aBeforeIndex > cnt Then
                aBeforeIndex = 0
                aAfterIndex = 0
            End If
            If aAfterIndex >= cnt Then
                aBeforeIndex = 0
                aAfterIndex = 0
            End If
            If aAfterIndex = 0 And aBeforeIndex = 0 Then
                Append(aNewEnts, False, bReverseOrder:=bReverseOrder)
                _rVal = LastMember()
            Else
                dxfUtils.LoopIndices(aNewEnts.Count, 1, aNewEnts.Count, si, ei, bReverseOrder, aStep)
                If aAfterIndex > 0 Then
                    idx = aAfterIndex
                    For i = si To ei Step aStep
                        aEnt = newmems(i - 1)
                        _rVal = AddToCollection(aEnt, aAfterIndex:=idx, bSuppressReindex:=True, bSuppressEvnts:=True)
                        idx += 1
                    Next i
                Else
                    idx = aBeforeIndex
                    For i = si To ei Step aStep
                        aEnt = newmems(i - 1)
                        _rVal = AddToCollection(aEnt, aBeforeIndex:=idx, bSuppressReindex:=True, bSuppressEvnts:=Not bSA, aAddClone:=bAddClones)
                    Next i
                End If
                If MaintainIndices Then ReIndex()
                If aNewEnts.Count > 0 And Not SuppressEvents And Not bSA Then
                    RaiseEntitiesChange(dxxCollectionEventTypes.Append, aNewEnts, False, False, True)
                End If
            End If
            Return _rVal
        End Function

        Public Function Inserts(Optional aBlockNames As String = "", Optional bWithAttributesOnly As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If String.IsNullOrWhiteSpace(aBlockNames) Then Return _rVal
            aBlockNames = aBlockNames.Trim()

            '#1a list of block names to filter the return (null string = all inserts)
            '#2flag to only reutn an insert if ot has attributes
            '#3flag to return copies
            '#4flag to remove the returned set from the current collection
            '^returns all the block reference entities (inserts) from the current collection
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aEnt As dxfEntity
            Dim aIns As dxeInsert
            Dim bKeep As Boolean

            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.EntityType = dxxEntityTypes.Insert Then
                    aIns = aEnt
                    bKeep = TLISTS.Contains(aIns.BlockName, aBlockNames, bReturnTrueForNullList:=True)
                    If bWithAttributesOnly Then
                        bKeep = aIns.Attributes.Count > 0
                    End If
                    If bKeep Then _rVal.Add(aIns, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function InsertsByBlockGUID(aGUID As String, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the guid to search for
            '#2flag to return copies
            '#3flag to remove the returned set from the current collection
            '^returns all the block reference entities (inserts) from the current collection
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aEnt As dxfEntity
            Dim aIns As dxeInsert
            aGUID = Trim(aGUID)
            If aGUID = "" Then Return _rVal
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.EntityType = dxxEntityTypes.Insert Then
                    aIns = aEnt
                    If String.Compare(aIns.BlockGUID, aGUID, ignoreCase:=True) Then
                        _rVal.Add(aIns, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function Intersections(aEntities As IEnumerable(Of dxfEntity), Optional bTheseAreInfinite As Boolean = False, Optional bCurvesAreInfinite As Boolean = False, Optional bNoDoubles As Boolean = False, Optional bReturnEndPts As Boolean = False, Optional bMustBeOnBoth As Boolean = True) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aEntities Is Nothing Or Count <= 0 Then Return _rVal
            dxfIntersections.Points(Me, aEntities, bTheseAreInfinite, bCurvesAreInfinite, bMustBeOnBoth, aCollector:=_rVal)
            If _rVal.Count = 0 Then Return _rVal
            If Not bReturnEndPts Then
                Dim v1 As TVECTOR
                Dim l1 As TLINE
                Dim bKeep As Boolean
                Dim d1 As Double
                Dim aEnt As dxfEntity
                For i As Integer = _rVal.Count To 1 Step -1
                    v1 = _rVal.ItemVector(i)
                    bKeep = True
                    For j As Integer = 1 To Count
                        aEnt = Item(j)
                        l1 = New TLINE(aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt), aEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt))
                        d1 = v1.DistanceTo(l1.SPT)
                        If d1 < 0.001 Then
                            bKeep = False
                            Exit For
                        End If
                        d1 = v1.DistanceTo(l1.EPT)
                        If d1 < 0.001 Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                    If Not bKeep Then
                        _rVal.Remove(i)
                    End If
                Next i
            End If
            If bNoDoubles Then _rVal.RemoveCoincidentVectors()
            Return _rVal
        End Function
        Public Shadows Function Item(aHandleOrNameOrGUID As String, Optional bReturnClone As Boolean = False) As dxfEntity
            '#1 the index, handle or name of the desired entity
            '^returns the requested entity
            If Count <= 0 Then Return Nothing
            If String.IsNullOrWhiteSpace(aHandleOrNameOrGUID) Then Return Nothing
            Dim _rVal As dxfEntity = Find(Function(mem) mem.Handle = aHandleOrNameOrGUID Or GUID = aHandleOrNameOrGUID Or mem.Name.ToUpper = aHandleOrNameOrGUID.ToUpper)
            If _rVal Is Nothing Then Return Nothing
            Return SetMemberInfo(_rVal, bReturnClone)
        End Function
        Public Shadows Function Item(aIndex As Integer, bReturnClone As Boolean) As dxfEntity
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            '#1 the index, handle or name of the desired entity
            '^returns the requested entity
            Return SetMemberInfo(MyBase.Item(aIndex - 1), bReturnClone)
        End Function

        Public Shadows Function Item(aIndex As Integer) As dxfEntity
            '#1 the index, handle or name of the desired entity
            '^returns the requested entity
            Return Item(aIndex, False)
        End Function
        Public Sub LCLSet(Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined)
            'sets the Layer, Color and Linetype of the entity
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aGraphicType = dxxGraphicTypes.Undefined Or aMem.GraphicType = aGraphicType Then
                    If aEntityType = dxxEntityTypes.Undefined Or aMem.EntityType = aEntityType Then
                        aMem.LCLSet(aLayerName, aColor, aLineType)
                    End If
                End If
            Next i
        End Sub
        Public Function Last(aCount As Integer, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '#1the number of entities to return
            '#2flag to return copies
            '#3flag to remove the subset from the collection
            '^returns the last members of the collection up to the passed count
            '~i.e. Last(4) returns the last 4 members
            If aCount <= 0 Then Return _rVal
            If aCount > Count Then aCount = Count
            Dim aEnt As dxfEntity
            aCount = Count - aCount + 1
            If aCount <= 0 Then aCount = 1
            For i As Integer = aCount To Count
                aEnt = MyBase.Item(i - 1)
                _rVal.Add(SetMemberInfo(aEnt, bReturnClones))
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function LastEntity(Optional aType As dxxEntityTypes = dxxEntityTypes.Undefined) As dxfEntity
            '^returns the last dxfEntity added to the current collection
            '^this can be a simple entity like a line or arc or it can be a complex entity like a leader or dimension
            Dim aEnt As dxfEntity
            If aType < 1 Then
                Return LastMember()
            Else
                For i As Integer = Count To 1 Step -1
                    aEnt = Item(i)
                    If aEnt.EntityType = aType Then
                        Return aEnt
                    End If
                Next i
            End If
            Return Nothing
        End Function
        Public Function LastMember(Optional bReturnClone As Boolean = False) As dxfEntity
            '^returns the last entity in the collection
            Return Item(Count, bReturnClone)
        End Function
        Public Function LastText() As dxeText
            '^returns the last text entity in the collection
            'On Error Resume Next
            Dim aEnt As dxfEntity
            Dim aLdr As dxeLeader
            Dim eTp As dxxGraphicTypes
            Dim aTable As dxeTable
            Dim aDim As dxeDimension
            Dim aSym As dxeSymbol
            For i As Integer = Count To 1 Step -1
                aEnt = Item(i)
                eTp = aEnt.GraphicType
                Select Case eTp
                    Case dxxGraphicTypes.Text, dxxGraphicTypes.MText
                        Return aEnt
                    Case dxxGraphicTypes.Dimension
                        aDim = aEnt
                        Return aDim.TextPrimary
                    Case dxxGraphicTypes.Table
                        aTable = aEnt
                        aTable.UpdatePath()
                        Return aTable.Entities.LastText
                    Case dxxGraphicTypes.Symbol
                        aSym = aEnt
                        aSym.UpdatePath()
                        Return aSym.Entities.LastText
                    Case dxxGraphicTypes.Leader
                        aLdr = aEnt
                        If aLdr.LeaderType = dxxLeaderTypes.LeaderText Then
                            aLdr.UpdatePath()
                            Return aLdr.MText
                        End If
                End Select
                'Application.DoEvents()
            Next i
            Return Nothing
        End Function
        Public Function LayerNames(Optional aLayerToInclude As String = "") As String
            Dim _rVal As String = aLayerToInclude?.Trim
            '^all of the layer names referenced by the entities in the collection
            'On Error Resume Next
            Dim aEnt As dxfEntity
            Dim lNames As String
            For i As Integer = 1 To Count
                aEnt = Item(i)
                lNames = aEnt.LayerNames
                If lNames <> "" Then TLISTS.Append(_rVal, lNames, True, bUpdateSourceString:=True)
            Next i
            Return _rVal
        End Function
        Public Function LeftMost(Optional aCS As dxfPlane = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.Center, Optional aPrecis As Integer = 3, Optional bReturnClone As Boolean = False) As dxfEntity
            Return GetByPoint(dxxPointFilters.AtMinX, aPlane:=aCS, aPrecis:=aPrecis, bReturnClone:=bReturnClone, bRemove:=False, aEntPointType:=aDefPtType, aEntityType:=aEntityType)
        End Function
        Public Function Length() As Double
            Dim _rVal As Double = 0.0
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                _rVal += aEnt.Length
            Next i
            Return _rVal
        End Function
        Public Function LineIntersections(aLineOrLines As Object, Optional bTheseAreInfinite As Boolean = False, Optional bLineIsInfinite As Boolean = False, Optional bNoDoubles As Boolean = False, Optional bReturnEndPts As Boolean = False, Optional bMustBeOnBoth As Boolean = True, Optional aMinLength As Double = 0.000001) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aLineOrLines Is Nothing Or Count <= 0 Then Return _rVal
            Dim aLines As TLINES = LinesV(aMinLength)
            If aLines.Count <= 0 Then Return _rVal
            Dim bLines As New TLINES(0)
            Dim aEnts As colDXFEntities
            Dim aLine As dxeLine


            If String.Compare(TypeName(aLineOrLines), "dxeLine", ignoreCase:=True) = 0 Then
                aLine = aLineOrLines
                If aLine.Length <= aMinLength Then Return _rVal
                bLines.Add(New TLINE(aLine))
            ElseIf String.Compare(TypeName(aLineOrLines), "colDXFEntities", ignoreCase:=True) = 0 Then
                aEnts = aLineOrLines
                bLines = aEnts.LinesV(aMinLength)
            End If
            If bLines.Count <= 0 Then Return _rVal
            _rVal = New colDXFVectors
            aLines.IntersectionPts(bLines, bTheseAreInfinite, bLineIsInfinite, bMustBeOnBoth, aCollector:=_rVal)
            If Not bReturnEndPts And _rVal.Count > 0 Then

                Dim l1 As TLINE

                For i As Integer = _rVal.Count To 1 Step -1
                    Dim v1 As TVECTOR = _rVal.ItemVector(i)
                    Dim bKeep As Boolean = True
                    For j As Integer = 1 To aLines.Count
                        l1 = aLines.Item(j)

                        If v1.DistanceTo(l1.SPT) < 0.001 Then
                            bKeep = False
                            Exit For
                        End If

                        If v1.DistanceTo(l1.EPT) < 0.001 Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                    For j As Integer = 1 To bLines.Count
                        l1 = bLines.Item(j)

                        If v1.DistanceTo(l1.SPT) < 0.001 Then
                            bKeep = False
                            Exit For
                        End If
                        If v1.DistanceTo(l1.EPT) < 0.001 Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                    If Not bKeep Then _rVal.Remove(i)
                Next i
            End If
            If bNoDoubles Then _rVal.RemoveCoincidentVectors()
            Return _rVal
        End Function
        Public Function LineItem(aIndex As Integer, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxeLine
            Dim _rVal As dxeLine = Nothing
            Dim aEnt As dxfEntity
            Dim cnt As Integer
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Line Then
                    cnt += 1
                    If cnt = aIndex Then
                        _rVal = aEnt
                        Exit For
                    End If
                End If
            Next i
            If _rVal IsNot Nothing Then
                If bReturnClone Then
                    _rVal = _rVal.Clone
                Else
                    If bRemove Then RemoveMember(_rVal)
                End If
            End If
            Return _rVal
        End Function
        Public Function GetLines(Optional aLength As Double = -1, Optional aPrecis As Integer = 4, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxeLine)
            Dim _rVal As New List(Of dxeLine)
            '^returns only the lines
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Dim bTestLen As Boolean
            If aLength >= 0 Then
                bTestLen = True
                aLength = Math.Round(aLength, aPrecis)
            End If
            Dim ents As New List(Of dxfEntity)


            For i As Integer = 1 To Count
                Dim aMem As dxfEntity = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Line Then
                    Dim aLn As dxeLine = aMem
                    Dim keep As Boolean = True
                    If bTestLen Then
                        keep = Math.Round(aLn.Length, aPrecis) = aLength
                    End If

                    If keep Then
                        If bRemove Then ents.Add(aMem)
                        If Not bReturnClones Then _rVal.Add(aMem) Else _rVal.Add(aMem.Clone())
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(ents, True) And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function LinesV(Optional aMinLength As Double = -1) As TLINES
            Dim _rVal As New TLINES(0)
            '^returns only the lines
            Dim aMem As dxfEntity
            Dim aLn As dxeLine
            Dim lStr As TLINE
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Line Then
                    aLn = aMem
                    lStr = New TLINE(aLn)
                    If aMinLength < 0 Then
                        _rVal.Add(lStr)
                    Else
                        If lStr.SPT.DistanceTo(lStr.EPT, 5) >= aMinLength Then
                            _rVal.Add(lStr)
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function Linetypes(Optional aLTToInclude As String = "") As String
            '^all of the linetype names referenced by the entities in the collection
            'On Error Resume Next
            Dim _rVal As String = aLTToInclude?.Trim()
            Dim aEnt As dxfEntity
            Dim lNames As String
            For i As Integer = 1 To Count
                aEnt = Item(i)
                lNames = aEnt.LineTypes
                If lNames <> "" Then TLISTS.Append(_rVal, lNames, True, bUpdateSourceString:=True)
            Next i
            Return _rVal
        End Function
        Public Function StyleNames(Optional aaStyleToInclude As String = "") As String
            '^all of the style names referenced by the entities in the collection
            'On Error Resume Next
            Dim _rVal As String = aaStyleToInclude?.Trim()
            Dim aEnt As dxfEntity
            Dim lNames As String
            For i As Integer = 1 To Count
                aEnt = Item(i)
                lNames = aEnt.Style.Name
                If lNames <> "" Then TLISTS.Add(_rVal, lNames)
            Next i
            Return _rVal
        End Function

        Public Function DimStyleNames(Optional aaStyleToInclude As String = "") As String
            '^all of the Dim style names referenced by the entities in the collection
            'On Error Resume Next
            Dim _rVal As String = aaStyleToInclude?.Trim()
            Dim aEnt As dxfEntity
            Dim lNames As String
            For i As Integer = 1 To Count
                aEnt = Item(i)
                lNames = aEnt.DimStyleName
                If lNames <> "" Then TLISTS.Add(_rVal, lNames)
            Next i
            Return _rVal
        End Function

        Public Function LongestMember(Optional bGetClone As Boolean = False, Optional aType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As dxfEntity
            '^returns the INtegerest member in the collection
            Dim aEnt As dxfEntity
            Dim cval As Double
            Dim lVal As Double
            cval = 0
            Dim idx As Integer
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aType = dxxGraphicTypes.Undefined Or (aType <> dxxGraphicTypes.Undefined And aEnt.GraphicType = aType) Then
                    lVal = aEnt.Length
                    If lVal > cval Then
                        idx = i
                        cval = lVal
                    End If
                End If
            Next i
            If idx > 0 Then Return Item(idx, bGetClone) Else Return Nothing
        End Function
        Public Function LongestMembers() As colDXFEntities
            Dim _rVal As New colDXFEntities
            Dim aMax As Double = -2
            Dim aEnt As dxfEntity
            Dim lg As Double
            Dim i As Integer
            For i = 1 To Count
                aEnt = Item(i)
                lg = Math.Round(aEnt.Length, 3)
                If lg > aMax Then aMax = lg
            Next i
            For i = 1 To Count
                aEnt = Item(i)
                lg = Math.Round(aEnt.Length, 3)
                If lg = aMax Then _rVal.Add(aEnt)
            Next i
            Return _rVal
        End Function
        Public Function Lowest(Optional aCS As dxfPlane = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.Center) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            If Count <= 0 Then Return _rVal
            Dim aEnt As dxfEntity
            Dim comp As Double
            Dim aPl As New TPLANE("")
            Dim v1 As TVECTOR
            Dim idx As Integer
            If aCS IsNot Nothing Then aPl = New TPLANE(aCS)
            comp = 2.6E+30
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    v1 = aEnt.DefinitionPoint(aDefPtType).Strukture
                    If aCS IsNot Nothing Then v1 = v1.WithRespectTo(aPl)
                    If v1.Y < comp Then
                        comp = v1.Y
                        idx = i
                    End If
                End If
            Next i
            If idx > 0 Then _rVal = Item(idx)
            Return _rVal
        End Function
        Public Function MemberExists(aMember As dxfEntity) As Boolean
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt Is aMember Then
                    Return True
                End If
            Next i
            Return False
        End Function
        Public Function MidPts(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnClones As Boolean = False, Optional aSearchTag As String = Nothing, Optional aSuppressedVal As Boolean? = Nothing) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1a entity type filter
            '#2flag to return copies of the points
            '#3a tag to apply to the search
            '^returns the mid points of all the entities in the collection
            _rVal = DefinitionPoints(dxxEntDefPointTypes.MidPt, aEntityType, aSearchTag, bReturnClones, aSuppressedVal)
            Return _rVal
        End Function
        Public Function Mirror(aLineXY As iLine) As Boolean
            '#1the entity to mirror across
            '#2returns the dxeLine that was used for the mirroring
            '^mirrors the entities across the passed entity
            '~returns True if the entities actually moves from this process

            '#1the line to mirror across
            '^mirrors the entities across the passed entity
            '~returns True if the entities actually moves from this process

            Return Transform(TTRANSFORM.CreateMirror(aLineXY), aEventName:="Mirror")

        End Function


        Public Function MirrorPlanar(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean = False
            '#1the x coordinate to mirror across
            '#2the y coordinate to mirror across
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
                _rVal = Transform(aTrs, aEventName:="MirrorPlanar")
            End If
            Return _rVal
        End Function
        Public Function Move(Optional aChangeX As Double = 0.0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the members by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions

            Return Transform(TTRANSFORM.CreateTranslation(aChangeX, aChangeY, aChangeZ, aPlane), aEventName:="Move")
        End Function
        Public Function MoveFromTo(aBasePointXY As iVector, aDestinationPointXY As iVector, Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0) As Boolean
            '^used to move the members from one reference vector to another
            Return Transform(TTRANSFORM.CreateFromTo(aBasePointXY, aDestinationPointXY, aXChange, aYChange, aZChange, SuppressEvents))
        End Function
        Public Function MovePolar(aBasePoint As iVector, mAngle As Double, mDistance As Double, Optional aCS As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean = False
            '#1the base point to use as the center
            '#2the direction angle to move in
            '#3the distance to move
            '#4the coordinate system to use to determine the direction based on the angle
            '^moves the entity to a point on a plane aligned with the XY plane of the passed coordinate system and centered at the base point.
            '~if the base point is nothing passed the center of the entities coordinate system is used.
            '~if the coordinate system is not passed the world coordinate system is used.
            Dim aEnt As dxfEntity
            Dim wuz As Boolean
            For i As Integer = 1 To Count
                aEnt = Item(i)
                wuz = aEnt.SuppressEvents
                aEnt.SuppressEvents = True
                If aEnt.MovePolar(aBasePoint, mAngle, mDistance, aCS) Then _rVal = True
                aEnt.SuppressEvents = wuz
            Next i
            If _rVal And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, Nothing, False, False, False)
            Return _rVal
        End Function
        Public Function MoveTo(DestinationXY As iVector, Optional ChangeX As Double = 0.0, Optional ChangeY As Double = 0.0, Optional ChangeZ As Double = 0.0) As Boolean
            Dim _rVal As Boolean = False
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the members from its current insertion point to the passed point
            '~returns True if the members actually moves from this process
            Dim aEnt As dxfEntity
            Dim wuz As Boolean
            For i As Integer = 1 To Count
                aEnt = Item(i)
                wuz = aEnt.SuppressEvents
                aEnt.SuppressEvents = True
                If aEnt.MoveTo(DestinationXY, ChangeX, ChangeY, ChangeZ) Then _rVal = True
                aEnt.SuppressEvents = wuz
            Next i
            If _rVal And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, Nothing, False, False, False)
            Return _rVal
        End Function

        Public Function Ordinates(aOrdinateType As dxxOrdinateDescriptors, Optional aPrecis As Integer = -1, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPlane As dxfPlane = Nothing, Optional bSortLowToHigh As Boolean = False, Optional rSpan As Double? = Nothing) As List(Of Double)
            Dim _rVal As New List(Of Double)
            '^returns the unique X,Y or Z ordinates referred to by at least one of the entities in the collection
            '^used to query the collection about the ordinates of the entities in the current collection
            If rSpan IsNot Nothing Then rSpan = 0
            If Count <= 0 Then Return _rVal
            Dim aOrd As Double
            Dim bHaveIt As Boolean
            Dim P1 As dxfVector
            Dim aMem As dxfEntity
            Dim aMax As Double
            Dim aMin As Double
            Dim j As Integer
            Dim v1 As TVECTOR
            Dim bPln As Boolean
            Dim aPl As New TPLANE(aPlane, bPln)
            If aDefPtType < dxxEntDefPointTypes.StartPt Or aDefPtType > dxxEntDefPointTypes.EndPt Then aDefPtType = dxxEntDefPointTypes.HandlePt
            If aOrdinateType < dxxOrdinateDescriptors.X And aOrdinateType > dxxOrdinateDescriptors.Z Then aOrdinateType = dxxOrdinateDescriptors.X
            If aPrecis < 0 Then aPrecis = -1
            If aPrecis > -1 Then aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            aMax = Double.MinValue
            aMin = Double.MaxValue
            'initialize
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem.EntityType = aEntityType Or aEntityType = dxxEntityTypes.Undefined Then
                    P1 = aMem.DefinitionPoint(aDefPtType)
                    If P1 Is Nothing Then P1 = New dxfVector
                    v1 = P1.Strukture
                    If bPln Then v1 = v1.WithRespectTo(aPl)
                    If aOrdinateType = dxxOrdinateDescriptors.Y Then
                        aOrd = v1.Y
                    ElseIf aOrdinateType = dxxOrdinateDescriptors.Z Then
                        aOrd = v1.Z
                    Else
                        aOrd = v1.X
                    End If
                    If aPrecis > -1 Then aOrd = Math.Round(aOrd, aPrecis)
                    bHaveIt = False
                    For j = 1 To _rVal.Count
                        If aOrd = _rVal.Item(j) Then
                            bHaveIt = True
                            Exit For
                        End If
                    Next j
                    If Not bHaveIt Then
                        If aOrd > aMax Then aMax = aOrd
                        If aOrd < aMin Then aMin = aOrd
                        _rVal.Add(aOrd)
                    End If
                End If
            Next i
            _rVal.Sort()
            If Not bSortLowToHigh Then _rVal.Reverse()
            If rSpan IsNot Nothing Then rSpan = aMax - aMin
            Return _rVal
        End Function
        Public Function OrthogonalLines(Optional bReturnInverse As Boolean = False, Optional aCS As dxfPlane = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            Dim bFlag As Boolean
            Dim aEnt As dxfEntity
            Dim aL As dxeLine
            Dim xDir As TVECTOR
            Dim yDir As TVECTOR
            Dim lDir As TVECTOR
            If Not bRemove And Not bReturnClones Then _rVal.MaintainIndices = False
            If Count <= 0 Then Return _rVal
            If aCS IsNot Nothing Then
                xDir = aCS.XDirectionV
                yDir = aCS.YDirectionV
            Else
                xDir = TVECTOR.WorldX
                yDir = TVECTOR.WorldY
            End If
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Line Then
                    aL = aEnt
                    lDir = aL.StartPtV.DirectionTo(aL.EndPtV, False, bFlag)
                    If Not bFlag Then
                        bFlag = lDir.Equals(xDir, True, 3)
                        If Not bFlag Then bFlag = lDir.Equals(yDir, True, 3)
                        If (bFlag And Not bReturnInverse) Or (Not bFlag And bReturnInverse) Then
                            _rVal.Add(aL, bAddClone:=bReturnClones)
                        End If
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembersV(_rVal, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function PhantomPoints(Optional aCurveDivisions As Integer = 20, Optional aLineDivision As Integer = 1, Optional bExcludeStartPts As Boolean = False, Optional bExcludeEndPts As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            Dim aEnt As dxfEntity
            Dim ppts As colDXFVectors
            For i As Integer = 1 To Count
                aEnt = Item(i)
                ppts = dxfUtils.PhantomPoints(aEnt, aCurveDivisions, aLineDivision)
                If bExcludeEndPts And i <> Count Then ppts.Remove(ppts.Count)
                If bExcludeStartPts And i <> 1 Then ppts.Remove(1)
                _rVal.Append(ppts, False)
            Next i
            Return _rVal
        End Function
        Public Function PolylineVertices() As colDXFVectors
            Return PolylineVertices(False, aCurveDivisions:=20, ioClosed:=False)
        End Function
        Public Function PolylineVertices(bAsLines As Boolean, aCurveDivisions As Integer, ByRef ioClosed As Boolean) As colDXFVectors
            Dim _rVal As New colDXFVectors With {.MaintainIndices = False}
            Dim aL As dxeLine
            Dim aA As dxeArc
            Dim aEnt As dxfEntity
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim sVerts As colDXFVectors
            Dim d1 As Double
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.GraphicType = dxxGraphicTypes.Arc Or aEnt.GraphicType = dxxGraphicTypes.Line Then
                    sVerts = Nothing
                    If aEnt.GraphicType = dxxGraphicTypes.Arc Then
                        aA = aEnt
                        If aA.SpannedAngle > 0.08 Then
                            sVerts = aA.ConvertToPolyline(bAsLines, aCurveDivisions).Vertices.Clone
                        End If
                    Else
                        aL = aEnt
                        If aL.Length > 0.0001 Then
                            sVerts = aL.ConvertToPolyline.Vertices.Clone
                        End If
                    End If
                    If sVerts IsNot Nothing Then
                        If _rVal.Count > 0 Then
                            ep = _rVal.ItemVector(_rVal.Count)
                            sp = sVerts.ItemVector(1)
                            d1 = ep.DistanceTo(sp)
                            If d1 < 0.08 Then _rVal.Remove(_rVal.Count)
                        End If
                        _rVal.Append(sVerts, False)
                    End If
                End If
            Next i
            ep = _rVal.ItemVector(_rVal.Count)
            sp = _rVal.ItemVector(1)
            If ioClosed Then
                d1 = ep.DistanceTo(sp)
                If d1 > 0.1 Then _rVal.AddV(sp)
            Else
                If sp.Equals(ep, 3) Then
                    ioClosed = True
                    _rVal.Remove(_rVal.Count)
                End If
            End If
            _rVal.MaintainIndices = True
            Return _rVal
        End Function

        Public Function PolylinesEnclosePoint(TestPoint As Object, Optional OnBoundIsIn As Boolean = True) As Boolean
            Dim rContainer As dxfPolyline = Nothing
            Return PolylinesEnclosePoint(TestPoint, OnBoundIsIn, rContainer)
        End Function
        Public Function PolylinesEnclosePoint(TestPoint As Object, OnBoundIsIn As Boolean, ByRef rContainer As dxfPolyline) As Boolean
            Dim _rVal As Boolean = False
            rContainer = Nothing
            '#1the point to test
            '#2flag indicating that a point on a boundary is considered to be interior to a polygon
            '#3returns the polygon that is found to contain the passed point
            '^used to test if any of the member polygons or polylines enclose the passed point
            If TestPoint Is Nothing Then Return _rVal
            Dim aPline As dxfPolyline
            Dim testresult As Boolean
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.EntityType = dxxEntityTypes.Polygon Or aEnt.EntityType = dxxEntityTypes.Polyline Or aEnt.EntityType = dxxEntityTypes.Hatch Then
                    aPline = aEnt
                    testresult = aPline.EnclosesPoint(TestPoint, OnBoundIsIn)
                    If testresult Then
                        rContainer = aPline
                        _rVal = True
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function

        Public Function TransferedToPlane(aPlane As dxfPlane) As colDXFEntities
            Dim _rVal As colDXFEntities = Me.Clone()
            dxfEntities.TransferEntitiesToPlane(_rVal, aPlane)
            Return _rVal
        End Function
        Public Sub TransferToPlane(aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing)
            dxfEntities.TransferEntitiesToPlane(Me, aPlane, aFromPlane)
        End Sub

        Public Sub Populate(newEnts As IEnumerable(Of dxfEntity), Optional bAddClones As Boolean = True, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional bCopyInstances As Boolean = False)
            '#1a collection of entities to add to the current collection
            '#2flag to add clones of the passed entities
            '#3flag to return the coordinate list
            '^appends the members of the passed entities to the current collection

            Dim bSA As Boolean = Not String.IsNullOrWhiteSpace(ImageGUID) Or Not String.IsNullOrWhiteSpace(BlockGUID)
            Populate(bSA, newEnts, bAddClones, aTag, aFlag, bCopyInstances)
        End Sub

        Friend Sub Populate(bSuppressEvents As Boolean, newEnts As IEnumerable(Of dxfEntity), Optional bAddClones As Boolean = True, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional bCopyInstances As Boolean = False)
            '#1a collection of entities to add to the current collection
            '#2flag to add clones of the passed entities
            '#3flag to return the coordinate list
            '^appends the members of the passed entities to the current collection
            Dim cnt As Integer = Count
            Clear(True)
            If newEnts Is Nothing Then Return

            For Each ent In newEnts
                Dim newent As dxfEntity = AddToCollection(ent, bSuppressEvnts:=bSuppressEvents, aAddClone:=bAddClones, aTag:=aTag, aFlag:=aFlag)
                If newent IsNot Nothing And bCopyInstances Then
                    newent.Instances.Copy(ent.Instances)
                End If
            Next
            If Not SuppressEvents And Count <> cnt And Not bSuppressEvents Then
                RaiseEntitiesChange(dxxCollectionEventTypes.Populate, newEnts, False, False, True)
            End If
        End Sub
        Public Function Project(aDirectionObj As iVector, aDistance As Double) As Boolean
            If aDirectionObj Is Nothing Or Count <= 0 Then Return False
            '#1the direction to project in
            '#2the distance to project
            '^projects the entities in the passed direction the requested distance
            Return Transform(TTRANSFORM.CreateProjection(aDirectionObj, aDistance), aEventName:="Project")
        End Function

        Public Function Project(aDirection As dxfDirection, aDistance As Double) As Boolean
            If aDirection Is Nothing Or Count <= 0 Then Return False
            '#1the direction to project in
            '#2the distance to project
            '^projects the entities in the passed direction the requested distance
            Return Transform(TTRANSFORM.CreateProjection(aDirection, aDistance), aEventName:="Project")
        End Function
        Friend Function Project(aDirection As TVECTOR, aDistance As Double, Optional bSuppressEvnts As Boolean = True) As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the entities in the passed direction the requested distance
            If aDirection.IsNull() Or aDistance = 0 Then Return False

            Return Transform(TTRANSFORM.CreateProjection(aDirection, aDistance), bSuppressEvnts, "ProjectV")
        End Function
        Public Function Projected(aDirectionObj As iVector, aDistance As Double) As colDXFEntities
            Dim _rVal As New colDXFEntities(Me, True)
            If aDirectionObj Is Nothing Or Count <= 0 Then Return _rVal

            '#1the direction to project in
            '#2the distance to project
            '^returns a copy of the current collection projected in the passed direction the requested distance
            Dim aTr As TTRANSFORM = TTRANSFORM.CreateProjection(aDirectionObj, aDistance, True)
            For Each ent In Me
                Dim aMem As dxfEntity = ent.Clone()
                TTRANSFORM.Apply(aTr, aMem, True)
                _rVal.AddToCollection(aMem, bSuppressEvnts:=True)
            Next

            Return _rVal
        End Function
        Public Function Projected(aDirection As dxfDirection, aDistance As Double) As colDXFEntities
            Dim _rVal As New colDXFEntities(Me, True)
            If aDirection Is Nothing Or Count <= 0 Then Return _rVal

            '#1the direction to project in
            '#2the distance to project
            '^returns a copy of the current collection projected in the passed direction the requested distance
            Dim aTr As TTRANSFORM = TTRANSFORM.CreateProjection(aDirection, aDistance, True)
            For Each ent In Me
                Dim aMem As dxfEntity = ent.Clone()
                TTRANSFORM.Apply(aTr, aMem, True)
                _rVal.AddToCollection(aMem, bSuppressEvnts:=True)
            Next

            Return _rVal
        End Function
        Public Function ProjectedFromTo(aFromPtXY As iVector, aToPtXY As iVector) As colDXFEntities
            '#1the base point of the projection
            '#2the destination point of the projected
            '#3returns the computed direction
            '#retrns the computed distance
            '^returns a copy of the current collection projected in the direction from the passed from Point to passed destination point.
            Dim rDistance As Double = 0
            Dim rDirection As New dxfDirection With {.Strukture = New TVECTOR(aFromPtXY).DirectionTo(New TVECTOR(aToPtXY), False, rDistance)}
            Return Projected(rDirection, rDistance)
        End Function

        Public Function ProjectedFromTo(aFromPtXY As iVector, aToPtXY As iVector, ByRef rDirection As dxfDirection, ByRef rDistance As Double) As colDXFEntities
            '#1the base point of the projection
            '#2the destination point of the projected
            '#3returns the computed direction
            '#retrns the computed distance
            '^returns a copy of the current collection projected in the direction from the passed from Point to passed destination point.
            rDirection = New dxfDirection With {.Strukture = New TVECTOR(aFromPtXY).DirectionTo(New TVECTOR(aToPtXY), False, rDistance)}
            Return Projected(rDirection, rDistance)
        End Function

        Public Function ProjectionToPlane(aPlane As dxfPlane) As colDXFEntities
            If dxfPlane.IsNull(aPlane) Then Return New colDXFEntities
            Return ProjectionToPlaneV(aPlane.Strukture)
        End Function
        Friend Function ProjectionToPlaneV(aPlane As TPLANE) As colDXFEntities
            Dim _rVal As New colDXFEntities
            For i As Integer = 1 To Count
                _rVal.Append(dxfEntity.ProjectToPlane(Item(i), aPlane))
            Next i
            Return _rVal
        End Function
        Friend Sub RaiseEntitiesMemberChange(aEvent As dxfEntityEvent)
            If aEvent Is Nothing Then Return
            RaiseEvent EntitiesMemberChange(aEvent)
            Dim block As dxfBlock
            If HasReferenceTo_Block Then
                block = MyBlock
                If aEvent.DirtyOnChange Then
                    block.IsDirty = True
                End If
            End If
            If Not String.IsNullOrWhiteSpace(ImageGUID) Then
                Dim img As dxfImage = Image
                If img IsNot Nothing Then
                    Dim canc As Boolean
                    img.RespondToCollectionEvent(Me, dxxCollectionEventTypes.MemberChange, aEvent.Entity, canc)
                End If
            End If
        End Sub
        Private Sub RaiseEntitiesChange(aType As dxxCollectionEventTypes, aMemberOrSubSet As Object, ByRef rCancel As Boolean, Optional rListnerWantsClone As Boolean = False, Optional bCountChange As Boolean = False, Optional bSuppressEvnts As Boolean = False)
            Dim rImage As dxfImage = Nothing
            RaiseEntitiesChange(aType, aMemberOrSubSet, rCancel, rListnerWantsClone, bCountChange, rImage, bSuppressEvnts)
        End Sub
        Private Sub RaiseEntitiesChange(aType As dxxCollectionEventTypes, aMemberOrSubSet As Object, ByRef rCancel As Boolean, ByRef rListnerWantsClone As Boolean, bCountChange As Boolean, ByRef rImage As dxfImage, Optional bSuppressEvnts As Boolean = False)
            rCancel = False
            rListnerWantsClone = False
            If Not MonitorMembers Then Return
            If aType = dxxCollectionEventTypes.PreAdd And aMemberOrSubSet Is Nothing Then
                rCancel = True
                Return
            End If
            Dim ensevent As New dxfEntitiesEvent(aType, GUID, ImageGUID, BlockGUID, OwnerGUID) With {.CountChange = bCountChange}
            If aMemberOrSubSet IsNot Nothing Then
                If TypeOf (aMemberOrSubSet) Is dxfEntity Then
                    Dim ent As dxfEntity = aMemberOrSubSet
                    ensevent.Member = ent
                ElseIf TypeOf (aMemberOrSubSet) Is colDXFEntities Then
                    Dim ents As colDXFEntities = aMemberOrSubSet
                    ensevent.Members = ents.CollectionObj
                ElseIf TypeOf (aMemberOrSubSet) Is List(Of dxfEntity) Then
                    Dim ents As List(Of dxfEntity) = aMemberOrSubSet
                    ensevent.Members = ents
                End If
            End If
            RaiseEvent EntitiesChange(ensevent)
            rCancel = ensevent.Undo
            If Not rCancel And MonitorMembers Then
                If GetImage(rImage) Then
                    If String.IsNullOrWhiteSpace(OwnerGUID) And String.IsNullOrWhiteSpace(BlockGUID) Then
                        If (Not bSuppressEvnts And Not SuppressEvents) Or aType = dxxCollectionEventTypes.Add Then
                            rImage.RespondToCollectionEvent(Me, aType, aMemberOrSubSet, rCancel, rListnerWantsClone)
                            ensevent.ListnerWantsClone = rListnerWantsClone
                        Else
                            If aType = dxxCollectionEventTypes.PreAdd Then
                                rImage.HandleGenerator.AssignTo(ensevent.Member)
                            End If
                        End If
                    Else
                        If Not String.IsNullOrWhiteSpace(OwnerGUID) Then
                            Dim owner As dxfHandleOwner = MyOwner
                            If owner IsNot Nothing Then
                                owner.RespondToEntitiesChangeEvent(ensevent)
                            End If
                        End If
                        'Dim block As dxfBlock = Nothing
                        'Dim entity As dxfEntity = dxfEvents.GetImageEntity(ImageGUID, OwnerGUID, BlockGUID, block)
                        'If entity  IsNot Nothing Then entity.RespondToEntitiesChangeEvent(ensevent)
                        'rCancel = ensevent.Undo
                        'If Not rCancel And block IsNot Nothing Then
                        '    block.RespondToEntitiesChangeEvent(ensevent)
                        'End If
                    End If
                End If
            End If
            rListnerWantsClone = ensevent.ListnerWantsClone
            rCancel = ensevent.Undo
        End Sub
        Public Sub ReIndex()
            '^updates collection indices of the current members
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                aEnt.Index = i
            Next i
        End Sub
        Public Function ReduceTo(aCount As Integer) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the entity count to reduce to
            '^removes memebrs from the collection until the count matchs the passed number
            If aCount > Count Then Return _rVal
            Dim aMem As dxfEntity
            For i As Integer = Count To aCount + 1 Step -1
                aMem = RemoveV(i, True)
                _rVal.Add(aMem, 1)
            Next i
            If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            Return _rVal
        End Function
        Public Function ReduceToCount(aCount As Integer, Optional bRemoveFromBeginning As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}
            '#1the count to reduce to
            '#2flag to remove members from the beginning of the collection rather than from the end
            '^reduces the current collection to the passed count
            If aCount <= Count Then Return _rVal
            If Not bRemoveFromBeginning Then
                Do Until Count <= aCount
                    _rVal.Add(RemoveV(Count, True))
                Loop
            Else
                Do Until Count <= aCount
                    _rVal.Add(RemoveV(1, True))
                Loop
            End If
            If MaintainIndices And bRemoveFromBeginning Then ReIndex()
            If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, _rVal, False, False, True)
            Return _rVal
        End Function
        Friend Function ReferenceNames(aReferenceType As dxxReferenceTypes, Optional aNameInclude As String = "") As String
            Dim _rVal As String = aNameInclude?.Trim
            '^all of the dim style names referenced by the entities in the collection
            'On Error Resume Next
            If aReferenceType <> dxxReferenceTypes.LAYER And aReferenceType <> dxxReferenceTypes.LTYPE And aReferenceType <> dxxReferenceTypes.STYLE And aReferenceType <> dxxReferenceTypes.DIMSTYLE Then Return _rVal
            Dim aEnt As dxfEntity
            Dim lNames As String
            For i As Integer = 1 To Count
                aEnt = Item(i)
                Select Case aReferenceType
                    Case dxxReferenceTypes.DIMSTYLE
                        lNames = aEnt.DimStyleName
                    Case dxxReferenceTypes.LAYER
                        lNames = aEnt.LayerNames
                    Case dxxReferenceTypes.LTYPE
                        lNames = aEnt.LineTypes
                    Case dxxReferenceTypes.STYLE
                        lNames = aEnt.TextStyleNames
                    Case Else
                        lNames = ""
                End Select
                If lNames <> "" Then TLISTS.Append(_rVal, lNames, bUniqueValues:=True, bUpdateSourceString:=True)
            Next i
            Return _rVal
        End Function
        Friend Function ReferencesBlock(aBlockName As String, Optional aReturnJustOne As Boolean = False) As Boolean
            Dim rInserts As List(Of dxfEntity) = Nothing
            Return ReferencesBlock(aBlockName, rInserts, aReturnJustOne)
        End Function
        Friend Function ReferencesBlock(aBlockName As String, ByRef rInserts As List(Of dxfEntity), Optional aReturnJustOne As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            rInserts = New List(Of dxfEntity)
            aBlockName = Trim(aBlockName)
            If aBlockName = "" Then Return _rVal



            For i As Integer = 1 To Count
                Dim aMem As dxfEntity = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Insert Then
                    Dim aInsert As dxeInsert = DirectCast(aMem, dxeInsert)
                    If String.Compare(aInsert.BlockName, aBlockName, ignoreCase:=True) = 0 Then
                        _rVal = True
                        rInserts.Add(aMem)
                        If aReturnJustOne Then Return _rVal
                    End If
                ElseIf aMem.EntityType = dxxEntityTypes.LeaderBlock Then
                    Dim aLdr As dxeLeader = DirectCast(aMem, dxeLeader)
                    If String.Compare(aLdr.BlockName, aBlockName, ignoreCase:=True) = 0 Then
                        _rVal = True
                        rInserts.Add(aMem)
                        If aReturnJustOne Then Return _rVal
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Friend Function ReferencesDimStyle(aStyleName As String) As Boolean
            Dim _rVal As Boolean = False
            aStyleName = Trim(aStyleName)
            If aStyleName = "" Then Return _rVal
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.DimStyleName, aStyleName, ignoreCase:=True) = 0 Then
                    _rVal = True
                    Exit For
                End If
            Next i
            Return _rVal
        End Function
        Public Function ReferencesLayer(aName As String) As Boolean
            Dim rIndex As Integer = 0
            Return ReferencesLayer(aName, rIndex)
        End Function
        Public Function ReferencesLayer(aName As String, ByRef rIndex As Integer) As Boolean
            rIndex = 0
            If String.IsNullOrWhiteSpace(aName) Then Return False
            aName = aName.Trim
            rIndex = FindIndex(Function(mem) String.Compare(mem.LayerName, aName, True) = 0) + 1
            Return (rIndex > 0)
        End Function
        Public Function ReferencesLinetype(aName As String) As Boolean
            Dim rIndex As Integer = 0
            Return ReferencesLayer(aName, rIndex)
        End Function
        Public Function ReferencesLinetype(aName As String, ByRef rIndex As Integer) As Boolean
            rIndex = 0
            If String.IsNullOrWhiteSpace(aName) Then Return False
            aName = aName.Trim
            rIndex = FindIndex(Function(mem) String.Compare(mem.Linetype, aName, True) = 0) + 1
            Return (rIndex > 0)
        End Function
        Friend Function ReferencesStyle(aStyleName As String) As Boolean
            aStyleName = Trim(aStyleName)
            If aStyleName = "" Then Return False
            Dim aMem As dxfEntity
            Dim aTbl As dxeTable
            For i As Integer = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.TextStyleName, aStyleName, ignoreCase:=True) = 0 Then
                    Return True
                End If
                If aMem.GraphicType = dxxGraphicTypes.Table Then
                    aTbl = aMem
                    If String.Compare(aTbl.HeaderTextStyle, aStyleName, ignoreCase:=True) = 0 Then
                        Return True
                    End If
                    If String.Compare(aTbl.TitleTextStyle, aStyleName, ignoreCase:=True) = 0 Then
                        Return True
                    End If
                    If String.Compare(aTbl.FooterTextStyle, aStyleName, ignoreCase:=True) = 0 Then
                        Return True
                    End If
                End If
            Next i
            Return False
        End Function
        Public Function RemoveByGraphicType(aGraphicType As dxxGraphicTypes, Optional aMaxCount As Integer = 0) As List(Of dxfEntity)
            Dim eTypes As TVALUES = TVALUES.BitCode_Decompose(aGraphicType)
            If aMaxCount <= 0 Then aMaxCount = 0
            Dim iCnt As Integer = 0
            Dim _rVal As New List(Of dxfEntity)
            For Each mem As dxfEntity In Me
                If eTypes.FindNumericValue(mem.GraphicType, 0) > 0 Then
                    iCnt += 1
                    _rVal.Add(mem)
                    If iCnt >= aMaxCount And aMaxCount > 0 Then Exit For
                End If
            Next
            If _rVal.Count <= 0 Then Return _rVal
            RemoveMembersV(vCol:=_rVal, bSuppressEvnts:=False)
            Return _rVal
        End Function
        Public Function RemoveByEntityType(aEntityType As dxxEntityTypes, Optional aMaxCount As Integer = 0) As List(Of dxfEntity)
            If aMaxCount <= 0 Then aMaxCount = 0
            Dim iCnt As Integer = 0
            Dim _rVal As New List(Of dxfEntity)
            For Each mem As dxfEntity In Me
                If mem.EntityType = aEntityType Then
                    iCnt += 1
                    _rVal.Add(mem)
                    If iCnt >= aMaxCount And aMaxCount > 0 Then Exit For
                End If
            Next
            If _rVal.Count <= 0 Then Return _rVal
            RemoveMembersV(vCol:=_rVal, bSuppressEvnts:=False)
            Return _rVal
        End Function
        Public Overloads Function Remove(aHandleOrNameOrGUID As String, Optional bHandlePassed As Boolean = False) As dxfEntity
            If String.IsNullOrWhiteSpace(aHandleOrNameOrGUID) Then Return Nothing
            Dim _rVal As dxfEntity = Nothing
            If Not bHandlePassed Then
                If Not TryGet(aHandleOrNameOrGUID, _rVal) Then Return Nothing
            Else
                _rVal = GetByHandle(aHandleOrNameOrGUID)
                If _rVal Is Nothing Then Return Nothing
            End If
            MyBase.Remove(_rVal)
            If MaintainIndices Then ReIndex()
            If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, _rVal, False, False, True)
            Return _rVal
        End Function
        Public Overloads Function Remove(aMember As dxfEntity) As dxfEntity
            If aMember Is Nothing Then Return Nothing
            Dim idx As Integer = MyBase.IndexOf(aMember)
            If idx < 0 Then Return Nothing
            MyBase.RemoveAt(idx)
            If MaintainIndices Then ReIndex()
            If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, aMember, False, False, True)

        End Function
        Public Overloads Function Remove(aIndex As Integer) As dxfEntity
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            Dim _rVal As dxfEntity = RemoveV(aIndex, True)
            If MaintainIndices Then ReIndex()
            If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, _rVal, False, False, True)
            Return _rVal
        End Function
        Public Function RemoveByFlag(aFlag As String, Optional aTag As String = "", Optional bContainsString As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False) As colDXFEntities
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4a entity type to match
            '^removes and returns all the entities that match the search criteria
            Return GetByFlag(aFlag, aTag, bContainsString, aEntityType, False, False, True, bReturnInverse)
        End Function
        Public Function RemoveByHandle(aHandle As String) As dxfEntity
            Return Remove(Find(Function(mem) mem.Handle = aHandle))
        End Function
        Public Function RemoveByHandles(aHandles As List(Of String)) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aHandles Is Nothing Or Count <= 0 Then Return _rVal
            For Each handle As String In aHandles
                Dim aEnt As dxfEntity = RemoveByHandle(handle)
                If aEnt IsNot Nothing Then
                    _rVal.Add(aEnt)
                End If
                'Do Until aEnt Is Nothing
                '    _rVal.Add(aEnt)
                '    aEnt = RemoveByHandle(handle)
                'Loop
            Next
            Return _rVal
        End Function
        Public Function RemoveByTag(aTag As String, Optional aFlag As String = Nothing, Optional bContainsString As Boolean = False, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnInverse As Boolean = False) As colDXFEntities
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4a entity type to match
            '^removes and returns all the entities that match the search criteria
            Return GetByTag(aTag:=aTag, aFlag:=aFlag, bContainsString:=bContainsString, aEntityType:=aEntityType, bReturnJustOne:=False, bReturnClones:=False, bRemove:=True, bReturnInverse:=bReturnInverse)
        End Function
        Public Function RemoveMember(aMember As dxfEntity) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            '#1the member to remove
            '#2a vb collection to remove the member from
            '^removes the object from the collection if it is currently a member of the collection
            Dim aEnt As dxfEntity
            Dim bBail As Boolean
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt Is aMember Then
                    bBail = False
                    RaiseEntitiesChange(dxxCollectionEventTypes.PreRemove, aEnt, bBail, False, False)
                    If Not bBail Then
                        _rVal = RemoveV(i, True)
                    End If
                    Exit For
                End If
            Next i
            If _rVal IsNot Nothing Then
                If Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, _rVal, False, False, True)
            End If
            Return _rVal
        End Function

        Public Function RemoveMembers(aMembers As IEnumerable(Of dxfEntity)) As Integer
            '^removes the passed entities from this collection if they are actually members of it
            '#1the members to remove in a VB collection
            '#2the members to remove in a entities collection
            Dim _rVal As Integer = RemoveMembersV(aMembers, True)
            If _rVal > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, Nothing, False, False, True)
            Return _rVal
        End Function
        Friend Function RemoveMembersV(vCol As IEnumerable(Of dxfEntity), Optional bSuppressEvnts As Boolean = True) As Integer
            Dim _rVal As Integer = 0
            If vCol Is Nothing Then Return 0
            '^removes the passed entities from this collection if they are actually members of it
            '#1the members to remove in a VB collection
            '#2the members to remove in a entities collection

            Dim bBail As Boolean
            Dim rCol As New List(Of dxfEntity)

            For Each ent As dxfEntity In vCol
                If ent Is Nothing Then Continue For
                If MyBase.IndexOf(ent) < 0 Then Continue For

                bBail = False
                RaiseEntitiesChange(dxxCollectionEventTypes.PreRemove, ent, bBail, False, False)
                If Not bBail Then
                    MyBase.Remove(ent)
                    rCol.Add(ent)
                    _rVal += 1
                    End If

            Next
            If _rVal > 0 And Not SuppressEvents And Not bSuppressEvnts Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, New colDXFEntities(rCol), False, False, True)
            Return _rVal
        End Function
        Friend Function RemoveMembersV(vCol As List(Of dxfEntity), Optional bSuppressEvnts As Boolean = True) As Integer
            Dim _rVal As Integer = 0
            If vCol Is Nothing Then Return 0
            '^removes the passed entities from this collection if they are actually members of it
            '#1the members to remove in a VB collection
            '#2the members to remove in a entities collection
            Dim aEnt As dxfEntity
            Dim bBail As Boolean
            Dim rCol As New List(Of dxfEntity)
            Dim aMem As dxfEntity
            For i As Integer = 1 To vCol.Count
                aEnt = vCol.Item(i - 1)
                aMem = Find(Function(mem) mem.GUID = aEnt.GUID)
                If aMem IsNot Nothing Then
                    bBail = False
                    RaiseEntitiesChange(dxxCollectionEventTypes.PreRemove, aMem, bBail, False, False)
                    If Not bBail Then
                        MyBase.Remove(aMem)
                        rCol.Add(aMem)
                        _rVal += 1
                    End If
                End If
            Next i
            If _rVal > 0 And Not SuppressEvents And Not bSuppressEvnts Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, New colDXFEntities(rCol), False, False, True)
            Return _rVal
        End Function
        Friend Function RemoveG(aMember As dxfEntity, bSuppressEvnts As Boolean) As Boolean
            If aMember Is Nothing Then Return False
            '#1the member to remove
            '#2a vb collection to remove the member from
            '^removes the object from the collection if it is currently a member of the collection
            Dim aMem As dxfEntity = Nothing
            If Not TryGet(aMember.GUID, aMem) Then Return False
            If BlockGUID <> "" Then aMem.BlockGUID = ""
            If OwnerGUID <> "" Then aMem.OwnerGUID = ""
            If ImageGUID <> "" Then aMem.ImageGUID = ""
            aMem.CollectionGUID = ""
            MyBase.Remove(aMem)
            If Not SuppressEvents And Not bSuppressEvnts Then RaiseEntitiesChange(dxxCollectionEventTypes.Remove, aMem, False, False, True)
            Return True
        End Function
        Friend Function RemoveV(aIndex As Integer, bSuppressEvnts As Boolean) As dxfEntity
            '#1the member to remove
            '#2a vb collection to remove the member from
            '^removes the object from the collection if it is currently a member of the collection
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            Dim _rVal As dxfEntity = Item(aIndex)
            RemoveG(_rVal, bSuppressEvnts)
            Return _rVal
        End Function
        Public Function Rescale(aScaleFactor As Double, Optional aReference As iVector = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the factor to scale the members by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the members in space and dimension by the passed factor
            If Count <= 0 Then Return False
            Dim _rVal As Boolean = False
            For Each ent As dxfEntity In Me
                If ent.Rescale(aScaleFactor, aReference, aPlane) Then _rVal = True
            Next
            Return _rVal
            'Return Transform(TTRANSFORM.CreateScale(New TVECTOR(aReference), aScaleFactor, aPlane:=aPlane), aEventName:="Rescale")
        End Function
        Friend Function RespondToLayerChange(aImage As dxfImage, aLayer As dxoLayer, aLayerProp As dxoProperty) As Integer
            If aLayerProp Is Nothing Or aLayer Is Nothing Then Return 0
            Dim _rVal As Integer = 0
            Dim i As Integer
            Dim aMem As dxfEntity
            Dim bNewPens As Boolean
            Dim lname As String
            lname = aLayer.PropValueStr(dxxLayerProperties.Name)
            If aLayerProp.GroupCode = 2 Then
                SetDisplayVariable(dxxDisplayProperties.LayerName, aLayerProp.Value, aLayerProp.LastValue)
            Else
                bNewPens = (aLayerProp.GroupCode = 6) Or (aLayerProp.GroupCode = 62) Or (aLayerProp.GroupCode = 370)
                For i = 1 To Count
                    aMem = Item(i)
                    If bNewPens Then
                        If aMem.ReferencesLayer(lname) Then
                            If aMem.HasSubEntities Then
                                aMem.IsDirty = True
                            End If
                        End If
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Public Function RightMost(Optional aCS As dxfPlane = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.Center, Optional aPrecis As Integer = 3, Optional bReturnClone As Boolean = False) As dxfEntity
            Return GetByPoint(dxxPointFilters.AtMaxX, aPlane:=aCS, aPrecis:=aPrecis, bReturnClone:=bReturnClone, bRemove:=False, aEntPointType:=aDefPtType, aEntityType:=aEntityType)
        End Function
        Public Function Rotate(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Rotate(aAngle, bInRadians, aPlane, rAxis)
        End Function
        Public Function Rotate(aAngle As Double, bInRadians As Boolean, aPlane As dxfPlane, ByRef rAxis As dxeLine) As Boolean
            '#1the angle to rotate the members
            '^used to rotate the members about the z axis of then passed coordinate system
            '~if no coordinate system is passed then the global z axis is used
            If dxfPlane.IsNull(aPlane) Then
                Return Transform(TTRANSFORM.CreatePlanarRotation(New TVECTOR, TPLANE.World, aAngle, bInRadians, Nothing, dxxAxisDescriptors.Z, rAxis))
            Else
                Return Transform(TTRANSFORM.CreatePlanarRotation(aPlane.OriginV, aPlane.Strukture, aAngle, bInRadians, Nothing, dxxAxisDescriptors.Z, rAxis))
            End If
        End Function
        Public Function RotateAbout(aPoint As iVector, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z) As Boolean
            '#1the point to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the plane which is used to obtaint the axis of rotation. If null, the world plane is assumed.
            '#5an optional axis descriptor to selected a planes primary axis to use other than the Z axis.
            '^rotates the members about an axis starting at the passed point and aligned with the Z axis of the passed plane
            '~if the passed point is null the members is rotated about the origin of the passed coordinated system
            If Count = 0 Then Return False
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane
            If aPoint Is Nothing Then aPoint = New dxfVector(aPlane.Origin)
            Dim wuz As Boolean = SuppressEvents
            SuppressEvents = True
            Dim _rVal As Boolean = Transform(TTRANSFORM.CreateRotation(aPoint, aPlane, aAngle, bInRadians, Nothing, True, aAxisDescriptor), aEventName:="RotateAbout-Point")
            SuppressEvents = wuz
            If _rVal And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, Nothing, False, False, False)
            Return _rVal
        End Function
        Public Function RotateAbout(aPlane As dxfPlane, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPoint As dxfVector = Nothing, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z) As Boolean
            '#1the plane which is used to obtaint the axis of rotation. If null, the world plane is assumed.
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the point to rotate about
            '#5an optional axis descriptor to selected a planes primary axis to use other than the Z axis.
            '^rotates the members about the an axis the requested  starting at the passed point and aligned with the X axis of the passef plane
            '~if the passed point is null the members is rotated about the origin of the passed coordinated system
            If Count = 0 Then Return False
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane
            If aPoint Is Nothing Then aPoint = New dxfVector(aPlane.Origin)
            Dim wuz As Boolean = SuppressEvents
            SuppressEvents = True
            Dim _rVal As Boolean = Transform(TTRANSFORM.CreateRotation(aPoint, aPlane, aAngle, bInRadians, Nothing, True, aAxisDescriptor), aEventName:="RotateAbout-Point")
            SuppressEvents = wuz
            If _rVal And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, Nothing, False, False, False)
            Return _rVal
        End Function
        Public Function RotateAbout(aAxis As iLine, aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            If aAxis Is Nothing Or Count = 0 Then Return False
            '#1the line to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '^rotates the members about the passed axis the requested angle
            '~if the passed line is nothing no action is taken
            Dim wuz As Boolean = SuppressEvents
            SuppressEvents = True
            Dim _rVal As Boolean = Transform(TTRANSFORM.CreateRotation(aAxis, Nothing, aAngle, bInRadians, SuppressEvents), aEventName:="RotateAbout-Line")
            SuppressEvents = wuz
            If _rVal And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, Nothing, False, False, False)
            Return _rVal
        End Function
        Friend Sub RotateToPlaneV(aFromPlane As TPLANE, aToPane As TPLANE, Optional aRotation As Double = 0.0)
            Dim aTr As TTRANSFORMS = TTRANSFORMS.CreateRotateToPlane(aFromPlane, aToPane, aRotation, False)
            If aTr.Count > 0 Then Transform(aTr, aEventName:="RotateToPlaneV")
        End Sub
        Public Function SetColoyByLinetype(aLineType As String, aColor As dxxColors) As Integer
            If aLineType Is Nothing Then Return 0
            Dim _rVal As Integer = 0
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If String.Compare(aEnt.Linetype, aLineType, ignoreCase:=True) = 0 Then
                    If aEnt.Color <> aColor Then
                        _rVal += 1
                        aEnt.Color = aColor
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function SetCoordinates(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnChangers As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the X coordinate to set
            '#2the Y coordinate to set
            '#3the Z coordinate to set
            '^sets the member ordiates to the passed values
            '~unpassed values or non-numeric values are ignored.
            Dim DoX As Boolean = aX IsNot Nothing And aX.HasValue
            Dim doY As Boolean = aY IsNot Nothing And aY.HasValue
            Dim doZ As Double = aZ IsNot Nothing And aZ.HasValue
            Dim bKeep As Boolean
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aPt As dxfVector
            Dim cnt As Integer
            Dim aEnt As dxfEntity
            Dim xx As Double
            Dim yy As Double
            Dim zz As Double
            Dim P1 As dxfVector
            Dim P2 As dxfVector
            If bReturnChangers Then
                _rVal.MaintainIndices = False
            End If
            If DoX Then xx = aX.Value
            If doY Then yy = aY.Value
            If doZ Then zz = aZ.Value
            If Not DoX And Not doY And Not doZ Then Return _rVal
            P1 = New dxfVector
            P2 = New dxfVector
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    aPt = aEnt.DefinitionPoint(dxxEntDefPointTypes.HandlePt)
                    If aPt IsNot Nothing Then
                        v1 = aPt.Strukture
                        v2 = v1
                        If DoX Then v2.X = xx
                        If doY Then v2.Y = yy
                        If doZ Then v2.Z = zz
                        P1.Strukture = v1
                        P2.Strukture = v2
                        bKeep = aEnt.MoveFromTo(P1, P2)
                        If bKeep Then cnt += 1
                        If bReturnChangers And bKeep Then
                            _rVal.Add(aEnt)
                        End If
                    End If
                End If
            Next i
            If cnt > 0 And Not SuppressEvents Then
                RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, Nothing, False, False, False)
            End If
            Return _rVal
        End Function
        Friend Sub SetDisplayStructure(aStruc As TDISPLAYVARS, Optional aIdentifier As String = Nothing)
            Dim bIdent As Boolean = aIdentifier IsNot Nothing
            Dim aDisp As New dxfDisplaySettings(aStruc)
            For i As Integer = 1 To Count
                Dim aEnt As dxfEntity = Item(i)
                aEnt.CopyDisplayValues(aDisp)
                If bIdent Then aEnt.Identifier = aIdentifier
            Next i
        End Sub


        ''' <summary>
        ''' sets the members indicated display variable to the new value
        ''' </summary>
        ''' <param name="aDisplaySettings">the property type to set the value for</param>
        ''' <param name="aSearchType">a filter for enity type</param>
        ''' <param name="aTagList">a list of tags to search for</param>
        ''' <param name="aFlagList">a list of tags to search for</param>
        ''' <param name="aStartID">an optional statrt index used to limit the search set</param>
        ''' <param name="aEndID">an optional end index used to limit the search set</param>
        ''' <returns></returns>
        Public Function SetDisplayVariables(aDisplaySettings As dxfDisplaySettings, Optional aSearchType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aTagList As String = "", Optional aFlagList As String = "", Optional aStartID As Integer = 0, Optional aEndID As Integer = 0) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aDisplaySettings Is Nothing Or Count <= 0 Then Return _rVal


            Dim tg As String = String.Empty
            Dim fg As String = String.Empty
            Dim bT As Boolean = Not String.IsNullOrWhiteSpace(aTagList)
            Dim bF As Boolean = Not String.IsNullOrWhiteSpace(aFlagList)
            Dim si As Integer
            Dim ei As Integer
            Dim istep As Integer
            If dxfUtils.LoopIndices(Count, aStartID:=aStartID, aEndID:=aEndID, si, ei, Nothing, istep) Then
                Dim TorF As Boolean = bT Or bF
                For i As Integer = si To ei Step istep
                    Dim aMem As dxfEntity = Item(i)
                    If TorF Then aMem.TFVGet(tg, fg, Nothing)
                    Dim bDoIt As Boolean = aSearchType = dxxEntityTypes.Undefined Or aMem.EntityType = aSearchType
                    If bDoIt And bT Then
                        bDoIt = TLISTS.Contains(tg, aTagList)
                    End If
                    If bDoIt And bF Then
                        bDoIt = TLISTS.Contains(fg, aFlagList)
                    End If
                    If bDoIt Then
                        If aMem.CopyDisplayValues(aDisplaySettings) Then
                            _rVal.Add(aMem)
                        End If
                    End If
                Next i
            End If

            Return _rVal
        End Function

        ''' <summary>
        ''' sets the members indicated display variable to the new value
        ''' </summary>
        ''' <param name="aPropertyType">the property type to set the value for</param>
        ''' <param name="aNewValue">the value to apply to the property</param>
        ''' <param name="aMatchValue">if this is passed, only properties that currently match this value are changed</param>
        ''' <param name="aSearchType">a filter for enity type</param>
        ''' <param name="aTagList">a list of tags to search for</param>
        ''' <param name="aFlagList">a list of tags to search for</param>
        ''' <param name="aStartID">an optional statrt index used to limit the search set</param>
        ''' <param name="aEndID">an optional end index used to limit the search set</param>
        ''' <returns></returns>
        Public Function SetDisplayVariable(aPropertyType As dxxDisplayProperties, aNewValue As Object, Optional aMatchValue As String = Nothing, Optional aSearchType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aTagList As String = "", Optional aFlagList As String = "", Optional aStartID As Integer = 0, Optional aEndID As Integer = 0) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aPropertyType = dxxDisplayProperties.Undefined Or Count <= 0 Then Return _rVal


            Dim tg As String = String.Empty
            Dim fg As String = String.Empty
            Dim bT As Boolean = Not String.IsNullOrWhiteSpace(aTagList)
            Dim bF As Boolean = Not String.IsNullOrWhiteSpace(aFlagList)
            Dim si As Integer
            Dim ei As Integer
            Dim istep As Integer
            If dxfUtils.LoopIndices(Count, aStartID:=aStartID, aEndID:=aEndID, si, ei, Nothing, istep) Then
                Dim TorF As Boolean = bT Or bF
                For i As Integer = si To ei Step istep
                    Dim aMem As dxfEntity = Item(i)
                    If TorF Then aMem.TFVGet(tg, fg, Nothing)
                    Dim bDoIt As Boolean = aSearchType = dxxEntityTypes.Undefined Or aMem.EntityType = aSearchType
                    If bDoIt And bT Then
                        bDoIt = TLISTS.Contains(tg, aTagList)
                    End If
                    If bDoIt And bF Then
                        bDoIt = TLISTS.Contains(fg, aFlagList)
                    End If
                    If bDoIt Then
                        If aMem.SetDisplayProperty(aPropertyType, aNewValue, aMatchValue) Then
                            _rVal.Add(aMem)
                        End If
                    End If
                Next i
            End If

            Return _rVal
        End Function
        Public Sub SetFlags(aFlag As String, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined)
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or (aEntityType <> dxxEntityTypes.Undefined And aEnt.EntityType = aEntityType) Then
                    aEnt.Flag = aFlag
                End If
            Next i
        End Sub
        Public Sub SetGroupName(aGroupName As String)
            '^used to set the group name of all the member entities in one call
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                aEnt.GroupName = aGroupName
            Next i
        End Sub
        Public Sub SetHoleDepth(aDepth As Double, Optional aZ As Double? = Nothing)
            If Count <= 0 Then Return

            Dim aHl As dxeHole
            Dim bSetZ As Boolean = aZ IsNot Nothing And aZ.HasValue

            For i As Integer = 1 To Count
                Dim aMem As dxfEntity = Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Hole Then
                    aHl = DirectCast(aMem, dxeHole)
                    aHl.Depth = aDepth
                    If bSetZ Then aHl.Z = aZ.Value
                End If
            Next i
        End Sub
        Public Function SetLayerNameByColor(aLayerName As String, aColor As dxxColors, Optional bInvertProcess As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}
            '#1the new value for the layer name property
            '#2a color to match
            '#3flag to set members with a color not equal to the passed value to the layer
            '#4a graphic type filter
            '#5a entity type filter
            '^sets the members with the passed color to the new layer
            aLayerName = Trim(aLayerName)
            aColor = Trim(aColor)
            If aColor = dxxColors.Undefined Or aLayerName = "" Then Return _rVal
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aGraphicType = dxxGraphicTypes.Undefined Or aMem.GraphicType = aGraphicType Then
                    If aEntityType = dxxEntityTypes.Undefined Or aMem.EntityType = aEntityType Then
                        If Not bInvertProcess Then
                            If aMem.Color = aColor Then
                                aMem.LayerName = aLayerName
                                _rVal.AddToCollection(aMem, bSuppressEvnts:=True)
                            End If
                        Else
                            If aMem.Color <> aColor Then
                                aMem.LayerName = aLayerName
                                _rVal.AddToCollection(aMem, bSuppressEvnts:=True)
                            End If
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function SetLayerNameByLineType(aLayerName As String, aLineType As String, Optional bInvertProcess As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}
            '#1the new value for the layer name property
            '#2a linetype name to match
            '#3flag to set members with a linetype not equal to the passed value to the layer
            '#4a graphic type filter
            '#5a entity type filter
            '^sets the members with the passed linetype to the new layer
            aLayerName = Trim(aLayerName)
            aLineType = Trim(aLineType)
            If aLineType = "" Or aLayerName = "" Then Return _rVal
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aGraphicType = dxxGraphicTypes.Undefined Or aMem.GraphicType = aGraphicType Then
                    If aEntityType = dxxEntityTypes.Undefined Or aMem.EntityType = aEntityType Then
                        If Not bInvertProcess Then
                            If String.Compare(aMem.Linetype, aLineType, ignoreCase:=True) = 0 Then
                                aMem.LayerName = aLayerName
                                _rVal.AddToCollection(aMem, bSuppressEvnts:=True)
                            End If
                        Else
                            If String.Compare(aMem.Linetype, aLineType, ignoreCase:=True) <> 0 Then
                                aMem.LayerName = aLayerName
                                _rVal.AddToCollection(aMem, bSuppressEvnts:=True)
                            End If
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function SetLineWeight(aLineWeight As dxxLineWeights, Optional aSearchValue As dxxLineWeights = dxxLineWeights.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined) As List(Of dxfEntity)
            '#1the new value for the lineweight property
            '#2a variable value to match
            '#3a entity type filter
            '^sets the members lineweight to the new value
            '~if a search value is passed then only members with a current value equal to the search value will be affected.
            '~returns the affected members.
            Dim sVal As Integer
            If aSearchValue >= dxxLineWeights.ByBlock And aSearchValue <= dxxLineWeights.LW_211 Then sVal = aSearchValue
            Return SetDisplayVariable(dxxDisplayProperties.LineWeight, aLineWeight, sVal, aEntityType)
        End Function
        Public Function SetSuppressed(aSuppressionValue As Boolean, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aTagList As String = "", Optional aFlagList As String = "") As List(Of dxfEntity)
            Return SetDisplayVariable(dxxDisplayProperties.Suppressed, aSuppressionValue, aSearchType:=aEntityType, aTagList:=aTagList, aFlagList:=aFlagList)
        End Function
        Public Sub SetTags(aTag As String, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined)
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or aEnt.EntityType = aEntityType Then
                    aEnt.Tag = aTag
                End If
            Next i
        End Sub
        Public Sub SetTagsAndFlags(Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aSearchTag As String = Nothing, Optional aSearchFlag As String = Nothing, Optional aStartID As Integer = -1, Optional aEndID As Integer = -1, Optional bAddTagIndices As Boolean = False)
            '#1the new tag to assign to the members. null input is ignored.
            '#2the new flag to assign to the members. null input is ignored.
            '#3an existing tag to match
            '#4an existing flag to match
            '#5an optional starting index
            '#6an optional ending index
            '#7flag to append the index of the member to the passed tag value for each member
            '^used to set the tags and flags of the members in one call
            Dim aMem As dxfEntity
            Dim bTags As Boolean
            Dim bFlags As Boolean
            Dim aStr As String = "'"
            Dim bStr As String = String.Empty
            Dim aTg As String = String.Empty
            Dim aFg As String = String.Empty
            Dim i As Integer
            Dim si As Integer
            Dim ei As Integer
            Dim bTestTag As Boolean
            Dim bTestFlag As Boolean
            Dim bDoIt As Boolean
            bTags = aTag IsNot Nothing
            bFlags = aFlag IsNot Nothing
            bTestTag = aSearchTag IsNot Nothing
            bTestFlag = aSearchFlag IsNot Nothing
            If Not bTags And Not bFlags Then Return
            If bTags Then aStr = aTag
            If bFlags Then bStr = aFlag
            If bTestTag Then aTg = aSearchTag
            If bTestFlag Then aFg = aSearchFlag
            If Not dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei) Then Return
            For i = si To ei
                aMem = Item(i)
                bDoIt = True
                If bTestTag Or bTestFlag Then
                    If bTestTag Then
                        If String.Compare(aMem.Tag, aTg, ignoreCase:=True) <> 0 Then bDoIt = False
                    End If
                    If bTestFlag Then
                        If String.Compare(aMem.Flag, aFg, ignoreCase:=True) <> 0 Then bDoIt = False
                    End If
                End If
                If bDoIt Then
                    If bTags Then
                        If bAddTagIndices Then aMem.Tag = $"{aStr}{i}" Else aMem.Tag = aStr
                    End If
                    If bFlags Then aMem.Flag = bStr
                End If
            Next i
        End Sub
        Public Sub SetValues(aValue As Double, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined)

            For i As Integer = 1 To Count
                Dim aEnt As dxfEntity = Item(i)
                If aEntityType = dxxEntityTypes.Undefined Or aEnt.EntityType = aEntityType Then
                    aEnt.Value = aValue
                End If
            Next i
        End Sub
        Public Overloads Function Sort(aOrder As dxxSortOrders, Optional aReferencePt As dxfVector = Nothing, Optional aEntDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aCS As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean = False
            Try
                If Count <= 1 Then Return _rVal
                If aEntDefPtType < dxxEntDefPointTypes.StartPt Then aEntDefPtType = dxxEntDefPointTypes.StartPt
                If dxxEntDefPointTypes.EndPt < dxxEntDefPointTypes.StartPt Then aEntDefPtType = dxxEntDefPointTypes.EndPt
                Dim dPts As colDXFVectors
                Dim aIds As New List(Of Integer)
                dPts = DefinitionPoints(aEntDefPtType)
                dPts.Sort(aOrder, aReferencePt, aCS, aIds)
                Dim newCol As New List(Of dxfEntity)



                Dim aMem As dxfEntity
                For i As Integer = 1 To dPts.Count
                    Dim v1 As dxfVector = dPts.Item(i)
                    Dim idx As Integer = aIds.Item(i - 1)
                    If idx <> i Then _rVal = True
                    aMem = Item(idx)
                    If aMem IsNot Nothing Then newCol.Add(aMem)
                Next i
                MyBase.Clear()
                MyBase.AddRange(newCol)
                Return _rVal
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Overloads Function Sort(aOrder As dxxSortOrders) As Boolean
            Return (Sort(aOrder, Nothing, dxxEntDefPointTypes.HandlePt, Nothing))
        End Function
        Public Function Spin(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Spin(aAngle, bInRadians, aPlane, rAxis)
        End Function
        Public Function Spin(aAngle As Double, bInRadians As Boolean, aPlane As dxfPlane, ByRef rAxis As dxeLine) As Boolean
            '#1the angle to rotate the members
            '^used to rotate the members about the z axis of then passed coordinate system
            '~if no coordinate system is passed then the global z axis is used
            If dxfPlane.IsNull(aPlane) Then
                Return Transform(TTRANSFORM.CreatePlanarRotation(New TVECTOR, TPLANE.World, aAngle, bInRadians, Nothing, dxxAxisDescriptors.Z, rAxis))
            Else
                Return Transform(TTRANSFORM.CreatePlanarRotation(aPlane.OriginV, aPlane.Strukture, aAngle, bInRadians, Nothing, dxxAxisDescriptors.Z, rAxis))
            End If
        End Function
        Public Function StartPt() As dxfVector
            If Count <= 0 Then Return (Nothing)
            Return FirstMember.DefinitionPoint(dxxEntDefPointTypes.StartPt)
        End Function
        Public Function StartPts(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bReturnClones As Boolean = False, Optional aSearchTag As String = Nothing, Optional aSuppressedVal As Boolean? = Nothing) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1a entity type filter
            '#2flag to return copies of the points
            '#3a tag to apply to the search
            '^returns the mid points of all the entities in the collection
            _rVal = DefinitionPoints(dxxEntDefPointTypes.StartPt, aEntityType, aSearchTag, bReturnClones, aSuppressedVal)
            Return _rVal
        End Function

        Friend Function SubEntities_Get(bIncludeInstances As Boolean) As TENTITIES
            Return dxfEntities.GetStructures(Me, bIncludeInstances, OwnerGUID, ImageGUID)
        End Function

        Friend Sub SubEntities_Set(aSubEnts As IEnumerable(Of dxfEntity), bIncludeInstances As Boolean, Optional bNoHandles As Boolean = False, Optional aImageGUID As String = Nothing, Optional aSuppressedVal As Boolean? = Nothing, Optional bGetClones As Boolean = False)
            Clear(bSuppressEvnts:=True)
            If aSubEnts Is Nothing Then Return

            Dim sEnt As dxfEntity

            Dim sEnts As dxfEntities
            Dim iGUID As String = TVALUES.To_STR(aImageGUID, ImageGUID)
            Dim testsuppressed As Boolean = aSuppressedVal IsNot Nothing And aSuppressedVal.HasValue

            For i As Integer = 1 To aSubEnts.Count
                sEnt = aSubEnts(i - 1)
                If sEnt Is Nothing Then Continue For
                sEnt.ImageGUID = iGUID

                If Not bIncludeInstances Or sEnt.Instances.Count <= 0 Then

                    If bNoHandles Then
                        sEnt.Handle = ""
                    End If
                    If testsuppressed Then
                        If sEnt.Suppressed = aSuppressedVal.Value Then AddToCollection(sEnt, aAddClone:=bGetClones, bSuppressEvnts:=True)
                    Else
                        AddToCollection(sEnt, aAddClone:=bGetClones, bSuppressEvnts:=True)
                    End If
                Else



                    sEnts = dxfEntities.GetInstanceEntities(sEnt)
                    For j As Integer = 1 To sEnts.Count
                        Dim aMem As dxfEntity = sEnts.Item(j - 1)
                        If aMem Is Nothing Then Continue For

                        aMem.ImageGUID = iGUID
                        If bNoHandles Then
                            'aMem.GUID = dxfEvents.NextEntityGUID(aMem.GraphicType)
                            aMem.Handle = ""
                        End If
                        If testsuppressed Then
                            If aMem.Suppressed = aSuppressedVal.Value Then AddToCollection(aMem, bSuppressEvnts:=True)
                        Else
                            AddToCollection(aMem, bSuppressEvnts:=True)
                        End If

                    Next j
                End If
            Next i
        End Sub

        Friend Sub SubEntities_Set(aSubEnts As TENTITIES, bIncludeInstances As Boolean, Optional bNoHandles As Boolean = False, Optional aImageGUID As String = Nothing, Optional aSuppressedVal As Boolean? = Nothing)
            Clear(True)
            Dim sEnt As TENTITY
            Dim aMem As dxfEntity
            Dim sEnts As TENTITIES
            Dim iGUID As String = TVALUES.To_STR(aImageGUID, ImageGUID)
            Dim testsuppressed As Boolean = aSuppressedVal IsNot Nothing And aSuppressedVal.HasValue

            For i As Integer = 1 To aSubEnts.Count
                sEnt = aSubEnts.Item(i)
                If Not bIncludeInstances Then
                    aMem = sEnt.GetEntity(bNewGUID:=bNoHandles)
                    If aMem IsNot Nothing Then
                        aMem.ImageGUID = iGUID
                        If bNoHandles Then
                            'aMem.GUID = dxfEvents.NextEntityGUID(aMem.GraphicType)
                            aMem.Handle = ""
                        End If
                        If testsuppressed Then
                            If aMem.Suppressed = aSuppressedVal.Value Then AddToCollection(aMem, bSuppressEvnts:=True)
                        Else
                            AddToCollection(aMem, bSuppressEvnts:=True)
                        End If
                    End If
                Else
                    sEnts = sEnt.GetEntities()
                    For j As Integer = 1 To sEnts.Count
                        aMem = sEnts.Item(j).GetEntity(bNewGUID:=bNoHandles)
                        If aMem IsNot Nothing Then
                            aMem.ImageGUID = iGUID
                            If bNoHandles Then
                                'aMem.GUID = dxfEvents.NextEntityGUID(aMem.GraphicType)
                                aMem.Handle = ""
                            End If
                            If testsuppressed Then
                                If aMem.Suppressed = aSuppressedVal.Value Then AddToCollection(aMem, bSuppressEvnts:=True)
                            Else
                                AddToCollection(aMem, bSuppressEvnts:=True)
                            End If
                        End If
                    Next j
                End If
            Next i
        End Sub



        ''' <summary>
        ''' returns a contiguous subset of the collection
        ''' </summary>
        ''' <remarks>if the ending index is null it is assumed to be the end of the collection</remarks>
        ''' <param name="aStartID">the starting index</param>
        ''' <param name="aEndID">the ending index</param>
        ''' <param name="aGraphicType">a filter for graphic type</param>
        ''' <param name="bReturnClones">a flag to returns clones of the target members</param>
        ''' <param name="bRemove">aflag to remove the target set from the collection</param>
        ''' <returns></returns>
        Public Function SubSet(aStartID As Integer, aEndID As Integer?, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If Count <= 0 Then Return _rVal

            Dim mems As List(Of dxfEntity) = dxfEntities.SubSet(Me, aStartID, aEndID, aGraphicType)
            If mems.Count <= 0 Then Return _rVal
            _rVal.Populate(mems, bAddClones:=bReturnClones)

            If bRemove Then
                If RemoveMembersV(mems, True) > 0 And Not SuppressEvents Then RaiseEntitiesChange(dxxCollectionEventTypes.RemoveSet, mems, False, False, True)
            End If
            Return _rVal
        End Function
        Public Sub Suppress(Optional aTagToSuppress As String = Nothing)
            Dim aEnt As dxeLine
            Dim bTest As Boolean

            bTest = aTagToSuppress IsNot Nothing

            For i As Integer = 1 To Count
                aEnt = Item(i)
                If Not bTest Then
                    aEnt.Suppressed = True
                Else
                    If String.Compare(aEnt.Tag, aTagToSuppress, True) = 0 Then
                        aEnt.Suppressed = True
                    End If
                End If
            Next i
        End Sub
        Friend Sub SwapCollection(aEntities As colDXFEntities)
            If aEntities Is Nothing Then Return
            Dim aCol As List(Of dxfEntity) = aEntities.CollectionObj
            aEntities.CollectionObj = Me
            MyBase.Clear()
            MyBase.AddRange(aCol)

        End Sub
        Public Function TagList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                TLISTS.Add(_rVal, aMem.Tag, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function TagSets(Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aExtrusionDirection As dxfDirection = Nothing, Optional bReturnClones As Boolean = False) As List(Of colDXFEntities)
            Dim _rVal As New List(Of colDXFEntities)
            Dim aTags As List(Of String)
            Dim aSet As colDXFEntities = Me
            If aGraphicType <> dxxGraphicTypes.Undefined Then aSet = aSet.GetByGraphicType(aGraphicType)
            If aEntityType <> dxxEntityTypes.Undefined Then aSet = aSet.GetByEntityType(aGraphicType)
            If aExtrusionDirection IsNot Nothing Then aSet = aSet.GetByExtrusionDirection(aExtrusionDirection)
            aTags = Tags(True)
            For Each tag As String In aTags
                _rVal.Add(aSet.GetByTag(tag, bReturnClones:=bReturnClones))
            Next
            Return _rVal
        End Function
        Public Function TagString(Optional aDelimitor As String = ",", Optional bUniqueOnly As Boolean = True, Optional bIncludeFlags As Boolean = False, Optional aFlagDelimitor As String = ":") As String
            Dim _rVal As String = String.Empty
            '^returns a string containing the unique tags of the members
            Dim aTags As List(Of String) = Tags(bUniqueOnly, bIncludeFlags, aFlagDelimitor)
            For Each tag As String In aTags
                If Not String.IsNullOrWhiteSpace(tag) Then
                    If _rVal <> "" Then _rVal += aDelimitor
                    _rVal += tag
                End If
            Next
            Return _rVal
        End Function
        Public Function TaggedCount(Optional aTagValue As String = Nothing, Optional aType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional bContaining As Boolean = False) As Integer
            Dim _rVal As Integer = 0
            '#1the tag to count
            '^returns the number of entities in the collection that have their tag property defined
            '^or have a tag that matches the passed value
            Dim aEnt As dxfEntity
            Dim bTest As Boolean = aTagValue IsNot Nothing
            Dim aStr As String = String.Empty
            If bTest Then aStr = aTagValue
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aType = dxxEntityTypes.Undefined Or aEnt.EntityType = aType Then
                    If Not bTest Then
                        If aEnt.Tag <> "" Then _rVal += 1
                    Else
                        If Not bContaining Then
                            If String.Compare(aEnt.Tag, aStr, ignoreCase:=True) = 0 Then _rVal += 1
                        Else
                            If aEnt.Tag.IndexOf(aStr, StringComparison.OrdinalIgnoreCase) + 1 > 0 Then _rVal += 1
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function Tags(Optional bUniqueOnly As Boolean = False, Optional bIncludeFlags As Boolean = False, Optional aFlagDelimitor As String = ":") As List(Of String)
            Dim _rVal As New List(Of String)
            '#1flag to return only the unique flags
            '^returns a collection of strings containing the flags of the members
            Dim aMem As dxfEntity
            Dim bKeep As Boolean
            Dim sval As String
            For i As Integer = 1 To Count
                aMem = Item(i)
                bKeep = True
                sval = aMem.Tag
                If bIncludeFlags Then
                    If Not String.IsNullOrWhiteSpace(aMem.Flag) Then
                        sval += aFlagDelimitor + aMem.Flag
                    End If
                End If
                If bUniqueOnly Then
                    bKeep = _rVal.FindIndex(Function(mem) String.Compare(mem, sval, True) = 0) < 0
                End If
                If bKeep Then _rVal.Add(sval)
            Next i
            Return _rVal
        End Function
        Public Function TextEntities(Optional aTextType As dxxTextTypes = dxxTextTypes.Undefined) As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}
            If Not dxfEnums.Validate(GetType(dxxTextTypes), aTextType, bSkipNegatives:=True) Then
                Dim aMem As dxfEntity
                For i As Integer = 1 To Count
                    aMem = Item(i)
                    If aMem.IsText Then _rVal.Add(aMem)
                Next
            Else
                Select Case aTextType
                    Case dxxTextTypes.AttDef
                        _rVal = GetByType(dxxEntityTypes.Attdef)
                    Case dxxTextTypes.Attribute
                        _rVal = GetByType(dxxEntityTypes.Attribute)
                    Case dxxTextTypes.DText
                        _rVal = GetByType(dxxEntityTypes.Text)
                    Case dxxTextTypes.Multiline
                        _rVal = GetByType(dxxEntityTypes.MText)
                End Select
            End If
            Return _rVal
        End Function
        Public Function TextStyleNames(Optional aStyleToInclude As String = "") As String
            Dim _rVal As String = aStyleToInclude?.Trim
            '^all of the text style names referenced by the entities in the collection
            Dim aEnt As dxfEntity
            Dim lNames As String
            For i As Integer = 1 To Count
                aEnt = Item(i)
                lNames = aEnt.TextStyleNames
                If lNames <> "" Then TLISTS.Add(_rVal, lNames)
            Next i
            Return _rVal
        End Function
        Public Function Tip(aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Tip(aAngle, bInRadians, aPlane, rAxis)
        End Function
        Public Function Tip(aAngle As Double, bInRadians As Boolean, aPlane As dxfPlane, ByRef rAxis As dxeLine) As Boolean
            '#1the angle to rotate the members
            '^used to rotate the members about the z axis of then passed coordinate system
            '~if no coordinate system is passed then the global z axis is used
            If dxfPlane.IsNull(aPlane) Then
                Return Transform(TTRANSFORM.CreatePlanarRotation(New TVECTOR, TPLANE.World, aAngle, bInRadians, Nothing, dxxAxisDescriptors.Z, rAxis))
            Else
                Return Transform(TTRANSFORM.CreatePlanarRotation(aPlane.OriginV, aPlane.Strukture, aAngle, bInRadians, Nothing, dxxAxisDescriptors.Z, rAxis))
            End If
        End Function
        Public Function TopMost(Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDefPtType As dxxEntDefPointTypes = dxxEntDefPointTypes.HandlePt, Optional aPrecis As Integer = 3, Optional bReturnClone As Boolean = False, Optional aCS As dxfPlane = Nothing) As dxfEntity
            Return GetByPoint(dxxPointFilters.AtMaxY, aPlane:=aCS, aPrecis:=aPrecis, bReturnClone:=bReturnClone, bRemove:=False, aEntPointType:=aDefPtType, aEntityType:=aEntityType)

        End Function
        Public Function TotalArea() As Double
            Dim _rVal As Double = 0.0
            '^returns the summation of the lengths of all the members
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                _rVal += aEnt.Area()
            Next i
            Return _rVal
        End Function
        Public Function TotalLength() As Double
            Dim _rVal As Double = 0.0
            '^returns the summation of the lengths of all the members
            Dim aEnt As dxfEntity
            For i As Integer = 1 To Count
                aEnt = Item(i)
                _rVal += aEnt.Length
            Next i
            Return _rVal
        End Function
        Public Function TrailingMembers(aMember As dxfEntity, Optional bIncludeTheMember As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities With {.MaintainIndices = False}
            '#1the member to start collection after
            '#2flag to include the member in the returned collection
            '^returns the members from the collection which follow the passed member
            If aMember Is Nothing Then Return _rVal
            Dim bKeep As Boolean
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aMem Is aMember Then
                    bKeep = True
                    If bIncludeTheMember Then _rVal.Add(aMem)
                Else
                    If bKeep Then _rVal.Add(aMem)
                End If
            Next i
            Return _rVal
        End Function
        Friend Function Transform(aTransforms As TTRANSFORMS, Optional bSuppressEvnts As Boolean = False, Optional aEventName As String = "") As Boolean
            Dim _rVal As Boolean = False
            If Count <= 0 Or aTransforms.Count <= 0 Then Return _rVal
            Dim bUndo As Boolean
            For Each aMem As dxfEntity In Me
                If TTRANSFORMS.Apply(aTransforms, aMem, True) Then _rVal = True
            Next

            If Not _rVal Or SuppressEvents Or bSuppressEvnts Then Return _rVal

            'If OwnerGUID <> "" Then
            '    goEvents.DirtyOwner(Nothing, OwnerGUID)
            'End If

            RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, ToList(), bUndo, False, False)
            If bUndo Then
                Dim aTrs As TTRANSFORMS = TTRANSFORMS.Invert(aTransforms)
                For Each aMem As dxfEntity In Me
                    If TTRANSFORMS.Apply(aTransforms, aMem, True) Then _rVal = True
                Next
                _rVal = False
            End If

            Return _rVal
        End Function
        Friend Function Transform(aTransform As TTRANSFORM, Optional bSuppressEvnts As Boolean = False, Optional aEventName As String = "") As Boolean
            Dim _rVal As Boolean = False
            If Count <= 0 Or aTransform.IsUndefined Then Return _rVal
            Dim bUndo As Boolean
            For Each ent In Me
                'If ent.GraphicType = dxxGraphicTypes.Leader Then
                '    Beep()
                'End If
                If TTRANSFORM.Apply(aTransform, ent, bSuppressEvnts) Then _rVal = True
            Next

            If _rVal Then
                'If OwnerGUID <> "" Then
                '    goEvents.DirtyOwner(Nothing, OwnerGUID)
                'End If
                If Not SuppressEvents And Not bSuppressEvnts Then
                    RaiseEntitiesChange(dxxCollectionEventTypes.CollectionMove, ToList(), bUndo, False, False)
                End If
                If bUndo Then
                    aTransform.Invert()
                    For i As Integer = 1 To Count
                        TTRANSFORM.Apply(aTransform, MyBase.Item(i - 1), True)
                    Next i
                    _rVal = False
                End If
            End If
            Return _rVal
        End Function
        Public Function Translate(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aPlane As dxfPlane = Nothing) As Boolean
            If aX = 0 And aY = 0 And aZ = 0 Then Return False
            Return Transform(TTRANSFORM.CreateTranslation(aX, aY, aZ, aPlane), aEventName:="Translate")
        End Function
        Public Function Translate(aTranslation As iVector, Optional aPlane As dxfPlane = Nothing) As Boolean
            Return Transform(TTRANSFORM.CreateTranslation(aTranslation.X, aTranslation.Y, aTranslation.Z, aPlane), aEventName:="Translate")
        End Function
        Friend Function TranslateV(aTranslation As TVECTOR, Optional aPlane As dxfPlane = Nothing, Optional bSuppressEvnts As Boolean = False) As Boolean
            Return Transform(TTRANSFORM.CreateTranslation(aTranslation, aPlane:=aPlane), aEventName:="Translate")
        End Function
        Public Function TryGet(aHandleOrNameOrGUID As String, ByRef rEntity As dxfEntity) As Boolean
            rEntity = Nothing
            If String.IsNullOrWhiteSpace(aHandleOrNameOrGUID) Then Return Nothing
            rEntity = Find(Function(mem) mem.GUID = aHandleOrNameOrGUID Or mem.Handle = aHandleOrNameOrGUID Or mem.Name.ToUpper = aHandleOrNameOrGUID.ToUpper)
            Return rEntity IsNot Nothing
        End Function
        Public Function TypeCount(aType As dxxGraphicTypes, Optional bInvertCount As Boolean = False) As Integer
            Dim _rVal As Integer = 0
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If Not bInvertCount Then
                    If aMem.GraphicType = aType Then _rVal += 1
                Else
                    If aMem.GraphicType <> aType Then _rVal += 1
                End If
            Next i
            Return _rVal
        End Function
        Public Sub UnSuppress(Optional aTagToUnSuppress As String = Nothing)
            Dim bTest As Boolean = aTagToUnSuppress IsNot Nothing

            For i As Integer = 1 To Count
                Dim aEnt As dxfEntity = Item(i)
                If Not bTest Then
                    aEnt.Suppressed = False
                Else
                    If String.Compare(aEnt.Tag, aTagToUnSuppress, True) = 0 Then
                        aEnt.Suppressed = False
                    End If
                End If
            Next i
        End Sub
        Public Function GetNestedInserts(Optional aImage As dxfImage = Nothing) As List(Of dxeInsert)
            If aImage Is Nothing Then aImage = MyImage
            If aImage Is Nothing Then Return New List(Of dxeInsert)
            Return dxfEntities.GetNestedInserts(Me, aImage)
        End Function
        Public Function GetInstanceMemberPoints(Optional bReturnBasePts As Boolean = True, Optional aEntityType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aSearchTag As String = Nothing, Optional aSearchFlag As String = Nothing) As colDXFVectors
            Return New colDXFVectors(dxfEntities.GetInstanceMemberPoints(Me, bReturnBasePts, aEntityType, aSearchTag, aSearchFlag))
        End Function
        Public Function UpdateImage(Optional bErase As Boolean = True) As Boolean
            Dim _rVal As Boolean = False
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                aMem.UpdateImage(bErase)
            Next i
            Return _rVal
        End Function
        Public Sub UpdatePaths(Optional bRegen As Boolean = False)
            '#1flag to force a regeneration of the entities path
            '^updates the entities basic path object to reflect the current properties
            '~the path is recomputed if it is dirty or the passed flag is True
            'On Error Resume Next
            Dim aEnt As dxfEntity
            'Dim bSetReferenceHandle As Boolean

            Dim aImage As dxfImage = MyImage
            If aImage Is Nothing And Not String.IsNullOrWhiteSpace(ImageGUID) Then
                aImage = dxfEvents.GetImage(ImageGUID)
                SetImage(aImage, False)
            End If
            If aImage Is Nothing Then Return

            For i As Integer = 1 To Count
                aEnt = Item(i)
                aEnt.UpdatePath(bRegen, aImage)
            Next i
        End Sub
        Public Function ValueList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                TLISTS.Add(_rVal, aMem.Value, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function Values(Optional bUniqueOnly As Boolean = False) As List(Of Object)
            Dim _rVal As New List(Of Object)
            '#1flag to return only the unique values
            '^returns a collection of strings containing the values of the members
            Dim aMem As dxfEntity
            Dim bKeep As Boolean
            For i As Integer = 1 To Count
                aMem = Item(i)
                bKeep = True
                If bUniqueOnly Then
                    bKeep = _rVal.FindIndex(Function(mem) mem = aMem.Value) < 0
                End If
                If bKeep Then _rVal.Add(aMem.Value)
            Next i
            Return _rVal
        End Function
        Public Function VerticalLines(Optional aTag As String = "", Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 4) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1a tag to filter for
            '^returns only the horizontal lines in the collection
            Dim aEnt As dxfEntity
            Dim aLine As dxeLine
            For i As Integer = 1 To Count
                aEnt = Item(i)
                If aEnt.EntityType = dxxEntityTypes.Line Then
                    aLine = aEnt
                    If aLine.IsVertical(aCS, aPrecis) Then
                        If aTag = "" Or (aTag <> "" And String.Compare(aLine.Tag, aTag, ignoreCase:=True) = 0) Then
                            _rVal.Add(aLine)
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function Vertices(Optional bReturnClones As Boolean = False, Optional aGraphicType As dxxGraphicTypes = dxxGraphicTypes.Undefined) As colDXFVectors
            Dim _rVal As New colDXFVectors
            'On Error Resume Next
            Dim aMem As dxfEntity
            For i As Integer = 1 To Count
                aMem = Item(i)
                If aGraphicType = dxxGraphicTypes.Undefined Or aMem.GraphicType = aGraphicType Then
                    _rVal.Append(aMem.Vertices, bReturnClones)
                End If
            Next i
            Return _rVal
        End Function
        Friend Function zDirtyText(aTextStyle As dxoStyle, aImage As dxfImage) As Boolean
            Dim _rVal As Boolean = False
            If aTextStyle Is Nothing Then Return _rVal
            Dim aTxt As dxeText
            Dim aDim As dxeDimension
            Dim aLdr As dxeLeader
            Dim aSym As dxeSymbol
            Dim aTbl As dxeTable
            Dim aEnt As dxfEntity
            Dim sName As String
            Dim aIns As dxeInsert
            Dim aBlk As dxfBlock
            sName = aTextStyle.Name
            For i As Integer = 1 To Count
                aEnt = Item(i)
                Select Case aEnt.GraphicType
                    Case dxxGraphicTypes.Insert
                        aIns = aEnt
                        aBlk = aImage.Blocks.GetByName(aIns.BlockName)
                        If aBlk IsNot Nothing Then
                            If aBlk.Entities.zDirtyText(aTextStyle, aImage) Then
                                aIns.IsDirty = True
                                _rVal = True
                            End If
                        End If
                    Case dxxGraphicTypes.Text
                        aTxt = aEnt
                        If String.Compare(aTxt.TextStyleName, sName, ignoreCase:=True) = 0 Then
                            aTxt.IsDirty = True
                            _rVal = True
                        End If
                    Case dxxGraphicTypes.Table
                        aTbl = aEnt
                        If String.Compare(aTbl.HeaderTextStyle, sName, ignoreCase:=True) = 0 Then
                            aTbl.IsDirty = True
                            _rVal = True
                        End If
                        If String.Compare(aTbl.FooterTextStyle, sName, ignoreCase:=True) = 0 Then
                            aTbl.IsDirty = True
                            _rVal = True
                        End If
                        If String.Compare(aTbl.TextStyleName, sName, ignoreCase:=True) = 0 Then
                            aTbl.IsDirty = True
                            _rVal = True
                        End If
                        If String.Compare(aTbl.TitleTextStyle, sName, ignoreCase:=True) = 0 Then
                            aTbl.IsDirty = True
                            _rVal = True
                        End If
                    Case dxxGraphicTypes.Dimension
                        aDim = aEnt
                        If String.Compare(aDim.TextStyleName, sName, ignoreCase:=True) = 0 Then
                            aDim.IsDirty = True
                            _rVal = True
                        End If
                    Case dxxGraphicTypes.Leader
                        aLdr = aEnt
                        If aLdr.LeaderType = dxxLeaderTypes.LeaderText Then
                            If aLdr.MText IsNot Nothing Then aLdr.MText.IsDirty = True
                            If String.Compare(aLdr.TextStyleName, sName, ignoreCase:=True) = 0 Then
                                aLdr.IsDirty = True
                                _rVal = True
                            End If
                        End If
                    Case dxxGraphicTypes.Symbol
                        aSym = aEnt
                        If String.Compare(aSym.TextStyleName, sName, ignoreCase:=True) = 0 Then
                            aSym.IsDirty = True
                            _rVal = True
                        End If
                End Select
            Next i
            Return _rVal
        End Function
        Public Overloads Sub Reverse()
            If Count <= 1 Then Return
            MyBase.Reverse()
            If MaintainIndices Then ReIndex()
        End Sub
        Public Function Reversed() As colDXFEntities
            Dim _rVal As New colDXFEntities(Me)
            _rVal.Reverse()
            Return _rVal
        End Function
        Friend Sub ReleaseReferences()
            OwnerGUID = String.Empty
            _OwnerPointer = Nothing
            BlockGUID = String.Empty
            _BlockPtr = Nothing
            CollectionGUID = String.Empty
            ' _CollectionPtr = Nothing
            ImageGUID = String.Empty
            _ImagePtr = Nothing


            For Each ent As dxfEntity In Me
                'ent.Dispose()
                ent.ReleaseReferences()
            Next
            '_Members.Clear()
            MyBase.Clear()
        End Sub
        Public Function Arcs() As List(Of dxeArc)
            If Count <= 0 Then Return New List(Of dxeArc)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Arc).ConvertAll(Function(ent) CType(ent, dxeArc))
        End Function
        Public Function Lines() As List(Of dxeLine)
            If Count <= 0 Then Return New List(Of dxeLine)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Line).ConvertAll(Function(ent) CType(ent, dxeLine))
        End Function
        Public Function Polylines() As List(Of dxePolyline)
            If Count <= 0 Then Return New List(Of dxePolyline)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Polyline).ConvertAll(Function(ent) CType(ent, dxePolyline))
        End Function
        Public Function Solids() As List(Of dxeSolid)
            If Count <= 0 Then Return New List(Of dxeSolid)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Solid).ConvertAll(Function(ent) CType(ent, dxeSolid))
        End Function
        Public Function Inserts() As List(Of dxeInsert)
            If Count <= 0 Then Return New List(Of dxeInsert)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Insert).ConvertAll(Function(ent) CType(ent, dxeInsert))
        End Function
        Public Function Points() As List(Of dxePoint)
            If Count <= 0 Then Return New List(Of dxePoint)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Point).ConvertAll(Function(ent) CType(ent, dxePoint))
        End Function
        Public Function Texts() As List(Of dxeText)
            If Count <= 0 Then Return New List(Of dxeText)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Text Or mem.GraphicType = dxxGraphicTypes.MText).ConvertAll(Function(ent) CType(ent, dxeText))
        End Function
        Public Function AttDefs() As List(Of dxeText)
            If Count <= 0 Then Return New List(Of dxeText)
            Return FindAll(Function(mem) mem.EntityType = dxxEntityTypes.Attdef).ConvertAll(Function(ent) CType(ent, dxeText))
        End Function
        Public Function Attribs() As List(Of dxeText)
            If Count <= 0 Then Return New List(Of dxeText)
            Return FindAll(Function(mem) mem.EntityType = dxxEntityTypes.Attribute).ConvertAll(Function(ent) CType(ent, dxeText))
        End Function
        Public Function Symbols() As List(Of dxeSymbol)
            If Count <= 0 Then Return New List(Of dxeSymbol)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Symbol).ConvertAll(Function(ent) CType(ent, dxeSymbol))
        End Function
        Public Function Dimensions() As List(Of dxeDimension)
            If Count <= 0 Then Return New List(Of dxeDimension)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Dimension).ConvertAll(Function(ent) CType(ent, dxeDimension))
        End Function
        Public Function Polygons() As List(Of dxePolygon)
            If Count <= 0 Then Return New List(Of dxePolygon)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Polygon).ConvertAll(Function(ent) CType(ent, dxePolygon))
        End Function
        Public Function Beziers() As List(Of dxeBezier)
            If Count <= 0 Then Return New List(Of dxeBezier)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Bezier).ConvertAll(Function(ent) CType(ent, dxeBezier))
        End Function
        Public Function Shapes() As List(Of dxeShape)
            If Count <= 0 Then Return New List(Of dxeShape)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Shape).ConvertAll(Function(ent) CType(ent, dxeShape))
        End Function
        Public Function Ellipses() As List(Of dxeEllipse)
            If Count <= 0 Then Return New List(Of dxeEllipse)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Ellipse).ConvertAll(Function(ent) CType(ent, dxeEllipse))
        End Function
        Public Function Leaders() As List(Of dxeLeader)
            If Count <= 0 Then Return New List(Of dxeLeader)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Leader).ConvertAll(Function(ent) CType(ent, dxeLeader))
        End Function
        Public Function Hatches() As List(Of dxeHatch)
            If Count <= 0 Then Return New List(Of dxeHatch)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Hatch).ConvertAll(Function(ent) CType(ent, dxeHatch))
        End Function
        Public Function Holes() As List(Of dxeHole)
            If Count <= 0 Then Return New List(Of dxeHole)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Hole).ConvertAll(Function(ent) CType(ent, dxeHole))
        End Function
        Public Function Tables() As List(Of dxeTable)
            If Count <= 0 Then Return New List(Of dxeTable)
            Return FindAll(Function(mem) mem.GraphicType = dxxGraphicTypes.Table).ConvertAll(Function(ent) CType(ent, dxeTable))
        End Function
#End Region 'Methods
#Region "oEvents_EventHandlers"
        'Private Sub _Events_EntitiesRequest(aGUID As String, ByRef rEntities As colDXFEntities)
        '    If aGUID = GUID Then rEntities = Me
        '    If String.IsNullOrWhiteSpace(GUID) Then
        '        If _Events IsNOt Nothing Then RemoveHandler _Events.EntititiesRequest, AddressOf _Events_EntitiesRequest
        '        _Events = Nothing
        '    End If
        'End Sub
#End Region 'oEvents_EventHandlers"
#Region "IDisposable Implementation"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    'If _Events IsNOt Nothing Then RemoveHandler _Events.EntititiesRequest, AddressOf _Events_EntitiesRequest
                    '_Events = Nothing
                    ' dispose managed state (managed objects)
                    'Clear(True, MyImage)
                    ReleaseReferences()

                    For Each ent As dxfEntity In Me
                        ent.Dispose()
                    Next
                    MyBase.Clear()
                End If
                'free unmanaged resources (unmanaged objects) and override finalizer
                'set large fields to null
                disposedValue = True
            End If
        End Sub
        ' override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
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


#Region "iHandleOwner"

        Public Function GetImage(ByRef aImage As dxfImage) As Boolean
            If aImage Is Nothing Then aImage = MyImage
            Return aImage IsNot Nothing

        End Function

        Friend ReadOnly Property HasReferenceTo_Block As Boolean
            Get
                If String.IsNullOrWhiteSpace(BlockGUID) Or _BlockPtr Is Nothing Then Return False
                Dim target As dxfBlock = Nothing
                Dim _rVal As Boolean = _BlockPtr.TryGetTarget(target)
                If Not _rVal Then
                    _BlockPtr = Nothing

                End If
                Return _rVal
            End Get
        End Property

        Friend ReadOnly Property HasReferenceTo_Image As Boolean
            Get
                If String.IsNullOrWhiteSpace(ImageGUID) Or _ImagePtr Is Nothing Then Return False
                Dim target As dxfImage = Nothing
                Dim _rVal As Boolean = _ImagePtr.TryGetTarget(target)
                If Not _rVal Then
                    _ImagePtr = Nothing
                Else
                    If target.Disposed Then
                        _rVal = False
                        _ImagePtr = Nothing
                    End If
                End If
                Return _rVal
            End Get
        End Property
        Friend ReadOnly Property HasReferenceTo_Owner As Boolean
            Get
                If String.IsNullOrWhiteSpace(OwnerGUID) Or _OwnerPointer Is Nothing Then Return False
                Dim target As dxfHandleOwner = Nothing
                Dim _rVal As Boolean = _OwnerPointer.TryGetTarget(target)
                If Not _rVal Then
                    _ImagePtr = Nothing
                End If
                Return _rVal
            End Get
        End Property
        Friend Overridable ReadOnly Property MyOwner As dxfHandleOwner
            Get
                If Not HasReferenceTo_Owner Then Return Nothing
                Dim _rVal As dxfHandleOwner = Nothing
                If Not _OwnerPointer.TryGetTarget(_rVal) Then
                    _OwnerPointer = Nothing
                    Return Nothing
                End If
                If String.Compare(OwnerGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetOwner(Nothing, False)
                Return _rVal
            End Get
        End Property

        Friend ReadOnly Property MyImage As dxfImage
            Get

                If Not HasReferenceTo_Image Then Return Nothing
                Dim _rVal As dxfImage = Nothing
                Try
                    _ImagePtr.TryGetTarget(_rVal)
                    If _rVal IsNot Nothing AndAlso _rVal.Disposed Then _rVal = Nothing
                    If _rVal IsNot Nothing Then
                        If String.IsNullOrWhiteSpace(ImageGUID) Or String.Compare(ImageGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetImage(Nothing, False)
                    End If
                Catch ex As Exception
                    _ImagePtr = Nothing

                End Try

                Return _rVal
            End Get
        End Property

        Friend Overridable ReadOnly Property MyBlock As dxfBlock
            Get
                If Not HasReferenceTo_Block Then Return Nothing
                Dim _rVal As dxfBlock = Nothing
                If Not _BlockPtr.TryGetTarget(_rVal) Then
                    _BlockPtr = Nothing
                    Return Nothing
                End If
                If String.Compare(BlockGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetBlock(Nothing, False)
                Return _rVal
            End Get
        End Property

        Friend Sub SetBlock(aBlock As dxfBlock, bDontReleaseOnNull As Boolean)
            If aBlock IsNot Nothing Then
                BlockGUID = aBlock.GUID
                _BlockPtr = New WeakReference(Of dxfBlock)(aBlock)
                If aBlock.HasReferenceTo_Image Then
                    SetImage(aBlock.Image, True)
                End If
            Else
                If Not bDontReleaseOnNull Then
                    BlockGUID = ""
                    _BlockPtr = Nothing
                End If
            End If
        End Sub

        Dim _ImagePtr As WeakReference(Of dxfImage) = Nothing
        Dim _BlockPtr As WeakReference(Of dxfBlock) = Nothing
        Dim _OwnerPointer As WeakReference(Of dxfHandleOwner) = Nothing
        Friend Sub SetOwner(aOwner As dxfHandleOwner, bDontReleaseOnNull As Boolean)
            If aOwner IsNot Nothing Then
                OwnerType = aOwner.FileObjectType
                OwnerGUID = aOwner.GUID
                _OwnerPointer = New WeakReference(Of dxfHandleOwner)(aOwner)
            Else
                If Not bDontReleaseOnNull Then
                    OwnerType = dxxFileObjectTypes.Undefined
                    OwnerGUID = ""
                    _OwnerPointer = Nothing
                End If
            End If
        End Sub

        Friend Function SetImage(aImage As dxfImage, bDontReleaseOnNull As Boolean) As Boolean
            Dim img As dxfImage = aImage
            If img IsNot Nothing AndAlso img.Disposed Then
                img = Nothing
                Return False
            End If
            If img IsNot Nothing Then
                ImageGUID = img.GUID
                _ImagePtr = New WeakReference(Of dxfImage)(img)
                Return True
            Else
                If Not bDontReleaseOnNull Then
                    ImageGUID = ""
                    _ImagePtr = Nothing
                End If
                Return False
            End If
        End Function

        Private _Handlez As New THANDLES
        Public Property ReactorGUID As String Implements iHandleOwner.ReactorGUID
            Get
                Return _Handlez.ReactorGUID
            End Get
            Set(value As String)
                _Handlez.ReactorGUID = value
            End Set
        End Property

        Public Property BlockGUID As String Implements iHandleOwner.BlockGUID
            Get
                Return _Handlez.BlockGUID
            End Get
            Set(value As String)
                _Handlez.BlockGUID = value
            End Set
        End Property

        Public Property CollectionGUID As String Implements iHandleOwner.CollectionGUID
            Get
                Return _Handlez.CollectionGUID
            End Get
            Set(value As String)
                _Handlez.CollectionGUID = value
            End Set
        End Property

        Public Property BlockCollectionGUID As String Implements iHandleOwner.BlockCollectionGUID
            Get
                Return _Handlez.BlockCollectionGUID
            End Get
            Set(value As String)
                _Handlez.BlockCollectionGUID = value
            End Set
        End Property

        Public Property Handle As String Implements iHandleOwner.Handle
            Get
                Return _Handlez.Handle
            End Get
            Set(value As String)
                _Handlez.Handle = value

            End Set
        End Property

        Public Property Image As dxfImage
            '^the parent image asssociated to this entity
            Get
                If String.IsNullOrWhiteSpace(ImageGUID) Then Return Nothing
                Dim img As dxfImage = MyImage
                If img IsNot Nothing Then Return img
                img = dxfEvents.GetImage(ImageGUID)
                SetImage(img, False)
                Return img
            End Get
            Set(value As dxfImage)
                If value IsNot Nothing Then
                    ImageGUID = value.GUID
                Else
                    ImageGUID = ""
                End If
            End Set
        End Property

        Public Property ImageGUID As String Implements iHandleOwner.ImageGUID
            Get
                Return _Handlez.ImageGUID
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _Handlez.ImageGUID = value
                If Not String.IsNullOrWhiteSpace(value) Then
                    Dim img As dxfImage = dxfEvents.GetImage(value)
                    If img Is Nothing Then
                        _Handlez.ImageGUID = String.Empty
                        _ImagePtr = Nothing
                    Else
                        _ImagePtr = New WeakReference(Of dxfImage)(img)
                    End If
                    MonitorMembers = img IsNot Nothing
                Else
                    _ImagePtr = Nothing
                    MonitorMembers = False
                End If

            End Set
        End Property

        Public Property Index As Integer Implements iHandleOwner.Index
            Get
                Return _Handlez.Index
            End Get
            Set(value As Integer)
                _Handlez.Index = value

            End Set
        End Property

        Public Property OwnerGUID As String Implements iHandleOwner.OwnerGUID
            Get
                Return _Handlez.OwnerGUID
            End Get
            Set(value As String)
                _Handlez.OwnerGUID = value
            End Set
        End Property

        Public Property SourceGUID As String Implements iHandleOwner.SourceGUID
            Get
                Return _Handlez.SourceGUID
            End Get
            Set(value As String)
                _Handlez.SourceGUID = value
            End Set
        End Property

        Public Property Domain As dxxDrawingDomains Implements iHandleOwner.Domain
            Get
                Return _Handlez.Domain
            End Get
            Set(value As dxxDrawingDomains)
                _Handlez.Domain = value
            End Set
        End Property

        Public Property Identifier As String Implements iHandleOwner.Identifier
            Get
                Return _Handlez.Identifier
            End Get
            Set(value As String)
                _Handlez.Identifier = value
            End Set
        End Property

        Public Property ObjectType As dxxFileObjectTypes Implements iHandleOwner.ObjectType
            Get
                Return dxxFileObjectTypes.Blocks
            End Get
            Set(value As dxxFileObjectTypes)
                value = dxxFileObjectTypes.Block
            End Set
        End Property

        Public Property OwnerType As dxxFileObjectTypes Implements iHandleOwner.OwnerType
            Get
                Return _Handlez.OwnerType
            End Get
            Set(value As dxxFileObjectTypes)
                _Handlez.OwnerType = value
            End Set
        End Property

        Public Property Name As String Implements iHandleOwner.Name
            Get
                Return _Handlez.Name
            End Get
            Friend Set(value As String)
                _Handlez.Name = value
            End Set
        End Property

        Public Property GUID As String Implements iHandleOwner.GUID
            Get
                Return _Handlez.GUID
            End Get
            Friend Set(value As String)
                _Handlez.GUID = value

            End Set
        End Property


        Friend Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix Implements iHandleOwner.DXFFileProperties
            Throw New NotImplementedException
        End Function

        Friend Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As dxfPlane, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As dxoPropertyArray Implements iHandleOwner.DXFProps
            Dim _rVal As New dxoPropertyArray
            Dim ocs As New TPLANE(aOCS)
            For Each ent In Me
                _rVal.Append(ent.DXFProps(aInstances, aInstance, ocs, rTypeName, aImage))
            Next
            Return _rVal

        End Function

        Public ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.Entities
            End Get
        End Property
        Friend Property Suppressed As Boolean Implements iHandleOwner.Suppressed
            Get
                Return _Suppressed
            End Get
            Set(value As Boolean)
                _Suppressed = value
            End Set
        End Property

        Friend Overridable Sub ProcessEvent(aEvent As dxfEvent)
            If aEvent Is Nothing Then Return
            'Select Case aEvent.BaseType
            '    Case dxxEventTypes.EntityCollection
            '        Dim evnt As dxfEntitiesEvent = TryCast(aEvent, dxfEntitiesEvent)
            '        RespondToEntitiesChangeEvent(evnt)
            '    Case dxxEventTypes.PropertyChange
            '        Dim evnt As dxfPropertyChangeEvent = TryCast(aEvent, dxfPropertyChangeEvent)
            '        RespondToDimStylePropertyChange(evnt)
            '    Case dxxEventTypes.Vertex
            '        Dim evnt As dxfVertexEvent = TryCast(aEvent, dxfVertexEvent)
            '        RespondToVectorsMemberChange(evnt)
            '    Case dxxEventTypes.DefPt
            '        Dim evnt As dxfDefPtEvent = TryCast(aEvent, dxfDefPtEvent)
            '        RespondToDefPtChange(evnt)
            '    Case dxxEventTypes.PlaneChange
            '        Dim evnt As dxfPlaneEvent = TryCast(aEvent, dxfPlaneEvent)
            '        RespondToPlaneChangeEvent(evnt)
            'End Select
        End Sub

#End Region 'iHandleOwner



#Region "Shared Methods"


#End Region 'Shared Methods
    End Class 'colDXFEntities
End Namespace
