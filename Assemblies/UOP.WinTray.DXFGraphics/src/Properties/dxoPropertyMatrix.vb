Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoPropertyMatrix
#Region "Members"
        Private _Struc As TPROPERTYMATRIX
#End Region 'Members
#Region "Constructors"
#End Region 'Constructors
        Public Sub New()
            _Struc = New TPROPERTYMATRIX("")
        End Sub
        Friend Sub New(aGroupName As String, aExtentededData As TPROPERTYARRAY)
            _Struc = New TPROPERTYMATRIX("")
            GroupName = aGroupName
            ExtendedData = aExtentededData
        End Sub
        Friend Sub New(aGroupName As String, Optional aExtentededData As dxoPropertyArray = Nothing)
            _Struc = New TPROPERTYMATRIX("")
            GroupName = aGroupName
            ExtendedData = New TPROPERTYARRAY(aExtentededData)
        End Sub
        Friend Sub New(aMatrix As TPROPERTYMATRIX)
            _Struc = aMatrix
        End Sub

        Friend Sub New(aProperties As dxpProperties)
            _Struc = New TPROPERTYMATRIX("")
            If aProperties IsNot Nothing Then Add(New dxoPropertyArray(aProperties))
        End Sub
        Friend Sub New(aArray As TPROPERTYARRAY)
            _Struc = New TPROPERTYMATRIX("")
            Add(aArray)
        End Sub
#Region "Properties"
        Friend Property Block As TBLOCK
            Get
                Return _Struc.Block
            End Get
            Set(value As TBLOCK)
                _Struc.Block = value
            End Set
        End Property
        Public ReadOnly Property Count As Integer
            Get
                Return _Struc.Count
            End Get
        End Property
        Friend Property ExtendedData As TPROPERTYARRAY
            Get
                Return _Struc.ExtendedData
            End Get
            Set(value As TPROPERTYARRAY)
                _Struc.ExtendedData = value
            End Set
        End Property
        Public Property GroupName As String
            Get
                Return _Struc.GroupName
            End Get
            Set(value As String)
                _Struc.GroupName = value
            End Set
        End Property
        Public Overridable Property Name As String
            Get
                Return _Struc.Name
            End Get
            Set(value As String)
                _Struc.Name = value
            End Set
        End Property
        Friend Property Strukture As TPROPERTYMATRIX
            Get
                Return _Struc
            End Get
            Set(value As TPROPERTYMATRIX)
                _Struc = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Function Item(aIndex As Integer) As TPROPERTYARRAY
            Return _Struc.Item(aIndex)
        End Function
        Friend Sub SetItem(aIndex As Integer, value As TPROPERTYARRAY)
            _Struc.SetItem(aIndex, value)
        End Sub
        Friend Sub Add(aPropArray As TPROPERTYARRAY, Optional aName As String = Nothing)
            _Struc.Add(aPropArray, aName)
        End Sub
        Friend Sub Add(aPropArray As dxoPropertyArray, Optional aName As String = Nothing)
            _Struc.Add(New TPROPERTYARRAY(aPropArray), aName)
        End Sub
        Friend Sub AddProperties(aProps As TPROPERTIES, Optional aArrayName As String = "", Optional aPropsName As String = "")
            _Struc.Add(aProps, aArrayName, aPropsName)
        End Sub
        Friend Sub Append(aMatrix As dxoPropertyMatrix, Optional bFlatten As Boolean = False)
            If aMatrix IsNot Nothing Then _Struc.Append(aMatrix.Strukture, bFlatten)
        End Sub
#End Region 'Methods
    End Class 'dxoPropertyMatrix
End Namespace
