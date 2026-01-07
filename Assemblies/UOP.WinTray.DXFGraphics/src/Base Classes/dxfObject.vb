Imports UOP.DXFGraphics.dxfGlobals

Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfObject
        Inherits dxfHandleOwner
        Implements ICloneable
#Region "Members"
        Private _Object As TDXFOBJECT
        Private _Entries As TDICTIONARYENTRIES
#End Region
#Region "Constructors"
        Public Sub New()
            MyBase.New("")
        End Sub
        Friend Sub New(aObjectType As dxxObjectTypes, Optional aName As String = "", Optional aPropertyString As String = "")
            MyBase.New(dxfEvents.NextObjectGUID(aObjectType))
            _Object = New TDXFOBJECT(aObjectType, aName, aPropertyString)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(dxfEvents.NextObjectGUID(aObject.ObjectType, aObject.GUID))
            _Object = aObject
            Handle = aObject.Props.Handle
            If _Object.Props.Count <= 0 Then
                _Object.Props = dxpProperties.Get_ObjectProperties(ObjectType)
            End If
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend Property Entries As TDICTIONARYENTRIES
            Get
                Return _Entries
            End Get
            Set(value As TDICTIONARYENTRIES)
                _Entries = value
            End Set
        End Property
        Friend Property ExtendedData As TPROPERTYARRAY
            Get
                Return _Object.ExtendedData
            End Get
            Set(value As TPROPERTYARRAY)
                _Object.ExtendedData = value
            End Set
        End Property
        Public Property NameGroupCode As Integer
            Get
                Return _Entries.NameGroupCode
            End Get
            Friend Set(value As Integer)
                _Entries.NameGroupCode = value
            End Set
        End Property
        Public Property HandleGroupCode As Integer
            Get
                Return _Entries.HandleGroupCode
            End Get
            Friend Set(value As Integer)
                _Entries.HandleGroupCode = value
            End Set
        End Property
        Public Property ReactorHandle As String
            Get
                Return _Object.Props.GCValueStr(330)
            End Get
            Friend Set(value As String)
                If String.IsNullOrEmpty(value) Then value = "0"
                value = value.Trim
                If value = String.Empty Then value = "0"
                AddReactorHandle(ReactorGUID, value)
            End Set
        End Property
        Public Overrides ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.DXFObject
            End Get
        End Property
        Friend Property Handlez As THANDLES
            Get
                Return MyBase.HStrukture
            End Get
            Set(value As THANDLES)
                MyBase.HStrukture = New THANDLES(value, GUID)
                _Object.Props.Handle = value.Handle
            End Set
        End Property
        Public Overloads Property Index As Integer
            Get
                Return _Object.Index
            End Get
            Friend Set(value As Integer)
                _Object.Index = value
                MyBase.Index = value
            End Set
        End Property
        Public Overrides Property Name As String
            Get
                '^the name assigned to this object
                '~some objects don't have a name
                MyBase.Name = _Object.Name
                Return MyBase.Name
            End Get
            Friend Set(value As String)
                MyBase.Name = value
                _Object.Name = value
            End Set
        End Property
        Public ReadOnly Property ObjectName As String
            Get
                '^the object type name of the object like 'DICTIONARY' or 'LAYOUT'
                Dim _rVal As String = _Object.Props.GCValueStr(0)
                If _rVal = "" Then _rVal = dxfEnums.Description(_Object.ObjectType)
                Return _rVal
            End Get
        End Property
        Public Overrides Property GUID As String
            Get
                Return MyBase.GUID
            End Get
            Friend Set(value As String)
                MyBase.GUID = value
                _Object.Props.GUID = value
            End Set
        End Property
        Public Overrides Property Handle As String
            Get
                Return MyBase.Handle
            End Get
            Friend Set(value As String)
                MyBase.Handle = value
                _Object.Props.Handle = value
            End Set
        End Property
        Public Overrides Property ObjectType As dxxObjectTypes
            Get
                '^the enumeration object type of the object
                Return _Object.ObjectType
            End Get
            Friend Set(value As dxxObjectTypes)
                _Object.ObjectType = value
            End Set
        End Property
        Friend Property Reactors As TPROPERTYARRAY
            Get
                Return _Object.Reactors
            End Get
            Set(value As TPROPERTYARRAY)
                _Object.Reactors = value
            End Set
        End Property
        Friend Property Props As TPROPERTIES
            Get
                Return _Object.Props
            End Get
            Set(value As TPROPERTIES)
                _Object.Props = value
            End Set
        End Property
        Friend Property Strukture As TDXFOBJECT
            Get
                _Object.ImageGUID = ImageGUID
                Return _Object
            End Get
            Set(value As TDXFOBJECT)
                Dim sGUID As String = GUID
                _Object = value
                _Object.GUID = GUID
                MyBase.Name = _Object.Name
            End Set
        End Property
        Friend Overrides Property Suppressed As Boolean
            Get
                '^flag indicating if the object should be  saved
                Return _Object.Suppressed
            End Get
            Set(value As Boolean)
                _Object.Suppressed = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public MustOverride Function Clone() As dxfObject
        Public Function AddReactorHandle(aReactorGUID As String, aHandle As String) As Boolean
            Dim _rVal As Boolean
            If String.IsNullOrEmpty(aHandle) Then aHandle = "0"
            aHandle = aHandle.Trim
            If aHandle = "" Then aHandle = "0"
            _rVal = SetProperty(330, aHandle, aOccurance:=1)
            If aHandle <> "0" And _rVal Then
                TPROPERTIES.ReactorAdd(_Object.Props, 5, aReactorGUID, aHandle)
            End If
            Return _rVal
            '_Object.Props.SetValGC(330, value, aOccurance:=1)
        End Function
        Public Overrides Function ToString() As String
            Dim sName As String = Name
            If sName = "" Then sName = GUID
            Return $"{ObjectName}[{sName}]"
        End Function
        Public Function SetProperty(aNameOrGroupCode As Object, aValue As Object, Optional aOccurance As Integer = 0) As Boolean
            Dim _rVal As Boolean
            Dim aProp As TPROPERTY = _Object.Props.GetMember(aNameOrGroupCode, aOccurance)
            If aProp.Index < 0 Then Return _rVal
            Dim bProp As TPROPERTY = aProp.Clone
            Dim newval As Object
            Dim bDontChange As Boolean
            Dim idx As Integer
            newval = aValue
            _rVal = aProp.SetVal(newval)
            If Not _rVal Then Return _rVal
            Dim aImage As dxfImage = Image
            Dim err As String = String.Empty
            If aImage IsNot Nothing Then
                aImage.RespondToObjectEvent(Me, aProp.GroupCode, aOccurance, aProp.Value, newval, bDontChange, bProp, idx, err)
                If Not bDontChange Then
                    aProp = bProp
                Else
                    Return _rVal
                End If
            End If
            _Object.Props.UpdateProperty = aProp
            If aImage IsNot Nothing And idx > 0 Then
                Dim aObjs As colDXFObjects = aImage.Objex
                aObjs.SetItem(Index, Me)
            End If
            If aProp.GroupCode = 75 Then
                If ObjectType = dxxObjectTypes.Layout Then
                    SetProperty(147, dxfUtils.ConvertPlotScale(TVALUES.To_LNG(aProp.Value)))
                End If
            End If
            Return _rVal
        End Function
        Public Function GetProperty(aNameOrGroupCode As Object, Optional aOccurance As Integer = 0) As dxoProperty
            Dim aProp As TPROPERTY = _Object.Props.GetMember(aNameOrGroupCode, aOccurance)
            If aProp.Name = 0 Then Return Nothing Else Return CType(aProp, dxoProperty)
        End Function
        Public Function PropertyValue(aNameOrGroupCode As Object, Optional aOccurance As Integer = 0) As Object
            Dim aProp As TPROPERTY = _Object.Props.GetMember(aNameOrGroupCode, aOccurance)
            If aProp.Name = "" Then Return String.Empty Else Return aProp.Value
        End Function
        Friend Function TryGetEntry(aNameOrHandle As String, ByRef rEntry As TDICTIONARYENTRY) As Boolean
            Return _Entries.TryGet(aNameOrHandle, rEntry)
        End Function
        Friend Function TryGetEntry(aName As String, aHandle As String, ByRef rEntry As TDICTIONARYENTRY) As Boolean
            Return _Entries.TryGet(aName, aHandle, rEntry)
        End Function
        Friend Function AddEntry(aEntryName As String, aEntryHandle As String) As Boolean
            Dim rEntry As TDICTIONARYENTRY = TDICTIONARYENTRY.Null
            Return AddEntry(aEntryName, aEntryHandle, rEntry)
        End Function
        Friend Function AddEntry(aEntryName As String, aEntryHandle As String, ByRef rEntry As TDICTIONARYENTRY) As Boolean
            Return _Entries.Add(aEntryName, aEntryHandle, rEntry)
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function ObjectNameToType(aObjectName As String) As dxxObjectTypes
            Select Case UCase(Trim(aObjectName))
                Case ""
                    Return dxxObjectTypes.Undefined
                Case "ACAD_PROXY_OBJECT"
                    Return dxxObjectTypes.ProxyObject
                Case "ACDBDICTIONARYWDFLT"
                    Return dxxObjectTypes.DictionaryWDFLT
                Case "ACDBPLACEHOLDER"
                    Return dxxObjectTypes.PlaceHolder
                Case "DATATABLE"
                    Return dxxObjectTypes.DataTable
                Case "DICTIONARY"
                    Return dxxObjectTypes.Dictionary
                Case "DICTIONARYVAR"
                    Return dxxObjectTypes.DictionaryVar
                Case "DIMASSOC"
                    Return dxxObjectTypes.DimAssoc
                Case "ACAD_FIELD"
                    Return dxxObjectTypes.Field
                Case "GROUP"
                    Return dxxObjectTypes.Group
                Case "IDBUFFER"
                    Return dxxObjectTypes.IDBuffer
                Case "IMAGEDEF"
                    Return dxxObjectTypes.ImageDef
                Case "IMAGEDEF_REACTOR"
                    Return dxxObjectTypes.ImageDefReactor
                Case "LAYER_INDEX"
                    Return dxxObjectTypes.LayerIndex
                Case "LAYER_FILTER"
                    Return dxxObjectTypes.LayerFilter
                Case "LAYOUT"
                    Return dxxObjectTypes.Layout
                Case "LIGHTLIST"
                    Return dxxObjectTypes.LightList
                Case "MATERIAL"
                    Return dxxObjectTypes.Material
                Case "MLINESTYLE"
                    Return dxxObjectTypes.MLineStyle
                Case "OBJECT_PTR"
                    Return dxxObjectTypes.ObjectPtr
                Case "PLOTSETTINGS"
                    Return dxxObjectTypes.PlotSetting
                Case "RASTERVARIABLES"
                    Return dxxObjectTypes.RasterVariables
                Case "RENDERENVIRONMENT"
                    Return dxxObjectTypes.RenderEnvironment
                Case "SECTIONMANAGER"
                    Return dxxObjectTypes.SectionManager
                Case "SPATIAL_INDEX"
                    Return dxxObjectTypes.SpatialIndex
                Case "SPATIAL_FILTER"
                    Return dxxObjectTypes.SpatialFilter
                Case "SORTENTSTABLE"
                    Return dxxObjectTypes.SortEntsTable
                Case "SUNSTUDY"
                    Return dxxObjectTypes.SunStudy
                Case "TABLESTYLE"
                    Return dxxObjectTypes.TableStyle
                Case "UNDERLAYDEFINITION"
                    Return dxxObjectTypes.UnderlayDefinition
                Case "VISUALSTYLE"
                    Return dxxObjectTypes.VisualStyle
                Case "VBA_PROJECT"
                    Return dxxObjectTypes.VBAProject
                Case "WIPEOUTVARIABLES"
                    Return dxxObjectTypes.WipeoutVariables
                Case "XRECORD"
                    Return dxxObjectTypes.XRecord
                Case "CELLSTYLEMAP"
                    Return dxxObjectTypes.CellStyleMap
                Case "SCALE"
                    Return dxxObjectTypes.Scale
                Case "MLEADERSTYLE"
                    Return dxxObjectTypes.MLeaderStyle
                Case Else
                    Return dxxObjectTypes.UserDefined
            End Select
        End Function
        Friend Shared Function Create(aObject As TDXFOBJECT, Optional aImageGUID As String = Nothing) As dxfObject
            Dim _rVal As dxfObject
            Select Case aObject.ObjectType
                Case dxxObjectTypes.ProxyObject
                    _rVal = New dxfoProxyObject(aObject)
                Case dxxObjectTypes.DictionaryWDFLT
                    _rVal = New dxfoDictionaryWDFLT(aObject)
                Case dxxObjectTypes.PlaceHolder
                    _rVal = New dxfoPlaceHolder(aObject)
                Case dxxObjectTypes.DataTable
                    _rVal = New dxfoDataTable(aObject)
                Case dxxObjectTypes.Dictionary
                    _rVal = New dxfoDictionary(aObject)
                Case dxxObjectTypes.DictionaryVar
                    _rVal = New dxfoDictionaryVar(aObject)
                Case dxxObjectTypes.DimAssoc
                    _rVal = New dxfoDimAssoc(aObject)
                Case dxxObjectTypes.Field
                    _rVal = New dxfoField(aObject)
                Case dxxObjectTypes.Group
                    _rVal = New dxfoGroup(aObject)
                Case dxxObjectTypes.IDBuffer
                    _rVal = New dxfoIDBuffer(aObject)
                Case dxxObjectTypes.ImageDef
                    _rVal = New dxfoImageDef(aObject)
                Case dxxObjectTypes.LayerIndex
                    _rVal = New dxfoLayerIndex(aObject)
                Case dxxObjectTypes.LayerFilter
                    _rVal = New dxfoLayerFilter(aObject)
                Case dxxObjectTypes.Layout
                    _rVal = New dxfoLayout(aObject)
                Case dxxObjectTypes.LightList
                    _rVal = New dxfoLightList(aObject)
                Case dxxObjectTypes.Material
                    _rVal = New dxfoMaterial(aObject)
                Case dxxObjectTypes.MLineStyle
                    _rVal = New dxfoMLineStyle(aObject)
                Case dxxObjectTypes.ObjectPtr
                    _rVal = New dxfoObjectPtr(aObject)
                Case dxxObjectTypes.PlotSetting
                    _rVal = New dxfoPlotSetting(aObject)
                Case dxxObjectTypes.RasterVariables
                    _rVal = New dxfoRasterVariables(aObject)
                Case dxxObjectTypes.RenderEnvironment
                    _rVal = New dxfoRenderEnvironment(aObject)
                Case dxxObjectTypes.SectionManager
                    _rVal = New dxfoRenderEnvironment(aObject)
                Case dxxObjectTypes.SpatialIndex
                    _rVal = New dxfoSpatialIndex(aObject)
                Case dxxObjectTypes.SpatialFilter
                    _rVal = New dxfoSpatialFilter(aObject)
                Case dxxObjectTypes.SortEntsTable
                    _rVal = New dxfoSortEntsTable(aObject)
                Case dxxObjectTypes.SunStudy
                    _rVal = New dxfoSunStudy(aObject)
                Case dxxObjectTypes.TableStyle
                    _rVal = New dxfoTableStyle(aObject)
                Case dxxObjectTypes.UnderlayDefinition
                    _rVal = New dxfoUnderlayDefinition(aObject)
                Case dxxObjectTypes.VisualStyle
                    _rVal = New dxfoVisualStyle(aObject)
                Case dxxObjectTypes.VBAProject
                    _rVal = New dxfoVBAProject(aObject)
                Case dxxObjectTypes.WipeoutVariables
                    _rVal = New dxfoWipeoutVariables(aObject)
                Case dxxObjectTypes.XRecord
                    _rVal = New dxfoXRecord(aObject)
                Case dxxObjectTypes.CellStyleMap
                    _rVal = New dxfoCellStyleMap(aObject)
                Case dxxObjectTypes.Scale
                    _rVal = New dxfoScale(aObject)
                Case dxxObjectTypes.TableCell
                    _rVal = New dxfoTableCell(aObject)
                Case dxxObjectTypes.MLeaderStyle
                    _rVal = New dxfoMLeaderStyle(aObject)
                Case Else
                    _rVal = New dxfoUserDefined(aObject)
            End Select
            If _rVal IsNot Nothing Then
                If Not String.IsNullOrEmpty(aImageGUID) Then _rVal.ImageGUID = aImageGUID.Trim
            End If
            Return _rVal
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#End Region 'Shared MEthods
    End Class
End Namespace
