<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFonts
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
        cmdClose = New System.Windows.Forms.Button()
        cboFonts = New System.Windows.Forms.ComboBox()
        Label1 = New System.Windows.Forms.Label()
        Label2 = New System.Windows.Forms.Label()
        cboStyles = New System.Windows.Forms.ComboBox()
        Label3 = New System.Windows.Forms.Label()
        lblFData_0 = New System.Windows.Forms.Label()
        lblFData_1 = New System.Windows.Forms.Label()
        Label5 = New System.Windows.Forms.Label()
        lblFData_2 = New System.Windows.Forms.Label()
        Label8 = New System.Windows.Forms.Label()
        chkTTF = New System.Windows.Forms.CheckBox()
        chkSHX = New System.Windows.Forms.CheckBox()
        cmdSave = New System.Windows.Forms.Button()
        btnReload = New System.Windows.Forms.Button()
        picPreview = New System.Windows.Forms.PictureBox()
        CType(Me.picPreview, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        'cmdClose
        '
        cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdClose.Location = New System.Drawing.Point(55, 389)
        cmdClose.Name = "cmdClose"
        cmdClose.Size = New System.Drawing.Size(75, 23)
        cmdClose.TabIndex = 5
        cmdClose.Text = "Cancel"
        cmdClose.UseVisualStyleBackColor = True
        '
        'cboFonts
        '
        cboFonts.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        cboFonts.FormattingEnabled = True
        cboFonts.Location = New System.Drawing.Point(81, 241)
        cboFonts.Name = "cboFonts"
        cboFonts.Size = New System.Drawing.Size(213, 28)
        cboFonts.TabIndex = 6
        '
        'Label1
        '
        Label1.AutoSize = True
        Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Label1.Location = New System.Drawing.Point(17, 241)
        Label1.Name = "Label1"
        Label1.Size = New System.Drawing.Size(58, 20)
        Label1.TabIndex = 7
        Label1.Text = "Fonts :"
        '
        'Label2
        '
        Label2.AutoSize = True
        Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Label2.Location = New System.Drawing.Point(12, 276)
        Label2.Name = "Label2"
        Label2.Size = New System.Drawing.Size(60, 20)
        Label2.TabIndex = 9
        Label2.Text = "Styles :"
        '
        'cboStyles
        '
        cboStyles.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        cboStyles.FormattingEnabled = True
        cboStyles.Location = New System.Drawing.Point(81, 272)
        cboStyles.Name = "cboStyles"
        cboStyles.Size = New System.Drawing.Size(213, 28)
        cboStyles.TabIndex = 8
        '
        'Label3
        '
        Label3.AutoSize = True
        Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Label3.Location = New System.Drawing.Point(503, 246)
        Label3.Name = "Label3"
        Label3.Size = New System.Drawing.Size(88, 20)
        Label3.TabIndex = 10
        Label3.Text = "File Name :"
        '
        'lblFData_0
        '
        lblFData_0.AutoSize = True
        lblFData_0.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblFData_0.Location = New System.Drawing.Point(597, 245)
        lblFData_0.Name = "lblFData_0"
        lblFData_0.Size = New System.Drawing.Size(87, 20)
        lblFData_0.TabIndex = 11
        lblFData_0.Tag = "FileName"
        lblFData_0.Text = "lblFData_0"
        '
        'lblFData_1
        '
        lblFData_1.AutoSize = True
        lblFData_1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblFData_1.Location = New System.Drawing.Point(597, 266)
        lblFData_1.Name = "lblFData_1"
        lblFData_1.Size = New System.Drawing.Size(87, 20)
        lblFData_1.TabIndex = 13
        lblFData_1.Tag = "TypeFace"
        lblFData_1.Text = "lblFData_1"
        '
        'Label5
        '
        Label5.AutoSize = True
        Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Label5.Location = New System.Drawing.Point(492, 266)
        Label5.Name = "Label5"
        Label5.Size = New System.Drawing.Size(91, 20)
        Label5.TabIndex = 12
        Label5.Text = "Type Face :"
        '
        'lblFData_2
        '
        lblFData_2.AutoSize = True
        lblFData_2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblFData_2.Location = New System.Drawing.Point(597, 290)
        lblFData_2.Name = "lblFData_2"
        lblFData_2.Size = New System.Drawing.Size(87, 20)
        lblFData_2.TabIndex = 15
        lblFData_2.Tag = "FaceName"
        lblFData_2.Text = "lblFData_2"
        '
        'Label8
        '
        Label8.AutoSize = True
        Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Label8.Location = New System.Drawing.Point(492, 286)
        Label8.Name = "Label8"
        Label8.Size = New System.Drawing.Size(99, 20)
        Label8.TabIndex = 14
        Label8.Text = "Face Name :"
        '
        'chkTTF
        '
        chkTTF.AutoSize = True
        chkTTF.Checked = True
        chkTTF.CheckState = System.Windows.Forms.CheckState.Checked
        chkTTF.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        chkTTF.Location = New System.Drawing.Point(333, 249)
        chkTTF.Name = "chkTTF"
        chkTTF.Size = New System.Drawing.Size(145, 24)
        chkTTF.TabIndex = 18
        chkTTF.Text = "Show TTF Fonts"
        chkTTF.UseVisualStyleBackColor = True
        '
        'chkSHX
        '
        chkSHX.AutoSize = True
        chkSHX.Checked = True
        chkSHX.CheckState = System.Windows.Forms.CheckState.Checked
        chkSHX.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        chkSHX.Location = New System.Drawing.Point(333, 276)
        chkSHX.Name = "chkSHX"
        chkSHX.Size = New System.Drawing.Size(164, 24)
        chkSHX.TabIndex = 19
        chkSHX.Text = "Show Shape Fonts"
        chkSHX.UseVisualStyleBackColor = True
        '
        'cmdSave
        '
        cmdSave.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdSave.Location = New System.Drawing.Point(601, 389)
        cmdSave.Name = "cmdSave"
        cmdSave.Size = New System.Drawing.Size(75, 23)
        cmdSave.TabIndex = 20
        cmdSave.Text = "Save"
        cmdSave.UseVisualStyleBackColor = True
        '
        'btnReload
        '
        btnReload.Location = New System.Drawing.Point(359, 378)
        btnReload.Name = "btnReload"
        btnReload.Size = New System.Drawing.Size(119, 45)
        btnReload.TabIndex = 21
        btnReload.Text = "Reload"
        btnReload.UseVisualStyleBackColor = True
        '
        'picPreview
        '
        picPreview.BackColor = System.Drawing.Color.White
        picPreview.Location = New System.Drawing.Point(12, 12)
        picPreview.Name = "picPreview"
        picPreview.Size = New System.Drawing.Size(776, 117)
        picPreview.TabIndex = 23
        picPreview.TabStop = False
        '
        'frmFonts
        '
        AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        CancelButton = cmdClose
        ClientSize = New System.Drawing.Size(800, 450)
        ControlBox = False
        Controls.Add(Me.picPreview)
        Controls.Add(Me.btnReload)
        Controls.Add(Me.cmdSave)
        Controls.Add(Me.chkSHX)
        Controls.Add(Me.chkTTF)
        Controls.Add(Me.lblFData_2)
        Controls.Add(Me.Label8)
        Controls.Add(Me.lblFData_1)
        Controls.Add(Me.Label5)
        Controls.Add(Me.lblFData_0)
        Controls.Add(Me.Label3)
        Controls.Add(Me.Label2)
        Controls.Add(Me.cboStyles)
        Controls.Add(Me.Label1)
        Controls.Add(Me.cboFonts)
        Controls.Add(Me.cmdClose)
        Name = "frmFonts"
        Text = "Fonts"
        CType(Me.picPreview, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents cmdClose As Button
    Friend WithEvents cboFonts As ComboBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents cboStyles As ComboBox
    Friend WithEvents Label3 As Label
    Friend WithEvents lblFData_0 As Label
    Friend WithEvents lblFData_1 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents lblFData_2 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents chkTTF As CheckBox
    Friend WithEvents chkSHX As CheckBox
    Friend WithEvents cmdSave As Button
    Friend WithEvents btnReload As Button
    Friend WithEvents picPreview As PictureBox
End Class
