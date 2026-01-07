Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoPoints
        Inherits List(Of dxoPoint)
        Implements ICloneable
#Region "Constructors"

        Public Sub New()
            Clear()
        End Sub
        Public Sub New(aPointsToCopy As dxoPoints)
            Clear()
            If aPointsToCopy Is Nothing Then Return
            For Each mem As dxoPoint In aPointsToCopy
                Add(mem.Clone())
            Next
        End Sub
        Friend Sub New(aPointsToCopy As TPOINTS)
            Clear()
            For i As Integer = 1 To aPointsToCopy.Count
                Add(New dxoPoint(aPointsToCopy.Item(i)))
            Next
        End Sub
#End Region 'Constructors

#Region "Methods"

        Public Overrides Function ToString() As String
            Return $"dxoPoints[{ Count}]"
        End Function

        Public Function Clone() As dxoPoints
            Return New dxoPoints(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function

        Public Sub Append(aPoints As List(Of dxoPoint), Optional bAddClones As Boolean = False)
            If aPoints Is Nothing Then Return
            If Not bAddClones Then
                AddRange(aPoints)
                Return
            End If

            For Each pt As dxoPoint In aPoints
                Add(pt.Clone())
            Next
        End Sub

        Public Overloads Sub Add(Optional aX As Double = 0, Optional aY As Double = 0, Optional aCode As Byte? = Nothing)
            Add(New TPOINT(aX, aY), aCode)
        End Sub
        Friend Overloads Sub Add(aPoint As TPOINT, Optional aCode As Byte? = Nothing)
            Add(New dxoPoint(aPoint), aCode)
        End Sub

        Friend Overloads Sub Add(aPoint As dxoPoint, Optional aCode As Byte? = Nothing)
            If aPoint Is Nothing Then Return
            If aCode.HasValue Then
                aPoint.Code = aCode.Value
            End If
            MyBase.Add(aPoint)
        End Sub

        Friend Overloads Sub Add(aVector As TVECTOR, Optional aCode As Byte? = Nothing)
            Add(CType(aVector, dxoPoint), aCode)
        End Sub
        Friend Overloads Sub Add(aLine As TLINE)

            AddLine(CType(aLine.SPT, TPOINT), CType(aLine.EPT, TPOINT))
        End Sub
        Public Sub Print()
            Dim aStr As String
            Dim bStr As String
            Dim i As Integer = 0
            For Each P1 As dxoPoint In Me
                i += 1

                If P1.Code = dxxVertexStyles.MOVETO Then
                    aStr = dxfEnums.Description(dxxVertexStyles.MOVETO)
                ElseIf P1.Code = dxxVertexStyles.CLOSEFIGURE Then
                    aStr = dxfEnums.Description(dxxVertexStyles.CLOSEFIGURE)
                ElseIf P1.Code = dxxVertexStyles.LINETO Then
                    aStr = dxfEnums.Description(dxxVertexStyles.LINETO)
                ElseIf P1.Code = dxxVertexStyles.BEZIERTO Then
                    aStr = dxfEnums.Description(dxxVertexStyles.BEZIERTO)
                ElseIf P1.Code = dxxVertexStyles.PIXEL Then
                    aStr = dxfEnums.Description(dxxVertexStyles.PIXEL)
                Else
                    aStr = dxfEnums.Description(dxxVertexStyles.UNDEFINED)
                End If
                bStr = P1.Coords(4)
                bStr = bStr.Replace(") ", $",{ aStr})")
                Console.WriteLine($"{ i } - {bStr}")
            Next
        End Sub
        Public Sub Scale(aScaleX As Double, Optional aScaleY As Double? = Nothing)
            '#1the vectors to scale
            '#2the scale factor to apply
            '#3the center to scale with resect to
            '#4the y scale to apply
            '^moves the current coordinates of the vectors in the collection to a vector scaled with respect to the passed center
            Dim yScl As Double = aScaleX

            If aScaleY.HasValue Then yScl = aScaleY.Value
            For Each mem As dxoPoint In Me
                mem.Scale(aScaleX, yScl)
            Next


        End Sub
        Public Overloads Sub Remove(aIndex As Integer)

            If aIndex <= 0 Or aIndex > Count Then Return
            MyBase.RemoveAt(aIndex - 1)
        End Sub

        Public Function Bounds(aPlane As dxfPlane, Optional aShearAngle As Double = 0) As dxfRectangle
            Dim aPl As New TPLANE(aPlane)
            Return New dxfRectangle(ToPlaneVectorsV(aPl, aShearAngle).BoundingRectangle(aPl, Nothing, True))
        End Function

        Friend Function BoundsV(aPlane As TPLANE, Optional aShearAngle As Double = 0) As TPLANE
            Return ToPlaneVectorsV(aPlane, aShearAngle).BoundingRectangle(aPlane, Nothing, True)
        End Function

        Public Function ToPlaneVectors(aPlane As dxfPlane, Optional aShearAngle As Double = 0, Optional aRelativeToPlane As dxfPlane = Nothing, Optional aTag As String = "") As colDXFVectors
            Dim _rVal As New colDXFVectors
            Dim aPl As New TPLANE(aPlane)

            For Each mem As dxoPoint In Me
                Dim vpt As TVECTOR = aPl.Vector(mem.X, mem.Y, 0, aShearAngle)
                If aRelativeToPlane IsNot Nothing Then
                    vpt = vpt.WithRespectTo(aRelativeToPlane)
                End If
                _rVal.AddV(vpt, aCode:=mem.Code, aTag:=aTag)
            Next
            Return _rVal
        End Function

        Friend Function ToPlaneVectorsV(aPlane As TPLANE, Optional aShearAngle As Double = 0, Optional aRelativeToPlane As dxfPlane = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            For Each mem As dxoPoint In Me
                Dim vpt As TVECTOR = aPlane.Vector(mem.X, mem.Y, 0, aShearAngle)
                If aRelativeToPlane IsNot Nothing Then
                    vpt = vpt.WithRespectTo(aRelativeToPlane)
                End If

                _rVal.Add(vpt, aCode:=mem.Code)
            Next


            Return _rVal
        End Function

        Friend ReadOnly Property Limits As TLIMITS
            Get

                Dim _rVal As New TLIMITS()
                If Count <= 0 Then Return _rVal
                For i As Integer = 1 To Count
                    Dim pt As dxoPoint = MyBase.Item(i - 1)
                    If i = 1 Then
                        _rVal.Update(aLeft:=pt.X, aRight:=pt.X, aBottom:=pt.Y, aTop:=pt.Y)
                    Else
                        _rVal.Update(pt)
                    End If

                Next
                Return _rVal
            End Get
        End Property

        Friend Function ConvertToPath(aBasePlane As TPLANE, aDisplay As TDISPLAYVARS, aDomain As dxxDrawingDomains, bFilled As Boolean, Optional aShearAngle As Double = 0, Optional aRelativeToPlane As dxfPlane = Nothing, Optional aIdentifier As String = "") As TPATH


            If dxfPlane.IsNull(aRelativeToPlane) Then aRelativeToPlane = New dxfPlane(aBasePlane)

            Return New TPATH(aDomain, aDisplay, aRelativeToPlane, ToPlaneVectorsV(aBasePlane, aShearAngle, aRelativeToPlane)) With {.Linetype = dxfLinetypes.Continuous, .Filled = bFilled, .Color = aDisplay.Color, .Relative = aRelativeToPlane IsNot Nothing, .LayerName = aDisplay.LayerName, .Identifier = aIdentifier}

        End Function

        Public Sub SetCode(aIndex As Integer, aCode As Object)
            If aIndex > 0 And aIndex <= Count Then
                MyBase.Item(aIndex - 1).Code = TVALUES.ToByte(aCode)
            End If
        End Sub

        Friend Sub Append(bPoints As TPOINTS, Optional aStartID As Integer = 0)
            'On Error Resume Next

            If bPoints.Count <= 0 Then Return

            Dim sid As Integer
            If aStartID > 0 Then sid = aStartID
            If sid > bPoints.Count Then sid = bPoints.Count

            For i As Integer = sid To bPoints.Count
                Add(bPoints.Item(i))
            Next i

        End Sub

        Friend Sub Append(aVectors As TVECTORS, Optional aStartID As Integer = 1)
            'On Error Resume Next
            If aVectors.Count <= 0 Then Return

            Dim sid As Integer
            If aStartID > 0 Then sid = aStartID
            If sid > aVectors.Count Then sid = aVectors.Count
            For i As Integer = sid To aVectors.Count

                If i >= 1 And i <= aVectors.Count Then
                    Add(aVectors.Item(i))
                End If
            Next i
        End Sub


        Public Sub Print(Optional aTag As String = "")
            Console.WriteLine("")
            Console.WriteLine($"{aTag} dxoPoints[{ Count }]".Trim())
            For i As Integer = 0 To Count - 1
                Console.WriteLine($"   { i + 1} - { MyBase.Item(i)}")
            Next
        End Sub
        Public Sub Scale(Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing)
            Try
                Dim sX As Double = 1
                Dim sY As Double = 1

                If aXScale.HasValue Then sX = aXScale.Value
                If aYScale.HasValue Then sY = aYScale.Value
                If sX = 1 And sY = 1 Then Return
                For Each mem As dxoPoint In Me
                    mem.X *= sX
                    mem.Y *= sY
                Next
            Catch ex As Exception
            End Try
        End Sub
        Friend Sub AddTwo(aPoint As TPOINT, bPoint As TPOINT, Optional aCode As Byte? = Nothing, Optional bCode As Byte? = Nothing)
            Add(aPoint, aCode)
            Add(bPoint, bCode)
        End Sub
        Friend Sub AddLine(aLine As TLINE, Optional bTestLast As Boolean = False, Optional aPrecis As Integer = -1)
            AddLine(aLine.SPT, aLine.EPT, bTestLast, aPrecis)
        End Sub
        Friend Sub AddLine(P1 As TPOINT, P2 As TPOINT, Optional bTestLast As Boolean = False, Optional aPrecis As Integer = -1)
            If Not bTestLast Or Count <= 0 Then
                AddTwo(P1, P2, dxxVertexStyles.MOVETO, dxxVertexStyles.LINETO)
            Else
                If dxoPoint.Compare(MyBase.Last(), P1, aPrecis) Then
                    Add(New dxoPoint(P2), dxxVertexStyles.LINETO)
                Else
                    AddTwo(P1, P2, dxxVertexStyles.MOVETO, dxxVertexStyles.LINETO)
                End If
            End If
        End Sub
        Friend Sub AddLine(V1 As TVECTOR, V2 As TVECTOR, Optional bTestLast As Boolean = False, Optional aPrecis As Integer = -1)
            AddLine(CType(V1, TPOINT), CType(V2, TPOINT), bTestLast, aPrecis)
        End Sub
        Public Sub Translate(aXChange As Double, aYChange As Double)
            For Each mem As dxoPoint In Me
                mem.X += aXChange
                mem.Y += aYChange
            Next

        End Sub

        Friend Function First(aCount As Integer, Optional bRemove As Boolean = False) As dxoPoints
            Dim _rVal As New dxoPoints()
            '#1the number of vectors to return
            '#2flag to return copies
            '#3flag to remove the subset from the collection
            '^returns the first members of the collection up to the passed count
            '~i.e. pnts_First(4) returns the first 4 members
            If Count <= 0 Then Return _rVal
            If aCount > Count Then aCount = Count

            For i As Integer = 1 To aCount
                If i > Count Then Exit For
                _rVal.Add(MyBase.Item(i - 1))

            Next i

            If bRemove Then
                For Each mem As dxoPoint In _rVal
                    MyBase.Remove(mem)
                Next
            End If

            Return _rVal
        End Function

        Friend Function Structure_Get() As TPOINTS
            Dim _rVal As New TPOINTS(0)
            For Each mem As dxoPoint In Me
                _rVal.Add(mem.X, mem.Y, mem.Code)
            Next
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function FromAPIPoints(aPointArray() As System.Drawing.Point, aPointTypes() As Byte, Optional aXScaler As Double = 1, Optional aYScaler As Double = 1, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As dxoPoints
            Dim _rVal As New dxoPoints()
            Try
                Dim lb1 As Integer
                Dim ub1 As Integer
                Dim lb2 As Integer
                Dim ub2 As Integer
                Dim i As Integer
                Dim pAPI As New System.Drawing.Point
                Dim P1 As TPOINT
                lb1 = 0
                ub1 = aPointArray.Length - 1
                lb2 = 0
                ub2 = aPointTypes.Length - 1
                For i = lb1 To ub1
                    pAPI = aPointArray(i)
                    P1 = New TPOINT(pAPI.X * aXScaler + aXOffset, pAPI.Y * aYScaler + aYOffset, dxxVertexStyles.MOVETO)
                    If i >= lb2 And i <= ub2 Then P1.Code = aPointTypes(i)
                    _rVal.Add(P1)
                Next i
                Return _rVal
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine(ex.Message)
                Return _rVal
            End Try
        End Function
#End Region 'Shared Methods
#Region "Operators"
        Public Shared Widening Operator CType(aPoints As dxoPoints) As colDXFVectors
            Dim _rVal As New colDXFVectors()
            If aPoints Is Nothing Then Return _rVal
            For Each mem As dxoPoint In aPoints
                _rVal.Add(CType(mem, dxfVector))
            Next
            Return _rVal
        End Operator
#End Region 'Operators
    End Class

End Namespace
