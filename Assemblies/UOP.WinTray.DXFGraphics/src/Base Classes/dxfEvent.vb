Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public MustInherit Class dxfEvent
#Region "Variables"
        Private _CollectionGUID As String
        Private _ImageGUID As String
        Private _BlockGUID As String
        Private _OwnerGUID As String
        Private _OwnerType As dxxFileObjectTypes
        Private _RefType As dxxReferenceTypes
        Private _SettingType As dxxSettingTypes
        Private _Undo As Boolean
        Private _SourceProperty As TPROPERTY
        Private _BaseType As dxxEventTypes
        Private _Block As dxfBlock
        Private _CountChange As Boolean
        Private _OptionFlag As Boolean
        Private _ImageNotified As Boolean
        Private _CollectionNotified As Boolean
        Private _OwnerNotified As Boolean
#End Region 'Variables
#Region "Constructors"
        Public Sub New(aBaseType As dxxEventTypes, Optional aCollectionGUID As String = "", Optional aImageGUID As String = "", Optional aBlockGUID As String = "", Optional aOwnerGUID As String = "", Optional aOwnertType As dxxFileObjectTypes = dxxFileObjectTypes.Undefined, Optional aRefType As dxxReferenceTypes = dxxReferenceTypes.UNDEFINED, Optional aProperty As dxoProperty = Nothing, Optional aSettingType As dxxSettingTypes = dxxSettingTypes.UNDEFINED)
            CollectionGUID = aCollectionGUID
            ImageGUID = aImageGUID
            BlockGUID = aBlockGUID
            OwnerGUID = aOwnerGUID
            OwnerType = aOwnertType
            RefType = aRefType
            SettingType = aSettingType
            SourceProperty = aProperty
            _BaseType = aBaseType
            _Undo = False
            _Block = Nothing
            _CountChange = False
            _OptionFlag = False
            _ImageNotified = False
            _CollectionNotified = False
            _OwnerNotified = False
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property OldValue As Object
            Get
                Return _SourceProperty.LastValue
            End Get
            Friend Set(value As Object)
                _SourceProperty.Value = value
            End Set
        End Property
        Public Property NewValue As Object
            Get
                Return _SourceProperty.Value
            End Get
            Friend Set(value As Object)
                If value Is Nothing Then value = String.Empty
                _SourceProperty.Value = value
            End Set
        End Property
        Public Property PropertyName As String
            Get
                Return _SourceProperty.Name
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _SourceProperty._Name = value
            End Set
        End Property
        Public Property CountChange As Boolean
            Get
                Return _CountChange
            End Get
            Set(value As Boolean)
                _CountChange = value
            End Set
        End Property
        Public Property OptionFlag As Boolean
            Get
                Return _OptionFlag
            End Get
            Set(value As Boolean)
                _OptionFlag = value
            End Set
        End Property
        Friend Property Block As dxfBlock
            Get
                Return _Block
            End Get
            Set(value As dxfBlock)
                _Block = value
            End Set
        End Property
        Public Property SourceProperty As dxoProperty
            Get
                Return New dxoProperty(_SourceProperty)
            End Get
            Set(value As dxoProperty)
                _SourceProperty = New TPROPERTY(value)
            End Set
        End Property
        Friend Property SourcePropertyV As TPROPERTY
            Get
                Return _SourceProperty
            End Get
            Set(value As TPROPERTY)
                _SourceProperty = value
            End Set
        End Property
        Public ReadOnly Property BaseType As dxxEventTypes
            Get
                Return _BaseType
            End Get
        End Property
        Public Property OwnerType As dxxFileObjectTypes
            Get
                Return _OwnerType
            End Get
            Friend Set(value As dxxFileObjectTypes)
                _OwnerType = value
            End Set
        End Property
        Public Property RefType As dxxReferenceTypes
            Get
                Return _RefType
            End Get
            Friend Set(value As dxxReferenceTypes)
                _RefType = value
            End Set
        End Property
        Public Property SettingType As dxxSettingTypes
            Get
                Return _SettingType
            End Get
            Friend Set(value As dxxSettingTypes)
                _SettingType = value
            End Set
        End Property
        Friend Property CollectionGUID As String
            '^the guid of the vector collection that this vector is a member of
            Get
                Return _CollectionGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _CollectionGUID = value
            End Set
        End Property
        Public Property ImageNotified As Boolean
            '^flag indicating that the parent image was notfied of the event
            Get
                Return _ImageNotified
            End Get
            Friend Set(value As Boolean)
                _ImageNotified = value
            End Set
        End Property
        Public Property CollectionNotified As Boolean
            '^flag indicating that the parent collection was notified of the event
            Get
                Return _CollectionNotified
            End Get
            Friend Set(value As Boolean)
                _CollectionNotified = value
            End Set
        End Property
        Public Property OwnerNotified As Boolean
            '^flag indicating that the parent dxf object (entity, block, reference etc.) was notified of the event
            Get
                Return _OwnerNotified
            End Get
            Friend Set(value As Boolean)
                _OwnerNotified = value
            End Set
        End Property
        Friend Property ImageGUID As String
            '^the guid of the image that this vector is associated to
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
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
        Friend Property OwnerGUID As String
            '^the guid of the image.Entity that this vector is associated to
            Get
                Return _OwnerGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _OwnerGUID = value
            End Set
        End Property
        Public Property Undo As Boolean
            '^flag to undo the change which can be set by the event listener to prevent/undo the change
            Get
                Return _Undo
            End Get
            Set(value As Boolean)
                _Undo = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Return dxfEnums.Description(BaseType) & "Event"
        End Function
#End Region 'Methods
    End Class
End Namespace
