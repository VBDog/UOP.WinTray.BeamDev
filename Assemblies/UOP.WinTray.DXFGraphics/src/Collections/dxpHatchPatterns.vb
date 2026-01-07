
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Friend Class dxpHatchPatterns
#Region "Members"
        Private _Struc As THATCHPATTERNS
        Friend Sub New()
            _Struc = New THATCHPATTERNS
        End Sub
#End Region 'Members
#Region "Constructors"
#End Region 'Constructors
#Region "Properties"
        Friend ReadOnly Property Count As Integer
            Get
                Count = _Struc.Count
                Return Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Sub Add(aPattern As THATCHPATTERN, Optional bOverrideExisting As Boolean = False)
            If _Struc.Count >= Integer.MaxValue Then Return
            If aPattern.Name = "" Then Return
            If aPattern.HatchLineCnt <= 0 Then Return
            Dim idx As Integer
            GetByName(aPattern.Name, idx)
            If idx < 0 Then

                _Struc.Add(aPattern)
            Else
                If Not bOverrideExisting Then Return
                _Struc.Members(idx) = aPattern
            End If
        End Sub
        Friend Function GetByName(aName As String, ByRef rIndex As Integer) As THATCHPATTERN
            Dim _rVal As New THATCHPATTERN
            rIndex = -1
            aName = Trim(aName)
            If _Struc.Count <= 0 Or aName = "" Then Return Nothing
            Dim i As Integer
            Dim aMem As New THATCHPATTERN
            For i = 1 To _Struc.Count
                aMem = _Struc.Members(i - 1)
                If String.Compare(aMem.Name, aName, True) = 0 Then
                    _rVal = aMem
                    rIndex = i - 1
                    Return _rVal
                    Exit For
                End If
            Next i
            Return Nothing
        End Function
        Friend Function Item(aIndex As Object) As THATCHPATTERN
            Dim _rVal As New THATCHPATTERN
            Dim idx As Integer
            If Not TVALUES.IsNumber(aIndex) Then Return _rVal
            idx = TVALUES.To_INT(aIndex)
            If idx > 0 And idx <= _Struc.Count Then _rVal = _Struc.Members(idx - 1)
            Return _rVal
        End Function
        Friend Function LoadFromFile(aFileName As String, Optional bOverrideExisting As Boolean = False) As Boolean
            If String.IsNullOrWhiteSpace(aFileName) Then Return False
            Dim _rVal As Boolean
            Dim aErr As String = String.Empty
            aFileName = aFileName.Trim
            If aFileName = "" Then Return False
            If Not IO.File.Exists(aFileName) Then Return False
            Dim F As IO.StreamReader = Nothing
            Dim aL As String

            Dim pLines As New List(Of String)
            Dim cStars As New List(Of String)
            Dim lPat() As String

            Dim j As Integer
            Dim k As Integer
            Dim n As Integer
            Dim m As Integer

            Try
                F = New IO.StreamReader(aFileName)
                Do
                    aL = F.ReadLine.Trim()
                    If aL.StartsWith(";") Then aL = ""
                    If aL <> "" Then
                        If aL.Contains(";") Then aL = dxfUtils.LeftOf(aL, ";")
                    End If
                    If aL <> "" Then
                        pLines.Add(aL)
                        If Left(aL, 1) = "*" Then cStars.Add(pLines.Count)
                    End If
                    If F.EndOfStream Then Exit Do
                Loop
                For i As Integer = 1 To cStars.Count
                    j = cStars.Item(i)
                    If i + 1 <= cStars.Count Then k = cStars.Item(i + 1) - 1 Else k = pLines.Count
                    If k > j Then 'at lest one line descritption
                        Dim patDat As New List(Of String)
                        Dim tHP = New THATCHPATTERN("")
                        tHP.HatchLineCnt = 0
                        For n = j To k
                            aL = pLines.Item(n)
                            If n = j Then
                                patDat.Add(pLines.Item(n))
                            Else
                                lPat = aL.Split(",")
                                If lPat.Length - 1 >= 4 Then patDat.Add(aL)
                            End If
                        Next n
                        tHP.HatchLineCnt = patDat.Count - 1
                        If tHP.HatchLineCnt > 0 Then 'has line desciptions
                            ReDim tHP.HatchLines(0 To tHP.HatchLineCnt - 1)
                            For n = 1 To patDat.Count
                                aL = patDat.Item(n)
                                lPat = aL.Split(",")
                                If n = 1 Then
                                    tHP.Name = Right(lPat(0), lPat(0).Length - 1)
                                    If lPat.Length - 1 > 0 Then tHP.Description = Trim(lPat(1))
                                Else
                                    Dim tHL As New THATCHLINE(0)

                                    Dim sDashes As New List(Of Double)
                                    For m = 0 To lPat.Length - 1
                                        lPat(m) = lPat(m).Trim()
                                        Select Case m
                                            Case 0
                                                If TVALUES.IsNumber(lPat(m)) Then tHL.Angle = TVALUES.To_DBL(lPat(m))
                                            Case 1
                                                If TVALUES.IsNumber(lPat(m)) Then tHL.OriginX = TVALUES.To_DBL(lPat(m))
                                            Case 2
                                                If TVALUES.IsNumber(lPat(m)) Then tHL.OriginY = TVALUES.To_DBL(lPat(m))
                                            Case 3
                                                If TVALUES.IsNumber(lPat(m)) Then tHL.DeltaX = TVALUES.To_DBL(lPat(m))
                                            Case 4
                                                If TVALUES.IsNumber(lPat(m)) Then tHL.DeltaY = TVALUES.To_DBL(lPat(m))
                                            Case Else
                                                If TVALUES.IsNumber(lPat(m)) Then
                                                    sDashes.Add(TVALUES.To_DBL(lPat(m)))
                                                End If
                                        End Select
                                    Next m
                                    tHL.DashCount = sDashes.Count
                                    If tHL.DashCount > 0 Then
                                        ReDim tHL.Dashes(0 To tHL.DashCount - 1)
                                        For m = 1 To sDashes.Count
                                            tHL.Dashes(m - 1) = sDashes.Item(m)
                                        Next m
                                    End If
                                    tHP.HatchLines(n - 2) = tHL
                                End If
                            Next n
                            'save the pattern
                            Add(tHP)
                        End If
                    End If
                Next i
                _rVal = True
                F.Close()
                F = Nothing
            Catch ex As Exception
                aErr = ex.Message
                If F IsNot Nothing Then F.Close()
                F = Nothing
                Throw New Exception("dxpHatchPatterns.LoadFromFile - " & aErr)
            End Try
            Return _rVal
        End Function
        Friend Function Remove(aIndex As Object) As THATCHPATTERN
            Dim _rVal As New THATCHPATTERN("")
            Dim idx As Integer
            If Not TVALUES.IsNumber(aIndex) Then Return New THATCHPATTERN("")
            idx = TVALUES.To_INT(aIndex)
            If idx > 0 And idx <= _Struc.Count Then
                Dim i As Integer
                Dim nStruc As New THATCHPATTERNS(_Struc)


                ReDim _Struc.Members(0)
                For i = 1 To nStruc.Count
                    If i <> idx Then

                        _Struc.Add(nStruc.Members(i - 1))
                    End If
                Next i
            End If
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxpHatchPatterns
End Namespace
