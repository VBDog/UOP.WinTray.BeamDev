
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Friend Class colDXFObjects
        Inherits List(Of dxfObject)
        Implements IEnumerable(Of dxfObject), IDisposable
        Implements ICloneable

#Region "Members"
        Private _ImageGUID As String
        Private ImagePtr As WeakReference


        Private _Disposed As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(aImage As dxfImage, Optional aImageGUID As String = "")
            MyBase.New()
            _Disposed = False

            If aImage IsNot Nothing Then
                If String.IsNullOrWhiteSpace(aImageGUID) Then aImageGUID = aImage.GUID Else aImageGUID = aImageGUID.Trim
                _ImageGUID = aImageGUID
                ImagePtr = New WeakReference(aImage)
            End If
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Shadows Function IndexOf(aMember As dxfObject) As Integer
            If aMember Is Nothing Or Count <= 0 Then Return 0
            Return MyBase.IndexOf(aMember) + 1
        End Function
        Public Shadows Function Item(aIndex As Integer) As dxfObject
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            Dim _rVal As dxfObject = MyBase.Item(aIndex - 1)
            _rVal.Index = aIndex
            _rVal.ImageGUID = ImageGUID
            Return _rVal

        End Function
        Friend Sub SetItem(aIndex As Integer, aMem As dxfObject)
            If aMem Is Nothing Or aIndex <= 0 Or aIndex > Count Then Return
            MyBase.RemoveAt(aIndex - 1)
            MyBase.Insert(aIndex - 1, aMem)

        End Sub

        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Friend Set(value As String)
                _ImageGUID = value
            End Set
        End Property


