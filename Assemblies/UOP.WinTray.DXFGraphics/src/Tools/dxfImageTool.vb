
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics

    Public Class dxfImageTool
        Friend Shared Function Entities_Erase(aImage As dxfImage, aEntities As colDXFEntities, Optional aHandles As List(Of String) = Nothing, Optional bRemoveFromImage As Boolean = True, Optional aTagString As String = "", Optional aDelimiter As Char = ",") As Boolean
            Dim _rVal As Boolean
            '#1the subject image
            '#2the entities to Erase
            '#3a collection of string handles to erase
            '#4flag to remove the entities form the image if they are members
            '#5if passed any image entities with matching tags will be erased
            '#6the delimiter for the tag string
            '^draws over the passed entities with the current background color. If Handlez
            '^handles are passed then the entities are located fom the current image and erased and
            If aImage Is Nothing Then Return _rVal
            Dim aEnt As dxfEntity
            Dim bEntities As colDXFEntities
            Dim sVals(0) As String
            Dim aTag As String
            Dim removed As New List(Of dxfEntity)
            Dim iEnts As List(Of dxfEntity) = aImage.Entities.CollectionObj
            If aEntities IsNot Nothing Then
                For i As Integer = 1 To aEntities.Count
                    aEnt = aEntities.Item(i)
                    If dxfImageTool.Entity_Erase(aImage, aEnt) Then _rVal = True
                    If bRemoveFromImage Then
                        If iEnts.IndexOf(aEnt) >= 0 Then
                            removed.Add(aEnt)
                            _rVal = True
                        End If
                    End If
                Next i
            End If
            If aHandles IsNot Nothing Then
                bEntities = aImage.Entities.GetByHandles(aHandles)
                For i As Integer = 1 To bEntities.Count
                    aEnt = bEntities.Item(i)
                    If dxfImageTool.Entity_Erase(aImage, aEnt) Then _rVal = True
                    If bRemoveFromImage Then
                        If iEnts.IndexOf(aEnt) >= 0 Then
                            removed.Add(aEnt)
                            _rVal = True
                        End If
                    End If
                Next i
            End If
            If Not String.IsNullOrWhiteSpace(aTagString) Then aTagString = aTagString.Trim Else aTagString = ""
            If aTagString <> "" Then
                If Not String.IsNullOrWhiteSpace(aDelimiter) Then
                    If aTagString.Contains(aDelimiter) Then
                        sVals = aTagString.Split(aDelimiter)
                    Else
                        ReDim sVals(0)
                        sVals(0) = aTagString
                    End If
                Else
                    ReDim sVals(0)
                    sVals(0) = aTagString
                End If
                For i As Integer = 1 To sVals.Length
                    aTag = sVals(i - 1)
                    If Not String.IsNullOrWhiteSpace(aTag) Then
                        Dim iEntCol As List(Of dxfEntity) = iEnts.FindAll(Function(mem) String.Compare(mem.Tag, aTag, ignoreCase:=True) = 0)
                        If iEntCol.Count > 0 Then
                            For Each aEnt In iEntCol
                                If dxfImageTool.Entity_Erase(aImage, aEnt) Then _rVal = True
                                If bRemoveFromImage Then
                                    removed.Add(aEnt)
                                End If
                            Next
                            _rVal = True
                        End If
                    End If
                Next
            End If
            '    oDisplay.AutoRedraw = aDraw
            If removed.Count > 0 Then
                _rVal = True
                aImage.Entities.RemoveMembersV(removed)
            End If
            Return _rVal
        End Function
        Friend Shared Function Entity_Erase(aImage As dxfImage, aEntity As dxfEntity) As Boolean
            If aEntity Is Nothing Or aImage Is Nothing Then Return False
            Dim _rVal As Boolean
            '#1the subject image
            '#2the subject entity
            '^redraws the passed entity with the null pen
            Try
                aImage.Erasing = True
                _rVal = aImage.Render_Entity(aEntity, True, True, True) > 0
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfImageTool", ex)
            Finally
                aImage.Erasing = False
            End Try
            Return _rVal
        End Function
        Friend Shared Function Entity_Update(aImage As dxfImage, aEntity As dxfEntity, Optional bEraseFlag As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the subject image
            '#2the entity to be update
            '#3flag to erase the entity before redrawing it
            '^redraws the entity if its GUID is part of the current images entities collection
            If aEntity Is Nothing Or aImage Is Nothing Then Return False
            Try
                Dim aGUID As String
                Dim aEnt As dxfEntity
                Dim aBlk As dxfBlock = Nothing
                Dim aEnts As colDXFEntities
                Dim i As Integer
                aEnts = New colDXFEntities
                aGUID = aEntity.BlockGUID
                If aGUID <> "" Then
                    If Not aImage.Blocks.TryGet(aGUID, aBlk, aPassedType:=dxxBlockReferenceTypes.GUID) Then
                        aGUID = ""
                    Else
                        aEnts = aImage.Entities.Inserts(aBlk.Name)
                    End If
                End If
                If aGUID = "" Then aGUID = aEntity.OwnerGUID
                If aGUID <> "" Then
                    If Not Not aImage.Blocks.TryGet(aGUID, aBlk, aPassedType:=dxxBlockReferenceTypes.GUID) Then
                        aGUID = ""
                    Else
                        aEnts.Add(aEntity)
                    End If
                End If
                If aGUID = "" Then
                    aGUID = aEntity.GUID
                    aEnts.Add(aImage.Entities.FindByGUID(aGUID))
                End If
                If aEnts.Count <= 0 Then Return _rVal
                If Not aImage.AutoRedraw Then
                    aImage.IsDirty = True
                    _rVal = True
                Else
                    For i = 1 To aEnts.Count
                        aEnt = aEnts.Item(i)
                        If bEraseFlag Then
                            If dxfImageTool.Entity_Erase(aImage, aEnt) Then _rVal = True
                        End If
                        aEnt.UpdatePath(False, aImage)
                        If aImage.Render_Entity(aEnt) > 0 Then _rVal = True
                    Next i
                End If
                If Not _rVal Then aImage.IsDirty = True
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfImageTool", ex)
            Finally
                aImage.Erasing = False
            End Try
            Return _rVal
        End Function
        Friend Shared Function TableCreate(aImage As dxfImage, aTableType As dxxReferenceTypes, bSuppressHandles As Boolean, bSuppressDefaultObjects As Boolean) As TTABLE
            Dim _rVal As New TTABLE(aTableType, Not bSuppressDefaultObjects)
            If aImage Is Nothing Then Return _rVal
            _rVal.ImageGUID = aImage.GUID
            Dim oTbl As dxfTable = dxfTable.FromTTABLE(_rVal)
            If Not bSuppressHandles Then aImage.HandleGenerator.AssignTo(oTbl)
            Return oTbl.Strukture
        End Function
        Friend Shared Function DisplayStructure_Symbol(aImage As dxfImage, aTextStyleName As String, ByRef rSettings As TTABLEENTRY, aLayerName As String, aColor As dxxColors, ByRef rTStyle As dxoStyle) As TDISPLAYVARS
            If aImage Is Nothing Then Return New TDISPLAYVARS
            Dim hdrDisp As TDISPLAYVARS
            Dim defLayr As String = String.Empty
            Dim defClr As dxxColors
            ' Dim bDefLayr As Boolean
            hdrDisp = dxfImageTool.HeaderDisplayVars(aImage)
            aTextStyleName = Trim(aTextStyleName)
            aLayerName = Trim(aLayerName)
            rSettings = aImage.BaseSettings(dxxSettingTypes.SYMBOLSETTINGS)
            rSettings.IsCopied = True
            If aTextStyleName = "" Then aTextStyleName = rSettings.Props.ValueStr("TextStyle")
            If aTextStyleName = "" Then aTextStyleName = hdrDisp.TextStyle
            aTextStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aTextStyleName)
            rSettings.Props.SetVal("TextStyle", aTextStyleName)
            rTStyle = aImage.TextStyles.TryGetOrAdd(aTextStyleName, "Standard", True)
            rSettings.Props.SetVal("TextStyle", rTStyle.Name)
            'set the return to a copy of the table held dimstyle(the parent)
            'assign the current overrides
            'set the layer
            If aLayerName = "" Then
                defLayr = rSettings.Props.ValueStr("LayerName")
                defClr = rSettings.Props.ValueI("LayerColor")
                If defLayr <> "" Then
                    'bDefLayr = True
                    aLayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, defLayr, aColor:=defClr)
                    If aColor = dxxColors.Undefined Then aColor = dxxColors.ByLayer
                End If
            End If
            If aLayerName = "" Then
                aLayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, hdrDisp.LayerName)
            Else
                If defLayr = "" Then aLayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aLayerName)
            End If
            Dim _rVal As New TDISPLAYVARS With {
                .Color = aColor,
                .LayerName = aLayerName,
                .Linetype = hdrDisp.Linetype,
                .LTScale = hdrDisp.LTScale,
                .LineWeight = hdrDisp.LineWeight,
                .TextStyle = aTextStyleName
            }
            Return _rVal
        End Function
        Friend Shared Function DisplayStructure_Table(aImage As dxfImage, ByRef rTableSettings As TTABLEENTRY) As TDISPLAYVARS
            Dim _rVal As New TDISPLAYVARS
            If aImage Is Nothing Then Return _rVal
            Dim aName As String
            Dim bname As String
            rTableSettings = aImage.BaseSettings(dxxSettingTypes.TABLESETTINGS).Clone
            rTableSettings.IsCopied = True
            _rVal = dxfImageTool.HeaderDisplayVars(aImage)
            _rVal.Linetype = dxfLinetypes.Continuous
            aName = rTableSettings.Props.ValueStr("LayerName")
            If aName <> "" Then
                _rVal.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aName, aColor:=rTableSettings.Props.ValueI("LayerColor"), aLineType:=dxfLinetypes.Continuous)
            End If
            aName = rTableSettings.Props.ValueStr("TextStyleName")
            If aName <> "" Then
                aName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aName)
                _rVal.TextStyle = aName
                rTableSettings.Props.SetVal("TextStyleName", aName)
            End If
            bname = rTableSettings.Props.ValueStr("TitleTextStyle")
            If bname <> "" Then
                bname = aImage.GetOrAdd(dxxReferenceTypes.STYLE, bname)
                rTableSettings.Props.SetVal("TitleTextStyle", bname)
            Else
                rTableSettings.Props.SetVal("TitleTextStyle", "")
            End If
            bname = rTableSettings.Props.ValueStr("FooterTextStyle")
            If bname <> "" Then
                bname = aImage.GetOrAdd(dxxReferenceTypes.STYLE, bname)
                rTableSettings.Props.SetVal("FooterTextStyle", bname)
            Else
                rTableSettings.Props.SetVal("FooterTextStyle", "")
            End If
            bname = rTableSettings.Props.ValueStr("HeaderTextStyle")
            If bname <> "" Then
                bname = aImage.GetOrAdd(dxxReferenceTypes.STYLE, bname)
                rTableSettings.Props.SetVal("HeaderTextStyle", bname)
            Else
                rTableSettings.Props.SetVal("HeaderTextStyle", "")
            End If
            Return _rVal
        End Function
        Friend Shared Function DisplayStructure_Text(aImage As dxfImage, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aTextStyleName As String = "") As TDISPLAYVARS
            If aImage Is Nothing Then Return New TDISPLAYVARS("0")
            If String.IsNullOrWhiteSpace(aLayerName) Then aLayerName = ""
            If String.IsNullOrWhiteSpace(aTextStyleName) Then aTextStyleName = ""
            aLayerName = aLayerName.Trim
            aTextStyleName = aTextStyleName.Trim
            Dim defLayr As String = String.Empty
            Dim defClr As dxxColors
            Dim hdrDisp As TDISPLAYVARS = dxfImageTool.HeaderDisplayVars(aImage)
            If aColor = dxxColors.ByBlock Then aColor = dxxColors.ByLayer
            If aLayerName = "" Then
                Dim p As TPROPERTIES = aImage.BaseSettings(dxxSettingTypes.TEXTSETTINGS).Props
                defLayr = p.ValueStr("LayerName")
                defClr = p.ValueI("LayerColor")
                If defLayr <> "" Then
                    aLayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, defLayr, aColor:=defClr, aLineWeigth:=p.ValueI("LineWeight"))
                    If aColor = dxxColors.Undefined Then aColor = dxxColors.ByLayer
                End If
            End If
            If aLayerName = "" Then
                aLayerName = hdrDisp.LayerName
                aLayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aLayerName)
            Else
                If defLayr = "" Then aLayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aLayerName)
            End If
            If aTextStyleName = "" Then aTextStyleName = hdrDisp.TextStyle
            aTextStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aTextStyleName)
            If aColor = dxxColors.Undefined Then aColor = aImage.BaseSettings(dxxSettingTypes.TEXTSETTINGS).Props.ValueI("Color")
            If aColor = dxxColors.Undefined Then aColor = hdrDisp.Color
            Return New TDISPLAYVARS(aLayerName, dxfLinetypes.Continuous, aColor, hdrDisp.LineWeight, hdrDisp.LTScale, aTextStyleName:=aTextStyleName)
        End Function
        Friend Shared Function LayerPropsGET(aImage As dxfImage, aName As String, ByRef rColor As dxxColors, ByRef rLinetype As String, rLineWeight As dxxLineWeights, Optional bReturnDefaultLWt As Boolean = True) As Object
            rColor = dxxColors.Undefined
            rLinetype = ""
            rLineWeight = dxxLineWeights.Undefined
            Dim _rVal As Object = Nothing
            If aImage Is Nothing Then Return _rVal
            If String.IsNullOrWhiteSpace(aName) Then aName = ""
            aName = Trim(aName)
            If aName = "" Then aName = aImage.Header.LayerName
            Dim aEntry As dxoLayer = aImage.Layers.TryGetOrAdd(aName, "0")

            rColor = aEntry.PropValueI(dxxLayerProperties.Color, bAbsVal:=True)
            rLinetype = aEntry.Linetype
            rLineWeight = aEntry.LineWeight
            If rLineWeight = dxxLineWeights.ByDefault And bReturnDefaultLWt Then
                rLineWeight = aImage.Header.LineWeightDefault
            End If
            Return _rVal
        End Function
        Friend Shared Function DisplayStructure(aImage As dxfImage, aLayer As String, aColor As dxxColors, aLineType As String, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As TDISPLAYVARS
            Dim rHeaderVars As TDISPLAYVARS = Nothing
            Return DisplayStructure(aImage, rHeaderVars, aLayer, aColor, aLineType, aLTLFlag)
        End Function
        Friend Shared Function DisplayStructure(aImage As dxfImage, ByRef rHeaderVars As TDISPLAYVARS, aLayer As String, aColor As dxxColors, aLineType As String, aLTLFlag As dxxLinetypeLayerFlag) As TDISPLAYVARS
            rHeaderVars = dxfImageTool.HeaderDisplayVars(aImage)
            If aImage Is Nothing Then Return New TDISPLAYVARS
            Dim _rVal As TDISPLAYVARS = rHeaderVars.Clone
            '#1the subject image
            '#2an overriding layername
            '#3an overriding color
            '#4an overriding linetype
            '#5an overriding linetype layer setting
            '^returns the display settings that would be applied to a new entity added to the image
            If aLTLFlag < 0 Or aLTLFlag > 2 Then aLTLFlag = aImage.LinetypeLayers.Setting
            If String.IsNullOrWhiteSpace(aLayer) Then aLayer = "" Else aLayer = aLayer.Trim()
            If String.IsNullOrWhiteSpace(aLineType) Then aLineType = rHeaderVars.Linetype Else aLineType = aLineType.Trim()
            aLineType = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, aLineType)
            If aColor = dxxColors.Undefined Then aColor = rHeaderVars.Color
            If aLayer = "" Then
                If aLTLFlag <> dxxLinetypeLayerFlag.Suppressed And aImage.LinetypeLayers.Count > 0 Then
                    If (String.Compare(aLineType, dxfLinetypes.ByBlock, True) <> 0) And (String.Compare(aLineType, dxfLinetypes.ByLayer, True) <> 0) Then
                        aImage.LinetypeLayers.Apply(aLTLFlag, aImage, aLineType, aLayer, aColor)
                    End If
                End If
            End If
            If aLayer = "" Then aLayer = rHeaderVars.LayerName
            aLayer = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aLayer)
            Return New TDISPLAYVARS(aLayer, aLineType, aColor, rHeaderVars.LineWeight, rHeaderVars.LTScale, rHeaderVars.DimStyle, rHeaderVars.TextStyle)
        End Function

        Friend Shared Function DisplayStructure_DIM(aImage As dxfImage, aForLeader As Boolean, ByRef ioInput As TDIMINPUT, ByRef rDStyle As dxoDimStyle, ByRef rTStyle As dxoStyle) As TDISPLAYVARS
            rDStyle = Nothing
            rTStyle = Nothing
            ioInput.DisplayVars = dxfImageTool.DisplayStructure(aImage, aLayer:="", aColor:=dxxColors.Undefined, aLineType:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
            If aImage Is Nothing Then Return ioInput.DisplayVars
            Dim defClr As dxxColors
            Dim bDefLyr As Boolean
            Dim hdrDisp As TDISPLAYVARS = dxfImageTool.HeaderDisplayVars(aImage)
            Dim defLayr As String
            Dim aBl As dxfBlock = Nothing
            Dim idstyle As dxoDimStyle = Nothing
            If String.IsNullOrWhiteSpace(ioInput.DimStyleName) Then ioInput.DimStyleName = aImage.Header.DimStyleName
            ioInput.DimStyleName = aImage.GetOrAdd(dxxReferenceTypes.DIMSTYLE, ioInput.DimStyleName, rEntry:=idstyle)
            rDStyle = New dxoDimStyle(idstyle) With {.IsCopied = True}

            'set the return to a copy of the table held dimstyle(the parent)
            'assign the current overrides


            'if the name of the dimstyle is the current then pick up the overrides
            If String.Compare(ioInput.DimStyleName, hdrDisp.DimStyle, True) = 0 Then
                rDStyle.Properties.CopyVals(aImage.DimStyleOverrides.Properties, bSkipHandles:=False, bSkipPointers:=False)
            End If
            If Not ioInput.DimLineColor.HasValue Then
                ioInput.DimLineColor = rDStyle.DimLineColor
            Else
                If ioInput.DimLineColor.Value <> dxxColors.Undefined Then
                    rDStyle.PropValueSet(dxxDimStyleProperties.DIMCLRD, ioInput.DimLineColor.Value, True)
                Else
                    ioInput.DimLineColor = rDStyle.DimLineColor
                End If
            End If
            If Not ioInput.ExtLineColor.HasValue Then
                ioInput.DimLineColor = rDStyle.ExtLineColor
            Else
                If ioInput.ExtLineColor.Value <> dxxColors.Undefined Then
                    rDStyle.PropValueSet(dxxDimStyleProperties.DIMCLRE, ioInput.ExtLineColor.Value, True)
                Else
                    ioInput.ExtLineColor = rDStyle.ExtLineColor
                End If
            End If
            If Not ioInput.TextColor.HasValue Then
                ioInput.TextColor = rDStyle.TextColor
            Else
                If ioInput.TextColor.Value <> dxxColors.Undefined Then
                    rDStyle.PropValueSet(dxxDimStyleProperties.DIMCLRT, ioInput.TextColor.Value, True)
                Else
                    ioInput.TextColor = rDStyle.TextColor
                End If
            End If




            'verify and retrieve the text style
            If ioInput.TextStyleName = "" Then
                ioInput.TextStyleName = rDStyle.TextStyleName
            End If
            Dim tstyle As dxoStyle = Nothing
            ioInput.TextStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, ioInput.TextStyleName, rEntry:=tstyle)
            rTStyle = New dxoStyle(tstyle) With {.IsCopied = True}
            'set the layer
            'defClr = dxxColors.Undefined
            If ioInput.LayerName = "" Then
                Dim s As TTABLEENTRY = aImage.BaseSettings(dxxSettingTypes.DIMSETTINGS)
                If aForLeader Then
                    defLayr = s.Props.ValueStr("LeaderLayer")
                    defClr = s.Props.ValueI("LeaderLayerColor")
                Else
                    defLayr = s.Props.ValueStr("DimLayer")
                    defClr = s.Props.ValueI("DimLayerColor")
                End If
                If defLayr <> "" Then
                    bDefLyr = True
                    If defClr = dxxColors.Undefined Then defClr = dxxColors.BlackWhite
                    If ioInput.Color = dxxColors.Undefined Then ioInput.Color = dxxColors.ByLayer
                    ioInput.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, defLayr, aColor:=defClr, aLineType:=dxfLinetypes.Continuous)
                End If
            End If
            If Not bDefLyr Then
                If String.IsNullOrEmpty(ioInput.LayerName) Then ioInput.LayerName = hdrDisp.LayerName
                ioInput.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, ioInput.LayerName)
                If ioInput.Color = dxxColors.Undefined Then ioInput.Color = hdrDisp.Color
            End If
            If ioInput.DimLineColor = dxxColors.Undefined Then
                If bDefLyr Then ioInput.DimLineColor = dxxColors.ByLayer Else ioInput.DimLineColor = rDStyle.PropValueI(dxxDimStyleProperties.DIMCLRD)
            End If
            If ioInput.ExtLineColor = dxxColors.Undefined Then
                If bDefLyr Then ioInput.ExtLineColor = dxxColors.ByLayer Else ioInput.ExtLineColor = rDStyle.PropValueI(dxxDimStyleProperties.DIMCLRE)
            End If
            If String.IsNullOrEmpty(ioInput.Linetype) Then ioInput.Linetype = dxfLinetypes.ByLayer
            ioInput.Linetype = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, ioInput.Linetype)
            If ioInput.TextColor = dxxColors.Undefined Then
                If bDefLyr Then
                    If aImage.BaseSettings(dxxSettingTypes.TEXTSETTINGS).Props.ValueStr("LayerName") <> "" Then
                        defClr = aImage.BaseSettings(dxxSettingTypes.TEXTSETTINGS).Props.ValueI("LayerColor")
                        If defClr <> dxxColors.Undefined Then ioInput.TextColor = defClr
                    End If
                End If
                ioInput.TextColor = rDStyle.PropValueI(dxxDimStyleProperties.DIMCLRT)
            End If
            If Not ioInput.DimScale.HasValue Then ioInput.DimScale = rDStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
            If Not ioInput.DimScale.Value <= 0 Then ioInput.DimScale = rDStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
            If String.IsNullOrEmpty(ioInput.Prefix) Then ioInput.Prefix = rDStyle.PropValueStr(dxxDimStyleProperties.DIMPREFIX)
            If String.IsNullOrEmpty(ioInput.Suffix) Then ioInput.Suffix = rDStyle.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)
            If ioInput.ArrowSize <= 0 Then ioInput.ArrowSize = rDStyle.PropValueD(dxxDimStyleProperties.DIMASZ)
            '    With rDStyle.ArrowSize
            If aForLeader Then
                If Not String.IsNullOrEmpty(ioInput.ArrowBlockL) Then

                    If Not aImage.Blocks.TryGet(ioInput.ArrowBlockL, aBl) Then
                        ioInput.ArrowHeadL = dxxArrowHeadTypes.ByStyle
                    Else
                        If aBl.IsArrowHead Then ioInput.ArrowHeadL = dxfArrowheads.GetBlockType(aBl.Name)
                        ioInput.ArrowBlockL = aBl.Name
                        rDStyle.PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, aBl.Name)
                        rDStyle.PropValueSet(dxxDimStyleProperties.DIMLDRBLK, aBl.Handle)
                    End If
                End If
                If ioInput.ArrowHeadL >= dxxArrowHeadTypes.ClosedFilled And ioInput.ArrowHeadL <= dxxArrowHeadTypes.None Then
                    ioInput.ArrowBlockL = dxfEnums.Description(ioInput.ArrowHeadL)
                    rDStyle.PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, ioInput.ArrowBlockL)
                    rDStyle.PropValueSet(dxxDimStyleProperties.DIMLDRBLK, aImage.Blocks.GetHandle(ioInput.ArrowBlockL, True))
                End If
            Else
                If Not String.IsNullOrEmpty(ioInput.ArrowBlock1) Then

                    If Not String.IsNullOrEmpty(ioInput.ArrowBlock1) Then
                        If Not aImage.Blocks.TryGet(ioInput.ArrowBlock1, aBl) Then
                            ioInput.ArrowHead1 = dxxArrowHeadTypes.ByStyle
                        Else
                            If aBl.IsArrowHead Then ioInput.ArrowHead1 = dxfArrowheads.GetBlockType(aBl.Name)
                            ioInput.ArrowBlock1 = aBl.Name
                            rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, aBl.Name)
                            rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK1, aBl.Handle)
                        End If
                    End If
                    If ioInput.ArrowHead1 >= dxxArrowHeadTypes.ClosedFilled And ioInput.ArrowHead1 <= dxxArrowHeadTypes.None Then
                        ioInput.ArrowBlock1 = dxfEnums.Description(ioInput.ArrowHead1)
                        rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, ioInput.ArrowBlock1)
                        rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK1, aImage.Blocks.GetHandle(ioInput.ArrowBlock1, True))
                    End If
                End If
                If Not String.IsNullOrEmpty(ioInput.ArrowBlock2) Then

                    If Not aImage.Blocks.TryGet(ioInput.ArrowBlock2, aBl) Then
                        ioInput.ArrowHead2 = dxxArrowHeadTypes.ByStyle
                    Else
                        If aBl.IsArrowHead Then ioInput.ArrowHead2 = dxfArrowheads.GetBlockType(aBl.Name)
                        ioInput.ArrowBlock2 = aBl.Name
                        rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK2_NAME, aBl.Name)
                        rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK2, aBl.Handle)
                    End If
                End If
                If ioInput.ArrowHead2 >= dxxArrowHeadTypes.ClosedFilled And ioInput.ArrowHead2 <= dxxArrowHeadTypes.None Then
                    ioInput.ArrowBlock2 = dxfEnums.Description(ioInput.ArrowHead2)
                    rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK2_NAME, ioInput.ArrowBlock2)
                    rDStyle.PropValueSet(dxxDimStyleProperties.DIMBLK2, aImage.Blocks.GetHandle(ioInput.ArrowBlock2, True))
                End If
            End If

            rDStyle.IsCopied = True
            rDStyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY, rTStyle.Handle, True)
            rDStyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, rTStyle.Name, True)
            rDStyle.PropValueSet(dxxDimStyleProperties.DIMPREFIX, ioInput.Prefix, True)
            rDStyle.PropValueSet(dxxDimStyleProperties.DIMSUFFIX, ioInput.Suffix, True)
            rDStyle.PropValueSet(dxxDimStyleProperties.DIMSCALE, ioInput.DimScale.Value, True)
            rDStyle.PropValueSet(dxxDimStyleProperties.DIMASZ, ioInput.ArrowSize, True)
            '    img_ConfirmArrowHeads aImage, rDStyle
            ioInput.DisplayVars.Color = ioInput.Color
            ioInput.DisplayVars.LayerName = ioInput.LayerName
            ioInput.DisplayVars.Linetype = ioInput.Linetype
            ioInput.DisplayVars.LTScale = hdrDisp.LTScale
            ioInput.DisplayVars.LineWeight = hdrDisp.LineWeight
            ioInput.DisplayVars.DimStyle = ioInput.DimStyleName
            ioInput.DisplayVars.TextStyle = ioInput.TextStyleName
            ioInput.ExtLineColor = rDStyle.ExtLineColor
            ioInput.DimLineColor = rDStyle.DimLineColor
            ioInput.TextColor = rDStyle.TextColor
            Return ioInput.DisplayVars
        End Function

        Friend Shared Function HeaderDisplayVars(aImage As dxfImage) As TDISPLAYVARS
            If aImage Is Nothing Then Return New TDISPLAYVARS
            Dim aHdr As dxsHeader = aImage.Header
            Return New TDISPLAYVARS(aHdr.LayerName, aHdr.Linetype, aHdr.Color, aHdr.LineWeight, aHdr.LineTypeScaleEnt, aHdr.DimStyleName, aHdr.TextStyleName)
        End Function
        Friend Shared Sub ConfirmArrowHeads(aImage As dxfImage, aStyle As TTABLEENTRY, Optional bSuppressImageUpdate As Boolean = False)
            If aImage Is Nothing Then Return
            If aStyle.Props.Count <= 0 Or aStyle.EntryType <> dxxReferenceTypes.DIMSTYLE Then Return
            dxfImageTool.VerifyDimstyle(aImage, aStyle, aImage.Blocks, aStyle.IsCopied Or bSuppressImageUpdate)
        End Sub

        Friend Shared Sub ConfirmArrowHeads(aImage As dxfImage, aStyle As dxoDimStyle, Optional bSuppressImageUpdate As Boolean = False)
            If aImage Is Nothing Or aStyle Is Nothing Then Return

            dxfImageTool.VerifyDimstyle(aImage, aStyle, aImage.Blocks, aStyle.IsCopied Or bSuppressImageUpdate)
        End Sub
        Friend Shared Sub VerifyDimstyle(aImage As dxfImage, aStyle As TTABLEENTRY, ByRef rBlocks As colDXFBlocks, Optional bSuppressImageUpdate As Boolean = False)
            If aImage Is Nothing Then Return
            If aStyle.EntryType <> dxxReferenceTypes.DIMSTYLE Or aStyle.Props.Count <= 0 Then Return
            If rBlocks Is Nothing Then rBlocks = aImage.Blocks
            Dim aRef As dxfTableEntry = Nothing
            Dim nval As String
            Dim hVal As String
            Dim ridx As Integer = -1
            Dim i As dxxDimStyleProperties
            Dim j As Integer
            Dim aBl As dxfBlock
            Dim sName As String
            Dim rHnd As String
            Dim bChange As Boolean
            Dim pname As String
            Dim rName As String
            Dim dRef As dxfTableEntry = Nothing
            Dim hProp As TPROPERTY
            Dim nProp As TPROPERTY
            Dim bn0 As String
            Dim bn1 As String = String.Empty
            Dim bn2 As String = String.Empty
            Dim p1 As TPROPERTY = TPROPERTY.Null
            Dim p2 As TPROPERTY = TPROPERTY.Null
            Dim p3 As TPROPERTY = TPROPERTY.Null
            Try
                sName = aStyle.Name
                Dim aProps As TPROPERTIES = aStyle.Props
                j = 1
                For i = dxxDimStyleProperties.DIMTXSTY To dxxDimStyleProperties.DIMLTEX2
                    pname = dxfEnums.PropertyName(i).ToUpper()
                    hProp = aProps.Item(pname)  'get the handle property
                    If hProp.Index > 0 Then
                        nProp = aProps.Item($"*{ pname}_NAME")  'get the handle property
                        rHnd = hProp.ValueS.Trim()
                        If rHnd = "0" Then rHnd = ""
                        rName = nProp.ValueS.Trim()
                        aBl = Nothing
                        If i = dxxDimStyleProperties.DIMTXSTY Then
                            'textstyle
                            dRef = aImage.Styles.Item("Standard")
                            If rName = "" And rHnd = "" Then rName = "Standard"
                            If Not String.IsNullOrWhiteSpace(rHnd) Then aRef = aImage.Styles.Find(Function(x) x.Handle = rHnd)
                            If aRef Is Nothing And rName <> "" Then aRef = aImage.Styles.Find(Function(x) String.Compare(x.Name, rName, True) = 0)
                            If aRef Is Nothing Then aRef = aImage.TextStyle("Standard")
                            nval = aRef.Name : If nProp.SetVal(nval) Then bChange = True
                            hVal = aRef.Handle : If hProp.SetVal(hVal) Then bChange = True
                            hProp.SuppressedValue = dRef.Handle
                            aProps.UpdateProperty = nProp
                            aProps.UpdateProperty = hProp
                        ElseIf i >= dxxDimStyleProperties.DIMLDRBLK And i <= dxxDimStyleProperties.DIMBLK2 Then
                            'arrowheads
                            If i = dxxDimStyleProperties.DIMLDRBLK Then dRef = aImage.BlockRecords.Item("_ClosedFilled")
                            If rName = "" And rHnd = "" Then rName = "_ClosedFilled"
                            'get the block And rectified handle and name.  will get "_ClosedFilled" if the block can't be found
                            aBl = rBlocks.GetArrowHead(ioBRHandle:=rHnd, ioBlockName:=rName, bTryHandleFirst:=False, bReturnDefaultIfNotFound:=True, aImage:=aImage)
                            'If aBl Is NOthing Then
                            '    If rName = "_ClosedFilled" Then
                            '        MessageBox.Show(rBlocks.Names)
                            '    End If
                            'End If
                            If aBl IsNot Nothing Then
                                aBl.Suppressed = String.Compare(aBl.Name, "_ClosedFilled", True) = 0
                                nval = aBl.Name : If nProp.SetVal(nval) Then bChange = True
                                hVal = aBl.BlockRecordHandle : If hProp.SetVal(hVal) Then bChange = True
                                hProp.Suppressed = aBl.Suppressed
                                hProp.SuppressedValue = dRef.Handle
                                aProps.UpdateProperty = nProp
                                aProps.UpdateProperty = hProp
                            Else
                                hVal = dRef.Handle
                                nval = "_ClosedFilled"
                            End If
                            If i = dxxDimStyleProperties.DIMBLK Then
                                bn0 = nval
                                p1 = hProp
                            ElseIf i = dxxDimStyleProperties.DIMBLK1 Then
                                bn1 = nval
                                p2 = hProp
                            ElseIf i = dxxDimStyleProperties.DIMBLK2 Then
                                bn2 = nval
                                p3 = hProp
                            End If
                            aProps.UpdateProperty = nProp
                            aProps.UpdateProperty = hProp
                        Else
                            If i = dxxDimStyleProperties.DIMLTYPE Then dRef = aImage.Linetypes.Item(dxfLinetypes.ByBlock)
                            If rName = "" And rHnd = "" Then rName = dxfLinetypes.ByBlock
                            If Not String.IsNullOrWhiteSpace(rHnd) Then aRef = aImage.Linetypes.Find(Function(x) x.Handle = rHnd)
                            If aRef Is Nothing And rName <> "" Then aRef = aImage.Linetypes.Find(Function(x) String.Compare(x.Name, rName, True) = 0)
                            If aRef Is Nothing Then aRef = aImage.Linetypes.Item(dxfLinetypes.ByBlock)

                            nval = aRef.Name : If nProp.SetVal(nval) Then bChange = True
                            hVal = aRef.Handle : If hProp.SetVal(hVal) Then bChange = True
                            'hProp.Suppressed = String.Compare(nval, dxfLinetypes.ByBlock, True) = 0
                            hProp.SuppressedValue = dRef.Handle
                            'linetypes
                            aProps.UpdateProperty = hProp
                            aProps.UpdateProperty = nProp
                        End If
                        j += 1
                    End If
                Next i
                p1.Suppressed = String.Compare(bn1, bn2, True) <> 0
                p2.Suppressed = Not p1.Suppressed
                p3.Suppressed = p2.Suppressed
                If Not p1.Suppressed Then
                    aProps.SetVal("DIMSAH", 0)
                    nProp = aProps.Item("*DIMBLK_NAME")
                    p2.Value = p1.Value
                    p3.Value = p1.Value
                    aProps.SetVal("*DIMBLK1_NAME", nProp.Value)
                    aProps.SetVal("*DIMBLK2_NAME", nProp.Value)
                Else
                    aProps.SetVal("DIMSAH", 1)
                End If
                aProps.UpdateProperty = p1
                aProps.UpdateProperty = p2
                aProps.UpdateProperty = p3
                ''save the changes
                aStyle.Props = aProps
                If aStyle.IsGlobal Then
                    aImage.BaseSettings_Set(aStyle)
                Else
                    If Not aStyle.IsCopied And Not bSuppressImageUpdate Then
                        'Dim aTbl As TTABLE = aImage.BaseTable(dxxReferenceTypes.DIMSTYLE)
                        'If aTbl.Contains(sName) Then
                        '    aTbl.UpdateEntry = aStyle
                        '    aImage.BaseTable_Set(aTbl)
                        'End If
                    Else
                        If bChange Then
                            aStyle.IsDirty = True
                        End If
                    End If
                End If
            Catch ex As Exception
                aImage.HandleError("VerifyDimStyle", "dxfImageTool", ex)
            End Try
        End Sub
        Friend Shared Sub VerifyDimstyle(aImage As dxfImage, aStyle As dxoDimStyle, ByRef rBlocks As colDXFBlocks, Optional bSuppressImageUpdate As Boolean = False)
            If aImage Is Nothing Then Return
            If aStyle Is Nothing Then Return



            If rBlocks Is Nothing Then rBlocks = aImage.Blocks

            Dim bChange As Boolean = False

            Try
                bChange = VerifyDimstyleProperties(aImage, aStyle.Properties, rBlocks)

                ''save the changes
                If aStyle.IsGlobal Then
                    aImage.BaseSettings_Set(New TTABLEENTRY(aStyle))
                Else
                    If Not aStyle.IsCopied And Not bSuppressImageUpdate Then
                        Dim aTbl As dxoDimStyles = aImage.DimStyles
                    Else
                        If bChange Then
                            aStyle.IsDirty = True
                        End If
                    End If
                End If
            Catch ex As Exception
                aImage.HandleError("VerifyDimStyle", "dxfImageTool", ex)
            End Try
        End Sub

        Friend Shared Function VerifyDimstyleProperties(aImage As dxfImage, aStyleProps As dxoProperties, ByRef ioBlocks As colDXFBlocks) As Boolean
            If aImage Is Nothing Then Return False
            If aStyleProps Is Nothing Then Return False
            If ioBlocks Is Nothing Then ioBlocks = aImage.Blocks


            Dim ridx As Integer = -1
            Dim i As dxxDimStyleProperties
            Dim j As Integer
            Dim aBl As dxfBlock
            Dim sName As String
            Dim rHnd As String
            Dim bChange As Boolean

            Dim rName As String
            Dim dRef As dxfTableEntry = Nothing


            Dim bn0 As String
            Dim bn1 As String = String.Empty
            Dim bn2 As String = String.Empty
            Dim p1 As dxoProperty = Nothing
            Dim p2 As dxoProperty = Nothing
            Dim p3 As dxoProperty = Nothing
            Dim prefixes As New List(Of String)({"*", "$"})
            Try
                sName = aStyleProps.ValueS(2)

                j = 1
                For i = dxxDimStyleProperties.DIMTXSTY To dxxDimStyleProperties.DIMLTEX2
                    Dim pname As String = dxfEnums.PropertyName(i)
                    Dim hProp As dxoProperty = Nothing
                    Dim nProp As dxoProperty = Nothing
                    Dim aRef As dxfTableEntry = Nothing
                    Dim nval As String = String.Empty
                    Dim hVal As String = String.Empty
                    hProp = aStyleProps.Find(Function(x) String.Compare(x.Name, pname, True) = 0)
                    If aStyleProps.TryGet(pname, hProp, aPrefixsToConsider:=prefixes) Then 'get the handle property
                        If aStyleProps.TryGet($"{ pname}_NAME", nProp, aPrefixsToConsider:=prefixes) Then 'get the name property
                            rHnd = hProp.ValueS
                            If rHnd = "0" Then rHnd = ""
                            rName = nProp.ValueS
                            aBl = Nothing
                            If i = dxxDimStyleProperties.DIMTXSTY Then
                                'textstyle
                                dRef = aImage.TextStyle("Standard")
                                If rName = "" And rHnd = "" Then rName = "Standard"
                                aRef = aImage.Styles.Find(Function(x) String.Compare(x.Handle, rHnd, True) = 0)
                                If aRef Is Nothing And rName <> "" Then aRef = aImage.Styles.Find(Function(x) String.Compare(x.Name, rName, True) = 0)
                                If aRef Is Nothing Then aRef = aImage.TextStyle("Standard")
                                nval = aRef.Name : If nProp.SetVal(nval) Then bChange = True
                                hVal = aRef.Handle : If hProp.SetVal(hVal) Then bChange = True
                                hProp.SuppressedValue = dRef.Handle
                            ElseIf i >= dxxDimStyleProperties.DIMLDRBLK And i <= dxxDimStyleProperties.DIMBLK2 Then
                                'arrowheads
                                If i = dxxDimStyleProperties.DIMLDRBLK Then dRef = aImage.BlockRecords.Entry("_ClosedFilled")
                                If rName = "" And rHnd = "" Then rName = "_ClosedFilled"
                                'get the block And rectified handle and name.  will get "_ClosedFilled" if the block can't be found
                                aBl = ioBlocks.GetArrowHead(ioBRHandle:=rHnd, ioBlockName:=rName, bTryHandleFirst:=False, bReturnDefaultIfNotFound:=True, aImage:=aImage)
                                'If aBl Is NOthing Then
                                '    If rName = "_ClosedFilled" Then
                                '        MessageBox.Show(ioBlocks.Names)
                                '    End If
                                'End If
                                If aBl IsNot Nothing Then
                                    aBl.Suppressed = String.Compare(aBl.Name, "_ClosedFilled", True) = 0
                                    nval = aBl.Name : If nProp.SetVal(nval) Then bChange = True
                                    hVal = aBl.BlockRecordHandle : If hProp.SetVal(hVal) Then bChange = True
                                    hProp.Suppressed = aBl.Suppressed
                                    hProp.SuppressedValue = dRef.Handle
                                Else
                                    hVal = dRef.Handle
                                    nval = "_ClosedFilled"
                                End If
                                If i = dxxDimStyleProperties.DIMBLK Then
                                    bn0 = nval
                                    p1 = hProp
                                ElseIf i = dxxDimStyleProperties.DIMBLK1 Then
                                    bn1 = nval
                                    p2 = hProp
                                ElseIf i = dxxDimStyleProperties.DIMBLK2 Then
                                    bn2 = nval
                                    p3 = hProp
                                End If
                            Else
                                If i = dxxDimStyleProperties.DIMLTYPE Then

                                    dRef = aImage.Linetypes.Entry(dxfLinetypes.ByBlock)
                                End If
                                If rName = String.Empty And rHnd = String.Empty Then rName = dxfLinetypes.ByBlock
                                If rHnd <> String.Empty Then aRef = aImage.Linetypes.Find(Function(x) String.Compare(x.Handle, rHnd, True) = 0)
                                If aRef Is Nothing And rName <> String.Empty Then aRef = aImage.Linetypes.Find(Function(x) String.Compare(x.Name, rName, True) = 0)
                                If aRef Is Nothing Then aRef = aImage.Linetypes.Entry(dxfLinetypes.ByBlock)
                                nval = aRef.Name : If nProp.SetVal(nval) Then bChange = True
                                hVal = aRef.Handle : If hProp.SetVal(hVal) Then bChange = True
                                'hProp.Suppressed = String.Compare(nval, dxfLinetypes.ByBlock, True) = 0
                                hProp.SuppressedValue = dRef.Handle
                                'linetypes
                            End If
                        End If

                        j += 1
                    End If
                Next i
                p1.Suppressed = String.Compare(bn1, bn2, True) <> 0
                p2.Suppressed = Not p1.Suppressed
                p3.Suppressed = p2.Suppressed
                If Not p1.Suppressed Then
                    aStyleProps.SetVal("DIMSAH", 0, True, aPrefixsToConsider:=prefixes)
                    Dim nProp As dxoProperty = aStyleProps.Item("*DIMBLK_NAME")
                    p2.Value = p1.Value
                    p3.Value = p1.Value
                    aStyleProps.SetVal("*DIMBLK1_NAME", nProp.Value, aPrefixsToConsider:=prefixes)
                    aStyleProps.SetVal("*DIMBLK2_NAME", nProp.Value, aPrefixsToConsider:=prefixes)
                Else
                    aStyleProps.SetVal("DIMSAH", 1, aPrefixsToConsider:=prefixes)
                End If
                ''save the changes
                If bChange Then
                    aStyleProps.IsDirty = True
                End If
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), "dxfImageTool", ex)
            End Try

            Return bChange

        End Function
        Friend Shared Sub VerifyDefaultMembers(aImage As dxfImage, Optional bClearGroups As Boolean = False, Optional bUpdateBitCodes As Boolean = False)
            If aImage Is Nothing Then Return


            Dim aHG As dxoHandleGenerator = aImage.HandleGenerator

            Dim blocks As colDXFBlocks = aImage.Blocks
            Dim aErr As String = String.Empty
            Dim aBlk As dxfBlock = Nothing
            Dim aBlocks As List(Of dxfBlock) = blocks.CollectionObj
            Dim entry As dxfTableEntry = Nothing
            Dim addentries As New List(Of dxfTableEntry)()
            Dim tables As dxoTables = aImage.Tables

            Try
                For Each table As dxfTable In tables
                    addentries.Clear()

                    Dim defNames As List(Of String) = dxfTable.DefaultMemberNames(table.TableType)
                    For Each defName As String In defNames
                        If Not table.TryGetEntry(defName, rEntry:=entry) Then
                            entry = dxfTableEntry.CreateEntry(table.TableType, defName)
                            If entry IsNot Nothing Then
                                table.Add(entry)

                                addentries.Add(entry)
                            End If

                        End If
                    Next
                    aHG.AssignTo(table, bSuppressMembers:=False)
                    If bUpdateBitCodes Then table.UpdateBitCodes()
                    If table.TableType = dxxReferenceTypes.DIMSTYLE Then
                        'set the handle of the active style
                        Dim defName As String = aImage.Header.DimStyleName

                        If table.TryGetEntry(defName, entry) Then
                            table.Properties.SetVal(340, entry.Handle)
                        End If
                        For Each entry In table
                            entry.IsGlobal = False
                            dxfImageTool.VerifyDimstyle(aImage, entry, blocks)
                        Next

                        dxfImageTool.VerifyDimstyleProperties(aImage, aImage.DimStyleOverrides.Properties, blocks)
                    End If

                    If table.TableType = dxxReferenceTypes.BLOCK_RECORD Then
                        For Each defName As String In defNames
                            aBlk = aBlocks.Find(Function(mem) String.Compare(mem.Name, defName, ignoreCase:=True) = 0)
                            If aBlk Is Nothing Then ' aBlocks.TryGetValue(defName.ToUpper, aBlk) Then
                                If defName.StartsWith("_") Then
                                    aBlk = dxfArrowheads.CreateArrowHeadBlock(aImage:=Nothing, defName, bDontUpdateImage:=True)
                                Else
                                    If String.Compare(defName, "*PaperSpace", ignoreCase:=True) = 0 Then
                                        aBlk = New dxfBlock(defName, dxxDrawingDomains.Paper) With {.IsDefault = True}
                                    Else
                                        aBlk = New dxfBlock(defName, dxxDrawingDomains.Model) With {.IsDefault = True}
                                    End If
                                End If
                                aImage.Blocks.AddToCollection(aBlk, aImage:=aImage)
                            End If
                        Next
                        aBlocks = aImage.Blocks.CollectionObj
                        For Each aBlk In blocks
                            aHG.AssignTo(aBlk)
                            If Not table.TryGetEntry(aBlk.Name, entry) Then
                                entry = dxfTableEntry.CreateEntry(dxxReferenceTypes.BLOCK_RECORD, aBlk.Name)
                                addentries.Add(entry)
                            End If
                            If entry.Handle <> aBlk.BlockRecordHandle Then
                                entry.Handle = aBlk.BlockRecordHandle
                                aBlk.BlockRecord = New dxoBlockRecord(entry)

                            End If

                        Next
                    End If

                    If addentries.Count > 0 Then
                        For i = 1 To addentries.Count
                            aImage.RespondToTableEvent(table, dxxCollectionEventTypes.Add, addentries.Item(i - 1), False, False, aErr)
                        Next
                    End If
                Next

