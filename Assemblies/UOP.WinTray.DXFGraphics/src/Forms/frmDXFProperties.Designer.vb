<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmDXFProperties
    Inherits System.Windows.Forms.Form
    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        GenSettings = New System.Windows.Forms.Panel()
        cmdReload = New System.Windows.Forms.Button()
        gbShow = New System.Windows.Forms.GroupBox()
        gbViewControl = New System.Windows.Forms.GroupBox()
        chkSuppress_0 = New System.Windows.Forms.CheckBox()
        chkIO_2 = New System.Windows.Forms.CheckBox()
        chkIO_1 = New System.Windows.Forms.CheckBox()
        gbNatives = New System.Windows.Forms.GroupBox()
        CheckBox10 = New System.Windows.Forms.CheckBox()
        gbTables = New System.Windows.Forms.GroupBox()
        chkTable_9 = New System.Windows.Forms.CheckBox()
        chkTable_8 = New System.Windows.Forms.CheckBox()
        chkTable_6 = New System.Windows.Forms.CheckBox()
        chkTable_5 = New System.Windows.Forms.CheckBox()
        chkTable_4 = New System.Windows.Forms.CheckBox()
        chkTable_3 = New System.Windows.Forms.CheckBox()
        chkTable_1 = New System.Windows.Forms.CheckBox()
        chkTable_2 = New System.Windows.Forms.CheckBox()
        chkTable_7 = New System.Windows.Forms.CheckBox()
        chkSuppress_10 = New System.Windows.Forms.CheckBox()
        chkSuppress_9 = New System.Windows.Forms.CheckBox()
        chkSuppress_8 = New System.Windows.Forms.CheckBox()
        chkSuppress_7 = New System.Windows.Forms.CheckBox()
        chkSuppress_3 = New System.Windows.Forms.CheckBox()
        chkSuppress_6 = New System.Windows.Forms.CheckBox()
        chkSuppress_2 = New System.Windows.Forms.CheckBox()
        chkSuppress_1 = New System.Windows.Forms.CheckBox()
        chkSuppress_5 = New System.Windows.Forms.CheckBox()
        gbSettings = New System.Windows.Forms.GroupBox()
        chkSettings_0 = New System.Windows.Forms.CheckBox()
        chkSettings_7 = New System.Windows.Forms.CheckBox()
        chkSettings_5 = New System.Windows.Forms.CheckBox()
        chkSettings_4 = New System.Windows.Forms.CheckBox()
        chkSettings_3 = New System.Windows.Forms.CheckBox()
        chkSettings_6 = New System.Windows.Forms.CheckBox()
        chkSettings_2 = New System.Windows.Forms.CheckBox()
        chkSettings_8 = New System.Windows.Forms.CheckBox()
        chkSettings_1 = New System.Windows.Forms.CheckBox()
        chkSuppress_4 = New System.Windows.Forms.CheckBox()
        fraTools = New System.Windows.Forms.Panel()
        cmdClose = New System.Windows.Forms.Button()
        sbStatus = New System.Windows.Forms.StatusStrip()
        ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        TabControl1 = New System.Windows.Forms.TabControl()
        TreeViews = New System.Windows.Forms.TabPage()
        fraTrees = New System.Windows.Forms.Panel()
        txtErrorKey = New System.Windows.Forms.TextBox()
        trvErrors = New System.Windows.Forms.TreeView()
        txtHandleKey = New System.Windows.Forms.TextBox()
        txtNode = New System.Windows.Forms.TextBox()
        trvHandles = New System.Windows.Forms.TreeView()
        trvProps = New System.Windows.Forms.TreeView()
        FileView = New System.Windows.Forms.TabPage()
        gbFileView = New System.Windows.Forms.GroupBox()
        btnWriteDXF = New System.Windows.Forms.Button()
        txtFileName = New System.Windows.Forms.TextBox()
        btnBrowseFolder = New System.Windows.Forms.Button()
        txtFolder1 = New System.Windows.Forms.TextBox()
        rtbSgnatures = New System.Windows.Forms.RichTextBox()
        rtbFileView = New System.Windows.Forms.RichTextBox()
        rtbMenu1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        rtbMenu1_Copy = New System.Windows.Forms.ToolStripMenuItem()
        rtbMenu1_SelectAll = New System.Windows.Forms.ToolStripMenuItem()
        FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        GenSettings.SuspendLayout()
        gbShow.SuspendLayout()
        gbViewControl.SuspendLayout()
        gbNatives.SuspendLayout()
        gbTables.SuspendLayout()
        gbSettings.SuspendLayout()
        sbStatus.SuspendLayout()
        TabControl1.SuspendLayout()
        TreeViews.SuspendLayout()
        fraTrees.SuspendLayout()
        FileView.SuspendLayout()
        gbFileView.SuspendLayout()
        rtbMenu1.SuspendLayout()
        SuspendLayout()
        '
        'GenSettings
        '
        GenSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        GenSettings.Controls.Add(Me.cmdReload)
        GenSettings.Controls.Add(Me.gbShow)
        GenSettings.Controls.Add(Me.fraTools)
        GenSettings.Controls.Add(Me.cmdClose)
        GenSettings.Location = New System.Drawing.Point(804, 32)
        GenSettings.Name = "GenSettings"
        GenSettings.Size = New System.Drawing.Size(567, 539)
        GenSettings.TabIndex = 1
        '
        'cmdReload
        '
        cmdReload.Location = New System.Drawing.Point(95, 378)
        cmdReload.Name = "cmdReload"
        cmdReload.Size = New System.Drawing.Size(75, 23)
        cmdReload.TabIndex = 10
        cmdReload.Text = "Load Buffer"
        cmdReload.UseVisualStyleBackColor = True
        '
        'gbShow
        '
        gbShow.Controls.Add(Me.gbViewControl)
        gbShow.Controls.Add(Me.gbNatives)
        gbShow.Controls.Add(Me.gbSettings)
        gbShow.Controls.Add(Me.chkSuppress_4)
        gbShow.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        gbShow.Location = New System.Drawing.Point(2, 9)
        gbShow.Margin = New System.Windows.Forms.Padding(2)
        gbShow.Name = "gbShow"
        gbShow.Padding = New System.Windows.Forms.Padding(2)
        gbShow.Size = New System.Drawing.Size(536, 301)
        gbShow.TabIndex = 9
        gbShow.TabStop = False
        '
        'gbViewControl
        '
        gbViewControl.Controls.Add(Me.chkSuppress_0)
        gbViewControl.Controls.Add(Me.chkIO_2)
        gbViewControl.Controls.Add(Me.chkIO_1)
        gbViewControl.Location = New System.Drawing.Point(316, 185)
        gbViewControl.Name = "gbViewControl"
        gbViewControl.Size = New System.Drawing.Size(218, 106)
        gbViewControl.TabIndex = 21
        gbViewControl.TabStop = False
        gbViewControl.Text = "Output Settings"
        '
        'chkSuppress_0
        '
        chkSuppress_0.AutoSize = True
        chkSuppress_0.Location = New System.Drawing.Point(8, 69)
        chkSuppress_0.Name = "chkSuppress_0"
        chkSuppress_0.Size = New System.Drawing.Size(183, 19)
        chkSuppress_0.TabIndex = 24
        chkSuppress_0.Tag = "SuppressDimOverrides"
        chkSuppress_0.Text = "Suppress Dimstyle Overrides"
        chkSuppress_0.UseVisualStyleBackColor = True
        '
        'chkIO_2
        '
        chkIO_2.AutoSize = True
        chkIO_2.Location = New System.Drawing.Point(13, 44)
        chkIO_2.Name = "chkIO_2"
        chkIO_2.Size = New System.Drawing.Size(159, 19)
        chkIO_2.TabIndex = 23
        chkIO_2.Tag = "HandlesToLongs"
        chkIO_2.Text = "Show Handles as Longs"
        chkIO_2.UseVisualStyleBackColor = True
        '
        'chkIO_1
        '
        chkIO_1.AutoSize = True
        chkIO_1.Location = New System.Drawing.Point(13, 19)
        chkIO_1.Name = "chkIO_1"
        chkIO_1.Size = New System.Drawing.Size(207, 19)
        chkIO_1.TabIndex = 22
        chkIO_1.Tag = "SwitchesToBooleans"
        chkIO_1.Text = "Show Switch Values as Booleans"
        chkIO_1.UseVisualStyleBackColor = True
        '
        'gbNatives
        '
        gbNatives.Controls.Add(Me.CheckBox10)
        gbNatives.Controls.Add(Me.gbTables)
        gbNatives.Controls.Add(Me.chkSuppress_10)
        gbNatives.Controls.Add(Me.chkSuppress_9)
        gbNatives.Controls.Add(Me.chkSuppress_8)
        gbNatives.Controls.Add(Me.chkSuppress_7)
        gbNatives.Controls.Add(Me.chkSuppress_3)
        gbNatives.Controls.Add(Me.chkSuppress_6)
        gbNatives.Controls.Add(Me.chkSuppress_2)
        gbNatives.Controls.Add(Me.chkSuppress_1)
        gbNatives.Controls.Add(Me.chkSuppress_5)
        gbNatives.Location = New System.Drawing.Point(5, 18)
        gbNatives.Name = "gbNatives"
        gbNatives.Size = New System.Drawing.Size(297, 278)
        gbNatives.TabIndex = 20
        gbNatives.TabStop = False
        gbNatives.Text = "What To Show - DXF Native"
        '
        'CheckBox10
        '
        CheckBox10.AutoSize = True
        CheckBox10.Location = New System.Drawing.Point(14, 116)
        CheckBox10.Name = "CheckBox10"
        CheckBox10.Size = New System.Drawing.Size(67, 19)
        CheckBox10.TabIndex = 28
        CheckBox10.Tag = "ShowObjects"
        CheckBox10.Text = "Objects"
        CheckBox10.UseVisualStyleBackColor = True
        '
        'gbTables
        '
        gbTables.Controls.Add(Me.chkTable_9)
        gbTables.Controls.Add(Me.chkTable_8)
        gbTables.Controls.Add(Me.chkTable_6)
        gbTables.Controls.Add(Me.chkTable_5)
        gbTables.Controls.Add(Me.chkTable_4)
        gbTables.Controls.Add(Me.chkTable_3)
        gbTables.Controls.Add(Me.chkTable_1)
        gbTables.Controls.Add(Me.chkTable_2)
        gbTables.Controls.Add(Me.chkTable_7)
        gbTables.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        gbTables.Location = New System.Drawing.Point(8, 148)
        gbTables.Margin = New System.Windows.Forms.Padding(2)
        gbTables.Name = "gbTables"
        gbTables.Padding = New System.Windows.Forms.Padding(2)
        gbTables.Size = New System.Drawing.Size(274, 82)
        gbTables.TabIndex = 27
        gbTables.TabStop = False
        gbTables.Text = "TABLES "
        '
        'chkTable_9
        '
        chkTable_9.AutoSize = True
        chkTable_9.Location = New System.Drawing.Point(148, 57)
        chkTable_9.Name = "chkTable_9"
        chkTable_9.Size = New System.Drawing.Size(121, 19)
        chkTable_9.TabIndex = 22
        chkTable_9.Tag = "ShowTableBLOCKRECORDS"
        chkTable_9.Text = "BLOCK RECORD"
        chkTable_9.UseVisualStyleBackColor = True
        '
        'chkTable_8
        '
        chkTable_8.AutoSize = True
        chkTable_8.Location = New System.Drawing.Point(148, 38)
        chkTable_8.Name = "chkTable_8"
        chkTable_8.Size = New System.Drawing.Size(86, 19)
        chkTable_8.TabIndex = 21
        chkTable_8.Tag = "ShowTableDIMSTYLES"
        chkTable_8.Text = "DIMSTYLE"
        chkTable_8.UseVisualStyleBackColor = True
        '
        'chkTable_6
        '
        chkTable_6.AutoSize = True
        chkTable_6.FlatStyle = System.Windows.Forms.FlatStyle.System
        chkTable_6.Location = New System.Drawing.Point(74, 53)
        chkTable_6.Name = "chkTable_6"
        chkTable_6.Size = New System.Drawing.Size(57, 20)
        chkTable_6.TabIndex = 20
        chkTable_6.Tag = "ShowTableUCSS"
        chkTable_6.Text = "UCS"
        chkTable_6.UseVisualStyleBackColor = True
        '
        'chkTable_5
        '
        chkTable_5.AutoSize = True
        chkTable_5.Location = New System.Drawing.Point(74, 38)
        chkTable_5.Name = "chkTable_5"
        chkTable_5.Size = New System.Drawing.Size(55, 19)
        chkTable_5.TabIndex = 19
        chkTable_5.Tag = "ShowTableVIEWS"
        chkTable_5.Text = "VIEW"
        chkTable_5.UseVisualStyleBackColor = True
        '
        'chkTable_4
        '
        chkTable_4.AutoSize = True
        chkTable_4.Location = New System.Drawing.Point(74, 19)
        chkTable_4.Name = "chkTable_4"
        chkTable_4.Size = New System.Drawing.Size(63, 19)
        chkTable_4.TabIndex = 18
        chkTable_4.Tag = "ShowTableSTYLES"
        chkTable_4.Text = "STYLE"
        chkTable_4.UseVisualStyleBackColor = True
        '
        'chkTable_3
        '
        chkTable_3.AutoSize = True
        chkTable_3.Location = New System.Drawing.Point(5, 57)
        chkTable_3.Name = "chkTable_3"
        chkTable_3.Size = New System.Drawing.Size(64, 19)
        chkTable_3.TabIndex = 17
        chkTable_3.Tag = "ShowTableLAYERS"
        chkTable_3.Text = "LAYER"
        chkTable_3.UseVisualStyleBackColor = True
        '
        'chkTable_1
        '
        chkTable_1.AutoSize = True
        chkTable_1.Location = New System.Drawing.Point(5, 19)
        chkTable_1.Name = "chkTable_1"
        chkTable_1.Size = New System.Drawing.Size(66, 19)
        chkTable_1.TabIndex = 16
        chkTable_1.Tag = "ShowTableVPORTS"
        chkTable_1.Text = "VPORT"
        chkTable_1.UseVisualStyleBackColor = True
        '
        'chkTable_2
        '
        chkTable_2.AutoSize = True
        chkTable_2.Location = New System.Drawing.Point(5, 38)
        chkTable_2.Name = "chkTable_2"
        chkTable_2.Size = New System.Drawing.Size(63, 19)
        chkTable_2.TabIndex = 15
        chkTable_2.Tag = "ShowTableLTYPES"
        chkTable_2.Text = "LTYPE"
        chkTable_2.UseVisualStyleBackColor = True
        '
        'chkTable_7
        '
        chkTable_7.AutoSize = True
        chkTable_7.Location = New System.Drawing.Point(148, 19)
        chkTable_7.Name = "chkTable_7"
        chkTable_7.Size = New System.Drawing.Size(61, 19)
        chkTable_7.TabIndex = 14
        chkTable_7.Tag = "ShowTableAPPIDS"
        chkTable_7.Text = "APPID"
        chkTable_7.UseVisualStyleBackColor = True
        '
        'chkSuppress_10
        '
        chkSuppress_10.AutoSize = True
        chkSuppress_10.ForeColor = System.Drawing.Color.Coral
        chkSuppress_10.Location = New System.Drawing.Point(107, 40)
        chkSuppress_10.Name = "chkSuppress_10"
        chkSuppress_10.Size = New System.Drawing.Size(110, 19)
        chkSuppress_10.TabIndex = 26
        chkSuppress_10.Tag = "ShowHiddenObjects"
        chkSuppress_10.Text = "Hidden Objects"
        chkSuppress_10.UseVisualStyleBackColor = True
        '
        'chkSuppress_9
        '
        chkSuppress_9.AutoSize = True
        chkSuppress_9.Location = New System.Drawing.Point(14, 78)
        chkSuppress_9.Name = "chkSuppress_9"
        chkSuppress_9.Size = New System.Drawing.Size(62, 19)
        chkSuppress_9.TabIndex = 25
        chkSuppress_9.Tag = "ShowBlocks"
        chkSuppress_9.Text = "Blocks"
        chkSuppress_9.UseVisualStyleBackColor = True
        '
        'chkSuppress_8
        '
        chkSuppress_8.AutoSize = True
        chkSuppress_8.Location = New System.Drawing.Point(14, 97)
        chkSuppress_8.Name = "chkSuppress_8"
        chkSuppress_8.Size = New System.Drawing.Size(66, 19)
        chkSuppress_8.TabIndex = 24
        chkSuppress_8.Tag = "ShowEntities"
        chkSuppress_8.Text = "Entities"
        chkSuppress_8.UseVisualStyleBackColor = True
        '
        'chkSuppress_7
        '
        chkSuppress_7.AutoSize = True
        chkSuppress_7.Location = New System.Drawing.Point(14, 59)
        chkSuppress_7.Name = "chkSuppress_7"
        chkSuppress_7.Size = New System.Drawing.Size(63, 19)
        chkSuppress_7.TabIndex = 23
        chkSuppress_7.Tag = "ShowTables"
        chkSuppress_7.Text = "Tables"
        chkSuppress_7.UseVisualStyleBackColor = True
        '
        'chkSuppress_3
        '
        chkSuppress_3.AutoSize = True
        chkSuppress_3.Location = New System.Drawing.Point(14, 40)
        chkSuppress_3.Name = "chkSuppress_3"
        chkSuppress_3.Size = New System.Drawing.Size(69, 19)
        chkSuppress_3.TabIndex = 22
        chkSuppress_3.Tag = "ShowClasses"
        chkSuppress_3.Text = "Classes"
        chkSuppress_3.UseVisualStyleBackColor = True
        '
        'chkSuppress_6
        '
        chkSuppress_6.AutoSize = True
        chkSuppress_6.Location = New System.Drawing.Point(107, 59)
        chkSuppress_6.Name = "chkSuppress_6"
        chkSuppress_6.Size = New System.Drawing.Size(125, 19)
        chkSuppress_6.TabIndex = 21
        chkSuppress_6.Tag = "ShowHiddenProperties"
        chkSuppress_6.Text = "Hidden Properties"
        chkSuppress_6.UseVisualStyleBackColor = True
        '
        'chkSuppress_2
        '
        chkSuppress_2.AutoSize = True
        chkSuppress_2.Location = New System.Drawing.Point(14, 21)
        chkSuppress_2.Name = "chkSuppress_2"
        chkSuppress_2.Size = New System.Drawing.Size(67, 19)
        chkSuppress_2.TabIndex = 20
        chkSuppress_2.Tag = "ShowHeader"
        chkSuppress_2.Text = "Header"
        chkSuppress_2.UseVisualStyleBackColor = True
        '
        'chkSuppress_1
        '
        chkSuppress_1.AutoSize = True
        chkSuppress_1.Location = New System.Drawing.Point(107, 78)
        chkSuppress_1.Name = "chkSuppress_1"
        chkSuppress_1.Size = New System.Drawing.Size(151, 19)
        chkSuppress_1.TabIndex = 19
        chkSuppress_1.Tag = "ShowSuppressedProperties"
        chkSuppress_1.Text = "Suppressed Properties"
        chkSuppress_1.UseVisualStyleBackColor = True
        '
        'chkSuppress_5
        '
        chkSuppress_5.AutoSize = True
        chkSuppress_5.Location = New System.Drawing.Point(107, 21)
        chkSuppress_5.Name = "chkSuppress_5"
        chkSuppress_5.Size = New System.Drawing.Size(78, 19)
        chkSuppress_5.TabIndex = 17
        chkSuppress_5.Tag = "ShowInstances"
        chkSuppress_5.Text = "Instances"
        chkSuppress_5.UseVisualStyleBackColor = True
        '
        'gbSettings
        '
        gbSettings.Controls.Add(Me.chkSettings_0)
        gbSettings.Controls.Add(Me.chkSettings_7)
        gbSettings.Controls.Add(Me.chkSettings_5)
        gbSettings.Controls.Add(Me.chkSettings_4)
        gbSettings.Controls.Add(Me.chkSettings_3)
        gbSettings.Controls.Add(Me.chkSettings_6)
        gbSettings.Controls.Add(Me.chkSettings_2)
        gbSettings.Controls.Add(Me.chkSettings_8)
        gbSettings.Controls.Add(Me.chkSettings_1)
        gbSettings.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        gbSettings.Location = New System.Drawing.Point(316, 23)
        gbSettings.Margin = New System.Windows.Forms.Padding(2)
        gbSettings.Name = "gbSettings"
        gbSettings.Padding = New System.Windows.Forms.Padding(2)
        gbSettings.Size = New System.Drawing.Size(213, 138)
        gbSettings.TabIndex = 19
        gbSettings.TabStop = False
        gbSettings.Text = "Settings To Show - DXF Graphics"
        '
        'chkSettings_0
        '
        chkSettings_0.AutoSize = True
        chkSettings_0.Location = New System.Drawing.Point(41, 19)
        chkSettings_0.Name = "chkSettings_0"
        chkSettings_0.Size = New System.Drawing.Size(134, 19)
        chkSettings_0.TabIndex = 22
        chkSettings_0.Tag = "SuppressSettings"
        chkSettings_0.Text = "Supress All Settings"
        chkSettings_0.UseVisualStyleBackColor = True
        '
        'chkSettings_7
        '
        chkSettings_7.AutoSize = True
        chkSettings_7.Location = New System.Drawing.Point(126, 101)
        chkSettings_7.Name = "chkSettings_7"
        chkSettings_7.Size = New System.Drawing.Size(66, 19)
        chkSettings_7.TabIndex = 21
        chkSettings_7.Tag = "ShowDisplaySettings"
        chkSettings_7.Text = "Display"
        chkSettings_7.UseVisualStyleBackColor = True
        '
        'chkSettings_5
        '
        chkSettings_5.AutoSize = True
        chkSettings_5.Location = New System.Drawing.Point(126, 82)
        chkSettings_5.Name = "chkSettings_5"
        chkSettings_5.Size = New System.Drawing.Size(49, 19)
        chkSettings_5.TabIndex = 20
        chkSettings_5.Tag = "ShowTextSettings"
        chkSettings_5.Text = "Text"
        chkSettings_5.UseVisualStyleBackColor = True
        '
        'chkSettings_4
        '
        chkSettings_4.AutoSize = True
        chkSettings_4.Location = New System.Drawing.Point(126, 63)
        chkSettings_4.Name = "chkSettings_4"
        chkSettings_4.Size = New System.Drawing.Size(57, 19)
        chkSettings_4.TabIndex = 19
        chkSettings_4.Tag = "ShowTableSettings"
        chkSettings_4.Text = "Table"
        chkSettings_4.UseVisualStyleBackColor = True
        '
        'chkSettings_3
        '
        chkSettings_3.AutoSize = True
        chkSettings_3.Location = New System.Drawing.Point(126, 44)
        chkSettings_3.Name = "chkSettings_3"
        chkSettings_3.Size = New System.Drawing.Size(78, 19)
        chkSettings_3.TabIndex = 18
        chkSettings_3.Tag = "ShowLTLSettings"
        chkSettings_3.Text = "Linetypes"
        chkSettings_3.UseVisualStyleBackColor = True
        '
        'chkSettings_6
        '
        chkSettings_6.AutoSize = True
        chkSettings_6.Location = New System.Drawing.Point(18, 101)
        chkSettings_6.Name = "chkSettings_6"
        chkSettings_6.Size = New System.Drawing.Size(104, 19)
        chkSettings_6.TabIndex = 17
        chkSettings_6.Tag = "ShowDimOverrideSettings"
        chkSettings_6.Text = "Dim Overrides"
        chkSettings_6.UseVisualStyleBackColor = True
        '
        'chkSettings_2
        '
        chkSettings_2.AutoSize = True
        chkSettings_2.Location = New System.Drawing.Point(18, 82)
        chkSettings_2.Name = "chkSettings_2"
        chkSettings_2.Size = New System.Drawing.Size(73, 19)
        chkSettings_2.TabIndex = 16
        chkSettings_2.Tag = "ShowSymbolSettings"
        chkSettings_2.Text = "Symbols"
        chkSettings_2.UseVisualStyleBackColor = True
        '
        'chkSettings_8
        '
        chkSettings_8.AutoSize = True
        chkSettings_8.Location = New System.Drawing.Point(18, 63)
        chkSettings_8.Name = "chkSettings_8"
        chkSettings_8.Size = New System.Drawing.Size(92, 19)
        chkSettings_8.TabIndex = 15
        chkSettings_8.Tag = "ShowDimSettings"
        chkSettings_8.Text = "Dimensions"
        chkSettings_8.UseVisualStyleBackColor = True
        '
        'chkSettings_1
        '
        chkSettings_1.AutoSize = True
        chkSettings_1.Location = New System.Drawing.Point(18, 44)
        chkSettings_1.Name = "chkSettings_1"
        chkSettings_1.Size = New System.Drawing.Size(65, 19)
        chkSettings_1.TabIndex = 14
        chkSettings_1.Tag = "ShowScreenSettings"
        chkSettings_1.Text = "Screen"
        chkSettings_1.UseVisualStyleBackColor = True
        '
        'chkSuppress_4
        '
        chkSuppress_4.AutoSize = True
        chkSuppress_4.Location = New System.Drawing.Point(30, 498)
        chkSuppress_4.Name = "chkSuppress_4"
        chkSuppress_4.Size = New System.Drawing.Size(67, 19)
        chkSuppress_4.TabIndex = 4
        chkSuppress_4.Tag = "ShowObjects"
        chkSuppress_4.Text = "Objects"
        chkSuppress_4.UseVisualStyleBackColor = True
        '
        'fraTools
        '
        fraTools.Location = New System.Drawing.Point(15, 315)
        fraTools.Name = "fraTools"
        fraTools.Size = New System.Drawing.Size(523, 57)
        fraTools.TabIndex = 5
        '
        'cmdClose
        '
        cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        cmdClose.Location = New System.Drawing.Point(389, 378)
        cmdClose.Name = "cmdClose"
        cmdClose.Size = New System.Drawing.Size(75, 23)
        cmdClose.TabIndex = 4
        cmdClose.Text = "Close"
        cmdClose.UseVisualStyleBackColor = True
        '
        'sbStatus
        '
        sbStatus.ImageScalingSize = New System.Drawing.Size(20, 20)
        sbStatus.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1})
        sbStatus.Location = New System.Drawing.Point(0, 580)
        sbStatus.Name = "sbStatus"
        sbStatus.Size = New System.Drawing.Size(1389, 22)
        sbStatus.TabIndex = 2
        sbStatus.Text = "StatusStrip1"
        '
        'ToolStripStatusLabel1
        '
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New System.Drawing.Size(1374, 17)
        ToolStripStatusLabel1.Spring = True
        '
        'TabControl1
        '
        TabControl1.Controls.Add(Me.TreeViews)
        TabControl1.Controls.Add(Me.FileView)
        TabControl1.Location = New System.Drawing.Point(9, 10)
        TabControl1.Margin = New System.Windows.Forms.Padding(2)
        TabControl1.Name = "TabControl1"
        TabControl1.SelectedIndex = 0
        TabControl1.Size = New System.Drawing.Size(794, 561)
        TabControl1.TabIndex = 3
        '
        'TreeViews
        '
        TreeViews.BackColor = System.Drawing.SystemColors.Control
        TreeViews.Controls.Add(Me.fraTrees)
        TreeViews.Location = New System.Drawing.Point(4, 22)
        TreeViews.Margin = New System.Windows.Forms.Padding(2)
        TreeViews.Name = "TreeViews"
        TreeViews.Padding = New System.Windows.Forms.Padding(2)
        TreeViews.Size = New System.Drawing.Size(786, 535)
        TreeViews.TabIndex = 0
        TreeViews.Text = "Tree View"
        '
        'fraTrees
        '
        fraTrees.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        fraTrees.Controls.Add(Me.txtErrorKey)
        fraTrees.Controls.Add(Me.trvErrors)
        fraTrees.Controls.Add(Me.txtHandleKey)
        fraTrees.Controls.Add(Me.txtNode)
        fraTrees.Controls.Add(Me.trvHandles)
        fraTrees.Controls.Add(Me.trvProps)
        fraTrees.Location = New System.Drawing.Point(5, 6)
        fraTrees.Name = "fraTrees"
        fraTrees.Size = New System.Drawing.Size(776, 526)
        fraTrees.TabIndex = 1
        '
        'txtErrorKey
        '
        txtErrorKey.Location = New System.Drawing.Point(3, 501)
        txtErrorKey.Name = "txtErrorKey"
        txtErrorKey.Size = New System.Drawing.Size(768, 20)
        txtErrorKey.TabIndex = 5
        '
        'trvErrors
        '
        trvErrors.Location = New System.Drawing.Point(3, 406)
        trvErrors.Name = "trvErrors"
        trvErrors.Size = New System.Drawing.Size(768, 89)
        trvErrors.TabIndex = 4
        '
        'txtHandleKey
        '
        txtHandleKey.Location = New System.Drawing.Point(532, 380)
        txtHandleKey.Name = "txtHandleKey"
        txtHandleKey.Size = New System.Drawing.Size(239, 20)
        txtHandleKey.TabIndex = 3
        '
        'txtNode
        '
        txtNode.Location = New System.Drawing.Point(3, 380)
        txtNode.Name = "txtNode"
        txtNode.Size = New System.Drawing.Size(523, 20)
        txtNode.TabIndex = 2
        '
        'trvHandles
        '
        trvHandles.Location = New System.Drawing.Point(532, 3)
        trvHandles.Name = "trvHandles"
        trvHandles.Size = New System.Drawing.Size(239, 363)
        trvHandles.TabIndex = 1
        '
        'trvProps
        '
        trvProps.Location = New System.Drawing.Point(3, 3)
        trvProps.Name = "trvProps"
        trvProps.Size = New System.Drawing.Size(523, 363)
        trvProps.TabIndex = 0
        '
        'FileView
        '
        FileView.BackColor = System.Drawing.SystemColors.ButtonFace
        FileView.Controls.Add(Me.gbFileView)
        FileView.Location = New System.Drawing.Point(4, 22)
        FileView.Margin = New System.Windows.Forms.Padding(2)
        FileView.Name = "FileView"
        FileView.Padding = New System.Windows.Forms.Padding(2)
        FileView.Size = New System.Drawing.Size(786, 535)
        FileView.TabIndex = 1
        FileView.Text = "File View"
        '
        'gbFileView
        '
        gbFileView.Controls.Add(Me.btnWriteDXF)
        gbFileView.Controls.Add(Me.txtFileName)
        gbFileView.Controls.Add(Me.btnBrowseFolder)
        gbFileView.Controls.Add(Me.txtFolder1)
        gbFileView.Controls.Add(Me.rtbSgnatures)
        gbFileView.Controls.Add(Me.rtbFileView)
        gbFileView.Location = New System.Drawing.Point(15, 10)
        gbFileView.Name = "gbFileView"
        gbFileView.Size = New System.Drawing.Size(766, 520)
        gbFileView.TabIndex = 26
        gbFileView.TabStop = False
        '
        'btnWriteDXF
        '
        btnWriteDXF.Location = New System.Drawing.Point(318, 459)
        btnWriteDXF.Name = "btnWriteDXF"
        btnWriteDXF.Size = New System.Drawing.Size(93, 25)
        btnWriteDXF.TabIndex = 31
        btnWriteDXF.Text = "Write DXF"
        btnWriteDXF.UseVisualStyleBackColor = True
        '
        'txtFileName
        '
        txtFileName.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        txtFileName.Location = New System.Drawing.Point(13, 459)
        txtFileName.Name = "txtFileName"
        txtFileName.Size = New System.Drawing.Size(285, 22)
        txtFileName.TabIndex = 30
        '
        'btnBrowseFolder
        '
        btnBrowseFolder.Location = New System.Drawing.Point(599, 487)
        btnBrowseFolder.Name = "btnBrowseFolder"
        btnBrowseFolder.Size = New System.Drawing.Size(50, 23)
        btnBrowseFolder.TabIndex = 29
        btnBrowseFolder.Text = "..."
        btnBrowseFolder.UseVisualStyleBackColor = True
        '
        'txtFolder1
        '
        txtFolder1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        txtFolder1.Location = New System.Drawing.Point(13, 488)
        txtFolder1.Name = "txtFolder1"
        txtFolder1.Size = New System.Drawing.Size(580, 22)
        txtFolder1.TabIndex = 28
        '
        'rtbSgnatures
        '
        rtbSgnatures.Location = New System.Drawing.Point(185, 18)
        rtbSgnatures.Margin = New System.Windows.Forms.Padding(2)
        rtbSgnatures.Name = "rtbSgnatures"
        rtbSgnatures.Size = New System.Drawing.Size(543, 429)
        rtbSgnatures.TabIndex = 27
        rtbSgnatures.Text = ""
        '
        'rtbFileView
        '
        rtbFileView.ContextMenuStrip = rtbMenu1
        rtbFileView.Location = New System.Drawing.Point(9, 18)
        rtbFileView.Margin = New System.Windows.Forms.Padding(2)
        rtbFileView.Name = "rtbFileView"
        rtbFileView.Size = New System.Drawing.Size(172, 429)
        rtbFileView.TabIndex = 26
        rtbFileView.Text = ""
        '
        'rtbMenu1
        '
        rtbMenu1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.rtbMenu1_Copy, rtbMenu1_SelectAll})
        rtbMenu1.Name = "rtbMenu1"
        rtbMenu1.Size = New System.Drawing.Size(123, 48)
        '
        'rtbMenu1_Copy
        '
        rtbMenu1_Copy.Name = "rtbMenu1_Copy"
        rtbMenu1_Copy.Size = New System.Drawing.Size(122, 22)
        rtbMenu1_Copy.Text = "Copy"
        '
        'rtbMenu1_SelectAll
        '
        rtbMenu1_SelectAll.Name = "rtbMenu1_SelectAll"
        rtbMenu1_SelectAll.Size = New System.Drawing.Size(122, 22)
        rtbMenu1_SelectAll.Text = "Select All"
        '
        'frmDXFProperties
        '
        AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        CancelButton = cmdClose
        ClientSize = New System.Drawing.Size(1389, 602)
        ControlBox = False
        Controls.Add(Me.TabControl1)
        Controls.Add(Me.sbStatus)
        Controls.Add(Me.GenSettings)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Name = "frmDXFProperties"
        ShowInTaskbar = False
        SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Text = "dxfImage Properties"
        GenSettings.ResumeLayout(False)
        gbShow.ResumeLayout(False)
        gbShow.PerformLayout()
        gbViewControl.ResumeLayout(False)
        gbViewControl.PerformLayout()
        gbNatives.ResumeLayout(False)
        gbNatives.PerformLayout()
        gbTables.ResumeLayout(False)
        gbTables.PerformLayout()
        gbSettings.ResumeLayout(False)
        gbSettings.PerformLayout()
        sbStatus.ResumeLayout(False)
        sbStatus.PerformLayout()
        TabControl1.ResumeLayout(False)
        TreeViews.ResumeLayout(False)
        fraTrees.ResumeLayout(False)
        fraTrees.PerformLayout()
        FileView.ResumeLayout(False)
        gbFileView.ResumeLayout(False)
        gbFileView.PerformLayout()
        rtbMenu1.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents GenSettings As Panel
    Friend WithEvents cmdClose As Button
    Friend WithEvents fraTools As Panel
    Friend WithEvents sbStatus As StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TreeViews As TabPage
    Friend WithEvents FileView As TabPage
    Friend WithEvents gbShow As GroupBox
    Friend WithEvents chkSuppress_4 As CheckBox
    Friend WithEvents gbNatives As GroupBox
    Friend WithEvents chkSuppress_10 As CheckBox
    Friend WithEvents chkSuppress_9 As CheckBox
    Friend WithEvents chkSuppress_8 As CheckBox
    Friend WithEvents chkSuppress_7 As CheckBox
    Friend WithEvents chkSuppress_3 As CheckBox
    Friend WithEvents chkSuppress_6 As CheckBox
    Friend WithEvents chkSuppress_2 As CheckBox
    Friend WithEvents chkSuppress_1 As CheckBox
    Friend WithEvents chkSuppress_5 As CheckBox
    Friend WithEvents gbSettings As GroupBox
    Friend WithEvents chkSettings_7 As CheckBox
    Friend WithEvents chkSettings_5 As CheckBox
    Friend WithEvents chkSettings_4 As CheckBox
    Friend WithEvents chkSettings_3 As CheckBox
    Friend WithEvents chkSettings_6 As CheckBox
    Friend WithEvents chkSettings_2 As CheckBox
    Friend WithEvents chkSettings_8 As CheckBox
    Friend WithEvents chkSettings_1 As CheckBox
    Friend WithEvents cmdReload As Button
    Friend WithEvents gbTables As GroupBox
    Friend WithEvents chkTable_9 As CheckBox
    Friend WithEvents chkTable_8 As CheckBox
    Friend WithEvents chkTable_6 As CheckBox
    Friend WithEvents chkTable_5 As CheckBox
    Friend WithEvents chkTable_4 As CheckBox
    Friend WithEvents chkTable_3 As CheckBox
    Friend WithEvents chkTable_1 As CheckBox
    Friend WithEvents chkTable_2 As CheckBox
    Friend WithEvents chkTable_7 As CheckBox
    Friend WithEvents fraTrees As Panel
    Friend WithEvents txtErrorKey As TextBox
    Friend WithEvents trvErrors As TreeView
    Friend WithEvents txtHandleKey As TextBox
    Friend WithEvents txtNode As TextBox
    Friend WithEvents trvHandles As TreeView
    Friend WithEvents trvProps As TreeView
    Friend WithEvents CheckBox10 As CheckBox
    Friend WithEvents chkSettings_0 As CheckBox
    Friend WithEvents gbViewControl As GroupBox
    Friend WithEvents chkIO_2 As CheckBox
    Friend WithEvents chkIO_1 As CheckBox
    Friend WithEvents rtbMenu1 As ContextMenuStrip
    Friend WithEvents rtbMenu1_Copy As ToolStripMenuItem
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents rtbMenu1_SelectAll As ToolStripMenuItem
    Friend WithEvents chkSuppress_0 As CheckBox
    Friend WithEvents gbFileView As GroupBox
    Friend WithEvents btnBrowseFolder As Button
    Friend WithEvents txtFolder1 As TextBox
    Friend WithEvents rtbSgnatures As RichTextBox
    Friend WithEvents rtbFileView As RichTextBox
    Friend WithEvents btnWriteDXF As Button
    Friend WithEvents txtFileName As TextBox
End Class
