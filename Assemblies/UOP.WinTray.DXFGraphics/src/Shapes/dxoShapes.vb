Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoShapes
#Region "Members"
        Private _Struc As TSHAPES
#End Region'Members
#Region "Constructors"
        Public Sub New()
            _Struc = TSHAPES.Null
        End Sub
        Friend Sub New(aStructure As TSHAPES)
            _Struc = aStructure
        End Sub
        Public Sub New(aFileName As String)
            Dim sErr As String = String.Empty
            Dim aShapes As TSHAPES = TSHAPES.ReadFromFile(aFileName, sErr, True)
            If sErr <> "" Then Throw New Exception(sErr) Else _Struc = aShapes
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                Return _Struc.Count
            End Get
        End Property
        Public ReadOnly Property FileName As String
            Get
                Return _Struc.FileName
            End Get
        End Property
        Public ReadOnly Property IsFont As Boolean
            Get
                Return _Struc.IsFont
            End Get
        End Property
        Public ReadOnly Property Name As String
            Get
                Return _Struc.Name
            End Get
        End Property
        Public ReadOnly Property Header As String
            Get
                Return _Struc.Header
            End Get
        End Property
        Public ReadOnly Property Comment As String
            Get
                Return _Struc.Comment
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As dxoShape
            If aIndex > 0 And aIndex <= Count Then
                Return New dxoShape(_Struc.Item(aIndex))
            Else
                Return Nothing
            End If
        End Function
        Public Function Item(aKeyOrName As String) As dxoShape
            Dim aShp As TSHAPE = _Struc.Member(aKeyOrName)
            If aShp.Index <> -1 Then
                Return New dxoShape(aShp)
            Else
                Return Nothing
            End If
        End Function
#End Region 'Properties
    End Class
End Namespace
