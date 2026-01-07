

Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfStrings
        Inherits List(Of dxoString)

#Region "Fields"
        Private _CharBox As dxoCharBox
#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Init()
        End Sub

        Friend Sub New(aStrings As TSTRINGS)
            Init()
            Alignment = aStrings.Alignment
            AlignmentPtV = aStrings.AlignmentPtV
            Domain = aStrings.Domain

            DTextString = aStrings.DTextString
            FitLength = aStrings.FitLength
            FormatString = aStrings.FormatString
            HasFormats = aStrings.HasFormats
            LayerName = aStrings.LayerName

            LineSpacingFactor = aStrings.LineSpacingFactor
            LineSpacingStyle = aStrings.LineSpacingStyle

            LineWeight = aStrings.LineWeight
            RecomputeCharPaths = aStrings.RecomputeCharPaths
            StrAlignH = aStrings.StrAlignH
            StrAlignV = aStrings.StrAlignV
            SubStrAlignH = aStrings.SubStrAlignH
            SubStrAlignV = aStrings.SubStrAlignV
            TextType = aStrings.TextType
            _CharBox = New dxoCharBox(aStrings.CharBox)
            For i As Integer = 1 To aStrings.Count
                Add(New dxoString(aStrings.SubString(i)))
            Next

        End Sub

        Friend Sub New(aStrings As dxfStrings)
            Init()
            If aStrings Is Nothing Then Return

            Alignment = aStrings.Alignment
            AlignmentPtV = aStrings.AlignmentPtV
            Domain = aStrings.Domain
            DTextString = aStrings.DTextString
            FitLength = aStrings.FitLength
            FormatString = aStrings.FormatString
            HasFormats = aStrings.HasFormats
            LayerName = aStrings.LayerName

            LineSpacingFactor = aStrings.LineSpacingFactor
            LineSpacingStyle = aStrings.LineSpacingStyle

            LineWeight = aStrings.LineWeight
            RecomputeCharPaths = aStrings.RecomputeCharPaths
            StrAlignH = aStrings.StrAlignH
            StrAlignV = aStrings.StrAlignV
            SubStrAlignH = aStrings.SubStrAlignH
            SubStrAlignV = aStrings.SubStrAlignV
            TextType = aStrings.TextType
            _CharBox = New dxoCharBox(aStrings.CharBox)
            For Each mem As dxoString In aStrings
                Add(mem.Clone())
            Next

        End Sub
        Public Sub New(aText As dxeText, aStyle As dxoStyle, Optional aImage As dxfImage = Nothing)
            Init(aText, aStyle, aImage)

        End Sub

        Private Sub Init(Optional aText As dxeText = Nothing, Optional aStyle As dxoStyle = Nothing, Optional aImage As dxfImage = Nothing)
            Clear()
            If aText Is Nothing Then Return
            'the string to be rendered including format characters
            FormatString = aText.TextString
            TextType = aText.TextType
            Dim haveimage As Boolean = aText.GetImage(aImage)
            Dim tht As Double
            Dim fInfo As TFONTSTYLEINFO
            If aStyle Is Nothing And haveimage Then
                aStyle = New dxoStyle(aImage.GetOrAddReference(aText.TextStyleName, dxxReferenceTypes.STYLE))
            End If
            If aStyle Is Nothing Then aStyle = New dxoStyle("Standard")

            BaseFormats.FontStyleInfo = aStyle.GetFontStyleInfo()
            If Not fInfo.NotFound Then
                aStyle.FontIndex = fInfo.FontIndex
            End If
            If aText.TextHeight <= 0 Then
                tht = aStyle.TextHeight
                If tht > 0 Then
                    aText.TextHeight = tht
                Else
                    If aImage IsNot Nothing Then
                        aText.TextHeight = aImage.Header.TextSize
                    End If
                End If
            End If
            If aText.LineSpacingFactor <= 0 Then aText.LineSpacingFactor = aStyle.LineSpacingFactor
            LineSpacingFactor = aText.LineSpacingFactor
            If LineSpacingFactor <= 0 Then LineSpacingFactor = 1
            AlignmentPtV = aText.AlignmentPt1V
            LayerName = aText.LayerName
            LineWeight = aText.LineWeight
            If aImage IsNot Nothing Then
                TextBoxes = aImage.Header.QuickText
            End If
            LineSpacingFactor = aText.LineSpacingFactor
            LineSpacingStyle = aText.LineSpacingStyle
            Alignment = aText.Alignment

            BaseFormats.Color = aText.Color

            BaseFormats.CharHeight = aText.TextHeight
            If BaseFormats.CharHeight <= 0 Then BaseFormats.CharHeight = 0.2
            BaseFormats.Backwards = aText.Backwards
            BaseFormats.ObliqueAngle = aText.ObliqueAngle
            BaseFormats.WidthFactor = aText.WidthFactor
            BaseFormats.HeightFactor = 1
            BaseFormats.UpsideDown = aText.UpsideDown
            BaseFormats.Rotation = aText.Rotation
            BaseFormats.Vertical = aText.Vertical

            '.Rotation = Rotation  'Rotation is now handled by call to PlaneV
            BaseFormats.Tracking = 1
            If TextType = dxxTextTypes.Multiline Then
                BaseFormats.WidthFactor = aStyle.WidthFactor
                BaseFormats.ObliqueAngle = aStyle.ObliqueAngle
                BaseFormats.UpsideDown = False
                BaseFormats.Backwards = False

            Else
                If Not BaseFormats.IsShape Or aText.IsDimensionText Then
                    BaseFormats.Vertical = False
                Else

                    If aText.DrawingDirection = dxxTextDrawingDirections.ByStyle Then
                        BaseFormats.Vertical = aStyle.Vertical
                    Else
                        BaseFormats.Vertical = aText.DrawingDirection = dxxTextDrawingDirections.Vertical
                    End If
                End If


                If BaseFormats.WidthFactor <= 0 Then BaseFormats.WidthFactor = aStyle.WidthFactor

                BaseFormats.UpsideDown = aStyle.UpsideDown
                BaseFormats.Backwards = aStyle.Backwards
            End If
            BaseFormats.TextStyleName = aStyle.Name


            'get the string array with the texts properties assigned
            Dim ap1 As TVECTOR
            Dim ap2 As TVECTOR

            Dim txtplane As TPLANE = aText.PlaneV
            aText.Planarize(ap1, ap2)

            CharBox.AlignToOCS(aText.Plane)

            If TextType = dxxTextTypes.Attribute Then
                FormatString = aText.AttributeTag
            End If
            Dim d1 As Double = ap1.DistanceTo(ap2)
            Dim bFit As Boolean = False

            If Alignment = dxxMTextAlignments.Fit Or Alignment = dxxMTextAlignments.Aligned Then
                bFit = d1 > 0
                If bFit Then
                    If Alignment = dxxMTextAlignments.Fit Then FitLength = d1
                    Dim rot As Double = txtplane.XDirection.AngleTo(ap1.DirectionTo(ap2), txtplane.ZDirection)
                    If Vertical Then
                        rot = txtplane.YDirection.AngleTo(ap1.DirectionTo(ap2), txtplane.ZDirection) - 180
                    Else
                        rot = txtplane.XDirection.AngleTo(ap1.DirectionTo(ap2), txtplane.ZDirection)
                    End If
                    Rotation = rot
                End If
            Else

            End If

            _CorrectedAlignmentPt = AlignmentPtV
            If (Alignment = dxxMTextAlignments.Fit Or Alignment = dxxMTextAlignments.Aligned) And Not bFit Then Alignment = dxxMTextAlignments.BaselineLeft
        End Sub

