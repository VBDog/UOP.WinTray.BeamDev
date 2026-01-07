<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmInput
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
        lblCaption = New System.Windows.Forms.Label()
        lblInfo = New System.Windows.Forms.Label()
        txtInput = New System.Windows.Forms.TextBox()
        cmdCancel = New System.Windows.Forms.Button()
        cmdOK = New System.Windows.Forms.Button()
        cboInput = New System.Windows.Forms.ComboBox()
        Panel1.SuspendLayout()
        SuspendLayout()
        '
        'Panel1
        '
        Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Panel1.Controls.Add(Me.cboInput)
        Panel1.Controls.Add(Me.txtInput)
        Panel1.Controls.Add(Me.lblInfo)
        Panel1.Controls.Add(Me.lblCaption)
        Panel1.Location = New System.Drawing.Point(12, 12)
        Panel1.Name = "Panel1"
        Panel1.Size = New System.Drawing.Size(354, 86)
        Panel1.TabIndex = 0
        '
        'lblCaption
        '
        lblCaption.AutoSize = True
        lblCaption.Location = New System.Drawing.Point(13, 11)
        lblCaption.Name = "lblCaption"
        lblCaption.Size = New System.Drawing.Size(46, 13)
        lblCaption.TabIndex = 0
        lblCaption.Text = "Caption:"
        '
        'lblInfo
        '
        lblInfo.AutoSize = True
        lblInfo.Location = New System.Drawing.Point(13, 50)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New System.Drawing.Size(28, 13)
        lblInfo.TabIndex = 1
        lblInfo.Text = "Info:"
        '
        'txtInput
        '
        txtInput.Location = New System.Drawing.Point(16, 27)
        txtInput.Name = "txtInput"
        txtInput.Size = New System.Drawing.Size(293, 20)
        txtInput.TabIndex = 2
        '
        'cmdCancel
        '
        cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdCancel.Location = New System.Drawing.Point(12, 104)
        cmdCancel.Name = "cmdCancel"
        cmdCancel.Size = New System.Drawing.Size(75, 23)
        cmdCancel.TabIndex = 3
        cmdCancel.Text = "Cancel"
        cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdOK
        '
        cmdOK.Location = New System.Drawing.Point(291, 104)
        cmdOK.Name = "cmdOK"
        cmdOK.Size = New System.Drawing.Size(75, 23)
        cmdOK.TabIndex = 4
        cmdOK.Text = "OK"
        cmdOK.UseVisualStyleBackColor = True
        '
        'cboInput
        '
        cboInput.FormattingEnabled = True
        cboInput.Location = New System.Drawing.Point(16, 27)
        cboInput.Name = "cboInput"
        cboInput.Size = New System.Drawing.Size(293, 21)
        cboInput.TabIndex = 3
        '
        'frmInput
        '
        AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(378, 134)
        ControlBox = False
        Controls.Add(Me.cmdOK)
        Controls.Add(Me.Panel1)
        Controls.Add(Me.cmdCancel)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Name = "frmInput"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Input"
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        ResumeLayout(False)
    End Sub
    Friend WithEvents Panel1 As Panel
    Friend WithEvents txtInput As TextBox
    Friend WithEvents lblInfo As Label
    Friend WithEvents lblCaption As Label
    Friend WithEvents cmdCancel As Button
    Friend WithEvents cmdOK As Button
    Friend WithEvents cboInput As ComboBox
End Class
