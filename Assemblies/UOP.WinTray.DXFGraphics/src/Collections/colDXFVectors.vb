Imports System.Xml
Imports SharpDX.Direct2D1
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke.User32

Namespace UOP.DXFGraphics
    Public Class colDXFVectors
        Inherits List(Of dxfVector)
        Implements IEnumerable(Of dxfVector)
        Implements IDisposable
#Region "IEnumerable Implementation"
        'Public Function GetEnumerator() As IEnumerator(Of dxfVector) Implements IEnumerable(Of dxfVector).GetEnumerator
        '    Return DirectCast(_Members, IEnumerable(Of dxfVector)).GetEnumerator()
        'End Function
        'Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        '    Return DirectCast(_Members, IEnumerable).GetEnumerator()
        'End Function
#End Region 'IEnumerable Implementation
#Region "Members"
        ' Private _Events As dxpEventHandler
        'Private _Members As List(Of dxfVector)
        Private _SuppressEvents As Boolean
        Private _CollectionGUID As String
        Private _ImageGUID As String
        Private _BlockGUID As String
        Private _OwnerGUID As String
        Private _MaintainIndices As Boolean
        Private _Tag As String
        Private _LastVector As dxfVector
        Private _LastVectorIndex As Integer
        Private disposedValue As Boolean
        Friend OwnerPtr As WeakReference
#End Region 'Members
#Region "Constructors"
        Public Sub New()

            '_Events = goEvents()
            'AddHandler _Events.VectorsRequest, AddressOf _Events_VectorsRequest
            OwnerPtr = Nothing
            'iIndex = -1
        End Sub
        Public Sub New(aVector As iVector, Optional bVector As iVector = Nothing, Optional cVector As iVector = Nothing, Optional dVector As iVector = Nothing, Optional eVector As iVector = Nothing, Optional fVector As iVector = Nothing, Optional gVector As iVector = Nothing, Optional hVector As iVector = Nothing)
            Init()
            If aVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(aVector))
            If bVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(bVector))
            If cVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(cVector))
            If dVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(dVector))
            If eVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(eVector))
            If fVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(fVector))
            If gVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(gVector))
            If hVector IsNot Nothing Then MyBase.Add(dxfVector.FromIVector(hVector))

        End Sub
        Friend Sub New(aVectors As TVECTORS)
            Init()
            For i As Integer = 1 To aVectors.Count
                MyBase.Add(New dxfVector(aVectors.Item(i)))
            Next

        End Sub
        Friend Sub New(aVertices As TVERTICES)
            Init()
            For i As Integer = 1 To aVertices.Count
                MyBase.Add(New dxfVector(aVertices.Item(i)))
            Next


        End Sub
        Public Sub New(aVectors As IEnumerable(Of iVector), Optional bAddClones As Boolean = True)
            Init()

            If aVectors Is Nothing Then Return

            For Each item As iVector In aVectors
                If item Is Nothing Then Continue For
                If TypeOf item Is dxfVector Then
                    Dim v1 As dxfVector = DirectCast(item, dxfVector)
                    If bAddClones Then MyBase.Add(New dxfVector(v1)) Else MyBase.Add(v1)
                Else
                    MyBase.Add(New dxfVector(item))
                End If

            Next

        End Sub
        Public Sub New(aCoordinatesString As String, Optional aDelimitor As Char = "¸")
            Init()
            If String.IsNullOrWhiteSpace(aCoordinatesString) Then Return
            If Char.IsWhiteSpace(aDelimitor) Then Return
            Dim sVals() As String = aCoordinatesString.Split(aDelimitor)

            For i As Integer = 0 To sVals.Length - 1
                MyBase.Add(New dxfVector(sVals(i)))
            Next i
        End Sub

        Private Sub Init()
            MyBase.Clear()
            _MaintainIndices = True
            _CollectionGUID = ""
            _ImageGUID = ""
            _BlockGUID = ""
            _OwnerGUID = ""
            _MaintainIndices = False
            _Tag = ""
            _LastVector = Nothing
            disposedValue = False
            OwnerPtr = Nothing
        End Sub
#End Region 'Constructors
#Region "Events"
        Public Event VectorsMemberChange(aEvent As dxfVertexEvent)
        Public Event VectorsChange(aEvent As dxfVectorsEvent)
        Public Event OCSRequest(aOCS As dxfPlane)
#End Region 'Events
#Region "Constructors"
#End Region 'Constructors
#Region "Properties"




        Friend ReadOnly Property GUID As String
            Get
                Return _CollectionGUID
            End Get
        End Property
        Friend Property ImageGUID As String
            '^the guid of the image that this vector is associated to
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                If String.Compare(value, _ImageGUID, ignoreCase:=True) <> 0 Then
                    _ImageGUID = value
                    'If Not String.IsNullOrWhiteSpace(_ImageGUID) Then
                    '    If _Events  IsNot Nothing Then RemoveHandler _Events.VectorsRequest, AddressOf _Events_VectorsRequest
                    '    _Events = Nothing
                    'Else
                    '    If _Events IS Nothing Then _Events = goEvents()
                    '    AddHandler _Events.VectorsRequest, AddressOf _Events_VectorsRequest
                    'End If
                End If
                _ImageGUID = value
            End Set
        End Property
        Friend Property BlockGUID As String
            '^the guid of the image.block that this vector is associated to
            Get
                Return _BlockGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _BlockGUID = value
            End Set
        End Property
        Friend Property EntityGUID As String
            '^the guid of the image.Entity that this vector is associated to
            Get
                Return _OwnerGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _OwnerGUID = value
            End Set
        End Property



        Public Property MaintainIndices As Boolean
            Get
                Return _MaintainIndices Or MonitorMembers
            End Get
            Set(value As Boolean)
                If Not _MaintainIndices = value Then
                    _MaintainIndices = value
                    If MaintainIndices Then ReIndex()
                End If
            End Set
        End Property
        Friend Property MonitorMembers As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_CollectionGUID)
            End Get
            Set(value As Boolean)
                If value Then
                    MaintainIndices = True
                    If String.IsNullOrWhiteSpace(_CollectionGUID) Then
                        _CollectionGUID = dxfEvents.NextVectorsGUID
                        For Each v1 As dxfVector In Me
                            v1.CollectionGUID = _CollectionGUID
                            v1.CollectionPtr = New WeakReference(Me)
                        Next
                    End If
                Else
                    If Not String.IsNullOrWhiteSpace(_CollectionGUID) Then
                        For Each v1 As dxfVector In Me
                            v1.ReleaseCollectionReference()
                        Next
                    End If
                    _CollectionGUID = ""
                End If
                'If Not String.IsNullOrWhiteSpace(_ImageGUID) Then
                '    If _Events  IsNot Nothing Then RemoveHandler _Events.VectorsRequest, AddressOf _Events_VectorsRequest
                '    _Events = Nothing
                'Else
                '    If _Events IS Nothing Then _Events = goEvents()
                '    AddHandler _Events.VectorsRequest, AddressOf _Events_VectorsRequest
                'End If
            End Set
        End Property

        Public Property SuppressEvents As Boolean
            Get
                Return _SuppressEvents
            End Get
            Set(value As Boolean)
                _SuppressEvents = value
            End Set
        End Property
        Public Property Tag As String
            Get
                '^a tag to assign to new members
                Return _Tag
            End Get
            Set(value As String)
                '^a tag to assign to new members
                _Tag = value
            End Set
        End Property


        Private ReadOnly Property MyOwner As dxfHandleOwner
            Get
                If String.IsNullOrWhiteSpace(_OwnerGUID) Or OwnerPtr Is Nothing Then Return Nothing
                Dim _rVal As dxfEntity = TryCast(OwnerPtr.Target, dxfHandleOwner)
                If _rVal IsNot Nothing Then
                    If String.Compare(_OwnerGUID, _rVal.GUID, ignoreCase:=True) Then
                        ReleaseOwnerReference()
                        _rVal = Nothing
                    End If
                End If
                Return _rVal
            End Get
        End Property
