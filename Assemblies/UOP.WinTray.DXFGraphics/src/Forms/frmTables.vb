Imports SharpDX.Direct2D1
Imports UOP.DXFGraphics
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Public Class frmTables
    Private Enum EditMode
        Format = 1
        SelectTablular = 2
        SelectCombo = 3
    End Enum
#Region "Members"
    Private _Image As dxfImage
    Private _InitTable As dxfTable
    Private _curTable As dxfTable
    Private _newTable As dxfTable
    Private _Redraw As Boolean
    Private _ToggleScreen As Boolean
    Private _Changed As Boolean
    Private _SelectedName As String
    Private _SelectedHandle As String
    Private _SelectedRow As Integer
    Private _SelectedColumn As Integer
    Private _Loaded As Boolean
    Private _Deleted As dxfTable
    Private _Added As dxfTable
    Private _Mode As EditMode = EditMode.Format
    Private _ModeName As String = "FORMAT"
    Private _CurrentCell As DataGridViewCell
    Private _SkipList As String = String.Empty
    Private _RefType As dxxReferenceTypes = dxxReferenceTypes.UNDEFINED
    Private _RefTypeName As String
    Private _ShowLogicals As Boolean = False
    Private _InitHeader As dxsHeader
    Private _Working As Boolean
    Private _Canceled As Boolean
    Private _OpCanceled As Boolean
    Private _CurName As String
    Private _InitRow As Integer = -1
    Private WithEvents _Grid As DataGridView = Nothing
    Private WithEvents _AcceptButton As Button
    Private WithEvents _CancelButton As Button
    Private _Colors As System.Collections.Generic.Dictionary(Of String, Color)
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        pnlFormat.Visible = False
        pnlSelect.Visible = False
        pnlSelectCombo.Visible = False
        Text = "Table Editor"
    End Sub
#End Region
#Region "Properties"
    Private Property Mode As EditMode
        Get
            Return _Mode
        End Get
        Set(value As EditMode)
            _Mode = value
            Select Case value
                Case EditMode.Format
                    _Grid = gdFormat
                    _ModeName = "FORMAT"
                    _Grid.EditMode = DataGridViewEditMode.EditOnEnter
                Case EditMode.SelectTablular
                    _Grid = gdSelect
                    _ModeName = "SELECT"
                    _Grid.EditMode = DataGridViewEditMode.EditOnEnter
                Case EditMode.SelectCombo
                    _ModeName = "SELECT"
                    _Grid = Nothing
            End Select
        End Set
    End Property
    Friend Property RefType As dxxReferenceTypes
        Get
            Return _RefType
        End Get
        Set(value As dxxReferenceTypes)
            If _RefType = value Then Return
            _RefType = value
            _RefTypeName = dxfEnums.DisplayName(value)

            If _RefType = dxxReferenceTypes.LTYPE Then

                _InitTable = dxfTable.FromTTABLE(dxfLinetypes.GlobalLinetypes)
            Else

                _InitTable = Nothing
                _Image.Tables.TryGetTable(_RefType, _InitTable, True)

            End If
            Dim bTbl As New TTABLE(_InitTable)
            bTbl.ImageGUID = ""   ' so our table won't aleter the image on member or count changes
            bTbl.GUID = _InitTable.GUID
            _Deleted = dxfTable.FromTTABLE(bTbl)
            _Added = dxfTable.FromTTABLE(bTbl)
        End Set
    End Property
    Friend Property SkipList As String
        Get
            Return _SkipList
        End Get
        Set(value As String)
            If Not value Is Nothing Then _SkipList = value.Trim Else _SkipList = ""
        End Set
    End Property
