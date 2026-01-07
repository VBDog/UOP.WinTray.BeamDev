Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoSubStrings
        Inherits List(Of dxoString)

#Region "Constructors"

        Public Sub New()
            Init()
        End Sub

        Public Sub New(aStrings As dxfStrings, aBaseFormat As dxoCharFormat)
            Init()
            If aStrings Is Nothing Then Return
            If aStrings.CharacterCount <= 0 Then Return
            Dim aStr As dxoString

            Dim lChar As dxoCharacter
            Dim chrCnt As Integer
            Dim aChrs As List(Of dxoCharacter)
            Dim hndl As String
            Dim lhndl As String
            Dim gi As Integer
            Dim sStr As dxoString
            For li As Integer = 1 To aStrings.Count
                aStr = aStrings.Item(li - 1)
                aChrs = aStr.Characters.FindAll(Function(x) Not x.IsFormatCode)
                chrCnt = aChrs.Count
                gi = -1
                lhndl = ""
                sStr = aStr.Clone(True)
                For Each strchar In aChrs
                    hndl = $"{Format(strchar.CharHeight, "0.000000") }:{ strchar.Color }:{ strchar.StackID }:{ strchar.CharAlign}"
                    If hndl <> lhndl Then
                        If sStr.Characters.Count > 0 Then
                            Add(sStr.Clone())
                            sStr = aStr.Clone(True)
                        End If
                        gi += 1
                    End If
                    strchar.GroupIndex = gi
                    sStr.Characters.Add(strchar)
                    lhndl = hndl
                    lChar = strchar
                Next


                If sStr.Characters.Count > 0 Then
                    Add(sStr.Clone())
                    sStr = aStr.Clone(True)
                End If

            Next li

        End Sub


        Private Sub Init()
            Clear()
        End Sub
#End Region 'Constructors

#Region "Properties"

#End Region 'Properties

#Region "Methods"
        Public Function CharacterCount(Optional aIndex As Integer = 0) As Integer
            '#1an optional substring index to get the character count for
            'returns the number of visible characters
            Dim _rVal As Integer = 0
            For i As Integer = 1 To Count
                If aIndex <= 0 Or aIndex = i Then _rVal += Item(i).CharacterCount
            Next
            Return _rVal
        End Function

        Public Shadows Function Item(aIndex As Integer) As dxoString
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            MyBase.Item(aIndex - 1).LineNo = aIndex
            Return MyBase.Item(aIndex - 1)

        End Function

        Public Overloads Sub Add(aMember As dxoString)
            If aMember Is Nothing Then Return
            aMember.LineNo = Count + 1
            MyBase.Add(aMember)
        End Sub

        Friend Function ItemV(aIndex As Integer) As TSTRING
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TSTRING("")
            End If
            MyBase.Item(aIndex - 1).LineNo = aIndex
            Return MyBase.Item(aIndex - 1).Structure_Get()

        End Function



        Friend Sub AddV(aString As TSTRING)

            aString.LineNo = Count + 1
            Add(New dxoString(aString))
        End Sub
#End Region 'Methods

    End Class

End Namespace
