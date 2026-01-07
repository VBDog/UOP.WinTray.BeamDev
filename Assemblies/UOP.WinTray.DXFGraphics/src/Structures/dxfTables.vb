

Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures


#Region "Structures"

    Friend Structure TTABLECELL
        Implements ICloneable

#Region "Members"

        Public Alignment As dxxRectangularAlignments
        Public Column As Integer
        Public Data As Object
        Public FillColor As dxxColors
        Public IsAttribute As Boolean
        Public IsHeaderCell As Boolean
        Public Name As String
        Public Plane As TPLANE
        Public Prompt As String
        Public Row As Integer
        Public StyleName As String
        Public TextAngle As Double
        Public TextColor As dxxColors
        Public TextRectangle As TPLANE
        Public TextScale As Double
        Public WidthFactor As Double

        Private _BorderThicknesses() As Double
        Private _BorderToggles() As Boolean

#End Region 'Members

#Region "Constructors"

        Public Sub New(Optional aName As String = "")
            'init -----------------------------------------
            Alignment = dxxRectangularAlignments.TopLeft
            Column = 0
            Data = ""
            FillColor = dxxColors.ByLayer
            IsAttribute = False
            IsHeaderCell = False
            Name = ""
            Plane = TPLANE.World
            Prompt = ""
            Row = 0
            StyleName = ""
            TextAngle = 0
            TextColor = dxxColors.Undefined
            TextRectangle = TPLANE.World
            TextScale = 1
            WidthFactor = 1
            ReDim _BorderThicknesses(0 To 3)
            ReDim _BorderToggles(0 To 3)
            'init -----------------------------------------

        End Sub
        Public Sub New(aCell As TTABLECELL, Optional aData As Object = Nothing)
            'init -----------------------------------------
            Alignment = aCell.Alignment
            Column = aCell.Column
            Data = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aCell.Data)
            FillColor = aCell.FillColor
            IsAttribute = aCell.IsAttribute
            IsHeaderCell = aCell.IsHeaderCell
            Name = aCell.Name
            Plane = New TPLANE(aCell.Plane)
            Prompt = aCell.Prompt
            Row = aCell.Row
            StyleName = aCell.StyleName
            TextAngle = aCell.TextAngle
            TextColor = aCell.TextColor
            TextRectangle = New TPLANE(aCell.TextRectangle)
            TextScale = aCell.TextScale
            WidthFactor = aCell.WidthFactor
            ReDim _BorderThicknesses(0 To 3)
            ReDim _BorderToggles(0 To 3)

            'init -----------------------------------------
            If aCell._BorderThicknesses IsNot Nothing Then _BorderThicknesses = aCell._BorderThicknesses.Clone()
            If aCell._BorderToggles IsNot Nothing Then _BorderToggles = aCell._BorderToggles.Clone()

            If aData IsNot Nothing Then Data = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aData)

        End Sub
        Public Sub New(aRow As Integer, aColumn As Integer, Optional aName As String = "", Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.MiddleCenter)
            'init -----------------------------------------
            Alignment = aAlignment
            Column = aColumn
            Data = ""
            FillColor = dxxColors.ByLayer
            IsAttribute = False
            IsHeaderCell = False
            Name = IIf(String.IsNullOrWhiteSpace(aName), "", aName.Trim())
            Plane = TPLANE.World
            Prompt = ""
            Row = aRow
            StyleName = ""
            TextAngle = 0
            TextColor = dxxColors.Undefined
            TextRectangle = TPLANE.World
            TextScale = 1
            WidthFactor = 1
            ReDim _BorderThicknesses(0 To 3)
            ReDim _BorderToggles(0 To 3)
            'init -----------------------------------------



        End Sub

#End Region 'Constructors

