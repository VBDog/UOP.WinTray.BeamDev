Imports System.Windows.Controls
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures


    Friend Structure TDICTIONARYENTRY
        Implements ICloneable
        Public Name As String
        Public Handle As String
        Public Index As Integer
#Region "Constructors"
        Public Sub New(Optional aName As String = "", Optional aHandle As String = "")
            'init =============
            Name = aName
            Handle = aHandle
            Index = 0
            'init =============

        End Sub

        Public Sub New(aEntry As TDICTIONARYENTRY)
            'init =============
            Name = aEntry.Name
            Handle = aEntry.Handle
            Index = aEntry.Index
            'init =============
        End Sub
        Public Sub New(aEntry As dxoDictionaryEntry)
            'init =============
            Name = String.Empty
            Handle = String.Empty
            Index = 0
            'init =============
            If aEntry Is Nothing Then Return
            Name = aEntry.Name
            Handle = aEntry.Handle
            Index = aEntry.Index
        End Sub
#End Region 'Constructors
#Region "Methods"
        Public Function Clone() As TDICTIONARYENTRY
            Return New TDICTIONARYENTRY(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDICTIONARYENTRY(Me)
        End Function
        Public Overrides Function ToString() As String
            Return $"Dictionary Entry[Name:{Name } Handle:{ Handle }]"
        End Function

#End Region 'Methods

#Region "Shared Methods"
        Public Shared ReadOnly Property Null As TDICTIONARYENTRY
            Get
                Return New TDICTIONARYENTRY("", "") With {.Index = -1}
            End Get

        End Property
#End Region 'Shared Methods
    End Structure
    Friend Structure TDICTIONARYENTRIES
        Implements ICloneable
        Private _NameGC As Integer
        Private _HandleGC As Integer
        Private _Members() As TDICTIONARYENTRY
        Private _Init As Boolean
#Region "Constructors"
        Public Sub New(aNameGC As Integer, aHandleGC As Integer)
            'init ------------------
            _NameGC = aNameGC
            _HandleGC = aHandleGC
            _Init = True
            ReDim _Members(-1)
            'init ------------------

            _Init = True
        End Sub

        Public Sub New(aDictionary As TDICTIONARYENTRIES)
            'init ------------------
            _NameGC = aDictionary._NameGC
            _HandleGC = aDictionary._HandleGC
            _Init = True
            ReDim _Members(-1)
            'init ------------------
            If aDictionary._Init Then
                _Members = aDictionary._Members.Clone()
            End If

        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then Clear()
                Return _Members.Count
            End Get
        End Property
        Public Property NameGroupCode As Integer
            Get
                Return _NameGC
            End Get
            Set(value As Integer)
                _NameGC = value
            End Set
        End Property
        Public Property HandleGroupCode As Integer
            Get
                Return _HandleGC
            End Get
            Set(value As Integer)
                _HandleGC = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As TDICTIONARYENTRY
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TDICTIONARYENTRY.Null
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TDICTIONARYENTRY)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Friend Function Properties(Optional bSuppressNames As Boolean = False) As TPROPERTIES
            Dim _rVal As New TPROPERTIES("Entries")
            Dim aEntry As TDICTIONARYENTRY
            Dim pname As String = String.Empty
            For i As Integer = 1 To Count
                aEntry = Item(i)
                If Not bSuppressNames Then
                    If aEntry.Name <> "" Then _rVal.Add(New TPROPERTY(_NameGC, aEntry.Name, $"Name_{ i}", dxxPropertyTypes.dxf_String))
                End If
                If aEntry.Handle <> "" Then
                    pname = aEntry.Name
                    If pname = "" Then pname = $"Handle_{ i}"
                    _rVal.Add(New TPROPERTY(_HandleGC, aEntry.Handle, pname, dxxPropertyTypes.Pointer))
                End If
            Next
            Return _rVal
        End Function
        Public Function Clone() As TDICTIONARYENTRIES
            Return New TDICTIONARYENTRIES(Me)

        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDICTIONARYENTRIES(Me)
        End Function
        Public Overrides Function ToString() As String
            Return $"TDICTIONARYENTRIES[{ Count }]"
        End Function
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub
        Public Function Add(aEntry As TDICTIONARYENTRY) As Boolean
            If aEntry.Handle = "" And aEntry.Name = "" Then Return False
            If aEntry.Handle = "0" Then Return False
            Array.Resize(_Members, Count + 1)
            aEntry.Index = _Members.Count
            _Members(_Members.Count - 1) = aEntry
            Return True
        End Function
        Public Function Remove(aIndex As Integer) As TDICTIONARYENTRY
            Dim _rVal As TDICTIONARYENTRY = TDICTIONARYENTRY.Null
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            _rVal = Item(aIndex)
            Dim cnt As Integer = Count
            Dim newmems(0 To Count - 1) As TDICTIONARYENTRY
            Dim idx As Integer = 0
            For i As Integer = 1 To Count
                If i <> aIndex Then
                    newmems(idx) = _Members(i - 1)
                    idx += 1
                End If
            Next i
            _Members = newmems
            Return _rVal
        End Function
        Public Function Add(aName As String, aHandle As String) As Boolean
            Dim rEntry As TDICTIONARYENTRY = TDICTIONARYENTRY.Null
            Return Add(aName, aHandle, rEntry)
        End Function
        Public Function Add(aName As String, aHandle As String, ByRef rEntry As TDICTIONARYENTRY) As Boolean
            rEntry = New TDICTIONARYENTRY
            If aHandle = "" And aName = "" Then Return False
            If aHandle = "" Then Return aHandle = "0"
            If aHandle = "0" Then Return False
            Dim _rVal As Boolean = False
            If TryGet(aName, aHandle, rEntry) Then
                rEntry.Name = aName
                rEntry.Handle = aHandle
                SetItem(rEntry.Index, rEntry)
                _rVal = False
            Else
                _rVal = Add(New TDICTIONARYENTRY(aName, aHandle))
                If _rVal Then rEntry = Item(Count)
            End If
            Return _rVal
        End Function
        Public Function TryGet(aNameOrHandle As String, ByRef rEntry As TDICTIONARYENTRY) As Boolean
            rEntry = TDICTIONARYENTRY.Null
            If String.IsNullOrEmpty(aNameOrHandle) Then Return False
            For i As Integer = 1 To Count
                If String.Compare(_Members(i - 1).Name, aNameOrHandle, ignoreCase:=True) = 0 Or String.Compare(_Members(i - 1).Handle, aNameOrHandle, ignoreCase:=True) = 0 Then
                    rEntry = _Members(i - 1)
                    rEntry.Index = i
                    Return True
                End If
            Next
            Return False
        End Function
        Public Function TryGet(aName As String, aHandle As String, ByRef rEntry As TDICTIONARYENTRY) As Boolean
            rEntry = TDICTIONARYENTRY.Null
            Dim namePassed As Boolean = Not String.IsNullOrEmpty(aName)
            Dim handlePassed As Boolean = Not String.IsNullOrEmpty(aHandle)
            If Not namePassed And Not handlePassed Then Return False
            For i As Integer = 1 To Count
                If namePassed And handlePassed Then
                    If String.Compare(_Members(i - 1).Name, aName, ignoreCase:=True) = 0 Or String.Compare(_Members(i - 1).Handle, aHandle, ignoreCase:=True) = 0 Then
                        rEntry = _Members(i - 1)
                        rEntry.Index = i
                        Return True
                    End If
                ElseIf namePassed Then
                    If String.Compare(_Members(i - 1).Name, aName, ignoreCase:=True) = 0 Then
                        rEntry = _Members(i - 1)
                        rEntry.Index = i
                        Return True
                    End If
                ElseIf handlePassed Then
                    If String.Compare(_Members(i - 1).Handle, aHandle, ignoreCase:=True) = 0 Then
                        rEntry = _Members(i - 1)
                        rEntry.Index = i
                        Return True
                    End If
                End If
            Next
            Return False
        End Function
