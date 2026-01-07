Namespace [Global]

    Public Class dxoSettingsDimOverrides
        Implements idxfSettingsObject

#Region "Members"

        Private _Entry As TTABLEENTRY



#End Region 'Members

#Region "Events"

        Public Event DimStylePropertyChange(aName As String, aOldValue As Object, aNewValue As Object, bUndo As Boolean, aGroupCode As Integer)

#End Region 'Events

#Region "Constructors"
        Public Sub New()
            _Entry = New TTABLEENTRY(dxxReferenceTypes.DIMOVERRIDES, "Standard")

        End Sub

        Friend Sub New(aEntry As TTABLEENTRY)
            _Entry = New TTABLEENTRY(dxxReferenceTypes.DIMOVERRIDES, "Standard")
            If aEntry.EntryType = dxxReferenceTypes.DIMOVERRIDES Then
                _Entry = aEntry
            ElseIf aEntry.EntryType = dxxReferenceTypes.DIMSTYLE Then
                _Entry = TPROPERTIES.CopyDimStyleProps(_Entry, aEntry)
            End If

        End Sub

        Public ReadOnly Property SettingType() As dxxReferenceTypes Implements idxfSettingsObject.SettingType
            Get
                Return dxxReferenceTypes.DIMOVERRIDES
            End Get
        End Property

        Friend Sub New(aName As String)
            _Entry = New TTABLEENTRY(dxxReferenceTypes.DIMOVERRIDES, aName)
        End Sub
#End Region 'Constructors


