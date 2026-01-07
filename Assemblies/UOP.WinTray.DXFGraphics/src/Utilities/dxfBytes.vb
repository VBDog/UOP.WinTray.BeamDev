

Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfBytes
        Friend Shared Function ToOSCCode(aByte As Byte) As String
            Dim _rVal As String

            Dim neg As Boolean
            If aByte > 128 Then
                neg = True
                aByte -= 128
            End If
            Dim sVal As Double = aByte / 16
            Dim whol As Integer = Fix(sVal)
            Dim frac As Double = sVal - whol
            If neg Then
                _rVal = "-0"
            Else
                _rVal = "0"
            End If
            _rVal += whol & Fix(frac * 16)
            Return _rVal
        End Function
        Friend Shared Function CommaString(aBytes() As Byte, Optional aMaxIndex As Long = -1, Optional aMaxLength As Long = 0, Optional bPadToThree As Boolean = False) As String
            Dim _rVal As String = String.Empty
            If aBytes.Count <= 0 Then Return _rVal

            For i As Integer = 0 To aBytes.Count - 1
                Dim b1 As Byte = aBytes(i)
                Dim txt As String = b1.ToString()
                If bPadToThree Then
                    Do While txt.Length < 3
                        txt = $"0{txt}"
                    Loop
                End If
                TLISTS.Add(_rVal, txt, bAllowDuplicates:=True)
                If aMaxIndex > 0 Then
                    If i + 1 >= aMaxIndex Then Exit For
                End If
            Next i
            If aMaxLength > 0 Then
                If _rVal.Length > aMaxLength Then
                    Dim txt As String = _rVal
                    _rVal = ""
                    Do While txt.Length > aMaxLength
                        If _rVal <> "" Then _rVal += vbLf
                        _rVal += txt.Substring(0, aMaxLength)
                        txt = Right(txt, txt.Length - aMaxLength)
                    Loop
                    If txt <> "" Then _rVal += vbLf & txt
                End If
            End If
            Return _rVal
        End Function

        Friend Shared Function FindEndOfPathCommands(aBytes() As Byte) As Integer
            Dim _rVal As Integer = -1
            Try

                Dim j As Integer
                Dim k As Integer
                Dim n As Integer

                Dim lb As Integer = 0
                Dim ub As Integer = aBytes.Length - 1

                For i As Integer = lb To ub
                    Dim p0 As Byte = aBytes(i)
                    If p0 = 0 Then
                        _rVal = i
                        Exit For
                    End If
                    Dim skip As Integer = 0
                    Dim bCmd As Boolean = p0 > 0 And p0 < 14
                    If bCmd Then
                        Select Case p0
                            Case 3, 4
                                'multiply or divide
                                skip = 1
                            Case 7
                                'subshape
                                skip = 2
                            Case 8
                                'X-Y Displacement
                                skip = 2
                            Case 9
                                'multiple X-Y Displacement
                                skip = 0
                                j = i + 1
                                k = i + 2
                                Do While j <= ub And k <= ub
                                    skip += 2
                                    If aBytes(j) = 0 And aBytes(k) = 0 Then
                                        Exit Do
                                    End If
                                    j = k + 1
                                    k = j + 1
                                Loop
                            Case 10
                                'Octant Arc
                                skip = 2
                            Case 11
                                'Fractional Arc
                                skip = 5
                            Case 12
                                'Bulge Arc
                                skip = 3
                            Case 13
                                'multi -Bulge Arc
                                j = i + 1
                                k = i + 2
                                n = i + 3
                                Do While j <= ub And k <= ub And n <= ub
                                    skip += 3
                                    If aBytes(j) = 0 And aBytes(k) = 0 Then
                                        skip -= 1
                                        Exit Do
                                    Else
                                        j = n + 1
                                        k = j + 1
                                        n = k + 1
                                    End If
                                Loop
                        End Select
                        If skip > 0 Then
                            i += skip
                            If i + 1 > ub Then Exit For
                        End If
                    End If
                    'Application.DoEvents()
                Next i
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function

        Friend Shared Function GetFirst(ByRef aBytes() As Byte, aCnt As Integer, Optional bDontRemove As Boolean = False) As Byte()
            Dim rRemains As Integer = 0
            Return dxfBytes.GetFirst(aBytes, aCnt, bDontRemove, rRemains)
        End Function

        Friend Shared Function GetFirst(ByRef aBytes() As Byte, aCnt As Integer, bDontRemove As Boolean, ByRef rRemains As Integer) As Byte()
            Dim rBytes() As Byte
            ReDim rBytes(0)
            rRemains = 0
            If aCnt <= 0 Then Return rBytes
            Dim remBytes(0) As Byte
            Dim ub As Integer = aBytes.Length - 1
            Dim lb As Integer = 0
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim aTot As Integer = aBytes.Length


            If aCnt > aTot Then aCnt = aTot
            rRemains = aTot
            If aCnt = aTot Then
                rBytes = aBytes
                If Not bDontRemove Then
                    ReDim aBytes(0)
                    rRemains = 0
                Else
                    rRemains = aTot
                End If
                Return rBytes
            Else
                If Not bDontRemove Then
                    rRemains = aTot - aCnt
                End If
            End If
            ReDim rBytes(0 To aCnt - 1)
            If Not bDontRemove Then ReDim remBytes(0 To (aTot - aCnt) - 1)
            j = 0
            k = 0
            For i = 0 To ub
                If i + 1 <= aCnt Then
                    rBytes(j) = aBytes(i)
                    j += 1
                Else
                    If bDontRemove Then Exit For
                    remBytes(k) = aBytes(i)
                    k += 1
                End If
            Next i
            '    Debug.Print dxfBytes.CommaString(aBytes)
            '     Debug.Print dxfBytes.CommaString(rBytes)
            '     Debug.Print dxfBytes.CommaString(remBytes)
            If Not bDontRemove Then aBytes = remBytes
            Return rBytes

        End Function


        Friend Shared Function GetIndex(aBytes() As Byte, aByte As Byte, Optional aOccur As Integer = 0) As Integer
            Dim _rVal As Integer = -1

            Dim cnt As Integer
            aOccur = Math.Abs(aOccur)
            If aOccur <= 0 Then aOccur = 1

            For i As Integer = 0 To aBytes.Length - 1
                If aBytes(i) = aByte Then
                    cnt += 1
                    If cnt = aOccur Then
                        _rVal = i
                        Exit For
                    End If
                End If
                'Application.DoEvents()
            Next i
            Return _rVal

        End Function

        Friend Shared Sub RemoveLeadBytes(ByRef aBytes() As Byte, aLeadByte As Byte, ByRef rRemoved As Integer)
            Dim rRemains As Integer = 0
            dxfBytes.RemoveLeadBytes(aBytes, aLeadByte, rRemoved, rRemains)
        End Sub
        Friend Shared Sub RemoveLeadBytes(ByRef aBytes() As Byte, aLeadByte As Byte, ByRef rRemoved As Integer, ByRef rRemains As Integer)
            Dim rBytes() As Byte
            ReDim rBytes(0)
            Dim i As Integer
            Dim b1 As Byte
            Dim lb As Integer
            Dim ub As Integer
            Dim bDone As Boolean
            Dim bKeep As Boolean
            rRemains = 0
            rRemoved = 0
            lb = 0
            ub = aBytes.Length - 1
            For i = lb To ub
                b1 = aBytes(i)
                If i = lb And b1 <> aLeadByte Then
                    rRemains = ub - lb + 1
                    rBytes = aBytes
                    Exit For
                End If
                bKeep = True
                If Not bDone Then
                    If b1 <> aLeadByte Then
                        bDone = True
                    Else
                        bKeep = False
                    End If
                End If
                If bKeep Then
                    rRemains += 1
                    System.Array.Resize(rBytes, rRemains)
                    rBytes(rRemains - 1) = b1
                Else
                    rRemoved += 1
                End If
            Next i
            aBytes = rBytes
        End Sub


        Friend Shared Function RemoveToByte(ByRef aBytes() As Byte, aLeadByte As Byte, Optional aReturnLead As Boolean = False, Optional aOccur As Integer = 0) As Byte()
            Dim rRemoved As Integer = 0
            Dim rRemains As Integer = 0
            Return RemoveToByte(aBytes, aLeadByte, aReturnLead, aOccur, rRemoved, rRemains)
        End Function
        Friend Shared Function RemoveToByte(ByRef aBytes() As Byte, aLeadByte As Byte, aReturnLead As Boolean, aOccur As Integer, ByRef rRemoved As Integer) As Byte()
            Dim rRemains As Integer = 0
            Return RemoveToByte(aBytes, aLeadByte, aReturnLead, aOccur, rRemoved, rRemains)
        End Function
        Friend Shared Function RemoveToByte(ByRef aBytes() As Byte, aLeadByte As Byte, aReturnLead As Boolean, aOccur As Integer, ByRef rRemoved As Integer, ByRef rRemains As Integer) As Byte()
            rRemains = 0
            rRemoved = 0
            Dim rBytes() As Byte
            ReDim rBytes(-1)
            If aBytes.Length <= 0 Then Return rBytes

            Dim bDone As Boolean
            Dim cnt As Integer
            Dim cnt2 As Integer
            Dim nBytes() As Byte
            Dim j As Integer
            If aOccur <= 0 Then aOccur = 1

            ReDim nBytes(-1)

            rRemains = 0
            rRemoved = 0
            bDone = False
            For i As Integer = 0 To aBytes.Length - 1
                Dim b1 As Byte = aBytes(i)
                If Not bDone Then
                    If b1 = aLeadByte Then
                        cnt += 1
                        If cnt >= aOccur Then
                            bDone = True
                            j = i - 1
                            If aReturnLead Then
                                j = i
                                rRemoved += 1
                            End If
                        Else
                            rRemoved += 1
                        End If
                    Else
                        rRemoved += 1
                    End If
                Else
                    rRemoved += 1
                End If
                'Application.DoEvents()
            Next i
            If rRemoved > 0 Then
                cnt = 0
                cnt2 = 0
                For i = 0 To aBytes.Length - 1
                    Dim b1 As Byte = aBytes(i)
                    If i <= j Then
                        cnt += 1
                        System.Array.Resize(rBytes, cnt)
                        rBytes(cnt - 1) = b1
                    Else
                        cnt2 += 1
                        System.Array.Resize(nBytes, cnt2)
                        nBytes(cnt2 - 1) = b1
                    End If
                    'Application.DoEvents()
                Next i
                aBytes = nBytes

            End If
            Return rBytes
        End Function

        Friend Shared Sub Print(aBytes() As Byte, Optional aByteCount As Integer = 10, Optional aMaxByte As Integer = -1, Optional bBreakAtZeros As Boolean = False, Optional bLettersAsAscii As Boolean = False, Optional aFilePath As String = "")
            'On Error Resume Next

            If Not String.IsNullOrWhiteSpace(aFilePath) Then
                aFilePath = aFilePath.Trim()
                If Not IO.File.Exists(aFilePath) Then aFilePath = ""
            Else
                aFilePath = ""
            End If


            Dim j As Integer = 0
            Dim txt As String = String.Empty
            Dim txt1 As String

            Dim bLtr As Boolean
            Dim fout As IO.StreamWriter = IIf(aFilePath = "", Nothing, New IO.StreamWriter(aFilePath))
            Dim FNum As Integer = IIf(fout Is Nothing, 0, 1)

            Try
                aByteCount = TVALUES.LimitedValue(aByteCount, 1, 100)

                For i As Integer = 0 To aBytes.Count - 1
                    Dim b1 As Byte = aBytes(i)
                    If bLettersAsAscii Then
                        bLtr = (b1 >= 65 And b1 <= 90) Or (b1 >= 97 And b1 <= 122)
                    End If
                    If bLtr Then
                        txt1 = Char.ConvertFromUtf32(b1)
                    Else
                        txt1 = b1.ToString()
                        Do While txt1.Length < 3
                            txt1 = $"0{txt1}"
                        Loop
                    End If
                    TLISTS.Add(txt, txt1, bAllowDuplicates:=True)
                    j += 1
                    If Not bBreakAtZeros Then
                        If j = aByteCount Then
                            If FNum = 0 Then
                                System.Diagnostics.Debug.WriteLine(txt)
                            Else
                                fout.WriteLine(txt)
                            End If
                            j = 0
                            txt = ""
                        End If
                    Else
                        If b1 = 0 Then
                            If FNum = 0 Then
                                System.Diagnostics.Debug.WriteLine(txt)
                            Else
                                fout.WriteLine(txt)
                            End If
                            j = 0
                            txt = ""
                        End If
                    End If
                    If aMaxByte > 0 Then
                        If i + 1 >= aMaxByte Then Exit For
                    End If
                Next i
                If txt <> "" Then
                    If FNum = 0 Then
                        System.Diagnostics.Debug.WriteLine(txt)
                    Else
                        fout.WriteLine(txt)
                    End If
                End If
            Catch ex As Exception
            Finally
                If fout IsNot Nothing Then
                    fout.Close()
                    fout.Dispose()
                End If
            End Try
        End Sub

        Friend Shared Function RemoveToLeadBytes(ByRef aBytes() As Byte, aLeadBytes As String, ByRef rRemoved As Integer) As Byte()
            Dim rRemains As Integer
            Return RemoveToLeadBytes(aBytes, aLeadBytes, rRemoved, rRemains)
        End Function
        Friend Shared Function RemoveToLeadBytes(ByRef aBytes() As Byte, aLeadBytes As String, ByRef rRemoved As Integer, ByRef rRemains As Integer) As Byte()
            Dim rBytes() As Byte
            ReDim rBytes(0)
            Dim retBytes() As Byte
            Dim rmBytes() As Byte
            ReDim retBytes(0)
            ReDim rmBytes(0)
            rRemains = 0
            rRemoved = 0
            If aBytes Is Nothing Then Return rBytes
            If aBytes.Length <= 0 Then Return rBytes
            If String.IsNullOrWhiteSpace(aLeadBytes) Then Return rBytes

            Dim sVals() As String = aLeadBytes.Trim().Split(",")
            Dim lVals As New List(Of Byte)

            For j As Integer = 0 To sVals.Length - 1
                lVals.Add(TVALUES.ToByte(sVals(j)))
            Next j
            Dim bDone As Boolean = False

            For Each byt As Byte In aBytes
                Dim bKeep As Boolean = True
                If Not bDone Then
                    bDone = lVals.Contains(byt)
                    If Not bDone Then bKeep = False
                End If

                If bKeep Then
                    rRemains += 1
                    System.Array.Resize(retBytes, rRemains)
                    retBytes(rRemains - 1) = byt
                Else
                    rRemoved += 1
                    System.Array.Resize(rmBytes, rRemoved)
                    rmBytes(rRemoved - 1) = byt
                End If
            Next

            aBytes = retBytes
            rBytes = rmBytes
            Return rBytes
        End Function
        Friend Shared Function ToAscii(ByRef aBytes() As Byte, Optional aMaxIndex As Integer = -1, Optional bRegularCharsOnly As Boolean = False) As String
            Dim _rVal As String = String.Empty
            Dim i As Integer
            For i = 0 To aBytes.Length - 1
                If Not bRegularCharsOnly Then
                    _rVal += ChrW(aBytes(i))
                Else
                    If aBytes(i) >= 32 And aBytes(i) <= 126 Then
                        _rVal += ChrW(aBytes(i))
                    End If
                End If
                If aMaxIndex > 0 Then
                    If i + 1 >= aMaxIndex Then Exit For
                End If
            Next i
            Return _rVal
        End Function
        Friend Shared Function ToPathCommands(ByRef aBytes() As Byte, ByRef rCommands As TVALUES, ByRef rCount As Integer, bReturnString As Boolean, aMaxIndex As Integer, ByRef rSubShapes As Boolean) As String
            Dim _rVal As String = String.Empty
            Try
                Dim i As Integer
                Dim j As Integer
                Dim k As Integer
                Dim skip As Integer
                Dim p0 As Byte
                Dim P1 As Byte
                Dim P2 As Byte
                Dim p3 As Byte
                Dim p4 As Byte
                Dim p5 As Byte
                Dim bCode As Boolean
                Dim pBytes() As Byte
                Dim bcnt As Integer
                Dim cmd As String
                Dim dX As Integer
                Dim dY As Integer
                Dim idx As Integer
                Dim sCmds(0 To 20000) As String
                Dim allCmds As New TVALUES
                Dim blg As Integer
                Dim lb As Integer
                Dim ub As Integer
                pBytes = aBytes
                ub = pBytes.Length - 1
                lb = 0
                allCmds = New TVALUES
                rCommands = allCmds
                rSubShapes = False
                bcnt = ub - lb + 1
                cmd = ""
                idx = 1
                bCode = False
                rCount = 0
                If aMaxIndex > lb And aMaxIndex < ub Then ub = aMaxIndex
                For idx = lb To ub
                    p0 = pBytes(idx)
                    If idx + 1 <= ub Then P1 = pBytes(idx + 1)
                    If idx + 2 <= ub Then P2 = pBytes(idx + 2)
                    If idx + 3 <= ub Then p3 = pBytes(idx + 3)
                    If idx + 4 <= ub Then p4 = pBytes(idx + 4)
                    If idx + 5 <= ub Then p5 = pBytes(idx + 5)
                    bcnt -= 1
                    bCode = p0 >= 0 And p0 <= 14
                    cmd = ""
                    'Erase sCmds
                    If bCode Then
                        sCmds(0) = p0.ToString
                        Select Case sCmds(0)
                            Case "3", "4"
                                'multiply or divide
                                If idx + 1 <= ub Then
                                    idx += 1
                                    bcnt -= 1
                                    sCmds(1) = P1
                                Else
                                    Exit For
                                End If
                            Case "7"
                                'subshape
                                rSubShapes = True
                                If idx + 1 <= ub Then
                                    idx += 1
                                    '                    bcnt = bcnt - 2
                                    '
                                    sCmds(1) = ""
                                    sCmds(2) = P2
                                Else
                                    Exit For
                                End If
                            Case "8"
                                'X-Y Displacement
                                If idx + 2 <= ub Then
                                    idx += 2
                                    bcnt -= 2
                                    dX = P1
                                    dY = P2
                                    Do While (Math.Abs(dX)) > 128
                                        dX -= 256
                                    Loop
                                    Do While (Math.Abs(dY)) > 128
                                        dY -= 256
                                    Loop
                                    sCmds(1) = "(" & dX
                                    sCmds(2) = dY & ")"
                                Else
                                    Exit For
                                End If
                            Case "9"
                                'multiple X-Y Displacement
                                skip = 0
                                j = idx + 1
                                k = j + 1
                                Do While j <= ub And k <= ub
                                    skip += 2
                                    If pBytes(j) = 0 And pBytes(k) = 0 Then
                                        Exit Do
                                    End If
                                    j = k + 1
                                    k = j + 1
                                Loop
                                k = 1
                                For i = 1 To skip
                                    If idx + 1 <= ub Then
                                        idx += 1
                                        P1 = pBytes(idx)
                                        dX = P1
                                        Do While (Math.Abs(dX)) > 128
                                            dX -= 256
                                        Loop
                                        If k = 1 Then
                                            sCmds(i) = "(" & dX
                                        Else
                                            sCmds(i) = dX & ")"
                                        End If
                                    Else
                                        Exit For
                                    End If
                                    k = -k
                                    'Application.DoEvents()
                                Next i
                            Case "10"
                                'Octant Arc
                                If idx + 2 <= ub Then
                                    idx += 2
                                    bcnt -= 2
                                    sCmds(1) = "(" & P1
                                    sCmds(2) = dxfBytes.ToOSCCode(P2) & ")"
                                Else
                                    Exit For
                                End If
                            Case "11"
                                'Octant Arc
                                If idx + 5 <= ub Then
                                    idx += 5
                                    bcnt -= 5
                                    sCmds(1) = "(" & P1
                                    sCmds(2) = P2
                                    sCmds(3) = p3
                                    sCmds(4) = p4
                                    sCmds(5) = dxfBytes.ToOSCCode(p5) & ")"
                                Else
                                    Exit For
                                End If
                            Case "12"
                                'Bulge Arc
                                If idx + 3 <= ub Then
                                    idx += 3
                                    bcnt -= 3
                                    dX = P1
                                    dY = P2
                                    Do While (Math.Abs(dX)) > 128
                                        dX -= 256
                                    Loop
                                    Do While (Math.Abs(dY)) > 128
                                        dY -= 256
                                    Loop
                                    blg = p3
                                    If blg > 127 Then
                                        blg -= 127
                                        blg = -(127 - blg + 2)
                                    End If
                                    sCmds(1) = "(" & dX
                                    sCmds(2) = dY
                                    sCmds(3) = blg & ")"
                                Else
                                    Exit For
                                End If
                            Case "13"
                                'multi -Bulge Arc
                                If idx + 3 > ub Then Exit For
                                i = 1
                                j = 0
                                idx += 1
                                bcnt -= 1
                                Do While idx <= ub
                                    j += 1
                                    If j = 1 Then
                                        P1 = pBytes(idx)
                                        dX = P1
                                        Do While (Math.Abs(dX)) > 128
                                            dX -= 256
                                        Loop
                                        sCmds(i) = "(" & dX
                                        i += 1
                                    ElseIf j = 2 Then
                                        P2 = pBytes(idx)
                                        dY = P2
                                        Do While (Math.Abs(dY)) > 128
                                            dY -= 256
                                        Loop
                                        If dX = 0 And dY = 0 Then
                                            sCmds(i) = dY & ")"
                                            Exit Do
                                        Else
                                            sCmds(i) = dY
                                            i += 1
                                        End If
                                    ElseIf j = 3 Then
                                        p3 = pBytes(idx)
                                        blg = p3
                                        If blg > 127 Then
                                            blg -= 127
                                            blg = -(127 - blg + 2)
                                        End If
                                        sCmds(i) = blg & ")"
                                        i += 1
                                        j = 0
                                    End If
                                    idx += 1
                                    bcnt -= 1
                                Loop
                        End Select
                    Else
                        sCmds(0) = "0" & Hex(p0)
                    End If
                    If sCmds(0) <> "" Then
                        For i = 0 To 20000
                            If sCmds(i) <> "" Then
                                allCmds.Add(sCmds(i))
                            Else
                                Exit For
                            End If
                            'Application.DoEvents()
                        Next i
                    End If
                    If idx + 1 > ub Then Exit For
                    'Application.DoEvents()
                Next idx
                rCount = allCmds.Count
                rCommands = allCmds
                If bReturnString Then _rVal = allCmds.ToList()
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
    End Class


End Namespace
