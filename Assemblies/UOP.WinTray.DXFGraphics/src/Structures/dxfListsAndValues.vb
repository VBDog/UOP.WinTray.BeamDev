

Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures


#Region "Structures"
    Friend Structure TVALUES
        Implements ICloneable
#Region "Members"
        Public BaseValue As Object
        'Public Count As Integer
        Public Defined As Boolean
        Public Index As Integer
        Friend _Members() As Object
        Public Name As String
        Public NodePath As String
        Public MaintainList As Boolean
        Public ListValues As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            'init ---------------------------------
            BaseValue = Nothing
            ' Count = 0
            Defined = False
            Index = 0
            ReDim _Members(-1)
            Name = ""
            NodePath = ""
            MaintainList = False
            ListValues = ""
            'init ---------------------------------

            For i As Integer = 1 To aCount Step 1
                Add("")
            Next i
        End Sub
        Public Sub New(Optional aName As String = "", Optional aBaseValue As Object = Nothing, Optional aCount As Integer = 0, Optional aNodePath As String = "")

            'init ---------------------------------
            BaseValue = Nothing
            'Count = 0
            Defined = False
            Index = 0
            _Members = Nothing
            Name = ""
            NodePath = ""
            MaintainList = False
            ListValues = ""
            'init ---------------------------------

            Name = aName
            NodePath = aNodePath

            If aBaseValue IsNot Nothing Then
                BaseValue = aBaseValue
            Else
                BaseValue = 0
            End If
            If aCount > 0 Then

                For i As Integer = 1 To aCount
                    Add("")
                Next i
            End If
            If aName <> "" And aNodePath <> "" Then NodePath = TLISTS.BuildPath(NodePath, Name)
        End Sub

        Public Sub New(aValues As TVALUES, Optional bEmpty As Boolean = False)
            'init ---------------------------------
            BaseValue = Nothing
            'Count = 0
            'Defined = False
            Index = 0
            _Members = Nothing
            Name = ""
            NodePath = ""
            MaintainList = False
            ListValues = ""
            'init ---------------------------------


            BaseValue = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aValues.BaseValue)
            If Not bEmpty And aValues.Count > 0 Then
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object())(aValues._Members)

            End If

            ' Defined = aValues.Defined
            Index = aValues.Index
            Name = aValues.Name
            NodePath = aValues.NodePath
            MaintainList = aValues.MaintainList
            ListValues = aValues.ListValues
        End Sub


#End Region 'Constructors
#Region "Properties"




        Public ReadOnly Property Count As Integer
            Get
                If _Members Is Nothing Then ReDim _Members(-1)
                Return _Members.Count
            End Get
        End Property

