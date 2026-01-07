Imports System.Security
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoTableCells
        Implements ICloneable
#Region "Members"
        Private _Struc As TTABLEROWS
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            _Struc = New TTABLEROWS
            SetDimensions(1, 1)
        End Sub
        Public Sub New(aCells As dxoTableCells)
            _Struc = New TTABLEROWS
            SetDimensions(1, 1)
            If aCells Is Nothing Then Return
            _Struc = New TTABLEROWS(aCells.Strukture)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend ReadOnly Property CellBoundary As TPLANE
            Get
                CellBoundary = _Struc.CellBounds
                Return CellBoundary
            End Get
        End Property
        Public ReadOnly Property ColumnCount As Integer
            Get
                If _Struc.Count > 0 Then Return _Struc.Row(1).Cells.Count Else Return 0
            End Get
        End Property
        Public Property IsDirty As Boolean
            '^flag indicating that either a cell or row has changed or the collection count has changed
            Get
                Return _Struc.IsDirty
            End Get
            Set(value As Boolean)
                _Struc.IsDirty = value
            End Set
        End Property
        Public ReadOnly Property MaxColumns As Integer
            Get
                '^returns the number of cells in the row with the most cells
                Dim aMem As TTABLEROW
                Dim i As Integer
                Dim _rVal As Integer = 0
                For i = 1 To _Struc.Count
                    aMem = _Struc.Row(i)
                    If aMem.Cells.Count > _rVal Then _rVal = aMem.Cells.Count
                Next i
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property RowCount As Integer
            Get
                Return _Struc.Count
            End Get
        End Property
        Friend Property Strukture As TTABLEROWS
            Get
                Return _Struc
            End Get
            Set(value As TTABLEROWS)
                _Struc = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Function Item(aRowID As Integer) As TTABLEROW
            Return _Struc.Row(aRowID)
        End Function
        Friend Sub SetItem(aRowID As Integer, value As TTABLEROW)
            _Struc.SetRow(value, aRowID)
        End Sub
        Friend Sub Add(aRow As TTABLEROW)
            '#1the row to add
            '^adds the passed row to the array
            'preserve squareness
            aRow.ColumnCount = ColumnCount
            _Struc.Add(aRow)
            'preserve squareness
            If aRow.Cells.Count > _Struc.ColumnCount Then SetColumnCount(aRow.Cells.Count)
            Return
        End Sub
        Public Sub AddByCollection(aData As List(Of String), Optional aDelimiter As String = ",", Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.MiddleCenter)
            '#1the collection containing the delimited strings of data
            '#2the delimiter of the passed string
            '^adds new rows based on the data strings in the passed collection
            If aData Is Nothing Then Return
            If aData.Count <= 0 Then Return
            Dim rowCnt As Integer
            Dim i As Integer
            rowCnt = _Struc.Count
            SetRowCount(rowCnt + TVALUES.To_INT(aData.Count), ColumnCount)
            For i = 1 To aData.Count
                AddRowByString(aData.Item(i), aDelimiter, rowCnt + i, aAlignment)
            Next i
        End Sub
        Public Sub AddRowByCollection(aData As List(Of String), Optional aRowID As Integer = 0, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.MiddleCenter)
            '#1the collection containing the data
            '#2an optional row to update
            '^adds a new row based on the values in the passed collection
            'if the passed row id > 0 then the row at this id is updated with the values of the passed string
            If aData Is Nothing Then Return
            If aData.Count <= 0 Then Return
            Dim i As Integer
            Dim colCnt As Integer = ColumnCount
            If aData.Count > colCnt Then
                colCnt = TVALUES.To_INT(aData.Count)
                SetColumnCount(colCnt)
            End If
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            If aRowID = 0 Then
                aRow = New TTABLEROW(colCnt)
                For i = 1 To aData.Count
                    aCell = aRow.Cell(i)
                    aCell.Data = aData.Item(i)
                    aCell.Alignment = aAlignment
                    aRow.SetCell(aCell, i)
                Next i
                Add(aRow)
            Else
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    For i = 1 To aData.Count
                        aCell = aRow.Cell(i)
                        aCell.Data = aData.Item(i)
                        aRow.SetCell(aCell, i)
                    Next i
                    SetItem(aRowID, aRow)
                End If
            End If
        End Sub
        Public Sub AddRowByString(aData As String, Optional aDelimiter As String = "|", Optional aRowID As Integer = 0, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.MiddleCenter)
            '#1the string containing the data
            '#2the delimiter of the passed string
            '#3an optional row to update
            '^adds a new row based on the values in the passed delimited string
            'if the passed row id > 0 then the row at this id is updated with the values of the passed string
            If String.IsNullOrWhiteSpace(aData) Then Return
            Dim i As Integer
            Dim aRow As TTABLEROW
            Dim colCnt As Integer = ColumnCount
            Dim aCell As TTABLECELL
            Dim aStrs() As String = aData.Split(aDelimiter)
            If aAlignment = dxxRectangularAlignments.General Then aAlignment = dxxRectangularAlignments.MiddleCenter

            If aStrs.Length > colCnt Then colCnt = aStrs.Length
            If aRowID = 0 Then
                aRow = New TTABLEROW(colCnt)
                For i = 0 To aStrs.Length - 1
                    aCell = aRow.Cell(i)
                    aCell.Data = aStrs(i)
                    aCell.Alignment = aAlignment
                    aRow.SetCell(aCell, i)
                Next i
                IsDirty = True
                Add(aRow)
            Else
                aRow = Item(aRowID)
                If aRow.Row > 0 Then

                    Do Until aRow.Cells.Count >= colCnt
                        aRow.Cells.Add(New TTABLECELL(aRow.Row, aRow.Cells.Count) With {.Alignment = aAlignment})
                        IsDirty = True
                    Loop
                    For i = 0 To aStrs.Length - 1
                        aCell = aRow.Cell(i + 1)
                        If aCell.Data <> aStrs(i) Then IsDirty = True
                        aCell.Data = aStrs(i)
                        aCell.Alignment = aAlignment
                        aRow.SetCell(aCell, i + 1)
                    Next i
                    SetItem(aRowID, aRow)
                End If
            End If
            SetColumnCount(colCnt)
        End Sub
        Friend Function Cell(aRowID As Integer, aColID As Integer) As TTABLECELL
            Dim _rVal As TTABLECELL = Nothing
            If aRowID > 0 And aRowID <= _Struc.Count Then
                Dim aRow As TTABLEROW
                aRow = Item(aRowID)
                If aColID > 0 And aColID <= aRow.Cells.Count Then
                    _rVal = aRow.Cell(aColID)
                End If
            End If
            Return _rVal
        End Function
        Public Function Clone() As dxoTableCells
            Return New dxoTableCells(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoTableCells(Me)
        End Function
        Public Function Data(aRowID As Integer, aColID As Integer) As Object
            Return Cell(aRowID, aColID).Data

        End Function
        Public Sub DefineByCollection(aData As List(Of String), Optional aDelimiter As String = "|", Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.MiddleCenter)
            '#1the collection containing the delimited strings of data
            '#2the delimiter of the passed string
            '^adds new rows based on the data strings in the passed collection
            If aData Is Nothing Then Return
            If String.IsNullOrWhiteSpace(aDelimiter) Then aDelimiter = "|" Else aDelimiter = aDelimiter.Trim
            SetRowCount(aData.Count, 0)

            For i As Integer = 1 To aData.Count
                AddRowByString(aData(i - 1), aDelimiter, i, aAlignment)
            Next i
        End Sub
        Public Function RemoveRow(aRow As Integer) As Boolean
            Dim _rVal As Boolean = _Struc.Remove(aRow)
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetAlignment(aAlign As dxxRectangularAlignments, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the alignment to apply
            '#2the row to apply the alignment to
            '#3an optional column ID to limit the application to
            '^applies the passed alignment value to cells of the indicated row
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            If aAlign < 1 Or aAlign > 9 Then Return _rVal
            Dim aMem As TTABLEROW

            If aRowID > 0 And aRowID <= _Struc.Count Then
                aMem = Item(aRowID)
                _rVal = aMem.SetAlignment(aAlign, aColID)
                SetItem(aRowID, aMem)
                If _rVal Then IsDirty = True
                Return _rVal
            End If
            For i As Integer = 1 To _Struc.Count
                aMem = Item(i)
                If aMem.SetAlignment(aAlign, aColID) Then _rVal = True
                SetItem(i, aMem)
            Next i
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Friend Sub SetCell(aCell As TTABLECELL, aRowID As Integer, aColID As Integer)
            If aRowID > 0 And aRowID <= _Struc.Count Then
                Dim aRow As TTABLEROW
                aRow = Item(aRowID)
                If aColID > 0 And aColID <= aRow.Cells.Count Then
                    aRow.SetCell(aCell, aColID)
                    SetItem(aRowID, aRow)
                End If
            End If
        End Sub
        Public Sub SetCellNames(aTextCol As List(Of String), Optional aDelimiter As String = "|")
            '#1a collection of row data strings like 'sadsd|sasdffsdf|1212332'
            '#2the delimeter to seperate the column data
            '^uses the strings in the passed text collection to set the cell names
            '~the passed delimiter seperates the row information in each string
            If aTextCol Is Nothing Then Return
            aDelimiter = Trim(aDelimiter)
            If aDelimiter = "" Then aDelimiter = "|"
            Dim i As Integer
            Dim j As Integer
            Dim aStr As String = String.Empty
            Dim sStr() As String
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            For i = 1 To aTextCol.Count
                If i > _Struc.Count Then Exit For
                aRow = Item(i)
                aStr = aTextCol.Item(i - 1).ToString()
                sStr = aStr.Split(aDelimiter)
                For j = 0 To sStr.Length - 1
                    If j + 1 > aRow.Cells.Count Then Exit For
                    aCell = aRow.Cell(j + 1)
                    aCell.Name = sStr(j)
                    aRow.SetCell(aCell, j + 1)
                Next j
                SetItem(i, aRow)
            Next i
        End Sub
        Public Sub SetCellPrompts(aTextCol As List(Of String), Optional aDelimiter As String = "|")
            '#1a collection of row data strings like 'sadsd|sasdffsdf|1212332'
            '#2the delimeter to seperate the column data
            '^uses the strings in the passed text collection to set the cell names
            '~the passed delimiter seperates the row information in each string
            If aTextCol Is Nothing Then Return
            aDelimiter = aDelimiter.Trim()
            If aDelimiter = "" Then aDelimiter = "|"
            Dim i As Integer
            Dim j As Integer
            Dim aStr As String = String.Empty
            Dim sStr() As String
            Dim aRow As TTABLEROW
            Dim aCell As TTABLECELL
            For i = 1 To aTextCol.Count
                If i > _Struc.Count Then Exit For
                aRow = Item(i)
                aStr = aTextCol.Item(i - 1).ToString()
                sStr = aStr.Split(aDelimiter)
                For j = 0 To sStr.Length - 1
                    If j + 1 > aRow.Cells.Count Then Exit For
                    aCell = aRow.Cell(j + 1)
                    aCell.Prompt = sStr(j)
                    aRow.SetCell(aCell, j + 1)
                Next j
                SetItem(i, aRow)
            Next i
        End Sub
        Public Function SetColumnCount(aColumnCount As Integer) As Boolean
            '#1the desired number of columns
            '^will expand or contract all the member rows so their cell count matches the passed value
            If aColumnCount < 1 Then aColumnCount = 1
            If _Struc.ColumnCount <> aColumnCount Then
                _Struc.ColumnCount = aColumnCount
                Return True
            Else
                Return False
            End If
        End Function
        Public Function SetData(aData As Object, Optional aRowID As Integer = 0, Optional aColID As Integer = 0, Optional bNullCellsOnly As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the data to apply
            '#2the row index to apply the data to
            '#3the column index to apply the data to
            '#4flag to apply the data only to empty cells
            '^puts the passed data value into the cells of the indicated row
            '~a row id of 0 applies the change to all the rows.
            '~a col id of 0 applies the change to all the columns.
            '~returns True if a change occurred
            Dim aMem As TTABLEROW
            If aRowID > 0 And aRowID <= _Struc.Count Then
                aMem = Item(aRowID)
                _rVal = aMem.SetData(aData, aColID, bNullCellsOnly)
                SetItem(aRowID, aMem)
                If _rVal Then IsDirty = True
                Return _rVal
            End If
            For i = 1 To _Struc.Count
                aMem = Item(i)
                If aMem.SetData(aData, aColID, bNullCellsOnly) Then _rVal = True
                SetItem(i, aMem)
            Next i
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetDimensions(aRowCount As Integer, aColCount As Integer) As Boolean
            Dim _rVal As Boolean
            SetRowCount(aRowCount, aColCount)
            Return _rVal
        End Function
        Public Function SetFillColor(aColor As dxxColors, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the fill color to apply
            '#2the row to apply the fill color to
            '#3the column to apply the color to
            '^applies the passed fill color value to cells of the indicated row
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            Dim aRow As TTABLEROW
            Dim i As Integer
            If aRowID <> 0 Then
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    _rVal = aRow.SetFillColor(aColor, aColID)
                    SetItem(aRowID, aRow)
                End If
            Else
                For i = 1 To _Struc.Count
                    aRow = Item(i)
                    If aRow.SetFillColor(aColor, aColID) Then _rVal = True
                    SetItem(i, aRow)
                Next i
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetHeight(aHeight As Double, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the height to apply
            '#2the row to apply the height to
            '#3the column to apply the height to
            '^applies the passed height value to cells of the indicated row
            '~passing row id of 0 applies the passed height to all the rows in the collection
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            Dim aRow As TTABLEROW
            Dim i As Integer
            If aRowID <> 0 Then
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    _rVal = aRow.SetHeight(aHeight, aColID)
                    SetItem(aRowID, aRow)
                End If
            Else
                For i = 1 To _Struc.Count
                    aRow = Item(i)
                    If aRow.SetHeight(aHeight, aColID) Then _rVal = True
                    SetItem(i, aRow)
                Next i
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Friend Sub SetRow(aRow As TTABLEROW, aRowID As Integer)
            If aRowID > 0 And aRowID <= _Struc.Count Then
                Dim colCnt As Integer
                colCnt = ColumnCount
                SetItem(aRowID, aRow)
                If Item(aRowID).Cells.Count <> colCnt And _Struc.Count > 1 Then
                    SetColumnCount(Item(aRowID).Cells.Count)
                End If
            End If
        End Sub
        Public Function SetRowCount(aRowCount As Integer, Optional aColumnCount As Object = Nothing) As Boolean
            '#1the desired number of rows
            '#2the desired number of columns
            '^will expand or contract all the member rows so their cell count matches the passed value
            If aRowCount < 1 Then aRowCount = 1
            If Not TVALUES.IsNumber(aColumnCount) Then aColumnCount = _Struc.Count
            If aColumnCount < 1 Then aColumnCount = 1
            If aRowCount = _Struc.Count And aColumnCount = _Struc.ColumnCount Then Return False
            _Struc.ColumnCount = aColumnCount
            If aRowCount < _Struc.Count Then
                Do Until _Struc.Count = aRowCount
                    _Struc.Remove(_Struc.Count)
                Loop
                Return True
            Else
                Dim aRow As TTABLEROW = _Struc.Row(_Struc.Count).Clone
                aRow.ColumnCount = aColumnCount
                Do Until _Struc.Count = aRowCount
                    _Struc.Add(aRow.Clone)
                Loop
                Return True
            End If
        End Function
        Public Function SetTextAngle(aTextAngle As Double, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the text angle to apply
            '#2the row to apply the text angle to
            '#3the column to apply the text angle to
            '^applies the passed text angle value to cells of the indicated row
            '~passing row id of 0 applies the passed text angle to all the rows in the collection
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            Dim aRow As TTABLEROW
            Dim i As Integer
            If aRowID <> 0 Then
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    _rVal = aRow.SetTextAngle(aTextAngle, aColID)
                    SetItem(aRowID, aRow)
                End If
            Else
                For i = 1 To _Struc.Count
                    aRow = Item(i)
                    If aRow.SetTextAngle(aTextAngle, aColID) Then _rVal = True
                    SetItem(i, aRow)
                Next i
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetTextColor(aColor As dxxColors, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the text color to apply
            '#2the row to apply the text color to
            '#3the column to apply the color to
            '^applies the passed text color value to cells of the indicated row
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            Dim aRow As TTABLEROW
            Dim i As Integer
            If aRowID <> 0 Then
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    _rVal = aRow.SetTextColor(aColor, aColID)
                    SetItem(aRowID, aRow)
                End If
            Else
                For i = 1 To _Struc.Count
                    aRow = Item(i)
                    If aRow.SetTextColor(aColor, aColID) Then _rVal = True
                    SetItem(i, aRow)
                Next i
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetTextScale(aScale As Double, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the text scale to apply
            '#2the row to apply the text scale to
            '#3the column to apply the text scale to
            '^applies the passed text scale value to cells of the indicated row
            '~passing row id of 0 applies the passed text scale to all the rows in the collection
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            Dim i As Integer
            Dim aRow As TTABLEROW
            If aRowID = 0 Then
                For i = 1 To _Struc.Count
                    aRow = Item(i)
                    If aRow.SetTextScale(aScale, aColID) Then _rVal = True
                    SetItem(i, aRow)
                Next i
            Else
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    _rVal = aRow.SetTextScale(aScale, aColID)
                    SetItem(aRowID, aRow)
                End If
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetTextStyle(aStyleName As String, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the text style name to apply
            '#2the row to apply the text style name to
            '#3the column to apply the text style name to
            '^applies the passed text style name value to cells of the indicated row
            '~passing row id of 0 applies the passed text style name to all the rows in the collection
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            Dim aRow As TTABLEROW
            Dim i As Integer
            If aRowID <> 0 Then
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    _rVal = aRow.SetTextStyle(aStyleName, aColID)
                    SetItem(aRowID, aRow)
                End If
            Else
                For i = 1 To _Struc.Count
                    aRow = Item(i)
                    If aRow.SetTextStyle(aStyleName, aColID) Then _rVal = True
                    SetItem(i, aRow)
                Next i
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Public Function SetWidthFactor(aWidthFactor As Double, Optional aRowID As Integer = 0, Optional aColID As Integer = 0) As Boolean
            Dim _rVal As Boolean
            '#1the text width factor to apply
            '#2the row to apply the text width factor to
            '#3the column to apply the text width factor to
            '^applies the passed text width factor value to cells of the indicated row
            '~passing row id of 0 applies the passed text width factor to all the rows in the collection
            '~a row id of 0 applies the change to all the rows
            '~a col id of 0 applies the change to all the columns
            Dim aRow As TTABLEROW
            Dim i As Integer
            If aRowID <> 0 Then
                aRow = Item(aRowID)
                If aRow.Row > 0 Then
                    _rVal = aRow.SetWidthFactor(aWidthFactor, aColID)
                    SetItem(aRowID, aRow)
                End If
            Else
                For i = 1 To _Struc.Count
                    aRow = Item(i)
                    If aRow.SetWidthFactor(aWidthFactor, aColID) Then _rVal = True
                    SetItem(i, aRow)
                Next i
            End If
            If _rVal Then IsDirty = True
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxoTableCells
End Namespace
