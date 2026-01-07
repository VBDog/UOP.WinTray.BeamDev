Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures
Imports System.Windows.Shapes

Namespace UOP.DXFGraphics

    Public Class dxeText
        Inherits dxfEntity
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aTextType As dxxTextTypes = dxxTextTypes.Multiline, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aGUIDPrefix As String = "")
            MyBase.New(aTextType, aGUIDPrefix:=aGUIDPrefix)
            If aDisplaySettings IsNot Nothing Then DisplayStructure = aDisplaySettings.Strukture
        End Sub

        Public Sub New(aEntity As dxeText, Optional bCloneInstances As Boolean = False)
            MyBase.New(aEntity.TextType, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Public Sub New(aAttrib As dxfAttribute, Optional aGUIDPrefix As String = "", Optional bCopyHandles As Boolean = False)
            MyBase.New(dxxTextTypes.Attribute, aGUIDPrefix:=aGUIDPrefix)
            If aAttrib Is Nothing Then Return
            Properties.CopyVals(aAttrib.Properties, aNamesToSkip:=New List(Of String)({"*TEXTTYPE"}), bSkipHandles:=Not bCopyHandles, bSkipPointers:=Not bCopyHandles)
            If bCopyHandles Then Handle = aAttrib.Handle
            AlignmentPt1 = Properties.Vector(10)
            AlignmentPt2 = Properties.Vector(11)

            Dim myPlane As dxfPlane = Plane
            myPlane.AlignZTo(Properties.Vector(210))
            Plane = myPlane

        End Sub

#End Region 'Constructors
#Region "Properties"

        Public Overrides ReadOnly Property EntityType As dxxEntityTypes
            Get
                Select Case TextType
                    Case dxxTextTypes.AttDef
                        Return dxxEntityTypes.Attdef
                    Case dxxTextTypes.Attribute
                        Return dxxEntityTypes.Attribute
                    Case dxxTextTypes.DText
                        Return dxxEntityTypes.Text
                    Case dxxTextTypes.Multiline
                        Return dxxEntityTypes.MText
                    Case Else
                        Return dxxEntityTypes.Text
                End Select

            End Get
        End Property
        Public Property Alignment As dxxMTextAlignments
            '^the Alignment of the text object with respect to it's insertion vector
            '~default = dxfBaselineLeft
            Get
                Return MyBase.Text_Alignment
            End Get
            Set(value As dxxMTextAlignments)
                MyBase.Text_Alignment = value


            End Set
        End Property

        Public ReadOnly Property DXFAlignment As dxxMTextAlignments
            '^the Alignment used in the output if the current alignment is baseline
            '~baseline alignments are not allowed om mtext objects in autocad
            '~use DXFInsertionPoint to get the adjusted insertion point
            Get
                Dim _rVal As dxxMTextAlignments = Alignment
                If GraphicType = dxxGraphicTypes.MText Then
                    If _rVal = dxxMTextAlignments.BaselineLeft Then
                        _rVal = dxxMTextAlignments.BottomLeft
                    ElseIf _rVal = dxxMTextAlignments.BaselineMiddle Then
                        _rVal = dxxMTextAlignments.BottomCenter
                    ElseIf _rVal = dxxMTextAlignments.BaselineRight Then
                        _rVal = dxxMTextAlignments.BottomRight
                    End If
                End If
                Return _rVal
            End Get
        End Property

        Public ReadOnly Property DXFInsertionPoint As dxfVector
            '^the Alignment used in the output if the current alignment is baseline
            '~baseline alignments are not allowed om mtext objects in autocad
            '~use DXFInsertionPoint to get the adjusted insertion point
            Get
                Dim _rVal As dxfVector = InsertionPt.Clone
                Dim alignt As dxxMTextAlignments = Alignment
                If GraphicType = dxxGraphicTypes.MText Then
                    If alignt = dxxMTextAlignments.BaselineLeft Then
                        _rVal = BoundingRectangle.BottomLeft
                    ElseIf alignt = dxxMTextAlignments.BaselineMiddle Then
                        _rVal = BoundingRectangle.BottomCenter
                    ElseIf alignt = dxxMTextAlignments.BaselineRight Then
                        _rVal = BoundingRectangle.BottomRight
                    End If
                End If
                Return _rVal
            End Get
        End Property

        Public ReadOnly Property AlignmentName As String
            Get
                Return dxfEnums.Description(Alignment)
            End Get
        End Property

        Public Property AlignmentPt1 As dxfVector
            '^the primary aligment pt vector for the text string
            Get
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                DefPts.VectorSet(1, New TVECTOR(value))
            End Set
        End Property

        Friend Property AlignmentPt1V As TVECTOR
            '^the primary aligment pt vector for the text string
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                '^the primary aligment pt vector for the text string
                DefPts.VectorSet(1, value)
            End Set
        End Property

        Public Property AlignmentPt2 As dxfVector
            '^the secondary aligment pt vector for the text string
            '~used for aligned and fit text
            Get
                Return DefPts.Vector2
            End Get
            Set(value As dxfVector)
                DefPts.VectorSet(2, New TVECTOR(value))
            End Set
        End Property

        Friend Property AlignmentPt2V As TVECTOR
            '^the secondary aligment pt vector for the text string
            '~used for aligned and fit text
            Get
                Return DefPts.VectorGet(2)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(2, value)
            End Set
        End Property

        Public ReadOnly Property AlignmentPtM As dxfVector
            '^the aligment pt used to write an MText to file
            '~adjusts the alignment pt to account for baseline alignments
            '~since autocad doesn't allow these for mtext
            Get
                Return New dxfVector With {.Strukture = AlignmentPtMV()}
            End Get
        End Property


        Public Property AttributeTag As String
            Get
                If GraphicType = dxxGraphicTypes.MText Then Return String.Empty
                Return PropValueStr(2)
            End Get
            Set(value As String)
                If GraphicType = dxxGraphicTypes.MText Then Return
                If value Is Nothing Then value = String.Empty
                SetPropVal(2, value.Trim().Replace(" ", "_"))
            End Set
        End Property

        Public Property AttributePrompt As String
            Get
                If GraphicType = dxxGraphicTypes.MText Then Return String.Empty
                Return PropValueStr(3)
            End Get
            Set(value As String)
                If GraphicType = dxxGraphicTypes.MText Then Return
                If value Is Nothing Then value = String.Empty
                SetPropVal(3, value.Trim().Replace(" ", "_"))
            End Set
        End Property

        Public Property AttributeType As dxxAttributeTypes
            '^the attribute definitions attribute type (if the text is an attribute)
            '~default = VerifyOnInput
            Get
                Dim _rVal As dxxAttributeTypes
                If Invisible Then _rVal = 1
                If Constant Then _rVal += 2
                If Verify Then _rVal += 4
                If Preset Then _rVal += 8
                Return _rVal
            End Get
            Set(value As dxxAttributeTypes)
                Dim valu As Integer = value
                Preset = False
                Constant = False
                Verify = False
                Invisible = False
                'set flag properties
                If valu > 7 Then
                    valu -= 8
                    Preset = True
                End If
                If valu > 3 Then
                    valu -= 4
                    Verify = True
                End If
                If valu > 1 Then
                    valu -= 2
                    Constant = True
                End If
                If valu > 0 Then
                    valu -= 1
                    Invisible = True
                End If
            End Set
        End Property

        Friend ReadOnly Property AttribV As TPROPERTY
            Get
                Return New TPROPERTY(Replace(Trim(AttributeTag), " ", "_")) With {.Prompt = Prompt, .Value = TextString}
            End Get
        End Property

        Public Property Backwards As Boolean
            Get
                If TextType = dxxTextTypes.Multiline Then Return False
                Return PropValueB("*Backwards")
            End Get
            Set(value As Boolean)
                If TextType = dxxTextTypes.Multiline Then Return
                SetPropVal("*Backwards", value, True)
            End Set
        End Property

        Public ReadOnly Property BaseLine As dxeLine
            Get
                Return New dxeLine(Strings.CharBox.BaseLineV)
            End Get
        End Property

        Public ReadOnly Property CharacterCount As Integer
            Get
                Return Strings.CharacterCount
            End Get
        End Property

        Public Property Constant As Boolean
            '^the attributes constant property (if the text is an attribute)
            Get
                Return PropValueB("*Constant")
            End Get
            Set(value As Boolean)
                SetPropVal("*Constant", value, False)
            End Set
        End Property

        Public Property DrawingDirection As dxxTextDrawingDirections
            Get
                Return PropValueI("*DrawingDirection", aDefault:=DirectCast(dxxTextDrawingDirections.ByStyle, Integer))
            End Get
            Set(value As dxxTextDrawingDirections)
                If value <> dxxTextDrawingDirections.ByStyle And value <> dxxTextDrawingDirections.Horizontal And value <> dxxTextDrawingDirections.Vertical Then Return
                SetPropVal("*DrawingDirection", value, True)
            End Set
        End Property
        Public Property DimensionTextAngle As Double
            Get
                Return PropValueD("*DimensionTextAngle")
            End Get
            Friend Set(value As Double)
                SetPropVal("*DimensionTextAngle", value, False)
            End Set
        End Property


        Public ReadOnly Property DTextString As String
            Get
                Return Strings.DTextString
            End Get
        End Property

        Friend Property FitFactor As Double
            Get
                If GraphicType = dxxGraphicTypes.MText Then Return 1 Else Return PropValueD("*FitFactor")
            End Get
            Set(value As Double)
                If GraphicType = dxxGraphicTypes.MText Then Return Else SetPropVal("*FitFactor", value, False)

            End Set
        End Property

        Public ReadOnly Property HasFormatCodes As Boolean
            Get
                Return Strings.HasFormats
            End Get
        End Property

        Public Shadows ReadOnly Property Height As Double
            Get
                UpdatePath()
                Return Bounds.Height
            End Get
        End Property

        Public ReadOnly Property HorizontalAlignment As dxxTextJustificationsHorizontal
            '^the horizontal Alignment of the text object with respect to it's insertion vector
            '~default = Left
            Get
                Return TFONT.AlignmentH(Alignment)
            End Get
        End Property

        Public Property InsertionPt As dxfVector
            '^the baseline start vector for the text string
            Get
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                AlignmentPt1 = value
            End Set
        End Property

        Friend Property InsertionPtV As TVECTOR
            '^the baseline start vector for the text string
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property

        Public Property Invisible As Boolean
            Get
                Return PropValueB("*Invisible")
            End Get
            Set(value As Boolean)
                SetPropVal("*Invisible", value, False)
            End Set
        End Property


        Public ReadOnly Property LineCount As Integer
            '^The total number of lines in the text
            Get
                Return Strings.Count
            End Get
        End Property

        Public Property LineNo As Integer
            '^the line no of a substring extracted form this text
            Get
                Return PropValueI("*LineNo")
            End Get
            Set(value As Integer)
                SetPropVal("*LineNo", value, False)
            End Set
        End Property

        Public Property LineSpacingFactor As Double
            '^controls the space between text lines
            '~min value is 0.25 and max value is 4
            Get
                If GraphicType = dxxGraphicTypes.MText Then
                    Return PropValueD("Line Spacing Factor", 1)
                Else
                    Return PropValueD("*Line Spacing Factor", 1)
                End If
            End Get
            Set(value As Double)
                If value < 0.25 Then value = 0.25
                If value > 4 Then value = 4
                If GraphicType = dxxGraphicTypes.MText Then
                    SetPropVal("Line Spacing Factor", value, True)
                Else
                    SetPropVal("*Line Spacing Factor", value, True)
                End If
            End Set
        End Property

        Public Property LineSpacingStyle As dxxLineSpacingStyles
            '^ the line spacing stype applied dureing path generation
            '~1 = "AtLeast" 2 = "Exact"
            Get
                If GraphicType = dxxGraphicTypes.MText Then
                    Return PropValueI("Line Spacing Style")
                Else
                    Return PropValueI("*Line Spacing Style")
                End If
            End Get
            Set(value As dxxLineSpacingStyles)
                If value < dxxLineSpacingStyles.AtLeast Or value > dxxLineSpacingStyles.Exact Then Return
                If GraphicType = dxxGraphicTypes.MText Then
                    SetPropVal("Line Spacing Style", value, True)
                Else
                    SetPropVal("*Line Spacing Style", value, True)
                End If
            End Set
        End Property

        Public Property LineStep As Double
            '^the distance between the centerlines of lines of the text
            '~ = 1.66 * Text Height * the line spacing factor (0.25-4)
            Get
                Return LineSpacingFactor * TextHeight * 1.66666666
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                LineSpacingFactor = value / (TextHeight * 1.66666666)
            End Set
        End Property

        Public ReadOnly Property MidPt As dxfVector
            '^the mid-point of the text line
            Get
                Return DefinitionPoint(dxxEntDefPointTypes.MidPt)
            End Get
        End Property

        Friend ReadOnly Property MidPtV As TVECTOR
            Get
                '^the mid-point of the text line
                Return MidPt.Strukture
            End Get
        End Property

        Friend Property MultiAttribute As Boolean
            Get
                Return PropValueB("*MultiAttribute")
            End Get
            Set(value As Boolean)
                SetPropVal("*MultiAttribute", value, False)
            End Set
        End Property

        Public Property ObliqueAngle As Double
            '^the text's oblique angle
            '~-85 to 85
            Get
                If TextType = dxxTextTypes.Multiline Then Return 0
                Return PropValueD(51)
            End Get
            Set(value As Double)
                If TextType = dxxTextTypes.Multiline Then Return

                SetPropVal(51, TVALUES.ObliqueAngle(value), True)
            End Set
        End Property

        Public Property Preset As Boolean
            '^the attributes preset property (if the text is an attribute)
            Get
                Return PropValueB("*Preset")
            End Get
            Set(value As Boolean)
                SetPropVal("*Preset", value, False)
            End Set
        End Property

        Public Property Prompt As String
            '^the Prompt value of the attribute (if the text is an attribute)
            Get
                If GraphicType = dxxGraphicTypes.MText Then Return String.Empty
                Return PropValueStr("Attribute Prompt")
            End Get
            Set(value As String)
                If GraphicType = dxxGraphicTypes.MText Then Return

                SetPropVal("Attribute Prompt", value, False)
            End Set
        End Property

        Public Property Rotation As Double
            '^the text angle
            '~default = 0
            Get
                Dim _rVal As Double = PropValueD(50)
                If TextType = dxxTextTypes.Multiline Then
                    _rVal *= 180 / Math.PI
                End If
                Return _rVal
                'If TextType <> dxxTextTypes.Multiline Then
                '    Return PropValueD("Rotation")
                'Else
                '    Return PropValueD("Text Angle(radians)", aMultiplier:=180 / Math.PI)
                'End If
            End Get
            Set(value As Double)
                value = TVALUES.NormAng(value, False, True)
                If TextType = dxxTextTypes.Multiline Then value *= Math.PI / 180
                SetPropVal(50, value, True)
            End Set
        End Property

        Friend Property SourceCount As Integer
            Get
                Return PropValueI("*SourceCount")
            End Get
            Set(value As Integer)
                SetPropVal("*SourceCount", value, False)
            End Set
        End Property

        Friend Property SourceString As String
            Get
                Return PropValueStr("*SourceString")
            End Get
            Set(value As String)
                SetPropVal("*SourceString", value, False)
            End Set
        End Property
        Public Overrides Property Strings As dxfStrings
            Get
                UpdatePath()
                Return MyBase.Strings
            End Get
            Friend Set(value As dxfStrings)
                MyBase.Strings = value
            End Set
        End Property
        Public Overrides Property TextStyleName As String
            Get
                Return PropValueStr(7, "Standard")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                SetPropVal(7, value, True)
            End Set
        End Property

        Public Property TextHeight As Double
            '^the hieght the text is drawn at unless it's style is of fixed height
            Get
                Return PropValueD("Text Height", 0.2)
            End Get
            Set(value As Double)
                If value = 0 Then Return
                value = Math.Abs(value)
                SetPropVal("Text Height", value, True)
            End Set
        End Property

        Public Property TextString As String
            '^the string used to generate the text
            '~this string can contain AutoCad Format codes for text generation
            Get
                Return PropValueStr(1)
            End Get
            Set(value As String)
                SetPropVal(1, value, True)
            End Set
        End Property



        Public ReadOnly Property TextTypeName As String
            Get
                Return dxfEnums.Description(TextType).ToUpper
            End Get
        End Property

        Public Property UpsideDown As Boolean
            Get
                If TextType = dxxTextTypes.Multiline Then Return False
                Return PropValueB("*UpsideDown")
            End Get
            Set(value As Boolean)
                If TextType = dxxTextTypes.Multiline Then Return
                SetPropVal("*UpsideDown", value, True)
            End Set
        End Property

        Public Property Verify As Boolean
            '^the attributes Verify property  (if the text is an attribute)
            '~a prompt appears when the attribute is inserted
            Get
                Return PropValueB("*Verify")
            End Get
            Set(value As Boolean)
                SetPropVal("*Verify", value, False)
            End Set
        End Property

        Public Property Vertical As Boolean
            '^controls how the characters are aligned
            Get
                If Not IsDimensionText And TextType <> dxxTextTypes.Multiline Then Return PropValueB("*Vertical") Else Return False
            End Get
            Set(value As Boolean)
                If IsDimensionText Or TextType = dxxTextTypes.Multiline Then value = False Else SetPropVal("*Vertical", value, True)
            End Set
        End Property

        Public Property IsDimensionText As Boolean
            Get
                Return PropValueB("*IsDimensionText")
            End Get
            Friend Set(value As Boolean)
                SetPropVal("*IsDimensionText", value, True)
            End Set
        End Property

        Public ReadOnly Property VerticalAlignment As dxxTextJustificationsVertical
            '^the vertical Alignment of the text object with respect to it's insertion vector
            '~default = Baseline
            Get
                Return TFONT.AlignmentV(Alignment)
            End Get
        End Property

        Public Shadows ReadOnly Property Width As Double
            Get
                Return Bounds.Width
            End Get
        End Property

        Public Property WidthFactor As Double
            '^the width factor for the string
            '~default = 1 min = 0.01 max = 100
            Get
                If GraphicType = dxxGraphicTypes.MText Then Return 1 Else Return PropValueD("Width Factor", 1)
            End Get
            Set(value As Double)
                If GraphicType = dxxGraphicTypes.MText Then Return Else SetPropVal("Width Factor", TVALUES.LimitedValue(Math.Abs(value), 0.01, 100), True)


            End Set
        End Property
#Region "MustOverride Entity Methods"

        Friend Function GetStrings(Optional bRegen As Boolean = False, Optional aImage As dxfImage = Nothing) As dxfStrings
            UpdatePath(bRegen, aImage)
            Return Strings
        End Function

        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)


            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim txt As String
            Dim txt1 As String
            Dim txt2 As String
            Dim txt3 As String
            Dim aLm As dxxMTextAlignments
            Dim ang As Double
            Dim vAlign As dxxTextJustificationsVertical
            Dim hAlign As dxxTextJustificationsHorizontal
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))
            Dim styname As String
            Dim aTextType As dxxTextTypes = TextType
            Dim k As Integer
            DisplayStructure = aObj.DisplayVars
            Select Case aTextType
                Case dxxTextTypes.Multiline
                Case Else
                    hAlign = aObj.Properties.GCValueL(72, dxxTextJustificationsHorizontal.Left)
                    vAlign = aObj.Properties.GCValueL(73, dxxTextJustificationsVertical.Baseline)
            End Select
            txt = aObj.Properties.GCValueStr(0, "TEXT")
            styname = aObj.Properties.GCValueStr(7, "Standard")
            If aStyle IsNot Nothing Then styname = aStyle.Name
            TextStyleName = styname
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
            '=============================================
            If aTextType = dxxTextTypes.Multiline Then
                '=============================================
                AlignmentPt1V = aObj.Properties.GCValueV(10)
                TextHeight = aObj.Properties.GCValueD(40, 0.2)
                Alignment = aObj.Properties.GCValueL(71, dxxMTextAlignments.TopLeft)
                'get the text angle
                Dim txtdir As dxfVector = aObj.Properties.GCVector(11)
                If txtdir IsNot Nothing Then
                    aPlane.AlignTo(txtdir, dxxAxisDescriptors.X)
                Else
                    ang = aObj.Properties.GCValueD(50, 0, 1, txt)
                    If txt <> "" Then Rotation = ang * 180 / Math.PI
                End If
                LineSpacingFactor = aObj.Properties.GCValueD(44, 1)
                k = 1
                txt1 = aObj.Properties.GCValueStr(1)
                txt3 = aObj.Properties.GCValueStr(3, "", k)
                If txt3 = "" Then
                    txt = txt1
                Else
                    txt2 = "X"
                    Do Until txt2 = ""
                        k += 1
                        txt2 = aObj.Properties.GCValueStr(3, "", k)
                        If txt2 <> "" Then txt3 += txt2
                    Loop
                    txt = txt3 & txt1
                End If
                txt = TFONT.CADTextToScreenText(txt)
                If String.Compare(Left(txt, 1), "{", True) = 0 And String.Compare(Right(txt, 1), "}", True) = 0 Then
                    txt = Mid(txt, 2, txt.Length - 2)
                End If
                TextString = txt
                '=============================================
            Else
                '=============================================
                aLm = TFONT.DecodeAlignment(vAlign, hAlign)
                Alignment = aLm
                TextHeight = aObj.Properties.GCValueD(40)
                WidthFactor = aObj.Properties.GCValueD(41, 1)
                ObliqueAngle = aObj.Properties.GCValueD(51)
                Rotation = aObj.Properties.GCValueD(50)
                LineSpacingFactor = aObj.Properties.GCValueD(44, 1)
                txt = aObj.Properties.GCValueStr(1)
                txt = TFONT.CADTextToScreenText(txt)
                TextString = txt
                '           pln_Revolve aPlane, -ang
                v1 = aObj.Properties.GCValueV(10)
                v1 = aPlane.WorldVector(v1)
                v2 = aObj.Properties.GCValueV(11, TVECTOR.Zero, 1, txt, 0)
                If txt = "" Then
                    v2 = v1
                Else
                    v2 = aPlane.WorldVector(v2)
                End If
                If aLm <> dxxMTextAlignments.Fit And aLm <> dxxMTextAlignments.Aligned Then
                    AlignmentPt1V = v2
                    AlignmentPt2V = v1
                Else
                    AlignmentPt1V = v1
                    AlignmentPt2V = v2
                End If
                If aTextType = dxxTextTypes.AttDef Or aTextType = dxxTextTypes.Attribute Then
                    Prompt = aObj.Properties.GCValueStr(3)
                    AttributeTag = aObj.Properties.GCValueStr(2)
                    AttributeType = aObj.Properties.GCValueStr(70)
                End If
            End If
            PlaneV = aPlane
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeText
            Dim _rVal As dxeText = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeText
            Return New dxeText(Me)
        End Function
        Public Overloads Function Clone(aNewInsertionPt As iVector, Optional aNewTextString As String = Nothing, Optional aNewTextHeight As Double? = Nothing, Optional aNewAlignment As dxxMTextAlignments? = Nothing, Optional aNewTextType As dxxTextTypes? = Nothing) As dxeText
            Dim ttype As dxxTextTypes = TextType
            '^returns a new object with properties matching those of the cloned object
            Dim _rVal As dxeText = Nothing
            If aNewTextType.HasValue Then
                If aNewTextType.Value = ttype Then
                    aNewTextType = Nothing
                Else

                End If
            End If


            If aNewTextType.HasValue Then

                UpdateCommonProperties(bUpdateProperties:=True)
                Dim myprops As dxoProperties = Properties
                _rVal = New dxeText(aNewTextType.Value) With
                {
                    .TextString = TextString,
                    .TextHeight = TextHeight,
                    .Alignment = Alignment,
                    .Rotation = Rotation,
                    .TextStyleName = TextStyleName,
                    .DisplayStructure = DisplayStructure,
                    .PlaneV = PlaneV
                }

                _rVal.DefPts.Copy(DefPts)

                Dim cprops As dxoProperties = _rVal.Properties
                cprops.SetVal("*IsDimensionText", myprops.ValueB("*IsDimensionText"))
                If ttype = dxxTextTypes.Multiline Then  'change from mtext to dtext
                    _rVal.TextString = DTextString


                    cprops.SetVal(50, myprops.ValueD(50) * 180 / Math.PI) 'text angle
                    cprops.SetVal("*Line Spacing Factor", myprops.ValueD(44))
                    cprops.SetVal("*Line Spacing Style", myprops.ValueD(73))
                    cprops.SetVal("*DrawingDirection", myprops.ValueD(72))

                Else
                    If aNewTextType.Value = dxxTextTypes.Multiline Then 'change From dtext to mtext
                        _rVal.TextString = DTextString
                    Else
                        'change amongst dtext types so the properties are the same
                        cprops.CopyVals(myprops, aGCsToSkip:=New List(Of Integer)({0, 100}), aNamesToSkip:=New List(Of String)({"*GUID", "*GroupName", "*EntityType"}), bSkipHandles:=True, bSkipPointers:=True)
                    End If
                End If
            Else
                _rVal = New dxeText(Me)
            End If

            If aNewTextString IsNot Nothing Then _rVal.TextString = aNewTextString
            If aNewInsertionPt IsNot Nothing Then _rVal.InsertionPt = New dxfVector(aNewInsertionPt)
            If aNewTextHeight.HasValue Then
                If aNewTextHeight.Value > 0 Then _rVal.TextHeight = aNewTextHeight.Value
            End If
            If aNewAlignment.HasValue Then
                If aNewAlignment.Value <> dxxMTextAlignments.AlignUnknown Then _rVal.Alignment = aNewAlignment.Value
            End If

            _rVal.SourceGUID = GUID
            Return _rVal
        End Function
