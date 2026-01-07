Imports System.Security.AccessControl
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfTable
        Inherits List(Of dxfTableEntry)
        Implements IEnumerable(Of dxfTableEntry)
        Implements iHandleOwner
        Implements IDisposable

#Region "Members"

        Friend _Handlez As THANDLES

#End Region 'Members
#Region "Constructors"

        Friend Sub New(aTableType As dxxReferenceTypes, Optional aMembers As IEnumerable(Of dxfTableEntry) = Nothing)
            _TableType = aTableType

            _Properties = dxpProperties.Properties_Table(TableType, GUID)
            _Handlez = New THANDLES(dxfEvents.NextTableGUID(_TableType))

            If aMembers Is Nothing Then Return
            For Each mem As dxfTableEntry In aMembers
                If mem Is Nothing Then Continue For
                If mem.EntryType <> TableType Then Continue For
                AddToCollection(mem.Clone())
            Next
        End Sub


#End Region 'Constructors


#Region "iHandleOwner"
        Public Property ReactorGUID As String Implements iHandleOwner.ReactorGUID
            Get
                Return _Handlez.ReactorGUID
            End Get
            Set(value As String)
                _Handlez.ReactorGUID = value
            End Set
        End Property

        Public Property BlockGUID As String Implements iHandleOwner.BlockGUID
            Get
                Return String.Empty
            End Get
            Set(value As String)
                value = String.Empty
            End Set
        End Property

        Public Property CollectionGUID As String Implements iHandleOwner.CollectionGUID
            Get
                Return String.Empty
            End Get
            Set(value As String)
                value = String.Empty
            End Set
        End Property

        Public Property BlockCollectionGUID As String Implements iHandleOwner.BlockCollectionGUID
            Get
                Return String.Empty
            End Get
            Set(value As String)
                value = String.Empty
            End Set
        End Property

        Public Property Handle As String Implements iHandleOwner.Handle
            Get
                Return _Handlez.Handle
            End Get
            Set(value As String)
                _Handlez.Handle = value
                Properties.Handle = _Handlez.Handle
                UpdateGUIDS()
            End Set
        End Property

        Public Property ImageGUID As String Implements iHandleOwner.ImageGUID
            Get
                Return _Handlez.ImageGUID
            End Get
            Set(value As String)
                If String.Compare(_Handlez.ImageGUID, value, True) <> 0 Then
                    _ImagePtr = Nothing
                    _Handlez.ImageGUID = value
                    UpdateGUIDS()

                End If
            End Set
        End Property

        Public Property Index As Integer Implements iHandleOwner.Index
            Get
                Return _Handlez.Index
            End Get
            Set(value As Integer)
                _Handlez.Index = value

            End Set
        End Property

        Public Property OwnerGUID As String Implements iHandleOwner.OwnerGUID
            Get
                Return _Handlez.OwnerGUID
            End Get
            Set(value As String)
                _Handlez.OwnerGUID = value
            End Set
        End Property

        Public Property SourceGUID As String Implements iHandleOwner.SourceGUID
            Get
                Return _Handlez.SourceGUID
            End Get
            Set(value As String)
                _Handlez.SourceGUID = value
            End Set
        End Property

        Public Property Domain As dxxDrawingDomains Implements iHandleOwner.Domain
            Get
                Return _Handlez.Domain
            End Get
            Set(value As dxxDrawingDomains)
                _Handlez.Domain = value
            End Set
        End Property

        Public Property Identifier As String Implements iHandleOwner.Identifier
            Get
                Return _Handlez.Identifier
            End Get
            Set(value As String)
                _Handlez.Identifier = value
            End Set
        End Property

        Public Property ObjectType As dxxFileObjectTypes Implements iHandleOwner.ObjectType
            Get
                Return FileObjectType
            End Get
            Set(value As dxxFileObjectTypes)
                value = FileObjectType
            End Set
        End Property

        Public Property OwnerType As dxxFileObjectTypes Implements iHandleOwner.OwnerType
            Get
                Return dxxFileObjectTypes.Undefined
            End Get
            Set(value As dxxFileObjectTypes)
                value = dxxFileObjectTypes.Undefined
            End Set
        End Property

        Public Property Name As String Implements iHandleOwner.Name
            Get
                Return TableTypeName()
            End Get
            Friend Set(value As String)
                value = TableTypeName()
            End Set
        End Property

        Public Property GUID As String Implements iHandleOwner.GUID
            Get
                Return _Handlez.GUID
            End Get
            Friend Set(value As String)
                _Handlez.GUID = value
                Properties.GUID = _Handlez.GUID
                UpdateGUIDS()
            End Set
        End Property

        Friend Property Suppressed As Boolean Implements iHandleOwner.Suppressed
            Get
                Return _Handlez.Suppressed
            End Get
            Set(value As Boolean)
                _Handlez.Suppressed = value
            End Set
        End Property

        Friend Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix Implements iHandleOwner.DXFFileProperties
            Throw New NotImplementedException
        End Function

        Friend Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As dxfPlane, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As dxoPropertyArray Implements iHandleOwner.DXFProps
            Throw New NotImplementedException
        End Function

        Public ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.Table
            End Get
        End Property