#Region "DEFAULT OBJECTS"
                'iImage.VerifyNamesDictionary(idx)
                'iImage.VerifyNamedDictionary(aImage.HandleGenerator, aDictionaryName:="ACAD_GROUP", bWDFlt:=False, aNamedDicIndex:=0)
                'aImage.HandleGenerator.AssignTo(iImage.obj_OBJECTS)
                aImage.Objex.VerifyNamedDictionary("ACAD_GROUP", aImage:=aImage)
                'aImage.Strukture = iImage
#End Region 'DEFAULT OBJECTS
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

        Friend Shared Function ValidateSettings(aImage As dxfImage, aSettings As TTABLEENTRY, aProp As TPROPERTY, aNewValue As Object, ByRef rError As String) As Boolean
            Dim _rVal As Boolean
            Dim sName As String = String.Empty
            rError = String.Empty
            _rVal = True
            If aImage Is Nothing Then Return _rVal
            '^catchs and halts invalid header variable values
            Try
                Select Case aSettings.EntryType
            '=================================================
                    Case dxxSettingTypes.TABLESETTINGS
                        '=================================================
                        sName = "TableSettings"
                        _rVal = ValidateSettings_TABLE(aImage, aSettings, aProp, aNewValue, rError)
            '=================================================
                    Case dxxSettingTypes.SYMBOLSETTINGS
                        '=================================================
                        sName = "SymbolSettings"
                        _rVal = ValidateSettings_SYMBOL(aImage, aSettings, aProp, aNewValue, rError)
            '=================================================
                    Case dxxSettingTypes.DIMOVERRIDES
                        '=================================================
                        If aSettings.IsGlobal Then
                            sName = "DimStyleOverrides"
                        Else
                            sName = "DimStyle[" & aSettings.PropValueStr(dxxDimStyleProperties.DIMNAME) & "]"
                            If aSettings.IsCopied Then sName = "*" & sName
                        End If
                        _rVal = ValidateSettings_DIMSTYLE(aImage, aSettings, aProp, aNewValue, rError)
                End Select
