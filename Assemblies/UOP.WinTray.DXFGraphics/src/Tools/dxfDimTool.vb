
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfDimTool
#Region "Methods"
        Friend Shared Function PickLine(aLine As dxeLine, bLine As dxeLine, aIndex As Integer) As dxeLine
            If aIndex = 2 Then Return bLine Else Return aLine
        End Function
        Friend Shared Function PickVector(aVector As TVECTOR, bVector As TVECTOR, aIndex As Integer) As TVECTOR
            If aIndex = 2 Then Return bVector Else Return aVector
        End Function
        Friend Shared Function SwapArrows(bArrowsOut As Boolean, ByRef rDimLine1 As TLINE, ByRef rDimLine2 As TLINE, aExtLine1 As TLINE, aExtLine2 As TLINE, aArrow1 As dxfEntity, aArrow2 As dxfEntity, aPlane As TPLANE, aArrowSize As Double, aDimensionLine1 As dxeLine, aDimensionLine2 As dxeLine) As Boolean

            If Not bArrowsOut Then
                rDimLine1 = dxfProjections.LineAcrossLine(rDimLine1, aExtLine1)
                rDimLine2 = dxfProjections.LineAcrossLine(rDimLine2, aExtLine2)
                Dim aInsert As dxeInsert
                Dim aSolid As dxeSolid
                If aArrow1 IsNot Nothing Then
                    If aArrow1.EntityType = dxxEntityTypes.Solid Then
                        aSolid = aArrow1
                        aSolid.Vertices.Transform(TTRANSFORM.CreateMirror(aExtLine1.SPT, aExtLine1.EPT, True), aEventName:="Mirror")
                    Else
                        If aArrow1.EntityType = dxxEntityTypes.Insert Then
                            aInsert = aArrow1
                            If aInsert.BlockName <> "_NONE" And aInsert.BlockName <> "_SMALL" And aInsert.BlockName <> "_DOTSMALL" Then
                                aInsert.RotationAngle = aPlane.XDirection.AngleTo(rDimLine1.EPT.DirectionTo(rDimLine1.SPT), aPlane.ZDirection)
                            End If
                        End If
                    End If
                End If
                If aArrow2 IsNot Nothing Then
                    If aArrow2.EntityType = dxxEntityTypes.Solid Then
                        aSolid = aArrow2
                        aSolid.Vertices.Transform(TTRANSFORM.CreateMirror(aExtLine2.SPT, aExtLine2.EPT, True), aEventName:="Mirror")
                    Else
                        If aArrow2.EntityType = dxxEntityTypes.Insert Then
                            aInsert = aArrow2
                            If aInsert.BlockName <> "_NONE" And aInsert.BlockName <> "_SMALL" And aInsert.BlockName <> "_DOTSMALL" Then
                                aInsert.RotationAngle = aPlane.XDirection.AngleTo(rDimLine2.EPT.DirectionTo(rDimLine2.SPT), aPlane.ZDirection)
                            End If
                        End If
                    End If
                End If
                If aArrowSize = 0 Then
                    rDimLine1.EPT = rDimLine1.SPT
                    rDimLine2.EPT = rDimLine2.SPT
                End If
                If aDimensionLine1 IsNot Nothing Then aDimensionLine1.LineStructure = rDimLine1
                If aDimensionLine2 IsNot Nothing Then aDimensionLine2.LineStructure = rDimLine2
            End If

            Return True
        End Function
        Friend Shared Function GetArrowRectangles(aImage As dxfImage, aDimPt1 As TVECTOR, aDimPt2 As TVECTOR, aPlane As TPLANE, aArrow1 As dxfEntity, aArrow2 As dxfEntity, ByRef iowRec1 As TPLANE, ByRef iowRec2 As TPLANE, ByRef iowBnd1 As TLINES, ByRef iowBnd2 As TLINES) As Object
            Dim _rVal As Object = Nothing
            Dim aPths As TPATHS
            'set the boundaries of the arrowheads
            Try
                iowRec1 = aPlane.Clone
                iowRec2 = aPlane.Clone
                iowRec1.Origin = aDimPt1.Clone
                iowRec2.Origin = aDimPt2.Clone
                iowRec1.SetDimensions(0, 0)
                iowRec2.SetDimensions(0, 0)
                If aArrow1 IsNot Nothing Then
                    aArrow1.UpdatePath(False, aImage)
                    aPths = aArrow1.Paths
                    iowRec1 = aPths.ExtentVectors.Bounds(aPlane)
                    iowBnd1 = iowRec1.Borders
                    'aPths.Print("ARROW1")
                End If
                If aArrow2 IsNot Nothing Then
                    aArrow2.UpdatePath(False, aImage)
                    aPths = aArrow2.Paths
                    iowRec2 = aPths.ExtentVectors.Bounds(aPlane)
                    iowBnd2 = iowRec2.Borders
                    ' aPths.Print("ARROW2")
                End If
            Catch ex As Exception
                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
            End Try
            Return _rVal
        End Function
        Friend Shared Function MakeHooks(bArrowsOut As Boolean, aDimEnt1 As dxfEntity, aDimEnt2 As dxfEntity, aHookPt As TVECTOR, bHookPt As TVECTOR, aArrowSize As Double, ByRef rHook1 As dxfEntity, ByRef rHook2 As dxfEntity, aOCS As TPLANE, bInverted As Boolean, aExtension As Double, Optional bOneHookOnly As Boolean = False) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            rHook1 = Nothing
            rHook2 = Nothing
            Dim hLine1 As dxeLine = Nothing
            Dim hLine2 As dxeLine = Nothing
            Dim dimLn As TLINE
            Dim dLine As dxeLine
            Dim aDir As TVECTOR
            Dim bDir As TVECTOR
            Dim hookDir As TVECTOR
            Dim hang As Double
            Dim arwSize As Double
            Dim sclr As Double
            Dim bTwoHooks As Boolean
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim dp1 As TVECTOR
            Dim dp2 As TVECTOR
            Dim bSuppress1 As Boolean
            arwSize = Math.Abs(aArrowSize)
            sclr = 1
            If aDimEnt2.GraphicType = dxxGraphicTypes.Line Then
                If Not bInverted Then
                    dLine = aDimEnt2
                    dp1 = bHookPt
                    dp2 = aHookPt
                    hookDir = aHookPt.DirectionTo(bHookPt)
                Else
                    dLine = aDimEnt1
                    dp1 = aHookPt
                    dp2 = bHookPt
                    hookDir = bHookPt.DirectionTo(aHookPt)
                End If
                dimLn = New TLINE(dLine)
                bTwoHooks = Not hookDir.Equals(aOCS.XDirection, True, 3) And Not bOneHookOnly
                If bTwoHooks Then
                    If bArrowsOut Then
                        If dLine.Suppressed Then
                            v1 = dp2
                            v2 = dp1 + hookDir * (arwSize + Math.Abs(aExtension))
                            v3 = v2 + aOCS.XDirection * arwSize
                        Else
                            bSuppress1 = True
                            v1 = dp1
                            v2 = dimLn.EPT
                            If aExtension <> 0 Then v2 += hookDir * Math.Abs(aExtension)
                            v3 = v2 + aOCS.XDirection * arwSize
                        End If
                    Else
                        v1 = dp1
                        v2 = dp1 + hookDir * (arwSize + Math.Abs(aExtension))
                        v3 = v2 + aOCS.XDirection * arwSize
                    End If
                Else
                    If bArrowsOut Then
                        If dLine.Suppressed Then
                            v1 = dp2
                            v2 = dimLn.EPT
                        Else
                            v1 = dp1
                            v2 = dimLn.EPT
                        End If
                        If aExtension <> 0 Then v2 += hookDir * Math.Abs(aExtension)
                    Else
                        v1 = dp1
                        v2 = v1 + hookDir * (arwSize + Math.Abs(aExtension))
                    End If
                End If
                If bTwoHooks Then
                    If Not bSuppress1 Then
                        hLine1 = dLine.Clone
                        hLine1.SetVectors(v1, v2)
                        hLine1.Identifier = "Dimension.HookLine1"
                    End If
                    hLine2 = dLine.Clone
                    bDir = v1.DirectionTo(v2)
                    aDir = v2.DirectionTo(v3)
                    hang = aOCS.YDirection.AngleTo(bDir, aOCS.ZDirection)
                    If hang > 0 And hang <= 180 Then v3 = v2 + aOCS.XDirection * -arwSize
                    hLine2.SetVectors(v2, v3)
                    If bSuppress1 Then hLine2.Identifier = "Dimension.HookLine1" Else hLine2.Identifier = "Dimension.HookLine2"
                Else
                    hLine1 = dLine.Clone
                    hLine1.SetVectors(v1, v2)
                    hLine1.Identifier = "Dimension.HookLine1"
                End If
                rHook1 = hLine1
                rHook2 = hLine2
                If bTwoHooks Then _rVal = rHook2 Else _rVal = rHook1
            Else
            End If
            Return _rVal
        End Function
        Friend Shared Function MakeCenterMarks(aDim As dxeDimension, aScaleFactor As Double, ByRef rCenter As dxfVector) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '^creates the extension lines for the passed dimension
            'On Error Resume Next
            Dim msiz As Double
            Dim eClr As dxxColors
            Dim aLine As dxeLine
            Dim bLine As dxeLine
            Dim cLine As dxeLine
            Dim aYDir As dxfDirection
            Dim aXDir As dxfDirection
            Dim rad As Double
            Dim RPt As dxfVector
            Dim bClines As Boolean
            Dim bCMarks As Boolean
            Dim P1 As dxfVector
            Dim dp1 As dxfVector
            Dim dp2 As dxfVector
            Dim d1 As Double
            Dim tpt As dxfVector
            Dim aDir As dxfDirection
            Dim bDir As dxfDirection
            Dim aFlag As Boolean
            Dim aX As dxeLine

            '======= initialize =========================
            _rVal = New colDXFEntities
            msiz = Math.Round(aDim.Style.PropValueD(dxxDimStyleProperties.DIMCEN) * aScaleFactor, 4)
            eClr = aDim.Style.PropValueI(dxxDimStyleProperties.DIMCLRE)
            bClines = msiz < 0
            bCMarks = msiz <> 0
            If Not bCMarks Then bClines = False
            msiz = Math.Abs(msiz)
            If msiz = 0 Then msiz = Math.Round(0.09 * aScaleFactor, 4)
            aXDir = aDim.XDirection
            aYDir = aDim.YDirection
            dp1 = New dxfVector(aDim.DefPt10V.ProjectedTo(aDim.PlaneV))
            dp2 = New dxfVector(aDim.DefPt15V.ProjectedTo(aDim.PlaneV))
            RPt = dp2.Clone
            tpt = New dxfVector(aDim.DefPt11V.ProjectedTo(aDim.PlaneV))
            tpt.ProjectToPlane(aDim.Plane)
            If aDim.DimType = dxxDimTypes.Radial Then
                rCenter = dp1.Clone
            Else
                rad = dp1.DistanceTo(dp2) / 2
                rCenter = dp1.Projected(dp1.DirectionTo(dp2), rad)
            End If
            rad = rCenter.DistanceTo(RPt)
            '============= cente marks =====================
            aLine = New dxeLine
            aLine.CopyDisplayProps(aDim)
            aLine.Color = eClr
            aLine.ConvergeTo(rCenter, aXDir, msiz)
            aLine.Suppressed = Not bCMarks
            aLine.Identifier = "Extension.CenterMark1"
            aLine.StartPt += aXDir * -msiz
            _rVal.Add(aLine)
            bLine = aLine.Clone(Not bCMarks, "Extension.CenterMark2")

            bLine.ConvergeTo(rCenter, aYDir, msiz)
            bLine.StartPt += aYDir * -msiz
            _rVal.Add(bLine)
            '=============== centerlines ========================
            P1 = aLine.EndPt.Projected(aXDir, msiz)
            d1 = rCenter.DistanceTo(P1)
            If d1 <= rad Then
                d1 = (rad - d1) + msiz
                cLine = New dxeLine
                cLine.CopyDisplayProps(aDim)
                cLine.Color = eClr
                cLine.Suppressed = Not bClines
                cLine.ConvergeTo(P1, aXDir, d1)
                cLine.Identifier = "Extension.CenterLine1"
                _rVal.Add(cLine)
                P1 = aLine.StartPt.Projected(aXDir, -msiz)
                cLine = New dxeLine
                cLine.CopyDisplayProps(aDim)
                cLine.Color = eClr
                cLine.Suppressed = Not bClines
                cLine.Identifier = "Extension.CenterLine2"
                cLine.ConvergeTo(P1, aXDir.Inverse, d1)
                _rVal.Add(cLine)
                P1 = bLine.EndPt.Projected(aYDir, msiz)
                cLine = New dxeLine
                cLine.CopyDisplayProps(aDim)
                cLine.Color = eClr
                cLine.Suppressed = Not bClines
                cLine.Identifier = "Extension.CenterLine3"
                cLine.ConvergeTo(P1, aYDir, d1)
                _rVal.Add(cLine)
                P1 = bLine.StartPt.Projected(aYDir, -msiz)
                cLine = New dxeLine
                cLine.CopyDisplayProps(aDim)
                cLine.Color = eClr
                cLine.Suppressed = Not bClines
                cLine.Identifier = "Extension.CenterLine4"
                cLine.ConvergeTo(P1, aYDir, -d1)
                _rVal.Add(cLine)
            End If
