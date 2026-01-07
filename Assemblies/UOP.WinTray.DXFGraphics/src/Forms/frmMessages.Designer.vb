<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMessages
    Inherits System.Windows.Forms.Form
    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Panel1 = New System.Windows.Forms.Panel()
        lstMessages = New System.Windows.Forms.TextBox()
        cmdClose = New System.Windows.Forms.Button()
        Panel1.SuspendLayout()
        SuspendLayout()
        '
        'Panel1
        '
        Panel1.Controls.Add(Me.lstMessages)
        Panel1.Location = New System.Drawing.Point(3, 6)
        Panel1.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Panel1.Name = "Panel1"
        Panel1.Size = New System.Drawing.Size(592, 156)
        Panel1.TabIndex = 0
        '
        'lstMessages
        '
        lstMessages.Location = New System.Drawing.Point(2, 3)
        lstMessages.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        lstMessages.Multiline = True
        lstMessages.Name = "lstMessages"
        lstMessages.Size = New System.Drawing.Size(589, 140)
        lstMessages.TabIndex = 0
        '
        'cmdClose
        '
        cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdClose.Location = New System.Drawing.Point(9, 167)
        cmdClose.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        cmdClose.Name = "cmdClose"
        cmdClose.Size = New System.Drawing.Size(94, 26)
        cmdClose.TabIndex = 1
        cmdClose.Text = "Close"
        cmdClose.UseVisualStyleBackColor = True
        '
        'frmMessages
        '
        AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        AutoSize = True
        BackColor = System.Drawing.SystemColors.ControlLight
        CancelButton = cmdClose
        ClientSize = New System.Drawing.Size(604, 203)
        ControlBox = False
        Controls.Add(Me.cmdClose)
        Controls.Add(Me.Panel1)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Name = "frmMessages"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Messages"
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        ResumeLayout(False)
    End Sub
    Friend WithEvents Panel1 As Panel
    Friend WithEvents lstMessages As TextBox
    Friend WithEvents cmdClose As Button
End Class
