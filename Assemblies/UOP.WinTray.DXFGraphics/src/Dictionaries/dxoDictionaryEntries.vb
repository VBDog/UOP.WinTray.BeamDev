Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoDictionaryEntries
        Inherits List(Of dxoDictionaryEntry)
        Implements ICloneable
        Implements IEnumerable(Of dxoDictionaryEntry)

#Region "Constructors"



        Friend Sub New(aEntry As TDICTIONARYENTRY)
            'init =============
            _NameGC = 0
            _HandleGC = 0

            'init =============
            Add(New dxoDictionaryEntry(aEntry))
        End Sub
        Public Sub New(aEntry As dxoDictionaryEntry)
            'init =============
            _NameGC = 0
            _HandleGC = 0

            'init =============
            If aEntry Is Nothing Then Return
            Add(aEntry)
        End Sub
        Public Sub New()
            'init ------------------
            _NameGC = 0
            _HandleGC = 0

            'init ------------------


        End Sub

        Public Sub New(aNameGC As Integer, aHandleGC As Integer)
            'init ------------------
            _NameGC = aNameGC
            _HandleGC = aHandleGC

            'init ------------------


        End Sub

        Friend Sub New(aDictionary As TDICTIONARYENTRIES)
            'init ------------------
            _NameGC = aDictionary.NameGroupCode
            _HandleGC = aDictionary.HandleGroupCode

            'init ------------------
            For i As Integer = 1 To aDictionary.Count
                Add(New dxoDictionaryEntry(aDictionary.Item(i)))
            Next

        End Sub
        Public Sub New(aDictionary As dxoDictionaryEntries)
            'init ------------------
            _NameGC = 0
            _HandleGC = 0

            'init ------------------
            If aDictionary Is Nothing Then Return
            _NameGC = aDictionary.NameGroupCode
            _HandleGC = aDictionary.HandleGroupCode
            For i As Integer = 1 To aDictionary.Count
                Add(New dxoDictionaryEntry(aDictionary.Item(i)))
            Next

        End Sub
#End Region 'Constructors

#Region "Properties"
        Private _NameGC As Integer
        Public Property NameGroupCode As Integer
            Get
                Return _NameGC
            End Get
            Set(value As Integer)
                _NameGC = value
            End Set
        End Property

        Private _HandleGC As Integer
        Public Property HandleGroupCode As Integer
            Get
                Return _HandleGC
            End Get
            Set(value As Integer)
                _HandleGC = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Clone() As dxoDictionaryEntries
            Return New dxoDictionaryEntries(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoDictionaryEntries(Me)
        End Function
        Public Overrides Function ToString() As String
            Return $"TDICTIONARYENTRIES[{ Count }]"
        End Function

        Public Overloads Sub Add(aEntry As dxoDictionaryEntry)
            If aEntry IsNot Nothing Then Return
            MyBase.Add(aEntry)
        End Sub

        Public Function AddUnique(aName As String, aHandle As String) As Boolean
            If String.IsNullOrEmpty(aHandle) Then Return False
            If aHandle = "0" Then Return False

            Dim aentry As dxoDictionaryEntry = Nothing
            If TryGet(aName, aHandle, aentry) Then Return False

            Add(New dxoDictionaryEntry(aName, aHandle))

            Return True
        End Function

        Public Function TryGet(aName As String, aHandle As String, ByRef rEntry As dxoDictionaryEntry) As Boolean
            rEntry = New dxoDictionaryEntry("", "")
            Dim namePassed As Boolean = Not String.IsNullOrEmpty(aName)
            Dim handlePassed As Boolean = Not String.IsNullOrEmpty(aHandle)
            If Not namePassed And Not handlePassed Then Return False
            For i As Integer = 1 To Count
                If namePassed And handlePassed Then
                    If String.Compare(MyBase.Item(i - 1).Name, aName, ignoreCase:=True) = 0 Or String.Compare(MyBase.Item(i - 1).Handle, aHandle, ignoreCase:=True) = 0 Then
                        rEntry = MyBase.Item(i - 1)
                        rEntry.Index = i
                        Return True
                    End If
                ElseIf namePassed Then
                    If String.Compare(MyBase.Item(i - 1).Name, aName, ignoreCase:=True) = 0 Then
                        rEntry = MyBase.Item(i - 1)
                        rEntry.Index = i
                        Return True
                    End If
                ElseIf handlePassed Then
                    If String.Compare(MyBase.Item(i - 1).Handle, aHandle, ignoreCase:=True) = 0 Then
                        rEntry = MyBase.Item(i - 1)
                        rEntry.Index = i
                        Return True
                    End If
                End If
            Next
            Return False
        End Function
#End Region 'Methods



    End Class


End Namespace