#Region "Properties"

        Public ReadOnly Property MTextAlignment As dxxMTextAlignments
            Get
                Dim halgn As dxxHorizontalJustifications = dxxHorizontalJustifications.Center
                Dim valgn As dxxVerticalJustifications = dxxVerticalJustifications.Center
                dxfUtils.ParseRectangleAlignment(Alignment, halgn, valgn)
                Return dxfUtils.CreateMTextAlignment(halgn, valgn)
            End Get
        End Property

        Public ReadOnly Property StringData As String
            Get
                If Data Is Nothing Then Return String.Empty Else Return Data.ToString
            End Get
        End Property

        Public Property Height As Double
            Get
                Return Plane.Height
            End Get
            Set
                Plane.Height = Value
            End Set
        End Property

        Public Property Width As Double
            Get
                Return Plane.Width
            End Get
            Set
                Plane.Width = Value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub ClearSettings()

            ReDim _BorderThicknesses(0 To 3)
            ReDim _BorderToggles(0 To 3)

        End Sub

        Public Sub SetBorderThickness(aThickness As Double, Optional aIndex As dxxBorderPointers = dxxBorderPointers.All)
            If aThickness < 0 Then aThickness = 0

            If aIndex < 0 Or aIndex > 3 Then
                _BorderThicknesses(0) = aThickness
                _BorderThicknesses(1) = aThickness
                _BorderThicknesses(2) = aThickness
                _BorderThicknesses(3) = aThickness
            Else
                _BorderThicknesses(aIndex) = aThickness
            End If

        End Sub

        Public Sub SetBorderToggle(aValue As Double, Optional aIndex As dxxBorderPointers = dxxBorderPointers.All)
            If _BorderToggles Is Nothing Then
                ReDim _BorderThicknesses(0 To 3)
                ReDim _BorderToggles(0 To 3)
            End If
            If aIndex < 0 Or aIndex > 3 Then
                _BorderToggles(0) = aValue
                _BorderToggles(1) = aValue
                _BorderToggles(2) = aValue
                _BorderToggles(3) = aValue
            Else
                _BorderToggles(aIndex) = aValue
            End If

        End Sub

        Public Function BorderIsVisible(aIndex As dxxBorderPointers) As Boolean
            If aIndex < 0 Or aIndex > 3 Then Return False
            Return _BorderToggles(CType(aIndex, Integer))
        End Function

        Public Function BorderThickness(aIndex As dxxBorderPointers) As Double
            If aIndex < 0 Or aIndex > 3 Then Return False
            Return _BorderThicknesses(CType(aIndex, Integer))
        End Function

        Public Function Clone(Optional aData As Object = Nothing) As TTABLECELL
            Return New TTABLECELL(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTABLECELL(Me)
        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String = $"TTABLECELL { Row}:{Column} ["

            If Data IsNot Nothing Then _rVal += Data.ToString
            _rVal += "]"
            Return _rVal

        End Function

#End Region 'Methods

    End Structure 'TTABLECELL

    Friend Structure TTABLECELLS
        Implements ICloneable
#Region "Members"

        Private _Init As Boolean
        Public _Members() As TTABLECELL
        Public Row As Integer

#End Region 'Members

#Region "Constructors"
        Public Sub New(aCount As Integer)
            'init ---------------------------------------------------
            Row = 0
            _Init = True
            ReDim _Members(-1)
            'init ---------------------------------------------------
            For i As Integer = 1 To aCount
                Add(New TTABLECELL(Row, i))
            Next

        End Sub

        Public Sub New(aCells As TTABLECELLS)
            'init ---------------------------------------------------
            Row = aCells.Row
            _Init = True
            ReDim _Members(-1)
            'init ---------------------------------------------------
            For i As Integer = 1 To aCells.Count
                Add(New TTABLECELL(aCells.Cell(i)))
            Next

        End Sub
#End Region 'Constructors

#Region "Properties"

        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)

                End If
                Return _Members.Count
            End Get
        End Property




#End Region 'Properties

#Region "Methods"

        Public Function ToList() As List(Of TTABLECELL)
            If Count <= 0 Then Return New List(Of TTABLECELL)
            Return _Members.ToList()
        End Function
        Public Function Cell(aColumn As Integer) As TTABLECELL

            If aColumn < 1 Or aColumn > Count Then Return Nothing
            _Members(aColumn - 1).Row = Row
            _Members(aColumn - 1).Column = aColumn
            Return _Members(aColumn - 1)


        End Function

        Public Sub SetCell(value As TTABLECELL, aColumn As Integer)

            If aColumn < 1 Or aColumn > Count Then Return
            _Members(aColumn - 1) = value

        End Sub

        Public Overrides Function ToString() As String
            Return $"TTABLECELLS [{ Count }]"
        End Function

        Public Sub Add(aCell As TTABLECELL, Optional aData As Object = Nothing)

            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aCell
            If aData IsNot Nothing Then _Members(_Members.Count - 1).Data = aData
            _Members(_Members.Count - 1).Row = Row
            _Members(_Members.Count - 1).Column = _Members.Count

        End Sub

        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub

        Public Function Clone() As TTABLECELLS

            Return New TTABLECELLS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTABLECELLS(Me)
        End Function
        Public Function Remove(aRow As Integer) As Boolean
            If aRow < 1 Or aRow > Count Or Count <= 0 Then Return False
            If aRow = Count Then

                Array.Resize(_Members, Count - 1)

                Return True
            End If
            Dim newMems(0 To Count - 1) As TTABLECELL
            Dim j As Integer
            Dim i As Integer
            For i = 1 To Count
                If i <> aRow Then
                    newMems(j) = _Members(i - 1)
                    newMems(j).Column = j + 1
                    j += 1
                End If
            Next
            _Members = newMems

            Return True
        End Function
