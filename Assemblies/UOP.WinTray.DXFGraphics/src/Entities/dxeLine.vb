Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxeLine
        Inherits dxfEntity
        Implements iLine
        Implements iPolylineSegment

#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Line)
        End Sub

        Public Sub New(aEntity As dxeLine, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Line, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Friend Sub New(aDisplayVars As TDISPLAYVARS, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressed As Boolean = False, Optional aIdentifier As String = "")
            MyBase.New(dxxGraphicTypes.Line)
            DisplayStructure = aDisplayVars
            If aColor <> dxxColors.Undefined Then Color = aColor
            If aLineType <> "" Then Linetype = aLineType
            Identifier = aIdentifier
            Suppressed = bSuppressed
        End Sub
        Public Sub New(aStartPt As iVector, aEndPt As iVector, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxGraphicTypes.Line, aDisplaySettings)
            If TypeOf (aStartPt) Is dxfVector Then
                StartPt = aStartPt
            Else
                StartPt = New dxfVector(aStartPt)
            End If

            If TypeOf (aEndPt) Is dxfVector Then
                EndPt = aEndPt
            Else
                EndPt = New dxfVector(aEndPt)
            End If



        End Sub
        Public Sub New(aOrigin As iVector, aDirection As iVector, aDistance As Double, Optional bInverted As Boolean = False)
            MyBase.New(dxxGraphicTypes.Line)
            Dim l1 As New TLINE(New TVECTOR(aOrigin), New TVECTOR(aDirection), aDistance, bInverted)
            StartPtV = l1.SPT
            EndPtV = l1.EPT
        End Sub
        Friend Sub New(aStartPt As TVECTOR, aEndPt As TVECTOR, Optional bInvert As Boolean = False, Optional aExtend As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aIdentifier As String = "")
            MyBase.New(dxxGraphicTypes.Line, aDisplaySettings)

            If Not bInvert Then
                StartPtV = aStartPt
                EndPtV = aEndPt
            Else
                EndPtV = aStartPt
                StartPtV = aEndPt
            End If
            If aExtend <> 0 Then Extend(aExtend)
            Identifier = aIdentifier
        End Sub
        Friend Sub New(aLine As TLINE, Optional bInvert As Boolean = False, Optional aExtend As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxGraphicTypes.Line)
            If aDisplaySettings IsNot Nothing Then CopyDisplayValues(aDisplaySettings.Strukture)
            If Not bInvert Then
                StartPtV = aLine.SPT
                EndPtV = aLine.EPT
            Else
                EndPtV = aLine.SPT
                StartPtV = aLine.EPT
            End If
            Tag = aLine.Tag
            If aExtend <> 0 Then Extend(aExtend)
        End Sub

        Public Sub New(aLine As iLine, Optional bInvert As Boolean = False, Optional aExtend As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxGraphicTypes.Line)

            If aLine Is Nothing Then
                If aDisplaySettings IsNot Nothing Then CopyDisplayValues(aDisplaySettings)
                Return
            End If
            If TypeOf aLine Is dxeLine Then
                Dim dxflin As dxeLine = DirectCast(aLine, dxeLine)
                SetProps(New TPROPERTIES(dxflin.ActiveProperties()))
                If aDisplaySettings Is Nothing Then aDisplaySettings = dxflin.DisplaySettings
            End If
            If aDisplaySettings IsNot Nothing Then CopyDisplayValues(aDisplaySettings)
            If Not bInvert Then
                StartPtV = New TVECTOR(aLine.StartPt)
                EndPtV = New TVECTOR(aLine.EndPt)
            Else
                EndPtV = New TVECTOR(aLine.StartPt)
                StartPtV = New TVECTOR(aLine.EndPt)
            End If
            If aExtend <> 0 Then Extend(aExtend)
        End Sub
        Friend Sub New(aSegment As TSEGMENT)
            MyBase.New(dxxGraphicTypes.Line)

            SetArcLineStructure(aSegment)

        End Sub
        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Line)
            DefineByObject(aObject)
        End Sub