#End Region 'Methods
    End Structure

    Friend Structure TDICTIONARY_CHAR
        Implements ICloneable
        Public Name As String
        Private _Keys() As String
        Friend _Vals() As TCHAR
        Private _Init As Boolean
#Region "Constructors"
        Public Sub New(aName As String)
            'init ------------------
            Name = aName
            _Init = True
            ReDim _Keys(-1)
            ReDim _Vals(-1)
            'init ------------------
        End Sub
        Public Sub New(aDictionary As TDICTIONARY_CHAR)
            'init ------------------
            Name = aDictionary.Name
            _Init = True
            ReDim _Keys(-1)
            ReDim _Vals(-1)
            If aDictionary._Init Then
                _Keys = aDictionary._Keys.Clone()
                _Vals = aDictionary._Vals.Clone()
            End If

            'init ------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then Clear()
                Return _Vals.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As TCHAR
            '1 based
            If Not _Init Then Return New TCHAR("")
            If aIndex < 1 Or aIndex > Count Then Return New TCHAR("")
            _Vals(aIndex - 1).Index = aIndex
            Return _Vals(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TCHAR)
            If Not _Init Then Return
            If aIndex < 1 Or aIndex > Count Then Return
            _Vals(aIndex - 1) = value
            _Vals(aIndex - 1).Index = aIndex
        End Sub
        Public Function Item(aKey As String) As TCHAR
            '1 based
            If Not _Init Then Return New TCHAR("")
            Dim idx As Integer
            If Not ContainsKey(aKey, idx) Then Return New TCHAR("")
            _Vals(idx - 1).Index = idx
            Return _Vals(idx - 1)
        End Function
        Public Sub SetItem(aKey As String, value As TCHAR)
            If Not _Init Then Return
            Dim idx As Integer
            If Not ContainsKey(aKey, idx) Then Return
            _Vals(idx - 1) = value
            _Vals(idx - 1).Index = idx
        End Sub
        Public Overrides Function ToString() As String
            Return "TDICTIONARY_CHAR [" & Count & "]"
        End Function
        Public Function Remove(aIndex As Integer) As TCHAR
            Dim cnt As Integer = Count
            If aIndex < 1 Or aIndex > cnt Then Return New TCHAR("")
            Dim newKeys(0 To cnt - 1) As String
            Dim newVals(0 To cnt - 1) As TCHAR
            Dim j As Integer = 0
            Dim aMem As TCHAR
            Dim _rVal As New TCHAR("")
            For i As Integer = 1 To cnt
                aMem = _Vals(i - 1)
                If i <> aIndex Then
                    j += 1
                    newVals(j - 1) = aMem
                    newKeys(j - 1) = aMem.Key
                Else
                    _rVal = aMem
                    _rVal.Index = i
                End If
            Next i
            _Vals = newVals
            _Keys = newKeys
            Return _rVal
        End Function
        Public Function Remove(aKey As String) As TCHAR
            Dim idx As Integer
            If Not ContainsKey(aKey, idx) Then Return New TCHAR("")
            Return Remove(idx)
        End Function
        Public Function Add(aKey As String, aChar As TCHAR) As Boolean
            If Not _Init Then Clear()
            If String.IsNullOrEmpty(aKey) Then Return False
            If ContainsKey(aKey) Then Return False
            Dim newCnt As Integer = Count + 1
            Array.Resize(_Vals, newCnt)
            Array.Resize(_Keys, newCnt)
            _Vals(newCnt - 1) = aChar
            _Keys(newCnt - 1) = aKey
            Return True
        End Function
        Public Function UpdateMember(aIndex As Integer, aChar As TCHAR) As Boolean
            If aIndex < 1 Or aIndex > Count Then Return False
            aChar.Key = _Keys(aIndex - 1)
            aChar.Index = aIndex
            _Vals(aIndex - 1) = aChar
            Return True
        End Function
        Public Function TryGetValue(aKey As String) As Boolean
            Dim rChar As TCHAR = TCHAR.Null
            Return TryGetValue(aKey, rChar)
        End Function
        Public Function TryGetValue(aKey As String, ByRef rChar As TCHAR) As Boolean
            rChar = New TCHAR("")
            If Not _Init Or String.IsNullOrEmpty(aKey) Then Return False
            Dim idx As Integer = 0
            If Not ContainsKey(aKey, idx) Then Return False
            rChar = _Vals(idx - 1)
            rChar.Index = idx
            Return True
        End Function
        Public Function ContainsKey(aKey As String) As Boolean
            Dim rIndex As Integer = 0
            Return ContainsKey(aKey, rIndex)
        End Function
        Public Function ContainsKey(aKey As String, ByRef rIndex As Integer) As Boolean
            rIndex = 0
            If Not _Init Or String.IsNullOrEmpty(aKey) Then Return False
            Dim idx As Integer = Array.IndexOf(_Keys, aKey)
            If idx < 0 Then Return False
            rIndex = idx + 1
            Return True
        End Function
        Public Function Clone() As TDICTIONARY_CHAR
            Return New TDICTIONARY_CHAR(Me)

        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDICTIONARY_CHAR(Me)
        End Function


        Public Sub Clear()
            _Init = True
            ReDim _Keys(-1)
            ReDim _Vals(-1)

        End Sub
