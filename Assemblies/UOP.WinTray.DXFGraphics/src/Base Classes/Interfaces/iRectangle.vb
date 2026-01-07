Namespace UOP.DXFGraphics

    Public Interface iRectangle

        Property Plane As dxfPlane

        Property Width As Double

        Property Height As Double

        Property Center As iVector

        ReadOnly Property Left As Double
        ReadOnly Property Right As Double
        ReadOnly Property Top As Double

        ReadOnly Property Bottom As Double

    End Interface

End Namespace

