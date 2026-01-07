Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfArrowheads

        Public Shared ReadOnly Property ArrowHeadNames As String = "_ClosedFilled,_ClosedBlank,_Closed,_Dot,_ArchTick,_Oblique,_Open,_Origin,_Origin2,_Open90,_Open30,_DotSmall,_Small,_DotBlank,_BoxBlank,_BoxFilled,_DatumBlank,_DatumFilled,_Integral,_None"

#Region "Shared Methods"
        Friend Shared Function Align(aPlane As TPLANE, aAligmentEntity As dxfEntity, aArrowSize As Double, ByRef rArrowPoint As TVECTOR, ByRef rSuppress As Boolean, ByRef rInvalid As Boolean, ByRef rArcLine As TSEGMENT) As Double
            Dim _rVal As Double
            'On Error Resume Next
            rArrowPoint = New TVECTOR(aPlane.Origin.X, aPlane.Origin.Y, aPlane.Origin.Z)
            rArcLine = aAligmentEntity.ALSStructure(rInvalid, True, rSuppress)
            If rInvalid Then
                rSuppress = True
                Return _rVal
            End If
            Dim aLn As TLINE = rArcLine.LineStructure
            Dim aX As TVECTOR = aPlane.XDirection * -1
            If Not rArcLine.IsArc Then
                rArrowPoint = aLn.SPT
                Dim d2 As TVECTOR = aLn.SPT.DirectionTo(aLn.EPT)
                _rVal = aX.AngleTo(d2, aPlane.ZDirection)
            Else
                Dim aAr As TARC = rArcLine.ArcStructure
                rArrowPoint = aAr.StartPt
                If aArrowSize <> 0 Then
                    Dim aPts As TVECTORS
                    aPts = aAr.IntersectionPts(New TARC(aAr.Plane, rArrowPoint, Math.Abs(aArrowSize)))
                    If aPts.Count > 0 Then
                        aLn.SPT = rArrowPoint
                        aLn.EPT = aPts.Nearest(aAr.EndPt, Nothing)
                        _rVal = aX.AngleTo(aLn.SPT.DirectionTo(aLn.EPT), aPlane.ZDirection)
                    End If
                End If
            End If
            Return _rVal
        End Function

        Friend Shared Function GetBlockType(aArrowName As String) As dxxArrowHeadTypes
            aArrowName = Trim(UCase(aArrowName))
            If Left(aArrowName, 1) <> "_" Then aArrowName = "_" & aArrowName
            Select Case aArrowName.ToUpper
                Case "_ClosedFilled".ToUpper
                    Return dxxArrowHeadTypes.ClosedFilled
                Case "_ClosedBlank".ToUpper
                    Return dxxArrowHeadTypes.ClosedBlank
                Case "_Closed".ToUpper
                    Return dxxArrowHeadTypes.Closed
                Case "_Dot".ToUpper
                    Return dxxArrowHeadTypes.Dot
                Case "_ArchTick".ToUpper
                    Return dxxArrowHeadTypes.ArchTick
                Case "_Oblique".ToUpper
                    Return dxxArrowHeadTypes.Oblique
                Case "_Open".ToUpper
                    Return dxxArrowHeadTypes.Open
                Case "_Origin".ToUpper
                    Return dxxArrowHeadTypes.Origin
                Case "_Origin2".ToUpper
                    Return dxxArrowHeadTypes.Origin2
                Case "_Open90".ToUpper
                    Return dxxArrowHeadTypes.Open90
                Case "_Open30".ToUpper
                    Return dxxArrowHeadTypes.Open30
                Case "_DotSmall".ToUpper
                    Return dxxArrowHeadTypes.DotSmall
                Case "_Small".ToUpper
                    Return dxxArrowHeadTypes.Small
                Case "_DotBlank".ToUpper
                    Return dxxArrowHeadTypes.DotBlank
                Case "_BoxBlank".ToUpper
                    Return dxxArrowHeadTypes.BoxBlank
                Case "_BoxFilled".ToUpper
                    Return dxxArrowHeadTypes.BoxFilled
                Case "_DatumBlank".ToUpper
                    Return dxxArrowHeadTypes.DatumBlank
                Case "_DatumFilled".ToUpper
                    Return dxxArrowHeadTypes.DatumFilled
                Case "_Integral".ToUpper
                    Return dxxArrowHeadTypes.Integral
                Case "_None".ToUpper
                    Return dxxArrowHeadTypes.None
                Case Else
                    Return dxxArrowHeadTypes.UserDefined
            End Select
        End Function
        Friend Shared Function GetBlockName(aArrowType As dxxArrowHeadTypes)
            If dxfEnums.Validate(GetType(dxxArrowHeadTypes), aArrowType, bSkipNegatives:=True) Then
                Return dxfEnums.Description(aArrowType)
            Else
                Return "_ClosedFilled"
            End If
        End Function
        Friend Shared Function GetEntity(aDStyle As TTABLEENTRY, aPlane As TPLANE, aImage As dxfImage, aArrowIndex As dxxArrowIndicators, aLineOrArc As dxfEntity, aIdentifier As String, Optional bNoTicks As Boolean = False, Optional aSize As Double = 0.0) As dxfEntity
            Dim rSuppress As Boolean = False
            Return GetEntity(aDStyle, aPlane, aImage, aArrowIndex, aLineOrArc, aIdentifier, bNoTicks, rSuppress, aSize)
        End Function
        Friend Shared Function GetEntity(aDStyle As TTABLEENTRY, aPlane As TPLANE, aImage As dxfImage, aArrowIndex As dxxArrowIndicators, aLineOrArc As dxfEntity, aIdentifier As String, bNoTicks As Boolean, ByRef rSuppress As Boolean, Optional aSize As Double = 0.0) As dxfEntity
            rSuppress = False
            If aArrowIndex < dxxArrowIndicators.One Then aArrowIndex = dxxArrowIndicators.One
            If aArrowIndex > dxxArrowIndicators.Leader Then aArrowIndex = dxxArrowIndicators.Leader
            Dim bImage As dxfImage
            Dim aBlock As dxfBlock = Nothing
            Dim aInsert As dxeInsert
            Dim aSolid As dxeSolid
            Dim aTick As dxeLine
            Dim dsp As New TDISPLAYVARS
            Dim arrowPt As TVECTOR
            Dim aDir As TVECTOR
            Dim asz As Double
            Dim bTick As Boolean = False
            Dim SF As Double
            Dim bsup As Boolean = False
            Dim dlen1 As Double
            Dim bname As String = String.Empty
            Dim aRotation As Double
            Dim bNoEnt As Boolean
            Dim dClr As dxxColors
            Dim aALS As New TSEGMENT
            bImage = aImage
            If bImage Is Nothing Then bImage = dxfEvents.GetImage(aDStyle.ImageGUID)
            If bImage Is Nothing Then bImage = dxfGlobals.New_Image()
            If aDStyle.Props.Count <= 0 Then aDStyle = New TTABLEENTRY(bImage.DimStyle)
            asz = 0
            If aSize > 0 Then asz = aSize
            If aArrowIndex = dxxArrowIndicators.Leader Then bNoTicks = True
            dClr = aDStyle.PropValueI(dxxDimStyleProperties.DIMCLRD)
            SF = aDStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
            If SF <= 0 Then SF = 1
            If asz = 0 Then
                If Not bNoTicks Then asz = aDStyle.PropValueD(dxxDimStyleProperties.DIMTSZ) * SF
                If asz = 0 Then asz = aDStyle.PropValueD(dxxDimStyleProperties.DIMASZ) * SF Else bTick = True
                bsup = asz <= 0
                If bsup Then asz = 0.09 * SF
            End If
            If Not bTick Then
                aBlock = dxfArrowheads.GetArrowHeads(bImage, aDStyle).Item(aArrowIndex)
                bname = aBlock.Name.ToUpper
            End If
            'set the entity to align the arrowhead on
            If aLineOrArc IsNot Nothing Then
                dsp = TENTITY.DisplayVarsFromProperties(aLineOrArc.Properties)
                dsp.LineWeight = aDStyle.PropValueI(dxxDimStyleProperties.DIMLWE)
                aRotation = dxfArrowheads.Align(aPlane, aLineOrArc, asz, arrowPt, bsup, bNoEnt, aALS)
            Else
                dsp = New TDISPLAYVARS("0", dxfLinetypes.ByBlock, dClr, aDStyle.PropValueI(dxxDimStyleProperties.DIMLWE), 1)
                bNoEnt = True
            End If
            rSuppress = bsup
            dsp.Suppressed = rSuppress
            If rSuppress Then asz = 1
            If bTick Then
                dlen1 = Math.Sqrt(asz ^ 2 + asz ^ 2)
                aDir = aPlane.Direction(45 + aRotation, False) '45 degrees
                Return New dxeLine(arrowPt + aDir * dlen1, arrowPt + aDir * -dlen1) With {.DisplayStructure = dsp, .ImageGUID = bImage.GUID}
                Return aTick
            Else
                If bname = "_CLOSEDFILLED" Then
                    aSolid = DirectCast(aBlock.Entities.GetByGraphicType(dxxGraphicTypes.Solid).Item(1, bReturnClone:=True), dxeSolid)
                    'aSolid.Vertices.Vectors.Print()
                    Dim aTrs As New TTRANSFORMS()
                    aPlane = aSolid.PlaneV.AlignedTo(aPlane.ZDirection, dxxAxisDescriptors.Z)
                    aTrs.Add(TTRANSFORM.CreateScale(New TVECTOR, asz))
                    Dim axis As dxeLine = Nothing
                    If Not bNoEnt And aRotation <> 0 Then aTrs.Add(TTRANSFORM.CreatePlanarRotation(aPlane.Origin, aPlane, aRotation, False, Nothing, dxxAxisDescriptors.Z, axis))
                    If Not TVECTOR.IsNull(arrowPt) Then aTrs.Add(TTRANSFORM.CreateTranslation(arrowPt))
                    'aSolid.Transform(TTRANSFORM.CreateScale(New TVECTOR, asz), True)
                    aSolid.Suppressed = False
                    'rotate to plane
                    aSolid.PlaneV = aPlane
                    'aSolid.Translate(arrowPt)
                    aSolid.DisplayStructure = dsp
                    aSolid.ImageGUID = bImage.GUID
                    aSolid.Tag = bname
                    aSolid.Identifier = aIdentifier
                    aSolid.Transform(aTrs, True)
                    'If Not bNoEnt And aRotation <> 0 Then aSolid.Rotate(aRotation)
                    aSolid.Flag = bname
                    Return aSolid
                Else
                    aInsert = New dxeInsert With {
                        .PlaneV = aPlane,
                        .Block = aBlock,
                        .ScaleFactor = asz,
                        .InsertionPtV = arrowPt,
                        .RotationAngle = aRotation,
                        .DisplayStructure = dsp,
                        .ImageGUID = bImage.GUID,
                        .Tag = bname,
                        .Identifier = aIdentifier
                    }
                    Return aInsert
                End If
            End If
            Return Nothing
        End Function
        Friend Shared Function CreateArrowHeadBlock(aImage As dxfImage, aBlockName As String, Optional bDontUpdateImage As Boolean = False) As dxfBlock
            If Not IsDefault(aBlockName) Then Return Nothing
            Dim _rVal As New dxfBlock(aBlockName)
            aBlockName = aBlockName.ToUpper
            Dim bCF As Boolean
            Dim aEnt As dxfEntity
            Select Case aBlockName
            '------------------------------------------------------------
                Case "_NONE"
            '------------------------------------------------------------
            '------------------------------------------------------------
                Case "_CLOSEDFILLED"
                    '------------------------------------------------------------
                    bCF = True
                    _rVal.Entities.AddSolid("(0,0,0)¸(-1, -0.166666666666667)¸(-1, 0.166666666666667) ")
                    _rVal.IsDefault = True
            '------------------------------------------------------------
                Case "_CLOSEDBLANK", "_CLOSED", "_OPEN", "_OPEN90", "_OPEN30"
                    '------------------------------------------------------------
                    If aBlockName = "_OPEN90" Then
                        _rVal.Entities.AddLine(-0.5, 0.5)
                    ElseIf aBlockName = "_OPEN30" Then
                        _rVal.Entities.AddLine(-1, 0.26794919, 0, 0)
                    Else
                        _rVal.Entities.AddLine(-1, 0.166666666666667, 0, 0)
                    End If
                    If aBlockName = "_OPEN90" Then
                        _rVal.Entities.AddLine(-0.5, -0.5, 0, 0)
                    ElseIf aBlockName = "_OPEN30" Then
                        _rVal.Entities.AddLine(-1, -0.26794919)
                    Else
                        _rVal.Entities.AddLine(-1, -0.166666666666667)
                    End If
                    If aBlockName = "_CLOSEDBLANK" Or aBlockName = "_CLOSED" Then
                        _rVal.Entities.AddLine(-1, 0.166666666666667, -1, -0.166666666666667)
                    End If
                    If aBlockName <> "_CLOSEDBLANK" Then
                        _rVal.Entities.AddLine(-1, 0)
                    End If
            '------------------------------------------------------------
                Case "_DOT"
                    '------------------------------------------------------------
                    _rVal.Entities.AddPolyline("(-0.25,0,0.25)¸(0.25,0,0.25) ", True, aSegmentWidth:=0.5)
                    _rVal.Entities.AddLine(-0.5, 0, -1, 0)
            '------------------------------------------------------------
                Case "_ARCHTICK"
                    '------------------------------------------------------------
                    _rVal.Entities.AddPolyline("(-0.5, -0.5,0)¸(0.5, 0.5,0) ", False, aSegmentWidth:=0.15)
            '------------------------------------------------------------
                Case "_OBLIQUE"
                    '------------------------------------------------------------
                    _rVal.Entities.AddLine(-0.5, -0.5, 0.5, 0.5)
            '------------------------------------------------------------
                Case "_ORIGIN", "_ORIGIN2", "_DOTBLANK", "_SMALL"
                    '------------------------------------------------------------
                    If aBlockName <> "_SMALL" Then _rVal.Entities.AddArc(0, 0, 0.5) Else _rVal.Entities.AddArc(0, 0, 0.25)
                    If aBlockName = "_ORIGIN2" Then
                        _rVal.Entities.AddArc(0, 0, 0.25)
                    End If
                    If aBlockName <> "_SMALL" Then
                        If aBlockName = "_ORIGIN" Then
                            _rVal.Entities.AddLine(0, 0, -1, 0)
                        Else
                            _rVal.Entities.AddLine(-0.5, 0, -1, 0)
                        End If
                    End If
            '------------------------------------------------------------
                Case "_DOTSMALL"
                    '------------------------------------------------------------
                    _rVal.Entities.AddPolyline("(-.0625, 0,.0625)¸(.0625, 0,.0625)", True, aSegmentWidth:=0.5)
            '------------------------------------------------------------
                Case "_BOXBLANK", "_BOXFILLED"
                    '------------------------------------------------------------
                    If aBlockName = "_BOXBLANK" Then
                        _rVal.Entities.AddLine(-0.5, -0.5, 0.5, -0.5)
                        _rVal.Entities.AddLine(0.5, -0.5, 0.5, 0.5)
                        _rVal.Entities.AddLine(0.5, 0.5, -0.5, 0.5)
                        _rVal.Entities.AddLine(-0.5, 0.5, -0.5, -0.5)
                    Else
                        _rVal.Entities.AddSolid("(-0.5, 0.5)¸(0.5, 0.5)¸(0.5, -0.5)¸(-0.5, -0.5)")
                    End If
                    _rVal.Entities.AddLine(-1, 0, -0.5, 0)
            '------------------------------------------------------------
                Case "_DATUMBLANK", "_DATUMFILLED"
                    '------------------------------------------------------------
                    If aBlockName = "_DATUMBLANK" Then
                        _rVal.Entities.AddLine(0, 0.57735027, -1, 0)
                        _rVal.Entities.AddLine(0, -0.57735027, -1, 0)
                        _rVal.Entities.AddLine(0, -0.57735027, 0, 0.57735027)
                    Else
                        _rVal.Entities.AddSolid("(0, 0.57735027)¸(-1,0)¸(0, -0.57735027)¸(0, -0.57735027)")
                    End If
            '------------------------------------------------------------
                Case "_INTEGRAL"
                    '------------------------------------------------------------
                    _rVal.Entities.AddArc(0.44488802, -0.09133463, 0.45416667, 101.999999998039, 167.999999979919)
                    _rVal.Entities.AddArc(-0.44488802, 0.09133463, 0.45416667, 282.000000021543, 348.000000003422)
            End Select
            Dim aDSP As New TDISPLAYVARS(aLineType:=dxfLinetypes.ByBlock, aColor:=dxxColors.ByBlock, aLineweight:=dxxLineWeights.ByBlock)
            For i As Integer = 1 To _rVal.Entities.Count
                aEnt = _rVal.Entities.Item(i)
                aEnt.DisplayStructure = aDSP
            Next
            _rVal.IsArrowHead = True
            _rVal.Suppressed = bCF
            If aImage IsNot Nothing Then
                aImage.HandleGenerator.AssignTo(_rVal)
            End If
            Return _rVal
        End Function
        Friend Shared Function GetArrowHeads(aImage As dxfImage, aDimStyle As TTABLEENTRY, Optional aBlockName As String = "") As List(Of dxfBlock)
            Dim _rVal As New List(Of dxfBlock)
            Dim aBlk As dxfBlock
            If aImage Is Nothing Then aImage = aDimStyle.Image
            If aImage Is Nothing Then Return _rVal
            dxfImageTool.ConfirmArrowHeads(aImage, aDimStyle)
            If String.IsNullOrEmpty(aBlockName) Then aBlockName = ""
            aBlockName = aBlockName.Trim
            If aBlockName <> "" Then
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, aBlockName, bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
            Else
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, "DIMBLK1", bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, "DIMBLK2", bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, "DIMLDRBLK", bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
            End If
            Return _rVal
        End Function

        Friend Shared Function GetArrowHeads(aImage As dxfImage, aDimStyle As dxoDimStyle, Optional aBlockName As String = "") As List(Of dxfBlock)
            Dim _rVal As New List(Of dxfBlock)
            If aDimStyle Is Nothing Then Return _rVal
            Dim aBlk As dxfBlock
            If aImage Is Nothing Then aImage = aDimStyle.Image
            If aImage Is Nothing Then Return _rVal
            dxfImageTool.ConfirmArrowHeads(aImage, aDimStyle)
            If String.IsNullOrEmpty(aBlockName) Then aBlockName = ""
            aBlockName = aBlockName.Trim
            If aBlockName <> "" Then
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, aBlockName, bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
            Else
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, "DIMBLK1", bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, "DIMBLK2", bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
                aBlk = dxfArrowheads.GetArrowHead(aImage, aDimStyle, "DIMLDRBLK", bSuppressConfirmation:=True)
                If aBlk IsNot Nothing Then _rVal.Add(aBlk)
            End If
            Return _rVal
        End Function

        Friend Shared Function GetArrowHead(aImage As dxfImage, aDimStyle As TTABLEENTRY, aBlockName As String, Optional bSuppressConfirmation As Boolean = False) As dxfBlock
            If aImage Is Nothing Then aImage = aDimStyle.Image
            If aImage Is Nothing Then Return Nothing
            If Not bSuppressConfirmation Then dxfImageTool.ConfirmArrowHeads(aImage, aDimStyle)
            If String.IsNullOrEmpty(aBlockName) Then aBlockName = ""
            aBlockName = aBlockName.Trim.ToUpper
            If aBlockName = "" Then Return Nothing
            Dim bname As String = "_None"
            Dim idxName As dxxDimStyleProperties
            Dim idxHandle As dxxDimStyleProperties
            Select Case aBlockName
                Case "DIMBLK", "DIMBLK_NAME"
                    idxName = dxxDimStyleProperties.DIMBLK_NAME
                    idxHandle = dxxDimStyleProperties.DIMBLK
                    bname = aDimStyle.PropValueStr(idxName)
                Case "DIMBLK1", "DIMBLK1_NAME"
                    idxName = dxxDimStyleProperties.DIMBLK1_NAME
                    idxHandle = dxxDimStyleProperties.DIMBLK1
                    bname = aDimStyle.PropValueStr(idxName)
                Case "DIMBLK2", "DIMBLK2_NAME"
                    idxName = dxxDimStyleProperties.DIMBLK2_NAME
                    idxHandle = dxxDimStyleProperties.DIMBLK2
                    bname = aDimStyle.PropValueStr(idxName)
                Case "DIMLDRBLK_NAME", "DIMLDRBLK"
                    idxName = dxxDimStyleProperties.DIMLDRBLK_NAME
                    idxHandle = dxxDimStyleProperties.DIMLDRBLK
                    bname = aDimStyle.PropValueStr(idxName)
                Case Else
                    Return Nothing
            End Select
            If bname = "" Then bname = "_ClosedFilled"
            Dim aBlk As dxfBlock = aImage.Blocks.GetByName(bname, bLoadDefaults:=True)
            If aBlk Is Nothing And bname <> "_None" Then aBlk = aImage.Blocks.GetByName("_None", bLoadDefaults:=True)
            aDimStyle.PropValueSet(idxName, aBlk.Name)
            aDimStyle.PropValueSet(idxHandle, aBlk.BlockRecordHandle)
            Return aBlk
        End Function

        Friend Shared Function GetArrowHead(aImage As dxfImage, aDimStyle As dxoDimStyle, aBlockName As String, Optional bSuppressConfirmation As Boolean = False) As dxfBlock
            If aDimStyle Is Nothing Then Return Nothing
            If aImage Is Nothing Then aImage = aDimStyle.Image
            If aImage Is Nothing Then Return Nothing
            If Not bSuppressConfirmation Then dxfImageTool.ConfirmArrowHeads(aImage, aDimStyle)
            If String.IsNullOrWhiteSpace(aBlockName) Then aBlockName = "" Else aBlockName = aBlockName.Trim
            aBlockName = aBlockName.Trim.ToUpper
            If aBlockName = "" Then Return Nothing
            Dim bname As String = "_None"
            Dim idxName As dxxDimStyleProperties
            Dim idxHandle As dxxDimStyleProperties
            Select Case aBlockName
                Case "DIMBLK", "DIMBLK_NAME"
                    idxName = dxxDimStyleProperties.DIMBLK_NAME
                    idxHandle = dxxDimStyleProperties.DIMBLK
                    bname = aDimStyle.PropValueStr(idxName)
                Case "DIMBLK1", "DIMBLK1_NAME"
                    idxName = dxxDimStyleProperties.DIMBLK1_NAME
                    idxHandle = dxxDimStyleProperties.DIMBLK1
                    bname = aDimStyle.PropValueStr(idxName)
                Case "DIMBLK2", "DIMBLK2_NAME"
                    idxName = dxxDimStyleProperties.DIMBLK2_NAME
                    idxHandle = dxxDimStyleProperties.DIMBLK2
                    bname = aDimStyle.PropValueStr(idxName)
                Case "DIMLDRBLK_NAME", "DIMLDRBLK"
                    idxName = dxxDimStyleProperties.DIMLDRBLK_NAME
                    idxHandle = dxxDimStyleProperties.DIMLDRBLK
                    bname = aDimStyle.PropValueStr(idxName)
                Case Else
                    Return Nothing
            End Select
            If String.IsNullOrWhiteSpace(bname) Then bname = "_ClosedFilled"
            Dim aBlk As dxfBlock = aImage.Blocks.GetByName(bname, bLoadDefaults:=True)
            If aBlk Is Nothing And String.Compare(bname, "_None", True) <> 0 Then aBlk = aImage.Blocks.GetByName("_None", bLoadDefaults:=True)
            aDimStyle.PropValueSet(idxName, aBlk.Name)
            aDimStyle.PropValueSet(idxHandle, aBlk.BlockRecordHandle)
            Return aBlk
        End Function


        Friend Shared Function GetEntitySymbol(aSymbol As dxeSymbol, aImage As dxfImage, aArrowType As dxxArrowHeadTypes, aLineOrArc As dxfEntity, aIdentifier As String, Optional bSuppress As Boolean = False, Optional aSize As Double = 0.0) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            bSuppress = False
            If aSymbol Is Nothing Then Return _rVal
            Dim bImage As dxfImage
            Dim aBlock As dxfBlock = Nothing
            Dim asz As Double
            Dim SF As Double
            Dim bsup As Boolean
            Dim trimrad As Double
            Dim gType As dxxGraphicTypes
            Dim arrowPt As TVECTOR
            Dim dlen1 As Double
            Dim v1 As TVECTOR
            Dim iPts As TVECTORS
            Dim bname As String
            Dim aInsert As dxeInsert
            Dim aSolid As dxeSolid
            Dim aAlignTo As New TLINE
            Dim aAlignDir As TVECTOR

            Dim aLine As dxeLine = Nothing
            Dim aArc As New dxeArc
            Dim aRotation As Double
            Dim aRec As New TPLANE("")
            Dim aPaths As TPATHS
            Dim aPline As dxePolyline
            Dim aPlane As New TPLANE("")
            Dim dClr As dxxColors
            Dim sp As TVECTOR

            aPlane = aSymbol.PlaneV
            bImage = aImage
            aSymbol.GetImage(bImage)
            If bImage Is Nothing Then bImage = dxfGlobals.New_Image()
            bsup = False
            dClr = aSymbol.LineColor
            SF = aSymbol.FeatureScale
            If aSize <= 0 Then
                asz = aSymbol.ArrowSize * SF
            Else
                asz = aSize
            End If
            bsup = asz <= 0
            If bsup Then asz = 0.09 * SF
            aBlock = aImage.Blocks.GetByName(dxfEnums.Description(aSymbol.ArrowHead), True)
            If aBlock Is Nothing Then aBlock = aImage.Blocks.GetByName("_None", True)
            aBlock.Suppressed = False
            Dim dsp As New TDISPLAYVARS With {
               .Color = dClr,
               .LayerName = aSymbol.LayerName,
               .Linetype = dxfLinetypes.Continuous,
               .LineWeight = dxxLineWeights.ByLayer,
               .LTScale = 1
            }
            gType = dxxGraphicTypes.Undefined
            'set the entity to align the arrowhead on
            If aLineOrArc IsNot Nothing Then
                gType = aLineOrArc.GraphicType
                If gType = dxxGraphicTypes.Polyline Then
                    aPline = aLineOrArc
                    aLineOrArc = aPline.Segments.FirstMember
                    If aLineOrArc IsNot Nothing Then gType = aLineOrArc.GraphicType
                End If
                If aLineOrArc IsNot Nothing Then
                    If gType = dxxGraphicTypes.Line Then
                        aLine = aLineOrArc
                        arrowPt = aLine.StartPtV
                        If aLine.Suppressed Then bsup = True
                        If Math.Round(aLine.Length, 4) <= 1.99 * asz Then bsup = True
                        aAlignTo = New TLINE(aLine)
                    ElseIf gType = dxxGraphicTypes.Arc Then
                        aArc = aLineOrArc
                        arrowPt = aArc.StartPtV
                        If aArc.Suppressed Then bsup = True
                        If Math.Round(aArc.Length, 4) <= 1.99 * asz Then bsup = True
                        aAlignTo = dxfUtils.ArcTangentLine(aArc.ArcStructure, aArc.StartAngle, 10)
                    End If
                    aAlignDir = aAlignTo.SPT.DirectionTo(aAlignTo.EPT, False, dlen1)
                    If dlen1 <= 0.0001 Then
                        bsup = True
                        gType = dxxGraphicTypes.Undefined
                    Else
                        aRotation = (aPlane.XDirection * -1).AngleTo(aAlignDir, aPlane.ZDirection)
                    End If
                End If
            End If
            dsp.Suppressed = bsup
            If bsup Then asz = 1
            bname = UCase(aBlock.Name)
            If bname = "_CLOSEDFILLED" Then
                aSolid = aBlock.Entities.Item(1, True)
                aSolid.Transform(TTRANSFORM.CreateScale(New TVECTOR, asz), True)
                'rotate to plane
                aSolid.PlaneV = aSolid.PlaneV.AlignedTo(aPlane.ZDirection, dxxAxisDescriptors.Z)
                aSolid.Translate(arrowPt)
                aSolid.DisplayStructure = dsp
                aSolid.ImageGUID = bImage.GUID
                If gType <> dxxGraphicTypes.Undefined Then
                    aSolid.Rotate(aRotation)
                    If Not bsup Then trimrad = asz
                End If
                aSolid.Flag = bname
                _rVal = aSolid
            Else
                aInsert = New dxeInsert(aBlock) With {
                    .PlaneV = aPlane,
                    .ScaleFactor = asz,
                    .InsertionPtV = arrowPt,
                    .RotationAngle = aRotation,
                    .DisplayStructure = dsp,
                    .ImageGUID = bImage.GUID
                }
                'rotate to alignment entity
                If gType <> dxxGraphicTypes.Undefined Then
                    If Not bsup And aBlock.Entities.Count > 0 Then
                        If aBlock.IsArrowHead Then
                            If bname <> "_NONE" And bname <> "_ARCHTICK" And bname <> "_OBLIQUE" And bname <> "_SMALL" And bname <> "_INTEGRAL" And bname <> "_DOTSMALL" Then trimrad = asz
                        Else
                            aPaths = aBlock.RelativePaths
                            aRec = aPaths.ExtentVectors.Bounds(New TPLANE(""))
                            trimrad = 0.5 * aRec.Width
                        End If
                    End If
                End If
                _rVal = aInsert
            End If
            If _rVal IsNot Nothing Then
                _rVal.OwnerGUID = aSymbol.GUID
                _rVal.Tag = bname
                _rVal.Identifier = aIdentifier
                If aImage IsNot Nothing Then _rVal.ImageGUID = aImage.GUID
            End If
            bSuppress = bsup
            If gType <> dxxGraphicTypes.Undefined And trimrad > 0 And Not bSuppress Then
                Dim trimArc As New TARC(aPlane, arrowPt, trimrad)
                If Not aLine Is Nothing Then
                    iPts = New TLINE(aLine).IntersectionPts(trimArc, True)
                    If iPts.Count > 0 Then
                        sp = aLine.EndPtV
                        v1 = iPts.Nearest(sp, TVECTOR.Zero)
                        aLine.StartPtV = v1
                    End If
                ElseIf aArc IsNot Nothing Then
                    Dim bArc As TSEGMENT = aArc.ArcLineStructure
                    iPts = bArc.ArcStructure.IntersectionPts(trimArc)
                    If iPts.Count > 0 Then
                        sp = bArc.ArcStructure.EndPt
                        v1 = iPts.Nearest(sp, TVECTOR.Zero)
                        If bArc.ContainsVector(v1, bSuppressPlaneCheck:=True) Then
                            bArc.ArcStructure.StartAngle = aPlane.XDirection.AngleTo(bArc.ArcStructure.Plane.Origin.DirectionTo(v1), aPlane.ZDirection)
                            bArc.ArcStructure = TARC.ArcStructure(bArc.ArcStructure.Plane, bArc.ArcStructure.Plane.Origin, bArc.ArcStructure.Radius, bArc.ArcStructure.StartAngle, bArc.ArcStructure.EndAngle, bArc.ArcStructure.ClockWise)
                            aArc.ArcStructure = bArc.ArcStructure
                        End If
                    End If
                End If
            End If
            Return _rVal
        End Function
        Friend Shared Function GetPaths(aDimensionEntity As dxfEntity, aImage As dxfImage, aArrowIndex As dxxArrowIndicators, aLineOrArc As TSEGMENT, ByRef rExtentPts As TVECTORS, bNoTicks As Boolean, ByRef rSuppress As Boolean, ByRef ioBlock As dxfBlock) As TPATHS
            rSuppress = True
            If aDimensionEntity Is Nothing Then Return New TPATHS(dxxDrawingDomains.Model)
            Dim _rVal As New TPATHS(aDimensionEntity.Domain)
            Dim aDimStyle As dxoDimStyle = aDimensionEntity.DimStyle
            If aDimStyle Is Nothing Then Return _rVal
            If Not aDimStyle.GetImage(aImage) Then Return _rVal
            If ioBlock IsNot Nothing Then bNoTicks = True
            Dim asz As Double = 0
            Dim bTick As Boolean = False
            Dim SF As Double = aDimStyle.FeatureScaleFactor
            Dim bsup As Boolean = False
            Dim trimrad As Double
            Dim gType As dxxGraphicTypes
            Dim dsp As TDISPLAYVARS = aDimensionEntity.DisplayStructure
            Dim arrowPt As TVECTOR
            Dim dlen1 As Double
            Dim aDir As TVECTOR
            Dim iPts As TVECTORS
            Dim bname As String
            Dim aAlignTo As New TLINE("")
            Dim aAlignDir As TVECTOR
            Dim trimArc As TARC
            Dim aArc As dxeArc = Nothing
            Dim aRotation As Double
            Dim aRec As TPLANE
            Dim aPaths As TPATHS
            Dim aPlane As TPLANE = aDimensionEntity.PlaneV
            Dim bPlane As TPLANE = aPlane
            Dim aPath As TPATH
            Dim bDefArrow As Boolean
            Dim i As Integer
            Dim bPath As TPATH
            Dim blkExts As TVECTORS
            Dim v1 As TVECTOR
            Dim aHandles As THANDLES = aDimensionEntity.Handlez_Get
            Dim bArc As TSEGMENT
            Dim aTrs As New TTRANSFORMS
            rSuppress = False
            If aArrowIndex < dxxArrowIndicators.One Then aArrowIndex = dxxArrowIndicators.One
            If aArrowIndex > dxxArrowIndicators.Leader Then aArrowIndex = dxxArrowIndicators.Leader
            If aArrowIndex = dxxArrowIndicators.Leader Then bNoTicks = True
            If Not bNoTicks Then asz = aDimStyle.ArrowTickSize * SF
            If asz = 0 Then asz = aDimStyle.ArrowSize * SF Else bTick = True
            bsup = asz <= 0
            If bsup Then asz = 0.09 * SF
            If Not bTick Then
                If ioBlock Is Nothing Then ioBlock = aDimStyle.GetArrowHeads(aImage).Item(aArrowIndex)
            End If
            dsp.Color = aDimStyle.DimLineColor
            If dsp.Color = dxxColors.ByBlock Then dsp.Color = aDimensionEntity.Color
            dsp.LineWeight = aDimStyle.PropValueI(dxxDimStyleProperties.DIMLWE)
            If dsp.LineWeight = dxxLineWeights.ByBlock Then dsp.LineWeight = aDimensionEntity.LineWeight
            'set the entity to align the arrowhead on
            'dsp = aLineOrArc.DisplayStructure
            gType = dxxGraphicTypes.Undefined
            If Not aLineOrArc.IsArc Then
                gType = dxxGraphicTypes.Line
                aAlignTo = aLineOrArc.LineStructure
                arrowPt = aAlignTo.SPT
                If dsp.Suppressed Then bsup = True
                dlen1 = Math.Round(aAlignTo.Length, 4)
                If dlen1 <= 1.99 * asz Then
                    bsup = True
                End If
            ElseIf gType = dxxGraphicTypes.Arc Then
                aArc = aLineOrArc
                arrowPt = aArc.StartPtV
                If aArc.Suppressed Then bsup = True
                If Math.Round(aArc.Length, 4) <= 1.99 * asz Then bsup = True
                aAlignTo = dxfUtils.ArcTangentLine(aArc.ArcStructure, aArc.StartAngle, 10)
            Else
            End If
            If gType <> dxxGraphicTypes.Undefined Then
                aAlignDir = aAlignTo.Direction
                aRotation = (aPlane.XDirection * -1).AngleTo(aAlignDir, aPlane.ZDirection)
            End If
            dsp.Suppressed = bsup
            If bsup Then asz = 1
            If ioBlock Is Nothing Then
                bname = "Tick"
                dlen1 = Math.Sqrt(asz ^ 2 + asz ^ 2)
                aDir = aPlane.Direction(45 + aRotation, False) '45 degrees
                aPath = New TPATH With {.Plane = aPlane}
                aPath.DisplayVars = dsp
                aPath.AddLine(arrowPt + aDir * dlen1, arrowPt + aDir * -dlen1)
                _rVal.Add(aPath)
                rExtentPts.Append(aPath.Looop(1))
            Else
                If ioBlock.Entities.Count > 0 Then
                    bDefArrow = ioBlock.IsArrowHead
                    If bDefArrow Then
                        SF = asz
                        bname = ioBlock.Name.ToUpper
                        If gType <> dxxGraphicTypes.Undefined Then
                            If bname <> "_NONE" And bname <> "_ARCHTICK" And bname <> "_OBLIQUE" And bname <> "_SMALL" And bname <> "_INTEGRAL" And bname <> "_DOTSMALL" Then trimrad = asz
                        End If
                    Else
                        aRec = ioBlock.ExtentPts.Bounds(New TPLANE(""))
                        If aRec.Width > 0 Then
                            SF = asz / aRec.Width
                            If gType <> dxxGraphicTypes.Undefined Then trimrad = asz / 2
                        End If
                    End If
                    'v1 = ioBlock.InsertionPtV
                    'aPaths = ioBlock.Entities.Paths(False, aImage)
                    'If Not v1.IsNull() Then aTrs.Add(TTRANSFORM.CreateTranslation(v1 * -1))
                    'If SF <> 1 And SF <> 0 Then aTrs.Add(TTRANSFORM.CreateScale(New TVECTOR, SF))
                    'If aTrs.Count > 0 Then TTRANSFORMS.Apply(aTrs, aPaths, False, False)
                    bPlane = aPlane.Clone
                    bPlane.Origin = arrowPt
                    'If aRotation <> 0 Then bPlane.Revolve(aRotation)
                    aPaths = dxfPaths.Paths_Insert(ioBlock, New dxfVector(bPlane.Origin), SF, aRotation, New dxfPlane(bPlane), dsp, aImage, aDimensionEntity.Domain, "Arrowhead")
                    'Dim aInsrt As New dxeInsert(ioBlock, New dxfVector(bPlane.Origin), SF, aRotation:=aRotation) With {.PlaneV = bPlane}
                    'aInsrt.UpdatePath(True, aImage)
                    'aPaths =   aInsrt.Paths.Clone
                    'aPaths = ioBlock.Entities.Paths(aImage:=aImage)
                    For i = 1 To aPaths.Count
                        bPath = aPaths.Item(i)
                        If bPath.Relative Then bPath.ConvertToWorld()
                        aPath = bPath
                        _rVal.AddOrJoin(aPath)
                    Next i
                    blkExts = aPaths.ExtentVectors '.WithRespectToPlane(bPlane, 1, 1, 1, 0)
                    rExtentPts.Append(blkExts)
                End If
            End If
            rSuppress = bsup
            If gType <> dxxGraphicTypes.Undefined And trimrad > 0 And Not rSuppress Then
                trimArc = New TARC(aPlane, arrowPt, trimrad)
                If gType = dxxGraphicTypes.Line Then
                    iPts = aLineOrArc.LineStructure.IntersectionPts(trimArc, True)
                    If iPts.Count > 0 Then
                        v1 = iPts.Nearest(aAlignTo.EPT, TVECTOR.Zero)
                        aLineOrArc.LineStructure.SPT = v1
                    End If
                ElseIf gType = dxxGraphicTypes.Arc Then
                    bArc = aArc.ArcLineStructure
                    iPts = bArc.ArcStructure.IntersectionPts(trimArc)
                    If iPts.Count > 0 Then
                        v1 = iPts.Nearest(bArc.ArcStructure.EndPt, TVECTOR.Zero)
                        If bArc.ContainsVector(v1, bSuppressPlaneCheck:=True) Then
                            bArc.ArcStructure.StartAngle = aPlane.XDirection.AngleTo(bArc.ArcStructure.Plane.Origin.DirectionTo(v1), aPlane.ZDirection)
                            bArc.ArcStructure = TARC.ArcStructure(bArc.ArcStructure.Plane, bArc.ArcStructure.Plane.Origin, bArc.ArcStructure.Radius, bArc.ArcStructure.StartAngle, bArc.ArcStructure.EndAngle, bArc.ArcStructure.ClockWise)
                            aArc.ArcStructure = bArc.ArcStructure
                        End If
                    End If
                End If
            End If
            _rVal.SetLayerName(aDimensionEntity.LayerName)
            Return _rVal
        End Function
        Public Shared Function IsDefault(ByRef ioName As String) As Boolean
            Dim _rVal As Boolean
            Dim aName As String = ioName.Trim()
            If aName = "" Or aName = "_" Then
                aName = "_ClosedFilled"
                Return True
            End If
            _rVal = ArrowHeads.FindIndex(Function(x) String.Compare(x, aName, True) = 0) >= 0
            If Not _rVal And Not aName.StartsWith("_") Then
                If ArrowHeads.FindIndex(Function(x) String.Compare(x, $"_{aName}", True) = 0) >= 0 Then
                    _rVal = True
                    ioName = $"_{aName}"
                End If
            End If
            Return _rVal
        End Function

        Private Shared _ArrowHeads As List(Of String) = Nothing
        Public Shared ReadOnly Property ArrowHeads As List(Of String)
            Get
                If _ArrowHeads Is Nothing Then
                    _ArrowHeads = New List(Of String)({"_ClosedFilled", "_ClosedBlank", "_Closed", "_Dot", "_ArchTick", "_Oblique", "_Open", "_Origin", "_Origin2", "_Open90", "_Open30", "_DotSmall", "_Small", "_DotBlank", "_BoxBlank", "_BoxFilled", "_DatumBlank", "_DatumFilled", "_Integral", "_None"})
                End If
                Return _ArrowHeads
            End Get
        End Property

        Friend Shared Function ArrowsFit(aWorkPlane As TPLANE, aDimPt1 As TVECTOR, aDimPt2 As TVECTOR, dimLn1 As TLINE, dimLn2 As TLINE, ByRef iowRec1 As TPLANE, ByRef iowBnd1 As TLINES, ByRef iowRec2 As TPLANE, ByRef iowBnd2 As TLINES) As Boolean
            'see if the arrows fit
            'the direction from the first dim line  start pt to the second
            Dim aDir As TVECTOR
            Dim iLns As New TLINES(1)
            Dim bFlag As Boolean
            aDir = dimLn1.SPT.DirectionTo(dimLn2.SPT, False, bFlag)
            If bFlag Then Return False 'the arrows dont fit if the start pts are coincident
            If Not aDir.Equals(aWorkPlane.XDirection, 3) Then Return False 'the arrows dont fit if the direction is inverse to the working direction
            If (iowRec1.Width <= 0 And iowRec2.Width <= 0) Then Return False
            If iowRec1.Width > 0 Then 'see the bounding rectangle of the first arrow is intesected by the second dimension line
                iLns.SetItem(1, dimLn2)
                If iLns.IntersectionPts(iowBnd1, False, False, True, True).Count > 0 Then Return False
            End If
            If iowRec2.Width > 0 Then 'see the bounding rectangle of the first arrow is intesected by the second dimension line
                iLns.SetItem(1, dimLn1)
                If iLns.IntersectionPts(iowBnd2, False, False, True, True).Count > 0 Then Return False
            End If
            If iowRec1.Width > 0 Then 'see the bounding rectangle of the first arrow is intesected by the second extension line
                iLns.SetItem(1, New TLINE(aDimPt2, aDimPt2 + aWorkPlane.YDirection * 10))
                If iLns.IntersectionPts(iowBnd1, True, False, True, True).Count > 0 Then Return False
            End If
            If iowRec2.Width > 0 Then 'see the bounding rectangle of the second arrow is intesected by the First extension line
                iLns.SetItem(1, New TLINE(aDimPt1, aDimPt1 + aWorkPlane.YDirection * 10))
                If iLns.IntersectionPts(iowBnd2, True, False, True, True).Count > 0 Then Return False
            End If
            If iowRec2.Width > 0 Then 'see if the the arrow rectangles intersect
                If iowBnd1.IntersectionPts(iowBnd2, False, False, True, True).Count > 0 Then Return False
            End If
            Return True
        End Function
#End Region 'Shared Methods
    End Class ' dxfArrowheads


End Namespace
