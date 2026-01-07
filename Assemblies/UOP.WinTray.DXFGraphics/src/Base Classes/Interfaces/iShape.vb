Namespace UOP.DXFGraphics
    Public Interface iShape
        ReadOnly Property Vertices As IEnumerable(Of iVector)

        ReadOnly Property Plane As dxfPlane

        ReadOnly Property Closed As Boolean


    End Interface

End Namespace
