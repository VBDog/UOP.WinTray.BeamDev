Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxeTable
        Inherits dxfEntity
#Region "Members"

#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Table)

        End Sub

        Public Sub New(aEntity As dxeTable, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Table, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Table)


            DefineByObject(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Borders As colDXFEntities
            Get
                Return Entities.GetByGraphicType(dxxGraphicTypes.Line, bReturnClones:=True)
            End Get
        End Property
        Public ReadOnly Property CellBoundary As dxfRectangle
            Get
                UpdatePath()
                Return New dxfRectangle(Cells.CellBoundary)
            End Get
        End Property

        Public Property Cells As dxoTableCells
            Get
                Return TableCells
            End Get
            Friend Set(value As dxoTableCells)
                TableCells = value
            End Set
        End Property

        Public Property ColumnCount As Integer
            '^the number of rows in the table
            Get
                Return PropValueI("ColCount")
            End Get
            Set(value As Integer)
                If value < 1 Then value = 1
                If value > 1000 Then value = 1000
                If SetPropVal("ColCount", value, True) Then
                    Resize()
                End If
            End Set
        End Property
        Public Property Delimiter As String
            '^the string delimiter used to seperate cell values in a string
            '~default = '|'
            Get
                Return PropValueStr("Delimiter")
            End Get
            Set(value As String)
                '^the string delimiter used to seperate cell values in a string
                '~default = '|'
                value = Trim(value)
                If value = String.Empty Then Return
                SetPropVal("Delimiter", value, False)
            End Set
        End Property
        Public ReadOnly Property Dimensions As String
            Get
                '^returns Rows X Columns in a string
                Return RowCount & " X " & ColumnCount
            End Get
        End Property
        Friend ReadOnly Property Entities As colDXFEntities
            Get
                Return New colDXFEntities(PathEntities, aImageGUID:=ImageGUID, bIncludeInstances:=False)
            End Get
        End Property
        Public Property Footer As String
            Get
                '^the text string drawn below the table boundary
                Return PropValueStr("Footer")
            End Get
            Set(value As String)
                '^the text string drawn below the table boundary
                value = Trim(value)
                SetPropVal("Footer", value, True)
            End Set
        End Property
        Public Property HeaderCol As Integer
            '^the column which will be treated as the header column
            Get
                Return PropValueI("HeaderCol")
            End Get
            Set(value As Integer)
                If value < 0 Then value = 0
                SetPropVal("HeaderCol", value, True)
            End Set
        End Property
        Public Property HeaderRow As Integer
            '^the row which will be treated as the header row
            Get
                Return PropValueI("HeaderRow")
            End Get
            Set(value As Integer)
                If value < 0 Then value = 0
                SetPropVal("HeaderRow", value, True)
            End Set
        End Property
        Public Shadows ReadOnly Property Height As Double
            '^the height of the table
            Get
                Return BoundingRectangle.Height
            End Get
        End Property
        Public Property HeaderTextStyle As String
            '^the text stype to apply to the header text
            Get
                Return PropValueStr("HeaderTextStyle")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("HeaderTextStyle", value, True)
            End Set
        End Property
        Public Property TitleTextStyle As String
            '^the text style that will be applied to the title text
            Get
                Return PropValueStr("TitleTextStyle")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("TitleTextStyle", value, True)
            End Set
        End Property
        Public Property FooterTextStyle As String
            '^the text style that will be applied to the footer text
            Get
                Return PropValueStr("FooterTextStyle")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("HeaderTextStyle", value, True)
            End Set
        End Property
        Public Overrides Property TextStyleName As String
            '^the text style that will be applied to the cell text
            Get
                Return PropValueStr("TextStyleName")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = "Standard" Else value = value.Trim
                SetPropVal("TextStyleName", Trim(value))
            End Set
        End Property
        Public Property Alignment As dxxRectangularAlignments
            '^used to set the tables alignment value
            '~controls how the table is aligned with respect to its insertion point.
            '~default = dxfTopLeft.
            Get
                Return PropValueI("Alignment")
            End Get
            Set(value As dxxRectangularAlignments)
                If value >= 1 And value <= 9 Then SetPropVal("Alignment", value, True)
            End Set
        End Property

        Public Property CellAlignment As dxxRectangularAlignments
            '^used to set the tables alignment value
            '~controls how the table is aligned with respect to its insertion point.
            '~default = dxfTopLeft.
            Get
                Return PropValueI("CellAlignment")
            End Get
            Set(value As dxxRectangularAlignments)
                If value >= 1 And value <= 9 Then SetPropVal("CellAlignment", value, True)
            End Set
        End Property

        Public Property InsertionPt As dxfVector
            '^the point where the entity was inserted
            Get
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                MyBase.DefPts.SetVector(value, 1)
            End Set
        End Property
        Friend Property InsertionPtV As TVECTOR
            '^the point where the entity was inserted
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property
        Public Property RowCount As Integer
            '^the number of rows in the table
            Get
                Return Cells.RowCount
            End Get
            Set(value As Integer)
                If value >= 1 Then
                    Cells.SetDimensions(value, Cells.ColumnCount)
                End If
            End Set
        End Property
        Friend Property Settings As TTABLEENTRY
            Get
                Dim _rVal As New TTABLEENTRY(dxxSettingTypes.TABLESETTINGS)
                _rVal.Props.CopyByName(Properties)
                Return _rVal
            End Get
            Set(value As TTABLEENTRY)
                If value.EntryType <> dxxSettingTypes.TABLESETTINGS Then Return
                Dim myProps As New TPROPERTIES(Properties)
                Dim dirty As Boolean
                myProps.CopyByName(value.Props, False, False, "", rChanged:=dirty)
                If dirty Then IsDirty = True
                SetProps(myProps)
            End Set
        End Property
        Public Property TitleAlignment As dxxHorizontalJustifications
            '^controls the horizontal text aligment  of the title text
            Get
                Return PropValueI("TitleAlignment")
            End Get
            Set(value As dxxHorizontalJustifications)
                If value >= 0 And value <= 3 Then SetPropVal("TitleAlignment", value, True)
            End Set
        End Property
        Public Property FooterAlignment As dxxHorizontalJustifications
            '^controls the horizontal text aligment of the footer text
            Get
                Return PropValueI("FooterAlignment")
            End Get
            Set(value As dxxHorizontalJustifications)
                If value >= 0 And value <= 3 Then SetPropVal("FooterAlignment", value, True)
            End Set
        End Property
        Public Property GridStyle As dxxTableGridStyles
            '^controls how horizontal and vertical grid lines are displayed in the table
            Get
                Return PropValueI("GridStyle")
            End Get
            Set(value As dxxTableGridStyles)
                If value >= 0 And value <= 3 Then
                    SetPropVal("GridStyle", value, True)
                End If
            End Set
        End Property
        Public Property RotationAngle As Double
            '^the rotation to apply to table
            Get
                Return PropValueD("RotationAngle")
            End Get
            Set(value As Double)
                SetPropVal("RotationAngle", TVALUES.NormAng(value, ThreeSixtyEqZero:=True), True)
            End Set
        End Property
        Public Property FeatureScale As Double
            '^the scale factor to apply to the entities of the table when they are created
            Get
                Return PropValueD("FeatureScale", aDefault:=1)
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("FeatureScale", value, True)
            End Set
        End Property
        Public Property BorderLineThickness As Double
            '^the default thickness for table border lines
            '~scaled by the current feature scale when applied
            Get
                Return PropValueD("BorderLineThickness")
            End Get
            Set(value As Double)
                If value >= 0 Then SetPropVal("BorderLineThickness", value, True)
            End Set
        End Property
        Public Property GridLineThickness As Double
            '^the default thickness for table grid lines
            '~scaled by the current feature scale when applied
            Get
                Return PropValueD("GridLineThickness", aDefault:=1)
            End Get
            Set(value As Double)
                If value >= 0 Then SetPropVal("GridLineThickness", value, True)
            End Set
        End Property
        Public Property TextColor As dxxColors
            '^the color to apply to grid text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return PropValueI("TextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Then SetPropVal("TextColor", value, True)
            End Set
        End Property
        Public Property HeaderTextColor As dxxColors
            '^the color to apply to header text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return PropValueI("HeaderTextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Or value = dxxColors.Undefined Then SetPropVal("HeaderTextColor", value, True)
            End Set
        End Property
        Public Property TitleTextColor As dxxColors
            '^the color to apply to title text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return PropValueI("TitleTextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Or value = dxxColors.Undefined Then SetPropVal("TitleTextColor", value, True)
            End Set
        End Property
        Public Property FooterTextColor As dxxColors
            '^the color to apply to footer text
            '~-1 (dxxColors.Undefined) means the grid text color is applied
            Get
                Return PropValueI("FooterTextColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Or value = dxxColors.Undefined Then SetPropVal("FooterTextColor", value, True)
            End Set
        End Property
        Public Property TextGap As Double
            '^the gap around text in the table as a fraction of the width of a single character
            '~0 to 6 default = 0.5. Scaled by the current feature scale
            Get
                Return PropValueD("TextGap")
            End Get
            Set(value As Double)
                value = TVALUES.LimitedValue(value, 0, 6)
                SetPropVal("TextGap", value, True)
            End Set
        End Property
        Public Property ColumnGap As Double
            '^a length to add to the column cells to stretch the table lengthwise
            Get
                Return PropValueD("ColumnGap")
            End Get
            Set(value As Double)
                If value < 0 Then value = 0
                SetPropVal("ColumnGap", value, True)
            End Set
        End Property
        Public Property RowGap As Double
            '^a length to add to the row heights to stretch the table height wise
            Get
                Return PropValueD("RowGap")
            End Get
            Set(value As Double)
                If value < 0 Then value = 0
                SetPropVal("RowGap", value, True)
            End Set
        End Property
        Public Property GridColor As dxxColors
            '^the color to apply to the grid lines
            Get
                Return PropValueI("GridColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Then SetPropVal("GridColor", value, True)
            End Set
        End Property
        Public Property BorderColor As dxxColors
            '^the color to apply to the border lines
            Get
                Return PropValueI("BorderColor")
            End Get
            Set(value As dxxColors)
                If dxfColors.ColorIsReal(value) Then SetPropVal("BorderColor", value, True)
            End Set
        End Property
        Public Property ShowColumnLines As Boolean
            '^True if the column seperator lines should be drawn
            Get
                Return GridStyle = dxxTableGridStyles.All Or GridStyle = dxxTableGridStyles.ColumnLines
            End Get
            Set(value As Boolean)
                If value Then
                    If GridStyle = dxxTableGridStyles.RowLines Then GridStyle = dxxTableGridStyles.All Else GridStyle = dxxTableGridStyles.ColumnLines
                Else
                    If GridStyle = dxxTableGridStyles.All Then GridStyle = dxxTableGridStyles.RowLines Else GridStyle = dxxTableGridStyles.None
                End If
            End Set
        End Property
        Public Property ShowRowLines As Boolean
            '^True if the row seperator lines should be drawn
            Get
                Return GridStyle = dxxTableGridStyles.All Or GridStyle = dxxTableGridStyles.RowLines
            End Get
            Set(value As Boolean)
                If value Then
                    If GridStyle = dxxTableGridStyles.ColumnLines Then GridStyle = dxxTableGridStyles.All Else GridStyle = dxxTableGridStyles.RowLines
                Else
                    If GridStyle = dxxTableGridStyles.All Then GridStyle = dxxTableGridStyles.ColumnLines Else GridStyle = dxxTableGridStyles.None
                End If
            End Set
        End Property

        Public Property Title As String
            '^the title text
            Get
                Return PropValueStr("Title")
            End Get
            Set(value As String)
                SetPropVal("Title", value, True)
            End Set
        End Property
        Public Property TextSize As Double
            '^the text height for the grid text
            '~Default = 0.2
            '~the current feature scale is applied
            Get
                Return PropValueD("TextSize")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("TextSize", value, True)
            End Set
        End Property
        Public Property HeaderTextScale As Double
            '^used to set the current table header text scale
            '~table header text size is equal to the grid text size multiplied by this factor.
            '~default = 1.
            Get
                Return PropValueD("HeaderTextScale")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("HeaderTextScale", value, True)
            End Set
        End Property
        Public ReadOnly Property HeaderTextSize As Double
            '^returns the current table header text size
            '~table header text size is equal to the grid text size multiplied HeaderTextScale.
            Get
                Return HeaderTextScale * TextSize
            End Get
        End Property
        Public Property FooterTextScale As Double
            '^used to set the current table footer text scale
            '~table footer text size is equal to the grid text size multiplied by this factor.
            '~default = 1.
            Get
                Return PropValueD("FooterTextScale")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("FooterTextScale", value, True)
            End Set
        End Property
        Public ReadOnly Property FooterTextSize As Double
            '^returns the current table footer text size
            '~table footer text size is equal to the grid text size multiplied FooterTextScale.
            Get
                Return FooterTextScale * TextSize
            End Get
        End Property
        Public Property TitleTextScale As Double
            '^used to set the current table title text scale
            '~table title text size is equal to the grid text size multiplied by this factor.
            '~default = 1.
            Get
                Return PropValueD("TitleTextScale")
            End Get
            Set(value As Double)
                If value > 0 Then SetPropVal("TitleTextScale", value, True)
            End Set
        End Property
        Public ReadOnly Property TitleTextSize As Double
            '^returns the current table title text size
            '~table title text size is equal to the grid text size multiplied TitleTextScale.
            Get
                Return TitleTextScale * TextSize
            End Get
        End Property
        Public Property TextWidthFactor As Double
            '^the width factor to apply to grid text
            '~0 is default and means to use the current width factor of the grid text style
            Get
                Return PropValueD("TextWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("TextWidthFactor", value, True)
            End Set
        End Property
        Public Property HeaderWidthFactor As Double
            '^the width factor to apply to header text
            '~0 is default and means to use the current width factor of the grid text style
            Get
                Return PropValueD("HeaderWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("HeaderWidthFactor", value, True)
            End Set
        End Property
        Public Property TitleWidthFactor As Double
            '^the width factor to apply to title text
            '~0 is default and means to use the current width factor of the title text style
            Get
                Return PropValueD("TitleWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("TitleWidthFactor", value, True)
            End Set
        End Property
        Public Property FooterWidthFactor As Double
            '^the width factor to apply to footer text
            '~0 is default and means to use the current width factor of the grid text style
            Get
                Return PropValueD("FooterWidthFactor")
            End Get
            Set(value As Double)
                If value <> 0 Then value = TVALUES.LimitedValue(Math.Abs(value), 0.1, 100)
                SetPropVal("FooterWidthFactor", value, True)
            End Set
        End Property
        Public Property SaveAsBlock As Boolean
            '^a flag to save the table as a block
            '~if False the individual elements of the table will be added to the output file
            Get
                Return PropValueStr("SaveAsBlock")
            End Get
            Set(value As Boolean)
                SetPropVal("SaveAsBlock", value, True)
            End Set
        End Property
        Public Property SaveAttributes As Boolean
            '^controls how the table is saved to file
            '~if true and the table is saved as a block then the cell text is
            '~saved as attributes
            Get
                Return PropValueB("SaveAttributes")
            End Get
            Set(value As Boolean)
                SetPropVal("SaveAttributes", value, True)
            End Set
        End Property
        Public Property SuppressBorder As Boolean
            '^a flag control borders
            Get
                Return PropValueStr("SuppressBorder")
            End Get
            Set(value As Boolean)
                SetPropVal("SuppressBorder", value, True)
            End Set
        End Property
        Public Shadows ReadOnly Property Width As Double
            '^the width of the table
            Get
                Return BoundingRectangle.Width
            End Get
        End Property
        Public Property BlockName As String
            Get
                Dim _rVal As String = PropValueStr("BlockName")
                If String.IsNullOrEmpty(_rVal) Then _rVal = Name
                If String.IsNullOrEmpty(_rVal) Then _rVal = GUID
                Return _rVal
            End Get
            Set(value As String)
                Name = value
            End Set
        End Property
#End Region 'Properties
        Public Overrides Property Name As String
            Get
                Dim _rVal As String = MyBase.Name
                If String.IsNullOrEmpty(_rVal) Then _rVal = PropValueStr("BlockName")
                If String.IsNullOrEmpty(_rVal) Then _rVal = Identifier
                If String.IsNullOrEmpty(_rVal) Then _rVal = Tag
                If String.IsNullOrEmpty(_rVal) Then _rVal = GUID
                Return _rVal
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                MyBase.Name = value
                SetPropVal("*Name", value, False)
                SetPropVal("BlockName", value)
            End Set
        End Property
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Throw New Exception("dxeTables cannot be defined by Object")
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeTable
            Dim _rVal As dxeTable = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeTable
            Return New dxeTable(Me)
        End Function

        Public Overrides Property IsDirty As Boolean
            Get
                Return MyBase.IsDirty Or Cells.IsDirty
            End Get
            Friend Set(value As Boolean)
                Cells.IsDirty = value
                MyBase.IsDirty = value
            End Set
        End Property
        Public ReadOnly Property SubEntities As colDXFEntities
            Get
                UpdatePath()
                Return New colDXFEntities(PathEntities, aImageGUID:=ImageGUID, bIncludeInstances:=False, aSuppressedValue:=False)

            End Get
        End Property
#End Region 'MustOverride Entity Methods
#Region "Methods"
        Public Sub CopySettings(aSettings As dxoSettingsTable)
            If aSettings Is Nothing Then Return
            If Settings.Props.CopyByName(aSettings.Strukture.Props).Count > 0 Then IsDirty = True
        End Sub
        Public Function ConvertToBlock() As dxfBlock
            Return New dxfBlock(BlockName, Domain, Plane, SubEntities, InsertionPt) With {.ImageGUID = ImageGUID}
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            If PathEntities.Count <= 0 Then Return _rVal
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            Dim gname As String
            Dim svBlk As Boolean
            Dim sEnt As dxfEntity
            Dim aTxt As dxeText
            Dim bname As String
            Dim aBlk As dxfBlock
            Dim aIns As dxeInsert
            Dim bAtts As Boolean
            Dim v1 As TVECTOR
            Dim iGUID As String
            Dim bEnt As dxfEntity
            Dim attVals As New TPROPERTIES("Attributes")
            Dim aSubEnts As colDXFEntities
            iGUID = aImage.GUID
            svBlk = SaveAsBlock
            gname = GroupName
            aSubEnts = Entities
            If Not svBlk Then
                aInstances.IsParent = True
                aInstances.Plane = PlaneV
                For i = 1 To iCnt
                    If aInstance <= 0 Or i = aInstance Then
                        For j As Integer = 1 To aSubEnts.Count
                            _rVal.Append(aSubEnts.Item(j).DXFFileProperties(aInstances, aImage, Nothing, aInstance:=i), True)
                        Next j
                    End If
                Next i
                'include my instances
                _rVal.Name = "TABLE"
                If iCnt > 1 Then
                    _rVal.Name += $"-{ iCnt } INSTANCES"
                End If
            Else
                bname = Name
                If bname = "" Then bname = aImage.HandleGenerator.NextTableName(aImage.Blocks)
                bAtts = SaveAttributes
                aBlk = New dxfBlock(bname) With {.ImageGUID = iGUID, .LayerName = LayerName}
                v1 = (InsertionPtV * -1)
                aIns = New dxeInsert With {.ImageGUID = iGUID}
                attVals = aIns.AttributesVals
                For j = 1 To aSubEnts.Count
                    sEnt = aSubEnts.Item(j)
                    bEnt = sEnt
                    bEnt.GroupName = ""
                    bEnt.LayerName = LayerName
                    bEnt.Move(v1.X, v1.Y, v1.Z)
                    If sEnt.GraphicType = dxxGraphicTypes.Text Then
                        aTxt = DirectCast(bEnt, dxeText)
                        If bAtts Then
                            aTxt = aTxt.Clone(Nothing, aNewTextType:=dxxTextTypes.AttDef)
                            aIns.Attributes.Add(New dxfAttribute(aTxt))
                            bEnt = aTxt
                        End If
                    End If
                    aBlk.Entities.AddToCollection(bEnt, bSuppressEvnts:=True)
                Next j
                rBlock = aBlk
                aIns.InsertionPtV = InsertionPtV
                aIns.IsDirty = True
                aIns.GroupName = gname
                aIns.Block = aBlk
                aIns.SetAttributes(attVals)
                _rVal = aIns.DXFFileProperties(aInstances, aImage, Nothing, bSuppressInstances, False)
                _rVal.Name = $"TABLE_INSERT({ aBlk.Name })"
                If iCnt > 1 Then
                    _rVal.Name += $"-{ iCnt } INSTANCES"
                End If
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            If aInstances Is Nothing Then
                aInstances = Instances
                aInstances.IsParent = True
                aInstances.ParentPlane = PlaneV
            End If
            GetImage(aImage)
            Return New TPROPERTYARRAY(Entities.DXFProps(aInstances, aInstance, New dxfPlane(aOCS), rTypeName, aImage))
        End Function
        Private Sub Resize()
            IsDirty = True
            Dim iR As Integer
            Dim iC As Integer
            '**UNUSED VAR** Dim i As Long
            iR = RowCount
            iC = ColumnCount
            If iR < 1 Then iR = 1
            If iC < 1 Then iC = 1
            Cells.SetDimensions(iR, iC)
            RowCount = Cells.RowCount
            ColumnCount = Cells.ColumnCount
        End Sub
        Public Sub SetCellAlignments(aNumCol As List(Of String), Optional aDelimiter As String = "|", Optional bApplyLastEntryToRemainingRows As Boolean = True)
            '#1a collection of row cell Alignments like '1|5|3' or 'MiddleCenter|TopLeft|BottomRight'
            '#2the delimeter to seperate the column data
            '#3flag to use the last string in the passed array to apply to all the rows at an index greater than the count of the passed array
            '^uses the strings in the passed text list to set the cell Alignments
            '~the passed delimiter seperates the columns in each string
            If aNumCol Is Nothing Then Return
            If aNumCol.Count < 1 Then Return
            If String.IsNullOrWhiteSpace(aDelimiter) Then aDelimiter = "|" Else aDelimiter = aDelimiter.Trim
            Dim aStr As String = String.Empty
            Dim sStr() As String
            Dim aInt As String
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            Dim aMem As String
            Dim algn As Integer
            Dim bChange As Boolean
            For i As Integer = 1 To Cells.RowCount
                aRow = Cells.Item(i)
                If i <= aNumCol.Count Then
                    If aNumCol.Item(i - 1) IsNot Nothing Then
                        aMem = aNumCol.Item(i - 1)
                    Else
                        aMem = ""
                    End If
                Else
                    If bApplyLastEntryToRemainingRows Then
                        aMem = aNumCol.Item(aNumCol.Count - 1)
                    Else
                        aMem = ""
                    End If
                End If
                If Not String.IsNullOrWhiteSpace(aMem) Then
                    aStr = aMem
                    sStr = aStr.Split(aDelimiter)
                    For j As Integer = 0 To sStr.Length - 1
                        If j + 1 > aRow.Cells.Count Then Exit For
                        aCell = aRow.Cell(j + 1)
                        aInt = Trim(sStr(j))
                        If TVALUES.IsNumber(aInt) Then
                            algn = TVALUES.To_INT(aInt)
                            If algn >= 1 And algn <= 9 Then
                                If aCell.Alignment <> algn Then bChange = True
                                aCell.Alignment = algn
                            End If
                        Else
                            algn = dxfUtils.AlignmentNameDecode(aInt)
                            If algn >= 1 And algn <= 9 Then
                                If aCell.Alignment <> algn Then bChange = True
                                aCell.Alignment = algn
                            End If
                        End If
                        aRow.SetCell(aCell, j + 1)
                    Next j
                End If
                Cells.SetRow(aRow, i)
            Next i
            If bChange Then Cells.IsDirty = True
        End Sub
        Public Sub SetCellData(aTextCol As List(Of String), Optional aDelimiter As String = "|", Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.General)
            '#1a collection of row data strings like 'sadsd|sasdffsdf|1212332'
            '#2the delimeter to seperate the column data
            '^uses the strings in the passed text collection to set the cell data
            '~the passed delimiter seperates the row information in each string
            If aTextCol Is Nothing Then Return
            If aTextCol.Count <= 0 Then Return
            If String.IsNullOrWhiteSpace(aDelimiter) Then aDelimiter = "|" Else aDelimiter = aDelimiter.Trim
            If aAlignment = dxxRectangularAlignments.General Then aAlignment = CellAlignment
            Cells.DefineByCollection(aTextCol, aDelimiter, aAlignment)


        End Sub
        Public Sub SetCellNames(aTextCol As List(Of String), Optional aDelimiter As String = "|")
            '#1a collection of row data strings like 'sadsd|sasdffsdf|1212332'
            '#2the delimeter to seperate the column data
            '^uses the strings in the passed text collection to set the cell names
            '~the passed delimiter seperates the row information in each string
            Cells.SetCellNames(aTextCol, aDelimiter)
        End Sub
        Public Sub SetCellPrompts(aTextCol As List(Of String), Optional aDelimiter As String = "|")
            '#1a collection of row data strings like 'sadsd|sasdffsdf|1212332'
            '#2the delimeter to seperate the column data
            '^uses the strings in the passed text collection to set the cell names
            '~the passed delimiter seperates the row information in each string
            Cells.SetCellPrompts(aTextCol, aDelimiter)
        End Sub
        Public Sub SetDimensions(aRowCount As Integer, aColCount As Integer)
            Dim bChng As Boolean
            If aRowCount >= 1 Then
                If SetPropVal("RowCount", aRowCount, False) Then bChng = True
            End If
            If aColCount >= 1 Then
                If SetPropVal("ColCount", aColCount, False) Then bChng = True
            End If
            If bChng Then Resize()
        End Sub
        Friend Function xGetTableRows(ByRef iHeaderRow As Integer, ByRef iHeaderCol As Integer, bRowLines As Boolean, bColLines As Boolean, bSuppressBorders As Boolean, ByRef rFillColors As Boolean, aGridLineThickness As Double, aBorderLineThickness As Double) As TTABLEROWS
            rFillColors = False
            Dim _rVal As TTABLEROWS = Cells.Strukture
            If _rVal.Count <= 0 Then Return _rVal
            Dim iR As Integer
            Dim iC As Integer
            Dim aCells As TTABLECELLS
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            Dim aPlane As TPLANE = PlaneV
            Dim SF As Double = FeatureScale
            Dim ang As Double = RotationAngle
            Dim bPlane As TPLANE = aPlane.Clone
            If ang <> 0 Then bPlane.Revolve(ang, False)
            Try
                _rVal.Angle = ang
                _rVal.Attributes = SaveAttributes
                _rVal.BasePt = InsertionPtV
                _rVal.Plane = aPlane
                If iHeaderRow > 0 And iHeaderRow <= _rVal.Count Then
                    _rVal.HeaderRow = iHeaderRow
                Else
                    _rVal.HeaderRow = -100
                End If
                If iHeaderCol > 0 And iHeaderCol <= Cells.MaxColumns Then
                    _rVal.HeaderCol = iHeaderCol
                Else
                    _rVal.HeaderCol = -100
                End If
                _rVal.GridThickness = aGridLineThickness
                _rVal.BorderThickness = aBorderLineThickness
                For iR = 1 To _rVal.Count
                    aRow = _rVal.Row(iR)
                    aCells = aRow.Cells
                    aCells.Row = iR
                    For iC = 1 To aCells.Count
                        aCell = aCells.Cell(iC)
                        aCell.ClearSettings()
                        aCell.SetBorderThickness(_rVal.GridThickness, dxxBorderPointers.All)
                        aCell.Row = iR
                        aCell.Column = iC
                        If aCell.FillColor <> dxxColors.Undefined Then rFillColors = True
                        aCell.IsHeaderCell = (iR = iHeaderRow) Or (iC = iHeaderCol)
                        aCell.SetBorderToggle(False, dxxBorderPointers.All)
                        If Not bRowLines Then
                            aCell.SetBorderThickness(0, dxxBorderPointers.Bottom)
                            aCell.SetBorderThickness(0, dxxBorderPointers.Top)
                        End If
                        If Not bColLines Then
                            aCell.SetBorderThickness(0, dxxBorderPointers.Left)
                            aCell.SetBorderThickness(0, dxxBorderPointers.Right)
                        End If
                        If iR = 1 Then aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Top)
                        If iR = _rVal.Count Then aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Bottom)
                        If iC = 1 Then aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Left)
                        If iC = aCells.Count Then aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Right)
                        'set bottom and right toggles
                        If bRowLines And iR < _rVal.Count Then
                            If (iR <> _rVal.HeaderRow - 1 And iR <> _rVal.HeaderRow) Or bSuppressBorders Then
                                aCell.SetBorderToggle(True, dxxBorderPointers.Bottom)
                            End If
                        End If
                        If bColLines And iC < aCells.Count Then
                            If (iC <> _rVal.HeaderCol - 1 And iC <> _rVal.HeaderCol) Or bSuppressBorders Then
                                aCell.SetBorderToggle(True, dxxBorderPointers.Right)
                            End If
                        End If
                        If iR = _rVal.HeaderRow Then
                            aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Bottom)
                            If iR > 1 Then aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Top)
                        End If
                        If iR = _rVal.HeaderRow - 1 Then
                            aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Bottom)
                        End If
                        If iR = _rVal.HeaderRow + 1 Then
                            aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Top)
                        End If
                        If iC = _rVal.HeaderCol Then
                            aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Left)
                            If iC < aCells.Count Then aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Right)
                        End If
                        If iC = _rVal.HeaderCol - 1 Then
                            aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Right)
                        End If
                        If iC = _rVal.HeaderCol + 1 Then
                            aCell.SetBorderThickness(_rVal.BorderThickness, dxxBorderPointers.Left)
                        End If
                        aCell.Plane = bPlane
                        aCells.SetCell(aCell, iC)
                        aRow.Cells = aCells
                        'Application.DoEvents()
                    Next iC
                    _rVal.SetRow(aRow, iR)
                    'Application.DoEvents()
                Next iR
                Dim setngs As New TPROPERTIES(Properties)
                'set the text heights
                _rVal.ClearSettings()
                _rVal.SetTextSizes(TextSize * SF, HeaderTextSize * SF, TitleTextSize * SF, FooterTextSize * SF)
                'set the stylenames
                _rVal.SetTextStyles(TextStyleName, HeaderTextStyle, TitleTextStyle, FooterTextStyle)
                setngs.SetVal("TextStyleName", _rVal.TexStyle(dxxCellTextTypes.Field))
                If String.Compare(_rVal.TexStyle(dxxCellTextTypes.Field), _rVal.TexStyle(dxxCellTextTypes.Header)) = 0 Then setngs.SetVal("HeaderTextStyle", "")
                If String.Compare(_rVal.TexStyle(dxxCellTextTypes.Field), _rVal.TexStyle(dxxCellTextTypes.Title)) = 0 Then setngs.SetVal("TitleTextStyle", "")
                If String.Compare(_rVal.TexStyle(dxxCellTextTypes.Field), _rVal.TexStyle(dxxCellTextTypes.Footer)) = 0 Then setngs.SetVal("FooterTextStyle", "")
                'set the width factors
                _rVal.SetWidthFactors(TextWidthFactor, HeaderWidthFactor, TitleWidthFactor, FooterWidthFactor)
                _rVal.SetTextColors(TextColor, HeaderTextColor, TitleTextColor, FooterTextColor)
                SetProps(setngs)
                Cells.Strukture = _rVal
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Friend Function xCreateCellText(aImage As dxfImage, aLayerName As String, aRows As TTABLEROWS, aGap As Double, aColumnGap As Double, aRowGap As Double) As List(Of dxeText)
            Dim _rVal As New List(Of dxeText)
            'create the table text
            If aRows.Count <= 0 Then Return _rVal


            Dim i As dxxCellTextTypes
            Dim awf As Double
            Dim tht As Double
            Dim styname As String
            Dim tClr As dxxColors
            Dim aStr As String = String.Empty
            Dim mTxt As dxeText
            Dim cRect As TPLANE
            Dim bPlane As TPLANE
            Dim d1 As Double
            Dim maxCol As Integer
            Try
                'create the text and set the cell sizes
                For iR As Integer = 1 To aRows.Count
                    Dim aRow As TTABLEROW = aRows.Row(iR)
                    For iC As Integer = 1 To aRow.Cells.Count
                        Dim aCell As TTABLECELL = aRow.Cell(iC)
                        aCell.Column = iC
                        aCell.Row = iR
                        If aCell.IsHeaderCell Then i = dxxCellTextTypes.Header Else i = dxxCellTextTypes.Field
                        tht = aRows.TextSize(i)
                        styname = aRows.TexStyle(i)
                        tClr = aRows.TextColor(i)
                        awf = aRows.WidthFactor(i)
                        aCell.IsAttribute = aRows.Attributes
                        If aCell.IsHeaderCell Then aCell.IsAttribute = False
                        If aCell.TextScale > 0 Then tht *= aCell.TextScale
                        If aCell.WidthFactor > 0.01 Then awf = aCell.WidthFactor
                        If aCell.StyleName <> "" Then
                            If String.Compare(aCell.StyleName, aRows.TexStyle(i), ignoreCase:=True) = 0 Then aCell.StyleName = "" Else styname = aCell.StyleName
                        End If
                        If aCell.TextColor <> dxxColors.Undefined Then
                            If aCell.TextColor = aRows.TextColor(i) Then
                                aCell.TextColor = dxxColors.Undefined
                                tClr = aRows.TextColor(i)
                            Else
                                tClr = aCell.TextColor
                            End If
                        End If
                        Dim mtxtalgn As dxxMTextAlignments = aCell.MTextAlignment
                        'tAlgn = dxxMTextAlignments.MiddleCenter
                        'Select Case aCell.Alignment
                        '    Case dxxRectangularAlignments.MiddleLeft, dxxRectangularAlignments.TopLeft, dxxRectangularAlignments.BottomLeft
                        '        tAlgn = dxxMTextAlignments.MiddleLeft
                        '    Case dxxRectangularAlignments.MiddleRight, dxxRectangularAlignments.TopRight, dxxRectangularAlignments.BottomRight
                        '        tAlgn = dxxMTextAlignments.MiddleRight
                        'End Select
                        aStr = aCell.StringData
                        '                If .IsAttribute And Trim(aStr) = "" Then aStr = Replace(.Name, ",", "_")
                        If aStr Is Nothing Then aStr = ""
                        If aStr = "" Then aStr = "i"
                        bPlane = aRows.Plane.Clone
                        If aRows.Angle <> 0 Then bPlane.Revolve(aRows.Angle)
                        If aCell.TextAngle Then bPlane.Revolve(aCell.TextAngle)
                        mTxt = xTableText(aImage, bPlane, aStr, tht, mtxtalgn, aLayerName, styname, tClr, awf, 0, dxxTextTypes.Multiline)
                        mTxt.Suppressed = String.IsNullOrWhiteSpace(aCell.StringData)
                        mTxt.Prompt = aCell.Prompt
                        mTxt.Identifier = aCell.ToString
                        If aCell.Name = "" Then
                            mTxt.AttributeTag = $"CELL_{ aCell.Row }_{ aCell.Column}"
                        Else
                            mTxt.AttributeTag = aCell.Name
                        End If
                        'create the boundary rectangle
                        mTxt.UpdatePath(True, aImage)
                        cRect = mTxt.BoundingRectangle(aRows.Plane)
                        'add in use requested gaps
                        cRect.Resize((2 * aColumnGap) + (2 * aGap), (2 * aRowGap) + (2 * aGap))
                        'apply user forced minumum widths and heights
                        cRect.Width = Math.Max(aCell.Width, cRect.Width)
                        cRect.Height = Math.Max(aCell.Height, cRect.Height)
                        'increase width for borders
                        cRect.Width += 0.5 * aCell.BorderThickness(dxxBorderPointers.Left) + 0.5 * aCell.BorderThickness(dxxBorderPointers.Right)
                        If iC = 1 Then cRect.Width += 0.5 * aCell.BorderThickness(dxxBorderPointers.Left)
                        If iC = aRow.Cells.Count Then cRect.Width += 0.5 * aCell.BorderThickness(dxxBorderPointers.Right)
                        'increase height for borders
                        cRect.Height += 0.5 * aCell.BorderThickness(dxxBorderPointers.Bottom) + 0.5 * aCell.BorderThickness(dxxBorderPointers.Top)
                        If iR = 1 Then cRect.Height += 0.5 * aCell.BorderThickness(dxxBorderPointers.Top)
                        If iR = aRows.Count Then cRect.Height += 0.5 * aCell.BorderThickness(dxxBorderPointers.Bottom)
                        _rVal.Add(mTxt)
                        aCell.Height = cRect.Height
                        aCell.Width = cRect.Width
                        mTxt.Row = iR
                        mTxt.Col = iC
                        mTxt.Name = aCell.Name
                        'save the previously created cells
                        aRow.SetCell(aCell, iC)
                    Next iC
                    'save the previously created rows
                    aRows.SetRow(aRow, iR)
                Next iR
                maxCol = 0
                For iR As Integer = 1 To aRows.Count
                    Dim aRow As TTABLEROW = aRows.Row(iR)
                    'get the max cell heights
                    If aRow.CellCount > maxCol Then maxCol = aRow.CellCount
                    aRow.CellHeight = aRow.MaxCellHeight
                    aRows.SetRow(aRow, iR)
                Next iR
                For iC As Integer = 1 To maxCol
                    d1 = aRows.MaxCellWidth(iC)
                    aRows.SetCellWidth(iC, d1)
                Next iC
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Private Function xTableText(aImage As dxfImage, aPlane As TPLANE, aString As Object, aTextHeight As Double, aAlignment As dxxMTextAlignments, aLayer As String, aStyleName As String, aColor As dxxColors, aWidthFactor As Double, aTextAngle As Double, aTextType As dxxTextTypes) As dxeText
            Dim _rVal As dxeText = Nothing
            '#1the image for the text
            '#2 the plane upon which the text should be placed
            '#3a text height to apply which overrides the current dxoDrawingTool.TextHeight value
            '#4a text alignment to apply which overrides the current dxoDrawingTool.Alignment value
            '#5the string to create a text object for
            '#6a layer to put the text on
            '#7a text style to use
            '#8flag to indicate if the text should be drawn to the screen or just added to the aImage file.
            '#9a color to use rather that the current color
            '#10a width factor to use
            '^used to create text for the current table
            '~creates and returns dxeText object.
            Dim tStr As String
            Dim tht As Double
            Dim tang As Double
            Dim wf As Double
            Dim tStyl As New dxoStyle
            If aTextType < dxxTextTypes.DText Or aTextType > dxxTextTypes.Multiline Then aTextType = dxxTextTypes.Multiline
            'text style
            tStyl = aImage.Styles.Member(aStyleName)
            If tStyl Is Nothing Then
                aStyleName = aStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aImage.TableSettings.TextStyleName, rEntry:=tStyl)
            End If
            tStr = aString.ToString()
            'text height
            tht = aTextHeight
            'angle
            tang = TVALUES.NormAng(aTextAngle, ThreeSixtyEqZero:=True)
            'width factor
            wf = aWidthFactor
            If wf <= 0 Or aTextType = dxxTextTypes.Multiline Then wf = tStyl.PropValueD(dxxStyleProperties.WIDTHFACTOR)
            'alignment
            If aAlignment < 0 Or aAlignment > 13 Then aAlignment = dxxMTextAlignments.BaselineLeft
            'initialize the dtext object
            _rVal = aImage.CreateText(tStyl, tStr, aTextType, dxxTextDrawingDirections.Horizontal, aTextHeight:=tht, aAlignment:=aAlignment, aAngle:=tang, aPlane:=aPlane.ToPlane, aInsertPt:=New dxfVector(aPlane.Origin))
            aStyleName = tStyl.Name
            _rVal.DisplayStructure = dxfImageTool.DisplayStructure_Text(aImage, aLayer, aColor, aStyleName)
            _rVal.Backwards = False
            _rVal.UpsideDown = False
            Return _rVal
        End Function
        Friend Function xAlignTableCells(aImage As dxfImage, aRows As TTABLEROWS, aXDir As TVECTOR, aYDir As TVECTOR, aTexts As List(Of dxeText), aTextGap As Double, ByRef rAccents As List(Of dxfEntity)) As TTABLEROWS
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            Dim aDir As TVECTOR
            Dim bpt As TVECTOR
            Dim d1 As Double
            Dim d2 As Double
            Dim mTxt As dxeText
            Dim txtRec As TPLANE
            Dim cellRec As TPLANE
            If rAccents Is Nothing Then rAccents = New List(Of dxfEntity)
            bpt = aRows.BasePt
            'align the cells and tet vertically
            For iR As Integer = 1 To aRows.Count
                aRow = aRows.Row(iR)
                For iC As Integer = 1 To aRow.Cells.Count
                    If iC = 1 Then
                        v2 = bpt + aYDir * -(d1 + 0.5 * aRow.FirstCell.Height)
                    End If
                    aCell = aRow.Cell(iC)
                    v2 += aXDir * (0.5 * aCell.Width)
                    aCell.Plane.Origin = v2
                    mTxt = aTexts.Find(Function(mem) mem.Identifier = aCell.ToString)
                    'center then text in the cell
                    If mTxt IsNot Nothing Then
                        If Not mTxt.Suppressed Then
                            mTxt.UpdatePath(False, aImage:=aImage)
                            txtRec = mTxt.BoundingRectangle(aRows.Plane)
                            aDir = txtRec.Origin.DirectionTo(v2, False, rDistance:=d2)
                            mTxt.Translate(aDir * d2)
                        End If
                    End If
                    v2 += aXDir * (0.5 * aCell.Width)
                    aRow.SetCell(aCell, iC)
                Next iC
                d1 += aRow.FirstCell.Height
                aRows.SetRow(aRow, iR)
            Next iR
            'align text horizontally
            For iR As Integer = 1 To aRows.Count
                aRow = aRows.Row(iR)
                For iC As Integer = 1 To aRow.Cells.Count
                    Dim r As Integer = iR
                    Dim c As Integer = iC
                    aCell = aRow.Cell(iC)
                    mTxt = aTexts.Find(Function(mem) mem.Row = r And mem.Col = c)
                    aCell.TextRectangle = aCell.Plane
                    d1 = 0.5 * aCell.BorderThickness(dxxBorderPointers.Top)
                    If iR = 1 Then d1 = 2 * d1
                    aCell.TextRectangle.Height -= d1
                    aCell.TextRectangle.Origin += aYDir * (-0.5 * d1)
                    d1 = 0.5 * aCell.BorderThickness(dxxBorderPointers.Bottom)
                    If iR = aRows.Count Then d1 = 2 * d1
                    aCell.TextRectangle.Height -= d1
                    aCell.TextRectangle.Origin += aYDir * (0.5 * d1)
                    d1 = 0.5 * aCell.BorderThickness(dxxBorderPointers.Left)
                    If iC = 1 Then d1 = 2 * d1
                    aCell.TextRectangle.Width -= d1
                    aCell.TextRectangle.Origin += aXDir * (0.5 * d1)
                    d1 = 0.5 * aCell.BorderThickness(dxxBorderPointers.Right)
                    If iC = aRow.Cells.Count Then d1 = 2 * d1
                    aCell.TextRectangle.Width -= d1
                    aCell.TextRectangle.Origin += aXDir * (-0.5 * d1)
                    cellRec = aCell.TextRectangle.Clone
                    If mTxt IsNot Nothing Then
                        If Not mTxt.Suppressed Then
                            Dim mtxtalgn As dxxMTextAlignments = aCell.MTextAlignment
                            Dim halgn As dxxHorizontalJustifications = dxxHorizontalJustifications.Center
                            Dim valgn As dxxVerticalJustifications = dxxVerticalJustifications.Center
                            If mTxt.Alignment <> mtxtalgn Then
                                mTxt.Alignment = mtxtalgn
                            End If
                            mTxt.UpdatePath(True, aImage)
                            txtRec = mTxt.BoundingRectangle(aRows.Plane)
                            If aTextGap <> 0 Then
                                cellRec.Expand(-aTextGap, -aTextGap)
                            End If
                            v2 = cellRec.AlignmentPoint(aCell.Alignment, True, rHAlign:=halgn, rVAlign:=valgn)
                            v1 = txtRec.AlignmentPoint(aCell.Alignment)
                            'If halgn = dxxHorizontalJustifications.Left Then
                            '    'v1 += aXDir * (-0.5 * (txtRec.Width))
                            '    'v2 += aXDir * (-0.5 * aCell.TextRectangle.Width + aTextGap)
                            '    v2 += aXDir * aTextGap
                            'ElseIf halgn = dxxHorizontalJustifications.Right Then
                            '    'v1 += aXDir * (0.5 * (txtRec.Width))
                            '    'v2 += aXDir * (0.5 * aCell.TextRectangle.Width - aTextGap)
                            '    v2 += aXDir * -aTextGap
                            'End If
                            'If valgn = dxxVerticalJustifications.Top Then
                            '    'v1 += aYDir * (0.5 * (txtRec.Height))
                            '    'v2 += aYDir * (0.5 * aCell.TextRectangle.Height - aTextGap)
                            '    v2 += aYDir * -aTextGap
                            'ElseIf valgn = dxxVerticalJustifications.Bottom Then
                            '    'v1 += aYDir * (-0.5 * (txtRec.Height + aTextGap))
                            '    'v2 += aYDir * (-0.5 * aCell.TextRectangle.Height + aTextGap)
                            '    v2 += aYDir * aTextGap
                            'End If
                            'mTxt.Alignment = mtxtalgn
                            'mTxt.InsertionPt = New dxfVector(v2)
                            'mTxt.UpdatePath(True, aImage)
                            'txtRec = mTxt.Bounds
                            aDir = v1.DirectionTo(v2, False, rDistance:=d2)
                            d2 = Math.Round(d2, 6)
                            If (d2 <> 0) Then
                                mTxt.Translate(aDir * d2)
                                mTxt.UpdatePath(True, aImage)
                                txtRec.Origin += aDir * d2
                            End If
                            'rAccents.Add(New dxePolyline(New colDXFVectors(cellRec.Corners), True, New dxfDisplaySettings("", aColor:=dxxColors.Orange)))
                            'rAccents.Add(New dxePolyline(mTxt.BoundingRectangle().Corners, True, New dxfDisplaySettings("", aColor:=dxxColors.Red)))
                            'rAccents.Add(New dxeArc(New dxfVector(v1), 0.1 * mTxt.TextHeight, aDisplaySettings:=New dxfDisplaySettings("", aColor:=dxxColors.Red)))
                            'rAccents.Add(New dxeArc(New dxfVector(v2), 0.2 * mTxt.TextHeight, aDisplaySettings:=New dxfDisplaySettings("", aColor:=dxxColors.Orange)))
                        End If
                    End If
                    aRow.SetCell(aCell, iC)
                    'Application.DoEvents()
                Next iC
                aRows.SetRow(aRow, iR)
                'Application.DoEvents()
            Next iR
            Return aRows
        End Function
        Friend Function xGridLines(aRows As TTABLEROWS, aDisplayStructure As TDISPLAYVARS, aSegWidth As Double, aColor As dxxColors) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aRows.Count <= 0 Then Return _rVal
            Dim aPlane As TPLANE
            Dim aP As dxePolyline = Nothing
            Dim bP As dxePolyline = Nothing
            Dim ident As String
            Dim iR As Integer
            Dim iC As Integer
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim cnt As Integer = 0
            Dim iMaxCol As Integer
            Try
                bP = New dxePolyline With
                {
                    .Closed = False,
                    .DisplayStructure = aDisplayStructure,
                    .Color = aColor,
                    .SegmentWidth = aSegWidth,
                    .Value = 0
                }
                'first do the bottom row lines
                cnt = 0
                aP = Nothing
                For iR = 1 To aRows.Count
                    aRow = aRows.Row(iR)
                    aP = Nothing
                    For iC = 1 To aRow.Cells.Count
                        aCell = aRow.Cell(iC)
                        If iC > iMaxCol Then iMaxCol = iC
                        aPlane = aCell.Plane
                        ident = "Cell_" & aCell.Row & "_" & aCell.Column & "_"
                        If aCell.BorderIsVisible(dxxBorderPointers.Top) Then cnt += 1
                        If aCell.BorderIsVisible(dxxBorderPointers.Bottom) Then
                            v1 = aPlane.Point(dxxRectanglePts.BottomLeft)
                            v2 = aPlane.Point(dxxRectanglePts.BottomRight)
                            If aP IsNot Nothing Then
                                If Not v1.IsEqualTo(aP.LastVertex, 1) Then aP = Nothing
                            End If
                            If aP Is Nothing Then
                                aP = bP.Clone
                                aP.AddV(v1)
                                aP.AddV(v2)
                                aP.Tag = "Bottom"
                                aP.Flag = aCell.Row.ToString()
                                aP.Identifier = ident & aP.Tag
                                _rVal.Add(aP)
                            Else
                                aP.AddV(v2)
                            End If
                        End If
                    Next iC
                Next iR
                'do the top row lines
                If cnt > 0 Then
                    For iR = 1 To aRows.Count
                        aRow = aRows.Row(iR)
                        aP = Nothing
                        For iC = 1 To aRow.Cells.Count
                            aCell = aRow.Cell(iC)
                            aPlane = aCell.Plane
                            ident = "Cell_" & aCell.Row & "_" & aCell.Column & "_"
                            If aCell.BorderIsVisible(dxxBorderPointers.Top) Then
                                v1 = aPlane.Point(dxxRectanglePts.TopLeft)
                                v2 = aPlane.Point(dxxRectanglePts.TopRight)
                                If aP IsNot Nothing Then
                                    If Not v1.IsEqualTo(aP.LastVertex, 1) Then aP = Nothing
                                End If
                                If aP Is Nothing Then
                                    aP = bP.Clone
                                    aP.AddV(v1)
                                    aP.AddV(v2)
                                    aP.Tag = "Top"
                                    aP.Flag = aCell.Row.ToString()
                                    aP.Identifier = ident & aP.Tag
                                    _rVal.Add(aP)
                                Else
                                    aP.AddV(v2)
                                End If
                            End If
                        Next iC
                    Next iR
                End If
                'do the right lines
                iC = 1
                cnt = 0
                Do Until iC > iMaxCol
                    For iR = 1 To aRows.Count
                        aRow = aRows.Row(iR)
                        aCell = aRow.Cell(iC)
                        If iC > iMaxCol Then iMaxCol = iC
                        aPlane = aCell.Plane
                        ident = "Cell_" & aCell.Row & "_" & aCell.Column & "_"
                        If aCell.BorderIsVisible(dxxBorderPointers.Left) Then cnt += 1
                        If aCell.BorderIsVisible(dxxBorderPointers.Right) Then
                            v1 = aPlane.Point(dxxRectanglePts.TopRight)
                            v2 = aPlane.Point(dxxRectanglePts.BottomRight)
                            If aP IsNot Nothing Then
                                If Not v1.IsEqualTo(aP.LastVertex, 1) Then aP = Nothing
                            End If
                            If aP Is Nothing Then
                                aP = bP.Clone
                                aP.AddV(v1)
                                aP.AddV(v2)
                                aP.Tag = "Right"
                                aP.Flag = aCell.Row.ToString()
                                aP.Identifier = ident & aP.Tag
                                _rVal.Add(aP)
                            Else
                                aP.AddV(v2)
                            End If
                        End If
                    Next iR
                    iC += 1
                Loop
                'do the left lines
                If cnt > 0 Then
                    iC = 1
                    Do Until iC > iMaxCol
                        For iR = 1 To aRows.Count
                            aRow = aRows.Row(iR)
                            aCell = aRow.Cell(iC)
                            If iC > iMaxCol Then iMaxCol = iC
                            aPlane = aCell.Plane
                            ident = $"Cell_{aCell.Row }_{ aCell.Column }_"
                            If aCell.BorderIsVisible(dxxBorderPointers.Left) Then
                                v1 = aPlane.Point(dxxRectanglePts.TopLeft)
                                v2 = aPlane.Point(dxxRectanglePts.BottomLeft)
                                If aP IsNot Nothing Then
                                    If Not v1.IsEqualTo(aP.LastVertex, 1) Then aP = Nothing
                                End If
                                If aP Is Nothing Then
                                    aP = bP.Clone
                                    aP.AddV(v1)
                                    aP.AddV(v2)
                                    aP.Tag = "Left"
                                    aP.Flag = aCell.Row.ToString
                                    aP.Identifier = ident & aP.Tag
                                    _rVal.Add(aP)
                                Else
                                    aP.AddV(v2)
                                End If
                            End If
                        Next iR
                        iC += 1
                    Loop
                End If
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Friend Function xBorderLines(aRows As TTABLEROWS, aDisplayStructure As TDISPLAYVARS, aColor As dxxColors, aSegWidth As Double, bSuppressBorders As Boolean, aXDir As TVECTOR, aYDir As TVECTOR, ByRef rExtentPts As TVECTORS) As List(Of dxfEntity)
            'add the exterior border
            Dim _rVal As New List(Of dxfEntity)
            rExtentPts = New TVECTORS
            aXDir.Normalize()
            aYDir.Normalize()
            If aRows.Count <= 0 Then Return _rVal
            Try
                Dim aP As dxePolyline
                Dim aRow As TTABLEROW
                Dim bRow As TTABLEROW
                Dim aCell As TTABLECELL
                Dim bCell As TTABLECELL
                aRow = aRows.FirstRow
                bRow = aRows.LastRow
                rExtentPts.Add(aRow.FirstCell.Plane.Point(dxxRectanglePts.TopLeft))
                rExtentPts.Add(bRow.FirstCell.Plane.Point(dxxRectanglePts.BottomLeft))
                rExtentPts.Add(bRow.LastCell.Plane.Point(dxxRectanglePts.BottomRight))
                rExtentPts.Add(aRow.LastCell.Plane.Point(dxxRectanglePts.TopRight))
                aP = New dxePolyline With
                {
                    .PlaneV = aRows.Plane,
                    .Closed = True,
                    .DisplayStructure = aDisplayStructure,
                    .Color = aColor,
                    .SegmentWidth = aSegWidth,
                    .Identifier = "Exterior Border",
                    .Suppressed = bSuppressBorders
                }
                aP.AddV(rExtentPts.Item(1) + aXDir * (0.5 * aSegWidth) + aYDir * (-0.5 * aSegWidth))
                aP.AddV(rExtentPts.Item(2) + aXDir * (0.5 * aSegWidth) + aYDir * (0.5 * aSegWidth))
                aP.AddV(rExtentPts.Item(3) + aXDir * (-0.5 * aSegWidth) + aYDir * (0.5 * aSegWidth))
                aP.AddV(rExtentPts.Item(4) + aXDir * (-0.5 * aSegWidth) + aYDir * (-0.5 * aSegWidth))
                _rVal.Add(aP)
                If aRows.HeaderRow > 0 Then
                    aRow = aRows.Row(aRows.HeaderRow)
                    If aRows.HeaderRow > 1 Then
                        aP = New dxePolyline With
                        {
                            .PlaneV = aRows.Plane,
                            .Closed = False,
                            .DisplayStructure = aDisplayStructure,
                            .Color = aColor,
                            .SegmentWidth = aSegWidth,
                            .Identifier = "Exterior Border",
                            .Suppressed = bSuppressBorders
                        }
                        aP.AddV(aRow.FirstCell.Plane.Point(dxxRectanglePts.TopLeft) + aXDir * aSegWidth)
                        aP.AddV(aRow.LastCell.Plane.Point(dxxRectanglePts.TopRight) + aXDir * -aSegWidth)
                        _rVal.Add(aP)
                    End If
                    If aRows.HeaderRow <> aRows.Count Then
                        aP = New dxePolyline With
                        {
                            .PlaneV = aRows.Plane,
                            .Closed = False,
                            .DisplayStructure = aDisplayStructure,
                            .Color = aColor,
                            .SegmentWidth = aSegWidth,
                            .Identifier = "Exterior Border",
                            .Suppressed = bSuppressBorders
                        }
                        aP.AddV(aRow.FirstCell.Plane.Point(dxxRectanglePts.BottomLeft) + aXDir * aSegWidth)
                        aP.AddV(aRow.LastCell.Plane.Point(dxxRectanglePts.BottomRight) + aXDir * -aSegWidth)
                        _rVal.Add(aP)
                    End If
                End If
                If aRows.HeaderCol > 0 Then
                    aCell = aRows.FirstRow.Cell(aRows.HeaderCol)
                    bCell = aRows.LastRow.Cell(aRows.HeaderCol)
                    If aRows.HeaderCol > 1 Then
                        aP = New dxePolyline With
                        {
                            .PlaneV = aRows.Plane,
                            .Closed = False,
                            .DisplayStructure = aDisplayStructure,
                            .Color = aColor,
                            .SegmentWidth = aSegWidth,
                            .Identifier = "Exterior Border",
                            .Suppressed = bSuppressBorders
                        }
                        aP.AddV(aCell.Plane.Point(dxxRectanglePts.TopLeft) + aYDir * -aSegWidth)
                        aP.AddV(bCell.Plane.Point(dxxRectanglePts.BottomLeft) + aYDir * aSegWidth)
                        _rVal.Add(aP)
                    End If
                    If aRows.HeaderCol < aRows.FirstRow.Cells.Count Then
                        aP = New dxePolyline With
                        {
                            .PlaneV = aRows.Plane,
                            .Closed = False,
                            .DisplayStructure = aDisplayStructure,
                            .Color = aColor,
                            .SegmentWidth = aSegWidth,
                            .Identifier = "Exterior Border",
                            .Suppressed = bSuppressBorders
                        }
                        aP.AddV(aCell.Plane.Point(dxxRectanglePts.TopRight) + aYDir * -aSegWidth)
                        aP.AddV(bCell.Plane.Point(dxxRectanglePts.BottomRight) + aYDir * aSegWidth)
                        _rVal.Add(aP)
                    End If
                End If
                Return _rVal
            Catch
                Return _rVal
            End Try
        End Function
        Friend Sub xTableTitleAndFooter(aImage As dxfImage, aRows As TTABLEROWS, aCollector As List(Of dxeText), aExtentPts As TVECTORS, aXDir As TVECTOR, aYDir As TVECTOR, aGap As Double)
            Dim aStr As String = String.Empty
            Dim talng As dxxMTextAlignments
            Dim hAlign As dxxHorizontalJustifications
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aDir As TVECTOR
            Dim d1 As Double
            Dim tExts As New TVECTORS
            Dim mTxt As dxeText
            Dim bPlane As TPLANE = aRows.Plane
            Dim i As dxxCellTextTypes

            If aRows.Angle <> 0 Then bPlane.Revolve(aRows.Angle, False)
            aRows.CellBounds = aExtentPts.Bounds(bPlane)
            aStr = Trim(Title)
            If aStr <> "" Then
                hAlign = TitleAlignment
                If hAlign = dxxHorizontalJustifications.Center Then
                    talng = dxxMTextAlignments.MiddleCenter
                ElseIf hAlign = dxxHorizontalJustifications.Right Then
                    talng = dxxMTextAlignments.MiddleRight
                Else
                    talng = dxxMTextAlignments.MiddleLeft
                End If
                i = dxxCellTextTypes.Title
                mTxt = xTableText(aImage, bPlane, aStr, aRows.TextSize(i), talng, LayerName, aRows.TexStyle(i), aRows.TextColor(i), aRows.WidthFactor(i), 0, dxxTextTypes.Multiline)
                mTxt.Identifier = "Title"
                mTxt.UpdatePath()
                If hAlign = dxxHorizontalJustifications.Center Then
                    v1 = aExtentPts.Item(1).MidPt(aExtentPts.Item(4))
                    v2 = mTxt.Bounds.Point(dxxRectanglePts.BottomCenter)
                ElseIf hAlign = dxxHorizontalJustifications.Right Then
                    v1 = aExtentPts.Item(4)
                    v2 = mTxt.Bounds.Point(dxxRectanglePts.BottomRight)
                Else
                    v1 = aExtentPts.Item(1)
                    v2 = mTxt.Bounds.Point(dxxRectanglePts.BottomLeft)
                End If
                v1 += aYDir * aGap
                aDir = v2.DirectionTo(v1, False, rDistance:=d1)
                mTxt.Translate(aDir * d1)
                tExts.Append(mTxt.ExtentPts)
                If aCollector IsNot Nothing Then aCollector.Add(mTxt)
            End If
            'add the Footer
            aStr = Footer.Trim
            If aStr <> "" Then
                hAlign = FooterAlignment
                If hAlign = dxxHorizontalJustifications.Center Then
                    talng = dxxMTextAlignments.MiddleCenter
                ElseIf hAlign = dxxHorizontalJustifications.Right Then
                    talng = dxxMTextAlignments.MiddleRight
                Else
                    talng = dxxMTextAlignments.MiddleLeft
                End If
                i = dxxCellTextTypes.Footer
                mTxt = xTableText(aImage, bPlane, aStr, aRows.TextSize(i), talng, LayerName, aRows.TexStyle(i), aRows.TextColor(i), aRows.WidthFactor(i), 0, dxxTextTypes.Multiline)
                mTxt.Identifier = "Footer"
                mTxt.UpdatePath()
                If hAlign = dxxHorizontalJustifications.Center Then
                    v1 = aExtentPts.Item(2).MidPt(aExtentPts.Item(3))
                    v2 = mTxt.Bounds.Point(dxxRectanglePts.TopCenter)
                ElseIf hAlign = dxxHorizontalJustifications.Right Then
                    v1 = aExtentPts.Item(3)
                    v2 = mTxt.Bounds.Point(dxxRectanglePts.TopRight)
                Else
                    v1 = aExtentPts.Item(2)
                    v2 = mTxt.Bounds.Point(dxxRectanglePts.TopLeft)
                End If
                v1 += aYDir * -aGap
                aDir = v2.DirectionTo(v1, False, rDistance:=d1)
                mTxt.Translate(aDir * d1)
                tExts.Append(mTxt.ExtentPts)
                If aCollector IsNot Nothing Then aCollector.Add(mTxt)
            End If
            aExtentPts.Append(tExts)
        End Sub
        Friend Sub xUpdateTableRows(aTableRows As TTABLEROWS, aOffsetDirection As TVECTOR, ByRef rOffsetDistance As Double)
            '^return the cell settings from the passed table rows to the cells of the passed table
            Dim iR As Integer
            Dim iC As Integer
            Dim aCells As TTABLECELLS
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            Dim v1 As TVECTOR = aOffsetDirection * rOffsetDistance
            aTableRows.CellBounds.Origin += v1
            For iR = 1 To aTableRows.Count
                aRow = aTableRows.Row(iR)
                aCells = aRow.Cells
                For iC = 1 To aCells.Count
                    aCell = aCells.Cell(iC)
                    aCell.Row = iR
                    aCell.Column = iC
                    aCell.Plane.Origin += v1
                    aCell.TextRectangle.Origin += v1
                    aRow.Cells.SetCell(aCell, iC)
                Next iC
                aTableRows.SetRow(aRow, iR)
            Next iR
            Cells.Strukture = aTableRows
        End Sub

