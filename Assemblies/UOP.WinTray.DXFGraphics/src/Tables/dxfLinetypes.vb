Imports System.IO
Imports System.Reflection
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Class dxfLinetypes
#Region "Methods"
#End Region 'Methods
#Region "Shared Methods"
        Friend Shared Function GetCurrentDefinition(aName As String) As TTABLEENTRY
            Dim rFound As Boolean = False
            Return GetCurrentDefinition(aName, rFound)
        End Function
        Public Shared ReadOnly Property Invisible As String
            Get
                Return "Invisible"
            End Get
        End Property
        Public Shared ReadOnly Property ByLayer As String
            Get
                Return "ByLayer"
            End Get
        End Property
        Public Shared ReadOnly Property ByBlock As String
            Get
                Return "ByBlock"
            End Get
        End Property
        Public Shared ReadOnly Property Continuous As String
            Get
                Return "Continuous"
            End Get
        End Property
        Public Shared ReadOnly Property Hidden As String
            Get
                Return "Hidden"
            End Get
        End Property
        Public Shared ReadOnly Property Hidden2 As String
            Get
                Return "Hidden2"
            End Get
        End Property

        Public Shared ReadOnly Property Center As String
            Get
                Return "Center"
            End Get
        End Property
        Public Shared ReadOnly Property Center2 As String
            Get
                Return "Center2"
            End Get
        End Property

        Public Shared ReadOnly Property Phantom As String
            Get
                Return "Phantom"
            End Get
        End Property

        Public Shared ReadOnly Property Phantom2 As String
            Get
                Return "Phantom2"
            End Get
        End Property
        Friend Shared Function GetCurrentDefinition(aName As String, ByRef rFound As Boolean) As TTABLEENTRY
            rFound = False
            Dim _rVal As TTABLEENTRY = TTABLEENTRY.Null
            Select Case aName.ToUpper
                Case "BYLAYER"
                    rFound = True
                    Return New TTABLEENTRY(dxxReferenceTypes.LTYPE, dxfLinetypes.ByLayer, bIsDefault:=True) With {.Domain = dxxDrawingDomains.Paper}
                Case "BYBLOCK"
                    rFound = True
                    Return New TTABLEENTRY(dxxReferenceTypes.LTYPE, dxfLinetypes.ByBlock, bIsDefault:=True) With {.Domain = dxxDrawingDomains.Paper}
                Case "CONTINUOUS"
                    rFound = True
                    Return New TTABLEENTRY(dxxReferenceTypes.LTYPE, dxfLinetypes.Continuous, bIsDefault:=True)
                Case "INVISIBLE"
                    rFound = True
                    Return New TTABLEENTRY(dxxReferenceTypes.LTYPE, dxfLinetypes.Invisible, bIsDefault:=True)
                Case Else
                    'look in the global array.....
                    If dxfLinetypes.GlobalLinetypes.TryGet(aName, "", False, rEntry:=_rVal) Then
                        _rVal.IsDefault = True
                        rFound = True
                        Return _rVal
                    Else
                        'use the known defaults if we can
                        _rVal = dxfLinetypes.GlobalLinetypes.Entry(dxfLinetypes.Continuous)
                        _rVal.IsDefault = True
                        _rVal.Name = aName
                        Return _rVal
                    End If
            End Select
            Return _rVal
        End Function

        Friend Shared Function DefaultDefinition(aName As String, Optional bReturnSomething As Boolean = False) As TTABLEENTRY
            Dim rDefFound As Boolean = False
            Dim rName As String = String.Empty
            Return DefaultDefinition(aName, bReturnSomething, rDefFound, rName)
        End Function
        Friend Shared Function DefaultDefinition(aName As String, bReturnSomething As Boolean, ByRef rDefFound As Boolean, ByRef rName As String) As TTABLEENTRY
            If aName IsNot Nothing Then aName = aName.Trim() Else aName = String.Empty
            If String.IsNullOrWhiteSpace(aName) Then aName = dxfLinetypes.ByLayer
            rName = String.Empty
            Dim aEntry As New TTABLEENTRY(dxxReferenceTypes.UNDEFINED, "")
            aEntry = dxfLinetypes.GetCurrentDefinition(aName, rDefFound)
            If bReturnSomething And Not rDefFound Then
                aEntry = dxfLinetypes.GetCurrentDefinition(dxfLinetypes.ByLayer, rDefFound)
                rDefFound = True
            End If
            If rDefFound Then rName = aEntry.Name
            Return aEntry
        End Function

        Public Shared Function DefaultDef(aName As String, Optional bReturnSomething As Boolean = False, Optional bNoLogicals As Boolean = False) As dxoLinetype
            Dim rDefFound As Boolean = False
            Dim rName As String = String.Empty
            Return DefaultDef(aName, bReturnSomething, rDefFound, rName, bNoLogicals)
        End Function
        Public Shared Function DefaultDef(aName As String, bReturnSomething As Boolean, ByRef rDefFound As Boolean, ByRef rName As String, Optional bNoLogicals As Boolean = False) As dxoLinetype
            If aName IsNot Nothing Then aName = aName.Trim() Else aName = String.Empty
            If String.IsNullOrWhiteSpace(aName) Then aName = dxfLinetypes.ByLayer
            rName = String.Empty
            Dim aEntry As dxoLinetype = dxfLinetypes.GetCurrentDef(aName, rDefFound)
            If bReturnSomething And Not rDefFound Then
                If Not bNoLogicals Then
                    aEntry = dxfLinetypes.GetCurrentDef(dxfLinetypes.ByLayer, rDefFound)
                Else
                    aEntry = dxfLinetypes.GetCurrentDef(dxfLinetypes.Continuous, rDefFound)
                End If

                rDefFound = True
            End If
            If rDefFound Then rName = aEntry.Name
            Return aEntry
        End Function
        Public Shared Function GetCurrentDef(aName As String) As dxoLinetype
            Dim rFound As Boolean = False
            Return GetCurrentDef(aName, rFound)
        End Function


        Public Shared Function GetCurrentDef(aName As String, ByRef rFound As Boolean) As dxoLinetype
            rFound = False

            Select Case aName.ToUpper
                Case "BYLAYER"
                    rFound = True
                    Return New dxoLinetype(dxfLinetypes.ByLayer) With {.IsDefault = True, .Domain = dxxDrawingDomains.Paper}
                Case "BYBLOCK"
                    rFound = True
                    Return New dxoLinetype(dxfLinetypes.ByBlock) With {.IsDefault = True, .Domain = dxxDrawingDomains.Paper}

                Case "CONTINUOUS"
                    rFound = True
                    Return New dxoLinetype(dxfLinetypes.Continuous) With {.IsDefault = True, .Domain = dxxDrawingDomains.Model}
                Case "INVISIBLE"
                    rFound = True
                    Return New dxoLinetype(dxfLinetypes.Invisible) With {.IsDefault = True, .Domain = dxxDrawingDomains.Model}

                Case Else
                    'look in the global array.....
                    Dim glt As TTABLEENTRY = TTABLEENTRY.Null
                    If dxfLinetypes.GlobalLinetypes.TryGet(aName, "", False, rEntry:=glt) Then
                        Dim _rVal As New dxoLinetype(glt)
                        _rVal.IsDefault = True
                        rFound = True
                        Return _rVal
                    Else
                        'use the known defaults if we can
                        glt = dxfLinetypes.GlobalLinetypes.Entry(dxfLinetypes.Continuous)
                        Dim _rVal As New dxoLinetype(glt)
                        _rVal.IsDefault = True
                        rFound = True
                        Return _rVal
                    End If
            End Select
            Return Nothing
        End Function

        Friend Shared Sub AssignPropertyNames(aLTProps As TPROPERTIES)
            Dim i As Integer
            Dim idx As Integer
            Dim eCnt As Integer
            Dim aProp As TPROPERTY
            idx = aLTProps.GroupCodeIndex(70, 1)
            If idx <= 0 Then idx = 1
            For i = idx To aLTProps.Count
                aProp = aLTProps.Item(i)
                Select Case aProp.GroupCode
                    Case 49
                        eCnt += 1
                        aProp.Name = $"Element({ eCnt }) Length"
                    Case 74
                        aProp.Name = $"Element({ eCnt }) Element Type"
                    Case 75
                        aProp.Name = $"Element({ eCnt }) Shape Number"
                    Case 340
                        aProp.Name = $"Element({ eCnt }) Style"
                        aProp.PropType = dxxPropertyTypes.Pointer
                    Case 46
                        aProp.Name = $"Element({ eCnt }) Scale"
                    Case 50
                        aProp.Name = $"Element({ eCnt }) Shape Rotation"
                    Case 44
                        aProp.Name = $"Element({ eCnt }) X Offset"
                    Case 45
                        aProp.Name = $"Element({ eCnt }) Y Offset"
                    Case 9
                        aProp.Name = $"Element({ eCnt }) Text String"
                End Select
                aLTProps.SetItem(i, aProp)
            Next i
        End Sub
        Public Shared Function SelectLineType(aOwner As IWin32Window, aInitName As String, bIcludeLogicals As Boolean, Optional bComboStyle As Boolean = False) As String
            Dim aFrm As New frmTables
            Dim _rVal As String = aFrm.ShowSelect(aOwner, dxxReferenceTypes.LTYPE, Nothing, aInitName, bComboStyle, bIcludeLogicals, False)
            aFrm.Dispose()
            aFrm = Nothing
            Return _rVal
        End Function

        Private Shared _GlobalLinetypes As TTABLE


        Friend Shared ReadOnly Property GlobalLinetypes As TTABLE
            Get
                If _GlobalLinetypes.Count <= 0 Then
                    _GlobalLinetypes = New TTABLE(dxxReferenceTypes.LTYPE)
                    Dim aLtStrings As List(Of String) = dxfLinetypes.DefaultDefsDefinitionStrings


                    For i As Integer = 0 To aLtStrings.Count - 1
                        Dim aStr As String = aLtStrings.Item(i)
                        If i + 1 > aLtStrings.Count - 1 Then Exit For
                        Dim bStr As String = aLtStrings.Item(i + 1)  'two lines per lintetype

                        _GlobalLinetypes.Add(CreateLinetypeByString(aStr, bStr))
                        i += 1
                    Next i
                    _GlobalLinetypes.ImageGUID = ""
                End If
                Return _GlobalLinetypes
            End Get

        End Property

        Friend Shared Function CreateLinetypeByString(aString As String, bString As String) As TTABLEENTRY
            Dim _rVal As New TTABLEENTRY(dxxReferenceTypes.LTYPE, "")
            Dim ltVals As New TVALUES(0)
            _rVal.Props = dxfLinetypes.PropsByLines(_rVal.Props, aString, bString, bString.Contains("["), ltVals)
            _rVal.Values = ltVals
            Return _rVal
        End Function
        Friend Shared Function PropsByLines(aBaseProps As TPROPERTIES, aString1 As String, aString2 As String, aComplex As Boolean, ByRef rElements As TVALUES) As TPROPERTIES
            Dim _rVal As TPROPERTIES = aBaseProps
            If _rVal.Count <= 0 Then _rVal = dxpProperties.GetReferenceProps(dxxReferenceTypes.LTYPE, "")

            Dim aStr As String = String.Empty

            Dim sVals1() As String = aString1.Split(",")
            Dim sVals2() As String = aString2.Split(",")
            rElements = New TVALUES(0)
            If sVals1.Length - 1 >= 1 Then
                aStr = sVals1(0).Trim()
                If aStr.Length >= 1 Then
                    If aStr.StartsWith("*") Then aStr = aStr.Substring(1, aStr.Length - 1).Trim()
                    _rVal.SetValueGC(2, aStr)  'name
                End If
                _rVal.SetValueGC(3, sVals1(1).Trim()) 'description
            End If
            If sVals2.Length >= 1 Then
                Dim eCnt As Integer
                Dim eTot As Double
                Dim eLen As Double
                aStr = sVals2(0).Trim()
                If aStr.Length >= 1 Then
                    For i As Integer = 1 To sVals2.Length
                        If TVALUES.IsNumber(sVals2(i - 1)) Then
                            eCnt += 1
                            eLen = TVALUES.To_DBL(sVals2(i - 1))
                            rElements.Add(eLen)
                            eTot += Math.Abs(eLen)
                            _rVal.Add(New TPROPERTY(49, eLen, "Element Length", dxxPropertyTypes.dxf_Double))
                            _rVal.Add(New TPROPERTY(74, 0, "Segment Count", dxxPropertyTypes.dxf_Integer, mzValueControls.Positive))
                        End If

                    Next i
                    _rVal.SetValueGC(73, eCnt)
                    _rVal.SetValueGC(40, eTot)
                    rElements.BaseValue = eTot

                End If
            End If

            dxfLinetypes.AssignPropertyNames(_rVal)
            Return _rVal
        End Function
        Friend Shared Function ReadToTextCollection(aFileName As String, ByRef rErrFlag As Boolean) As List(Of String)
            Dim _rVal As List(Of String) = Nothing
            '#1the file name to read the linetypes from
            '^returns true if the linetypes were read succesfully
            rErrFlag = True
            If String.IsNullOrWhiteSpace(aFileName) Then Return _rVal Else aFileName = aFileName.Trim()
            If Not IO.File.Exists(aFileName) Then Return _rVal

            Try
                Dim aTxt As String
                Dim bTxt As String
                Dim sLines As List(Of String) = IO.File.ReadAllLines(aFileName).ToList()

                _rVal = New List(Of String)

                For i As Integer = 1 To sLines.Count
                    aTxt = sLines.Item(i - 1)
                    If i + 1 > sLines.Count Then Exit For
                    bTxt = sLines.Item(i)
                    If aTxt.Contains(",") And bTxt.Contains(",") Then
                        If aTxt.StartsWith("*") And bTxt.Substring(0, 2).ToUpper() = "A," Then
                            _rVal.Add(aTxt)
                            _rVal.Add(bTxt)

                        End If
                    End If
                    'Application.DoEvents()
                    i += 1
                Next i
                rErrFlag = _rVal.Count <= 0
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Linetype Read Error")
                rErrFlag = True

            End Try
            Return _rVal

        End Function

        Private Shared _DefaultDefsDefinitionStrings As List(Of String)
        Friend Shared ReadOnly Property DefaultDefsDefinitionStrings As List(Of String)
            Get
                If _DefaultDefsDefinitionStrings Is Nothing Then
                    Dim aCol As List(Of String) = Nothing
                    Dim aFlg As Boolean
                    If IO.File.Exists(IO.Path.Combine(Application.ExecutablePath, "\Acad.Lin")) Then
                        aCol = dxfLinetypes.ReadToTextCollection(IO.Path.Combine(Application.ExecutablePath, "\Acad.Lin"), aFlg)
                        If Not aFlg Then aCol = Nothing
                    End If
                    If aCol IsNot Nothing Then
                        _DefaultDefsDefinitionStrings = aCol
                    Else
                        _DefaultDefsDefinitionStrings = New List(Of String) From {
                            "*BORDER,Border __ __ . __ __ . __ __ . __ __ . __ __ .",
                            "A,.5,-.25,.5,-.25,0,-.25",
                            "*BORDER2,Border(.5x) __.__.__.__.__.__.__.__.__.__.__.",
                            "A,.25,-.125,.25,-.125,0,-.125",
                            "*BORDERX2,Border(2x) ____  ____  .  ____  ____  .  ___",
                            "A,1.0,-.5,1.0,-.5,0,-.5",
                            "*CENTER,Center ____ _ ____ _ ____ _ ____ _ ____ _ ____",
                            "A,1.25,-.25,.25,-.25",
                            "*CENTER2,Center(.5x) ___ _ ___ _ ___ _ ___ _ ___ _ ___",
                            "A,.75,-.125,.125,-.125",
                            "*CENTERX2,Center(2x) ________  __  ________  __  _____",
                            "A,2.5,-.5,.5,-.5",
                            "*DASHDOT,Dash dot __ . __ . __ . __ . __ . __ . __ . __",
                            "A,.5,-.25,0,-.25",
                            "*DASHDOT2,Dash dot(.5x) _._._._._._._._._._._._._._._.",
                            "A,.25,-.125,0,-.125",
                            "*DASHDOTX2,Dash dot(2x) ____  .  ____  .  ____  .  ___",
                            "A,1.0,-.5,0,-.5",
                            "*DASHED,Dashed __ __ __ __ __ __ __ __ __ __ __ __ __ _",
                            "A,.5,-.25",
                            "*DASHED2,Dashed(.5x) _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _",
                            "A,.25,-.125",
                            "*DASHEDX2,Dashed(2x) ____  ____  ____  ____  ____  ___",
                            "A,1.0,-.5",
                            "*DIVIDE,Divide ____ . . ____ . . ____ . . ____ . . ____",
                            "A,.5,-.25,0,-.25,0,-.25",
                            "*DIVIDE2,Divide(.5x) __..__..__..__..__..__..__..__.._",
                            "A,.25,-.125,0,-.125,0,-.125",
                            "*DIVIDEX2,Divide(2x) ________  .  .  ________  .  .  _",
                            "A,1.0,-.5,0,-.5,0,-.5",
                            "*DOT,Dot . . . . . . . . . . . . . . . . . . . . . . . .",
                            "A,0,-.25",
                            "*DOT2,Dot(.5x)........................................",
                            "A,0,-.125",
                            "*DOTX2,Dot(2x).  .  .  .  .  .  .  .  .  .  .  .  .  .",
                            "A,0,-.5",
                            "*HIDDEN,Hidden __ __ __ __ __ __ __ __ __ __ __ __ __ __",
                            "A,.25,-.125",
                            "*HIDDEN2,Hidden(.5x) _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _",
                            "A,.125,-.0625",
                            "*HIDDENX2,Hidden(2x) ____ ____ ____ ____ ____ ____ ____",
                            "A,.5,-.25",
                            "*PHANTOM,Phantom ______  __  __  ______  __  __  ______",
                            "A,1.25,-.25,.25,-.25,.25,-.25",
                            "*PHANTOM2,Phantom(.5x) ___ _ _ ___ _ _ ___ _ _ ___ _ _",
                            "A,.625,-.125,.125,-.125,.125,-.125",
                            "*PHANTOMX2,Phantom(2x) ____________    ____    ____   _",
                            "A,2.5,-.5,.5,-.5,.5,-.5",
                            "*ACAD_ISO02W100,ISO dash __ __ __ __ __ __ __ __ __ __ __ __ __",
                            "A,12,-3",
                            "*ACAD_ISO03W100,ISO dash space __    __    __    __    __    __",
                            "A,12,-18",
                            "*ACAD_ISO04W100,ISO long-dash dot ____ . ____ . ____ . ____ . _",
                            "A,24,-3,0,-3",
                            "*ACAD_ISO05W100,ISO long-dash double-dot ____ .. ____ .. ____ .",
                            "A,24,-3,0,-3,0,-3",
                            "*ACAD_ISO06W100,ISO long-dash triple-dot ____ ... ____ ... ____",
                            "A,24,-3,0,-3,0,-3,0,-3",
                            "*ACAD_ISO07W100,ISO dot . . . . . . . . . . . . . . . . . . . .",
                            "A,0,-3",
                            "*ACAD_ISO08W100,ISO long-dash short-dash ____ __ ____ __ ____ _",
                            "A,24,-3,6,-3",
                            "*ACAD_ISO09W100,ISO long-dash double-short-dash ____ __ __ ____",
                            "A,24,-3,6,-3,6,-3",
                            "*ACAD_ISO10W100,ISO dash dot __ . __ . __ . __ . __ . __ . __ .",
                            "A,12,-3,0,-3",
                            "*ACAD_ISO11W100,ISO double-dash dot __ __ . __ __ . __ __ . __ _",
                            "A,12,-3,12,-3,0,-3",
                            "*ACAD_ISO12W100,ISO dash double-dot __ . . __ . . __ . . __ . .",
                            "A,12,-3,0,-3,0,-3",
                            "*ACAD_ISO13W100,ISO double-dash double-dot __ __ . . __ __ . . _",
                            "A,12,-3,12,-3,0,-3,0,-3",
                            "*ACAD_ISO14W100,ISO dash triple-dot __ . . . __ . . . __ . . . _",
                            "A,12,-3,0,-3,0,-3,0,-3",
                            "*ACAD_ISO15W100,ISO double-dash triple-dot __ __ . . . __ __ . .",
                            "A,12,-3,12,-3,0,-3,0,-3,0,-3",
                            "*FENCELINE1,Fenceline circle ----0-----0----0-----0----0-----0--",
                            "A,.25,-.1,[CIRC1,ltypeshp.shx,x=-.1,s=.1],-.1,1",
                            "*FENCELINE2,Fenceline square ----[]-----[]----[]-----[]----[]---",
                            "A,.25,-.1,[BOX,ltypeshp.shx,x=-.1,s=.1],-.1,1",
                            "*TRACKS,Tracks -|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-",
                            "A,.15,[TRACK1,ltypeshp.shx,s=.25],.15",
                            "*BATTING,Batting SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS",
                            "A,.0001,-.1,[BAT,ltypeshp.shx,x=-.1,s=.1],-.2,[BAT,ltypeshp.shx,r=180,x=.1,s=.1],-.1",
                            "*HOT_WATER_SUPPLY,Hot water supply ---- HW ---- HW ---- HW ----",
                            "A,.5,-.2,[""HW"",STANDARD,S=.1,R=0.0,X=-0.1,Y=-.05],-.2",
                            "*GAS_LINE,Gas line ----GAS----GAS----GAS----GAS----GAS----GAS--",
                            "A,.5,-.2,[""GAS"" ,STANDARD,S=.1,R=0.0,X=-0.1,Y=-.05],-.25",
                            "*ZIGZAG,Zig zag /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/",
                            "A,.0001,-.2,[ZIG,ltypeshp.shx,x=-.2,s=.2],-.4,[ZIG,ltypeshp.shx,r=180,x=.2,s=.2],-.2"
                        }
                    End If
                End If
                AddEmbeddedLineTypes(_DefaultDefsDefinitionStrings, "acadlinetyes.txt", False)
                Return _DefaultDefsDefinitionStrings
            End Get
        End Property

        Private Shared Sub AddEmbeddedLineTypes(ByRef ioLinetypeStrings As List(Of String), aFilename As String, Optional bReplaceExisting As Boolean = False)
            '#1the path to the target .SHP file
            '#2flag to overwrite the definitions of shape fonts that are already defined
            '^loads a font from the target .SHP file
            '~errors are ignored

            If ioLinetypeStrings Is Nothing Then ioLinetypeStrings = New List(Of String)

            Dim s As Stream = Nothing

            Try
                If Not String.IsNullOrEmpty(aFilename) Then aFilename = aFilename.Trim Else aFilename = "acadlinetyes.txt"
                If aFilename.Length < 5 Then Return
                s = Assembly.GetExecutingAssembly.GetManifestResourceStream(aFilename)
                If s Is Nothing Then Return

                Dim aReader As New StreamReader(s)
                Dim sLines As New List(Of String)
                Do While Not aReader.EndOfStream
                    Dim txt1 As String = aReader.ReadLine.Trim()
                    If txt1 = "" Or Not txt1.StartsWith("*") Or txt1.StartsWith(";") Then Continue Do
                    If txt1.Contains(",") Then sLines.Add(txt1)

                Loop



                s.Close()
                s.Dispose()
                s = Nothing

                For i As Integer = 1 To sLines.Count
                    Dim aTxt As String = sLines.Item(i - 1)
                    If i + 1 > sLines.Count Then Exit For
                    Dim bTxt As String = sLines.Item(i)
                    Dim idx As Integer = aTxt.IndexOf(",")
                    If idx > 0 And bTxt.Contains(",") Then
                        If aTxt.StartsWith("*") And bTxt.Substring(0, 2).ToUpper() = "A," Then
                            Dim ltname As String = aTxt.Substring(0, idx)

                            If ioLinetypeStrings.FindIndex(Function(x) x.ToUpper.StartsWith(ltname)) >= 0 Then Continue For

                        End If
                    End If
                Next

            Catch ex As Exception
                Return
            Finally
                If s IsNot Nothing Then

                    s.Close()
                    s.Dispose()
                End If

            End Try
        End Sub

        Friend Shared Function StyleData(aLineType As TTABLEENTRY, Optional bRegen As Boolean = False, Optional rPatternLength As Single = 0) As TVALUES
            Dim _rVal As New TVALUES(0)
            Try
                Dim idx As Integer
                Dim aProps As TPROPERTIES
                Dim eCnt As Integer
                Dim ltname As String
                Dim conv As Single
                rPatternLength = 0
                ltname = aLineType.Name
                eCnt = aLineType.PropValueI(dxxLinetypeProperties.Elements)
                aProps = aLineType.Reactors.GetProps("ElementLengths", idx, False)
                If Not aLineType.Values.Defined Then bRegen = True
                If aProps.Count <> eCnt Or idx < 0 Or aLineType.Values.Count <> eCnt Then bRegen = True
                If bRegen Then
                    aLineType.Values = New TVALUES(0)
                    aProps = aLineType.Props.GetByGroupCode(49)
                    If ltname.Contains("ISO", StringComparer.OrdinalIgnoreCase) >= 0 Then conv = 1 / 25.4 Else conv = 1
                    aLineType.Props.SetValGC(73, aProps.Count, 1)
                    If aProps.Count > 0 Then
                        aLineType.Values = New TVALUES(aProps.Count)
                        Dim tot As Double = 0
                        Dim sval As Single
                        For i = 1 To aLineType.Values.Count
                            sval = TVALUES.To_SNG(aProps.Item(i).Value * conv)
                            tot += Math.Abs(sval)
                            aLineType.Values.SetItem(i, sval)

                        Next i
                        aLineType.Values.BaseValue = tot
                        aLineType.Props.SetValGC(40, tot, 1)
                        If idx >= 0 Then aLineType.Reactors.ClearMember("ElementLengths")
                        aLineType.Reactors.Add(aProps, "ElementLengths")
                        aLineType.Values.Defined = True
                    End If
                End If
                _rVal = aLineType.Values
                rPatternLength = TVALUES.To_SNG(_rVal.BaseValue)
                Return _rVal
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("dxfLinetype.StyleData.Error: " & ex.Message)
                Return _rVal
            End Try
        End Function
#End Region 'Shared Methods
    End Class 'dxfLinetypes
End Namespace