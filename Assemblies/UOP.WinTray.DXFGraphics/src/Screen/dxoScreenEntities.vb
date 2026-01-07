Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoScreenEntities
#Region "Members"
        Private _Struc As TSCREEN
#End Region 'Members
#Region "Events"
        Public Event Update()
#End Region 'Events
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                Return _Struc.Entities.Count
            End Get
        End Property
        Friend ReadOnly Property Image As dxfImage
            Get
                If Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then Return dxfEvents.GetImage(_Struc.ImageGUID) Else Return Nothing
            End Get
        End Property
        Friend Property ImageGUID As String
            Get
                Return _Struc.ImageGUID
            End Get
            Set(value As String)
                _Struc.ImageGUID = value
            End Set
        End Property
        Friend Property Strukture As TSCREEN
            Get
                Return _Struc
            End Get
            Set(value As TSCREEN)
                _Struc = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As dxoScreenEntity
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return Nothing
            End If
            Return New dxoScreenEntity(_Struc.Entities.Item(aIndex))
        End Function
        Public Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage Is Nothing And Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then rImage = dxfEvents.GetImage(_Struc.ImageGUID)
            Return rImage IsNot Nothing
        End Function
        Public Sub Clear()
            _Struc.Clear(True)
        End Sub
        Public Function Delete(aHandles As String) As Boolean
            If _Struc.DeleteEntity(aHandles, Image) Then
                RaiseEvent Update()
                Return True
            Else
                Return False
            End If
        End Function
        Public Sub Refresh(Optional aEntityType As dxxScreenEntityTypes = dxxScreenEntityTypes.Undefined)
            Dim aImage As dxfImage = Nothing
            GetImage(aImage)
            If _Struc.Refresh(aImage, New TPENS(0), False, aEntityType) Then
                _Struc = aImage.obj_SCREEN
                RaiseEvent Update()
            End If
        End Sub
        Public Function Remove(aIndex As Integer, Optional aEntityType As dxxScreenEntityTypes = dxxScreenEntityTypes.Undefined) As Boolean
            If _Struc.RemoveEntity(aEntityType, aIndex, Image) Then
                RaiseEvent Update()
                Return True
            Else
                Return False
            End If
        End Function
        Public Function RemoveByType(aEntityType As dxxScreenEntityTypes, bSuppressRedraw As Boolean, Optional aTags As String = "", Optional aDelimiter As String = ",") As Integer
            Dim _rVal As Integer = 0
            If _Struc.RemoveByType(aEntityType, Nothing, bSuppressRedraw, _rVal, aTags, aDelimiter) Then
                RaiseEvent Update()
            End If
            Return _rVal
        End Function
        Public Function aAxis(aPlane As dxfPlane, Optional aScreenFraction As Object = Nothing, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Model, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1the plane to get the directions from
            '#2the fraction of the screen Height to use for the  height (0.01-0.5)
            '#3flag to control if the axis is redraw in the next redraw or is cleared
            '^used to draw and axis symbol to the screen only.
            If TPLANE.IsNull(aPlane) Then Return _rVal
            'On Error Resume Next
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            _rVal = _Struc.DrawAxis(aImage, aPlane.Strukture, aScreenFraction, bPersist, aColor, aDomain, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aCircle(aPointsObj As IEnumerable(Of iVector), aRadius As Object, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = 0, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the point to draw the circle
            '#2the radius of the circle
            '#3either world or screen domain.screen domain  indicates that the passed point is relative to the screen and units are pixels
            '#2flag to control if the circle is redraw in the next redraw or is cleared
            '#3the color to apply
            '^used to draw a circle to the screen only.
            'On Error Resume Next
            aRadius = TVALUES.To_DBL(aRadius, True, 6)
            If aRadius <= 0 Then Return _rVal
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPts As New TVECTORS(0)
            Dim aPln As TPLANE = xPlanePts(aImage, aPointsObj, aDomain, aPts)
            aPln.Height = TVALUES.To_DBL(aRadius)
            _rVal = _Struc.DrawCircle(aImage, aPln, aPts, aDomain, bPersist, bFilled, aColor, aPenWidth, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aLine(aPointsObj As IEnumerable(Of iVector), Optional bDashed As Boolean = False, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional aSize As Integer = -1, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the points to draw lines between
            '#2flag indicated that the lines is dashed
            '#3flag to control if the entity is redraw in the next redraw or is cleared
            '#4the color to apply
            '#5the size to apply
            '^used to draw a line to the screen only.
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPts As New TVECTORS
            xPlanePts(aImage, aPointsObj, aDomain, aPts)
            If aPts.Count <= 0 Then Return _rVal
            _rVal = _Struc.DrawLine(aImage, aPts, aDomain, bDashed, bPersist, aColor, aSize, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aPill(aPointsObj As IEnumerable(Of iVector), aWidth As Object, Optional aHeight As Object = Nothing, Optional aRotation As Double? = Nothing, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = -1, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the point to draw the triangle
            '#2the width of the triangle
            '#3either world or screen domain.screen domain  indicates that the passed point is relative to the screen and units are pixels
            '#2flag to control if the triangle is redraw in the next redraw or is cleared
            '#3the color to apply
            '^used to draw a circle to the screen only.
            'On Error Resume Next
            aWidth = TVALUES.To_DBL(aWidth, True, 6)
            If aWidth = 0 Then Return _rVal
            aHeight = TVALUES.ToDouble(aHeight, True, aWidth, 6)
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPln As New TPLANE("")
            Dim aPts As New TVECTORS
            aPln = xPlanePts(aImage, aPointsObj, aDomain, aPts, aRotation)
            If aPts.Count <= 0 Then Return _rVal
            aPln.Width = TVALUES.To_DBL(aWidth)
            aPln.Height = TVALUES.To_DBL(aHeight)
            _rVal = _Struc.DrawPill(aImage, aPln, aPts, aDomain, bPersist, bFilled, aColor, aPenWidth, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aPoint(aPointsObj As IEnumerable(Of iVector), Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional aSize As Integer = -1, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the point or points to draw
            '#2flag to control if the point is redraw in the next redraw or is cleared
            '#3the color to apply
            '#4the size to apply
            '^used to draw a point to the screen only.
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPts As New TVECTORS
            xPlanePts(aImage, aPointsObj, aDomain, aPts)
            If aPts.Count <= 0 Then Return _rVal
            _rVal = _Struc.DrawPoint(aImage, aPts, aDomain, bPersist, aColor, aSize, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aPointer(aPointsObj As IEnumerable(Of iVector), aWidth As Object, Optional aHeight As Object = Nothing, Optional aRotation As Double? = Nothing, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = -1, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the point to draw the pointer
            '#2the width of the pointer
            '#3either world or screen domain.screen domain  indicates that the passed point is relative to the screen and units are pixels
            '#2flag to control if the pointer is redraw in the next redraw or is cleared
            '#3the color to apply
            '^used to draw a circle to the screen only.
            'On Error Resume Next
            aWidth = TVALUES.To_DBL(aWidth, True, 6)
            If aWidth = 0 Then Return _rVal
            aHeight = TVALUES.ToDouble(aHeight, True, aWidth, 6)
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPts As New TVECTORS
            Dim aPln As TPLANE = xPlanePts(aImage, aPointsObj, aDomain, aPts, aRotation)
            If aPts.Count <= 0 Then Return _rVal
            aPln.Width = TVALUES.To_DBL(aWidth)
            aPln.Height = TVALUES.To_DBL(aHeight)
            _rVal = _Struc.DrawTriangle(aImage, aPln, aPts, aDomain, True, bPersist, aColor, bFilled, aPenWidth, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aRectangle(aPointsObj As IEnumerable(Of iVector), aWidth As Object, Optional aHeight As Object = Nothing, Optional aRotation As Double? = Nothing, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = -1, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the point to draw the rectangle
            '#2the width of the rectangle
            '#3either world or screen domain.screen domain  indicates that the passed point is relative to the screen and units are pixels
            '#2flag to control if the circle is redraw in the next redraw or is cleared
            '#3the color to apply
            '^used to draw a circle to the screen only.
            'On Error Resume Next
            aWidth = TVALUES.To_DBL(aWidth, True, 6)
            If aWidth = 0 Then Return _rVal
            aHeight = TVALUES.ToDouble(aHeight, True, aWidth, 6)
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPln As New TPLANE("")
            Dim aPts As New TVECTORS
            aPln = xPlanePts(aImage, aPointsObj, aDomain, aPts, aRotation)
            aPln.Width = TVALUES.ToDouble(aWidth)
            aPln.Height = TVALUES.ToDouble(aHeight)
            _rVal = _Struc.DrawRectangle(aImage, aPln, aPts, aDomain, bPersist, aColor, bFilled, aPenWidth, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aRectangle(aRect As dxfRectangle, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = -1, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the plane to get the directions and dimensions from
            '#2flag to control if the axis is redraw in the next redraw or is cleared
            '^used to draw and axis symbol to the screen only.
            If aRect Is Nothing Then Return _rVal
            If Not aRect.IsDefined Then Return _rVal
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            _rVal = _Struc.DrawRectangle(aImage, aRect.Strukture, New TVECTORS, dxxDrawingDomains.Model, bPersist, aColor, bFilled, aPenWidth, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aScreenAxis(aPlane As dxfPlane, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.BottomLeft, Optional aScreenFraction As Double = 0.0, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1the plane to get the directions from
            '#2the aligment for the screen
            '#3the fraction of the screen Height to use for the  height (0.01-0.5)
            '#4flag to control if the axis is redraw in the next redraw or is cleared
            '^used to draw and axis symbol to the screen only.
            'On Error Resume Next
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPln As New TPLANE("Screen")
            If TPLANE.IsNull(aPlane) Then aPln = aImage.obj_UCS Else aPln = New TPLANE(aPlane)
            _rVal = _Struc.DrawScreenAxis(aImage, aPln, aAlignment, aScreenFraction, bPersist, aColor, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aScreenText(aScreenPt As dxfVector, aTextString As Object, Optional aScreenFraction As Double = 0.0, Optional bPersist As Boolean = True, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft, Optional aColor As dxxColors = dxxColors.Undefined, Optional aWidthFactor As Double = 0.0, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1the point defined in screen pixels to draw the text
            '#2the string to to draw to the screen
            '#3the fraction of the screen Height to use for the text height (0.01- 0.5)
            '#4flag to control if the screen text is redraw in the next redraw or is cleared
            '#5the alignment of the text with regard the display rectangle (if not point is passed)
            '^used to draw text to the display only.  The text is not added To the saved entities
            '~creates and returns dxeText object.
            'On Error Resume Next
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            _rVal = _Struc.DrawScreenText(aImage, aScreenPt, aTextString, aScreenFraction, bPersist, aAlignment, aColor, aWidthFactor, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Public Function aScreenText(aTextString As String, aAlignment As dxxRectangularAlignments, Optional aScreenFraction As Double = 0.0, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional aWidthFactor As Double = 0.0, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1the point defined in screen pixels to draw the text
            '#1the string to to draw to the screen
            '#2the alignment of the text with regard the display rectangle (if not point is passed)
            '#3the fraction of the screen Height to use for the text height (0.01- 0.5)
            '#4flag to control if the screen text is redraw in the next redraw or is cleared
            '^used to draw text to the display only.  The text is not added To the saved entities
            '~creates and returns dxeText object.
            Try
                Dim aImage As dxfImage = Nothing
                If Not GetImage(aImage) Then Return _rVal
                If aAlignment < 1 Or aAlignment > 9 Then aAlignment = dxxRectangularAlignments.TopLeft
                _rVal = _Struc.DrawScreenText(aImage, Nothing, aTextString, aScreenFraction, bPersist, aAlignment, aColor, aWidthFactor, aTag, bRefreshAll)
                _Struc = aImage.obj_SCREEN
                RaiseEvent Update()
                If Not bPersist Then xDeleteLast(aImage)
            Catch
            End Try
            Return _rVal
        End Function
        Public Function aTriangle(aPointsObj As IEnumerable(Of iVector), aWidth As Object, Optional aHeight As Object = Nothing, Optional aRotation As Double? = Nothing, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = True, Optional aColor As dxxColors = dxxColors.Undefined, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = -1, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            '#1the point to draw the triangle
            '#2the width of the triangle
            '#3either world or screen domain.screen domain  indicates that the passed point is relative to the screen and units are pixels
            '#2flag to control if the triangle is redraw in the next redraw or is cleared
            '#3the color to apply
            '^used to draw a circle to the screen only.
            'On Error Resume Next
            aWidth = TVALUES.To_DBL(aWidth, True, 6)
            If aWidth = 0 Then Return _rVal
            aHeight = TVALUES.ToDouble(aHeight, True, aWidth, 6)
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aImage As dxfImage = Nothing
            If Not GetImage(aImage) Then Return _rVal
            Dim aPln As New TPLANE("")
            Dim aPts As New TVECTORS
            aPln = xPlanePts(aImage, aPointsObj, aDomain, aPts, aRotation)
            If aPts.Count <= 0 Then Return _rVal
            aPln.Width = TVALUES.To_DBL(aWidth)
            aPln.Height = TVALUES.To_DBL(aHeight)
            _rVal = _Struc.DrawTriangle(aImage, aPln, aPts, aDomain, False, bPersist, aColor, bFilled, aPenWidth, aTag, bRefreshAll)
            _Struc = aImage.obj_SCREEN
            RaiseEvent Update()
            If Not bPersist Then xDeleteLast(aImage)
            Return _rVal
        End Function
        Private Sub xDeleteLast(aImage As dxfImage)
            'On Error Resume Next
            If _Struc.Entities.Count > 0 Then
                _Struc.Entities.Remove(_Struc.Entities.Count)
                RaiseEvent Update()
                If aImage IsNot Nothing Then aImage.obj_SCREEN = _Struc
            End If
        End Sub
        Private Function xPlanePts(aImage As dxfImage, aPointsObj As IEnumerable(Of iVector), aDomain As dxxDrawingDomains, ByRef rVectors As TVECTORS, Optional aRotation As Double? = Nothing) As TPLANE
            Dim _rVal As TPLANE
            'On Error Resume Next
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            rVectors = dxfVectors.GetTVECTORS(aPointsObj)
            If aDomain = dxxDrawingDomains.Screen Then
                _rVal = aImage.obj_DISPLAY.pln_DEVICE
            Else
                _rVal = TPLANE.World
            End If
            If rVectors.Count > 0 Then
                '        If aDomain = Screen Then
                '              For i = 1 To rVectors.Count
                '                  rVectors.Members(i - 1).Y = -rVectors.Members(i - 1).Y
                '              Next i
                '        End If
                _rVal.Origin = rVectors.Item(1)
            End If
            If aRotation.HasValue Then
                If aRotation.Value <> 0 Then _rVal.Revolve(aRotation.Value)
            End If
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return $"dxoScreenEntities [{ Count }]"
        End Function
#End Region 'Methods
    End Class 'dxoScreenEntities
End Namespace
