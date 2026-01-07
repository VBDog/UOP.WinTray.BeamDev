
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics.Structures

#Region "Structures"
    Friend Structure TSCREENSETTINGS
        Implements ICloneable
#Region "Members"
        Public AxisColor As dxxColors
        Public AxisSize As Double
        Public CircleColor As dxxColors
        Public DeviceHwnd As Long
        Public DeviceRectangle As TPLANE
        Public LineColor As dxxColors
        Public Linetype As String
        Public PixelsPerUnit As Integer
        Public PointColor As dxxColors
        Public PointerColor As dxxColors
        Public PointSize As Integer
        Public RectangleColor As dxxColors
        Public TextColor As dxxColors
        Public TextSize As Double
        Public ViewRectangle As TPLANE
        Public ZoomFactor As Double
        Public TextBoxes As Boolean
#End Region 'Members


#Region "Constructors"

        Public Sub New(aSettings As TSCREENSETTINGS)
            'init -----------------------------------------
            AxisColor = aSettings.AxisColor
            AxisSize = aSettings.AxisSize
            CircleColor = aSettings.CircleColor
            DeviceHwnd = aSettings.DeviceHwnd
            DeviceRectangle = New TPLANE(aSettings.DeviceRectangle)
            LineColor = aSettings.LineColor
            Linetype = aSettings.Linetype
            PixelsPerUnit = aSettings.PixelsPerUnit
            PointColor = aSettings.PointColor
            PointerColor = aSettings.PointerColor
            PointSize = aSettings.PointSize
            RectangleColor = aSettings.RectangleColor
            TextColor = aSettings.TextColor
            TextSize = aSettings.TextSize
            ViewRectangle = aSettings.ViewRectangle
            ZoomFactor = aSettings.ZoomFactor
            TextBoxes = aSettings.TextBoxes
            'init -----------------------------------------
        End Sub


#End Region 'Constructors

#Region "Methods"
        Public Function Clone() As TSCREENSETTINGS
            Return New TSCREENSETTINGS(Me)

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSCREENSETTINGS(Me)
        End Function


        Public Overrides Function ToString() As String
            Return $"TSCREENSETTINGS"
        End Function


#End Region 'Methods

    End Structure 'TSCREENSETTINGS
    Friend Structure TSCREENENTITY
        Implements ICloneable
#Region "Members"
        Public DxfHandle As ULong
        Public Alignment As dxxRectangularAlignments
        Public Color As Integer
        Public Dashed As Boolean
        Public Domain As dxxDrawingDomains
        Public Drawn As Boolean
        Public EntityGUID As String
        Public EntityType As dxxGraphicTypes
        Public Filled As Boolean
        Public Handle As String
        Public Height As Double
        Public Paths As TPATHS
        Public PenWidth As Integer
        Public Persist As Boolean
        Public Plane As TPLANE
        Public ScreenFraction As Double
        Public Size As Integer
        Public Tag As String
        Public TextString As String
        Public Type As dxxScreenEntityTypes
        Public Vectors As TVECTORS
        Public WidthFactor As Double
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aType As dxxScreenEntityTypes = dxxScreenEntityTypes.Undefined, Optional aScreenFraction As Double = 0)
            'init ----------------------------------------------
            DxfHandle = 0
            Alignment = dxxRectangularAlignments.TopLeft
            Color = 0
            Dashed = False
            Domain = dxxDrawingDomains.Screen
            Drawn = False
            EntityGUID = ""
            EntityType = dxxGraphicTypes.Undefined
            Filled = False
            Handle = ""
            Height = 0
            Paths = New TPATHS(dxxDrawingDomains.Screen, "")
            PenWidth = 0
            Persist = False
            Plane = TPLANE.World
            ScreenFraction = 0
            Size = 0
            Tag = ""
            TextString = ""
            Type = dxxScreenEntityTypes.Undefined
            Vectors = New TVECTORS(0)
            WidthFactor = 0
            'init ----------------------------------------------
            Type = aType
            ScreenFraction = aScreenFraction
        End Sub

        Public Sub New(aEntity As TSCREENENTITY)
            'init ----------------------------------------------
            DxfHandle = aEntity.DxfHandle
            Alignment = aEntity.Alignment
            Color = aEntity.Color
            Dashed = aEntity.Dashed
            Domain = aEntity.Domain
            Drawn = aEntity.Drawn
            EntityGUID = aEntity.EntityGUID
            EntityType = aEntity.EntityType
            Filled = aEntity.Filled
            Handle = aEntity.Handle
            Height = aEntity.Height
            Paths = New TPATHS(aEntity.Paths)
            PenWidth = aEntity.PenWidth
            Persist = aEntity.Persist
            Plane = New TPLANE(aEntity.Plane)
            ScreenFraction = aEntity.ScreenFraction
            Size = aEntity.Size
            Tag = aEntity.Tag
            TextString = aEntity.TextString
            Type = aEntity.Type
            Vectors = New TVECTORS(aEntity.Vectors)
            WidthFactor = aEntity.WidthFactor
            'init ----------------------------------------------
        End Sub
#End Region 'Constructors
#Region "Methods"
        Public Function Clone() As TSCREENENTITY
            Return New TSCREENENTITY(Me)

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSCREENENTITY(Me)
        End Function

        Public Sub Clear()

            Paths = New TPATHS(0)
            Vectors = New TVECTORS(0)

        End Sub

        Public Overrides Function ToString() As String
            Return $"TSCREENENTITY [{ dxfEnums.Description(Type) }]"
        End Function


