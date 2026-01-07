Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Enum dxxEventTypes
        Undefined = 0
        PropertyChange = 1
        EntityCollection = 2
        EntityCollectionMember = 3
        VectorCollection = 4
        Vertex = 5
        DefPt = 6
        PlaneChange = 7
    End Enum


    Public Class dxfPropertyChangeEvent
        Inherits dxfEvent
#Region "Variables"

#End Region 'Variables

#Region "Constructors"


        Public Sub New(aProperty As dxoProperty, aImageGUID As String, aBlockGUID As String, aOwnerGUID As String, aOwnerType As dxxFileObjectTypes, Optional aRefType As dxxReferenceTypes = dxxReferenceTypes.UNDEFINED, Optional aSettingType As dxxSettingTypes = dxxSettingTypes.UNDEFINED)
            MyBase.New(dxxEventTypes.PropertyChange, aCollectionGUID:="", aImageGUID:=aImageGUID, aBlockGUID:=aBlockGUID, aOwnerGUID:=aOwnerGUID, aOwnertType:=aOwnerType, aRefType:=aRefType, aProperty:=aProperty, aSettingType:=aSettingType)

        End Sub
#End Region 'Constructors

    End Class

    Public Class dxfVertexEvent
        Inherits dxfEvent

#Region "Variables"

        Private _EventType As dxxVertexEventTypes
        Private _Vector As dxfVector
        Private _PropertyType As dxxVectorProperties

#End Region 'Variables

#Region "Constructors"


        Friend Sub New(aVertex As dxfVector, aEventType As dxxVertexEventTypes, aCollectionGUID As String, aImageGUID As String, aBlockGUID As String, aEntityGUID As String)
            MyBase.New(dxxEventTypes.Vertex, aCollectionGUID:="", aImageGUID:=aImageGUID, aBlockGUID:=aBlockGUID, aOwnerGUID:=aEntityGUID, aOwnertType:=dxxFileObjectTypes.Entity, aRefType:=dxxReferenceTypes.UNDEFINED, aProperty:=Nothing)
            EventType = aEventType

            Vertex = aVertex

        End Sub


#End Region 'Constructors

#Region "Properties"



        Public Property PropertyType As dxxVectorProperties
            Get
                Return _PropertyType
            End Get
            Friend Set

                _PropertyType = Value
            End Set
        End Property


        Public Property Vertex As dxfVector
            Get
                Return _Vector
            End Get
            Friend Set
                _Vector = Value
            End Set
        End Property
        Public Property EventType As dxxVertexEventTypes
            Get
                Return _EventType
            End Get
            Friend Set
                _EventType = Value
            End Set
        End Property



        Friend Property EntityGUID As String
            '^the guid of the image.Entity that this vector is associated to

            Get
                Return OwnerGUID
            End Get
            Set
                OwnerGUID = Value
            End Set
        End Property



#End Region 'Properties

    End Class

    Public Class dxfDefPtEvent
        Inherits dxfEvent

#Region "Variables"

        Private _EventType As dxxVertexEventTypes
        Private _Vector As dxfVector
        Private _PropertyType As dxxVectorProperties

#End Region 'Variables

#Region "Constructors"


        Friend Sub New(aVertex As dxfVector, aEventType As dxxVertexEventTypes, aCollectionGUID As String, aImageGUID As String, aBlockGUID As String, aEntityGUID As String)
            MyBase.New(dxxEventTypes.DefPt, aCollectionGUID:="", aImageGUID:=aImageGUID, aBlockGUID:=aBlockGUID, aOwnerGUID:=aEntityGUID, aOwnertType:=dxxFileObjectTypes.Entity, aRefType:=dxxReferenceTypes.UNDEFINED, aProperty:=Nothing)
            EventType = aEventType

            Vertex = aVertex

        End Sub


#End Region 'Constructors

#Region "Properties"



        Public Property PropertyType As dxxVectorProperties
            Get
                Return _PropertyType
            End Get
            Friend Set

                _PropertyType = Value
            End Set
        End Property


        Public Property Vertex As dxfVector
            Get
                Return _Vector
            End Get
            Friend Set
                _Vector = Value
            End Set
        End Property
        Public Property EventType As dxxVertexEventTypes
            Get
                Return _EventType
            End Get
            Friend Set
                _EventType = Value
            End Set
        End Property



        Friend Property EntityGUID As String
            '^the guid of the image.Entity that this vector is associated to

            Get
                Return OwnerGUID
            End Get
            Set
                OwnerGUID = Value
            End Set
        End Property



#End Region 'Properties

    End Class


    Public Class dxfVectorsEvent
        Inherits dxfEvent

#Region "Variables"

        Private _EventType As dxxCollectionEventTypes
        Private _Vector As dxfVector
        Private _Vectors As List(Of dxfVector)


#End Region 'Variables

#Region "Constructors"


        Friend Sub New(aEventType As dxxCollectionEventTypes, aCollectionGUID As String, aImageGUID As String, aBlockGUID As String, aEntityGUID As String)
            MyBase.New(dxxEventTypes.VectorCollection, aCollectionGUID:=aCollectionGUID, aImageGUID:=aImageGUID, aBlockGUID:=aBlockGUID, aOwnerGUID:=aEntityGUID, aOwnertType:=dxxFileObjectTypes.Entity, aRefType:=dxxReferenceTypes.UNDEFINED, aProperty:=Nothing)
            EventType = aEventType
        End Sub
#End Region 'Constructors

