

Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoCharacter
        Implements ICloneable


#Region "Constructors"

        Friend Sub New(aChar As TCHAR)
            Init()

            CharacterString = aChar.Charr
            _AsciiIndex = aChar.AsciiIndex
            Key = aChar.Key
            GroupIndex = aChar.GroupIndex
            ReplacedChar = aChar.ReplacedChar
            Index = aChar.Index
            FormatCode = aChar.FormatCode
            LineIndex = aChar.LineIndex
            LineNo = aChar.LineNo
            StringIndex = aChar.StringIndex
            PathDefined = aChar.PathDefined
            BracketGroup = aChar.BracketGroup


            _CharBox = New dxoCharBox(aChar.CharBox)
            _Formats = New dxoCharFormat(aChar.Formats)
            _Shape = New dxoShape(aChar.Shape)
            _AccentPaths = New dxoPoints(aChar.AccentPaths)
            _ExtentPts = New dxoPoints(aChar.ExtentPts)
        End Sub

        Friend Sub New(aChar As dxoCharacter)
            Init()
            If aChar Is Nothing Then Return

            CharacterString = aChar.CharacterString

            Key = aChar.Key
            GroupIndex = aChar.GroupIndex
            ReplacedChar = aChar.ReplacedChar
            Index = aChar.Index
            LayerName = aChar.LayerName
            FormatCode = aChar.FormatCode
            LineIndex = aChar.LineIndex
            LineNo = aChar.LineNo
            StringIndex = aChar.StringIndex
            PathDefined = aChar.PathDefined
            BracketGroup = aChar.BracketGroup

            _CharBox = New dxoCharBox(aChar.CharBox)
            _Formats = aChar.Formats.Clone()
            _Shape = aChar.Shape.Clone()
            _AccentPaths = New dxoPoints(aChar.AccentPaths)
            _ExtentPts = New dxoPoints(aChar.ExtentPts)
            If aChar._Bounds IsNot Nothing Then _Bounds = aChar._Bounds.Clone()
        End Sub

        Friend Sub New(aChar As Char, Optional aFormats As dxoCharFormat = Nothing, Optional aCharBox As dxoCharBox = Nothing)
            Init()
            Charr = aChar

            _AsciiIndex = Strings.Asc(Charr)
            _CharacterString = Charr.ToString()
            Formats = aFormats
            If aCharBox IsNot Nothing Then _CharBox.CopyDirections(aCharBox, bMoveTo:=True)
        End Sub

        Private Sub Init()

            Charr = " "
            _AsciiIndex = Strings.Asc(Charr)
            GroupIndex = 0
            Index = -1
            LayerName = ""
            LineIndex = 0
            LineNo = 0
            PathDefined = False
            StringIndex = 0
            FormatCode = dxxCharFormatCodes.None
            Key = ""
            ReplacedChar = ""
            _CharBox = New dxoCharBox()
            _Shape = New dxoShape()
            _Formats = New dxoCharFormat()
            _ExtentPts = New dxoPoints()
            BracketGroup = 0


        End Sub


#End Region 'Constructors

