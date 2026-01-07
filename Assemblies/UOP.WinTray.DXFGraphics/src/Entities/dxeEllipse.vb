Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxeEllipse
        Inherits dxfArc

#Region "Members"
#End Region 'Members
#Region "Constructors"
#End Region 'Constructors
        Public Sub New()
            MyBase.New(dxxArcTypes.Ellipse)
        End Sub

        Public Sub New(aEntity As dxeEllipse, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxArcTypes.Ellipse, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Friend Sub New(aSegment As TSEGMENT)
            MyBase.New(dxxArcTypes.Ellipse)
            SetArcLineStructure(aSegment)
        End Sub
        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(dxxArcTypes.Ellipse, aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxArcTypes.Ellipse)
            DefineByObject(aObject)
        End Sub

        Public Sub New(aCenter As iVector, aMajorRadius As Double, aMinorRadius As Double, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 360, Optional cClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxArcTypes.Ellipse, aDisplaySettings)

            Try



                Dim rad1 As Double = Math.Abs(aMajorRadius)
                If rad1 <= 0 Then Throw New Exception("The Passed Major Radius Is Invalid")
                Dim rad2 As Double = Math.Abs(aMinorRadius)
                If rad2 <= 0 Then Throw New Exception("The Passed Minor Radius Is Invalid")
                If aStartAngle = aEndAngle Then aEndAngle = aStartAngle + 360
                Dim cp As dxfVector = New dxfVector(aCenter)
                Dim sa As Double
                Dim ea As Double

                Dim bDir As dxfDirection
                TVALUES.SortTwoValues(True, rad1, rad2)

                If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane()
                Dim zax As dxeLine = aPlane.ZAxis

                Dim aDir As dxfDirection = aPlane.XDirection
                bDir = New dxfDirection(aDir)
                aDir.RotateAboutLine(zax, aStartAngle)
                bDir.RotateAboutLine(zax, aEndAngle)
                sa = aPlane.XDirection.AngleTo(aDir)
                ea = aPlane.XDirection.AngleTo(bDir)
                If ea = sa Then
                    sa = 0
                    ea = 360
                End If
                Center.Strukture = cp.Strukture
                Radius = rad2
                MinorRadius = rad1
                ClockWise = cClockwise
                StartAngle = sa
                EndAngle = ea
                PlaneV = New TPLANE(aPlane)
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

#Region "Properties"


        Public Overrides ReadOnly Property ChordArea As Double
            '^the area of the poylgon defined by the arcs chord
            Get
                Return Area
            End Get
        End Property



#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Dim cp As TVECTOR = aObj.Properties.GCValueV(10)
            Dim xep As TVECTOR = cp + aObj.Properties.GCValueV(11)
            Dim bFlag As Boolean
            Dim xDir As TVECTOR = cp.DirectionTo(xep, False, bFlag)
            Dim zDir As TVECTOR = aObj.Properties.GCValueV(210, TVECTOR.WorldZ)
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(zDir)
            If bFlag Then xDir = aPlane.XDirection
            Dim yDir As TVECTOR = zDir.CrossProduct(xDir, True)
            aPlane = aPlane.ReDefined(cp, xDir, yDir)

            DisplayStructure = aObj.DisplayVars
            PlaneV = aPlane
            CenterV = cp
            Dim rad1 As Double = dxfProjections.DistanceTo(cp, xep)
            Dim rat As Double = aObj.Properties.GCValueD(40)
            Dim rad2 As Double = rat * rad1
            TVALUES.SortTwoValues(True, rad1, rad2)
            MyBase.SetPropVal("*MinorRadius", rad1, False)
            MyBase.SetPropVal("*MajorRadius", rad2, False)
            Dim ang1 As Double = aObj.Properties.GCValueD(41, 0)
            Dim ang2 As Double = aObj.Properties.GCValueD(42, 2 * Math.PI)
            Do While ang1 > 2 * Math.PI
                ang1 -= (2 * Math.PI)
            Loop
            Do While ang2 > 2 * Math.PI
                ang2 -= (2 * Math.PI)
            Loop
            Dim va As Double = -Radius
            Dim vb As Double = -MinorRadius


            Dim v1 As New TVECTOR(aPlane, va * Math.Cos(ang1), vb * Math.Cos(ang1))
            Dim aDir As TVECTOR = cp.DirectionTo(v1)
            Dim vVal As Double = aPlane.XDirection.AngleTo(aDir, aPlane.ZDirection)
            If ang1 > 0 And ang1 < 0.5 * Math.PI Then
                StartAngle = vVal - 180
            ElseIf ang1 > 0.5 * Math.PI And ang1 < Math.PI Then
                StartAngle = vVal - 180
            ElseIf ang1 > Math.PI And ang1 < 1.5 * Math.PI Then
                StartAngle = 180 + vVal
            ElseIf ang1 > 1.5 * Math.PI And ang1 < 2 * Math.PI Then
                StartAngle = 180 + vVal
            Else
                StartAngle = ang1 * 180 / Math.PI
            End If
            v1.X = va * Math.Cos(ang2)
            v1.Y = vb * Math.Sin(ang2)
            v1 = New TVECTOR(aPlane, v1.X, v1.Y)
            aDir = cp.DirectionTo(v1)
            vVal = aPlane.XDirection.AngleTo(aDir, aPlane.ZDirection)
            If ang2 > 0 And ang2 < 0.5 * Math.PI Then
                EndAngle = vVal - 180
            ElseIf ang2 > 0.5 * Math.PI And ang2 < Math.PI Then
                EndAngle = vVal - 180
            ElseIf ang2 > Math.PI And ang2 < 1.5 * Math.PI Then
                EndAngle = 180 + vVal
            ElseIf ang2 > 1.5 * Math.PI And ang2 < 2 * Math.PI Then
                EndAngle = 180 + vVal
            Else
                EndAngle = ang2 * 180 / Math.PI
            End If
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
        End Sub

#End Region 'MustOverride Entity Methods
#Region "Methods"
        Public Overrides Function SetPropVal(aPropName As String, aValue As Object, Optional bDirtyOnChange As Boolean = False, Optional aOccurance As Integer = 1) As Boolean
            Dim myprop As dxoProperty = Nothing
            If Not Properties.TryGet(aPropName, myprop, aOccurance) Then Return False
            Dim _rVal As Boolean = False
            Dim bsortrads As Boolean = False
            Select Case myprop.Name.ToUpper()
                Case "*MAJORRADIUS", "MAJORRADIUS"
                    Dim dval As Double = TVALUES.To_DBL(aValue, bAbsVal:=True, aDefault:=MajorDiameter / 2)
                    bsortrads = True
                    aValue = dval
                Case "*MINORRADIUS", "MINORRADIUS"
                    Dim dval As Double = TVALUES.To_DBL(aValue, bAbsVal:=True, aDefault:=MinorDiameter / 2)
                    bsortrads = True
                    aValue = dval
            End Select

            _rVal = MyBase.SetPropVal(myprop, aValue, bDirtyOnChange)
            If _rVal And bsortrads Then


                Dim R1 As Double = PropValueD("*MinorRadius")
                    Dim R2 As Double = PropValueD("*MajorRadius")
                    If TVALUES.SortTwoValues(True, R1, R2) Then
                        MyBase.SetPropVal("*MinorRadius", R1, False)
                        MyBase.SetPropVal("*MajorRadius", R2, False)
                        IsDirty = True
                    End If

            End If
            Return _rVal
        End Function
        Public Overrides Function SetPropVal(aGroupCode As Integer, aValue As Object, Optional bDirtyOnChange As Boolean = False, Optional aOccurance As Integer = 1) As Boolean
            Dim myprop As dxoProperty = Nothing
            If Not Properties.TryGet(aGroupCode, myprop, aOccurance) Then Return False
            Dim _rVal As Boolean = False
            Dim bsortrads As Boolean = False
            Select Case myprop.Name.ToUpper()
                Case "*MAJORRADIUS", "MAJORRADIUS"
                    Dim dval As Double = TVALUES.To_DBL(aValue, bAbsVal:=True, aDefault:=MajorDiameter / 2)
                    bsortrads = True
                    aValue = dval
                Case "*MINORRADIUS", "MINORRADIUS"
                    Dim dval As Double = TVALUES.To_DBL(aValue, bAbsVal:=True, aDefault:=MinorDiameter / 2)
                    bsortrads = True
                    aValue = dval
            End Select

            _rVal = MyBase.SetPropVal(myprop, aValue, bDirtyOnChange)
            If _rVal And bsortrads Then


                Dim R1 As Double = PropValueD("*MinorRadius")
                Dim R2 As Double = PropValueD("*MajorRadius")
                If TVALUES.SortTwoValues(True, R1, R2) Then
                    MyBase.SetPropVal("*MinorRadius", R1, False)
                    MyBase.SetPropVal("*MajorRadius", R2, False)
                    IsDirty = True
                End If

            End If
            Return _rVal
        End Function


        Public Function Break(aBreaker As Object, Optional bBreakersAreInfinite As Boolean = False, Optional bWasBroken As Boolean = False, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            '#1the entity or entities to use to break the ellipse
            '#2flag indicating that the breaker(s) are of infinite length
            '#3returns true if the ellipse was broken
            '#4returns the points of intersection that were used for the break points
            '^returns the ellipse broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Ellipse(Me, aBreaker, bBreakersAreInfinite, bWasBroken, aIntersects)
        End Function

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeEllipse
            Dim _rVal As dxeEllipse = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeEllipse
            Return New dxeEllipse(Me)
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
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname))
            Next i
            If iCnt > 1 Then
                _rVal.Name = tname & "-" & iCnt & " INSTANCES"
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
            Dim myProps As TPROPERTIES
            Dim aTrs As TTRANSFORMS
            Dim aPl As TPLANE = PlaneV
            Dim scl As Double = 1
            Dim ang As Double = 0
            Dim bInv As Boolean
            Dim bLft As Boolean
            Dim rad1 As Double
            Dim rad2 As Double
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl)
                myProps = New TPROPERTIES(ActiveProperties())
                rTypeName = Trim(myProps.Item(1).Value)
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
            Else
                myProps = New TPROPERTIES(ActiveProperties())
                myProps.Handle = Handle
                rad1 = MinorRadius
                rad2 = Radius
                If TVALUES.SortTwoValues(True, rad1, rad2) Then
                    myProps.SetVal("*MinorRadius", rad1)
                    myProps.SetVal("*Radius", rad2)
                End If
                myProps.SetVal("Ratio of minor axis to major axis", rad1 / rad2)
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                myProps.SetVectorGC(210, aOCS.ZDirection, 1, bNormalize:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                myProps.SetVal("Start parameter", ParameterStart)
                myProps.SetVal("End parameter", ParameterEnd)
                myProps.SetVectorGC(210, aOCS.ZDirection, 1, bNormalize:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                myProps.SetVectorGC(10, aPl.Origin, 1)
                SetProps(myProps)
                UpdateCommonProperties(rTypeName)
                myProps = New TPROPERTIES(Properties)
                rTypeName = "ELLIPSE"
            End If
            Dim v1 As TVECTOR
            Dim param1 As Double
            Dim param2 As Double
            rad1 = MinorRadius * scl
            rad2 = Radius * scl
            If rad1 <= 0 Or rad2 <= 0 Then Return _rVal
            myProps.SetVectorGC(210, aOCS.ZDirection, bNormalize:=False)
            v1 = aPl.Origin + aPl.XDirection * rad2
            v1 -= aPl.Origin
            myProps.SetVectorGC(10, aPl.Origin, bNormalize:=False)
            myProps.SetVectorGC(11, v1, bNormalize:=False)
            If ClockWise Then
                param1 = ParamEnd(aPl)
            Else
                param1 = ParamStart(aPl)
            End If
            myProps.SetVal("Start Parameter", param1)
            If ClockWise Then
                param2 = ParamStart(aPl)
            Else
                param2 = ParamEnd(aPl)
            End If
            myProps.SetVal("End Parameter", param2)
            _rVal.Add(myProps, rTypeName, True, True)
            Return _rVal
        End Function
        Public Sub Define(aCenter As iVector, aMajorRadius As Object, aMinorRadius As Object, aStartAngle As Double, aEndAngle As Double, Optional cClockwise As Boolean = False)
            Try
                If Not TVALUES.IsNumber(aMajorRadius) Then Throw New Exception("The Passed Major Radius Is Invalid")
                If Not TVALUES.IsNumber(aMinorRadius) Then Throw New Exception("The Passed Minor Radius Is Invalid")
                Dim rad1 As Double = TVALUES.To_DBL(aMajorRadius)
                If rad1 <= 0 Then Throw New Exception("The Passed Major Radius Is Invalid")
                Dim rad2 As Double = TVALUES.To_DBL(aMinorRadius)
                If rad2 <= 0 Then Throw New Exception("The Passed Minor Radius Is Invalid")
                If aStartAngle = aEndAngle Then aEndAngle = aStartAngle + 360
                Dim cp As dxfVector = New dxfVector(aCenter)
                Dim sa As Double
                Dim ea As Double
                Dim aDir As dxfDirection
                Dim bDir As dxfDirection
                TVALUES.SortTwoValues(True, rad1, rad2)

                aDir = g_WCS.XDirection
                bDir = aDir.Clone
                aDir.RotateAboutLine(g_WCS.ZAxis, aStartAngle)
                bDir.RotateAboutLine(g_WCS.ZAxis, aEndAngle)
                sa = g_WCS.XDirection.AngleTo(aDir)
                ea = g_WCS.XDirection.AngleTo(bDir)
                If ea = sa Then
                    sa = 0
                    ea = 360
                End If
                Center.Strukture = cp.Strukture
                Radius = rad2
                MinorRadius = rad1
                ClockWise = cClockwise
                StartAngle = sa
                EndAngle = ea
            Catch ex As Exception
                Throw ex
            End Try
        End Sub
        Public Function Divide(aAngle As Double) As colDXFEntities
            Dim _rVal As New colDXFEntities
            Dim sang As Double
            Dim eang As Double
            Dim aArc As dxeEllipse
            Dim divs As Integer
            Dim i As Integer
            aAngle = TVALUES.NormAng(aAngle, False, True)
            If aAngle = 0 Then aAngle = 360
            sang = SpannedAngle
            If aAngle = 360 Or aAngle >= sang Or sang = 0 Then
                _rVal.Add(Clone)
                Return _rVal
            End If
            divs = Fix(sang / aAngle)
            aArc = Clone()
            aArc.ClockWise = False
            sang = Center.AngleTo(StartPt)
            i = 1
            Do Until i = divs + 1
                eang = sang + aAngle
                aArc.StartAngle = sang
                aArc.EndAngle = eang
                _rVal.Add(aArc, bAddClone:=True)
                i += 1
                sang = eang
            Loop
            If divs * aAngle < SpannedAngle Then
                sang = eang
                eang = sang + SpannedAngle - (divs * aAngle)
                aArc.StartAngle = sang
                aArc.EndAngle = eang
                _rVal.Add(aArc)
            End If
            Return _rVal
        End Function


        ''' <summary>
        ''' returns a properties object loaded with the entities current properties
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ActiveProperties() As dxoProperties
            Properties.Handle = Handle
            UpdateCommonProperties(bUpdateProperties:=True)
            Properties.SetVal("Parameter Start", ParameterStart)
            Properties.SetVal("Parameter End", ParameterEnd)
            Return New dxoProperties(Properties)
        End Function

#End Region 'Methods
    End Class 'dxeEllipse
End Namespace
