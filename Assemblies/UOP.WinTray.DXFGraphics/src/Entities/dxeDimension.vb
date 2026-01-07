Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxeDimension
        Inherits dxfEntity
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Friend Sub New()
            MyBase.New(dxxGraphicTypes.Dimension)
            DimType = dxxDimTypes.LinearHorizontal
        End Sub

        Public Sub New(aEntity As dxeDimension, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Dimension, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
            If aEntity IsNot Nothing Then DimType = aEntity.DimType
        End Sub

        Public Sub New(aStyle As dxoDimStyle, aDimType As dxxDimTypes, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxGraphicTypes.Dimension)
            If Not dxfEnums.Validate(GetType(dxxDimTypes), aDimType) Then aDimType = dxxDimTypes.LinearHorizontal
            DimType = aDimType
            If aStyle IsNot Nothing Then
                DimStyleStructure = New TTABLEENTRY(aStyle)
                TextRotation = aStyle.TextAngle
            End If

            DisplayStructure = New TDISPLAYVARS(aDisplaySettings)
            If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
        End Sub


        Public Sub New(aDimType As dxxDimTypes, aImage As dxfImage, Optional aStyleName As String = Nothing, Optional bSuppressCurrentOverrides As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxGraphicTypes.Dimension)
            If Not dxfEnums.Validate(GetType(dxxDimTypes), aDimType) Then aDimType = dxxDimTypes.LinearHorizontal
            DimType = aDimType
            Dim aStyle As dxoDimStyle
            If aImage Is Nothing Then
                If Not String.IsNullOrEmpty(aStyleName) Then DimStyleName = aStyleName.Trim
                aStyle = New dxoDimStyle(DimStyleName) With {.IsCopied = True}
                If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane)
            Else
                If Not dxfPlane.IsNull(aPlane) Then PlaneV = New TPLANE(aPlane) Else PlaneV = aImage.UCS.Strukture
                Dim curSty As String = aImage.Header.DimStyleName
                If Not String.IsNullOrEmpty(aStyleName) Then
                    DimStyleName = aStyleName.Trim
                Else
                    DimStyleName = curSty
                End If
                ImageGUID = aImage.GUID
                Dim aEntry As dxoDimStyle = Nothing
                aImage.GetOrAdd(dxxReferenceTypes.DIMSTYLE, DimStyleName, rEntry:=aEntry)
                aStyle = New dxoDimStyle(aEntry) With {.IsCopied = True}
                If String.Compare(aStyle.Name, curSty, ignoreCase:=True) = 0 And Not bSuppressCurrentOverrides Then
                    aStyle.Properties = dxoDimStyle.CopyDimStyleProperties(aStyle.Properties, aImage.DimStyleOverrides.Properties, False, False)
                End If
                aStyle.ImageGUID = ImageGUID
                aStyle.OwnerGUID = GUID
                MyBase.DimStyle = aStyle
            End If
            If aDisplaySettings IsNot Nothing Then DisplaySettings = aDisplaySettings
        End Sub



        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            MyBase.New(dxxGraphicTypes.Dimension)
            DefineByObject(aObject, bNoHandles, aStyle, aBlock)
        End Sub

        Friend Sub New(aStyle As dxoDimStyle, aEntityType As dxxEntityTypes, aDisplaySettings As TDISPLAYVARS, aPlane As TPLANE)
            MyBase.New(dxxGraphicTypes.Dimension)
            DimType = aEntityType.DimensionType()
            If aStyle IsNot Nothing Then
                DimStyleStructure = New TTABLEENTRY(aStyle)
                TextRotation = aStyle.TextAngle
            End If

            DisplayStructure = aDisplaySettings
            PlaneV = aPlane
        End Sub

        Friend Sub New(aStyle As dxoDimStyle, aDimType As dxxDimTypes, aDisplaySettings As TDISPLAYVARS, aPlane As TPLANE)
            MyBase.New(dxxGraphicTypes.Dimension)
            If Not dxfEnums.Validate(GetType(dxxDimTypes), aDimType) Then aDimType = dxxDimTypes.LinearHorizontal
            DimType = aDimType
            If aStyle IsNot Nothing Then
                DimStyleStructure = New TTABLEENTRY(aStyle)
                TextRotation = aStyle.TextAngle
            End If

            DisplayStructure = aDisplaySettings
            PlaneV = aPlane
        End Sub