#End Region 'Properties
#Region "Methods"
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function

        Public Function Clone() As colDXFObjects
            Dim _rVal As New colDXFObjects(Nothing, ImageGUID)
            For Each aMem As dxfObject In Me
                _rVal.Add(aMem.Clone())
            Next
            Return _rVal
        End Function
        Friend Function Structure_Get() As TDXFOBJECTS
            Dim _rVal As New TDXFOBJECTS(0, ImageGUID)
            For i As Integer = 1 To Count
                Dim aMem As dxfObject = Item(i)
                _rVal.Add(aMem.Strukture)
            Next
            Return _rVal
        End Function

        Friend Sub Structure_Set(value As TDXFOBJECTS)
            Clear()
            Dim aMem As TDXFOBJECT
            Dim iGUID As String = ImageGUID
            _ImageGUID = ""
            For i As Integer = 1 To value.Count
                aMem = value.Item(i)
                Add(dxfObject.Create(aMem, Nothing))
            Next
            _ImageGUID = iGUID
            Dim aImage As dxfImage = Nothing
            If GetImage(aImage) Then
                For i As Integer = 1 To Count
                    aImage.HandleGenerator.AssignTo(Item(i))
                Next
            End If
        End Sub
        Public Function DictionaryNames(Optional aImage As dxfImage = Nothing) As dxfoDictionary
            Dim _rVal As dxfoDictionary = Nothing

            If Not TryGet("DICTIONARY NAMES", dxxObjectTypes.Dictionary, _rVal) Then
                Add(New dxfoDictionary("DICTIONARY NAMES"), True, aImage:=aImage)
                _rVal = GetObject("DICTIONARY NAMES", dxxObjectTypes.Dictionary)
            End If
            Return _rVal
        End Function
        Public Function VerifyNamedDictionary(aDictionaryName As String, Optional bWDFT As Boolean = False, Optional aImage As dxfImage = Nothing) As dxfObject
            If String.IsNullOrEmpty(aDictionaryName) Then Return Nothing
            aDictionaryName = aDictionaryName.Trim()
            Dim nmDict As dxfoDictionary = DictionaryNames(aImage:=aImage)
            Dim _rVal As dxfObject = Nothing
            If Not TryGet(aDictionaryName, dxxObjectTypes.Dictionary, _rVal) Then
                If bWDFT Then _rVal = New dxfoDictionaryWDFLT Else _rVal = New dxfoDictionary(aDictionaryName)
                _rVal.Name = aDictionaryName
                If Not Add(_rVal, aImage:=aImage) Then Return Nothing
            End If
            _rVal.ReactorGUID = nmDict.GUID
            _rVal.AddReactorHandle(nmDict.GUID, nmDict.Handle)
            Return _rVal
        End Function
        Public Sub ClearGroups(Optional aImage As dxfImage = Nothing)
            If Not GetImage(aImage) Then Return
            'Dim gDict As dxfoDictionary = VerifyNamedDictionary("ACAD_GROUP")
            'Dim newMems As New List(Of  dxfObject)
            'gDict.Entries = New TDICTIONARYENTRIES(gDict.Entries.NameGroupCode, gDict.Entries.HandleGroupCode)
            Dim aMem As dxfObject
            Dim grp As dxfoGroup
            For i = 1 To Count
                aMem = Item(i)
                If aMem.ObjectType = dxxObjectTypes.Group Then
                    grp = aMem
                    grp.Entries = New TDICTIONARYENTRIES(grp.Entries.NameGroupCode, grp.Entries.HandleGroupCode)
                    'newMems.Add(aMem.GUID, aMem)
                End If
            Next
            '_Members.Clear()
            '_Members = newMems
        End Sub


        Public Function AddGroupEntry(aGroupName As String, aEntryObj As dxfEntity) As Boolean
            If String.IsNullOrEmpty(aGroupName) Or aEntryObj Is Nothing Then Return False
            If aEntryObj.Handle = "" Then Return False
            aGroupName = aGroupName.Trim()
            Dim grpDic As dxfoDictionary = VerifyNamedDictionary("ACAD_GROUP")
            Dim aGrp As dxfoGroup = Nothing
            If Not TryGet(aGroupName, dxxObjectTypes.Group, aGrp) Then
                aGrp = New dxfoGroup(aGroupName)
                If Not Add(aGrp) Then
                    Return False
                Else
                    aGrp = Item(Count)
                End If
            End If
            grpDic.AddEntry(aGrp.Name, aGrp.Handle)
            Dim gEntry As TDICTIONARYENTRY = TDICTIONARYENTRY.Null
            If aGrp.AddEntry(aEntryObj.GUID, aEntryObj.Handle, gEntry) Then
                'System.Diagnostics.Debug.WriteLine(gEntry.ToString)
            Else
                'System.Diagnostics.Debug.WriteLine("EXISTING:" & gEntry.ToString)
            End If
            'System.Diagnostics.Debug.WriteLine(aGrp.Entries.Count)
            'Dim grpEntries As TDICTIONARYENTRIES = aGrp.Entries
            'Dim gEntry as TDICTIONARYENTRY =  TDICTIONARYENTRY.Null
            'If Not grpEntries.TryGet(aEntryObj.Handle, gEntry) Then
            '    grpEntries.Add(aEntryObj.GUID, aEntryObj.Handle)
            '    aGrp.Entries = grpEntries
            'End If
            aGrp.AddReactorHandle(grpDic.GUID, grpDic.Handle)
            aEntryObj.AddReactorHandle(aGrp.GUID, aGrp.Handle, 5)
            Return True
        End Function
        Public Function GetObjects(aObjectType As dxxObjectTypes) As List(Of dxfObject)
            Return FindAll(Function(x) x.ObjectType = aObjectType)
        End Function
        Public Function GetObject(aNameOrGUIDorHandle As String, aObjectType As dxxObjectTypes) As dxfObject
            If String.IsNullOrWhiteSpace(aNameOrGUIDorHandle) Then Return Nothing
            Dim objs As List(Of dxfObject)
            If aObjectType = dxxObjectTypes.Dictionary Or aObjectType = dxxObjectTypes.DictionaryWDFLT Then
                objs = FindAll(Function(x) x.ObjectType = dxxObjectTypes.Dictionary Or x.ObjectType = dxxObjectTypes.DictionaryWDFLT)
            Else
                objs = FindAll(Function(x) x.ObjectType = aObjectType)
            End If
            If objs.Count <= 0 Then Return Nothing

            Dim _rVal As dxfObject = objs.Find(Function(x) String.Compare(x.Name, aNameOrGUIDorHandle, True) = 0)
            If _rVal IsNot Nothing Then Return _rVal
            _rVal = objs.Find(Function(x) String.Compare(x.GUID, aNameOrGUIDorHandle, True) = 0)

            If _rVal IsNot Nothing Then Return _rVal
            _rVal = objs.Find(Function(x) String.Compare(x.Handle, aNameOrGUIDorHandle, True) = 0)

            Return _rVal
        End Function
        Public Function TryGet(aNameOrGUIDorHandle As String, aObjectType As dxxObjectTypes, ByRef rObject As dxfObject) As Boolean
            rObject = GetObject(aNameOrGUIDorHandle, aObjectType)
            Return rObject IsNot Nothing
        End Function
        Public Function GetDictionary(aNameOrGUIDorHandle As String) As dxfoDictionary
            If String.IsNullOrWhiteSpace(aNameOrGUIDorHandle) Then Return Nothing
            Dim objs As List(Of dxfObject) = FindAll(Function(x) x.ObjectType = dxxObjectTypes.Dictionary)

            If objs.Count <= 0 Then Return Nothing

            Dim _rVal As dxfObject = objs.Find(Function(x) String.Compare(x.Name, aNameOrGUIDorHandle, True) = 0)
            If _rVal IsNot Nothing Then Return _rVal
            _rVal = objs.Find(Function(x) String.Compare(x.GUID, aNameOrGUIDorHandle, True) = 0)

            If _rVal IsNot Nothing Then Return _rVal
            _rVal = objs.Find(Function(x) String.Compare(x.Handle, aNameOrGUIDorHandle, True) = 0)

            Return _rVal
        End Function
        Private Sub RegisterDictionary(aDictionary As dxfObject)
            Dim nmsdic As dxfoDictionary = DictionaryNames()
            Dim aEntry As TDICTIONARYENTRY = TDICTIONARYENTRY.Null
            If Not nmsdic.TryGetEntry(aDictionary.Name, aDictionary.Handle, aEntry) Then
                nmsdic.AddEntry(aDictionary.Name, aDictionary.Handle)
            Else
                aEntry.Name = aDictionary.Name
                aEntry.Handle = aDictionary.Handle
                Dim dEntries As TDICTIONARYENTRIES = nmsdic.Entries
                dEntries.SetItem(aEntry.Index, aEntry)
                nmsdic.Entries = dEntries
            End If
            aDictionary.ReactorGUID = nmsdic.GUID
            aDictionary.AddReactorHandle(nmsdic.GUID, nmsdic.Handle)
        End Sub
        Public Overloads Function Add(aObject As dxfObject, Optional bSuppressDictionaryAssoc As Boolean = False, Optional aImage As dxfImage = Nothing) As Boolean
            If aObject Is Nothing Then Return False
            If aObject.GUID = "" Then aObject.GUID = dxfEvents.NextObjectGUID(aObject.ObjectType)

            If FindIndex(Function(mem) mem.GUID = aObject.GUID) >= 0 Then Return False
            If Not GetImage(aImage) Then
                MyBase.Add(aObject)
            Else
                Dim bDontAdd As Boolean
                aObject.SetProperty(5, "")

                aImage.RespondToObjectsEvent(Me, dxxCollectionEventTypes.PreAdd, aObject, bDontAdd)
                If bDontAdd Then Return False
                aImage.RespondToObjectsEvent(Me, dxxCollectionEventTypes.Add, aObject, bDontAdd)
                aObject.Index = Count + 1
                MyBase.Add(aObject)
            End If
            If Not bSuppressDictionaryAssoc Then
                If aObject.ObjectType = dxxObjectTypes.Dictionary Or aObject.ObjectType = dxxObjectTypes.DictionaryWDFLT Then
                    If aObject.Name <> "" And aObject.Handle <> "" Then
                        RegisterDictionary(aObject)
                    End If
                End If
            End If
            Return True
        End Function


        Public Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage IsNot Nothing Then Return True
            If _ImageGUID <> "" Then rImage = dxfEvents.GetImage(_ImageGUID)
            Return rImage IsNot Nothing
        End Function
#End Region 'Methods

#Region "IDisposable Implementation"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _Disposed Then
                If disposing Then
                    Clear()

                End If
                _Disposed = True
            End If
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region '"IDisposable Implementation
    End Class
    Friend Structure TDXFOBJECT
        Implements ICloneable
#Region "Members"
        Public ExtendedData As TPROPERTYARRAY
        Public Reactors As TPROPERTYARRAY
        Public Props As TPROPERTIES
        Public DictionaryIndex As Integer
        Public GUID As String
        Public ImageGUID As String
        Private _Name As String
        Public Index As Integer
        Public ObjectType As dxxObjectTypes
        Public Suppressed As Boolean