#End Region 'Constructors
#Region "Properties"
        Public Overrides ReadOnly Property EntityType As dxxEntityTypes
            Get
                If StartWidth > 0 Or EndWidth Then
                    Return dxxEntityTypes.Polyline
                Else
                    Return dxxEntityTypes.Line
                End If
            End Get
        End Property

        Public Property EndPt As dxfVector
            '^the line's end point
            Get
                Return DefPts.Vector2
            End Get
            Set(value As dxfVector)
                '^the line's end point
                DefPts.Vector2 = value
            End Set
        End Property
        Friend ReadOnly Property EndPtsV As TVECTORS
            Get
                Dim _rVal As New TVECTORS
                _rVal.Add(StartPtV)
                _rVal.Add(EndPtV)
                Return _rVal
            End Get
        End Property
        Friend Property EndPtV As TVECTOR
            Get
                Return EndPt.Strukture
            End Get
            Set(value As TVECTOR)
                EndPt.Strukture = value
            End Set
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
        Public ReadOnly Property MidPt As dxfVector
            Get
                '^the MidPt of the Line
                Return New dxfVector(MidPtV)
            End Get
        End Property
        Friend ReadOnly Property MidPtV As TVECTOR
            Get
                Return StartPtV.Interpolate(EndPtV, 0.5)
            End Get
        End Property
        Public ReadOnly Property Dir As dxfDirection
            Get

                Return New dxfDirection(Me, False)
            End Get
        End Property

        Public ReadOnly Property Slope As Double
            Get
                '^the slope of the Line
                '~a vertical line has an infinite slope
                '~so the returned value is not numeric and will be a string = "+infinity" or "-infinity"
                'change of y over change of x
                Dim x1 As Double
                Dim x2 As Double
                Dim Y1 As Double
                Dim Y2 As Double
                Dim _rVal As Double
                'evaluate the line left to right
                If EndPt.X >= StartPt.X Then
                    x1 = StartPt.X
                    Y1 = StartPt.Y
                    x2 = EndPt.X
                    Y2 = EndPt.Y
                Else
                    x2 = StartPt.X
                    Y2 = StartPt.Y
                    x1 = EndPt.X
                    Y1 = EndPt.Y
                End If
                If Math.Abs(EndPt.X - StartPt.X) < 0.001 Then
                    If EndPt.Y > StartPt.Y Then
                        _rVal = Double.PositiveInfinity
                    Else
                        _rVal = Double.NegativeInfinity
                    End If
                    Return _rVal
                End If
                _rVal = (Y2 - Y1) / (x2 - x1)
                Return _rVal
            End Get
        End Property
        Public Property StartPt As dxfVector
            '^the line's start point
            Get
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                MyBase.DefPts.SetVector(value, 1)
            End Set
        End Property
        Friend Property StartPtV As TVECTOR
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property
        Public Property StartWidth As Double
            Get
                Return PropValueD("*StartWidth")
            End Get
            Set(value As Double)
                SetPropVal("*StartWidth", Math.Round(Math.Abs(value), 8), True)
            End Set
        End Property
        Public ReadOnly Property Y_Intercept As Object
            '^the Y intercept of the Line
            '~the line doesn't have to pass through the Y axis for this value to be calculated.
            '~if the line is vertical then the Y intercept is undefined and the returned value will
            '~be a string = "undefined"
            Get
                Dim m As Double = Slope
                If m = Double.PositiveInfinity Or m = Double.NegativeInfinity Then Return "undefined"
                'knowing y = mx + b
                Return StartPt.Y - m * StartPt.X
            End Get
        End Property

        Private Property iLine_StartPt As iVector Implements iLine.StartPt
            Get
                Return StartPt
            End Get
            Set(value As iVector)
                StartPt = New dxfVector(value)
            End Set
        End Property

        Private Property iLine_EndPt As iVector Implements iLine.EndPt
            Get
                Return EndPt
            End Get
            Set(value As iVector)
                EndPt = New dxfVector(value)
            End Set
        End Property

        Public ReadOnly Property SegmentType As dxxSegmentTypes Implements iPolylineSegment.SegmentType
            Get
                Return dxxSegmentTypes.Line
            End Get
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function AlignTo(aLine As iLine, Optional bInvertAlign As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            '#1the line to align to
            '#2flag to align end to start instead of start to end
            '^moves this lines start and end points to the passed lines start and end points
            If aLine Is Nothing Then Return _rVal
            Dim wuz As Boolean = SuppressEvents
            SuppressEvents = True
            If Not bInvertAlign Then
                If StartPt.SetStructure(New TVECTOR(aLine.StartPt)) Then _rVal = True
                If EndPt.SetStructure(New TVECTOR(aLine.EndPt)) Then _rVal = True
            Else
                If EndPt.SetStructure(New TVECTOR(aLine.StartPt)) Then _rVal = True
                If StartPt.SetStructure(New TVECTOR(aLine.EndPt)) Then _rVal = True
            End If
            SuppressEvents = wuz
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function AngleOfInclination(Optional aPlane As dxfPlane = Nothing) As Double
            '#1the coordinate system to use to get the X Axis
            '^the angle from the X Axis to this line
            '~if nothing is passed the world coordinate system is used
            Dim bPl As New dxfPlane(aPlane)
            Return bPl.XDirection.AngleTo(Direction)
        End Function
        Public Function AngleTo(aLineObj As iLine, Optional aNormal As dxfDirection = Nothing) As Double
            '#1the line to use for the calculation
            '#2the normal to use in determining the angle
            '^returns the angle from the passed line to this one
            If aLineObj Is Nothing Then Return 0
            Dim line As New TLINE(aLineObj)

            If line.Length <= 0 Then Return 0
            Dim aN As dxfDirection
            If aNormal Is Nothing Then aN = Plane.ZDirection Else aN = aNormal
            Return Direction.AngleTo(New dxfVector(line.Direction), aN)
        End Function
        Friend Function Beziers(Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim v1 As TVECTOR = StartPtV
            Dim v2 As TVECTOR = EndPtV
            Dim aBez As New dxeBezier With {
                .SuppressEvents = True,
                .PlaneV = PlaneV,
                .StartPtV = v1,
                .ControlPt1V = v1.Interpolate(v2, 1 / 3),
                .ControlPt2V = v1.Interpolate(v2, 2 / 3),
                .EndPtV = v2
            }
            If aDisplaySettings Is Nothing Then aBez.DisplayStructure = DisplayStructure Else aBez.DisplayStructure = aDisplaySettings.Strukture
            aBez.SuppressEvents = False
            Return New colDXFEntities(aBez)
        End Function

        Public Function Break(aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            '#1the entity or entities to use to break the line
            '#2flag indicating that the breaker(s) are of infinite length

            '#4returns the points of intersection that were used for the break points
            '^returns the line broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Line(Me, aBreaker, bBreakersAreInfinite, False, aIntersects)
        End Function

        Public Function Break(aBreaker As dxfEntity, bBreakersAreInfinite As Boolean, ByRef rWasBroken As Boolean, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            '#1the entity or entities to use to break the line
            '#2flag indicating that the breaker(s) are of infinite length
            '#3returns true if the line was broken
            '#4returns the points of intersection that were used for the break points
            '^returns the line broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Line(Me, aBreaker, bBreakersAreInfinite, rWasBroken, aIntersects)
        End Function
        Public Function Break(aSegs As IEnumerable(Of dxfEntity), Optional aTreatAsInfinite As Boolean = False) As colDXFEntities
            '^returns the line broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Line(Me, aSegs, aTreatAsInfinite)
        End Function
        Public Function ClearWidth() As Boolean
            '^resets the start and end width to 0
            If SetPropVal("*StartWidth", 0, False) Or SetPropVal("*EndWidth", 0, False) Then
                IsDirty = True
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeLine
            Dim _rVal As dxeLine = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeLine
            Return New dxeLine(Me)
        End Function

        Friend Overloads Function Clone(bSuppress As Boolean, Optional aIdentifier As String = Nothing) As dxeLine
            '^returns a new object with properties matching those of the cloned object
            Dim _rVal As dxeLine = DirectCast(MyBase.Clone, dxeLine)

            _rVal.Suppressed = bSuppress
            If aIdentifier IsNot Nothing Then _rVal.Identifier = aIdentifier
            Return _rVal
        End Function
        Public Overloads Function Moved(Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxeLine
            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^returns a copy of the enttity moved by the passed displacement info
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Dim _rVal As dxeLine = Clone()
            _rVal.Move(aXChange, aYChange, aZChange, aPlane)
            Return _rVal
        End Function

        Public Function ConvergeTo(aVectorObj As iVector, Optional aEndPtDirection As dxfDirection = Nothing) As Boolean
            '#1the point to converge to
            '#2a direction to project the end point
            '#3a distance to project the end point
            '^moves the start and end points to the passed location and projects the end point in the passed direction the passed distance
            Dim rDistance As Double = 0.0
            Return ConvergeToV(New TVECTOR(aVectorObj), aEndPtDirection, rDistance)
        End Function
        Public Function ConvergeTo(aVectorObj As iVector, aEndPtDirection As dxfDirection, ByRef rDistance As Double) As Boolean
            '#1the point to converge to
            '#2a direction to project the end point
            '#3a distance to project the end point
            '^moves the start and end points to the passed location and projects the end point in the passed direction the passed distance
            Return ConvergeToV(New TVECTOR(aVectorObj), aEndPtDirection, rDistance)
        End Function
        Friend Function ConvergeToV(aVector As TVECTOR, Optional aEndPtDirection As dxfDirection = Nothing) As Boolean
            Dim rDistance As Double = 0.0
            Return ConvergeToV(aVector, aEndPtDirection, rDistance)
        End Function
        Friend Function ConvergeToV(aVector As TVECTOR, aEndPtDirection As dxfDirection, ByRef rDistance As Double) As Boolean
            rDistance = 0
            Dim _rVal As Boolean = False
            '#1the point to converge to
            '#2a direction to project the end point
            '#3a distance to project the end point
            '^moves the start and end points to the passed location and projects the end point in the passed direction the passed distance
            Dim ePts As colDXFVectors = EndPoints()
            Dim wuz As Boolean = SuppressEvents

            SuppressEvents = True
            If ePts.ConvergeTo(aVector, True) Then _rVal = True
            If aEndPtDirection IsNot Nothing And rDistance <> 0 Then
                If EndPt.SetStructure(EndPt.Strukture + (aEndPtDirection.Strukture * rDistance)) Then _rVal = True
            End If
            SuppressEvents = wuz
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function ConvertToPolyline() As dxePolyline
            '^returns the entity as a Polyline
            Dim _rVal As New dxePolyline With {
                .SuppressEvents = True,
                .DisplayStructure = DisplayStructure,
                .GroupName = GroupName,
                .PlaneV = PlaneV
                }
            _rVal.Instances.Copy(Instances)
            _rVal.Vertices.AddV(StartPtV, 0, aStartWidth:=StartWidth, aEndWidth:=EndWidth)
            _rVal.Vertices.AddV(EndPtV)
            If StartWidth = EndWidth Then
                If StartWidth > 0 Then _rVal.SegmentWidth = StartWidth
            End If
            _rVal.SuppressEvents = False
            Return _rVal
        End Function
        Public Function CrossProduct(aLine As dxeLine) As dxfDirection
            '^returns the cross product of this lines direction witht he passed lines direction
            '~this is direction is perpendicular to both lines
            If aLine Is Nothing Then Return Nothing
            If aLine.Length <= 0 Then Return Nothing
            If Length <= 0 Then Return Nothing
            Return Direction.CrossProduct(aLine.Direction)
        End Function
        Public Function CrossesX(Optional aPlane As dxfPlane = Nothing, Optional rInterPt As dxfVector = Nothing, Optional bTreatInfinite As Boolean = False) As Boolean
            '#1the coordinate system to get the X Axis from
            '#2returns the point where the entity intersects the X Axis
            '#3flag to treat this entity as infinitely long
            '^returns true if the X axis passes through this segment
            Dim aLn As New TLINE("")
            Dim lLn As New TLINE(Me)
            Dim ip As TVECTOR
            Dim bExists As Boolean
            Dim bOnMe As Boolean
            If Not dxfPlane.IsNull(aPlane) Then
                aLn = New TPLANE(aPlane).LineH(0, 10)
            Else
                aLn.EPT = aLn.SPT + TVECTOR.WorldX * 10
            End If
            Dim rInterceptExists As Boolean = False
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            ip = lLn.IntersectionPt(aLn, rLinesAreParallel, rLinesAreCoincident, bOnMe, rIsOnSecondLine, bExists)
            If bExists Then
                If Not bTreatInfinite And Not bOnMe Then Return False
                If rInterPt IsNot Nothing Then rInterPt.Strukture = ip
                Return True
            Else
                Return False
            End If
        End Function
        Public Function CrossesY(Optional aPlane As dxfPlane = Nothing, Optional rInterPt As dxfVector = Nothing, Optional bTreatInfinite As Boolean = False) As Boolean
            '#1the coordinate system to get the Y Axis from
            '#2returns the point where the entity intersects the Y Axis
            '#3flag to treat this entity as infinitely long
            '^returns true if the Y axis passes through this segment
            Dim aLn As New TLINE("")
            Dim lLn As New TLINE(Me)
            Dim ip As TVECTOR
            Dim bExists As Boolean
            Dim bOnMe As Boolean
            If Not dxfPlane.IsNull(aPlane) Then
                aLn = New TPLANE(aPlane).LineV(0, 10)
            Else
                aLn.EPT = aLn.SPT + TVECTOR.WorldY * 10
            End If
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            ip = lLn.IntersectionPt(aLn, rLinesAreParallel, rLinesAreCoincident, bOnMe, rIsOnSecondLine, bExists)
            If bExists Then
                If Not bTreatInfinite And Not bOnMe Then Return False
                If rInterPt IsNot Nothing Then rInterPt.Strukture = ip
                Return True
            End If
            Return False
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aOCS As TPLANE = TPLANE.World
            Dim tname As String = String.Empty
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            If StartWidth = 0 And EndWidth = 0 Then
                For i = 1 To iCnt
                    If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname))
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
                    _rVal.Name = "WIDELINE" & "-" & (iCnt) & " INSTANCES"
                Else
                    _rVal.Name = "WIDELINE"
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
            Dim myProps As New TPROPERTIES(ActiveProperties())
            Dim aTrs As TTRANSFORMS = Nothing
            Dim aPl As TPLANE = PlaneV.Clone
            Dim scl As Double = 1
            Dim ang As Double = 0
            Dim bInv As Boolean
            Dim bLft As Boolean
            Dim v1 As TVECTOR = StartPtV
            Dim v2 As TVECTOR = EndPtV
            Dim lng As Double = dxfProjections.DistanceTo(v1, v2) * scl
            If lng = 0 Then Return _rVal
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl)
                rTypeName = Trim(myProps.Item(1).Value)
            Else
                myProps.Handle = Handle
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                myProps.SetVectorGC(210, aOCS.ZDirection, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                SetProps(myProps)
                UpdateCommonProperties(rTypeName)
                myProps = New TPROPERTIES(Properties)
                rTypeName = "LINE"
            End If
            If aInstance > 1 Then
                TTRANSFORMS.Apply(aTrs, v1)
                TTRANSFORMS.Apply(aTrs, v2)
            End If
            myProps.SetVectorGC(10, v1)
            myProps.SetVectorGC(11, v2)
            If aInstance = 1 Then SetProps(myProps)
            _rVal.Add(myProps, rTypeName, True, True)
            Return _rVal
        End Function
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
           Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData,bClear:=True )
            DisplayStructure = aObj.DisplayVars
            StartPtV = aObj.Properties.GCValueV(10)
            EndPtV = aObj.Properties.GCValueV(11)
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
        End Sub


        Public Sub DefinePointDirectionLength(aPoint As iVector, aDirection As iVector, aLength As Double, Optional bPointIsEndPt As Boolean = False)
            DefinePointDirectionLengthV(New TVECTOR(aPoint), New TVECTOR(aDirection), aLength)
        End Sub
        Friend Sub DefinePointDirectionLengthV(aPoint As TVECTOR, aDirection As TVECTOR, aLength As Double, Optional bPointIsEndPt As Boolean = False)
            If Not bPointIsEndPt Then
                StartPtV = aPoint
                EndPtV = aPoint + (aDirection * aLength)
            Else
                EndPtV = aPoint
                StartPtV = aPoint + (aDirection * aLength)
            End If
        End Sub
        Public Function Direction(Optional bReturnInverse As Boolean = False, Optional aRotation As Double = 0.0, Optional aAxis As iVector = Nothing) As dxfDirection
            Dim _rVal As New dxfDirection(Me, bReturnInverse)
            '#1flag to return the inverse direction
            '^the direction from the start to the end
            If aRotation = 0 Then Return _rVal
            Dim aDir As TVECTOR = _rVal.Strukture
            Dim aAx As TVECTOR
            If aAxis IsNot Nothing Then aAx = New TVECTOR(aAxis) Else aAx = TVECTOR.WorldZ
            aDir.RotateAbout(aAx, aRotation, False)
            _rVal.Strukture = aDir
            Return _rVal
        End Function
        Friend Function DirectionV(Optional bReturnInverse As Boolean = False) As TVECTOR
            Dim rZeroLength As Boolean = False
            Return DirectionV(bReturnInverse, rZeroLength)
        End Function
        Friend Function DirectionV(bReturnInverse As Boolean, ByRef rZeroLength As Boolean) As TVECTOR
            Dim _rVal As TVECTOR = StartPtV.DirectionTo(EndPtV, bReturnInverse, rZeroLength)
            '#1flag to return the inverse direction
            '#2flag returns true if the line is of null length
            '^the direction from the start to the end
            If rZeroLength Then
                If Not bReturnInverse Then _rVal = TVECTOR.WorldX Else _rVal = New TVECTOR(-1, 0, 0)
            End If
            Return _rVal
        End Function
        Friend Function DirectionVR(bReturnInverse As Boolean, aRotation As Double, aAxis As TVECTOR) As TVECTOR
            Dim _rVal As TVECTOR
            '#1flag to return the inverse direction
            '#2flag to return the perpendicular direction
            '^the direction from the start to the end
            If dxfProjections.DistanceTo(StartPtV, EndPtV) > 0 Then
                _rVal = StartPtV.DirectionTo(EndPtV, bReturnInverse)
            Else
                If Not bReturnInverse Then _rVal = TVECTOR.WorldX Else _rVal = New TVECTOR(-1, 0, 0)
            End If
            If aRotation <> 0 Then _rVal.RotateAbout(aAxis, aRotation, False)
            Return _rVal
        End Function
        Public Function EndPoints(Optional bReturnClones As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            _rVal.Add(StartPt, bAddClone:=bReturnClones)
            _rVal.Add(EndPt, bAddClone:=bReturnClones)
            Return _rVal
        End Function

        ''' <summary>
        ''' extends the line in its current direction (start->end) the passed distance
        ''' </summary>
        ''' <remarks>negative values cause the line to shorten</remarks>
        ''' <param name="aDistance">the distance to extend the line</param>
        ''' <param name="bInverse">flag to extend the end point instead of the start</param>
        ''' <param name="bExtendFromCenter">flag to extend theend points out from the center 1/2 the distance each</param>

        Public Sub Extend(aDistance As Double, Optional bInverse As Boolean = False, Optional bExtendFromCenter As Boolean = False)
            If aDistance = 0 Then Return
            Dim aDir As TVECTOR = TVECTOR.Zero
            Dim wuz As Boolean = SuppressEvents
            SuppressEvents = True
            If Not bExtendFromCenter Then
                If Not bInverse Then
                    aDir = DirectionV()
                    EndPt.SetStructure(EndPt.Strukture + (aDir * aDistance))
                Else
                    aDir = DirectionV(True)
                    StartPt.SetStructure(StartPt.Strukture + (aDir * aDistance))
                End If

            Else
                aDir = DirectionV()
                EndPt.SetStructure(EndPt.Strukture + (aDir * aDistance / 2))
                StartPt.SetStructure(StartPt.Strukture + (aDir * -aDistance / 2))

            End If
            SuppressEvents = wuz
        End Sub
        Public Function GetOrdinate(aSearchParam As dxxOrdinateTypes, Optional aPlane As dxfPlane = Nothing) As Double

            '#1parameter controling the value returned
            '^returns the requested ordinate of either the start or end point of the line based on the search parameter
            Dim rLinePt As dxfVector = Nothing
            Return GetOrdinate(aSearchParam, aPlane, rLinePt)
        End Function

        Public Function GetOrdinate(aSearchParam As dxxOrdinateTypes, aPlane As dxfPlane, ByRef rLinePt As dxfVector) As Double
            Dim _rVal As Double = 0.0
            '#1parameter controling the value returned
            '^returns the requested ordinate of either the start or end point of the line based on the search parameter
            Dim aPts As colDXFVectors
            aPts = New colDXFVectors
            Dim aSP As dxfVector
            Dim aEP As dxfVector
            Dim aMP As dxfVector
            Dim idx As Integer
            Dim aPl As New TPLANE("")
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane)
            aSP = StartPt
            aEP = EndPt
            aMP = MidPt
            idx = 1
            v1 = aSP.Strukture
            v2 = aEP.Strukture
            v3 = aMP.Strukture
            If Not dxfPlane.IsNull(aPlane) Then
                v1 = v1.WithRespectTo(aPl)
                v2 = v2.WithRespectTo(aPl)
                v3 = v3.WithRespectTo(aPl)
            End If
            Select Case aSearchParam
                Case dxxOrdinateTypes.MaxX
                    _rVal = v1.X
                    If v2.X > v1.X Then
                        idx = 2
                        _rVal = v2.X
                    End If
                Case dxxOrdinateTypes.MinX
                    _rVal = v1.X
                    If v2.X < v1.X Then
                        idx = 2
                        _rVal = v2.X
                    End If
                Case dxxOrdinateTypes.MaxY
                    _rVal = v1.Y
                    If v2.Y > v1.Y Then
                        idx = 2
                        _rVal = v2.Y
                    End If
                Case dxxOrdinateTypes.MinY
                    _rVal = v1.Y
                    If v2.Y < v1.Y Then
                        idx = 2
                        _rVal = v2.Y
                    End If
                Case dxxOrdinateTypes.MaxZ
                    _rVal = v1.Z
                    If v2.Z > v1.Z Then
                        idx = 2
                        _rVal = v2.Z
                    End If
                Case dxxOrdinateTypes.MinZ
                    _rVal = v1.Z
                    If v2.Z < v1.Z Then
                        idx = 2
                        _rVal = v2.Z
                    End If
                Case dxxOrdinateTypes.MidX
                    idx = 3
                    _rVal = v3.X
                Case dxxOrdinateTypes.MidY
                    idx = 3
                    _rVal = v3.Y
                Case dxxOrdinateTypes.MidZ
                    idx = 3
                    _rVal = v3.Z
            End Select
            If rLinePt IsNot Nothing Then
                If idx = 1 Then
                    rLinePt.VertexV = aSP.VertexV
                ElseIf idx = 2 Then
                    rLinePt.VertexV = aEP.VertexV
                ElseIf idx = 3 Then
                    rLinePt.VertexV = aMP.VertexV
                End If
            End If
            Return _rVal
        End Function


        ''' <summary>
        ''' returns the intersection vector of this line and the passed line
        ''' </summary>
        ''' <param name="aLine">1the Line to find intersections for</param>
        ''' <param name="thisLine_IsInfinite">Flag to treat this line as infinite</param>
        ''' <param name="aLine_IsInfinite">flag to treat the passed line as infinite</param>
        ''' <returns></returns>
        Public Function IntersectPoint(aLine As iLine, Optional thisLine_IsInfinite As Boolean = True, Optional aLine_IsInfinite As Boolean = True) As dxfVector

            Dim rInterceptExists As Boolean
            Dim rLinesAreParallel As Boolean
            Dim rLinesAreCoincident As Boolean
            Dim rIsOnFirstLine As Boolean
            Dim rIsOnSecondLine As Boolean
            Return IntersectPoint(aLine, thisLine_IsInfinite, aLine_IsInfinite, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
        End Function
        Public Function IntersectPoint(aLine As iLine, thisLine_IsInfinite As Boolean, aLine_IsInfinite As Boolean, ByRef rLinesAreParallel As Boolean, ByRef rLinesAreCoincident As Boolean, ByRef rIsOnFirstLine As Boolean, ByRef rIsOnSecondLine As Boolean, ByRef rInterceptExists As Boolean) As dxfVector
            '#1the Line to find intersections for
            '#1Flag to treat this line as infinite
            '#2flag to treat then passed line as infinite
            '^returns the intersection vector of this line and the passed line
            '^assumes that this and the passed entity are both infinite
            rInterceptExists = False
            rLinesAreParallel = False
            rLinesAreCoincident = False
            rIsOnFirstLine = False
            rIsOnSecondLine = False
            If aLine Is Nothing Then Return Nothing

            If Length <= 0 Then Return Nothing

            Dim thisline As New TLINE(Me)
            Dim thatline As New TLINE(aLine)
            If thatline.Length <= 0 Then
                Return Nothing
            End If

            Dim ip As TVECTOR = thisline.IntersectionPt(thatline, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
            If Not rInterceptExists Then
                Return Nothing
            End If
            Return New dxfVector(ip) With {.DisplayStructure = DisplayStructure}
        End Function
        Public Function IntersectPointPlanarLine(aCenterX As Double, aCenterY As Double, Optional aAngle As Double = 0.0, Optional bTreatAsInfinite As Boolean = False, Optional aPlane As dxfPlane = Nothing) As dxfVector
            Dim aPl As New TPLANE(aPlane)

            Dim aLn As TLINE = TLINE.PolarLine(aPl.XDirection, aPl.ZDirection, 100, aAngle, False, 0, aPl.Vector(aCenterX, aCenterY), False, True)
            Dim bExists As Boolean
            Dim rInterceptExists As Boolean = False
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnFirstLine As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            Dim ip As TVECTOR = New TLINE(Me).IntersectionPt(aLn, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, bExists)
            If Not bExists Then Return Nothing
            Return New dxfVector(ip)
        End Function
        Public Function IntersectPt(aEntity As dxfEntity, Optional thisEntity_IsInfinite As Boolean = True, Optional theEntity_IsInfinite As Boolean = True) As dxfVector
            Dim _rVal As dxfVector = dxfIntersections.Point(Me, aEntity, thisEntity_IsInfinite, theEntity_IsInfinite)
            '#1the entity to find insections for
            '^returns the first intersection vector of this line and the passed line (or other entity)
            '^assumes that this and the passed entity are both infinite
            If _rVal IsNot Nothing Then _rVal.DisplayStructure = DisplayStructure
            Return _rVal
        End Function

        Public Function Inverse() As dxeLine
            Dim _rVal As dxeLine = Clone()
            '^returns a new Line with it's end point and start point the opposite of the current Line
            _rVal.SetVectors(EndPtV, StartPtV)
            Return _rVal
        End Function
        Public Sub Invert()
            Dim v1 As TVECTOR = New TVECTOR(StartPtV)
            StartPtV = New TVECTOR(EndPtV)
            EndPtV = v1
        End Sub
        Public Function IsEqual(Segment As dxfEntity, Optional CompareLineType As Boolean = True, Optional MustBeCoincident As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            '#1the segment to compare
            '#2flag indicating that the linetype should be used in the comparison
            '#3flag indicating that the start and end points must be identical
            '^returns True if the passed segment is a line whose length and angle of inclination are equal to that of this line.
            '~if the argument MustBeCoincident = True then the two lines must have start and endpoints at the same spacial coordinates.
            If Segment Is Nothing Then Return _rVal
            If Segment.GraphicType <> dxxGraphicTypes.Line Then Return _rVal
            If CompareLineType And Segment.Linetype <> Linetype Then Return _rVal
            If Math.Abs(Length - Segment.Length) > 0.001 Then Return _rVal
            Dim ang1 As Double
            Dim ang2 As Double
            Dim lSeg As dxeLine
            lSeg = Segment
            ang1 = AngleOfInclination()
            ang2 = lSeg.AngleOfInclination
            _rVal = Math.Abs(ang1 - ang2) < 0.01
            If Not _rVal Then
                If ang2 < 180 Then ang2 += 180 Else ang2 -= 180
                _rVal = Math.Abs(ang1 - ang2) < 0.01
            End If
            If _rVal And MustBeCoincident Then
                Dim v1 As dxfVector
                Dim v2 As dxfVector
                Dim v3 As dxfVector
                Dim v4 As dxfVector
                v1 = StartPt
                v2 = EndPt
                v3 = lSeg.StartPt
                v4 = lSeg.EndPt
                _rVal = v1.IsEqual(v3) Or v1.IsEqual(v4)
                If _rVal Then
                    _rVal = v2.IsEqual(v3) Or v2.IsEqual(v4)
                End If
            End If
            Return _rVal
        End Function
        Public Function IsHorizontal(Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 4) As Boolean
            '#1the coordinate system to use to get the x axis
            '#2the precision for the comparison
            '^returns True if the lines direction is equal to that of the X axis of the subject coordinate system
            'On Error Resume Next
            Dim dComp As TVECTOR
            If TPLANE.IsNull(aPlane) Then dComp = TVECTOR.WorldX Else dComp = aPlane.XDirectionV
            Return DirectionV.Equals(dComp, True, aPrecis)

        End Function
        Public Function IsParallelTo(aLineObj As iLine, Optional aPrecis As Integer = 4) As Boolean
            Dim rIsInverseDirection As Boolean = False
            Return IsParallelTo(aLineObj, aPrecis, rIsInverseDirection)
        End Function
        Public Function IsParallelTo(aLineObj As iLine, aPrecis As Integer, ByRef rIsInverseDirection As Boolean) As Boolean
            Dim _rVal As Boolean = False
            '#1the line to test
            '#2the precision for the comparison
            '#3returns true if this lines direction is equal to the inverse direction of the passed line
            '^returns True if this lines direction (either way) is equal to that of the passed line
            rIsInverseDirection = False
            If Length = 0 Or aLineObj Is Nothing Then Return False
            Dim aLine As New dxeLine(aLineObj)

            Dim d1 As dxfDirection = Direction()
            Dim d2 As dxfDirection

            d2 = aLine.Direction
            _rVal = d1.IsEqual(d2, aPrecis, True, rIsInverseDirection)
            Return _rVal
        End Function
        Public Function IsVertical(Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 4) As Boolean
            '#1the coordinate system to use to get the y axis
            '#2the precision for the comparison
            '^returns True if the lines direction is equal to that of the Y axis of the subject coordinate system
            Dim dComp As TVECTOR
            If TPLANE.IsNull(aPlane) Then dComp = TVECTOR.WorldY Else dComp = aPlane.YDirectionV
            Return DirectionV.Equals(dComp, True, aPrecis)
        End Function
        Public Function Point(aDistanceOrPct As Double, Optional bFromEnd As Boolean = False, Optional bPercentPassed As Boolean = False) As dxfVector
            '#1the distance from the start point to use or the percentace of the lines length
            '#2flag indicating that the returned point is with respect to the lines start or end pt
            '#3flag indicating that the first argument should be treated as a percentage
            '^returns a point on the line based on the values of the first two arguments and the lines current direction.
            '~if the first argument is negative the direction to the returned vector will be reversed.
            Dim d1 As Double = Math.Abs(aDistanceOrPct)
            If bPercentPassed Then d1 = d1 / 100 * Length
            Dim v1 As TVECTOR
            Dim aDir As TVECTOR
            If Not bFromEnd Then v1 = StartPtV Else v1 = EndPtV
            aDir = DirectionV(aDistanceOrPct < 0)
            Return New dxfVector(v1 + (aDir * d1))
        End Function
        Public Function MoveOrthogonal(aDistance As Double, Optional aPlane As dxfPlane = Nothing) As Boolean
            If aDistance = 0 Then Return False
            Dim aDir As TVECTOR = DirectionV()
            If TPLANE.IsNull(aPlane) Then aDir.RotateAbout(TVECTOR.WorldZ, 90, False) Else aDir.RotateAbout(aPlane.ZDirectionV, 90, False)
            Return Transform(TTRANSFORM.CreateProjection(aDir, aDistance, bSupressEvents:=SuppressEvents))
        End Function
        Public Function Perimeter(Optional bAsLines As Boolean = False, Optional aCurveDivisions As Double = 20, Optional bClosed As Boolean = False) As dxePolyline
            '^returns the entity as a Polyline
            Dim _rVal As New dxePolyline With {
                .SuppressEvents = True,
                .DisplayStructure = DisplayStructure,
                .PlaneV = PlaneV,
                .Identifier = "LINE" 'real important for gridding
            }
            _rVal.Vertices.AddV(StartPtV)
            _rVal.Vertices.AddV(EndPtV)
            _rVal.SuppressEvents = False
            Return _rVal
        End Function
        Public Function PointsDown(Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 2) As Boolean
            Dim zerolength As Boolean
            Dim aDir As TVECTOR = StartPt.Strukture.DirectionTo(EndPt.Strukture, False, zerolength)
            If zerolength Then Return False
            If Not dxfPlane.IsNull(aPlane) Then
                Return aDir.Equals((aPlane.YDirectionV * -1), aPrecis)
            Else
                Return aDir.Equals(New TVECTOR(0, -1, 0), aPrecis)
            End If
        End Function
        Public Function PointsLeft(Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 2) As Boolean
            Dim zerolength As Boolean
            Dim aDir As TVECTOR = StartPt.Strukture.DirectionTo(EndPt.Strukture, False, zerolength)
            If zerolength Then Return False
            If aCS IsNot Nothing Then
                Return aDir.Equals((aCS.XDirectionV * -1), aPrecis)
            Else
                Return aDir.Equals(New TVECTOR(-1, 0, 0), aPrecis)
            End If
        End Function
        Public Function PointsRight(Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 2) As Boolean
            Dim zerolength As Boolean
            Dim aDir As TVECTOR = StartPt.Strukture.DirectionTo(EndPt.Strukture, False, zerolength)
            If zerolength Then Return False
            If aCS IsNot Nothing Then
                Return aDir.Equals(aCS.XDirectionV, aPrecis)
            Else
                Return aDir.Equals(TVECTOR.WorldX, aPrecis)
            End If
        End Function
        Public Function PointsUp(Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 2) As Boolean
            Dim zerolength As Boolean
            Dim aDir As TVECTOR = StartPt.Strukture.DirectionTo(EndPt.Strukture, False, zerolength)
            If zerolength Then Return False
            If aCS IsNot Nothing Then
                Return aDir.Equals(aCS.YDirectionV, aPrecis)
            Else
                Return aDir.Equals(TVECTOR.WorldY, aPrecis)
            End If
        End Function
        Public Function Projected(aDirection As dxfDirection, aDistance As Object) As dxeLine
            '#1the direction to project in
            '#2the distance to project
            '^projects the entity in the passed direction the requested distance
            '#1the direction to project in
            '#2the distance to project
            '^projects the entity in the passed direction the requested distance
            Dim _rVal As dxeLine = Clone()
            If aDirection IsNot Nothing Then _rVal.Project(aDirection, aDistance)
            Return _rVal
        End Function
        Public Function ProjectedToPlane(aPlane As dxfPlane) As dxeLine

            '#1the plane to project to
            '#2the projection direction (if not passed a perpendicular project is is done)
            '^retuns aline that is this line projected to the passed plane
            Dim _rVal As dxeLine = Clone()
            If TPLANE.IsNull(aPlane) Then Return _rVal
            _rVal.StartPtV = _rVal.StartPtV.ProjectedTo(aPlane.Strukture)
            _rVal.EndPtV = _rVal.EndPtV.ProjectedTo(aPlane.Strukture)
            _rVal.PlaneV = New TPLANE(aPlane)
            Return _rVal
        End Function
        Public Sub Rectify(ByRef RectY As Boolean, Optional bReverse As Boolean = False)
            '#1flag to rectify the the line by X or Y ordinate
            '#2flag to invert the rectification
            '^ensures that the lines X or Y ordinates are in a particular order
            '~In Y the default is start Y < end Y (upward pointing)
            '~In X the default is start X < end X (right pointing)
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim v1 As TVECTOR
            Dim bSwap As Boolean
            sp = StartPtV
            ep = EndPtV
            If RectY Then
                If Not bReverse Then bSwap = sp.Y > ep.Y Else bSwap = sp.Y < ep.Y
            Else
                If Not bReverse Then bSwap = sp.X > ep.X Else bSwap = sp.X < ep.X
            End If
            If bSwap Then
                v1 = sp
                StartPtV = ep
                EndPtV = v1
            End If
        End Sub
        Public Overloads Sub SetCoordinates(Optional aSPX As Double? = Nothing, Optional aSPY As Double? = Nothing, Optional aSPZ As Double? = Nothing, Optional aEPX As Double? = Nothing, Optional aEPY As Double? = Nothing, Optional aEPZ As Double? = Nothing)
            '#1the x ordinate to assign to the start point
            '#2the y ordinate to assign to the start point
            '#3the x ordinate to assign to the end point
            '#4the y ordinate to assign to the end point
            '#5the z ordinate to assign to the start point
            '#6the z ordinate to assign to the end point
            '^defines the coordinates of the lines start and end points.
            'Unpassed or non-numeric values are assumed to be the current values
            StartPtV = StartPtV.Updated(aSPX, aSPY, aSPZ)
            EndPtV = EndPtV.Updated(aEPX, aEPY, aEPZ)
        End Sub
        Public Sub SetCoordinates2D(Optional aSPX As Double? = Nothing, Optional aSPY As Double? = Nothing, Optional aEPX As Double? = Nothing, Optional aEPY As Double? = Nothing)
            '#1the x ordinate to assign to the start point
            '#2the y ordinate to assign to the start point
            '#3the x ordinate to assign to the end point
            '#4the y ordinate to assign to the end point
            '^defines the coordinates of the lines start and end points.
            '~Z is assumed to be zero. Unpassed or non-numeric values are assumed to be the current values
            SetCoordinates(aSPX, aSPY, 0, aEPX, aEPY, 0)
        End Sub
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
        Friend Function SetVectors(aStartPt As TVECTOR, aEndPt As TVECTOR) As Boolean
            Dim _rVal As Boolean = False
            '#1the new startpt structure
            '#2the new endpt structure
            '^set the endpoints in one call
            If StartPt.SetStructure(aStartPt) Then _rVal = True
            If EndPt.SetStructure(aEndPt) Then _rVal = True
            Return _rVal
        End Function
        Public Function SetVectors(aStartPt As dxfVector, aEndPt As dxfVector) As Boolean

            '#1the new startpt
            '#2the new endpt
            '
            '^set the endpoints in one call
            Dim sp As TVECTOR = StartPtV
            Dim ep As TVECTOR = EndPtV
            If aStartPt IsNot Nothing Then sp = aStartPt.Strukture
            If aEndPt IsNot Nothing Then ep = aEndPt.Strukture
            Return SetVectors(sp, ep)
        End Function
        Public Sub TrimBetweenLines(aLine As dxeLine, bLine As dxeLine, Optional extend As Boolean = True)
            If aLine Is Nothing And bLine Is Nothing Then Return
            Dim v1 As TVECTOR
            Dim aFlag As Boolean
            Dim aDir As TVECTOR
            aDir = DirectionV(False, aFlag)
            If aFlag Then Return
            Dim lStr As New TLINE(Me)
            Dim nStr As TLINE = lStr

            Dim isOnFirst As Boolean
            Dim isOnSecond As Boolean
            Dim isP As Boolean
            Dim isC As Boolean

            If Not aLine Is Nothing Then
                v1 = lStr.IntersectionPt(New TLINE(aLine), isP, isC, isOnFirst, isOnSecond, aFlag)
                If extend Then
                    If aFlag Then nStr.SPT = v1
                Else
                    If isOnFirst Then nStr.SPT = v1
                End If
            End If
            If bLine IsNot Nothing Then
                v1 = lStr.IntersectionPt(New TLINE(bLine), isP, isC, isOnFirst, isOnSecond, aFlag)
                If extend Then
                    If aFlag Then nStr.EPT = v1
                Else
                    If isOnFirst Then nStr.EPT = v1
                End If
            End If
            LineStructure = nStr
        End Sub

#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function PlanarLine(aStartPtX As Double, aStartPtY As Double, Optional aEndPtX As Double = 0.0, Optional aEndPtY As Double = 0.0, Optional aTag As String = "", Optional aFlag As String = "", Optional aLineType As String = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As dxeLine
            '#1the X coordinate of the start point of the new line
            '#2the Y coordinate of the start point of the new line
            '#3the X coordinate of the end point of the new line
            '#4the Y coordinate of the end point of the new line
            '#5a tag to assign to the line
            '#6a flag to assign to the line
            '#7an overriding linetype assign to the line
            '#8the display settings for the line
            '#9the coordinate system to use
            '^creates a new line based on the passed info
            Dim plane As dxfPlane
            If TPLANE.IsNull(aPlane) Then plane = New dxfPlane() Else plane = aPlane
            Dim _rVal As New dxeLine(plane.Vector(aStartPtX, aStartPtY), plane.Vector(aEndPtX, aEndPtY), aDisplaySettings) With {.Tag = aTag, .Flag = aFlag}
            If Not String.IsNullOrWhiteSpace(aLineType) Then _rVal.Linetype = aLineType
            Return _rVal
        End Function
        Public Shared Function ByProjection(aOrigin As iVector, aDirection As iVector, aDistance As Double, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeLine
            Dim v1 As New TVECTOR(aOrigin)
            Dim v2 As New TVECTOR(aDirection)
            v2 = v1 + v2.Normalized * aDistance

            Return New dxeLine(v1, v2, aDisplaySettings:=aDisplaySettings)

            '#1the start pt for the new line
            '#2the direction to project the line in
            '#3the distance to project (length)

            '^returns a line with its start point at the Origin and its end point projected the passed distance in the passed direction

        End Function
        Public Shared Function FromTo(aVector As iVector, bVector As iVector, Optional bInvert As Boolean = False, Optional aExtend As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeLine
            '#1the start point
            '#2theend point
            '#3flag to return the return the line from B to A instead of A to B
            '#4a distance to extend the line after it is created
            '#5the display settings to apply to the new line
            '^returns a line starting at this aPoint and ending at bPoint


            Return New dxeLine(New TVECTOR(aVector), New TVECTOR(bVector), bInvert, aExtend, aDisplaySettings)
        End Function
#End Region 'Shared Methods
    End Class 'dxeLine
End Namespace
