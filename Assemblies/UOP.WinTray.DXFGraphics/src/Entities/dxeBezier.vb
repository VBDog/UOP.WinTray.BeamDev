
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxeBezier
        Inherits dxfEntity
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Bezier)
        End Sub


        Public Sub New(aEntity As dxeBezier, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Bezier, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Bezier)
            DefineByObject(aObject)
        End Sub
        Friend Sub New(CP1 As TVECTOR, CP2 As TVECTOR, CP3 As TVECTOR, CP4 As TVECTOR, aPlane As TPLANE, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxGraphicTypes.Bezier)
            PlaneV = aPlane
            Dim aPl As TPLANE = PlaneV
            StartPtV = CP1.ProjectedTo(aPl)
            ControlPt1V = CP2.ProjectedTo(aPl)
            ControlPt2V = CP3.ProjectedTo(aPl)
            EndPtV = CP4.ProjectedTo(aPl)

            If aDisplaySettings IsNot Nothing Then CopyDisplayValues(aDisplaySettings)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend Property BezierStructure As TBEZIER
            Get
                Return New TBEZIER(StartPtV, ControlPt1V, ControlPt2V, EndPtV, PlaneV)
            End Get
            Set(value As TBEZIER)

                StartPtV = value.CP1
                ControlPt1V = value.CP2
                ControlPt2V = value.CP3
                EndPtV = value.CP4
                PlaneV = value.Plane
            End Set
        End Property
        Public ReadOnly Property Chord As dxeLine
            '^returns the dxeLine that connects the curves start point to its end point
            Get
                Return New dxeLine(StartPt, EndPt, DisplaySettings)
            End Get
        End Property
        Public ReadOnly Property ControlPt1 As dxfVector
            Get
                Return MyBase.DefPts.Vector2
            End Get
        End Property
        Friend Property ControlPt1V As TVECTOR
            Get
                Return MyBase.DefPts.VectorGet(2)
            End Get
            Set(value As TVECTOR)
                MyBase.DefPts.VectorSet(2, value)
            End Set
        End Property
        Public ReadOnly Property ControlPt2 As dxfVector
            Get
                Return MyBase.DefPts.Vector3
            End Get
        End Property
        Friend Property ControlPt2V As TVECTOR
            Get
                Return MyBase.DefPts.VectorGet(3)
            End Get
            Set(value As TVECTOR)
                MyBase.DefPts.VectorSet(3, value)
            End Set
        End Property
        Public ReadOnly Property EndPoints As colDXFVectors
            Get
                Return New colDXFVectors(StartPt, EndPt)
            End Get
        End Property
        Public ReadOnly Property EndPt As dxfVector
            Get
                Return MyBase.DefPts.Vector4
            End Get
        End Property
        Friend Property EndPtV As TVECTOR
            Get
                Return MyBase.DefPts.VectorGet(4)
            End Get
            Set(value As TVECTOR)
                MyBase.DefPts.VectorSet(4, value)
            End Set
        End Property
        Public ReadOnly Property FitPoints As colDXFVectors
            Get
                '^returns a point collection of the requried fit foints
                Return New colDXFVectors(New dxfVector(StartPt), New dxfVector(EndPt))
            End Get
        End Property
        Public ReadOnly Property MidPt As dxfVector
            Get
                Return GetPathVector(0.5)
            End Get
        End Property
        Public Property Points As colDXFVectors
            '^returns a point collection with copies of the four defining points of the curve
            Get
                Return New colDXFVectors(New dxfVector(StartPt), New dxfVector(ControlPt1), New dxfVector(ControlPt2), New dxfVector(EndPt))
            End Get
            Set(value As colDXFVectors)
                If value Is Nothing Then Return

                If value.Count >= 4 Then
                    StartPt.MoveTo(value.Item(1))
                    ControlPt1.MoveTo(value.Item(2))
                    ControlPt2.MoveTo(value.Item(3))
                    EndPt.MoveTo(value.Item(4))
                End If

            End Set
        End Property
        Public ReadOnly Property StartPt As dxfVector
            Get
                Return MyBase.DefPts.Vector1
            End Get
        End Property
        Friend Property StartPtV As TVECTOR
            Get
                Return MyBase.DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                MyBase.DefPts.VectorSet(1, value)
            End Set
        End Property
        Friend ReadOnly Property SubEntities As colDXFEntities
            Get
                Return New colDXFEntities
            End Get
        End Property
        Friend Property Vectors As TVECTORS
            Get
                '^returns the curves 4 definition points
                Vectors = New TVECTORS
                Vectors.Add(StartPtV, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                Vectors.Add(ControlPt1V, TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                Vectors.Add(ControlPt2V, TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                Vectors.Add(EndPtV, TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                Return Vectors
            End Get
            Set(value As TVECTORS)
                '^returns the curves 4 definition points
                If value.Count >= 1 Then StartPtV = value.Item(1)
                If value.Count >= 2 Then ControlPt1V = value.Item(2)
                If value.Count >= 3 Then ControlPt2V = value.Item(3)
                If value.Count >= 4 Then EndPtV = value.Item(4)
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            '    Structure = ent_DefineProperties(Structure, aObj.Properties, bNoHandle)
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210))
            DisplayStructure = aObj.DisplayVars
            PlaneV = aPlane
            DefPts.VectorSet(1, aObj.Properties.GCValueV(10))
            DefPts.VectorSet(2, aObj.Properties.GCValueV(11))
            DefPts.VectorSet(3, aObj.Properties.GCValueV(12))
            DefPts.VectorSet(4, aObj.Properties.GCValueV(13))
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5, Handle)
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeBezier
            Dim _rVal As dxeBezier = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeBezier
            Return New dxeBezier(Me)
        End Function
#End Region 'MustOverride Entity Methods
#Region "Methods"
        Public Sub Coefficients(aX As Double, bX As Double, ByRef rCX As Double, ByRef rdX As Double, ByRef raY As Double, ByRef rbY As Double, ByRef rCY As Double, ByRef rDY As Double)
            '^returns the polynomial cooefficient for the curve
            Dim b0 As dxfVector = StartPt
            Dim b1 As dxfVector = ControlPt1
            Dim B2 As dxfVector = ControlPt2
            Dim b3 As dxfVector = EndPt
            rCX = 3 * (b1.X - b0.X)
            bX = 3 * (B2.X - b1.X) - rCX
            aX = b3.X - b0.X - rCX - bX
            rdX = b0.X
            rCY = 3 * (b1.Y - b0.Y)
            rbY = 3 * (B2.Y - b1.Y) - rCY
            raY = b3.Y - b0.Y - rCY - raY
            rDY = b0.Y
        End Sub
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
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname, aImage))
            Next i
            If iCnt > 1 Then
                _rVal.Name = tname & "-" & iCnt & " INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            'On Error Resume Next
            aInstance = Math.Abs(aInstance)
            If aInstance <= 0 Then aInstance = 1
            _rVal = New TPROPERTYARRAY(aInstance:=aInstance)
            Dim myProps As New TPROPERTIES(ActiveProperties())
            Dim aTrs As TTRANSFORMS
            Dim aPl As TPLANE = PlaneV
            Dim scl As Double
            Dim ang As Double
            Dim bInv As Boolean
            Dim bLft As Boolean
            scl = 1
            rTypeName = "SPLINE"
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl)
            Else
                myProps.Handle = Handle
                aTrs = New TTRANSFORMS
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                myProps.SetVectorGC(210, aOCS.ZDirection)
                SetProps(myProps)
                MyBase.UpdateCommonProperties(rTypeName)
                myProps = New TPROPERTIES(Properties)
            End If
            Dim ePts As TVECTORS = Vectors
            If aInstance > 1 Then TTRANSFORMS.Apply(aTrs, ePts)
            myProps.SetVal("Entity Type", rTypeName)
            myProps.SetVectorGC(10, ePts.Item(1), 1)
            myProps.SetVectorGC(10, ePts.Item(2), 2)
            myProps.SetVectorGC(10, ePts.Item(3), 3)
            myProps.SetVectorGC(10, ePts.Item(4), 4)
            _rVal.Add(myProps, rTypeName, True, True)
            Return _rVal
        End Function
        Public Function GetPathVector(aLengthFactor As Double) As dxfVector
            Dim _rVal As New dxfVector
            '#1a factor from 0 to 1 to retrieve a point along the length of the curve
            If aLengthFactor < 0 Then aLengthFactor = 0
            If aLengthFactor > 1 Then aLengthFactor = 1
            Dim a_x As Double
            Dim b_x As Double
            Dim c_x As Double
            Dim d_x As Double
            Dim a_y As Double
            Dim b_y As Double
            Dim c_y As Double
            Dim d_y As Double
            Coefficients(a_x, b_x, c_x, d_x, a_y, b_y, c_y, d_y)
            _rVal.X = a_x * aLengthFactor ^ 3 + b_x * aLengthFactor ^ 2 + c_x * aLengthFactor + d_x
            _rVal.Y = a_y * aLengthFactor ^ 3 + b_y * aLengthFactor ^ 2 + c_y * aLengthFactor + d_y
            Return _rVal
        End Function
        Public Function IsEqual(Segment As dxeBezier, Optional bCompareLineType As Boolean = True, Optional bMustBeCoincident As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            '#1the segment to compare
            '#2flag indicating that the linetype should be used in the comparison
            '#3flag indicating that the start and end points must be identical
            '^returns True if the passed segment is an arc whose start and end angles are equal to those of this instance.
            '~if the argument bMustBeCoincident = True then the two arcs must also have centers at the same spacial coordinates.
            If Segment Is Nothing Then Return _rVal
            If bCompareLineType And Segment.Linetype <> Linetype Then Return _rVal
            If Math.Abs(Length() - Segment.Length) > 0.001 Then Return _rVal
            '    Dim ang1 As Single
            '    Dim ang2 As Single
            '    Dim ang3 As Single
            '    Dim ang4 As Single
            '    Dim aSeg As dxeArc
            '
            '    Set aSeg = Segment
            '
            '    ang1 = StartAngle
            '    ang2 = EndAngle
            '    ang3 = aSeg.StartAngle
            '    ang4 = aSeg.EndAngle
            '
            '    IsEqual = Abs(ang1 - ang3) < 0.01 And Abs(ang2 - ang4) < 0.01
            '
            '    If IsEqual And bMustBeCoincident Then
            '        Dim v1 As dxfVector
            '        Dim v2 As dxfVector
            '
            '        Set v1 = Center
            '        Set v2 = aSeg.Center
            '
            '        If Not v1.IsEqual(v2) Then IsEqual = False
            '      End If
            Return _rVal
        End Function
        Public Function LineSegments(Optional CurveDivisions As Integer = 20) As colDXFEntities
            '^returns the entity as a collection of lines
            Return PhantomPoints(CurveDivisions).ConnectingLines(False, aLineWidth:=0, aDisplaySettings:=DisplaySettings)
        End Function
        Friend Sub Planarize()
            'On Error Resume Next
            Dim aPl As TPLANE = PlaneV
            StartPtV = StartPtV.ProjectedTo(aPl)
            ControlPt1V = ControlPt1V.ProjectedTo(aPl)
            ControlPt2V = ControlPt2V.ProjectedTo(aPl)
            EndPtV = EndPtV.ProjectedTo(aPl)
        End Sub
        Public Function Polyline(Optional aDivisions As Integer = 100) As dxePolyline
            '^returns the bezier convertedt to a polyline
            Return New dxePolyline(PhantomPoints(aDivisions), False, DisplaySettings)
        End Function

#End Region 'Methods
    End Class 'dxeBezier
End Namespace
