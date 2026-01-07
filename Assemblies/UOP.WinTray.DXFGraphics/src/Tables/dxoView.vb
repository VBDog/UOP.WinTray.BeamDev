Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoView
        Inherits dxfTableEntry
        Implements ICloneable
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxReferenceTypes.VIEW)
        End Sub
        Friend Sub New(aEntry As TTABLEENTRY)
            MyBase.New(dxxReferenceTypes.VIEW, aEntry.Name, aGUID:=aEntry.GUID)
            If aEntry.EntryType = dxxReferenceTypes.VPORT Then Properties.CopyVals(aEntry)
        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxReferenceTypes.VIEW, aName)
            OwnerPtr = Nothing
        End Sub
        Public Sub New(aEntry As dxoView)
            MyBase.New(aEntry)
        End Sub
#End Region 'Constructors
#Region "dxfHandleOwner"
        Friend Overrides Property Suppressed As Boolean
            Get
                Return False
            End Get
            Set(value As Boolean)
                value = False
            End Set
        End Property
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Throw New NotImplementedException
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Throw New NotImplementedException
        End Function
        Public Overrides ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.TableEntry
            End Get
        End Property
#End Region 'dxfHandleOwner
#Region "Properties"
        Public Property Direction As dxfDirection
            Get
                Return Properties.Direction(11, aDefault:=New dxfDirection(0, 0, 1))
            End Get
            Set(value As dxfDirection)
                If value Is Nothing Then value = New dxfDirection(0, 0, 1)
                Properties.SetDirection(11, value)

            End Set
        End Property
        Public Property TargetPoint As dxfVector
            Get
                Return Properties.Vector(12, aDefault:=dxfVector.Zero)
            End Get
            Set(value As dxfVector)
                If value Is Nothing Then value = dxfVector.Zero
                Properties.SetVector(12, value, True)

            End Set
        End Property
        Public Property ViewCenter As dxfVector
            Get
                Return Properties.Vector(10, aDefault:=dxfVector.Zero)
            End Get
            Set(value As dxfVector)
                If value Is Nothing Then value = dxfVector.Zero
                Properties.SetVector(10, value, True)

            End Set
        End Property
        Public Property UCSOrigin As dxfVector
            Get
                Return Properties.Vector(110, aDefault:=dxfVector.Zero)
            End Get
            Set(value As dxfVector)
                If value Is Nothing Then value = dxfVector.Zero
                Properties.SetVector(12, value, True)

            End Set
        End Property
        Public Property UCSXAxisDirection As dxfDirection
            Get
                Return Properties.Direction(111, aDefault:=New dxfDirection(1, 0, 0))
            End Get
            Set(value As dxfDirection)
                If value Is Nothing Then value = New dxfDirection(1, 0, 0)
                Properties.SetDirection(111, value)

            End Set
        End Property
        Public Property UCSYAxisDirection As dxfDirection
            Get
                Return Properties.Direction(112, aDefault:=New dxfDirection(0, 1, 0))
            End Get
            Set(value As dxfDirection)
                If value Is Nothing Then value = New dxfDirection(0, 1, 0)
                Properties.SetDirection(112, value)

            End Set
        End Property

        Public Property UCSElevation As Double
            Get
                Return Properties.ValueD(146)
            End Get
            Set(value As Double)
                Properties.SetVal(146, value)
            End Set
        End Property

        Public Property ViewMode As dxxViewModes
            Get

                Return DirectCast(Properties.ValueI(71), dxxViewModes)
            End Get
            Set(value As dxxViewModes)
                Properties.SetVal(71, value)
            End Set
        End Property

        Public Property UCSOrthographicType As dxxOrthoGraphicTypes
            Get
                Return DirectCast(Properties.ValueI(79), dxxOrthoGraphicTypes)
            End Get
            Set(value As dxxOrthoGraphicTypes)
                Properties.SetVal(79, value)
            End Set
        End Property

        Public Property UCSRecordHandle As String
            Get
                Return Properties.ValueS(345)
            End Get
            Friend Set(value As String)
                Properties.SetVal(345, value)
            End Set
        End Property

        Public Property UCS As dxfPlane
            Get
                Return New dxfPlane(UCSOrigin, aXDir:=UCSXAxisDirection, aYDir:=UCSYAxisDirection)
            End Get
            Set(value As dxfPlane)
                If value Is Nothing Then value = New dxfPlane
                Properties.SetVector(110, value.Origin)
                Properties.SetDirection(111, value.XDirection)
                Properties.SetDirection(112, value.YDirection)
            End Set
        End Property

        Public Property UCSBaseRecordHandle As String
            Get
                Return Properties.ValueS(346)
            End Get
            Friend Set(value As String)
                Properties.SetVal(346, value)
            End Set
        End Property
        Public Property ViewHeight As Double
            Get
                Return Properties.ValueD(40)
            End Get
            Set(value As Double)
                Properties.SetVal(40, value)
            End Set
        End Property
        Public Property ViewWidth As Double
            Get
                Return Properties.ValueD(41)
            End Get
            Set(value As Double)
                Properties.SetVal(41, value)
            End Set
        End Property
        Public Property LensLength As Double
            Get
                Return Properties.ValueD(42)
            End Get
            Set(value As Double)
                Properties.SetVal(42, value)
            End Set
        End Property
        Public Property TwistAngle As Double
            Get
                Return Properties.ValueD(50)
            End Get
            Set(value As Double)
                Properties.SetVal(50, value)
            End Set
        End Property
        Public Property FrontClippingPlane As Double
            Get
                Return Properties.ValueD(43)
            End Get
            Set(value As Double)
                Properties.SetVal(43, value)
            End Set

        End Property
        Public Property BackClippingPlane As Double
            Get
                Return Properties.ValueD(44)
            End Get
            Set(value As Double)
                Properties.SetVal(44, value)
            End Set
        End Property

        Public Property RenderMode As dxxRenderModes
            Get
                Return DirectCast(Properties.ValueI(281), dxxRenderModes)
            End Get
            Set(value As dxxRenderModes)
                Properties.SetVal(281, value)
            End Set
        End Property

        Private _IsPaperSpace As Boolean
        Public Property IsPaperSpace As Boolean
            Get
                Return _IsPaperSpace
            End Get
            Set(value As Boolean)
                _IsPaperSpace = value
                UpdateProperty70()
            End Set
        End Property

        Private _XRefDependant As Boolean
        Public Property XRefDependant As Boolean
            Get
                Return _XRefDependant
            End Get
            Set(value As Boolean)
                _XRefDependant = value
                UpdateProperty70()
            End Set
        End Property

        Private _XRefResolved As Boolean
        Public Property XRefResolved As Boolean
            Get
                Return _XRefResolved And XRefDependant
            End Get
            Set(value As Boolean)
                _XRefResolved = value
                UpdateProperty70()
            End Set
        End Property

        Public Property HasUCS As Boolean
            Get
                Return Properties.ValueB(72)
            End Get
            Set(value As Boolean)
                Properties.SetVal(72, value)
            End Set
        End Property
        Public Property CameraIsPlottable As Boolean
            Get
                Return Properties.ValueB(73)
            End Get
            Set(value As Boolean)
                Properties.SetVal(72, value)
            End Set
        End Property

        Public Property BackgroundHandle As String
            Get
                Return Properties.ValueS(332)
            End Get
            Friend Set(value As String)
                Properties.SetVal(332, value)
            End Set
        End Property
        Public Property LiveSectionHandle As String
            Get
                Return Properties.ValueS(334)
            End Get
            Friend Set(value As String)
                Properties.SetVal(334, value)
            End Set
        End Property
        Public Property VisualStyleHandle As String
            Get
                Return Properties.ValueS(348)
            End Get
            Friend Set(value As String)
                Properties.SetVal(348, value)
            End Set
        End Property

        Public ReadOnly Property ViewFlag As Integer
            Get
                UpdateProperty70()
                Return Properties.ValueI(70)
            End Get
        End Property

#End Region 'Properties

#Region "Methods"
        Public Overrides Sub UpdateProperties(Optional aImage As dxfImage = Nothing)
            UpdateProperty70()
        End Sub
        Public Sub UpdateProperty70()

            Dim ival As Integer = 0
            If IsPaperSpace Then ival += 1
            If XRefDependant Then ival += 16
            If XRefResolved Then ival += 32
            Properties.SetVal(70, ival, bSuppressEvents:=True)
        End Sub

        Public Shadows Function Clone() As dxoView
            Return New dxoView(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function


        Friend Overrides Function PropValueSetByName(aName As String, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Or String.IsNullOrWhiteSpace(aName) Then Return False
            'reviewv changes for validity before proceding
            Select Case aName.ToUpper()
                Case "a property name here"

            End Select
            Return MyBase.PropValueSetByName(aName, aValue, aOccur, bSuppressEvnts)

        End Function

#End Region 'Methods

    End Class
End Namespace