#End Region 'Methods

    End Structure 'TSCREENENTITY
    Friend Structure TSCREENENTITIES
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TSCREENENTITY
        Public ImageGUID As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aImageGUID As String = "")
            'init --------------------
            _Init = True
            ReDim _Members(-1)
            ImageGUID = aImageGUID
            'init --------------------


        End Sub
        Public Sub New(aEntities As TSCREENENTITIES)
            'init --------------------
            _Init = True
            ReDim _Members(-1)
            ImageGUID = aEntities.ImageGUID
            'init --------------------

            If aEntities._Init Then _Members = aEntities._Members.Clone()
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function ToList() As List(Of TSCREENENTITY)
            If Count <= 0 Then Return New List(Of TSCREENENTITY)
            Return _Members.ToList()
        End Function
        Public Function Item(aIndex As Integer) As TSCREENENTITY
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TSCREENENTITY(dxxScreenEntityTypes.Undefined)
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TSCREENENTITY)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Overrides Function ToString() As String
            Return $"TSCREENENTITIES [ {Count }]"
        End Function
        Public Function Clone() As TSCREENENTITIES
            Return New TSCREENENTITIES(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSCREENENTITIES(Me)
        End Function
        Public Sub Clear()

            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Function Add(aEntity As TSCREENENTITY) As Boolean
            If Count >= Integer.MaxValue Then Return False
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aEntity
            Return True
        End Function
        Public Sub Remove(aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Or Count = 0 Then Return
            If aIndex = Count Then
                System.Array.Resize(_Members, Count - 1)
                Return
            End If
            If Count = 1 And aIndex = 1 Then
                Clear()
                Return
            End If
            Dim i As Integer
            Dim j As Integer = 0
            Dim newMems(0 To Count - 1) As TSCREENENTITY
            For i = 1 To Count
                If i <> aIndex Then
                    newMems(j) = Item(i)
                    j += 1
                End If
            Next i
            _Members = newMems
        End Sub
#End Region 'Methods
    End Structure 'TSCREENENTITIES
    Friend Structure TSCREEN
        Implements ICloneable
#Region "Members"

        Public Entities As TSCREENENTITIES
        Public EntityIndex As Integer
        Private _ImageGUID As String
        Public Props As TPROPERTIES
        Public TextStyle As TTABLEENTRY
#End Region 'Members
#Region "Properties"
        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                _ImageGUID = value
                TextStyle.ImageGUID = value
            End Set
        End Property
        Public ReadOnly Property ViewTransform As TTRANSFORMATION
            Get
                Return New TTRANSFORMATION
            End Get
        End Property
#End Region 'Properties
#Region "Constructors"
        Public Sub New(aImageGUID As String)
            'init ---------------------------------
            Entities = New TSCREENENTITIES(_ImageGUID)
            EntityIndex = 0
            _ImageGUID = aImageGUID
            Props = dxpProperties.GetReferenceProps(dxxSettingTypes.SCREENSETTINGS)
            TextStyle = New TTABLEENTRY(dxxReferenceTypes.STYLE, "SCREENSTYLE") With {.IsGlobal = True}
            'init ---------------------------------

        End Sub

        Public Sub New(aScreen As TSCREEN)
            'init ---------------------------------
            Entities = New TSCREENENTITIES(aScreen.Entities)
            EntityIndex = aScreen.EntityIndex
            _ImageGUID = aScreen.ImageGUID
            Props = New TPROPERTIES(aScreen.Props)
            TextStyle = New TTABLEENTRY(aScreen.TextStyle)
            'init ---------------------------------

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Image As dxfImage
            Get
                If _ImageGUID <> "" Then Return dxfEvents.GetImage(_ImageGUID) Else Return Nothing
            End Get
        End Property
        Public Property AxisColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.AxisColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.AxisColor, value)
            End Set
        End Property
        Public Property CircleColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.CircleColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.CircleColor, value)
            End Set
        End Property
        Public Property BoundingRectangles As Boolean
            Get
                '^controls if entity bounding rectangles are draw to the screen
                Return GetPropVal(dxxScreenProperties.BoundingRectangles)
            End Get
            Set(value As Boolean)
                '^controls if entity bounding rectangles are draw to the screen
                SetValue(dxxScreenProperties.BoundingRectangles, value)
            End Set
        End Property
        Public Property EntitySymbolColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.EntitySymbolColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.EntitySymbolColor, value)
            End Set
        End Property
        Public Property ExtentRectangle As Boolean
            Get
                '^controls if the images extent rectangle is draw to the screen
                Return GetPropVal(dxxScreenProperties.ExtentRectangle)
            End Get
            Set(value As Boolean)
                '^controls if the images extent rectangle is draw to the screen
                SetValue(dxxScreenProperties.ExtentRectangle, value)
            End Set
        End Property
        Public Property ExtentRectangleColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.ExtentRectangleColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.ExtentRectangleColor, value)
            End Set
        End Property
        Public Property LineColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.LineColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.LineColor, value)
            End Set
        End Property
        Public Property Linetype As String
            Get
                Return GetPropVal(dxxScreenProperties.LineType)
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
                Return GetPropVal(dxxScreenProperties.LTScale)
            End Get
            Set(value As Double)
                '^the linetype scale to apply to screen linetypes
                SetValue(dxxScreenProperties.LTScale, value)
            End Set
        End Property
        Public Property OCSs As Boolean
            Get
                '^controls if entity coordinate axes are drawn to the screen
                Return GetPropVal(dxxScreenProperties.OCSs)
            End Get
            Set(value As Boolean)
                '^controls if entity coordinate axes are drawn to the screen
                SetValue(dxxScreenProperties.OCSs, value)
            End Set
        End Property
        Public Property OCSSize As Double
            Get
                '^the size to draw entity ocs axis as a fraction of the display size
                Return GetPropVal(dxxScreenProperties.OCSSize)
            End Get
            Set(value As Double)
                '^the size to draw entity ocs axis as a fraction of the display size
                SetValue(dxxScreenProperties.OCSSize, value)
            End Set
        End Property
        Public Property PointColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.PointColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.PointColor, value)
            End Set
        End Property
        Public Property PointerColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.PointerColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.PointerColor, value)
            End Set
        End Property
        Public Property PointSize As Integer
            Get
                '^the size to draw points on the screen in pixels
                Return GetPropVal(dxxScreenProperties.PointSize)
            End Get
            Set(value As Integer)
                '^the size to draw points on the screen in pixels
                SetValue(dxxScreenProperties.PointSize, value)
            End Set
        End Property
        Public Property RectangleColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.RectangleColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.RectangleColor, value)
            End Set
        End Property
        Public Property TextBoxes As Boolean
            Get
                '^controls if entity text boxes are added to screen text
                Return GetPropVal(dxxScreenProperties.TextBoxes)
            End Get
            Set(value As Boolean)
                '^controls if entity text boxes are added to screen text
                SetValue(dxxScreenProperties.TextBoxes, value)
            End Set
        End Property
        Public Property TextColor As dxxColors
            Get
                Return GetPropVal(dxxScreenProperties.TextColor)
            End Get
            Set(value As dxxColors)
                SetValue(dxxScreenProperties.TextColor, value)
            End Set
        End Property
        Public Property TextSize As Double
            Get
                '^the size to draw entity text as a fraction of the display size
                Return GetPropVal(dxxScreenProperties.TextSize)
            End Get
            Set(value As Double)
                '^the size to draw text as a fraction of the display size
                SetValue(dxxScreenProperties.TextSize, value)
            End Set
        End Property
        Public Property Suppressed As Boolean
            Get
                Return GetPropVal(dxxScreenProperties.Suppressed)
            End Get
            Set(value As Boolean)
                SetValue(dxxScreenProperties.Suppressed, value)
            End Set
        End Property
        Public Property ExtentPts As Boolean
            Get
                '^controls if entity extent points are draw to the screen
                Return GetPropVal(dxxScreenProperties.ExtentPts)
            End Get
            Set(value As Boolean)
                '^controls if entity extent points are draw to the screen
                SetValue(dxxScreenProperties.ExtentPts, value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Clone() As TSCREEN
            Return New TSCREEN(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSCREEN(Me)
        End Function
        Public Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage Is Nothing And _ImageGUID <> "" Then rImage = Image
            Return rImage IsNot Nothing
        End Function
        Public Sub Clear(Optional bRetainSettings As Boolean = False)

            Entities = New TSCREENENTITIES(ImageGUID)

            If Not bRetainSettings Then Props = dxpProperties.GetReferenceProps(dxxSettingTypes.SCREENSETTINGS)
            Dim aImage As dxfImage = Nothing
            If GetImage(aImage) Then aImage.RaiseScreenEvent(dxxScreenEventTypes.Clear)

        End Sub
        Public Function AddEntity(aEntity As TSCREENENTITY) As String
            If Entities.Add(aEntity) Then
                EntityIndex += 1
                aEntity.Handle = TVALUES.To_Handle(EntityIndex)
                Entities.SetItem(Entities.Count, aEntity)
                Return aEntity.Handle
            Else
                Return String.Empty
            End If
        End Function
        Public Function DeleteEntity(aHandles As String, Optional aImage As dxfImage = Nothing) As Boolean
            Dim rRemoved As Boolean = False
            If Not GetImage(aImage) Or aHandles = "" Then Return False
            Dim idx As Integer
            Dim aEnt As TSCREENENTITY
            Dim aEnts As New TSCREENENTITIES
            Dim bKeep As Boolean
            Dim aPens As New TPENS(0)
            Dim aSets As TSCREENSETTINGS = CurrentSettings(aImage)
            Dim bAll As Boolean
            bAll = String.Compare(aHandles, "ALL", True) = 0
            aEnts.Clear()
            For idx = 1 To Entities.Count
                aEnt = Entities.Item(idx)
                bKeep = Not bAll
                If bKeep Then
                    If TLISTS.Contains(aEnt.Handle, aHandles) Then
                        rRemoved = True
                        bKeep = False
                    End If
                End If
                If bKeep Then
                    aEnts.Add(aEnt)
                Else
                    Entity_Draw(aImage, aSets, aPens, bRefreshSettings:=False, aEnt, aSuppressRefresh:=True, bErase:=True)
                End If
            Next idx
            Entities = aEnts
            If aImage IsNot Nothing And rRemoved Then
                aImage.obj_SCREEN = Me
                aImage.obj_DISPLAY.DrawBitmap(aImage)
            End If
            Return rRemoved
        End Function
        Public Function CurrentSettings(Optional aImage As dxfImage = Nothing) As TSCREENSETTINGS
            GetImage(aImage)
            Dim _rVal As New TSCREENSETTINGS With {
                    .AxisSize = OCSSize,
                    .AxisColor = AxisColor
                }
            If _rVal.AxisColor = 0 Or _rVal.AxisColor = -1 Or _rVal.AxisColor = 256 Then
                _rVal.AxisColor = dxxColors.Blue
                AxisColor = _rVal.AxisColor
            End If
            _rVal.TextSize = TextSize
            If _rVal.TextSize < 0.005 Or _rVal.TextSize > 0.5 Then
                If _rVal.TextSize < 0.005 Then _rVal.TextSize = 0.005
                If _rVal.TextSize > 0.5 Then _rVal.TextSize = 0.5
                TextSize = _rVal.TextSize
            End If
            _rVal.TextColor = TextColor
            If _rVal.TextColor = 0 Or _rVal.TextColor = -1 Or _rVal.TextColor = 256 Then
                _rVal.TextColor = dxxColors.Blue
                TextColor = _rVal.TextColor
            End If
            _rVal.RectangleColor = RectangleColor
            If _rVal.RectangleColor = 0 Or _rVal.RectangleColor = -1 Or _rVal.RectangleColor = 256 Then
                _rVal.RectangleColor = dxxColors.Blue
                RectangleColor = _rVal.RectangleColor
            End If
            _rVal.CircleColor = CircleColor
            If _rVal.CircleColor = 0 Or _rVal.CircleColor = -1 Or _rVal.CircleColor = 256 Then
                _rVal.CircleColor = dxxColors.Blue
                CircleColor = _rVal.CircleColor
            End If
            _rVal.PointerColor = PointerColor
            If _rVal.PointerColor = 0 Or _rVal.PointerColor = -1 Or _rVal.PointerColor = 256 Then
                _rVal.PointerColor = dxxColors.Blue
                PointerColor = _rVal.PointerColor
            End If
            _rVal.LineColor = LineColor
            If _rVal.LineColor = 0 Or _rVal.LineColor = -1 Or _rVal.LineColor = 256 Then
                _rVal.LineColor = dxxColors.Blue
                LineColor = _rVal.LineColor
            End If
            _rVal.PointColor = PointColor
            If _rVal.PointColor = 0 Or _rVal.PointColor = -1 Or _rVal.PointColor = 256 Then
                _rVal.PointColor = dxxColors.Blue
                PointColor = _rVal.PointColor
            End If
            _rVal.PointSize = PointSize
            If _rVal.PointSize < 0 Or _rVal.PointSize > 50 Then
                If _rVal.PointSize < 0 Then _rVal.PointSize = 0
                If _rVal.PointSize > 50 Then _rVal.PointSize = 50
                PointSize = _rVal.PointSize
            End If
            _rVal.Linetype = Linetype
            If _rVal.Linetype = "" Then
                _rVal.Linetype = dxfLinetypes.Continuous
                Linetype = _rVal.Linetype
            End If
            _rVal.TextBoxes = TextBoxes
            _rVal.ZoomFactor = 1
            _rVal.PixelsPerUnit = 96
            If aImage IsNot Nothing Then
                Dim aDsp As TDISPLAY = aImage.obj_DISPLAY
                _rVal.DeviceRectangle = aDsp.pln_DEVICE
                _rVal.ViewRectangle = aDsp.pln_VIEW
                _rVal.DeviceHwnd = aDsp.DeviceHwnd
                aImage.obj_SCREEN = Me
                _rVal.ZoomFactor = aDsp.ZoomFactor
                _rVal.PixelsPerUnit = aImage.bmp_Screen.DPI
            End If
            Return _rVal
        End Function
        Public Function SetValue(aIndex As dxxScreenProperties, aValue As Object) As Boolean
            Dim rProp As TPROPERTY = TProperty.Null
            Return SetValue(aIndex, aValue, rProp)
        End Function
        Public Function SetValue(aIndex As dxxScreenProperties, aValue As Object, ByRef rProp As TPROPERTY) As Boolean
            rProp = Nothing
            If aValue Is Nothing Then Return False
            Dim sKey As String = dxfEnums.Description(aIndex)
            rProp = Props.GetMember(sKey)
            If rProp.Name = "" Then Return False 'prop not found
            Dim pVal As Object = aValue
            If Not ValidatePropertyValue(aIndex, pVal) Then Return False
            If rProp.SetVal(aValue) Then
                Props.UpdateProperty = rProp
                Return True
            Else
                Return False
            End If
        End Function
        Public Function ValidatePropertyValue(aIndex As dxxScreenProperties, aValue As Object) As Boolean
            Dim pname As String = dxfEnums.Description(aIndex)
            Select Case aIndex
                Case dxxScreenProperties.Suppressed
                    aValue = TVALUES.ToBoolean(aValue)
                Case dxxScreenProperties.BoundingRectangles
                    aValue = TVALUES.ToBoolean(aValue)
                Case dxxScreenProperties.BoundingRectangles
                    aValue = TVALUES.ToBoolean(aValue)
                Case dxxScreenProperties.OCSs
                    aValue = TVALUES.ToBoolean(aValue)
                Case dxxScreenProperties.ExtentPts
                    aValue = TVALUES.ToBoolean(aValue)
                Case dxxScreenProperties.ExtentRectangle
                    aValue = TVALUES.ToBoolean(aValue)
                Case dxxScreenProperties.TextBoxes
                    aValue = TVALUES.ToBoolean(aValue)
                Case dxxScreenProperties.EntitySymbolColor
                Case dxxScreenProperties.TextColor
                    aValue = TVALUES.ToLong(aValue)
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.AxisColor ' = 9
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.RectangleColor ' = 10
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.ExtentRectangleColor ' = 11
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.PointColor ' = 12
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.OCSSize ' = 13
                    aValue = TVALUES.ToInteger(aValue, True, PointSize, 1, 20)
                Case dxxScreenProperties.TextSize ' = 14
                    aValue = TVALUES.ToDouble(aValue, True, Props.ValueD(pname), 4, 0.005, 0.5)
                Case dxxScreenProperties.PointSize ' = 15
                    aValue = TVALUES.ToInteger(aValue, True, Props.ValueD(pname), 1, 20)
                Case dxxScreenProperties.CircleColor ' = 16
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.PointerColor ' = 17
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.LineColor ' = 18
                    If aValue = 0 Or aValue = -1 Or aValue = 256 Then Return False
                Case dxxScreenProperties.LineType ' = 19
                    Dim aLt As TTABLEENTRY
                    aValue = Trim(aValue)
                    If aValue = "" Then aValue = dxfLinetypes.Continuous
                    If String.Compare(aValue, dxfLinetypes.ByLayer, True) = 0 Or String.Compare(aValue, dxfLinetypes.ByBlock, True) = 0 Then Return False
                    If String.Compare(aValue, dxfLinetypes.Continuous, True) <> 0 Then
                        aLt = dxfLinetypes.GlobalLinetypes.Entry(aValue)
                        If aLt.Index <= 0 Then Return False
                        aValue = aLt.Name
                    End If
                Case dxxScreenProperties.LTScale ' = 20
                    aValue = TVALUES.ToDouble(aValue, True, LTScale, aValueControl:=mzValueControls.PositiveNonZero)
                Case Else
                    Return False
            End Select
            Return True
        End Function
        Public Function GetPropVal(aIndex As dxxScreenProperties) As Object
            Return Props.Value(dxfEnums.Description(aIndex))
        End Function
        Public Function DrawAxis(aImage As dxfImage, aPlane As TPLANE, Optional aScreenFraction As Object = Nothing, Optional bPersist As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Model, Optional aTag As String = Nothing, Optional bRefreshAll As Boolean = False) As String
            Dim _rVal As String = String.Empty
            'On Error Resume Next
            If Not GetImage(aImage) Then Return _rVal
            '#1the subject image
            '#2the plane to get the directions from
            '#3the fraction of the screen Height to use for the  height (0.01-0.5)
            '#4flag to control if the axis is redraw in the next redraw or is cleared
            '^used to draw and axis symbol to the screen only.
            If aScreenFraction IsNot Nothing Then
                aScreenFraction = TVALUES.ToDouble(aScreenFraction, True, aPrecis:=6, aMinVal:=0.01, aMaxVal:=0.5)
            Else
                aScreenFraction = 0
            End If
            Dim aScrnEnt As New TSCREENENTITY With {
                    .Domain = aDomain,
                    .Type = dxxScreenEntityTypes.Axis,
                    .Color = aColor,
                    .Persist = bPersist,
                    .ScreenFraction = aScreenFraction,
                    .Plane = aPlane,
                    .Tag = aPlane.Tag
                }
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag
            _rVal = SaveEntity(aScrnEnt, aImage, bRefreshAll)
            Return _rVal
        End Function
        Public Function DrawCircle(aImage As dxfImage, aPlane As TPLANE, ByRef rVectors As TVECTORS, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = False, Optional bFilled As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional aPenWidth As Integer = 0, Optional aTag As String = Nothing, Optional bRefreshAll As Boolean = False) As String
            'On Error Resume Next
            If Not GetImage(aImage) Then Return String.Empty
            '#1the subject image
            '#2the point to draw
            '#3the domain of the point
            '#4flag to control if the point is redraw in the next redraw or is cleared
            '#5the color to apply
            '#6the size to apply
            '^used to draw a point to the screen only.
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aScrnEnt As New TSCREENENTITY With {
                    .Domain = aDomain,
                    .Type = dxxScreenEntityTypes.Circle,
                    .Color = aColor,
                    .Persist = bPersist,
                    .Plane = aPlane,
                    .Vectors = rVectors.Clone,
                    .Filled = bFilled,
                    .PenWidth = TVALUES.ToInteger(aPenWidth, True, 0, aMaxVal:=100)
                }
            If rVectors.Count > 0 Then aScrnEnt.Plane.Origin = rVectors.Item(1)
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag
            Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
        End Function
        Public Function DrawLine(aImage As dxfImage, aPoints As TVECTORS, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bDashed As Boolean = False, Optional bPersist As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional aSize As Integer = -1, Optional aTag As String = Nothing, Optional bRefreshAll As Boolean = False) As String
            'On Error Resume Next
            If Not GetImage(aImage) Or aPoints.Count <= 0 Then Return String.Empty
            '#1the subject image
            '#2the points to draw
            '#3the domain of the point
            '#4flag to control if the point is redraw in the next redraw or is cleared
            '#5the color to apply
            '#6the size to apply
            '^used to draw a point to the screen only.
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aScrnEnt As New TSCREENENTITY With {
                    .Domain = aDomain,
                    .Type = dxxScreenEntityTypes.Line,
                    .Color = aColor,
                    .Persist = bPersist,
                    .Vectors = aPoints.Clone,
                    .Size = aSize,
                    .PenWidth = aSize,
                    .Dashed = bDashed
                }
            aScrnEnt.Plane.Origin = aPoints.Item(1)
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag
            Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
        End Function
        Public Function DrawPill(aImage As dxfImage, aPlane As TPLANE, aVectors As TVECTORS, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = False, Optional bFilled As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional aPenWidth As Integer = 0, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            'On Error Resume Next
            If Not GetImage(aImage) Then Return String.Empty
            '#1the subject image
            '#2the point to draw
            '#3the domain of the point
            '#4flag to control if the point is redraw in the next redraw or is cleared
            '#5the color to apply
            '#6the size to apply
            '^used to draw a point to the screen only.
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aScrnEnt As New TSCREENENTITY With {
                    .Domain = aDomain,
                    .Type = dxxScreenEntityTypes.Pill,
                    .Color = aColor,
                    .Persist = bPersist,
                    .Plane = aPlane,
                    .Vectors = aVectors,
                    .Filled = bFilled,
                    .PenWidth = TVALUES.ToInteger(aPenWidth, True, aDefault:=0, aMaxVal:=100)
                }
            If aVectors.Count > 0 Then aScrnEnt.Plane.Origin = aVectors.Item(1)
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag.ToString
            Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
        End Function
        Public Function DrawPoint(aImage As dxfImage, aPoints As TVECTORS, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional aSize As Integer = -1, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            'On Error Resume Next
            If Not GetImage(aImage) Or aPoints.Count <= 0 Then Return String.Empty
            '#1the subject image
            '#2the points to draw
            '#3the domain of the point
            '#4flag to control if the point is redraw in the next redraw or is cleared
            '#5the color to apply
            '#6the size to apply
            '^used to draw a point to the screen only.
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aScrnEnt As New TSCREENENTITY With {
                    .Domain = aDomain,
                    .Type = dxxScreenEntityTypes.Point,
                    .Color = aColor,
                    .Persist = bPersist,
                    .Vectors = aPoints,
                    .Size = aSize
                }
            aScrnEnt.Plane.Origin = aPoints.Item(1)
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag.ToString
            Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
        End Function
        Public Function DrawRectangle(aImage As dxfImage, aRectangle As TPLANE, aVectors As TVECTORS, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPersist As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = 0, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            'On Error Resume Next
            If Not GetImage(aImage) Then Return String.Empty
            '#1the subject image
            '#2the plane to get the directions and dimensions from
            '#3the domain to apply to the rectangle
            '#4flag to control if the rectangle is redraw in the next redraw or is cleared
            '^used to draw a rectangle to the screen only.
            If Not (aRectangle.Width > 0 And aRectangle.Height > 0) Then Return String.Empty
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            Dim aScrnEnt As New TSCREENENTITY With {
                        .Domain = aDomain,
                        .Type = dxxScreenEntityTypes.Rectangle,
                        .Color = aColor,
                        .Persist = bPersist,
                    .PenWidth = TVALUES.ToInteger(aPenWidth, True, 0, aMaxVal:=100),
                    .Plane = aRectangle,
                        .Vectors = aVectors,
                    .Tag = aRectangle.Tag,
                .Filled = bFilled
                }
            If aVectors.Count > 0 Then
                aScrnEnt.Plane.Origin = aVectors.Item(1)
            Else
                aScrnEnt.Vectors.Add(aScrnEnt.Plane.Origin)
            End If
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag.ToString
            Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
        End Function
        Public Function DrawScreenText(aImage As dxfImage, aScreenPt As dxfVector, aTextString As Object, Optional aScreenFraction As Double = 0.0, Optional bPersist As Boolean = False, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft, Optional aColor As dxxColors = dxxColors.Undefined, Optional aWidthFactor As Double = 0.0, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen) As String
            If Not GetImage(aImage) Then Return String.Empty
            '#1the subject image
            '#2the string to to draw to the screen
            '#3the fraction of the screen Height to use for the text height (0.01- 0.5)
            '#4flag to control if the screen text is redraw in the next redraw or is cleared
            '#5the alignment of the text with regard the display rectangle
            '^used to draw text to the display only.  The text is not added To the saved entities
            Try
                If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
                If aTextString.ToString().Trim() = "" Then Return String.Empty
                If aAlignment < dxxRectangularAlignments.TopLeft Or aAlignment > dxxRectangularAlignments.BottomRight Then aAlignment = dxxRectangularAlignments.TopLeft
                'initialize the mtext object
                Dim aScrnEnt As New TSCREENENTITY With {
                        .Domain = aDomain,
                        .Type = dxxScreenEntityTypes.Text,
                        .Color = aColor,
                        .Persist = bPersist,
                        .ScreenFraction = aScreenFraction,
                        .Alignment = aAlignment,
                        .TextString = TVALUES.To_STR(aTextString, bTrim:=True),
                        .WidthFactor = aWidthFactor
                    }
                If aScrnEnt.WidthFactor <= 0 Then aScrnEnt.WidthFactor = 1
                If aScrnEnt.WidthFactor < 0.01 Then aScrnEnt.WidthFactor = 0.01
                If aWidthFactor > 100 Then aScrnEnt.WidthFactor = 100
                aScrnEnt.Tag = TVALUES.To_STR(aTag)
                Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
        Public Function DrawScreenAxis(aImage As dxfImage, aPlane As TPLANE, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.BottomLeft, Optional aScreenFraction As Double = 0.0, Optional bPersist As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            'On Error Resume Next
            If Not GetImage(aImage) Then Return String.Empty
            '#1the subject image
            '#2the plane to get the directions from
            '#3the fraction of the screen Height to use for the  height (0.01-0.5)
            '#4flag to control if the axis is redraw in the next redraw or is cleared
            '^used to draw and axis symbol to the screen only.
            If aAlignment < 1 Or aAlignment > 9 Then aAlignment = dxxRectangularAlignments.BottomLeft
            Dim aScrnEnt As New TSCREENENTITY With {
                    .Alignment = aAlignment,
                    .Domain = dxxDrawingDomains.Screen,
                    .Type = dxxScreenEntityTypes.Axis,
                    .Color = aColor,
                    .Persist = bPersist,
                    .ScreenFraction = aScreenFraction,
                    .Plane = aPlane,
                    .Tag = aPlane.Tag
                }
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag.ToString()
            Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
        End Function
        Public Function DrawTriangle(aImage As dxfImage, aPlane As TPLANE, aVectors As TVECTORS, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Screen, Optional bPointer As Boolean = False, Optional bPersist As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional bFilled As Boolean = False, Optional aPenWidth As Integer = 0, Optional aTag As Object = Nothing, Optional bRefreshAll As Boolean = False) As String
            'On Error Resume Next
            If Not GetImage(aImage) Then Return String.Empty
            '#1the subject image
            '#2the plane to get the directions and dimensions from
            '#3the domain to apply to the rectangle
            '#4flag to control if the rectangle is redraw in the next redraw or is cleared
            '^used to draw a rectangle to the screen only.
            If Not (aPlane.Width > 0 And aPlane.Height > 0) Then Return String.Empty
            Dim aScrnEnt As New TSCREENENTITY
            If aDomain < dxxDrawingDomains.Screen Or aDomain > dxxDrawingDomains.Model Then aDomain = dxxDrawingDomains.Model
            aScrnEnt.Domain = aDomain
            If Not bPointer Then
                aScrnEnt.Type = dxxScreenEntityTypes.Triangle
            Else
                aScrnEnt.Type = dxxScreenEntityTypes.Pointer
            End If
            aScrnEnt.Color = aColor
            aScrnEnt.Persist = bPersist
            aScrnEnt.PenWidth = TVALUES.ToInteger(aPenWidth, True, aDefault:=0, aMaxVal:=100)
            aScrnEnt.Plane = aPlane
            aScrnEnt.Vectors = aVectors
            If aVectors.Count > 0 Then aScrnEnt.Plane.Origin = aVectors.Item(1)
            aScrnEnt.Tag = aPlane.Tag
            If aTag IsNot Nothing Then aScrnEnt.Tag = aTag.ToString
            aScrnEnt.Filled = bFilled
            Return SaveEntity(aScrnEnt, aImage, bRefreshAll)
        End Function
        Public Function NumberVectors(aImage As dxfImage, aVertices As IEnumerable(Of iVector), Optional aTextScaleFactor As Double = 0.0, Optional aColor As dxxColors = dxxColors.Blue, Optional bShowMemberIndices As Boolean = False, Optional bSaveToFile As Boolean = False, Optional aPropsToList As dxxVectorProperties = dxxVectorProperties.Undefined, Optional bPersist As Boolean = True, Optional aStartIndex As Integer = 0, Optional aEndIndex As Integer = 0) As String
            Dim _rVal As String = String.Empty
            '#1the subject image
            '#2the subject screen
            '#3the vectors to number
            '#4a scale factor for the text. default size is 1.5% of the current view height
            '#5a color for the text
            '#6flag to include the index of the vector in the text
            '#7flag to output the text in a file if a write request is made
            '#8flag to include the row and column numbers of the vectors in the text
            '#9flag to incude the tag and flag strings of the vectors in the text
            '^lables the passed vectors with screen text
            'On Error Resume Next
            If aVertices Is Nothing Or Not GetImage(aImage) Then Return String.Empty
            Dim i As Integer
            Dim j As Integer
            Dim tht As Double
            Dim v1 As dxfVector
            Dim aTxt As dxeText
            Dim txt As String
            Dim ttxt As String
            Dim pval As String
            Dim stxt As New TSCREENENTITY
            Dim tVals As New TVALUES
            Dim nVals As New TVALUES
            Dim aPens As New TPENS(0)
            Dim si As Integer = aStartIndex
            Dim ei As Integer = aEndIndex
            Dim aPts As New colDXFVectors(dxfVectors.GetTVERTICES(aVertices))
            _rVal = aPts.Count
            If aPts.Count <= 0 Then Return _rVal
            stxt.EntityType = dxxGraphicTypes.Text
            stxt.Type = dxxScreenEntityTypes.Text
            If aColor <> dxxColors.ByBlock And aColor <> dxxColors.ByLayer And aColor <> dxxColors.Undefined Then stxt.Color = aColor
            stxt.ScreenFraction = 0.015
            tht = aImage.obj_DISPLAY.pln_VIEW.Height * 0.015
            If aTextScaleFactor > 0 Then
                tht *= aTextScaleFactor
                stxt.ScreenFraction *= aTextScaleFactor
            End If
            tVals = TVALUES.BitCode_Decompose(aPropsToList)
            If tVals.Count > 0 Then
                nVals = tVals
                For j = 1 To tVals.Count
                    nVals.SetMember(j - 1, dxfUtils.VectorPropertyName(TVALUES.To_INT(tVals.Member(j - 1))))
                Next j
            End If
            If si = 0 Then si = 1
            If si > aPts.Count Then si = aPts.Count
            If ei = 0 Then ei = aPts.Count
            If ei > aPts.Count Then ei = aPts.Count
            Dim stp As Integer = 1
            If si > ei Then stp = -1
            For i = si To ei Step stp
                v1 = aPts.Item(i)
                If Not bShowMemberIndices Then txt = i Else txt = v1.Index
                If tVals.Count > 0 Then
                    ttxt = ""
                    'txt = txt & "\H0.75X;"
                    For j = 1 To tVals.Count
                        pval = dxfVector.PropertyValue(v1, TVALUES.To_INT(tVals.Member(j - 1))).ToString()
                        If pval <> "" Then ttxt += $"\P{nVals.Member(j - 1)}={ pval}"
                    Next j
                    If ttxt <> "" Then
                        'txt = txt & ttxt
                        txt += "\H0.6X;" & ttxt
                    End If
                End If
                If bSaveToFile Then
                    aTxt = aImage.Entities.Add(aImage.EntityTool.Create_Text(v1, "\fTxt.shx;" & txt, tht, dxxMTextAlignments.MiddleCenter, aColor:=aColor, bSuppressEffects:=True))
                    TLISTS.Add(_rVal, aTxt.GUID)
                Else
                    aTxt = New dxeText(dxxTextTypes.Multiline) With {
                    .InsertionPt = v1,
                    .TextString = "\fTxt.shx;" & txt,
                    .Alignment = dxxMTextAlignments.MiddleCenter,
                    .Color = dxxColors.Blue,
                    .TextHeight = tht,
                    .LineWeight = dxxLineWeights.ByDefault,
                    .Domain = dxxDrawingDomains.Screen}
                    aImage.RaiseOverlayEvent(False, aTxt)
                    stxt.TextString = txt
                    TLISTS.Add(_rVal, stxt.Handle)
                    stxt.Alignment = dxxRectangularAlignments.MiddleCenter
                    stxt.Plane.Origin = v1.Strukture
                    stxt.Persist = bPersist
                    TLISTS.Add(_rVal, AddEntity(stxt))
                End If
            Next i
            ' aImage.obj_SCREEN = Me
            If Not bSaveToFile Then Refresh(aImage, aPens)
            Return _rVal
        End Function
        Public Function Refresh(aImage As dxfImage, ByRef ioPens As TPENS, Optional aSuppressRefresh As Boolean = False, Optional aEntityType As dxxScreenEntityTypes = dxxScreenEntityTypes.Undefined) As Boolean
            Dim _rVal As Boolean
            If Not GetImage(aImage) Then Return False
            aImage.obj_SCREEN = Me
            Dim bPensPassed As Boolean = ioPens.Count > 0
            Dim entPaths As TPATHS
            Dim nPaths As TPATHS
            Dim v1 As TVECTOR
            Dim scrEnt As TSCREENENTITY
            Dim aEnt As TSCREENENTITY
            Dim aDsp As TDISPLAY = aImage.obj_DISPLAY
            Dim aEnts As New TSCREENENTITIES
            Dim dcnt As Integer = 0
            Dim aSets As TSCREENSETTINGS = CurrentSettings(aImage)
            Dim bRefr As Boolean
            Dim hndl As String
            aEnts.Clear()
            'If Entities.Count > 0 Or (ExtentRectangle And Not Suppressed) Or Image.pth_UCSICON.Count > 0 Then
            '    aImage.RaiseScreenEvent(dxxScreenEventTypes.Clear)
            'End If
            For i As Integer = 1 To Entities.Count
                aEnt = Entities.Item(i)
                If Not aSuppressRefresh Then
                    bRefr = dcnt >= 10
                End If
                hndl = aEnt.Handle
                If Entity_Draw(aImage, aSets, ioPens, bRefreshSettings:=False, ioEnt:=aEnt, aSuppressRefresh:=Not bRefr, bSuppressImageEvents:=True) Then
                    _rVal = True
                    Dim dxse As New dxoScreenEntity(aEnt)
                    If hndl = "" Then
                        aEnt.Handle = TVALUES.To_Handle(EntityIndex)
                        aImage.RaiseScreenEvent(dxxScreenEventTypes.ScreenEntityDrawn, aScreenEntity:=dxse)
                    Else
                        aImage.RaiseScreenEvent(dxxScreenEventTypes.ScreenEntityReDrawn, aScreenEntity:=dxse)
                    End If
                    aEnt.DxfHandle = dxse.DxfHandle
                    dcnt += 1
                    If dcnt >= 10 Then
                        If Not aSuppressRefresh Then aImage.obj_DISPLAY.DrawBitmap(aImage)
                        dcnt = 0
                    End If
                    If aEnt.Persist Then
                        aEnts.Add(aEnt)
                    End If
                    'Application.DoEvents()
                End If
            Next i
            Entities = aEnts
            If ExtentRectangle And Not Suppressed Then
                Dim aPln As TPLANE = aDsp.rec_EXTENTS.Clone
                If aPln.DirectionsAreDefined Then
                    entPaths = New TPATHS(dxxDrawingDomains.Screen)
                    _rVal = True
                    aEnt.Color = ExtentRectangleColor
                    v1 = aPln.Origin ' dsp_WorldToDevice(aDsp, aPln.Origin)
                    'aPln.Origin = v1
                    '            pln_ScaleUp( aPln, asets.PixelsPerUnit * asets.ZoomFactor, 0, v1)
                    entPaths.Clear()
                    Dim aPth As TPATH = TPATH.RECTANGLE(aPln, False)
                    aPth.Color = aEnt.Color
                    aPth.Linetype = dxfLinetypes.Continuous
                    entPaths.Add(aPth)
                    entPaths.Domain = dxxDrawingDomains.Model
                    nPaths = RenderPaths(aImage, ioPens, entPaths, bReturnDrawn:=False, bIsDeviceCoords:=False, bBitmapOutput:=Not aSuppressRefresh, bIgnoreVisibility:=False, bSuppressImageEvents:=True)
                    scrEnt = New TSCREENENTITY(dxxScreenEntityTypes.Rectangle)
                    aImage.RaiseScreenEvent(dxxScreenEventTypes.ExtentRectangleDrawn, aScreenEntity:=New dxoScreenEntity(scrEnt))
                End If
            End If
            'ucsicon
            If aImage.pth_UCSICON.Count > 0 Then
                _rVal = True
                entPaths = aImage.pth_UCSICON
                RenderPaths(aImage, ioPens, entPaths, bReturnDrawn:=False, bIsDeviceCoords:=True, bBitmapOutput:=Not aSuppressRefresh, bIgnoreVisibility:=False, bSuppressImageEvents:=True)
                aImage.RaiseScreenEvent(dxxScreenEventTypes.UCSDrawn, aScreenEntity:=New dxoScreenEntity(New TSCREENENTITY(dxxScreenEntityTypes.Axis)))
            End If
            aImage.obj_SCREEN = Me
            If Not aSuppressRefresh Then
                aImage.obj_DISPLAY.DrawBitmap(aImage)
            End If
            If _rVal Then
                aImage.RaiseScreenEvent(dxxScreenEventTypes.Refresh)
            End If
            Return _rVal
        End Function
        Public Function DrawEntitySymbols(aImage As dxfImage, ByRef ioDisplay As TDISPLAY, aEnt As dxfEntity, Optional aRectanges As Object = Nothing, Optional aExtpts As Object = Nothing, Optional aAxes As Object = Nothing) As Boolean
            Dim ioPixelSize As Integer = -1
            Dim ioColor As dxxColors = dxxColors.Undefined
            Dim ioAxisSize As Double = 0
            Return DrawEntitySymbols(aImage, ioDisplay, aEnt, aRectanges, aExtpts, aAxes, ioPixelSize, ioColor, ioAxisSize)
        End Function
        Public Function DrawEntitySymbols(aImage As dxfImage, ByRef ioDisplay As TDISPLAY, aEnt As dxfEntity, aRectanges As Object, aExtpts As Object, aAxes As Object, ByRef ioPixelSize As Integer, ByRef ioColor As dxxColors, ByRef ioAxisSize As Double) As Boolean
            Dim _rVal As Boolean
            If Not GetImage(aImage) Then Return False
            Try
                If ioAxisSize <= 0 Then ioAxisSize = OCSSize
                If ioColor = dxxColors.Undefined Then ioColor = EntitySymbolColor
                If ioPixelSize < 0 Then ioPixelSize = PointSize
            Catch ex As Exception
            End Try
            Dim bRectanges As Boolean = TVALUES.ToBoolean(aRectanges, BoundingRectangles)
            Dim bExtpts As Boolean = TVALUES.ToBoolean(aExtpts, ExtentPts)
            Dim bAxes As Boolean = TVALUES.ToBoolean(aAxes, OCSs)
            _rVal = (bAxes Or bRectanges Or bExtpts) And Not Suppressed
            If Not _rVal Then Return _rVal
            If bAxes Then
                Entity_OCS(aImage, ioDisplay, aEnt, ioPixelSize, ioColor, ioAxisSize)
            End If
            If bRectanges Then
                Entity_Rectangle(aImage, ioDisplay, aEnt, ioColor)
            End If
            If bExtpts Then
                Entity_ExtentPoints(aImage, ioDisplay, aEnt, ioPixelSize, ioColor)
            End If
            Return _rVal
        End Function
        Friend Function RemoveByType(aScreen As TSCREEN, aEntityType As dxxScreenEntityTypes, aImage As dxfImage, bSuppressRedraw As Boolean, ByRef rCount As Integer, Optional aTags As String = "", Optional aDelimiter As String = ",") As TSCREEN
            Dim _rVal As TSCREEN = aScreen
            rCount = 0
            Dim i As Integer
            Dim aKeepers As New TSCREENENTITIES
            Dim aEnt As New TSCREENENTITY
            Dim eTypes As String = String.Empty
            Dim bTags As Boolean
            Dim bKeep As Boolean
            Dim bAll As Boolean
            Dim aSets As New TSCREENSETTINGS
            Dim aPens As New TPENS(0)
            If Not GetImage(aImage) Then Return _rVal
            bTags = aTags <> ""
            aKeepers.Clear()
            bAll = aEntityType = dxxScreenEntityTypes.All
            aSets = aScreen.CurrentSettings(aImage)
            If bAll And Not bTags Then
                rCount = aScreen.Entities.Count
            Else
                If TVALUES.BitCode_Decompose(aEntityType, eTypes).Count > 0 Or bAll Then
                    For i = 1 To aScreen.Entities.Count
                        aEnt = aScreen.Entities.Item(i)
                        If bTags Then
                            bKeep = Not TLISTS.Contains(aEnt.Tag, aTags, aDelimiter)
                        Else
                            bKeep = True
                        End If
                        If bKeep Then
                            If bAll Then
                                bKeep = False
                            Else
                                bKeep = Not TLISTS.Contains(aEnt.Type, eTypes)
                            End If
                        End If
                        If bKeep Then
                            aKeepers.Add(aEnt)
                        Else
                            aScreen.Entity_Draw(aImage, aSets, aPens, False, aEnt, True, True)
                        End If
                    Next i
                End If
                rCount = aScreen.Entities.Count - aKeepers.Count
            End If
            If rCount > 0 Then
                _rVal.Entities = aKeepers
                aImage.obj_SCREEN = _rVal
                If bAll And Not bTags Then
                    aImage.obj_SCREEN.Refresh(aImage, aPens, False)
                Else
                    aImage.obj_DISPLAY.DrawBitmap(aImage)
                End If
            End If
            Return _rVal
        End Function
        Public Function RemoveByType(aEntityType As dxxScreenEntityTypes, aImage As dxfImage, bSuppressRedraw As Boolean, ByRef rCount As Integer, Optional aTags As String = "", Optional aDelimiter As String = ",") As Boolean
            Dim _rVal As Boolean
            rCount = 0
            Dim i As Integer
            Dim aKeepers As New TSCREENENTITIES
            Dim aEnt As TSCREENENTITY
            Dim eTypes As String = String.Empty
            Dim bTags As Boolean
            Dim bKeep As Boolean
            Dim bAll As Boolean
            Dim aSets As New TSCREENSETTINGS
            Dim aPens As New TPENS(0)
            If Not GetImage(aImage) Then Return _rVal
            bTags = aTags <> ""
            aKeepers.Clear()
            bAll = aEntityType = dxxScreenEntityTypes.All
            aSets = CurrentSettings(aImage)
            If bAll And Not bTags Then
                rCount = Entities.Count
            Else
                If TVALUES.BitCode_Decompose(aEntityType, eTypes).Count > 0 Or bAll Then
                    For i = 1 To Entities.Count
                        aEnt = Entities.Item(i)
                        If bTags Then
                            bKeep = Not TLISTS.Contains(aEnt.Tag, aTags, aDelimiter)
                        Else
                            bKeep = True
                        End If
                        If bKeep Then
                            If bAll Then
                                bKeep = False
                            Else
                                bKeep = Not TLISTS.Contains(aEnt.Type, eTypes)
                            End If
                        End If
                        If bKeep Then
                            aKeepers.Add(aEnt)
                        Else
                            Entity_Draw(aImage, aSets, aPens, False, aEnt, True, True)  'erase and don't keep
                            _rVal = True
                            rCount += 1
                        End If
                    Next i
                End If
            End If
            If rCount > 0 Then
                _rVal = True
                Entities = aKeepers
                aImage.obj_SCREEN = Me
                If bAll And Not bTags Then
                    aImage.obj_SCREEN.Refresh(aImage, aPens, False)
                Else
                    aImage.obj_DISPLAY.DrawBitmap(aImage)
                End If
            End If
            Return _rVal
        End Function
        Public Function RemoveEntity(aEntityType As dxxScreenEntityTypes, aIndex As Integer, Optional aImage As dxfImage = Nothing) As Boolean
            Dim _rVal As New Boolean
            If Not GetImage(aImage) Then Return False
            Dim idx As Integer
            Dim aEnt As TSCREENENTITY
            Dim aEnts As New TSCREENENTITIES
            Dim cnt As Integer
            Dim bKeep As Boolean
            Dim aSets As TSCREENSETTINGS = Nothing
            Dim aPens As New TPENS(0)
            aEnts.Clear()
            For idx = 1 To Entities.Count
                aEnt = Entities.Item(idx)
                bKeep = True
                If aEnt.Type = aEntityType Or aEntityType = dxxScreenEntityTypes.Undefined Then
                    cnt += 1
                    If cnt = aIndex Then
                        _rVal = True
                        bKeep = False
                    End If
                End If
                If bKeep Then
                    aEnts.Add(aEnt)
                Else
                    Entity_Draw(aImage, aSets, aPens, False, aEnt, True, True)  'erase and don't keep
                    _rVal = True
                End If
            Next idx
            Entities = aEnts
            If aImage IsNot Nothing And _rVal Then
                aImage.obj_SCREEN = Me
                aImage.obj_DISPLAY.DrawBitmap(aImage)
            End If
            Return _rVal
        End Function
        Public Function RemoveEntsByGUID(aImage As dxfImage, aGUIDs As TVALUES) As Boolean
            Dim _rVal As Boolean
            If aGUIDs.Count <= 0 Then Return _rVal
            GetImage(aImage)
            Dim i As Integer
            Dim aEnt As New TSCREENENTITY
            Dim aEnts As New TSCREENENTITIES
            Dim bKeep As Boolean
            Dim j As Integer
            aEnts.Clear()
            For i = 1 To Entities.Count
                aEnt = Entities.Item(i)
                bKeep = True
                If aEnt.EntityGUID <> "" Then
                    For j = 1 To aGUIDs.Count
                        If String.Compare(aEnt.EntityGUID, aGUIDs.Member(j - 1).ToString(), True) = 0 Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                End If
                If bKeep Then
                    aEnts.Add(aEnt)
                Else
                    _rVal = True
                End If
            Next i
            Entities = aEnts
            If _rVal Then
                If aImage IsNot Nothing Then aImage.obj_SCREEN = Me
            End If
            Return _rVal
        End Function
        Public Function RenderPaths(aImage As dxfImage, ByRef ioPens As TPENS, ByRef ioPaths As TPATHS, Optional bReturnDrawn As Boolean = False, Optional bIsDeviceCoords As Boolean = False, Optional bBitmapOutput As Boolean = False, Optional bIgnoreVisibility As Boolean = False, Optional bSuppressImageEvents As Boolean = False, Optional aEntity As dxoScreenEntity = Nothing) As TPATHS
            If Not GetImage(aImage) Or ioPaths.Count <= 0 Then Return Nothing
            'drawit
            Return aImage.Render_Paths(ioPens, ioPaths, bIsDeviceCoords, bBitmapOutput, bIgnoreVisibility, bScreenPath:=True)
        End Function
        Public Function SaveEntity(ByRef ioEnt As TSCREENENTITY, aImage As dxfImage, bRefreshAll As Boolean) As String
            Dim _rVal As String = String.Empty
            If Not GetImage(aImage) Then Return String.Empty
            Dim aSets As New TSCREENSETTINGS
            Dim aPens As New TPENS(0)
            _rVal = AddEntity(ioEnt)
            If _rVal <> "" Then
                If bRefreshAll Then
                    Refresh(aImage, aPens, False)
                Else
                    Entity_Draw(aImage, aSets, aPens, True, ioEnt, False)
                    Entities.SetItem(Entities.Count, ioEnt)
                End If
                aImage.obj_SCREEN = Me
            End If
            Return _rVal
        End Function
        Public Function Entity_Draw(aImage As dxfImage, ByRef ioSettings As TSCREENSETTINGS, ByRef ioPens As TPENS, bRefreshSettings As Boolean, ByRef ioEnt As TSCREENENTITY, Optional aSuppressRefresh As Boolean = False, Optional bErase As Boolean = False, Optional bSuppressImageEvents As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If Not GetImage(aImage) Then Return _rVal
            aImage.obj_SCREEN = Me
            Dim entPaths As TPATHS
            Dim tClr As dxxColors
            Dim nPaths As TPATHS
            Dim wuz As Boolean
            Dim bPensPassed As Boolean
            bPensPassed = ioPens.Count > 0
            If ioEnt.Domain = dxxDrawingDomains.Screen And ioEnt.Drawn Then
                entPaths = ioEnt.Paths
                _rVal = entPaths.Count > 0
                entPaths.Domain = dxxDrawingDomains.Screen
                entPaths.PixelSize = ioSettings.PointSize
                entPaths.PenWidth = ioEnt.PenWidth
                entPaths.Color = Entity_Color(ioSettings, ioEnt)
                If entPaths.Linetype = "" Then entPaths.Linetype = ioSettings.Linetype
            Else
                entPaths = Entity_Format(aImage, ioSettings, ioPens, ioEnt, bRefreshSettings, _rVal, tClr)
            End If
            If bErase Then
                If Not _rVal Then Return _rVal
                wuz = aImage.Erasing
                aImage.Erasing = True
            End If
            If _rVal Then
                nPaths = RenderPaths(aImage, ioPens, entPaths, True, ioEnt.Domain = dxxDrawingDomains.Screen, Not aSuppressRefresh, bIgnoreVisibility:=True, bSuppressImageEvents:=True, New dxoScreenEntity(ioEnt))
                If Not bErase Then
                    ioEnt.Paths = nPaths
                    ioEnt.Drawn = True
                End If
            End If
            If bErase Then aImage.Erasing = wuz
            aImage.obj_SCREEN = Me
            If bErase Then
                aImage.RaiseScreenEvent(dxxScreenEventTypes.ScreenEntityErase, aScreenEntity:=New dxoScreenEntity(ioEnt))
            Else
                aImage.RaiseScreenEvent(dxxScreenEventTypes.ScreenEntityDrawn, aScreenEntity:=New dxoScreenEntity(ioEnt))
            End If
            Return _rVal
        End Function
        Public Function Entity_Color(aSets As TSCREENSETTINGS, aEntity As TSCREENENTITY) As dxxColors

            If aSets.ZoomFactor <= 0 Then aSets = CurrentSettings(Nothing)
            Dim _rVal As dxxColors
            'On Error Resume Next
            _rVal = aEntity.Color
            If _rVal <> 0 And _rVal <> -1 And _rVal <> 256 Then Return _rVal
            Select Case aEntity.Type
         '===============================================================
                Case dxxScreenEntityTypes.Axis
                    '===============================================================
                    _rVal = aSets.AxisColor
         '===============================================================
                Case dxxScreenEntityTypes.Point
                    '===============================================================
                    _rVal = aSets.PointColor
         '===============================================================
                Case dxxScreenEntityTypes.Circle
                    '===============================================================
                    _rVal = aSets.CircleColor
         '===============================================================
                Case dxxScreenEntityTypes.Line
                    '===============================================================
                    _rVal = aSets.LineColor
         '===============================================================
                Case dxxScreenEntityTypes.Pill
                    '===============================================================
                    _rVal = aSets.CircleColor
         '===============================================================
                Case dxxScreenEntityTypes.Rectangle, dxxScreenEntityTypes.Triangle, dxxScreenEntityTypes.Pointer
                    '===============================================================
                    _rVal = aSets.RectangleColor
         '===============================================================
                Case dxxScreenEntityTypes.Text
                    '===============================================================
                    _rVal = aSets.TextColor
                Case Else
                    _rVal = dxxColors.Black
            End Select
            Return _rVal
        End Function
        Public Function Entity_Format(aImage As dxfImage, ByRef ioSettings As TSCREENSETTINGS, ByRef ioPens As TPENS, ByRef ioEntity As TSCREENENTITY, bUpdateSettings As Boolean, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As TPATHS = Nothing
            'On Error Resume Next
            GetImage(aImage)
            rDrawIt = True
            If bUpdateSettings Then ioSettings = CurrentSettings(aImage)
            Dim lt As String = String.Empty
            Select Case ioEntity.Type
         '===============================================================
                Case dxxScreenEntityTypes.Axis
                    '===============================================================
                    _rVal = xFormatEntity_Axis(aImage, Me, ioSettings, ioEntity, rDrawIt, rColor)
                    lt = dxfLinetypes.Continuous
         '===============================================================
                Case dxxScreenEntityTypes.Point
                    '===============================================================
                    _rVal = xFormatEntity_Point(aImage, Me, ioSettings, ioEntity, rDrawIt, rColor)
                    lt = dxfLinetypes.Continuous
         '===============================================================
                Case dxxScreenEntityTypes.Circle
                    '===============================================================
                    _rVal = xFormatEntity_Circle(aImage, Me, ioSettings, ioEntity, rDrawIt, rColor)
         '===============================================================
                Case dxxScreenEntityTypes.Line
                    '===============================================================
                    _rVal = xFormatEntity_Line(aImage, Me, ioSettings, ioEntity, rDrawIt, rColor)
         '===============================================================
                Case dxxScreenEntityTypes.Pill
                    '===============================================================
                    _rVal = xFormatEntity_Pill(aImage, Me, ioSettings, ioEntity, rDrawIt, rColor)
         '===============================================================
                Case dxxScreenEntityTypes.Rectangle, dxxScreenEntityTypes.Triangle, dxxScreenEntityTypes.Pointer
                    '===============================================================
                    _rVal = xFormatEntity_RecTri(aImage, Me, ioSettings, ioEntity, rDrawIt, rColor)
         '===============================================================
                Case dxxScreenEntityTypes.Text
                    '===============================================================
                    _rVal = xFormatEntity_Text(aImage, Me, ioSettings, ioEntity, rDrawIt, rColor)
                    lt = dxfLinetypes.Continuous
            End Select
            If ioEntity.Domain = dxxDrawingDomains.Screen And ioEntity.Type <> dxxScreenEntityTypes.Text Then
                _rVal.Scale(ioEntity.Plane.Origin, 1, -1, 1)
            End If
            If rDrawIt Then
                rDrawIt = _rVal.Count > 0
            End If
            If lt <> "" Then
                _rVal.Linetype = lt
            Else
                _rVal.Linetype = ioEntity.Paths.Linetype
                If _rVal.Linetype = "" Then _rVal.Linetype = ioSettings.Linetype
            End If
            _rVal.PenWidth = ioEntity.PenWidth
            _rVal.Color = rColor
            Return _rVal
        End Function
        Public Function Entity_OCS(aImage As dxfImage, ByRef ioDisplay As TDISPLAY, aEnt As dxfEntity, Optional aPixelSize As Integer = -1, Optional aColor As dxxColors = dxxColors.Undefined, Optional aAxisSize As Double = 0) As Boolean
            'On Error Resume Next
            If Not GetImage(aImage) Or aEnt Is Nothing Then Return False
            Dim gType As dxxGraphicTypes
            gType = aEnt.GraphicType
            If gType = dxxGraphicTypes.Point Then Return False
            '#1the subject image
            '#2the points to draw
            '#3the domain of the points
            '#4flag to control if the points are redraw in the next redraw or is cleared
            '#5the color to apply
            '#6the size to apply
            '^used to draw points to the screen only.
            Dim aVecs As TVECTORS
            aVecs = aEnt.ExtentPts(False)
            If aVecs.Count <= 0 Then Return False
            If aAxisSize <= 0 Then aAxisSize = OCSSize
            If aColor = dxxColors.Undefined Then aColor = EntitySymbolColor
            If aPixelSize < 0 Then aPixelSize = PointSize
            Dim dPths As TPATHS
            Dim aPln As TPLANE = aEnt.DefPts.Plane
            Dim lng As Double
            If aAxisSize < 0.005 Then aAxisSize = 0.005
            If aAxisSize > 0.5 Then aAxisSize = 0.5
            lng = (aAxisSize * ioDisplay.pln_VIEW.Height)
            Dim aPths As New TPATHS(dxxDrawingDomains.Model) With {.PixelSize = aPixelSize}
            aPths.Add(TPATH.UCS(aPln, aColor:=aColor, aLength:=lng))
            dPths = RenderPaths(aImage, New TPENS(0), aPths, bReturnDrawn:=False, bIsDeviceCoords:=False, bBitmapOutput:=False, bIgnoreVisibility:=True)
            Dim scrEnt As New TSCREENENTITY(dxxScreenEntityTypes.Axis, aAxisSize)
            If dPths.Count > 0 Then aImage.RaiseScreenEvent(dxxScreenEventTypes.EntityOCSDrawn, aScreenEntity:=New dxoScreenEntity(scrEnt), aImageEntity:=aEnt)
            Return dPths.Count > 0
        End Function
        Private Function Entity_Rectangle(aImage As dxfImage, ByRef ioDisplay As TDISPLAY, aEnt As dxfEntity, Optional aColor As dxxColors = dxxColors.Undefined) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If Not GetImage(aImage) Or aEnt Is Nothing Then Return _rVal
            If aEnt.PropValueB("*Boundless") Then Return _rVal
            '#1the subject image
            '#2the subject strin
            '^used to draw a rectangle to the screen only.
            Dim aRec As New TPLANE("")
            Dim aPaths As TPATHS
            Dim dPths As TPATHS
            Dim pth As TPATH
            aRec = aEnt.ExtentPts(False).Bounds(ioDisplay.pln_VIEW)
            If Not (aRec.Width > 0 Or aRec.Height > 0) Then Return _rVal
            _rVal = True
            If aColor = dxxColors.Undefined Then aColor = EntitySymbolColor
            aPaths = New TPATHS(dxxDrawingDomains.Model, aEnt.GUID)
            pth = TPATH.RECTANGLE(aRec, False)
            pth.Color = aColor
            pth.Linetype = dxfLinetypes.Continuous
            pth.LayerName = "0"
            pth.LineWeight = dxxLineWeights.LW_025
            aPaths.Add(pth)
            dPths = RenderPaths(aImage, New TPENS(0), aPaths, False, False, False, True)
            Dim scrEnt As New TSCREENENTITY(dxxScreenEntityTypes.Rectangle)
            aImage.RaiseScreenEvent(dxxScreenEventTypes.EntityBoundsDrawn, aScreenEntity:=New dxoScreenEntity(scrEnt), aImageEntity:=aEnt)
            Return _rVal
        End Function
        Public Function Entity_ExtentPoints(aImage As dxfImage, ByRef ioDisplay As TDISPLAY, aEnt As dxfEntity, Optional aPixelSize As Integer = -1, Optional aColor As dxxColors = dxxColors.Undefined) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If Not GetImage(aImage) Or aEnt Is Nothing Then Return False
            Dim gType As dxxGraphicTypes
            gType = aEnt.GraphicType
            If gType = dxxGraphicTypes.Point Then Return False
            '#1the subject image
            '#2the points to draw
            '#3the domain of the points
            '#4flag to control if the points are redraw in the next redraw or is cleared
            '#5the color to apply
            '#6the size to apply
            '^used to draw points to the screen only.
            Dim aVecs As TVECTORS
            aVecs = aEnt.ExtentPts(False)
            If aVecs.Count <= 0 Then Return _rVal
            _rVal = True
            If aColor = dxxColors.Undefined Then aColor = EntitySymbolColor
            If aPixelSize < 0 Then aPixelSize = PointSize
            Dim aPths As New TPATHS(dxxDomains.World, aEnt.GUID)
            Dim dPths As TPATHS
            Dim pth As TPATH
            Dim aLoop As New TVECTORS(0)
            aPths.PixelSize = aPixelSize
            pth = New TPATH() With {
                .Color = aColor,
                    .Linetype = dxfLinetypes.Continuous,
                    .LayerName = "0",
                    .LineWeight = dxxLineWeights.LW_025
                }
            For i As Integer = 1 To aVecs.Count
                aLoop.Add(aVecs.Item(i), TVALUES.ToByte(dxfGlobals.PT_PIXELTO))
            Next i
            pth.AddLoop(aLoop)
            aPths.Add(pth)
            dPths = RenderPaths(aImage, New TPENS(0), aPths, False, False, False, True)
            aImage.RaiseScreenEvent(dxxScreenEventTypes.EntityExtentPtsDrawn, aImageEntity:=aEnt)
            Return _rVal
        End Function
        Private Function xFormatEntity_Axis(aImage As dxfImage, ByRef ioScreen As TSCREEN, ByRef ioSettings As TSCREENSETTINGS, ByRef ioEntity As TSCREENENTITY, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As New TPATHS(dxxDrawingDomains.Screen)
            'On Error Resume Next
            rDrawIt = True
            Dim sz As Double
            Dim aPln As TPLANE = ioEntity.Plane
            Dim lng As Double
            Dim aPath As TPATH
            If ioEntity.EntityGUID <> "" Then ioEntity.ScreenFraction = ioSettings.AxisSize
            sz = ioEntity.ScreenFraction
            If sz <= 0 Then sz = ioSettings.AxisSize
            sz = TVALUES.LimitedValue(sz, 0.005, 0.5)
            If ioEntity.Domain = dxxDrawingDomains.Model Then
                If ioEntity.Height <= 0 Then
                    lng = (sz * ioSettings.DeviceRectangle.Height)
                    ioEntity.Height = lng
                End If
            Else
                lng = sz * ioSettings.DeviceRectangle.Height
                ioEntity.Height = lng
                If ioEntity.Alignment > 0 Then
                    aPln.Origin = New TVECTOR(0.5 * ioSettings.DeviceRectangle.Width, 0.5 * ioSettings.DeviceRectangle.Height)
                    Select Case ioEntity.Alignment
                        Case dxxRectangularAlignments.TopLeft
                            aPln.Origin += New TVECTOR(-(0.5 * ioSettings.DeviceRectangle.Width - 0.15 * lng - 4), -(0.5 * ioSettings.DeviceRectangle.Height - 1.25 * lng - 4))
                        Case dxxRectangularAlignments.TopCenter
                            aPln.Origin += New TVECTOR(0, -(0.5 * ioSettings.DeviceRectangle.Height - 1.25 * lng - 4))
                        Case dxxRectangularAlignments.TopRight
                            aPln.Origin += New TVECTOR((0.5 * ioSettings.DeviceRectangle.Width - 1.25 * lng - 4), -(0.5 * ioSettings.DeviceRectangle.Height - 1.25 * lng - 4))
                        Case dxxRectangularAlignments.MiddleLeft
                            aPln.Origin += New TVECTOR(-(0.5 * ioSettings.DeviceRectangle.Width - 0.15 * lng - 4), 0)
                        Case dxxRectangularAlignments.MiddleCenter
                        Case dxxRectangularAlignments.MiddleRight
                            aPln.Origin += New TVECTOR((0.5 * ioSettings.DeviceRectangle.Width - 1.25 * lng - 4), 0)
                        Case dxxRectangularAlignments.BottomLeft
                            aPln.Origin += New TVECTOR(-(0.5 * ioSettings.DeviceRectangle.Width - 0.15 * lng - 4), (0.5 * ioSettings.DeviceRectangle.Height - 0.15 * lng - 4))
                        Case dxxRectangularAlignments.BottomCenter
                            aPln.Origin += New TVECTOR(0, (0.5 * ioSettings.DeviceRectangle.Height - 0.15 * lng - 4))
                        Case dxxRectangularAlignments.BottomRight
                            aPln.Origin += New TVECTOR((0.5 * ioSettings.DeviceRectangle.Width - 1.25 * lng - 4), (0.5 * ioSettings.DeviceRectangle.Height - 0.15 * lng - 4))
                    End Select
                    ioEntity.Plane = aPln
                End If
            End If
            If rDrawIt Then
                rColor = ioScreen.Entity_Color(ioSettings, ioEntity)
                _rVal.Domain = dxxDrawingDomains.Screen
                aPln = ioEntity.Plane
                If ioEntity.Domain <> dxxDrawingDomains.Screen Then
                    lng = ioEntity.Height / (ioSettings.PixelsPerUnit * ioSettings.ZoomFactor)
                Else
                    lng = ioEntity.Height
                End If
                aPath = TPATH.UCS(aPln, aColor:=rColor, aLength:=lng)
                aPath.Domain = ioEntity.Domain
                _rVal.Add(aPath)
            End If
            Return _rVal
        End Function
        Private Function xFormatEntity_Circle(aImage As dxfImage, ByRef ioScreen As TSCREEN, ByRef ioSettings As TSCREENSETTINGS, ByRef ioEntity As TSCREENENTITY, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As TPATHS
            'On Error Resume Next
            rDrawIt = True
            _rVal = New TPATHS(dxxDrawingDomains.Model)
            Dim aPln As New TPLANE("")
            Dim bPln As New TPLANE("")
            Dim aPath As New TPATH
            Dim bPath As TPATH
            Dim i As Integer
            aPln = ioEntity.Plane
            If aPln.Height <= 0 Then
                ioEntity.Persist = False
                rDrawIt = False
            End If
            If rDrawIt Then
                rColor = ioScreen.Entity_Color(ioSettings, ioEntity)
                If ioEntity.Vectors.Count > 0 Then
                    For i = 1 To ioEntity.Vectors.Count
                        bPln = aPln
                        bPln.Origin = ioEntity.Vectors.Item(i)
                        If i = 1 Then
                            aPath = TPATH.CIRCLE(aPln)
                            aPath.Filled = ioEntity.Filled
                            aPath.Domain = ioEntity.Domain
                            _rVal.Add(aPath)
                        Else
                            bPath = TPATH.CIRCLE(bPln)
                            aPath.AddLoop(bPath.Looop(1))
                            _rVal.SetItem(1, aPath)
                        End If
                    Next i
                End If
                _rVal.Domain = dxxDrawingDomains.Screen
            End If
            Return _rVal
        End Function
        Private Function xFormatEntity_Line(aImage As dxfImage, ByRef ioScreen As TSCREEN, ByRef ioSettings As TSCREENSETTINGS, ByRef ioEntity As TSCREENENTITY, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As TPATHS
            'On Error Resume Next
            rDrawIt = ioEntity.Vectors.Count >= 2
            _rVal = New TPATHS(dxxDrawingDomains.Model)
            '**UNUSED VAR** Dim sz As Single
            Dim pxSz As Integer
            Dim aPath As TPATH
            Dim aPln As New TPLANE("")
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aLoop As TVECTORS
            If rDrawIt Then
                pxSz = ioEntity.Size
                If pxSz < 0 Then pxSz = ioSettings.PointSize
                If pxSz > 50 Then pxSz = 50
                rColor = ioScreen.Entity_Color(ioSettings, ioEntity)
                aPln = ioEntity.Plane
                aPath = New TPATH(ioEntity.Domain, aPlane:=New dxfPlane(aPln))
                For i = 1 To ioEntity.Vectors.Count
                    v1 = ioEntity.Vectors.Item(i)
                    If i + 1 > ioEntity.Vectors.Count Then Exit For
                    v2 = ioEntity.Vectors.Item(i + 1)
                    If i = 1 Then aPln.Origin = ioEntity.Vectors.Item(i)
                    aLoop = aPath.Looop(1)
                    If Not ioEntity.Dashed Then
                        If i = 1 Then aLoop.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        aLoop.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Else
                        aLoop.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        aLoop.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                        i += 1
                    End If
                    aPath.SetLoop(1, aLoop)
                Next i
                _rVal.PixelSize = pxSz
                _rVal.Add(aPath)
                _rVal.Domain = dxxDrawingDomains.Screen
            End If
            Return _rVal
        End Function
        Private Function xFormatEntity_Pill(aImage As dxfImage, ByRef ioScreen As TSCREEN, ByRef ioSettings As TSCREENSETTINGS, ByRef ioEntity As TSCREENENTITY, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As New TPATHS(ioEntity.Domain)
            'On Error Resume Next
            rDrawIt = True
            Dim aPln As TPLANE = ioEntity.Plane
            Dim i As Integer
            rDrawIt = (aPln.Width > 0 And aPln.Height > 0) And ioEntity.Vectors.Count > 0
            If Not rDrawIt Then
                ioEntity.Persist = False
                rDrawIt = False
            End If
            If aPln.Origin.Rotation <> 0 Then
                aPln.Revolve(aPln.Origin.Rotation, False)
            End If
            If rDrawIt Then
                rColor = ioScreen.Entity_Color(ioSettings, ioEntity)
                Dim aPath As New TPATH(ioEntity.Domain) With {.Filled = ioEntity.Filled}
                _rVal.Add(aPath)
                For i = 1 To ioEntity.Vectors.Count
                    aPln = ioEntity.Plane
                    aPln.Origin = ioEntity.Vectors.Item(i)
                    If aPln.Origin.Rotation <> 0 Then
                        aPln.Revolve(aPln.Origin.Rotation, False)
                    End If
                    aPath = TPATH.PILL(aPln)
                    aPath.AddLoop(aPath.Looop(1))
                    _rVal.SetItem(1, aPath)
                Next i
                _rVal.Domain = dxxDrawingDomains.Screen
            End If
            Return _rVal
        End Function
        Private Function xFormatEntity_Point(aImage As dxfImage, ByRef ioScreen As TSCREEN, ByRef ioSettings As TSCREENSETTINGS, ByRef ioEntity As TSCREENENTITY, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As New TPATHS(dxxDrawingDomains.Screen)
            'On Error Resume Next
            rDrawIt = True
            Dim aPln As New TPLANE("")
            Dim pxSz As Integer
            Dim aPath As TPATH
            Dim i As Integer
            aPln = ioEntity.Plane
            If rDrawIt Then
                pxSz = ioEntity.Size
                If pxSz < 0 Then pxSz = ioSettings.PointSize
                If pxSz > 50 Then pxSz = 50
                rColor = ioScreen.Entity_Color(ioSettings, ioEntity)
                _rVal.PixelSize = pxSz
                aPath = New TPATH
                Dim aLoop As New TVECTORS(0)
                aLoop.Add(aPln.Origin, TVALUES.ToByte(dxxVertexStyles.PIXEL))
                For i = 2 To ioEntity.Vectors.Count
                    aLoop.Add(ioEntity.Vectors.Item(i), TVALUES.ToByte(dxxVertexStyles.PIXEL))
                Next i
                aPath.AddLoop(aLoop)
                _rVal.Add(aPath)
            End If
            Return _rVal
        End Function
        Private Function xFormatEntity_RecTri(aImage As dxfImage, ByRef ioScreen As TSCREEN, ByRef ioSettings As TSCREENSETTINGS, ByRef ioEntity As TSCREENENTITY, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As TPATHS
            'On Error Resume Next
            rDrawIt = True
            _rVal = New TPATHS(dxxDrawingDomains.Model)
            Dim aPln As New TPLANE("")
            Dim bPln As New TPLANE("")
            Dim aPath As TPATH
            Dim bPath As TPATH
            Dim i As Integer
            aPln = ioEntity.Plane
            If Not (aPln.Width > 0 And aPln.Height > 0) Then rDrawIt = False
            If rDrawIt Then
                rColor = ioScreen.Entity_Color(ioSettings, ioEntity)
                '        If aPln.Origin.Rotation <> 0 Then
                '            pln_Revolve aPln, -aPln.Origin.Rotation, False
                '        End If
                '        If ioEntity.Type = Rectangle Then
                '            aPath = pth_RECTANGLE(aPln, ioEntity.EntityType = dxxGraphicTypes.Text)
                '        Else
                '           aPath = pth_TRIANGLE(aPln, ioEntity.Type = Triangle)
                '        End If
                '        aPath.Filled = ioEntity.Filled
                '        aPath.Plane = aPln
                '        pths_Add xFormatEntity_RecTri, aPath
                For i = 1 To ioEntity.Vectors.Count
                    bPln = ioEntity.Plane
                    bPln.Origin = ioEntity.Vectors.Item(i)
                    If i > 1 Then
                        If ioEntity.Vectors.Item(i).Rotation <> 0 Then
                            bPln.Revolve(ioEntity.Vectors.Item(i).Rotation, False)
                        End If
                    Else
                        If ioEntity.Vectors.Item(i).Rotation <> 0 Then
                            bPln.Revolve(ioEntity.Vectors.Item(i).Rotation, False)
                        End If
                    End If
                    If ioEntity.Type = dxxScreenEntityTypes.Rectangle Then
                        aPath = TPATH.RECTANGLE(bPln, ioEntity.EntityType = dxxGraphicTypes.Text)
                    Else
                        aPath = TPATH.TRIANGLE(bPln, ioEntity.Type = dxxScreenEntityTypes.Triangle)
                    End If
                    If i = 1 Then
                        aPath.Filled = ioEntity.Filled
                        aPath.Plane = aPln
                        _rVal.Add(aPath)
                    Else
                        bPath = _rVal.Item(1)
                        bPath.AddLoop(aPath.Looop(1))
                        _rVal.SetItem(1, bPath)
                    End If
                Next i
                _rVal.Domain = dxxDrawingDomains.Screen
            End If
            Return _rVal
        End Function
        Private Function xFormatEntity_Text(aImage As dxfImage, ByRef ioScreen As TSCREEN, ByRef ioSettings As TSCREENSETTINGS, ByRef ioEntity As TSCREENENTITY, ByRef rDrawIt As Boolean, ByRef rColor As dxxColors) As TPATHS
            Dim _rVal As New TPATHS(ioEntity.Domain)
            rDrawIt = True
            Dim sz As Double
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim devRec As TPLANE = ioSettings.DeviceRectangle
            Dim entRec As TPLANE
            Dim aDisplay As TDISPLAY = aImage.obj_DISPLAY
            rColor = ioEntity.Color
            Dim zf As Double = aDisplay.ZoomFactor
            Dim aTxt As New dxeText(dxxTextTypes.Multiline) With {.Color = rColor, .Domain = dxxDrawingDomains.Screen, .TextType = dxxTextTypes.Multiline, .ImageGUID = aImage.GUID}
            sz = ioEntity.ScreenFraction
            If sz <= 0 Then sz = ioSettings.TextSize
            If sz < 0.005 Then sz = 0.005
            If sz > 0.5 Then sz = 0.5
            ioEntity.ScreenFraction = sz
            If ioEntity.Alignment < dxxRectangularAlignments.TopLeft Or ioEntity.Alignment > dxxRectangularAlignments.BottomRight Then ioEntity.Alignment = dxxRectangularAlignments.TopLeft
            rColor = ioScreen.Entity_Color(ioSettings, ioEntity)
            ioEntity.Color = rColor
            aTxt.Factor = sz
            aTxt.Color = rColor
            aTxt.TextString = ioEntity.TextString
            aTxt.TextHeight = (sz * devRec.Height)
            If ioEntity.WidthFactor <= 0.01 Then
                aTxt.WidthFactor = ioScreen.TextStyle.PropValueD(dxxStyleProperties.WIDTHFACTOR)
            Else
                aTxt.WidthFactor = ioEntity.WidthFactor
            End If
            If ioEntity.Domain = dxxDrawingDomains.Model Then
                v1 = ioEntity.Plane.Origin
                aTxt.AlignmentPt1V = ioEntity.Plane.Origin
                aTxt.TextHeight = (sz * devRec.Height / ioSettings.PixelsPerUnit / ioSettings.ZoomFactor)
                aTxt.Alignment = dxfUtils.RectangleAligmentToMTextAlignment(ioEntity.Alignment)
            Else
                aTxt.TextHeight = ((sz * devRec.Height) / ioSettings.PixelsPerUnit) / zf
                aTxt.Alignment = dxfUtils.RectangleAligmentToMTextAlignment(ioEntity.Alignment)
            End If
            aTxt.UpdatePath(False, aImage)
            _rVal = aTxt.Paths
            If ioEntity.Domain = dxxDrawingDomains.Model Then
                _rVal.Domain = dxxDrawingDomains.Model
            Else
                _rVal.Domain = dxxDrawingDomains.Screen
                v1 = aTxt.InsertionPtV
                Dim hAln As dxxHorizontalJustifications
                Dim vAln As dxxVerticalJustifications
                'convert the world path to screen vectors
                _rVal = aImage.obj_DISPLAY.WorldPathsToDevicePaths(_rVal)
                'align the paths to the screen
                entRec = _rVal.Bounds(_rVal.Item(1).Plane)
                'add a text box
                If ioSettings.TextBoxes Then
                    entRec.Expand(10, 10)
                    _rVal.Add(TPATH.RECTANGLE(entRec, False, dxxDrawingDomains.Screen))
                End If
                'expand the text box for a buffer around the edges
                entRec.Expand(10, 10)
                'align the entity to the device rectangle
                If ioEntity.Vectors.Count > 0 Then
                    v1 = aImage.obj_DISPLAY.WorldToDevice(ioEntity.Vectors.Item(1))
                Else
                    v1 = devRec.AlignmentPoint(ioEntity.Alignment, True, hAln, vAln)
                End If
                v2 = entRec.AlignmentPoint(ioEntity.Alignment) - New TVECTOR(0.5 * devRec.Width, 0.5 * devRec.Height)
                Dim d1 As Double
                Dim aDir As TVECTOR = v2.DirectionTo(v1, False, rDistance:=d1)
                'move the path to the aligmnent pt
                If d1 <> 0 Then
                    _rVal.Translate(aDir * d1)
                    '_rVal.Project(d1, aDir)
                End If
            End If
            _rVal.Domain = dxxDrawingDomains.Screen
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"

#End Region 'Shared Methods
    End Structure 'TSCREEN
#End Region 'Structures




End Namespace
