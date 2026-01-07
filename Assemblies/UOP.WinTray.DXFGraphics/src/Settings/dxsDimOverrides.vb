Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke

Namespace UOP.DXFGraphics
    Public Class dxsDimOverrides
        Inherits dxfSettingObject
        Implements ICloneable

#Region "Constructors"
        Friend Sub New()
            MyBase.New(dxxSettingTypes.DIMOVERRIDES)
        End Sub
        Friend Sub New(aImage As dxfImage)
            MyBase.New(dxxSettingTypes.DIMOVERRIDES, aImage)
        End Sub

        Friend Sub New(aSettings As dxsDimOverrides)
            MyBase.New(dxxSettingTypes.DIMOVERRIDES)
            MyBase.Copy(aSettings)

            If aSettings IsNot Nothing Then
                If aSettings.SettingType = SettingType Then
                End If
            End If
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Overrides Property Name As String
            Get
                Return Properties.ValueS(2, aDefault:="Standard")
            End Get
            Set(value As String)
                Properties.SetVal(2, value)
            End Set
        End Property


        Public Property AngularPrecision As Integer
            '^the number of decimal places for formating angular dimensions
            '~if set to -1 then angular precision follows DIMDEC
            Get
                Return PropValueI(dxxDimStyleProperties.DIMADEC)
            End Get
            Set(value As Integer)
                PropValueSet(dxxDimStyleProperties.DIMADEC, value)
            End Set
        End Property
        Public Property AngUnits As dxxAngularUnits
            '^Sets the units format for angular dimensions.
            '~DegreesDecimal = 0 = Decimal degrees.
            '~DegreesMinSec = 1 = Degrees Minutes Seconds.
            '~Gradians = 2 = gradians.
            '~Radians = 3 = Radians.
            Get
                Return PropValueI(dxxDimStyleProperties.DIMAUNIT)
            End Get
            Set(value As dxxAngularUnits)
                If Not dxfEnums.Validate(GetType(dxxAngularUnits), value, bSkipNegatives:=True) Then Return
                PropValueSet(dxxDimStyleProperties.DIMAUNIT, value)
            End Set
        End Property
        Public Property ArrowHead1 As dxxArrowHeadTypes
            '^the type of the first arrowhead block
            Get
                Return dxfArrowheads.GetBlockType(ArrowHeadBlock1)
            End Get
            Set(value As dxxArrowHeadTypes)
                PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, dxfArrowheads.GetBlockName(value))
            End Set
        End Property
        Public Property ArrowHead2 As dxxArrowHeadTypes
            '^the type of the second arrowhead block
            Get
                Return dxfArrowheads.GetBlockType(ArrowHeadBlock2)
            End Get
            Set(value As dxxArrowHeadTypes)
                If value < 0 Then Return
                PropValueSet(dxxDimStyleProperties.DIMBLK2_NAME, dxfArrowheads.GetBlockName(value))
            End Set
        End Property
        Public Property ArrowHeadBlock As String
            '^the name of the both arrowhead blocks
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMBLK_NAME)
            End Get
            Set(value As String)
                dxfArrowheads.IsDefault(value)
                PropValueSet(dxxDimStyleProperties.DIMBLK_NAME, value)
            End Set
        End Property
        Public Property ArrowHeadBlock1 As String
            '^the name of the first arrowhead block
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMBLK1_NAME)
            End Get
            Set(value As String)
                dxfArrowheads.IsDefault(value)
                PropValueSet(dxxDimStyleProperties.DIMBLK1_NAME, value)
            End Set
        End Property
        Public ReadOnly Property ArrowHeadBlock1Handle As String
            '^the block record handle of the first arrowhead block
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMBLK1)
            End Get
        End Property
        Public Property ArrowHeadBlock2 As String
            '^the name of the second arrowhead block
            Get
                Dim _rVal As String = PropValueStr(dxxDimStyleProperties.DIMBLK2_NAME)
                If _rVal = "" Then _rVal = "_ClosedFilled"
                Return _rVal
            End Get
            Set(value As String)
                If value = String.Empty Then value = "_ClosedFilled"
                PropValueSet(dxxDimStyleProperties.DIMBLK_NAME, value)
            End Set
        End Property
        Public ReadOnly Property ArrowHeadBlock2Handle As String
            '^the block record handle of the second arrowhead block
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMBLK2)
            End Get
        End Property
        Public ReadOnly Property ArrowHeadBlockHandle As String
            '^the block record handle of the both arrowhead blocks
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMBLK)
            End Get
        End Property
        Public Property ArrowHeadBlockLeader As String
            '^the name of the leader arrowhead block
            Get
                ArrowHeadBlockLeader = PropValueStr(dxxDimStyleProperties.DIMLDRBLK_NAME)
                If ArrowHeadBlockLeader = "" Then ArrowHeadBlockLeader = "_ClosedFilled"
                Return ArrowHeadBlockLeader
            End Get
            Set(value As String)
                If value = String.Empty Then value = "_ClosedFilled"
                dxfArrowheads.IsDefault(value)
                PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, value)
            End Set
        End Property
        Public ReadOnly Property ArrowHeadBlockLeaderHandle As String
            '^the block record handle of the leader arrowhead block
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMLDRBLK)
            End Get
        End Property
        Public Property ArrowHeadLeader As dxxArrowHeadTypes
            '^the type of the second arrowhead block
            Get
                Return dxfArrowheads.GetBlockType(ArrowHeadBlockLeader)
            End Get
            Set(value As dxxArrowHeadTypes)
                '^the type of the second arrowhead block
                PropValueSet(dxxDimStyleProperties.DIMLDRBLK_NAME, dxfArrowheads.GetBlockName(value))
            End Set
        End Property
        '^the type of the both arrowhead blocks
        Public Property ArrowHeads As dxxArrowHeadTypes
            Get
                Dim aName As String
                aName = ArrowHeadBlock
                If aName <> "" Then Return dxfArrowheads.GetBlockType(aName) Else Return dxxArrowHeadTypes.Various
            End Get
            Set(value As dxxArrowHeadTypes)
                PropValueSet(dxxDimStyleProperties.DIMBLK_NAME, dxfArrowheads.GetBlockName(value))
            End Set
        End Property
        Public Property ArrowSize As Double
            '^the size of the arrowheads drawn under this style
            Get
                Return PropValueD(dxxDimStyleProperties.DIMASZ)
            End Get
            Set(value As Double)
                If value >= 0 Then PropValueSet(dxxDimStyleProperties.DIMASZ, value)
            End Set
        End Property
        Public Property ArrowTickSize As Double
            '^Specifies the size of oblique strokes drawn instead of arrowheads for linear, radius, and diameter dimensioning.
            '~0 Draws arrowheads.
            '~>0 Draws oblique strokes instead of arrowheads. The size of the oblique strokes is determined by this value multiplied by the DIMSCALE value
            Get
                Return PropValueD(dxxDimStyleProperties.DIMTSZ)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMTSZ, value)
            End Set
        End Property
        Public Property CenterMarkSize As Double
            '^the size of center marks for radial and diametric dimensions
            '~negative values indicate that centerlines should also be drawn
            Get
                Return PropValueD(dxxDimStyleProperties.DIMCEN)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMCEN, value)
            End Set
        End Property
        Public Property DecimalSeperator As String
            '^the character used to as the decimal seperator
            '~default  = "."
            Get
                Return Chr(PropValueI(dxxDimStyleProperties.DIMDSEP))
            End Get
            Set(value As String)
                value = value.Trim()
                If value.Length < 1 Then Return
                If value.Length > 11 Then value = value.Substring(0, 1)
                PropValueSet(dxxDimStyleProperties.DIMDSEP, Asc(value))
            End Set
        End Property
        Public Property DimLineColor As dxxColors
            '^the color of the dimension lines
            '~also applies to leader lines
            Get
                Return PropValueI(dxxDimStyleProperties.DIMCLRD)
            End Get
            Set(value As dxxColors)
                PropValueSet(dxxDimStyleProperties.DIMCLRD, value)
            End Set
        End Property
        Public Property DimLinetype As String
            '^the linetype of the dimension lines
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMLTYPE_NAME)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMLTYPE_NAME, value)
            End Set
        End Property
        Public Property DimLineWeight As dxxLineWeights
            '^the lineweight of the dimension lines
            '~also applies to leader lines
            Get
                Return PropValueI(dxxDimStyleProperties.DIMLWD)
            End Get
            Set(value As dxxLineWeights)
                If value > dxxLineWeights.ByDefault Then PropValueSet(dxxDimStyleProperties.DIMLWD, value)
            End Set
        End Property
        Public Property DimTextMovement As dxxDimTextMovementTypes
            '^Controls how text is positioned if the default poisition can't be used
            Get
                Return PropValueI(dxxDimStyleProperties.DIMTMOVE)
            End Get
            Set(value As dxxDimTextMovementTypes)
                If Not dxfEnums.Validate(GetType(dxxDimTextMovementTypes), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMTMOVE, value)
            End Set
        End Property
        Public Property ExtLineColor As dxxColors
            '^the color of the extension lines drawn under this style
            Get
                Return PropValueI(dxxDimStyleProperties.DIMCLRE)
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined Then PropValueSet(dxxDimStyleProperties.DIMCLRE, value)
            End Set
        End Property
        Public Property ExtLineExtend As Double
            '^Specifies how far to extend the extension line beyond the dimension line.
            Get
                Return PropValueD(dxxDimStyleProperties.DIMEXE)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMEXE, value)
            End Set
        End Property
        Public Property ExtLineOffset As Double
            '^Specifies how far extension lines are offset from origin points. With fixed-length
            '^extension lines, this value determines the minimum offset.
            Get
                Return PropValueD(dxxDimStyleProperties.DIMEXO)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMEXO, value)
            End Set
        End Property
        Public Property ExtLinetype1 As String
            '^the linetype of the first extension line lines
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMLTEX1_NAME)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMLTEX1_NAME, value)
            End Set
        End Property
        Public Property ExtLinetype2 As String
            '^the linetype of the second extension line lines
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMLTEX2_NAME)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMLTEX2_NAME, value)
            End Set
        End Property
        Public Property ExtLineWeight As dxxLineWeights
            '^the lineweight of the extensions lines
            Get
                Return PropValueI(dxxDimStyleProperties.DIMLWE)
            End Get
            Set(value As dxxLineWeights)
                If value > dxxLineWeights.ByDefault Then PropValueSet(dxxDimStyleProperties.DIMLWE, value)
            End Set
        End Property
        Public Property FeatureScaleFactor As Double
            '^the scale factor applied to all dimension features
            Get
                Return PropValueD(dxxDimStyleProperties.DIMSCALE)
            End Get
            Set(value As Double)
                If value > 0 Then PropValueSet(dxxDimStyleProperties.DIMSCALE, value)
            End Set
        End Property
        Public Property ForceDimLines As Boolean
            '^Controls whether a dimension line is drawn between the extension lines even when the text is placed outside. For radius and diameter dimensions (when DIMTIX is off), draws a dimension line inside the circle or arc and places the text, arrowheads, and leader outside.
            '~False - Does not draw dimension lines between the measured points when arrowheads are placed outside the measured points
            '~True - Draws dimension lines between the measured points even when arrowheads are placed outside the measured points
            Get
                Return PropValueB(dxxDimStyleProperties.DIMTOFL)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTOFL, value)
            End Set
        End Property
        Public Property ForceTextBetweenExtLine As Boolean
            '^forces text between extension lines.
            '~False - Varies with the type of dimension. For linear and angular dimensions, text is placed inside the extension lines if there is sufficient room. For radius and diameter dimensions that don¸t fit inside the circle or arc, DIMTIX has no effect and always forces the text outside the circle or arc.
            '~True - Draws dimension text between the extension lines even if it would ordinarily be placed outside those lines
            Get
                Return PropValueB(dxxDimStyleProperties.DIMTIX)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTIX, value)
            End Set
        End Property
        Public Property FractionStackType As dxxDimFractionStyles
            '^controls how fractions are displayed
            Get
                Return PropValueI(dxxDimStyleProperties.DIMFRAC)
            End Get
            Set(value As dxxDimFractionStyles)
                If Not dxfEnums.Validate(GetType(dxxDimFractionStyles), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMFRAC, value)
            End Set
        End Property
        Friend ReadOnly Property InputStructure As TDIMINPUT
            Get
                Return New TDIMINPUT With {
             .CenterMarkSize = CenterMarkSize,
             .DimLineColor = DimLineColor,
             .DimStyleName = Name,
             .ExtLineColor = ExtLineColor,
             .Linetype = dxfLinetypes.ByLayer,
             .Color = dxxColors.ByLayer,
             .Prefix = Prefix,
             .Suffix = Suffix,
             .TextColor = TextColor,
             .TextStyleName = TextStyleName}
            End Get
        End Property


        Public Property LinearPrecision As Integer
            '^the number of decimal places for formating linear dimensions
            '~0 - 8
            Get
                Return PropValueI(dxxDimStyleProperties.DIMDEC)
            End Get
            Set(value As Integer)
                PropValueSet(dxxDimStyleProperties.DIMDEC, value)
            End Set
        End Property
        Public Property LinearScaleFactor As Double
            '^the scale factor applied to all linear dimensions
            Get
                Return PropValueD(dxxDimStyleProperties.DIMLFAC)
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                PropValueSet(dxxDimStyleProperties.DIMLFAC, value)
            End Set
        End Property
        Friend Property LineSuppressionFlags As TDIMLINESPRESSION
            Get
                Return New TDIMLINESPRESSION(SuppressDimLine1, SuppressDimLine2, SuppressExtLine1, SuppressExtLine2)
            End Get
            Set(value As TDIMLINESPRESSION)

                SuppressDimLine1 = TVALUES.ToBoolean(value.SuppressDimLine1)
                SuppressDimLine2 = TVALUES.ToBoolean(value.SuppressDimLine2)
                SuppressExtLine1 = TVALUES.ToBoolean(value.SuppressExtLine1)
                SuppressExtLine2 = TVALUES.ToBoolean(value.SuppressExtLine2)
            End Set
        End Property
        Public Property Prefix As String
            '^the prefix added to all dimensions created under this style
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMPREFIX)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMPREFIX, value)
            End Set
        End Property
        Public Property RoundTo As Double
            '^how dimensions units are rounded
            Get
                Return PropValueD(dxxDimStyleProperties.DIMRND)
            End Get
            Set(value As Double)
                If value < 0 Then Return
                PropValueSet(dxxDimStyleProperties.DIMRND, value)
            End Set
        End Property
        Public Property Suffix As String
            '^the suffix added to all dimensions created under this style
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMSUFFIX)
            End Get
            Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMSUFFIX, value)
            End Set
        End Property
        Public Property SuppressDimLine1 As Boolean
            '^Suppresses display of the first dimension line.
            Get
                Return PropValueB(dxxDimStyleProperties.DIMSD1)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSD1, value)
            End Set
        End Property
        Public Property SuppressDimLine2 As Boolean
            '^Suppresses display of the second dimension line.
            Get
                Return PropValueB(dxxDimStyleProperties.DIMSD2)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSD2, value)
            End Set
        End Property
        Public Property SuppressExteriorDimLines As Boolean
            '^Suppresses drawing of dimension lines outside the extension lines.
            '~False - dimension Line Is Not Suppressed.
            '~On - dimension Line Is Suppressed.
            '~If the dimension lines would be outside the extension lines and DIMTIX is on, setting DIMSOXD to On suppresses the dimension line. If DIMTIX is off, DIMSOXD has no effect.
            Get
                Return PropValueB(dxxDimStyleProperties.DIMSOXD)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSOXD, value)
            End Set
        End Property
        Public Property SuppressExtLine1 As Boolean
            '^Suppresses display of the first extension line.
            Get
                Return PropValueB(dxxDimStyleProperties.DIMSE1)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSE1, value)
            End Set
        End Property
        Public Property SuppressExtLine2 As Boolean
            '^Suppresses display of the second extension line.
            Get
                Return PropValueB(dxxDimStyleProperties.DIMSE2)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMSE2, value)
            End Set
        End Property
        Public Property TextColor As dxxColors
            '^the color of the text drawn under this style
            Get
                Return PropValueI(dxxDimStyleProperties.DIMCLRT)
            End Get
            Set(value As dxxColors)

                PropValueSet(dxxDimStyleProperties.DIMCLRT, value)
            End Set
        End Property
        Public Property TextFit As dxxDimTextFitTypes
            '^Determines how dimension text and arrows are arranged when space is not sufficient to place both within the extension lines.
            '~0 Places both text and arrows outside extension lines
            '~1 Moves arrows first, then text
            '~2 Moves text first, then arrows
            '~3 Moves either text or arrows, whichever fits best
            Get
                Return PropValueI(dxxDimStyleProperties.DIMATFIT)
            End Get
            Set(value As dxxDimTextFitTypes)
                If Not dxfEnums.Validate(GetType(dxxDimTextFitTypes), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMATFIT, value)
            End Set
        End Property
        Public Property TextGap As Double
            '^Sets the distance around the dimension text when the dimension line
            '^breaks to accommodate dimension text. Also sets the gap between
            '^annotation and a hook line created with leaders.
            '^If you enter a negative value, a box is places around the dimension text.
            '^the size of the arrowheads drawn under this style
            Get
                Return PropValueD(dxxDimStyleProperties.DIMGAP)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMGAP, value)
            End Set
        End Property

        '^Controls if text boxes are drawn around the dimension text.  If true the TextGap is set to it's negative value other it is set to it's absolute value
        Public Property ShowTextBoxes As Boolean
            Get
                Return TextGap < 0
            End Get
            Set(value As Boolean)
                If value Then
                    TextGap = -1 * Math.Abs(TextGap)
                Else
                    TextGap = Math.Abs(TextGap)
                End If
            End Set
        End Property

        Public Property TextAngle As Double
            '^the angle of text draw under this style
            Get
                Return PropValueD(dxxDimStyleProperties.DIMTANGLE)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMTANGLE, value)
            End Set
        End Property
        Public Property TextHeight As Double
            '^the size of text draw under this style
            Get
                Return PropValueD(dxxDimStyleProperties.DIMTXT)
            End Get
            Set(value As Double)
                PropValueSet(dxxDimStyleProperties.DIMTXT, value)
            End Set
        End Property
        Public Property TextInsideHorizontal As Boolean
            '^Controls the position of dimension text inside the extension lines for all dimension types except Ordinate.
            Get
                Return PropValueB(dxxDimStyleProperties.DIMTIH)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTIH, value)
            End Set
        End Property
        Public ReadOnly Property TextMovement As dxxDimTextMovementTypes
            '^Suppresses drawing of dimension lines outside the extension lines.
            '~False - dimension Line Is Not Suppressed.
            '~On - dimension Line Is Suppressed.
            '~If the dimension lines would be outside the extension lines and DIMTIX is on, setting DIMSOXD to On suppresses the dimension line. If DIMTIX is off, DIMSOXD has no effect.
            Get
                Return PropValueI(dxxDimStyleProperties.DIMTMOVE)
            End Get
        End Property
        Public Property TextOutsideHorizontal As Boolean
            '^Controls the position of dimension text outside the extension lines.
            Get
                Return PropValueB(dxxDimStyleProperties.DIMTOH)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxDimStyleProperties.DIMTOH, value)
            End Set
        End Property
        Public Property TextPositionH As dxxDimJustSettings
            '^Controls the horizontal positioning of dimension text.
            '~Centered = 0   'Positions the text above the dimension line and center-justifies it between the extension lines
            '~Ext1 = 1 'Positions the text next to the first extension line
            '~Ext2 = 2 'Positions the text next to the second extension line
            '~AlignExt1 = 3 'Positions the text above and aligned with the first extension line
            '~AlignExt2 = 4 'Positions the text above and aligned with the second extension line
            Get
                Return PropValueI(dxxDimStyleProperties.DIMJUST)
            End Get
            Set(value As dxxDimJustSettings)
                If Not dxfEnums.Validate(GetType(dxxDimJustSettings), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMJUST, value)
            End Set
        End Property
        Public Property TextPositionV As dxxDimTadSettings
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
                Return PropValueI(dxxDimStyleProperties.DIMTAD)
            End Get
            Set(value As dxxDimTadSettings)
                If Not dxfEnums.Validate(GetType(dxxDimTadSettings), value) Then Return
                PropValueSet(dxxDimStyleProperties.DIMTAD, value)
            End Set
        End Property

        Public Property TextStyleHandle As String
            '^the handle of the referenced text style
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMTXSTY)
            End Get
            Friend Set(value As String)
                PropValueSet(dxxDimStyleProperties.DIMTXSTY, value)
            End Set
        End Property
        Public Property TextStyleName As String
            '^the name of the referenced text style
            Get
                Return PropValueStr(dxxDimStyleProperties.DIMTXSTY_NAME)
            End Get
            Set(value As String)
                '^the name of the referenced text style
                value = Trim(value)
                If value = String.Empty Then value = "Standard"
                If value <> "" Then If PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, value) Then TextStyleHandle = ""
            End Set
        End Property

        Public Property ZeroSuppressionAngular As dxxZeroSuppression
            '^controls zero suppression for angular dimension text
            Get
                Return PropValueI(dxxDimStyleProperties.DIMAZIN)
            End Get
            Set(value As dxxZeroSuppression)
                If Not dxfEnums.Validate(GetType(dxxZeroSuppression), TVALUES.To_INT(value)) Then Return
                PropValueSet(dxxDimStyleProperties.DIMAZIN, value)
            End Set
        End Property
        Public Property ZeroSuppressionArchitectural As dxxZeroSuppressionsArchitectural
            '^controls zero suppression for linear dimension text when architectural formating is on
            Get
                Return PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH)
            End Get
            Set(value As dxxZeroSuppressionsArchitectural)
                If Not dxfEnums.Validate(GetType(dxxZeroSuppressionsArchitectural), TVALUES.To_INT(value)) Then Return
                If PropValueSet(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH, value) Then
                    PropValueSet(dxxDimStyleProperties.DIMZIN, value + ZeroSuppression())
                End If
            End Set
        End Property
        Friend Overrides Property Properties As dxoProperties
            Get
                Return MyBase.Properties
            End Get
            Set(value As dxoProperties)
                MyBase.Properties = value
                PropValueSet(dxxDimStyleProperties.DIMPOST, dxfUtils.DimPrefixSuffix(PropValueStr(dxxDimStyleProperties.DIMPREFIX), PropValueStr(dxxDimStyleProperties.DIMSUFFIX)))
                PropValueSet(dxxDimStyleProperties.DIMAPOST, dxfUtils.DimPrefixSuffix(PropValueStr(dxxDimStyleProperties.DIMAPREFIX), PropValueStr(dxxDimStyleProperties.DIMASUFFIX)))

            End Set
        End Property
