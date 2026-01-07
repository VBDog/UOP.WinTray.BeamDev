
Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke
Namespace UOP.DXFGraphics.Structures

    Friend Structure TBRUSH
        Implements ICloneable
#Region "Members"
        Public Color As Integer
        Public Created As Boolean
        Friend _Handle As Gdi32.SafeHBRUSH
        Friend _HandleWuz As Gdi32.SafeHBRUSH
        Public Hatch As Long
        Public hdc As Long
        Public Index As Integer
        Public IsNull As Boolean
        Public IsSolid As Boolean
        Public Logical As Boolean
        Public Style As Long
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aIndex As Integer = 0)
            'init --------------------------------------
            Color = 0
            Created = False
            _Handle = Nothing
            _HandleWuz = Nothing
            Hatch = 0
            hdc = 0
            Index = aIndex
            IsNull = False
            IsSolid = True
            Logical = False
            Style = 0
            'init --------------------------------------
        End Sub

        Public Sub New(aBrush As TBRUSH)
            'init --------------------------------------
            Color = aBrush.Color
            Created = aBrush.Created
            _Handle = aBrush._Handle
            _HandleWuz = aBrush._HandleWuz
            Hatch = aBrush.Hatch
            hdc = aBrush.hdc
            Index = aBrush.Index
            IsNull = aBrush.IsNull
            IsSolid = aBrush.IsNull
            Logical = aBrush.Logical
            Style = aBrush.Style
            'init --------------------------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Handle As Long
            Get
                If _Handle IsNot Nothing Then
                    Return TVALUES.To_LNG(_Handle.DangerousGetHandle)
                Else
                    Return 0
                End If
            End Get
        End Property
        Public ReadOnly Property HandleWuz As Long
            Get
                If _HandleWuz IsNot Nothing Then
                    Return TVALUES.To_LNG(_HandleWuz.DangerousGetHandle)
                Else
                    Return 0
                End If
            End Get
        End Property
#End Region 'Properties

