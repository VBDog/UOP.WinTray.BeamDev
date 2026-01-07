Imports System.Text
Public Class frmMessages
    Private _Canceled As Boolean
    Public Sub ShowMessages(aErrorCol As List(Of String), Optional aWindowCaption As String = Nothing, Optional aOwner As IWin32Window = Nothing)
        _Canceled = False
        If Not String.IsNullOrEmpty(aWindowCaption) Then Text = aWindowCaption
        lstMessages.Text = ""
        Dim SB As New StringBuilder
        If aErrorCol IsNot Nothing Then
            For i As Integer = 1 To aErrorCol.Count
                If aErrorCol.Count > 0 Then
                    SB.AppendLine($"{i} - aErrorCol.Item(i - 1)")
                Else
                    SB.AppendLine(aErrorCol.Item(i - 1))
                End If
            Next
        End If
        lstMessages.Text = SB.ToString
        lstMessages.SelectionLength = 0
        cmdClose.Select()
        ShowDialog(aOwner)
    End Sub
    Private Sub cmdClose_Click(sender As Object, e As EventArgs) Handles cmdClose.Click
        _Canceled = True
        Hide()
    End Sub
    Private Sub lstMessages_TextChanged(sender As Object, e As EventArgs) Handles lstMessages.TextChanged
    End Sub
    Private Sub lstMessages_KeyPress(sender As Object, e As KeyPressEventArgs) Handles lstMessages.KeyPress
        e.KeyChar = ""
    End Sub
    Private Sub frmMessages_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    End Sub
End Class
