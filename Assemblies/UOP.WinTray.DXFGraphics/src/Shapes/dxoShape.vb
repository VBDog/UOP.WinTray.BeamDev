Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoShape
        Implements ICloneable
#Region "Members"
        Friend Commands As TSHAPECOMMANDS
        Friend PointStack As TPOINTS
        Friend Path As TPOINTS
        Friend PathBytes() As Byte
        Friend PathCommands As TVALUES
#End Region'Members
#Region "Constructors"
        Public Sub New()
            'init -------------------------------------
            Ascent = 0
            ByteCount = 0
            FileName = ""
            GroupName = ""
            HasSubShapes = False
            Index = 0
            Name = ""
            Key = ""
            PathString = ""
            PenDown = False
            ShapeNumber = 0
            ShapeType = ""
            Commands = New TSHAPECOMMANDS(0)
            PointStack = New TPOINTS(0)
            PathCommands = New TVALUES(0)
            ReDim PathBytes(0)
            'init -------------------------------------
        End Sub
        Friend Sub New(aStructure As TSHAPE)
            'init -------------------------------------
            Ascent = aStructure.Ascent
            ByteCount = aStructure.ByteCount
            Descent = aStructure.Descent
            FileName = aStructure.FileName
            GroupName = aStructure.GroupName
            HasSubShapes = aStructure.HasSubShapes
            Index = aStructure.Index
            Name = aStructure.Name
            Key = aStructure.Key
            PathString = aStructure.PathString
            PenDown = aStructure.PenDown
            ShapeNumber = aStructure.ShapeNumber
            ShapeType = aStructure.ShapeType
            Commands = New TSHAPECOMMANDS(aStructure.Commands)
            PointStack = New TPOINTS(aStructure.PointStack)
            PathCommands = New TVALUES(aStructure.PathCommands)
            Path = New TPOINTS(aStructure.Path)
            ReDim PathBytes(0)
            'init -------------------------------------

            If aStructure.PathBytes IsNot Nothing Then
                PathBytes = aStructure.PathBytes.Clone()

            End If

        End Sub

        Friend Sub New(aShape As dxoShape)
            'init -------------------------------------
            Ascent = 0
            ByteCount = 0
            FileName = ""
            GroupName = ""
            HasSubShapes = False
            Index = 0
            Name = ""
            Key = ""
            PathString = ""
            PenDown = False
            ShapeNumber = 0
            ShapeType = ""
            Commands = New TSHAPECOMMANDS(0)
            PointStack = New TPOINTS(0)
            PathCommands = New TVALUES(0)
            ReDim PathBytes(0)
            'init -------------------------------------

            If aShape Is Nothing Then Return

            Ascent = aShape.Ascent
            ByteCount = aShape.ByteCount
            Descent = aShape.Descent
            FileName = aShape.FileName
            GroupName = aShape.GroupName
            HasSubShapes = aShape.HasSubShapes
            Index = aShape.Index
            Name = aShape.Name
            Key = aShape.Key
            PathString = aShape.PathString
            PenDown = aShape.PenDown
            ShapeNumber = aShape.ShapeNumber
            ShapeType = aShape.ShapeType
            Commands = New TSHAPECOMMANDS(aShape.Commands)
            PointStack = New TPOINTS(aShape.PointStack)
            PathBytes = aShape.PathBytes.Clone()
            PathCommands = New TVALUES(aShape.PathCommands)
            Path = New TPOINTS(aShape.Path)
        End Sub


#End Region 'Constructors
#Region "Properties"

        Private _Key As String

        Public Property Key As String
            Get
                Return _Key
            End Get
            Friend Set(value As String)
                _Key = value
            End Set
        End Property

        Private _Index As Integer
        Public Property Index As Integer
            Get
                Return _Index
            End Get
            Friend Set(value As Integer)
                _Index = value
            End Set
        End Property

        Private _PathString As String
        Public Property PathString As String
            Get
                Return _PathString
            End Get
            Friend Set(value As String)
                _PathString = value
            End Set
        End Property

        Private _FileName As String
        Public Property FileName As String
            Get
                Return _FileName
            End Get
            Friend Set(value As String)
                _FileName = value
            End Set
        End Property

        Private _GroupName As String
        Public Property GroupName As String
            Get
                Return _GroupName
            End Get
            Friend Set(value As String)
                _GroupName = value
            End Set
        End Property

        Private _ShapeType As String
        Public Property ShapeType As String
            Get
                Return _ShapeType
            End Get
            Friend Set(value As String)
                _ShapeType = value
            End Set
        End Property

        Private _Name As String
        Public Property Name As String
            Get
                Return _Name
            End Get
            Friend Set(value As String)
                _Name = value
            End Set
        End Property

        Private _ShapeNumber As Integer
        Public Property ShapeNumber As Integer
            Get
                Return _ShapeNumber
            End Get
            Friend Set(value As Integer)
                _ShapeNumber = value
            End Set
        End Property

        Private _Ascent As Byte
        Public Property Ascent As Byte
            Get
                Return _Ascent
            End Get
            Friend Set(value As Byte)
                _Ascent = value
            End Set
        End Property

        Private _Descent As Byte
        Public Property Descent As Byte
            Get
                Return _Descent
            End Get
            Friend Set(value As Byte)
                _Descent = value
            End Set
        End Property

        Private _ByteCount As Integer
        Public Property ByteCount As Integer
            Get
                Return _ByteCount
            End Get
            Friend Set(value As Integer)
                _ByteCount = value
            End Set
        End Property

        Private _HasSubShapes As Boolean
        Public Property HasSubShapes As Boolean
            Get
                Return _HasSubShapes
            End Get
            Friend Set(value As Boolean)
                _HasSubShapes = value
            End Set
        End Property

        Private _PenDown As Boolean
        Public Property PenDown As Boolean
            Get
                Return _PenDown
            End Get
            Friend Set(value As Boolean)
                _PenDown = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"{Index }-{ ShapeNumber }[{ Name }]={ PathString}"
        End Function



        Public Function Clone() As dxoShape
            Return New dxoShape(Me)

        End Function

        Friend Function Structure_Get()
            Return New TSHAPE With
            {
            .Ascent = Ascent,
            .ByteCount = ByteCount,
            .Descent = Descent,
            .FileName = FileName,
            .GroupName = GroupName,
            .HasSubShapes = HasSubShapes,
            .Index = Index,
            .Name = Name,
            .Key = Key,
            .PathString = PathString,
            .PenDown = PenDown,
            .ShapeNumber = ShapeNumber,
            .ShapeType = ShapeType,
            .Commands = Commands.Clone(),
            .PointStack = PointStack.Clone(),
            .PathBytes = PathBytes.Clone(),
            .PathCommands = PathCommands.Clone()
            }
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#End Region 'Properties
    End Class
End Namespace