#Region "Properties"

        Public Property AutoReset() As Boolean

            Get
                Return _Entry.AutoReset
            End Get

            Set(value As Boolean)
                If value = _Entry.AutoReset Then Return
                _Entry.AutoReset = value
                Dim aImg As dxfImage = Nothing
                If GetImage(aImg) Then
                        aImg.BaseSettings(dxxReferenceTypes.DIMOVERRIDES) = _Entry
                    End If

            End Set
        End Property

        Public ReadOnly Property EntryTypeName() As String
            Get
                Return dxfEnums.DisplayName(dxxReferenceTypes.DIMOVERRIDES)
            End Get
        End Property


        Public Property AngularPrecision() As Integer

            '^the number of decimal places for formating angular dimensions
            '~if set to -1 then angular precision follows DIMDEC 
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMADEC)
            End Get
            Set(value As Integer)
                PropValueSet(dxxDimStyleProperties.DIMADEC, value)
            End Set
        End Property

        Public Property AngUnits() As dxxAngularUnits
            '^Sets the units format for angular dimensions.

            '~DegreesDecimal = 0 = Decimal degrees.
            '~DegreesMinSec = 1 = Degrees Minutes Seconds.
            '~Gradians = 2 = gradians.
            '~Radians = 3 = Radians.

            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMAUNIT)
            End Get

            Set(value As dxxAngularUnits)
                If Not dxfEnums.Validate(GetType(dxxAngularUnits), value, bSkipNegatives:=True) Then Return
                PropValueSet(dxxDimStyleProperties.DIMAUNIT, value)
            End Set
        End Property

        Public Property ArrowHead1() As dxxArrowHeadTypes

            '^the type of the first arrowhead block
            Get
                Return dxfArrowheads.GetBlockType(ArrowHeadBlock1)
            End Get

            Set(value As dxxArrowHeadTypes)


                PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, dxfArrowheads.GetBlockName(value))
            End Set

        End Property

        Public Property ArrowHead2() As dxxArrowHeadTypes

            '^the type of the second arrowhead block
            Get
                Return dxfArrowheads.GetBlockType(ArrowHeadBlock2)
            End Get
            Set(value As dxxArrowHeadTypes)
                If value < 0 Then Return

                PropValueSet(dxxDimStyleProperties.DIMBLK2_NAME, dxfArrowheads.GetBlockName(value))
            End Set
        End Property

        Public Property ArrowHeadBlock() As String
            '^the name of the both arrowhead blocks
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK_NAME)
            End Get

            Set(value As String)
                dxfArrowheads.IsDefault(value)
                PropValueSet(dxxDimStyleProperties.DIMBLK_NAME, value)
            End Set

        End Property

        Public Property ArrowHeadBlock1() As String
            '^the name of the first arrowhead block
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK1_NAME)
            End Get
            Set(value As String)
                dxfArrowheads.IsDefault(value)
                PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, value)
            End Set

        End Property

        Public ReadOnly Property ArrowHeadBlock1Handle() As String
            '^the block record handle of the first arrowhead block
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK1)
            End Get

        End Property

        Public Property ArrowHeadBlock2() As String
            '^the name of the second arrowhead block
            Get
                Dim _rVal As String = _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK2_NAME)
                If _rVal = "" Then _rVal = "_ClosedFilled"
                Return _rVal
            End Get
            Set(value As String)
                If value = "" Then value = "_ClosedFilled"
                PropValueSet(dxxDimStyleProperties.DIMBLK_NAME, value)
            End Set
        End Property

        Public ReadOnly Property ArrowHeadBlock2Handle() As String
            '^the block record handle of the second arrowhead block
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK2)
            End Get
        End Property

        Public ReadOnly Property ArrowHeadBlockHandle() As String
            '^the block record handle of the both arrowhead blocks
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK)
            End Get
        End Property

        Public Property ArrowHeadBlockLeader() As String
            '^the name of the leader arrowhead block
            Get
                ArrowHeadBlockLeader = _Entry.PropValueStr(dxxDimStyleProperties.DIMLDRBLK_NAME)
                If ArrowHeadBlockLeader = "" Then ArrowHeadBlockLeader = "_ClosedFilled"
                Return ArrowHeadBlockLeader
            End Get
            Set(value As String)
                If value = "" Then value = "_ClosedFilled"
                dxfArrowheads.IsDefault(value)
                PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, value)
            End Set
        End Property

        Public ReadOnly Property ArrowHeadBlockLeaderHandle() As String
            '^the block record handle of the leader arrowhead block
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMLDRBLK)
            End Get
        End Property

        Public Property ArrowHeadLeader() As dxxArrowHeadTypes
            '^the type of the second arrowhead block
            Get
                Return dxfArrowheads.GetBlockType(ArrowHeadBlockLeader)

            End Get

            Set(value As dxxArrowHeadTypes)

                '^the type of the second arrowhead block
                PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, dxfArrowheads.GetBlockName(value))

            End Set

        End Property

        Public Property ArrowHeads() As dxxArrowHeadTypes

            '^the type of the both arrowhead blocks
            Get
                Dim aName As String
                aName = ArrowHeadBlock
                If aName <> "" Then Return dxfArrowheads.GetBlockType(aName) Else Return dxxArrowHeadTypes.Various
            End Get

            Set(value As dxxArrowHeadTypes)

                PropValueSet(dxxDimStyleProperties.DIMBLK_NAME, dxfArrowheads.GetBlockName(value))
            End Set

        End Property

        Public Property ArrowSize() As Double
            '^the size of the arrowheads drawn under this style
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMASZ)
            End Get
            Set(value As Double)
                If value >= 0 Then PropValueSet(dxxDimStyleProperties.DIMASZ, value)
            End Set

        End Property

        Public Property ArrowTickSize() As Double
            '^Specifies the size of oblique strokes drawn instead of arrowheads for linear, radius, and diameter dimensioning.
            '~0 Draws arrowheads.
            '~>0 Draws oblique strokes instead of arrowheads. The size of the oblique strokes is determined by this value multiplied by the DIMSCALE value
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMTSZ)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMTSZ, value)
            End Set

        End Property



        Public Property CenterMarkSize() As Double
            '^the size of center marks for radial and diametric dimensions
            '~negative values indicate that centerlines should also be drawn
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMCEN)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMCEN, value)
            End Set
        End Property

        Public Property DecimalSeperator() As String
            '^the character used to as the decimal seperator
            '~default  = "."
            Get
                Return Chr(_Entry.PropValueI(dxxDimStyleProperties.DIMDSEP))
            End Get
            Set(value As String)
                value = Trim(value)
                If Len(value) < 1 Then Return
                If Len(value) > 1 Then value = Left(value, 1)
                PropValueSet(dxxDimStyleProperties.DIMDSEP, Asc(value))
            End Set
        End Property

        Public Property DimLineColor() As dxxColors
            '^the color of the dimension lines
            '~also applies to leader lines
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMCLRD)
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.dxfUndefined Then
                    PropValueSet(dxxDimStyleProperties.DIMCLRD, value)
                End If
            End Set
        End Property

        Public Property DimLinetype() As String
            '^the linetype of the dimension lines
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMLTYPE_NAME)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMLTYPE_NAME, value)
            End Set
        End Property

        Public Property DimLineWeight() As dxxLineWeights
            '^the lineweight of the dimension lines
            '~also applies to leader lines
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMLWD)
            End Get

            Set(value As dxxLineWeights)
                If value > dxxLineWeights.ByDefault Then PropValueSet(dxxDimStyleProperties.DIMLWD, value)
            End Set
        End Property

        Public Property DimTextMovement() As dxxDimTextMovementTypes
            '^Controls how text is positioned if the default poisition can't be used
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMTMOVE)
            End Get
            Set(value As dxxDimTextMovementTypes)
                If Not dxfEnums.Validate(GetType(dxxDimTextMovementTypes), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMTMOVE, value)
            End Set
        End Property

        Public Property ExtLineColor() As dxxColors
            '^the color of the extension lines drawn under this style
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMCLRE)
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.dxfUndefined Then PropValueSet(dxxDimStyleProperties.DIMCLRE, value)
            End Set
        End Property

        Public Property ExtLineExtend() As Double
            '^Specifies how far to extend the extension line beyond the dimension line.
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMEXE)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMEXE, value)
            End Set
        End Property

        Public Property ExtLineOffset() As Double
            '^Specifies how far extension lines are offset from origin points. With fixed-length
            '^extension lines, this value determines the minimum offset.
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMEXO)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMEXO, value)
            End Set
        End Property

        Public Property ExtLinetype1() As String
            '^the linetype of the first extension line lines
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMLTEX1_NAME)
            End Get

            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMLTEX1_NAME, value)
            End Set
        End Property

        Public Property ExtLinetype2() As String
            '^the linetype of the second extension line lines
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMLTEX2_NAME)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMLTEX2_NAME, value)
            End Set
        End Property

        Public Property ExtLineWeight() As dxxLineWeights
            '^the lineweight of the extensions lines
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMLWE)
            End Get
            Set(value As dxxLineWeights)
                If value > dxxLineWeights.ByDefault Then PropValueSet(dxxDimStyleProperties.DIMLWE, value)
            End Set
        End Property

        Public Property FeatureScaleFactor() As Double
            '^the scale factor applied to all dimension features
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMSCALE)
            End Get
            Set(value As Double)
                If value > 0 Then PropValueSet(dxxDimStyleProperties.DIMSCALE, value)
            End Set
        End Property

        Public Property ForceDimLines() As Boolean
            '^Controls whether a dimension line is drawn between the extension lines even when the text is placed outside. For radius and diameter dimensions (when DIMTIX is off), draws a dimension line inside the circle or arc and places the text, arrowheads, and leader outside.
            '~False - Does not draw dimension lines between the measured points when arrowheads are placed outside the measured points
            '~True - Draws dimension lines between the measured points even when arrowheads are placed outside the measured points
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMTOFL)
            End Get

            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTOFL, value)
            End Set
        End Property

        Public Property ForceTextBetweenExtLine() As Boolean
            '^forces text between extension lines.
            '~False - Varies with the type of dimension. For linear and angular dimensions, text is placed inside the extension lines if there is sufficient room. For radius and diameter dimensions that don’t fit inside the circle or arc, DIMTIX has no effect and always forces the text outside the circle or arc.
            '~True - Draws dimension text between the extension lines even if it would ordinarily be placed outside those lines
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMTIX)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTIX, value)
            End Set
        End Property

        Public Property FractionStackType() As dxxDimFractionStyles
            '^controls how fractions are displayed
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMFRAC)
            End Get
            Set(value As dxxDimFractionStyles)
                If Not dxfEnums.Validate(GetType(dxxDimFractionStyles), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMFRAC, value)
            End Set

        End Property

        Public Property IsDirty() As Boolean
            Get
                Return _Entry.IsDirty
            End Get
            Set(value As Boolean)
                _Entry.IsDirty = value
            End Set
        End Property


        Friend ReadOnly Property InpuStructure() As TDIMINPUT
            Get

                Return New TDIMINPUT With {
             .CenterMarkSize = CenterMarkSize,
             .DimLineColor = DimLineColor,
             .DimStyleName = Name,
             .ExtLineColor = ExtLineColor,
             .Linetype = "ByLayer",
             .Color = dxxColors.dxfByLayer,
             .Prefix = Prefix,
             .Suffix = Suffix,
             .TextColor = TextColor,
             .TextStyleName = TextStyleName}
            End Get

        End Property


        Public Property LinearFormatType(Optional bAltUnits As Boolean = False) As dxxLinearUnitFormats
            '^controls how dimension text is displayed for linear dimensions
            Get
                If Not bAltUnits Then
                    Return _Entry.PropValueI(dxxDimStyleProperties.DIMLUNIT)
                Else
                    Return _Entry.PropValueI(dxxDimStyleProperties.DIMALTU)
                End If

            End Get

            Set(value As dxxLinearUnitFormats)
                If Not dxfEnums.Validate(GetType(dxxLinearUnitFormats), value, bSkipNegatives:=True) Then Return

                If Not bAltUnits Then
                    PropValueSet(dxxDimStyleProperties.DIMLUNIT, value)
                Else
                    PropValueSet(dxxDimStyleProperties.DIMALTU, value)
                End If
            End Set

        End Property

        Public Property LinearPrecision() As Integer
            '^the number of decimal places for formating linear dimensions
            '~0 - 8
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMDEC)
            End Get
            Set(value As Integer)
                PropValueSet(dxxDimStyleProperties.DIMDEC, value)
            End Set
        End Property

        Public Property LinearScaleFactor() As Double
            '^the scale factor applied to all linear dimensions
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMLFAC)
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                PropValueSet(dxxDimStyleProperties.DIMLFAC, value)
            End Set
        End Property

        Friend Property LineSuppressionFlags() As TDIMLINESPRESSION
            Get
                Return New TDIMLINESPRESSION(SuppressDimLine1, SuppressDimLine2, SuppressExtLine1, SuppressExtLine2)
            End Get
            Set(value As TDIMLINESPRESSION)
                If IsNothing(value) Then Return
                SuppressDimLine1 = TVALUES.ToBoolean(value.SuppressDimLine1)
                SuppressDimLine2 = TVALUES.ToBoolean(value.SuppressDimLine2)
                SuppressExtLine1 = TVALUES.ToBoolean(value.SuppressExtLine1)
                SuppressExtLine2 = TVALUES.ToBoolean(value.SuppressExtLine2)
            End Set
        End Property


        Public Property Prefix() As String
            '^the prefix added to all dimensions created under this style
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMPREFIX)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMPREFIX, value)
            End Set
        End Property

        Public Property RoundTo() As Double
            '^how dimensions units are rounded
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMRND)
            End Get
            Set(value As Double)
                If value < 0 Then Return
                PropValueSet(dxxDimStyleProperties.DIMRND, value)
            End Set

        End Property

        Public Property Suffix() As String
            '^the suffix added to all dimensions created under this style
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMSUFFIX, value)
            End Set
        End Property


        Public Property SuppressDimLine1() As Boolean
            '^Suppresses display of the first dimension line.
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMSD1)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSD1, value)
            End Set
        End Property

        Public Property SuppressDimLine2() As Boolean
            '^Suppresses display of the second dimension line.
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMSD2)
            End Get

            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSD2, value)
            End Set
        End Property

        Public Property SuppressExteriorDimLines() As Boolean
            '^Suppresses drawing of dimension lines outside the extension lines.
            '~False - dimension Line Is Not Suppressed.
            '~On - dimension Line Is Suppressed.
            '~If the dimension lines would be outside the extension lines and DIMTIX is on, setting DIMSOXD to On suppresses the dimension line. If DIMTIX is off, DIMSOXD has no effect.
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMSOXD)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSOXD, value)
            End Set
        End Property

        Public Property SuppressExtLine1() As Boolean
            '^Suppresses display of the first extension line.
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMSE1)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSE1, value)
            End Set
        End Property

        Public Property SuppressExtLine2() As Boolean
            '^Suppresses display of the second extension line.
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMSE2)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSE2, value)
            End Set
        End Property

        Public Property TextColor() As dxxColors
            '^the color of the text drawn under this style
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMCLRT)
            End Get
            Set(value As dxxColors)
                If value = dxxColors.dxfUndefined Then Return
                PropValueSet(dxxDimStyleProperties.DIMCLRT, value)
            End Set
        End Property

        Public Property TextFit() As dxxDimTextFitTypes
            '^Determines how dimension text and arrows are arranged when space is not sufficient to place both within the extension lines.
            '~0 Places both text and arrows outside extension lines
            '~1 Moves arrows first, then text
            '~2 Moves text first, then arrows
            '~3 Moves either text or arrows, whichever fits best

            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMATFIT)
            End Get

            Set(value As dxxDimTextFitTypes)
                If Not dxfEnums.Validate(GetType(dxxDimTextFitTypes), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMATFIT, value)

            End Set

        End Property

        Public Property TextGap() As Double
            '^Sets the distance around the dimension text when the dimension line
            '^breaks to accommodate dimension text. Also sets the gap between
            '^annotation and a hook line created with leaders.
            '^If you enter a negative value, a box is places around the dimension text.
            '^the size of the arrowheads drawn under this style
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMGAP)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMGAP, value)
            End Set
        End Property

        Public Property TextAngle() As Double
            '^the angle of text draw under this style
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMTANGLE)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMTANGLE, value)
            End Set
        End Property

        Public Property TextHeight() As Double
            '^the size of text draw under this style
            Get
                Return _Entry.PropValueD(dxxDimStyleProperties.DIMTXT)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMTXT, value)
            End Set
        End Property

        Public Property TextInsideHorizontal() As Boolean
            '^Controls the position of dimension text inside the extension lines for all dimension types except Ordinate.
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMTIH)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTIH, value)
            End Set
        End Property

        Public ReadOnly Property TextMovement() As dxxDimTextMovementTypes
            '^Suppresses drawing of dimension lines outside the extension lines.
            '~False - dimension Line Is Not Suppressed.
            '~On - dimension Line Is Suppressed.
            '~If the dimension lines would be outside the extension lines and DIMTIX is on, setting DIMSOXD to On suppresses the dimension line. If DIMTIX is off, DIMSOXD has no effect.
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMTMOVE)
            End Get
        End Property

        Public Property TextOutsideHorizontal() As Boolean
            '^Controls the position of dimension text outside the extension lines.
            Get
                Return _Entry.PropValueB(dxxDimStyleProperties.DIMTOH)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTOH, value)
            End Set
        End Property

        Public Property TextPositionH() As dxxDimJustSettings
            '^Controls the horizontal positioning of dimension text.
            '~Centered = 0   'Positions the text above the dimension line and center-justifies it between the extension lines
            '~Ext1 = 1 'Positions the text next to the first extension line
            '~Ext2 = 2 'Positions the text next to the second extension line
            '~AlignExt1 = 3 'Positions the text above and aligned with the first extension line
            '~AlignExt2 = 4 'Positions the text above and aligned with the second extension line
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMJUST)
            End Get
            Set(value As dxxDimJustSettings)
                If Not dxfEnums.Validate(GetType(dxxDimJustSettings), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMJUST, value)
            End Set
        End Property

        Public Property TextPositionV() As dxxDimTadSettings
            '^Controls the vertical position of text in relation to the dimension line.
            '~Centered = 0 text center lies in line with the dimension lines
            '~Above = 1 Places the dimension text above the dimension line except when _
            '~the dimension line is not horizontal and text inside the extension lines is forced _
            '~horizontal (DIMTIH = 1). The distance from the dimension line to the baseline of _
            '~the lowest line of text is the current DIMGAP value.
            '~OppositeSide = 2 Places the dimension text on the side of the dimension line_
            '~farthest away from the defining points.
            '~JIS = 3 Places the dimension text to conform to Japanese Industrial Standards (JIS).
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMTAD)
            End Get
            Set(value As dxxDimTadSettings)
                If Not dxfEnums.Validate(GetType(dxxDimTadSettings), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMTAD, value)
            End Set
        End Property

        Friend WriteOnly Property TextStyle(newobj As dxoStyle)
            '^client access to set the text style
            Set
                If Not IsNothing(Value) Then
                    PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, Value.Name)
                    PropValueSet(dxxDimStyleProperties.DIMTXSTY, Value.Handle)
                Else
                    PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, "Standard")
                    PropValueSet(dxxDimStyleProperties.DIMTXSTY, "")
                End If
            End Set

        End Property

        Public Property TextStyleHandle() As String
            '^the handle of the referenced text style
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMTXSTY)
            End Get
            Friend Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMTXSTY, value)
            End Set
        End Property

        Public Property TextStyleName() As String

            '^the name of the referenced text style
            Get
                Return _Entry.PropValueStr(dxxDimStyleProperties.DIMTXSTY_NAME)
            End Get

            Set(value As String)

                '^the name of the referenced text style
                value = Trim(value)
                If value = "" Then value = "Standard"
                If value <> "" Then If PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, value) Then TextStyleHandle = ""

            End Set

        End Property

        Public Property ZeroSuppression(Optional bTolerance As Boolean = False) As dxxZeroSuppression
            '^controls zero suppression for linear dimension text
            Get
                If Not bTolerance Then
                    Return _Entry.PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION)
                Else
                    Return _Entry.PropValueI(dxxDimStyleProperties.DIMTOLZEROSUPPRESSION)
                End If

            End Get
            Set(value As dxxZeroSuppression)
                If Not dxfEnums.Validate(GetType(dxxZeroSuppression), CInt(value)) Then Return
                If Not bTolerance Then
                    If PropValueSet(dxxDimStyleProperties.DIMZEROSUPPRESSION, value) Then
                        PropValueSet(dxxDimStyleProperties.DIMZIN, value + ZeroSuppressionArchitectural)
                    End If
                Else

                End If

            End Set
        End Property

        Public Property ZeroSuppressionAngular() As dxxZeroSuppression
            '^controls zero suppression for angular dimension text
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMAZIN)
            End Get

            Set(value As dxxZeroSuppression)
                If Not dxfEnums.Validate(GetType(dxxZeroSuppression), CInt(value)) Then Return
                PropValueSet(dxxDimStyleProperties.DIMAZIN, value)
            End Set

        End Property

        Public Property ZeroSuppressionArchitectural() As dxxZeroSuppressionsArchitectural
            '^controls zero suppression for linear dimension text when architectural formating is on
            Get
                Return _Entry.PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH)
            End Get
            Set(value As dxxZeroSuppressionsArchitectural)
                If Not dxfEnums.Validate(GetType(dxxZeroSuppressionsArchitectural), CInt(value)) Then Return
                If PropValueSet(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH, value) Then
                    PropValueSet(dxxDimStyleProperties.DIMZIN, value + ZeroSuppression)
                End If
            End Set

        End Property

        Public Property ImageGUID() As String
            Get
                Return _Entry.ImageGUID
            End Get

            Friend Set(value As String)
                _Entry.ImageGUID = value
            End Set
        End Property

        Public Property Name() As String Implements idxfSettingsObject.Name

            Get

                Return _Entry.Name
            End Get
            Set(value As String)
                value = value.Trim
                If String.IsNullOrEmpty(value) Then value = "Standard"
                If String.Compare(value, _Entry.Name, ignoreCase:=True) = 0 Then Return
                Dim myImg As dxfImage = Nothing
                If Not GetImage(myImg) Then
                    _Entry.Name = TVALUES.To_STR(value, "Standard", bTrim:=True)
                Else
                    Dim aEntry As TTABLEENTRY = Nothing
                    value = myImg.GetOrAdd(dxxReferenceTypes.DIMSTYLE, value, aEntry)
                    _Entry.Name = aEntry.Name
                    _Entry = TPROPERTIES.CopyDimStyleProps(_Entry, aEntry)
                    myImg.Header.DimStyleName = _Entry.Name
                    myImg.BaseSettings(dxxReferenceTypes.DIMOVERRIDES) = _Entry
                End If


            End Set
        End Property

        Friend Property Strukture() As TTABLEENTRY
            Get
                Return _Entry
            End Get
            Set(value As TTABLEENTRY)
                If value.EntryType = dxxReferenceTypes.DIMSTYLE Then
                    _Entry = TPROPERTIES.CopyDimStyleProps(_Entry, value)
                    Name = value.Name
                ElseIf value.EntryType = dxxReferenceTypes.DIMOVERRIDES Then
                    _Entry = value
                ElseIf value.EntryType = dxxReferenceTypes.HEADER Then
                    _Entry.Props = TPROPERTIES.CopyDimStyleProps(_Entry.Props, value.Props, False, True)
                    Name = value.Props.ValueStr("$DIMSTYLE")
                End If

            End Set
        End Property

        Friend Property Props() As TPROPERTIES

            Get
                Return _Entry.Props

            End Get

            Set(value As TPROPERTIES)
                If value.Count <= 0 Then Return
                _Entry.Props = TPROPERTIES.CopyDimStyleProps(_Entry.Props, value, False, False)


                PropValueSet(dxxDimStyleProperties.DIMPOST, dxfUtils.DimPrefixSuffix(_Entry.PropValueStr(dxxDimStyleProperties.DIMPREFIX), _Entry.PropValueStr(dxxDimStyleProperties.DIMSUFFIX)))
                PropValueSet(dxxDimStyleProperties.DIMAPOST, dxfUtils.DimPrefixSuffix(_Entry.PropValueStr(dxxDimStyleProperties.DIMAPREFIX), _Entry.PropValueStr(dxxDimStyleProperties.DIMASUFFIX)))

            End Set

        End Property

        Public ReadOnly Property Properties() As dxoProperties
            '^returns a copy of the entries current properties
            Get
                Return New dxoProperties(_Entry.Props)
            End Get
        End Property



