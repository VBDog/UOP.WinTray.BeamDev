Namespace UOP.DXFGraphics
    'Defines an object that subscries to receive messages
    Friend Interface IEventSubscriber(Of T)
        Sub OnAggregateEvent(message As Object)
    End Interface
End Namespace