#End Region 'Constructors
#Region "Properties"

        Private _CorrectedAlignmentPt As TVECTOR
        Friend Property CorrectedAlignmentPt As TVECTOR
            Get
                Return _CorrectedAlignmentPt
            End Get
            Set(value As TVECTOR)
                _CorrectedAlignmentPt = value

            End Set
        End Property

        Public ReadOnly Property CharacterCount As Integer
            Get
                If Count <= 0 Then Return 0
                Dim _rVal As Integer
                For Each mem As dxoString In Me
                    _rVal += mem.CharacterCount
                Next
                Return _rVal
            End Get
        End Property




        Private _LineWeight As dxxLineWeights
        Public Property LineWeight As dxxLineWeights
            Get
                Return _LineWeight
            End Get
            Friend Set(value As dxxLineWeights)
                _LineWeight = value
            End Set
        End Property

        Private _LayerName As String
        Public Property LayerName As String
            Get
                Return _LayerName
            End Get
            Friend Set(value As String)
                _LayerName = value
            End Set
        End Property



        Private _SubStrAlignV As dxxTextJustificationsVertical
        Public Property SubStrAlignV As dxxTextJustificationsVertical
            Get
                Return _SubStrAlignV
            End Get
            Friend Set(value As dxxTextJustificationsVertical)
                _SubStrAlignV = value
            End Set
        End Property

        Private _SubStrAlignH As dxxTextJustificationsHorizontal
        Public Property SubStrAlignH As dxxTextJustificationsHorizontal
            Get
                Return _SubStrAlignH
            End Get
            Friend Set(value As dxxTextJustificationsHorizontal)
                _SubStrAlignH = value
            End Set
        End Property

        Private _StrAlignV As dxxTextJustificationsVertical
        Public Property StrAlignV As dxxTextJustificationsVertical
            Get
                Return _StrAlignV
            End Get
            Friend Set(value As dxxTextJustificationsVertical)
                _StrAlignV = value
            End Set
        End Property

        Private _StrAlignH As dxxTextJustificationsHorizontal
        Public Property StrAlignH As dxxTextJustificationsHorizontal
            Get
                Return _StrAlignH
            End Get
            Friend Set(value As dxxTextJustificationsHorizontal)
                _StrAlignH = value
            End Set
        End Property


        Public ReadOnly Property BaseCharHeight As Double
            Get
                Return _BaseFormats.CharHeight
            End Get
        End Property

        Private _Domain As dxxDrawingDomains
        Public Property Domain As dxxDrawingDomains
            Get
                Return _Domain
            End Get
            Friend Set(value As dxxDrawingDomains)
                _Domain = value
            End Set
        End Property

        Private _RecomputeCharPaths As Boolean
        Public Property RecomputeCharPaths As Boolean
            Get
                Return _RecomputeCharPaths
            End Get
            Friend Set(value As Boolean)
                _RecomputeCharPaths = value
            End Set
        End Property
        Private _AlignmentPt As TVECTOR
        Friend Property AlignmentPtV As TVECTOR
            Get
                Return _AlignmentPt
            End Get
            Set(value As TVECTOR)
                _AlignmentPt = value
            End Set
        End Property

        Private _Alignment As dxxMTextAlignments
        Public Property Alignment As dxxMTextAlignments
            '^the Alignment of the text object with respect to it's insertion vector
            '~default = dxfBaselineLeft
            Get
                Return _Alignment
            End Get
            Friend Set(value As dxxMTextAlignments)
                If value < 1 Or value > 14 Then value = dxxMTextAlignments.BaselineLeft
                _Alignment = value
                SetSubStringAlignment()
            End Set
        End Property

        Private _CorrectedAlignment As dxxMTextAlignments
        Public Property CorrectedAlignment As dxxMTextAlignments
            '^the Alignment of the text object with respect to it's insertion vector
            '~default = dxfBaselineLeft
            Get
                Return _CorrectedAlignment
            End Get
            Friend Set(value As dxxMTextAlignments)
                _CorrectedAlignment = value
            End Set
        End Property

        Private _DTextString As String
        Public Property DTextString As String
            Get
                Return _DTextString
            End Get
            Friend Set(value As String)
                _DTextString = value
            End Set
        End Property

        Private _FitLength As Double
        Public Property FitLength As Double
            Get
                Return _FitLength
            End Get
            Friend Set(value As Double)
                _FitLength = value
            End Set
        End Property

        Private _BaseFormats As dxoCharFormat
        Friend Property BaseFormats As dxoCharFormat
            Get
                Return _BaseFormats
            End Get
            Set(value As dxoCharFormat)
                _BaseFormats = New dxoCharFormat(value)
            End Set
        End Property

        Private _FormatString As String
        Public Property FormatString As String
            Get
                Return _FormatString
            End Get
            Friend Set(value As String)
                If String.Compare(_FormatString, value, False) <> 0 Then
                    Init()
                End If
                _FormatString = value
            End Set
        End Property

        Private _HasFormats As Boolean
        Public Property HasFormats As Boolean
            Get
                Return _HasFormats
            End Get
            Friend Set(value As Boolean)
                _HasFormats = value
            End Set
        End Property

        Private _LineSpacingFactor As Double
        Public Property LineSpacingFactor As Double
            Get
                If _LineSpacingFactor <= 0 Then _LineSpacingFactor = 1
                Return _LineSpacingFactor
            End Get
            Friend Set(value As Double)
                _LineSpacingFactor = TVALUES.LimitedValue(Math.Round(value, 4), 0.25, 4)
            End Set
        End Property

        Private _LineSpacingStyle As dxxLineSpacingStyles
        Public Property LineSpacingStyle As dxxLineSpacingStyles
            Get
                If _LineSpacingStyle <> dxxLineSpacingStyles.AtLeast And _LineSpacingStyle <> dxxLineSpacingStyles.Exact Then
                    _LineSpacingStyle = dxxLineSpacingStyles.AtLeast
                End If
                Return _LineSpacingStyle
            End Get
            Friend Set(value As dxxLineSpacingStyles)
                If value <> dxxLineSpacingStyles.AtLeast And value <> dxxLineSpacingStyles.Exact Then Return
                _LineSpacingStyle = value
            End Set
        End Property

        Public ReadOnly Property CharacterBox As dxoCharBox
            Get
                Return New dxoCharBox(_CharBox)
            End Get
        End Property
        Friend ReadOnly Property CharBox As dxoCharBox
            Get
                Return _CharBox
            End Get
        End Property


        Friend ReadOnly Property PlaneV As TPLANE
            Get
                Return CharBox.PlaneV(True)
            End Get
        End Property

        Public Property Rotation As Double
            Get
                Return BaseFormats.Rotation
            End Get
            Friend Set(value As Double)
                BaseFormats.Rotation = value
            End Set
        End Property

        Public Property Vertical As Boolean
            Get
                Return BaseFormats.Vertical
            End Get
            Friend Set(value As Boolean)
                BaseFormats.Vertical = value
            End Set
        End Property

        Public Property Color As dxxColors
            Get
                Return BaseFormats.Color
            End Get
            Friend Set(value As dxxColors)
                BaseFormats.Color = value
            End Set
        End Property


        Private _Bounds As dxfRectangle
        Friend ReadOnly Property Bounds As dxfRectangle
            Get

                Return _Bounds
            End Get
        End Property

        Public ReadOnly Property BoundingRectangle As dxfRectangle
            Get
                If Bounds Is Nothing Then Return New dxfRectangle(New TPLANE(AlignmentPtV, New TVECTOR(1, 0, 0), New TVECTOR(0, 1, 0), 0, 0))
                Return New dxfRectangle(SubStringExtentVectors(False).Bounds(_CharBox.PlaneV(False)))
            End Get
        End Property

        Private _TextBoxes As Boolean
        Public Property TextBoxes As Boolean
            Get
                Return _TextBoxes
            End Get
            Friend Set(value As Boolean)
                _TextBoxes = value
            End Set
        End Property

        Private _TextType As dxxTextTypes
        Public Property TextType As dxxTextTypes
            Get
                If _TextType < dxxTextTypes.DText Or _TextType > dxxTextTypes.Multiline Then _TextType = dxxTextTypes.Multiline
                Return _TextType
            End Get
            Friend Set(value As dxxTextTypes)
                If value < dxxTextTypes.DText Or value > dxxTextTypes.Multiline Then Return
                _TextType = value
            End Set
        End Property

        Public ReadOnly Property NoEffects As Boolean
            Get
                Return IsDText
            End Get
        End Property

        Public ReadOnly Property IsDText As Boolean
            Get
                Return TextType <> dxxTextTypes.Multiline
            End Get
        End Property

        Private _NullChar As TCHAR
        Friend Property NullChar As TCHAR
            Get
                Return _NullChar
            End Get
            Set(value As TCHAR)
                _NullChar = value
            End Set
        End Property