#End Region 'Methods
    End Structure 'TTABLECELLS

    Friend Structure TTABLEROW
        Implements ICloneable
#Region "Members"

        Public Cells As TTABLECELLS


#End Region 'Members

#Region "Constructors"
        Public Sub New(aColumnCount As Integer, Optional aData As Object = Nothing)
            'init ------------------------------------------------------
            Cells = New TTABLECELLS(0)
            Row = 0
            'init ------------------------------------------------------
            Dim c As Integer = 1
            If aColumnCount > 0 Then
                Do Until Cells.Count = aColumnCount
                    Cells.Add(New TTABLECELL(0, c), aData)
                    c += 1
                Loop
            End If

        End Sub

        Public Sub New(aCells As TTABLECELLS, aRow As Integer)
            'init ------------------------------------------------------
            Cells = New TTABLECELLS(aCells)
            Row = aRow
            'init ------------------------------------------------------

        End Sub

        Public Sub New(aRow As TTABLEROW)
            'init ------------------------------------------------------
            Cells = New TTABLECELLS(aRow.Cells)
            Row = aRow.Row
            'init ------------------------------------------------------

        End Sub
#End Region 'Constructors

#Region "Properties"

        Public Property Row As Integer
            Get
                Return Cells.Row
            End Get

            Set
                Cells.Row = Value
            End Set
        End Property


        Public Function Cell(aColumn As Integer) As TTABLECELL

            Return Cells.Cell(aColumn)

        End Function

        Public Sub SetCell(Value As TTABLECELL, aColumn As Integer)


            Cells.SetCell(Value, aColumn)

        End Sub


        Public ReadOnly Property FirstCell As TTABLECELL
            Get
                Return Cells.Cell(1)
            End Get
        End Property
        Public ReadOnly Property LastCell As TTABLECELL
            Get
                Return Cells.Cell(Cells.Count)
            End Get
        End Property

        Public ReadOnly Property CellCount As Integer
            Get
                Return Cells.Count
            End Get
        End Property

        Public ReadOnly Property MaxCellHeight As Double
            Get
                Dim _rVal As Double = 0
                For iC As Integer = 1 To Cells.Count
                    If Cells.Cell(iC).Height > _rVal Then
                        _rVal = Cells.Cell(iC).Height
                    End If
                Next iC

                Return _rVal
            End Get

        End Property
        Public WriteOnly Property CellHeight As Double
            Set
                Dim _rVal As Double = 0
                Dim aCell As TTABLECELL
                For iC As Integer = 1 To Cells.Count
                    aCell = Cells.Cell(iC)
                    aCell.Height = Value
                    Cells.SetCell(aCell, iC)
                Next iC


            End Set

        End Property

        Public WriteOnly Property ColumnCount As Integer
            Set
                If Value < 0 Then Return
                If Cells.Count = Value Then Return
                If Value = 0 Then
                    Cells.Clear()
                    Return
                End If


                Dim aCell As TTABLECELL

                If Cells.Count < Value Then  'grow

                    If Cells.Count > 0 Then aCell = Cell(Cells.Count).Clone("") Else aCell = New TTABLECELL("")

                    Do Until Cells.Count = Value
                        Cells.Add(aCell.Clone(""))
                    Loop
                Else 'shrink
                    Do Until Cells.Count = Value
                        If Not Cells.Remove(Cells.Count) Then Exit Do
                    Loop

                End If
            End Set
        End Property

#End Region 'Properties

