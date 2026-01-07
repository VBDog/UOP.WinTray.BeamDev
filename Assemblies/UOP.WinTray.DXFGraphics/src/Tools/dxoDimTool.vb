Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoDimTool
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
        Private _ImageGUID As String
        Friend ImagePtr As WeakReference
        Private _Status As String
        Private _LastStatus As String
        Private _LastErr As String
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage)
            If aImage IsNot Nothing Then
                _ImageGUID = aImage.GUID
                ImagePtr = New WeakReference(aImage)
            End If
            _Status = ""
            _LastStatus = ""
            _LastErr = ""
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property LastError As String
            Get
                Return _LastErr
            End Get
            Friend Set(value As String)
                If Not String.IsNullOrWhiteSpace(value) Then _LastErr = value
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
        Friend Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then _ImageGUID = "" Else _ImageGUID = value.Trim
                If ImagePtr Is Nothing And Not String.IsNullOrWhiteSpace(_ImageGUID) Then
                    Dim img As dxfImage = dxfEvents.GetImage(_ImageGUID)
                    If img IsNot Nothing Then ImagePtr = New WeakReference(img)
                End If
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function TickLine(aDimension As dxeDimension, aTckPt As Integer, Optional aOffset As Double? = 0, Optional aLength As Double? = Nothing, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLine
            Dim rErrString As String = String.Empty
            Return TickLine(aDimension, rErrString, aTckPt, aOffset, aLength, aLayer, aColor, aLineType, bCreateOnly, bSuppressErrors, aImage)
        End Function
        Public Function TickLine(aDimension As dxeDimension, ByRef rErrString As String, aTckPt As Integer, Optional aOffset As Double? = 0, Optional aLength As Double? = 0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeLine
            Dim _rVal As dxeLine = Nothing
            '#1the dimension to use to create the tick line
            '#2indicates which of the dimensions def points to use to create the tick line (only applies to linear dimensions)
            '#3the distance to offset the tick line from the point
            '#4the length to create the line (if not passed the images current dimsettings.DimTickLength is used and the dimscale of the dimension is applied)
            '#7flag to create the entity but not to add it to the image
            rErrString = ""
            If aDimension Is Nothing Then Return _rVal
            If aDimension.DimensionFamily = dxxDimensionTypes.Radial Then Return _rVal
            aDimension.GetImage(aImage)
            If Not GetImage(aImage) Then Return _rVal
            Dim aPl As TPLANE = aDimension.PlaneV
            Dim lng As Double = 0
            Dim tckL As dxeLine
            Dim v13 As TVECTOR
            Dim v14 As TVECTOR
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Try
                If aLength.HasValue Then lng = Math.Abs(aLength.Value)
                aDimension.UpdatePath(False, aImage)
                Dim oset As Double = 0
                If aOffset IsNot Nothing Then
                    If aOffset.HasValue Then oset = aOffset.Value
                End If
                If lng = 0 Then
                    lng = aImage.DimSettings.DimTickLength
                    lng *= aDimension.DimScale
                End If
                If lng = 0 Then Return _rVal
                If aDimension.DimensionFamily = dxxDimensionTypes.Linear Then
                    v13 = aDimension.DefPt13V.ProjectedTo(aPl)
                    v14 = aDimension.DefPt14V.ProjectedTo(aPl)
                    If aTckPt = 1 Then
                        tckL = aDimension.ExtensionLine1
                        v1 = v13
                        v2 = v14
                    Else
                        tckL = aDimension.ExtensionLine2
                        v2 = v13
                        v1 = v14
                    End If
                    If tckL Is Nothing Then Return _rVal
                    _rVal = New dxeLine(tckL) With {.IsDirty = True}

                    _rVal.LCLSet(aLayer, aColor, aLineType)
                    _rVal.Translate(v1.DirectionTo(v2) * oset)
                    _rVal.EndPt = _rVal.StartPt + _rVal.Direction() * lng
                Else
                    tckL = aDimension.ExtensionLine1
                    If tckL Is Nothing Then Return _rVal
                    _rVal = New dxeLine(tckL)
                    _rVal.LCLSet(aLayer, aColor, aLineType)
                    _rVal.IsDirty = True
                    _rVal.EndPt = _rVal.StartPt + _rVal.Direction() * lng
                    If aDimension.DimType = dxxDimTypes.OrdHorizontal Then
                        _rVal.Translate(aPl.XDirection * oset)
                    Else
                        _rVal.Translate(aPl.YDirection * oset)
                    End If
                End If
                If Not bCreateOnly And _rVal IsNot Nothing Then
                    _rVal = aImage.SaveEntity(_rVal)
                End If
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
        Public Function Angular(aLine1XY As iLine, aLine2XY As iLine, Optional aOffsetRadius As Double? = Nothing,
                                Optional aOffsetAngle As Double? = Nothing, Optional aPlacementPointXY As dxfVector = Nothing,
                                Optional aOverideText As String = Nothing, Optional aPrefix As String = Nothing, Optional aSuffix As String = Nothing,
                                Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                                Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                Optional aExtLineColor As dxxColors = dxxColors.Undefined,
                                Optional aDimLineColor As dxxColors = dxxColors.Undefined, Optional aTextColor As dxxColors = dxxColors.Undefined,
                                Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                                Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            If Not GetImage(aImage) Then Return Nothing
            Dim supFlags As TDIMLINESPRESSION = aImage.DimStyleOverrides.LineSuppressionFlags
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the first of two lines that forms the angle to be dimensioned
            '#2the second of two lines that forms the angle to be dimensioned
            '#3the distance to offset the text form the intersection point
            '#4the angle to offset the text form the intersection point (measured form the first line)
            '#5the pick point that indicates where the dimension text should be placed and the angle to dimension
            '#6a string to replace the actual dimension text with
            '#7a prefix string to add to the dimension text
            '#8a suffix string to add to the dimension text
            '#9the dimstyle to use (other than current)
            '#10the textstyle to use (other than dimstyles text style)
            '#11 the layer to put the dimension on (other than current)
            '#12 the color to assign to the dimension (other than current)
            '#13 the color to assign to the dimensions extension lines (other than the dim styles)
            '#14 the color to assign to the dimensions dimensions lines (other than the dim styles)
            '#15 the color to assign to the dimensions text (other than the dim styles)
            '#16 the linetype to assign to the dimension (other than current)
            '^used to draw an Angular dimension
            '~the passed line object must have a "StartPt" and "EndPt" object Property
            '~returns the dxeDimension that was created
            Dim dInput As TDIMINPUT
            Dim aLine As TLINE
            Dim bLine As TLINE
            Dim ppt As TVECTOR
            Dim aPl As dxfPlane = Nothing
            Dim ang As Double
            Dim d1 As Double
            Dim d2 As Double
            Dim aDir As TVECTOR
            Dim aFlg As Boolean
            Dim ip As TVECTOR
            Try
                LastError = ""
                'get the plane
                aImage.CreateVector(0, 0, 0, bSuppressUCS, bSuppressElevation, aPl)
                aLine = New TLINE(aLine1XY, aPl)
                bLine = New TLINE(aLine2XY, aPl)
                If aLine.Length = 0 Or bLine.Length = 0 Then Throw New Exception("Invalid Line Object Passed")
                ip = aLine.IntersectionPt(bLine, rInterceptExists:=aFlg)
                If Not aFlg Then Throw New Exception("Passed Lines Do Not Intersect")
                aPl.OriginV = ip
                If dxfProjections.DistanceTo(ip, aLine.SPT) > dxfProjections.DistanceTo(ip, aLine.EPT) Then aLine.Invert()
                If dxfProjections.DistanceTo(ip, bLine.SPT) > dxfProjections.DistanceTo(ip, bLine.EPT) Then bLine.Invert()
                If aPlacementPointXY IsNot Nothing Then
                    ppt = aImage.CreateVector(aPlacementPointXY, bSuppressUCS, bSuppressElevation, aPl).Strukture
                Else
                    aDir = aLine.Direction
                    If Not aOffsetAngle.HasValue Then
                        ang = aDir.AngleTo(bLine.Direction, aPl.ZDirectionV) / 2
                    Else
                        ang = aOffsetAngle.Value
                    End If

                    Dim orad As Double
                    If Not aOffsetRadius.HasValue Then
                        TVALUES.SortTwoValues(True, d1, d2)
                        orad = d2
                    Else
                        orad = aOffsetRadius.Value
                    End If
                    If ang <> 0 Then aDir.RotateAbout(New TVECTOR, aPl.ZDirectionV, ang, False, True)
                    ppt = ip + aDir * orad
                End If
                If String.IsNullOrWhiteSpace(aOverideText) Then aOverideText = ""
                If String.IsNullOrWhiteSpace(aPrefix) Then aPrefix = ""
                If String.IsNullOrWhiteSpace(aSuffix) Then aSuffix = ""
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimAngular, aLayerName:="", aColor:=dxxColors.Undefined, aLineType:="", aStyleName:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False)
                End If
                'initiaize
                dInput = New TDIMINPUT(aDimStyleName, aTextStyleName, dxxDimTypes.Angular) With {
                    .Plane = aPl.Strukture,
                    .XYPoint1 = aLine.SPT,
                    .XYPoint2 = aLine.EPT,
                    .XYPoint3 = bLine.SPT,
                    .XYPoint4 = bLine.EPT,
                    .XYPoint5 = ppt,
                      .OverideText = aOverideText,
                    .LayerName = aDisplaySettings.LayerName,
                    .Color = aDisplaySettings.Color,
                    .ExtLineColor = aExtLineColor,
                    .DimLineColor = aDimLineColor,
                    .TextColor = aTextColor,
                    .Linetype = aDisplaySettings.Linetype,
                    .ArrowSize = -1,
                    .Prefix = aPrefix,
                    .Suffix = aSuffix,
                    .DisplayVars = aDisplaySettings.Strukture
                }
                _rVal = xCreateDimension(aImage, dInput, Nothing, _LastErr)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                If Not bCreateOnly Then _rVal = aImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            Finally
                aImage.DimStyleOverrides.LineSuppressionFlags = supFlags
            End Try
            Return _rVal
        End Function
        Public Function Angular3P(aCenterXY As iVector, aDimPT1XY As iVector, aDimPT2XY As iVector,
                                  Optional aOffsetRadius As Double = 0.0, Optional aOffsetAngle As Double? = Nothing,
                                  Optional aPlacementPointXY As iVector = Nothing, Optional aOverideText As String = "",
                                  Optional aPrefix As String = Nothing, Optional aSuffix As String = Nothing,
                                  Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                  Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                                  Optional aExtLineColor As dxxColors = dxxColors.Undefined, Optional aDimLineColor As dxxColors = dxxColors.Undefined, Optional aTextColor As dxxColors = dxxColors.Undefined,
                                  Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                                  Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            If Not GetImage(aImage) Then Return Nothing
            Dim supFlags As TDIMLINESPRESSION = aImage.DimStyleOverrides.LineSuppressionFlags
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the center point of the three points that form the angle to be dimensioned
            '#2the first of the three points that forms the angle to be dimensioned
            '#3the second of the three points that forms the angle to be dimensioned
            '#4the distance to offset the text form the intersection point
            '#5the angle to offset the text form the intersection point (measured form the first line)
            '#6the pick point that indicates where the dimension text should be placed and the angle to dimension
            '#7a string to replace the actual dimension text with
            '#8a prefix string to add to the dimension text
            '#9a suffix string to add to the dimension text
            '#10the dimstyle to use (other than current)
            '#11the textstyle to use (other than dimstyles text style)
            '#12 the layer to put the dimension on (other than current)
            '#13 the color to assign to the dimension (other than current)
            '#14 the color to assign to the dimensions extension lines (other than the dim styles)
            '#15 the color to assign to the dimensions dimensions lines (other than the dim styles)
            '#16 the color to assign to the dimensions text (other than the dim styles)
            '#17 the linetype to assign to the dimension (other than current)
            '^used to draw an Angular dimension
            '~returns the dxeDimension that was created
            Try
                LastError = ""
                Dim aPl As dxfPlane = Nothing
                'get the plane
                aImage.CreateUCSVector(TVECTOR.Zero, bSuppressUCS, bSuppressElevation, aPl)
                Dim ip As TVECTOR = aImage.CreateUCSVector(aCenterXY, bSuppressUCS, bSuppressElevation, aPl)
                Dim v1 As TVECTOR = aImage.CreateUCSVector(aDimPT1XY, bSuppressUCS, bSuppressElevation, aPl)
                Dim v2 As TVECTOR = aImage.CreateUCSVector(aDimPT2XY, bSuppressUCS, bSuppressElevation, aPl)
                Dim d1 As Double
                Dim aDir As TVECTOR = ip.DirectionTo(v1, False, d1)
                If Math.Round(d1, 6) <= 0.0001 Then Throw New Exception("Coincident Vectors Passed")
                Dim d2 As Double
                Dim bDir As TVECTOR = ip.DirectionTo(v2, False, d2)
                If Math.Round(d2, 6) <= 0.0001 Then Throw New Exception("Coincident Vectors Passed")
                Dim zDir As TVECTOR = aDir.CrossProduct(bDir, True)
                Dim basePl As New TPLANE(ip, aDir, zDir.CrossProduct(aDir, True))
                Dim ppt As TVECTOR
                Dim ang As Object = 0
                Dim dInput As TDIMINPUT
                'get or create the text placement point (angle and distance)
                If aPlacementPointXY IsNot Nothing Then
                    ppt = aImage.CreateUCSVector(aPlacementPointXY, bSuppressUCS, bSuppressElevation, aPl)
                Else
                    If aOffsetAngle.HasValue Then
                        ang = aOffsetAngle.Value
                    Else
                        ang = aDir.AngleTo(bDir, aPl.ZDirectionV) / 2
                    End If

                    If aOffsetRadius = 0 Then
                        TVALUES.SortTwoValues(True, d1, d2)
                        aOffsetRadius = d2
                    End If
                    ppt = ip + (basePl.Direction(ang) * aOffsetRadius)
                End If
                If String.IsNullOrWhiteSpace(aOverideText) Then aOverideText = ""
                If String.IsNullOrWhiteSpace(aPrefix) Then aPrefix = ""
                If String.IsNullOrWhiteSpace(aSuffix) Then aSuffix = ""
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimAngular3P, aLayerName:="", aColor:=dxxColors.Undefined, aLineType:="", aStyleName:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False)
                End If
                dInput = New TDIMINPUT(aDimStyleName, aTextStyleName, dxxDimTypes.Angular3P) With {
                    .Plane = aPl.Strukture,
                    .XYPoint1 = ip,
                    .XYPoint2 = v1,
                    .XYPoint3 = v2,
                    .XYPoint4 = ppt,
                    .Prefix = aPrefix,
                    .Suffix = aSuffix,
                    .OverideText = aOverideText,
                    .LayerName = aDisplaySettings.LayerName,
                    .Color = aDisplaySettings.Color,
                    .ExtLineColor = aExtLineColor,
                    .DimLineColor = aDimLineColor,
                    .TextColor = aTextColor,
                    .Linetype = aDisplaySettings.Linetype,
                    .ArrowSize = -1,
                    .DisplayVars = aDisplaySettings.Strukture}
                _rVal = xCreateDimension(aImage, dInput, Nothing, _LastErr)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                If Not bCreateOnly Then
                    _rVal = aImage.SaveEntity(_rVal)
                End If
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            Finally
                aImage.DimStyleOverrides.LineSuppressionFlags = supFlags
            End Try
            Return _rVal
        End Function
        Public Function Vertical(aPoint As iVector, bPoint As iVector, aDimOffset As Double, Optional aTextOffset As Double = 0.0,
                                 Optional aPrefix As String = "", Optional aSuffix As String = "",
                                 Optional bAbsolutePlacement As Boolean = False, Optional aOverideText As String = "",
                                 Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                 Optional aDimStyle As String = "", Optional aTextStyle As String = "",
                                 Optional aExtLineColor As dxxColors = dxxColors.Undefined, Optional aDimLineColor As dxxColors = dxxColors.Undefined,
                                Optional aTextColor As dxxColors = dxxColors.Undefined,
                                Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False,
                                 Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            If Not GetImage(aImage) Then Return Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the first dimension point
            '#2the second dimension point
            '#3the distance to offset the dimension lines
            '#4the distance to offset the dimension text
            '#5a prefix string to add to the dimension text
            '#6a suffix string to add to the dimension text
            '#7flag to indicate that the aDimOffset value is an actual ordinate to place the dimension lines
            '#8a string to replace the actual dimension text with
            '#9the dimstyle to use (other than current)
            '#10the textstyle to use (other than the dimstyles)
            '#11a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#12a color to apply to the entity instead of the current color
            '#13a line type to apply to the entity instead of the current line type setting
            '^used to create and draw Linear Vertical Dimension
            '~ the function returns the created dxeDimension Entity.
            '~colors assigned to dimension entities are always set to the nearest acl color (0 to 256)
            Dim _rVal As dxeDimension = Nothing
            _LastErr = ""
            Try
                _rVal = Linear(dxxLinearDimTypes.LinearVertical, aPoint, bPoint, aDimOffset, aTextOffset,
    aPrefix, aSuffix, bAbsolutePlacement, aOverideText, aDimStyle, aTextStyle,
     aDisplaySettings, aExtLineColor, aDimLineColor, aTextColor, bSuppressUCS, bSuppressElevation, bCreateOnly, True, aImage)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function Linear(aDimensionType As dxxLinearDimTypes, aXYPoint1 As iVector, aXYPoint2 As iVector, aDimOffset As Double,
                               Optional aTextOffset As Double = 0.0, Optional aPrefix As String = "", Optional aSuffix As String = "",
                               Optional aAbsolutePlacement As Boolean = False, Optional aOverideText As String = "",
                               Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                               Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                               Optional aExtLineColor As dxxColors = dxxColors.Undefined, Optional aDimLineColor As dxxColors = dxxColors.Undefined,
                               Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                               Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            LastError = ""
            If Not GetImage(aImage) Then Return Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the type of linear dimension being requested (aligned, vertical or horizontal)
            '#2the first dimension point (world coordinates only)
            '#3the second dimension point (world coordinates only)
            '#4the distance to offset the dimension lines (in screen inches)
            '#5the distance to offset the dimension text (in screen inches)
            '#6a prefix string to add to the dimension text
            '#7a suffix string to add to the dimension text
            '#8flag to indicate that the DimOffset value is an actual ordinate to place the dimension lines
            '#9a string to replace the actual dimension text with
            '#10the dimstyle to use (other than current)
            '#11the textstyle to use (other than dimstyles text style)
            '#12 the layer to put the dimension on (other than current)
            '#13 the color to assign to the dimension (other than current)
            '#14 the color to assign to the dimensions extension lines (other than the dim styles)
            '#15 the color to assign to the dimensions dimensions lines (other than the dim styles)
            '#16 the color to assign to the dimensions text (other than the dim styles)
            '#17 the linetype to assign to the dimension (other than current)
            '^used to create and draw Linear and Aligned
            '~ the function returns the created dxeDimension Entity.
            '~colors assigned to dimension entities are always set to the nearest acl color (0 to 256)
            Try
                If Not dxfEnums.Validate(GetType(dxxLinearDimTypes), aDimensionType, bSkipNegatives:=True) Then Throw New Exception("Invalid Linear Dimension Type Requested")
                '     If DimOffset = 0 And Not AbsolutePlacement Then Throw New Exception( "DimOffset Passed as Zero Not Allowed")
                'get the plane
                Dim aPl As dxfPlane = Nothing
                aImage.CreateVector(0, 0, 0, bSuppressUCS, bSuppressElevation, aPl)
                'Dim dInput As TDIMINPUT
                'initiaize
                Dim P1 As dxfVector = aImage.CreateVector(aXYPoint1, bSuppressUCS, bSuppressElevation, aPl)
                Dim P2 As dxfVector = aImage.CreateVector(aXYPoint2, bSuppressUCS, bSuppressElevation, aPl)
                Dim dType As dxxDimTypes
                If aDimensionType = dxxLinearDimTypes.LinearAligned Then
                    dType = dxxDimTypes.LinearAligned
                ElseIf aDimensionType = dxxLinearDimTypes.LinearHorizontal Then
                    dType = dxxDimTypes.LinearHorizontal
                Else
                    dType = dxxDimTypes.LinearVertical
                End If
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearA, aLayerName:="", aColor:=dxxColors.Undefined, aLineType:="", aStyleName:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False)
                End If
                Dim dInput As New TDIMINPUT(aDimStyleName, aTextStyleName, dType) With {
                   .Plane = aPl.Strukture,
                    .XYPoint1 = P1.Strukture,
                    .XYPoint2 = P2.Strukture,
                    .DimOffset = aDimOffset,
                    .TextOffset = aTextOffset,
                    .Prefix = aPrefix,
                    .Suffix = aSuffix,
                    .AbsolutePlacement = aAbsolutePlacement,
                    .OverideText = aOverideText,
                    .DimStyleName = aDimStyleName,
                    .TextStyleName = aTextStyleName,
                    .LayerName = aDisplaySettings.LayerName,
                    .Color = aDisplaySettings.Color,
                    .Linetype = aDisplaySettings.Linetype,
                    .ExtLineColor = aExtLineColor,
                    .DimLineColor = aDimLineColor,
                    .TextColor = aTextColor,
                    .ArrowSize = -1}
                _rVal = xCreateDimension(aImage, dInput, Nothing, _LastErr)
                If Not String.IsNullOrWhiteSpace(_LastErr) Then Throw New Exception(_LastErr)
                If Not bCreateOnly Then
                    _rVal = aImage.SaveEntity(_rVal)
                End If
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
Err:
        End Function
        Public Function Aligned(aPoint As iVector, bPoint As iVector, aDimOffset As Double, Optional aTextOffset As Double = 0.0,
                                 Optional aPrefix As String = "", Optional aSuffix As String = "",
                                 Optional bAbsolutePlacement As Boolean = False, Optional aOverideText As String = "",
                                 Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                 Optional aDimStyle As String = "", Optional aTextStyle As String = "",
                                 Optional aExtLineColor As dxxColors = dxxColors.Undefined, Optional aDimLineColor As dxxColors = dxxColors.Undefined,
                                Optional aTextColor As dxxColors = dxxColors.Undefined,
                                Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False,
                                 Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            If Not GetImage(aImage) Then Return Nothing
            Dim _rVal As dxeDimension = Nothing
            Try
                LastError = ""
                _rVal = Linear(dxxLinearDimTypes.LinearAligned, aPoint, bPoint, aDimOffset, aTextOffset,
    aPrefix, aSuffix, bAbsolutePlacement, aOverideText, aDimStyle, aTextStyle,
     aDisplaySettings, aExtLineColor, aDimLineColor, aTextColor, bSuppressUCS, bSuppressElevation, bCreateOnly, True, aImage)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function Horizontal(aPoint As iVector, bPoint As iVector, aDimOffset As Double, Optional aTextOffset As Double = 0.0,
                                 Optional aPrefix As String = "", Optional aSuffix As String = "",
                                 Optional bAbsolutePlacement As Boolean = False, Optional aOverideText As String = "",
                                 Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                 Optional aDimStyle As String = "", Optional aTextStyle As String = "",
                                 Optional aExtLineColor As dxxColors = dxxColors.Undefined, Optional aDimLineColor As dxxColors = dxxColors.Undefined,
                                Optional aTextColor As dxxColors = dxxColors.Undefined,
                                Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False,
                                 Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            If Not GetImage(aImage) Then Return Nothing
            Dim _rVal As dxeDimension = Nothing
            Try
                LastError = ""
                _rVal = Linear(dxxLinearDimTypes.LinearHorizontal, aPoint, bPoint, aDimOffset, aTextOffset,
    aPrefix, aSuffix, bAbsolutePlacement, aOverideText, aDimStyle, aTextStyle,
     aDisplaySettings, aExtLineColor, aDimLineColor, aTextColor, bSuppressUCS, bSuppressElevation, bCreateOnly, True, aImage)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function OrdinateH(aBasePt As iVector, aDimPt As iVector, aDimOffset As Double,
                               Optional aTextOffset As Double = 0.0, Optional aPrefix As String = "", Optional aSuffix As String = "",
                                Optional aTextAngle As Double = 0,
                                Optional aAbsolutePlacement As Boolean = False, Optional aOverideText As String = "",
                               Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                               Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                               Optional aExtLineColor As dxxColors = dxxColors.Undefined,
                               Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                               Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            If Not GetImage(aImage) Then Return Nothing
            Dim _rVal As dxeDimension = Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the base dimension point (world coordinates only)
            '#2the dimension point (world coordinates only)
            '#3the distance to offset the dimension lines (in screen inches)
            '#4the distance to offset the dimension text (in screen inches)
            '#5a prefix string to add to the dimension text
            '#6a suffix string to add to the dimension text
            '#7flag to indicate that the DimOffset value is an actual ordinate to place the dimension lines
            '#8a string to replace the actual dimension text with
            '#9the dimstyle to use (other than current)
            '#10the textstyle to use (other than dimstyles text style)
            '#11 the layer to put the dimension on (other than current)
            '#12 the color to assign to the dimension (other than current)
            '#13 the color to assign to the dimensions lines (other than the dim styles)
            '#14 the color to assign to the dimensions text (other than the dim styles)
            '#15 the linetype to assign to the dimension (other than current)
            '#18an override text angle for the dimension text
            '^used to create and draw verical and horizontal ordinate dimensions
            '~ the function returns the created dxeDimension Entity.
            '~if AbsolutePlacement is False then the DimOffset and TextOffset are interpreted relative offsets from the dimension ppoint in screen inches based on the current zoom.
            '~if AbsolutePlacement is True then the DimOffset and TextOffset are interpreted as the desired X and Y ordinates of end of the leader line.
            '~colors assigned to dimension entities are always set to the nearest acl color (0 to 256).
            LastError = ""
            Try
                _rVal = Ordinate(dxxOrdinateDimTypes.OrdHorizontal, aBasePt, aDimPt, aDimOffset, aTextOffset, aPrefix, aSuffix, aTextAngle, aAbsolutePlacement, aOverideText, aDimStyleName, aTextStyleName, aDisplaySettings, aExtLineColor, aTextColor, bSuppressUCS, bSuppressElevation, bCreateOnly, True, aImage)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function Ordinate(aDimensionType As dxxOrdinateDimTypes, aBasePt As iVector, aDimPt As iVector, aDimOffset As Double,
                               Optional aTextOffset As Double = 0.0, Optional aPrefix As String = "", Optional aSuffix As String = "",
                                Optional aTextAngle As Double = 0,
                                Optional aAbsolutePlacement As Boolean = False, Optional aOverideText As String = "",
                               Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                               Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                               Optional aExtLineColor As dxxColors = dxxColors.Undefined,
                               Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                               Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            If Not GetImage(aImage) Then Return Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the type of ordinate dimension being requested (vertical or horizontal)
            '#2the base dimension point (world coordinates only)
            '#3the dimension point (world coordinates only)
            '#4the distance to offset the dimension lines (in screen inches)
            '#5the distance to offset the dimension text (in screen inches)
            '#6a prefix string to add to the dimension text
            '#7a suffix string to add to the dimension text
            '#8flag to indicate that the DimOffset value is an actual ordinate to place the dimension lines
            '#9a string to replace the actual dimension text with
            '#10indicates one of the two passed dimension points to add a dimension tick to
            '#11indicates the distance to offset the the dimension tick from the indicated dimension point
            '#12the dimstyle to use (other than current)
            '#13the textstyle to use (other than dimstyles text style)
            '#14 the layer to put the dimension on (other than current)
            '#15 the color to assign to the dimension (other than current)
            '#16 the color to assign to the dimensions lines (other than the dim styles)
            '#17 the color to assign to the dimensions text (other than the dim styles)
            '#18 the linetype to assign to the dimension (other than current)
            '#19an override text angle for the dimension text
            '^used to create and draw verical and horizontal ordinate dimensions
            '~ the function returns the created dxeDimension Entity.
            '~if AbsolutePlacement is False then the DimOffset and TextOffset are interpreted relative offsets from the dimension ppoint in screen inches based on the current zoom.
            '~if AbsolutePlacement is True then the DimOffset and TextOffset are interpreted as the desired X and Y ordinates of end of the leader line.
            '~colors assigned to dimension entities are always set to the nearest acl color (0 to 256).
            Try
                If aDimensionType <> dxxOrdinateDimTypes.OrdHorizontal And aDimensionType <> dxxOrdinateDimTypes.OrdVertical Then Throw New Exception("Unknow Ordinate Dimension Type Requested")
                Dim P1 As TVECTOR
                Dim P2 As TVECTOR
                Dim dInput As TDIMINPUT
                Dim aPl As dxfPlane = Nothing
                'get the plane
                aImage.CreateVector(0, 0, 0, bSuppressUCS, bSuppressElevation, aPl)
                'initiaize
                If aBasePt Is Nothing Then
                    P1 = aPl.OriginV
                Else
                    P1 = New TVECTOR(aBasePt)
                End If
                If aDimPt Is Nothing Then
                    P2 = P1.Clone
                Else
                    P2 = New TVECTOR(aDimPt)
                End If
                ' P1 = aImage.CreateVector(aBasePtXY, True, bSuppressElevation, aPl)
                'P2 = aImage.CreateVector(aDimPt, True, bSuppressElevation, aPl)
                Dim dType As dxxDimTypes = dxxDimTypes.OrdHorizontal
                If aDimensionType = dxxOrdinateDimTypes.OrdVertical Then dType = dxxDimTypes.OrdVertical
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimOrdinateH, aLayerName:="", aColor:=dxxColors.Undefined, aLineType:="", aStyleName:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False)
                End If
                dInput = New TDIMINPUT(aDimStyleName, aTextStyleName, dType) With {
                .XYPoint1 = P1,
                    .XYPoint2 = P2,
                    .Plane = aPl.Strukture,
                    .DimOffset = aDimOffset,
                    .TextOffset = aTextOffset,
                    .Prefix = aPrefix,
                    .Suffix = aSuffix,
                    .AbsolutePlacement = aAbsolutePlacement,
                    .OverideText = aOverideText,
                .LayerName = aDisplaySettings.LayerName,
                .Color = aDisplaySettings.Color,
                    .Linetype = aDisplaySettings.Linetype,
                    .ExtLineColor = aExtLineColor,
                    .DimLineColor = Nothing,
                    .TextColor = aTextColor,
                    .aSingle = aTextAngle,
                    .ArrowSize = -1}
                _rVal = xCreateDimension(aImage, dInput, Nothing, _LastErr)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                If Not bCreateOnly Then
                    _rVal = aImage.SaveEntity(_rVal)
                End If
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function OrdinateV(aBasePt As iVector, aDimPt As iVector, aDimOffset As Double,
                               Optional aTextOffset As Double = 0.0, Optional aPrefix As String = "", Optional aSuffix As String = "",
                                Optional aTextAngle As Double = 0,
                                Optional aAbsolutePlacement As Boolean = False, Optional aOverideText As String = "",
                               Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                               Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                               Optional aExtLineColor As dxxColors = dxxColors.Undefined,
                               Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bCreateOnly As Boolean = False,
                               Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As dxeDimension
            If Not GetImage(aImage) Then Return Nothing
            Dim _rVal As dxeDimension = Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the base dimension point (world coordinates only)
            '#2the dimension point (world coordinates only)
            '#3the distance to offset the dimension lines (in screen inches)
            '#4the distance to offset the dimension text (in screen inches)
            '#5a prefix string to add to the dimension text
            '#6a suffix string to add to the dimension text
            '#7flag to indicate that the DimOffset value is an actual ordinate to place the dimension lines
            '#8a string to replace the actual dimension text with
            '#9the dimstyle to use (other than current)
            '#10the textstyle to use (other than dimstyles text style)
            '#11 the layer to put the dimension on (other than current)
            '#12 the color to assign to the dimension (other than current)
            '#13 the color to assign to the dimensions lines (other than the dim styles)
            '#14 the color to assign to the dimensions text (other than the dim styles)
            '#15 the linetype to assign to the dimension (other than current)
            '#18an override text angle for the dimension text
            '^used to create and draw verical and horizontal ordinate dimensions
            '~ the function returns the created dxeDimension Entity.
            '~if AbsolutePlacement is False then the DimOffset and TextOffset are interpreted relative offsets from the dimension ppoint in screen inches based on the current zoom.
            '~if AbsolutePlacement is True then the DimOffset and TextOffset are interpreted as the desired X and Y ordinates of end of the leader line.
            '~colors assigned to dimension entities are always set to the nearest acl color (0 to 256).
            LastError = ""
            Try
                _rVal = Ordinate(dxxOrdinateDimTypes.OrdVertical, aBasePt, aDimPt, aDimOffset, aTextOffset, aPrefix, aSuffix, aTextAngle, aAbsolutePlacement, aOverideText, aDimStyleName, aTextStyleName, aDisplaySettings, aExtLineColor, aTextColor, bSuppressUCS, bSuppressElevation, bCreateOnly, True, aImage)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function Radial(aDimensionType As dxxRadialDimensionTypes, aArc As dxeArc,
                               aPlacementAngle As Double, Optional aPlacementDistance As Double = 0.0, Optional aCenterMarkSize As Double? = Nothing,
                               Optional aPrefix As String = "", Optional aSuffix As String = "", Optional aOverideText As String = "",
                               Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                               Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                               Optional aMarkColor As dxxColors = dxxColors.Undefined, Optional aLeaderColor As dxxColors = dxxColors.Undefined,
                               Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False,
                               Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing,
                               Optional aOverridePrecision As Integer? = Nothing) As dxeDimension
            If Not GetImage(aImage) Then Return Nothing
            Dim _rVal As dxeDimension = Nothing
            Dim supFlags As TDIMLINESPRESSION
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the radial dimension type being requested (radius or diametric)
            '#2the arc that is being dimensioned
            '#3The angle measured counterclockwise from the positive X axis to place the dimension text
            '#4the distance from the curves center to place the dimension text
            '#5a prefix string to add to the dimension text
            '#6a suffix string to add to the dimension text
            '#7a string to replace the actual dimension text with
            '#9 flag to suppress then centermarks
            '^used to draw an Radial aDim
            '~returns the dxeDimension that was created
            Try


                LastError = ""
                If aArc Is Nothing Then Throw New Exception("the Passed Arc Is Undefined")
                supFlags = aImage.DimStyleOverrides.LineSuppressionFlags
                'get the plane
                Dim aPl As dxfPlane = aImage.UCS
                Dim dType As dxxDimTypes = dxxDimTypes.Radial
                If aDimensionType = dxxRadialDimensionTypes.Diametric Then dType = dxxDimTypes.Diametric
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimRadialR, aLayerName:=String.Empty, aColor:=dxxColors.Undefined, aLineType:=String.Empty, aStyleName:=String.Empty, aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False)
                End If
                Dim dInput As TDIMINPUT = New TDIMINPUT(aDimStyleName, aTextStyleName, dType) With {
                     .PlacementAngle = aPlacementAngle,
                    .PlacementDistance = aPlacementDistance,
                    .Prefix = aPrefix,
                    .Suffix = aSuffix,
                    .Plane = aPl.Strukture,
                    .OverideText = aOverideText,
                    .DimStyleName = aDimStyleName,
                    .TextStyleName = aTextStyleName,
                    .LayerName = aDisplaySettings.LayerName,
                    .ExtLineColor = aLeaderColor,
                    .DimLineColor = aMarkColor,
                    .TextColor = aTextColor,
                    .Color = aDisplaySettings.Color,
                    .Linetype = aDisplaySettings.Linetype,
                    .CenterMarkSize = aCenterMarkSize,
                    .ArrowSize = -1,
                    .LinearPrecision = aOverridePrecision}
                _rVal = xCreateDimension(aImage, dInput, aArc, _LastErr)
                If Not String.IsNullOrWhiteSpace(_LastErr) Then Throw New Exception(_LastErr)
                If Not bCreateOnly Then
                    _rVal = aImage.SaveEntity(_rVal)
                End If
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            Finally
                aImage.DimStyleOverrides.LineSuppressionFlags = supFlags
            End Try
Err:
        End Function
        Public Function RadialR(aArc As dxeArc,
                               aPlacementAngle As Double, Optional aPlacementDistance As Double = 0.0, Optional aCenterMarkSize As Double? = Nothing,
                               Optional aPrefix As String = "", Optional aSuffix As String = "", Optional aOverideText As String = "",
                               Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                               Optional aDisplaySetings As dxfDisplaySettings = Nothing,
                               Optional aMarkColor As dxxColors = dxxColors.Undefined, Optional aLeaderColor As dxxColors = dxxColors.Undefined,
                               Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False,
                               Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing,
                                Optional aOverridePrecision As Integer? = Nothing) As dxeDimension
            LastError = ""
            Dim _rVal As dxeDimension = Nothing
            If Not GetImage(aImage) Then Return Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the arc that is being dimensioned
            '#2The angle measured counterclockwise from the positive X axis to place the dimension text
            '#3the distance from the curves center to place the dimension text
            '#4a prefix string to add to the dimension text
            '#5a suffix string to add to the dimension text
            '#6a string to replace the actual dimension text with
            '#7 flag to suppress then centermarks
            Try
                _rVal = Radial(dxxRadialDimensionTypes.Radial, aArc, aPlacementAngle, aPlacementDistance, aCenterMarkSize, aPrefix, aSuffix, aOverideText, aDimStyleName, aTextStyleName, aDisplaySetings, aMarkColor, aLeaderColor, aTextColor, bSuppressUCS, bSuppressElevation, bCreateOnly, True, aImage, aOverridePrecision)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function RadialD(aArc As dxeArc,
                               aPlacementAngle As Double, Optional aPlacementDistance As Double = 0.0, Optional aCenterMarkSize As Double? = Nothing,
                               Optional aPrefix As String = "", Optional aSuffix As String = "", Optional aOverideText As String = "",
                               Optional aDimStyleName As String = "", Optional aTextStyleName As String = "",
                               Optional aDisplaySetings As dxfDisplaySettings = Nothing,
                               Optional aMarkColor As dxxColors = dxxColors.Undefined, Optional aLeaderColor As dxxColors = dxxColors.Undefined,
                               Optional aTextColor As dxxColors = dxxColors.Undefined,
                               Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False,
                               Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing,
                                Optional aOverridePrecision As Integer? = Nothing) As dxeDimension
            LastError = ""
            Dim _rVal As dxeDimension = Nothing
            If Not GetImage(aImage) Then Return Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the arc that is being dimensioned
            '#2The angle measured counterclockwise from the positive X axis to place the dimension text
            '#3the distance from the curves center to place the dimension text
            '#4a prefix string to add to the dimension text
            '#5a suffix string to add to the dimension text
            '#6a string to replace the actual dimension text with
            '#7 flag to suppress then centermarks
            Try
                _rVal = Radial(dxxRadialDimensionTypes.Diametric, aArc, aPlacementAngle, aPlacementDistance, aCenterMarkSize, aPrefix, aSuffix, aOverideText, aDimStyleName, aTextStyleName, aDisplaySetings, aMarkColor, aLeaderColor, aTextColor, bSuppressUCS, bSuppressElevation, bCreateOnly, True, aImage, aOverridePrecision)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function Stack_Horizontal(aDimPointsXY As IEnumerable(Of iVector), aDimOffset As Double,
                                         Optional aAdditionalDimSpace As Double = 0.0, Optional aBotToTopLeftToRight As Boolean = True,
                                         Optional bTagsAsPrefixes As Boolean = False, Optional bFlagsAsSuffixes As Boolean = False,
                                         Optional aAlignTextToDimLines As Boolean = False, Optional aSuppressBaseExtLine As Boolean = False,
                                         Optional aDimStyle As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing,
                                         Optional bCreateOnly As Boolean = False,
                                         Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the collection of points to dimension from the base point
            '#2the distance to offset the first dimension line
            '#3an adder to increase the distance between subsequent dimensions.
            '#4a flag indicating that the passed vectors current Tag property values should be used as the prefix for the dimension created with the point
            '#5a flag indicating that the passed vectors current Flag property values should be used as the suffix for the dimension created with the point
            '#6flag to work left to right or right to left.
            '#7flag to align the dimension text with the dim lines
            '#8flag to suppress the base extension line
            '#9a dim style to use other than the current dim style
            '#10a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#11a color to apply to the entity instead of the current color
            '#12a line type to apply to the entity instead of the current line type setting
            '#13if passed the dimensions are created on this plane otherwise the current UCS XY plane is used
            '#14flag to just create the dimensions but not to add them to the image
            '^used to create a stack of Horizontal linear dimensions
            '~returns a collection of the created dimension.
            '~a negative DimSpace value will cause the stack to extend to the left.
            '~an example of the best use for it would be dimensioning several points from a single centerline.
            Try
                _rVal = xStackHV(dxxDimTypes.LinearHorizontal, aDimPointsXY, aDimOffset, aAdditionalDimSpace, aBotToTopLeftToRight, bTagsAsPrefixes, bFlagsAsSuffixes, aAlignTextToDimLines, aSuppressBaseExtLine, aDimStyle, aDisplaySettings, aPlane, aCollector, bCreateOnly, True, aImage)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors And aImage IsNot Nothing Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
Err:
        End Function
        Public Function Stack_Vertical(aDimPointsXY As IEnumerable(Of iVector), aDimOffset As Double,
                                         Optional aAdditionalDimSpace As Double = 0.0, Optional aBotToTopLeftToRight As Boolean = True,
                                         Optional bTagsAsPrefixes As Boolean = False, Optional bFlagsAsSuffixes As Boolean = False,
                                         Optional aAlignTextToDimLines As Boolean = False, Optional aSuppressBaseExtLine As Boolean = False,
                                         Optional aDimStyle As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing,
                                         Optional bCreateOnly As Boolean = False,
                                         Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the collection of points to dimension from the base point
            '#2the distance to offset the first dimension line
            '#3an adder to increase the distance between subsequent dimensions.
            '#4a flag indicating that the passed vectors current Tag property values should be used as the prefix for the dimension created with the point
            '#5a flag indicating that the passed vectors current Flag property values should be used as the suffix for the dimension created with the point
            '#6flag to work top to bottom or bottom to top for vertical dimensions
            '#7flag to align the dimension text with the dim lines
            '#8flag to suppress the base extension line
            '#9a dim style to use other than the current dim style
            '#10a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#11a color to apply to the entity instead of the current color
            '#12a line type to apply to the entity instead of the current line type setting
            '#13if passed the dimensions are created on this plane otherwise the current UCS XY plane is used
            '#14flag to just create the dimensions but not to add them to the image
            '^used to create a stack of Vertical linear dimensions
            '~returns a collection of the created dimension.
            '~a negative DimSpace value will cause the stack to extend downward.
            '~an example of the best use for it would be dimensioning several points from a single centerline.
            Try
                _rVal = xStackHV(dxxDimTypes.LinearVertical, aDimPointsXY, aDimOffset, aAdditionalDimSpace, aBotToTopLeftToRight, bTagsAsPrefixes, bFlagsAsSuffixes, aAlignTextToDimLines, aSuppressBaseExtLine, aDimStyle, aDisplaySettings, aPlane, aCollector, bCreateOnly, True, aImage)
                If _LastErr <> "" Then Throw New Exception(_LastErr)
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors And aImage IsNot Nothing Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Public Function Stack_Concentric(aDimType As dxxLinearDimTypes, aPointSetsXY As IEnumerable(Of iVector), Optional aDimOffset As Double = 0.2,
                                         Optional aOffsetIsAbsolute As Boolean = False, Optional aStepText As Boolean = False,
                                         Optional aStackRightOrDown As Boolean = False, Optional aAlignText As Boolean = False,
                                         Optional aTextOffset As Double = 0.0, Optional aAdditionalSpace As Double = 0.0,
                                         Optional aDimStyle As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                         Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing,
                                         Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As colDXFEntities
            LastError = ""
            Dim _rVal As New colDXFEntities
            If Not GetImage(aImage) Then Return _rVal
            '#1the type of dimensions to generate (linear vertical or horizontal)
            '#2a collection of vectors to dimension
            '#3the starting offset distance
            '#4flag indicating that the offset is an actual ordinate rather than a screen distance
            '#5flag to step the text for each subsequent dimension
            '#6used to invert the direction of the stack default is left for vertical dims and up for horizontal dims
            '#7flag to align the dimension text with the dim lines
            '#8a distance to offset the dim text form the default position
            '#9 and additional space to spread or compress the stack
            '#10a dim style to use other than the current dim style
            '#11a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#12a color to apply to the entity instead of the current color
            '#13a line type to apply to the entity instead of the current line type setting
            '#14if passed the dimensions are created on this plane otherwise the current UCS XY plane is used
            '^a composite dimensioning function that can be used to neatly dimension point sets that are concentric about
            '^either a horizontal or vertical centerline. The vectors are match into pairs the firts to the last, the second to the second last etc.
            '~this function attempts to neatly stack vertical or horizontal dimensions drawn from the collection of passed vectors.
            '~if the Tag property of each point pair is the same the tag is used as the prefix text of the created dimension.
            '~if the Flag property of each point pair is the same then flag used as the suffix text of the created dimension.
            '~if the Value property of each point pair is the same then then value used as the override text of the created dimension.
            '~an example of the best use for it would be dimensioning the diameters of concentric circles.
            Try
                Dim dPoints As colDXFVectors
                Dim dType As dxxDimTypes = dxxDimTypes.LinearHorizontal
                Dim P1 As dxfVector
                Dim P2 As dxfVector
                Dim aDim As dxeDimension
                Dim bDim As dxeDimension
                Dim lDim As dxeDimension = Nothing
                Dim bPlane As dxfPlane
                Dim aPrecis As Integer
                Dim iGUID As String
                Dim i As Integer
                Dim idx As Integer
                Dim dInput As TDIMINPUT
                Dim eStruc As New TDISPLAYVARS
                Dim dVecs As TVECTORS
                Dim bRec As New TPLANE("")
                Dim aLn As New TLINE
                Dim osetDir As TVECTOR
                Dim aPairs As New TPAIRS
                Dim aPair As New TVECTORPAIR
                Dim bPairs As New TPAIRS
                '**UNUSED VAR** Dim tpt As TVECTOR
                Dim dpt As TVECTOR
                Dim dSty As dxoDimStyle = Nothing
                Dim tSty As dxoStyle = Nothing
                Dim tstep As Double
                Dim d1 As Double
                Dim cnt As Integer
                Dim dcnt As Integer
                Dim j As Integer
                Dim oset As Double
                Dim tgap As Double
                Dim xscal As Double
                Dim aStep As Double
                Dim toset As Double
                Dim ext As Double
                iGUID = aImage.GUID
                If dxfPlane.IsNull(aPlane) Then bPlane = aImage.UCS Else bPlane = New dxfPlane(aPlane)
                If aDimType = dxxLinearDimTypes.LinearVertical Then dType = dxxDimTypes.LinearVertical
                If aPointSetsXY Is Nothing Then Throw New Exception("Undefined Point Sets Detected")
                dPoints = bPlane.CreateVectors(aPointSetsXY)
                If dPoints.Count < 2 Then Throw New Exception("Undefined Point Sets Detected")
                'layer color etc and dimstyle
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearA, aLayerName:="", aColor:=dxxColors.Undefined, aLineType:="", aStyleName:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False)
                End If
                dInput = New TDIMINPUT(aDimStyle, "", dType) With {.LayerName = aDisplaySettings.LayerName, .Color = aDisplaySettings.Color, .Linetype = aDisplaySettings.Linetype}
                xscal = 1 / aImage.obj_DISPLAY.ZoomFactor
                eStruc = dxfImageTool.DisplayStructure_DIM(aImage, False, dInput, rDStyle:=dSty, rTStyle:=tSty)
                dSty.PropValueSet(dxxDimStyleProperties.DIMSD1, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMSD2, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMSE1, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMSE2, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMATFIT, dxxDimTextFitTypes.MoveArrowsFirst)
                If dType = dxxDimTypes.LinearHorizontal Then
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTOH, Not aAlignText)
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTIH, Not aAlignText)
                Else
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTOH, True)
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTIH, True)
                End If
                dSty.PropValueSet(dxxDimStyleProperties.DIMTIX, True)
                aPrecis = dSty.PropValueD(dxxDimStyleProperties.DIMDEC)
                xscal = dSty.PropValueD(dxxDimStyleProperties.DIMSCALE)
                tgap = xscal * Math.Abs(dSty.PropValueD(dxxDimStyleProperties.DIMGAP))
                ext = xscal * dSty.PropValueD(dxxDimStyleProperties.DIMEXE)
                dVecs = New TVECTORS(dPoints)
                'create a rectangle around the points
                bRec = dVecs.Bounds(bPlane.Strukture)
                If dType = dxxDimTypes.LinearHorizontal Then
                    osetDir = bRec.YDirection
                    If aStackRightOrDown Then osetDir *= -1
                    oset = 0.5 * bRec.Height
                    aLn.SPT = bRec.Vector(-0.5 * bRec.Width)
                    aLn.EPT = bRec.Vector(0.5 * bRec.Width)
                Else
                    osetDir = bRec.XDirection
                    If aStackRightOrDown Then osetDir *= -1
                    oset = 0.5 * bRec.Width
                    aLn.SPT = bRec.Vector(0, 0, -0.5 * bRec.Height)
                    aLn.EPT = bRec.Vector(0, 0, 0.5 * bRec.Height)
                End If
                If aOffsetIsAbsolute Then
                    oset += Math.Abs(aDimOffset)
                Else
                    oset += Math.Abs(aDimOffset) * xscal
                End If
                aLn.Translate(osetDir * oset)
                'create the pairs
                cnt = Fix(dPoints.Count / 2)
                aPairs.Count = cnt
                ReDim aPairs.Members(0 To cnt - 1)
                dcnt = dPoints.Count
                For i = 1 To cnt
                    P1 = dPoints.Item(i)
                    P2 = dPoints.Item(dcnt)
                    dcnt -= 1
                    aPairs.Members(i - 1).Point1 = P1.Strukture
                    aPairs.Members(i - 1).Point2 = P2.Strukture
                    If String.Compare(P1.Tag, P2.Tag, True) = 0 Then aPairs.Members(i - 1).Tag1 = P1.Tag
                    If String.Compare(P1.Flag, P2.Flag, True) = 0 Then aPairs.Members(i - 1).Tag2 = P1.Flag
                    If P1.Value = P2.Value Then aPairs.Members(i - 1).Tag3 = P1.Value.ToString()
                    If dType = dxxDimTypes.LinearHorizontal Then aPairs.Members(i - 1).Span = Math.Abs(aPairs.Members(i - 1).Point1.X - aPairs.Members(i - 1).Point2.X) Else aPairs.Members(i - 1).Span = Math.Abs(aPairs.Members(i - 1).Point1.Y - aPairs.Members(i - 1).Point2.Y)
                    aPairs.Members(i - 1).Span = Math.Round(aPairs.Members(i - 1).Span, aPrecis)
                Next i
                toset = aTextOffset * xscal
                aDim = New dxeDimension(dSty, dType, eStruc, bPlane.Strukture) With {.TextOffset = toset}
                dcnt = 0
                Do Until aPairs.Count = 0
                    If aPairs.Count = 1 Then
                        aPair = aPairs.Members(0)
                        bPairs.Count = 0
                    Else
                        bPairs.Count = aPairs.Count - 1
                        ReDim bPairs.Members(bPairs.Count - 1)
                        idx = -1
                        d1 = 2.6E+26
                        For i = 0 To aPairs.Count - 1
                            aPair = aPairs.Members(i)
                            If aPair.Span < d1 Then
                                d1 = aPair.Span
                                idx = i
                            End If
                        Next i
                        j = 0
                        For i = 0 To aPairs.Count - 1
                            If idx <> i Then
                                bPairs.Members(j) = aPairs.Members(i)
                                j += 1
                            Else
                                aPair = aPairs.Members(i)
                            End If
                        Next i
                    End If
                    dcnt += 1
                    bDim = aDim.Clone
                    If dcnt = 1 Then
                        dpt = aPair.Point2.ProjectedTo(aLn)
                    Else
                        If dType = dxxDimTypes.LinearHorizontal Then
                            aStep = 0.5 * lDim.TextPrimary.Height + tgap + tstep
                        Else
                            aStep = 0.5 * lDim.TextPrimary.Height + tgap + tstep
                        End If
                        dpt += osetDir * (aStep + tgap)
                    End If
                    bDim.ImageGUID = iGUID
                    bDim.DimStyle.Prefix = aPair.Tag1
                    bDim.DimStyle.Suffix = aPair.Tag2
                    bDim.OverideText = aPair.Tag3
                    bDim.DefPt13V = aPair.Point1
                    bDim.DefPt14V = aPair.Point2
                    bDim.DefPt10V = dpt
                    bDim.TextOffset = toset
                    bDim.SetImage(aImage, False)
                    If dcnt = 1 Then
                        tstep = bDim.TextPrimary.TextHeight * 1.2 + Math.Abs(aAdditionalSpace) * xscal + ext
                    Else
                        If dType = dxxDimTypes.LinearVertical Then
                            If aStepText Then
                                toset += lDim.TextPrimary.Height + tgap
                                bDim.TextOffset = toset
                                bDim.IsDirty = True
                            End If
                            If Not aAlignText Then
                                dpt += osetDir * (0.5 * bDim.TextPrimary.Width + tgap)
                                bDim.DefPt10V = dpt
                                bDim.IsDirty = True
                            End If
                        End If
                    End If
                    If bCreateOnly Then
                        _rVal.Add(bDim)
                    Else
                        bDim = _rVal.Add(aImage.Entities.Add(bDim))
                    End If
                    lDim = bDim
                    If aCollector IsNot Nothing Then aCollector.Add(bDim)
                    aPairs = bPairs
                    If aPairs.Count <= 0 Then Exit Do
                Loop
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
Err:
        End Function
        Public Function Stack_Ordinate(aDimType As dxxOrdinateDimTypes, aBasePointXY As iVector, aDimPointsXY As IEnumerable(Of iVector), aDimOffset As Double,
                                       Optional aStackShift As Double = 0.0, Optional aInvertStack As Boolean = False, Optional aSpaceAdder As Double? = Nothing,
                                       Optional bCompressStack As Boolean = False, Optional aTagsToSkip As String = Nothing, Optional aTagsAsOverrideText As Boolean = False,
                                       Optional bAlternateOffset As Boolean = False, Optional aOrdListToSkip As String = "",
                                       Optional aDimStyle As String = "",
                                       Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                       Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing,
                                       Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False,
                                       Optional rDimOrdinates As List(Of Double) = Nothing, Optional aOrdinatesToSkip As List(Of Double) = Nothing) As List(Of dxeDimension)
            Dim _rVal As New List(Of dxeDimension)
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If aDimPointsXY Is Nothing Then Return _rVal
            If aDimPointsXY.Count <= 0 Then Return _rVal
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the type of Dimension being requested (must be OrdinateHorizontal or OrdinateVertical)
            '#2the base point for the dimension stack
            '#3the collection of points to dimension from the base point
            '#4the x or y coordinate to place the stack dimensions
            '#5optional X or Y distance to offset the text stack
            '#6flag to reverse the point order
            '#7a distance to add to the minimum distance between the text of the stack
            '#8flag to place the dim text of each dimensions in a compressed stack
            '#9a comma delimited lis of Tags that will prevent a point from being dimensioned
            '#11 a flag to alternate between the passed offset and a computed longer offset which stacks the text in a zig-zag fashion allowing for closer stacking of the dimensions
            '#10flag to use the tags of the passed points as the override text for the dimensions
            '#12a list of ordinates to skip
            '#13returns a list of ordinates that were dimensions
            '#14a dim style to use other than the current dim style
            '#15a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#16a color to apply to the entity instead of the current color
            '#17a line type to apply to the entity instead of the current line type setting
            '#18if passed the dimensions are created on this plane otherwise the current UCS XY plane is used
            '#19flag to create the dimensions but not to add them to the image
            '^used to create a stack of Horizontal or Vertical ordinate dimensions
            '~returns a collection of the created dimension.
            '~an example of the best use for it would be dimensioning the many points on a line where the points are.
            '~this function assures that all the ordinate dims pulled from these points are stacked neatly and evenly spaced.
            '~points with a flag property = "NODIM" do not get dimensioned.
            '~points with a flag property = "V" alway get a vertical dimension.
            '~points with a flag property = "H" alway get a horizontal dimension.
            '~points with a flag property = "HV" alway get a vertical and horizontal dimension.
            '~points with a flag property = "NOV" never get a vertical dimension.
            '~points with a flag property = "NOH" never get a horizontal dimension.
            Try
                Dim aDim As dxeDimension
                Dim lDim As dxeDimension
                Dim cDim As dxeDimension
                Dim dType As dxxDimTypes = dxxDimTypes.OrdHorizontal
                Dim ctxt As dxeText = Nothing
                Dim ltxt As dxeText = Nothing
                Dim bndRec As TPLANE
                Dim aVrts As New TVERTICES(0)
                Dim cVrts As TVERTICES
                Dim dStyle As dxoDimStyle = Nothing
                Dim tStyle As dxoStyle = Nothing
                Dim aPln As TPLANE
                Dim v0 As TVECTOR
                Dim v1 As TVECTOR
                Dim eStruc As TDISPLAYVARS
                Dim limLn As TLINE
                Dim stackLn As TLINE
                Dim stepDir As TVECTOR
                Dim osetDir As TVECTOR
                Dim dInput As TDIMINPUT
                Dim v13 As TVERTEX
                Dim v14 As TVERTEX
                Dim v14Last As TVERTEX
                Dim cRec As TPLANE
                Dim lRec As TPLANE
                Dim sSkipValues As New List(Of Double)
                Dim tgap As Double
                Dim i As Integer
                Dim j As Integer
                Dim k As Integer
                Dim iGUID As String
                Dim aPrecis As Integer
                Dim minspan As Double
                Dim ord As Double
                Dim tht As Double
                Dim linscale As Double
                Dim d1 As Double
                Dim d2 As Double
                Dim d3 As Double
                Dim bMoveIt As Boolean
                Dim bBoxIt As Boolean
                Dim bDimIt As Boolean
                Dim f1 As Double
                Dim f2 As Double
                Dim oset As Double
                Dim FS As Double
                Dim tpl As Tuple(Of Double, Integer, TVERTEX)
                Dim srt As New List(Of Tuple(Of Double, Integer, TVERTEX))
                Dim dimverts As New List(Of Tuple(Of Double, Integer, TVERTEX))
                Dim sOrds As New List(Of Double)
                Dim sForced As New List(Of Double)
                Dim skiptags As List(Of String) = TLISTS.ToStringList(aTagsToSkip, bNoDupes:=True, bUCase:=True)
                Dim lEdge As dxxRectangleLines
                Dim cPt As dxxRectanglePts
                Dim aDir As TVECTOR
                Dim tDir As TVECTOR
                Dim altoset As Double
                Dim alt As Integer
                Dim forceflags As List(Of String)
                'initiaize
                If aDimType = dxxOrdinateDimTypes.OrdVertical Then
                    dType = dxxDimTypes.OrdVertical
                    forceflags = New List(Of String)({"NODIM", "HV", "V", "NOV"})
                Else
                    forceflags = New List(Of String)({"NODIM", "HV", "H", "NOH"})
                End If
                If dxfPlane.IsNull(aPlane) Then aPln = New TPLANE(aImage.obj_UCS) Else aPln = New TPLANE(aPlane)
                'create the base point
                v0 = aPln.Origin
                If aBasePointXY IsNot Nothing Then v0 = New TVECTOR(aBasePointXY)
                'move the plane to the basepoint
                aPln.Origin = v0
                iGUID = aImage.GUID
                'get the dimension style
                'layer color etc
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(aEntityType:=dxxEntityTypes.DimOrdinateH, "", dxxColors.Undefined, "", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False, aStyleName:=aDimStyle)
                End If
                dInput = New TDIMINPUT(aDimStyle, "", dType) With {.LayerName = aDisplaySettings.LayerName, .Color = aDisplaySettings.Color, .Linetype = aDisplaySettings.Linetype}
                eStruc = dxfImageTool.DisplayStructure_DIM(aImage, False, dInput, rDStyle:=dStyle, rTStyle:=tStyle)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSD1, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSD2, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSE1, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSE2, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMTAD, dxxDimTadSettings.Centered)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMJUST, dxxDimJustSettings.Centered)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMATFIT, dxxDimTextFitTypes.BestFit)
                aPrecis = TVALUES.ToInteger(dStyle.PropValueI(dxxDimStyleProperties.DIMDEC), True, aMinVal:=0, aMaxVal:=15)
                FS = dStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
                tht = dStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * FS
                linscale = dStyle.PropValueD(dxxDimStyleProperties.DIMLFAC)
                tgap = dStyle.PropValueD(dxxDimStyleProperties.DIMGAP)
                bBoxIt = tgap < 0
                If tgap = 0 Then tgap = 0.09
                tgap = FS * tgap
                tgap = Math.Abs(tgap)
                If bBoxIt Then
                    minspan = 1.5 * tgap
                Else
                    minspan = 1 * tgap
                End If
                If aSpaceAdder IsNot Nothing Then
                    If aSpaceAdder.HasValue Then minspan += Math.Abs(aSpaceAdder.Value)
                End If
                oset = Math.Round(aDimOffset, 6)
                If oset = 0 Then oset = 0.1 * aImage.obj_DISPLAY.PaperScale
                'get the list of ordinates to NOT put dimensions on
                If aOrdListToSkip <> "" Then
                    sSkipValues = TLISTS.ToNumericList(aOrdListToSkip, bNoDupes:=True, aPrecis:=aPrecis)
                End If
                'create the dimension points and get the ordinates to dimension
                cVrts = New TVERTICES(0)
                For i = 1 To aDimPointsXY.Count
                    v13 = New TVERTEX(aDimPointsXY(i - 1))
                    v13.Vector = v13.Vector.ProjectedTo(aPln)
                    v13.Flag = v13.Flag.Trim.ToUpper
                    v1 = v13.Vector.WithRespectTo(aPln, aPrecis:=aPrecis + 1)
                    If dType = dxxDimTypes.OrdHorizontal Then
                        ord = Math.Round(v1.X * linscale, aPrecis)
                    Else
                        ord = Math.Round(v1.Y * linscale, aPrecis)
                    End If
                    bDimIt = True
                    If skiptags.Count > 0 Then
                        bDimIt = skiptags.IndexOf(v13.Tag) < 0
                    End If
                    v13.Inverted = False
                    If v13.Flag <> "" And bDimIt And forceflags.IndexOf(v13.Flag) > -1 Then
                        If v13.Flag = "NODIM" Or v13.Flag = "NOV" Or v13.Flag = "NOH" Then
                            bDimIt = False
                        Else
                            v13.Inverted = True
                        End If
                    End If
                    If Not v13.Inverted And bDimIt Then
                        bDimIt = sSkipValues.IndexOf(ord) < 0
                        If bDimIt And aOrdinatesToSkip IsNot Nothing Then
                            If aOrdinatesToSkip.IndexOf(Math.Round(ord / linscale, aPrecis)) > -1 Then
                                bDimIt = False
                            End If
                        End If
                    End If
                    v13.Value = ord
                    If bDimIt Then
                        cVrts.Add(v13)
                        sOrds.Add(ord)
                        srt.Add(New Tuple(Of Double, Integer, TVERTEX)(ord, i, v13))
                        If v13.Inverted Then sForced.Add(ord)
                    End If
                    aVrts.Add(v13)
                Next i
                If srt.Count <= 0 Then Return _rVal
                'set the sort line and the dim placement line
                bndRec = aVrts.Bounds(aPln)
                If Not aInvertStack Then f1 = 1 Else f1 = -1
                If oset > 0 Then f2 = 1 Else f2 = -1
                oset = Math.Abs(oset)
                If dType = dxxDimTypes.OrdVertical Then
                    stepDir = aPln.YDirection
                    osetDir = aPln.XDirection
                    d2 = 0.5 * aPln.Width
                    d3 = 0.5 * aPln.Height
                    v1 = bndRec.Origin + osetDir * ((0.5 * bndRec.Width + oset) * f2)
                    stackLn = bndRec.LineV(v1, 10, bByStartPt:=True)
                Else
                    stepDir = aPln.XDirection
                    osetDir = aPln.YDirection
                    v1 = bndRec.Origin + osetDir * ((0.5 * bndRec.Height + oset) * f2)
                    stackLn = bndRec.LineH(v1, 10, bByStartPt:=True)
                End If
                'aImage.Entities.AddRectangleV bndRec
                'aImage.Entities.AddLine stackLn.SPT.X, stackLn.SPT.Y, stackLn.EPT.X, stackLn.EPT.Y
                '
                'create the template dimension with the common properties
                aDim = New dxeDimension(dStyle, dType, eStruc, aPln) With {.DefPt10V = aPln.Origin}
                'get the points closest to the stack line
                srt.Sort(Function(tupl1 As Tuple(Of Double, Integer, TVERTEX), tupl2 As Tuple(Of Double, Integer, TVERTEX)) tupl1.Item1.CompareTo(tupl2.Item1))
                If aInvertStack Then srt.Reverse()
                If bAlternateOffset Then
                    alt = -1
                    d2 = sOrds.Max
                    d2 = TVALUES.To_DBL($"{Math.Truncate(d2) }. { New String("9", aPrecis)}")
                    altoset = aImage.CreateText(tStyle, dStyle.FormatNumber(d2, True), dxxTextTypes.Multiline, dxxTextDrawingDirections.Horizontal, tht, dxxMTextAlignments.MiddleCenter).Length + (2 * tgap)
                End If
                For Each tpl In srt
                    i = tpl.Item2
                    ord = tpl.Item1
                    bDimIt = dimverts.FindIndex(Function(tupl1 As Tuple(Of Double, Integer, TVERTEX)) tupl1.Item1 = ord) < 0
                    If bDimIt Then
                        aVrts = cVrts.GetByValue(ord)
                        If aVrts.Count = 1 Then
                            v13 = aVrts.Item(1)
                            dimverts.Add(New Tuple(Of Double, Integer, TVERTEX)(ord, i, v13))
                        Else
                            v13 = aVrts.NearestToLine(stackLn, False, k)
                            dimverts.Add(New Tuple(Of Double, Integer, TVERTEX)(ord, i, v13))
                            If sForced.IndexOf(ord) >= 0 Then
                                For j = 1 To aVrts.Count
                                    If j <> k Then
                                        If aVrts.Inverted(j) Then
                                            v13 = aVrts.Item(j)
                                            dimverts.Add(New Tuple(Of Double, Integer, TVERTEX)(ord, i, v13))
                                            'inverted means forced
                                        End If
                                    End If
                                Next j
                            End If
                        End If
                    End If
                Next
                lDim = Nothing
                If dType = dxxDimTypes.OrdHorizontal Then
                    If Not aInvertStack Then lEdge = dxxRectangleLines.BottomEdge Else lEdge = dxxRectangleLines.TopEdge
                Else
                    If Not aInvertStack Then lEdge = dxxRectangleLines.TopEdge Else lEdge = dxxRectangleLines.BottomEdge
                End If
                If lEdge = dxxRectangleLines.BottomEdge Then cPt = dxxRectanglePts.TopCenter Else cPt = dxxRectanglePts.BottomCenter
                tDir = New TVECTOR(stepDir.X * -f1, stepDir.Y * -f1, stepDir.Z * -f1)
                dimverts.Sort(Function(tupl1 As Tuple(Of Double, Integer, TVERTEX), tupl2 As Tuple(Of Double, Integer, TVERTEX)) tupl1.Item1.CompareTo(tupl2.Item1))
                If aInvertStack Then dimverts.Reverse()
                For Each tpl In dimverts
                    alt *= -1
                    v13 = tpl.Item3
                    ord = tpl.Item1
                    v14 = v13.ProjectedTo(stackLn)
                    If rDimOrdinates IsNot Nothing Then
                        rDimOrdinates.Add(Math.Round(ord / linscale, aPrecis))
                    End If
                    If bAlternateOffset And alt = 1 Then
                        v14 += osetDir * altoset
                    End If
                    'apply the shift
                    If aStackShift <> 0 Then
                        If (bCompressStack And i = 1) Or Not bCompressStack Then
                            v14 += stepDir * aStackShift
                        End If
                    End If

                    cDim = aDim.Clone
                    v1 = New TVECTOR(tpl.Item3)
                    cDim.DefPt10V = v0
                    cDim.DefPt13V = v1
                    cDim.DefPt14V = v14.Vector
                    If aTagsAsOverrideText Then cDim.OverideText = v13.Tag
                    cDim.ImageGUID = iGUID
                    cDim.SetImage(aImage, False)
                    cDim.UpdatePath(True, aImage)
                    If lDim IsNot Nothing Then
                        bMoveIt = False
                        ctxt = cDim.TextPrimary
                        ltxt = lDim.TextPrimary
                        If bBoxIt Then
                            cRec = ctxt.Bounds.Stretched(2 * tgap)
                            lRec = ltxt.Bounds.Stretched(2 * tgap)
                        Else
                            cRec = ctxt.Bounds
                            lRec = ltxt.Bounds
                        End If
                        'collision test
                        limLn = lRec.Edge(lEdge, aLengthAdder:=2)
                        'aImage.Entities.Add(New dxfRectangle(cRec).Perimeter)
                        v1 = cRec.Point(cPt)
                        d2 = 0
                        v1 = dxfProjections.ToLine(v1, limLn, rOrthoDirection:=aDir, rDistance:=d1)
                        'aImage.Entities.Add(New dxeLine(limLn))
                        If Not aDir.Equals(tDir) Then
                            'the dim is below the last so move it above including the default gap
                            d2 = (d1 + minspan) * f1
                        Else
                            'the dim is above
                            If d1 < minspan Then
                                'enforce the default gap
                                d2 = (minspan - d1) * f1
                            Else
                                If bCompressStack Then
                                    'enforce the default gap
                                    d2 = (d1 - minspan) * -f1
                                End If
                            End If
                        End If
                        If d2 <> 0 Then
                            v14.Vector += stepDir * d2
                            cDim.DefPt14V = v14.Vector
                            cDim.IsDirty = True
                        End If
                    End If
                    If Not bCreateOnly Then
                        cDim = aImage.Entities.Add(cDim)
                    End If
                    _rVal.Add(cDim)
                    If aCollector IsNot Nothing Then aCollector.Add(cDim)
                    v14Last = v14
                    If Not bAlternateOffset Then
                        lDim = cDim
                    Else
                        lDim = _rVal.Item(_rVal.Count - 1)
                    End If
                Next
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
            End Try
            Return _rVal
        End Function
        Public Function DimensionVertices(aDimensionOrigin As Boolean, aBaseVertexXY As iVector, aVerticesXY As IEnumerable(Of iVector), aCenterXY As dxfVector,
                                          aDimOffset As Double, Optional aInvertStacks As Boolean = False, Optional aSpaceAdder As Double = 0.0,
                                          Optional aYsToOmmit As List(Of Double) = Nothing, Optional aXsToOmmit As List(Of Double) = Nothing,
                                          Optional aTagToIgnore As String = Nothing, Optional bTagsAsSuffixes As Boolean = False,
                                          Optional bDontDimBase As Boolean = False,
                                          Optional aVShift As Double = 0.0, Optional aHShift As Double = 0.0,
                                          Optional aPlane As dxfPlane = Nothing, Optional aDimStyle As String = "",
                                           Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                          Optional bCreateOnly As Boolean = False,
                                          Optional aImage As dxfImage = Nothing) As List(Of dxeDimension)
            Dim _rVal As New List(Of dxeDimension)
            If Not GetImage(aImage) Then Return _rVal
            '#1flag to add the plane origin to the dimension points
            '#2the base vertex (0,0) for the returned dimension. nothing means use the plane origin
            '#3the collection of vertices to dimension
            '#4the center to use for division of dimension points into quadrants
            '#5the offset distance for the placement of the dimensions
            '#6flag to reverses how the dimensions get stacked
            '#7a distance to add to the minimum distance between the text of the stack
            '#8a Y ordinate to skip completely (absolute relative to base point)
            '#9a X ordinate to skip completely (absolute relative to base point)
            '#10a a group of Y ordinates to skip completely
            '#11a group of X ordinate to skip completely
            '#12a point tag to ignore completely
            '#13a flag to use the passed points tags as the suffix to assign the dimensions
            '#14a flag to prevent the base point from being dimensioned
            '#15a distance to shift the vertical dimensions
            '#16a distance to shift the horizontal dimensions
            '#17if passed the dimensions are created on this plane otherwise the current UCS XY plane is used
            '#18a dim style to use other than the current dim style
            '#19a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#20a color to apply to the entity instead of the current color
            '#21a line type to apply to the entity instead of the current line type setting
            '#22flag to create the dimensions but not to add them to the image
            '^used to dimension a set of points with ordinate dimensions
            '~this routine attempts to dimension all of the X and Y ordinates of the points in the passed collection.
            '~It try's to avoid overlap of dimensions and repeating dimensions where several vertices share the same ordinate.
            '~points with a flag property = "NODIM" do not get dimensioned.
            '~points with a flag property = "V" alway get a vertical dimension.
            '~points with a flag property = "H" alway get a horizontal dimension.
            '~points with a flag property = "HV" alway get a vertical and horizontal dimension.
            '~points with a flag property = "NOV" never get a vertical dimension.
            '~points with a flag property = "NOH" never get a horizontal dimension.
            '~points with a flag property = "VONLY"  alway get a vertical and never get a horizontal dimension.
            '~points with a flag property = "HONLY"  alway get a horizontal and never get a vertical dimension.
            'intitialize
            Dim hDim As dxeDimension
            Dim vDim As dxeDimension
            Dim lDim As dxeDimension
            Dim bndRec As TPLANE
            Dim quadRec As TPLANE
            Dim aVecs As TVECTORS
            Dim aVrts As TVERTICES
            Dim bVrts As TVERTICES
            Dim txtLn As TLINE
            Dim sortLn As TLINE
            Dim v14 As TVERTEX
            Dim v13 As TVERTEX
            Dim v14Last As TVERTEX
            Dim srcVrt As TVERTEX
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim stepDir As TVECTOR
            Dim aDir As TVECTOR
            Dim aPl As TPLANE
            Dim limLine As TLINE
            Dim arQuadsV As New TVERTEXTARRAY(4)
            Dim arQuadsH As New TVERTEXTARRAY(4)
            Dim quadVts As TVERTICES
            Dim vrt As TVERTEX
            Dim vvrt As TVERTEX
            Dim hvrt As TVERTEX
            Dim dStyle As dxoDimStyle = Nothing
            Dim tStyle As dxoStyle = Nothing
            Dim txtLns(0 To 3) As TLINE
            Dim vqds(0 To 3) As Byte
            Dim hqds(0 To 3) As Byte
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim ivDim As Integer
            Dim aPrecis As Integer
            Dim hqd As Integer
            Dim vqd As Integer
            Dim basequad As Integer
            Dim iGUID As String
            Dim sSkipTag As String = String.Empty
            Dim hVals As New List(Of Double)
            Dim vVals As New List(Of Double)
            Dim tht As Double
            Dim tgap As Double
            Dim minspan As Double
            Dim cnt As Integer
            Dim d1 As Double
            Dim d2 As Double
            Dim bDimIt As Boolean
            Dim bMoveIt As Boolean
            Dim bTestTag As Boolean
            Try
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimOrdinateH, aLayerName:="", aColor:=dxxColors.Undefined, aLineType:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False, aStyleName:=aDimStyle)
                End If
                If dxfPlane.IsNull(aPlane) Then aPl = aImage.obj_UCS Else aPl = New TPLANE(aPlane)
                iGUID = aImage.GUID
                If aBaseVertexXY IsNot Nothing Then
                    aPl.Origin = New TVECTOR(aBaseVertexXY)
                End If
                'create the dimension points relative to the plane origin

                aVrts = dxfVectors.GetTVERTICES(aVerticesXY)
                If aVrts.Count <= 0 Then Throw New Exception("Undefined Dimension Points Detected")
                'style layer color etc
                Dim dInput As New TDIMINPUT(aDimStyle, "") With {.LayerName = aDisplaySettings.LayerName, .Color = aDisplaySettings.Color, .Linetype = aDisplaySettings.Linetype}
                'get the diplay settings and the styles
                Dim eStruc As TDISPLAYVARS = dxfImageTool.DisplayStructure_DIM(aImage, False, dInput, rDStyle:=dStyle, rTStyle:=tStyle)
                'set the dimstyle properties
                Dim linscale As Double = dStyle.PropValueD(dxxDimStyleProperties.DIMLFAC)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMPREFIX, "")
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSUFFIX, "")
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSD1, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSD2, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSE1, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSE2, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMATFIT, dxxDimTextFitTypes.BestFit)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMTAD, dxxDimTadSettings.Centered)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMJUST, dxxDimJustSettings.Centered)
                aPrecis = dStyle.PropValueI(dxxDimStyleProperties.DIMDEC)
                d1 = dStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
                tht = dStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * d1
                tgap = dStyle.PropValueD(dxxDimStyleProperties.DIMGAP)
                tgap = d1 * tgap
                If tgap = 0 Then tgap = 0.09 * d1
                d2 = Math.Abs(tgap) + Math.Abs(aSpaceAdder)
                If tgap < 0 Then
                    minspan = d2 + tht + Math.Abs(tgap)
                Else
                    minspan = d2 + tht
                End If
                'eliminate points not to be dimension
                bTestTag = Not String.IsNullOrWhiteSpace(aTagToIgnore)
                If bTestTag Then sSkipTag = aTagToIgnore.ToUpper
                'create the bounding rectangle and set the centroid for point division
                bndRec = aVrts.Bounds(aPl)
                aVecs = aVrts.ProjectedTo(aPl, bReturnWithRespectTo:=True)
                quadRec = bndRec.Clone
                Dim vYsToSkip As New List(Of Double)
                If aYsToOmmit IsNot Nothing Then
                    Dim pVals As List(Of Double) = From d In aYsToOmmit Select d Distinct
                    For Each ord As Double In pVals
                        vYsToSkip.Add(Math.Round(ord, aPrecis))
                    Next
                Else
                End If
                Dim vXsToSkip As New List(Of Double)
                If aXsToOmmit IsNot Nothing Then
                    Dim pVals As List(Of Double) = From d In aXsToOmmit Select d Distinct
                    For Each ord As Double In pVals
                        vXsToSkip.Add(Math.Round(ord, aPrecis))
                    Next
                End If
                If bDontDimBase Then
                    If vYsToSkip.IndexOf(0) < 0 Then vYsToSkip.Add(0)
                    If vXsToSkip.IndexOf(0) < 0 Then vXsToSkip.Add(0)
                End If
                If aDimensionOrigin Then
                    'v1 = aPl.Origin.WithRespectTo(bndRec)

                    aVrts.RemoveAtCoordinate(aPl.X, aPl.Y, aPl.Z, aPrecis + 1)
                    aVrts.Add(New TVERTEX(aPl.X, aPl.Y, aPl.Z, aFlag:="HV"))
                End If
                If aCenterXY IsNot Nothing Then
                    quadRec.Origin = aCenterXY.Strukture.ProjectedTo(aPl)
                End If
                aDimOffset = Math.Round(aDimOffset, 6)
                If aDimOffset = 0 Then aDimOffset = 0.1 * aImage.obj_DISPLAY.PaperScale
                d1 = 0.5 * bndRec.Width + Math.Abs(aDimOffset)
                txtLns(0) = aPl.LineV(bndRec.Origin + aPl.XDirection * -d1, 10, bByStartPt:=True)
                txtLns(1) = aPl.LineV(bndRec.Origin + aPl.XDirection * d1, 10, bByStartPt:=True)
                d1 = 0.5 * bndRec.Height + Math.Abs(aDimOffset)
                txtLns(2) = aPl.LineH(bndRec.Origin + aPl.YDirection * -d1, 10, bByStartPt:=True)
                txtLns(3) = aPl.LineH(bndRec.Origin + aPl.YDirection * d1, 10, bByStartPt:=True)
                hDim = New dxeDimension(dStyle, dxxDimTypes.OrdHorizontal, eStruc, aPl) With {.DefPt10V = aPl.Origin}
                hDim.SetImage(aImage, False)
                vDim = New dxeDimension(dStyle, dxxDimTypes.OrdVertical, eStruc, aPl) With {.DefPt10V = aPl.Origin}
                vDim.SetImage(aImage, False)

                'set the order of the stacking
                '+++++++++++++++++++++++++++++++++++
                ''SetQuatrants:
                'get the points defined with respect to the boundary center and determine quadrant
                v2 = aPl.Origin.WithRespectTo(quadRec)
                'determine the order
                If v2.X <= 0 Then
                    basequad = 2
                    If v2.Y <= 0 Then basequad = 3
                Else
                    basequad = 1
                    If v2.Y <= 0 Then basequad = 4
                End If
                Select Case basequad
                    Case 1
                        vqds(0) = 1
                        vqds(1) = 3
                        vqds(2) = 2
                        vqds(3) = 4
                        hqds(0) = 2
                        hqds(1) = 4
                        hqds(2) = 3
                        hqds(3) = 1
                    Case 2
                        vqds(0) = 2
                        vqds(1) = 4
                        vqds(2) = 3
                        vqds(3) = 1
                        hqds(0) = 3
                        hqds(1) = 1
                        hqds(2) = 4
                        hqds(3) = 2
                    Case 3
                        vqds(0) = 3
                        vqds(1) = 4
                        vqds(2) = 1
                        vqds(3) = 2
                        hqds(0) = 4
                        hqds(1) = 1
                        hqds(2) = 2
                        hqds(3) = 3
                    Case 4
                        vqds(0) = 4
                        vqds(1) = 2
                        vqds(2) = 3
                        vqds(3) = 1
                        hqds(0) = 1
                        hqds(1) = 3
                        hqds(2) = 2
                        hqds(3) = 4
                End Select
                '+++++++++++++++++++++++++++++++++++
                'define the dim points quadrants based on the bounding rectangle center
                For i = 1 To aVrts.Count
                    aVrts.SetFlag(aVrts.Flag(i).Trim.ToUpper, i, False)

                    vrt = aVrts.Item(i)
                    bDimIt = vrt.Flag <> "NODIM"
                    If bDimIt And bTestTag And String.Compare(vrt.Tag, sSkipTag, ignoreCase:=True) = 0 Then bDimIt = False
                    If bDimIt Then
                        v1 = aVrts.Vector(i)
                        v2 = v1.WithRespectTo(quadRec, aPrecis:=aPrecis)
                        v3 = v1.WithRespectTo(aPl, aPrecis:=aPrecis)
                        If v2.X <= 0 Then
                            If v2.Y <= 0 Then vqd = 3 Else vqd = 2
                        Else
                            If v2.Y >= 0 Then vqd = 1 Else vqd = 4
                        End If
                        If v2.X < 0 Then
                            If v2.Y > 0 Then hqd = 2 Else hqd = 3
                        Else
                            If v2.Y < 0 Then hqd = 4 Else hqd = 1
                        End If
                        vvrt = New TVERTEX(vrt)
                        vvrt.Value = Math.Round(v3.Y * linscale, aPrecis) 'this will be the dimension value
                        hvrt = New TVERTEX(vrt)
                        hvrt.Value = Math.Round(v3.X * linscale, aPrecis)  'this will be the dimension value
                        'suppress by flag value
                        vvrt.Suppressed = (vrt.Flag = "NOV") Or (vrt.Flag = "HONLY") Or (vrt.Flag = "NODIM")
                        hvrt.Suppressed = (vrt.Flag = "NOH") Or (vrt.Flag = "VONLY") Or (vrt.Flag = "NODIM")
                        'mark to force a dimension
                        vvrt.Mark = vvrt.Flag = "V" Or vvrt.Flag = "HV" Or vvrt.Flag = "VONLY" 'force V dim
                        hvrt.Mark = hvrt.Flag = "H" Or hvrt.Flag = "HV" Or hvrt.Flag = "HONLY" 'force H dim
                        'suppress any vector in the ordinate skip list
                        If vYsToSkip.IndexOf(v3.Y) >= 0 Then vvrt.Suppressed = True
                        If vXsToSkip.IndexOf(v3.X) >= 0 Then vvrt.Suppressed = True
                        If Not vvrt.Suppressed Then
                            vvrt.Vector = v1.Clone
                            vvrt.Col = i
                            'save the vertex to quadrant vectors
                            arQuadsV.Add(vqd, vvrt)
                            If v2.X = 0 Then
                                'if its on the centerline also add it to the opposite quadrant
                                If vqd = 1 Or vqd = 2 Then
                                    If vqd = 1 Then arQuadsV.Add(2, vvrt) Else arQuadsV.Add(1, vvrt)
                                Else
                                    If vqd = 3 Then arQuadsV.Add(4, vvrt) Else arQuadsV.Add(3, vvrt)
                                End If
                            End If
                        End If
                        If Not hvrt.Suppressed Then
                            hvrt.Vector = v1.Clone
                            hvrt.Col = i
                            'save the vertex to quadrant vectors
                            arQuadsH.Add(hqd, hvrt)
                            If v2.Y = 0 Then
                                'if its on the centerline also add it to the opposite quadrant
                                If hqd = 1 Or hqd = 4 Then
                                    If hqd = 1 Then arQuadsH.Add(4, hvrt) Else arQuadsH.Add(1, hvrt)
                                Else
                                    If hqd = 2 Then arQuadsH.Add(3, hvrt) Else arQuadsH.Add(2, hvrt)
                                End If
                            End If
                        End If
                    End If
                Next i
                'sort the vectors
                sortLn = aPl.LineH(quadRec.Origin, 10, bByStartPt:=True)
                For i = 1 To 4
                    quadVts = arQuadsV.Item(i)
                    quadVts.SortByDistanceToLine(sortLn, aInvertStacks)
                    arQuadsV.SetItem(i, quadVts)
                    For j = 1 To quadVts.Count
                        If quadVts.Mark(j) Then
                            For loopi As Integer = 1 To 4
                                bVrts = arQuadsV.Item(loopi)
                                For k = 1 To bVrts.Count
                                    If Not bVrts.Mark(k) Then
                                        If bVrts.Value(k) = quadVts.Value(j) Then
                                            bVrts.SetSuppressed(True, k)
                                        End If
                                    End If
                                Next k
                                arQuadsV.SetItem(loopi, bVrts)
                            Next loopi
                        End If
                    Next j
                Next i
                sortLn = aPl.LineV(quadRec.Origin, 10, bByStartPt:=True)
                For i = 1 To 4
                    quadVts = arQuadsH.Item(i)
                    quadVts.SortByDistanceToLine(sortLn, aInvertStacks)
                    arQuadsH.SetItem(i, quadVts)
                    For j = 1 To quadVts.Count
                        If quadVts.Mark(j) Then
                            For loopi = 1 To 4
                                bVrts = arQuadsH.Item(loopi)
                                For k = 1 To bVrts.Count
                                    If Not bVrts.Mark(k) Then
                                        If bVrts.Value(k) = quadVts.Value(j) Then
                                            bVrts.SetSuppressed(True, k)
                                        End If
                                    End If
                                Next k
                                arQuadsH.SetItem(loopi, bVrts)
                            Next loopi
                        End If
                    Next j
                Next i
                'create and save the dimensions
                '===========================================================
                'first the verticals
                '===========================================================
                For q As Integer = 0 To 3
                    vqd = vqds(q)
                    stepDir = aPl.YDirection
                    If vqd = 3 Or vqd = 4 Then stepDir *= -1
                    If aInvertStacks Then stepDir *= -1
                    lDim = Nothing
                    If vqd = 2 Or vqd = 3 Then txtLn = txtLns(0) Else txtLn = txtLns(1)
                    quadVts = arQuadsV.Item(vqd)
                    cnt = 0
                    'loop on the points in the quadrant
                    For i = 1 To quadVts.Count
                        v13 = quadVts.Item(i)
                        bDimIt = Not v13.Suppressed
                        d1 = v13.Value
                        Dim haveit As Boolean = vVals.IndexOf(d1) >= 0

                        'don't dim it if we did it before unless it was marked to get a dimension
                        If Not v13.Mark And bDimIt Then
                            If haveit Then bDimIt = False
                        End If
                        If Not bDimIt Then Continue For

                        cnt += 1
                        'passed the test so make a dimension for the point
                        'use the extreme if there are multiples at the same ordinate
                        bVrts = quadVts.GetByValue(v13.Value)
                        If bVrts.Count > 1 Then
                            v13 = bVrts.NearestToLine(txtLn)
                        End If

                        vVals.Add(d1)
                        'Console.WriteLine($"v{vVals.Count} = {d1}")

                        v14 = v13.ProjectedTo(txtLn)
                        If aVShift <> 0 Then
                            Dim f1 As Integer = 1
                            If aInvertStacks Then f1 = -1
                            v14 += stepDir * (Math.Abs(aVShift) * f1)
                        End If
                        If cnt > 1 Then
                            limLine = aPl.LineH(v14Last.Vector, 10, bByStartPt:=True)
                            limLine.Project(stepDir, minspan)
                            v2 = dxfProjections.ToLine(v14.Vector, limLine, rOrthoDirection:=aDir, rDistance:=d1)
                            bMoveIt = False
                            If d1 > 0.00001 Then
                                bMoveIt = aDir.Equals(stepDir, 4)
                            End If
                            If bMoveIt Then v14.Vector = limLine.SPT
                        End If
                        srcVrt = aVrts.Item(v13.Col)
                        vDim.DefPt13V = srcVrt.Vector
                        vDim.DefPt14V = v14.Vector
                        vDim.TFVSet(srcVrt.Tag, srcVrt.Flag, srcVrt.Value)
                        lDim = vDim.Clone
                        lDim.Tag = srcVrt.Tag
                        lDim.Flag = srcVrt.Flag
                        lDim.ImageGUID = iGUID
                        lDim.SetImage(aImage, False)
                        If bTagsAsSuffixes Then
                            Dim suf As String = lDim.Tag
                            If Not String.IsNullOrWhiteSpace(suf) Then
                                If Not suf.StartsWith(" ") Then suf = " " & suf
                                lDim.DimStyle.Suffix = suf
                            End If
                        End If
                        If Not bCreateOnly Then lDim = aImage.Entities.Add(lDim) Else lDim.SetImage(aImage, False)

                        _rVal.Add(lDim)
                        v14Last = v14
                    Next i
                Next q
                '===========================================================
                'now the horizontals
                '===========================================================
                'dim the orgin in the base quadrant if the is a point there
                ivDim = -1
                If Not bDontDimBase Then
                    hqd = basequad
                    bVrts = arQuadsH.Item(basequad)
                    If hqd = 3 Or hqd = 4 Then txtLn = txtLns(2) Else txtLn = txtLns(3)
                    d1 = 2.6E+26
                    For i = 0 To bVrts.Count - 1
                        v13 = bVrts.Item(i + 1)
                        If v13.Value = 0 Then
                            v1 = dxfProjections.ToLine(v13.Vector, txtLn, d2)
                            If d2 < d1 Then
                                d1 = d2
                                ivDim = i
                            End If
                        End If
                    Next i
                End If
                For q = 0 To 3
                    hqd = hqds(q)
                    stepDir = aPl.XDirection
                    If hqd = 2 Or hqd = 3 Then stepDir *= -1
                    If aInvertStacks Then stepDir *= -1
                    lDim = Nothing
                    If hqd = 3 Or hqd = 4 Then txtLn = txtLns(2) Else txtLn = txtLns(3)
                    quadVts = arQuadsH.Item(hqd)
                    cnt = 0
                    'loop on the points in the quadrant
                    For i = 1 To quadVts.Count
                        v13 = quadVts.Item(i)
                        bDimIt = Not v13.Suppressed
                        d1 = v13.Value
                        Dim haveit As Boolean = hVals.IndexOf(d1) >= 0



                        If hqd <> basequad And Not v13.Mark Then
                            If d1 = 0 Then bDimIt = False
                        End If
                        If Not v13.Mark And bDimIt Then
                            If haveit Then bDimIt = False
                        End If
                        If Not bDimIt Then Continue For

                        cnt += 1
                        'use the extreme if there are multiples at the same ordinate
                        bVrts = quadVts.GetByValue(v13.Value)
                        If bVrts.Count > 1 Then
                            v13 = bVrts.NearestToLine(txtLn)
                        End If
                        hVals.Add(v13.Value)
                        v14 = v13.ProjectedTo(txtLn)
                        If aHShift <> 0 Then
                            d1 = Math.Abs(aHShift)
                            If aInvertStacks Then d1 *= -1
                            v14.Vector += stepDir * d1
                        End If
                        If cnt > 1 Then
                            'collision test
                            limLine = aPl.LineV(v14Last.Vector, 10, bByStartPt:=True)
                            limLine.Project(stepDir, minspan)
                            v2 = dxfProjections.ToLine(v14.Vector, limLine, rOrthoDirection:=aDir, rDistance:=d1)
                            bMoveIt = False
                            If d1 > 0.00001 Then

                                bMoveIt = aDir.Equals(stepDir, 4)
                            End If
                            If bMoveIt Then v14.Vector = limLine.SPT
                        End If
                        srcVrt = aVrts.Item(v13.Col)
                        hDim.DefPt13V = srcVrt.Vector
                        hDim.DefPt14V = v14.Vector
                        hDim.TFVSet(srcVrt.Tag, srcVrt.Flag, srcVrt.Value)
                        lDim = hDim.Clone
                        lDim.Tag = srcVrt.Tag
                        lDim.Flag = srcVrt.Flag
                        lDim.ImageGUID = iGUID
                        lDim.SetImage(aImage, False)
                        If bTagsAsSuffixes Then lDim.DimStyle.Suffix = lDim.Tag
                        If Not bCreateOnly Then lDim = aImage.Entities.Add(lDim)
                        _rVal.Add(lDim)
                        v14Last = v14
                    Next i
                Next q
                Return _rVal
            Catch ex As Exception
                Dim rErrorString As String = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function DimensionVerticesHV(aDimType As dxxOrdinateDimTypes, aDimensionOrigin As Boolean, aBaseVertexXY As iVector,
                                            aVerticesXY As IEnumerable(Of iVector), aCenterXY As iVector, aDimOffset As Double,
                                            Optional aInvertStacks As Boolean = False, Optional aSpaceAdder As Double = 0.0,
                                            Optional aClockwise As Boolean = False, Optional aUniqueOnly As Boolean = False,
                                            Optional aOrdinateToOmmit As Double? = Nothing, Optional aOrdsToOmmit As List(Of Double) = Nothing,
                                            Optional aTagsToIgnore As List(Of String) = Nothing, Optional bDontDimBase As Boolean = False,
                                            Optional aShift1 As Double = 0.0, Optional aShift2 As Double = 0.0,
                                            Optional aPlane As dxfPlane = Nothing, Optional aDimStyle As String = "",
                                            Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aCollector As colDXFEntities = Nothing, Optional bCreateOnly As Boolean = False,
                                            Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As List(Of dxeDimension)
            Dim _rVal As New List(Of dxeDimension)
            LastError = ""
            If Not GetImage(aImage) Then Return _rVal
            '#1the type of Dimension being requested (must be OrdinateHorizontal or OrdinateVertical)
            '#2flag to add the plane origin to the dimension points
            '#3the base vertex (0,0) for the returned dimension. nothing means use the plane origin
            '#4the collection of vertices to dimension
            '#5the center to use for division of dimension points into quadrants
            '#6the offset distance for the placement of the dimensions
            '#7flag to reverses how the dimensions get stacked
            '#8a distance to add to the minimum distance between the text of the stack
            '#9flag to reverse the direction of the offsets
            '#10flag to only dimension each ordinate once
            '#11a ordinate to skip completely (absolute relative to base point)
            '#12a a group of ordinates to skip completely
            '#13a comma delimited list of point tags to ignore completely
            '#14a flag to prevent the base point from being dimensioned
            '#15a distance to shift the first dimensions
            '#16a distance to shift the second dimensions
            '#17if passed the dimensions are created on this plane otherwise the current UCS XY plane is used
            '#18a dim style to use other than the current dim style
            '#19a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#20a color to apply to the entity instead of the current color
            '#21a line type to apply to the entity instead of the current line type setting
            '#22flag to create the dimensions but not to add them to the image
            '^used to dimension a set of points with ordinate dimensions
            '~this routine attempts to dimension all of the X and Y ordinates of the points in the passed collection.
            '~It try's to avoid overlap of dimensions and repeating dimensions where several vertices share the same ordinate.
            '~points with a flag property = "NODIM" do not get dimensioned.
            '~points with a flag property = "V" alway get a vertical dimension.
            '~points with a flag property = "H" alway get a horizontal dimension.
            '~points with a flag property = "HV" alway get a vertical and horizontal dimension.
            '~points with a flag property = "NOV" never get a vertical dimension.
            '~points with a flag property = "NOH" never get a horizontal dimension.
            '~points with a flag property = "VONLY"  alway get a vertical and never get a horizontal dimension.
            '~points with a flag property = "HONLY"  alway get a horizontal and never get a vertical dimension.
            Try
                Dim aDim As dxeDimension
                Dim lDim As dxeDimension
                Dim bndRec As New TPLANE("")
                Dim quadRec As New TPLANE("")
                Dim aVecs As New TVECTORS
                Dim aVrts As New TVERTICES
                Dim bVrts As New TVERTICES
                Dim txtLn As New TLINE
                Dim v14 As New TVERTEX
                Dim v13 As New TVERTEX
                Dim v14Last As New TVERTEX
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim stepDir As TVECTOR
                Dim aDir As TVECTOR
                Dim aPl As New TPLANE("")
                Dim limLine As New TLINE
                Dim eStruc As New TDISPLAYVARS
                Dim quads(0 To 2) As TVERTICES
                Dim quadVts As New TVERTICES
                Dim vrt As New TVERTEX
                Dim dStyle As dxoDimStyle = Nothing
                Dim tStyle As dxoStyle = Nothing
                Dim txtLns(0 To 2) As TLINE
                Dim sortLns(0 To 2) As TLINE
                Dim i As Integer
                Dim j As Integer
                Dim aPrecis As Integer
                Dim qd As Integer
                Dim dType As dxxDimTypes = dxxDimTypes.OrdHorizontal
                Dim vcnt As Integer
                Dim cnt As Integer
                Dim iGUID As String
                Dim vVals() As Double
                Dim tht As Double
                Dim tgap As Double
                Dim minspan As Double
                Dim d1 As Double
                Dim d2 As Double
                Dim bDimIt As Boolean
                Dim bMoveIt As Boolean
                Dim bTestTag As Boolean
                'intitialize
                If dxfPlane.IsNull(aPlane) Then aPl = aImage.obj_UCS Else aPl = New TPLANE(aPlane)
                iGUID = aImage.GUID
                If aBaseVertexXY IsNot Nothing Then
                    aPl.Origin = New TVECTOR(aBaseVertexXY)
                End If
                'create the dimension points relative to the plane origin
                aVrts = dxfVectors.GetTVERTICES(aVerticesXY)
                If aVrts.Count <= 0 Then Throw New Exception("Undefined Dimension Points Detected")
                If aDimType = dxxOrdinateDimTypes.OrdVertical Then dType = dxxDimTypes.OrdVertical
                'style layer color etc
                Dim dInput As TDIMINPUT
                If aDisplaySettings Is Nothing Then
                    dInput = New TDIMINPUT(aDimStyle, "")
                Else
                    dInput = New TDIMINPUT(aDimStyle, "") With {.LayerName = aDisplaySettings.LayerName, .Color = aDisplaySettings.Color, .Linetype = aDisplaySettings.Linetype}
                End If
                eStruc = dxfImageTool.DisplayStructure_DIM(aImage, False, dInput, rDStyle:=dStyle, rTStyle:=tStyle)
                'set the dimstyle properties
                dStyle.PropValueSet(dxxDimStyleProperties.DIMPREFIX, "")
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSUFFIX, "")
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSD1, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSD2, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSE1, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMSE2, False)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMATFIT, dxxDimTextFitTypes.BestFit)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMTAD, dxxDimTadSettings.Centered)
                dStyle.PropValueSet(dxxDimStyleProperties.DIMJUST, dxxDimJustSettings.Centered)
                aPrecis = dStyle.PropValueI(dxxDimStyleProperties.DIMDEC)
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 8)
                d1 = dStyle.PropValueD(dxxDimStyleProperties.DIMSCALE)
                tht = dStyle.PropValueD(dxxDimStyleProperties.DIMTXT) * d1
                tgap = dStyle.PropValueD(dxxDimStyleProperties.DIMGAP)
                tgap = d1 * tgap
                If tgap = 0 Then tgap = 0.09 * d1
                d2 = Math.Abs(tgap) + Math.Abs(aSpaceAdder)
                If tgap > 0 Then
                    minspan = d2 + tht
                Else
                    minspan = d2 + tht + Math.Abs(tgap)
                End If
                'add the origin if it is not present
                If aDimensionOrigin Then
                    v1 = aPl.Origin
                    bVrts = aVrts.GetAtCoordinate(v1.X, v1.Y, v1.Z, aPrecis + 1, bJustOne:=True)
                    If bVrts.Count <= 0 Then
                        aVrts.Add(New TVERTEX(v1.X, v1.Y, v1.Z, aFlag:="HV"))
                    End If
                End If
                'create the bounding rectangle and set the centroid for point division
                bndRec = aVrts.Bounds(aPl)
                aVecs.ProjectTo(aPl)
                quadRec = bndRec
                If aCenterXY IsNot Nothing Then
                    quadRec.Origin = New TVECTOR(aCenterXY)
                    quadRec.Origin = quadRec.Origin.ProjectedTo(aPl)
                End If
                aDimOffset = Math.Round(aDimOffset, 6)
                If aDimOffset = 0 Then aDimOffset = 0.1 * aImage.obj_DISPLAY.PaperScale
                If dType = dxxDimTypes.OrdHorizontal Then
                    d1 = 0.5 * bndRec.Height + Math.Abs(aDimOffset)
                    If Not aInvertStacks Then
                        txtLns(1) = bndRec.LineH(-d1, 10)
                        txtLns(2) = bndRec.LineH(d1, 10)
                    Else
                        txtLns(1) = bndRec.LineH(d1, 10)
                        txtLns(2) = bndRec.LineH(-d1, 10)
                    End If
                Else
                    d1 = 0.5 * bndRec.Width + Math.Abs(aDimOffset)
                    If Not aInvertStacks Then
                        txtLns(1) = bndRec.LineV(-d1, 10)
                        txtLns(2) = bndRec.LineV(d1, 10)
                    Else
                        txtLns(1) = bndRec.LineV(d1, 10)
                        txtLns(2) = bndRec.LineV(-d1, 10)
                    End If
                End If
                aDim = New dxeDimension(dStyle, dType, eStruc, aPl) With {.DefPt10V = aPl.Origin}
                'eliminate points not to be dimension
                bTestTag = aTagsToIgnore IsNot Nothing
                If Not bTestTag Then aTagsToIgnore = New List(Of String)
                If aOrdinateToOmmit IsNot Nothing Then
                    If aOrdinateToOmmit.HasValue Then
                        If aOrdsToOmmit Is Nothing Then aOrdsToOmmit = New List(Of Double)
                        aOrdsToOmmit.Add(aOrdinateToOmmit.Value)
                    End If


                End If
                'define the dim points quadrants based on the bounding rectangle center
                'and suppress points not to be dimension
                For i = 1 To aVrts.Count
                    aVrts.SetFlag(aVrts.Flag(i).ToUpper.Trim, i)
                    aVrts.SetInverted(False, i)
                    vrt = aVrts.Item(i)
                    bDimIt = vrt.Flag <> "NODIM"
                    If bTestTag Then
                        If Not String.IsNullOrEmpty(vrt.Tag) Then
                            If aTagsToIgnore.Find(Function(mem) String.Compare(mem, vrt.Tag, ignoreCase:=True)) >= 0 Then bDimIt = False
                        End If
                    End If
                    If bDimIt Then
                        v1 = aVecs.Item(i)
                        v2 = v1.WithRespectTo(quadRec, aPrecis:=aPrecis)
                        v3 = v1.WithRespectTo(aPl, aPrecis:=aPrecis)
                        If dType = dxxDimTypes.OrdHorizontal Then
                            vrt.Value = v3.X
                            If vrt.Flag = "NOH" Or vrt.Flag = "VONLY" Then
                                vrt.Suppressed = True
                            End If
                            vrt.Inverted = vrt.Flag = "H" Or vrt.Flag = "HV" Or vrt.Flag = "HONLY" 'force H dim
                        Else
                            If vrt.Flag = "NOV" Or vrt.Flag = "HONLY" Then
                                vrt.Suppressed = True
                            End If
                            vrt.Value = v3.Y
                            vrt.Inverted = vrt.Flag = "V" Or vrt.Flag = "HV" Or vrt.Flag = "VONLY" 'force V dim
                        End If
                        If bDontDimBase Then
                            If dType = dxxDimTypes.OrdHorizontal Then
                                If v3.X = 0 Then vrt.Suppressed = True
                            Else
                                If v3.Y = 0 Then vrt.Suppressed = True
                            End If
                        End If
                        If aOrdsToOmmit IsNot Nothing Then
                            If aOrdsToOmmit.FindIndex(Function(mem) Math.Abs(Math.Round(mem, aPrecis)) = Math.Abs(vrt.Value)) >= 0 Then
                                vrt.Suppressed = True
                            End If
                        End If
                        If Not vrt.Suppressed Then
                            If dType = dxxDimTypes.OrdHorizontal Then
                                If Not aInvertStacks Then
                                    If v2.Y <= 0 Then qd = 1 Else qd = 2
                                Else
                                    If v2.Y < 0 Then qd = 2 Else qd = 1
                                End If
                            Else
                                If Not aInvertStacks Then
                                    If v2.X <= 0 Then qd = 1 Else qd = 2
                                Else
                                    If v2.X < 0 Then qd = 2 Else qd = 1
                                End If
                            End If
                            vrt.Vector = v1
                            vrt.Color = i
                            quads(qd).Add(vrt)
                        End If
                    End If
                Next i
                'sort the vectors
                If dType = dxxDimTypes.OrdHorizontal Then
                    If Not aClockwise Then
                        sortLns(1) = aPl.LineV(quadRec.Point(dxxRectanglePts.BottomLeft), 10, bByStartPt:=True)
                        sortLns(2) = aPl.LineV(quadRec.Point(dxxRectanglePts.BottomRight), 10, bByStartPt:=True)
                    Else
                        sortLns(1) = aPl.LineV(quadRec.Point(dxxRectanglePts.BottomRight), 10, bByStartPt:=True)
                        sortLns(2) = aPl.LineV(quadRec.Point(dxxRectanglePts.BottomLeft), 10, bByStartPt:=True)
                    End If
                Else
                    If Not aClockwise Then
                        sortLns(1) = aPl.LineH(quadRec.Point(dxxRectanglePts.TopLeft), 10, bByStartPt:=True)
                        sortLns(2) = aPl.LineH(quadRec.Point(dxxRectanglePts.BottomRight), 10, bByStartPt:=True)
                    Else
                        sortLns(1) = aPl.LineH(quadRec.Point(dxxRectanglePts.BottomRight), 10, bByStartPt:=True)
                        sortLns(2) = aPl.LineH(quadRec.Point(dxxRectanglePts.TopLeft), 10, bByStartPt:=True)
                    End If
                End If
                quads(1).SortByDistanceToLine(sortLns(1), aInvertStacks)
                quads(2).SortByDistanceToLine(sortLns(2), aInvertStacks)
                'create and save the dimensions
                vcnt = 0
                ReDim vVals(0)
                For qd = 1 To 2
                    If dType = dxxDimTypes.OrdHorizontal Then
                        stepDir = aPl.XDirection
                    Else
                        stepDir = (aPl.YDirection * -1)
                    End If
                    If Not aInvertStacks Then
                        If Not aClockwise Then
                            If qd = 2 Then stepDir *= -1
                        Else
                            If qd = 1 Then stepDir *= -1
                        End If
                    Else
                        If Not aClockwise Then
                            If qd = 1 Then stepDir *= -1
                        Else
                            If qd = 2 Then stepDir *= -1
                        End If
                    End If
                    If Not aUniqueOnly Then
                        vcnt = 0
                        ReDim vVals(0)
                    End If
                    lDim = Nothing
                    d1 = 0.5 * bndRec.Width + Math.Abs(aDimOffset)
                    txtLn = txtLns(qd)
                    quadVts = quads(qd)
                    cnt = 0
                    'loop on the points in the quadrant
                    For i = 0 To quadVts.Count - 1
                        v13 = quadVts.Item(i + 1)
                        bDimIt = True
                        d1 = v13.Value
                        If Not v13.Inverted Then
                            For j = 1 To vcnt
                                If vVals(j) = v13.Value Then
                                    bDimIt = False
                                    Exit For
                                End If
                            Next j
                        End If
                        If bDimIt Then
                            cnt += 1
                            'passed the test so make a dimension for the point
                            'use the extreme if there are multiples at the same ordinate
                            bVrts = quadVts.GetByValue(v13.Value)
                            If bVrts.Count > 1 Then
                                v13 = bVrts.NearestToLine(txtLn)
                            End If
                            vcnt += 1
                            System.Array.Resize(vVals, vcnt + 1)
                            vVals(vcnt) = v13.Value
                            v14 = dxfProjections.ToLine(v13, txtLn, d1)
                            If qd = 1 Then
                                If aShift1 <> 0 Then
                                    If dType = dxxDimTypes.OrdHorizontal Then
                                        v14.Vector += aPl.XDirection * aShift1
                                    Else
                                        v14.Vector += aPl.YDirection * aShift1
                                    End If
                                End If
                            Else
                                If aShift2 <> 0 Then
                                    If dType = dxxDimTypes.OrdHorizontal Then
                                        v14.Vector += aPl.XDirection * aShift2
                                    Else
                                        v14.Vector += aPl.YDirection * aShift2
                                    End If
                                End If
                            End If
                            If cnt > 1 Then
                                If dType = dxxDimTypes.OrdHorizontal Then
                                    limLine = aPl.LineV(v14Last.Vector, 10, bByStartPt:=True)
                                Else
                                    limLine = aPl.LineH(v14Last.Vector, 10, bByStartPt:=True)
                                End If
                                limLine.Project(stepDir, minspan)
                                v2 = dxfProjections.ToLine(v14.Vector, limLine, rOrthoDirection:=aDir, rDistance:=d1)
                                bMoveIt = False
                                If d1 > 0.00001 Then
                                    bMoveIt = aDir.Equals(stepDir, 4)
                                End If
                                If bMoveIt Then
                                    v14.Vector = limLine.SPT
                                End If
                            End If
                            lDim = aDim.Clone
                            lDim.DefPt13V = aVrts.Vector(v13.Color)
                            lDim.DefPt14V = v14.Vector
                            lDim.ImageGUID = iGUID
                            lDim.SetImage(aImage, False)
                            If Not bCreateOnly Then lDim = aImage.Entities.Add(lDim)
                            _rVal.Add(lDim)
                            If aCollector IsNot Nothing Then aCollector.Add(lDim)
                            v14Last = v14
                        End If
                    Next i
                Next qd
                '+++++++++++++++++++++++++++++++++++
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
        Private Function xAngular(aImage As dxfImage, aDStyle As dxoDimStyle, aInput As TDIMINPUT) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            Dim ppt As TVECTOR
            Dim aPl As TPLANE = aInput.Plane
            Dim line1 As TLINE
            Dim line2 As TLINE
            'initiaize
            If aDStyle Is Nothing Then aDStyle = New dxoDimStyle(aImage.DimStyle) With {.IsCopied = True}
            Try
                aInput.XYPoint1.ProjectTo(aPl)
                aInput.XYPoint2.ProjectTo(aPl)
                aInput.XYPoint3.ProjectTo(aPl)
                aInput.XYPoint4.ProjectTo(aPl)
                aInput.XYPoint5.ProjectTo(aPl)
                If aInput.XYPoint1.DistanceTo(aInput.XYPoint2) <= 0 Then Throw New Exception("Undefine Line Passed")
                If aInput.XYPoint3.DistanceTo(aInput.XYPoint4) <= 0 Then Throw New Exception("Undefine Line Passed")
                line1 = New TLINE(aInput.XYPoint1, aInput.XYPoint2)
                line2 = New TLINE(aInput.XYPoint3, aInput.XYPoint4)
                ppt = aInput.XYPoint5
                If Not line1.Intersects(line2) Then Throw New Exception("The Passed Lines Do Not Intersect")
                _rVal = New dxeDimension(aDStyle, dxxDimTypes.Angular, aInput.DisplayVars, aPl) With {
                    .SuppressEvents = True,
                    .OverideText = aInput.OverideText,
                    .DefPt10V = line1.EPT,
                    .DefPt15V = line1.SPT,
                    .DefPt13V = line2.SPT,
                    .DefPt14V = line2.EPT,
                    .DefPt16V = ppt
                }
                If aInput.LinearPrecision.HasValue Then _rVal.DimStyle.LinearPrecision = aInput.LinearPrecision.Value
                If aInput.AngularPrecision.HasValue Then _rVal.DimStyle.AngularPrecision = aInput.AngularPrecision.Value
                _rVal.SetImage(aImage, False)
                _rVal.SuppressEvents = False
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Private Function xAngular3P(aImage As dxfImage, aDStyle As dxoDimStyle, aInput As TDIMINPUT) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            Try
                If aDStyle Is Nothing Then aDStyle = New dxoDimStyle(aImage.DimStyle) With {.IsCopied = True}
                Dim aPl As TPLANE = aInput.Plane
                Dim ip As TVECTOR = aInput.XYPoint1.ProjectedTo(aPl)
                Dim ep1 As TVECTOR = aInput.XYPoint2.ProjectedTo(aPl)
                Dim ep2 As TVECTOR = aInput.XYPoint3.ProjectedTo(aPl)
                Dim ppt As TVECTOR = aInput.XYPoint4.ProjectedTo(aPl)

                If dxfProjections.DistanceTo(ip, ep1, 3) <= 0 Then Throw New Exception("co-Incident points passed")
                If dxfProjections.DistanceTo(ip, ep2, 3) <= 0 Then Throw New Exception("co-Incident points passed")
                If dxfProjections.DistanceTo(ep2, ep1, 3) <= 0 Then Throw New Exception("co-Incident points passed")
                If dxfProjections.DistanceTo(ip, ppt, 3) <= 0 Then Throw New Exception("co-Incident points passed")
                If dxfProjections.DistanceTo(ep1, ppt, 3) <= 0 Then Throw New Exception("co-Incident points passed")
                If dxfProjections.DistanceTo(ep2, ppt, 3) <= 0 Then Throw New Exception("co-Incident points passed")
                _rVal = New dxeDimension(aDStyle, dxxDimTypes.Angular3P, aInput.DisplayVars, aPl) With {.SuppressEvents = True,
                   .OverideText = aInput.OverideText,
                    .DefPt10V = ppt,
                    .DefPt13V = ep1,
                    .DefPt14V = ep2,
                    .DefPt15V = ip}
                If aInput.LinearPrecision.HasValue Then _rVal.DimStyle.LinearPrecision = aInput.LinearPrecision.Value
                If aInput.AngularPrecision.HasValue Then _rVal.DimStyle.AngularPrecision = aInput.AngularPrecision.Value
                _rVal.SetImage(aImage, False)
                _rVal.SuppressEvents = False
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Private Function xLinear(aDimType As dxxLinearDimTypes, aImage As dxfImage, aDStyle As dxoDimStyle, aInput As TDIMINPUT) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            Try
                Dim xscal As Double = aImage.obj_DISPLAY.PaperScale
                Dim ang As Double
                Dim v10 As TVECTOR
                Dim osetDir As TVECTOR
                Dim bLine As New TLINE

                Dim dType As dxxDimTypes
                Dim aPl As TPLANE = aInput.Plane
                Dim v13 As TVECTOR = aInput.XYPoint1.ProjectedTo(aPl)
                Dim v14 As TVECTOR = aInput.XYPoint2.ProjectedTo(aPl)
                If aDStyle Is Nothing Then aDStyle = New dxoDimStyle(aImage.DimStyle) With {.IsCopied = True}
                'TWO D ONLY
                If aDimType = dxxLinearDimTypes.LinearHorizontal Then
                    dType = dxxDimTypes.LinearHorizontal
                    ang = 0
                ElseIf aDimType = dxxLinearDimTypes.LinearVertical Then
                    dType = dxxDimTypes.LinearVertical
                    ang = 90
                Else
                    dType = dxxDimTypes.LinearAligned
                    ang = v13.DirectionTo(v14).AngleTo(aPl.XDirection, aPl.ZDirection)
                End If
                _rVal = New dxeDimension(aDStyle, dType, aInput.DisplayVars, aPl) With {
                    .SuppressEvents = True,
                    .DefPt14V = v14,
                    .DefPt13V = v13
                }
                If aDimType = dxxLinearDimTypes.LinearHorizontal Then
                    osetDir = aPl.YDirection
                    If aInput.AbsolutePlacement Then v10 = aPl.Vector(0, aInput.DimOffset)
                ElseIf aDimType = dxxLinearDimTypes.LinearVertical Then
                    osetDir = aPl.XDirection
                    If aInput.AbsolutePlacement Then v10 = aPl.Vector(aInput.DimOffset)
                ElseIf aDimType = dxxLinearDimTypes.LinearAligned Then
                    osetDir = v13.DirectionTo(v14)
                    osetDir.RotateAbout(aPl.ZDirection, 90, False)
                    If aInput.AbsolutePlacement Then v10 = aPl.Vector(aInput.DimOffset, aInput.DimOffset)
                End If
                If Not aInput.AbsolutePlacement Then
                    _rVal.TextOffset = aInput.TextOffset * xscal
                    If aInput.DimOffset < 0 Then osetDir *= -1
                    aInput.DimOffset = Math.Abs(aInput.DimOffset) * xscal
                    'set the dimension line pt point
                    v10 = v14 + osetDir * aInput.DimOffset
                Else
                    bLine = New TLINE(v14, v14 + osetDir * 10)
                    v10 = v10.ProjectedTo(bLine)
                End If
                _rVal.DefPt10V = v10
                _rVal.OverideText = aInput.OverideText
                _rVal.Angle = ang
                If aInput.LinearPrecision.HasValue Then _rVal.DimStyle.LinearPrecision = aInput.LinearPrecision.Value
                If aInput.AngularPrecision.HasValue Then _rVal.DimStyle.AngularPrecision = aInput.AngularPrecision.Value
                _rVal.IsDirty = False
                _rVal.SetImage(aImage, False)
                _rVal.SuppressEvents = False
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Private Function xOrdinate(aDimType As dxxOrdinateDimTypes, aImage As dxfImage, aDStyle As dxoDimStyle, aInput As TDIMINPUT) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            Try
                Dim xscal As Double
                Dim aDir As TVECTOR
                Dim osetDir As TVECTOR
                Dim v13 As TVECTOR
                Dim v10 As TVECTOR
                Dim v14 As TVECTOR
                Dim aPl As TPLANE = aInput.Plane
                Dim dType As dxxDimTypes
                If aDStyle Is Nothing Then aDStyle = New dxoDimStyle(aImage.DimStyle) With {.IsCopied = True}
                'initiaize
                v10 = aInput.XYPoint1.ProjectedTo(aPl)
                v13 = aInput.XYPoint2.ProjectedTo(aPl)
                'TWO D ONLY
                If aDimType = dxxOrdinateDimTypes.OrdVertical Then
                    osetDir = aPl.XDirection
                    dType = dxxDimTypes.OrdVertical
                Else
                    osetDir = aPl.YDirection
                    dType = dxxDimTypes.OrdHorizontal
                End If
                _rVal = New dxeDimension(aDStyle, dType, aInput.DisplayVars, aPl) With {
                    .SuppressEvents = True,
                    .DefPt10V = v10,
                    .DefPt13V = v13
                }
                If aInput.AbsolutePlacement Then
                    If aDimType = dxxOrdinateDimTypes.OrdHorizontal Then
                        v14.X = aInput.TextOffset
                        v14.Y = aInput.DimOffset
                    Else
                        v14.Y = aInput.TextOffset
                        v14.X = aInput.DimOffset
                    End If
                    v14.ProjectTo(aPl)
                Else
                    xscal = aImage.obj_DISPLAY.PaperScale
                    aInput.TextOffset *= xscal
                    If aInput.DimOffset < 0 Then osetDir *= -1
                    aInput.DimOffset = Math.Abs(aInput.DimOffset) * xscal
                    v14 = v13 + osetDir * aInput.DimOffset
                    If aInput.TextOffset <> 0 Then
                        If aDimType = dxxOrdinateDimTypes.OrdHorizontal Then
                            aDir = aPl.XDirection
                        Else
                            aDir = aPl.YDirection
                        End If
                        If aInput.TextOffset < 0 Then aDir *= -1
                        v14 += aDir * Math.Abs(aInput.TextOffset)
                    End If
                End If
                _rVal.OverideText = aInput.OverideText
                If aInput.aSingle <> 0 Then _rVal.TextRotation = TVALUES.NormAng(aInput.aSingle, False, True)
                _rVal.DefPt14V = v14
                If aInput.LinearPrecision.HasValue Then _rVal.DimStyle.LinearPrecision = aInput.LinearPrecision.Value
                If aInput.AngularPrecision.HasValue Then _rVal.DimStyle.AngularPrecision = aInput.AngularPrecision.Value
                _rVal.SetImage(aImage, False)
                _rVal.SuppressEvents = False
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Private Function xRadial(aDimType As dxxRadialDimensionTypes, aArc As dxeArc, aImage As dxfImage, aDStyle As dxoDimStyle, aInput As TDIMINPUT) As dxeDimension
            Dim _rVal As dxeDimension = Nothing
            Try
                If aArc Is Nothing Then Throw New Exception("The Passed Arc Is Undefined")
                If aArc.Radius <= 0 Then Throw New Exception("The Passed Arc Has a Zero Radius")
                Dim aOCS As dxfPlane
                Dim aDir As dxfDirection
                Dim tpt As dxfVector
                Dim cp As dxfVector
                Dim ppt As dxfVector
                Dim rad As Double
                Dim dpt As dxfVector
                Dim dType As dxxDimTypes = dxxDimTypes.Radial
                If Not TPLANE.IsNull(aInput.Plane) Then aOCS = New dxfPlane(aInput.Plane) Else aOCS = New dxfPlane(aArc.PlaneV)
                aOCS.OriginV = aImage.obj_UCS.Origin
                If aDStyle Is Nothing Then aDStyle = New dxoDimStyle(aImage.DimStyle) With {.IsCopied = True}
                If aDimType = dxxRadialDimensionTypes.Diametric Then dType = dxxDimTypes.Diametric
                cp = aArc.Center
                rad = aArc.Radius
                aDir = aOCS.AngularDirection(aInput.PlacementAngle)
                'aDir.RotateAbout(aOCS.ZDirection, aInput.PlacementAngle)
                'aDir = aOCS.XDirection
                tpt = cp + (aDir * aInput.PlacementDistance)
                ppt = cp + (aDir * rad)
                dpt = cp + (aDir.Inverse * rad)
                'initiaize
                _rVal = New dxeDimension(aDStyle, dType, aInput.DisplayVars, aOCS.Strukture) With {
                    .SuppressEvents = True
                }
                If aInput.CenterMarkSize.HasValue Then _rVal.DimStyle.CenterMarkSize = aInput.CenterMarkSize.Value
                If aInput.LinearPrecision.HasValue Then _rVal.DimStyle.LinearPrecision = aInput.LinearPrecision.Value
                If aInput.AngularPrecision.HasValue Then _rVal.DimStyle.AngularPrecision = aInput.AngularPrecision.Value
                _rVal.OverideText = aInput.OverideText
                _rVal.DefPt11.MoveTo(tpt)
                If aDimType = dxxRadialDimensionTypes.Radial Then
                    _rVal.DefPt10.MoveTo(cp)
                    _rVal.DefPt15.MoveTo(ppt)
                Else
                    _rVal.DefPt15.MoveTo(ppt)
                    _rVal.DefPt10.MoveTo(dpt)
                End If
                _rVal.SetImage(aImage, False)
                _rVal.SuppressEvents = False
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Function xCreateDimension(aImage As dxfImage, aInput As TDIMINPUT, aArc As dxeArc, ByRef rErrString As String) As dxeDimension
            rErrString = ""
            Dim _rVal As dxeDimension = Nothing
            If Not GetImage(aImage) Then Return Nothing
            '#1returns then settings for the new dimension
            '#2the arc being dimensioned if the requested dimension is radial
            '#3the dimstyle to apply
            '#4returns the error desription if one occurs
            '#5flag to suppress validation of the dim style
            '^creates and returns a dimension with its style settings set
            'all dimensions created by the entity tool must pass through here
            Dim lType As dxxLinearDimTypes
            Dim otype As dxxOrdinateDimTypes
            Dim rType As dxxRadialDimensionTypes
            Dim dstyle As dxoDimStyle = Nothing
            Dim tstyle As dxoStyle = Nothing
            Dim dType As dxxDimTypes = aInput.DimType
            Try
                'get the file if it is not passed
                aInput.Plane.Verify(True, aImage.UCS)
                'get the display vars and styles
                dxfImageTool.DisplayStructure_DIM(aImage, False, aInput, rDStyle:=dstyle, rTStyle:=tstyle)
                'Dim aDStyle As New TTABLEENTRY(dstyle)
                'Dim aTStyle As New TTABLEENTRY(tstyle)

                Status = $"Creating {dxfEnums.DisplayName(dType)} Dimension"
                Select Case dType
         '------------------------------------ LINEAR ---------------------------------------------------------------------------------------
                    Case dxxDimTypes.LinearHorizontal, dxxDimTypes.LinearAligned, dxxDimTypes.LinearVertical
                        '----------------------------------------------------------------------------------------------------------------------------------------------
                        If dType = dxxDimTypes.LinearHorizontal Then
                            lType = dxxLinearDimTypes.LinearHorizontal
                        ElseIf dType = dxxDimTypes.LinearVertical Then
                            lType = dxxLinearDimTypes.LinearVertical
                        Else
                            lType = dxxLinearDimTypes.LinearAligned
                        End If
                        _rVal = xLinear(lType, aImage, dstyle, aInput)
         '------------------------------------ ORDINATE ---------------------------------------------------------------------------------------
                    Case dxxDimTypes.OrdHorizontal, dxxDimTypes.OrdVertical
                        '----------------------------------------------------------------------------------------------------------------------------------------------
                        If dType = dxxDimTypes.OrdHorizontal Then otype = dxxOrdinateDimTypes.OrdHorizontal Else otype = dxxOrdinateDimTypes.OrdVertical
                        _rVal = xOrdinate(otype, aImage, dstyle, aInput)
         '------------------------------------ RADIAL ---------------------------------------------------------------------------------------
                    Case dxxDimTypes.Radial, dxxDimTypes.Diametric
                        '----------------------------------------------------------------------------------------------------------------------------------------------
                        If dType = dxxDimTypes.Diametric Then rType = dxxRadialDimensionTypes.Diametric Else rType = dxxRadialDimensionTypes.Radial
                        _rVal = xRadial(rType, aArc, aImage, dstyle, aInput)
         '------------------------------------ ANGULAR ---------------------------------------------------------------------------------------
                    Case dxxDimTypes.Angular
                        _rVal = xAngular(aImage, dstyle, aInput)
         ' ---------------------------------- ANGULAR #P ---------------------------------------------------------------------------------------
                    Case dxxDimTypes.Angular3P
                        '----------------------------------------------------------------------------------------------------------------------------------------------
                        _rVal = xAngular3P(aImage, dstyle, aInput)

                    Case Else
                        Throw New Exception("The Passed Entity Type Is Not a Known Dimension Entity type")
                End Select
                _rVal.SuppressEvents = False
                _rVal.ImageGUID = aImage.GUID
                _rVal.SetImage(aImage, False)
                If aInput.SaveAndDraw Then
                    aImage.Entities.Add(_rVal)
                End If
                Status = ""
                aImage.DimStyleOverrides.ForceTextBetweenExtLine = False
                If aImage.DimStyleOverrides.AutoReset Then aImage.DimStyleOverrides.ResetPropsToParent(aImage)
                Return _rVal
            Catch ex As Exception
                rErrString = ex.Message
                Status = "Dimension Creation Error"
                Return _rVal
            End Try
        End Function
        Private Function xStackHV(aDimType As dxxDimTypes, aDimPointsXY As IEnumerable(Of dxfVector), aDimOffset As Double,
                                     Optional aAdditionalDimSpace As Double = 0.0, Optional aBotToTopLeftToRight As Boolean = True,
                                     Optional bTagsAsPrefixes As Boolean = False, Optional bFlagsAsSuffixes As Boolean = False,
                                     Optional aAlignTextToDimLines As Boolean = False, Optional aSuppressBaseExtLine As Boolean = False,
                                     Optional aDimStyle As String = "",
                                     Optional aDisplaySettings As dxfDisplaySettings = Nothing,
                                     Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing,
                                     Optional bCreateOnly As Boolean = False, Optional bSuppressErrors As Boolean = False, Optional aImage As dxfImage = Nothing) As colDXFEntities
            LastError = ""
            Dim _rVal As New colDXFEntities
            'get the image and the working plane
            If Not GetImage(aImage) Then Return _rVal
            '[]Me~Using Dimensions~HelpDocs\UsingDimensions.htm~File
            '#1the type of Dimension being requested (must be Horizontal or Vertical)
            '#2the collection of points to dimension from the base point
            '#3the distance to offset the first dimension line
            '#4an adder to increase the distance between subsequent dimensions.
            '#5a flag indicating that the passed vectors current Tag property values should be used as the prefix for the dimension created with the point
            '#6a flag indicating that the passed vectors current Flag property values should be used as the suffix for the dimension created with the point
            '#7flag to work left to right or right to left for horizontal dims or top to bottom or bottom to top for vertical dimensions
            '#8flag to align the dimension text with the dim lines
            '#9flag to suppress the base extension line
            '#10a dim style to use other than the current dim style
            '#11a layer to put the entity on instead of the current dimension layer (settings.DimLayer)
            '#12a color to apply to the entity instead of the current color
            '#13a line type to apply to the entity instead of the current line type setting
            '#14if passed the dimensions are created on this plane otherwise the current UCS XY plane is used
            '#15flag to just create the dimensions but not to add them to the image
            '^used to create a stack of Horizontal or Vertical linear dimensions
            '~returns a collection of the created dimension.
            '~a negative DimSpace value will cause the stack to extend to the left or down depending on the aDimType requested.
            '~an example of the best use for it would be dimensioning several points from a single centerline.
            Try
                Dim dPoints As colDXFVectors

                Dim dSty As dxoDimStyle = Nothing
                Dim tSty As dxoStyle = Nothing
                Dim aDim As dxeDimension
                Dim bDim As dxeDimension
                Dim lDim As dxeDimension = Nothing
                Dim bPlane As TPLANE
                Dim dInput As TDIMINPUT
                Dim eStruc As TDISPLAYVARS
                Dim osetDir As TVECTOR
                Dim v13 As TVECTOR
                Dim v14 As TVERTEX
                Dim v10 As TVECTOR
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim aLn As New TLINE
                Dim baseLn As New TLINE
                Dim dVerts As New TVERTICES
                Dim dscale As Double
                Dim aPrecis As Integer
                Dim iGUID As String
                Dim i As Integer
                Dim xscal As Double
                Dim tgap As Double
                Dim ext As Double
                Dim sPrefix As String
                Dim sSuffix As String
                Dim d1 As Double
                Dim lBox As dxfRectangle = Nothing
                Dim aBox As dxfRectangle
                Dim osettp As dxxOrthoDirections
                Dim bInvPop As Boolean
                'trap
                If aDimType <> dxxDimTypes.LinearHorizontal And aDimType <> dxxDimTypes.LinearVertical Then aDimType = dxxDimTypes.LinearHorizontal
                If aDimPointsXY Is Nothing Then Throw New Exception("Undefined Point Sets Detected")
                iGUID = aImage.GUID
                'get the points with respect to the plane
                If dxfPlane.IsNull(aPlane) Then aPlane = aImage.UCS
                bPlane = New TPLANE(aPlane)
                dPoints = aPlane.CreateVectors(dxfVectors.ProjectedToPlane(aDimPointsXY))
                If dPoints.Count < 2 Then Return _rVal
                'get the styles and display structure from the image
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearA, aLayerName:="", aColor:=dxxColors.Undefined, aLineType:="", aStyleName:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined, bSuppressed:=False)
                End If
                dInput = New TDIMINPUT(aDimStyle, "", aDimType) With {.LayerName = aDisplaySettings.LayerName, .Color = aDisplaySettings.Color, .Linetype = aDisplaySettings.Linetype}
                xscal = aImage.Display.PaperScale
                eStruc = dxfImageTool.DisplayStructure_DIM(aImage, False, dInput, rDStyle:=dSty, rTStyle:=tSty)
                'set some dimstyle props
                dSty.PropValueSet(dxxDimStyleProperties.DIMSD1, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMSD2, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMSE1, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMSE2, False)
                dSty.PropValueSet(dxxDimStyleProperties.DIMATFIT, dxxDimTextFitTypes.MoveArrowsFirst)
                dSty.PropValueSet(dxxDimStyleProperties.DIMPREFIX, "")
                dSty.PropValueSet(dxxDimStyleProperties.DIMSUFFIX, "")
                If aDimType = dxxDimTypes.LinearVertical Then
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTOH, Not aAlignTextToDimLines)
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTIH, Not aAlignTextToDimLines)
                Else
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTOH, True)
                    dSty.PropValueSet(dxxDimStyleProperties.DIMTIH, True)
                End If
                aPrecis = dSty.PropValueI(dxxDimStyleProperties.DIMDEC)
                dscale = dSty.PropValueD(dxxDimStyleProperties.DIMSCALE)
                tgap = dscale * Math.Abs(dSty.PropValueD(dxxDimStyleProperties.DIMGAP))
                ext = dscale * dSty.PropValueD(dxxDimStyleProperties.DIMEXE)
                aAdditionalDimSpace = Math.Abs(aAdditionalDimSpace)
                'get the base point based on the dimension type
                If aDimType = dxxDimTypes.LinearHorizontal Then
                    osetDir = bPlane.YDirection
                    If aDimOffset < 0 Then
                        osettp = dxxOrthoDirections.Down
                        osetDir *= -1
                        v10 = dxfVectors.FindVertexVector(dPoints, dxxPointFilters.AtMinY)
                        bInvPop = True
                    Else
                        osettp = dxxOrthoDirections.Up
                        v10 = dxfVectors.FindVertexVector(dPoints, dxxPointFilters.AtMaxY)
                        'bInvPop = aBotToTopLeftToRight
                    End If
                    If aBotToTopLeftToRight Then v13 = dPoints.GetVertex(dxxPointFilters.AtMinX, bRemove:=True).Vector Else v13 = dPoints.GetVertex(dxxPointFilters.AtMaxX, bRemove:=True).Vector
                Else
                    osetDir = bPlane.XDirection
                    If aDimOffset < 0 Then
                        osettp = dxxOrthoDirections.Left
                        osetDir *= -1
                        v10 = dxfVectors.FindVertexVector(dPoints, dxxPointFilters.AtMinX)
                    Else
                        bInvPop = True
                        osettp = dxxOrthoDirections.Right
                        v10 = dxfVectors.FindVertexVector(dPoints, dxxPointFilters.AtMaxX)
                    End If
                    If aBotToTopLeftToRight Then v13 = dPoints.GetVertex(dxxPointFilters.AtMinY, bRemove:=True).Vector Else v13 = dPoints.GetVertex(dxxPointFilters.AtMaxY, bRemove:=True).Vector
                End If
                'create the base extension line from the base point
                If aBotToTopLeftToRight Then baseLn = New TLINE(v13, v13 + osetDir * -10) Else baseLn = New TLINE(v13, v13 + osetDir * 10)
                bPlane.Origin = v13
                'create base dimension line
                aLn.SPT = v10 + osetDir * Math.Abs(aDimOffset)
                If aDimType = dxxDimTypes.LinearHorizontal Then
                    aLn.EPT = aLn.SPT + bPlane.XDirection * 1
                Else
                    aLn.EPT = aLn.SPT + bPlane.YDirection * 1
                End If
                'create the template dimension with the common properties
                aDim = New dxeDimension(dSty, aDimType, eStruc, bPlane) With {.DefPt13V = v13}
                'sort the points from the bases smallest distance to largest
                dVerts = New TVERTICES(dPoints)
                dVerts.SortRelativeToLine(baseLn, bPlane.ZDirection)
                'loop on the points away from the base creating dimensions as we go.
                For i = 1 To dVerts.Count
                    v14 = dVerts.Item(i)
                    If bTagsAsPrefixes Then sPrefix = v14.Tag Else sPrefix = ""
                    If bFlagsAsSuffixes Then
                        sSuffix = v14.Flag
                    Else
                        sSuffix = ""
                    End If
                    If i = 1 Then
                        v10 = dxfProjections.ToLine(v14.Vector, aLn, d1)
                    Else
                        Dim aEnts As colDXFEntities = lDim.DimEntities(True)
                        lBox = aEnts.BoundingRectangle(lDim.Plane)
                        Select Case osettp
                            Case dxxOrthoDirections.Down
                                v10 = lBox.BottomCenterV
                            Case dxxOrthoDirections.Left
                                v10 = lBox.MiddleLeftV
                            Case dxxOrthoDirections.Right
                                v10 = lBox.MiddleRightV
                            Case dxxOrthoDirections.Up
                                v10 = lBox.TopCenterV
                        End Select
                        v10 = dxfProjections.ToLine(v10, baseLn, d1)

                    End If
                    If d1 > 0 Or i = 1 Then
                        bDim = aDim.Clone
                        bDim.DefPt13V = v14.Vector
                        bDim.DefPt14V = v13
                        bDim.DefPt10V = v10
                        bDim.DimStyle.Prefix = sPrefix
                        bDim.DimStyle.Suffix = sSuffix
                        bDim.SetImage(aImage, False)
                        bDim.Value = Math.Round(d1, aPrecis)
                        If lDim IsNot Nothing Then
                            v10 = bDim.DefPt10V + osetDir * (aAdditionalDimSpace + tgap)
                            bDim.UpdatePath(False, aImage)
                            aBox = bDim.DimEntities(True).BoundingRectangle(bDim.Plane)
                            v10 = bDim.DefPt10V
                            d1 = 0
                            Select Case osettp
                                Case dxxOrthoDirections.Down
                                    v1 = lBox.BottomCenterV
                                    v2 = aBox.TopCenterV
                                Case dxxOrthoDirections.Up
                                    v1 = lBox.TopCenterV
                                    v2 = aBox.BottomCenterV
                                Case dxxOrthoDirections.Left
                                    v1 = lBox.MiddleLeftV
                                    v2 = aBox.MiddleRightV
                                Case dxxOrthoDirections.Right
                                    v1 = lBox.MiddleRightV
                                    v2 = aBox.MiddleLeftV
                            End Select
                            v1 = v1.WithRespectTo(bPlane)
                            v2 = v2.WithRespectTo(bPlane)
                            Select Case osettp
                                Case dxxOrthoDirections.Down
                                    d1 = v2.Y - (v1.Y - (aAdditionalDimSpace + tgap))
                                Case dxxOrthoDirections.Up
                                    d1 = (v1.Y + (aAdditionalDimSpace + tgap)) - v2.Y
                                Case dxxOrthoDirections.Left
                                    d1 = v2.X - (v1.X - (aAdditionalDimSpace + tgap))
                                Case dxxOrthoDirections.Right
                                    d1 = (v1.X + (aAdditionalDimSpace + tgap)) - v2.X
                            End Select
                            If d1 <> 0 Then
                                v10 += osetDir * d1
                                bDim.DefPt10V = v10
                            End If
                        End If
                        'save the new dimension to the image and return
                        If bCreateOnly Then
                            lDim = _rVal.Add(bDim)
                        Else
                            lDim = _rVal.Add(aImage.Entities.Add(bDim))
                        End If
                        lBox = lDim.DimEntities(True).BoundingRectangle(lDim.Plane)
                        If aCollector IsNot Nothing Then aCollector.Add(lDim)
                    End If
                Next i
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrors Then
                    LastError = aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Else
                    LastError = ex.Message
                End If
                Return _rVal
            End Try
        End Function
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
#End Region 'Methods
    End Class 'dxoDimTool
End Namespace
