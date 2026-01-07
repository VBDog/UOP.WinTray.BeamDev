
Imports System.Runtime.Remoting
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfSegments

        Friend Shared Function SegmentsToList(aSegments As TSEGMENTS, ByRef ioLayer As String, ioColor As dxxColors, ByRef ioLType As String, ByRef ioLTScale As Double, aLinWeight As dxxLineWeights, ByRef rMarkers As List(Of String)) As List(Of TSEGMENT)
            rMarkers = New List(Of String)
            If String.IsNullOrWhiteSpace(ioLayer) Then ioLayer = "0" Else ioLayer = ioLayer.Trim()
            If String.IsNullOrWhiteSpace(ioLType) Then ioLType = dxfLinetypes.ByLayer Else ioLType = ioLType.Trim()
            If ioLTScale <= 0 Then ioLTScale = 1
            If ioColor = dxxColors.Undefined Then ioColor = dxxColors.ByLayer

            Dim _rVal As New List(Of TSEGMENT)
            If aSegments.Count <= 0 Then Return _rVal


            For i As Integer = 1 To aSegments.Count
                Dim mem As TSEGMENT = aSegments.Item(i)
                If String.IsNullOrWhiteSpace(mem.LayerName) Then mem.LayerName = ioLayer Else mem.LayerName = mem.LayerName.Trim()
                If String.IsNullOrWhiteSpace(mem.Linetype) Then mem.Linetype = ioLType Else mem.Linetype = mem.Linetype.Trim()
                If mem.Color = dxxColors.Undefined Then mem.Color = ioColor
                mem.LTScale = ioLTScale
                mem.LineWeight = aLinWeight
                mem.Mark = $"{mem.LayerName},{ mem.Color},{mem.Linetype}"

                If rMarkers.IndexOf(mem.Mark) < 0 Then
                    rMarkers.Add(mem.Mark)
                End If
                mem.Marker = False
                aSegments.SetItem(i, mem)
                _rVal.Add(mem)
            Next
            Return _rVal
        End Function

        Friend Shared Function SegmentsToList(aSegments As List(Of iPolylineSegment), ByRef ioLayer As String, ByRef ioColor As dxxColors, ByRef ioLType As String, ByRef ioLTScale As Double, aLinWeight As dxxLineWeights, ByRef rMarkers As List(Of String)) As List(Of TSEGMENT)
            rMarkers = New List(Of String)
            If String.IsNullOrWhiteSpace(ioLayer) Then ioLayer = "0" Else ioLayer = ioLayer.Trim()
            If String.IsNullOrWhiteSpace(ioLType) Then ioLType = dxfLinetypes.ByLayer Else ioLType = ioLType.Trim()
            If ioLTScale <= 0 Then ioLTScale = 1
            If ioColor = dxxColors.Undefined Then ioColor = dxxColors.ByLayer

            Dim _rVal As New List(Of TSEGMENT)
            If aSegments Is Nothing Then Return _rVal
            If aSegments.Count <= 0 Then Return _rVal


            For i As Integer = 1 To aSegments.Count
                Dim mem As TSEGMENT = New TSEGMENT(aSegments(i - 1))
                If String.IsNullOrWhiteSpace(mem.LayerName) Then mem.LayerName = ioLayer Else mem.LayerName = mem.LayerName.Trim()
                If String.IsNullOrWhiteSpace(mem.Linetype) Then mem.Linetype = ioLType Else mem.Linetype = mem.Linetype.Trim()
                If mem.Color = dxxColors.Undefined Then mem.Color = ioColor
                mem.LTScale = ioLTScale
                mem.LineWeight = aLinWeight
                mem.Mark = $"{mem.LayerName},{ mem.Color},{mem.Linetype}"

                If rMarkers.IndexOf(mem.Mark) < 0 Then
                    rMarkers.Add(mem.Mark)
                End If
                mem.Marker = False

                _rVal.Add(mem)
            Next
            Return _rVal
        End Function

        Public Shared Function SegmentsToPolylines(aSegments As List(Of iPolylineSegment), aPlane As dxfPlane, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLTScale As Double = 1,
                                    Optional aLinWeight As dxxLineWeights = dxxLineWeights.Undefined, Optional aGroupName As String = "", Optional bSingleSegs As Boolean = False) As dxfEntities
            Dim _rVal As New dxfEntities()

            Dim sMarks As New List(Of String)

            Dim mysegs As List(Of TSEGMENT) = SegmentsToList(aSegments, aLayer, aColor, aLineType, aLTScale, aLinWeight, sMarks)

            If mysegs.Count <= 0 Then Return _rVal
            Dim aSeg As TSEGMENT
            Dim bSeg As TSEGMENT
            Dim aPl As dxfEntity
            Dim Grps As New List(Of List(Of TSEGMENT))
            Dim nSegs As New List(Of TSEGMENT)
            Dim sSegs As List(Of TSEGMENT)
            Dim sp1 As TVECTOR
            Dim ep1 As TVECTOR
            Dim plane As New TPLANE(aPlane)
            Dim grp As List(Of TSEGMENT)

            'get the groups with matching marks
            For Each mark As String In sMarks
                sSegs = mysegs.FindAll(Function(x) String.Compare(x.Mark, mark, True) = 0)
                If sSegs.Count <= 0 Then Continue For
                If Not bSingleSegs Then
                    Grps.Add(sSegs)
                Else
                    For Each seg As TSEGMENT In sSegs
                        Grps.Add(New List(Of TSEGMENT)({seg}))
                    Next
                End If
            Next

            'sort the groups end to start etc.
            For i As Integer = 1 To Grps.Count
                grp = Grps.Item(i - 1)
                If grp.Count = 1 Then
                    aPl = dxfSegments.SegmentsToPolylineEnt(grp, plane, aGroupName)
                    _rVal.Add(aPl)

                    Continue For
                End If

                nSegs.Clear()
                aSeg = grp(0)
                nSegs.Add(aSeg)
                grp.RemoveAt(0)
                aSeg.SegmentPts(sp1, ep1)

                Do While grp.Count > 0

                    Dim fnnd As Integer = grp.FindIndex(Function(x) x.StartPt.DistanceTo(ep1, 3) = 0)
                    Do While fnnd >= 0
                        bSeg = grp(fnnd)
                        ep1 = bSeg.EndPt
                        nSegs.Add(bSeg)
                        grp.RemoveAt(fnnd)
                        fnnd = grp.FindIndex(Function(x) x.StartPt.DistanceTo(ep1, 3) = 0)
                    Loop

                    fnnd = grp.FindIndex(Function(x) x.EndPt.DistanceTo(sp1, 3) = 0)
                    Do While fnnd >= 0
                        bSeg = grp(fnnd)
                        sp1 = bSeg.StartPt
                        nSegs.Insert(0, bSeg)
                        grp.RemoveAt(fnnd)
                        fnnd = grp.FindIndex(Function(x) x.EndPt.DistanceTo(sp1, 3) = 0)
                    Loop

                    If nSegs.Count > 0 Then
                        'If nSegs.Count > 1 Then

                        aPl = dxfSegments.SegmentsToPolylineEnt(nSegs, plane, aGroupName)
                        _rVal.Add(aPl)
                        'Else
                        '    Dim tseg As TENTITY = dxfSegments.SegmentsToPolyline(nSegs, aPlane, aGroupName)
                        '    _rVal.Add(tseg)
                        'End If
                    End If

                    If grp.Count > 0 Then
                        nSegs.Clear()
                        aSeg = grp(0)
                        nSegs.Add(aSeg)
                        grp.RemoveAt(0)
                        aSeg.SegmentPts(sp1, ep1)
                        If grp.Count = 0 Then
                            aPl = dxfSegments.SegmentsToPolylineEnt(nSegs, plane, aGroupName)
                            _rVal.Add(aPl)
                        End If
                    End If
                Loop
            Next i
            Return _rVal
        End Function


        Friend Shared Function SegmentsToPolylineEnts(aSegments As TSEGMENTS, aPlane As TPLANE, aLayer As String, aColor As dxxColors, aLineType As String, aLTScale As Double,
                                    aLinWeight As dxxLineWeights, aGroupName As String, Optional aOwnerGUID As String = "", Optional aImageGUID As String = "",
                                    Optional bSingleSegs As Boolean = False) As dxfEntities
            Dim _rVal As New dxfEntities() With {.Owner = aOwnerGUID, .ImageGUID = aImageGUID}
            Dim sMarks As New List(Of String)

            Dim mysegs As List(Of TSEGMENT) = dxfSegments.SegmentsToList(aSegments, aLayer, aColor, aLineType, aLTScale, aLinWeight, sMarks)

            If mysegs.Count <= 0 Then Return _rVal
            Dim aSeg As TSEGMENT
            Dim bSeg As TSEGMENT
            Dim aPl As dxfEntity
            Dim Grps As New List(Of List(Of TSEGMENT))
            Dim nSegs As New List(Of TSEGMENT)
            Dim sSegs As List(Of TSEGMENT)
            Dim sp1 As TVECTOR
            Dim ep1 As TVECTOR

            Dim grp As List(Of TSEGMENT)

            'get the groups with matching marks
            For Each mark As String In sMarks
                sSegs = mysegs.FindAll(Function(x) String.Compare(x.Mark, mark, True) = 0)
                If sSegs.Count <= 0 Then Continue For
                If Not bSingleSegs Then
                    Grps.Add(sSegs)
                Else
                    For Each seg As TSEGMENT In sSegs
                        Grps.Add(New List(Of TSEGMENT)({seg}))
                    Next
                End If
            Next

            'sort the groups end to start etc.
            For i As Integer = 1 To Grps.Count
                grp = Grps.Item(i - 1)
                If grp.Count = 1 Then
                    aPl = dxfSegments.SegmentsToPolylineEnt(grp, aPlane, aGroupName)
                    _rVal.Add(aPl)

                    Continue For
                End If

                nSegs.Clear()
                aSeg = grp(0)
                nSegs.Add(aSeg)
                grp.RemoveAt(0)
                aSeg.SegmentPts(sp1, ep1)

                Do While grp.Count > 0

                    Dim fnnd As Integer = grp.FindIndex(Function(x) x.StartPt.DistanceTo(ep1, 3) = 0)
                    Do While fnnd >= 0
                        bSeg = grp(fnnd)
                        ep1 = bSeg.EndPt
                        nSegs.Add(bSeg)
                        grp.RemoveAt(fnnd)
                        fnnd = grp.FindIndex(Function(x) x.StartPt.DistanceTo(ep1, 3) = 0)
                    Loop

                    fnnd = grp.FindIndex(Function(x) x.EndPt.DistanceTo(sp1, 3) = 0)
                    Do While fnnd >= 0
                        bSeg = grp(fnnd)
                        sp1 = bSeg.StartPt
                        nSegs.Insert(0, bSeg)
                        grp.RemoveAt(fnnd)
                        fnnd = grp.FindIndex(Function(x) x.EndPt.DistanceTo(sp1, 3) = 0)
                    Loop

                    If nSegs.Count > 0 Then
                        'If nSegs.Count > 1 Then

                        aPl = dxfSegments.SegmentsToPolylineEnt(nSegs, aPlane, aGroupName)
                        _rVal.Add(aPl)
                        'Else
                        '    Dim tseg As TENTITY = dxfSegments.SegmentsToPolyline(nSegs, aPlane, aGroupName)
                        '    _rVal.Add(tseg)
                        'End If
                    End If

                    If grp.Count > 0 Then
                        nSegs.Clear()
                        aSeg = grp(0)
                        nSegs.Add(aSeg)
                        grp.RemoveAt(0)
                        aSeg.SegmentPts(sp1, ep1)
                        If grp.Count = 0 Then
                            aPl = dxfSegments.SegmentsToPolylineEnt(nSegs, aPlane, aGroupName)
                            _rVal.Add(aPl)
                        End If
                    End If
                Loop
            Next i
            Return _rVal
        End Function

        Friend Shared Function SegmentsToPolyline(aSegments As List(Of TSEGMENT), aPlane As TPLANE, Optional aGroupName As String = "") As TENTITY


            If aSegments Is Nothing Then Return New TENTITY(dxxGraphicTypes.Undefined)
            If aSegments.Count <= 0 Then Return New TENTITY(dxxGraphicTypes.Undefined)
            Dim aSeg As TSEGMENT = aSegments(0)
            Dim seg1 As TENTITY = aSeg.ToSubEnt(aGroupName)

            seg1.TagFlagValue = aSeg.TagFlagValue
            seg1.Props.SetVal("*Identifier", aSeg.Identifier)
            seg1.ImageGUID = aSeg.ImageGUID
            seg1.Props.CopyDisplayProperties(aSeg.DisplayStructure)
            seg1.DefPts.Plane = aPlane
            If aSegments.Count = 1 Then

                Return seg1
            End If
            Dim _rVal As New TENTITY(dxxGraphicTypes.Polyline)
            _rVal.Props.CopyCommonProps(seg1.Props)
            _rVal.Props.SetVal("*GroupName", aGroupName)
            _rVal.TagFlagValue = seg1.TagFlagValue
            _rVal.Props.SetVal("*Identifier", aSeg.Identifier)
            _rVal.ImageGUID = seg1.ImageGUID
            _rVal.Props.CopyDisplayProperties(aSeg.DisplayStructure)
            _rVal.DefPts.Plane = aPlane

            Dim verts As New TVERTICES(0)
            aSeg = aSegments(0)


            Dim sp As New TVERTEX(0)
            Dim ep As New TVERTEX(0)
            aSeg.SegmentVerts(sp, ep)


            verts.Add(sp)


            _rVal.DefPts.Verts = New TVERTICES(0)
            For i As Integer = 2 To aSegments.Count
                Dim bSeg As TSEGMENT = aSegments(i - 1)
                bSeg.SegmentVerts(sp, ep)
                verts.Add(sp)



                If i = aSegments.Count Then
                    If ep.DistanceTo(verts.Item(1), 3) = 0 Then
                        _rVal.Props.SetVal("*Closed", True)

                    Else


                        verts.Add(ep)

                    End If
                End If

            Next i


            _rVal.DefPts.Verts = verts
            Return _rVal
        End Function

        Friend Shared Function SegmentsToPolylineEnt(aSegments As List(Of TSEGMENT), aPlane As TPLANE, Optional aGroupName As String = "") As dxfEntity


            If aSegments Is Nothing Then Return Nothing
            If aSegments.Count <= 0 Then Return Nothing
            Dim aSeg As TSEGMENT = aSegments(0)
            Dim seg1 As dxfEntity = aSeg.ToSubEntity(aGroupName)
            If Not dxfPlane.IsNull(aPlane) Then seg1.DefPts.Plane = aPlane


            If aSegments.Count = 1 Then Return seg1

            Dim sp As New TVERTEX(0)
            Dim ep As New TVERTEX(0)
            Dim pline As New dxePolyline
            Dim dsp As TDISPLAYVARS = seg1.DisplayStructure


            Dim _rVal As New TENTITY(dxxGraphicTypes.Polyline)
            pline.DisplayStructure = seg1.DisplayStructure
            pline.GroupName = seg1.GroupName
            pline.TagFlagValue = seg1.TagFlagValue
            pline.Identifier = seg1.Identifier
            pline.ImageGUID = seg1.ImageGUID
            pline.DefPts.Plane = seg1.DefPts.Plane

            Dim verts As New colDXFVectors()
            aSeg.SegmentVerts(sp, ep)
            verts.Add(sp)


            _rVal.DefPts.Verts = New TVERTICES(0)
            For i As Integer = 2 To aSegments.Count
                Dim bSeg As TSEGMENT = aSegments(i - 1)
                bSeg.SegmentVerts(sp, ep)
                verts.Add(sp)



                If i = aSegments.Count Then
                    If ep.DistanceTo(verts.ItemVector(1), 3) = 0 Then
                        pline.Closed = True

                    Else


                        verts.Add(ep)

                    End If
                End If

            Next i


            pline.DefPts.Vertices = verts
            Return pline
        End Function

        Friend Shared Sub MiterSegments(aSegments As List(Of TSEGMENT), aPlane As TPLANE, Optional bIgnoreSuppressed As Boolean = False)
            If aSegments Is Nothing Then Return
            If aSegments.Count <= 1 Then Return
            'On Error Resume Next
            Dim aSeg As TSEGMENT
            Dim bSeg As TSEGMENT
            Dim aArc As New TARC("")
            Dim bArc As New TARC("")
            Dim aLine As New TSEGMENT()
            Dim bLine As New TSEGMENT
            Dim aSegs As TSEGMENTS
            Dim bSegs As TSEGMENTS
            Dim lN1 As TSEGMENT
            Dim lN2 As TSEGMENT
            Dim eEdge As TSEGMENT
            Dim sEdge As TSEGMENT
            Dim ang As Double
            Dim j As Integer
            Dim bDoIt As Boolean
            Dim asw As Double
            Dim bew As Double
            Dim sp1 As TVECTOR
            Dim ep2 As TVECTOR


            For i As Integer = 1 To aSegments.Count
                aSeg = aSegments(i - 1)
                bDoIt = True
                If i > 1 Then
                    j = i - 2
                Else
                    j = aSegments.Count - 1
                End If
                bSeg = aSegments(j)
                If Not bIgnoreSuppressed Then
                    If aSeg.DisplayStructure.Suppressed Or bSeg.DisplayStructure.Suppressed Then bDoIt = False
                End If
                If aSeg.IsArc Then
                    aArc = aSeg.ArcStructure
                    asw = aArc.StartWidth
                    sp1 = aArc.StartPt
                Else

                    asw = aSeg.LineStructure.StartWidth
                    sp1 = aSeg.LineStructure.SPT
                End If
                If bSeg.IsArc Then
                    bArc = bSeg.ArcStructure
                    bew = bArc.EndWidth
                    ep2 = bArc.EndPt
                Else

                    bew = bSeg.LineStructure.EndWidth
                    ep2 = bSeg.LineStructure.EPT
                End If
                If asw = 0 Or bew = 0 Then bDoIt = False
                If bDoIt Then
                    If sp1.DistanceTo(ep2, 3) > 0.001 Then bDoIt = False
                End If
                If bDoIt Then
                    If aSeg.IsArc Then
                        aLine.LineStructure = dxfUtils.ArcTangentLine(aArc, aArc.StartAngle, 5)
                        aLine.LineStructure.StartWidth = aArc.StartWidth
                        aLine.LineStructure.EndWidth = aArc.EndWidth
                    Else
                        aLine = aSeg
                    End If
                    If bSeg.IsArc Then
                        bLine.LineStructure = dxfUtils.ArcTangentLine(bArc, bArc.EndAngle, 5)
                        bLine.LineStructure.StartWidth = bArc.StartWidth
                        bLine.LineStructure.EndWidth = bArc.EndWidth
                    Else
                        bLine = bSeg
                    End If
                    If aLine.LineStructure.SPT.DirectionTo(aLine.LineStructure.EPT).Equals(bLine.LineStructure.SPT.DirectionTo(bLine.LineStructure.EPT), True, 4) Then
                        bDoIt = False
                    End If
                End If
                If bDoIt Then
                    aSegs = aLine.SegmentBounds(aPlane, False)
                    bSegs = bLine.SegmentBounds(aPlane, False)
                    lN1 = aSegs.GetByIdentifier("EDGE1").Item(1)
                    lN2 = bSegs.GetByIdentifier("EDGE1").Item(1)
                    eEdge = aSegs.GetByIdentifier("STARTWIDTH").Item(1)
                    sEdge.LineStructure.SPT = lN1.LineStructure.IntersectionPt(lN2.LineStructure)
                    lN1 = aSegs.GetByIdentifier("EDGE2").Item(1)
                    lN2 = bSegs.GetByIdentifier("EDGE2").Item(1)
                    sEdge.LineStructure.EPT = lN1.LineStructure.IntersectionPt(lN2.LineStructure)
                    ang = eEdge.LineStructure.SPT.DirectionTo(eEdge.LineStructure.EPT).AngleTo(sEdge.LineStructure.SPT.DirectionTo(sEdge.LineStructure.EPT), aPlane.ZDirection)
                    aSeg.StartEdgeAngle = ang
                    eEdge = bSegs.GetByIdentifier("ENDWIDTH").Item(1)
                    ang = eEdge.LineStructure.SPT.DirectionTo(eEdge.LineStructure.EPT).AngleTo(sEdge.LineStructure.SPT.DirectionTo(sEdge.LineStructure.EPT), aPlane.ZDirection)
                    bSeg.EndEdgeAngle = ang
                    aSegments(i - 1) = aSeg
                    aSegments(j) = bSeg
                End If
            Next i
        End Sub
        Friend Shared Function PolylineSegments(aVertices As TVERTICES, aPlane As TPLANE, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTScale As Double = 1, Optional sGlobalWidth As Double = -1, Optional aIdentifier As String = "", Optional aDefSettings As dxfDisplaySettings = Nothing) As List(Of TSEGMENT)
            Dim rHasWidth As Boolean = False
            Return PolylineSegments(aVertices, rHasWidth, aPlane, bClosed, aDisplaySettings, aLTScale, sGlobalWidth, aIdentifier, aDefSettings)
        End Function
        Friend Shared Function PolylineSegments(aVertices As TVERTICES, ByRef rHasWidth As Boolean, aPlane As TPLANE, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTScale As Double = 1, Optional sGlobalWidth As Double = -1, Optional aIdentifier As String = "", Optional aDefSettings As dxfDisplaySettings = Nothing) As List(Of TSEGMENT)
            Dim rSegments As New List(Of TSEGMENT)()
            rHasWidth = False
            If aVertices.Count <= 2 Then Return rSegments
            '^returns the collection of lines and arc defined by the vectors in the collection
            '~the segments returned are defined by the vectors in the collection in logical order (1 to count).
            '~the type of segment returned is based on the vertex properties of the of the vectors.
            Try

                Dim pVars As TVERTEXVARS
                Dim aEntSet As New TDISPLAYVARS("0")
                Dim aFlg As Boolean
                Dim bAddLine As Boolean
                Dim bSingleSets As Boolean
                Dim defClr As dxxColors
                Dim defLayer As String = String.Empty
                Dim defLType As String = String.Empty
                Dim defLWT As dxxLineWeights
                Dim sw As Double
                Dim ew As Double
                defClr = dxxColors.ByLayer
                If aDefSettings IsNot Nothing Then
                    defClr = aDefSettings.Color
                    defLayer = aDefSettings.LayerName
                    defLType = aDefSettings.Linetype '  dxfLinetypes.ByLayer
                    defLWT = aDefSettings.LineWeight
                Else
                    defLWT = dxxLineWeights.ByLayer
                End If
                If aLTScale <= 0 Then aLTScale = 1
                If aDisplaySettings IsNot Nothing Then
                    aEntSet = aDisplaySettings.Strukture
                    bSingleSets = True
                End If
                For i As Integer = 1 To aVertices.Count
                    Dim v1 As TVERTEX = aVertices.Item(i)
                    Dim v2 As TVERTEX = IIf(i < aVertices.Count, aVertices.Item(i + 1), aVertices.Item(1))
                    pVars = v1.Vars
                    pVars.StartWidth = Math.Round(pVars.StartWidth, 8)
                    pVars.EndWidth = Math.Round(pVars.EndWidth, 8)
                    If pVars.Color = dxxColors.ByLayer Then pVars.Color = dxxColors.ByLayer

                    If i = aVertices.Count Then
                        If Not bClosed Then Exit For
                        If rSegments.Count < 1 Then Exit For
                    End If
                    If v1.Vector.DistanceTo(v2.Vector) < 0.0005 Then Continue For

                    If Not bSingleSets Then
                        aEntSet.Color = pVars.Color
                        aEntSet.Linetype = pVars.Linetype
                        aEntSet.LayerName = pVars.LayerName
                        aEntSet.LTScale = pVars.LTScale
                        aEntSet.Suppressed = pVars.Suppressed
                        aEntSet.LineWeight = dxxLineWeights.ByLayer
                        If aEntSet.Linetype = "" Or String.Compare(aEntSet.Linetype, dxfLinetypes.ByBlock) = 0 Then aEntSet.Linetype = defLType
                        If aEntSet.LayerName = "" Then aEntSet.LayerName = defLayer
                        If aEntSet.Color = dxxColors.Undefined Or aEntSet.Color = dxxColors.ByBlock Then aEntSet.Color = defClr
                    End If
                    If sGlobalWidth > 0 Then
                        sw = sGlobalWidth
                        ew = sGlobalWidth
                    Else
                        sw = pVars.StartWidth
                        ew = pVars.EndWidth
                        If sw < 0 Then sw = 0
                        If ew < 0 Then ew = 0
                    End If
                    bAddLine = Math.Round(pVars.Radius, 8) = 0
                    Dim aSeg As New TSEGMENT("")

                    If Not bAddLine Then
                        If pVars.Radius < 0 Then
                            pVars.Radius = Math.Abs(pVars.Radius)
                            pVars.Inverted = True
                        End If

                        If dxfProjections.DistanceTo(v1.Vector, v2.Vector) <= 2 * pVars.Radius Then
                            aSeg = dxfPrimatives.ArcBetweenPointsV(pVars.Radius, v1.Vector, v2.Vector, aPlane, pVars.Inverted, False, True, aFlg)
                            If Not aFlg Then
                                bAddLine = True
                            End If
                        Else
                            bAddLine = True
                        End If
                    End If
                    'If aSeg.Radius = 0.125 And pVars.Inverted Then
                    '    Console.Beep()
                    'End If
                    aSeg.Identifier = aIdentifier
                    aSeg.DisplayStructure = aEntSet
                    aSeg.LineWeight = defLWT
                    aSeg.LTScale = aLTScale
                    aSeg.StartWidth = sw
                    aSeg.EndWidth = ew
                    aSeg.Suppressed = String.Compare(aSeg.Linetype, dxfLinetypes.Invisible, ignoreCase:=True) = 0
                    If Not bAddLine Then
                        If aSeg.ArcStructure.ClockWise Then
                            'aSeg.ArcStructure = aSeg.ArcStructure.Inverse
                        End If
                    End If
                    If Not bAddLine Then
                        pVars.Bulge = aSeg.Bulge
                    Else
                        pVars.Bulge = 0
                    End If
                    aSeg.Tag = pVars.Tag
                    aSeg.Flag = pVars.Flag
                    aSeg.Value = pVars.Value
                    If sw > 0 Or ew > 0 Then rHasWidth = True
                    '=================================================================================
                    If bAddLine Then
                        '=================================================================================
                        aSeg.IsArc = False
                        aSeg.LineStructure = New TLINE(v1, v2)
                    End If
                    rSegments.Add(aSeg)
                    v1.Vars = pVars
                    aVertices.SetItem(i, v1)

                Next i
                'set the miter angles
                If rSegments.Count > 1 And rHasWidth Then
                    MiterSegments(rSegments, aPlane, False)
                End If
                Return rSegments
            Catch ex As Exception
                If dxfUtils.RunningInIDE Then MessageBox.Show(ex.Message)
                Return rSegments
            End Try
        End Function


        Public Shared Function PolylineSegments(aVertices As IEnumerable(Of dxfVector), Optional aPlane As dxfPlane = Nothing, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTScale As Double = 1, Optional sGlobalWidth As Double = -1, Optional aIdentifier As String = "", Optional aDefSettings As dxfDisplaySettings = Nothing) As List(Of iPolylineSegment)
            Dim rHasWidth As Boolean = False
            Return PolylineSegments(aVertices, rHasWidth, aPlane, bClosed, aDisplaySettings, aLTScale, sGlobalWidth, aIdentifier, aDefSettings)
        End Function


        Public Shared Function PolylineSegments(aVertices As IEnumerable(Of dxfVector), ByRef rHasWidth As Boolean, Optional aPlane As dxfPlane = Nothing, Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTScale As Double = 1, Optional sGlobalWidth As Double = -1, Optional aIdentifier As String = "", Optional aDefSettings As dxfDisplaySettings = Nothing) As List(Of iPolylineSegment)
            If aVertices Is Nothing Then Return New List(Of iPolylineSegment)

            Dim aSegments As List(Of TSEGMENT) = dxfSegments.PolylineSegments(New TVERTICES(aVertices), New TPLANE(aPlane), bClosed, aDisplaySettings, aLTScale, sGlobalWidth, aIdentifier, aDefSettings)

            Dim _rVal As New List(Of iPolylineSegment)

            For i As Integer = 1 To aSegments.Count
                Dim seg As TSEGMENT = aSegments(i - 1)
                If seg.IsArc Then
                    _rVal.Add(New dxeArc(seg))
                Else
                    _rVal.Add(New dxeLine(seg))
                End If

            Next


            Return _rVal


        End Function


        Public Shared Function SegmentsByTag(aVertices As IEnumerable(Of iVector), aTagVal As String, RemoveVerts As Boolean, Optional aLineType As String = "", Optional aRelationship As dxxSegmentTypes = dxxSegmentTypes.Line, Optional aRadius As Double = 0.0, Optional aColor As dxxColors = dxxColors.Undefined, Optional bInverted As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "", Optional aLayerName As String = "") As colDXFEntities
            Dim _rVal As New colDXFEntities
            If aRelationship <> dxxSegmentTypes.Arc And aRelationship <> dxxSegmentTypes.Line Then aRelationship = dxxSegmentTypes.Line
            '^used to add an additional segmenst based on two or more of the polygons existing boundiong vertices
            Dim inverts As New colDXFVectors(aVertices)
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            Dim verts As colDXFVectors

            verts = inverts.GetByTag(aTagVal, bRemove:=RemoveVerts)
            If verts.Count <= 1 Then Return _rVal
            aLineType = IIf(String.IsNullOrWhiteSpace(aLineType), String.Empty, aLineType.Trim())
            aLayerName = IIf(String.IsNullOrWhiteSpace(aLayerName), String.Empty, aLayerName.Trim())
            If aColor = dxxColors.Undefined Then aColor = dxxColors.ByBlock
            For i As Integer = 1 To verts.Count Step 2
                If i + 1 > verts.Count Then Exit For
                v1 = verts.Item(i)
                v2 = verts.Item(i + 1)
                If aRelationship = dxxSegmentTypes.Arc Then
                    _rVal.AddArcPointToPoint(v1, v2, aRadius, aTag, aFlag, bInverted, aLineType, aColor, aLayerName)
                Else
                    _rVal.AddLine(v1, v2, New dxfDisplaySettings(aLayerName, aColor, aLineType), aTag, aFlag)
                End If
            Next i
            Return _rVal
        End Function
    End Class

End Namespace
