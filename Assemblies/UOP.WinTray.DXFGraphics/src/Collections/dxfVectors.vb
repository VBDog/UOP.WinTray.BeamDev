Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfVectors

#Region "Shared Methods"

        ''' <summary>
        ''' returns the 2D area summation of all the vectors in the collection
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aPlane">the working plane</param>
        ''' <returns></returns>
        Public Shared Function AreaSummation(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing) As Double
            Dim rPlanarVectors As colDXFVectors = Nothing
            Return AreaSummation(aVectors, aPlane, rPlanarVectors)
        End Function

        ''' <summary>
        ''' returns the 2D area summation of all the vectors in the collection
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aPlane">the working plane</param>
        ''' <param name="rPlanarVectors">returns the members defined with respect to the working plane</param>
        ''' <returns></returns>
        Public Shared Function AreaSummation(aVectors As IEnumerable(Of iVector), aPlane As dxfPlane, ByRef rPlanarVectors As colDXFVectors) As Double
            rPlanarVectors = colDXFVectors.Zero
            If aVectors Is Nothing Or aVectors.Count() <= 0 Then Return 0

            Dim sumation As Double
            Dim v2 As TVECTOR
            Dim aPln As New TPLANE(aPlane)
            Dim _rVal As Double = 0

            Dim vctrs As List(Of iVector) = aVectors.ToList()

            For i As Integer = 1 To vctrs.Count
                Dim v1 As New TVECTOR(vctrs(i - 1))
                If i < vctrs.Count Then v2 = New TVECTOR(vctrs(i)) Else v2 = New TVECTOR(vctrs(0))
                v1 = v1.WithRespectTo(aPln)
                v2 = v2.WithRespectTo(aPln)
                sumation = Math.Abs(v1.X * v2.Y - v2.X * v1.Y)
                _rVal += sumation
                rPlanarVectors.AddV(v1)
            Next i

            Return 0.5 * _rVal
        End Function

        ''' <summary>
        ''' computes the centroid of the points in the collection
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aPlane">the plane to use (world by default)</param>
        ''' <returns></returns>
        Public Shared Function Centroid(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing) As dxfVector
            Dim rArea As Double = 0
            Dim rPlanarVectors As colDXFVectors = Nothing
            Return Centroid(aVectors, aPlane, rArea, rPlanarVectors)
        End Function
        ''' <summary>
        ''' computes the centroid of the points in the collection
        ''' </summary>
        ''' <param name="aPlane">the plane to use (world by default)</param>
        ''' <param name="rArea">returns the area defined by the points</param>
        ''' <param name="rPlanarVectors">returns the members projected to the working plane</param>
        ''' <returns></returns>
        Public Shared Function Centroid(aVectors As IEnumerable(Of iVector), aPlane As dxfPlane, ByRef rArea As Double, ByRef rPlanarVectors As colDXFVectors) As dxfVector

            rArea = dxfVectors.AreaSummation(aVectors, aPlane, rPlanarVectors)


            If aVectors Is Nothing Or rPlanarVectors Is Nothing Then Return Nothing

            Dim vctrs As List(Of iVector) = aVectors.ToList()
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            For i As Integer = 1 To rPlanarVectors.Count
                Dim v1 As TVECTOR = rPlanarVectors.ItemVector(i)
                If i + 1 <= rPlanarVectors.Count Then v2 = rPlanarVectors.ItemVector(i + 1) Else v2 = rPlanarVectors.ItemVector(1)
                Dim sumation As Double = v1.X * v2.Y - v2.X * v1.Y
                v3.X += (v1.X + v2.X) * sumation
                v3.Y += (v1.Y + v2.Y) * sumation
            Next i
            If rArea <> 0 Then
                v3.X /= 6 * rArea
                v3.Y /= 6 * rArea
            End If
            v3 = New TPLANE(aPlane).Vector(v3.X, v3.Y)
            Return New dxfVector(v3)
        End Function

        Public Shared Function UniqueMembers(aVectors As IEnumerable(Of iVector), Optional aPrecis As Integer = 4) As List(Of iVector)
            Dim _rVal As New List(Of iVector)()
            If aVectors Is Nothing Then Return _rVal
            '^removes and returns the vectors that occur more than once

            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            For Each v As iVector In aVectors
                Dim v1 As New TVECTOR(v)
                Dim keep As Boolean = True
                For Each r As iVector In _rVal
                    Dim v2 As New TVECTOR(r)

                    If Math.Round(v1.DistanceTo(v2), aPrecis) <= 0.00000001 Then
                        keep = False
                        Exit For
                    End If
                Next
                If keep Then _rVal.Add(v)
            Next
            Return _rVal
        End Function
        Public Shared Function MatchPlanar(aVectors As IEnumerable(Of iVector), bVectors As IEnumerable(Of iVector), aPlane As dxfPlane, Optional aPrecis As Integer = 3) As Boolean
            '^returns true if there is a one for one equality of the members of A to B using the passed plane as reference.
            '~Is not dependant on order or elevation
            If aVectors Is Nothing Or bVectors Is Nothing Then Return False
            If aVectors.Count <> bVectors.Count Then Return False
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If dxfPlane.IsNull(aPlane) Then aPlane = dxfPlane.World
            Dim vbase As List(Of dxfVector) = dxfVectors.ProjectedToPlane(aVectors, aPlane)
            Dim vtest As List(Of dxfVector) = dxfVectors.ProjectedToPlane(bVectors, aPlane)
            Dim arc1 As dxeArc = dxfVectors.BoundingCircle(vbase, aPlane)
            Dim arc2 As dxeArc = dxfVectors.BoundingCircle(vtest, aPlane)
            If Math.Round(arc1.Radius, 2) <> Math.Round(arc2.Radius, 2) Then Return False


            Dim idx As Integer
            For Each v1 As dxfVector In vbase
                idx = vtest.FindIndex(Function(mem) Math.Round(mem.DistanceTo(arc2.Center), aPrecis) = Math.Round(v1.DistanceTo(arc1.Center), aPrecis))
                If idx < 0 Then
                    Return False
                Else
                    vtest.RemoveAt(idx)
                End If
            Next

            Return True
        End Function


        ''' <summary>
        ''' projects the vectors on to the passed plane
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aPlane">the plane to project to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aDir">an optional direction vector. if not passed, the Z direction of the plane is used</param>
        ''' <remarks></remarks>
        Public Shared Sub ProjectVectorsToPlane(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing, Optional aDir As dxfDirection = Nothing)
            ProjectVectorsToPlane(aVectors, New TPLANE(aPlane), aDir)
        End Sub


        ''' <summary>
        ''' returns a clone of the  vectors projected on to the passed plane
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aPlane">the plane to project to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aDir">an optional direction vector. if not passed, the Z direction of the plane is used</param>
        ''' <remarks></remarks>
        Friend Shared Sub ProjectVectorsToPlane(aVectors As IEnumerable(Of iVector), aPlane As TPLANE, Optional aDir As dxfDirection = Nothing)
            If aVectors Is Nothing Then Return
            Dim pdir As TVECTOR = aPlane.ZDirection
            If aDir IsNot Nothing Then
                pdir = New TVECTOR(aDir)
            End If

            For Each v In aVectors
                Dim v1 As New TVECTOR(v)
                If aDir Is Nothing Then
                    v1.ProjectTo(aPlane)
                Else
                    v1.ProjectTo(aPlane, pdir)
                End If

                v.X = v1.X
                v.Y = v1.Y
                v.Z = v1.Z
            Next

        End Sub

        ''' <summary>
        ''' transfers the vectors from one plane to another
        ''' </summary>
        ''' <remarks>
        '''  the coordinates of the vectors are transformed from the source plane to the target plane
        ''' </remarks>
        ''' <param name="aVectors">the subject vectors </param>
        ''' <param name="aFromPlane">the plane to transfer from. this is assumed to be the world XY plane if null is passed. </param>
        ''' <param name="aToPlane">the plane to transfer to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aXScale">an optional X scale factor</param>
        ''' <param name="aYScale">an optional Y scale factor</param>
        ''' <param name="aZScale">an optional Z scale factor</param>
        ''' <param name="aRotation">an optional rotation angle in degrees</param>
        ''' <param name="bKeepOrigin">if true, the origin of the new plane is kept at the origin of the old plane</param>
        ''' <returns></returns>
        Friend Shared Function TransferVectorsToPlane(aVectors As IEnumerable(Of iVector), Optional aFromPlane As TPLANE? = Nothing, Optional aToPlane As TPLANE? = Nothing, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aRotation As Double? = Nothing, Optional bKeepOrigin As Boolean = False) As Integer
            If aVectors Is Nothing Then Return 0
            Dim _rVal As Integer = 0
            Dim baseplane As TPLANE = TPLANE.World
            If (aFromPlane.HasValue) Then
                baseplane = aFromPlane.Value
            End If
            Dim newplane As TPLANE = TPLANE.World
            If (aToPlane.HasValue) Then
                newplane = aToPlane.Value
            End If
            If bKeepOrigin Then newplane.Origin = baseplane.Origin
            Dim rot As Double = 0
            If aRotation.HasValue Then
                rot = dxfUtils.NormalizeAngle(aRotation.Value, bThreeSixtyEqZero:=True)
            End If
            If rot <> 0 Then newplane.Revolve(rot)

            For Each v In aVectors
                Dim v1 As New TVECTOR(v)
                v1 = v1.WithRespectTo(baseplane)
                If aXScale.HasValue Then v1.X *= aXScale.Value
                If aYScale.HasValue Then v1.Y *= aYScale.Value
                If aZScale.HasValue Then v1.Z *= aZScale.Value
                v1 = newplane.Vector(v1.X, v1.Y, v1.Z)
                If (v1.X <> v.X Or v1.Y <> v.Y And v1.Z <> v.Z) Then _rVal += 1


                v.X = v1.X
                v.Y = v1.Y
                v.Z = v1.Z

            Next
            Return _rVal
        End Function


        Public Shared Function ProjectedToPlane(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing) As List(Of dxfVector)
            If aVectors Is Nothing Then Return New List(Of dxfVector)()
            Dim _rVal As New List(Of dxfVector)()
            Dim plane As New TPLANE(aPlane)
            For Each v In aVectors
                Dim v1 As New dxfVector(v)
                v1.ProjectTo(plane)
                _rVal.Add(v1)
            Next
            Return _rVal
        End Function


        ''' <summary>
        ''' returns the points that make up convex hull of the passed vectors with respect to the passed plane
        ''' </summary>
        '''<remarks>In geometry, a convex hull is the smallest convex shape that encloses a set of points, often visualized as the shape formed by a rubber band stretched around nails pounded into a plane</remarks>
        ''' <param name="aVectors">the subject vertices</param>
        ''' <param name="aPlane">the subject plane</param>
        ''' <param name="bOnBorder"></param>
        ''' <returns></returns>
        Public Shared Function ConvexHull(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing, Optional bOnBorder As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)()
            If aVectors Is Nothing Then Return _rVal
            If dxfPlane.IsNull(aPlane) Then aPlane = New dxfPlane()
            Dim aPln As New TPLANE(aPlane)
            Dim P1 As TVECTOR
            Dim Ps As TVECTOR
            Dim Pl As TVECTOR
            Dim v1 As TVECTOR
            Dim i As Integer
            Dim aVecs As New TVECTORS(dxfVectors.UniqueMembers(aVectors, 5))
            Dim bVecs As TVECTORS = TVECTORS.Zero
            Dim StartIndex As Integer
            Dim LastIndex As Integer
            Dim Selected As Integer
            Dim Cross As Double
            Dim Dot1 As Double
            Dim Dot2 As Double
            For i = 1 To aVecs.Count
                v1 = aVecs.Item(i)
                v1 = v1.WithRespectTo(aPln)
                v1.Z = 0
                aVecs.Update(i, v1, 0)
                '0 Code means the point has not yet been marked as in the hull
            Next i
            If aVecs.Count <= 3 Then
                bVecs = aVecs
            Else
                'get the left/top most point index
                aVecs.GetVector(dxxPointFilters.GetLeftTop, 0, Nothing, 3, rIndex:=StartIndex)
                LastIndex = StartIndex
                Do
                    Selected = 0
                    Pl = aVecs.Item(LastIndex)
                    For i = 1 To aVecs.Count
                        P1 = aVecs.Item(i)
                        If P1.Code = 0 And i <> LastIndex Then
                            If Selected = 0 Then
                                'no point has been selected yet so select this one
                                Selected = i
                            Else
                                Ps = aVecs.Item(Selected)
                                Cross = (Pl.X - P1.X) * (Ps.Y - P1.Y) - (Pl.Y - P1.Y) * (Ps.X - P1.X)  '= P1.CrossProduct(Ps)
                                '
                                If Cross = 0 Then
                                    'Since we want the points on the border, take the one closer to LastIndex
                                    Dot1 = (P1.X - Pl.X) * (P1.X - Pl.X) + (P1.Y - Pl.Y) * (P1.Y - Pl.Y) ' vecs_DotProduct(Pl, P1, P1) '
                                    Dot2 = (Ps.X - Pl.X) * (Ps.X - Pl.X) + (Ps.Y - Pl.Y) * (Ps.Y - Pl.Y) 'vecs_DotProduct(Pl, Ps, Ps) '
                                    '
                                    If bOnBorder Then
                                        If Dot1 < Dot2 Then
                                            Selected = i
                                        End If
                                        'Since we don't want the points on the border, take the one further from LastIndex
                                    Else
                                        If Dot1 > Dot2 Then
                                            Selected = i
                                        End If
                                    End If
                                ElseIf Cross < 0 Then
                                    'Ps is more counterclockwise
                                    Selected = i
                                End If
                            End If
                        End If
                    Next i
                    'set the last index to the last selected point
                    LastIndex = Selected
                    'mark the selected point as being in the hull
                    aVecs.SetCode(Selected, 1)
                    bVecs.Add(aVecs.Item(Selected))
                Loop While LastIndex <> StartIndex
            End If
            For i = 1 To bVecs.Count
                P1 = bVecs.Item(i)
                _rVal.Add(New dxfVector(aPln.Vector(P1.X, P1.Y)))
            Next i
            Return _rVal
        End Function

        ''' <summary>
        ''' returns a circle that encompasses all the member vectors projected to the working plane
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aPlane">the plane to use</param>
        ''' <returns></returns>

        Public Shared Function BoundingCircle(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing) As dxeArc
            Return New dxeArc(dxfVectors.BoundingArc(aVectors, aPlane))
        End Function

        ''' <summary>
        ''' returns a circle that encompasses all the member vectors projected to the working plane
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aPlane">the plane to use</param>
        ''' <returns></returns>
        Friend Shared Function BoundingArc(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing) As TARC
            Dim _rVal As New TARC("") With {.Radius = 0, .Plane = New TPLANE(aPlane)}
            If aVectors Is Nothing Then Return _rVal

            Dim cHull As List(Of dxfVector) = ConvexHull(aVectors, aPlane)
            If cHull.Count <= 0 Then Return _rVal
            If cHull.Count = 1 Then
                _rVal.Center = New TVECTOR(cHull(0))
                _rVal.Radius = 0
                Return _rVal
            End If
            Dim d1 As Double
            Dim dMax As Double
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            Dim aPln As New TPLANE(aPlane)
            Dim aAr As New TARC With {.Plane = aPln, .Radius = 0.0000001}
            Dim aArs(0 To 3) As TARC
            Dim ids() As Integer = {0, -1, -1, -1, -1}
            Dim Pts(0 To 4) As TVECTOR
            Dim aVecs As TVECTORS
            aAr.Plane.Origin = New TVECTOR(cHull(0))
            'first get the two points that are farthest apart
            For i As Integer = 0 To cHull.Count - 1
                v1 = cHull(i)
                For j As Integer = 0 To cHull.Count - 1
                    If i <> j Then
                        v2 = cHull(j)
                        d1 = dxfProjections.DistanceTo(v1, v2)
                        If d1 > dMax Then
                            ids(1) = i
                            ids(2) = j
                            dMax = d1
                        End If
                    End If
                Next j
            Next i
            Pts(1) = New TVECTOR(cHull(ids(1)))
            Pts(2) = New TVECTOR(cHull(ids(2)))
            aAr.Radius = Math.Round(Pts(1).DistanceTo(Pts(2)) / 2, 8)
            aAr.Plane.Origin = Pts(1).MidPt(Pts(2))
            If ids(1) <> ids(2) And aAr.Radius > 0 Then
                'see if all the points are in the circle
                dMax = aAr.Radius
                For i = 0 To cHull.Count - 1
                    If i <> ids(1) And i <> ids(2) Then
                        v1 = cHull(i)
                        d1 = aAr.Plane.Origin.DistanceTo(v1, 8)
                        If d1 > dMax Then
                            ids(3) = i
                            Pts(3) = New TVECTOR(v1)
                            dMax = d1
                            'keep the one that is the farthest out
                        End If
                    End If
                Next i
                'at least one is not in the circle
                If ids(3) >= 0 Then
                    'create a three point circle with the first two as the start pts and the new one
                    'as an pt on the circle between the original two
                    aAr = dxfPrimatives.ArcThreePointV(Pts(1), Pts(3), Pts(2), True, aAr.Plane, True)
                    dMax = Math.Round(aAr.Radius, 8)
                    'see if any are out of the new circle
                    For i = 0 To cHull.Count - 1
                        If i <> ids(1) And i <> ids(2) And i <> ids(3) Then
                            v1 = cHull(i)
                            d1 = aAr.Plane.Origin.DistanceTo(v1, 8)
                            If d1 > dMax Then
                                ids(4) = i
                                Pts(4) = New TVECTOR(v1)
                                dMax = d1
                                'keep the one that is the farthest out
                            End If
                        End If
                    Next i
                    If ids(4) >= 0 Then
                        'one of the three posible 3 pt circles using the 4 points is nthe solution
                        '(1,2,4) , (1,3,4) , (2,3,4)
                        aVecs = New TVECTORS(3)
                        'try 1,2,4
                        aVecs.SetItem(1, Pts(1))
                        aVecs.SetItem(2, Pts(2))
                        aVecs.SetItem(3, Pts(4))
                        aVecs.Clockwise(aAr.Plane, 0)
                        aArs(1) = dxfPrimatives.ArcThreePointV(aVecs.Item(1), aVecs.Item(2), aVecs.Item(3), True, aAr.Plane, True)
                        dMax = Math.Round(aArs(1).Radius, 8)
                        'aArs(1).Bulge = 0
                        'see if any are out of the new new circle
                        For i = 0 To cHull.Count - 1
                            If i <> ids(1) And i <> ids(2) And i <> ids(4) Then
                                v1 = cHull(i)
                                d1 = aArs(1).Plane.Origin.DistanceTo(v1, 8)
                                If d1 > dMax Then
                                    'aArs(1).Bulge = 1
                                    Exit For
                                End If
                            End If
                        Next i
                        aVecs.SetItem(1, Pts(1))
                        aVecs.SetItem(2, Pts(3))
                        aVecs.SetItem(3, Pts(4))
                        aVecs.Clockwise(aAr.Plane, 0)
                        aArs(2) = dxfPrimatives.ArcThreePointV(aVecs.Item(1), aVecs.Item(2), aVecs.Item(3), True, aAr.Plane, True)
                        dMax = Math.Round(aArs(2).Radius, 8)
                        'aArs(2).Bulge = 0
                        'see if any are out of the new new circle
                        For i = 0 To cHull.Count - 1
                            If i <> ids(1) And i <> ids(3) And i <> ids(4) Then
                                v1 = cHull(i)
                                d1 = aArs(2).Plane.Origin.DistanceTo(v1, 8)
                                If d1 > dMax Then
                                    'aArs(2).Bulge = 1
                                    Exit For
                                End If
                            End If
                        Next i
                        aVecs.SetItem(1, Pts(2))
                        aVecs.SetItem(2, Pts(3))
                        aVecs.SetItem(3, Pts(4))
                        aVecs.Clockwise(aAr.Plane, 0)
                        aArs(3) = dxfPrimatives.ArcThreePointV(aVecs.Item(1), aVecs.Item(2), aVecs.Item(3), True, aAr.Plane, True)
                        dMax = Math.Round(aArs(3).Radius, 8)
                        'aArs(3).Bulge = 0
                        'see if any are out of the new new circle
                        For i = 0 To cHull.Count - 1
                            If i <> ids(2) And i <> ids(3) And i <> ids(4) Then
                                v1 = cHull(i)
                                d1 = aArs(3).Plane.Origin.DistanceTo(v1, 8)
                                If d1 > dMax Then
                                    'aArs(3).Bulge = 1
                                    Exit For
                                End If
                            End If
                        Next i
                        'keep the smallest arc of the three possible that encloses all the points
                        Dim j As Integer = -1
                        For i = 1 To 3
                            If aArs(i).Bulge = 0 Then
                                If j = -1 Then
                                    j = i
                                Else
                                    If aArs(i).Radius < aArs(j).Radius Then j = i
                                End If
                            End If
                        Next i
                        If j > 0 Then
                            aAr = aArs(j)
                        End If
                    End If
                End If
            End If
            aAr.StartAngle = 0
            aAr.EndAngle = 360
            Return aAr
        End Function
        Public Shared Function TypeIsVector(aObject As Object) As Boolean
            If aObject Is Nothing Then Return False
            If TypeOf aObject Is TVECTOR Then
                Return True
            ElseIf TypeOf aObject Is TVERTEX Then
                Return True
            ElseIf TypeOf aObject Is TPOINT Then
                Return True
            ElseIf TypeOf aObject Is dxfVector Then
                Return True
            ElseIf TypeOf aObject Is iVector Then
                Return True
            Else
                Return False
            End If

        End Function

        Friend Shared Function GetVector(aObject As Object) As TVECTOR
            If aObject Is Nothing Then Return TVECTOR.Zero
            If TypeOf aObject Is TVECTOR Then
                Return DirectCast(aObject, TVECTOR)
            ElseIf TypeOf aObject Is TVERTEX Then
                Dim vrt = DirectCast(aObject, TVERTEX)
                Return vrt.Vector
            ElseIf TypeOf aObject Is TPOINT Then
                Dim vrt = DirectCast(aObject, TPOINT)
                Return New TVECTOR(vrt.X, vrt.Y)
            ElseIf TypeOf aObject Is dxfVector Then
                Dim vrt = DirectCast(aObject, dxfVector)
                Return New TVECTOR(vrt)
            ElseIf TypeOf aObject Is iVector Then
                Dim vrt = DirectCast(aObject, iVector)
                Return New TVECTOR(vrt)
            Else
                Return TVECTOR.Zero
            End If

        End Function

        ''' <summary>
        ''' returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag
        ''' </summary>
        ''' <param name="aVectors">the subject vector list to search</param>
        ''' <param name="aControlFlag">flag indicating what type of vector to search for</param>
        ''' <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        ''' <param name="aPlane">an optional coordinate system to use</param>
        ''' <param name="aPrecis">a precision for numerical comparison (0 to 15)</param>
        ''' <param name="bSuppressPlane">flag ignore the passed plane and compare the ordinates of the members without respect to the passed</param>
        ''' <param name="bResetMarks">flag to set all the member 'Mark' property to False</param>
        ''' <returns></returns>

        Friend Shared Function FindVertex(aVectors As IEnumerable(Of iVector), aControlFlag As dxxPointFilters, Optional aOrdinate As Double = 0, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bSuppressPlane As Boolean = False, Optional bResetMarks As Boolean = False) As TVERTEX

            Return New TVERTEX(FindVector(aVectors, aControlFlag, aOrdinate, aPlane, aPrecis, bSuppressPlane, bResetMarks))

        End Function
        ''' <summary>
        ''' returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag
        ''' </summary>
        ''' <param name="aVectors">the subject vector list to search</param>
        ''' <param name="aControlFlag">flag indicating what type of vector to search for</param>
        ''' <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        ''' <param name="aPlane">an optional coordinate system to use</param>
        ''' <param name="aPrecis">a precision for numerical comparison (0 to 15)</param>
        ''' <param name="bSuppressPlane">flag ignore the passed plane and compare the ordinates of the members without respect to the passed</param>
        ''' <param name="bResetMarks">flag to set all the member 'Mark' property to False</param>
        ''' <returns></returns>

        Friend Shared Function FindVertexVector(aVectors As IEnumerable(Of iVector), aControlFlag As dxxPointFilters, Optional aOrdinate As Double = 0, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bSuppressPlane As Boolean = False, Optional bResetMarks As Boolean = False) As TVECTOR

            Return New TVECTOR(FindVector(aVectors, aControlFlag, aOrdinate, aPlane, aPrecis, bSuppressPlane, bResetMarks))

        End Function

        ''' <summary>
        ''' returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag
        ''' </summary>
        ''' <param name="aVectors">the subject vector list to search</param>
        ''' <param name="aControlFlag">flag indicating what type of vector to search for</param>
        ''' <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        ''' <param name="aPlane">an optional coordinate system to use</param>
        ''' <param name="aPrecis">a precision for numerical comparison (0 to 15)</param>
        ''' <param name="bSuppressPlane">flag ignore the passed plane and compare the ordinates of the members without respect to the passed</param>
        ''' <param name="bResetMarks">flag to set all the member 'Mark' property to False</param>
        ''' <returns></returns>

        Public Shared Function FindVector(aVectors As IEnumerable(Of iVector), aControlFlag As dxxPointFilters, Optional aOrdinate As Double = 0, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bSuppressPlane As Boolean = False, Optional bResetMarks As Boolean = False) As iVector

            Dim rIndex As Integer = 0
            If aVectors Is Nothing Then Return Nothing
            If aVectors.Count <= 0 Then Return Nothing

            Dim aMem As dxfVector
            Dim iMem As iVector
            If dxfPlane.IsNull(aPlane) Then bSuppressPlane = True
            Dim workplane As New dxfPlane(aPlane)
            If aVectors.Count = 1 Then
                iMem = aVectors(0)
                If iMem IsNot Nothing And bResetMarks Then

                    If TypeOf iMem Is dxfVector Then
                        aMem = DirectCast(iMem, dxfVector)
                        aMem.Mark = False
                    End If
                End If
                rIndex = 1
            Else

                aPrecis = dxfUtils.LimitedValue(aPrecis, 0, 15)
                Dim aOrd As Double
                Dim valu As Double
                Dim comp As Double
                Dim d1 As Double
                Dim d2 As Double
                Dim v1 As TVECTOR
                Dim aOrdType As dxxOrdinateTypes
                Dim maxval As Double
                Dim minval As Double

                Dim aOrdDesc As dxxOrdinateDescriptors
                Dim bMin As Boolean
                '===================================================================
                If aControlFlag >= dxxPointFilters.AtMaxX And aControlFlag <= dxxPointFilters.AtMinZ Then
                    '===================================================================
                    'search for vectors at extremes
                    'returns the first one that satisfies
                    Select Case aControlFlag
                        Case dxxPointFilters.AtMaxX
                            aOrdType = dxxOrdinateTypes.MaxX
                            aControlFlag = dxxPointFilters.AtX
                        Case dxxPointFilters.AtMaxY
                            aOrdType = dxxOrdinateTypes.MaxY
                            aControlFlag = dxxPointFilters.AtY
                        Case dxxPointFilters.AtMaxZ
                            aOrdType = dxxOrdinateTypes.MaxZ
                            aControlFlag = dxxPointFilters.AtZ
                        Case dxxPointFilters.AtMinX
                            aOrdType = dxxOrdinateTypes.MinX
                            aControlFlag = dxxPointFilters.AtX
                        Case dxxPointFilters.AtMinY
                            aOrdType = dxxOrdinateTypes.MinY
                            aControlFlag = dxxPointFilters.AtY
                        Case dxxPointFilters.AtMinZ
                            aOrdType = dxxOrdinateTypes.MinZ
                            aControlFlag = dxxPointFilters.AtZ
                    End Select
                    aOrdinate = GetPlaneOrdinate(aVectors, aOrdType, workplane, bSuppressPlane)
                End If
                'search for vectors at nearest to the passed ordinate
                Select Case aControlFlag
             '===================================================================
                    Case dxxPointFilters.NearestToX, dxxPointFilters.FarthestFromX, dxxPointFilters.NearestToY, dxxPointFilters.FarthestFromY, dxxPointFilters.NearestToZ, dxxPointFilters.FarthestFromZ
                        '===================================================================
                        rIndex = 1
                        Dim id1 As Integer = 1
                        Dim ID2 As Integer = 1

                        iMem = aVectors.FirstOrDefault()
                        If bResetMarks And iMem IsNot Nothing Then
                            If TypeOf iMem Is dxfVector Then
                                aMem = DirectCast(iMem, dxfVector)
                                aMem.Mark = False
                            End If

                        End If


                        v1 = New TVECTOR(iMem)
                        If Not bSuppressPlane Then
                            v1 = v1.WithRespectTo(workplane, aPrecis:=-1)
                        End If
                        Select Case aControlFlag
                            Case dxxPointFilters.NearestToX, dxxPointFilters.FarthestFromX
                                d1 = Math.Abs(v1.X - aOrdinate)
                            Case dxxPointFilters.NearestToY, dxxPointFilters.FarthestFromY
                                d1 = Math.Abs(v1.Y - aOrdinate)
                            Case dxxPointFilters.NearestToZ, dxxPointFilters.FarthestFromZ
                                d1 = Math.Abs(v1.Z - aOrdinate)
                        End Select

                        minval = d1
                        maxval = d1
                        For i As Integer = 2 To aVectors.Count
                            iMem = aVectors(i - 1)
                            If iMem Is Nothing Then Continue For

                            If bResetMarks Then
                                If TypeOf iMem Is dxfVector Then
                                    aMem = DirectCast(iMem, dxfVector)
                                    aMem.Mark = False
                                End If

                            End If

                            v1 = New TVECTOR(iMem)
                            If Not bSuppressPlane Then
                                v1 = v1.WithRespectTo(workplane, aPrecis:=-1)
                            End If
                            Select Case aControlFlag
                                Case dxxPointFilters.NearestToX, dxxPointFilters.FarthestFromX
                                    d2 = Math.Abs(iMem.X - aOrdinate)
                                Case dxxPointFilters.NearestToY, dxxPointFilters.FarthestFromY
                                    d2 = Math.Abs(iMem.Y - aOrdinate)
                                Case dxxPointFilters.NearestToZ, dxxPointFilters.FarthestFromZ
                                    d2 = Math.Abs(iMem.Z - aOrdinate)
                            End Select
                            If d2 >= maxval Then
                                id1 = i
                                maxval = d2
                            End If
                            If d2 <= minval Then
                                ID2 = i
                                minval = d2
                            End If
                        Next i
                        rIndex = id1
                        If aControlFlag = dxxPointFilters.NearestToX Or aControlFlag = dxxPointFilters.NearestToY Or aControlFlag = dxxPointFilters.NearestToZ Then
                            rIndex = ID2
                        End If
             '===================================================================
                    Case dxxPointFilters.AtX, dxxPointFilters.AtY, dxxPointFilters.AtZ
                        '===================================================================
                        'searching for a vector at a particular ordinate (ie at X = 10)
                        'returns the first one that satisfies

                        For i As Integer = 1 To aVectors.Count
                            iMem = aVectors(i - 1)
                            If iMem Is Nothing Then Continue For

                            If bResetMarks Then
                                If TypeOf iMem Is dxfVector Then
                                    aMem = DirectCast(iMem, dxfVector)
                                    aMem.Mark = False
                                End If

                            End If

                            v1 = New TVECTOR(iMem)

                            If Not bSuppressPlane Then
                                v1 = v1.WithRespectTo(workplane, aPrecis:=-1)
                            End If
                            Select Case aControlFlag
                                Case dxxPointFilters.AtX
                                    comp = Math.Abs(v1.X - aOrdinate)
                                Case dxxPointFilters.AtY
                                    comp = Math.Abs(v1.Y - aOrdinate)
                                Case dxxPointFilters.AtZ
                                    comp = Math.Abs(v1.Z - aOrdinate)
                            End Select
                            If Math.Round(Math.Abs(comp), aPrecis) = 0 Then
                                If rIndex = 0 Then rIndex = i
                                If Not bResetMarks Then Exit For
                            End If
                        Next i
             '===================================================================
                    Case dxxPointFilters.GetTopLeft, dxxPointFilters.GetTopRight, dxxPointFilters.GetBottomLeft, dxxPointFilters.GetBottomRight, dxxPointFilters.GetLeftTop, dxxPointFilters.GetRightTop, dxxPointFilters.GetLeftBottom, dxxPointFilters.GetRightBottom
                        '===================================================================
                        'searching for a relative vector (lower left, top right etc. etc.)
                        aOrdType = -99
                        Select Case aControlFlag
                            Case dxxPointFilters.GetTopLeft
                                aOrdType = dxxOrdinateTypes.MaxY
                                aOrdDesc = dxxOrdinateDescriptors.Y
                                bMin = True
                            Case dxxPointFilters.GetTopRight
                                aOrdType = dxxOrdinateTypes.MaxY
                                aOrdDesc = dxxOrdinateDescriptors.Y
                                bMin = False

                            Case dxxPointFilters.GetBottomLeft
                                aOrdType = dxxOrdinateTypes.MinY
                                aOrdDesc = dxxOrdinateDescriptors.Y
                                bMin = True
                            Case dxxPointFilters.GetBottomRight
                                aOrdType = dxxOrdinateTypes.MinY
                                aOrdDesc = dxxOrdinateDescriptors.Y
                                bMin = False

                            Case dxxPointFilters.GetLeftTop
                                aOrdType = dxxOrdinateTypes.MinX
                                aOrdDesc = dxxOrdinateDescriptors.X
                                bMin = False
                            Case dxxPointFilters.GetRightTop
                                aOrdType = dxxOrdinateTypes.MaxX
                                aOrdDesc = dxxOrdinateDescriptors.X
                                bMin = False

                            Case dxxPointFilters.GetLeftBottom
                                aOrdType = dxxOrdinateTypes.MinX
                                aOrdDesc = dxxOrdinateDescriptors.X
                                bMin = True
                            Case dxxPointFilters.GetRightBottom
                                aOrdType = dxxOrdinateTypes.MaxX
                                aOrdDesc = dxxOrdinateDescriptors.X
                                bMin = True

                        End Select
                        If aOrdType >= 0 Then
                            Dim vlist As List(Of iVector) = aVectors.ToList()
                            aOrd = GetPlaneOrdinate(aVectors, aOrdType, workplane, bSuppressPlane, aPrecis)
                            Dim Pts As List(Of iVector) = GetAtPlanarOrdinate(aVectors, aOrd, aOrdDesc, workplane, aPrecis, bSuppressPlane)
                            If Pts.Count > 0 Then
                                iMem = Pts(0)
                                If bResetMarks Then
                                    If TypeOf iMem Is dxfVector Then
                                        aMem = DirectCast(iMem, dxfVector)
                                        aMem.Mark = False
                                    End If

                                End If
                                v1 = New TVECTOR(iMem)
                                If Not bSuppressPlane Then v1 = v1.WithRespectTo(workplane, aPrecis:=-1)
                                If aOrdDesc = dxxOrdinateDescriptors.Y Then
                                    valu = v1.X
                                Else
                                    valu = v1.Y
                                End If
                                For i = 1 To Pts.Count
                                    iMem = Pts(i - 1)
                                    If bResetMarks Then
                                        If TypeOf iMem Is dxfVector Then
                                            aMem = DirectCast(iMem, dxfVector)
                                            aMem.Mark = False
                                        End If

                                    End If
                                    v1 = New TVECTOR(iMem)
                                    If Not bSuppressPlane Then v1 = v1.WithRespectTo(workplane)
                                    If aOrdDesc = dxxOrdinateDescriptors.Y Then
                                        aOrd = v1.X
                                    Else
                                        aOrd = v1.Y
                                    End If
                                    If bMin Then
                                        If aOrd <= valu Then
                                            valu = aOrd
                                            rIndex = vlist.IndexOf(iMem) + 1
                                        End If
                                    Else
                                        If aOrd >= valu Then
                                            valu = aOrd
                                            rIndex = vlist.IndexOf(iMem) + 1
                                        End If
                                    End If
                                Next i
                            End If
                        End If
                End Select
            End If


            If rIndex > 0 Then
                iMem = aVectors(rIndex - 1)
                If TypeOf iMem Is dxfVector Then
                    aMem = DirectCast(iMem, dxfVector)
                    aMem.Index = rIndex
                End If
                Return iMem
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' returns the vectors in the passed vectors that are contained within the passed rectangle
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aRectangle">the subject rectangle</param>
        ''' <param name="bOnIsIn">flag to the vectors on the bounds of the rectangle as withn the rectangle </param>
        ''' <param name="aPrecis">the precision to apply for the conparison</param>
        ''' <param name="bReturnTheInverse">flag to return the members that ARE NOT contained within the passed rectangle </param>
        ''' <param name="bSuppressPlanes">flag to suppress plane project of the members for the test.</param>
        ''' <param name="bReturnPlanarVectors">flag to return the vectors projected to and with resoect top the plane of the rectangle</param>
        ''' <returns></returns>

        Public Shared Function FindVectors(aVectors As IEnumerable(Of iVector), aRectangle As iRectangle, Optional bOnIsIn As Boolean = True, Optional aPrecis As Integer = 3, Optional bReturnTheInverse As Boolean = False, Optional bSuppressPlanes As Boolean = False, Optional bReturnPlanarVectors As Boolean = False) As List(Of iVector)
            Dim _rVal As New List(Of iVector)
            If aVectors Is Nothing Or aRectangle Is Nothing Then Return _rVal
            If aVectors.Count <= 0 Then Return _rVal

            Dim rectang As dxfRectangle = New dxfRectangle(aRectangle)
            Dim halfwd As Double = rectang.Width / 2
            Dim halfht As Double = rectang.Height / 2
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            Dim left As Double = 0
            Dim right As Double = 0
            Dim bot As Double = 0
            Dim top As Double = 0

            If Not bSuppressPlanes Then
                halfwd = Math.Round(halfwd, aPrecis)
                halfht = Math.Round(halfht, aPrecis)
            Else
                left = Math.Round(aRectangle.Left, aPrecis)
                right = Math.Round(aRectangle.Right, aPrecis)
                bot = Math.Round(aRectangle.Bottom, aPrecis)
                top = Math.Round(aRectangle.Top, aPrecis)

            End If
            For Each iv As iVector In aVectors
                If iv Is Nothing Then Continue For


                If Not bSuppressPlanes Then
                    Dim v As dxfVector = dxfVector.FromIVector(iv)
                    v = v.WithRespectToPlane(aRectangle)
                    Dim X As Double = Math.Abs(Math.Round(v.X, aPrecis))
                    Dim Y As Double = Math.Abs(Math.Round(v.Y, aPrecis))
                    If Not bReturnTheInverse Then
                        If X > halfwd Or Y > halfht Then Continue For
                        If (X = halfwd Or Y = halfht) And Not bOnIsIn Then Continue For
                    Else
                        If X < halfwd Or Y < halfht Then Continue For
                        If (X = halfwd Or Y = halfht) And bOnIsIn Then Continue For

                    End If
                    If bReturnPlanarVectors Then _rVal.Add(v) Else _rVal.Add(iv)
                Else
                    Dim X As Double = Math.Round(iv.X, aPrecis)
                    Dim Y As Double = Math.Round(iv.Y, aPrecis)
                    If Not bReturnTheInverse Then
                        If X < left Or X > right Then Continue For
                        If (X = left Or X = right) And Not bOnIsIn Then Continue For
                        If Y > top Or Y < bot Then Continue For
                        If (Y = top Or Y = bot) And Not bOnIsIn Then Continue For
                    Else
                        If X > left And X < right Then Continue For
                        If (X = left Or X = right) And bOnIsIn Then Continue For
                        If Y < top And Y > bot Then Continue For
                        If (Y = top Or Y = bot) And bOnIsIn Then Continue For
                    End If
                    _rVal.Add(iv)



                End If


            Next

            Return _rVal
        End Function

        ''' <summary>
        ''' returns the vectors in the passed vectors that are contained within the passed circle
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aCircle">the subject circle</param>
        ''' <param name="bOnIsIn">flag to the vectors on the circumference of the circle as withn the circle </param>
        ''' <param name="aPrecis">the precision to apply for the conparison</param>
        ''' <param name="bReturnTheInverse">flag to return the members that ARE NOT contained within the passed circle </param>
        ''' <param name="bSuppressPlanes">flag to suppress plane project of the members for the test.</param>
        ''' <param name="bReturnPlanarVectors">flag to return the vectors projected to the plane of the circle</param>
        ''' <returns></returns>

        Public Shared Function FindVectors(aVectors As IEnumerable(Of iVector), aCircle As iArc, Optional bOnIsIn As Boolean = True, Optional aPrecis As Integer = 3, Optional bReturnTheInverse As Boolean = False, Optional bSuppressPlanes As Boolean = False, Optional bReturnPlanarVectors As Boolean = False) As List(Of iVector)
            Dim _rVal As New List(Of iVector)
            If aVectors Is Nothing Or aCircle Is Nothing Then Return _rVal
            If aVectors.Count <= 0 Then Return _rVal

            If bSuppressPlanes Then bReturnTheInverse = False
            Dim arc As dxeArc = New dxeArc(aCircle)
            Dim plane As dxfPlane = arc.Plane
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            Dim rad As Double = Math.Round(arc.Radius, aPrecis)
            For Each iv As iVector In aVectors
                If iv Is Nothing Then Continue For
                Dim v As dxfVector = dxfVector.FromIVector(iv)


                If Not bSuppressPlanes Then v = v.ProjectedToPlane(plane)
                Dim d1 As Double = plane.Origin.DistanceTo(v, aPrecis)
                If Not bReturnTheInverse Then
                    If d1 > rad Or (d1 = rad And Not bOnIsIn) Then Continue For
                Else
                    If d1 < rad Or (d1 = rad And bOnIsIn) Then Continue For
                End If

            Next

            Return _rVal
        End Function


        ''' <summary>
        ''' returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag
        ''' </summary>
        ''' <param name="aVectors">the subject vector list to search</param>
        ''' <param name="aFilter">search type parameter</param>
        ''' <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        ''' <param name="bOnIsIn">flag indicating if equal values should be returned</param>
        ''' <param name="aRefPt">the reference to use to compute relative distances when the filter is NearestTo or FarthestFrom  </param>
        ''' <param name="aPlane">an optional coordinate system to use</param>
        ''' <param name="aPrecis">a precision for numerical comparison (0 to 15)</param>
        ''' <param name="bReturnPlanarVectors">flag to return the vectors defined with respect to the passed plane</param>
        ''' <param name="rTheOthers">if passed, the members that do not meet the search criteria are add to the the passed list</param>

        Public Shared Function FindVectors(aVectors As IEnumerable(Of iVector), aFilter As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional bOnIsIn As Boolean = True, Optional aRefPt As iVector = Nothing, Optional aPlane As dxfPlane = Nothing,
                                    Optional aPrecis As Integer = 3, Optional bReturnPlanarVectors As Boolean = False, Optional rTheOthers As List(Of iVector) = Nothing) As List(Of iVector)
            Dim _rVal As New List(Of iVector)
            If aVectors Is Nothing Then Return _rVal
            If aVectors.Count <= 0 Then Return _rVal
            Dim aVector As dxfVector
            Dim comp As Double
            Dim iMem As iVector
            Dim v1 As TVERTEX
            Dim bFilt As dxxPointFilters
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            'rIndices = New List(Of Integer)

            Dim bSuppressPlane As Boolean = dxfPlane.IsNull(aPlane)
            Dim workplane As New dxfPlane(aPlane)
            If bSuppressPlane Then bReturnPlanarVectors = False

            If aFilter = dxxPointFilters.NearestToX Or aFilter = dxxPointFilters.NearestToY Or aFilter = dxxPointFilters.NearestToZ Or aFilter = dxxPointFilters.FarthestFromX Or aFilter = dxxPointFilters.FarthestFromY Or aFilter = dxxPointFilters.FarthestFromZ Then

                If aVectors.Count = 1 Then
                    iMem = aVectors(0)
                    If TypeOf iMem Is dxfVector Then
                        aVector = DirectCast(iMem, dxfVector)
                        aVector.Index = 1
                    End If
                    Dim v3 As New TVERTEX(iMem)

                    If bReturnPlanarVectors Then
                        _rVal.Add(New dxfVector(v3.WithRespectTo(workplane)) With {.Index = 1})
                    Else
                        _rVal.Add(iMem)
                    End If
                    Return _rVal

                End If
                Dim ord As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X
                If aFilter = dxxPointFilters.NearestToY Or aFilter = dxxPointFilters.FarthestFromY Then
                    ord = dxxOrdinateDescriptors.Y
                ElseIf aFilter = dxxPointFilters.NearestToZ Or aFilter = dxxPointFilters.FarthestFromZ Then
                    ord = dxxOrdinateDescriptors.Z

                End If
                Dim farther As Boolean = aFilter = dxxPointFilters.FarthestFromX Or aFilter = dxxPointFilters.FarthestFromY Or aFilter = dxxPointFilters.FarthestFromZ

                Dim vList As List(Of iVector) = aVectors.ToList()
                Dim refPt As New TVECTOR(aRefPt)
                Dim comptodistance As Double = IIf(farther, Double.MinValue, Double.MaxValue)
                For Each ivec As iVector In aVectors
                    If ivec Is Nothing Then Continue For
                    If TypeOf ivec Is dxfVector Then
                        aVector = DirectCast(ivec, dxfVector)
                        aVector.Index = vList.IndexOf(ivec) + 1
                    End If
                    Dim v3 As New TVERTEX(ivec)
                    If bReturnPlanarVectors Then v3 = v3.WithRespectTo(workplane)
                    Dim dif As TVECTOR = v3.Vector - refPt
                    Dim d1 As Double
                    If ord = dxxOrdinateDescriptors.X Then
                        d1 = Math.Round(Math.Abs(dif.X), aPrecis)
                    ElseIf ord = dxxOrdinateDescriptors.Y Then
                        d1 = Math.Round(Math.Abs(dif.Y), aPrecis)
                    Else
                        d1 = Math.Round(Math.Abs(dif.Z), aPrecis)
                    End If
                    If farther Then
                        If d1 > comptodistance Then
                            comptodistance = d1
                        End If
                    Else
                        If d1 < comptodistance Then
                            comptodistance = d1
                        End If
                    End If

                Next

                Dim i As Integer = 0
                For Each ivec As iVector In aVectors
                    i += 1
                    If ivec Is Nothing Then Continue For
                    Dim v3 As New TVERTEX(ivec) With {.Index = i}
                    If bReturnPlanarVectors Then v3 = v3.WithRespectTo(workplane)
                    Dim dif As TVECTOR = v3.Vector - refPt
                    Dim d1 As Double
                    If ord = dxxOrdinateDescriptors.X Then
                        d1 = Math.Round(Math.Abs(dif.X), aPrecis)
                    ElseIf ord = dxxOrdinateDescriptors.Y Then
                        d1 = Math.Round(Math.Abs(dif.Y), aPrecis)
                    Else
                        d1 = Math.Round(Math.Abs(dif.Z), aPrecis)
                    End If
                    If d1 = comptodistance Then

                        If bReturnPlanarVectors Then

                            _rVal.Add(New dxfVector(v3) With {.Index = i})
                        Else
                            _rVal.Add(ivec)
                        End If

                    Else
                        If rTheOthers Is Nothing Then Continue For
                        If Not bReturnPlanarVectors Then
                            rTheOthers.Add(ivec)
                        Else
                            rTheOthers.Add(New dxfVector(v3) With {.Index = i})
                        End If
                    End If

                Next

            End If


            If aFilter = dxxPointFilters.GetTopLeft Or aFilter = dxxPointFilters.GetTopRight Or aFilter = dxxPointFilters.GetBottomLeft Or aFilter = dxxPointFilters.GetBottomRight Or aFilter = dxxPointFilters.GetLeftTop Or aFilter = dxxPointFilters.GetRightTop Or aFilter = dxxPointFilters.GetLeftBottom Or aFilter = dxxPointFilters.GetRightBottom Then
                iMem = FindVector(aVectors, aFilter, 0, workplane, aPrecis, bSuppressPlane, True)
                If iMem Is Nothing Then Return _rVal

                Dim vList As List(Of iVector) = aVectors.ToList()
                Dim idx As Integer = vList.IndexOf(iMem) + 1
                If TypeOf iMem Is dxfVector Then
                    aVector = DirectCast(iMem, dxfVector)
                    aVector.Index = idx

                End If

                v1 = New TVERTEX(iMem)
                If bReturnPlanarVectors Then
                    v1 = v1.WithRespectTo(workplane)
                    _rVal.Add(New dxfVector(v1) With {.Index = idx})
                Else
                    _rVal.Add(iMem)
                End If
                Dim v2 As New TVERTEX(v1)

                For i As Integer = 1 To aVectors.Count
                    iMem = aVectors(i - 1)
                    If iMem Is Nothing Or idx = i Then
                        Continue For
                    End If
                    If TypeOf iMem Is dxfVector Then
                        aVector = DirectCast(iMem, dxfVector)
                        aVector.Index = i

                    End If
                    Dim v3 As New TVERTEX(iMem) With {.Index = i}

                    If bReturnPlanarVectors Then v3 = v3.WithRespectTo(workplane)

                    Dim d1 As Double = Math.Round(v3.DistanceTo(v2), aPrecis)
                    If d1 = 0 Then
                        If bReturnPlanarVectors Then

                            _rVal.Add(New dxfVector(v3) With {.Index = i})
                        Else
                            _rVal.Add(iMem)
                        End If

                    Else
                        If rTheOthers Is Nothing Then Continue For
                        If Not bReturnPlanarVectors Then
                            rTheOthers.Add(iMem)
                        Else
                            rTheOthers.Add(New dxfVector(v3) With {.Index = i})
                        End If
                    End If
                Next

                Return _rVal
            End If

            'search for vectors aInteger a particular extreme ordinate
            If aFilter = dxxPointFilters.AtMaxX Or aFilter = dxxPointFilters.AtMaxY Or aFilter = dxxPointFilters.AtMaxY Or aFilter = dxxPointFilters.AtMinX Or aFilter = dxxPointFilters.AtMinY Or aFilter = dxxPointFilters.AtMinZ Then
                If aFilter = dxxPointFilters.AtMaxX Then
                    bFilt = dxxPointFilters.AtX
                    aOrdinate = dxfVectors.GetPlaneOrdinate(aVectors, dxxOrdinateTypes.MaxX, workplane, bSuppressPlane)
                ElseIf aFilter = dxxPointFilters.AtMaxY Then
                    bFilt = dxxPointFilters.AtY
                    aOrdinate = dxfVectors.GetPlaneOrdinate(aVectors, dxxOrdinateTypes.MaxY, workplane, bSuppressPlane)
                ElseIf aFilter = dxxPointFilters.AtMaxZ Then
                    bFilt = dxxPointFilters.AtZ
                    aOrdinate = dxfVectors.GetPlaneOrdinate(aVectors, dxxOrdinateTypes.MaxZ, workplane, bSuppressPlane)
                ElseIf aFilter = dxxPointFilters.AtMinX Then
                    bFilt = dxxPointFilters.AtX
                    aOrdinate = dxfVectors.GetPlaneOrdinate(aVectors, dxxOrdinateTypes.MinX, workplane, bSuppressPlane)
                ElseIf aFilter = dxxPointFilters.AtMinY Then
                    bFilt = dxxPointFilters.AtY
                    aOrdinate = dxfVectors.GetPlaneOrdinate(aVectors, dxxOrdinateTypes.MinY, workplane, bSuppressPlane)
                ElseIf aFilter = dxxPointFilters.AtMinZ Then
                    bFilt = dxxPointFilters.AtZ
                    aOrdinate = dxfVectors.GetPlaneOrdinate(aVectors, dxxOrdinateTypes.MinZ, workplane, bSuppressPlane)
                End If
                aFilter = bFilt
            End If

            'search for vectors aINteger a particular ordinate
            If aFilter = dxxPointFilters.AtX Or aFilter = dxxPointFilters.AtY Or aFilter = dxxPointFilters.AtZ Then
                For i As Integer = 1 To aVectors.Count
                    iMem = aVectors(i - 1)
                    If iMem Is Nothing Then Continue For
                    If TypeOf iMem Is dxfVector Then
                        aVector = DirectCast(iMem, dxfVector)
                        aVector.Index = i
                    End If
                    v1 = New TVERTEX(iMem)

                    If bReturnPlanarVectors Then v1 = v1.WithRespectTo(workplane)
                    If aFilter = dxxPointFilters.AtX Then
                        comp = Math.Abs(Math.Round(v1.X - aOrdinate, aPrecis))
                    ElseIf aFilter = dxxPointFilters.AtY Then
                        comp = Math.Abs(Math.Round(v1.Y - aOrdinate, aPrecis))
                    Else
                        comp = Math.Abs(Math.Round(v1.Z - aOrdinate, aPrecis))
                    End If
                    If comp = 0 Then
                        If Not bReturnPlanarVectors Then
                            _rVal.Add(iMem)
                        Else
                            _rVal.Add(New dxfVector(v1))
                        End If
                        'rIndices.Add(i)
                    Else
                        If rTheOthers Is Nothing Then Continue For
                        If Not bReturnPlanarVectors Then
                            rTheOthers.Add(iMem)
                        Else
                            rTheOthers.Add(New dxfVector(v1))
                        End If

                    End If
                Next i
                Return _rVal
            End If
            Dim greaterthan As Boolean = aFilter = dxxPointFilters.GreaterThanX Or aFilter = dxxPointFilters.GreaterThanY Or aFilter = dxxPointFilters.GreaterThanZ
            Dim lessthan As Boolean = aFilter = dxxPointFilters.LessThanX Or aFilter = dxxPointFilters.LessThanY Or aFilter = dxxPointFilters.LessThanZ
            'search for vectors greater than a particular ordinate
            If greaterthan Or lessthan Then
                Dim ord As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X
                If aFilter = dxxPointFilters.GreaterThanY Or aFilter = dxxPointFilters.LessThanY Then
                    ord = dxxOrdinateDescriptors.Y
                ElseIf aFilter = dxxPointFilters.GreaterThanZ Or aFilter = dxxPointFilters.LessThanZ Then
                    ord = dxxOrdinateDescriptors.Z

                End If

                For i As Integer = 1 To aVectors.Count
                    iMem = aVectors(i - 1)
                    If iMem Is Nothing Then Continue For
                    If TypeOf iMem Is dxfVector Then
                        aVector = DirectCast(iMem, dxfVector)
                        aVector.Index = i
                    End If
                    v1 = New TVERTEX(iMem)
                    If bReturnPlanarVectors Then v1 = v1.WithRespectTo(workplane)
                    Select Case ord
                        Case dxxOrdinateDescriptors.X
                            comp = Math.Round(v1.X - aOrdinate, aPrecis)
                        Case dxxOrdinateDescriptors.Y
                            comp = Math.Round(v1.Y - aOrdinate, aPrecis)
                        Case dxxOrdinateDescriptors.Z
                            comp = Math.Round(v1.Z - aOrdinate, aPrecis)
                    End Select
                    Dim keep As Boolean = IIf(greaterthan, comp >= 0, comp <= 0)
                    If keep And comp = 0 Then
                        If Not bOnIsIn Then keep = False
                    End If
                    If keep Then

                        If Not bReturnPlanarVectors Then
                            _rVal.Add(iMem)
                        Else
                            _rVal.Add(New dxfVector(v1))
                        End If
                        'rIndices.Add(i)
                    Else
                        If rTheOthers Is Nothing Then Continue For
                        If Not bReturnPlanarVectors Then
                            rTheOthers.Add(iMem)
                        Else
                            rTheOthers.Add(New dxfVector(v1))
                        End If

                    End If
                Next i
            End If
            '


            Return _rVal
        End Function


        Friend Shared Function GetTVERTICES(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = -1, Optional bJustProject As Boolean = False) As TVERTICES
            '#1the subject vectors objects

            '#2the plane to define the returned vertices with respect to
            '#3the precis to apply if the vertices should be define with respect to the plane
            '#4flag to just project to the plane and not define the return with respcet to the plane
            '^used to create a collection of dxfVectors based on the passed collection or single xyz object(s)
            '~clones are returned if real vectors are not passed

            Dim _rVal As New TVERTICES(0)
            If aVectors Is Nothing Then Return _rVal


            Dim projectit As Boolean = Not dxfPlane.IsNull(aPlane)

            Dim aPln As TPLANE = IIf(projectit, New TPLANE(aPlane), TPLANE.World)
            Dim prec As Integer = TVALUES.LimitedValue(aPrecis, -1, 15)

            For Each ivec As iVector In aVectors
                If ivec Is Nothing Then Continue For
                Dim v1 As TVERTEX = New TVERTEX(ivec)
                If projectit Then
                    If bJustProject Then
                        v1.ProjectTo(aPln)
                    Else
                        v1 = v1.WithRespectTo(aPln)
                        If prec >= 0 Then v1.SetCoordinates(Math.Round(v1.X, prec), Math.Round(v1.Y, prec), Math.Round(v1.Z, prec))
                    End If
                End If

                _rVal.Add(v1)
            Next
            Return _rVal

        End Function

        Friend Shared Function GetTVECTORS(aVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = -1, Optional bJustProject As Boolean = False) As TVECTORS

            '#1the subject vectors objects

            '#2the plane to define the returned vertices with respect to
            '#3the precis to apply if the vertices should be define with respect to the plane
            '#4flag to just project to the plane and not define the return with respcet to the plane
            '^used to create a collection of dxfVectors based on the passed collection or single xyz object(s)
            '~clones are returned if real vectors are not passed

            Dim _rVal As New TVECTORS(0)
            If aVectors Is Nothing Then Return _rVal


            Dim projectit As Boolean = Not dxfPlane.IsNull(aPlane)

            Dim aPln As TPLANE = IIf(projectit, New TPLANE(aPlane), TPLANE.World)
            Dim prec As Integer = TVALUES.LimitedValue(aPrecis, -1, 15)

            For Each ivec As iVector In aVectors
                If ivec Is Nothing Then Continue For
                Dim v1 As TVECTOR = New TVECTOR(ivec)
                If projectit Then
                    If bJustProject Then
                        v1.ProjectTo(aPln)
                    Else
                        v1 = v1.WithRespectTo(aPln)
                        If prec >= 0 Then v1.SetCoordinates(Math.Round(v1.X, prec), Math.Round(v1.Y, prec), Math.Round(v1.Z, prec))
                    End If
                End If

                _rVal.Add(v1)
            Next
            Return _rVal

        End Function


        Public Shared Function GetAtPlanarOrdinate(aVectors As IEnumerable(Of iVector), aOrdinate As Double, aOrdinateType As dxxOrdinateDescriptors, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bSuppressPlane As Boolean = False) As List(Of iVector)
            Dim _rVal As New List(Of iVector)
            If aVectors Is Nothing Then Return _rVal
            '#1the coordinate system to use
            '#2the coordinate to match
            '#3the coordinate type to match
            '#4a precision for the comparison (0 to 15)
            '^searchs for and returns vectors from the collection whose specified ordinate  match the passed ordinate
            aPrecis = dxfUtils.LimitedValue(aPrecis, 0, 15)

            If dxfPlane.IsNull(aPlane) Then bSuppressPlane = True
            Dim workplane As New dxfPlane(aPlane)

            For Each aMem As iVector In aVectors

                If aMem Is Nothing Then Continue For
                Dim vOrd As Double = 0
                Dim v1 As TVECTOR = New TVECTOR(aMem)
                If Not bSuppressPlane Then v1 = v1.WithRespectTo(workplane, aPrecis:=15)
                Select Case aOrdinateType
                    Case dxxOrdinateDescriptors.Y
                        vOrd = Math.Round(v1.Y, aPrecis)
                    Case dxxOrdinateDescriptors.Z
                        vOrd = Math.Round(v1.Z, aPrecis)
                    Case Else
                        vOrd = Math.Round(v1.X, aPrecis)
                End Select
                Dim aDif As Double = Math.Round(vOrd - aOrdinate, aPrecis)
                If aDif = 0 Then _rVal.Add(aMem)
            Next

            Return _rVal
        End Function

        ''' <summary>
        ''' returns the requested ordinate based on the search parameter and the members of the passed collection
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aSearchParam">parameter controling the value returned</param>
        ''' <param name="aPlane">a plane to define the member ordinates against</param>
        ''' <param name="bSuppressPlane"></param>
        ''' <param name="aPrecis"></param>
        ''' <returns>Double</returns>
        Public Shared Function GetPlaneOrdinate(aVectors As IEnumerable(Of iVector), aSearchParam As dxxOrdinateTypes, Optional aPlane As dxfPlane = Nothing, Optional bSuppressPlane As Boolean = False, Optional aPrecis As Integer = -1) As Double
            Dim _rVal As Double = 0
            If aVectors Is Nothing Then Return 0

            Dim imaxX As Integer
            Dim iminX As Integer
            Dim imaxY As Integer
            Dim iminY As Integer
            Dim imaxZ As Integer
            Dim iminZ As Integer
            If dxfPlane.IsNull(aPlane) Then bSuppressPlane = True
            Dim workplane As New dxfPlane(aPlane)


            Dim vMax As TVECTOR = New TVECTOR(-Double.MaxValue, -Double.MaxValue, -Double.MaxValue)
            Dim vMin As TVECTOR = New TVECTOR(Double.MaxValue, Double.MaxValue, Double.MaxValue)
            Dim i As Integer = 0
            For Each mem As iVector In aVectors
                i += 1
                If mem Is Nothing Then Continue For
                Dim v1 As TVECTOR = New TVECTOR(mem)
                If Not bSuppressPlane Then v1 = v1.WithRespectTo(workplane)
                If aPrecis >= 0 Then v1 = v1.Rounded(aPrecis)
                If v1.X > vMax.X Then
                    imaxX = i
                    vMax.X = v1.X
                End If
                If v1.X < vMin.X Then
                    iminX = i
                    vMin.X = v1.X
                End If
                If v1.Y > vMax.Y Then
                    imaxY = i
                    vMax.Y = v1.Y
                End If
                If v1.Y < vMin.Y Then
                    iminY = i
                    vMin.Y = v1.Y
                End If
                If v1.Z > vMax.Z Then
                    imaxZ = i
                    vMax.Z = v1.Z
                End If
                If v1.Z < vMin.Z Then
                    iminZ = i
                    vMin.Z = v1.Z
                End If
            Next

            Select Case aSearchParam
                Case dxxOrdinateTypes.MinX
                    If iminX > 0 Then
                        _rVal = vMin.X
                    End If
                Case dxxOrdinateTypes.MinY
                    If iminY > 0 Then
                        _rVal = vMin.Y
                    End If
                Case dxxOrdinateTypes.MinZ
                    If iminZ > 0 Then
                        _rVal = vMin.Z
                    End If
                Case dxxOrdinateTypes.MaxX
                    If imaxX > 0 Then
                        _rVal = vMax.X
                    End If
                Case dxxOrdinateTypes.MaxY
                    If imaxY > 0 Then
                        _rVal = vMax.Y
                    End If
                Case dxxOrdinateTypes.MaxZ
                    If imaxZ > 0 Then
                        _rVal = vMax.Z
                    End If
                Case dxxOrdinateTypes.MidX
                    If imaxX > 0 And iminX > 0 Then
                        _rVal = vMin.X + (vMax.X - vMin.X) / 2
                    End If
                Case dxxOrdinateTypes.MidY
                    If imaxY > 0 And iminY > 0 Then
                        _rVal = vMin.Y + (vMax.Y - vMin.Y) / 2
                    End If
                Case dxxOrdinateTypes.MidZ
                    If imaxZ > 0 And iminZ > 0 Then
                        _rVal = vMin.Z + (vMax.Z - vMin.Z) / 2
                    End If
            End Select
            Return _rVal
        End Function

        ''' <summary>
        ''' sets the indicated ordinate of the passsed vectors to the passed value
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aOrdinateType">the target ordinate X, Y or Z</param>
        ''' <param name="aOrdinateValue">the value to assign to the vectors X, Y or Z ordinate</param>
        ''' <param name="aMatchOrdinate">if passed only the members with ordiantes that currently match the match ordinate are affected</param>
        ''' <param name="aPrecis">the precis to use to comare the match ordinates</param>
        ''' <returns></returns>
        Public Shared Function SetMemberOrdinates(aVectors As IEnumerable(Of iVector), aOrdinateType As dxxOrdinateDescriptors, aOrdinateValue As Double, Optional aMatchOrdinate As Double? = Nothing, Optional aPrecis As Integer? = Nothing) As List(Of iVector)
            If aVectors Is Nothing Then Return New List(Of iVector)
            Dim prec As Integer = 15
            If aPrecis.HasValue Then prec = TVALUES.LimitedValue(aPrecis.Value, 0, 15, 15)
            Dim _rVal As New List(Of iVector)

            For Each vector As iVector In aVectors
                If vector Is Nothing Then Continue For
                If aMatchOrdinate.HasValue Then
                    Select Case aOrdinateType
                        Case dxxOrdinateDescriptors.X
                            If Math.Round(vector.X, prec) <> Math.Round(aMatchOrdinate.Value, prec) Then Continue For
                        Case dxxOrdinateDescriptors.Y
                            If Math.Round(vector.Y, prec) <> Math.Round(aMatchOrdinate.Value, prec) Then Continue For
                        Case dxxOrdinateDescriptors.Z
                            If Math.Round(vector.Z, prec) <> Math.Round(aMatchOrdinate.Value, prec) Then Continue For
                    End Select

                End If

                Select Case aOrdinateType
                    Case dxxOrdinateDescriptors.X
                        If vector.X <> aOrdinateValue Then
                            _rVal.Add(vector)
                            vector.X = aOrdinateValue
                        End If

                    Case dxxOrdinateDescriptors.Y
                        If vector.Y <> aOrdinateValue Then
                            _rVal.Add(vector)
                            vector.Y = aOrdinateValue
                        End If
                    Case dxxOrdinateDescriptors.Z
                        If vector.Z <> aOrdinateValue Then
                            _rVal.Add(vector)
                            vector.Z = aOrdinateValue
                        End If
                End Select


            Next
            Return _rVal
        End Function

        Public Shared Function WithRespectToPlane(aVectors As IEnumerable(Of iVector), aPlane As dxfPlane, Optional aTransferPlane As dxfPlane = Nothing, Optional aTransferElevation As Double? = Nothing, Optional aTransferRotation As Double? = Nothing, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing) As List(Of dxfVector)
            If aVectors Is Nothing Then Return New List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            Dim aPl As New dxfPlane(aPlane)
            For Each vector As iVector In aVectors
                Dim v1 As dxfVector = dxfVector.FromIVector(vector)
                Dim v2 As dxfVector = v1.WithRespectToPlane(aPlane:=aPl, aTransferPlane:=aTransferPlane, aTransferElevation:=aTransferElevation, aTransferRotation:=aTransferRotation, aXScale:=aXScale, aYScale:=aYScale)
                _rVal.Add(v2)
            Next
            Return _rVal
        End Function
        ''' <summary>
        ''' sorts the vectors in the collection in the requested order
        ''' </summary>
        ''' <param name="aVectors"> the subject vectors</param>
        ''' <param name="aOrder"> the order to apply</param>
        ''' <param name="aReferencePt" >the reference to use for relative orders"</param>
        ''' <param name="aPlane"> the plane to use</param>
        ''' <param name="rChanged"> returns true if a member changed position in the list</param>
        ''' <param name="rIndices"> returns the indices of the reordered members</param>
        ''' <param name="aPrecis" > the precision to apply for comparisons</param>
        ''' <returns></returns>
        Public Shared Function Sort(aVectors As IEnumerable(Of iVector), aOrder As dxxSortOrders, aReferencePt As iVector, aPlane As dxfPlane, ByRef rChanged As Boolean, ByRef rIndices As List(Of Integer), Optional aPrecis As Integer = 3) As List(Of iVector)
            rChanged = False
            Dim _rVal As New List(Of iVector)
            rIndices = New List(Of Integer)
            If aVectors Is Nothing Then Return _rVal
            Dim verts As List(Of iVector) = aVectors.ToList()
            If verts.Count = 0 Then Return _rVal
            If verts.Count = 1 Then
                _rVal.Add(verts.Item(0))
                Return _rVal
            End If

            If aPlane Is Nothing Then aPlane = dxfPlane.World

            Dim i As Integer
            Dim v1 As TVECTOR
            Dim idx As Integer
            Dim j As Integer

            Dim tpl As Tuple(Of Double, Integer)
            Dim srt As List(Of Tuple(Of Double, Integer))
            Dim workplane As New dxfPlane(aPlane)

            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)

            Select Case aOrder
         '========== nearest to farthest etc ========================
                Case dxxSortOrders.NearestToFarthest, dxxSortOrders.FarthestToNearest
                    v1 = New TVECTOR(aReferencePt)

                    srt = New List(Of Tuple(Of Double, Integer))

                    For i = 1 To verts.Count
                        Dim mem As iVector = verts.Item(i - 1)
                        If mem Is Nothing Then Continue For

                        Dim v2 As TVECTOR = New TVECTOR(mem)
                        srt.Add(New Tuple(Of Double, Integer)(Math.Round(v1.DistanceTo(v2), aPrecis), i))
                    Next i
                    srt.Sort(Function(tupl1 As Tuple(Of Double, Integer), tupl2 As Tuple(Of Double, Integer)) tupl1.Item1.CompareTo(tupl2.Item1))
                    If aOrder = dxxSortOrders.FarthestToNearest Then srt.Reverse()
                    j = 0
                    For Each tpl In srt
                        idx = tpl.Item2
                        Dim mem As iVector = verts.Item(idx - 1)
                        j += 1
                        If (j <> idx) Then
                            rChanged = True

                        End If
                        _rVal.Add(mem)
                        rIndices.Add(idx)
                    Next


         '========== clockwise or counterclockwise ========================
                Case dxxSortOrders.Clockwise, dxxSortOrders.CounterClockwise
                    _rVal = SortClockWise(aVectors, aReferencePt, 0, bReverseSort:=aOrder = dxxSortOrders.CounterClockwise, aPlane:=aPlane, rChanged:=rChanged, rIndices:=rIndices)
         '========== left to right etc. ========================
                Case dxxSortOrders.LeftToRight, dxxSortOrders.RightToLeft, dxxSortOrders.TopToBottom, dxxSortOrders.BottomToTop
                    Dim frstV As iVector = Nothing
                    Dim DoX As Boolean
                    Dim v2 As TVECTOR
                    Dim d1 As Double
                    Select Case aOrder
                        Case dxxSortOrders.LeftToRight
                            DoX = True
                            frstV = FindVector(verts, dxxPointFilters.AtMinX, 0, workplane, aPrecis, bResetMarks:=True)
                        Case dxxSortOrders.RightToLeft
                            DoX = True
                            frstV = FindVector(verts, dxxPointFilters.AtMaxX, 0, workplane, aPrecis, bResetMarks:=True)
                        Case dxxSortOrders.TopToBottom
                            frstV = FindVector(verts, dxxPointFilters.AtMaxY, 0, workplane, aPrecis, bResetMarks:=True)
                        Case dxxSortOrders.BottomToTop
                            frstV = FindVector(verts, dxxPointFilters.AtMinY, 0, workplane, aPrecis, bResetMarks:=True)
                    End Select
                    idx = verts.IndexOf(frstV) + 1
                    If idx = 0 Then
                        frstV = verts(0)
                        idx = 1
                        Console.WriteLine($" Sort Error {aOrder}")
                    End If
                    srt = New List(Of Tuple(Of Double, Integer))({New Tuple(Of Double, Integer)(0, idx)})
                    v1 = New TVECTOR(frstV).WithRespectTo(aPlane)
                    For i = 1 To verts.Count
                        Dim mem As iVector = verts.Item(i - 1)
                        If mem Is Nothing Then Continue For

                        If i <> idx Then

                            v2 = New TVECTOR(mem).WithRespectTo(aPlane)
                            If DoX Then d1 = Math.Abs(v2.X - v1.X) Else d1 = Math.Abs(v2.Y - v1.Y)
                            srt.Add(New Tuple(Of Double, Integer)(d1, i))
                        End If
                    Next i
                    srt.Sort(Function(tupl1 As Tuple(Of Double, Integer), tupl2 As Tuple(Of Double, Integer)) tupl1.Item1.CompareTo(tupl2.Item1))
                    j = 0
                    For Each tpl In srt
                        idx = tpl.Item2
                        If idx > 0 Then
                            Dim mem As iVector = verts.Item(idx - 1)
                            j += 1
                            If (j <> idx) Then
                                rChanged = True

                            End If
                            _rVal.Add(mem)
                            rIndices.Add(idx)
                        Else
                            Console.WriteLine($" Sort Error")
                        End If

                    Next



         '========== top to bottom left to right ========================
                Case dxxSortOrders.TopToBottomAndLeftToRight
                    rChanged = True

                    Dim aSubSet As List(Of iVector)
                    Dim aValues As List(Of Double) = PlanarOrdinates(verts, dxxOrdinateDescriptors.Y, aPlane, 4, True)
                    Dim aMem As dxfVector
                    Dim d1 As Double
                    aValues.Sort()
                    aValues.Reverse()
                    For Each d1 In aValues
                        aSubSet = GetAtPlanarOrdinate(verts, d1, dxxOrdinateDescriptors.Y, aPlane, 4)
                        srt = New List(Of Tuple(Of Double, Integer))
                        For Each aMem In aSubSet
                            v1 = New TVECTOR(aMem).WithRespectTo(aPlane, aPrecis:=4)
                            idx = verts.IndexOf(aMem) + 1
                            srt.Add(New Tuple(Of Double, Integer)(v1.X, idx))
                        Next
                        srt.Sort(Function(tupl1 As Tuple(Of Double, Integer), tupl2 As Tuple(Of Double, Integer)) tupl1.Item1.CompareTo(tupl2.Item1))
                        j = 0
                        For Each tpl In srt
                            idx = tpl.Item2
                            Dim mem As iVector = verts.Item(idx - 1)
                            j += 1

                            If (j <> idx) Then
                                rChanged = True

                            End If
                            _rVal.Add(mem)
                            rIndices.Add(idx)
                        Next
                    Next


            End Select

            Return _rVal
        End Function

        ''' <summary>
        ''' sorts the vectors in the collection in the requested order
        ''' </summary>
        ''' <param name="aVectors"> the subject vectors</param>
        ''' <param name="aOrder"> the order to apply</param>
        ''' <param name="aReferencePt" >the reference to use for relative orders"</param>
        ''' <param name="aPrecis" > the precision to apply for comparisons</param>
        ''' <param name="aPlane"> the plane to use</param>
        ''' <returns></returns>
        Public Shared Function Sort(aVectors As IEnumerable(Of iVector), aOrder As dxxSortOrders, aReferencePt As iVector, Optional aPrecis As Integer = 3, Optional aPlane As dxfPlane = Nothing) As List(Of iVector)
            Dim changed As Boolean
            Dim indices As New List(Of Integer)
            Return Sort(aVectors, aOrder, aReferencePt, aPlane, changed, indices, aPrecis)
        End Function

        ''' <summary>
        ''' sorts the vectors in the collection in the requested order
        ''' </summary>
        ''' <param name="aVectors"> the subject vectors</param>
        ''' <param name="aCenter"> an alternative  center to use for the operation. If not passed the plane origin is used"</param>
        ''' <param name="aBaseAngle"> the angle to cosider as the base</param>
        ''' <param name="bReverseSort"> if true, the return is sorted counter clockwise rather than clock wise</param>
        ''' <param name="aPlane" > the plane to use</param>
        ''' <param name="aPrecis"> the precision to apply for comparisons</param>
        ''' <returns></returns>
        Public Shared Function SortClockWise(aVectors As IEnumerable(Of iVector), Optional aCenter As iVector = Nothing, Optional aBaseAngle As Double = 0, Optional bReverseSort As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3) As List(Of iVector)
            Dim rIndices As New List(Of Integer)
            Dim rChanged As Boolean = False
            Return SortClockWise(aVectors, aCenter, aBaseAngle, bReverseSort, aPlane, rChanged, rIndices, aPrecis)

        End Function

        ''' <summary>
        ''' sorts the vectors in the collection in the requested order
        ''' </summary>
        ''' <param name="aVectors"> the subject vectors</param>
        ''' <param name="aCenter"> an alternative  center to use for the operation. If not passed the plane origin is used"</param>
        ''' <param name="aBaseAngle"> the angle to cosider as the base</param>
        ''' <param name="bReverseSort"> if true, the return is sorted counter clockwise rather than clock wise</param>
        ''' <param name="aPlane" > the plane to use</param>
        ''' <param name="aPrecis"> the precision to apply for comparisons</param>
        ''' <returns></returns>
        Public Shared Function SortClockWise(aVectors As IEnumerable(Of iVector), aCenter As iVector, aBaseAngle As Double, bReverseSort As Boolean, aPlane As dxfPlane, ByRef rChanged As Boolean, ByRef rIndices As List(Of Integer), Optional aPrecis As Integer = 3) As List(Of iVector)
            rIndices = New List(Of Integer)
            rChanged = False
            If aVectors Is Nothing Then Return New List(Of iVector)
            If aVectors.Count = 1 Then
                rIndices.Add(1)
                Return New List(Of iVector)({aVectors.ElementAt(0)})
            End If

            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            Dim v1 As TVECTOR
            Dim ang1 As Double
            Dim v0 As TVECTOR
            'Dim arSort As New TVALUES(0)
            'Dim aIndices As New TVALUES(0)
            Dim _rVal As New List(Of iVector)
            Dim j As Integer
            Dim idx As Integer

            Dim bPlane As TPLANE = New TPLANE(aPlane)
            Dim tpl As Tuple(Of Double, Integer)
            Dim srt As New List(Of Tuple(Of Double, Integer))
            Dim verts As List(Of iVector) = aVectors.ToList()
            'Dim angleToIndex As New Dictionary(Of Integer, Integer)
            If aCenter IsNot Nothing Then bPlane.Origin = New TVECTOR(aCenter)
            If aBaseAngle <> 0 Then bPlane.Revolve(aBaseAngle, False)
            For i As Integer = 1 To verts.Count
                Dim mem As iVector = verts.Item(i - 1)

                v0 = New TVECTOR(mem)

                v1 = v0.WithRespectTo(bPlane)
                ang1 = 0
                If v1.X <> 0 Or v1.Y <> 0 Then
                    If v1.X = 0 Then
                        If v1.Y > 0 Then ang1 = 90 Else ang1 = 270
                    ElseIf v1.Y = 0 Then
                        If v1.X > 0 Then ang1 = 0 Else ang1 = 180
                    Else
                        ang1 = Math.Atan(Math.Abs(v1.Y) / Math.Abs(v1.X)) * 180 / Math.PI
                        If v1.X > 0 Then
                            If v1.Y < 0 Then ang1 = 360 - ang1
                        Else
                            If v1.Y > 0 Then
                                ang1 = 180 - ang1
                            Else
                                ang1 = 180 + ang1
                            End If
                        End If
                    End If
                End If
                ang1 = Math.Round(ang1, aPrecis)
                If ang1 = 0 Then ang1 = 360
                'arSort.Add(ang1 + 360)
                'If Not angleToIndex.ContainsKey(ang1 + 360) Then angleToIndex.Add(ang1 + 360, i)
                srt.Add(New Tuple(Of Double, Integer)(ang1 + 360, i))
            Next i
            srt.Sort(Function(tupl1 As Tuple(Of Double, Integer), tupl2 As Tuple(Of Double, Integer)) tupl1.Item1.CompareTo(tupl2.Item1))
            If Not bReverseSort Then srt.Reverse()
            'aIndices = arSort.Sorted(Not bReverseSort, bNumeric:=True)
            j = 0
            For Each tpl In srt
                idx = tpl.Item2
                j += 1
                Dim P1 As iVector = verts.Item(idx - 1)
                If idx <> j Then
                    rChanged = True

                End If
                rIndices.Add(idx)
                _rVal.Add(P1)
            Next



            Return _rVal
        End Function

        Public Shared Function PlanarOrdinates(aVectors As IEnumerable(Of iVector), aOrdinateType As dxxOrdinateDescriptors, aPlane As dxfPlane, Optional aPrecis As Integer = -1, Optional bSortLowToHigh As Boolean = False, Optional bSuppressPlane As Boolean = False, Optional bUniqueVaues As Boolean = True) As List(Of Double)
            Dim _rVal As New List(Of Double)
            If aVectors Is Nothing Then Return _rVal
            If aVectors.Count <= 0 Then Return _rVal

            '^returns the unique X,Y or Z ordinates referred to by at least one of the vectors in the collection
            '^used to query the collection about the ordinates of the vectors in the current collection
            Dim aOrd As Double
            Dim bHaveIt As Boolean
            If aPlane Is Nothing Then aPlane = New dxfPlane()
            If aOrdinateType < dxxOrdinateDescriptors.X And aOrdinateType > dxxOrdinateDescriptors.Z Then aOrdinateType = dxxOrdinateDescriptors.X
            aPrecis = TVALUES.LimitedValue(aPrecis, -1, 15)

            For Each vi In aVectors
                Dim v1 As New TVECTOR(vi)
                If Not bSuppressPlane Then v1 = v1.WithRespectTo(aPlane)
                If aOrdinateType = dxxOrdinateDescriptors.Y Then
                    aOrd = v1.Y
                ElseIf aOrdinateType = dxxOrdinateDescriptors.Z Then
                    aOrd = v1.Z
                Else
                    aOrd = v1.X
                End If
                If aPrecis >= 0 Then aOrd = Math.Round(aOrd, aPrecis)
                If bUniqueVaues Then
                    bHaveIt = _rVal.FindIndex(Function(ord) ord = aOrd) >= 0
                Else
                    bHaveIt = False
                End If
                If Not bHaveIt Then _rVal.Add(aOrd)
            Next

            If bSortLowToHigh Then
                _rVal.Sort()
            End If
            Return _rVal
        End Function
        ''' <summary>
        ''' returns the vector from the collection which is the nearest to or farthest from the passed vector
        ''' </summary>
        ''' <param name="bNearest">If true, the nearest member is return otherwise the farthest</param>
        ''' <param name="aVectors"></param>
        '''  <param name="aSearchVector">the vector to compare to</param>
        ''' <param name="aDirection">if passed, only members whose direction to or from the passed search vector are considered</param>
        ''' <param name="bCompareInverseDirection">flag to only consider members whose direction is from the search vector to the member is a direction is passed</param>
        ''' <param name="bReturnClone"> flag to return a clone</param>
        ''' <param name="aSkipMem"></param>
        ''' <returns></returns>
        Friend Shared Function GetRelativeMember(bNearest As Boolean, aVectors As IEnumerable(Of iVector), aSearchVector As iVector, Optional aDirection As dxfDirection = Nothing, Optional bCompareInverseDirection As Boolean = False, Optional bReturnClone As Boolean = False, Optional aSkipMem As iVector = Nothing) As iVector
            Dim rDistance As Double = 0
            Dim rIndex As Integer = 0
            Return GetRelativeMember(bNearest, aVectors, aSearchVector, rDistance, rIndex, aDirection, bCompareInverseDirection, bReturnClone, aSkipMem:=aSkipMem)
        End Function

        ''' <summary>
        ''' returns the vector from the collection which is the nearest to or farthest from the passed vector
        ''' </summary>
        ''' <param name="bNearest">If true, the nearest member is return otherwise the farthest</param>
        ''' <param name="aVectors"></param>
        '''  <param name="aSearchVector">the vector to compare to</param>
        ''' <param name="rDistance">returns the distance between the the search vector and the returned (if there is one)</param>
        ''' <param name="rIndex">returns the index of the return vector</param>
        ''' <param name="aDirection">if passed, only members whose direction to or from the passed search vector are considered</param>
        ''' <param name="bCompareInverseDirection">flag to only consider members whose direction is from the search vector to the member is a direction is passed</param>
        ''' <param name="bReturnClone"> flag to return a clone</param>
        ''' <param name="aSkipMem"></param>
        ''' <returns></returns>
        Friend Shared Function GetRelativeMember(bNearest As Boolean, aVectors As IEnumerable(Of iVector), aSearchVector As iVector, ByRef rDistance As Double, ByRef rIndex As Integer, Optional aDirection As dxfDirection = Nothing, Optional bCompareInverseDirection As Boolean = False, Optional bReturnClone As Boolean = False, Optional aSkipMem As iVector = Nothing) As iVector
            If bNearest Then rDistance = Double.MaxValue Else rDistance = Double.MinValue
            rIndex = 0
            If aVectors Is Nothing Then Return Nothing
            If aVectors.Count <= 0 Then Return Nothing
            '#1the vectors to search
            '#2the vector to compare to
            '#3returns the distance between the passed vector and the returned vector
            '#4 returns the index of the return vectro in the passed search list (base 1) 
            '^returns the vector from the collection which is the nearest to the passed vector
            Dim _rVal As iVector = Nothing
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim testdist As Double = rDistance

            Dim bTestDir As Boolean = aDirection IsNot Nothing
            Dim tDir As TVECTOR
            Dim bFlag As Boolean
            Dim srcV As New TVECTOR(aSearchVector)
            Dim aDir As TVECTOR

            If bTestDir Then tDir = aDirection.Strukture
            For i As Integer = 1 To aVectors.Count
                Dim P1 As iVector = aVectors(i - 1)
                If P1 Is Nothing Then Continue For
                If aSkipMem IsNot Nothing Then
                    If P1 Is aSkipMem Then
                        Continue For
                    End If
                End If

                v1 = New TVECTOR(P1)
                If Not bTestDir Then
                    d1 = v1.DistanceTo(srcV)
                    If bNearest Then
                        If d1 < testdist Then
                            testdist = d1
                            rIndex = i
                        End If
                    Else
                        If d1 > testdist Then
                            testdist = d1
                            rIndex = i
                        End If
                    End If
                Else
                    aDir = srcV.DirectionTo(v1, False, rDirectionIsNull:=bFlag)
                    If bFlag Then aDir = tDir
                    If Not aDir.Equals(tDir, bCompareInverse:=bCompareInverseDirection, aPrecis:=3) Then Continue For
                    d1 = v1.DistanceTo(srcV)
                    If bNearest Then
                        If d1 < testdist Then
                            testdist = d1
                            rIndex = i
                        End If
                    Else
                        If d1 > testdist Then
                            testdist = d1
                            rIndex = i
                        End If
                    End If
                End If

            Next i
            If rIndex > 0 Then
                rDistance = testdist
                _rVal = aVectors(rIndex - 1)

            Else
                rDistance = 0
            End If
            Return _rVal


        End Function

        ''' <summary>
        ''' returns True if a vector with coordinates match those of the passed vector is in the collection
        ''' </summary>
        ''' <param name="aVectors">the vectors to search</param>
        ''' <param name="aVector"></param>
        ''' <param name="rExistingIndex">returns the index of the matching vector if it exists (zero based)</param>
        ''' <param name="aPrecis">the precision to use for the comparison</param>
        ''' <returns></returns>
        Friend Shared Function ContainsVector(aVectors As IEnumerable(Of iVector), aVector As iVector, ByRef rExistingIndex As Integer, Optional aPrecis As Integer = 4) As Boolean
            rExistingIndex = -1
            If aVectors Is Nothing Then Return False
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Dim v1 As TVECTOR = New TVECTOR(aVector).Rounded(aPrecis)
            rExistingIndex = aVectors.ToList().FindIndex(Function(x) Math.Round(x.X, aPrecis) = v1.X And Math.Round(x.Y, aPrecis) = v1.Y And Math.Round(x.Z, aPrecis) = v1.Z)
            Return rExistingIndex >= 0

        End Function

        ''' <summary>
        ''' used to set the tags and flags of the members in one call
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aTag">the new tag to assign to the members. null input is ignored.</param>
        ''' <param name="aFlag">the new flag to assign to the members. null input is ignored.</param>
        ''' <param name="aSearchTag">an existing tag to match</param>
        ''' <param name="aSearchFlag">an existing flag to match</param>
        ''' <param name="aStartID">an optional starting index</param>
        ''' <param name="aEndID">an optional ending index</param>
        ''' <param name="bAddTagIndices">flag to append the index of the member to the passed tag value for each member</param>
        Public Shared Sub SetTagsAndFlags(aVectors As List(Of dxfVector), Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aSearchTag As String = Nothing, Optional aSearchFlag As String = Nothing, Optional aStartID As Integer = -1, Optional aEndID As Integer = -1, Optional bAddTagIndices As Boolean = False)
            If aVectors Is Nothing Then Return
            Dim bTags As Boolean = aTag IsNot Nothing
            Dim bFlags As Boolean = aFlag IsNot Nothing
            If Not bTags And Not bFlags Then Return
            If aVectors.Count <= 0 Then Return
            Dim aStr As String = String.Empty
            If bTags Then aStr = aTag.ToString
            Dim bStr As String = String.Empty
            If bFlags Then bStr = bFlags.ToString


            Dim si As Integer
            Dim ei As Integer
            Dim bTestTag As Boolean = aSearchTag IsNot Nothing
            Dim bTestFlag As Boolean = aSearchFlag IsNot Nothing
            Dim aTg As String = String.Empty
            Dim aFg As String = String.Empty

            Dim tlist As New List(Of String)
            Dim flist As New List(Of String)
            If bTestTag Then
                aTg = aSearchTag.Trim()
            End If
            If bTestFlag Then
                aFg = aSearchFlag.Trim()
            End If
            If Not dxfUtils.LoopIndices(aVectors.Count, aStartID, aEndID, si, ei) Then Return

            For i As Integer = si To ei
                Dim aMem As dxfVector = aVectors(i - 1)
                If aMem Is Nothing Then Continue For
                If bTestTag Or bTestFlag Then
                    If bTestTag Then
                        If Not String.Compare(aMem.Tag, aTg, True) = 0 Then Continue For
                    End If
                    If bTestFlag Then
                        If Not String.Compare(aMem.Flag, aFg, True) = 0 Then Continue For
                    End If
                End If
                If bTags Then
                    If bAddTagIndices Then aMem.Tag = $"{aStr}{ i}" Else aMem.Tag = aStr
                End If
                If bFlags Then aMem.Flag = bStr

            Next i
        End Sub

        ''' <summary>
        ''' returns true if there is a one for one equality of the members of A to B.
        ''' </summary>
        ''' <remarks>Is not dependant on order</remarks>
        ''' <param name="aVectors"></param>
        ''' <param name="bVectors"></param>
        ''' <param name="aPrecis"></param>
        ''' <param name="bIgnoreZ"></param>
        ''' <returns></returns>
        Public Shared Function Match(aVectors As IEnumerable(Of iVector), bVectors As IEnumerable(Of iVector), Optional aPrecis As Integer = 3, Optional bIgnoreZ As Boolean = True) As Boolean
            If aVectors Is Nothing Or bVectors Is Nothing Then Return False
            If aVectors.Count <> bVectors.Count Then Return False
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Dim vcol1 As List(Of iVector) = aVectors.ToList
            Dim vcol2 As List(Of iVector) = bVectors.ToList
            Dim idx As Integer
            For Each v1 In vcol1
                If bIgnoreZ Then
                    idx = vcol2.FindIndex(Function(mem) Math.Round(mem.X, aPrecis) = Math.Round(v1.X, aPrecis) And Math.Round(mem.Y, aPrecis) = Math.Round(v1.Y, aPrecis))
                Else
                    idx = vcol2.FindIndex(Function(mem) Math.Round(mem.X, aPrecis) = Math.Round(v1.X, aPrecis) And Math.Round(mem.Y, aPrecis) = Math.Round(v1.Y, aPrecis) And Math.Round(mem.Z, aPrecis) = Math.Round(v1.Z, aPrecis))
                End If
                If idx < 0 Then
                    Return False
                Else
                    vcol2.RemoveAt(idx)
                End If
            Next
            Return True
        End Function

        ''' <summary>
        ''' returns True if a vector with coordinates match those of the passed vector is in the collection
        ''' </summary>
        ''' <param name="aVectors">the vectors to search</param>
        ''' <param name="aVector"></param>
        ''' <param name="aPrecis">the precision to use for the comparison</param>
        ''' <returns></returns>
        Friend Shared Function ContainsVector(aVectors As IEnumerable(Of iVector), aVector As iVector, Optional aPrecis As Integer = 4) As Boolean
            Dim rExistingIndex As Integer = -1
            Return dxfVectors.ContainsVector(aVectors, aVector, rExistingIndex, aPrecis)

        End Function

        ''' <summary>
        ''' returns the requested ordinates in a comma delimited string
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="OrdToReturn">the ordinate to return (X,Y or Z)</param>
        ''' <param name="bUniqueOnly">flag to return only the unique set</param>
        ''' <param name="aPrecis">the precis to use</param>
        ''' <returns></returns>
        Public Shared Function GetOrdinateList(aVectors As IEnumerable(Of iVector), Optional OrdToReturn As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X, Optional bUniqueOnly As Boolean = False, Optional aPrecis As Integer = 4, Optional aPlane As dxfPlane = Nothing) As String
            If aVectors Is Nothing Then Return String.Empty
            Dim _rVal As String = String.Empty
            Dim ord As String = String.Empty
            Dim fmat As String = "0.0"
            Dim planar As Boolean = Not dxfPlane.IsNull(aPlane)
            Dim plane As TPLANE = New TPLANE(aPlane)
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If aPrecis > 1 Then fmat += New String("#", aPrecis - 1)
            For Each vector As iVector In aVectors
                Dim v As New TVECTOR(vector)
                If planar Then v = v.WithRespectTo(plane)
                If OrdToReturn = dxxOrdinateDescriptors.Y Then
                    ord = String.Format(Math.Round(v.Y, aPrecis), fmat)
                ElseIf OrdToReturn = dxxOrdinateDescriptors.Z Then
                    ord = String.Format(Math.Round(v.Z, aPrecis), fmat)
                Else
                    ord = String.Format(Math.Round(v.X, aPrecis), fmat)
                End If
                TLISTS.Add(_rVal, ord, bAllowDuplicates:=Not bUniqueOnly)
            Next


            Return _rVal
        End Function


        ''' <summary>
        ''' used to query the collection about the ordinates of the vectors in the current collection
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="aOrdType"></param>
        ''' <param name="bUniqueValues"></param>
        ''' <param name="aPrecision"></param>
        ''' <param name="aPlane"></param>
        ''' <returns></returns>
        Public Shared Function GetOrdinates(aVectors As IEnumerable(Of iVector), aOrdType As dxxOrdinateDescriptors, Optional bUniqueValues As Boolean = True, Optional aPrecision As Integer = -1, Optional aPlane As dxfPlane = Nothing) As List(Of Double)
            If aVectors Is Nothing Then Return New List(Of Double)

            Dim _rVal As New List(Of Double)
            Dim oval As Double
            Dim aPl As TPLANE

            If aPrecision >= 0 Then aPrecision = TVALUES.LimitedValue(aPrecision, 0, 15)
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            For Each vector As iVector In aVectors

                Dim v1 As TVECTOR = New TVECTOR(vector)
                If Not dxfPlane.IsNull(aPlane) Then v1 = v1.WithRespectTo(aPl)

                If aOrdType = dxxOrdinateDescriptors.Z Then
                    oval = v1.Z
                ElseIf aOrdType = dxxOrdinateDescriptors.Y Then
                    oval = v1.Y
                Else
                    oval = v1.X
                End If

                If aPrecision > 0 Then oval = Math.Round(oval, aPrecision)


                '++++++++++++++++++++++++
                If bUniqueValues Then
                    If Not _rVal.Contains(oval) Then _rVal.Add(oval)

                Else
                    _rVal.Add(oval)

                End If


            Next
            Return _rVal

        End Function

        ''' <summary>
        ''' used to query the collection about the ordinates of the vectors in the passed collection
        ''' </summary>
        ''' <param name="aVectors">the subject vectors</param>
        ''' <param name="rXOrds">returns the X ordinates referred to by at least one of the vectors in the collection</param>
        ''' <param name="rYOrds">returns the Y ordinates referred to by at least one of the vectors in the collection</param>
        ''' <param name="rZOrds">returns the Z ordinates referred to by at least one of the vectors in the collection</param>
        ''' <param name="aPlane"> a Plane that if passsed, he returned values will be with respect to the passed plane.</param>
        ''' <param name="aPrecision">a precision to round the returned values to</param>
        ''' <param name="bUniqueValues">flag to return only the unique ordinates or all of them</param>
        Public Shared Sub GetOrdinates(aVectors As IEnumerable(Of iVector), ByRef rXOrds As List(Of Double), ByRef rYOrds As List(Of Double), ByRef rZOrds As List(Of Double), Optional aPlane As dxfPlane = Nothing, Optional aPrecision As Integer = -1, Optional bUniqueValues As Boolean = True)


            Dim aPl As TPLANE
            'initialize
            If rXOrds Is Nothing Then rXOrds = New List(Of Double)
            If rYOrds Is Nothing Then rYOrds = New List(Of Double)
            If rZOrds Is Nothing Then rZOrds = New List(Of Double)

            If aVectors Is Nothing Then Return

            If aPrecision >= 0 Then aPrecision = TVALUES.LimitedValue(aPrecision, 0, 15)
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = TPLANE.World
            For Each vector As iVector In aVectors
                Dim v1 As TVECTOR = New TVECTOR(vector)

                If Not dxfPlane.IsNull(aPlane) Then v1 = v1.WithRespectTo(aPl)
                Dim xval As Double = v1.X
                Dim yval As Double = v1.Y
                Dim zVal As Double = v1.Z
                If aPrecision >= 0 Then

                    xval = Math.Round(xval, aPrecision)
                    yval = Math.Round(yval, aPrecision)
                    zVal = Math.Round(zVal, aPrecision)
                End If
                '++++++++++++++++++++++++
                If bUniqueValues Then
                    If Not rXOrds.Contains(xval) Then rXOrds.Add(xval)
                    If Not rYOrds.Contains(yval) Then rYOrds.Add(yval)
                    If Not rZOrds.Contains(zVal) Then rZOrds.Add(zVal)
                Else
                    rXOrds.Add(xval)
                    rYOrds.Add(yval)
                    rZOrds.Add(zVal)

                End If


            Next
        End Sub

        Friend Shared Function ConvertToTVECTORS(aPtCol As IEnumerable(Of iVector)) As List(Of TVECTOR)
            Dim _rVal As New List(Of TVECTOR)
            If aPtCol Is Nothing Then Return _rVal
            For Each v As iVector In aPtCol
                _rVal.Add(New TVECTOR(v))
            Next
            Return _rVal
        End Function

        ''' <summary>
        ''' returns true of the two collections contain the same number of points the members are equal within the precision  
        ''' </summary>
        ''' '''<remarks> the order does not mattter unless orderwise is true</remarks>
        ''' <param name="aPtCol">the first comparitor</param>
        ''' <param name="bPtCol">the second comparitor</param>
        ''' <param name="aPrecis">the precision to apply</param>
        ''' <param name="bOrderWise">if true, the memebrs must be equal and in the same order</param>
        ''' <returns></returns>
        Public Shared Function IsEqual(aPtCol As IEnumerable(Of iVector), bPtCol As IEnumerable(Of iVector), Optional aPrecis As Integer = 4, Optional bOrderWise As Boolean = False) As Boolean

            Dim c1 As List(Of TVECTOR) = dxfVectors.ConvertToTVECTORS(aPtCol)
            Dim c2 As List(Of TVECTOR) = dxfVectors.ConvertToTVECTORS(bPtCol)

            If c1.Count <> c2.Count Then Return False

            If Not bOrderWise Then
                For Each v As TVECTOR In c1
                    If c2.RemoveAll(Function(x) x.Equals(v, aPrecis)) = 0 Then Return False
                Next
                Return True
            Else
                For i As Integer = 1 To c1.Count
                    If Not c1(i - 1).Equals(c2(i - 1), aPrecis) Then Return False
                Next
                Return True
            End If

        End Function

        Public Shared Function VectorAngle(aVector As iVector, bVector As iVector, ByRef ioNormal As dxfDirection, Optional aPrecis As Integer = -1) As Double

            '#1the from vector
            '#2the to vector
            '#3the normal (cross product of the two passed vectors)
            '^the angle between the two passed vectors in degrees
            '~if the known normal is passed then the counter clockwise angle between the vectors is returned (0 to 360)
            '~otherwise the angle is returned (0 to 180) and the normal is returned
            Return TVECTOR.VectorAngle(New TVECTOR(aVector), New TVECTOR(bVector), New TVECTOR(ioNormal), aPrecis)

        End Function
#End Region 'Shared Methods
    End Class
End Namespace
