Imports UOP.DXFGraphics
Namespace UOP.DXFGraphics
    Public Class Message_ImageRequest
        Public Sub New(aGUID As String)
            ImageGUID = aGUID
            _Image = Nothing
        End Sub
        Private _ImageGUID As String
        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                _ImageGUID = value
            End Set
        End Property
        Private _Image As dxfImage
        Public Property Image As dxfImage
            Get
                Return _Image
            End Get
            Set(value As dxfImage)
                _Image = value
            End Set
        End Property
    End Class
End Namespace
