




Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures


Namespace UOP.DXFGraphics
    Public Class dxfPaths
#Region "Methods"
        Private Shared _entImage As dxfImage
        Friend Shared Function Paths_Entity(aEntity As dxfEntity, Optional aImage As dxfImage = Nothing, Optional bRegenBlockPaths As Boolean = False) As TPATHS
            '^computes and returns the paths to draw the passed entity in world coordinates
            Dim _rVal As New TPATHS(aEntity.Domain)
            Dim bNullImg As Boolean
            Dim bRecursive As Boolean
            Dim iOld As dxfImage = Nothing
            If aEntity Is Nothing Then Return _rVal
            Dim iGUID As String
            Try
                iGUID = aEntity.ImageGUID
                'Static strCalcs As Integer
                ''strCalcs = 0
                aEntity.State = dxxEntityStates.GeneratingPath
                If aImage Is Nothing Then
                    If _entImage IsNot Nothing AndAlso _entImage.Disposed Then
                        _entImage = Nothing
                    End If
                    If _entImage IsNot Nothing Then
                        If _entImage.GUID = aEntity.ImageGUID Then
                            bRecursive = True
                            aImage = _entImage
                            iGUID = aImage.GUID
                        End If
                    End If
                End If
                If aEntity.PathIsImageDependant Then
                    If Not aEntity.GetImage(aImage) Then
                        bNullImg = True
                        aImage = dxfGlobals.New_Image()
                        iGUID = aImage.GUID
                    End If
                End If
                aEntity.SetImage(aImage, False)
                aEntity.ImageGUID = iGUID
                iOld = _entImage
                _entImage = aImage
                Select Case aEntity.GraphicType
         '--------------------- PRIMATIVES -----------------------------------------------------
                    Case dxxGraphicTypes.Arc
                        _rVal = epths_ARC(aEntity, aImage)
                    Case dxxGraphicTypes.Line
                        _rVal = epths_LINE(aEntity, aImage)
                    Case dxxGraphicTypes.Bezier
                        _rVal = epths_BEZIER(aEntity, aImage)
                    Case dxxGraphicTypes.Ellipse
                        _rVal = epths_ELLIPSE(aEntity, aImage)
                        ''--------------------- COMPOSITES -----------------------------------------------------
                    Case dxxGraphicTypes.Polygon
                        _rVal = epths_PGON(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Polyline
                        _rVal = epths_PLINE(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Text, dxxGraphicTypes.MText
                        _rVal = epths_TEXT(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Dimension
                        _rVal = epths_DIM(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Hatch
                        _rVal = epths_HATCH(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Hole
                        _rVal = epths_HOLE(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Insert
                        _rVal = epths_INSERT(aEntity, aImage, bRegenBlockPaths)
                        iOld = Nothing
                    Case dxxGraphicTypes.Leader
                        _rVal = epths_LEADER(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Point
                        _rVal = epths_POINT(aEntity, aImage)
                    Case dxxGraphicTypes.Solid
                        _rVal = epths_SOLID(aEntity, aImage)
                    Case dxxGraphicTypes.Symbol
                        _rVal = epths_SYMBOL(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Table
                        _rVal = epths_TABLE(aEntity, aImage)
                        iOld = Nothing
                    Case dxxGraphicTypes.Shape
                        _rVal = epths_SHAPE(aEntity, aImage)
                        iOld = Nothing
                End Select
                _rVal.EntityGUID = aEntity.GUID
                Return _rVal
            Catch ex As Exception
                If aImage IsNot Nothing Then
                    If Not bNullImg Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                End If
            Finally
                _entImage = iOld
                aEntity.State = dxxEntityStates.Steady
                If Not bRecursive And bNullImg Then
                    aImage.Dispose()
                End If
            End Try
            Return _rVal
        End Function
        Friend Shared Function Paths_Insert(aBlock As dxfBlock, aInsertionPt As dxfVector, aScaleFactor As Double, aRotation As Double, aInsertPlane As dxfPlane, aDisplayVars As TDISPLAYVARS, aImage As dxfImage, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Model, Optional aIndentifier As String = "", Optional bRegenBlockPaths As Boolean = False) As TPATHS
            Dim rPaths As New TPATHS(aDomain)
            If aBlock Is Nothing Or aImage Is Nothing Then Return rPaths

            Dim aPlane As TPLANE = aInsertPlane.Strukture
            Dim txtPlane As TPLANE = aPlane.Revolved(aRotation)
            Dim rHndl As String = String.Empty
            'Dim aHndl As String = aInsert.Handle
            Dim pth As TPATH
            Try
                'initialize
                'eGUID = aInsert.GUID
                'get the block
                'compute the paths of the block applying the scales etc. of the insert
                rPaths = aBlock.InsertPaths(aDomain, aScaleFactor, aRotation, aPlane, aImage, True, aDisplayVars, bRegen:=bRegenBlockPaths)
                'rPaths.EntityGUID = eGUID
                If aBlock.IsArrowHead Then
                    For j As Integer = 1 To rPaths.Count
                        pth = rPaths.Item(j)
                        pth.Identifier = aIndentifier ' rPaths.Identifier
                        pth.DisplayVars = aDisplayVars
                        rPaths.SetItem(j, pth)
                    Next j
                End If
                '        rPaths = pths_Combine(rPaths)
                'rPaths.Bounds = rPaths.ExtentVectors.Bounds(aPlane)
                rPaths.Identifier = aIndentifier
                'set instancing properties
                'Dim aInsts As TINSTANCES = aInsert.InstancesV
                'aInsts.Plane = aPlane
                ''aInsts.ExtentPts = rPaths.ExtentVectors
                ' If IIf(aImage Is Nothing, aInsts.Count >= 1, Not aImage.UsingDxfViewer) Then
                '    'insts_WriteToDebug aInsts
                '    'aInsts.ExtentPts = pln_Corners(rPaths.Bounds)
                '    rPaths = aInsts.Apply(rPaths)
                '    'aInsts.ExtentPts = aInsts.Apply(aInsts.ExtentPts)
                'End If
                'aInsert.InstancesV = aInsts
                '.AttribsV = aAttribs
                'rPaths.EntityGUID = aInsert.GUID
                '.Paths = rPaths
                Return rPaths
            Catch ex As Exception
                Return rPaths
            End Try
        End Function
        Private Shared Function epths_ARC(aArc As dxeArc, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aArc)
            Dim bPath As TPATH
            Dim aPlane As TPLANE = aArc.PlaneV
            Dim aSegs As New TSEGMENTS
            Dim aInsts As dxoInstances = aArc.Instances

            Try
                bPath = aArc.ArcLineStructure.GetPath(aPlane, aSegs, rPaths.ExtentVectors, False)
                bPath.DisplayVars = aArc.DisplayStructure
                rPaths.Add(bPath)
                rPaths.Identifier = aArc.Identifier
                'set instancing properties
                aInsts.Plane = aPlane

                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex


                Return rPaths
            Finally
                aArc.SavePaths(rPaths, aSegs)
            End Try
        End Function
        Private Shared Function epths_BEZIER(aBezier As dxeBezier, aimage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aBezier)
            Dim aPlane As TPLANE = aBezier.PlaneV
            Dim bPath As TPATH
            Dim aInsts As dxoInstances = aBezier.Instances
            Dim aSegs As New TSEGMENTS(0)
            Try
                aBezier.Planarize()
                bPath = aBezier.Vectors.ToPath(aBezier.PlaneV, False, aBezier.DisplayStructure)
                Dim aLoop As TVECTORS = bPath.Looop(1)
                aLoop.SetCode(1, dxxVertexStyles.MOVETO)
                aLoop.SetCode(2, dxxVertexStyles.BEZIERTO)
                aLoop.SetCode(3, dxxVertexStyles.BEZIERTO)
                aLoop.SetCode(4, dxxVertexStyles.BEZIERTO)
                bPath.SetLoop(1, aLoop)
                rPaths.Add(bPath)
                'bounding rectangle
                rPaths.ExtentVectors = aBezier.BezierStructure.PhantomPoints(10, True)
                'rPaths.Bounds = rPaths.ExtentVectors.Bounds(aPlane)
                rPaths.Identifier = aBezier.Identifier
                'set instancing properties
                aInsts.Plane = aPlane
                rPaths.EntityGUID = aBezier.GUID

                Return rPaths
            Catch ex As Exception
                If aimage IsNot Nothing Then aimage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex

                Return rPaths
            Finally
                aBezier.SavePaths(rPaths, aSegs)
                aBezier.State = dxxEntityStates.Steady
            End Try
        End Function
        Private Shared Function epths_DIM(aDim As dxeDimension, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aDim)
            Dim aInsts As dxoInstances = aDim.Instances
            Dim aSegs As New TSEGMENTS(0)
            Dim iGUID As String = aImage.GUID
            Dim eGUID As String = aDim.GUID

            Dim SubEnts As New dxfEntities() With {.Owner = eGUID, .ImageGUID = iGUID}

            Try
                If Not aDim.GetImage(aImage) Then aImage = dxfGlobals.New_Image()

                Dim ePth As TPATH
                Dim aPlane As TPLANE = aDim.PlaneV


                'confirm the dimstyle
                Dim dsttyle As dxoDimStyle = Nothing
                If Not aImage.DimStyles.TryGet(aDim.DimStyle.Name, dsttyle) Then

                    aImage.DimStyles.Add(New dxoDimStyle(aDim.DimStyle))
                End If
                aDim.State = dxxEntityStates.GeneratingPath
                aDim.ImageGUID = iGUID

                'set the path display properties
                Dim dimDSP As TDISPLAYVARS = aDim.DisplayStructure
                Dim dStyl As dxoDimStyle = aDim.DimStyle
                dimDSP.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aDim.LayerName)
                Dim lyrDSP As TDISPLAYVARS = aImage.Layers.Member(dimDSP.LayerName).DisplayVars


                dimDSP.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, dimDSP.LayerName)

                If dimDSP.Color = dxxColors.ByLayer Then dimDSP.Color = lyrDSP.Color
                If String.Compare(dimDSP.Linetype, "BYLAYER", True) = 0 Then dimDSP.Linetype = lyrDSP.Linetype
                If dimDSP.LineWeight = dxxLineWeights.ByLayer Then dimDSP.LineWeight = lyrDSP.LineWeight
                Dim dClr As dxxColors = TDISPLAYVARS.ParseColor(dStyl.DimLineColor, dimDSP.Color, lyrDSP.Color) 'dimension line path color
                Dim eClr As dxxColors = TDISPLAYVARS.ParseColor(dStyl.ExtLineColor, dimDSP.Color, lyrDSP.Color) 'extension line path color
                Dim tClr As dxxColors = TDISPLAYVARS.ParseColor(dStyl.TextColor, dimDSP.Color, lyrDSP.Color) 'text color
                Dim dLwt As dxxLineWeights = TDISPLAYVARS.ParseLineWeight(dStyl.DimLineWeight, dimDSP.LineWeight, lyrDSP.LineWeight)  'dimension line path line weight
                Dim eLwt As dxxLineWeights = TDISPLAYVARS.ParseLineWeight(dStyl.ExtLineWeight, dimDSP.LineWeight, lyrDSP.LineWeight)   'extension line path line weight

                Dim dLt As String = TDISPLAYVARS.ParseLineType(dStyl.DimLinetype, dimDSP.Linetype, lyrDSP.Linetype)
                Dim eLt1 As String = TDISPLAYVARS.ParseLineType(dStyl.ExtLinetype1, dimDSP.Linetype, lyrDSP.Linetype)
                Dim eLt2 As String = TDISPLAYVARS.ParseLineType(dStyl.ExtLinetype2, dimDSP.Linetype, lyrDSP.Linetype)

                'create the dimension entities
                Dim blockEnts As List(Of dxfEntity) = dxfDimTool.CreateDimEntities(aImage, aDim)


                'loop on the dimension entities to set some props and get their paths
                For j As Integer = blockEnts.Count To 1 Step -1
                    Dim aEnt As dxfEntity = blockEnts.Item(j - 1)
                    If aEnt.GraphicType = dxxGraphicTypes.Point Then aEnt.LayerName = "DefPoints" Else aEnt.LayerName = dimDSP.LayerName
                    'get their paths
                    Dim pthDSP As New TDISPLAYVARS(dimDSP)
                    aEnt.ImageGUID = iGUID
                    aEnt.UpdatePath(False, aImage)
                    Dim ePths As TPATHS = aEnt.Paths
                    Dim sIDent As String = aEnt.Identifier
                    pthDSP.Suppressed = aEnt.Suppressed
                    Select Case aEnt.GraphicType
                        Case dxxGraphicTypes.Point
                            pthDSP.LayerName = "DefPoints" : pthDSP.Color = dxxColors.ByLayer : pthDSP.LineWeight = dxxLineWeights.LW_000 : pthDSP.Linetype = dxfLinetypes.Continuous
                        Case dxxGraphicTypes.Text, dxxGraphicTypes.MText
                            pthDSP.Color = tClr : pthDSP.Linetype = dxfLinetypes.Continuous : pthDSP.LineWeight = dxxLineWeights.LW_000

                        Case dxxGraphicTypes.Insert, dxxGraphicTypes.Solid
                            pthDSP.Color = dClr : pthDSP.Linetype = dxfLinetypes.Continuous : pthDSP.LineWeight = dxxLineWeights.LW_000
                        Case Else
                            aEnt.LayerName = dimDSP.LayerName
                            'dim linesd an associated entities
                            If sIDent.StartsWith("Dimension.", comparisonType:=StringComparison.OrdinalIgnoreCase) Or sIDent.StartsWith("ArrowHead.", comparisonType:=StringComparison.OrdinalIgnoreCase) Or sIDent.StartsWith("TextBox.", comparisonType:=StringComparison.OrdinalIgnoreCase) Then
                                'dim lines
                                pthDSP.Color = dClr : pthDSP.Linetype = dLt : pthDSP.LineWeight = dLwt
                                'If TSTRING.StartsWith(sIDent, "TextBox.") Then pthDSP.Linetype = dimDSP.Linetype
                            Else  'extension lines
                                pthDSP.Color = eClr : pthDSP.Linetype = eLt2 : pthDSP.LineWeight = eLwt
                                If Right(sIDent, 1) = "1" Then pthDSP.Linetype = eLt1
                            End If
                    End Select

                    For i As Integer = 1 To ePths.Count
                        ePth = ePths.Item(i)
                        ePth.Identifier = sIDent
                        ePth.GraphicType = aEnt.GraphicType
                        ePth.DisplayVars = pthDSP
                        ePths.SetItem(i, ePth)
                        If Not aEnt.Suppressed Then rPaths.Add(ePth)
                    Next i
                    If Not aEnt.Suppressed Then rPaths.ExtentVectors.Append(ePths.ExtentVectors)
                    ' System.Diagnostics.Debug.WriteLine(aEnt.Identifier)
                    'ePths.ExtentVectors.Print("*" & aEnt.Identifier & "*")

                    SubEnts.Add(aEnt)
                Next j

                'rPaths.Bounds = rPaths.ExtentVectors.Bounds(aDim.PlaneV)
                rPaths.Combine()
                rPaths.EntityGUID = aDim.GUID
                rPaths.Identifier = aDim.Identifier
                'set instancing properties
                aInsts.Plane = aPlane
                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aDim.SavePaths(rPaths, aSegs, aPathEntities:=SubEnts)
                aDim.State = dxxEntityStates.Steady
            End Try
        End Function
        Private Shared Function epths_ELLIPSE(aEllipse As dxeEllipse, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aEllipse)
            Dim bPath As TPATH
            Dim aArc As TARC = aEllipse.ArcStructure
            Dim aPlane As TPLANE = aArc.Plane
            Dim aInsts As dxoInstances = aEllipse.Instances
            Dim aSegs As New TSEGMENTS(0)
            Try
                bPath = New TPATH(aEllipse.Domain, aEllipse.DisplayStructure) With {.Plane = aPlane}
                bPath.AddLoop(aArc.PathPoints)
                rPaths.ExtentVectors = aArc.PhantomPoints(20)
                bPath.Plane = aPlane
                rPaths.Add(bPath)
                'rPaths.Bounds = rPaths.ExtentVectors.Bounds(aPlane)
                rPaths.Identifier = aEllipse.Identifier
                rPaths.EntityGUID = aEllipse.GUID
                'set instancing properties
                aInsts.Plane = aPlane

                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex

                Return rPaths
            Finally
                aEllipse.SavePaths(rPaths, aSegs)
                aEllipse.State = dxxEntityStates.Steady
            End Try
        End Function
        Private Shared Function epths_HATCH(aHatch As dxeHatch, aImage As dxfImage) As TPATHS

            Dim rPaths As TPATHS = TPATHS.NullEnt(aHatch)
            Dim aPth As New TPATH
            Dim gProps As New TGRID
            Dim hBounds As New TBOUNDLOOPS(0)
            Dim aInsts As dxoInstances = aHatch.Instances
            Dim aSegs As New TSEGMENTS(0)

            Try
                If Not aHatch.GetImage(aImage) Then aImage = dxfGlobals.New_Image()
                Dim aBounds As colDXFEntities = aHatch.BoundingEntities
                Dim aPlane As TPLANE = aHatch.PlaneV
                Dim iGUID As String = aImage.GUID

                hBounds.ExtentPts = rPaths.ExtentVectors
                aPth.Plane = aPlane
                aPth.DisplayVars = aHatch.DisplayStructure
                'get the boundary loops
                hBounds = dxfHatches.GetBoundLoops(aBounds, aPlane, aImage)
                'set the bounding rectangle
                rPaths.ExtentVectors = hBounds.ExtentPts
                'rPaths.Bounds = rPaths.ExtentVectors.Bounds(aPlane)
                gProps.HatchStyle = aHatch.HatchStyle
                gProps.Method = aHatch.Method
                gProps.ScaleFactor = aHatch.ScaleFactor
                gProps.Rotation = aHatch.Rotation
                gProps.HATCHPAT = aHatch.HatchPatternV
                gProps.Plane = aPlane
                gProps.BoundaryLoops = hBounds
                gProps.Plane.Origin = aHatch.OriginV
                gProps.LINEORIGIN = gProps.Plane.Origin
                'apply the hatch pattern to the edge loops
                aPth = dxfHatches.Paths(aPth, gProps, False)
                gProps.Plane.Origin = gProps.LINEORIGIN

                aPth.Plane = aPlane
                rPaths.Add(aPth)
                rPaths.Identifier = aHatch.Identifier
                'set instancing properties
                aInsts.Plane = aPlane
                aHatch.BoundaryLoops = hBounds
                rPaths.EntityGUID = aHatch.GUID

                Return rPaths
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message)

                Return rPaths
            Finally
                aHatch.SavePaths(rPaths, aSegs)
                aHatch.State = dxxEntityStates.Steady
            End Try
        End Function
        Private Shared Function epths_HOLE(aHole As dxeHole, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aHole)
            Dim aInsts As dxoInstances = aHole.Instances
            Dim aSegs As TSEGMENTS = New TSEGMENTS(0)
            Try

                Dim aPlane As TPLANE = aHole.PlaneV

                Dim segSegs As New TSEGMENTS(0)
                aSegs = dxfPrimatives.CreateHoleSegments(aHole)

                For i As Integer = 1 To aSegs.Count
                    Dim aSeg As TSEGMENT = aSegs.Item(i)
                    Dim eVecs As TVECTORS = TVECTORS.Zero

                    Dim segPath As TPATH = aSeg.GetPath(aPlane, segSegs, eVecs, False)
                    rPaths.ExtentVectors.Append(eVecs)
                    rPaths.AddOrJoin(segPath)
                Next i
                rPaths.Identifier = aHole.Identifier
                aInsts.Plane = aPlane

                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aHole.SavePaths(rPaths, aSegs)
                aHole.State = dxxEntityStates.Steady
            End Try
        End Function
        Private Shared Function epths_INSERT(aInsert As dxeInsert, aImage As dxfImage, Optional bRegenBlockPaths As Boolean = False) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aInsert)
            Dim aBlock As dxfBlock = Nothing

            Dim aPlane As TPLANE = aInsert.PlaneV
            Dim txtPlane As New TPLANE(aPlane)
            Dim dsp As TDISPLAYVARS = aInsert.DisplayStructure
            Dim eGUID As String = aInsert.GUID
            Dim idntfy As String = aInsert.Identifier
            Dim aInsts As dxoInstances = aInsert.Instances
            Dim aSegs As New TSEGMENTS(0)
            Try
                If Not aInsert.GetImage(aImage) Then aImage = dxfGlobals.New_Image()

                txtPlane.Revolve(aInsert.RotationAngle)

                'get the block
                If aInsert.GetBlock(aImage, aBlock) Then

                    'compute the paths of the block applying the scales etc. of the insert
                    rPaths = aBlock.InsertPaths(aImage, aInsert, bRegen:=bRegenBlockPaths)
                    rPaths.EntityGUID = eGUID
                    If aBlock.IsArrowHead Then
                        For j As Integer = 1 To rPaths.Count
                            Dim pth As TPATH = rPaths.Item(j)
                            pth.Identifier = idntfy ' rPaths.Identifier
                            pth.DisplayVars = dsp
                            rPaths.SetItem(j, pth)
                        Next j
                    End If
                End If
                'set instancing properties
                aInsts.Plane = aPlane

                Return rPaths
            Catch ex As Exception
                Return rPaths
            Finally
                aInsert.SavePaths(rPaths, aSegs)
                aInsert.State = dxxEntityStates.Steady
            End Try
        End Function
        Private Shared Function epths_LEADER(aLeader As dxeLeader, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = Nothing
            Dim aMText As dxeText = Nothing
            Dim aInsert As dxeInsert = Nothing
            Dim aPlane As TPLANE = aLeader.PlaneV
            Dim aInsts As dxoInstances = aLeader.Instances
            Dim aSegs As New TSEGMENTS(0)
            Dim iGUID As String = aImage.GUID
            Dim eGUID As String = aLeader.GUID
            Dim SubEnts As New dxfEntities() With {.Owner = eGUID, .ImageGUID = iGUID}

            Try
                If Not aLeader.GetImage(aImage) Then aImage = dxfGlobals.New_Image()

                Dim dstyle As dxfTableEntry = Nothing
                aLeader.DimStyleName = aImage.GetOrAdd(dxxReferenceTypes.DIMSTYLE, aLeader.DimStyleName, dstyle)
                aLeader.DimStyle = New dxoDimStyle(dstyle) With {.IsCopied = True}
                rPaths = dxfDimTool.CreateLeaderPaths(aImage, aLeader, aMText, aInsert, SubEnts)

                'set instancing properties
                aInsts.Plane = aPlane


                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aLeader.MText = aMText
                aLeader.Insert = aInsert
                Dim reactor As dxfEntity = Nothing
                Select Case aLeader.LeaderType
                    Case dxxLeaderTypes.LeaderText
                        reactor = aMText
                    Case dxxLeaderTypes.LeaderBlock
                        reactor = aInsert
                End Select

                aLeader.SavePaths(rPaths, aSegs, reactor, aPathEntities:=SubEnts)
                aLeader.State = dxxEntityStates.Steady
            End Try
        End Function
        Private Shared Function epths_LINE(aLine As dxeLine, aimage As dxfImage) As TPATHS

            Dim rPaths As TPATHS = TPATHS.NullEnt(aLine)
            Dim aPlane As TPLANE = aLine.PlaneV
            Dim bPath As TPATH
            Dim aInsts As dxoInstances = aLine.Instances
            Dim aSegs As New TSEGMENTS(0)
            Try
                bPath = aLine.ArcLineStructure.GetPath(aPlane, aSegs, rPaths.ExtentVectors, False)
                bPath.Identifier = aLine.Identifier
                bPath.DisplayVars = aLine.DisplayStructure
                If aSegs.Count = 1 Then bPath.GraphicType = dxxGraphicTypes.Line
                rPaths.Add(bPath)
                aInsts.Plane = rPaths.Bounds(aPlane)
                Return rPaths
            Catch ex As Exception
                If aimage IsNot Nothing Then aimage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aLine.SavePaths(rPaths, aSegs)

            End Try
        End Function
        Private Shared Function epths_PGON(aPline As dxePolygon, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aPline)

            Dim v1 As TVERTEX
            Dim vSegs As New TSEGMENTS(0)
            Dim segPath As TPATH
            Dim eVecs As New TVECTORS(0)
            Dim segSegs As New TSEGMENTS(0)
            Dim aPlane As TPLANE = aPline.PlaneV
            Dim sPaths As TPATHS
            Dim verts As TVERTICES = aPline.VerticesV
            Dim i As Integer

            Dim lynames As New List(Of String)
            Dim ltnames As New List(Of String)
            Dim gname As String = aPline.GroupName
            Dim aSeg As TSEGMENT

            Dim iGUID As String = aPline.ImageGUID
            verts.ProjectTo(aPlane)
            If aImage IsNot Nothing Then iGUID = aImage.GUID
            Dim aInsts As dxoInstances = aPline.Instances
            Dim aSegs As New TSEGMENTS(0)
            Dim SubEnts As New dxfEntities() With {.Owner = aPline.GUID, .ImageGUID = iGUID}
            Try
#Region "Bounding Segments" 'get the lines and arcs that make up the polyline bound of the polygon
                Dim aSegments As List(Of TSEGMENT) = dxfSegments.PolylineSegments(verts, aPlane, aPline.Closed, aLTScale:=rPaths.LTScale, sGlobalWidth:=aPline.SegmentWidth, aIdentifier:="BOUNDS", aDefSettings:=aPline.DisplaySettings)
                aPline.VerticesV = verts

                For i = 1 To aSegments.Count
                    v1 = verts.Item(i)
                    aSeg = aSegments(i - 1)
                    aSeg.ImageGUID = iGUID
                    aSeg.Identifier = "BOUNDS"
                    aSeg.OwnerGUID = rPaths.EntityGUID
                    aSeg.LineWeight = rPaths.LineWeight

                    If aSeg.Color = dxxColors.ByBlock Or aSeg.Color = dxxColors.Undefined Then aSeg.Color = rPaths.Color
                    If String.Compare(aSeg.Linetype, dxfLinetypes.ByBlock, ignoreCase:=True) = 0 Or String.IsNullOrWhiteSpace(aSeg.Linetype) Then aSeg.Linetype = rPaths.Linetype
                    If String.IsNullOrWhiteSpace(aSeg.LayerName) Then aSeg.LayerName = rPaths.LayerName
                    If String.Compare(aSeg.Linetype, dxfLinetypes.Invisible, ignoreCase:=True) = 0 Then
                        aSeg.Suppressed = True
                    End If
                    'aSegments(i - 1) = aSeg

                    If aSeg.Suppressed Then Continue For
                    If aImage IsNot Nothing Then
                        If ltnames.FindIndex(Function(x) String.Compare(x, aSeg.Linetype, True) = 0) < 0 Then
                            aSeg.Linetype = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, aSeg.Linetype)
                            ltnames.Add(aSeg.Linetype)
                        End If
                        If lynames.FindIndex(Function(x) String.Compare(x, aSeg.LayerName, True) = 0) < 0 Then
                            aSeg.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aSeg.LayerName)
                            lynames.Add(aSeg.LayerName)
                        End If
                    End If
                    segPath = aSeg.GetPath(aPlane, segSegs, eVecs, False)
                    If Not aSeg.IsArc Then
                        If segPath.LoopCount > 0 Then
                            If segPath.Looop(1).Count = 2 Then
                                segPath.GraphicType = dxxGraphicTypes.Line
                            End If
                        End If
                    End If
                    ' aSegments(i - 1) = aSeg
                    vSegs.Add(aSeg, rPaths.EntityGUID)
                    rPaths.ExtentVectors.Append(eVecs)
                    rPaths.AddOrJoin(segPath)

                    ' aSegments(i - 1) = aSeg
                    aSegs.Add(aSeg)
                Next i
#End Region 'Bounding Segments

                'get the lines and arcs that make up the polyline bound of the polygon
                'the bounding segments can be a single polyline or a collection of lines, polylines and arcs depending if the vertices have varying display properties
                SubEnts = dxfSegments.SegmentsToPolylineEnts(vSegs, aPlane, aLayer:=rPaths.LayerName, aColor:=rPaths.Color, aLineType:=rPaths.Linetype,
                                                                aLTScale:=rPaths.LTScale, aLinWeight:=rPaths.LineWeight, aGroupName:=gname, aOwnerGUID:=aPline.GUID, aImageGUID:=iGUID,
                                                                bSingleSegs:=False)


#Region "Additional Segments" 'add the paths of the additional segments
                Dim aEnts As colDXFEntities = IIf(Not aPline.SuppressAdditionalSegments, aPline.AdditionalSegments, New colDXFEntities())

                For Each subent As dxfEntity In aEnts
                    If subent.Suppressed Then Continue For

                    'If String.Compare(subent.Tag, "END SUPPORTS", True) = 0 And subent.GraphicType = dxxGraphicTypes.Polyline Then
                    '    Debug.Print(subent.HandlePt.ToString())
                    'End If

                    If aImage IsNot Nothing Then
                        If ltnames.FindIndex(Function(x) String.Compare(x, subent.Linetype, True) = 0) < 0 Then
                            subent.Linetype = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, subent.Linetype)
                            ltnames.Add(subent.Linetype)
                        End If
                        If lynames.FindIndex(Function(x) String.Compare(x, subent.LayerName, True) = 0) < 0 Then
                            subent.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, subent.LayerName)
                            lynames.Add(subent.LayerName)
                        End If
                    End If
                    sPaths = Paths_Entity(subent, aImage)
                    'If aEnt.GraphicType = dxxGraphicTypes.Line Then
                    '    System.Diagnostics.Debug.WriteLine(aEnt.Descriptor & " :: " & sPaths.Item(1).Looop(1).CoordinatesP)
                    'End If
                    rPaths.Append(sPaths, bAddClones:=True)
                    rPaths.ExtentVectors.Append(sPaths.ExtentVectors)
                    SubEnts.Add(subent, bAddClone:=True)

                Next

#End Region 'Additional Segments add the paths of the additional segments
                'combine the paths with the same layer, color and linetype
                rPaths.Combine()
                'set instancing properties
                aInsts.Plane = aPlane


                'return the computed paths
                Return rPaths

            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aPline.SavePaths(rPaths, aSegs, aPathEntities:=SubEnts)
            End Try
        End Function
        Private Shared Function epths_PLINE(aPline As dxePolyline, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aPline)
            Dim aInsts As dxoInstances = aPline.Instances
            Dim aSegs As New TSEGMENTS(0)
            Try

                Dim segSegs As TSEGMENTS = Nothing
                Dim eVecs As New TVECTORS
                Dim segPath As TPATH
                Dim dsp As TDISPLAYVARS = aPline.DisplayStructure
                Dim aPlane As TPLANE = aPline.PlaneV
                Dim hndls As THANDLES = aPline.Handlez_Get
                Dim verts As TVERTICES = aPline.VerticesV.ProjectedTo(aPlane)

                Dim iGUID As String = aPline.ImageGUID
                Dim eGUID As String = aPline.GUID

                rPaths.Identifier = aPline.Identifier
                'get the lines and arcs that make up the polyline
                Dim aSegments As List(Of TSEGMENT) = dxfSegments.PolylineSegments(verts, aPlane, aPline.Closed, aDisplaySettings:=aPline.DisplaySettings, aLTScale:=aPline.LTScale, sGlobalWidth:=aPline.SegmentWidth, aIdentifier:="BOUNDS")
                aPline.VerticesV = verts

                For Each seg As TSEGMENT In aSegments
                    seg.ImageGUID = iGUID
                    seg.Identifier = "BOUNDS"
                    seg.OwnerGUID = eGUID

                    segPath = seg.GetPath(aPlane, segSegs, eVecs, False)
                    segPath.DisplayVars = dsp
                    rPaths.ExtentVectors.Append(eVecs)

                    rPaths.AddOrJoin(segPath, bIgnoreDisplayProperties:=True)
                    aSegs.Add(seg)
                Next

                'set instancing properties

                aInsts.Plane = aPline.PlaneV


                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aPline.SavePaths(rPaths, aSegs)

            End Try
        End Function
        Private Shared Function epths_POINT(aPoint As dxePoint, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aPoint)
            Dim aInsts As dxoInstances = aPoint.Instances
            Dim aSegs As New TSEGMENTS(0)
            Try
                Dim pdmd As dxxPointModes = dxxPointModes.None
                Dim devht As Double
                Dim sz As Double
                Dim ePath As TPATH
                rPaths.ExtentVectors.Add(aPoint.Vector)
                Dim aPlane As TPLANE = aPoint.PlaneV
                If aImage IsNot Nothing Then

                    pdmd = aImage.Header.PointMode
                    sz = aImage.Header.PointSize
                    devht = aImage.obj_DISPLAY.pln_VIEW.Height
                Else
                End If
                If pdmd = dxxPointModes.None Then pdmd = dxxPointModes.Dot
                If sz = 0 Then sz = -5
                If devht <= 0 Or String.Compare(aPoint.DisplayStructure.LayerName, "DefPoints", True) = 0 Then
                    pdmd = dxxPointModes.Dot
                End If
                If sz <= 0 Then ' sz < 0
                    sz = Math.Abs(sz)
                    sz = ((sz / 100) * devht)
                Else
                    sz = sz '* aImage.ToInches
                End If
                ePath = TPATH.POINT(pdmd, sz, aPlane, rPaths.ExtentVectors, aPoint.DisplayStructure)
                ePath.Plane = aPlane
                rPaths.Add(ePath)
                ePath.Linetype = dxfLinetypes.Continuous

                'set instancing properties
                aInsts.Plane = aPlane
                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aPoint.SavePaths(rPaths, aSegs)

            End Try
        End Function
        Private Shared Function epths_SHAPE(aShape As dxeShape, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aShape)
            Dim aInsts As dxoInstances = aShape.Instances
            Dim aSegs As New TSEGMENTS(0)
            '^returns the collection of lines defined by the vectors in the collection
            '~the segments returned are defined by the vectors in the collection in logical order (1 to count).
            Try
                Dim fname As String
                Dim idx As Integer
                Dim vShape As New TSHAPE
                Dim aShapes As New TSHAPES
                fname = aShape.ShapeFileName
                aShapes = aImage.Shapes.GetShapes(aShape.ShapeFileName, idx, True, aImage)
                If idx < 0 Then Return rPaths
                If Not aShapes.Contains(aShape.ShapeName) Then
                    vShape = aShapes.GetByShapeNumber(aShape.ShapeNumber, idx)
                Else
                    vShape = aShapes.Member(aShape.ShapeName)
                    idx = vShape.Index
                End If
                If idx <= 0 Then Return rPaths

                Dim aPlane As New TPLANE("")
                Dim wPlane As New TPLANE("")

                Dim aLims As TLIMITS
                Dim pVecs As TVECTORS
                Dim eVpts As New TPOINTS(0)
                Dim bPath As TPATH
                Dim dsp As TDISPLAYVARS
                Dim aTrns As New TTRANSFORMS
                Dim ht As Double
                Dim wf As Double
                Dim i As Integer
                Dim oblq As Double


                aShape.ShapeFileName = aShapes.FileName
                aShape.ShapeName = vShape.Name
                aShape.ShapeNumber = vShape.ShapeNumber
                dsp = aShape.DisplayStructure
                aPlane = aShape.PlaneV
                ht = aShape.Height
                If Not aShape.SaveExploded Then
                    wf = aShape.WidthFactor
                    oblq = aShape.ObliqueAngle
                End If
                If ht <= 0 Then ht = 1
                If wf <= 0 Then wf = 1
                If aShape.Rotation <> 0 Then
                    aPlane.Revolve(aShape.Rotation)
                End If
                'get the path on the world plane starting at 0,0
                aSegs = TSHAPES.ComputePath(aShapes, vShape, eVpts, aLims, 1, bIncludeMoveToPoint:=True)
                For i = 1 To aSegs.Count
                    aSegs.SetDisplayStructure(i, dsp)
                Next i
                aShape.ShapeCommands = vShape.PathCommands.ToList()
                pVecs = CType(vShape.Path, TVECTORS)
                rPaths.ExtentVectors = CType(eVpts, TVECTORS)
                aTrns.Add(TTRANSFORM.CreateScale(wPlane.Origin, ht * wf, ht, 1))
                'apply width factors and height
                TTRANSFORMS.Apply(aTrns, pVecs)
                TTRANSFORMS.Apply(aTrns, rPaths.ExtentVectors)
                TTRANSFORMS.Apply(aTrns, aSegs)
                'apply oblique angle
                If oblq <> 0 Then
                    pVecs = pVecs.ShearX(wPlane.Origin, oblq)
                    rPaths.ExtentVectors = rPaths.ExtentVectors.ShearX(wPlane.Origin, oblq)
                End If
                aTrns = TTRANSFORMS.CreateRotateToPlane(wPlane, aPlane, 0, True)
                'apply rotations and move to insertion point
                TTRANSFORMS.Apply(aTrns, pVecs)
                TTRANSFORMS.Apply(aTrns, rPaths.ExtentVectors)
                TTRANSFORMS.Apply(aTrns, aSegs)
                bPath = New TPATH(aShape.Domain, aPlane:=New dxfPlane(aPlane))
                bPath.AddLoop(pVecs)
                bPath.DisplayVars = aShape.DisplayStructure
                rPaths.Add(bPath)
                'set instancing properties
                aInsts.Plane = aPlane
                Return rPaths
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message)
                Return rPaths
            Finally
                aShape.SavePaths(rPaths, aSegs)
            End Try
        End Function
        Private Shared Function epths_SOLID(aSolid As dxeSolid, aImage As dxfImage) As TPATHS
            '^returns the collection of lines defined by the vectors in the collection
            '~the segments returned are defined by the vectors in the collection in logical order (1 to count).
            Dim rPaths As TPATHS = TPATHS.NullEnt(aSolid)
            Dim aInsts As dxoInstances = aSolid.Instances
            Dim aSegs As New TSEGMENTS(0)
            Try
                Dim aPlane As TPLANE = aSolid.PlaneV
                Dim bPath As TPATH
                aSolid.Planarize()
                bPath = New TPATH(aSolid.Domain, aSolid.DisplayStructure) With {.Linetype = dxfLinetypes.Continuous, .Filled = aSolid.Filled, .Identifier = aSolid.Identifier, .Plane = aPlane}
                bPath.AddLoop(aSolid.Vectors(False))
                rPaths.ExtentVectors = bPath.Looop(1).Clone
                rPaths.Add(bPath)
                aSegs = bPath.Looop(1).ToLineSegments()
                'set instancing properties
                aInsts.Plane = aSolid.PlaneV
                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aSolid.SavePaths(rPaths, aSegs)
            End Try
        End Function
        Private Shared Function epths_SYMBOL(aSymbol As dxeSymbol, aImage As dxfImage) As TPATHS
            Dim rPaths As TPATHS = TPATHS.NullEnt(aSymbol)
            Dim aInsts As dxoInstances = aSymbol.Instances
            Dim aSegs As New TSEGMENTS(0)
            Dim iGUID As String = aImage.GUID
            Dim eGUID As String = aSymbol.GUID
            Dim SubEnts As New dxfEntities() With {.Owner = eGUID, .ImageGUID = iGUID}

            Try
                Dim sEnts As List(Of dxfEntity)
                Dim j As Integer
                Dim aEnt As dxfEntity
                Dim Hi As Integer
                Dim ePths As TPATHS
                Dim ePth As TPATH
                Dim aHndl As String = String.Empty
                Dim rHndl As String = String.Empty
                Dim symLyr As String = String.Empty
                Dim symClr As dxxColors
                Dim symLt As String = String.Empty
                Dim symLwt As dxxLineWeights
                Dim layClr As dxxColors
                Dim layLT As String = String.Empty
                Dim layLwt As dxxLineWeights
                Dim pthLayer As String = String.Empty
                Dim pthLT As String = String.Empty
                Dim pthClr As dxxColors
                Dim pthLwt As dxxLineWeights


                If aSymbol.Handle = "" Or aSymbol.ImageGUID <> iGUID Then
                    aSymbol.ClearHandles()
                End If
                Dim ltscl As Double = 0
                aSymbol.LCLGet(symLyr, symClr, symLt, symLwt, ltscl)
                aHndl = aSymbol.Handle
                dxfImageTool.LayerPropsGET(aImage, symLyr, layClr, layLT, layLwt, True)
                '============= create the entities that make up the symbols block ============
                sEnts = dxoSymbolTool.CreateSymbolEntities(aImage, aSymbol)
                Hi = 1
                For Each aEnt In sEnts
                    aEnt.ImageGUID = iGUID
                    aEnt.OwnerGUID = eGUID
                    If Not aEnt.Suppressed Then
                        If aEnt.GraphicType <> dxxGraphicTypes.Text Then
                            aEnt.LCLSet(symLyr, symClr, symLt)
                        End If
                        aEnt.UpdatePath(False, aImage)
                        ePths = aEnt.Paths
                        pthLayer = ""
                        Select Case aEnt.GraphicType
                            Case dxxGraphicTypes.Point
                                pthLayer = "DefPoints"
                                pthClr = symClr
                                pthLwt = symLwt
                                pthLT = dxfLinetypes.Continuous
                            Case dxxGraphicTypes.Text
                                pthClr = aEnt.Color
                                pthLwt = layLwt
                                pthLT = dxfLinetypes.Continuous
                            Case dxxGraphicTypes.Insert
                                pthClr = symClr
                                pthLT = dxfLinetypes.Continuous
                                pthLwt = layLwt
                            Case dxxGraphicTypes.Solid
                                pthClr = symClr
                                pthLT = dxfLinetypes.Continuous
                                pthLwt = layLwt
                            Case Else
                                pthLayer = symLyr
                                pthClr = symClr
                                pthLwt = symLwt
                                pthLT = symLt
                        End Select
                        If pthClr = dxxColors.ByLayer And pthLayer = symLyr Then
                            pthClr = layClr
                        End If
                        For j = 1 To ePths.Count
                            ePth = ePths.Item(j)
                            ePth.Color = pthClr
                            ePth.Linetype = pthLT
                            ePth.LayerName = pthLayer
                            ePths.SetItem(j, ePth)
                            rPaths.Add(ePth)
                        Next j
                        'ePths.Members(0).Loops(0).Print()
                        rPaths.ExtentVectors.Append(ePths.ExtentVectors)
                        SubEnts.Add(aEnt)
                    End If
                Next

                rPaths.Combine()
                'set instancing properties
                aInsts.Plane = aSymbol.PlaneV

                Return rPaths
            Catch ex As Exception
                If aImage IsNot Nothing Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message) Else Throw ex
                Return rPaths
            Finally
                aSymbol.SavePaths(rPaths, aSegs, aPathEntities:=SubEnts)

            End Try
        End Function
        Private Shared Function epths_TABLE(aTable As dxeTable, aImage As dxfImage) As TPATHS
            If aTable Is Nothing Then Return Nothing
            Dim rPaths As TPATHS = TPATHS.NullEnt(aTable)
            Dim aInsts As dxoInstances = aTable.Instances
            Dim aSegs As New TSEGMENTS(0)
            Dim iGUID As String = aImage.GUID
            Dim eGUID As String = aTable.GUID
            Dim SubEnts As New dxfEntities() With {.Owner = eGUID, .ImageGUID = iGUID}

            If Not aTable.GetImage(aImage) Then aImage = dxfGlobals.New_Image()
            Try
                Dim ePaths As TPATHS
                Dim aPlane As TPLANE = aTable.PlaneV


                Dim extPts As New TVECTORS(0)
                rPaths.Identifier = aTable.Identifier
                Dim sEnts As colDXFEntities = dxeTable.CreateSubEntities(aTable, aImage, extPts)
                rPaths.ExtentVectors = extPts
                For i As Integer = 1 To sEnts.Count
                    Dim aEnt As dxfEntity = sEnts.Item(i)
                    If Not aEnt.Suppressed Then
                        aEnt.UpdatePath(False, aImage)
                        ePaths = aEnt.Paths
                        rPaths.Append(ePaths)
                    End If
                    SubEnts.Add(aEnt)
                Next i
                rPaths.Combine()

                'set instancing properties
                aInsts.Plane = aPlane
                'aInsts.ExtentPts = rPaths.ExtentVectors
                Return rPaths
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message)
                Return rPaths
            Finally
                aTable.SavePaths(rPaths, aSegs, aPathEntities:=SubEnts)

            End Try
        End Function
        Private Shared Function epths_TEXT(aString As dxeText, aImage As dxfImage) As TPATHS
            '#1a string entity to build the sub-strings and characters for
            '^creates the the characters and sub-string for the passed string entity
            Dim rPaths As TPATHS = TPATHS.NullEnt(aString)
            Dim aInsts As dxoInstances = aString.Instances
            Dim aSegs As New TSEGMENTS(0)
            Dim rStrings As New dxfStrings()
            Try
                If Not aString.GetImage(aImage) Then aImage = dxfGlobals.New_Image()

                Dim aPlane As TPLANE = aString.PlaneV

                Dim tSty As dxoStyle
                Dim tStyles As dxfTable = aImage.Styles
                Dim subStrs As TSTRINGS
                Dim ap1 As TVECTOR = aString.AlignmentPt1V.ProjectedTo(aPlane)
                Dim ap2 As TVECTOR = aString.AlignmentPt2V.ProjectedTo(aPlane)
                '                Dim bDrawTag As Boolean = as

                Dim bNullText As Boolean
                Dim Stl As dxoStyle = Nothing
                aString.AlignmentPt1V = ap1
                Dim vertical As Boolean = aString.Vertical
                'init
                'get the text style
                If aString.Domain <> dxxDrawingDomains.Screen Then
                    aString.TextStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aString.TextStyleName, rEntry:=Stl)
                    tSty = DirectCast(aImage.Styles.Entry(aString.TextStyleName), dxoStyle)
                Else
                    tSty = aImage.Screen.TextStyle
                End If
                If aString.EntityType = dxxEntityTypes.Attribute Then
                    Console.WriteLine(aString.TextString)
                End If
                'If (subStrs.Alignment = dxxMTextAlignments.Fit Or subStrs.Alignment = dxxMTextAlignments.Aligned) And Not bFit Then subStrs.Alignment = dxxMTextAlignments.BaselineLeft
                'get the character patterns at 0,0 baselineleft
                'and assign the line numbers and display settings based
                'on the in string formatting
                '=============================================================================================
                rStrings = New dxfStrings(aText:=aString, aStyle:=tSty, aImage:=aImage)
                rPaths = New TPATHS(aString.Domain, rStrings.GeneratePaths(False), aString.GUID) ' subStrs.GeneratePaths(False)
                subStrs = New TSTRINGS(rStrings)
                '=============================================================================================
                rPaths.EntityGUID = aString.GUID
                rPaths.Identifier = aString.Identifier
                bNullText = rStrings.CharacterCount <= 0
                'extent pts are just the bounding rectangle corners
                If Not bNullText Then
                    rPaths.ExtentVectors = rStrings.SubStringExtentVectors(False)
                Else
                    rPaths.ExtentVectors = New TVECTORS(aString.AlignmentPt1V)

                End If



                If rStrings.CorrectedAlignment <> dxxMTextAlignments.AlignUnknown Then
                    aString.Alignment = rStrings.CorrectedAlignment
                    aString.AlignmentPt1V = rStrings.CorrectedAlignmentPt
                End If

                'set instancing properties
                aInsts.Plane = aPlane

                If rStrings.Alignment <> dxxMTextAlignments.Aligned And rStrings.Alignment <> dxxMTextAlignments.Fit Then
                    aString.AlignmentPt2V = ap1
                Else
                    aString.AlignmentPt2V = ap2
                    aString.Rotation = rStrings.Rotation
                End If
                aString.FitFactor = 1
                If rStrings.Count > 0 And rStrings.Alignment = dxxMTextAlignments.Fit Then
                    aString.FitFactor = rStrings.Item(0).FitFactor
                End If
                'If Not bDrawTag Then aString.TextString = rStrings.FormatString

                Return rPaths
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxfPaths", ex.Message)
                Return New TPATHS(aString.Domain, aString.GUID)
            Finally
                aString.Strings = rStrings
                aString.SavePaths(rPaths, aSegs)

                aString.State = dxxEntityStates.Steady

            End Try
        End Function
#End Region 'Methods
    End Class 'dxfPaths
End Namespace