#End Region 'Properties

#Region "Methods"

        Friend Function Characters(Optional bVisible As Boolean = True, Optional bOmmitStackChars As Boolean = False) As List(Of dxoCharacter)

            Dim _rVal As New List(Of dxoCharacter)()

            For Each mem As dxoString In Me
                Dim lidx As Integer = 0
                For Each memchar As dxoCharacter In mem.Characters
                    lidx += 1
                    If bVisible Then memchar.LineIndex = lidx
                    If Not bVisible And memchar.IsFormatCode Then Continue For
                    If bOmmitStackChars And memchar.IsStacked Then Continue For
                    _rVal.Add(memchar)

                Next

            Next

            Return _rVal

        End Function


        Public Function GetBounds(bForAlignment As Boolean) As dxfRectangle
            If _Bounds Is Nothing Then UpdateBounds()


            If Not bForAlignment Then Return _Bounds Else Return GetAlignmentBounds()

        End Function
        Public Function GetAlignmentBoundaries(Optional aLineNo As Integer = 0) As List(Of dxfRectangle)

            Dim _rVal As New List(Of dxfRectangle)
            If Bounds Is Nothing Then UpdateBounds()

            If IsDText Then
                aLineNo = 1

                Dim aRec As dxfRectangle
                If Vertical Then
                    aRec = dxoCharacters.CharBounds(Characters(bVisible:=True), bCharBoxBounds:=False)
                    If aRec.Height > 0 Then
                        aRec.Descent = aRec.Height - BaseCharHeight
                    End If

                Else
                    Select Case Alignment
                        Case dxxRectangularAlignments.MiddleLeft, dxxRectangularAlignments.MiddleCenter, dxxRectangularAlignments.MiddleRight
                            aRec = _CharBox.AscentRectangle(BaseFormats.CharHeight)

                        Case dxxMTextAlignments.TopCenter, dxxMTextAlignments.TopLeft, dxxMTextAlignments.TopRight

                            aRec = _CharBox.AscentRectangle(BaseFormats.CharHeight)
                        Case dxxMTextAlignments.BottomCenter, dxxMTextAlignments.BottomLeft, dxxMTextAlignments.BottomRight
                            aRec = New dxoCharBox(CharBox)

                        Case Else
                            aRec = New dxoCharBox(CharBox)

                    End Select
                End If

                _rVal.Add(aRec)
                Return _rVal
            End If


            Dim myPl As TPLANE = PlaneV
            Dim topline As TLINE = _Bounds.EdgeV(dxxRectangleLines.TopEdge, True, 10)
            Dim baseline As New TLINE()
            Dim updir As TVECTOR = CharBox.YDirectionV
            Dim downdir As TVECTOR = updir * -1

            For s As Integer = 1 To Count
                Dim mem As dxoString

                If aLineNo <= 0 Then
                    mem = Item(s - 1)
                Else
                    If s = aLineNo Then mem = Item(s - 1) Else Continue For
                End If


                Dim extPts As New TVECTORS(0)

                If Not IsDText Then

                    Dim vchrs As List(Of dxoCharacter) = mem.Characters.FindAll(Function(x) Not x.IsFormatCode)


                    Dim skippers As New List(Of dxoCharacter)()

                    If LineSpacingStyle = dxxLineSpacingStyles.AtLeast Then
                        skippers = vchrs.FindAll(Function(x) x.StackStyle <> dxxCharacterStackStyles.None)
                    End If
                    Dim c As Integer = 0
                    For Each vchar In vchrs
                        c += 1
                        If c = 1 Then
                            topline = vchar.GetBounds().EdgeV(dxxRectangleLines.TopEdge, True, 10)
                            baseline = vchar.GetBounds().EdgeV(dxxRectangleLines.BottomEdge, True, 10)
                        End If
                        Dim cnrs As TVECTORS

                        If Not IsDText Then
                            cnrs = vchar.ExtentPointsV()
                        Else
                            cnrs = vchar.CharBox.CornersV()
                        End If

                        Dim skipit As Boolean = (s = Count Or s = 1) And skippers.Contains(vchar) And Not IsDText
                        If skipit Then
                            For i As Integer = 1 To cnrs.Count
                                Dim v1 As TVECTOR = cnrs.Item(i)

                                Dim v2 As TVECTOR = v1.ProjectedTo(topline)
                                Dim v3 As TVECTOR = v1.DirectionTo(v2)
                                If v3.Equals(downdir, 2) Then
                                    extPts.Add(v2)
                                Else
                                    v2 = v1.ProjectedTo(baseline)
                                    v3 = v1.DirectionTo(v2)
                                    If v3.Equals(updir, 2) Then
                                        extPts.Add(v2)
                                    Else
                                        extPts.Add(v1)

                                    End If
                                End If

                            Next
                        Else
                            extPts.Append(cnrs)
                        End If

                    Next
                Else
                    extPts = mem.CharBox.CornersV
                End If



                _rVal.Add(New dxfRectangle(extPts.BoundingRectangle(myPl, bSuppressProjection:=True)) With {.Descent = 0})

            Next

            Return _rVal

        End Function

        Public Function GetAlignmentBounds(Optional aLineNo As Integer = 0) As dxfRectangle
            Dim strrecs As List(Of dxfRectangle) = GetAlignmentBoundaries(aLineNo)
            'If IsDText Then
            '    Return New dxoCharBox(CharBox)
            'End If


            Dim extPts As New TVECTORS(0)
            Dim myPl As TPLANE = PlaneV
            For Each rec As dxfRectangle In strrecs
                Dim cnrs As TVECTORS = rec.CornersV
                For i As Integer = 1 To cnrs.Count
                    extPts.Add(cnrs.Item(i))
                Next
            Next

            Dim _rVal As New dxfRectangle(extPts.BoundingRectangle(PlaneV, bSuppressProjection:=True)) With {.Descent = 0}
            If IsDText Then _rVal.Descent = _CharBox.Descent
            Return _rVal

        End Function

        Friend Function Structure_Get() As TSTRINGS
            Return New TSTRINGS(Me)

        End Function

        Public Shadows Function Item(aIndex As Integer) As dxoString
            If aIndex < 0 Or aIndex > Count - 1 Then Return Nothing
            Dim aStr As dxoString = MyBase.Item(aIndex)
            aStr.BaseFormats = BaseFormats
            aStr.CopyDirections(CharBox)
            aStr.LineNo = aIndex + 1
            aStr.TextType = TextType
            Return aStr

        End Function

        Public Overrides Function ToString() As String
            Return $"dxfStrings[{ Count }] { DTextString }"

        End Function

        Friend Function GeneratePaths(Optional bRecomputeCharPaths As Boolean = False, Optional aForBitmap As Boolean = False, Optional bBlackIsWhite As Boolean = False) As List(Of TPATH)

            '#1flag to recalculate the fonts character patterns
            '#2flag indicating that the paths ar for display in a bitmap
            '#3flag indicating that black and white should be inverted
            '^returns the paths need to render of the current FormatString

            '~This must be executed only once after New(aText,aStyle, aImage) is invoked

            Try
                CorrectedAlignment = dxxMTextAlignments.AlignUnknown
                RecomputeCharPaths = bRecomputeCharPaths
                CharBox.Define(AlignmentPtV, CharBox.XDirectionV, CharBox.YDirectionV, True, aHeight:=0, aWidth:=0)
                If Rotation <> 0 Then CharBox.Revolve(Rotation, False)

                If String.IsNullOrWhiteSpace(_FormatString) Then Return New List(Of TPATH)

                Dim lcnt As Integer
                Dim fInfo As TFONTSTYLEINFO = BaseFormats.UpdateFontStyleInfo()

                'color settings for bitmaps instead of images
                If aForBitmap Then
                    If BaseFormats.Color = dxxColors.ByBlock Or BaseFormats.Color = dxxColors.ByLayer Then BaseFormats.Color = 0 'vbBlack
                    If bBlackIsWhite And BaseFormats.Color = 0 Then BaseFormats.Color = 16777215 'vbWhite
                End If
                BaseFormats.Backwards = BaseFormats.Backwards And TextType <> dxxTextTypes.Multiline
                BaseFormats.UpsideDown = BaseFormats.UpsideDown And TextType <> dxxTextTypes.Multiline
                If BaseFormats.Backwards Then
                    CharBox.Define(CharBox.BasePtV, CharBox.XDirectionV * -1, CharacterBox.YDirectionV)
                End If
                If BaseFormats.UpsideDown Then
                    CharBox.Define(CharBox.BasePtV, CharBox.XDirectionV, CharacterBox.YDirectionV * -1)
                End If


                'format the null char
                NullChar = fInfo.GetNullChar(BaseCharHeight, CharBox, BaseFormats)


                'get the string (with format codes)
                'replace linefeeds with \P and get the number of lines
                _FormatString = ParseString(_FormatString, lcnt)
                If lcnt > 1 Then _HasFormats = True

                'this call sets the format codes of the characters in the subject string
                Dim formatedChars As New dxoCharacters(_FormatString, BaseFormats, NoEffects, CharBox)

                DTextString = formatedChars.CharacterString() 'visible part only
                _FormatString = formatedChars.CharacterString(formatedChars)  'with character codes
                Dim lid As Integer = 1
                Dim curString As New dxoString(CharBox) With {.Vertical = Vertical, .LineSpacingStyle = LineSpacingStyle, .LineNo = lid, .TextType = TextType}

                curString.AddChar(NullChar) 'add the alignment char
                For Each formatedChar In formatedChars

                    'update the character path for size height and width
                    dxoFonts.GetCharacterPath(formatedChar, RecomputeCharPaths, bDontFormat:=formatedChar.IsFormatCode)
                    If formatedChar.FormatCode = dxxCharFormatCodes.LineFeed Then
                        Add(curString)
                        lid += 1
                        curString = New dxoString(CharBox) With {.Vertical = Vertical, .LineSpacingStyle = LineSpacingStyle, .LineNo = lid, .TextType = TextType}
                        curString.AddChar(NullChar) 'add the alignment char
                    Else
                        curString.AddChar(formatedChar)
                    End If
                Next
                If curString.CharacterCount > 0 Then Add(curString)


                ApplyAlignment()

                Return GetPaths(Domain)
            Catch ex As Exception
                Return New List(Of TPATH)
            End Try


        End Function

        Private Sub ApplyAlignment()

            If Count <= 0 Then Return
            Try

                UpdateBounds(True) 'the subsrings are aligned baseline left to the first string

                Dim d1 As Double

                Dim ap1 As TVECTOR = CharBox.BasePtV
                Dim ap2 As TVECTOR = AlignPtV()
                Dim aDir As TVECTOR = ap2.DirectionTo(ap1, False, rDistance:=d1)
                If Math.Round(d1, 8) <> 0 Then
                    Translate(aDir * d1)
                End If
            Catch ex As Exception
            End Try

        End Sub

        Public Overloads Sub Clear()

            MyBase.Clear()
            _CharBox = New dxoCharBox()
            _Bounds = Nothing
            _LayerName = "0"
            _LineSpacingFactor = 1
            _LineSpacingStyle = dxxLineSpacingStyles.AtLeast
            _BaseFormats = New dxoCharFormat()
            TextType = dxxTextTypes.Multiline
            _HasFormats = False
            BaseFormats.Tracking = 1
            BaseFormats.CharAlign = dxxCharacterAlignments.Bottom
            BaseFormats.Underline = False
            BaseFormats.Overline = False
            BaseFormats.HeightFactor = 1
            BaseFormats.Rotation = 0
            DTextString = ""
            RecomputeCharPaths = False
            CorrectedAlignment = dxxMTextAlignments.AlignUnknown
        End Sub

        Private Function UpdateBounds(Optional bApplyLineStep As Boolean = False) As dxfRectangle
            'define the collection rectangles

            Dim aStr As dxoString = Item(0)
            Dim lStr As dxoString

            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim baseLine As New TLINE("")
            Dim alnLine As New TLINE("")

            Dim vLine As TLINE
            Dim aDir As TVECTOR
            Dim d1 As Double

            Dim visChars As New List(Of dxoCharacter)
            Dim xDir As TVECTOR = _CharBox.XDirectionV
            Dim yDir As TVECTOR = _CharBox.YDirectionV
            _Bounds = New dxfRectangle(_CharBox.PlaneV(False))
            Dim myPl As TPLANE = _CharBox.PlaneV(True)
            Dim extPts As New TVECTORS(0)

            If Count <= 0 Then Return _Bounds

            If IsDText Then
                _Bounds = New dxfRectangle(aStr.UpdateBounds())
                _CharBox = New dxoCharBox(aStr.CharBox)
                Return _Bounds
            End If

            _CharBox.Width = 0
            _CharBox.Ascent = 0
            _CharBox.Descent = 0
            _Bounds.SetDimensions(0, 0)
            SetSubStringAlignment()
            Dim basept As TVECTOR = _CharBox.BasePtV
            baseLine = New TLINE(basept, basept + (xDir * 100))
            vLine = New TLINE(basept, basept + (yDir * 100))
            For li As Integer = 1 To Count

                aStr = Item(li - 1)

                If li = 1 Then
                    aStr.CharBox.BasePtV = basept
                    aStr.CopyDirections(_CharBox)
                    aStr.GetBounds(True) 'this call aligns the characters in the sub-string


                Else
                    lStr = Item(li - 2)
                    lStr.GetBounds()


                    If Not Vertical Then
                        basept += myPl.YDirection * (-LineSpacingFactor * BaseCharHeight * (5 / 3))
                        aStr.CharBox.CopyDirections(CharBox, True)
                        aStr.CharBox.BasePtV = basept

                        aStr.GetBounds(True) 'this call aligns the characters in the sub-string
                        If LineSpacingStyle = dxxLineSpacingStyles.Exact Then
                            v1 = lStr.CharBox.BaselineLeftV + myPl.YDirection * -LineSpacingFactor * lStr.BaseFormats.CharHeight * (5 / 3)
                            Dim line1 As New TLINE(v1, v1 + myPl.XDirection * 10)
                            v1 = aStr.CharBox.BaselineLeftV
                            v2 = dxfProjections.ToLine(v1, line1, rDistance:=d1)
                            aDir = v1.DirectionTo(v2, rDistance:=d1)

                            aStr.Translate(aDir * d1)

                        End If

                        If LineSpacingStyle = dxxLineSpacingStyles.AtLeast Then
                            Dim lstep As Double = LineSpacingFactor * aStr.Ascent * (5 / 3)
                            v1 = lStr.CharBox.BaselineLeftV + myPl.YDirection * -lstep
                            Dim line1 As New TLINE(v1, v1 + myPl.XDirection * 10)
                            v1 = aStr.CharBox.BaselineLeftV
                            v2 = dxfProjections.ToLine(v1, line1, rDistance:=d1)
                            aDir = v1.DirectionTo(v2, rDistance:=d1)

                            aStr.Translate(aDir * d1)


                        End If



                    End If
                End If

                'Continue For

                ''move the line to the basept
                'v1 = aStr.Bounds.TopLeftV
                'v2 = v1.ProjectedTo(vLine)
                'aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                'trans = aDir * d1

                ''aDir = ssBox.BasePt.DirectionTo(_CharBox.BasePt, rDistance:=d1)
                'If d1 <> 0 And aDir.Equals(myPl.XDirection, True, 3) Then
                '    'aligh left edges
                '    aStr.Translate(aDir * d1)

                'End If
                ''apply line spacing
                'If li > 1 Then
                '    lStr = SubString(li - 1)
                '    lStr.GetBounds()
                '    lBox = lStr.CharBox
                '    '========================================
                '    'VERTICAL TEXT
                '    '========================================
                '    If Vertical Then
                '        'set the base line to baseline space
                '        lBox.VerticalAlignment = False
                '        v1 = lBox.PointV(dxxRectanglePts.TopRight, True)
                '        v2 = aStr.Bounds.TopLeftV
                '        aDir = v2.DirectionTo(v1 + (xDir * (LineStep - lBox.Width)), False, rDistance:=d1)
                '        If d1 <> 0 Then
                '            v3 = aDir * d1
                '            aStr.Translate(v3)
                '            v2 += v3
                '        End If
                '        'if the spacing is atleast then make sure the gap is at least the default gap
                '        If LineSpacingStyle = dxxLineSpacingStyles.AtLeast Then
                '            v3 = v1 + (xDir * _LineGap)
                '            aDir = v2.DirectionTo(v3, False, rDistance:=d1)
                '            If d1 > 0 And aDir.Equals(xDir) Then
                '                v3 = aDir * d1
                '                aStr.Translate(v3)
                '            End If
                '        End If
                '    Else


                'update ascent
                v1 = aStr.Bounds.TopLeftV.WithRespectTo(myPl)
                If v1.Y > _CharBox.Ascent Then _CharBox.Ascent = v1.Y
                'update descent
                v1 = aStr.Bounds.BottomLeftV.WithRespectTo(myPl)
                If v1.Y < 0 And Math.Abs(v1.Y) > _CharBox.Descent Then _CharBox.Descent = Math.Abs(v1.Y)
                'update width
                v1 = aStr.Bounds.TopRightV.WithRespectTo(myPl)
                If v1.X > _CharBox.Width Then _CharBox.Width = v1.X
                extPts.Append(aStr.Bounds.CornersV)
                If IsDText Then
                    extPts.Append(aStr.CharBox.CornersV)
                End If
                visChars.AddRange(aStr.Characters.VisibleCharacters)
            Next li
            _Bounds = New dxfRectangle(extPts.BoundingRectangle(myPl))
            'justify the lines
            'all the strings are now aligned baseline left at the parent string array (TSTRINGS) and spaced
            If Count > 1 And Alignment <> dxxMTextAlignments.BaselineLeft Then
                Dim apt As dxxRectanglePts
                v3 = TVECTOR.Zero
                If Vertical Then
                    If SubStrAlignV = dxxTextJustificationsVertical.Middle Then
                        alnLine = _Bounds.LineH(0, 100)
                        apt = dxxRectanglePts.MiddleCenter
                    ElseIf SubStrAlignV = dxxTextJustificationsVertical.Bottom Then
                        alnLine = _Bounds.LineH(-_Bounds.Height / 2, 100)
                        apt = dxxRectanglePts.BottomCenter
                    End If
                Else
                    If SubStrAlignH = dxxTextJustificationsHorizontal.Center Then
                        alnLine = _Bounds.LineV(0, 100)
                        apt = dxxRectanglePts.MiddleCenter
                    ElseIf SubStrAlignH = dxxTextJustificationsHorizontal.Right Then
                        alnLine = _Bounds.LineV(_Bounds.Width / 2, 100)
                        apt = dxxRectanglePts.MiddleRight
                    End If
                End If
                For li = 1 To Count
                    aStr = Item(li - 1)
                    d1 = 0
                    v1 = aStr.BoundingRectangleV.Point(apt)
                    v2 = v1.ProjectedTo(alnLine)
                    aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                    If d1 <> 0 Then aStr.Translate(aDir * d1)

                Next li
            End If
            If Not IsDText Then
                _CharBox.Width = _Bounds.Width

            Else
                aStr = Item(0)
                _CharBox = New dxoCharBox(aStr.CharBox)
                _Bounds = dxoCharacters.CharBounds(visChars)

            End If

            Return _Bounds
        End Function

        Friend Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return
            _CharBox.Translate(aTranslation)
            If Bounds IsNot Nothing Then _Bounds.Translate(aTranslation)
            For Each mem As dxoString In Me
                mem.Translate(aTranslation)
            Next
        End Sub

        Friend Sub Scale(aScaleX As Double, aReference As TVECTOR, Optional aScaleY As Double = 0.0, Optional aScaleZ As Double = 0.0)
            FitLength *= Math.Abs(aScaleX)
            _CharBox.Rescale(aScaleX, aReference)
            If Bounds IsNot Nothing Then _Bounds.ScaleUp(aScaleX, aScaleY, aReference, True, aScaleZ)
            For Each mem As dxoString In Me
                mem.Rescale(aScaleX, aReference)
            Next
        End Sub
        Friend Sub RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False)
            _AlignmentPt.RotateAbout(aOrigin, aAxis, aAngle, True, True)
            If Bounds IsNot Nothing Then _Bounds.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bRotateOrigin, bRotateDirections, True)
            For Each mem As dxoString In Me
                mem.Rotate(aOrigin, aAxis, bInRadians, bSuppressNorm)
            Next
        End Sub

        Friend Sub Mirror(aLine As TLINE, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False)
            Mirror(aLine.SPT, aLine.EPT, bMirrorOrigin, bMirrorDirections, bSuppressCheck)
        End Sub
        Friend Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False)

            If aSP = aEP Then Return
            Dim mirline As New TLINE(aSP, aEP)
            _AlignmentPt.Mirror(mirline, bSuppressCheck:=True)
            If Bounds IsNot Nothing Then _Bounds.Mirror(mirline, bMirrorOrigin, bMirrorDirections)
            For Each mem As dxoString In Me
                mem.Mirror(aSP, aEP, bMirrorDirections, True)
            Next

        End Sub

        Private Sub SetStringAlignment()

            TFONT.EncodeAlignment(Alignment, _StrAlignV, _StrAlignH)
            'StrAlignV = dxxTextJustificationsVertical.Baseline
            'StrAlignH = dxxTextJustificationsHorizontal.Left
            'Select Case Alignment
            ' '---------------------------------------------------------------------------------------------------------
            '    Case dxxMTextAlignments.BaselineLeft
            '        Return
            '    Case dxxMTextAlignments.BaselineMiddle
            '        StrAlignH = dxxTextJustificationsHorizontal.Center
            '    Case dxxMTextAlignments.BaselineRight
            '        StrAlignH = dxxTextJustificationsHorizontal.Right
            ' '---------------------------------------------------------------------------------------------------------
            '    Case dxxMTextAlignments.BottomLeft
            '        StrAlignV = dxxTextJustificationsVertical.Bottom
            '    Case dxxMTextAlignments.BottomCenter
            '        StrAlignV = dxxTextJustificationsVertical.Bottom
            '        StrAlignH = dxxTextJustificationsHorizontal.Center
            '    Case dxxMTextAlignments.BottomRight
            '        StrAlignV = dxxTextJustificationsVertical.Bottom
            '        StrAlignH = dxxTextJustificationsHorizontal.Right
            ' '---------------------------------------------------------------------------------------------------------
            '    Case dxxMTextAlignments.MiddleLeft
            '        StrAlignV = dxxTextJustificationsVertical.Middle
            '    Case dxxMTextAlignments.MiddleCenter
            '        StrAlignV = dxxTextJustificationsVertical.Middle
            '        StrAlignH = dxxTextJustificationsHorizontal.Center
            '    Case dxxMTextAlignments.MiddleRight
            '        StrAlignV = dxxTextJustificationsVertical.Middle
            '        StrAlignH = dxxTextJustificationsHorizontal.Right
            ' '---------------------------------------------------------------------------------------------------------
            '    Case dxxMTextAlignments.TopLeft
            '        StrAlignV = dxxTextJustificationsVertical.Top
            '    Case dxxMTextAlignments.TopCenter
            '        StrAlignV = dxxTextJustificationsVertical.Top
            '        StrAlignH = dxxTextJustificationsHorizontal.Center
            '    Case dxxMTextAlignments.TopRight
            '        StrAlignV = dxxTextJustificationsVertical.Top
            '        StrAlignH = dxxTextJustificationsHorizontal.Right
            'End Select
        End Sub

        Private Sub SetSubStringAlignment()
            Dim rV As dxxTextJustificationsVertical
            Dim rH As dxxTextJustificationsHorizontal
            dxfEnums.DecomposeMTextAlignment(Alignment, rV, rH, Vertical)

            SubStrAlignV = rV
            SubStrAlignH = rH

        End Sub


        Private Function GetPaths(aDomain As dxxDrawingDomains) As List(Of TPATH)

            Dim aChar As TCHAR
            Dim aDsp As New TDISPLAYVARS(LayerName, dxfLinetypes.Continuous, Color, LineWeight)
            Dim myPl As New dxfPlane(CharBox.PlaneV(True))
            'initialize the base path
            Dim _rVal As New List(Of TPATH)
            Dim allvischars As New List(Of dxoCharacter)
            If Not TextBoxes Or aDomain = dxxDrawingDomains.Screen Then
                For Each substr As dxoString In Me
                    'get the visible characters in the member substring
                    Dim vischars As List(Of dxoCharacter) = substr.Characters.FindAll(Function(x) Not x.IsFormatCode)

                    For Each vischar As dxoCharacter In vischars
                        Dim istruetype As Boolean = vischar.IsTrueType
                        aDsp.Color = vischar.Color
                        aDsp.Linetype = dxfLinetypes.Continuous

                        If vischar.Shape.Path.Count <= 0 Then Continue For

                        'get the visible character paths with respect to the global plane.  this sets the identifier on the returned paths
                        Dim chrPaths As List(Of TPATH) = vischar.Paths(aDsp, myPl, aDomain)
                        Dim mergeinto As TPATH

                        For Each charpath As TPATH In chrPaths
                            'find a path int the return with the same color and filled (true type) property combination
                            Dim idx As Integer = _rVal.FindIndex(Function(x) String.Compare(x.Identifier, charpath.Identifier, True) = 0)

                            If idx < 0 Then
                                'just save the uniq character path
                                _rVal.Add(charpath)
                            Else
                                'merge the uniq character path to another that is the same color and filled (true type) property combination
                                mergeinto = _rVal.Item(idx)
                                mergeinto.AppendToLooop(1, charpath.Looop(1))
                                _rVal.Item(idx) = mergeinto
                            End If
                            allvischars.Add(vischar)
                        Next
                    Next
                Next


            End If
            If TextBoxes And aDomain <> dxxDrawingDomains.Screen Then
                Dim subStrs As New dxoSubStrings(Me, New dxoCharFormat(BaseFormats))
                For li As Integer = 1 To subStrs.Count
                    Dim grpStr As TSTRING = subStrs.ItemV(li)
                    Dim aRec As TPLANE = grpStr.BoundingRectangleV
                    aChar = grpStr.CharacterV(1)
                    Dim chrPath As TPATH = aChar.Path(aDsp, myPl, True)

                    chrPath.SetLoop(1, aRec.Corners(True, True).WithRespectToPlane(myPl))
                    _rVal.Add(chrPath)
                Next li
            End If

            Return _rVal
        End Function

        Friend Function MTextAlignmentPt(aAlignmentPt1 As TVECTOR, ByRef rAlignment As dxxMTextAlignments) As TVECTOR
            Dim _rVal As TVECTOR = aAlignmentPt1.Clone()
            '^the aligment pt used to write an MText to file
            '~adjusts the alignment pt to account for baseline alignments
            '~since autocad doesn't allow these for mtext
            rAlignment = Alignment
            If rAlignment > dxxMTextAlignments.BottomRight Then
                If rAlignment = dxxMTextAlignments.BaselineLeft Then
                    rAlignment = dxxMTextAlignments.TopLeft
                    _rVal = _CharBox.TopLeftV
                ElseIf rAlignment = dxxMTextAlignments.BaselineMiddle Then
                    rAlignment = dxxMTextAlignments.TopCenter
                    _rVal = _CharBox.TopCenterV
                ElseIf rAlignment = dxxMTextAlignments.BaselineRight Then
                    rAlignment = dxxMTextAlignments.TopRight
                    _rVal = _CharBox.TopRightV
                End If
            End If
            Return _rVal
        End Function

        Friend Function AlignmentVectors(aOCS As TPLANE, aAlignmentPt1 As TVECTOR, aAlignmentPt2 As TVECTOR, ByRef tAlign As dxxMTextAlignments, aWidthFactor As Double, aTransforms As TTRANSFORMS) As TVECTORS
            Dim _rVal As New TVECTORS
            '#1the text to get the dxf properties for
            '^returns the properties required to write the object to a dxf file
            '~the DXF code of an Entity is it's entry in the Entities section in the DXF file
            Dim v1 As TVECTOR = aAlignmentPt1.Clone()
            Dim v2 As TVECTOR = aAlignmentPt1.Clone()
            Dim v3 As TVECTOR
            If tAlign <> dxxMTextAlignments.BaselineLeft Then
                If tAlign = dxxMTextAlignments.Aligned Or tAlign = dxxMTextAlignments.Fit Then
                    v2 = aAlignmentPt2.Clone()
                Else
                    v3 = v1.Clone()
                    v1 = CharacterBox.BasePtV
                    v2 = v3.Clone()
                End If
            End If
            If aTransforms.Count > 0 Then
                TTRANSFORMS.Apply(aTransforms, v1)
                TTRANSFORMS.Apply(aTransforms, v2)
            End If
            v1 = v1.WithRespectTo(aOCS, aPrecis:=6)
            v2 = v2.WithRespectTo(aOCS, aPrecis:=6)
            _rVal.Add(v1)
            _rVal.Add(v2)
            Return _rVal
        End Function

        Friend Function Character(aCharIndex As Integer, Optional aLineNo As Integer = 0) As dxoCharacter
            Dim _rVal As dxoCharacter = Nothing
            'On Error Resume Next
            Dim aStr As dxoString = Nothing
            Dim cnt As Long
            Dim i As Integer
            Dim idx As Integer
            If aCharIndex <= 0 Then Return Nothing
            If aLineNo > 0 Then
                If aLineNo > Count Then Return Nothing
                aStr = Item(aLineNo - 1)
                _rVal = aStr.Character(aCharIndex)

                If aCharIndex > 0 And aCharIndex <= aStr.CharacterCount Then

                    _rVal.LineIndex = aCharIndex
                    _rVal.LineNo = aLineNo

                End If

            Else
                cnt = 0
                idx = -1
                For i = 1 To Count
                    aStr = Item(i - 1)
                    If aCharIndex <= cnt + aStr.CharacterCount Then
                        idx = aCharIndex - cnt
                        Exit For
                    Else
                        cnt += aStr.CharacterCount
                    End If
                Next i
                If aStr IsNot Nothing Then
                    If idx > 0 And idx <= aStr.CharacterCount Then
                        _rVal = aStr.Character(idx)
                        _rVal.LineIndex = aCharIndex
                        _rVal.LineNo = aStr.LineNo
                    End If
                End If

            End If
            Return _rVal
        End Function

        Public Function SubText(aText As dxeText, aLineNo As Integer, aGroupNo As Integer, Optional aTextType As dxxTextTypes = dxxTextTypes.Multiline) As dxeText
            Dim _rVal As dxeText = Nothing
            If aText Is Nothing Then Return Nothing
            If aLineNo < 0 Or aLineNo > Count Then Return Nothing
            If Not dxfEnums.Validate(GetType(dxxTextTypes), TVALUES.To_INT(aTextType), bSkipNegatives:=True) Then aTextType = aText.TextType
            Try
                Dim aChar As dxoCharacter
                Dim fStr As String
                Dim aPl As TPLANE = PlaneV
                Dim aStr As dxoString = Item(aLineNo - 1)
                Dim bStr As dxoString
                Dim v1 As TVECTOR
                Dim fInfo As TFONTSTYLEINFO
                Dim si As Integer = 0
                If aStr Is Nothing Then Return _rVal

                If aStr.GroupCount > 1 Then
                    bStr = aStr.GetByGroupIndex(aGroupNo, si, si)
                Else
                    bStr = aStr
                End If
                _rVal = aText.ConvertToTextType(aTextType).Item(0)
                If bStr.CharacterCount <= 0 Then Return _rVal
                If bStr.CharacterCount <> aStr.CharacterCount Then
                    bStr.UpdateBounds()
                    aStr = bStr
                Else
                    aStr.UpdateBounds()
                End If
                aChar = aStr.Character(1)
                aPl.Clone()
                _rVal.ImageGUID = aText.ImageGUID
                _rVal.OwnerGUID = aText.GUID
                _rVal.Color = aChar.Formats.Color
                _rVal.TextHeight = aChar.Formats.CharHeight
                _rVal.ObliqueAngle = aChar.Formats.ObliqueAngle
                _rVal.WidthFactor = aChar.Formats.WidthFactor
                _rVal.SourceString = aText.TextString
                _rVal.SourceCount = Count
                v1 = aStr.AlignPt(_rVal.Alignment)
                '        v1 = v1.WithRespectTo( aPl)
                '        v1 = aPl.Vector( v1.X, v1.Y)
                _rVal.AlignmentPt1V = v1
                If _rVal.Alignment = dxxMTextAlignments.Aligned Or _rVal.Alignment = dxxMTextAlignments.Fit Then
                    _rVal.WidthFactor = 1
                    _rVal.AlignmentPt2V = aStr.AlignPt(dxxMTextAlignments.BaselineRight)
                End If
                If Vertical Then
                    _rVal.DrawingDirection = dxxTextDrawingDirections.Vertical
                Else
                    _rVal.DrawingDirection = dxxTextDrawingDirections.Horizontal
                End If
                If aChar.Formats.FontIndex <> BaseFormats.FontIndex Or aChar.Formats.StyleIndex <> BaseFormats.StyleIndex Then
                    _rVal.TextType = dxxTextTypes.Multiline
                    fInfo = dxoFonts.GetFontStyleInfo(aIndex:=aChar.Formats.FontIndex, aStyleIndex:=aChar.Formats.StyleIndex, bReturnDefault:=True)
                    If Not fInfo.IsShape Then
                        fStr = "\F" & fInfo.FaceName
                        If fInfo.TTFStyle = dxxTextStyleFontSettings.Bold Or fInfo.TTFStyle = dxxTextStyleFontSettings.BoldItalic Then
                            fStr += "|b1"
                        Else
                            fStr += "|b0"
                        End If
                        If fInfo.TTFStyle = dxxTextStyleFontSettings.Italic Or fInfo.TTFStyle = dxxTextStyleFontSettings.BoldItalic Then
                            fStr += "|i1"
                        Else
                            fStr += "|i0"
                        End If
                        fStr += "|c0|p34;"
                    Else
                        fStr = "\F" & fInfo.FontName & ";"
                    End If
                    fStr += aStr.Characters.CharacterString()
                Else
                    fStr = aStr.Characters.CharacterString()
                End If
                _rVal.TextString = fStr
            Catch
            End Try
            Return _rVal
        End Function
        Friend Function AlignPtV() As TVECTOR
            Dim rSubStrAlignV As dxxTextJustificationsVertical = dxxTextJustificationsVertical.Baseline
            Dim rSubStrAlignH As dxxTextJustificationsHorizontal = dxxTextJustificationsHorizontal.Left
            Return AlignPtV(rSubStrAlignV, rSubStrAlignH)
        End Function
        Public Function SubStringExtentPoints(Optional bReturnAllCharExtentPoints As Boolean = False) As colDXFVectors
            Return New colDXFVectors(SubStringExtentVectors(bReturnAllCharExtentPoints))
        End Function
        Friend Function SubStringExtentVectors(Optional bReturnAllCharExtentPoints As Boolean = False) As TVECTORS
            Dim brec As dxfRectangle = GetBounds(False)
            Dim _rVal As New TVECTORS(0)
            Dim myPlane As New TPLANE(brec)

            For Each substring As dxoString In Me
                Dim evecs As TVECTORS = substring.ExtentPointsV
                If Not bReturnAllCharExtentPoints Then
                    Dim erec As TPLANE = evecs.BoundingRectangle(myPlane)
                    evecs = erec.Corners
                End If
                _rVal.Append(evecs)
            Next
            Return _rVal
        End Function

        Friend Function AlignPtV(ByRef rSubStrAlignV As dxxTextJustificationsVertical, ByRef rSubStrAlignH As dxxTextJustificationsHorizontal) As TVECTOR

            If Bounds Is Nothing Then UpdateBounds()
            If Bounds Is Nothing Then Return AlignmentPtV

            SetSubStringAlignment()
            rSubStrAlignV = SubStrAlignV
            rSubStrAlignH = SubStrAlignH
            SetStringAlignment()


            If Count <= 0 Then Return CharBox.BasePtV
            Dim aRec As dxfRectangle = GetBounds(True)

            'If LineSpacingStyle = dxxLineSpacingStyles.Exact And Not Vertical Then
            '    Dim bRec As New dxfRectangle(ExtentPointsV(True).Bounds(PlaneV))
            '    Dim v1 As TVECTOR = bRec.TopCenterV.WithRespectTo(aRec.Strukture)
            '    Dim d1 As Double = 0
            '    If v1.Y > aRec.Height / 2 Then
            '        d1 = v1.Y - aRec.Height / 2
            '        aRec.Translate(aRec.YDirectionV * d1 / 2)
            '        aRec.Height -= d1
            '    End If

            '    v1 = bRec.BottomCenterV.WithRespectTo(aRec.Strukture)
            '    If v1.Y < 0 And Math.Abs(v1.Y) > aRec.Height / 2 Then
            '        d1 = Math.Abs(v1.Y) - aRec.Height / 2
            '        aRec.Translate(bRec.YDirectionV * -d1 / 2)
            '        aRec.Height -= d1
            '    End If

            'End If


            Dim aPt As dxxRectanglePts = dxxRectanglePts.BaselineLeft
            Dim fStr As dxoString = Item(0)
            Dim lStr As dxoString = Item(Count - 1)
            Dim dX As Double
            Dim dY As Double
            Dim algnm As dxxMTextAlignments = Alignment
            If Vertical Then
                Dim myPl As TPLANE = PlaneV
                Select Case algnm
                    Case dxxMTextAlignments.BaselineLeft
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dY = -BaseCharHeight
                        dX = -0.5 * aRec.Width

                    Case dxxMTextAlignments.BaselineMiddle
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dY = -BaseCharHeight
                        dX = 0
                    Case dxxMTextAlignments.BaselineRight
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dY = -BaseCharHeight
                        dX = 0.5 * aRec.Width
                    Case dxxMTextAlignments.TopLeft
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dX = -0.5 * aRec.Width
                    Case dxxMTextAlignments.TopRight
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dX = 0.5 * aRec.Width
                    Case dxxMTextAlignments.MiddleLeft
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dX = -0.5 * aRec.Width
                        dY = -0.5 * aRec.Height
                    Case dxxMTextAlignments.MiddleCenter
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dX = 0
                        dY = -0.5 * aRec.Height
                    Case dxxMTextAlignments.MiddleRight
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        algnm = dxxMTextAlignments.TopCenter
                        dX = 0.5 * aRec.Width
                        dY = -0.5 * aRec.Height

                    Case dxxMTextAlignments.BottomLeft
                        algnm = dxxMTextAlignments.TopCenter
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        dX = -0.5 * aRec.Width
                        dY = -aRec.Height
                    Case dxxMTextAlignments.BottomCenter
                        algnm = dxxMTextAlignments.TopCenter
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        dX = 0
                        dY = -aRec.Height

                    Case dxxMTextAlignments.BottomRight
                        algnm = dxxMTextAlignments.TopCenter
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                        dX = 0.5 * aRec.Width
                        dY = -aRec.Height

                    Case dxxMTextAlignments.Fit
                        'vertical text can't be fit
                        algnm = dxxMTextAlignments.TopCenter
                        CorrectedAlignment = dxxMTextAlignments.TopCenter
                    Case dxxMTextAlignments.Aligned
                        algnm = dxxMTextAlignments.TopCenter
                    Case Else
                        dX = 0
                        dY = 0
                End Select

                CorrectedAlignmentPt = AlignmentPtV + myPl.YDirection * -dY + myPl.XDirection * -dX
            End If
            'If StrAlignV = dxxTextJustificationsVertical.Top Then
            '    dY = CharBox.Ascent
            'ElseIf StrAlignV = dxxTextJustificationsVertical.Bottom Then
            '    dY = -CharBox.Descent
            'ElseIf StrAlignV = dxxTextJustificationsVertical.Middle Then
            '    dY = -CharBox.Descent + 0.5 * CharBox.Height
            'End If
            'If StrAlignH = dxxTextJustificationsHorizontal.Center Then
            '    dX = 0.5 * CharBox.Width
            'ElseIf StrAlignH = dxxTextJustificationsHorizontal.Right Then
            '    dX = CharBox.Width
            'End If
            'Return aRec.AlignmentPoint(Alignment)
            ''Return CharBox.Vector(dX, dY, True)
            Select Case algnm
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.BaselineLeft
                    aPt = dxxRectanglePts.BaselineLeft
                Case dxxMTextAlignments.BaselineMiddle
                    aPt = dxxRectanglePts.BaselineCenter
                Case dxxMTextAlignments.BaselineRight
                    aPt = dxxRectanglePts.BaselineRight
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.BottomLeft
                    aPt = dxxRectanglePts.BottomLeft
                Case dxxMTextAlignments.BottomCenter
                    aPt = dxxRectanglePts.BottomCenter
                Case dxxMTextAlignments.BottomRight
                    aPt = dxxRectanglePts.BottomRight
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.MiddleLeft
                    aPt = dxxRectanglePts.MiddleLeft
                Case dxxMTextAlignments.MiddleCenter
                    aPt = dxxRectanglePts.MiddleCenter
                Case dxxMTextAlignments.MiddleRight
                    aPt = dxxRectanglePts.MiddleRight
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.TopLeft
                    aPt = dxxRectanglePts.TopLeft
                Case dxxMTextAlignments.TopCenter
                    aPt = dxxRectanglePts.TopCenter
                Case dxxMTextAlignments.TopRight
                    aPt = dxxRectanglePts.TopRight
            End Select
            Return aRec.PointV(aPt, aXOffset:=dX, aYOffset:=dY)

        End Function
        Friend Function ParseString(aString As String, ByRef rLineCount As Integer) As String
            '#1string to base the substrings collection on
            '^returns the substrings for the passed string
            rLineCount = 0
            If aString Is Nothing Then Return String.Empty
            Try
                Dim _rVal As String = aString
                Dim aCr As String 'As String * 1
                Dim i As Integer
                Dim j As Integer
                Dim sLen As String
                Dim cnt As Integer
                sLen = _rVal.Length
                If sLen = 0 Then Return String.Empty



                _rVal = Replace(_rVal, vbTab, New String(" ", 4))
                _rVal = Replace(_rVal, Chr(160), " ")
                _rVal = Replace(_rVal, vbLf, "\P")
                _rVal = Replace(_rVal, vbCr, "\P")
                _rVal = Replace(_rVal, vbFormFeed, "\P")
                '_rVal = Replace(_rVal, vbNewLine, "\P")
                _rVal = Replace(_rVal, vbCrLf, "\P")
                rLineCount = 1
                cnt = 0
                j = -1
                i = InStrRev(_rVal, "\P", j, True)
                Do While i > 0
                    If i > 0 Then
                        If i = 1 Then
                            If Not NoEffects Then rLineCount += 1
                        Else
                            aCr = Mid(_rVal, i - 1, 1)
                            If aCr <> "\" Then
                                If Not NoEffects Then rLineCount += 1
                            End If
                        End If
                        If i > 2 Then
                            j = i - 1
                            i = InStrRev(_rVal, "\P", j, True)
                        Else
                            Exit Do
                        End If
                    End If
                Loop
                Return _rVal

            Catch ex As Exception
                Return aString
            End Try
        End Function
#End Region 'Methods

    End Class


End Namespace