#Region "Properties"



        Public Property Member As dxfVector
            Get
                Return _Vector
            End Get
            Friend Set
                _Vector = Value
            End Set
        End Property

        Public Property Members As List(Of dxfVector)
            Get
                Return _Vectors
            End Get
            Friend Set
                _Vectors = Value
            End Set
        End Property

        Public Property EventType As dxxCollectionEventTypes
            Get
                Return _EventType
            End Get
            Friend Set
                _EventType = Value
            End Set
        End Property

        Friend Property EntityGUID As String
            '^the guid of the image.Entity that this vector is associated to

            Get
                Return OwnerGUID
            End Get
            Set
                OwnerGUID = Value
            End Set
        End Property


#End Region 'Properties

    End Class

    Public Class dxfEntitiesEvent
        Inherits dxfEvent

#Region "Variables"

        Private _EventType As dxxCollectionEventTypes
        Private _Entity As dxfEntity
        Private _Entities As List(Of dxfEntity)

#End Region 'Variables

#Region "Constructors"


        Friend Sub New(aEventType As dxxCollectionEventTypes, aCollectionGUID As String, aImageGUID As String, aBlockGUID As String, aOwnerGUID As String)
            MyBase.New(dxxEventTypes.EntityCollection, aCollectionGUID:=aCollectionGUID, aImageGUID:=aImageGUID, aBlockGUID:=aBlockGUID, aOwnerGUID:=aOwnerGUID, aOwnertType:=dxxFileObjectTypes.Entities, aRefType:=dxxReferenceTypes.UNDEFINED, aProperty:=Nothing)

            EventType = aEventType

            OptionFlag = False

        End Sub
#End Region 'Constructors

#Region "Properties"


        Public Property ListnerWantsClone As Boolean
            Get
                Return OptionFlag
            End Get
            Set
                OptionFlag = Value
            End Set
        End Property


        Public Property Member As dxfEntity
            Get
                Return _Entity
            End Get
            Friend Set
                _Entity = Value
            End Set
        End Property

        Public Property Members As List(Of dxfEntity)
            Get
                Return _Entities
            End Get
            Friend Set
                _Entities = Value
            End Set
        End Property

        Public Property EventType As dxxCollectionEventTypes
            Get
                Return _EventType
            End Get
            Friend Set
                _EventType = Value
            End Set
        End Property


#End Region 'Properties

    End Class


    Public Class dxfEntityEvent
        Inherits dxfEvent

#Region "Variables"

        Private _EventType As dxxEntityEventTypes
        Private _Member As dxfEntity


#End Region 'Variables

#Region "Contructors"

        Friend Sub New(aMember As dxfEntity, aEventType As dxxEntityEventTypes, aCollectionGUID As String, aImageGUID As String, aBlockGUID As String, aEntityGUID As String, Optional aProperty As dxoProperty = Nothing)
            MyBase.New(dxxEventTypes.EntityCollectionMember, aCollectionGUID:=aCollectionGUID, aImageGUID:=aImageGUID, aBlockGUID:=aBlockGUID, aOwnerGUID:=aEntityGUID, aOwnertType:=dxxFileObjectTypes.Entities, aRefType:=dxxReferenceTypes.UNDEFINED, aProperty:=aProperty)

            EventType = aEventType
            Member = aMember

        End Sub


#End Region 'Constructors

#Region "Properties"

        Public Property Member As dxfEntity
            Get
                Return _Member
            End Get
            Friend Set
                _Member = Value
            End Set
        End Property

        Public Property EventType As dxxEntityEventTypes
            Get
                Return _EventType
            End Get
            Friend Set
                _EventType = Value
            End Set
        End Property

        Private _Entity As dxfEntity
        Friend Property Entity As dxfEntity
            Get
                Return _Entity
            End Get
            Set
                _Entity = Value
            End Set
        End Property
        Friend Property EntityGUID As String
            '^the guid of the image.Entity that this vector is associated to

            Get
                Return OwnerGUID
            End Get
            Set

                OwnerGUID = Value
            End Set
        End Property

        Public Property DirtyOnChange As Boolean
            Get
                Return MyBase.OptionFlag
            End Get
            Friend Set
                MyBase.OptionFlag = Value
            End Set
        End Property

#End Region 'Properties

    End Class

    Public Class dxfPlaneEvent
        Inherits dxfEvent

#Region "Variables"

        Private _EventType As dxxCoordinateSystemEventTypes
        Private _Plane As dxfPlane


#End Region 'Variables

#Region "Contructors"

        Friend Sub New(aEventType As dxxCoordinateSystemEventTypes, Optional aImageGUID As String = Nothing, Optional aOwnerGUID As String = Nothing, Optional aOwnerType As dxxFileObjectTypes = dxxFileObjectTypes.Undefined, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxEventTypes.PlaneChange, aCollectionGUID:="", aImageGUID:=aImageGUID, aBlockGUID:="", aOwnerGUID:=aOwnerGUID, aOwnertType:=aOwnerType, aRefType:=dxxReferenceTypes.UNDEFINED, aProperty:=Nothing)

            EventType = aEventType
            Member = aPlane

        End Sub


#End Region 'Constructors

#Region "Properties"

        Public Property Member As dxfPlane
            Get
                Return _Plane
            End Get
            Friend Set
                If Value IsNot Nothing Then _Plane = New dxfPlane(Value) Else _Plane = Nothing
            End Set
        End Property

        Public Property EventType As dxxCoordinateSystemEventTypes
            Get
                Return _EventType
            End Get
            Friend Set
                _EventType = Value
            End Set
        End Property




#End Region 'Properties

    End Class
End Namespace