Done:
            'make sure the text pt is on line with the center and def points
            aDir = rCenter.DirectionTo(tpt)
            bDir = rCenter.DirectionTo(dp2)
            d1 = rCenter.DistanceTo(tpt)
            If d1 > 0 Then
                aFlag = aDir.IsEqual(bDir)
                If Not aFlag Then
                    aX = aDim.Plane.ZAxis
                    aX.MoveTo(rCenter)
                    d1 = bDir.AngleTo(aDir)
                    tpt.RotateAboutLine(aX, d1)
                    dp1.RotateAboutLine(aX, d1)
                    dp2.RotateAboutLine(aX, d1)
                End If
            End If

            Return _rVal
        End Function
        Friend Shared Function MakeText(aDimension As dxeDimension, aImage As dxfImage, aInsertPt As TVECTOR, aAngle As Double,
                              bAngleValue As Boolean, ByRef rSpan As Double, ByRef rDimText As String,
                              ByRef rTextRect As dxfRectangle, aRectStretch As Double,
                              ByRef rTextHt As Double, ByRef rTextWidth As Double) As dxeText
            Dim _rVal As dxeText = Nothing
            rSpan = aDimension.DimensionValue
            rDimText = ""
            rTextRect = Nothing
            rTextHt = 0
            rTextWidth = 0
            If aDimension Is Nothing Then Return _rVal
            '#1the dimension to create the text for
            '#2the file object which is the source for layers and styles and settings
            '#3 the point where the text should be placed
            '^used to create the text for the passed dimension
            '~creates and returns a dxeText object

            Dim aCS As New dxfPlane(aDimension.Plane)


            Dim aStyle As dxoDimStyle = aDimension.DimStyle
            aDimension.GetImage(aImage)
            If aImage Is Nothing Then aImage = dxfGlobals.New_Image()
            Dim tClr As dxxColors = aStyle.TextColor
            'compute the number being displayed
            If tClr = dxxColors.ByBlock Then tClr = aDimension.Color

            rDimText = aDimension.OverideText
            If rDimText = "" Then
                If Not bAngleValue Then
                    rDimText = aStyle.FormatNumber(rSpan, True)
                    If aDimension.EntityType = dxxEntityTypes.DimRadialD Or aDimension.EntityType = dxxEntityTypes.DimRadialR Then
                        If aDimension.DimType = dxxDimTypes.Diametric Then
                            rDimText = $"%%C{ rDimText}"
                        Else
                            rDimText = $"R{ rDimText}"
                        End If
                    End If
                Else
                    rDimText = aStyle.FormatAngle(rSpan)
                End If
            End If
            rDimText = $"{aStyle.Prefix}{ rDimText }{ aStyle.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)}"
            'style
            Dim styname As String = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aStyle.PropValueStr(dxxDimStyleProperties.DIMTXSTY_NAME))
            'text height
            Dim tht As Double = aStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * aStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
            'intialize
            'initialize the mtext object
            aCS = New dxfPlane(aCS)
            If aAngle <> 0 Then aCS.Rotate(aAngle)
            _rVal = aImage.CreateText(aImage.TableEntry(dxxReferenceTypes.STYLE, styname), "{\A1;" + rDimText + "}", dxxTextTypes.Multiline, dxxTextDrawingDirections.Horizontal, tht, dxxMTextAlignments.MiddleCenter, aAngle:=0, aPlane:=aCS)
            _rVal.DisplayStructure = dxfImageTool.DisplayStructure_Text(aImage, aDimension.LayerName, aColor:=tClr, aTextStyleName:=styname)
            '_rVal.Rotation = 0 ' aAngle
            _rVal.AlignmentPt1.Strukture = aInsertPt.Clone
            rTextRect = _rVal.BoundingRectangle
            If aRectStretch <> 0 Then rTextRect.Stretch(aRectStretch)
            rTextHt = rTextRect.Height : rTextWidth = rTextRect.Width
            _rVal.Vertical = False
            _rVal.IsDimensionText = True
            _rVal.DrawingDirection = dxxTextDrawingDirections.Horizontal
            _rVal.DimensionTextAngle = aDimension.Plane.XDirection.AngleTo(_rVal.Plane.XDirection)
            Return _rVal
        End Function
        Friend Shared Function CreateAngular(aImage As dxfImage, aDim As dxeDimension) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '^used internally by dxoDrawingTool.Dimension to draw a linear Dimension
            '~returns the dimensions block object
            Dim ArcPt1 As TVECTOR
            Dim ArcPt2 As TVECTOR
            Dim IntersectPt As TVECTOR
            Dim extLine1 As dxeLine = Nothing
            Dim extLine2 As dxeLine = Nothing
            Dim Arc1 As dxeArc = Nothing
            Dim Arc2 As dxeArc = Nothing
            Dim Arrow1 As dxfEntity = Nothing
            Dim Arrow2 As dxfEntity = Nothing
            Dim MText As dxeText = Nothing
            Dim aAngle As Double
            Dim AngleArc As dxeArc = Nothing
            Dim hcnt As Integer = 0

            Dim aPln As TPLANE = aDim.PlaneV
            Dim SF As Double
            Try
                '======= initialize =========================
                aDim.SuppressEvents = True
                '========= EXTENSION LINES ======================
                dxfDimTool.ExtensionLines_Angular(aImage, aDim, SF, extLine1, extLine2, ArcPt1, ArcPt2, aAngle, AngleArc, IntersectPt)

                '========= DIM LINES ======================
                dxfDimTool.DimLines_Angular(aImage, aDim, SF, ArcPt1, ArcPt2, Arc1, Arc2, aAngle, AngleArc, IntersectPt)
                '    ========= TEXT ======================
                Dim textEnts As colDXFEntities = dxfDimTool.DimText_Angular(MText, aImage, aDim, SF, aAngle, AngleArc, extLine1, extLine2, Arc1, Arc2, ArcPt1, ArcPt2, Arrow1, Arrow2, IntersectPt)
                '========= SET DXF ENTITY PROPERTIES ========================

                If extLine1 IsNot Nothing Then _rVal.Add(extLine1)
                If extLine2 IsNot Nothing Then _rVal.Add(extLine2)
                If Arc1 IsNot Nothing Then _rVal.Add(Arc1)
                If Arc2 IsNot Nothing Then _rVal.Add(Arc2)
                If Arrow1 IsNot Nothing Then _rVal.Add(Arrow1)
                If Arrow2 IsNot Nothing Then _rVal.Add(Arrow2)
                For Each tent As dxfEntity In textEnts
                    If tent IsNot Nothing Then _rVal.Add(tent)
                Next
                If MText IsNot Nothing Then
                    aDim.DefPt11V = MText.AlignmentPt1V
                End If
                Dim aDS As dxfDisplaySettings = aDim.DisplaySettings
                If aDim.AngularDimensionType = dxxAngularDimTypes.Angular Then
                    _rVal.Add(New dxePoint(aDim.DefPt10V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt10"))
                    _rVal.Add(New dxePoint(aDim.DefPt13V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt13"))
                    _rVal.Add(New dxePoint(aDim.DefPt14V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt14"))
                    _rVal.Add(New dxePoint(aDim.DefPt15V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt15"))
                    _rVal.Add(New dxePoint(aDim.DefPt16V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt16"))
                Else
                    _rVal.Add(New dxePoint(aDim.DefPt10V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt10"))
                    _rVal.Add(New dxePoint(aDim.DefPt13V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt13"))
                    _rVal.Add(New dxePoint(aDim.DefPt14V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt14"))
                    _rVal.Add(New dxePoint(aDim.DefPt15V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt15"))
                End If
                'save these two vectors to the dimensions structure
                aDim.Properties.SetVal(50, 0) '"Angle of rotated, horizontal, or vertical dimensions"
                aDim.Properties.SetVector("*Vector1", ArcPt1)
                aDim.Properties.SetVector("*Vector2", ArcPt2)
                Return _rVal
            Catch ex As Exception
                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
                Return _rVal
            Finally

                aDim.SuppressEvents = False
            End Try
        End Function
        Friend Shared Function CreateLinear(aImage As dxfImage, aDim As dxeDimension) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            '^used internally by dxoDrawingTool.Dimension to draw a linear Dimension
            '~returns the dimensions block object
            Try
                Dim ELine1 As dxeLine = Nothing
                Dim ELine2 As dxeLine = Nothing
                Dim DLine1 As dxeLine = Nothing
                Dim DLine2 As dxeLine = Nothing
                Dim Arrow1 As dxfEntity = Nothing
                Dim Arrow2 As dxfEntity = Nothing
                Dim MText As dxeText = Nothing
                Dim dimPt As TVECTOR
                Dim v10 As TVECTOR
                Dim hcnt As Integer = 0
                Dim textEnts As colDXFEntities = Nothing
                Dim aDS As dxfDisplaySettings
                Dim wrkPl As New TPLANE(aDim.Plane)

                Dim SF As Double
                '======= initialize =========================
                aDim.SuppressEvents = True

                '========= EXTENSION LINES ======================
                dxfDimTool.ExtensionLines_Linear(aImage, aDim, SF, ELine1, ELine2, dimPt, v10, hcnt, wrkPl)
                '========= DIM LINES ======================
                dxfDimTool.DimLines_Linear(aImage, aDim, SF, dimPt, v10, DLine1, DLine2, hcnt, wrkPl)
                '========= TEXT ======================
                Try
                    textEnts = dxfDimTool.DimText_Linear(MText, aImage, aDim, SF, ELine1, ELine2, DLine1, DLine2, dimPt, v10, Arrow1, Arrow2, hcnt, wrkPl)
                Catch ex As Exception
                    xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)

                End Try
                '========= SET DXF ENTITY PROPERTIES ========================

                If ELine1 IsNot Nothing Then _rVal.Add(ELine1)
                If ELine2 IsNot Nothing Then _rVal.Add(ELine2)
                If DLine1 IsNot Nothing Then _rVal.Add(DLine1)
                If DLine2 IsNot Nothing Then _rVal.Add(DLine2)
                If Arrow1 IsNot Nothing Then _rVal.Add(Arrow1)
                If Arrow2 IsNot Nothing Then _rVal.Add(Arrow2)
                For Each tent As dxfEntity In textEnts
                    If tent IsNot Nothing Then _rVal.Add(tent)
                Next

                If MText IsNot Nothing Then aDim.DefPt11V = MText.AlignmentPt1V
                aDS = aDim.DisplaySettings
                _rVal.Add(New dxePoint(aDim.DefPt13V.ProjectedTo(wrkPl), aDS, "DefPoints", "DefPt13"))
                _rVal.Add(New dxePoint(aDim.DefPt14V.ProjectedTo(wrkPl), aDS, "DefPoints", "DefPt14"))
                _rVal.Add(New dxePoint(aDim.DefPt10V.ProjectedTo(wrkPl), aDS, "DefPoints", "DefPt10"))
                _rVal.Add(New dxePoint(aDim.DefPt11V.ProjectedTo(wrkPl), aDS, "DefPoints", "DefPt11"))

                'save these two vectors to the dimensions structure

                aDim.Properties.SetVector("*Vector1", dimPt)
                aDim.Properties.SetVector("*Vector2", v10)

            Catch ex As Exception
                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
            Finally
                aDim.SuppressEvents = False
            End Try
            Return _rVal
        End Function
        Friend Shared Function CreateOrdinate(aImage As dxfImage, aDim As dxeDimension) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            'On Error Resume Next
            '^used internally by dxoDrawingTool.Dimension to draw a ordinate Dimension
            '~returns the dimensions block object

            Dim ELine1 As dxeLine = Nothing
            Dim ELine2 As dxeLine = Nothing
            Dim ELine3 As dxeLine = Nothing
            Dim MText As dxeText = Nothing
            Dim bSingleLine As Boolean
            Dim hcnt As Integer = 0
            Dim aDS As dxfDisplaySettings
            Dim aPln As TPLANE = aDim.PlaneV

            Dim SF As Double
            '======= initialize =========================
            aDim.SuppressEvents = True

            '========= EXTENSION LINES ======================
            dxfDimTool.ExtensionLines_Ordinate(aImage, aDim, SF, ELine1, ELine2, ELine3, bSingleLine, hcnt)
            '========= TEXT ======================
            Dim textEnts As colDXFEntities = dxfDimTool.DimText_Ordinate(MText, aImage, aDim, SF, ELine1, ELine2, ELine3, bSingleLine, hcnt)
            '========= SET DXF ENTITY PROPERTIES ========================

            If ELine1 IsNot Nothing Then _rVal.Add(ELine1)
            If ELine2 IsNot Nothing Then _rVal.Add(ELine2)
            If ELine3 IsNot Nothing Then _rVal.Add(ELine3)
            For Each tent As dxfEntity In textEnts
                If tent IsNot Nothing Then _rVal.Add(tent)
            Next

            If MText IsNot Nothing Then aDim.DefPt11V = MText.AlignmentPt1V
            aDS = aDim.DisplaySettings

            _rVal.Add(New dxePoint(aDim.DefPt10V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt10"))
            _rVal.Add(New dxePoint(aDim.DefPt11V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt11"))
            _rVal.Add(New dxePoint(aDim.DefPt13V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt13"))
            _rVal.Add(New dxePoint(aDim.DefPt14V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt14"))

            aDim.SuppressEvents = False
            Return _rVal
        End Function
        Friend Shared Function CreateRadial(aImage As dxfImage, aDim As dxeDimension) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            'On Error Resume Next
            '^used internally by dxoDrawingTool.Dimension to draw a linear Dimension
            '~returns the dimensions block object
            Dim Arrow1 As dxfEntity = Nothing
            Dim MText As dxeText = Nothing
            Dim textEnts As colDXFEntities
            Dim DLine1 As dxeLine = Nothing
            Dim DLine2 As dxeLine = Nothing
            Dim aCenter As dxfVector = Nothing
            Dim cMarks As colDXFEntities = Nothing
            Dim minLeader As Double
            Dim SF As Double
            Dim bPopText As Boolean
            Dim hcnt As Integer = 0
            Dim aDS As dxfDisplaySettings
            Dim i As Integer
            Dim aPln As TPLANE = aDim.PlaneV
            Dim aEnt As dxfEntity
            '======= initialize =========================
            aDim.SuppressEvents = True

            'set the minimum leader length
            Dim s As TTABLEENTRY = aDim.Style
            SF = s.PropValueD(dxxDimStyleProperties.DIMSCALE)
            minLeader = s.PropValueD(dxxDimStyleProperties.DIMTSZ) * SF
            If minLeader <> 0 Then
                minLeader = Math.Sqrt(minLeader ^ 2 + minLeader ^ 2)
            Else
                minLeader = s.PropValueD(dxxDimStyleProperties.DIMASZ) * SF
                If minLeader > 0 Then minLeader = 2 * minLeader Else minLeader = 0.18 * SF
            End If
            '========= CENTER MARKS ======================
            cMarks = dxfDimTool.MakeCenterMarks(aDim, SF, aCenter)
            '========= DIM LINES ======================
            bPopText = dxfDimTool.DimLines_Radial(aImage, aDim, SF, DLine1, aCenter, minLeader, hcnt)
            '    ========= TEXT ======================
            textEnts = dxfDimTool.DimText_Radial(MText, aImage, aDim, SF, DLine1, DLine2, aCenter, bPopText, minLeader, Arrow1, hcnt)
            '========= SET DXF ENTITY PROPERTIES ========================

            If bPopText Then
                For i = 1 To cMarks.Count
                    aEnt = cMarks.Item(i)
                    If aEnt IsNot Nothing Then _rVal.Add(aEnt)
                Next i
            End If
            If DLine1 IsNot Nothing Then
                DLine1.Identifier = "Dimension.Line1"
                _rVal.Add(DLine1)
            End If
            If DLine2 IsNot Nothing Then
                DLine2.Identifier = "Dimension.Line2"
                _rVal.Add(DLine2)
            End If
            If Arrow1 IsNot Nothing Then _rVal.Add(Arrow1)
            For Each tent As dxfEntity In textEnts
                If tent IsNot Nothing Then _rVal.Add(tent)
            Next
            If MText IsNot Nothing Then
                aDim.DefPt11V = MText.AlignmentPt1V
            End If
            aDS = aDim.DisplaySettings

            _rVal.Add(New dxePoint(aDim.DefPt10V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt10"))
            _rVal.Add(New dxePoint(aDim.DefPt11V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt11"))
            _rVal.Add(New dxePoint(aDim.DefPt15V.ProjectedTo(aPln), aDS, "DefPoints", "DefPt15"))

            aDim.SuppressEvents = False
            Return _rVal
        End Function
        Friend Shared Sub DimLines_Angular(aImage As dxfImage, aDim As dxeDimension, aScaleFactor As Double, aArcPt1 As TVECTOR, aArcPt2 As TVECTOR, ByRef rDimArc1 As dxeArc, ByRef rDimArc2 As dxeArc, aAngle As Double, aAngleArc As dxeArc, aIntersectPt As TVECTOR)
            'On Error Resume Next
            '^used internally by dxoDrawingTool.aDimensionAngular to draw a linear angular
            '~returns the dimensions block object


            '======= initialize =========================
            rDimArc1 = New dxeArc
            rDimArc2 = New dxeArc
            aDim.Angle = aAngleArc.SpannedAngle
            rDimArc1.CopyDisplayProps(aDim)
            rDimArc2.CopyDisplayProps(aDim)
            'intialize the dim arcs
            Dim p As TPROPERTIES = aDim.Style.Props
            Dim dClr As dxxColors = aDim.Style.PropValueI(dxxDimStyleProperties.DIMCLRD)
            Dim arSize As Double = aDim.Style.PropValueD(dxxDimStyleProperties.DIMASZ) * aScaleFactor
            rDimArc1.Color = dClr
            rDimArc2.Color = dClr
            rDimArc1.Suppressed = aDim.Style.PropValueB(dxxDimStyleProperties.DIMSD1)
            rDimArc2.Suppressed = aDim.Style.PropValueB(dxxDimStyleProperties.DIMSD2)
            rDimArc1.Identifier = "Dimension.Arc1"
            rDimArc2.Identifier = "Dimension.Arc2"
            rDimArc1.Linetype = aDim.Style.PropValueStr(dxxDimStyleProperties.DIMLTYPE_NAME)
            rDimArc2.Linetype = aDim.Style.PropValueStr(dxxDimStyleProperties.DIMLTYPE_NAME)
            Dim ip As TVECTOR = aIntersectPt
            Dim ap1 As TVECTOR = aArcPt1
            Dim ap2 As TVECTOR = aArcPt2
            Dim rad As Double = dxfProjections.DistanceTo(ip, ap1)
            If rad = 0 Then Return
            Dim bCW As Boolean = aAngleArc.ClockWise
            Dim aCS As TPLANE = aDim.PlaneV
            aCS.Origin = aIntersectPt
            '========= DIM ARCS ======================
            Dim arLen As Double
            If arSize > 0 Then arLen = 2 * arSize Else arLen = 0.09 * aScaleFactor * 2
            Dim arAng As Double = (arLen / rad) * 180 / Math.PI
            If bCW Then arAng = -arAng
            If 2 * Math.Abs(arAng) > aAngle Then
                arAng = -arAng
                bCW = Not bCW
            End If
            Dim P1 As TVECTOR = ap1.RotatedAbout(aCS.Origin, aCS.ZDirection, arAng, False)
            rDimArc1.ArcStructure = TARC.DefineWithPoints(aCS, ip, ap1, P1, bCW)
            P1 = ap2.RotatedAbout(aCS.Origin, aCS.ZDirection, -arAng, False)
            rDimArc2.ArcStructure = TARC.DefineWithPoints(aCS, ip, ap2, P1, Not bCW)
        End Sub
        Friend Shared Sub DimLines_Linear(aImage As dxfImage, aDim As dxeDimension, aScaleFactor As Double, aDimLinePt1 As TVECTOR, aDimLinePt2 As TVECTOR, ByRef rDimLine1 As dxeLine, ByRef rDimLine2 As dxeLine, ByRef hcnt As Integer, aWorkPlane As TPLANE)
            'On Error Resume Next
            '^creates the dimension lines for a linear dimension

            Dim dClr As dxxColors
            Dim arSize As Double
            Dim tkSize As Double
            Dim lng1 As Double
            Dim lng2 As Double
            Dim v10 As TVECTOR = aDimLinePt2
            Dim dimPt As TVECTOR = aDimLinePt1
            Dim Span As Double = v10.DistanceTo(dimPt)
            Dim dimL1 As New TLINE
            Dim dimL2 As New TLINE
            Dim aDir As TVECTOR
            Dim bDir As TVECTOR
            Dim f1 As Double
            Dim f2 As Double
            Dim hJust As dxxDimJustSettings
            Dim bname1 As String
            Dim bname2 As String
            '======= initialize =========================

            dxfArrowheads.GetArrowHeads(aImage, aDim.DimStyle)
            hJust = aDim.Style.PropValueI(dxxDimStyleProperties.DIMJUST)
            bname1 = aDim.Style.PropValueStr(dxxDimStyleProperties.DIMBLK1_NAME)
            bname2 = aDim.Style.PropValueStr(dxxDimStyleProperties.DIMBLK2_NAME)
            dClr = aDim.Style.PropValueI(dxxDimStyleProperties.DIMCLRD)
            arSize = aDim.Style.PropValueD(dxxDimStyleProperties.DIMASZ) * aScaleFactor
            tkSize = aDim.Style.PropValueD(dxxDimStyleProperties.DIMTSZ) * aScaleFactor

            '========= DIM LINES ======================
            aDir = aWorkPlane.XDirection
            bDir = (aDir * -1)
            dimL1.SPT = dimPt
            dimL2.SPT = v10
            If tkSize > 0 Then
                lng1 = 2 * tkSize
                lng2 = lng1
                arSize = tkSize
            Else
                f1 = 2
                f2 = 2
                If bname1 = "_SMALL" Or bname1 = "_ARCHTICK" Or bname1 = "_OBLIQUE" Or bname1 = "_DOTSMALL" Or bname1 = "_INTEGRAL" Then
                    f1 = 1
                End If
                If bname2 = "_SMALL" Or bname2 = "_ARCHTICK" Or bname2 = "_OBLIQUE" Or bname2 = "_DOTSMALL" Or bname2 = "_INTEGRAL" Then
                    f2 = 1
                End If
                If arSize > 0 Then lng1 = arSize Else lng1 = 0.18 * aScaleFactor
                lng2 = f2 * lng1
                lng1 = f1 * lng1
            End If

            dimL1.EPT = dimPt + aDir * lng1
            dimL2.EPT = v10 + bDir * lng2
            rDimLine1 = New dxeLine(dimL1, aDisplaySettings:=aDim.DisplaySettings) With
        {
        .Suppressed = aDim.Style.PropValueB(dxxDimStyleProperties.DIMSD1) And hJust <> dxxDimJustSettings.AlignExt1 And hJust <> dxxDimJustSettings.AlignExt2,
         .Linetype = aDim.Style.PropValueStr(dxxDimStyleProperties.DIMLTYPE_NAME),
         .Color = dClr,
         .Identifier = "Dimension.Line1"
            }
            rDimLine2 = New dxeLine(dimL2, aDisplaySettings:=aDim.DisplaySettings) With
        {
             .Suppressed = aDim.Style.PropValueB(dxxDimStyleProperties.DIMSD2) And hJust <> dxxDimJustSettings.AlignExt1 And hJust <> dxxDimJustSettings.AlignExt2,
         .Linetype = rDimLine1.Linetype,
         .Color = dClr,
         .Identifier = "Dimension.Line2"
            }
        End Sub
        Friend Shared Function DimLines_Radial(aImage As dxfImage, aDim As dxeDimension, aScaleFactor As Double, ByRef rDimLine1 As dxeLine, aCenter As dxfVector, ByRef minLeader As Double, ByRef hcnt As Integer) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            _rVal = False
            Dim dClr As dxxColors
            Dim d1 As Double
            Dim ldrLen As Double
            Dim aDir As TVECTOR
            Dim bDir As TVECTOR
            Dim DimLineDirection1 As TVECTOR
            Dim rad As Double
            Dim ppt As TVECTOR
            Dim tpt As TVECTOR
            Dim tangLine As dxeLine
            Dim aPln As TPLANE = aDim.PlaneV.Clone
            Dim cp As TVECTOR
            Dim aStyle As TTABLEENTRY = aDim.Style
            cp = aCenter.Strukture
            rDimLine1 = Nothing
            '======= initialize =========================
            If aImage Is Nothing Then aImage = dxfGlobals.New_Image()
            dClr = aStyle.PropValueI(dxxDimStyleProperties.DIMCLRD)
            tpt = aDim.DefPt11V.ProjectedTo(aPln)
            ppt = aDim.DefPt15V.ProjectedTo(aPln)
            rad = Math.Round(aDim.Radius, 4)
            ldrLen = ppt.DistanceTo(tpt)
            aDir = cp.DirectionTo(ppt)
            'see if we are leadering in or out
            _rVal = cp.DistanceTo(tpt) >= cp.DistanceTo(ppt)
            If _rVal Then
                If ldrLen < minLeader Then tpt = ppt + (aDir * minLeader)
            Else
                If ldrLen < minLeader Then tpt = ppt + (aDir * minLeader)
                d1 = cp.DistanceTo(tpt)
                If d1 >= rad Then
                    _rVal = True
                    tpt = ppt + (aDir * minLeader)
                End If
            End If
            DimLineDirection1 = ppt.DirectionTo(tpt)
            d1 = Math.Abs(rad - ppt.DistanceTo(tpt))
            bDir = aDir.RotatedAbout(aDim.PlaneV.ZDirection, 90, False, True)
            tangLine = New dxeLine(ppt, ppt + (bDir * rad))
            d1 = ppt.DistanceTo(tpt)
            '========= DIM LINES ======================
            rDimLine1 = New dxeLine(ppt, ppt + (DimLineDirection1 * d1), aDisplaySettings:=aDim.DisplaySettings) With {
        .Color = dClr,
        .Suppressed = aStyle.PropValueB(dxxDimStyleProperties.DIMSD1),
        .Linetype = aStyle.PropValueStr(dxxDimStyleProperties.DIMLTYPE_NAME),
        .Identifier = "Dimension.Line1"}
            Return _rVal
        End Function
        Friend Shared Function DimText_Angular(ByRef rMText As dxeText, aImage As dxfImage, aDim As dxeDimension, aScaleFactor As Double, aAngle As Double, aAngleArc As dxeArc, ByRef extLine1 As dxeLine, ByRef extLine2 As dxeLine, dimArc1 As dxeArc, dimArc2 As dxeArc, ByRef ArcPt1 As TVECTOR, ByRef ArcPt2 As TVECTOR, aArrow1 As dxfEntity, aArrow2 As dxfEntity, ByRef IntersectPt As TVECTOR) As colDXFEntities
            '^used internally create the text entities for a linear Dimension
            Dim _rVal As New colDXFEntities()
            Try
#Region "Declares"
                Dim aStyle As TTABLEENTRY = aDim.Style
                Dim aRect As dxfRectangle = Nothing
                Dim DSP As dxfDisplaySettings = aDim.DisplaySettings
                Dim iPts As New TVECTORS(0)
                Dim rLines As colDXFEntities
                Dim wLine As dxeLine = Nothing
                Dim aInsert As dxeInsert
                Dim aHook As dxeArc = Nothing
                Dim bHook As dxeLine = Nothing
                Dim txtArc As dxeArc = Nothing
                Dim fitArc As dxeArc = Nothing
                Dim aSolid As dxeSolid = Nothing
                Dim iArc As dxeArc = Nothing
                Dim dArc1 As dxeArc = Nothing
                Dim dArc2 As dxeArc = Nothing
                Dim txtBox As dxePolyline
                If aScaleFactor <= 0 Then aScaleFactor = 1
                Dim dClr As dxxColors = aStyle.PropValueI(dxxDimStyleProperties.DIMCLRD)
                Dim tht As Double = aStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * aScaleFactor
                Dim tgap As Double = aStyle.PropValueD(dxxDimStyleProperties.DIMGAP) * aScaleFactor
                Dim vjust As dxxDimTadSettings = aStyle.PropValueD(dxxDimStyleProperties.DIMTAD)
                Dim hJust As dxxDimJustSettings = aStyle.PropValueI(dxxDimStyleProperties.DIMJUST)
                Dim aFit As dxxDimTextFitTypes = aStyle.PropValueI(dxxDimStyleProperties.DIMATFIT)
                Dim asz As Double = aStyle.PropValueD(dxxDimStyleProperties.DIMASZ) * aScaleFactor
                Dim tang As Double
                Dim bCW As Boolean = aAngleArc.ClockWise
                Dim rad As Double = aAngleArc.Radius
                Dim mpt As TVECTOR = aAngleArc.MidPtV
                Dim txtLength As Double = 0
                Dim pdist As Double
                Dim oset As Double
                Dim txtHeight As Double = 0
                Dim i As Integer
                Dim PopText As Boolean
                Dim PopArrows As Boolean
                Dim bOverArcI As Boolean
                Dim j As Integer
                Dim bArrowsIn As Boolean
                Dim bTextFits As Boolean
                Dim bArrowsFit As Boolean
                Dim bTextCrash As Boolean
                Dim bArrowCrash As Boolean
                Dim bTextFitsWithoutArrows As Boolean
                Dim bArrowsFitsWithoutText As Boolean
                Dim bMergeArcs As Boolean
                Dim txtRot As Double
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim aDir As TVECTOR
                Dim aOCS As TPLANE = aDim.PlaneV
                Dim txtDir As TVECTOR
                Dim xDir As TVECTOR = aOCS.XDirection
                Dim zDir As TVECTOR = aOCS.ZDirection
                Dim TextPt As TVECTOR
                Dim prjPt As TVECTOR
                Dim aLn As New TLINE
                Dim bTIH As Boolean = aStyle.PropValueB(dxxDimStyleProperties.DIMTIH)
                Dim bTOH As Boolean = aStyle.PropValueB(dxxDimStyleProperties.DIMTOH)
                Dim span As Double = 0
                Dim dtxt As String = String.Empty
#End Region 'Declares
                txtRot = aDim.TextRotation
                'set the arrowheads
                aArrow1 = dxfArrowheads.GetEntity(aStyle, aOCS, aImage, dxxArrowIndicators.One, dimArc1, "ArrowHead.1", True)
                aArrow2 = dxfArrowheads.GetEntity(aStyle, aOCS, aImage, dxxArrowIndicators.Two, dimArc2, "ArrowHead.2", True)
                aDim.Angle = aAngleArc.SpannedAngle
                '_rVal.Add(aAngleArc)
                '_rVal.Add(New dxePoint(mpt))
                bArrowsIn = aAngle > 0 And dimArc1.ClockWise = bCW
                '+++++++++++++++++++++Special case++++++++++++++++++++++++++++++++++++++++++++++
                If (hJust = dxxDimJustSettings.AlignExt1 Or hJust = dxxDimJustSettings.AlignExt2) Then
                    PopText = True
                    pdist = aStyle.PropValueD(dxxDimStyleProperties.DIMEXE) * aScaleFactor
                    If hJust = dxxDimJustSettings.AlignExt1 Then
                        j = -1 : aLn = New TLINE(IntersectPt, ArcPt1) + (IntersectPt.DirectionTo(ArcPt1) * pdist)
                    Else
                        j = 1 : aLn = New TLINE(IntersectPt, ArcPt2) + (IntersectPt.DirectionTo(ArcPt2) * pdist)
                    End If
                    wLine = New dxeLine(aLn)
                    TextPt = aLn.EPT : txtDir = aLn.Direction
                    If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                    tang = xDir.AngleTo(txtDir, zDir, 3)
                    If tang > 90 And tang <= 270 Then
                        txtDir *= -1 : tang = xDir.AngleTo(txtDir, zDir, 3)
                    End If
                    aDir = aLn.Direction
                    '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    rMText = dxfDimTool.MakeText(aDim, aImage, TextPt, tang, True, span, dtxt, aRect, 2 * Math.Abs(tgap), txtHeight, txtLength)
                    'get the bounds of the text
                    If iArc Is Nothing Then iArc = aAngleArc
                    iPts = aRect.IntersectionPts(iArc.ArcStructure, True, True, True)
                    If txtArc Is Nothing Then
                        If iPts.Count > 0 Then
                            v1 = iPts.Nearest(ArcPt1, TVECTOR.Zero)
                            v2 = iPts.Farthest(ArcPt1, TVECTOR.Zero)
                            txtArc = New dxeArc(IntersectPt, v1, v2, False, aOCS)
                        End If
                    End If
                    '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    prjPt = TextPt + aDir * (0.5 * txtLength)
                    If vjust <> dxxDimTadSettings.Centered Then
                        aDir.RotateAbout(zDir, j * 90)
                        pdist = 0.5 * txtHeight
                        If (tgap < 0) Then pdist += Math.Abs(tgap)
                        prjPt += aDir * pdist
                        If hJust = dxxDimJustSettings.AlignExt1 Then wLine = extLine1 Else wLine = extLine2
                        If Not wLine.Suppressed Then wLine.Extend(txtLength - Math.Abs(tgap))
                    End If
                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    ''MoveTextEntities:
                    TextPt = prjPt
                    If rMText IsNot Nothing Then rMText.Translate(prjPt - rMText.InsertionPtV)
                    If aRect IsNot Nothing Then
                        aRect.CenterV = prjPt
                        rLines = aRect.BorderLines(aDisplaySettings:=aDim.DisplaySettings)
                        For i = 1 To rLines.Count
                            wLine = rLines.Item(i)
                            wLine.DisplaySettings = aDim.DisplaySettings
                            wLine.Color = dClr
                        Next i
                    End If
                Else
                    '1111set the inside text angle1111
                    If Not bTIH Then
                        txtDir = IntersectPt.DirectionTo(mpt).RotatedAbout(zDir, 90)
                    Else
                        txtDir = xDir
                    End If
                    If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    ''SetTextAngle:
                    tang = xDir.AngleTo(txtDir, zDir, 2)
                    If tang = 0 Or tang = 360 Or tang = 180 Then tang = 0
                    If tang > 90 And tang <= 270 Then
                        txtDir *= -1
                        tang = xDir.AngleTo(txtDir, zDir, 2)
                    End If
                    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    '11111111111111111111111111111111111111111111111111111111
                    '22222222222222 CREATE THE TEXT INSIDE 22222222222222222
                    'If iArc  Is Nothing Then iArc = aAngleArc
                    'TextPt = aDim.DefPt15V.ProjectedTo(aDim.PlaneV)
                    TextPt = mpt
                    'If TextPt.DistanceTo(IntersectPt, 3) <> Math.Round(aAngleArc.Radius, 3) Then
                    'TextPt = mpt
                    'End If
                    '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    rMText = dxfDimTool.MakeText(aDim, aImage, TextPt, tang, True, span, dtxt, aRect, 2 * Math.Abs(tgap), txtHeight, txtLength)
                    'get the bounds of the text
                    If iArc Is Nothing Then iArc = aAngleArc
                    iPts = aRect.IntersectionPts(iArc.ArcStructure, True)
                    If txtArc Is Nothing Then
                        If iPts.Count > 0 Then
                            v1 = iPts.Nearest(ArcPt1, TVECTOR.Zero)
                            v2 = iPts.Farthest(ArcPt1, TVECTOR.Zero)
                            txtArc = New dxeArc(IntersectPt, v1, v2, False, aOCS)
                        End If
                    End If
                    '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    If bOverArcI Then
                        pdist = 0.5 * txtHeight
                        If (tgap < 0) Then pdist += Math.Abs(tgap)
                        TextPt += IntersectPt.DirectionTo(TextPt) * pdist
                        iArc = aAngleArc.Clone
                        iArc.Radius = IntersectPt.DistanceTo(TextPt)
                        fitArc = iArc.Clone
                        txtArc = Nothing
                        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        ''CreateDimText:
                        rMText = dxfDimTool.MakeText(aDim, aImage, TextPt, tang, True, span, dtxt, aRect, 2 * Math.Abs(tgap), txtHeight, txtLength)
                        If iArc Is Nothing Then iArc = aAngleArc
                        iPts = aRect.IntersectionPts(iArc.ArcStructure, True)
                        If txtArc Is Nothing Then
                            If iPts.Count > 0 Then
                                v1 = iPts.Nearest(ArcPt1, TVECTOR.Zero)
                                v2 = iPts.Farthest(ArcPt1, TVECTOR.Zero)
                                txtArc = New dxeArc(IntersectPt, v1, v2, False, aOCS)
                            End If
                        End If
                        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    End If
                    'builde the inside fit arc
                    If Not bOverArcI Then
                        v1 = dimArc1.EndPtV
                        v2 = dimArc2.EndPtV
                        If Not bArrowsIn Then
                            v1 = ArcPt1
                            v2 = ArcPt2
                        End If
                        fitArc = New dxeArc(v1, mpt, v2, True, aOCS)  'dxfPrimatives.ArcThreePoint(v1, mpt, v2, True, aOCS)
                    End If
                    '2222222222222222222222222222222222222222222222222222222222222222
                    '33333333333333333 DECIDE WHAT FITS 33333333333333333333333333333
                    bArrowsFit = aAngle > 0 And dimArc1.ClockWise = bCW
                    If bArrowsIn And Not bArrowsFit Then
                        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        ''SwapArrows:
                        If Not bOverArcI Then fitArc = New dxeArc(ArcPt1, mpt, v2, True, aOCS) 'dxfPrimatives.ArcThreePoint(ArcPt1, mpt, dimArc2.CenterV, True, aOCS)
                        If bArrowsIn Then
                            dimArc1.Mirror(extLine1)
                            'Return
                            If aArrow1 IsNot Nothing Then
                                If aArrow1.GraphicType = dxxGraphicTypes.Solid Then
                                    aSolid = aArrow1
                                    aSolid.Mirror(extLine1)
                                Else
                                    If aArrow1.GraphicType = dxxGraphicTypes.Insert Then
                                        aInsert = aArrow1
                                        wLine = New dxeLine(ArcPt1, dimArc1.StartPtV)
                                        If Math.Round(wLine.Length, 2) > 0 Then
                                            aInsert.RotationAngle = xDir.AngleTo(wLine.DirectionV, zDir)
                                        End If
                                    End If
                                End If
                            End If
                            dimArc2.Mirror(extLine2)
                            If aArrow2 IsNot Nothing Then
                                If aArrow2.GraphicType = dxxGraphicTypes.Solid Then
                                    aSolid = aArrow2
                                    aSolid.Mirror(extLine2)
                                Else
                                    If aArrow2.GraphicType = dxxGraphicTypes.Insert Then
                                        aInsert = aArrow2
                                        wLine = New dxeLine
                                        wLine.SetVectors(ArcPt2, ArcPt2)
                                        wLine.StartPtV = dimArc2.StartPtV
                                        If Math.Round(wLine.Length, 2) > 0 Then
                                            aInsert.RotationAngle = xDir.AngleTo(wLine.DirectionV, zDir)
                                        End If
                                    End If
                                End If
                            End If
                            fitArc = dxfPrimatives.CreateArcThreePoint(ArcPt1, mpt, ArcPt2, True, aOCS)
                            bArrowsIn = False
                            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        End If
                    End If
                    bTextFits = False
                    If txtArc IsNot Nothing Then
                        bTextFits = txtArc.Length <= fitArc.Length
                    End If
                    If aStyle.PropValueB(dxxDimStyleProperties.DIMTIX) Then 'ForceTextBetweenExtLine
                        bTextFits = True
                    Else
                        iPts = aRect.IntersectionPts(New TLINE(extLine1), True)
                        bTextCrash = iPts.Count > 0
                        If Not bTextCrash Then
                            iPts = aRect.IntersectionPts(New TLINE(extLine2), True)
                            bTextCrash = iPts.Count > 0
                        End If
                        If bTextCrash Then bTextFits = False
                    End If
                    If Not bOverArcI Then
                        If bArrowsFit And bTextFits Then
                            bArrowCrash = aRect.IntersectionPts(dimArc1.ArcStructure).Count > 0
                        End If
                        If bArrowsFit And bTextFits And Not bArrowCrash Then
                            bArrowCrash = aRect.IntersectionPts(dimArc2.ArcStructure).Count > 0
                        End If
                    End If
                    If bArrowCrash Then bArrowsFit = False
                    '3333333333333333333333333333333333333333333333333333333333333333
                    '44444444444444444 DECIDE WHAT TO POP 44444444444444444444444444
                    If aStyle.PropValueB(dxxDimStyleProperties.DIMTIX) Then
                        bTextFits = True
                        PopText = False
                        If Not bArrowsFit Then PopArrows = True
                    Else
                        If Not bArrowsFit Or Not bTextFits Then
                            If aFit = dxxDimTextFitTypes.MoveTextAndArrows Then
                                PopText = True
                                PopArrows = True
                            Else
                                bTextFitsWithoutArrows = False
                                If Not bTextFits Then
                                    If bArrowsIn And Not bOverArcI Then
                                        If txtArc IsNot Nothing Then
                                            If txtArc.Length <= aAngleArc.Length Then
                                                'will the text will fit without the arrows?
                                                bTextFitsWithoutArrows = Not bTextCrash
                                            End If
                                        End If
                                    End If
                                End If
                                bArrowsFitsWithoutText = False
                                If Not bArrowsFit Then
                                    If bArrowsIn Then
                                        'will the arrows  fit without the text?
                                        bArrowsFitsWithoutText = Not bArrowCrash
                                    End If
                                End If
                                If aFit = dxxDimTextFitTypes.BestFit Then
                                    If bTextFitsWithoutArrows Then
                                        bTextFits = True
                                        bArrowsFit = False
                                    ElseIf bArrowsFitsWithoutText Then
                                        bArrowsFit = True
                                        bTextFits = False
                                    End If
                                ElseIf aFit = dxxDimTextFitTypes.MoveTextFirst Then
                                    If bArrowsFitsWithoutText Then
                                        bArrowsFit = True
                                        bTextFits = False
                                    End If
                                ElseIf aFit = dxxDimTextFitTypes.MoveArrowsFirst Then
                                    If bTextFitsWithoutArrows Then
                                        bTextFits = True
                                        bArrowsFit = False
                                    End If
                                End If
                                If Not bTextFits Then PopText = True
                                If Not bArrowsFit Then PopArrows = True
                            End If
                        End If
                    End If
                    If PopArrows And bArrowsIn Then
                        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        ''SwapArrows:
                        If Not bOverArcI Then fitArc = dxfPrimatives.CreateArcThreePoint(ArcPt1, mpt, dimArc2.CenterV, True, aOCS)
                        If bArrowsIn Then
                            dimArc1.Mirror(extLine1)
                            If aArrow1 IsNot Nothing Then
                                If aArrow1.GraphicType = dxxGraphicTypes.Solid Then
                                    aSolid = aArrow1
                                    aSolid.Mirror(extLine1)
                                Else
                                    If aArrow1.GraphicType = dxxGraphicTypes.Insert Then
                                        aInsert = aArrow1
                                        wLine = New dxeLine
                                        wLine.ConvergeToV(ArcPt1)
                                        wLine.StartPtV = dimArc1.StartPtV
                                        If Math.Round(wLine.Length, 2) > 0 Then
                                            aInsert.RotationAngle = xDir.AngleTo(wLine.DirectionV, zDir)
                                        End If
                                    End If
                                End If
                            End If
                            dimArc2.Mirror(extLine2)
                            If aArrow2 IsNot Nothing Then
                                If aArrow2.GraphicType = dxxGraphicTypes.Solid Then
                                    aSolid = aArrow2
                                    aSolid.Mirror(extLine2)
                                Else
                                    If aArrow2.GraphicType = dxxGraphicTypes.Insert Then
                                        aInsert = aArrow2
                                        wLine = New dxeLine
                                        wLine.SetVectors(ArcPt2, ArcPt2)
                                        wLine.StartPtV = dimArc2.StartPtV
                                        If Math.Round(wLine.Length, 2) > 0 Then
                                            aInsert.RotationAngle = xDir.AngleTo(wLine.DirectionV, zDir)
                                        End If
                                    End If
                                End If
                            End If
                            fitArc = dxfPrimatives.CreateArcThreePoint(ArcPt1, mpt, ArcPt2, True, aOCS)
                            bArrowsIn = False
                            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        End If
                    End If
                    '44444444444444444444444444444444444444444444444444444444444444444
                    '55555555555555555 MOVE TEXT OUTSIDE 5555555555555555555555555555
                    If PopText Then
                        If bArrowsIn = True Then
                            oset = asz
                            If bTOH Then
                                aDir = xDir
                                txtDir = aDir
                                If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                            End If
                            If asz = 0 Then
                                oset = 0.1
                                bHook = New dxeLine
                                bHook.SetVectors(ArcPt1, ArcPt1)
                                wLine = aAngleArc.GetTangentLine(aAngleArc.StartAngle, 1)
                                If Not bTOH Then
                                    aDir = IntersectPt.DirectionTo(ArcPt1)
                                    aDir.RotateAbout(zDir, 90)
                                    txtDir = aDir
                                    If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                                    TextPt = ArcPt1
                                End If
                            Else
                                aHook = dimArc1.Clone
                                If bTOH Then
                                    bHook = New dxeLine
                                    wLine = aHook.GetTangentLine(aHook.EndAngle, -1)
                                    bHook.ConvergeTo(aHook.EndPt)
                                End If
                            End If
                        ElseIf bArrowsIn = False Then
                            bMergeArcs = True
                            oset = asz
                            If bTOH Then
                                aDir = xDir
                                txtDir = aDir
                                If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                            End If
                            If asz = 0 Then
                                oset = 0.1
                                bHook = New dxeLine
                                bHook.SetVectors(ArcPt1, ArcPt1)
                                wLine = aAngleArc.GetTangentLine(aAngleArc.StartAngle, 1)
                                If Not bTOH Then
                                    aDir = IntersectPt.DirectionTo(ArcPt1)
                                    aDir.RotateAbout(zDir, 90)
                                    txtDir = aDir
                                    TextPt = ArcPt1
                                    If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                                End If
                            Else
                                v1 = ArcPt1
                                v2 = v1
                                v2.RotateAbout(IntersectPt, zDir, -(asz / rad), True)
                                aHook = New dxeArc(IntersectPt, v1, v2, Not dimArc1.ClockWise, aOCS)
                                If bTOH Then
                                    bHook = New dxeLine
                                    wLine = aHook.GetTangentLine(aHook.EndAngle, -1)
                                    bHook.ConvergeTo(aHook.EndPt)
                                End If
                            End If
                        End If
                        'build the hooks and place the text
                        If bHook Is Nothing Then
                            If aHook IsNot Nothing Then
                                TextPt = aHook.EndPtV
                                If aHook.ClockWise Then j = 1 Else j = -1
                                wLine = aHook.GetTangentLine(aHook.EndAngle, j)
                                aDir = wLine.DirectionV
                                txtDir = aDir
                                If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                ''SetTextAngle:
                                tang = xDir.AngleTo(txtDir, zDir, 2)
                                If tang = 0 Or tang = 360 Or tang = 180 Then tang = 0
                                If tang > 90 And tang <= 270 Then
                                    txtDir *= -1
                                    tang = xDir.AngleTo(txtDir, zDir, 2)
                                End If
                                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                TextPt = aHook.EndPtV + (aDir * 0.5 * txtLength)
                                If vjust <> dxxDimTadSettings.Centered Then
                                    bHook = New dxeLine
                                    bHook.ConvergeToV(aHook.EndPtV)
                                    bHook.Transform(TTRANSFORM.CreateProjection(aDir, txtLength, True))
                                    pdist = 0.5 * txtHeight
                                    If (tgap < 0) Then pdist += Math.Abs(tgap)
                                    aDir.RotateAbout(zDir, 90)
                                    TextPt += aDir * pdist
                                End If
                                '.Add wLine.Clone
                                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                ''CreateDimText:
                                rMText = dxfDimTool.MakeText(aDim, aImage, TextPt, tang, True, span, dtxt, aRect, 2 * Math.Abs(tgap), txtHeight, txtLength)
                                If iArc Is Nothing Then iArc = aAngleArc
                                iPts = aRect.IntersectionPts(iArc.ArcStructure, True)
                                If txtArc Is Nothing Then
                                    If iPts.Count > 0 Then
                                        v1 = iPts.Nearest(ArcPt1, TVECTOR.Zero)
                                        v2 = iPts.Farthest(ArcPt1, TVECTOR.Zero)
                                        txtArc = New dxeArc(IntersectPt, v1, v2, False, aOCS)
                                    End If
                                End If
                                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            End If
                        Else
                            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            ''SetTextAngle:
                            tang = xDir.AngleTo(txtDir, zDir, 2)
                            If tang = 0 Or tang = 360 Or tang = 180 Then tang = 0
                            If tang > 90 And tang <= 270 Then
                                txtDir *= -1
                                tang = xDir.AngleTo(txtDir, zDir, 2)
                            End If
                            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            bHook.EndPt.Project(aDir, oset)
                            pdist = wLine.Direction.AngleTo(bHook.Direction)
                            If pdist < 90 Or pdist >= 270 Then bHook.EndPt.Project(bHook.Direction, -2 * oset)
                            aDir = bHook.DirectionV
                            TextPt = bHook.EndPtV + aDir * (0.5 * txtLength)
                            If vjust <> dxxDimTadSettings.Centered Then
                                pdist = 0.5 * txtHeight
                                If (tgap < 0) Then pdist += Math.Abs(tgap)
                                aDir.RotateAbout(zDir, 90)
                                TextPt += aDir * pdist
                                bHook.Extend(txtLength - Math.Abs(tgap))
                            End If
                            '.Add wLine.Clone
                            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            ''CreateDimText:
                            rMText = dxfDimTool.MakeText(aDim, aImage, TextPt, tang, True, span, dtxt, aRect, 2 * Math.Abs(tgap), txtHeight, txtLength)
                            If iArc Is Nothing Then iArc = aAngleArc
                            iPts = aRect.IntersectionPts(iArc.ArcStructure, True)
                            If txtArc Is Nothing Then
                                If iPts.Count > 0 Then
                                    v1 = iPts.Nearest(ArcPt1, TVECTOR.Zero)
                                    v2 = iPts.Farthest(ArcPt1, TVECTOR.Zero)
                                    txtArc = New dxeArc(IntersectPt, v1, v2, False, aOCS)
                                End If
                            End If
                            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            If asz = 0 Then bHook = Nothing
                            'If aHook.Suppressed Then aHook = Nothing
                        End If
                    End If
                    '55555555555555555555555555555555555555555555555555555555555555555
                    If hJust = dxxDimJustSettings.Ext1 Or hJust = dxxDimJustSettings.Ext2 Then
                        If Not PopText Then
                            If hJust = dxxDimJustSettings.Ext1 Then
                                TextPt = dimArc2.EndPtV
                                j = -1
                            Else
                                TextPt = dimArc1.EndPtV
                                j = 1
                            End If
                            TextPt.RotateAbout(IntersectPt, zDir, j * 0.5 * txtArc.SpannedAngle)
                            aDir = IntersectPt.DirectionTo(TextPt)
                            If Not bTIH Then
                                If vjust <> dxxDimTadSettings.Centered Then
                                    pdist = 0.5 * txtHeight
                                    If (tgap < 0) Then pdist += Math.Abs(tgap)
                                    TextPt += aDir * pdist
                                End If
                                txtDir = aDir
                                txtDir.RotateAbout(zDir, 90)
                                If txtRot <> 0 Then txtDir.RotateAbout(zDir, txtRot)
                                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                ''SetTextAngle:
                                tang = xDir.AngleTo(txtDir, zDir, 2)
                                If tang = 0 Or tang = 360 Or tang = 180 Then tang = 0
                                If tang > 90 And tang <= 270 Then
                                    txtDir *= -1
                                    tang = xDir.AngleTo(txtDir, zDir, 2)
                                End If
                                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            End If
                            txtArc = Nothing
                            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            ''CreateDimText:
                            rMText = dxfDimTool.MakeText(aDim, aImage, TextPt, tang, True, span, dtxt, aRect, 2 * Math.Abs(tgap), txtHeight, txtLength)
                            If iArc Is Nothing Then iArc = aAngleArc
                            iPts = aRect.IntersectionPts(iArc.ArcStructure, True)
                            If txtArc Is Nothing Then
                                If iPts.Count > 0 Then
                                    v1 = iPts.Nearest(ArcPt1, TVECTOR.Zero)
                                    v2 = iPts.Farthest(ArcPt1, TVECTOR.Zero)
                                    txtArc = New dxeArc(IntersectPt, v1, v2, False, aOCS)
                                End If
                            End If
                            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        End If
                    End If
                End If
                If Not PopText And bOverArcI Then bMergeArcs = True
                If bMergeArcs And bArrowsIn Then
                    dimArc1.ArcStructure = TARC.DefineWithPoints(aOCS, IntersectPt, dimArc1.StartPtV, mpt, dimArc1.ClockWise)
                    dimArc2.ArcStructure = TARC.DefineWithPoints(aOCS, IntersectPt, dimArc2.StartPtV, mpt, dimArc2.ClockWise)
                Else
                    If Not PopText And Not bOverArcI And bArrowsIn Then
                        dimArc1.ArcStructure = TARC.DefineWithPoints(aOCS, IntersectPt, dimArc1.StartPtV, txtArc.StartPtV, dimArc1.ClockWise)
                        dimArc2.ArcStructure = TARC.DefineWithPoints(aOCS, IntersectPt, dimArc2.StartPtV, txtArc.EndPtV, dimArc2.ClockWise)
                    End If
                End If
                If aStyle.PropValueB(dxxDimStyleProperties.DIMTOFL) And Not bArrowsIn Then
                    If bOverArcI Then
                        dArc1 = aAngleArc.Clone
                    Else
                        If PopText Then
                            dArc1 = aAngleArc.Clone
                        Else
                            dArc1 = New dxeArc(IntersectPt, ArcPt1, txtArc.StartPtV, Not dimArc1.ClockWise, aOCS)
                            dArc2 = New dxeArc(IntersectPt, ArcPt2, txtArc.EndPtV, Not dimArc2.ClockWise, aOCS)
                        End If
                    End If
                End If
                '================ RETURN STUFF ==========================
#Region "RETURN"
                rMText.Identifier = "Text.1"
                _rVal.Add(rMText)
                aRect = rMText.BoundingRectangle.Stretched(2 * Math.Abs(tgap))
                txtBox = aRect.Perimeter
                txtBox.Suppressed = (tgap >= 0)
                txtBox.Color = dClr
                txtBox.Linetype = aDim.Linetype
                txtBox.Identifier = "TextBox.1"
                txtBox.VectorsV = aRect.Strukture.Corners
                _rVal.Add(txtBox)
                If aHook IsNot Nothing Then
                    If aHook IsNot dimArc2 Then
                        If aHook.Length > 0 Then
                            aHook.DisplaySettings = aDim.DisplaySettings
                            aHook.Color = dClr
                            aHook.Identifier = "Dimension.HookArc"
                            _rVal.Add(aHook)
                        End If
                    End If
                End If
                If bHook IsNot Nothing Then
                    If bHook.Length > 0 Then
                        bHook.DisplaySettings = aDim.DisplaySettings
                        bHook.Color = dClr
                        bHook.Identifier = "Dimension.HookLine"
                        _rVal.Add(bHook)
                    End If
                End If
                If dArc1 IsNot Nothing Then
                    dArc1.DisplaySettings = aDim.DisplaySettings
                    dArc1.Color = dClr
                    dArc1.Identifier = "Dimension.dimArc1"
                    _rVal.Add(dArc1)
                End If
                If dArc2 IsNot Nothing Then
                    dArc2.DisplaySettings = aDim.DisplaySettings
                    dArc2.Color = dClr
                    dArc2.Identifier = "Dimension.dimArc2"
                    _rVal.Add(dArc2)
                End If
                If asz <= 0 Then
                    aArrow1.Suppressed = True
                    aArrow2.Suppressed = True
                Else
                    If dArc1 IsNot Nothing Then
                        If dArc1.Length < 0.99 * asz Then aArrow1.Suppressed = True
                    End If
                    If dArc2 IsNot Nothing Then
                        If dArc2.Length < 0.99 * asz Then aArrow2.Suppressed = True
                    End If
                End If
                'If txtArc isNot Nothing Then
                'txtArc.Color = dxxColors.BlackWhite
                'txtArc.LineType = dxfLinetypes.Continuous
                '.Add txtArc
                'End If
                'If Not fitArc Is Nothing Then
                'fitArc.Color = dxxColors.Red
                'fitArc.LineType = dxfLinetypes.Continuous
                '.Add fitArc
                'End If
#End Region 'RETURN
                Return _rVal
            Catch ex As Exception

                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
                Return _rVal
            End Try
        End Function
        Friend Shared Function DimText_Linear(ByRef rMText As dxeText, aImage As dxfImage, aDim As dxeDimension, aScaleFactor As Double, aExtLine1 As dxeLine, aExtLine2 As dxeLine, aDimLine1 As dxeLine, aDimLine2 As dxeLine, DimLinePt1 As TVECTOR, dimLinePt2 As TVECTOR, ByRef rArrow1 As dxfEntity, ByRef rArrow2 As dxfEntity, ByRef hcnt As Integer, aWorkPlane As TPLANE) As colDXFEntities
            '^used internally create the text entities for a linear Dimension
            Dim _rVal As New colDXFEntities
            Try
                Dim dStyle As dxoDimStyle = aDim.DimStyle
                Dim aStyle As New TTABLEENTRY(dStyle)
                If aScaleFactor <= 0 Then aScaleFactor = dStyle.FeatureScaleFactor
                If aScaleFactor <= 0 Then aScaleFactor = 1
                Dim txtBox As dxePolyline
                Dim aHook As dxeLine = Nothing
                Dim bHook As dxeLine = Nothing
                Dim cHook As dxeLine = Nothing
                Dim wLine As dxeLine = Nothing
                Dim aFit As dxxDimTextFitTypes = dStyle.PropValueI(dxxDimStyleProperties.DIMATFIT)
                Dim vjust As dxxDimTadSettings = dStyle.PropValueI(dxxDimStyleProperties.DIMTAD)
                Dim hJust As dxxDimJustSettings = dStyle.PropValueI(dxxDimStyleProperties.DIMJUST)
                Dim Span As Double = aDim.DimensionValue
                Dim tgap As Double = dStyle.PropValueD(dxxDimStyleProperties.DIMGAP) * aScaleFactor
                Dim txtRot As Double = aDim.TextRotation
                Dim asz As Double = dStyle.PropValueD(dxxDimStyleProperties.DIMTSZ) * aScaleFactor * 2
                If asz <= 0 Then asz = dStyle.PropValueD(dxxDimStyleProperties.DIMASZ) * aScaleFactor
                Dim d1 As Double
                Dim ialgn As Integer
                Dim toset As Double
                Dim bAlignToElines As Boolean = hJust = dxxDimJustSettings.AlignExt1 Or hJust = dxxDimJustSettings.AlignExt2
                Dim bPopText As Boolean
                Dim bPopArrows As Boolean
                Dim bArrowsFit_WithText As Boolean
                Dim bTextFits As Boolean
                Dim bArrowsFit As Boolean = True
                Dim bFlag As Boolean
                Dim bArrowsOut As Boolean
                Dim bForceTextInside As Boolean = dStyle.PropValueB(dxxDimStyleProperties.DIMTIX)
                Dim aLine As TLINE
                Dim prjDir As TVECTOR
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim aDir As TVECTOR
                Dim xDir As TVECTOR = aWorkPlane.XDirection
                Dim yDir As TVECTOR = aWorkPlane.YDirection
                Dim zDir As TVECTOR = aWorkPlane.ZDirection
                Dim aPlane As TPLANE = aDim.PlaneV
                Dim v10 As TVECTOR = dimLinePt2
                Dim v13 As TVECTOR = aDim.DefPt13V.ProjectedTo(aPlane)
                Dim v14 As TVECTOR = aDim.DefPt14V.ProjectedTo(aPlane)
                Dim dimPt As TVECTOR = DimLinePt1
                Dim txtLn_V As New TLINE("")
                Dim txtLn_H As New TLINE("")
                Dim extLn1 As New TLINE(aExtLine1)
                Dim extLn2 As New TLINE(aExtLine2)
                Dim dimLn1 As New TLINE(aDimLine1)
                Dim dimLn2 As New TLINE(aDimLine2)
                Dim limLns As New TLINES(4)
                Dim txtBndLns As New TLINES(0)
                Dim txtRec As TPLANE = TPLANE.World
                Dim txtPl As New TPLANE(aWorkPlane)
                Dim arwBnd1 As New TLINES(0)
                Dim arwBnd2 As New TLINES(0)
                Dim arwRec1 As TPLANE = TPLANE.World
                Dim arwRec2 As TPLANE = TPLANE.World
                Dim tPts As New TVECTORS(0)
                Dim ePts As New TVECTORS(0)
                Dim algnTo As New dxeLine(dimPt, v10)
                'set the arrowheads
                rArrow1 = dxfArrowheads.GetEntity(aStyle, aPlane, aImage, dxxArrowIndicators.One, aDimLine1, "ArrowHead.1")
                rArrow2 = dxfArrowheads.GetEntity(aStyle, aPlane, aImage, dxxArrowIndicators.Two, aDimLine2, "ArrowHead.2")
                If bAlignToElines Then
                    If vjust <> dxxDimTadSettings.Centered Then vjust = dxxDimTadSettings.Above
                    If hJust = dxxDimJustSettings.AlignExt1 Then ialgn = 1 Else ialgn = 2
                    bForceTextInside = False
                Else
                    If hJust = dxxDimJustSettings.Ext1 Then ialgn = 1 Else ialgn = 2
                    toset = aDim.TextOffset
                End If
                If txtRot <> 0 Then vjust = dxxDimTadSettings.Centered
                'set the text plane and point
                v1 = dimPt.MidPt(v10)
                If bAlignToElines Then
                    If hJust = dxxDimJustSettings.AlignExt1 Then
                        v1 = v13 : v2 = extLn1.EPT
                    ElseIf hJust = dxxDimJustSettings.AlignExt1 Then
                        v1 = v14 : v2 = extLn2.EPT
                    End If
                    aDir = v1.DirectionTo(v2, False, bFlag)
                    If bFlag Then aDir = aPlane.XDirection
                Else
                    If dStyle.PropValueB(dxxDimStyleProperties.DIMTIH) Then aDir = aPlane.XDirection Else aDir = xDir
                End If
                txtPl.Define(v1, aDir, aDir.RotatedAbout(zDir, 90))
                'set the boundaries of the arrowheads
                dxfDimTool.GetArrowRectangles(aImage, dimPt, v10, aWorkPlane, rArrow1, rArrow2, arwRec1, arwRec2, arwBnd1, arwBnd2)
                'see if the arrows fit
                If asz > 0 Then bArrowsFit = dxfArrowheads.ArrowsFit(aWorkPlane, v13, v14, dimLn1, dimLn2, arwRec1, arwBnd1, arwRec2, arwBnd2)
                If Not bAlignToElines Then
                    '================ BASIC DIM =================================
                    'create the text
                    rMText = dxfDimTool.MakeText(aWorkPlane, aStyle, aDim, aImage, Nothing, txtPl, algnTo, txtRec, txtBndLns, Span, txtRot, txtLn_H, txtLn_V, False, "CENTERED", vjust, hJust, ePts)
                    'set the limits for the text between the extension lines
                    'the outermost lines (1 & 4) are just the extension lines
                    limLns.SetItem(1, New TLINE(dimPt, yDir, 10))
                    limLns.SetItem(4, New TLINE(v10, yDir, 10))
                    'the innermost lines (2 & 3) are the limits for the text to interfere with the arrowheads
                    v1 = arwRec1.Vector(0.5 * arwRec1.Width + Math.Abs(tgap))
                    v2 = arwRec2.Vector(-0.5 * arwRec2.Width - Math.Abs(tgap))
                    limLns.SetItem(2, New TLINE(v1, yDir, 10))
                    limLns.SetItem(3, New TLINE(v2, yDir, 10))
                    'see if the text fits between the outermost limit lines
                    bTextFits = limLns.Item(1).IntersectionPts(txtBndLns, True, False, True, True).Count = 0
                    If bTextFits Then
                        bTextFits = limLns.Item(4).IntersectionPts(txtBndLns, True, False, True, True).Count = 0
                    End If
                    'see if the text fits between the innermost limit lines
                    bArrowsFit_WithText = bTextFits
                    If vjust = dxxDimTadSettings.Centered And tgap <> 0 And bTextFits Then
                        bArrowsFit_WithText = limLns.Item(2).IntersectionPts(txtBndLns, True, False, True, True).Count = 0
                        If bArrowsFit_WithText Then bArrowsFit_WithText = limLns.Item(3).IntersectionPts(txtBndLns, True, False, True, True).Count = 0
                    End If
                    '============ apply the text offsets and justifications =============
                    'apply inside the lines text offsets
                    If bArrowsFit_WithText And bArrowsFit Then
                        If toset <> 0 And hJust <> dxxDimJustSettings.Ext1 And hJust <> dxxDimJustSettings.Ext2 Then
                            If toset > 0 Then
                                aLine = limLns.Item(3)
                                v1 = ePts.Nearest(v10, TVECTOR.Zero)
                            Else
                                aLine = limLns.Item(2)
                                v1 = ePts.Nearest(dimPt, TVECTOR.Zero)
                            End If
                            v2 = v1 + (xDir * toset)
                            aLine.IntersectionPt(New TLINE(v1, v2), bFlag)
                            If bFlag Then
                                If toset > 0 Then hJust = dxxDimJustSettings.Ext2 Else hJust = dxxDimJustSettings.Ext1
                            Else
                                v1 = xDir * toset
                                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                ''ProjectText:
                                rMText.Translate(v1)
                                txtRec.Origin += v1
                                txtPl.Origin = txtRec.Origin
                                txtBndLns.Translate(v1)
                                txtLn_H.Translate(v1)
                                txtLn_V.Translate(v1)
                                ePts.SetItem(1, txtLn_H.SPT)
                                ePts.SetItem(2, txtLn_H.EPT)
                                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            End If
                        End If
                        If hJust = dxxDimJustSettings.Ext1 Or hJust = dxxDimJustSettings.Ext2 Then
                            If hJust = dxxDimJustSettings.Ext2 Then
                                aLine = limLns.Item(3)
                                v1 = ePts.Nearest(v10, TVECTOR.Zero)
                            Else
                                aLine = limLns.Item(2)
                                v1 = ePts.Nearest(dimPt, TVECTOR.Zero)
                            End If
                            v2 = dxfProjections.ToLine(v1, aLine, rOrthoDirection:=prjDir, rDistance:=d1)
                            If d1 > 0 Then
                                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                ''ProjectText:
                                v1 = prjDir * d1
                                rMText.Translate(v1)
                                txtRec.Origin += v1
                                txtPl.Origin = txtRec.Origin
                                txtBndLns.Translate(v1)
                                txtLn_H.Translate(v1)
                                txtLn_V.Translate(v1)
                                ePts.SetItem(1, txtLn_H.SPT)
                                ePts.SetItem(2, txtLn_H.EPT)
                                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                            End If
                        End If
                    End If
                    'decide what to do if something doesn't fit
                    If Not bArrowsFit Then bPopArrows = True
                    If bForceTextInside Then
                        If Not bArrowsFit_WithText Then bPopArrows = True
                    Else
                        If Not bTextFits And Not bArrowsFit Then
                            bPopText = True
                            bPopArrows = True
                        Else
                            If Not bArrowsFit_WithText Then
                                Select Case aFit
                                    Case dxxDimTextFitTypes.MoveArrowsFirst
                                        bPopArrows = True
                                    Case dxxDimTextFitTypes.MoveTextAndArrows
                                        bPopArrows = True
                                        bPopText = True
                                    Case dxxDimTextFitTypes.MoveTextFirst
                                        bPopText = True
                                    Case Else
                                        bPopText = True
                                        'bPopArrows = True
                                End Select
                            End If
                            If Not bTextFits Then bPopText = True
                        End If
                    End If
                    'flip the arrows out
                    If bPopArrows Then bArrowsOut = dxfDimTool.SwapArrows(bArrowsOut, dimLn1, dimLn2, extLn1, extLn2, rArrow1, rArrow2, aPlane, asz, aDimLine1, aDimLine2)
                    If bPopText Then
                        'create hook lines if the text is out
                        bFlag = dStyle.PropValueB(dxxDimStyleProperties.DIMTOH) ' TextOutsideHorizontal
                        If bFlag Then aDir = aPlane.XDirection Else aDir = xDir
                        txtPl.Define(Nothing, aDir, aDir.RotatedAbout(zDir, 90))
                        cHook = dxfDimTool.MakeHooks(bArrowsOut, aDimLine1, aDimLine2, dimPt, v10, asz, aHook, bHook, aPlane, (ialgn = 1), toset, Not bFlag)
                        'move the text to the end of hook line
                        wLine = dxfDimTool.PickLine(aDimLine1, aDimLine2, ialgn)
                        If Math.Round(cHook.Length, 4) > 0 Then
                            algnTo.LineStructure = New TLINE(cHook)
                        Else
                            v2 = dxfDimTool.PickVector(dimPt, v10, ialgn)
                            algnTo.LineStructure = New TLINE(txtPl.Origin, v2)
                        End If
                        rMText = dxfDimTool.MakeText(aWorkPlane, aStyle, aDim, aImage, rMText, txtPl, algnTo, txtRec, txtBndLns, Span, txtRot, txtLn_H, txtLn_V, False, "ALIGNED", vjust, hJust, ePts)
                        If bHook IsNot Nothing And bArrowsOut Then
                            v1 = bHook.StartPtV
                            If ialgn = 1 Then dimLn1.EPT = v1 Else dimLn2.EPT = v1
                        End If
                        cHook.EndPtV = algnTo.EndPtV
                    End If
                Else
                    'ALIGN TEXT TO EXTENSION LINES
                    wLine = dxfDimTool.PickLine(aExtLine1, aExtLine2, ialgn)
                    v1 = dxfDimTool.PickVector(v13, v14, ialgn)
                    v2 = wLine.EndPtV
                    txtPl.Origin = v2
                    algnTo.LineStructure = New TLINE(v1, v2)
                    aDir = v1.DirectionTo(v2, False, d1)
                    rMText = dxfDimTool.MakeText(aWorkPlane, aStyle, aDim, aImage, Nothing, txtPl, algnTo, txtRec, txtBndLns, Span, txtRot, txtLn_H, txtLn_V, False, "ALIGNED", vjust, hJust, ePts)
                    wLine.EndPtV = algnTo.EndPtV
                    bPopArrows = Not bArrowsFit
                    'flip the arrows out
                    If bPopArrows Then bArrowsOut = dxfDimTool.SwapArrows(bArrowsOut, dimLn1, dimLn2, extLn1, extLn2, rArrow1, rArrow2, aPlane, asz, aDimLine1, aDimLine2)
                End If
                '================ RETURN STUFF ==========================
                'adjust the dim line endpts if the text and arrows are inside
                If Not bPopArrows Then
                    If Not bPopText And vjust = dxxDimTadSettings.Centered And Not bAlignToElines Then tPts = ePts
                    If tPts.Count > 0 Then
                        dimLn1.EPT = tPts.Nearest(dimPt, TVECTOR.Zero)
                        dimLn2.EPT = tPts.Nearest(v10, TVECTOR.Zero)
                    Else
                        dimLn1.EPT = dimLn2.SPT
                        aDimLine1.Suppressed = aDimLine1.Suppressed And aDimLine2.Suppressed
                        aDimLine2.Suppressed = True
                    End If
                Else
                    If bPopText Then
                        If dStyle.PropValueB(dxxDimStyleProperties.DIMTOFL) Then '   ForceDimLines
                            wLine = New dxeLine(aDimLine1) With {.Suppressed = False, .Identifier = "Dimension.Line3"}
                            wLine.LineStructure = New TLINE(dimPt, v10)
                            _rVal.Add(wLine)
                        End If
                    End If
                End If
                'dimLn1.SPT = ePts.Item(1)
                'dimLn1.EPT = ePts.Item(2)
                'dimLn2.Translate(New TVECTOR(0, -2, 0))
                aDimLine1.LineStructure = dimLn1
                aDimLine2.LineStructure = dimLn2
                rMText.Identifier = "Text.1"
                _rVal.Add(rMText)
                txtBox = New dxePolyline(New colDXFVectors(txtRec.Corners), True, aDimLine2.DisplaySettings, New dxfPlane(txtRec)) With
            {
            .PlaneV = txtRec,
            .Suppressed = tgap >= 0,
            .Identifier = "TextBox.1",
            .Linetype = aDim.Linetype
            }

                '.Suppressed = False
                _rVal.Add(txtBox)
                If aHook IsNot Nothing Then
                    aHook.Suppressed = (aDimLine1.Suppressed And aDimLine2.Suppressed) Or aHook.Length = 0
                    _rVal.Add(aHook)
                End If
                If bHook IsNot Nothing Then
                    bHook.Suppressed = (aDimLine1.Suppressed And aDimLine2.Suppressed) Or bHook.Length = 0
                    _rVal.Add(bHook)
                End If
                If asz <= 0 Then
                    rArrow1.Suppressed = True
                    rArrow2.Suppressed = True
                Else
                    '        If aDimLine1.Length < 0.99 * asz Then rArrow1.Suppressed = True
                    '        If aDimLine2.Length < 0.99 * asz Then rArrow2.Suppressed = True
                End If
                Return _rVal
            Catch ex As Exception
                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
            End Try
            Return _rVal
        End Function
        Friend Shared Function DimText_Ordinate(ByRef rMText As dxeText, aImage As dxfImage, aDim As dxeDimension, aScaleFactor As Double, ByRef extLine1 As dxeLine, ByRef extLine2 As dxeLine, ByRef ExtLine3 As dxeLine, bSingleLine As Boolean, ByRef hcnt As Integer) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '^used internally create the text entities for a ordinate Dimension
            Try
                Dim aPl As New TPLANE(aDim.PlaneV)
                'Dim bPl As TPLANE = aPl.Clone
                Dim wLine As TLINE
                Dim bDir As TVECTOR
                Dim dimPt As TVECTOR = aDim.DefPt13V.ProjectedTo(aPl)
                Dim BasePt As TVECTOR = aDim.DefPt10V.ProjectedTo(aPl)
                Dim aRect As dxfRectangle = Nothing

                Dim tlen As Double = 0
                Dim txtHt As Double = 0
                Dim oset As Double
                Dim txtRot As Double = aDim.TextRotation
                Dim style As dxoDimStyle = aDim.DimStyle
                Dim tClr As dxxColors = style.TextColor
                Dim eClr As dxxColors = style.ExtLineColor
                Dim dClr As dxxColors = style.DimLineColor
                Dim tgap As Double = style.TextGap * aScaleFactor
                Dim vjust As dxxDimTadSettings = style.TextPositionV
                Dim bBoxIt As Boolean = tgap < 0
                tgap = Math.Abs(tgap)
                If bSingleLine Then wLine = New TLINE(extLine1) Else wLine = New TLINE(ExtLine3)
                '==============================================================
                'determine the text angle
                '==============================================================
                Dim txtDir As TVECTOR = wLine.SPT.DirectionTo(wLine.EPT)
                If txtRot <> 0 Then txtDir.RotateAbout(aPl.ZDirection, txtRot)
                Dim tang As Double = aPl.XDirection.AngleTo(txtDir, aPl.ZDirection, 4)
                If tang > 90 And tang <= 270 Then
                    txtDir *= -1
                    tang = aPl.XDirection.AngleTo(txtDir, aPl.ZDirection, 4) + aDim.TextRotation
                End If
                'If aDim.OrdinateDimensionType = dxxOrdinateDimTypes.OrdHorizontal Then
                '    Beep()
                'End If
                'If tang <> 0 Then bPl.RotateAbout( bPl.Origin, bPl.ZDirection, tang, False, False, True, True)
                '==============================================================
                'create the text
                '==============================================================
                Dim span As Double = 0
                Dim dtxt As String = String.Empty
                rMText = dxfDimTool.MakeText(aDim, aImage, TVECTOR.Zero, tang, False, span, dtxt, aRect, 2 * tgap, txtHt, tlen)
                '==============================================================
                'place the text
                '==============================================================
                Dim aDir As TVECTOR = wLine.SPT.DirectionTo(wLine.EPT)
                Dim txtpt As TVECTOR = wLine.EPT + aDir * (0.5 * tlen)
                'set the text over the work line
                If vjust <> dxxDimTadSettings.Centered And aDim.TextRotation = 0 Then
                    oset = 0.5 * txtHt
                    If bBoxIt Then oset += tgap
                    If aDim.DimType = dxxDimTypes.OrdHorizontal Then
                        bDir = aDir.RotatedAbout(aPl.ZDirection, 90).Normalized
                    Else
                        bDir = aDir.RotatedAbout(aPl.ZDirection, -90).Normalized
                    End If
                    wLine.EPT = txtpt + aDir * (0.5 * rMText.BoundingRectangle.Width + tgap)
                    txtpt += bDir * oset
                End If
                '================ RETURN STUFF ==========================
                If bSingleLine Then extLine1.LineStructure = wLine Else ExtLine3.LineStructure = wLine

                rMText.MoveFromTo(aRect.Center, New dxfVector(txtpt))
                rMText.Identifier = "Text.1"
                rMText.ImageGUID = aDim.ImageGUID
                _rVal.Add(rMText)
                aRect = rMText.BoundingRectangle().Stretched(2 * tgap)
                Dim aPline As dxePolyline = New dxePolyline(aRect.Corners, True) With
                {
                    .Suppressed = Not bBoxIt,
                    .Color = dClr,
                    .Identifier = "TextBox.1",
                    .Linetype = aDim.Linetype
                }
                _rVal.Add(aPline)
                Return _rVal
            Catch ex As Exception
                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
                Return _rVal
            End Try
        End Function
        Friend Shared Function DimText_Radial(ByRef rMText As dxeText, aImage As dxfImage, aDim As dxeDimension, aScaleFactor As Double, DLine1 As dxeLine, DLine2 As dxeLine, aCenter As dxfVector, ByRef PopText As Boolean, ByRef minLeader As Double, aArrow1 As dxfEntity, ByRef hcnt As Integer) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            'On Error Resume Next
            _rVal = New colDXFEntities
            '^used internally create the text entities for a linear Dimension
            Dim aPl As TPLANE = aDim.PlaneV.Clone

            Dim aRect As dxfRectangle = Nothing
            Dim limLine As dxeLine
            Dim rLines As colDXFEntities
            Dim txtDir As dxfDirection
            Dim bDir As dxfDirection
            Dim prjPt As dxfVector
            Dim aA As dxeArc
            Dim aSolid As dxeSolid
            Dim aInsert As dxeInsert
            Dim textLn As Double = 0
            Dim textHt As Double = 0
            Dim tang As Double
            Dim ang As Double
            Dim P1 As dxfVector
            Dim d1 As Double
            Dim aPline As dxePolyline
            Dim aStyle As TTABLEENTRY = aDim.Style
            'set the arrowheads
            aArrow1 = dxfArrowheads.GetEntity(aStyle, aDim.PlaneV, aImage, dxxArrowIndicators.One, DLine1, "Dimension.aArrow1")
            Dim tClr As dxxColors = aStyle.PropValueI(dxxDimStyleProperties.DIMCLRT)
            Dim dClr As dxxColors = aStyle.PropValueI(dxxDimStyleProperties.DIMCLRD)
            Dim tgap As Double = aStyle.PropValueD(dxxDimStyleProperties.DIMGAP) * aScaleFactor
            Dim tht As Double = aStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * aScaleFactor
            Dim vjust As dxxDimTadSettings = aStyle.PropValueI(dxxDimStyleProperties.DIMTAD) * aScaleFactor
            Dim asz As Double = aStyle.PropValueD(dxxDimStyleProperties.DIMASZ) * aScaleFactor
            Dim bBoxIt As Boolean = tgap < 0
            tgap = Math.Abs(tgap)

            Dim rad As Double = aDim.Radius
            Dim aXDir As dxfDirection = aDim.XDirection
            Dim aZDir As dxfDirection = aDim.ZDirection
            Dim ppt As New dxfVector(aDim.DefPt15V.ProjectedTo(aDim.PlaneV))
            Dim TextPt As New dxfVector(DLine1.EndPt)
            'create the tangent line
            Dim tangLine As New dxeLine
            Dim aDir As dxfDirection = aCenter.DirectionTo(ppt)
            aDir.Rotate(90)
            tangLine.ConvergeTo(ppt, aDir, rad)
            'create the lim line
            limLine = New dxeLine
            limLine.ConvergeTo(aCenter, aDir, rad)
            If aStyle.PropValueB(dxxDimStyleProperties.DIMTIH) Then txtDir = aDim.XDirection Else txtDir = DLine1.Direction
            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ''SetTextAngle:
            tang = Math.Round(TVALUES.NormAng(aXDir.AngleTo(txtDir) + aDim.TextRotation, False, True, True), 2)
            If tang = 0 Or tang = 360 Or tang = 180 Then tang = 0
            If tang > 90 And tang <= 270 Then
                txtDir.Invert()
                tang = Math.Round(aXDir.AngleTo(txtDir), 2)
            End If
            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ''CreateDimText:
            Dim span As Double = 0
            Dim dtxt As String = String.Empty
            rMText = dxfDimTool.MakeText(aDim, aImage, TextPt.Strukture, tang, False, span, dtxt, aRect, 2 * tgap, textHt, textLn)
            'rLines = aRect.BorderLines
            'For i = 1 To rLines.Count
            '    aLine = rLines.Item(i)
            '    If textHt = 0 And i = 1 Then textHt = aLine.Length
            '    If textLn = 0 And i = 2 Then textLn = aLine.Length
            'Next i
            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            'see if the text wont fit inside
            If Not PopText Then
                If textHt >= rad Then PopText = True
                If textLn / 2 >= 0.4 * rad Then PopText = True
                If Not PopText Then
                    aA = New dxeArc With {.Radius = rad, .Plane = aDim.Plane}
                    aA.Center.MoveTo(aCenter.Strukture)
                    Dim iPts As TVECTORS = aRect.IntersectionPts(aA.ArcStructure, True)
                    If iPts.Count > 0 Then PopText = True
                End If
                If PopText Then
                    DLine1.ConvergeTo(ppt, aCenter.DirectionTo(ppt), d1)
                    minLeader = TVALUES.To_DBL(d1)
                    '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    ''SwapArrows:
                    DLine1.Mirror(tangLine)
                    If aArrow1 IsNot Nothing Then
                        If aArrow1.EntityType = dxxEntityTypes.Solid Then
                            aSolid = aArrow1
                            aSolid.Mirror(tangLine)
                        Else
                            If aArrow1.EntityType = dxxEntityTypes.Insert Then
                                aInsert = aArrow1
                                aInsert.RotationAngle = aXDir.AngleTo(tangLine.Direction)
                            End If
                        End If
                    End If
                    '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                End If
            End If
            If Not PopText Then
                If aStyle.PropValueB(dxxDimStyleProperties.DIMTIH) Then
                    aDir = aXDir.Clone
                    DLine2 = DLine1.Clone
                    ang = aDir.AngleTo(DLine1.Direction)
                    If ang > 90 And ang <= 270 Then aDir.Invert()
                    DLine2.ConvergeTo(DLine1.EndPt, aDir, minLeader / 2)
                    prjPt = DLine2.EndPt.Projected(aDir, 0.5 * textLn)
                    If vjust <> dxxDimTadSettings.Centered Then
                        aDir = aDim.Plane.YDirection
                        d1 = 0.5 * textHt
                        If bBoxIt Then d1 += tgap
                        prjPt += aDir * d1
                        DLine2.Extend(textLn - tgap)
                    End If
                    P1 = prjPt.ProjectedToLine(limLine, aDir, d1)
                    bDir = DLine1.Direction.Inverse
                    If aDir.IsEqual(bDir) Then
                        prjPt += aDir * d1
                        DLine1.EndPt += aDir * d1
                        DLine2.Translate(aDir * d1)
                    End If
                Else
                    aDir = DLine1.Direction
                    prjPt = DLine1.EndPt.Projected(aDir, 0.5 * textLn)
                    If vjust <> dxxDimTadSettings.Centered Then
                        aDir.Rotate(90)
                        ang = aDir.AngleTo(aXDir)
                        If ang <= 90 Or ang > 270 Then aDir.Invert()
                        d1 = 0.5 * textHt
                        If bBoxIt Then d1 += tgap
                        prjPt += aDir * d1
                        DLine1.Extend(textLn - tgap)
                    End If
                    P1 = prjPt.ProjectedToLine(limLine, aDir, d1)
                    bDir = DLine1.Direction.Inverse
                    If aDir.IsEqual(bDir) Then
                        prjPt += aDir * d1
                        DLine1.EndPt += aDir * d1
                    End If
                End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                ''MoveTextEntities:
                If prjPt IsNot Nothing Then
                    If TextPt IsNot Nothing Then TextPt.MoveTo(prjPt.Strukture)
                    If rMText IsNot Nothing Then rMText.MoveTo(prjPt.Strukture)
                    If aRect IsNot Nothing Then
                        aRect.MoveTo(prjPt)
                        'rLines = aRect.BorderLines
                    End If
                End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            End If
            If PopText Then
                If aStyle.PropValueB(dxxDimStyleProperties.DIMTOH) Then txtDir = aDim.XDirection Else txtDir = DLine1.Direction
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                ''SetTextAngle:
                tang = Math.Round(TVALUES.NormAng(aXDir.AngleTo(txtDir) + aDim.TextRotation, False, True, True), 2)
                If tang = 0 Or tang = 360 Or tang = 180 Then tang = 0
                If tang > 90 And tang <= 270 Then
                    txtDir.Invert()
                    tang = Math.Round(aXDir.AngleTo(txtDir), 2)
                End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                ''CreateDimText:
                rMText = dxfDimTool.MakeText(aDim, aImage, TextPt.Strukture, tang, False, span, dtxt, aRect, 2 * tgap, textHt, textLn)
                '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                If aStyle.PropValueB(dxxDimStyleProperties.DIMTOH) Then
                    aDir = aXDir.Clone
                    DLine2 = DLine1.Clone
                    ang = aDir.AngleTo(DLine1.Direction)
                    If ang > 90 And ang <= 270 Then aDir.Invert()
                    DLine2.ConvergeTo(DLine1.EndPt, aDir, minLeader / 2)
                    prjPt = DLine2.EndPt.Projected(aDir, 0.5 * textLn)
                    If vjust <> dxxDimTadSettings.Centered Then
                        aDir = aDim.Plane.YDirection
                        d1 = 0.5 * textHt
                        If bBoxIt Then d1 += tgap
                        prjPt += aDir * d1
                        DLine2.Extend(textLn - tgap)
                    End If
                Else
                    aDir = DLine1.Direction
                    prjPt = DLine1.EndPt.Projected(aDir, 0.5 * textLn)
                    If vjust <> dxxDimTadSettings.Centered Then
                        aDir.Rotate(90)
                        ang = aDir.AngleTo(aXDir)
                        If ang <= 90 Or ang > 270 Then aDir.Invert()
                        d1 = 0.5 * textHt
                        If bBoxIt Then d1 += tgap
                        prjPt += aDir * d1
                        DLine1.Extend(textLn - tgap)
                    End If
                End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                ''MoveTextEntities:
                If prjPt IsNot Nothing Then
                    If TextPt IsNot Nothing Then TextPt.MoveTo(prjPt)
                    If rMText IsNot Nothing Then rMText.MoveTo(prjPt)
                    If aRect IsNot Nothing Then
                        aRect.MoveTo(prjPt)
                        rLines = aRect.BorderLines
                    End If
                End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            End If
            '================ RETURN STUFF ==========================
            If rMText IsNot Nothing Then rMText.Identifier = "Text.1"
            _rVal.Add(rMText)
            If DLine2 IsNot Nothing Then
                If DLine2.Length <= 0 Then DLine2 = Nothing
            End If
            aRect = rMText.BoundingRectangle.Stretched(2 * tgap)
            aPline = aRect.Perimeter
            aPline.Suppressed = Not bBoxIt
            aPline.Color = dClr
            aPline.Identifier = "TextBox.1"
            aPline.Linetype = aDim.Linetype
            _rVal.Add(aPline)
            If DLine2 IsNot Nothing Then DLine2.Identifier = "Dimension.Line2"
            If asz <= 0 Then
                aArrow1.Suppressed = True
            Else
                If DLine1.Length < 0.99 * asz Then aArrow1.Suppressed = True
            End If
            Return _rVal
        End Function
        Friend Shared Sub ExtensionLines_Angular(aImage As dxfImage, aDim As dxeDimension, ByRef rScaleFactor As Double, ByRef rextLine1 As dxeLine, ByRef rextLine2 As dxeLine, ByRef rArcPt1 As TVECTOR, ByRef rArcPt2 As TVECTOR, ByRef rAngle As Double, ByRef rAngleArc As dxeArc, ByRef rIntersectPt As TVECTOR)
            '^creates the extension lines for the passed dimension
            Dim dStyle As TTABLEENTRY = aDim.Style
            rScaleFactor = dStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
            Dim wLine As TLINE
            Dim P1 As TVECTOR
            Dim P2 As TVECTOR
            Dim p3 As TVECTOR
            Dim p4 As TVECTOR

            Dim ip As TVECTOR
            Dim aDir As TVECTOR
            Dim aPl As TPLANE = aDim.PlaneV
            Dim xDir As TVECTOR = aPl.XDirection
            Dim zDir As TVECTOR = aPl.ZDirection
            Dim arPt1 As TVECTOR
            Dim arPt2 As TVECTOR
            Dim extD1 As TVECTOR
            Dim extD2 As TVECTOR
            Dim ppt As TVECTOR
            Dim eClr As dxxColors = dStyle.PropValueI(dxxDimStyleProperties.DIMCLRE)
            Dim exe As Double = dStyle.PropValueD(dxxDimStyleProperties.DIMEXE) * rScaleFactor
            Dim exo As Double = dStyle.PropValueD(dxxDimStyleProperties.DIMEXO) * rScaleFactor

            Dim ang1 As Double
            Dim ang2 As Double
            Dim ang3 As Double
            Dim bFlag As Boolean
            Dim bSwap As Boolean
            Dim b3Pt As Boolean = aDim.DimType = dxxDimTypes.Angular3P
            Dim aLine As TLINE
            Dim bLine As TLINE
            '======= initialize =========================
            rextLine1 = New dxeLine(aDim.DisplayStructure, eClr, dStyle.PropValueStr(dxxDimStyleProperties.DIMLTEX1_NAME), dStyle.PropValueB(dxxDimStyleProperties.DIMSE1), "Extension.Line1")
            rextLine2 = New dxeLine(aDim.DisplayStructure, eClr, dStyle.PropValueStr(dxxDimStyleProperties.DIMLTEX2_NAME), dStyle.PropValueB(dxxDimStyleProperties.DIMSE2), "Extension.Line2")
            rAngleArc = Nothing
            If Not b3Pt Then
                ppt = aDim.DefPt16V.ProjectedTo(aPl)
                aLine = New TLINE(aDim.DefPt15V.ProjectedTo(aPl), aDim.DefPt10V.ProjectedTo(aPl))
                bLine = New TLINE(aDim.DefPt13V.ProjectedTo(aPl), aDim.DefPt14V.ProjectedTo(aPl))
            Else
                ppt = aDim.DefPt10V.ProjectedTo(aPl)
                aLine = New TLINE(aDim.DefPt15V.ProjectedTo(aPl), aDim.DefPt13V.ProjectedTo(aPl))
                bLine = New TLINE(aLine.SPT, aDim.DefPt14V.ProjectedTo(aPl))
            End If
            '============= DETERMINE ANGLE TO DIMENSION ========================
            If Not b3Pt Then
                ip = aLine.IntersectionPt(bLine, bFlag)
                If bFlag Then ip = aLine.SPT
                If dxfProjections.DistanceTo(ip, aLine.EPT) < dxfProjections.DistanceTo(ip, aLine.SPT) Then aLine = aLine.Inverse
                If dxfProjections.DistanceTo(ip, bLine.EPT) < dxfProjections.DistanceTo(ip, bLine.SPT) Then bLine = bLine.Inverse
            Else
                ip = aLine.SPT
            End If
            aDir = ip.DirectionTo(ppt)
            'determine which angle to dimension
            ang1 = xDir.AngleTo(aLine.Direction, zDir, 4)
            ang2 = xDir.AngleTo(bLine.Direction, zDir, 4)
            ang3 = xDir.AngleTo(aDir, zDir, 4)
            If ang1 = ang2 Then
                bLine = bLine.Inverse
                ang2 += 180
            End If
            If ang3 = ang1 Or ang3 = ang2 Or ang3 = ang1 + 180 Or ang3 = ang2 + 180 Then
                ppt.RotateAbout(ip, zDir, 1, True)
            End If
            Dim rad As Double = dxfProjections.DistanceTo(ip, ppt)
            rAngle = TVALUES.NormAng(ang2 - ang1, False, True)
            aDim.Properties.SetVal(50, rAngle, 1) '"Angle of rotated, horizontal, or vertical dimensions"
            extD1 = aLine.SPT.DirectionTo(aLine.EPT)
            extD2 = bLine.SPT.DirectionTo(bLine.EPT)
            If rAngle <> 0 Then
                Dim doit As Boolean
                For trys As Integer = 1 To 2
                    doit = True
OneMoreTime:
                    If rAngle = 180 Then
                        P1 = ip + (extD1 * rad)
                        P2 = ip + (extD2 * rad)
                        rAngleArc = New dxeArc(TARC.DefineWithPoints(aPl, ip, P1, P2, False))
                        If Not rAngleArc.ContainsVector(ppt, 0.001) Then
                            rAngleArc.ArcStructure = TARC.DefineWithPoints(aPl, ip, P2, P1, False)
                            bSwap = True
                        End If
                        trys = 2
                    Else
                        P1 = ip + (extD1 * rad)
                        P2 = ip + (extD2 * rad)
                        p3 = ip + (extD1 * -rad)
                        p4 = ip + (extD2 * -rad)
                        rAngleArc = New dxeArc(TARC.DefineWithPoints(aPl, ip, P1, P2, False))
                        If b3Pt Then
                            If Not rAngleArc.ContainsVector(ppt, 0.001) Then
                                bSwap = True
                                rAngleArc.ArcStructure = TARC.DefineWithPoints(aPl, ip, P2, P1, False)
                            End If
                        Else
                            If rAngleArc.SpannedAngle > 180 And Not bFlag And trys = 1 Then
                                bFlag = True
                                wLine = aLine
                                aLine = bLine
                                bLine = wLine
                                extD1 = aLine.SPT.DirectionTo(aLine.EPT)
                                extD2 = bLine.SPT.DirectionTo(bLine.EPT)
                                doit = False
                            End If
                            If doit Then
                                If Not rAngleArc.ContainsVector(ppt, 0.001) Then
                                    rAngleArc.ArcStructure = TARC.DefineWithPoints(aPl, ip, P2, p3, False)
                                    If Not rAngleArc.ContainsVector(ppt, 0.001) Then
                                        rAngleArc.ArcStructure = TARC.DefineWithPoints(aPl, ip, p3, p4, False)
                                        If Not rAngleArc.ContainsVector(ppt, 0.001) Then
                                            rAngleArc.ArcStructure = TARC.DefineWithPoints(aPl, ip, p4, P1, False)
                                            bLine = bLine.Inverse
                                            bSwap = True
                                        Else
                                            aLine = aLine.Inverse
                                            bLine = bLine.Inverse
                                        End If
                                    Else
                                        aLine = aLine.Inverse
                                        bSwap = True
                                    End If
                                End If
                            Else
                                trys = 2
                            End If
                        End If
                    End If
                    rAngle = rAngleArc.SpannedAngle
                Next trys
            End If
            '========= EXTENSION LINES ======================
            If bSwap Then
                wLine = aLine
                aLine = bLine
                bLine = wLine
            End If
            extD1 = aLine.SPT.DirectionTo(aLine.EPT)
            extD2 = bLine.SPT.DirectionTo(bLine.EPT)
            arPt1 = ip + (extD1 * rad)
            arPt2 = ip + (extD2 * rad)
            If b3Pt Then
                If rad < dxfProjections.DistanceTo(ip, aLine.EPT) Then extD1 *= -1
                If rad < dxfProjections.DistanceTo(ip, bLine.EPT) Then extD2 *= -1
            End If
            rextLine1.StartPtV = aLine.EPT + (extD1 * exo)
            rextLine1.EndPtV = arPt1 + (extD1 * exe)
            rextLine2.StartPtV = bLine.EPT + (extD2 * exo)
            rextLine2.EndPtV = arPt2 + (extD2 * exe)
            If rAngle <> 0 Then
                rAngleArc = New dxeArc With {.ArcStructure = TARC.DefineWithPoints(aPl, ip, arPt1, arPt2, False)}
                rAngle = rAngleArc.SpannedAngle
            End If
            If Not b3Pt Then
                If Not rextLine1.DirectionV.Equals(extD1, 2) Then rextLine1.Suppressed = True
                If Not rextLine2.DirectionV.Equals(extD2, 2) Then rextLine2.Suppressed = True
            Else
                If bSwap Then
                    P1 = aDim.DefPt13V
                    aDim.DefPt13V = aDim.DefPt14V
                    aDim.DefPt14V = P1
                End If
            End If
            rIntersectPt = ip
            rArcPt1 = arPt1
            rArcPt2 = arPt2
        End Sub
        Friend Shared Sub ExtensionLines_Linear(aImage As dxfImage, aDim As dxeDimension, ByRef rScaleFactor As Double, ByRef rExtline1 As dxeLine, ByRef rExtline2 As dxeLine, ByRef rDimLinePt1 As TVECTOR, ByRef rDimLinePt2 As TVECTOR, ByRef hcnt As Integer, ByRef rWorkPlane As TPLANE)
            '^creates the extension lines for the passed dimension
            rExtline1 = New dxeLine
            rExtline2 = New dxeLine
            rExtline1.CopyDisplayProps(aDim)
            rExtline2.CopyDisplayProps(aDim)
            Dim exe As Double
            Dim exo As Double
            Dim eClr As dxxColors
            Dim d1 As Double
            Dim bFlag As Boolean
            Dim v10 As TVECTOR
            Dim v13 As TVECTOR
            Dim v14 As TVECTOR
            Dim extD1 As TVECTOR
            Dim extD2 As TVECTOR
            Dim l1 As New TLINE
            Dim extL1 As New TLINE
            Dim extL2 As New TLINE
            Dim dimPt As TVECTOR
            Dim xDir As TVECTOR
            Dim yDir As TVECTOR
            Dim Span As Double
            '======= initialize =========================
            rWorkPlane = aDim.PlaneV
            v13 = aDim.DefPt13V.ProjectedTo(rWorkPlane)
            v14 = aDim.DefPt14V.ProjectedTo(rWorkPlane)
            v10 = aDim.DefPt10V
            Span = v13.DistanceTo(v14, 10)
            eClr = aDim.Style.PropValueI(dxxDimStyleProperties.DIMCLRE)
            rScaleFactor = aDim.Style.PropValueD(dxxDimStyleProperties.DIMSCALE)
            exe = aDim.Style.PropValueD(dxxDimStyleProperties.DIMEXE) * rScaleFactor
            exo = aDim.Style.PropValueD(dxxDimStyleProperties.DIMEXO) * rScaleFactor
            rExtline1.Suppressed = aDim.Style.PropValueB(dxxDimStyleProperties.DIMSE1)
            rExtline2.Suppressed = aDim.Style.PropValueB(dxxDimStyleProperties.DIMSE2)
            rExtline1.Linetype = aDim.Style.PropValueStr(dxxDimStyleProperties.DIMLTEX1_NAME)
            rExtline2.Linetype = aDim.Style.PropValueStr(dxxDimStyleProperties.DIMLTEX2_NAME)
            rExtline1.Color = eClr
            rExtline1.Identifier = "Extension.Line1"
            rExtline2.Identifier = "Extension.Line2"
            rExtline2.Color = eClr
            'set the working plane with the xdirection aligned from the first dim pt to the second
            'create a line aligned with the second dimension line
            Select Case aDim.DimType
                Case dxxDimTypes.LinearHorizontal
                    xDir = rWorkPlane.XDirection
                    yDir = rWorkPlane.YDirection
                Case dxxDimTypes.LinearVertical
                    xDir = rWorkPlane.YDirection
                    yDir = rWorkPlane.XDirection
                Case Else
                    If Span > 0# Then
                        xDir = v13.DirectionTo(v14)
                        yDir = (xDir * -1).CrossProduct(rWorkPlane.ZDirection)
                    Else
                        xDir = rWorkPlane.XDirection
                        yDir = rWorkPlane.YDirection
                    End If
            End Select
            rWorkPlane = rWorkPlane.ReDefined(v13, xDir, yDir)
            'project the extension line pt to the created line
            l1.SPT = v14
            l1.EPT = l1.SPT + (yDir * 10)
            v10 = dxfProjections.ToLine(v10, l1)

            'get the direction and distance from the dim line pt to the second dim pt
            'this is the direcion of the second extension dine
            extD2 = v14.DirectionTo(v10, False, bFlag, d1)
            If bFlag Then extD2 = rWorkPlane.YDirection
            'if the distance is to small adjust it
            If d1 < 2 * exo Then v10 = v14 + extD2 * (2 * exo)
            '========= EXTENSION LINES ======================
            'create the second ext line first
            extL2.SPT = v14 + (extD2 * exo) 'start with the gap
            extL2.EPT = v10 + (extD2 * exe) 'end with the overshoot
            'start the first ext line same as the other
            extL1.SPT = v13 + extD2 * exo
            extL1.EPT = extL1.SPT + (extD2 * extL2.SPT.DistanceTo(extL2.EPT))
            'get the opposing extension line point
            dimPt = v10.ProjectedTo(extL1) 'start with the gap
            extD1 = v13.DirectionTo(dimPt, False, bFlag)
            If bFlag Then extD1 = rWorkPlane.YDirection
            'redefine the end pts
            extL1.SPT = v13 + (extD1 * exo)
            extL1.EPT = dimPt + (extD1 * exe) 'end with the overshoot
            'create the first extension line
            extD1 = v13.DirectionTo(dimPt)
            'create a tick line if indicated
            'set the returns
            rDimLinePt2 = v10
            rDimLinePt1 = dimPt
            rExtline1.LineStructure = extL1
            rExtline2.LineStructure = extL2
            'redefine the work plane to align with the dimension points
            If Span > 0 Then
                xDir = dimPt.DirectionTo(v10)
                yDir = v14.DirectionTo(v10)
                rWorkPlane = rWorkPlane.ReDefined(v13, xDir, yDir, Nothing, Nothing)
            End If
        End Sub
        Friend Shared Sub ExtensionLines_Ordinate(aImage As dxfImage, aDim As dxeDimension, ByRef rScaleFactor As Double, ByRef rExtline1 As dxeLine, ByRef rExtline2 As dxeLine, ByRef rExtline3 As dxeLine, ByRef rSingleLine As Boolean, ByRef hcnt As Integer)


            '^creates the extension lines for the passed dimension
            Dim ordType As dxxDimTypes = aDim.DimType
            Dim aStyl As dxoDimStyle = aDim.DimStyle
            rScaleFactor = aStyl.FeatureScaleFactor

            Dim aPl As TPLANE = aDim.PlaneV
            Dim v13 As TVECTOR = aDim.DefPt13V.ProjectedTo(aPl)
            Dim v14 As TVECTOR = aDim.DefPt14V.ProjectedTo(aPl)
            Dim extL1 As New TLINE("Extension.Line1")
            Dim extL2 As New TLINE("Extension.Line2")
            Dim extL3 As New TLINE("Extension.Line3")

            Dim v1 As TVECTOR

            Dim exo As Double = aStyl.ExtLineOffset * rScaleFactor
            Dim eClr As dxxColors = aStyl.ExtLineColor
            Dim asz As Double = aStyl.ArrowSize * rScaleFactor 'arrow size
            Dim d1 As Double
            '======= initialize =========================
            rExtline1 = New dxeLine(TVECTOR.Zero, TVECTOR.Zero, aDisplaySettings:=aDim.DisplaySettings, aIdentifier:="Extension.Line1") With {.Linetype = aStyl.PropValueStr(dxxDimStyleProperties.DIMLTEX2_NAME), .Color = eClr}
            rExtline2 = New dxeLine(TVECTOR.Zero, TVECTOR.Zero, aDisplaySettings:=aDim.DisplaySettings, aIdentifier:="Extension.Line2") With {.Linetype = aStyl.PropValueStr(dxxDimStyleProperties.DIMLTEX2_NAME), .Color = eClr}
            rExtline3 = New dxeLine(TVECTOR.Zero, TVECTOR.Zero, aDisplaySettings:=aDim.DisplaySettings, aIdentifier:="Extension.Line3") With {.Linetype = aStyl.PropValueStr(dxxDimStyleProperties.DIMLTEX2_NAME), .Color = eClr}

            Dim extDir As TVECTOR = aPl.YDirection
            Dim aDir As TVECTOR = aPl.XDirection
            If asz <= 0 Then asz = 0.09 * rScaleFactor
            If ordType = dxxDimTypes.OrdVertical Then
                extDir = aPl.XDirection
                aDir = aPl.YDirection
            End If
            'correction for 0 length line
            If v13.DistanceTo(v14, 4) <= exo Then
                If exo > 0 Then
                    v14 = v13 + (extDir * (2 * exo))
                Else
                    v14 = v13 + extDir * asz
                End If
                aDim.DefPt14V = v14
            End If
            Dim aLine As New TLINE(v13, v13 + extDir * 10)
            Dim bLine As New TLINE(v13, v13 + aDir * 10)


            v1 = v14.ProjectedTo(bLine)
            rSingleLine = v1.DistanceTo(v13, 4) = 0
            If Not v1.DirectionTo(v14).Equals(extDir, 3) Then
                extDir *= -1
            End If
            '========= EXTENSION LINES ======================
            extL1.SPT = v13 + (extDir * exo)
            If rSingleLine Then
                extL1.EPT = v14
                rExtline1.Suppressed = dxfProjections.DistanceTo(extL1.SPT, extL1.EPT, 6) = 0
                If rExtline1.Suppressed Then
                    extL1.EPT = v14 + (extDir * asz)
                End If
                rExtline2.Suppressed = True
                rExtline3.Suppressed = True
            Else
                extL1.EPT = extL1.SPT + (extDir * (1.5 * asz))
                rExtline1.Suppressed = dxfProjections.DistanceTo(extL1.SPT, extL1.EPT, 6) = 0
                If rExtline1.Suppressed Then extL1.EPT = extL1.SPT + (extDir * asz)
                extL3 = New TLINE(v14, v14) With {
                .SPT = v14 + extDir * (-2 * asz)
            }
                rExtline3.Suppressed = dxfProjections.DistanceTo(extL3.SPT, extL3.EPT, 6) = 0
                If rExtline3.Suppressed Then extL3.EPT = extL3.SPT + (extDir * 0.1)
                If Not rExtline1.Suppressed Then extL2.SPT = extL1.EPT Else extL2.SPT = v13 + (extDir * exo)
                If Not rExtline3.Suppressed Then extL2.EPT = extL3.SPT Else extL2.EPT = v14
                If Not rExtline1.Suppressed Then
                    v1 = extL2.EPT + extDir * (-2 * asz)
                    v1 = dxfProjections.ToLine(v1, extL1)
                    aDir = extL1.SPT.DirectionTo(v1)
                    If aDir.Equals(extDir, 3) Then
                        d1 = extL1.SPT.DistanceTo(v1)
                        If d1 > 1.5 * asz Then
                            extL1.EPT = v1
                            extL2.SPT = v1
                        End If
                    End If
                End If
                rExtline2.Suppressed = extL2.SPT.DistanceTo(extL2.EPT, 6) = 0
            End If
            rExtline1.LineStructure = extL1
            rExtline2.LineStructure = extL2
            rExtline3.LineStructure = extL3
        End Sub
        Friend Shared Function MakeText(ByRef ioWorkPlane As TPLANE, ByRef ioStyle As TTABLEENTRY, aDimension As dxeDimension,
                             aImage As dxfImage, ByRef ioMText As dxeText, ByRef ioTextPlane As TPLANE, aInterceptLine As dxeLine,
                             ByRef rTextRectangle As TPLANE, ByRef rBoundLines As TLINES, ByRef rSpan As Double, aRotation As Double,
                             ByRef rHorizontalIntercept As TLINE, ByRef rVerticalIntercept As TLINE,
                             bAngleValue As Boolean, aAlign As String, vjust As dxxDimTadSettings,
                             hJust As dxxDimJustSettings, ByRef rEndPTs As TVECTORS) As dxeText
            rTextRectangle = TPLANE.World
            rBoundLines = New TLINES(0)
            rSpan = 0
            rHorizontalIntercept = TLINE.Null
            rVerticalIntercept = TLINE.Null
            rEndPTs = TVECTORS.Zero

            If aDimension Is Nothing Then Return Nothing
            aDimension.GetImage(aImage)
            If aImage Is Nothing Then aImage = ioStyle.Image
            If aImage Is Nothing Then Return Nothing
            '#1the dimension to create the text for
            '#2the file object which is the source for layers and styles and settings
            '#3 the point where the text should be placed
            '^used to create the text for the passed dimension
            '~creates and returns a dxeText object
            aAlign = aAlign.Trim().ToUpper()
            Dim aDimText As String
            Dim interLn As New TLINE("")
            Dim aGap As Double
            Dim org As TVECTOR
            Dim ang As Integer
            Dim hDir As TVECTOR
            Dim vDir As TVECTOR
            Dim iLine As New TLINE("")
            Dim iPts As TVECTORS
            Dim tht As Double
            Dim hLine As New TLINE("")
            Dim vLine As New TLINE("")
            Dim dimOCS As TPLANE = aDimension.PlaneV
            Dim txtOCS As TPLANE = dimOCS.Clone
            Dim xDir As TVECTOR
            Dim zDir As TVECTOR
            Dim v1 As TVECTOR
            Dim aDir As TVECTOR
            Dim d1 As Double
            Dim finalv As TVECTOR
            Dim i As Integer
            Dim tClr As dxxColors
            Dim tsName As String
            If ioMText IsNot Nothing Then
                aDimText = ioMText.TextString
            Else
                rSpan = aDimension.DimensionValue
                aDimText = aDimension.OverideText
                'compute the number being displayed

                If String.IsNullOrWhiteSpace(aDimText) Then
                    If Not bAngleValue Then
                        aDimText = ioStyle.FormatNumber(rSpan, True)
                        If aDimension.EntityType = dxxEntityTypes.DimRadialD Or aDimension.EntityType = dxxEntityTypes.DimRadialR Then
                            If aDimension.DimType = dxxDimTypes.Diametric Then
                                aDimText = $"%%C{aDimText}"
                            Else
                                aDimText = $"R{aDimText}"
                            End If
                        End If
                    Else
                        aDimText = ioStyle.FormatAngle(rSpan)
                    End If
                End If
                aDimText = ioStyle.PropValueStr(dxxDimStyleProperties.DIMPREFIX) & aDimText & ioStyle.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)
                'style
                tsName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, ioStyle.PropValueStr(dxxDimStyleProperties.DIMTXSTY_NAME))
                ioStyle.PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, tsName)
                'text height
                tht = ioStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * ioStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
                'color
                tClr = ioStyle.PropValueI(dxxDimStyleProperties.DIMCLRT)
                'initialize the mtext object
                ioMText = New dxeText(dxxTextTypes.Multiline, New dxfDisplaySettings(dxfImageTool.DisplayStructure_Text(aImage, aDimension.LayerName, aColor:=tClr, aTextStyleName:=tsName))) With {
                .ImageGUID = aImage.GUID,
                .SuppressEvents = True,
                .PlaneV = dimOCS,
            .TextStyleName = tsName,
            .Alignment = dxxMTextAlignments.MiddleCenter,
                .Rotation = 0,
                .Vertical = False,
                .DrawingDirection = dxxTextDrawingDirections.Horizontal,
                .TextHeight = tht,
                .TextString = "{\A1;" + aDimText + "}"
            }
                ioMText.SuppressEvents = False
            End If
            aGap = ioStyle.PropValueD(dxxDimStyleProperties.DIMGAP) * ioStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
            org = ioTextPlane.Origin
            zDir = dimOCS.ZDirection
            If aRotation <> 0 Then
                vjust = -1
                xDir = dimOCS.XDirection.RotatedAbout(zDir, -aRotation).Normalized
            Else
                xDir = ioTextPlane.XDirection
                ang = dimOCS.XDirection.AngleTo(xDir, zDir)
                If ang > 90 And ang <= 270 Then
                    xDir.RotateAbout(zDir, 180)
                End If
            End If
            txtOCS.Define(ioTextPlane.Origin, xDir, xDir.RotatedAbout(zDir, 90).Normalized)
            ioMText.AlignmentPt1V = ioTextPlane.Origin
            ioMText.Rotation = 0
            ioMText.PlaneV = txtOCS
            '.MoveTo(ioTextPlane.Origin)
            rTextRectangle = ioMText.BoundingRectangle(txtOCS)
            rTextRectangle.Stretch(2 * Math.Abs(aGap))
            org = ioTextPlane.Origin
            rBoundLines = rTextRectangle.Borders
            If aInterceptLine IsNot Nothing Then
                'get the points on the bounding rectangle that intersect the vertical and horizontal intersection points
                interLn = New TLINE(aInterceptLine)
                hDir = interLn.Direction()
                vDir = zDir.CrossProduct(hDir) ' hDir.RotatedAbout(zDir, 90).Normalized
                iLine = New TLINE(org, hDir, 10, False)
                iPts = iLine.IntersectionPts(rBoundLines, True, False, True)
                'horizontal rectangle intersection
                hLine = iPts.Line(1, 2)
                iLine = New TLINE(org, vDir, 10, False)
                iPts = iLine.IntersectionPts(rBoundLines, True, False, True)
                'vertical rectangle intersection
                vLine = iPts.Line(1, 2)
                '================================================
                If aAlign = "CENTERED" Then
                    '================================================
                    finalv = interLn.MPT
                    i = 0
                    Select Case vjust
                        Case dxxDimTadSettings.Above, dxxDimTadSettings.JIS
                            i = 1
                            aDir = ioWorkPlane.YDirection
                            ang = dimOCS.YDirection.AngleTo(aDir, zDir)
                            If ang > 90 And ang <= 270 Then i = -1
                        Case dxxDimTadSettings.OppositeSide
                            aDir = ioWorkPlane.YDirection
                            i = 1
                    End Select
                    If i <> 0 Then
                        d1 = vLine.SPT.DistanceTo(vLine.EPT) / 2
                        If aGap < 0 Then d1 += Math.Abs(aGap)
                        finalv += aDir * (d1 * i)
                    End If
                    '================================================
                ElseIf aAlign = "ALIGNED" Then
                    '================================================
                    finalv = interLn.EPT + hDir * 0.5 * hLine.SPT.DistanceTo(hLine.EPT)
                    If hJust = dxxDimJustSettings.AlignExt1 Or hJust = dxxDimJustSettings.AlignExt2 Then
                        If vjust <> dxxDimTadSettings.Centered Then vjust = dxxDimTadSettings.Above
                    End If
                    '==================== align over the line
                    If vjust <> dxxDimTadSettings.Centered Then
                        iLine = New TLINE(aInterceptLine)
                        If Not (hJust = dxxDimJustSettings.AlignExt1 Or hJust = dxxDimJustSettings.AlignExt2) Then
                            aDir = iLine.Direction(False, False, d1)
                            If d1 > 0 Then
                                i = 0
                                aDir = aDir.RotatedAbout(zDir, 90).Normalized
                                Select Case vjust
                                    Case dxxDimTadSettings.Above, dxxDimTadSettings.JIS
                                        i = 1
                                        ang = dimOCS.YDirection.AngleTo(aDir, zDir)
                                        If ang > 90 And ang <= 270 Then i = -1
                                    Case dxxDimTadSettings.OppositeSide
                                        i = 1
                                End Select
                                If i <> 0 Then
                                    d1 = vLine.SPT.DistanceTo(vLine.EPT) / 2
                                    If aGap < 0 Then d1 += Math.Abs(aGap)
                                    finalv += aDir * (d1 * i)
                                End If
                            End If
                        Else
                            d1 = vLine.SPT.DistanceTo(vLine.EPT) / 2
                            If (aGap < 0) Then d1 += Math.Abs(aGap)
                            aDir = ioWorkPlane.XDirection
                            If hJust = dxxDimJustSettings.AlignExt1 Then i = -1 Else i = 1
                            finalv += (aDir * (i * d1))
                        End If
                        If Not finalv.Equals(ioTextPlane.Origin, 3) Then
                            aDir = ioTextPlane.Origin.DirectionTo(finalv, False, d1)
                            ioTextPlane.Origin = finalv
                            ioMText.Translate(aDir * d1)
                            rTextRectangle.Origin = finalv
                            hLine.Translate(aDir * d1)
                            vLine.Translate(aDir * d1)
                        End If
                    End If
                    'extent the extension lines
                    If vjust <> dxxDimTadSettings.Centered Then
                        iLine = New TLINE(aInterceptLine)
                        v1 = hLine.EndPts.Farthest(iLine.EPT, TVECTOR.Zero)
                        iLine.EPT = v1.ProjectedTo(iLine)
                        aInterceptLine.LineStructure = iLine
                    End If
                End If
                If Not finalv.Equals(ioTextPlane.Origin, 3) Then
                    aDir = ioTextPlane.Origin.DirectionTo(finalv, False, d1)
                    ioTextPlane.Origin = finalv
                    v1 = aDir * d1
                    ioMText.Translate(v1) : hLine.Translate(v1) : vLine.Translate(v1)
                End If
            End If
            rTextRectangle.Origin = finalv
            rHorizontalIntercept = hLine
            rVerticalIntercept = vLine
            rBoundLines = rTextRectangle.Borders
            'add the horizontal line to the bounding lines
            rBoundLines.Add(hLine)
            rEndPTs = hLine.EndPts
            Return ioMText
        End Function

        Friend Shared Function CreateDimEntities(aImage As dxfImage, aDim As dxeDimension) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aDim Is Nothing Then Return _rVal
            If Not aDim.GetImage(aImage) Then Return _rVal
            '#1the image which the dimension is a member of
            '#2the subject dimension entity
            '^used internally by the path generator to create the block entities of the passed dimension
            '~returns the dimensions entities including the suppressed ones.
            'make sure all the defining vectors are on the current plane of the dimension
            Try
                aDim.SuppressEvents = True

                Select Case aDim.DimensionFamily
                    Case dxxDimensionTypes.Linear
                        _rVal = dxfDimTool.CreateLinear(aImage, aDim)
                    Case dxxDimensionTypes.Ordinate
                        _rVal = dxfDimTool.CreateOrdinate(aImage, aDim)
                    Case dxxDimensionTypes.Angular
                        _rVal = dxfDimTool.CreateAngular(aImage, aDim)
                    Case dxxDimensionTypes.Radial
                        _rVal = dxfDimTool.CreateRadial(aImage, aDim)

                End Select
                Return _rVal
            Catch ex As Exception
                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
                Return _rVal
            Finally
                If _rVal IsNot Nothing Then
                    _rVal.RemoveAll(Function(x) x Is Nothing)
                    For Each ent As dxfEntity In _rVal
                        If aImage IsNot Nothing Then ent.ImageGUID = aImage.GUID
                        ent.OwnerGUID = aDim.GUID
                        ent.BlockGUID = aDim.BlockGUID

                    Next
                End If
                aDim.SuppressEvents = False
            End Try
        End Function
        Friend Shared Function CreateLeaderPaths(aImage As dxfImage, aLeader As dxeLeader, ByRef rMText As dxeText, ByRef rInsert As dxeInsert, ByRef rSubEnts As dxfEntities) As TPATHS
            '#1the image which the leader is a member of
            '#2the subject leader entity
            '^used internally by the path generator to create the sub-entities of the passed leader
            Dim rPaths As TPATHS = TPATHS.NullEnt(aLeader)
            rMText = Nothing
            rInsert = Nothing
            If aLeader Is Nothing Then Return rPaths
            If rSubEnts Is Nothing Then rSubEnts = New dxfEntities() With {.Owner = aLeader.GUID, .ImageGUID = aImage.GUID}

            Dim i As Integer
            Dim d1 As Double
            Dim txt As String = aLeader.TextString
            Dim iGUID As String = aLeader.ImageGUID
            Dim alignm As dxxMTextAlignments
            Dim hcnt As Integer
            If Not aLeader.GetImage(aImage) Then Return rPaths
            Dim aArowPths As New TPATHS(aLeader.Domain)
            Dim entPaths As New TPATHS(aArowPths.Domain)
            Dim aBlock As dxfBlock = Nothing
            Dim aPlane As TPLANE = aLeader.PlaneV
            Dim l1 As New TLINE("")
            Dim aDsp As TDISPLAYVARS = aLeader.DisplayStructure
            Dim lVerts As TVECTORS
            Dim aRwExtents As New TVECTORS(0)
            Dim aDir As TVECTOR
            Dim tempvar As String = aLeader.DefPts.Vectors.Coordinates
            Dim verts As TVECTORS = aLeader.Planarize
            Dim aRec As TPLANE
            Dim lp As TVECTOR
            Dim ip As TVECTOR = verts.Item(1, True)
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim tp As TVECTOR
            Dim bFlag As Boolean

            Dim aDStyle As dxoDimStyle = aLeader.DimStyle

            Dim xscal As Double = aDStyle.FeatureScaleFactor
            Dim tht As Double = aDStyle.TextHeight * xscal
            Dim asz As Double = aDStyle.ArrowSize * xscal
            Dim tgap As Double = aDStyle.TextGap
            Dim bAbv As Boolean = aDStyle.TextPositionV <> dxxDimTadSettings.Centered
            Dim bBoxIt As Boolean = tgap < 0
            tgap = Math.Abs(tgap) * xscal
            Dim dClr As dxxColors = aDStyle.DimLineColor
            Dim txtClr As dxxColors = aDStyle.TextColor
            If dClr = dxxColors.ByBlock Then dClr = aDsp.Color
            If txtClr = dxxColors.ByBlock Then txtClr = aDsp.Color
            Dim tsName As String = aDStyle.TextStyleName
            Dim txtLayer As String
            Dim deftxtLayer As String
            Dim aclr As dxxColors
            Dim defLayer As String

            If aLeader.LeaderType = dxxLeaderTypes.LeaderText Then

                rMText = aLeader._MText
                'rMText = Nothing
                If rMText IsNot Nothing Then
                    If String.Compare(rMText.TextString, aLeader.TextString, False) <> 0 Then
                        rMText = Nothing
                    End If
                End If
                If rMText IsNot Nothing Then
                    rMText.TextStyleName = tsName
                    tsName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, rMText.TextStyleName)
                    rMText.TextStyleName = tsName
                    txt = rMText.TextString
                    If txt = "" Then txt = aLeader.TextString
                    rMText.TextHeight = tht
                    rMText.TextString = txt
                    aLeader.TextString = txt
                    aLeader.TextLayer = rMText.LayerName
                    aLeader.TextHeight = rMText.TextHeight
                End If
            ElseIf aLeader.LeaderType = dxxLeaderTypes.LeaderBlock Then
                rInsert = aLeader._Insert
                tsName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aDStyle.TextStyleName)
            End If
            hcnt = 0
            Dim tStyle As dxoStyle = aImage.TableEntry(dxxReferenceTypes.STYLE, tsName)
            aLeader.HasArrowHead = False
            aDStyle.SetValue(dxxDimStyleProperties.DIMTXSTY_NAME, tStyle.Name, True)
            aDStyle.SetValue(dxxDimStyleProperties.DIMTXSTY, tStyle.Handle, True)
            aLeader.PopLeft = False
            aLeader.HookDirection = aPlane.XDirection
            aDsp.Color = dClr
            Dim bNoHook As Boolean = aLeader.SuppressHook Or asz <= 0
            Select Case aLeader.LeaderType
                Case dxxLeaderTypes.LeaderText
                    If String.IsNullOrWhiteSpace(txt) Then bNoHook = True
                Case dxxLeaderTypes.LeaderBlock
                    If aLeader.BlockGUID <> "" Then aBlock = aImage.Blocks.GetByGUID(aLeader.BlockGUID)
                    If aBlock Is Nothing Then
                        If aLeader.BlockName <> "" Then aBlock = aImage.Blocks.GetByName(aLeader.BlockName)
                    End If
                    If aBlock IsNot Nothing Then
                        bNoHook = True
                        aLeader.SetPropVal("*BlockName", aBlock.Name)
                        aLeader.BlockGUID = aBlock.GUID
                        rInsert = aLeader.Insert
                        If rInsert IsNot Nothing Then
                            If String.Compare(rInsert.BlockName, aBlock.Name, True) <> 0 Then
                                rInsert = Nothing
                                aLeader.Insert = Nothing
                            Else
                                v1 = rInsert.InsertionPtV
                                If aLeader.XOffset <> 0 Then v1 += aPlane.XDirection * -aLeader.XOffset
                                If aLeader.YOffset <> 0 Then v1 += aPlane.YDirection * -aLeader.YOffset
                                verts.SetItem(verts.Count, v1)
                            End If
                        End If
                    Else
                        rMText = Nothing
                        rInsert = Nothing
                    End If
                Case Else
                    rMText = Nothing
                    rInsert = Nothing
            End Select
            'create the leader lines and paths
            If verts.Count > 0 Then
                lVerts = New TVECTORS
                lVerts.Add(verts.Item(1))
                For i = 0 To verts.Count - 2
                    l1.SPT = verts.Item(i + 1)
                    l1.EPT = verts.Item(i + 2)
                    If l1.SPT.DistanceTo(l1.EPT) > 0 Then lVerts.Add(l1.EPT)
                Next i
                v1 = TVECTOR.Zero
                If rMText IsNot Nothing Then
                   
                    v1 = rMText.InsertionPtV - aLeader.LastRef
                ElseIf rInsert IsNot Nothing Then
                    v1 = rInsert.InsertionPtV - aLeader.LastRef
                End If
                If Not TVECTOR.IsNull(v1, 6) Then
                    lVerts.SetItem(lVerts.Count, lVerts.Item(lVerts.Count) + v1)
                End If
                If lVerts.Count = 1 Then verts.Add(lVerts.Item(1) + aPlane.XDirection * tgap)
                verts = lVerts.UniqueMembers(4)
                aLeader.VectorsV = verts
                ip = verts.Item(1)
                lp = verts.Last
                aDir = ip.DirectionTo(lp)
                Dim ang As Double = aPlane.XDirection.AngleTo(aDir, aPlane.ZDirection)
                If Not ((ang > 15 And ang < 165) Or (ang > 195 And ang < 345)) Then bNoHook = True
                aLeader.PopLeft = ang > 90 And ang < 270
                If aLeader.PopLeft Then aDir = aPlane.XDirection Else aDir = (aPlane.XDirection * -1)
                aLeader.HookDirection = (aDir * -1)
                'If tempvar <> aLeader.VectorsV.Coordinates Then
                '    System.Diagnostics.Debug.WriteLine(aLeader.VectorsV.Coordinates)
                'End If
                If Not bNoHook Then 'SuppressHook
                    verts.SetItem(verts.Count, lp + aDir * asz)
                    verts.Add(lp)
                    aLeader.HasHook = True
                End If
                aArowPths.Suppressed = True
                aLeader.HasArrowHead = False
                If asz > 0 And Not aLeader.SuppressArrowHead And verts.Count >= 2 Then
                    '================================= ARROW HEAD =========================================
                    'put the arrowhead on
                    Dim arwBlk As dxfBlock = dxfArrowheads.GetArrowHead(aImage, aLeader.DimStyle, "DIMLDRBLK") 'aDStyle.GetArrowHead_Leader(aImage)

                    If arwBlk.Entities.Count > 0 Then
                        aLeader.HasArrowHead = True
                        Dim aLine As New TSEGMENT(verts.Item(1), verts.Item(2)) With {.DisplayStructure = aDsp}
                        'aArowPths = arwBlk.RelativePaths
                        aArowPths = dxfArrowheads.GetPaths(aLeader, aImage, dxxArrowIndicators.Leader, aLineOrArc:=aLine, rExtentPts:=aRwExtents, bNoTicks:=True, rSuppress:=bFlag, ioBlock:=arwBlk)
                        aArowPths.Identifier = "Leader.ArrowHead"
                        If Not bFlag Then
                            aArowPths.Suppressed = False
                            verts.SetItem(1, aLine.LineStructure.SPT)
                            aArowPths.SetLayerName(aDsp.LayerName)
                        Else
                            aArowPths.Suppressed = True
                        End If
                    End If
                End If
                '=====================================================================================
                'create the text or block paths
                '======================================================
                If aLeader.EntityType = dxxEntityTypes.LeaderBlock Then
                    '======================================================
                    If aBlock IsNot Nothing Then
                        If rInsert Is Nothing Then
                            rInsert = New dxeInsert(aBlock) With {.GroupName = aLeader.GroupName,
                            .PlaneV = aPlane,
                            .XScaleFactor = aLeader.BlockScale,
                            .YScaleFactor = aLeader.BlockScale,
                            .ZScaleFactor = aLeader.BlockScale,
                            .RotationAngle = aLeader.BlockRotation}
                            rInsert.SetAttributes(aLeader.BlockAttributes)
                            v1 = verts.Last
                            If aLeader.XOffset <> 0 Then v1 += aPlane.XDirection * aLeader.XOffset
                            If aLeader.YOffset <> 0 Then v1 += aPlane.YDirection * aLeader.YOffset
                        Else
                            v1 = rInsert.InsertionPtV
                        End If
                        rInsert.Block = aBlock
                        rInsert.ImageGUID = aImage.GUID
                        rInsert.InsertionPtV = v1
                        rInsert.Identifier = "Leader.Insert"
                        rInsert.LayerName = aDsp.LayerName
                        rInsert.SetAttributes(aLeader.BlockAttributes)
                    End If
                    '======================================================
                ElseIf aLeader.LeaderType = dxxLeaderTypes.LeaderText Then
                    '======================================================
                    'create the text
                    tp = verts.Last + aLeader.HookDirection * tgap
                    If aLeader.PopLeft Then alignm = dxxMTextAlignments.TopRight Else alignm = dxxMTextAlignments.TopLeft
                    aclr = dxxColors.Undefined
                    If rMText Is Nothing Then
                        rMText = New dxeText(dxxTextTypes.Multiline) With {.GroupName = aLeader.GroupName, .Color = txtClr}
                        defLayer = aImage.DimSettings.LeaderLayer
                        deftxtLayer = aImage.TextSettings.LayerName
                        If aDsp.LayerName = defLayer And txtClr = dxxColors.ByLayer Then
                            txtLayer = defLayer
                            If deftxtLayer <> "" Then
                                Dim txtLay As dxoLayer = aImage.TableEntry(dxxReferenceTypes.LAYER, deftxtLayer)
                                If txtLay IsNot Nothing Then
                                    aclr = txtLay.Color
                                Else
                                    aclr = aImage.TextSettings.LayerColor
                                End If
                            End If
                            If aclr <> dxxColors.Undefined Then txtClr = aclr
                        Else
                            txtLayer = deftxtLayer
                            If txtLayer = "" Then
                                txtLayer = aDsp.LayerName
                                txtLayer = aImage.GetOrAdd(dxxReferenceTypes.LAYER, txtLayer)
                            Else
                                txtLayer = aImage.GetOrAdd(dxxReferenceTypes.LAYER, txtLayer, aColor:=aImage.TextSettings.LayerColor)
                            End If
                        End If
                    Else
                        txtLayer = rMText.LayerName
                        txtClr = rMText.Color
                        txtLayer = aImage.GetOrAdd(dxxReferenceTypes.LAYER, txtLayer)
                    End If
                    rMText.ImageGUID = iGUID
                    rMText.OwnerGUID = ""
                    rMText.Vertical = False
                    rMText.DrawingDirection = dxxTextDrawingDirections.Horizontal
                    rMText.TextStyleName = tsName
                    rMText.DisplayStructure = aDsp
                    rMText.Color = txtClr
                    rMText.LayerName = txtLayer
                    rMText.Linetype = dxfLinetypes.Continuous
                    rMText.PlaneV = aPlane
                    rMText.AlignmentPt1V = tp
                    rMText.AlignmentPt2V = tp
                    rMText.Alignment = alignm
                    rMText.TextString = txt
                    rMText.Identifier = "Text.1"
                    If aLeader.TextHeight > 0 Then
                        rMText.TextHeight = aLeader.TextHeight
                    Else
                        rMText.TextHeight = tht
                    End If
                    rMText.UpdatePath(True, aImage)
                    l1.SPT = tp
                    l1.EPT = tp + aPlane.XDirection * 10
                    If Not bAbv Then
                        Select Case aLeader.TextJustification
                            Case dxxVerticalJustifications.Bottom
                                d1 = rMText.Bounds.Height - 0.5 * rMText.TextHeight
                            Case dxxVerticalJustifications.Top
                                d1 = 0.5 * rMText.TextHeight
                            Case Else
                                d1 = 0.5 * rMText.Bounds.Height
                        End Select
                        aDir = aPlane.YDirection
                        aLeader.XOffset = 0
                        aLeader.YOffset = -d1

                        rMText.Transform(TTRANSFORM.CreateProjection(aDir, d1, True))
                    Else
                        rMText.UpdatePath(False, aImage)
                        v2 = rMText.Bounds.Point(dxxRectanglePts.BottomLeft)
                        v1 = dxfProjections.ToLine(v2, l1, rOrthoDirection:=aDir, rDistance:=d1)
                        d1 += tgap
                        v1 = tp + aDir * d1
                        rMText.Transform(TTRANSFORM.CreateProjection(aDir, d1, True))
                        rMText.UpdatePath(False, aImage)
                        aRec = rMText.Bounds
                        If aLeader.PopLeft Then l1.SPT = aRec.Point(dxxRectanglePts.BottomLeft) Else l1.SPT = aRec.Point(dxxRectanglePts.BottomRight)
                        l1.EPT = l1.SPT + aPlane.YDirection * 10
                        verts.SetItem(verts.Count, verts.Item(verts.Count).ProjectedTo(l1))
                    End If
                    'v1 = tp + aLeader.OffsetVector
                    'aDir = rMText.InsertionPtV.DirectionTo(v1, rDistance:=d1)
                    'rMText.Transform(TTRANSFORM.CreateProjection(aDir, d1, True))
                    rMText.OwnerGUID = aLeader.GUID
                End If
            End If
            'RETURN STUFF
            If verts.Count > 0 Then
                rPaths.ExtentVectors = verts
                aLeader.LastRef = verts.Last
                rPaths.GraphicType = dxxGraphicTypes.Leader
                'the leader line
                Dim ldrPath As TPATH = verts.ToPath(aPlane, False, aDsp, aLeader.Domain)
                ldrPath.Identifier = "Leader"
                rPaths.Add(ldrPath)
                If Not aArowPths.Suppressed Then
                    rPaths.Append(aArowPths)
                    rPaths.ExtentVectors.Append(aRwExtents)
                End If
                If rMText IsNot Nothing Then
                    rMText.ImageGUID = iGUID
                    rMText.UpdatePath(False, aImage)
                    rPaths.ExtentVectors.Append(rMText.ExtentPts)
                    entPaths = rMText.Paths
                    entPaths.Identifier = "Leader.MText"
                    aLeader.LastRef = rMText.InsertionPtV
                    If bBoxIt Then
                        aRec = entPaths.Bounds(rMText.PlaneV).Stretched(2 * Math.Abs(tgap), bMaintainOrigin:=True)
                        entPaths.Add(aRec.Corners.ToPath(aRec, True, aDsp))
                    End If
                Else
                    If rInsert IsNot Nothing Then
                        rInsert.ImageGUID = iGUID
                        rInsert.UpdatePath(False, aImage)
                        rPaths.ExtentVectors.Append(rInsert.ExtentPts)
                        entPaths = rInsert.Paths
                        entPaths.Identifier = "Leader.Insert"
                        aLeader.LastRef = rInsert.InsertionPtV
                    End If
                End If
                If entPaths.Count > 0 Then

                    rPaths.Append(entPaths)
                End If
                rSubEnts.Add(New dxePolyline(verts, False, aLeader.DisplaySettings, aLeader.Plane))
            End If
            If rMText IsNot Nothing Then
                aLeader.MText = rMText
                rSubEnts.Add(rMText)
            ElseIf rInsert IsNot Nothing Then
                aLeader.Insert = rInsert
                rSubEnts.Add(rInsert)
            End If
            Return rPaths
        End Function
        Friend Shared Function GetOverrideProps(aDimEntity As dxfEntity, aImage As dxfImage) As TPROPERTIES
            Dim _rVal As New TPROPERTIES("ACAD")
            If aDimEntity Is Nothing Then Return _rVal
            Dim aDStyle As dxoDimStyle = aDimEntity.DimStyle 'get the override dimstyle
            If aDStyle Is Nothing Then Return _rVal
            If aImage Is Nothing Then aImage = dxfEvents.GetImage(aDimEntity.ImageGUID)
            If aImage Is Nothing Then Return _rVal
            Dim iProp As dxoProperty = Nothing
            Dim eProp As dxoProperty = Nothing
            Dim iSty As dxoDimStyle = Nothing
            Dim eSty As dxoDimStyle = aDStyle
            Dim tSty As dxoStyle = Nothing
            Dim bKeep As Boolean
            Dim aVal As String = String.Empty
            Dim bVal As String = String.Empty
            Dim cnt As Integer
            Dim dtCode As Integer
            Dim sName As String = aDStyle.PropValueStr(dxxDimStyleProperties.DIMNAME)
            Dim aDimProp As dxxDimStyleProperties
            Dim pname As String
            Dim dTbl As New TTABLE(aImage.DimStyles)
            dxfImageTool.VerifyDimstyle(aImage, eSty, aImage.Blocks)

            Try

                iSty = aImage.DimStyles.TryGetOrAdd(sName, "Standard", True)
                eSty.PropValueSet(dxxDimStyleProperties.DIMNAME, iSty.Name, True)


                tSty = aImage.TextStyles.TryGetOrAdd(iSty.TextStyleName, "Standard", True)
                eSty.PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, tSty.Name, True)
                eSty.PropValueSet(dxxDimStyleProperties.DIMTXSTY, tSty.Handle, True)

                eSty.UpdateDimPost()

                aDimEntity.DimStyle = aDStyle
                cnt = 0
                Dim enumVals As System.Collections.Generic.Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxDimStyleProperties))
                For i As Integer = 1 To enumVals.Count
                    aDimProp = enumVals.ElementAt(i - 1).Value
                    bKeep = aDimProp >= dxxDimStyleProperties.DIMSCALE And aDimProp <= dxxDimStyleProperties.DIMLWE
                    pname = enumVals.ElementAt(i - 1).Key
                    'dxxDimStyleProperties.DIMSTATUS +2 To eSty.Props.Count
                    'aDimProp = i
                    'pname = dxfEnums.PropertyName(aDimProp)
                    If bKeep Then
                        eProp = eSty.Prop(pname)
                        iProp = iSty.Prop(pname)
                        'If eProp.Name = "DIMLTEX1" Then
                        'Beep
                        'End If
                        dtCode = eProp.DataTypeCode
                        bKeep = dtCode > 0
                    End If
                    If bKeep Then
                        If dtCode = 1005 Then 'pointer
                            If String.IsNullOrWhiteSpace(eProp.ValueS) Then eProp.Value = "0"
                            If String.IsNullOrWhiteSpace(iProp.ValueS) Then iProp.Value = "0"
                            bKeep = eProp.Value <> iProp.Value
                            If bKeep Then
                                sName = eSty.Properties.ValueS($"*{ eProp.Name}")
                                If String.Compare(sName, dxfLinetypes.ByBlock, True) = 0 Then bKeep = False
                            End If
                        Else
                            bKeep = Not iProp.Suppressed
                        End If
                        If bKeep Then
                            aVal = eProp.ValueS.Trim()
                            bVal = iProp.ValueS.Trim()
                            If dtCode = 1040 Then
                                aVal = Math.Round(eProp.ValueD, 6).ToString()
                                bVal = Math.Round(iProp.ValueD, 6).ToString()
                            End If
                            bKeep = aVal <> bVal
                        End If
                        If bKeep Then
                            cnt += 1
                            If cnt = 1 Then
                                _rVal.Add(New TPROPERTY(1001, "ACAD", "ACAD", dxxPropertyTypes.dxf_String))
                                _rVal.Add(New TPROPERTY(1000, "DSTYLE", "DSTYLE", dxxPropertyTypes.dxf_String))
                                _rVal.Add(New TPROPERTY(1002, "{", "Start Dimstyle Overrides", dxxPropertyTypes.dxf_String))
                            End If
                            _rVal.Add(New TPROPERTY(1070, eProp.GroupCode, $"{eProp.Name} GC", dxxPropertyTypes.dxf_String))
                            _rVal.Add(New TPROPERTY(dtCode, aVal, $"{eProp.Name } Val", dxxPropertyTypes.Undefined))
                            _rVal.MemberTypeSet(_rVal.Count, eProp.PropertyType)
                        End If
                    End If
                Next i
                If cnt > 0 Then
                    _rVal.Add(New TPROPERTY(1002, "}", "End Dimstyle Overrides", dxxPropertyTypes.dxf_String))
                Else
                    _rVal.Clear()
                End If
            Catch ex As Exception
                xHandleError(aImage, Reflection.MethodBase.GetCurrentMethod(), ex)
            End Try
            Return _rVal
        End Function

        Private Shared Sub xHandleError(aImage As dxfImage, aMethod As Reflection.MethodBase, e As Exception)
            If e Is Nothing Or aImage Is Nothing Then Return
            aImage.HandleError(aMethod, "dxfDimTool", e)
        End Sub
#End Region 'Methods
    End Class 'dxfDimTool
End Namespace
