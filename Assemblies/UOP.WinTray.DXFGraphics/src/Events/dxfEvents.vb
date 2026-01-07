
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Friend NotInheritable Class dxfEvents

        Private Shared _ImageTotal As Long
        Private Shared _Images As Long
        Private Shared _BlockGUID As Double
        Private Shared _BlocksGUID As Double
        Private Shared garColIndexes(0 To 2) As Double
        Private Shared _StringIndexes As Dictionary(Of String, Double)
        Private Shared _GraphicTypeIndexes As Dictionary(Of dxxGraphicTypes, Double)
        Private Shared _RefTypeIndexes As Dictionary(Of dxxReferenceTypes, Double)
        Private Shared _TableTypeIndexes As Dictionary(Of dxxReferenceTypes, Double)
        Private Shared _ObjectTypeIndexes As Dictionary(Of dxxObjectTypes, Double)

        Friend Shared Function NextImageGUID() As String
            _ImageTotal += 1
            _Images += 1

            Return $"IMAGE({ _Images}.0)"
        End Function
        Friend Shared Function NextImageGUID(aOldGUID As String) As String
            If String.IsNullOrWhiteSpace(aOldGUID) Then Return NextImageGUID()
            aOldGUID = aOldGUID.Trim
            Dim svals() As String = aOldGUID.Split(".")
            If svals.Count < 2 Then Return NextImageGUID()
            If Not TVALUES.IsNumber(svals(0)) Or Not TVALUES.IsNumber(svals(1)) Then Return NextImageGUID()
            Dim ivals(0 To 1) As Integer
            ivals(0) = Math.Abs(TVALUES.To_INT(svals(0)))
            ivals(1) = Math.Abs(TVALUES.To_INT(svals(1))) + 1
            Return $"IMAGE({ ivals(0)}.{ivals(1)})"
        End Function
        Friend Shared Function NextEntityGUID(aGraphicType As dxxGraphicTypes, Optional aExistingGUID As String = Nothing, Optional aGUIDPrefix As String = Nothing) As String
            If Not String.IsNullOrWhiteSpace(aExistingGUID) Then Return aExistingGUID.Trim()
            If String.IsNullOrWhiteSpace(aGUIDPrefix) Then aGUIDPrefix = String.Empty Else aGUIDPrefix = aGUIDPrefix.Trim()
            If _GraphicTypeIndexes Is Nothing Then _GraphicTypeIndexes = New Dictionary(Of dxxGraphicTypes, Double)
            Dim aGraphicTypeName As String = dxfEnums.Description(aGraphicType)
            If Not _GraphicTypeIndexes.ContainsKey(aGraphicType) Then
                _GraphicTypeIndexes.Add(aGraphicType, 0.00)
            End If
            Dim baseval As Double = _GraphicTypeIndexes(aGraphicType)
            baseval += 0.01
            _GraphicTypeIndexes(aGraphicType) = baseval
            Dim _rVal As String = $"{aGUIDPrefix}{ aGraphicTypeName.ToUpper }({ baseval:0.00})"
            Return _rVal
        End Function
        Friend Shared Function NextObjectGUID(aObjectType As dxxObjectTypes, Optional aExistingGUID As String = Nothing) As String
            If Not String.IsNullOrWhiteSpace(aExistingGUID) Then Return aExistingGUID.Trim()
            If _ObjectTypeIndexes Is Nothing Then _ObjectTypeIndexes = New Dictionary(Of dxxObjectTypes, Double)
            Dim aObjectTypeName As String = dxfEnums.Description(aObjectType)
            If Not _ObjectTypeIndexes.ContainsKey(aObjectType) Then
                _ObjectTypeIndexes.Add(aObjectType, 0.00)
            End If
            Dim baseval As Double = _ObjectTypeIndexes(aObjectType)
            baseval += 0.01
            _ObjectTypeIndexes(aObjectType) = baseval
            Dim _rVal As String = $"{aObjectTypeName.ToUpper }({ baseval:0.00})"
            Return _rVal
        End Function
        Friend Shared Function NextGUID(aObjectTypeName As String, Optional aExistingGUID As String = Nothing) As String
            If Not String.IsNullOrWhiteSpace(aExistingGUID) Then Return aExistingGUID.Trim()
            If String.IsNullOrWhiteSpace(aObjectTypeName) Then Return String.Empty
            If _StringIndexes Is Nothing Then _StringIndexes = New Dictionary(Of String, Double)
            If Not _StringIndexes.ContainsKey(aObjectTypeName) Then
                _StringIndexes.Add(aObjectTypeName, 0.00)
            End If
            Dim baseval As Double = _StringIndexes(aObjectTypeName)
            baseval += 0.01
            _StringIndexes(aObjectTypeName) = baseval
            Dim _rVal As String = $"{aObjectTypeName.ToUpper }({ baseval:0.00})"
            Return _rVal
        End Function
        Friend Shared Function NextVectorsGUID(Optional aExistingGUID As String = Nothing) As String
            Return NextGUID("VECTORS", aExistingGUID)
        End Function
        Friend Shared Function NextEntitiesGUID(Optional aExistingGUID As String = Nothing) As String
            Return NextGUID("ENTITIES", aExistingGUID)
        End Function
        Friend Shared Function NextEntryGUID(aRefType As dxxReferenceTypes, Optional aExistingGUID As String = Nothing) As String
            If Not String.IsNullOrWhiteSpace(aExistingGUID) Then Return aExistingGUID
            If _RefTypeIndexes Is Nothing Then _RefTypeIndexes = New Dictionary(Of dxxReferenceTypes, Double)
            Dim RefTypeName As String = dxfEnums.Description(aRefType).ToUpper
            If Not _RefTypeIndexes.ContainsKey(aRefType) Then
                _RefTypeIndexes.Add(aRefType, 0.00)
            End If
            Dim baseval As Double = _RefTypeIndexes(aRefType)
            baseval += 0.01
            _RefTypeIndexes(aRefType) = baseval
            Dim _rVal As String = RefTypeName & "(" & Format(baseval, "0.00") & ")"
            Return _rVal
        End Function

        Friend Shared Function NextSettingGUID(aRefType As dxxSettingTypes, Optional aExistingGUID As String = Nothing) As String
            If Not String.IsNullOrWhiteSpace(aExistingGUID) Then Return aExistingGUID
            If _RefTypeIndexes Is Nothing Then _RefTypeIndexes = New Dictionary(Of dxxReferenceTypes, Double)
            Dim RefTypeName As String = dxfEnums.Description(aRefType).ToUpper
            If Not _RefTypeIndexes.ContainsKey(aRefType) Then
                _RefTypeIndexes.Add(aRefType, 0.00)
            End If
            Dim baseval As Double = _RefTypeIndexes(aRefType)
            baseval += 0.01
            _RefTypeIndexes(aRefType) = baseval
            Dim _rVal As String = RefTypeName & "(" & Format(baseval, "0.00") & ")"
            Return _rVal
        End Function
        Friend Shared Function NextTableGUID(aRefType As dxxReferenceTypes, Optional aExistingGUID As String = Nothing) As String
            If Not String.IsNullOrWhiteSpace(aExistingGUID) Then Return aExistingGUID
            If _TableTypeIndexes Is Nothing Then _TableTypeIndexes = New Dictionary(Of dxxReferenceTypes, Double)
            Dim RefTypeName As String = dxfEnums.Description(aRefType).ToUpper & "_TABLE"
            If Not _TableTypeIndexes.ContainsKey(aRefType) Then
                _TableTypeIndexes.Add(aRefType, 0.00)
            End If
            Dim baseval As Double = _TableTypeIndexes(aRefType)
            baseval += 0.01
            _TableTypeIndexes(aRefType) = baseval
            Dim _rVal As String = $"{RefTypeName }({baseval:0.00})"
            Return _rVal
        End Function
        Friend Shared Function NextBlockGUID() As String
            _BlockGUID += 0.01
            Return $"BLOCK({_BlockGUID:0.00})"
        End Function
        Friend Shared Function NextBlocksGUID() As String
            _BlocksGUID += 0.01
            Return $"BLOCKS({_BlocksGUID:0.00})"
        End Function
        Friend Shared Function GetImage(aImageGUID As String) As dxfImage
            If String.IsNullOrWhiteSpace(aImageGUID) Then Return Nothing

            Dim msg As New Message_ImageRequest(aImageGUID)
            dxfGlobals.Aggregator.Publish(msg)
            Return msg.Image
        End Function
        Friend Shared Function GetImagePtr(aImageGUID As String) As WeakReference(Of dxfImage)
            If String.IsNullOrWhiteSpace(aImageGUID) Then Return Nothing
            Dim _rVal As dxfImage = GetImage(aImageGUID)
            If _rVal Is Nothing OrElse _rVal.Disposed Then Return Nothing
            Return New WeakReference(Of dxfImage)(_rVal)
        End Function
        Friend Shared Function GetImageUCS(aImageGUID As String) As dxfPlane
            Dim rImage As dxfImage = Nothing
            Return GetImageUCS(aImageGUID, rImage)
        End Function
        Friend Shared Function GetImageUCS(aImageGUID As String, ByRef rImage As dxfImage) As dxfPlane
            Return New dxfPlane With {.Strukture = GetImageUCSV(aImageGUID, rImage)}
        End Function
        Friend Shared Function GetImageUCSV(aImageGUID As String) As TPLANE
            Dim rImage As dxfImage = Nothing
            Return GetImageUCSV(aImageGUID, rImage)
        End Function
        Friend Shared Function GetImageUCSV(aImageGUID As String, ByRef rImage As dxfImage) As TPLANE

            rImage = GetImage(aImageGUID)
            Dim _rVal As TPLANE = IIf(rImage IsNot Nothing, rImage.obj_UCS, New TPLANE("World", dxxDeviceUnits.Inches))
            Return _rVal
        End Function
    End Class
End Namespace