#Region "Properties"

        Private _FontStyleInfo As TFONTSTYLEINFO
        Friend Property FontStyleInfo As TFONTSTYLEINFO
            Get
                Return _FontStyleInfo
            End Get
            Set(value As TFONTSTYLEINFO)
                _FontStyleInfo = value
            End Set
        End Property


        Private _Shape As dxoShape
        Friend Property Shape As dxoShape
            Get
                Return _Shape
            End Get
            Set(value As dxoShape)
                _Shape = New dxoShape(value)

            End Set
        End Property


        Private _CharBox As dxoCharBox
        Friend Property CharBox As dxoCharBox
            Get
                Return _CharBox
            End Get
            Set(value As dxoCharBox)
                _CharBox = New dxoCharBox(value)
            End Set
        End Property

        Friend ReadOnly Property CharBoxV As TCHARBOX
            Get
                Return New TCHARBOX(_CharBox)
            End Get
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

        Private _ReplacedChar As String
        Public Property ReplacedChar As String
            Get
                Return _ReplacedChar
            End Get
            Friend Set(value As String)
                _ReplacedChar = value
            End Set
        End Property

        Private _ExtentPts As dxoPoints
        Friend Property ExtentPts As dxoPoints
            Get
                Return _ExtentPts
            End Get
            Set(value As dxoPoints)
                _ExtentPts = value
            End Set
        End Property

        Private _AccentPaths As dxoPoints
        Friend ReadOnly Property AccentPaths As dxoPoints
            Get
                Return _AccentPaths
            End Get

        End Property

        Private _Index As Integer
        Public Property Index As Integer
            Get
                Return _Index
            End Get
            Friend Set(value As Integer)
                _Index = value
            End Set
        End Property


        Private _StringIndex As Integer
        Public Property StringIndex As Integer
            Get
                Return _StringIndex
            End Get
            Friend Set(value As Integer)
                _StringIndex = value
            End Set
        End Property

        Private _LineIndex As Integer
        Public Property LineIndex As Integer
            Get
                Return _LineIndex
            End Get
            Friend Set(value As Integer)
                _LineIndex = value
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

        Private _BracketGroup As Integer
        Public Property BracketGroup As Integer
            Get
                Return _BracketGroup
            End Get
            Friend Set(value As Integer)
                _BracketGroup = value
            End Set
        End Property

        Private _GroupIndex As Integer
        Public Property GroupIndex As Integer
            Get
                Return _GroupIndex
            End Get
            Friend Set(value As Integer)
                _GroupIndex = value
            End Set
        End Property

        Private _Formats As dxoCharFormat
        Public Property Formats As dxoCharFormat
            Get
                Return _Formats
            End Get
            Friend Set(value As dxoCharFormat)
                _Formats = New dxoCharFormat(value)

            End Set
        End Property

        Public Property CharHeight As Double
            Get
                Return Formats.CharHeight
            End Get
            Friend Set(value As Double)
                Formats.CharHeight = value
            End Set
        End Property

        Public Property CharAlign As dxxCharacterAlignments
            Get
                Return Formats.CharAlign
            End Get
            Friend Set(value As dxxCharacterAlignments)
                Formats.CharAlign = value
            End Set
        End Property


        Public Property StackID As Integer
            Get
                Return Formats.StackID
            End Get
            Friend Set(value As Integer)
                Formats.StackID = value
            End Set
        End Property

        Public Property StackStyle As dxxCharacterStackStyles
            Get
                Return Formats.StackStyle
            End Get
            Friend Set(value As dxxCharacterStackStyles)
                Formats.StackStyle = value
            End Set
        End Property

        Public Property Ascent As Double
            Get
                Return CharBox.Ascent
            End Get
            Friend Set(value As Double)
                CharBox.Ascent = value
            End Set
        End Property

        Public Property Descent As Double
            Get
                Return CharBox.Descent
            End Get
            Friend Set(value As Double)
                CharBox.Descent = value
            End Set
        End Property

        Public ReadOnly Property Alignment As dxxMTextAlignments
            Get
                '^the mtext alignment of the character
                If Formats.Vertical Then
                    Return dxxMTextAlignments.TopCenter
                Else
                    Return dxxMTextAlignments.BaselineLeft
                End If
            End Get
        End Property

        Public ReadOnly Property AlignmentH As dxxTextJustificationsHorizontal
            Get
                '^the horizontal alignment of the text
                Return TFONT.AlignmentH(Alignment)
            End Get
        End Property

        Public ReadOnly Property AlignmentName As String
            Get
                Return dxfEnums.Description(Alignment)
            End Get
        End Property

        Public ReadOnly Property AlignmentPt As dxfVector
            Get
                '^the Alignment point of the character
                Return New dxfVector With {.Strukture = AlignmentPtV}
            End Get
        End Property

        Friend ReadOnly Property AlignmentPtV As TVECTOR
            Get
                '^the Alignment point of the character
                Return CharBox.StartPtV
            End Get
        End Property

        Friend ReadOnly Property StartPtV As TVECTOR
            Get
                '^the Alignment point of the character
                If Not Vertical Then Return CharBox.StartPtV Else Return CharBox.TopLeftV
            End Get
        End Property

        Friend ReadOnly Property EndPtV As TVECTOR
            Get
                '^the Alignment point of the character
                If Not Vertical Then Return CharBox.EndPtV Else Return CharBox.BottomLeftV
            End Get
        End Property

        Public ReadOnly Property AlignmentV As dxxTextJustificationsVertical
            Get
                '^the vertical alignment of the text
                Return TFONT.AlignmentV(Alignment)
            End Get
        End Property

        Public ReadOnly Property BaseLine As dxeLine
            Get
                '^the baseline of the character
                Return New dxeLine(CharBox.BaseLineV)
            End Get
        End Property

        Public Property IsShape As Boolean
            Get
                Return Formats.IsShape
            End Get
            Friend Set(value As Boolean)
                Formats.IsShape = value
            End Set
        End Property


        Private _Bounds As dxfRectangle
        Public Property Bounds As dxfRectangle
            Get
                '^a rectangle that encompasses the entities extents on it own plane
                Return _Bounds
            End Get
            Friend Set(value As dxfRectangle)
                _Bounds = value
            End Set
        End Property

        Public Function GetBounds(Optional bRecompute As Boolean = False) As dxfRectangle
            If _Bounds IsNot Nothing Then
                If _Bounds.Area = 0 Then bRecompute = True
            Else
                bRecompute = True
            End If
            If bRecompute Then
                _Bounds = New dxfRectangle(BoundingRectangleV(True))
            End If
            Return _Bounds
        End Function
        Private Function BoundingRectangleV(Optional bIncludeAccents As Boolean = True) As TPLANE

            '^a rectangle that encompasses the entities extents on it own plane


            Dim myPl As TPLANE = _CharBox.PlaneV(True)
            myPl.SetDimensions(0, 0)


            Dim aPoints As New dxoPoints()
            Dim myBox As TCHARBOX = CharBoxV
            myBox.ObliqueAngle = Formats.ObliqueAngle

            If ExtentPts.Count > 0 Then
                aPoints.Append(ExtentPts)

            ElseIf Shape.Path.Count > 0 Then
                aPoints = New dxoPoints(Shape.Path)
            End If

            If bIncludeAccents Then
                aPoints.Append(AccentPaths)
            End If


            Dim _rVal As TPLANE = aPoints.BoundsV(myPl, ObliqueAngle)

            Dim v1 As TVECTOR

            'If AccentPaths.Count > 0 Then
            '    Dim acntbox As TPLANE = AccentPaths.BoundsV(myPl, ObliqueAngle)
            '    v1 = acntbox.Point(dxxRectanglePts.TopLeft).WithRespectTo(_rVal)
            '    Dim dy As Double = v1.Y - _rVal.Height / 2
            '    If dy > 0 Then
            '        _rVal.Origin += _rVal.YDirection * dy / 2
            '        _rVal.Height += dy
            '    End If
            'End If

            v1 = _rVal.Point(dxxRectanglePts.TopLeft).WithRespectTo(myPl)
            If v1.Y > 0 And v1.Y > _CharBox.Ascent Then
                _CharBox.Ascent = v1.Y 'Else myBox.Ascent = 0
            End If
            If v1.X > _CharBox.Width Then
                '_CharBox.Width = v1.X
            End If
            v1 = _rVal.Point(dxxRectanglePts.BottomLeft).WithRespectTo(myPl)
            If v1.Y < 0 And Math.Abs(v1.Y) > _CharBox.Descent Then
                _CharBox.Descent = Math.Abs(v1.Y)
            End If

            'If _rVal.Height * _rVal.Width <= 0 Then
            '    _rVal.Define(_CharBox.AscentCenterV, _CharBox.XDirectionV, _CharBox.YDirectionV, _CharBox.Ascent, 0.75 * _CharBox.Width)

            'End If

            Return _rVal

        End Function

        Friend Function GetPathPoints(bIncludeAccents As Boolean, aCharBox As dxoCharBox) As TVECTORS

            Dim _rVal As New TVECTORS(0)
            If _Shape.Path.Count <= 0 Then Return _rVal

            '^the points that define the characters path with respect to the character box plane

            Dim pthpts As New dxoPoints(_Shape.Path)
            If bIncludeAccents Then pthpts.AddRange(AccentPaths)
            If aCharBox Is Nothing Then aCharBox = _CharBox
            Dim myPl As TPLANE = aCharBox.PlaneV(True)
            Dim shear As Double = ObliqueAngle

            For Each apt As dxoPoint In pthpts
                Dim p1 As TVECTOR = myPl.Vector(apt.X, apt.Y, aShearXAngle:=shear)
                _rVal.Add(p1, aCode:=apt.Code)
            Next
            Return _rVal

        End Function

        Public ReadOnly Property Center As dxfVector
            Get
                '^the center of the characters bounding rectangle
                Return GetBounds.Center
            End Get
        End Property

        Friend Charr As Char


        Private _CharacterString As String
        Public Property CharacterString As String
            Get
                '^the single character which is the characters character value
                Return Charr.ToString()
            End Get
            Friend Set(value As String)
                If String.IsNullOrEmpty(value) Then value = " "
                Charr = value.Chars(0)
                _CharacterString = value
                _AsciiIndex = Strings.Asc(Charr)
            End Set
        End Property

        Private _AsciiIndex As Integer
        Public Property AsciiIndex As Integer
            Get
                '^the single character which is the characters character value
                Return _AsciiIndex
            End Get
            Friend Set(value As Integer)
                _AsciiIndex = value
            End Set

        End Property

        Public Property Color As dxxColors
            Get
                '^the character's color
                Return Formats.Color
            End Get
            Set(value As dxxColors)
                Formats.Color = value
            End Set
        End Property
        Public ReadOnly Property FontName As String
            Get
                '^the fonts name
                Return dxoFonts.GetFontName(Formats.FontIndex)
            End Get
        End Property

        Public Property FontIndex As Integer
            Get
                '^the fonts index
                Return Formats.FontIndex
            End Get
            Friend Set(value As Integer)
                Formats.FontIndex = value
            End Set
        End Property

        Public ReadOnly Property Height As Double
            Get
                '^the text height of the character
                Return Formats.CharHeight
            End Get
        End Property
        Public Property HeightFactor As Double
            Get
                Return Formats.HeightFactor
            End Get
            Friend Set(value As Double)
                Formats.HeightFactor = value
            End Set
        End Property

        Private _Key As String
        Public Property Key As String
            Get
                Return _Key
            End Get
            Friend Set(value As String)
                _Key = value
            End Set
        End Property

        Private _FormatCode As dxxCharFormatCodes
        Public Property FormatCode As dxxCharFormatCodes
            Get
                Return _FormatCode
            End Get
            Friend Set(value As dxxCharFormatCodes)
                _FormatCode = value
            End Set
        End Property

        Public ReadOnly Property IsFormatCode As Boolean
            Get
                Return FormatCode <> dxxCharFormatCodes.None Or AsciiIndex <= 0
            End Get
        End Property

        Public ReadOnly Property IsStacked As Boolean
            Get
                Return StackStyle > dxxCharacterStackStyles.None
            End Get

        End Property

        Private _PathDefined As Boolean
        Friend Property PathDefined As Boolean
            Get
                Return _PathDefined
            End Get
            Set(value As Boolean)
                _PathDefined = value
            End Set
        End Property

        Public ReadOnly Property PathPoints As colDXFVectors
            Get
                Return New colDXFVectors(_Shape.Path.ToPlaneVectors(CharBox.PlaneV(True), ObliqueAngle))
            End Get
        End Property

        Friend ReadOnly Property ExtentPointsV As TVECTORS
            Get
                '^ returns the characters extentPts expressed in world cooridinats
                Return _ExtentPts.ToPlaneVectorsV(CharBox.PlaneV(True), ObliqueAngle)
            End Get
        End Property

        Public Function ExtentPoints(Optional aRelativeToPlane As dxfPlane = Nothing, Optional aTag As String = "") As colDXFVectors

            Return _ExtentPts.ToPlaneVectors(CharBox.Plane, ObliqueAngle, aRelativeToPlane)
        End Function

        Public ReadOnly Property DisplaySettings As dxfDisplaySettings
            Get
                Return New dxfDisplaySettings(LayerName, Color, dxfLinetypes.Continuous)
            End Get
        End Property

        Public Property ObliqueAngle As Double
            Get
                Return Formats.ObliqueAngle
            End Get
            Set(value As Double)
                CharBox.ObliqueAngle = value
                Formats.ObliqueAngle = CharBox.ObliqueAngle
            End Set
        End Property

        Public Property Overline As Boolean
            Get
                Return Formats.Overline
            End Get
            Set(value As Boolean)
                Formats.Overline = True
            End Set
        End Property

        Public ReadOnly Property Plane As dxfPlane
            Get
                '^the XY plane of the entities current object coordinate system
                Return New dxfPlane(PlaneV)
            End Get
        End Property

        Friend ReadOnly Property PlaneV As TPLANE
            Get
                Return _CharBox.PlaneV(True)
            End Get
        End Property

        Public ReadOnly Property TextRectangle As dxfRectangle
            Get
                'myBox.Ascent += aAscentAdder
                Return New dxfRectangle(_CharBox.PlaneV(False))

            End Get
        End Property

        Public Property TextStyleName As String
            Get
                Return _Formats.TextStyleName
            End Get
            Set(value As String)
                _Formats.TextStyleName = value
            End Set
        End Property



        Public Property Tracking As Double
            Get
                '^scales the distance between each character
                '~0.75  to 4.0
                Return Formats.Tracking

            End Get
            Friend Set(value As Double)
                Formats.Tracking = value
            End Set
        End Property

        Public ReadOnly Property TrackPoint As dxfVector
            Get
                Return New dxfVector(_CharBox.Vector(_CharBox.Width, 0.5 * _CharBox.Ascent))
            End Get
        End Property
        Public Property Underline As Boolean
            Get
                Return Formats.Underline
            End Get
            Set(value As Boolean)
                Formats.Underline = True
            End Set
        End Property

        Public Property StrikeThru As Boolean
            Get
                Return Formats.StrikeThru
            End Get
            Set(value As Boolean)
                Formats.StrikeThru = True
            End Set
        End Property
        Public Property Width As Double
            Get
                Return _CharBox.Width
            End Get
            Friend Set(value As Double)
                _CharBox.Width = value
            End Set
        End Property
        Public Property WidthFactor As Double
            Get
                '^the width factor for the string
                '~default = 1
                Return Formats.WidthFactor
            End Get
            Friend Set(value As Double)
                Formats.WidthFactor = value
            End Set
        End Property
        Public ReadOnly Property X As Double
            Get
                Return Center.X
            End Get
        End Property
        Public ReadOnly Property XDirection As dxfDirection
            Get
                Return New dxfDirection(PlaneV.XDirection)
            End Get
        End Property
        Public ReadOnly Property Y As Double
            Get
                Return Center.Y
            End Get
        End Property
        Public ReadOnly Property YDirection As dxfDirection
            Get
                Return New dxfDirection With {.Strukture = PlaneV.YDirection}
            End Get
        End Property
        Public ReadOnly Property Z As Double
            Get
                Return Center.Z
            End Get
        End Property
        Public ReadOnly Property ZDirection As dxfDirection
            Get
                Return New dxfDirection(PlaneV.ZDirection)
            End Get
        End Property

        Public Property Vertical As Boolean
            Get
                Return Formats.Vertical
            End Get
            Set(value As Boolean)
                Formats.Vertical = value
            End Set
        End Property
        Public ReadOnly Property IsTrueType As Boolean
            Get
                Return Formats.IsTrueType
            End Get
        End Property

        Public ReadOnly Property IsDigit As Boolean
            Get
                Return Char.IsDigit(Charr)
            End Get
        End Property
        Public ReadOnly Property IsLetter As Boolean
            Get
                Return Char.IsLetter(Charr)
            End Get
        End Property

