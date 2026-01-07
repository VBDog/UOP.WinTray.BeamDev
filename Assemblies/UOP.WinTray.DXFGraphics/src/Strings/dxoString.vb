

Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoString
        Implements ICloneable

        Private _Characters As dxoCharacters
        Private _Bounds As dxfRectangle
#Region "Constructors"
        Public Sub New()
            Init()
        End Sub

        Friend Sub New(aString As TSTRING)
            Init()
            Alignment = aString.Alignment
            FitFactor = aString.FitFactor
            GroupCount = aString.GroupCount
            HasAlignments = aString.HasAlignments
            HasStacks = aString.HasStacks
            LineNo = aString.LineNo
            ScreenAlignment = aString.ScreenAlignment
            StackCount = aString.StackCount
            Vertical = aString.Vertical
            _Characters = New dxoCharacters(aString.Characters)
            _CharBox.CopyDirections(aString.CharacterBox, True, True)
        End Sub



        Friend Sub New(aCharBox As TCHARBOX)
            Init()

            'LeadChar = New TCHAR("")
            _Characters = New dxoCharacters()
            _CharBox.CopyDirections(aCharBox, True)
            'LeadChar.CharBox.CopyDirections(aCharBox)
        End Sub

        Friend Sub New(aCharBox As dxoCharBox)
            Init()
            If aCharBox Is Nothing Then Return
            'LeadChar = New TCHAR("")
            _Characters = New dxoCharacters()
            _CharBox.CopyDirections(aCharBox, True)
            'LeadChar.CharBox.CopyDirections(aCharBox)
        End Sub
        Private Sub Init()
            Alignment = dxxMTextAlignments.BaselineLeft
            _Characters = New dxoCharacters()
            _CharBox = New dxoCharBox()
            _Bounds = Nothing
            _LineSpacingStyle = dxxLineSpacingStyles.AtLeast
            _BaseFormats = New dxoCharFormat()
            _TextType = dxxTextTypes.Multiline
        End Sub
#End Region 'Constructors

#Region "Properties"

        Private _LineSpacingStyle As dxxLineSpacingStyles
        Public Property LineSpacingStyle As dxxLineSpacingStyles
            Get
                Return _LineSpacingStyle
            End Get
            Friend Set(value As dxxLineSpacingStyles)
                _LineSpacingStyle = value
            End Set
        End Property
        Public ReadOnly Property CharacterBox As dxoCharBox
            Get
                Return New dxoCharBox(_CharBox)
            End Get
        End Property


        Private _CharBox As dxoCharBox
        Friend Property CharBox As dxoCharBox
            Get
                Return _CharBox
            End Get
            Set(value As dxoCharBox)
                If _CharBox.CopyDirections(value, True, True) Then
                    _Bounds = Nothing
                End If
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

        Friend Function AlignPt(aAlignment As dxxMTextAlignments) As TVECTOR
            If aAlignment = dxxMTextAlignments.BaselineLeft Then Return CharBox.BasePtV
            Dim aPt As dxxRectanglePts
            Select Case aAlignment
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
            Return CharBox.PointV(aPt)
        End Function

        Public ReadOnly Property Bounds As dxfRectangle
            Get
                Return _Bounds
            End Get
        End Property

        Friend ReadOnly Property BoundingRectangleV As TPLANE
            Get
                Dim _rVal As TPLANE = CharBox.PlaneV(False)

                If CharacterCount <= 0 Then Return _rVal
                Dim vchars As List(Of dxoCharacter) = Characters.VisibleCharacters()
                _rVal.SetDimensions(0, 0)
                _rVal.Descent = 0
                For Each vchar As dxoCharacter In vchars
                    _rVal.ExpandToVectors(vchar.GetBounds().CornersV)
                Next

                _rVal.Name = $"Bounds of String [{ LineNo }]"
                Return _rVal
            End Get
        End Property

        Public ReadOnly Property CharacterCount As Integer
            '^returns the number of visible chracters in the string
            Get
                Return _Characters.CharacterCount
            End Get
        End Property
        Public Property LastChar As dxoCharacter
            Get
                Return _Characters.LastChar
            End Get
            Friend Set(value As dxoCharacter)
                _Characters.LastChar = value
            End Set
        End Property
        Public Property Descent As Double
            Get
                Return CharBox.Descent
            End Get
            Set(value As Double)
                CharBox.Descent = value
            End Set
        End Property

        Public ReadOnly Property Characters As dxoCharacters
            Get
                Return _Characters
            End Get
        End Property

        Public Property Ascent As Double
            Get
                Return CharBox.Ascent
            End Get
            Set(value As Double)
                CharBox.Ascent = value
            End Set
        End Property
        Private _Alignment As dxxMTextAlignments
        Public Property Alignment As dxxMTextAlignments
            Get
                Return _Alignment
            End Get
            Friend Set(value As dxxMTextAlignments)
                _Alignment = value
            End Set
        End Property
        Private _FitFactor As Double
        Public Property FitFactor As Double
            Get
                Return _FitFactor
            End Get
            Friend Set(value As Double)
                _FitFactor = value
            End Set
        End Property

        Private _GroupCount As Integer
        Public Property GroupCount As Integer
            Get
                Return _GroupCount
            End Get
            Friend Set(value As Integer)
                _GroupCount = value
            End Set
        End Property

        Private _LineNo As Integer
        Public Property LineNo As Integer
            Get
                Return _LineNo
            End Get
            Friend Set(value As Integer)
                _LineNo = value
            End Set
        End Property

        Private _StackCount As Integer
        Public Property StackCount As Integer
            Get
                Return _StackCount
            End Get
            Friend Set(value As Integer)
                _StackCount = value
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

        Public Property Vertical As Boolean
            Get
                Return BaseFormats.Vertical
            End Get
            Friend Set(value As Boolean)
                BaseFormats.Vertical = value
            End Set
        End Property
        Private _ScreenAlignment As dxxRectangularAlignments
        Public Property ScreenAlignment As dxxRectangularAlignments
            Get
                Return _ScreenAlignment
            End Get
            Friend Set(value As dxxRectangularAlignments)
                _ScreenAlignment = value
            End Set
        End Property

        Private _HasAlignments As Boolean
        Public Property HasAlignments As Boolean
            Get
                Return _HasAlignments
            End Get
            Friend Set(value As Boolean)
                _HasAlignments = value
            End Set
        End Property

        Private _HasStacks As Boolean
        Public Property HasStacks As Boolean
            Get
                Return _HasStacks
            End Get
            Friend Set(value As Boolean)
                _HasStacks = value
            End Set
        End Property
#End Region 'Properties

