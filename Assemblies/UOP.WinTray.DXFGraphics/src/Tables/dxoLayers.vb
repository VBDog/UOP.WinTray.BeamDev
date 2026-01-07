Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Class dxoLayers
        Inherits dxfTable
        'Implements IEnumerable(Of dxoLayer)
        Implements ICloneable

        Public Sub New()
            MyBase.New(dxxReferenceTypes.LAYER)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean)
            MyBase.New(dxxReferenceTypes.LAYER)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me)
        End Sub

        Public Sub New(bAddDefaultMembers As Boolean, aImage As dxfImage)
            MyBase.New(dxxReferenceTypes.LAYER)
            If bAddDefaultMembers Then dxfTable.LoadDefaultMembers(Me, aImage)
        End Sub

        Public Sub New(aTable As dxoLayers)
            MyBase.New(dxxReferenceTypes.LAYER, aTable)
        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxoLayers(Me)
        End Function
        Public Function Clone() As dxoLayers
            Return New dxoLayers(Me)
        End Function

        Public Overloads Sub Add(aEntry As dxoLayer)
            AddEntry(aEntry)
        End Sub

        Public Function Member(aNameOrHandleOrGUID As String) As dxoLayer
            Dim mem As dxoLayer = Nothing
            If Not TryGet(aNameOrHandleOrGUID, mem) Then Return Nothing Else Return mem
        End Function
        ''' <summary>
        ''' used to add a new entry to current table
        ''' </summary>
        ''' <param name="aLayerName">the name to apply to the new member</param>
        ''' <param name="aColor">the ACL color to apply to the new member</param>
        ''' <param name="aLinetype">the linetype to apply to the new member</param>
        ''' <param name="bVisible"></param>
        ''' <param name="aLineWeight"></param>
        ''' <param name="bMakeCurrent">flag to make the name style the current active style in the parent image</param>
        ''' <param name="bOverrideExisting">flag to update the existing style by the given name to the passed properties</param>
        ''' <returns></returns>
        Public Overloads Function Add(aLayerName As String, Optional aColor As dxxColors? = Nothing, Optional aLinetype As String = "Continuous", Optional bVisible As Boolean? = Nothing, Optional aLineWeight As dxxLineWeights? = Nothing, Optional bMakeCurrent As Boolean = False, Optional bOverrideExisting As Boolean = True) As dxoLayer

            Dim aImage As dxfImage = MyImage
            Try

                If String.IsNullOrWhiteSpace(aLayerName) Then
                    Throw New Exception("Invalid Name Detected")
                End If
                aLayerName = aLayerName.Trim()

                Dim existing As dxoLayer = Nothing
                Dim exists As Boolean = TryGet(aLayerName, existing)
                If exists Then
                    If Not bOverrideExisting Then Throw New Exception($"{TableTypeName}'{aLayerName }' Already Exists")
                    If aColor.HasValue Then
                        If Not aColor.Value.IsLogical() Then existing.Color = aColor.Value
                    End If
                    If Not String.IsNullOrWhiteSpace(aLinetype) Then
                        aLinetype = aLinetype.Trim()
                        If String.Compare(aLinetype, dxfLinetypes.ByLayer, True) <> 0 Or String.Compare(aLinetype, dxfLinetypes.ByBlock, True) <> 0 Then
                            If aImage IsNot Nothing Then aLinetype = aImage.GetOrAddReference(aLinetype, dxxReferenceTypes.LTYPE).Name
                            existing.Linetype = aLinetype
                        End If
                    End If
                    If aLineWeight.HasValue Then
                        If Not aLineWeight.Value.IsLogical() Then
                            existing.LineWeight = aLineWeight.Value
                        End If
                    End If
                    If bVisible.HasValue Then
                        existing.Visible = bVisible.Value
                    End If

                    If bMakeCurrent And aImage IsNot Nothing Then
                        aImage.Header.SetCurrentReferenceName(dxxReferenceTypes.LAYER, existing.Name)
                    End If
                    Return existing
                End If

                If Not String.IsNullOrWhiteSpace(aLinetype) Then aLinetype = aLinetype.Trim() Else aLinetype = dxfLinetypes.Continuous

                If String.Compare(aLinetype, dxfLinetypes.ByLayer, True) = 0 Or String.Compare(aLinetype, dxfLinetypes.ByBlock, True) = 0 Then
                    Throw New Exception($"Invalid Linetype passed.  layers cannot have logical linetypes assigned")
                Else
                    If aImage IsNot Nothing Then aLinetype = aImage.GetOrAddReference(aLinetype, dxxReferenceTypes.LTYPE).Name
                End If
                If Not aLineWeight.HasValue Then aLineWeight = dxxLineWeights.ByDefault


                If aLineWeight.Value <> dxxLineWeights.ByDefault Then
                    If aLineWeight.Value.IsLogical() Then Throw New Exception($"Invalid Lineweight passed. Layers cannot have logical lineweights assigned")
                End If

                If Not aColor.HasValue Then aColor = dxxColors.BlackWhite
                If aColor.Value.IsLogical() Then
                    Throw New Exception($"Invalid Color passed. Layers cannot have logical colors assigned")
                End If
                Dim aEntry As New dxoLayer(aLayerName)
                If Not bVisible.HasValue Then bVisible = True

                If Not bVisible.Value Then aColor = -1 * aColor.Value
                aEntry.PropValueSet(dxxLayerProperties.Color, aColor.Value, bSuppressEvnts:=True)
                aEntry.PropValueSet(dxxLayerProperties.Linetype, aLinetype, bSuppressEvnts:=True)
                aEntry.PropValueSet(dxxLayerProperties.LineWeight, aLineWeight.Value, bSuppressEvnts:=True)
                Return DirectCast(AddToCollection(aEntry, bOverrideExisting:=bOverrideExisting, bSetCurrent:=bMakeCurrent, ioImage:=aImage), dxoLayer)
            Catch ex As Exception
                If aImage IsNot Nothing Then
                    aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    Throw ex
                End If
                Return Nothing
            End Try
        End Function

        Public Function TryGet(aNameOrHandleOrGUID As String, ByRef rMember As dxoLayer) As Boolean

            rMember = Nothing
            If String.IsNullOrWhiteSpace(aNameOrHandleOrGUID) Then Return False Else aNameOrHandleOrGUID = aNameOrHandleOrGUID.Trim()
            Dim entry As dxfTableEntry = Nothing
            If Not TryGetEntry(aNameOrHandleOrGUID, entry) Then
                If String.Compare(aNameOrHandleOrGUID, "DefPoints", True) = 0 Then
                    entry = AddToCollection(New dxoLayer("Defpoints") With {.PlotFlag = False})
                ElseIf String.Compare(aNameOrHandleOrGUID, "0", True) = 0 Then
                    entry = AddToCollection(New dxoLayer("0"))
                Else
                    Return False
                End If

            End If
            If entry.EntryType <> TableType Then
                Remove(entry)
                Return False
            End If
            rMember = DirectCast(entry, dxoLayer)
            Return True
        End Function

        Public Overrides Function ToString() As String
            Return $"{TableTypeName} [{Count}]"
        End Function

        Public Function GetOrAdd(aName As String, Optional aColor As dxxColors? = Nothing, Optional aLineType As String = Nothing, Optional aLineWeigth As dxxLineWeights? = Nothing, Optional bSuppressNew As Boolean = False, Optional bUpdateIfFound As Boolean = False) As dxoLayer
            If String.IsNullOrWhiteSpace(aName) Then Return Nothing

            Dim existing As dxoLayer = Find((Function(x) String.Compare(x.Name, aName, True) = 0))
            Dim rFound As Boolean = existing IsNot Nothing
            If rFound And Not bUpdateIfFound Then Return existing
            Dim myImage As dxfImage = Nothing
            Dim haveimage = GetImage(myImage)
            If rFound Then
                If aColor.HasValue Then
                    If Not aColor.Value.IsLogical() Then
                        existing.Color = aColor.Value
                    End If
                End If
                If Not String.IsNullOrWhiteSpace(aLineType) Then
                    aLineType = aLineType.Trim()
                    If haveimage Then
                        aLineType = myImage.GetOrAddReference(aLineType, dxxReferenceTypes.LTYPE).Name
                    End If

                    If Not dxoLinetype.LTIsLogical(aLineType) Then existing.Linetype = aLineType
                End If

                If (aLineWeigth.HasValue) Then
                    If Not aLineWeigth.Value.IsLogical() Then existing.LineWeight = aLineWeigth.Value
                End If
                Return existing
            Else
                If Not aColor.HasValue Then
                    aColor = dxxColors.BlackWhite
                Else
                    If aColor.Value.IsLogical() Then
                        aColor = dxxColors.BlackWhite
                    End If
                End If
                If Not String.IsNullOrWhiteSpace(aLineType) Then
                    aLineType = aLineType.Trim()
                    If dxoLinetype.LTIsLogical(aLineType) Then aLineType = dxfLinetypes.Continuous
                    If haveimage Then
                        aLineType = myImage.GetOrAddReference(aLineType, dxxReferenceTypes.LTYPE).Name
                    End If
                Else
                    aLineType = dxfLinetypes.Continuous
                End If

                If (aLineWeigth.HasValue) Then
                    If aLineWeigth.Value.IsLogical() Then aLineWeigth = dxxLineWeights.ByDefault
                Else
                    aLineWeigth = dxxLineWeights.ByDefault
                End If
                Return DirectCast(AddToCollection(New dxoLayer(aName) With {.Color = aColor.Value, .Linetype = aLineType, .LineWeight = aLineWeigth.Value}), dxoLayer)

            End If
        End Function
        'Private Function IEnumerable_GetEnumerator() As IEnumerator(Of dxoLayer) Implements IEnumerable(Of dxoLayer).GetEnumerator
        '    Return DirectCast(Me, IEnumerable).GetEnumerator()
        'End Function
    End Class

End Namespace