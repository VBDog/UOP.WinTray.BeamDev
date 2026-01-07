Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfArc
        Inherits dxfEntity

        Public Sub New(aArcType As dxxArcTypes, Optional aEntityToCopy As dxfEntity = Nothing, Optional bCloneInstances As Boolean = False)
            MyBase.New(ArcTypeToGraphicType(aArcType), aEntityToCopy:=aEntityToCopy, bCloneInstances:=bCloneInstances)
            _ArcType = aArcType
        End Sub

        Public Sub New(aArcType As dxxArcTypes, aDisplaySettings As dxfDisplaySettings, Optional aEntityToCopy As dxfEntity = Nothing)
            MyBase.New(ArcTypeToGraphicType(aArcType), aDisplaySettings, aEntityToCopy:=aEntityToCopy)
            _ArcType = aArcType
        End Sub
        Friend Sub New(aArcType As dxxArcTypes, aSubEntity As TENTITY, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bNewGUID As Boolean = False)

            MyBase.New(aSubEntity, aDisplaySettings, bNewGUID)
            _ArcType = aArcType
        End Sub

        Friend Sub New(aArc As TARC, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(ArcTypeToGraphicType(IIf(aArc.Elliptical, dxxArcTypes.Ellipse, dxxArcTypes.Arc)), aDisplaySettings)
            _ArcType = IIf(aArc.Elliptical, dxxArcTypes.Ellipse, dxxArcTypes.Arc)
            ArcStructure = aArc
        End Sub

        Private _ArcType As dxxArcTypes
        Public ReadOnly Property ArcType As dxxArcTypes
            Get
                Return _ArcType
            End Get
        End Property

        Public Property ClockWise As Boolean
            Get
                '^flag used to indicated that the arc is swept in a clockwise directions
                Return PropValueB("*Clockwise")
            End Get
            Set(value As Boolean)
                '^flag used to indicated that the arc is swept in an a clockwise directions
                SetPropVal("*Clockwise", value, True)
            End Set
        End Property

        Public Property Diameter As Double
            '^the major diameter of the ellipse
            Get
                Return 2 * Radius
            End Get
            Set(value As Double)
                Radius = value / 2
            End Set
        End Property



        Public ReadOnly Property Eccentricity As Double
            Get
                Dim A As Double
                Dim B As Double
                A = Radius
                B = MinorRadius
                TVALUES.SortTwoValues(True, B, A)
                If A <> 0 Then Return Math.Sqrt(1 - ((B ^ 2) / (A ^ 2))) Else Return 0
            End Get
        End Property

        Public ReadOnly Property Elliptical As Boolean
            Get
                If ArcType = dxxArcTypes.Arc Then Return False
                Return ArcType = Math.Abs(Math.Round(Radius - MinorRadius, 4)) <> 0
            End Get
        End Property
        Public ReadOnly Property MidLine As dxeLine
            Get
                '^returns the line that connects the arcs mid point point to its center
                Return New dxeLine(Center, Chord.MidPt, DisplaySettings)
            End Get
        End Property

        Public ReadOnly Property MidPt As dxfVector
            '^the mid vector of the Ellipse
            '~dynamically calculated based on current ellipse properties
            Get
                Return New dxfVector(MidPtV)
            End Get
        End Property
        Friend ReadOnly Property MidPtV As TVECTOR
            '^the mid vector of the Arc
            '~dynamically calculated based on current ellipse properties
            Get
                Return ArcStructure.MidPoint
            End Get
        End Property
        Public Property MinorDiameter As Double
            '^the minor diameter of the ellipse
            Get
                Return 2 * MinorRadius
            End Get
            Set(value As Double)
                MinorRadius = 0.5 * value
            End Set
        End Property


        Public ReadOnly Property EndPt As dxfVector
            Get
                '^the vector where the arc ends in wcs
                Return New dxfVector(EndPtV)
            End Get
        End Property
        Public Property MinorRadius As Double
            Get
                '^the radius of the arc
                If ArcType = dxxArcTypes.Ellipse Then
                    Return PropValueD("*MinorRadius")
                Else
                    Return PropValueD("Radius", 0.00000000001)
                End If

            End Get
            Set(value As Double)
                If ArcType = dxxArcTypes.Ellipse Then
                    SetPropVal("*MinorRadius", Math.Abs(value))
                Else
                    SetPropVal("Radius", Math.Abs(value))
                End If

            End Set
        End Property

        Public Property Radius As Double
            Get
                '^the radius of the arc
                If ArcType = dxxArcTypes.Ellipse Then
                    Return PropValueD("*MajorRadius")
                Else
                    Return PropValueD("Radius", 0.00000000001)
                End If

            End Get
            Set(value As Double)
                If ArcType = dxxArcTypes.Ellipse Then
                    SetPropVal("*MajorRadius", Math.Abs(value), Not SuppressEvents)
                Else
                    SetPropVal("Radius", Math.Abs(value), Not SuppressEvents)
                End If


            End Set
        End Property

        Public Property StartAngle As Double
            Get
                '^arc start angle
                If ArcType = dxxArcTypes.Ellipse Then Return PropValueD("*StartAngle") Else Return PropValueD("Start Angle")
            End Get
            Set(value As Double)
                If ArcType = dxxArcTypes.Ellipse Then
                    SetPropVal("*StartAngle", value, True)
                Else
                    SetPropVal("Start Angle", TVALUES.NormAng(value), True)
                End If

            End Set
        End Property



        Public ReadOnly Property StartPt As dxfVector
            Get
                '^the point where the Arc starts
                Return New dxfVector(StartPtV)
            End Get
        End Property
        Public ReadOnly Property EndPoints As colDXFVectors
            Get
                Return New colDXFVectors(StartPt, EndPt)
            End Get
        End Property

        Public Property EndAngle As Double
            '^the angle form the center to the end point of the Arc
            '~degrees are assumed and value is rounded to 6 decimal places.
            '~angle is measured clockwise from the positive X axis.
            Get
                If ArcType = dxxArcTypes.Ellipse Then
                    Return PropValueD("*EndAngle")
                Else
                    Return PropValueD("End Angle")
                End If

            End Get
            Set(value As Double)
                If ArcType = dxxArcTypes.Ellipse Then
                    SetPropVal("*EndAngle", value, True)
                Else
                    SetPropVal("End Angle", value, True)
                End If

            End Set
        End Property

        Friend ReadOnly Property EndPtV As TVECTOR
            Get
                If ArcType = dxxArcTypes.Ellipse Then
                    Return dxfUtils.EllipsePoint(CenterV, 2 * Radius, 2 * MinorRadius, EndAngle, PlaneV)
                Else
                    Return PlaneV.AngleVector(EndAngle, Radius, False)
                End If

            End Get
        End Property

        Public Property MajorDiameter As Double
            '^the major diameter of the arc
            Get
                Return 2 * Radius
            End Get
            Set(value As Double)
                Radius = value / 2
            End Set
        End Property

        Friend ReadOnly Property StartPtV As TVECTOR
            '^the vector where the entity starts in wcs
            Get
                If ArcType = dxxArcTypes.Ellipse Then
                    Return dxfUtils.EllipsePoint(CenterV, 2 * Radius, 2 * MinorRadius, StartAngle, PlaneV)
                Else
                    Return PlaneV.AngleVector(StartAngle, Radius, False)
                End If

            End Get
        End Property

        Public Property SpannedAngle As Double
            '^arc spanned angle
            '~dynamically calculated based on current arc properties
            '~returns the angle covered by the arc
            '~useful in determining midpoint and arc length.
            Get
                Return dxfMath.SpannedAngle(ClockWise, StartAngle, EndAngle)
            End Get
            Set(value As Double)
                'On Error Resume Next
                Dim dif As Double
                Dim sa As Double
                Dim ea As Double
                sa = dxfMath.SpannedAngle(ClockWise, StartAngle, EndAngle)
                value = TVALUES.NormAng(value, False, False, True)
                dif = sa - Math.Abs(value)
                If Math.Abs(dif) <= 0.01 Then Return
                If value = 360 Then
                    sa = 0
                    ea = 360
                Else
                    If Not ClockWise Then
                        sa = StartAngle + 0.5 * dif
                        ea = EndAngle - 0.5 * dif
                    Else
                        sa = StartAngle - 0.5 * dif
                        ea = EndAngle + 0.5 * dif
                    End If
                    sa = TVALUES.NormAng(sa, False, True)
                    ea = TVALUES.NormAng(ea, False, False)
                End If
                StartAngle = sa
                EndAngle = ea
                IsDirty = True
            End Set
        End Property

        Public ReadOnly Property Chord As dxeLine
            Get
                '^returns the line that connects the arcs start point to its end point
                Return New dxeLine(EndPt, StartPt, DisplaySettings)
            End Get
        End Property
        Public Overridable ReadOnly Property ChordArea As Double
            Get
                '^the area of the poylgon defined by the arcs chord
                Dim ang As Double
                Dim are As Double
                ang = dxfMath.Deg2Rad(SpannedAngle)
                are = ((2 * Radius) ^ 2) / 8
                are *= (ang - Math.Sin(ang))
                Return are
            End Get
        End Property

        Public Property Center As dxfVector
            '^the arc center
            Get
                Return MyBase.DefPts.GetVector(1, Radius)
            End Get
            Set(value As dxfVector)
                '^the arc center

                MyBase.DefPts.SetVector(value, 1)
            End Set
        End Property
        Friend Property CenterV As TVECTOR
            Get
                Return MyBase.DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                MyBase.DefPts.VectorSet(1, value)
            End Set
        End Property

        Public ReadOnly Property MajorAxis As dxeLine
            '^returns the line which is the major axis of the ellipse
            Get
                Return New dxeLine(VertexPoint1, VertexPoint2, DisplaySettings)
            End Get
        End Property

        Public ReadOnly Property ParameterEnd As Double
            '^the end parameter used to describe the ellipse in DXF
            Get
                If SpannedAngle >= 359.99 Then
                    Return 2 * Math.PI
                ElseIf ClockWise Then
                    Return ParamStart(PlaneV)
                Else
                    Return ParamEnd(PlaneV)
                End If
            End Get
        End Property
        Public ReadOnly Property ParameterStart As Double
            '^the start parameter used to describe the ellipse in DXF
            Get
                If SpannedAngle >= 359.99 Then
                    Return 0
                ElseIf ClockWise Then
                    Return ParamEnd(PlaneV)
                Else
                    Return ParamStart(PlaneV)
                End If

            End Get
        End Property



        Public ReadOnly Property MinorAxis As dxeLine
            Get
                '^returns the line which is the minor axis of the ellipse
                Return New dxeLine(VertexPoint3, VertexPoint4, DisplaySettings)
            End Get
        End Property

        Public ReadOnly Property Ratio As Double
            '^width/height
            Get
                Try
                    Return Radius / MinorRadius
                Catch ex As Exception
                    Return 0
                End Try
            End Get
        End Property

        Public ReadOnly Property VertexPoint1 As dxfVector
            Get
                '^the start point of the major axis
                Return QuadrantPoint(3)
            End Get
        End Property
        Public ReadOnly Property VertexPoint2 As dxfVector
            Get
                '^the end point of the major axis
                Return QuadrantPoint(1)
            End Get
        End Property
        Public ReadOnly Property VertexPoint3 As dxfVector
            Get
                '^the start point of the minor axis
                Return QuadrantPoint(4)
            End Get
        End Property
        Public ReadOnly Property VertexPoint4 As dxfVector
            Get
                '^the end point of the minor axis
                Return QuadrantPoint(2)
            End Get
        End Property

        Public Function QuadrantPoint(Quadrant As Integer, Optional aRotation As Double = 0) As dxfVector

            '^returns a point on the arc at 0,90,180 or 270 depending on the requeste quadrant
            Quadrant = TVALUES.LimitedValue(Math.Abs(Quadrant), 1, 4, 1)
            If Quadrant = 1 Then

                Return AnglePoint(0 + aRotation, "East")
            ElseIf Quadrant = 2 Then
                Return AnglePoint(90 + aRotation, "North")
            ElseIf Quadrant = 3 Then
                Return AnglePoint(180 + aRotation, "West")
            Else
                Return AnglePoint(270 + aRotation, "South")
            End If

        End Function
        Public Function QuadrantPoints(Optional aRotation As Double = 0.0) As colDXFVectors
            Return New colDXFVectors(ArcStructure.QuadrantPoints(aRotation))
        End Function

        Public ReadOnly Property Coordinates As String
            Get
                '^returns a text string containing the segments coordinates
                '~Center,Start,End
                'On Error Resume Next
                Dim _rVal As String = $"C{Center.CoordinatesR }{ dxfGlobals.Delim }S{ StartPt.CoordinatesR }{ dxfGlobals.Delim }E{ EndPt.CoordinatesR}"
                If ClockWise Then _rVal += $"{dxfGlobals.Delim}CW" Else _rVal += $"{dxfGlobals.Delim}CCW"
                Return _rVal
            End Get
        End Property

        Public Shared Function ArcTypeToGraphicType(aArcType As dxxArcTypes) As dxxGraphicTypes
            If aArcType = dxxArcTypes.Ellipse Then
                Return dxxGraphicTypes.Ellipse
            Else
                Return dxxGraphicTypes.Arc
            End If
        End Function

        Public Function AnglePoint(aAngle As Double, Optional aTag As Object = Nothing, Optional aAdder As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxfVector
            '#1an angle when added to the arcs start angle returns a vector on the ellipse
            '#2a tag to assignt ot the point
            '^returns a point on the arcs path at the specified angle from the ellipses X Axis
            Dim _rVal As New dxfVector(AngleVector(aAngle, False, aAdder, aPlane))
            If aTag IsNot Nothing Then _rVal.Tag = aTag.ToString()
            Return _rVal
        End Function

        Friend Function AngleVector(aAngle As Double, Optional bInRadians As Boolean = False, Optional aAdder As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As TVECTOR
            '#1an angle used to calculate a point on the arc
            '#2flag indicating that the passed angle is in radians
            '#3a distance to add to the arc radius
            '^creates a vector on the arcs path at the passed angle
            '~the angle is measured counter clockwise from the x axis of the arcs OCS
            Dim aPln As TPLANE
            If TPLANE.IsNull(aPlane) Then
                aPln = PlaneV
            Else
                aPln = New TPLANE(aPlane)
                aPln.Origin = CenterV
            End If
            If bInRadians Then aAngle = aAngle * 180 / Math.PI
            If ArcType = dxxArcTypes.Ellipse Then
                Return dxfUtils.EllipsePoint(CenterV, Diameter + aAdder, MinorDiameter + aAdder, aAngle, aPln)
            Else
                Return aPln.AngleVector(aAngle, Radius + aAdder, False)
            End If

        End Function
        Public Function IntersectPt(aEntity As dxfEntity, Optional thisEntity_IsInfinite As Boolean = True, Optional theEntity_IsInfinite As Boolean = True) As dxfVector
            Dim _rVal As dxfVector = dxfIntersections.Point(Me, aEntity, thisEntity_IsInfinite, theEntity_IsInfinite)
            '#1the entity to find insections for
            '^returns the first intersection vector of this line and the passed line (or other entity)
            '^assumes that this and the passed entity are both infinite
            If _rVal IsNot Nothing Then _rVal.DisplayStructure = DisplayStructure
            Return _rVal
        End Function


        Public ReadOnly Property Circumference As Double
            Get
                '^the circumference of the complete arc
                If ArcType = dxxArcTypes.Ellipse Then
                    Dim A As Double = Radius
                    Dim B As Double = MinorRadius
                    TVALUES.SortTwoValues(True, B, A)
                    Return Math.PI * Math.Sqrt(2 * (A ^ 2 + B ^ 2) - ((A - B) ^ 2 / 2))
                Else
                    Return 2 * Math.PI * Radius
                End If
            End Get
        End Property

        Public Function ConvertToLines(Optional aCurveDivisions As Integer = 20) As colDXFEntities
            '^returns the entity parsed into small line segments
            Return PhantomPoints(aCurveDivisions).ConnectingLines(True, aDisplaySettings:=DisplaySettings)
        End Function
        Public Function ConvertToPolyline(Optional bAsLines As Boolean = False, Optional aSegmentCount As Integer = 20) As dxePolyline
            '^returns the arc converted to a polyline
            UpdatePath()
            If ArcType = dxxArcTypes.Ellipse Then
                Return dxfPrimatives.CreateEllipse(Me, aSegmentCount)
            Else
                Return dxfPrimatives.CreateArc(Me, bAsLines, aSegmentCount)
            End If

        End Function
        Public Function IsEqual(aArc As dxfArc, Optional CompareLineType As Boolean = True, Optional MustBeCoincident As Boolean = False) As Boolean
            '#1the segment to compare
            '#2flag indicating that the linetype should be used in the comparison
            '#3flag indicating that the start and end vectors must be identical
            '^returns True if the passed segment is an ellipse whose start and end angles are equal to those of this instance.
            '~if the argument MustBeCoincident = True then the two arcs must also have centers at the same spacial coordinates.
            If aArc Is Nothing Then Return False
            If Math.Abs(aArc.SpannedAngle - SpannedAngle) > 0.01 Then Return False
            If Math.Abs(aArc.Radius - Radius) > 0.01 Then Return False
            If CompareLineType And String.Compare(aArc.Linetype, Linetype, True) <> 0 Then Return False
            If MustBeCoincident Then
                If Not aArc.Center.IsEqual(Center) Then Return False
                If Not aArc.StartPt.IsEqual(StartPt) Then Return False
                If Not aArc.EndPt.IsEqual(EndPt) Then Return False
            End If
            Return True
        End Function

        Public Function IsSimilarTo(aArc As dxfArc, Optional bCompareDisplaySettings As Boolean = True) As Boolean
            If aArc Is Nothing Then Return False
            If Math.Abs(aArc.Radius - Radius) > 0.0001 Then Return False
            If Math.Abs(aArc.MinorRadius - MinorRadius) > 0.0001 Then Return False
            If aArc.ClockWise <> ClockWise Then Return False
            If bCompareDisplaySettings Then
                If aArc.Color <> Color Then Return False
                If String.Compare(aArc.Linetype, Linetype, True) <> 0 Then Return False
                If String.Compare(aArc.LayerName, LayerName, True) <> 0 Then Return False
                If Math.Abs(aArc.LTScale - LTScale) > 0.0001 Then Return False
            End If
            Return True
        End Function
        Friend Sub DefineByString(DescriptorString As String)
            '#1the descriptor string to define the entity with
            '^sets the entities properties based on the passed descriptor string
            Dim dstr As String = Trim(DescriptorString)
            Dim vals() As String
            If Not dstr.Contains(dxfGlobals.Delim) Then Throw New Exception("[dxeArc.DefineByString] The Passed Descriptor String Is Invalid")
            vals = dstr.Split(dxfGlobals.Delim)
            If vals(0) <> TypeName Then Throw New Exception("[dxeArc.DefineByString] The Passed Descriptor String Does Not Contain Arc Data")
            'get rest of properties
            Center.DefineByString(vals(1))
            Radius = vals(2)
            StartAngle = vals(3)
            EndAngle = vals(4)
            Tag = vals(5)
            '    bClockWise = TVALUES.ToBoolean(TValues.To_INT(vals(6)))
        End Sub

        Public Function GetTangentLine(aAngle As Double, aLength As Double) As dxeLine
            '#1the arc angle to generate a tangent line at
            '#2the length for the returned line
            '^returns a tangent line to the arc starting at a point on the arc at the passed angle
            '~a negative length will invert the returned lines direction
            Return dxfUtils.ArcTangentLineE(Me, aAngle, aLength)
        End Function
        Public Function LiesOnPlane(aPlane As dxfPlane) As Boolean
            '#1the plane to check

            '^returns true if the entity lies on the passed plane
            Return Plane.IsEqual(aPlane, 1, False, False)
        End Function
        Public Function LineSegments(Optional aCurveDivisions As Integer = 20) As colDXFEntities
            '^returns the entity as a collection of lines
            Return PhantomPoints(aCurveDivisions).ConnectingLines(False, Tag, aDisplaySettings:=DisplaySettings)
        End Function
        Public Sub Expand(aAngle As Double)
            aAngle = Math.Abs(aAngle)
            aAngle = TVALUES.NormAng(aAngle, False, True)
            If aAngle = 0 Then Return
            If aAngle >= 360 - SpannedAngle Then aAngle = 360 - SpannedAngle
            If ClockWise Then
                StartAngle += 0.5 * aAngle
                EndAngle -= 0.5 * aAngle
            Else
                StartAngle -= 0.5 * aAngle
                EndAngle += 0.5 * aAngle
            End If
        End Sub

        Public Sub Contract(aAngle As Double)
            aAngle = Math.Abs(aAngle)
            aAngle = TVALUES.NormAng(aAngle, False, True)
            If aAngle = 0 Then Return
            If aAngle >= 360 - SpannedAngle Then aAngle = 360 - SpannedAngle
            If ClockWise Then
                StartAngle -= 0.5 * aAngle
                EndAngle += 0.5 * aAngle
            Else
                StartAngle += 0.5 * aAngle
                EndAngle -= 0.5 * aAngle
            End If
        End Sub
        Public Function SetSpan(aStartAngle As Double, aEndAngle As Double) As Boolean
            Dim _rVal As Boolean = False
            Dim newval As Double
            newval = TVALUES.NormAng(aStartAngle)
            If StartAngle <> newval Then
                _rVal = True
                StartAngle = newval
            End If
            newval = TVALUES.NormAng(aEndAngle)
            If EndAngle <> newval Then
                _rVal = True
                EndAngle = newval
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function

        Friend Function ParamEnd(aPlane As TPLANE) As Double

            '^the end parameter used to describe the ellipse in DXF
            Dim va As Double = -Radius
            Dim vb As Double = -MinorRadius
            If va = 0 Or vb = 0 Then Return 0
            Dim v1 As TVECTOR
            Dim ang As Double = EndAngle
            Dim vVal As Double
            Dim aPl As TPLANE = aPlane.Clone
            If ClockWise Then
                aPl.RotateAbout(aPl.Origin, aPl.YDirection, 180, False)
            End If
            v1 = EndPtV.WithRespectTo(aPl)
            If v1.X <> 0 Then
                vVal = Math.Atan((v1.Y / vb) / (v1.X / va))
            End If
            If ang > 0 And ang < 90 Then
                Return vVal
            ElseIf ang > 90 And ang < 180 Then
                Return Math.PI + vVal
            ElseIf ang > 180 And ang < 270 Then
                Return Math.PI + vVal
            ElseIf ang > 270 And ang < 360 Then
                Return 2 * Math.PI + vVal
            Else
                Return ang * Math.PI / 180
            End If
        End Function

        Public Function Inverse() As dxeArc
            Dim _rVal As dxeArc = New dxeArc(Me)
            '^returns a new arc segment with it's end point and start point the opposite of the current arc segment
            _rVal.StartAngle = EndAngle
            _rVal.EndAngle = StartAngle
            Return _rVal
        End Function
        Friend Function Beziers() As colDXFEntities
            Dim _rVal As New colDXFEntities
            'On Error Resume Next
            UpdatePath()
            Dim aSegs As TSEGMENTS = dxfUtils.ArcDivide(ArcStructure, 90)
            If aSegs.Count <= 0 Then Return _rVal

            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim v4 As TVECTOR
            Dim aBz As dxeBezier
            Dim bSegs As New TSEGMENTS

            Dim eVecs As New TVECTORS

            For i As Integer = 1 To aSegs.Count
                Dim aSeg As TSEGMENT = aSegs.Item(i)
                aSeg.ArcStructure.StartWidth = 0
                aSeg.ArcStructure.EndWidth = 0
                Dim aPath As TPATH = aSeg.GetPath(aSeg.ArcStructure.Plane, bSegs, eVecs)
                v1 = aPath.Looop(1).Item(i)
                v2 = aPath.Looop(1).Item(i + 1)
                v3 = aPath.Looop(1).Item(i + 2)
                v4 = aPath.Looop(1).Item(i + 3)
                aBz = New dxeBezier With {
                .PlaneV = PlaneV,
                .DisplayStructure = DisplayStructure,
                .StartPtV = v1,
                .ControlPt1V = v2,
                .ControlPt2V = v3,
                .EndPtV = v4}
                _rVal.Add(aBz)
                i += 2
            Next i
            Return _rVal
        End Function
        Friend Function ParamStart(aPlane As TPLANE) As Double
            '^the start parameter used to describe the ellipse in DXF
            Dim va As Double = -Radius
            Dim vb As Double = -MinorRadius
            If va = 0 Or vb = 0 Then Return 0
            Dim ang As Double = StartAngle
            Dim vVal As Double
            Dim aPl As TPLANE = aPlane.Clone
            Dim v1 As TVECTOR
            v1 = StartPtV.WithRespectTo(aPl)
            If v1.X <> 0 Then
                vVal = Math.Atan((v1.Y / vb) / (v1.X / va))
            End If
            If ang > 0 And ang < 90 Then
                Return vVal
            ElseIf ang > 90 And ang < 180 Then
                Return Math.PI + vVal
            ElseIf ang > 180 And ang < 270 Then
                Return Math.PI + vVal
            ElseIf ang > 270 And ang < 360 Then
                Return 2 * Math.PI + vVal
            Else
                Return ang * Math.PI / 180
            End If
        End Function
        Public Function Perimeter(Optional aCurveDivisions As Double = 20, Optional bClosed As Boolean = False) As dxePolyline
            '^returns the ellipse as a Polyline
            Dim _rVal As New dxePolyline With {
                .SuppressEvents = True,
                .DisplayStructure = DisplayStructure,
                .PlaneV = PlaneV
            }
            _rVal.Vertices.Populate(ArcStructure.PhantomPoints(aCurveDivisions))
            If SpannedAngle < 359.99 Then
                _rVal.Closed = bClosed
            End If
            Return _rVal
        End Function

        Public Sub Reverse()
            '^the returned arc is NOT co-incident with this one
            ClockWise = Not ClockWise

        End Sub


        Public Function Reversed() As dxeArc
            Dim _rVal As dxeArc = New dxeArc(Me)
            '^returns a new arc segment with it's end vector and start vector the same as the current arc segment
            '^but in the opposite direction
            '^the returned arc is NOT co-incident with this one
            _rVal.Reverse()
            Return _rVal
        End Function
    End Class

End Namespace