#End Region 'Properties
#Region "Methods"
    Friend Function ShowSelect(aOwner As IWin32Window, aRefType As dxxReferenceTypes, aImage As dxfImage, aInitName As String, Optional bComboStyle As Boolean = False, Optional aShowLogicals As Boolean = False, Optional bAllowAdd As Boolean = True) As String
        _ShowLogicals = aShowLogicals
        _Image = aImage
        Select Case aRefType
            Case dxxReferenceTypes.LAYER
                If String.IsNullOrWhiteSpace(aInitName) Then aInitName = aImage.Header.LayerName
                If aImage Is Nothing Then Return aInitName
                If aInitName IsNot Nothing Then _CurName = aInitName.Trim Else _CurName = aImage.Header.LayerName
            Case dxxReferenceTypes.LTYPE
                If String.IsNullOrWhiteSpace(aInitName) Then aInitName = aImage.Header.Linetype
                _CurName = aInitName.Trim
        End Select
        If bComboStyle Then Mode = EditMode.SelectCombo Else Mode = EditMode.SelectTablular
        RefType = aRefType
        ConfigureForm()
        LoadList()
        ShowDialog(aOwner)
        If Not _Canceled Then
            Return _SelectedName
        Else
            Return aInitName
        End If
        TerminateObjects()
    End Function
    Friend Function ShowFormat(aOwner As IWin32Window, aRefType As dxxReferenceTypes, aImage As dxfImage, bAllowAdd As Boolean, ByRef rRedraw As Boolean) As Boolean
        rRedraw = False
        If aImage Is Nothing Then Return False
        _Image = aImage
        Mode = EditMode.Format
        RefType = aRefType
        cmdAdd.Enabled = bAllowAdd
        cmdDelete.Enabled = bAllowAdd
        _InitHeader = _Image.Header
        If RefType = dxxReferenceTypes.LAYER Then _CurName = _InitHeader.LayerName
        ConfigureForm()
        LoadList()
        ShowDialog(aOwner)
        Dim _rVal As Boolean = _Changed And Not _Canceled
        If Not _Canceled Then
            If _Added.Count > 0 Then _rVal = True
            If _Deleted.Count > 0 Then _rVal = True
        End If
        If _rVal Then
            rRedraw = _Redraw
            Dim aNewMem As dxfTableEntry
            Dim i As Integer
            Dim aTbl As dxfTable = Nothing

            If _Image.Tables.TryGetTable(aRefType, aTbl, True) Then

                Dim aErr As String = String.Empty
                If _rVal Then
                    If _Added.Count > 0 Then
                        For i = 1 To _Added.Count
                            aNewMem = _Added.Item(i)
                            _Image.RespondToTableEvent(_newTable, dxxCollectionEventTypes.Add, aNewMem, False, False, False, aErr)
                            _newTable.UpdateEntry = aNewMem
                        Next
                    End If
                    If _Deleted.Count > 0 Then
                        For i = 1 To _Deleted.Count
                            aNewMem = _Deleted.Item(i)
                            If _InitTable.MemberExists(aNewMem.Handle) Then
                                _Image.RespondToTableEvent(_newTable, dxxCollectionEventTypes.Remove, aNewMem, False, False, aErr)
                            End If
                        Next
                    End If
                    aImage.Tables.UpdateTable(_newTable.Strukture)
                End If
                If _ToggleScreen Then
                    Dim aScrn As TSCREEN = _Image.obj_SCREEN
                    If aScrn.Entities.Count > 0 Then rRedraw = True
                    aScrn.Suppressed = chkScreen.Checked
                    _Image.obj_SCREEN = aScrn
                End If
                'If bRedraw Then aImage.Display.Redraw()
            End If
            TerminateObjects()
        End If

        Return _rVal
    End Function
    Private Sub ConfigureForm()
        _Canceled = True
        _OpCanceled = True
        _Loaded = False
        _AcceptButton = Nothing
        _CancelButton = Nothing
        _SelectedName = ""
        _SelectedRow = -1
        _SelectedHandle = ""
        Dim sizeTo As GroupBox = Nothing
        lblSelectedVal0.Text = ""
        lblSelectedVal1.Text = ""
        lblCurrentVal1.Text = ""
        pnlFormat.Visible = (Mode = EditMode.Format)
        pnlSelectCombo.Visible = (Mode = EditMode.SelectCombo)
        pnlSelect.Visible = (Mode = EditMode.SelectTablular)
        If _Image IsNot Nothing Then _InitHeader = _Image.Header
        Select Case Mode
            Case EditMode.SelectTablular
                sizeTo = pnlSelect
                cmdCancel2.DialogResult = DialogResult.Cancel
                CancelButton = cmdCancel2
                sizeTo.Text = "Select " & _RefTypeName
                Text = "Select " & _RefTypeName
                _AcceptButton = cmdSelect
                _Grid.Columns("Description_SELECT").Visible = RefType = dxxReferenceTypes.LTYPE Or RefType = dxxReferenceTypes.LAYER
            Case EditMode.SelectCombo
                sizeTo = pnlSelectCombo
                sizeTo.Text = "Select " & _RefTypeName
                cmdCancel3.DialogResult = DialogResult.Cancel
                CancelButton = cmdCancel3
                _AcceptButton = cmdSelect1
                Text = "Select " & _RefTypeName
            Case EditMode.Format
                sizeTo = pnlFormat
                sizeTo.Text = "Configure " & _RefTypeName & "s"
                Text = "Configure " & _RefTypeName & "s"
                cmdCancel.DialogResult = DialogResult.Cancel
                _AcceptButton = cmdOk
                _CancelButton = cmdCancel
        End Select
        CancelButton = _CancelButton
        Text = "Drawing " & _RefTypeName & "s"
        sizeTo.Location = New Point(9, 12)
        lblSelected0.Text = "Selected " & _RefTypeName & ":"
        lblSelected1.Text = "Selected " & _RefTypeName & ":"
        lblCurrent.Text = "Current " & _RefTypeName & ":"
        Width = sizeTo.Width + 4.5 * sizeTo.Left
        Height = sizeTo.Height + 4.5 * sizeTo.Top
        Select Case _RefType
            Case dxxReferenceTypes.LAYER
        End Select
    End Sub
    Friend Sub DisposeAll()
        _Image = Nothing
        _AcceptButton = Nothing
        _CancelButton = Nothing
        _Grid = Nothing
        Dispose()
    End Sub
    Private Sub LoadCombo(aColumnName As String, aList As List(Of String), Optional aSelName As String = "", Optional bNoReload As Boolean = False)
        If _Image Is Nothing Or _Grid Is Nothing Then Return
        If Not _Grid.Columns.Contains(aColumnName) Then Return
        Dim aCol As DataGridViewColumn = _Grid.Columns.Item(aColumnName)
        If TypeOf aCol IsNot DataGridViewComboBoxColumn Then Return
        Dim aCol1 As DataGridViewComboBoxColumn = aCol

        Try
            If Not bNoReload Then
                aCol1.Items.Clear()
                'load the linetypes into the combo
                For i As Integer = 1 To aList.Count
                    Dim sVal As String = aList.Item(i - 1)
                    aCol1.Items.Add(sVal)
                Next
            Else
                Dim aRow As DataGridViewRow
                Dim aCell As DataGridViewComboBoxCell
                For j As Integer = 0 To _Grid.Rows.Count - 1
                    aRow = _Grid.Rows.Item(j)
                    aCell = aRow.Cells.Item(aColumnName)
                    For i As Integer = 1 To aList.Count
                        Dim sVal As String = aList.Item(i - 1)
                        aCell.Items.Add(sVal)
                    Next
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub LoadList(Optional aSelName As String = "", Optional bNoReload As Boolean = False)
        Dim i As Integer
        _InitRow = -1
        Dim imgCol As DataGridViewImageColumn
        Dim iRow As Integer = 1
        Dim bSkip As Boolean
        Dim aEntry As dxfTableEntry
        Dim idx As Integer = -1
        If _Grid IsNot Nothing Then
            _Grid.RowCount = 1
            _Grid.Columns.Item("Status_" & _ModeName).ReadOnly = True
            _Grid.MultiSelect = False
            _Grid.Columns.Item("RefName_" & _ModeName).ReadOnly = True
        End If
        If RefType = dxxReferenceTypes.LTYPE Then
            Dim aLT As dxoLinetype = Nothing
            If _ShowLogicals Then
                If Not _InitTable.MemberExists(dxfLinetypes.ByLayer) And String.Compare(_CurName, dxfLinetypes.ByLayer, True) <> 0 Then
                    If _Image IsNot Nothing Then aLT = _Image.Linetypes.Entry(dxfLinetypes.ByLayer) Else aLT = Nothing
                    If aLT Is Nothing Then aLT = New dxoLinetype(dxfLinetypes.ByLayer)
                    AddRow(aLT, iRow)
                    iRow += 1
                End If
                If Not _InitTable.MemberExists(dxfLinetypes.ByBlock) And String.Compare(_CurName, dxfLinetypes.ByBlock, True) <> 0 Then
                    If _Image IsNot Nothing Then aLT = _Image.Linetypes.Entry(dxfLinetypes.ByBlock) Else aLT = Nothing
                    If aLT Is Nothing Then aLT = New dxoLinetype(dxfLinetypes.ByBlock)
                    AddRow(aLT, iRow)
                    iRow += 1
                End If
            End If
            If Not _InitTable.MemberExists(dxfLinetypes.Continuous) And String.Compare(_CurName, dxfLinetypes.Continuous, True) <> 0 Then
                If _Image IsNot Nothing Then aLT = _Image.Linetypes.Entry(dxfLinetypes.Continuous) Else aLT = Nothing
                If aLT Is Nothing Then aLT = New dxoLinetype(dxfLinetypes.Continuous)
                AddRow(aLT, iRow)
                iRow += 1
            End If
            If _CurName <> "" And Not _InitTable.MemberExists(_CurName) Then
                If _Image IsNot Nothing Then aLT = _Image.Linetypes.Entry(_CurName) Else aLT = Nothing
                If aLT Is Nothing Then aLT = New dxoLinetype(_CurName)
                AddRow(aLT, iRow)
                If Mode = EditMode.SelectCombo Then _InitRow = cboEntries.Items.Count - 1 Else _InitRow = _Grid.Rows.Count - 1
                iRow += 1
            End If
        End If
        Select Case Mode
            Case EditMode.SelectCombo
                cboEntries.Items.Clear()
                For i = 1 To _InitTable.Count
                    Try
                        aEntry = _InitTable.Item(i)
                        If String.Compare(aEntry.Name, _CurName, True) = 0 Then
                            idx = i - 1
                            _InitRow = idx
                        End If
                        cboEntries.Items.Add(aEntry.Name)
                        If String.Compare(aEntry.Name, _CurName, True) = 0 Then
                            _InitRow = cboEntries.Items.Count - 1
                        End If
                    Catch ex As Exception
                    Finally
                        iRow += 1
                    End Try
                Next i
            Case EditMode.SelectTablular
                _InitRow = -1
                _Grid.Columns.Item("Status_" & _ModeName).Visible = False
                For i = 1 To _InitTable.Count
                    Try
                        aEntry = _InitTable.Item(i)
                        bSkip = Not TLISTS.Contains(aEntry.Name, _SkipList, bReturnTrueForNullList:=True)
                        If Not bSkip Then
                            AddRow(aEntry, iRow)
                            If String.Compare(aEntry.Name, _CurName, True) = 0 Then
                                _InitRow = _Grid.Rows.Count - 1
                            End If
                        End If
                    Catch ex As Exception
                    Finally
                        If Not bSkip Then iRow += 1
                    End Try
                Next i
                lblSelectedVal1.Text = _CurName
            Case EditMode.Format
                _Colors = New System.Collections.Generic.Dictionary(Of String, Color)
                chkScreen.Visible = True
                chkScreen.Checked = Not _Image.obj_SCREEN.Suppressed
                _Grid.Columns.Item("Transparency").ReadOnly = True
                _Grid.Columns.Item("Handle_FORMAT").ReadOnly = True
                imgCol = _Grid.Columns.Item("Color")
                imgCol.ImageLayout = DataGridViewImageCellLayout.Stretch
                'LoadCombo("Linetype", CType(_Image.Linetypes.Names("ByLayer,ByBlock"), ArrayList))
                LoadCombo("LineWeight", dxpProperties.Props_LineWeightNames.StringValues("ByLayer,ByBlock"))
                lblCurrentVal1.Text = _CurName
                For i = 1 To _InitTable.Count
                    Try
                        aEntry = _InitTable.Item(i)
                        bSkip = Not TLISTS.Contains(aEntry.Name, _SkipList, bReturnTrueForNullList:=True)
                        If Not bSkip Then AddRow(aEntry, iRow)
                    Catch ex As Exception
                    Finally
                        If Not bSkip Then iRow += 1
                    End Try
                Next i
        End Select
        _Loaded = True
        UpdateSelectedRow(idx)
    End Sub
    Private Sub AddRow(aEntry As dxfTableEntry, iRow As Integer)
        If Mode = EditMode.SelectCombo Then
            cboEntries.Items.Add(aEntry.Name)
            Return
        End If
        If _Grid Is Nothing Then Return
        Dim aRow As DataGridViewRow
        Dim aCell As DataGridViewCell
        Dim sVal As String
        Dim aCol1 As DataGridViewComboBoxColumn
        If iRow > _Grid.Rows.Count Then _Grid.Rows.AddCopy(0)
        iRow = _Grid.Rows.Count - 1
        aRow = _Grid.Rows.Item(iRow)
        Try
            aRow.Cells.Item("Status_" & _ModeName).Value = String.Compare(aEntry.Name, _CurName, True) = 0
            aRow.Cells.Item("RefName_" & _ModeName).ReadOnly = True
            aRow.Cells.Item("RefName_" & _ModeName).Value = aEntry.Name
            Select Case Mode
                Case EditMode.SelectTablular
                    If RefType = dxxReferenceTypes.LAYER Then
                        aRow.Cells.Item("Description_SELECT").Value = aEntry.Properties.ValueS("*Description")
                    Else
                        aRow.Cells.Item("Description_SELECT").Value = aEntry.Properties.ValueS("Description")
                    End If
                Case EditMode.Format
                    Select Case _RefType
                        Case dxxReferenceTypes.LAYER
                            Dim aLayer As dxoLayer = aEntry
                            aRow.Cells.Item("IsOn").Value = aLayer.Visible
                            aRow.Cells.Item("Frozen").Value = aLayer.Frozen
                            aRow.Cells.Item("Locked").Value = aLayer.Locked
                            aRow.Cells.Item("Plot").Value = aLayer.PlotFlag
                            'handle
                            sVal = aLayer.Handle
                            aRow.Cells.Item("Handle_FORMAT").Value = sVal
                            'color
                            Dim aClr As Color = aLayer.Color64
                            AddColorToCell(iRow, "Color", aClr)
                            aRow.Cells.Item("LineType").ReadOnly = True
                            aRow.Cells.Item("LineType").Value = aLayer.Linetype
                            aCell = aRow.Cells.Item("Lineweight")
                            sVal = dxfEnums.Description(aLayer.LineWeight)
                            aCol1 = _Grid.Columns.Item(aCell.ColumnIndex)
                            If aCol1.Items.Contains(sVal) Then aCell.Value = aCol1.Items(aCol1.Items.IndexOf(sVal))
                            aRow.Cells.Item("Transparency").ReadOnly = True
                            aRow.Cells.Item("Transparency").Value = aLayer.Transparency
                            aRow.Cells.Item("Description").Value = aLayer.Description
                    End Select
            End Select
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Private ReadOnly Property CurrentLayers As dxfTable
        Get
            Dim lyrs As dxoLayers = DirectCast(_InitTable, dxoLayers)

            Dim _rVal As dxfTable = lyrs.Clone()
            _rVal.ImageGUID = ""
            _rVal.Clear()
            _rVal.ImageGUID = ""
            _rVal.GUID = _InitTable.GUID
            Dim iR As Integer

            Dim _Struc As New TTABLE(_rVal, True, True)

            Dim bStat As Boolean
            Dim lName As String
            Dim lHndl As String = String.Empty
            Dim iGUID As String = String.Empty
            For iR = 0 To _Grid.Rows.Count - 1
                Try
                    bStat = CellValue(iR, $"Status_{_ModeName}", dxxPropertyTypes.dxf_Boolean)
                    lName = CellValue(iR, "RefName_FORMAT", dxxPropertyTypes.dxf_String)
                    lHndl = CellValue(iR, "Handle_FORMAT", dxxPropertyTypes.dxf_String)
                    Dim aLayer As dxoLayer = Nothing
                    '!!! MAINTAIN GUIDS AND HANDLES FOR EXISTING ENTRIES !!!!
                    If _InitTable.TryGetEntry(lHndl, aLayer) Then
                        aLayer = New dxoLayer(New TTABLEENTRY(dxxReferenceTypes.LAYER, lName))
                        iGUID = aLayer.GUID
                    Else
                        iGUID = aLayer.GUID
                        aLayer = aLayer.Clone()
                        aLayer.GUID = iGUID
                    End If
                    aLayer.Frozen = CellValue(iR, "Frozen", dxxPropertyTypes.dxf_Boolean)
                    aLayer.Locked = CellValue(iR, "Locked", dxxPropertyTypes.dxf_Boolean)
                    aLayer.PlotFlag = CellValue(iR, "Plot", dxxPropertyTypes.dxf_Boolean)
                    aLayer.Color64 = CellValue(iR, "Color")
                    aLayer.Linetype = CellValue(iR, "LineType", dxxPropertyTypes.dxf_String)
                    aLayer.LineWeight = CellValue(iR, "LineWeight", dxxPropertyTypes.dxf_String)
                    aLayer.Transparency = CellValue(iR, "Transparency", dxxPropertyTypes.dxf_Integer)
                    aLayer.Description = CellValue(iR, "Description", dxxPropertyTypes.dxf_String)
                    aLayer.Handle = lHndl
                    aLayer.Visible = CellValue(iR, "IsOn", dxxPropertyTypes.dxf_Boolean) 'do this last
                    aLayer.GUID = iGUID
                    _rVal.AddEntry(aLayer)
                Catch ex As Exception
                    Beep()
                Finally

                End Try
            Next iR

            Return _rVal
        End Get
    End Property
    Private Function SaveTable(ByRef rChanged As Boolean, ByRef rToggleScreen As Boolean, ByRef rRedraw As Boolean, ByRef rError As String) As TTABLE
        rChanged = False
        rToggleScreen = False
        rError = String.Empty
        rRedraw = False
        If _curTable Is Nothing Then Return _InitTable.Strukture
        Dim i As Integer
        Dim oldMem As TTABLEENTRY
        Dim newMem As TTABLEENTRY
        Dim dProps As TPROPERTIES
        Dim lVal As Object = ""
        Dim j As Integer
        Dim aProp As TPROPERTY
        Dim bProp As TPROPERTY
        Dim difcnt As Integer
        Dim aFlg As Boolean
        Dim aScr As TSCREEN = _Image.obj_SCREEN
        Dim _rVal As New TTABLE(_Image.Layers)
        Dim bChangeItBack As Boolean
        Dim oldEntry As dxfTableEntry
        Dim newEntry As dxfTableEntry
        Dim cProp As TPROPERTY
        rChanged = _InitTable.Count <> _curTable.Count
        aProp = aScr.Props.Item("Suppressed")
        If aProp.Value <> Not chkScreen.Checked Then
            aProp.SetVal(Not chkScreen.Checked)
            aScr.Props.UpdateProperty = aProp
            _Image.obj_SCREEN = aScr
            rRedraw = True
            _Image.RespondToSettingChange(_Image.Screen, aProp)
            rChanged = True
        End If
        'current layer change
        If _InitHeader.Properties.SetVal("$CLAYER", _CurName) Then
            rChanged = True
            _Image.RespondToSettingChange(_Image.Header, _InitHeader.Properties.Item("$CLAYER"))
        End If
        'check for changed layers
        For i = 1 To _curTable.Count
            newEntry = _curTable.Item(i)
            oldEntry = _InitTable.Entry(newEntry.Handle) ' .GetByHandle(newEntry.Handle).Entry
            If oldEntry IsNot Nothing Then
                'check for property changes
                oldMem = New TTABLEENTRY(oldEntry)
                newMem = New TTABLEENTRY(newEntry)
                'see if the existing layers are being changed
                dProps = oldMem.Props.GetDifferences(newMem.Props, difcnt, "0, 5, 330, 100", "*GUID", False, False, True)
                If difcnt > 0 Then
                    Dim idx As Integer = 0
                    Dim cprops As New TPROPERTIES("")
                    For j = 1 To dProps.Count
                        bProp = dProps.Item(j)
                        bProp = newMem.Props.Item(bProp.Name)
                        aProp = oldMem.Props.Item(bProp.Name)
                        cProp = aProp.Clone
                        cProp.SetVal(bProp.Value)
                        'this save the change to the image
                        _Image.RespondToTableMemberEvent(oldEntry, cProp, bChangeItBack, idx, rError, aFlg, True, cprops)
                        If rError <> "" Or bChangeItBack Then Return _InitTable.Strukture
                        If aFlg Then rRedraw = True
                        rChanged = True
                    Next j
                End If
            End If
        Next i
        Return _curTable.Strukture
    End Function
    Private Sub AddColorToCell(iRow As Integer, sCol As String, aColor As Color)
        Dim dbmp As dxfBitmap = Nothing
        Try
            If Not _Grid.Columns.Contains(sCol) Then Return
            Dim iCol As Integer = _Grid.Columns.Item(sCol).Index
            Dim aCol As DataGridViewColumn = _Grid.Columns.Item(iCol)
            If TypeOf (aCol) IsNot DataGridViewImageColumn Then Return
            Dim aRow As DataGridViewRow = _Grid.Rows.Item(iRow)
            Dim hndl As String = aRow.Cells.Item("Handle_FORMAT").Value.ToString
            If Not _Colors.ContainsKey(hndl) Then
                _Colors.Add(hndl, aColor)
            Else
                _Colors.Item(hndl) = aColor
            End If
            Dim aCell As DataGridViewImageCell = aRow.Cells.Item(iCol)
            Dim ms As New IO.MemoryStream()
            dbmp = New dxfBitmap(50, 50, aColor)
            Dim img As System.Drawing.Image = dbmp.Bitmap
            img.Save(ms, Imaging.ImageFormat.Bmp)
            aCell.Value = ms.ToArray
        Catch ex As Exception
        Finally
            If dbmp IsNot Nothing Then dbmp.Dispose()
        End Try
    End Sub
    Private Sub ApplyChanges()
        Try
            Enabled = False
            _OpCanceled = False
            Dim sError As String = String.Empty
            Dim bChange As Boolean = False
            Select Case Mode
                Case EditMode.SelectCombo
                    _SelectedName = cboEntries.Text
                    _Canceled = _SelectedName = ""
                    _Changed = _SelectedName <> _CurName
                Case EditMode.SelectTablular
                    If _Grid Is Nothing Then Return
                    _Grid.RefreshEdit()
                    Application.DoEvents()
                    _SelectedName = lblSelectedVal1.Text
                    _Canceled = _SelectedName = ""
                    _Changed = _SelectedName <> _CurName
                Case EditMode.Format
                    If _Grid Is Nothing Then Return
                    _Grid.RefreshEdit()
                    Application.DoEvents()
                    If _CurrentCell IsNot Nothing Then
                        _Grid.EndEdit()
                    End If
                    _curTable = CurrentLayers
                    _newTable = dxfTable.FromTTABLE(SaveTable(bChange, _ToggleScreen, _Redraw, sError))
                    If sError <> "" Then
                        Throw New Exception(sError)
                        _Canceled = True
                        _OpCanceled = True
                        Return
                    Else
                        _Canceled = False
                        If bChange Then _Changed = True
                    End If
            End Select
        Catch ex As Exception
            _Canceled = True
            MessageBox.Show($"{ Reflection.MethodBase.GetCurrentMethod.Name} -{ ex.Message}", "Unable To Save Changes", buttons:=MessageBoxButtons.OK, icon:=MessageBoxIcon.Error)
        Finally
            Enabled = True
            If Not _OpCanceled Then Hide()
        End Try
    End Sub
    Private Function CellValue(iRow As Integer, iCol As Integer, Optional aPropType As dxxPropertyTypes = dxxPropertyTypes.Undefined) As Object
        If _Grid Is Nothing Then Return String.Empty
        If iRow < 0 Or iRow > _Grid.Rows.Count - 1 Then Return String.Empty
        If iCol < 0 Or iCol > _Grid.Columns.Count - 1 Then Return String.Empty
        Dim aRow As DataGridViewRow = _Grid.Rows.Item(iRow)
        If _CurrentCell IsNot Nothing Then
            _Grid.BeginEdit(False)
            _Grid.EndEdit()
        End If
        Dim aCell As DataGridViewCell = aRow.Cells.Item(iCol)
        Dim cName As String = _Grid.Columns.Item(iCol).Name
        Application.DoEvents()
        _Grid.BeginEdit(False)
        _Grid.EndEdit()
        Try
            _Grid.UpdateCellValue(iCol, iRow)
        Catch ex As Exception
            If dxfUtils.RunningInIDE Then MessageBox.Show(ex.Message)
        End Try
        Dim _rVal As Object
        If TypeOf aCell Is DataGridViewImageCell Then
            'return the color
            Dim hnd As String = aRow.Cells.Item("Handle_FORMAT").Value.ToString
            _rVal = _Colors.Item(hnd)
        Else
            _rVal = aCell.Value
            If aPropType <> dxxPropertyTypes.Undefined Then
                Select Case aPropType
                    Case dxxPropertyTypes.dxf_Boolean
                        _rVal = TVALUES.ToBoolean(_rVal)
                    Case dxxPropertyTypes.dxf_Integer
                        _rVal = TVALUES.To_INT(_rVal)
                    Case dxxPropertyTypes.dxf_Double
                        _rVal = TVALUES.To_DBL(_rVal)
                    Case dxxPropertyTypes.Switch
                        If TPROPERTY.SwitchValue(_rVal) Then _rVal = 1 Else _rVal = 0
                    Case dxxPropertyTypes.dxf_Long, dxxPropertyTypes.Color
                        _rVal = TVALUES.To_LNG(_rVal)
                    Case dxxPropertyTypes.dxf_Single
                        _rVal = TVALUES.To_SNG(_rVal)
                    Case dxxPropertyTypes.Angle
                        _rVal = TVALUES.To_DBL(_rVal)
                        _rVal = TVALUES.NormAng(_rVal, False, bReturnPosive:=True)
                    Case dxxPropertyTypes.dxf_String, dxxPropertyTypes.ClassMarker, dxxPropertyTypes.Pointer
                        _rVal = TVALUES.To_STR(_rVal)
                        Dim bLwt As Boolean = _Grid.Columns.Item(iCol).Name.ToUpper.Contains("LINEWEIGHT")
                        If bLwt Then
                            Dim idx As Integer = 1
                            Dim aProp As TPROPERTY = dxpProperties.Props_LineWeightNames.GetByStringValue(_rVal, 1, idx)
                            _rVal = dxpProperties.Props_LineWeightValues.ValueI(aProp.Name)
                        End If
                        'Case Is >= dxxPropertyTypes.Vector2D
                End Select
            End If
        End If
        Return _rVal
    End Function
    Private Function CellValue(iRow As Integer, sCol As String, Optional aPropType As dxxPropertyTypes = dxxPropertyTypes.Undefined) As Object
        If _Grid Is Nothing Then Return String.Empty
        If iRow < 0 Or iRow > _Grid.Rows.Count - 1 Then Return String.Empty
        If Not _Grid.Columns.Contains(sCol) Then Return String.Empty
        Dim iCol As Integer = _Grid.Columns.Item(sCol).Index
        Return CellValue(iRow, iCol, aPropType)

    End Function
    Private Sub TerminateObjects()
        _Image = Nothing
    End Sub
    Private Sub DeleteOne()
        If _Image Is Nothing Or _SelectedName = "" Then Return

        If MessageBox.Show($"Delete '{ _RefTypeName }.{_SelectedName }' ?", "Delete?", MessageBoxButtons.YesNo, icon:=MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Dim aMember As dxfTableEntry = _InitTable.Entry(_SelectedName)
        Dim sErr As String = String.Empty
        Dim bCanc As Boolean = False
        If aMember IsNot Nothing Then
            _Image.RespondToTableEvent(_InitTable, dxxCollectionEventTypes.PreRemove, aMember, bCanc, False, sErr, True, True)
            If bCanc Then
                MessageBox.Show($"Unabe To Delete '{ _RefTypeName }.{_SelectedName }'  Message:{sErr}", "Message", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)


                Return
            End If
            If Not bCanc Then
                _Image.RespondToTableEvent(_InitTable, dxxCollectionEventTypes.Remove, aMember, bCanc, False, sErr, True, True)
                If bCanc Then
                    MessageBox.Show($"Unabe To Delete '{ _RefTypeName }.{_SelectedName }'  Message:{sErr}", "Message", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)

                    Return
                End If
            End If
        Else
            Dim aRef As dxfTableEntry = _Added.Entry(_SelectedHandle)
            'aMember = _Added.GetByHandle(_SelectedHandle)
            If aRef IsNot Nothing Then _Added.Delete(aRef.Name)
            _Image.HandleGenerator.ReleaseHandle(_SelectedHandle)
        End If
        'delete it
        If aMember IsNot Nothing Then _Deleted.AddEntry(aMember)

        _Grid.Rows.RemoveAt(_SelectedRow)
        _Changed = True
        If _SelectedRow <= 0 Then _SelectedRow = 1
        _Grid.Rows.Item(_SelectedRow - 1).Cells.Item($"RefName_{ _ModeName}").Selected = True
    End Sub
    Private Sub AddOne()
        If _Image Is Nothing Or _SelectedName = "" Then Return
        Dim lname As String = $"New{ _RefTypeName}"
        Dim curTable As dxfTable = CurrentLayers
        Dim i As Integer = 1
        Dim suff As String = String.Empty
        Dim bBail As Boolean = False
        Dim sErr As String = String.Empty
        Do Until Not curTable.MemberExists(lname & suff)
            suff = $"_{ i}"
            i += 1
        Loop
        lname += suff
        lname = InputBox($"Enter Name For New { _RefTypeName}Add { _RefTypeName}", lname)
        If lname = "" Then Return
        Dim newEntry As dxfTableEntry = dxfTableEntry.CreateEntry(_RefType, lname)
        newEntry.Index = _Grid.Rows.Count + 1
        _Image.RespondToTableEvent(_InitTable, dxxCollectionEventTypes.PreAdd, newEntry, bBail, False, sErr, True, True)
        If bBail Then
            MessageBox.Show($"Unabe To Add '{ _RefTypeName }.{_SelectedName }'  Message:{sErr}", "Message", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)


            Return
        End If
        _Image.RespondToTableEvent(_InitTable, dxxCollectionEventTypes.Add, newEntry, bBail, False, sErr, True, True)
        If bBail Then
            MessageBox.Show($"Unabe To Add '{ _RefTypeName }.{_SelectedName }'  Message:{sErr}", "Message", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
            Return
        End If
        _Added.AddEntry(newEntry)
        AddRow(newEntry, _Grid.Rows.Count + 1)
        _Changed = True
        _Grid.Rows.Item(_Grid.Rows.Count - 1).Cells.Item("RefName_" & _ModeName).Selected = True
    End Sub
    Private Sub UpdateSelectedRow(Optional aRowIndex As Integer = -1)
        _SelectedName = ""
        lblSelectedVal0.Text = ""
        _SelectedRow = -1
        _SelectedHandle = ""
        lblSelectedVal1.Text = ""
        If _Grid Is Nothing Then Return
        Try
            Dim aRow As DataGridViewRow = Nothing
            Dim aCell As DataGridViewCell = Nothing
            If aRowIndex >= 0 Then
                If aRowIndex < 0 Or aRowIndex > _Grid.Rows.Count Then Return
                aRow = _Grid.Rows.Item(aRowIndex)
                'find the name column
                aCell = aRow.Cells.Item("RefName_" & _ModeName)
            Else
                If _Grid.SelectedRows.Count > 0 Then
                    aRow = _Grid.SelectedRows.Item(0)
                    aCell = aRow.Cells.Item("RefName_" & _ModeName)
                Else
                    If _Grid.SelectedCells.Count > 0 Then
                        aCell = _Grid.SelectedCells.Item(0)
                        If aCell IsNot Nothing Then
                            aRow = _Grid.Rows.Item(aCell.RowIndex)
                            aCell = aRow.Cells.Item("RefName_" & _ModeName)
                        End If
                    End If
                End If
                If aCell Is Nothing Then aCell = _Grid.Rows.Item(0).Cells.Item("RefName_" & _ModeName)
            End If
            If aCell Is Nothing Then Return
            aRow = _Grid.Rows.Item(aCell.RowIndex)
            _SelectedRow = aCell.RowIndex
            _SelectedName = TVALUES.To_STR(aCell.Value, True)
            If Mode = EditMode.Format Then
                _SelectedHandle = TVALUES.To_STR(aRow.Cells.Item("Handle_FORMAT").Value, True)
                lblSelectedVal0.Text = _SelectedName
            Else
                lblSelectedVal1.Text = _SelectedName
            End If
        Catch ex As Exception
            _SelectedName = ""
            lblSelectedVal0.Text = ""
            _SelectedRow = -1
            _SelectedHandle = ""
            lblSelectedVal1.Text = ""
        End Try
    End Sub
    Private Sub RespondToDoubleClick()
        If _Grid Is Nothing Then Return
        If Mode = EditMode.SelectTablular Then
            If lblSelectedVal1.Text <> "" Then ApplyChanges()
        ElseIf Mode = EditMode.Format Then
            If _Grid Is Nothing Then Return
            Dim iRow As Integer = _SelectedRow
            Dim iCol As Integer = _SelectedColumn
            If iRow < 0 Or Not _OpCanceled Or _Working Or iRow > _Grid.RowCount - 1 Then Return
            If iCol < 0 Or iCol > _Grid.ColumnCount - 1 Then Return
            Dim aCol As DataGridViewColumn = _Grid.Columns.Item(iCol)
            Dim htxt As String = aCol.HeaderText.ToUpper
            If TypeOf (aCol) Is DataGridViewImageColumn Then htxt = "COLOR"
            Select Case htxt
                Case "STATUS"
                    'If the user checked this box, then uncheck all the other rows
                    Dim isChecked As Boolean = TVALUES.ToBoolean(_Grid.Rows(iRow).Cells($"Status_{ _ModeName}").Value)
                    If (isChecked) Then Return
                    _Working = True
                    For Each row As DataGridViewRow In _Grid.Rows
                        If (row.Index <> iRow) Then
                            row.Cells.Item("Status_" & _ModeName).Value = isChecked
                        Else
                            row.Cells.Item("Status_" & _ModeName).Value = Not isChecked
                            _CurName = _Grid.Rows.Item(iRow).Cells.Item($"RefName_{ _ModeName}").Value
                            lblCurrentVal1.Text = _CurName
                            lblSelectedVal1.Text = _CurName
                        End If
                    Next row
                Case "COLOR"
                    Dim opCanc As Boolean = False
                    Dim aClr As Color = CellValue(iRow, iCol)
                    Dim dClr As dxfColor = dxfColors.SelectColor(Me, opCanc, True, aWin64Color:=aClr)
                    If Not opCanc Then
                        AddColorToCell(iRow, aCol.Name, dClr.ToWin64)
                    End If
                Case "LINETYPE"
                    Dim curLt As String = CellValue(iRow, iCol, dxxPropertyTypes.dxf_String)
                    Dim aLt As String = dxfLinetypes.SelectLineType(Me, curLt, False, False)
                    If aLt = "" Or String.Compare(aLt, curLt, True) = 0 Then Return
                    _Grid.Rows.Item(iRow).Cells.Item(iCol).Value = aLt
            End Select
            _Working = False
        End If
    End Sub
#End Region 'Methods
#Region "Event Handlers"
    Private Sub _AcceptButton_Click(sender As Object, e As EventArgs) Handles _AcceptButton.Click
        ApplyChanges()
    End Sub
    Private Sub _CancelButton_Click(sender As Object, e As EventArgs) Handles _CancelButton.Click
        _Canceled = True
        Hide()
    End Sub
    Private Sub cmdDelete_Click(sender As Object, e As EventArgs) Handles cmdDelete.Click
        DeleteOne()
    End Sub
    Private Sub _Grid_Enter(sender As Object, e As EventArgs) 'Handles _Grid.Enter
        If _Loaded Then
            UpdateSelectedRow()
        End If
    End Sub
    Private Sub _Grid_RowEnter(sender As Object, e As DataGridViewCellEventArgs) ' Handles _Grid.RowEnter
        If _Loaded Then
            UpdateSelectedRow(e.RowIndex)
        End If
    End Sub
    Private Sub cmdAdd_Click(sender As Object, e As EventArgs) Handles cmdAdd.Click
        AddOne()
    End Sub
    Private Sub frmEdit_LAYERS_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        If _InitRow < 0 Then Return
        Try
            If Mode = EditMode.SelectTablular Then
                If _Grid Is Nothing Then Return
                If _InitRow < 0 Or _InitRow > _Grid.Rows.Count - 1 Then Return
                Dim aRow As DataGridViewRow = _Grid.Rows.Item(_InitRow)
                aRow.Cells.Item("RefName_" & _ModeName).Selected = True
            ElseIf Mode = EditMode.SelectCombo Then
                cboEntries.SelectedIndex = _InitRow
            End If
        Catch ex As Exception
        Finally
            _InitRow = -1
        End Try
    End Sub
    Private Sub _Grid_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles _Grid.CellClick
        If e.RowIndex < 0 Or e.ColumnIndex < 0 Then Return
        _SelectedColumn = e.ColumnIndex
        If _SelectedColumn >= 0 Then _CurrentCell = _Grid.Rows.Item(e.RowIndex).Cells.Item(_SelectedColumn)
        If _Loaded Then
            UpdateSelectedRow(e.RowIndex)
        End If
    End Sub
    Private Sub _Grid_DoubleClick(sender As Object, e As EventArgs) Handles _Grid.DoubleClick
        RespondToDoubleClick()
    End Sub
    Private Sub _Grid_CellEnter(sender As Object, e As DataGridViewCellEventArgs) Handles _Grid.CellEnter
        If e.RowIndex < 0 Or e.ColumnIndex < 0 Then Return
        _CurrentCell = _Grid.Rows.Item(e.RowIndex).Cells.Item(e.ColumnIndex)
    End Sub
    Private Sub _Grid_CellLeave(sender As Object, e As DataGridViewCellEventArgs) Handles _Grid.CellLeave
        If e.RowIndex < 0 Or e.ColumnIndex < 0 Then Return
        If _CurrentCell IsNot Nothing Then
            _Grid.EndEdit()
        End If
    End Sub
    Private Sub frmTables_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        DisposeAll()
    End Sub
    Private Sub cmdCancel_ChangeUICues(sender As Object, e As UICuesEventArgs) Handles cmdCancel.ChangeUICues
    End Sub
#End Region 'Event Handlers
End Class
