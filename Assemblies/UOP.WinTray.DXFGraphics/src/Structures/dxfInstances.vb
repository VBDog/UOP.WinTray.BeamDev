

Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures

    Friend Structure TINSTANCE
        Implements ICloneable
#Region "Members"
        Public Index As Integer
        Public Inverted As Boolean
        Public LeftHanded As Boolean
        Public Rotation As Double
        Public ScaleFactor As Double
        Public XOffset As Double
        Public YOffset As Double
        Public Tag As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aXOffset As Double = 0, Optional aYOffset As Double = 0, Optional aRotation As Double = 0, Optional aScaleFactor As Double = 1, Optional bInverted As Boolean = False, Optional bLeftHanded As Boolean = False)
            'init -------------------------------------
            Index = 0
            Inverted = bInverted
            LeftHanded = bLeftHanded
            Rotation = aRotation
            ScaleFactor = aScaleFactor
            XOffset = aXOffset
            YOffset = aYOffset
            Tag = String.Empty
            'init -------------------------------------

        End Sub
        Public Sub New(Optional aTag As String = "")
            'init -------------------------------------
            Index = 0
            Inverted = False
            LeftHanded = False
            Rotation = 0
            ScaleFactor = 1
            XOffset = 0
            YOffset = 0
            Tag = aTag
            'init -------------------------------------

        End Sub
        Public Sub New(aInstance As TINSTANCE)
            'init -------------------------------------
            Index = aInstance.Index
            Inverted = aInstance.Inverted
            LeftHanded = aInstance.LeftHanded
            Rotation = aInstance.Rotation
            ScaleFactor = aInstance.ScaleFactor
            XOffset = aInstance.XOffset
            YOffset = aInstance.YOffset
            Tag = aInstance.Tag
            'init -------------------------------------
        End Sub
        Public Sub New(aInstance As dxoInstance)
            'init -------------------------------------
            Index = 0
            Inverted = False
            LeftHanded = False
            Rotation = 0
            ScaleFactor = 1
            XOffset = 0
            YOffset = 0
            Tag = ""
            'init -------------------------------------
            If aInstance Is Nothing Then Return
            'init -------------------------------------
            Index = aInstance.Index
            Inverted = aInstance.Inverted
            LeftHanded = aInstance.LeftHanded
            Rotation = aInstance.Rotation
            ScaleFactor = aInstance.ScaleFactor
            XOffset = aInstance.XOffset
            YOffset = aInstance.YOffset
            Tag = aInstance.Tag
            'init -------------------------------------
        End Sub

