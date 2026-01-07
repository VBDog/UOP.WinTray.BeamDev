Imports System.Collections.Concurrent
Namespace UOP.DXFGraphics
    'An object that can route messages to 0, 1 or many subscribers.
    Friend Class EventAggregator
        Inherits DisposableObject
        Private subscribers As ConcurrentBag(Of Subscriber)
        Friend Sub New()
            subscribers = New ConcurrentBag(Of Subscriber)
        End Sub
        'Publishes a particular type of message to all subscribers that implement <see cref="IEventSubscriber{T}"/>.
        'The subscribers will receive the message as long as the type specified in
        'their implementation of <see cref="IEventSubscriber{T}"/> matches the type of
        'the published message
        Public Sub Publish(Of T)(message As T)
            If message Is Nothing Then Throw New ArgumentNullException(paramName:=NameOf(message), message:="Unable to publish a null message")
            Dim subscribersToNotify = subscribers.ToArray()
            Dim messageType = message.GetType()
            Dim deadSubscribers As New List(Of Subscriber)
            For Each subscriber In subscribersToNotify
                If subscriber.IsListeningForMessageType(messageType) Then
                    If Not subscriber.ReceiveMessage(message) Then deadSubscribers.Add(subscriber)
                End If
            Next
            If deadSubscribers.Any Then
                Dim i As Integer
                For i = deadSubscribers.Count - 1 To 0 Step -1
                    Dim deadSubscriber = deadSubscribers.Item(i)
                    deadSubscribers.RemoveAt(i)
                    subscribers = New ConcurrentBag(Of Subscriber)(collection:=subscribers.Except(New Subscriber() {deadSubscriber}))
                    deadSubscriber.Dispose()
                Next i
            End If
        End Sub
        'Subscribes an object so that object can receive messages of type T, as long as the object implements <see cref="IEventSubscriber{T}"/>.
        Public Sub Subscribe(subscriber As Object)
            If subscriber Is Nothing Then Return 'Throw New ArgumentNullException(paramName:=NameOf(subscriber), message:="Unable to subscribe null objects")
            If subscribers.Any(Function(x) x.Matches(subscriber)) Then Return
            subscribers.Add(New Subscriber(subscriber))
        End Sub
        'Checks the list of subscribers to see if any one of them can handle the message type
        Public Function SubscriberExistFor(messageType As Type) As Boolean
            Return subscribers.Any(Function(x) x.IsListeningForMessageType(messageType))
        End Function
        'Removes all message subscriptions for given object.
        Public Sub Unsubscribe(subscriber As Object)
            If subscriber Is Nothing Then Return ' Throw New ArgumentNullException(paramName:=NameOf(subscriber), message:="Unable to subscribe null objects")
            Dim found = subscribers.FirstOrDefault(Function(x) x.Matches(subscriber))
            If found IsNot Nothing Then
                subscribers = New ConcurrentBag(Of Subscriber)(collection:=subscribers.Except(New Subscriber() {found}))
            End If
        End Sub
        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                For i As Integer = subscribers.Count - 1 To 0 Step -1
                    Dim deadSubscriber = subscribers.ElementAt(i)
                    subscribers = New ConcurrentBag(Of Subscriber)(collection:=subscribers.Except(New Subscriber() {deadSubscriber}))
                    deadSubscriber.Dispose()
                Next
                subscribers = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
