Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoUCS
        Inherits dxfTableEntry
        Implements ICloneable
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxReferenceTypes.UCS)
        End Sub
        Friend Sub New(aEntry As TTABLEENTRY)
            MyBase.New(dxxReferenceTypes.UCS, aEntry.Name, aGUID:=aEntry.GUID)
            If aEntry.EntryType = dxxReferenceTypes.UCS Then Properties.CopyVals(aEntry)
        End Sub
        Friend Sub New(aPlane As TPLANE, Optional aElevation As Double = 0, Optional bIsGlobal As Boolean = False)
            MyBase.New(dxxReferenceTypes.UCS, aName:=aPlane.Name)

            IsGlobal = bIsGlobal
            Properties.SetVector(10, aPlane.Origin)
            Properties.SetVector(11, aPlane.XDirection)
            Properties.SetVector(12, aPlane.YDirection)
            If aElevation <> 0 Then Properties.SetVal(146, aElevation)

        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxReferenceTypes.UCS, aName)
        End Sub
        Public Sub New(aEntry As dxoUCS)
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
        Private _Plane As dxfPlane
        Public Property Plane As dxfPlane
            Get
                If _Plane Is Nothing Then _Plane = New dxfPlane()
                Return _Plane
            End Get
            Set(value As dxfPlane)
                _Plane = value
            End Set
        End Property

        Private _RelativeOrigin As dxfVector
        Public Property RelativeOrigin As dxfVector
            Get
                Return _RelativeOrigin
            End Get
            Set(value As dxfVector)
                _RelativeOrigin = value
            End Set
        End Property

        Public Property Elevation As Double
            Get
                Return Properties.ValueD(146)
            End Get
            Set(value As Double)
                Properties.SetVal(46, value)
            End Set
        End Property

        Public Property OrthographicType As dxxOrthoGraphicTypes
            Get
                Return DirectCast(Properties.ValueI(71), dxxOrthoGraphicTypes)
            End Get
            Set(value As dxxOrthoGraphicTypes)
                Properties.SetVal(71, value)
            End Set
        End Property
#End Region 'Properties

#Region "Methods"

        Public Overrides Sub UpdateProperties(Optional aImage As dxfImage = Nothing)
            Properties.SetVector(10, Plane.Origin)
            Properties.SetDirection(11, Plane.XDirection)
            Properties.SetDirection(12, Plane.YDirection)
            Properties.SetVector(13, _RelativeOrigin)
        End Sub


        Public Shadows Function Clone() As dxoUCS
            Return New dxoUCS(Me)
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
