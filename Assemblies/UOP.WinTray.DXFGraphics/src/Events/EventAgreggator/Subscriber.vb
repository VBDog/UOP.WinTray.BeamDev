Imports System.Reflection
Namespace UOP.DXFGraphics
    Public Class Subscriber
        Inherits DisposableObject
        Private _ObjectPointer As WeakReference
        Private _EventHandlingMethods As New Dictionary(Of Type, MethodInfo)
        Protected Friend ReadOnly Property EventMessageTypes As Dictionary(Of Type, MethodInfo)
            Get
                Return _EventHandlingMethods
            End Get
        End Property
        'Defines a subscriber and the message types it listens for.
        Public Sub New(aSubscriber As Object)
            If aSubscriber Is Nothing Then Throw New Exception("Subscribers to the event aggregator have to be alive.")
            _ObjectPointer = New WeakReference(aSubscriber)
            Dim ifaces = aSubscriber.GetType().GetInterfaces()
            Dim eventSubscriberInterfaces As New List(Of Type)
            For Each iface In ifaces
                'Dim ifo = iface.GetTypeInfo
                If iface.Name.ToUpper().StartsWith("IEVENTSUBSCRIBER") Then
                    'If String.Compare(iface.Name.Substring(0, 16), "IEventSubscriber", True) = 0 Then
                    eventSubscriberInterfaces.Add(iface)
                End If
            Next
            '= From x In aSubscriber.GetType().GetInterfaces() Where GetType(IEventSubscriber(Of T)).IsAssignableFrom(x) And x.GetTypeInfo().IsGenericType
            If Not eventSubscriberInterfaces.Any() Then
                Throw New Exception("The subscriber needs to implement IEventSubscriber(of T) in order to receive messages from the event aggregator.")
            End If
            'Dim ptype = aSubscriber.GetType
            'Dim parentmethods = ptype.GetMethods.ToList.FindAll(Function(x) x.Name.ToUpper.StartsWith("ONAGGREGATEEVENT"))
            'For Each methodinfo In parentmethods
            '    If methodinfo.Name.ToUpper.StartsWith("ONAGGREGATEEVENT") Then
            '        Console.WriteLine(methodinfo.Name)
            '    End If
            'Next
            For Each ifacetype In eventSubscriberInterfaces
                Dim methods = ifacetype.GetMethods
                For Each method In methods
                    If method.Name.ToUpper.StartsWith("ONAGGREGATEEVENT") Then
                        'Console.WriteLine(method.Name)
                        Dim dclr = method.DeclaringType
                        Dim genargs = dclr.GetGenericArguments()
                        If genargs.Length > 0 Then
                            Dim messageType = genargs(0)
                            _EventHandlingMethods(messageType) = method
                            ''Dim eventHandlingMethod = method.GetGenericMethodDefinition ' ifacetype.GetMethod(NameOf(IEventSubscriber(Of Object).OnAggregateEvent), types:=New Type() {messageType})
                        End If
                    End If
                Next
            Next
            'For Each eventSubscriberInterface In eventSubscriberInterfaces
            '    Dim args = eventSubscriberInterface.GetGenericArguments()
            '    Dim messageType = eventSubscriberInterface.GetGenericArguments()(0)
            '    Dim eventHandlingMethod = eventSubscriberInterface.GetMethod(NameOf(IEventSubscriber(Of Object).OnAggregateEvent), types:=New Type() {messageType})
            '    If eventHandlingMethod  IsNot Nothing Then
            '        _EventHandlingMethods(messageType) = eventHandlingMethod
            '    End If
            'Next
        End Sub
        'check to see if the actual object represented by the notion of a subscriber is still alive and not GC'd
        Public ReadOnly Property IsAlive As Boolean
            Get
                If _ObjectPointer Is Nothing Then Return False
                Return _ObjectPointer.IsAlive
            End Get
        End Property
        Public Function Matches(otherSubscriber As Object) As Boolean
            If Not IsAlive Then Return False
            Return _ObjectPointer.Target Is otherSubscriber
        End Function
        ' A message is relayed to the subscriber so they can handle it as needed.
        Public Function ReceiveMessage(message As Object) As Boolean
            Dim messageType = message.GetType()
            If (Not _EventHandlingMethods.ContainsKey(messageType)) Then Return False
            If (Not IsAlive) Then Return False
            For Each eventHandlingMethod In _EventHandlingMethods
                Dim methodParameter = eventHandlingMethod.Key
                Dim methodPointer = eventHandlingMethod.Value
                If methodParameter.IsAssignableFrom(messageType) Then
                    methodPointer.Invoke(_ObjectPointer.Target, parameters:=New Object() {message})
                    Return True
                End If
                Try
                Catch ex As Exception
                    Return False
                End Try
            Next
            Return True
        End Function
        'Checks to see if the subscriber is listening for a message of particular type
        Public Function IsListeningForMessageType(messageType As Type) As Boolean
            If _EventHandlingMethods Is Nothing Then Return False
            Return _EventHandlingMethods.ContainsKey(messageType)
        End Function
        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                _ObjectPointer = Nothing
                If _EventHandlingMethods IsNot Nothing Then _EventHandlingMethods.Clear()
                _EventHandlingMethods = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
