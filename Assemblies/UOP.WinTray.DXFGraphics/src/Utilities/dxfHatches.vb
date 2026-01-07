

Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfHatches
#Region "Methods"
        Friend Shared Function BoundLoops_Entities(aEntities As IEnumerable(Of dxfEntity), aHatchPlane As TPLANE, Optional aBezierSegments As Integer = 100, Optional bEntities As IEnumerable(Of dxfEntity) = Nothing) As TBOUNDLOOPS
            Dim _rVal As New TBOUNDLOOPS(0)

            If aEntities Is Nothing And bEntities Is Nothing Then Return _rVal

            Dim aEnt As dxfEntity


            Dim idx As Integer
            If aEntities IsNot Nothing Then
                For i As Integer = 1 To aEntities.Count
                    aEnt = aEntities(i - 1)
                    Dim aSet As TBOUNDLOOPS = dxfHatches.BoundLoops_Entity(aEnt, aHatchPlane, aBezierSegments)
                    If aSet.Count > 0 Then
                        _rVal.ExtentPts.Append(aSet.ExtentPts)
                        For j As Integer = 1 To aSet.Count
                            If aSet.Item(j).Segments.Count > 0 Then
                                _rVal.Add(aSet.Item(j))
                            End If
                        Next j
                    End If
                Next i
            End If
            If bEntities Is Nothing Then Return _rVal
            idx = _rVal.Count + 1
            'these should be end to end arcs and lines
            For i As Integer = 1 To bEntities.Count
                aEnt = bEntities(i - 1)
                Dim aArL As TSEGMENT = aEnt.ArcLineStructure(False)
                _rVal.AddLoopAEL(idx, aArL, dxxLoopTypes.Polyline)
            Next i

            Return _rVal
        End Function
        Friend Shared Function BoundLoops_Entity(aEntity As dxfEntity, aHatchPlane As TPLANE, Optional aBezierSegments As Integer = 100) As TBOUNDLOOPS
            Dim _rVal As New TBOUNDLOOPS(0)

            If aEntity Is Nothing Then Return _rVal
            aEntity.UpdatePath()
            Dim aTxts As colDXFEntities = Nothing
            Dim sTx As dxeText
            Dim ePl As TPLANE = aEntity.PlaneV
            Dim aPts As New TVECTORS(0)
            Dim aRec As TPLANE
            Dim aSet As New TBOUNDLOOPS
            Dim bSet As New TBOUNDLOOPS
            Dim cSet As New TBOUNDLOOPS

            Dim bOrthog As Boolean
            Dim aTx As dxeText = Nothing
            Dim diff As String = String.Empty
            Dim nang As Double = 0

            Dim bPlaner As Boolean = TPLANES.Compare(ePl, aHatchPlane, 3, False, False, bOrthog, nang, diff)

            'entities that are on an orthognal plane have no hatch bounds on the hatch plane
            If bOrthog Then Return _rVal
            Select Case aEntity.GraphicType
                Case dxxGraphicTypes.Arc
                    Dim aA As dxeArc = aEntity
                    If bPlaner Then
                        _rVal.AddLoopAEL(_rVal.Count + 1, dxfEntity.ArcLine(aA), dxxLoopTypes.CircularArc)
                    Else
                        aPts = aA.ArcStructure.PhantomPoints(aBezierSegments, True)
                    End If
                Case dxxGraphicTypes.Bezier
                    Dim aB As dxeBezier = aEntity
                    aPts = aB.BezierStructure.PhantomPoints(aBezierSegments)
                Case dxxGraphicTypes.Dimension
                    Dim aD As dxeDimension = aEntity
                    aTxts = aD.SubEntities.TextEntities
         'no hatch bounds
                Case dxxGraphicTypes.Ellipse
                    Dim aE As dxeEllipse = aEntity
                    If bPlaner Then
                        _rVal.AddLoopAEL(_rVal.Count + 1, dxfEntity.ArcLine(aE), dxxLoopTypes.EllipticalArc)
                    Else
                        aPts = aE.ArcStructure.PhantomPoints(aBezierSegments)
                    End If
                Case dxxGraphicTypes.Hatch
                    'Dim aH As dxeHatch = aEntity
         'no hatch bounds
                Case dxxGraphicTypes.Hole
                    Dim aHl As dxeHole = aEntity
                    If aHl.Depth > 0 Then
                        bSet = dxfHatches.BoundLoops_Entities(aHl.SubEntities, aHatchPlane, aBezierSegments)
                    Else
                        bSet = dxfHatches.BoundLoops_Entities(New colDXFEntities(aHl.BoundingEntity), aHatchPlane, aBezierSegments)
                    End If
                Case dxxGraphicTypes.Insert
                    Dim aI As dxeInsert
                    aI = aEntity
         'no hatch bounds
                Case dxxGraphicTypes.Leader
                    Dim aL As dxeLeader = aEntity
                    aTxts = New colDXFEntities(aL.MText)
                Case dxxGraphicTypes.Line
                    Dim aLn As dxeLine = aEntity
                    _rVal.AddLoopAEL(_rVal.Count + 1, dxfEntity.ArcLine(aLn), dxxLoopTypes.Line)
                Case dxxGraphicTypes.Point
                    Dim aP As dxePoint = aEntity
         'no hatch bounds
                Case dxxGraphicTypes.Polygon
                    Dim aPg As dxePolygon
                    aPg = aEntity
                    aSet = dxfHatches.BoundLoops_Entities(Nothing, aHatchPlane, aBezierSegments, CType(aPg.Segments, List(Of dxfEntity)))
                    If Not aPg.SuppressAdditionalSegments Then bSet = dxfHatches.BoundLoops_Entities(aPg.AdditionalSegments, aHatchPlane, aBezierSegments)
                Case dxxGraphicTypes.Polyline
                    Dim aPl As dxePolyline = aEntity
                    aSet = dxfHatches.BoundLoops_Entities(Nothing, aHatchPlane, aBezierSegments, CType(aPl.Segments, List(Of dxfEntity)))
                Case dxxGraphicTypes.Solid
                    Dim aSl As dxeSolid
                    aSl = aEntity
                    aPts = New TVECTORS
                    aPts.Add(aSl.Vertex1V)
                    aPts.Add(aSl.Vertex2V)
                    aPts.Add(aSl.Vertex3V)
                    If Not aSl.Triangular Then aPts.Add(aSl.Vertex4V)
                    aPts.Add(aSl.Vertex1V)
                Case dxxGraphicTypes.Symbol
                    Dim aSy As dxeSymbol
                    aSy = aEntity
                    aTxts = aSy.Entities.GetByGraphicType(dxxGraphicTypes.Text)
                Case dxxGraphicTypes.Table
                    Dim aTB As dxeTable
                    aTB = aEntity
                    aTxts = aTxts.TextEntities
                Case dxxGraphicTypes.Text
                    aTx = aEntity
                    aRec = aTx.Bounds.Stretched(0.4 * aTx.TextHeight, True, True, bMaintainOrigin:=True)
                    aPts = aRec.Corners(True, True)
                Case dxxGraphicTypes.Shape
                    Dim aShp As dxeShape
                    aShp = aEntity
                    aRec = aShp.Bounds
                    aPts = aRec.Corners(True, True)
            End Select
            If aPts.Count > 0 Then
                _rVal.AddLineSegments(aPts, aHatchPlane, False, False)
            End If
            If aTxts IsNot Nothing Then
                For i As Integer = 1 To aTxts.Count
                    sTx = aTxts.Item(i)
                    aRec = aTx.Bounds.Stretched(0.4 * sTx.TextHeight, True, True)
                    aPts = aRec.Corners(True, True)
                    _rVal.AddLineSegments(aPts, aHatchPlane, False, False, True)
                Next i
            End If
            If aSet.Count > 0 Then _rVal.Append(aSet)
            If bSet.Count > 0 Then _rVal.Append(bSet)
            If cSet.Count > 0 Then _rVal.Append(cSet)
            Return _rVal
        End Function
        Friend Shared Function CreateDashPts(aPatternLength As Double, bGridPts As Boolean, aXDirection As TVECTOR, aYDirection As TVECTOR, aLine As TLINE, aOrigin As TVECTOR, aPatternLine As THATCHLINE, Optional aScaler As Double = 1) As TVECTORS
            Dim _rVal As New TVECTORS
            _rVal = New TVECTORS
            Dim vRet As TVECTORS
            Dim aDashCount As Integer
            Dim aDashes() As Double
            Dim vSort As TVECTOR
            Dim aSP As TVECTOR
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim d1 As Double
            Dim totPat As Double
            Dim i As Integer
            Dim aDir As TVECTOR
            Dim d2 As Double
            Dim aDash As Double
            Dim aStep As Double
            Dim llen As Double
            Dim bOnLine As Boolean
            Dim wLine As TLINE
            Dim aFlg As Boolean
            Dim d2SP As Double
            Dim d2EP As Double
            Try
                'get the origin that must be on the path of the line but
                'not necessarily within the start and end points
                vSort = aOrigin
                'get the direction of the line
                totPat = aPatternLength
                llen = aLine.SPT.DistanceTo(aLine.EPT, 5) 'Round(lLen, 5)
                aDashCount = aPatternLine.DashCount
                aDashes = aPatternLine.Dashes
                vRet = _rVal
                If aDashCount <= 0 Or Math.Round(totPat, 5) <= 0.0001 Or llen <= 0.0001 Then
                    _rVal.AddLine(aLine.SPT, aLine.EPT)
                    Return _rVal
                End If
                'get the direction from the lines start pt to the origin
                aDir = vSort.DirectionTo(aLine.SPT, False, aFlg, d1)
                'determine how many patterns between the origin and the line start pt
                'set the dash start back to somewhere before the line start
                If Not aFlg Then
                    If aDir.Equals(aXDirection, 2) Then
                        'if the origin is in line with the direction of the line
                        If d1 <= totPat Then
                            wLine.SPT = vSort + (aXDirection * -totPat)
                        Else
                            wLine.SPT = vSort + (aXDirection * (Fix(d1 / totPat) * totPat))
                        End If
                    Else
                        'if the origin is the opposite direction the direction of the line
                        wLine.SPT = vSort + (aXDirection * -totPat)
                    End If
                End If
                '    k = Fix(wLine.SPT.DistanceTo( aLine.EPT) / totPat) + 1
                '    wLine.EPT = wLine.SPT+ ( aXDirection *( k * totPat))
                wLine.EPT = aLine.EPT + (aXDirection * totPat)
                'vecs_AddArrow vRet, aLine.SPT, aLine.SPT.DirectionTo( aLine.EPT), aYDirection, 0.03
                ''vecs_AddArrow vRet, aLine.EPT, aLine.EPT.DirectionTo( aLine.SPT), aYDirection, 0.03
                '
                'vecs_AddArrow vRet, wLine.SPT, aXDirection, aYDirection, 0.35
                'vecs_AddArrow vRet, wLine.EPT, (aXDirection* -1), aYDirection, 0.09
                'get the total distance to create dashes on
                d2EP = wLine.SPT.DistanceTo(aLine.EPT)
                d2SP = wLine.SPT.DistanceTo(aLine.SPT)
                d2 = 0
                aSP = wLine.SPT
                bOnLine = False
                Do Until d2 >= d2EP
                    'loop on the dash length
                    For i = 0 To aDashCount - 1
                        aDash = aDashes(i) * aScaler
                        aStep = Math.Abs(aDash)
                        'increment d2 with the current dash length
                        'jump the ep forward from the current start
                        If aDash < 0 Then
                            '(negatives mean pen up - nothing added!)
                        ElseIf aDash = 0 Then 'add a dot
                            If bOnLine Then
                                If bGridPts Then
                                    vRet.Add(aSP, TVALUES.ToByte(dxxVertexStyles.PIXEL))
                                Else
                                    vRet.AddLine(aSP, aSP)
                                End If
                            End If
                        Else
                            If bOnLine Then v1 = aSP Else v1 = aLine.SPT
                            If d2 + aStep > d2SP Then
                                If d2 + aStep <= d2EP Then
                                    v2 = aSP + (aXDirection * aStep)
                                Else
                                    v2 = aLine.EPT
                                End If
                                vRet.AddLine(v1, v2)
                            End If
                        End If
                        d2 += aStep
                        aSP = aSP + (aXDirection * aStep)
                        If d2 >= d2SP Then bOnLine = True
                        If d2 > d2EP Then Exit Do
                    Next i
                Loop
                _rVal = vRet
                '    halfLen = lLen / 2
                '
                '
                '    'get the direction from the lines start pt to the origin
                '    aDir = aLine.SPT.DirectionTo( vSort, , , d1)
                '
                '    'determine how many patterns between the origin and the line start pt
                '    k = Fix(d1 / totPat) + 1
                '
                '    'set the dash start back to somewhere before the line start
                '    If aDir.Equals( aXDirection,  2) Then
                '        'if the origin is in line with the direction of the line
                '        wLine.EPT = vSort.Projected( aXDirection, -k * totPat, True)
                '    Else
                '        'if the origin is the opposite direction the direction of the line
                '        wLine.EPT = vSort.Projected( aXDirection, k * totPat, True)
                '    End If
                '    wLine.SPT = wLine.EPT.Projected( aXDirection, -totPat, True)
                '
                ''    k = Fix(lLen / totPat) + 1
                ''     wLine.EPT = wLine.SPT.Projected( aXDirection, k * totPat, True)
                '
                '
                '' vRet.Add( wLine.SPT, PIXEL)
                '' vRet.Add( wLine.EPT, PIXEL)
                '
                '
                '    'get the total distance to create dashes on
                '    d1 = wLine.SPT.DistanceTo( aLine.EPT)
                '    d2 = 0
                '    bBeforMidPt = True
                '    mpt = aLine.SPT.interpolate( aLine.EPT, 0.5)
                '
                '    'leap frog from the dash start pt down the line making the dashes as you go
                '    'we are done when d2 exceeds the total dashing distance
                '    aSP = wLine.SPT
                '    dmpt = aSP.DistanceTo( mpt)
                '
                '    Do Until d2 >= d1
                '        'loop on the dash length
                '        For i = 0 To aDashCount - 1
                '            aDash = aDashes(i)
                '            astep = Abs(aDash) * aScaler
                '            'increment d2 with the current dash length
                '            d2 = d2 + astep
                '            'jump the ep forward from the current start
                '            If aDash < 0 Then
                '                '(negatives mean pen up - no line added!)
                '                aEP = aSP + ( aXDirection* astep)
                '            ElseIf aDash = 0 Then
                '
                '                aEP = aSP
                '                If mpt.DistanceTo( aSP, 5) <= halfLen Then
                '                    If bGridPts Then
                '                         vRet.Add( aSP, PIXEL)
                '                    Else
                '                        vecs_AddLine vRet, aSP, aSP
                '                    End If
                '
                '                End If
                '            Else
                '
                '                aEP = aSP.Projected( aXDirection, astep, True)
                '
                '                If bBeforMidPt Then
                '                    'before we get to the midpt the line is only kept if the end pt is on the line
                '
                '                    bOnLine = mpt.DistanceTo( aEP, 5) < halfLen
                '
                '                    If bOnLine Then
                ''vRet.Add( aEP, PIXEL)
                '
                '                        If mpt.DistanceTo( aSP, 5) > halfLen Then
                '                            aSP = aLine.SPT
                '                        End If
                '                        vecs_AddLine vRet, aSP, aEP
                '                    End If
                '
                '                Else
                '                    'after we get to the midpt the line is only kept if the start pt is on the line
                '                    bOnLine = mpt.DistanceTo( aSP, 5) < halfLen
                '
                '                    If bOnLine Then
                '                        If mpt.DistanceTo( aEP, 5) > halfLen Then
                '                             aEP = aLine.EPT
                '                        End If
                '                        vecs_AddLine vRet, aSP, aEP
                '                    End If
                '                End If
                '            End If
                '
                '
                '            'move the start pt to the end of the last dash
                '            aSP = aEP
                '            'use the distance to the mid pt to determine when we cross half way
                '
                '            dmpt = dmpt - astep
                '            If dmpt < 0 Then bBeforMidPt = False
                '
                '
                '        Next i
                '
                '    Loop
                '    dxfHatches.CreateDashPts = vRet
            Catch ex As Exception
                _rVal.AddLine(aLine.SPT, aLine.EPT)
            End Try
            Return _rVal
        End Function
        Friend Shared Function GetBoundLoops(aEntities As colDXFEntities, aPlane As TPLANE, aImage As dxfImage) As TBOUNDLOOPS
            Dim _rVal As New TBOUNDLOOPS(0)

            If aEntities Is Nothing Then Return _rVal
            Dim i As Integer
            Dim aEnt As dxfEntity
            'loop on the bounding entities
            For i = 1 To aEntities.Count
                aEnt = aEntities.Item(i)
                'use the current
                If aImage IsNot Nothing Then
                    If aEnt.Handle <> "" Then
                        aEnt = aImage.Entities.Item(aEnt.Handle)
                        If aEnt Is Nothing Then
                            aEnt = aEntities.Item(i)
                        End If
                    End If
                End If
                'get the entities created when the boundary entity is project to the current plane
                _rVal.Append(dxfHatches.BoundLoops_Entity(aEnt, aPlane))
            Next i
            Return _rVal
        End Function
        Friend Shared Function GetBoundaryLoopProps(aHatch As dxeHatch, aOCS As TPLANE, aTransforms As TTRANSFORMS) As TPROPERTIES
            Dim _rVal As New TPROPERTIES

            If aHatch Is Nothing Then Return _rVal

            Dim bArL As TSEGMENT
            'Dim bAddit As Boolean
            'Dim segCnt As Integer
            Dim v1 As TVECTOR
            Dim aLn As TLINE
            Dim aAr As TARC
            'Dim lFlag As Long

            Dim bndLoops As TBOUNDLOOPS = aHatch.BoundaryLoops
            '=========== begin path data
            For i As Integer = 1 To bndLoops.Count
                Dim aProps As New TPROPERTIES("")
                Dim aLoop As TBOUNDLOOP = bndLoops.Item(i)
                Dim aArL As TSEGMENT = aLoop.Segments.Item(1, True)
                TTRANSFORMS.Apply(aTransforms, aArL)
                'If i <= bndLoops.Count Then lFlag = 1 Else lFlag = 16
                'bAddit = False
                'segCnt = 0
                Select Case aLoop.LoopType
                    Case dxxLoopTypes.Line
                        aLn = aArL.LineStructure
                        aProps.Add(New TPROPERTY(92, 33, "Loop Type Bit Code", dxxPropertyTypes.BitCode))
                        aProps.Add(New TPROPERTY(93, 1, "Edge Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                        aProps.Add(New TPROPERTY(72, 1, "Line Flag", dxxPropertyTypes.Switch))
                        v1 = aLn.SPT.WithRespectTo(aOCS)
                        aProps.AddPoint(v1, "Line StartPt", 10)
                        v1 = aLn.EPT.WithRespectTo(aOCS)
                        aProps.AddPoint(v1, "Line EndPt", 11)
                    Case dxxLoopTypes.EllipticalArc
                        aAr = aArL.ArcStructure
                        aProps.Add(New TPROPERTY(92, 1, "Loop Type Bit Code", dxxPropertyTypes.BitCode))
                        aProps.Add(New TPROPERTY(93, 1, "Edge Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                        aProps.Add(New TPROPERTY(72, 3, "Ellipse Flag", dxxPropertyTypes.Switch))
                        v1 = aAr.Plane.Origin.WithRespectTo(aOCS)
                        aProps.AddPoint(v1, "Ellipse Center", 10)
                        v1 = aAr.Plane.Origin + (aOCS.XDirection * aAr.Radius)
                        v1 = v1.WithRespectTo(aOCS)
                        aProps.AddPoint(v1, "Ellipse Major Axis End", 11)
                        aProps.Add(New TPROPERTY(40, aAr.MinorRadius / aAr.Radius, "Ellipse ratio", dxxPropertyTypes.dxf_Double))
                        aProps.Add(New TPROPERTY(50, aAr.StartAngle, "ellipse Start Angle", dxxPropertyTypes.dxf_Double))
                        aProps.Add(New TPROPERTY(51, aAr.EndAngle, "ellipse End Angle", dxxPropertyTypes.dxf_Double))
                        aProps.Add(New TPROPERTY(73, Not aAr.ClockWise, "counter clockwise flag", dxxPropertyTypes.Switch))
                    Case dxxLoopTypes.CircularArc
                        aAr = aArL.ArcStructure
                        aProps.Add(New TPROPERTY(92, 16, "Loop Type Bit Code", dxxPropertyTypes.BitCode))
                        aProps.Add(New TPROPERTY(93, 1, "Edge Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                        aProps.Add(New TPROPERTY(72, 2, "Arc Flag", dxxPropertyTypes.Switch))
                        v1 = aAr.Plane.Origin.WithRespectTo(aOCS)
                        aProps.AddPoint(v1, "Arc Center", 10)
                        aProps.Add(New TPROPERTY(40, aAr.Radius, "Radius", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        aProps.Add(New TPROPERTY(50, aAr.StartAngle, "Arc Start Angle", dxxPropertyTypes.dxf_Double))
                        aProps.Add(New TPROPERTY(51, aAr.EndAngle, "Arc End Angle", dxxPropertyTypes.dxf_Double))
                        aProps.Add(New TPROPERTY(73, Not aAr.ClockWise, "counter clockwise flag", dxxPropertyTypes.Switch))
                    Case dxxLoopTypes.Polyline
                        aProps.Add(New TPROPERTY(92, 1, "Loop Type Bit Code", dxxPropertyTypes.BitCode))
                        aProps.Add(New TPROPERTY(93, aLoop.Segments.Count, "Edge Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                        For j As Integer = 1 To aLoop.Segments.Count
                            bArL = aLoop.Segments.Item(j)
                            TTRANSFORMS.Apply(aTransforms, bArL)
                            If aArL.IsArc Then
                                aAr = bArL.ArcStructure
                                aProps.Add(New TPROPERTY(72, 2, "Arc Flag", dxxPropertyTypes.Switch))
                                v1 = aAr.Plane.Origin.WithRespectTo(aOCS)
                                aProps.AddPoint(v1, "Arc Center", 10)
                                aProps.Add(New TPROPERTY(40, aAr.Radius, "Radius", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                                aProps.Add(New TPROPERTY(50, aAr.StartAngle, "Arc Start Angle", dxxPropertyTypes.dxf_Double))
                                aProps.Add(New TPROPERTY(51, aAr.EndAngle, "Arc End Angle", dxxPropertyTypes.dxf_Double))
                                aProps.Add(New TPROPERTY(73, Not aAr.ClockWise, "counter clockwise flag", dxxPropertyTypes.Switch))
                            Else
                                aLn = bArL.LineStructure
                                aProps.Add(New TPROPERTY(72, 1, "Line Flag", dxxPropertyTypes.Switch))
                                v1 = aLn.SPT.WithRespectTo(aOCS)
                                aProps.AddPoint(v1, "Line StartPt", 10)
                                v1 = aLn.EPT.WithRespectTo(aOCS)
                                aProps.AddPoint(v1, "Line EndPt", 11)
                            End If
                            aLoop.Segments.SetItem(j, bArL)
                        Next j
                End Select
                aProps.Add(New TPROPERTY(97, 0, "Source Object Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                _rVal.Append(aProps)
                aLoop.Segments.SetItem(i, aArL)
            Next i
            Return _rVal
        End Function
        Friend Shared Function GetHatchLineProps(aHatch As TPROPERTIES, Optional aScaleFactor As Double = 0.0, Optional aRotation As Double = 0.0) As TPROPERTIES
            Dim _rVal As New TPROPERTIES("HATCH LINE")
            If aHatch.Count <= dxfGlobals.CommonProps Then Return _rVal
            Dim aPat As THATCHPATTERN = dxfHatches.PatternFromHatch(aHatch)

            Dim SF As Double = aHatch.ValueD("*ScaleFactor")
            Dim rot As Double = aHatch.GCValueD(52)

            If SF <= 0 Then SF = 1
            If aScaleFactor <> 0 Then SF *= Math.Abs(aScaleFactor)

            For i As Integer = 1 To aPat.HatchLineCnt
                Dim hLine As THATCHLINE = aPat.HatchLines(i - 1)
                Dim aStr As String = "Def Line" & i
                Dim v1 As New TVECTOR(hLine.DeltaX, hLine.DeltaY, 0)
                If hLine.Angle <> 0 Then
                    v1.RotateAbout(New TVECTOR, TVECTOR.WorldZ, hLine.Angle, False)
                End If
                _rVal.Add(New TPROPERTY(53, TVALUES.NormAng(hLine.Angle + rot + aRotation, False, True, True), $"{aStr } Angle", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New TPROPERTY(43, hLine.OriginX * SF, $"{aStr } Origin X", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New TPROPERTY(44, hLine.OriginY * SF, $"{aStr }Origin Y", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New TPROPERTY(45, v1.X * SF, $"{aStr } Offset X", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New TPROPERTY(46, v1.Y * SF, $"{aStr } Offset Y", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New TPROPERTY(79, hLine.DashCount, $"{aStr } Dash Count", dxxPropertyTypes.dxf_Integer))
                If hLine.DashCount > 0 Then
                    For j As Integer = 0 To hLine.DashCount - 1
                        _rVal.Add(New TPROPERTY(49, hLine.Dashes(j) * SF, $"{aStr }Dash Length {j + 1}", dxxPropertyTypes.dxf_Double))
                    Next j
                End If
            Next i
            Return _rVal
        End Function
        Friend Shared Function GridPointsPattern(aPitch As dxxPitchTypes, aHPitch As Double, aVPitch As Double) As THATCHPATTERN
            Dim _rVal As New THATCHPATTERN With {
                .Name = "_GRIDPOINTS",
                .Description = "Grid Points",
                .HatchLineCnt = 1
            }
            ReDim _rVal.HatchLines(0)
            _rVal.HatchLines(0).DashCount = 2
            ReDim _rVal.HatchLines(0).Dashes(0 To 1)
            If aPitch = dxxPitchTypes.Triangular Or aPitch = dxxPitchTypes.InvertedTriangular Then
                _rVal.HatchLines(0).Dashes(0) = 0
                _rVal.HatchLines(0).Dashes(1) = -aHPitch
                _rVal.HatchLines(0).DeltaX = 0.5 * aHPitch
                _rVal.HatchLines(0).DeltaY = aVPitch
                If aPitch = dxxPitchTypes.InvertedTriangular Then
                    _rVal.HatchLines(0).OriginX = 0.5 * aHPitch
                End If
            Else
                _rVal.HatchLines(0).Dashes(0) = 0
                _rVal.HatchLines(0).Dashes(1) = -aHPitch
                _rVal.HatchLines(0).DeltaY = aVPitch
            End If
            Return _rVal
        End Function
        Friend Shared Function Paths(aBasePath As TPATH, ByRef gProps As TGRID, bGridPts As Boolean, Optional aOrigin As dxfVector = Nothing) As TPATH
            Dim _rVal As TPATH

            Dim bndRec As TPLANE
            Dim aPlane As TPLANE
            Dim bPlane As TPLANE
            Dim aOCS As TPLANE
            Dim ips As TVECTORS
            Dim bndCrns As TVECTORS
            Dim pathVecs As TVECTORS
            Dim aDir As TVECTOR
            Dim xDir As TVECTOR
            Dim yDir As TVECTOR
            Dim stepDir As TVECTOR
            Dim lorg As TVECTOR
            Dim org As TVECTOR
            Dim v1 As TVECTOR
            Dim vSort As TVECTOR
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim hLine As New TLINE
            Dim iLine As New TLINE
            Dim yLine As New TLINE
            Dim vDashPts As TVECTORS
            Dim li As Integer
            Dim j As Integer
            Dim k As Integer
            Dim lcnt As Integer
            Dim diag As Double
            Dim yStep As Double
            Dim SF As Double
            Dim d1 As Double
            Dim d2 As Double
            Dim ang As Double
            Dim patLength As Double
            Dim stepLim As Double
            Dim aFlg As Boolean
            Dim iLp As Integer = 1
            _rVal = New TPATH(aBasePath) With {.GraphicType = dxxGraphicTypes.Hatch}
            If aBasePath.LoopCount = 0 Then
                _rVal.AddLoop(New TVECTORS(0))
            Else
                _rVal.AddLoop(New TVECTORS(0))
                iLp = _rVal.LoopCount
            End If
            Dim aLoop As TVECTORS = _rVal.Looop(iLp)
            'get the segments of the hatch boundary
            Dim bndLoops As TBOUNDLOOPS = gProps.BoundaryLoops
            'no edges no hatch!
            If bndLoops.Count <= 0 Or bndLoops.ExtentPts.Count <= 1 Then Return _rVal
            If Not bGridPts Then
                If gProps.HatchStyle = dxxHatchStyle.dxfHatchSolidFill Then
                    'just use the exterior paths for a fill region
                    aLoop = bndLoops.ToPath(gProps.Plane)
                    _rVal.Filled = True
                    _rVal.SetLoop(iLp, aLoop)

                    Return _rVal
                End If
            End If
            _rVal.SetLoop(iLp, aLoop)
            'get the patther which describes the hatch line family
            Dim aPat As THATCHPATTERN = gProps.HATCHPAT
            lcnt = aPat.HatchLineCnt
            'no hatch lines no hatch!
            If lcnt <= 0 Then Return _rVal
            aPlane = gProps.Plane
            aOCS = TPLANE.ArbitraryCS(aPlane.ZDirection, True)
            ang = TVALUES.NormAng(gProps.Rotation, False, True, True)
            Do While ang >= 180
                ang -= 180
            Loop
            If ang <> 0 Then
                'apply the big rotation for the whole hatch to the plane
                aPlane.Revolve(ang)
            End If
            bndRec = bndLoops.ExtentPts.Bounds(aPlane)
            If Not (bndRec.Width > 0 And bndRec.Height > 0) Then Return _rVal
            bndRec.Stretch(0.1, True, True, bMaintainOrigin:=True)
            bndCrns = bndRec.Corners(True, False)
            'get the diagonal length of the rectangle which we use for our initial hatch lines
            diag = Math.Sqrt(bndRec.Height ^ 2 + bndRec.Width ^ 2) / 2
            'initialize variables
            pathVecs = New TVECTORS
            If Not bGridPts Then
                If gProps.HatchStyle = dxxHatchStyle.dxfHatchPreDefined Then
                    SF = gProps.ScaleFactor
                    If SF <= 0 Then SF = 1
                Else
                    SF = 1
                End If
            End If
            'get the origin of the hatch pattern
            If bGridPts Then
                If aOrigin IsNot Nothing Then
                    v1 = aOrigin.Strukture
                    v1.ProjectTo(bndRec)
                    v1 = v1.WithRespectTo(bndRec)
                    org = bndRec.Vector(v1.X + gProps.OffsetX, v1.Y + gProps.OffsetY)
                Else
                    org = bndRec.Vector(gProps.OffsetX, gProps.OffsetY)
                End If
            Else
                org = New TVECTOR
                org.ProjectTo(aPlane)
            End If
            aOCS.Origin = org
            lorg = org
            aPlane.Origin = org
            gProps.LINEORIGIN = org
            'loop on the hatch line definitions
            For li = 1 To lcnt
                'get the hatch line definition
                Dim aPatL As THATCHLINE = aPat.HatchLines(li - 1)
                yStep = Math.Abs(aPatL.DeltaY * SF)
                patLength = 0
                If aPatL.DashCount > 0 Then
                    For j = 0 To aPatL.DashCount - 1
                        patLength += Math.Abs(aPatL.Dashes(j))
                    Next j
                End If
                patLength *= SF
                If yStep > 0 Then 'there must be a step or we will loop forever!
                    bPlane = aOCS
                    'set the X and Y directions
                    xDir = aPlane.XDirection
                    yDir = aPlane.YDirection
                    ang = aPatL.Angle
                    If ang <> 0 Then
                        'apply the big rotation for the whole hatch to the plane
                        xDir.RotateAbout(aPlane.ZDirection, ang)
                        yDir.RotateAbout(aPlane.ZDirection, ang)
                    End If
                    'create the hatch line origin
                    lorg = aOCS.Origin
                    If aPatL.OriginX <> 0 Or aPatL.OriginY <> 0 Then
                        If aPatL.OriginX <> 0 Then
                            lorg += aOCS.XDirection * (aPatL.OriginX * SF)
                        End If
                        If aPatL.OriginY <> 0 Then
                            lorg += aOCS.YDirection * (aPatL.OriginY * SF)
                        End If
                    End If
                    aOCS.Origin = lorg
                    'place the first line outside the rectangle
                    yLine.SPT = lorg
                    yLine.EPT = yLine.SPT + yDir
                    ips = dxfProjections.ToLine(bndCrns, yLine)
                    sp = ips.Farthest(lorg, yDir, dxxLineDescripts.Normal)
                    ep = ips.Farthest(sp, yDir, dxxLineDescripts.Normal)
                    aDir = lorg.DirectionTo(sp, False, d1)
                    sp = lorg + aDir * ((Fix(d1 / yStep) + 1) * yStep)
                    stepDir = sp.DirectionTo(ep, False, d1)
                    stepLim = (Fix(d1 / yStep) + 1) * yStep
                    ep = sp + stepDir * stepLim
                    yDir = (stepDir * -1)
                    xDir = yDir
                    xDir.RotateAbout(aPlane.ZDirection, -90)
                    yLine.SPT = lorg
                    yLine.EPT = yLine.SPT + yDir * 1
                    iLine.SPT = sp
                    iLine.EPT = iLine.SPT + xDir * 1
                    'march in the step direction for the distance of the diameter plus a little
                    d2 = 0
                    Do Until d2 > stepLim
                        'get the intersections of the long line with the hatch edges
                        ips = iLine.IntersectionPts(bndLoops, True)
                        If ips.Count > 1 Then
                            v1 = ips.Item(1) + xDir * (-2 * diag)
                            ips.SortNearestToFarthest(v1, False, True, 3)
                            'vecs_AddArrow pathVecs, ips.Members(0), xDir, yDir, 0.05
                        End If
                        If ips.Count > 1 Then
                            If patLength > 0 Then
                                hLine.SPT = ips.Item(1)
                                hLine.EPT = ips.Last
                                vSort = dxfProjections.ToLine(lorg, hLine, d1)
                                aDir = vSort.DirectionTo(hLine.SPT, False, aFlg, d1)
                                If Not aFlg Then
                                    If Not aDir.Equals(xDir, 3) Then
                                        If patLength > 0 Then
                                            d1 = (Fix(d1 / patLength) + 1) * patLength
                                        End If
                                        vSort += aDir * d1
                                    End If
                                End If
                                'vecs_AddArrow pathVecs, vSort, xDir, yDir, 0.15
                            End If
                            'leap frog through the intersection points making lines
                            For j = 1 To ips.Count - 1
                                'define the current hatch line
                                hLine.EPT = ips.Item(j + 1)
                                hLine.SPT = ips.Item(j)
                                If aPatL.DashCount = 0 Or patLength = 0 Then
                                    'no dashes so save the hatch line and the path vertices
                                    pathVecs.AddLine(hLine.SPT, hLine.EPT)
                                Else
                                    'create smaller lines by applying the dashes to the hatch line
                                    vDashPts = dxfHatches.CreateDashPts(patLength, bGridPts, xDir, stepDir, hLine, vSort, aPatL, SF)
                                    'append the dashes to hatch lines collections and save the path vertices
                                    For k = 0 To vDashPts.Count - 1
                                        v1 = vDashPts.Item(k + 1)
                                        If v1.Code = dxxVertexStyles.PIXEL Then
                                            'save the grid points if this is a grid points hatch
                                            pathVecs.Add(vDashPts.Item(k + 1), TVALUES.ToByte(dxxVertexStyles.PIXEL))
                                        Else
                                            'every other point defines the end of a line
                                            If v1.Code = dxxVertexStyles.LINETO And k - 1 >= 0 Then
                                                pathVecs.AddLine(vDashPts.Item(k), v1)
                                            End If
                                        End If
                                    Next k
                                End If
                                j += 1
                            Next j
                        End If
                        'take a step
                        If aPatL.DeltaX <> 0 Then
                            lorg += xDir * (aPatL.DeltaX * SF)
                        End If
                        iLine.Translate(stepDir * yStep)
                        d2 += yStep
                    Loop
                End If
            Next li
            '
            'For i = 1 To bndLoops.Count
            '    For j = 1 To bndLoops.Members(i - 1).Segments.Count
            '        vecs_AddLine pathVecs, bndLoops.Members(i - 1).Segments.Members(j - 1).LineStructure.SPT, bndLoops.Members(i - 1).Segments.Members(j - 1).LineStructure.EPT
            '    Next j
            'Next i
            '
            ' pathVecs.Add(bndRec.Point( TopLeft), MOVETO)
            ' pathVecs.Add(bndRec.Point( BottomLeft), LINETO)
            ' pathVecs.Add(bndRec.Point( BottomRight), LINETO)
            ' pathVecs.Add(bndRec.Point( TopRight), LINETO)
            ' pathVecs.Add(bndRec.Point( TopLeft), LINETO)
            _rVal.SetLoop(iLp, pathVecs)
            _rVal.GraphicType = dxxGraphicTypes.Hatch
            Return _rVal
        End Function
        Friend Shared Function PatternFromHatch(aProps As TPROPERTIES) As THATCHPATTERN
            Dim _rVal As New THATCHPATTERN
            Dim aSty As dxxHatchStyle = aProps.ValueI("*Style")
            If aSty = dxxHatchStyle.dxfHatchPreDefined Then

                Dim idx As Integer
                Dim aPat As THATCHPATTERN = dxfGlobals.goHatchPatterns.GetByName(aProps.GCValueStr(2), idx)
                If idx >= 0 Then
                    _rVal = aPat
                    Return _rVal
                Else
                    aSty = dxxHatchStyle.dxfHatchUserDefined
                End If
            End If
            Select Case aSty
                Case dxxHatchStyle.dxfHatchSolidFill
                    _rVal.Name = "SOLID"
                    _rVal.Description = "Solid fill"
                    _rVal.HatchLineCnt = 1
                    ReDim _rVal.HatchLines(0)
                    _rVal.HatchLines(0).Angle = 45
                    _rVal.HatchLines(0).DeltaY = 0.125
                Case dxxHatchStyle.dxfHatchUserDefined
                    _rVal.Name = "_USER"
                    _rVal.Description = "User-defined"
                    If aProps.GCValueB(77) Then 'doubled
                        _rVal.HatchLineCnt = 2
                        ReDim _rVal.HatchLines(0 To 1)
                        _rVal.HatchLines(1).Angle = 90
                        _rVal.HatchLines(0).DeltaY = aProps.GCValueD(41)  'patthern scale
                        _rVal.HatchLines(1).DeltaY = _rVal.HatchLines(0).DeltaY
                    Else
                        _rVal.HatchLineCnt = 1
                        ReDim _rVal.HatchLines(0)
                        _rVal.HatchLines(0).DeltaY = aProps.GCValueD(41)  'patthern scale
                    End If
            End Select
            Return _rVal
        End Function

#End Region 'Methods
    End Class 'dxfHatches
End Namespace
