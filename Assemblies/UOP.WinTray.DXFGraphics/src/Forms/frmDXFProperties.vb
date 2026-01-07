

Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Public Class frmDXFProperties
    Private _Image As dxfImage
    Private WithEvents _Gen As dxoFileTool
    Private WithEvents _Writer As dxoFileTool
    Private _Props As System.Collections.Generic.Dictionary(Of String, dxoProperty)
    Private _HandleNodes As System.Collections.Generic.Dictionary(Of String, dxoProperty)
    Private _Buffer As TBUFFER
    Private _CurrentNode As TreeNode
    Private _SuppressInstances As Boolean
    Private _OutputFile As IO.StreamWriter
    Private _Errors As New List(Of Exception)
    Private _DXFFile As String
    Private _ErrCount As Long
    Private _Loading As Boolean
    Private _Canceled As Boolean
    Private _CheckBoxes As System.Collections.Generic.Dictionary(Of String, CheckBox)
    Private _Colors As System.Collections.Generic.Dictionary(Of String, Color)
    Private _nodeColor As Color
    Private _SwitchesToBooleans As Boolean
    Private _HandlesToLongs As Boolean
    Private _WorkingFolder As String
    Private _EOF As Boolean
    Private Const tickLimit As Integer = 20
    Private ticks As Integer = 1
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        SettingsLoad()
    End Sub
    Private Property Loading As Boolean
        Get
            Return _Loading
        End Get
        Set(value As Boolean)
            If _Loading = value Then Return
            _Loading = value
            fraTools.Enabled = Not _Loading
            gbShow.Enabled = Not _Loading
            gbSettings.Enabled = Not _Loading
            gbFileView.Enabled = Not _Loading
            fraTrees.Enabled = Not _Loading
            cmdReload.Enabled = Not _Loading
            If Not _Loading Then
                cmdClose.Text = "Close"
                cmdClose.DialogResult = DialogResult.Cancel
                Cursor = Cursors.Default
                If _Gen IsNot Nothing Then
                    _Gen.CancelBuffer = True
                End If
            Else
                _Canceled = False
                cmdClose.DialogResult = Nothing
                Status = ""
                cmdClose.Text = "Cancel"
                Cursor = Cursors.WaitCursor
            End If
        End Set
    End Property
    Private WriteOnly Property Status As String
        Set(value As String)
            sbStatus.Items(0).Text = value
            sbStatus.Refresh()
        End Set
    End Property
    Private ReadOnly Property OutputType As String
        Get
            Return "DXF"
        End Get
    End Property
    Private Function CurrentPath(Optional aFinalString As String = "") As String
        If _Gen Is Nothing Then Return String.Empty
        Return _Gen.CurrentPath(aFinalString)

    End Function
    Private ReadOnly Property SuppressionList As String
        Get
            Dim rStr As String = String.Empty
            If Not CheckVal("ShowHeader") Then TLISTS.Add(rStr, "HEADER")
            If Not CheckVal("ShowClasses") Then TLISTS.Add(rStr, "CLASSES")
            If Not CheckVal("ShowTables") Then TLISTS.Add(rStr, "TABLES")
            If Not CheckVal("ShowBlocks") Then TLISTS.Add(rStr, "BLOCKS")
            If Not CheckVal("ShowEntities") Then TLISTS.Add(rStr, "ENTITIES")
            If Not CheckVal("ShowObjects") Then TLISTS.Add(rStr, "OBJECTS")
            If Not CheckVal("ShowTableAPPIDS") Then TLISTS.Add(rStr, "APPID")
            If Not CheckVal("ShowTableVPORTS") Then TLISTS.Add(rStr, "VPORT")
            If Not CheckVal("ShowTableVIEWS") Then TLISTS.Add(rStr, "VIEW")
            If Not CheckVal("ShowTableLAYERS") Then TLISTS.Add(rStr, "LAYER")
            If Not CheckVal("ShowTableUCSS") Then TLISTS.Add(rStr, "UCS")
            If Not CheckVal("ShowTableSTYLES") Then TLISTS.Add(rStr, "STYLE")
            If Not CheckVal("ShowTableDIMSTYLES") Then TLISTS.Add(rStr, "DIMSTYLE")
            If Not CheckVal("ShowTableBLOCKRECORDS") Then TLISTS.Add(rStr, "BLOCK_RECORD")
            If Not CheckVal("ShowTableLTYPES") Then TLISTS.Add(rStr, "LTYPE")
            Return rStr
        End Get
    End Property
    Private ReadOnly Property SettingsString As String
        Get
            Dim rStr As String = String.Empty
            If CheckVal("ShowDimSettings") Then TLISTS.Add(rStr, "DIMSETTINGS")
            If CheckVal("ShowScreenSettings") Then TLISTS.Add(rStr, "SCREEN")
            If CheckVal("ShowSymbolSettings") Then TLISTS.Add(rStr, "SYMBOLSETTINGS")
            If CheckVal("ShowLTLSettings") Then TLISTS.Add(rStr, "LINETYPESETTINGS")
            If CheckVal("ShowTableSettings") Then TLISTS.Add(rStr, "TABLESETTINGS")
            If CheckVal("ShowTextSettings") Then TLISTS.Add(rStr, "TEXTSETTINGS")
            If CheckVal("ShowDimOverrideSettings") Then TLISTS.Add(rStr, "DIMOVERRIDES")
            If CheckVal("ShowDisplaySettings") Then TLISTS.Add(rStr, "DISPLAY")
            Return rStr
        End Get
    End Property
    Private Function TryGetNode(aTreeView As TreeView, aPath As String, ByRef rNode As TreeNode, Optional bCreateIt As Boolean = True, Optional aTextVal As String = "", Optional aPathDelimitor As String = "") As Boolean
        rNode = Nothing
        If String.IsNullOrEmpty(aPath) Or aTreeView Is Nothing Then Return False
        Dim sNodes() As TreeNode = aTreeView.Nodes.Find(aPath, searchAllChildren:=True)
        If sNodes.Length > 0 Then
            rNode = sNodes(0)
        Else
            rNode = Nothing
            If Not bCreateIt Then Return False
        End If
        If rNode Is Nothing Then
            Dim pdelim As String = Trim(aPathDelimitor)
            If pdelim = "" Then pdelim = _Gen.PathDelimitor
            Dim sVals As New TLIST(pdelim, aPath)
            If sVals.Count <= 0 Then Return False
            Dim sVal As String
            Dim aNode As TreeNode = Nothing
            Dim pth As String = String.Empty
            Dim lNode As TreeNode = Nothing
            If aTreeView.Nodes.Count > 0 Then lNode = aTreeView.Nodes.Item(0)
            For i As Integer = 1 To sVals.Count
                sVal = sVals.Item(i)
                If i = 1 Then pth = sVal Else pth += (pdelim & sVal)
                If i < sVals.Count Then
                    sNodes = aTreeView.Nodes.Find(pth, searchAllChildren:=True)
                    If sNodes.Length <= 0 Then
                        lNode = aTreeView.Nodes.Add(pth, sVal)
                    Else
                        lNode = sNodes(0)
                    End If
                Else
                    If lNode IsNot Nothing Then
                        rNode = lNode.Nodes.Add(pth, sVal)
                    Else
                        rNode = aTreeView.Nodes.Add(pth, sVal)
                    End If
                End If
            Next
        End If
        If rNode IsNot Nothing Then
            rNode.EnsureVisible()
            aTreeView.Refresh()
            If Not String.IsNullOrEmpty(aTextVal) Then rNode.Text = aTextVal
            Return True
        Else
            Return False
        End If
    End Function
    Private Sub LoadList(Optional bFirstTime As Boolean = False)
        If Loading Or _Image Is Nothing Then Return
        'txtGoto.Text = ""
        rtbFileView.Clear()
        rtbSgnatures.Clear()
        _nodeColor = Color.Black
        _ErrCount = 0
        _Gen = New dxoFileTool
        txtNode.Text = ""
        txtHandleKey.Text = ""
        txtErrorKey.Text = ""
        trvProps.Nodes.Clear()
        _EOF = False
        Dim nNode As TreeNode
        _Errors = New List(Of Exception)
        Try
            trvHandles.Nodes.Clear()
            trvHandles.Nodes.Add("Handles", "Handles")
            trvErrors.Nodes.Clear()
            nNode = trvErrors.Nodes.Add("Errors", "Errors")
            trvErrors.Nodes.Item("Errors").Nodes.Add("Errors.Handles", "Handles")
            trvErrors.Nodes.Item("Errors").Nodes.Add("Errors.Pointers", "Pointers")
            nNode.Expand()
            _Props = New System.Collections.Generic.Dictionary(Of String, dxoProperty)
            _HandleNodes = New System.Collections.Generic.Dictionary(Of String, dxoProperty)
            If bFirstTime Then
                nNode = trvProps.Nodes.Add(_Image.GUID, "Press Load Buffer")
                nNode.Expand()
            Else
                Loading = True
                SettingsSave()
                nNode = trvProps.Nodes.Add(_Image.GUID, "Loading Image Buffer...")
                nNode.Expand()
                trvProps.Refresh()
                'If CheckVal("WriteToFile") Then
                '    _DXFFile = _Gen.zStartStream(dxxFileTypes.DXF, "", _OutputFile)
                '    If _DXFFile <> "" Then
                '        If IO.File.Exists(_DXFFile) Then IO.File.Delete(_DXFFile, True)
                '    End If
                '    _DXFFile = ""
                '    _OutputFile = Nothing
                'End If
                'let  the generator create the buffer
                _SuppressInstances = Not CheckVal("ShowInstances")
                _Gen.CancelBuffer = False
                If CheckVal("ShowBlocks") Then
                    SetCheckVal(True, "ShowTables")
                    SetCheckVal(True, "ShowTableBLOCKRECORDS")
                End If
                Dim supList As String = SuppressionList
                Dim setList As String = SettingsString
                Select Case OutputType
                    Case "DXF"
                        _Buffer = _Gen.Buffer_CREATE(_Image, bIncludeSup:=CheckVal("ShowSuppressedProperties"), bIncludeHidn:=CheckVal("ShowHiddenProperties"),
                                                 bShowHiddenObjects:=CheckVal("ShowHiddenObjects"), bSuppressDimOverrides:=CheckVal("SuppressDimOverrides"),
                                             aVersion:=dxxACADVersions.DefaultVersion, aSuppressList:=supList, aThrowHandleErrors:=True,
                                             aSettingsString:=setList, bForFileOutput:=False)
                        trvProps.Nodes.Item(0).Text = _Image.FileName(dxxFileTypes.DXF)
                        'Case "OBJ"
                        '    trvProps.Nodes.Item(1).Text = _Image.GUID
                        '    _Buffer = _Gen.Buffer_CREATEOBJECT(_Image, bIncludeSup:=CheckVal("ShowSuppressedProperties"), bSuppressDimOverrides:=CheckVal("ShowDimOverrides"),
                        '                                   aVersion:=dxxACADVersions.DefaultVersion, aSuppressList:=supList, aThrowHandleErrors:=True,
                        '                                   aSettingsString:=setList, rErrorList:=_Errors)
                End Select
                Try
                    'trvHandles.Nodes.Item(1).Sorted = True
                    txtNode.Text = ""
                    txtHandleKey.Text = ""
                    txtErrorKey.Text = ""
                    If _DXFFile <> "" Then
                        If Not IO.File.Exists(_DXFFile) Then _DXFFile = ""
                    End If
                Catch ex As Exception
                    SaveException(ex)
                Finally
                    trvProps.Nodes.Item(0).Expand()
                End Try
                'cmdReadText.Enabled = _DXFFile <> ""
            End If
        Catch ex As Exception
            SaveException(ex)
        Finally
            trvProps.Nodes.Item(0).Expand()
            Loading = False
            If _Canceled Then SaveException(New Exception("Buffer Canceled Prematurely"))
            ShowExceptions()
        End Try
    End Sub
    Private Sub SaveException(aException As Exception)
        If _Errors Is Nothing Or aException Is Nothing Then Return
        Dim msg As String = "frmDXFProperties"
        If Not String.IsNullOrEmpty(msg) Then msg += "."
        msg += aException.Message
        Dim ex As New Exception(msg) With {.Source = "frmDXFProperties"}
        _Errors.Add(ex)
    End Sub
    Private Sub ShowExceptions()
        If _Errors Is Nothing Then Return
        If _Errors.Count <= 0 Then Return
        Dim aNode As TreeNode
        Dim basePath As String = "Exceptions"
        Dim ex As Exception
        If Not trvErrors.Nodes.ContainsKey(basePath) Then
            aNode = trvErrors.Nodes.Add(basePath, basePath)
        Else
            aNode = trvErrors.Nodes.Item(basePath)
        End If
        For i As Integer = 1 To _Errors.Count
            ex = _Errors.Item(i - 1)
            aNode.Nodes.Add(basePath & "[" & i & "]", ex.Source & "[" & ex.Message & "]")
        Next
    End Sub
    Private Function CheckVal(aTag As String, Optional aColor As Color = Nothing) As Boolean

        If _CheckBoxes Is Nothing Then Return False
        Dim ckbox As CheckBox = Nothing
        If _CheckBoxes.TryGetValue(aTag, ckbox) Then
            Return ckbox.Checked
        Else
            Return False
        End If


    End Function

    Private Sub SetCheckVal(value As Boolean, aTag As String, Optional aColor As Color? = Nothing)
        If _CheckBoxes Is Nothing Then Return
        Dim ckbox As CheckBox = Nothing
        If _CheckBoxes.TryGetValue(aTag, ckbox) Then
            If aColor.HasValue Then
                If aColor.Value.Name <> "0" Then
                    ckbox.ForeColor = aColor.Value
                End If
            End If
            ckbox.Checked = value
        Else
            Beep()
        End If
    End Sub
    Private Function SetPostScript(aProperty As dxoProperty) As String
        Dim txt As String
        Dim rStr As String = String.Empty
        If aProperty.IsHeaderProperty Then
            If aProperty.Name = "$ACADVER" Then
                rStr = goACAD.Versions.GetVersionByName(aProperty.Value, bHeaderName:=True).Name
                Return rStr
            End If
        End If
        Select Case aProperty.PropertyType
            Case dxxPropertyTypes.Color
                Return String.Empty
                txt = dxfColors.ACLName(TVALUES.To_INT(aProperty.Value))
                If txt <> "" Then Return txt
            Case dxxPropertyTypes.Handle, dxxPropertyTypes.Pointer
                rStr = aProperty.ValueString
                If rStr <> "" Then
                    'convert hext to long
                    Return String.Empty 'TVALUES.HexToInteger(rStr).ToString()
                End If
            Case dxxPropertyTypes.Switch
                Return String.Empty
        End Select
        If rStr = "" Then
            If _Gen.SectionName = "SETTINGS" Then
                If _Gen.ObjectTypeName = "LINETYPESETTINGS" Then
                    If aProperty.GroupCode = 1 Then
                        Select Case aProperty.Value
                            Case dxxLinetypeLayerFlag.ForceToLayer
                                rStr = "ForceToLayer"
                            Case dxxLinetypeLayerFlag.ForceToColor
                                rStr = "ForceToColor"
                            Case dxxLinetypeLayerFlag.Suppressed
                                rStr = "Off"
                        End Select
                    ElseIf aProperty.GroupCode > 1 Then
                        txt = dxfColors.ACLName(aProperty.Color)
                        If txt.IndexOf(",") >= 0 Then txt = ""
                        rStr = $"COLOR={ aProperty.Color}"
                        If txt <> "" Then
                            rStr += $" [{txt}]"
                        End If
                        rStr = "LAYER=" & aProperty.Value & ";" & rStr
                        aProperty.Color = -1
                        aProperty.Value = rStr
                        rStr = ""
                    End If
                End If
            End If
        End If
        Return rStr
    End Function
    Private Sub SettingsLoad()
        Dim bWuz As Boolean = Loading
        Try
            _Colors = New System.Collections.Generic.Dictionary(Of String, Color)
            txtFolder1.Text = WorkingFolder
            Loading = True
            Width = GenSettings.Right + 25
            Height = GenSettings.Bottom + sbStatus.Height + 50
            _CheckBoxes = New System.Collections.Generic.Dictionary(Of String, CheckBox)
            Dim cntrl As Control
            Dim chkBx As CheckBox
            For Each cntrl In gbSettings.Controls
                If TypeOf (cntrl) Is CheckBox Then
                    chkBx = cntrl
                    If chkBx.Tag <> "" Then
                        If Not _CheckBoxes.ContainsKey(chkBx.Tag) Then
                            _CheckBoxes.Add(chkBx.Tag, chkBx)
                        End If
                    End If
                End If
            Next
            For Each cntrl In gbNatives.Controls
                If TypeOf (cntrl) Is CheckBox Then
                    chkBx = cntrl
                    If chkBx.Tag <> "" Then
                        If Not _CheckBoxes.ContainsKey(chkBx.Tag) Then
                            _CheckBoxes.Add(chkBx.Tag, chkBx)
                        End If
                    End If
                End If
            Next
            For Each cntrl In gbTables.Controls
                If TypeOf (cntrl) Is CheckBox Then
                    chkBx = cntrl
                    If chkBx.Tag <> "" Then
                        If Not _CheckBoxes.ContainsKey(chkBx.Tag) Then
                            _CheckBoxes.Add(chkBx.Tag, chkBx)
                        End If
                    End If
                End If
            Next
            For Each cntrl In gbViewControl.Controls
                If TypeOf (cntrl) Is CheckBox Then
                    chkBx = cntrl
                    If chkBx.Tag <> "" Then
                        If Not _CheckBoxes.ContainsKey(chkBx.Tag) Then
                            _CheckBoxes.Add(chkBx.Tag, chkBx)
                        End If
                    End If
                End If
            Next
            _Colors.Add(dxfLinetypes.Hidden, Color.Blue)
            _Colors.Add("Suppressed", Color.Gray)
            _Colors.Add(dxfLinetypes.Invisible, Color.Coral)
            SetCheckVal(My.Settings.PropertyViewer_SuppressDimOverrides, "SuppressDimOverrides")
            SetCheckVal(My.Settings.PropertyViewer_ShowSuppressedProperties, "ShowSuppressedProperties", _Colors.Item("Suppressed"))
            SetCheckVal(My.Settings.PropertyViewer_ShowHeader, "ShowHeader")
            SetCheckVal(My.Settings.PropertyViewer_ShowClasses, "ShowClasses")
            SetCheckVal(My.Settings.PropertyViewer_ShowObjects, "ShowObjects")
            SetCheckVal(My.Settings.PropertyViewer_ShowInstances, "ShowInstances")
            SetCheckVal(My.Settings.PropertyViewer_ShowHiddenObjects, "ShowHiddenObjects", _Colors.Item(dxfLinetypes.Invisible))
            SetCheckVal(My.Settings.PropertyViewer_ShowHiddenProperties, "ShowHiddenProperties", _Colors.Item(dxfLinetypes.Hidden))
            SetCheckVal(My.Settings.PropertyViewer_ShowTables, "ShowTables")
            SetCheckVal(My.Settings.PropertyViewer_ShowEntities, "ShowEntities")
            SetCheckVal(My.Settings.PropertyViewer_ShowBlocks, "ShowBlocks")
            SetCheckVal(My.Settings.PropertyViewer_ShowDimSettings, "ShowDimSettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowScreenSettings, "ShowScreenSettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowSymbolSettings, "ShowSymbolSettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowLTLSettings, "ShowLTLSettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableSettings, "ShowTableSettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowTextSettings, "ShowTextSettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowDimOverrideSettings, "ShowDimOverrideSettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowDisplaySettings, "ShowDisplaySettings")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableAPPIDS, "ShowTableAPPIDS")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableBLOCKRECORDS, "ShowTableBLOCKRECORDS")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableDIMSTYLES, "ShowTableDIMSTYLES")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableLAYERS, "ShowTableLAYERS")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableLTYPES, "ShowTableLTYPES")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableSTYLES, "ShowTableSTYLES")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableUCSS, "ShowTableUCSS")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableVIEWS, "ShowTableVIEWS")
            SetCheckVal(My.Settings.PropertyViewer_ShowTableVPORTS, "ShowTableVPORTS")
        Catch ex As Exception
            Beep()
        Finally
            Loading = bWuz
        End Try
    End Sub
    Private Sub SettingsSave()
        Try
            My.Settings.PropertyViewer_SuppressDimOverrides = CheckVal("SuppressDimOverrides")
            My.Settings.PropertyViewer_ShowSuppressedProperties = CheckVal("ShowSuppressedProperties")
            My.Settings.PropertyViewer_ShowHeader = CheckVal("ShowHeader")
            My.Settings.PropertyViewer_ShowClasses = CheckVal("ShowClasses")
            My.Settings.PropertyViewer_ShowObjects = CheckVal("ShowObjects")
            My.Settings.PropertyViewer_ShowInstances = CheckVal("ShowInstances")
            My.Settings.PropertyViewer_ShowHiddenProperties = CheckVal("ShowHiddenProperties")
            My.Settings.PropertyViewer_ShowHiddenObjects = CheckVal("ShowHiddenObjects")
            My.Settings.PropertyViewer_ShowTables = CheckVal("ShowTables")
            My.Settings.PropertyViewer_ShowEntities = CheckVal("ShowEntities")
            My.Settings.PropertyViewer_ShowBlocks = CheckVal("ShowBlocks")
            My.Settings.PropertyViewer_ShowDimSettings = CheckVal("ShowDimSettings")
            My.Settings.PropertyViewer_ShowScreenSettings = CheckVal("ShowScreenSettings")
            My.Settings.PropertyViewer_ShowSymbolSettings = CheckVal("ShowSymbolSettings")
            My.Settings.PropertyViewer_ShowLTLSettings = CheckVal("ShowLTLSettings")
            My.Settings.PropertyViewer_ShowTableSettings = CheckVal("ShowTableSettings")
            My.Settings.PropertyViewer_ShowTextSettings = CheckVal("ShowTextSettings")
            My.Settings.PropertyViewer_ShowDimOverrideSettings = CheckVal("ShowDimOverrideSettings")
            My.Settings.PropertyViewer_ShowDisplaySettings = CheckVal("ShowDisplaySettings")
            My.Settings.PropertyViewer_ShowTableAPPIDS = CheckVal("ShowTableAPPIDS")
            My.Settings.PropertyViewer_ShowTableBLOCKRECORDS = CheckVal("ShowTableBLOCKRECORDS")
            My.Settings.PropertyViewer_ShowTableDIMSTYLES = CheckVal("ShowTableDIMSTYLES")
            My.Settings.PropertyViewer_ShowTableLAYERS = CheckVal("ShowTableLAYERS")
            My.Settings.PropertyViewer_ShowTableLTYPES = CheckVal("ShowTableLTYPES")
            My.Settings.PropertyViewer_ShowTableSTYLES = CheckVal("ShowTableSTYLES")
            My.Settings.PropertyViewer_ShowTableUCSS = CheckVal("ShowTableUCSS")
            My.Settings.PropertyViewer_ShowTableVIEWS = CheckVal("ShowTableVIEWS")
            My.Settings.PropertyViewer_ShowTableVPORTS = CheckVal("ShowTableVPORTS")
            _SwitchesToBooleans = CheckVal("SwitchesToBooleans")
            _HandlesToLongs = CheckVal("HandlesToLongs")
        Catch ex As Exception
        End Try
    End Sub
    Private Sub PropertySaved(aProperty As dxoProperty, aInstance As Integer, aPostScript As String, aParentPath As String, bSuppressed As Boolean, bHidden As Boolean, bInvisible As Boolean) Handles _Gen.PropertySaved
        'when the dxf generator raises an event that a property has been saved to the buffer it passes through here
        'we display it various ways from here
        If aInstance > 1 And _SuppressInstances Then Return
        Dim sig As String = String.Empty
        Dim propPath As String = aProperty.Path
        Dim pVal As String = TVALUES.To_STR(aProperty.Value)
        Dim bAddNode As Boolean = True
        Dim pname = aProperty.Name
        Dim bPrimeNode As Boolean = aProperty.GroupCode = 0
        Try
            If aProperty.NonDXF Or _EOF = True Then
                If aProperty.GroupCode = 0 Then sig = aPostScript
                If sig = "" Then
                    sig = aProperty.Signature(bSuppressName:=False, bReturnHandleLongs:=_HandlesToLongs, aPostScript:=aPostScript, bDecoded:=True, bSwitchesAsBooleans:=True)
                End If
                bAddNode = Not pVal.ToUpper.StartsWith("END")
            Else
                If aProperty.GroupCode = 0 Then
                    sig = aPostScript
                    bAddNode = Not pVal.ToUpper.StartsWith("END") Or pVal.ToUpper = "ENDBLK"
                    If sig = "" Then
                        sig = aProperty.GroupCode.ToString & "=" & pVal
                    Else
                        aPostScript = ""
                        If sig.ToUpper = "TABLE" Then
                            sig += "{" & aPostScript & "}"
                        End If
                    End If
                    If pVal.ToUpper = "EOF" Then
                        _EOF = True
                        bAddNode = False
                    Else
                        If Not String.IsNullOrEmpty(aPostScript) Then sig += "{" & aPostScript & "}"
                    End If
                Else
                    If String.IsNullOrEmpty(aPostScript) Then aPostScript = SetPostScript(aProperty)
                    sig = aProperty.Signature(bSuppressName:=False, bReturnHandleLongs:=_HandlesToLongs, aPostScript:=aPostScript, bDecoded:=True, bSwitchesAsBooleans:=_SwitchesToBooleans)
                    If aProperty.Name.ToUpper = "SECTIONNAME" Then
                        bAddNode = False
                    End If
                End If
            End If
            If bAddNode Then
                If aProperty.IsPointer Then aProperty.PropertyType = dxxPropertyTypes.Pointer
                If Not _Props.ContainsKey(aProperty.Key) Then
                    _Props.Add(aProperty.Key, aProperty)
                Else
                    Dim idx As Integer = 1
                    Dim npth As String = "_" & idx
                    Do Until Not _Props.ContainsKey(aProperty.Key & npth)
                        idx += 1
                        npth = "_" & idx
                    Loop
                    aProperty.Key += npth
                    _Props.Add(aProperty.Key, aProperty)
                End If
                TryGetNode(trvProps, aProperty.Key, _CurrentNode, bCreateIt:=True, aTextVal:=sig)
                Dim aClr As Color = _nodeColor
                If bInvisible Then
                    aClr = _Colors.Item(dxfLinetypes.Invisible)
                ElseIf bHidden Then
                    aClr = _Colors.Item(dxfLinetypes.Hidden)
                ElseIf bSuppressed Then
                    aClr = _Colors.Item("Suppressed")
                End If
                If _CurrentNode IsNot Nothing Then _CurrentNode.ForeColor = aClr
                Dim aNode As TreeNode
                If aProperty.PropertyType = dxxPropertyTypes.Handle Then
                    sig = TVALUES.To_STR(aProperty.Value)
                    Dim hProp As dxoProperty = Nothing
                    If Not _HandleNodes.TryGetValue(sig, hProp) Then
                        _HandleNodes.Add(sig, aProperty)
                    End If
                    If sig <> "" Then
                        sig = TVALUES.HexToInteger(sig) '& " [" & aProperty.Value & "]"
                        If Len(sig) < 7 Then
                            sig = New String("0", 7 - Len(sig)) & sig
                        End If
                    Else
                        sig = New String("0", 7)
                    End If
                    aNode = trvHandles.Nodes.Item("Handles").Nodes.Add(aProperty.Path, sig)
                    aNode.ForeColor = _CurrentNode.ForeColor
                    trvHandles.Nodes.Item("Handles").Expand()
                End If
            End If
            'write the property to the rich text box that will have the files contents
            If Not bInvisible And Not bHidden Then
                PopulateFile(aProperty)
            End If
            ticks += 1
            If ticks >= tickLimit Then
                Application.DoEvents()
                ticks = 0
            End If
        Catch ex As Exception
            SaveException(ex)
        End Try
    End Sub
    Private Sub PopulateFile(aProperty As dxoProperty)
        If aProperty Is Nothing Then Return
        If aProperty.Hidden Or aProperty.NonDXF Then Return
        'Dim aProp As TPROPERTY = aProperty.Strukture
        'Dim aStr As String = String.Empty
        'Dim sVal As String
        'Dim hVal As Integer
        'If enuFileType = dxxFileTypes.DXT Then
        '    If aProp.PropType <> dxxPropertyTypes.Switch Then
        '        sVal = aProp.Value.ToString()
        '    Else
        '        If aProp.Value = 1 Then sVal = "True" Else sVal = "False"
        '    End If
        '    If bolNumericHandles Then
        '        If aProp.PropType = dxxPropertyTypes.Handle Or aProp.PropType = dxxPropertyTypes.Pointer Then
        '            If sVal <> "" Then
        '                hVal = tValues.to_HandleINT(sVal)
        '                sVal = hVal.ToString()
        '            End If
        '        End If
        '    End If
        '    If aProp.GroupCode = 0 Then
        '        tbFileView.Text +="OBJECT=" & sVal)
        '    Else
        '        If aProp.PropType >= dxxPropertyTypes.Vector2D Then
        '            sVal = "(" & sVal
        '            sVal += "," & aProp.Value2.ToString()
        '            If aProp.PropType >= dxxPropertyTypes.Vector3D Then sVal += "," & aProp.Value3.ToString()
        '            sVal += ") "
        '        End If
        '        If Not bolSuppressDXTNames Then
        '            tbFileView.Text +="[" & aProp.Name & "]" & aProp.GroupCode & "=" & sVal)
        '        Else
        '            tbFileView.Text +=aProp.GroupCode & "=" & sVal)
        '        End If
        '    End If
        'Else
        Try
            Dim aProp As New TPROPERTY(aProperty)
            Dim aStr As String = String.Empty
            rtbSgnatures.Text += aProp.Signature(bSuppressName:=Not aProp.IsHeader, bReturnHandleLongs:=False, bDecoded:=False, bSwitchesAsBooleans:=False) & vbCr
            If Not aProp.Suppressed Then
                If aProp.IsHeader Then
                    aStr = aProp.Name
                    If aStr <> "" Then
                        rtbFileView.Text += "9" & vbCr
                        rtbFileView.Text += aStr & vbCr
                    End If
                End If
                rtbFileView.Text += aProp.GroupCode.ToString & vbCr
                rtbFileView.Text += aProp.StringValue(bSwitchesAsBooleans:=False) & vbCr

            End If
        Catch ex As Exception
            SaveException(ex)
        End Try
        'End If
    End Sub
    Private Sub frmDXFProperties_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    End Sub
    Private Sub frmDXFProperties_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Static bOnce As Boolean
        If bOnce Then Return
        TabControl1.TabIndex = 0
        bOnce = True
        LoadList(True)
    End Sub
    Private Sub _Gen_BufferTerminated() Handles _Gen.BufferTerminated
        Status = "Buffer Terminated Prematurely"
        _Canceled = True
        SaveException(New Exception("Buffer Terminated Prematurely"))
    End Sub
    Private Sub _Gen_HandleError(aErrorString As String, aErrorType As String, aProperty As dxoProperty, bProperty As dxoProperty) Handles _Gen.HandleError
        Dim ky As String
        Dim aProp As dxoProperty = aProperty
        Dim bProp As dxoProperty = bProperty
        Dim aNode As TreeNode = Nothing
        Dim pNode As TreeNode = Nothing
        Dim nNode As TreeNode
        _ErrCount += 1
        ky = "Error(" & _ErrCount & ")"
        TryGetNode(trvErrors, "Errors." & aErrorType, pNode, True, aErrorType, ".")
        pNode.ForeColor = Color.Red
        TryGetNode(trvErrors, pNode.Name & "." & ky, aNode, True, aErrorString, ".")
        If aProp IsNot Nothing Then
            Dim aPth As String = aProperty.Key
            nNode = aNode.Nodes.Add(aPth)
            aNode.Expand()
        End If
        If bProp IsNot Nothing Then
            Dim bPth As String = bProperty.Key
            nNode = aNode.Nodes.Add(bPth)
            aNode.Expand()
        End If
    End Sub
    Private Sub _Gen_StatusChange(aStatus As String) Handles _Gen.StatusChange, _Writer.StatusChange
        If Not _Canceled Then Status = aStatus
    End Sub
    Private Sub cmdClose_Click(sender As Object, e As EventArgs) Handles cmdClose.Click
        If _Gen IsNot Nothing Then
            If _Gen.Buffering Then
                _Gen.CancelBuffer = True
                Return
            End If
        End If
        Loading = False
        Hide()
    End Sub
    Private Sub frmDXFProperties_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        If Not Visible Then Loading = False
    End Sub
    Friend Sub ShowImageProperties(aImage As dxfImage, aOwnerForm As System.Windows.Forms.Form)
        Loading = False
        chkTable_1_CheckedChanged(Nothing, Nothing)
        chkSettings_1_CheckedChanged(Nothing, Nothing)
        'txtGoto.Text = ""
        If aImage Is Nothing Then Return
        _Image = aImage
        Try
            ShowDialog(aOwnerForm)
        Catch ex As Exception
        Finally
            TeminateReferences()
        End Try
    End Sub
    Private Sub TeminateReferences()
        _Gen = Nothing
        _Image = Nothing
        _Props = Nothing
    End Sub
    Private Function GetHandleProp(aHandle As String) As dxoProperty
        If aHandle = "" Or aHandle = "0" Or _Props Is Nothing Then Return Nothing
        For Each aProp As dxoProperty In _Props.Values
            If aProp.PropertyType = dxxPropertyTypes.Handle Then
                If aProp.Value.ToString() = aHandle Then
                    Return aProp
                    Exit For
                End If
            End If
        Next
        Return Nothing
    End Function
    Private Function HighlightNode(aNode As TreeNode) As Boolean
        Static bHighlight As Boolean
        If bHighlight Or Loading Then Return False
        bHighlight = True
        Dim _rVal As Boolean = aNode IsNot Nothing
        If _rVal Then
            _CurrentNode = aNode
            txtNode.Text = aNode.Name
        Else
            txtNode.Text = ""
        End If
        txtNode.Refresh()
        bHighlight = False
        Return _rVal
    End Function
    Private Sub SetNodeFocus(aNodePath As String)
        Dim aProp As dxoProperty = Nothing
        Dim sNodes() As TreeNode = trvProps.Nodes.Find(aNodePath, searchAllChildren:=True)
        If sNodes.Length <= 0 Then Return
        Dim bNode As TreeNode = sNodes(0)
        trvProps.Select()
        trvProps.SelectedNode = bNode
        bNode.EnsureVisible()
        HighlightNode(bNode)
    End Sub
    Private Function GetAppPath() As String
        Dim i As Integer
        Dim rPath As String = System.Reflection.Assembly.GetExecutingAssembly.Location()
        i = InStrRev(rPath, "\", Compare:=vbTextCompare)
        If i > 0 Then rPath = rPath.Substring(1, i - 1)
        Return rPath
    End Function
    Private Property WorkingFolder As String
        Get
            If _WorkingFolder = "" Then
                _WorkingFolder = My.Settings.ProperyViewer_WorkingFolder
                If _WorkingFolder <> "" Then
                    If Not IO.Directory.Exists(_WorkingFolder) Then _WorkingFolder = ""
                End If
            End If
            If _WorkingFolder = "" Then
                If IO.Directory.Exists("C:\Users\E342367\Documents\Junk") Then
                    _WorkingFolder = "C:\Users\E342367\Documents\Junk"
                Else
                    _WorkingFolder = GetAppPath()
                End If
                My.Settings.ProperyViewer_WorkingFolder = _WorkingFolder
            End If
            Return _WorkingFolder
        End Get
        Set(value As String)
            _WorkingFolder = value
            My.Settings.ProperyViewer_WorkingFolder = _WorkingFolder
        End Set
    End Property
    Private Sub WriteDXF()
        Dim path As String = txtFolder1.Text.Trim
        Dim fname As String = txtFileName.Text.Trim
        If Not path.EndsWith("\") Then path += "\"
        Dim fpath = path & fname
        If IO.File.Exists(fpath) Then


            If MessageBox.Show($"Overwrite '{ fpath }' ?", "File Aleady Exists", MessageBoxButtons.YesNo, icon:=MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Else
            If MessageBox.Show($"Write '{ fpath }' ?", "Write New File?", MessageBoxButtons.YesNo, icon:=MessageBoxIcon.Warning) <> DialogResult.No Then Return

        End If
        If IO.File.Exists(fpath) Then
            If dxfUtils.FileIsOpen(New IO.FileInfo(fpath)) Then

                MessageBox.Show($"File '{fpath }' Is Locked By Another Application. Output Aborted.", "Output Aborted", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)

                Return
            End If
        End If
        _Image.FolderPath = path
        Dim rErr As String = String.Empty
        Loading = True
        _Writer = New dxoFileTool
        Try
            fpath = _Image.WriteToFile(_Writer, aFileType:=dxxFileTypes.DXF, aFileSpec:=fpath, aVersion:=_Image.FileVersion, bSuppressDimOverrides:=True, bNoErrors:=True, rErrString:=rErr, bSuppressDXTNames:=False, bNumericHandles:=False)
            If rErr <> "" Then
                MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {rErr}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)

                Status = $"File Write Error: '{ rErr }'"
            Else
                Status = $"File Written Successfully: '{ fpath }'"
            End If
        Catch ex As Exception
        Finally
            _Writer = Nothing
            Loading = False
        End Try
    End Sub
    Private Sub trvProps_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles trvProps.NodeMouseClick
        HighlightNode(e.Node)
    End Sub
    Private Sub rtbFileView_KeyPress(sender As Object, e As KeyPressEventArgs) Handles rtbFileView.KeyPress
        e.KeyChar = ""
    End Sub
    Private Sub rtbFileView_KeyDown(sender As Object, e As KeyEventArgs) Handles rtbFileView.KeyDown
        e.SuppressKeyPress = True
    End Sub
    Private Sub rtbSgnatures_KeyPress(sender As Object, e As KeyPressEventArgs) Handles rtbSgnatures.KeyPress
        e.KeyChar = ""
    End Sub
    Private Sub rtbSgnatures_KeyDown(sender As Object, e As KeyEventArgs) Handles rtbSgnatures.KeyDown
        e.SuppressKeyPress = True
    End Sub
    Private Sub frmDXFProperties_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        SettingsSave()
        TeminateReferences()
    End Sub
    Private Sub cmdReload_Click(sender As Object, e As EventArgs) Handles cmdReload.Click
        If Not Loading Then LoadList()
    End Sub
    Private Sub trvProps_NodeMouseHover(sender As Object, e As TreeNodeMouseHoverEventArgs) Handles trvProps.NodeMouseHover
        'HighlightNode(e.Node)
    End Sub
    Private Sub trvProps_Click(sender As Object, e As EventArgs) Handles trvProps.Click
        HighlightNode(trvProps.SelectedNode)
    End Sub
    Private Sub trvProps_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles trvProps.NodeMouseDoubleClick
        If HighlightNode(trvProps.SelectedNode) And _Props IsNot Nothing Then
            Dim aProp As dxoProperty = Nothing
            If Not _Props.TryGetValue(_CurrentNode.Name, aProp) Then Return
            If aProp.PropertyType = dxxPropertyTypes.Pointer Then
                Dim bProp As dxoProperty = Nothing
                If Not _HandleNodes.TryGetValue(TVALUES.To_STR(aProp.Value), bProp) Then Return
                SetNodeFocus(bProp.Key)
            End If
        End If
    End Sub
    Private Sub chkSettings_0_CheckedChanged(sender As Object, e As EventArgs) Handles chkSettings_0.CheckedChanged
        If _Loading Then Return
        Dim wuz As Boolean = _Loading
        _Loading = True
        Dim chkBox As CheckBox
        For Each cntrl As Control In gbSettings.Controls
            If TypeOf cntrl Is CheckBox Then
                chkBox = cntrl
                If chkBox IsNot chkSettings_0 Then
                    chkBox.Checked = Not chkSettings_0.Checked
                End If
            End If
        Next
        _Loading = wuz
    End Sub
    Private Sub chkSuppress_7_CheckedChanged(sender As Object, e As EventArgs) Handles chkSuppress_7.CheckedChanged
        If _Loading Then Return
        Dim wuz As Boolean = _Loading
        _Loading = True
        Dim chkBox As CheckBox
        For Each cntrl As Control In gbTables.Controls
            If TypeOf cntrl Is CheckBox Then
                chkBox = cntrl
                If chkBox IsNot chkSuppress_7 Then
                    chkBox.Checked = chkSuppress_7.Checked
                End If
            End If
        Next
        _Loading = wuz
    End Sub
    Private Sub chkSettings_1_CheckedChanged(sender As Object, e As EventArgs) Handles chkSettings_1.CheckedChanged, chkSettings_2.CheckedChanged, chkSettings_3.CheckedChanged, chkSettings_4.CheckedChanged, chkSettings_5.CheckedChanged,
    chkSettings_6.CheckedChanged, chkSettings_7.CheckedChanged, chkSettings_8.CheckedChanged
        If _Loading Then Return
        Dim wuz As Boolean = _Loading
        _Loading = True
        Dim bNone As Boolean = Not chkSettings_1.Checked And Not chkSettings_2.Checked And Not chkSettings_3.Checked And Not chkSettings_4.Checked
        bNone = bNone And Not chkSettings_5.Checked And Not chkSettings_6.Checked And Not chkSettings_7.Checked And Not chkSettings_8.Checked
        chkSettings_0.Checked = bNone
        _Loading = wuz
    End Sub
    Private Sub chkTable_1_CheckedChanged(sender As Object, e As EventArgs) Handles chkTable_1.CheckedChanged, chkTable_2.CheckedChanged, chkTable_3.CheckedChanged, chkTable_4.CheckedChanged, chkTable_5.CheckedChanged,
   chkTable_6.CheckedChanged, chkTable_7.CheckedChanged, chkTable_8.CheckedChanged, chkTable_9.CheckedChanged
        If _Loading Then Return
        Dim wuz As Boolean = _Loading
        _Loading = True
        Dim bNone As Boolean = Not chkTable_1.Checked And Not chkTable_2.Checked And Not chkTable_3.Checked And Not chkTable_4.Checked
        bNone = bNone And Not chkTable_5.Checked And Not chkTable_6.Checked And Not chkTable_7.Checked And Not chkTable_8.Checked And Not chkTable_9.Checked
        SetCheckVal(Not bNone, "ShowTables")
        _Loading = wuz
    End Sub
    Private Sub _Gen_PathChangeEvent(e As dxoFileHandlerEventArg) Handles _Gen.PathChangeEvent
        Dim aNode As TreeNode = Nothing
        If e.EventType = dxxFileHandlerEvents.PathIncrease Then
            If TryGetNode(trvProps, e.LastPath, aNode, False) Then
                aNode.Collapse()
            End If
            If TryGetNode(trvProps, e.CurrentPath, aNode, True) Then
                aNode.Expand()
                aNode.EnsureVisible()
            End If
        ElseIf e.EventType = dxxFileHandlerEvents.PathDecrease Then
            If TryGetNode(trvProps, e.LastPath, aNode, False) Then
                aNode.Collapse()
            End If
            If TryGetNode(trvProps, e.CurrentPath, aNode, True) Then
                aNode.EnsureVisible()
            End If
        End If
        trvProps.Refresh()
    End Sub
    Private Sub trvProps_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles trvProps.AfterSelect
    End Sub
    Private Sub trvErrors_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles trvErrors.NodeMouseDoubleClick
        If trvErrors.SelectedNode IsNot Nothing And _Props IsNot Nothing Then
            SetNodeFocus(trvErrors.SelectedNode.Text)
        End If
    End Sub
    Private Sub trvErrors_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles trvErrors.NodeMouseClick
        If trvErrors.SelectedNode Is Nothing Then Return
        txtErrorKey.Text = trvErrors.SelectedNode.Text
    End Sub
    Private Sub rtbMenu1_Copy_Click(sender As Object, e As EventArgs) Handles rtbMenu1_Copy.Click
        rtbFileView.Copy()
    End Sub
    Private Sub rtbMenu1_SelectAll_Click(sender As Object, e As EventArgs) Handles rtbMenu1_SelectAll.Click
        rtbFileView.SelectAll()
    End Sub
    Private Sub txtFolder1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtFolder1.KeyPress
        e.KeyChar = ""
    End Sub
    Private Sub txtFolder1_DoubleClick(sender As Object, e As EventArgs) Handles txtFolder1.DoubleClick
        FolderBrowserDialog1.SelectedPath = WorkingFolder
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            WorkingFolder = FolderBrowserDialog1.SelectedPath
            txtFolder1.Text = WorkingFolder
        End If
    End Sub
    Private Sub btnBrowseFolder_Click(sender As Object, e As EventArgs) Handles btnBrowseFolder.Click
        txtFolder1_DoubleClick(sender, e)
    End Sub
    Private Sub rtbMenu1_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles rtbMenu1.Opening
    End Sub
    Private Sub gbFileView_Enter(sender As Object, e As EventArgs) Handles gbFileView.Enter
    End Sub
    Private Sub TabControl1_TabIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.TabIndexChanged
        If TabControl1.TabIndex = 1 Then
            txtFileName.Text = _Image.FileName(bSuppressPath:=True)
        End If
    End Sub
    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        If TabControl1.SelectedIndex = 1 Then
            txtFileName.Text = _Image.FileName(bSuppressPath:=True)
        End If
    End Sub
    Private Sub txtFolder1_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFolder1.KeyDown
        e.SuppressKeyPress = True
    End Sub
    Private Sub btnWriteDXF_Click(sender As Object, e As EventArgs) Handles btnWriteDXF.Click
        If Loading Then Return
        WriteDXF()
    End Sub
End Class
