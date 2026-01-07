<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmTables
    Inherits System.Windows.Forms.Form
    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
                DisposeAll()
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTables))
        ColorDialog1 = New System.Windows.Forms.ColorDialog()
        pnlSelect = New System.Windows.Forms.GroupBox()
        cmdSelect = New System.Windows.Forms.Button()
        lblSelectedVal1 = New System.Windows.Forms.Label()
        lblSelected1 = New System.Windows.Forms.Label()
        cmdCancel2 = New System.Windows.Forms.Button()
        gdSelect = New System.Windows.Forms.DataGridView()
        Status_SELECT = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        RefName_SELECT = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Description_SELECT = New System.Windows.Forms.DataGridViewTextBoxColumn()
        pnlFormat = New System.Windows.Forms.GroupBox()
        cmdOk = New System.Windows.Forms.Button()
        cmdCancel = New System.Windows.Forms.Button()
        lblSelectedVal0 = New System.Windows.Forms.Label()
        lblSelected0 = New System.Windows.Forms.Label()
        lblCurrentVal1 = New System.Windows.Forms.Label()
        lblCurrent = New System.Windows.Forms.Label()
        chkScreen = New System.Windows.Forms.CheckBox()
        cmdDelete = New System.Windows.Forms.Button()
        cmdAdd = New System.Windows.Forms.Button()
        gdFormat = New System.Windows.Forms.DataGridView()
        pnlSelectCombo = New System.Windows.Forms.GroupBox()
        cboEntries = New System.Windows.Forms.ComboBox()
        cmdSelect1 = New System.Windows.Forms.Button()
        cmdCancel3 = New System.Windows.Forms.Button()
        Status_FORMAT = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        RefName_FORMAT = New System.Windows.Forms.DataGridViewTextBoxColumn()
        IsOn = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Frozen = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Locked = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Plot = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Color = New System.Windows.Forms.DataGridViewImageColumn()
        LineType = New System.Windows.Forms.DataGridViewTextBoxColumn()
        LineWeight = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Transparency = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Description = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Handle_FORMAT = New System.Windows.Forms.DataGridViewTextBoxColumn()
        pnlSelect.SuspendLayout()
        CType(Me.gdSelect, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlFormat.SuspendLayout()
        CType(Me.gdFormat, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlSelectCombo.SuspendLayout()
        SuspendLayout()
        '
        'pnlSelect
        '
        pnlSelect.Controls.Add(Me.cmdSelect)
        pnlSelect.Controls.Add(Me.lblSelectedVal1)
        pnlSelect.Controls.Add(Me.lblSelected1)
        pnlSelect.Controls.Add(Me.cmdCancel2)
        pnlSelect.Controls.Add(Me.gdSelect)
        pnlSelect.Location = New System.Drawing.Point(16, 15)
        pnlSelect.Margin = New System.Windows.Forms.Padding(4)
        pnlSelect.Name = "pnlSelect"
        pnlSelect.Padding = New System.Windows.Forms.Padding(4)
        pnlSelect.Size = New System.Drawing.Size(429, 469)
        pnlSelect.TabIndex = 1
        pnlSelect.TabStop = False
        pnlSelect.Text = "Select Layer"
        '
        'cmdSelect
        '
        cmdSelect.DialogResult = System.Windows.Forms.DialogResult.OK
        cmdSelect.Location = New System.Drawing.Point(293, 416)
        cmdSelect.Margin = New System.Windows.Forms.Padding(4)
        cmdSelect.Name = "cmdSelect"
        cmdSelect.Size = New System.Drawing.Size(121, 38)
        cmdSelect.TabIndex = 23
        cmdSelect.Text = "&Select"
        cmdSelect.UseVisualStyleBackColor = True
        '
        'lblSelectedVal1
        '
        lblSelectedVal1.AutoSize = True
        lblSelectedVal1.Location = New System.Drawing.Point(137, 36)
        lblSelectedVal1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblSelectedVal1.Name = "lblSelectedVal1"
        lblSelectedVal1.Size = New System.Drawing.Size(103, 16)
        lblSelectedVal1.TabIndex = 22
        lblSelectedVal1.Text = "lblSelectedVal1"
        '
        'lblSelected1
        '
        lblSelected1.AutoSize = True
        lblSelected1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblSelected1.Location = New System.Drawing.Point(13, 36)
        lblSelected1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblSelected1.Name = "lblSelected1"
        lblSelected1.Size = New System.Drawing.Size(96, 13)
        lblSelected1.TabIndex = 21
        lblSelected1.Text = "Selected Layer:"
        '
        'cmdCancel2
        '
        cmdCancel2.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdCancel2.Location = New System.Drawing.Point(17, 416)
        cmdCancel2.Margin = New System.Windows.Forms.Padding(4)
        cmdCancel2.Name = "cmdCancel2"
        cmdCancel2.Size = New System.Drawing.Size(121, 38)
        cmdCancel2.TabIndex = 20
        cmdCancel2.Text = "&Cancel"
        cmdCancel2.UseVisualStyleBackColor = True
        '
        'gdSelect
        '
        gdSelect.AllowUserToAddRows = False
        gdSelect.AllowUserToDeleteRows = False
        gdSelect.AllowUserToOrderColumns = True
        gdSelect.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        gdSelect.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Status_SELECT, RefName_SELECT, Description_SELECT})
        gdSelect.Location = New System.Drawing.Point(17, 58)
        gdSelect.Margin = New System.Windows.Forms.Padding(4)
        gdSelect.MultiSelect = False
        gdSelect.Name = "gdSelect"
        gdSelect.ReadOnly = True
        gdSelect.RowHeadersWidth = 51
        gdSelect.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        gdSelect.Size = New System.Drawing.Size(397, 351)
        gdSelect.TabIndex = 0
        '
        'Status_SELECT
        '
        Status_SELECT.HeaderText = "Status"
        Status_SELECT.MinimumWidth = 6
        Status_SELECT.Name = "Status_SELECT"
        Status_SELECT.ReadOnly = True
        Status_SELECT.Width = 50
        '
        'RefName_SELECT
        '
        RefName_SELECT.HeaderText = "Name"
        RefName_SELECT.MinimumWidth = 6
        RefName_SELECT.Name = "RefName_SELECT"
        RefName_SELECT.ReadOnly = True
        RefName_SELECT.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        RefName_SELECT.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        RefName_SELECT.Width = 75
        '
        'Description_SELECT
        '
        Description_SELECT.HeaderText = "Description"
        Description_SELECT.MinimumWidth = 6
        Description_SELECT.Name = "Description_SELECT"
        Description_SELECT.ReadOnly = True
        Description_SELECT.Width = 200
        '
        'pnlFormat
        '
        pnlFormat.Controls.Add(Me.cmdOk)
        pnlFormat.Controls.Add(Me.cmdCancel)
        pnlFormat.Controls.Add(Me.lblSelectedVal0)
        pnlFormat.Controls.Add(Me.lblSelected0)
        pnlFormat.Controls.Add(Me.lblCurrentVal1)
        pnlFormat.Controls.Add(Me.lblCurrent)
        pnlFormat.Controls.Add(Me.chkScreen)
        pnlFormat.Controls.Add(Me.cmdDelete)
        pnlFormat.Controls.Add(Me.cmdAdd)
        pnlFormat.Controls.Add(Me.gdFormat)
        pnlFormat.Location = New System.Drawing.Point(505, 26)
        pnlFormat.Margin = New System.Windows.Forms.Padding(4)
        pnlFormat.Name = "pnlFormat"
        pnlFormat.Padding = New System.Windows.Forms.Padding(4)
        pnlFormat.Size = New System.Drawing.Size(1035, 422)
        pnlFormat.TabIndex = 2
        pnlFormat.TabStop = False
        pnlFormat.Text = "Configure Layers"
        '
        'cmdOk
        '
        cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK
        cmdOk.Location = New System.Drawing.Point(876, 367)
        cmdOk.Margin = New System.Windows.Forms.Padding(4)
        cmdOk.Name = "cmdOk"
        cmdOk.Size = New System.Drawing.Size(121, 38)
        cmdOk.TabIndex = 20
        cmdOk.Text = "&Apply"
        cmdOk.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdCancel.Location = New System.Drawing.Point(40, 367)
        cmdCancel.Margin = New System.Windows.Forms.Padding(4)
        cmdCancel.Name = "cmdCancel"
        cmdCancel.Size = New System.Drawing.Size(121, 38)
        cmdCancel.TabIndex = 19
        cmdCancel.Text = "&Cancel"
        cmdCancel.UseVisualStyleBackColor = True
        '
        'lblSelectedVal0
        '
        lblSelectedVal0.AutoSize = True
        lblSelectedVal0.Location = New System.Drawing.Point(599, 25)
        lblSelectedVal0.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblSelectedVal0.Name = "lblSelectedVal0"
        lblSelectedVal0.Size = New System.Drawing.Size(103, 16)
        lblSelectedVal0.TabIndex = 18
        lblSelectedVal0.Text = "lblSelectedVal0"
        '
        'lblSelected0
        '
        lblSelected0.AutoSize = True
        lblSelected0.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblSelected0.Location = New System.Drawing.Point(463, 25)
        lblSelected0.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblSelected0.Name = "lblSelected0"
        lblSelected0.Size = New System.Drawing.Size(96, 13)
        lblSelected0.TabIndex = 17
        lblSelected0.Text = "Selected Layer:"
        '
        'lblCurrentVal1
        '
        lblCurrentVal1.AutoSize = True
        lblCurrentVal1.Location = New System.Drawing.Point(160, 25)
        lblCurrentVal1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblCurrentVal1.Name = "lblCurrentVal1"
        lblCurrentVal1.Size = New System.Drawing.Size(91, 16)
        lblCurrentVal1.TabIndex = 16
        lblCurrentVal1.Text = "lblCurrentVal1"
        '
        'lblCurrent
        '
        lblCurrent.AutoSize = True
        lblCurrent.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblCurrent.Location = New System.Drawing.Point(36, 25)
        lblCurrent.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblCurrent.Name = "lblCurrent"
        lblCurrent.Size = New System.Drawing.Size(87, 13)
        lblCurrent.TabIndex = 15
        lblCurrent.Text = "Current Layer:"
        '
        'chkScreen
        '
        chkScreen.AutoSize = True
        chkScreen.Location = New System.Drawing.Point(617, 377)
        chkScreen.Margin = New System.Windows.Forms.Padding(4)
        chkScreen.Name = "chkScreen"
        chkScreen.Size = New System.Drawing.Size(147, 20)
        chkScreen.TabIndex = 12
        chkScreen.Text = "Screen Layer On/Off"
        chkScreen.UseVisualStyleBackColor = True
        '
        'cmdDelete
        '
        cmdDelete.Location = New System.Drawing.Point(924, 20)
        cmdDelete.Margin = New System.Windows.Forms.Padding(4)
        cmdDelete.Name = "cmdDelete"
        cmdDelete.Size = New System.Drawing.Size(87, 25)
        cmdDelete.TabIndex = 14
        cmdDelete.Text = "&Delete"
        cmdDelete.UseVisualStyleBackColor = True
        '
        'cmdAdd
        '
        cmdAdd.Location = New System.Drawing.Point(817, 20)
        cmdAdd.Margin = New System.Windows.Forms.Padding(4)
        cmdAdd.Name = "cmdAdd"
        cmdAdd.Size = New System.Drawing.Size(87, 25)
        cmdAdd.TabIndex = 13
        cmdAdd.Text = "A&dd"
        cmdAdd.UseVisualStyleBackColor = True
        '
        'gdFormat
        '
        gdFormat.AllowUserToAddRows = False
        gdFormat.AllowUserToDeleteRows = False
        gdFormat.CausesValidation = False
        gdFormat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        gdFormat.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Status_FORMAT, RefName_FORMAT, IsOn, Frozen, Locked, Plot, Color, LineType, LineWeight, Transparency, Description, Handle_FORMAT})
        gdFormat.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
        gdFormat.Location = New System.Drawing.Point(24, 52)
        gdFormat.Margin = New System.Windows.Forms.Padding(4)
        gdFormat.MultiSelect = False
        gdFormat.Name = "gdFormat"
        gdFormat.RowHeadersWidth = 51
        gdFormat.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        gdFormat.ShowCellErrors = False
        gdFormat.ShowRowErrors = False
        gdFormat.Size = New System.Drawing.Size(987, 311)
        gdFormat.TabIndex = 11
        '
        'pnlSelectCombo
        '
        pnlSelectCombo.Controls.Add(Me.cboEntries)
        pnlSelectCombo.Controls.Add(Me.cmdSelect1)
        pnlSelectCombo.Controls.Add(Me.cmdCancel3)
        pnlSelectCombo.Location = New System.Drawing.Point(563, 116)
        pnlSelectCombo.Margin = New System.Windows.Forms.Padding(4)
        pnlSelectCombo.Name = "pnlSelectCombo"
        pnlSelectCombo.Padding = New System.Windows.Forms.Padding(4)
        pnlSelectCombo.Size = New System.Drawing.Size(429, 122)
        pnlSelectCombo.TabIndex = 3
        pnlSelectCombo.TabStop = False
        pnlSelectCombo.Text = "Select Layer"
        '
        'cboEntries
        '
        cboEntries.FormattingEnabled = True
        cboEntries.Location = New System.Drawing.Point(36, 23)
        cboEntries.Margin = New System.Windows.Forms.Padding(4)
        cboEntries.Name = "cboEntries"
        cboEntries.Size = New System.Drawing.Size(364, 24)
        cboEntries.TabIndex = 24
        '
        'cmdSelect1
        '
        cmdSelect1.DialogResult = System.Windows.Forms.DialogResult.OK
        cmdSelect1.Location = New System.Drawing.Point(296, 76)
        cmdSelect1.Margin = New System.Windows.Forms.Padding(4)
        cmdSelect1.Name = "cmdSelect1"
        cmdSelect1.Size = New System.Drawing.Size(121, 38)
        cmdSelect1.TabIndex = 23
        cmdSelect1.Text = "&Select"
        cmdSelect1.UseVisualStyleBackColor = True
        '
        'cmdCancel3
        '
        cmdCancel3.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdCancel3.Location = New System.Drawing.Point(20, 76)
        cmdCancel3.Margin = New System.Windows.Forms.Padding(4)
        cmdCancel3.Name = "cmdCancel3"
        cmdCancel3.Size = New System.Drawing.Size(121, 38)
        cmdCancel3.TabIndex = 20
        cmdCancel3.Text = "&Cancel"
        cmdCancel3.UseVisualStyleBackColor = True
        '
        'Status_FORMAT
        '
        Status_FORMAT.HeaderText = "Status"
        Status_FORMAT.MinimumWidth = 6
        Status_FORMAT.Name = "Status_FORMAT"
        Status_FORMAT.Width = 40
        '
        'RefName_FORMAT
        '
        RefName_FORMAT.HeaderText = "Name"
        RefName_FORMAT.MinimumWidth = 6
        RefName_FORMAT.Name = "RefName_FORMAT"
        RefName_FORMAT.Width = 125
        '
        'IsOn
        '
        IsOn.HeaderText = "On"
        IsOn.MinimumWidth = 6
        IsOn.Name = "IsOn"
        IsOn.Width = 35
        '
        'Frozen
        '
        Frozen.HeaderText = "Freeze"
        Frozen.MinimumWidth = 6
        Frozen.Name = "Frozen"
        Frozen.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Frozen.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Frozen.Width = 35
        '
        'Locked
        '
        Locked.HeaderText = "Lock"
        Locked.MinimumWidth = 6
        Locked.Name = "Locked"
        Locked.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Locked.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Locked.Width = 35
        '
        'Plot
        '
        Plot.HeaderText = "Plot"
        Plot.MinimumWidth = 6
        Plot.Name = "Plot"
        Plot.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Plot.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Plot.Width = 35
        '
        'Color
        '
        Color.HeaderText = "Color"
        Color.MinimumWidth = 6
        Color.Name = "Color"
        Color.Width = 35
        '
        'LineType
        '
        LineType.HeaderText = "Linetype"
        LineType.MinimumWidth = 6
        LineType.Name = "LineType"
        LineType.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        LineType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        LineType.Width = 125
        '
        'LineWeight
        '
        LineWeight.HeaderText = "LineWeight"
        LineWeight.MinimumWidth = 6
        LineWeight.Name = "LineWeight"
        LineWeight.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        LineWeight.Width = 125
        '
        'Transparency
        '
        Transparency.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        Transparency.HeaderText = "Transparency"
        Transparency.MinimumWidth = 6
        Transparency.Name = "Transparency"
        Transparency.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Transparency.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Transparency.Width = 98
        '
        'Description
        '
        Description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        Description.HeaderText = "Description"
        Description.MinimumWidth = 6
        Description.Name = "Description"
        Description.Width = 101
        '
        'Handle_FORMAT
        '
        Handle_FORMAT.HeaderText = "Handle"
        Handle_FORMAT.MinimumWidth = 6
        Handle_FORMAT.Name = "Handle_FORMAT"
        Handle_FORMAT.ReadOnly = True
        Handle_FORMAT.Width = 125
        '
        'frmTables
        '
        AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(1556, 642)
        Controls.Add(Me.pnlSelectCombo)
        Controls.Add(Me.pnlFormat)
        Controls.Add(Me.pnlSelect)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "frmTables"
        ShowInTaskbar = False
        SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Drawing Layers"
        pnlSelect.ResumeLayout(False)
        pnlSelect.PerformLayout()
        CType(Me.gdSelect, System.ComponentModel.ISupportInitialize).EndInit()
        pnlFormat.ResumeLayout(False)
        pnlFormat.PerformLayout()
        CType(Me.gdFormat, System.ComponentModel.ISupportInitialize).EndInit()
        pnlSelectCombo.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents ColorDialog1 As ColorDialog
    Friend WithEvents pnlSelect As GroupBox
    Friend WithEvents pnlFormat As GroupBox
    Friend WithEvents cmdOk As Button
    Friend WithEvents cmdCancel As Button
    Friend WithEvents lblSelectedVal0 As Label
    Friend WithEvents lblSelected0 As Label
    Friend WithEvents lblCurrentVal1 As Label
    Friend WithEvents lblCurrent As Label
    Friend WithEvents chkScreen As CheckBox
    Friend WithEvents cmdDelete As Button
    Friend WithEvents cmdAdd As Button
    Friend WithEvents gdFormat As DataGridView
    Friend WithEvents gdSelect As DataGridView
    Friend WithEvents cmdCancel2 As Button
    Friend WithEvents lblSelectedVal1 As Label
    Friend WithEvents lblSelected1 As Label
    Friend WithEvents cmdSelect As Button
    Friend WithEvents pnlSelectCombo As GroupBox
    Friend WithEvents cmdSelect1 As Button
    Friend WithEvents cmdCancel3 As Button
    Friend WithEvents cboEntries As ComboBox
    Friend WithEvents Status_SELECT As DataGridViewCheckBoxColumn
    Friend WithEvents RefName_SELECT As DataGridViewTextBoxColumn
    Friend WithEvents Description_SELECT As DataGridViewTextBoxColumn
    Friend WithEvents Status_FORMAT As DataGridViewCheckBoxColumn
    Friend WithEvents RefName_FORMAT As DataGridViewTextBoxColumn
    Friend WithEvents IsOn As DataGridViewCheckBoxColumn
    Friend WithEvents Frozen As DataGridViewCheckBoxColumn
    Friend WithEvents Locked As DataGridViewCheckBoxColumn
    Friend WithEvents Plot As DataGridViewCheckBoxColumn
    Friend WithEvents Color As DataGridViewImageColumn
    Friend WithEvents LineType As DataGridViewTextBoxColumn
    Friend WithEvents LineWeight As DataGridViewComboBoxColumn
    Friend WithEvents Transparency As DataGridViewTextBoxColumn
    Friend WithEvents Description As DataGridViewTextBoxColumn
    Friend WithEvents Handle_FORMAT As DataGridViewTextBoxColumn
End Class