#Region "Methods"
        Public Function Clone() As TBRUSH
            Return New TBRUSH(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TBRUSH(Me)
        End Function
#End Region 'Methods

    End Structure 'TBRUSH
    Friend Structure TPEN
        Implements ICloneable
#Region "Members"
        Private _Color As Color
        Private _Descriptor As String
        Friend _Index As Integer
        Private _LTName As String
        Private _PatternScale As Double
        Friend _PixelElements() As Single
        Private _Width As Single
        Private _SegmentCount As Integer
#End Region 'Members
#Region "Constructors"
        Public Sub New(aLineType As TTABLEENTRY, aColor As Color, aPatternScale As Double, Optional aWidth As Single = 0)
            '#2the linetype to use to define the windows pen properties
            '#3the color to set to the pen
            '#4a scale factor for the linetype elements
            '#5the scale factor for the target device to convert the gdo line elements (in inches) to pixels
            '#6flag to override to a continuous pen
            '^creates the pen resource based on the properties of the passed linetype and values

            ' init --------------------------
            _Color = aColor
            _Descriptor = aLineType.PropValueStr(dxxLinetypeProperties.Description)
            _Index = -1
            _LTName = aLineType.PropValueStr(dxxLinetypeProperties.Name).ToUpper()
            _PatternScale = IIf(Math.Abs(aPatternScale) > 0, Math.Abs(aPatternScale), 1)
            ReDim _PixelElements(-1)
            _Width = IIf(aWidth > 0, aWidth, 1)
            _SegmentCount = 0
            ' init --------------------------


            Dim maxPX As Single = 0
            Dim plen As Double


            'get the style data

            If Not aLineType.Values.Defined Then
                aLineType.Values = aLineType.GetLinetpyeStyleData(plen)
                aLineType.Values.Defined = True
                aLineType.Values.BaseValue = aLineType.Values.Total(True)
            End If
            _SegmentCount = aLineType.Values.Count

            If _SegmentCount > 0 And aLineType.Values.BaseValue > 0 Then
                'convert the pattern data to scaled pixels
                System.Array.Resize(_PixelElements, _SegmentCount)
                For i As Integer = 1 To _SegmentCount
                    Dim sLen As Single = CType(aLineType.Values.Member(i - 1), Single)
                    If sLen = 0 Then
                        _PixelElements(i - 1) = 1
                    Else
                        Dim pixs As Single = Math.Abs(sLen) * _PatternScale
                        If pixs = 0 Then pixs = 1
                        _PixelElements(i - 1) = pixs
                        If pixs > maxPX Then maxPX = pixs
                    End If
                Next i
            End If
            If _Descriptor = "" Then _Descriptor = TPEN.Description(LTName, ColorTranslator.ToWin32(Color), _Width, _PatternScale)
        End Sub



        Public Sub New(aLineType As dxoLinetype, aColor As Color, aPatternScale As Double, Optional aWidth As Single = 0)
            '#2the linetype to use to define the windows pen properties
            '#3the color to set to the pen
            '#4a scale factor for the linetype elements
            '#5the scale factor for the target device to convert the gdo line elements (in inches) to pixels
            '#6flag to override to a continuous pen
            '^creates the pen resource based on the properties of the passed linetype and values

            ' init --------------------------
            _Color = aColor
            _Descriptor = aLineType.PropValueStr(dxxLinetypeProperties.Description)
            _Index = -1
            _LTName = aLineType.PropValueStr(dxxLinetypeProperties.Name).ToUpper()
            _PatternScale = IIf(Math.Abs(aPatternScale) > 0, Math.Abs(aPatternScale), 1)
            ReDim _PixelElements(-1)
            _Width = IIf(aWidth > 0, aWidth, 1)
            _SegmentCount = 0
            ' init --------------------------


            Dim maxPX As Single = 0
            Dim plen As Double


            'get the style data

            If Not aLineType.Values.Defined Then
                aLineType.Values = aLineType.GetLinetpyeStyleData(plen)
                aLineType.Values.Defined = True
                aLineType.Values.BaseValue = aLineType.Values.Total(True)
            End If
            _SegmentCount = aLineType.Values.Count

            If _SegmentCount > 0 And aLineType.Values.BaseValue > 0 Then
                'convert the pattern data to scaled pixels
                System.Array.Resize(_PixelElements, _SegmentCount)
                For i As Integer = 1 To _SegmentCount
                    Dim sLen As Single = CType(aLineType.Values.Member(i - 1), Single)
                    If sLen = 0 Then
                        _PixelElements(i - 1) = 1
                    Else
                        Dim pixs As Single = Math.Abs(sLen) * _PatternScale
                        If pixs = 0 Then pixs = 1
                        _PixelElements(i - 1) = pixs
                        If pixs > maxPX Then maxPX = pixs
                    End If
                Next i
            End If
            If _Descriptor = "" Then _Descriptor = TPEN.Description(LTName, ColorTranslator.ToWin32(Color), _Width, _PatternScale)
        End Sub

        Public Sub New(aDescriptor As String)

            ' init --------------------------
            _Color = Color.Black
            _Descriptor = aDescriptor
            _Index = -1
            _LTName = ""
            _PatternScale = 1
            ReDim _PixelElements(-1)
            _SegmentCount = 0
            ' init --------------------------

        End Sub

        Public Sub New(aColor As Color, aWidth As Single, Optional aLTName As String = "CONTINUOUS")
            ' init --------------------------
            _Color = aColor
            _Descriptor = ""
            _Index = -1
            _LTName = IIf(String.IsNullOrWhiteSpace(aLTName), "CONTINUOUS", aLTName.Trim().ToUpper())
            _PatternScale = 1
            ReDim _PixelElements(-1)
            _Width = IIf(aWidth > 0, aWidth, 1)
            _SegmentCount = 0
            ' init --------------------------
            _Descriptor = TPEN.Description(LTName, ColorTranslator.ToWin32(Color), Width, PatternScale)
        End Sub

        Public Sub New(aPen As TPEN)
            ' init --------------------------
            _Color = aPen.Color
            _Descriptor = aPen.Descriptor
            _Index = aPen.Index
            _LTName = aPen.LTName
            _PatternScale = aPen.PatternScale
            ReDim _PixelElements(-1)
            _Width = aPen.Width
            _SegmentCount = aPen._SegmentCount
            ' init --------------------------
            If aPen._PixelElements IsNot Nothing Then
                _PixelElements = aPen._PixelElements.Clone()
                _SegmentCount = _PixelElements.Count
            End If
        End Sub


#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property GPen As Pen
            Get
                Dim _rVal As New Pen(Color, Width)
                If _SegmentCount > 0 Then
                    _rVal.DashStyle = Drawing2D.DashStyle.Custom
                    _rVal.DashPattern = _PixelElements
                End If
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property GBrush As Brush
            Get
                Return New SolidBrush(Color)
            End Get
        End Property
        Public Property Width As Single
            Get
                Return _Width
            End Get
            Set(value As Single)
                _Width = value
            End Set
        End Property
        Public Property PatternScale As Double
            Get
                Return _PatternScale
            End Get
            Set(value As Double)
                _PatternScale = value
            End Set
        End Property
        Public Property LTName As String
            Get
                Return _LTName
            End Get
            Set(value As String)
                _LTName = value
            End Set
        End Property
        Public ReadOnly Property Index As Integer
            Get
                Return _Index
            End Get
        End Property
        Public Property Descriptor As String
            Get
                Return _Descriptor
            End Get
            Set(value As String)
                _Descriptor = value
            End Set
        End Property
        Public Property Color As Color
            Get
                Return _Color
            End Get
            Set(value As Color)
                _Color = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Return _Descriptor
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function Description(aPenName As String, aColor As Integer, aWidth As Long, aPatternScale As Double) As String
            Dim _rVal As String = String.Empty
            If aWidth < 1 Then aWidth = 1
            aPenName = Trim(UCase(aPenName))
            If aPatternScale <= 0 Then aPatternScale = 1
            If aPenName = "" Then Return _rVal
            If aPenName = "CONTINUOUS" Then aPatternScale = 1
            _rVal = aPenName
            If aPenName = "INVISIBLE" Then Return _rVal
            TLISTS.Add(_rVal, aColor, bAllowDuplicates:=True)
            TLISTS.Add(_rVal, aWidth, bAllowDuplicates:=True)
            If aPenName <> "CONTINUOUS" Then
                TLISTS.Add(_rVal, Format(aPatternScale, "0.00000"), bAllowDuplicates:=True)
            End If
            Return _rVal
        End Function

        Public Function Clone() As TPEN
            Return New TPEN(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPEN(Me)
        End Function

#End Region 'Shared Methods
    End Structure 'TPEN
    Friend Structure TPENS
        Implements ICloneable

#Region "Members"


        Private _BackGroundColor As Color
        Private _ColorMode As dxxColorModes
        Private _DefaultLineWeight As Integer
        Private _DPI As Integer
        Private _GlobalLTScale As Double
        Private _LineWeightScale As Double
        Private _ScreenLTScale As Double
        Private _ShowLineWeights As Boolean
        Private _ZoomFactor As Double
        Private _Init As Boolean
        Private _Members() As TPEN
        Public InvisibleLayers As TLIST
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCount As Integer)
            'init ------------------------------------
            _BackGroundColor = Color.White
            _ColorMode = dxxColorModes.Full
            _DefaultLineWeight = 0
            _DPI = 0
            _GlobalLTScale = 0
            _LineWeightScale = 1
            _ScreenLTScale = 1
            _ShowLineWeights = False
            _ZoomFactor = 1
            _Init = True
            ReDim _Members(-1)
            InvisibleLayers = New TLIST(dxfGlobals.Delim)
            'init ------------------------------------

        End Sub

        Public Sub New(aImage As dxfImage)
            'init ------------------------------------
            _BackGroundColor = Color.White
            _ColorMode = dxxColorModes.Full
            _DefaultLineWeight = 0
            _DPI = 96
            _GlobalLTScale = 0
            _LineWeightScale = 1
            _ScreenLTScale = 1
            _ShowLineWeights = False
            _ZoomFactor = 1
            _Init = True
            ReDim _Members(-1)
            InvisibleLayers = New TLIST(dxfGlobals.Delim)
            'init ------------------------------------
            If aImage Is Nothing Then Return
            Dim aDisplay As TDISPLAY = aImage.obj_DISPLAY
            Dim aHdr As dxsHeader = aImage.Header

            _DPI = aImage.GetBitmap(False).DPI
            If _DPI <= 0 Then _DPI = 96
            _ZoomFactor = aDisplay.ZoomFactor
            If _ZoomFactor = 0 Then _ZoomFactor = 1
            _BackGroundColor = aDisplay.BackGroundColor ', .hdc)
            _GlobalLTScale = aHdr.LineTypeScale
            If _GlobalLTScale = 0 Then _GlobalLTScale = 1
            _ScreenLTScale = aImage.obj_SCREEN.GetPropVal(dxxScreenProperties.LTScale)
            If _ScreenLTScale = 0 Then _ScreenLTScale = 1
            _ShowLineWeights = aHdr.LineWeightDisplay
            _ColorMode = aDisplay.ColorMode
            _DefaultLineWeight = aHdr.LineWeightDefault
            If _ShowLineWeights Then _LineWeightScale = aHdr.LineWeightScale

            Add(New TPEN(Color.Transparent, 1, "INVISIBLE"))
            'add the Continous pen
            Add(New TPEN(Color.Black, 1, "CONTINUOUS"))
            'create pens for the defined linetypes
            Dim ltTbl As New TTABLE(aImage.Linetypes)
            For i As Integer = 1 To ltTbl.Count
                Dim aLType As TTABLEENTRY = ltTbl.Item(i)

                Dim ltnm As String = aLType.Props.GCValueStr(2).ToUpper()
                If ltnm = "BYLAYER" Or ltnm = "BYBLOCK" Or ltnm = "CONTINUOUS" Then Continue For

                Add(New TPEN(aLType, Color.Black, _GlobalLTScale, 1))


            Next i
            'set the invisible layers
            Dim lyTbl As New TTABLE(aImage.Layers)
            For i = 1 To lyTbl.Count
                Dim aLyr As TTABLEENTRY = lyTbl.Item(i)
                If Not aLyr.PropValueB(dxxLayerProperties.Visible) Then
                    InvisibleLayers.Add(aLyr.Name, bSuppressTest:=True)
                ElseIf aLyr.PropValueB(dxxLayerProperties.Frozen) Then
                    InvisibleLayers.Add(aLyr.Name, bSuppressTest:=True)
                End If

            Next i

        End Sub

        Public Sub New(aPens As TPENS)
            'init ------------------------------------

            _BackGroundColor = aPens._BackGroundColor
            _ColorMode = aPens._ColorMode
            _DefaultLineWeight = aPens._DefaultLineWeight
            _DPI = aPens._DPI
            _GlobalLTScale = aPens._GlobalLTScale
            _LineWeightScale = aPens._LineWeightScale
            _ScreenLTScale = aPens._ScreenLTScale
            _ShowLineWeights = aPens._ShowLineWeights
            _ZoomFactor = aPens._ZoomFactor
            _Init = True
            ReDim _Members(-1)
            InvisibleLayers = New TLIST(aPens.InvisibleLayers)
            'init ------------------------------------
            If aPens.Count > 0 Then _Members = aPens._Members.Clone()


        End Sub

#End Region 'Constructors
#Region "Properties"

        Public ReadOnly Property BackGroundBrightness As Byte
            Get
                Return dxfColors.Win64ToHSB(_BackGroundColor).B
            End Get
        End Property
        Public Property ColorMode As dxxColorModes
            Get
                Return _ColorMode
            End Get
            Set(value As dxxColorModes)
                _ColorMode = value
            End Set
        End Property
        Public Property DefaultLineWeight As Integer
            Get
                Return _DefaultLineWeight
            End Get
            Set(value As Integer)
                _DefaultLineWeight = value
            End Set
        End Property
        Public Property DPI As Integer
            Get
                Return _DPI
            End Get
            Set(value As Integer)
                _DPI = value
                If _DPI <> 96 Then
                    Beep()
                End If
            End Set
        End Property
        Public Property LineWeightScale As Double
            Get
                If _LineWeightScale <= 0 Then
                    Return 1
                Else
                    Return _LineWeightScale
                End If
            End Get
            Set(value As Double)
                _LineWeightScale = value
            End Set
        End Property
        Public Property GlobalLTScale As Double
            Get
                Return _GlobalLTScale
            End Get
            Set(value As Double)
                _GlobalLTScale = value
            End Set
        End Property
        Public Property ScreenLTScale As Double
            Get
                Return _ScreenLTScale
            End Get
            Set(value As Double)
                _ScreenLTScale = value
            End Set
        End Property
        Public Property ShowLineWeights As Boolean
            Get
                Return _ShowLineWeights
            End Get
            Set(value As Boolean)
                _ShowLineWeights = value
            End Set
        End Property
        Public Property ZoomFactor As Double
            Get
                If _ZoomFactor <= 0 Then _ZoomFactor = 1
                Return _ZoomFactor
            End Get
            Set(value As Double)
                _ZoomFactor = value
            End Set
        End Property
        Public Property BackGroundColor As Color
            Get
                Return _BackGroundColor
            End Get
            Set(value As Color)
                _BackGroundColor = value
            End Set
        End Property
        Public Property BackColor As Integer
            Get
                Return dxfColors.Win64ToWin32(BackGroundColor)
            End Get
            Set(value As Integer)
                BackGroundColor = dxfColors.Win32ToWin64(value)
            End Set
        End Property
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
        Public Function Item(aIndex As Integer) As TPEN
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TPEN("")
            End If
            _Members(aIndex - 1)._Index = aIndex
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TPEN)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
            _Members(aIndex - 1)._Index = aIndex
        End Sub
        Public Function GetPathPen(aImage As dxfImage, aPaths As TPATHS, aPathIndex As Integer) As TPEN
            Dim rPen As TPEN = Nothing
            'On Error Resume Next
            Dim aPath As TPATH = aPaths.Item(aPathIndex)
            Dim i As Integer
            Dim aPen As New TPEN
            Dim bLWts As Boolean
            Dim aDsp As TDISPLAYVARS = aPath.DisplayVars
            Dim bErase As Boolean = aImage.Erasing
            Dim pClr As Color
            Dim pidx As Integer
            Dim aLyr As dxoLayer = Nothing
            Dim pwd As Integer = 1
            Dim descr As String
            Dim gLTScale As Double = GlobalLTScale
            Dim ZF As Double
            Dim aPatScale As Double = 1
            Dim lwt As Double
            Dim pFound As Boolean
            If Not bErase Then ZF = ZoomFactor Else ZF = 1
            If Not String.IsNullOrWhiteSpace(aPath.LayerName) Then aDsp.LayerName = aPath.LayerName.Trim Else aDsp.LayerName = ""
            'aDsp.LayerName = aPath.LayerName.Trim
            If aDsp.LayerName = "" Then aDsp.LayerName = aPaths.LayerName
            aDsp.Color = aPath.Color
            If aDsp.Color = dxxColors.ByBlock Or aDsp.Color = dxxColors.Undefined Then aDsp.Color = aPaths.Color
            If Not String.IsNullOrWhiteSpace(aPath.Linetype) Then aDsp.Linetype = aPath.Linetype.Trim Else aDsp.Linetype = ""
            'aDsp.Linetype = aPath.Linetype.Trim
            If aDsp.Linetype.ToUpper = "BYBLOCK" Or aDsp.Linetype = "" Then aDsp.Linetype = aPaths.Linetype.Trim
            aDsp.LineWeight = aPath.LineWeight
            aDsp.LTScale = aPath.LTScale
            If aDsp.LTScale <= 0 Then aDsp.LTScale = aPaths.LTScale
            If aPaths.Domain = dxxDrawingDomains.Screen Then
                gLTScale = ScreenLTScale
                ZF = 1
                aDsp.LayerName = "0"
                If aDsp.Linetype.ToUpper = "BYBLOCK" Or aDsp.Linetype = "" Then aDsp.Linetype = dxfLinetypes.Continuous
                If aDsp.Color = dxxColors.ByBlock Or aDsp.Color = dxxColors.ByLayer Then aDsp.Color = dxxColors.BlackWhite
                aDsp.LineWeight = dxxLineWeights.LW_000
                If aPaths.PenWidth > 0 And Not aPath.Filled Then
                    pwd = aPaths.PenWidth
                End If
                If aPath.Domain = dxxDrawingDomains.Model And Not bErase Then
                    ZF = ZoomFactor
                End If
            Else
                If String.IsNullOrWhiteSpace(aDsp.LayerName) Then aDsp.LayerName = "0"
                If aDsp.Color = dxxColors.ByBlock Or aDsp.Color = dxxColors.Undefined Then aDsp.Color = dxxColors.ByLayer
                If aDsp.Linetype.ToUpper = "BYBLOCK" Or aDsp.Linetype = "" Then aDsp.Linetype = dxfLinetypes.ByLayer
                If aDsp.LTScale <= 0 Then aDsp.LTScale = 1
                bLWts = ShowLineWeights And Not aPath.Filled
                If bLWts Then
                    If aDsp.LineWeight <= dxxLineWeights.ByBlock Then aDsp.LineWeight = aPaths.LineWeight
                    If aDsp.LineWeight <= dxxLineWeights.ByBlock Then aDsp.LineWeight = dxxLineWeights.ByLayer
                End If
            End If
            If bErase Then aDsp.Linetype = dxfLinetypes.Continuous
            If aDsp.Linetype.ToUpper <> "INVISIBLE" Then
                If aDsp.Linetype.ToUpper = "BYLAYER" Or aDsp.Color = dxxColors.ByLayer Or aDsp.LineWeight = dxxLineWeights.ByLayer Then
                    aDsp.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, aDsp.LayerName)
                    aLyr = aImage.Layer(aDsp.LayerName)
                End If
                If bLWts Then
                    If aDsp.LineWeight = dxxLineWeights.ByLayer Then
                        aDsp.LineWeight = aLyr.PropValueI(dxxLayerProperties.LineWeight)
                        If aDsp.LineWeight < dxxLineWeights.LW_000 Then aDsp.LineWeight = DefaultLineWeight
                    End If
                    If aDsp.LineWeight < dxxLineWeights.LW_000 Then aDsp.LineWeight = dxxLineWeights.LW_000
                    lwt = (aDsp.LineWeight / 100) / 25.4 * _LineWeightScale * 1.85
                    pwd = TVALUES.To_INT(lwt * _DPI) ' TVALUES.To_LNG(((.LineWeight / 100) / 25.4 * DPI * LineWeightScale) / ZF)
                    If pwd <= 1 Then
                        bLWts = False
                        pwd = 1
                    End If
                End If
                If aDsp.Linetype.ToUpper = "BYLAYER" Then
                    aDsp.Linetype = aLyr.PropValueStr(dxxLayerProperties.Linetype)
                End If
                If aDsp.Color = dxxColors.ByLayer Then
                    aDsp.Color = aLyr.PropValueI(dxxLayerProperties.Color)
                End If
                'set the pen color
                pClr = PenColor(aDsp.Color, bErase)
            Else
                pwd = 1
                aDsp.LTScale = 1
                pClr = Color.Black
            End If
            'get the pen
            If pwd <= 0 Then pwd = 1
            If aDsp.Linetype.ToUpper = "INVISIBLE" Then
                pidx = 1
            ElseIf aDsp.Linetype.ToUpper = "CONTINUOUS" And pwd = 1 Then
                pidx = 2
            Else
                aPatScale = TVALUES.To_DBL(aDsp.LTScale * gLTScale * DPI * ZF)
                descr = TPEN.Description(aDsp.Linetype, ColorTranslator.ToWin32(pClr), pwd, aPatScale)
                For i = 1 To Count
                    aPen = Item(i)
                    If aPen.Descriptor = descr Then
                        pidx = i
                        Exit For
                    End If
                Next i
            End If
            If pidx = 0 Then
                If aImage IsNot Nothing Then
                    aDsp.Linetype = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, aDsp.Linetype)
                    Dim aLT As dxoLinetype = aImage.TableEntry(dxxReferenceTypes.LTYPE, aDsp.Linetype)
                    Add(New TPEN(aLT, pClr, aPatScale, pwd))
                    pidx = Count
                End If
            End If
            pFound = pidx > 0
            If pFound Then
                rPen = Item(pidx)
                rPen.Color = pClr
            End If
            'select the pen to the device
            If Not pFound Then Return New TPEN(pClr, pwd) Else Return rPen
        End Function
        Public Function PenColor(aColor As dxxColors, bErase As Boolean) As Color
            Dim _rVal As Integer = 0
            If bErase Then
                _rVal = BackColor
            Else
                _rVal = dxfColors.GetPenColor(aColor)
                If _ColorMode = dxxColorModes.BlackWhite Then
                    If _rVal <> 16777215 Then _rVal = 0 'vbWhite Then _rVal = vbBlack
                ElseIf _ColorMode = dxxColorModes.GreyScales Then
                    _rVal = dxfColors.Win32ToARGB(_rVal).Greyscale.ToWin32
                End If
                'black to white/ white to black
                If _rVal = 0 Then 'vbBlack
                    If BackGroundBrightness < dxfGlobals.BlackWhiteBrightness Then _rVal = 16777215 'vbWhite
                ElseIf _rVal = 16777215 Then 'vbWhite Then
                    If BackGroundBrightness >= dxfGlobals.BlackWhiteBrightness Then _rVal = 0 'vbBlack
                End If
            End If
            Return dxfColors.Win32ToWin64(_rVal)
        End Function
        Public Sub Add(aPen As TPEN)

            If Count >= Integer.MaxValue Then Return
            aPen.Descriptor = TPEN.Description(aPen.LTName, ColorTranslator.ToWin32(aPen.Color), aPen.Width, aPen.PatternScale)
            If aPen.Descriptor <> "" Then
                System.Array.Resize(_Members, Count + 1)
                _Members(_Members.Count - 1) = aPen
                _Members(_Members.Count - 1)._Index = _Members.Count
            End If

        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub

        Public Function Clone() As TPENS

            Return New TPENS(Me)

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPENS(Me)
        End Function

        Public Function ToList() As List(Of TPEN)
            If Count <= 0 Then Return New List(Of TPEN)
            Return _Members.ToList()
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function NullPen() As TPEN
            Return New TPEN With {.LTName = "INVISIBLE", .PatternScale = 1}
        End Function
#End Region 'Shared Methods
    End Structure 'TPENS

End Namespace
