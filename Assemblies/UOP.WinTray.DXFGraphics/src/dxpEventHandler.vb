
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Friend Class dxpEventHandler
#Region "Members"
#End Region 'Members
#Region "Events"
        Friend Event VectorsRequest(aGUID As String, ByRef rVectors As colDXFVectors)
        Friend Event BlockRequest(aGUID As String, ByRef rBlock As dxfBlock)
        Friend Event ImageRequest(aGUID As String, ByRef rImage As dxfImage)
        Friend Event EntityRequest(aGUID As String, ByRef rEntity As dxfEntity)
        Friend Event EntititiesRequest(aGUID As String, ByRef rEntities As colDXFEntities)
#End Region 'Events
#Region "Constructors"
#End Region 'Constructors
#Region "Methods"
        Friend Function DeleteGraphic(aHandles As THANDLES) As Boolean
            '^removes the entity from an image's entities collection and bitmap
            If aHandles.ImageGUID = "" Or aHandles.Handle = "" Then Return False
            Dim aImage As dxfImage = GetImage(aHandles.ImageGUID)
            If aImage Is Nothing Then Return False
            Return aImage.DeleteGraphicEntity(aHandles)
        End Function
        Friend Function DirtyBlock(aEntity As dxfHandleOwner, Optional arBlockGUID As String = "") As dxfBlock
            If aEntity IsNot Nothing Then arBlockGUID = aEntity.BlockGUID.Trim
            If String.IsNullOrWhiteSpace(arBlockGUID) Then Return Nothing
            Dim rBlock As dxfBlock = GetBlock(arBlockGUID)
            If rBlock Is Nothing Then Return Nothing
            rBlock.IsDirty = True
            Return rBlock
        End Function
        Friend Function DirtyOwner(aEntity As dxfEntity, ByRef ioOwnerGUID As String, Optional aDependentGUID As String = "") As dxfEntity
            Dim rOwner As dxfEntity = RetrieveOwner(aEntity, ioOwnerGUID)
            If rOwner IsNot Nothing Then rOwner.IsDirty = True
            Return rOwner
        End Function
        Friend Function GetBlock(aBlockGUID As String) As dxfBlock
            If String.IsNullOrWhiteSpace(aBlockGUID) Then Return Nothing
            Dim _rVal As Object = Nothing
            RaiseEvent BlockRequest(aBlockGUID, _rVal)
            Return _rVal
        End Function
        Friend Function GetHeaderVariable(aGUID As String, aVarName As String, Optional aImage As dxfImage = Nothing, Optional aDefault As Object = Nothing) As Object
            Dim _rVal As Object = Nothing
            aVarName = Trim(aVarName)
            If aImage Is Nothing Then aImage = GetImage(aGUID)
            If aImage IsNot Nothing And aVarName <> "" Then
                Dim bEx As Boolean
                Dim b1 As Boolean
                _rVal = aImage.Header.GetPropertyValue(aVarName, bEx, b1)
                If Not bEx Then _rVal = aDefault
            Else
                If aDefault IsNot Nothing Then _rVal = aDefault
            End If
            Return _rVal
        End Function
        Friend Function GetImage(aImageGUID As String) As dxfImage
            If String.IsNullOrWhiteSpace(aImageGUID) Then Return Nothing
            Dim _rVal As dxfImage = dxfEvents.GetImage(aImageGUID)
            'RaiseEvent ImageRequest(aImageGUID, _rVal)
            Return _rVal
        End Function
        Friend Function GetImagePtr(aImageGUID As String) As WeakReference
            If String.IsNullOrWhiteSpace(aImageGUID) Then Return Nothing
            Dim _rVal As dxfImage = Nothing
            RaiseEvent ImageRequest(aImageGUID, _rVal)
            If _rVal Is Nothing OrElse _rVal.Disposed Then Return Nothing
            Return New WeakReference(_rVal)
        End Function
        Friend Sub NotifyDependents(aEntity As dxfEntity, aEvent As dxfEntityEvent)
            If aEntity Is Nothing Or aEvent Is Nothing Then Return
            If Not String.IsNullOrWhiteSpace(aEntity.OwnerGUID) Then
                Dim owner As dxfHandleOwner = aEntity.MyOwner
                If owner IsNot Nothing Then
                    owner.ProcessEvent(aEvent)
                End If
            End If
            If Not String.IsNullOrWhiteSpace(aEntity.BlockGUID) Then
                Dim block As dxfBlock = aEntity.MyBlock
                If block IsNot Nothing Then
                    block.ProcessEvent(aEvent)
                End If
            End If
            If Not String.IsNullOrWhiteSpace(aEntity.CollectionGUID) Then
                Dim entcol As colDXFEntities = aEntity.myCollection
                If entcol IsNot Nothing Then
                    entcol.ProcessEvent(aEvent)
                End If
            End If
        End Sub
        Friend Function GetImageBitmap(aImageGUID As String, bScreen As Boolean) As dxfBitmap
            Dim aImage As dxfImage = GetImage(aImageGUID)
            If aImage Is Nothing Then Return Nothing
            Return aImage.GetBitmap(bScreen)
        End Function
        Friend Function GetImageBlocks(aImageGUID As String) As colDXFBlocks
            Dim aImage As dxfImage = GetImage(aImageGUID)
            If aImage Is Nothing Then Return Nothing
            Return aImage.Blocks
        End Function
        Friend Function GetImageUCS(aImageGUID As String) As dxfPlane
            Return New dxfPlane(GetImageUCSV(aImageGUID))
        End Function
        Friend Function GetImageUCSV(aImageGUID As String) As TPLANE
            Dim aImage As dxfImage = GetImage(aImageGUID)
            Return New TPLANE(aImage?.UCS)
        End Function
        Friend Function NewEntity(aGraphicType As dxxGraphicTypes, ByRef rDefPts As dxpDefPoints, ByRef ioGUID As String) As TENTITY
            'On Error Resume Next
            If rDefPts Is Nothing Then rDefPts = New dxpDefPoints(aGraphicType, Nothing)
            Dim aSubEnt As TENTITY = NewSubEntity(aGraphicType, ioGUID)
            Dim _rVal As TENTITY = New TENTITY(aSubEnt)
            rDefPts.Copy(aSubEnt.DefPts)
            _rVal.TagFlagValue.Tag = ""
            _rVal.TagFlagValue.Flag = ""
            _rVal.TagFlagValue.Value = 0
            _rVal.Reactors = New TPROPERTYARRAY(aOwner:=ioGUID)
            _rVal.ExtendedData = _rVal.Reactors
            _rVal.SubEntities = New TENTITYARRAY("", "")
            _rVal.DefPts = New TDEFPOINTS(rDefPts)
            Select Case aGraphicType
            '        Case dxxGraphicTypes.Arc
            '
            '        Case dxxGraphicTypes.Bezier
            '
                Case dxxGraphicTypes.Dimension
                    _rVal.ExtendedData.Add(New TPROPERTIES("ACAD"))
            '        Case dxxGraphicTypes.Ellipse
            '        Case dxxGraphicTypes.Hatch
            '        Case dxxGraphicTypes.Hole
                Case dxxGraphicTypes.Insert


                    _rVal.SubEntities.Add(New TENTITIES("", "", "ATTRIBUTES"), "ATTRIBUTES", True, False, True)
                    '        Case dxxGraphicTypes.Leader
                    '        Case dxxGraphicTypes.Line
                    '        Case dxxGraphicTypes.Point
                    '        Case dxxGraphicTypes.Polygon
                    '       Case dxxGraphicTypes.Polyline
                    '        Case dxxGraphicTypes.Solid
                    '       Case dxxGraphicTypes.Symbol
                    '
                    '       Case dxxGraphicTypes.Table
                    '        Case dxxGraphicTypes.Text
                    '        Case Else
            End Select
            Return _rVal
        End Function
        Friend Function NewSubEntity(aGraphicType As dxxGraphicTypes, ByRef rGUID As String) As TENTITY
            rGUID = dxfEvents.NextEntityGUID(aGraphicType, rGUID)
            Dim _rVal As New TENTITY(aGraphicType, rGUID)
            'On Error Resume Next
            'assign a unique guid to the entity

            _rVal.Props = dxpProperties.Get_EntityProps(aGraphicType, rGUID)
            _rVal.SubProps.Owner = rGUID
            _rVal.Instances.GraphicType = aGraphicType
            'clear it's reactors
            _rVal.GUID = rGUID
            _rVal.DefPts.OwnerGUID = rGUID
            _rVal.Components.Paths.EntityGUID = rGUID
            Return _rVal
        End Function
        Friend Function RetrieveOwner(aHandleOwner As dxfHandleOwner, Optional arOwnerGUID As String = "") As dxfHandleOwner
            If aHandleOwner Is Nothing And String.IsNullOrWhiteSpace(arOwnerGUID) Then Return Nothing
            If String.IsNullOrWhiteSpace(arOwnerGUID) Then arOwnerGUID = aHandleOwner.OwnerGUID
            Dim aEnt As dxfEntity = Nothing
            RaiseEvent EntityRequest(arOwnerGUID, aEnt)
            Return aEnt
        End Function
        Friend Function UpdateEntityGraphic(aEntity As dxfEntity, Optional bErase As Boolean = True) As Boolean
            If aEntity Is Nothing Then Return False
            If aEntity.ImageGUID = "" Then Return False
            Dim aImage As dxfImage = GetImage(aEntity.ImageGUID)
            If aImage Is Nothing Then Return False
            Return dxfImageTool.Entity_Update(aImage, aEntity, bErase)
        End Function
#End Region 'Methods
    End Class 'dxpEventHandler
End Namespace
