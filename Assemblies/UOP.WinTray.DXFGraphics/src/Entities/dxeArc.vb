Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxeArc
        Inherits dxfArc
        Implements iPolylineSegment
        Implements iArc

#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxArcTypes.Arc)
        End Sub

        Public Sub New(aEntity As iArc, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxArcTypes.Arc)
            If aEntity IsNot Nothing Then
                If aEntity.GetType() Is GetType(dxeArc) Then
                    Dim ent As dxfEntity = DirectCast(aEntity, dxfEntity)
                    Copy(ent, bCloneInstances:=bCloneInstances)
                    SetPropVal("*SourceGUID", ent.GUID)
                Else
                    Plane = aEntity.Plane
                    CenterV = New TVECTOR(aEntity.Center)
                    Radius = aEntity.Radius
                    StartAngle = aEntity.StartAngle
                    EndAngle = aEntity.EndAngle
                End If
            End If
            If aDisplaySettings IsNot Nothing Then DisplayStructure = aDisplaySettings.Strukture
        End Sub

        Public Sub New(aCenter As iVector, aRadius As Double, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 0, Optional bClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxArcTypes.Arc, aDisplaySettings)
            If aEndAngle = aStartAngle Then aEndAngle = TVALUES.NormAng(aStartAngle + 360)
            Define(New TVECTOR(aCenter), aRadius, aStartAngle, aEndAngle, bClockwise, aPlane:=aPlane)
        End Sub
        Friend Sub New(aSegment As TSEGMENT)
            MyBase.New(dxxArcTypes.Arc)
            SetArcLineStructure(aSegment)
        End Sub
        Friend Sub New(aArc As TARC, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxArcTypes.Arc, aDisplaySettings)
            ArcStructure = aArc
        End Sub
        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxArcTypes.Arc)
            DefineByObject(aObject, False, TTABLEENTRY.Null)
        End Sub



        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(dxxArcTypes.Arc, aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aDisplayVars As TDISPLAYVARS, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aIdentifier As String = "")
            MyBase.New(dxxArcTypes.Arc, New dxfDisplaySettings(aDisplayVars))
            If aColor <> dxxColors.Undefined Then Color = aColor
            If aLineType <> "" Then Linetype = aLineType
            Identifier = aIdentifier
        End Sub
        Friend Sub New(aCenterPt As TVECTOR, aStartPt As TVECTOR, aEndPt As TVECTOR, bClockwise As Boolean, aPlane As TPLANE)
            MyBase.New(dxxArcTypes.Arc)
            ArcStructure = New TARC(aPlane, aCenterPt, aStartPt, aEndPt, bClockwise)
        End Sub
        Friend Sub New(aPlane As TPLANE, aRadius As Double, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 360, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxArcTypes.Arc, aDisplaySettings)
            CenterV = aPlane.Origin
            If Not TPLANE.IsNull(aPlane) Then PlaneV = aPlane
            If aRadius <> 0 Then Radius = Math.Abs(aRadius)
            StartAngle = aStartAngle
            EndAngle = aEndAngle
        End Sub
        Friend Sub New(aPlane As dxfPlane, aRadius As Double, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 360, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxArcTypes.Arc, aDisplaySettings)
            Dim plane As New TPLANE(aPlane)
            CenterV = plane.Origin
            If aRadius <> 0 Then Radius = Math.Abs(aRadius)
            StartAngle = aStartAngle
            EndAngle = aEndAngle
        End Sub