#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Aligned As Boolean
            Get
                Return (DimType = dxxDimTypes.LinearAligned)
            End Get
        End Property
        Public Property Angle As Double
            Get
                Return PropValueD(50) '"Angle of rotated, horizontal, or vertical dimensions"
            End Get
            Set(value As Double)
                value = TVALUES.NormAng(value, False, True, True)
                If value = 180 Or value = 360 Then value = 0
                If value = 270 Then value = 90
                SetPropVal(50, value, True) '"Angle of rotated, horizontal, or vertical dimensions"
            End Set
        End Property
        Public ReadOnly Property Angular As Boolean
            '^true if this is an angular dimension
            Get
                Return (EntityType > 1030 And EntityType < 1031)
            End Get
        End Property
        Public ReadOnly Property AngularDimensionType As dxxAngularDimTypes
            '^the dimension entities angular dimension dimension type
            Get
                Select Case DimType
                    Case dxxDimTypes.Angular
                        Return dxxAngularDimTypes.Angular
                    Case dxxDimTypes.Angular3P
                        Return dxxAngularDimTypes.Angular3P
                    Case Else
                        Return dxxAngularDimTypes.Undefined
                End Select
            End Get
        End Property
        Friend ReadOnly Property Arrow1Handle As String
            Get
                Return DimStyle.ArrowHeadBlock1Handle
            End Get
        End Property
        Public ReadOnly Property Arrow1Name As String
            Get
                Return DimStyle.ArrowHeadBlock1
            End Get
        End Property
        Friend ReadOnly Property Arrow2Handle As String
            Get
                Return DimStyle.ArrowHeadBlock2Handle
            End Get
        End Property
        Public ReadOnly Property Arrow2Name As String
            Get
                Return DimStyle.ArrowHeadBlock2
            End Get
        End Property
        Public Overrides ReadOnly Property EntityType As dxxEntityTypes
            Get
                Return DimType.EntityType()
            End Get
        End Property

        Public Property BlockName As String
            Get
                Return PropValueStr("Block Name")
            End Get
            Friend Set(value As String)
                If SetPropVal("Block Name", value, True) Then
                    BlockGUID = ""
                End If
            End Set
        End Property
        Public ReadOnly Property CenterMark1 As dxeLine
            Get
                Return GetSubEnt("Extension.CenterMark1")
            End Get
        End Property
        Public ReadOnly Property CenterMark2 As dxeLine
            Get
                Return GetSubEnt("Extension.CenterMark2")
            End Get
        End Property
        Friend ReadOnly Property DefinitionPoints As colDXFVectors
            Get
                Dim _rVal As New colDXFVectors(DefPt10, DefPt11, DefPt13, DefPt14) From {
                    DefPt15,
                    DefPt16
                }
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property DefPt10 As dxfVector
            '^the first dimension definition point
            '~for linear dimensions it is the point where the dimension lines intersect the second exension line.
            '~for radial dimensions it is the center of the dimensioned arc.
            '~for diametric dimensions it is one of the points on the dimensioned arc.
            '~for angular dimensions it is the endpt of the first dimensioned line.
            '~for angular 3 point dimensions it is the text placement point.
            '~for ordinate dimensions it is reference point of the dimension.
            Get
                Return DefPts.Vector1
            End Get
        End Property
        Friend Property DefPt10V As TVECTOR
            '^the first dimension definition point
            '~for linear dimensions it is the point where the dimension lines intersect the second exension line.
            '~for radial dimensions it is the center of the dimensioned arc.
            '~for diametric dimensions it is one of the points on the dimensioned arc.
            '~for angular dimensions it is the endpt of the first dimensioned line.
            '~for angular 3 point dimensions it is the text placement point.
            '~for ordinate dimensions it is reference point of the dimension.
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property
        Public ReadOnly Property DefPt11 As dxfVector
            '^the second dimension definition point
            '~it is the center point of the dimension text.
            Get
                Return DefPts.Vector2
            End Get
        End Property
        Friend Property DefPt11V As TVECTOR
            '^the second dimension definition point
            '~it is the center point of the dimension text.
            Get
                Return DefPts.VectorGet(2)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(2, value)
            End Set
        End Property
        Public ReadOnly Property DefPt12 As dxfVector
            '^the insertion pt for baseline and continue dimensions
            Get
                Return DefPts.Vector3
            End Get
        End Property
        Friend Property DefPt12V As TVECTOR
            '^the insertion pt for baseline and continue dimensions
            Get
                Return DefPts.VectorGet(3)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(3, value)
            End Set
        End Property
        Public ReadOnly Property DefPt13 As dxfVector
            '^the third dimension definition point
            '~for linear dimensions it is the first dimensioned point.
            '~for radial and diametric dimensions it has no meaning.
            '~for angular dimensions it is the startpt of the second dimensioned line.
            '~for angular 3 point dimensions it is the endpt of the second dimensioned line.
            '~for ordinate dimensions it is the dimensioned point.
            Get
                Return DefPts.Vector4
            End Get
        End Property
        Friend Property DefPt13V As TVECTOR
            '^the third dimension definition point
            '~for linear dimensions it is the first dimensioned point.
            '~for radial and diametric dimensions it has no meaning.
            '~for angular dimensions it is the startpt of the second dimensioned line.
            '~for angular 3 point dimensions it is the endpt of the second dimensioned line.
            '~for ordinate dimensions it is the dimensioned point.
            Get
                Return DefPts.VectorGet(4)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(4, value)
            End Set
        End Property
        Public ReadOnly Property DefPt14 As dxfVector
            '^the third dimension definition point
            '~for linear dimensions it is the first dimensioned point.
            '~for radial and diametric dimensions it has no meaning.
            '~for angular dimensions it is the startpt of the second dimensioned line.
            '~for angular 3 point dimensions it is the endpt of the second dimensioned line.
            '~for ordinate dimensions it is dimensioned point.
            Get
                Return DefPts.Vector5
            End Get
        End Property
        Friend Property DefPt14V As TVECTOR
            '^the forth dimension definition point
            '~for linear dimensions it is the second dimensioned point.
            '~for radial and diametric dimensions it has no meaning.
            '~for angular dimensions it is the endpt of the second dimensioned line.
            '~for angular 3 point dimensions it is the endpt of the first dimensioned line.
            '~for ordinate dimensions it is the end point of the dimension lines.
            Get
                Return DefPts.VectorGet(5)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(5, value)
            End Set
        End Property
        Public ReadOnly Property DefPt15 As dxfVector
            '^the fifth dimension definition point
            '~for linear dimensions it has no meaning.
            '~for radial and diametric dimensions it is a pt on the dimensioned arc.
            '~for angular dimensions it is the starpt of the first dimensioned line.
            '~for angular 3 point dimensions it is the intersction pt of the dimensioned lines.
            '~for ordinate dimensions it has no meaning.
            Get
                Return DefPts.Vector6
            End Get
        End Property
        Friend Property DefPt15V As TVECTOR
            '^the fifth dimension definition point
            '~for linear dimensions it has no meaning.
            '~for radial and diametric dimensions it is a pt on the dimensioned arc.
            '~for angular dimensions it is the starpt of the first dimensioned line.
            '~for angular 3 point dimensions it is the intersction pt of the dimensioned lines.
            '~for ordinate dimensions it has no meaning.
            Get
                Return DefPts.VectorGet(6)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(6, value)
            End Set
        End Property
        Public ReadOnly Property DefPt16 As dxfVector
            '^the sixth dimension definition point
            '~for linear dimensions it has no meaning.
            '~for radial and diametric dimensions it has no meaning.
            '~for angular dimensions it is the text placement point.
            '~for angular 3 point dimensions it has no meaning.
            '~for ordinate dimensions it has no meaning.
            Get
                Return DefPts.Vector7
            End Get
        End Property
        Friend Property DefPt16V As TVECTOR
            '^the sixth dimension definition point
            '~for linear dimensions it has no meaning.
            '~for radial and diametric dimensions it has no meaning.
            '~for angular dimensions it is the text placement point.
            '~for angular 3 point dimensions it has no meaning.
            '~for ordinate dimensions it has no meaning.
            Get
                Return DefPts.VectorGet(7)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(7, value)
            End Set
        End Property
        Public ReadOnly Property DimensionArc1 As dxeArc
            '^the first of the dimension arcs (for angular dimensions)
            Get
                Return GetSubEnt("Dimension.Arc1")
            End Get
        End Property
        Public ReadOnly Property DimensionArc2 As dxeArc
            '^the second of the dimension arcs (for angular dimensions)
            Get
                Return GetSubEnt("Dimension.Arc2")
            End Get
        End Property
        Public ReadOnly Property DimensionLine1 As dxeLine
            '^the first of the dimension lines
            Get
                Return GetSubEnt("Dimension.Line1")
            End Get
        End Property
        Public ReadOnly Property DimensionLine2 As dxeLine
            Get
                '^the second of the dimension lines
                Return GetSubEnt("Dimension.Line2")
            End Get
        End Property
        Public ReadOnly Property DimensionPt1 As dxfVector
            Get
                '^the first dimension point of the dimension
                '~this point varies for the different dimension types
                Select Case DimensionFamily
                    Case dxxDimensionTypes.Linear
                        Return DefPt13
                    Case dxxDimensionTypes.Ordinate
                        Return DefPt10
                    Case dxxDimensionTypes.Radial
                        Return DefPt10
                    Case dxxDimensionTypes.Angular
                        Return DefPt13
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property
        Public ReadOnly Property DimensionPt2 As dxfVector
            Get
                '^the second dimension point of the dimension
                '~this point varies for the different dimension types
                Select Case DimensionFamily
                    Case dxxDimensionTypes.Linear
                        Return DefPt14
                    Case dxxDimensionTypes.Ordinate
                        Return DefPt13
                    Case dxxDimensionTypes.Radial
                        Return DefPt15
                    Case dxxDimensionTypes.Angular
                        If AngularDimensionType = dxxAngularDimTypes.Angular3P Then
                            Return DefPt14
                        Else
                            Return DefPt15
                        End If
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property
        Friend ReadOnly Property DimensionPts As colDXFVectors
            Get
                Return DefinitionPoints
            End Get
        End Property

        ''' <summary>
        ''' returns the dimension type of the dimension entity
        ''' </summary>
        ''' <remarks   >this dimension type is more detailed than the parent dimension type</remarks>
        ''' <returns></returns>
        Public Property DimType As dxxDimTypes
            Get
                Return DirectCast(PropValueI("*DimType"), dxxDimTypes)
            End Get
            Friend Set(value As dxxDimTypes)
                If value = dxxDimTypes.Undefined Then Return
                SetPropVal("*DimType", value, True)
            End Set
        End Property
        ''' <summary>
        ''' returns the parent dimension type of this dimension
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DimensionFamily As dxxDimensionTypes
            Get
                Return DimType.DimensionFamily()
            End Get
        End Property

        Public ReadOnly Property FamilyName As String
            Get
                Return dxfEnums.Description(DimensionFamily)
            End Get
        End Property
        Public ReadOnly Property DimensionValue As Double
            Get
                '^returns then actual measurement of the dimension
                '~returns and angle for angular dimensions and a linear distance for any other
                Dim v1 As New TVECTOR(DimensionPt1)
                Dim v2 As New TVECTOR(DimensionPt2)
                Dim v3 As TVECTOR
                Dim aPl As TPLANE = PlaneV
                Dim l1 As New TLINE
                'compute the number being displayed
                Dim _rVal As Double = 0
                Select Case DimType
                    Case dxxDimTypes.LinearAligned, dxxDimTypes.LinearVertical, dxxDimTypes.LinearHorizontal

                        aPl.Origin = v1
                        Select Case DimType
                            Case dxxDimTypes.LinearHorizontal
                                v3 = v2.ProjectedTo(aPl.LineH(0, 10, bByStartPt:=True))
                                Return dxfProjections.DistanceTo(v1, v3)
                            Case dxxDimTypes.LinearVertical
                                l1 = aPl.LineV(0, 10, bByStartPt:=True)
                                v3 = v2.ProjectedTo(l1)
                                Return dxfProjections.DistanceTo(v1, v3)
                            Case dxxDimTypes.LinearAligned
                                Return dxfProjections.DistanceTo(v1, v2)
                        End Select
                    Case dxxDimTypes.OrdHorizontal, dxxDimTypes.OrdVertical
                        v1 = New TVECTOR(DimensionPt1)
                        v2 = New TVECTOR(DimensionPt2)
                        aPl.Origin = v1
                        Select Case DimType
                            Case dxxDimTypes.OrdVertical
                                l1 = aPl.LineV(0, 10, bByStartPt:=True)
                                v3 = v2.ProjectedTo(l1)
                                Return dxfProjections.DistanceTo(v1, v3)
                            Case dxxDimTypes.OrdHorizontal
                                l1 = aPl.LineH(0, 10, bByStartPt:=True)
                                v3 = v2
                                v3 = dxfProjections.ToLine(v2, l1)
                                Return dxfProjections.DistanceTo(v1, v3)
                        End Select
                    Case dxxDimTypes.Radial, dxxDimTypes.Diametric
                        v1 = DefPt10V
                        v2 = DefPt15V
                        Return dxfProjections.DistanceTo(v1, v2)
                    Case dxxDimTypes.Angular, dxxDimTypes.Angular3P
                        Return Angle
                End Select
                Return _rVal
            End Get
        End Property

        ''' <summary>
        ''' the point where the dimensions first dimension line intesects the first extension line
        ''' </summary>
        ''' <remarks>only meaningful for linear and angular dimensions</remarks>
        ''' <returns></returns>
        Public ReadOnly Property DimPt1 As dxfVector
            Get
                UpdatePath()
                Select Case DimensionFamily
                    Case dxxDimensionTypes.Linear, dxxDimensionTypes.Angular
                        Return New dxfVector(Properties.Vector("*Vector1"))
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property

        ''' <summary>
        ''' the point where the dimensions second dimension line intesects the second extension line
        ''' </summary>
        ''' <remarks>only meaningful for linear and angular dimensions</remarks>
        ''' <returns></returns>
        Public ReadOnly Property DimPt2 As dxfVector
            Get

                UpdatePath()
                Select Case DimensionFamily
                    Case dxxDimensionTypes.Linear, dxxDimensionTypes.Angular
                        Return New dxfVector(Properties.Vector("*Vector2"))
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property
        Public Property DimScale As Double
            Get
                Return DimStyle.FeatureScaleFactor
            End Get
            Set(value As Double)
                DimStyle.FeatureScaleFactor = value
            End Set
        End Property

        ''' <summary>
        ''' returns a copy of the unsuppressed entities that define block of dimension
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Entities As colDXFEntities
            Get
                Return New colDXFEntities(PathEntities, aImageGUID:=ImageGUID, bIncludeInstances:=True, bNoHandles:=True, aSuppressedValue:=False, bGetClones:=True)
            End Get
        End Property

        ''' <summary>
        ''' the first of the extension lines
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExtensionLine1 As dxeLine
            Get
                Return GetSubEnt("Extension.Line1")
            End Get
        End Property

        ''' <summary>
        ''' the second of the extension lines
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExtensionLine2 As dxeLine
            Get
                Return GetSubEnt("Extension.Line2")
            End Get
        End Property

        ''' <summary>
        ''' the third of the extension lines
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExtensionLine3 As dxeLine
            Get
                Return GetSubEnt("Extension.Line3")
            End Get
        End Property

        ''' <summary>
        ''' the first hook line in the dimension block (if required)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HookLine1 As dxeLine
            Get
                Return GetSubEnt("Dimension.HookLine1")
            End Get
        End Property

        ''' <summary>
        ''' the second hook line in the dimension block (if required)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HookLine2 As dxeLine
            Get
                Return GetSubEnt("Dimension.HookLine2")
            End Get
        End Property

        ''' <summary>
        ''' returns the linear sub type type of the dimension
        ''' </summary>
        ''' <remarks>only applicable if the dimesnion family is linear</remarks>
        ''' <returns></returns>
        Public ReadOnly Property LinearDimensionType As dxxLinearDimTypes
            Get
                Select Case DimType
                    Case dxxDimTypes.LinearAligned
                        Return dxxLinearDimTypes.LinearAligned
                    Case dxxDimTypes.LinearHorizontal
                        Return dxxLinearDimTypes.LinearHorizontal
                    Case dxxDimTypes.LinearVertical
                        Return dxxLinearDimTypes.LinearVertical
                    Case Else
                        Return dxxLinearDimTypes.Undefined
                End Select
            End Get
        End Property

        ''' <summary>
        ''' returns the ordinate  sub type type of the dimension
        ''' </summary>
        ''' <remarks>only applicable if the dimesnion family is ordinate</remarks>
        ''' <returns></returns>
        Public ReadOnly Property OrdinateDimensionType As dxxOrdinateDimTypes
            '^the dimension entities dimension type name
            Get
                Select Case DimType
                    Case dxxDimTypes.OrdVertical
                        Return dxxOrdinateDimTypes.OrdVertical
                    Case dxxDimTypes.OrdHorizontal
                        Return dxxOrdinateDimTypes.OrdHorizontal
                    Case Else
                        Return dxxOrdinateDimTypes.Undefined
                End Select
            End Get
        End Property

        Public Overrides Property Name As String
            Get
                MyBase.Name = BlockName
                Return MyBase.Name
            End Get
            Friend Set(value As String)
                MyBase.Name = value
                SetPropVal("*BlockName", value, True)
            End Set
        End Property

        ''' <summary>
        ''' text to replace the actual dimension text with
        ''' </summary>
        ''' <returns></returns>
        Public Property OverideText As String
            Get
                Return PropValueStr("Override Text")
            End Get
            Set(value As String)
                SetPropVal("Override Text", value, True)
            End Set
        End Property

        ''' <summary>
        ''' the prefix added to the dimension text
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Prefix As String
            Get
                Return DimStyle.Prefix
            End Get
        End Property

        ''' <summary>
        ''' returns the radial sub type of the dimension
        ''' </summary>
        ''' <remarks>only applicable if the dimesnion family is Radial</remarks>
        ''' <returns></returns>
        Public ReadOnly Property RadialDimensionType As dxxRadialDimensionTypes
            Get
                Select Case DimType
                    Case dxxDimTypes.Radial
                        Return dxxRadialDimensionTypes.Radial
                    Case dxxDimTypes.Diametric
                        Return dxxRadialDimensionTypes.Diametric
                    Case Else
                        Return dxxRadialDimensionTypes.Undefined
                End Select
            End Get
        End Property

        ''' <summary>
        ''' returns the radius of the arc being dimensioned
        ''' </summary>
        ''' <remarks>0 if this is not a radial dimension</remarks>
        ''' <returns></returns>
        Public ReadOnly Property Radius As Double
            '^
            Get
                If DimensionFamily = dxxDimensionTypes.Radial Then Return dxfProjections.DistanceTo(DefPt10V, DefPt15V) Else Return 0
            End Get
        End Property

        ''' <summary>
        ''' the suffix added to the dimension text
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Suffix As String
            Get
                Return DimStyle.Suffix
            End Get
        End Property

        Public Property TextLineSpacingFactor As Double
            Get
                Return PropValueD("Text Line Spacing factor")
            End Get
            Set(value As Double)
                If value < 0.25 Then value = 0.25
                If value > 4 Then value = 4
                SetPropVal("Text Line Spacing factor", value, True)
            End Set
        End Property

        Public Property TextLineSpacingStyle As dxxLineSpacingStyles
            Get
                Return PropValueI("Text Line Spacing style")
            End Get
            Set(value As dxxLineSpacingStyles)
                If value < 1 Or value > 2 Then Return
                SetPropVal("Text Line Spacing style", value, True)
            End Set
        End Property

        Friend Property TextOffset As Double
            Get
                Return PropValueD("*TextOffset")
            End Get
            Set(value As Double)
                SetPropVal("*TextOffset", value, True)
            End Set
        End Property

        ''' <summary>
        ''' the primary dimension text
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TextPrimary As dxeText
            Get
                Return GetSubEnt("Text.1")
            End Get
        End Property

        ''' <summary>
        ''' the insertion point of the dimensions primary text
        ''' </summary>
        ''' <remarks>returns a c clone. DefPt11 for all dimension types</remarks>
        ''' <returns></returns>
        Public ReadOnly Property TextPt As dxfVector
            Get
                UpdatePath()
                Return New dxfVector(DefPt11)
            End Get
        End Property

        ''' <summary>
        ''' the bounding rectangle of the dimensions primary text
        ''' </summary>
        ''' <remarks> this rectangle is stretched byt 2 times the current text gap</remarks>
        ''' <returns></returns>
        Friend ReadOnly Property TextRectangle As dxfRectangle
            Get
                Dim aMText As dxeText = TextPrimary
                If aMText IsNot Nothing Then
                    Dim _rVal As dxfRectangle = aMText.BoundingRectangle
                    _rVal.Stretch(2 * DimStyle.TextGap)
                    Return _rVal
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' an additional rotation to apply to the dimension text
        ''' </summary>
        ''' <returns></returns>
        Public Property TextRotation As Double
            Get
                Return PropValueD("Text Rotation")
            End Get
            Set(value As Double)
                SetPropVal("Text Rotation", TVALUES.NormAng(value, False, True, True), True)
            End Set
        End Property

        ''' <summary>
        ''' the entity type flag used in DXF code generation
        ''' </summary>
        ''' <remarks>default = 0,128 = user placed text</remarks>
        ''' <returns></returns>
        Friend ReadOnly Property TypeFlag As Integer

            Get
                Dim _rVal As Integer = 0
                Select Case DimensionFamily
                    Case dxxDimensionTypes.Ordinate
                        If DimType = dxxDimTypes.OrdHorizontal Then _rVal = 32 + 6 + 64 Else _rVal = 32 + 6
                    Case dxxDimensionTypes.Radial
                        If DimType = dxxDimTypes.Diametric Then _rVal = 32 + 3 Else _rVal = 32 + 4
                    Case dxxDimensionTypes.Angular
                        If DimType = dxxDimTypes.Angular Then _rVal = 32 + 2 Else _rVal = 32 + 5
                    Case Else
                        If DimType = dxxDimTypes.LinearAligned Then _rVal = 33 Else _rVal = 32
                End Select
                Return _rVal
            End Get
        End Property