#End Region 'Properties

#Region "Methods"

        Friend Function GetImage(ByRef rImage As dxfImage) As Boolean
            If IsNothing(rImage) Then
                If ImageGUID <> "" Then rImage = goEvents.GetImage(ImageGUID)
            End If
            Return Not IsNothing(rImage)
        End Function


        Friend Sub CopyDimStyleProps(aDimstyleOrHeader As TTABLEENTRY, bHeaderPassed As Boolean, Optional aSkipList As String = "2,1")
            _Entry = TPROPERTIES.CopyDimStyleProps(_Entry, aDimstyleOrHeader, aSkipList)

        End Sub


        Public Sub ClearLineSuppressions()
            PropValueSet(dxxDimStyleProperties.DIMSD1, False)
            PropValueSet(dxxDimStyleProperties.DIMSD2, False)
            PropValueSet(dxxDimStyleProperties.DIMSE1, False)
            PropValueSet(dxxDimStyleProperties.DIMSE2, False)
        End Sub




        Friend Sub GetArrowHeadNames(bIncludeUnderScores As Boolean, ByRef sDimBlk1 As String, ByRef sDimBlk2 As String, ByRef sDimLdrBlk As String, bNoClosed As Boolean)
            sDimLdrBlk = _Entry.PropValueStr(dxxDimStyleProperties.DIMLDRBLK_NAME)
            sDimBlk1 = _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK1_NAME)
            sDimBlk2 = _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK2_NAME)

            If bNoClosed Then
                If StrComp(sDimLdrBlk, "_ClosedFilled", vbTextCompare) = 0 Then sDimLdrBlk = ""
                If StrComp(sDimBlk1, "_ClosedFilled", vbTextCompare) = 0 Then sDimBlk1 = ""
                If StrComp(sDimBlk2, "_ClosedFilled", vbTextCompare) = 0 Then sDimBlk2 = ""
            End If

            If Not bIncludeUnderScores Then
                If Left(sDimLdrBlk, 1) = "_" Then sDimLdrBlk = Right(sDimLdrBlk, Len(sDimLdrBlk) - 1)
                If Left(sDimBlk1, 1) = "_" Then sDimBlk1 = Right(sDimBlk1, Len(sDimBlk1) - 1)
                If Left(sDimBlk2, 1) = "_" Then sDimBlk2 = Right(sDimBlk2, Len(sDimBlk2) - 1)
            End If
        End Sub

        Friend Function GetArrowHead_1(ByRef aImage As dxfImage) As dxfBlock

            Return dxfArrowheads.GetArrowHead(aImage, Strukture, "DIMBLK1")


        End Function

        Friend Function GetArrowHead_2(ByRef aImage As dxfImage) As dxfBlock

            Return dxfArrowheads.GetArrowHead(aImage, Strukture, "DIMBLK2")

        End Function

        Friend Function GetArrowHead_Leader(ByRef aImage As dxfImage) As dxfBlock

            Return dxfArrowheads.GetArrowHead(aImage, Strukture, "DIMLDRBLK")

        End Function

        Friend Function GetArrowHeads(ByRef aImage As dxfImage) As Collection


            If Not GetImage(aImage) Then Return New Collection
            Return dxfArrowheads.GetArrowHeads(aImage, Strukture)

        End Function


        Public Function IsEqual(Style As dxoDimStyle, Optional TestName As Boolean = True, Optional TestDimScale As Boolean = False) As Boolean

            '#1the style to compare to
            '#2flag to include name as a matching requirement
            '#3flag to include "DIMSCALE" as a matching requirement

            '^used to see if one style's properties match another

            If Style Is Nothing Then Return False
            Dim GCs As String

            'dxeDimension to see if the passed style has the same property values as this instance
            If TestName Then
                GCs = "2,100,105,330,70"
            Else
                GCs = "100,105,330,70"
            End If

            If Not TestDimScale Then TLISTS.Add(GCs, "40")
            Return Props.Equals(Style.Strukture.Props, GCs)

        End Function


        Friend Function ReferencesBlock(aBlockName As String, aRecordHandle As String) As Boolean


            aBlockName = Trim(aBlockName)
            aRecordHandle = Trim(aRecordHandle)
            If StrComp(ArrowHeadBlock, aBlockName, vbTextCompare) = 0 Then
                Return True

            ElseIf StrComp(ArrowHeadBlock1, aBlockName, vbTextCompare) = 0 Then
                Return True

            ElseIf StrComp(ArrowHeadBlock2, aBlockName, vbTextCompare) = 0 Then
                Return True
            ElseIf StrComp(ArrowHeadBlockLeader, aBlockName, vbTextCompare) = 0 Then
                Return True

            ElseIf StrComp(ArrowHeadBlockHandle, aRecordHandle, vbTextCompare) = 0 Then
                Return True

            ElseIf StrComp(ArrowHeadBlock1Handle, aRecordHandle, vbTextCompare) = 0 Then
                Return True

            ElseIf StrComp(ArrowHeadBlock2Handle, aRecordHandle, vbTextCompare) = 0 Then
                Return True

            ElseIf StrComp(ArrowHeadBlockLeaderHandle, aRecordHandle, vbTextCompare) = 0 Then
                Return True
            Else
                Return False
            End If

        End Function

        Friend Overridable Function PropValueSet(aIndex As [Enum], aValue As Object, Optional aOccur As Integer = 0, Optional ByRef rFound As Boolean = False, Optional ByRef rProp As TPROPERTY = Nothing, Optional bSuppressEvnts As Boolean = False) As Boolean

            Dim _rVal As Boolean
            Dim aImage As dxfImage = Nothing
            Try
                Dim pname As String = ""

                'get the property

                _rVal = _Entry.PropValueSet(aIndex, aValue, aOccur, rFound, rProp)
                If Not rFound Then Return False  'cant find it
                If Not _rVal Then Return False 'no change
                If Not bSuppressEvnts Then
                    'notify the image
                    If GetImage(aImage) Then

                        Dim bDontChange As Boolean
                        aImage.RespondToSettingChange(Me, rProp)

                        If bDontChange Then
                            _Entry.Props.Item(rProp.Name) = rProp
                            Return False
                        End If

                    End If
                End If


                Return True
            Catch ex As Exception
                'add an error
                Return False
                If Not IsNothing(aImage) Then
                    aImage.HandleError("PropValueSet", "dxoSettingsDimStyleOverrides {" & Name & "}", ex.Message)
                Else
                    Throw ex
                End If


            End Try
        End Function

        Public Sub RefreshProperties()
            '^used to reset the properties collection to the initial state

            Props = goProperties.ReferenceProperties(dxxReferenceTypes.DIMSTYLE, Name, aString:=TextStyleName)
        End Sub

        Public Sub Reset()
            ResetPropsToParent(Nothing, True)
        End Sub

        Friend Sub ResetPropsToParent(ByRef aImage As dxfImage, Optional aByNameOnly As Boolean = False)
            If Not GetImage(aImage) Then Return
            Dim aNm As String
            Dim aParent As TTABLEENTRY = Nothing
            aNm = TVALUES.To_STR(aImage.BaseSettings(dxxReferenceTypes.HEADER).GetHeaderVal(dxxHeaderVars.DIMSTYLE), Name)
            aNm = aImage.GetOrAdd(dxxReferenceTypes.DIMSTYLE, aNm, aParent)
            _Entry = TPROPERTIES.CopyDimStyleProps(_Entry, aParent)

            aImage.BaseSettings(dxxReferenceTypes.DIMOVERRIDES) = _Entry

        End Sub

        Public Sub SetAngularFormat(Optional bNoLead As Boolean = False, Optional bNoTrail As Boolean = False)
            '^the format string applied to linear numbers


            If Not bNoLead And Not bNoTrail Then
                ZeroSuppressionAngular = dxxZeroSuppression.None
            Else
                If bNoLead And bNoTrail Then
                    ZeroSuppressionAngular = dxxZeroSuppression.LeadingAndTrailing
                Else
                    If bNoLead Then
                        ZeroSuppressionAngular = dxxZeroSuppression.Leading
                    End If
                    If bNoTrail Then
                        ZeroSuppressionAngular = dxxZeroSuppression.Trailing
                    End If
                End If
            End If
        End Sub

        Public Sub SetArrowHeads(Optional aArrowHead1 As dxxArrowHeadTypes = dxxArrowHeadTypes.Undefined, Optional aArrowHead2 As dxxArrowHeadTypes = dxxArrowHeadTypes.Undefined, Optional aArrowHeadLeader As dxxArrowHeadTypes = dxxArrowHeadTypes.Undefined)
            Dim bname0 As String = _Entry.PropValueStr(dxxDimStyleProperties.DIMBLK_NAME)
            If String.IsNullOrEmpty(bname0) Then bname0 = "_ClosedFilled"
            Dim bname1 As String = ArrowHeadBlock1
            If String.IsNullOrEmpty(bname1) Then bname1 = bname0
            Dim bname2 As String = ArrowHeadBlock2
            If String.IsNullOrEmpty(bname2) Then bname2 = bname0
            Dim bname3 As String = ArrowHeadBlockLeader
            If String.IsNullOrEmpty(bname3) Then bname3 = "_ClosedFilled"
            Dim bname As String
            Dim cnhgs(2) As Boolean

            If dxfEnums.Validate(GetType(dxxArrowHeadTypes), aArrowHead1, "20", bSkipNegatives:=True) Then
                bname = dxfArrowheads.GetBlockName(aArrowHead1)
                If String.Compare(bname, bname1, ignoreCase:=True) <> 0 Then
                    cnhgs(0) = True
                    bname1 = bname
                End If
            End If
            If dxfEnums.Validate(GetType(dxxArrowHeadTypes), aArrowHead2, "20", bSkipNegatives:=True) Then
                bname = dxfArrowheads.GetBlockName(aArrowHead2)
                If String.Compare(bname, bname2, ignoreCase:=True) <> 0 Then
                    cnhgs(1) = True
                    bname2 = bname
                End If
            End If
            If dxfEnums.Validate(GetType(dxxArrowHeadTypes), aArrowHeadLeader, "20", bSkipNegatives:=True) Then
                bname = dxfArrowheads.GetBlockName(aArrowHeadLeader)
                If String.Compare(bname, bname3, ignoreCase:=True) <> 0 Then
                    cnhgs(2) = True
                    bname3 = bname
                End If
            End If

            If Not cnhgs(0) And Not cnhgs(1) And Not cnhgs(2) Then Return

            Dim myImg As dxfImage = Nothing
            Dim bimb As Boolean = GetImage(myImg)
            If cnhgs(0) Then
                PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, bname1)
            End If
            If cnhgs(1) Then
                PropValueSet(dxxDimStyleProperties.DIMBLK2_NAME, bname2)
            End If
            If cnhgs(2) Then
                PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, bname2)
            End If
            If String.Compare(bname1, bname2, ignoreCase:=True) = 0 Then
                PropValueSet(dxxDimStyleProperties.DIMBLK_NAME, bname1)

            End If
            If bimb Then dxfImageTools.VerifyDimstyle(myImg, _Entry, myImg.Blocks, False)


        End Sub

        Public Sub SetColors(Optional aLineColor As dxxColors = dxxColors.dxfUndefined, Optional aTextColor As dxxColors = dxxColors.dxfUndefined)
            '^set the extension line and dimension line colors in a single call
            If aLineColor <> dxxColors.dxfUndefined Then
                ExtLineColor = aLineColor
                DimLineColor = aLineColor
            End If
            If aTextColor <> dxxColors.dxfUndefined Then TextColor = aTextColor
        End Sub

        Public Sub SetGapsAndOffsets(aGap As Double, Optional aArrowSize As Object = Nothing, Optional aTextGap As Object = Nothing)
            '^set the extension line offset, extension line extension and text gap in one call
            '~a negative value cause text blocks around text
            Dim tgapp As Double
            tgapp = aGap
            If Not IsNothing(aTextGap) Then tgapp = TVALUES.To_DBL(aTextGap)
            TextGap = tgapp
            ExtLineExtend = Math.Abs(aGap)
            ExtLineOffset = Math.Abs(aGap)
            If Not IsNothing(aArrowSize) Then ArrowSize = TVALUES.To_DBL(aArrowSize)

        End Sub

        Public Sub SetLinearFormat(Optional bNoLead As Boolean = False, Optional bNoTrail As Boolean = False)
            '^the format string applied to linear numbers

            If Not bNoLead And Not bNoTrail Then
                ZeroSuppression = dxxZeroSuppression.None
            Else
                If bNoLead And bNoTrail Then
                    ZeroSuppression = dxxZeroSuppression.LeadingAndTrailing
                Else
                    If bNoLead Then
                        ZeroSuppression = dxxZeroSuppression.Leading
                    End If
                    If bNoTrail Then
                        ZeroSuppression = dxxZeroSuppression.Trailing
                    End If
                End If
            End If
        End Sub

        Public Function SetProperty(ByRef aProperty As dxxDimStyleProperties, aValue As Object) As Boolean

            '#1the property to search for
            '#2the Value to set the property value to

            '^searches for a style property by name and sets it value if it is found
            '~search is not case sensitive. an error is raised if the property requested is not found
            Return PropValueSet(aProperty, aValue)

        End Function

        Public Sub SetSuppressionFlags(Optional aSupDimLn1 As Object = Nothing, Optional aSupDimLn2 As Object = Nothing, Optional aSupExtLn1 As Object = Nothing, Optional aSupExtLn2 As Object = Nothing)
            If TVALUES.IsBoolean(aSupDimLn1) Then SuppressDimLine1 = TVALUES.ToBoolean(aSupDimLn1)
            If TVALUES.IsBoolean(aSupDimLn2) Then SuppressDimLine2 = TVALUES.ToBoolean(aSupDimLn2)
            If TVALUES.IsBoolean(aSupExtLn1) Then SuppressExtLine1 = TVALUES.ToBoolean(aSupExtLn1)
            If TVALUES.IsBoolean(aSupExtLn2) Then SuppressExtLine2 = TVALUES.ToBoolean(aSupExtLn2)
        End Sub


        Friend Function SetValue(aIndex As dxxDimStyleProperties, aValue As Object, Optional bSuppressEvnts As Boolean = False) As Boolean

            Dim _rVal As Boolean = False
            Try
                _rVal = PropValueSet(aIndex, aValue, 0)


            Catch ex As Exception
                If Not bSuppressEvnts Then
                    Dim aImage As dxfImage = Nothing
                    If GetImage(aImage) Then
                        aImage.HandleError("SetValue", "dxoDimStlye {" & EntryTypeName() & "}", ex.Message)
                    End If
                Else
                    Throw ex
                End If

            End Try
            Return _rVal


        End Function

        Public Function SetZeroSuppressionDecimal(bNoLead As Boolean, bNoTrail As Boolean, Optional ByVal aLinearPrecision As Integer = -1, Optional ByVal aAngularPrecision As Integer = -1) As dxxZeroSuppression

            Dim _rVal As dxxZeroSuppression

            _rVal = dxxZeroSuppression.None
            If bNoLead Then _rVal += dxxZeroSuppression.Leading
            If bNoTrail Then _rVal += dxxZeroSuppression.Trailing
            ZeroSuppression = _rVal

            Dim aZn As dxxZeroSuppression
            aZn = ZeroSuppression
            If aZn = dxxZeroSuppression.LeadingAndTrailing Then
                ZeroSuppressionAngular = dxxZeroSuppression.LeadingAndTrailing
            ElseIf aZn = dxxZeroSuppression.Leading Then
                ZeroSuppressionAngular = dxxZeroSuppression.Leading
            ElseIf aZn = dxxZeroSuppression.None Then
                ZeroSuppressionAngular = dxxZeroSuppression.None
            ElseIf aZn = dxxZeroSuppression.Trailing Then
                ZeroSuppressionAngular = dxxZeroSuppression.Trailing
            End If

            If aLinearPrecision >= 0 Then LinearPrecision = aLinearPrecision
            If aAngularPrecision >= 0 Then AngularPrecision = aAngularPrecision

            Return _rVal

        End Function


        Public Function UpdateToImage(Optional aStyleName As String = "", Optional aSkipList As String = "", Optional ByRef aImage As dxfImage = Nothing) As Boolean

            Dim _rVal As Boolean
            aStyleName = Trim(aStyleName)

            '#1the parent style to copy the properties from
            '#2a list of group codes or names of properties to ignore
            '^If the Style is a copy of a style in the parent images style table
            '^the current properties of the parent style are copied to this style.
            '~if an alternate name is passed the properties of the passed style are copied.

            If Not GetImage(aImage) Then Return _rVal

            Dim myName As String = Name
            If aStyleName = "" Then aStyleName = myName

            Dim aStyles As TTABLE = aImage.BaseTable(dxxReferenceTypes.DIMSTYLE)
            Dim aDStyle As TTABLEENTRY = aImage.BaseTable(dxxReferenceTypes.DIMSTYLE).Entry(aStyleName)
            If aDStyle.Index <= 0 Then Return _rVal

            Dim i As Integer
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim bSkipList As String = "" '({"Invisible", "ByBlock", "ByLayer", "Continuous"})
            Dim wuz As Boolean = IsDirty
            Dim idx As Integer = aDStyle.Index
            Dim chngdProps As TPROPERTIES



            Dim askipLst As New TLIST(",", aSkipList)
            askipLst.AddList("2,100,0")

            If aStyleName = "" Then aStyleName = myName Else askipLst.Add(105)
            Dim bskipLst As New TLIST(",", "DIMTXSTY,DIMLDRBLK,DIMBLK,DIMBL1,DIMBLK2,DIMLTYPE,DIMLTEX1,DIMLTEX2") 'arrow block and linetype handles




            Dim myStruc As TTABLEENTRY = Strukture

            myStruc.ExtendedData = aDStyle.ExtendedData
            myStruc.Values = aDStyle.Values
            chngdProps = Props.Clone(bReturnEmpty:=True)

            '
            For i = 2 To myStruc.Props.Count
                aProp = myStruc.Props.Item(i)

                If Not askipLst.Contains(aProp.GroupCode) And Not askipLst.Contains(aProp.Name) Then
                    bProp = aDStyle.Props.Item(i)

                    If aProp.SetValue(bProp.Value) Then
                        If Not aProp.Hidden Then
                            chngdProps.Add(aProp)
                        End If

                    End If
                    myStruc.Props.Item(i) = aProp
                End If

            Next i
            If chngdProps.Count > 0 Then
                _rVal = True
                IsDirty = True
            End If

            For i = 2 To myStruc.Props.Count
                aProp = myStruc.Props.Item(i)


                If Not bskipLst.Contains(aProp.Name) Then
                    bProp = aDStyle.Props.Item(i)
                    If aProp.SetValue(bProp.Value) Then
                        chngdProps.Add(aProp)
                    End If
                End If
                myStruc.Props.Item(i) = aProp

            Next i

            If _rVal Then

                aImage.BaseSettings(dxxReferenceTypes.DIMOVERRIDES) = myStruc
                For i = 1 To chngdProps.Count
                    aProp = chngdProps.Item(i)
                    aImage.RespondToSettingChange(Me, aProp)
                Next


            End If
            Strukture = myStruc

            Return _rVal

        End Function

#End Region 'Methods
    End Class

End Namespace