#End Region 'Properties
#Region "Methods"

        Public Function Total(Optional bAbsValue As Boolean = True) As Double
            Dim _rVal As Double = 0
            Dim i As Integer
            Dim aVal As Object
            _rVal = 0
            For i = 1 To Count
                aVal = _Members(i - 1)
                If TVALUES.IsNumber(aVal) Then
                    _rVal += TVALUES.To_DBL(aVal, bAbsValue)
                End If
            Next i
            Return _rVal

        End Function

        Public Function ValueCount(aSearchValue As Object) As Integer

            Dim _rVal As Integer
            Dim i As Integer
            For i = 1 To Count
                If String.Compare(_Members(i - 1).ToString(), aSearchValue.ToString(), True) = 0 Then _rVal += 1
            Next i
            Return _rVal
        End Function

        Public Function StringVal(aIndex As Integer, Optional bUCase As Boolean = False) As String
            'base 1

            If aIndex < 1 Or aIndex > Count Then Return String.Empty
            Dim _rVal As String = _Members(aIndex - 1).ToString()
            If bUCase Then _rVal = UCase(_rVal)
            Return _rVal
        End Function

        Public Function Last(Optional aDefault As Object = Nothing)

            If Count > 0 Then
                Return _Members(Count - 1)
            Else
                Return aDefault
            End If

        End Function
        Public Function Item(aIndex As Integer) As Object
            'base 1
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            Return _Members(aIndex - 1)
        End Function

        Public Function Member(aIndex As Integer) As Object
            'base 0
            If aIndex < 0 Or aIndex > Count - 1 Then Return Nothing
            Return _Members(aIndex)
        End Function
        Public Sub SetMember(aIndex As Integer, aValue As Object)
            'base 0
            If aIndex < 0 Or aIndex > Count - 1 Then Return
            _Members(aIndex) = aValue
        End Sub
        Public Sub SetItem(aIndex As Integer, value As Object)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Overrides Function ToString() As String
            If MaintainList Then
                Return $"TVALUES[{  ListValues }]"
            Else
                Return $"TVALUES[{Count}]"
            End If
        End Function
        Public Function Add(aValue As Object, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bNoDupes As Boolean = False) As Boolean
            Dim rIndex As Integer = 0

            Return Add(aValue, aBeforeIndex, aAfterIndex, rIndex, bNoDupes)
        End Function
        Public Function Add(aValue As Object, aBeforeIndex As Integer, aAfterIndex As Integer, ByRef rIndex As Integer, Optional bNoDupes As Boolean = False) As Boolean
            rIndex = 0
            If Count >= Integer.MaxValue Or aValue Is Nothing Then Return False
            Dim NewMems() As Object
            Dim i As Integer
            Dim j As Integer
            Dim aFlg As Boolean
            Dim sVal As String
            sVal = aValue.ToString
            If bNoDupes And Count > 0 Then
                For i = 1 To Count
                    If String.Compare(_Members(i - 1), sVal, True) = 0 Then
                        If aBeforeIndex > 0 And aBeforeIndex <= Count Then
                            aFlg = MoveValue(i, aBeforeIndex - 1)
                        ElseIf aAfterIndex > 0 And aAfterIndex <= Count Then
                            aFlg = MoveValue(i, aAfterIndex + 1)
                        End If
                        Return False
                    End If
                Next i
            End If
            If Count = 0 Then
                aBeforeIndex = 0
                aAfterIndex = 0
                rIndex = 1
            Else
                If aBeforeIndex < 1 Then aBeforeIndex = 0
                If aAfterIndex < 1 Then aAfterIndex = 0
                If aBeforeIndex >= Count Then
                    aBeforeIndex = 0
                    aAfterIndex = 0
                End If
                If aAfterIndex >= Count Then
                    aBeforeIndex = 0
                    aAfterIndex = 0
                End If
            End If
            If aBeforeIndex = 0 And aAfterIndex = 0 Then

                System.Array.Resize(_Members, _Members.Count + 1)
                _Members(Count - 1) = aValue
                rIndex = Count
            Else

                NewMems = _Members
                System.Array.Resize(NewMems, NewMems.Count + 1)
                If aBeforeIndex > 0 Then
                    rIndex = aBeforeIndex - 1
                    j = 0
                    For i = 0 To Count - 1
                        If j + 1 = aBeforeIndex Then
                            NewMems(i) = aValue
                            i += 1
                            NewMems(i) = _Members(j)
                        Else
                            NewMems(i) = _Members(j)
                        End If
                        j += 1
                    Next i
                Else
                    rIndex = aAfterIndex + 1
                    j = 0
                    For i = 0 To Count - 1
                        If j + 1 = aAfterIndex Then
                            NewMems(i) = _Members(j)
                            i += 1
                            NewMems(i) = aValue
                        Else
                            NewMems(i) = _Members(j)
                        End If
                        j += 1
                    Next i
                End If
                _Members = NewMems
            End If
            'Application.DoEvents()
            If MaintainList Then TLISTS.Add(ListValues, sVal, bAllowDuplicates:=True, aDelimitor:=dxfGlobals.Delim, bAllowNulls:=True)
            Return True
        End Function
        Public Function AddNumber(aValue As Object, Optional bNoDupes As Boolean = False, Optional aPrecis As Integer = 5) As Boolean
            Dim rIndex As Integer = 0
            Return AddNumber(aValue, rIndex, bNoDupes, aPrecis)
        End Function
        Public Function AddNumber(aValue As Object, ByRef rIndex As Integer, Optional bNoDupes As Boolean = False, Optional aPrecis As Integer = 5) As Boolean
            rIndex = 0
            If Count >= Integer.MaxValue Then Return False
            Dim aVal As Double
            Dim bVal As Double
            Dim i As Integer
            aVal = TVALUES.To_DBL(aValue, aPrecis:=aPrecis)
            If bNoDupes Then
                For i = 1 To Count
                    bVal = TVALUES.To_DBL(_Members(i - 1), aPrecis:=aPrecis)
                    If aVal = bVal Then Return False
                Next i
            End If
            Return Add(aVal, 0, 0, rIndex)
        End Function
        Public Function AddString(aString As Object, Optional bTrim As Boolean = True, Optional bNoNulls As Boolean = True, Optional bNoDupes As Boolean = False, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As Boolean
            Dim rIndex As Integer = 0
            Return AddString(aString, bTrim, bNoNulls, bNoDupes, aBeforeIndex, aAfterIndex, rIndex)
        End Function
        Public Function AddString(aString As Object, bTrim As Boolean, bNoNulls As Boolean, bNoDupes As Boolean, aBeforeIndex As Integer, aAfterIndex As Integer, ByRef rIndex As Integer) As Boolean
            rIndex = 0
            If Count >= Integer.MaxValue Or aString Is Nothing Then Return False
            Dim aStr As String = String.Empty
            aStr = aString
            If bTrim Then aStr = Trim(aStr)
            If bNoNulls Then
                If Trim(aStr) = "" Then Return False
            End If
            Return Add(aStr, aBeforeIndex, aAfterIndex, rIndex, bNoDupes)
        End Function
        Public Function Append(bValues As TVALUES, Optional aPrefix As Object = Nothing, Optional bNoDupes As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim bPref As Boolean = aPrefix IsNot Nothing
            For i As Integer = 1 To bValues.Count
                If Not bPref Then
                    If Add(bValues._Members(i - 1), bNoDupes:=bNoDupes) Then _rVal = True
                Else
                    If Add($"{aPrefix} {bValues._Members(i - 1)}", bNoDupes:=bNoDupes) Then _rVal = True
                End If
            Next i
            Return _rVal
        End Function
        Public Function Clear() As Boolean
            Dim _rVal As Boolean = Count > 0
            ListValues = ""

            ReDim _Members(-1)
            Return _rVal
        End Function
        Public Function Clone(Optional bEmpty As Boolean = False) As TVALUES
            Return New TVALUES(Me, bEmpty)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVALUES(Me)
        End Function

        Public Sub RemoveValue(aValue As Object)
            Dim bValues As TVALUES
            Dim i As Integer
            bValues = New TVALUES(Me, True)
            For i = 1 To Count
                If _Members(i - 1) <> aValue Then
                    bValues.Add(_Members(i - 1))
                End If
            Next i

            _Members = bValues._Members
        End Sub
        Public Function RemoveDupes(aValues As TVALUES) As TVALUES
            Dim _rVal As TVALUES = Clone(True)
            Dim i As Integer
            Dim j As Integer
            Dim bKeep As Boolean
            Dim bValues As New TVALUES
            Dim aVal As Object
            Dim bVal As Object
            For i = 1 To Count
                aVal = _Members(i - 1)
                bKeep = True
                For j = i + 1 To Count
                    bVal = _Members(j - 1)
                    If String.Compare(aVal, bVal, True) = 0 Then
                        bKeep = False
                        Exit For
                    End If
                Next j
                If bKeep Then
                    bValues.Add(aVal)
                Else
                    _rVal.Add(i - 1)
                End If
            Next i

            _Members = bValues._Members.Clone
            Return _rVal
        End Function
        Public Function RemoveStringValue(aValue As Object, Optional bJustOne As Boolean = False) As Boolean
            Dim rIndex As Integer = 0
            Dim rCount As Integer = 0
            Return RemoveStringValue(aValue, rIndex, rCount, bJustOne)
        End Function
        Public Function RemoveStringValue(aValue As Object, ByRef rIndex As Integer, ByRef rCount As Integer, Optional bJustOne As Boolean = False) As Boolean
            Dim _rVal As Boolean
            rIndex = -1
            rCount = 0
            Dim i As Integer
            Dim aVals As TVALUES
            aVals = Clone(True)
            For i = 1 To Count
                If String.Compare(_Members(i - 1), aValue, True) <> 0 Then
                    aVals.Add(_Members(i - 1))
                    _rVal = True
                    If rIndex < 0 Then rIndex = i - 1
                    rCount += 1
                    If bJustOne Then Exit For
                End If
            Next i

            _Members = aVals._Members.Clone
            Return _rVal
        End Function
        Public Function SubSet(aMaxIndex As Integer, Optional aIndexList As String = "") As TVALUES
            Dim _rVal As TVALUES = Clone(True)
            Dim i As Integer
            For i = 1 To Count
                If i > aMaxIndex And aMaxIndex > 0 Then Exit For
                If TLISTS.Contains(i, aIndexList, bReturnTrueForNullList:=True) Then
                    _rVal.Add(_Members(i - 1))
                End If
            Next i
            Return _rVal
        End Function
        Public Function SubValues(aStartIndex As Object, aEndIndex As Object) As TVALUES
            Dim _rVal As New TVALUES
            '#1the source values
            '#2the starting index of the value range to return
            '#3the ending index of the value range to return
            '^returns a contiguous sus-set of the passed values
            Dim si As Integer
            Dim ei As Integer
            Dim i As Integer
            si = TVALUES.To_INT(aStartIndex)
            ei = TVALUES.To_INT(aEndIndex)
            TVALUES.SortTwoValues(True, si, ei)
            _rVal = Clone(True)
            For i = 1 To Count
                If i >= si And i <= ei Then
                    _rVal.Add(_Members(i - 1))
                End If
            Next i
            Return _rVal
        End Function
        Public Function CompareString(bValues As TVALUES, Optional aSkipList As String = "", Optional bBailOnOne As Boolean = False, Optional aDelimitor As String = ",") As Boolean
            Dim rMisMatches As String = String.Empty
            Return CompareString(bValues, aSkipList, bBailOnOne, rMisMatches, aDelimitor)
        End Function
        Public Function CompareString(bValues As TVALUES, aSkipList As String, bBailOnOne As Boolean, ByRef rMisMatches As String, Optional aDelimitor As String = ",") As Boolean
            rMisMatches = ""
            If bValues.Count <= 0 Then Return False

            Dim skippers = Not String.IsNullOrWhiteSpace(aSkipList)
            If skippers Then aSkipList = aSkipList.Trim()



            For i As Integer = 1 To Count

                If i > bValues.Count Then Exit For
                If skippers Then
                    If TLISTS.Contains(i, aSkipList, aDelimitor) Then Continue For
                End If
                Dim aVal As Object = Item(i)
                Dim bVal As Object = bValues.Item(i)
                Dim sval1 As String = IIf(aVal Is Nothing, "", aVal.ToString)
                Dim sval2 As String = IIf(bVal Is Nothing, "", bVal.ToString)

                If String.Compare(sval1, sval2, True) <> 0 Then
                    TLISTS.Add(rMisMatches, i, bAllowDuplicates:=True, aDelimitor:=aDelimitor)
                    If bBailOnOne Then Exit For
                End If

            Next i
            Return rMisMatches = ""

        End Function
        Public Function ContainsStrings(aList As String, Optional aDelimitor As String = ",") As Boolean
            Dim rMatchCount As Integer = 0
            Return ContainsStrings(aList, aDelimitor, rMatchCount)
        End Function
        Public Function ContainsStrings(aList As String, aDelimitor As String, ByRef rMatchCount As Integer) As Boolean
            Dim _rVal As Boolean
            rMatchCount = 0
            If Count <= 0 Then Return _rVal
            Dim aVals As TVALUES = TLISTS.ToValues(aList, aDelimitor)
            If aVals.Count <= 0 Then Return _rVal
            Dim i As Integer
            For i = 1 To aVals.Count
                If FindStringValue(aVals._Members(i - 1)) > 0 Then
                    rMatchCount += 1
                End If
            Next i
            _rVal = (rMatchCount = aVals.Count)
            Return _rVal
        End Function
        Public Function ExtremeValue(bMax As Boolean, Optional bAbs As Boolean = False) As Double
            Dim rIndex As Integer = 0
            Return ExtremeValue(bMax, bAbs, rIndex)
        End Function
        Public Function ExtremeValue(bMax As Boolean, bAbs As Boolean, ByRef rIndex As Integer) As Double
            Dim _rVal As Double = 0
            rIndex = -1

            Dim bVal As Double = 0
            Dim aVal As Double = 0
            For i As Integer = 1 To Count
                If i = 1 Then

                    bVal = TVALUES.ToDouble(_Members(i - 1), bAbs, aPrecis:=6)

                Else
                    aVal = TVALUES.ToDouble(_Members(i - 1), bAbs, aPrecis:=6)
                    If bMax Then
                        If aVal > bVal Then rIndex = i - 1
                    Else
                        If aVal < bVal Then rIndex = i - 1
                    End If
                End If
            Next i
            If rIndex > -1 Then _rVal = aVal
            Return _rVal
        End Function
        Public Function FindNumericValue(aValue As Object, Optional aPrecis As Integer? = Nothing, Optional bAbsoluteValue As Boolean = False, Optional aOccur As Integer = 0, Optional iStart As Integer = 1) As Integer
            If Count <= 0 Or Not TVALUES.IsNumber(aValue) Then Return 0
            Dim _rVal As Integer = 0
            If aOccur <= 0 Then aOccur = 1
            Dim prec As Integer = -1
            If aPrecis IsNot Nothing Then
                If aPrecis.HasValue Then prec = TVALUES.LimitedValue(aPrecis.Value, -1, 15)
            End If
            Dim cnt As Integer
            Dim aVal As Double = TVALUES.To_DBL(aValue, bAbsoluteValue, prec)
            Dim dVal As Double
            Dim si As Integer
            Dim ei As Integer
            If Not dxfUtils.LoopIndices(Count, iStart, Count, si, ei) Then Return _rVal


            For i As Integer = si To ei
                dVal = TVALUES.To_DBL(_Members(i - 1), bAbsoluteValue, prec)
                If aVal = dVal Then
                    cnt += 1
                    If cnt = aOccur Then
                        _rVal = i
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function

        Public Function ContainsString(aString As String, Optional bIgnoreCase As Boolean = True) As Boolean
            If Count <= 0 Or aString Is Nothing Then Return False
            For i As Integer = 1 To _Members.Count
                If String.Compare(_Members(i - 1).ToString, aString, bIgnoreCase) = 0 Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function FindStringValue(aStringVal As Object, Optional aLimitChar As Char? = Nothing, Optional aOccur As Integer = 0, Optional iStart As Integer = 1, Optional bStartsWith As Boolean = False) As Integer
            Dim _rVal As Integer = 0
            If Count <= 0 Or aStringVal Is Nothing Then Return 0
            If aOccur <= 0 Then aOccur = 1

            Dim cnt As Integer
            Dim bStr As String
            Dim j As Integer
            Dim aStr As String = aStringVal.ToString()
            Dim llen As Long

            If bStartsWith Then
                llen = aStr.Length
                If llen <= 0 Then Return _rVal
            End If

            Dim si As Integer
            Dim ei As Integer
            If Not dxfUtils.LoopIndices(Count, iStart, Count, si, ei) Then Return _rVal

            For i As Integer = si To ei
                If _Members(i - 1) Is Nothing Then Continue For

                bStr = _Members(i - 1).ToString()

                If aLimitChar.HasValue Then
                    j = bStr.IndexOf(aLimitChar.Value)
                    If j >= 0 Then
                        bStr = bStr.Substring(0, j + 1)
                    End If
                End If
                If bStartsWith Then
                    If bStr.Length >= llen Then
                        If String.Compare(bStr.Substring(0, llen), aStr, True) = 0 Then
                            cnt += 1
                            If cnt = aOccur Then
                                _rVal = i
                                Exit For
                            End If
                        End If
                    End If
                Else
                    If String.Compare(bStr, aStr, True) = 0 Then
                        cnt += 1
                        If cnt = aOccur Then
                            _rVal = i
                            Exit For
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Friend Function GetInRange(aLower As Object, aUpper As Object, bOnisIn As Boolean, ByRef rIndices As TVALUES, Optional aPrecis As Integer? = Nothing) As TVALUES
            Dim _rVal As TVALUES = Clone(True)
            '#1the subject values
            '#2the lower bound to apply
            '#3the upper bound to apply
            '#4flag indicating if a value on a bound should be considered withing the range
            '#5returns the indices of the members in the range
            '#6a precision to apply
            '^returns the values that fall within the passed limits

            Dim uVal As Double
            Dim lVal As Double
            Dim dVal As Double
            Dim precis As Integer = -1
            If aPrecis IsNot Nothing Then
                If aPrecis.HasValue Then precis = TVALUES.LimitedValue(aPrecis.Value, -1, 15)
            End If
            rIndices = New TVALUES(0)

            uVal = TVALUES.To_DBL(aUpper, bAbsVal:=False, aPrecis:=IIf(precis >= 0, precis, Nothing))
            lVal = TVALUES.To_DBL(aLower, bAbsVal:=False, aPrecis:=IIf(precis >= 0, precis, Nothing))
            TVALUES.SortTwoValues(True, lVal, uVal)

            For i As Integer = 1 To Count
                dVal = TVALUES.To_DBL(_Members(i - 1), bAbsVal:=False, aPrecis:=IIf(precis >= 0, precis, Nothing))
                If bOnisIn Then
                    If dVal >= lVal And dVal <= uVal Then
                        _rVal.Add(_Members(i - 1))
                        rIndices.Add(i)
                    End If
                Else
                    If dVal > lVal And dVal < uVal Then
                        _rVal.Add(_Members(i - 1))
                        rIndices.Add(i)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Sub Invert()
            If Count > 1 Then System.Array.Reverse(_Members)
        End Sub
        Public Function MatchingValues(bValues As TVALUES) As TVALUES
            Dim rMatchCount As Integer = 0
            Return MatchingValues(bValues, rMatchCount)
        End Function
        Public Function MatchingValues(bValues As TVALUES, ByRef rMatchCount As Integer) As TVALUES
            Dim _rVal As TVALUES = New TVALUES(Me, True)
            rMatchCount = 0

            For i As Integer = 1 To Count
                If i > bValues.Count Then Exit For
                Dim aVal As Object = Item(i)
                Dim bVal As Object = bValues.Item(i)
                Dim sval1 As String = IIf(aVal Is Nothing, "", aVal.ToString)
                Dim sval2 As String = IIf(bVal Is Nothing, "", bVal.ToString)

                If String.Compare(sval1, sval2, True) = 0 Then
                    rMatchCount += 1
                    _rVal.Add(_Members(i - 1))
                Else
                    Exit For
                End If


            Next i
            Return _rVal
        End Function
        Public Function MoveValue(aIndex As Integer, aToIndex As Integer) As Boolean
            Dim _rVal As Boolean
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            If aToIndex < 1 Then aToIndex = 1
            If aToIndex > Count Then aToIndex = Count
            If aIndex = aToIndex Then Return _rVal
            'On Error Resume Next
            _rVal = True
            Dim nVals As TVALUES
            Dim i As Integer
            Dim aVal As Object
            nVals = Clone()
            aVal = _Members(aIndex - 1)
            If aIndex > aToIndex Then 'moving up
                For i = 1 To Count
                    If i < aToIndex Then
                        nVals._Members(i - 1) = _Members(i - 1)
                    Else
                        If i = aToIndex Then
                            nVals._Members(i - 1) = aVal
                        Else
                            nVals._Members(i - 1) = _Members(i - 2)
                        End If
                    End If
                Next i
            Else 'moving down
                For i = 1 To Count
                    If i > aToIndex Then
                        nVals._Members(i - 1) = _Members(i - 1)
                    Else
                        If i = aIndex Then
                            nVals._Members(i - 1) = aVal
                        Else
                            nVals._Members(i - 1) = _Members(i)
                        End If
                    End If
                Next i
            End If
            _Members = nVals._Members
            Return _rVal
        End Function
        Public Sub Print(Optional bBaseZero As Boolean = False)
            Dim i As Integer
            Dim j As Integer
            System.Diagnostics.Debug.WriteLine("")
            For i = 1 To Count
                j = i
                If bBaseZero Then j -= 1
                If _Members(i - 1) Is Nothing Then
                    System.Diagnostics.Debug.WriteLine(j & " Is Empty ")
                Else
                    System.Diagnostics.Debug.WriteLine(j & " = " & _Members(i - 1))
                End If
            Next i
        End Sub
        Public Function Remove(aIndex As Integer) As Boolean
            'base 1
            If aIndex < 1 Or aIndex > Count Or Count <= 0 Then Return False
            If Count = 1 Then
                Clear()
                Return True
            Else
                If aIndex = Count Then
                    System.Array.Resize(_Members, Count)
                Else
                    Dim newMems(0 To Count - 2) As Object
                    Dim j As Integer = 0
                    For i As Integer = 1 To Count
                        If i <> aIndex Then
                            newMems(j) = _Members(i - 1)
                            j += 1
                        End If
                    Next i

                    _Members = newMems
                End If

            End If
            If MaintainList Then ListValues = ToList(dxfGlobals.Delim)
            Return True
        End Function
        Public Function SetValue(aIndex As Object, aValue As Object) As Boolean
            Dim _rVal As Boolean
            Dim i As Integer = TVALUES.To_INT(aIndex)
            If i > 0 And i <= Count Then
                If _Members(i - 1) <> aValue Then _rVal = True
                _Members(i - 1) = aValue
            End If
            Return _rVal
        End Function

        Public Function Sort(bHighToLow As Boolean, bNumeric As Boolean, Optional bRemoveDupes As Boolean = False, Optional aPrecis As Integer? = Nothing) As Boolean

            '^sorts the values in the array from low to high
            '#1 flag to sort from hight to low
            '#2 flag to convert to numbers and sort numerically
            '#3 flag to remove any duplicates
            '#4 a precis to apply if the sort is numeric

            If Count <= 0 Then Return False


            If bNumeric And Not aPrecis.HasValue Then aPrecis = 10
            Dim oldVals As List(Of Object) = ToListObj(False, aPrecis)
            Dim newVals As List(Of Object) = ToListObj(bRemoveDupes, aPrecis)

            Dim _rVal As Boolean = newVals.Count <> Count
            newVals.Sort()
            If bHighToLow Then newVals.Reverse()

            Dim aMems(newVals.Count) As Object
            For i As Integer = 1 To newVals.Count
                If Not _rVal Then
                    If newVals.Item(i - 1) <> oldVals(i - 1) Then _rVal = True
                End If
                aMems(i - 1) = newVals.Item(i - 1)
            Next

            _Members = aMems
            If MaintainList Then ListValues = ToList(dxfGlobals.Delim)

            Return _rVal

        End Function

        Public Function ToListObj(bRemoveDupes As Boolean, Optional aPrecis? As Integer = Nothing) As List(Of Object)

            Dim bDoubles As Boolean = aPrecis.HasValue
            Dim precis = IIf(bDoubles, TVALUES.LimitedValue(aPrecis.Value, 0, 15), 0)

            Dim _rVal As New List(Of Object)
            For i = 1 To Count
                Dim ival As Object = Item(i)
                If ival Is Nothing Then Continue For
                If Not bDoubles Then
                    If Not bRemoveDupes Then
                        _rVal.Add(ival)
                    Else
                        If Not _rVal.Contains(ival) Then _rVal.Add(ival)
                    End If
                Else
                    If Not TVALUES.IsNumber(ival) Then Continue For
                    Dim dval As Double = TVALUES.To_DBL(ival, aPrecis:=precis)
                    If Not bRemoveDupes Then
                        _rVal.Add(dval)
                    Else
                        If Not _rVal.Contains(dval) Then _rVal.Add(dval)
                    End If

                End If

            Next
            Return _rVal
        End Function

        Public Function StringExists(aString As Object) As Boolean
            Dim rIndex As Integer = 0
            Return StringExists(aString, rIndex)
        End Function
        Public Function StringExists(aString As Object, ByRef rIndex As Integer) As Boolean
            rIndex = FindStringValue(aString)
            Return rIndex > 0
        End Function
        Public ReadOnly Property ToStringList As List(Of String)
            Get
                Dim _rVal As New List(Of String)
                For i As Integer = 1 To Count
                    _rVal.Add(Item(i).ToString)
                Next
                Return _rVal
            End Get
        End Property
        Public Function ToList(Optional aDelim As Char = ",", Optional bNoNulls As Boolean = False, Optional aLastDelim As String = "", Optional aMaxIndex As Integer = 0, Optional aIndexList As String = "") As String
            Dim rListCount As Integer = 0
            Return ToList(aDelim, bNoNulls, rListCount, aLastDelim, aMaxIndex, aIndexList)
        End Function
        Public Function ToList(aDelim As Char, bNoNulls As Boolean, ByRef rListCount As Integer, Optional aLastDelim As String = "", Optional aMaxIndex As Integer = 0, Optional aIndexList As String = "") As String
            Dim _rVal As String = String.Empty
            rListCount = 0

            If Not bNoNulls And aMaxIndex <= 0 Then
                Return Strings.Join(_Members, aDelim)
            Else
                For i As Integer = 1 To Count
                    If i > aMaxIndex And aMaxIndex > 0 Then Exit For
                    If TLISTS.Contains(i, aIndexList, bReturnTrueForNullList:=True) Then
                        If TLISTS.Add(_rVal, _Members(i - 1), bAllowDuplicates:=True, aDelimitor:=aDelim, bAllowNulls:=Not bNoNulls) Then
                            rListCount += 1
                        End If
                    End If
                Next i
            End If
            If Not String.IsNullOrWhiteSpace(aLastDelim) And rListCount > 1 Then
                Dim i As Integer = _rVal.LastIndexOf(aDelim)
                If i > 0 Then
                    _rVal = $"{_rVal.Substring(0, i)}{aLastDelim}{_rVal.Substring(i + 1, _rVal.Length - (i + 1))}"


                End If
            End If
            Return _rVal
        End Function
        Public Function UniqueValues(Optional bNumeric As Boolean = False, Optional aPrecis As Integer? = Nothing, Optional bAbsoluteVal As Boolean = False) As TVALUES

            If Count <= 1 Then Return New TVALUES(Me)
            Dim _rVal As TVALUES = New TVALUES(Me, True)
            Dim idx As Integer
            Dim prec As Integer = -1
            If aPrecis IsNot Nothing Then
                If aPrecis.HasValue Then prec = TVALUES.LimitedValue(aPrecis.Value, -1, 15)
            End If

                For i As Integer = 1 To Count
                Dim aVal As Object = _Members(i - 1)
                If Not bNumeric Then
                    idx = _rVal.FindStringValue(aVal)
                Else
                    idx = _rVal.FindNumericValue(aVal, aPrecis:=IIf(prec >= 0, prec, Nothing), bAbsoluteValue:=bAbsoluteVal)
                End If
                If idx <= 0 Then
                    _rVal.Add(aVal)
                End If
            Next i
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function CompareNumbers(aValue As Object, bValue As Object, Optional aPrecis As Integer = 6, Optional bAbsoluteVal As Boolean = False) As Boolean
            Dim rDifference As Double = 0
            Return CompareNumbers(aValue, bValue, aPrecis, bAbsoluteVal, rDifference)
        End Function
        Public Shared Function CompareNumbers(aValue As Object, bValue As Object, aPrecis As Integer, bAbsoluteVal As Boolean, ByRef rDifference As Double) As Boolean
            rDifference = 0
            If aValue Is Nothing Or bValue Is Nothing Then Return False
            If Not IsNumber(aValue) Or Not IsNumber(bValue) Then Return False
            Dim ip As Integer = aPrecis
            If TypeOf (aValue) Is Integer Or TypeOf (aValue) Is Long Or TypeOf (aValue) Is Byte Then ip = 1
            Dim d1 As Double = To_DBL(aValue, bAbsoluteVal, ip)
            Dim d2 As Double = To_DBL(bValue, bAbsoluteVal, ip)
            rDifference = d1 - d2
            Return rDifference = 0
        End Function

        Public Shared Function BitCode_FindSubCode(aBitCodeSum As Integer, aSearchVal As Object) As Boolean
            Dim _rVal As Boolean = False
            If Not TVALUES.IsNumber(aSearchVal) Then Return False
            Dim lVal As Integer = TVALUES.To_INT(aBitCodeSum, 0)
            Dim iVal As Integer = Math.Abs(TVALUES.To_INT(aSearchVal, 0))
            If iVal = lVal Then Return True
            If iVal > lVal Then Return False
            Dim rMemberList As String = String.Empty
            TVALUES.BitCode_Decompose(lVal, rMemberList, iVal, _rVal)
            Return _rVal
        End Function
        Public Shared Function BitCode_Decompose(aBitCodeSum As Integer, Optional aBitCodeSubValToFind As Object = Nothing) As TVALUES
            Dim rSearchValueFound As Boolean = False
            Dim rMemberList As String = String.Empty
            Return BitCode_Decompose(aBitCodeSum, rMemberList, aBitCodeSubValToFind, rSearchValueFound)
        End Function
        Public Shared Function BitCode_Decompose(aBitCodeSum As Integer) As TVALUES
            '^returns the bit code sum parsed into a value array containing all the present members in the sum
            Dim _rVal As New TVALUES(0)
            Dim lVal As Integer = Math.Abs(aBitCodeSum)
            If lVal <= 0 Then Return _rVal
            Dim i As Integer = 1
            Dim mVal As Integer
            Dim aVal As Integer = 1
            Dim bitVals As TVALUES
            bitVals = New TVALUES(0)

            bitVals.Add(aVal)
            Do While mVal < lVal
                If i > 1 Then
                    aVal *= 2
                    bitVals.Add(aVal)
                End If
                mVal += aVal
                i += 1
            Loop
            For i = bitVals.Count To 1 Step -1
                aVal = bitVals._Members(i - 1)
                If lVal >= aVal Then
                    _rVal.Add(aVal)
                    Do While lVal >= aVal
                        lVal -= aVal
                    Loop
                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function BitCode_Decompose(aBitCodeSum As Integer, Optional aSearchVal As Object = Nothing, Optional bReturnOnlySearchVal As Boolean = False) As TVALUES
            Dim rMemberList As String = String.Empty
            Dim rSearchValueFound As Boolean = False
            Return BitCode_Decompose(aBitCodeSum, rMemberList, aSearchVal, rSearchValueFound, bReturnOnlySearchVal)
        End Function
        Public Shared Function BitCode_Decompose(aBitCodeSum As Integer, ByRef rMemberList As String, aSearchVal As Object, ByRef rSearchValueFound As Boolean, Optional bReturnOnlySearchVal As Boolean = False) As TVALUES
            '#1the bitcode sum to decompose
            '#2returns a list of the bit values in the passed value
            '#3an optional bitcode sub-value to look for if one is passed
            '#4a return flag indicating if the search value was found in the sum
            '#5flag indicating that the the list of values should only include the search for bit code value
            '^returns the bit code sum parsed into a value array containing all the present members in the sum
            Dim _rVal As New TVALUES(0)
            rMemberList = "" 'a list of the bit values in the passed value
            rSearchValueFound = False 'returns true if a bit value to search for is passed and it is found
            Dim lVal As Integer = TVALUES.To_INT(aBitCodeSum, 0)
            If lVal <= 0 Then Return _rVal
            Dim i As Integer = 1
            Dim mVal As Integer
            Dim aVal As Integer = 1
            Dim bitVals As TVALUES
            Dim svalidx As Integer
            Dim srchVal As Integer
            Dim bSrch As Boolean
            If Not TVALUES.IsNumber(aSearchVal) Then
                bSrch = True
                srchVal = TVALUES.To_INT(aSearchVal)
            End If
            bitVals = New TVALUES(0)

            bitVals.Add(aVal)
            Do While mVal < lVal
                If i > 1 Then
                    aVal *= 2
                    bitVals.Add(aVal)
                End If
                mVal += aVal
                i += 1
            Loop
            For i = bitVals.Count To 1 Step -1
                aVal = bitVals._Members(i - 1)
                If lVal >= aVal Then
                    _rVal.Add(aVal)
                    TLISTS.Add(rMemberList, aVal, bAllowDuplicates:=True)
                    If bSrch Then
                        If aVal = srchVal Then rSearchValueFound = True
                        svalidx = i
                    End If
                    Do While lVal >= aVal
                        lVal -= aVal
                    Loop
                End If
            Next i
            If bReturnOnlySearchVal And bSrch Then
                bitVals = New TVALUES(0)
                If svalidx > 0 Then
                    bitVals.Add(_rVal._Members(svalidx - 1))
                End If
                _rVal = bitVals
            End If
            Return _rVal
        End Function
        Public Shared Function ParseCommaString(aString As String) As TVALUES
            Dim _rVal As New TVALUES
            aString = Trim(aString)
            If aString = "" Then Return _rVal
            Dim i As Long
            Dim aStr As String = String.Empty
            Dim iQuote As Long
            Dim aCr As String
            iQuote = 1
            For i = 1 To aString.Length
                aCr = Mid(aString, i, 1)
                If Asc(aCr) = 34 Then
                    iQuote += 1
                ElseIf Asc(aCr) = 44 Then
                    If iQuote Mod 2 = 1 Then
                        _rVal.Add(Trim(aStr))
                        aStr = ""
                        iQuote = 1
                    Else
                        aStr += aCr
                    End If
                Else
                    aStr += aCr
                End If
            Next i
            If _rVal.Count <= 0 Then
                _rVal.Add(aString)
            End If
            Return _rVal
        End Function
        Public Shared Function FromList(aList As String, Optional bTrimmed As Boolean = True, Optional bNoNulls As Boolean = False, Optional aDelim As String = ",", Optional bUnique As Boolean = False) As TVALUES
            Dim _rVal As New TVALUES(0)
            If String.IsNullOrWhiteSpace(aList) Then Return _rVal
            aList = aList.Trim
            Dim sVals() As String = aList.Split(aDelim)

            For i As Integer = 0 To sVals.Length - 1
                Dim sVal As String = sVals(i)
                If bTrimmed Then sVal = sVal.Trim
                If Not bNoNulls Or (bNoNulls And Not String.IsNullOrWhiteSpace(sVal)) Then
                    If Not bUnique Then
                        _rVal.Add(sVal)
                    Else
                        If Not _rVal.ContainsString(sVal) Then _rVal.Add(sVal)
                    End If

                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function BitCode_ToggleSubCode(ByRef aBitCodeSum As Integer, aBitCodeSubVal As Integer, bInclude As Boolean, Optional aBaseVal As Integer = 0) As Boolean
            '#1 the current bitcode sum
            '#2 the bitcode sub-value to add or remove from the sum
            '#3 true to add the bitcode sub-value to the bitcode sum false to remove it from the sum
            '#4an optional base value to always include in the sum
            '^returns true if the passed code is not in the current bitcode sum and the request is to include it or
            '^returns true if the passed code is in the current bitcode sum and the request is to exclude it
            Dim _rVal As Boolean
            Dim iVal As Integer = Math.Abs(aBitCodeSum)
            Dim bFound As Boolean
            If aBaseVal > 0 Then
                If iVal > aBaseVal Then
                    iVal = aBaseVal
                Else
                    aBaseVal = 0
                End If
            Else
                aBaseVal = 0
            End If
            aBitCodeSubVal = Math.Abs(aBitCodeSubVal)
            Dim rMemberList As String = String.Empty
            'get all the values currently in the sum and see if the passed value is currently included
            Dim bitVals As TVALUES = TVALUES.BitCode_Decompose(iVal, rMemberList, aBitCodeSubVal, bFound)
            _rVal = (bInclude And Not bFound) Or (Not bInclude And bFound)
            If bInclude And Not bFound Then
                'Return true if the current sum did Not include the code And we are requested to add it
                _rVal = True
                iVal += aBitCodeSubVal  'add it to the sum
            ElseIf Not bInclude And bFound Then
                'Return true if the current sum did include the code And we are requested to remove it
                _rVal = True
                iVal -= aBitCodeSubVal  'remove it from the sum
            End If
            iVal += aBaseVal
            aBitCodeSum = iVal
            Return _rVal
        End Function
        Public Shared Function ParseWords(aString As String, Optional bIgnoreParens As Boolean = False, Optional bIgnoreQuotes As Boolean = False) As TVALUES
            Dim rWordList As String = String.Empty
            Return ParseWords(aString, bIgnoreParens, bIgnoreQuotes, rWordList)
        End Function
        Public Shared Function ParseWords(aString As String, bIgnoreParens As Boolean, bIgnoreQuotes As Boolean, ByRef rWordList As String) As TVALUES
            rWordList = ""
            If String.IsNullOrWhiteSpace(aString) Then Return New TVALUES("", "")
            aString = aString.Trim
            Dim _rVal As New TVALUES("", aString)
            Dim aCr As String
            Dim bCr As String
            Dim cCr As String
            Dim fCr As String
            Dim llen As Long
            Dim idx As Integer = -1
            Dim iParen As Integer
            Dim iQt As Integer
            Dim iSpaces As Integer
            Dim bKeep As Boolean
            rWordList = ""
            llen = aString.Length
            iParen = 1
            iQt = 1
            fCr = ""
            For i As Integer = 1 To llen
                aCr = Mid(aString, i, 1)
                If aCr <> " " Then
                    iSpaces = 0
                    bKeep = True
                    If fCr = "" And (bIgnoreParens Or (Not bIgnoreParens And aCr <> "(")) And (bIgnoreQuotes Or (Not bIgnoreQuotes And aCr <> """")) Then
                        fCr = aCr
                        idx += 1
                        _rVal.Add(fCr)
                        bKeep = False
                    End If
                Else
                    iSpaces += 1
                    bKeep = False
                End If
                If bKeep And Not bIgnoreParens Then
                    If aCr = "(" Then
                        bKeep = False
                        iParen = 1
                        TLISTS.Add(rWordList, _rVal._Members(idx), aDelimitor:="¸")
                        idx += 1
                        _rVal.Add(aCr)
                        For j As Integer = i + 1 To llen
                            bCr = Mid(aString, j, 1)
                            If bCr = ") " Then
                                iParen -= 1
                                If iParen = 0 Then
                                    _rVal._Members(idx) = _rVal._Members(idx) & bCr
                                    TLISTS.Add(rWordList, _rVal._Members(idx), aDelimitor:="¸")
                                    i = j
                                    Exit For
                                End If
                            ElseIf bCr = "(" Then
                                iParen += 1
                                _rVal._Members(idx) = _rVal._Members(idx) & bCr
                            Else
                                _rVal._Members(idx) = _rVal._Members(idx) & bCr
                            End If
                        Next j
                    End If
                End If
                If bKeep And Not bIgnoreQuotes Then
                    If aCr = """" Then
                        bKeep = False
                        iQt = 1
                        TLISTS.Add(rWordList, _rVal._Members(idx), aDelimitor:="¸")
                        idx += 1
                        _rVal.Add(aCr)
                        For j As Integer = i + 1 To llen
                            bCr = Mid(aString, j, 1)
                            If j < llen Then cCr = Mid(aString, j + 1, 1) Else cCr = ""
                            If bCr = """" Then
                                If cCr <> """" Then
                                    iQt -= 1
                                    If iQt = 0 Then
                                        _rVal._Members(idx) = _rVal._Members(idx) & bCr
                                        TLISTS.Add(rWordList, _rVal._Members(idx), aDelimitor:="¸")
                                        i = j
                                        Exit For
                                    End If
                                ElseIf cCr = """" Then
                                    _rVal._Members(idx) = _rVal._Members(idx) & bCr
                                    j += 1
                                Else
                                    _rVal._Members(idx) = _rVal._Members(idx) & bCr
                                End If
                            Else
                                _rVal._Members(idx) = _rVal._Members(idx) & bCr
                            End If
                        Next j
                    End If
                End If
                If bKeep Then
                    _rVal._Members(idx) = _rVal._Members(idx) & aCr
                Else
                    If fCr <> "" And aCr = " " Then
                        If iSpaces = 1 Then
                            TLISTS.Add(rWordList, _rVal._Members(idx), aDelimitor:="¸")
                        End If
                        fCr = ""
                    End If
                End If
            Next i
            If iSpaces = 0 And idx >= 0 Then
                TLISTS.Add(rWordList, _rVal._Members(idx), aDelimitor:="¸")
            End If
            _rVal.BaseValue = _rVal.ToList(" ")
            Return _rVal
        End Function

        Friend Shared Function SortTwoValues(bLowToHigh As Boolean, ByRef ioValue1 As Single, ByRef ioValue2 As Single) As Boolean
            Dim _rVal As Boolean
            '^sorts the passed to values as indicated
            '~returns True if the values were swapped
            Dim v1 As Object
            Dim v2 As Object
            If bLowToHigh Then
                If ioValue1 <= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            Else
                If ioValue1 >= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            End If
            ioValue1 = v1
            ioValue2 = v2
            Return _rVal
        End Function
        Friend Shared Function SortTwoValues(bLowToHigh As Boolean, ByRef ioValue1 As Long, ByRef ioValue2 As Long) As Boolean
            Dim _rVal As Boolean
            '^sorts the passed to values as indicated
            '~returns True if the values were swapped
            Dim v1 As Object
            Dim v2 As Object
            If bLowToHigh Then
                If ioValue1 <= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            Else
                If ioValue1 >= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            End If
            ioValue1 = v1
            ioValue2 = v2
            Return _rVal
        End Function


        Friend Shared Function SortTwoValues(bLowToHigh As Boolean, ByRef ioValue1 As Double, ByRef ioValue2 As Double) As Boolean
            Dim _rVal As Boolean
            '^sorts the passed to values as indicated
            '~returns True if the values were swapped
            Dim v1 As Double
            Dim v2 As Double
            If bLowToHigh Then
                If ioValue1 <= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            Else
                If ioValue1 >= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            End If
            ioValue1 = v1
            ioValue2 = v2
            Return _rVal
        End Function
        Friend Shared Function SortTwoValues(bLowToHigh As Boolean, ByRef ioValue1 As Integer, ByRef ioValue2 As Integer) As Boolean
            Dim _rVal As Boolean
            '^sorts the passed to values as indicated
            '~returns True if the values were swapped
            Dim v1 As Integer
            Dim v2 As Integer
            If bLowToHigh Then
                If ioValue1 <= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            Else
                If ioValue1 >= ioValue2 Then
                    v1 = ioValue1
                    v2 = ioValue2
                Else
                    _rVal = True
                    v1 = ioValue2
                    v2 = ioValue1
                End If
            End If
            ioValue1 = v1
            ioValue2 = v2
            Return _rVal
        End Function
        Public Shared Function ToBoolean(aVariant As Object, Optional aDefault As Boolean = False, Optional bSwitchVal As Boolean = False) As Boolean
            Dim _rVal As Boolean = aDefault
            If aVariant Is Nothing Then Return _rVal
            If TypeOf aVariant Is Boolean Then
                _rVal = aVariant
            Else
                If TVALUES.IsNumber(aVariant) Then
                    If Not bSwitchVal Then
                        _rVal = TVALUES.To_INT(aVariant) = -1
                    Else
                        _rVal = Not TVALUES.To_INT(aVariant) = 0
                    End If
                Else
                    Dim aStr As String = aVariant.ToString.ToUpper
                    _rVal = (aStr = "TRUE") Or (aStr = "YES") Or (aStr = "Y") Or (aStr = "T")
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function ToByte(aVariant As Object, Optional aDefault As Byte = 0) As Byte
            If aVariant Is Nothing Then Return aDefault
            If Not TVALUES.IsNumber(aVariant) Then Return aDefault
            Try

                Dim lVal As Integer = Convert.ToInt32(aVariant)
                If lVal <= Byte.MinValue Then
                    Return 0
                ElseIf lVal > Byte.MaxValue Then
                    Return 255
                Else
                    Return Convert.ToByte(lVal)
                End If
            Catch ex As Exception
                Return aDefault
            End Try

        End Function

        Public Shared Function ToByte(aVariant As Object, aDefault As Byte, aMinVal As Byte?, aMaxVal As Byte?) As Byte
            If aVariant Is Nothing Then Return aDefault
            If Not TVALUES.IsNumber(aVariant) Then Return aDefault
            Dim _rVal As Byte
            Try

                Dim lVal As Integer = Convert.ToInt32(aVariant)
                If lVal <= Byte.MinValue Then
                    _rVal = 0
                ElseIf lVal > Byte.MaxValue Then
                    _rVal = 255
                Else
                    _rVal = Convert.ToByte(lVal)
                End If
            Catch ex As Exception
                _rVal = aDefault
            Finally
                If aMinVal.HasValue Then
                    If _rVal < aMinVal Then _rVal = aMinVal.Value
                End If
                If aMaxVal.HasValue Then
                    If _rVal > aMaxVal Then _rVal = aMaxVal.Value
                End If
            End Try


            Return _rVal

        End Function
        Public Shared Function ToAngle(aVariant As Object, Optional aDefault As Object = Nothing, Optional bInputRadians As Boolean = False, Optional bOutputRadians As Object = Nothing, Optional aPrecis As Integer = 5) As Double
            'On Error Resume Next
            Dim _rVal As Double = TVALUES.ToDouble(aVariant, False, aDefault, aPrecis:=aPrecis)
            Dim bRadsOut As Boolean = TVALUES.ToBoolean(bOutputRadians, bInputRadians)
            _rVal = TVALUES.NormAng(_rVal, bInputRadians)
            If bRadsOut And Not bInputRadians Then
                _rVal = dxfMath.Deg2Rad(_rVal)
            ElseIf Not bRadsOut And bInputRadians Then
                _rVal = dxfMath.Rad2Deg(_rVal)
            End If
            Return _rVal
        End Function
        Public Shared Function ToDouble(aVariant As Object, Optional bAbsoluteVal As Boolean = False, Optional aDefault As Object = Nothing, Optional aPrecis As Integer? = Nothing, Optional aMinVal As Double? = Nothing, Optional aMaxVal As Double? = Nothing, Optional aValueControl As mzValueControls = mzValueControls.None) As Double
            Dim _rVal As Double
            'On Error Resume Next
            Dim bDefPassed As Boolean
            Dim bValid As Boolean
            _rVal = TVALUES.To_DBL(aVariant, rValidInput:=bValid, bAbsVal:=bAbsoluteVal, aPrecis:=aPrecis)
            If Not bValid Then
                bDefPassed = aDefault IsNot Nothing
                _rVal = IIf(Not bDefPassed, 0, TVALUES.To_DBL(aDefault, bAbsVal:=bAbsoluteVal, aPrecis:=aPrecis))
            End If
            If aMinVal IsNot Nothing Then
                If aMinVal.HasValue Then
                    If _rVal < aMinVal.Value Then _rVal = aMinVal.Value
                End If

            End If
            If aMaxVal IsNot Nothing Then
                If aMaxVal.HasValue Then
                    If _rVal > aMaxVal.Value Then _rVal = aMaxVal.Value
                End If
            End If
            If aValueControl > mzValueControls.None Then
                _rVal = TVALUES.AppyValueControl(_rVal, aValueControl)
            End If
            Return _rVal
        End Function
        Public Shared Function ToInteger(aVariant As Object, Optional bAbsoluteVal As Boolean = False, Optional aDefault As Object = Nothing, Optional aMinVal As Integer? = Nothing, Optional aMaxVal As Integer? = Nothing, Optional aValueControl As mzValueControls = mzValueControls.None) As Integer
            Dim _rVal As Integer
            Dim iDef As Integer = 0
            Dim bDefPassed As Boolean
            If aDefault IsNot Nothing Then
                If TVALUES.IsNumber(aDefault) Then
                    iDef = TVALUES.To_INT(aDefault)
                    bDefPassed = True
                End If
            End If
            _rVal = TVALUES.To_INT(aVariant, iDef)
            If bAbsoluteVal Then _rVal = Math.Abs(_rVal)
            If aMinVal IsNot Nothing Then
                If aMinVal.HasValue Then
                    If _rVal < aMinVal.Value Then _rVal = aMinVal.Value
                End If

            End If
            If aMaxVal IsNot Nothing Then
                If aMaxVal.HasValue Then
                    If _rVal > aMaxVal.Value Then _rVal = aMaxVal.Value
                End If
            End If
            If aValueControl > mzValueControls.None Then
                If bDefPassed Then
                    _rVal = TVALUES.AppyValueControl(_rVal, aValueControl, iDef)
                Else
                    _rVal = TVALUES.AppyValueControl(_rVal, aValueControl)
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function ToLong(aVariant As Object, Optional bAbsoluteVal As Boolean = False, Optional aDefault As Object = Nothing, Optional aMinVal As Object = Nothing, Optional aMaxVal As Object = Nothing, Optional aValueControl As mzValueControls = mzValueControls.None) As Long
            Dim _rVal As Long
            Dim iDef As Long = 0
            Dim bDefPassed As Boolean
            If aDefault IsNot Nothing Then
                If TVALUES.IsNumber(aDefault) Then
                    iDef = TVALUES.To_INT(aDefault)
                    bDefPassed = True
                End If
            End If
            _rVal = TVALUES.To_INT(aVariant, iDef)
            If bAbsoluteVal Then _rVal = Math.Abs(_rVal)
            If aMinVal IsNot Nothing Then
                Dim iMin As Long = TVALUES.To_LNG(aMinVal, _rVal)
                If _rVal < iMin Then _rVal = iMin
            End If
            If aMaxVal IsNot Nothing Then
                Dim iMax As Long = TVALUES.To_LNG(aMaxVal, _rVal)
                If _rVal > iMax Then _rVal = iMax
            End If
            If aValueControl > mzValueControls.None Then
                If bDefPassed Then
                    _rVal = TVALUES.AppyValueControl(_rVal, aValueControl, iDef)
                Else
                    _rVal = TVALUES.AppyValueControl(_rVal, aValueControl)
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function ToSingle(aVariant As Object, Optional bAbsoluteVal As Boolean = False, Optional aDefault As Object = Nothing, Optional aPrecis As Integer? = Nothing, Optional aMinVal As Object = Nothing, Optional aMaxVal As Object = Nothing, Optional aValueControl As mzValueControls = mzValueControls.None) As Single
            Dim _rVal As Single
            'On Error Resume Next
            Dim bDefPassed As Boolean
            Dim bValid As Boolean
            _rVal = TVALUES.To_SNG(aVariant, bAbsoluteVal, aPrecis, bValid)
            If Not bValid Then
                aDefault = TVALUES.To_SNG(aDefault, bAbsoluteVal, aPrecis, bDefPassed)
                If bDefPassed Then _rVal = aDefault
            End If
            If aMinVal IsNot Nothing Or aMaxVal IsNot Nothing Then
                If aMinVal IsNot Nothing Then
                    aMinVal = TVALUES.To_SNG(aMinVal, bAbsVal:=False, aPrecis:=Nothing, bValid)
                    If bValid And _rVal < aMinVal Then _rVal = aMinVal
                End If
                If aMaxVal IsNot Nothing Then
                    aMaxVal = TVALUES.To_SNG(aMaxVal, bAbsVal:=False, aPrecis:=Nothing, bValid)
                    If bValid And _rVal > aMaxVal Then _rVal = aMaxVal
                End If
            End If
            If aValueControl > mzValueControls.None Then
                _rVal = TVALUES.AppyValueControl(_rVal, aValueControl)
            End If
            Return _rVal
        End Function
        Public Shared Function ToNumberStr(aVariant As Object, Optional aValueControl As mzValueControls = mzValueControls.None, Optional aMaxWholeNumberLength As Integer = -1, Optional aPrecis As Integer = -1, Optional aMinVal As Object = Nothing, Optional aMaxVal As Object = Nothing) As String

            If aVariant Is Nothing Then aVariant = "0"

            Dim aStr As String = String.Empty
            Dim aWhl As String
            Dim aDec As String
            Dim i As Integer
            Dim bStr As String
            Dim aChr As Integer
            Dim bNeg As Boolean
            Dim ac As String
            Dim aDbl As Double
            Dim mx As Double
            Dim mn As Double
            mx = Double.MaxValue
            mn = -mx
            If aMinVal IsNot Nothing Then mn = TVALUES.To_DBL(aMinVal)
            If aMaxVal IsNot Nothing Then mx = TVALUES.To_DBL(aMaxVal)
            aStr = aVariant.ToString().Trim()
            aWhl = "0"
            aDec = "0"
            If aStr <> "" Then

                If aStr.Contains(".") Then
                    aWhl = dxfUtils.LeftOf(aStr, ".")
                    aDec = dxfUtils.RightOf(aStr, ".")
                Else
                    aWhl = aStr
                End If
                If aWhl.StartsWith("-") Then
                    bNeg = aValueControl <> mzValueControls.Positive
                End If
                bStr = ""
                For i = 1 To aWhl.Length
                    ac = Mid(aWhl, i, 1)
                    aChr = Asc(ac)
                    If aChr < 48 Or aChr > 57 Then
                        ac = ""
                    Else
                        bStr += ac
                        If aMaxWholeNumberLength > 0 Then
                            If bStr.Length = aMaxWholeNumberLength Then Exit For
                        End If
                    End If
                Next i
                aWhl = bStr
                If aWhl = "" Then aWhl = "0"
                bStr = ""
                For i = 1 To aDec.Length
                    ac = Mid(aDec, i, 1)
                    aChr = Asc(ac)
                    If aChr < 48 Or aChr > 57 Then
                        ac = ""
                    Else
                        bStr += ac
                    End If
                Next i
                aDec = bStr
                If aDec = "" Then aDec = "0"
            End If
            If aMaxWholeNumberLength > 0 Then
                If aWhl.Length > aMaxWholeNumberLength Then aWhl = aWhl.Substring(0, aMaxWholeNumberLength)
            End If
            aDbl = TVALUES.ToDouble($"{aWhl}.{aDec}", False, aPrecis:=aPrecis)
            If bNeg Then aDbl = -aDbl
            If aDbl < mn Then aDbl = mn
            If aDbl > mx Then aDbl = mx
            Return aDbl.ToString()

        End Function
        Public Shared Function To_Handle(aHandle As Integer) As String
            Try
                If aHandle < 0 Then Return "0"
                Return Hex(aHandle)
            Catch ex As Exception
                Return "0"
            End Try
        End Function
        Public Shared Function HexToInteger(aHexString As String, Optional aDefault As Integer = 0) As Integer
            Try
                If String.IsNullOrWhiteSpace(aHexString) Then Return aDefault
                aHexString = aHexString.ToUpper().Trim
                If aHexString.StartsWith("&H") Then aHexString = aHexString.Substring(2, aHexString.Length - 2)
                Dim _rVal As Integer = Convert.ToInt32($"{aHexString}", 16)
                Return _rVal
            Catch ex As Exception
                Return aDefault
            End Try
        End Function
        Public Shared Function HexToDouble(aHexString As String, Optional aDefault As Integer = 0) As Double
            Try
                Dim iVal As Integer = HexToInteger(aHexString, aDefault)
                Return Convert.ToDouble(iVal)
            Catch ex As Exception
                Return aDefault
            End Try
        End Function


        Public Shared Function To_DBL(aVariant As Object, Optional bAbsVal As Boolean = False, Optional aPrecis As Integer? = Nothing, Optional aDefault As Double = 0.0) As Double
            Dim rValidInput As Boolean = False
            Return To_DBL(aVariant, rValidInput, bAbsVal, aPrecis, aDefault)
        End Function
        Public Shared Function To_DBL(aVariant As Object, ByRef rValidInput As Boolean, bAbsVal As Boolean, Optional aPrecis As Integer? = Nothing, Optional aDefault As Double = 0.0) As Double
            Dim _rVal As Double = aDefault
            Dim prec As Integer = -1
            If aPrecis.HasValue Then prec = TVALUES.LimitedValue(aPrecis.Value, -1, 15)

            rValidInput = aVariant IsNot Nothing
            Try

                If rValidInput Then
                    If TypeOf aVariant Is Double Then
                        _rVal = DirectCast(aVariant, Double)
                        If prec >= 0 Then _rVal = Math.Round(_rVal, prec)
                        Return _rVal
                    End If

                    rValidInput = TVALUES.IsNumber(aVariant)
                    If rValidInput Then
                        Dim dval As Double
                        Try
                            dval = Convert.ToDouble(aVariant)
                        Catch ex As Exception
                            dval = aDefault
                            rValidInput = False
                        End Try
                        _rVal = dval


                    End If

                End If

                Return _rVal
            Catch ex As Exception
                rValidInput = True
                _rVal = aDefault
            Finally
                If Double.IsNaN(_rVal) Then
                    rValidInput = False
                    _rVal = aDefault
                End If
                If bAbsVal Then _rVal = Math.Abs(_rVal)
                If prec >= 0 Then
                    _rVal = Math.Round(_rVal, prec)
                End If
            End Try
            Return _rVal
        End Function
        Public Shared Function To_STR(aVariant As Object, Optional aDefault As String = "", Optional bTrim As Boolean = False, Optional bReturnDefaultForNullString As Boolean = True) As String
            Try
                Dim _rVal As String = String.Empty
                If aDefault Is Nothing Then aDefault = ""
                If aVariant Is Nothing Then
                    _rVal = aDefault
                    If bTrim Then _rVal = _rVal.Trim()
                    Return _rVal
                Else
                    _rVal = aVariant.ToString
                    If bTrim Then _rVal = _rVal.Trim()
                    If bReturnDefaultForNullString And _rVal = String.Empty Then
                        _rVal = aDefault
                        If bTrim Then _rVal = _rVal.Trim()
                    End If
                    Return _rVal
                End If
            Catch ex As Exception
                Return aDefault
            End Try
        End Function
        Public Shared Function To_INT(aVariant As Object, Optional aDefault As Integer = 0) As Integer
            Try
                If aVariant Is Nothing Then Return aDefault
                If Not TVALUES.IsNumber(aVariant) Then
                    If TypeOf aVariant Is String Then
                        If aVariant.ToString().StartsWith("&H") Then
                            Return HexToInteger(aVariant.ToString())
                        Else
                            Return aDefault
                        End If
                    End If
                    If TypeOf aVariant Is IntPtr Then
                        Dim ptr As IntPtr = DirectCast(aVariant, IntPtr)
                        Return ptr.ToInt32
                    Else
                        Return aDefault
                    End If



                    Return aDefault
                End If
                Dim lVal As Long = Convert.ToInt64(aVariant)
                If Math.Abs(lVal) <= Integer.MaxValue Then
                    Return Convert.ToInt32(lVal)
                Else
                    If lVal < 0 Then Return -Integer.MaxValue Else Return Integer.MaxValue
                End If
            Catch ex As Exception
                Return aDefault
            End Try
        End Function
        Public Shared Function To_LNG(aVariant As Object, Optional aDefault As Long = 0) As Long
            Try
                If aVariant Is Nothing Then Return aDefault
                If Not TVALUES.IsNumber(aVariant) Then
                    If TypeOf aVariant Is String Then
                        If (aVariant.ToString().StartsWith("&H")) Then
                            Return Convert.ToInt64(HexToInteger(aVariant.ToString()))
                        Else
                            Return aDefault
                        End If
                    End If
                    If TypeOf aVariant Is IntPtr Then
                        Dim ptr As IntPtr = DirectCast(aVariant, IntPtr)
                        Return ptr.ToInt64
                    Else
                        Return aDefault
                    End If



                    Return aDefault
                End If
                Dim lVal As Long = Convert.ToInt64(aVariant)
                If Math.Abs(lVal) <= Long.MaxValue Then
                    Return lVal
                Else
                    Return aDefault
                End If
            Catch ex As Exception
                Return aDefault
            End Try
        End Function
        Public Shared Function To_SNG(aVariant As Object, Optional bAbsVal As Boolean = False, Optional aPrecis As Integer? = Nothing, Optional aDefault As Single = 0.0) As Single
            Dim rValidInput As Boolean = False
            Return To_SNG(aVariant, bAbsVal, aPrecis, rValidInput, aDefault)
        End Function
        Public Shared Function To_SNG(aVariant As Object, bAbsVal As Boolean, aPrecis As Integer?, ByRef rValidInput As Boolean, Optional aDefault As Single = 0.0) As Single

            Dim _rVal As Single = aDefault
            Dim prec As Integer = -1
            If aPrecis IsNot Nothing Then
                If aPrecis.HasValue Then prec = TVALUES.LimitedValue(aPrecis.Value, -1, 15)
            End If
            rValidInput = aVariant IsNot Nothing
            Try

                If rValidInput Then
                    If TypeOf aVariant Is Single Then
                        _rVal = DirectCast(aVariant, Single)
                        If prec >= 0 Then _rVal = Math.Round(_rVal, prec)
                        Return _rVal
                    End If

                    rValidInput = TVALUES.IsNumber(aVariant)
                    If rValidInput Then
                        Dim dval As Single
                        Try
                            dval = Convert.ToSingle(aVariant)
                        Catch ex As Exception
                            dval = aDefault
                            rValidInput = False
                        End Try
                        _rVal = dval


                    End If

                End If

                Return _rVal
            Catch ex As Exception
                rValidInput = True
                _rVal = aDefault
            Finally
                If Single.IsNaN(_rVal) Then
                    rValidInput = False
                    _rVal = aDefault
                End If
                If bAbsVal Then _rVal = Math.Abs(_rVal)
                If prec >= 0 Then _rVal = Math.Round(_rVal, prec)
            End Try
            Return _rVal


        End Function
        Public Shared Function NormAng(aAngle As Double, Optional bInRadians As Boolean = False, Optional ThreeSixtyEqZero As Boolean = False, Optional bReturnPosive As Boolean = True) As Double
            Dim _rVal As Double
            '#1the angle to normalize
            '#2flag indicating if the passed value is in radians
            '#3flag to return 360 as 0
            '^used to convert an angle to a positive counterclockwise value <= 360 or 2 * pi
            'if radians are passed radians are returned
            Try
                If (Double.IsNaN(aAngle)) Then
                    aAngle = 0
                End If
                Dim aSgn As Integer
                Dim aMax As Double
                If aAngle < 0 Then aSgn = -1 Else aSgn = 1
                aAngle = Math.Abs(aAngle)
                aAngle = Math.Round(aAngle, 6)
                If bInRadians Then
                    aMax = 2 * Math.PI
                Else
                    aMax = 360
                End If
                'first get it down to less than 360
                Do Until aAngle <= aMax
                    aAngle -= aMax
                Loop
                If ThreeSixtyEqZero And aAngle = aMax Then aAngle = 0
                _rVal = aSgn * aAngle
                If bReturnPosive And aSgn = -1 Then
                    _rVal = aMax + _rVal
                End If
                '    If bInRadians Then NormAng = (NormAng * PI) / 180
            Catch ex As Exception
                _rVal = aAngle
                'If bInRadians Then _rVal *= Math.PI / 180
            End Try
            Return _rVal
Err:
            Return _rVal
        End Function
        Public Shared Function AppyValueControl(aValue As Object, aValueControl As mzValueControls, Optional aDefault As Object = Nothing) As Object
            Dim rViolates As Boolean = False
            Return AppyValueControl(aValue, aValueControl, aDefault, rViolates)
        End Function
        Public Shared Function AppyValueControl(aValue As Object, aValueControl As mzValueControls, aDefault As Object, ByRef rViolates As Boolean) As Object
            Dim _rVal As Object = Nothing
            rViolates = False
            _rVal = aValue
            If aValueControl <= mzValueControls.None Then Return _rVal
            If aValue Is Nothing Then Return _rVal
            Dim bDefPassed As Boolean = aDefault IsNot Nothing
            'On Error Resume Next
            Dim defval As Double = IIf(bDefPassed, TVALUES.ToDouble(aDefault), 0)
            Dim dval As Double = TVALUES.To_DBL(aValue)
            Select Case aValueControl
                Case mzValueControls.Positive
                    If dval < 0 Then
                        If bDefPassed Then
                            _rVal = Math.Abs(defval)
                        Else
                            _rVal = Math.Abs(dval)
                        End If
                    End If
                Case mzValueControls.PositiveNonZero
                    If dval <= 0 Then
                        If bDefPassed Then
                            _rVal = Math.Abs(defval)
                        Else
                            _rVal = Math.Abs(dval)
                        End If
                        If _rVal = 0 Then _rVal = 1
                    End If
                Case mzValueControls.Negative
                    If dval > 0 Then
                        If bDefPassed Then
                            _rVal = -Math.Abs(defval)
                        Else
                            _rVal = -Math.Abs(dval)
                        End If
                    End If
                Case mzValueControls.NegativeNonZero
                    If dval <= 0 Then
                        If bDefPassed Then
                            _rVal = -Math.Abs(defval)
                        Else
                            _rVal = -Math.Abs(dval)
                        End If
                        If _rVal = 0 Then _rVal = 1
                    End If
                Case mzValueControls.NonZero
                    If dval = 0 Then
                        If bDefPassed Then
                            _rVal = defval
                            If _rVal = 0 Then _rVal = 1
                        End If
                    End If
            End Select
            Return _rVal
        End Function
        Public Shared Function LimitedValue(aInput As Integer, aMin As Integer, aMax As Integer, Optional aDefault As Integer? = Nothing) As Integer
            Dim defval As Integer = 0
            If aDefault IsNot Nothing Then
                If aDefault.HasValue Then defval = aDefault.Value
            End If

            Dim _rVal As Integer = aInput
            TVALUES.SortTwoValues(True, aMin, aMax)

            If _rVal < aMin Then
                If aDefault Is Nothing Then
                    _rVal = aMin
                Else
                    _rVal = defval
                End If
            End If
            If _rVal > aMax Then
                If aDefault Is Nothing Then
                    _rVal = aMax
                Else
                    _rVal = defval
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function LimitedValue(aInput As Double, aMin As Double, aMax As Double, Optional aDefault As Double? = Nothing) As Double
            Dim _rVal As Double = aInput
            TVALUES.SortTwoValues(True, aMin, aMax)
            If _rVal < aMin Then
                If aDefault Is Nothing Then
                    _rVal = aMin
                Else
                    _rVal = TVALUES.To_DBL(aDefault, aDefault:=aMin)
                End If
            End If
            If _rVal > aMax Then
                If aDefault Is Nothing Then
                    _rVal = aMax
                Else
                    If aDefault.HasValue Then
                        _rVal = aDefault.Value
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function IsNumber(aVariant As Object) As Boolean
            If aVariant Is Nothing Then Return False
            Dim vtype As Type = aVariant.GetType()
            If vtype.IsEnum Then
                Return True
            End If
            If TypeOf aVariant Is Boolean Then Return False
            If TypeOf aVariant Is Double Or TypeOf (aVariant) Is Integer Or TypeOf (aVariant) Is Single Or TypeOf (aVariant) Is Int16 Or TypeOf (aVariant) Is Int32 Or TypeOf (aVariant) Is Int64 Or TypeOf (aVariant) Is Byte Then
                Return True
            End If
            If TypeOf (aVariant) Is String Then
                Dim istr As String = TryCast(aVariant, String)
                If Not String.IsNullOrWhiteSpace(istr) Then
                    Return IsNumeric(istr)
                Else
                    Return False
                End If
            End If
            If Not String.IsNullOrWhiteSpace(aVariant.ToString) Then
                Return IsNumeric(aVariant)
            Else
                Return False
            End If
        End Function
        Public Shared Function IsBoolean(aVariant As Object) As Boolean
            If aVariant Is Nothing Then Return False
            Dim _rVal As Boolean = TypeOf (aVariant) Is Boolean
            If _rVal Then Return _rVal
            If TypeOf (aVariant) Is String Then
                _rVal = TLISTS.Contains(aVariant, "True,False,Yes,No,T,F")
            End If
            Return _rVal
        End Function
        Public Shared Function GetTypeName(aObject As Object) As String
            Dim sval As String = String.Empty
            Try
                sval = aObject.GetType.ToString()
                If sval.Contains(".") Then
                    sval = dxfUtils.RightOf(sval, ".", bFromEnd:=True)
                End If
            Catch ex As Exception
            End Try
            Return sval
        End Function
        Public Shared Function ObliqueAngle(aAngle As Double) As Double

            Dim _rVal As Double = Math.Round(aAngle, 2)
            If _rVal = 0 Then Return 0
            If _rVal < 0 Then
                Do While Math.Abs(_rVal) >= 360
                    _rVal += 360
                Loop
                _rVal = 360 + _rVal
            Else
                Do While _rVal >= 360
                    _rVal -= 360
                Loop
            End If
            If _rVal <= 180 Then
                If _rVal > 85 Then _rVal = 85
            Else
                If _rVal < 275 Then _rVal = 275
            End If
            Return _rVal
        End Function

#End Region 'Shared Methods
#Region "Operators"
        Public Shared Narrowing Operator CType(aValues As TVALUES) As Collection
            Dim _rVal As New Collection

            For i As Integer = 1 To aValues.Count
                _rVal.Add(aValues._Members(i - 1))
            Next i
            Return _rVal
        End Operator
        Public Shared Narrowing Operator CType(aValues As TVALUES) As List(Of String)

            Dim _rVal As New List(Of String)

            For i As Integer = 1 To aValues.Count
                _rVal.Add(IIf(aValues._Members(i - 1) Is Nothing, "", aValues._Members(i - 1).ToString()))
            Next i
            Return _rVal
        End Operator
        Public Shared Narrowing Operator CType(aValues As TVALUES) As ArrayList

            Dim _rVal As New ArrayList

            For i As Integer = 1 To aValues.Count
                _rVal.Add(aValues._Members(i - 1))
            Next i
            Return _rVal
        End Operator
#End Region 'Operators
    End Structure 'TVALUES
    Friend Structure TLIST
        Implements ICloneable
#Region "Members"
        Private _String As String
        Private _Delim As Char
        Private _Values As System.Collections.Generic.Dictionary(Of String, String)
#End Region 'Members


#Region "Constructors"
        Public Sub New(Optional aDelimitor As Char = ",", Optional aBaseValue As String = "")
            'init ------------------------------------
            _Values = New Dictionary(Of String, String)
            _String = ""
            _Delim = aDelimitor
            'init ------------------------------------
            If aBaseValue <> "" Then
                Dim vVals() As String = aBaseValue.Split(_Delim)
                For i As Integer = 1 To vVals.Length
                    Add(vVals(i - 1))
                Next
            End If
        End Sub
        Public Sub New(aList As TLIST)
            'init ------------------------------------
            _Values = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Dictionary(Of String, String))(aList._Values)
            _String = aList._String
            _Delim = aList._Delim
            'init ------------------------------------

        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If _Values Is Nothing Then _Values = New System.Collections.Generic.Dictionary(Of String, String)
                Return _Values.Count
            End Get
        End Property

        Public ReadOnly Property LastValue As String
            Get
                Return Item(Count)
            End Get
        End Property
        Public ReadOnly Property StringValue As String
            Get
                Return _String
            End Get
        End Property
        Public Property Delimitor As Char
            Get
                If _Delim = String.Empty Then _Delim = ","
                Return _Delim
            End Get
            Set(value As Char)
                If value <> "" Then _Delim = value
            End Set
        End Property
#End Region 'Properties
#Region " Methods"

        Public Function Clone() As TLIST
            Return New TLIST(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TLIST(Me)
        End Function

        Public Function SubList(aIndex As Integer) As String

            If aIndex <= 0 Or Count <= 0 Then Return String.Empty
            If aIndex >= Count Then Return StringValue
            Dim _rVal As String = Item(1)
            For i As Integer = 2 To aIndex
                _rVal += Delimitor & Item(i)
            Next
            Return _rVal

        End Function

        Public Function Item(aIndex As Integer) As String
            If aIndex < 1 Or aIndex > Count Then Return String.Empty
            Return _Values.ElementAt(aIndex - 1).Value
        End Function
        Public Sub Clear()
            _Values.Clear()

        End Sub
        Public Overrides Function ToString() As String
            Return StringValue
        End Function
        Public Function Add(aValue As Object, Optional aDefaultValue As String = "", Optional bSuppressTest As Boolean = False) As Boolean
            If _Values Is Nothing Then _Values = New System.Collections.Generic.Dictionary(Of String, String)
            If aValue Is Nothing Then Return False
            Dim aVal As String = TVALUES.To_STR(aValue)
            If aVal = "" Then aVal = aDefaultValue
            If String.IsNullOrEmpty(aVal) Then Return False
            aVal = aVal.Replace(Delimitor, "")
            If Not bSuppressTest And Count > 0 Then
                If Contains(aVal) Then Return False
            End If
            If _String <> "" Then _String += Delimitor
            _String += aVal
            _Values.Add(aVal.ToUpper, aVal)
            Return True
        End Function
        Public Function AddList(aList As String, Optional aDelim As String = "") As Integer
            If String.IsNullOrEmpty(aList) Then Return 0
            Dim _rVal As Integer = 0
            Dim sDelim As Char = IIf(String.IsNullOrEmpty(aDelim), Delimitor, aDelim.Trim())

            Dim sVals() As String = aList.Split(sDelim)
            For i As Integer = 1 To sVals.Length
                If Add(sVals(i - 1)) Then _rVal += 1
            Next
            Return _rVal
        End Function
        Public Function Contains(aValue As Object, Optional bReturnTrueForNullList As Boolean = False) As Boolean
            If Count <= 0 And bReturnTrueForNullList Then Return True
            If aValue Is Nothing Then Return False
            Return _Values.ContainsKey(aValue.ToString.ToUpper)
        End Function
        Public Function ContainsNumber(aValue As Object, Optional bReturnTrueForNullList As Boolean = False, Optional aPrecis As Integer = 4) As Boolean
            Dim rStartID As Integer = 0
            Return ContainsNumber(aValue, bReturnTrueForNullList, aPrecis, rStartID)
        End Function
        Public Function ContainsNumber(aValue As Object, bReturnTrueForNullList As Boolean, aPrecis As Integer, ByRef rStartID As Integer) As Boolean
            Dim _rVal As Boolean
            rStartID = 0
            If aValue Is Nothing Then Return False
            If Trim(_String) = "" Then
                If bReturnTrueForNullList Then Return True
            End If
            If Not TVALUES.IsNumber(aValue) Then Return False
            If aPrecis < 0 Then aPrecis = 0
            If aPrecis > 10 Then aPrecis = 10
            Dim aVal As String
            Dim bVal As Double
            Dim i As Integer
            Dim vList() As String
            bVal = TVALUES.ToDouble(aValue, aPrecis:=aPrecis)
            vList = aValue.ToString().Split(Delimitor)
            For i = 0 To vList.Length - 1
                aVal = vList(i)
                If TVALUES.IsNumber(aVal) Then
                    If Math.Round(TVALUES.To_DBL(aVal), aPrecis) = bVal Then
                        _rVal = True
                        rStartID = i + 1
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function Append(aSubList As Object, aDelimitor As Char) As String
            If String.IsNullOrWhiteSpace(aDelimitor.ToString()) Then aDelimitor = Delimitor
            Dim aVals As TVALUES = TLISTS.ToValues(TVALUES.To_STR(aSubList), aDelimitor, bReturnNulls:=False, bTrim:=False, bNoDupes:=False)
            If aVals.Count <= 0 Then Return _String

            For i As Integer = 1 To aVals.Count
                Add(aVals.Member(i - 1))
            Next i
            Return _String
        End Function
        Public Function ToValues(Optional aDelimitor As String = "", Optional bReturnNulls As Boolean = False, Optional bTrim As Boolean = False, Optional bNoDupes As Boolean = False, Optional bNumbersOnly As Boolean = False, Optional aPrecis As Integer? = Nothing, Optional bRemoveParens As Boolean = False, Optional aSkipList As String = "") As TVALUES
            Dim _rVal As New TVALUES(0)
            If String.IsNullOrWhiteSpace(_String) Then Return _rVal

            Dim i As Integer
            Dim sVals() As String
            Dim sVal As String

            Dim aStr As String = _String.Trim()


            Dim bSkipTest As Boolean = Not String.IsNullOrWhiteSpace(aSkipList)
            Dim bKeep As Boolean

            If bSkipTest Then aSkipList = aSkipList.Trim
            If bRemoveParens Then
                Do While aStr.StartsWith("(")
                    aStr = aStr.Substring(1, aStr.Length - 1).Trim()
                    If aStr.Length <= 0 Then Exit Do
                Loop
                Do While aStr.EndsWith(")")
                    aStr = aStr.Substring(0, aStr.Length - 1).Trim()
                    If aStr.Length <= 0 Then Exit Do
                Loop
            End If
            If aStr.Length <= 0 Then Return _rVal
            Dim delim As Char = IIf(String.IsNullOrEmpty(aDelimitor), aDelimitor = Delimitor, aDelimitor.Chars(0))
            sVals = aStr.Split(delim)
            If bNumbersOnly Then bReturnNulls = False
                For i = 0 To sVals.Length - 1
                    If bSkipTest Then
                        If Not TLISTS.Contains(i + 1, aSkipList) Then Continue For
                    End If

                sVal = sVals(i)
                If bNumbersOnly Then
                    sVal = sVal.Trim()
                    If sVal = "" Then Continue For
                    Dim dVal As Double = TVALUES.To_DBL(sVal, rValidInput:=bKeep, bAbsVal:=False, aPrecis:=aPrecis)
                    If bKeep Then _rVal.Add(dVal, bNoDupes:=bNoDupes)
                    Continue For
                End If
                If bTrim Then sVal = sVal.Trim()
                If Not bReturnNulls And String.IsNullOrWhiteSpace(sVal) Then Continue For
                _rVal.Add(sVal, bNoDupes:=bNoDupes)

            Next i

            Return _rVal
        End Function
        Public Function Remove(aValue As Object) As Boolean
            If aValue Is Nothing Then Return False
            If _String = "" Or Count <= 0 Then Return False
            Dim rVal As String = TVALUES.To_STR(aValue).Replace(Delimitor, "")
            If Not Contains(rVal) Then Return False
            _Values.Remove(rVal.ToUpper)
            RebuildString()
            Return True
        End Function
        Public Function ReduceTo(aValue As String, Optional aValueToAdd As Object = Nothing) As String
            Dim rChanged As Boolean = False
            Dim rValueWas As String = String.Empty
            Return ReduceTo(aValue, rValueWas, rChanged, aValueToAdd)
        End Function
        Public Function ReduceTo(aValue As String, ByRef rValueWas As String, ByRef rChanged As Boolean, Optional aValueToAdd As Object = Nothing) As String
            rValueWas = StringValue
            rChanged = False
            If Contains(aValue) Then
                Dim aVal As String
                For i As Integer = _Values.Count To 1 Step -1
                    aVal = _Values.ElementAt(i - 1).Value
                    If String.Compare(aVal, aValue, True) <> 0 Then
                        rChanged = True
                        _Values.Remove(_Values.ElementAt(i - 1).Key)
                    Else
                        Exit For
                    End If
                Next
            Else
            End If
            If aValueToAdd IsNot Nothing Then
                If Add(aValueToAdd) Then rChanged = True
            End If
            Return RebuildString()
        End Function
        Public Function RebuildString(Optional aValueToAdd As Object = Nothing) As String
            If _Values Is Nothing Then _Values = New System.Collections.Generic.Dictionary(Of String, String)

            _String = ""
            For i As Integer = 1 To _Values.Count
                Dim sVal As String = _Values.ElementAt(i - 1).Value
                If i > 1 Then _String += _Delim
                _String += sVal
            Next

            If aValueToAdd IsNot Nothing Then Add(aValueToAdd)
            Return _String
        End Function
#End Region ' Methods
    End Structure  'TLIST
    Friend Structure TLISTS
#Region "Shared Methods"
        Public Shared Function Add(ByRef ioList As String, aValue As Object, Optional aDefaultValue As String = "", Optional bAllowDuplicates As Boolean = False, Optional aDelimitor As String = ",", Optional aPrecis As Integer = 0, Optional bAllowNulls As Boolean = False) As Boolean
            Try
                If String.IsNullOrWhiteSpace(ioList) Then ioList = ""
                Dim aVal As String = String.Empty
                If aValue IsNot Nothing Then aVal = aValue.ToString



                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                If aPrecis > 0 Then
                    If TVALUES.IsNumber(aVal) Then
                        aVal = Math.Round(TVALUES.To_DBL(aVal), aPrecis).ToString
                    End If
                End If
                If aVal = "" And aDefaultValue IsNot Nothing Then aVal = aDefaultValue
                If aVal = "" And Not bAllowNulls Then Return False
                If Not bAllowDuplicates And ioList <> "" Then
                    If TLISTS.Contains(aVal, ioList, aDelimitor) Then Return False
                End If
                If ioList <> "" Then ioList += aDelimitor
                ioList += aVal
                Return True
            Catch
                Return False
            End Try

        End Function
        Public Shared Function Append(ByRef ioList As String, aSubList As Object, Optional bUniqueValues As Boolean = True, Optional aDelimitor As String = ",", Optional bNoNulls As Boolean = True, Optional bUpdateSourceString As Boolean = False) As String
            Dim _rVal As String = ioList
            Dim aVals As TVALUES
            aVals = TLISTS.ToValues(aSubList.ToString(), aDelimitor, Not bNoNulls, bNoDupes:=bUniqueValues)
            If aVals.Count <= 0 Then Return _rVal
            Dim i As Integer
            For i = 1 To aVals.Count
                TLISTS.Add(_rVal, aVals.Item(i), bAllowDuplicates:=Not bUniqueValues, aDelimitor:=aDelimitor, bAllowNulls:=Not bNoNulls)
            Next i
            If bUpdateSourceString Then ioList = _rVal
            Return _rVal
        End Function
        Public Shared Function Contains(aValue As Object, aList As String, Optional aDelimitor As String = ",", Optional bReturnTrueForNullList As Boolean = False) As Boolean
            Dim rStartID As Integer = 0
            Return Contains(aValue, aList, aDelimitor, bReturnTrueForNullList, rStartID)
        End Function
        Public Shared Function Contains(aValue As Object, aList As String, aDelimitor As String, bReturnTrueForNullList As Boolean, ByRef rStartID As Integer) As Boolean
            rStartID = 0
            If bReturnTrueForNullList Then
                If String.IsNullOrWhiteSpace(aList) Then Return True
            End If
            Dim sVal As String = TVALUES.To_STR(aValue, "")
            Dim lg As Integer
            Dim rmv As Integer

            If sVal = "" Then Return bReturnTrueForNullList
            If aDelimitor <> "" Then
                lg = aDelimitor.Length
                Do Until String.Compare(Left(aList, lg), aDelimitor, True) <> 0
                    aList = Right(aList, aList.Length - lg)
                    rmv += lg
                    If aList = "" Then
                        If bReturnTrueForNullList Then Return True
                    End If
                Loop
            End If
            lg = sVal.Length
            If lg = 0 Then Return aList.Contains(aDelimitor & aDelimitor, StringComparer.OrdinalIgnoreCase)
            If lg > aList.Length Then Return False
            If lg = aList.Length Then
                If String.Compare(aList, sVal, True) = 0 Then
                    rStartID = 1 + rmv
                    Return True
                Else
                    Return False
                End If
            End If
            If String.Compare(Left(aList, lg + 1), sVal & aDelimitor, True) = 0 Then
                rStartID = 1 + rmv
                Return True
            ElseIf String.Compare(Right(aList, lg + 1), aDelimitor & sVal, True) = 0 Then
                rStartID = aList.Length - lg + 1 + rmv
                Return True
            Else
                rStartID = aList.IndexOf(aDelimitor + sVal + aDelimitor, StringComparison.OrdinalIgnoreCase) + 1
                If rStartID > 0 Then
                    rStartID += 1 + rmv
                    Return True
                Else
                    Return False
                End If
            End If
        End Function
        Public Shared Function StringCompare(aList As String, bList As String, Optional aSkipList As String = "", Optional bBailOnOne As Boolean = False, Optional aDelimitor As String = ",", Optional bTrim As Boolean = True) As Boolean
            Dim rMisMatches As String = String.Empty
            Return StringCompare(aList, bList, aSkipList, bBailOnOne, rMisMatches, aDelimitor, bTrim)
        End Function
        Public Shared Function StringCompare(aList As String, bList As String, aSkipList As String, bBailOnOne As Boolean, ByRef rMisMatches As String, Optional aDelimitor As String = ",", Optional bTrim As Boolean = True) As Boolean
            Return TLISTS.ToValues(aList, aDelimitor, True, bTrim, False).CompareString(TLISTS.ToValues(bList, aDelimitor, True, bTrim, False), aSkipList, bBailOnOne, rMisMatches, aDelimitor)
        End Function
        Public Shared Function ToValues(aList As Object, Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bTrim As Boolean = False, Optional bNoDupes As Boolean = False, Optional bNumbersOnly As Boolean = False, Optional aPrecis As Integer? = Nothing, Optional bRemoveParens As Boolean = False, Optional aSkipList As String = "") As TVALUES
            Dim _rVal As New TVALUES(0)
            If aList Is Nothing Then Return _rVal
            Dim i As Integer
            Dim sVals() As String

            Dim aStr As String = aList.ToString().Trim
            If bRemoveParens Then
                Do While aStr.StartsWith("("c)
                    aStr = aStr.Substring(1, aStr.Length - 1).Trim()
                Loop
                Do While aStr.EndsWith(")"c)
                    aStr = aStr.Substring(0, aStr.Length - 1).Trim()
                Loop
            End If
            If aStr.Length <= 0 Then Return _rVal

            Dim bSkipTest As Boolean = Not String.IsNullOrWhiteSpace(aSkipList)
            Dim bKeep As Boolean

            If bSkipTest Then aSkipList = aSkipList.Trim()


            sVals = aStr.Split(aDelimitor)
            If bNumbersOnly Then bReturnNulls = False
            For i = 0 To sVals.Length - 1
                If bSkipTest Then
                    If TLISTS.Contains(i + 1, aSkipList) Then Continue For
                End If

                Dim oVal As Object = sVals(i)
                Dim sVal As String = IIf(oVal Is Nothing, "", oVal.ToString())
                If bNumbersOnly Then
                    sVal = sVal.Trim()
                    If sVal = "" Then Continue For
                    Dim dVal As Double = TVALUES.To_DBL(oVal, rValidInput:=bKeep, bAbsVal:=False, aPrecis:=aPrecis)
                    If bKeep Then _rVal.Add(dVal, bNoDupes:=bNoDupes)
                    Continue For
                End If

                If bTrim Then sVal = sVal.Trim()
                If Not bReturnNulls And String.IsNullOrWhiteSpace(sVal) Then Continue For


                _rVal.Add(sVal, bNoDupes:=bNoDupes)

            Next i

            Return _rVal
        End Function
        Public Shared Function ToStringList(aList As String, Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bTrim As Boolean = False, Optional bNoDupes As Boolean = False, Optional bUCase As Boolean = False, Optional bRemoveParens As Boolean = False, Optional aSkipList As String = "") As List(Of String)
            Dim _rVal As New List(Of String)
            If String.IsNullOrWhiteSpace(aList) Then Return _rVal

            Dim aStr As String = aList.Trim()
            Dim bSkipTest As Boolean = Not String.IsNullOrWhiteSpace(aSkipList)
            If bSkipTest Then aSkipList = aSkipList.Trim()

            If bRemoveParens Then
                Do While aStr.StartsWith("("c)
                    aStr = aStr.Substring(1, aStr.Length - 1).Trim()
                Loop
                Do While aStr.EndsWith(")"c)
                    aStr = aStr.Substring(0, aStr.Length - 1).Trim()
                Loop
            End If

            If aStr = "" Or String.IsNullOrWhiteSpace(aDelimitor) Then Return _rVal

            Dim sVals() As String = aStr.Split(aDelimitor)
            For i As Integer = 1 To sVals.Count
                If bSkipTest Then
                    If TLISTS.Contains(i, aSkipList) Then Continue For
                End If

                Dim sVal As String = sVals(i - 1)

                If bUCase Then sVal = sVal.ToUpper()
                If bTrim Then sVal = sVal.Trim()
                If Not bReturnNulls And String.IsNullOrWhiteSpace(aStr) Then Continue For
                If bNoDupes Then
                    If _rVal.Find(Function(mem) mem = sVal) >= 0 Then Continue For
                End If
                _rVal.Add(sVal)

            Next i

            Return _rVal
        End Function
        Public Shared Function ToNumericList(aList As Object, Optional aDelimitor As String = ",", Optional bReturnNullsAsZero As Boolean = False, Optional bNoDupes As Boolean = False, Optional aPrecis As Integer? = Nothing, Optional aSkipList As String = "") As List(Of Double)
            Dim _rVal As New List(Of Double)
            If aList Is Nothing Then Return _rVal
            Dim aStr As String = aList.ToString().Trim()
            Dim bSkipTest As Boolean = Not String.IsNullOrWhiteSpace(aSkipList)
            If bSkipTest Then aSkipList = aSkipList.Trim()

            Do While aStr.StartsWith("("c)
                aStr = aStr.Substring(1, aStr.Length - 1).Trim()
            Loop
            Do While aStr.EndsWith(")"c)
                aStr = aStr.Substring(0, aStr.Length - 1).Trim()
            Loop
            If aStr.Length <= 0 Then Return _rVal
            Dim delim As Char = IIf(String.IsNullOrWhiteSpace(aDelimitor), ",", aDelimitor.Chars(0))
            Dim sVals() As String = aStr.Split(delim)
            For i As Integer = 0 To sVals.Length - 1
                Dim sVal As String = sVals(i).Trim()
                If bSkipTest Then
                    If TLISTS.Contains(sVal, aSkipList) Then Continue For
                End If
                If Not bReturnNullsAsZero And sVal = "" Then Continue For
                If bReturnNullsAsZero And sVal = "" Then sVal = "0.0"

                If Not TVALUES.IsNumber(sVal) Then Continue For
                Dim inv As Boolean = False
                Dim dVal As Double = TVALUES.To_DBL(sVal, rValidInput:=inv, bAbsVal:=False, aPrecis:=aPrecis)
                If inv Then Continue For
                If bNoDupes Then
                    If _rVal.IndexOf(dVal) >= 0 Then Continue For
                End If

                _rVal.Add(dVal)

            Next i

            Return _rVal
        End Function

        Public Shared Function ToIntegerList(aList As Object, Optional aDelimitor As String = ",", Optional bReturnNullsAsZero As Boolean = False, Optional bNoDupes As Boolean = False, Optional aSkipList As List(Of Integer) = Nothing) As List(Of Integer)
            Dim _rVal As New List(Of Integer)
            If aList Is Nothing Then Return _rVal
            Dim aStr As String = aList.ToString().Trim()
            Dim bSkipTest As Boolean = aSkipList IsNot Nothing

            Do While aStr.StartsWith("("c)
                aStr = aStr.Substring(1, aStr.Length - 1).Trim()
            Loop
            Do While aStr.EndsWith(")"c)
                aStr = aStr.Substring(0, aStr.Length - 1).Trim()
            Loop
            If aStr.Length <= 0 Then Return _rVal
            Dim delim As Char = IIf(String.IsNullOrWhiteSpace(aDelimitor), ",", aDelimitor.Chars(0))
            Dim sVals() As String = aStr.Split(delim)
            For i As Integer = 0 To sVals.Length - 1
                Dim sVal As String = sVals(i).Trim()
                If Not bReturnNullsAsZero And sVal = "" Then Continue For
                If bReturnNullsAsZero And sVal = "" Then sVal = "0"

                If Not TVALUES.IsNumber(sVal) Then Continue For

                Dim iVal As Integer = TVALUES.To_INT(sVal)

                If bSkipTest Then
                    If aSkipList.IndexOf(iVal) >= 0 Then Continue For
                End If


                If bNoDupes Then
                    If _rVal.IndexOf(iVal) >= 0 Then Continue For
                End If

                _rVal.Add(iVal)

            Next i

            Return _rVal
        End Function

        Public Shared Function QuotedListToValues(aList As Object, Optional aDelimitor As String = ",", Optional bRemoveQuotes As Boolean = False) As TVALUES
            Dim _rVal As New TVALUES(0)
            If aList Is Nothing Then Return _rVal



            Dim iQt As Integer = 2
            Dim aStr As String = aList.ToString().Trim
            Dim bStr As String = String.Empty

            For i As Integer = 1 To aStr.Length
                Dim aCr As String = aStr.Substring(i - 1, 1)
                If aCr = """" Then iQt += 1
                If aCr = aDelimitor Then
                    If iQt Mod 2 <> 0 Then
                        bStr += aCr
                    Else
                        _rVal.Add(bStr)
                        bStr = ""
                        iQt = 2
                    End If
                Else
                    bStr += aCr
                End If
            Next i
            If bRemoveQuotes Then
                For i = 1 To _rVal.Count
                    bStr = _rVal.Item(i)
                    _rVal.SetItem(i, dxfUtils.StripParens(bStr, """", """"))
                Next i
            End If
            Return _rVal
        End Function
        Public Shared Function ToSubValues(aList As Object, ByRef rSubVals As TVALUES, Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bTrim As Boolean = False, Optional bNoDupes As Boolean = False, Optional bNumbersOnly As Boolean = False, Optional aPrecis As Integer? = Nothing, Optional bRemoveParens As Boolean = False) As TVALUES
            Dim _rVal As New TVALUES(0)

            Dim aStr As String = String.Empty

            rSubVals = TLISTS.ToValues(aList, aDelimitor, bReturnNulls, bTrim, bNoDupes, bNumbersOnly, aPrecis, bRemoveParens)
            If rSubVals.Count <= 0 Then Return _rVal
            For i As Integer = 1 To rSubVals.Count
                If i > 1 Then aStr += aDelimitor
                aStr += rSubVals.Item(i)
                _rVal.Add(aStr)
            Next i
            Return _rVal
        End Function
        Public Shared Function Remove(ByRef ioList As String, aValue As Object, Optional aDelimitor As String = ",", Optional iOccurance As Integer = 0) As Boolean
            Dim _rVal As Boolean
            If ioList = "" Then Return _rVal
            Dim lVal As String
            Dim rVal As String
            Dim lVals() As String
            Dim rVals() As String
            Dim iVals() As Integer
            Dim i As Integer
            Dim j As Integer
            Dim rList As String
            Dim cnt As Integer
            Dim lcnt As Integer
            Dim rcnt As Integer
            Dim bKeep As Boolean
            rVal = aValue.ToString()
            rList = ioList
            If rVal = "" Then
                Do Until rList.IndexOf(aDelimitor + aDelimitor, StringComparison.OrdinalIgnoreCase) < 0
                    rList = rList.Replace(aDelimitor + aDelimitor, aDelimitor)
                Loop
            Else
                lVals = ioList.Split(aDelimitor)
                lcnt = lVals.Length - 1
                rVals = rVal.Split(aDelimitor)
                rcnt = rVals.Length - 1
                ReDim iVals(0 To lcnt)
                If iOccurance > 0 Then
                    For i = 0 To lcnt
                        If iVals(i) = 0 Then
                            cnt = 1
                            lVal = lVals(i)
                            iVals(i) = cnt
                            For j = i + 1 To lcnt
                                If String.Compare(lVals(j), lVal, True) = 0 Then
                                    cnt += 1
                                    iVals(j) = cnt
                                End If
                            Next j
                        End If
                    Next i
                End If
                For i = 0 To lcnt
                    lVal = lVals(i)
                    bKeep = True
                    For j = 0 To rcnt
                        rVal = rVals(j)
                        If String.Compare(lVal, rVal, True) = 0 Then
                            If iOccurance > 0 Then
                                If iVals(i) = iOccurance Then
                                    bKeep = False
                                    Exit For
                                End If
                            Else
                                bKeep = False
                                Exit For
                            End If
                        End If
                    Next j
                    If Not bKeep Then _rVal = True
                    If bKeep Then TLISTS.Add(rList, lVal, bAllowDuplicates:=True, aDelimitor:=aDelimitor, bAllowNulls:=True)
                Next i
            End If
            ioList = rList
            Return _rVal
        End Function
        Public Shared Function BuildPath(Optional aStep As String = "", Optional bStep As String = "", Optional cStep As String = "", Optional dStep As String = "", Optional eStep As String = "", Optional aDelimitor As String = ".") As String
            Dim _rVal As String = String.Empty
            If aStep <> "" Then TLISTS.Add(_rVal, aStep, bAllowDuplicates:=True, aDelimitor:=aDelimitor)
            If bStep <> "" Then TLISTS.Add(_rVal, bStep, bAllowDuplicates:=True, aDelimitor:=aDelimitor)
            If cStep <> "" Then TLISTS.Add(_rVal, cStep, bAllowDuplicates:=True, aDelimitor:=aDelimitor)
            If dStep <> "" Then TLISTS.Add(_rVal, dStep, bAllowDuplicates:=True, aDelimitor:=aDelimitor)
            If eStep <> "" Then TLISTS.Add(_rVal, eStep, bAllowDuplicates:=True, aDelimitor:=aDelimitor)
            Return _rVal
        End Function
        Public Shared Function ToList(aList As String, Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            If String.IsNullOrWhiteSpace(aList) Then Return _rVal
            Dim sVals() As String = aList.Split(aDelimitor)
            Dim sVal As String

            For i As Integer = 0 To sVals.Length - 1
                sVal = sVals(i)
                If bReturnNulls Or (Not bReturnNulls And Not String.IsNullOrWhiteSpace(sVal)) Then
                    _rVal.Add(sVal)
                End If
            Next i
            Return _rVal
        End Function
#End Region 'Shared Methods
    End Structure  'TLISTS
#End Region 'Structures

End Namespace
