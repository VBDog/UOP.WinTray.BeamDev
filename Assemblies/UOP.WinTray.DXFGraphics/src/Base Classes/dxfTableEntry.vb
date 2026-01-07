Imports System.Reflection
Imports System.Windows.Documents
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfTableEntry
        Inherits dxfHandleOwner
        Implements IDisposable
        'Implements idxfSettingsObject
#Region "Members"
        Friend Values As TVALUES
        Private disposedValue As Boolean
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aStructure As TTABLEENTRY)
            MyBase.New(aStructure.GUID)
            MyBase.Name = aStructure.Name
            Initialize(aStructure)
        End Sub

        Public Sub New(aEntryType As dxxReferenceTypes, Optional aName As String = "", Optional bIsDefault As Boolean = False, Optional aGUID As String = "", Optional aDescription As String = "")

            MyBase.New(dxfEvents.NextEntryGUID(aEntryType, aGUID))
            MyBase.Name = aName
            Inititialize(aEntryType, MyBase.Name, bIsDefault, aGUID, aDescription)
        End Sub

        Public Sub New(aEntry As dxfTableEntry)
            MyBase.New(String.Empty)
            Initialize(aEntry)


        End Sub

        Friend Sub Inititialize(Optional aEntryType As dxxReferenceTypes? = Nothing, Optional aName As String = Nothing, Optional bIsDefault As Boolean = False, Optional aGUID As String = Nothing, Optional aDescription As String = "")
            'init -------------------------------------------------
            If aEntryType.HasValue Then EntryType = aEntryType.Value

            If Not String.IsNullOrWhiteSpace(aName) Then MyBase.Name = aName.Trim()
            If Not String.IsNullOrWhiteSpace(aGUID) Then MyBase.GUID = aGUID.Trim()
            BinaryData = New dxoProperties("BinaryData")
            ExtendedData = New dxoPropertyArray("ExtendedData")
            _Properties = dxpProperties.GetReferenceProperties(EntryType, aName:=MyBase.Name, aDescription:=aDescription, aGUID:=GUID)
            Reactors = New dxoPropertyArray("Reactors")
            Values = New TVALUES(0)
            AutoReset = False
            IsCopied = False
            IsDirty = False
            IsGlobal = False
            IsDefault = bIsDefault
            Suppressed = False
            Index = -1

            'init -------------------------------------------------

        End Sub

        Friend Sub Initialize(aEntry As TTABLEENTRY)
            Dim myguid As String = MyBase.GUID
            If String.IsNullOrWhiteSpace(myguid) Then myguid = dxfEvents.NextEntryGUID(aEntry.EntryType)

            MyBase.Name = aEntry.Name

            BinaryData = New dxoProperties(aEntry.BinaryData, "BinaryData")
            ExtendedData = New dxoPropertyArray(aEntry.ExtendedData, "ExtendedData")
            Reactors = New dxoPropertyArray(aEntry.Reactors, "Reactors")
            If EntryType = dxxReferenceTypes.LTYPE Then
                _Properties = New dxoProperties(aEntry.Props, "Properties")
            Else
                _Properties = dxpProperties.GetReferenceProperties(aEntry.EntryType, aName:=Name, aGUID:=myguid, aProps:=aEntry.Props)
            End If
            _Properties.Name = "Properties"

            Values = New TVALUES(0)
            MyBase.HStrukture = aEntry.Handlez
            MyBase.GUID = myguid
            IsCopied = aEntry.IsCopied
            IsDirty = aEntry.IsDirty
            IsGlobal = aEntry.IsGlobal
            IsDefault = aEntry.IsDefault
            Index = aEntry.Index
            Suppressed = aEntry.Suppressed


            _EntryType = aEntry.EntryType
            _AutoReset = aEntry.AutoReset


            MyBase.Name = aEntry.Name
            Properties.SetVal("Name", MyBase.Name)

            If String.IsNullOrWhiteSpace(GUID) Then GUID = aEntry.GUID
            Properties.GUID = GUID
            Select Case EntryType
                Case dxxReferenceTypes.STYLE
                    Dim aVal As Integer
                    aVal = Properties.ValueI("Bit Code")
                    Do While aVal > 64
                        aVal -= 64
                    Loop
                    Properties.PropValueSet(dxxStyleProperties.XREFRESOLVED, aVal >= 32, bSuppressEvnts:=True) 'XRef is resolved
                    If aVal >= 32 Then aVal -= 32
                    Properties.PropValueSet(dxxStyleProperties.XREFDEPENANT, aVal >= 16, bSuppressEvnts:=True) 'XRef is dependant
                    If aVal >= 16 Then aVal -= 16
                    Properties.PropValueSet(dxxStyleProperties.VERTICAL, aVal >= 4, bSuppressEvnts:=True) 'vetical
                    If aVal >= 4 Then aVal -= 4
                    Properties.PropValueSet(dxxStyleProperties.SHAPEFLAG, aVal >= 1, bSuppressEvnts:=True)
                    aVal = Properties.ValueI("Generation Flag")
                    Dim switchval As Integer = 0
                    If aVal = 4 Or aVal = 6 Then switchval = 1
                    Properties.PropValueSet(dxxStyleProperties.UPSIDEDOWN, switchval, bSuppressEvnts:=True)
                    If aVal = 2 Or aVal = 6 Then switchval = 1 Else switchval = 0

                    Properties.PropValueSet(dxxStyleProperties.BACKWARDS, switchval, bSuppressEvnts:=True)
            End Select
        End Sub

        Private Sub Initialize(aEntry As dxfTableEntry)
            If aEntry Is Nothing Then aEntry = New dxoLayer("0")
            Dim myguid As String = MyBase.GUID
            If String.IsNullOrWhiteSpace(myguid) Then myguid = dxfEvents.NextEntryGUID(aEntry.EntryType)

            MyBase.HStrukture = aEntry.HStrukture

            MyBase.Name = aEntry.Name
            BinaryData = New dxoProperties(aEntry.BinaryData)
            ExtendedData = New dxoPropertyArray(aEntry.ExtendedData)
            Reactors = New dxoPropertyArray(aEntry.Reactors)
            '_Properties = dxpProperties.GetReferenceProperties(aEntry.EntryTypeName, aName:=Name, aGUID:=myguid)
            _Properties = New dxoProperties(aEntry.Properties)
            Values = New TVALUES(0)
            MyBase.GUID = myguid

            IsCopied = aEntry.IsCopied
            IsDirty = aEntry.IsDirty
            IsGlobal = aEntry.IsGlobal
            IsDefault = aEntry.IsDefault
            Index = aEntry.Index
            Suppressed = aEntry.Suppressed


            _EntryType = aEntry.EntryType
            _AutoReset = aEntry.AutoReset


            MyBase.Name = aEntry.Name
            Properties.SetVal("Name", MyBase.Name)

            Properties.GUID = myguid
            Select Case EntryType
                Case dxxReferenceTypes.STYLE
                    Dim aVal As Integer
                    aVal = Properties.ValueI("Bit Code")
                    Do While aVal > 64
                        aVal -= 64
                    Loop
                    Properties.PropValueSet(dxxStyleProperties.XREFRESOLVED, aVal >= 32, bSuppressEvnts:=True) 'XRef is resolved
                    If aVal >= 32 Then aVal -= 32
                    Properties.PropValueSet(dxxStyleProperties.XREFDEPENANT, aVal >= 16, bSuppressEvnts:=True) 'XRef is dependant
                    If aVal >= 16 Then aVal -= 16
                    Properties.PropValueSet(dxxStyleProperties.VERTICAL, aVal >= 4, bSuppressEvnts:=True) 'vetical
                    If aVal >= 4 Then aVal -= 4
                    Properties.PropValueSet(dxxStyleProperties.SHAPEFLAG, aVal >= 1, bSuppressEvnts:=True)
                    aVal = Properties.ValueI("Generation Flag")
                    Dim switchval As Integer = 0
                    If aVal = 4 Or aVal = 6 Then switchval = 1
                    Properties.PropValueSet(dxxStyleProperties.UPSIDEDOWN, switchval, bSuppressEvnts:=True)
                    If aVal = 2 Or aVal = 6 Then switchval = 1 Else switchval = 0

                    Properties.PropValueSet(dxxStyleProperties.BACKWARDS, switchval, bSuppressEvnts:=True)
            End Select
        End Sub