#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"TTABLEROW { Row }x{ Cells.Count}"

        End Function
        Public Function Clone() As TTABLEROW
            Return New TTABLEROW(Me)

        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTABLEROW(Me)
        End Function
        Public Function SetAlignment(aAlign As dxxRectangularAlignments, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the alignment to apply
            '#3an optional column ID to limit the application to
            '^applies the passed alignment value to cells of the row
            '~passing a non-zero colid applies the passed alignment to only the cells with a column id matching the passed value

            If aAlign < 1 Or aAlign > 9 Then Return _rVal
            Dim aCell As TTABLECELL
            Dim i As Integer
            If aColID = 0 Then
                For i = 1 To Cells.Count
                    aCell = Cell(i)
                    aCell.Column = i
                    If aCell.Alignment <> aAlign Then _rVal = True
                    aCell.Alignment = aAlign
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else
                If aColID > 0 And aColID <= Cells.Count Then

                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    If aCell.Alignment <> aAlign Then _rVal = True
                    aCell.Alignment = aAlign
                    SetCell(aCell, aColID)
                End If

            End If

            Return _rVal

        End Function

        Public Function SetData(aData As Object, Optional aColID As Integer = 0, Optional bNullCellsOnly As Boolean = False) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the data to apply
            '#3the column index to apply the data to
            '#4flag to apply the data only to empty cells
            '^puts the passed data value into the cells of the row
            '~a col id of 0 applies the data to all the columns
            Dim aCell As TTABLECELL
            Dim i As Integer
            If aColID <> 0 Then
                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    aCell.Row = Row
                    If (bNullCellsOnly And aCell.Data.ToString() = "") Or Not bNullCellsOnly Then
                        _rVal = aCell.Data <> aData
                        aCell.Data = aData
                    End If
                    aCell.Column = aColID
                    SetCell(aCell, aColID)
                End If
            Else
                For i = 1 To Cells.Count
                    aCell = Cell(i)
                    aCell.Column = i
                    aCell.Row = Row
                    If (bNullCellsOnly And aCell.Data.ToString() = "") Or Not bNullCellsOnly Then
                        If aCell.Data <> aData Then _rVal = True
                        aCell.Data = aData
                    End If
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i

            End If

            Return _rVal

        End Function

        Public Function SetFillColor(aColor As dxxColors, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the fill color to apply
            '#3the column to apply the color to

            '^applies the passed fill color value to cells of the row
            '~a col id of 0 applies the change to all the columns
            Dim i As Integer
            Dim aCell As TTABLECELL
            If aColID = 0 Then
                For i = 1 To Cells.Count

                    aCell = Cell(i)
                    aCell.Column = i
                    If aCell.FillColor <> aColor Then _rVal = True
                    aCell.FillColor = aColor
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else

                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    If aCell.FillColor <> aColor Then _rVal = True
                    aCell.FillColor = aColor
                    SetCell(aCell, aColID)
                End If

            End If

            Return _rVal

        End Function

        Public Function SetHeight(aHeight As Double, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the cell height to apply
            '#3the column to apply the cell height to

            '^applies the passed cell height value to cells of the row
            '~a col id of 0 applies the change to all the columns
            If aHeight < 0 Then Return _rVal
            Dim aCell As TTABLECELL
            Dim i As Integer
            If aColID = 0 Then
                For i = 1 To Cells.Count
                    aCell = Cell(i)
                    aCell.Column = i
                    If aHeight <> aCell.Height Then _rVal = True
                    aCell.Height = aHeight
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else
                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    If aHeight <> aCell.Height Then _rVal = True
                    aCell.Height = aHeight
                    SetCell(aCell, aColID)
                End If
            End If

            Return _rVal

        End Function

        Public Function SetTextAngle(aTextAngle As Double, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the cell text angle to apply
            '#3the column to apply the cell text angle to

            '^applies the passed cell text angle value to cells of the row
            '~a col id of 0 applies the change to all the columns
            If aTextAngle < 0 Then Return _rVal
            Dim aCell As TTABLECELL
            Dim i As Integer
            If aColID = 0 Then
                For i = 1 To Cells.Count
                    aCell = Cell(i)
                    aCell.Column = i
                    If aTextAngle <> aCell.Height Then _rVal = True
                    aCell.TextAngle = aTextAngle
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else
                If aColID > 0 And aColID <= Cells.Count Then

                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    If aTextAngle <> aCell.TextAngle Then _rVal = True
                    aCell.TextAngle = aTextAngle
                    SetCell(aCell, aColID)
                End If
            End If

            Return _rVal

        End Function

        Public Function SetTextColor(aColor As dxxColors, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the text color to apply
            '#3the column to apply the color to

            '^applies the passed text color value to cells of the row
            '~a col id of 0 applies the change to all the columns
            Dim i As Integer
            Dim aCell As TTABLECELL
            If aColID = 0 Then
                For i = 1 To Cells.Count
                    aCell = Cell(i)
                    aCell.Column = i
                    aCell.Row = Row
                    If aCell.TextColor <> aColor Then _rVal = True
                    aCell.TextColor = aColor
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else
                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Row = Row
                    aCell.Column = aColID
                    If aCell.TextColor <> aColor Then _rVal = True
                    aCell.TextColor = aColor
                    SetCell(aCell, aColID)
                End If

            End If

            Return _rVal

        End Function

        Public Function SetTextScale(aScale As Double, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the text scale to apply
            '#2the column to apply the scale to

            '^applies the passed text scale value to cells of the row
            '~a col id of 0 applies the change to all the columns

            Dim i As Integer
            Dim aCell As TTABLECELL
            If aColID = 0 Then
                For i = 1 To Cells.Count

                    aCell = Cell(i)
                    aCell.Column = i
                    aCell.Row = Row
                    If aCell.TextScale <> aScale Then _rVal = True
                    aCell.TextScale = aScale
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else

                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    aCell.Row = Row
                    If aCell.TextScale <> aScale Then _rVal = True
                    aCell.TextScale = aScale
                    SetCell(aCell, aColID)
                End If

            End If

            Return _rVal

        End Function

        Public Function SetTextStyle(aStyleName As String, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the text style name to apply
            '#3the column to apply the style name to

            '^applies the passed text style name value to cells of the row
            '~a col id of 0 applies the change to all the columns
            Dim i As Integer
            Dim aCell As TTABLECELL
            If aColID = 0 Then
                For i = 1 To Cells.Count

                    aCell = Cell(i)
                    aCell.Column = i
                    Row = Row
                    If aCell.StyleName <> aStyleName Then _rVal = True
                    aCell.StyleName = aStyleName
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else

                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    aCell.Row = Row
                    If aCell.StyleName <> aStyleName Then _rVal = True
                    aCell.StyleName = aStyleName
                    SetCell(aCell, aColID)
                End If

            End If

            Return _rVal

        End Function

        Public Function SetWidth(aWidth As Double, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#2the cell width to apply
            '#3the column to apply the cell width to

            '^applies the passed cell width value to cells of the row
            '~a col id of 0 applies the change to all the columns
            If aWidth < 0 Then Return _rVal
            Dim aCell As TTABLECELL
            Dim i As Integer
            If aColID = 0 Then
                For i = 1 To Cells.Count
                    aCell = Cell(i)
                    aCell.Column = i
                    aCell.Row = Row
                    If aWidth <> aCell.Width Then _rVal = True
                    aCell.Width = aWidth
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else
                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    aCell.Row = Row
                    If aWidth <> aCell.Width Then _rVal = True
                    aCell.Width = aWidth
                    SetCell(aCell, aColID)
                End If
            End If

            Return _rVal

        End Function

        Public Function SetWidthFactor(aWidthFactor As Double, Optional aColID As Integer = 0) As Boolean

            Dim _rVal As Boolean

            '#1the subject row
            '#1the cell text width factor to apply
            '#2the column to apply the cell text width factor to

            '^applies the passed cell text width factor value to cells of the row
            '~a col id of 0 applies the change to all the columns
            If aWidthFactor < 0 Then Return _rVal
            Dim aCell As TTABLECELL
            Dim i As Integer
            If aColID = 0 Then
                For i = 1 To Cells.Count
                    aCell = Cell(i)
                    aCell.Column = i
                    aCell.Row = Row
                    If aWidthFactor <> aCell.WidthFactor Then _rVal = True
                    aCell.WidthFactor = aWidthFactor
                    SetCell(aCell, i)
                    'Application.DoEvents()
                Next i
            Else
                If aColID > 0 And aColID <= Cells.Count Then
                    aCell = Cell(aColID)
                    aCell.Column = aColID
                    aCell.Row = Row
                    If aWidthFactor <> aCell.WidthFactor Then _rVal = True
                    aCell.WidthFactor = aWidthFactor
                    SetCell(aCell, aColID)
                End If
            End If

            Return _rVal

        End Function

#End Region 'Methods"
    End Structure 'TTABLEROW

    Friend Structure TTABLEROWS
        Implements ICloneable
#Region "Members"

        Public Angle As Double
        Public Attributes As Boolean
        Public BasePt As TVECTOR
        Public BorderThickness As Double
        Public CellBounds As TPLANE
        Public GridThickness As Double
        Public HeaderCol As Integer
        Public HeaderRow As Integer
        Public IsDirty As Boolean
        Public Plane As TPLANE
        Private _StyleNames() As String
        Private _TextColors() As dxxColors
        Private _TextHeights() As Double
        Private _WidthFactors() As Double
        Public Name As String

        Private _Init As Boolean
        Private _Members() As TTABLEROW


#End Region 'Members

#Region "Constructors"

        Public Sub New(Optional aName As String = "")
            'init -----------------------------------------------
            Angle = 0
            Attributes = False
            BasePt = TVECTOR.Zero
            BorderThickness = 0
            CellBounds = TPLANE.World
            GridThickness = 0
            HeaderCol = 0
            HeaderRow = 0
            IsDirty = False
            Plane = TPLANE.World
            Name = ""

            _Init = True
            ReDim _Members(-1)
            ReDim _StyleNames(0 To 4)
            ReDim _TextColors(0 To 4)
            ReDim _TextHeights(0 To 4)
            ReDim _WidthFactors(0 To 4)
            'init -----------------------------------------------

            If Not String.IsNullOrEmpty(aName) Then Name = aName
        End Sub
        Public Sub New(aRows As TTABLEROWS)
            'init -----------------------------------------------
            Angle = aRows.Angle
            Attributes = aRows.Attributes
            BasePt = New TVECTOR(aRows.BasePt)
            BorderThickness = aRows.BorderThickness
            CellBounds = New TPLANE(aRows.CellBounds)
            GridThickness = aRows.GridThickness
            HeaderCol = aRows.HeaderCol
            HeaderRow = aRows.HeaderRow
            IsDirty = aRows.IsDirty
            Plane = New TPLANE(aRows.Plane)
            Name = aRows.Name

            _Init = True
            ReDim _Members(-1)
            ReDim _StyleNames(0 To 4)
            ReDim _TextColors(0 To 4)
            ReDim _TextHeights(0 To 4)
            ReDim _WidthFactors(0 To 4)
            'init -----------------------------------------------

            For i As Integer = 1 To aRows.Count
                Add(New TTABLEROW(aRows.Row(i)))
            Next
            If aRows._StyleNames IsNot Nothing Then _StyleNames = aRows._StyleNames.Clone()
            If aRows._TextColors IsNot Nothing Then _TextColors = aRows._TextColors.Clone()
            If aRows._TextHeights IsNot Nothing Then _TextHeights = aRows._TextHeights.Clone()
            If aRows._WidthFactors IsNot Nothing Then _WidthFactors = aRows._WidthFactors.Clone()


        End Sub
#End Region 'Constructors

#Region "Properties"

        Public Property ColumnCount As Integer
            Get
                If Count > 0 Then Return Row(1).Cells.Count Else Return 0
            End Get

            Set
                If Value < 0 Then Value = 0

                For i As Integer = 1 To Count
                    _Members(i - 1).ColumnCount = Value
                Next i
            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property



        Public ReadOnly Property FirstRow As TTABLEROW
            Get
                Return Row(1)
            End Get
        End Property
        Public ReadOnly Property LastRow As TTABLEROW
            Get
                Return Row(Count)
            End Get
        End Property




#End Region 'Properties

#Region "Methods"

        Public Function ToList() As List(Of TTABLEROW)
            If Count <= 0 Then Return New List(Of TTABLEROW)
            Return _Members.ToList()
        End Function
        Public Function Row(aRow As Integer) As TTABLEROW

            If aRow < 1 Or aRow > Count Then Return Nothing
            _Members(aRow - 1).Row = aRow
            Return _Members(aRow - 1)


        End Function

        Public Function Cell(aRow As Integer, aColumn As Integer) As TTABLECELL

            If aRow <= 0 Or aRow > Count Then Return Nothing
            Return Row(aRow).Cells.Cell(aColumn)

        End Function

        Public Sub SetRow(Value As TTABLEROW, aRow As Integer)
            If aRow < 1 Or aRow > Count Then Return
            _Members(aRow - 1) = Value
        End Sub



        Public Sub SetCell(Value As TTABLECELL, aRow As Integer, aColumn As Integer)

            If aRow <= 0 Or aRow > Count Then Return
            Dim aMem As TTABLEROW = Row(aRow)
            If aMem.Row > 0 Then
                aMem.Cells.SetCell(Value, aColumn)
                SetRow(aMem, aRow)
            End If

        End Sub

        Public Sub ClearSettings()
            ReDim _TextHeights(0 To 3)
            ReDim _StyleNames(0 To 3)
            ReDim _TextColors(0 To 3)
            ReDim _WidthFactors(0 To 3)


        End Sub
        Public Overrides Function ToString() As String
            Return $"TTABLEROWS [{ Count }]"
        End Function


        Public Function Clone() As TTABLEROWS
            Return New TTABLEROWS(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTABLEROWS(Me)
        End Function
        Public Sub Add(aRow As TTABLEROW)

            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aRow
            _Members(_Members.Count - 1).Row = _Members.Count
            IsDirty = True
        End Sub


        Public Function Remove(aRow As Integer) As Boolean
            If aRow < 1 Or aRow > Count Or Count <= 0 Then Return False
            If aRow = Count Then
                If Count > 1 Then Array.Resize(_Members, Count - 1) Else Array.Resize(_Members, 0)
                Return True
            End If
            Dim newMems(0 To Count - 1) As TTABLEROW
            Dim j As Integer
            Dim i As Integer
            For i = 1 To Count
                If i <> aRow Then
                    newMems(j) = _Members(i - 1)
                    j += 1
                End If
            Next
            _Members = newMems
            Return True
        End Function

        Public Function MaxCellWidth(aCol As Integer) As Double
            Dim _rVal As Double
            Dim aRow As TTABLEROW
            For iR As Integer = 1 To Count
                aRow = Row(iR)
                If aCol >= 1 And aCol <= aRow.CellCount Then

                    If aRow.Cell(aCol).Width > _rVal Then _rVal = aRow.Cell(aCol).Width
                End If

            Next
            Return _rVal
        End Function

        Public Sub SetTextSizes(aFieldSize As Double, aHeaderSize As Double, aTitleSize As Double, aFooterSize As Double)
            aFieldSize = Math.Abs(aFieldSize)
            If aFieldSize <= 0 Then aFieldSize = 0.2
            aHeaderSize = Math.Abs(aHeaderSize)
            If aHeaderSize <= 0 Then aHeaderSize = aFieldSize
            aTitleSize = Math.Abs(aTitleSize)
            If aTitleSize <= 0 Then aTitleSize = aFieldSize
            aFooterSize = Math.Abs(aFooterSize)
            If aFooterSize <= 0 Then aFooterSize = aFieldSize
            ReDim _TextHeights(0 To 3)
            _TextHeights(dxxCellTextTypes.Field) = aFieldSize
            _TextHeights(dxxCellTextTypes.Header) = aHeaderSize
            _TextHeights(dxxCellTextTypes.Title) = aTitleSize
            _TextHeights(dxxCellTextTypes.Footer) = aFooterSize


        End Sub

        Public Sub SetWidthFactors(aFieldFactor As Double, aHeaderFactor As Double, aTitleFactor As Double, aFooterFactor As Double)
            aFieldFactor = Math.Abs(aFieldFactor)
            If aFieldFactor <= 0.1 Then aFieldFactor = 1
            aHeaderFactor = Math.Abs(aHeaderFactor)
            If aHeaderFactor <= 0.1 Then aHeaderFactor = aFieldFactor
            aTitleFactor = Math.Abs(aTitleFactor)
            If aTitleFactor <= 0.1 Then aTitleFactor = aFieldFactor
            aFooterFactor = Math.Abs(aFooterFactor)
            If aFooterFactor <= 0.1 Then aFooterFactor = aFieldFactor
            ReDim _WidthFactors(0 To 3)
            _WidthFactors(dxxCellTextTypes.Field) = aFieldFactor
            _WidthFactors(dxxCellTextTypes.Header) = aHeaderFactor
            _WidthFactors(dxxCellTextTypes.Title) = aTitleFactor
            _WidthFactors(dxxCellTextTypes.Footer) = aFooterFactor


        End Sub

        Public Sub SetTextStyles(aFieldStyle As String, aHeaderStyle As String, aTitleStyle As String, aFooterStyle As String)
            If String.IsNullOrWhiteSpace(aFieldStyle) Then aFieldStyle = "Standard" Else aFieldStyle = aFieldStyle.Trim
            If String.IsNullOrWhiteSpace(aHeaderStyle) Then aHeaderStyle = aFieldStyle Else aHeaderStyle = aHeaderStyle.Trim
            If String.IsNullOrWhiteSpace(aTitleStyle) Then aTitleStyle = aFieldStyle Else aTitleStyle = aTitleStyle.Trim
            If String.IsNullOrWhiteSpace(aFooterStyle) Then aFooterStyle = aFieldStyle Else aFooterStyle = aFooterStyle.Trim


            ReDim _StyleNames(0 To 3)
            _StyleNames(dxxCellTextTypes.Field) = aFieldStyle
            _StyleNames(dxxCellTextTypes.Header) = aHeaderStyle
            _StyleNames(dxxCellTextTypes.Title) = aTitleStyle
            _StyleNames(dxxCellTextTypes.Footer) = aFooterStyle


        End Sub

        Public Sub SetTextColors(aFieldColor As dxxColors, aHeaderColor As dxxColors, aTitleColor As dxxColors, aFooterColor As dxxColors)
            If aFieldColor = dxxColors.Undefined Then aFieldColor = dxxColors.ByLayer
            If aHeaderColor = dxxColors.Undefined Then aFieldColor = dxxColors.ByLayer
            If aTitleColor = dxxColors.Undefined Then aTitleColor = dxxColors.ByLayer
            If aFooterColor = dxxColors.Undefined Then aFooterColor = dxxColors.ByLayer

            ReDim _TextColors(0 To 3)
            _TextColors(dxxCellTextTypes.Field) = aFieldColor
            _TextColors(dxxCellTextTypes.Header) = aHeaderColor
            _TextColors(dxxCellTextTypes.Title) = aTitleColor
            _TextColors(dxxCellTextTypes.Footer) = aFooterColor


        End Sub

        Public Sub SetTextSize(aHeight As Double, aTextType As dxxCellTextTypes)
            aHeight = Math.Abs(aHeight)
            If aHeight = 0 Then aHeight = 0.2
            If aTextType < 0 Or aTextType > 3 Then
                _TextHeights(dxxCellTextTypes.Field) = aHeight
                _TextHeights(dxxCellTextTypes.Header) = aHeight
                _TextHeights(dxxCellTextTypes.Title) = aHeight
                _TextHeights(dxxCellTextTypes.Footer) = aHeight
            Else
                _TextHeights(aTextType) = aHeight
            End If

        End Sub

        Public Function TextSize(aTextType As dxxCellTextTypes) As Double
            If aTextType < 0 Or aTextType > 3 Then
                Return _TextHeights(dxxCellTextTypes.Field)
            Else
                Return _TextHeights(aTextType)
            End If
        End Function

        Public Sub SetTextStyle(aStyleName As String, aTextType As dxxCellTextTypes)
            If String.IsNullOrWhiteSpace(aStyleName) Then aStyleName = "Standard" Else aStyleName = aStyleName.Trim

            If aTextType < 0 Or aTextType > 3 Then
                _StyleNames(dxxCellTextTypes.Field) = aStyleName
                _StyleNames(dxxCellTextTypes.Header) = aStyleName
                _StyleNames(dxxCellTextTypes.Title) = aStyleName
                _StyleNames(dxxCellTextTypes.Footer) = aStyleName
            Else
                _StyleNames(aTextType) = aStyleName
            End If

        End Sub

        Public Function TexStyle(aTextType As dxxCellTextTypes) As String
            If aTextType < 0 Or aTextType > 3 Then
                Return _StyleNames(dxxCellTextTypes.Field)
            Else
                Return _StyleNames(aTextType)
            End If
        End Function

        Public Sub SetWidthFactor(aFactor As Double, aTextType As dxxCellTextTypes)
            aFactor = Math.Abs(aFactor)
            If aFactor = 0 Then aFactor = 1
            If aFactor < 0.1 Then aFactor = 0.1

            If aTextType < 0 Or aTextType > 3 Then
                _TextHeights(dxxCellTextTypes.Field) = aFactor
                _TextHeights(dxxCellTextTypes.Header) = aFactor
                _TextHeights(dxxCellTextTypes.Title) = aFactor
                _TextHeights(dxxCellTextTypes.Footer) = aFactor
            Else
                _TextHeights(aTextType) = aFactor
            End If

        End Sub

        Public Function WidthFactor(aTextType As dxxCellTextTypes) As Double
            If aTextType < 0 Or aTextType > 3 Then
                Return _WidthFactors(dxxCellTextTypes.Field)
            Else
                Return _WidthFactors(aTextType)
            End If
        End Function

        Public Sub SetTextColor(aColor As dxxColors, aTextType As dxxCellTextTypes)

            If aTextType < 0 Or aTextType > 3 Then
                _TextColors(dxxCellTextTypes.Field) = aColor
                _TextColors(dxxCellTextTypes.Header) = aColor
                _TextColors(dxxCellTextTypes.Title) = aColor
                _TextColors(dxxCellTextTypes.Footer) = aColor
            Else
                _TextColors(aTextType) = aColor
            End If

        End Sub

        Public Function TextColor(aTextType As dxxCellTextTypes) As Double
            If aTextType < 0 Or aTextType > 3 Then
                Return _TextColors(dxxCellTextTypes.Field)
            Else
                Return _TextColors(aTextType)
            End If
        End Function


        Public Sub SetCellWidth(aCol As Integer, aWidth As Double)

            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            For iR As Integer = 1 To Count
                aRow = Row(iR)
                If aCol >= 1 And aCol <= aRow.CellCount Then
                    aCell = aRow.Cell(aCol)
                    aCell.Width = aWidth
                    aRow.SetCell(aCell, aCol)
                    SetRow(aRow, iR)
                End If

            Next

        End Sub

#End Region 'Methods

    End Structure 'TTABLEROWS
#End Region 'Structures

End Namespace

