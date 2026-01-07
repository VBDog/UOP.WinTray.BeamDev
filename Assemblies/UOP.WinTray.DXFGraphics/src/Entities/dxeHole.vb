Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxeHole
        Inherits dxfEntity
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Hole)
        End Sub

        Public Sub New(aEntity As dxeHole, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Hole, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub


        Public Sub New(aCenter As dxfVector, aDiameter As Double, Optional aLength As Double = 0, Optional aTag As String = "", Optional aDepth As Double = 0,
        Optional aRotation As Double = 0, Optional aFlag As String = "", Optional aInset As Double = 0,
        Optional aDownSet As Double = 0, Optional aMinorRadius As Double = 0, Optional bWeldedBolt As Boolean = False, Optional bIsSquare As Boolean = False, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxGraphicTypes.Hole)
            Center = aCenter
            Diameter = aDiameter
            Length = aLength
            Tag = aTag
            Depth = aDepth
            Rotation = aRotation
            Flag = aFlag
            Inset = aInset
            DownSet = aDownSet
            MinorRadius = aMinorRadius
            WeldedBolt = bWeldedBolt
            IsSquare = bIsSquare
        End Sub
        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Hole)
            DefineByObject(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"

        Public Overrides ReadOnly Property EntityType As dxxEntityTypes
            Get
                If Math.Round(Radius * 2, 5) >= Math.Round(Length, 5) Then
                    Return dxxEntityTypes.Hole
                Else
                    Return dxxEntityTypes.Slot
                End If
            End Get
        End Property
        Public ReadOnly Property Axis As dxeLine
            Get
                Return Plane.ZAxis
            End Get
        End Property
        Public Property BlockName As String
            Get
                Dim _rVal As String = PropValueStr("BlockName")
                If _rVal = "" Then _rVal = GUID
                Return _rVal
            End Get
            Set(value As String)
                SetPropVal("BlockName", value.Trim)
            End Set
        End Property
        Public Property Center As dxfVector
            '^the center of the hole
            Get
                Return DefPts.GetVector(1, Radius, PropValueD("Rotation"))

            End Get
            Set(value As dxfVector)
                If Not value Is Nothing Then CenterV = value.Strukture Else CenterV = New TVECTOR
            End Set
        End Property
        Friend Property CenterV As TVECTOR
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property
        Public Property Depth As Double
            '^the depth of the hole
            Get
                Return PropValueD("Depth")
            End Get
            Set(value As Double)
                '^the depth of the hole
                SetPropVal("Depth", Math.Abs(value), True)
            End Set
        End Property
        Public Property Diameter As Double
            '^the hole diameter
            Get
                Return 2 * Radius
            End Get
            Set(value As Double)
                Radius = 0.5 * value
            End Set
        End Property
        Public Property DownSet As Double
            '^the distance from an edge down to the hole center
            Get
                Return PropValueD("Downset")
            End Get
            Set(value As Double)
                SetPropVal("Downset", value, False)
            End Set
        End Property
        Public ReadOnly Property Entities As colDXFEntities
            Get
                Return SubEntities()
            End Get
        End Property
        Public ReadOnly Property ExtentLines As colDXFEntities
            Get
                Dim _rVal As New colDXFEntities
                Dim aPl As TPLANE = PlaneV
                If Rotation <> 0 Then aPl.Revolve(Rotation, False)
                _rVal.Add(New dxeLine(aPl.Vector(-0.5 * Length, 0), aPl.Vector(0.5 * Length, 0)))
                _rVal.Add(New dxeLine(aPl.Vector(0, -Radius), aPl.Vector(0, Radius)))
                _rVal.Add(New dxeLine(aPl.Origin + aPl.ZDirection * (0.5 * Depth), aPl.Origin + aPl.ZDirection * (-0.5 * Depth)))
                Return _rVal
            End Get
        End Property
        Public Shadows Property Height As Double
            Get
                Return 2 * Radius
            End Get
            Set(value As Double)
                Radius = 0.5 * value
            End Set
        End Property
        Public ReadOnly Property HoleHandle As String
            Get
                '^the tag and the flag strings returned in a comma delimited string
                Dim _rVal As String = Tag
                Dim fg As String = Flag
                If fg <> "" Then TLISTS.Add(_rVal, fg, bAllowDuplicates:=True)
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property HoleType As dxxHoleTypes
            Get
                Dim eType As dxxEntityTypes = EntityType
                If eType = dxxEntityTypes.Hole Then
                    If IsSquare Then Return dxxHoleTypes.Square Else Return dxxHoleTypes.Round
                Else
                    If IsSquare Then Return dxxHoleTypes.SquareSlot Else Return dxxHoleTypes.RoundSlot
                End If
            End Get
        End Property
        Public Property Inset As Double
            Get
                '^the distance from an edge to the hole center
                Return PropValueD("Inset")
            End Get
            Set(value As Double)
                '^the distance from an edge to the hole center
                SetPropVal("Inset", value, False)
            End Set
        End Property
        Public Property IsSquare As Boolean
            '^flag indicating if the hole is square or round
            Get
                Return PropValueB("IsSquare")
            End Get
            Set(value As Boolean)
                '^flag indicating if the hole is square or round
                SetPropVal("IsSquare", value, True)
            End Set
        End Property
        Public Overrides Property Length As Double
            Get
                Return PropValueD("Length")
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                If value = 0 Then Return
                If SetPropVal("Length", value, False) Then
                    If value < 2 * Radius Then
                        SetPropVal("Radius", 0.5 * value, False)
                    End If
                    IsDirty = True
                End If
            End Set
        End Property
        Public Property MinorDiameter As Double
            '^the width of a hole if it has a flat in it ("D" hole)
            Get
                Return 2 * MinorRadius
            End Get
            Set(value As Double)
                MinorRadius = value / 2
            End Set
        End Property
        Public Property MinorRadius As Double
            Get
                '^the width of a hole if it has a flat in it ("D" hole)
                Return PropValueD("MinorRadius")
            End Get
            Set(value As Double)
                '^the width of a hole if it has a flat in it ("D" hole)
                If value <= 0 Then value = 0
                SetPropVal("MinorRadius", value, True)
            End Set
        End Property
        Public ReadOnly Property QuadrantPoints As colDXFVectors
            Get
                QuadrantPoints = New colDXFVectors
                Dim aPl As dxfPlane
                aPl = Plane
                aPl.Rotate(Rotation)
                If Math.Abs(Math.Round(2 * Radius - Length, 3)) = 0 Then
                End If
                Return QuadrantPoints
            End Get
        End Property
        Public Property Radius As Double
            '^the radius of the hole
            Get
                Return PropValueD("Radius", 1)
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                If SetPropVal("Radius", value, False) Then
                    If Length < 2 * value Then
                        SetPropVal("Length", 2 * value, False)
                    End If
                    IsDirty = True
                End If
            End Set
        End Property
        Public Property Rotation As Double
            'the orientation angle of the hole with respect to the horizontal X axis on its plane
            Get
                Return PropValueD("Rotation")
            End Get
            Set(value As Double)
                SetPropVal("Rotation", TVALUES.NormAng(value, False, True), True)
            End Set
        End Property
        Public Property SimpleDescriptor As String
            '^a string that defines a simple round 2D hole on the XY plane
            Get
                Dim _rVal As String = PropValueStr("*Descriptor")
                If _rVal = "" Then
                    Dim hType As dxxEntityTypes
                    hType = EntityType
                    If hType = dxxEntityTypes.Slot Then
                        _rVal = $"(1,{ Length},{ 2 * Radius},{Rotation },{ Center.X},{ Center.Y })"
                    Else
                        _rVal = $"(0,{ 2 * Radius },{ Center.X },{ Center.Y })"
                    End If
                End If
                Return _rVal
            End Get
            Set(value As String)
                value = dxfUtils.StripParens(value)
                If value = String.Empty Then Return

                Dim vStr As String = value
                Dim vals() As String = vStr.Split(",")


                If vals.Length < 4 Then Return
                SetPropVal("*Descriptor", value, False)
                If TVALUES.To_INT(vals(0)) = 0 Then 'round hole
                    Diameter = vals(1)
                    Center.X = vals(2)
                    Center.Y = vals(3)
                    Center.Z = 0
                Else
                    Length = vals(1)
                    Diameter = vals(2)
                    If vals.Length - 1 >= 5 Then
                        Rotation = vals(3)
                        Center.X = vals(4)
                        Center.Y = vals(5)
                    Else
                        Center.X = vals(3)
                        Center.Y = vals(4)
                    End If
                    Center.Z = 0
                End If
                Return
            End Set
        End Property

        Public ReadOnly Property Volumn As Double
            Get
                '^returns the volume if the hole in sqr units
                Return Area() * Depth
            End Get
        End Property
        Public Property WeldedBolt As Boolean
            Get
                '^true if the hole is intended for a bolt tack welded into place
                Return PropValueB("Welded Bolt")
            End Get
            Set(value As Boolean)
                '^true if the hole is intended for a bolt tack welded into place
                SetPropVal("Welded Bolt", value, False)
            End Set
        End Property
        Public Shadows Property Width As Double
            Get
                Return Diameter
            End Get
            Set(value As Double)
                Diameter = value
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Throw New Exception("dxeHoles cannot be defined by Object")
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeHole
            Dim _rVal As dxeHole = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeHole
            Return New dxeHole(Me)
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            If aInstances Is Nothing Then
                aInstances = Instances
                aInstances.IsParent = True
                aInstances.ParentPlane = PlaneV
            End If
            GetImage(aImage)
            Return New TPROPERTYARRAY(SubEntities.DXFProps(aInstances, aInstance, New dxfPlane(aOCS), rTypeName, aImage))
        End Function
#End Region 'MustOverride Entity Methods
#Region "Methods"

        Public Function SubEntities(Optional aGroupName As Object = Nothing) As colDXFEntities

            Dim gname As String
            If aGroupName IsNot Nothing Then gname = aGroupName.ToString() Else gname = GroupName
            Dim _rval As New colDXFEntities
            _rval.ArcLineStructures_Set(dxfPrimatives.CreateHoleSegments(Me))
            _rval.SetGroupName(gname)
            Return _rval

        End Function


        Public Function AnglePoint(aAngle As Double, Optional bInRadians As Boolean = False) As dxfVector
            '^returns a point on the perimenter segments at the requested angle
            Dim aPerim As dxePolyline = Boundary()
            Dim aL As dxeLine
            Dim Segs As colDXFEntities = aPerim.Segments
            Dim iPts As colDXFVectors
            Dim aPl As TPLANE = aPerim.BoundingRectangle.Strukture
            aL = New dxeLine With {.StartPtV = aPl.Origin, .EndPtV = aPl.AngleVector(aAngle, 5 * Length, bInRadians)}
            iPts = aL.Intersections(Segs, False, False, False)
            Return iPts.Item(1)
        End Function
        Public Function AsViewedFrom(ViewVector As String, Optional aCS As dxfPlane = Nothing) As dxeHole
            Dim _rVal As dxeHole = Nothing
            '#1the name of the axis to look at the hole from
            '^returns a clone of the hole as view from the indicated direction axis
            '~the holes are assumed to be defined with respect to the standard xyz coordinate system.
            '~the passed argument must be either "X" or "Y"
            ViewVector = Trim(UCase(ViewVector))
            _rVal = Clone()
            If ViewVector <> "X" And ViewVector <> "Y" And ViewVector <> "Z" Then Return _rVal
            Dim cx As Double
            Dim cy As Double
            Dim cz As Double
            Dim aPl As TPLANE = PlaneV
            Dim bPl As New TPLANE(aCS)
            Dim aX As TVECTOR
            Dim aY As TVECTOR
            Dim aZ As TVECTOR
            Dim bZ As TVECTOR
            Dim ang As Double
            Dim aV As TVECTOR
            Dim aN As TVECTOR
            Dim bFlag As Boolean

            Center.GetComponents(cx, cy, cz)
            aX = aPl.XDirection
            aY = aPl.YDirection
            aZ = aPl.ZDirection
            bZ = bPl.ZDirection
            If ViewVector = "X" Then
                aV = bPl.XDirection
            ElseIf ViewVector = "Y" Then
                aV = bPl.YDirection
            Else
                aV = bPl.ZDirection
            End If
            If aV.Equals(aPl.ZDirection, True, 3, bFlag) Then
                bPl = aPl
                If Not bPl.ZDirection.Equals(bZ, True, 3, bFlag) Then
                    ang = bPl.ZDirection.AngleTo(bZ, bPl.ZDirection)
                    If ang <> 0 Then
                        aN = bPl.ZDirection.CrossProduct(bZ, True)
                        bPl.RotateAbout(bPl.Origin, aN, ang)
                    End If
                End If
                bPl = bPl.AlignedTo(bZ, dxxAxisDescriptors.Z)
            Else
                bPl.RotateAbout(bPl.Origin, bPl.XDirection, -90)
                bPl = bPl.AlignedTo(aZ, dxxAxisDescriptors.Z)
                If aV.Equals(aX, True, 3, bFlag) Then
                    ang = bPl.XDirection.AngleTo(bZ, bPl.ZDirection)
                    If ang <> 0 Then
                        aN = bPl.XDirection.CrossProduct(bZ, True)
                        bPl.RotateAbout(bPl.Origin, aN, ang)
                    End If
                ElseIf aV.Equals(aY, True, 3, bFlag) Then
                    ang = bPl.YDirection.AngleTo(bZ, bPl.ZDirection)
                    If ang <> 0 Then
                        aN = bPl.YDirection.CrossProduct(bZ, True)
                        bPl.RotateAbout(bPl.Origin, aN, ang)
                    End If
                End If
            End If
            If Rotation <> 0 Then
                bPl.Revolve(Rotation, False)
            End If
            _rVal.Rotation = 0
            _rVal.PlaneV = bPl
            '==========================================
            If ViewVector = "X" Then
                '==========================================
                _rVal.Y = cy
                _rVal.X = -cz
                _rVal.Z = cx
                '==========================================
            ElseIf ViewVector = "Y" Then
                '==========================================
                _rVal.Y = cz
                _rVal.Z = cy
                _rVal.X = cx
                '==========================================
            Else
                '==========================================
                _rVal.Y = cx
                _rVal.Z = cy
                _rVal.X = cz
            End If
            Return _rVal
        End Function
        Public Function Boundary() As dxePolyline
            Return New dxePolyline With {
                .DisplayStructure = DisplayStructure,
                .Closed = True,
                .VerticesV = dxfPrimatives.CreateHoleVertices(Radius, MinorRadius, Length, IsSquare, Rotation, PlaneV),
                .PlaneV = PlaneV,
                .Identifier = "HOLE"
            }
        End Function
        Public Function BoundingEntity(Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0, Optional aMirrorLine As dxeLine = Nothing) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            Dim aAr As dxeArc
            Dim aPl As dxePolyline
            If EntityType = dxxEntityTypes.Hole And (MinorRadius > Radius Or MinorRadius <= 0) And Not IsSquare Then
                aAr = New dxeArc With {
                    .PlaneV = PlaneV
                }
                aAr.CenterV += New TVECTOR(aXOffset, aYOffset, aZOffset)
                If aMirrorLine IsNot Nothing Then
                    If aMirrorLine.Length <> 0 Then aAr.CenterV.Mirror(New TLINE(aMirrorLine), True)
                End If
                aAr.DisplayStructure = DisplayStructure
                aAr.Radius = Radius
                aAr.LCLSet(aLayerName, aColor, aLineType)
                aAr.TFVCopy(Me)
                aAr.Identifier = "HOLE"
                aAr.ImageGUID = ImageGUID
                _rVal = aAr
            Else
                aPl = New dxePolyline With {
                    .DisplayStructure = DisplayStructure,
                    .PlaneV = PlaneV,
                    .Closed = True,
                    .VerticesV = dxfPrimatives.CreateHoleVertices(Radius, MinorRadius, Length, IsSquare, Rotation, PlaneV, aXOffset, aYOffset, aZOffset, aMirrorLine),
                    .Identifier = "HOLE",
                    .Tag = Tag,
                    .Flag = Flag,
                    .Value = Value,
                    .ImageGUID = ImageGUID
                }
                aPl.LCLSet(aLayerName, aColor, aLineType)
                _rVal = aPl
            End If
            Return _rVal
        End Function
        Public Function CopyToPlane(aPlane As dxfPlane) As dxeHole
            Dim _rVal As dxeHole = Clone()
            If Not dxfPlane.IsNull(aPlane) Then
                Dim bPln As TPLANE = PlaneV.AlignedTo(aPlane.ZDirectionV, dxxAxisDescriptors.Z)
                bPln.AlignTo(aPlane.XDirectionV, dxxAxisDescriptors.X)
                _rVal.PlaneV = bPln
                _rVal.CenterV = bPln.Origin
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim j As Integer
            Dim iCnt As Integer
            Dim aSubEnts As colDXFEntities
            Dim aEnt As dxfEntity
            If aInstances Is Nothing Then
                aInstances = Instances
                aInstances.IsParent = True
                aInstances.ParentPlane = PlaneV
            End If
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            aSubEnts = SubEntities()
            If aSubEnts.Count <= 0 Then Return _rVal
            For j = 1 To aSubEnts.Count
                aEnt = aSubEnts.Item(j)
                If aInstance <= 0 Or i = aInstance Then _rVal.Append(aEnt.DXFFileProperties(aInstances, aImage, Nothing), True)
            Next j
            _rVal.Name = "HOLE"
            If iCnt > 1 Then
                _rVal.Name += "-" & iCnt & " INSTANCES"
            End If
            Return _rVal
        End Function
        Public Sub DefineByString(sDescriptor As String)
            '#1the descriptor string of a hole to extract the holes properties from
            '^used th set the properties of the hole based on the values in the passed comma delimated string
            '~see dxeHole.Descriptor
            Try
                sDescriptor = sDescriptor.Trim()
                If sDescriptor = "" Then Throw New Exception("Null String Passed")

                If sDescriptor.Contains("(") Then sDescriptor = dxfUtils.LeftOf(sDescriptor, "(")
                If sDescriptor.EndsWith(")") Then sDescriptor = dxfUtils.LeftOf(sDescriptor, ")")
                Dim aDir As String
                Dim vals() As String
                Dim cnt As Integer
                Dim wuz As Boolean
                wuz = SuppressEvents
                SuppressEvents = True
                Dim d1 As String
                Dim d2 As String
                Dim aPlane As dxfPlane
                aPlane = Plane
                d1 = Descriptor
                vals = sDescriptor.Split(",")
                cnt = vals.Length - 1
                If cnt >= 4 Then Center.SetCoordinates(Val(vals(1)), TVALUES.To_DBL(vals(2)), TVALUES.To_DBL(vals(3)))
                If cnt >= 5 Then Radius = TVALUES.To_DBL(vals(4))
                If cnt >= 6 Then Length = TVALUES.To_DBL(vals(5))
                If cnt >= 7 Then Rotation = TVALUES.To_DBL(vals(6))
                If cnt >= 8 Then Depth = TVALUES.To_DBL(vals(7))
                If cnt >= 9 Then DownSet = TVALUES.To_DBL(vals(8))
                If cnt >= 10 Then Inset = TVALUES.To_DBL(vals(9))
                If cnt >= 11 Then IsSquare = TVALUES.To_INT(vals(10))
                If cnt >= 12 Then Tag = vals(11)
                If cnt >= 13 Then Flag = vals(12)
                If cnt >= 14 Then MinorRadius = TVALUES.To_DBL(vals(13))
                aDir = "(0,0,1) "
                If cnt >= 15 Then aDir = Replace(vals(14), dxfGlobals.Delim, ",")
                aPlane.ZDirection.Components = aDir
                aDir = "(1,0,0) "
                If cnt >= 16 Then aDir = Replace(vals(15), dxfGlobals.Delim, ",")
                aPlane.XDirection.Components = aDir
                PlaneV = New TPLANE(aPlane)
                d2 = Descriptor
                SuppressEvents = wuz
            Catch ex As Exception
                Throw New Exception("dxeHole.DefineByString Invalid Hole Descriptor String Was Passed - " & ex.Message)
            End Try
        End Sub
        Public Function HorizontalCenterLine(Optional aPlane As dxfPlane = Nothing, Optional aColor As dxxColors = dxxColors.Red, Optional aLineType As String = "Center") As dxeLine
            '^returns the centerline of the hole as viewed in the passed image
            UpdatePath()
            Dim bPlane As dxfPlane
            If TPLANE.IsNull(aPlane) Then bPlane = Plane Else bPlane = aPlane
            Dim bRec As TPLANE = Components.Paths.ExtentVectors.Bounds(bPlane.Strukture)
            Dim v1 As TVECTOR = CenterV.ProjectedTo(bRec)
            Dim _rVal As New dxeLine With {
                .Tag = Tag,
                .Flag = Flag,
                .Identifier = Identifier,
                .GroupName = GroupName,
                .DisplayStructure = DisplayStructure,
                .StartPtV = v1 + bRec.XDirection * -(0.075 * Radius + 0.5 * bRec.Width),
                .EndPtV = v1 + bRec.XDirection * (0.075 * Radius + 0.5 * bRec.Width)
            }
            If aColor <> dxxColors.Undefined Then _rVal.Color = aColor
            If Trim(aLineType) <> "" Then _rVal.Linetype = aLineType
            Return _rVal
        End Function
        Friend Function IntersectsPlaneV(aPlane As TPLANE) As Boolean
            Dim _rVal As Boolean = False
            Dim bPlane As TPLANE = PlaneV
            Dim rCoPlanar As Boolean = False
            If aPlane.IsCoplanar(bPlane) Then
                _rVal = True
            Else
                Dim dpth As Double
                dpth = Depth
                If dpth > 0 Then
                    Dim iLine As New TLINE(bPlane.Origin + bPlane.ZDirection * (0.5 * dpth), bPlane.Origin + bPlane.ZDirection * (-0.5 * dpth))
                    dxfIntersections.LinePlane(iLine, aPlane, _rVal, rCoPlanar)
                End If
            End If
            Return _rVal
        End Function
        Public Function IsEqual(aHole As dxeHole, Optional bCompareCenters As Boolean = True) As Boolean
            If aHole Is Nothing Then Return False
            '#1the hole to compare to
            '^returns True if the passed hole is identical to this instance
            If aHole.HoleType <> HoleType Then Return False
            If Math.Abs(aHole.Radius - Radius) > 0.001 Then Return False
            If Math.Abs(aHole.Depth - Depth) > 0.001 Then Return False
            If Not PlaneV.IsEqualTo(aHole.PlaneV, True, False, False, 3) Then Return False
            If bCompareCenters Then
                If Not CenterV.Equals(aHole.CenterV, 4) Then Return False
            End If
            If Math.Abs(aHole.Rotation - Rotation) > 0.001 Then Return False
            If aHole.IsSquare <> IsSquare Then Return False
            Return True
        End Function
        Public Function IsSimiliar(aHole As dxeHole) As Boolean
            '^returns true if the passed hole has the same properties as the hole
            If aHole Is Nothing Then Return False
            If aHole.HoleType <> HoleType Then Return False
            If PlaneV.IsEqualTo(aHole.PlaneV, True, False, False, 3) Then Return False
            If aHole.Rotation <> Rotation Then Return False
            If aHole.IsSquare <> IsSquare Then Return False
            If aHole.Radius <> Radius Then Return False
            If HoleType = HoleType Then
                If aHole.MinorDiameter <> MinorDiameter Then Return False
            Else
                If aHole.Length <> Length Then Return False
            End If
            Return True
        End Function
        Public Function LeaderText(aDimStyle As dxoDimStyle, ByRef Count As Long, Optional TwoLines As Boolean = True, Optional aMultiplier As Double = 1) As String
            Dim _rVal As String = String.Empty
            '#1the dim style to use to foramt the numbers
            '#2the hole to build the description based on
            '#3the number of holes to put in the description
            '#4flag to return a string with a line break or a single line of text
            '^creates the string used to leader a hole with its desciption
            '~like "0.5 Hole 4 Places" or "1.0 x 3.0 Slot 18 Places"
            If aDimStyle IsNot Nothing Then
                If EntityType = dxxEntityTypes.Hole Then
                    If Not IsSquare Then
                        _rVal = "%%C" & aDimStyle.FormatNumber(Diameter / aMultiplier) & " HOLE"
                    Else
                        _rVal = aDimStyle.FormatNumber(Diameter / aMultiplier) & " SQR. HOLE"
                    End If
                Else
                    _rVal = aDimStyle.FormatNumber(Width / aMultiplier) & " X " & aDimStyle.FormatNumber(Length / aMultiplier) & " SLOT"
                End If
            Else
                If EntityType = dxxEntityTypes.Hole Then
                    If Not IsSquare Then
                        _rVal = "%%C" & Format(Diameter / aMultiplier, "0.0##") & " HOLE"
                    Else
                        _rVal = aDimStyle.FormatNumber(Diameter / aMultiplier) & " SQR. HOLE"
                    End If
                Else
                    _rVal = Format(Width / aMultiplier, "0.0##") & " X " & aDimStyle.FormatNumber(Length / aMultiplier) & " SLOT"
                End If
            End If
            If Count > 1 Then
                If TwoLines Then
                    _rVal += vbLf & Count & " PLACES"
                Else
                    _rVal += " " & Count & " PLACES"
                End If
            End If
            Return _rVal
        End Function
        Public Function Perimeter(Optional bAsLines As Boolean = False, Optional aCurveDivisions As Double = 20, Optional bClosed As Boolean = False) As dxePolyline
            '^returns the holes bounds as a Polyline
            If Not bAsLines Then
                Return Boundary()
            Else
                Dim _rVal As New dxePolyline(Boundary.Segments.PolylineVertices(bAsLines, aCurveDivisions, True), bClosed, DisplaySettings, Plane) With {
                    .Identifier = "HOLE"
            }
                'identifier real important for gridding
                Return _rVal
            End If
        End Function
        Public Sub SetDimensions(Optional aRadius As Object = Nothing, Optional aLength As Object = Nothing, Optional aDepth As Object = Nothing, Optional aMinorRadius As Object = Nothing)
            'On Error Resume Next
            If TVALUES.IsNumber(aRadius) Then Radius = TVALUES.To_DBL(aRadius, aDefault:=Radius)
            If TVALUES.IsNumber(aLength) Then Length = TVALUES.To_DBL(aLength, aDefault:=Length)
            If TVALUES.IsNumber(aDepth) Then Depth = TVALUES.To_DBL(aDepth, aDefault:=Depth)
            If TVALUES.IsNumber(aMinorRadius) Then MinorRadius = TVALUES.To_DBL(aMinorRadius, aDefault:=MinorRadius)
        End Sub
        Public Function VerticalCenterLine(Optional aPlane As dxfPlane = Nothing, Optional aColor As dxxColors = dxxColors.Red, Optional aLineType As String = "Center") As dxeLine
            '^returns the vertical centerline of the hole as viewed in the passed image
            UpdatePath()
            Dim bPlane As dxfPlane
            If TPLANE.IsNull(aPlane) Then bPlane = Plane Else bPlane = aPlane
            Dim bRec As TPLANE = Components.Paths.ExtentVectors.Bounds(bPlane.Strukture)
            Dim v1 As TVECTOR = CenterV.ProjectedTo(bRec)
            Dim _rVal As New dxeLine With {
                .Tag = Tag,
                .Flag = Flag,
                .Identifier = Identifier,
                .GroupName = GroupName,
                .DisplayStructure = DisplayStructure,
                .StartPtV = v1 + bRec.YDirection * -(0.075 * Radius + 0.5 * bRec.Height),
                .EndPtV = v1 + bRec.YDirection * (0.075 * Radius + 0.5 * bRec.Height)
                }
            If aColor <> dxxColors.Undefined Then _rVal.Color = aColor
            If Not String.IsNullOrWhiteSpace(aLineType) Then _rVal.Linetype = aLineType.Trim
            Return _rVal
        End Function

#End Region 'Methods
    End Class 'dxeHole
End Namespace