#End Region 'Properties
#Region "Methods"

        Public Sub Circularize(aRadius As Double, Optional aPrecis As Integer = 4, Optional aCircleCenter As iVector = Nothing, Optional bDontSort As Boolean = False, Optional aPlane As dxfPlane = Nothing)

            If Count <= 1 Or aRadius <= 0 Then Return


            Dim plane As New dxfPlane(aPlane, aCircleCenter)
            Dim v0 As New dxfVector(plane.Origin)

            Dim precis As Integer = TVALUES.LimitedValue(aPrecis, 1, 15, 4)
            If Not bDontSort Then Sort(dxxSortOrders.CounterClockwise, v0, aPlane:=plane, aPrecis:=precis)

            Dim rad As Double = Math.Round(aRadius, precis)

            For i As Integer = 1 To Count

                Dim v1 As dxfVector = Item(i)
                Dim vnext As dxfVector = Nothing
                If i < Count Then vnext = Item(i + 1) Else vnext = Item(1)

                Dim d1 As Double = Math.Round(v0.DistanceTo(v1), precis)
                Dim d2 As Double = Math.Round(v0.DistanceTo(vnext), precis)
                If d1 = rad Then
                    If d2 = rad Then
                        v1.VertexRadius = aRadius
                    Else
                        v1.VertexRadius = 0
                    End If

                Else
                    If Math.Round(v1.VertexRadius, precis) = rad Then v1.VertexRadius = 0
                End If


            Next


        End Sub

        Public Function LengthSummation(Optional bClosed As Boolean = False) As Double
            '#1flag to include the distance from the last to the first
            '^returns the total of the distance between each member
            '~ (1 to 2) + (2 to 3) + (3 to 4) etc.
            If Count <= 1 Then Return 0
            Dim _rVal As Double
            For i As Integer = 1 To Count - 1
                _rVal += Item(i).DistanceTo(Item(i + 1)) ' .Strukture.DistanceTo(v2.Strukture)
            Next i
            If bClosed And Count > 2 Then
                _rVal += Item(Count).DistanceTo(Item(1))
            End If
            Return _rVal
        End Function

        Public Function ToList(Optional bGetClones As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            If Count <= 0 Then Return _rVal
            For Each v1 As dxfVector In Me
                If Not bGetClones Then
                    _rVal.Add(v1)
                Else
                    _rVal.Add(v1.Clone())
                End If

            Next
            Return _rVal
        End Function

        Friend Function ItemVector(aIndex As Integer, Optional bSuppressIndexErr As Boolean = False) As TVECTOR
            '#1the requested item number
            '^returns the vector  from the collection at the requested index in the collection.
            If aIndex < 1 Or aIndex > Count Then
                If bSuppressIndexErr Then Return TVECTOR.Zero
                If aIndex < 1 Or aIndex > Count Then
                    If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                    Return TVECTOR.Zero
                End If
            End If
            Return Item(aIndex).Strukture
        End Function
        Friend Sub SetItemVector(aIndex As Integer, value As TVECTOR)
            If aIndex > 0 And aIndex <= Count Then Return
            Item(aIndex).Strukture = value
        End Sub
        Friend Sub ReleaseOwnerReference()
            _OwnerGUID = ""
            OwnerPtr = Nothing
        End Sub
        Public Overloads Function IndexOf(aMember As dxfVector, Optional bReturnNearestVector As Boolean = False) As Integer
            If aMember Is Nothing Or Count <= 0 Then Return 0
            Dim _rVal As Integer = MyBase.IndexOf(aMember) + 1
            If _rVal = 0 And bReturnNearestVector Then
                Dim v1 As dxfVector = NearestVector(aMember)
                If v1 IsNot Nothing Then
                    _rVal = MyBase.IndexOf(v1) + 1
                End If
            End If
            Return _rVal
        End Function
        Friend Function Vertices(Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 8) As TVERTICES
            Dim i As Integer
            Dim v1 As dxfVector
            Dim doplane As Boolean = Not dxfPlane.IsNull(aPlane)
            Dim _rVal As New TVERTICES(0)
            Dim plane As New TPLANE(aPlane)

            If Count <= 0 Then Return _rVal
            Dim mem As TVERTEX
            For i = 1 To Count
                v1 = Item(i)
                mem = v1.VertexV
                If doplane Then mem = mem.WithRespectTo(plane, aPrecis)
                _rVal.Add(mem)
            Next i
            Return _rVal
        End Function
        Public Function Coordinates_Get()
            '^a concantonated string of all the vector coordinates in the collection
            '~the delimitor is "¸" (char 184)

            Dim _rVal As String = String.Empty
            For i As Integer = 1 To Count
                Dim pt As dxfVector = Item(i)
                If _rVal <> "" Then _rVal += dxfGlobals.Delim
                _rVal += pt.Coordinates
            Next i
            Return _rVal

        End Function
        Public Function CoordinatesP(Optional aPrecis As Integer = 3, Optional bIndexed As Boolean = False) As String
            '^a concantonated string of all the vector coordinates in the collection
            '~the delimitor is "¸" (char 184) and the vector ordinates are round to the passed precision
            '~the coordinates are augmented with the name of the vectors vertex style
            Dim v1 As dxfVector
            Dim _rVal As String = String.Empty
            For i As Integer = 1 To Count
                v1 = Item(i)
                If Not bIndexed Then
                    If _rVal <> "" Then _rVal += dxfGlobals.Delim
                    _rVal += v1.CoordinatesP(aPrecis)
                Else
                    If _rVal <> "" Then _rVal += vbLf
                    _rVal += $"{i} - { v1.CoordinatesP(aPrecis)}"
                End If
            Next i
            Return _rVal
        End Function
        Public Function CoordinatesV(Optional aPrecis As Integer = 3, Optional bIndexed As Boolean = False) As String
            '^a concantonated string of all the vector coordinates in the collection
            '~the delimitor is "¸" (char 184) and the vector ordinates are round to the passed precision
            '~the coordinates are augmented with the name of the vectors vertex TYPE (begin line or begin arc w/radius&clockwise)
            Dim _rVal As String = String.Empty
            Dim v1 As dxfVector
            Dim i As Integer
            For i = 1 To Count
                v1 = Item(i)
                If Not bIndexed Then
                    If _rVal <> "" Then _rVal += dxfGlobals.Delim
                    _rVal += v1.CoordinatesV(aPrecis)
                Else
                    If _rVal <> "" Then _rVal += vbLf
                    _rVal += $"{i} - { v1.CoordinatesV(aPrecis)}"
                End If
            Next i
            Return _rVal
        End Function
        Private Sub RaiseChangeEvent(aType As dxxCollectionEventTypes, aVector As dxfVector, aPropertyName As String, aOldValue As Object, aNewValue As Object, ByRef rCancel As Boolean, bOptionFlag As Boolean, bCountChange As Boolean)
            rCancel = False
            Dim vecsevent As New dxfVectorsEvent(aType, GUID, ImageGUID, BlockGUID, EntityGUID) With {
            .OldValue = aOldValue,
            .NewValue = aNewValue,
            .PropertyName = aPropertyName,
            .OptionFlag = bOptionFlag,
            .CountChange = bCountChange
            }
            If aVector IsNot Nothing Then vecsevent.Member = aVector

            RaiseEvent VectorsChange(vecsevent)
            rCancel = vecsevent.Undo
            If Not String.IsNullOrWhiteSpace(_OwnerGUID) Then
                Dim owner As dxfHandleOwner = MyOwner
                If owner IsNot Nothing Then
                    owner.RespondToVectorsChange(vecsevent)
                    vecsevent.ImageNotified = True
                End If
            End If
            'If MonitorMembers Then
            '    If _ImageGUID <> "" And _OwnerGUID <> "" Then
            '        Dim entity As dxfEntity = dxfEvents.GetImageEntity(_ImageGUID, _OwnerGUID, _BlockGUID)
            '        If entity IsNot Nothing Then
            '            entity.RespondToVectorsChange(vecsevent)
            '            rCancel = vecsevent.Undo
            '        End If
            '    End If
            'End If
        End Sub

        Private Sub RaiseChangeEvent(aType As dxxCollectionEventTypes, aVectors As IEnumerable(Of iVector), aPropertyName As String, aOldValue As Object, aNewValue As Object, ByRef rCancel As Boolean, bOptionFlag As Boolean, bCountChange As Boolean)
            rCancel = False
            Dim vecsevent As New dxfVectorsEvent(aType, GUID, ImageGUID, BlockGUID, EntityGUID) With {
            .OldValue = aOldValue,
            .NewValue = aNewValue,
            .PropertyName = aPropertyName,
            .OptionFlag = bOptionFlag,
            .CountChange = bCountChange
            }
            If aVectors IsNot Nothing Then
                If TypeOf aVectors Is colDXFVectors Then
                    Dim vecs As colDXFVectors = aVectors
                    vecsevent.Members = vecs.ToList()

                ElseIf TypeOf aVectors Is List(Of dxfVector) Then
                    Dim vecs As List(Of dxfVector) = aVectors
                    vecsevent.Members = vecs
                Else
                    Dim vecs As New List(Of dxfVector)
                    For Each vec As iVector In aVectors

                        If vec Is Nothing Then Continue For
                        If TypeOf vec Is dxfVector Then vecs.Add(DirectCast(vec, dxfVector)) Else vecs.Add(New dxfVector(vec))
                    Next
                    vecsevent.Members = vecs
                End If
            End If
            RaiseEvent VectorsChange(vecsevent)
            rCancel = vecsevent.Undo
            If Not String.IsNullOrWhiteSpace(_OwnerGUID) Then
                Dim owner As dxfHandleOwner = MyOwner
                If owner IsNot Nothing Then
                    owner.RespondToVectorsChange(vecsevent)
                    vecsevent.ImageNotified = True
                End If
            End If
            'If MonitorMembers Then
            '    If _ImageGUID <> "" And _OwnerGUID <> "" Then
            '        Dim entity As dxfEntity = dxfEvents.GetImageEntity(_ImageGUID, _OwnerGUID, _BlockGUID)
            '        If entity IsNot Nothing Then
            '            entity.RespondToVectorsChange(vecsevent)
            '            rCancel = vecsevent.Undo
            '        End If
            '    End If
            'End If
        End Sub

        Public Function Above(aYValue As Double, Optional bOnisIn As Boolean = True, Optional aPrecis As Integer = 3, Optional aPlane As dxfPlane = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxfVector)
            Return GetVectors(aFilter:=dxxPointFilters.GreaterThanY, aOrdinate:=aYValue, bOnIsIn:=bOnisIn, aPlane:=aPlane, aPrecis:=aPrecis, bReturnClones:=bReturnClones, bRemove:=bRemove)
        End Function
        Public Function AddByString(aPointString As String, Optional aClearExisting As Boolean = False, Optional aDelimiter As Char = "¸") As List(Of dxfVector)
            '#1the descriptor string to add points to the collection with
            '#2flag to keep or discard points already in the collection
            '^adds new points to the collection based on the passed point coordinate string(s).
            '~strings like (1,2,3)¸(5,6,7).  Delimator is chr(184).
            Dim _rVal As New List(Of dxfVector)
            If String.IsNullOrWhiteSpace(aPointString) Then Return _rVal
            If String.IsNullOrWhiteSpace(aDelimiter) Then aDelimiter = dxfGlobals.Delim
            Dim dstr As String = aPointString.Trim()
            Dim pstrg As String
            If Not dstr.Contains(aDelimiter) Then
                aDelimiter = dxfGlobals.MultiDelim
            End If

            Dim vals() As String = dstr.Split(aDelimiter)
            If aClearExisting Then MyBase.Clear()

            For i As Integer = 0 To vals.Length - 1
                pstrg = vals(i)
                _rVal.Add(AddToCollection(New dxfVector(pstrg)))
            Next i
            Return _rVal
        End Function
        Public Function AddInsertionPt(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aRotation As Double = 0.0, Optional aCS As dxfPlane = Nothing, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional aTag As String = "", Optional aFlag As String = "") As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the X coordinate of the new vector
            '#2the Y coordinate of the new vector
            '#3the Z coordinate of the new vector
            '#4a rotation angle to assign to the new vector
            '#5a coordinate system to define the the new vector by
            '#6the index to insert the vector into the collection before
            '#7the index to insert the vector into the collection after
            '^shorthand way to add a vector without all the code to create and add one conventionally
            '~returns the new vector that was created.
            If aCS Is Nothing Then
                _rVal = New dxfVector(TVALUES.To_DBL(aX), TVALUES.To_DBL(aY), TVALUES.To_DBL(aZ))
            Else
                _rVal = aCS.Vector(aX, aY, aZ)
            End If
            _rVal.Rotation = aRotation
            _rVal = AddToCollection(_rVal, aBeforeIndex, aAfterIndex, aTag:=aTag, aFlag:=aFlag)
            If _LastVectorIndex > 0 Then Return Item(_LastVectorIndex) Else Return Nothing
        End Function
        Public Sub AddMirrors(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aReverseOrder As Boolean = False, Optional aPlane As dxfPlane = Nothing)
            '#1the x coordinate to mirror across
            '#2the y coordinate to mirror across
            '#3flag to add the mirrors in reverse order
            '#4acollection of points to get the mirrors from other than this one
            '^adds the mirrors of the points to the end of this collection
            If aMirrorX Is Nothing And aMirrorY Is Nothing Then Return
            Dim aPt As dxfVector
            Dim bpt As dxfVector
            Dim i As Integer
            Dim si As Integer
            Dim ei As Integer
            Dim stp As Integer
            Dim bPlane As dxfPlane
            If TPLANE.IsNull(aPlane) Then bPlane = New dxfPlane Else bPlane = New dxfPlane
            If aReverseOrder Then
                si = Count
                ei = 1
                stp = -1
            Else
                ei = Count
                si = 1
                stp = 1
            End If
            For i = si To ei Step stp
                aPt = Item(i)
                bpt = aPt.Clone
                bpt.MirrorPlanar(aMirrorX, aMirrorY, bPlane)
                Add(bpt)
            Next i
        End Sub
        Public Function AddPair(aX1 As Double, aY1 As Double, aZ1 As Double, aX2 As Double, aY2 As Double, aZ2 As Double, Optional aTag As String = "", Optional aFlag As String = "", Optional aValue As Double? = Nothing) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '^a shorthand way to add a vector pair without all the code to create and add one conventionally
            '~returns the created pair
            '~the first vector is added to the start of the collection and the second to the end
            Dim v1 As dxfVector = Add(aX1, aY1, aZ1, aTag:=aTag, aFlag:=aFlag, aBeforeIndex:=1)
            v1.Value = aValue
            _rVal.Add(v1)
            v1 = Add(aX2, aY2, aZ2, aTag:=aTag, aFlag:=aFlag)
            v1.Value = aValue
            _rVal.Add(v1)
            Return _rVal
        End Function
        Public Function AddPlaneVector(aPlane As dxfPlane, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aVertexRadius As Double = 0.0, Optional aTag As String = "", Optional aFlag As String = "", Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aValue As Double? = Nothing) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the subject plane
            '#2the X coordinate of the new vector
            '#3the Y coordinate of the new vector
            '#4the Z coordinate of the new vector
            '#5a vertex radius to assign to the new vector
            '#6a tag to assign to the new vector
            '#7a flag to assign to the new vector
            '#8the index to insert the vector into the collection before
            '#9the index to insert the vector into the collection after
            '#10a coordinate system to define the the new vector by
            '#11a layer name to assign to the new vector
            '#12a color to assign to the new vector
            '#13a linetype to assign to the new vector
            '^shorthand way to add a vector without all the code to create and add one conventionally
            '~returns the new vector that was created.
            _rVal = New dxfVector
            If TPLANE.IsNull(aPlane) Then _rVal.SetComponentsV(aX, aY, aZ) Else _rVal.Strukture = New TPLANE(aPlane).Vector(aX, aY, aZ)
            _rVal.LCLSet(aLayer, aColor, aLineType)
            _rVal.VertexRadius = aVertexRadius
            _rVal = AddToCollection(_rVal, aBeforeIndex, aAfterIndex, aTag:=aTag, aFlag:=aFlag, aValue:=aValue)
            If _LastVectorIndex > 0 Then Return _rVal Else Return Nothing
        End Function
        Public Function AddPlaneVectorPolar(aPlane As dxfPlane, aAngle As Double, aRadius As Double, Optional aZ As Double = 0, Optional aVertexRadius As Double? = Nothing, Optional bInRadians As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "", Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aValue As Double? = Nothing) As Integer ' dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the subject plane
            '#2the Angle to apply
            '#3the radius to apply
            '#4the Z coordinate of the new vector
            '#4a vertex radius to assign to the new vector
            '#5a tag to assign to the new vector
            '#6a flag to assign to the new vector
            '#7the index to insert the vector into the collection before
            '#8the index to insert the vector into the collection after
            '#9a coordinate system to define the the new vector by
            '#10a layer name to assign to the new vector
            '#11a color to assign to the new vector
            '#12a linetype to assign to the new vector
            '^shorthand way to add a vector without all the code to create and add one conventionally
            '~returns the new vector that was created.
            _rVal = New dxfVector(New TPLANE(aPlane).AngleVector(aAngle, aRadius, bInRadians, aZ))
            _rVal.LCLSet(aLayer, aColor, aLineType)
            If aVertexRadius.HasValue Then _rVal.VertexRadius = aVertexRadius.Value
            _rVal = AddToCollection(_rVal, aBeforeIndex, aAfterIndex, aTag:=aTag, aFlag:=aFlag, aValue:=aValue)
            If _LastVectorIndex > 0 Then Return _LastVectorIndex Else Return 0
        End Function
        Public Function AddPolar(aAngle As Double, aDistance As Double, Optional aVertexRadius As Double = 0.0, Optional bClockwise As Boolean? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTagValue As String = "", Optional aFlagValue As String = "", Optional aPolarToIndex As Double? = Nothing, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As dxfVector

            '#1angle to use to calculate the new coordinates of the new point
            '#2distance to use to calculate the new coordinates of the new point
            '#3the radius of the new vector (negative values indicate clockwise arc start)
            '#4flag to force clockwise or counterclockwise arcs
            '#5the plane to get the angles from
            '#6a layername to assign to the new vector
            '#7a color to the new vector
            '#8the linetype to assign to the vector
            '#9the tag value to assign to the new vector
            '#10the flag value to assign to the new vector
            '^a shorthand way to add a point without all the code to create and add one conventionally
            '~adds a new point polar relative the last point in the collection.
            '~returns the new point that was created.
            Dim v1 As New TVERTEX

            Dim idx As Integer
            If Count > 0 Then
                If aPolarToIndex.HasValue Then idx = aPolarToIndex.Value Else idx = Count
                If idx < 1 Then idx = 1
                If idx > Count Then idx = Count
                v1 = ItemVertex(idx, True)
            End If
            aPlane = GetPlane(True, aPlane)
            Dim aPln As TPLANE = New TPLANE(aPlane)
            If aDistance <> 0 Then
                v1.Vector = aPln.AngleVector(v1.Vector, aAngle, aDistance, False)
            End If
            Return AddV(v1.Vector, aVertexRadius, bClockwise, aLayerName:=aLayerName, aColor:=aColor, aLineType:=aLineType, aTag:=aTagValue, aFlag:=aFlagValue, aBeforeIndex:=aBeforeIndex, aAfterIndex:=aAfterIndex)
        End Function
        Public Function AddPolarTo(aXYObject As iVector, aAngle As Double, aDistance As Double, Optional aVertexRadius As Double = 0.0, Optional bClockwise As Boolean? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTagValue As String = "", Optional aFlagValue As String = "", Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#2angle to use to calculate the new coordinates of the new point
            '#3distance to use to calculate the new coordinates of the new point
            '#4the radius of the new vector (negative values indicate clockwise arc start)
            '#5flag to force clockwise or counterclockwise arcs
            '#6the plane to get the angles from
            '#7a layername to assign to the new vector
            '#8a color to the new vector
            '#9the linetype to assign to the vector
            '#10the tag value to assign to the new vector
            '#11the flag value to assign to the new vector
            '^a shorthand way to add a point without all the code to create and add one conventionally
            '~adds a new point polar relative the last point in the collection.
            '~returns the new point that was created.
            Dim v1 As New TVECTOR(aXYObject)
            Dim aPln As TPLANE
            aPlane = GetPlane(True, aPlane)
            aPln = New TPLANE(aPlane)
            If aDistance <> 0 Then v1 = aPln.AngleVector(v1, aAngle, aDistance, False)
            _rVal = AddV(v1, aVertexRadius, bClockwise, aLayerName:=aLayerName, aColor:=aColor, aLineType:=aLineType, aTag:=aTagValue, aFlag:=aFlagValue, aBeforeIndex:=aBeforeIndex, aAfterIndex:=aAfterIndex)
            Return _rVal
        End Function
        Public Function AddRelative(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aVertexRadius As Double? = Nothing, Optional bClockwise As Boolean? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aStartWidth As Double? = Nothing, Optional aEndWidth As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "", Optional aRelativeToIndex As Integer? = Nothing, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As dxfVector
            Dim _rVal As dxfVector = Nothing
            If Count <= 0 Then Return Nothing
            '#1the X offset of the new vector
            '#2the Y offset of the new vector
            '#3the Z offset of the new vector
            '#42the radius of the new vector (negative values indicate clockwise arc start)
            '#5flag to force clockwise or counterclockwise arcs
            '#6the plane to get for the X, Y and Z directions
            '#7a start width to assign to the new vector
            '#8an end width to assign to the new vector
            '#9a layername to assign to the new vector
            '#10a color to the new vector
            '#11the linetype to assign to the vector
            '#12the tag value to assign to the new vector
            '#13the flag value to assign to the new vector
            '#14the index of the member to create the new vector relative to (the last vector is assumed)
            '^a shorthand way to add a vector without all the code to create and add one conventionally
            '~adds a new vector relative to the last vector in the collection.
            '~if a plane is passed the the offsets are applied with repect to the planes X, Y and Z directions
            '~otherwise the values are added to the last vectors current X, Y and/or Z ordinates.
            '~returns the new vector that was created.
            Dim idx As Integer = Count
            If aRelativeToIndex.HasValue Then idx = aRelativeToIndex.Value
            Dim v1 As TVERTEX = ItemVertex(idx)
            aPlane = GetPlane(False, aPlane)
            If TPLANE.IsNull(aPlane) Then
                v1.Vector += New TVECTOR(aX, aY, aZ)
            Else
                v1.Vector = New TPLANE(aPlane).VectorRelative(v1.Vector, aX, aY, aZ)
            End If
            AddV(v1.Vector, aVertexRadius, bClockwise, aStartWidth, aEndWidth, aLayerName, aColor, aLineType, aTag, aFlag, aBeforeIndex, aAfterIndex)
            _rVal = _LastVector
            'Return _rVal
            If _LastVectorIndex > 0 Then
                _rVal.Index = _LastVectorIndex
                Return _rVal
            Else
                Return Nothing
            End If
        End Function
        Public Function AddRelativeTo(aXYObject As iVector, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aVertexRadius As Double? = Nothing, Optional bClockwise As Boolean? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aStartWidth As Double? = Nothing, Optional aEndWidth As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "", Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As Integer 'dxfVector
            Dim _rVal As dxfVector = Nothing
            '#2the X offset of the new vector
            '#3the Y offset of the new vector
            '#4the Z offset of the new vector
            '#5the radius of the new vector (negative values indicate clockwise arc start)
            '#6flag to force clockwise or counterclockwise arcs
            '#7the plane to get for the X, Y and Z directions
            '#8a start width to assign to the new vector
            '#9an end width to assign to the new vector
            '#10a layername to assign to the new vector
            '#11a color to the new vector
            '#12the linetype to assign to the vector
            '#13the tag value to assign to the new vector
            '#14the flag value to assign to the new vector
            '^a shorthand way to add a vector without all the code to create and add one conventionally
            '~adds a new vector relative to the last vector in the collection.
            '~returns the new vector that was created.
            Dim v1 As New TVECTOR(aXYObject)
            aPlane = GetPlane(False, aPlane)
            If TPLANE.IsNull(aPlane) Then
                v1 += New TVECTOR(aX, aY, aZ)
            Else
                v1 = New TPLANE(aPlane).VectorRelative(v1, aX, aY, aZ)
            End If
            AddV(v1, aVertexRadius, bClockwise, aStartWidth, aEndWidth, aLayerName, aColor, aLineType, aTag, aFlag, aBeforeIndex, aAfterIndex)
            _rVal = _LastVector
            Return _LastVectorIndex '_rVal
        End Function
        Private Function AddToCollection(aVector As dxfVector, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bSuppressReindex As Boolean = False, Optional bSuppressEvnts As Boolean = False, Optional aAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aRadius As Double? = Nothing) As dxfVector
            _LastVector = Nothing
            _LastVectorIndex = 0
            Dim _rVal As dxfVector = Nothing
            If _SuppressEvents Then bSuppressEvnts = True
            If aVector Is Nothing Then Return Nothing
            Dim bBail As Boolean
            Dim bAddClone As Boolean
            Dim breindex As Boolean = False
            'raised to give the owner a change to inspect or reject the new member
            RaiseChangeEvent(dxxCollectionEventTypes.PreAdd, aVector, $"{Reflection.MethodBase.GetCurrentMethod.Name}.PreAdd", 0, 0, bBail, bAddClone, False)
            If bBail Then Return _rVal
            Try
                If aAddClone Or bAddClone Then
                    _rVal = New dxfVector(aVector)
                Else
                    _rVal = aVector
                End If
                If _Tag <> "" Then _rVal.Tag = _Tag
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
                    MyBase.Add(_rVal)
                    If MaintainIndices Then _rVal.Index = Count
                    _LastVectorIndex = Count
                Else
                    If aBeforeIndex > 0 Then
                        _LastVectorIndex = aBeforeIndex - 1
                        MyBase.Insert(_LastVectorIndex, _rVal)
                    Else
                        _LastVectorIndex = aAfterIndex
                        MyBase.Insert(_LastVectorIndex, _rVal)
                    End If
                    If MaintainIndices And Not bSuppressReindex Then breindex = True
                End If
                If aTag IsNot Nothing Then _rVal.Tag = aTag
                If aFlag IsNot Nothing Then _rVal.Flag = aFlag
                If aValue.HasValue Then _rVal.Value = aValue.Value
                If aRadius.HasValue Then
                    _rVal.Radius = aRadius.Value
                End If

                'Application.DoEvents()
                _LastVectorIndex = MyBase.IndexOf(_rVal) + 1
                If _LastVectorIndex > 0 Then
                    _rVal = SetMemberInfo(_rVal, _LastVectorIndex).Item1
                    _LastVector = _rVal
                End If
                If breindex Then ReIndex()
            Catch
            End Try
            If _rVal IsNot Nothing And Not bSuppressEvnts Then RaiseChangeEvent(dxxCollectionEventTypes.Add, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, True)
            Return _rVal
        End Function
        Friend Overloads Function Add(aVector As TVERTEX) As dxfVector
            Dim _rVal As New dxfVector(aVector)
            MyBase.Add(_rVal)
            Return _rVal
        End Function
        Friend Function AddV(aVector As TVECTOR, Optional aVertexRadius As Double? = Nothing, Optional bClockwise As Boolean? = Nothing, Optional aStartWidth As Double? = Nothing, Optional aEndWidth As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "", Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional aCode As Integer? = Nothing) As dxfVector
            Dim _rVal As dxfVector = Nothing

            '#1the vector which carries the coordinate of the ne member
            '#2the radius of the new vector (negative values indicate clockwise arc start)
            '#3flag to force clockwise or counterclockwise arcs
            '#4a start width to assign to the new vector
            '#5an end width to assign to the new vector
            '#6a layername to assign to the new vector
            '#7a color to the new vector
            '#8the linetype to assign to the vector
            '#9the tag value to assign to the new vector
            '#10the flag value to assign to the new vector
            '^shorthand way to add a vector without all the code to create and add one conventionally
            '~returns the new vector that was created.
            _rVal = New dxfVector(aVector)
            Dim rad As Double = 0
            If aVertexRadius.HasValue Then rad = aVertexRadius.Value
            If rad < 0 Then
                bClockwise = True
                rad *= -1
            End If
            _rVal.VertexRadius = rad
            If bClockwise.HasValue Then _rVal.Inverted = bClockwise.Value
            If aStartWidth.HasValue Then _rVal.StartWidth = Math.Abs(aStartWidth.Value)
            If aEndWidth.HasValue Then _rVal.EndWidth = Math.Abs(aEndWidth.Value)
            If aCode.HasValue Then
                If aCode.Value >= 0 And aCode.Value <= 255 Then
                    _rVal.VertexCode = TVALUES.ToByte(aCode.Value)
                End If
            End If

            _rVal.TFVSet(aTag, aFlag)
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal = AddToCollection(_rVal, aBeforeIndex, aAfterIndex)
            Return _rVal
        End Function


        '#1the X value for the new member
        '#2the Y value for the new member
        '#3the Z value for the new member
        '#4the radius of the new vector (negative values indicate clockwise arc start)
        '#5flag to force clockwise or counterclockwise arcs
        '#6a start width to assign to the new vector
        '#7an end width to assign to the new vector
        '#8a layername to assign to the new vector
        '#9a color to the new vector
        '#10the linetype to assign to the vector
        '#11the tag value to assign to the new vector
        '#12the flag value to assign to the new vector
        '^shorthand way to add a vector without all the code to create and add one conventionally
        '~returns the new vector that was created.
        Public Overloads Function Add(aX As Double, aY As Double, Optional aZ As Double = 0, Optional aVertexRadius As Double? = Nothing, Optional bClockwise As Boolean? = Nothing, Optional aStartWidth As Double? = Nothing, Optional aEndWidth As Double? = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "", Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As dxfVector
            Return AddV(New TVECTOR(aX, aY, aZ), aVertexRadius, bClockwise, aStartWidth, aEndWidth, aLayerName, aColor, aLineType, aTag, aFlag, aBeforeIndex, aAfterIndex)
        End Function
        Public Overloads Function Add(aVector As iVector, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional bAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aLayerName As String = Nothing, Optional aVertexRadius As Double? = Nothing) As dxfVector
            '#1the object with X,Yand Z properties to add to the collection
            '^used to add vector objects to the collection
            '~won't add nothing to the collection (no error is raised).
            '~returns True if the vector was added
            If aVector Is Nothing Then Return Nothing
            Dim vadd As dxfVector = Nothing
            If TypeOf (aVector) Is dxfVector Then
                vadd = DirectCast(aVector, dxfVector)
                If bAddClone Then vadd = New dxfVector(vadd)
            Else
                vadd = New dxfVector(aVector)
                bAddClone = False
            End If
            If aVertexRadius.HasValue Then
                Dim rad As Double = aVertexRadius.Value
                If rad < 0 Then
                    vadd.Inverted = True
                    rad *= -1
                End If
                vadd.VertexRadius = rad

            End If

            Dim _rVal As dxfVector = AddToCollection(vadd, aBeforeIndex, aAfterIndex, aAddClone:=False, aTag:=aTag, aFlag:=aFlag, aValue:=aValue)
            If _rVal IsNot Nothing Then
                If aLayerName IsNot Nothing Then _rVal.LayerName = aLayerName
            End If
            Return _rVal
        End Function
        Public Overloads Function Add(aPlane As dxfPlane, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aVertexRadius As Double = 0.0, Optional aTag As String = "", Optional aFlag As String = "", Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aValue As Double = 0) As dxfVector

            '#1the subject plane
            '#2the X coordinate of the new vector
            '#3the Y coordinate of the new vector
            '#4the Z coordinate of the new vector
            '#5a vertex radius to assign to the new vector
            '#6a tag to assign to the new vector
            '#7a flag to assign to the new vector
            '#8the index to insert the vector into the collection before
            '#9the index to insert the vector into the collection after
            '#10a coordinate system to define the the new vector by
            '#11a layer name to assign to the new vector
            '#12a color to assign to the new vector
            '#13a linetype to assign to the new vector
            '^shorthand way to add a vector without all the code to create and add one conventionally
            '~if a plane is passed the the coordinated of the new vector will be with repect to the planes origin and X, Y and Z directions
            '~otherwise the  X, Y and Z ordinates are applied as passed.
            '~returns the new vector that was created.
            Dim _rVal As New dxfVector(GetPlane(False, aPlane), aX, aY, aZ, aTag, aFlag) With
            {
               .LayerName = aLayer,
               .Color = aColor,
               .Linetype = aLineType,
               .VertexRadius = aVertexRadius,
            .Value = aValue
            }
            _rVal = AddToCollection(_rVal, aBeforeIndex, aAfterIndex)
            If _LastVectorIndex > 0 Then Return _rVal Else Return Nothing
        End Function
        Public Function AddAfter(aVector As dxfVector, aAfterMember As dxfVector, Optional bAddClone As Boolean = False, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing) As dxfVector
            '#1the vector to add to the collection
            '^used to add vector objects to the collection
            '~won't add nothing to the collection (no error is raised).
            '~returns True if the vector was added
            If aVector Is Nothing Then Return Nothing
            If Count <= 0 Or aAfterMember Is Nothing Then Return Add(aVector, 0, 0, bAddClone, aTag, aFlag, aValue)
            Dim after As Integer = MyBase.IndexOf(aAfterMember) + 1
            AddToCollection(aVector, 0, after, aAddClone:=bAddClone, aTag:=aTag, aFlag:=aFlag, aValue:=aValue)
            If _LastVectorIndex > 0 Then Return MyBase.Item(_LastVectorIndex - 1) Else Return Nothing
        End Function
        Public Function Append(NewVectors As IEnumerable(Of iVector), Optional bAppendClones As Boolean = True, Optional bClearCurrent As Boolean = False, Optional aProjectPlane As dxfPlane = Nothing, Optional aStartID As Integer = -1, Optional aEndID As Integer = -1, Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aRadius As Double? = Nothing) As Integer
            Dim _rVal As Integer = 0
            '#1a collection of vectors to add to the current collection
            '#2flag to add clones of the passed vectors
            '#3flag to remove any existing members first
            '#4a plane to project the new members onto before adding them
            '^appends the members of the passed vectors to the current collection
            If bClearCurrent Then Clear()
            If NewVectors Is Nothing Then Return _rVal
            If NewVectors.Count <= 0 Then Return _rVal


            Dim bProject As Boolean
            Dim si As Integer
            Dim ei As Integer
            Dim astp As Integer
            Dim aPlane As New TPLANE("")
            dxfUtils.LoopIndices(NewVectors.Count, aStartID, aEndID, si, ei, Nothing, rStep:=astp)
            bProject = Not dxfPlane.IsNull(aProjectPlane)
            If bProject Then
                aPlane = New TPLANE(aProjectPlane)
                bAppendClones = False
            End If
            _rVal = 0
            For i As Integer = si To ei Step astp

                Dim v1 As dxfVector = dxfVector.FromIVector(NewVectors(i - 1), bCloneIt:=bAppendClones)
                If v1 Is Nothing Then Continue For

                If bProject Then
                    v1 = v1.ProjectedToPlane(aPlane)
                End If
                If AddToCollection(v1, bSuppressEvnts:=True, aAddClone:=False, aTag:=aTag, aFlag:=aFlag, aValue:=aValue, aRadius:=aRadius) IsNot Nothing Then _rVal += 1
            Next i
            If Not _SuppressEvents And _rVal > 0 Then
                RaiseChangeEvent(dxxCollectionEventTypes.Append, NewVectors, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, True)
            End If
            Return _rVal
        End Function

        Public Function Append(aCoordinateString As String, Optional aDelimitor As Char = "¸") As Integer
            '#1a String containg vector coordinates separated by the delimitor
            '#2the delimitor character that the string is joined with. Defalt = '¸" Char 184
            '^appends the members of the passed vectors to the current collection
            If String.IsNullOrWhiteSpace(aCoordinateString) Then Return 0
            If Char.IsWhiteSpace(aDelimitor) Then Return 0
            Dim sVals() As String = aCoordinateString.Split(aDelimitor)

            For i As Integer = 0 To sVals.Length - 1
                Add(New dxfVector(sVals(i)))
            Next i
            Return sVals.Length
        End Function
        Friend Sub Append(aVectors As TVECTORS, Optional aTag As String = Nothing, Optional aFlag As String = Nothing)
            Try

                If aVectors.Count <= 0 Then Return
                Dim cnt As Integer = Count
                Dim tg As String = IIf(aTag Is Nothing, "", aTag)
                Dim fg As String = IIf(aFlag Is Nothing, "", aFlag)
                For i As Integer = 1 To aVectors.Count
                    cnt += 1
                    Dim mem As New dxfVector(aVectors.Item(i)) With {.Index = cnt, .Tag = tg, .Flag = fg}

                    MyBase.Add(mem)
                Next i

            Catch ex As Exception
            End Try
        End Sub
        Friend Sub Append(aVectors As TVERTICES, Optional aTag As String = Nothing, Optional aFlag As String = Nothing)
            Try
                If aVectors.Count <= 0 Then Return


                Dim cnt As Integer = Count

                For i As Integer = 0 To aVectors.Count - 1
                    cnt += 1
                    Dim mem As New dxfVector(aVectors.Item(i)) With {.Index = cnt}
                    If aTag IsNot Nothing Then mem.Tag = aTag
                    If aFlag IsNot Nothing Then mem.Flag = aFlag
                    MyBase.Add(mem)

                Next i

            Catch ex As Exception
            End Try
        End Sub
        Public Function AppendMirrors(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aStartID As Integer = 0, Optional aEndID As Integer = 0, Optional bReverseOrder As Boolean = False) As List(Of dxfVector)
            '#1the line to mirror around
            '^adds the mirror points of the current collection to the collection
            If (aMirrorX Is Nothing And aMirrorY Is Nothing) Or Count <= 0 Then Return New List(Of dxfVector)
            Dim si As Integer
            Dim ei As Integer
            Dim stp As Integer
            Dim v1 As TVERTEX
            Dim aPl As New TPLANE(aPlane)
            Dim aTrs As New TTRANSFORMS

            If aMirrorX IsNot Nothing Then
                If aMirrorX.HasValue Then aTrs.Add(TTRANSFORM.CreateMirror(aPl.LineV(aMirrorX.Value, 10), True))
            End If
            If aMirrorY IsNot Nothing Then
                If aMirrorY.HasValue Then aTrs.Add(TTRANSFORM.CreateMirror(aPl.LineH(aMirrorY.Value, 10), True))
            End If
            Dim apnd As New List(Of dxfVector)
            dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei, bReverseOrder, stp)
            For i As Integer = si To ei Step stp
                v1 = ItemVertex(i)
                TTRANSFORMS.Apply(aTrs, v1)
                apnd.Add(New dxfVector(v1))
            Next
            If apnd.Count > 0 Then
                Append(apnd)
                If Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.Append, New colDXFVectors(apnd), Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, True)
            End If
            Return apnd
        End Function
        Public Function ArcVertex(aRadius As Double, Optional aOccurance As Integer = 1, Optional aPrecis As Integer = 3, Optional aReturnClone As Boolean = False) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the radius to search for
            '#2the instance to return
            '#3the precision for the comparison
            '#4flag to return a copy of the matching vector
            '#5returns the collection index of the matching vector
            '^returns the first vector in the collection whose radius property match the passed radius
            Dim i As Integer
            Dim v1 As dxfVector
            Dim rad As Double
            Dim cnt As Integer
            Dim rIndex As Integer = 0
            If aOccurance <= 0 Then aOccurance = 1
            If aPrecis <= 0 Then aPrecis = 0
            If aPrecis > 10 Then aPrecis = 10
            rad = Math.Round(aRadius, aPrecis)
            rIndex = 0
            For i = 1 To Count
                v1 = Item(i)
                If Math.Round(v1.Radius, aPrecis) = rad Then
                    cnt += 1
                    If cnt = aOccurance Then
                        rIndex = i
                        Exit For
                    End If
                End If
            Next i
            If rIndex > 0 Then _rVal = Item(rIndex)
            Return _rVal
        End Function
        Public Function AreSymetric(Optional aOrdinateType As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X, Optional aOrigin As iVector = Nothing, Optional aPlane As dxfPlane = Nothing, Optional rNonSymetricCenters As colDXFVectors = Nothing, Optional aPrecis As Integer = 3) As Boolean
            Dim _rVal As Boolean
            '#1the ordinate to tests
            '#2the ordinate to align the axis on
            '
            '^returns True if the points are symetric with regards to the indicated ordinate across an axis located at the given origin on the given plane
            If aOrdinateType < dxxOrdinateDescriptors.X Or aOrdinateType > dxxOrdinateDescriptors.Z Then aOrdinateType = dxxOrdinateDescriptors.X
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If Count <= 0 Then Return True
            Dim v1 As TVERTEX
            Dim v2 As TVERTEX
            Dim cnt As Integer
            Dim bFoundOne As Boolean
            Dim bPlane As New TPLANE("")
            If Not dxfPlane.IsNull(aPlane) Then bPlane = New TPLANE(aPlane)
            If aOrigin IsNot Nothing Then bPlane.Origin = New TVECTOR(aOrigin)
            Dim aVrts As TVERTICES = PlanarVectorsV(bPlane, aPrecis, True) 'get the planar vectors with the precision applied and the marks set to false
            For i As Integer = 1 To aVrts.Count
                If aOrdinateType = dxxOrdinateDescriptors.X Then
                    If aVrts.X(i) = 0 Then aVrts.SetMark(True, i)
                ElseIf aOrdinateType = dxxOrdinateDescriptors.Y Then
                    If aVrts.Y(i) = 0 Then aVrts.SetMark(True, i)
                Else
                    If aVrts.Z(i) = 0 Then aVrts.SetMark(True, i)
                End If
                v1 = aVrts.Item(i)
                If Not v1.Mark Then
                    bFoundOne = False
                    For j As Integer = 1 To aVrts.Count
                        If j <> i Then
                            If aOrdinateType = dxxOrdinateDescriptors.X Then
                                If aVrts.X(j) = 0 Then aVrts.SetMark(True, j)
                            ElseIf aOrdinateType = dxxOrdinateDescriptors.Y Then
                                If aVrts.Y(j) = 0 Then aVrts.SetMark(True, j)
                            Else
                                If aVrts.Z(j) = 0 Then aVrts.SetMark(True, j)
                            End If
                            v2 = aVrts.Item(j)
                            If Not v2.Mark Then
                                If aOrdinateType = dxxOrdinateDescriptors.X Then
                                    If v1.X = -v2.X Then
                                        bFoundOne = True
                                    End If
                                ElseIf aOrdinateType = dxxOrdinateDescriptors.Y Then
                                    If v1.Y = -v2.Y Then
                                        bFoundOne = True
                                    End If
                                Else
                                    If v1.Z = -v2.Z Then
                                        bFoundOne = True
                                    End If
                                End If
                            End If
                        End If
                        If bFoundOne Then
                            aVrts.SetMark(True, i)
                            aVrts.SetMark(True, j)
                            Exit For
                        End If
                    Next j
                    If Not bFoundOne Then
                        If rNonSymetricCenters IsNot Nothing Then rNonSymetricCenters.Add(Item(v1.Index))
                        cnt += 1
                    End If
                End If
            Next i
            _rVal = cnt = 0
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the 2D area summation of all the vectors in the collection
        ''' </summary>
        ''' <param name="aPlane">the working plane</param>
        ''' <returns></returns>
        Public Function AreaSummation(Optional aPlane As dxfPlane = Nothing) As Double
            Dim rPlanarVectors As colDXFVectors = Nothing
            Return dxfVectors.AreaSummation(Me, aPlane, rPlanarVectors)
        End Function

        ''' <summary>
        ''' returns the 2D area summation of all the vectors in the collection
        ''' </summary>
        ''' <param name="aPlane">the working plane</param>
        ''' <param name="rPlanarVectors">returns the members defined with respect to the working plane</param>
        ''' <returns></returns>
        Public Function AreaSummation(aPlane As dxfPlane, ByRef rPlanarVectors As colDXFVectors) As Double
            Return dxfVectors.AreaSummation(Me, aPlane, rPlanarVectors)
        End Function
        Public Sub AssignRowsAndColumns(Optional aPlane As dxfPlane = Nothing, Optional bBottomToTop As Boolean = False, Optional bRightToLeft As Boolean = False, Optional aPrecis As Integer = 4)
            Dim rRowCount As Integer = 0
            Dim rColCount As Integer = 0
            Dim YOrds As List(Of Double) = Ordinates(dxxOrdinateDescriptors.Y, aPrecis, aPlane)
            Dim XOrds As List(Of Double) = Ordinates(dxxOrdinateDescriptors.X, aPrecis, aPlane)
            Dim P1 As dxfVector
            Dim v1 As TVECTOR
            Dim aPl As New TPLANE(aPlane)
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            rRowCount = YOrds.Count
            rColCount = XOrds.Count
            YOrds.Sort()
            If bBottomToTop Then YOrds.Reverse()
            XOrds.Sort()
            If Not bRightToLeft Then XOrds.Reverse()
            For i As Integer = 1 To Count
                P1 = Item(i)
                v1 = P1.Strukture.WithRespectTo(aPl, aPrecis)
                P1.Row = 0 : P1.Col = 0
                For j As Integer = 1 To YOrds.Count
                    If v1.Y = YOrds.Item(j) Then
                        P1.Row = j
                        Exit For
                    End If
                Next j
                For j As Integer = 1 To XOrds.Count
                    If v1.X = XOrds.Item(j) Then
                        P1.Col = j
                        Exit For
                    End If
                Next j
            Next i
        End Sub
        Public Function Below(aYValue As Double, Optional bOnisIn As Boolean = True, Optional aPrecis As Integer = 3, Optional aPlane As dxfPlane = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxfVector)
            Return GetVectors(aFilter:=dxxPointFilters.LessThanY, aOrdinate:=aYValue, bOnIsIn:=bOnisIn, aPlane:=aPlane, aPrecis:=aPrecis, bReturnClones:=bReturnClones, bRemove:=bRemove)
        End Function
        Public Function BoundingRectangle(Optional aPlane As dxfPlane = Nothing, Optional bSuppressProjection As Boolean = False) As dxfRectangle
            Return New dxfRectangle(Me, aPlane, bSuppressProjection)
        End Function
        Friend Function BoundingRectangleV(aPlane As TPLANE) As TPLANE
            Return New TVECTORS(Me).Bounds(aPlane)
        End Function
        Public Function Center(Optional aStartID As Integer = 0, Optional aEndID As Integer = 0) As dxfVector
            Return Center(Me, aStartID, aEndID)
        End Function
        Public Shared Function Center(SearchCol As List(Of dxfVector), Optional aStartID As Integer = 0, Optional aEndID As Integer = 0) As dxfVector
            If SearchCol Is Nothing Then Return Nothing
            If SearchCol.Count <= 0 Then Return Nothing
            If SearchCol.Count = 1 Then Return New dxfVector(SearchCol.Item(0))
            '#1an optional VB collection of vectors to get the center of
            '^the center of all the vectors in the collection
            '^the center is the average of all the individual coordinates
            Dim sumationX As Double
            Dim sumationY As Double
            Dim sumationZ As Double
            Dim v1 As dxfVector
            Dim si As Integer = 0
            Dim ei As Integer = 0
            Dim stp As Integer = 1
            Dim n As Integer = SearchCol.Count
            dxfUtils.LoopIndices(n, aStartID, aEndID, si, ei)
            For i As Integer = si To ei
                v1 = SearchCol.Item(i - 1)
                If v1 IsNot Nothing Then
                    sumationX += v1.X
                    sumationY += v1.Y
                    sumationZ += v1.Z
                End If
            Next
            Return New dxfVector(New TVECTOR(sumationX / n, sumationY / n, sumationZ / n))
        End Function
        Public Function CenterPoint(Optional aPlane As dxfPlane = Nothing) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '^returns a point that is centered between the minimum and maximum X and Y ordinates
            '~this does not represent the centroid
            Dim aMaxX As Double
            Dim aMinX As Double
            Dim aMaxY As Double
            Dim aMinY As Double
            GetExtremeOrdinates(aMaxX, aMinX, aMaxY, aMinY, 0, 0, aPlane)
            _rVal = New dxfVector
            _rVal.SetCoordinates(aMinX + (aMaxX - aMinX) / 2, aMinY + (aMaxY - aMinY) / 2)
            Return _rVal
        End Function
        Friend Function Center(Optional aStartID As Integer = 0, Optional aEndID As Integer = 0, Optional aProjectionPlane As dxfPlane = Nothing) As TVECTOR

            '^the center of all the vectors in the collection
            '^the center is the average of all the individual coordinates
            Dim n As Integer = Count
            If n = 0 Then Return TVECTOR.Zero
            If n = 1 Then Return ItemVector(1)
            Dim si As Integer
            Dim ei As Integer
            If Not dxfUtils.LoopIndices(n, aStartID, aEndID, si, ei) Then Return TVECTOR.Zero

            Dim _rVal As TVECTOR = TVECTOR.Zero
            Dim sumationX As Double
            Dim sumationY As Double
            Dim sumationZ As Double


            Dim aPlane As New TPLANE(aProjectionPlane)
            Dim bProj As Boolean = Not dxfPlane.IsNull(aProjectionPlane)


            n = (si - ei) + 1
            For i As Integer = si To ei
                Dim v1 As TVECTOR = ItemVector(i, True)
                If bProj Then v1.ProjectTo(aPlane)
                sumationX += v1.X
                sumationY += v1.Y
                sumationZ += v1.Z
            Next i
            Return New TVECTOR(sumationX / n, sumationY / n, sumationZ / n)

        End Function

        ''' <summary>
        ''' computes the centroid of the points in the collection
        ''' </summary>
        ''' <param name="aPlane">the plane to use (world by default)</param>
        ''' <returns></returns>
        Public Function Centroid(Optional aPlane As dxfPlane = Nothing) As dxfVector
            Dim rArea As Double = 0
            Dim rPlanarVectors As colDXFVectors = Nothing
            Return dxfVectors.Centroid(Me, aPlane, rArea, rPlanarVectors)
        End Function
        ''' <summary>
        ''' computes the centroid of the points in the collection
        ''' </summary>
        ''' <param name="aPlane">the plane to use (world by default)</param>
        ''' <param name="rArea">returns the area defined by the points</param>
        ''' <param name="rPlanarVectors">returns the members projected to the working plane</param>
        ''' <returns></returns>
        Public Function Centroid(aPlane As dxfPlane, ByRef rArea As Double, ByRef rPlanarVectors As colDXFVectors) As dxfVector

            Return dxfVectors.Centroid(Me, aPlane, rArea, rPlanarVectors)

        End Function

        ''' <summary>
        ''' returns a circle that encompasses all the member vectors projected to the working plane
        ''' </summary>
        ''' <param name="aPlane">the plane to use</param>
        ''' <returns></returns>
        Public Function BoundingCircle(Optional aPlane As dxfPlane = Nothing) As dxeArc
            Return dxfVectors.BoundingCircle(Me, aPlane)
        End Function
        ''' <summary>
        ''' returns the points that make up convex hull of the vectors with respect to the passed plane
        ''' </summary>
        '''<remarks>In geometry, a convex hull is the smallest convex shape that encloses a set of points, often visualized as the shape formed by a rubber band stretched around nails pounded into a plane</remarks>
        ''' <param name="aPlane">the subject plane</param>
        ''' <param name="bOnBorder"></param>
        ''' <returns></returns>
        Public Function ConvexHull(Optional bOnBorder As Boolean = False, Optional aPlane As dxfPlane = Nothing) As List(Of dxfVector)
            Return dxfVectors.ConvexHull(Me, aPlane, bOnBorder)
        End Function
        Friend Function PlanarVectorsV(aPlane As TPLANE, Optional aPrecis As Integer = 0, Optional bSetMark As Boolean = False) As TVERTICES
            Dim _rVal As New TVERTICES(Me)
            For i As Integer = 1 To Count
                Dim v1 As TVERTEX = _rVal.Item(i)
                v1.Vector = v1.Vector.WithRespectTo(aPlane, aPrecis)
                v1.Index = i
                If bSetMark Then v1.Mark = False
                _rVal.SetItem(i, v1)
            Next i
            Return _rVal
        End Function

        Public Overloads Sub Clear()
            '^resets the wrapped collection to a new collection
            Dim bUndo As Boolean
            Dim colWuz As New List(Of dxfVector)

            For Each v1 As dxfVector In Me
                v1.ReleaseCollectionReference()
                colWuz.Add(v1)
            Next
            MyBase.Clear()

            If Not _SuppressEvents And colWuz.Count > 0 Then
                RaiseChangeEvent(dxxCollectionEventTypes.Clear, colWuz, Reflection.MethodBase.GetCurrentMethod.Name, 0, colWuz.Count, bUndo, False, True)
                If bUndo Then
                    MyBase.AddRange(colWuz)
                Else
                    For Each v1 As dxfVector In colWuz
                        v1.ReleaseCollectionReference()
                    Next
                    If MonitorMembers Then
                        _CollectionGUID = dxfEvents.NextVectorsGUID
                    End If
                End If
            End If
        End Sub
        Public Function Clone(Optional bReturnEmpty As Boolean = False) As colDXFVectors
            '^returns a new collection of vectors that is an exact copy
            '^of the current collection
            Dim _rVal As New colDXFVectors With {
                .SuppressEvents = True,
                .Tag = _Tag,
                .MaintainIndices = _MaintainIndices
            }
            If Not bReturnEmpty Then
                For i As Integer = 1 To Count
                    _rVal.Add(Item(i), bAddClone:=True)
                Next i
            End If
            _rVal.SuppressEvents = _SuppressEvents
            Return _rVal
        End Function
        Public Function Color(aIndex As Integer) As dxxColors
            Dim _rVal As dxxColors = dxxColors.ByLayer
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            _rVal = MyBase.Item(aIndex - 1).Color
            If _rVal = dxxColors.Undefined Then _rVal = dxxColors.ByLayer
            Return _rVal
        End Function
        Public Function Colors(Optional aColorToInclude As dxxColors = dxxColors.Undefined, Optional bUnquieValues As Boolean = True) As List(Of dxxColors)
            Dim _rVal As New List(Of dxxColors)
            '^all of the colors referenced by the entities in the collection
            If aColorToInclude <> dxxColors.Undefined Then _rVal.Add(aColorToInclude)
            Dim aMem As dxfVector
            Dim lname As dxxColors
            Dim keep As Boolean
            For i As Integer = 1 To Count
                aMem = MyBase.Item(i - 1)
                lname = aMem.Color
                If lname = dxxColors.Undefined Then lname = dxxColors.ByLayer
                keep = True
                If bUnquieValues Then
                    keep = _rVal.IndexOf(lname) < 0
                End If
                If keep Then _rVal.Add(lname)
            Next i
            Return _rVal
        End Function
        Public Function Colinear() As Boolean
            Dim d1 As dxfDirection = Nothing
            Dim l1 As dxeLine = Nothing
            Return Colinear(d1, l1)
        End Function
        Public Function Colinear(ByRef rDirection As dxfDirection, ByRef rLine As dxeLine) As Boolean
            Dim _rVal As Boolean
            '^returns True if all the members lie aINteger a Single line
            rDirection = New dxfDirection
            rLine = New dxeLine
            rLine.StartPt.Strukture = ItemVector(1)
            rLine.EndPt.Strukture = ItemVector(1)
            If Count <= 1 Then Return _rVal
            _rVal = True
            Dim aDir As TVECTOR
            Dim bDir As TVECTOR
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim bFlag As Boolean
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            '**UNUSED VAR** Dim v3 As TVECTOR
            Dim bAntiP As Boolean
            Dim lstrt As Double
            Dim lend As Double
            Dim lng As Double
            v1 = ItemVector(1)
            sp = v1
            ep = v1
            For i = 2 To Count
                v2 = ItemVector(i)
                If Not v1.Equals(v2, 4) Then
                    If Not bFlag Then
                        aDir = v1.DirectionTo(v2, False, lng)
                        rDirection.Strukture = aDir
                        ep = v2
                        lend = lng
                        bFlag = True
                    Else
                        bDir = v1.DirectionTo(v2, False, lng)
                        If Not aDir.Equals(bDir, True, 4, bAntiP) Then
                            _rVal = False
                            Exit For
                        Else
                            If bAntiP Then
                                If lng > lstrt Then
                                    lstrt = lng
                                    sp = v2
                                End If
                            Else
                                If lng > lend Then
                                    lend = lng
                                    ep = v2
                                End If
                            End If
                        End If
                    End If
                End If
            Next i
            rLine.StartPt.Strukture = sp
            rLine.EndPt.Strukture = ep
            Return _rVal
        End Function
        Public Function ConnectingLines(Optional bClosed As Boolean = False, Optional aTag As String = "", Optional aLineWidth As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities

            If Count <= 1 Then Return _rVal
            Dim i As Integer
            Dim aL As dxeLine
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim P1 As dxfVector
            For i = 1 To Count - 1
                P1 = Item(i)
                v1 = P1.Strukture
                v2 = ItemVector(i + 1)
                If dxfProjections.DistanceTo(v1, v2) > 0.0001 Then
                    aL = New dxeLine With {
                        .SuppressEvents = True
                    }
                    aL.StartPt.SetStructure(v1)
                    aL.EndPt.SetStructure(v2)
                    If aDisplaySettings Is Nothing Then aL.CopyDisplayValues(P1.DisplaySettings) Else aL.CopyDisplayValues(aDisplaySettings)
                    aL.StartWidth = aLineWidth
                    aL.EndWidth = aLineWidth
                    If aTag <> "" Then aL.Tag = aTag
                    aL.SuppressEvents = False
                    _rVal.Add(aL)
                End If
            Next i
            If bClosed And Count > 2 Then
                P1 = LastVector()
                v1 = P1.Strukture
                v2 = ItemVector(1)
                If dxfProjections.DistanceTo(v1, v2) > 0.0001 Then
                    aL = New dxeLine With {
                        .SuppressEvents = True
                    }
                    aL.StartPt.SetStructure(v1)
                    aL.EndPt.SetStructure(v2)
                    If aDisplaySettings Is Nothing Then aL.CopyDisplayValues(P1.DisplaySettings) Else aL.CopyDisplayValues(aDisplaySettings)
                    aL.StartWidth = aLineWidth
                    aL.EndWidth = aLineWidth
                    aL.Tag = aTag
                    aL.SuppressEvents = False
                    _rVal.Add(aL)
                End If
            End If
            Return _rVal
        End Function
        Public Function ConvergeTo(aVectorObj As iVector) As Boolean
            Return ConvergeTo(New TVECTOR(aVectorObj), True)
            '#1the object to move to
            '^moves all the members to the passed point

        End Function
        Friend Function ConvergeTo(aVector As TVECTOR, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the vector to move to
            '^moves all the members to the passed vector
            If Count <= 0 Then Return _rVal


            Dim sWas As String = String.Empty
            Dim sIs As String = String.Empty
            Dim bUndo As Boolean
            Dim movers As New List(Of dxfVector)
            If _SuppressEvents Then bSuppressEvnts = True
            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                If _CollectionGUID <> "" Then aMem.CollectionGUID = _CollectionGUID
                If MaintainIndices Then aMem.Index = i
                sWas += aMem.Coordinates
                If aMem.SetComponentsV(aVector.X, aVector.Y, aVector.Z, True) Then
                    movers.Add(aMem)
                    _rVal = True
                End If
                sIs += aMem.Coordinates
            Next i
            If _rVal And Not bSuppressEvnts Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, sWas, sIs, bUndo, False, False)
            End If
            Return _rVal
        End Function
        Public Sub ConvertToPlane(aPlane As dxfPlane)
            '#1the plane to convert to
            '^converts the members to coordiates with respect to the passed plane

            ConvertToPlane(New TPLANE(aPlane))

        End Sub
        Friend Sub ConvertToPlane(aPlane As TPLANE)
            If TPLANE.IsNull(aPlane) Then Return

            Dim newCol As New List(Of dxfVector)
            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i, bReturnClone:=True)

                v1.Strukture = aPlane.WorldVector(v1.Strukture)
                newCol.Add(v1)
            Next i
            MyBase.Clear()
            MyBase.AddRange(newCol)
        End Sub
        Public Function ConvertedToPlane(aPlane As dxfPlane) As colDXFVectors
            Dim _rVal As New colDXFVectors With {.Tag = _Tag, .MaintainIndices = _MaintainIndices}
            Dim aPl As New TPLANE(aPlane)

            '#1the plane to convert to
            '^converts the members to coordiates with respect to the passed plane

            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i, True)
                aMem.Strukture = aMem.Strukture.WithRespectTo(aPl)
                _rVal.Add(aMem)
            Next i
            Return _rVal
        End Function
        Public Function CoordinatesR(Optional aPrecis As Integer = 3, Optional aDelimiter As String = "¸", Optional bIndexed As Boolean = False, Optional aMultiplier As Double = 0.0, Optional bSuppressZ As Boolean = False, Optional bSuppressParens As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '^a concantonated string of all the vector coordinates in the collection
            '~the delimitor is "¸" (char 184) and the vector ordinates are round to the passed precision

            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                If Not bIndexed Then
                    If _rVal <> "" Then _rVal += aDelimiter
                    _rVal += v1.CoordinatesR(aPrecis, aMultiplier, bSuppressZ, bSuppressParens)
                Else
                    If _rVal <> "" Then _rVal += vbLf
                    _rVal += $"{ i} - { v1.CoordinatesR(aPrecis, aMultiplier, bSuppressZ, bSuppressParens)}"
                End If
            Next i
            Return _rVal
        End Function
        Public Function CopyDisplayValues(aEntitySet As dxfDisplaySettings, Optional aMatchLayer As String = "", Optional aMatchColor As dxxColors = dxxColors.Undefined, Optional aMatchLineType As String = "") As Boolean
            Dim _rVal As Boolean
            '#1the entity settings to copy
            '#2a layer name that if passed the entities layer name will not be changed unless it currently matches this string
            '#3a color that if defined the entities color will not be changed unless it currently matches this value
            '#4a linetype name that if passed the entities linetype name will not be changed unless it currently matches this string
            '^copies the values of the passed display settings to this entities display settings
            If aEntitySet Is Nothing Then Return _rVal

            For i As Integer = 1 To Count
                Dim aV As dxfVector = Item(i)
                If aV.CopyDisplayValues(aEntitySet, aMatchLayer, aMatchColor, aMatchLineType) Then _rVal = True
            Next i
            Return _rVal
        End Function

        ''' <summary>
        ''' appends to the collection based on then passed ValueString
        ''' </summary>
        ''' <remarks>a value string is X,Y, Value</remarks>
        ''' <param name="aValString"></param>
        ''' <param name="aDelimiter"></param>
        ''' <param name="aZDestination">if the passed </param>

        Public Sub DefineValueString(aValString As String, Optional aDelimiter As Char = "¸", Optional aZDestination As String = "VALUE")

            If String.IsNullOrWhiteSpace(aDelimiter) Then aDelimiter = dxfGlobals.Delim
            If String.IsNullOrWhiteSpace(aZDestination) Then aZDestination = "VALUE"

            aZDestination = aZDestination.Trim().ToUpper()
            Dim strVals() As String = aValString.Split(aDelimiter)
            Clear()
            For i As Integer = 0 To strVals.Length - 1
                Dim vStr As String = strVals(i)
                If Not vStr.Contains(",") Then Continue For
                Do While vStr.StartsWith("(")
                    vStr = vStr.Substring(1, vStr.Length - 1).Trim()
                Loop
                Do While vStr.EndsWith("}")
                    vStr = vStr.Substring(0, vStr.Length - 1).Trim()
                Loop
                Dim sVals() As String = vStr.Split(",")
                If sVals.Length < 2 Then Continue For
                Dim aMem As New dxfVector(TVALUES.To_DBL(sVals(0)), TVALUES.To_DBL(sVals(1)))
                If sVals.Length < 3 Then Continue For
                Dim sVal As Double = 0D
                If TVALUES.IsNumber(sVals(2)) Then sVal = TVALUES.To_DBL(sVals(2))
                Select Case aZDestination
                    Case "VALUE"
                        aMem.Value = sVal
                    Case "ROTATION"
                        aMem.Rotation = sVal
                    Case "Z"
                        aMem.Z = sVal
                    Case "RADIUS"
                        aMem.Radius = sVal
                    Case "TAG"
                        aMem.Tag = sVals(2)
                    Case "FLAG"
                        aMem.Flag = sVals(2)
                End Select
                Add(aMem)


            Next i
        End Sub

        ''' <summary>
        ''' returns the vector from the collection which is the farthest from the passed vector
        ''' </summary>
        ''' <param name="aSearchVector">the vector to compare to</param>
        ''' <param name="bReturnClone"> flag to return a clone</param>
        ''' <param name="bRemove">flag to remove the nearest vector form this collection</param>
        ''' <returns></returns>
        Public Function FarthestVector(aSearchVector As iVector, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfVector
            Dim rDistance As Double = 0.0
            Return FarthestVector(aSearchVector, rDistance, bReturnClone:=bReturnClone, bRemove:=bRemove)
        End Function

        ''' <summary>
        ''' returns the vector from the collection which is the farthest from the passed vector
        ''' </summary>
        ''' <param name="aSearchVector">the vector to compare to</param>
        ''' <param name="rDistance">returns the distance between the search vector and the farthest vector (if there is one)</param>
        ''' <param name="aDirection">if passed, only members whose direction to or from the passed search vector are considered</param>
        ''' <param name="bCompareInverseDirection">flag to only consider members whose direction is from the search vector to the member if a direction is passed</param>
        ''' <param name="bReturnClone"> flag to return a clone</param>
        ''' <param name="bRemove">flag to remove the nearest vector form this collection</param>
        ''' <returns></returns>
        Public Function FarthestVector(aSearchVector As iVector, ByRef rDistance As Double, Optional aDirection As dxfDirection = Nothing, Optional bCompareInverseDirection As Boolean = False, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfVector
            rDistance = 0
            If aSearchVector Is Nothing Then Return Nothing
            If Count <= 0 Then Return Nothing

            Dim rIndex As Integer = 0
            Dim _rVal As dxfVector = DirectCast(dxfVectors.GetRelativeMember(False, Me, aSearchVector, rDistance, rIndex, aDirection, bCompareInverseDirection), dxfVector)

            If rIndex <= 0 Then Return Nothing
            rIndex = MyBase.IndexOf(_rVal) + 1
            If bRemove Then Remove(_rVal)
            If bReturnClone Then
                Return New dxfVector(_rVal)
            Else
                Return _rVal
            End If

        End Function

        Public Overloads Function RemoveAll(match As Predicate(Of dxfVector)) As List(Of dxfVector)
            Dim cnt As Integer = Count
            If cnt <= 0 Or match Is Nothing Then Return New List(Of dxfVector)
            Dim _rVal As List(Of dxfVector) = MyBase.FindAll(match)
            If (_rVal.Count <= 0) Then Return _rVal
            Dim canc As Boolean
            RaiseChangeEvent(dxxCollectionEventTypes.PreRemove, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, cnt, cnt - _rVal.Count, canc, False, True)
            If canc Then Return New List(Of dxfVector)
            For Each item As dxfVector In _rVal
                MyBase.Remove(item)
            Next
            RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, cnt, cnt - _rVal.Count, canc, False, True)
            Return _rVal
        End Function

        ''' <summary>
        ''' used to create subsets of the current vector collection by applying filters against some of the ordinates
        ''' </summary>
        ''' <remarks>the only acceptable filter arguments are AtMaxX,AtMinX,AtMaxY, or AtMinY. other filters should be used with GetVectors. </remarks>
        ''' <param name="Filter">the filter to apply to the current collection</param>
        ''' <param name="aCS">an optional coordinate system to use for relative ordinates</param>
        ''' <param name="aPrecis">a precision for the comparison (1 to 15)</param>
        ''' <returns></returns>
        Public Function FilterVectors(Filter As dxxPointFilters, Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 3) As colDXFVectors
            Dim _rVal As New colDXFVectors
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            '**UNUSED VAR** Dim comp As Single
            Dim Xs As List(Of Double) = Nothing
            Dim Ys As List(Of Double) = Nothing
            Dim Zs As List(Of Double) = Nothing
            Dim Pts As colDXFVectors
            Dim RetVector As dxfVector
            Dim d1 As Double
            'return only the vectors on the maximums
            If Filter = dxxPointFilters.AtMaxX Or Filter = dxxPointFilters.AtMaxY Then
                GetOrdinates(Xs, Ys, Zs, aCS)
                Select Case Filter
                    Case dxxPointFilters.AtMaxX
                        For Each d1 In Ys
                            Pts = GetAtCoordinate(aY:=d1, aPlane:=aCS, aPrecis:=aPrecis)
                            RetVector = Pts.GetVector(dxxPointFilters.AtMaxX, aOrdinate:=0, aPrecis:=aPrecis)
                            If RetVector IsNot Nothing Then _rVal.Add(RetVector)
                        Next
                    Case dxxPointFilters.AtMaxY
                        For Each d1 In Xs
                            Pts = GetAtCoordinate(aX:=d1, aPlane:=aCS, aPrecis:=aPrecis)
                            RetVector = Pts.GetVector(dxxPointFilters.AtMaxX, aOrdinate:=0, aPrecis:=aPrecis)
                            If RetVector IsNot Nothing Then _rVal.Add(RetVector)
                        Next
                End Select
            ElseIf Filter = dxxPointFilters.AtMinX Or Filter = dxxPointFilters.AtMinY Then
                'return only the vectors on the minimums
                GetOrdinates(Xs, Ys, Zs, aCS)
                Select Case Filter
                    Case dxxPointFilters.AtMinX
                        For Each d1 In Ys
                            Pts = GetAtCoordinate(aY:=d1, aPlane:=aCS, aPrecis:=aPrecis)
                            RetVector = Pts.GetVector(dxxPointFilters.AtMinX, aPrecis:=aPrecis)
                            If RetVector IsNot Nothing Then _rVal.Add(RetVector)
                        Next
                    Case dxxPointFilters.AtMinY
                        For Each d1 In Xs
                            Pts = GetAtCoordinate(aX:=d1, aPlane:=aCS, aPrecis:=aPrecis)
                            RetVector = Pts.GetVector(dxxPointFilters.AtMinY, aPrecis:=aPrecis)
                            If RetVector IsNot Nothing Then _rVal.Add(RetVector)
                        Next
                End Select
            End If
            Return _rVal
        End Function
        Public Function First(aCount As Integer, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1the number of vectors to return
            '#2flag to return copies
            '#3flag to remove the subset from the collection
            '^returns the first members of the collection up to the passed count
            '~i.e. First(4) returns the first 4 members
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aCount <= 0 Then Return _rVal
            If aCount > Count Then aCount = Count
            Dim i As Integer
            Dim v1 As dxfVector
            For i = 1 To aCount
                v1 = Item(i)
                _rVal.Add(v1, bAddClone:=bReturnClones)
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function FirstVector(Optional bReturnClone As Boolean = False) As dxfVector
            '^returns the first vector in the collection (if there are any)
            Return Item(1, bReturnClone)
        End Function
        Public Function FlagList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty
            Dim i As Integer
            Dim aMem As dxfVector
            For i = 1 To Count
                aMem = Item(i)
                TLISTS.Add(_rVal, aMem.Flag, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function Flags(Optional bUniqueOnly As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            '#1flag to return only the unique flags
            '^returns a collection of strings containing the flags of the members
            Dim aMem As dxfVector
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
        Friend Function GetPlane(Optional bReturnSomething As Boolean = False, Optional aPlane As dxfPlane = Nothing) As dxfPlane
            If Not dxfPlane.IsNull(aPlane) Then Return aPlane
            Dim _rVal As dxfPlane = Nothing
            RaiseEvent OCSRequest(_rVal)
            If _rVal Is Nothing Then
                If Not String.IsNullOrWhiteSpace(_OwnerGUID) Then
                    Dim owner As dxfHandleOwner = MyOwner
                    If owner IsNot Nothing Then
                        If owner.FileObjectType = dxxFileObjectTypes.Entity Then
                            Dim entity As dxfEntity = TryCast(owner, dxfEntity)
                            If entity IsNot Nothing Then _rVal = entity.Plane
                        End If
                    End If
                End If
            End If
            If _rVal Is Nothing And bReturnSomething Then _rVal = New dxfPlane
            Return _rVal
        End Function
        Friend Sub SetGUIDS(aImageGUID As String, aEntityGUID As String, aBlockGUID As String, aSupressEvnts As Boolean)
            ImageGUID = aImageGUID : EntityGUID = aEntityGUID : BlockGUID = aBlockGUID
            SuppressEvents = aSupressEvnts
            MonitorMembers = Not String.IsNullOrWhiteSpace(aEntityGUID)
            If MonitorMembers Then MaintainIndices = True
        End Sub
        Public Function GetAtCoordinate(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bJustOne As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the X coordinate to match
            '#2the Y coordinate to match
            '#3the Z coordinate to match
            '#4an optional coordinate system to use
            '#5a precision for the comparison (1 to 15)
            '^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
            '~if an any of the ordinates (X, Y or Z) are null or undefined they are not used in the comparison.
            '~say ony an X value is passed, then all the vectors with the same X ordinate are returned regardless of their
            '~respective Y and Z ordinate values.
            _rVal = GetAtPlanarCoordinate(New TPLANE(aPlane), aX, aY, aZ, aPrecis, True, bReturnClones, bRemove, True, bJustOne)
            If bRemove And Not _SuppressEvents Then
                RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetAtEqualOrdinate(Optional aOrdinate As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X, Optional aPrecis As Integer = 4, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aCS As dxfPlane = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If Not bRemove And Not bReturnClones Then _rVal.MaintainIndices = False
            If aOrdinate < dxxOrdinateDescriptors.X Or aOrdinate > dxxOrdinateDescriptors.Z Then Return _rVal
            aPrecis = Math.Abs(aPrecis)
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Dim P1 As dxfVector
            Dim P2 As dxfVector
            Dim i As Integer
            Dim Ord1 As Double
            Dim Ord2 As Double
            Dim p3 As dxfVector
            Dim j As Integer
            Dim k As Integer
            Dim aPl As New TPLANE("")
            Dim v1 As TVECTOR
            Dim bHaveIt As Boolean
            For i = 1 To Count
                P1 = Item(i)
                bHaveIt = False
                For j = 1 To _rVal.Count
                    p3 = _rVal.Item(j)
                    If P1 Is p3 Then
                        bHaveIt = True
                        Exit For
                    End If
                Next j
                If Not bHaveIt Then
                    If aOrdinate = dxxOrdinateDescriptors.X Then
                        Ord1 = Math.Round(P1.X, aPrecis)
                    ElseIf aOrdinate = dxxOrdinateDescriptors.Y Then
                        Ord1 = Math.Round(P1.Y, aPrecis)
                    Else
                        Ord1 = Math.Round(P1.Z, aPrecis)
                    End If
                    For j = i + 1 To Count
                        P2 = Item(j)
                        bHaveIt = False
                        For k = 1 To _rVal.Count
                            p3 = _rVal.Item(k)
                            If P1 Is p3 Then
                                bHaveIt = True
                                Exit For
                            End If
                        Next k
                        If Not bHaveIt Then
                            v1 = P2.Strukture
                            If aCS IsNot Nothing Then v1 = v1.WithRespectTo(aPl)
                            If aOrdinate = dxxOrdinateDescriptors.X Then
                                Ord2 = Math.Round(v1.X, aPrecis)
                            ElseIf aOrdinate = dxxOrdinateDescriptors.Y Then
                                Ord2 = Math.Round(v1.Y, aPrecis)
                            Else
                                Ord2 = Math.Round(v1.Z, aPrecis)
                            End If
                            If Ord1 = Ord2 Then _rVal.Add(P2, bAddClone:=bReturnClones)
                        End If
                    Next j
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function GetAtPlanarCoordinate(aPlane As TPLANE, Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aPrecis As Integer = 3, Optional bSuppressPlane As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bSuppressEvnts As Boolean = False, Optional bJustOne As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If _SuppressEvents Then bSuppressEvnts = True
            '#1the coordinate system to use
            '#2the X coordinate to match
            '#3the Y coordinate to match
            '#4the Z coordinate to match
            '#5a precision for the comparison (1 to 15)
            '^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
            '~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
            '~say ony an X value is passed, then all the vectors with the same X ordinate are returned regarless of their
            '~respective Y and Z ordinate values.
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            Dim aMem As dxfVector
            Dim v1 As TVECTOR
            Dim isMatchX As Boolean
            Dim isMatchY As Boolean
            Dim isMatchZ As Boolean

            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If Not aX.HasValue And Not aY.HasValue And Not aZ.HasValue Then Return _rVal
            For i As Integer = 1 To Count
                aMem = Item(i)
                v1 = aMem.Strukture
                If Not bSuppressPlane Then v1 = v1.WithRespectTo(aPlane)
                isMatchX = True
                isMatchY = True
                isMatchZ = True
                If aX.HasValue Then
                    isMatchX = Math.Round(v1.X - aX.Value, aPrecis) = 0
                End If
                If aY.HasValue Then
                    isMatchY = Math.Round(v1.Y - aY.Value, aPrecis) = 0

                End If
                If aZ.HasValue Then
                    isMatchZ = Math.Round(v1.Z - aZ.Value, aPrecis) = 0

                End If
                If isMatchX And isMatchY And isMatchZ Then
                    _rVal.Add(aMem, bAddClone:=bReturnClones)
                    If bJustOne Then Exit For
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not bSuppressEvnts Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function GetAtPlanarOrdinate(aPlane As TPLANE, aOrdinate As Double, aOrdinateType As dxxOrdinateDescriptors, Optional aPrecis As Integer = 3, Optional bSuppressPlane As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bSuppressEvnts As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1the coordinate system to use
            '#2the coordinate to match
            '#3the coordinate type to match
            '#4a precision for the comparison (0 to 15)
            '^searchs for and returns vectors from the collection whose specified ordinate  match the passed ordinate
            aPrecis = dxfUtils.LimitedValue(aPrecis, 0, 15)
            Dim aMem As dxfVector
            Dim v1 As TVECTOR
            Dim aDif As Double
            Dim vOrd As Double
            For i = 1 To Count
                aMem = Item(i, bReturnClones)
                v1 = aMem.Strukture
                If Not bSuppressPlane Then v1 = v1.WithRespectTo(aPlane, aPrecis:=15)
                Select Case aOrdinateType
                    Case dxxOrdinateDescriptors.Y
                        vOrd = Math.Round(v1.Y, aPrecis)
                    Case dxxOrdinateDescriptors.Z
                        vOrd = Math.Round(v1.Z, aPrecis)
                    Case Else
                        vOrd = Math.Round(v1.X, aPrecis)
                End Select
                aDif = Math.Round(vOrd - aOrdinate, aPrecis)
                If (aDif = 0) Then _rVal.Add(aMem)
            Next i
            If bRemove And _rVal.Count > 0 Then
                If RemoveMembers(_rVal, True) > 0 Then
                    If Not _SuppressEvents And Not bSuppressEvnts Then
                        RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Function GetBetweenOrdinates(aOrdinateType As dxxOrdinateDescriptors, aOrdinate1 As Double, aOrdinate2 As Double, Optional bOnisIn As Boolean = True, Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional aIndices As List(Of Integer) = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnPlanarVectors As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1the ordinate to use in the search
            '#2the first ordinate to use in the search
            '#3the second ordinate to use in the search
            '#4flag indicating if equal values should be returned
            '#5an optional coordinate system to use
            '#6returns the indices of the matches
            '#7flag to return copies
            '#8flag to remove the matching set
            '#9a precision for numerical comparison (1 to 8)
            '^returns the members from the collection whose coordinates match the passed search criteria
            If Not bReturnClones And Not bRemove And Not bReturnPlanarVectors Then _rVal.MaintainIndices = False
            Dim i As Integer
            Dim P1 As dxfVector
            Dim v1 As TVECTOR
            Dim aPl As New TPLANE(aCS)
            Dim o1 As Double = Math.Round(aOrdinate1, aPrecis)
            Dim o2 As Double = Math.Round(aOrdinate2, aPrecis)
            Dim comp As Double
            aPrecis = dxfUtils.LimitedValue(aPrecis, 1, 15)
            If aIndices Is Nothing Then aIndices = New List(Of Integer)
            TVALUES.SortTwoValues(True, o1, o2)

            For i = 1 To Count
                P1 = Item(i)
                v1 = P1.Strukture
                If aCS IsNot Nothing Then v1 = v1.WithRespectTo(aPl)
                Select Case aOrdinateType
                    Case dxxOrdinateDescriptors.Z
                        comp = Math.Round(v1.Z, aPrecis)
                    Case dxxOrdinateDescriptors.Y
                        comp = Math.Round(v1.Y, aPrecis)
                    Case Else
                        comp = Math.Round(v1.X, aPrecis)
                End Select
                If bOnisIn Then
                    If comp >= o1 And comp <= o2 Then
                        If Not bReturnPlanarVectors Then _rVal.Add(P1, bAddClone:=bReturnClones) Else _rVal.AddV(v1)
                        aIndices.Add(i)
                    End If
                Else
                    If comp > o1 And comp < o2 Then
                        If Not bReturnPlanarVectors Then _rVal.Add(P1, bAddClone:=bReturnClones) Else _rVal.AddV(v1)
                        aIndices.Add(i)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByColumn(aCol As Integer, Optional aRow As Integer? = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1the column to search for
            '#2an optional row that all returns must match
            '#3returns then highest column number in the collection
            '#4returns then highest row number in the collection
            '#5flag to return clones
            '#6flag to remove the return set from this collection
            Dim rMaxRow As Integer = 0
            Dim rMaxCol As Integer = 0


            Dim bTest As Boolean
            Dim arw As Integer
            rMaxRow = 0
            rMaxCol = 0
            If aRow IsNot Nothing Then
                If aRow.HasValue Then
                    bTest = True
                    arw = aRow.Value
                End If
            End If
            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                If v1.Row > rMaxRow Then rMaxRow = v1.Row
                If v1.Col > rMaxCol Then rMaxCol = v1.Col
                If v1.Col = aCol Then
                    If Not bTest Then
                        If Not bReturnClones Then _rVal.Add(v1) Else _rVal.Add(v1.Clone)
                    Else
                        If v1.Row = arw Then
                            If Not bReturnClones Then _rVal.Add(v1) Else _rVal.Add(v1.Clone)
                        End If
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetBySuppressed(aSuppressedVal As Boolean, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional TheOthers As colDXFVectors = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False


            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                If v1.Suppressed = aSuppressedVal Then
                    If bReturnClones Then _rVal.Add(v1.Clone) Else _rVal.Add(v1)
                Else
                    If TheOthers IsNot Nothing Then

                        If bReturnClones Then TheOthers.Add(v1.Clone) Else TheOthers.Add(v1)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function


        Public Function GetByDisplayVariableValue(aPropertyType As dxxDisplayProperties, aValue As Object, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bCaseSensitive As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1the variable to seach on
            '#2the value to search for
            '#3flag to return clones
            '#4flag to remove the results
            '^searchs for members with matching values to the passed variable name
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            Dim aVariableName As String = dxfEnums.Description(aPropertyType).ToUpper()
            If aVariableName = "" Then Return _rVal
            Dim bStrComp As Boolean = aPropertyType = dxxDisplayProperties.Linetype Or aPropertyType = dxxDisplayProperties.LayerName
            Dim v1 As dxfVector
            Dim bVal As Object
            Dim bKeep As Boolean
            For i As Integer = 1 To Count
                v1 = Item(i)
                bVal = v1.GetDisplayProperty(aPropertyType)
                If bVal IsNot Nothing Then
                    If bStrComp Then
                        bKeep = String.Compare(aValue, bVal, ignoreCase:=Not bCaseSensitive) = 0
                    Else
                        bKeep = aValue = bVal
                    End If
                    If Not bReturnInverse Then
                        If bKeep Then _rVal.Add(v1, bAddClone:=bReturnClones)
                    Else
                        If Not bKeep Then _rVal.Add(v1, bAddClone:=bReturnClones)
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, $"{Reflection.MethodBase.GetCurrentMethod.Name}.{aVariableName}", Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByFlag(aFlag As String, Optional aTag As String = Nothing, Optional bContainsString As Boolean = False, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the flag to search for
            '#2an optional tag to include in the search criteria
            '#3flag to include any member whose flag string contains the passed search strings instead of a full string match
            '#4flag to stop searching when the first match is found
            '#5flag to return clones of the matches
            '#6flag to remove the matches from the collection
            '^returns all the vectors that match the search criteria
            _rVal = New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False

            Dim bTest As Boolean = aTag IsNot Nothing

            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                Dim bKeep As Boolean = False
                If Not bContainsString Then
                    If String.Compare(v1.Flag, aFlag, ignoreCase:=True) = 0 Then bKeep = True
                Else
                    If v1.Flag.Contains(aFlag, StringComparer.OrdinalIgnoreCase) Then bKeep = True
                End If
                If bKeep And bTest Then
                    If Not bContainsString Then
                        If String.Compare(v1.Tag, aTag, ignoreCase:=True) <> 0 Then bKeep = False
                    Else
                        If Not v1.Tag.Contains(aTag, StringComparer.OrdinalIgnoreCase) Then bKeep = False
                    End If
                End If
                If bKeep Then
                    _rVal.Add(v1, bAddClone:=bReturnClones)
                    If bReturnJustOne Then Exit For
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByHandle(aHandle As String, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors = New colDXFVectors
            '#1the tag to search for
            '#2flag to stop searching when the first match is found
            '#3flag to return clones of the matches
            '#4flag to remove the match from the collection
            '^returns all the vectors that match the search criteria

            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False

            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                Dim bKeep As Boolean = False
                If String.Compare(v1.Handle, aHandle, True) = 0 Then bKeep = True
                If bKeep Then
                    _rVal.Add(v1, bAddClone:=bReturnClones)
                    If bReturnJustOne Then Exit For
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByIndexList(aList As String, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1a comma delimited list of indexes
            '#2an optional tag to include in the search criteria
            '#3flag to include any member whose flag string contains the passed search strings instead of a full string match
            '#4flag to stop searching when the first match is found
            '#5flag to return clones of the matches
            '#6flag to remove the matches from the collection
            '^returns all the vectors that match the search criteria
            _rVal = New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            aList = Trim(aList)
            If Count <= 0 Then Return _rVal
            If aList = "" Then aList = ""
            Dim v1 As dxfVector
            Dim i As Integer
            Dim aStr As String = String.Empty
            Dim sVals() As String
            Dim j As Integer
            Dim idx As Integer
            Dim ids As Collection
            Dim bKeep As Boolean
            sVals = aList.Split(",")
            ids = New Collection
            For j = 0 To sVals.Length - 1
                aStr = sVals(j).Trim()
                If aStr <> "" Then
                    If TVALUES.IsNumber(aStr) Then
                        idx = TVALUES.To_INT(aStr)
                        If idx > 0 And idx <= Count Then
                            bKeep = True
                            For i = 1 To ids.Count
                                If idx = ids.Item(i) Then
                                    bKeep = False
                                    Exit For
                                End If
                            Next i
                            If bKeep Then
                                v1 = Item(idx)
                                _rVal.Add(v1, bAddClone:=bReturnClones)
                                ids.Add(idx)
                            End If
                        End If
                    End If
                End If
            Next j
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByLayer(aValue As String, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the layer name to search for
            '#2flag to return clones
            '#3flag to remove the results
            '#4flag to return the inverse of the request
            '^searchs for vectors with matching values
            _rVal = GetByDisplayVariableValue(dxxDisplayProperties.LayerName, aValue, bReturnClones, bRemove, bReturnInverse:=bReturnInverse)
            Return _rVal
        End Function
        Public Function GetByLineType(aValue As String, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the linetype to search for
            '#2flag to return clones
            '#3flag to remove the results
            '#4flag to return the inverse of the request
            '^searchs for vectors with matching values
            _rVal = GetByDisplayVariableValue(dxxDisplayProperties.Linetype, aValue, bReturnClones, bRemove, bReturnInverse:=bReturnInverse)
            Return _rVal
        End Function
        Public Function GetByPlane(aPlane As dxfPlane, Optional bReturnClones As Boolean = False, Optional aFudgeFactor As Double = 0.001, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            _rVal = New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If TPLANE.IsNull(aPlane) Then Return _rVal
            Dim i As Integer
            Dim v1 As dxfVector
            For i = 1 To Count
                v1 = Item(i)
                If v1.LiesOnPlane(aPlane, aFudgeFactor) Then _rVal.Add(v1, bAddClone:=bReturnClones)
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByPropertyValue(aProperty As dxxVectorProperties, aPropertyValue As Object, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            _rVal = New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aProperty < 1 Or aProperty > dxxVectorProperties.Suppressed Then Return _rVal
            Dim i As Integer
            Dim aMem As dxfVector
            Dim aVal As Object
            Dim bVal As Object
            Dim aFlg As Boolean
            bVal = aPropertyValue
            Select Case aProperty
                Case dxxVectorProperties.EndWidth, dxxVectorProperties.StartWidth, dxxVectorProperties.Radius, dxxVectorProperties.Value, dxxVectorProperties.Rotation, dxxVectorProperties.LTScale
                    If TVALUES.IsNumber(aPropertyValue) Then
                        If aPrecis >= 0 Then bVal = Math.Round(TVALUES.To_DBL(aPropertyValue), TVALUES.LimitedValue(aPrecis, 0, 15))
                    End If
                Case Else
            End Select
            Dim pname As String = String.Empty
            For i = 1 To Count
                aMem = Item(i)
                aVal = dxfVector.PropertyValue(aMem, aProperty, aPrecis, aFlg, pname)
                If aFlg Then Exit For
                If aVal = bVal Then _rVal.Add(aMem, bAddClone:=bReturnClones)
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByProximity(aVector As dxfVector, aDistance As Double, Optional aPrecis As Integer = 4, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the vector to match
            '#2the precision for the comparison
            '#3flag to return copies
            '#4flag to remove the return set from this set
            '^searchs for and returns vectors from the collection whose coordinates match the coordinates of the passed vector
            _rVal = New colDXFVectors
            If aVector Is Nothing Then Return _rVal
            _rVal = GetByProximityV(aVector.Strukture, aDistance, aPrecis, bReturnClones, False, True)
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function GetByProximityV(aVector As TVECTOR, aDistance As Double, Optional aPrecis As Integer = 4, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bSuppressEvnts As Boolean = True) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the vector to match
            '#2the distance to consider
            '#3the precision for the comparison
            '#4flag to return copies
            '#5flag to remove the return set from this set
            '^searchs for and returns vectors from the collection whose distance from the passed vector is less than oe equal to the passed distance
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim P1 As dxfVector
            Dim dst As Double
            _rVal = New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            For i = 1 To Count
                P1 = Item(i)
                v1 = P1.Strukture
                dst = v1.DistanceTo(aVector, aPrecis)
                If dst <= aDistance Then
                    _rVal.Add(P1, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents And Not bSuppressEvnts Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByQuadrant(aQuadrant As Integer, aCenterVector As iVector, Optional bOnIsIn As Boolean = True, Optional aPlane As dxfPlane = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            If aQuadrant < 1 Or aQuadrant > 4 Then aQuadrant = 1
            '#1the quadrant to return (1-4)
            '#2the center used to divide the vectors by quadrant
            '#3flag to indicate if vectors lying on the boundaries should be returned or not
            '#4the system to use
            '#5flag to return copies
            '#6flag to remove the return
            '^returns the vectors in the collection that lie in the indicated quadrant with respect to the passed vector and system
            Dim plane As New TPLANE(aPlane)
            Dim v1 As TVECTOR
            Dim aPt As dxfVector
            Dim difx As Double
            Dim dify As Double
            Dim i As Integer

            If aCenterVector IsNot Nothing Then plane = plane.MovedTo(New TVECTOR(aCenterVector))
            For i = 1 To Count
                aPt = Item(i)
                v1 = aPt.Strukture.WithRespectTo(aPlane)
                difx = Math.Round(v1.X, 3)
                dify = Math.Round(v1.Y, 3)
                Select Case aQuadrant
                    Case 1
                        If (bOnIsIn And difx = 0) Or difx > 0 Then
                            If (bOnIsIn And dify = 0) Or dify > 0 Then
                                _rVal.Add(aPt, bAddClone:=bReturnClones)
                            End If
                        End If
                    Case 2
                        If (bOnIsIn And difx = 0) Or difx < 0 Then
                            If (bOnIsIn And dify = 0) Or dify > 0 Then
                                _rVal.Add(aPt, bAddClone:=bReturnClones)
                            End If
                        End If
                    Case 3
                        If (bOnIsIn And difx = 0) Or difx < 0 Then
                            If (bOnIsIn And dify = 0) Or dify < 0 Then
                                _rVal.Add(aPt, bAddClone:=bReturnClones)
                            End If
                        End If
                    Case 4
                        If (bOnIsIn And difx = 0) Or difx > 0 Then
                            If (bOnIsIn And dify = 0) Or dify < 0 Then
                                _rVal.Add(aPt, bAddClone:=bReturnClones)
                            End If
                        End If
                End Select
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByRow(aRow As Integer, Optional aCol As Integer? = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional aValue As Double? = Nothing) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1the row to search for
            '#2an optional column that all returns must match
            '#3flag to return clones
            '#4flag to remove the return set from this collection
            Dim rMaxRow As Integer = 0
            Dim rMaxCol As Integer = 0


            Dim bTest As Boolean
            Dim aCl As Integer
            Dim bTestVal As Boolean
            Dim bKeep As Boolean
            bTestVal = aValue IsNot Nothing
            If bTestVal Then
                If Not aValue.HasValue Then bTestVal = False
            End If
            rMaxRow = 0
            rMaxCol = 0
            If aCol IsNot Nothing Then
                bTest = aCol.HasValue
                If bTest Then aCl = aCol.Value
            End If
            For i As Integer = 1 To Count
                Dim v1 As dxfVector = MyBase.Item(i - 1)
                If v1.Row > rMaxRow Then rMaxRow = v1.Row
                If v1.Col > rMaxCol Then rMaxCol = v1.Col
                bKeep = (v1.Row = aRow)
                If bKeep Then
                    If bTest Then
                        If v1.Col <> aCl Then bKeep = False
                    End If
                    If bTestVal Then
                        If v1.Value <> aValue.Value Then bKeep = False
                    End If
                End If
                If bKeep Then _rVal.Add(SetMemberInfo(v1, bReturnClones, i).Item1)
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByTag(aTag As String, Optional aFlag As String = Nothing, Optional bContainsString As Boolean = False, Optional bReturnJustOne As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aValue As Double? = Nothing, Optional aDelimitor As String = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aTag Is Nothing Then Return _rVal
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4flag to stop searching when the first match is found
            '#5flag to return clones of the matches
            '#6flag to to remove the match from the collection
            '#7flag to return the members that don't match the search criteria
            '#8an optional value to include in the search criteria
            '^returns all the entities that match the search criteria
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False

            Dim bTest As Boolean = aFlag IsNot Nothing

            Dim bTestVal As Boolean = aValue IsNot Nothing
            If bTestVal Then If Not aValue.HasValue Then bTestVal = False
            Dim srch As List(Of dxfVector) = Me

            Dim ret As New List(Of dxfVector)
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
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=True) = 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=True) = 0 And String.Compare(mem.Flag, aFlag, ignoreCase:=True) = 0))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=True) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) String.Compare(mem.Tag, tag, ignoreCase:=True) < 0 And String.Compare(mem.Flag, aFlag, ignoreCase:=True) < 0))
                        End If
                    End If
                Else
                    If Not bReturnInverse Then
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag.IndexOf(tag, comparisonType:=StringComparison.OrdinalIgnoreCase) > -1))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag.IndexOf(tag, comparisonType:=StringComparison.OrdinalIgnoreCase) > -1 And mem.Flag.IndexOf(aFlag, comparisonType:=StringComparison.OrdinalIgnoreCase) > -1))
                        End If
                    Else
                        If Not bTest Then
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag.IndexOf(tag, comparisonType:=StringComparison.OrdinalIgnoreCase) < 0))
                        Else
                            ret.AddRange(srch.FindAll(Function(mem) mem.Tag.IndexOf(tag, comparisonType:=StringComparison.OrdinalIgnoreCase) < 0 And mem.Flag.IndexOf(aFlag, comparisonType:=StringComparison.OrdinalIgnoreCase) < 0))
                        End If
                    End If
                End If
                If bReturnJustOne And ret.Count > 0 Then Exit For
            Next
            If bTestVal Then
                ret = ret.FindAll(Function(mem) mem.Value = aValue.Value)
            End If
            If ret.Count > 0 Then
                If bReturnJustOne Then
                    _rVal.Add(Item(IndexOf(ret.Item(0))), bAddClone:=bReturnClones)
                Else
                    _rVal.Populate(ret, bAddClones:=bReturnClones)
                End If
            Else
                Return _rVal
            End If
            If bRemove Then
                If RemoveMembers(ret, True) > 0 And Not SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, ret, Reflection.MethodBase.GetCurrentMethod.Name, Count + ret.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByTags(aTagList As String, Optional aFlagList As String = "", Optional aDelimiter As String = ",", Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aTagList Is Nothing Then Return _rVal
            '#1the list of tags to search for
            '#2an optional list of flags to include in the search criteria
            '#3the delimiting character for the passed lists
            '#4flag to return clones of the matches
            '#5flag to remove the match from the collection
            '#6a flag to retun the members that DON'T match the search criteria
            '^returns all the vectors that match the search criteria


            Dim bKeep As Boolean
            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                If aMem.Tag <> "" Then
                    bKeep = TLISTS.Contains(aMem.Tag, aTagList, aDelimiter)
                Else
                    bKeep = False
                End If
                If bKeep Then bKeep = TLISTS.Contains(aMem.Flag, aFlagList, aDelimiter, bReturnTrueForNullList:=True)
                If bReturnInverse Then bKeep = Not bKeep
                If bKeep Then
                    _rVal.Add(aMem, bAddClone:=bReturnClones)
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByFlagList(aFlags As String, Optional aDelimiter As String = ",", Optional bContainsString As Boolean = False, Optional aTag As String = Nothing, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnInverse As Boolean = False, Optional aValue As Double? = Nothing) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the list of flags to search for
            '#2the delimiter that seperates the values in the list
            '#3flag to include any member whose flag string contains the passed search strings instead of a full string match
            '#4a tag value to apply to the search
            '#5a entity type to match
            '#6flag to return clones of the matches
            '#7flag to to remove the matches from the collection
            '#8flag to return the members that don't match the search criteria
            '#9an optional value to include in the search criteria
            '^returns all the entities that match the search criteria
            _rVal = New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False



            Dim bTest As Boolean = aTag IsNot Nothing
            Dim bKeep As Boolean
            Dim bTestVal As Boolean = aValue IsNot Nothing
            Dim tVals As TVALUES = TLISTS.ToValues(aFlags, aDelimiter)
            Dim aFlag As String

            If bTestVal Then
                bTestVal = aValue.HasValue
            End If

            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                For j As Integer = 1 To tVals.Count
                    aFlag = tVals.Member(j - 1)
                    bKeep = False
                    If Not bContainsString Then
                        If String.Compare(aMem.Flag, aFlag, True) = 0 Then bKeep = True
                    Else
                        If aMem.Flag.IndexOf(aFlag, StringComparison.OrdinalIgnoreCase) + 1 > 0 Then bKeep = True
                    End If
                    If bKeep And bTest Then
                        If Not bContainsString Then
                            If String.Compare(aMem.Tag, aTag, True) <> 0 Then bKeep = False
                        Else
                            If aMem.Tag.IndexOf(aTag, StringComparison.OrdinalIgnoreCase) < 0 Then bKeep = False
                        End If
                    End If
                    If bKeep And bTestVal Then
                        If aMem.Value <> aValue.Value Then bKeep = False
                    End If
                    If Not bReturnInverse Then
                        If bKeep Then
                            _rVal.Add(aMem, bAddClone:=bReturnClones)
                        End If
                    Else
                        If Not bKeep Then
                            _rVal.Add(aMem, bAddClone:=bReturnClones)
                        End If
                    End If
                Next j
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function GetByUniqueOrdinate(OrdToGet As dxxOrdinateDescriptors, Optional aPrecis As Integer = 3, Optional aTag As String = "", Optional aFlag As String = "") As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the ordinate to search on (X, Y or Z)
            '#2the precision for the comparison
            '#3a collection of points to search other that this one
            '^returns the first point found at each unique ordinate as requested (X, Y or Z)
            _rVal = New colDXFVectors
            If OrdToGet <> dxxOrdinateDescriptors.X And OrdToGet <> dxxOrdinateDescriptors.Y And OrdToGet <> dxxOrdinateDescriptors.Z Then OrdToGet = dxxOrdinateDescriptors.X
            If aPrecis < 0 Then aPrecis = 0
            If aPrecis > 5 Then aPrecis = 5
            '**UNUSED VAR** Dim stp As INteger
            Dim oVals As Collection
            Dim aPt As dxfVector
            Dim i As Integer
            Dim ord As Double
            Dim bHaveIt As Boolean
            Dim j As Integer
            Dim bpt As dxfVector
            oVals = New Collection
            For i = 1 To Count
                aPt = Item(i)
                If OrdToGet = dxxOrdinateDescriptors.X Then
                    ord = Math.Round(aPt.X, aPrecis)
                ElseIf OrdToGet = dxxOrdinateDescriptors.Y Then
                    ord = Math.Round(aPt.Y, aPrecis)
                ElseIf OrdToGet = dxxOrdinateDescriptors.Z Then
                    ord = Math.Round(aPt.Z, aPrecis)
                End If
                bHaveIt = False
                For j = 1 To oVals.Count
                    If ord = oVals.Item(j) Then
                        bHaveIt = True
                        Exit For
                    End If
                Next j
                If Not bHaveIt Then
                    oVals.Add(ord)
                    bpt = New dxfVector(aPt)
                    If aTag <> "" Then bpt.Tag = aTag
                    If aFlag <> "" Then bpt.Flag = aFlag
                    _rVal.Add(bpt)
                End If
            Next i
            Return _rVal
        End Function

        Public Function GetByVector(aVector As iVector, Optional aPrecis As Integer = 4, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the vector to match
            '#2the precision for the comparison
            '#3flag to return copies
            '#4flag to remove the return set from this set
            '^searchs for and returns vectors from the collection whose coordinates match the coordinates of the passed vector
            _rVal = New colDXFVectors
            If aVector Is Nothing Then Return _rVal
            _rVal = GetByVector(New TVECTOR(aVector), aPrecis, bReturnClones, False, True)
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Friend Function GetByVector(aVector As TVECTOR, aPrecis As Integer, bReturnClones As Boolean, bRemove As Boolean, bSuppressEvnts As Boolean) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the vector to match
            '#2the precision for the comparison
            '#3flag to return copies
            '#4flag to remove the return set from this set
            '^searchs for and returns vectors from the collection whose coordinates match the coordinates of the passed vector
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)


            Dim v1 As TVECTOR

            _rVal = New colDXFVectors
            If Not bReturnClones And Not bRemove Then _rVal.MaintainIndices = False
            For i As Integer = 1 To Count
                Dim P1 As dxfVector = Item(i)
                v1 = New TVECTOR(P1)
                If Math.Round(v1.X - aVector.X, aPrecis) = 0 Then
                    If Math.Round(v1.Y - aVector.Y, aPrecis) = 0 Then
                        If Math.Round(v1.Z - aVector.Z, aPrecis) = 0 Then
                            _rVal.Add(P1, bAddClone:=bReturnClones)
                        End If
                    End If
                End If
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, True) > 0 And Not _SuppressEvents And Not bSuppressEvnts Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Sub GetDimensions(ByRef rXSpan As Double, ByRef rYSpan As Double, ByRef rZSpan As Double, Optional aPlane As dxfPlane = Nothing, Optional bReturnZ As Boolean = False)
            '#1returns the span of the X ordinates of all the vectors in the collection
            '#2returns the span of the Y ordinates of all the vectors in the collection
            '#3returns the span of the Z ordinates of all the vectors in the collection
            '#4flag to compute the Z values
            '^returns then dimensions of the collection of vectors
            Dim rMinX As Double = 0.0
            Dim rMaxX As Double = 0.0
            Dim rMinY As Double = 0.0
            Dim rMaxY As Double = 0.0
            Dim rMinZ As Double = 0.0
            Dim rMaxZ As Double = 0.0
            GetExtremeOrdinates(rMaxX, rMinX, rMaxY, rMinY, rMaxZ, rMinZ, aPlane)
            rXSpan = Math.Abs(rMaxX - rMinX)
            rYSpan = Math.Abs(rMaxY - rMinY)
            rZSpan = Math.Abs(rMaxZ - rMinZ)
        End Sub
        Public Sub GetExtremeOrdinates(ByRef rMaxX As Double, ByRef rMinX As Double, ByRef rMaxY As Double, ByRef rMinY As Double, rMaxZ As Double, ByRef rMinZ As Double, Optional aPlane As dxfPlane = Nothing)
            '#1returns the maximum X ordinate
            '#2returns the minimum X ordinate
            '#3returns the maximum Y ordinate
            '#4returns the minimum Y ordinate
            '#5returns the maximum Z ordinate
            '#6returns the minimum Z ordinate
            '#7an optional plane to define the memebr ordinates against
            '^returns the extrems of the ordinates of the members of the collection
            If Count = 0 Then
                rMaxX = 0
                rMinX = 0
                rMaxY = 0
                rMinY = 0
                rMaxZ = 0
                rMinZ = 0
            Else
                rMaxX = Double.MinValue
                rMinX = Double.MaxValue
                rMaxY = Double.MinValue
                rMinY = Double.MaxValue
                rMaxZ = Double.MinValue
                rMinZ = Double.MaxValue
                Dim bPln As Boolean = Not TPLANE.IsNull(aPlane)
                Dim aPl As TPLANE = IIf(Not bPln, TPLANE.World, New TPLANE(aPlane))


                For Each item As dxfVector In Me
                    Dim v1 As TVECTOR = item.Strukture
                    If bPln Then v1 = v1.WithRespectTo(aPl)
                    If v1.X < rMinX Then rMinX = v1.X
                    If v1.X > rMaxX Then rMaxX = v1.X
                    If v1.Y < rMinY Then rMinY = v1.Y
                    If v1.Y > rMaxY Then rMaxY = v1.Y
                    If v1.Z < rMinZ Then rMinZ = v1.Z
                    If v1.Z > rMaxZ Then rMaxZ = v1.Z
                Next
            End If
        End Sub
        Public Function GetFlagged(aFlag As String, Optional aTag As String = Nothing, Optional bContainsString As Boolean = False, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False, Optional aOccur As Integer = 0, Optional bIgnoreCase As Boolean = True) As dxfVector
            Dim member As dxfVector = Nothing
            '#1the flag to search for
            '#2an optional tag to include in the search criteria
            '#3flag to include any member whose flag string contains the passed search strings instead of a full string match
            '#4flag to return a clone of the match
            '#5flag to remove the match from the collection
            '^returns the first vector that matches the search criteria
            Dim rIndex As Integer = 0
            If aOccur <= 0 Then aOccur = 1
            If aFlag Is Nothing Then Return Nothing

            Dim srch As List(Of dxfVector) = Me

            If Not bContainsString Then
                If aTag Is Nothing Then
                    srch = srch.FindAll(Function(mem) String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) = 0)
                Else
                    srch = srch.FindAll(Function(mem) String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Tag, aTag, ignoreCase:=bIgnoreCase) = 0)
                End If
            Else
                Dim comp As StringComparison = StringComparison.OrdinalIgnoreCase
                If Not bIgnoreCase Then comp = StringComparison.Ordinal
                If aFlag Is Nothing Then
                    srch = srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Flag.IndexOf(aFlag, comparisonType:=comp) > -1)
                Else
                    srch = srch.FindAll(Function(mem) mem.Flag IsNot Nothing AndAlso mem.Tag.IndexOf(aFlag, comparisonType:=comp) > -1 And mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aTag, comparisonType:=comp) > -1)
                End If
            End If
            If srch.Count <= 0 Then Return Nothing
            If aOccur >= 1 And aOccur > srch.Count Then Return Nothing
            member = srch.Item(aOccur - 1)
            Dim info As Tuple(Of dxfVector, Integer) = SetMemberInfo(member, bReturnClone, True)
            member = info.Item1
            rIndex = info.Item2
            If bRemove And member IsNot Nothing Then
                MyBase.Remove(member)
                If MaintainIndices Then ReIndex()
                If Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.Remove, member, Reflection.MethodBase.GetCurrentMethod.Name, Count + 1, Count, False, False, True)
            End If
            Return member
        End Function
        Public Function GetOrdinate(aSearchParam As dxxOrdinateTypes, Optional aPlane As dxfPlane = Nothing) As Double
            '#1parameter controling the value returned
            '#2an optional plane to define the memebr ordinates against
            '^returns the requested ordinate based on the search parameter and the members of the current collection
            Return dxfVectors.GetPlaneOrdinate(Me, aSearchParam, aPlane)
        End Function


        ''' <summary>
        ''' returns the requested ordinates in a comma delimited string
        ''' </summary>
        ''' <param name="OrdToReturn">the ordinate to return (X,Y or Z)</param>
        ''' <param name="bUniqueOnly">flag to return only the unique set</param>
        ''' <param name="aPrecis">the precis to use</param>
        ''' <returns></returns>
        Public Function GetOrdinateList(Optional OrdToReturn As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X, Optional bUniqueOnly As Boolean = False, Optional aPrecis As Integer = 4) As String
            Return dxfVectors.GetOrdinateList(Me, OrdToReturn, bUniqueOnly, aPrecis)

        End Function

        ''' <summary>
        ''' used to query the collection about the ordinates of the vectors in the current collection
        ''' </summary>
        ''' <param name="aOrdType"></param>
        ''' <param name="bUniqueValues"></param>
        ''' <param name="aPrecision"></param>
        ''' <param name="aPlane"></param>
        ''' <returns></returns>
        Public Function GetOrdinates(aOrdType As dxxOrdinateDescriptors, Optional bUniqueValues As Boolean = True, Optional aPrecision As Integer = -1, Optional aPlane As dxfPlane = Nothing) As List(Of Double)
            Return dxfVectors.GetOrdinates(Me, aOrdType, bUniqueValues, aPrecision, aPlane)

        End Function

        ''' <summary>
        ''' used to query the collection about the ordinates of the vectors in the passed collection
        ''' </summary>
        ''' <param name="rXOrds">returns the X ordinates referred to by at least one of the vectors in the collection</param>
        ''' <param name="rYOrds">2returns the Y ordinates referred to by at least one of the vectors in the collection</param>
        ''' <param name="rZOrds">2returns the Z ordinates referred to by at least one of the vectors in the collection</param>
        ''' <param name="aPlane"> a Plane that if passsed, he returned values will be with respect to the passed plane.</param>
        ''' <param name="aPrecision">a precision to round the returned values to</param>
        ''' <param name="bUniqueValues">flag to return only the unique ordinates or all of them</param>
        Public Sub GetOrdinates(ByRef rXOrds As List(Of Double), ByRef rYOrds As List(Of Double), ByRef rZOrds As List(Of Double), Optional aPlane As dxfPlane = Nothing, Optional aPrecision As Integer = -1, Optional bUniqueValues As Boolean = True)
            dxfVectors.GetOrdinates(Me, rXOrds, rYOrds, rZOrds, aPlane, aPrecision, bUniqueValues)

        End Sub

        ''' <summary>
        ''' returns the requested ordinate based on the search parameter and the members of the passed collection
        ''' </summary>
        ''' <param name="aSearchParam">parameter controling the value returned</param>
        ''' <param name="aPlane">a plane to define the member ordinates against</param>
        ''' <param name="bSuppressPlane"></param>
        ''' <param name="aPrecis"></param>
        ''' <returns>Double</returns>
        Friend Function GetPlaneOrdinate(aSearchParam As dxxOrdinateTypes, aPlane As TPLANE, Optional bSuppressPlane As Boolean = False, Optional aPrecis As Integer = -1) As Double
            If dxfPlane.IsNull(aPlane) Then bSuppressPlane = True
            Dim workplane As New dxfPlane(aPlane)

            Return dxfVectors.GetPlaneOrdinate(Me, aSearchParam, workplane, bSuppressPlane, aPrecis)
        End Function
        Public Function GetPropertyValues(aProperty As dxxVectorProperties, Optional bUniqueValues As Boolean = False, Optional aPrecis As Integer = 3) As List(Of Object)
            Dim _rVal As New List(Of Object)
            If aProperty < 1 Or aProperty > dxxVectorProperties.Suppressed Then Return _rVal
            Dim aMem As dxfVector
            Dim aVal As Object
            Dim bKeep As Boolean
            Dim aFlg As Boolean
            Dim pname As String = String.Empty
            For i As Integer = 1 To Count
                aMem = Item(i)
                aVal = dxfVector.PropertyValue(aMem, aProperty, aPrecis, aFlg, pname)
                If aFlg Then Exit For
                If bUniqueValues Then
                    bKeep = True
                    For j As Integer = 1 To _rVal.Count
                        If _rVal.Item(j) = aVal Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                    If bKeep Then _rVal.Add(aVal)
                Else
                    _rVal.Add(aVal)
                End If
            Next i
            Return _rVal
        End Function
        Public Sub GetQuadrants(aCenterPt As dxfVector, ByRef rQuad1 As List(Of dxfVector), ByRef rQuad2 As List(Of dxfVector), ByRef rQuad3 As List(Of dxfVector), ByRef rQuad4 As List(Of dxfVector), Optional aPlane As dxfPlane = Nothing, Optional bReturnClones As Boolean = True)
            '#1the vector to use as the center for the division
            '#2returns the vectors in the upper right quadrant (first quadrant)
            '#3returns the vectors in the upper left quadrant (second quadrant)
            '#4returns the vectors in the lower left quadrant (third quadrant)
            '#5returns the vectors in the lower right quadrant (fourth quadrant)
            '#6an optional plane to compare the vectors on
            '^used to create subsets of the current vector collection by dividing them into quadrants arround the passed center


            Dim bCS As New dxfPlane(New TPLANE(aPlane))
            If aCenterPt IsNot Nothing Then bCS.Origin = aCenterPt.Strukture
            Dim cx As Double = bCS.X
            Dim cy As Double = bCS.Y
            rQuad2 = New colDXFVectors(GetVectors(aFilter:=dxxPointFilters.GreaterThanY, aOrdinate:=0, bOnIsIn:=True, aPlane:=bCS), bAddClones:=False).GetVectors(aFilter:=dxxPointFilters.LessThanX, aOrdinate:=0, bOnIsIn:=True, aPlane:=bCS)
            rQuad3 = New colDXFVectors(GetVectors(aFilter:=dxxPointFilters.LessThanY, aOrdinate:=0, bOnIsIn:=False, aPlane:=bCS), bAddClones:=False).GetVectors(aFilter:=dxxPointFilters.LessThanX, aOrdinate:=0, bOnIsIn:=True, aPlane:=bCS)
            rQuad1 = New colDXFVectors(GetVectors(aFilter:=dxxPointFilters.GreaterThanY, aOrdinate:=0, bOnIsIn:=True, aPlane:=bCS), bAddClones:=False).GetVectors(aFilter:=dxxPointFilters.GreaterThanX, aOrdinate:=0, bOnIsIn:=False, aPlane:=bCS)
            rQuad4 = New colDXFVectors(GetVectors(aFilter:=dxxPointFilters.LessThanY, aOrdinate:=0, bOnIsIn:=False, aPlane:=bCS), bAddClones:=False).GetVectors(aFilter:=dxxPointFilters.GreaterThanX, aOrdinate:=0, bOnIsIn:=False, aPlane:=bCS)
        End Sub
        Public Function GetTagged(aTag As String, Optional aFlag As String = Nothing, Optional bContainsString As Boolean = False, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False, Optional aOccur As Integer = 0, Optional bIgnoreCase As Boolean = True) As dxfVector
            Dim member As dxfVector = Nothing
            '#1the tag to search for
            '#2an optional flag to include in the search criteria
            '#3flag to include any member whose tag string contains the passed search strings instead of a full string match
            '#4flag to return a clone of the match
            '#5flag to remove the match from the collection
            '^returns the first vector that match the search criteria
            If aTag Is Nothing Then Return Nothing
            If aOccur <= 0 Then aOccur = 1

            Dim srch As List(Of dxfVector) = Me
            If Not bContainsString Then
                If aFlag Is Nothing Then
                    srch = srch.FindAll(Function(mem) String.Compare(mem.Tag, aTag, ignoreCase:=bIgnoreCase) = 0)
                Else
                    srch = srch.FindAll(Function(mem) String.Compare(mem.Tag, aTag, ignoreCase:=bIgnoreCase) = 0 And String.Compare(mem.Flag, aFlag, ignoreCase:=bIgnoreCase) = 0)
                End If
            Else
                Dim comp As StringComparison = StringComparison.OrdinalIgnoreCase
                If Not bIgnoreCase Then comp = StringComparison.Ordinal
                If aFlag Is Nothing Then
                    srch = srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aTag, comparisonType:=comp) > -1)
                Else
                    srch = srch.FindAll(Function(mem) mem.Tag IsNot Nothing AndAlso mem.Tag.IndexOf(aTag, comparisonType:=comp) > -1 And mem.Flag IsNot Nothing AndAlso mem.Tag.IndexOf(aFlag, comparisonType:=comp) > -1)
                End If
            End If
            If srch.Count <= 0 Then Return Nothing
            If aOccur >= 1 And aOccur > srch.Count Then Return Nothing
            member = srch.Item(aOccur - 1)
            Dim info As Tuple(Of dxfVector, Integer) = SetMemberInfo(member, bReturnClone, True)
            member = info.Item1
            If bRemove And member IsNot Nothing Then
                MyBase.Remove(member)
                If MaintainIndices Then ReIndex()
                If Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.Remove, member, Reflection.MethodBase.GetCurrentMethod.Name, Count + 1, Count, False, False, True)
            End If
            Return member
        End Function
        Public Function GetTags() As List(Of String)
            Dim _rVal As New List(Of String)
            '^returns the tags of all the members in the collection
            For Each aMem As dxfVector In Me
                _rVal.Add(aMem.Tag)
            Next

            Return _rVal
        End Function


        Public Function GetVector(aControlFlag As dxxPointFilters, Optional aOrdinate As Double = 0D, Optional aPlane As dxfPlane = Nothing, Optional bReturnClone As Boolean = False, Optional aPrecis As Integer = 3, Optional bRemove As Boolean = False) As dxfVector
            '#1flag indicating what type of vector to search for
            '#2the ordinate to search for if the search is ordinate specific
            '#3a coordinate system to use
            '#4flag to return a clone
            '#4a precision for numerical comparison (1 to 16)
            '#5flag to remove the member from the collection
            '^returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag

            Dim v1 As TVERTEX = GetVertex(aControlFlag, aOrdinate, aPlane, aPrecis, bRemove, True, True)
            If v1.Index <= 0 Then Return Nothing
            Return Item(v1.Index, bReturnClone)
        End Function

        ''' <summary>
        ''' returns the vectors in the vectors that are contained within the passed rectangle
        ''' </summary>
        ''' <param name="aRectangle">the subject rectangle</param>
        ''' <param name="bOnIsIn">flag to the vectors on the bounds of the rectangle as withn the rectangle </param>
        ''' <param name="aPrecis">the precision to apply for the conparison</param>
        ''' <param name="bSuppressPlanes">flag to suppress plane project of the members for the test.</param>
        ''' <param name="bReturnTheInverse">flag to return the members that ARE NOT contained within the passed rectangle </param>
        ''' <param name="bReturnClones">flag to return clones </param>
        ''' <param name="bRemove">flag to remove the return memebrs (or their souce) from this collection</param>
        ''' <returns></returns>
        Public Function GetVectors(aRectangle As iRectangle, Optional bOnIsIn As Boolean = True, Optional aPrecis As Integer = 3, Optional bSuppressPlanes As Boolean = False, Optional bReturnTheInverse As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnPlanarVectors As Boolean = False) As List(Of dxfVector)
            'use the iVector function to find the members that match the search criteria
            If Not bSuppressPlanes Then bReturnPlanarVectors = False
            If bReturnPlanarVectors Then bReturnClones = False
            Dim iVecs As List(Of iVector) = dxfVectors.FindVectors(Me, aRectangle, bOnIsIn, aPrecis, bReturnTheInverse, bSuppressPlanes, bReturnPlanarVectors)
            Dim removers As New List(Of dxfVector)

            Dim _rVal As New List(Of dxfVector)
            Dim idx As Integer = 0
            'populate the return based on the indices of the matchers
            For Each ivec As iVector In iVecs

                Dim mem As dxfVector = DirectCast(ivec, dxfVector)
                idx += 1

                Dim src As dxfVector = Item(idx)
                'save the matchers to the remove list if removing is requested
                If bRemove Then removers.Add(src)
                If bReturnClones Then src = New dxfVector(mem)

                _rVal.Add(mem)



            Next
            If bRemove Then
                If RemoveMembers(removers, bSuppressEvnts:=True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + removers.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function


        ''' <summary>
        ''' returns the vectors in the vectors that are contained within the passed circle
        ''' </summary>
        ''' <param name="aCircle">the subject circle</param>
        ''' <param name="bOnIsIn">flag to the vectors on the circumference of the circle as withn the circle </param>
        ''' <param name="aPrecis">the precision to apply for the conparison</param>
        ''' <param name="bSuppressPlanes">flag to suppress plane projection of the members for the test.</param>
        ''' <param name="bReturnTheInverse">flag to return the members that ARE NOT contained within the passed circle </param>
        ''' <param name="bReturnClones">flag to return clones </param>
        ''' <param name="bRemove">flag to remove the return memebrs (or their souce) from this collection</param>
        ''' <returns></returns>
        Public Function GetVectors(aCircle As iArc, Optional bOnIsIn As Boolean = True, Optional aPrecis As Integer = 3, Optional bSuppressPlanes As Boolean = False, Optional bReturnTheInverse As Boolean = False, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False, Optional bReturnPlanarVectors As Boolean = False) As List(Of dxfVector)
            'use the iVector function to find the members that match the search criteria
            If Not bSuppressPlanes Then bReturnPlanarVectors = False
            If bReturnPlanarVectors Then bReturnClones = False
            Dim iVecs As List(Of iVector) = dxfVectors.FindVectors(Me, aCircle, bOnIsIn, aPrecis, bReturnTheInverse, bSuppressPlanes, bReturnPlanarVectors)
            Dim removers As New List(Of dxfVector)

            Dim _rVal As New List(Of dxfVector)
            Dim idx As Integer = 0
            'populate the return based on the indices of the matchers
            For Each ivec As iVector In iVecs

                Dim mem As dxfVector = DirectCast(ivec, dxfVector)
                idx += 1

                Dim src As dxfVector = Item(idx)
                'save the matchers to the remove list if removing is requested
                If bRemove Then removers.Add(src)
                If bReturnClones Then src = New dxfVector(mem)

                _rVal.Add(mem)



            Next
            If bRemove Then
                If RemoveMembers(removers, bSuppressEvnts:=True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + removers.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function


        ''' <summary>
        ''' returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag
        ''' </summary>
        ''' <param name="aFilter">search type parameter</param>
        ''' <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        ''' <param name="bOnIsIn">flag indicating if equal values should be returned</param>
        ''' <param name="aRefPt">the reference to use to compute relative distances when the filter is NearestTo or FarthestFrom  </param>
        ''' <param name="aPlane">an optional coordinate system to use</param>
        ''' <param name="aPrecis">a precision for numerical comparison (0 to 15)</param>
        ''' <param name="bReturnPlanarVectors">flag to return the vectors defined with respect to the passed plane</param>
        ''' <param name="rTheOthers">if passed, the members that do not meet the search criteria are add to the the passed list</param>

        Public Function GetVectors(aFilter As dxxPointFilters, Optional aOrdinate As Double = 0.0, Optional bOnIsIn As Boolean = True, Optional aRefPt As iVector = Nothing, Optional aPlane As dxfPlane = Nothing,
                                    Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False,
                                   Optional bReturnPlanarVectors As Boolean = False, Optional rTheOthers As List(Of iVector) = Nothing) As List(Of dxfVector)


            Dim _rVal As New List(Of dxfVector)

            Dim bSuppressPlane As Boolean = dxfPlane.IsNull(aPlane)
            Dim workplane As New dxfPlane(aPlane)
            Dim removers As New List(Of dxfVector)
            If bSuppressPlane Then bReturnPlanarVectors = False
            If bReturnPlanarVectors Then bReturnClones = True

            'use the iVector function to find the members that match the search criteria
            Dim iVecs As List(Of iVector) = dxfVectors.FindVectors(Me, aFilter, aOrdinate, bOnIsIn, aRefPt, workplane, aPrecis, bReturnPlanarVectors, rTheOthers)

            'populate the return based on the indices of the matchers
            For Each ivec As iVector In iVecs

                Dim mem As New dxfVector(ivec)

                If mem.Index >= 1 And mem.Index <= Count Then
                    Dim src As dxfVector = Item(mem.Index)
                    'save the matchers to the remove list if removing is requested
                    If bRemove Then removers.Add(src)
                    If bReturnClones Then
                        src = New dxfVector(src)
                        If bReturnPlanarVectors Then src.SetCoordinates(mem.X, mem.Y, mem.Z)
                    End If
                    _rVal.Add(src)
                Else
                    'If dxfUtils.RunningInIDE Then
                    '    MessageBox.Show("Get Vectors Problem")
                    'End If

                End If


            Next

            'Dim aPt As dxfVector
            'Dim comp As Double
            'Dim i As Integer
            'Dim aPl As New TPLANE(aPlane)
            'Dim v1 As TVERTEX
            'Dim bFilt As dxxPointFilters
            'aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            ''rIndices = New List(Of Integer)

            'If bRemove Then bReturnClones = False

            ''search for vectors aINteger a particular extreme ordinate
            'If aFilter >= dxxPointFilters.AtMaxX And aFilter <= dxxPointFilters.AtMinZ Then
            '    If aFilter = dxxPointFilters.AtMaxX Then
            '        bFilt = dxxPointFilters.AtX
            '        aOrdinate = GetOrdinate(dxxOrdinateTypes.MaxX, aPlane)
            '    ElseIf aFilter = dxxPointFilters.AtMaxY Then
            '        bFilt = dxxPointFilters.AtY
            '        aOrdinate = GetOrdinate(dxxOrdinateTypes.MaxY, aPlane)
            '    ElseIf aFilter = dxxPointFilters.AtMaxZ Then
            '        bFilt = dxxPointFilters.AtZ
            '        aOrdinate = GetOrdinate(dxxOrdinateTypes.MaxZ, aPlane)
            '    ElseIf aFilter = dxxPointFilters.AtMinX Then
            '        bFilt = dxxPointFilters.AtX
            '        aOrdinate = GetOrdinate(dxxOrdinateTypes.MinX, aPlane)
            '    ElseIf aFilter = dxxPointFilters.AtMinY Then
            '        bFilt = dxxPointFilters.AtY
            '        aOrdinate = GetOrdinate(dxxOrdinateTypes.MinY, aPlane)
            '    ElseIf aFilter = dxxPointFilters.AtMinZ Then
            '        bFilt = dxxPointFilters.AtZ
            '        aOrdinate = GetOrdinate(dxxOrdinateTypes.MinZ, aPlane)
            '    End If
            '    aFilter = bFilt
            'End If
            ''search for vectors aINteger a particular ordinate
            'If aFilter = dxxPointFilters.AtX Or aFilter = dxxPointFilters.AtY Or aFilter = dxxPointFilters.AtZ Then
            '    For i = 1 To Count
            '        aPt = Item(i, bAssignIndex:=True)
            '        v1 = aPt.VertexV
            '        If aPlane IsNot Nothing Or bReturnPlanarVectors Then v1 = v1.WithRespectTo(aPl)
            '        If aFilter = dxxPointFilters.AtX Then
            '            comp = Math.Abs(Math.Round((v1.X - aOrdinate), aPrecis))
            '        ElseIf aFilter = dxxPointFilters.AtY Then
            '            comp = Math.Abs(Math.Round((v1.Y - aOrdinate), aPrecis))
            '        Else
            '            comp = Math.Abs(Math.Round((v1.Z - aOrdinate), aPrecis))
            '        End If
            '        If comp = 0 Then
            '            If Not bReturnPlanarVectors Then _rVal.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else _rVal.Add(New dxfVector(v1))
            '            'rIndices.Add(i)
            '        Else
            '            If rTheOthers IsNot Nothing Then
            '                If Not bReturnPlanarVectors Then rTheOthers.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else rTheOthers.Add(New dxfVector(v1))
            '            End If
            '        End If
            '    Next i
            'End If
            ''search for vectors greater than a particular ordinate
            'If aFilter = dxxPointFilters.GreaterThanX Or aFilter = dxxPointFilters.GreaterThanY Or aFilter = dxxPointFilters.GreaterThanZ Then
            '    For i = 1 To Count
            '        aPt = Item(i, bAssignIndex:=True)
            '        v1 = aPt.VertexV
            '        If aPlane IsNot Nothing Or bReturnPlanarVectors Then v1 = v1.WithRespectTo(aPl)
            '        Select Case aFilter
            '            Case dxxPointFilters.GreaterThanX
            '                comp = Math.Round(v1.X - aOrdinate, aPrecis)
            '            Case dxxPointFilters.GreaterThanY
            '                comp = Math.Round(v1.Y - aOrdinate, aPrecis)
            '            Case dxxPointFilters.GreaterThanZ
            '                comp = Math.Round(v1.Z - aOrdinate, aPrecis)
            '        End Select
            '        If comp >= 0 Then
            '            If comp = 0 Then
            '                If bOnIsIn Then
            '                    If Not bReturnPlanarVectors Then _rVal.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else _rVal.Add(New dxfVector(v1))
            '                    'rIndices.Add(i)
            '                Else
            '                    If rTheOthers IsNot Nothing Then
            '                        If Not bReturnPlanarVectors Then rTheOthers.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else rTheOthers.Add(New dxfVector(v1))
            '                    End If
            '                End If
            '            Else
            '                If Not bReturnPlanarVectors Then _rVal.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else _rVal.Add(New dxfVector(v1))
            '                'rIndices.Add(i)
            '            End If
            '        Else
            '            If rTheOthers IsNot Nothing Then
            '                If Not bReturnPlanarVectors Then rTheOthers.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else rTheOthers.Add(New dxfVector(v1))
            '            End If
            '        End If
            '    Next i
            'End If
            ''search for vectors less than a particular ordinate
            'If aFilter = dxxPointFilters.LessThanX Or aFilter = dxxPointFilters.LessThanY Or aFilter = dxxPointFilters.LessThanZ Then
            '    For i = 1 To Count
            '        aPt = Item(i, bAssignIndex:=True)
            '        v1 = aPt.Strukture
            '        If aPlane IsNot Nothing Or bReturnPlanarVectors Then v1 = v1.WithRespectTo(aPl)
            '        Select Case aFilter
            '            Case dxxPointFilters.LessThanX
            '                comp = Math.Round(v1.X - aOrdinate, aPrecis)
            '            Case dxxPointFilters.LessThanY
            '                comp = Math.Round(v1.Y - aOrdinate, aPrecis)
            '            Case dxxPointFilters.LessThanZ
            '                comp = Math.Round(v1.Z - aOrdinate, aPrecis)
            '        End Select
            '        If comp <= 0 Then
            '            If comp = 0 Then
            '                If bOnIsIn Then
            '                    If Not bReturnPlanarVectors Then _rVal.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else _rVal.Add(New dxfVector(v1))
            '                    'rIndices.Add(i)
            '                Else
            '                    If rTheOthers IsNot Nothing Then
            '                        If Not bReturnPlanarVectors Then rTheOthers.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else rTheOthers.Add(New dxfVector(v1))
            '                    End If
            '                End If
            '            Else
            '                If Not bReturnPlanarVectors Then _rVal.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else _rVal.Add(New dxfVector(v1))
            '                'rIndices.Add(i)
            '            End If
            '        Else
            '            If rTheOthers IsNot Nothing Then
            '                If Not bReturnPlanarVectors Then rTheOthers.Add(Item(i, bReturnClone:=bReturnClones, bAssignIndex:=True)) Else rTheOthers.Add(New dxfVector(v1))
            '            End If
            '        End If
            '    Next i
            'End If
            If bRemove Then
                If RemoveMembers(removers, bSuppressEvnts:=True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + removers.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag
        ''' </summary>
        ''' <param name="aControlFlag">flag indicating what type of vector to search for</param>
        ''' <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        ''' <param name="aPlane">an optional coordinate system to use</param>
        ''' <param name="aPrecis">a precision for numerical comparison (0 to 15)</param>
        ''' <param name="bSuppressPlane">flag ignore the passed plane and compare the ordinates of the members without respect to the passed</param>
        ''' <param name="bResetMarks">flag to set all the member 'Mark' property to False</param>
        ''' <returns></returns>
        ''' 
        Friend Function GetVertex(aControlFlag As dxxPointFilters, Optional aOrdinate As Double = 0, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bRemove As Boolean = False, Optional bSuppressPlane As Boolean = False, Optional bSuppressEvnts As Boolean = True, Optional bResetMarks As Boolean = False) As TVERTEX
            Dim _rVal As TVERTEX = TVERTEX.Zero
            Dim rIndex As Integer = 0
            Dim iMem As iVector = dxfVectors.FindVector(Me, aControlFlag, aOrdinate, aPlane, aPrecis, bSuppressPlane, bResetMarks)
            Dim aMem As dxfVector

            If iMem IsNot Nothing Then
                aMem = DirectCast(iMem, dxfVector)
                rIndex = MyBase.IndexOf(aMem) + 1
            End If


            _rVal.Index = 0
            If rIndex > 0 Then

                aMem = Item(rIndex)

                _rVal = aMem.VertexV
                _rVal.Index = rIndex
                If bRemove Then
                    MyBase.Remove(aMem)
                    If Not _SuppressEvents And Not bSuppressEvnts Then
                        RaiseChangeEvent(dxxCollectionEventTypes.Remove, aMem, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, True)
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Sub InvertOrdinate(aOrdinate As dxxOrdinateDescriptors, aBaseOrdinate As Double)
            '#1the ordinate to work on
            '#2the reference ordinate
            '^sets the indicated ordinate to 2 times the difference between the current ordinate value and the passes base ordinate.
            '~this is basically mirroring orthogonal across the passed ordinate
            Dim i As Integer
            Dim aMem As dxfVector
            Dim dif As Double
            For i = 1 To Count
                aMem = Item(i)
                If aOrdinate = dxxOrdinateDescriptors.Y Then
                    dif = aBaseOrdinate - aMem.Y
                    aMem.Y = aBaseOrdinate + dif
                ElseIf aOrdinate = dxxOrdinateDescriptors.Z Then
                    dif = aBaseOrdinate - aMem.Z
                    aMem.Z = aBaseOrdinate + dif
                Else
                    dif = aBaseOrdinate - aMem.X
                    aMem.X = aBaseOrdinate + dif
                End If
            Next i
        End Sub
        ''' <summary>
        ''' returns true of the this collection contain the same number of points and the members are equal within the precision  
        ''' </summary>
        ''' '''<remarks> the order does not mattter unless orderwise is true</remarks>
        ''' <param name="aPtCol">the first comparitor</param>
        ''' <param name="aPrecis">the precision to apply</param>
        ''' <param name="bOrderWise">if true, the memebrs must be equal and in the same order</param>
        ''' <returns></returns>
        Public Function IsEqual(aPtCol As IEnumerable(Of iVector), Optional aPrecis As Integer = 4, Optional bOrderWise As Boolean = False) As Boolean
            Return dxfVectors.IsEqual(Me, aPtCol, aPrecis, bOrderWise)

        End Function
        '#1the requested item number
        '#2flag to return a copy of the requested vector
        '#3occurance number to use when searching by tag
        '^returns the object from the collection at the requested index in the collection.
        '~returns nothing if the passed index is outside the bounds of the current collection
        '~if a string is passed i.e "VECTOR 1" then the member with the matching tag at the indicted occurance is returned
        Public Shadows Function Item(aIndex As Integer, Optional bReturnClone As Boolean = False, Optional bAssignIndex As Boolean = False) As dxfVector
            Dim _rVal As dxfVector = Nothing
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            _rVal = MyBase.Item(aIndex - 1)
            If _rVal Is Nothing Then Return Nothing
            Return SetMemberInfo(_rVal, bReturnClone:=bReturnClone, bAssignIndex:=bAssignIndex, aIndex:=aIndex).Item1
        End Function

        Public Function NextVector(aMember As dxfVector, Optional aStep As Integer = 1, Optional bReturnClone As Boolean = False) As dxfVector

            If Count <= 0 Then Return Nothing
            Dim idx As Integer = IndexOf(aMember)
            If idx < 0 Then Return Nothing

            idx += aStep
            If idx = 0 Then
                idx = Count
            ElseIf idx < 0 Then
                idx = Count + idx
            End If

            Do While idx > Count
                idx -= Count
            Loop

            If idx > 1 And idx <= Count Then
                Return Item(idx, bReturnClone)
            Else
                Return Nothing
            End If


        End Function

        Private Function SetMemberInfo(aMember As dxfVector, Optional bReturnClone As Boolean = False, Optional bAssignIndex As Boolean = False, Optional aIndex As Integer = 0) As Tuple(Of dxfVector, Integer)
            If aMember Is Nothing Then Return New Tuple(Of dxfVector, Integer)(Nothing, 0)
            If MonitorMembers Then
                aMember.CollectionGUID = _CollectionGUID
                aMember.CollectionPtr = New WeakReference(Me)
            Else
                aMember.ReleaseCollectionReference()
            End If
            aMember.SetGUIDS(_ImageGUID, _OwnerGUID, _BlockGUID, SuppressEvents)
            If aIndex <= 0 Then aIndex = MyBase.IndexOf(aMember) + 1

            If bReturnClone Then aMember = New dxfVector(aMember)

            If MaintainIndices Or bAssignIndex Then aMember.Index = aIndex
            aMember.ColID = aIndex
            Return New Tuple(Of dxfVector, Integer)(aMember, aIndex)
        End Function
        Friend Function ItemVertex(aIndex As Integer, Optional bSuppressIndexErr As Boolean = True) As TVERTEX
            Dim _rVal As TVERTEX = TVERTEX.Zero
            '#1the requested item number
            '^returns the vector from the collection at the requested index in the collection.
            If Count <= 0 Then
                If bSuppressIndexErr Then Return TVERTEX.Zero
                Throw New IndexOutOfRangeException
            End If
            Dim idx As Integer = aIndex
            Dim cnt As Integer = Count

            If idx = 0 Then
                idx = cnt
            ElseIf idx > 0 Then
                If idx > cnt Then
                    Do Until idx <= cnt
                        idx -= cnt
                    Loop
                End If
            ElseIf idx < 0 Then
                If Math.Abs(idx) > cnt Then
                    idx = Math.Abs(idx)
                    Do Until idx <= cnt
                        idx -= cnt
                    Loop
                    idx = -idx
                End If
                idx = cnt + idx + 1
            End If
            If idx > 0 And idx <= cnt Then
                _rVal = Item(idx).VertexV
            Else
                If bSuppressIndexErr Then Return TVERTEX.Zero
                Throw New IndexOutOfRangeException
            End If
            Return _rVal
        End Function
        Friend Sub SetItemVertex(aIndex As Integer, value As TVERTEX)
            If aIndex < 1 Or aIndex > Count Then Return
            MyBase.Item(aIndex - 1).VertexV = value
        End Sub
        Public Function Jumbled() As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '^returns a copy of the current collection but randomly sorted
            If Count <= 0 Then Return _rVal

            Dim ids As New List(Of Integer)

            Do Until _rVal.Count = Count
                Dim idx As Integer = dxfUtils.RandomInteger(1, Count)
                If Not ids.Contains(idx) Then
                    ids.Add(idx)
                    _rVal.Add(Item(idx, True))
                End If
            Loop

            Return _rVal
        End Function
        Public Function Last(aCount As Integer, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1the number of points to return
            '#2flag to return copies
            '#3flag to remove the subset from the collection
            '^returns the last members of the collection up to the passed count
            '~i.e. Last(4) returns the last 4 members
            If bRemove Then bReturnClones = False
            If aCount <= 0 Then Return _rVal
            If aCount > Count Then aCount = Count
            Dim i As Integer
            Dim v1 As dxfVector
            aCount = Count - aCount + 1
            If aCount <= 0 Then aCount = 1
            For i = aCount To Count
                v1 = Item(i, bReturnClones)
                _rVal.Add(v1)
            Next i
            If bRemove Then
                If RemoveMembers(_rVal, bSuppressEvnts:=True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
            End If
            Return _rVal
        End Function
        Public Function LastVector(Optional bReturnClone As Boolean = False) As dxfVector
            '^returns the last vector in the collection (if there are any)
            If Count <= 0 Then Return Nothing
            Return Item(Count, bReturnClone)
        End Function
        Public Function LayerName(aIndex As Integer) As String
            Dim _rVal As String = String.Empty
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            _rVal = MyBase.Item(aIndex - 1).LayerName
            If String.IsNullOrWhiteSpace(_rVal) Then _rVal = "0"
            Return _rVal
        End Function
        Public Function LayerNames(Optional aLayerToInclude As String = "", Optional bUnquieValues As Boolean = True) As List(Of String)
            Dim _rVal As New List(Of String)
            '^all of the layernames names referenced by the entities in the collection
            If Not String.IsNullOrWhiteSpace(aLayerToInclude) Then _rVal.Add(aLayerToInclude.Trim)


            Dim keep As Boolean
            For i As Integer = 1 To Count
                Dim aMem As dxfVector = MyBase.Item(i - 1)
                Dim lname As String = aMem.LayerName
                If String.IsNullOrEmpty(lname) Then lname = "0"
                keep = True
                If bUnquieValues Then
                    keep = _rVal.IndexOf(lname) < 0
                End If
                If keep Then _rVal.Add(lname)
            Next i
            Return _rVal
        End Function
        Public Function LineFromTo(aSPIndex As Integer, aEPIndex As Integer, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aTag As String = Nothing) As dxeLine
            Dim v1 As TVERTEX = ItemVertex(aSPIndex, True)
            If Not String.IsNullOrEmpty(aTag) Then v1.Tag = aTag

            Return New dxeLine(LineSegment(aSPIndex, aEPIndex), False, 0, aDisplaySettings:=aDisplaySettings) With {.Tag = v1.Tag, .Flag = v1.Flag}
        End Function
        Public Function LineSegment(aStartID As Integer, Optional bInvertEndPt As Boolean = False, Optional aVertexStep As Integer = 1) As dxeLine
            If aStartID < 0 Or aStartID > Count Or Count < 2 Then Return Nothing
            Dim v1 As dxfVector = Item(aStartID)
            Dim i As Integer = aStartID

            Dim _rVal As New dxeLine(v1, v1)
            If bInvertEndPt Then
                i -= Math.Abs(aVertexStep)
            Else
                i += Math.Abs(aVertexStep)
            End If
            If i >= 1 And i <= Count Then _rVal.EndPt = Item(i, True)
            Return _rVal
        End Function
        Public Function LineSegment(aStartTag As String, Optional bInvertEndPt As Boolean = False, Optional aVertexStep As Integer = 1) As dxeLine
            Dim v1 As dxfVector = GetTagged(aStartTag)
            If v1 Is Nothing Then Return Nothing
            Dim i As Integer = MyBase.IndexOf(v1) + 1
            Dim _rVal As New dxeLine(v1, v1)
            If bInvertEndPt Then
                i -= Math.Abs(aVertexStep)
            Else
                i += Math.Abs(aVertexStep)
            End If
            If i >= 1 And i <= Count Then _rVal.EndPt = Item(i, True)
            Return _rVal
        End Function
        Friend Function LineSegment(aSPIndex As Integer, aEPIndex As Integer) As TLINE
            Dim v1 As TVERTEX = ItemVertex(aSPIndex, True)

            Return New TLINE(v1.Vector, ItemVector(aEPIndex, True)) With {.Tag = v1.Tag}

        End Function
        Friend Function ToLineSegments(Optional bClosed As Boolean = False) As TLINES
            Dim _rVal As New TLINES(0)
            If Count <= 1 Then Return _rVal

            Dim P2 As dxfVector = Nothing

            For i As Integer = 1 To Count - 1

                Dim v1 As TVECTOR = ItemVector(i)
                Dim v2 As TVECTOR = ItemVector(i + 1)
                P2 = Item(i + 1)
                If dxfProjections.DistanceTo(v1, v2) > 0.001 Then
                    _rVal.Add(v1, v2)
                End If
            Next i
            If bClosed And P2 IsNot Nothing Then
                If _rVal.Count > 0 Then
                    Dim v1 As TVECTOR = New TVECTOR(P2)
                    Dim v2 As TVECTOR = ItemVector(1, True)
                    If dxfProjections.DistanceTo(v1, v2) > 0.0001 Then
                        _rVal.Add(v1, v2)
                    End If
                End If
            End If
            Return _rVal
        End Function

        Public Function Linetype(aIndex As Integer) As String
            Dim _rVal As String = String.Empty
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            _rVal = MyBase.Item(aIndex - 1).Linetype
            If String.IsNullOrWhiteSpace(_rVal) Then _rVal = dxfLinetypes.ByLayer
            Return _rVal
        End Function

        Public Function Linetypes(Optional aLTToInclude As String = "", Optional bUnquieValues As Boolean = True) As List(Of String)
            Dim _rVal As New List(Of String)
            '^all of the linetype names referenced by the entities in the collection
            If Not String.IsNullOrWhiteSpace(aLTToInclude) Then _rVal.Add(aLTToInclude.Trim)
            Dim aMem As dxfVector
            Dim lname As String
            Dim keep As Boolean
            For i As Integer = 1 To Count
                aMem = MyBase.Item(i - 1)
                lname = aMem.Linetype
                If String.IsNullOrEmpty(lname) Then lname = dxfLinetypes.ByLayer
                keep = True
                If bUnquieValues Then
                    keep = _rVal.IndexOf(lname) < 0
                End If
                If keep Then _rVal.Add(lname)
            Next i
            Return _rVal
        End Function

        Public Function MidPoint(Optional bClosed As Boolean = False) As dxfVector
            Return New dxfVector With {.Strukture = MidPointV(bClosed)}
        End Function

        Friend Function MidPointV(Optional bClosed As Boolean = False) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            Dim ltot As Double
            Dim l1 As Double
            Dim l2 As Double
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            If Count = 1 Then
                _rVal = ItemVector(1)
            ElseIf Count = 2 Then
                _rVal = ItemVector(1).MidPt(ItemVector(2))
            Else
                For i = 1 To Count
                    v1 = ItemVector(i)
                    If i = Count Then
                        If Not bClosed Then Exit For Else v2 = ItemVector(1)
                    Else
                        v2 = ItemVector(i + 1)
                    End If
                    ltot += dxfProjections.DistanceTo(v1, v2)
                Next i
                If ltot > 0 Then
                    ltot /= 2
                    For i = 1 To Count
                        v1 = ItemVector(i)
                        If i = Count Then
                            If Not bClosed Then Exit For Else v2 = ItemVector(1)
                        Else
                            v2 = ItemVector(i + 1)
                        End If
                        l1 = dxfProjections.DistanceTo(v1, v2)
                        If l1 > 0 Then
                            If l2 + l1 >= ltot Then
                                _rVal = v1 + v1.DirectionTo(v2) * (ltot - l2)
                                Exit For
                            Else
                                l2 += l1
                            End If
                        End If
                    Next i
                End If
            End If
            Return _rVal
        End Function

        Public Function Mirror(aMirrorAxis As iLine) As Boolean
            '#1the line to mirror across
            '^mirrors the vectors across the passed line
            '~returns True if the vectors actually moves from this process
            If aMirrorAxis Is Nothing Then Return False

            Return Transform(TTRANSFORM.CreateMirror(aMirrorAxis, False), aEventName:="Mirror")
        End Function


        Public Function MirrorCopy(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aStartID As Integer = 0, Optional aEndID As Integer = 0, Optional bReverseOrder As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors(Me, bAddClones:=True)
            If aMirrorX Is Nothing And aMirrorY Is Nothing Then Return _rVal
            '#1the x coordinate to mirror across
            '#2the y coordinate to mirror across
            '^returns a new set of point that are the mirrors of the current collect
            '~only allows orthogonal mirroring.
            _rVal.MirrorPlanar(aMirrorX, aMirrorY, aPlane, aStartID, aEndID, bReverseOrder:=bReverseOrder)
            Return _rVal
        End Function

        Public Function MirrorPlanar(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aStartID As Integer = 0, Optional aEndID As Integer = 0, Optional bReverseOrder As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If Not aMirrorX.HasValue And Not aMirrorY.HasValue Then Return False
            '#1the x coordinate to mirror across
            '#2the y coordinate to mirror across
            '^moves the current coordinates of vertices in the collection to a vector mirrored across the passed values
            '~only allows orthogonal mirroring.
            Dim aPl As New TPLANE("")
            Dim aLn As New TLINE
            Dim aTrs As New TTRANSFORMS
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane)
            If aMirrorX.HasValue Then
                aLn = aPl.LineV(aMirrorX.Value, 10)
                aTrs.Add(TTRANSFORM.CreateMirror(aLn, True))
            End If
            If aMirrorY.HasValue Then
                aLn = aPl.LineH(aMirrorY.Value, 10)
                aTrs.Add(TTRANSFORM.CreateMirror(aLn, True))
            End If
            If aTrs.Count > 0 Then
                aTrs.SetEventSuppression(aTrs.Count, _SuppressEvents)
                _rVal = Transform(aTrs, aEventName:="MirrorPlanar", aStartID:=aStartID, aEndID:=aEndID, bReverseOrder:=bReverseOrder)
            End If
            Return _rVal
        End Function

        Public Function Move(Optional aChangeX As Double = 0, Optional aChangeY As Double = 0, Optional aChangeZ As Double = 0, Optional aPlane As dxfPlane = Nothing) As Boolean

            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the members by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Return Transform(TTRANSFORM.CreateTranslation(aChangeX, aChangeY, aChangeZ, aPlane), aEventName:="Move")

        End Function

        Public Function MoveFromTo(aBasePointXY As iVector, aDestinationPointXY As iVector, Optional aXChange As Double = 0, Optional aYChange As Double = 0, Optional aZChange As Double = 0) As Boolean

            '^used to move the object from one reference vector to another
            Return Transform(TTRANSFORM.CreateFromTo(aBasePointXY, aDestinationPointXY, aXChange, aYChange, aZChange), aEventName:="MoveFromTo")

        End Function

        Public Function NearestToLine(aLine As iLine, ByRef rDistance As Double, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfVector
            '#1the line to test
            '#2returns the distance between the returned vector and the passed Line
            '#3returns the collection index of the returned vector
            '^returns the vector from the collection which is the nearest to the passed line
            rDistance = 0
            If aLine Is Nothing Then Return Nothing

            Return NearestToLineV(New TLINE(aLine), rDistance, bReturnClone, bRemove)
        End Function

        Friend Function NearestToLineV(aLine As TLINE, ByRef rDistance As Double, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfVector
            rDistance = 0
            Dim rIndex As Integer
            Dim _rVal As dxfVector = Nothing
            '#1the line to test
            '#2returns the distance between the returned vector and the passed Line
            '#3returns the collection index of the returned vector
            '^returns the vector from the collection which is the nearest to the passed line
            If aLine.SPT.DistanceTo(aLine.EPT, 6) = 0 Then


                dxfVectors.GetRelativeMember(True, Me, New dxfVector(aLine.EPT), rDistance, rIndex)

                If rIndex > 0 Then _rVal = Item(rIndex)

            Else

                Dim d1 As Double
                Dim minval As Double

                rIndex = -1
                minval = 3.6E+36
                rDistance = 0
                For i As Integer = 1 To Count
                    Dim P1 As dxfVector = Item(i)
                    Dim v1 As TVECTOR = P1.Strukture
                    v1 = dxfProjections.ToLine(v1, aLine, rDistance:=d1)
                    If d1 < minval Then
                        minval = d1
                        rIndex = i
                    End If
                    If d1 = 0 Then Exit For
                Next i
                If rIndex > 0 Then
                    rDistance = minval

                Else
                    rDistance = 0
                End If
            End If

            If rIndex > 0 Then

                _rVal = Item(rIndex, bReturnClone)
                If bRemove Then Remove(rIndex)
            End If

            Return _rVal
        End Function

        ''' <summary>
        ''' returns the vector from the collection which is the nearest to the passed vector
        ''' </summary>
        ''' <param name="aSearchVector">the vector to compare to</param>
        ''' <param name="bReturnClone"> flag to return a clone</param>
        ''' <param name="bRemove">flag to remove the nearest vector form this collection</param>
        ''' <returns></returns>
        Public Function NearestVector(aSearchVector As iVector, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfVector
            Dim rDistance As Double = 0.0
            Return NearestVector(aSearchVector, rDistance, bReturnClone:=bReturnClone, bRemove:=bRemove)
        End Function

        ''' <summary>
        ''' returns the vector from the collection which is the nearest to the passed vector
        ''' </summary>
        ''' <param name="aSearchVector">the vector to compare to</param>
        ''' <param name="rDistance">returns the distance between the nearest vector (if there is one)</param>
        ''' <param name="aDirection">if passed, only members whose direction to or from the passed search vector are considered</param>
        ''' <param name="bCompareInverseDirection">flag to only consider members whose direction is from the search vector to the member is a direction is passed</param>
        ''' <param name="bReturnClone"> flag to return a clone</param>
        ''' <param name="bRemove">flag to remove the nearest vector form this collection</param>
        ''' <returns></returns>
        Public Function NearestVector(aSearchVector As iVector, ByRef rDistance As Double, Optional aDirection As dxfDirection = Nothing, Optional bCompareInverseDirection As Boolean = False, Optional bReturnClone As Boolean = False, Optional bRemove As Boolean = False) As dxfVector
            rDistance = 0
            If aSearchVector Is Nothing Then Return Nothing
            If Count <= 0 Then Return Nothing

            Dim rIndex As Integer = 0
            Dim _rVal As dxfVector = DirectCast(dxfVectors.GetRelativeMember(True, Me, aSearchVector, rDistance, rIndex, aDirection, bCompareInverseDirection), dxfVector)

            If rIndex <= 0 Then Return Nothing
            rIndex = MyBase.IndexOf(_rVal) + 1
            If bRemove Then Remove(_rVal)
            If bReturnClone Then
                Return New dxfVector(_rVal)
            Else
                Return _rVal
            End If

        End Function


        Friend Sub RespondToMemberEvent(aEvent As dxfVertexEvent)
            If Count <= 0 Or _SuppressEvents Or aEvent Is Nothing Then Return
            aEvent.CollectionNotified = True
            If aEvent.Undo Then Return
            If MyBase.IndexOf(aEvent.Vertex) < 0 Then
                Return
            End If
            RaiseEvent VectorsMemberChange(aEvent)
            If Not String.IsNullOrWhiteSpace(_OwnerGUID) Then
                Dim owner As dxfHandleOwner = MyOwner
                If owner IsNot Nothing Then
                    owner.RespondToVectorsMemberChange(aEvent)
                    aEvent.ImageNotified = True
                End If
            End If
            'Dim entity As dxfEntity = MyOwner
            'If MonitorMembers And Not aEvent.ImageNotified Then
            '    If ImageGUID <> "" And EntityGUID <> "" Then
            '        entity = dxfEvents.GetImageEntity(ImageGUID, EntityGUID, BlockGUID, aEvent.Block)
            '        If entity  IsNot Nothing Then
            '            If aEvent.Vertex.DefPntIndex > 0 Then
            '                aEvent.ImageNotified = True
            '                entity.RespondToDefPtChange(aEvent)
            '            Else
            '                If entity.HasVertices AndAlso entity.Vertices.GUID = aEvent.CollectionGUID Then
            '                    aEvent.ImageNotified = True
            '                    entity.RespondToVectorsMemberChange(aEvent)
            '                End If
            '            End If
            '        End If
            '    End If
            'End If
        End Sub

        Public Function OrdinateList(aOrdinateType As dxxOrdinateDescriptors, Optional aPrecis As Integer = -1, Optional aDelimiter As String = ",", Optional aPlane As dxfPlane = Nothing, Optional bSortLowToHigh As Boolean = False, Optional bUniqueVaues As Boolean = True) As String
            Dim _rVal As String = String.Empty
            '^returns the unique X,Y or Z ordinates referred to by at least one of the vectors in the collection
            '^used to query the collection about the ordinates of the vectors in the current collection
            Dim aOrds As List(Of Double)
            Dim i As Integer
            aOrds = Ordinates(aOrdinateType, aPrecis, aPlane, bSortLowToHigh, bUniqueVaues)
            For i = 1 To aOrds.Count
                TLISTS.Add(_rVal, aOrds.Item(i - 1), bAllowDuplicates:=True, aDelimitor:=aDelimiter)
            Next i
            Return _rVal
        End Function

        Public Function Ordinates(aOrdinateType As dxxOrdinateDescriptors, Optional aPrecis As Integer = -1, Optional aPlane As dxfPlane = Nothing, Optional bSortLowToHigh As Boolean = False, Optional bUniqueVaues As Boolean = True) As List(Of Double)
            '^returns the unique X,Y or Z ordinates referred to by at least one of the vectors in the collection
            '^used to query the collection about the ordinates of the vectors in the current collection
            Return PlanarOrdinates(aOrdinateType, New TPLANE(aPlane), aPrecis, bSortLowToHigh, True, bUniqueVaues)
        End Function

        Public Function Orthoganolize(aMergeDistance As Double, bMergeOnX As Boolean, bMergeOnY As Boolean, bMergeOnZ As Boolean) As Boolean
            Dim _rVal As Boolean = False



            For i As Integer = Count To 1 Step -1
                Dim v1 As dxfVector = Item(i)
                For j As Integer = i - 1 To 1 Step -1
                    Dim v2 As dxfVector = Item(j)
                    If bMergeOnX Then
                        If v2.X >= v1.X - aMergeDistance And v2.X <= v1.X + aMergeDistance Then
                            If v2.X <> v1.X Then _rVal = True
                            v2.X = v1.X
                        End If
                    End If
                    If bMergeOnY Then
                        If v2.Y >= v1.Y - aMergeDistance And v2.Y <= v1.Y + aMergeDistance Then
                            If v2.Y <> v1.Y Then _rVal = True
                            v2.Y = v1.Y
                        End If
                    End If
                    If bMergeOnZ Then
                        If v2.Z >= v1.Z - aMergeDistance And v2.Z <= v1.Z + aMergeDistance Then
                            If v2.Z <> v1.Z Then _rVal = True
                            v2.Z = v1.Z
                        End If
                    End If
                Next j
            Next i
            Return _rVal
        End Function

        Public Function PlanarCenter(aPlane As dxfPlane, Optional bSuppressProjection As Boolean = False) As dxfVector
            Dim rXSpan As Double = 0
            Dim rYSpan As Double = 0
            '#1the plane to test
            '#4flag to skip the projection of the members to the plane (set to True if the vectors are know to be on the passed plane)
            '^the center of all the members with respect to the horizontal and vertical directions of the passed plane
            Return New dxfVector(PlanarCenterV(New TPLANE(aPlane), rXSpan, rYSpan, bSuppressProjection))
        End Function

        Public Function PlanarCenter(aPlane As dxfPlane, ByRef rXSpan As Double, ByRef rYSpan As Double, Optional bSuppressProjection As Boolean = False) As dxfVector
            '#1the plane to test
            '#2returns the horizontal span of the vectors in the collection with respect to the calulated center and the horizontal direction of the passed plane
            '#3returns the vertical span of the vectors in the collection with respect to the calulated center and the vertical direction of the passed plane
            '#4flag to skip the projection of the members to the plane (set to True if the vectors are know to be on the passed plane)
            '^the center of all the members with respect to the horizontal and vertical directions of the passed plane
            Return New dxfVector(PlanarCenterV(New TPLANE(aPlane), rXSpan, rYSpan, bSuppressProjection))
        End Function

        Friend Function PlanarCenterV(aPlane As TPLANE, ByRef rXSpan As Double, ByRef rYSpan As Double, Optional bSuppressProjection As Boolean = False) As TVECTOR
            '#1the plane to test
            '#2returns the horizontal span of the vectors in the collection with respect to the calulated center and the horizontal direction of the passed plane
            '#3returns the vertical span of the vectors in the collection with respect to the calulated center and the vertical direction of the passed plane
            '#4flag to skip the projection of the members to the plane (set to True if the vectors are know to be on the passed plane)
            '^the center of all the members with respect to the horizontal and vertical directions of the passed plane
            rXSpan = 0
            rYSpan = 0
            If Count = 0 Then Return aPlane.Origin
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim UL As TVECTOR
            Dim LR As TVECTOR
            Dim minX As Double
            Dim MinY As Double
            Dim maxX As Double
            Dim MaxY As Double
            If Count > 0 Then
                For i As Integer = 1 To Count
                    v1 = ItemVector(i)
                    v2 = v1.WithRespectTo(aPlane)
                    If i = 1 Then
                        minX = v2.X
                        maxX = v2.X
                        MinY = v2.Y
                        MaxY = v2.Y
                    Else
                        If v2.X < minX Then minX = v2.X
                        If v2.X > maxX Then maxX = v2.X
                        If v2.Y < MinY Then MinY = v2.Y
                        If v2.Y > MaxY Then MaxY = v2.Y
                    End If
                Next i
            End If
            rXSpan = Math.Abs(maxX - minX)
            rYSpan = Math.Abs(MaxY - MinY)
            UL = aPlane.Origin + (aPlane.XDirection * minX) + (aPlane.YDirection * MaxY)
            LR = aPlane.Origin + (aPlane.XDirection * maxX) + (aPlane.YDirection * MinY)
            Return UL.Interpolate(LR, 0.5)
        End Function

        Friend Function PlanarOrdinates(aOrdinateType As dxxOrdinateDescriptors, aPlane As TPLANE, Optional aPrecis As Integer = -1, Optional bSortLowToHigh As Boolean = False, Optional bSuppressPlane As Boolean = False, Optional bUniqueVaues As Boolean = True) As List(Of Double)
            '^returns the unique X,Y or Z ordinates referred to by at least one of the vectors in the collection
            '^used to query the collection about the ordinates of the vectors in the current collection
            Return dxfVectors.PlanarOrdinates(Me, aOrdinateType, New dxfPlane(aPlane), aPrecis, bSortLowToHigh, bSuppressPlane, bUniqueVaues)
        End Function


        Friend Sub Populate(aVectors As TVECTORS)
            Clear()
            Append(aVectors)
        End Sub

        Friend Sub Populate(aVertices As TVERTICES)
            Clear()
            Append(aVertices)
        End Sub


        ''' <summary>
        ''' clears the current collection and popolates it with the passed vectors
        ''' </summary>
        ''' <param name="NewVectors">the vectors to add</param>
        ''' <param name="bAddClones">flag to add clones of the passed vectors</param>
        ''' <param name="bReturnString">flag to return a list of the coordinates of the added vectors</param>
        ''' <param name="aFinalCount">if passed and greater than zero, the final member count will be increased or reduced to this number</param>
        ''' <returns></returns>
        Public Function Populate(NewVectors As IEnumerable(Of iVector), Optional bAddClones As Boolean = True, Optional bReturnString As Boolean = False, Optional aFinalCount As Integer = 0) As String
            Dim _rVal As String = String.Empty
            Dim cnt As Integer = Count
            MyBase.Clear()

            If NewVectors Is Nothing Then Return _rVal
            Dim v1 As dxfVector
            For Each iv As iVector In NewVectors
                v1 = dxfVector.FromIVector(iv, bCloneIt:=bAddClones)
                If v1 Is Nothing Then Continue For

                v1 = AddToCollection(v1, bSuppressReindex:=True, bSuppressEvnts:=True, aAddClone:=False)
                If bReturnString Then
                    If v1 IsNot Nothing Then
                        If Not String.IsNullOrWhiteSpace(_rVal) Then _rVal += dxfGlobals.Delim
                        _rVal += v1.Strukture.Coordinates(0)
                    End If
                End If

            Next


            If aFinalCount > 0 And Count <> aFinalCount Then
                If Count < aFinalCount Then
                    Do Until Count = aFinalCount
                        If Count > 0 Then
                            v1 = AddToCollection(Item(Count), bSuppressReindex:=True, bSuppressEvnts:=True, aAddClone:=True)
                        Else
                            v1 = dxfVector.Zero
                        End If
                        If bReturnString Then
                            If v1 IsNot Nothing Then
                                If _rVal <> "" Then _rVal += dxfGlobals.Delim
                                _rVal += v1.Strukture.Coordinates()
                            End If
                        End If
                    Loop
                Else
                    Do Until Count = aFinalCount
                        MyBase.Remove(MyBase.Item(Count - 1))
                    Loop
                End If
                If MaintainIndices Then ReIndex()
            End If
            If Not _SuppressEvents Then
                RaiseChangeEvent(dxxCollectionEventTypes.Populate, NewVectors, Reflection.MethodBase.GetCurrentMethod.Name, cnt, Count, False, False, True)
            End If
            Return _rVal
        End Function

        Public Function Project(aDirection As dxfDirection, aDistance As Double) As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the corners in the passed direction the requested distance
            If aDirection Is Nothing Or aDistance = 0 Or Count <= 0 Then Return False
            Return Transform(TTRANSFORM.CreateProjection(aDirection, aDistance), aEventName:="Project")
        End Function

        Public Function ProjectToLine(aLine As iLine) As Boolean
            If Count <= 0 Or aLine Is Nothing Then Return False
            '#1plane to project to
            '^returns the members projected to the passed plane
            Dim bLine As New TLINE(aLine)

            If bLine.Length = 0 Then Return False


            Dim ortho As New TVECTOR(0)
            Dim aLn As New TLINE(bLine)
            Dim _rVal As Boolean
            Dim movers As New List(Of dxfVector)
            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                If v1.ProjectToLine(aLn, ortho, bSuppressEvnts:=True) Then
                    _rVal = True
                    movers.Add(v1)
                End If
            Next i
            If _rVal And Not _SuppressEvents Then
                Dim bUndo As Boolean
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, bUndo, False, False)
                If bUndo Then
                    _rVal = False
                    ResetToLast()
                    RaiseChangeEvent(dxxCollectionEventTypes.CollectionMoveUndo, movers, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, False)
                End If
            End If
            Return _rVal
        End Function


        Friend Function Project(aDirection As TVECTOR, aDistance As Double, Optional bSuppressEvnts As Boolean = False) As Boolean
            If Count <= 0 Or aDistance = 0 Or aDirection.IsNull Then Return False
            '#1the direction to project in
            '#2the distance to project
            '^projects the corners in the passed direction the requested distance

            Dim movers As New List(Of dxfVector)
            aDirection.Normalize()
            For i As Integer = 1 To Count
                Dim P1 As dxfVector = Item(i)
                If P1.Project(aDirection, aDistance, True) Then
                    movers.Add(P1)
                End If
            Next i
            If movers.Count > 0 And Not _SuppressEvents And Not bSuppressEvnts Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, False)
            End If
            Return True
        End Function

        Public Function ProjectedToLine(aLine As iLine) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1plane to project to
            '^returns the members projected to the passed plane

            Dim bLine As New TLINE(aLine)

            If bLine.Length = 0 Then Return _rVal


            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i, True)
                Dim v1 As TVECTOR = dxfProjections.ToLine(aMem.Strukture, bLine)
                aMem.Strukture = v1
                _rVal.Add(aMem)
            Next i
            Return _rVal
        End Function

        ''' <summary>
        ''' projects the vectors to the passed plane
        ''' </summary>
        ''' <param name="aPlane">the plane to project to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aDirection">an optional direction vector. if not passed, the Z direction of the plane is used</param>
        ''' <remarks></remarks>
        Public Sub ProjectToPlane(Optional aPlane As dxfPlane = Nothing, Optional aDirection As dxfDirection = Nothing)
            dxfVectors.ProjectVectorsToPlane(Me, aPlane, aDirection)
        End Sub


        ''' <summary>
        ''' returns a clone of the vectors projected to the passed plane. 
        ''' </summary>
        ''' <param name="aPlane">the plane to project to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aDirection">an optional direction vector. if not passed, the Z direction of the plane is used</param>
        ''' <remarks></remarks>

        Public Function ProjectedToPlane(aPlane As dxfPlane, Optional aDirection As dxfDirection = Nothing) As colDXFVectors
            Dim _rVal As colDXFVectors = New colDXFVectors(Me, bAddClones:=True)
            dxfVectors.ProjectVectorsToPlane(_rVal, aPlane, aDirection)
            Return _rVal
        End Function

        ''' <summary>
        ''' returns a clone of the vectors projected to the passed plane. 
        ''' </summary>
        ''' <param name="aPlane">the plane to project to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aDirection">an optional direction vector. if not passed, the Z direction of the plane is used</param>
        ''' <remarks></remarks>
        Friend Function ProjectedToPlane(aPlane As TPLANE, Optional aDirection As dxfDirection = Nothing) As colDXFVectors
            Dim _rVal As colDXFVectors = New colDXFVectors(Me, bAddClones:=True)
            dxfVectors.ProjectVectorsToPlane(_rVal, aPlane, aDirection)
            Return _rVal
        End Function

        Public Sub ReIndex()
            '^updates collection indices of the current members


            For i As Integer = Count - 1 To 0 Step -1
                Dim aMem As dxfVector = MyBase.Item(i)
                If aMem IsNot Nothing Then
                    SetMemberInfo(aMem, aIndex:=i + 1)
                Else
                    MyBase.RemoveAt(i)
                End If
                'Application.DoEvents()
            Next i
        End Sub

        Public Function ReduceToCount(aCount As Integer, Optional bRemoveFromBeginning As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the count to reduce to
            '#2flag to remove members from the beginning of the collection rather than from the end
            '^reduces the current collection to the passed count
            _rVal = New colDXFVectors With {.MaintainIndices = _MaintainIndices}
            If aCount >= Count Then Return _rVal
            If Not bRemoveFromBeginning Then
                Do Until Count <= aCount
                    _rVal.Add(Item(Count))
                    MyBase.Item(Count - 1).ReleaseCollectionReference()
                    MyBase.Remove(MyBase.Item(Count - 1))
                Loop
            Else
                Do Until Count <= aCount
                    _rVal.Add(Item(1))
                    MyBase.Item(0).ReleaseCollectionReference()
                    MyBase.Remove(MyBase.Item(0))
                Loop
            End If
            If MaintainIndices Then ReIndex()
            If Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, True)
            Return _rVal
        End Function

        Public Shadows Function Remove(aIndex As Integer) As dxfVector

            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            '#1the item number to remove
            '^removes the object from the collection at the indicated index
            Dim _rVal As dxfVector = Item(aIndex)
            Dim bReIndex As Boolean
            Dim bUndo As Boolean
            bReIndex = aIndex < Count And MaintainIndices
            RaiseChangeEvent(dxxCollectionEventTypes.PreRemove, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, bUndo, False, False)
            If bUndo Then Return Nothing
            _rVal.ReleaseCollectionReference()
            MyBase.Remove(_rVal)
            If bReIndex Then ReIndex()
            If Not _SuppressEvents Then
                RaiseChangeEvent(dxxCollectionEventTypes.Remove, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, True)
            End If
            Return _rVal
        End Function

        Public Shadows Function Remove(aVector As dxfVector) As dxfVector

            If aVector Is Nothing Then Return Nothing
            '#1the member to remove
            '^removes the object from the collection at the indicated index
            Dim idx As Integer = MyBase.IndexOf(aVector)
            If idx < 0 Then Return Nothing
            Return Remove(idx + 1)
        End Function

        Public Function RemoveAtCoordinate(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional aCS As dxfPlane = Nothing, Optional aPrecis As Integer = 3, Optional bReturnClones As Boolean = False, Optional bRemove As Boolean = False) As colDXFVectors
            '#1the X coordinate to match
            '#2the Y coordinate to match
            '#3the Z coordinate to match
            '#4an optional coordinate system to use
            '#5a precision for the comparison (1 to 8)
            '^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
            '~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
            '~say ony an X value is passed, then all the vectors with the same X ordinate are returned regarless of their
            '~respective Y and Z ordinate values.
            Return GetAtCoordinate(aX, aY, aZ, aCS, aPrecis, False, bRemove:=True)
        End Function

        Public Function RemoveAtEqualOrdinate(Optional aOrdinate As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X, Optional Precision As Integer = 4) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            Precision = TVALUES.LimitedValue(Precision, 0, 6)
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            Dim i As Integer
            Dim j As Integer
            Dim Ord1 As Double
            Dim Ord2 As Double
            For i = 1 To Count
                v1 = MyBase.Item(i - 1)
                If _rVal.IndexOf(v1) < 0 Then
                    Ord1 = v1.Ordinate(aOrdinate)
                    For j = i + 1 To Count
                        v2 = MyBase.Item(j - 1)
                        If _rVal.IndexOf(v2) < 0 Then
                            Ord2 = v2.Ordinate(aOrdinate)
                            If Math.Round(Ord1 - Ord2, Precision) = 0 Then _rVal.Add(v2)
                        End If
SkipPoint:
                    Next j
                End If
            Next i
            Dim removed As New List(Of dxfVector)
            RemoveMembers(_rVal, Nothing, rRemoved:=removed)
            Return removed
        End Function

        Public Function RemoveCoincidentVectors(Optional aPrecis As Integer = 4, Optional aStartID As Integer = 1, Optional aEndID As Integer = 0, Optional bReverseSerch As Boolean = False) As List(Of dxfVector)
            '^removes and returns the vectors that occur more than once

            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 10)
            Dim cnt As Integer = Count
            If cnt <= 1 Then Return New List(Of dxfVector)

            Dim v1 As dxfVector
            Dim v2 As dxfVector
            Dim remove As New List(Of dxfVector)
            Dim si As Integer
            Dim ei As Integer
            Dim stp As Integer
            Dim f1 As Integer = 1
            Dim d1 As Double = 0
            If bReverseSerch Then f1 = -1
            If dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei, bReverseSerch, stp) Then
                For j As Integer = si To ei Step stp
                    v1 = MyBase.Item(j - 1)
                    For k As Integer = j + f1 To ei Step stp
                        v2 = MyBase.Item(k - 1)
                        If j = si Then v2.Mark = False
                        If Not v2.Mark Then
                            d1 = Math.Round((v1.Strukture - v2.Strukture).Magnitude, aPrecis)
                            If d1 = 0 Then
                                remove.Add(v2)
                                v2.Mark = True
                            End If
                        End If
                    Next
                Next
            End If

            If remove.Count <= 0 Then Return remove
            Dim removed As New List(Of dxfVector)
            RemoveMembers(remove, bSuppressEvnts:=True, removed)
            If Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, removed, Reflection.MethodBase.GetCurrentMethod.Name, cnt, Count, False, False, True)
            Return removed
        End Function

        Public Function RemoveLast() As dxfVector
            '^removes the last object from the collection
            If Count > 0 Then Return Remove(Count) Else Return Nothing
        End Function

        Public Function RemoveMember(aVector As dxfVector) As dxfVector

            '^removes the object from the collection if it is currently a member of the collection
            '#1the member to remove
            '#2an optional collection of vectors to remove the passed vector from
            Dim i As Integer = MyBase.IndexOf(aVector)
            If i < 0 Then Return Nothing
            Dim bBail As Boolean = False
            RaiseChangeEvent(dxxCollectionEventTypes.PreRemove, aVector, Reflection.MethodBase.GetCurrentMethod.Name, "", "", bBail, False, False)
            If bBail Then Return Nothing
            aVector.ReleaseCollectionReference()
            If Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.Remove, aVector, Reflection.MethodBase.GetCurrentMethod.Name, Count + 1, Count, False, False, True)
            Return aVector
        End Function


        Public Function RemoveMembers(vCol As IEnumerable(Of dxfVector)) As Integer
            Return RemoveMembers(vCol, _SuppressEvents)
        End Function


        Friend Function RemoveMembers(vCol As IEnumerable(Of dxfVector), bSuppressEvnts As Boolean?) As Integer
            Dim rRemoved As List(Of dxfVector) = Nothing
            Return RemoveMembers(vCol, bSuppressEvnts, rRemoved)
        End Function
        Friend Function RemoveMembers(vCol As IEnumerable(Of dxfVector), bSuppressEvnts As Boolean?, ByRef rRemoved As List(Of dxfVector)) As Integer
            rRemoved = New List(Of dxfVector)
            If vCol Is Nothing Or Count <= 0 Then Return 0
            If Not bSuppressEvnts.HasValue Then bSuppressEvnts = True
            If _SuppressEvents Then bSuppressEvnts = True
            '^removes the passed vectors from this collection if they are actually members of it
            '#1the members to remove in a VB collection
            '#2the members to remove in a vector collection

            Dim bBail As Boolean

            For Each member As dxfVector In vCol

                Dim idx As Integer = MyBase.IndexOf(member)
                If idx >= 0 Then
                    RaiseChangeEvent(dxxCollectionEventTypes.PreRemove, member, Reflection.MethodBase.GetCurrentMethod.Name, "", "", bBail, False, False)
                    If Not bBail Then
                        MyBase.RemoveAt(idx)
                        rRemoved.Add(member)
                        member.ReleaseCollectionReference()
                    End If
                End If
            Next
            If rRemoved.Count > 0 And MaintainIndices Then ReIndex()
            If rRemoved.Count = 0 Then Return 0
            If Not bSuppressEvnts.Value Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, rRemoved, Reflection.MethodBase.GetCurrentMethod.Name, Count + rRemoved.Count, Count, False, False, True)
            Return rRemoved.Count
        End Function
        Public Function Rescale(aScaleX As Double, aRefVectorXY As iVector, Optional aScaleY As Double? = Nothing, Optional aScaleZ As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the scale factor to apply
            '#2the center to scale with resect to
            '#3the y scale to apply
            '#3the z scale to apply

            '^moves the current coordinates of the vectors in the collection to a vector scaled with respect to the passed center
            Return Transform(TTRANSFORM.CreateScale(New TVECTOR(aRefVectorXY), aScaleX, aScaleY, aScaleZ, aPlane), aEventName:="Rescale")
        End Function
        Friend Function ResetToLast() As Boolean
            Dim _rVal As Boolean
            '^sets the members back to their last coordinates

            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                If v1.SetStructure(v1.LastPositionV) Then _rVal = True
            Next i
            Return _rVal
        End Function
        Public Function ResetVectors() As Boolean

            '^sets the members back to their last coordinates
            Return ResetVectors(False)

        End Function
        Friend Function ResetVectors(Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '^sets the members back to their last coordinates

            Dim movers As New List(Of dxfVector)
            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                If v1.ResetVectorV(True) Then
                    movers.Add(v1)
                    _rVal = True
                End If
            Next i
            If movers.Count > 0 And Not _SuppressEvents And Not bSuppressEvnts Then
                Dim bUndo As Boolean
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, bUndo, False, False)
                If bUndo Then
                    _rVal = False
                    ResetToLast()
                    RaiseChangeEvent(dxxCollectionEventTypes.CollectionMoveUndo, movers, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, False)
                End If
            End If
            Return _rVal
        End Function
        Public Function ReverseOrder() As colDXFVectors

            Dim _rVal As colDXFVectors
            '^returns a new collection of Vectors in the reverse order as the current collection
            Dim v1 As dxfVector
            Dim v2 As dxfVector
            Dim i As Integer
            _rVal = New colDXFVectors
            For i = Count To 1 Step -1
                _rVal.Add(Item(i, True))
            Next i
            For i = 1 To Count
                v1 = Item(i)
                v2 = _rVal.Item(i)
                v2.VertexStyle = v1.VertexStyle
            Next i
            Return _rVal
        End Function

        ''' <summary>
        ''' rotates the members about an axis starting at the passed point and aligned with the Z axis (or X or Y) of the passed plane.
        ''' </summary>
        ''' <remarks>if the passed point is null the members is rotated about the origin of the passed coordinated system</remarks>
        ''' <param name="aPoint">the point to rotate about</param>
        ''' <param name="aAngle">the angle to rotate</param>
        ''' <param name="bInRadians">flag indicating the passed angle is in radians</param>
        ''' <param name="aPlane">the plane which is used to obtaint the axis of rotation. If null, the world plane is assumed.</param>
        ''' <param name="aAxisDescriptor">an optional axis descriptor to selected a planes primary axis to use other than the Z axis.</param>
        ''' <returns></returns>
        Public Function RotateAbout(aPoint As iVector, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z, Optional bAddRotationToMembers As Boolean = False) As Boolean

            If Count = 0 Then Return False
            If dxfPlane.IsNull(aPlane) Then aPlane = dxfPlane.World
            If aPoint Is Nothing Then aPoint = aPlane.Origin.Clone
            Return Transform(TTRANSFORM.CreateRotation(aPoint, aPlane, aAngle, bInRadians, Nothing, SuppressEvents, aAxisDescriptor, bAddRotationToMembers), aEventName:="RotateAbout-Point")
        End Function

        ''' <summary>
        ''' rotates the members about the an axis the requested  starting at the passed point and aligned with the Z axis (or X or Y) of the passed plane.
        ''' </summary>
        ''' <remarks>if the passed point is null the members is rotated about the origin of the passed coordinated system</remarks>
        ''' <param name="aPlane">the plane which is used to obtaint the axis of rotation. If null, the world plane is assumed.</param>
        ''' <param name="aAxisDescriptor">an axis descriptor to selected a planes primary axis to use .</param>
        ''' <param name="aAngle">the angle to rotate</param>
        ''' <param name="bInRadians">flag indicating the passed angle is in radians</param>
        ''' <param name="aPoint">the point to rotate about</param>
        ''' <returns></returns>
        Public Function RotateAbout(aPlane As dxfPlane, aAxisDescriptor As dxxAxisDescriptors, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPoint As iVector = Nothing, Optional bAddRotationToMembers As Boolean = False) As Boolean

            If Count = 0 Then Return False
            If dxfPlane.IsNull(aPlane) Then aPlane = dxfPlane.World
            If aPoint Is Nothing Then aPoint = aPlane.Origin.Clone
            Return Transform(TTRANSFORM.CreateRotation(aPoint, aPlane, aAngle, bInRadians, Nothing, True, aAxisDescriptor, bAddRotationToMembers), aEventName:="RotateAbout-Plane")
        End Function

        ''' <summary>
        ''' rotates the members about the passed axis the requested angle
        ''' </summary>
        ''' <remarks>if the passed line is nothing no action is taken</remarks>
        ''' <param name="aAxis">the line to rotate about</param>
        ''' <param name="aAngle">the angle to rotate</param>
        ''' <param name="bInRadians">flag indicating the passed angle is in radians</param>
        ''' <param name="aPlane">the plane which is used to obtaint the axis of rotation. If null, the world plane is assumed.</param>
        ''' <returns></returns>
        Public Function RotateAbout(aAxis As iLine, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional bAddRotationToMembers As Boolean = False) As Boolean
            If aAxis Is Nothing Or Count = 0 Then Return False

            Return Transform(TTRANSFORM.CreateRotation(aAxis, aPlane, aAngle, bInRadians, Nothing, SuppressEvents, bAddRotationToMembers), aEventName:="RotateAbout-Line")
        End Function


        Public Function RoundOrdinates(aDecimalPlaces As Integer, Optional bReturnClones As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            If bReturnClones Then _rVal = New colDXFVectors Else _rVal = Nothing
            aDecimalPlaces = TVALUES.LimitedValue(aDecimalPlaces, 0, 12)
            '#1the number of decimal places to  round the ordinates to
            '#2flag to return a collection of rouned vectors and leave the mebers of this collection unchanged
            '^rounds each vectors coordinates to the passed number of decimal places

            For i As Integer = 1 To Count
                Dim pt As dxfVector = Item(i)
                If bReturnClones Then
                    Dim P1 As dxfVector = pt.Clone
                    P1.X = Math.Round(P1.X, aDecimalPlaces)
                    P1.Y = Math.Round(P1.Y, aDecimalPlaces)
                    P1.Z = Math.Round(P1.Z, aDecimalPlaces)
                    _rVal.Add(P1)
                Else
                    pt.X = Math.Round(pt.X, aDecimalPlaces)
                    pt.Y = Math.Round(pt.Y, aDecimalPlaces)
                    pt.Z = Math.Round(pt.Z, aDecimalPlaces)
                End If
            Next i
            Return _rVal
        End Function
        Public Function SetCoordinates(Optional aX As Double? = Nothing, Optional aY As Double? = Nothing, Optional aZ As Double? = Nothing, Optional bReturnChangers As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1the X coordinate to set
            '#2the Y coordinate to set
            '#3the Z coordinate to set
            '^sets the member ordiates to the passed values
            '~unpassed values or non-numeric values are ignored.
            Dim DoX As Boolean = aX.HasValue
            Dim doY As Boolean = aY.HasValue
            Dim doZ As Boolean = aZ.HasValue

            If Not DoX And Not doY And Not doZ Then
                Return _rVal
            End If


            Dim xx As Double
            Dim yy As Double
            Dim zz As Double
            If bReturnChangers Then
                _rVal = New colDXFVectors With {.MaintainIndices = False}
            Else
                _rVal = Nothing
            End If
            If DoX Then xx = aX.Value
            If doY Then yy = aY.Value
            If doZ Then zz = aZ.Value
            Dim movers As New List(Of dxfVector)
            For i As Integer = 1 To Count
                Dim aPt As dxfVector = Item(i)
                Dim v1 As New TVECTOR(aPt)
                If DoX Then v1.X = xx
                If doY Then v1.Y = yy
                If doZ Then v1.Z = zz
                Dim bKeep As Boolean = aPt.SetStructure(v1)  'a change was made
                If bKeep Then movers.Add(aPt)
                If bReturnChangers And bKeep Then
                    _rVal.Add(aPt)
                End If
            Next i
            If movers.Count > 0 And Not _SuppressEvents Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, False)
            End If
            Return _rVal
        End Function
        Public Function SetDisplayVariable(aPropertyType As dxxDisplayProperties, aNewValue As Object, Optional aSearchList As String = "", Optional aStartID As Integer = 1, Optional aEndID As Integer = 0, Optional aTagList As String = "", Optional aFlagList As String = "") As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1the name of the display variable to affect (LayerName, Color, Linetype etc.)
            '#2the new value for the  display variable
            '#3a list of values to consider a match
            '^sets the members indicated display variable to the new value
            '~if a seach value is passed then only members with a current value equal to the search value will be affected.
            '~returns the affected members.
            If Count <= 0 Or aPropertyType = dxxDisplayProperties.DimStyle Or aPropertyType = dxxDisplayProperties.IsDirty Or aPropertyType = dxxDisplayProperties.LineWeight Or aPropertyType = dxxDisplayProperties.Undefined Then Return _rVal
            Dim aVariableName As String = dxfEnums.Description(aPropertyType).ToUpper()
            If aVariableName = "" Then Return _rVal
            Dim v1 As dxfVector
            Dim i As Integer
            Dim si As Integer
            Dim ei As Integer
            Dim srchList As New TLIST(",", aSearchList)
            Dim bT As Boolean = Not String.IsNullOrWhiteSpace(aTagList)
            Dim bF As Boolean = Not String.IsNullOrWhiteSpace(aFlagList)
            Dim bDoIt As Boolean = False
            Dim TorF As Boolean = bT Or bF
            Dim tg As String = String.Empty
            Dim fg As String = String.Empty
            If Not dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei) Then Return _rVal
            If aPropertyType = dxxDisplayProperties.Color Then
                If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
            ElseIf aPropertyType = dxxDisplayProperties.LTScale Then
                If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
            ElseIf aPropertyType = dxxDisplayProperties.Suppressed Then
                If Not TVALUES.IsBoolean(aNewValue) Then Return _rVal
            End If
            For i = si To ei
                If i > Count Or i <= 0 Then Exit For
                v1 = Item(i)
                bDoIt = True
                If TorF Then
                    tg = v1.Tag
                    fg = v1.Flag
                End If
                If bDoIt And bT Then
                    bDoIt = TLISTS.Contains(tg, aTagList)
                End If
                If bDoIt And bF Then
                    bDoIt = TLISTS.Contains(fg, aFlagList)
                End If
                If bDoIt Then
                    If v1.SetDisplayProperty(aPropertyType, aNewValue, srchList) Then _rVal.Add(v1)
                End If
            Next i
            Return _rVal
        End Function
        Public Sub SetLayerColorLinetype(aLayer As String, aColor As dxxColors, aLineType As String, Optional aLTScale As Double = 0.0)
            '^used to set the aLayer of all the members in one call
            Dim aMem As dxfVector
            Dim i As Integer
            aLayer = Trim(aLayer)
            aLineType = Trim(aLineType)
            For i = 1 To Count
                aMem = Item(i)
                aMem.LCLSet(aLayer, aColor, aLineType)
                If aLTScale > 0 Then aMem.LTScale = aLTScale
            Next i
        End Sub

        ''' <summary>
        ''' sets the indicated ordinate of the vectors to the passed value
        ''' </summary>
        ''' <param name="aOrdinateType">the target ordinate X, Y or Z</param>
        ''' <param name="aOrdinateValue">the value to assign to the vectors X, Y or Z ordinate</param>
        ''' <param name="aMatchOrdinate">if passed only the members with ordiantes that currently match the match ordinate are affected</param>
        ''' <param name="aPrecis">the precis to use to comare the match ordinates</param>
        ''' <returns></returns>
        Public Function SetOrdinates(aOrdinateValue As Double, aOrdinateType As dxxOrdinateDescriptors, Optional aMatchOrdinate As Double? = Nothing, Optional aPrecis As Integer? = Nothing) As List(Of dxfVector)

            '#1the value to assign to the members
            '#2the type of the ordinate to assign (X, Y or Z)
            '^sets all the members indicated ordinate to the passed value
            Dim movers As New List(Of dxfVector)

            Dim changers As List(Of iVector) = dxfVectors.SetMemberOrdinates(Me, aOrdinateType, aOrdinateValue, aMatchOrdinate, aPrecis)

            If changers.Count <= 0 Then Return movers

            For Each imem As iVector In changers
                movers.Add(DirectCast(imem, dxfVector))
            Next
            Dim bUndo As Boolean
            RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, bUndo, False, False)
            If bUndo Then

                ResetToLast()
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMoveUndo, movers, Reflection.MethodBase.GetCurrentMethod.Name, 0, 0, False, False, False)
                Return New List(Of dxfVector)
            End If
            Return movers
        End Function
        Public Sub SetPropertyValues(aProperty As dxxVectorProperties, aPropertyValue As Object, Optional aStartID As Integer = -1, Optional aEndID As Integer = -1)
            If aProperty < 1 Or aProperty > dxxVectorProperties.Suppressed Then Return
            Dim i As Integer
            Dim aMem As dxfVector
            Dim si As Integer
            Dim ei As Integer
            If Not dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei) Then Return
            For i = si To ei
                If i > 0 And i <= Count Then
                    aMem = Item(i)
                    Select Case aProperty
                        Case dxxVectorProperties.Inverted
                            aMem.Inverted = TVALUES.ToBoolean(aPropertyValue)
                        Case dxxVectorProperties.EndWidth
                            aMem.EndWidth = TVALUES.To_DBL(aPropertyValue, aDefault:=aMem.EndWidth)
                        Case dxxVectorProperties.StartWidth
                            aMem.StartWidth = TVALUES.To_DBL(aPropertyValue, aDefault:=aMem.StartWidth)
                        Case dxxVectorProperties.Flag
                            aMem.Flag = aPropertyValue.ToString()
                        Case dxxVectorProperties.Mark
                            aMem.Mark = TVALUES.ToBoolean(aPropertyValue)
                        Case dxxVectorProperties.Radius
                            aMem.Radius = TVALUES.To_DBL(aPropertyValue, aDefault:=aMem.Radius)
                        Case dxxVectorProperties.Tag
                            aMem.Tag = aPropertyValue.ToString()
                        Case dxxVectorProperties.Value
                            aMem.Value = aPropertyValue
                        Case dxxVectorProperties.Rotation
                            aMem.Rotation = TVALUES.To_DBL(aPropertyValue, aDefault:=aMem.Rotation)
                        Case dxxVectorProperties.Color
                            aMem.Color = TVALUES.To_INT(aPropertyValue, aMem.Color)
                        Case dxxVectorProperties.LayerName
                            aMem.LayerName = aPropertyValue.ToString()
                        Case dxxVectorProperties.Linetype
                            aMem.Linetype = aPropertyValue.ToString()
                        Case dxxVectorProperties.LTScale
                            aMem.LTScale = TVALUES.To_DBL(aPropertyValue, bAbsVal:=True, aDefault:=aMem.LTScale)
                        Case dxxVectorProperties.Suppressed
                            aMem.Suppressed = TVALUES.ToBoolean(aPropertyValue)
                        Case dxxVectorProperties.Row
                            aMem.Row = TVALUES.To_INT(aPropertyValue, aMem.Row)
                        Case dxxVectorProperties.Col
                            aMem.Col = TVALUES.To_INT(aPropertyValue, aMem.Col)
                        Case dxxVectorProperties.X
                            aMem.X = TVALUES.To_DBL(aPropertyValue, aDefault:=aMem.X)
                        Case dxxVectorProperties.Y
                            aMem.Y = TVALUES.To_DBL(aPropertyValue, aDefault:=aMem.Y)
                        Case dxxVectorProperties.Z
                            aMem.Z = TVALUES.To_DBL(aPropertyValue, aDefault:=aMem.Z)
                    End Select
                End If
            Next i
        End Sub
        Public Sub SetRowsAndColumns(aPlane As dxfPlane, aColStep As Double, aRowStep As Double, Optional bTriangular As Boolean = False, Optional aMaxCol As Integer = 0, Optional aMaxRow As Integer = 0)
            '#1the plane to use for the cardinal directions
            '#2the distance between columns
            '#3the distance between Rows
            '^assigns row and column numbers to the members based on the passed criteria
            SetRowsAndColumnsV(New TPLANE(aPlane), aColStep, aRowStep, bTriangular, aMaxCol, aMaxRow)
        End Sub
        Friend Sub SetRowsAndColumnsV(aPlane As TPLANE, aColStep As Double, aRowStep As Double, Optional bTriangular As Boolean = False, Optional aMaxCol As Integer = 0, Optional aMaxRow As Integer = 0)
            '#1the plane to use for the cardinal directions
            '#2the distance between columns
            '#3the distance between Rows
            '^assigns row and column numbers to the members based on the passed criteria
            aColStep = Math.Abs(aColStep)
            aRowStep = Math.Abs(aRowStep)
            aMaxCol = 0
            aMaxRow = 0
            If aColStep + aRowStep = 0 Then Return
            If bTriangular Then aColStep /= 2
            If Count <= 0 Then
                Return
            ElseIf Count = 1 Then
                Item(1).Row = 1
                Item(1).Col = 1
                aMaxCol = 1
                aMaxRow = 1
                Return
            End If
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim P1 As dxfVector
            Dim XYs(0 To Count - 1, 0 To 1) As Double
            Dim minX As Double
            Dim MaxY As Double
            Dim dX As Double
            Dim dY As Double
            'get the XYs of the members with respect to the plane
            minX = 3.0E+33
            MaxY = -3.0E+33
            For i = 1 To Count
                P1 = Item(i)
                v1 = P1.Strukture.WithRespectTo(aPlane)
                XYs(i - 1, 0) = v1.X
                XYs(i - 1, 1) = v1.Y
                If v1.X < minX Then minX = v1.X
                If v1.Y > MaxY Then MaxY = v1.Y
            Next i
            For i = 1 To Count
                P1 = Item(i)
                dX = Math.Abs(XYs(i - 1, 0) - minX)
                dY = Math.Abs(MaxY - XYs(i - 1, 1))
                If aColStep <> 0 Then
                    dX /= aColStep
                    P1.Col = TVALUES.To_INT(Math.Round(dX, 0)) + 1
                    If P1.Col > aMaxCol Then aMaxCol = P1.Col
                End If
                If aRowStep <> 0 Then
                    dY /= aRowStep
                    P1.Row = TVALUES.To_INT(Math.Round(dY, 0)) + 1
                    If P1.Row > aMaxRow Then aMaxRow = P1.Row
                End If
            Next i
        End Sub
        Public Function SetSegmentWidth(aSegWidth As Double) As Boolean
            Dim _rVal As Boolean
            '^sets the segment width property of all the members
            If aSegWidth < 0 Then Return _rVal
            Dim i As Integer
            Dim aMem As dxfVector
            For i = 1 To Count
                aMem = Item(i)
                If aMem.StartWidth <> aSegWidth Or aMem.EndWidth <> aSegWidth Then _rVal = True
                aMem.StartWidth = aSegWidth
                aMem.EndWidth = aSegWidth
            Next i
            Return _rVal
        End Function
        Public Sub SetTagsAndFlags(Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aSearchTag As String = Nothing, Optional aSearchFlag As String = Nothing, Optional aStartID As Integer = 1, Optional aEndID As Integer = -1, Optional bAddTagIndices As Boolean = False)
            '#1the new tag to assign to the members. null input is ignored.
            '#2the new flag to assign to the members. null input is ignored.
            '#3an existing tag to match
            '#4an existing flag to match
            '#5an optional starting index
            '#6an optional ending index
            '#7flag to append the index of the member to the passed tag value for each member
            '^used to set the tags and flags of the members in one call
            If Count <= 0 Then Return
            Dim bTags As Boolean = aTag IsNot Nothing
            Dim bFlags As Boolean = aFlag IsNot Nothing
            If Not bTags And Not bFlags Then Return
            Dim aStr As String = String.Empty
            If bTags Then aStr = aTag.ToString
            Dim bStr As String = String.Empty
            If bFlags Then bStr = bFlags.ToString
            Dim aMem As dxfVector
            Dim i As Integer
            Dim si As Integer
            Dim ei As Integer
            Dim bTestTag As Boolean = aSearchTag IsNot Nothing
            Dim bTestFlag As Boolean = aSearchFlag IsNot Nothing
            Dim aTg As String = String.Empty
            Dim aFg As String = String.Empty
            Dim bDoIt As Boolean
            Dim tlist As New List(Of String)
            Dim flist As New List(Of String)
            If bTestTag Then
                aTg = aSearchTag.ToString.Trim
                bTestTag = Not String.IsNullOrWhiteSpace(aTg)
            End If
            If bTestFlag Then
                aFg = aSearchFlag.ToString.Trim
                bTestFlag = Not String.IsNullOrWhiteSpace(aFg)
            End If
            dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei)
            For i = si To ei
                aMem = Item(i)
                If aMem IsNot Nothing Then
                    bDoIt = True
                    If bTestTag Or bTestFlag Then
                        If bTestTag Then
                            If Not TLISTS.Contains(aMem.Tag, aTg) Then bDoIt = False
                        End If
                        If bTestFlag Then
                            If Not TLISTS.Contains(aMem.Flag, aFg) Then bDoIt = False
                        End If
                    End If
                    If bDoIt Then
                        If bTags Then
                            If bAddTagIndices Then aMem.Tag = aStr & i Else aMem.Tag = aStr
                        End If
                        If bFlags Then aMem.Flag = bStr
                    End If
                End If
            Next i
        End Sub
        ''' <summary>
        ''' Sorts the vectors based on the type of sort order request
        ''' </summary>
        ''' <param name="aOrder">the requested order</param>
        ''' <param name="aReferencePt">a reference point to use if the order is nearest or farthest</param>
        ''' <param name="aPlane">a plane to apply</param>
        ''' <param name="rIndexes">returns the </param>
        ''' <param name="aPrecis"></param>
        ''' <returns></returns>
        Public Overloads Function Sort(aOrder As dxxSortOrders, aReferencePt As iVector, aPlane As dxfPlane, ByRef rIndexes As List(Of Integer), Optional aPrecis As Integer = 3) As Boolean
            Dim v1 As New TVECTOR(aReferencePt)
            Dim aPl As New TPLANE(aPlane)
            Dim _rVal As Boolean = Sort(aOrder, v1, aPl, True, rIndexes, aPrecis)
            If Not _SuppressEvents And _rVal Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, New List(Of dxfVector), Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, False)
            End If
            Return _rVal
        End Function

        Public Overloads Function Sort(aOrder As dxxSortOrders, Optional aReferencePt As iVector = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3) As Boolean
            Dim rIndexes As List(Of Integer) = Nothing
            Return Sort(aOrder, aReferencePt, aPlane, rIndexes, aPrecis)
        End Function

        ''' <summary>
        ''' sorts the vectors in the collection in the requested order
        ''' </summary>
        ''' <param name="aCenter"> an alternative  center to use for the operation. If not passed the plane origin is used</param>
        ''' <param name="aBaseAngle"> the angle to cosider as the base</param>
        ''' <param name="bReverseSort">if true, the return is sorted counter clockwise rather than clockwise</param>
        ''' <param name="aPlane" >the plane to use</param>
        ''' <param name="aPrecis" >the precision to apply for comparisons</param>
        ''' <returns></returns>
        Public Function SortClockWise(Optional aCenter As iVector = Nothing, Optional aBaseAngle As Double = 0, Optional bReverseSort As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aPrecis As Integer = 3) As Boolean
            Dim _rVal As Boolean = False

            If Count <= 1 Then Return False
            Dim bRID As Boolean = _MaintainIndices
            Dim movers As New List(Of dxfVector)

            Dim sorted As List(Of iVector) = dxfVectors.SortClockWise(Me, aCenter, aBaseAngle, bReverseSort, aPlane, aPrecis)
            MyBase.Clear()

            For Each item As iVector In sorted
                MyBase.Add(DirectCast(item, dxfVector))
            Next

            _MaintainIndices = bRID
            If _rVal And MaintainIndices Then ReIndex()
            If Not _SuppressEvents And _rVal Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, False)
            End If
            Return _rVal
        End Function



        Public Function CounterClockwise(Optional aCenter As iVector = Nothing, Optional aBaseAngle As Double = 0, Optional bReverseSort As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aFollowers As colDXFVectors = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the center to sort with respect to  (if nothing is passed the plane origin is assumed)
            '#2the planar angle to treat as 0 (0 = the X direction of the plane)
            '#3flag to sort the vectors counter-clockwise rather than clock wise
            '#4the subject plane (if nothing is passed the world plane is assumed)
            '#4an optional collection to sort aINteger with the vectors
            '^sorts the members in a clockwise or counter-clockwise way with respect to the subject plane
            Dim aIndices As New List(Of Integer)
            Dim aPln As New TPLANE(aPlane)
            Dim v1 As TVECTOR
            Dim ang As Double = aBaseAngle
            If aCenter IsNot Nothing Then v1 = New TVECTOR(aCenter) Else v1 = aPln.Origin
            If aFollowers IsNot Nothing Then aIndices = New List(Of Integer)

            _rVal = SortClockWiseV(v1, ang, True, aPln, True, aIndices)
            If aFollowers IsNot Nothing Then
                Dim newCol As New List(Of dxfVector)

                Dim idx As Integer
                Dim mxidx As Integer
                For i As Integer = 1 To aIndices.Count
                    idx = aIndices.Item(i - 1)
                    If idx <= aFollowers.Count Then
                        If idx > mxidx Then mxidx = idx
                        newCol.Add(aFollowers.Item(idx))
                    End If
                Next i
                For i As Integer = mxidx + 1 To aFollowers.Count
                    newCol.Add(aFollowers.Item(i))
                Next i
                aFollowers.Populate(newCol)
            End If
            If Not _SuppressEvents And _rVal Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, New List(Of dxfVector), Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, False)
            End If
            Return _rVal
        End Function
        Public Function Clockwise(Optional aCenter As iVector = Nothing, Optional aBaseAngle As Double = 0, Optional bReverseSort As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aFollowers As colDXFVectors = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the center to sort with respect to  (if nothing is passed the plane origin is assumed)
            '#2the planar angle to treat as 0 (0 = the X direction of the plane)
            '#3flag to sort the vectors counter-clockwise rather than clock wise
            '#4the subject plane (if nothing is passed the world plane is assumed)
            '#4an optional collection to sort aINteger with the vectors
            '^sorts the members in a clockwise or counter-clockwise way with respect to the subject plane
            Dim aIndices As New List(Of Integer)
            Dim aPln As New TPLANE(aPlane)
            Dim v1 As TVECTOR
            Dim ang As Double = aBaseAngle
            If aCenter IsNot Nothing Then v1 = New TVECTOR(aCenter) Else v1 = aPln.Origin
            If aFollowers IsNot Nothing Then aIndices = New List(Of Integer)

            _rVal = SortClockWiseV(v1, ang, bReverseSort, aPln, True, aIndices)
            If aFollowers IsNot Nothing Then
                Dim newCol As New List(Of dxfVector)
                Dim i As Integer
                Dim idx As Integer
                Dim mxidx As Integer
                For i = 1 To aIndices.Count
                    idx = aIndices.Item(i - 1)
                    If idx <= aFollowers.Count Then
                        If idx > mxidx Then mxidx = idx
                        newCol.Add(aFollowers.Item(idx))
                    End If
                Next i
                For i = mxidx + 1 To aFollowers.Count
                    newCol.Add(aFollowers.Item(i))
                Next i
                aFollowers.Populate(newCol)
            End If
            If Not _SuppressEvents And _rVal Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, New List(Of dxfVector), Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, False)
            End If
            Return _rVal
        End Function
        Friend Function SortClockWiseV(aCenter As TVECTOR, aBaseAngle As Double, bReverseSort As Boolean, aPlane As TPLANE, Optional bSuppressEvnts As Boolean = True, Optional ByRef rIndices As List(Of Integer) = Nothing) As Boolean
            rIndices = New List(Of Integer)
            Dim _rVal As Boolean
            If Count <= 1 Then Return _rVal
            Dim v1 As TVECTOR
            Dim ang1 As Double
            Dim v0 As dxfVector
            'Dim arSort As New TVALUES(0)
            'Dim aIndices As New TVALUES(0)
            Dim aNewCol As New List(Of dxfVector)
            Dim j As Integer
            Dim idx As Integer
            Dim P1 As dxfVector
            Dim bPlane As TPLANE = aPlane.Clone
            Dim tpl As Tuple(Of Double, Integer)
            Dim srt As New List(Of Tuple(Of Double, Integer))
            'Dim angleToIndex As New Dictionary(Of Integer, Integer)
            bPlane.Origin = aCenter
            If aBaseAngle <> 0 Then bPlane.Revolve(aBaseAngle, False)
            For i As Integer = 1 To Count
                v0 = Item(i)
                v1 = v0.Strukture.WithRespectTo(bPlane)
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
                ang1 = Math.Round(ang1, 3)
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
                P1 = Item(idx)
                If idx <> j Then _rVal = True
                rIndices.Add(idx)
                aNewCol.Add(P1)
            Next
            MyBase.Clear()
            MyBase.AddRange(aNewCol)

            If MaintainIndices Then ReIndex()
            If Not bSuppressEvnts And Not _SuppressEvents And _rVal Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, New List(Of dxfVector), Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, False)
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' sorts the vectors in the collection in the requested order
        ''' </summary>
        ''' <param name="aOrder"> the order to apply</param>
        ''' <param name="aReferencePt" >the reference to use for relative orders"</param>
        ''' <param name="aPlane"> the plane to use</param>
        ''' <param name="bSuppressEvnts">prevents the collection from raising a change event if the order changes</param>
        ''' <param name="rIndexes"> returns the indices of the reordered members</param>
        ''' <param name="aPrecis" > the precision to apply for comparisons</param>
        ''' <returns></returns>
        Friend Overloads Function Sort(aOrder As dxxSortOrders, aReferencePt As TVECTOR, aPlane As TPLANE, bSuppressEvnts As Boolean, ByRef rIndexes As List(Of Integer), Optional aPrecis As Integer = 3) As Boolean
            Dim _rVal As Boolean = False
            rIndexes = New List(Of Integer)
            If Count <= 1 Then Return False
            Dim movers As New List(Of dxfVector)
            Dim sorted As List(Of iVector) = dxfVectors.Sort(Me, aOrder, New dxfVector(aReferencePt), New dxfPlane(aPlane), _rVal, rIndexes, aPrecis)

            For Each item As iVector In sorted
                Dim v1 As dxfVector = DirectCast(item, dxfVector)
                If IndexOf(v1) <> sorted.IndexOf(item) Then movers.Add(v1)
            Next


            MyBase.Clear()

            For Each item As iVector In sorted
                Dim v1 As dxfVector = DirectCast(item, dxfVector)
                If _MaintainIndices Then v1.Index = Count + 1
                MyBase.Add(v1)
            Next

            If Not bSuppressEvnts And Not _SuppressEvents And _rVal Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, movers, Reflection.MethodBase.GetCurrentMethod.Name, "", "", False, False, False)
            End If
            Return _rVal
        End Function
        Public Function Stretch(aStretch As Double, Optional aPlane As dxfPlane = Nothing, Optional aCenter As iVector = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the stetch to apply
            '#2the plane to use for the stretch direction (X Direction)
            '#3an optional center for the plane
            '^moves the members the stretch distance in the X Direction of the passed plane relative to the passed center
            '~if ithe plane is not passed the world plane is used
            '~if the center is not passed the center of the plane is assumed

            If aStretch = 0 Or Count <= 0 Then Return _rVal

            Dim i As Integer
            Dim aPln As New TPLANE(aPlane)
            Dim aMem As dxfVector
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim xDir As TVECTOR
            Dim f1 As Integer
            Dim f2 As Integer
            If aCenter IsNot Nothing Then aPln.Origin = New TVECTOR(aCenter)
            xDir = aPln.XDirection
            If aStretch < 0 Then f2 = -1 Else f2 = 1
            aStretch = Math.Abs(aStretch)
            For i = 1 To Count
                aMem = Item(i)
                v1 = aMem.Strukture
                v2 = v1.WithRespectTo(aPln)
                If v2.X <> 0 Then
                    If v2.X < 0 Then f1 = -1 * f2 Else f1 = 1 * f2
                    If aMem.Project(xDir, aStretch * f1, False) Then
                        _rVal = True
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function SubSet(Optional aStartID As Integer? = Nothing, Optional aEndID As Integer? = Nothing, Optional bRemoveSubset As Boolean = False, Optional bReturnClones As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            If Count <= 0 Then Return _rVal
            '#1the starting index
            '#2the ending index
            '#3flag to remove the subset from this set
            '^returns a section of the collection from the starting index to the ending index

            Dim si As Integer
            Dim ei As Integer
            Dim aMem As dxfVector
            Dim stp As Integer
            Dim sii As Integer = 1
            Dim eii As Integer = Count

            If aStartID.HasValue Then sii = aStartID.Value
            If aEndID.HasValue Then eii = aEndID.Value

            If bRemoveSubset Then bReturnClones = False
            If dxfUtils.LoopIndices(Count, sii, eii, si, ei, False, rStep:=stp) Then
                For i As Integer = si To ei Step stp
                    aMem = Item(i, bReturnClones)
                    _rVal.Add(aMem)
                Next
            End If

            If bRemoveSubset And Not bReturnClones Then
                If RemoveMembers(_rVal, bSuppressEvnts:=True) > 0 And Not _SuppressEvents Then RaiseChangeEvent(dxxCollectionEventTypes.RemoveSet, _rVal, Reflection.MethodBase.GetCurrentMethod.Name, Count + _rVal.Count, Count, False, False, True)
                If MaintainIndices Then ReIndex()
            End If
            Return _rVal
        End Function
        Public Function TagList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty

            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                TLISTS.Add(_rVal, aMem.Tag, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function TagString(Optional aDelimitor As String = ",", Optional bUniqueOnly As Boolean = True, Optional bIncludeFlags As Boolean = False, Optional aFlagDelimitor As String = ":") As String
            Dim _rVal As String = String.Empty
            '^returns a string containing the unique tags of the members
            Dim aTags As List(Of String) = Tags(bUniqueOnly, bIncludeFlags, aFlagDelimitor)
            For i As Integer = 1 To aTags.Count
                If Not String.IsNullOrWhiteSpace(aTags.Item(i)) Then
                    If _rVal <> String.Empty Then _rVal += aDelimitor
                    _rVal += aTags.Item(i)
                End If
            Next i
            Return _rVal
        End Function
        Public Function Tags(Optional bUniqueOnly As Boolean = False, Optional bIncludeFlags As Boolean = False, Optional aFlagDelimitor As String = ":") As List(Of String)
            Dim _rVal As New List(Of String)
            '#1flag to return only the unique tags
            '^returns a collection of strings containing the tags of the members
            Dim aMem As dxfVector
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

        Public Function TransferedToPlane(aFromPlane As dxfPlane, aToPlane As dxfPlane, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aRotation As Double? = Nothing, Optional bKeepOrigin As Boolean = False) As colDXFVectors

            Dim _rVal As New colDXFVectors(Me, bAddClones:=True)
            dxfVectors.TransferVectorsToPlane(_rVal, New TPLANE(aFromPlane), New TPLANE(aToPlane), aXScale, aYScale, aZScale, aRotation, bKeepOrigin)
            Return _rVal
        End Function


        ''' <summary>
        ''' transfers the vectors from one plane to another
        ''' </summary>
        ''' <remarks>
        '''  the coordinates of the vectors are transformed from the source plane to the target plane
        ''' </remarks>
        ''' <param name="aFromPlane">the plane to transfer from. this is assumed to be the world XY plane if null is passed. </param>
        ''' <param name="aToPlane">the plane to transfer to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aXScale">an optional X scale factor</param>
        ''' <param name="aYScale">an optional Y scale factor</param>
        ''' <param name="aZScale">an optional Z scale factor</param>
        ''' <param name="aRotation">an optional rotation angle in degrees</param>
        ''' <param name="bKeepOrigin">if true, the origin of the new plane is kept at the origin of the old plane</param>
        ''' <returns></returns>
        Public Function TransferToPlane(aFromPlane As dxfPlane, aToPlane As dxfPlane, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aRotation As Double? = Nothing, Optional bKeepOrigin As Boolean = False) As Integer
            Return dxfVectors.TransferVectorsToPlane(Me, New TPLANE(aFromPlane), New TPLANE(aToPlane), aXScale, aYScale, aZScale, aRotation, bKeepOrigin)
        End Function

        ''' <summary>
        ''' transfers the vectors from one plane to another
        ''' </summary>
        ''' <remarks>
        '''  the coordinates of the vectors are transformed from the source plane to the target plane
        ''' </remarks>
        ''' <param name="aFromPlane">the plane to transfer from. this is assumed to be the world XY plane if null is passed. </param>
        ''' <param name="aToPlane">the plane to transfer to. this is assumed to be the world XY plane if null is passed.</param>
        ''' <param name="aXScale">an optional X scale factor</param>
        ''' <param name="aYScale">an optional Y scale factor</param>
        ''' <param name="aZScale">an optional Z scale factor</param>
        ''' <param name="aRotation">an optional rotation angle in degrees</param>
        ''' <param name="bKeepOrigin">if true, the origin of the new plane is kept at the origin of the old plane</param>
        ''' <returns></returns>
        Friend Function TransferToPlane(aFromPlane As TPLANE, aToPlane As TPLANE, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aRotation As Double? = Nothing, Optional bKeepOrigin As Boolean = False) As Integer
            Return dxfVectors.TransferVectorsToPlane(Me, aFromPlane, aToPlane, aXScale, aYScale, aZScale, aRotation, bKeepOrigin)
        End Function

        Friend Function Transform(aTransforms As TTRANSFORMS, Optional bSuppressEvnts As Boolean = False, Optional aEventName As String = "Transforms", Optional aStartID As Integer = 0, Optional aEndID As Integer = 0, Optional bReverseOrder As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim bUndo As Boolean
            Dim si As Integer
            Dim ei As Integer
            Dim stp As Integer
            If Not dxfUtils.LoopIndices(Count, aStartID, aEndID, si, ei, bReverseOrder, stp) Then Return False
            For i As Integer = si To ei Step stp
                If Item(i).Transform(aTransforms, True) Then _rVal = True
            Next i
            If _rVal And Not bSuppressEvnts And Not _SuppressEvents Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, New List(Of dxfVector), Reflection.MethodBase.GetCurrentMethod.Name, "", "", bUndo, False, False)
            End If
            If bUndo Then
                For i As Integer = si To ei Step stp
                    Item(i).VertexV = Item(i).VertexLastV
                Next i
            End If
            Return _rVal
        End Function
        Friend Function Transform(aTransform As TTRANSFORM, Optional bSuppressEvnts As Boolean = False, Optional aEventName As String = "Transforms") As Boolean
            Dim _rVal As Boolean = TTRANSFORM.Apply(aTransform, Me, True)
            Dim bUndo As Boolean

            If _rVal And Not bSuppressEvnts And Not _SuppressEvents Then
                RaiseChangeEvent(dxxCollectionEventTypes.CollectionMove, New List(Of dxfVector), Reflection.MethodBase.GetCurrentMethod.Name, "", "", bUndo, False, False)
            End If
            If bUndo Then
                For i As Integer = 1 To Count
                    Item(i).VertexV = Item(i).VertexLastV
                Next i
            End If
            Return _rVal
        End Function
        Public Function Translate(aTranslation As iVector, Optional aPlane As dxfPlane = Nothing) As Boolean
            If aTranslation Is Nothing Then Return False
            Return Transform(TTRANSFORM.CreateTranslation(New TVECTOR(aTranslation), SuppressEvents, aPlane), aEventName:="Translate")
        End Function
        Public Function UniqueMembers(Optional aPrecis As Integer = 5, Optional bReturnClones As Boolean = False) As List(Of dxfVector)
            Dim _rVal As New List(Of dxfVector)
            '#1the precisons to use for comparison (0 to 8)
            '#2flag to return clones
            '#3a vb collection of vectors
            '^returns only the unique members of the collection
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)

            For i As Integer = 1 To Count
                Dim v1 As dxfVector = Item(i)
                v1.Index = i
                Dim bAddit As Boolean = True
                For j As Integer = 1 To _rVal.Count
                    Dim v2 As dxfVector = _rVal.Item(j)
                    If v1.IsEqual(v2, aPrecis) Then
                        bAddit = False
                        Exit For
                    End If
                Next j
                If bAddit Then
                    If bReturnClones Then _rVal.Add(New dxfVector(v1)) Else _rVal.Add(v1)
                End If
            Next i
            Return _rVal
        End Function
        Public Function ValueList(Optional aDelimitor As String = ",", Optional bReturnNulls As Boolean = False, Optional bUniqueList As Boolean = True) As String
            Dim _rVal As String = String.Empty

            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                TLISTS.Add(_rVal, aMem.Value, bAllowDuplicates:=Not bUniqueList, aDelimitor:=aDelimitor, bAllowNulls:=bReturnNulls)
            Next i
            Return _rVal
        End Function
        Public Function ValueStrings(Optional aPrecis As Integer = 4, Optional aDelimiter As String = "", Optional aValueSource As String = "VALUE") As String
            Dim _rVal As String = String.Empty
            '^returns the points X, Y and Value Properties in a comma delimited string

            If String.IsNullOrWhiteSpace(aDelimiter) Then aDelimiter = dxfGlobals.Delim
            aDelimiter = aDelimiter.Trim()

            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                If _rVal <> "" Then _rVal += aDelimiter
                _rVal += aMem.ValueString(aPrecis, aValueSource)
            Next i
            Return _rVal
        End Function
        Public Function Values(Optional bUniqueOnly As Boolean = False) As List(Of Object)
            Dim _rVal As New List(Of Object)
            '#1flag to return only the unique values
            '^returns a collection containing the values of the members


            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                Dim bKeep As Boolean = True
                If bUniqueOnly Then
                    For j As Integer = 1 To _rVal.Count
                        If _rVal.Item(j) = aMem.Value Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                End If
                If bKeep Then _rVal.Add(aMem.Value)
            Next i
            Return _rVal
        End Function
        Public Function VectorIsPresent(aVectorObj As iVector, ByRef rExistingIndex As Integer, Optional aPrecis As Integer = 4) As Boolean
            '#1the vector to match
            '#2returns the index of the matching vector if it exists
            '#3the precision to use for the comparison
            '^returns True if a vector with coordinates match those of the passed vector is in the collection
            rExistingIndex = 0
            If aVectorObj Is Nothing Then Return False
            Return dxfVectors.ContainsVector(Me, aVectorObj, rExistingIndex, aPrecis)
        End Function

        Public Function VertexCoordinatesGet(Optional aPlane As dxfPlane = Nothing, Optional aDelimiter As String = "¸", Optional bSuppressRads As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '^a concantonated string of all the vertex coordinates in the collection
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)

            Dim aPl As New TPLANE(aPlane)
            For i As Integer = 1 To Count
                Dim aMem As dxfVector = Item(i)
                If _rVal <> "" Then _rVal += aDelimiter
                _rVal += aMem.VertexCoordinatesV(aPl, bSuppressRads)
            Next i
            Return _rVal
        End Function
        Public Sub VertexCoordinatesSet(aVertexString As String, Optional aPlane As dxfPlane = Nothing, Optional bRetainCurrentMembers As Boolean = False, Optional aDelimiter As String = "¸", Optional aMaxAdd As Integer = 0)
            '^a concantonated string of all the vertex coordinates in the collection
            '~vertex coordinates are the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            '~the delimitor is "¸" (char 184)
            Dim dstr As String

            Dim NewPoint As dxfVector
            Dim pstrg As String
            Dim aPl As New TPLANE(aPlane)
            Dim cnt As Integer
            If Asc(aDelimiter) = 63 Then
                aDelimiter = dxfGlobals.Delim
            End If
            If Not String.IsNullOrWhiteSpace(aVertexString) Then dstr = aVertexString.Trim() Else dstr = ""
            If Not dstr.Contains(aDelimiter) Then aDelimiter = dxfGlobals.MultiDelim
            Dim pVals As TVALUES = TLISTS.ToValues(dstr, aDelimiter, False)
            If Not bRetainCurrentMembers Then MyBase.Clear()

            For i As Integer = 1 To pVals.Count
                pstrg = pVals.Item(i)
                cnt += 1
                If aMaxAdd > 0 And cnt > aMaxAdd Then Exit For
                NewPoint = New dxfVector()
                NewPoint.VertexCoordinatesSetV(pstrg, aPl)
                Add(NewPoint)
            Next i
        End Sub


        Public Function WithRespectToPlane(aPlane As dxfPlane, Optional aTransferPlane As dxfPlane = Nothing, Optional aTransferElevation As Double? = Nothing, Optional aTransferRotation As Double? = Nothing, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing) As colDXFVectors
            Return New colDXFVectors(dxfVectors.WithRespectToPlane(Me, aPlane:=aPlane, aTransferPlane:=aTransferPlane, aTransferElevation:=aTransferElevation, aTransferRotation:=aTransferRotation, aXScale:=aXScale, aYScale:=aYScale), bAddClones:=False)

        End Function
        Public Shadows Sub Reverse()
            If Count <= 1 Then Return
            MyBase.Reverse()
            If MaintainIndices Then ReIndex()
        End Sub
        Public Function Reversed() As colDXFVectors
            Dim _rVal As New colDXFVectors(Me, bAddClones:=True)
            _rVal.Reverse()
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            If Count > 0 And Count <= 6 Then
                Return CoordinatesR(3, ",")
            Else
                Return $"dxfVectors[{ Count}]"
            End If
        End Function
        Public Function X(aIndex As Integer) As Double
            If aIndex < 1 Or aIndex > Count Then Throw New IndexOutOfRangeException Else Return MyBase.Item(aIndex - 1).X
        End Function
        Public Function Y(aIndex As Integer) As Double
            If aIndex < 1 Or aIndex > Count Then Throw New IndexOutOfRangeException Else Return MyBase.Item(aIndex - 1).Y
        End Function
        Public Function Z(aIndex As Integer) As Double
            If aIndex < 1 Or aIndex > Count Then Throw New IndexOutOfRangeException Else Return MyBase.Item(aIndex - 1).Z
        End Function
        Public Function Rotation(aIndex As Integer) As Double
            If aIndex < 1 Or aIndex > Count Then Throw New IndexOutOfRangeException Else Return MyBase.Item(aIndex - 1).Rotation
        End Function
#End Region 'Methods
#Region "oEvents_EventHandlers"
        Private Sub _Events_VectorsRequest(aGUID As String, ByRef rVectors As colDXFVectors)
            If aGUID = _CollectionGUID Then rVectors = Me
            'If String.IsNullOrWhiteSpace(_CollectionGUID) Then
            '    If _Events IsNot Nothing Then RemoveHandler _Events.VectorsRequest, AddressOf _Events_VectorsRequest
            '    _Events = Nothing
            'End If
        End Sub
#End Region 'oEvents_EvewntHandlers
#Region "IDisposable Implementation"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    'If _Events IsNOt Nothing Then RemoveHandler _Events.VectorsRequest, AddressOf _Events_VectorsRequest
                    '_Events = Nothing
                    MonitorMembers = False
                    Clear()
                End If
                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub
        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            ''GC.SuppressFinalize(Me)
        End Sub
#End Region '"IDisposable Implementation
#Region "Shared Methods"
        Public Shared ReadOnly Property Zero As colDXFVectors
            Get
                Return New colDXFVectors()
            End Get
        End Property


#End Region 'Shared MeEthods
#Region "Operators"
        Public Shared Operator +(A As colDXFVectors, B As colDXFVectors) As colDXFVectors
            If A Is Nothing And B IsNot Nothing Then Return B.Clone()
            If B Is Nothing And A IsNot Nothing Then Return A.Clone()
            Dim _rVal As colDXFVectors = A.Clone()
            _rVal.Append(B, bAppendClones:=True)
            Return _rVal

        End Operator
        Public Shared Operator -(A As colDXFVectors, B As colDXFVectors) As colDXFVectors
            If A Is Nothing Then Return Nothing
            If B Is Nothing And A IsNot Nothing Then Return A.Clone()
            Dim _rVal As colDXFVectors = A.Clone()
            For Each v1 As dxfVector In B
                _rVal.GetByVector(v1, bRemove:=True)
            Next

            Return _rVal

        End Operator

#End Region 'Operators
    End Class 'colDXFVectors
End Namespace