#End Region 'Constructors
#Region "Properties"
        ''' <summary>
        ''' the bulge value for the arc  = tan(spanned angle/4)
        ''' </summary>
        ''' <remarks>negative if counterclockwise. this value is used in autocad to define an arc polyline segment</remarks>
        ''' <returns></returns>
        Public ReadOnly Property Bulge As Double
            Get
                Return Math.Tan((SpannedAngle * Math.PI / 180) / 4) * BulgeFactor
            End Get
        End Property

        ''' <summary>
        ''' the factor applied to the returned bulge to invert the arc. 1 or -1.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BulgeFactor As Integer
            Get
                If ClockWise Then Return -1 Else Return 1
            End Get
        End Property

        Public Overrides ReadOnly Property EntityType As dxxEntityTypes
            Get
                If Math.Abs(SpannedAngle) <= 359.99 Then Return dxxEntityTypes.Arc Else Return dxxEntityTypes.Circle
            End Get
            'Friend Set(value As dxxEntityTypes)
            '    'MyBase.EntityType = value
            'End Set
        End Property

        Public Property EndWidth As Double
            Get
                Return PropValueD("*EndWidth")
            End Get
            Set(value As Double)
                SetPropVal("*EndWidth", Math.Round(Math.Abs(value), 8), True)
            End Set
        End Property
        Public ReadOnly Property HasWidth As Boolean
            Get
                '^returns True if the entity has a start or end with defined
                Return StartWidth > 0 Or EndWidth > 0
            End Get
        End Property



        Public Property StartWidth As Double
            Get
                Return PropValueD("*StartWidth")
            End Get
            Set(value As Double)
                SetPropVal("*StartWidth", Math.Round(Math.Abs(value), 8), True)
            End Set
        End Property

        Public ReadOnly Property SegmentType As dxxSegmentTypes Implements iPolylineSegment.SegmentType
            Get
                Return dxxSegmentTypes.Arc
            End Get
        End Property

        Private ReadOnly Property iArc_Plane As dxfPlane Implements iArc.Plane
            Get
                Return Plane
            End Get
        End Property

        Private Property iArc_Radius As Double Implements iArc.Radius
            Get
                Return Radius
            End Get
            Set(value As Double)
                Radius = value
            End Set
        End Property

        Private Property iArc_StartAngle As Double Implements iArc.StartAngle
            Get
                Return StartAngle
            End Get
            Set(value As Double)
                StartAngle = value
            End Set
        End Property

        Private Property iArc_EndAngle As Double Implements iArc.EndAngle
            Get
                Return EndAngle
            End Get
            Set(value As Double)
                EndAngle = value
            End Set
        End Property

        Private Property iArc_Center As iVector Implements iArc.Center
            Get
                Return Center
            End Get
            Set(value As iVector)
                Center = New dxfVector(value)
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"
        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeArc
            Dim _rVal As dxeArc = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeArc
            Return New dxeArc(Me)
        End Function
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            '    Structure = ent_DefineProperties(Structure, aObj.Properties, bNoHandle)
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210))
            DisplayStructure = aObj.DisplayVars
            PlaneV = aPlane
            CenterV = aPlane.WorldVector(aObj.Properties.GCValueV(10))
            Radius = aObj.Properties.GCValueD(40)
            If String.Compare(aObj.Properties.GCValueStr(0, "ARC"), "ARC", True) = 0 Then
                StartAngle = aObj.Properties.GCValueD(50, StartAngle)
                EndAngle = aObj.Properties.GCValueD(51, EndAngle)
            End If
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5, Handle)
        End Sub


#End Region 'MustOverride Entity Methods
#Region "Methods"



        Public Function Break(aBreaker As Object, bBreakersAreInfinite As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            '#1the entity or entities to use to break the arc
            '#2flag indicating that the breaker(s) are of infinite length
            '#3returns the points of intersection that were used for the break points
            '^returns the arc broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Arc(Me, aBreaker, bBreakersAreInfinite, aIntersects)
        End Function

        Public Function Break(aBreaker As Object, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            '#1the entity or entities to use to break the arc
            '#2flag indicating that the breaker(s) are of infinite length
            '#3returns true if the arc was broken
            '#4returns the points of intersection that were used for the break points
            '^returns the arc broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Arc(Me, aBreaker, bBreakersAreInfinite, rWasBroken, aIntersects)
        End Function
        Public Function ChordLine(aOffset As Double, Optional aRotation As Double = 0, Optional aScaler As Double = 0) As dxeLine
            '#1the offset from center to make the chord line
            '#2an optional rotation to apply
            '^returns a line that defines a chord on the arc.
            '^Nothing is returned if the offst is greater that the radius
            Dim rad As Double = Radius
            If Math.Abs(aOffset) >= rad Then Return Nothing
            Dim _rVal As New dxeLine With {.StartPt = Plane.Vector(-Math.Sqrt(rad ^ 2 - aOffset ^ 2), aOffset), .EndPt = Plane.Vector(Math.Sqrt(rad ^ 2 - aOffset ^ 2), aOffset)}
            If aRotation <> 0 Then _rVal.RotateAbout(Plane.ZAxis, aRotation)
            If aScaler <> 0 Then _rVal.Rescale(aScaler, _rVal.MidPt)
            _rVal.CopyDisplayValues(Me.DisplaySettings)
            Return _rVal
        End Function
        Public Function ClearWidth() As Boolean
            If StartWidth = 0 And EndWidth = 0 Then Return False
            '^resets the start and end width to 0
            StartWidth = 0
            EndWidth = 0
            IsDirty = True
            Return True
        End Function

        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aOCS As New TPLANE("")
            Dim tname As String = String.Empty
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            If StartWidth = 0 And EndWidth = 0 Then
                For i = 1 To iCnt
                    If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname, aImage))
                Next i
                If iCnt > 1 Then
                    _rVal.Name = tname & "-" & iCnt & " INSTANCES"
                End If
            Else
                aInstances.IsParent = True
                aInstances.ParentPlane = PlaneV
                For i = 1 To iCnt
                    If aInstance <= 0 Or i = aInstance Then _rVal.Add(ConvertToPolyline.DXFProps(aInstances, i, aOCS, tname))
                Next i
                If iCnt > 1 Then
                    _rVal.Name = "WIDEARC" & "-" & (iCnt) & " INSTANCES"
                Else
                    _rVal.Name = "WIDEARC"
                End If
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            'On Error Resume Next
            aInstance = Math.Abs(aInstance)
            If aInstance <= 0 Then aInstance = 1
            Dim _rVal As New TPROPERTYARRAY(aInstance:=aInstance)
            If Radius <= 0 Then Return _rVal
            Dim myProps As New TPROPERTIES(ActiveProperties())
            Dim aTrs As TTRANSFORMS
            Dim aPl As TPLANE = PlaneV
            Dim bPl As TPLANE
            Dim scl As Double
            Dim ang As Double
            Dim bInv As Boolean
            Dim bLft As Boolean
            scl = 1
            ang = 0
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl)
                rTypeName = Trim(myProps.Item(1).Value)
                '        If SpannedAngle >= 359.98 Then rTypeName = "CIRCLE" Else rTypeName = "ARC"
            Else
                myProps.Handle = Handle
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                myProps.SetVectorGC(210, aOCS.ZDirection, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                If SpannedAngle >= 359.98 Then
                    rTypeName = "CIRCLE"
                Else
                    rTypeName = "ARC"
                End If
                MyBase.SetProps(myProps)
                MyBase.UpdateCommonProperties(rTypeName)
                myProps = New TPROPERTIES(MyBase.Properties)
                myProps.SetVal("Entity Type", rTypeName)
            End If
            Dim cp As TVECTOR
            Dim ep As TVECTOR
            Dim sp As TVECTOR
            Dim rad As Double
            'Dim saidx As Integer
            'Dim eaidx As Integer
            Dim sa As Double
            Dim ea As Double
            rad = Radius * Math.Abs(scl)
            'If Not ClockWise Then
            '    saidx = 7
            '    eaidx = 8
            'Else
            '    saidx = 8
            '    eaidx = 7
            'End If
            myProps.SetVal("Radius", rad)
            cp = aPl.Origin '= aPl.Origin.WithRespectTo( aOCS)
            myProps.SetVector("Center", cp)
            If rTypeName = "ARC" Then
                aPl.Origin = CenterV
                PlaneV = aPl
                bPl = aOCS
                bPl.Origin = aPl.Origin
                sa = StartAngle
                ea = EndAngle
                sp = aPl.AngleVector(sa, rad, False)
                ep = aPl.AngleVector(ea, rad, False)
                sa = bPl.XDirection.AngleTo(bPl.Origin.DirectionTo(sp), bPl.ZDirection)
                ea = bPl.XDirection.AngleTo(bPl.Origin.DirectionTo(ep), bPl.ZDirection)
                myProps.SetVal("Start Angle", sa)
                myProps.SetVal("End Angle", ea)
                myProps.SetSuppressed("Entity Sub Class Marker_2,Start Angle,End Angle", ",", False)
            Else
                myProps.SetSuppressed("Entity Sub Class Marker_2,Start Angle,End Angle", ",", True)
            End If
            _rVal.Add(myProps, rTypeName, True, True)
            Return _rVal
        End Function
        Public Sub Define(aCenter As iVector, aRadius As Double, aStartAngle As Double, aEndAngle As Double, Optional cClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing)
            Try
                If aRadius <= 0 Then Throw New Exception("The Passed Radius Is Invalid")
                aStartAngle = TVALUES.NormAng(aStartAngle, bReturnPosive:=True)
                aEndAngle = TVALUES.NormAng(aEndAngle, False, False)
                If aEndAngle = 0 Then aEndAngle = 360
                If aStartAngle = aEndAngle Then Throw New Exception("The Passed Angles Are Invalid")
                If Not dxfPlane.IsNull(aPlane) Then
                    PlaneV = New TPLANE(aPlane)
                End If
                Center.Strukture = New TVECTOR(aPlane:=Plane, aCenter)
                Radius = aRadius
                StartAngle = aStartAngle
                EndAngle = aEndAngle
                ClockWise = cClockwise
            Catch ex As Exception
                Throw New Exception($"dxeArc.Define - { ex.Message}")
            End Try
        End Sub
        Friend Sub Define(aCenter As TVECTOR, aRadius As Double, aStartAngle As Double, aEndAngle As Double, Optional cClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing)
            Try
                If aRadius <= 0 Then Throw New Exception("The Passed Radius Is Invalid")
                aStartAngle = TVALUES.NormAng(aStartAngle, bReturnPosive:=True)
                aEndAngle = TVALUES.NormAng(aEndAngle, False, False)
                If aEndAngle = 0 Then aEndAngle = 360
                If aStartAngle = aEndAngle Then Throw New Exception("he Passed Angles Are Invalid")
                If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
                CenterV = aCenter.ProjectedTo(PlaneV)
                Radius = aRadius
                StartAngle = aStartAngle
                EndAngle = aEndAngle
                ClockWise = cClockwise
            Catch ex As Exception
                Throw New Exception("dxeArc.Define - " & ex.Message)
            End Try
        End Sub

        Public Function DefineWithPoints(CenterPtXY As Object, StartPointXY As Object, EndPointXY As Object, Optional aClockwise As Boolean = False, Optional aCS As dxfPlane = Nothing) As Boolean
            Return dxfUtils.Arc_DefineWithPoints(Me, CenterPtXY, StartPointXY, EndPointXY, aClockwise, aCS)
        End Function
        Public Function Divide(aAngle As Double) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '^#1the angle to divide the arc by
            '^returns a subset of arcs that are this arc divided by the passed angle
            '~the last member is the remaining portion and may be of a span less that the passed angle
            Dim aSegs As TSEGMENTS = dxfUtils.ArcDivide(ArcStructure, aAngle)


            For i As Integer = 1 To aSegs.Count
                Dim bArc As dxeArc = Clone()
                bArc.ArcStructure = aSegs.Item(i).ArcStructure
                _rVal.Add(bArc)
            Next i
            Return _rVal
        End Function



        Public Function SetSegmentWidth(aWidth As Double) As Boolean
            Dim _rVal As Boolean = False
            '^used to set the start and end width in one call
            aWidth = Math.Round(Math.Abs(aWidth), 8)
            If StartWidth <> aWidth Or EndWidth <> aWidth Then
                StartWidth = aWidth
                EndWidth = aWidth
                IsDirty = True
            End If
            Return _rVal
        End Function

        Public Function TrimWithRectangle(aRectangle As dxfRectangle, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bReturnInteriors As Boolean = True, Optional bReturnExteriors As Boolean = True) As List(Of dxeArc)
            Dim _rVal As New List(Of dxeArc)
            If aRectangle Is Nothing Then Return _rVal
            Dim rec As TPLANE = aRectangle.Strukture
            Dim myplane As TPLANE = PlaneV
            If Not rec.ZDirection.Equals(myplane.ZDirection) Then
                rec = rec.ProjectedToPlane(myplane)
            End If
            If aDisplaySettings Is Nothing Then aDisplaySettings = DisplaySettings
            Return dxfPrimatives.CreateCircleSegments(Radius, rec.Left, rec.Right, rec.Top, rec.Bottom, aPlane:=New dxfPlane(myplane), aDisplaySettings:=aDisplaySettings, bReturnInteriors, bReturnExteriors)
        End Function

        Public Overrides Function SetPropVal(aPropName As String, aValue As Object, Optional bDirtyOnChange As Boolean = False, Optional aOccurance As Integer = 1) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(aPropName, prop, aOccurance) Then Return False

            Select Case prop.Name.ToUpper()
                Case "RADIUS", "STARTWIDTH", "ENDWIDTH'", "*STARTWIDTH", "*ENDWIDTH'"
                    Dim dval As Double = TVALUES.To_DBL(aValue, bAbsVal:=True)
                    aValue = dval
            End Select
            Return MyBase.SetPropVal(aPropName, aValue, bDirtyOnChange, aOccurance)
        End Function
        Public Overrides Function SetPropVal(aGroupCode As Integer, aValue As Object, Optional bDirtyOnChange As Boolean = False, Optional aOccurance As Integer = 1) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(aGroupCode, prop, aOccurance) Then Return False
            Select Case prop.Name.ToUpper()
                Case "STARTWIDTH", "ENDWIDTH'", "*STARTWIDTH", "*ENDWIDTH'"
                    Dim dval As Double = TVALUES.To_DBL(aValue, bAbsVal:=True)
                    aValue = dval
                Case "RADIUS"
                    Dim dval As Double = TVALUES.To_DBL(aValue, bAbsVal:=True)
                    If dval = 0 Then dval = 0.00000000001
                    aValue = dval
            End Select

            Return MyBase.SetPropVal(aGroupCode, aValue, bDirtyOnChange, aOccurance)
        End Function

#End Region 'Methods

    End Class 'dxeArc
End Namespace
