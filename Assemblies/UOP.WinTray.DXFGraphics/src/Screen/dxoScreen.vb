Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoScreen
        Implements idxfSettingsObject
#Region "Members"
        Private _Struc As TSCREEN
        Friend ImagePtr As WeakReference
        Private _Entities As dxoScreenEntities
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage)
            If aImage IsNot Nothing Then
                _Struc = New TSCREEN(aImage.GUID)
                ImagePtr = New WeakReference(aImage)
            Else
                _Struc = New TSCREEN("")
            End If
            _Struc.TextStyle = New TTABLEENTRY(dxxReferenceTypes.STYLE, "Screen") With {.IsGlobal = True}
        End Sub
        Friend Sub New(aStructure As TSCREEN)
            _Struc = aStructure
        End Sub
#End Region 'Constructors
#Region "Properties"


        Public Property Properties As dxoProperties Implements idxfSettingsObject.Properties
            Get
                Return New dxoProperties(_Struc.Props)
            End Get
            Set(value As dxoProperties)
                _Struc.Props.CopyValues(value)
            End Set
        End Property

        Public ReadOnly Property SettingType As dxxReferenceTypes Implements idxfSettingsObject.SettingType
            Get
                Return dxxSettingTypes.SCREENSETTINGS
            End Get
        End Property
        Public ReadOnly Property Name As String Implements idxfSettingsObject.Name
            Get
                Return dxfEnums.DisplayName(SettingType)
            End Get
        End Property
        Public Property AxisColor As dxxColors
            Get
                Return _Struc.AxisColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.AxisColor, value)
            End Set
        End Property
        Public ReadOnly Property Bitmap As dxfBitmap
            Get
                '^returns a copy of the current screen entities overlay bitmap
                Dim aImg As dxfImage = Nothing
                GetImage(aImg)
                Dim _rVal As dxfBitmap = Nothing
                If aImg IsNot Nothing Then
                    _Struc = aImg.obj_SCREEN
                    _rVal = aImg.GetBitmap(True)
                End If
                Return _rVal
            End Get
        End Property
        Public Property BoundingRectangles As Boolean
            Get
                '^controls if entity bounding rectangles are draw to the screen
                Return _Struc.BoundingRectangles
            End Get
            Set(value As Boolean)
                '^controls if entity bounding rectangles are draw to the screen
                SetValue(dxxScreenProperties.BoundingRectangles, value)
            End Set
        End Property
        Public ReadOnly Property Entities As dxoScreenEntities
            Get
                If _Entities Is Nothing Then
                    _Entities = New dxoScreenEntities
                    AddHandler _Entities.Update, AddressOf _Entities_Update
                    _Entities.Strukture = _Struc
                End If
                _Entities.ImageGUID = ImageGUID
                Return _Entities
            End Get
        End Property
        Public Property EntitySymbolColor As dxxColors
            Get
                Return _Struc.EntitySymbolColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.EntitySymbolColor, value)
            End Set
        End Property
        Public Property ExtentPts As Boolean
            Get
                '^controls if entity extent points are draw to the screen
                Return _Struc.ExtentPts
            End Get
            Set(value As Boolean)
                '^controls if entity extent points are draw to the screen
                SetValue(dxxScreenProperties.ExtentPts, value)
            End Set
        End Property
        Public Property ExtentRectangle As Boolean
            Get
                '^controls if the images extent rectangle is draw to the screen
                Return _Struc.ExtentRectangle
            End Get
            Set(value As Boolean)
                '^controls if the images extent rectangle is draw to the screen
                SetValue(dxxScreenProperties.ExtentRectangle, value)
            End Set
        End Property
        Public Property ExtentRectangleColor As dxxColors
            Get
                Return _Struc.ExtentRectangleColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.ExtentRectangleColor, value)
            End Set
        End Property
        Friend Property ImageGUID As String
            Get
                Return _Struc.ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then _Struc.ImageGUID = "" Else _Struc.ImageGUID = value.Trim
                If ImagePtr Is Nothing And Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then
                    Dim img As dxfImage = dxfEvents.GetImage(_Struc.ImageGUID)
                    If img IsNot Nothing Then ImagePtr = New WeakReference(img)
                End If
                _Struc.ImageGUID = value
                _Struc.TextStyle.ImageGUID = value
            End Set
        End Property
        Public Property LineColor As dxxColors
            Get
                Return _Struc.LineColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.LineColor, value)
            End Set
        End Property
        Public Property Linetype As String
            Get
                Return _Struc.Linetype
            End Get
            Set(value As String)
                value = Trim(value)
                If value = String.Empty Then
                    value = dxfLinetypes.Continuous
                End If
                SetValue(dxxScreenProperties.LineType, value)
            End Set
        End Property
        Public Property LTScale As Double
            Get
                '^the linetype scale to apply to screen linetypes
                Return _Struc.LTScale
            End Get
            Set(value As Double)
                '^the linetype scale to apply to screen linetypes
                SetValue(dxxScreenProperties.LTScale, value)
            End Set
        End Property
        Public Property OCSs As Boolean
            Get
                '^controls if entity coordinate axes are drawn to the screen
                Return _Struc.OCSs
            End Get
            Set(value As Boolean)
                '^controls if entity coordinate axes are drawn to the screen
                SetValue(dxxScreenProperties.OCSs, value)
            End Set
        End Property
        Public Property OCSSize As Double
            Get
                '^the size to draw entity ocs axis as a fraction of the display size
                Return _Struc.OCSSize
            End Get
            Set(value As Double)
                '^the size to draw entity ocs axis as a fraction of the display size
                SetValue(dxxScreenProperties.OCSSize, value)
            End Set
        End Property
        Public Property PointColor As dxxColors
            Get
                Return _Struc.PointColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.PointColor, value)
            End Set
        End Property
        Public Property PointerColor As dxxColors
            Get
                Return _Struc.PointerColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.PointerColor, value)
            End Set
        End Property
        Public Property PointSize As Integer
            Get
                '^the size to draw points on the screen in pixels
                Return _Struc.PointSize
            End Get
            Set(value As Integer)
                '^the size to draw points on the screen in pixels
                SetValue(dxxScreenProperties.PointSize, value)
            End Set
        End Property
        Public Property RectangleColor As dxxColors
            Get
                Return _Struc.RectangleColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.RectangleColor, value)
            End Set
        End Property
        Friend Property Strukture As TSCREEN
            Get
                Return _Struc
            End Get
            Set(value As TSCREEN)
                _Struc = value
                If _Entities IsNot Nothing Then _Entities.Strukture = _Struc
            End Set
        End Property
        Public Property Suppressed As Boolean
            Get
                Return _Struc.Suppressed
            End Get
            Set(value As Boolean)
                SetValue(dxxScreenProperties.Suppressed, value)
            End Set
        End Property
        Public Property TextBoxes As Boolean
            '^controls if entity text boxes are added to screen text
            Get
                Return _Struc.TextBoxes
            End Get
            Set(value As Boolean)
                SetValue(dxxScreenProperties.TextBoxes, value)
            End Set
        End Property
        Public Property TextColor As dxxColors
            Get
                Return _Struc.TextColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.TextColor, value)
            End Set
        End Property
        Public Property TextSize As Double
            Get
                '^the size to draw entity text as a fraction of the display size
                Return _Struc.TextSize
            End Get
            Set(value As Double)
                '^the size to draw text as a fraction of the display size
                SetValue(dxxScreenProperties.TextSize, value)
            End Set
        End Property
        Public ReadOnly Property TextStyle As dxoStyle
            Get
                Return New dxoStyle(obj_TextStyle)
            End Get
        End Property
        Friend Property obj_TextStyle As TTABLEENTRY
            Get
                _Struc.TextStyle.IsGlobal = True
                _Struc.TextStyle.ImageGUID = ImageGUID
                Return _Struc.TextStyle
            End Get
            Set(value As TTABLEENTRY)
                If value.EntryType <> dxxReferenceTypes.STYLE Then Return
                If value.Props.Count <= 0 Then Return
                _Struc.TextStyle = value
                _Struc.TextStyle.IsGlobal = True
                _Struc.TextStyle.ImageGUID = ImageGUID
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
                _Struc.ImageGUID = rImage.GUID
                ImagePtr = New WeakReference(rImage)
            End If
            Return rImage IsNot Nothing
        End Function
        Friend Sub Dispose()
            If _Entities IsNot Nothing Then
                RemoveHandler _Entities.Update, AddressOf _Entities_Update
                _Entities.Clear()
            End If
            _Entities = Nothing
        End Sub
        Public Function Bounds() As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            Dim aImg As dxfImage = Nothing
            GetImage(aImg)
            _rVal = New dxfRectangle
            If aImg IsNot Nothing Then _rVal.Strukture = aImg.obj_DISPLAY.pln_DEVICE
            Return _rVal
        End Function
        Public Property CircleColor As dxxColors
            Get
                Return _Struc.CircleColor
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.CircleColor, value)
            End Set
        End Property


        Public Function InchesPerPixel(Optional bIncludeZoom As Boolean = False, Optional aImage As dxfImage = Nothing) As Double
            Return 1 / PixelsPerInch(bIncludeZoom, aImage)
        End Function
        Public Function InchesToPixels(aValue As Object, Optional bIncludeZoom As Boolean = False, Optional aImage As dxfImage = Nothing) As Double
            Return TVALUES.To_DBL(aValue, True, 6) * PixelsPerInch(bIncludeZoom, aImage)
        End Function
        Public Function NumberVectors(aVertices As Object, Optional aTextScaleFactor As Double = 0.0, Optional aColor As dxxColors = dxxColors.Blue, Optional bShowMemberIndices As Boolean = False, Optional bSaveToFile As Boolean = False, Optional aPropsToList As dxxVectorProperties = dxxVectorProperties.Undefined, Optional bPersist As Boolean = True) As String
            '#1the vectors to number
            '#2a scale factor for the text. default size is 1.5% of the current view height
            '#3a color for the text
            '#4flag to include the index of the vector in the text
            '#5flag to output the text in a file if a write request is made
            '#6flag to include the row and column numbers of the vectors in the text
            '#7flag to incude the tag and flag strings of the vectors in the text
            '^lables the passed vectors with screen text
            Dim aImg As dxfImage = Nothing
            GetImage(aImg)
            Return _Struc.NumberVectors(aImg, aVertices, aTextScaleFactor, aColor, bShowMemberIndices, bSaveToFile, aPropsToList, bPersist)
        End Function
        Public Function NumberVectors(aVertices As colDXFVectors, Optional aTextScaleFactor As Double = 0.0, Optional aStartIndex As Integer = 0, Optional aEndIndex As Integer = 0, Optional aColor As dxxColors = dxxColors.Blue, Optional bShowMemberIndices As Boolean = False, Optional bSaveToFile As Boolean = False, Optional aPropsToList As dxxVectorProperties = dxxVectorProperties.Undefined, Optional bPersist As Boolean = True) As String
            '#1the vectors to number
            '#2a scale factor for the text. default size is 1.5% of the current view height
            '#3a color for the text
            '#4flag to include the index of the vector in the text
            '#5flag to output the text in a file if a write request is made
            '#6flag to include the row and column numbers of the vectors in the text
            '#7flag to incude the tag and flag strings of the vectors in the text
            '^lables the passed vectors with screen text
            Dim aImg As dxfImage = Nothing
            GetImage(aImg)
            Return _Struc.NumberVectors(aImg, aVertices, aTextScaleFactor, aColor, bShowMemberIndices, bSaveToFile, aPropsToList, bPersist, aStartIndex, aEndIndex)
        End Function
        Public Function PixelsPerInch(Optional bIncludeZoom As Boolean = False, Optional aImage As dxfImage = Nothing) As Double
            Dim _rVal As Double = 0.0
            If GetImage(aImage) Then
                _rVal = aImage.obj_DISPLAY.DPI
                If bIncludeZoom Then _rVal *= aImage.obj_DISPLAY.ZoomFactor
            Else
                _rVal = 96
            End If
            Return _rVal
        End Function
        Public Function Plane() As dxfPlane
            Dim _rVal As dxfPlane
            Dim aImage As dxfImage = Nothing
            _rVal = New dxfPlane
            If Not GetImage(aImage) Then _rVal.Strukture = aImage.obj_DISPLAY.pln_DEVICE
            Return _rVal
        End Function
        Public Sub Refresh(Optional aEntityType As dxxScreenEntityTypes = dxxScreenEntityTypes.Undefined)
            Dim aImg As dxfImage = Nothing
            GetImage(aImg)
            Dim aPens As New TPENS(0)
            If _Struc.Refresh(aImg, aPens, False, aEntityType) Then
                _Struc = aImg.obj_SCREEN
                If _Entities IsNot Nothing Then _Entities.Strukture = _Struc
            End If
        End Sub
        Private Function SetValue(aIndex As dxxScreenProperties, aNewValue As Object) As Boolean
            Dim _rVal As Boolean = False
            Dim sKey As String = dxfEnums.Description(aIndex)
            Dim aProp As TPROPERTY = _Struc.Props.GetMember(sKey)
            Dim pVal As Object = aNewValue
            If aProp.Name = "" Then Return False 'prop not found
            If Not _Struc.ValidatePropertyValue(aIndex, pVal) Then Return False
            If aProp.SetVal(pVal) Then
                _Struc.Props.UpdateProperty = aProp
                Dim aImage As dxfImage = Nothing
                If GetImage(aImage) Then aImage.RespondToSettingChange(Me, aProp)
                If _Entities IsNot Nothing Then _Entities.Strukture = _Struc
                Return True
            Else
                Return False
            End If
        End Function
        Public Function Vector(Optional aX As Double = 0, Optional aY As Double = 0, Optional aImage As dxfImage = Nothing) As dxfVector
            Dim _rVal As New dxfVector(aX, aY)
            If GetImage(aImage) Then _rVal.Strukture = aImage.obj_DISPLAY.pln_DEVICE.Vector(aX, aY)
            Return _rVal
        End Function
#End Region 'Methods
#Region "_Entities_EventHandlers"
        Private Sub _Entities_Update()
            _Struc = _Entities.Strukture
        End Sub
#End Region '_Entities_EventHandlers
    End Class 'dxoScreen
End Namespace