#End Region 'Methods
    End Structure 'TDICTIONARY_CHAR
    Friend Structure TDICTIONARY_TTABLEENTRY
        Implements ICloneable
        Public Name As String
        Private _Keys() As String
        Private _Vals() As TTABLEENTRY
        Private _Init As Boolean
#Region "Constructors"
        Public Sub New(aName As String)
            'init ------------------
            Name = aName
            _Init = True
            ReDim _Keys(-1)
            ReDim _Vals(-1)
            'init ------------------
        End Sub
        Public Sub New(aDictionary As TDICTIONARY_TTABLEENTRY)
            'init ------------------
            Name = aDictionary.Name
            _Init = True
            ReDim _Keys(-1)
            ReDim _Vals(-1)
            If aDictionary._Init Then
                _Keys = aDictionary._Keys.Clone()
                _Vals = aDictionary._Vals.Clone()
            End If

            'init ------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then Clear()
                Return _Vals.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As TTABLEENTRY
            '1 based
            If Not _Init Then Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED)
            If aIndex < 1 Or aIndex > Count Then Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED)
            _Vals(aIndex - 1).Index = aIndex
            Return _Vals(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TTABLEENTRY)
            If Not _Init Then Return
            If aIndex < 1 Or aIndex > Count Then Return
            _Vals(aIndex - 1) = value
            _Vals(aIndex - 1).Index = aIndex
        End Sub
        Public Function Item(aKey As String) As TTABLEENTRY
            '1 based
            If Not _Init Then Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED)
            Dim idx As Integer
            If Not ContainsKey(aKey, idx) Then Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED)
            _Vals(idx - 1).Index = idx
            Return _Vals(idx - 1)
        End Function
        Public Sub SetItem(aKey As String, value As TTABLEENTRY)
            If Not _Init Then Return
            Dim idx As Integer
            If Not ContainsKey(aKey, idx) Then Return
            _Vals(idx - 1) = value
            _Vals(idx - 1).Index = idx
        End Sub
        Public Overrides Function ToString() As String
            Return "TDICTIONARY_TTABLEENTRY [" & Count & "]"
        End Function
        Public Function Remove(aIndex As Integer) As TTABLEENTRY
            Dim cnt As Integer = Count
            If aIndex < 1 Or aIndex > cnt Then Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED)
            Dim newKeys(0 To cnt - 1) As String
            Dim newVals(0 To cnt - 1) As TTABLEENTRY
            Dim j As Integer = 0
            Dim aMem As TTABLEENTRY
            Dim _rVal As New TTABLEENTRY(dxxReferenceTypes.UNDEFINED)
            For i As Integer = 1 To cnt
                aMem = _Vals(i - 1)
                If i <> aIndex Then
                    j += 1
                    newVals(j - 1) = aMem
                    newKeys(j - 1) = aMem.Name.ToUpper
                Else
                    _rVal = aMem
                    _rVal.Index = i
                End If
            Next i
            _Vals = newVals
            _Keys = newKeys
            Return _rVal
        End Function
        Public Function Remove(aKey As String) As TTABLEENTRY
            Dim idx As Integer
            If Not ContainsKey(aKey, idx) Then Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED)
            Return Remove(idx)
        End Function
        Public Function Add(aKey As String, aEntry As TTABLEENTRY) As Boolean
            If Not _Init Then Clear()
            If String.IsNullOrEmpty(aKey) Then Return False
            If aEntry.Name = "" Then Return False
            If ContainsKey(aKey) Then Return False
            Dim newCnt As Integer = Count + 1
            Array.Resize(_Vals, newCnt)
            Array.Resize(_Keys, newCnt)
            _Vals(newCnt - 1) = aEntry
            _Keys(newCnt - 1) = aKey
            Return True
        End Function
        Public Function UpdateMember(aIndex As Integer, aEntry As TTABLEENTRY) As Boolean
            If aIndex < 1 Or aIndex > Count Then Return False
            aEntry.Index = aIndex
            _Vals(aIndex - 1) = aEntry
            Return True
        End Function
        Public Function TryGetValue(aKey As String) As Boolean
            Dim rEntry As TTABLEENTRY = TTABLEENTRY.Null
            Return TryGetValue(aKey, rEntry)
        End Function
        Public Function TryGetValue(aKey As String, ByRef rEntry As TTABLEENTRY) As Boolean
            If Not _Init Or String.IsNullOrEmpty(aKey) Then
                rEntry = Nothing
                Return False
            End If
            Dim idx As Integer = 0
            If Not ContainsKey(aKey, idx) Then
                rEntry = Nothing
                Return False
            End If
            rEntry = _Vals(idx - 1)
            rEntry.Index = idx
            Return True
        End Function
        Public Function ContainsKey(aKey As String) As Boolean
            Dim rIndex As Integer = 0
            Return ContainsKey(aKey, rIndex)
        End Function
        Public Function ContainsKey(aKey As String, ByRef rIndex As Integer) As Boolean
            rIndex = 0
            If Not _Init Or String.IsNullOrEmpty(aKey) Then Return False
            Dim idx As Integer = Array.IndexOf(_Keys, aKey.ToUpper())
            If idx < 0 Then Return False
            rIndex = idx + 1
            Return True
        End Function
        Public Function Clone() As TDICTIONARY_TTABLEENTRY
            Return New TDICTIONARY_TTABLEENTRY(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDICTIONARY_TTABLEENTRY(Me)
        End Function
        Public Sub Clear()

            _Init = True
            ReDim _Keys(-1)
            ReDim _Vals(-1)

        End Sub
#End Region 'Methods
    End Structure 'TDICTIONARY_TTABLEENTRY

    Friend Structure TDICTIONARY_HANDLES
        Implements ICloneable
        Public ImageGUID As String
        Private _IKeys() As Integer
        Private _IReleasedKeys() As Integer
        Private _HANDLES() As THANDLE
        Private _RHANDLES() As THANDLE
        Private _Init As Boolean
        Private _Count As Integer
        Private _RCount As Integer
        Private _Max As Integer
#Region "Constructors"
        Public Sub New(aImageGUID As String)
            'init --------------------------
            ImageGUID = aImageGUID
            _Init = True
            ReDim _IKeys(0)
            ReDim _HANDLES(0)
            ReDim _IReleasedKeys(0)
            ReDim _RHANDLES(0)

            _Count = 0
            _RCount = 0
            _Max = 0
            'init --------------------------

        End Sub

        Public Sub New(aDictionary As TDICTIONARY_HANDLES)
            'init --------------------------
            ImageGUID = aDictionary.ImageGUID
            _Init = True
            ReDim _IKeys(0)
            ReDim _HANDLES(0)
            ReDim _IReleasedKeys(0)
            ReDim _RHANDLES(0)

            _Count = 0
            _RCount = 0
            _Max = 0
            'init --------------------------

            If aDictionary.Count <= 0 Then Return
            _IKeys = aDictionary._IKeys.Clone()
            _HANDLES = aDictionary._HANDLES.Clone()
            _IReleasedKeys = aDictionary._IReleasedKeys.Clone()
            _RHANDLES = aDictionary._RHANDLES.Clone()
            _Count = aDictionary._Count
            _RCount = aDictionary._RCount
            _Max = aDictionary._Max

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property MaxHandle As Integer
            Get
                Return _Max
            End Get
            Set(value As Integer)
                If value >= 0 Then _Max = value Else _Max = 0
            End Set
        End Property
        Private _SuppressReuse As Boolean
        Public Property SuppressReuse As Boolean
            Get
                Return _SuppressReuse
            End Get
            Set(value As Boolean)
                _SuppressReuse = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function HandleIsUsed(aHandle As String, ByRef rOwnerGUID As String) As Boolean
            rOwnerGUID = ""
            Dim aMem As THANDLE = Nothing
            If TryGetHandle(TVALUES.HexToInteger(aHandle), aMem) Then
                rOwnerGUID = aMem.OwnerGUID
                Return True
            Else
                Return False
            End If
        End Function
        Public Function Count(Optional bReleased As Boolean = False) As Integer
            If Not _Init Then Return 0
            If Not bReleased Then Return _Count Else Return _RCount
        End Function
        Public Function Item(aIndex As Integer, Optional bReleased As Boolean = False) As THANDLE
            '1 based
            If Not _Init Then Return Nothing
            If aIndex < 1 Or aIndex > Count(bReleased) Then Return Nothing
            If bReleased Then
                Return _RHANDLES(aIndex - 1)
            Else
                Return _HANDLES(aIndex - 1)
            End If
        End Function
        Public Sub SetItem(aIndex As Integer, value As THANDLE, Optional bReleased As Boolean = False)
            If Not _Init Then Return
            If aIndex < 1 Or aIndex > Count(bReleased) Then Return
            If bReleased Then
                _RHANDLES(aIndex - 1) = value
            Else
                _HANDLES(aIndex - 1) = value
            End If
        End Sub
        Public Function Handle_Get(aHandle As Integer, Optional bReleased As Boolean = False) As THANDLE
            '1 based
            If Not _Init Then Return Nothing
            Dim rHandle As THANDLE = Nothing
            If Not TryGetHandle(aHandle, rHandle, bReleased:=bReleased) Then Return New THANDLE("")
            Return rHandle
        End Function
        Public Sub Handle_Set(aHandle As Integer, Optional bReleased As Boolean = False)
            '1 based
            If Not _Init Then Return
            Dim rHandle As THANDLE = Nothing
            Dim idx As Integer = 0
            If Not TryGetHandle(aHandle, rHandle, rIndex:=idx, bReleased:=bReleased) Then Return
            rHandle.Value = aHandle
            If Not bReleased Then
                _HANDLES(idx - 1) = rHandle
            Else
                _RHANDLES(idx - 1) = rHandle
            End If
        End Sub
        Public Overrides Function ToString() As String
            Return "TDICTIONARY_HANDLES [" & Count() & "]"
        End Function
        Public Sub ReduceTo(aCount As Integer)
            If Not _Init Then Return
            If aCount < 0 Then aCount = 0
            If aCount > Count() Or Count() = 0 Then Return
            _Count = aCount
            If aCount = 0 Then
                ReDim _HANDLES(0)
                ReDim _IKeys(0)
            Else
                Array.Resize(_HANDLES, _Count)
                Array.Resize(_IKeys, _Count)
            End If
        End Sub
        Public Function NextHandle(ByRef rUsed As Boolean) As Integer
            rUsed = False
            If Not _Init Then Clear()
            Dim _rVal As Integer
            Dim cnt As Integer = Count(True)
            Dim aMem As THANDLE
            If Count(True) > 0 And Not _SuppressReuse Then
                aMem = _RHANDLES(cnt - 1)
                rUsed = True
                _rVal = aMem.Value
            Else
                _rVal = _Max + 1
            End If
            Dim hndl As THANDLE = Nothing
            Dim idx As Integer
            Do Until Not ContainsHandle(_rVal, idx, False)  'just in case
                _Max = _rVal
                _rVal += 1
                rUsed = False
            Loop
            Return _rVal
        End Function
        Public Function TryGetHandle(aHandle As Integer, Optional bReleased As Boolean = False) As Boolean
            Dim rHandle As THANDLE = Nothing
            Dim rIndex As Integer = 0
            Return TryGetHandle(aHandle, rHandle, rIndex, bReleased)
        End Function
        Public Function TryGetHandle(aHandle As Integer, ByRef rHandle As THANDLE, Optional bReleased As Boolean = False) As Boolean
            Dim rIndex As Integer = 0
            Return TryGetHandle(aHandle, rHandle, rIndex, bReleased)
        End Function
        Public Function TryGetHandle(aHandle As Integer, ByRef rHandle As THANDLE, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            rHandle = New THANDLE
            If Not _Init Then Return False
            Dim idx As Integer = 0
            If Not ContainsHandle(aHandle, rIndex, bReleased) Then Return False
            If bReleased Then
                rHandle = _RHANDLES(rIndex - 1)
            Else
                rHandle = _HANDLES(rIndex - 1)
            End If
            Return True
        End Function
        Public Function TryGetHandleByGUID(aGUID As Integer, Optional bReleased As Boolean = False) As Boolean
            Dim rHandle As THANDLE = Nothing
            Dim rIndex As Integer = 0
            Return TryGetHandleByGUID(aGUID, rHandle, rIndex, bReleased)
        End Function
        Public Function TryGetHandleByGUID(aGUID As Integer, ByRef rHandle As THANDLE, Optional bReleased As Boolean = False) As Boolean
            Dim rIndex As Integer = 0
            Return TryGetHandleByGUID(aGUID, rHandle, rIndex, bReleased)
        End Function
        Public Function TryGetHandleByGUID(aGUID As String, ByRef rHandle As THANDLE, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            rHandle = New THANDLE("")
            If Not _Init Then Return False
            If String.IsNullOrEmpty(aGUID) Then Return False
            Dim idx As Integer = 0
            If Not ContainsGUID(aGUID, rIndex, bReleased) Then Return False
            If bReleased Then
                rHandle = _RHANDLES(rIndex - 1)
            Else
                rHandle = _HANDLES(rIndex - 1)
            End If
            Return True
        End Function
        Public Function SaveHandle(aHandle As Integer, aOwnerGUID As String, Optional aOwnerName As String = "") As Boolean
            Return Add(New THANDLE(aOwnerGUID) With {.OwnerName = aOwnerName, .Value = aHandle}, False)
        End Function
        Public Function Add(aHandle As THANDLE, Optional bReleased As Boolean = False) As Boolean
            If aHandle.Value <= 0 Then
                Return False
            End If
            If aHandle.OwnerGUID = "" And Not bReleased Then
                If dxfUtils.RunningInIDE Then
                    'Throw New Exception("TDICTIONARY_HANDLES.Add: Owner GUID Cannot Be Null")
                    Return False
                End If
            End If
            If Not _Init Then Clear()
            Dim cnt As Integer = Count(bReleased)
            Dim hndl As THANDLE = Nothing
            Dim idx As Integer = 0
            Dim aFlg As Boolean = TryGetHandle(aHandle.Value, hndl, idx, bReleased)
            Try
                If aFlg And Not bReleased Then
                    Throw New Exception("The Passed Handle Is already Used")
                End If
                If aHandle.Value <= 0 And Not bReleased Then
                    Throw New Exception("Handles must be positive and greater than zero")
                End If
                Dim newCnt As Integer = cnt + 1
                aHandle.Index = newCnt
                If Not bReleased Then
                    Array.Resize(_HANDLES, newCnt)
                    Array.Resize(_IKeys, newCnt)
                    _HANDLES(newCnt - 1) = aHandle
                    _IKeys(newCnt - 1) = aHandle.Value
                    _Count = newCnt
                Else
                    Array.Resize(_RHANDLES, newCnt)
                    Array.Resize(_IReleasedKeys, newCnt)
                    _RHANDLES(newCnt - 1) = aHandle
                    _IReleasedKeys(newCnt - 1) = aHandle.Value
                    _RCount = newCnt
                End If
                If aHandle.Value > _Max Then _Max = aHandle.Value
                Return True
            Catch ex As Exception
                Throw ex
                Return False
            End Try
        End Function
        Public Function Release(aHandle As Integer, Optional bReleased As Boolean = False) As Boolean
            Dim hndl As THANDLE = Nothing
            Dim idx As Integer = 0
            Dim aFlg As Boolean = TryGetHandle(aHandle, hndl, idx, bReleased:=bReleased)
            If Not aFlg Then Return False
            If Not bReleased Then Add(hndl, True)
            RemoveArrayItem(idx, bReleased)
            Return True
        End Function
        Public Function Release(aHandle As String) As Boolean
            If Not _Init Then Return False
            If String.IsNullOrEmpty(aHandle) Then Return False
            Dim iVal As Integer = TVALUES.HexToInteger(aHandle)
            If iVal = 0 Then Return False
            Dim hndl As THANDLE = Nothing
            Dim idx As Integer = 0
            If Not TryGetHandle(iVal, hndl, idx, bReleased:=False) Then Return False
            Add(hndl, True)
            RemoveArrayItem(idx, False)
            Return True
        End Function
        Public Function ContainsHandle(aHandle As Integer, Optional bReleased As Boolean = False) As Boolean
            Dim rIndex As Integer = 0
            Return ContainsHandle(aHandle, rIndex, bReleased)
        End Function
        Public Function ContainsHandle(aHandle As Integer, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            If Not _Init Then Return False
            If Count(bReleased) <= 0 Then Return False
            Dim idx As Integer
            If Not bReleased Then
                idx = Array.IndexOf(_IKeys, aHandle)
            Else
                idx = Array.IndexOf(_IReleasedKeys, aHandle)
            End If
            If idx < 0 Then Return False
            rIndex = idx + 1
            Return True
        End Function
        Public Function ContainsGUID(aGUID As String, Optional bReleased As Boolean = False) As Boolean
            Dim rIndex As Integer = 0
            Return ContainsGUID(aGUID, rIndex, bReleased)
        End Function
        Public Function ContainsGUID(aGUID As String, ByRef rIndex As Integer, Optional bReleased As Boolean = False) As Boolean
            rIndex = 0
            If Not _Init Then Return False
            If Count(bReleased) <= 0 Then Return False
            Dim aMem As THANDLE
            If Not bReleased Then
                For i As Integer = 1 To Count()
                    aMem = _HANDLES(i - 1)
                    If String.Compare(aMem.OwnerGUID, aGUID, ignoreCase:=True) = 0 Then
                        rIndex = i
                        Return True
                    End If
                Next
            Else
                For i As Integer = 1 To Count()
                    aMem = _RHANDLES(i - 1)
                    If String.Compare(aMem.OwnerGUID, aGUID, ignoreCase:=True) = 0 Then
                        rIndex = i
                        Return True
                    End If
                Next
            End If
            Return False
        End Function
        Public Function Clone() As TDICTIONARY_HANDLES
            Return New TDICTIONARY_HANDLES(Me)
        End Function

        Public Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDICTIONARY_HANDLES(Me)
        End Function


        Public Sub Clear()

            _Init = True
            ReDim _IKeys(0)
            ReDim _HANDLES(0)
            ReDim _IReleasedKeys(0)
            ReDim _RHANDLES(0)

            _Count = 0
            _RCount = 0
            _Max = 0
        End Sub
        Private Function RemoveArrayItem(aIndex As Integer, Optional bRealeased As Boolean = False) As Integer
            If Not _Init Then Return False
            Dim cnt As Integer = Count(bRealeased)
            If aIndex < 1 Or aIndex > cnt Then Return False
            Dim idx As Integer
            Dim newcnt As Integer = cnt - 1
            Dim newKeys(0 To newcnt - 1) As Integer
            Dim newHndls(0 To newcnt - 1) As THANDLE
            Dim srcKeys() As Integer
            Dim srcHndls() As THANDLE
            If Not bRealeased Then
                srcKeys = _IKeys
                srcHndls = _HANDLES
            Else
                srcKeys = _IReleasedKeys
                srcHndls = _RHANDLES
            End If
            For i As Integer = 1 To cnt
                If i <> aIndex Then
                    newKeys(idx) = srcKeys(i - 1)
                    newHndls(idx) = srcHndls(i - 1)
                    idx += 1
                End If
            Next
            If Not bRealeased Then
                _IKeys = srcKeys
                _HANDLES = srcHndls
                _Count = newcnt
            Else
                _IReleasedKeys = srcKeys
                _RHANDLES = srcHndls
                _RCount = newcnt
            End If
            Return True
        End Function
#End Region 'Methods
    End Structure 'TDICTIONARY_HANDLES

End Namespace