#End Region 'Members
#Region "Constructors"

        Public Sub New(Optional aName As String = "")
            'init ----------------------------------------
            ExtendedData = New TPROPERTYARRAY(0)
            Reactors = New TPROPERTYARRAY(0)
            Props = New TPROPERTIES(0)
            DictionaryIndex = 0
            GUID = ""
            ImageGUID = ""
            _Name = aName
            Index = -1
            ObjectType = dxxObjectTypes.Undefined
            Suppressed = False
            'init ----------------------------------------
        End Sub
        Public Sub New(aObject As TDXFOBJECT)
            'init ----------------------------------------
            ExtendedData = New TPROPERTYARRAY(aObject.ExtendedData)
            Reactors = New TPROPERTYARRAY(aObject.Reactors)
            Props = New TPROPERTIES(aObject.Props)
            DictionaryIndex = aObject.DictionaryIndex
            GUID = aObject.GUID
            ImageGUID = aObject.ImageGUID
            _Name = aObject.Name
            Index = aObject.Index
            ObjectType = aObject.ObjectType
            Suppressed = aObject.Suppressed
            'init ----------------------------------------
        End Sub
        Public Sub New(aObjectType As dxxObjectTypes, Optional aName As String = "", Optional aPropertyString As String = "", Optional bSuppressProps As Boolean = False)
            'init ----------------------------------------
            ExtendedData = New TPROPERTYARRAY(0)
            Reactors = New TPROPERTYARRAY(0)
            Props = New TPROPERTIES(0)
            DictionaryIndex = 0
            GUID = ""
            ImageGUID = ""
            _Name = ""
            Index = -1
            ObjectType = dxxObjectTypes.Undefined
            Suppressed = False
            'init ----------------------------------------

            If Not bSuppressProps Then
                Props = dxpProperties.Get_ObjectProperties(aObjectType, aName, aPropertyString)
            End If
            GUID = dxfEvents.NextObjectGUID(aObjectType)

            Reactors.Add(New TPROPERTIES("{ACAD_REACTORS"), "", True)
            Select Case aObjectType
                Case dxxObjectTypes.MLineStyle
                    If aPropertyString <> "" Then
                        Reactors.Add(Props.GetByGroupCode(49, aFollowerCount:=2, bRemove:=True, aName:="Elements", aNameList:="Element Offset,Element Color,Element Linetype"), "")
                    End If
                Case dxxObjectTypes.Dictionary
                    Reactors.Add(New TPROPERTIES("Members"), "", True)
                Case dxxObjectTypes.DictionaryWDFLT
                    Reactors.Add(New TPROPERTIES("Members"), "", True)
                Case dxxObjectTypes.Layout
                    Props.SetValueGC(1, aName, -1)
                Case dxxObjectTypes.PlotSetting
                    Props.SetValueGC(1, aName)
                Case dxxObjectTypes.Group
                    Reactors.Add(New TPROPERTIES("Members"), "", True)
                Case dxxObjectTypes.MLineStyle
                    Reactors.GetProps("Members", True)
                Case dxxObjectTypes.MLeaderStyle
                    Dim extData As New TPROPERTIES("ACAD_MLEADERVER")
                    extData.Add(New TPROPERTY(1070, 2, "MLEADERVER", dxxPropertyTypes.dxf_Integer))
                    ExtendedData.Add(extData, "ACAD_MLEADERVER", True)
            End Select
            '     obj_Null.Props.SetValueGC( 5, aHandle)
            ObjectType = aObjectType
            Props.SetVal("*GUID", GUID)
            Name = aName
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
                Props.SetVal("*Name", value)
                Props.SetVal("Name", value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Sub Clear()

            Props = New TPROPERTIES(dxfEnums.Description(ObjectType))
            ExtendedData = New TPROPERTYARRAY("Extended Data")
            Reactors = New TPROPERTYARRAY("Reactors")

        End Sub
        Public Function Clone() As TDXFOBJECT
            Return New TDXFOBJECT(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDXFOBJECT(Me)
        End Function

        Public Function AddReactor(aHandle As String, Optional bIsParent As Boolean = False) As Boolean
            aHandle = Trim(aHandle)
            If bIsParent Then
                Props.SetValueGC(330, aHandle)
                Reactors.AddReactor("{ACAD_REACTORS", 330, aHandle)
            Else
                If aHandle = "" Then Return False
                Reactors.AddReactor("{ACAD_REACTORS", 330, aHandle)
            End If
            Return True
        End Function
        Public Sub AddReferenceTo(aHandleGenerator As dxoHandleGenerator, bObject As TDXFOBJECT, Optional bAddReactor As Boolean = False, Optional aPointerGC As Integer = 350, Optional aRefName As String = "")
            Dim aHndl As String
            Dim bHndl As String
            Dim bNm As String
            Dim idx As Integer
            Dim memberProps As TPROPERTIES = Reactors.GetProps("Members", idx, True)
            Dim nProp As TPROPERTY
            Dim hProp As TPROPERTY
            If aRefName = "" Then bNm = bObject.Name Else bNm = aRefName
            bHndl = bObject.Props.GCValueStr(5)
            aHndl = Props.GCValueStr(5)
            If bHndl <> "" And bNm <> "" Then
                nProp = memberProps.GetByStringValue(bNm)
                If nProp.Index <= 0 Then
                    memberProps.Add(New TPROPERTY(3, bNm, "Entry Name", dxxPropertyTypes.dxf_String))
                    memberProps.Add(New TPROPERTY(aPointerGC, bHndl, "Entry Handle", dxxPropertyTypes.Pointer))
                Else
                    hProp = memberProps.Item(nProp.Index + 1)
                    hProp.Value = bHndl
                    memberProps.SetItem(nProp.Index + 1, hProp)
                End If
                Reactors.SetItem(idx, memberProps)
                'Reactors.AddMemberPair("Members", 3, bNm, aPointerGC, bHndl, "Entry Name", "Entry Handle")
                If bAddReactor Then
                    bObject.Props.SetValueGC(330, aHndl)
                    Dim rProp As TPROPERTY
                    Dim rProps As TPROPERTIES = bObject.Reactors.GetProps("{ACAD_REACTORS", idx, True)
                    If aHandleGenerator IsNot Nothing And rProps.Count > 0 Then
                        Dim hGUID As String = String.Empty
                        For i As Integer = rProps.Count To 1 Step -1
                            rProp = rProps.Item(i)
                            If Not aHandleGenerator.HandleIsUsed(TVALUES.To_STR(rProp.Value), hGUID) Then
                                rProps.Remove(i)
                            Else
                                If hGUID = bObject.GUID Or hGUID = GUID Then
                                    rProp.Value = aHndl
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                    hProp = rProps.GetByStringValue(aHndl)
                    If hProp.Index <= 0 Then
                        rProps.Add(New TPROPERTY(330, aHndl, $"Reactor_{ rProps.Count}", dxxPropertyTypes.Pointer))
                    Else
                        hProp.Value = aHndl
                        rProps.SetItem(hProp.Index, hProp)
                    End If
                    'bObject.Reactors.AddReactor("{ACAD_REACTORS", 330, aHndl)
                    bObject.Reactors.SetItem(idx, rProps)
                End If
            End If
        End Sub
        Public Function HasReferenceTo(aObjectName As String, ByRef rHandle As String) As Boolean
            Dim _rVal As Boolean
            rHandle = ""
            If aObjectName = "" Then Return _rVal
            Dim rArrayIndex As Integer


            Dim bProp As TPROPERTY
            Dim aRefs As TPROPERTIES = Reactors.GetProps("Members", rArrayIndex, True)
            Dim rIndex As Integer

            Dim aProp As TPROPERTY = aRefs.GetByStringValue(aObjectName, 1, rIndex:=rIndex, aGC:=3)
            If rIndex > 0 And rIndex < aRefs.Count Then
                bProp = aRefs.Item(rIndex)
                rHandle = bProp.Value.ToString()
                _rVal = True
            Else
                aProp = Props.GetByStringValue(aObjectName, 1, rIndex:=rIndex, aGC:=3)
                If rIndex > 0 And rIndex < Props.Count Then
                    bProp = Props.Item(rIndex)
                    rHandle = bProp.Value.ToString()
                    _rVal = True
                Else
                    rIndex = -1
                End If
            End If
            Return _rVal
        End Function
        Public Function HasReferenceToHandle(aHandle As String, ByRef rName As String, ByRef rIndex As Integer) As Boolean
            rName = ""
            rIndex = -1
            If aHandle = "" Then Return False
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim aRefs As TPROPERTIES
            aRefs = Reactors.GetProps("Members", bAddIfNotFound:=True)
            aProp = aRefs.GetByStringValue(aHandle, 1, rIndex:=rIndex, bPointersOnly:=True)
            If rIndex > 1 And rIndex <= aRefs.Count Then
                bProp = aRefs.Item(rIndex - 1)
                rName = bProp.Value.ToString()
            Else
                rIndex = -1
            End If
            If rIndex < 0 Then
                aProp = Props.GetByStringValue(aHandle, 1, rIndex:=rIndex, bPointersOnly:=True)
                If rIndex > 0 And rIndex < Props.Count Then
                    bProp = Props.Item(rIndex - 1)
                    rName = bProp.Value.ToString()
                Else
                    rIndex = -1
                End If
            End If
            Return rIndex <> -1
        End Function
        Public Sub Print(Optional bDXFFormat As Boolean = False, Optional aStream As IO.StreamWriter = Nothing)
            '^prints the objects to the debug screen and or the passed stream
            Dim i As Integer
            Dim j As Integer
            Dim otname As String
            Dim otype As dxxObjectTypes
            Dim aProp As TPROPERTY
            Dim rHandles As New TPROPERTIES("Handles")
            otname = Props.GCValueStr(0)
            otype = ObjectType
            If otype = dxxObjectTypes.Undefined Then
                otype = dxfObject.ObjectNameToType(otname)
                ObjectType = otype
            End If
            For i = 1 To Props.Count
                aProp = Props.Item(i)
                aProp.Print(bDXFFormat, aStream)
                If aProp.GroupCode = 5 Then
                    rHandles = Reactors.GetProps("{ACAD_XDICTIONARY")
                    If rHandles.Count > 0 Then
                        rHandles = rHandles.Wrapped(102, rHandles.Name, "Start Dictionary Reactors", 102, "}", "End Reactors")
                        For j = 1 To rHandles.Count
                            rHandles.Item(j).Print(bDXFFormat, aStream)
                        Next j
                    End If
                    rHandles = Reactors.GetProps("{ACAD_REACTORS")
                    If rHandles.Count > 0 Then
                        rHandles = rHandles.Wrapped(102, rHandles.Name, "Start Reactors", 102, "}", "End Reactors")
                        For j = 1 To rHandles.Count
                            rHandles.Item(j).Print(bDXFFormat, aStream)
                        Next j
                    End If
                End If
                If otype = dxxObjectTypes.Dictionary Or otype = dxxObjectTypes.DictionaryWDFLT Then
                    If aProp.GroupCode = 281 Then
                        rHandles = Reactors.GetProps("Members")
                        For j = 1 To rHandles.Count
                            rHandles.Item(j).Print()
                        Next j
                    End If
                End If
                If otype = dxxObjectTypes.Group Then
                    If aProp.GroupCode = 71 Then
                        rHandles = Reactors.GetProps("Members")
                        For j = 1 To rHandles.Count
                            rHandles.Item(j).Print()
                        Next j
                    End If
                End If
            Next i
        End Sub
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function DEFAULT_CELLSTYLEMAP(Optional aTextStyleHandle As Object = Nothing) As TDXFOBJECT
            Dim _rVal As New TDXFOBJECT(dxxObjectTypes.CellStyleMap) With {.Props = dxpProperties.Get_ObjectProperties(dxxObjectTypes.CellStyleMap, "ACAD_ROUNDTRIP_2008_TABLESTYLE_CELLSTYLEMAP")}
            If aTextStyleHandle <> "" Then _rVal.Props.SetVal("TextStyle", aTextStyleHandle)
            Return _rVal
        End Function
#End Region 'Shared MEthods
    End Structure 'TDXFOBJECT
    Friend Structure TDXFOBJECTS
        Implements ICloneable
#Region "Members"
        Public ImageGUID As String
        Public ObjectType As dxxObjectTypes
        Public ReadFromFile As Boolean
        Private _Init As Boolean
        Private _Members() As TDXFOBJECT
#End Region 'Members
#Region "Constructors"
        Public Sub New(aObjectType As dxxObjectTypes, Optional aImageGUID As String = "")
            'init ---------------------
            _Init = True
            ReDim _Members(-1)
            ObjectType = aObjectType
            ImageGUID = aImageGUID
            'init ---------------------
        End Sub
        Public Sub New(Optional aImageGUID As String = "")
            'init ---------------------
            _Init = True
            ReDim _Members(-1)
            ObjectType = dxxObjectTypes.Undefined
            ImageGUID = aImageGUID
            'init ---------------------
        End Sub

        Public Sub New(aObject As TDXFOBJECTS, Optional bDontCloneMembers As Boolean = False)
            'init ---------------------
            _Init = True
            ReDim _Members(-1)
            ObjectType = aObject.ObjectType
            ImageGUID = aObject.ImageGUID
            'init ---------------------
            If aObject._Init And Not bDontCloneMembers Then _Members = aObject._Members.Clone()
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As TDXFOBJECT
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TDXFOBJECT("")
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TDXFOBJECT)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Sub Add(aObj As TDXFOBJECT, Optional aHandle As String = Nothing, Optional aBeforeIndex As Integer = 0, Optional bSuppressIndexes As Boolean = False)
            Dim rIndex As Integer = 0
            If Count >= Integer.MaxValue Then Return
            If Not String.IsNullOrEmpty(aHandle) Then aObj.Props.SetValueGC(5, aHandle.Trim)
            If aBeforeIndex <= 0 Or aBeforeIndex > Count Or Count <= 0 Then aBeforeIndex = 0
            aObj.ImageGUID = ImageGUID
            If aBeforeIndex > 0 Then
                Dim i As Integer
                Dim newMems(0 To Count + 1) As TDXFOBJECT
                Dim j As Integer
                j = 1
                For i = 1 To Count
                    If j = aBeforeIndex Then
                        newMems(j - 1) = aObj
                        rIndex = j - 1
                        j += 1
                    End If
                    newMems(j - 1) = _Members(i - 1)
                    j += 1
                Next i
                If Not bSuppressIndexes Then newMems(rIndex).Index = rIndex
                _Members = newMems
            Else
                Array.Resize(_Members, Count + 1)
                If Not bSuppressIndexes Then aObj.Index = Count
                _Members(_Members.Count - 1) = aObj
                rIndex = Count
            End If
        End Sub
        Public Sub AddAfter(aObj As TDXFOBJECT, aAfterIndex As Integer, ByRef rIndex As Integer, Optional aHandle As String = Nothing)
            rIndex = 0
            If Not String.IsNullOrEmpty(aHandle) Then aObj.Props.SetValueGC(5, aHandle.Trim)
            If aAfterIndex < 1 Then aAfterIndex = 1
            If aAfterIndex >= Count Then
                Add(aObj)
                rIndex = Count
                Return
            End If
            Dim newMems(0 To Count + 1) As TDXFOBJECT
            aObj.ImageGUID = ImageGUID
            Dim j As Integer = 0
            For i As Integer = 1 To Count
                newMems(j) = _Members(i - 1)
                j += 1
                If i = aAfterIndex Then
                    aObj.Index = j + 1
                    newMems(j) = aObj
                    j += 1
                    rIndex = aObj.Index
                End If
            Next i
            _Members = newMems
        End Sub
        Public Sub AddBefore(aObj As TDXFOBJECT, aBeforeIndex As Integer, ByRef rIndex As Integer, Optional aHandle As String = Nothing)
            rIndex = 0
            If Not String.IsNullOrEmpty(aHandle) Then aObj.Props.SetValueGC(5, aHandle.Trim)
            If aBeforeIndex < 1 Then aBeforeIndex = 1
            If aBeforeIndex >= Count Then
                Add(aObj)
                rIndex = Count
                Return
            End If
            Dim newMems(0 To Count + 1) As TDXFOBJECT
            aObj.ImageGUID = ImageGUID
            Dim j As Integer = 0
            For i As Integer = 1 To Count
                newMems(j) = _Members(i - 1)
                If i = aBeforeIndex Then
                    aObj.Index = j + 1
                    newMems(j - 1) = aObj
                    j += 1
                    rIndex = aObj.Index
                End If
                newMems(j - 1) = _Members(i - 1)
                j += 1
            Next i
            _Members = newMems
        End Sub
        Public Sub Clear()

            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Function Clone(Optional bDontCloneMembers As Boolean = False) As TDXFOBJECTS
            Return New TDXFOBJECTS(Me, bDontCloneMembers)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDXFOBJECTS(Me)
        End Function

        Public Function GetByHandle(aHandle As String, ByRef rIndex As Integer, Optional bSuppressIndexes As Boolean = False, Optional aObjectType As dxxObjectTypes = dxxObjectTypes.Undefined) As TDXFOBJECT
            rIndex = 0
            Dim _rVal As New TDXFOBJECT(dxxObjectTypes.Undefined)
            '#1the object handle to search for
            '#2returns the index of the object in the collection if it is found
            '^returns the requested object if the passed handle is found
            '~is not case dependant and lead and trailing spaces are ignored.
            '~returns nothing if the object can't be found.
            Dim aObj As TDXFOBJECT
            Dim hndl As String
            For i As Integer = 1 To Count
                aObj = Item(i)
                aObj.ImageGUID = ImageGUID
                If aObjectType = dxxObjectTypes.Undefined Or aObj.ObjectType = aObjectType Then
                    hndl = aObj.Props.GCValueStr(5)
                    If String.Compare(hndl, aHandle, True) = 0 Then
                        _rVal = aObj
                        If Not bSuppressIndexes Then _rVal.Index = i
                        rIndex = i
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetByName(aName As String, aObjectType As dxxObjectTypes, ByRef rIndex As Integer, Optional aOccurance As Integer = 0, Optional bSuppressIndexes As Boolean = False) As TDXFOBJECT
            rIndex = 0
            Dim _rVal As New TDXFOBJECT(dxxObjectTypes.Undefined)
            Dim i As Integer
            Dim aObj As TDXFOBJECT
            Dim cnt As Integer
            Dim objName As String
            Dim aMem As TDXFOBJECT
            If aOccurance <= 0 Then aOccurance = 1
            For i = 1 To Count
                aMem = Item(i)
                If Not bSuppressIndexes Then aMem.Index = i
                aMem.ImageGUID = ImageGUID
                aObj = aMem
                If aObj.ObjectType <= dxxObjectTypes.Undefined Then
                    aObj.ObjectType = dxfObject.ObjectNameToType(aObj.Props.GCValueStr(0))
                    aMem = aObj
                End If
                If aObjectType = dxxObjectTypes.Undefined Or aObj.ObjectType = aObjectType Then
                    objName = aObj.Name
                    If objName = "" Then
                        If aObj.ObjectType = dxxObjectTypes.Layout Or aObj.ObjectType = dxxObjectTypes.Material Then
                            objName = aObj.Props.GCValueStr(1)
                        End If
                    End If
                    If objName = "" Then
                        objName = LookUpObjectName(aObj)
                        aObj.Name = objName
                        aMem = aObj
                    End If
                    If String.Compare(objName, aName, True) = 0 Then
                        cnt += 1
                        If cnt >= aOccurance Then
                            rIndex = i
                            _rVal = aObj
                            Exit For
                        End If
                    End If
                End If
            Next i
            If bSuppressIndexes And rIndex > 0 Then
                If _rVal.Index > 0 Then rIndex = _rVal.Index
            End If
            Return _rVal
        End Function
        Public Function ObjectsByObjectName(aObjectName As String) As TDXFOBJECTS
            Dim _rVal As TDXFOBJECTS = Clone(bDontCloneMembers:=True)
            '#2the object name to search for
            '~is not case dependant and lead and trailing spaces are ignored.
            aObjectName = Trim(aObjectName)
            If aObjectName = "" Then Return _rVal
            Dim aMem As TDXFOBJECT
            Dim i As Integer
            For i = 1 To Count - 1
                aMem = Item(i)
                aMem.ImageGUID = ImageGUID
                SetItem(i, aMem)
                If String.Compare(aMem.Props.GCValueStr(0), aObjectName, True) = 0 Then
                    _rVal.Add(aMem)
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetByObjectType(aObjectType As dxxObjectTypes, Optional bReturnInverse As Boolean = False, Optional bSuppressIndices As Boolean = False) As TDXFOBJECTS
            Dim _rVal As TDXFOBJECTS = Clone(bDontCloneMembers:=True)
            Dim aObj As TDXFOBJECT
            Dim otype As dxxObjectTypes
            For i As Integer = 1 To Count
                aObj = Item(i)
                If Not bSuppressIndices Then aObj.Index = i
                aObj.ImageGUID = ImageGUID
                otype = dxfObject.ObjectNameToType(aObj.Props.GCValueStr(0))
                If (Not bReturnInverse And otype = aObjectType) Or (bReturnInverse And otype <> aObjectType) Then
                    _rVal.Add(aObj, bSuppressIndexes:=bSuppressIndices)
                End If
            Next i
            _rVal.ObjectType = aObjectType
            Return _rVal
        End Function

        Friend Function objs_GetDictionaryContainingEntry(aMemberName As String, ByRef rDictionaryIndex As Integer, Optional aEntryIndex As Integer = 0) As TDXFOBJECT
            Dim _rVal As New TDXFOBJECT
            Dim i As Integer
            Dim idx As Integer
            Dim dcnt As Integer
            Dim aEntry As New TDXFOBJECT
            rDictionaryIndex = 0
            aEntryIndex = 0
            For i = 1 To Count
                If Item(i).ObjectType = dxxObjectTypes.Dictionary Or Item(i).ObjectType = dxxObjectTypes.DictionaryWDFLT Then
                    dcnt += 1
                    aEntry = GetDictionaryMember(aMemberName, i, idx)
                    If idx >= 0 Then
                        rDictionaryIndex = i
                        aEntryIndex = aEntry.DictionaryIndex
                        _rVal = Item(1)
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetDictionaryMember(aMemberName As String, aDictionaryIndex As Integer, ByRef rIndex As Integer) As TDXFOBJECT
            Dim _rVal As New TDXFOBJECT
            rIndex = 0
            aMemberName = Trim(aMemberName)
            _rVal = New TDXFOBJECT(dxxObjectTypes.Undefined, aMemberName) With {
                    .Index = -1
                }
            If aDictionaryIndex <= 0 Or aDictionaryIndex > Count Or aMemberName = "" Or Count <= 1 Then Return _rVal
            Dim aMems As TPROPERTIES
            Dim aProps As TPROPERTIES
            Dim pidx As Integer
            'get the members array of the named object dictionary
            aMems = Item(aDictionaryIndex).Reactors.GetProps("Members", bAddIfNotFound:=True)
            If aMems.Count <= 0 Then Return _rVal
            'look up the member handle
            aProps = aMems.GetByGroupCode(3, Nothing, 1, bRemove:=False, aName:=aMemberName, bJustOne:=False, rFirstIndex:=pidx)
            If aProps.Count <= 1 Then Return _rVal
            _rVal = GetByHandle(aProps.Item(2).Value.ToString(), rIndex)
            _rVal.DictionaryIndex = pidx
            Return _rVal
        End Function
        Public Function GetDictionaryMembers(aDictionaryIndex As Integer) As TDXFOBJECTS
            Dim _rVal As TDXFOBJECTS = Clone(bDontCloneMembers:=True)
            Dim aProps As TPROPERTIES
            Dim aMem As TDXFOBJECT
            Dim idx As Integer
            Dim hndl As String
            Dim bProps As TPROPERTIES
            Dim aDic As TDXFOBJECT
            Dim rReactors As TPROPERTIES
            Dim aNm As String
            Dim dhndl As String
            Dim pidx As Integer
            If aDictionaryIndex <= 0 Or aDictionaryIndex > Count Then Return _rVal
            aDic = Item(aDictionaryIndex)
            dhndl = aDic.Props.GCValueStr(5)
            aProps = aDic.Reactors.GetProps("Members", pidx, True)
            rReactors = aProps.Clone
            rReactors.Clear()
            bProps = aProps.GetByGroupCode(3, aFollowerCount:=1)
            For i As Integer = 2 To bProps.Count Step 2
                aNm = bProps.Item(i - 1).Value.ToString()
                hndl = bProps.Item(i).Value.ToString()
                aMem = GetByHandle(hndl, idx)
                If idx > 0 Then
                    aMem.Name = aNm
                    aMem.Props.SetValueGC(330, dhndl)
                    SetItem(idx, aMem)
                    rReactors.Add(New TPROPERTY(bProps.Item(i - 1).GroupCode, aNm, "Entry Name", dxxPropertyTypes.Undefined))
                    rReactors.Add(New TPROPERTY(bProps.Item(i).GroupCode, hndl, "Entry Handle", dxxPropertyTypes.Undefined))
                    aMem.DictionaryIndex = i - 1
                    aMem.Index = idx
                    _rVal.Add(aMem, bSuppressIndexes:=True)
                End If
            Next i
            aDic.Reactors.SetItem(pidx, rReactors)
            SetItem(aDictionaryIndex, aDic)
            Return _rVal
        End Function
        Public Function GetNamedDictionary(aDictionaryName As String, ByRef rIndex As Integer) As TDXFOBJECT
            Dim _rVal As New TDXFOBJECT
            rIndex = 0
            aDictionaryName = Trim(aDictionaryName)
            If Count <= 0 Or aDictionaryName = "" Then Return _rVal
            Dim mem1 As TDXFOBJECT = Item(1)
            If mem1.ObjectType <= 0 Then mem1.ObjectType = dxfObject.ObjectNameToType(mem1.Props.GCValueStr(0))
            If mem1.ObjectType <> dxxObjectTypes.Dictionary Then Return _rVal
            Dim aProps As TPROPERTIES = mem1.Reactors.GetProps("Members", bAddIfNotFound:=True)
            aProps = aProps.GetByGroupCode(3, aDictionaryName, 1)
            If aProps.Count < 2 Then Return _rVal
            _rVal = GetByName(aProps.Item(1).Value.ToString(), dxxObjectTypes.Undefined, rIndex)
            If rIndex <= 0 Then
                _rVal = GetByHandle(aProps.Item(2).Value.ToString(), rIndex:=rIndex)
            End If
            Return _rVal
        End Function
        Public Function GetNamedDictionaryMembers(aDictionaryName As String, ByRef rDictionaryIndex As Integer) As TDXFOBJECTS
            Dim aDic As TDXFOBJECT = GetNamedDictionary(aDictionaryName, rDictionaryIndex)
            If rDictionaryIndex <= 0 Then Return Clone(bDontCloneMembers:=True)
            Return GetDictionaryMembers(rDictionaryIndex)
        End Function
        Public Function GetNamedDictionary(aObjects As TDXFOBJECTS, aDictionaryName As String, ByRef rIndex As Integer) As TDXFOBJECT
            Dim _rVal As New TDXFOBJECT
            rIndex = -1
            aDictionaryName = Trim(aDictionaryName)
            If Count <= 0 Or aDictionaryName = "" Then Return _rVal
            Dim mem1 As TDXFOBJECT = Item(1)
            If mem1.ObjectType <= 0 Then mem1.ObjectType = dxfObject.ObjectNameToType(mem1.Props.GCValueStr(0))
            If mem1.ObjectType <> dxxObjectTypes.Dictionary Then Return _rVal
            Dim aProps As TPROPERTIES = mem1.Reactors.GetProps("Members", bAddIfNotFound:=True)
            aProps = aProps.GetByGroupCode(3, aDictionaryName, 1)
            If aProps.Count < 2 Then Return _rVal
            _rVal = GetByHandle(aProps.Item(2).Value.ToString(), rIndex)
            Return _rVal
        End Function
        Public Function LookUpDictionaryName(aMemberHandle As String, ByRef rParentHandle As String) As String
            Dim _rVal As String = String.Empty
            rParentHandle = ""
            If Count <= 0 Then Return _rVal
            Dim i As Integer
            Dim idx As Integer
            Dim aProps As TPROPERTIES
            Dim aMem As TDXFOBJECT
            For i = 1 To Count
                aMem = Item(i)
                If aMem.ObjectType = dxxObjectTypes.Dictionary Or aMem.ObjectType = dxxObjectTypes.DictionaryWDFLT Then
                    aProps = aMem.Reactors.GetProps("Members", bAddIfNotFound:=True)
                    If aProps.Count > 0 Then
                        aProps.GetByStringValue(aMemberHandle, 1, rIndex:=idx, bPointersOnly:=True)
                        If idx > 0 Then
                            _rVal = aProps.Item(idx).Value.ToString()
                            rParentHandle = aMem.Props.GCValueStr(5)
                            Exit For
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function LookUpHandleByName(aName As String) As String
            Dim _rVal As String = String.Empty
            Dim i As Integer
            Dim aObj As TDXFOBJECT
            Dim rHndl As String = String.Empty
            Dim otype As dxxObjectTypes
            For i = 0 To Count
                aObj = Item(i)
                otype = dxfObject.ObjectNameToType(aObj.Props.GCValueStr(0))
                If otype = dxxObjectTypes.Dictionary Or otype = dxxObjectTypes.DictionaryWDFLT Then
                    If aObj.HasReferenceTo(aName, rHndl) Then
                        _rVal = rHndl
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function LookUpObjectName(aObject As TDXFOBJECT) As String
            Dim rParentIndex As Integer = 0
            Return LookUpObjectName(aObject, rParentIndex)
        End Function
        Public Function LookUpObjectName(aObject As TDXFOBJECT, ByRef rParentIndex As Integer) As String
            Dim _rVal As String = String.Empty
            rParentIndex = -1
            If Count <= 0 Then Return _rVal
            If aObject.Props.Count <= 0 Then Return _rVal
            Dim idx As Integer
            Dim hndl As String = aObject.Props.GCValueStr(5)
            If hndl = "" Then Return _rVal
            Dim ohndl As String = aObject.Props.GCValueStr(330)
            If ohndl = "" Then Return _rVal
            Dim aOwner As TDXFOBJECT
            Dim otype As dxxObjectTypes
            aOwner = GetByHandle(ohndl, idx)
            If idx <= 0 Then Return _rVal
            rParentIndex = idx
            If aOwner.HasReferenceToHandle(hndl, _rVal, idx) Then
                aObject.Name = _rVal
                otype = dxfObject.ObjectNameToType(aObject.Props.GCValueStr(0))
                If otype = dxxObjectTypes.Layout Or otype = dxxObjectTypes.PlotSetting Then
                    aObject.Props.SetValueGC(1, _rVal)
                End If
            End If
            Return _rVal
        End Function
        Public Function MemberExists(aName As String, aObjectType As String, ByRef rIndex As Integer) As Boolean
            Dim _rVal As Boolean
            '#1the object name to search for
            '^tests to see if a object with the same name as the passed object name is present in the collection
            '~is not case dependant and lead and trailing spaces are ignored.
            Dim oname As String
            rIndex = -1
            aObjectType = Trim(aObjectType)
            aName = Trim(aName)
            For i As Integer = 1 To Count
                _Members(i - 1).Index = i
                _Members(i - 1).ImageGUID = ImageGUID
                oname = _Members(i - 1).Props.GCValueStr(0)
                If String.Compare(oname, aObjectType, True) = 0 Then
                    If String.Compare(_Members(i - 1).Name, aName, True) = 0 Then
                        _rVal = True
                        rIndex = i
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function ReplaceMembers(aReplace As TDXFOBJECTS, aObjectType As dxxObjectTypes) As TDXFOBJECTS
            Dim _rVal As TDXFOBJECTS = Clone(bDontCloneMembers:=True)
            Dim j As Integer = 0
            Dim aMem As TDXFOBJECT
            Dim bObjs As TDXFOBJECTS = Clone(bDontCloneMembers:=True)
            For i As Integer = 1 To Count
                aMem = Item(i)
                If dxfObject.ObjectNameToType(aMem.Props.GCValueStr(0)) = aObjectType Then
                    If j = 0 Then
                        j = i
                    End If
                Else
                    bObjs.Add(aMem)
                End If
            Next i
            If j = 0 Then j = bObjs.Count
            For i As Integer = 1 To bObjs.Count
                _rVal.Add(bObjs.Item(i))
                If i = j Then
                    For k As Integer = 1 To aReplace.Count
                        _rVal.Add(aReplace.Item(k))
                    Next k
                    j = 0
                End If
            Next i
            Return _rVal
        End Function
        Public Sub SetImageGUID(aGUID As String)
            ImageGUID = aGUID
            Dim i As Integer
            '**UNUSED VAR** Dim aMem As TDXFOBJECT
            For i = 1 To Count
                _Members(i - 1).ImageGUID = aGUID
            Next i
        End Sub
        Public Function UpdateLayoutsPlotSettings(aPlotSettings As TDXFOBJECT) As TDXFOBJECTS
            Dim _rVal As TDXFOBJECTS = Clone()
            Dim i As Integer
            Dim nm As String
            Dim aObj As TDXFOBJECT
            Dim si1 As Integer
            Dim si2 As Integer
            Dim j As Integer
            Dim k As Integer
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            si2 = aPlotSettings.Props.GroupCodeIndex(1, 1)
            nm = aPlotSettings.Props.GCValueStr(1)
            For i = 1 To Count
                aObj = Item(i)
                aProp = aObj.Props.Item(1)
                bProp = aObj.Props.Item(2)
                If String.Compare(aProp.Value.ToString, "LAYOUT", True) = 0 Then
                    If String.Compare(bProp.Value.ToString, nm, True) = 0 Then
                        If si1 <= 0 Then
                            si1 = aObj.Props.GroupCodeIndex(1, 1)
                        End If
                        k = 0
                        For j = si2 To aPlotSettings.Props.Count
                            aObj.Props.SetVal(si1 + k, aPlotSettings.Props.ValueStr(j))
                            k += 1
                        Next j
                        _rVal.SetItem(i, aObj)
                    End If
                End If
            Next i
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function DEFAULT_SCALES() As TDXFOBJECTS
            Dim _rVal As New TDXFOBJECTS(dxxObjectTypes.Scale)
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A0", "70=0,300=  1,140=1,141=1,290=1,5=B7"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A1", "70=0,300=  2,140=1,141=2,290=0,5=124"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A2", "70=0,300=  4,140=1,141=4,290=0,5=125"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A3", "70=0,300=  5,140=1,141=5,290=0,5=126"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A4", "70=0,300=  8,140=1,141=8,290=0,5=127"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A5", "70=0,300=  10,140=1,141=10,290=0,5=128"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A6", "70=0,300=  16,140=1,141=16,290=0,5=129"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A7", "70=0,300=  20,140=1,141=20,290=0,5=12A"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A8", "70=0,300=  30,140=1,141=30,290=0,5=12B"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "A9", "70=0,300=  40,140=1,141=40,290=0,5=12C"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B0", "70=0,300=  50,140=1,141=50,290=0,5=12D"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B1", "70=0,300=  100,140=1,141=100,290=0,5=12E"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B2", "70=0,300=  1,140=2,141=1,290=0,5=12F"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B3", "70=0,300=  1,140=4,141=1,290=0,5=130"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B4", "70=0,300=  1,140=8,141=1,290=0,5=131"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B5", "70=0,300=   1,140=10,141=1,290=0,5=132"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B6", "70=0,300=100:1,140=100,141=1,290=0,5=133"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B7", "70=0,300=1/128"" = 1'-0"", 140=0.0078125, 141=12, 290=0, 5=134"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B8", "70=0,300=1/64"" = 1'-0"", 140=0.015625, 141=12, 290=0, 5=135"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "B9", "70=0,300=1/32"" = 1'-0"", 140=0.03125, 141=12, 290=0, 5=136"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C0", "70=0,300=1/16"" = 1'-0"", 140=0.0625, 141=12, 290=0, 5=137"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C1", "70=0,300=3/32"" = 1'-0"", 140=0.09375, 141=12, 290=0, 5=138"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C2", "70=0,300=1/8"" = 1'-0"", 140=0.125, 141=12, 290=0, 5=139"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C3", "70=0,300=3/16"" = 1'-0"", 140=0.1875, 141=12, 290=0, 5=13A"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C4", "70=0,300=1/4"" = 1'-0"", 140=0.125, 141=12, 290=0, 5=13B"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C5", "70=0,300=3/8"" = 1'-0"", 140=0.375, 141=12, 290=0, 5=13C"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C6", "70=0,300=1/2"" = 1'-0"", 140=0.5, 141=12, 290=0, 5=13D"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C7", "70=0,300=3/4"" = 1'-0"", 140=0.75, 141=12, 290=0, 5=13E"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C8", "70=0,300=1"" = 1'-0"", 140=1, 141=12, 290=0, 5=13F"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "C9", "70=0,300=1-1/2"" = 1'-0"", 140=1.5, 141=12, 290=0, 5=140"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "D0", "70=0,300=3"" = 1'-0"", 140=3.0, 141=12, 290=0, 5=141"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "D1", "70=0,300=6"" = 1'-0"", 140=6.0, 141=12, 290=0, 5=142"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.Scale, "D2", "70=0,300=1'-0"" = 1'-0"", 140=12, 141=12, 290=0, 5=143"))
            Return _rVal
        End Function
        Public Shared Function DEFAULT_VISUALSTYLES() As TDXFOBJECTS
            Dim _rVal As New TDXFOBJECTS(dxxObjectTypes.VisualStyle)
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "2dWireframe", "2=2dWireframe,70=4,71=0,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=0,66=257,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=0,45=0,5=9F"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "3D Hidden", "2=3D Hidden,70=6,71=1,72=2,73=2,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=2,91=2,64=7,65=257,75=2,175=1,42=40,92=0,66=257,424=0,43=1,76=1,77=6,78=2,67=7,79=3,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=0,45=0,5=A1"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "3dWireframe", "2=3dWireframe,70=5,71=0,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=0,66=257,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=0,45=0,5=A0"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Basic", "2=Basic,70=7,71=1,72=0,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=0,91=4,64=7,65=257,75=1,175=1,42=1,92=8,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=9E"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Brighten", "2=Brighten,70=12,71=2,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=8,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=50,173=0,291=1,45=0,5=A5"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "ColorChange", "2=ColorChange,70=16,71=2,72=2,73=3,90=0,40=-0.6,41=-30,62=5,63=8,421=8421504,74=1,91=4,64=7,65=257,75=1,175=1,142=1,92=8,66=8,424=8421504,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=A9"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Conceptual", "2=Conceptual,70=9,71=3,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=2,91=2,64=7,65=257,75=1,175=1,42=40,92=8,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=3,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=0,45=0,5=A2"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Dim", "2=Dim,70=11,71=2,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=8,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=-50,173=0,291=1,45=0,5=A4"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Facepattern", "2=Facepattern,70=15,71=2,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=8,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=A8"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Flat", "2=Flat,70=0,71=2,72=1,73=1,90=2,40=-0.6,41=30,62=5,63=7,421=16777215,74=0,91=4,64=7,65=257,75=1,175=1,42=1,92=8,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=9A"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "FlatWithEdges", "2=FlatWithEdges,70=1,71=2,72=1,73=1,90=2,40=-0.6,41=30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=0,66=257,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=9B"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Gouraud", "2=Gouraud,70=2,71=2,72=2,73=1,90=2,40=-0.6,41=30,62=5,63=7,421=16777215,74=0,91=4,64=7,65=257,75=1,175=1,42=1,92=0,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=9C"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "GouraudWithEdges", "2=GouraudWithEdges,70=3,71=2,72=2,73=1,90=2,40=-0.6,41=30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=0,66=257,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=9D"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Linepattern", "2=Linepattern,70=14,71=2,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=7,175=7,42=1,92=8,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=A7"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Realistic", "2=Realistic,70=8,71=2,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=0,64=7,65=257,75=1,175=1,42=1,92=8,66=8,424=7895160,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=13,44=0,173=0,291=0,45=0,5=A3"))
            _rVal.Add(New TDXFOBJECT(dxxObjectTypes.VisualStyle, "Thicken", "2=Thicken,70=13,71=2,72=2,73=1,90=0,40=-0.6,41=-30,62=5,63=7,421=16777215,74=1,91=4,64=7,65=257,75=1,175=1,42=1,92=12,66=7,424=0,43=1,76=1,77=6,78=2,67=7,79=5,170=0,171=0,290=0,174=0,93=1,44=0,173=0,291=1,45=0,5=A6"))
            Return _rVal
        End Function
#End Region 'Shared Methods
    End Structure 'TDXFOBJECTS

End Namespace