#End Region 'MustOverride Entity Methods
#End Region 'Properties
#Region "Methods"


        Public Function VisibleCharacters(aIncludeLineFeeds As Boolean) As String

            Dim _rVal As String = String.Empty
            'On Error Resume Next
            Dim i As Integer
            Dim aStrs As dxfStrings = Strings
            For i = 0 To aStrs.Count - 1
                If aIncludeLineFeeds And i > 0 Then _rVal += vbLf
                _rVal += aStrs.Item(i).ToString()
            Next i
            Return _rVal

        End Function

        Friend Function AlignmentPtV(aAlignment As dxxMTextAlignments) As TVECTOR
            '^the aligment pt vector for the text string bassed on the passed alignment

            Dim aPt As dxxRectanglePts
            Dim aStrs As dxfStrings = Strings
            If aStrs.CharacterCount <= 0 Then
                Return AlignmentPt1V
            End If
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
            Return aStrs.CharBox.PointV(aPt)

        End Function

        Public Function ConvertToTextType(aTextType As dxxTextTypes) As List(Of dxeText)
            If Not dxfEnums.Validate(GetType(dxxTextTypes), TVALUES.To_INT(aTextType), bSkipNegatives:=True) Then Return New List(Of dxeText)
            Dim _rVal As New List(Of dxeText)
            If aTextType = TextType Then
                _rVal.Add(Clone())
                Return _rVal
            End If
            '^returns a new object with properties matching those of the cloned object
            Dim txt As dxeText
            If aTextType <> dxxTextTypes.Multiline And TextType <> dxxTextTypes.Multiline Then
                txt = Clone()
                txt.TextType = aTextType
                _rVal.Add(txt)
                Return _rVal
            End If
            If TextType = dxxTextTypes.Multiline Then
                Dim substrs As List(Of dxeText) = SubStrings(aTextType)
                _rVal.AddRange(substrs)
                Return _rVal

            End If

            txt = New dxeText(aTextType, DisplaySettings) With {
                .TextString = TextString,
                .Alignment = Alignment,
                .AlignmentPt1V = AlignmentPt1V,
                .AlignmentPt2V = AlignmentPt2V,
                .TextHeight = TextHeight,
                .WidthFactor = WidthFactor
            }
            _rVal.Add(txt)
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Dim suf As String = $"[{ TextTypeName }]"

            'Select Case TextType
            '    Case dxxTextTypes.AttDef
            '        suf += $" { AttributeTag}"
            '    Case dxxTextTypes.Attribute
            '        suf += $" { AttributeTag}:{ DTextString}"
            '    Case Else
            '        suf += $" { DTextString}"
            'End Select
            Return MyBase.ToString & suf
        End Function
        Friend Function AlignmentPtMV() As TVECTOR
            Dim rAlignment As dxxMTextAlignments = dxxMTextAlignments.AlignUnknown
            Return AlignmentPtMV(rAlignment)
        End Function
        Friend Function AlignmentPtMV(ByRef rAlignment As dxxMTextAlignments) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            '^the aligment pt used to write an MText to file
            '~adjusts the alignment pt to account for baseline alignments
            '~since autocad doesn't allow these for mtext
            rAlignment = Alignment
            _rVal = AlignmentPt1V
            Dim aStrs As dxfStrings = Strings
            If rAlignment > dxxMTextAlignments.BottomRight And TextType = dxxTextTypes.Multiline Then
                _rVal = aStrs.MTextAlignmentPt(_rVal, rAlignment)
            End If
            Return _rVal
        End Function
        Public Function SubRectangle(Optional aLineNo As Integer = 0, Optional aPlane As dxfPlane = Nothing, Optional bIncludeInstances As Boolean = False) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            '#1an optional line number
            '#2an optional plane to get the rectangle on
            '^a rectangle that encompasses the entity
            '~if the plan is not passed the entity's definition plane is assumed
            UpdatePath()
            Dim aStrs As dxfStrings = Strings
            If dxfPlane.IsNull(aPlane) Then aPlane = Plane
            If aLineNo <= 0 Then
                _rVal = BoundingRectangle(aPlane, bIncludeInstances)
            Else
                _rVal = New dxfRectangle(aPlane.Strukture)
                If aLineNo > 0 And aLineNo <= aStrs.Count Then
                    If dxfPlane.IsNull(aPlane) Then
                        _rVal.Strukture = aStrs.Item(aLineNo - 1).BoundingRectangleV
                    Else
                        _rVal.Strukture = aStrs.Item(aLineNo - 1).BoundingRectangleV
                    End If
                End If
            End If
            Return _rVal
        End Function
        Public Function Character(aCharIndex As Integer, Optional aLineNo As Integer = 0) As dxoCharacter
            '^the returns the character on the spcified line and position (does not include format code chars)
            Dim aChar As dxoCharacter = Strings.Character(aCharIndex, aLineNo)
            If aChar Is Nothing Then Return Nothing
            Return New dxoCharacter(aChar)
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aOCS As TPLANE = TPLANE.World
            Dim tname As String = String.Empty
            Dim aArray As New TPROPERTYARRAY
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            Dim j As Integer
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then
                    aArray = DXFProps(aInstances, i, aOCS, tname, aImage)
                    If aArray.Count > 1 Then
                        If i = 1 Then tname = "MUTLI_" & tname
                        For j = 1 To aArray.Count
                            _rVal.AddProperties(aArray.Item(j))
                        Next j
                    Else
                        _rVal.Add(aArray)
                    End If
                End If
            Next i
            If iCnt > 1 Then
                _rVal.Name = tname & "-" & iCnt & " INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            'On Error Resume Next
            GetImage(aImage)
            aInstance = Math.Abs(aInstance)
            If aInstance <= 0 Then aInstance = 1
            If aInstances Is Nothing Then aInstances = Instances
            Return dxfEntity.Text_DXF(Me, aImage, aInstance, aOCS, rTypeName, aInstances)
        End Function

        Public Function LiesOnPlane(aPlane As dxfPlane, Optional aFudgeFactor As Double = 0.001) As Boolean
            Dim _rVal As Boolean = False
            '#1the plane to dxeDimension
            '#2the distance to determine if then start and end points are on the plane
            '^returns true if the entity lies on the passed plane
            _rVal = Plane.IsEqual(aPlane, 1, False, False)
            Return _rVal
        End Function
        Public Function LineSegments(Optional CurveDivisions As Integer = 20, Optional LineDivisions As Integer = 1) As colDXFEntities
            '^returns the entity as a collection of lines
            Return BoundingRectangle.BorderLines
        End Function

        Friend Function NullStrings(aStyle As dxoStyle, Optional aImage As dxfImage = Nothing) As TSTRINGS
            Dim _rVal As New TSTRINGS(PlaneV, Domain, TextType)
            'the string to be rendered including format characters
            _rVal.FormatString = TextString
            GetImage(aImage)
            Dim tht As Double
            Dim fInfo As TFONTSTYLEINFO
            If aStyle.Properties.Count = 0 Then
                aStyle = GetTextStyle(aImage)
            End If
            fInfo = dxoFonts.GetFontStyleInfo(aStyle.FontName, aStyle.FontIndex, aTTFStyle:=aStyle.FontStyle)
            If Not fInfo.NotFound Then
                aStyle.FontIndex = fInfo.FontIndex
            End If
            If TextHeight <= 0 Then
                tht = aStyle.TextHeight
                If tht > 0 Then
                    TextHeight = tht
                Else
                    If aImage IsNot Nothing Then
                        TextHeight = aImage.Header.TextSize
                    End If
                End If
            End If
            If LineSpacingFactor <= 0 Then LineSpacingFactor = aStyle.LineSpacingFactor
            If LineSpacingFactor <= 0 Then LineSpacingFactor = 1
            _rVal.AlignmentPtV = AlignmentPt1V
            _rVal.LayerName = LayerName
            _rVal.LineWeight = LineWeight
            If aImage IsNot Nothing Then
                _rVal.TextBoxes = aImage.Header.PropValueI(dxxHeaderVars.QTEXTMODE) = 1
            End If
            _rVal.LineSpacingFactor = LineSpacingFactor
            _rVal.LineSpacingStyle = LineSpacingStyle
            _rVal.Alignment = Alignment
            Dim aFmats As TCHARFORMAT = _rVal.Formats
            aFmats.Color = Color
            aFmats.FontIndex = fInfo.FontIndex
            aFmats.StyleIndex = fInfo.StyleIndex
            aFmats.IsShape = fInfo.IsShape
            aFmats.CharHeight = TextHeight
            If aFmats.CharHeight <= 0 Then aFmats.CharHeight = 0.2
            aFmats.Backwards = Backwards
            aFmats.ObliqueAngle = ObliqueAngle
            aFmats.WidthFactor = WidthFactor
            aFmats.HeightFactor = 1
            aFmats.UpsideDown = UpsideDown
            '.Rotation = Rotation  'Rotation is now handled by call to PlaneV
            aFmats.Tracking = 1
            If TextType = dxxTextTypes.Multiline Then
                aFmats.WidthFactor = aStyle.WidthFactor
                aFmats.ObliqueAngle = aStyle.ObliqueAngle
                aFmats.UpsideDown = False
                aFmats.Backwards = False
                If DrawingDirection = dxxTextDrawingDirections.ByStyle Then
                    aFmats.Vertical = aStyle.Vertical
                Else
                    aFmats.Vertical = DrawingDirection = dxxTextDrawingDirections.Vertical
                End If
                If fInfo.IsShape Then
                    aFmats.Vertical = aStyle.PropValueB(dxxStyleProperties.VERTICAL)
                End If
            Else
                If aFmats.WidthFactor <= 0 Then aFmats.WidthFactor = aStyle.WidthFactor
                If fInfo.IsShape Then aFmats.Vertical = Vertical
                aFmats.UpsideDown = aStyle.UpsideDown
                aFmats.Backwards = aStyle.Backwards
            End If
            aFmats.TextStyleName = aStyle.Name
            _rVal.Formats = aFmats

            'times_End NullStrings.Times
            Return _rVal
        End Function
        Public Function Perimeter(Optional bAsLines As Boolean = False, Optional aCurveDivisions As Double = 20, Optional bClosed As Boolean = False) As dxePolyline
            Dim _rVal As dxePolyline = Nothing
            '^returns the entity as a Polyline
            _rVal = BoundingRectangle.Perimeter
            Return _rVal
        End Function
        Friend Sub Planarize(ByRef rAlignPt1 As TVECTOR, ByRef rAlignPt2 As TVECTOR)
            Dim aPl As TPLANE = PlaneV
            AlignmentPt1V = AlignmentPt1V.ProjectedTo(aPl)
            rAlignPt1 = AlignmentPt1V
            AlignmentPt2V = AlignmentPt2V.ProjectedTo(aPl)
            rAlignPt2 = AlignmentPt2V
        End Sub

        Public Function SubString(aLineNo As Integer, aGroupNo As Integer, Optional aTextType As dxxTextTypes = dxxTextTypes.Multiline) As dxeText
            Return Strings.SubText(Me, aLineNo, aGroupNo, aTextType)
        End Function
        Public Function SubStrings(Optional aTextType As dxxTextTypes = dxxTextTypes.Undefined) As List(Of dxeText)
            Dim _rVal As New List(Of dxeText)
            '^returns the substrings of the passed string
            'On Error Resume Next

            If aTextType < dxxTextTypes.DText And aTextType > dxxTextTypes.Multiline Then aTextType = TextType
            Dim aStrs As dxfStrings = Strings
            Dim aStr As dxoString
            Dim i As Integer

            Dim aTxt As dxeText
            Dim tagbase As String = AttributeTag
            If String.IsNullOrWhiteSpace(tagbase) Then tagbase = "Attrib"
            For i = 1 To aStrs.Count
                aStr = aStrs.Item(i - 1)
                If aStr.CharacterCount > 0 Then
                    If aStr.GroupCount > 0 Then
                        For j = 1 To aStr.GroupCount
                            aTxt = aStrs.SubText(Me, i, j, aTextType)
                            aTxt.AttributeTag = tagbase
                            _rVal.Add(aTxt)
                        Next j
                    Else
                        aTxt = aStrs.SubText(Me, i, 1, aTextType)
                        aTxt.AttributeTag = tagbase
                        _rVal.Add(aTxt)
                    End If
                End If
            Next i
            If _rVal.Count > 0 Then
                i = 1
                For Each aTxt In _rVal
                    aTxt.AttributeTag += $"_{i}"
                    i += 1
                Next

            End If
            Return _rVal
        End Function
        Friend Function ToDText(aTextType As dxxTextTypes, aImage As dxfImage) As List(Of dxeText)
            Dim _rVal As New List(Of dxeText)
            If Not GetImage(aImage) Then aImage = dxfGlobals.New_Image()
            If aTextType <> dxxTextTypes.AttDef And aTextType <> dxxTextTypes.Attribute And aTextType <> dxxTextTypes.DText Then aTextType = dxxTextTypes.DText
            Dim aTxt As dxeText



            UpdatePath(False, aImage)

            Dim subStrs As dxfStrings = Strings

            Dim aTag As String = AttributeTag
            Dim bAtts As Boolean = aTextType = dxxTextTypes.AttDef Or aTextType = dxxTextTypes.Attribute
            Dim idstr As String

            If bAtts And aTag = "" Then aTag = aImage.HandleGenerator.NextAttributeTag()
            Dim idx As Integer = 0
            If subStrs.Count > 1 Then
                For i As Integer = 1 To subStrs.Count
                    Dim SubStr As dxoString = subStrs.Item(i - 1)
                    For j As Integer = 1 To SubStr.GroupCount
                        aTxt = SubString(i, j, aTextType)
                        If aTxt IsNot Nothing Then
                            aTxt.MultiAttribute = bAtts
                            idx += 1
                            aTxt.LineNo = idx
                            If bAtts Then
                                idstr = idx
                                If idstr.Length < 3 Then idstr = New String("0", idstr.Length - 3) & idstr
                                aTxt.AttributeTag = $"{aTag}_{ idstr}"
                            End If
                            If idx = 1 Then
                                aTxt.Handle = Handle
                                aTxt.GUID = GUID
                            End If
                            aTxt.UpdatePath(False, aImage)
                            _rVal.Add(aTxt)
                        End If
                    Next j
                Next i
            Else
                idx = 1
                aTxt = SubString(1, 1, aTextType)
                aTxt.MultiAttribute = False
                aTxt.UpdatePath(False, aImage)
                _rVal.Add(aTxt)
            End If
            For i As Integer = 1 To _rVal.Count
                aTxt = _rVal.Item(i)
                aTxt.SourceCount = idx
            Next i
            Return _rVal
        End Function
        Public Function TextBoxes(Optional bTextRectangles As Boolean = False, Optional bSuppressGroups As Boolean = False, Optional aLineNo As Integer = 0, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLayer As String = "") As colDXFEntities
            Dim _rVal As New colDXFEntities()
            Dim aStrs As dxfStrings = Strings
            Dim i As Integer
            Dim j As Integer
            Dim gi As Integer
            Dim aStr As dxoString
            Dim bStr As dxoString
            Dim aRec As TPLANE
            Dim aPl As dxePolyline
            aPl = New dxePolyline With {
            .DisplayStructure = DisplayStructure,
            .ImageGUID = ImageGUID,
            .Closed = True,
            .PlaneV = PlaneV,
            .Boundless = True}
            If aColor <> dxxColors.Undefined Then aPl.Color = aColor

            For i = 1 To aStrs.Count
                If aLineNo = 0 Or (i = aLineNo) Then
                    aStr = aStrs.Item(i - 1)
                    j = 0
                    If aStr.CharacterCount > 0 Then
                        If Not bSuppressGroups Then
                            For gi = 1 To aStr.GroupCount
                                bStr = aStr.GetByGroupIndex(gi, j, j)
                                If bStr.CharacterCount > 0 Then
                                    bStr.UpdateBounds()
                                    aRec = bStr.BoundingRectangleV
                                    aPl.VectorsV = aRec.RectanglePts(bTextRectangles, False)
                                    _rVal.Add(aPl, bAddClone:=True)
                                    j = bStr.LastChar.LineIndex
                                End If
                            Next gi
                        Else
                            If bTextRectangles Then aRec = aStr.CharBox.PlaneV(False) Else aRec = aStr.BoundingRectangleV
                            aPl.VectorsV = aRec.RectanglePts(bTextRectangles, False)
                            _rVal.Add(aPl, bAddClone:=True)
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function TextRectangle(Optional aLineNo As Integer = 0, Optional aBuffer As Double = 0.0) As dxoCharBox

            Return New dxoCharBox(TextRectangleV(aLineNo, aBuffer))

        End Function
        Friend Function TextRectangleV(Optional aLineNo As Integer = 0, Optional aBuffer As Double = 0.0) As TCHARBOX
            Dim _rVal As TCHARBOX
            Dim aStrs As dxfStrings = Strings
            If aLineNo < 1 Or aLineNo > aStrs.Count Then aLineNo = 0
            If aLineNo = 0 Then
                _rVal = New TCHARBOX(aStrs.CharBox)
            Else
                _rVal = New TCHARBOX(aStrs.Item(aLineNo - 1).CharBox)

            End If
            If aBuffer <> 0 Then
                _rVal.Stretch(aBuffer, True, True)
            End If
            Return _rVal
        End Function
        Public Function BoundingRect(Optional aLineNo As Integer = 0, Optional aBuffer As Double = 0.0) As dxfRectangle
            Return New dxfRectangle(BoundingRectangleV(aLineNo, aBuffer))
        End Function
        Friend Function BoundingRectangleV(Optional aLineNo As Integer = 0, Optional aBuffer As Double = 0.0) As TPLANE
            Dim _rVal As TPLANE
            Dim aStrs As dxfStrings = Strings
            If aLineNo < 1 Or aLineNo > aStrs.Count Then aLineNo = 0
            If aLineNo = 0 Then
                _rVal = BoundingRectangle.Strukture
            Else
                _rVal = aStrs.Item(aLineNo - 1).GetBounds().Strukture
            End If
            If aBuffer <> 0 Then
                _rVal.Stretch(aBuffer, True, True, False, True)
            End If
            Return _rVal
        End Function
        Friend Function GetTextStyle(Optional arImage As dxfImage = Nothing) As dxoStyle
            If GetImage(arImage) Then
                Return arImage.TableEntry(dxxReferenceTypes.STYLE, TextStyleName)
            Else
                Return New dxoStyle(TextStyleName)
            End If

        End Function
#End Region 'Methods
#Region "Shared Methods"
        Friend Shared Function DecodeTextType(aTextType As Object) As dxxTextTypes
            Dim iVal As Integer = TVALUES.To_INT(aTextType, dxxTextTypes.DText)
            If iVal = dxxTextTypes.DText Then Return dxxTextTypes.DText
            If iVal = dxxTextTypes.AttDef Then Return dxxTextTypes.AttDef
            If iVal = dxxTextTypes.Attribute Then Return dxxTextTypes.Attribute
            If iVal = dxxTextTypes.Multiline Then Return dxxTextTypes.Multiline
            Return dxxTextTypes.DText
        End Function


#End Region 'Shared Methods
    End Class 'dxeText
End Namespace
