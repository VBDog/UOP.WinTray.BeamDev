
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoCharacters
        Inherits List(Of dxoCharacter)

#Region "Constructors"
        Public Sub New()
            Init()
        End Sub
        Friend Sub New(aChars As TCHARS)

            Init()

            For i As Integer = 1 To aChars.Count
                Add(New dxoCharacter(aChars.Item(i)))
            Next

        End Sub

        Public Sub New(aString As String, aBaseFormat As dxoCharFormat, bNoEffects As Boolean, Optional aCharBox As dxoCharBox = Nothing)
            Dim backslashes As New List(Of dxoCharacter)
            Init(aString, aBaseFormat, bNoEffects, aCharBox, backslashes)
            If Count <= 0 Or bNoEffects Then Return
            backslashes = FindAll(Function(x) x.FormatCode = dxxCharFormatCodes.Backslash)

            'Dim rFormats.Item(ci)  As dxoCharFormat = aBaseFormat

            Dim aCode As dxxCharFormatCodes = dxxCharFormatCodes.None
            Dim aStr As String = String.Empty

            Dim grpID As Integer = 0
            Dim gi As Integer = 0
            Dim uBnd As Integer = Count - 1
            Dim bFlg As Boolean = False
            Dim aDbl As Double = 0
            Dim aInt As Integer = 0

            Dim ci As Integer
            Dim iStackID As Integer = 0
            Dim lid As Integer = 1
            Dim controlChars As List(Of Char) = dxoCharacters.ControlCharacters
            Dim controlCodes As List(Of Tuple(Of Char, dxxCharFormatCodes)) = dxoCharacters.ControlCodes

            Dim stackchars As List(Of Char) = dxoCharacters.StackMarkers
            Dim stackcodes As List(Of Tuple(Of Char, dxxCharacterStackStyles)) = dxoCharacters.StackCodes

            Dim theTail As List(Of dxoCharacter) = Nothing
            Dim subChars As List(Of dxoCharacter) = Nothing


            For Each slash As dxoCharacter In backslashes
                Try
                    ci = IndexOf(slash)
                    Dim charPlus1 As dxoCharacter = GetCharacter(ci + 1)
                    If charPlus1 Is Nothing Then Exit For
                    Dim charPlus2 As dxoCharacter = GetCharacter(ci + 2)
                    Dim semicolon As dxoCharacter = Nothing

                    Dim code As Tuple(Of Char, dxxCharFormatCodes) = controlCodes.Find(Function(x) x.Item1 = charPlus1.Charr)

                    If Not charPlus1.IsFormatCode Then

                        Select Case charPlus1.Charr
                                '==========================================================
                            Case "\" 'free backslash
                                '==========================================================
                                charPlus1.FormatCode = dxxCharFormatCodes.None
                                Continue For
                            '==========================================================
                            Case "{", "}" 'free open brace or close brace
                                '==========================================================

                                charPlus1.FormatCode = dxxCharFormatCodes.None


                        End Select
                    Else


                        Select Case charPlus1.FormatCode
                            Case dxxCharFormatCodes.LineFeed
                                Try
                                    charPlus1.CharacterString = "P"  ' in case it is lower case
                                    lid += 1
                                    theTail = FindAll(Function(x) IndexOf(x) > IndexOf(charPlus1))
                                    For Each mem As dxoCharacter In theTail
                                        mem.LineNo = lid
                                    Next
                                Catch

                                End Try

                            Case dxxCharFormatCodes.UnderlineOn
                                Try

                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(charPlus1, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                    For Each mem As dxoCharacter In theTail
                                        mem.Underline = True
                                    Next
                                Catch

                                End Try

                            Case dxxCharFormatCodes.UnderlineOff
                                Try
                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(charPlus1, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                    For Each mem As dxoCharacter In theTail
                                        mem.Underline = False
                                    Next
                                Catch

                                End Try

                            Case dxxCharFormatCodes.StrikeThruOn
                                Try
                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(charPlus1, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)


                                    For Each mem As dxoCharacter In theTail
                                        mem.StrikeThru = True
                                    Next
                                Catch ex As Exception

                                End Try

                            Case dxxCharFormatCodes.StrikeThruOff
                                Try
                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(charPlus1, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)


                                    For Each mem As dxoCharacter In theTail
                                        mem.StrikeThru = False
                                    Next
                                Catch ex As Exception

                                End Try

                            Case dxxCharFormatCodes.OverlineOn
                                Try
                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(charPlus1, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)


                                    For Each mem As dxoCharacter In theTail
                                        mem.Overline = True
                                    Next
                                Catch ex As Exception

                                End Try

                            Case dxxCharFormatCodes.OverlineOff
                                Try
                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(charPlus1, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)


                                    For Each mem As dxoCharacter In theTail
                                        mem.Overline = False
                                    Next
                                Catch ex As Exception

                                End Try

                            Case dxxCharFormatCodes.Color
                                Try
                                    Dim winLong As Boolean = charPlus1 = "c"c
                                    Dim clr As dxxColors = aBaseFormat.Color
                                    Dim dbl As Double = TVALUES.To_DBL(aBaseFormat.Color)

                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then Continue For

                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat

                                    'get the color index

                                    Dim intval As Integer = TVALUES.LimitedValue(TVALUES.To_INT(dbl), 1, 255)

                                    clr = DirectCast(intval, dxxColors)
                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(semicolon, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)


                                    For Each schar As dxoCharacter In theTail
                                        schar.Color = clr
                                    Next
                                Catch ex As Exception

                                End Try

                            Case dxxCharFormatCodes.Alignment
                                Try
                                    Dim dbl As Double = 0
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)
                                    If subChars.Count <= 0 Then Continue For

                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat

                                    Dim intval As Integer = TVALUES.LimitedValue(TVALUES.To_INT(dbl), 0, 2)
                                    Dim alng As dxxCharacterAlignments = DirectCast(intval, dxxCharacterAlignments)

                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(semicolon, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                    For Each schar As dxoCharacter In theTail
                                        schar.CharAlign = alng
                                    Next
                                Catch ex As Exception

                                End Try

                            Case dxxCharFormatCodes.WidthFactor

                                Try
                                    'read forward to the semicolon
                                    Dim dbl As Double = aBaseFormat.WidthFactor
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then Continue For

                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat

                                    If dbl > 0 Then
                                        dbl = TVALUES.LimitedValue(dbl, 0.1, 10)
                                        Dim bracketgroup As Integer? = Nothing
                                        If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                        theTail = GetTrailingChars(semicolon, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                        For Each schar As dxoCharacter In theTail
                                            schar.WidthFactor = dbl
                                        Next

                                    End If
                                Catch

                                End Try

                            Case dxxCharFormatCodes.Height, dxxCharFormatCodes.HeightFactor
                                Try
                                    charPlus1.CharacterString = "H"
                                    Dim dbl As Double = 0
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=-1, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then Continue For
                                    Dim multi As Boolean = subChars.Item(subChars.Count - 2).ToUpper() = "X"
                                    If multi Then charPlus1.FormatCode = dxxCharFormatCodes.HeightFactor Else charPlus1.FormatCode = dxxCharFormatCodes.Height
                                    For Each schar As dxoCharacter In subChars
                                        schar.FormatCode = charPlus1.FormatCode
                                        schar.GroupIndex = charPlus1.GroupIndex
                                    Next
                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat

                                    If dbl > 0 Then

                                        Dim bracketgroup As Integer? = Nothing
                                        If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                        theTail = GetTrailingChars(semicolon, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                        For Each schar As dxoCharacter In theTail
                                            If multi Then
                                                schar.HeightFactor = dbl
                                                schar.WidthFactor *= dbl
                                                'Console.WriteLine($"{schar} - {schar.HeightFactor * aBaseFormat.CharHeight} ")
                                            Else
                                                schar.CharHeight = dbl
                                            End If

                                        Next

                                    End If
                                Catch

                                End Try

                            Case dxxCharFormatCodes.Font
                                Try
                                    charPlus1.CharacterString = "f"
                                    Dim dbl As Double = 0

                                    'get the font string
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=-1, aFormatCode:=charPlus1.FormatCode)
                                    If subChars.Count <= 0 Then Continue For

                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat

                                    If aStr <> "" Then
                                        Dim fInfo As TFONTSTYLEINFO = dxoFonts.GetFontStyleInfoByString(aStr)
                                        If Not fInfo.NotFound Then

                                            Dim bracketgroup As Integer? = Nothing
                                            If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                            theTail = GetTrailingChars(semicolon, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                            For Each schar As dxoCharacter In theTail
                                                schar.FontIndex = fInfo.FontIndex
                                            Next
                                        End If
                                    End If
                                Catch

                                End Try

                            Case dxxCharFormatCodes.Oblique
                                Try
                                    Dim dbl As Double = aBaseFormat.ObliqueAngle
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then Continue For

                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat

                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(semicolon, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                    dbl = TVALUES.NormAng(dbl, ThreeSixtyEqZero:=True)
                                    For Each schar As dxoCharacter In theTail
                                        schar.ObliqueAngle = dbl
                                    Next
                                Catch

                                End Try

                            Case dxxCharFormatCodes.Tracking
                                Try
                                    Dim dbl As Double = -1
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Or dbl = 0 Then Continue For

                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat

                                    dbl = TVALUES.LimitedValue(Math.Abs(dbl), 0.75, 4)
                                    Dim bracketgroup As Integer? = Nothing
                                    If charPlus1.BracketGroup > 0 Then bracketgroup = charPlus1.BracketGroup
                                    theTail = GetTrailingChars(semicolon, bReturnLeader:=False, bIsFormatCode:=False, aBracketGroup:=bracketgroup)

                                    For Each schar As dxoCharacter In theTail
                                        schar.Tracking = dbl
                                    Next

                                Catch

                                End Try

                            Case dxxCharFormatCodes.Stack
                                Try
                                    Dim dbl As Double = 0
                                    subChars = dxoCharacters.ReadToChar(Me, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then Continue For

                                    semicolon = subChars.Last()
                                    semicolon.FormatCode = dxxCharFormatCodes.EndFormat


                                    Dim stkStlye As dxxCharacterStackStyles = dxxCharacterStackStyles.None

                                    Dim sNumer As String = String.Empty
                                    Dim sDenom As String = String.Empty
                                    Dim idx As Integer = -1
                                    Dim group1 As List(Of dxoCharacter)
                                    Dim group2 As List(Of dxoCharacter)


                                    For Each stackchar In stackchars
                                        idx = subChars.FindIndex(Function(x) x = stackchar)
                                        If idx >= 0 Then
                                            Dim stackcode As Tuple(Of Char, dxxCharacterStackStyles) = stackcodes.Find(Function(x) x.Item1 = stackchar)
                                            If stackcode IsNot Nothing Then
                                                stkStlye = stackcode.Item2
                                            End If

                                            Exit For
                                        End If
                                    Next
                                    If idx >= 0 Then
                                        group1 = dxoCharacters.GetSubChars(subChars, 1, idx - 1, aFormatCode:=dxxCharFormatCodes.None)
                                        group2 = dxoCharacters.GetSubChars(subChars, idx + 1, subChars.Count - 2, aFormatCode:=dxxCharFormatCodes.None)
                                        sNumer = dxoCharacters.GetVisibleString(group1)
                                        sDenom = dxoCharacters.GetVisibleString(group2)
                                        If stkStlye = dxxCharacterStackStyles.Tolerance Then '^ marker
                                            If group1.Count = 0 And group2.Count > 0 Then
                                                stkStlye = dxxCharacterStackStyles.SubScript
                                            ElseIf group1.Count > 0 And group2.Count = 0 Then
                                                stkStlye = dxxCharacterStackStyles.SuperScript
                                            End If

                                        End If

                                        If stkStlye = dxxCharacterStackStyles.DecimalStack And group2.Count > 0 Then
                                            group2.Item(0).FormatCode = dxxCharFormatCodes.DecimalStackLeader
                                        End If


                                        If group1.Count > 0 Then
                                            iStackID += 1

                                            For Each schar As dxoCharacter In group1
                                                schar.StackStyle = stkStlye
                                                schar.StackID = iStackID
                                            Next

                                        End If
                                        If group2.Count > 0 Then
                                            iStackID += 1

                                            For Each schar As dxoCharacter In group2
                                                schar.StackStyle = stkStlye
                                                schar.StackID = iStackID
                                            Next
                                        End If

                                        Item(IndexOf(subChars.Item(idx))).FormatCode = dxxCharFormatCodes.StackBreak

                                    End If
                                Catch

                                End Try


                        End Select

                    End If
                Catch ex As Exception
                    Continue For
                End Try

            Next




            Return
        End Sub

        Private Sub Init(Optional aString As String = Nothing, Optional aBaseFormat As dxoCharFormat = Nothing, Optional bNoEffects As Boolean = True, Optional aCharBox As dxoCharBox = Nothing, Optional rBackSlashes As List(Of dxoCharacter) = Nothing)
            Clear()
            If aString Is Nothing Then Return

            If aBaseFormat Is Nothing Then aBaseFormat = New dxoCharFormat

            Dim aChars As List(Of Char) = aString.ToCharArray.ToList()
            If aChars.Count <= 0 Then Return

            Dim lastCr As String
            Dim nextCr As String
            Dim nextnextCr As String

            Dim uBnd As Integer = aChars.Count - 1

            Dim percentIDS As New List(Of Integer)
            Dim controlChars As List(Of Char) = dxoCharacters.ControlCharacters
            Dim controlCodes As List(Of Tuple(Of Char, dxxCharFormatCodes)) = dxoCharacters.ControlCodes
            Dim openbrackets As New List(Of dxoCharacter)
            Dim closebrackets As New List(Of dxoCharacter)
            Dim lastMem As dxoCharacter = Nothing
            Dim lastcode As dxxCharFormatCodes = dxxCharFormatCodes.None
            'all chrs in in this group

            For ci As Integer = 0 To uBnd
                Dim aCode As dxxCharFormatCodes = dxxCharFormatCodes.None
                If ci > 0 Then lastCr = aChars.Item(ci - 1).ToString() Else lastCr = String.Empty
                If ci + 1 <= uBnd Then nextCr = aChars.Item(ci + 1).ToString() Else nextCr = String.Empty
                If ci + 2 <= uBnd Then nextnextCr = aChars.Item(ci + 2).ToString() Else nextnextCr = String.Empty
                aCode = dxxCharFormatCodes.None
                If Not bNoEffects Then

                    'identify active back slashes
                    If aChars.Item(ci) = "\"c And nextCr <> "\" Then
                        aCode = dxxCharFormatCodes.Backslash
                    ElseIf aChars.Item(ci) = "{"c And lastcode <> dxxCharFormatCodes.Backslash And controlChars.Contains(nextnextCr) Then
                        '                                If aChars.Item(ci) = "{"c And lastMem.FormatCode <> dxxCharFormatCodes.Backslash And nextCr = "\" And nextnextCr <> "\" Then

                        aCode = dxxCharFormatCodes.OpenGroupBracket

                    ElseIf aChars.Item(ci) = "}"c And lastcode <> dxxCharFormatCodes.Backslash Then
                        'ElseIf aChars.Item(ci) = "}"c And lastMem.FormatCode <> dxxCharFormatCodes.Backslash And nextCr = "\" And nextnextCr <> "\" Then
                        aCode = dxxCharFormatCodes.CloseGroupBracket

                    End If

                End If


                Dim mem As New dxoCharacter(aChars(ci), aBaseFormat, aCharBox) With {.FormatCode = aCode, .GroupIndex = 0, .LineNo = 1}

                Add(mem)

                If mem.FormatCode = dxxCharFormatCodes.OpenGroupBracket Then
                    openbrackets.Add(mem)
                End If

                If mem.FormatCode = dxxCharFormatCodes.CloseGroupBracket Then
                    closebrackets.Add(mem)
                End If
                If Not mem.IsFormatCode And lastMem IsNot Nothing Then
                    If lastMem.FormatCode = dxxCharFormatCodes.Backslash Then
                        Dim code As Tuple(Of Char, dxxCharFormatCodes) = controlCodes.Find(Function(x) x.Item1 = mem.Charr)
                        If code IsNot Nothing Then
                            mem.FormatCode = code.Item2
                        End If
                    End If
                End If

                If mem = "%"c And nextCr = "%"c And nextnextCr <> "" Then
                    If "P,D,C".Contains(nextnextCr.ToUpper()) Then
                        percentIDS.Add(ci)
                    End If
                End If

                If rBackSlashes IsNot Nothing Then
                    If mem.FormatCode = dxxCharFormatCodes.Backslash Then
                        rBackSlashes.Add(mem)
                    End If
                End If
                lastMem = mem
                lastcode = mem.FormatCode
            Next ci




            For gi As Integer = 1 To openbrackets.Count
                Dim openbracket As dxoCharacter = openbrackets.Item(gi - 1)
                Dim iopen As Integer = IndexOf(openbracket)
                openbracket.GroupIndex = gi
                Dim brackets As List(Of dxoCharacter) = FindAll(Function(x) IndexOf(x) > iopen And (x.FormatCode = dxxCharFormatCodes.OpenGroupBracket Or x.FormatCode = dxxCharFormatCodes.CloseGroupBracket))

                Dim cnt As Integer = 0
                Dim closebracket As dxoCharacter = Nothing
                For Each bracket As dxoCharacter In brackets
                    If bracket.FormatCode = dxxCharFormatCodes.CloseGroupBracket Then cnt -= 1 Else cnt += 1
                    If cnt = -1 Then
                        closebracket = bracket
                        Exit For
                    End If
                Next

                If cnt = -1 Then
                    Dim iclose As Integer = IndexOf(closebracket)

                    For i As Integer = iopen To iclose
                        Item(i).BracketGroup = gi
                    Next i
                End If


            Next

            For Each idx As Integer In percentIDS
                Dim chr1 As dxoCharacter = Item(idx)
                Dim chr2 As dxoCharacter = Item(idx + 1)

                Dim chr3 As dxoCharacter = Item(idx + 2)
                Dim replaceidx As Integer

                Select Case chr3.Charr
                    Case "D"c, "d"c  'degree symbol
                        chr1.FormatCode = dxxCharFormatCodes.DegreeSymbol
                        replaceidx = 176
                    Case "P"c, "p"c
                        chr1.FormatCode = dxxCharFormatCodes.PlusMinusSymbol
                        replaceidx = 177
                    Case "C"c, "c"c
                        chr1.FormatCode = dxxCharFormatCodes.DiameterSymbol
                        replaceidx = 248
                End Select
                chr2.FormatCode = chr1.FormatCode
                chr3.ReplacedChar = Chr(replaceidx)
            Next

        End Sub

#End Region 'Constructors

#Region "Properties"




        Public Function CharacterString(Optional aChars As List(Of dxoCharacter) = Nothing) As String

            Dim vishchars As List(Of dxoCharacter) = aChars

            If vishchars Is Nothing Then vishchars = VisibleCharacters()
            Dim _rVal As String = String.Empty
            For Each mem As dxoCharacter In vishchars
                _rVal += mem.CharacterString
            Next
            Return _rVal
        End Function



        Public Property LastChar As dxoCharacter
            Get
                Return Character(CharacterCount)
            End Get
            Set(value As dxoCharacter)
                If CharacterCount > 0 Then SetChar(CharacterCount, value)
            End Set
        End Property

        Public ReadOnly Property CharacterCount As Integer
            '^returns the number of visible chracters in the string
            Get
                Return VisibleCharacters.Count
            End Get
        End Property

        Public ReadOnly Property FirstChar As dxoCharacter
            Get

                Return Character(1)
            End Get
        End Property


        Public ReadOnly Property CodeCount As Integer
            Get
                Return FormatCharacters.Count
            End Get
        End Property



#End Region 'Properties

#Region "Methods"

        Public Function VisibleCharacters() As List(Of dxoCharacter)
            Return FindAll(Function(x) Not x.IsFormatCode)
        End Function

        Public Function GetVisibleCharacters(aCharBoxToAlignTo As dxoCharBox, bVerticalAlignment As Boolean, bSetCharIndexes As Boolean, ByRef rStacks As Boolean, ByRef rStackedChars As List(Of List(Of dxoCharacter))) As List(Of dxoCharacter)
            rStacks = False
            rStackedChars = New List(Of List(Of dxoCharacter))
            If aCharBoxToAlignTo Is Nothing Then
                aCharBoxToAlignTo = New dxoCharBox()
                'Else
                '    _CharBox.CopyDirections(aCharBoxToAlignTo, True)
            End If

            Dim myBox As dxoCharBox = aCharBoxToAlignTo
            Dim _rVal As New List(Of dxoCharacter)
            Dim basept As TVECTOR = myBox.BasePtV
            Dim lineidx As Integer = 1

            Dim vLine As New TLINE(aOrigin:=myBox.BasePtV, aDirection:=myBox.YDirectionV, 10)
            Dim stackon As Boolean

            Dim stackchars As List(Of dxoCharacter) = Nothing
            Dim curstack As Integer = 0

            For Each mem As dxoCharacter In Me
                mem.Vertical = bVerticalAlignment
                If mem.IsFormatCode Then
                    mem.MoveFromTo(mem.CharBox.EndPtV, myBox.BasePtV)
                    If Not bVerticalAlignment Then
                        If mem.FormatCode = dxxCharFormatCodes.Stack Then
                            rStacks = True
                        ElseIf mem.FormatCode = dxxCharFormatCodes.EndStack Then
                            stackon = False
                        ElseIf mem.FormatCode = dxxCharFormatCodes.StackBreak Then
                            stackon = False

                        End If
                    End If
                Else
                    If bSetCharIndexes Then mem.LineIndex = lineidx
                    Dim v1 As TVECTOR = mem.StartPtV
                    Dim v2 As TVECTOR = basept
                    If Not bVerticalAlignment Then
                        If lineidx = 1 Then
                            Dim blimits As TLIMITS = mem.ExtentPts.Limits()
                            ' v1 += myBox.XDirectionV * blimits.Left
                        End If
                    End If

                    mem.MoveFromTo(v1, v2)
                    _rVal.Add(mem)
                    basept = mem.EndPtV
                    lineidx += 1

                    If Not bVerticalAlignment Then
                        If mem.StackID > 0 Then
                            If mem.StackID > curstack Then
                                stackchars = New List(Of dxoCharacter)()
                                rStackedChars.Add(stackchars)
                            End If
                            curstack = mem.StackID

                            stackchars.Add(mem)
                            ' Debug.Print($"{mem.CharacterString} - STACK - { mem.StackID}")
                        End If
                    End If


                End If
            Next


            Return _rVal
        End Function

        Public Function GetTrailingChars(aChar As dxoCharacter, Optional bReturnLeader As Boolean = False, Optional bIsFormatCode As Boolean? = Nothing, Optional aBracketGroup As Integer? = Nothing) As List(Of dxoCharacter)

            Dim idx As Integer = IndexOf(aChar)
            If idx < 0 Then Return New List(Of dxoCharacter)()
            Dim _rVal As New List(Of dxoCharacter)()

            If bReturnLeader Then _rVal.Add(aChar)
            If Not bIsFormatCode.HasValue And Not aBracketGroup.HasValue Then
                _rVal.AddRange(FindAll(Function(x) IndexOf(x) > idx))
            ElseIf bIsFormatCode.HasValue And aBracketGroup.HasValue Then
                _rVal.AddRange(FindAll(Function(x) IndexOf(x) > idx And x.IsFormatCode = bIsFormatCode.Value And x.BracketGroup = aBracketGroup.Value))
            ElseIf bIsFormatCode.HasValue Then
                _rVal.AddRange(FindAll(Function(x) IndexOf(x) > idx And x.IsFormatCode = bIsFormatCode.Value))
            ElseIf aBracketGroup.HasValue Then
                _rVal.AddRange(FindAll(Function(x) IndexOf(x) > idx And x.BracketGroup = aBracketGroup.Value))
            End If


            Return _rVal
        End Function


        Public Function FormatCharacters() As List(Of dxoCharacter)
            Return FindAll(Function(x) x.IsFormatCode)
        End Function
        Public Function Character(aIndex As Integer) As dxoCharacter
            '^gets or sets a visible character
            '~loop 1 to CharacterCount to retrieve sequentially
            Dim vChars As List(Of dxoCharacter) = VisibleCharacters()
            If aIndex < 1 Or aIndex > vChars.Count Then Return Nothing

            Dim aChar As dxoCharacter = vChars.Item(aIndex - 1)

            aChar.Index = aIndex
            Return aChar
        End Function



        Public Sub SetChar(aIndex As Integer, value As dxoCharacter)
            Dim aChar As dxoCharacter = Character(aIndex)

            If aChar Is Nothing Or value Is Nothing Then Return
            aChar.Index = aIndex
            MyBase.Item(aChar.Index - 1) = aChar

        End Sub

        Public Overloads Sub Clear()
            MyBase.Clear()

        End Sub

        Friend Sub AddV(aChar As TCHAR)

            Add(New dxoCharacter(aChar))
        End Sub

        Public Overloads Sub Add(aChar As dxoCharacter)
            If aChar Is Nothing Then Return

            If aChar.IsFormatCode Then
                aChar.Key = $"CODE:{CodeCount + 1}"
            Else
                aChar.Index = CharacterCount + 1
                aChar.Key = aChar.Index.ToString()
            End If

            MyBase.Add(aChar)
        End Sub


        Public Function Clone() As dxoCharacters
            Dim _rVal As New dxoCharacters()
            For Each mem As dxoCharacter In Me
                _rVal.Add(mem.Clone())
            Next
            Return _rVal
        End Function

        Friend Function CopyDirections(aCharBox As dxoCharBox) As Boolean
            Dim _rVal As Boolean
            For Each mem As dxoCharacter In Me
                If mem.CharBox.CopyDirections(aCharBox) Then _rVal = True
            Next
            Return _rVal
        End Function


        Friend Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return

            For Each mem As dxoCharacter In Me
                mem.Translate(aTranslation)
            Next

        End Sub

        Friend Sub Rescale(aScaleFactor As Double, aRefPt As TVECTOR)

            For Each mem As dxoCharacter In Me
                mem.Rescale(aScaleFactor, aRefPt)
            Next

        End Sub

        Friend Sub RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False)

            For Each mem As dxoCharacter In Me
                mem.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
            Next

        End Sub

        Friend Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, bMirrorDirections As Boolean, Optional bSuppressCheck As Boolean = False)

            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP,aEP) < 0.00001 Then Return
            End If

            For Each mem As dxoCharacter In Me
                mem.Mirror(aSP, aEP, bMirrorDirections, True)
            Next
        End Sub

        Public Function GetCharacter(aIndex As Integer, Optional bBaseOne As Boolean = False) As dxoCharacter
            If Not bBaseOne Then
                If aIndex < 0 Or aIndex > Count - 1 Then Return Nothing
                Return MyBase.Item(aIndex)
            Else
                If aIndex < 1 Or aIndex > Count Then
                    If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                    Return Nothing
                End If
                Return MyBase.Item(aIndex - 1)
            End If
        End Function

        Friend Function ExtentRectangle(aBasePlane As TPLANE) As dxfRectangle
            Dim extpts As TVECTORS = ExtentPts()
            Return New dxfRectangle(extpts.BoundingRectangle(aBasePlane, bSuppressProjection:=True))
        End Function


        Friend Function ExtentPts(Optional aVisChars As List(Of dxoCharacter) = Nothing, Optional aSkipChars As List(Of dxoCharacter) = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)

            If aVisChars Is Nothing Then aVisChars = VisibleCharacters()
            For Each mem As dxoCharacter In aVisChars
                If aSkipChars IsNot Nothing Then
                    If aSkipChars.IndexOf(mem) >= 0 Then Continue For
                End If
                _rVal.Append(mem.ExtentPointsV())

            Next

            Return _rVal
        End Function

        Friend Function ExtentPoints(Optional aVisChars As List(Of dxoCharacter) = Nothing, Optional aSkipChars As List(Of dxoCharacter) = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors()

            If aVisChars Is Nothing Then aVisChars = VisibleCharacters()
            For Each mem As dxoCharacter In aVisChars
                If aSkipChars IsNot Nothing Then
                    If aSkipChars.IndexOf(mem) >= 0 Then Continue For
                End If
                _rVal.Append(mem.ExtentPts.ToPlaneVectorsV(mem.PlaneV), aTag:=$"{IndexOf(mem) + 1}")

            Next

            Return _rVal
        End Function
#End Region 'Methods

#Region "Shared Methods"

        Public Const FormatChars As String = "A,C,c,F,f,T,Q,W,H,O,o,L,l,S,K,k,~"

        Friend Shared Function CreateStringChars(aString As String, aBaseFormat As dxoCharFormat, bNoEffects As Boolean) As List(Of dxoCharacter)

            Dim _rVal As New List(Of dxoCharacter)
            If String.IsNullOrEmpty(aString) Then Return _rVal


            Dim aChars() As Char = aString.ToCharArray
            If aChars.Length <= 0 Then Return _rVal

            'Dim rFormats.Item(ci)  As dxoCharFormat = aBaseFormat
            Dim fInfo As TFONTSTYLEINFO = Nothing
            Dim aCode As dxxCharFormatCodes = dxxCharFormatCodes.None
            Dim aStr As String = String.Empty
            Dim k As Integer
            Dim j As Integer
            Dim curCr As Char
            Dim lastCr As Char
            Dim nextCr As Char
            Dim nextnextCr As Char
            Dim grpID As Integer = 0
            Dim gi As Integer = 0

            Dim uBnd As Integer = aChars.Length - 1
            Dim bFlg As Boolean = False
            Dim aDbl As Double = 0
            Dim aInt As Integer = 0

            Dim iGoupCount As Integer = 0
            Dim grpProp As String = String.Empty
            Dim grpInfos As New List(Of TCHARGROUPINFO)
            Dim grpInfo As TCHARGROUPINFO
            Dim si As Integer
            Dim aChar As TCHAR
            Dim iStackID As Integer = 0
            Dim lid As Integer = 1


            'all chrs in in this group
            grpInfos.Add(New TCHARGROUPINFO(0, 0, uBnd))
            For ci As Integer = 0 To uBnd
                curCr = aChars(ci)
                If ci > 0 Then lastCr = aChars(ci - 1).ToString Else lastCr = ""
                If ci + 1 <= uBnd Then nextCr = aChars(ci + 1).ToString Else nextCr = ""
                If ci + 2 <= uBnd Then nextnextCr = aChars(ci + 2).ToString Else nextnextCr = ""
                aCode = dxxCharFormatCodes.None
                If Not bNoEffects Then
                    'identify groupings
                    If curCr = "{" And lastCr <> "\" And nextCr = "\" Then
                        If FormatChars.IndexOf(nextnextCr) >= 0 Then
                            iGoupCount += 1
                            gi += 1
                            k = 1
                            aCode = dxxCharFormatCodes.OpenGroupBracket
                            grpInfo = New TCHARGROUPINFO(iGoupCount, ci, ci)
                            For j = ci + 1 To uBnd
                                If aChars(j) = "{" And aChars(j - 1) <> "\" Then k += 1
                                If (aChars(j) = "}" And aChars(j - 1) <> "\") Or j = uBnd Then
                                    k -= 1
                                    If k = 0 Or j = uBnd Then : grpInfo.EndIndex = j : Exit For : End If
                                End If
                            Next j
                            grpInfos.Add(grpInfo)
                        End If
                    ElseIf curCr = "}" And lastCr <> "\" Then
                        aCode = dxxCharFormatCodes.CloseGroupBracket
                        gi -= 1
                    End If
                End If




                Dim schar As New dxoCharacter(curCr, aBaseFormat) With {.FormatCode = aCode, .GroupIndex = gi}
                _rVal.Add(schar)
            Next ci

            gi = 0
            uBnd = _rVal.Count - 1
            Dim stackchars As New List(Of Char)({"^"c, "#"c, "/"c, "~"c})

            For ci As Integer = 0 To _rVal.Count - 1
                Dim curChar As dxoCharacter = _rVal.Item(ci)
                curChar.Index = ci
                Dim charPlus1 As dxoCharacter = Nothing
                Dim charPlus2 As dxoCharacter = Nothing
                Dim dbl As Double = 0
                Dim subChars As List(Of dxoCharacter) = Nothing
                Dim semicolon As Integer
                Dim theTail As List(Of dxoCharacter)
                nextCr = ""
                nextnextCr = ""
                aChar = New TCHAR(curChar)
                curChar.LineNo = lid
                curCr = curChar.Charr
                If ci > 0 Then lastCr = _rVal.Item(ci - 1).Charr Else lastCr = ""
                If ci + 1 <= uBnd Then
                    charPlus1 = _rVal.Item(ci + 1)
                    nextCr = charPlus1.Charr

                End If
                If ci + 2 <= uBnd Then
                    charPlus2 = _rVal.Item(ci + 2)
                    nextnextCr = charPlus2.Charr

                End If



                Select Case curChar.Charr
                    Case "\"  'BACKSLASH FOUND
                        If bNoEffects Or charPlus1 Is Nothing Then Continue For

                        If Not "A,C,c,F,f,T,Q,W,H,O,o,L,l,S,K,k,~".Contains(charPlus1.Charr) Then

                            Select Case charPlus1.Charr
                                '==========================================================
                                Case "\" 'free backslash
                                    '==========================================================
                                    curChar.FormatCode = dxxCharFormatCodes.Backslash
                                    charPlus1.FormatCode = dxxCharFormatCodes.None
                                    ci += 1 : Continue For
                                    '==========================================================
                                Case "{", "}" 'free open brace or close brace
                                    '==========================================================
                                    curChar.FormatCode = dxxCharFormatCodes.Backslash
                                    charPlus1.FormatCode = dxxCharFormatCodes.None
                                    ci += 1 : Continue For
                                '==========================================================
                                Case "P", "p" 'NEW LINE
                                    '============================================================

                                    curChar.FormatCode = dxxCharFormatCodes.Backslash
                                    charPlus1.FormatCode = dxxCharFormatCodes.LineFeed
                                    lid += 1 : grpID = 0
                                    If charPlus1 = "p" Then
                                        curChar.CharacterString = "P"
                                    End If
                                    ci += 1 : Continue For

                            End Select
                        Else
                            'known format control codes A,C,c,F,f,T,Q,W,H,O,o,L,l,S,K,k,~
                            curChar.FormatCode = dxxCharFormatCodes.Backslash

                            'find the applicble group
                            gi = 0
                            For j = grpInfos.Count To 1 Step -1
                                If grpInfos.Item(j - 1).Contains(ci) Then
                                    gi = grpInfos.Item(j - 1).Index
                                    Exit For
                                End If
                            Next j
                            grpInfo = grpInfos.Item(gi)
                            grpProp = ""
                        End If

                        Dim iMark As Integer = 0
                        si = ci



                        Select Case charPlus1.Charr

                                    '================================================
                            Case "L", "l"
                                '================================================
                                grpProp = "UNDERLINE"

                                If charPlus1 = "L"c Then charPlus1.FormatCode = dxxCharFormatCodes.UnderlineOn Else charPlus1.FormatCode = dxxCharFormatCodes.UnderlineOff
                                ci += 1 : Continue For
                                   '================================================
                            Case "O", "o"
                                '================================================
                                grpProp = "OVERLINE"

                                If charPlus1 = "O"c Then charPlus1.FormatCode = dxxCharFormatCodes.OverlineOn Else charPlus1.FormatCode = dxxCharFormatCodes.OverlineOff
                                ci += 1 : Continue For

                                '================================================
                            Case "K", "k"
                                '================================================
                                grpProp = "STRIKETHRU"
                                If charPlus1 = "K"c Then charPlus1.FormatCode = dxxCharFormatCodes.StrikeThruOn Else charPlus1.FormatCode = dxxCharFormatCodes.StrikeThruOff
                                ci += 1 : Continue For

                                    '================================================
                            Case "C", "c"    'COLOR
                                '================================================
                                grpID += 1 : curChar.GroupIndex = grpID
                                Dim winLong As Boolean = charPlus1 = "c"
                                charPlus1.GroupIndex = grpID
                                charPlus1.FormatCode = dxxCharFormatCodes.Color
                                Dim clr As dxxColors = aBaseFormat.Color
                                dbl = TVALUES.To_DBL(aBaseFormat.Color)

                                'read forward to the semicolon
                                subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                If subChars.Count <= 0 Then
                                    ci += 1 : Continue For
                                End If
                                semicolon = _rVal.IndexOf(subChars.Last())
                                _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat
                                'get the color index
                                grpProp = "COLOR"
                                Dim intval As Integer = TVALUES.To_INT(dbl)
                                If intval > 0 And intval < 256 Then

                                    clr = DirectCast(intval, dxxColors)
                                    theTail = _rVal.FindAll(Function(x) _rVal.IndexOf(x) > semicolon)
                                    For Each schar As dxoCharacter In theTail
                                        schar.Color = clr
                                    Next

                                End If
                                ci = semicolon : Continue For
                            '================================================
                            Case "F", "f" 'FONT
                                '================================================
                                charPlus1.FormatCode = dxxCharFormatCodes.Font

                                'get the font string
                                'read forward to the semicolon
                                subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=0, aFormatCode:=charPlus1.FormatCode)
                                If subChars.Count <= 0 Then
                                    ci += 1 : Continue For
                                End If
                                semicolon = _rVal.IndexOf(subChars.Last())
                                _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat

                                If aStr <> "" Then
                                    fInfo = dxoFonts.GetFontStyleInfoByString(aStr)
                                    If Not fInfo.NotFound Then
                                        grpProp = "FONT"
                                        theTail = _rVal.FindAll(Function(x) _rVal.IndexOf(x) > semicolon)
                                        For Each schar As dxoCharacter In theTail
                                            schar.FontIndex = fInfo.FontIndex
                                        Next
                                    End If
                                End If
                                ci = semicolon : Continue For
                                    '================================================
                            Case "Q"  'OBLIQUE_ANGLE
                                '================================================
                                grpID += 1
                                curChar.FormatCode = dxxCharFormatCodes.Oblique
                                charPlus1.FormatCode = curChar.FormatCode
                                dbl = aBaseFormat.ObliqueAngle
                                'read forward to the semicolon
                                subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                If subChars.Count <= 0 Then
                                    ci += 1 : Continue For
                                End If
                                semicolon = _rVal.IndexOf(subChars.Last())
                                _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat
                                'get the color index
                                grpProp = "OBLIQUE"

                                theTail = _rVal.FindAll(Function(x) _rVal.IndexOf(x) > semicolon)
                                dbl = TVALUES.NormAng(dbl, ThreeSixtyEqZero:=True)
                                For Each schar As dxoCharacter In theTail
                                    schar.ObliqueAngle = dbl
                                    schar.GroupIndex = grpID
                                Next
                                ci = semicolon : Continue For
               '==========================================================
                            Case "S" 'Stacking
                                '============================================================
                                If Not bNoEffects Then
                                    grpID += 1
                                    charPlus1.FormatCode = dxxCharFormatCodes.Stack
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=dbl, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then
                                        ci += 1 : Continue For
                                    End If
                                    semicolon = _rVal.IndexOf(subChars.Last())
                                    _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat


                                    Dim stkStlye As dxxCharacterStackStyles = dxxCharacterStackStyles.None

                                    Dim sNumer As String = String.Empty
                                    Dim sDenom As String = String.Empty
                                    Dim idx As Integer = -1
                                    Dim group1 As List(Of dxoCharacter)
                                    Dim group2 As List(Of dxoCharacter)

                                    For Each stackchar In stackchars
                                        idx = subChars.FindIndex(Function(x) x = stackchar)
                                        If idx >= 0 Then
                                            Select Case stackchar
                                                Case "^"c
                                                    'tolerance
                                                    stkStlye = dxxCharacterStackStyles.Tolerance
                                                Case "#"c
                                                    stkStlye = dxxCharacterStackStyles.Diagonal
                                                Case "/"c
                                                    stkStlye = dxxCharacterStackStyles.Horizontal
                                                Case "~"c
                                                    stkStlye = dxxCharacterStackStyles.DecimalStack

                                            End Select
                                            Exit For
                                        End If
                                    Next
                                    If idx >= 0 Then
                                        _rVal.Item(_rVal.IndexOf(subChars.Item(idx))).FormatCode = dxxCharFormatCodes.StackBreak
                                        group1 = dxoCharacters.GetSubChars(subChars, 0, idx - 1)
                                        group2 = dxoCharacters.GetSubChars(subChars, idx + 1, semicolon - 1)
                                        sNumer = dxoCharacters.GetVisibleString(group1)
                                        sDenom = dxoCharacters.GetVisibleString(group2)
                                        If group1.Count > 0 Then
                                            iStackID += 1
                                            iGoupCount += 1
                                            For Each schar As dxoCharacter In group1
                                                schar.StackStyle = stkStlye
                                                schar.StackID = iStackID
                                                schar.GroupIndex = iGoupCount
                                                schar.FormatCode = dxxCharFormatCodes.None
                                            Next

                                        End If
                                        If group2.Count > 0 Then
                                            iStackID += 1
                                            iGoupCount += 1
                                            For Each schar As dxoCharacter In group2
                                                schar.StackStyle = stkStlye
                                                schar.StackID = iStackID
                                                schar.GroupIndex = iGoupCount
                                                schar.FormatCode = dxxCharFormatCodes.None
                                            Next

                                        End If
                                    End If
                                    ci = semicolon : Continue For

                                End If

                                    '==========================================================
                            Case "A" 'character alignment
                                '============================================================
                                If Not bNoEffects Then
                                    charPlus1.FormatCode = dxxCharFormatCodes.Alignment
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=-1, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then
                                        ci += 1 : Continue For
                                    End If
                                    semicolon = _rVal.IndexOf(subChars.Last())
                                    _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat
                                    dbl = TVALUES.LimitedValue(dbl, 0, 1, 0)


                                    Dim almcode As dxxCharacterAlignments = DirectCast(TVALUES.To_INT(dbl), dxxCharacterAlignments)
                                    theTail = _rVal.FindAll(Function(x) _rVal.IndexOf(x) > semicolon)

                                    For Each schar As dxoCharacter In theTail
                                        schar.CharAlign = almcode
                                    Next
                                    grpProp = "CHARALIGN"
                                    ci = semicolon : Continue For
                                End If


                                    '==========================================================
                            Case "H" 'char height change
                                '============================================================
                                If Not bNoEffects Then
                                    charPlus1.FormatCode = dxxCharFormatCodes.Alignment
                                    charPlus1 = _rVal.Item(ci + 1)
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=-1, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then
                                        ci += 1 : Continue For
                                    End If
                                    semicolon = _rVal.IndexOf(subChars.Last())
                                    _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat

                                    If dbl > 0 Then

                                        theTail = _rVal.FindAll(Function(x) _rVal.IndexOf(x) > semicolon)
                                        Dim multi As Boolean = subChars.Item(subChars.Count - 2).ToUpper() = "X"
                                        For Each schar As dxoCharacter In theTail
                                            If multi Then schar.HeightFactor = dbl Else schar.CharHeight = dbl

                                        Next
                                        grpProp = "CHARHEIGHT"
                                    End If
                                    ci = semicolon : Continue For


                                End If


                                    '==========================================================
                            Case "W" 'width factor change
                                '============================================================
                                If Not bNoEffects Then

                                    charPlus1.FormatCode = dxxCharFormatCodes.WidthFactor
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=-1, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then
                                        ci += 1 : Continue For
                                    End If
                                    semicolon = _rVal.IndexOf(subChars.Last())
                                    _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat

                                    If dbl > 0 Then
                                        dbl = TVALUES.LimitedValue(dbl, 0.1, 10)
                                        theTail = _rVal.FindAll(Function(x) _rVal.IndexOf(x) > semicolon)

                                        For Each schar As dxoCharacter In theTail

                                            schar.WidthFactor = dbl
                                        Next
                                        grpProp = "WIDTH FACTOR"
                                    End If
                                    ci = semicolon : Continue For


                                End If

                                     '==========================================================
                            Case "T" 'tracking change
                                '============================================================
                                If Not bNoEffects Then

                                    charPlus1.FormatCode = dxxCharFormatCodes.Tracking
                                    'read forward to the semicolon
                                    subChars = dxoCharacters.ReadToChar(_rVal, charPlus1, ";", rFullString:=aStr, rNumber:=dbl, aDefaultNum:=-1, aFormatCode:=charPlus1.FormatCode)

                                    If subChars.Count <= 0 Then
                                        ci += 1 : Continue For
                                    End If
                                    semicolon = _rVal.IndexOf(subChars.Last())
                                    _rVal.Item(semicolon).FormatCode = dxxCharFormatCodes.EndFormat

                                    If dbl <> 0 Then

                                        dbl = TVALUES.LimitedValue(Math.Abs(dbl), 0.75, 4)
                                        theTail = _rVal.FindAll(Function(x) _rVal.IndexOf(x) > semicolon)

                                        For Each schar As dxoCharacter In theTail

                                            schar.Tracking = dbl
                                        Next
                                        grpProp = "TRACKING"
                                    End If
                                    ci = semicolon : Continue For
                                End If


                        End Select


                        grpProp = ""



                End Select
                If ci + 1 > uBnd Then Exit For
            Next ci

            Return _rVal
        End Function

        Public Shared Function CharBounds(aChars As List(Of dxoCharacter), Optional aSkipChars As List(Of dxoCharacter) = Nothing, Optional bCharBoxBounds As Boolean = False) As dxfRectangle

            Dim _rVal As New dxfRectangle()

            If aChars Is Nothing Then Return _rVal
            If aChars.Count <= 0 Then Return _rVal
            Dim expts As New TVECTORS(0)
            Dim cplane As TPLANE = aChars.Item(0).PlaneV
            For Each schar As dxoCharacter In aChars
                If aSkipChars IsNot Nothing Then
                    If aSkipChars.Contains(schar) Then Continue For
                End If
                If Not bCharBoxBounds Then
                    expts.Append(schar.ExtentPointsV)
                Else
                    expts.Append(schar.CharBox.CornersV)
                End If

            Next

            _rVal.Strukture = expts.BoundingRectangle(cplane, bSuppressProjection:=True)  'they should all lie on the same plane here
            Return _rVal
        End Function

        Public Shared Function GetSubString(aChars As List(Of dxoCharacter), aStartID As Integer, aEndID As Integer) As String
            Dim _rVal As String = String.Empty
            If aChars Is Nothing Then Return _rVal

            For i As Integer = aStartID To aEndID
                If i >= 0 And i <= aChars.Count - 1 Then
                    _rVal += aChars.Item(i).Charr
                End If
            Next

            Return _rVal
        End Function

        Public Shared Function GetSubChars(aChars As List(Of dxoCharacter), aStartID As Integer, aEndID As Integer, Optional bSkipFormats As Boolean = False, Optional aFormatCode As dxxCharFormatCodes? = Nothing) As List(Of dxoCharacter)
            Dim _rVal As New List(Of dxoCharacter)()
            If aChars Is Nothing Then Return _rVal

            For i As Integer = aStartID To aEndID
                If i >= 0 And i <= aChars.Count - 1 Then
                    If bSkipFormats And aChars.Item(i).IsFormatCode Then Continue For
                    If aFormatCode.HasValue Then aChars.Item(i).FormatCode = aFormatCode.Value
                    _rVal.Add(aChars.Item(i))
                End If
            Next

            Return _rVal
        End Function

        Public Shared Function ReadToChar(aChars As List(Of dxoCharacter), aStartChar As dxoCharacter, aEndChar As Char?, ByRef rFullString As String, ByRef rNumber As Double, Optional bSkipFormats As Boolean = False, Optional aDefaultNum As Double = 0, Optional aFormatCode As dxxCharFormatCodes? = Nothing) As List(Of dxoCharacter)
            rFullString = ""
            rNumber = aDefaultNum

            Dim _rVal As New List(Of dxoCharacter)
            If aChars Is Nothing Then Return _rVal
            If aChars.Count <= 0 Then Return _rVal
            If aEndChar Is Nothing Then Return _rVal
            Dim si As Integer = 0
            If aStartChar IsNot Nothing Then si = aChars.IndexOf(aStartChar)
            If si < 0 Then Return _rVal

            Dim ei As Integer = aChars.FindIndex(Function(x) aChars.IndexOf(x) >= si And x.Charr.Equals(aEndChar))
            If ei < 0 Then Return _rVal

            Dim numstr As String = String.Empty

            Dim stopnum As Boolean = False
            Dim innum As Boolean = False
            For i As Integer = si To ei
                Dim schar As dxoCharacter = aChars.Item(i)
                If schar Is Nothing Then Continue For
                If bSkipFormats And schar.IsFormatCode Then Continue For
                If aFormatCode.HasValue Then schar.FormatCode = aFormatCode.Value
                _rVal.Add(schar)
                Dim flag As Boolean = False

                rFullString += schar.Charr
                If stopnum Then Continue For

                If schar.IsDigit Then
                    innum = True
                    numstr += schar.Charr
                ElseIf schar = "." Then
                    If Not flag Then
                        numstr += schar.Charr
                        flag = True
                        innum = True
                    Else
                        stopnum = True
                    End If
                Else
                    If innum Then stopnum = True
                End If


            Next

            If numstr <> String.Empty Then
                If numstr.EndsWith(".") Then
                    numstr = numstr.Substring(0, numstr.Length - 1)
                End If
                If numstr.StartsWith(".") Then
                    numstr = $"0{numstr}"
                End If
                If TVALUES.IsNumber(numstr) Then
                    rNumber = TVALUES.To_DBL(numstr, aDefault:=aDefaultNum)
                End If
            End If


            Return _rVal
        End Function

        Public Shared Function GetTextRectangle(aChars As List(Of dxoCharacter), Optional aBasePt As dxfVector = Nothing, Optional aSkipChars As List(Of dxoCharacter) = Nothing, Optional bForDText As Boolean = False) As dxoCharBox

            If aChars Is Nothing Then Return New dxoCharBox()
            If aChars.Count <= 0 Then Return New dxoCharBox()

            Dim c1 As dxoCharacter = aChars.Item(0)
            Dim _rVal As dxoCharBox = New dxoCharBox(c1.CharBox) With {.Width = 0, .Descent = 0}
            Dim pln As TPLANE = c1.PlaneV
            Dim charidx As Integer = 0
            Dim baseascent As Double = c1.Ascent
            Dim basedescent As Double = c1.Descent
            Dim viscount As Integer = aChars.FindAll(Function(x) Not x.IsFormatCode).Count
            For i As Integer = 0 To aChars.Count - 1
                Dim schar As dxoCharacter = aChars.Item(i)

                If schar.IsFormatCode Then Continue For


                Dim expts As TVECTORS = schar.ExtentPointsV()
                Dim v1 As TVECTOR

                charidx += 1
                If charidx = 1 Then
                    _rVal = New dxoCharBox(schar.CharBox) With {.Width = 0, .Descent = 0, .Ascent = 0}
                    If aBasePt IsNot Nothing Then _rVal.BasePtV = aBasePt.Strukture

                    'baseascent = schar.Ascent
                    'basedescent = schar.Descent
                    If bForDText Then
                        expts = schar.CharBox.CornersV
                        '_rVal.BasePtV = schar.CharBox.BasePtV
                        'expts.Add(schar.CharBox.BasePtV)
                    End If
                    pln = _rVal.PlaneV(True)
                End If
                'If charidx = viscount Then
                '    expts.Add(schar.CharBox.EndPtV)
                'End If

                For j As Integer = 1 To expts.Count
                    v1 = expts.Item(j).WithRespectTo(pln)
                    If v1.Y < 0 Then
                        If Math.Abs(v1.Y) > _rVal.Descent Then _rVal.Descent = Math.Abs(v1.Y)
                    End If
                    If v1.X > _rVal.Width Then _rVal.Width = v1.X
                    If aSkipChars IsNot Nothing Then
                        If aSkipChars.Contains(schar) Then Continue For
                    End If
                    If v1.Y > _rVal.Ascent Then _rVal.Ascent = v1.Y
                Next
            Next
            If bForDText Then
                If _rVal.Ascent < baseascent Then _rVal.Ascent = baseascent
                If _rVal.Descent < basedescent Then _rVal.Descent = basedescent
            End If
            Return _rVal
        End Function

        Public Shared Function GetVisibleString(aChars As List(Of dxoCharacter)) As String
            Dim _rVal As String = String.Empty
            If aChars Is Nothing Then Return _rVal
            If aChars.Count <= 0 Then Return _rVal
            For Each mem As dxoCharacter In aChars
                If Not mem.IsFormatCode Then _rVal += mem.CharacterString
            Next
            Return _rVal
        End Function


        Friend Shared Sub MoveChars(aChars As List(Of dxoCharacter), aTranslation As TVECTOR)
            If aChars Is Nothing Then Return
            For Each schar As dxoCharacter In aChars
                schar.Translate(aTranslation)
            Next

        End Sub

        Public Shared ReadOnly Property ControlCharacters As List(Of Char)
            Get
                Return New List(Of Char)({"A"c, "C"c, "c"c, "F"c, "f"c, "T"c, "Q"c, "W"c, "H"c, "O"c, "o"c, "L"c, "l"c, "S"c, "K"c, "k"c, "P"c, "p", "~"c})
            End Get
        End Property

        Public Shared ReadOnly Property StackMarkers As List(Of Char)
            Get
                Return New List(Of Char)({"^"c, "#"c, "/"c, "~"c})
            End Get
        End Property
        Friend Shared ReadOnly Property StackCodes As List(Of Tuple(Of Char, dxxCharacterStackStyles))
            Get
                Dim controlChars As List(Of Char) = StackMarkers
                Dim _rVal As New List(Of Tuple(Of Char, dxxCharacterStackStyles))
                For Each controlChar As Char In controlChars
                    Select Case controlChar
                        Case "/"c
                            _rVal.Add(New Tuple(Of Char, dxxCharacterStackStyles)(controlChar, dxxCharacterStackStyles.Horizontal))
                        Case "#"c
                            _rVal.Add(New Tuple(Of Char, dxxCharacterStackStyles)(controlChar, dxxCharacterStackStyles.Diagonal))
                        Case "^"c
                            _rVal.Add(New Tuple(Of Char, dxxCharacterStackStyles)(controlChar, dxxCharacterStackStyles.Tolerance))
                        Case "~"c
                            _rVal.Add(New Tuple(Of Char, dxxCharacterStackStyles)(controlChar, dxxCharacterStackStyles.DecimalStack))


                    End Select
                Next


                Return _rVal
            End Get
        End Property

        Friend Shared ReadOnly Property ControlCodes As List(Of Tuple(Of Char, dxxCharFormatCodes))
            Get
                Dim controlChars As List(Of Char) = ControlCharacters
                Dim _rVal As New List(Of Tuple(Of Char, dxxCharFormatCodes))
                For Each controlChar As Char In controlChars
                    Select Case controlChar
                            '================================================
                        Case "L"c
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.UnderlineOn))

                           '================================================
                        Case "l"c
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.UnderlineOff))
               '================================================
                        Case "O"c
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.OverlineOn))

                                      '================================================
                        Case "o"c
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.OverlineOff))


                                '================================================
                        Case "K"c
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.StrikeThruOn))

                        '================================================
                        Case "k"c
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.StrikeThruOff))
                                     '================================================
                        Case "P"c, "p"c    'Paragraph
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.LineFeed))

                            '================================================
                        Case "C"c, "c"c    'COLOR
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.Color))

                        '================================================
                        Case "F"c, "f"c 'FONT
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.Font))

                       '================================================
                        Case "Q"c  'OBLIQUE_ANGLE
                            '================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.Oblique))

                        '==========================================================
                        Case "S"c 'Stacking
                            '============================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.Stack))

                        '==========================================================
                        Case "A"c 'character alignment
                            '============================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.Alignment))

                        '==========================================================
                        Case "H"c 'char height change
                            '============================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.Height))
                        '==========================================================
                        Case "W"c 'width factor change
                            '============================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.WidthFactor))
                        '==========================================================
                        Case "T"c 'tracking change
                            '============================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.Tracking))
                        Case "~"c 'non breaking space
                            '============================================================
                            _rVal.Add(New Tuple(Of Char, dxxCharFormatCodes)(controlChar, dxxCharFormatCodes.NBS))

                    End Select
                Next
                Return _rVal
            End Get
        End Property
#End Region 'Shared Methods
    End Class



End Namespace

