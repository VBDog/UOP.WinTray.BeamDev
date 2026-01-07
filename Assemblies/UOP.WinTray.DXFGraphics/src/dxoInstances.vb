
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke

Namespace UOP.DXFGraphics
    Public Class dxoInstances
        Inherits List(Of dxoInstance)
        Implements IEnumerable(Of dxoInstance)
        Implements ICloneable
#Region "Members"


        Private _GraphicType As dxxGraphicTypes
        Private _IsDirty As Boolean
        Private _IsParent As Boolean
        Private _ParentPlane As TPLANE
        Private _Plane As TPLANE

#End Region 'Members
#Region "Events"
        Public Event InstanceChange()
#End Region 'Events
#Region "Constructors"
        Public Sub New()
            'init ------------------------------------------
            _IsDirty = False
            _IsParent = False
            _ParentPlane = TPLANE.World
            _Plane = TPLANE.World
            _GraphicType = dxxGraphicTypes.Undefined
            _OwnerPtr = Nothing
            'init ----------------------------------------------------------------

        End Sub

        Friend Sub New(aOwner As dxfEntity)
            'init ------------------------------------------
            _IsDirty = False
            _IsParent = False
            _ParentPlane = TPLANE.World
            _Plane = TPLANE.World
            _GraphicType = dxxGraphicTypes.Undefined
            _OwnerPtr = Nothing
            'init ----------------------------------------------------------------
            If aOwner Is Nothing Then Return
            _OwnerPtr = New WeakReference(aOwner)
            _GraphicType = aOwner.GraphicType
            _Plane = aOwner.PlaneV
            If aOwner.GraphicType = dxxGraphicTypes.Polyline Then
                _Plane.Origin = aOwner.Vertices.BoundingRectangleV(_Plane).Origin
            End If

        End Sub

        Friend Sub New(aStructure As TINSTANCES, Optional aOwner As dxfEntity = Nothing)
            'init ------------------------------------------
            _IsDirty = aStructure.IsDirty
            _IsParent = aStructure.IsParent
            _ParentPlane = New TPLANE(aStructure.ParentPlane)
            _Plane = New TPLANE(aStructure.Plane)
            _GraphicType = aStructure.GraphicType
            _OwnerPtr = Nothing
            'init ------------------------------------------
            For i As Integer = 1 To aStructure.Count
                AddV(aStructure.Item(i), bSupressUpdate:=True)
            Next
            If aOwner IsNot Nothing Then
                _OwnerPtr = New WeakReference(aOwner)
                _GraphicType = aOwner.GraphicType
                _Plane = aOwner.PlaneV
            End If
        End Sub
        Public Sub New(aInstances As dxoInstances)
            'init ------------------------------------------
            _IsDirty = False
            _IsParent = False
            _ParentPlane = TPLANE.World
            _Plane = TPLANE.World
            _GraphicType = dxxGraphicTypes.Undefined
            _OwnerPtr = Nothing
            'init ------------------------------------------

            If aInstances Is Nothing Then Return
            Copy(aInstances)

        End Sub
        Public Sub New(aBasePt As iVector)
            'init ------------------------------------------
            _IsDirty = False
            _IsParent = False
            _ParentPlane = TPLANE.World
            _Plane = TPLANE.World
            _GraphicType = dxxGraphicTypes.Undefined
            _OwnerPtr = Nothing
            'init ---------------------------------------
            _Plane.Origin = New TVECTOR(aBasePt)

        End Sub
#End Region 'Constructors
#Region "Properties"

        Private _OwnerPtr As WeakReference
        Friend Property Owner As dxfEntity
            Get
                If _OwnerPtr Is Nothing Then Return Nothing
                If Not _OwnerPtr.IsAlive Then
                    _OwnerPtr = Nothing
                    Return Nothing
                End If
                Dim _rVal As dxfEntity = TryCast(_OwnerPtr.Target, dxfEntity)
                _Plane = _rVal.PlaneV
                'If _rVal.GraphicType = dxxGraphicTypes.Polyline Then
                '    _Plane.Origin = _rVal.Vertices.BoundingRectangleV(_Plane).Origin
                'End If

                Return _rVal
            End Get
            Set(value As dxfEntity)
                If value Is Nothing Then
                    _OwnerPtr = Nothing
                Else
                    _OwnerPtr = New WeakReference(value)
                    _GraphicType = value.GraphicType
                    _Plane = value.PlaneV
                    'If value.GraphicType = dxxGraphicTypes.Polyline Then
                    '    _Plane.Origin = value.Vertices.BoundingRectangleV(_Plane).Origin
                    'End If

                End If

            End Set
        End Property

        Friend Property GraphicType As dxxGraphicTypes
            Get
                Return _GraphicType
            End Get
            Set(value As dxxGraphicTypes)
                _GraphicType = value
            End Set
        End Property
        Public Property IsDirty As Boolean
            Get
                Return _IsDirty
            End Get
            Set(value As Boolean)
                _IsDirty = value
            End Set
        End Property
        Public Property IsParent As Boolean
            Get
                Return _IsParent
            End Get
            Set(value As Boolean)
                _IsParent = value
            End Set
        End Property
        Friend Property ParentPlane As TPLANE
            Get
                Return _ParentPlane
            End Get
            Set(value As TPLANE)
                _ParentPlane = value
            End Set
        End Property
        Public Property BasePlane As dxfPlane
            Get
                Return New dxfPlane(_Plane)
            End Get
            Set(value As dxfPlane)
                _Plane = New TPLANE(value)

            End Set
        End Property
        Friend Property Plane As TPLANE
            Get
                Return _Plane
            End Get
            Set(value As TPLANE)
                _Plane = value
            End Set
        End Property

