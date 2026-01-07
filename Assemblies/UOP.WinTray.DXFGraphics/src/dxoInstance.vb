Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Class dxoInstance
        Implements ICloneable
        Implements iInstance

#Region "Fields"

        Private _Index As Integer
        Private _Inverted As Boolean
        Private _LeftHanded As Boolean
        Private _Rotation As Double
        Private _ScaleFactor As Double
        Private _XOffset As Double
        Private _YOffset As Double
        Private _Tag As String
#End Region 'Fields

#Region "Constructors"


        Public Sub New()
            Init()
        End Sub
        Public Sub New(aInstance As iInstance)
            Copy(aInstance)

        End Sub
        Friend Sub New(aStructure As TINSTANCE)
            Copy(aStructure)
        End Sub
        Public Sub New(Optional aXOffset As Double = 0, Optional aYOffset As Double = 0, Optional aRotation As Double = 0, Optional aScaleFactor As Double = 0, Optional bInverted As Boolean = False, Optional bLeftHanded As Boolean = False)
            Init()
            'init -------------------------------------
            _Inverted = bInverted
            _LeftHanded = bLeftHanded
            _Rotation = TVALUES.NormAng(aRotation, False, True, True)
            _ScaleFactor = aScaleFactor
            _XOffset = aXOffset
            _YOffset = aYOffset
            'init -------------------------------------

        End Sub

        Private Sub Init()
            'init -------------------------------------
            _Index = 0
            _Inverted = False
            _LeftHanded = False
            _Rotation = 0
            _ScaleFactor = 1
            _XOffset = 0
            _YOffset = 0
            _Tag = String.Empty
            'init -------------------------------------
        End Sub
#End Region 'Constructors

#Region "Properties"


        Public Property Tag As String Implements iInstance.Tag
            Get
                Return _Tag
            End Get
            Set(value As String)
                _Tag = value
            End Set
        End Property
        Public Property XOffset As Double Implements iInstance.XOffset
            Get
                Return _XOffset
            End Get
            Set(value As Double)
                _XOffset = value
            End Set
        End Property
        Public Property YOffset As Double Implements iInstance.YOffset
            Get
                Return _YOffset
            End Get
            Set(value As Double)
                _YOffset = value
            End Set
        End Property
        Public Property Rotation As Double Implements iInstance.Rotation
            Get
                Return _Rotation
            End Get
            Set(value As Double)
                _Rotation = TVALUES.NormAng(value, False, True, True)
            End Set
        End Property
        Public Property ScaleFactor As Double Implements iInstance.ScaleFactor
            Get
                Return _ScaleFactor
            End Get
            Set(value As Double)
                _ScaleFactor = value
            End Set
        End Property
        Public Property Inverted As Boolean Implements iInstance.Inverted
            Get
                Return _Inverted
            End Get
            Set(value As Boolean)
                _Inverted = value
            End Set
        End Property

        Public Property Index As Integer Implements iInstance.Index
            Get
                Return _Index
            End Get
            Set(value As Integer)
                _Index = value
            End Set
        End Property

        Public Property LeftHanded As Boolean Implements iInstance.LeftHanded
            Get
                Return _LeftHanded
            End Get
            Set(value As Boolean)
                _LeftHanded = value
            End Set
        End Property
#End Region 'Properties