#End Region 'Properties
#Region "Methods"
        Friend Sub TransferToPlane(aToPlane As TPLANE)
            CharBox.CopyDirections(aToPlane)
            CharBox.BasePtV = CharBox.BasePtV.TransferedToPlane(CharBox.PlaneV(False), aToPlane, 1, 1, 1, 0)
        End Sub

        Public Overrides Function ToString() As String
            If Not IsFormatCode Then Return CharacterString Else Return $"{ CharacterString } [{dxfEnums.Description(FormatCode)}]"
        End Function

        Friend Sub ApplyFormats()
            '^computes the world path of the passed character
            Try
                _AccentPaths = New dxoPoints

                CharBox.ObliqueAngle = 0
                If IsFormatCode Then
                    ObliqueAngle = 0
                    Return
                End If

                CharBox.ObliqueAngle = ObliqueAngle
                Dim ovrd As Double = 0.2 * CharHeight
                Dim udrd As Double = 0.2 * CharHeight
                'overline
                '---------------------------------------------------------
                If Overline Then

                    AccentPaths.Add(New TPOINT(0, Ascent + ovrd, dxxVertexStyles.MOVETO))
                    AccentPaths.Add(New TPOINT(CharBox.Width, Ascent + ovrd, dxxVertexStyles.LINETO))
                End If
                'underline
                '---------------------------------------------------------
                If Underline Then
                    AccentPaths.Add(New TPOINT(0, -udrd, dxxVertexStyles.MOVETO))
                    AccentPaths.Add(New TPOINT(CharBox.Width, -udrd, dxxVertexStyles.LINETO))
                End If
                If StrikeThru Then
                    AccentPaths.Add(New TPOINT(0, 0.5 * Ascent, dxxVertexStyles.MOVETO))
                    AccentPaths.Add(New TPOINT(CharBox.Width, 0.5 * Ascent, dxxVertexStyles.LINETO))
                End If
                'backwards or upside down
                '---------------------------------------------------------
                If Formats.Backwards Or Formats.UpsideDown Then
                    Orient(Formats.Backwards, Formats.UpsideDown)
                End If
            Catch ex As Exception
            End Try
        End Sub
        Friend Sub Orient(bBackwards As Boolean, bUpsideDown As Boolean)
            If Not bBackwards And Not bUpsideDown Then Return
            If bBackwards Then
                CharBox.RotateAbout(CharBox.BasePtV, CharBox.YDirectionV, 180, False, True, True, True)
            End If
            If bUpsideDown Then
                CharBox.RotateAbout(CharBox.BasePtV, CharBox.XDirectionV, 180, False, True, True, True)
            End If
        End Sub
        Friend Function CopyDirections(aCharBox As TCHARBOX) As Boolean

            If TPLANE.IsNull(aCharBox) Then Return False
            Dim _rVal As Boolean = CharBox.CopyDirections(aCharBox)
            If _rVal Then _Bounds = Nothing
            Return _rVal
        End Function

        Friend Function CopyDirections(aCharBox As dxoCharBox) As Boolean
            If aCharBox Is Nothing Then Return False
            If TPLANE.IsNull(aCharBox) Then Return False
            Dim _rVal As Boolean = CharBox.CopyDirections(aCharBox)
            If _rVal Then _Bounds = Nothing
            Return _rVal
        End Function

        Public Function ConvertToText() As dxeText
            Dim _rVal As New dxeText(dxxTextTypes.DText) With {
                .TextString = CharacterString,
                .PlaneV = PlaneV,
                .Alignment = dxxMTextAlignments.BaselineLeft,
                .AlignmentPt1V = PlaneV.Origin,
                .Color = Formats.Color,
                .TextStyleName = TextStyleName,
                .Vertical = Formats.Vertical,
                .Backwards = Formats.Backwards,
                .UpsideDown = Formats.UpsideDown,
                .WidthFactor = Formats.WidthFactor,
                .ObliqueAngle = Formats.ObliqueAngle,
                .LayerName = LayerName,
                .TextHeight = Formats.CharHeight
            }
            Return _rVal
        End Function

        Public ReadOnly Property CharacterBox As dxoCharBox
            Get
                Return New dxoCharBox(_CharBox)
            End Get
        End Property

        Public Function Clone() As dxoCharacter
            Return New dxoCharacter(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function

        Friend Sub Copy(aChar As TCHAR)

            CharacterString = aChar.Charr

            'Key = aChar.Key
            'GroupIndex = aChar.GroupIndex
            'ReplacedChar = aChar.ReplacedChar
            'Index = aChar.Index
            'LayerName = aChar.LayerName
            'FormatCode = aChar.FormatCode
            'LineIndex = aChar.LineIndex
            'LineNo = aChar.LineNo
            'StringIndex = aChar.StringIndex
            PathDefined = aChar.PathDefined
            ' _Formats = New dxoCharFormat(aChar.Formats)
            _CharBox = New dxoCharBox(aChar.CharBox)
            '_Formats = aChar.Formats.Clone()
            _Shape = New dxoShape(aChar.Shape)
            _AccentPaths = New dxoPoints(aChar.AccentPaths)
            _ExtentPts = New dxoPoints(aChar.ExtentPts)
            _Bounds = Nothing
        End Sub


        Friend Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, bMirrorDirections As Boolean, Optional bSuppressCheck As Boolean = False)

            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return
            End If
            CharBox.Mirror(aSP, aEP, True, bMirrorDirections, True)
        End Sub

        Friend Sub Translate(aTranslation As TVECTOR)
            CharBox.Translate(aTranslation)
            If _Bounds IsNot Nothing Then _Bounds.Translate(aTranslation)
        End Sub
        Public Sub Scale(aXScale As Double, aYScale As Double)
            If aXScale <= 0 Then aXScale = 1
            If aYScale <= 0 Then aYScale = 1
            If aXScale = 1 And aYScale = 1 Then Return
            CharBox.Scale(aXScale, aYScale)
            Shape.Path.Scale(aXScale, aYScale)
            ExtentPts.Scale(aXScale, aYScale)
            CharHeight *= aYScale
        End Sub
        Friend Function CharPath(aDisplay As TDISPLAYVARS, aParentPlane As dxfPlane, aDomain As dxxDrawingDomains) As TPATH
            LayerName = aDisplay.LayerName
            aDisplay.Color = _Formats.Color

            Dim myBox As TCHARBOX = CharBoxV

            myBox.ObliqueAngle = _Formats.ObliqueAngle
            Dim rRelative As Boolean = Not dxfPlane.IsNull(aParentPlane)
            If rRelative Then myBox.CopyDirections(aParentPlane)

            Dim myPlane As TPLANE = myBox.Plane(True)

            Dim _rVal As New TPATH(_Shape, myPlane, aDisplay, aDomain, IsTrueType, ObliqueAngle, aParentPlane, $"{Color},{IsShape},CHAR_PATH") With {.GraphicType = dxxGraphicTypes.Text}

            ''_rVal.Plane = IIf(rRelative, New TPLANE(aParentPlane), myPlane)

            Return _rVal


        End Function

        Friend Function AccentPath(aDisplay As TDISPLAYVARS, aParentPlane As dxfPlane, aDomain As dxxDrawingDomains) As TPATH
            LayerName = aDisplay.LayerName
            aDisplay.Color = Color

            Dim myBox As TCHARBOX = CharBoxV

            myBox.ObliqueAngle = _Formats.ObliqueAngle
            Dim rRelative As Boolean = aParentPlane IsNot Nothing
            If rRelative Then myBox.CopyDirections(aParentPlane)

            Dim myPlane As TPLANE = myBox.Plane(True)

            Return AccentPaths.ConvertToPath(myPlane, aDisplay, aDomain, False, ObliqueAngle, aParentPlane, $"{Color},{IsShape},ACCENT_PATH")
        End Function

        Friend Function Paths(aDisplay As TDISPLAYVARS, aParentPlane As dxfPlane, aDomain As dxxDrawingDomains) As List(Of TPATH)
            Dim _rVal As New List(Of TPATH)
            If IsTrueType Then
                _rVal.Add(CharPath(aDisplay, aParentPlane, aDomain))
                If AccentPaths.Count > 0 Then
                    _rVal.Add(AccentPath(aDisplay, aParentPlane, aDomain))
                End If
            Else
                '_rVal.Add(Path(aDisplay, aParentPlane, rRelative, aDomain))
                _rVal.Add(CharPath(aDisplay, aParentPlane, aDomain))
                If AccentPaths.Count > 0 Then
                    _rVal.Add(AccentPath(aDisplay, aParentPlane, aDomain))
                End If
            End If
            Return _rVal
        End Function


        Friend Function MoveFromTo(aFromPt As TVECTOR, aToPt As TVECTOR) As Double

            Dim d1 As Double = 0
            Dim dir As TVECTOR = aFromPt.DirectionTo(aToPt, False, rDistance:=d1)
            If d1 <> 0 Then
                CharBox.Translate(dir * d1)
                If _Bounds IsNot Nothing Then _Bounds.Translate(dir * d1)
            End If
            Return d1
        End Function


        Friend Function MoveTo(aVector As TVECTOR) As Double
            Dim d1 As Double = 0
            Dim dir As TVECTOR = CharBox.BasePtV.DirectionTo(aVector, rDistance:=d1)
            If d1 <> 0 Then
                CharBox.Translate(dir * d1)
                If _Bounds IsNot Nothing Then _Bounds.Translate(dir * d1)
            End If
            Return d1

        End Function

        Friend Sub Rescale(aScaleFactor As Single, aRefPt As TVECTOR)
            _Formats.Tracking *= aScaleFactor
            _Formats.CharHeight *= aScaleFactor
            CharBox.Rescale(aScaleFactor, aRefPt)
            If _Bounds IsNot Nothing Then _Bounds.RescaleV(aScaleFactor, aRefPt)

        End Sub

        Friend Sub RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False)
            CharBox.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, True, True, bSuppressNorm)
            If _Bounds IsNot Nothing Then
                _Bounds.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, True, True, True)
            End If
        End Sub

        Public Function ToUpper() As Char
            Return Char.ToUpper(Charr)
        End Function
        Public Function ToLower() As Char
            Return Char.ToLower(Charr)
        End Function