#End Region 'dxfHandleOwner
#Region "Properties"


        Private _Reactors As dxoPropertyArray
        Public Property Reactors As dxoPropertyArray
            Get
                Return _Reactors
            End Get
            Friend Set(value As dxoPropertyArray)
                _Reactors = value
            End Set
        End Property

        ''' <summary>
        ''' eturnd a copy of the currently defined properties of the object
        ''' </summary>
        ''' <remarks>changing these properties does not affect the object</remarks>
        ''' <returns></returns>
        Public ReadOnly Property ActiveProperties
            Get
                Return New dxoProperties(Properties)
            End Get
        End Property

        Private _Properties As dxoProperties
        Friend Property Properties As dxoProperties
            Get
                If _Properties Is Nothing Then
                    If TableType <> dxxReferenceTypes.UNDEFINED Then
                        Return dxpProperties.Properties_Table(TableType, GUID)
                    Else
                        _Properties = dxpProperties.Properties_Table(TableType, GUID)
                    End If

                End If
                _Properties.FileObjectType = dxxFileObjectTypes.Table
                Return _Properties
            End Get
            Set(value As dxoProperties)
                _Properties = value
            End Set
        End Property

        Friend ReadOnly Property Strukture As TTABLE
            Get
                Return New TTABLE(Me)

            End Get
        End Property

        Private _TableType As dxxReferenceTypes

        Public ReadOnly Property TableType As dxxReferenceTypes
            Get

                Return _TableType
            End Get
        End Property

        Friend WriteOnly Property UpdateEntry As dxfTableEntry
            Set(value As dxfTableEntry)
                SetEntry(value.Name, value)
            End Set
        End Property

        Public ReadOnly Property DefaultNames As List(Of String)
            Get
                Return dxfTable.DefaultMemberNames(TableType)
            End Get
        End Property

#End Region 'Properties
#Region "Methods"

        Public Function ContainsEntry(aNameOrHandleOrGUID As String)
            Dim entry As dxfTableEntry = Nothing
            Return TryGetEntry(aNameOrHandleOrGUID, entry)
        End Function

        Public Sub UpdateBitCodes()
            For Each aEntry As dxfTableEntry In Me
                Select Case TableType
            '=================================================================
                    Case dxxReferenceTypes.VPORT
            '=================================================================
            '=================================================================
                    Case dxxReferenceTypes.DIMSTYLE
            '=================================================================
            '=================================================================
                    Case dxxReferenceTypes.STYLE
            '=================================================================
            '=================================================================
                    Case dxxReferenceTypes.LAYER
            '=================================================================
            'update the bitcodes
            '=================================================================
                    Case dxxReferenceTypes.LTYPE
                        '=================================================================
                End Select
            Next
        End Sub

        Public Function Entry(aNameOrHandleOrGUID As String) As dxfTableEntry
            '#1the name or index of the member to return
            '^returns the object from the array that has a matching name or handle.
            Dim aEntry As dxfTableEntry = Nothing
            If TryGetEntry(aNameOrHandleOrGUID, aEntry) Then
                aEntry.ImageGUID = ImageGUID
                aEntry.OwnerGUID = GUID
                aEntry.TableHandle = Handle
            End If
            Return aEntry

        End Function
        Public Function EntryHandle(aNameOrHandleOrGUID As String) As String
            '#1the name or index of the member to return
            '^returns the object from the array that has a matching name or handle.
            Dim aEntry As dxfTableEntry = Nothing
            If TryGetEntry(aNameOrHandleOrGUID, aEntry) Then
                aEntry.ImageGUID = ImageGUID
                aEntry.OwnerGUID = GUID
                aEntry.TableHandle = Handle
                Return aEntry.Handle
            End If
            Return String.Empty

        End Function
        Public Function Entry(aIndex As Integer) As dxfTableEntry
            '#1the name or index of the member to return
            '^returns the object from the array that has a matching name or handle.
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then
                    Throw New IndexOutOfRangeException
                Else
                    Return Nothing
                End If
            End If
            Return MyBase.Item(aIndex - 1)

        End Function
        Friend Sub SetEntry(aNameOrHandle As String, aEntry As dxfTableEntry, Optional bAddIfNotFound As Boolean = False)
            If Count = 0 Or String.IsNullOrWhiteSpace(aNameOrHandle) Or aEntry Is Nothing Then Return
            If aEntry.EntryType <> TableType Then Return
            aNameOrHandle = aNameOrHandle.ToUpper.Trim()
            Dim aMem As dxfTableEntry = Nothing
            If TryGetEntry(aNameOrHandle, aMem) Then
                Me(MyBase.IndexOf(aMem)) = aEntry
            Else
                If Not bAddIfNotFound Then Return
                AddEntry(aEntry)
            End If
        End Sub

        ''' <summary>
        ''' Reteturns the entry as the passed index
        ''' </summary>
        ''' <remarks>an exceptions is thrown if the passed index is outside the bounds of the current array</remarks>
        ''' <param name="aIndex"></param>
        ''' <returns></returns>
        Public Shadows Function Item(aIndex As Integer) As dxfTableEntry
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then
                    Throw New IndexOutOfRangeException
                Else
                    Return Nothing
                End If
            End If
            Return MyBase.Item(aIndex - 1)

        End Function
        ''' <summary>
        ''' Reteturns the entry as the passed index
        ''' </summary>
        ''' <remarks>an exceptions is thrown if the passed index is outside the bounds of the current array</remarks>
        ''' <param name="aNameOrHandleOrGUID"></param>
        ''' <returns></returns>
        Public Shadows Function Item(aNameOrHandleOrGUID As String) As dxfTableEntry
            Return Find(Function(x) String.Compare(aNameOrHandleOrGUID, x.Name, True) = 0 Or String.Compare(aNameOrHandleOrGUID, x.Handle, True) = 0 Or String.Compare(aNameOrHandleOrGUID, x.GUID, True) = 0)

        End Function
        Public Function GetNames(Optional bReturnUpperCase As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            For Each mem As dxfTableEntry In Me
                If Not bReturnUpperCase Then _rVal.Add(mem.Name) Else _rVal.Add(mem.Name.ToUpper())
            Next
            Return _rVal
        End Function

        Public Function SelectLayer(aOwner As IWin32Window, aInitName As String, bIcludeLogicals As Boolean, Optional bComboStyle As Boolean = False) As String
            If TableType <> dxxReferenceTypes.LAYER Then Return aInitName
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return aInitName
            Dim aFrm As New frmTables
            Dim _rVal As String = aFrm.ShowSelect(aOwner, dxxReferenceTypes.LAYER, aImage, aInitName, bComboStyle, bIcludeLogicals, False)
            aFrm.Dispose()
            Return _rVal
        End Function


        Friend Sub AddReferenceToEntry(aNameOrHandle As String, aHandle As String)
            Dim aEntry As dxfTableEntry = Entry(aNameOrHandle)
            If aEntry Is Nothing Then Return
            aEntry.ReferenceADD(aHandle)
        End Sub

        Public Function Delete(aName As String) As Boolean
            Dim rError As String = String.Empty
            Return Delete(aName, rError)
        End Function

        Public Function Delete(aName As String, ByRef rError As String) As Boolean
            rError = String.Empty
            Dim mem As dxfTableEntry = Nothing
            If Not TryGetEntry(aName, mem) Then Return False
            Dim aImage = Nothing
            Dim myImage As dxfImage = Nothing
            If GetImage(myImage) Then
                Dim bBail As Boolean
                myImage.RespondToTableEvent(Me, TableType, mem, bBail, False, rError)
                If bBail Then Return False
            End If
            MyBase.Remove(mem)

            Return True
        End Function

        Public Function Delete(aIndex As Integer) As Boolean
            Dim rError As String = String.Empty
            Return Delete(aIndex, rError)
        End Function

        Public Function Delete(aIndex As Integer, ByRef rError As String) As Boolean
            'base 1
            rError = String.Empty
            If aIndex < 0 Or aIndex > Count Then Return False
            Dim aEntry As dxfTableEntry = Me(aIndex - 1)

            Dim cnt As Integer = Count
            Dim myImage As dxfImage = Nothing
            If GetImage(myImage) Then
                Dim bBail As Boolean
                myImage.RespondToTableEvent(Me, TableType, aEntry, bBail, False, rError)
                If bBail Then Return False
            End If
            Remove(aEntry)
            Return Count <> cnt
        End Function

        Friend Function AddToCollection(aNewEntry As dxfTableEntry, Optional bOverrideExisting As Boolean = False, Optional bSetCurrent As Boolean = False, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As dxfTableEntry
            If aNewEntry Is Nothing Then Return Nothing

            Dim img As dxfImage = Nothing
            Return AddToCollection(aNewEntry, bOverrideExisting, bSetCurrent, img, aBeforeIndex, aAfterIndex)
        End Function

        Public Overloads Function IndexOf(aMember As dxfTableEntry) As Integer
            Return MyBase.IndexOf(aMember) + 1
        End Function

        Friend Function AddToCollection(aNewEntry As dxfTableEntry, bOverrideExisting As Boolean, bSetCurrent As Boolean, ByRef ioImage As dxfImage, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As dxfTableEntry

            If aNewEntry Is Nothing Then Return Nothing
            If IndexOf(aNewEntry) > 0 Then Return aNewEntry
            Dim bBail As Boolean
            Dim aName As String = aNewEntry.Name
            Dim tname As String = TableTypeName()
            Dim sErr As String = String.Empty

            Try
                GetImage(ioImage)
                If String.IsNullOrWhiteSpace(aName) Then Throw New Exception("An attempt was made to add an entry with  an undefined Name")
                If aNewEntry.Properties.Count <= 0 Then Throw New Exception("An attempt was made to add an entry with 0 property count")
                If aNewEntry.EntryType <> TableType Then Throw New Exception($"Bad Entry Type : Entries of Type '{ aNewEntry.EntryType}' Cannot be added to tables of type '{ tname}'")
                'clear the handles
                aNewEntry.Handle = ""
                aNewEntry.Properties.SetVal(330, Handle)
                aNewEntry.ImageGUID = ""
                aNewEntry.IsCopied = False
                aNewEntry.IsGlobal = False
                Dim existingEntry As dxfTableEntry = Nothing
                Dim bExists As Boolean = TryGetEntry(aName, existingEntry)
                If bExists Then
                    If Not bOverrideExisting Then
                        Throw New Exception($"Bad { tname } Entry Detected '{ aName }' : An Attempt Was Made To Add a {tname } Entry With a Name That Is Already Assigned To Another Member")
                        Return Nothing
                    End If
                End If

                If ioImage IsNot Nothing And (Not bExists Or (bExists And Not bOverrideExisting)) Then
                    ioImage.RespondToTableEvent(Me, dxxCollectionEventTypes.PreAdd, aNewEntry, bBail, False, sErr)
                    If bBail And Not bOverrideExisting Then
                        Throw New Exception(sErr)  'the image declined the entry
                    Else
                        aNewEntry.SetImage(ioImage, False)
                    End If
                End If
                If bExists Then
                    'lets update it
                    MyBase.Remove(existingEntry)
                    aNewEntry.Handle = existingEntry.Handle
                    aNewEntry.GUID = existingEntry.Handle
                    'assign handles
                    If aNewEntry.Handle = "" And ioImage IsNot Nothing Then
                        ioImage.HandleGenerator.AssignTo(aNewEntry)
                    End If


                Else
                    'add it
                    If aAfterIndex > 0 And aBeforeIndex > 0 Then aAfterIndex = 0
                    If aAfterIndex > 0 And aAfterIndex >= Count Then aAfterIndex = 0
                    If aAfterIndex > 0 Then aBeforeIndex = aAfterIndex + 1
                    If aBeforeIndex > Count Then aBeforeIndex = 0
                    If aAfterIndex > 0 And aAfterIndex < Count Then
                        MyBase.Insert(aAfterIndex - 1, aNewEntry)
                    ElseIf aBeforeIndex >= 1 And aBeforeIndex <= Count Then
                        MyBase.Insert(aBeforeIndex - 2, aNewEntry)
                    Else
                        MyBase.Add(aNewEntry)
                    End If

                    'make sure it got added
                    If ioImage IsNot Nothing Then

                        ioImage.RespondToTableEvent(Me, dxxCollectionEventTypes.Add, aNewEntry, False, bSetCurrent, sErr)
                    End If
                End If
                Return aNewEntry
                'Application.DoEvents()
            Catch ex As Exception
                If ioImage IsNot Nothing Then
                    ioImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    Throw ex
                End If
                Return Nothing
            End Try
        End Function

        Public Sub Append(aTable As IEnumerable(Of dxfTableEntry), bOverrideExisting As Boolean)
            If aTable Is Nothing Then Return

            For Each mem As dxfTableEntry In aTable
                AddToCollection(mem, bOverrideExisting:=bOverrideExisting, bSetCurrent:=False)
            Next
        End Sub

        Public Sub AddEntry(aEntry As dxfTableEntry)
            If aEntry Is Nothing Then Return
            If aEntry.EntryType <> TableType Then Return
            aEntry.TableHandle = Handle
            aEntry.ImageGUID = ImageGUID
            aEntry.OwnerGUID = GUID
            AddToCollection(aEntry)
        End Sub

        Public Function MemberExists(aNameOrHandleOrGUID As String, Optional bNameOnly As Boolean = False)
            If Not bNameOnly Then
                Return FindIndex(Function(x) String.Compare(x.Name, aNameOrHandleOrGUID, True) = 0 Or String.Compare(x.GUID, aNameOrHandleOrGUID, True) = 0 Or String.Compare(x.Handle, aNameOrHandleOrGUID, True) = 0) >= 0
            Else
                Return FindIndex(Function(x) String.Compare(x.Name, aNameOrHandleOrGUID, True) = 0) >= 0
            End If

        End Function


        Friend Function Names(Optional aSkipList As String = "") As List(Of String)
            Dim _rVal As New List(Of String)
            '^returns a comma delimited string containing the names of the members
            For Each mem As dxfTableEntry In Me
                If Not TLISTS.Contains(mem, aSkipList, bReturnTrueForNullList:=True) Then _rVal.Add(mem.Name)
            Next

            Return _rVal
        End Function

        Public Function NameList(Optional aDelimiter As Char = ",", Optional aSkipList As String = "") As String
            Dim _rVal As String = String.Empty
            '^returns a comma delimited string containing the names of the members
            Dim names As List(Of String) = Me.Names
            For i As Integer = 1 To names.Count
                If i > 1 Then _rVal += aDelimiter
                _rVal += names(i - 1)
            Next
            Return _rVal
        End Function


        Public ReadOnly Property TableTypeName As String
            Get
                Return dxfEnums.DisplayName(TableType)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Friend Sub UpdateGUIDS()
            For Each aMem As dxfTableEntry In Me
                aMem.ImageGUID = ImageGUID
                aMem.CollectionGUID = GUID
                aMem.OwnerGUID = GUID
                aMem.Properties.SetVal(330, Handle)
            Next

        End Sub

        Friend Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage Is Nothing Then
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then
                    rImage = MyImage
                    If rImage Is Nothing Then
                        rImage = dxfEvents.GetImage(ImageGUID)
                        If rImage IsNot Nothing Then
                            If Not rImage.Disposed Then

                                SetImage(rImage, False)
                                Return True
                            Else
                                _ImagePtr = Nothing
                                ImageGUID = ""
                                Return False
                            End If

                        End If
                    Else
                        If Not rImage.Disposed Then

                            SetImage(rImage, False)
                            Return True
                        Else
                            SetImage(Nothing, True)
                        End If
                    End If
                End If
            Else
                If Not rImage.Disposed Then

                    SetImage(rImage, False)
                    Return True
                End If
            End If
            Return False
        End Function

        Dim _ImagePtr As WeakReference(Of dxfImage)
        Private disposedValue As Boolean

        Friend Sub SetImage(aImage As dxfImage, bDontReleaseOnNull As Boolean)
            Dim img As dxfImage = aImage
            If img IsNot Nothing AndAlso img.Disposed Then
                img = Nothing
            End If
            If img IsNot Nothing Then
                ImageGUID = img.GUID
                _ImagePtr = New WeakReference(Of dxfImage)(img)
            Else
                If Not bDontReleaseOnNull Then
                    ImageGUID = ""
                    _ImagePtr = Nothing
                End If
            End If
            UpdateGUIDS()
        End Sub

        Friend ReadOnly Property HasReferenceTo_Image As Boolean
            Get
                If String.IsNullOrWhiteSpace(ImageGUID) Or _ImagePtr Is Nothing Then Return False
                Dim img As dxfImage = Nothing
                Return _ImagePtr.TryGetTarget(img)
            End Get
        End Property
        Friend Overridable ReadOnly Property MyImage As dxfImage
            Get
                If Not HasReferenceTo_Image Then Return Nothing
                Dim _rVal As dxfImage = Nothing
                _ImagePtr.TryGetTarget(_rVal)
                If _rVal IsNot Nothing AndAlso _rVal.Disposed Then _rVal = Nothing
                If _rVal IsNot Nothing Then
                    If String.IsNullOrWhiteSpace(ImageGUID) Or String.Compare(ImageGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetImage(Nothing, False)
                End If
                Return _rVal
            End Get
        End Property
        Public Function TryGetEntry(aNameOrHandleOrGUID As String, ByRef rEntry As dxfTableEntry) As Boolean
            If String.IsNullOrWhiteSpace(aNameOrHandleOrGUID) Then
                rEntry = Nothing
                Return False
            End If
            rEntry = Find(Function(x) String.Compare(x.Name, aNameOrHandleOrGUID, True) = 0 Or String.Compare(x.GUID, aNameOrHandleOrGUID, True) = 0 Or String.Compare(x.Handle, aNameOrHandleOrGUID, True) = 0)
            If rEntry IsNot Nothing Then rEntry.Index = IndexOf(rEntry)
            Return rEntry IsNot Nothing
        End Function

        Public Function TryGetOrAdd(aName As String, aDefaultName As String, Optional bAddDefaultIfNotFound As Boolean = False) As dxfTableEntry
            Dim _rVal As Boolean
            Dim rEntry As dxfTableEntry = Nothing

            If String.IsNullOrWhiteSpace(aName) Then
                aName = String.Empty
            Else
                aName = aName.Trim()
                _rVal = TryGetEntry(aName, rEntry)
                If Not _rVal And Not bAddDefaultIfNotFound Then
                    rEntry = dxfTableEntry.CreateEntry(TableType, aName)
                    AddToCollection(rEntry)
                End If
            End If


            If String.IsNullOrWhiteSpace(aDefaultName) And bAddDefaultIfNotFound Then
                aDefaultName = DefaultMemberNames(TableType).FirstOrDefault()
            End If

            If Not _rVal And Not String.IsNullOrWhiteSpace(aDefaultName) Then
                _rVal = TryGetEntry(aDefaultName, rEntry)
                If Not _rVal Then
                    rEntry = dxfTableEntry.CreateEntry(TableType, aDefaultName)
                    AddToCollection(rEntry)
                End If
            End If
            Return rEntry
        End Function

        Public Sub ReferenceADD(aEntryName As String, aReference As String, Optional aGC As Integer = 330)
            Dim aMem As dxfTableEntry = Nothing
            If TryGetEntry(aEntryName, aMem) Then
                aMem.ReferenceADD(aReference, aGC)
            End If
        End Sub
        Public Sub ReferenceRemove(aEntryName As String, aReference As String)
            Dim aMem As dxfTableEntry = Nothing
            If TryGetEntry(aEntryName, aMem) Then
                aMem.ReferenceREMOVE(aReference)
            End If
        End Sub

        Public Function GetByPropertyValue(aGC As Integer, aValue As Object, aStringCompare As Boolean, Optional aOccur As Integer = 0, Optional aSecondaryValue As Object = Nothing) As dxfTableEntry
            For Each mem As dxfTableEntry In Me
                If mem.Properties.GetByPropertyValue(aGC, aValue, aStringCompare, aOccur, aSecondaryValue) IsNot Nothing Then Return mem
            Next
            Return Nothing
        End Function

        Public Overloads Function Remove(aNameOrHandleOrGUID As String) As List(Of dxfTableEntry)
            Dim _rVal As List(Of dxfTableEntry) = FindAll(Function(x) String.Compare(x.Name, aNameOrHandleOrGUID, True) = 0 Or String.Compare(x.GUID, aNameOrHandleOrGUID, True) = 0 Or String.Compare(x.Handle, aNameOrHandleOrGUID, True) = 0)
            For Each item As dxfTableEntry In _rVal
                MyBase.Remove(item)
            Next
            Return _rVal

        End Function

        Public Function GetMatchingEntry(aEntry As dxfTableEntry, Optional bTestHandle As Boolean = False, Optional bMustBeMember As Boolean = False) As dxfTableEntry
            If aEntry Is Nothing Then Return Nothing
            Dim _rVal As dxfTableEntry = Nothing
            If bTestHandle Then
                _rVal = Find(Function(x) String.Compare(x.Name, aEntry.Name, True) = 0 Or String.Compare(x.GUID, aEntry.GUID, True) = 0 Or String.Compare(x.Handle, aEntry.Handle, True) = 0)
            Else
                _rVal = Find(Function(x) String.Compare(x.Name, aEntry.Name, True) = 0 Or String.Compare(x.GUID, aEntry.GUID, True) = 0)
            End If
            If bMustBeMember And _rVal IsNot Nothing Then
                If _rVal IsNot aEntry Then Return Nothing
            End If
            Return _rVal

        End Function
#End Region 'Methods

#Region "Shared Methods"
        Public Shared Sub LoadDefaultMembers(aTable As dxfTable, Optional aImage As dxfImage = Nothing)

            If aTable Is Nothing Then Return
            Dim defNames As List(Of String) = dxfTable.DefaultMemberNames(aTable.TableType)

            Dim newEntries As New List(Of dxfTableEntry)
            For Each defName As String In defNames
                Dim aEntry As dxfTableEntry = Nothing
                If Not aTable.TryGetEntry(defName, rEntry:=aEntry) Then
                    aEntry = dxfTableEntry.CreateNewReference(aTable.TableType, defName)
                    If aEntry IsNot Nothing Then



                        aTable.Add(aEntry)
                        newEntries.Add(aEntry)

                    End If


                Else
                    Select Case aTable.TableType
                        Case dxxReferenceTypes.LTYPE
                            If String.Compare(defName, dxfLinetypes.Continuous, True) = 0 Then aEntry.Domain = dxxDrawingDomains.Model Else aEntry.Domain = dxxDrawingDomains.Paper
                            aEntry.Suppressed = String.Compare(defName, dxfLinetypes.Invisible, True) = 0
                            'Case dxxReferenceTypes.LAYER
                            'Case dxxReferenceTypes.STYLE
                            'Case dxxReferenceTypes.DIMSTYLE
                            'Case dxxReferenceTypes.VPORT
                            'Case dxxReferenceTypes.VIEW
                            'Case dxxReferenceTypes.UCS
                            'Case dxxReferenceTypes.APPID
                            'Case dxxReferenceTypes.BLOCK_RECORD
                    End Select



                End If
            Next


            If aTable.GetImage(aImage) Then
                aImage.HandleGenerator.AssignTo(aTable, aPreferedHandle:=aTable.Handle)
            End If

            aTable.UpdateBitCodes()
        End Sub
        Friend Shared Function DefaultMemberNames(aRefType As dxxReferenceTypes, Optional bReturnUpperCase As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            Try
                Select Case aRefType
                    Case dxxReferenceTypes.LTYPE
                        _rVal = New List(Of String)({dxfLinetypes.Invisible, dxfLinetypes.ByBlock, dxfLinetypes.ByLayer, dxfLinetypes.Continuous})
                    Case dxxReferenceTypes.LAYER
                        _rVal = New List(Of String)({"0"})
                    Case dxxReferenceTypes.STYLE
                        _rVal = New List(Of String)({"Standard", "Annotative"})
                    Case dxxReferenceTypes.DIMSTYLE
                        _rVal = New List(Of String)({"Standard", "Annotative"})
                    Case dxxReferenceTypes.VPORT
                        _rVal = New List(Of String)({"*Active"})
                    Case dxxReferenceTypes.VIEW
                        _rVal = New List(Of String)
                    Case dxxReferenceTypes.UCS
                        _rVal = New List(Of String)
                    Case dxxReferenceTypes.APPID
                        _rVal = New List(Of String)({"ACAD", "AcadAnnoPO", "AcadAnnotative", "ACAD_DSTYLE_DIMJAG", "ACAD_DSTYLE_DIMTALN", "ACAD_MLEADERVER"})
                    Case dxxReferenceTypes.BLOCK_RECORD
                        _rVal = New List(Of String)({"_ClosedFilled", "*Model_Space", "*Paper_Space"})
                End Select
                If bReturnUpperCase Then
                    For i As Integer = 0 To _rVal.Count - 1
                        _rVal.Item(i) = _rVal.Item(i).ToUpper
                    Next
                End If
            Catch ex As Exception
            End Try
            Return _rVal
        End Function

        Friend Shared Function FromTTABLE(aTable As TTABLE, Optional aImage As dxfImage = Nothing) As dxfTable
            Dim _rVal As dxfTable = CreateTable(aTable.TableType)
            If _rVal Is Nothing Then Return _rVal
            For i = 1 To aTable.Count
                Dim entry As TTABLEENTRY = aTable.Item(i)
                If entry.EntryType = aTable.TableType Then
                    Select Case aTable.TableType
                        Case dxxReferenceTypes.LTYPE
                            _rVal.Add(New dxoLinetype(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.LAYER
                            _rVal.Add(New dxoLayer(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.STYLE
                            _rVal.Add(New dxoStyle(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.DIMSTYLE
                            _rVal.Add(New dxoDimStyle(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.VPORT
                            _rVal.Add(New dxoViewPort(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.VIEW
                            _rVal.Add(New dxoView(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.UCS
                            _rVal.Add(New dxoUCS(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.APPID
                            _rVal.Add(New dxoAPPID(entry) With {.Image = aImage})
                        Case dxxReferenceTypes.BLOCK_RECORD
                            _rVal.Add(New dxoBlockRecord(entry) With {.Image = aImage})

                    End Select


                End If
            Next
            Return _rVal

        End Function

        Friend Shared Function CreateTable(aRefType As dxxReferenceTypes) As dxfTable
            Select Case aRefType
                Case dxxReferenceTypes.LTYPE
                    Return New dxoLineTypes()
                Case dxxReferenceTypes.LAYER
                    Return New dxoLayers()
                Case dxxReferenceTypes.STYLE
                    Return New dxoStyles()
                Case dxxReferenceTypes.DIMSTYLE
                    Return New dxoDimStyles()
                Case dxxReferenceTypes.VPORT
                    Return New dxoViewPorts()
                Case dxxReferenceTypes.VIEW
                    Return New dxoViews()
                Case dxxReferenceTypes.UCS
                    Return New dxoUCSs()
                Case dxxReferenceTypes.APPID
                    Return New dxoAPPIDs()
                Case dxxReferenceTypes.BLOCK_RECORD
                    Return New dxoBlockRecords()
                Case Else
                    Return Nothing
            End Select

        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    If Properties IsNot Nothing Then Properties.Clear()
                    Properties = Nothing

                    _ImagePtr = Nothing
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
            GC.SuppressFinalize(Me)
        End Sub

#End Region 'Shared Methods
    End Class 'dxoTable
End Namespace
