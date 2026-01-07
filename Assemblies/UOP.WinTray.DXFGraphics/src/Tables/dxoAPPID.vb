Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoAPPID
        Inherits dxfTableEntry
        Implements ICloneable
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxReferenceTypes.APPID)
        End Sub
        Friend Sub New(aEntry As TTABLEENTRY)
            MyBase.New(dxxReferenceTypes.APPID, aEntry.Name, aGUID:=aEntry.GUID)
            If aEntry.EntryType = dxxReferenceTypes.APPID Then MyBase.Initialize(aEntry)
        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxReferenceTypes.APPID, aName)
        End Sub
        Public Sub New(aEntry As dxoAPPID)
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
#End Region 'Properties

#Region "Methods"

        Public Shadows Function Clone() As dxoAPPID
            Return New dxoAPPID(Me)
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