#End Region 'Constructors

#Region "Properties"

        Public ReadOnly Property EntryTypeName As String
            Get
                Return dxfEnums.DisplayName(EntryType)
            End Get
        End Property

        ''' <summary>
        ''' the handle assigned to the object for unique identification in DXF code generation
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property Handle As String

            Get
                Return MyBase.Handle
            End Get
            Friend Set(value As String)

                MyBase.Handle = value
                Properties.Handle = MyBase.Handle
            End Set
        End Property

        ''' <summary>
        ''' the unique identifier assigned to the object
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property GUID As String
            '
            Get
                Return MyBase.GUID
            End Get
            Friend Set(value As String)
                MyBase.GUID = value
                Properties.GUID = MyBase.GUID
            End Set
        End Property



        Private _AutoReset As Boolean

        Public Property AutoReset As Boolean
            Get
                Return _AutoReset
            End Get
            Set(value As Boolean)
                _AutoReset = value
                'If SettingType = dxxSettingTypes.DIMOVERRIDES Then
                '    Dim aImg As dxfImage = Nothing
                '    If GetImage(aImg) Then aImg.BaseSettings_Set(New TTABLEENTRY(Me))
                'End If
            End Set
        End Property

        Private _EntryType As dxxReferenceTypes
        Public Property EntryType As dxxReferenceTypes
            Get
                Return _EntryType
            End Get
            Set(value As dxxReferenceTypes)
                If value = _EntryType Then Return
                _EntryType = value

            End Set
        End Property

        Friend Property IsCopied As Boolean

        Friend Property IsDefault As Boolean

        Friend Property IsGlobal As Boolean

        Public Property IsDirty As Boolean

        Public Property TableHandle As String
            '^the handle of the entry's parent table
            Get
                Return Properties.ValueS("Table Handle")
            End Get
            Friend Set(value As String)
                Properties.SetVal("Table Handle", value)
            End Set
        End Property

        Friend Property BinaryData As dxoProperties

        Friend Property ExtendedData As dxoPropertyArray

        Friend Property Reactors As dxoPropertyArray



        Private _Properties As dxoProperties
        Friend Overridable Property Properties As dxoProperties  'Implements idxfSettingsObject.Properties
            '^returns a copy of the entries current properties
            Get
                If _Properties Is Nothing Then
                    If EntryType <> dxxReferenceTypes.UNDEFINED Then _Properties = dxpProperties.GetReferenceProperties(EntryType, Name, aGUID:=GUID)
                End If

                Return _Properties
            End Get
            Set(value As dxoProperties)
                If _Properties IsNot Nothing Then
                    _Properties.CopyVals(value, bSkipHandles:=True, bSkipPointers:=True, bCopyNewMembers:=True)
                Else
                    If value Is Nothing Then _Properties = dxpProperties.GetReferenceProperties(EntryType, Name, aGUID:=GUID) Else _Properties = value
                End If
            End Set
        End Property


        ''' <summary>
        ''' the name of the entry
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property Name As String
            Get
                If _Properties IsNot Nothing Then
                    Return _Properties.ValueS("Name")
                Else
                    Return String.Empty
                End If
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                value = value.Trim()

                MyBase.Name = value
                Properties.SetVal("Name", MyBase.Name)
            End Set
        End Property

        Public Overrides ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.TableEntry
            End Get
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

