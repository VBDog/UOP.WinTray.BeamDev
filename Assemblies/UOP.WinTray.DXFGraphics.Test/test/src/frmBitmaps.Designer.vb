<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBitmaps
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.lblPXs = New System.Windows.Forms.Label()
        Me.txtPic2 = New System.Windows.Forms.Label()
        Me.lblWorld = New System.Windows.Forms.Label()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(55, 317)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(182, 58)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.Image = Global.dxfGraphicsTest.My.Resources.Resources.Blue_hills
        Me.PictureBox1.Location = New System.Drawing.Point(12, 25)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(561, 250)
        Me.PictureBox1.TabIndex = 1
        Me.PictureBox1.TabStop = False
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(272, 317)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(182, 58)
        Me.Button2.TabIndex = 2
        Me.Button2.Text = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'PictureBox2
        '
        Me.PictureBox2.BackColor = System.Drawing.Color.White
        Me.PictureBox2.Location = New System.Drawing.Point(717, 25)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(699, 286)
        Me.PictureBox2.TabIndex = 3
        Me.PictureBox2.TabStop = False
        '
        'PictureBox3
        '
        Me.PictureBox3.BackColor = System.Drawing.Color.CadetBlue
        Me.PictureBox3.Location = New System.Drawing.Point(635, 361)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(114, 43)
        Me.PictureBox3.TabIndex = 4
        Me.PictureBox3.TabStop = False
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(869, 361)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(207, 20)
        Me.TextBox1.TabIndex = 5
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(489, 317)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(182, 58)
        Me.Button3.TabIndex = 6
        Me.Button3.Text = "Button3"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(1173, 323)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(182, 58)
        Me.Button4.TabIndex = 7
        Me.Button4.Text = "Button4"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'lblPXs
        '
        Me.lblPXs.AutoSize = True
        Me.lblPXs.Location = New System.Drawing.Point(724, 317)
        Me.lblPXs.Name = "lblPXs"
        Me.lblPXs.Size = New System.Drawing.Size(36, 13)
        Me.lblPXs.TabIndex = 8
        Me.lblPXs.Text = "lblPXs"
        '
        'txtPic2
        '
        Me.txtPic2.AutoSize = True
        Me.txtPic2.Location = New System.Drawing.Point(745, 9)
        Me.txtPic2.Name = "txtPic2"
        Me.txtPic2.Size = New System.Drawing.Size(39, 13)
        Me.txtPic2.TabIndex = 9
        Me.txtPic2.Text = "txtPic2"
        '
        'lblWorld
        '
        Me.lblWorld.AutoSize = True
        Me.lblWorld.Location = New System.Drawing.Point(724, 330)
        Me.lblWorld.Name = "lblWorld"
        Me.lblWorld.Size = New System.Drawing.Size(45, 13)
        Me.lblWorld.TabIndex = 10
        Me.lblWorld.Text = "lblWorld"
        '
        'frmBitmaps
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1443, 427)
        Me.Controls.Add(Me.lblWorld)
        Me.Controls.Add(Me.txtPic2)
        Me.Controls.Add(Me.lblPXs)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.PictureBox3)
        Me.Controls.Add(Me.PictureBox2)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.Button1)
        Me.Name = "frmBitmaps"
        Me.Text = "Form1"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Button2 As Button
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button3 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents lblPXs As Label
    Friend WithEvents txtPic2 As Label
    Friend WithEvents lblWorld As Label
End Class
