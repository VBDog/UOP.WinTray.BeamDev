Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxfBlockSource
        Private _DefinedBlocks As New List(Of List(Of dxfFileObject))
        Private _Struc As New TBUFFERS()
        Private tFileBuffer As TBUFFER


        Private _Files As dxfFiles

        Public Sub New()
            _Files = New dxfFiles
        End Sub




        Public Function FileNames(Optional bReturnFullPaths As Boolean = False) As String

            Dim _rVal As String = String.Empty
            For Each file As dxfFile In _Files
                Dim fname As String = file.FileSpec
                If fname <> "" Then
                    If Not bReturnFullPaths Then
                        fname = IO.Path.GetFileName(fname)
                    End If
                    TLISTS.Add(_rVal, fname)
                End If

            Next


            Return _rVal

        End Function


        Public Function GetBlock(aFileName As String, aBlockName As String, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False) As List(Of dxfBlock)
            '#1the source file name
            '#2the name of the block to retrieve

            '^returns the block and it's sub blocks from the indicated source file
            '~if the blocks haven't been read from the source yet they are here

            Dim _rVal As List(Of dxfFileObject) = zGetBlocks(aFileName, aBlockName, -1, 0, True, rErrorString, True)

            If rErrorString <> "" And Not bSuppressErrors Then
                Throw New Exception(rErrorString)

            End If
            Return New List(Of dxfBlock)
        End Function

        Private Function zGetBlocks(aFileName As String, aBlockName As String, aBufferID As Integer, aColID As Integer, bReturnClone As Boolean, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False) As List(Of dxfFileObject)

            rErrorString = ""
            Dim _rVal As List(Of dxfFileObject) = Nothing
            If String.IsNullOrWhiteSpace(aBlockName) Then Return Nothing


            Dim aBls As List(Of dxfFileObject)


            Try
                If aBufferID < 0 Then
                    aBufferID = _Files.FindIndex(Function(x) String.Compare(x.FileName, aFileName, True) + 1)
                End If
                If aBufferID < 0 Then Return Nothing

                aBlockName = aBlockName.Trim()


                If aColID > 0 And aColID < _DefinedBlocks.Count Then
                    aBls = _DefinedBlocks.Item(aColID - 1)
                    If aBls.Count > 0 Then
                        Dim aBl As dxfFileObject = aBls(0)
                        If String.Compare(aBl.Name, aBlockName, True) = 0 Then
                            _rVal = aBls
                        Else
                            aColID = 0
                        End If
                    Else
                        _DefinedBlocks.RemoveAt(aColID)
                        aColID = 0
                    End If

                End If

                If _rVal Is Nothing Then
                    For i As Integer = _DefinedBlocks.Count To 1 Step -1

                        aBls = _DefinedBlocks.Item(i - 1)
                        If aBls.Count > 0 Then
                            Dim aBl As dxfFileObject = aBls(0)
                            If String.Compare(aBl.Name, aBlockName, True) = 0 Then
                                _rVal = aBls
                                aColID = i
                            End If
                        Else
                            _DefinedBlocks.RemoveAt(i)
                        End If
                    Next i


                End If

                Dim aGen As New dxoFileTool
                Dim bllist As String = String.Empty
                If _rVal Is Nothing And aBufferID > 0 And aBufferID <= _Files.Count Then

                    Dim aFile As dxfFile = _Files.Item(aBufferID)

                    If Not aFile.Loaded Then
                        rErrorString = aFile.Read(aFile.FileSpec, False)
                        If rErrorString <> "" Then
                            If Not bSuppressErrors Then Return Nothing
                            Throw New Exception(rErrorString)
                        End If
                    End If

                    aBls = aFile.GetBlocks(aBlockName, rErrorString)
                    If rErrorString <> "" Then
                        If Not bSuppressErrors Then Throw New Exception(rErrorString)
                        Return Nothing
                    End If

                    _DefinedBlocks.Add(aBls)
                    aColID = _DefinedBlocks.Count
                    _rVal = aBls

                End If



            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw ex
            End Try

            Return _rVal

        End Function


        Public Function AddSourceFile(aFileSpec As String, bLoadBlocks As Boolean, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False, Optional bReloadIfFound As Boolean = False) As Boolean
            rErrorString = ""
            If String.IsNullOrWhiteSpace(aFileSpec) Then Return False

            aFileSpec = aFileSpec.Trim()


            Dim idx As Integer
            Dim aBuffer As dxfFile = _Files.GetByFileName(aFileSpec, idx)
            Dim aExt As String
            Try
                If idx >= 0 And bReloadIfFound Then
                    If IO.File.Exists(aFileSpec) Then
                        _Files.Remove(aBuffer)
                        idx = -1
                    End If
                End If

                If idx < 0 Then


                    If Not IO.File.Exists(aFileSpec) Then
                        rErrorString = $"File Not Found '{ aFileSpec }'"
                        If Not bSuppressErrors Then Throw New Exception(rErrorString) Else Return False

                    End If

                    aExt = IO.Path.GetExtension(aFileSpec).ToUpper

                    If aExt = ".DXF" Then
                        aBuffer = New dxfFile(aFileSpec)
                        _Files.Add(aBuffer)
                        idx = _Struc.Count

                    Else
                        rErrorString = $"Only Files With the Extension DXF can Be Added"
                        If Not bSuppressErrors Then Throw New Exception(rErrorString) Else Return False

                    End If


                End If
                Dim _rVal As Boolean = _Files.GetByFileName(aFileSpec, idx) IsNot Nothing
                If bLoadBlocks And _rVal Then

                    BufferLoad(aFileSpec, idx, rErrorString, True)
                    If Not bSuppressErrors Then Throw New Exception(rErrorString) Else Return False

                End If

                Return _rVal

            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw ex
            End Try



            Return idx > 0

        End Function

        Private Function BufferLoad(aFileSpec As String, aIndex As Integer, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False) As dxfFile
            rErrorString = ""

            Dim idx As Integer
            Dim _rVal As dxfFile = Nothing

            Try
                If aIndex >= 1 And aIndex <= _Files.Count Then
                    idx = aIndex
                    _rVal = _Files.Item(aIndex)
                Else
                    If aFileSpec <> "" Then
                        _rVal = _Files.GetByFileName(aFileSpec, idx)
                    End If
                End If
                If _rVal Is Nothing Then Return _rVal
                If _rVal.Loaded Then Return _rVal

                rErrorString = _rVal.Read(_rVal.FileSpec, rErrorString)

                If rErrorString <> "" Then
                    If Not bSuppressErrors Then Throw New Exception(rErrorString) Else Return _rVal
                End If



            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw ex
                Return _rVal

            End Try
            Return _rVal


        End Function
        Public Sub ClearExistingBlocks(Optional aBlockName As String = "")
            If String.IsNullOrWhiteSpace(aBlockName) Then
                _DefinedBlocks = New List(Of List(Of dxfFileObject))
                _Files.Clear()
            Else
                If _DefinedBlocks IsNot Nothing Then
                    Dim rmv As List(Of dxfFileObject) = Nothing
                    For Each bls As List(Of dxfFileObject) In _DefinedBlocks
                        Dim blk As dxfFileObject = bls.Find(Function(x) String.Compare(x.Name, aBlockName, True) = 0)
                        If blk IsNot Nothing Then
                            rmv = bls
                            Exit For
                        End If
                    Next


                    If rmv IsNot Nothing Then
                        _DefinedBlocks.Remove(rmv)
                    End If
                End If
            End If

        End Sub
        Public Function FileExists(aFileName As String, ByRef rIndex As Integer) As Boolean
            rIndex = -1
            If String.IsNullOrWhiteSpace(aFileName) Then Return False


            Dim aBuffer As dxfFile = _Files.GetByFileName(aFileName, rIndex)
            Return rIndex > 0

        End Function

        Public Function FileExists(aFileName As String) As Boolean
            Dim rIndex As Integer = -1
            Return FileExists(aFileName, rIndex)
        End Function

        Public Function FileIsLoaded(aFileName As String) As Boolean
            If Not FileExists(aFileName) Then Return False
            aFileName = aFileName.Trim()
            Dim idx As Integer = 0
            Return _Files.GetByFileName(aFileName, idx).Loaded
        End Function
        Public Function FileIsLoaded(aFileIndex As Integer) As Boolean
            If aFileIndex < 1 Or aFileIndex > _Files.Count Then Return False
            Return _Files.Item(aFileIndex).Loaded
        End Function

        Public Function FileName(aFileIndex As Integer) As String
            If aFileIndex < 1 Or aFileIndex > _Files.Count Then Return String.Empty
            Return _Files.Item(aFileIndex).FileName
        End Function

        Public Sub LoadFile(aFileSpec As String, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False)
            rErrorString = ""
            aFileSpec = Trim(aFileSpec)
            If aFileSpec = "" Then Return


            Dim idx As Integer
            Dim aBuffer As dxfFile

            Try
                If FileExists(aFileSpec, idx) Then

                    aBuffer = _Files.Item(idx)
                    If Not aBuffer.Loaded Then
                        Dim aGen As New dxoFileTool
                        Dim blist As String = String.Empty
                        rErrorString = aBuffer.Read(aBuffer.FileSpec, True)
                        If rErrorString <> "" Then

                            If Not bSuppressErrors Then Throw New Exception(aBuffer.ErrorString)
                        End If


                    End If

                Else
                    AddSourceFile(aFileSpec, True, rErrorString, True)

                    If rErrorString <> "" Then
                        If Not bSuppressErrors Then Throw New Exception(rErrorString)

                    End If
                End If

            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw ex
            End Try



        End Sub

        Public Sub LoadFile(aFileIndex As Integer, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False)
            rErrorString = ""

            If aFileIndex < 1 Or aFileIndex > _Files.Count Then
                rErrorString = $"The Passed Index '{aFileIndex}' Is Not Valid"
                Return
            End If


            Dim aBuffer As dxfFile

            Try

                aBuffer = _Files.Item(aFileIndex)
                If Not aBuffer.Loaded Then
                    Dim aGen As New dxoFileTool
                    Dim blist As String = String.Empty
                    rErrorString = aBuffer.Read(aBuffer.FileSpec, True)
                    If rErrorString <> "" Then

                        If bSuppressErrors Then Return
                        Throw New Exception(aBuffer.ErrorString)
                    End If


                End If

            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw ex
            End Try



        End Sub

        Public Function LoadFolderFiles(aFolderSpec As String, Optional bLoadBlocks As Boolean = False, Optional bSuppressDXF As Boolean = False, Optional bSuppressDWG As Boolean = False) As String



            If String.IsNullOrWhiteSpace(aFolderSpec) Then Return String.Empty
            If bSuppressDXF And bSuppressDWG Then Return String.Empty
            aFolderSpec = aFolderSpec.Trim()
            If Not IO.Directory.Exists(aFolderSpec) Then Return String.Empty

            Dim bKeep As Boolean
            Dim f1 As String
            Dim aErr As String = String.Empty
            Dim exts() As String = {"*.dxf"}
            Dim fnames As New List(Of String)
            For Each foundFile As String In My.Computer.FileSystem.GetFiles(aFolderSpec, FileIO.SearchOption.SearchAllSubDirectories)
                If String.Compare(IO.Path.GetExtension(foundFile), ".dxf", True) = 0 Then
                    fnames.Add(foundFile)
                End If

            Next


            If fnames.Count <= 0 Then Return String.Empty

            Dim _rVal As String = String.Empty

            Try

                For Each f1 In fnames
                    If f1.EndsWith(".DXF", comparisonType:=StringComparison.OrdinalIgnoreCase) Then
                        bKeep = True
                        If bSuppressDXF Then
                            If String.Compare(Right(f1, 4), ".DXF", True) = 0 Then
                                bKeep = False
                            End If
                        End If
                        If bSuppressDWG Then
                            If String.Compare(Right(f1, 4), ".DWG", True) = 0 Then
                                bKeep = False
                            End If
                        End If

                        If bKeep Then
                            If AddSourceFile(f1, bLoadBlocks, aErr, True) Then
                                TLISTS.Add(_rVal, IO.Path.GetFileName(f1), bAllowDuplicates:=True)
                            End If
                        End If


                    End If


                Next f1
            Catch ex As Exception

            End Try

            Return _rVal
        End Function

        Private Function xBlockExists(aFileName As String, aBlockName As String, ByRef rBufferIndex As Integer, ByRef rBlocksIndex As Integer) As Boolean
            rBufferIndex = -1
            rBlocksIndex = 0
            If String.IsNullOrWhiteSpace(aFileName) Or String.IsNullOrWhiteSpace(aBlockName) Then Return False
            aFileName = aFileName.Trim()
            aBlockName = aBlockName.Trim()

            Dim aBuffer As dxfFile = _Files.Find(Function(x) String.Compare(x.FileName, aFileName, True) = 0)
            If aBuffer Is Nothing Then Return False
            rBufferIndex = _Files.IndexOf(aBuffer) + 1

            Dim _rVal As Boolean = aBuffer.BlockNames.FindIndex(Function(x) String.Compare(x, aBlockName, True) = 0) >= 0
            If _rVal Then
                'rBlocksIndex = aBuffer.Ge

                'For i As Integer = _DefinedBlocks.Count To 1 Step -1
                '    Dim bCol As List(Of dxfFileObject) = _DefinedBlocks.Item(i)
                '    If bCol.Count > 0 Then
                '        Dim aBlk As dxfBlock = bCol.Item(0)
                '        If String.Compare(aBlk.Name, aBlockName, True) = 0 Then
                '            rBlocksIndex = i
                '            Exit For
                '        End If
                '    Else
                '        _DefinedBlocks.RemoveAt(i)
                '    End If
                'Next i
            End If

            Return _rVal

        End Function

        ''' <summary>
        ''' transfers the requested block form this source to the passed image
        ''' </summary>
        ''' <param name="aImage">the target image</param>
        ''' <param name="aSourceFile">the name of the file to get the block from</param>
        ''' <param name="blockockName">the block to retrieve</param>
        ''' <param name="bSuppressReferences">a flag to prevent the transfering layers etc. that the block references from the fime to the passed image</param>
        ''' <param name="bConvertAttribsToText">a flag to convert any attributes definitions in the text</param>
        ''' <param name="rErrorString">returns an erro string if an exception is encountered</param>
        ''' <param name="bSuppressErrors">a flag to pass any exceptions to the image for handling or to throw them </param>
        ''' <returns></returns>
        Public Function TransferBlock(aImage As dxfImage, aSourceFile As String, blockockName As String, bSuppressReferences As Boolean, bConvertAttribsToText As Boolean, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False) As dxfBlock

            Dim _rVal As dxfBlock = Nothing
            rErrorString = ""

            If String.IsNullOrWhiteSpace(aSourceFile) Or String.IsNullOrWhiteSpace(blockockName) Then Return Nothing
            If aImage Is Nothing Then Return Nothing

            aSourceFile = aSourceFile.Trim()
            blockockName = blockockName.Trim()
            Dim fidx As Integer
            Dim blkidx As Integer
            Dim aFile As dxfFile = Nothing
            Dim blockobjs As List(Of dxfFileObject)


            Try
                If Not xBlockExists(aSourceFile, blockockName, fidx, blkidx) Then
                    rErrorString = $"The File '{ aSourceFile }' Is Not A Loaded Source File"
                    Return Nothing
                End If


                aFile = _Files.Item(fidx)
                blkidx = aFile.Blocks.SubObjects.FindIndex(Function(x) String.Compare(x.Name, blockockName, True) = 0) + 1
                If blkidx <= 0 Then
                    rErrorString = $"The Block '{ blockockName }' Is Not Found In The Indicated Source File '{ aSourceFile }'"
                    Return Nothing

                End If

                blockobjs = zGetBlocks(aSourceFile, blockockName, fidx, blkidx, False, rErrorString, True)

                If Not String.IsNullOrWhiteSpace(rErrorString) Then Return Nothing

                If blockobjs Is Nothing Then Return Nothing

                If blockobjs.Count <= 0 Then Return Nothing





                Dim bnames As String = String.Empty

                For Each obj In blockobjs
                    If Not String.IsNullOrWhiteSpace(bnames) Then bnames += ","
                    bnames += obj.Name
                Next

                Dim dxfblocks As List(Of dxfBlock) = aFile.TransferToImage_Blocks(aImage, bNewMembersOnly:=True, bnames, bConvertAttribsToText:=bConvertAttribsToText, bSuppressReferences:=bSuppressReferences)
                _rVal = dxfblocks.LastOrDefault()
                Dim tranfererrs As List(Of String) = aFile.ErrorStrings
                rErrorString = tranfererrs.FirstOrDefault

                Dim appIds As New List(Of String)
                Dim layernames As New List(Of String)
                Dim stylenames As New List(Of String)
                Dim dstylenames As New List(Of String)
                Dim ltypenames As New List(Of String)
                For Each block In dxfblocks
                    block.ImageGUID = aImage.GUID
                    If Not bSuppressReferences Then
                        If Not layernames.Contains(block.LayerName) Then layernames.Add(block.LayerName)
                    End If
                    For Each ent As dxfEntity In block.Entities

                        'save extended data appids
                        Dim xData As dxoPropertyArray = ent.ExtendedData
                        For k As Integer = 1 To xData.Count
                            If Not appIds.Contains(xData.Item(k).Name) Then appIds.Add(xData.Item(k).Name)
                        Next k
                        If Not bSuppressReferences Then

                            If Not String.IsNullOrWhiteSpace(ent.LayerName) And Not layernames.Contains(ent.LayerName) Then layernames.Add(ent.LayerName)
                            If Not String.IsNullOrWhiteSpace(ent.TextStyleName) And Not stylenames.Contains(ent.TextStyleName) Then stylenames.Add(ent.TextStyleName)
                            If Not String.IsNullOrWhiteSpace(ent.DimStyleName) And Not dstylenames.Contains(ent.DimStyleName) Then dstylenames.Add(ent.DimStyleName)
                            If Not String.IsNullOrWhiteSpace(ent.Linetype) And Not ltypenames.Contains(ent.Linetype) Then ltypenames.Add(ent.Linetype)



                        End If
                    Next
                Next



                For Each block In dxfblocks
                    block.ImageGUID = aImage.GUID
                    If _rVal Is Nothing Then
                        _rVal = aImage.Blocks.Add(block)
                    Else
                        aImage.Blocks.Add(block)
                    End If

                Next

                tranfererrs = aFile.TransferToImage_References(aImage, aLTNamesOrHandlesList:=ltypenames, aStyleNamesOrHandlesList:=stylenames, aDimStyleNamesOrHandlesList:=dstylenames, aLayersNamesOrHandlesList:=layernames, aAPPIDNamesOrHandlesList:=appIds, bOverrideExising:=False)



            Catch ex As Exception
                rErrorString = ex.Message
                _rVal = Nothing

            Finally
                If rErrorString <> "" Then
                    rErrorString = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod(), "dxfBlockSource", New Exception(rErrorString))
                    If Not bSuppressErrors Then Throw New Exception(rErrorString)
                End If
                _rVal = aImage.Blocks.Find(Function(x) String.Compare(x.Name, blockockName, True) = 0)
            End Try

            Return _rVal


        End Function


    End Class

End Namespace