#End Region 'Properties
#Region "MustOverride Entity Methods"

        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            '#1the DXF object that carries the properties to define a new dxeDimension
            '#2the the base dim style for the dimension
            '#3the the block that was defined for the dimension
            '^Defines a new dxeDimension based on the properties of the passed objects.
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Dim eType As dxxEntityTypes
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))
            Dim aStr As String = String.Empty
            Dim bUserTxt As Boolean
            'determine the dimension type
            SuppressEvents = True
            Handle = aObj.Properties.GCValueStr(5)
            DisplayStructure = aObj.DisplayVars
            'get the dim properties
            PlaneV = aPlane
            Angle = aObj.Properties.GCValueD(51)
            eType = dxfUtils.DecodeDimensionType(aObj.Properties.GCValueI(70), aObj.Properties.GCValueD(50), bUserTxt)
            SetPropVal("*UserPositionedText", bUserTxt, False)
            DimType = eType.DimensionType()
            TextLineSpacingFactor = aObj.Properties.GCValueD(41)
            aStr = aObj.Properties.GCValueStr(1)
            If Trim(aStr) <> "" And Trim(aStr) <> "<>" Then OverideText = aStr
            DefPt10V = aObj.Properties.GCValueV(10) 'defpt 10
            DefPt11V = aPlane.WorldVector(aObj.Properties.GCValueV(11)) 'defpt 11
            DefPt13V = aObj.Properties.GCValueV(13)
            DefPt14V = aObj.Properties.GCValueV(14)
            DefPt15V = aObj.Properties.GCValueV(15)
            DefPt16V = aPlane.WorldVector(aObj.Properties.GCValueV(15))
            PlaneV = aPlane
            If aStyle IsNot Nothing Then
                aStyle.IsCopied = True
                Style = New TTABLEENTRY(aStyle)
            End If
            SetPropVal("*BlockName", "")
            BlockGUID = ""
            If aBlock IsNot Nothing Then
                If aBlock.Name <> "" Then
                    If aBlock.Name.ToUpper.StartsWith("*D") Then
                        SetPropVal("*BlockName", aBlock.Name)
                        BlockGUID = aBlock.GUID
                        PathEntities = New dxfEntities(aBlock.Entities)
                    End If
                End If
            End If
            If Me.PathEntities.Count > 0 Then IsDirty = False
            SuppressEvents = False
        End Sub
