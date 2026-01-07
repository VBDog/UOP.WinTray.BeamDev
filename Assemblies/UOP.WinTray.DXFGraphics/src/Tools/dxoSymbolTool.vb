Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoSymbolTool
#Region "Members"

        Private _ImageGUID As String
        Friend ImagePtr As WeakReference
        Private _Status As String

#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage)
            If aImage IsNot Nothing Then
                _ImageGUID = aImage.GUID
                ImagePtr = New WeakReference(aImage)
            End If
            _Status = ""

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Status As String
            Get
                Return _Status
            End Get
            Set(value As String)
                If _Status <> value Then

                    _Status = value
                End If
            End Set
        End Property
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
        Public Function Arrow(aArrowPtXY As iVector, Optional aAngle As Double = 0.0, Optional aLength As Double = 1, Optional aTrailerText As String = Nothing, Optional aTextHeight As Double = 0.0, Optional aArrowSize As Double? = Nothing, Optional aAboveText As String = Nothing, Optional aBelowText As String = Nothing, Optional aLeadText As String = Nothing, Optional bScaleToScreen As Boolean = False, Optional aArrowType As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional aTextStyle As String = "", Optional aName As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol

            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
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
            '^used to draw pointing arrows
            Try
                Return myImage.SaveEntity(myImage.EntityTool.CreateSymbol_ArrowPointer(aArrowPtXY, aAngle, aLength, aTrailerText, aTextHeight, aArrowSize, aAboveText, aBelowText, aLeadText, bScaleToScreen, aArrowType, aArrowStyle, aTextStyle, aName, bSuppressUCS, bSuppressElevation))
                'save to file

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function Axis(aPlane As dxfPlane, Optional aCenter As iVector = Nothing, Optional aAxisLength As Double = 1, Optional aRotation As Double? = Nothing, Optional aXLabel As String = "X", Optional aYLabel As String = "Y", Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aAxisStyle As Integer = -1, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional aTextStyle As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or dxfPlane.IsNull(aPlane) Then Return Nothing

            '#1plane to base the symbol on
            '#2 an optional center to center the axis symbol on
            '#3the length of the draw axis lines
            '#4a rotation to apply
            '#5the label for the X axis
            '#6the label for the Y axis
            '#7the text height to apply instead of the current symbol text size setting
            '#8flag to rescale the entity to the current display on every redraw
            '#9flag to align the text with the axis
            '#10if True the symbol is saved as a block insert when the file is written
            '#10flag to scale the axis to the current display
            '#11the name to assign to the arrow
            '^used to draw an X,Y axis arrows symbol at the passed point

            Try
                Return myImage.SaveEntity(myImage.EntityTool.CreateSymbol_Axis(aPlane, aAxisLength, aRotation, aXLabel, aYLabel, aTextHeight, bScaleToScreen, aName, aAxisStyle, aArrowStyle, aTextStyle, bSuppressUCS, bSuppressElevation))
                'save the entity
                'save to file

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function Axis(aOriginXY As iVector, Optional aAxisLength As Double = 1, Optional aRotation As Double = 0.0, Optional aXLabel As String = "X", Optional aYLabel As String = "Y", Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aAxisStyle As Integer = -1, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional aTextStyle As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
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
            '^used to draw an X,Y axis arrows symbol at the passed point
            Try

                _rVal = myImage.EntityTool.CreateSymbol_Axis(aOriginXY, aAxisLength, aRotation, aXLabel, aYLabel, aTextHeight, bScaleToScreen, aName, aAxisStyle, aArrowStyle, aTextStyle, bSuppressUCS, bSuppressElevation)
                'save the entity
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function Bubble(aBubbleType As dxxBubbleTypes, aInsertPtXY As iVector, aBubbleText As Object, Optional aBubbleHt As Double = 0.0, Optional aBubbleLg As Double = 0.0, Optional aBubbleAngle As Double = 0.0, Optional aHexText As String = Nothing, Optional aTrailerText As String = Nothing, Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
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
            '#13flag to move the insertion point of the symol to the leading end of the symbol entities
            '#14the name to assign to the symbol
            '^used to create various types of bubbles with text inside
            Try
                _rVal = myImage.EntityTool.CreateSymbol_Bubble(aBubbleType, aInsertPtXY, aBubbleText, aBubbleHt, aBubbleLg, aBubbleAngle, aHexText, aTrailerText, aTextHeight, bScaleToScreen, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function Bubble(aBubbleType As dxxBubbleTypes, aLeaderPTsXY As IEnumerable(Of iVector), aBubbleText As String, Optional aBubbleHt As Double = 0.0, Optional aBubbleLg As Double = 0.0, Optional aBubbleAngle As Double = 0.0, Optional aHexText As String = Nothing, Optional aTrailerText As String = Nothing, Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
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
            '#13flag to move the insertion point of the symol to the leading end of the symbol entities
            '#14the name to assign to the symbol
            '^used to create various types of bubbles with text inside
            Try
                _rVal = myImage.EntityTool.CreateSymbol_Bubble(aBubbleType, aLeaderPTsXY, aBubbleText, aBubbleHt, aBubbleLg, aBubbleAngle, aHexText, aTrailerText, aTextHeight, bScaleToScreen, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function DetailBubble(aBubblePointXY As iVector, aRadius As Double, TextString As Object, aLeaderLength As Double, aLeaderAngle As Double, Optional aTextHeight As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aLineType As String = "Continuous", Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
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
                _rVal = myImage.EntityTool.CreateSymbol_DetailBubble(aBubblePointXY, aRadius, TextString, aLeaderLength, aLeaderAngle, aTextHeight, bScaleToScreen, aLineType, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' used to draw section arrows
        ''' </summary>
        ''' <remarks> see dxfImage.SymbolSettings for the stored defaults for new symbol creation</remarks>
        ''' <param name="aVerticesXY">the vertices of the section line</param>
        ''' <param name="aLegLength">the length of the desired arrows</param>
        ''' <param name="aLabel">the text to place above the corners of the section arrow</param>
        ''' <param name="aAngle">the orientation angle of the arrow</param>
        ''' <param name="aTextHeight">the height of the text to apply instead of the current symbol text size setting</param>
        ''' <param name="aTextAligment">controls how the text is aligned to the arrow</param>
        ''' <param name="aArrowStyle">the type of section arrow to draw</param>
        ''' <param name="bScaleToScreen">flag to scale the arrow the current display</param>
        ''' <param name="aLineType">the linetype for the section lines</param>
        ''' <param name="aName">an optional name to asign to the new symbol</param>
        ''' <param name="aTextStyle"> an override style to use for the text </param>
        ''' <param name="aArrowHead">the type of arrowheads to use</param>
        ''' <param name="aArrowSize">an override size for the arrow heads</param>
        ''' <param name="bSuppressUCS">flag to suppress the application of the current UCS to the vertices</param>
        ''' <param name="bSuppressElevation">flag to suppress the curerent elecation of the current UCS is applied</param>
        ''' <returns></returns>
        Public Function SectionArrow(aVerticesXY As IEnumerable(Of iVector), Optional aLegLength As Double = 0.5, Optional aLabel As String = "", Optional aAngle As Double = 0.0, Optional aTextHeight As Double = 0.0, Optional aTextAligment As dxxRectangularAlignments = dxxRectangularAlignments.General, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional bScaleToScreen As Boolean = False, Optional aLineType As String = "", Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Try
                _rVal = myImage.EntityTool.CreateSymbol_SectionArrow(aVerticesXY, aLegLength, aLabel, aAngle, aTextHeight, aTextAligment, aArrowStyle, bScaleToScreen, aLineType, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function ViewArrow(aArrowPtXY As iVector, aSpan As Double, Optional aLegLength As Double = 0.5, Optional aLabel As String = "", Optional aAngle As Double = 0.0, Optional aTextHeight As Double = 0.0, Optional aExtentionLength As Double = 0.0, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional bAlignText As Boolean? = Nothing, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the midpoint of the desired view arrow to draw (this is the center of the passed span)
            '#2the span of the desired arrow
            '#3the length of the legs of the desired arrows
            '#4the text to place above the corners of the view arrow
            '#5the orientation angle of the arrow
            '#6the height of the text to apply instead of the current symbol text size setting
            '#7the type of view arrow to draw (1 or 2)
            '#8flag to align the text to the arrow
            '#9flag to scale the arrow the current display
            '#10if True the arrow is saved as as a block when the file is saved to disk
            '#11the name to assign to the arrow
            Try
                aSpan = Math.Round(Math.Abs(aSpan), 4)
                If aSpan = 0 Then Throw New Exception("Invalid Span Passed")
                If aLegLength <= 0 Then Throw New Exception("Invalid Leg Length Passed")
                aName = Trim(aName)
                '^used to draw view arrows
                _rVal = myImage.EntityTool.CreateSymbol_ViewArrow(New List(Of dxfVector)({New dxfVector(New TVECTOR(aArrowPtXY))}), aSpan, aLegLength, aLabel, aAngle, aTextHeight, aExtentionLength, aArrowStyle, bAlignText, bScaleToScreen, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function ViewArrow(aArrowsPtXY As IEnumerable(Of iVector), aSpan As Double, Optional aLegLength As Double = 0.5, Optional aLabel As String = "", Optional aAngle As Double = 0.0, Optional aTextHeight As Double = 0.0, Optional aExtentionLength As Double = 0.0, Optional aArrowStyle As dxxArrowStyles = dxxArrowStyles.Undefined, Optional bAlignText As Boolean? = Nothing, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double = -1, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the midpoint of the desired view arrow to draw (this is the center of the passed span)
            '#2the span of the desired arrow
            '#3the length of the legs of the desired arrows
            '#4the text to place above the corners of the view arrow
            '#5the orientation angle of the arrow
            '#6the height of the text to apply instead of the current symbol text size setting
            '#7the type of view arrow to draw (1 or 2)
            '#8flag to align the text to the arrow
            '#9flag to scale the arrow the current display
            '#10if True the arrow is saved as as a block when the file is saved to disk
            '#11the name to assign to the arrow
            Try
                aSpan = Math.Round(Math.Abs(aSpan), 4)
                If aSpan = 0 Then Throw New Exception("Invalid Span Passed")
                If aLegLength <= 0 Then Throw New Exception("Invalid Leg Length Passed")
                aName = Trim(aName)
                '^used to draw view arrows
                _rVal = myImage.EntityTool.CreateSymbol_ViewArrow(aArrowsPtXY, aSpan, aLegLength, aLabel, aAngle, aTextHeight, aExtentionLength, aArrowStyle, bAlignText, bScaleToScreen, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function Weld(aInsertPtXY As iVector, aWeldType As dxxWeldTypes, Optional aTextHeight As Double = 0.0, Optional bBothSides As Boolean = False, Optional bAllAround As Boolean = False, Optional Side1Dims As String = Nothing, Optional Side2Dims As String = Nothing, Optional NoteText As String = Nothing, Optional bSuppressTail As Boolean = False, Optional aAngle As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
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
            '#12the block name for the symbol
            '#13the text style name to use for the symbol
            '#14flag to save the symbol with text rather that attributes
            '^used to draw Weld Symbols
            '~weld symbols are placed on the symbol layer if the symbol layer color is defined (see dxfImage.SymbolSettings.DefaultLayer)
            Try
                If aInsertPtXY Is Nothing Then aInsertPtXY = New dxfVector
                _rVal = myImage.EntityTool.CreateSymbol_Weld(New List(Of iVector)({aInsertPtXY}), aWeldType, aTextHeight, bBothSides, bAllAround, Side1Dims, Side2Dims, NoteText, bSuppressTail, aAngle, bScaleToScreen, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function Weld(aInsertPtOrLeaderPTsXY As IEnumerable(Of iVector), aWeldType As dxxWeldTypes, Optional aTextHeight As Double = 0.0, Optional bBothSides As Boolean = False, Optional bAllAround As Boolean = False, Optional Side1Dims As String = Nothing, Optional Side2Dims As String = Nothing, Optional NoteText As String = Nothing, Optional bSuppressTail As Boolean = False, Optional aAngle As Double = 0.0, Optional bScaleToScreen As Boolean = False, Optional aName As String = "", Optional aTextStyle As String = "", Optional aArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ByStyle, Optional aArrowSize As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeSymbol
            Dim _rVal As dxeSymbol = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the leader points for the symbol
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
            '#12the block name for the symbol
            '#13the text style name to use for the symbol
            '#14flag to save the symbol with text rather that attributes
            '^used to draw Weld Symbols
            '~weld symbols are placed on the symbol layer if the symbol layer color is defined (see dxfImage.SymbolSettings.DefaultLayer)
            Try
                _rVal = myImage.EntityTool.CreateSymbol_Weld(aInsertPtOrLeaderPTsXY, aWeldType, aTextHeight, bBothSides, bAllAround, Side1Dims, Side2Dims, NoteText, bSuppressTail, aAngle, bScaleToScreen, aName, aTextStyle, aArrowHead, aArrowSize, bSuppressUCS, bSuppressElevation)
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Private Sub xHandleError(aMethod As Reflection.MethodBase, e As Exception, Optional myImage As dxfImage = Nothing)
            If aMethod Is Nothing Or e Is Nothing Or Not GetImage(myImage) Then Return
            myImage.HandleError(aMethod, Me.GetType(), e)
        End Sub


#End Region 'Methods
#Region "Shared Methods"
        Friend Shared Function CreateSymbolEntities(aImage As dxfImage, aSymbol As dxeSymbol) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            Dim suffix As String = String.Empty
            If aSymbol Is Nothing Then Return _rVal
            If Not aSymbol.GetImage(aImage) Then Return _rVal
            Try
                aSymbol.Planarize()
                Select Case aSymbol.SymbolType
         '---------------------------------------------------------------
                    Case dxxSymbolTypes.Arrow
                        '---------------------------------------------------------------
                        Select Case aSymbol.ArrowType
                            Case dxxArrowTypes.Pointer
                                suffix = ".Arrow.Pointer"
                                _rVal = xSymbol_Pointer(aImage, aSymbol)
                            Case dxxArrowTypes.View
                                suffix = ".Arrow.View"
                                _rVal = xSymbol_ViewArrows(aImage, aSymbol)
                            Case dxxArrowTypes.Section
                                suffix = ".Arrow.Section"
                                _rVal = xSymbol_Section(aImage, aSymbol)
                            Case dxxArrowTypes.Axis
                                suffix = ".Arrow.Axis"
                                _rVal = xSymbol_Axis(aImage, aSymbol)
                        End Select
         '---------------------------------------------------------------
                    Case dxxSymbolTypes.Bubble
                        '---------------------------------------------------------------
                        suffix = ".Bubble"
                        _rVal = xSymbol_Bubble(aImage, aSymbol)
         '---------------------------------------------------------------
                    Case dxxSymbolTypes.DetailBubble
                        '---------------------------------------------------------------
                        suffix = ".DetailBubble"
                        _rVal = xSymbol_DetailBubble(aImage, aSymbol)
         '---------------------------------------------------------------
                    Case dxxSymbolTypes.Weld
                        '---------------------------------------------------------------
                        suffix = ".Weld"
                        _rVal = xSymbol_Weld(aImage, aSymbol)
                End Select
                Return _rVal
            Catch ex As Exception
                aImage.HandleError(Reflection.MethodBase.GetCurrentMethod.Name & suffix, "dxoSymbolTool", ex)
                Return _rVal
            End Try
        End Function
        Friend Shared Function xSymbol_Axis(aImage As dxfImage, aSymbol As dxeSymbol) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aSymbol Is Nothing Then Return _rVal
            '~returns the lines, solids, arrowheads and text of the axis arrows in an entity collection
            Try
                Dim lng As Double = aSymbol.Length
                If lng <= 0 Then lng = 0.05 * aImage.obj_DISPLAY.pln_VIEW.Height
                If lng <= 0 Then lng = 1
                Dim aPt As dxePoint
                Dim aLine As dxeLine
                Dim aPl As dxePolyline
                Dim mTxt As dxeText
                Dim dsp As TDISPLAYVARS = aSymbol.DisplayStructure
                Dim ePln As TPLANE = aSymbol.PlaneV
                Dim tPln As TPLANE
                Dim v1 As TVECTOR = ePln.Origin
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR = ePln.Origin


                Dim aRec As New TPLANE
                Dim bRec As TPLANE
                Dim dClr As dxxColors = aSymbol.LineColor
                Dim arwSty As dxxArrowStyles = aSymbol.ArrowStyle
                Dim algn As dxxRectangularAlignments
                Dim xscal As Double = aSymbol.FeatureScale
                Dim ang As Double = aSymbol.Rotation
                If ang <> 0 Then
                    ePln.Revolve(ang, False)
                End If
                Dim xDir As TVECTOR = ePln.XDirection
                Dim yDir As TVECTOR = ePln.YDirection

                Dim tgap As Double = aSymbol.TextGap * xscal
                Dim tht As Double = aSymbol.TextHeight * xscal
                Dim bBoxIt As Boolean = aSymbol.BoxText
                Dim axStyl As Integer = aSymbol.AxisStyle
                Dim asz As Double = 0.3 * lng
                Dim xcor As Double
                Dim ycor As Double

                If dClr = dxxColors.ByBlock Then dClr = dsp.Color


                If tht <= 0 Then tht = 2 * 0.285 * asz

                dsp.Color = dClr
                'create the entities
                aPl = New dxePolyline With
                {
                    .ImageGUID = aImage.GUID,
                    .OwnerGUID = aSymbol.GUID,
                    .Closed = True,
                    .PlaneV = ePln,
                    .DisplayStructure = dsp,
                    .Color = dClr
                 }
                dsp = aPl.DisplayStructure
                If axStyl = 0 Then
                    ePln.Width = 0.1 * lng
                    ePln.Height = 0.1 * lng
                    aPt = New dxePoint(ePln.Origin, Nothing, "DefPts", "OriginPt") With {.PlaneV = ePln, .Color = dClr, .ImageGUID = aImage.GUID, .Identifier = "Origin", .OwnerGUID = aSymbol.GUID}
                    _rVal.Add(aPt)
                    aPl = aPl.Clone
                    aPl.ImageGUID = aImage.GUID
                    aPl.OwnerGUID = aSymbol.GUID
                    aPl.VectorsV = ePln.Corners
                    aPl.Identifier = "Box"
                    _rVal.Add(aPl)
                    v1 += xDir * (0.5 * ePln.Width)
                    v3 += yDir * (0.5 * ePln.Width)
                End If
                'x axis
                v2 = v1 + xDir * (lng - 0.5 * ePln.Width)
                Dim iPts As TVECTORS = zArrowHeads(aSymbol, aImage, v1, v2, asz, arwSty, "ArrowHead-X", False, _rVal)
                bRec = iPts.Bounds(ePln)
                aLine = New dxeLine(v1, v2, aDisplaySettings:=New dxfDisplaySettings(dsp), aIdentifier:="Line-X") With {.PlaneV = ePln, .ImageGUID = aImage.GUID, .OwnerGUID = aSymbol.GUID}
                _rVal.Add(aLine)
                tPln = New TPLANE(ePln)
                If axStyl = 0 Then
                    algn = dxxRectangularAlignments.MiddleRight
                Else
                    algn = dxxRectangularAlignments.BottomCenter
                    xcor = -asz / 2
                End If
                mTxt = zArrowText(aSymbol, aImage, 1, bBoxIt, tPln, tht, tgap, bRec, aRec, algn, xcor)
                mTxt.Identifier = "Text-X"
                _rVal.Add(mTxt)
                aPl = aPl.Clone()
                aPl.Identifier = "TextBox-X"
                aPl.VectorsV = aRec.Corners
                If mTxt.Suppressed Then aPl.Suppressed = True Else aPl.Suppressed = Not bBoxIt
                _rVal.Add(aPl)
                'y axis
                v2 = v3 + yDir * (lng - 0.5 * ePln.Width)
                iPts = zArrowHeads(aSymbol, aImage, v3, v2, asz, arwSty, "ArrowHead-Y", False, _rVal)
                bRec = iPts.Bounds(ePln)
                aLine = New dxeLine(v3, v2, aDisplaySettings:=New dxfDisplaySettings(dsp), aIdentifier:="Line-Y") With {.PlaneV = ePln, .ImageGUID = aImage.GUID, .OwnerGUID = aSymbol.GUID}

                _rVal.Add(aLine)
                tPln = New TPLANE(ePln)
                If axStyl = 0 Then
                    algn = dxxRectangularAlignments.TopCenter
                Else
                    algn = dxxRectangularAlignments.MiddleLeft
                    ycor = -0.5 * asz
                End If
                mTxt = zArrowText(aSymbol, aImage, 2, bBoxIt, tPln, tht, tgap, bRec, aRec, algn, aYCorrection:=ycor)
                mTxt.Identifier = "Text-Y"
                _rVal.Add(mTxt)
                aPl = aPl.Clone
                aPl.Identifier = "TextBox-Y"
                aPl.VectorsV = aRec.Corners
                If mTxt.Suppressed Then aPl.Suppressed = True Else aPl.Suppressed = Not bBoxIt
                _rVal.Add(aPl)
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Shared Function xSymbol_Bubble(aImage As dxfImage, aBubble As dxeSymbol) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            Try
                Dim Text1 As dxeText
                Dim Text3 As dxeText
                Dim Text2 As dxeText
                Dim aRect As dxfRectangle
                Dim aPl As dxePolyline
                Dim aL As dxeLine
                Dim Arrow1 As dxfEntity = Nothing
                Dim aLdr As dxePolyline = Nothing
                Dim aEnt As dxfEntity
                Dim aTxt As dxeText
                Dim txtEnts As List(Of dxfEntity)
                Dim dClr As dxxColors
                Dim tClr As dxxColors
                Dim i As Integer
                Dim xscal As Double
                Dim ang As Double
                Dim hexHt As Double
                Dim hexLn As Double
                Dim tgap As Double
                Dim tht As Double
                Dim bubLn As Double
                Dim bubHt As Double
                Dim asz As Double
                Dim rad As Double
                Dim oset As Double
                Dim bNoHook As Boolean
                Dim bsup As Boolean
                Dim bPopLeft As Boolean
                Dim intxt As String
                Dim outtext As String
                Dim hxtext As String
                Dim dsp As New TDISPLAYVARS
                Dim verts As TVECTORS
                Dim popDir As TVECTOR
                Dim tp As TVECTOR
                Dim v1 As TVECTOR
                Dim aDir As TVECTOR
                Dim ep As TVECTOR
                Dim aPlane As TPLANE
                Dim bPlane As TPLANE
                Dim bubverts As New TVERTICES
                Dim tStyle As dxoStyle = Nothing
                Dim iGUID As String
                Dim eGUID As String
                iGUID = aImage.GUID
                eGUID = aBubble.GUID
                aPlane = aBubble.PlaneV
                verts = aBubble.VectorsV
                verts = verts.UniqueMembers(3)
                dsp = aBubble.DisplayStructure
                xscal = aBubble.FeatureScale
                asz = aBubble.ArrowSize * xscal
                dClr = aBubble.LineColor
                tClr = aBubble.TextColor
                If tClr = dxxColors.ByBlock Then tClr = dsp.Color
                If dClr = dxxColors.ByBlock Then dClr = dsp.Color
                tgap = aBubble.TextGap
                tht = aBubble.TextHeight * xscal
                aImage.GetOrAdd(dxxReferenceTypes.STYLE, aBubble.TextStyleName, rEntry:=tStyle)
                '           bBoxIt = aBubble.BoxText
                ang = aBubble.Rotation
                If ang > 90 And ang <= 270 Then bPopLeft = True
                bPlane = aPlane
                If ang <> 0 Then bPlane.Revolve(ang, False)
                popDir = bPlane.XDirection
                intxt = Trim(aBubble.Text1)
                hxtext = Trim(aBubble.Text2)
                outtext = Trim(aBubble.Text3)
                dsp.Color = dClr
                bubHt = aBubble.Height
                bubLn = aBubble.Length
                txtEnts = New List(Of dxfEntity)
                aPl = New dxePolyline With {
                    .ImageGUID = iGUID,
                    .OwnerGUID = eGUID,
                    .PlaneV = aPlane,
                    .DisplayStructure = dsp
                }
                'create the leader
                ep = verts.Last
                If verts.Count > 1 Then
                    aLdr = aPl.Clone
                    aLdr.VectorsV = verts
                    aLdr.Identifier = "Leader.Polyline"
                    aL = New dxeLine
                    aL.SetVectors(verts.Item(1), verts.Item(2))
                    If aBubble.ArrowHead <> dxxArrowHeadTypes.None Then
                        bsup = False
                        Arrow1 = dxfArrowheads.GetEntitySymbol(aBubble, aImage, aBubble.ArrowHead, aL, "Leader.ArrowHead", bsup, asz)
                        Arrow1.Suppressed = bsup
                        If Not bsup Then
                            aLdr.Vertices.FirstVector.Strukture = aL.StartPtV
                        End If
                    End If
                    bPopLeft = False
                    bPlane = aPlane
                    popDir = aPlane.XDirection
                    asz = 0.09 * xscal
                    aDir = verts.Item(verts.Count - 1).DirectionTo(verts.Item(verts.Count))
                    ang = aPlane.XDirection.AngleTo(aDir, aPlane.ZDirection)
                    If ang > 90 And ang <= 270 Then
                        popDir *= -1
                        bPopLeft = True
                    End If
                End If
                'create the primary text
                '=============== the bubble interior text
                bsup = (intxt = "")
                If bsup Then intxt = "X"
                tp = ep
                Text1 = New dxeText(dxxTextTypes.Multiline) With {
                    .ImageGUID = iGUID,
                    .OwnerGUID = eGUID,
                    .PlaneV = bPlane,
                    .TextStyleName = tStyle.Name,
                    .DisplayStructure = dsp,
                    .Color = tClr,
                    .Identifier = "Text1",
                    .DrawingDirection = dxxTextDrawingDirections.Horizontal,
                    .Vertical = False,
                    .Alignment = dxxMTextAlignments.MiddleCenter,
                    .AlignmentPt1V = tp,
                    .Suppressed = bsup,
                    .TextString = intxt,
                    .TextHeight = tht
                    }
                If aBubble.Rotation > 90 And aBubble.Rotation <= 270 Then Text1.Rotation = 180
                aRect = Text1.BoundingRectangle
                txtEnts.Add(Text1)
                bPlane.Origin = ep
                bPlane.Width = aRect.Width + 2 * tgap
                bPlane.Height = aRect.Height + 2 * tgap
                '==================================='the bubble
                If aBubble.BubbleType = dxxBubbleTypes.Circular Then 'Bubble Type
                    bubLn = 2 * bPlane.Origin.DistanceTo(bPlane.Point(dxxRectanglePts.TopLeft))
                    '        If bBoxIt Then bubLn = bubLn + tgap
                    If aBubble.Length > bubLn Then bubLn = aBubble.Length
                    If aBubble.Height > bubLn Then bubLn = aBubble.Height
                    bubHt = bubLn
                    bubverts = dxfPrimatives.CreateVertices_Circle(bPlane.Origin, bPlane, bubLn / 2)
                Else
                    bubHt = bPlane.Height
                    '        If bBoxIt Then bubHt = bubHt + tgap
                    If aBubble.Height > bubHt Then bubHt = aBubble.Height
                    bubLn = bPlane.Width
                    '        If bBoxIt Then bubLn = bubLn + tgap
                    If aBubble.BubbleType = dxxBubbleTypes.Rectangular Then
                        If aBubble.Length > bubLn Then bubLn = aBubble.Length
                        bubverts = dxfPrimatives.CreateVertices_Rectangle(bPlane.Origin, bPlane, bubHt, bubLn)
                    Else
                        bubLn += bubHt
                        If aBubble.Length > bubLn Then bubLn = aBubble.Length
                        bubverts = dxfPrimatives.CreateVertices_Pill(bPlane.Origin, bPlane, bubLn, bubHt, 0, aBubble.BubbleType = dxxBubbleTypes.Hexagonal)
                    End If
                End If
                aPl = aPl.Clone
                aPl.Closed = True
                aPl.Identifier = "Bubble"
                aPl.VerticesV = bubverts
                _rVal.Add(aPl)
                tp += popDir * (0.5 * bubLn)
                'the text box
                '    If bBoxIt And Not Text1.Suppressed Then
                '        Set aPl = aPl.Clone
                '        aPl.Identifier = "TextBox1"
                '        aPl.Suppressed = False
                '        aPl.Vertices.Vectors = pln_Corners(aRect.Stretched(2 * tgap).Structure, True)
                '        aPl.IsDirty = True
                '        xSymbol_Bubble.Add aPl
                '    End If
                '============== the hex bubble
                If Not String.IsNullOrWhiteSpace(hxtext) Then
                    Text2 = Text1.Clone(Nothing, aNewTextString:=hxtext)
                    If hxtext.Trim <> "" Then
                        Text2.TextString = Trim(hxtext)
                        Text2.Suppressed = False
                    Else
                        Text2.TextString = "X"
                        Text2.Suppressed = True
                    End If
                    Text2.AlignmentPt1V = tp
                    aRect = Text2.BoundingRectangle.Stretched(2 * tgap)
                    hexHt = bubHt
                    hexLn = hexHt ' aRect.Width + hexHt
                    rad = (hexHt / 2) * 0.8666
                    Text2.Transform(TTRANSFORM.CreateProjection(popDir, hexLn / 2), True)
                    aRect.ProjectV(popDir, hexLn / 2, True)
                    Text2.Identifier = "Text2"
                    txtEnts.Add(Text2)
                    tp = Text2.AlignmentPt1V + (popDir * hexLn / 2)
                    'the hexagon
                    aPl = aPl.Clone
                    aPl.Suppressed = Text2.Suppressed
                    aPl.Identifier = "Hexagon"
                    'aPl.verticesv = dxfPrimatives.CreateVertices_Pill(Text2.AlignmentPt1V, bPlane, hexLn, hexHt, 0, True)
                    aPl.VerticesV = dxfPrimatives.CreateVertices_Polygon(Text2.AlignmentPt1V, bPlane, 6, rad, 0, True)
                    _rVal.Add(aPl)
                    'the text box
                    '    If bBoxIt And Not Text1.Suppressed Then
                    '        Set aPl = aPl.Clone
                    '        aPl.Identifier = "TextBox2"
                    '        aPl.Suppressed = False
                    '        aPl.Vertices.Vectors = pln_Corners(aRect.Structure, True)
                    '        aPl.IsDirty = True
                    '        xSymbol_Bubble.Add aPl
                    '    End If
                End If
                '============== the trailing text
                If Not String.IsNullOrWhiteSpace(outtext) Then
                    Text3 = Text1.Clone(Nothing, aNewTextString:=outtext)
                    Text3.Suppressed = False
                    Text3.AlignmentPt1V = tp + (popDir * tgap)
                    Text3.Identifier = "TEXT3"
                    If bPopLeft Then
                        Text3.Alignment = dxxMTextAlignments.MiddleRight
                    Else
                        Text3.Alignment = dxxMTextAlignments.MiddleLeft
                    End If
                    txtEnts.Add(Text3)
                End If
                If Not bNoHook Then oset += asz
                If bPopLeft Then oset += bPlane.Width
                If aLdr IsNot Nothing Then
                    oset = 0.5 * bubLn
                    If Not bNoHook Then oset += asz
                    v1 = ep + (popDir * oset)
                    v1 -= ep
                    For i = 1 To _rVal.Count
                        aEnt = _rVal.Item(i)
                        aEnt.Move(v1.X, v1.Y, v1.Z)
                    Next i
                    For i = 1 To txtEnts.Count
                        aEnt = txtEnts.Item(i)
                        aEnt.Move(v1.X, v1.Y, v1.Z)
                    Next i
                    If Not bNoHook Then
                        aL = New dxeLine With {.Identifier = "HookLine",
                        .PlaneV = aPlane,
                        .DisplayStructure = aPl.DisplayStructure,
                        .Suppressed = False}
                        aL.SetVectors(ep, ep + (popDir * asz))
                        _rVal.Add(aL)
                    End If
                    _rVal.Add(aLdr)
                    If Arrow1 IsNot Nothing Then _rVal.Add(Arrow1)
                End If
                For i = 1 To txtEnts.Count
                    aTxt = txtEnts.Item(i)
                    _rVal.Add(aTxt)
                Next i
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Shared Function xSymbol_DetailBubble(aImage As dxfImage, aSymbol As dxeSymbol) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            Try
                Dim aCirc As dxeArc
                '**UNUSED VAR** Dim aLdr As dxeLeader
                Dim mTxt As dxeText
                Dim aPline As dxePolyline
                Dim tStyle As dxoStyle
                Dim aRect As dxfRectangle
                Dim aL As dxeLine
                Dim dClr As dxxColors
                Dim tClr As dxxColors
                Dim bSupp As Boolean
                Dim rad As Double
                Dim ldrLen As Double
                Dim lAng As Double
                Dim tgap As Double
                Dim tht As Double
                '**UNUSED VAR** Dim tang As Single
                Dim bAddHook As Boolean
                Dim asz As Double
                Dim xscal As Double
                Dim bBoxIt As Boolean
                Dim txt As String
                Dim hklen As Double
                Dim aPlane As TPLANE
                Dim txtPlane As TPLANE
                Dim dsp As New TDISPLAYVARS
                Dim bP As TVECTOR
                Dim ldrDir As TVECTOR
                Dim hookDir As TVECTOR
                Dim sp As TVECTOR
                Dim ep As TVECTOR
                Dim vecs As New TVECTORS
                Dim hp As TVECTOR
                Dim txtpt As TVECTOR
                Dim Arrow1 As dxfEntity
                'intialize

                aPlane = aSymbol.PlaneV
                lAng = aSymbol.Rotation
                txt = Trim(aSymbol.Text1)
                rad = aSymbol.Height
                ldrLen = aSymbol.Length
                dsp = aSymbol.DisplayStructure
                bP = aSymbol.InsertionPtV
                xscal = aSymbol.FeatureScale
                dClr = aSymbol.LineColor
                tClr = aSymbol.TextColor
                If tClr = dxxColors.ByBlock Then tClr = dsp.Color
                If dClr = dxxColors.ByBlock Then dClr = dsp.Color
                tgap = aSymbol.TextGap * xscal
                asz = aSymbol.ArrowSize * xscal
                tht = aSymbol.TextHeight * xscal
                aSymbol.TextStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aSymbol.TextStyleName)
                tStyle = New dxoStyle(aImage.TextStyle(aSymbol.TextStyleName)) With {.IsCopied = True}

                hklen = asz
                If hklen = 0 Then hklen = 0.09 * xscal
                bBoxIt = aSymbol.BoxText
                dsp.Color = dClr
                ldrDir = aPlane.Direction(lAng, False)
                'the circle
                aCirc = New dxeArc With {
                    .PlaneV = aPlane,
                    .DisplayStructure = dsp,
                    .Radius = rad,
                    .CenterV = bP,
                    .Identifier = "Bubble"
                }
                _rVal.Add(aCirc)
                'the leader
                sp = bP + (ldrDir * rad)
                ep = sp + (ldrDir * ldrLen)
                vecs.Add(sp)
                txtPlane = aPlane
                If lAng >= 90 And lAng < 270 Then
                    txtPlane.Define(Nothing, ldrDir * -1, txtPlane.ZDirection.CrossProduct(ldrDir * -1, True))
                Else
                    txtPlane.Define(Nothing, ldrDir, txtPlane.ZDirection.CrossProduct(ldrDir, True))
                End If
                hookDir = ldrDir
                If lAng <= 15 Then bAddHook = False
                If lAng >= 165 And lAng <= 195 Then bAddHook = False
                If lAng >= 345 Then bAddHook = False
                If bAddHook Then
                    hp = ep + hookDir * hklen
                    vecs.Add(hp)
                End If
                vecs.Add(ep)
                'create the leader
                aPline = New dxePolyline With
                {
                    .DisplayStructure = dsp,
                    .Identifier = "LabelLine",
                    .VectorsV = vecs,
                    .Suppressed = False
                }
                _rVal.Add(aPline)
                txtpt = ep
                mTxt = New dxeText(dxxTextTypes.Multiline, New dxfDisplaySettings(dsp)) With
                {
                    .ImageGUID = aImage.GUID,
                    .OwnerGUID = aSymbol.GUID,
                    .PlaneV = txtPlane,
                    .TextStyleName = tStyle.Name,
                    .DrawingDirection = dxxTextDrawingDirections.Horizontal,
                    .Vertical = False,
                    .Color = tClr,
                    .TextHeight = tht,
                    .Alignment = dxxMTextAlignments.MiddleCenter,
                    .AlignmentPt1 = New dxfVector(txtpt),
                    .Identifier = "MText1"
                }
                If txt = "" Then
                    mTxt.TextString = "X"
                    mTxt.Suppressed = True
                Else
                    mTxt.TextString = txt
                End If
                aRect = mTxt.BoundingRectangle.Stretched(2 * tgap)
                txtpt += hookDir * (0.5 * aRect.Width)
                sp = txtpt - ep
                mTxt.Move(sp.X, sp.Y, sp.Z)
                _rVal.Add(mTxt)
                If asz > 0 Then
                    If aSymbol.ArrowHeadType <> dxxArrowHeadTypes.None Then
                        aL = New dxeLine With {.DisplayStructure = dsp}
                        aL.SetVectors(vecs.Item(1), vecs.Item(2))
                        Arrow1 = dxfArrowheads.GetEntitySymbol(aSymbol, aImage, aSymbol.ArrowHead, aL, "ArrowHead", bSupp)
                        Arrow1.Suppressed = bSupp
                        If Not bSupp Then
                            aPline.Vertices.FirstVector.Strukture = aL.StartPtV
                        End If
                        _rVal.Add(Arrow1)
                    End If
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Shared Function xSymbol_Pointer(aImage As dxfImage, aSymbol As dxeSymbol) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aSymbol Is Nothing Then Return _rVal
            Try
                Dim aPl As dxePolyline
                Dim aLine As dxeLine
                Dim mTxt As dxeText
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim ePln As TPLANE = aSymbol.PlaneV
                Dim tPln As TPLANE
                Dim bRec As TPLANE
                Dim aRec As New TPLANE
                Dim iPts As TVECTORS
                Dim dsp As TDISPLAYVARS = aSymbol.DisplayStructure
                Dim vecs As TVECTORS
                Dim txtVals() As String
                Dim dClr As dxxColors = aSymbol.LineColor
                Dim algn As dxxRectangularAlignments
                Dim arwSty As dxxArrowStyles = aSymbol.ArrowStyle
                Dim i As Integer
                Dim xscal As Double = aSymbol.FeatureScale
                Dim tht As Double = aSymbol.TextHeight * xscal
                Dim asz As Double = aSymbol.ArrowSize * xscal
                Dim ang As Double = aSymbol.Rotation
                Dim leng As Double = aSymbol.Length
                Dim tgap As Double = aSymbol.TextGap * xscal
                Dim bBoxIt As Boolean = aSymbol.BoxText
                Dim txt As String
                Dim xcor As Double
                'initialize
                vecs = New TVECTORS(2)
                If dClr = dxxColors.ByBlock Then dClr = dsp.Color
                vecs.SetItem(1, aSymbol.InsertionPtV)
                If arwSty < dxxArrowStyles.StdBlocks Then arwSty = dxxArrowStyles.StdBlocks
                If leng <= 0 Then leng = 2 * asz
                If leng <= 0 Then leng = 1
                dsp.Color = dClr
                If ang <> 0 Then ePln.Revolve(ang, False)
                tPln = ePln.Clone
                aPl = New dxePolyline With {
                    .ImageGUID = aImage.GUID,
                    .OwnerGUID = aSymbol.GUID,
                    .Closed = True,
                    .PlaneV = ePln,
                    .DisplayStructure = dsp,
                    .Color = dClr,
                    .Identifier = "BaseLine1"
                }
                dsp = aPl.DisplayStructure
                '================== Create Arrow Entities
                vecs.SetItem(2, vecs.Item(1) + (ePln.XDirection * -leng))
                v1 = vecs.Item(1).Clone
                v2 = vecs.Item(2).Clone
                iPts = zArrowHeads(aSymbol, aImage, v2, v1, asz, arwSty, "ArrowHead1", False, _rVal)
                vecs.SetItem(1, v1)
                vecs.SetItem(2, v2)
                bRec = iPts.Bounds(ePln)
                'create the the line
                aLine = New dxeLine(vecs.Item(1), vecs.Item(2), aDisplaySettings:=New dxfDisplaySettings(dsp)) With {
                    .ImageGUID = aImage.GUID,
                    .OwnerGUID = aSymbol.GUID,
                    .PlaneV = ePln,
                    .Identifier = "LINE"
                }
                _rVal.Add(aLine)
                leng = aLine.Length
                ReDim txtVals(0 To 3)
                txtVals(0) = aSymbol.Text1.Trim 'tAbove
                txtVals(1) = aSymbol.Text2.Trim 'tBelow
                txtVals(2) = aSymbol.Text3.Trim 'tTrail
                txtVals(3) = aSymbol.Text4.Trim 'tLead
                '        If Not IsMissing(aAboveText) Then .Text1 = Trim(aAboveText)
                '        If Not IsMissing(aBelowText) Then .Text2 = Trim(aBelowText)
                '        If Not IsMissing(aTrailerText) Then .Text3 = Trim(aTrailerText)
                '        If Not IsMissing(aLeadText) Then .Text4 = Trim(aLeadText)
                For i = 1 To 4
                    xcor = 0
                    txt = txtVals(i - 1)
                    Select Case i
                        Case 1
                            algn = dxxRectangularAlignments.TopCenter
                            xcor = -0.5 * asz
                        Case 2
                            algn = dxxRectangularAlignments.BottomCenter
                            xcor = -0.5 * asz
                        Case 3
                            algn = dxxRectangularAlignments.MiddleLeft
                        Case 4
                            algn = dxxRectangularAlignments.MiddleRight
                    End Select
                    If txt <> "" Then
                        mTxt = zArrowText(aSymbol, aImage, i, bBoxIt, tPln, tht, tgap, bRec, aRec, algn, xcor)
                        If mTxt IsNot Nothing Then
                            _rVal.Add(mTxt)
                            aPl = aPl.Clone
                            aPl.Identifier = "TextBox" & i
                            aPl.Suppressed = Not bBoxIt Or mTxt.Suppressed
                            aPl.VectorsV = aRec.Corners
                            _rVal.Add(aPl)
                        End If
                    End If
                Next i
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Shared Function xSymbol_Section(aImage As dxfImage, aSymbol As dxeSymbol) As List(Of dxfEntity)
            '~returns the lines, solids, arrowheads and text of the view arrows in an entity collection
            Dim _rVal As New List(Of dxfEntity)
            If aSymbol Is Nothing Then Return _rVal
            Try
                Dim aPl As dxePolyline
                Dim aL As dxeLine
                Dim mTxt As dxeText
                Dim ePln As TPLANE
                Dim tPln As TPLANE
                Dim dsp As New TDISPLAYVARS
                Dim vecs As TVECTORS
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim pVerts As TVECTORS
                Dim iPts As TVECTORS
                Dim aRec As New TPLANE
                Dim bRec As TPLANE
                Dim dClr As dxxColors
                Dim arwSty As dxxArrowStyles
                Dim algn As dxxRectangularAlignments
                Dim asz As Double
                Dim tgap As Double
                Dim tht As Double
                '**UNUSED VAR** Dim tSty As String
                Dim bBoxIt As Boolean
                Dim ang As Double
                Dim longleg As Double
                Dim xscal As Double
                ePln = aSymbol.PlaneV
                dsp = aSymbol.DisplayStructure
                pVerts = aSymbol.VectorsV
                xscal = aSymbol.FeatureScale
                dClr = aSymbol.LineColor
                algn = aSymbol.ArrowTextAlignment
                If dClr = dxxColors.ByBlock Then dClr = dsp.Color
                tgap = aSymbol.TextGap * xscal
                asz = aSymbol.ArrowSize * xscal
                arwSty = aSymbol.ArrowStyle
                tht = aSymbol.TextHeight * xscal
                bBoxIt = aSymbol.BoxText
                tgap = Math.Abs(tgap)
                dsp.Color = dClr
                ang = aSymbol.Rotation
                longleg = aSymbol.Length
                If arwSty <> 1 And arwSty <> 2 Then
                    If longleg < 2 * asz Then longleg = 2 * asz
                Else
                    If longleg < 1.25 * asz Then longleg = 1.25 * asz
                End If
                If ang <> 0 Then ePln.Revolve(ang)
                tPln = ePln
                'create the entities
                aPl = New dxePolyline With {
                    .ImageGUID = aImage.GUID,
                    .OwnerGUID = aSymbol.GUID,
                    .PlaneV = ePln,
                    .DisplayStructure = dsp,
                    .VectorsV = pVerts,
                    .Identifier = "SectionLine",
                    .Color = dClr
                }
                dsp.Linetype = dxfLinetypes.Continuous
                _rVal.Add(aPl)
                vecs = New TVECTORS(4)
                vecs.SetItem(1, pVerts.Item(1).Clone)
                vecs.SetItem(2, vecs.Item(1) + (ePln.XDirection * longleg))
                vecs.SetItem(3, pVerts.Item(pVerts.Count).Clone)
                vecs.SetItem(4, vecs.Item(3) + (ePln.XDirection * longleg))
                'the arrow lines
                aL = New dxeLine With {
                    .ImageGUID = aImage.GUID,
                    .OwnerGUID = aSymbol.GUID,
                    .PlaneV = ePln,
                    .DisplayStructure = dsp,
                    .Identifier = "Pointer.Line1"
                }
                'first arrow head
                v1 = vecs.Item(1)
                v2 = vecs.Item(2)
                iPts = zArrowHeads(aSymbol, aImage, v1, v2, asz, arwSty, "ArrowHead1", False, _rVal)
                vecs.SetItem(1, v1)
                vecs.SetItem(2, v2)
                bRec = iPts.Bounds(ePln)
                aL.StartPtV = vecs.Item(1)
                aL.EndPtV = vecs.Item(2)
                _rVal.Add(aL)
                'first text
                mTxt = zArrowText(aSymbol, aImage, 1, bBoxIt, tPln, tht, tgap, bRec, aRec, algn)
                If mTxt IsNot Nothing Then
                    _rVal.Add(mTxt)
                    aPl = aPl.Clone
                    aPl.ImageGUID = aImage.GUID
                    aPl.OwnerGUID = aSymbol.GUID
                    aPl.Closed = True
                    aPl.Suppressed = mTxt.Suppressed Or Not bBoxIt
                    aPl.Identifier = "TextBox1"
                    aPl.VectorsV = aRec.Corners
                    _rVal.Add(aPl)
                End If
                aL = aL.Clone
                aL.ImageGUID = aImage.GUID
                aL.OwnerGUID = aSymbol.GUID
                aL.Identifier = "Pointer.Line2"
                'secod arrow head
                v1 = vecs.Item(3)
                v2 = vecs.Item(4)
                iPts = zArrowHeads(aSymbol, aImage, v1, v2, asz, arwSty, "ArrowHead2", False, _rVal)
                bRec = iPts.Bounds(ePln)
                vecs.SetItem(3, v1)
                vecs.SetItem(4, v2)
                aL.StartPtV = vecs.Item(3)
                aL.EndPtV = vecs.Item(4)
                _rVal.Add(aL)
                'second text
                Select Case algn
                    Case dxxRectangularAlignments.TopLeft
                        algn = dxxRectangularAlignments.BottomLeft
                    Case dxxRectangularAlignments.TopCenter
                        algn = dxxRectangularAlignments.BottomCenter
                    Case dxxRectangularAlignments.TopRight
                        algn = dxxRectangularAlignments.BottomRight
                    Case dxxRectangularAlignments.BottomLeft
                        algn = dxxRectangularAlignments.TopLeft
                    Case dxxRectangularAlignments.BottomCenter
                        algn = dxxRectangularAlignments.TopCenter
                    Case dxxRectangularAlignments.BottomRight
                        algn = dxxRectangularAlignments.TopRight
                End Select
                mTxt = zArrowText(aSymbol, aImage, 1, bBoxIt, tPln, tht, tgap, bRec, aRec, algn)
                If mTxt IsNot Nothing Then
                    _rVal.Add(mTxt)
                    aPl = aPl.Clone
                    aPl.ImageGUID = aImage.GUID
                    aPl.OwnerGUID = aSymbol.GUID
                    aPl.Closed = True
                    aPl.Suppressed = mTxt.Suppressed Or Not bBoxIt
                    aPl.Identifier = "TextBox2"
                    aPl.VectorsV = aRec.Corners
                    _rVal.Add(aPl)
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Shared Function xSymbol_ViewArrows(aImage As dxfImage, aSymbol As dxeSymbol) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aSymbol Is Nothing Then Return _rVal
            '~returns the lines, solids, arrowheads and text of the view arrows in an entity collection
            Try
                Dim aPl As dxePolyline
                Dim mTxt As dxeText
                Dim aSpan As Double
                Dim ang As Double
                Dim shortleg As Double
                Dim longleg As Double
                Dim xscal As Double = aSymbol.FeatureScale
                Dim tgap As Double
                Dim tht As Double
                Dim asz As Double
                Dim bBoxIt As Boolean
                Dim d1 As Double
                Dim arwSty As dxxArrowStyles = aSymbol.ArrowStyle
                Dim dClr As dxxColors = aSymbol.LineColor
                Dim tAlgn As dxxRectangularAlignments
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim ePln As TPLANE = aSymbol.PlaneV
                Dim tPln As TPLANE
                Dim aRec As New TPLANE
                Dim bRec As TPLANE
                Dim xDir As TVECTOR
                Dim yDir As TVECTOR
                Dim iPts As TVECTORS
                Dim vecs As TVECTORS
                Dim dsp As TDISPLAYVARS = aSymbol.DisplayStructure
                Dim mirLine As New TLINE
                xscal = aSymbol.FeatureScale
                dClr = aSymbol.LineColor
                If dClr = dxxColors.ByBlock Then dClr = dsp.Color
                arwSty = aSymbol.ArrowStyle
                asz = aSymbol.ArrowSize * xscal
                tgap = aSymbol.TextGap * xscal
                tht = aSymbol.TextHeight * xscal
                bBoxIt = aSymbol.BoxText
                tAlgn = aSymbol.ArrowTextAlignment
                ang = aSymbol.Rotation
                aPl = New dxePolyline With {
                    .PlaneV = ePln,
                    .DisplayStructure = dsp,
                    .Color = dClr,
                    .Identifier = "BaseLine1"
                }
                dsp = aPl.DisplayStructure
                xDir = ePln.Direction(ang, False)
                yDir = xDir.RotatedAbout(ePln.ZDirection, 90).Normalized
                mirLine.SPT = aSymbol.InsertionPtV
                ePln.Define(Nothing, xDir, yDir)
                aSpan = aSymbol.Span
                'get the x axis at the center of the arrow span
                mirLine.EPT = mirLine.SPT + (xDir * 10)
                'set the leg lengths
                longleg = aSymbol.Length * xscal
                If longleg < 1.25 * asz Then longleg = 1.25 * asz
                shortleg = 0.35 * longleg + Math.Abs(aSymbol.Height) * xscal
                '================ lines and arrowheads ==========================
                vecs = New TVECTORS(6)
                vecs.SetItem(1, mirLine.SPT + (yDir * (aSpan / 2)))
                vecs.SetItem(2, vecs.Item(1) + (yDir * shortleg))
                vecs.SetItem(3, vecs.Item(2) + (xDir * longleg))
                vecs.SetItem(4, mirLine.SPT - (yDir * (aSpan / 2)))
                vecs.SetItem(5, vecs.Item(4) - (yDir * shortleg))
                vecs.SetItem(6, vecs.Item(5) + (xDir * longleg))
                v1 = vecs.Item(1)
                v2 = vecs.Item(2)
                v3 = vecs.Item(3)
                'first arrow head
                iPts = zArrowHeads(aSymbol, aImage, v2, v3, asz, arwSty, "ArrowHead1", False, _rVal)
                bRec = iPts.Bounds(ePln)
                vecs.SetItem(2, v2)
                vecs.SetItem(3, v3)
                Dim iLine As New TLINE(v2, v3)
                'first polyline
                aPl.VectorsV = vecs.SubSet(1, 3, bReturnClones:=True)
                _rVal.Add(aPl)
                'second arrow head
                v2 = vecs.Item(5)
                v3 = vecs.Item(6)
                zArrowHeads(aSymbol, aImage, v2, v3, asz, arwSty, "ArrowHead2", True, _rVal)
                vecs.SetItem(5, v2)
                vecs.SetItem(6, v3)
                aPl = aPl.Clone
                'second polyline
                aPl.Identifier = "BaseLine2"
                aPl.VectorsV = vecs.SubSet(4, 6, bReturnClones:=True)
                _rVal.Add(aPl)
                '================ Corner Lines ==========================
                d1 = 0.1 * (shortleg - Math.Abs(aSymbol.Height) * xscal)
                v1 = vecs.Item(2) + xDir * d1
                v1 += yDir * -d1
                d1 = (shortleg - Math.Abs(aSymbol.Height) * xscal) / 2
                vecs = New TVECTORS(v1 + yDir * -d1, v1, v1 + xDir * d1)
                aPl = aPl.Clone
                aPl.ImageGUID = aImage.GUID
                aPl.OwnerGUID = aSymbol.GUID
                aPl.Linetype = dxfLinetypes.Continuous
                aPl.Identifier = "Symbol1"
                aPl.VectorsV = vecs
                _rVal.Add(aPl)
                aPl = aPl.Clone
                aPl.ImageGUID = aImage.GUID
                aPl.OwnerGUID = aSymbol.GUID
                aPl.VectorsV = vecs.Mirrored(mirLine)
                aPl.Identifier = "Symbol2"
                _rVal.Add(aPl)
                '================ THE TEXT ==========================
                'align the text to the x direction of the arrow objects plane
                tPln = ePln
                'the text 1
                mTxt = zArrowText(aSymbol, aImage, 1, bBoxIt, tPln, tht, tgap, bRec, aRec, tAlgn)
                If mTxt IsNot Nothing Then
                    _rVal.Add(mTxt)
                    aPl = aPl.Clone
                    aPl.ImageGUID = aImage.GUID
                    aPl.OwnerGUID = aSymbol.GUID
                    aPl.Closed = True
                    aPl.Suppressed = mTxt.Suppressed Or Not bBoxIt
                    aPl.Identifier = "TextBox1"
                    vecs = aRec.Corners
                    aPl.VectorsV = vecs
                    _rVal.Add(aPl)
                    'the other side
                    mTxt = mTxt.Clone
                    mTxt.AlignmentPt1V = mTxt.AlignmentPt1V.Mirrored(mirLine)
                    _rVal.Add(mTxt)
                    vecs = vecs.Mirrored(mirLine)
                    aPl = aPl.Clone
                    aPl.ImageGUID = aImage.GUID
                    aPl.OwnerGUID = aSymbol.GUID
                    aPl.Identifier = "TextBox2"
                    aPl.VectorsV = vecs
                    _rVal.Add(aPl)
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Shared Function xSymbol_Weld(aImage As dxfImage, aSymbol As dxeSymbol) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            If aSymbol Is Nothing Then Return _rVal
            Try
                Dim aPlane As TPLANE
                Dim aRec As TPLANE
                Dim bRec As TPLANE
                Dim xDir As TVECTOR
                Dim yDir As TVECTOR
                Dim txtDir As TVECTOR
                Dim ip As TVECTOR
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim dsp As New TDISPLAYVARS
                Dim aDir As TVECTOR
                Dim verts As TVECTORS
                Dim tht As Double
                Dim tgap As Double
                Dim bBoxIt As Boolean
                Dim aL As dxeLine
                Dim bL As dxeLine
                Dim aPl As dxePolyline
                Dim aTail As dxePolyline
                Dim aSyb As dxePolyline
                Dim xscal As Double
                Dim dClr As dxxColors
                Dim tClr As dxxColors
                Dim txtTop As dxeText = Nothing
                Dim txtBot As dxeText = Nothing
                Dim txtTrail As dxeText = Nothing
                Dim txt1 As String
                Dim txt2 As String
                Dim txt3 As String
                Dim tlen As Double
                Dim ht As Double
                Dim tang As Double
                Dim d1 As Double
                Dim bInv As Boolean
                Dim Hi As Integer
                Dim rad As Double
                Dim iGUID As String
                Dim eGUID As String
                Dim crad As Double
                Dim tStyle As dxoStyle
                Dim ac As dxeArc
                Dim aRect As dxfRectangle
                Dim aCF As Double
                Dim Arrow1 As dxfEntity
                Dim bSupp As Boolean
                tStyle = New dxoStyle
                iGUID = aImage.GUID
                eGUID = aSymbol.GUID
                aPlane = aSymbol.PlaneV
                txt1 = aSymbol.Text1
                txt2 = aSymbol.Text2
                txt3 = aSymbol.Text3
                verts = aSymbol.VectorsV
                verts = verts.UniqueMembers(3)
                aSymbol.VectorsV = verts
                dsp = aSymbol.DisplayStructure
                Hi = 1
                If verts.Count > 0 Then ip = verts.Last Else ip = aPlane.Origin
                xscal = aSymbol.FeatureScale
                tht = aSymbol.TextHeight * xscal
                tgap = aSymbol.TextGap * xscal
                dClr = aSymbol.LineColor
                tClr = aSymbol.TextColor
                If dClr = dxxColors.ByBlock Then dClr = dsp.Color
                If tClr = dxxColors.ByBlock Then dClr = dsp.Color
                bBoxIt = aSymbol.BoxText
                aSymbol.TextStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aSymbol.TextStyleName)
                tStyle = New dxoStyle(aImage.TextStyle(aSymbol.TextStyleName)) With {.IsCopied = True}
                If dClr = dxxColors.ByBlock Then dClr = aSymbol.Color
                dsp.Color = dClr
                If tgap = 0 Then tgap = 0.09 * xscal
                aL = New dxeLine With {.DisplayStructure = dsp, .PlaneV = aPlane}
                If bBoxIt Then aCF = tgap
                ht = tht + 2 * tgap
                rad = 0.5 * ht
                crad = 0.75 * rad
                xDir = aPlane.XDirection
                yDir = aPlane.YDirection
                'create the leader
                If verts.Count > 1 Then
                    aPl = New dxePolyline With {
             .ImageGUID = iGUID,
             .OwnerGUID = eGUID,
                    .PlaneV = aPlane,
                    .VectorsV = verts,
                    .Identifier = "Leader.Polyline",
                    .DisplayStructure = dsp}
                    _rVal.Add(aPl)
                    aL.StartPtV = verts.Item(1)
                    aL.EndPtV = verts.Item(2)
                    If aSymbol.ArrowHead <> dxxArrowHeadTypes.None Then
                        Arrow1 = dxfArrowheads.GetEntitySymbol(aSymbol, aImage, aSymbol.ArrowHead, aL, "Leader.ArrowHead", bSupp)
                        Arrow1.Suppressed = bSupp
                        If Not bSupp Then aPl.Vertices.FirstVector.Strukture = aL.StartPtV
                        _rVal.Add(Arrow1)
                    End If
                    aDir = verts.Item(verts.Count - 1).DirectionTo(verts.Last)
                    tang = aPlane.XDirection.AngleTo(aDir, aPlane.ZDirection)
                    If tang > 90 And tang <= 270 Then xDir *= -1
                End If
                If aSymbol.Rotation <> 0 Then xDir.RotateAbout(aPlane.ZDirection, aSymbol.Rotation, False)
                txtDir = xDir
                tang = aPlane.XDirection.AngleTo(txtDir, aPlane.ZDirection)
                If tang > 90 And tang <= 270 Then
                    bInv = True
                    tang += 180
                End If
                '^create the top side text
                If bBoxIt Then d1 = tgap + 0.09 * xscal Else d1 = tgap
                txtTop = New dxeText(dxxTextTypes.Multiline) With {
        .ImageGUID = iGUID,
                    .OwnerGUID = eGUID,
                    .TextStyleName = tStyle.Name,
                    .DrawingDirection = dxxTextDrawingDirections.Horizontal,
                    .Vertical = False,
                    .AttributeTag = "Text1",
                    .PlaneV = aPlane,
                    .AlignmentPt1V = ip,
                    .TextHeight = tht,
                    .Rotation = tang,
                    .DisplayStructure = dsp,
                    .Color = tClr,
                    .Identifier = "Text1",
                    .Alignment = dxxMTextAlignments.MiddleCenter
                }
                If Trim(txt1) <> "" Then
                    txtTop.TextString = txt1
                Else
                    txtTop.TextString = "XXX"
                    txtTop.Suppressed = True
                End If
                txtTop.UpdatePath(False, aImage)
                aRec = txtTop.Bounds
                tlen = aRec.Width + 4 * tgap
                _rVal.Add(txtTop)
                '^create the bottom side text
                If Trim(txt2) <> "" Then
                    txtBot = New dxeText(dxxTextTypes.Multiline) With {
                        .ImageGUID = iGUID,
                        .OwnerGUID = eGUID,
                        .TextStyleName = tStyle.Name,
                        .DrawingDirection = dxxTextDrawingDirections.Horizontal,
                        .Vertical = False,
                        .AttributeTag = "Text2",
                        .PlaneV = aPlane,
                        .AlignmentPt1V = ip,
                        .TextHeight = tht,
                        .Rotation = tang,
                        .DisplayStructure = dsp,
                        .Color = tClr,
                        .Identifier = "Text2",
                        .Alignment = dxxMTextAlignments.MiddleCenter,
                        .TextString = txt2
                    }
                    txtBot.UpdatePath()
                    bRec = txtBot.Bounds
                    If bRec.Width + (2 * tgap) > tlen Then tlen = bRec.Width + (2 * tgap)
                    _rVal.Add(txtBot)
                End If
                d1 = rad + ht + tlen + 2 * tgap + 2 * aCF
                bL = aL.Clone
                bL.StartPtV = ip
                bL.EndPtV = ip + xDir * d1
                bL.Identifier = "BaseLine"
                _rVal.Add(bL)
                yDir = xDir.RotatedAbout(aPlane.ZDirection, 90).Normalized
                v1 = bL.EndPtV
                v1 += xDir * -(tgap + 0.5 * tlen)
                d1 = 0.5 * aRec.Height
                d1 += tgap
                If bBoxIt Then d1 += tgap
                v2 = v1 + yDir * d1
                txtTop.AlignmentPt1V = v2
                If txtBot IsNot Nothing Then
                    d1 = 0.5 * bRec.Height
                    d1 += tgap
                    If bBoxIt Then d1 += tgap
                    v2 = v1 + (yDir * -d1)
                    txtBot.AlignmentPt1V = v2
                End If
                '====================== weld type specific entities
                If aSymbol.WeldType = dxxWeldTypes.Fillet Then
                    aSyb = New dxePolyline With {
                        .ImageGUID = iGUID,
                        .OwnerGUID = eGUID,
                        .PlaneV = aPlane,
                        .DisplayStructure = dsp
                    }
                    v1 = ip + xDir * rad
                    v3 = ip + xDir * (rad + ht)
                    v2 = v3 + yDir * ht
                    If aSymbol.Flag1 Then
                        v3 += yDir * -ht
                        aSyb.Closed = True
                    End If
                    aSyb.Vertices.AddV(v1)
                    aSyb.Vertices.AddV(v2)
                    aSyb.Vertices.AddV(v3)
                    aSyb.Identifier = "Symbol1"
                    _rVal.Add(aSyb)
                    If aSymbol.Flag2 Then 'all arround
                        ac = New dxeArc With {.PlaneV = aPlane, .ImageGUID = iGUID, .OwnerGUID = eGUID, .Radius = crad,
                            .DisplayStructure = dsp, .CenterV = ip, .Identifier = "Symbol2"}
                        _rVal.Add(ac)
                    End If
                End If
                '====================== the tail
                aTail = New dxePolyline With {
                .ImageGUID = iGUID,
                    .OwnerGUID = eGUID,
                    .PlaneV = aPlane,
                    .DisplayStructure = dsp,
                    .Suppressed = aSymbol.Flag3,
                    .Identifier = "Tail",
                    .Closed = False
                }
                aDir = xDir
                aDir.RotateAbout(aPlane.ZDirection, 45, False)
                v2 = bL.EndPtV
                v1 = v2 + aDir * Math.Sqrt(2 * ht ^ 2)
                v3 = v1.Mirrored(New TLINE(bL))
                aTail.Vertices.AddV(v1)
                aTail.Vertices.AddV(v2)
                aTail.Vertices.AddV(v3)
                _rVal.Add(aTail)
                'the traing note
                If Not String.IsNullOrWhiteSpace(txt3) Then
                    v1 = bL.EndPtV
                    txtTrail = New dxeText(dxxTextTypes.Multiline) With {
                        .ImageGUID = iGUID,
                        .OwnerGUID = eGUID,
                        .TextStyleName = tStyle.Name,
                        .DrawingDirection = dxxTextDrawingDirections.Horizontal,
                        .Vertical = False,
                        .AttributeTag = "Text3",
                        .PlaneV = aPlane,
                        .AlignmentPt1V = v1,
                        .TextHeight = tht,
                        .Rotation = tang,
                        .DisplayStructure = dsp,
                        .Color = tClr,
                        .Identifier = "Text3",
                        .TextString = txt3
                        }
                    If Not bInv Then txtTrail.Alignment = dxxMTextAlignments.MiddleLeft Else txtTrail.Alignment = dxxMTextAlignments.MiddleRight
                    txtTrail.UpdatePath(False, aImage)
                    aRec = txtTrail.Bounds
                    d1 = 0.5 * aRec.Height
                    d1 += 2 * tgap
                    If bBoxIt Then d1 += tgap
                    aTail.UpdatePath()
                    aRec = aTail.Bounds
                    d1 = Math.Tan(45 * Math.PI / 180) * d1
                    If d1 > aRec.Width Then d1 = aRec.Width + 0.05 * xscal
                    txtTrail.Transform(TTRANSFORM.CreateProjection(xDir, d1), True)
                    _rVal.Add(txtTrail)
                End If
                'text boxes
                If bBoxIt Then
                    If txtTrail IsNot Nothing Then
                        aRect = txtTop.BoundingRectangle.Stretched(2 * tgap)
                        aPl = aRect.Perimeter
                        aPl.DisplayStructure = dsp
                        aPl.Suppressed = Not bBoxIt Or txtTop.Suppressed
                        aPl.Identifier = "TextBox1"
                        _rVal.Add(aPl)
                    End If
                    If txtBot IsNot Nothing Then
                        aRect = txtBot.BoundingRectangle.Stretched(2 * tgap)
                        aPl = aRect.Perimeter
                        aPl.DisplayStructure = dsp
                        aPl.Suppressed = Not bBoxIt Or txtBot.Suppressed
                        aPl.Identifier = "TextBox2"
                        _rVal.Add(aPl)
                    End If
                    If txtTrail IsNot Nothing Then
                        aRect = txtTrail.BoundingRectangle.Stretched(2 * tgap)
                        aPl = aRect.Perimeter
                        aPl.DisplayStructure = dsp
                        aPl.Identifier = "TextBox3"
                        aPl.Suppressed = Not bBoxIt Or txtTrail.Suppressed
                        _rVal.Add(aPl)
                    End If
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            End Try
        End Function
        Friend Shared Function zArrowText(aSymbol As dxeSymbol, aImage As dxfImage, aTextID As Integer, bBoxIt As Boolean, aTextPlane As TPLANE, aTextHeight As Double, aTextGap As Double, aAlignmentPlane As TPLANE, ByRef rTextBox As TPLANE, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.BottomCenter, Optional aXCorrection As Double = 0.0, Optional aYCorrection As Double = 0.0) As dxeText
            Dim v1 As TVECTOR
            Dim d1 As Double
            Dim txt As String
            Dim tang As Double
            Dim aPln As TPLANE = aSymbol.PlaneV
            Dim tPln As TPLANE = aTextPlane
            aPln = aSymbol.PlaneV
            If aTextID < 0 Then aTextID = 1
            If aTextID > 4 Then aTextID = 4
            tang = aPln.XDirection.AngleTo(tPln.XDirection, aPln.ZDirection)
            If tang > 90 And tang <= 270 Then
                tPln.Revolve(180)
            End If
            Dim _rVal As New dxeText(dxxTextTypes.Multiline) With {
            .ImageGUID = aImage.GUID,
            .OwnerGUID = aSymbol.GUID,
            .PlaneV = tPln,
            .TextStyleName = aImage.GetOrAdd(dxxReferenceTypes.STYLE, aSymbol.TextStyleName),
            .LayerName = aSymbol.LayerName,
            .TextType = dxxTextTypes.Multiline,
            .DrawingDirection = dxxTextDrawingDirections.Horizontal,
            .Vertical = False
            }
            txt = aSymbol.PropValueStr($"Text{ aTextID}").Trim()
            If txt = "" Then
                txt = "X"
                _rVal.Suppressed = True
            End If
            _rVal.TextString = txt
            _rVal.Color = aSymbol.TextColor
            If _rVal.Color = dxxColors.ByBlock Then _rVal.Color = aSymbol.Color
            _rVal.TextHeight = aTextHeight
            _rVal.Alignment = dxxMTextAlignments.MiddleCenter
            _rVal.Identifier = $"Text{ aTextID}"
            _rVal.UpdatePath(False, aImage)
            rTextBox = _rVal.Bounds
            v1 = aAlignmentPlane.Origin
            If aAlignment > 0 Then
                Dim halgn As dxxHorizontalJustifications
                Dim valgn As dxxVerticalJustifications
                v1 = aAlignmentPlane.AlignmentPoint(aAlignment, True, halgn, valgn)
                If valgn = dxxVerticalJustifications.Top Or valgn = dxxVerticalJustifications.Bottom Then
                    d1 = 0.5 * rTextBox.Height + aTextGap
                    If bBoxIt Then d1 += 0.5 * aTextGap
                    If valgn = dxxVerticalJustifications.Bottom Then d1 *= -1
                    v1 += aAlignmentPlane.YDirection * d1
                End If
                If valgn = dxxVerticalJustifications.Center And (halgn = dxxHorizontalJustifications.Left Or halgn = dxxHorizontalJustifications.Right) Then
                    d1 = 0.5 * rTextBox.Width + aTextGap
                    If bBoxIt Then d1 += 0.5 * aTextGap
                    If halgn = dxxHorizontalJustifications.Left Then d1 *= -1
                    v1 += aAlignmentPlane.XDirection * d1
                End If
                If aXCorrection <> 0 Then
                    v1 += aAlignmentPlane.XDirection * aXCorrection
                End If
                If aYCorrection <> 0 Then
                    v1 += aAlignmentPlane.YDirection * aYCorrection
                End If
            End If
            _rVal.Translate(v1)
            rTextBox.Origin += v1
            rTextBox.Stretch(2 * aTextGap, True, True, False, True)
            Return _rVal
        End Function
        Friend Shared Function zArrowHeads(aSymbol As dxeSymbol, aImage As dxfImage, aSP As TVECTOR, aEP As TVECTOR, aArrowSize As Double, aArrowStyle As dxxArrowStyles, aIdentifier As String, Optional bInvertY As Boolean = False, Optional aCollector As List(Of dxfEntity) = Nothing) As TVECTORS
            Dim rArrowHead As dxxArrowHeadTypes = dxxArrowHeadTypes.ClosedFilled
            Return zArrowHeads(aSymbol, aImage, aSP, aEP, aArrowSize, aArrowStyle, aIdentifier, bInvertY, rArrowHead, aCollector)
        End Function
        Friend Shared Function zArrowHeads(aSymbol As dxeSymbol, aImage As dxfImage, aSP As TVECTOR, aEP As TVECTOR, aArrowSize As Double, aArrowStyle As dxxArrowStyles, aIdentifier As String, bInvertY As Boolean, ByRef rArrowHead As dxxArrowHeadTypes, Optional aCollector As List(Of dxfEntity) = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(aSP, aEP)
            Try
                If aArrowSize <= 0 Or aSymbol Is Nothing Then Return _rVal
                If aArrowStyle < dxxArrowStyles.StdBlocks Then aArrowStyle = dxxArrowStyles.StdBlocks
                If aArrowStyle > dxxArrowStyles.StraightFullOpen Then aArrowStyle = dxxArrowStyles.StraightFullOpen
                Dim aSld As dxeSolid
                Dim lnam As String = String.Empty
                Dim Clr As Integer
                Dim aEnt As dxfEntity
                Dim aFlg As Boolean
                Dim d1 As Double
                Dim aPl As dxePolyline
                Dim vcnt As Integer
                Dim aPln As TPLANE = aSymbol.PlaneV
                Dim tsz As Double
                Dim v1 As TVECTOR = aEP.Clone
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim v4 As TVECTOR
                Dim vecs As TVECTORS
                Dim iTails As dxxArrowTails
                If aSymbol.ArrowType = dxxArrowTypes.Pointer Then
                    iTails = aSymbol.ArrowTails
                End If
                Dim lt As String = String.Empty : Dim ltscl As Double = 0 : Dim lw As dxxLineWeights = dxxLineWeights.Undefined
                aSymbol.LCLGet(lnam, Clr, lt, lw, ltscl)
                Dim xDir As TVECTOR = aSP.DirectionTo(aEP, False, aFlg, d1)
                If aFlg Then Return _rVal
                Dim yDir As TVECTOR = xDir.RotatedAbout(aPln.ZDirection, 90, False).Normalized
                If iTails <= 0 Then
                    If d1 < 1.25 * aArrowSize Then aEP += xDir * (1.25 * aArrowSize - d1)
                Else
                    If d1 < 1.25 * (2 * aArrowSize) Then aEP += xDir * ((1.25 * aArrowSize * 2) - d1)
                End If
                If aArrowStyle = dxxArrowStyles.StdBlocks Then
                    Dim aLn As New dxeLine(aEP, aSP)
                    rArrowHead = aSymbol.ArrowHeadType
                    If rArrowHead <> dxxArrowHeadTypes.None Then
                        aEnt = dxfArrowheads.GetEntitySymbol(aSymbol, aImage, rArrowHead, aLn, aIdentifier, False, aArrowSize)
                        If aEnt IsNot Nothing Then
                            aEP.Update(aLn.StartPtV.X, aLn.StartPtV.Y, aLn.StartPtV.Z)
                            aEnt.LCLSet(lnam, Clr)
                            aEnt.UpdatePath(False, aImage)
                            If aCollector IsNot Nothing Then aCollector.Add(aEnt)
                            _rVal.Append(aEnt.ExtentPts)
                        End If
                    End If
                Else
                    If aArrowStyle = dxxArrowStyles.AngledFull Or aArrowStyle = dxxArrowStyles.AngledFullOpen Or aArrowStyle = dxxArrowStyles.StraightFull Or aArrowStyle = dxxArrowStyles.StraightFullOpen Then
                        vcnt = 4
                    Else
                        vcnt = 3
                    End If
                    Dim f1 As Integer = 1
                    If bInvertY Then f1 *= -1
                    vecs = New TVECTORS(v1.Clone)
                    v3 = v1 + (xDir * -aArrowSize)
                    v2 = v3 + (yDir * (0.2 * aArrowSize * f1))
                    v4 = v2 + (yDir * (2 * 0.2 * aArrowSize * -f1))
                    'vecs.Update(1, aEP)
                    'vecs.Update(2, vecs.Item(1).Projected(xDir, -aArrowSize, True))
                    'vecs.Update(3, vecs.Item(1).Projected(yDir, 0.2 * aArrowSize, True, bInvertY))
                    'vAdd = vecs.Item(2).Projected(yDir, 2 * 0.2 * aArrowSize, True, Not bInvertY)
                    'If vcnt > 3 Then vecs.Update(4, vAdd)
                    vecs.Add(New TVECTOR(v2))
                    vecs.Add(New TVECTOR(v3))
                    If vcnt > 3 Then vecs.Add(New TVECTOR(v4))
                    If aArrowStyle = dxxArrowStyles.AngledFull Or aArrowStyle = dxxArrowStyles.AngledHalf Or aArrowStyle = dxxArrowStyles.AngledFullOpen Or aArrowStyle = dxxArrowStyles.AngledHalfOpen Then
                        'v1 = vecs.Item(3)
                        v3 += xDir * (0.175 * aArrowSize)
                        vecs.SetItem(3, New TVECTOR(v3))
                    End If
                    If aArrowStyle > dxxArrowStyles.StraightFull Then
                        aPl = New dxePolyline With
                        {
                        .PlaneV = aPln,
                         .ImageGUID = aImage.GUID,
                        .OwnerGUID = aSymbol.GUID,
                        .VectorsV = vecs,
                        .Closed = True
                        }
                        aPl.LCLSet(lnam, Clr, dxfLinetypes.Continuous)
                        If aCollector IsNot Nothing Then aCollector.Add(aPl)
                        aEP.Update(v3.X, v3.Y, v3.Z)
                    Else
                        If vcnt = 3 Then
                            aSld = New dxeSolid(New dxfVector(vecs.Item(1)), New dxfVector(vecs.Item(2)), New dxfVector(vecs.Item(3)), aDisplaySettings:=New dxfDisplaySettings(lnam, Clr, dxfLinetypes.Continuous))
                        Else
                            aSld = New dxeSolid(New dxfVector(vecs.Item(1)), New dxfVector(vecs.Item(2)), New dxfVector(vecs.Item(3)), New dxfVector(vecs.Item(4)), aDisplaySettings:=New dxfDisplaySettings(lnam, Clr, dxfLinetypes.Continuous))
                        End If
                        aSld.PlaneV = aPln
                        aSld.ImageGUID = aImage.GUID
                        aSld.OwnerGUID = aSymbol.GUID
                        aSld.PlaneV = aSymbol.PlaneV
                        aSld.Triangular = vcnt = 3
                        If aCollector IsNot Nothing Then aCollector.Add(aSld)
                    End If
                    _rVal.Append(vecs)
                End If
                _rVal.SetItem(1, aSP)
                _rVal.SetItem(2, aEP)
                If iTails > 0 Then
                    tsz = 0.75 * aArrowSize
                    Dim mLn As New TLINE With {.SPT = aSP, .EPT = aEP}
                    v1 = aSP + xDir * tsz
                    v2 = v1 + yDir * (0.2125 * aArrowSize)
                    v2 += xDir * (-0.175 * aArrowSize)
                    v3 = v2 + xDir * -tsz
                    v4 = aSP.Clone
                    vecs = New TVECTORS(v1, v2, v3, v4)
                    If iTails = dxxArrowTails.Open Then
                        aPl = New dxePolyline With {
                            .PlaneV = aPln,
                            .ImageGUID = aImage.GUID,
                            .OwnerGUID = aSymbol.GUID,
                            .VectorsV = vecs,
                            .Closed = True,
                            .Identifier = "Tail1"
                            }
                        aPl.LCLSet(lnam, Clr, dxfLinetypes.Continuous)
                        If aCollector IsNot Nothing Then aCollector.Add(aPl)
                        _rVal.Append(vecs)
                        aPl = aPl.Clone
                        vecs = vecs.Mirrored(mLn)
                        aPl.ImageGUID = aImage.GUID
                        aPl.OwnerGUID = aSymbol.GUID
                        aPl.VectorsV = vecs
                        aPl.Identifier = "Tail2"
                        If aCollector IsNot Nothing Then aCollector.Add(aPl)
                        _rVal.Append(vecs)
                    Else
                        aSld = New dxeSolid With {
                            .PlaneV = aPln,
                            .ImageGUID = aImage.GUID,
                            .OwnerGUID = aSymbol.GUID,
                            .Triangular = False,
                            .Vertex1V = v1.Clone,
                            .Vertex2V = v2.Clone,
                            .Vertex3V = v3.Clone,
                            .Vertex4V = v4.Clone,
                            .DisplayStructure = New TDISPLAYVARS(lnam, dxfLinetypes.Continuous, Clr),
                            .Identifier = "Tail1"
                         }
                        If aCollector IsNot Nothing Then aCollector.Add(aSld)
                        _rVal.Append(vecs)
                        aSld = aSld.Clone
                        vecs = vecs.Mirrored(mLn)
                        aSld.ImageGUID = aImage.GUID
                        aSld.OwnerGUID = aSymbol.GUID
                        aSld.Vertex1V = v1.Clone
                        aSld.Vertex2V = v2.Clone
                        aSld.Vertex3V = v3.Clone
                        aSld.Vertex4V = v4.Clone
                        aSld.Identifier = "Tail2"
                        If aCollector IsNot Nothing Then aCollector.Add(aSld)
                        _rVal.Append(vecs)
                    End If
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
            End Try
        End Function
#End Region 'Shared Methods
    End Class
End Namespace
