Imports UOP.DXFGraphics.Structures
Namespace UOP.DXFGraphics
    Public Class colDXFRectangles
        Inherits List(Of dxfRectangle)
        Implements IEnumerable(Of dxfRectangle)
#Region "Members"
        Private bKeyed As Boolean
        Private bMaintainIndices As Boolean

        Public Sub New()
            MyBase.Clear()
        End Sub
#End Region 'Members
#Region "Constructors"
#End Region 'Constructors
#Region "Properties"

        Public Property MaintainIndices As Boolean
            Get
                MaintainIndices = bMaintainIndices
                Return MaintainIndices
            End Get
            Set(value As Boolean)
                If bMaintainIndices <> value Then
                    bMaintainIndices = value
                    If value Then ReIndex()
                End If
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overloads Function Add(aRect As dxfRectangle, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxfRectangle
            If aRect Is Nothing Then Return Nothing
            Return AddToCollection(aRect, aBeforeIndex, aAfterIndex, aTag:=aTag, aFlag:=aFlag, aAddClone:=bAddClone)
        End Function
        Public Overloads Function Add(aCenter As iVector, aWidth As Double, aHeight As Double, Optional aTag As String = Nothing, Optional aFlag As String = Nothing) As dxfRectangle
            '#1the center of the new rectangle
            '#2the width of the new rectangle
            '#3the height of the new rectangle
            '#4the tag to assign to the new rectangle
            '#5the flag to assign to the new rectangle
            '^short hand method used to add a rectangle to the collection
            Return AddToCollection(New dxfRectangle(aCenter, aWidth, aHeight, aTag:=aTag, aFlag:=aFlag))
        End Function
        Private Function AddToCollection(aRect As dxfRectangle, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bSuppressReindex As Boolean = False, Optional bSuppressEvnts As Boolean = False, Optional aAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            Try
                If aRect Is Nothing Then Return _rVal
                If MyBase.IndexOf(aRect) >= 0 Then aAddClone = True
                If aAddClone Then _rVal = aRect.Clone Else _rVal = aRect
                If aTag IsNot Nothing Then _rVal.Tag = aTag
                If aFlag IsNot Nothing Then _rVal.Flag = aFlag
                If aValue IsNot Nothing Then
                    If aValue.HasValue Then _rVal.Value = aValue.Value
                End If
                If Count = 0 Then
                    aBeforeIndex = 0
                    aAfterIndex = 0
                Else
                    If aBeforeIndex < 1 Then aBeforeIndex = 0
                    If aAfterIndex < 1 Then aAfterIndex = 0
                    If aBeforeIndex > Count Then
                        aBeforeIndex = 0
                        aAfterIndex = 0
                    End If
                    If aAfterIndex >= Count Then
                        aBeforeIndex = 0
                        aAfterIndex = 0
                    End If
                End If
                If aBeforeIndex = 0 And aAfterIndex = 0 Then
                    If bKeyed Then MyBase.Add(_rVal) Else MyBase.Add(_rVal)
                    If bMaintainIndices Then _rVal.Index = Count
                Else
                    If aBeforeIndex > 0 Then
                        MyBase.Insert(aBeforeIndex - 1, _rVal)
                    Else
                        MyBase.Insert(aAfterIndex, _rVal)
                    End If
                    If bMaintainIndices And Not bSuppressReindex Then ReIndex()
                End If
                'Application.DoEvents()
                '    If Not bSuppressEvents And Not bSuppressEvnts Then Call RaiseEntitiesChange(dxxCollectionEventTypes.Add, AddToCollection, False, False, True)
                Return _rVal
            Catch
                Return _rVal
            End Try
        End Function
        Public Function TagList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty
            Dim aMem As dxfRectangle
            For i As Integer = 1 To Count
                aMem = Item(i)
                TLISTS.Add(_rVal, aMem.Tag, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function FlagList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty
            Dim aMem As dxfRectangle
            For i As Integer = 1 To Count
                aMem = Item(i)
                TLISTS.Add(_rVal, aMem.Flag, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function Tags(Optional bUniqueOnly As Boolean = False, Optional bIncludeFlags As Boolean = False, Optional aFlagDelimitor As String = ":") As List(Of String)
            Dim _rVal As New List(Of String)
            '#1flag to return only the unique tags
            '^returns a collection of strings containing the tags of the members
            Dim aMem As dxfRectangle
            Dim bKeep As Boolean
            Dim aVal As String
            For i As Integer = 1 To Count
                aMem = Item(i)
                bKeep = True
                aVal = aMem.Tag
                If bIncludeFlags Then
                    If aMem.Flag <> "" Then
                        aVal += aFlagDelimitor & aMem.Flag
                    End If
                End If
                If bUniqueOnly Then
                    For j As Integer = 1 To _rVal.Count
                        If String.Compare(_rVal.Item(j), aVal, True) = 0 Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                End If
                If bKeep Then _rVal.Add(aVal)
            Next i
            Return _rVal
        End Function
        Public Function Flags(Optional bUniqueOnly As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            '#1flag to return only the unique flags
            '^returns a collection of strings containing the flags of the members
            Dim aMem As dxfRectangle
            Dim bKeep As Boolean
            For i As Integer = 1 To Count
                aMem = Item(i)
                bKeep = True
                If bUniqueOnly Then
                    For j As Integer = 1 To _rVal.Count
                        If String.Compare(_rVal.Item(j), aMem.Flag, True) = 0 Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                End If
                If bKeep Then _rVal.Add(aMem.Flag)
            Next i
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return $"colDXFRectangles [{ Count }]"
        End Function

        Public Function Clone() As colDXFRectangles
            Dim _rVal As colDXFRectangles
            _rVal = New colDXFRectangles
            For Each item As dxfRectangle In Me
                _rVal.Add(item, bAddClone:=True)
            Next
            Return _rVal
        End Function

        ''' <summary>
        ''' used to test if any of the member rectangles contain the passed point
        ''' </summary>
        ''' <remarks>the simple test assumes that the vector is and all the members lie on the same plane and that none of the members have any rotation</remarks>
        ''' <param name="aTestPoint">the point to test</param>
        ''' <param name="bSimpleTest">flag to test just the X and Y coordinates against the limits of each member</param>
        ''' <param name="rContainer">returns the first rectangle that is found to contain the passed point</param>
        ''' <param name="OnBoundIsIn">flag indicating that a point on a boundary is considered to be interior to a rectangle</param>
        ''' <param name="bSuppressPlaneTest">flag to assume that the vector is and all the members lie on the same plane</param>
        ''' <param name="bSuppressEdgeTest">flag to suppress the test to see if the vector lies on an edge</param>
        ''' <returns></returns>

        Public Function EnclosesPoint(aTestPoint As iVector, bSimpleTest As Boolean, ByRef rContainer As dxfRectangle, Optional OnBoundIsIn As Boolean = True, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False) As Boolean

            rContainer = Nothing
            Return colDXFRectangles.RectanglesEncloseVectorV(Me, New TVECTOR(aTestPoint), bSimpleTest, rContainer, OnBoundIsIn, bSuppressPlaneTest, bSuppressEdgeTest)

        End Function

        ''' <summary>
        ''' used to test if any of the member rectangles contain the passed point
        ''' </summary>
        ''' <remarks>the simple test assumes that the vector is and all the members lie on the same plane and that none of the members have any rotation</remarks>
        ''' <param name="aVector">the point to test</param>
        ''' <param name="bSimpleTest">flag to test just the X and Y coordinates against the limits of each member</param>
        ''' <param name="rContainer">returns the first rectangle that is found to contain the passed point</param>
        ''' <param name="bOnBoundIsIn">flag indicating that a point on a boundary is considered to be interior to a rectangle</param>
        ''' <param name="bSuppressPlaneTest">flag to assume that the vector is and all the members lie on the same plane</param>
        ''' <param name="bSuppressEdgeTest">flag to suppress the test to see if the vector lies on an edge</param>
        ''' <returns></returns>
        Friend Function EnclosesVectorV(aVector As TVECTOR, bSimpleTest As Boolean, ByRef rContainer As dxfRectangle, Optional bOnBoundIsIn As Boolean = True, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False) As Boolean
            rContainer = Nothing
            Return colDXFRectangles.RectanglesEncloseVectorV(Me, aVector, bSimpleTest, rContainer, bOnBoundIsIn, bSuppressPlaneTest, bSuppressEdgeTest)
        End Function

        Public Function GetBySuppressed(aSuppressedVal As Boolean) As List(Of dxfRectangle)
            Return FindAll(Function(mem) mem.Suppressed = aSuppressedVal)
            'Return _Members.Where(Function(mem) mem.Suppressed = aSuppressedVal)
        End Function
        Public Function GetByHandle(aHandle As String) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            '#1the handle to search for
            '#2returns the collection index of the member whose handle match the passed handle
            '^returns the first member whose handle matches the passed string
            If aHandle = "" Then Return _rVal
            If Count <= 0 Then Return _rVal
            _rVal = Find(Function(mem) String.Compare(mem.Handle, aHandle, ignoreCase:=True) = 0)
            Return _rVal
        End Function
        Public Overloads Sub RemoveRange(aRange As List(Of dxfRectangle))
            If aRange Is Nothing Or Count <= 0 Then Return
            For Each rect As dxfRectangle In aRange
                Dim idx As Integer = MyBase.IndexOf(rect)
                If idx >= 0 Then MyBase.Remove(rect)
            Next
        End Sub
        Public Function GetContainingMember(aVector As iVector, Optional bReturnTheNearest As Boolean = False, Optional bReturnClone As Boolean = False, Optional bSuppressProjection As Boolean = False) As dxfRectangle

            Dim aMem As dxfRectangle = GetByNearestCenter(aCenter:=aVector, bReturnClone:=False)
            If aMem Is Nothing Then Return Nothing
            If Not bReturnTheNearest Then
                If Not aMem.ContainsVector(aVector, bSuppressProjection:=bSuppressProjection) Then Return Nothing
            End If
            If bReturnClone Then Return aMem.Clone Else Return aMem
        End Function
        Public Function GetByNearestCenter(aCenter As iVector, Optional aMinDistance As Double = 0.0, Optional bReturnClone As Boolean = False) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            aMinDistance = 0

            Dim v1 As New TVECTOR(aCenter)
            Dim idx As Integer = 1
            If Count <= 0 Then
                Return _rVal
            ElseIf Count = 1 Then
                _rVal = Item(1)
            Else

                Dim minD As Double = 2.6E+26
                For i As Integer = 1 To Count
                    Dim aMem As dxfRectangle = Item(i)
                    Dim ctr As TVECTOR = aMem.CenterV
                    Dim D As Double = v1.DistanceTo(ctr)
                    If D < minD Then
                        minD = D
                        idx = i
                    End If
                Next i
                _rVal = Item(idx)
            End If
            aMinDistance = _rVal.CenterV.DistanceTo(v1)
            If Not bReturnClone Then Return _rVal Else Return New dxfRectangle(_rVal) With {.Index = idx}
            Return _rVal
        End Function
        Public Function GetByFlag(aFlag As String, Optional aTag As Object = Nothing, Optional bContainsString As Boolean = False, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aValue As Double? = Nothing, Optional aDelimitor As String = Nothing, Optional bIgnoreCase As Boolean = True) As List(Of dxfRectangle)
            '#1the flag to search for
            '#2an optional tag to include in the search criteria
            '#3flag to include any member whose flag string contains the passed search strings instead of a full string match
            '#4flag to stop searching when the first match is found
            '#5flag to return clones of the matches
            '#6flag to to remove the matches from the collection
            '^returns all the rectangles that match the search criteria
            Dim _rVal As New List(Of dxfRectangle)
            If aFlag Is Nothing Then Return _rVal
            Dim aStr As String = String.Empty
            Dim bTest As Boolean = aTag IsNot Nothing
            Dim bTestVal As Boolean = False
            If aValue.HasValue Then bTestVal = True
            Dim srch As List(Of dxfRectangle) = Me
            If bTest Then aStr = TVALUES.To_STR(aTag)
            Dim ret As New List(Of dxfRectangle)
            Dim srchvals() As String
            If String.IsNullOrWhiteSpace(aDelimitor) Then
                srchvals = New String() {aFlag}
            Else
                srchvals = aFlag.Split(aDelimitor)
            End If
            For Each flag As String In srchvals
                If Not bContainsString Then
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) = 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Tag, aStr, ignoreCase:=bIgnoreCase) = 0))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Flag, flag, ignoreCase:=bIgnoreCase) < 0 And String.Compare(mem.Tag, aStr, ignoreCase:=bIgnoreCase) < 0))
                        End If
                    End If
                Else
                    Dim comp As StringComparison = StringComparison.OrdinalIgnoreCase
                    If Not bIgnoreCase Then comp = StringComparison.Ordinal
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(flag, comparisonType:=comp) > -1))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(flag, comparisonType:=comp) > -1 And mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aStr, comparisonType:=comp) > -1))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(flag, comparisonType:=comp) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Tag.IndexOf(flag, comparisonType:=comp) < 0 And mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aStr, comparisonType:=comp) < 0))
                        End If
                    End If
                End If
            Next
            If bTestVal Then
                ret = ret.FindAll(Function(mem) mem.Value = aValue.Value)
            End If
            If ret.Count > 0 Then
                For Each mem As dxfRectangle In ret
                    If (bReturnClones) Then _rVal.Add(mem.Clone) Else _rVal.Add(mem)
                    If bReturnJustOne Then Exit For
                Next
            Else
                Return _rVal
            End If
            If bRemove Then
                RemoveRange(ret)
            End If
            Return _rVal
        End Function
        Public Function GetFlagged(aFlag As String, Optional aTag As String = Nothing, Optional bContainsString As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As dxfRectangle
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4flag to return a clone of the matches
            '#5flag to to remove the match from the collection
            '^returns the first entities that match the search criteria
            Dim all As List(Of dxfRectangle) = GetByFlag(aFlag, aTag, bContainsString, bReturnJustOne:=True)
            If all.Count <= 0 Then Return Nothing
            Dim _rVal As dxfRectangle = all.Item(0)
            If bRemove Then Remove(IndexOf(_rVal))
            If bReturnClones Then _rVal = _rVal.Clone
            Return _rVal
        End Function
        Public Function GetByTag(aTag As String, Optional aFlag As Object = Nothing, Optional bContainsString As Boolean = False, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aValue As Object = Nothing, Optional aDelimitor As String = Nothing, Optional bIgnoreCase As Boolean = True) As List(Of dxfRectangle)
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4flag to stop searching when the first match is found
            '#5flag to return clones of the matches
            '#6flag to to remove the matches from the collection
            '^returns all the rectangles that match the search criteria
            Dim _rVal As New List(Of dxfRectangle)
            If aTag Is Nothing Then Return _rVal
            Dim aStr As String = String.Empty
            Dim bTest As Boolean = aFlag IsNot Nothing
            Dim bTestVal As Boolean = aValue IsNot Nothing
            Dim srch As List(Of dxfRectangle) = Me
            If bTest Then aStr = TVALUES.To_STR(aFlag)
            Dim ret As New List(Of dxfRectangle)
            Dim srchvals() As String
            If String.IsNullOrWhiteSpace(aDelimitor) Then
                srchvals = New String() {aTag}
            Else
                srchvals = aTag.Split(aDelimitor)
            End If
            For Each tag As String In srchvals
                If Not bContainsString Then
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) = 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Flag, aStr, ignoreCase:=bIgnoreCase) = 0))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=bIgnoreCase) < 0 And String.Compare(mem.Flag, aStr, ignoreCase:=bIgnoreCase) < 0))
                        End If
                    End If
                Else
                    Dim comp As StringComparison = StringComparison.OrdinalIgnoreCase
                    If Not bIgnoreCase Then comp = StringComparison.Ordinal
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) > -1))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) > -1 And mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(aStr, comparisonType:=comp) > -1))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(tag, comparisonType:=comp) < 0 And mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(aStr, comparisonType:=comp) < 0))
                        End If
                    End If
                End If
            Next
            If bTestVal Then
                ret = ret.FindAll(Function(mem) mem.Value = aValue)
            End If
            If ret.Count > 0 Then
                For Each mem As dxfRectangle In ret
                    If (bReturnClones) Then _rVal.Add(mem.Clone) Else _rVal.Add(mem)
                    If bReturnJustOne Then Exit For
                Next
            Else
                Return _rVal
            End If
            If bRemove Then
                RemoveRange(ret)
            End If
            Return _rVal
        End Function
        Public Function GetTagged(aTag As String, Optional aFlag As String = Nothing, Optional bContainsString As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As dxfRectangle
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4flag to return a clone of the matches
            '#5flag to to remove the match from the collection
            '^returns the first entities that match the search criteria
            Dim all As List(Of dxfRectangle) = GetByTag(aTag, aFlag, bContainsString, bReturnJustOne:=True)
            If all.Count <= 0 Then Return Nothing
            Dim _rVal As dxfRectangle = all.Item(0)
            If bRemove Then Remove(IndexOf(_rVal))
            If bReturnClones Then _rVal = _rVal.Clone
            Return _rVal
        End Function
        Public Overloads Function IndexOf(aMember As dxfRectangle) As Integer
            If aMember Is Nothing Or Count <= 0 Then Return 0
            Return MyBase.IndexOf(aMember) + 1
        End Function
        Public Shadows Function Item(aIndex As Object) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            '#1 the index, handle or name of the desired entity
            '^returns the requested entity
            If Count <= 0 Or aIndex Is Nothing Then Return _rVal
            Dim idx As Integer = 0
            If aIndex.GetType() Is GetType(String) Then
                _rVal = GetByHandle(aIndex.ToString())
                If _rVal IsNot Nothing Then idx = MyBase.IndexOf(_rVal) + 1
                If _rVal Is Nothing Then If TVALUES.IsNumber(aIndex) Then idx = TVALUES.To_INT(aIndex)
            Else
                If TVALUES.IsNumber(aIndex) Then idx = TVALUES.To_INT(aIndex)
            End If
            If (idx > 0 And idx <= Count) And _rVal Is Nothing Then
                _rVal = MyBase.Item(idx - 1)
            End If
            If _rVal IsNot Nothing Then
                If bMaintainIndices Then _rVal.Index = idx
            End If
            Return _rVal
        End Function
        Friend Sub ReIndex()
            '^updates collection indices of the current members
            For i As Integer = 1 To Count
                MyBase.Item(i - 1).Index = i
            Next i
        End Sub
        Public Overloads Function Remove(aIndex As Integer) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            If aIndex > 0 And aIndex <= Count Then
                _rVal = Item(aIndex)
                MyBase.Remove(_rVal)
                If bMaintainIndices And aIndex <> Count + 1 Then ReIndex()
                '        If Not bSuppressEvents Then Call RaiseEntitiesChange(dxxCollectionEventTypes.Remove, Remove, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function RemoveMember(aRectangle As dxfRectangle) As Boolean
            Dim _rVal As Boolean = False
            If aRectangle Is Nothing Then
                Return False
            Else
                _rVal = MyBase.Contains(aRectangle)
                If _rVal Then MyBase.Remove(aRectangle)
            End If
            If bMaintainIndices And _rVal Then ReIndex()
            Return _rVal
        End Function
        Public Function RemoveMembers(aRectangles As colDXFRectangles) As Boolean
            Dim rRemoved As List(Of dxfRectangle) = Nothing
            Return RemoveMembers(aRectangles, rRemoved)
        End Function
        Public Function RemoveMembers(aRectangles As colDXFRectangles, ByRef rRemoved As List(Of dxfRectangle)) As Boolean
            rRemoved = New List(Of dxfRectangle)
            If aRectangles Is Nothing Then
                Return False
            Else
                If aRectangles Is Me Then Return False
                Dim aMem As dxfRectangle
                Dim idx As Integer = -1
                For i As Integer = 1 To aRectangles.Count
                    aMem = aRectangles.Item(i)
                    idx = MyBase.IndexOf(aMem)
                    If idx >= 0 Then
                        MyBase.Remove(aMem)
                        rRemoved.Add(aMem)
                    End If
                Next i
            End If
            If rRemoved.Count > 0 And MaintainIndices Then ReIndex()
            Return rRemoved.Count > 0
        End Function
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub


#End Region 'Methods


        ''' <summary>
        ''' used to test if any of the member rectangles contain the passed point
        ''' </summary>
        ''' <remarks>the simple test assumes that the vector is and all the members lie on the same plane and that none of the members have any rotation</remarks>
        ''' <param name="aRectangles">a list of rectangles</param>
        ''' <param name="aVector">the point to test</param>
        ''' <param name="bSimpleTest">flag to test just the X and Y coordinates against the limits of each member</param>
        ''' <param name="rContainer">returns the first rectangle that is found to contain the passed point</param>
        ''' <param name="bOnBoundIsIn">flag indicating that a point on a boundary is considered to be interior to a rectangle</param>
        ''' <param name="bSuppressPlaneTest">flag to assume that the vector is and all the members lie on the same plane</param>
        ''' <param name="bSuppressEdgeTest">flag to suppress the test to see if the vector lies on an edge</param>
        ''' <returns></returns>
#Region "Shared Methods"
        Public Shared Function RectanglesEncloseVector(aRectangles As IEnumerable(Of dxfRectangle), aVector As iVector, bSimpleTest As Boolean, ByRef rContainer As dxfRectangle, Optional bOnBoundIsIn As Boolean = True, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False) As Boolean
            rContainer = Nothing
            If aRectangles Is Nothing Then Return False

            Return RectanglesEncloseVectorV(aRectangles, New TVECTOR(aVector), bSimpleTest, rContainer, bOnBoundIsIn, bSuppressPlaneTest, bSuppressEdgeTest)
        End Function

        ''' <summary>
        ''' used to test if any of the member rectangles contain the passed point
        ''' </summary>
        ''' <remarks>the simple test assumes that the vector is and all the members lie on the same plane and that none of the members have any rotation</remarks>
        ''' <param name="aRectangles">a list of rectangles</param>
        ''' <param name="aVector">the point to test</param>
        ''' <param name="bSimpleTest">flag to test just the X and Y coordinates against the limits of each member</param>
        ''' <param name="rContainer">returns the first rectangle that is found to contain the passed point</param>
        ''' <param name="bOnBoundIsIn">flag indicating that a point on a boundary is considered to be interior to a rectangle</param>
        ''' <param name="bSuppressPlaneTest">flag to assume that the vector is and all the members lie on the same plane</param>
        ''' <param name="bSuppressEdgeTest">flag to suppress the test to see if the vector lies on an edge</param>
        ''' <returns></returns>
        Friend Shared Function RectanglesEncloseVectorV(aRectangles As IEnumerable(Of dxfRectangle), aVector As TVECTOR, bSimpleTest As Boolean, ByRef rContainer As dxfRectangle, Optional bOnBoundIsIn As Boolean = True, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False) As Boolean
            rContainer = Nothing
            If aRectangles Is Nothing Then Return False
            Dim onbound As Boolean = False
            Dim iscorner As Boolean = False
            For Each aMem As dxfRectangle In aRectangles
                Dim testresult As Boolean = aMem.ContainsVector(aVector, 0.001, onbound, iscorner, bSuppressPlaneTest, bSuppressEdgeTest, bSimpleTest)
                If onbound Or iscorner Then
                    If Not bOnBoundIsIn Then testresult = False
                End If
                If testresult Then
                    rContainer = aMem
                    Return True
                    Exit For
                End If
            Next
            Return False
        End Function
#End Region 'Shared Methods

    End Class 'colDXFRectangles
End Namespace
