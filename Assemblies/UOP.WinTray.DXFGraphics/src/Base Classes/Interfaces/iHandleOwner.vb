Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Friend Interface iHandleOwner
        Property ReactorGUID As String

        Property BlockGUID As String
        Property CollectionGUID As String
        Property BlockCollectionGUID As String
        Property GUID As String
        Property Handle As String
        Property ImageGUID As String
        Property Index As Integer
        Property OwnerGUID As String
        Property SourceGUID As String
        Property Domain As dxxDrawingDomains
        Property Identifier As String
        Property ObjectType As dxxFileObjectTypes
        Property OwnerType As dxxFileObjectTypes
        Property Name As String
        Property Suppressed As Boolean

        Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As dxfPlane, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As dxoPropertyArray
        Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix


    End Interface
End Namespace