#Region "Methods"

        Public Function Update(Optional aXOffsetAdder As Double? = Nothing, Optional aYOffsetAdder As Double? = Nothing, Optional aRotationAdder As Double? = Nothing, Optional bSwapInverted As Boolean = False, Optional bSwapLeftHanded As Boolean = False)
            Dim _rVal As Boolean = False
            If aXOffsetAdder.HasValue Then
                If aXOffsetAdder.Value <> 0 Then
                    _rVal = True
                    XOffset += aXOffsetAdder.Value
                End If
            End If
            If aYOffsetAdder.HasValue Then
                If aYOffsetAdder.Value <> 0 Then
                    _rVal = True
                    YOffset += aYOffsetAdder.Value
                End If
            End If
            If aRotationAdder.HasValue Then
                If aRotationAdder.Value <> 0 Then
                    Dim ang1 As Double = Rotation
                    Dim ang2 As Double = TVALUES.NormAng(ang1 + aRotationAdder.Value, False, True, True)
                    If ang1 <> ang2 Then
                        _rVal = True
                        Rotation = ang2
                    End If
                End If
            End If
            If bSwapInverted Then
                _rVal = True
                Inverted = Not Inverted
            End If

            If bSwapLeftHanded Then
                _rVal = True
                LeftHanded = Not LeftHanded
            End If

            Return _rVal
        End Function

        Public Overrides Function ToString() As String
            Dim _rVal As String
            If Rotation <> 0 Then
                _rVal = $"dxoInstance [DX: {XOffset:0.0####} DY: {YOffset:0.0####} R: {Rotation:0.0####}"
            Else
                _rVal = $"dxoInstance [DX: {XOffset:0.0####} DY: {YOffset:0.0####}"
            End If
            If Inverted Then _rVal += " Inverted"
            If LeftHanded Then _rVal += " LeftHanded"
            If Not String.IsNullOrWhiteSpace(Tag) Then _rVal += $" TAG:{Tag}"
            Return _rVal
        End Function

        Public Function Clone() As dxoInstance
            Return New dxoInstance(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoInstance(Me)
        End Function
        Friend Function Copy(aInstance As TINSTANCE)
            Dim _rVal As Boolean =
             _Inverted = aInstance.Inverted And
            Math.Round(_ScaleFactor, 4) = Math.Round(aInstance.ScaleFactor, 4) And
            Math.Round(_Rotation, 2) = Math.Round(aInstance.Rotation, 2) And
            Math.Round(_XOffset, 4) = Math.Round(aInstance.XOffset, 4) And
            Math.Round(_YOffset, 4) = Math.Round(aInstance.YOffset, 4) And
            _LeftHanded = aInstance.LeftHanded

            _Index = aInstance.Index
            _LeftHanded = aInstance.LeftHanded
            _Inverted = aInstance.Inverted
            _ScaleFactor = aInstance.ScaleFactor
            _XOffset = aInstance.XOffset
            _YOffset = aInstance.YOffset
            _Rotation = TVALUES.NormAng(aInstance.Rotation, False, True, True)
            _Tag = aInstance.Tag

            Return _rVal
        End Function

        Public Function Copy(aInstance As iInstance)
            If aInstance Is Nothing Then Return False
            Dim _rVal As Boolean = Me <> aInstance
            _LeftHanded = aInstance.LeftHanded
            _Inverted = aInstance.Inverted
            _ScaleFactor = aInstance.ScaleFactor
            _XOffset = aInstance.XOffset
            _YOffset = aInstance.YOffset
            _Rotation = TVALUES.NormAng(aInstance.Rotation, False, True, True)
            _Tag = aInstance.Tag
            Return _rVal
        End Function

        Friend Function Transforms(aPlane As TPLANE, ByRef rScaleFactor As Double, ByRef rAngle As Double, ByRef rInverted As Boolean, ByRef rLeftHanded As Boolean) As TTRANSFORMS
            Return dxoInstance.GetTransforms(Me, aPlane, rScaleFactor, rAngle, rInverted, rLeftHanded)
        End Function

        Public Function IsEqual(aInstance As iInstance)
            Return dxoInstance.InstanceCompare(Me, aInstance)
        End Function

#End Region 'Methods

#Region "Shared Methods"

        Friend Shared Function GetTransforms(aInstance As iInstance, aPlane As TPLANE, ByRef rScaleFactor As Double, ByRef rAngle As Double, ByRef rInverted As Boolean, ByRef rLeftHanded As Boolean) As TTRANSFORMS

            Dim _rVal As New TTRANSFORMS
            If aInstance Is Nothing Then Return _rVal
            rScaleFactor = 1
            rAngle = aInstance.Rotation
            rInverted = aInstance.Inverted
            rLeftHanded = aInstance.LeftHanded
            Dim v1 As TVECTOR
            v1 = (aPlane.XDirection * aInstance.XOffset) + (aPlane.YDirection * aInstance.YOffset)
            If aInstance.ScaleFactor > 0 And aInstance.ScaleFactor <> 1 Then
                rScaleFactor = aInstance.ScaleFactor
                _rVal.Add(TTRANSFORM.CreateScale(aPlane.Origin, aInstance.ScaleFactor, bSuppressEvents:=True))
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

        Public Shared Function InstanceCompare(A As iInstance, B As iInstance)
            If A Is Nothing And B Is Nothing Then Return True
            If A IsNot Nothing Or B IsNot Nothing Then Return False

            If A.Inverted <> B.Inverted Then Return False
            If A.LeftHanded <> B.LeftHanded Then Return False
            If Math.Round(A.ScaleFactor, 4) <> Math.Round(B.ScaleFactor, 4) Then Return False
            If Math.Round(A.Rotation, 2) <> Math.Round(B.Rotation, 2) Then Return False
            If Math.Round(A.XOffset, 4) <> Math.Round(B.XOffset, 4) Then Return False
            If Math.Round(A.YOffset, 4) <> Math.Round(B.YOffset, 4) Then Return False
            Return True
        End Function

#End Region 'Shared Methods

#Region "Operators"
        Public Shared Operator =(A As dxoInstance, B As dxoInstance) As Boolean
            Return dxoInstance.InstanceCompare(A, B)
        End Operator
        Public Shared Operator <>(A As dxoInstance, B As dxoInstance) As Boolean
            Return Not dxoInstance.InstanceCompare(A, B)

        End Operator
#End Region 'Operators
    End Class 'dxoInstance
End Namespace