#End Region 'Methods
#Region "Shared Methods"
        Friend Shared Function CreateSubEntities(aTable As dxeTable, aImage As dxfImage, ByRef rExtentPts As TVECTORS) As colDXFEntities
            If aTable Is Nothing Then Return New colDXFEntities
            Dim _rVal As New colDXFEntities With {.ImageGUID = aTable.ImageGUID, .OwnerGUID = aTable.GUID}
            rExtentPts = New TVECTORS(0)
            Try
                If Not aTable.GetImage(aImage) Then aImage = dxfGlobals.New_Image()
                _rVal.ImageGUID = aImage.GUID
                Dim gSty As dxxTableGridStyles = aTable.GridStyle
                Dim ang As Double = aTable.RotationAngle
                Dim SF As Double = aTable.FeatureScale
                Dim bThk As Double = aTable.BorderLineThickness * SF
                Dim gThk As Double = aTable.GridLineThickness * SF
                Dim tgap As Double = aTable.TextGap * SF
                Dim aPlane As TPLANE = aTable.PlaneV.Clone
                Dim ip As TVECTOR = aTable.InsertionPtV
                Dim rHdr As Integer = aTable.HeaderRow
                Dim cHdr As Integer = aTable.HeaderCol
                Dim bSupBorders As Boolean = aTable.SuppressBorder
                Dim svBlock As Boolean = aTable.SaveAsBlock
                Dim d1 As Double
                Dim bFillColors As Boolean
                Dim bRowLines As Boolean = (gSty = dxxTableGridStyles.All Or gSty = dxxTableGridStyles.RowLines)
                Dim bColLines As Boolean = (gSty = dxxTableGridStyles.All Or gSty = dxxTableGridStyles.ColumnLines)
                Dim tDisp As TDISPLAYVARS = TENTITY.DisplayVarsFromProperties(aTable.Properties)
                Dim xDir As TVECTOR = aPlane.Direction(ang)
                Dim yDir As TVECTOR = aPlane.Direction(ang + 90)

                Dim gLines As List(Of dxfEntity)
                Dim bLines As List(Of dxfEntity)
                Dim mTxt As dxeText

                If bSupBorders Then bThk = 0
                If tDisp.LayerName = "" Then
                    tDisp.LayerName = aImage.BaseSettings(dxxSettingTypes.TABLESETTINGS).Props.ValueStr("LayerName")
                    tDisp.Color = dxxColors.ByLayer
                End If
                If tDisp.LayerName = "" Then tDisp.LayerName = "0"
                aTable.DisplayStructure = tDisp
                'get the cells
                'mark the header cells and set some properties
                Dim tRows As TTABLEROWS = aTable.xGetTableRows(rHdr, cHdr, bRowLines, bColLines, bSupBorders, bFillColors, gThk, bThk)
                Dim accents As New List(Of dxfEntity)
                'create the cell text entities and cell rectangles
                Dim aTexts As List(Of dxeText) = aTable.xCreateCellText(aImage, tDisp.LayerName, tRows, tgap, aTable.ColumnGap * SF, aTable.RowGap * SF)
                'align the cells horizontally and vertically
                tRows = aTable.xAlignTableCells(aImage, tRows, xDir, yDir, aTexts, tgap, accents)
                ''add the cell color backgrounds
                'If bFillColors Then xCellBackgrounds(tRows, tDisp.LayerName, xDir, yDir, _rVal)
                'add the grid lines
                Dim aclr As dxxColors = aTable.GridColor
                If aclr = dxxColors.Undefined Then aclr = tDisp.Color
                gLines = aTable.xGridLines(tRows, tDisp, gThk, aclr)
                'add the borderlines and initialize the extent pts
                aclr = aTable.BorderColor
                If aclr = dxxColors.Undefined Then aclr = aTable.GridColor
                If aclr = dxxColors.Undefined Then aclr = tDisp.Color
                bLines = aTable.xBorderLines(tRows, tDisp, aclr, bThk, bSupBorders, xDir, yDir, rExtentPts)
                'add the title and footer and grow the extpts if
                aTable.xTableTitleAndFooter(aImage, tRows, aTexts, rExtentPts, xDir, yDir, tgap)
                _rVal.Append(gLines)
                _rVal.Append(bLines)
                _rVal.CollectionObj.AddRange(accents)
                Dim aCell As TTABLECELL
                'add the text
                For i As Integer = 1 To aTexts.Count
                    mTxt = aTexts.Item(i - 1)
                    If mTxt.Row > 0 And mTxt.Col > 0 Then
                        aCell = tRows.Row(mTxt.Row).Cell(mTxt.Col)
                    Else
                        aCell = New TTABLECELL(0, 0)
                    End If
                    If Not svBlock Then
                        _rVal.Add(mTxt)
                    Else
                        _rVal.Add(mTxt)
                    End If
                Next i
                Dim bndRec As TPLANE = rExtentPts.Bounds(aPlane)
                Dim aDir As TVECTOR = bndRec.GripPoint(aTable.Alignment).DirectionTo(ip, False, rDistance:=d1)
                'align to the alignment point
                If d1 <> 0 Then
                    _rVal.Project(aDir, d1, True)
                    rExtentPts.Project(aDir, d1, True)
                    aTable.xUpdateTableRows(tRows, aDir, d1)
                End If
                'set the returns
                aTable.SetPropVal("ColCount", aTable.Cells.ColumnCount)
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, "dxeTable", ex)
            Finally
                aTable.SuppressEvents = False
                aTable.IsDirty = False
            End Try
            Return _rVal
        End Function
#End Region 'Shared Methods
    End Class 'dxeTable
End Namespace