Done:
                If Not _rVal And rError <> "" Then
                    If rError <> "" Then Throw New Exception(rError)
                End If
            Catch ex As Exception
                aImage.HandleError("SetValue", sName, ex.Message)
            End Try
            Return _rVal
        End Function
        Private Shared Function ValidateSettings_DIMSTYLE(aImage As dxfImage, aSettings As TTABLEENTRY, aProp As TPROPERTY, aNewValue As Object, ByRef rError As String) As Boolean
            Dim _rVal As Boolean = True
            rError = String.Empty
            '^catchs and halts invalid dimstyle variable values
            Dim aArrow As dxfBlock
            Dim aEntry As dxfTableEntry
            Dim bEntry As dxfTableEntry
            Dim aStr As String
            Dim aLng As Integer
            Dim bInvalid As Boolean
            Dim eStr As String = String.Empty
            Dim minval As Object
            Dim maxval As Object
            Dim aName As String

            'Dim bNameChange As Boolean
            Dim bArrowChange As Boolean
            Dim styname As String
            Dim bDefArrow As Boolean
            styname = aSettings.PropValueStr(dxxDimStyleProperties.DIMNAME)
            If styname = "" Then Return _rVal
            aName = Trim(aProp.Name.ToUpper)
            If aName = "" Or aName = "$" Then
                _rVal = False
                Return _rVal
            End If
            If Left(aName, 1) = "$" Then aName = Right(aName, aName.Length - 1)
            aEntry = aImage.DimStyles.Item(styname)
            Select Case aName
         '-------------------------------------------------------------------------------
                Case "*DIMTXSTY_NAME"
                    '-------------------------------------------------------------------------------
                    aNewValue = aNewValue.Trim
                    If aNewValue = "" Then aNewValue = "Standard"
                    bEntry = aImage.Styles.Member(aNewValue)
                    bInvalid = bEntry.EntryType <> dxxReferenceTypes.STYLE
                    If bInvalid Then
                        eStr = $"Text Style '{ aNewValue }' Is Not Defined"
                    Else
                        aNewValue = bEntry.PropValueStr(dxxStyleProperties.NAME)
                        If bEntry.PropValueB(dxxStyleProperties.SHAPESTYLEFLAG) Then
                            eStr = $"Text Style '{ aNewValue }' Cannot Be Used For Dimensions"
                            bInvalid = True
                        End If
                    End If
                    If Not bInvalid Then
                        aSettings.PropValueSet(dxxDimStyleProperties.DIMTXSTY, bEntry.Name)
                    End If
         '-------------------------------------------------------------------------------
                Case "NAME"
                    '-------------------------------------------------------------------------------
                    aNewValue = aNewValue.Trim
                    bInvalid = aNewValue = ""
                    If Not bInvalid Then
                        If aSettings.IsGlobal Then
                            'overrides
                            bEntry = aImage.DimStyles.Item(aNewValue.ToString())
                            bInvalid = bEntry.EntryType <> dxxReferenceTypes.DIMSTYLE
                            If bInvalid Then
                                eStr = $"Invalid DimStyle Name Request For DimOverrides"
                            Else
                                aSettings.Props = New TPROPERTIES(bEntry.Properties)
                            End If
                        ElseIf aSettings.IsCopied Then
                            'dimensions and leader
                            bEntry = aImage.DimStyle(aNewValue.ToString())
                            bInvalid = bEntry.EntryType <> dxxReferenceTypes.DIMSTYLE
                            If bInvalid Then
                                eStr = $"Invalid DimStyle Name Request For Dimension Entity"
                            Else
                                aSettings.Props = New TPROPERTIES(bEntry.Properties)
                            End If
                        Else
                            'table members
                            bInvalid = String.Compare(aProp.Value, "Standard", True) = 0
                            If bInvalid Then
                                eStr = $"Standard Dim Style Cannot Be Rename"
                            Else
                                bEntry = aImage.DimStyle(aNewValue.ToString())
                                bInvalid = bEntry.EntryType <> dxxReferenceTypes.DIMSTYLE
                                If bInvalid Then
                                    eStr = $"Style Cannot Be Renamed To { aNewValue}' Because A Style With This Name Already Exists."
                                Else
                                    'bNameChange = True
                                End If
                            End If
                        End If
                    End If
         '-------------------------------------------------------------------------------
                Case "ACADVER"
         '-------------------------------------------------------------------------------
         '-------------------------------------------------------------------------------
                Case "LTSCALE", "TEXTSIZE", "DIMSCALE", "DIMLFAC", "DIMTXT"
                    '-------------------------------------------------------------------------------
                    eStr = $"Value Must Be Greater Than 0"
                    bInvalid = TVALUES.To_DBL(aNewValue) <= 0
         '-------------------------------------------------------------------------------
                Case "DIMASZ", "DIMTSZ", "DIMEXE", "DIMEXO", "DIMRND"
                    '-------------------------------------------------------------------------------
                    eStr = $"Value Must Be Greater Than or Equal to 0"
                    bInvalid = TVALUES.To_DBL(aNewValue) < 0
         '-------------------------------------------------------------------------------
                Case "DIMTMOVE"
                    '-------------------------------------------------------------------------------
                    minval = 0
                    maxval = 2
                    bInvalid = aNewValue < minval Or aNewValue > maxval
                    eStr = $"Value Must Be Greater Than or Equal to { minval } and Less Than or Equal to {maxval}"
         '-------------------------------------------------------------------------------
                Case "DIMATFIT", "DIMTAD", "DIMAUNIT"
                    '-------------------------------------------------------------------------------
                    minval = 0
                    maxval = 3
                    bInvalid = aNewValue < minval Or aNewValue > maxval
                    eStr = $"Value Must Be Greater Than or Equal to { minval } and Less Than or Equal to { maxval }"
         '-------------------------------------------------------------------------------
                Case "DIMJUST"
                    '-------------------------------------------------------------------------------
                    minval = 0
                    maxval = 4
                    bInvalid = aNewValue < minval Or aNewValue > maxval
                    eStr = $"Value Must Be Greater Than or Equal to { minval } and Less Than or Equal to { maxval }"
         '-------------------------------------------------------------------------------
                Case "DIMDEC", "DIMADEC"
                    '-------------------------------------------------------------------------------
                    minval = 0
                    maxval = 8
                    bInvalid = aNewValue < minval Or aNewValue > maxval
                    eStr = $"Value Must Be Greater Than or Equal to { minval } and Less Than or Equal to { maxval }"
         '-------------------------------------------------------------------------------
                Case "DIMCLRT", "DIMCLRD", "DIMCLRE"
                    '-------------------------------------------------------------------------------
                    bInvalid = aNewValue = dxxColors.Undefined
                    If Not bInvalid Then
                        aLng = TVALUES.To_INT(aNewValue)
                        If aLng < 0 Or aLng > 256 Then
                            aNewValue = dxfColors.NearestACLColor(dxfColors.Win32ToWin64(aLng)).ACLNumber
                        End If
                    End If
         '-------------------------------------------------------------------------------
                Case "*DIMLTYPE_NAME"
                    '-------------------------------------------------------------------------------
                    Dim aLt As dxoLinetype = Nothing
                    aNewValue = aNewValue.ToString().Trim()
                    If aNewValue = "" Then aNewValue = aEntry.Properties.ValueS(aName)
                    aNewValue = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, aNewValue.ToString(), rEntry:=aLt)
                    bInvalid = aLt Is Nothing
                    If bInvalid Then
                        eStr = $"Unrecognized Linetype Reference Detected"
                    Else
                        aSettings.PropValueSet(dxxDimStyleProperties.DIMLTYPE, aLt.Handle)
                    End If
         '-------------------------------------------------------------------------------
                Case "*DIMLTEX1_NAME"
                    '-------------------------------------------------------------------------------
                    Dim aLt As dxoLinetype = Nothing
                    aNewValue = aNewValue.ToString().Trim()
                    If aNewValue = "" Then aNewValue = aEntry.Properties.ValueS(aName)
                    aNewValue = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, aNewValue.ToString(), rEntry:=aLt)
                    bInvalid = aLt Is Nothing
                    If bInvalid Then
                        eStr = $"Unrecognized Linetype Reference Detected"
                    Else
                        aSettings.PropValueSet(dxxDimStyleProperties.DIMLTEX1, aLt.Handle)
                    End If
         '-------------------------------------------------------------------------------
                Case "*DIMLTEX2"
                    '-------------------------------------------------------------------------------
                    Dim aLt As dxoLinetype = Nothing
                    aNewValue = aNewValue.ToString().Trim()
                    If aNewValue = "" Then aNewValue = aEntry.Properties.ValueS(aName)
                    aNewValue = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, aNewValue.ToString(), rEntry:=aLt)
                    bInvalid = aLt Is Nothing
                    If bInvalid Then
                        eStr = $"Unrecognized Linetype Reference Detected"
                    Else
                        aSettings.PropValueSet(dxxDimStyleProperties.DIMLTEX2, aLt.Handle)
                    End If
         '-------------------------------------------------------------------------------
                Case "*DIMBLK_NAME", "*DIMBLK1_NAME", "*DIMBLK2_NAME", "*DIMLDRBLK_NAME"
                    '-------------------------------------------------------------------------------
                    aStr = aNewValue.ToString()
                    bArrowChange = True
                    bDefArrow = dxfArrowheads.IsDefault(aStr)
                    If bDefArrow Then aNewValue = aStr
                    If aStr <> "" Then
                        If aImage IsNot Nothing Then
                            aArrow = aImage.Blocks.GetByName(aStr, True)
                            bInvalid = aArrow Is Nothing
                            If bInvalid Then
                                eStr = $"Unknown Arrow Head Block(" & aStr & ") Requested"
                            Else
                                aNewValue = aArrow.Name
                                Select Case aName
                                    Case "*DIMBLK_NAME"
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, aArrow.Name)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK1, aArrow.BlockRecordHandle)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK2_NAME, aArrow.Name)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK2, aArrow.BlockRecordHandle)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, aArrow.Name)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMLDRBLK, aArrow.BlockRecordHandle)
                                    Case "*DIMBLK1_NAME"
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, aArrow.Name)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK1, aArrow.BlockRecordHandle)
                                    Case "*DIMBLK2_NAME"
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK2_NAME, aArrow.Name)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMBLK2, aArrow.BlockRecordHandle)
                                    Case "*DIMLDRBLK_NAME"
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, aArrow.Name)
                                        aSettings.PropValueSet(dxxDimStyleProperties.DIMLDRBLK, aArrow.BlockRecordHandle)
                                End Select
                            End If
                        End If
                    End If
            End Select
            _rVal = Not bInvalid
            If Not bInvalid Then
                aProp.LastValue = aProp.Value
                aProp.Value = aNewValue
                aSettings.Props.UpdateProperty = aProp
                If aProp.Name = "DIMZIN" Then aSettings.Props = TPROPERTIES.DecodeDIMZIN(aSettings.Props)
                If bArrowChange Then
                    aEntry.Properties.CopyValues(aSettings.Props)
                    dxfImageTool.ConfirmArrowHeads(aImage, aEntry)
                    aSettings.Props = New TPROPERTIES(aEntry.Properties)
                End If
            Else
                If eStr <> "" Then
                    rError = $"An Attempt Was Made To Change Property '{aName }' To An Invalid Value Of '{ aNewValue }'"
                    rError += $"( { eStr })"
                End If
            End If
            Return _rVal
        End Function
        Private Shared Function ValidateSettings_SYMBOL(aImage As dxfImage, aSettings As TTABLEENTRY, aProp As TPROPERTY, ByRef ioNewValue As Object, ByRef rError As String) As Boolean
            rError = String.Empty
            '^catchs and halts invalid table variable values
            Dim aName As String
            Dim bInvalid As Boolean
            aName = Trim(aProp.Name.ToUpper)
            Select Case aName.Trim.ToUpper()
                Case "LAYERNAME", "LAYERCOLOR"
                    If aSettings.IsCopied Then
                        bInvalid = True
                    End If
                Case "ARROWSTYLE"
                    If aProp.Value < 0 Or aProp.Value > dxxArrowStyles.StraightFullOpen Then
                        bInvalid = True
                    End If
                Case "ARROWHEAD"
                    If aProp.Value < dxxArrowHeadTypes.ClosedFilled Or aProp.Value > dxxArrowHeadTypes.None Then
                        bInvalid = True
                    End If
                Case "TEXTSTYLE"
                    If Not String.IsNullOrWhiteSpace(ioNewValue.ToString()) Then
                        If aSettings.IsCopied Then
                            bInvalid = aImage.Styles.ContainsEntry(ioNewValue.ToString())
                            If Not bInvalid Then
                                ioNewValue = aImage.TextStyle(ioNewValue.ToString()).Name
                            Else
                                rError = $"Unknow Style Requested ' { ioNewValue }' For Property '{ aName }'"
                            End If
                        End If
                    Else
                        If Not aImage.Styles.ContainsEntry(ioNewValue.ToString()) Then
                            ioNewValue = aImage.GetOrAdd(dxxReferenceTypes.STYLE, ioNewValue.ToString())
                        End If
                    End If
            End Select
            Return Not bInvalid
        End Function
        Private Shared Function ValidateSettings_TABLE(aImage As dxfImage, aSettings As TTABLEENTRY, aProp As TPROPERTY, ByRef ioNewValue As Object, ByRef rError As String) As Boolean
            rError = String.Empty
            '^catchs and halts invalid table variable values
            Dim aName As String = aProp.Name.ToUpper().Trim()
            Dim bInvalid As Boolean

            Select Case aName
                Case "LAYERNAME", "LAYERCOLOR"
                    If aSettings.IsCopied Then
                        bInvalid = True
                    End If
                Case "TEXTSTYLENAME", "TITLETEXTSTYLE", "FOOTERTEXTSTYLE", "HEADERTEXTSTYLE"
                    If ioNewValue.ToString() <> "" Then
                        If aSettings.IsCopied Then
                            bInvalid = Not aImage.TextStyles.ContainsEntry(ioNewValue.ToString())
                            If Not bInvalid Then
                                ioNewValue = aImage.TextStyle(ioNewValue.ToString()).Name
                            Else
                                rError = $"Unknown Style Requested '{ ioNewValue }' For Property '{ aName }'"
                            End If
                        End If
                    End If
            End Select
            Return Not bInvalid
        End Function

        Friend Shared Function GroupAdd(aImage As dxfImage, aGroupName As String, aDescription As String) As dxfoGroup
            Dim rIndex As Integer = 0
            If String.IsNullOrWhiteSpace(aGroupName) Then Return Nothing Else aGroupName = aGroupName.Trim
            Dim aMem As dxfoGroup = aImage.Objex.GetObject(aGroupName, dxxObjectTypes.Group)
            If aMem IsNot Nothing Then rIndex = aMem.Index
            Dim idx As Integer
            Dim aGroupsDic As dxfoDictionary = aImage.GroupNamesDictionary(idx)
            If rIndex <= 0 Then
                aMem = New dxfoGroup(aGroupName)
                aImage.HandleGenerator.AssignTo(aMem)
                aGroupsDic.AddEntry(aMem.Name, aMem.Handle)
                aMem.ReactorGUID = aGroupsDic.GUID
                If Not aImage.Objex.Add(aMem, True, aImage) Then Return Nothing
                rIndex = aImage.Objex.Count
            End If
            aMem.Description = aDescription
            aImage.Objex.SetItem(rIndex, aMem)
            Return aMem
        End Function
        Friend Shared Function GroupGet(aImage As dxfImage, aGroupName As String, bLoadIfNotFound As Boolean, Optional aImageObj As dxfImage = Nothing) As dxfoGroup
            Dim _rVal As dxfoGroup = Nothing
            Dim rIndex As Integer = -1
            If String.IsNullOrWhiteSpace(aGroupName) Then Return Nothing
            aGroupName = aGroupName.Trim()
            _rVal = aImage.Objex.GetObject(aGroupName, dxxObjectTypes.Group)
            If _rVal IsNot Nothing Then rIndex = _rVal.Index
            If bLoadIfNotFound And rIndex < 0 Then
                _rVal = dxfImageTool.GroupAdd(aImage, aGroupName, "")
            End If
            Return _rVal
        End Function

        Friend Shared Function LabelPathVectors(aImage As dxfImage, Optional aEntity As dxfEntity = Nothing, Optional aTextScaleFactor As Double = 0.0, Optional aColor As dxxColors = dxxColors.Blue, Optional bSaveToFile As Boolean = False) As Long
            Dim _rVal As Long = 0
            '#1the subject entity
            '#2a scale factor to apply to the labels text height
            '#3a color to apply to the labels
            '#4flag to mark the labels to be saved to file if the image is saved
            '^labels the vectors that define the screen path of the subject entity
            'On Error Resume Next
            If aImage Is Nothing Then Return _rVal
            If aEntity Is Nothing Then aEntity = aImage.Entities.LastEntity
            If aEntity Is Nothing Then Return _rVal
            Dim v1 As TVECTOR
            Dim i As Integer
            Dim tht As Double
            Dim P1 As dxfVector
            Dim aTxt As dxeText
            Dim txt As String
            Dim eTool As dxoEntityTool
            Dim aPl As New TPLANE("")
            Dim pVecs As TVECTORS
            aEntity.UpdatePath(False, aImage)
            pVecs = CType(aEntity.Paths, TVECTORS)
            aPl = aEntity.Plane.Strukture
            eTool = aImage.EntityTool
            tht = aImage.obj_DISPLAY.pln_VIEW.Height * 0.015
            If aTextScaleFactor > 0 Then tht *= aTextScaleFactor
            P1 = New dxfVector
            For i = 1 To pVecs.Count
                v1 = pVecs.Item(i)
                txt = i
                Select Case v1.Code
                    Case dxfGlobals.PT_PIXELTO ' pixel vector
                        txt += "\PPIXELTO"
                    Case dxfGlobals.PT_LINETO ' Straight line segment
                        txt += "\PLINETO"
                    Case dxfGlobals.PT_BEZIERTO ' Curve segment
                        txt += "\PBEZIERTO"
                    Case dxfGlobals.PT_MOVETO ' Move current drawing vector
                        txt += "\PMOVETO"
                End Select
                P1.Strukture = v1
                aTxt = eTool.Create_Text(P1, "\fTxt.shx;" & txt, tht, dxxMTextAlignments.MiddleCenter, aColor:=aColor, bSuppressEffects:=True, bSuppressUCS:=True, bSuppressElevation:=True)
                aTxt.AlignmentPt1V = v1
                aTxt.PlaneV = aPl
                aTxt = aImage.Entities.Add(aTxt)
                aTxt.DrawingDirection = dxxTextDrawingDirections.Horizontal
                aTxt.SaveToFile = bSaveToFile
            Next i
            _rVal = pVecs.Count
            Return _rVal
        End Function

        Friend Shared Function SelectionSet(aImage As dxfImage, Optional aSelectionType As dxxSelectionTypes = dxxSelectionTypes.CurrentSet, Optional aSelectionCriteria As Object = Nothing, Optional aSelectType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aGetClones As Boolean = False, Optional aRemove As Boolean = False) As colDXFEntities

            '#1the subject image
            '#2the type of selection set to return
            '#3an optional value to use in conjuction with the selection type
            '#4an entity type filter value
            '#5flag to return clones of the set rather than the actual enities
            '#6flag to remove the set from the current image
            '^provides extended methods for obtaining subsets of the images current entities collection
            'On Error Resume Next
            If aImage Is Nothing Then

                Return New colDXFEntities
            End If
            Dim _rVal As New colDXFEntities() With {.MaintainIndices = False}
            Dim wuz As Boolean = aImage.AutoRedraw
            aImage.AutoRedraw = False
            Select Case aSelectionType
                Case dxxSelectionTypes.CurrentSet
                    _rVal = aImage.Entities.SubSet(aImage.SelectionStartID, aImage.Entities.Count, bReturnClones:=aGetClones, bRemove:=aRemove)
                Case dxxSelectionTypes.Type
                    _rVal = aImage.Entities.GetByType(aSelectType, aGetClones, aRemove)
                Case dxxSelectionTypes.SelectAll
                    _rVal = aImage.Entities.SubSet(1, aImage.Entities.Count, bReturnClones:=aGetClones, bRemove:=aRemove)
                Case dxxSelectionTypes.Color
                    If aSelectionCriteria Is Nothing Then Return _rVal
                    _rVal = aImage.Entities.GetByDisplayVariableValue(dxxDisplayProperties.Color, TVALUES.To_INT(aSelectionCriteria), bReturnClones:=aGetClones, bRemove:=aRemove)
                Case dxxSelectionTypes.Layer
                    If aSelectionCriteria Is Nothing Then Return _rVal
                    _rVal = aImage.Entities.GetByDisplayVariableValue(dxxDisplayProperties.LayerName, aSelectionCriteria.ToString(), bReturnClones:=aGetClones, bRemove:=aRemove)
                Case dxxSelectionTypes.Linetype
                    If aSelectionCriteria Is Nothing Then Return _rVal
                    _rVal = aImage.Entities.GetByDisplayVariableValue(dxxDisplayProperties.Linetype, aSelectionCriteria.ToString(), bReturnClones:=aGetClones, bRemove:=aRemove)
                Case dxxSelectionTypes.dxfSelectDimsAndLeaders
                    _rVal = aImage.Entities.GetDimensionEntities(bReturnClones:=aGetClones, bRemove:=aRemove)
                Case Else
                    _rVal = New colDXFEntities
            End Select
            aImage.AutoRedraw = wuz
            Return _rVal
        End Function

        Friend Shared Function Purge(aImage As dxfImage, Optional bAll As Boolean = False, Optional bBlocks As Boolean = False, Optional bLayers As Boolean = False, Optional bLineTypes As Boolean = False, Optional bStyles As Boolean = False, Optional bDimStyles As Boolean = False) As Long
            If aImage Is Nothing Then Return 0
            Dim _rVal As Long = 0
            '#1the subject image
            '#2Flag to purge all
            '#3Flag to purge blocks
            '#4Flag to purge layers
            '#5Flag to purge linetypes
            '#6Flag to purge styles
            '#7Flag to purge dimstyles
            '^removes unreferenced objects from the image

            Dim aTable As dxfTable

            Dim aBlks As List(Of dxfBlock)
            Dim aBlk As dxfBlock
            Dim aNewBlocks As New List(Of dxfBlock)

            Dim cnt As Integer

            Dim bKeep As Boolean
            Dim rErr As String = String.Empty

            '============================ BLOCKS ======================================
            If bBlocks Or bAll Then
                aBlks = aImage.Blocks.CollectionObj
                cnt = 0
                aTable = aImage.DimStyles
                For Each aBlk In aBlks
                    If aBlk Is Nothing Then Continue For
                    Dim idx As Integer = aNewBlocks.FindIndex(Function(x) String.Compare(x.Name, aBlk.Name, True) = 0)
                    If idx >= 0 Then
                        Continue For
                    End If
                    bKeep = String.Compare(aBlk.Name, "_ClosedFilled", True) = 0
                    If Not bKeep Then
                        bKeep = aBlk.IsDefault
                    End If
                    If Not bKeep Then
                        bKeep = aImage.Blocks.ReferencesBlock(aBlk.Name, True)
                    End If
                    If Not bKeep Then
                        bKeep = aImage.Entities.ReferencesBlock(aBlk.Name, True)
                    End If

                    If Not bKeep Then
                        For Each entry As dxfTableEntry In aTable


                            Dim aProp As dxoProperty = entry.Properties.Member(341)
                            If aProp Is Nothing Then Continue For

                            Dim k As Integer = entry.Properties.IndexOf(aProp)
                            Dim sval As String = aProp.ValueS
                            If String.Compare(sval, aBlk.Handle, True) = 0 Then
                                bKeep = True
                                Exit For
                            End If
                            If entry.Properties.Item(k + 1).ValueS = aBlk.Handle Then
                                bKeep = True
                                Exit For
                            End If

                            If entry.Properties.Item(k + 2).ValueS = aBlk.Handle Then
                                bKeep = True
                                Exit For
                            End If

                        Next

                    End If

                    If bKeep Then
                        aNewBlocks.Add(aBlk)
                    End If
                Next
                If aNewBlocks.Count < aImage.Blocks.Count Then
                    _rVal += (aImage.Blocks.Count - aNewBlocks.Count)
                    aImage.Blocks.Populate(aNewBlocks, bAddClones:=False, bSuppressEvents:=True)
                End If
            End If

            'On Error Resume Next
            '============================ LAYERS ======================================
            If bLayers Or bAll Then
                aTable = aImage.Table(dxxReferenceTypes.LAYER)
                cnt = aTable.RemoveAll(Function(x) aImage.ReferenceCanBeDeleted(dxxReferenceTypes.LAYER, x.Handle, True, rErr))

            End If
            '============================ LINETYPES ======================================
            If bLineTypes Or bAll Then
                aTable = aImage.Table(dxxReferenceTypes.LTYPE)
                cnt = aTable.RemoveAll(Function(x) aImage.ReferenceCanBeDeleted(dxxReferenceTypes.LTYPE, x.Handle, True, rErr))

            End If
            '============================ DIMSTYLES ======================================
            If bDimStyles Or bAll Then
                aTable = aImage.Table(dxxReferenceTypes.DIMSTYLE)
                cnt = aTable.RemoveAll(Function(x) aImage.ReferenceCanBeDeleted(dxxReferenceTypes.DIMSTYLE, x.Handle, True, rErr))
            End If
            '============================ STYLES ======================================
            If bStyles Or bAll Then
                aTable = aImage.Table(dxxReferenceTypes.STYLE)
                cnt = aTable.RemoveAll(Function(x) aImage.ReferenceCanBeDeleted(dxxReferenceTypes.DIMSTYLE, x.Handle, True, rErr))

            End If

            Return _rVal
        End Function

        Friend Shared Function SetAttributes(aImage As dxfImage, aAttributesCol As TPROPERTYARRAY) As Boolean
            '#1the subject image
            '#2a collection of the dxfAttributes objects
            '^returns a collection of the dxfAttributes of all the insert objects in the image
            Dim _rVal As Boolean
            If aImage Is Nothing Then Return False
            If aAttributesCol.Count <= 0 Then Return False
            Try
                Dim eInserts As List(Of dxeInsert) = aImage.Entities.Inserts
                If eInserts.Count <= 0 Then Return _rVal
                Dim aAtts As TPROPERTIES
                Dim aI As dxeInsert
                For i As Integer = 1 To eInserts.Count
                    aI = eInserts.Item(i - 1)
                    For j As Integer = 1 To aAttributesCol.Count
                        aAtts = aAttributesCol.Item(j)
                        If aAtts.Owner = aI.GUID Then
                            aI.SetAttributes(aAtts)
                            If aI.IsDirty Then _rVal = True
                            Exit For
                        End If
                    Next j
                Next i
            Catch ex As Exception
                Throw New Exception("Invalid Attribute Collection Passed")
            End Try
            Return _rVal
        End Function

        Friend Shared Function SetTextStyleProperty(aImage As dxfImage, aStyleName As String, aProperty As String, aValue As Object) As Boolean
            Dim _rVal As Boolean
            '#1the subject image
            '#2the style to set a property for
            '#3the name of the property to set
            '#4the new value for the property
            '^used to set the text height,font name or width factor of a text style
            If aImage Is Nothing Then Return _rVal
            Try
                Dim pname As String
                Dim aSng As Double
                Dim bUpdateEm As Boolean
                Dim aTable As dxfTable = aImage.Table(dxxReferenceTypes.STYLE)
                If String.IsNullOrWhiteSpace(aStyleName) Then aStyleName = aImage.Header.TextStyleName
                aStyleName = aStyleName.Trim()
                If Not aTable.ContainsEntry(aStyleName) Then
                    Throw New Exception($"Text Style '{ aStyleName }' Does Not Exist")
                End If
                Dim aStyle As dxoStyle = DirectCast(aTable.Item(aStyleName), dxoStyle)
                pname = aProperty.ToUpper().Trim()
                If pname = "TEXT HEIGHT" Or pname = "TEXTHEIGHT" Or pname = "TEXT SIZE" Or pname = "TEXTSIZE" Then
                    If Not TVALUES.IsNumber(aValue) Then Throw New Exception("Numeric Input Required")
                    aSng = TVALUES.To_DBL(aValue)
                    If aSng < 0 Then Throw New Exception("Text Height Must Be Greater Than 0")
                    aStyle.Properties.SetVal(40, aSng)
                ElseIf pname = "WIDTH FACTOR" Or pname = "WIDTHFACTOR" Then
                    If Not TVALUES.IsNumber(aValue) Then Throw New Exception("Numeric Input Required")
                    aSng = TVALUES.To_DBL(aValue)
                    If aSng < 0.01 Or aSng > 100 Then Throw New Exception("Width Factor Must be >= 0.01 and <= 1000")
                    aStyle.Properties.SetVal(41, aSng)
                ElseIf pname = "FONT NAME" Or pname = "FONTNAME" Then
                    bUpdateEm = aStyle.UpdateFontName(aValue.ToString(), "")
                Else
                    Throw New Exception("'Text Height) ', 'Width Factor' and 'Font Name' Are The Only Properties That Can Be Set Using This Function")
                End If

            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), "dxfImageTool", ex)
            End Try
            Return _rVal
        End Function
        Friend Shared Function TransferBlockToImage(aToImage As dxfImage, aFromImage As dxfImage, aBlockName As String, Optional bOverwriteExistingReferences As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If aToImage Is Nothing Or aFromImage Is Nothing Then Return _rVal
            Dim blocks As List(Of dxfBlock) = aFromImage.Blocks.GetBlockAndSubBlocks(aFromImage, aBlockName, bReturnClones:=True)
            If blocks.Count <= 0 Then Return False


            Dim aRef As dxfTableEntry = Nothing
            Dim bRef As dxfTableEntry = Nothing
            Dim aEntry As dxfTableEntry = Nothing
            Dim transferedLAYERS As New List(Of String)
            Dim transferedSTYLES As New List(Of String)
            Dim transferedLTYPES As New List(Of String)
            Dim transferedDIMSTYLES As New List(Of String)

            Dim RefLists(0 To 4) As String

            _rVal = True
            For Each block As dxfBlock In blocks

                Dim rName As String = block.LayerName
                aToImage.HandleGenerator.AssignTo(block)
                If Not String.IsNullOrWhiteSpace(rName) Then
                    If Not transferedLAYERS.Contains(rName.ToUpper) Then

                        Dim bTbl As dxfTable = aFromImage.Table(dxxReferenceTypes.LAYER)
                        If bTbl.TryGetEntry(rName, bRef) Then
                            Dim aTbl As dxfTable = aToImage.Table(dxxReferenceTypes.LAYER)
                            If Not aTbl.TryGetEntry(rName, aRef) Then
                                aRef = bRef.Clone()
                                aToImage.HandleGenerator.AssignTo(aRef)
                                aTbl.Add(aRef)
                            Else
                                If bOverwriteExistingReferences Then
                                    aRef.Properties.CopyValues(bRef.Properties, True, "5,100,2")
                                End If
                            End If

                            transferedLAYERS.Add(rName.ToUpper())
                        End If
                    End If
                End If


                For Each ent As dxfEntity In block.Entities
                    aToImage.HandleGenerator.AssignTo(ent)
                    rName = ent.LayerName
                    If Not String.IsNullOrWhiteSpace(rName) Then

                        If Not transferedLAYERS.Contains(rName.ToUpper) Then

                            Dim bTbl As dxfTable = aFromImage.Table(dxxReferenceTypes.LAYER)
                            If bTbl.TryGetEntry(rName, bRef) Then
                                Dim aTbl As dxfTable = aToImage.Table(dxxReferenceTypes.LAYER)
                                If Not aTbl.TryGetEntry(rName, aRef) Then
                                    aRef = bRef.Clone()
                                    aToImage.HandleGenerator.AssignTo(aRef)
                                    aTbl.Add(aRef)
                                Else
                                    If bOverwriteExistingReferences Then
                                        aRef.Properties.CopyValues(bRef.Properties, True, "5,100,2")
                                    End If
                                End If

                                transferedLAYERS.Add(rName.ToUpper())
                            End If
                        End If
                    End If


                    rName = ent.Linetype
                    If Not String.IsNullOrWhiteSpace(rName) Then

                        If Not transferedLTYPES.Contains(rName.ToUpper) Then

                            Dim bTbl As dxfTable = aFromImage.Table(dxxReferenceTypes.LTYPE)
                            If bTbl.TryGetEntry(rName, bRef) Then
                                Dim aTbl As dxfTable = aToImage.Table(dxxReferenceTypes.LTYPE)
                                If Not aTbl.TryGetEntry(rName, aRef) Then
                                    aRef = bRef.Clone()
                                    aToImage.HandleGenerator.AssignTo(aRef)
                                    aTbl.Add(aRef)
                                Else
                                    If bOverwriteExistingReferences Then
                                        aRef.Properties.CopyValues(bRef.Properties, True, "5,100,2")
                                    End If
                                End If

                                transferedLTYPES.Add(rName.ToUpper())
                            End If
                        End If
                    End If
                    rName = ent.TextStyleName
                    If Not String.IsNullOrWhiteSpace(rName) Then

                        If Not transferedSTYLES.Contains(rName.ToUpper) Then

                            Dim bTbl As dxfTable = aFromImage.Table(dxxReferenceTypes.STYLE)
                            If bTbl.TryGetEntry(rName, bRef) Then
                                Dim aTbl As dxfTable = aToImage.Table(dxxReferenceTypes.STYLE)
                                If Not aTbl.TryGetEntry(rName, aRef) Then
                                    aRef = bRef.Clone()
                                    aToImage.HandleGenerator.AssignTo(aRef)
                                    aTbl.Add(aRef)
                                Else
                                    If bOverwriteExistingReferences Then
                                        aRef.Properties.CopyValues(bRef.Properties, True, "5,100,2")
                                    End If
                                End If

                                transferedSTYLES.Add(rName.ToUpper())
                            End If
                        End If
                    End If
                    rName = ent.DimStyleName
                    If Not String.IsNullOrWhiteSpace(rName) Then

                        If Not transferedDIMSTYLES.Contains(rName.ToUpper) Then

                            Dim bTbl As dxfTable = aFromImage.Table(dxxReferenceTypes.DIMSTYLE)
                            If bTbl.TryGetEntry(rName, bRef) Then
                                Dim aTbl As dxfTable = aToImage.Table(dxxReferenceTypes.DIMSTYLE)
                                If Not aTbl.TryGetEntry(rName, aRef) Then
                                    aRef = bRef.Clone()
                                    aToImage.HandleGenerator.AssignTo(aRef)
                                    aTbl.Add(aRef)
                                Else
                                    If bOverwriteExistingReferences Then
                                        aRef.Properties.CopyValues(bRef.Properties, True, "105,100,2")
                                    End If
                                End If

                                transferedDIMSTYLES.Add(rName.ToUpper())
                            End If
                        End If
                    End If
                Next  'ent
                aToImage.Blocks.Add(block, True)
            Next 'block
            Return _rVal
        End Function
        Friend Shared Sub TransferPolygon(aImage As dxfImage, aPolygon As dxePolygon, Optional aBlockName As String = "", Optional aLayer As String = "*", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "*", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bSuppressAddSegs As Boolean? = Nothing)
            If aImage Is Nothing Or aPolygon Is Nothing Then Return
            '#1 then subject image
            '#2the dxePolygon object to transfer
            '#3the layer name to assign to the new polygon. if a null string is passed the layer name of the passed polygon is retained. if '*' is passed the new polygon is put on the current layer
            '#4the color to assign to the new polygon. if dxxColors.Undefined is passed the new polygon is assigned the current color. if dxxColors.ByBlock is passed the new polygons color is retained.
            '#5the linetype name to assign to the new polygon. if a null string is passed the linetype name of the passed polygon is retained. if '*' is passed the new polygon is assigned the current linetype
            '#6flag to return the primitives of the polygon
            '#7flag to define the new polygon with respect to the current UCS
            '^used to copy the passed polygon to the images UCS and apply the images current settings
            Dim verts As colDXFVectors
            Dim aPlane As dxfPlane = Nothing
            Dim aEnts As colDXFEntities
            Dim aEnt As dxfEntity
            Dim iheader As dxsHeader = aImage.Header
            Dim aLtLs As dxsLinetypes = aImage.LinetypeLayers

            Try
                If aLTLSetting < 0 Or aLTLSetting > 2 Then aLTLSetting = aLtLs.Setting
                If String.IsNullOrWhiteSpace(aLayer) Then aLayer = String.Empty Else aLayer = aLayer.Trim()
                If String.IsNullOrWhiteSpace(aLineType) Then aLineType = String.Empty Else aLineType = aLineType.Trim()
                If String.IsNullOrWhiteSpace(aBlockName) Then aBlockName = String.Empty Else aBlockName = aBlockName.Trim()

                If aLayer = "*" Then
                    aLayer = iheader.LayerName
                ElseIf aLayer = String.Empty Then
                    aLayer = aPolygon.LayerName
                End If
                If aLineType = "*" Then
                    aLineType = iheader.Linetype
                ElseIf aLineType = String.Empty Then
                    aLineType = aPolygon.Linetype
                End If
                If aColor = dxxColors.Undefined Then
                    aColor = iheader.Color
                ElseIf aColor = dxxColors.ByBlock Then
                    aColor = aPolygon.Color
                End If
                If aLayer = String.Empty Then aLayer = iheader.LayerName
                aLayer = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aLayer)
                Dim iGUID As String = aImage.GUID
                Dim gname As String = aPolygon.GroupName
                If gname = String.Empty Then gname = aImage.GroupName
                aPolygon.IsDirty = True
                Dim pGUID As String = aPolygon.GUID
                aPolygon.GroupName = gname
                If aBlockName <> String.Empty Then aPolygon.SetPropVal("*BlockName", aBlockName)
                Dim pDsp As TDISPLAYVARS = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLSetting)
                aPolygon.DisplayStructure = pDsp
                If bSuppressAddSegs.HasValue Then
                    aPolygon.SuppressAdditionalSegments = bSuppressAddSegs.Value
                End If
                Dim aDsp As TDISPLAYVARS = aPolygon.DisplayStructure
                aPolygon.InsertionPt = aImage.CreateVector(aPolygon.InsertionPt, bSuppressUCS, bSuppressElevation, aPlane)
                Dim aPl As TPLANE = New TPLANE(aPlane, New dxfVector(aPolygon.InsertionPtV))

                aPolygon.PlaneV = aPl
                aPolygon.ImageGUID = iGUID
                Dim vts As TVERTICES = aPolygon.VerticesV.WithRespectToPlane(aPolygon.PlaneV)
                For j As Integer = 1 To vts.Count
                    Dim vt As TVERTEX = vts.Item(j)
                    vt.LTScale = pDsp.LTScale
                    If String.Compare(aDsp.LayerName, vt.LayerName, True) = 0 Or vt.LayerName = String.Empty Then
                        vt.LayerName = pDsp.LayerName
                    End If
                    If String.Compare(aDsp.Linetype, vt.Linetype, True) = 0 Or vt.Linetype = String.Empty Or String.Compare(dxfLinetypes.ByBlock, vt.Linetype, True) = 0 Then
                        vt.Linetype = pDsp.Linetype
                    End If
                    If (aDsp.Color = vt.Color) Or vt.Color = dxxColors.Undefined Or vt.Color = dxxColors.ByBlock Then
                        vt.Color = pDsp.Color
                    End If
                    If aLTLSetting = dxxLinetypeLayerFlag.ForceToColor Or aLTLSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                        aLtLs.Apply(aLTLSetting, aImage, vt.Linetype, vt.LayerName, vt.Color)
                    End If
                    vt.Vector = aPl.Vector(vt.Vector.X, vt.Vector.Y)
                    vts.SetItem(j, vt)
                Next j
                aPolygon.VerticesV = vts
                'addition segments
                aEnts = aPolygon.AdditionalSegments
                If aEnts IsNot Nothing Then
                    For i As Integer = 1 To aEnts.Count
                        aEnt = aEnts.Item(i)
                        Dim aFlg As Boolean = False
                        Dim bDsp As TDISPLAYVARS = aEnt.DisplayStructure
                        Dim cDsp As New TDISPLAYVARS(bDsp)
                        If String.Compare(aDsp.LayerName, bDsp.LayerName, True) = 0 Or bDsp.LayerName = String.Empty Then
                            bDsp.LayerName = pDsp.LayerName
                        End If
                        If String.Compare(aDsp.Linetype, bDsp.Linetype, True) = 0 Or String.Compare(bDsp.Linetype, "byBlock", True) = 0 Or bDsp.Linetype = String.Empty Then
                            bDsp.Linetype = pDsp.Linetype
                        End If
                        If (aDsp.Color = bDsp.Color) Or bDsp.Color = dxxColors.Undefined Or bDsp.Color = dxxColors.ByBlock Then
                            bDsp.Color = pDsp.Color
                        End If
                        bDsp = dxfImageTool.DisplayStructure(aImage, bDsp.LayerName, bDsp.Color, bDsp.Linetype, dxxLinetypeLayerFlag.Suppressed)
                        If aLTLSetting = dxxLinetypeLayerFlag.ForceToColor Or aLTLSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                            If aLtLs.Apply(aLTLSetting, aImage, bDsp.Linetype, bDsp.LayerName, bDsp.Color) Then aFlg = True
                        End If
                        If String.Compare(bDsp.LayerName, cDsp.LayerName, True) <> 0 Then aFlg = True
                        If String.Compare(bDsp.Linetype, cDsp.Linetype, True) <> 0 Then aFlg = True
                        If bDsp.Color <> cDsp.Color Then aFlg = True
                        If aFlg Then
                            aEnt.DisplayStructure = bDsp
                        End If
                        verts = aEnt.DefiningVectors
                        vts = New TVERTICES(verts).WithRespectToPlane(aPolygon.PlaneV)
                        For j As Integer = 1 To vts.Count
                            vts.SetVector(aPl.Vector(vts.X(j), vts.Y(j)), j)
                        Next j
                        verts.Populate(vts)
                        aEnt.IsDirty = True
                        aEnt.OwnerGUID = pGUID
                        aEnt.ImageGUID = iGUID
                    Next i
                End If
                aPolygon.Instances.Copy(aPolygon.Instances, True)
            Catch ex As Exception
                Throw New Exception($"img_TransferPolygon - { ex.Message}")
            End Try
        End Sub
    End Class

End Namespace