#End Region 'Methods

#Region "Operators"
        Public Shared Operator =(A As dxoCharacter, B As dxoCharacter) As Boolean
            If A Is Nothing And B Is Nothing Then Return True
            If A Is Nothing Or B Is Nothing Then Return False
            Return A.Charr = B.Charr

        End Operator
        Public Shared Operator <>(A As dxoCharacter, B As dxoCharacter) As Boolean
            If A Is Nothing And B Is Nothing Then Return True
            If A Is Nothing Or B Is Nothing Then Return False
            Return A.Charr <> B.Charr
        End Operator
        Public Shared Operator =(A As dxoCharacter, B As Char) As Boolean
            If A Is Nothing Then Return True

            Return A.Charr = B

        End Operator
        Public Shared Operator <>(A As dxoCharacter, B As Char) As Boolean
            If A Is Nothing Then Return True

            Return A.Charr <> B
        End Operator
        Public Shared Operator =(A As dxoCharacter, B As String) As Boolean
            If A Is Nothing And B Is Nothing Then Return True
            If A Is Nothing Or B Is Nothing Then Return False
            If B.Length <> 1 Then Return False
            Return A.Charr = B.Chars(0)

        End Operator
        Public Shared Operator <>(A As dxoCharacter, B As String) As Boolean
            If A Is Nothing And B Is Nothing Then Return True
            If A Is Nothing Or B Is Nothing Then Return False
            If B.Length <> 1 Then Return False
            Return A.Charr <> B.Chars(0)
        End Operator
#End Region 'Operators


    End Class 'dxoCharacter
End Namespace