#Region "Methods"

        Public Function BaseLine(Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeLine
            Return New dxeLine(BaseLineV, aDisplaySettings:=aDisplaySettings)
        End Function

        Friend Function BaseLineV() As TLINE
            Return New TLINE(CharBox.BasePtV, CharBox.PointV(dxxRectanglePts.BaselineRight))
        End Function

        Friend Function ExtentPointsV() As TVECTORS
            Dim _rVal As New TVECTORS(0)
            Dim visChars As List(Of dxoCharacter) = Characters.VisibleCharacters()
            If visChars.Count <= 0 Then Return _rVal

            For Each visChar As dxoCharacter In visChars
                _rVal.Append(visChar.ExtentPointsV())
            Next

            Return _rVal

        End Function

        Public Function GetBounds(Optional bRegen As Boolean = False) As dxfRectangle
            If _Bounds IsNot Nothing Then
                If _Bounds.Area <= 0 Then bRegen = True
            End If
            If bRegen Then
                _Bounds = Nothing
            End If
            If _Bounds Is Nothing Then
                UpdateBounds()
            End If
            Return _Bounds
        End Function

        Friend Function Structure_Get() As TSTRING
            Dim _rVal As New TSTRING() With
            {
              .Alignment = Alignment,
            .FitFactor = FitFactor,
            .GroupCount = GroupCount,
            .HasAlignments = HasAlignments,
            .HasStacks = HasStacks,
            .LineNo = LineNo,
            .ScreenAlignment = ScreenAlignment,
            .StackCount = StackCount,
            .Vertical = Vertical,
            .Characters = New TCHARS(_Characters)
            }
            Return _rVal
        End Function

        Friend Sub AddChar(aChar As TCHAR)

            Dim newmem As New dxoCharacter(aChar)

            If Characters.Count <= 0 Or aChar.IsFormatCode Then
                newmem.MoveFromTo(newmem.CharBox.EndPtV, CharBox.BasePtV)
            Else
                Dim lChar As dxoCharacter = LastChar
                If lChar IsNot Nothing Then
                    newmem.MoveFromTo(aChar.CharBox.StartPt, lChar.CharBox.EndPtV)
                End If

            End If
            _Characters.Add(newmem)
        End Sub

        Friend Sub AddChar(aChar As dxoCharacter)
            If aChar IsNot Nothing Then _Characters.Add(aChar)
        End Sub

        Public Function Character(aIndex As Integer) As dxoCharacter
            '^gets or sets a visible character
            Return _Characters.Character(aIndex)
        End Function

        Public Function GetByGroupIndex(aGroupID As Integer, aStartID As Integer, ByRef rLastID As Integer) As dxoString
            Dim i As Integer
            Dim rStr As dxoString = Clone(bNoChars:=True)
            Dim aChar As dxoCharacter
            rLastID = 0
            If aGroupID < 0 Then aGroupID = 1
            Dim vischars As List(Of dxoCharacter) = Characters.VisibleCharacters()

            For i = aStartID To vischars.Count
                If i < vischars.Count Then Continue For
                If i > vischars.Count Then Exit For

                aChar = vischars.Item(i - 1)
                If aChar.GroupIndex = aGroupID Then
                    rStr.Characters.Add(aChar.Clone())
                    rLastID = i
                Else
                    If aChar.GroupIndex > aGroupID Then
                        Exit For
                    End If
                End If
            Next i
            Return rStr
        End Function

        Friend Function CopyDirections(aPlane As TPLANE) As Boolean

            If TPLANE.IsNull(aPlane) Then Return False
            Dim _rVal As Boolean = CharBox.CopyDirections(aPlane)
            _Characters.CopyDirections(CharBox)
            If _rVal Then _Bounds = Nothing
            Return _rVal
        End Function
        Friend Function CopyDirections(aCharBox As TCHARBOX) As Boolean

            If TPLANE.IsNull(aCharBox) Then Return False
            Dim _rVal As Boolean = CharBox.CopyDirections(aCharBox)
            _Characters.CopyDirections(CharBox)
            If _rVal Then _Bounds = Nothing
            Return _rVal
        End Function


        Friend Function CopyDirections(aCharBox As dxoCharBox) As Boolean

            If TPLANE.IsNull(aCharBox) Then Return False
            Dim _rVal As Boolean = CharBox.CopyDirections(aCharBox)
            _Characters.CopyDirections(CharBox)
            If _rVal Then _Bounds = Nothing
            Return _rVal
        End Function


        Friend Sub Translate(aTranslation As TVECTOR, Optional bResetTanslation As Boolean = False)
            If TVECTOR.IsNull(aTranslation) Then Return
            _Characters.Translate(aTranslation)
            _CharBox.Translate(aTranslation)
            If _Bounds IsNot Nothing Then _Bounds.Translate(aTranslation)
            'LeadChar.Translate(aTranslation)
            If bResetTanslation Then aTranslation.SetCoordinates(0, 0, 0, -1)
        End Sub
        Friend Function MoveFromTo(aFromPt As TVECTOR, aToPt As TVECTOR) As Double
            Dim _rVal As Double = 0
            Dim aDir As TVECTOR
            aDir = aFromPt.DirectionTo(aToPt, False, rDistance:=_rVal)
            If _rVal <> 0 Then Translate(aDir * _rVal)
            Return _rVal
        End Function

        Public Overrides Function ToString() As String
            Dim sChrs As String = Characters.CharacterString()
            If sChrs.Length = 0 Then
                Return $"dxoString({ LineNo })"
            Else
                Return $"dxoString({ LineNo }) - { sChrs}"
            End If
        End Function
        Friend Sub Rescale(aScaleFactor As Double, aRefPt As TVECTOR)
            _Characters.Rescale(aScaleFactor, aRefPt)
            _CharBox.Rescale(aScaleFactor, aRefPt)
            If _Bounds IsNot Nothing Then
                _Bounds.RescaleV(aScaleFactor, aRefPt)
            End If
            'LeadChar.Rescale(aScaleFactor, aRefPt)
        End Sub
        Friend Sub Rotate(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False)
            'LeadChar.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
            _Characters.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
            _CharBox.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
            If _Bounds IsNot Nothing Then
                _Bounds.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, True, True, True)
            End If

        End Sub

        Friend Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, bMirrorDirections As Boolean, Optional bSuppressCheck As Boolean = False)

            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return
            End If
            _CharBox.Mirror(aSP, aEP, True, bMirrorDirections, True)
            _Characters.Mirror(aSP, aEP, bMirrorDirections, True)
        End Sub
        Private Function UpdateBounds_H(Optional bSetCharIndexes As Boolean = False) As dxfRectangle

            _Bounds = New dxfRectangle(_CharBox.PlaneV(False))

            'get the visible characters aligned on the baseline
            Dim stacks As Boolean = False
            Dim stackGroups As List(Of List(Of dxoCharacter)) = Nothing
            Dim visChars As List(Of dxoCharacter) = _Characters.GetVisibleCharacters(_CharBox, bVerticalAlignment:=False, bSetCharIndexes:=bSetCharIndexes, rStacks:=stacks, rStackedChars:=stackGroups)
            If visChars.Count <= 0 Then Return _Bounds

            'there should always be the null char as the lead char

            Dim v1 As TVECTOR
            Dim v2 As TVECTOR

            Dim myPl As TPLANE = _CharBox.PlaneV(True)
            Dim baseHt As Double = _Characters.Item(0).CharHeight

            Dim myBasePt As TVECTOR = CharBox.BasePtV

            _CharBox.Width = 0
            _Bounds.Strukture = myPl.Clone(True, "BoundingRectangle")
            _CharBox.ObliqueAngle = 0
            If TextType = dxxTextTypes.Multiline Then _CharBox.Descent = 0

            Dim vi As Integer = 0
            Dim midLine As New TLINE("")
            Dim topLine As New TLINE("")
            Dim vLine As TLINE = _CharBox.LineV(0, 10)

            For Each visChar As dxoCharacter In visChars
                vi += 1
                Dim charBounds As dxfRectangle = visChar.GetBounds(True)
                If bSetCharIndexes Then visChar.LineIndex = vi

                If vi = 1 Then


                    midLine = visChar.CharBox.LineH(aYOrdinate:=0.5 * visChar.Ascent, aLength:=10)
                    topLine = visChar.CharBox.LineH(aYOrdinate:=visChar.Ascent, aLength:=10)

                    _CharBox.Ascent = visChar.Ascent
                    Continue For

                End If

                'align the characters vertical with respect to the mid line
                If visChar.CharAlign = dxxCharacterAlignments.Center Or visChar.CharAlign = dxxCharacterAlignments.Top Then
                    If visChar.CharAlign = dxxCharacterAlignments.Center Then
                        v1 = visChar.CharBox.AscentCenterV
                        v2 = v1.ProjectedTo(midLine)
                    Else
                        v1 = visChar.CharBox.TopCenterV
                        v2 = v1.ProjectedTo(topLine)
                    End If
                    visChar.MoveFromTo(v1, v2)

                End If

            Next

            Dim skipChars As New List(Of dxoCharacter)()
            Dim numerators As New List(Of dxoCharacter)()
            'apply stacking
            If stacks Then
                Dim group As List(Of dxoCharacter)
                For i As Integer = 1 To stackGroups.Count
                    group = stackGroups.Item(i - 1)
                    If group.Count <= 0 Then Continue For

                    Dim stackchars As List(Of dxoCharacter) = group  'includes top and bottom group
                    Dim c1 As dxoCharacter = group.Item(0)
                    Dim idx As Integer = visChars.IndexOf(c1)
                    Dim alignTo As dxoCharacter = Nothing
                    Dim cline As TLINE = midLine

                    If idx > 0 Then

                        alignTo = visChars.Item(idx - 1)
                        cline = alignTo.CharBox.MidLineV
                    End If
                    Dim thetail As List(Of dxoCharacter) = visChars.FindAll(Function(x) visChars.IndexOf(x) > visChars.IndexOf(group.Item(group.Count - 1)))
                    Dim theleader As List(Of dxoCharacter) = visChars.FindAll(Function(x) visChars.IndexOf(x) < visChars.IndexOf(c1))

                    'If theleader.Count > 0 Then
                    '    Dim ldrBox As dxoCharBox = dxoCharacters.GetTextRectangle(theleader)
                    '    midLine = ldrBox.LineH(0.5 * ldrBox.Ascent)
                    '    'Dim lchar As dxoCharacter = theleader.Last()
                    '    'midLine = lchar.CharBox.LineH(0.5 * lchar.Ascent)
                    'End If

                    Dim d1 As Double = 0
                    Dim trans As New TVECTOR(0, 0, 0)
                    Dim c2 As dxoCharacter = Nothing
                    Dim nextgroup As List(Of dxoCharacter) = Nothing
                    If i < stackGroups.Count Then
                        nextgroup = stackGroups.Item(i)
                        stackchars.AddRange(nextgroup)
                        If nextgroup.Count <= 0 Then nextgroup = Nothing Else c2 = nextgroup.Item(0)
                    End If
                    Dim groupBounds As dxfRectangle = dxoCharacters.CharBounds(group)
                    Dim nextBounds As dxfRectangle = Nothing
                    Dim stackBound As dxfRectangle
                    Dim dX As Double = 0

                    '=============================================================
                    If c1.StackStyle = dxxCharacterStackStyles.SubScript Then
                        '=============================================================
                        v1 = groupBounds.TopLeftV
                        d1 = 0.1666666 * groupBounds.Height
                        v2 = v1.ProjectedTo(cline) - myPl.YDirection * d1
                        v1 = v1.DirectionTo(v2, rDistance:=d1)

                        trans = v1 * d1
                        If alignTo IsNot Nothing Then

                            dX = -c1.GetBounds().TopLeftV.DistanceTo(alignTo.CharBox.EdgeV(dxxRectangleLines.RightEdge))
                            trans += myPl.XDirection * dX
                        End If
                        skipChars.AddRange(group)
                        If Not TVECTOR.IsNull(trans) Then dxoCharacters.MoveChars(group, trans)


                    End If

                    '=============================================================
                    If c1.StackStyle = dxxCharacterStackStyles.SuperScript Then
                        '=============================================================
                        v1 = groupBounds.BottomLeftV
                        d1 = 0.1666666 * groupBounds.Height
                        v2 = v1.ProjectedTo(cline) + myPl.YDirection * d1
                        v1 = v1.DirectionTo(v2, rDistance:=d1)

                        trans = v1 * d1
                        If alignTo IsNot Nothing Then
                            dX = -c1.GetBounds().TopLeftV.DistanceTo(alignTo.CharBox.EdgeV(dxxRectangleLines.RightEdge))
                            trans += myPl.XDirection * dX
                        End If
                        skipChars.AddRange(group)
                        dxoCharacters.MoveChars(group, trans)

                        If LineSpacingStyle = dxxLineSpacingStyles.Exact Then
                            numerators.AddRange(group)
                        End If
                    End If

                    '=============================================================
                    If c1.StackStyle = dxxCharacterStackStyles.Horizontal Or c1.StackStyle = dxxCharacterStackStyles.Tolerance Or c1.StackStyle = dxxCharacterStackStyles.DecimalStack Then
                        '=============================================================

                        'move the top grop 
                        d1 = groupBounds.BottomLeftV.DistanceTo(midLine) + 0.2 * c1.CharHeight
                        trans = myPl.YDirection * d1
                        If alignTo IsNot Nothing Then
                            d1 = -c1.GetBounds().TopLeftV.DistanceTo(alignTo.CharBox.EdgeV(dxxRectangleLines.RightEdge))
                            trans += myPl.XDirection * d1
                        End If

                        dxoCharacters.MoveChars(group, trans)
                        groupBounds.Translate(trans)

                        If c2 IsNot Nothing Then

                            thetail = visChars.FindAll(Function(x) visChars.IndexOf(x) > visChars.IndexOf(nextgroup.Item(nextgroup.Count - 1)))
                            nextBounds = dxoCharacters.CharBounds(nextgroup)

                            d1 = -nextBounds.TopLeftV.DistanceTo(midLine) - 0.2 * c1.CharHeight
                            trans = myPl.YDirection * d1
                            If alignTo IsNot Nothing Then
                                d1 = -nextBounds.TopLeftV.DistanceTo(alignTo.CharBox.EdgeV(dxxRectangleLines.RightEdge))
                                trans += myPl.XDirection * d1
                            End If

                            dxoCharacters.MoveChars(nextgroup, trans)
                            nextBounds.Translate(trans)
                            i += 1
                        End If
                        'add the accent line
                        stackBound = dxoCharacters.CharBounds(stackchars, Nothing, False)
                        If c1.StackStyle <> dxxCharacterStackStyles.Tolerance And c1.StackStyle <> dxxCharacterStackStyles.DecimalStack Then
                            v1 = stackBound.MiddleLeftV.WithRespectTo(c1.PlaneV)
                            v2 = stackBound.MiddleRightV.WithRespectTo(c1.PlaneV)
                            c1.AccentPaths.AddLine(New TPOINT(v1.X, v1.Y), New TPOINT(v2.X, v2.Y))
                        End If


                        If thetail.Count > 0 Then
                            Dim tailBound As dxfRectangle = dxoCharacters.CharBounds(thetail, Nothing, True)
                            dX = -tailBound.TopLeftV.DistanceTo(stackBound.RightEdgeV)

                        End If

                        If LineSpacingStyle = dxxLineSpacingStyles.Exact Then
                            numerators.AddRange(group)
                        End If
                    End If

                    '=============================================================
                    If c1.StackStyle = dxxCharacterStackStyles.Diagonal Then
                        '=============================================================

                        'move the top group 
                        d1 = groupBounds.BottomLeftV.DistanceTo(midLine)
                        trans = myPl.YDirection * d1
                        If alignTo IsNot Nothing Then
                            d1 = -c1.GetBounds().TopLeftV.DistanceTo(alignTo.CharBox.EdgeV(dxxRectangleLines.RightEdge))
                            trans += myPl.XDirection * d1
                        End If

                        dxoCharacters.MoveChars(group, trans)
                        groupBounds.Translate(trans)

                        If c2 IsNot Nothing Then
                            thetail = visChars.FindAll(Function(x) visChars.IndexOf(x) > visChars.IndexOf(nextgroup.Item(nextgroup.Count - 1)))
                            nextBounds = dxoCharacters.CharBounds(nextgroup)

                            d1 = -nextBounds.TopLeftV.DistanceTo(midLine)
                            trans = myPl.YDirection * d1

                            d1 = nextgroup.Last().CharBox.TopLeftV.DistanceTo(group.Last().CharBox.EdgeV(dxxRectangleLines.RightEdge))
                            trans += myPl.XDirection * d1

                            If alignTo IsNot Nothing Then
                                d1 = -nextBounds.TopLeftV.DistanceTo(alignTo.CharBox.EdgeV(dxxRectangleLines.RightEdge))
                                trans += myPl.XDirection * d1
                            End If

                            dxoCharacters.MoveChars(nextgroup, trans)
                            nextBounds.Translate(trans)

                            'add the accent line
                            Dim c3 As dxoCharacter = group.Last()
                            v1 = c1.ExtentPointsV().PlanarCenter(c1.PlaneV)
                            v2 = c3.ExtentPointsV().PlanarCenter(c3.PlaneV)

                            v1 = v1.MidPt(v2)
                            v2 = myPl.PolarVector(c2.CharHeight, 180 + 55, False, 0, New dxfVector(v1)).WithRespectTo(c1.PlaneV)
                            Dim v3 As TVECTOR = myPl.PolarVector(c1.CharHeight, 55, False, 0, New dxfVector(v1)).WithRespectTo(c1.PlaneV)
                            v1 = v1.WithRespectTo(c1.PlaneV)
                            c1.AccentPaths.AddLine(New TPOINT(v3.X, v3.Y), New TPOINT(v2.X, v2.Y))

                            If LineSpacingStyle = dxxLineSpacingStyles.Exact Then
                                numerators.AddRange(group)
                            End If

                            i += 1
                        End If


                        stackBound = dxoCharacters.CharBounds(stackchars, Nothing, False)
                        'v1 = stackBound.BottomLeftV.WithRespectTo(c1.PlaneV)
                        'v2 = stackBound.TopRightV.WithRespectTo(c1.PlaneV)

                        'c1.AccentPaths.AddLine(New TPOINT(v1.X, v1.Y), New TPOINT(v2.X, v2.Y))


                        If thetail.Count > 0 Then
                            Dim tailBound As dxfRectangle = dxoCharacters.CharBounds(thetail, Nothing, True)
                            dX = -tailBound.TopLeftV.DistanceTo(stackBound.RightEdgeV)

                        End If

                    End If

                    If dX <> 0 Then
                        dxoCharacters.MoveChars(thetail, myPl.XDirection * dX)
                    End If

                Next

            End If

            'If TextType = dxxTextTypes.Multiline Then
            _CharBox = dxoCharacters.GetTextRectangle(visChars, New dxfVector(myBasePt), numerators, TextType <> dxxTextTypes.Multiline)
            _Bounds = dxoCharacters.CharBounds(visChars, bCharBoxBounds:=TextType <> dxxTextTypes.Multiline)

            'Else
            '    _CharBox = dxoCharacters.GetTextRectangle(visChars, New dxfVector(myBasePt))
            '    _Bounds = dxoCharacters.CharBounds(visChars, bCharBoxBounds:=False)

            'End If


            '_CharBox.Width = _Bounds.Width
            '_CharBox.BasePtV = myBasePt
            Return _Bounds


        End Function

        Private Function UpdateBounds_V(Optional bSetCharIndexes As Boolean = False) As dxfRectangle

            _Bounds = New dxfRectangle(_CharBox.PlaneV(False))

            'get the visible characters aligned on the baseline
            Dim stacks As Boolean = False
            Dim stackGroups As List(Of List(Of dxoCharacter)) = Nothing
            Dim visChars As List(Of dxoCharacter) = _Characters.GetVisibleCharacters(_CharBox, bVerticalAlignment:=False, bSetCharIndexes:=bSetCharIndexes, rStacks:=stacks, rStackedChars:=stackGroups)
            If visChars.Count <= 0 Then Return _Bounds

            'there should always be the null char as the lead char

            Dim v1 As TVECTOR
            Dim v2 As TVECTOR

            Dim myPl As TPLANE = _CharBox.PlaneV(True)
            Dim baseHt As Double = BaseFormats.CharHeight
            Dim myBasePt As TVECTOR = CharBox.BasePtV
            Dim lastchar As dxoCharacter
            _CharBox.Width = 0
            _Bounds.Strukture = myPl.Clone(True, "BoundingRectangle")
            _CharBox.ObliqueAngle = 0
            If TextType = dxxTextTypes.Multiline Then _CharBox.Descent = 0

            Dim vi As Integer = 0

            Dim botLine As New TLINE("")
            Dim vLine As TLINE = _CharBox.LineV(0, 10)
            Dim charBounds As dxfRectangle
            Dim charstep As Double = -0.43 * baseHt
            For Each visChar As dxoCharacter In visChars
                vi += 1
                charBounds = visChar.GetBounds(True)
                If bSetCharIndexes Then visChar.LineIndex = vi


                If vi = 1 Then



                    _CharBox.Width = visChar.Width
                    _CharBox.Ascent = visChar.Ascent
                    visChar.MoveFromTo(visChar.StartPtV, CharBox.BasePtV)
                    lastchar = visChar
                    botLine = charBounds.LineH(-0.5 * charBounds.Height + charstep, 10)

                    Continue For

                End If
                v1 = charBounds.TopLeftV
                v2 = v1.ProjectedTo(vLine)
                visChar.MoveFromTo(v1, v2)

                v1 = charBounds.TopLeftV
                v2 = v1.ProjectedTo(botLine)
                visChar.MoveFromTo(v1, v2)

                v1 = charBounds.BottomRightV.WithRespectTo(myPl)
                If v1.Y < 0 And Math.Abs(v1.Y) < CharBox.Descent Then CharBox.Descent = Math.Abs(v1.Y)
                If v1.X > CharBox.Width Then CharBox.Width = v1.X

                lastchar = visChar
                botLine = charBounds.LineH(-0.5 * charBounds.Height + charstep, 10)

            Next
            _Bounds = dxoCharacters.CharBounds(visChars, bCharBoxBounds:=False)
            vLine = New TLINE(_Bounds.TopCenterV, _Bounds.BottomCenterV)

            For Each visChar As dxoCharacter In visChars
                charBounds = visChar.GetBounds()
                v1 = charBounds.CenterV
                v2 = v1.ProjectedTo(vLine)
                visChar.MoveFromTo(v1, v2)
                v1 = charBounds.BottomRightV.WithRespectTo(myPl)
                If v1.Y < 0 And Math.Abs(v1.Y) < CharBox.Descent Then CharBox.Descent = Math.Abs(v1.Y)

                If v1.X > CharBox.Width Then CharBox.Width = v1.X
            Next

            Return _Bounds


        End Function


        Public Function UpdateBounds(Optional bSetCharIndexes As Boolean = True) As dxfRectangle

            _Bounds = New dxfRectangle(_CharBox.PlaneV(False))

            If _Characters Is Nothing Then Return _Bounds

            If _Characters.Count <= 0 Then Return _Bounds


            If Not Vertical Then
                UpdateBounds_H(bSetCharIndexes)
            Else
                UpdateBounds_V(bSetCharIndexes)
            End If
            Return _Bounds

            Dim fChar As dxoCharacter = _Characters.Item(0)
            _CharBox.Ascent = fChar.CharBox.Ascent
            _CharBox.Descent = fChar.CharBox.Descent
            _CharBox.Width = _CharBox.Ascent

            Dim lastChar As dxoCharacter = fChar
            Dim visChars As List(Of dxoCharacter) = _Characters.VisibleCharacters



            Dim aChar As dxoCharacter = fChar
            Dim bChar As dxoCharacter
            If aChar IsNot Nothing Then aChar.CopyDirections(_CharBox)
            Dim stkSty As dxxCharacterStackStyles
            'there should always be the null char as the lead char
            Dim chrCnt As Integer = Characters.Count
            Dim ci As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim d1 As Double
            Dim aDir As TVECTOR
            Dim aBnds As dxfRectangle = Nothing
            Dim bBnds As dxfRectangle = Nothing
            Dim tBnds As dxfRectangle = Nothing


            Dim bFirstChar As Boolean

            Dim myPl As TPLANE = _CharBox.PlaneV(True)

            Dim baseHt As Double = aChar.CharHeight
            Dim basePt As TVECTOR = _CharBox.BasePtV
            _CharBox.Width = 0

            _CharBox.ObliqueAngle = 0

            Dim baseLine As TLINE = _CharBox.LineH(0, aLength:=10)
            Dim midLine As TLINE = baseLine.Clone()
            Dim topLine As TLINE = baseLine.Clone()

            Dim dX As Double = 0
            Dim dY As Double = 0

            Dim bFlg As Boolean
            Dim cLine As New TLINE(_CharBox.Vector(0, 0.5 * baseHt), _CharBox.Vector(100, 0.5 * baseHt))
            Dim tLine As New TLINE(_CharBox.Vector(0, baseHt), _CharBox.Vector(100, baseHt))
            Dim vLine As New TLINE(_CharBox.Vector(0, 0), _CharBox.Vector(0, 100, True))
            Dim aIDs As New List(Of Integer)
            Dim aSet As New dxoCharacters()
            Dim bIDs As New List(Of Integer)
            Dim bSet As New dxoCharacters()

            Dim aBox As TCHARBOX
            Dim bBox As TCHARBOX
            Dim cBox As TCHARBOX
            Dim tBox As TCHARBOX
            Dim eVecs As TVECTORS
            Dim ascntBox As TCHARBOX
            Dim superScript As Boolean
            Dim SubScript As Boolean
            Dim lnidx As Integer = 0
            If CharacterCount > 0 Then
                _CharBox.Ascent = 0
                _CharBox.Descent = 0
            Else
                d1 = aChar.CharHeight / aChar.Ascent
                _CharBox.Ascent = aChar.Ascent * d1
                _CharBox.Descent = aChar.Descent * d1
                Return _Bounds
            End If

            _Bounds.Strukture = myPl.Clone(True, "BoundingRectangle")
            aDir = fChar.CharBox.BasePtV.DirectionTo(basePt, rDistance:=d1)
            If d1 <> 0 Then
                fChar.Translate(aDir * d1)
            End If

            _CharBox.Descent = fChar.Descent

            Dim vi As Integer = -1

            For Each visChar In visChars
                vi += 1
                visChar.CopyDirections(_CharBox)
                aBnds = visChar.GetBounds(True) 'update the characters bounding rectangle
                If vi = 0 Then

                    _CharBox.Ascent = visChar.Ascent
                    _CharBox.Descent = visChar.Descent
                    midLine += myPl.YDirection * (0.5 * _CharBox.Ascent)  '.Translate(myPl.YDirection * 0.5 * visChar.Ascent)
                    topLine += myPl.YDirection * _CharBox.Ascent
                    v1 = visChar.CharBox.BasePtV
                    v2 = basePt

                    aDir = v1.DirectionTo(v2, rDistance:=d1)
                    If d1 <> 0 Then
                        visChar.Translate(aDir * d1)
                        'dX = d1
                    End If

                Else
                    If dX <> 0 Then
                        visChar.Translate(myPl.XDirection * dX)
                    End If

                    lastChar = visChars.Item(vi - 1)
                    If Vertical Then
                        v1 = visChar.CharBox.TopLeftV
                        v2 = lastChar.CharBox.BottomLeftV
                    Else
                        v1 = visChar.CharBox.BasePtV
                        v2 = lastChar.CharBox.BaselineRightV
                    End If
                    aDir = v1.DirectionTo(v2, rDistance:=d1)
                    If d1 <> 0 Then
                        visChar.Translate(aDir * d1)
                    End If
                    basePt = v2
                End If


                If vi = 0 Then
                    Continue For
                End If

                If Not Vertical Then
                    If visChar.CharAlign = dxxCharacterAlignments.Center Or visChar.CharAlign = dxxCharacterAlignments.Top Then
                        If visChar.CharAlign = dxxCharacterAlignments.Center Then
                            v1 = visChar.CharBox.AscentCenterV
                            v2 = v1.ProjectedTo(midLine)
                        Else
                            v1 = visChar.CharBox.TopCenterV
                            v2 = v1.ProjectedTo(topLine)
                        End If

                        aDir = v1.DirectionTo(v2, rDistance:=d1)
                        If d1 <> 0 Then
                            visChar.Translate(aDir * d1)
                        End If
                    End If

                    v1 = aBnds.TopLeftV.WithRespectTo(_CharBox)
                    If v1.Y > _CharBox.Ascent Then _CharBox.Ascent = v1.Y
                    v1 = aBnds.BottomLeftV.WithRespectTo(_CharBox)
                    If v1.Y < 0 And Math.Abs(v1.Y) > _CharBox.Descent Then _CharBox.Descent = Math.Abs(v1.Y)
                End If
            Next



            _Bounds = _Characters.ExtentRectangle(myPl)
            _CharBox.Width = _Bounds.Width
            Dim aPl As TPLANE = _CharBox.PlaneV(True)
            aPl.Origin = _Bounds.BottomLeftV
            v1 = _CharBox.BasePtV.WithRespectTo(aPl)
            If v1.X < 0 Then
                _CharBox.Width += Math.Abs(v1.X)
            End If
            _CharBox.Descent = Math.Abs(v1.Y)
            Return _Bounds
            'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            If Vertical Then
                'determine overall width for vertical strings


                For ci = 1 To chrCnt
                    aChar = Characters.Item(ci - 1)
                    If Not aChar.IsFormatCode Then
                        vi += 1

                        If aChar.GetBounds().Width > _CharBox.Width Then _CharBox.Width = aChar.GetBounds().Width
                        If vi = 1 Then
                            v1 = aChar.CharBox.BasePtV
                            v2 = dxfProjections.ToLine(v1, baseLine, rDistance:=d1)
                            If d1 <> 0 Then
                                aDir = v1.DirectionTo(v2)
                                If aDir.Equals(_CharBox.YDirectionV) Then dY = d1 Else dY = -d1   'put the first char on the baseline
                            End If
                        End If
                    End If
                Next ci
                v1 = _CharBox.BasePtV
                v1 += _CharBox.XDirectionV * _CharBox.Width / 2
                v2 = v1 + (_CharBox.YDirectionV * 100)
                cLine = New TLINE(v1, v2)
            End If


            For ci = 1 To chrCnt
                aChar = _Characters.Item(ci - 1)
                aChar.CopyDirections(_CharBox)
                If Not aChar.IsFormatCode Then
                    lnidx += 1
                    If bSetCharIndexes Then aChar.LineIndex = lnidx
                    aChar.GetBounds()
                End If
                bFirstChar = lnidx = 1

                If Not Vertical Then

                    '========================================
                    'HORIZONTAL TEXT
                    '========================================
                    'adjust to align relative to baseline
                    If Not aChar.IsFormatCode Then


                        If bFirstChar Then
                            'force first char to the left edge of the text box
                            v1 = aChar.CharBox.PointV(dxxRectanglePts.TopLeft)
                            v2 = dxfProjections.ToLine(v1, vLine, rOrthoDirection:=aDir, rDistance:=d1)

                            If d1 <> 0 And aDir.Equals(myPl.XDirection, bCompareInverse:=True, aPrecis:=3, rIsInverseEqual:=bFlg) Then
                                If Not bFlg Then dX = d1 Else dX = -d1
                            End If

                            v1 = aChar.GetBounds().TopLeftV
                            v2 = dxfProjections.ToLine(v1, vLine, rOrthoDirection:=aDir, rDistance:=d1)
                            If d1 <> 0 Then
                                aChar.Translate(aDir * d1)

                                _CharBox.BasePtV = aChar.CharBox.BasePtV
                                myPl = _CharBox.PlaneV(True)
                                _Bounds.Strukture = myPl.Clone(True, "BoundingRectangle")
                            End If
                        Else
                            If dX <> 0 Then
                                aChar.Translate(myPl.XDirection * dX)
                            End If

                        End If

                        aIDs = New List(Of Integer)({ci})
                        aSet.Clear() : aSet.Add(aChar)
                        bSet.Clear() : bIDs.Clear()
                        stkSty = dxxCharacterStackStyles.None
                        bFlg = False
                        'collect stacked text to arrange collectively
                        If aChar.StackID <> 0 Then
                            stkSty = aChar.StackStyle
                            superScript = stkSty = dxxCharacterStackStyles.SuperScript
                            SubScript = stkSty = dxxCharacterStackStyles.SubScript
                            If SubScript Then
                                aSet.Clear()
                                bSet.Add(aChar)
                                aIDs.Clear() : bIDs.Add(ci)
                            End If
                            For j = ci + 1 To chrCnt
                                bChar = _Characters.Item(j - 1)
                                If bChar.StackID = aChar.StackID Then
                                    If Not bChar.IsFormatCode Then
                                        lnidx += 1
                                        If bSetCharIndexes Then aChar.LineIndex = lnidx
                                        If SubScript Then
                                            bSet.Add(bChar) : bIDs.Add(j)
                                        Else
                                            aSet.Add(bChar) : aIDs.Add(j)
                                        End If
                                    End If
                                    ci += 1
                                ElseIf bChar.StackID = aChar.StackID + 1 Then
                                    If Not bChar.IsFormatCode Then
                                        lnidx += 1
                                        If bSetCharIndexes Then aChar.LineIndex = lnidx
                                        bSet.Add(bChar) : bIDs.Add(j)
                                    End If
                                    ci += 1
                                Else
                                    Exit For
                                End If
                            Next j
                        End If
                        ' char alignment
                        If stkSty = dxxCharacterStackStyles.SubScript Then
                            aBnds = bSet.ExtentRectangle(myPl) : aBox = New TCHARBOX(CharBox)
                            bBox = aBox.Clone : bBnds = aBnds.Clone
                        Else
                            aBnds = aSet.ExtentRectangle(myPl) : aBox = New TCHARBOX(CharBox)
                            If bSet.CharacterCount > 0 Then
                                bBnds = bSet.ExtentRectangle(myPl) : bBox = New TCHARBOX(CharBox)
                            End If
                        End If
                        tBnds = aBnds.Clone : tBox = aBox.Clone
                        ascntBox = aBox.Clone : ascntBox.Descent = 0 ': ascntBox.Ascent = aBnds.Point(dxxRectanglePts.TopLeft).WithRespectTo(aBox).Y
                        ascntBox.Ascent = aChar.CharHeight
                        'align stacks
                        If stkSty <> dxxCharacterStackStyles.None Then
                            'align the bottom set below the top set of chars
                            v1 = aBnds.BottomLeftV
                            v2 = bBnds.TopLeftV
                            v1 += myPl.YDirection * -0.07 * baseHt
                            If stkSty = dxxCharacterStackStyles.Diagonal Then
                                'shift the bottom set right by the length of the top one
                                v1 += myPl.XDirection * aBnds.Width
                            Else
                                'center the bottom on the top one
                                v1 += myPl.XDirection * (0.5 * aBnds.Width - 0.5 * bBnds.Width)
                                If bBnds.Width > aBnds.Width Then
                                    v3 = myPl.XDirection * (0.5 * bBnds.Width - 0.5 * aBnds.Width)
                                    bFlg = True
                                    aSet.Translate(v3) : aBox.BasePt += v3 : aBnds.Translate(v3)
                                    bSet.Translate(v3) : bBox.BasePt += v3 : bBnds.Translate(v3)
                                    ascntBox.BasePt += v3 : v1 += v3 : v2 += v3
                                    tBnds.Translate(v3) : tBox.BasePt += v3
                                End If
                            End If
                            aDir = v2.DirectionTo(v1, False, rDistance:=d1)
                            v1 = aDir * d1
                            bSet.Translate(v1) : bBnds.CenterV += v1 : bBox.BasePt += v1
                            aBnds.ExpandToVectors(bBnds.Corners)
                            ascntBox = bBox.Clone : ascntBox.Descent = 0
                            ascntBox.Ascent = aBnds.Height - bBox.Descent
                            If bFlg Then
                                'make the bBox the aBox by swapping because it is the longer group
                                cBox = aBox : aBox = bBox : bBox = cBox
                            End If
                        End If
                        v2 = basePt.Clone : v1 = aBox.BasePt
                        aDir = v1.DirectionTo(v2, False, rDistance:=d1)

                        If d1 <> 0 Then
                            'v1 = aDir * d1
                            'aSet.Translate(v1) : aBox.BasePt += v1 : aBnds.Translate(v1)
                            'ascntBox.BasePt += v1 : tBnds.Translate(v1) : tBox.BasePt += v1
                            'If bSet.CharacterCount > 0 Then
                            '    bSet.Translate(v1) : bBox.BasePt += v1 : bBnds.Translate(v1)
                            'End If
                        End If


                        'vertical alignment
                        If aChar.CharAlign = dxxCharacterAlignments.Center Then
                            v1 = ascntBox.Center(True) : v2 = v1.ProjectedTo(cLine)
                            aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        ElseIf aChar.CharAlign = dxxCharacterAlignments.Top Then
                            v1 = ascntBox.Point(dxxRectanglePts.TopLeft) : v2 = v1.ProjectedTo(tLine)
                            aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        Else
                            v1 = ascntBox.Point(dxxRectanglePts.BottomLeft) : v2 = v1.ProjectedTo(baseLine)
                            aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        End If
                        If d1 <> 0 Then
                            v1 = aDir * d1
                            aSet.Translate(v1) : aBox.BasePt += v1 : aBnds.Translate(v1)
                            ascntBox.BasePt += v1 : tBnds.Translate(v1) : tBox.BasePt += v1
                            If bSet.CharacterCount > 0 Then
                                bSet.Translate(v1) : bBox.BasePt += v1 : bBnds.Translate(v1)
                            End If
                        End If
                        If stkSty = dxxCharacterStackStyles.Horizontal Then
                            aChar = aSet.Item(0)
                            cLine = tBox.BaseLine
                            cLine.SPT = cLine.SPT.ProjectedTo(aBnds.EdgeV(dxxRectangleLines.LeftEdge))
                            cLine.EPT = cLine.SPT + myPl.XDirection * aBnds.Width
                            cLine.Translate(myPl.YDirection * (-0.1 * aChar.CharHeight))
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))

                        ElseIf stkSty = dxxCharacterStackStyles.Diagonal Then
                            aChar = aSet.Item(0)
                            cLine.SPT = tBnds.TopRightV
                            cLine.EPT = bBnds.BottomLeftV
                            cLine.Rotate(cLine.MPT, myPl.ZDirection, -45)
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))

                        End If
                        'capture extents
                        eVecs = aBnds.CornersV
                        _Bounds.ExpandToVectorsV(eVecs)
                        v1 = eVecs.Item(1).WithRespectTo(myPl) 'top left
                        v3 = eVecs.Item(3).WithRespectTo(myPl) 'bottom right
                        'expand ascent
                        If v1.Y > _CharBox.Ascent Then _CharBox.Ascent = v1.Y
                        'expand descent
                        If v3.Y < 0 And Math.Abs(v3.Y) > _CharBox.Descent Then _CharBox.Descent = Math.Abs(v3.Y)
                        'capture width
                        If v3.X > _CharBox.Width Then _CharBox.Width = v3.X
                        basePt = aSet.LastChar.CharBox.EndPtV.ProjectedTo(baseLine) '  aBox.Vector(aBox.Width(True), 0).ProjectedTo(baseLine)
                        If bSet.CharacterCount > 0 Then
                            v1 = bSet.LastChar.CharBox.EndPtV.ProjectedTo(baseLine) 'bBox.Vector(bBox.Width(True), 0).ProjectedTo(baseLine)
                            If basePt.DirectionTo(v1).Equals(myPl.XDirection, 3) Then basePt = v1
                        End If
                    End If


                Else
                    '========================================
                    'VERTICAL TEXT
                    '========================================
                    'adjust for baseline alignment
                    If dY <> 0 Then
                        v1 = _CharBox.YDirectionV * dY
                        aChar.Translate(v1)
                        aBnds.Translate(v1)
                    End If
                    If Not aChar.IsFormatCode Then
                        aIDs.Clear() : aIDs.Add(ci)
                        aSet.Clear() : aSet.Add(aChar)
                        bSet.Clear() : bIDs.Clear()
                        stkSty = dxxCharacterStackStyles.None
                        bFlg = False
                        'collect stacked text to arrange collectively
                        If aChar.StackID <> 0 Then
                            stkSty = aChar.StackStyle
                            superScript = stkSty = dxxCharacterStackStyles.SuperScript
                            SubScript = stkSty = dxxCharacterStackStyles.SubScript
                            If SubScript Then
                                aSet.Clear()
                                bSet.Add(aChar)
                                aIDs.Clear() : bIDs.Add(ci)
                            End If
                            For j = ci + 1 To chrCnt
                                bChar = _Characters.Item(j - 1)
                                If bChar.StackID = aChar.StackID Then
                                    If Not bChar.IsFormatCode Then
                                        If bSetCharIndexes Then
                                            lnidx += 1
                                            aChar.LineIndex = lnidx
                                        End If
                                        If SubScript Then
                                            bSet.Add(bChar) : bIDs.Add(j)
                                        Else
                                            aSet.Add(bChar) : aIDs.Add(j)
                                        End If
                                    End If
                                    ci += 1
                                ElseIf bChar.StackID = aChar.StackID + 1 Then
                                    If Not bChar.IsFormatCode Then
                                        If bSetCharIndexes Then
                                            lnidx += 1
                                            aChar.LineIndex = lnidx
                                        End If
                                        bSet.Add(bChar) : bIDs.Add(j)
                                    End If
                                    ci += 1
                                Else
                                    Exit For
                                End If
                            Next j
                        End If
                        ' char alignment
                        If stkSty = dxxCharacterStackStyles.SubScript Then
                            aBnds = bSet.ExtentRectangle(myPl) : aBox = New TCHARBOX(CharBox)
                            bBox = aBox.Clone : bBnds = aBnds.Clone
                        Else
                            aBnds = aSet.ExtentRectangle(myPl) : aBox = New TCHARBOX(CharBox)
                            If bSet.CharacterCount > 0 Then
                                bBnds = bSet.ExtentRectangle(myPl)
                                bBox = New TCHARBOX(CharBox)
                            End If
                        End If
                        tBnds = aBnds.Clone : tBox = aBox.Clone
                        ascntBox = aBox.Clone : ascntBox.Descent = 0 ': ascntBox.Ascent = aBnds.Point(dxxRectanglePts.TopLeft).WithRespectTo(aBox).Y
                        ascntBox.Ascent = aChar.CharHeight
                        'align stacks
                        If stkSty <> dxxCharacterStackStyles.None Then
                            'align the bottom set below the top set of chars
                            v1 = aBnds.BottomLeftV
                            v2 = bBnds.TopLeftV
                            v1 += myPl.YDirection * -0.07 * baseHt
                            If stkSty = dxxCharacterStackStyles.Diagonal Then
                                'shift the bottom set right by the length of the top one
                                v1 += myPl.XDirection * aBnds.Width
                            Else
                                'center the bottom on the top one
                                v1 += myPl.XDirection * (0.5 * aBnds.Width - 0.5 * bBnds.Width)
                                If bBnds.Width > aBnds.Width Then
                                    v3 = myPl.XDirection * (0.5 * bBnds.Width - 0.5 * aBnds.Width)
                                    bFlg = True
                                    aSet.Translate(v3) : aBox.BasePt += v3 : aBnds.Translate(v3)
                                    bSet.Translate(v3) : bBox.BasePt += v3 : bBnds.Translate(v3)
                                    ascntBox.BasePt += v3 : v1 += v3 : v2 += v3

                                End If
                            End If
                            aDir = v2.DirectionTo(v1, False, rDistance:=d1)
                            v1 = aDir * d1
                            bSet.Translate(v1) : bBnds.Translate(v1) : bBox.BasePt += v1
                            aBnds.ExpandToVectors(bBnds.Corners)
                            ascntBox = bBox.Clone : ascntBox.Descent = 0
                            ascntBox.Ascent = aBnds.Height - bBox.Descent
                            If bFlg Then
                                'make the bBox the aBox by swapping because it is the longer group
                                cBox = aBox : aBox = bBox : bBox = cBox
                            End If
                        End If
                        'align the chars
                        v2 = basePt.Clone
                        d1 = 0
                        If bFirstChar Then
                            v1 = aBox.BasePt.ProjectedTo(aBnds.EdgeV(dxxRectangleLines.LeftEdge))
                            v1 = dxfProjections.ToLine(v1, vLine)
                        Else
                            v1 = aBnds.TopLeftV
                        End If
                        aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        If d1 <> 0 Then
                            v1 = aDir * d1
                            aSet.Translate(v1) : aBox.BasePt += v1 : aBnds.Translate(v1)
                            ascntBox.BasePt += v1 : tBnds.Translate(v1) : tBox.BasePt += v1
                            If bSet.CharacterCount > 0 Then
                                bSet.Translate(v1) : bBox.BasePt += v1 : bBnds.Translate(v1)
                            End If
                        End If
                        If stkSty = dxxCharacterStackStyles.Horizontal Then
                            aChar = aSet.Item(0)
                            cLine = tBox.BaseLine
                            cLine.SPT = cLine.SPT.ProjectedTo(aBnds.EdgeV(dxxRectangleLines.LeftEdge))
                            cLine.EPT = cLine.SPT + myPl.XDirection * aBnds.Width
                            cLine.Translate(myPl.YDirection * (-0.1 * aChar.CharHeight))
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))

                        ElseIf stkSty = dxxCharacterStackStyles.Diagonal Then
                            aChar = aSet.Item(0)

                            cLine.SPT = tBnds.TopLeftV
                            cLine.EPT = bBnds.BottomLeftV
                            cLine.Rotate(cLine.MPT, myPl.ZDirection, -45)
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))

                        End If
                        'capture extents
                        eVecs = aBnds.CornersV
                        _Bounds.ExpandToVectorsV(eVecs)
                        'update the ascent
                        If bFirstChar Then
                            v1 = aBnds.TopCenterV.WithRespectTo(myPl)
                            _CharBox.Ascent = v1.Y
                        End If
                        v1 = aBnds.BottomCenterV.WithRespectTo(myPl)
                        If v1.Y < 0 And Math.Abs(v1.Y) > _CharBox.Descent Then _CharBox.Descent = Math.Abs(v1.Y)
                        v1 = aBnds.TopRightV.WithRespectTo(myPl)
                        If v1.X > _CharBox.Width Then _CharBox.Width = v1.X
                        basePt = aBox.Point(dxxRectanglePts.BottomLeft, False) + myPl.YDirection * (-0.07 * baseHt)
                        basePt = dxfProjections.ToLine(basePt, vLine)


                    End If
                End If  'vertical/horizontal



            Next ci
            Dim charbounds As dxfRectangle = _Characters.ExtentRectangle(myPl)
            _CharBox.Width = charbounds.Width

            If Not Vertical Then
                'If dX <> 0 Then _CharBox.BasePt += myPl.XDirection * -dX

                _CharBox.BasePt = _Bounds.TopLeftV + myPl.YDirection * -_CharBox.Ascent
            End If

            Return _Bounds
        End Function
        Public Function Clone(Optional bNoChars As Boolean = False) As dxoString

            Dim chars As New dxoCharacters()
            If Not bNoChars Then chars = _Characters.Clone()
            Dim bnds As dxfRectangle = Nothing
            If _Bounds IsNot Nothing Then bnds = _Bounds.Clone()
            Return New dxoString() With
            {
                .Alignment = Alignment,
                .FitFactor = FitFactor,
                .GroupCount = GroupCount,
                .HasAlignments = HasAlignments,
                .HasStacks = HasStacks,
                .LineNo = LineNo,
                .ScreenAlignment = ScreenAlignment,
                .StackCount = StackCount,
                .Vertical = Vertical,
                ._Characters = chars,
                ._Bounds = bnds
            }

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function



#End Region 'Methods
    End Class

End Namespace