#End Region 'Constructors
#Region "Methods"
        Public Function Clone() As TINSTANCE
            Return New TINSTANCE(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TINSTANCE(Me)
        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String
            If Rotation <> 0 Then
                _rVal = $"TINSTANCE [DX: {XOffset:0.0####} DY: {YOffset:0.0####} R: {Rotation:0.0####}"
            Else
                _rVal = $"TINSTANCE [DX: {XOffset:0.0####} DY: {YOffset:0.0####}"
            End If
            If Inverted Then _rVal += " Inverted"
            If LeftHanded Then _rVal += " LeftHanded"
            If Not String.IsNullOrWhiteSpace(Tag) Then _rVal += $" TAG:{Tag}"
            Return _rVal
        End Function
        Friend Function Transforms(aPlane As TPLANE, ByRef rScaleFactor As Double, ByRef rAngle As Double, ByRef rInverted As Boolean, ByRef rLeftHanded As Boolean) As TTRANSFORMS
            Dim _rVal As New TTRANSFORMS
            rScaleFactor = 1
            rAngle = Rotation
            rInverted = Inverted
            rLeftHanded = LeftHanded
            Dim v1 As TVECTOR
            v1 = (aPlane.XDirection * XOffset) + (aPlane.YDirection * YOffset)
            If ScaleFactor > 0 And ScaleFactor <> 1 Then
                rScaleFactor = ScaleFactor
                _rVal.Add(TTRANSFORM.CreateScale(aPlane.Origin, ScaleFactor, bSuppressEvents:=True))
            End If
            If rAngle <> 0 Then
                _rVal.Add(TTRANSFORM.CreatePlanarRotation(aPlane.Origin, aPlane, rAngle, False, Nothing, dxxAxisDescriptors.Z, Nothing, True))
            End If
            If rLeftHanded Then
                _rVal.Add(TTRANSFORM.CreatePlanarRotation(aPlane.Origin, aPlane, 180, False, Nothing, dxxAxisDescriptors.Y, Nothing, True))
            End If
            If rInverted Then
                _rVal.Add(TTRANSFORM.CreatePlanarRotation(aPlane.Origin, aPlane, 180, False, Nothing, dxxAxisDescriptors.X, Nothing, True))
            End If
            _rVal.Add(TTRANSFORM.CreateTranslation(v1, True))
            Return _rVal
        End Function
#End Region 'Methods
#Region "Operators"
        Public Shared Operator =(A As TINSTANCE, B As TINSTANCE) As Boolean
            If A.Inverted <> B.Inverted Then Return False
            If Math.Round(A.ScaleFactor, 4) <> Math.Round(B.ScaleFactor, 4) Then Return False
            If Math.Round(A.Rotation, 2) <> Math.Round(A.Rotation, 2) Then Return False
            If Math.Round(A.XOffset, 4) <> Math.Round(B.XOffset, 4) Then Return False
            If Math.Round(A.YOffset, 4) <> Math.Round(B.YOffset, 4) Then Return False
            Return True
        End Operator
        Public Shared Operator <>(A As TINSTANCE, B As TINSTANCE) As Boolean
            If A.Inverted = B.Inverted And
                Math.Round(A.ScaleFactor, 4) = Math.Round(B.ScaleFactor, 4) And
                Math.Round(A.Rotation, 2) = Math.Round(A.Rotation, 2) And
                Math.Round(A.XOffset, 4) = Math.Round(B.XOffset, 4) And
                Math.Round(A.YOffset, 4) = Math.Round(B.YOffset, 4) And
                A.LeftHanded = B.LeftHanded Then
                Return True
            Else
                Return False
            End If
        End Operator
#End Region 'Operators
    End Structure 'TINSTANCE
    Friend Structure TINSTANCES
        Implements ICloneable
#Region "Members"

        Public GraphicType As dxxGraphicTypes
        Public IsDirty As Boolean
        Public IsParent As Boolean
        Public ParentPlane As TPLANE
        Public Plane As TPLANE
        Private _Init As Boolean
        Private _Members() As TINSTANCE
#End Region 'Members
#Region "Constructors"
        Public Sub New(aGraphicType As dxxGraphicTypes)
            'init ------------------------------------------
            IsDirty = False
            IsParent = False
            ParentPlane = TPLANE.World
            _Init = True
            ReDim _Members(-1)
            Plane = TPLANE.World
            GraphicType = dxxGraphicTypes.Undefined
            'init ------------------------------------------
            GraphicType = aGraphicType
        End Sub
        Public Sub New(aInstances As TINSTANCES)
            'init ------------------------------------------
            _Init = True
            ReDim _Members(-1)
            Plane = New TPLANE(aInstances.Plane)
            ParentPlane = New TPLANE(aInstances.ParentPlane)
            IsDirty = aInstances.IsDirty
            IsParent = aInstances.IsParent
            GraphicType = aInstances.GraphicType
            'init ------------------------------------------
            If aInstances.Count > 0 Then _Members = aInstances._Members.Clone()
        End Sub

        Public Sub New(aInstances As dxoInstances)
            'init ------------------------------------------
            IsDirty = False
            IsParent = False
            ParentPlane = TPLANE.World
            _Init = True
            ReDim _Members(-1)
            Plane = TPLANE.World
            GraphicType = dxxGraphicTypes.Undefined
            'init ------------------------------------------
            If aInstances Is Nothing Then Return
            GraphicType = aInstances.GraphicType
            Plane = New TPLANE(aInstances.Plane)
            ParentPlane = New TPLANE(aInstances.ParentPlane)
            IsDirty = aInstances.IsDirty
            IsParent = aInstances.IsParent
            GraphicType = aInstances.GraphicType
            For Each inst As dxoInstance In aInstances
                Add(New TINSTANCE(inst))
            Next

        End Sub

#End Region 'Constructors
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property

        Public ReadOnly Property Centers As TVERTICES
            Get
                Dim _rVal As New TVERTICES
                Dim i As Integer
                Dim aPlane As TPLANE = Plane
                Dim v1 As New TVERTEX
                Dim aInst As TINSTANCE
                v1.Vector = aPlane.Origin
                _rVal.Add(v1)
                For i = 1 To Count
                    aInst = Item(i)
                    v1.Vector.Rotation = aInst.Rotation
                    v1.Vector = aPlane.Origin + (aPlane.XDirection * aInst.XOffset)
                    v1.Vector += aPlane.YDirection * aInst.YOffset
                    _rVal.Add(v1)
                Next i
                Return _rVal
            End Get
        End Property


#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"TINSTANCES [{Count}]"
        End Function
        Public Function Clone() As TINSTANCES
            Return New TINSTANCES(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TINSTANCES(Me)
        End Function


        Public Function Add(aVector As dxfVector, aBaseVector As dxfVector, Optional bUseZAsRotation As Boolean = False, Optional bSuppressRotations As Boolean = False, Optional bUseZAsScaleFactor As Boolean = False, Optional bUseInvertedFlag As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If aVector Is Nothing Then Return False
            Dim bv As TVECTOR
            Dim v1 As New TVERTEX
            Dim dif As TVECTOR
            Dim aMem As New TINSTANCE
            If aBaseVector Is Nothing Then bv = Plane.Origin Else bv = aBaseVector.Strukture
            v1 = aVector.VertexV
            dif = v1.Vector - bv
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
                    aMem.Rotation = TVALUES.NormAng(v1.Vector.Z, False, True, True)
                End If
                Add(aMem)
            End If
            Return _rVal
        End Function
        Public Sub Add(aXOffset As Double, aYOffset As Double, Optional aScaleFactor As Double = 1, Optional aRotation As Double = 0.0, Optional bInverted As Boolean = False, Optional bLeftHanded As Boolean = False)
            Dim aMem As New TINSTANCE With {
                    .XOffset = aXOffset,
                    .YOffset = aYOffset,
                    .Rotation = TVALUES.NormAng(aRotation, False, True, True),
                    .Inverted = bInverted,
                    .LeftHanded = bLeftHanded
                }
            If aScaleFactor > 0 Then aMem.ScaleFactor = aScaleFactor Else aMem.ScaleFactor = 1
            Add(aMem)
        End Sub
        Public Function Apply(aIndex As Integer, aEntity As TENTITY) As TENTITY

            Dim _rVal As TENTITY = New TENTITY(aEntity)
            _rVal.Instances = New TINSTANCES(aEntity.DefPts.GraphicType)
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            If aIndex > Count Then Return _rVal
            Dim aEnt As New TENTITY(aEntity)
            Dim aTrs As TTRANSFORMS = Transforms(aIndex)


            For i As Integer = 1 To aTrs.Count
                TTRANSFORM.Apply(aTrs.Item(i), aEnt, True)
            Next i
            _rVal = New TENTITY(aEnt)
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

        Public Function Apply(aPaths As TPATHS) As TPATHS
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
        Public Function Apply(aIndex As Integer, aPath As TPATH) As TPATH
            Dim _rVal As TPATH = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TPATH)(aPath)
            If aIndex < 1 Or aIndex > Count Or aPath.LoopCount <= 0 Then Return _rVal
            If aPath.Looop(1).Count <= 0 Then Return _rVal
            Dim aInst As New TINSTANCE
            Dim aPlane As New TPLANE("")
            Dim bPlane As New TPLANE("")
            Dim j As Integer
            Dim SF As Double
            Dim aAdder As TVECTOR
            Dim xScl As Double
            Dim yScl As Double
            Dim aLoop As TVECTORS
            aInst = Item(aIndex)
            SF = aInst.ScaleFactor
            If SF <= 0 Then SF = 1
            aPlane = Plane
            bPlane = aPlane
            xScl = SF
            yScl = SF
            aAdder = New TVECTOR(aInst.XOffset, aInst.YOffset, 0)
            bPlane.Origin += aAdder
            If aInst.Rotation <> 0 Then bPlane.Revolve(aInst.Rotation, False)
            If aInst.Inverted Then yScl = -yScl
            If aInst.LeftHanded Then xScl = -xScl
            aLoop = aPath.Looop(1)
            For j = 1 To aLoop.Count
                aLoop.SetItem(j, aLoop.Item(j).TransferedToPlane(aPlane, bPlane, xScl, yScl, SF, 0))
            Next j
            aPath.SetLoop(1, aLoop)
            Return _rVal
        End Function
        Friend Function Apply(aVectors As TVECTORS) As TVECTORS
            Dim rVectors As New TVECTORS
            If aVectors.Count <= 0 Then Return rVectors
            If Count <= 0 Then Return rVectors
            Dim i As Integer
            Dim aTrs As New TTRANSFORMS
            Dim aVecs As TVECTORS
            rVectors = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TVECTORS)(aVectors)
            For i = 1 To Count
                aTrs = Transforms(i)
                aVecs = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TVECTORS)(aVectors)
                'aVecs.Print("iBefore =" & i)
                TTRANSFORMS.Apply(aTrs, aVecs)
                rVectors.Append(aVecs)
                'aVecs.Print("iAfter =" & i)
                'Application.DoEvents()
            Next i
            Return rVectors
        End Function
        Public Function Apply(aVector As iVector, Optional bReturnBaseVector As Boolean = True) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aVector Is Nothing Then Return _rVal
            _rVal.Add(aVector)
            Return Apply(_rVal, bReturnBaseVector)
        End Function
        Public Function Apply(aVectors As IEnumerable(Of iVector), Optional bReturnBaseVectors As Boolean = True) As colDXFVectors
            Dim rVectors As New colDXFVectors(aVectors)
            If Not bReturnBaseVectors Then rVectors.Clear()
            If aVectors Is Nothing Or aVectors.Count <= 0 Then Return rVectors
            If Count <= 0 Then Return rVectors


            For i As Integer = 1 To Count
                Dim aTrs As TTRANSFORMS = Transforms(i)
                Dim aVecs As colDXFVectors = New colDXFVectors(aVectors)
                'aVecs.Print("iBefore =" & i)
                TTRANSFORMS.Apply(aTrs, aVecs)
                rVectors.Append(aVecs)
                'aVecs.Print("iAfter =" & i)
                'Application.DoEvents()
            Next i
            Return rVectors
        End Function
        Public Sub Clear()
            If Count > 0 Then IsDirty = True
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Sub Add(aInstance As TINSTANCE)

            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aInstance
            _Members(_Members.Count - 1).Index = _Members.Count
            If _Members(_Members.Count - 1).ScaleFactor <= 0 Then _Members(_Members.Count - 1).ScaleFactor = 1
            IsDirty = True
        End Sub
        Public Function Item(aIndex As Integer) As TINSTANCE
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TINSTANCE("")
            End If
            _Members(aIndex - 1).Index = aIndex
            Return _Members(aIndex - 1)
        End Function
        Public Sub Update(aIndex As Integer, aInstance As TINSTANCE)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = aInstance
            _Members(aIndex - 1).Index = aIndex
        End Sub
        Public Function Update(aIndex As Integer, Optional aXOffset As Double? = Nothing, Optional aYOffset As Double? = Nothing, Optional aScaleFactor As Double? = Nothing, Optional aRotation As Double? = Nothing, Optional bInverted As Boolean? = Nothing, Optional bLeftHanded As Boolean? = Nothing) As Boolean
            Dim _rVal As Boolean = False
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            Dim aMem As New TINSTANCE
            aMem = Item(aIndex)
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
            Update(aIndex, aMem)
            Return _rVal
        End Function
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
            Dim aInst As TINSTANCE = Item(aIndex)
            If Not IsParent Then
                aPlane = Plane
            Else
                aPlane = ParentPlane
            End If
            Return aInst.Transforms(aPlane, rScaleFactor, rAngle, rInverted, rLeftHanded)
        End Function
        Public Function IsEqual(aInstances As TINSTANCES) As Boolean

            If Count <> aInstances.Count Then Return False
            If Not aInstances.Plane.ZDirection.Equals(Plane.ZDirection, 6) Then Return False
            Dim i As Integer
            For i = 1 To aInstances.Count
                If aInstances.Item(i) <> Item(i) Then Return False
            Next i
            Return True
        End Function
        Public Function Multiply(aXOffset As Double, aYOffset As Double, Optional aRotation As Double = 0.0, Optional bAddOne As Boolean = True) As Boolean
            Dim _rVal As Boolean

            Dim v1 As New TVECTOR(aXOffset, aYOffset, 0)
            Dim cnt As Integer
            Dim aMem As TINSTANCE

            If TVECTOR.IsNull(v1, 6) Then Return _rVal
            _rVal = True
            cnt = Count
            If bAddOne Then
                aMem = New TINSTANCE(v1.X, v1.Y, TVALUES.NormAng(aRotation, False, True, True))
                Add(aMem)
            End If
            For i As Integer = 1 To cnt
                aMem = Item(i)
                aMem.XOffset += v1.X
                aMem.YOffset += v1.Y
                If aRotation <> 0 Then aMem.Rotation = TVALUES.NormAng(aMem.Rotation + aRotation, False, True, True)
                Update(i, aMem)
            Next i
            Return _rVal
        End Function
        Public Function Remove(aIndex As Integer) As Boolean
            If aIndex < 1 Or aIndex > Count Then Return False
            Dim newMems As TINSTANCE()
            ReDim newMems(0 To Count - 1)
            Dim idx As Integer
            Dim cnt As Integer = Count
            idx = 0
            For i As Integer = 1 To cnt
                If i <> aIndex Then
                    newMems(idx) = _Members(i - 1)
                    idx += 1
                End If
            Next i
            _Members = newMems
            Return True
        End Function
        Public Function Copy(aInstances As TINSTANCES, Optional bSuppressPlane As Boolean = False) As Boolean
            Dim _rVal As Boolean = False

            If aInstances.Count <> Count Then _rVal = True

            If Not bSuppressPlane Then
                If Not TPLANES.Compare(Plane, aInstances.Plane, 3) Then
                    _rVal = True
                    Plane = New TPLANE(aInstances.Plane)
                End If
            End If
            For i As Integer = 1 To aInstances.Count
                Dim bInst As TINSTANCE = aInstances.Item(i)
                If i > Count Then
                    _rVal = True
                    Add(bInst)
                Else
                    Dim aInst As TINSTANCE = Item(i)

                    If aInst <> bInst Then _rVal = True
                    Update(i, bInst)
                End If
            Next i
            Return _rVal
        End Function

#End Region 'Methods

    End Structure 'TINSTANCES

End Namespace
