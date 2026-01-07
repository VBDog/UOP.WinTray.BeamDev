

Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoLeaderTool
#Region "Types"
        Friend Structure TVECTORPAIR
#Region "Members"
            Public Index As Integer
            Public Point1 As TVECTOR
            Public Point2 As TVECTOR
            Public Span As Double
            Public Tag1 As String
            Public Tag2 As String
            Public Tag3 As String
#End Region 'Members
#Region "Constructors"
#End Region 'Constructors
        End Structure 'TVECTORPAIR
        Friend Structure TPAIRS
#Region "Members"
            Public Count As Integer
            Public Members() As TVECTORPAIR
#End Region 'Members
#Region "Constructors"
#End Region 'Constructors
        End Structure 'TPAIRS
#End Region 'Types
#Region "Members"
        Private gInput As TDIMINPUT
        Private eStructure As TDISPLAYVARS
        Private _ImageGUID As String
        Friend ImagePtr As WeakReference
        Private _Status As String
        Private _LastStatus As String
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage)
            If aImage IsNot Nothing Then
                _ImageGUID = aImage.GUID
                ImagePtr = New WeakReference(aImage)
            End If
            _Status = ""
            _LastStatus = ""
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then _ImageGUID = "" Else _ImageGUID = value.Trim
                If ImagePtr Is Nothing And Not String.IsNullOrWhiteSpace(_ImageGUID) Then
                    Dim img As dxfImage = dxfEvents.GetImage(_ImageGUID)
                    If img IsNot Nothing Then ImagePtr = New WeakReference(img)
                End If
            End Set
        End Property
        Public Property Status As String
            Get
                Return _Status

            End Get
            Set(value As String)
                If _Status <> value Then
                    _LastStatus = _Status
                    _Status = value
                End If
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage Is Nothing Then
                Dim img As dxfImage
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then
                    If ImagePtr IsNot Nothing Then
                        If ImagePtr.IsAlive Then
                            img = TryCast(ImagePtr.Target, dxfImage)
                            If img IsNot Nothing Then
                                rImage = img
                                Return True
                            End If
                        End If
                    End If
                    rImage = dxfEvents.GetImage(ImageGUID)
                    If rImage IsNot Nothing Then
                        ImagePtr = New WeakReference(rImage)
                        Return True
                    End If
                End If
            Else
                _ImageGUID = rImage.GUID
                ImagePtr = New WeakReference(rImage)
            End If
            Return rImage IsNot Nothing
        End Function
        Private Function xCreateLeaderVertices(aStartPTOrVertCol As IEnumerable(Of iVector), aMidXY As iVector, aTerminusXY As iVector, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane, Optional aImage As dxfImage = Nothing) As TVECTORS
            rPlane = Nothing


            If Not GetImage(aImage) Then Return New TVECTORS(0)
            Dim _rVal As TVECTORS = aImage.CreateUCSVectors(aStartPTOrVertCol, bSuppressUCS, bSuppressElevation, rPlane, False)

            If aMidXY IsNot Nothing Then
                _rVal.Add(New TVECTOR(rPlane, aMidXY))

            End If
            If aTerminusXY IsNot Nothing Then
                _rVal.Add(New TVECTOR(rPlane, aTerminusXY))
            End If
            _rVal.RemoveCoincidentVectors(3)
            If _rVal.Count <= 0 Then _rVal.Add(rPlane.Origin)
            If _rVal.Count <= 1 Then _rVal.Add(rPlane.VectorPolar(1, 45))
            rPlane.OriginV = _rVal.Item(1)
            Return _rVal
        End Function

        Private Function xCreateLeaderVertices(aStartPTXY As iVector, aMidXY As iVector, aTerminusXY As iVector, bSuppressUCS As Boolean, bSuppressElevation As Boolean, ByRef rPlane As dxfPlane, Optional aImage As dxfImage = Nothing) As TVECTORS
            rPlane = New dxfPlane()

            Dim v1 As TVECTOR = aImage.CreateUCSVector(aStartPTXY, bSuppressUCS, bSuppressElevation, rPlane)
            If Not GetImage(aImage) Then Return New TVECTORS(0)
            Dim _rVal As New TVECTORS(v1)

            If aMidXY IsNot Nothing Then
                _rVal.Add(New TVECTOR(rPlane, aMidXY))

            End If
            If aTerminusXY IsNot Nothing Then
                _rVal.Add(New TVECTOR(rPlane, aTerminusXY))
            End If
            _rVal.RemoveCoincidentVectors(3)
            If _rVal.Count <= 0 Then _rVal.Add(rPlane.Origin)
            If _rVal.Count <= 1 Then _rVal.Add(rPlane.VectorPolar(1, 45))
            rPlane.OriginV = _rVal.Item(1)
            Return _rVal
        End Function

        Public Function NoRef(aStartPTXY As iVector, aTerminalPt As iVector, Optional aMidXY As iVector = Nothing, Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1, Optional aArrowBlock As String = "", Optional bSuppressHook As Boolean = False, Optional aDimStyle As String = "", Optional aTextStyle As String = "", Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLeader
            Dim _rVal As dxeLeader = Nothing
            If Not GetImage(aImage) Then Return Nothing
            Dim rErrString As String = String.Empty
            '#1the start point of the requested leader or a collection of leader vertices
            '#2the end point of the requested leader
            '#3an optional point to to add an additional vertex to the leader line
            '#4a arrow head type for the leader lines
            '#5the size for the arrowhead
            '#6the block to use if the arrow head type is user defined
            '#7flag to suppress the leader hooks
            '#8the dim style for the leader other than the current dim style
            '#9the text style to use for the text rather than the current text style
            '#10the layer to put the entity on instead of the current leader layer setting
            '#11a color to apply to the entity instead of the current color
            '#12a linetype to apply to the entity instead of the current linetype setting
            '^used to create Leaders with no associated entity (Text or Insert)
            '~if a collection of vectors is passed as the first argument all the points are used as the leader vertices.
            '~If a single point is passed as the first argument the first point is treated as the first leader point and then second
            '~argument is treated as the end point. If a point is passed in the third argument it is treated as an inflection point.
            Try
                _rVal = xNewLeader(dxxLeaderTypes.NoReactor, aStartPTXY, aTerminalPt, aMidXY, aArrowType, aArrowSize, aArrowBlock, bSuppressHook, aDimStyle, aTextStyle, aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation, bCreateOnly, rErrString, bSuppressErrors, aImage)
                If Not String.IsNullOrWhiteSpace(rErrString) Then Throw New Exception(rErrString)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    rErrString = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    rErrString = ex.Message
                End If
                Return _rVal
            End Try
        End Function

        Public Function NoRef(aVertCol As IEnumerable(Of iVector), aTerminalPt As iVector, Optional aMidXY As iVector = Nothing, Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1, Optional aArrowBlock As String = "", Optional bSuppressHook As Boolean = False, Optional aDimStyle As String = "", Optional aTextStyle As String = "", Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLeader
            Dim _rVal As dxeLeader = Nothing
            If Not GetImage(aImage) Then Return Nothing
            Dim rErrString As String = String.Empty
            '#1the start point of the requested leader or a collection of leader vertices
            '#2the end point of the requested leader
            '#3an optional point to to add an additional vertex to the leader line
            '#4a arrow head type for the leader lines
            '#5the size for the arrowhead
            '#6the block to use if the arrow head type is user defined
            '#7flag to suppress the leader hooks
            '#8the dim style for the leader other than the current dim style
            '#9the text style to use for the text rather than the current text style
            '#10the layer to put the entity on instead of the current leader layer setting
            '#11a color to apply to the entity instead of the current color
            '#12a linetype to apply to the entity instead of the current linetype setting
            '^used to create Leaders with no associated entity (Text or Insert)
            '~if a collection of vectors is passed as the first argument all the points are used as the leader vertices.
            '~If a single point is passed as the first argument the first point is treated as the first leader point and then second
            '~argument is treated as the end point. If a point is passed in the third argument it is treated as an inflection point.
            Try
                _rVal = xNewLeader(dxxLeaderTypes.NoReactor, aVertCol, aTerminalPt, aMidXY, aArrowType, aArrowSize, aArrowBlock, bSuppressHook, aDimStyle, aTextStyle, aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation, bCreateOnly, rErrString, bSuppressErrors, aImage)
                If Not String.IsNullOrWhiteSpace(rErrString) Then Throw New Exception(rErrString)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    rErrString = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    rErrString = ex.Message
                End If
                Return _rVal
            End Try
        End Function

        Private Function xNewLeader(aLeadertType As dxxLeaderTypes, aStartPTXY As iVector, aTerminalPt As iVector, aMidXY As iVector, aArrowType As dxxArrowHeadTypes, aArrowSize As Double, aArrowBlock As String, bSuppressHook As Boolean, aDimStyle As String, aTextStyle As String, aLayer As String, aColor As dxxColors, aLineType As String, bSuppressUCS As Boolean, bSuppressElevation As Boolean, bCreateOnly As Boolean, ByRef rErrString As String, bSuppressErrors As Boolean, aImage As dxfImage) As dxeLeader
            Dim _rVal As dxeLeader = Nothing
            '#1the start point of the requested leader or a collection of leader vertices
            '#2the end point of the requested leader
            '#3an optional point to to add an additional vertex to the leader line
            '#4a arrow head type for the leader lines
            '#5the size for the arrowhead
            '#6the block to use if the arrow head type is user defined
            '#7flag to suppress the leader hooks
            '#8the dim style for the leader other than the current dim style
            '#9the text style to use for the text rather than the current text style
            '#10the layer to put the entity on instead of the current leader layer setting
            '#11a color to apply to the entity instead of the current color
            '#12a linetype to apply to the entity instead of the current linetype setting
            '^used to create Leaders with no associated entity (Text or Insert)
            '~if a collection of vectors is passed as the first argument all the points are used as the leader vertices.
            '~If a single point is passed as the first argument the first point is treated as the first leader point and then second
            '~argument is treated as the end point. If a point is passed in the third argument it is treated as an inflection point.
            If aStartPTXY Is Nothing Then aStartPTXY = New dxfVector
            Return xNewLeader(aLeadertType, New List(Of iVector)({aStartPTXY}), aTerminalPt, aMidXY, aArrowType, aArrowSize, aArrowBlock, bSuppressHook, aDimStyle, aTextStyle, aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation, bCreateOnly, rErrString, bSuppressErrors, aImage)
        End Function

        Private Function xNewLeader(aLeadertType As dxxLeaderTypes, aVertCol As IEnumerable(Of iVector), aTerminalPt As iVector, aMidXY As iVector, aArrowType As dxxArrowHeadTypes, aArrowSize As Double, aArrowBlock As String, bSuppressHook As Boolean, aDimStyle As String, aTextStyle As String, aLayer As String, aColor As dxxColors, aLineType As String, bSuppressUCS As Boolean, bSuppressElevation As Boolean, bCreateOnly As Boolean, ByRef rErrString As String, bSuppressErrors As Boolean, aImage As dxfImage) As dxeLeader
            Dim _rVal As dxeLeader = Nothing
            '#1the start point of the requested leader or a collection of leader vertices
            '#2the end point of the requested leader
            '#3an optional point to to add an additional vertex to the leader line
            '#4a arrow head type for the leader lines
            '#5the size for the arrowhead
            '#6the block to use if the arrow head type is user defined
            '#7flag to suppress the leader hooks
            '#8the dim style for the leader other than the current dim style
            '#9the text style to use for the text rather than the current text style
            '#10the layer to put the entity on instead of the current leader layer setting
            '#11a color to apply to the entity instead of the current color
            '#12a linetype to apply to the entity instead of the current linetype setting
            '^used to create Leaders with no associated entity (Text or Insert)
            '~if a collection of vectors is passed as the first argument all the points are used as the leader vertices.
            '~If a single point is passed as the first argument the first point is treated as the first leader point and then second
            '~argument is treated as the end point. If a point is passed in the third argument it is treated as an inflection point.

            Dim aDStyle As dxoDimStyle = Nothing
            Dim aTStyle As dxoStyle = Nothing
            Dim aPlane As dxfPlane = Nothing
            Dim aInput As TDIMINPUT = TDIMINPUT.Null
            rErrString = ""
            If Not GetImage(aImage) Then Return _rVal
            Try
                Dim verts As TVECTORS = xCreateLeaderVertices(aVertCol, aMidXY, aTerminalPt, bSuppressUCS, bSuppressElevation, aPlane, aImage)
                _rVal = New dxeLeader(aLeadertType)
                '^creates a leader entitiy based on the passed input

                aInput.Color = aColor
                aInput.DimStyleName = aDimStyle
                aInput.DimType = dxxEntityTypes.Text
                aInput.LayerName = aLayer
                aInput.Linetype = aLineType
                aInput.TextStyleName = aTextStyle
                aInput.ArrowSize = aArrowSize
                aInput.ArrowBlockL = aArrowBlock.Trim
                If aArrowType >= dxxArrowHeadTypes.ClosedFilled And aArrowType <= dxxArrowHeadTypes.None Then
                    aInput.ArrowHeadL = aArrowType
                End If
                _rVal.DisplayStructure = dxfImageTool.DisplayStructure_DIM(aImage, True, aInput, rDStyle:=aDStyle, rTStyle:=aTStyle)
                gInput = aInput
                Dim ip As TVECTOR = verts.Item(1, True)
                Dim lp As TVECTOR = verts.Item(verts.Count, True)
                Dim aDir As TVECTOR = ip.DirectionTo(lp)
                Dim ang As Double = aPlane.XDirectionV.AngleTo(aDir, aPlane.ZDirectionV)
                If Not ((ang > 15 And ang < 165) Or (ang > 195 And ang < 345)) Then bSuppressHook = True
                _rVal.PlaneV = New TPLANE(aPlane)
                'get the current dim style properties
                _rVal.DimStyle = aDStyle
                _rVal.VectorsV = verts
                _rVal.SuppressHook = bSuppressHook
                _rVal.SuppressArrowHead = aArrowType = dxxArrowHeadTypes.Suppressed
                _rVal.PopLeft = ang > 90 And ang < 270
                _rVal.ImageGUID = aImage.GUID
                If Not bCreateOnly Then
                    _rVal = aImage.SaveEntity(_rVal)
                End If
            Catch ex As Exception
                If Not bSuppressErrors Then
                    rErrString = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    rErrString = ex.Message
                End If
            End Try
            Return _rVal
        End Function

        Public Function Text(aStartPTXY As iVector, aTextPt As iVector, aTextString As Object, Optional aMidPt As iVector = Nothing, Optional bSuppressHook As Boolean = False, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bCreateOnly As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLeader


            Return Text(New List(Of iVector)({aStartPTXY}), aTextPt, aTextString, aMidPt, 0, dxxVerticalJustifications.Undefined, dxxArrowHeadTypes.ByStyle, -1, "", bSuppressHook, "", "", aLayer, aColor, aLineType, False, False, bCreateOnly, False, aImage)
        End Function
        Public Function Text(aLeaderPts As IEnumerable(Of iVector), aTextPt As iVector, aTextString As Object, Optional aMidPt As iVector = Nothing, Optional bSuppressHook As Boolean = False, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bCreateOnly As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLeader
            Return Text(aLeaderPts, aTextPt, aTextString, aMidPt, 0, dxxVerticalJustifications.Undefined, dxxArrowHeadTypes.ByStyle, -1, "", bSuppressHook, "", "", aLayer, aColor, aLineType, False, False, bCreateOnly, False, aImage)
        End Function


        Public Function Text(aVertColXY As IEnumerable(Of iVector), aTextPtXY As dxfVector, aTextString As Object, Optional aMidXY As dxfVector = Nothing, Optional aTextHeight As Double = 0.0, Optional aTextJustification As dxxVerticalJustifications = dxxVerticalJustifications.Undefined, Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1, Optional aArrowBlock As String = "", Optional bSuppressHook As Boolean = False, Optional aDimStyle As String = "", Optional aTextStyle As String = "", Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLeader
            Dim _rVal As dxeLeader = Nothing
            Dim txt As String = TVALUES.To_STR(aTextString)
            If String.IsNullOrWhiteSpace(txt) Then Return Nothing
            If Not GetImage(aImage) Then Return Nothing
            '#1the start point of the requested leader or a collection of leader vertices
            '#2the end point of the requested leader
            '#3the leader text
            '#4an optional point to to add an additional vertex to the leader line
            '#5the text height to apply rather than the current text height
            '#6flag to align the leader line to the midpoint of the first line of text
            '#7a arrow head type for the leader lines
            '#8the block to use if the arrow head type is user defined
            '#9flag to suppress the leader hooks
            '#10the dim style for the leader other than the current dim style
            '#11the text style to use for the text rather than the current text style
            '#12the layer to put the entity on instead of the current leader layer setting
            '#13a color to apply to the entity instead of the current color
            '#14a linetype to apply to the entity instead of the current linetype setting
            '^used to draw text Leaders
            '~if a collection of vectors is passed as the first argument all the points are used as the leader vertices.
            '~If a single point is passed as the first argument the first point is treated as the first leader point and then second
            '~argument is treated as the end point. If a point is passed in the forth argument it is treated as an inflection point.
            Dim rErrString As String = String.Empty
            Try
                _rVal = xNewLeader(dxxLeaderTypes.LeaderText, aVertColXY, aTextPtXY, aMidXY, aArrowType, aArrowSize, aArrowBlock, bSuppressHook, aDimStyle, aTextStyle, aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation, True, rErrString, True, aImage)
                'error on invalid input
                'create the leader line
                '^creates a leader entitiy based on the passed input
                If txt.Trim <> "" Then
                    If aTextHeight > 0 Then _rVal.DimStyle.TextHeight = aTextHeight
                    If aTextJustification <> dxxVerticalJustifications.Undefined Then
                        _rVal.TextJustification = aTextJustification
                    ElseIf aImage IsNot Nothing Then
                        _rVal.TextJustification = aImage.BaseSettings(dxxSettingTypes.DIMSETTINGS).Props.ValueI("LeaderTextJustification")
                    End If
                    _rVal.TextString = txt
                End If
                If Not bCreateOnly Then _rVal = aImage.SaveEntity(_rVal) Else _rVal.SetImage(aImage, False)
                If rErrString <> "" Then Throw New Exception(rErrString)
            Catch ex As Exception
                If Not bSuppressErrors Then
                    rErrString = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    rErrString = ex.Message
                End If
            End Try
            Return _rVal
        End Function
        Public Function Block(aStartPTOrVertCol As Object, aBlockInsertPTXY As dxfVector, aBlockName As String, Optional aMidXY As dxfVector = Nothing, Optional aBlockScaleFactor As Double = 1, Optional aBlockRotation As Double = 0.0, Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1, Optional aArrowBlock As String = "", Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aAttributes As dxfAttributes = Nothing, Optional aDimStyle As String = "", Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLeader
            Dim _rVal As dxeLeader = Nothing
            If Not GetImage(aImage) Then Return Nothing
            '#2the start point of the requested leader or a collection of leader vertices
            '#3the end point of the requested leader
            '#4the block to draw the leader from
            '#5an optional point to to add an additional vertex to the leader line
            '#6the scale factor to apply to the inserted block
            '#7the rotation to apply to the inserted block
            '#8the type of arrowhead requested
            '#9an override size for the leader arrow head
            '#10the name of the block to use for the arrow head if the arrowhead type = UserDefined
            '#11a horizontal dstance to offset the block insertion pt from the end of the leader line
            '#13a vertical dstance to offset the block insertion pt from the end of the leader line
            '#14the dim style to associat to the leader
            '#15the layer to put the entity on instead of the current layer setting
            '#16a color to apply to the entity instead of the current color setting
            '#17a linetype to apply to the entity instead of the current linetype setting
            '^used to draw leaders attached to block inserts.
            '~the referenced block must have been created and added using Image.Blocks.Add prior to executing this method.
            '~if the block is not found then nothing is drawn.
            '~if a collection of vectors is passed as the first argument all the points are used as the leader vertices.
            '~If a single point is passed as the first argument the first point is treated as the first leader point and then second
            '~argument is treated as the end point. If a point is passed in the forth argument it is treated as an inflection point.
            Dim rErrString As String = String.Empty
            Try
                Dim bname As String
                Dim aBlock As dxfBlock = Nothing
                _rVal = xNewLeader(dxxLeaderTypes.LeaderBlock, aStartPTOrVertCol, aBlockInsertPTXY, aMidXY, aArrowType, aArrowSize, aArrowBlock, True, aDimStyle, "", aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation, True, rErrString, True, aImage)
                bname = Trim(aBlockName)
                If aImage IsNot Nothing Then aBlock = aImage.Blocks.GetByName(bname)
                If aBlock IsNot Nothing Then
                    _rVal.SetPropVal("*BlockName", aBlock.Name)
                    _rVal.BlockGUID = aBlock.GUID
                    _rVal.BlockScale = aBlockScaleFactor
                    _rVal.XOffset = aXOffset
                    _rVal.YOffset = aYOffset
                    _rVal.BlockRotation = aBlockRotation
                End If
                _rVal.BlockAttributes = aAttributes
                If Not bCreateOnly Then _rVal = aImage.SaveEntity(_rVal) Else _rVal.SetImage(aImage, False)
                If rErrString <> "" Then Throw New Exception(rErrString)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    rErrString = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, ex.Message)
                Else
                    rErrString = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function Stack_Vertical(aLeaderPtsXY As IEnumerable(Of iVector), aLeaderStrings As Collection, aFirstLeaderPointXY As Object, Optional aSpaceAdder As Double = 0.0,
                                       Optional aInvertStack As Boolean = False, Optional aTextHeight As Double = 0.0, Optional aTextColor As dxxColors = dxxColors.Undefined,
                                       Optional aTextJustification As dxxVerticalJustifications = dxxVerticalJustifications.Undefined, Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle,
                                       Optional aArrowSize As Double = -1, Optional aArrowBlock As String = "", Optional bSuppressHook As Boolean = False,
                                       Optional aDimStyle As String = "", Optional aTextStyle As String = "",
                                       Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                       Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                                       Optional aCollector As colDXFEntities = Nothing, Optional bSuppressErrors As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            '#1the collection of points to draw leaders from
            '#2the collection of strings to use as the leader text (nothing passed means the tags of the passed points will be used)
            '#3the point to place the first leader
            '#4a factor to increase the spacing between the leaders text
            '#5flag to stack the leaders bottom to top instead of top to bottom which is the default
            '#6the text height to apply rather than the current text height
            '#7an override text color to apply rather than that defined in the subject dimstyle
            '#8flag to align the leader line to the midpoint of the first line of text
            '#9a arrow head type for the leader lines
            '#10the block to use if the arrow head type is user defined
            '#11flag to suppress the leader hooks
            '#12the dim style for the leader other than the current dim style
            '#13the text style to use for the text rather than the current text style
            '#14the layer to put the entities on instead of the current leader layer setting
            '#15a color to apply to the entities instead of the current color
            '#16a linetype to apply to the entity instead of the current linetype setting
            '#17flag to add the leaders to the image
            '^a composite function used to draw multiple leaders that have the text stacked and aligned vertically starting at the passed point.
            '~this function creates one leader for each point in the passed collection.
            '~the leader text is obtained from the passed collection of strings.
            '~if the number of strings does not match the number of points then the tag property of the vector is used
            '~point whose index is greater that the number of text strings.
            '~the function returns the created leaders in an entity collection
            _rVal = New colDXFEntities
            If aLeaderPtsXY Is Nothing Then Return _rVal
            If aLeaderPtsXY.Count <= 0 Then Return _rVal
            Try
                Dim aPlane As dxfPlane = Nothing
                Dim P1 As dxfVector = aImage.CreateVector(aFirstLeaderPointXY, bSuppressUCS, bSuppressElevation, aPlane)
                Dim aVrts As TVERTICES = aImage.CreateUCSVertices(aLeaderPtsXY, True, True, aPlane)
                Dim aPln As New TPLANE(aPlane)
                Dim bndRec As TPLANE = CType(aVrts, TVECTORS).Bounds(aPln)
                Dim aDStyle As dxoDimStyle = Nothing
                Dim aTStyle As dxoStyle = Nothing
                Dim iGUID As String = aImage.GUID
                Dim bLdr As dxeLeader
                Dim lLdr As dxeLeader = Nothing
                Dim bTxt As dxeText
                Dim aTxt As dxeText
                Dim txtCol As New List(Of String)
                Dim vIDs As List(Of Integer) = Nothing
                Dim tpt As TVECTOR = P1.Strukture
                Dim v1 As TVECTOR
                If Not aInvertStack Then v1 = bndRec.Point(dxxRectanglePts.TopCenter) Else v1 = bndRec.Point(dxxRectanglePts.BottomCenter)
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim yDir As TVECTOR
                Dim aRec As TPLANE
                Dim bRec As TPLANE
                Dim vrt As TVERTEX
                Dim dInput As TDIMINPUT
                Dim sortLn = aPln.LineH(v1, 10)
                Dim txtLn As TLINE
                Dim txt As String
                Dim cnt As Integer
                Dim idx As Integer
                Dim d1 As Double
                Dim tht As Double
                Dim bTags As Boolean
                If aTextJustification = dxxVerticalJustifications.Undefined Then
                    aTextJustification = aImage.BaseSettings(dxxSettingTypes.DIMSETTINGS).Props.ValueI("LeaderTextJustification")
                End If

                aVrts.SortRelativeToLine(sortLn, aPln.ZDirection, aInvertStack, vIDs)
                'style layer color etc
                dInput = TDIMINPUT.Null
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(aEntityType:=dxxEntityTypes.LeaderText, "", dxxColors.Undefined, "", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False, aStyleName:=aDimStyle)
                End If
                dInput.LayerName = aDisplaySettings.LayerName
                dInput.Color = aDisplaySettings.Color
                dInput.DimLineColor = aDisplaySettings.Color
                dInput.Linetype = aDisplaySettings.Linetype
                dInput.DimStyleName = aDimStyle
                dInput.ArrowSize = aArrowSize
                dInput.ArrowBlockL = Trim(aArrowBlock)
                If aArrowType >= dxxArrowHeadTypes.ClosedFilled And aArrowType <= dxxArrowHeadTypes.None Then
                    dInput.ArrowHeadL = aArrowType
                End If
                dInput.TextColor = aTextColor
                Dim eStruc As TDISPLAYVARS = dxfImageTool.DisplayStructure_DIM(aImage, True, dInput, rDStyle:=aDStyle, rTStyle:=aTStyle)
                Dim FS As Double = aDStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
                If aTextHeight > 0 Then
                    tht = aTextHeight
                    aDStyle.PropValueSet(dxxDimStyleProperties.DIMTXT, aTextHeight / FS)
                Else
                    tht = aDStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * FS
                End If
                Dim tgap As Double = 0.5 * tht + Math.Abs(aSpaceAdder)
                Dim lVerts As New TVECTORS(New TVECTOR(0, 0), tpt)
                Dim aLdr As New dxeLeader(dxxLeaderTypes.LeaderText) With
                {
                    .DisplayStructure = eStruc,
                    .DimStyle = aDStyle,
                    .SuppressHook = bSuppressHook,
                    .VectorsV = lVerts,
                    .PlaneV = aPln,
                    .TextJustification = aTextJustification,
                    .SuppressArrowHead = aArrowType = dxxArrowHeadTypes.Suppressed
                }


                If aLeaderStrings Is Nothing Then
                    bTags = True
                Else
                    If aLeaderStrings.Count <= 0 Then bTags = True
                End If
                'create the text collection
                If Not bTags Then
                    For i As Integer = 1 To aLeaderStrings.Count
                        txtCol.Add(TVALUES.To_STR(aLeaderStrings.Item(i)))
                    Next i
                End If
                yDir = aPln.YDirection
                If aInvertStack Then yDir *= -1
                cnt = 0
                For i As Integer = 1 To vIDs.Count
                    idx = vIDs.Item(i - 1)
                    vrt = aVrts.Item(i)
                    txt = vrt.Tag.Trim()
                    If Not bTags Then
                        If idx <= txtCol.Count Then
                            txt = txtCol.Item(idx - 1).Trim
                        End If
                    End If
                    If txt = String.Empty Then Continue For
                    cnt += 1
                    If cnt > 1 Then
                        bLdr = lLdr.Clone
                    Else
                        bLdr = aLdr.Clone
                    End If

                    bLdr.ReactorEntity = Nothing
                    bLdr.TextString = txt
                    bLdr.ImageGUID = iGUID
                    lVerts.SetItem(1, vrt.Vector)
                    lVerts.SetItem(2, tpt)
                    bLdr.VectorsV = lVerts
                    bLdr.UpdatePath(True, True, aImage)
                    If cnt > 1 Then
                        'the last
                        aTxt = lLdr.MText
                        'the current
                        bTxt = aTxt.Clone() '  bLdr.MText
                        bLdr.MText = Nothing
                        bTxt.TextString = txt
                        bTxt.OwnerGUID = ""
                        aTxt.UpdatePath(bRegen:=False, aImage:=aImage)
                        bTxt.UpdatePath(bRegen:=False, aImage:=aImage)

                        aRec = aTxt.Bounds
                        bRec = bTxt.Bounds
                        If Not aInvertStack Then
                            v1 = aRec.Point(dxxRectanglePts.BottomCenter)
                            v2 = bRec.Point(dxxRectanglePts.TopCenter)
                        Else
                            v1 = aRec.Point(dxxRectanglePts.TopCenter)
                            v2 = bRec.Point(dxxRectanglePts.BottomCenter)
                        End If
                        v1 += yDir * -tgap
                        txtLn = aPln.LineH(v1, 10)
                        v3 = dxfProjections.ToLine(v2, txtLn, rDistance:=d1)
                        If d1 <> 0 Then

                            bTxt.Translate(v3 - v2)
                        End If
                        If cnt = 2 Then
                            aImage.Entities.Add(New dxeLine(txtLn))
                            aImage.Entities.AddArc(v3.X, v3.Y, 0.08)
                            aImage.Entities.AddArc(v2.X, v2.Y, 0.05)
                            aImage.Entities.Add(New dxePolyline(aRec.Corners, True))
                            aImage.Entities.Add(New dxePolyline(bRec.Corners, True))
                        End If
                        bTxt.OwnerGUID = bLdr.GUID
                        bLdr.MText = bTxt
                        bLdr.IsDirty = True
                        'bLdr.MText = bTxt
                    End If
                    If aCollector IsNot Nothing Then bLdr = aCollector.Add(bLdr)
                    If Not bCreateOnly Then bLdr = aImage.SaveEntity(bLdr) Else bLdr.SetImage(aImage, False)
                    _rVal.Add(bLdr)
                    lLdr = bLdr

                Next i
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            Finally
                aImage = Nothing
            End Try
        End Function
        Public Function Stack_Horizontal(aLeaderPtsXY As colDXFVectors, aOffset As Double, Optional bLeftToRight As Boolean = False, Optional aSpaceAdder As Double = 0.0,
                                         Optional bVecRadsAsCircles As Boolean = False, Optional bOffsetIsYOrdinate As Boolean = False, Optional aTextHeight As Double = 0.0,
                                         Optional aTextColor As dxxColors = dxxColors.Undefined, Optional aTextJustification As dxxVerticalJustifications = dxxVerticalJustifications.Undefined,
                                         Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1, Optional aArrowBlock As String = "",
                                         Optional bSuppressHook As Boolean = False, Optional aDimStyle As String = "", Optional aTextStyle As String = "",
                                         Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                         Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                                         Optional bSuppressErrors As Boolean = False) As colDXFEntities
            '#1the collection of points to draw leaders from
            '#2a offset distance
            '#3a flag indicating that the vectors and leaders should be arranged from left to right
            '#4a distance to increase the spacing between the leaders text
            '#5if True the radius property of the passed vectors is used to draw circles aroud the points
            '#6if True the offset distance is treated as the y ordinate in the current UCS to place the leaders on
            '#7the text height to apply rather than the current text height
            '#8an override text color to apply rather than that defined in the subject dimstyle
            '#9flag to align the leader line to the midpoint of the first line of text
            '#10a arrow head type for the leader lines
            '#11an override arrowhead size
            '#12the block to use if the arrow head type is user defined
            '#13flag to suppress the leader hooks
            '#14the dim style for the leader other than the current dim style
            '#15the text style to use for the text rather than the current text style
            '#16the layer to put the entities on instead of the current leader layer setting
            '#17a color to apply to the entities instead of the current color
            '#18a linetype to apply to the entity instead of the current linetype setting
            '#18flag to add the leaders to the image
            '#19a entity collection to add the leaders to
            '^a composite function used to draw multiple leaders that are aligned horizontally.
            '~the sign of the offset distance controls the placement of the leaders. positive offsets creates the leaders above the points. negative offsets creates
            '~the leaders below the points.
            '~this function creates one leader for each point in the passed collection that has a defined tag.
            '~the function returns the created leaders in an entity collection.
            Dim _rVal As New colDXFEntities
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Try
                If aLeaderPtsXY Is Nothing Then Return _rVal
                If aLeaderPtsXY.Count <= 0 Then Return _rVal
                Dim aPlane As dxfPlane = Nothing
                aImage.CreateUCSVector(TVECTOR.Zero, bSuppressUCS, bSuppressElevation, aPlane)
                Dim aVrts As TVERTICES = aImage.CreateUCSVertices(aLeaderPtsXY, True, True, aPlane)

                Dim aPln As New TPLANE(aPlane)


                Dim aDStyle As dxoDimStyle = Nothing
                Dim aTStyle As dxoStyle = Nothing
                Dim aLdr As dxeLeader
                Dim bLdr As dxeLeader
                Dim lLdr As dxeLeader = Nothing
                Dim ltxt As dxeText
                Dim aTxt As dxeText
                Dim aArc As dxeArc = Nothing
                Dim bArc As dxeArc
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim xDir As TVECTOR
                Dim aDir As TVECTOR
                Dim pDir As TVECTOR
                Dim aRec As TPLANE
                Dim lRec As TPLANE
                Dim vrt As TVERTEX
                Dim eStruc As TDISPLAYVARS
                Dim dInput As TDIMINPUT
                Dim lVerts As New TVECTORS(2)
                Dim iPts As TVECTORS
                Dim tpt As TVECTOR
                Dim aLn As TLINE
                Dim txtLn As TLINE
                Dim iGUID As String = aImage.GUID
                Dim txt As String
                Dim i As Integer
                Dim cnt As Integer
                Dim d1 As Double
                Dim d2 As Double
                Dim tgap As Double
                Dim tht As Double
                Dim FS As Double
                Dim bFlag As Boolean
                Dim asz As Double
                Dim ang As Double
                Dim oset As Double
                Dim f1 As Double
                aPln = aVrts.Bounds(aPln)
                'sort the vectors left to right or right to left depending on the angle
                If bLeftToRight Then f1 = -1 Else f1 = 1
                v1 = aPln.Vector(0.51 * aPln.Width * f1)
                aLn = aPln.LineV(v1, 10)
                aVrts.SortRelativeToLine(aLn, aPln.ZDirection, False)
                'aImage.Entities.AddRectangleV aPln
                'aImage.Entities.AddLine aLn.SPT.X, aLn.SPT.Y, aLn.EPT.X, aLn.EPT.Y
                'style layer color etc
                dInput = TDIMINPUT.Null
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(aEntityType:=dxxEntityTypes.LeaderText, "", dxxColors.Undefined, "", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False, aStyleName:=aDimStyle)
                End If
                dInput.LayerName = aDisplaySettings.LayerName
                dInput.Color = aDisplaySettings.Color
                dInput.DimLineColor = aDisplaySettings.Color
                dInput.Linetype = aDisplaySettings.Linetype
                dInput.DimStyleName = aDimStyle
                dInput.ArrowSize = aArrowSize
                dInput.ArrowBlockL = Trim(aArrowBlock)
                If aArrowType >= dxxArrowHeadTypes.ClosedFilled And aArrowType <= dxxArrowHeadTypes.None Then
                    dInput.ArrowHeadL = aArrowType
                End If
                dInput.TextColor = aTextColor
                eStruc = dxfImageTool.DisplayStructure_DIM(aImage, True, dInput, rDStyle:=aDStyle, rTStyle:=aTStyle)
                FS = aDStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
                If aTextHeight > 0 Then
                    tht = aTextHeight
                    aDStyle.PropValueSet(dxxDimStyleProperties.DIMTXT, aTextHeight / FS)
                Else
                    tht = aDStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * FS
                End If
                tgap = Math.Abs(aDStyle.PropValueD(dxxDimStyleProperties.DIMGAP)) * FS + Math.Abs(aSpaceAdder)
                asz = Math.Abs(aDStyle.PropValueD(dxxDimStyleProperties.DIMASZ)) * FS
                If Not bSuppressHook Then tgap += asz
                aLdr = New dxeLeader(dxxLeaderTypes.LeaderText) With
                {
                    .DisplayStructure = eStruc,
                    .DimStyle = aDStyle,
                    .SuppressHook = bSuppressHook,
                    .VectorsV = lVerts,
                    .PlaneV = aPln
                }
                If aTextJustification <> dxxVerticalJustifications.Undefined Then
                    aLdr.TextJustification = aTextJustification
                Else
                    aLdr.TextJustification = aImage.BaseSettings(dxxSettingTypes.DIMSETTINGS).Props.ValueI("LeaderTextJustification")
                End If
                If aArrowType = dxxArrowHeadTypes.Suppressed Then aLdr.SuppressArrowHead = True
                If bVecRadsAsCircles Then
                    aArc = New dxeArc With {
                    .PlaneV = aPln,
                    .DisplayStructure = eStruc,
                    .Color = aLdr.DimStyle.DimLineColor}
                End If
                xDir = aPln.XDirection
                cnt = 0
                If bOffsetIsYOrdinate Then
                    v1 = aImage.obj_UCS.Vector(0, TVALUES.ToDouble(aOffset, aPrecis:=5))
                    oset = v1.Y - aPln.Origin.Y
                Else
                    oset = TVALUES.ToDouble(aOffset, aPrecis:=5)
                    If oset = 0 And Not bOffsetIsYOrdinate Then oset = 2 * tht
                End If
                If oset > 0 Then
                    ang = (90 + 15 * f1)
                    txtLn = aPln.LineH(aPln.Vector(0, 0.5 * aPln.Height + Math.Abs(oset)), 10)
                Else
                    ang = (270 - 15 * f1)
                    txtLn = aPln.LineH(aPln.Vector(0, -(0.5 * aPln.Height + Math.Abs(oset))), 10)
                End If
                For i = 1 To aVrts.Count
                    vrt = aVrts.Item(i)
                    txt = Trim(vrt.Tag)
                    If txt <> "" Then
                        cnt += 1
                        bLdr = aLdr.Clone
                        bLdr.MText = Nothing
                        bLdr.TextString = txt
                        bLdr.ImageGUID = iGUID
                        bArc = Nothing
                        lVerts.SetItem(1, vrt.Vector)
                        aLn = aPln.LinePolar(vrt.Vector, ang, 10, False)
                        tpt = txtLn.IntersectionPt(aLn)
                        If bVecRadsAsCircles Then
                            If vrt.Radius > 0 Then
                                bArc = aArc.Clone
                                bArc.CenterV = vrt.Vector
                                bArc.Radius = vrt.Radius
                            End If
                        End If
                        If bSuppressHook Then
                            lVerts.SetItem(2, tpt)
                        Else
                            lVerts.SetItem(2, tpt + xDir * (-asz * f1))
                        End If
                        bLdr.VectorsV = lVerts
                        bLdr.UpdatePath(True, aImage)
                        If cnt > 1 Then
                            lLdr.UpdatePath(False, aImage)
                            ltxt = lLdr.MText
                            'the current
                            aTxt = bLdr.MText
                            aTxt.UpdatePath(False, aImage)
                            ltxt.UpdatePath(False, aImage)
                            aRec = aTxt.Bounds
                            lRec = ltxt.Bounds
                            d1 = 0
                            v1 = aRec.Vector(f1 * (0.5 * aRec.Width + tgap))
                            aLn = lRec.LineV(lRec.Vector(-f1 * (0.5 * lRec.Width + tgap)), 10)
                            v2 = dxfProjections.ToLine(v1, aLn, rOrthoDirection:=aDir, rDistance:=d2)
                            pDir = aDir
                            pDir.Equals(xDir, True, 3, bFlag)
                            If bLeftToRight Then bFlag = Not bFlag
                            If Not bFlag Then
                                If d2 < tgap Then d1 = tgap - d2
                            Else
                                d1 = d2 + tgap
                            End If
                            d1 = -f1 * d1
                            If d1 <> 0 Then
                                aTxt.OwnerGUID = ""
                                aTxt.Transform(TTRANSFORM.CreateProjection(xDir, d1), True)
                                aTxt.OwnerGUID = bLdr.GUID
                                bLdr.IsDirty = True
                                bLdr.MText = aTxt
                            End If
                        End If
                        If bArc IsNot Nothing Then
                            bLdr.UpdatePath(False, aImage)
                            lVerts.SetItem(1, bLdr.VertexV(1))
                            lVerts.SetItem(2, bLdr.VertexV(2))
                            aLn.SPT = lVerts.Item(1)
                            aLn.EPT = lVerts.Item(2)
                            iPts = aLn.IntersectionPts(bArc.ArcStructure, False, True)
                            If iPts.Count > 0 Then
                                lVerts.SetItem(1, iPts.Item(1))
                                bLdr.VectorsV = lVerts
                                bLdr.IsDirty = True
                            End If
                        End If
                        Select Case bLdr.TextJustification
                            Case dxxVerticalJustifications.Bottom
                                bLdr.MText.Alignment = If(bLeftToRight, dxxMTextAlignments.BottomLeft, dxxMTextAlignments.BottomRight)
                            Case dxxVerticalJustifications.Center
                                bLdr.MText.Alignment = If(bLeftToRight, dxxMTextAlignments.MiddleLeft, dxxMTextAlignments.MiddleRight)
                            Case dxxVerticalJustifications.Top
                                bLdr.MText.Alignment = If(bLeftToRight, dxxMTextAlignments.TopLeft, dxxMTextAlignments.TopRight)
                            Case Else
                        End Select
                        If Not bCreateOnly Then
                            bLdr = aImage.SaveEntity(bLdr)
                            If bArc IsNot Nothing Then bArc = aImage.SaveEntity(bArc)
                        End If
                        _rVal.AddToCollection(bLdr, aAddClone:=True)
                        lLdr = bLdr
                    End If
                Next i
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
            End Try
            Return _rVal
        End Function
        Public Function Stacks(aLeaderPts As colDXFVectors, aCenterPt As dxfVector, aLeaderLength As Double, aXOffset As Double,
                               Optional aSpaceAdder As Double = 0.0, Optional aLeftLeaderAngle As Double = 45,
                               Optional aRightLeaderAngle As Double = 45, Optional aInitYOffset As Double = 0.0,
                               Optional aTextHeight As Double = 0.0, Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional aTextJustification As dxxVerticalJustifications = dxxVerticalJustifications.Undefined,
                               Optional aDimStyle As String = "", Optional aTextStyle As String = "",
                               Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1,
                               Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "",
                               Optional bCreateOnly As Boolean = False, Optional aCollector As List(Of dxfEntity) = Nothing) As List(Of dxfEntity)
            Dim aImage As dxfImage = Nothing
            Dim _rVal As List(Of dxfEntity)
            If aCollector IsNot Nothing Then _rVal = aCollector Else _rVal = New List(Of dxfEntity)
            If aLeaderPts Is Nothing Or Not GetImage(aImage) Then Return _rVal
            If aLeaderPts.Count <= 0 Then Return _rVal
            '#1the collection of points to draw leaders from (the tags of the passed points are the leader texts)
            '#2the point to treat as the center to decided which direction then leaders should pop
            '#3the distance to offset the leader text form the points in the Y direction
            '#4a distance to offset the leader end points in the X direction
            '#5an additional space to seperate the text
            '#6the angle to apply to the first leader segment on the left (added to 90)
            '#7the angle to apply to the first leader segment on the right (subtracted from 90)
            '#8an override text height
            '#9a override text color
            '#10flag to align the leader line to the midpoint of the first line of text
            '#11the dimstyle to use other than the current
            '#12the textstyle to use other than the current
            '#13an override arrowhead to use
            '#14an override arrowhead size to use
            '#15the layer to put the entity on instead of the current leader layer setting
            '#16a color to apply to the entity instead of the current color
            '#17a linetype to apply to the entity instead of the current linetype setting
            '#18flag to add the leaders to the image
            '^a composite function used to draw multiple leaders that have the text stacked and aligned vertically starting at the passed point.
            '~this function creates one leader for each point in the passed collection.
            '~the leader text is obtained from the tags of the passed vectors.
            '~the function returns the created leaders in a collection
            Try
                Dim aPln As TPLANE = aLeaderPts.BoundingRectangleV(aImage.obj_UCS)
                Dim yDir As TVECTOR = aPln.YDirection
                Dim xDir As TVECTOR = aPln.XDirection
                Dim zDir As TVECTOR = aPln.ZDirection
                Dim lPts As TVERTICES
                Dim aDStyle As dxoDimStyle = Nothing
                Dim aTStyle As dxoStyle = Nothing
                Dim aLdr As dxeLeader
                Dim bLdr As dxeLeader

                Dim bTxt As dxeText
                Dim aTxt As dxeText
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim aRec As TPLANE
                Dim bRec As TPLANE
                Dim vrt As TVERTEX
                Dim eStruc As TDISPLAYVARS
                Dim dInput As TDIMINPUT
                Dim lVerts As TVECTORS
                Dim verts3 As TVECTORS
                Dim verts2 As TVECTORS

                Dim txtLn As New TLINE("")
                Dim iGUID As String = aImage.GUID
                Dim txt As String

                Dim d1 As Double = Double.MinValue
                Dim d2 As Double
                Dim tht As Double
                Dim x1 As Double = 0

                Dim aFlg As Boolean

                Dim bDir As TVECTOR
                Dim lftPts As New TVERTICES(0)
                Dim rghtPts As New TVERTICES(0)
                Dim vec As dxfVector
                If aCenterPt IsNot Nothing Then
                    v1 = aCenterPt.Strukture.WithRespectTo(aPln)
                    x1 = v1.X
                End If
                For i As Integer = 1 To aLeaderPts.Count
                    vec = aLeaderPts.Item(i)
                    If Not String.IsNullOrWhiteSpace(vec.Tag) Then
                        vrt = dxfProjections.ToPlane(vec.VertexV, aPln, zDir)
                        v1 = vrt.Vector.WithRespectTo(aPln)
                        If v1.X <= x1 Then
                            lftPts.Add(vrt)
                        Else
                            rghtPts.Add(vrt)
                        End If
                        If v1.Y > d1 Then d1 = v1.Y
                    End If
                Next i
                If lftPts.Count + rghtPts.Count <= 0 Then Return _rVal
                Dim limLn As New TLINE(aPln.Vector(-10, d1), aPln.Vector(0, d1))
                _rVal.Add(New dxeLine(limLn))
                lftPts.SortByDistanceToLine(limLn, False)
                rghtPts.SortByDistanceToLine(limLn, False)
                'style layer color etc
                dInput = New TDIMINPUT("") With
                {
                    .LayerName = aLayer,
                    .Color = aColor,
                    .DimLineColor = aColor,
                    .Linetype = aLineType,
                    .DimStyleName = aDimStyle,
                    .ArrowSize = aArrowSize,
                    .TextColor = aTextColor
                 }

                If aArrowType >= dxxArrowHeadTypes.ClosedFilled And aArrowType <= dxxArrowHeadTypes.None Then
                    dInput.ArrowHeadL = aArrowType
                End If

                eStruc = dxfImageTool.DisplayStructure_DIM(aImage, True, dInput, rDStyle:=aDStyle, rTStyle:=aTStyle)
                Dim FS As Double = aDStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
                Dim asz As Double = aDStyle.PropValueD(dxxDimStyleProperties.DIMASZ) * FS
                If aTextHeight > 0 Then
                    tht = aTextHeight
                    aDStyle.PropValueSet(dxxDimStyleProperties.DIMTXT, aTextHeight / FS)
                Else
                    tht = aDStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * FS
                End If
                Dim tgap As Double = 0.75 * tht + Math.Abs(aSpaceAdder)
                If aLeaderLength < 3 * asz Then aLeaderLength = 3 * asz
                aLdr = New dxeLeader(dxxLeaderTypes.LeaderText) With
                {
                    .DisplayStructure = eStruc,
                    .DimStyle = aDStyle,
                    .SuppressHook = True,
                    .PlaneV = aPln
                }
                If aTextJustification <> dxxVerticalJustifications.Undefined Then
                    aLdr.TextJustification = aTextJustification
                Else
                    aLdr.TextJustification = aImage.BaseSettings(dxxSettingTypes.DIMSETTINGS).Props.ValueI("LeaderTextJustification")
                End If
                If aArrowType = dxxArrowHeadTypes.Suppressed Then aLdr.SuppressArrowHead = True
                aXOffset = Math.Abs(aXOffset)
                verts2 = New TVECTORS(2)
                verts3 = New TVECTORS(3)
                aLeftLeaderAngle = Math.Round(Math.Abs(aLeftLeaderAngle), 6)
                aRightLeaderAngle = Math.Round(Math.Abs(aRightLeaderAngle), 6)
                Do Until aLeftLeaderAngle <= 180
                    aLeftLeaderAngle -= 180
                Loop
                Do Until aRightLeaderAngle <= 180
                    aRightLeaderAngle -= 180
                Loop
                aLeftLeaderAngle *= Math.PI / 180
                aRightLeaderAngle *= Math.PI / 180
                For k As Integer = 1 To 2
                    If k = 1 Then lPts = lftPts Else lPts = rghtPts
                    If lPts.Count <= 0 Then Continue For

                    Dim cnt As Integer = 0
                    Dim lLdr As dxeLeader = Nothing
                    Dim aDir As TVECTOR = yDir
                    Dim wd As Double = aPln.Width / 2 + aXOffset + asz
                    If k = 1 Then
                        d2 = (aLeaderLength * Math.Cos(aLeftLeaderAngle)) + asz
                        If wd < d2 Then wd = -d2
                        limLn.SPT = aPln.Origin + xDir * -wd
                        aDir.RotateAbout(zDir, aLeftLeaderAngle, True)
                    Else
                        d2 = (aLeaderLength * Math.Cos(aRightLeaderAngle)) + asz
                        If wd < d2 Then wd = d2
                        limLn.SPT = aPln.Origin + xDir * wd
                        aDir.RotateAbout(zDir, -aRightLeaderAngle, True)
                    End If
                    limLn.EPT = limLn.SPT + yDir * 10
                    For i As Integer = 1 To lPts.Count
                        cnt += 1
                        vrt = lPts.Item(i)
                        txt = vrt.Tag.Trim()
                        v1 = New TVECTOR(vrt)
                        v2 = v1 + aDir * aLeaderLength
                        If i = 1 And aInitYOffset <> 0 Then
                            v2 += aPln.YDirection * aInitYOffset
                        End If
                        v3 = dxfProjections.ToLine(v2, limLn, rOrthoDirection:=bDir, rDistance:=d1)

                        bLdr = aLdr.Clone
                        bLdr._MText = Nothing

                        bLdr.TextString = txt
                        bLdr.ImageGUID = iGUID
                        aFlg = Not bDir.Equals(xDir, False, 3)
                        If d1 <= asz Or (k = 1 And Not aFlg) Or (k = 2 And aFlg) Then
                            lVerts = verts2
                            lVerts.SetItem(1, v1)
                            lVerts.SetItem(2, v3)
                        Else
                            lVerts = verts3
                            lVerts.SetItem(1, v1)
                            lVerts.SetItem(2, v2)
                            lVerts.SetItem(3, v3)
                        End If
                        bLdr.VectorsV = New TVECTORS(lVerts)
                        bLdr.UpdatePath(True, True, aImage)
                        If cnt > 1 Then
                            'the last
                            lLdr.UpdatePath(True, True, aImage)
                            aTxt = lLdr.MText
                            'the current
                            bTxt = bLdr.MText
                            aTxt.UpdatePath(False, aImage)
                            bTxt.UpdatePath(False, aImage)
                            bTxt.OwnerGUID = ""
                            aRec = aTxt.Bounds
                            bRec = bTxt.Bounds
                            v1 = aRec.Point(dxxRectanglePts.BottomCenter)
                            v2 = v1 + yDir * -tgap
                            txtLn.SPT = v2
                            txtLn.EPT = v2 + xDir * 10
                            v3 = dxfProjections.ToLine(bRec.Point(dxxRectanglePts.TopCenter), txtLn, rOrthoDirection:=bDir, rDistance:=d1)

                            'don't move the text up!
                            If Not bDir.Equals(yDir, bCompareInverse:=False, aPrecis:=3) And d1 <> 0 Then
                                lVerts.Translate(bDir * d1, 2, lVerts.Count)
                                bLdr.LastRef += bDir * d1
                                'bTxt.Translate(bDir * d1)
                                bLdr.VectorsV = New TVECTORS(lVerts)
                                bLdr._MText = Nothing
                                bLdr.IsDirty = True
                            End If
                        End If
                        If Not bCreateOnly Then bLdr = aImage.SaveEntity(bLdr)
                        _rVal.Add(bLdr)
                        lLdr = bLdr
                        'Exit For
                    Next i

                Next k
                aImage = Nothing
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
            Finally
                aImage = Nothing
            End Try
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxoLeaderTool
End Namespace
