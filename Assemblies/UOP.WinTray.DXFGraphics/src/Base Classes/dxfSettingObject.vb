Imports System.Reflection
Imports System.Windows.Documents
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfSettingObject
        Implements IDisposable

#Region "Members"
        Friend Values As TVALUES
        Private disposedValue As Boolean
        Friend OwnerPtr As WeakReference
#End Region 'Members

        Public Event SettingPropertyChange(aEvent As dxfPropertyChangeEvent)

#Region "Constructors"


        Friend Sub New(aSettingType As dxxReferenceTypes, Optional aImage As dxfImage = Nothing, Optional aName As String = "", Optional bIsDefault As Boolean = False, Optional aGUID As String = "", Optional aOwnerGUID As String = "", Optional aSettingsToCopy As dxfSettingObject = Nothing)

            Initialize(aSettingType, aImage, aName, bIsDefault, aGUID, aOwnerGUID, aSettingsToCopy)
        End Sub



        Friend Sub Initialize(Optional aSettingType As dxxSettingTypes? = Nothing, Optional aImage As dxfImage = Nothing, Optional aName As String = Nothing, Optional bIsDefault As Boolean = False, Optional aGUID As String = Nothing, Optional aOwnerGUID As String = Nothing, Optional aSettingsToCopy As dxfSettingObject = Nothing)
            'init -------------------------------------------------
            If aSettingType.HasValue Then _SettingType = aSettingType.Value

            If Not String.IsNullOrWhiteSpace(aName) Then
                aName = aName.Trim()
            Else
                If _SettingType <> dxxSettingTypes.DIMOVERRIDES Then
                    aName = dxfEnums.MemberName(aSettingType)
                End If
            End If

            If Not String.IsNullOrWhiteSpace(aGUID) Then _GUID = aGUID.Trim()
            If String.IsNullOrWhiteSpace(GUID) Then _GUID = dxfEvents.NextSettingGUID(SettingType)
            If Not String.IsNullOrWhiteSpace(aOwnerGUID) Then OwnerGUID = aOwnerGUID.Trim()
            _Properties = dxpProperties.GetSettingsProperties(SettingType, aName:=aName, aGUID:=GUID)
            Values = New TVALUES(0)
            AutoReset = False
            IsCopied = False
            IsDirty = False
            Suppressed = False
            If aSettingsToCopy IsNot Nothing Then Copy(aSettingsToCopy)
            If aImage IsNot Nothing Then
                SetImage(aImage, False)
            End If

            'init -------------------------------------------------

        End Sub



#End Region 'Constructors