#End Region 'Properties


#Region "Methods"

        Public Function FormatAngle(Num As Object, Optional aAngUnits As dxxAngularUnits = dxxAngularUnits.Undefined) As String
            Return dxoDimStyle.DimStyleFormatAngle(Properties, Num, aAngUnits)
        End Function

        ''' <summary>
        ''' applies the dim styles linear format to the passed number
        ''' </summary>
        ''' <param name="Num">the number to format</param>
        ''' <param name="bApplyLinearMultiplier">a flag to suppress the application of the linear multiplier DIMLFAC of the dimstyles properties</param>
        ''' <param name="aType">an override linear format type </param>
        ''' <param name="aPrecision">an override linear precision to apply</param>
        ''' <param name="aSuffix">a suffix to append to the end of the return string</param>
        ''' <param name="aMultiplier">an additional multipler to apply to the passed number</param>
        Public Function FormatNumber(Num As Object, Optional bApplyLinearMultiplier As Boolean = True, Optional aType As dxxLinearUnitFormats = dxxLinearUnitFormats.Undefined, Optional aPrecision As Integer = -1, Optional aSuffix As String = Nothing, Optional aMultiplier As Double? = Nothing) As String
            Return dxoDimStyle.FormatDimensionalNumber(Properties, Num, bApplyLinearMultiplier, aType, aPrecision, aSuffix, aMultiplier)
        End Function

        ''' <summary>
        ''' applies the dim styles linear format to the passed numbers
        ''' </summary>
        ''' <param name="aNumbers">the numbers to format</param>
        ''' <param name="bApplyLinearMultiplier">a flag to suppress the application of the linear multiplier DIMLFAC of the dimstyles properties</param>
        ''' <param name="aType">an override linear format type </param>
        ''' <param name="aPrecision">an override linear precision to apply</param>
        ''' <param name="aSuffix">a suffix to append to the end of the return string</param>
        ''' <param name="aMultiplier">an additional multipler to apply to the passed number</param>
        ''' <returns></returns>
        Public Function FormatNumbers(aNumbers As IEnumerable(Of Object), Optional bApplyLinearMultiplier As Boolean = True, Optional aType As dxxLinearUnitFormats = dxxLinearUnitFormats.Undefined, Optional aPrecision As Integer = -1, Optional aSuffix As String = Nothing, Optional aMultiplier As Double? = Nothing) As List(Of String)
            Return dxoDimStyle.FormatDimensionalNumbers(Properties, aNumbers, bApplyLinearMultiplier, aType, aPrecision, aSuffix, aMultiplier)
        End Function


        Public Function ZeroSuppression(Optional bTolerance As Boolean = False) As dxxZeroSuppression
            '^controls zero suppression for linear dimension text

            If Not bTolerance Then
                Return PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION)
            Else
                Return PropValueI(dxxDimStyleProperties.DIMTOLZEROSUPPRESSION)
            End If


        End Function
        Public Sub SetZeroSuppression(value As dxxZeroSuppression, Optional bTolerance As Boolean = False)
            If Not dxfEnums.Validate(GetType(dxxZeroSuppression), TVALUES.To_INT(value)) Then Return
            If Not bTolerance Then
                If PropValueSet(dxxDimStyleProperties.DIMZEROSUPPRESSION, value) Then
                    PropValueSet(dxxDimStyleProperties.DIMZIN, value + ZeroSuppressionArchitectural)
                End If
            Else
            End If
        End Sub

        Friend Sub SetTextStyle(Value As dxoStyle)
            '^client access to set the text style

            If Value IsNot Nothing Then
                PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, Value.Name)
                PropValueSet(dxxDimStyleProperties.DIMTXSTY, Value.Handle)
            Else
                PropValueSet(dxxDimStyleProperties.DIMTXSTY_NAME, "Standard")
                PropValueSet(dxxDimStyleProperties.DIMTXSTY, "")
            End If
        End Sub
        Public Property LinearFormatType As dxxLinearUnitFormats
            '^controls how dimension text is displayed for linear dimensions
            Get
                Return PropValueI(dxxDimStyleProperties.DIMLUNIT)

            End Get
            Set(value As dxxLinearUnitFormats)
                PropValueSet(dxxDimStyleProperties.DIMLUNIT, value)
            End Set
        End Property

        Public Property LinearFormatTypeAlt As dxxLinearUnitFormats
            '^controls how dimension text is displayed for linear dimensions
            Get

                Return PropValueI(dxxDimStyleProperties.DIMALTU)


            End Get
            Set(value As dxxLinearUnitFormats)
                PropValueSet(dxxDimStyleProperties.DIMALTU, value)
            End Set
        End Property
        Public Sub SetArrowHeads(Optional aArrow1 As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrow2 As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aLeaderArrow As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle)
            If aArrow1 > 0 And aArrow1 < dxxArrowHeadTypes.UserDefined Then ArrowHead1 = aArrow1
            If aArrow2 > 0 And aArrow2 < dxxArrowHeadTypes.UserDefined Then ArrowHead2 = aArrow2
            If aLeaderArrow > 0 And aLeaderArrow < dxxArrowHeadTypes.UserDefined Then ArrowHeadLeader = aLeaderArrow
        End Sub
        Public Sub ReName(aName As String, Optional bRetainCurrentSettings As Boolean = False)
            If String.IsNullOrWhiteSpace(aName) Then Return Else aName = aName.Trim()

            Dim myprops As dxoProperties = New dxoProperties(Properties)
            If (String.Compare(aName, Name, ignoreCase:=True) = 0) Then Return
            Dim oldProps As dxoProperties = New dxoProperties(Properties)
            PropValueSet(dxxLinetypeProperties.Name, aName, False)


        End Sub
        Public Function Activate() As Boolean
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return False
            Dim myName As String = Name
            Dim aParent As dxoDimStyle = aImage.TableEntry(dxxReferenceTypes.DIMSTYLE, myName)
            If aParent.Index < 0 Then Return False
            aImage.Header.DimStyleName = myName
            Return True
        End Function
        Friend Sub CopyDimStyleProps(aDimstyleOrHeader As TTABLEENTRY, bHeaderPassed As Boolean, Optional aSkipList As String = Nothing)
            dxfTableEntry.CopyDimStyleProperties(Properties, aDimstyleOrHeader.Props, False, bHeaderPassed, aSkipList)

        End Sub
        Public Sub ClearLineSuppressions()
            PropValueSet(dxxDimStyleProperties.DIMSD1, False)
            PropValueSet(dxxDimStyleProperties.DIMSD2, False)
            PropValueSet(dxxDimStyleProperties.DIMSE1, False)
            PropValueSet(dxxDimStyleProperties.DIMSE2, False)
        End Sub
        Public Function CreateDimensionText(aValue As Object, aImage As dxfImage, Optional aInsertPt As Object = Nothing, Optional bSuppressLinearScaleFactor As Boolean = False, Optional bAngleValue As Boolean = False, Optional aPrefix As Object = Nothing, Optional aSuffix As Object = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aLinearType As dxxLinearUnitFormats = dxxLinearUnitFormats.Undefined, Optional aAngUnits As dxxAngularUnits = dxxAngularUnits.Undefined) As dxeText
            Dim _rVal As dxeText = Nothing
            '^used to create the text for the passed value
            '~creates and returns a dxeText object
            If aValue Is Nothing Then Return _rVal
            Dim tht As Double
            Dim bImage As dxfImage
            Dim styname As String
            Dim tClr As dxxColors
            Dim aDimText As String
            Dim prfx As String
            Dim sffx As String
            bImage = aImage
            If Not GetImage(bImage) Then bImage = New_Image()
            tClr = TextColor
            prfx = Prefix
            sffx = Suffix
            'compute the number being displayed
            If TVALUES.IsNumber(aValue) Then
                If Not bAngleValue Then
                    aDimText = FormatNumber(aValue, Not bSuppressLinearScaleFactor, aLinearType)
                Else
                    aDimText = FormatAngle(aValue, aAngUnits)
                End If
            Else
                aDimText = Trim(aValue)
            End If
            If aPrefix IsNot Nothing Then
                prfx = aPrefix.ToString()
            End If
            If aSuffix IsNot Nothing Then
                sffx = aSuffix.ToString()
            End If
            aDimText = prfx & aDimText & sffx
            'style
            styname = bImage.GetOrAdd(dxxReferenceTypes.STYLE, TextStyleName)
            'text height
            tht = TextHeight * FeatureScaleFactor
            _rVal = bImage.CreateText(bImage.TableEntry(dxxReferenceTypes.STYLE, styname), "{\A1;" + aDimText + "}", dxxTextTypes.Multiline, dxxTextDrawingDirections.Horizontal, tht, dxxMTextAlignments.MiddleCenter, aPlane:=aPlane, aInsertPt:=aInsertPt)
            _rVal.DisplayStructure = dxfImageTool.DisplayStructure_Text(bImage, aColor:=tClr, aTextStyleName:=styname)
            Return _rVal
        End Function


        Friend Sub GetArrowHeadNames(bIncludeUnderScores As Boolean, ByRef sDimBlk1 As String, ByRef sDimBlk2 As String, ByRef sDimLdrBlk As String, bNoClosed As Boolean)
            sDimLdrBlk = PropValueStr(dxxDimStyleProperties.DIMLDRBLK_NAME)
            sDimBlk1 = PropValueStr(dxxDimStyleProperties.DIMBLK1_NAME)
            sDimBlk2 = PropValueStr(dxxDimStyleProperties.DIMBLK2_NAME)
            If bNoClosed Then
                If String.Compare(sDimLdrBlk, "_ClosedFilled", True) = 0 Then sDimLdrBlk = ""
                If String.Compare(sDimBlk1, "_ClosedFilled", True) = 0 Then sDimBlk1 = ""
                If String.Compare(sDimBlk2, "_ClosedFilled", True) = 0 Then sDimBlk2 = ""
            End If
            If Not bIncludeUnderScores Then
                If Left(sDimLdrBlk, 1) = "_" Then sDimLdrBlk = Right(sDimLdrBlk, Len(sDimLdrBlk) - 1)
                If Left(sDimBlk1, 1) = "_" Then sDimBlk1 = Right(sDimBlk1, Len(sDimBlk1) - 1)
                If Left(sDimBlk2, 1) = "_" Then sDimBlk2 = Right(sDimBlk2, Len(sDimBlk2) - 1)
            End If
        End Sub

        Public Function CopyStyleProperties(aDimStyle As dxoDimStyle, Optional aPropertyEnums As List(Of dxxDimStyleProperties) = Nothing) As List(Of String)
            Dim _rVal As New List(Of String)
            If aDimStyle Is Nothing Then Return _rVal
            Dim passedprops As dxoProperties = aDimStyle.Properties
            Dim ivalues As List(Of Integer) = dxfEnums.EnumValueList(GetType(dxxDimStyleProperties))
            Dim value As Object
            Dim pname As String
            Dim doit As Boolean
            For Each ival As dxxDimStyleProperties In ivalues
                If ival <> dxxDimStyleProperties.DIMNAME Then
                    pname = ival.ToString()
                    If pname.StartsWith("DIM") Then
                        value = passedprops.Value(pname)
                        doit = True
                        If aPropertyEnums IsNot Nothing Then
                            doit = aPropertyEnums.Contains(ival)
                        End If
                        If doit Then
                            If PropValueSet(ival, value) Then
                                _rVal.Add(pname)
                            End If
                        End If
                    End If
                End If
            Next
            Return _rVal
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
            Return dxoProperties.Compare(Properties, Style.Properties, GCs)
        End Function
        Friend Function ReferencesBlock(aBlockName As String, aRecordHandle As String) As Boolean
            aBlockName = Trim(aBlockName)
            aRecordHandle = Trim(aRecordHandle)
            If String.Compare(ArrowHeadBlock, aBlockName, True) = 0 Then
                Return True
            ElseIf String.Compare(ArrowHeadBlock1, aBlockName, True) = 0 Then
                Return True
            ElseIf String.Compare(ArrowHeadBlock2, aBlockName, True) = 0 Then
                Return True
            ElseIf String.Compare(ArrowHeadBlockLeader, aBlockName, True) = 0 Then
                Return True
            ElseIf String.Compare(ArrowHeadBlockHandle, aRecordHandle, True) = 0 Then
                Return True
            ElseIf String.Compare(ArrowHeadBlock1Handle, aRecordHandle, True) = 0 Then
                Return True
            ElseIf String.Compare(ArrowHeadBlock2Handle, aRecordHandle, True) = 0 Then
                Return True
            ElseIf String.Compare(ArrowHeadBlockLeaderHandle, aRecordHandle, True) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function
        Public Sub RefreshProperties()
            '^used to reset the properties collection to the initial state
            Properties = New dxoProperties(dxpProperties.GetReferenceProps(dxxReferenceTypes.DIMSTYLE, Name, aString:=TextStyleName))
        End Sub
        Public Sub Reset()
            ResetPropsToParent(Nothing, True)
        End Sub
        Friend Sub ResetPropsToParent(aImage As dxfImage, Optional aByNameOnly As Boolean = False)
            If Not GetImage(aImage) Then Return
            Dim aNm As String

            Dim aTbl As dxoDimStyles = aImage.DimStyles
            Dim aParent As dxoDimStyle = Nothing

            aNm = Name
            If Not aTbl.MemberExists(aNm) And Not aByNameOnly Then aNm = aImage.Header.DimStyleName
            If Not aTbl.MemberExists(aNm) And Not aByNameOnly Then aNm = "Standard"
            'aParent = New dxoDimStyle(aTbl.Entry(aNm))
            If aTbl.TryGet(aNm, aParent) Then

                Properties.CopyVals(aParent.Properties, bSkipHandles:=False, bSkipPointers:=False)
                aImage.BaseSettings_Set(New TTABLEENTRY(Me))
            End If
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
        Public Sub SetColors(Optional aLineColor As dxxColors? = Nothing, Optional aTextColor As dxxColors? = Nothing)
            '^set the extension line and dimension line colors in a single call
            If aLineColor.HasValue Then
                If aLineColor.Value <> dxxColors.Undefined Then
                    ExtLineColor = aLineColor.Value
                    DimLineColor = aLineColor.Value
                End If
            End If
            If aTextColor.HasValue Then
                If aTextColor.Value <> dxxColors.Undefined Then TextColor = aTextColor.Value
            End If
        End Sub
        Public Sub SetGapsAndOffsets(aGap As Double, Optional aArrowSize As Double? = Nothing, Optional aTextGap As Double? = Nothing)
            '^set the extension line offset, extension line extension and text gap in one call
            '~a negative value cause text blocks around text
            If aTextGap.HasValue Then TextGap = aTextGap.Value
            ExtLineExtend = Math.Abs(aGap)
            ExtLineOffset = Math.Abs(aGap)
            If aArrowSize.HasValue Then ArrowSize = aArrowSize.Value
        End Sub
        Public Sub SetLinearFormat(Optional bNoLead As Boolean = False, Optional bNoTrail As Boolean = False)
            '^the format string applied to linear numbers
            If Not bNoLead And Not bNoTrail Then
                SetZeroSuppression(dxxZeroSuppression.None)
            Else
                If bNoLead And bNoTrail Then
                    SetZeroSuppression(dxxZeroSuppression.LeadingAndTrailing)
                Else
                    If bNoLead Then
                        SetZeroSuppression(dxxZeroSuppression.Leading)
                    End If
                    If bNoTrail Then
                        SetZeroSuppression(dxxZeroSuppression.Trailing)
                    End If
                End If
            End If
        End Sub
        ''' <summary>
        ''' searches for a style property by property type enum and sets it value if it is found
        ''' </summary>
        ''' <remarks></remarks>
        ''' <param name="aProperty">the property type enum to search for</param>
        ''' <param name="aValue">the Value to set the property value to</param>
        ''' <returns></returns>
        Public Function SetProperty(aProperty As dxxDimStyleProperties, aValue As Object) As Boolean

            Return PropValueSet(aProperty, aValue)
        End Function
        Public Sub SetSuppressionFlags(Optional aSupDimLn1 As Boolean? = Nothing, Optional aSupDimLn2 As Boolean? = Nothing, Optional aSupExtLn1 As Boolean? = Nothing, Optional aSupExtLn2 As Boolean? = Nothing)
            If aSupDimLn1.HasValue Then SuppressDimLine1 = aSupDimLn1.Value
            If aSupDimLn2.HasValue Then SuppressDimLine2 = aSupDimLn2.Value
            If aSupExtLn1.HasValue Then SuppressExtLine1 = aSupExtLn1.Value
            If aSupExtLn2.HasValue Then SuppressExtLine2 = aSupExtLn2.Value
        End Sub
        Friend Function SetValue(aIndex As dxxDimStyleProperties, aValue As Object, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            Try
                _rVal = PropValueSet(aIndex, aValue, 0, bSuppressEvnts:=bSuppressEvnts)
            Catch ex As Exception
                If Not bSuppressEvnts Then
                    Dim aImage As dxfImage = Nothing
                    If GetImage(aImage) Then
                        aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType(), ex)
                    End If
                Else
                    Throw ex
                End If
            End Try
            Return _rVal
        End Function
        Public Function SetZeroSuppressionDecimal(bNoLead As Boolean, bNoTrail As Boolean, Optional aLinearPrecision As Integer = -1, Optional aAngularPrecision As Integer = -1) As dxxZeroSuppression
            Dim _rVal As dxxZeroSuppression
            _rVal = dxxZeroSuppression.None
            If bNoLead Then _rVal += dxxZeroSuppression.Leading
            If bNoTrail Then _rVal += dxxZeroSuppression.Trailing
            SetZeroSuppression(_rVal)
            Dim aZn As dxxZeroSuppression
            aZn = ZeroSuppression()

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
        Public Function UpdateToImage(Optional aStyleName As String = "", Optional aImage As dxfImage = Nothing) As Boolean
            Dim _rVal As Boolean

            '#1the parent style to copy the properties from
            '^If the Style is a copy of a style in the parent images style table
            '^the current properties of the parent style are copied to this style.
            '~if an alternate name is passed the properties of the passed style are copied.
            If Not GetImage(aImage) Then Return _rVal
            Dim myName As String = Name
            If String.IsNullOrWhiteSpace(aStyleName) Then aStyleName = myName Else aStyleName = aStyleName.Trim()
            Dim aStyles As dxoStyles = aImage.Styles
            Dim aDStyle As dxoDimStyle = Nothing
            If Not aImage.DimStyles.TryGet(aStyleName, aDStyle) Then
                aDStyle = aImage.DimStyle()
            End If
            Dim idx As Integer = aImage.DimStyles.IndexOf(aDStyle)
            '  Properties.SetVal(2, aDStyle.Name)
            Dim chngdProps As New dxoProperties(Properties.Name) With {.IsDirty = Properties.IsDirty, .FileObjectType = Properties.FileObjectType}

            'update the references
            aDStyle.UpdateProperties(aImage)

            Dim dprops As dxoProperties = aDStyle.Properties
            Dim mykeys As List(Of String) = Properties.Keys(bIncludeHiddenProps:=True, bExcludeDoNotCopy:=True)
            For Each key As String In mykeys
                Dim myprop As dxoProperty = Properties.Find(Function(x) String.Compare(x.Key, key, True) = 0)
                Dim dprop As dxoProperty = dprops.Find(Function(x) String.Compare(x.Key, key, True) = 0)
                If dprop Is Nothing Then Continue For
                If myprop.CopyValue(dprop) Then
                    chngdProps.Add(New dxoProperty(myprop))
                End If
            Next

            If chngdProps.Count > 0 Then
                _rVal = True
                IsDirty = True
            End If

            If _rVal Then
                If chngdProps.Count > 0 And IsCopied Then
                    For i = 1 To chngdProps.Count
                        Dim aProp As dxoProperty = chngdProps.Item(i)
                        RaisePropertyChangeEvent(aProp)
                    Next
                End If
                If Not IsCopied Then
                    If IsGlobal Then

                    Else
                        If idx > 0 Then
                            'this is an table member
                            Dim rReject As Boolean = False

                            dxoDimStyle.UpdateReferences(Me, aImage)
                            Dim undo As Boolean = False
                            For i = 1 To chngdProps.Count
                                Dim aProp As dxoProperty = chngdProps.Item(i)
                                Notify(aProp, aProp.LastValue, aImage, undo)
                                If Not undo Then IsDirty = True
                            Next

                        End If
                    End If
                Else
                    aImage.IsDirty = True
                End If
            End If

            Return _rVal
        End Function

        Public Function SettingProperties() As List(Of dxoProperty)
            Dim _rVal As List(Of dxoProperty) = Properties.FindAll(Function(x) x.Name.ToUpper().StartsWith("DIM"))
            _rVal.RemoveAll(Function(x) x.GroupCode = 0)
            Return _rVal
        End Function




        Public Sub UpdateDimPost()
            PropValueSet(dxxDimStyleProperties.DIMPOST, dxfUtils.DimPrefixSuffix(PropValueStr(dxxDimStyleProperties.DIMPREFIX), PropValueStr(dxxDimStyleProperties.DIMSUFFIX)), bSuppressEvnts:=True)
            PropValueSet(dxxDimStyleProperties.DIMAPOST, dxfUtils.DimPrefixSuffix(PropValueStr(dxxDimStyleProperties.DIMAPREFIX), PropValueStr(dxxDimStyleProperties.DIMASUFFIX)), bSuppressEvnts:=True)
        End Sub

        Friend Overrides Function PropValueSet(aIndex As [Enum], aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean

            If aValue Is Nothing Then Return False
            Return PropValueSetByName(dxfEnums.PropertyName(aIndex), aValue, aOccur, bSuppressEvnts)

        End Function
        Friend Overrides Function PropValueSetByName(aName As String, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Or String.IsNullOrWhiteSpace(aName) Then Return False
            'reviewv changes for validity before proceding
            Select Case aName.ToUpper()
                Case "DIMCLRT", "DIMCLRE", "DIMCLRD"
                    Dim ival As Integer = TVALUES.To_INT(aValue)
                    If ival < 0 Then
                        Return False
                    End If
            End Select
            Return MyBase.PropValueSetByName(aName, aValue, aOccur, bSuppressEvnts)

        End Function


        Public Shadows Function Clone() As dxsDimOverrides
            Return New dxsDimOverrides(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#End Region 'Methods

    End Class

End Namespace