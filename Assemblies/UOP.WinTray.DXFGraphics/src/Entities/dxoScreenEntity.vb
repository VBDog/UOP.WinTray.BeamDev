Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoScreenEntity
#Region "Members"
        Private _Struckture As TSCREENENTITY
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aScreenEnt As TSCREENENTITY)
            _Struckture = aScreenEnt
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property DxfHandle As ULong
            Get
                Return _Struckture.DxfHandle
            End Get
            Set(value As ULong)
                _Struckture.DxfHandle = value
            End Set
        End Property
        Public Property Alignment As dxxRectangularAlignments
            Get
                Return _Struckture.Alignment
            End Get
            Friend Set(value As dxxRectangularAlignments)
                _Struckture.Alignment = value
            End Set
        End Property
        Public Property Color As Integer
            Get
                Return _Struckture.Color
            End Get
            Friend Set(value As Integer)
                _Struckture.Color = value
            End Set
        End Property
        Public Property Dashed As Boolean
            Get
                Return _Struckture.Dashed
            End Get
            Friend Set(value As Boolean)
                _Struckture.Dashed = value
            End Set
        End Property
        Public Property Domain As dxxDrawingDomains
            Get
                Return _Struckture.Domain
            End Get
            Friend Set(value As dxxDrawingDomains)
                _Struckture.Domain = value
            End Set
        End Property
        Public Property Drawn As Boolean
            Get
                Return _Struckture.Drawn
            End Get
            Friend Set(value As Boolean)
                _Struckture.Drawn = value
            End Set
        End Property
        Public Property EntityGUID As String
            Get
                Return _Struckture.EntityGUID
            End Get
            Friend Set(value As String)
                _Struckture.EntityGUID = value
            End Set
        End Property
        Public Property EntityType As dxxGraphicTypes
            Get
                Return _Struckture.EntityType
            End Get
            Friend Set(value As dxxGraphicTypes)
                _Struckture.EntityType = value
            End Set
        End Property
        Public Property Filled As Boolean
            Get
                Return _Struckture.Filled
            End Get
            Friend Set(value As Boolean)
                _Struckture.Filled = value
            End Set
        End Property
        Public Property Handle As String
            Get
                Return _Struckture.Handle
            End Get
            Friend Set(value As String)
                _Struckture.Handle = value
            End Set
        End Property
        Public Property Height As Double
            Get
                Return _Struckture.Height
            End Get
            Friend Set(value As Double)
                _Struckture.Height = value
            End Set
        End Property
        Public Property PenWidth As Integer
            Get
                Return _Struckture.PenWidth
            End Get
            Friend Set(value As Integer)
                _Struckture.PenWidth = value
            End Set
        End Property
        Public Property Persist As Boolean
            Get
                Return _Struckture.Persist
            End Get
            Friend Set(value As Boolean)
                _Struckture.Persist = value
            End Set
        End Property
        Public Property Plane As dxfPlane
            Get
                Return New dxfPlane(_Struckture.Plane)
            End Get
            Friend Set(value As dxfPlane)
                _Struckture.Plane = value.Strukture
            End Set
        End Property
        Public Property ScreenFraction As Double
            Get
                Return _Struckture.ScreenFraction
            End Get
            Friend Set(value As Double)
                _Struckture.ScreenFraction = value
            End Set
        End Property
        Public Property Size As Integer
            Get
                Return _Struckture.Size
            End Get
            Friend Set(value As Integer)
                _Struckture.Size = value
            End Set
        End Property
        Public Property Tag As String
            Get
                Return _Struckture.Tag
            End Get
            Friend Set(value As String)
                _Struckture.Tag = value
            End Set
        End Property
        Public Property TextString As String
            Get
                Return _Struckture.TextString
            End Get
            Friend Set(value As String)
                _Struckture.TextString = value
            End Set
        End Property
        Public Property Type As dxxScreenEntityTypes
            Get
                Return _Struckture.Type
            End Get
            Friend Set(value As dxxScreenEntityTypes)
                _Struckture.Type = value
            End Set
        End Property
        Public ReadOnly Property EntityTypeName As String
            Get
                Return dxfEnums.Description(Type)
            End Get
        End Property
        Public Property Vectors As colDXFVectors
            Get
                Return New colDXFVectors(_Struckture.Vectors)
            End Get
            Friend Set(value As colDXFVectors)
                _Struckture.Vectors = New TVECTORS(value)
            End Set
        End Property
        Public Property WidthFactor As Double
            Get
                Return _Struckture.WidthFactor
            End Get
            Friend Set(value As Double)
                _Struckture.WidthFactor = value
            End Set
        End Property
#End Region 'Properties
        Public Overrides Function ToString() As String
            Return "dxoScreenEntity [" & dxfEnums.Description(Type) & "]"
        End Function
    End Class
End Namespace