#Region "Properties"
        Friend ReadOnly Property HasReferenceTo_Owner As Boolean
            Get
                If String.IsNullOrWhiteSpace(OwnerGUID) Or OwnerPtr Is Nothing Then Return False
                Return OwnerPtr.IsAlive
            End Get
        End Property
        Friend Overridable ReadOnly Property MyOwner As dxfHandleOwner
            Get
                If Not HasReferenceTo_Owner Then Return Nothing
                Dim _rVal As dxfHandleOwner = TryCast(OwnerPtr.Target, dxfHandleOwner)
                If _rVal IsNot Nothing Then
                    If String.Compare(OwnerGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetOwner(Nothing, False)
                End If
                Return _rVal
            End Get
        End Property

        Private _OwnerGUID As String
        Public Property OwnerGUID As String
            Get
                Return _OwnerGUID
            End Get
            Set(value As String)
                If Not String.IsNullOrWhiteSpace(value) Then _OwnerGUID = value.Trim() Else _OwnerGUID = String.Empty
            End Set
        End Property


        Public ReadOnly Property SettingTypeName As String
            Get
                Return dxfEnums.DisplayName(SettingType)
            End Get
        End Property

        Public Overridable Property Name As String
            Get
                Return Properties.ValueS("Name")
            End Get
            Set(value As String)
                Properties.SetVal("Name", value)
            End Set
        End Property

        Friend Property Suppressed As Boolean
        Friend Property Index As Integer

        Private _GUID As String

        ''' <summary>
        ''' a unique identifier assigned to the object
        ''' </summary>
        ''' <returns></returns>
        Public Property GUID As String
            Get
                Return _GUID
            End Get
            Friend Set(value As String)
                _GUID = IIf(String.IsNullOrEmpty(value), String.Empty, value.Trim())
                Properties.GUID = _GUID
            End Set
        End Property
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
                Try
                    _ImagePtr.TryGetTarget(_rVal)
                    If _rVal IsNot Nothing AndAlso _rVal.Disposed Then _rVal = Nothing
                    If _rVal IsNot Nothing Then
                        If String.IsNullOrWhiteSpace(ImageGUID) Or String.Compare(ImageGUID, _rVal.GUID, ignoreCase:=True) <> 0 Then SetImage(Nothing, False)
                    End If
                Catch ex As Exception
                    _ImagePtr = Nothing

                End Try

                Return _rVal
            End Get
        End Property

        Friend Sub SetOwner(aOwner As dxfHandleOwner, bDontReleaseOnNull As Boolean)
            If aOwner IsNot Nothing Then

                OwnerGUID = aOwner.GUID
                OwnerPtr = New WeakReference(aOwner)
            Else
                If Not bDontReleaseOnNull Then

                    OwnerGUID = ""
                    OwnerPtr = Nothing
                End If
            End If
        End Sub



        Private _AutoReset As Boolean

        Public Property AutoReset As Boolean
            Get
                Return _AutoReset
            End Get
            Set(value As Boolean)
                _AutoReset = value

            End Set
        End Property

        Private _SettingType As dxxSettingTypes
        Public ReadOnly Property SettingType As dxxSettingTypes
            Get
                Return _SettingType
            End Get

        End Property

        Friend Property IsCopied As Boolean
        Friend Property IsGlobal As Boolean
        Friend Property IsDefault As Boolean



        Public Property IsDirty As Boolean
            Get
                If _Properties IsNot Nothing Then Return _Properties.IsDirty Else Return False
            End Get
            Friend Set(value As Boolean)
                If _Properties IsNot Nothing Then _Properties.IsDirty = value
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
        Friend Overridable Property Properties As dxoProperties
            '^returns a copy of the entries current properties
            Get
                If _Properties Is Nothing Then
                    If SettingType <> dxxReferenceTypes.UNDEFINED Then _Properties = dxpProperties.GetReferenceProperties(SettingType, String.Empty, aGUID:=GUID)
                End If
                Return _Properties
            End Get
            Set(value As dxoProperties)
                If _Properties IsNot Nothing Then
                    _Properties.CopyVals(value, bSkipHandles:=False, bSkipPointers:=False, bCopyNewMembers:=True)
                Else
                    If value Is Nothing Then _Properties = dxpProperties.GetReferenceProperties(SettingType, String.Empty, aGUID:=GUID) Else _Properties = value
                End If
            End Set
        End Property

        Friend Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage IsNot Nothing Then
                If rImage.Disposed Then rImage = Nothing
            End If
            If rImage Is Nothing Then
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then
                    rImage = MyImage
                    If rImage Is Nothing Then rImage = dxfEvents.GetImage(ImageGUID)

                End If
                Return SetImage(rImage, True)
            End If

            Return SetImage(rImage, True)

        End Function

        Private _StoredProperties As List(Of dxoProperties)
        Private Property StoredProperties As List(Of dxoProperties)
            Get
                If _StoredProperties Is Nothing Then _StoredProperties = New List(Of dxoProperties)
                Return _StoredProperties
            End Get
            Set(value As List(Of dxoProperties))
                _StoredProperties = value
            End Set
        End Property




        Private _ImageGUID As String

        Public Property ImageGUID As String
            '^the GUID of the image that owns this object
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                Dim newval As Boolean = String.Compare(value, ImageGUID, ignoreCase:=True)
                If Not newval Then Return
                _ImageGUID = value
                _ImagePtr = dxfEvents.GetImagePtr(_ImageGUID)
            End Set
        End Property

#End Region 'Properties
#Region "Methods"
        Friend _ImagePtr As WeakReference(Of dxfImage)
        Friend Function SetImage(aImage As dxfImage, bDontReleaseOnNull As Boolean) As Boolean
            Dim img As dxfImage = aImage
            If img IsNot Nothing AndAlso img.Disposed Then
                img = Nothing
                Return False
            End If
            If img IsNot Nothing Then
                ImageGUID = img.GUID
                _ImagePtr = New WeakReference(Of dxfImage)(img)
                Return True
            Else
                If Not bDontReleaseOnNull Then
                    ImageGUID = ""
                    _ImagePtr = Nothing
                End If
                Return False
            End If
        End Function
        Friend Overridable Function Clone() As dxfTableEntry
            '^returns a new object with properties matching those of the cloned object
            Dim _rVal As dxfTableEntry = dxfTableEntry.CreateEntry(SettingType, "")
            _rVal.ImageGUID = ""
            _rVal.Handle = ""
            _rVal.TableHandle = ""
            Return _rVal
        End Function


        Friend Overridable Function Copy(aSettings As dxfSettingObject) As Boolean
            If aSettings Is Nothing Then Return False
            If aSettings.SettingType <> SettingType Or _Properties Is Nothing Then Return False
            Return _Properties.CopyVals(aSettings.Properties, bSkipHandles:=False, bSkipPointers:=False)
        End Function

        Public Function GetPropertyValue(aPropName As String) As Object
            '^searches for a  property by name and returns it's value if it is found
            If String.IsNullOrWhiteSpace(aPropName) Then Return Nothing
            Dim mem As dxoProperty = Nothing

            If Not Properties.TryGet(aPropName, mem, 1, New List(Of String)({"*", "$"})) Then Return Nothing Else Return mem.Value


            Dim _rVal As Object = Properties.Value(aPropName)
            If _rVal Is Nothing Then
                If Not aPropName.StartsWith("*") Then
                    _rVal = Properties.Value("*" & aPropName)
                End If
            End If
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return $"{dxfEnums.DisplayName(SettingType)} - '{ ImageGUID }'"
        End Function
        Public Function PropertyGet(aIndex As [Enum], Optional aOccur As Integer = 0) As dxoProperty
            Dim rFound As Boolean = False
            Dim rPropName As String = String.Empty
            Return PropertyGet(aIndex, aOccur, rFound, rPropName)
        End Function
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


        Friend Function Structure_Get() As TTABLEENTRY
            Return New TTABLEENTRY(Me)
        End Function
        Friend Function PropValueI(aIndex As [Enum], Optional aOccur As Integer = 0, Optional bAbsVal As Boolean = False) As Integer
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return 0
            End If
            If Not bAbsVal Then Return prop.ValueI Else Return Math.Abs(prop.ValueI)

        End Function


        Friend Function PropValueStr(aIndex As [Enum], Optional aOccur As Integer = 0, Optional aDefault As String = "", Optional bReturnDefaultForNullString As Boolean = False) As String
            Dim prop As dxoProperty = Nothing
            If Not Properties.TryGet(dxfEnums.PropertyName(aIndex), prop, aOccur) Then
                Return aDefault
            End If
            Dim _rVal As String = prop.ValueS
            If bReturnDefaultForNullString And String.IsNullOrWhiteSpace(_rVal) Then Return aDefault
            Return _rVal

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
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), $"{Me.GetType()} [{ SettingTypeName() }]", ex)
                Else
                    Throw ex
                End If
                Return False
            End Try

        End Function

        Friend Overridable Function PropValueSetByName(aName As String, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Or String.IsNullOrWhiteSpace(aName) Then Return False
            Dim aImage As dxfImage = MyImage
            Try
                Dim myprop As dxoProperty = Nothing
                If Not Properties.TryGet(aName, myprop, aOccur) Then Return False
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
                Return Notify(myprop, oldval, aImage, undo)
            Catch ex As Exception
                'add an error

                If aImage IsNot Nothing Then
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), $"{Me.GetType()} [{ SettingTypeName() }]", ex)
                Else
                    Throw ex
                End If
                Return False
            End Try

        End Function
        Protected Friend Sub RaisePropertyChangeEvent(aProp As dxoProperty)
            If aProp Is Nothing Then Return
            Dim pchangeevent As New dxfPropertyChangeEvent(New dxoProperty(aProp), ImageGUID, String.Empty, String.Empty, String.Empty, aSettingType:=SettingType)
            RaiseEvent SettingPropertyChange(pchangeevent)
            If Not String.IsNullOrWhiteSpace(OwnerGUID) Then
                Dim owner As dxfHandleOwner = MyOwner
                If owner IsNot Nothing Then
                    owner.RespondToDimStylePropertyChange(pchangeevent)
                End If
            End If
        End Sub

        Friend Function Notify(aProperty As dxoProperty, Optional aOldValue As Object = Nothing) As Boolean
            Dim rUndo As Boolean = False
            If aProperty Is Nothing Then Return False
            Dim ioImage As dxfImage = Nothing
            If Not GetImage(ioImage) Then Return False
            Dim eventprop As New dxoProperty(aProperty)
            If aOldValue Is Nothing Then aOldValue = aProperty.LastValue Else eventprop.LastValue = aOldValue
            ioImage.RespondToSettingChange(Me, eventprop)

            If rUndo Then
                aProperty.Value = aOldValue
                Return False
            Else
                Return True
            End If
        End Function

        Friend Function Notify(aProperty As dxoProperty, aOldValue As Object, ByRef ioImage As dxfImage, ByRef rUndo As Boolean) As Boolean
            rUndo = False
            If aProperty Is Nothing Then Return False
            If Not GetImage(ioImage) Then Return False
            Dim eventprop As New dxoProperty(aProperty)
            If aOldValue Is Nothing Then aOldValue = aProperty.LastValue Else eventprop.LastValue = aOldValue
            ioImage.RespondToSettingChange(Me, eventprop)

            If rUndo Then
                aProperty.Value = aOldValue
                Return False
            Else
                Return True
            End If
        End Function

        Friend Overridable Function PropValueSet(aIndex As [Enum], aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean

            If aValue Is Nothing Then Return False

            Return PropValueSetByName(dxfEnums.PropertyName(aIndex), aValue, aOccur, bSuppressEvnts)

        End Function


#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function ValidateHeaderPropertyChange(aImageGUID As String, aProp As dxoProperty, ByRef ioNewValue As Object, ByRef rError As String, Optional aImage As dxfImage = Nothing) As Boolean
            Dim _rVal As Boolean
            rError = String.Empty
            If aProp Is Nothing Then Return False
            If String.IsNullOrWhiteSpace(aProp.Name) Then Return False
            _rVal = True
            '^catchs and halts invalid header variable values
            Try

                Dim aName As String = aProp.Name.ToUpper.Trim()
                Dim bInvalid As Boolean
                Dim bHid As Boolean
                Dim enu As dxxHeaderVars
                If aImage Is Nothing Then
                    If aImageGUID <> "" Then aImage = dxfEvents.GetImage(aImageGUID)
                End If
                Dim bImg As Boolean = aImage IsNot Nothing

                bHid = aName.StartsWith("*")
                If Not bHid Then
                    enu = aProp.Index - 1
                Else
                    enu = -aProp.Index
                End If
                Select Case enu
        '========================================================================================
                    Case dxxHeaderVars.LTSCALE, dxxHeaderVars.CELTSCALE
                        '========================================================================================
                        ioNewValue = TVALUES.To_DBL(ioNewValue)
                        bInvalid = ioNewValue <= 0
                        If bInvalid Then rError = "LT Scale Values Must Be Greater Than 0"
        '========================================================================================
                    Case dxxHeaderVars.CELTYPE
                        '========================================================================================
                        ioNewValue = ioNewValue.ToString().Trim()
                        If ioNewValue = "" Then ioNewValue = dxfLinetypes.ByLayer
                        bInvalid = Not TLISTS.Contains(ioNewValue, "ByLayer,ByBlock,Continuous")
                        If Not bInvalid And bImg Then
                            Dim lt As dxoLinetype = aImage.TableEntry(dxxReferenceTypes.LTYPE, ioNewValue.ToString())
                            bInvalid = lt Is Nothing
                            If Not bInvalid Then ioNewValue = lt.Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown Linetype({ ioNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.LWSCALE
                        '========================================================================================
                        ioNewValue = TVALUES.ToDouble(ioNewValue, True, aProp.LastValue, aMaxVal:=1, aValueControl:=mzValueControls.PositiveNonZero)
        '========================================================================================
                    Case dxxHeaderVars.DIMSTYLE
                        '========================================================================================
                        ioNewValue = ioNewValue.ToString().Trim()
                        If ioNewValue = "" Then ioNewValue = "Standard"
                        bInvalid = ioNewValue.ToString().ToUpper() <> "STANDARD"
                        If Not bInvalid And bImg Then
                            bInvalid = Not aImage.DimStyles.MemberExists(ioNewValue)
                            If Not bInvalid Then ioNewValue = aImage.DimStyle(aName).Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown Dim Style({ ioNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.PLINEWID
                        '========================================================================================
                        ioNewValue = TVALUES.To_DBL(ioNewValue)
                        bInvalid = ioNewValue < 0
                        If bInvalid Then rError = $"Invalid Polyline Width({ ioNewValue }) Requested"
        '========================================================================================
                    Case dxxHeaderVars.TEXTSTYLE
                        '========================================================================================
                        ioNewValue = ioNewValue.ToString().Trim()
                        If ioNewValue = "" Then ioNewValue = "Standard"
                        bInvalid = ioNewValue.ToString().ToUpper() <> "STANDARD"
                        If bInvalid And bImg Then
                            bInvalid = Not aImage.Styles.MemberExists(ioNewValue)
                            If Not bInvalid Then ioNewValue = aImage.TextStyle(ioNewValue).Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown Text Style({ ioNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.CLAYER
                        '========================================================================================
                        ioNewValue = ioNewValue.ToString().Trim()
                        If ioNewValue = "" Then ioNewValue = "0"
                        bInvalid = ioNewValue <> "0"
                        If bInvalid And bImg Then
                            bInvalid = Not aImage.Layers.MemberExists(ioNewValue)
                            If Not bInvalid Then ioNewValue = aImage.Layers(ioNewValue).Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown Layer({ ioNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.PDMODE
                        '========================================================================================
                        If (ioNewValue < 0 Or ioNewValue > 4) Then
                            If ioNewValue < 32 Or ioNewValue > 36 Then
                                If ioNewValue < 64 Or ioNewValue > 68 Then
                                    If ioNewValue < 96 Or ioNewValue > 100 Then
                                        bInvalid = True
                                        rError = "Invaid PDMode Value"
                                    End If
                                End If
                            End If
                        End If
        '========================================================================================
                    Case dxxHeaderVars.DIMLTYPE, dxxHeaderVars.DIMLTEX1, dxxHeaderVars.DIMLTEX2
                        '========================================================================================
                        ioNewValue = ioNewValue.ToString().Trim()
                        If ioNewValue = "" Then ioNewValue = dxfLinetypes.ByBlock
                        bInvalid = Not TLISTS.Contains(ioNewValue, "ByLayer,ByBlock,Continuous")
                        If Not bInvalid And bImg Then
                            bInvalid = Not aImage.Linetypes.MemberExists(ioNewValue)
                            If Not bInvalid Then ioNewValue = aImage.Linetypes.Entry(ioNewValue).Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown/Unloaded Linetype({ ioNewValue }) Requested"
                        End If
                End Select
                If Not bInvalid Then
                    _rVal = True
                End If
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function

        ''' <summary>
        ''' saves a copy of the current properties so they can be reset at a later time
        ''' </summary>
        Public Sub StoreProperties()
            StoredProperties.Add(New dxoProperties(Properties))
        End Sub

        ''' <summary>
        ''' restores the last stored properties
        ''' </summary>
        Public Sub RestoreProperties()
            Dim cnt As Integer = StoredProperties.Count
            If cnt <= 0 Then Return

            Dim props As dxoProperties = StoredProperties(cnt - 1)
            StoredProperties.RemoveAt(cnt - 1)

            Properties.CopyVals(props, bSkipHandles:=False, bSkipPointers:=False)

        End Sub

        Friend Shared Function CreateSetting(aRefType As dxxReferenceTypes, aName As String, Optional aGUID As String = "") As dxfSettingObject

            Select Case aRefType
                Case dxxSettingTypes.HEADER
                    Return New dxsHeader() With {.GUID = aGUID}

                Case Else
                    Return Nothing
            End Select
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then

                    Properties = Nothing
                    ReleaseReferences()
                    If _StoredProperties IsNot Nothing Then _StoredProperties.Clear()
                    _StoredProperties = Nothing
                    disposedValue = True
                End If
                disposedValue = True
            End If
        End Sub

        Friend Overridable Sub ReleaseReferences()

            ImageGUID = String.Empty
            _ImagePtr = Nothing

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