#End Region 'MustOverride Entity Methods
#Region "Methods"

        Public Overrides Sub UpdateProperties()
            MyBase.UpdateProperties()
            Properties.SetVal(70, TypeFlag)
        End Sub

        ''' <summary>
        ''' the dimension text box
        ''' </summary>
        ''' <remarks>the text box is always defined but is suppressed if the dimensions dimstyle text gap (DIMGAP) is greater than or equal to 0.</remarks>
        ''' <param name="aIndex">the index of the text to get (1 = primary)</param>
        ''' <returns></returns>
        Public Function TextBox(Optional aIndex As Integer = 1) As dxePolyline
            Return GetSubEnt($"TextBox.{ aIndex}")
        End Function

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeDimension
            Dim _rVal As dxeDimension = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeDimension
            Return New dxeDimension(Me)
        End Function

        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aOCS As TPLANE = PlaneV
            Dim tname As String = String.Empty
            Dim aBlk As dxfBlock
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            aBlk = GetBlock(aImage)
            'create the insert object
            If aBlk Is Nothing Then Return _rVal
            SetPropVal("*BlockName", aBlk.Name)
            BlockGUID = aBlk.GUID
            rBlock = aBlk
            aImage.AddDimStyleReference(Me)
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then
                    _rVal.Add(DXFProps(aInstances, i, aOCS, tname, aImage))
                End If
            Next i
            If iCnt > 1 Then
                _rVal.Name = $"{tname}-{ iCnt}_INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            'On Error Resume Next
            aInstance = Math.Abs(aInstance)
            If aInstance <= 0 Then aInstance = 1
            _rVal = New TPROPERTYARRAY(aInstance:=aInstance)
            Dim myProps As TPROPERTIES
            Dim aTrs As TTRANSFORMS
            Dim aPl As TPLANE = PlaneV
            Dim aVectors As TVECTORS
            rTypeName = "DIMENSION"
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1)
                TTRANSFORMS.Apply(aTrs, aPl)
                aVectors = DefPts.Vectors
                TTRANSFORMS.Apply(aTrs, aVectors)
                myProps = UpdateProps(aImage, aPl, aVectors, False)
            Else
                aVectors = DefPts.Vectors
                myProps = UpdateProps(aImage, aPl, aVectors, True)
            End If
            _rVal.Add(myProps, rTypeName, True, True)
            Return _rVal
        End Function

        ''' <summary>
        '''r eturns the requested subset of the dimensions block entities
        ''' </summary>
        ''' <param name="bSuppressExtLines"></param>
        ''' <param name="bSuppressDimLines"></param>
        ''' <param name="bSuppressText"></param>
        ''' <param name="bSuppressArrowHeads"></param>
        ''' <param name="bSuppressPoints"></param>
        ''' <param name="bReturnSuppressed"></param>
        ''' <returns></returns>
        Public Function DimEntities(Optional bSuppressExtLines As Boolean = False, Optional bSuppressDimLines As Boolean = False, Optional bSuppressText As Boolean = False, Optional bSuppressArrowHeads As Boolean = False, Optional bSuppressPoints As Boolean = True, Optional bReturnSuppressed As Boolean = False) As colDXFEntities
            Dim idents As String = String.Empty
            UpdatePath()
            If Not bSuppressDimLines Then TLISTS.Add(idents, "Dimension.", aDelimitor:=",")
            If Not bSuppressExtLines Then TLISTS.Add(idents, "Extension.", aDelimitor:=",")
            If Not bSuppressText Then
                TLISTS.Add(idents, "Text.", aDelimitor:=",")
                TLISTS.Add(idents, "TextBox.", aDelimitor:=",")
            End If
            If Not bSuppressArrowHeads Then TLISTS.Add(idents, "ArrowHead.", aDelimitor:=",")
            If Not bSuppressPoints Then TLISTS.Add(idents, "DefPt", aDelimitor:=",")
            Dim _rVal As New colDXFEntities(PathEntities.GetByIdentifiers(idents, ",", True, bIgnoreSuppressed:=Not bReturnSuppressed)) With {.ImageGUID = ImageGUID}
            _rVal.SetSuppressed(False)
            Return _rVal
        End Function

        ''' <summary>
        ''' used to extend the dimension out form its current dimension points
        ''' </summary>
        ''' <param name="aExtension"></param>
        Public Sub Extend(aExtension As Double)
            UpdatePath()
            If aExtension <> 0 Then
                Dim aLn As dxeLine
                Select Case DimensionFamily
                    Case dxxDimensionTypes.Linear
                        aLn = ExtensionLine2
                        DefPt10.Project(aLn.Direction, aExtension)
                    Case dxxDimensionTypes.Ordinate
                        aLn = ExtensionLine2
                        If aLn Is Nothing Then aLn = ExtensionLine1
                        DefPt11.Project(aLn.Direction, aExtension)
                    Case dxxDimensionTypes.Angular
                        aLn = ExtensionLine2
                        DefPt10.Project(aLn.Direction, aExtension)
                    Case dxxDimensionTypes.Radial
                        aLn = DefPt10.LineTo(DefPt15)
                        DefPt15.Project(aLn.Direction, aExtension)
                End Select
            End If
        End Sub

        ''' <summary>
        ''' the block containing all of the drawing entities that make up the dimension
        ''' </summary>
        ''' <param name="aImage"></param>
        ''' <returns></returns>
        Public Function GetBlock(aImage As dxfImage) As dxfBlock
            If Not GetImage(aImage) Then Return Nothing
            If BlockName = "" Then SetPropVal("*BlockName", aImage.HandleGenerator.NextDimBlockName())
            Dim _rVal As New dxfBlock(BlockName) With {.LayerName = LayerName}
            _rVal.SetEntities(Entities)
            Return _rVal
        End Function

        ''' <summary>
        ''' returns the dimensions block entity  with the matching identifier string assigned
        ''' </summary>
        ''' <param name="aIdentifier"></param>
        ''' <returns></returns>
        Public Function GetSubEnt(aIdentifier As String) As dxfEntity
            If String.IsNullOrWhiteSpace(aIdentifier) Then Return Nothing
            UpdatePath()
            Return PathEntities.Find(Function(x) String.Compare(x.Identifier, aIdentifier, True) = 0)
        End Function

        ''' <summary>
        ''' returns all the entities that make up the dimensions block (included the suppressed ones)
        ''' </summary>
        ''' <remarks>clones are returned</remarks>
        ''' <param name="bRegen"></param>
        ''' <returns></returns>
        Friend Function SubEntities(Optional bRegen As Boolean = False, Optional aSuppressedValue As Boolean? = Nothing) As colDXFEntities

            UpdatePath(bRegen)
            Return New colDXFEntities(PathEntities, aImageGUID:=ImageGUID, bIncludeInstances:=False, aSuppressedValue:=aSuppressedValue, bGetClones:=True)
        End Function


        Private Function UpdateProps(aImage As dxfImage, aPl As TPLANE, aVectors As TVECTORS, Optional bSave As Boolean = False) As TPROPERTIES
            Dim _rVal As TPROPERTIES
            If TPLANE.IsNull(aPl) Then aPl = PlaneV
            Dim aOCS As TPLANE = TPLANE.ArbitraryCS(aPl.ZDirection)
            Dim supv As String
            Dim dflg As Integer
            Dim aProps As New TPROPERTIES(ActiveProperties())
            Dim dType As dxxDimTypes = DimType

            If aVectors.Count <= 0 Then aVectors = DefPts.Vectors
            aProps.SetValueGC(2, BlockName)
            aProps.SetVectorGC(10, aVectors.Item(1))
            aProps.SetVectorGC(11, aVectors.Item(2).WithRespectTo(aOCS))
            aProps.SetVectorGC(12, aVectors.Item(3).WithRespectTo(aOCS))
            aProps.SetVectorGC(13, aVectors.Item(4))
            aProps.SetVectorGC(14, aVectors.Item(5))
            aProps.SetVectorGC(15, aVectors.Item(6))
            aProps.SetVectorGC(16, aVectors.Item(7).WithRespectTo(aOCS))
            aProps.SetVal("DimStyle Name", DimStyleName)
            aProps.SetVal("Horizontal Direction Angle(OCS)", aOCS.XDirection.AngleTo(aPl.XDirection, aOCS.ZDirection))
            aProps.SetVectorGC(210, aOCS.ZDirection, 1, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
            SetProps(aProps)
            MyBase.UpdateCommonProperties("DIMENSION")
            _rVal = New TPROPERTIES(Properties)
            If bSave Then
                _rVal = aProps.Clone
                GetImage(aImage)
                _rVal.ReduceToGC(100)
                'important
                _rVal.Append(dxfDimTool.GetOverrideProps(Me, aImage))
                If _rVal.ValueB("*UserPositionedText") Then
                    dflg = 128
                End If
                If Instances.Count = 0 Then dflg += 32
                _rVal.SetVal("leader length", 0)
                Select Case DimensionFamily
             '----------------------------------------------
                    Case dxxDimensionTypes.Ordinate
                        '----------------------------------------------
                        _rVal.SetValGC(100, "AcDbOrdinateDimension", 3)
                        _rVal.SetVal("Actual Measurement", DimensionValue)
                        dflg += 6
                        If dType = dxxDimTypes.OrdHorizontal Then
                            dflg += 64
                        End If
                        supv = "40,50,52,12,15,16"
             '----------------------------------------------
                    Case dxxDimensionTypes.Radial
                        '----------------------------------------------
                        _rVal.SetVal("Actual Measurement", Radius)
                        If dType = dxxDimTypes.Radial Then
                            _rVal.SetValGC(100, "AcDbRadialDimension", 3)
                            dflg += 4
                        Else
                            _rVal.SetValGC(100, "AcDbDiametricDimension", 3)
                            dflg += 3
                        End If
                        supv = "50,52,12,13,14,16"
             '----------------------------------------------
                    Case dxxDimensionTypes.Angular
                        '----------------------------------------------
                        _rVal.SetVal("Actual Measurement", DimensionValue)
                        dflg = 128 + 32
                        If dType = dxxDimTypes.Angular3P Then
                            _rVal.SetValGC(100, "AcDb3PointAngularDimension", 3)
                            dflg += 5
                        Else
                            _rVal.SetValGC(100, "AcDb2LineAngularDimension", 3)
                            dflg += 2
                        End If
                        supv = "40,50,52,12"
                        '----------------------------------------------
                    Case Else
                        '----------------------------------------------
                        supv = "40,52,12,15,16"
                        _rVal.SetValGC(100, "AcDbAlignedDimension", 3)
                        _rVal.SetVal("Actual Measurement", DimensionValue)
                        Select Case dType
                            Case dxxDimTypes.LinearAligned
                                dflg += 1
                                _rVal.SetValGC(50, Angle, 1) '"Angle of rotated, horizontal, or vertical dimensions"
                            Case dxxDimTypes.LinearVertical
                                _rVal.SetValGC(50, 90, 1)
                            Case dxxDimTypes.LinearHorizontal
                                _rVal.SetValGC(50, 0, 1)
                        End Select
                        If dType <> dxxDimTypes.LinearAligned Then
                            _rVal.SetValGC(100, "AcDbRotatedDimension", aOccurance:=4)
                        End If
                End Select
                _rVal.SetVal("Dimension type Flag", dflg)
                _rVal.SetSuppressionByGC(supv, True, True, aStartID:=dxfGlobals.CommonProps + 19)
            End If
            If bSave Then
                SetProps(_rVal)
            End If
            Return _rVal
        End Function
#End Region 'Methods



    End Class 'dxeDimension
End Namespace
