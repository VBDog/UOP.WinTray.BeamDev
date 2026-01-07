Imports System.Numerics
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoEntityTool
#Region "Members"
        Private _ImageGUID As String
        Friend ImagePtr As WeakReference
        Private strStatus As String
        Private strLastStatus As String
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage)
            If aImage IsNot Nothing Then
                _ImageGUID = aImage.GUID
                ImagePtr = New WeakReference(aImage)
            End If
        End Sub
#End Region 'Constructors
#Region "Properties"
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
        Public Property Status As String
            Get
                Return strStatus
            End Get
            Set(value As String)
                If strStatus <> value Then
                    strLastStatus = strStatus
                    strStatus = value
                End If
            End Set
        End Property
        Friend ReadOnly Property zMyImage As dxfImage
            Get
                If _ImageGUID <> "" Then Return dxfEvents.GetImage(_ImageGUID) Else Return Nothing
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Private Sub xHandleError(aMethod As Reflection.MethodBase, e As Exception, Optional myImage As dxfImage = Nothing)
            If e Is Nothing Or Not GetImage(myImage) Then Return
            myImage.HandleError(aMethod, Me.GetType(), e)
        End Sub
        Friend Function zCreateBubbleEntities(aImage As dxfImage, aCenter As iVector, BubbleType As dxxBubbleTypes, aHeight As Double, aLength As Double, Optional aMText As dxeText = Nothing, Optional aRotation As Double = 0.0, Optional aOCS As dxfPlane = Nothing) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            Try
                aHeight = Math.Abs(aHeight)
                If aHeight <= 0 Then Return _rVal
                aLength = Math.Abs(aLength)
                If aLength <= 0 Then aLength = aHeight
                If aOCS Is Nothing Then aOCS = aImage.UCS
                Dim rad As Double
                Dim ctr As New dxfVector(aCenter)
                Dim aCirc As dxeArc

                '======= circular bubble
                Select Case BubbleType
                    Case dxxBubbleTypes.Circular
                        rad = aHeight / 2
                        aCirc = New dxeArc(ctr, rad, aPlane:=aOCS)
                        _rVal = aCirc
                    Case dxxBubbleTypes.Hexagonal
                        '========= hexagonal bubble
                        rad = aHeight / 2 / 0.866
                        _rVal = CreateShape_Polygon(6, rad, ctr, aRotation)
                    Case dxxBubbleTypes.Rectangular
                        '========= rectangular bubble
                        _rVal = CreateShape_Rectangle(ctr, aLength, aHeight, aRotation:=aRotation)
                    Case Else
                        '========= pill bubble
                        _rVal = dxfPrimatives.CreatePill(ctr, aLength, aHeight, aRotation)
                End Select
                If aMText IsNot Nothing Then aMText.MoveTo(ctr)
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Function CreateShape_Circle(aCenter As iVector, aRadius As Double, Optional aSegmentWidth As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aOCS As dxfPlane = Nothing) As dxePolyline
            Dim _rVal As dxePolyline = Nothing
            Dim myImage As dxfImage = Nothing
            GetImage(myImage)
            Try
                Dim bOCS As dxfPlane

                Dim rad As Double = Math.Abs(aRadius)
                If rad = 0 Then Throw New Exception("Invalid Input Detected")
                If dxfPlane.IsNull(aOCS) Then
                    If myImage IsNot Nothing Then bOCS = New dxfPlane(myImage.UCS) Else bOCS = New dxfPlane()
                Else
                    bOCS = New dxfPlane(aOCS)
                End If
                If myImage IsNot Nothing Then
                    aDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Polyline, aSettingsToCopy:=aDisplaySettings)


                Else
                    aDisplaySettings = New dxfDisplaySettings(aDisplaySettings)
                End If
                _rVal = dxfPrimatives.CreateCircle(aCenter, rad, bOCS, False, aSegmentWidth, aDisplaySettings)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
                Return _rVal
            End Try
        End Function
        Public Function CreateShape_Pill(aCenter As Object, aLength As Double, aHeight As Double, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aOCS As dxfPlane = Nothing) As dxePolyline
            Dim aImage As dxfImage = Nothing
            Dim _rVal As dxePolyline = Nothing
            If Not GetImage(aImage) Then Return Nothing
            Try
                Dim rad As Double = 0.5 * Math.Abs(aHeight)
                If rad <= 0 Then Throw New Exception("Invalid Input Detected")
                Dim alen As Double
                Dim bOCS As dxfPlane
                Dim aESets As dxfDisplaySettings
                Dim cp As dxfVector
                alen = Math.Abs(aLength)
                If alen <= 2.5 * rad Then alen = 2.5 * rad
                aImage = zMyImage
                If aImage Is Nothing Then Return _rVal
                If aOCS Is Nothing Then bOCS = aImage.UCS Else bOCS = aOCS
                aESets = New dxfDisplaySettings With {.Strukture = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)}
                cp = aImage.CreateVector(aCenter, False)
                _rVal = dxfPrimatives.CreatePill(cp, alen, 2 * rad, aRotation, aSegmentWidth, bOCS, aESets, False)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, aImage)
                Return _rVal
            End Try
        End Function
        Public Function CreateShape_Polygon(aSideCnt As Integer, aRadius As Double, aCenter As dxfVector, Optional aRotation As Double? = Nothing, Optional bXScribed As Boolean = False, Optional aShearAngle As Double = 0.0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aCS As dxfPlane = Nothing, Optional aSegmentWidth As Double = 0.0, Optional bReturnPolygon As Boolean = False) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            Dim aImage As dxfImage = Nothing
            Dim bCS As dxfPlane
            aImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            If aCS Is Nothing Then bCS = aImage.UCS Else bCS = aCS
            Dim aPg As dxePolygon
            Dim aPl As dxePolyline
            If Not bReturnPolygon Then
                aPl = dxfPrimatives.CreatePolygon(aCenter, False, aSideCnt, aRadius, aRotation, bXScribed, aShearAngle, aLayer, aColor, aLineType, bCS, aSegmentWidth)
                aPl.DisplayStructure = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                _rVal = aPl
            Else
                aPg = dxfPrimatives.CreatePolygon(aCenter, True, aSideCnt, aRadius, aRotation, bXScribed, aShearAngle, aLayer, aColor, aLineType, bCS, aSegmentWidth)
                aPg.DisplayStructure = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                _rVal = aPg
            End If
            Return _rVal
        End Function

        ''' <summary>
        ''' used to create a rectanglular polyline
        ''' </summary>
        ''' <remarks>negative widths and heights can be used to invert the Rectangle about the corner point</remarks>
        ''' <param name="aCenterOrCornerXY">1the center or lower left corner of the new rectanglular polyline</param>
        ''' <param name="aWidth">the width of the new rectanglular polyline</param>
        ''' <param name="aHeight">the height of the new rectanglular polyline</param>
        ''' <param name="aDrawMethod">a enum control to determine if the passed point is thel lower left corner or the center of the rectangle</param>
        ''' <param name="aFilletOrChamfer">a fillet or chamfer distance</param>
        ''' <param name="bChamfer">flag indicating that the previous distance is a chamfer</param>
        ''' <param name="aRotation">a rotation angle</param>
        ''' <param name="aSegmentWidth">a segment with for the polyline</param>
        ''' <param name="aLayer">the layer to put the entity on instead of the current layer setting</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color setting</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>

        Public Function CreateShape_Rectangle(aCenterOrCornerXY As iVector, aWidth As Double, aHeight As Double, Optional aDrawMethod As dxxRectangleMethods = dxxRectangleMethods.ByCenter, Optional aFilletOrChamfer As Double = 0.0, Optional bChamfer As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFLag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxePolyline
            Dim _rVal As dxePolyline = Nothing

            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            If aHeight = 0 And aWidth = 0 Then aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType().Name, "Invalid Input Detected")
            If aDrawMethod < 0 Then aDrawMethod = dxxRectangleMethods.ByCenter
            If aDrawMethod > 1 Then aDrawMethod = dxxRectangleMethods.ByCorner



            Dim aOCS As dxfPlane = Nothing
            Dim ctr As TVERTEX = aImage.CreateUCSVertex(aCenterOrCornerXY, bSuppressUCS, bSuppressElevation, aOCS)
            Dim aPl As New TPLANE(aOCS)


            _rVal = New dxePolyline With {.DisplayStructure = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLFlag:=aLTLFLag), .Tag = ctr.Tag, .Flag = ctr.Flag, .Value = ctr.Value}
            Dim hDir As New dxfDirection(aOCS.XDirection)
            Dim vDir As New dxfDirection(aOCS.YDirection)

            If aHeight < 0 Then vDir.Invert()
            If aWidth < 0 Then hDir.Invert()
            aHeight = Math.Abs(aHeight)
            aWidth = Math.Abs(aWidth)
            Dim aFillet As Double = Math.Abs(aFilletOrChamfer)
            If aFillet >= 0.5 * aWidth Then aFillet = 0
            If aFillet >= 0.5 * aHeight Then aFillet = 0
            Dim cp As dxfVector = dxfVector.Zero
            Dim P1 As dxfVector = cp.Projected(hDir, -0.5 * aWidth)
            P1.Project(vDir, 0.5 * aHeight)
            Dim P2 As dxfVector = cp.Projected(hDir, -0.5 * aWidth)
            P2.Project(vDir, -0.5 * aHeight)
            Dim P3 As dxfVector = cp.Projected(hDir, 0.5 * aWidth)
            P3.Project(vDir, -0.5 * aHeight)
            Dim P4 As dxfVector = cp.Projected(hDir, 0.5 * aWidth)
            P4.Project(vDir, 0.5 * aHeight)
            Dim verts As New colDXFVectors(P1, P2, P3, P4)
            P1 = cp.Projected(hDir.Inverse, 0.5 * aWidth)
            If aDrawMethod = dxxRectangleMethods.ByCorner Then
                verts.Project(hDir, 0.5 * aWidth)
                verts.Project(vDir, 0.5 * aHeight)
            End If
            If aFillet <> 0 Then
                P1 = verts.Item(1).Clone
                P2 = verts.Item(2).Clone
                P3 = verts.Item(3).Clone
                P4 = verts.Item(4).Clone
                verts.Clear()
                verts.Add(P1.Projected(hDir, aFillet))
                verts.Add(P1.Projected(vDir.Inverse, aFillet))
                verts.Add(P2.Projected(vDir, aFillet))
                verts.Add(P2.Projected(hDir, aFillet))
                verts.Add(P3.Projected(hDir.Inverse, aFillet))
                verts.Add(P3.Projected(vDir, aFillet))
                verts.Add(P4.Projected(vDir.Inverse, aFillet))
                verts.Add(P4.Projected(hDir.Inverse, aFillet))
                If Not bChamfer Then
                    verts.Item(1).Radius = aFillet
                    verts.Item(3).Radius = aFillet
                    verts.Item(5).Radius = aFillet
                    verts.Item(7).Radius = aFillet
                End If
                If hDir.X = -1 Then
                    For i = 1 To verts.Count
                        P1 = verts.Item(i)
                        P1.Inverted = Not P1.Inverted
                    Next i
                End If
                If vDir.Y = -1 Then
                    For i As Integer = 1 To verts.Count
                        P1 = verts.Item(i)
                        P1.Inverted = Not P1.Inverted
                    Next i
                End If
            End If
            Dim aTr As New TTRANSFORMS(String.Empty)
            If aRotation <> 0 Then
                aTr.Add(TTRANSFORM.CreateRotation(cp.Strukture, aRotation, False, TVECTOR.WorldZ, True))
            End If
            If Math.Round(aOCS.ZDirectionV.Z, 6) <> 1 Then
                aPl = aOCS.Strukture
                aPl.Origin = cp.Strukture
                verts.ConvertToPlane(aPl)
            End If
            If Not TVECTOR.IsNull(ctr.Vector) Then
                aTr.Add(TTRANSFORM.CreateTranslation(ctr.Vector, True))
            End If
            If aTr.Count > 0 Then
                verts.Transform(aTr, True)
            End If
            _rVal.Vertices.Append(verts, bAppendClones:=False)
            _rVal.Closed = True
            If aSegmentWidth > 0 Then _rVal.SegmentWidth = aSegmentWidth
            If aInstances IsNot Nothing Then _rVal.Instances.Copy(aInstances, bSuppressPlane:=True)
            Return _rVal
        End Function
        Public Function CreateSymbol_ArrowPointer(aArrowPtXY As iVector, Optional aAngle As Double = 0.0, Optional aLength As Double = 1, Optional aTrailerText As String = Nothing, Optional aTextHeight As Double = 0.0, Optional aArrowSize As Double? = Nothing, Optional aAboveText As String = Nothing, Optional aBelowText As String = Nothing, Optional aLeadText As String = Nothing, Optional bScaleToScreen As Boolean = False, Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional aTextStyle As String = "", Optional aName As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return Nothing
            '#1the point of the desired arrow to draw (this point gets the Arrow Head)
            '#2the orientation angle of the arrow
            '#3the length of the arrow
            '#4the text string to draw behind the arrow
            '#5the height of the text to apply instead of the current symbol text size setting
            '#6arrow head size to use instead of the current arrow size setting
            '#7the text string to draw above the arrow
            '#8the text string to draw below the arrow
            '#9the text string to draw ahead of the arrow
            '#10flag to rescale the entity to the current display on every redraw
            '#11flag to scale the symbol to the current display
            '#12if True the symbol is saved as a block insert when the file is written
            '#13the line width to use for the arrows lines
            '#14the arrow head style to use to override that of the arrow dim style
            '#15the name to assign to the arrow
            '^used to Create pointing arrows
            Try
                Dim P1 As dxfVector
                Dim aPlane As dxfPlane = Nothing
                'initialize
                'If aArrowSize <= 0 Then aArrowSize = 0.25
                _rVal = xBasicSymbol(aImage, dxxSymbolTypes.Arrow, aTextHeight, aName, bScaleToScreen, aArrowSize, aTextStyle, aArrowType)
                aLength = Math.Abs(aLength)
                If aLength = 0 Then aLength = 1
                If bScaleToScreen Then
                    aLength *= aImage.obj_DISPLAY.PaperScale
                End If
                If aArrowStyle < 0 And (aArrowType >= 0 And aArrowType <= 19) Then
                    aArrowStyle = dxxArrowStyles.StdBlocks
                End If
                P1 = aImage.CreateVector(aArrowPtXY, bSuppressUCS, bSuppressElevation, aPlane)
                _rVal.PlaneV = New TPLANE(aPlane)
                _rVal.SymbolType =
                _rVal.ArrowType = dxxArrowTypes.Pointer
                _rVal.InsertionPtV = P1.Strukture
                If aAboveText IsNot Nothing Then _rVal.Text1 = aAboveText.Trim()
                If aBelowText IsNot Nothing Then _rVal.Text2 = aBelowText.Trim()
                If aTrailerText IsNot Nothing Then _rVal.Text3 = aTrailerText.Trim()
                If aLeadText IsNot Nothing Then _rVal.Text4 = aLeadText.Trim()
                _rVal.Length = aLength
                _rVal.Rotation = TVALUES.NormAng(aAngle, False, True)
                _rVal.ArrowHeadType = aArrowType
                _rVal.ArrowStyle = aArrowStyle
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
            End Try
            Return _rVal
        End Function
        Public Function CreateSymbol_Axis(aPlane As dxfPlane, Optional aAxisLength As Double = 1, Optional aRotation As Double? = Nothing, Optional aXLabel As String = "X", Optional aYLabel As String = "Y", Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aAxisStyle As Long = -1, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional aTextStyle As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim aImage As dxfImage = Nothing
            If TPLANE.IsNull(aPlane) Or Not GetImage(aImage) Then Return _rVal
            '#1plane to create a axis symbol for
            '#2the length of the draw axis lines
            '#3a rotation to apply
            '#4the label for the X axis
            '#5the label for the Y axis
            '#6the text height to apply instead of the current symbol text size setting
            '#7flag to rescale the entity to the current display on every redraw
            '#8flag to align the text with the axis
            '#9if True the symbol is saved as a block insert when the file is written
            '#10flag to scale the axis to the current display
            '#11the name to assign to the arrow
            '^used to create an X,Y axis arrows symbol at the passed point
            Try
                Dim v1 As dxfVector
                Dim plane As dxfPlane = Nothing
                'initialize
                If aTextHeight <= 0 Then aTextHeight = 0
                _rVal = xBasicSymbol(aImage, dxxArrowTypes.Axis, aTextHeight, aName, bScaleToScreen, aTextStyle:=aTextStyle)
                aAxisLength = Math.Abs(aAxisLength)
                If aAxisLength = 0 Then aAxisLength = 1
                If bScaleToScreen Then aAxisLength *= aImage.obj_DISPLAY.PaperScale
                v1 = aImage.CreateVector(aPlane.Origin, bSuppressUCS, bSuppressElevation, plane)
                _rVal.PlaneV = New TPLANE(aPlane)
                _rVal.SymbolType = dxxSymbolTypes.Arrow
                _rVal.InsertionPtV = v1.Strukture
                _rVal.ArrowStyle = aArrowStyle
                _rVal.ArrowType = dxxArrowTypes.Axis
                _rVal.Text1 = aXLabel.Trim
                _rVal.Text2 = aYLabel.Trim
                _rVal.TextHeight = aTextHeight
                If aAxisStyle >= 0 And aAxisStyle <= 1 Then _rVal.AxisStyle = aAxisStyle
                _rVal.Length = aAxisLength
                If aRotation.HasValue Then _rVal.Rotation = TVALUES.NormAng(aRotation.Value, False, True)
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function CreateSymbol_Axis(aOriginXY As iVector, Optional aAxisLength As Double = 1, Optional aRotation As Double? = Nothing, Optional aXLabel As String = "X", Optional aYLabel As String = "Y", Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aAxisStyle As Long = -1, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional aTextStyle As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return Nothing
            '#1point to center the drawn axis
            '#2the length of the draw axis lines
            '#3a rotation to apply
            '#4the label for the X axis
            '#5the label for the Y axis
            '#6the text height to apply instead of the current symbol text size setting
            '#7flag to rescale the entity to the current display on every redraw
            '#8flag to align the text with the axis
            '#9if True the symbol is saved as a block insert when the file is written
            '#10flag to scale the axis to the current display
            '#11the name to assign to the arrow
            '^used to create an X,Y axis arrows symbol at the passed point
            Try
                Dim aPlane As dxfPlane = Nothing
                Dim v1 As TVECTOR = aImage.CreateUCSVector(aOriginXY, bSuppressUCS, bSuppressElevation, aPlane)

                'initialize
                If aTextHeight <= 0 Then aTextHeight = 0
                _rVal = xBasicSymbol(aImage, dxxArrowTypes.Axis, aTextHeight, aName, bScaleToScreen, aTextStyle:=aTextStyle)
                aAxisLength = Math.Abs(aAxisLength)
                If aAxisLength = 0 Then aAxisLength = 1
                If bScaleToScreen Then aAxisLength *= aImage.Display.PaperScale

                _rVal.PlaneV = New TPLANE(aPlane)
                _rVal.SymbolType = dxxSymbolTypes.Arrow
                _rVal.InsertionPtV = v1
                _rVal.ArrowStyle = aArrowStyle
                _rVal.ArrowType = dxxArrowTypes.Axis
                _rVal.Text1 = aXLabel.Trim
                _rVal.Text2 = aYLabel.Trim
                _rVal.TextHeight = aTextHeight
                If aAxisStyle >= 0 And aAxisStyle <= 1 Then _rVal.AxisStyle = aAxisStyle
                _rVal.Length = aAxisLength
                _rVal.Rotation = TVALUES.NormAng(aRotation, False, True)
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function CreateSymbol_Bubble(aBubbleType As dxxBubbleTypes, aInsertPtXY As iVector, bubbleText As String, aBubbleHt As Double, aBubbleLg As Double, Optional aBubbleAngle As Double = 0.0, Optional aHexText As String = Nothing, Optional aTrailerText As String = Nothing, Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            '#1type of bubble to create
            '#2the center for the primary bubble
            '#3text string to put inside the primary bubble
            '#4the height for the bubble
            '#5the length of the bubble
            '#6the angle for the bubble
            '#7a text string to add after the bubble in a hexagon
            '#8a text string to add after the bubble and the hexagon
            '#9a text height to use instead of the current symbol text height setting
            '#10flag to resize the symbol every time the display is resized
            '#11flag to scale teh symbol to the current display
            '#12flag to save the symbol as a block insert when the file is saved
            '#13flag to move the insertion point of the symbol to the leading end of the symbol entities
            '#14the name to assign to the symbol
            '^used to create various types of bubbles with text inside
            Try
                If aBubbleType < dxxBubbleTypes.Circular Or aBubbleType > dxxBubbleTypes.Rectangular Then aBubbleType = dxxBubbleTypes.Pill
                aBubbleHt = Math.Abs(aBubbleHt)
                aBubbleLg = Math.Abs(aBubbleLg)
                If aBubbleLg <= 0 Then aBubbleLg = aBubbleHt
                Dim aImage As dxfImage = Nothing
                Dim aCS As dxfPlane = Nothing
                Dim verts As TVECTORS
                'initialize

                _rVal = xBasicSymbol(aImage, dxxSymbolTypes.Bubble, aTextHeight, aName, bScaleToScreen, aArrowSize, aTextStyle, aArrowHead)
                verts = aImage.CreateUCSVectors(New List(Of iVector)({New dxfVector(aInsertPtXY)}), bSuppressUCS, bSuppressElevation, aCS)
                aBubbleLg = Math.Abs(aBubbleLg)
                If aBubbleLg > 0 And bScaleToScreen Then
                    aBubbleLg *= aImage.obj_DISPLAY.PaperScale
                End If
                _rVal.PlaneV = New TPLANE(aCS)
                _rVal.BubbleType = aBubbleType
                _rVal.VectorsV = verts
                If bubbleText IsNot Nothing Then _rVal.Text1 = bubbleText.ToString().Trim
                If aHexText IsNot Nothing Then _rVal.Text2 = aHexText.ToString.Trim()
                If aTrailerText IsNot Nothing Then _rVal.Text3 = aTrailerText.ToString.Trim()
                _rVal.Length = aBubbleLg
                _rVal.Height = aBubbleHt
                _rVal.Rotation = TVALUES.NormAng(aBubbleAngle, False, True)
                _rVal.IsDirty = True
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function

        Public Function CreateSymbol_Bubble(aBubbleType As dxxBubbleTypes, aLeaderPtsXY As IEnumerable(Of iVector), bubbleText As String, aBubbleHt As Double, aBubbleLg As Double, Optional aBubbleAngle As Double = 0.0, Optional aHexText As String = Nothing, Optional aTrailerText As String = Nothing, Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            '#1type of bubble to create
            '#2the center for the primary bubble
            '#3text string to put inside the primary bubble
            '#4the height for the bubble
            '#5the length of the bubble
            '#6the angle for the bubble
            '#7a text string to add after the bubble in a hexagon
            '#8a text string to add after the bubble and the hexagon
            '#9a text height to use instead of the current symbol text height setting
            '#10flag to resize the symbol every time the display is resized
            '#11flag to scale teh symbol to the current display
            '#12flag to save the symbol as a block insert when the file is saved
            '#13flag to move the insertion point of the symbol to the leading end of the symbol entities
            '#14the name to assign to the symbol
            '^used to create various types of bubbles with text inside
            Try
                If aBubbleType < dxxBubbleTypes.Circular Or aBubbleType > dxxBubbleTypes.Rectangular Then aBubbleType = dxxBubbleTypes.Pill
                aBubbleHt = Math.Abs(aBubbleHt)
                aBubbleLg = Math.Abs(aBubbleLg)
                If aBubbleLg <= 0 Then aBubbleLg = aBubbleHt
                Dim aImage As dxfImage = Nothing
                Dim aCS As dxfPlane = Nothing
                Dim verts As TVECTORS
                'initialize
                _rVal = xBasicSymbol(aImage, dxxSymbolTypes.Bubble, aTextHeight, aName, bScaleToScreen, aArrowSize, aTextStyle, aArrowHead)
                verts = aImage.CreateUCSVectors(aLeaderPtsXY, bSuppressUCS, bSuppressElevation, aCS)
                aBubbleLg = Math.Abs(aBubbleLg)
                If aBubbleLg > 0 And bScaleToScreen Then
                    aBubbleLg *= aImage.obj_DISPLAY.PaperScale
                End If
                _rVal.PlaneV = New TPLANE(aCS)
                _rVal.BubbleType = aBubbleType
                _rVal.VectorsV = verts
                If bubbleText IsNot Nothing Then _rVal.Text1 = bubbleText.ToString().Trim
                If aHexText IsNot Nothing Then _rVal.Text2 = aHexText.ToString.Trim()
                If aTrailerText IsNot Nothing Then _rVal.Text3 = aTrailerText.ToString.Trim()
                _rVal.Length = aBubbleLg
                _rVal.Height = aBubbleHt
                _rVal.Rotation = TVALUES.NormAng(aBubbleAngle, False, True)
                _rVal.IsDirty = True
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Function CreateSymbol_DetailBubble(aBubblePointXY As iVector, aRadius As Double, TextString As Object, aLeaderLength As Double, aLeaderAngle As Double, Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aLineType As String = "Continuous", Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            '#1the point where the detail bubble should be centered
            '#2the radius of the bubble circle
            '#3the string to attach to the bubble
            '#4the length of the leader to attach to the bubble
            '#5the angle to place the leader
            '#6the text height to use for the bubble other the curren symbol text size
            '#7flag to scale the entities to the current display
            '#8the linetype for the circle and leader
            '#9flag to save the symbol as a block insert when the file is written
            '#10the name to assign to the bubble
            '^used to draw a Circle centered at the passed point with a leader draw from the circle edge to the passed leader point.
            '~the text is placed at the end of the leader.
            Try
                If aRadius = 0 Then Throw New Exception("Invalid Radius Detected")
                Dim P1 As dxfVector
                TextString = Trim(TextString)
                'initialize
                _rVal = xBasicSymbol(aImage, dxxSymbolTypes.DetailBubble, aTextHeight, aName, bScaleToScreen, aArrowSize, aTextStyle, aArrowHead)
                aRadius = Math.Abs(aRadius)
                If aRadius = 0 Then aRadius = 1
                aLeaderLength = Math.Abs(aLeaderLength)
                If aLeaderLength = 0 Then aLeaderLength = 1
                P1 = aImage.CreateVector(aBubblePointXY, bSuppressUCS, bSuppressElevation)
                _rVal.InsertionPt.MoveTo(P1)
                _rVal.Text1 = Trim(TextString)
                _rVal.Height = aRadius
                _rVal.Length = aLeaderLength
                _rVal.Rotation = TVALUES.NormAng(aLeaderAngle, False, True)
                _rVal.Linetype = aLineType
                _rVal.IsDirty = True
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function CreateSymbol_SectionArrow(aVerticesXY As IEnumerable(Of iVector), Optional aLegLength As Double = 0.5, Optional aLabel As String = "", Optional aAngle As Double = 0.0, Optional aTextHeight As Double = 0.0, Optional aTextAligment As dxxRectangularAlignments = dxxRectangularAlignments.General, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional bScaleToScreen As Boolean = False, Optional aLineType As String = "", Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            '#1the vertices of the section line
            '#2the length of the desired arrows
            '#3the text to place above the corners of the section arrow
            '#4the orientation angle of the arrow
            '#50the height of the text to apply instead of the current symbol text size setting
            '#6flag to align the text to the arrow
            '#7the type of section arrow to draw (1 or 2)
            '#8flag to scale the arrow the current display
            '#9the linetype for the section lines
            '#10if True the arrow is saved as as a block and an insert of the block is returned
            '#11the name to assign to the symbol
            '^used to create section arrows
            Dim aCS As dxfPlane = Nothing
            Dim aImage As dxfImage = Nothing
            Dim averts As TVECTORS
            Try
                If aVerticesXY Is Nothing Then Throw New Exception("Undefined Points Detected")
                averts = aImage.CreateUCSVectors(aVerticesXY, bSuppressUCS, bSuppressElevation, aCS)
                If averts.Count < 2 Then Throw New Exception("Undefined Points Detected")
                _rVal = xBasicSymbol(aImage, dxxSymbolTypes.Arrow, aTextHeight, aName, bScaleToScreen, aArrowSize, aTextStyle, aArrowHead)
                aLegLength = Math.Abs(aLegLength)
                If aLegLength = 0 Then aLegLength = 0.5
                If bScaleToScreen Then aLegLength *= aImage.obj_DISPLAY.PaperScale
                If aArrowStyle < 0 And (aArrowHead >= 0 And aArrowHead <= 19) Then
                    aArrowStyle = dxxArrowStyles.StdBlocks
                End If
                _rVal.PlaneV = New TPLANE(aCS)
                _rVal.ArrowType = dxxArrowTypes.Section
                _rVal.InsertionPtV = averts.PlanarCenter(aCS.Strukture)
                _rVal.ArrowTextAlignment = aTextAligment
                _rVal.Text1 = Trim(aLabel)
                If aLegLength < 0 Then aAngle += 180
                _rVal.Length = aLegLength
                _rVal.Rotation = TVALUES.NormAng(aAngle, False, True)
                _rVal.VectorsV = averts
                _rVal.ArrowStyle = aArrowStyle
                If aLineType <> "" Then _rVal.Linetype = aLineType
                _rVal.IsDirty = True
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Function CreateSymbol_ViewArrow(aArrowsPtXY As IEnumerable(Of iVector), aSpan As Double, Optional aLegLength As Double = 0.5, Optional aLabel As String = "", Optional aAngle As Double = 0.0, Optional aTextHeight As Double = 0.0, Optional aExtentionLength As Double = 0.0, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional bAlignText As Boolean? = Nothing, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            '#1the midpoint of the desired view arrow to draw (this is the center of the passed pan)
            '#2the span of the desired arrow
            '#3the length of the desired arrows
            '#4the text to place above the corners of the view arrow
            '#5the orientation angle of the arrow
            '#6the height of the text to apply instead of the current symbol text size setting
            '#7the type of view arrow to draw (0, 1 or 2)
            '#8flag to align the text to the arrow
            '#9flag to scale the arrow to the current display
            '#10if True the arrow is saved as as a block when the file is saved to disk
            '#11the name to assign to the arrow
            '^used to create view arrows
            aSpan = Math.Abs(aSpan)
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return Nothing
            Dim aCS As dxfPlane = Nothing
            Try
                If aArrowStyle < 0 And (aArrowHead >= 0 And aArrowHead <= 19) Then
                    aArrowStyle = dxxArrowStyles.StdBlocks
                End If
                Dim aCenters As TVERTICES = aImage.CreateUCSVertices(aArrowsPtXY, bSuppressUCS, bSuppressElevation, aCS)
                'initialize
                _rVal = xBasicSymbol(aImage, dxxSymbolTypes.Arrow, aTextHeight, aName, bScaleToScreen, aArrowSize, aTextStyle, aArrowHead)
                aLegLength = Math.Abs(aLegLength)
                If aLegLength = 0 Then aLegLength = 1
                _rVal.PlaneV = New TPLANE(aCS)
                If bScaleToScreen Then
                    _rVal.FeatureScale = aImage.obj_DISPLAY.PaperScale
                Else
                    _rVal.FeatureScale = aImage.BaseSettings(dxxSettingTypes.SYMBOLSETTINGS).Props.ValueD("FeatureScale")
                End If
                _rVal.Height = Math.Abs(aExtentionLength)
                _rVal.ArrowType = dxxArrowTypes.View
                _rVal.InsertionPtV = aCenters.Vector(1)
                _rVal.Text1 = Trim(aLabel)
                _rVal.Length = aLegLength
                _rVal.Span = aSpan
                _rVal.Rotation = aAngle
                _rVal.Length = aLegLength
                _rVal.ArrowStyle = aArrowStyle
                If aCenters.Count > 1 Then
                    _rVal.Instances.FromVertices(aCenters, _rVal.PlaneV, 1, 0)
                End If
                _rVal.IsDirty = True
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Function CreateSymbol_Weld(aLeaderPtsXY As IEnumerable(Of iVector), ByRef WeldType As dxxWeldTypes, Optional aTextHeight As Double = 0.0, Optional bBothSides As Boolean = False, Optional bAllAround As Boolean = False, Optional aSide1Dims As String = Nothing, Optional aSide2Dims As String = Nothing, Optional aNoteText As String = Nothing, Optional bSuppressTail As Boolean = False, Optional aAngle As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            '#1point to insert the weld symbol
            '#2the type of weld symbol being requested
            '#3the height of the symbol text
            '#4flag to indicate if the weld is on both sides of the joint
            '#5flag to indicate if the weld is to continue all the way around the joint
            '#6the text string to place above the weld symbol line
            '#7the text string to place below the weld symbol line
            '#8additional text drawn after the weld symbol
            '#9flag to toggle the drawing of a tail on the weld symbol
            '#10the angle for the symbol baseline
            '#11flag to scale the symbol to the current display
            '#12the name for the symbol
            '#13the text style name to use for the symbol
            '^used to draw Weld Symbols
            '~weld symbols are placed on the symbol layer if the symbol layer color is defined(see dxoDrawingTool.SymbolSettings.DefaultLayer)
            'intialize
            Dim aImage As dxfImage = Nothing
            Dim verts As TVECTORS
            Dim aCS As dxfPlane = Nothing
            Try
                'initialize
                If Not GetImage(aImage) Then Throw New Exception("Undefined Image Detected")
                _rVal = xBasicSymbol(aImage, dxxSymbolTypes.Weld, aTextHeight, aName, bScaleToScreen, aArrowSize, aTextStyle, aArrowHead)
                verts = aImage.CreateUCSVectors(aLeaderPtsXY, bSuppressUCS, bSuppressElevation, aCS)
                _rVal.PlaneV = New TPLANE(aCS)
                _rVal.WeldType = WeldType
                _rVal.VectorsV = verts
                _rVal.Flag1 = bBothSides
                _rVal.Flag2 = bAllAround
                _rVal.Flag3 = bSuppressTail
                If aSide1Dims IsNot Nothing Then _rVal.Text1 = aSide1Dims.Trim()
                If aSide2Dims IsNot Nothing Then _rVal.Text2 = aSide2Dims.Trim()
                If aNoteText IsNot Nothing Then _rVal.Text3 = aNoteText.Trim()
                _rVal.Rotation = TVALUES.NormAng(aAngle, False, True)
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Public Function Create_Hatch_ByFillStyle(aFillStyle As dxxFillStyles, aEntity As dxfEntity, Optional aRotation As Double = 0.0, Optional aLineSpace As Double = 1, Optional aScaleFactor As Double = 0.0, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional LastEntType As dxxEntityTypes = dxxEntityTypes.Undefined) As dxeHatch
            Dim _rVal As dxeHatch = Nothing
            Dim myImage As dxfImage = zMyImage
            If myImage Is Nothing Then Return _rVal
            '#1the entity to hatch
            '#2the angle of the hatch lines
            '#3the spacing between the hatch lines
            '#4a scale factor to apply to the hatch (0 means scale to current display)
            '#5flag indicating if the hatch is a solid
            '#6flag indicating if the hatch is a crossing pattern (a net)
            '#7a layer to put the entity on instead of the current layer
            '#8a color to apply to the entity instead of the current color setting
            '#9a line type to apply to the entity instead of the current line type setting
            '^used to add a hatch to the last drawn entity or a passed entity.
            '~if the entity is passed as nothing an attempt is made to hatch the last draw entity.
            '~if the entity does not have a defineable hatch region nothing is drawn
            Try
                Dim aEnt As dxfEntity

                If aEntity Is Nothing Then aEnt = myImage.Entities.LastEntity(LastEntType) Else aEnt = aEntity
                If aEnt Is Nothing Then Throw New Exception("No Hatchable Entity Found")

                If Not dxfEnums.Validate(GetType(dxxFillStyles), TVALUES.To_INT(aFillStyle), bSkipNegatives:=True) Then
                    Throw New Exception("Invalid Fill Style Passed")
                End If

                If aLineSpace <= 0 Then
                    aEnt.UpdatePath()
                    aLineSpace = 0.01 * aEnt.BoundingRectangle.Width
                End If
                If aLineSpace <= 0 Then Throw New Exception("Invalid Line Step Passed")
                aEnt.UpdatePath(aImage:=myImage)
                Dim bCrvs As TBOUNDLOOPS = dxfHatches.BoundLoops_Entity(aEnt, aEnt.Plane.Strukture)
                If bCrvs.Count <= 0 Then Throw New Exception("Un-Hatchable Enity Passed")
                If aScaleFactor <= 0 Then
                    aScaleFactor = myImage.obj_DISPLAY.PaperScale
                End If
                If aScaleFactor <= 0 Then aScaleFactor = 1

                _rVal = dxfPrimatives.Create_Hatch_ByFillStyle(aFillStyle, aEnt, aRotation, aLineSpace, aScaleFactor, New dxfDisplaySettings(dxfImageTool.DisplayStructure(myImage, aLayerName, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)))


                Return _rVal
            Catch ex As Exception
                myImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function Create_Hatch_UserDefined(aEntity As dxfEntity, Optional sLineAngle As Double = 45, Optional aLineSpace As Double = 1, Optional aScaleFactor As Double = 0.0, Optional bDoubled As Boolean = False, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional LastEntType As dxxEntityTypes = dxxEntityTypes.Undefined) As dxeHatch
            Dim _rVal As dxeHatch = Nothing
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            '#1the entity to hatch
            '#2the angle of the hatch lines
            '#3the spacing between the hatch lines
            '#4a scale factor to apply to the hatch (0 means scale to current display)
            '#5flag indicating if the hatch is a crossing pattern (a net)
            '#6a layer to put the entity on instead of the current layer
            '#7a color to apply to the entity instead of the current color setting
            '#8a line type to apply to the entity instead of the current line type setting
            '^used to add a hatch to the last drawn entity or a passed entity.
            '~if the entity is passed as nothing an attempt is made to hatch the last draw entity.
            '~if the entity does not have a defineable hatch region nothing is drawn
            Try
                Dim aEnt As dxfEntity

                If aEntity Is Nothing Then aEnt = aImage.Entities.LastEntity(LastEntType) Else aEnt = aEntity
                If aEnt Is Nothing Then Throw New Exception("No Hatchable Entity Found")
                aEnt.UpdatePath(False, aImage)
                Dim bCrvs As TBOUNDLOOPS = dxfHatches.BoundLoops_Entity(aEnt, aEnt.Plane.Strukture)
                If bCrvs.Count <= 0 Then Throw New Exception("No Hatchable Entity Found")
                'intitialize
                If aScaleFactor <= 0 Then aScaleFactor = aImage.obj_DISPLAY.PaperScale
                If aScaleFactor <= 0 Then aScaleFactor = 1
                If aLineSpace <= 0 Then aLineSpace = 1
                _rVal = New dxeHatch With {
                    .PlaneV = aImage.obj_UCS,
                    .DisplayStructure = dxfImageTool.DisplayStructure(aImage, aLayerName, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined),
                    .HatchStyle = dxxHatchStyle.dxfHatchUserDefined,
                    .Rotation = sLineAngle,
                    .LineStep = aLineSpace * aScaleFactor,
                    .Doubled = bDoubled
                }
                _rVal.BoundingEntities.Add(aEnt.CloneAll(aImage))
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function Create_Hole(aCenterXY As iVector, aHoleType As dxxEntityTypes, aDiameterOrHeight As Double, Optional aLength As Double = 0.0, Optional aRotation As Double = 0.0, Optional IsSquare As Boolean = False, Optional aMinorDia As Double = 0.0, Optional aPlane As dxxStandardPlanes = dxxStandardPlanes.XY, Optional CLineScaleFactorV As Double = 1, Optional CLineScaleFactorH As Double = 1, Optional aExtrusionDirection As dxfDirection = Nothing, Optional aName As String = "", Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "") As dxeHole
            Dim _rVal As dxeHole = Nothing
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            '#1the center of the new hole
            '#2the hole type to Draw
            '#3the diameter of the hole or height of a slot
            '#4the length for a slot
            '#5a angle for the hole
            '#6flag indicating if the hole is square
            '#7minor dia for D shaped holes
            '#8the plane the hole lies on
            '#8a scale factor for the holes vertical centerline
            '#9a scale factor for the holes horizontal centerline
            '#10the normal direction of the plane that the hole lies on (this overrides the plane argument)
            '#11the name to assign to the hole
            '#12the layer to put the entity on instead of the current layer setting
            '#13a color to apply to the entity instead of the current color setting
            '#14a linetype to apply to the entity instead of the current linetype setting
            '^used to create holes and slots
            Try
                If aDiameterOrHeight <= 0 Then
                    If aHoleType <> dxxEntityTypes.Hole Then Throw New Exception("Invalid Hole Diameter Passed") Else Throw New Exception("Invalid Slot Height Passed")
                End If
                If aHoleType <> dxxEntityTypes.Hole And aHoleType <> dxxEntityTypes.Slot Then aHoleType = dxxEntityTypes.Hole
                '     If aLength < aDiameterOrHeight Then aLength = aDiameterOrHeight
                Dim ctr As TVECTOR
                Dim aOCS As dxfPlane
                Dim aDir As dxfDirection
                aOCS = aImage.UCS
                ctr = aImage.CreateUCSVector(aCenterXY, False)
                If aExtrusionDirection Is Nothing Then
                    If aPlane < 0 And aPlane > 2 Then aPlane = dxxStandardPlanes.XY
                    If aPlane = dxxStandardPlanes.XY Then
                        aDir = aOCS.ZDirection
                    ElseIf aPlane = dxxStandardPlanes.XZ Then
                        aDir = aOCS.YDirection
                    ElseIf aPlane = dxxStandardPlanes.YZ Then
                        aDir = aOCS.XDirection
                    End If
                Else
                    aDir = aExtrusionDirection.Clone
                End If
                _rVal = New dxeHole With {
                    .DisplayStructure = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined),
                    .CenterV = ctr,
                    .Diameter = aDiameterOrHeight,
                    .Length = aLength,
                    .MinorDiameter = aMinorDia,
                    .IsSquare = IsSquare,
                    .Rotation = aRotation
                    }
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function Create_HoleCenterLines(aHoles As colDXFEntities, Optional aScaleFactor As Double = 1.2, Optional bSupressHorizontal As Boolean = False, Optional bSupressVertical As Boolean = False, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            '#1the holes to create centerlines for
            '#2the scale factor to apply to the centerlines
            '#3flag to suppress the creation of the horizontal centerlines
            '#4flag to suppress the creation of the vertical centerlines
            '#5the layer to apply
            '#6the color to apply
            '#7the linetype to apply
            '#8the plane to use (the current drawing plane is used by default)
            '^draws centerlines for the passed holes on the indicated plane
            Try
                If aHoles Is Nothing Then Return _rVal
                If aHoles.Count <= 0 Then Return _rVal
                If bSupressHorizontal And bSupressVertical Then Return _rVal
                If aScaleFactor < 1 Then aScaleFactor = 1
                Dim i As Integer
                Dim aH As dxeHole
                Dim aL As dxeLine
                Dim bPl As New TPLANE("")
                Dim aDsp As New TDISPLAYVARS
                Dim aRec As New TPLANE("")
                Dim ext As Double
                Dim cp As TVECTOR
                Dim aEnt As dxfEntity
                'Dim aRectn As dxfRectangle
                If aLayerName Is Nothing Then aLayerName = String.Empty Else aLayerName = aLayerName.Trim()
                If aLineType Is Nothing Then aLineType = String.Empty Else aLineType = aLineType.Trim()
                If aLineType = String.Empty Then aLineType = dxfLinetypes.Center
                aDsp = dxfImageTool.DisplayStructure(aImage, aLayerName, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                If TPLANE.IsNull(aPlane) Then bPl = aImage.obj_DISPLAY.pln_VIEW Else bPl = New TPLANE(aPlane)
                For i = 1 To aHoles.Count
                    aEnt = aHoles.Item(i)
                    If aEnt.GraphicType = dxxGraphicTypes.Hole Then
                        aH = aEnt
                        cp = aH.CenterV.ProjectedTo(bPl)
                        bPl.Origin = cp
                        aRec = New TVECTORS(aH.ExtentPoints).Bounds(bPl)
                        If aRec.Width > aRec.Height Then
                            ext = (aScaleFactor - 1) * aRec.Height
                        Else
                            ext = (aScaleFactor - 1) * aRec.Width
                        End If
                        If Not bSupressHorizontal Then
                            aL = New dxeLine With {
                            .StartPtV = bPl.AngleVector(0, -(0.5 * aRec.Width + ext), False),
                            .EndPtV = bPl.AngleVector(0, (0.5 * aRec.Width + ext), False),
                            .Tag = aH.Tag,
                            .Flag = "Horizontal",
                            .Identifier = aH.Identifier,
                            .DisplayStructure = aDsp
                            }
                            _rVal.Add(aL)
                            aL.Instances.Copy(aH.Instances, True)
                            If aCollector IsNot Nothing Then aCollector.Add(aL, bAddClone:=True)
                        End If
                        If Not bSupressVertical Then
                            aL = New dxeLine With {
                            .StartPtV = bPl.AngleVector(90, -(0.5 * aRec.Height + ext), False),
                            .EndPtV = bPl.AngleVector(90, 0.5 * aRec.Height + ext, False),
                            .Tag = aH.Tag,
                            .Flag = "Vertical",
                            .Identifier = aH.Identifier,
                           .DisplayStructure = aDsp}

                            aL.Instances.Copy(aH.Instances, True)
                            _rVal.Add(aL)
                            If aCollector IsNot Nothing Then aCollector.Add(aL, bAddClone:=True)
                        End If
                        'Set aRectn = New dxfRectangle
                        'aRectn.Structure = bPl
                        'aRectn.Height = aRec.Height
                        'aRectn.Width = aRec.Width
                        '   Create_HoleCenterLines.Add aRectn.Perimeter
                    End If
                Next i
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function Create_Insert(aImage As dxfImage, aBlockName As String, aInsertionPts As IEnumerable(Of iVector), Optional aRotationAngle As Double? = Nothing, Optional aScaleFactor As Double = 1, Optional aAttributeVals As dxfAttributes = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeInsert
            Dim _rVal As dxeInsert = Nothing
            If Not GetImage(aImage) Then Return Nothing
            '[]Me~Using aImage.Blocks~HelpDocs\UsingBlocks.htm~File
            '#1the name of the block to insert into the drawing
            '#2the point or points to insert the block
            '#3the angle to insert the block
            '#4the scale factor to apply
            '#5acollection of attribute values to apply to the attributes in the block
            '#6the layer to assign to the insert entity other than the current layer name
            '#7the color to assign to the insert entity other than the current color
            '#8the linetype to assign to the insert entity other than the current linetype
            '#9the Y scale to apply (if not passed then primary scale factor is assumed)
            '#10the Z scale to apply (if not passed then primary scale factor is assumed)
            '^used to insert defined blocks into the current drawing.
            '~the inserted block must have been created through code add added to the dxoDrawingTool.Blocks collection
            '~prior to insertion.
            '^attribute strings are tag $delimitor$ value
            '~if the second argument is a collection of points (colDXFVectors or Collection) then the requested block is inserted at all
            '~of the passed points otherwise the block is inserted at the single passed loaction with the applied row and column info
            Dim Block As dxfBlock
            Dim iPt As dxfVector
            Dim sclY As Double
            Dim sclZ As Double
            Dim aOCS As dxfPlane = Nothing
            Dim bAngPassed As Boolean = aRotationAngle.HasValue
            Dim pAng As Double
            If aDisplaySettings Is Nothing Then aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.Insert, "", "", "", dxxLinetypeLayerFlag.Undefined, False, "")
            Try
                Block = aImage.Blocks.GetByName(aBlockName, True)
                If Block Is Nothing Then Throw New Exception($"Block '{ aBlockName }' Was Not Found")
                Dim vPts As TVERTICES = aImage.CreateUCSVertices(aInsertionPts, False, False, aOCS, False, True)
                If vPts.Count <= 0 Then Return _rVal

                aScaleFactor = Math.Round(aScaleFactor, 8)
                If aScaleFactor = 0 Then aScaleFactor = 1
                If Block.UniformScale Then
                    sclY = aScaleFactor
                    sclZ = aScaleFactor
                Else
                    If aYScale.HasValue Then sclY = Math.Round(aYScale.Value, 8) Else sclY = aScaleFactor
                    If sclY = 0 Then sclY = aScaleFactor
                    If aZScale.HasValue Then sclZ = Math.Round(aZScale.Value, 8) Else sclZ = aScaleFactor
                    If sclZ = 0 Then sclZ = aScaleFactor

                    sclZ = TVALUES.To_DBL(aZScale, aDefault:=aScaleFactor)
                End If
                If bAngPassed Then
                    pAng = aRotationAngle.Value
                End If
                iPt = vPts.Item(1)

                'set the return entity properties
                _rVal = New dxeInsert(Block, iPt) With {.SuppressEvents = True, .Plane = aOCS, .DisplayStructure = aDisplaySettings.Strukture}

                _rVal.SetAttributes(aAttributeVals)
                _rVal.InsertionPtV = iPt.Strukture
                If bAngPassed Then
                    _rVal.RotationAngle = pAng
                Else
                    _rVal.RotationAngle = iPt.Rotation
                End If
                If vPts.Count > 0 Then vPts.SetRotation(_rVal.RotationAngle, 1)
                iPt.Rotation = _rVal.RotationAngle
                _rVal.XScaleFactor = aScaleFactor
                _rVal.YScaleFactor = sclY
                _rVal.ZScaleFactor = sclZ
                _rVal.Instances.DefineWithVectors(iPt, vPts, False, bAngPassed, False, False)
                _rVal.SuppressEvents = False
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, aImage)
                Return _rVal
            End Try
        End Function
        Public Function Create_Insert(aBlockName As String, aInsertPT As iVector, Optional aRotationAngle As Double? = Nothing, Optional aScaleFactor As Double = 1, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aAttributeVals As dxfAttributes = Nothing, Optional aImage As dxfImage = Nothing, Optional bThrowErrors As Boolean = False, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "") As dxeInsert
            If Not GetImage(aImage) Then Return Nothing
            '[]Me~Using Blocks~HelpDocs\UsingBlocks.htm~File
            '#1the name of the block to insert into the drawing
            '#2the point or points to insert the block
            '#3the angle to insert the block
            '#4the scale factor to apply
            '#5the override layer
            '#6a collection of attribute values to aplt to the atttributes of the block
            '#7the column count to assign to create a rectangular array of
            '#8the column space to assign to the insert
            '#9the row count to assignt o the insert
            '#10the row space to assign to the insert
            '#11the layer to assign to the insert entity other than the current layer name
            '#12the color to assign to the insert entity other than the current color
            '#13the linetype to assign to the insert entity other than the current linetype
            '#14the Y scale to apply (if not passed then primary scale factor is assumed)
            '#15the Z scale to apply (if not passed then primary scale factor is assumed)
            '^used to insert defined blocks into the current drawing.
            '~the inserted block must have been created through code add added to the dxfImage.Blocks collection
            '~prior to insertion.
            '~if the second argument is a collection of points (colDXFVectors or Collection) then the requested block is inserted at all points
            '~of the passed points otherwise the block is inserted at the single passed loaction with the applied row and column info
            If aInsertPT Is Nothing Then aInsertPT = dxfVector.Zero

            Try
                If String.IsNullOrWhiteSpace(aBlockName) Then
                    If bThrowErrors Then Throw New Exception($"Null Block Name passed.") Else xHandleError(Reflection.MethodBase.GetCurrentMethod(), New Exception($"Null Block Name passed."), aImage)
                End If
                aBlockName = aBlockName.Trim()
                Dim block As dxfBlock = Nothing
                If Not aImage.Blocks.TryGet(aBlockName, block) Then
                    If bThrowErrors Then Throw New Exception($"Block '{aBlockName}'  was not found in Image'{aImage.Name}'") Else xHandleError(Reflection.MethodBase.GetCurrentMethod(), New Exception($"Block '{aBlockName}'  was not found in Image'{aImage.Name}'"), aImage)
                End If
                Dim plane As dxfPlane = Nothing
                Dim v1 As TVERTEX = aImage.CreateUCSVertex(aInsertPT, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, rPlane:=plane)
                Dim rot = 0
                If aRotationAngle.HasValue Then rot = aRotationAngle.Value
                If aScaleFactor = 0 Then aScaleFactor = 1
                Dim _rVal As New dxeInsert(block, New dxfVector(v1), aScaleFactor, rot)
                aDisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.Insert, aLTLFlag:=aLTLSetting, aSettingsToCopy:=aDisplaySettings)

                _rVal.CopyDisplayValues(aDisplaySettings)
                If aYScale.HasValue Then
                    If aYScale.Value <> 0 Then _rVal.YScaleFactor = aYScale.Value
                End If
                If aZScale.HasValue Then
                    If aZScale.Value <> 0 Then _rVal.ZScaleFactor = aZScale.Value
                End If

                _rVal.Plane = plane

                _rVal.Tag = aTag
                _rVal.Flag = aFlag

                If aAttributeVals IsNot Nothing Then
                    _rVal.Attributes.CopyValues(aAttributeVals)
                End If
                Return _rVal

            Catch ex As Exception
                If bThrowErrors Then Throw ex Else xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, aImage)
                Return Nothing
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to creates inserts of  defined blocks into the parent drawing.
        ''' </summary>
        ''' <remarks>
        ''' the inserted block must have been created through code add added to the dxfImage.Blocks collection prior to insertion.
        '''If the block name is not found in the drawing blocks then nothing is added and no errors are thrown.
        '''If the rottion angle is not passed, the rotation value of the insertion points is used.
        '''</remarks>
        ''' <param name="aBlockName">the name of the block to insert into the drawing</param>
        ''' <param name="aInsertPTs">the points to insert the block</param>
        ''' <param name="aRotationAngle">the angle to insert the block</param>
        ''' <param name="aScaleFactor">the X scale factor to apply</param>
        ''' <param name="aLayerName">the layer to assign to the insert entity other than the current layer name</param>
        ''' <param name="aAttributeVals"> attribute values to pass to copy to the new inserts</param>
        ''' <param name="aColor">the color to assign to the insert entity other than the current color</param>
        ''' <param name="aLineType">the linetype to assign to the insert entity other than the current linetype name</param>
        ''' <param name="aYScale">the Y scale factor to apply. If not paased the X scale is assumed</param>
        ''' <param name="aZScale">the Z scale factor to apply. If not paased the X scale is assumed</param>
        ''' <param name="aImage">the parent image</param>
        ''' <param name="bThrowErrors">if the any errors will be raised otherwise they are passd to the parent image to handle</param>
        ''' <param name="aTag">an optional tag to assign to the new entities  </param>
        ''' <param name="bUseInsertionPtRotations">flag to force the use of the insertion pts rotation properties for the new inserts</param>
        ''' <param name="aLTLSetting"></param>
        ''' <returns></returns>
        Public Function Create_Inserts(aBlockName As String, aInsertPTs As IEnumerable(Of iVector), Optional aRotationAngle As Double? = Nothing, Optional aScaleFactor As Double = 1, Optional aLayerName As String = "", Optional aAttributeVals As dxfAttributes = Nothing, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aImage As dxfImage = Nothing, Optional bThrowErrors As Boolean = False, Optional bUseInsertionPtRotations As Boolean? = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "") As List(Of dxeInsert)
            Dim _rVal As New List(Of dxeInsert)
            If aInsertPTs Is Nothing Or Not GetImage(aImage) Then Return _rVal
            Try
                Dim rot As Double
                Dim usevrots = Not aRotationAngle.HasValue
                If Not usevrots Then rot = aRotationAngle.Value
                Dim isert As dxeInsert
                Dim dsp As dxfDisplaySettings = dxfDisplaySettings.Null(aLayerName, aColor, aLineType)
                For Each ip As iVector In aInsertPTs
                    If ip Is Nothing Then Continue For
                    Dim v As dxfVector = dxfVector.FromIVector(ip)
                    If bUseInsertionPtRotations.HasValue Then
                        rot = v.Rotation
                        If aRotationAngle.HasValue Then rot += aRotationAngle.Value
                    Else

                        If usevrots Then
                            rot = v.Rotation
                        End If
                    End If

                    isert = Create_Insert(aBlockName, v, aRotationAngle:=rot, aScaleFactor:=aScaleFactor, aAttributeVals:=aAttributeVals, aYScale:=aYScale, aZScale:=aZScale, aDisplaySettings:=dsp, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, aTag:=aTag, aFlag:=aFlag)
                    _rVal.Add(isert)
                Next
                Return _rVal
            Catch ex As Exception
                If bThrowErrors Then Throw ex Else xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, aImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to insert a new block insert into the drawing created from the passed polygon at the passed insertion points.
        ''' </summary>
        ''' <remarks>If the polygon is undefined nothing is added and no errors are thrown.
        ''' If the requested block name already exists in the drawing blocks then existing block is used and the polygon block is discarded.
        ''' 
        ''' </remarks>
        ''' <param name="aPolygon">a polygon to convert to a block and as a block</param>
        ''' <param name="aInsertPTs">the points to insert the block</param>
        ''' <param name="aRotationAngle">an optional angle to insert the block. If an angle is not passed the rotation value of the insertion points is used for each insertion.</param>
        ''' <param name="aBlockName">an optional name to assign to the new block. If not passed the polygons current block name is applied.</param>
        ''' <param name="aScaleFactor">the scale factor to apply</param>
        ''' <param name="bIncludesSubEntityInstances">a flag to suppress the instances of the polygons subentities</param>
        ''' <param name="aDisplaySettings"></param>
        ''' <param name="aLTLSetting">override display settings to apply to the new insert</param>
        ''' <param name="aYScale">a Y scale factor to apply</param>
        ''' <param name="aZScale">a Z scale factor to apply</param>
        ''' <param name="aImage">the target image</param>
        ''' <param name="bThrowErrors"></param>
        ''' <returns></returns>
        Public Function Create_Inserts(aPolygon As dxePolygon, aInsertPTs As IEnumerable(Of iVector), Optional aRotationAngle As Double? = Nothing, Optional aBlockName As String = Nothing, Optional aScaleFactor As Double = 1, Optional bIncludesSubEntityInstances As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aImage As dxfImage = Nothing, Optional bThrowErrors As Boolean = False, Optional bSuppressInstances As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As List(Of dxeInsert)
            Dim _rVal As New List(Of dxeInsert)
            If aPolygon Is Nothing Or Not GetImage(aImage) Then Return _rVal
            If aInsertPTs Is Nothing Then
                Dim ipts As New List(Of dxfVector)
                ipts.Add(New dxfVector(aPolygon.InsertionPt))
                aInsertPTs = ipts
            End If
            If aInsertPTs.Count <= 0 Then
                Return _rVal
            End If

            Try
                Dim rot As Double
                Dim usevrots = Not aRotationAngle.HasValue
                If Not usevrots Then rot = aRotationAngle.Value
                Dim instances As dxoInstances = Nothing
                Dim r1 As Double
                Dim primary As dxeInsert = Nothing
                Dim plane As dxfPlane = Nothing
                Dim insertionPoints As TVERTICES = aImage.CreateUCSVertices(aInsertPTs, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, rPlane:=plane, bErrorOnEmpty:=False, bReturnEmpty:=True)
                For i As Integer = 1 To insertionPoints.Count
                    Dim isert As dxeInsert = Nothing
                    Dim ip As New dxfVector(insertionPoints.Item(i))

                    If usevrots Then rot = ip.Rotation
                    If i = 1 Then
                        r1 = rot
                        isert = Create_Insert(aPolygon, ip, rot, aBlockName, aScaleFactor, bIncludesSubEntityInstances, aDisplaySettings, aLTLSetting, aYScale, aZScale, bSuppressUCS:=True, bSuppressElevation:=True)
                        If isert IsNot Nothing Then
                            aBlockName = isert.BlockName
                            aDisplaySettings = isert.DisplaySettings
                            instances = isert.Instances
                            primary = isert
                            _rVal.Add(isert)
                        End If
                    Else
                        If bSuppressInstances Then
                            isert = Create_Insert(aBlockName, ip, rot, aScaleFactor, aDisplaySettings, aYScale, aZScale, bSuppressUCS:=True, bSuppressElevation:=True)
                            If isert IsNot Nothing Then _rVal.Add(isert)
                        Else

                            instances.Add(ip.X - primary.X, ip.Y - primary.Y, aRotation:=r1 - rot)
                        End If

                    End If

                Next i

                If Not bSuppressInstances And primary IsNot Nothing And instances.Count > 0 Then
                    primary.Instances = instances
                End If
                Return _rVal
            Catch ex As Exception
                If bThrowErrors Then Throw ex Else xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, aImage)
            End Try
            Return Nothing
        End Function
        Public Function Create_Insert(aPolygon As dxePolygon, aInsertPT As iVector, Optional aRotationAngle As Double? = Nothing, Optional aBlockName As String = Nothing, Optional aScaleFactor As Double = 1, Optional bIncludesSubEntityInstances As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aImage As dxfImage = Nothing, Optional bThrowErrors As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeInsert

            If aPolygon Is Nothing Or Not GetImage(aImage) Then Return Nothing
            '[]Me~Using Blocks~HelpDocs\UsingBlocks.htm~File
            '#1a polygon to convert to a block and as a block
            '#2the point or points to insert the block
            '#3the angle to insert the block
            '#4an optional name to assign to the new block
            '#5the scale factor to apply
            '#6 a flag to suppress the instances of the polygons subentities
            '#7override display settings to apply to the new insert
            '#8a Y scale factor to apply
            '#9a Z scale factor to apply
            '^used to insert a new block into the drawing created from the passed polygon.
            '~If the polygon is undefined nothing is added and no errors are thrown.
            '~If the requested block name already exists in the drawing blocks then existing block is used and the polygon block is discarded.
            '~if the second argument is a collection of points (colDXFVectors or Collection) then the requested block is inserted at all points
            '~of the passed points otherwise the block is inserted at the single passed location
            Try
                Dim bname As String = dxfUtils.ThisOrThat(aBlockName, aPolygon.BlockName).Trim

                If bname = "" Then Return Nothing
                If aDisplaySettings IsNot Nothing Then
                    aDisplaySettings = New dxfDisplaySettings(dxfImageTool.DisplayStructure(aImage, aDisplaySettings.LayerName, aDisplaySettings.Color, aDisplaySettings.Linetype))
                Else
                    aDisplaySettings = New dxfDisplaySettings(aPolygon)
                End If
                dxfUtils.ValidateBlockName(bname, bFixIt:=True, aBlocks:=aImage.Blocks)

                Dim pblock As dxfBlock = Nothing
                If Not aImage.Blocks.TryGet(bname, pblock) Then
                    'aImage.LinetypeLayers.ApplyTo(aPolygon, aSetting:=aLTLSetting, aImage:=aImage)
                    pblock = aPolygon.Block(bname, bIncludesSubEntityInstances, aImage:=aImage, aLTLSettings:=aLTLSetting, bCenterAtIP:=False, aLayerName:=aDisplaySettings.LayerName)
                    'aImage.LinetypeLayers.ApplyTo(pblock, aSetting:=aLTLSetting, aImage:=aImage)
                    pblock = aImage.Blocks.Add(pblock)
                    bname = pblock.Name
                Else
                    'System.Diagnostics.Debug.WriteLine("Polygon Block '" & bname & "' Already Exists In Image:" & aImage.Name)
                End If
                If aInsertPT Is Nothing Then aInsertPT = New dxfVector(aPolygon.X, aPolygon.Y)
                Dim _rVal As dxeInsert = Create_Insert(aImage:=aImage, aBlockName:=bname, aInsertPT:=aInsertPT, aRotationAngle:=aRotationAngle, aScaleFactor:=aScaleFactor, aYScale:=aYScale, aZScale:=aZScale, aDisplaySettings:=aDisplaySettings, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation)
                _rVal.TFVCopy(aPolygon)
                Return _rVal
            Catch ex As Exception
                If bThrowErrors Then
                    Throw ex
                Else
                    xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, aImage)
                End If
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to create a polyline entity from the passed vertices.
        ''' </summary>
        ''' <remarks>
        ''' If the passed vertices are not defined then nothing is added and no errors are thrown.
        ''' If the segment width is not passed then the current image polyline width is used.
        ''' If the closed flag is set to true then the polyline will be closed.
        ''' </remarks>
        ''' <param name="aVerticesXY"></param>
        ''' <param name="bClosed">flag to indicate if the polyine should be closed</param>
        ''' <param name="aSegmentWidth">a segment width  for the polyline</param>
        ''' <param name="aLayer">the layer to put the entity on instead of the current layer setting</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color setting</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="bSuppressUCS">flag to suppress defining the vectors with respect to the current images current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress  the current elevation if the vectors will be defined with respect to the current images current UCS</param>
        ''' <returns></returns>
        Public Function Create_Polyline(aVerticesXY As IEnumerable(Of iVector), Optional bClosed As Boolean = False, Optional aSegmentWidth As Double? = Nothing, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxePolyline
            Dim _rVal As dxePolyline = Nothing
            Status = "Creating Polyline"
            Dim verts As TVERTICES
            Dim aOCS As dxfPlane = Nothing
            Dim aImage As dxfImage = Nothing
            Dim aWd As Double = -1
            aImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            Try
                If aSegmentWidth IsNot Nothing Then
                    If aSegmentWidth.HasValue Then
                        If aSegmentWidth.Value > 0 Then aWd = aSegmentWidth.Value
                    End If
                End If
                If aWd < 0 Then
                    aWd = aImage.Header.PolylineWidth
                End If
                If aVerticesXY Is Nothing Then Throw New Exception("Undefined Vertices Detected")
                verts = aImage.CreateUCSVertices(aVerticesXY, bSuppressUCS, bSuppressElevation, aOCS)
                '    If verts.Count < 2 Then Throw New Exception("Undefined Vertices Detected")
                If verts.Count < 3 Then bClosed = False
                'define the dxf entity properties
                _rVal = New dxePolyline(verts, bClosed, New dxfDisplaySettings(dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)), aOCS, aWd)
                Status = strLastStatus
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        ''' <summary>
        ''' used to create a polyline entity from the passed vertex coordinate string.
        ''' </summary>
        ''' <remarks>
        ''' If the passed vertex coordinate string is not defined then nothing is added and no errors are thrown.
        ''' If the segment width is not passed then the current image polyline width is used.
        ''' If the closed flag is set to true then the polyline will be closed.
        '''vertex coordinates are the 2D coordinates of the vector with respect to the subject plane augmented with the vertex radius of the vector
        '''e "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
        ''' </remarks>>
        ''' <param name="aVertexCoords">a string containing the vertext coordinates of th polyline vertices</param>
        ''' <param name="bClosed">flag to indicate if the polyline should be closed</param>
        ''' <param name="aPlane">the subject plane. If not paased the current Image UCS is assumed</param>
        ''' <param name="aSegmentWidth">a segment width for the polyline</param>
        ''' <param name="aDelimiter">the delimiter that divides the passed string into vector sets. The default delimiter is "¸" (char 184)</param>
        ''' <param name="aLayer">the layer to put the entity on instead of the current layer setting</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color setting</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="bSuppressUCS">flag to suppress defining the vectors with respect to the current images current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress  the current elevation if the vectors will be defined with respect to the current images current UCS</param>
        ''' <returns></returns>
        Public Function Create_PolylineByString(aVertexCoords As String, Optional bClosed As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aSegmentWidth As Double? = Nothing, Optional aDelimiter As String = "¸", Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxePolyline
            Dim _rVal As dxePolyline = Nothing
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal

            Try
                Status = "Creating Polyline"
                Dim verts As colDXFVectors
                Dim aWd As Double = -1
                If aSegmentWidth IsNot Nothing Then
                    If aSegmentWidth.HasValue Then
                        If aSegmentWidth.Value > 0 Then aWd = aSegmentWidth.Value
                    End If
                End If
                If aWd < 0 Then
                    aWd = aImage.Header.PolylineWidth
                End If
                If aWd = 0 Then aWd = -1
                'toget the plane
                aImage.CreateUCSVector(TVECTOR.Zero, bSuppressUCS, bSuppressElevation, aPlane)
                verts = New colDXFVectors
                verts.VertexCoordinatesSet(aVertexCoords, aPlane, aDelimiter:=aDelimiter)
                If verts.Count < 3 Then bClosed = False
                'define the dxf entity properties
                _rVal = New dxePolyline With
                {
                    .DisplayStructure = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined),
                    .PlaneV = New TPLANE(aPlane),
                    .Vertices = verts,
                    .Closed = bClosed,
                    .SegmentWidth = aWd
                }
                Status = strLastStatus
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            Finally
                Status = strLastStatus
            End Try
            Return _rVal
        End Function

        ''' <summary>
        ''' used to create a solid entity from the passed vertices.
        ''' </summary>
        ''' <remarks>
        ''' If the passed vertices are not defined an exception is thrown.
        ''' If the passed vertices count is less than 3 an exception is thrown.
        ''' If the layer, color or OCS are not passed then the current image settings are used.
        ''' </remarks>
        ''' <param name="PointsXY">a collection of Points that will be used as the vertices of the created solid</param>
        ''' <param name="aLayer">the layer to put the entity on instead of the current layer setting</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color setting</param>
        ''' <param name="aOCS"></param>
        ''' <returns></returns>
        Public Function Create_Solid(ByRef PointsXY As IEnumerable(Of iVector), Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aOCS As dxfPlane = Nothing) As dxeSolid

            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return Nothing
            Dim _rVal As dxeSolid = Nothing
            Try
                If PointsXY Is Nothing Then Throw New Exception("Invalid Vertices Passed")
                If PointsXY.Count < 3 Then Throw New Exception("Solids can only have 3 or 4 vertices")
                Dim bOCS As dxfPlane = Nothing
                Dim verts As TVECTORS = aImage.CreateUCSVectors(PointsXY, False, False, bOCS, aMaxReturn:=4)
                If dxfPlane.IsNull(aOCS) Then aOCS = bOCS
                Status = "Creating Solid"
                _rVal = New dxeSolid(verts, New dxfDisplaySettings(dxfImageTool.DisplayStructure(aImage, aLayer, aColor, dxfLinetypes.Continuous, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)), aOCS)
                Status = strLastStatus
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
            End Try
            Return _rVal
        End Function
        Public Function Create_Table(aName As String, aInsertionPt As Object, aCellData As List(Of String),
                                     Optional aCellAlignments As List(Of String) = Nothing, Optional aDelimiter As String = "|",
                                     Optional aTableAlign As dxxRectangularAlignments = dxxRectangularAlignments.General,
                                     Optional aGridStyle As dxxTableGridStyles = dxxTableGridStyles.Undefined,
                                     Optional bScaleToScreen As Boolean = False, Optional aHeaderRow As Double? = Nothing,
                                     Optional aHeaderCol As Double? = Nothing, Optional bSuppressBorders As Boolean = False,
                                     Optional aTitle As String = Nothing, Optional aFooter As String = Nothing,
                                     Optional aTextGap As Double = -1, Optional aColumnGap As Double = -1,
                                     Optional aRowGap As Double = -1, Optional bUseAttributes As Boolean = False,
                                     Optional aCellNames As List(Of String) = Nothing, Optional aCellPrompts As List(Of String) = Nothing,
                                      Optional aSaveAsBLock As Boolean = False) As dxeTable
            Dim _rVal As dxeTable = Nothing
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            '[]Me~Using Tables~HelpDocs\UsingTables.htm~File
            '#1the name to assign to the new table
            '#2the alignment point for the table
            '#3a collection of strings to define the table cell data
            '#4a collection of rectangular alignments to assign to the cells
            '#5 the delimiter that seperates the data in the passed collection
            '#6the alignment code for the table
            '#7the code that controls how grid lines are displayed in the table
            '#8flag to scale the table entities to the current display
            '#9the row to assign as the header row
            '#10the column to assign as the header column
            '#11 flag to suppress the drawing of exterior border
            '#12the title of the table
            '#13the footer string for the table
            '#13the gap to apply around table cells text as a fraction of a single character
            '#14the a length to add to the column widths
            '^used to create tables as groups
            '~the created table is assigned a group name and is treated as a group when saved
            Try
                Dim aOCS As dxfPlane = aImage.UCS
                Dim scalx As Double
                Dim aPlane As dxfPlane = Nothing
                Dim v1 As dxfVector
                Dim tablesettings As dxoSettingsTable = aImage.TableSettings
                Dim tablesets As TTABLEENTRY = tablesettings.Strukture
                Status = "Creating Table"
                'validate the name
                If String.IsNullOrWhiteSpace(aName) Then aName = "" Else aName = aName.Trim
                If aName <> "" Then dxfUtils.StringContainsCharacters(aName, dxfGlobals.BadBlockChars, False, True)
                If aName = "" Then aName = aImage.HandleGenerator.NextTableName(aImage.Blocks)
                If bScaleToScreen Then
                    scalx = aImage.obj_DISPLAY.PaperScale
                Else
                    scalx = tablesets.Props.ValueD("FeatureScale")
                End If
                If Not String.IsNullOrWhiteSpace(aDelimiter) Then aDelimiter = aDelimiter.Trim Else aDelimiter = "|"
                v1 = aImage.CreateVector(aInsertionPt, False, False, aPlane)
                _rVal = New dxeTable
                _rVal.Properties.CopyDisplayProperties(dxfImageTool.DisplayStructure_Table(aImage, tablesets))
                tablesets.Props.SetVal("FeatureScale", scalx)
                If aGridStyle >= 0 And aGridStyle <= 3 Then
                    tablesets.Props.SetVal("GridStyle", aGridStyle)
                End If
                If aTextGap >= 0 Then
                    If aTextGap > 6 Then aTextGap = 6
                    tablesets.Props.SetVal("TextGap", aTextGap)
                End If
                If aColumnGap >= 0 Then tablesets.Props.SetVal("ColumnGap", aColumnGap)
                If aRowGap >= 0 Then tablesets.Props.SetVal("RowGap", aRowGap)
                If aTableAlign >= dxxRectangularAlignments.TopLeft And aTableAlign <= dxxRectangularAlignments.BottomRight Then tablesets.Props.SetVal("Alignment", aTableAlign)
                tablesets.Props.SetVal("SuppressBorder", bSuppressBorders)
                tablesets.Props.SetVal("SaveAttributes", bUseAttributes)
                If aHeaderRow IsNot Nothing Then
                    If aHeaderRow.HasValue Then
                        tablesets.Props.SetVal("HeaderRow", aHeaderRow.Value)
                    End If
                End If
                If aHeaderCol IsNot Nothing Then
                    If aHeaderCol.HasValue Then
                        tablesets.Props.SetVal("HeaderCol", aHeaderCol.Value)
                    End If
                End If
                _rVal.Properties.CopyValues(tablesets.Props, bByName:=True)
                _rVal.Properties.SetVal("*NAME", aName)
                If aTitle IsNot Nothing Then _rVal.Properties.SetVal("Title", aTitle.ToString.Trim)
                If aFooter IsNot Nothing Then _rVal.Properties.SetVal("Footer", aFooter.ToString.Trim)
                _rVal.Properties.SetVal("Delimiter", aDelimiter)
                _rVal.PlaneV = New TPLANE(aPlane)
                _rVal.DefPts.SetVector(v1, 1)
                _rVal.SetCellData(aCellData, aDelimiter)
                _rVal.SetCellAlignments(aCellAlignments, aDelimiter)
                _rVal.SetCellNames(aCellNames, aDelimiter)
                _rVal.SetCellPrompts(aCellPrompts, aDelimiter)
                '.Settings = aSets
                _rVal.Name = aName
                _rVal.BlockName = aName
                _rVal.ImageGUID = aImage.GUID
                _rVal.InsertionPt = v1
                _rVal.SaveAsBlock = aSaveAsBLock
                Status = ""
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
            Finally
                Status = ""
            End Try
            Return _rVal
        End Function
        Public Function Create_Text(aAlignmentPt1XY As iVector, aString As Object, Optional aTextHeight As Double? = Nothing, Optional aAlignment As dxxMTextAlignments = dxxMTextAlignments.BaselineLeft,
                                    Optional aLayer As String = "", Optional aStyleName As String = "", Optional aColor As dxxColors = dxxColors.Undefined,
                                    Optional aAlignmentPt2XY As iVector = Nothing, Optional aWidthFactor As Double? = Nothing, Optional aTextAngle As Double? = Nothing, Optional aHeightFactor As Double = 0.0, Optional bSuppressEffects As Boolean = False, Optional aTextType As dxxTextTypes = dxxTextTypes.Multiline, Optional aOCS As dxfPlane = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aAttributeTag As String = "", Optional aAttributePrompt As String = "", Optional aAttributeType As dxxAttributeTypes = dxxAttributeTypes.None) As dxeText
            Dim _rVal As dxeText = Nothing
            '#1 the point where the text should be placed
            '#2a text height to apply which overrides the current dxoDrawingTool.TextHeight value
            '#3a text alignment to apply which overrides the current dxoDrawingTool.Alignment value
            '#4the string to create a text object for
            '#5a layer to put the text on
            '#6a text style to use
            '#7flag to indicate if the text should be drawn to the screen or just added to the aImage file.
            '#8a color to use rather that the current color
            '#9an overriding width factor to use
            '^used to create text
            '~creates and returns dxeText object.
            strLastStatus = strStatus
            Status = "Creating Text"
            Dim aImage As dxfImage = zMyImage

            If aImage Is Nothing Then Return Nothing
            Dim ap1 As dxfVector = aImage.CreateVector(aAlignmentPt1XY, bSuppressUCS, bSuppressElevation, aOCS)
            Dim ap2 As dxfVector

            Dim v1 As TVECTOR = ap1.Strukture
            Dim v2 As TVECTOR = ap1.Strukture

            If aAlignment = dxxMTextAlignments.Fit Or aAlignment = dxxMTextAlignments.Aligned Then
                If aAlignmentPt2XY Is Nothing Then
                    aAlignment = dxxMTextAlignments.BaselineLeft
                    ap2 = ap1.Clone
                Else
                    ap2 = aImage.CreateVector(aAlignmentPt2XY, bSuppressUCS, bSuppressElevation, aOCS)

                    If ap1.IsEqual(ap2) Then
                        v2 = v1
                        aAlignment = dxxMTextAlignments.BaselineLeft
                    End If
                End If
                v2 = ap2.Strukture
            End If
            'width factor
            Dim wf As Double = 1
            If aWidthFactor.HasValue Then wf = aWidthFactor.Value

            wf = TVALUES.ToDouble(wf, bAbsoluteVal:=True, aDefault:=1, aPrecis:=8, aMinVal:=0.01, aMaxVal:=100)
            Dim tang As Double = 0
            If aTextAngle.HasValue Then tang = aTextAngle.Value
            tang = TVALUES.ToAngle(tang, aDefault:=0, aPrecis:=5)
            Dim tht As Double = 0
            If aTextHeight.HasValue Then tht = aTextHeight.Value
            _rVal = zCreate_TextV(aImage, v1, aString, tht, aAlignment, aLayer, aStyleName, aColor, v2, wf, tang, aHeightFactor, bSuppressEffects, aTextType, aOCS.Strukture, aAttributeTag, aAttributePrompt, aAttributeType)
            Status = strLastStatus
            Return _rVal
        End Function
        Friend Function zCreate_TextV(aImage As dxfImage, aAlignPt1 As TVECTOR, aString As Object, aTextHeight As Double, aAlignment As dxxMTextAlignments, aLayer As String, aStyleName As String, aColor As dxxColors, aAlignPt2 As TVECTOR, aWidthFactor As Double, aTextAngle As Double, aHeightFactor As Double, bSuppressEffects As Boolean, aTextType As dxxTextTypes, aPlane As TPLANE, Optional aAttributeTag As String = "", Optional aAttributePrompt As String = "", Optional aAttributeType As dxxAttributeTypes = dxxAttributeTypes.None) As dxeText
            Dim _rVal As dxeText = Nothing
            '#1 the point where the text should be placed
            '#2a text height to apply which overrides the current dxoDrawingTool.TextHeight value
            '#3a text alignment to apply which overrides the current dxoDrawingTool.Alignment value
            '#4the string to create a text object for
            '#5a layer to put the text on
            '#6a text style to use
            '#7flag to indicate if the text should be drawn to the screen or just added to the aImage file.
            '#8a color to use rather that the current color
            '#9an overriding width factor to use
            '^used to create text
            '~creates and returns dxeText object.
            If aImage Is Nothing Then aImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            If aTextType < dxxTextTypes.DText Or aTextType > dxxTextTypes.Multiline Then aTextType = dxxTextTypes.Multiline
            Dim tStr As String = aString.ToString()
            Dim tht As Double = aTextHeight
            'display and style
            Try
                Dim dsp As TDISPLAYVARS = dxfImageTool.DisplayStructure_Text(aImage, aLayer, aColor:=aColor, aTextStyleName:=aStyleName)
                Dim aStyle As dxoStyle = aImage.TextStyle(dsp.TextStyle)
                'text height
                If tht <= 0 Then tht = aStyle.TextHeight
                If tht <= 0 Then tht = aImage.Header.TextSize
                If tht <= 0 Then tht = 0.05 * aImage.obj_DISPLAY.pln_VIEW.Height
                If aHeightFactor > 0 Then tht *= aHeightFactor
                'angle
                Dim tang As Double = TVALUES.NormAng(aTextAngle, ThreeSixtyEqZero:=True)
                'width factor
                Dim wf As Double = TVALUES.LimitedValue(aWidthFactor, 0.1, 100, aStyle.WidthFactor)
                'alignment
                If Not dxfEnums.Validate(GetType(dxxMTextAlignments), aAlignment, bSkipNegatives:=True) Then aAlignment = dxxMTextAlignments.BaselineLeft
                If aAlignment = dxxMTextAlignments.Fit Or aAlignment = dxxMTextAlignments.Aligned Then
                    If aAlignPt1.Equals(aAlignPt2, aPrecis:=3) Then aAlignment = dxxMTextAlignments.BaselineLeft
                End If
                'If tang <> 0 Then
                '    aPlane.Revolve(tang)
                '    tang = 0
                'End If
                'initialize the dtext object
                _rVal = New dxeText(aTextType, New dxfDisplaySettings(dsp)) With {
                    .ImageGUID = aImage.GUID,
                    .PlaneV = aPlane,
                    .TextString = tStr,
                    .AlignmentPt1V = aAlignPt1,
                    .AlignmentPt2V = aAlignPt2,
                    .Alignment = aAlignment,
                    .TextHeight = tht,
                    .Rotation = tang,
                    .WidthFactor = wf,
                    .TextStyleName = dsp.TextStyle
                }
                If _rVal.TextType = dxxTextTypes.AttDef Or _rVal.TextType = dxxTextTypes.Attribute Then
                    If _rVal.AttributeTag = "" Then _rVal.AttributeTag = aImage.HandleGenerator.NextAttributeTag
                    _rVal.AttributeTag = aAttributeTag.Trim().Replace(" ", "_")
                    _rVal.Prompt = aAttributePrompt
                    _rVal.AttributeType = aAttributeType
                End If
                If bSuppressEffects Then
                    _rVal.DrawingDirection = dxxTextDrawingDirections.Horizontal
                    _rVal.LineSpacingStyle = dxxLineSpacingStyles.AtLeast
                    _rVal.LineSpacingFactor = 1
                Else
                    _rVal.LineSpacingFactor = aStyle.LineSpacingFactor
                    _rVal.LineSpacingStyle = aStyle.LineSpacingStyle
                    _rVal.Vertical = aStyle.Vertical
                    _rVal.UpsideDown = aStyle.UpsideDown
                    _rVal.Backwards = aStyle.Backwards
                End If
                If _rVal.TextType <> dxxTextTypes.Multiline Then
                    _rVal.ObliqueAngle = aStyle.ObliqueAngle
                End If
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Public Function Create_Trace(ByRef VertsXY As Object, Optional aWidth As Double = -1, Optional bClosed As Boolean = False, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined) As colDXFEntities
            Dim _rVal As New colDXFEntities
            Dim aImage As dxfImage = zMyImage
            If aImage Is Nothing Then Return _rVal
            '#1a collection of Verts that will be used as the vertices of the trace
            '#2the width of the trace to create
            '#3flag to close the trace
            '#4the layer to put the entity on instead of the current layer setting
            '#5a color to apply to the entity instead of the current color setting
            '^used to draw a collection of Trace objects between the passed points
            '~a trace is a four vertex solid
            Dim vrt As TVERTEX
            Dim aPlane As TPLANE = aImage.obj_UCS
            Dim aDsp As New TDISPLAYVARS
            Dim aTrace As dxeSolid
            Dim aL As New TSEGMENT
            Dim aPth As New TPATH(dxxDrawingDomains.Model)
            Dim lPth As TPATH
            Dim lSegs As New List(Of TSEGMENT)
            Dim aVecs As New TVECTORS
            Dim aSegs As New TSEGMENTS
            Try
                If VertsXY Is Nothing Then Throw New Exception("Undefined Vertices Passed")
                Dim verts As TVERTICES = aImage.CreateUCSVertices(VertsXY)
                Status = "Creating Trace"
                'set the properties of the return solids
                verts.RemoveCoincident()
                If verts.Count <= 1 Then Throw New Exception("At Least Two Points Are Required To Create A Trace")
                If aWidth <= 0 Then aWidth = aImage.Header.TraceWidth
                If aWidth <= 0 Then aWidth = 0.05
                For i As Integer = 1 To verts.Count
                    vrt = verts.Item(i)
                    vrt.StartWidth = aWidth
                    vrt.EndWidth = aWidth
                    vrt.Radius = 0
                    verts.SetItem(i, vrt)
                Next i
                aDsp = dxfImageTool.DisplayStructure(aImage, aLayer, aColor, dxfLinetypes.Continuous)
                lSegs = dxfSegments.PolylineSegments(verts, aPlane, bClosed, sGlobalWidth:=aWidth)
                For i As Integer = 1 To lSegs.Count
                    aL = lSegs.Item(i)
                    lPth = aL.GetPath(aPlane, aSegs, aVecs)
                    If lPth.Looop(1).Count >= 4 Then
                        aTrace = New dxeSolid With {
                            .PlaneV = aPlane,
                            .DisplayStructure = aDsp,
                            .IsTrace = True,
                            .Vertex1V = aPth.Looop(1).Item(1),
                            .Vertex2V = aPth.Looop(1).Item(2),
                            .Vertex3V = aPth.Looop(1).Item(3),
                            .Vertex4V = aPth.Looop(1).Item(5)
                        }
                        _rVal.Add(aTrace)
                    End If
                Next i
                Status = strLastStatus
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), ex)
                Return _rVal
            End Try
        End Function
        Private Function xBasicSymbol(aImage As dxfImage, aSymbolType As dxxSymbolTypes, aTextHeight As Double, aName As String, Optional bScaleToScreen As Boolean = False, Optional aArrowSize As Double? = Nothing, Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle) As dxeSymbol
            Try
                If Not GetImage(aImage) Then Throw New Exception("Undefined Image Detected")
                aName = Trim(aName)
                If aName <> "" Then
                    If aImage.Blocks.BlockExists(aName) Then Throw New Exception($"Invalid Block Name Detected. A Block Named '{ aName }' Already Exists.")
                End If
                dxfUtils.StringContainsCharacters(aName, dxfGlobals.BadBlockChars, False, True)
                Dim aPlane As TPLANE = aImage.obj_UCS
                Dim aSets As TTABLEENTRY = TTABLEENTRY.Null
                Dim tstyle As dxoStyle = Nothing
                Dim eSets As New dxfDisplaySettings(dxfImageTool.DisplayStructure_Symbol(aImage, aTextStyle, aSets, aLayerName:="", aColor:=dxxColors.Undefined, rTStyle:=tstyle))
                Dim asz As Double = aSets.Props.ValueD("ArrowSize")
                If aArrowSize IsNot Nothing Then
                    If aArrowSize.HasValue Then
                        asz = aArrowSize.Value
                    End If
                End If
                If aArrowHead = dxxArrowHeadTypes.Suppressed Then asz = 0
                If aArrowHead >= 0 And aArrowHead <= 20 Then aSets.Props.SetVal("ArrowHead", aArrowHead)
                If aTextHeight > 0 Then aSets.Props.SetVal("TextHeight", aTextHeight)
                If bScaleToScreen Then aSets.Props.SetVal("FeatureScale", aImage.obj_DISPLAY.PaperScale)
                If asz >= 0 Then aSets.Props.SetVal("ArrowSize", asz)
                Return New dxeSymbol(aSymbolType, aSets, eSets, aName) With {
                     .PlaneV = aPlane,
                    .ImageGUID = aImage.GUID}
            Catch ex As Exception
                Throw ex
                Return Nothing
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
    End Class 'dxoEntityTool
End Namespace
