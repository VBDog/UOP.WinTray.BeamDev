Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoDictionaryEntry
        Implements ICloneable
        Public Name As String
        Public Handle As String
        Public Index As Integer
#Region "Constructors"

        Public Sub New()
            'init =============
            Name = String.Empty
            Handle = String.Empty
            Index = 0
            'init =============
        End Sub
        Public Sub New(Optional aName As String = "", Optional aHandle As String = "")
            'init =============
            Name = aName
            Handle = aHandle
            Index = 0
            'init =============

        End Sub

        Friend Sub New(aEntry As TDICTIONARYENTRY)
            'init =============
            Name = aEntry.Name
            Handle = aEntry.Handle
            Index = aEntry.Index
            'init =============
        End Sub
        Public Sub New(aEntry As dxoDictionaryEntry)
            'init =============
            Name = String.Empty
            Handle = String.Empty
            Index = 0
            'init =============
            If aEntry Is Nothing Then Return
            Name = aEntry.Name
            Handle = aEntry.Handle
            Index = aEntry.Index
        End Sub

#Region "Methods"
        Public Function Clone() As dxoDictionaryEntry
            Return New dxoDictionaryEntry(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoDictionaryEntry(Me)
        End Function
        Public Overrides Function ToString() As String
            Return $"Dictionary Entry[Name:{Name } Handle:{ Handle }]"
        End Function


#End Region 'Methods


#End Region 'Constructors

    End Class


End Namespace