#End Region 'Properties
#Region "Methods"

        Friend Function IsEqual(aInstances As TINSTANCES) As Boolean

            If Count <> aInstances.Count Then Return False
            If Not aInstances.Plane.ZDirection.Equals(Plane.ZDirection, 6) Then Return False
            Dim i As Integer
            For i = 1 To aInstances.Count
                If New dxoInstance(aInstances.Item(i)) <> Item(i) Then Return False
            Next i
            Return True
        End Function

        Friend Sub Copy(aInstances As TINSTANCES, Optional bSuppressPlane As Boolean = False)
            If Not bSuppressPlane And _OwnerPtr Is Nothing Then _Plane = New TPLANE(aInstances.Plane)
            _ParentPlane = New TPLANE(aInstances.ParentPlane)
            _IsDirty = aInstances.IsDirty
            _IsParent = aInstances.IsParent
            _GraphicType = aInstances.GraphicType
            MyBase.Clear()
            For i As Integer = 1 To aInstances.Count
                MyBase.Add(New dxoInstance(aInstances.Item(i)))
            Next
        End Sub




        Public Shadows Function Item(aIndex As Integer) As dxoInstance


            If aIndex < 0 Or aIndex > Count Then Return Nothing
            Dim _rVal As dxoInstance = MyBase.Item(aIndex - 1)
            _rVal.Index = aIndex
            Return _rVal
        End Function

        Public Function Apply(aIndex As Integer, aEntity As dxfEntity) As dxfEntity

            Dim _rVal As dxfEntity = aEntity.Clone()

            _rVal.Instances = New dxoInstances(_rVal)
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            If aIndex > Count Then Return _rVal

            Dim aTrs As TTRANSFORMS = Transforms(aIndex)

            For i As Integer = 1 To aTrs.Count
                TTRANSFORM.Apply(aTrs.Item(i), aEntity, True)
            Next i

            Return _rVal
        End Function

        Friend Function Apply(aPaths As TPATHS) As TPATHS
            Dim rPaths As TPATHS = aPaths
            If aPaths.Count <= 0 Then Return rPaths
            If Count <= 0 Then Return rPaths

            Dim bPaths As New TPATHS(aPaths)

            For i As Integer = 1 To Count
                Dim aTrs As TTRANSFORMS = Transforms(i)
                Dim cPaths As New TPATHS(bPaths)
                TTRANSFORMS.Apply(aTrs, cPaths, bMirDirs:=False, bNoDirections:=False, bConvertToWorld:=False)
                For j As Integer = 1 To cPaths.Count
                    Dim aPath As TPATH = rPaths.Item(j)
                    Dim cPath As TPATH = cPaths.Item(j)
                    If aPath.Relative Then
                        cPath = cPath.ToRelative(Nothing)
                        rPaths.Add(cPath)
                    Else
                        Dim lcnt As Integer = aPath.LoopCount
                        lcnt += cPath.LoopCount
                        If lcnt < Integer.MaxValue Then
                            For k As Integer = 1 To cPath.LoopCount
                                aPath.AddLoop(cPath.Looop(k))
                            Next k
                            rPaths.SetItem(j, aPath)
                        Else
                            rPaths.Add(cPath)
                        End If
                    End If
                Next j
                'Application.DoEvents()
            Next i
            Return rPaths
        End Function

        Friend Function Apply(aIndex As Integer, aPath As TPATH) As TPATH
            Dim _rVal As TPATH = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TPATH)(aPath)
            If aIndex < 1 Or aIndex > Count Or aPath.LoopCount <= 0 Then Return _rVal
            If aPath.Looop(1).Count <= 0 Then Return _rVal

            Dim aPlane As TPLANE = Plane
            Dim aInst As dxoInstance = Item(aIndex)
            Dim SF As Double = aInst.ScaleFactor
            If SF <= 0 Then SF = 1
            aPlane = Plane
            Dim bPlane As New TPLANE(aPlane)
            Dim xScl As Double = SF
            Dim yScl As Double = SF
            Dim aAdder As New TVECTOR(aInst.XOffset, aInst.YOffset, 0)
            bPlane.Origin += aAdder
            If aInst.Rotation <> 0 Then bPlane.Revolve(aInst.Rotation, False)
            If aInst.Inverted Then yScl = -yScl
            If aInst.LeftHanded Then xScl = -xScl
            Dim aLoop As TVECTORS = aPath.Looop(1)
            For j As Integer = 1 To aLoop.Count
                aLoop.SetItem(j, aLoop.Item(j).TransferedToPlane(aPlane, bPlane, xScl, yScl, SF, 0))
            Next j
            aPath.SetLoop(1, aLoop)
            Return _rVal
        End Function
        Public Function Apply(aVector As dxfVector, Optional bReturnBaseVector As Boolean = True) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aVector Is Nothing Then Return _rVal
            _rVal.Add(aVector)
            Return Apply(_rVal, bReturnBaseVector)
        End Function
        Public Function Apply(aVectors As colDXFVectors, Optional bReturnBaseVectors As Boolean = True, Optional aIndex As Integer? = Nothing) As colDXFVectors
            Dim rVectors As New colDXFVectors
            If aVectors Is Nothing Or aVectors.Count <= 0 Then Return rVectors
            If Count <= 0 Then Return rVectors


            rVectors = aVectors.Clone(Not bReturnBaseVectors)
            If aIndex.HasValue Then
                If aIndex < 0 Or aIndex > Count Then Return rVectors
                Dim aTrs As TTRANSFORMS = Transforms(aIndex.Value)
                'aVecs.Print("iBefore =" & i)
                TTRANSFORMS.Apply(aTrs, rVectors)
                Return rVectors
            Else
                For i As Integer = 1 To Count
                    Dim aTrs As TTRANSFORMS = Transforms(i)
                    Dim aVecs As New colDXFVectors(aVectors, True)
                    'aVecs.Print("iBefore =" & i)
                    TTRANSFORMS.Apply(aTrs, aVecs)
                    rVectors.Append(aVecs)
                    'aVecs.Print("iAfter =" & i)
                    'Application.DoEvents()
                Next i
            End If

            Return rVectors
        End Function

        Friend Function Apply(aVector As TVECTOR, Optional bReturnBaseVector As Boolean = True) As TVECTORS
            Dim _rVal As New TVECTORS(aVector)

            Return Apply(_rVal, bReturnBaseVector)
        End Function
        Friend Function Apply(aVectors As TVECTORS, Optional bReturnBaseVectors As Boolean = True) As TVECTORS
            Dim rVectors As TVECTORS
            If Not bReturnBaseVectors Then
                rVectors = TVECTORS.Zero
            Else
                rVectors = New TVECTORS(aVectors)
            End If
            If aVectors.Count <= 0 Then Return rVectors
            If Count <= 0 Then Return rVectors

            For i As Integer = 1 To Count
                Dim aTrs As TTRANSFORMS = Transforms(i)
                Dim aVecs As New TVECTORS(aVectors)
                'aVecs.Print("iBefore =" & i)
                TTRANSFORMS.Apply(aTrs, aVecs)
                rVectors.Append(aVecs)
                'aVecs.Print("iAfter =" & i)
                'Application.DoEvents()
            Next i
            Return rVectors
        End Function
        Private Sub RaiseChangeEvent()
            RaiseEvent InstanceChange()
            If _OwnerPtr IsNot Nothing Then
                UpdateOwner()
            End If
        End Sub

        Friend Function Transforms(aIndex As Integer) As TTRANSFORMS
            Dim rScaleFactor As Double = 0.0
            Dim rAngle As Double = 0.0
            Dim rInverted As Boolean = False
            Dim rLeftHanded As Boolean = False
            Return Transforms(aIndex, rScaleFactor, rAngle, rInverted, rLeftHanded)
        End Function
        Friend Function Transforms(aIndex As Integer, ByRef rScaleFactor As Double, ByRef rAngle As Double, ByRef rInverted As Boolean, ByRef rLeftHanded As Boolean) As TTRANSFORMS
            If aIndex < 1 Or aIndex > Count Then Return New TTRANSFORMS
            Dim aPlane As TPLANE
            rScaleFactor = 1
            rAngle = 0
            rInverted = False
            rLeftHanded = False
            Dim aInst As dxoInstance = Item(aIndex)
            If Not IsParent Then
                aPlane = Plane
            Else
                aPlane = ParentPlane
            End If
            Return aInst.Transforms(aPlane, rScaleFactor, rAngle, rInverted, rLeftHanded)
        End Function

        Friend Sub UpdateOwner()
            If _OwnerPtr Is Nothing Then Return
            Dim owner As dxfEntity = Me.Owner
            If owner Is Nothing Then Return
            If IsDirty Then owner.IsDirty = True


        End Sub

        Public Shadows Sub Add(aInstance As iInstance)
            If aInstance Is Nothing Then Return
            aInstance.Index = Count + 1
            If aInstance.GetType() Is GetType(dxoInstance) Then
                MyBase.Add(aInstance)
            Else
                MyBase.Add(New dxoInstance(aInstance))
            End If

            IsDirty = True
            RaiseChangeEvent()
        End Sub
        Public Shadows Sub Add(aInstance As dxoInstance, bAddClone As Boolean)
            If aInstance Is Nothing Then Return
            If bAddClone Then
                MyBase.Add(New dxoInstance(aInstance) With {.Index = Count + 1})
            Else
                MyBase.Add(aInstance)
            End If
            IsDirty = True
            RaiseChangeEvent()

        End Sub
        Public Shadows Sub Add(aXYString As String, Optional bThirdValIsRotation As Boolean = False, Optional aDelimitor As String = ",")
            If (String.IsNullOrWhiteSpace(aXYString)) Then Return
            Dim nums As List(Of Double) = TLISTS.ToNumericList(aXYString, aDelimitor)
            If nums.Count <= 0 Then Return
            Dim dx As Double
            Dim dy As Double
            Dim rot As Double
            For i As Integer = 0 To nums.Count - 1
                dx = nums.Item(i)
                If Not bThirdValIsRotation Then
                    If i + 1 < nums.Count Then dy = nums.Item(i + 1) Else dy = 0
                    i += 1
                    AddV(New TINSTANCE(dx, dy), True)
                Else
                    If i + 1 < nums.Count Then dy = nums.Item(i + 1) Else dy = 0
                    If i + 2 < nums.Count Then rot = nums.Item(i + 2) Else rot = 0
                    i += 2
                    AddV(New TINSTANCE(dx, dy, rot), True)
                End If
            Next
            IsDirty = True
            RaiseChangeEvent()

        End Sub
        Public Shadows Sub Add(aXOffset As Double, aYOffset As Double, Optional aScaleFactor As Double = 1, Optional aRotation As Double = 0.0, Optional bInverted As Boolean = False, Optional bLeftHanded As Boolean = False)
            If aScaleFactor <= 0 Then aScaleFactor = 1
            Dim aMem As New dxoInstance With {
                .XOffset = aXOffset,
                .YOffset = aYOffset,
                .ScaleFactor = aScaleFactor,
                .Rotation = TVALUES.NormAng(aRotation, False, True, True),
                .Inverted = bInverted,
                .LeftHanded = bLeftHanded
            }
            Add(aMem)
        End Sub
        Public Sub AddByVector(aVector As dxfVector, aBaseVector As dxfVector, Optional bUseZAsRotation As Boolean = False, Optional bSuppressRotations As Boolean = False, Optional bUseZAsScaleFactor As Boolean = False, Optional bUseInvertedFlag As Boolean = False)
            If aVector Is Nothing Then Return

            If Add(aVector, aBaseVector, bUseZAsRotation, bSuppressRotations, bUseZAsScaleFactor, bUseInvertedFlag) Then
                IsDirty = True
                RaiseChangeEvent()
            End If
        End Sub

        Public Shadows Function Add(aVector As iVector, aBaseVector As iVector, Optional bUseZAsRotation As Boolean = False, Optional bSuppressRotations As Boolean = False, Optional bUseZAsScaleFactor As Boolean = False, Optional bUseInvertedFlag As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If aVector Is Nothing Then Return False
            Dim bv As TVECTOR

            Dim aMem As New dxoInstance
            If aBaseVector Is Nothing Then bv = Plane.Origin Else bv = New TVECTOR(aBaseVector)
            Dim v1 As New TVERTEX(aVector)
            Dim dif As TVECTOR = v1.Vector - bv
            If bUseZAsRotation Then bUseZAsScaleFactor = False
            If bUseZAsRotation Or bUseZAsScaleFactor Then dif.Z = 0
            If Not TVECTOR.IsNull(dif, 4) Then
                _rVal = True
                aMem.XOffset = dif.X
                aMem.YOffset = dif.Y
                If bUseInvertedFlag Then aMem.Inverted = v1.Inverted
                If Not bSuppressRotations Then aMem.Rotation = TVALUES.NormAng(v1.Vector.Rotation, False, True, True)
                If bUseZAsScaleFactor Then
                    If v1.Vector.Z <> 0 Then aMem.ScaleFactor = Math.Abs(v1.Vector.Z) Else aMem.ScaleFactor = 1
                End If
                If bUseZAsRotation Then
                    aMem.Rotation = v1.Vector.Z
                End If
                Add(aMem)
            End If

            Return _rVal
        End Function
        Friend Sub AddV(aNewMem As TINSTANCE, Optional bSupressUpdate As Boolean = False)
            MyBase.Add(New dxoInstance(aNewMem) With {.Index = Count + 1})
            IsDirty = True
            If Not bSupressUpdate Then

                RaiseChangeEvent()
            End If
        End Sub
        Public Shadows Sub Clear()
            If Count <> 0 Then
                MyBase.Clear()
                RaiseChangeEvent()
            End If
        End Sub
        Public Function Clone() As dxoInstances
            Return New dxoInstances(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoInstances(Me)
        End Function

        Public Function Copy(aInstances As dxoInstances, Optional bSuppressPlane As Boolean = False) As Boolean
            Dim _rVal As Boolean = False

            If aInstances Is Nothing Then
                _rVal = Count > 0
                MyBase.Clear()
                Return _rVal
            End If

            If aInstances.Count <> Count Then _rVal = True

            If Not bSuppressPlane And _OwnerPtr Is Nothing Then
                If Not TPLANES.Compare(_Plane, aInstances.Plane, 3, bCompareOrigin:=True) Then
                    _rVal = True

                End If
                _Plane = New TPLANE(aInstances.Plane)
            End If

            Dim idx As Integer = 0
            For Each inst In aInstances
                idx += 1
                If idx > Count Then
                    _rVal = True
                    MyBase.Add(New dxoInstance(inst))
                Else
                    Dim aInst As dxoInstance = Item(idx)

                    If aInst.Copy(aInst) Then _rVal = True
                End If
            Next

            If _rVal Then
                IsDirty = True
                RaiseChangeEvent()
            End If
            Return _rVal
        End Function

        Public Function CreatePolarArray(aRadius As Double, aAngleOrCount As Object, Optional aCenterAngle As Double = 0.0, Optional bAppendTo As Boolean = False, Optional bCountPassed As Boolean = False, Optional bApplyRotations As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            If aRadius = 0 Then Return _rVal
            If Not TVALUES.IsNumber(aAngleOrCount) Then Return _rVal
            Try
                Dim aPl As TPLANE = Plane
                Dim bPl As TPLANE = aPl
                Dim ang As Double
                Dim cnt As Integer
                Dim cang As Double
                Dim rad As Double = Math.Abs(aRadius)


                _rVal = True
                If Not bAppendTo Then Clear()
                If bCountPassed Then
                    cnt = TVALUES.To_INT(aAngleOrCount)
                    ang = 360 / cnt
                Else
                    ang = TVALUES.NormAng(TVALUES.To_DBL(aAngleOrCount), False, True, True)
                    If ang = 0 Then ang = 360
                    cnt = 360 / ang
                End If
                If cnt <= 1 Or ang = 0 Then Return _rVal
                bPl.Origin = aPl.AngleVector(aCenterAngle, rad, False)
                If dxfUtils.IsOdd(cnt) Then
                    cang = ang / 2
                End If
                cang = TVALUES.NormAng(cang + aCenterAngle + 180 + ang, False, True, True)
                cnt -= 1
                For i As Integer = 1 To cnt
                    Dim v1 As TVECTOR = bPl.AngleVector(cang, rad, False)
                    Dim aMem As New TINSTANCE() With
                    {
                    .XOffset = Math.Round(v1.X - aPl.Origin.X, 6),
                    .YOffset = Math.Round(v1.Y - aPl.Origin.Y, 6),
                    .ScaleFactor = 1
                    }
                    If bApplyRotations Then aMem.Rotation = cang
                    AddV(aMem, True)
                    cang += ang
                Next i
            Catch ex As Exception
            End Try
            If _rVal Then
                IsDirty = True
                RaiseChangeEvent()
            End If
            Return _rVal
        End Function

        Public Function CreateRectangularArray(aColumns As Integer, aRows As Integer, aColumnSpace As Double, aRowSpace As Double, Optional aRotation As Double = 0.0, Optional bAppendTo As Boolean = False) As Boolean

            Dim _rVal As Boolean = False
            Try
                aColumnSpace = Math.Round(aColumnSpace, 6)
                aRowSpace = Math.Round(aRowSpace, 6)

                If aColumns < 1 Or aRows < 1 Then Return _rVal
                If aColumnSpace = 0 Then aColumns = 1
                If aRowSpace = 0 Then aRows = 1
                If aColumns * aRows <= 1 Then Return _rVal
                If aColumns > 1 And aColumnSpace = 0 Then Return _rVal
                If aRows > 1 And aRowSpace = 0 Then Return _rVal
                Dim aPl As TPLANE = Plane

                _rVal = True
                If Not bAppendTo Then Clear()
                If aRotation <> 0 Then aPl.Revolve(aRotation, False)
                Dim xDir As TVECTOR = aPl.XDirection
                Dim yDir As TVECTOR = aPl.YDirection * -1
                Dim v1 As TVECTOR = aPl.Origin

                For i As Integer = 1 To aColumns
                    Dim v2 As TVECTOR = v1 + xDir * ((i - 1) * aColumnSpace)
                    For j As Integer = 1 To aRows
                        Dim v3 As TVECTOR = v2 + yDir * ((j - 1) * aRowSpace)
                        If i <> 1 Or j <> 1 Then
                            v3 = v3.WithRespectTo(aPl)
                            Dim aMem As New TINSTANCE With {
                                    .XOffset = v3.X,
                                    .YOffset = v3.Y,
                                    .ScaleFactor = 1
                                }
                            AddV(aMem, True)
                        End If
                    Next j
                Next i
            Catch ex As Exception
            End Try
            If _rVal Then
                IsDirty = True
                RaiseChangeEvent()
            End If
            Return _rVal
        End Function

        Public Sub DefineWithVectors(aVectors As IEnumerable(Of iVector), aBaseVector As iVector, bAppendTo As Boolean, Optional bSuppressRotations As Boolean = False, Optional bUseZAsScaleFactor As Boolean = False, Optional bUseInvertedFlag As Boolean = False)

            If Not bAppendTo Then Clear()
            If aVectors Is Nothing Then Return
            If aVectors.Count <= 0 Then Return

            Dim bv As TVECTOR = _Plane.Origin

            Dim bAng As Double = 0

            If aBaseVector IsNot Nothing Then
                bv = New TVECTOR(aBaseVector)
                bAng = bv.Rotation
            End If
            _Plane.Origin = bv
            'bv = bv.WithRespectTo(_Plane)
            For Each vi In aVectors
                Dim P1 As New dxfVector(vi)
                Dim v1 As TVERTEX = P1.VertexV
                v1 = v1.WithRespectTo(_Plane)
                'v1.Vector = v1.Vector.WithRespectTo(_Plane)
                '                dif = (v1.Vector - bv).Rounded(6)
                ' dif = New TVECTOR(v1.Vector.X, v1.Vector.Y).Rounded(6)
                If TVECTOR.IsNull(v1.Vector, 4) Then Continue For
                Dim bMem As New TINSTANCE(v1.X, v1.Y)

                If bUseInvertedFlag Then bMem.Inverted = v1.Inverted
                If Not bSuppressRotations Then
                    bMem.Rotation = v1.Vector.Rotation
                    If aBaseVector IsNot Nothing Then
                        bMem.Rotation -= bAng
                    End If
                End If
                If bUseZAsScaleFactor Then
                    If v1.Z <> 0 Then bMem.ScaleFactor = Math.Abs(v1.Z) Else bMem.ScaleFactor = 1
                End If
                bMem.Tag = P1.Tag
                AddV(bMem, True)

            Next

            RaiseChangeEvent()
        End Sub

        Friend Function FromVertices(aVectors As TVERTICES, aPlane As TPLANE, Optional aBaseID As Integer = 1, Optional aBaseAngle As Double = 0.0, Optional bIgnoreRotations As Boolean = False, Optional bAppendTo As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            '^creates a vector array by adding each of the passed array to the passed base pt.
            If Not bAppendTo Then Clear()
            If aVectors.Count <= 1 Then Return _rVal
            Try


                If aBaseID < 1 Then aBaseID = 1
                If aBaseID > aVectors.Count Then aBaseID = aVectors.Count
                Dim bpt As TVECTOR = aVectors.Vector(aBaseID)
                _Plane.Origin = bpt

                For i As Integer = 1 To aVectors.Count
                    If i <> aBaseID Then
                        Dim v1 As TVERTEX = aVectors.Item(i)
                        Dim dif As TVECTOR = v1.Vector - bpt
                        If Not TVECTOR.IsNull(dif, 5) Then
                            Dim aInst As New TINSTANCE(aXOffset:=dif.X, aYOffset:=dif.Y, aScaleFactor:=1)
                            If Not bIgnoreRotations Then aInst.Rotation = aBaseAngle + v1.Vector.Rotation
                            AddV(aInst, True)
                            _rVal = True
                        End If
                    End If
                Next i
            Catch ex As Exception
            End Try
            If _rVal Then
                IsDirty = True
                RaiseChangeEvent()
            End If
            Return _rVal
        End Function

        Friend Sub DefineWithVectors(aBaseVector As iVector, aVertices As TVERTICES, bAppendTo As Boolean, Optional bSuppressRotations As Boolean = False, Optional bUseZAsScaleFactor As Boolean = False, Optional bUseInvertedFlag As Boolean = False)

            If Not bAppendTo Then
                If Count > 0 Then IsDirty = True
                MyBase.Clear()
            End If

            If aVertices.Count <= 0 Then Return
            Dim P1 As dxfVector
            Dim bv As TVECTOR
            Dim v1 As TVERTEX
            Dim dif As TVECTOR
            Dim bMem As TINSTANCE
            Dim bAng As Double
            If aBaseVector Is Nothing Then
                bv = _Plane.Origin
            Else
                bv = New TVECTOR(aBaseVector)
                bAng = bv.Rotation
            End If
            _Plane.Origin = bv
            bv = bv.WithRespectTo(_Plane)
            For i As Integer = 1 To aVertices.Count
                P1 = aVertices.Item(i).Vector
                v1 = P1.VertexV
                v1.Vector = v1.Vector.WithRespectTo(_Plane)
                dif = (v1.Vector - bv).Rounded(6)
                If TVECTOR.IsNull(dif, 4) Then
                    bMem = New TINSTANCE()
                    bMem.XOffset = dif.X
                    bMem.YOffset = dif.Y
                    If bUseInvertedFlag Then bMem.Inverted = v1.Inverted
                    If Not bSuppressRotations Then
                        bMem.Rotation = v1.Vector.Rotation
                        If aBaseVector IsNot Nothing Then
                            bMem.Rotation -= bAng
                        End If
                    End If
                    If bUseZAsScaleFactor Then
                        If v1.Vector.Z <> 0 Then bMem.ScaleFactor = Math.Abs(v1.Vector.Z) Else bMem.ScaleFactor = 1
                    End If
                    AddV(bMem, True)
                End If
            Next i

        End Sub
        Public Function MemberPoint(aIndex As Integer, Optional aBasePt As dxfVector = Nothing, Optional aCoordinatesOnly As Boolean = False, Optional bApplyRotations As Boolean = False) As dxfVector

            If aIndex < 1 Or aIndex > Count Then Return Nothing
            Try
                Dim aMem As dxoInstance = Item(aIndex)
                Dim vrt As New TVERTEX
                If aBasePt Is Nothing Then vrt.Vector = _Plane.Origin Else vrt.Vector = aBasePt.Strukture
                If aMem IsNot Nothing Then
                    Dim v1 As TVECTOR = vrt.Vector + _Plane.XDirection * aMem.XOffset + _Plane.YDirection * aMem.YOffset

                    If bApplyRotations And aMem.Rotation <> 0 Then
                        v1.RotateAbout(_Plane.ZAxis, aMem.Rotation)


                    End If
                    vrt.Vector = v1

                    If Not aCoordinatesOnly Then
                        vrt.Inverted = aMem.Inverted
                        vrt.Rotation = aMem.Rotation
                    End If

                End If
                Return New dxfVector(vrt)



            Catch
                Return Nothing
            End Try
            Return Nothing
        End Function

        Public Function UpdateMembers(Optional aXOffsetAdder As Double? = Nothing, Optional aYOffsetAdder As Double? = Nothing, Optional aRotationAdder As Double? = Nothing, Optional bSwapInverted As Boolean = False, Optional bSwapleftHanded As Boolean = False)
            If Count <= 0 Then Return False
            Dim _rVal As Boolean
            For Each mem As dxoInstance In Me
                If mem.Update(aXOffsetAdder, aYOffsetAdder, aRotationAdder, bSwapInverted, bSwapleftHanded) Then
                    _rVal = True
                End If
            Next
            If _rVal Then UpdateOwner()
            Return _rVal
        End Function

        Public Function MemberPoints(Optional aBasePt As iVector = Nothing, Optional aReturnBasePt As Boolean = False, Optional aCoordinatesOnly As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            Try
                Dim bpt As dxfVector
                If aBasePt IsNot Nothing Then bpt = New dxfVector(aBasePt) Else bpt = New dxfVector(_Plane.Origin)
                If aReturnBasePt Then
                    _rVal.Add(bpt)
                End If
                For i As Integer = 1 To Count
                    _rVal.Add(MemberPoint(i, bpt, aCoordinatesOnly))
                Next i
            Catch
            End Try
            Return _rVal
        End Function
        Public Function Multiply(aXOffset As Double, aYOffset As Double, Optional aRotation As Double = 0.0, Optional bAddOne As Boolean = True) As Boolean

            Dim _rVal As Boolean

            Dim v1 As New TVECTOR(aXOffset, aYOffset, 0)
            Dim cnt As Integer
            Dim aMem As dxoInstance

            If TVECTOR.IsNull(v1, 6) Then Return _rVal
            _rVal = True
            cnt = Count
            If bAddOne Then
                aMem = New dxoInstance(v1.X, v1.Y, TVALUES.NormAng(aRotation, False, True, True))
                Add(aMem)
            End If
            For i As Integer = 1 To cnt
                aMem = Item(i)
                aMem.XOffset += v1.X
                aMem.YOffset += v1.Y
                If aRotation <> 0 Then aMem.Rotation = TVALUES.NormAng(aMem.Rotation + aRotation, False, True, True)

            Next i

            If _rVal Then
                IsDirty = True
                RaiseChangeEvent()
            End If
            Return _rVal
        End Function
        Public Shadows Sub Remove(aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            MyBase.RemoveAt(aIndex - 1)
            RaiseChangeEvent()

        End Sub
        Friend Function Transformations(aIndex As Integer) As TTRANSFORMS
            Dim rScaleFactor As Double
            Dim rAngle As Double
            Dim rInverted As Boolean
            Dim rLeftHanded As Boolean
            Return Transformations(aIndex, rScaleFactor, rAngle, rInverted, rLeftHanded)
        End Function
        Friend Function Transformations(aIndex As Integer, ByRef rScaleFactor As Double, ByRef rAngle As Double, ByRef rInverted As Boolean, ByRef rLeftHanded As Boolean) As TTRANSFORMS

            If aIndex < 1 Or aIndex > Count Then Return New TTRANSFORMS
            Dim aPlane As TPLANE
            rScaleFactor = 1
            rAngle = 0
            rInverted = False
            rLeftHanded = False
            Dim aInst As dxoInstance = Item(aIndex)
            If Not IsParent Then
                aPlane = Plane
            Else
                aPlane = ParentPlane
            End If
            Return aInst.Transforms(aPlane, rScaleFactor, rAngle, rInverted, rLeftHanded)

        End Function
        Public Function Update(aIndex As Integer, Optional aXOffset As Double? = Nothing, Optional aYOffset As Double? = Nothing, Optional aScaleFactor As Double? = Nothing, Optional aRotation As Double? = Nothing, Optional bInverted As Boolean? = Nothing, Optional bLeftHanded As Boolean? = Nothing) As Boolean
            Dim _rVal As Boolean = False
            If aIndex < 1 Or aIndex > Count Then Return False
            Dim aMem As dxoInstance = Item(aIndex)
            If aXOffset.HasValue Then
                If aMem.XOffset <> aXOffset.Value Then _rVal = True
                aMem.XOffset = aXOffset.Value
            End If
            If aYOffset.HasValue Then
                If aMem.YOffset <> aYOffset.Value Then _rVal = True
                aMem.YOffset = aYOffset.Value
            End If
            If aScaleFactor.HasValue Then
                If aScaleFactor.Value > 0 Then
                    If aMem.ScaleFactor <> aScaleFactor.Value Then _rVal = True
                    aMem.ScaleFactor = aScaleFactor.Value
                End If
            End If
            If aRotation.HasValue Then
                If aMem.Rotation <> aRotation.Value Then _rVal = True
                aMem.Rotation = aRotation.Value
            End If
            If bInverted.HasValue Then
                If aMem.Inverted <> bInverted.Value Then _rVal = True
                aMem.Inverted = bInverted.Value
            End If
            If bLeftHanded.HasValue Then
                If aMem.LeftHanded <> bLeftHanded.Value Then _rVal = True
                aMem.LeftHanded = bLeftHanded.Value
            End If
            If _rVal Then
                RaiseChangeEvent()
                IsDirty = True
            End If
            Return _rVal

        End Function
        Public Sub PrintToConsole()
            System.Diagnostics.Debug.WriteLine($"INSTANCES COUNT = { Count}")
            If Not IsParent Then
                System.Diagnostics.Debug.WriteLine($"PLANE = { Plane}")
            Else
                System.Diagnostics.Debug.WriteLine($"PLANE(PARENT = { ParentPlane}")
            End If
            For i As Integer = 1 To Count
                Dim mem As dxoInstance = Item(i)
                System.Diagnostics.Debug.WriteLine($"MEMBER({ i })")
                System.Diagnostics.Debug.WriteLine($"   X Offset = { mem.XOffset}")
                System.Diagnostics.Debug.WriteLine($"   Y Offset = { mem.YOffset}")
                System.Diagnostics.Debug.WriteLine($"   ROTATION = { mem.Rotation}")
                System.Diagnostics.Debug.WriteLine($"   SCALE = { mem.ScaleFactor}")
                System.Diagnostics.Debug.WriteLine($"   INVERTED = { mem.Inverted}")
                System.Diagnostics.Debug.WriteLine($"   LEFT HANDED = { mem.LeftHanded}")
            Next i
        End Sub

        Public Function Centers(Optional bGetBasePt As Boolean = True) As colDXFVectors
            Dim _rVal As New colDXFVectors
            dxoInstances.CreateCenters(Me, _rVal, bGetBasePt)
            Return _rVal
        End Function
        Public Function IsEqual(aInstances As dxoInstances)
            Return dxoInstances.InstancesCompare(Me, aInstances)
        End Function

#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function FromVectors(aVectors As colDXFVectors, aBaseVector As dxfVector, Optional bSuppressRotations As Boolean = False, Optional bUseZAsScaleFactor As Boolean = False, Optional bUseInvertedFlag As Boolean = False) As dxoInstances
            Dim _rVal As New dxoInstances
            _rVal.DefineWithVectors(aVectors, aBaseVector, True, bSuppressRotations, bUseZAsScaleFactor, bUseInvertedFlag)
            Return _rVal
        End Function
        Friend Shared Function CreateCenters(aInstances As IEnumerable(Of iInstance), Optional aCollector As List(Of dxfVector) = Nothing, Optional bGetBasePt As Boolean = True) As TVERTICES

            Dim _rVal As TVERTICES = TVERTICES.Zero
            If aInstances Is Nothing Then Return _rVal

            Dim aPlane As TPLANE = TPLANE.World
            If aInstances.GetType() = GetType(dxoInstances) Then
                Dim dxinsts As dxoInstances = DirectCast(aInstances, dxoInstances)
                aPlane = dxinsts.Plane
            End If

            Dim v1 As New TVERTEX(aPlane.Origin)

            If (bGetBasePt) Then
                _rVal.Add(v1)
                If aCollector IsNot Nothing Then aCollector.Add(New dxfVector(v1))

            End If

            For Each inst As iInstance In aInstances
                v1.Vector.Rotation = inst.Rotation
                v1.Vector = aPlane.Origin + (aPlane.XDirection * inst.XOffset)
                v1.Vector += aPlane.YDirection * inst.YOffset
                _rVal.Add(v1)
                If aCollector IsNot Nothing Then aCollector.Add(New dxfVector(v1))
            Next

            Return _rVal

        End Function

        Public Shared Function InstancesCompare(A As dxoInstances, B As dxoInstances) As Boolean
            If A Is Nothing And B Is Nothing Then Return True
            If A Is Nothing Or B Is Nothing Then Return False
            If A.Count <> B.Count Then Return False
            If Not A.Plane.IsEqualTo(B.Plane, bCompareDirections:=True, bCompareDimensions:=False, bCompareOrigin:=False) Then Return False
            Dim bMems As List(Of dxoInstance) = B.OfType(Of dxoInstance)().ToList()
            For Each amem As dxoInstance In A
                Dim idx As Integer = bMems.FindIndex(Function(x) x = amem)
                If idx < 0 Then
                    Return False
                Else
                    bMems.RemoveAt(idx)
                End If

            Next
            Return bMems.Count > 0
        End Function
#End Region 'Shared Methods


#Region "Operators"
        Public Shared Operator =(A As dxoInstances, B As dxoInstances) As Boolean
            Return dxoInstances.InstancesCompare(A, B)
        End Operator
        Public Shared Operator <>(A As dxoInstances, B As dxoInstances) As Boolean
            Return Not dxoInstances.InstancesCompare(A, B)

        End Operator
#End Region 'Operators

    End Class 'dxoInstances

End Namespace