#End Region 'Properties
#Region "Methods"

        ''' <summary>
        ''' returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Friend Overridable Function Clone() As dxfTableEntry
            Dim _rVal As dxfTableEntry = dxfTableEntry.CreateEntry(EntryType, "")
            _rVal.ImageGUID = ""
            _rVal.Handle = ""
            _rVal.TableHandle = ""
            Return _rVal
        End Function

        ''' <summary>
        '''searches for a  property by name and returns it's value if it is found 
        ''' </summary>
        ''' <param name="aPropName"></param>
        ''' <returns></returns>
        Public Function GetPropertyValue(aPropName As String) As Object

            If String.IsNullOrWhiteSpace(aPropName) Then Return Nothing
            Dim mem As dxoProperty = Nothing
            If Not Properties.TryGet(aPropName, mem, 1, New List(Of String)({"*"})) Then Return Nothing Else Return mem.Value

        End Function
        Public Overrides Function ToString() As String
            Return $"{dxfEnums.DisplayName(EntryType)} - '{ Name }'"
        End Function


        ''' <summary>
        ''' searchs for a property by the passed enum value
        ''' </summary>
        ''' <param name="aIndex">the enum value to search for</param>
        ''' <param name="aOccur">the occurance factor to apply in the search</param>
        ''' <returns></returns>
        Public Function PropertyGet(aIndex As [Enum], Optional aOccur As Integer = 0) As dxoProperty
            Dim rFound As Boolean = False
            Dim rPropName As String = String.Empty
            Return PropertyGet(aIndex, aOccur, rFound, rPropName)
        End Function
        ''' <summary>
        ''' searchs for a property by the passed enum value
        ''' </summary>
        ''' <param name="aIndex">the enum value to search for</param>
        ''' <param name="aOccur">the occurance factor to apply in the search</param>
        ''' <param name="rFound">returns a boolean value indicationg if the property was found</param>
        ''' <param name="rPropName">returns the name of the property extracted from the description of the paased enum value</param>
        ''' <returns></returns>
        Public Function PropertyGet(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean, ByRef rPropName As String) As dxoProperty
            rFound = False
            Dim _rVal As dxoProperty = Nothing
            Try
                rPropName = dxfEnums.PropertyName(aIndex)
                rFound = Properties.TryGet(rPropName, _rVal)

                Return _rVal
            Catch ex As Exception
                Return Nothing
            End Try
        End Function


        Friend Function PropValueI(aIndex As [Enum], Optional aOccur As Integer = 0, Optional bAbsVal As Boolean = False) As Integer
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return 0
            End If
            If Not bAbsVal Then Return prop.ValueI Else Return Math.Abs(prop.ValueI)

        End Function


        Friend Function PropValueStr(aIndex As [Enum], Optional aOccur As Integer = 0) As String
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return String.Empty
            End If
            Return prop.ValueS

        End Function

        Friend Function PropValueB(aIndex As [Enum], Optional aOccur As Integer = 0) As Boolean
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return False
            End If
            Return prop.ValueB
        End Function

        Friend Function Prop(aGroupCode As Integer, Optional aOccur As Integer = 1) As dxoProperty
            Return Properties.Member(aGroupCode, aOccur)
        End Function

        Friend Function Prop(aName As String, Optional aOccur As Integer = 1) As dxoProperty
            Return Properties.Member(aName, aOccur)
        End Function

        Friend Function PropByEnum(aIndex As [Enum], Optional aOccur As Integer = 1) As dxoProperty
            Return Properties.Member(dxfEnums.PropertyName(aIndex), aOccur)
        End Function


        Friend Function PropValueD(aIndex As [Enum], Optional aOccur As Integer = 0) As Double
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return 0
            End If
            Return prop.ValueD
        End Function
        Friend Overridable Function PropValueSetGC(aGroupCode As Integer, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Then Return False
            Dim aImage As dxfImage = Nothing
            Try
                Dim myprop As dxoProperty = Nothing
                If Not Properties.TryGet(aGroupCode, myprop, aOccur) Then Return False
                Dim oldval As Object = myprop.Value
                Dim _rVal As Boolean = False
                If dxfVectors.TypeIsVector(aValue) Then
                    _rVal = Properties.SetVector(myprop.GroupCode, dxfVectors.GetVector(aValue), aOccur = myprop.Occurance)
                Else
                    _rVal = myprop.SetVal(aValue)
                End If

                If Not _rVal Or bSuppressEvnts Then Return _rVal
                'notify the image
                Dim undo As Boolean
                Return Notify(myprop, oldval, Nothing, undo)
            Catch ex As Exception
                'add an error

                If aImage IsNot Nothing Then
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), $"{Me.GetType()} [{ EntryTypeName() }]", ex)
                Else
                    Throw ex
                End If
                Return False
            End Try

        End Function

        ''' <summary>
        ''' update dependent properties
        ''' </summary>
        Public Overridable Sub UpdateProperties(Optional aImage As dxfImage = Nothing)

        End Sub

        Friend Overridable Function PropValueSetByName(aName As String, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Or String.IsNullOrWhiteSpace(aName) Then Return False

            Dim aImage As dxfImage = MyImage
            Try
                Dim myprop As dxoProperty = Nothing
                If Not Properties.TryGet(aName, myprop, aOccur) Then Return False
                Dim oldval As Object = myprop.Value
                Dim _rVal As Boolean = Properties.SetVal(myprop, aValue)

                If Not _rVal Or bSuppressEvnts Then Return _rVal
                'notify the image
                Dim undo As Boolean
                Return Notify(myprop, oldval, aImage, undo)
            Catch ex As Exception
                'add an error

                If aImage IsNot Nothing Then
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), $"{Me.GetType()} [{ EntryTypeName() }]", ex)
                Else
                    Throw ex
                End If
                Return False
            End Try

        End Function

        Friend Function Notify(aProperty As dxoProperty, aOldValue As Object, ByRef ioImage As dxfImage, ByRef rUndo As Boolean) As Boolean
            rUndo = False
            If aProperty Is Nothing Then Return False
            If ioImage Is Nothing Then
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then
                    GetImage(ioImage)
                Else
                    Return False
                End If
            End If
            If ioImage Is Nothing Then Return False

            If aOldValue Is Nothing Then aOldValue = aProperty.LastValue


            If Not IsCopied Then
                'the style is a member of an image table
                ioImage.RespondToTableMemberEvent(Me, False, New dxoProperty(aProperty), rUndo)
                If rUndo Then
                    aProperty.Value = aOldValue
                    Return False

                End If
                Return True
            Else
                'the style has been copied from the image table and belonges to a entity
                Return False
            End If

        End Function

        Friend Overridable Function PropValueSet(aIndex As [Enum], aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean

            If aValue Is Nothing Then Return False
            Return PropValueSetByName(dxfEnums.PropertyName(aIndex), aValue, aOccur, bSuppressEvnts)

        End Function

        Public Sub ReferenceADD(aReference As String, Optional aGC As Integer = 330)
            aReference = Trim(aReference)
            If aReference <> "" Then
                Reactors.AddReactor("{ACAD_REACTORS", aGC, aReference)
            End If
        End Sub
        Public Sub ReferenceREMOVE(aReference As String)
            Reactors.RemoveMemberPropertiesByStringValue("{ACAD_REACTORS", aReference)
        End Sub
#End Region 'Methods
#Region "Shared Methods"
        Friend Shared Function CreateEntry(aRefType As dxxReferenceTypes, aName As String, Optional aGUID As String = "") As dxfTableEntry

            Select Case aRefType
                Case dxxReferenceTypes.APPID
                    Return New dxoAPPID(aName) With {.GUID = aGUID}
                Case dxxReferenceTypes.BLOCK_RECORD
                    Return New dxoBlockRecord(aName) With {.GUID = aGUID}
                Case dxxReferenceTypes.DIMSTYLE
                    Dim _rVal As New dxoDimStyle(aName) With {.GUID = aGUID}
                    If String.Compare(aName, "Annotative", True) = 0 Then
                        _rVal.PropValueSet(dxxDimStyleProperties.DIMSCALE, 0)
                        _rVal.ExtendedData.Append(dxpProperties.DimStyleExtendedData)
                    End If
                    Return _rVal
                Case dxxReferenceTypes.LAYER
                    Return New dxoLayer(aName) With {.GUID = aGUID}
                Case dxxReferenceTypes.LTYPE
                    Dim _rVal As New dxoLinetype(dxfLinetypes.GetCurrentDefinition(aName)) With {.GUID = aGUID}
                    If String.Compare(aName, dxfLinetypes.Continuous, True) = 0 Then _rVal.Domain = dxxDrawingDomains.Model Else _rVal.Domain = dxxDrawingDomains.Paper
                    _rVal.Suppressed = String.Compare(_rVal.Name, dxfLinetypes.Invisible, True) = 0
                    Return _rVal
                Case dxxReferenceTypes.STYLE
                    Dim _rVal As New dxoStyle(aName) With {.GUID = aGUID}

                    If String.Compare(aName, "Annotative", True) = 0 Then
                        _rVal.ExtendedData.Add(New dxoProperties(dxpProperties.ExtendedData), "AcadAnnotative")
                    End If

                    Return _rVal
                Case dxxReferenceTypes.UCS
                    Return New dxoUCS(aName) With {.GUID = aGUID}
                Case dxxReferenceTypes.VIEW
                    Return New dxoView(aName) With {.GUID = aGUID}
                Case dxxReferenceTypes.VPORT
                    Return New dxoViewPort(aName) With {.GUID = aGUID}
                Case Else
                    Return Nothing
            End Select
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    BinaryData = Nothing
                    ExtendedData = Nothing
                    Properties = Nothing
                    Reactors = Nothing
                    ReleaseReferences()

                    disposedValue = True
                End If
                disposedValue = True
            End If
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region 'Shared Methods
#Region "Operators"
#End Region 'Operators

#Region "Shared Methods"
        Friend Shared Function CopyDimStyleProperties(aToProperties As dxoProperties, aFromProperties As TPROPERTIES, aHeaderPassed As Boolean, bHeaderPassed As Boolean, Optional aSkipList As String = Nothing) As dxoProperties
            Dim rProps As dxoProperties = aToProperties
            Dim enumVals As System.Collections.Generic.Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxDimStyleProperties))
            Dim nm As String
            Dim ival As dxxDimStyleProperties
            Dim bProp As TPROPERTY = TPROPERTY.Null
            Dim aProp As dxoProperty = Nothing
            Dim skipVals As New TLIST(",", aSkipList)
            Dim aprefx As String = String.Empty
            Dim bprefx As String = String.Empty
            If bHeaderPassed Then bprefx = "$"
            If aHeaderPassed Then aprefx = "$"
            For Each nm In enumVals.Keys
                ival = enumVals.Item(nm)
                If Not skipVals.Contains(aprefx & nm) Then
                    If aFromProperties.TryGet(bprefx & nm, rMember:=bProp) Then
                        If rProps.TryGet(aprefx & nm, rMember:=aProp) Then
                            If aProp.SetVal(bProp.Value) Then rProps.IsDirty = True

                        End If
                    End If
                End If
            Next
            Return rProps
        End Function
        Friend Shared Function CopyDimStyleProperties(aToProperties As dxoProperties, aFromProperties As dxoProperties, aHeaderPassed As Boolean, bHeaderPassed As Boolean, Optional aSkipList As String = Nothing) As dxoProperties
            Dim rProps As dxoProperties = aToProperties
            Dim enumVals As System.Collections.Generic.Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxDimStyleProperties))
            Dim nm As String
            Dim ival As dxxDimStyleProperties
            Dim bProp As dxoProperty = Nothing
            Dim aProp As dxoProperty = Nothing
            Dim skipVals As New TLIST(",", aSkipList)
            Dim aprefx As String = String.Empty
            Dim bprefx As String = String.Empty
            If bHeaderPassed Then bprefx = "$"
            If aHeaderPassed Then aprefx = "$"
            For Each nm In enumVals.Keys
                ival = enumVals.Item(nm)
                If Not skipVals.Contains(aprefx & nm) Then
                    If aFromProperties.TryGet(bprefx & nm, rMember:=bProp) Then
                        If rProps.TryGet(aprefx & nm, rMember:=aProp) Then
                            If aProp.SetVal(bProp.Value) Then rProps.IsDirty = True

                        End If
                    End If
                End If
            Next
            Return rProps
        End Function
        Public Shared Function CreateNewReference(aRefType As dxxReferenceTypes, aReferenceName As String) As dxfTableEntry
            If String.IsNullOrWhiteSpace(aReferenceName) Then Return Nothing Else aReferenceName = aReferenceName.Trim()
            Select Case aRefType
                Case dxxReferenceTypes.APPID
                    Return New dxoAPPID(aReferenceName)
                Case dxxReferenceTypes.LTYPE
                    Return New dxoLinetype(dxfLinetypes.GetCurrentDefinition(aReferenceName))
                Case dxxReferenceTypes.LAYER
                    Return New dxoLayer(aReferenceName)
                Case dxxReferenceTypes.STYLE
                    Return New dxoStyle(aReferenceName)
                Case dxxReferenceTypes.DIMSTYLE
                    Return New dxoDimStyle(aReferenceName)
                Case dxxReferenceTypes.VPORT
                    Return New dxoViewPort(aReferenceName)
                Case dxxReferenceTypes.VIEW
                    Return New dxoView(aReferenceName)
                Case dxxReferenceTypes.UCS
                    Return New dxoUCS(aReferenceName)
                Case dxxReferenceTypes.BLOCK_RECORD
                    Return New dxoBlockRecord(aReferenceName)
            End Select
            Return Nothing
        End Function
#End Region 'Shared Methods
    End Class 'dxfTableEntry
End Namespace
