Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics

    Public Class dxoStyles
        Inherits dxfTable
        Implements IEnumerable(Of dxoStyle)
        Implements ICloneable

        Public Sub New()
            MyBase.New(dxxReferenceTypes.STYLE)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.STYLE)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.STYLE)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub


        Public Sub New(aTable As dxoStyles)
            MyBase.New(dxxReferenceTypes.STYLE, aTable)
        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoStyles(Me)
        End Function
        Public Function Clone() As dxoStyles
            Return New dxoStyles(Me)
        End Function

        Public Overloads Sub Add(aEntry As dxoStyle)
            AddEntry(aEntry)
        End Sub

        Public Function AddShapeStyle(aShapeFileName As String, Optional aImage As dxfImage = Nothing) As dxoStyle
            Dim _rVal As dxoStyle = Nothing

            Try
                If Not GetImage(aImage) Then Return _rVal
                Dim aShpCol As dxoShapeArray
                Dim aShapes As New TSHAPES
                Dim fname As String
                Dim idx As Integer
                aShpCol = aImage.Shapes
                aShapes = aShpCol.GetShapes(aShapeFileName, idx, True, aImage)
                If idx < 0 Then
                    Throw New Exception("Shape File Not Found")
                Else
                    fname = aShapes.FileName
                    _rVal = GetByPropertyValue(3, fname, aStringCompare:=True, aSecondaryValue:=$"{aShapes.Name}.SHX")

                    If _rVal Is Nothing Then
                        _rVal = New dxoStyle With {.Name = aShapes.Name, .ShapeStyle = True, .FontName = aShapes.FileName}
                        Add(_rVal)
                    Else
                        Return _rVal
                    End If
                    _rVal.ImageGUID = aImage.GUID
                End If
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), Me.GetType(), ex)
            End Try
            Return _rVal
        End Function


        ''' <summary>
        ''' used to add a text style to the table
        ''' </summary>
        ''' <remarks>if the passed font name is not found the default font (arial.ttf) is assigned to the style</remarks>
        ''' <param name="aStyleName"></param>
        ''' <param name="aFontName"></param>
        ''' <param name="aTextHeight"></param>
        ''' <param name="aWidthFactor"></param>
        ''' <param name="aFontStyle"></param>
        ''' <param name="bVertical"></param>
        ''' <param name="bUpsideDown"></param>
        ''' <param name="bBackwards"></param>
        ''' <param name="bMakeCurrent"></param>
        ''' <param name="bOverrideExisting"></param>
        ''' <returns></returns>
        Public Overloads Function Add(aStyleName As String, Optional aFontName As String = "arial.ttf", Optional aTextHeight As Double? = Nothing, Optional aWidthFactor As Double? = Nothing, Optional aFontStyle As String = "", Optional bVertical As Boolean = False, Optional bUpsideDown As Boolean = False, Optional bBackwards As Boolean = False, Optional bMakeCurrent As Boolean = False, Optional bOverrideExisting As Boolean = False) As dxoStyle
            Dim aImage As dxfImage = MyImage
            Dim _rVal As dxoStyle = Nothing

            Dim aTTFStyle As dxxTextStyleFontSettings

            Try
                If String.IsNullOrWhiteSpace(aStyleName) Then Throw New Exception("Invalid Style Name Detected")
                aStyleName = aStyleName.Trim()
                Dim existing As dxoStyle = Nothing

                Dim exists As Boolean = TryGet(aStyleName, existing)
                If exists Then
                    If Not bOverrideExisting Then Throw New Exception($"{TableTypeName}'{aStyleName }' Already Exists")
                    If aWidthFactor.HasValue Then existing.WidthFactor = aWidthFactor
                    If aTextHeight.HasValue Then existing.TextHeight = aTextHeight.Value
                    If bMakeCurrent And aImage IsNot Nothing Then
                        aImage.Header.SetCurrentReferenceName(dxxReferenceTypes.STYLE, existing.Name)
                    End If
                    Return existing
                End If



                _rVal = New dxoStyle(aStyleName) With {.Index = -1}
                Dim fstyl As String = String.Empty

                aFontName = dxfUtils.ThisOrThat(aFontName, "Arial.ttf")
                If String.IsNullOrWhiteSpace(aFontStyle) Then fstyl = String.Empty Else fstyl = aFontStyle.Trim()
                dxoFonts.ParseFontName(aFontName, fstyl, aTTFStyle)


                Dim fInfo As TFONTSTYLEINFO = dxoFonts.GetFontStyleInfo(aFontName, aStyleName:=fstyl, bReturnDefault:=True, aTTFStyle:=aTTFStyle)
                _rVal.UpdateFontName(fInfo.FileName, fInfo.StyleName)
                If Not fInfo.IsShape Then
                    _rVal.PropValueSet(dxxStyleProperties.VERTICAL, False)
                    _rVal.PropValueSet(dxxStyleProperties.UPSIDEDOWN, False)
                Else
                    _rVal.PropValueSet(dxxStyleProperties.VERTICAL, bVertical)
                    _rVal.PropValueSet(dxxStyleProperties.UPSIDEDOWN, bUpsideDown)
                End If

                If aWidthFactor.HasValue Then _rVal.WidthFactor = aWidthFactor
                If aTextHeight.HasValue Then _rVal.TextHeight = aTextHeight.Value
                _rVal = AddToCollection(_rVal, bOverrideExisting:=False, bSetCurrent:=bMakeCurrent)
            Catch ex As Exception
                If aImage IsNot Nothing Then
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    Throw ex
                End If
            End Try
            Return _rVal
        End Function
        Public Function Member(aNameOrHandleOrGUID As String) As dxoStyle
            Dim mem As dxoStyle = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoStyle) As Boolean
            rMember = Nothing
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then Return False
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoStyle)
            Return True
        End Function

        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoStyle) Implements IEnumerable(Of dxoStyle).GetEnumerator
            Return DirectCast(Me, IEnumerable).GetEnumerator()
        End Function
    End Class

End Namespace