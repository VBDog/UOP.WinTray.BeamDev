
Imports UOP.DXFGraphics


Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics

#Region "Structures"
    Friend Structure TCHARGROUPINFO
        Implements ICloneable
#Region "Members"
        Public Index As Integer
        Public StartIndex As Integer
        Public EndIndex As Integer
#End Region 'Members
#Region "Constructors"
        Public Sub New(aIndex As Integer, aStartID As Integer, aEndID As Integer)
            'init ----------------------
            StartIndex = aStartID
            EndIndex = aEndID
            Index = aIndex
            'init ----------------------
            TVALUES.SortTwoValues(True, StartIndex, EndIndex)

        End Sub
        Public Sub New(aInfo As TCHARGROUPINFO)
            'init ----------------------
            StartIndex = aInfo.StartIndex
            EndIndex = aInfo.EndIndex
            Index = aInfo.Index
            'init ----------------------


        End Sub

#End Region 'Constructors
#Region "Methods"
        Public Function Contains(aCharIndex As Integer) As Boolean
            Return aCharIndex >= StartIndex And aCharIndex <= EndIndex
        End Function
        Public Overrides Function ToString() As String
            Return $"GROUP[{ Index }] { StartIndex } to { EndIndex }"
        End Function

        Public Function Clone() As TCHARGROUPINFO
            Return New TCHARGROUPINFO(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TCHARGROUPINFO(Me)
        End Function
#End Region 'Methods
    End Structure 'TCHARGROUPINFO
    Friend Structure TCHARBOX
        Implements ICloneable
#Region "Members"
        Private _Descent As Double
        Private _Ascent As Double


        Private _ObliqueAngle As Double
        Private _Width As Double
        Public BasePt As TVECTOR
        Public XDirection As TVECTOR
        Public YDirection As TVECTOR
        Public VerticalAlignment As Boolean
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aBasePt As TVECTOR, aWidth As Double, aAscent As Double, aDescent As Double)

            'init --------------------------------------
            _Ascent = 0
            _Descent = 0
            _Width = 0
            BasePt = TVECTOR.Zero
            XDirection = TVECTOR.WorldX
            YDirection = TVECTOR.WorldY
            VerticalAlignment = False
            _ObliqueAngle = 0
            'init --------------------------------------
            BasePt = aBasePt
            _Ascent = aAscent
            _Descent = aDescent
            _Width = aWidth


        End Sub

        Friend Sub New(aCharBox As dxoCharBox)

            'init --------------------------------------
            _Ascent = 0
            _Descent = 0
            _Width = 0

            BasePt = TVECTOR.Zero
            XDirection = TVECTOR.WorldX
            YDirection = TVECTOR.WorldY
            VerticalAlignment = False
            _ObliqueAngle = 0
            'init --------------------------------------

            If aCharBox Is Nothing Then Return
            BasePt = New TVECTOR(aCharBox.BasePtV)
            XDirection = New TVECTOR(aCharBox.XDirectionV)
            YDirection = New TVECTOR(aCharBox.YDirectionV)
            _ObliqueAngle = aCharBox.ObliqueAngle
            _Ascent = aCharBox.Ascent
            _Descent = aCharBox.Descent
            _Width = aCharBox.Width
            VerticalAlignment = aCharBox.VerticalAlignment


        End Sub

        Public Sub New(aCharBox As TCHARBOX)
            'init --------------------------------------
            _Ascent = aCharBox._Ascent
            _Descent = aCharBox._Descent
            _Width = aCharBox._Width

            BasePt = New TVECTOR(aCharBox.BasePt)
            XDirection = New TVECTOR(aCharBox.XDirection)
            YDirection = New TVECTOR(aCharBox.YDirection)
            _ObliqueAngle = aCharBox.ObliqueAngle
            VerticalAlignment = aCharBox.VerticalAlignment
            _ObliqueAngle = aCharBox._ObliqueAngle
            'init --------------------------------------
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public Property Ascent As Double
            Get
                Return _Ascent
            End Get
            Set(value As Double)
                _Ascent = value
            End Set
        End Property
        Public ReadOnly Property DirectionsAreDefined As Boolean
            Get
                Dim _rVal As Boolean = True
                Dim xFlag As Boolean
                If TVECTOR.IsNull(XDirection) Then
                    XDirection = TVECTOR.WorldX
                    YDirection = TVECTOR.WorldY
                    _rVal = False
                Else
                    Dim d1 As Boolean = False
                    XDirection.Normalize(xFlag, d1)
                    If xFlag Then
                        XDirection = TVECTOR.WorldX
                        YDirection = TVECTOR.WorldY
                        _rVal = False
                    End If
                End If
                If TVECTOR.IsNull(YDirection) Then
                    YDirection = TVECTOR.WorldZ.CrossProduct(XDirection)
                    _rVal = False
                Else
                    Dim d1 As Boolean = False
                    YDirection.Normalize(xFlag, d1)
                    If xFlag Then
                        YDirection = TVECTOR.WorldZ.CrossProduct(XDirection)
                        _rVal = False
                    End If
                End If
                If TVECTOR.IsNull(XDirection, 6) Then Return False
                If TVECTOR.IsNull(YDirection, 6) Then Return False
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property Height As Double
            Get
                Return _Ascent + _Descent
            End Get
        End Property
        Public Function Center(Optional bIncludeTracking As Boolean = False) As TVECTOR
            Dim _rVal As New TVECTOR(BasePt)
            _rVal += XDirection * (0.5 * Width) '.Project(XDirection, 0.5 * Width(bIncludeTracking))
            _rVal += YDirection * (-_Descent + 0.5 * Height) '.Project(YDirection, -_Descent + 0.5 * Height)
            Return _rVal
        End Function
        Public Property ObliqueAngle As Double
            '^the text's oblique angle
            '~-85 to 85
            Get
                Return _ObliqueAngle
            End Get
            Set(value As Double)
                Dim aval As Double = TVALUES.ObliqueAngle(value)
                If aval <> _ObliqueAngle Then
                    _ObliqueAngle = aval
                End If
            End Set
        End Property
        Public Function Plane(Optional bCenterOnBasePt As Boolean = True) As TPLANE
            Dim _rVal As New TPLANE("CHAR BOX")
            If Not bCenterOnBasePt Then
                _rVal.Define(Center(), XDirection, YDirection, aHeight:=Height, aWidth:=Width)
            Else
                _rVal.Define(BasePt, XDirection, YDirection, aHeight:=Height, aWidth:=Width)
            End If
            _rVal.Descent = _Descent
            _rVal.ShearAngle = 0
            Return _rVal
        End Function
        Public Function BaseLine(Optional aLengthAdder As Double = 0) As TLINE
            Dim dX As Double = _Width + aLengthAdder

            Return New TLINE(StartPt, Vector(dX, 0))
        End Function
        Public Function Corners(Optional bSuppressTracking As Boolean = False) As TVECTORS
            '^returns corners of the retangle
            Dim Cnrs As New TVECTORS(0)
            Cnrs.Add(Point(dxxRectanglePts.TopLeft, False))
            Cnrs.Add(Point(dxxRectanglePts.TopRight, False))
            Cnrs.Add(Point(dxxRectanglePts.BottomRight, False))
            Cnrs.Add(Point(dxxRectanglePts.BottomLeft, False))
            Return Cnrs
        End Function
        Public Property Descent As Double
            Get
                Return _Descent
            End Get
            Set(value As Double)
                If value = _Descent Then Return
                _Descent = value
            End Set
        End Property
        Public Function Point(aPointEnum As dxxRectanglePts, Optional bSuppressOblique As Boolean = False) As TVECTOR
            Dim _rVal As TVECTOR
            Dim dX As Double = 0
            Dim dY As Double = 0
            Dim xFctrT As Double = 0
            Dim xFctrB As Double = 0
            Dim wd As Double = Width
            If ObliqueAngle <> 0 And Not bSuppressOblique Then
                xFctrT = _Ascent * Math.Tan(ObliqueAngle * Math.PI / 180)
                xFctrB = -_Descent * Math.Tan(ObliqueAngle * Math.PI / 180)
            End If
            Select Case aPointEnum
                Case dxxRectanglePts.BaselineCenter
                    dX = 0.5 * wd
                    dY = 0
                Case dxxRectanglePts.BaselineLeft
                    dX = 0
                    dY = 0
                Case dxxRectanglePts.BaselineRight
                    dX = wd
                    dY = 0
                Case dxxRectanglePts.BottomCenter
                    dX = 0.5 * wd + xFctrB
                    dY = -_Descent
                Case dxxRectanglePts.BottomLeft
                    dX = 0 + xFctrB
                    dY = -_Descent
                Case dxxRectanglePts.BottomRight
                    dX = wd + xFctrB
                    dY = -_Descent
                Case dxxRectanglePts.MiddleCenter
                    dX = 0.5 * wd
                    dY = -_Descent + 0.5 * Height
                    If ObliqueAngle <> 0 And Not bSuppressOblique Then
                        If dY > 0 Then
                            dX += dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        Else
                            dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        End If
                    End If
                Case dxxRectanglePts.MiddleLeft
                    dX = 0
                    dY = -_Descent + 0.5 * Height
                    If ObliqueAngle <> 0 And Not bSuppressOblique Then
                        If dY > 0 Then
                            dX += dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        Else
                            dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        End If
                    End If
                Case dxxRectanglePts.MiddleRight
                    dX = wd
                    dY = -_Descent + 0.5 * Height
                    If ObliqueAngle <> 0 And Not bSuppressOblique Then
                        If dY > 0 Then
                            dX += dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        Else
                            dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        End If
                    End If
                Case dxxRectanglePts.TopCenter
                    dX = 0.5 * wd + xFctrT
                    dY = _Ascent
                Case dxxRectanglePts.TopLeft
                    dX = 0 + xFctrT
                    dY = _Ascent
                Case dxxRectanglePts.TopRight
                    dX = wd + xFctrT
                    dY = _Ascent
                Case dxxRectanglePts.UnderlineLeft
                    dX = 0
                    dY = -0.2 * _Descent
                    If ObliqueAngle <> 0 And Not bSuppressOblique Then
                        dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case dxxRectanglePts.UnderlineRight
                    dX = wd
                    dY = -0.2 * _Descent
                    If ObliqueAngle <> 0 And Not bSuppressOblique Then
                        dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case dxxRectanglePts.OverlineLeft
                    dX = 0
                    dY = _Ascent + 0.2 * _Descent
                    If ObliqueAngle <> 0 And Not bSuppressOblique Then
                        dX = dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case dxxRectanglePts.OverlineRight
                    dX = wd
                    dY = _Ascent + 0.2 * _Descent
                    If ObliqueAngle <> 0 And Not bSuppressOblique Then
                        dX = dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case Else
                    dX = 0
                    dY = 0
            End Select
            _rVal = BasePt.Clone
            If dY <> 0 Then _rVal += YDirection * dY
            If dX <> 0 Then _rVal += XDirection * dX
            Return _rVal
        End Function
        Public ReadOnly Property IsDefined As Boolean
            Get
                If TVECTOR.IsNull(XDirection) Then Return False
                Return Not TVECTOR.IsNull(YDirection)
            End Get
        End Property
        Public ReadOnly Property IsWorld As Boolean
            Get
                If Math.Round(XDirection.X, 6) <> 1 Then Return False
                If Math.Round(YDirection.Y, 6) <> 1 Then Return False
                Return True
            End Get
        End Property
        Public Property Width As Double
            Get
                Return _Width
            End Get
            Set(value As Double)
                _Width = value
            End Set
        End Property
        Public ReadOnly Property StartPt As TVECTOR
            Get
                If Not VerticalAlignment Then
                    'Return BasePt
                    Return Vector(0, 0)
                Else
                    Return Point(dxxRectanglePts.TopCenter, True)
                End If
            End Get
        End Property
        Public ReadOnly Property EndPt As TVECTOR
            Get
                If Not VerticalAlignment Then
                    Return Vector(Width, 0)
                    'Return Point(dxxRectanglePts.BaselineRight, True, bSuppressTracking)
                Else
                    Return Point(dxxRectanglePts.BottomCenter, True)
                End If
            End Get
        End Property

        'Private _Tracking As Double
        'Public Property Tracking As Double
        '    Get
        '        If _Tracking <= 0 Then _Tracking = 1
        '        Return _Tracking
        '    End Get
        '    Set(value As Double)
        '        value = Math.Abs(value)
        '        If _Tracking <= 0 Then _Tracking = 1
        '        If _Tracking = value Then Return
        '        _Tracking = value
        '        If _Tracking < 0.75 Then _Tracking = 0.75
        '        If _Tracking > 4.0 Then _Tracking = 4.0
        '    End Set
        'End Property
        Public ReadOnly Property ZDirection As TVECTOR
            Get
                Return XDirection.CrossProduct(YDirection)
            End Get
        End Property



#End Region 'Properties
#Region "Methods"
        Public Function Clone() As TCHARBOX
            Return New TCHARBOX(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TCHARBOX(Me)
        End Function

        Public Function Vector(aX As Double, aY As Double, Optional bSuppressOblique As Boolean = False) As TVECTOR
            Dim _rVal As New TVECTOR(BasePt)
            Dim xFctrT As Double = 0
            Dim YDir As TVECTOR = YDirection
            If ObliqueAngle <> 0 And Not bSuppressOblique And aY <> 0 Then
                xFctrT = Math.Abs(aY) * Math.Tan(ObliqueAngle * Math.PI / 180)
                If aY < 0 Then xFctrT *= -1
            End If
            Dim dX As Double = aX + xFctrT
            If dX <> 0 Then _rVal += XDirection * dX
            If aY <> 0 Then _rVal += YDirection * aY
            Return _rVal
        End Function

        Public Function Edge(aEdge As dxxRectangleLines, Optional bSuppressOblique As Boolean = False, Optional aLengthAdder As Double = 0.0) As TLINE
            Dim P1 As dxxRectanglePts
            Dim P2 As dxxRectanglePts
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Select Case aEdge
                Case dxxRectangleLines.Baseline
                    P1 = dxxRectanglePts.BaselineLeft
                    P2 = dxxRectanglePts.BaselineRight
                Case dxxRectangleLines.BottomEdge
                    P1 = dxxRectanglePts.BottomLeft
                    P2 = dxxRectanglePts.BottomRight
                Case dxxRectangleLines.Diagonal1
                    P1 = dxxRectanglePts.TopLeft
                    P2 = dxxRectanglePts.BottomRight
                Case dxxRectangleLines.Diagonal2
                    P1 = dxxRectanglePts.BottomLeft
                    P2 = dxxRectanglePts.TopRight
                Case dxxRectangleLines.LeftEdge
                    P1 = dxxRectanglePts.TopLeft
                    P2 = dxxRectanglePts.BottomLeft
                Case dxxRectangleLines.RightEdge
                    P1 = dxxRectanglePts.TopRight
                    P2 = dxxRectanglePts.BottomRight
                Case dxxRectangleLines.TopEdge
                    P1 = dxxRectanglePts.TopLeft
                    P2 = dxxRectanglePts.TopRight
                Case Else
                    P1 = dxxRectanglePts.BaselineLeft
                    P2 = dxxRectanglePts.BaselineRight
            End Select
            v1 = Point(P1, bSuppressOblique)
            v2 = Point(P2, bSuppressOblique)
            If aLengthAdder <> 0 Then
                Dim aDir As TVECTOR
                aDir = v1.DirectionTo(v2)
                v1 += aDir * (-0.5 * aLengthAdder)
                v2 += aDir * (0.5 * aLengthAdder)
            End If
            Return New TLINE(v1, v2)
        End Function
        Public Sub Scale(aXScale As Double, aYScale As Double)
            If aXScale <= 0 Then aXScale = 1
            If aYScale <= 0 Then aYScale = 1
            Width *= aXScale
            Descent *= aYScale
            Ascent *= aYScale

        End Sub
        Public Sub Rescale(aScaleFactor As Single, aRefPt As TVECTOR)
            _Ascent *= aScaleFactor
            _Descent *= aScaleFactor
            Width *= aScaleFactor
            BasePt.Scale(New TVECTOR(aScaleFactor, aScaleFactor, aScaleFactor), aRefPt)
        End Sub
        Public Function Define(aBasePt As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional aAscent As Double? = Nothing, Optional aDescent As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim rOriginChange As Boolean = False
            Dim rOrientationChange As Boolean = False
            Dim rDimensionChange As Boolean = False
            Return Define(aBasePt, aXDir, aYDir, rOriginChange, rOrientationChange, rDimensionChange, aAscent, aDescent, aWidth)
        End Function
        Public Function Define(aBasePt As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, ByRef rOriginChange As Boolean, ByRef rOrientationChange As Boolean, ByRef rDimensionChange As Boolean, Optional aAscent As Double? = Nothing, Optional aDescent As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            rDimensionChange = False
            rOrientationChange = Not DirectionsAreDefined()

            rOriginChange = Not BasePt.Equals(aBasePt, 6)
            BasePt = New TVECTOR(aBasePt)

            If TVECTOR.IsNull(aXDir) Then aXDir = New TVECTOR(XDirection) Else aXDir = aXDir.Normalized
            If TVECTOR.IsNull(aYDir) Then aYDir = New TVECTOR(YDirection) Else aYDir = aYDir.Normalized
            Dim zDir As TVECTOR = XDirection.CrossProduct(YDirection)

            Dim xErr As Boolean
            Dim yErr As Boolean
            Dim wd As Double = _Width
            Dim mAscent As Double = _Ascent
            Dim mDescent As Double = _Descent

            If aAscent.HasValue Then
                If aAscent.Value <> mAscent Then
                    rDimensionChange = True
                    mAscent = aAscent.Value
                End If
            End If
            If aDescent.HasValue Then
                If aDescent.Value <> mDescent Then
                    rDimensionChange = True
                    mDescent = aDescent.Value
                End If
            End If

            If aWidth.HasValue Then
                If aWidth.Value <> wd Then
                    rDimensionChange = True
                    wd = aWidth.Value
                End If
            End If
            Dim xDir As TVECTOR = TVECTOR.ToDirection(aXDir, xErr)
            Dim yDir As TVECTOR = TVECTOR.ToDirection(aYDir, yErr)
            If xErr And yErr Then
                xDir = XDirection
                yDir = YDirection
                zDir = ZDirection
            Else
                If xErr And Not yErr Then
                    zDir = ZDirection
                    xDir = yDir.CrossProduct(zDir, True)
                    xErr = False
                ElseIf yErr And Not xErr Then
                    zDir = ZDirection
                    yDir = zDir.CrossProduct(xDir, True)
                    yErr = False
                Else
                    zDir = xDir.CrossProduct(yDir)
                End If
            End If
            If xDir.Equals(yDir, True, 6) Then
                yDir = zDir.CrossProduct(xDir, True)
            End If
            If Math.Abs(xDir.AngleTo(yDir, zDir, 4)) <> 90 Then
                yDir = zDir.CrossProduct(xDir, True)
            End If
            If Not XDirection.Equals(xDir, 6) Then rOrientationChange = True
            If Not YDirection.Equals(yDir, 6) Then rOrientationChange = True
            Ascent = mAscent
            Descent = mDescent
            _Width = wd
            BasePt = aBasePt
            XDirection = xDir
            YDirection = yDir
            Return rOriginChange Or rOrientationChange Or rDimensionChange
        End Function

        Public Sub Stretch(aDist As Double, Optional aStretchWidth As Boolean = True, Optional aStretchHeight As Boolean = True)
            If (Not aStretchWidth And Not aStretchHeight) Or aDist = 0 Then Return
            If aStretchWidth Then
                Dim dx As Double = Math.Abs(Width + aDist) - Width
                Width += dx
                BasePt += XDirection * (-0.5 * dx)
            End If
            If aStretchHeight Then
                Dim f1 As Double = Math.Abs(Height + aDist) / Height

                Ascent = f1 * Ascent
                Descent = f1 * Descent

            End If
        End Sub

        Public Function CopyDirections(aPlane As TPLANE) As Boolean

            If TPLANE.IsNull(aPlane) Then Return False
            Return Define(BasePt, aPlane.XDirection, aPlane.YDirection)
        End Function
        Public Function CopyDirections(aPlane As dxfPlane) As Boolean

            If dxfPlane.IsNull(aPlane) Then Return False
            Return Define(BasePt, aPlane.XDirectionV, aPlane.YDirectionV)
        End Function
        Public Function CopyDirections(aCharBox As dxoCharBox, Optional bMoveTo As Boolean = False) As Boolean
            If aCharBox Is Nothing Then Return False
            If TPLANE.IsNull(aCharBox) Then Return False
            Dim bpt As TVECTOR = BasePt
            If bMoveTo Then bpt = aCharBox.BasePtV
            Return Define(bpt, aCharBox.XDirectionV, aCharBox.YDirectionV)
        End Function
        Public Function CopyDirections(aCharBox As TCHARBOX, Optional bMoveTo As Boolean = False) As Boolean

            If TPLANE.IsNull(aCharBox) Then Return False
            Dim bpt As TVECTOR = BasePt
            If bMoveTo Then bpt = aCharBox.BasePt
            Return Define(bpt, aCharBox.XDirection, aCharBox.YDirection)
        End Function
        Public Function Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False) As Boolean

            Dim _rVal As Boolean
            '#2the start pt of the line to mirror across
            '#3the end pt of the line to mirror across
            '#4flag to mirror the origin
            '#5flag to mirror the directions
            '^mirrors the system across the passed line
            '~returns True if the system actually moves from this process
            If Not bMirrorOrigin And Not bMirrorOrigin Then Return False
            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return False
            End If
            Dim v0 As TVECTOR = TVECTOR.Zero
            If bMirrorOrigin Then
                If BasePt.Mirror(New TLINE(aSP, aEP), bSuppressCheck:=True) Then _rVal = True
            End If
            If bMirrorDirections Then
                Dim aDir As TVECTOR = aSP.DirectionTo(aEP)
                If XDirection.Mirror(New TLINE(v0, aDir), bSuppressCheck:=True) Then _rVal = True
                If YDirection.Mirror(New TLINE(v0, aDir), bSuppressCheck:=True) Then _rVal = True
            End If
            Return _rVal
        End Function
        Public Function Revolve(aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            If aAngle = 0 Then Return False
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '^used to change the orientation and/or the origin of the system by rotating the it about its z axis
            Return Define(BasePt, XDirection.RotatedAbout(ZDirection, aAngle, bInRadians, True), YDirection.RotatedAbout(ZDirection, aAngle, bInRadians, True))
        End Function
        Public Function RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False) As Boolean

            If Not bRotateOrigin And Not bRotateDirections Then Return False
            aAngle = TVALUES.NormAng(aAngle, bInRadians, True)
            If aAngle = 0 Then Return False
            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the system about the axis
            '#5flag to rotate the X,Y and Zdirections about the axis
            '#7returns the line used for the rotation axis
            '#8flag to raise no change events
            '^used to change the orientation and/or the origin of the system by rotating the it about the passed axis
            Dim aXs As TVECTOR
            Dim aFlg As Boolean
            Dim xDir As New TVECTOR(XDirection)
            Dim yDir As New TVECTOR(YDirection)
            Dim v0 As New TVECTOR(BasePt)
            If Not bSuppressNorm Then
                aXs = aAxis.Normalized(aFlg)
                If aFlg Then Return False
            Else
                aXs = aAxis
            End If
            If bRotateDirections Then
                xDir.RotateAbout(aXs, aAngle, bInRadians, True)
                yDir.RotateAbout(aXs, aAngle, bInRadians, True)
            End If
            If bRotateOrigin Then
                v0.RotateAbout(aOrigin, aXs, aAngle, bInRadians, True)
            End If
            Return Define(v0, xDir, yDir)
        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String = "TextBox [" & BasePt.ToString & "] W:" & Format(Width, "0.0#") & " A:" & Format(_Ascent, "0.0#") & "D:" & Format(_Descent, "0.0#")
            If XDirection.X <> 1 Then _rVal += "X:" & XDirection.ToString
            Return _rVal
        End Function
#End Region 'Methods
    End Structure  'TCHARBOX

    Friend Structure TCHARFORMAT
        Implements ICloneable
#Region "Members"
        Public Backwards As Boolean
        Public CharAlign As dxxCharacterAlignments
        Public CharHeight As Double
        Public Color As dxxColors
        Public FontIndex As Integer
        Public IsShape As Boolean
        Public Overline As Boolean
        Public Rotation As Double
        Public StackBelow As Boolean
        Public StackID As Integer
        Public StackStyle As dxxCharacterStackStyles
        Public StyleIndex As Integer
        Public Underline As Boolean
        Public UpsideDown As Boolean
        Public Vertical As Boolean
        Public WidthFactor As Double
        Public HeightFactor As Double
        Public TextStyleName As String
        Public StrikeThru As Boolean
        Private _Tracking As Double
        Private _ObliqueAngle As Double
#End Region 'Members

#Region "Constructors"

        Public Sub New(aFormats As TCHARFORMAT)
            'init -----------------------------------------
            Backwards = False
            CharAlign = dxxCharacterAlignments.Bottom
            CharHeight = 1
            Color = dxxColors.BlackWhite
            FontIndex = 0
            IsShape = False
            Overline = False
            Rotation = 0
            StackBelow = False
            StackID = 0
            StackStyle = dxxCharacterStackStyles.None
            StyleIndex = 0
            Underline = False
            UpsideDown = False
            Vertical = False
            WidthFactor = 1
            HeightFactor = 1
            TextStyleName = ""
            StrikeThru = False
            _Tracking = 1
            _ObliqueAngle = 0
            'init -----------------------------------------

            Backwards = aFormats.Backwards
            CharAlign = aFormats.CharAlign
            CharHeight = aFormats.CharHeight
            Color = aFormats.Color
            FontIndex = aFormats.FontIndex
            IsShape = aFormats.IsShape
            Overline = aFormats.Overline
            Rotation = aFormats.Rotation
            StackBelow = aFormats.StackBelow
            StackID = aFormats.StackID
            StackStyle = aFormats.StackStyle
            StyleIndex = aFormats.StyleIndex
            Underline = aFormats.Underline
            UpsideDown = aFormats.UpsideDown
            Vertical = aFormats.Vertical
            WidthFactor = aFormats.WidthFactor
            HeightFactor = aFormats.HeightFactor
            TextStyleName = aFormats.TextStyleName
            StrikeThru = aFormats.StrikeThru
            _Tracking = aFormats._Tracking
            _ObliqueAngle = aFormats._ObliqueAngle
        End Sub

        Public Sub New(aFormats As dxoCharFormat)
            'init -----------------------------------------
            Backwards = False
            CharAlign = dxxCharacterAlignments.Bottom
            CharHeight = 1
            Color = dxxColors.BlackWhite
            FontIndex = 0
            IsShape = False
            Overline = False
            Rotation = 0
            StackBelow = False
            StackID = 0
            StackStyle = dxxCharacterStackStyles.None
            StyleIndex = 0
            Underline = False
            UpsideDown = False
            Vertical = False
            WidthFactor = 1
            HeightFactor = 1
            TextStyleName = ""
            StrikeThru = False
            _Tracking = 1
            _ObliqueAngle = 0
            'init -----------------------------------------

            If aFormats Is Nothing Then Return
            Backwards = aFormats.Backwards
            CharAlign = aFormats.CharAlign
            CharHeight = aFormats.CharHeight
            Color = aFormats.Color
            FontIndex = aFormats.FontIndex
            IsShape = aFormats.IsShape
            Overline = aFormats.Overline
            Rotation = aFormats.Rotation
            StackBelow = aFormats.StackBelow
            StackID = aFormats.StackID
            StackStyle = aFormats.StackStyle
            StyleIndex = aFormats.StyleIndex
            Underline = aFormats.Underline
            UpsideDown = aFormats.UpsideDown
            Vertical = aFormats.Vertical
            WidthFactor = aFormats.WidthFactor
            HeightFactor = aFormats.HeightFactor
            TextStyleName = aFormats.TextStyleName
            StrikeThru = aFormats.StrikeThru
            _Tracking = aFormats.Tracking
            _ObliqueAngle = aFormats.ObliqueAngle
        End Sub
        Public Sub New(Optional aAligment As dxxCharacterAlignments = dxxCharacterAlignments.Bottom)
            'init -----------------------------------------
            Backwards = False
            CharAlign = dxxCharacterAlignments.Bottom
            CharHeight = 1
            Color = dxxColors.BlackWhite
            FontIndex = 0
            IsShape = False
            Overline = False
            Rotation = 0
            StackBelow = False
            StackID = 0
            StackStyle = dxxCharacterStackStyles.None
            StyleIndex = 0
            Underline = False
            UpsideDown = False
            Vertical = False
            WidthFactor = 1
            HeightFactor = 1
            TextStyleName = ""
            StrikeThru = False
            _Tracking = 1
            _ObliqueAngle = 0
            'init -----------------------------------------


            CharAlign = aAligment
        End Sub

        Private Sub Init()
            'init -----------------------------------------
            Backwards = False
            CharAlign = dxxCharacterAlignments.Bottom
            CharHeight = 1
            Color = dxxColors.BlackWhite
            FontIndex = 0
            IsShape = False
            Overline = False
            Rotation = 0
            StackBelow = False
            StackID = 0
            StackStyle = dxxCharacterStackStyles.None
            StyleIndex = 0
            Underline = False
            UpsideDown = False
            Vertical = False
            WidthFactor = 1
            HeightFactor = 1
            TextStyleName = ""
            StrikeThru = False
            _Tracking = 1
            _ObliqueAngle = 0
            'init -----------------------------------------
        End Sub

        Public Function Clone() As TCHARFORMAT
            Return New TCHARFORMAT(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TCHARFORMAT(Me)
        End Function
#End Region 'Constructors

#Region "Properties"
        Public ReadOnly Property IsTrueType As Boolean
            Get
                Return Not IsShape
            End Get
        End Property

        Public Property FontStyleInfo As TFONTSTYLEINFO
            Get
                Dim _rVal As New TFONTSTYLEINFO With {.FontIndex = FontIndex, .StyleIndex = StyleIndex, .IsShape = IsShape}
                Return _rVal
            End Get
            Set(value As TFONTSTYLEINFO)
                FontIndex = value.FontIndex
                StyleIndex = value.StyleIndex
                IsShape = value.IsShape
                If Tracking <= 0 Then Tracking = 1
                If WidthFactor <= 0 Then WidthFactor = 1
            End Set
        End Property
        Public Property ObliqueAngle As Double
            Get
                Return _ObliqueAngle
            End Get
            Set(value As Double)
                _ObliqueAngle = TVALUES.ObliqueAngle(value)
            End Set
        End Property
        Public Property Tracking As Double
            Get
                If _Tracking <= 0 Then _Tracking = 1
                Return _Tracking
            End Get
            Set(value As Double)
                If _Tracking <= 0 Then _Tracking = 1
                _Tracking = TVALUES.LimitedValue(value, 0.75, 4)

            End Set
        End Property
#End Region 'Properties
    End Structure 'TCHARFORMAT
    Friend Structure TCHAR
        Implements ICloneable
#Region "Members"
        Public Charr As String 'As String * 1
        Public GroupIndex As Integer
        Public Index As Integer
        Public Key As String
        Public LayerName As String
        Public LineIndex As Integer
        Public LineNo As Integer
        Public PathDefined As Boolean
        Public StringIndex As Integer
        Public ReplacedChar As String
        Public AsciiIndex As Integer
        Public CharBox As TCHARBOX
        Public AccentPaths As TPOINTS
        Public FormatCode As dxxCharFormatCodes
        Public Shape As TSHAPE
        Public Formats As TCHARFORMAT
        Public ExtentPts As TPOINTS
        Public BracketGroup As Integer
#End Region 'Members
#Region "Constructors"
        Public Sub New(aChar As TCHAR)
            'init ---------------------------------------
            Charr = aChar.Charr
            AsciiIndex = aChar.AsciiIndex

            GroupIndex = aChar.GroupIndex
            Index = aChar.Index
            LayerName = aChar.LayerName
            LineIndex = aChar.LineIndex
            LineNo = aChar.LineNo
            PathDefined = aChar.PathDefined
            StringIndex = aChar.StringIndex
            FormatCode = aChar.FormatCode
            Key = aChar.Key
            ReplacedChar = aChar.ReplacedChar
            BracketGroup = aChar.BracketGroup

            CharBox = New TCHARBOX(aChar.CharacterBox)
            Shape = New TSHAPE(aChar.Shape)
            Formats = New TCHARFORMAT(aChar.Formats)
            AccentPaths = New TPOINTS(aChar.AccentPaths)
            ExtentPts = New TPOINTS(aChar.ExtentPts)
            'init ---------------------------------------



        End Sub

        Public Sub New(aChar As dxoCharacter)
            'init ---------------------------------------
            Charr = ""
            AsciiIndex = 0

            GroupIndex = 0
            Index = -1
            LayerName = ""
            LineIndex = 0
            LineNo = 0
            PathDefined = False
            StringIndex = 0
            FormatCode = dxxCharFormatCodes.None
            Key = ""
            ReplacedChar = ""
            BracketGroup = 0

            CharBox = New TCHARBOX(TVECTOR.Zero, 0, 0, 0)
            Shape = New TSHAPE("")
            Formats = New TCHARFORMAT(dxxCharacterAlignments.Bottom)
            AccentPaths = New TPOINTS(0)
            ExtentPts = New TPOINTS(0)
            'init ---------------------------------------

            If aChar Is Nothing Then Return
            Charr = aChar.Charr
            AsciiIndex = aChar.AsciiIndex
            CharBox = New TCHARBOX(aChar.CharBox)
            GroupIndex = aChar.GroupIndex
            Index = aChar.Index
            LayerName = aChar.LayerName
            LineIndex = aChar.LineIndex
            LineNo = aChar.LineNo
            PathDefined = aChar.PathDefined
            StringIndex = aChar.StringIndex
            FormatCode = aChar.FormatCode
            Key = aChar.Key
            ReplacedChar = aChar.ReplacedChar
            BracketGroup = aChar.BracketGroup

            Shape = New TSHAPE(aChar.Shape)
            Formats = New TCHARFORMAT(aChar.Formats)
            AccentPaths = New TPOINTS(aChar.AccentPaths)
            ExtentPts = New TPOINTS(aChar.ExtentPts)

        End Sub

        Public Sub New(aChar As String, Optional aAsciiIndex As Integer = 0)
            'init ---------------------------------------
            Charr = ""
            AsciiIndex = 0

            GroupIndex = 0
            Index = -1
            LayerName = ""
            LineIndex = 0
            LineNo = 0
            PathDefined = False
            StringIndex = 0
            FormatCode = dxxCharFormatCodes.None
            Key = ""
            ReplacedChar = ""
            BracketGroup = 0

            CharBox = New TCHARBOX(TVECTOR.Zero, 0, 0, 0)
            Shape = New TSHAPE("")
            Formats = New TCHARFORMAT(dxxCharacterAlignments.Bottom)
            AccentPaths = New TPOINTS(0)
            ExtentPts = New TPOINTS(0)
            'init ---------------------------------------

            AsciiIndex = aAsciiIndex
            If aChar.Length > 1 Then
                aChar = aChar.Substring(1)
            End If
            Charr = aChar

        End Sub
        Public Sub New(aChar As String, aCharBox As TCHARBOX, aFormat As TCHARFORMAT, Optional aFormatCode As dxxCharFormatCodes = dxxCharFormatCodes.None)

            'init ---------------------------------------
            Charr = ""
            AsciiIndex = 0

            GroupIndex = 0
            Index = -1
            LayerName = ""
            LineIndex = 0
            LineNo = 0
            PathDefined = False
            StringIndex = 0
            FormatCode = dxxCharFormatCodes.None
            Key = ""
            ReplacedChar = ""
            BracketGroup = 0

            CharBox = New TCHARBOX(TVECTOR.Zero, 0, 0, 0)
            Shape = New TSHAPE("")
            Formats = New TCHARFORMAT(dxxCharacterAlignments.Bottom)
            AccentPaths = New TPOINTS(0)
            ExtentPts = New TPOINTS(0)
            'init ---------------------------------------

            Formats = aFormat
            If aChar.Length > 1 Then
                aChar = aChar.Substring(1)
            End If
            AsciiIndex = Asc(aChar)
            CharBox.CopyDirections(aCharBox)
        End Sub


#End Region 'Constructors
#Region "Properties"


        Public ReadOnly Property IsFormatCode As Boolean
            Get
                Return FormatCode <> dxxCharFormatCodes.None Or AsciiIndex <= 0
            End Get
        End Property

        Public Property StackBelow As Boolean
            Get
                Return Formats.StackBelow
            End Get
            Set(value As Boolean)
                Formats.StackBelow = value
            End Set
        End Property

        Public Property StackID As Integer
            Get
                Return Formats.StackID
            End Get
            Set(value As Integer)
                Formats.StackID = value
            End Set
        End Property

        Public Property StackStyle As dxxCharacterStackStyles
            Get
                Return Formats.StackStyle
            End Get
            Set(value As dxxCharacterStackStyles)
                Formats.StackStyle = value
            End Set
        End Property

        Public Property Ascent As Double
            Get
                Return CharBox.Ascent
            End Get
            Set(value As Double)
                CharBox.Ascent = value
            End Set
        End Property

        Public Property FontStyleInfo As TFONTSTYLEINFO
            Get
                Return Formats.FontStyleInfo
            End Get
            Set(value As TFONTSTYLEINFO)
                Formats.FontStyleInfo = value
            End Set
        End Property

        Public Property Color As dxxColors
            Get
                Return Formats.Color
            End Get
            Set(value As dxxColors)
                Formats.Color = value
            End Set
        End Property

        Public Property CharHeight As Double
            Get
                Return Formats.CharHeight
            End Get
            Set(value As Double)
                Formats.CharHeight = value
            End Set
        End Property

        Public Property IsShape As Boolean
            Get
                Return Formats.IsShape
            End Get
            Set(value As Boolean)
                Formats.IsShape = value
            End Set
        End Property

        Public Property Vertical As Boolean
            Get
                Return Formats.Vertical
            End Get
            Set(value As Boolean)
                Formats.Vertical = value
                CharBox.VerticalAlignment = value
            End Set
        End Property

        Public Property FontIndex As Integer
            Get
                Return Formats.FontIndex
            End Get
            Set(value As Integer)
                Formats.FontIndex = value
            End Set
        End Property

        Public Property StyleIndex As Integer
            Get
                Return Formats.StyleIndex
            End Get
            Set(value As Integer)
                Formats.StyleIndex = value
            End Set
        End Property

        Public Property ObliqueAngle As Double
            Get
                Return Formats.ObliqueAngle
            End Get
            Set(value As Double)
                CharBox.ObliqueAngle = value
                Formats.ObliqueAngle = CharBox.ObliqueAngle
            End Set
        End Property


        Public Property Tracking As Double
            Get
                Return Formats.Tracking
            End Get
            Set(value As Double)
                Formats.Tracking = value

            End Set
        End Property

        Public Property WidthFactor As Double
            Get
                Return Formats.WidthFactor
            End Get
            Set(value As Double)
                Formats.WidthFactor = value
            End Set
        End Property

        'Public Property Shape As TSHAPE
        '    Get
        '        Return _Shape
        '    End Get
        '    Set(value As TSHAPE)
        '        If Not value Is Nothing Then _Shape = value.Clone
        '    End Set
        'End Property

        Public Property Descent As Double
            Get
                Return CharBox.Descent
            End Get
            Set(value As Double)
                CharBox.Descent = value
            End Set
        End Property

        Public Property Overline As Boolean
            Get
                Return Formats.Overline
            End Get
            Set(value As Boolean)
                Formats.Overline = value
            End Set
        End Property

        Public Property Rotation As Double
            Get
                Return Formats.Rotation
            End Get
            Set(value As Double)
                Formats.Rotation = value
            End Set
        End Property

        Public Property CharAlign As dxxCharacterAlignments
            Get
                Return Formats.CharAlign
            End Get
            Set(value As dxxCharacterAlignments)
                Formats.CharAlign = value
            End Set
        End Property

        Public Property Underline As Boolean
            Get
                Return Formats.Underline
            End Get
            Set(value As Boolean)
                Formats.Underline = value
            End Set
        End Property

        Public Property StrikeThru As Boolean
            Get
                Return Formats.StrikeThru
            End Get
            Set(value As Boolean)
                Formats.StrikeThru = value
            End Set
        End Property

        Public Property Width As Double
            Get
                Return CharBox.Width
            End Get
            Set(value As Double)
                CharBox.Width = value
            End Set
        End Property



        Public ReadOnly Property Plane As TPLANE
            Get
                Return CharBox.Plane(False)
            End Get
        End Property

        Public ReadOnly Property IsTrueType As Boolean
            Get
                Return Formats.IsTrueType
            End Get
        End Property
#End Region 'Properties

#Region "Methods"

        Public Function BoundingRectangle(Optional bIncludeAccents As Boolean = False) As TPLANE
            Dim _rVal As New TPLANE($"Bounds [{Charr }]", CharBox)

            Dim aPts As TPOINTS = New TPOINTS(0)
            Dim myBox As TCHARBOX = CharBox
            myBox.ObliqueAngle = Formats.ObliqueAngle

            If ExtentPts.Count > 0 Then
                aPts.Append(ExtentPts)
            ElseIf Shape.Path.Count > 0 Then
                aPts.Append(Shape.Path)

            End If
            If bIncludeAccents Then
                aPts.Append(AccentPaths)
            End If


            Dim myPl As TPLANE = myBox.Plane(True)
            _rVal = aPts.Bounds(myPl, ObliqueAngle)

            Dim v1 As TVECTOR = _rVal.Point(dxxRectanglePts.TopRight).WithRespectTo(myPl)
            If v1.Y > 0 And v1.Y > myBox.Ascent Then myBox.Ascent = v1.Y 'Else myBox.Ascent = 0
            If v1.X > myBox.Width Then
                myBox.Width = v1.X
            End If
            v1 = _rVal.Point(dxxRectanglePts.BottomLeft).WithRespectTo(myPl)
            If v1.Y < 0 And Math.Abs(v1.Y) > myBox.Descent Then myBox.Descent = Math.Abs(v1.Y) 'Else myBox.Descent = 0
            CharBox = myBox

            Return _rVal
        End Function

        Public Function Clone() As TCHAR
            Return New TCHAR(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TCHAR(Me)
        End Function

        Public Sub Scale(aXScale As Double, aYScale As Double)
            If Double.IsNaN(aXScale) Then aXScale = 1
            If Double.IsNaN(aYScale) Then aXScale = 1

            If aXScale <= 0 Then aXScale = 1
            If aYScale <= 0 Then aYScale = 1
            If aYScale = 1 And aXScale = 1 Then Return
            CharBox.Scale(aXScale, aYScale)
            Shape.Path.Scale(aXScale, aYScale)
            ExtentPts.Scale(aXScale, aYScale)
            CharHeight *= aYScale
        End Sub

        Public Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, bMirrorDirections As Boolean, Optional bSuppressCheck As Boolean = False)

            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return
            End If
            CharBox.Mirror(aSP, aEP, True, bMirrorDirections, True)
        End Sub

        Public Sub Translate(aTranslation As TVECTOR)
            CharBox.BasePt += aTranslation
        End Sub

        Public Function Path(aDisplay As TDISPLAYVARS, aParentPlane As dxfPlane, ByRef rRelative As Boolean) As TPATH
            LayerName = aDisplay.LayerName
            Dim P1 As TPOINT
            Dim v1 As TVECTOR
            Dim pPTs As TPOINTS = Shape.Path
            Dim myBox As TCHARBOX = CharBox.Clone
            myBox.ObliqueAngle = Formats.ObliqueAngle
            rRelative = aParentPlane IsNot Nothing
            If rRelative Then
                'myBox.BasePt += aParentPlane.Origin
                myBox.CopyDirections(aParentPlane)
                'Else
                '    aParentPlane = myBox.Plane(True)
            End If
            Dim myPlane As TPLANE = myBox.Plane(True)
            Dim aLoop As New TVECTORS(0)
            For i As Integer = 1 To pPTs.Count
                P1 = pPTs.Item(i)
                v1 = myBox.Vector(P1.X, P1.Y, 0)
                If rRelative Then
                    v1 = v1.WithRespectTo(aParentPlane)
                End If
                aLoop.Add(v1, P1.Code)
            Next i
            For i As Integer = 1 To AccentPaths.Count
                P1 = AccentPaths.Item(i)
                v1 = myBox.Vector(P1.X, P1.Y, 0)
                If rRelative Then
                    v1 = v1.WithRespectTo(aParentPlane)
                End If
                aLoop.Add(v1, P1.Code)
            Next
            Dim _rVal As New TPATH(dxxDrawingDomains.Model, aDisplay, aParentPlane, aLoop) With {.Linetype = dxfLinetypes.Continuous, .Filled = Not Formats.IsShape, .Color = Formats.Color, .Relative = rRelative, .LayerName = aDisplay.LayerName}
            _rVal.Plane = IIf(rRelative, New TPLANE(aParentPlane), myPlane)
            '_rVal.Print(4)
            'Path.Print()
            Return _rVal
            'Return Shape.GetPath(aPl, aDisplay, Not Formats.IsShape, Formats.Color, aParentPlane, rRelative)
        End Function

        Public Function MoveFromTo(aFromPt As TVECTOR, aToPt As TVECTOR) As Double
            Dim d1 As Double = 0
            Dim aDir As TVECTOR = aFromPt.DirectionTo(aToPt, False, rDistance:=d1)
            If d1 <> 0 Then Translate(aDir * d1)
            Return d1
        End Function

        Public Function AlignPt(aAlignment As dxxMTextAlignments, Optional aYAdder As Double = 0.0, Optional bIncludeTracking As Boolean = False) As TVECTOR
            Dim _rVal As TVECTOR = CharBox.BasePt.Clone
            If aAlignment <> dxxMTextAlignments.BaselineLeft Then
                Dim vAlign As dxxTextJustificationsVertical
                Dim hAlign As dxxTextJustificationsHorizontal
                Dim dY As Double
                Dim dX As Double


                TFONT.EncodeAlignment(aAlignment, vAlign, hAlign)
                Select Case vAlign
                    Case dxxTextJustificationsVertical.Top
                        dY = Ascent
                    Case dxxTextJustificationsVertical.Bottom
                        dY = -Descent
                    Case dxxTextJustificationsVertical.Middle
                        dY = -Descent + 0.5 * (Ascent + Descent)
                End Select
                Select Case hAlign
                    Case dxxTextJustificationsHorizontal.Center
                        dX = CharBox.Width * 0.5
                    Case dxxTextJustificationsHorizontal.Right
                        dX = CharBox.Width
                End Select
                If dX <> 0 Or dY <> 0 Then _rVal = CharBox.Vector(dX, dY)
            End If
            If aYAdder <> 0 Then _rVal += CharBox.YDirection * aYAdder
            Return _rVal
        End Function

        Public Overrides Function ToString() As String
            Return $"{AsciiIndex}[{ Charr }]"
        End Function

        Public Sub RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False)
            CharBox.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, True, True, bSuppressNorm)
        End Sub

        Public Function TextRectangle(Optional aAscentAdder As Double = 0.0) As TPLANE
            Dim myBox As TCHARBOX = CharBox.Clone
            myBox.Ascent += aAscentAdder
            Return myBox.Plane(False) '' _rVal
        End Function

        Public Function CharacterBox(Optional aAscentAdder As Double = 0.0) As dxoCharBox
            Dim myBox As TCHARBOX = CharBox.Clone
            myBox.Ascent += aAscentAdder
            Return New dxoCharBox(myBox)
        End Function

        Public Sub ApplyFormats()
            '^computes the world path of the passed character
            Try
                AccentPaths = New TPOINTS(0)

                CharBox.ObliqueAngle = 0
                If IsFormatCode Then
                    ObliqueAngle = 0
                    Return
                End If

                CharBox.ObliqueAngle = ObliqueAngle
                Dim ovrd As Double = 0.2 * CharHeight
                Dim udrd As Double = 0.2 * CharHeight
                If IsTrueType Then
                    ovrd = 0.25 * CharHeight
                    udrd = 0.25 * CharHeight
                End If
                Dim changeextents As Boolean = False
                Dim charlims As TLIMITS = ExtentPts.Limits
                'points are relative to the characters charbox plane
                'overline
                '---------------------------------------------------------
                If Overline Then
                    changeextents = True
                    Dim topcenter As TPOINT = New TPOINT(charlims.Left + 0.5 * charlims.Width, Ascent + ovrd, dxxVertexStyles.MOVETO)

                    AccentPaths.Add(New TPOINT(0, topcenter.Y, dxxVertexStyles.MOVETO))
                    AccentPaths.Add(New TPOINT(CharBox.Width, topcenter.Y, dxxVertexStyles.LINETO))

                    ExtentPts.Add(New TPOINT(topcenter.X - 0.5 * charlims.Width, topcenter.Y, dxxVertexStyles.MOVETO))
                    ExtentPts.Add(New TPOINT(topcenter.X + 0.5 * charlims.Width, topcenter.Y, dxxVertexStyles.LINETO))
                End If
                'underline
                '---------------------------------------------------------
                If Underline Then
                    AccentPaths.Add(New TPOINT(0, -udrd, dxxVertexStyles.MOVETO))
                    AccentPaths.Add(New TPOINT(CharBox.Width, -udrd, dxxVertexStyles.LINETO))
                    Dim botcenter As TPOINT = New TPOINT(charlims.Left + 0.5 * charlims.Width, -udrd, dxxVertexStyles.MOVETO)

                    ExtentPts.Add(New TPOINT(botcenter.X - 0.5 * charlims.Width, botcenter.Y, dxxVertexStyles.MOVETO))
                    ExtentPts.Add(New TPOINT(botcenter.X + 0.5 * charlims.Width, botcenter.Y, dxxVertexStyles.LINETO))

                End If
                If StrikeThru Then

                    AccentPaths.Add(New TPOINT(0, 0.5 * Ascent, dxxVertexStyles.MOVETO))
                    AccentPaths.Add(New TPOINT(CharBox.Width, 0.5 * Ascent, dxxVertexStyles.LINETO))
                End If
                'backwards or upside down
                '---------------------------------------------------------
                If Formats.Backwards Or Formats.UpsideDown Then

                    Orient(Formats.Backwards, Formats.UpsideDown)
                End If


            Catch ex As Exception
            End Try
        End Sub
        Public Sub TransferToPlane(aToPlane As TPLANE)
            CharBox.CopyDirections(aToPlane)
            CharBox.BasePt = CharBox.BasePt.TransferedToPlane(CharBox.Plane, aToPlane, 1, 1, 1, 0)
        End Sub
        Public Sub Rescale(aScaleFactor As Single, aRefPt As TVECTOR)
            Formats.Tracking *= aScaleFactor
            Formats.CharHeight *= aScaleFactor
            CharBox.Rescale(aScaleFactor, aRefPt)
        End Sub
        Public Sub Orient(bBackwards As Boolean, bUpsideDown As Boolean)
            If Not bBackwards And Not bUpsideDown Then Return
            If bBackwards Then
                CharBox.RotateAbout(CharBox.BasePt, CharBox.YDirection, 180, False, True, True, True)
            End If
            If bUpsideDown Then
                CharBox.RotateAbout(CharBox.BasePt, CharBox.XDirection, 180, False, True, True, True)
            End If
        End Sub
#End Region 'Methods

#Region "Shared Methods"
        Public Shared ReadOnly Property Null
            Get
                Return New TCHAR("", -1)
            End Get
        End Property

#End Region 'Shared Methods
    End Structure 'TCHAR
    Friend Structure TCHARS
        Implements ICloneable
#Region "Members"
        Private _VisibleCount As Integer
        Private _CodeCount As Integer
        Private _CharacterString As String
        Private _CharBox As TCHARBOX
        Private _Members() As TCHAR
        Private _Init As Boolean

#End Region 'Members
#Region "Constructors"
        Public Sub New(aCharBox As TCHARBOX)
            'init ------------------------------------------------
            _CodeCount = 0
            _VisibleCount = 0
            _CharacterString = ""
            _CharBox = New TCHARBOX(New TVECTOR, 0, 0, 0)
            _Init = True
            ReDim _Members(-1)

            'init ------------------------------------------------
            _CharBox.CopyDirections(aCharBox)
        End Sub
        Public Sub New(aCharacters As dxoCharacters)
            'init ------------------------------------------------
            _CodeCount = 0
            _VisibleCount = 0
            _CharacterString = ""
            _CharBox = New TCHARBOX(New TVECTOR, 0, 0, 0)
            _Init = True
            ReDim _Members(-1)

            'init ------------------------------------------------
            If aCharacters Is Nothing Then Return
            For Each mem As dxoCharacter In aCharacters
                Add(New TCHAR(mem))
            Next
        End Sub
        Public Sub New(aCharacters As TCHARS)
            'init ------------------------------------------------
            _CodeCount = aCharacters.CodeCount
            _VisibleCount = aCharacters._VisibleCount
            _CharacterString = aCharacters._CharacterString
            _CharBox = New TCHARBOX(aCharacters._CharBox)
            _Init = True
            ReDim _Members(-1)

            'init ------------------------------------------------

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property IsEmpty As Boolean
            Get
                If Not _Init Then Return True Else Return Count <= 0
            End Get
        End Property

        Public Property Ascent As Double
            Get
                Return _CharBox.Ascent
            End Get
            Set(value As Double)
                _CharBox.Ascent = value
            End Set
        End Property

        Public Property BasePt As TVECTOR
            Get
                Return _CharBox.BasePt
            End Get
            Set(value As TVECTOR)
                _CharBox.BasePt = value
            End Set
        End Property

        Public Property Descent As Double
            Get
                Return _CharBox.Descent
            End Get
            Set(value As Double)
                _CharBox.Descent = value
            End Set
        End Property

        Public Property CharBox As TCHARBOX
            Get
                Return _CharBox
            End Get
            Set(value As TCHARBOX)

                If TPLANE.IsNull(value) Then Return
                _CharBox = value
                Dim d1 As Double
                Dim aDir As TVECTOR = _CharBox.BasePt.DirectionTo(value.BasePt, False, rDistance:=d1)
                Dim bDirs As Boolean = _CharBox.CopyDirections(value)
                If d1 = 0 And Not bDirs Then Return

                For i As Integer = 1 To Count
                    Dim aMem As TCHAR = _Members(i - 1)
                    If bDirs Then aMem.CharBox.CopyDirections(value)
                    If d1 <> 0 Then
                        aMem.Translate(aDir * d1)
                    End If
                    _Members(i - 1) = aMem
                Next

                _CharBox = value
            End Set
        End Property

        Public ReadOnly Property CharacterString As String
            Get
                Return _CharacterString
            End Get
        End Property

        Public ReadOnly Property CharacterCount As Integer
            '^returns the number of visible chracters in the string
            Get
                Return _VisibleCount
            End Get
        End Property

        Public ReadOnly Property CodeCount As Integer
            Get
                Return _CodeCount
            End Get
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

        Public Property LastChar As TCHAR
            Get
                Return Item(Count)
            End Get
            Set(value As TCHAR)
                SetItem(Count, value)
            End Set
        End Property
        Public Property Width As Double
            Get
                Return _CharBox.Width
            End Get
            Set(value As Double)
                _CharBox.Width = value
            End Set
        End Property

#End Region 'Properties
#Region "Methods"

        Public Function ToList() As List(Of TCHAR)
            If Count <= 0 Then Return New List(Of TCHAR)
            Return _Members.ToList()
        End Function


        Public Function Character(aIndex As Integer) As TCHAR
            '^gets or sets a visible character
            '~loop 1 to CharacterCount to retrieve sequentially
            Return Item(aIndex)
        End Function
        Public Sub SetChar(aIndex As Integer, value As TCHAR)
            If IsEmpty Then Return
            If aIndex < 1 Or aIndex > Count Then Return
            value.Index = aIndex
            value.CharBox.XDirection = _CharBox.XDirection
            value.CharBox.YDirection = _CharBox.YDirection
            _Members(aIndex - 1) = value

        End Sub
        Public Function Item(aIndex As Integer) As TCHAR
            '^gets or sets a visible or code character
            '~loop 1 to CodeCount to retrieve sequentially
            If IsEmpty Then Return TCHAR.Null
            If aIndex < 1 Or aIndex > Count Then Return TCHAR.Null
            Dim aChar As TCHAR = _Members(aIndex - 1)
            aChar.CharBox.XDirection = _CharBox.XDirection
            aChar.CharBox.YDirection = _CharBox.YDirection
            aChar.Index = aIndex
            _Members(aIndex - 1) = aChar
            Return aChar
        End Function
        Public Sub SetItem(aIndex As Integer, value As TCHAR)
            SetChar(aIndex, value)
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
            _VisibleCount = 0
            _CodeCount = 0
            _CharacterString = ""
        End Sub
        Public Sub Add(aChar As TCHAR)
            If Not _Init Then
                _Init = True
                ReDim _Members(-1)
            End If
            aChar.CharBox.CopyDirections(_CharBox)
            If aChar.IsFormatCode Then
                _CodeCount += 1
                aChar.Key = $"CODE:{_CodeCount}"
            Else
                _VisibleCount += 1
                aChar.Index = _VisibleCount
                _CharacterString += aChar.Charr
                aChar.Key = _VisibleCount.ToString()
            End If

            Array.Resize(_Members, _Members.Count + 1)
            aChar.Index = _Members.Count
            _Members(_Members.Count - 1) = aChar

        End Sub
        Public Function CopyDirections(aPlane As TPLANE) As Boolean

            If _CharBox.CopyDirections(aPlane) Then
                If IsEmpty Then Return True


                For i As Integer = 1 To Count
                    Dim aMem As TCHAR = Item(i) 'to update directions and index

                Next
                Return True
            Else
                Return False
            End If
        End Function
        Public Function CopyDirections(aCharBox As TCHARBOX) As Boolean
            If _CharBox.CopyDirections(aCharBox) Then

                For i As Integer = 1 To Count
                    Dim aMem As TCHAR = Item(i) 'to update directions and index

                Next
                Return True
            Else
                Return False
            End If
        End Function
        Public Function Clone() As TCHARS
            Return New TCHARS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TCHARS(Me)
        End Function

        Public Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, bMirrorDirections As Boolean, Optional bSuppressCheck As Boolean = False)

            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return
            End If
            _CharBox.Mirror(aSP, aEP, True, bMirrorDirections, True)
            Dim aMem As TCHAR
            For i As Integer = 1 To Count
                aMem = Item(i)
                aMem.Mirror(aSP, aEP, False, True)
                _Members(i - 1) = aMem
            Next
        End Sub
        Public Sub RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False)
            Dim aMem As TCHAR
            For i As Integer = 1 To Count
                aMem = Item(i)
                aMem.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
                _Members(i - 1) = aMem
            Next
            _CharBox.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
        End Sub
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return
            _CharBox.BasePt += aTranslation
            Dim aMem As TCHAR
            For i As Integer = 1 To Count
                aMem = Item(i)
                aMem.Translate(aTranslation)
                _Members(i - 1) = aMem
            Next
        End Sub
        Public Sub Rescale(aScaleFactor As Double, aRefPt As TVECTOR)
            _CharBox.Rescale(aScaleFactor, aRefPt)
            Dim aMem As TCHAR
            For i As Integer = 1 To Count
                aMem = Item(i)
                aMem.Rescale(aScaleFactor, aRefPt)
                _Members(i - 1) = aMem
            Next
        End Sub
        Public Function UpdateBounds(bVertical As Boolean, ByRef rBoundingRectangle As TPLANE) As TCHARBOX
            rBoundingRectangle = CharBox.Plane(True)
            rBoundingRectangle.SetDimensions(0, 0)
            Dim myBox As TCHARBOX = CharBox.Clone
            myBox.Ascent = 0
            myBox.Descent = 0
            myBox.Width = 0
            Dim aChar As TCHAR
            Dim aDir As TVECTOR
            Dim d1 As Double = 0
            Dim basePt As TVECTOR = myBox.BasePt.Clone
            Dim cRec As TPLANE
            Dim j As Integer = 0
            Dim v1 As TVECTOR
            Dim recPts As TVECTORS
            'loop on visible chars
            For ci = 1 To Count
                aChar = Item(ci)
                If Not aChar.IsFormatCode Then
                    j += 1
                    cRec = aChar.BoundingRectangle(False)
                    If bVertical Then
                        If j = 1 Then
                            basePt = cRec.Point(dxxRectanglePts.BottomCenter, True)
                        Else
                            v1 = cRec.Point(dxxRectanglePts.TopCenter)
                        End If
                    Else
                        v1 = aChar.CharBox.BasePt
                    End If
                    aDir = v1.DirectionTo(basePt, False, rDistance:=d1)
                    If d1 <> 0 Then
                        v1 = aDir * d1
                        aChar.Translate(v1)
                        cRec.Origin += v1
                    End If
                    recPts = cRec.Corners
                    v1 = recPts.Item(1).WithRespectTo(myBox)
                    If bVertical Then
                        basePt = cRec.Point(dxxRectanglePts.BottomCenter, True)
                        If j = 1 Then
                            If v1.Y > myBox.Ascent Then myBox.Ascent = v1.Y
                        End If
                        v1 = recPts.Item(3).WithRespectTo(myBox)
                        If v1.Y < 0 And Math.Abs(v1.Y) > myBox.Descent Then myBox.Descent = Math.Abs(v1.Y)
                        If v1.X > myBox.Width Then myBox.Width = v1.X
                    Else
                        basePt += myBox.XDirection * aChar.CharBox.Width
                        If v1.Y > myBox.Ascent Then myBox.Ascent = v1.Y
                        v1 = recPts.Item(3).WithRespectTo(myBox)
                        If v1.Y < 0 And Math.Abs(v1.Y) > myBox.Descent Then myBox.Descent = Math.Abs(v1.Y)
                        'If v1.X > myBox.Width Then myBox.Width = v1.X
                        myBox.Width += aChar.CharBox.Width


                    End If
                    rBoundingRectangle.ExpandToVectors(recPts)
                    _Members(aChar.Index - 1) = aChar
                End If
            Next
            CharBox = myBox
            Return myBox
        End Function
        Public Overrides Function ToString() As String
            Return _CharacterString
        End Function
#End Region 'Methods
    End Structure 'TCHARS
    Friend Structure TCHARARRAY
        Implements ICloneable
#Region "Members"
        Private _Mems As TDICTIONARY_CHAR
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCount As Integer)
            'init -------------------------------------
            _Mems = New TDICTIONARY_CHAR("")
            'init -------------------------------------


            Dim letter As String
            If aCount > 0 Then
                For i As Integer = 0 To aCount - 1
                    letter = Chr(i).ToString

                    Add(New TCHAR(letter, i))
                Next
            End If
        End Sub
        Public Sub New(aArray As TCHARARRAY)
            _Mems = New TDICTIONARY_CHAR(aArray._Mems)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get

                Return _Mems.Count ' _Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Member(aCharCode As Integer) As TCHAR
            Dim idx As Integer
            If Not Contains(aCharCode, idx) Then
                Return Nothing
            Else
                Return _Mems.Item(idx)
            End If
        End Function
        Public Function Contains(aCharIndex As Integer) As Boolean
            Dim rIndex As Integer = 0
            Return Contains(aCharIndex, rIndex)
        End Function
        Public Function Contains(aCharIndex As Integer, ByRef rIndex As Integer) As Boolean

            rIndex = 0
            Return _Mems.ContainsKey(aCharIndex.ToString, rIndex)
        End Function

        Public Function TryGet(aCharIndex As Integer, ByRef rChar As TCHAR) As Boolean
            rChar = New TCHAR("", 0)
            Dim rIndex As Integer
            Dim _rVal As Boolean = _Mems.ContainsKey(aCharIndex.ToString, rIndex)
            If _rVal Then rChar = _Mems.Item(rIndex)
            rChar.Index = rIndex
            Return _rVal
        End Function

        Public Sub Clear()

            _Mems.Clear()

        End Sub
        Public Function Item(aIndex As Integer) As TCHAR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TCHAR.Null
            End If
            Return _Mems.Item(aIndex)
        End Function
        Public Sub UpdateMember(aChar As TCHAR)
            Dim idx As Integer = 0
            If Not Contains(aChar.AsciiIndex, idx) Then
                Add(aChar)
            Else
                _Mems.SetItem(idx, aChar)
            End If
        End Sub
        Public Sub Add(aChar As TCHAR)

            If _Mems.ContainsKey(aChar.AsciiIndex.ToString) Then
                Return
            End If
            'aChar.Index = _Mems.Count + 1
            _Mems.Add(aChar.AsciiIndex.ToString, aChar)
        End Sub
        Public Function Clone() As TCHARARRAY
            Return New TCHARARRAY(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TCHARARRAY(Me)
        End Function
#End Region 'Methods
    End Structure 'TCHARARRAY
    Friend Structure TSTRING
        Implements ICloneable
#Region "Members"
        Public Alignment As dxxMTextAlignments
        Public FitFactor As Double
        Public GroupCount As Integer
        Public HasAlignments As Boolean
        Public HasStacks As Boolean
        'Public LeadChar As TCHAR
        Public LineNo As Integer

        Public ScreenAlignment As dxxRectangularAlignments
        Public StackCount As Integer
        Public Vertical As Boolean
        Private _Characters As TCHARS
#End Region 'Members
#Region "Constructors"
        Public Sub New(aPlane As TPLANE)
            'init ----------------------------------------------------
            Alignment = dxxMTextAlignments.BaselineLeft
            FitFactor = 0
            GroupCount = 0
            HasAlignments = False
            HasStacks = False
            LineNo = 0
            ScreenAlignment = dxxRectangularAlignments.TopLeft
            StackCount = 0
            Vertical = False
            _Characters = New TCHARS(New TCHARBOX(New TVECTOR, 0, 0, 0))
            'init ----------------------------------------------------
            _Characters.CopyDirections(aPlane)
            'LeadChar.CharBox.CopyDirections(aPlane)
        End Sub

        Public Sub New(aTag As String)
            'init ----------------------------------------------------
            Alignment = dxxMTextAlignments.BaselineLeft
            FitFactor = 0
            GroupCount = 0
            HasAlignments = False
            HasStacks = False
            LineNo = 0
            ScreenAlignment = dxxRectangularAlignments.TopLeft
            StackCount = 0
            Vertical = False
            _Characters = New TCHARS(New TCHARBOX(New TVECTOR, 0, 0, 0))
            'init ----------------------------------------------------

            'LeadChar.CharBox.CopyDirections(aPlane)
        End Sub

        Public Sub New(aString As TSTRING)
            'init ----------------------------------------------------
            Alignment = aString.Alignment
            FitFactor = aString.FitFactor
            GroupCount = aString.GroupCount
            HasAlignments = aString.HasAlignments
            HasStacks = aString.HasStacks
            LineNo = aString.LineNo
            ScreenAlignment = aString.ScreenAlignment
            StackCount = aString.StackCount
            Vertical = aString.Vertical
            _Characters = New TCHARS(aString._Characters)
            'init ----------------------------------------------------

        End Sub

        Public Sub New(aCharBox As TCHARBOX)
            'init ----------------------------------------------------
            Alignment = dxxMTextAlignments.BaselineLeft
            FitFactor = 0
            GroupCount = 0
            HasAlignments = False
            HasStacks = False
            LineNo = 0
            ScreenAlignment = dxxRectangularAlignments.TopLeft
            StackCount = 0
            Vertical = False
            _Characters = New TCHARS(New TCHARBOX(New TVECTOR, 0, 0, 0))
            'init ----------------------------------------------------

            _Characters = New TCHARS(aCharBox)

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property CharacterBox As TCHARBOX
            Get
                _Characters.Descent = Descent
                Return _Characters.CharBox
            End Get
            Set(value As TCHARBOX)
                _Characters.CharBox = value
            End Set
        End Property

        Public Property Ascent As Double
            Get
                Return _Characters.Ascent
            End Get
            Set(value As Double)
                _Characters.Ascent = value
            End Set
        End Property

        Public Property Descent As Double
            Get
                Return _Characters.Descent
            End Get
            Set(value As Double)
                _Characters.Descent = value
            End Set
        End Property
        Public Property Characters As TCHARS
            Get
                Return _Characters
            End Get
            Set(value As TCHARS)
                _Characters = value
            End Set
        End Property
        Public ReadOnly Property CharacterCount As Integer
            '^returns the number of visible chracters in the string
            Get
                Return _Characters.CharacterCount
            End Get
        End Property
        Friend Property LastCharV As TCHAR
            Get
                If CharacterCount <= 0 Then Return _Characters.Item(1) 'LeadChar
                Return CharacterV(CharacterCount)
            End Get
            Set(value As TCHAR)
                If CharacterCount > 0 Then SetChar(CharacterCount, value)
            End Set
        End Property
        Friend ReadOnly Property FirstCharV As TCHAR
            Get
                If CharacterCount <= 0 Then Return Characters.Item(1) 'LeadChar
                Return CharacterV(1)
            End Get
        End Property

        Public ReadOnly Property Plane As TPLANE
            Get
                Return CharacterBox.Plane(False)
            End Get
        End Property
        Friend ReadOnly Property BoundingRectangleV As TPLANE
            Get
                Dim _rVal As TPLANE = CharacterBox.Plane(False)
                Dim aChar As TCHAR
                If CharacterCount <= 0 Then Return _rVal
                _rVal.SetDimensions(0, 0)
                _rVal.Descent = 0
                For ci As Integer = 1 To CharacterCount
                    aChar = CharacterV(ci)
                    _rVal.ExpandToVectors(aChar.BoundingRectangle.Corners)
                Next ci
                _rVal.Name = "Bounds of String [" & LineNo & "]"
                Return _rVal
            End Get
        End Property
        Public Property Width As Double
            Get
                Return _Characters.Width
            End Get
            Set(value As Double)
                _Characters.Width = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function AlignPt(aAlignment As dxxMTextAlignments) As TVECTOR
            If aAlignment = dxxMTextAlignments.BaselineLeft Then Return CharacterBox.BasePt
            Dim aPt As dxxRectanglePts
            Select Case aAlignment
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.BaselineLeft
                    aPt = dxxRectanglePts.BaselineLeft
                Case dxxMTextAlignments.BaselineMiddle
                    aPt = dxxRectanglePts.BaselineCenter
                Case dxxMTextAlignments.BaselineRight
                    aPt = dxxRectanglePts.BaselineRight
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.BottomLeft
                    aPt = dxxRectanglePts.BottomLeft
                Case dxxMTextAlignments.BottomCenter
                    aPt = dxxRectanglePts.BottomCenter
                Case dxxMTextAlignments.BottomRight
                    aPt = dxxRectanglePts.BottomRight
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.MiddleLeft
                    aPt = dxxRectanglePts.MiddleLeft
                Case dxxMTextAlignments.MiddleCenter
                    aPt = dxxRectanglePts.MiddleCenter
                Case dxxMTextAlignments.MiddleRight
                    aPt = dxxRectanglePts.MiddleRight
             '---------------------------------------------------------------------------------------------------------
                Case dxxMTextAlignments.TopLeft
                    aPt = dxxRectanglePts.TopLeft
                Case dxxMTextAlignments.TopCenter
                    aPt = dxxRectanglePts.TopCenter
                Case dxxMTextAlignments.TopRight
                    aPt = dxxRectanglePts.TopRight
            End Select
            Return CharacterBox.Point(aPt)
        End Function
        Friend Function CharacterV(aIndex As Integer) As TCHAR
            '^gets or sets a visible character
            Return _Characters.Character(aIndex)
        End Function
        Public Sub SetChar(aIndex As Integer, value As TCHAR)
            _Characters.SetChar(aIndex, value)
        End Sub
        Public Overrides Function ToString() As String
            Dim sChrs As String = Characters.CharacterString
            If sChrs.Length = 0 Then
                Return $"TSTRING({ LineNo })"
            Else
                Return $"TSTRING({ LineNo }) - { sChrs}"
            End If
        End Function
        Public Sub AddChar(aChar As TCHAR)

            If Characters.Count <= 0 Or aChar.IsFormatCode Then
                aChar.MoveFromTo(aChar.CharBox.EndPt, CharacterBox.BasePt)
            Else
                Dim lChar As TCHAR = LastCharV
                aChar.MoveFromTo(aChar.CharBox.StartPt, lChar.CharBox.EndPt)
            End If
            _Characters.Add(aChar)
        End Sub
        Public Sub Add(aChar As TCHAR)

            _Characters.Add(aChar)
        End Sub
        Public Sub CopyDirections(aPlane As TPLANE)

            If TPLANE.IsNull(aPlane) Then Return
            _Characters.CopyDirections(aPlane)
        End Sub
        Public Sub CopyDirections(aCharBox As TCHARBOX)

            If TPLANE.IsNull(aCharBox) Then Return
            _Characters.CopyDirections(aCharBox)
        End Sub
        Public Sub Translate(aTranslation As TVECTOR, Optional bResetTanslation As Boolean = False)
            If TVECTOR.IsNull(aTranslation) Then Return
            _Characters.Translate(aTranslation)
            'LeadChar.Translate(aTranslation)
            If bResetTanslation Then aTranslation.SetCoordinates(0, 0, 0, -1)
        End Sub
        Public Function MoveFromTo(aFromPt As TVECTOR, aToPt As TVECTOR) As Double
            Dim _rVal As Double = 0
            Dim aDir As TVECTOR
            aDir = aFromPt.DirectionTo(aToPt, False, rDistance:=_rVal)
            If _rVal <> 0 Then Translate(aDir * _rVal)
            Return _rVal
        End Function
        Public Sub Rescale(aScaleFactor As Double, aRefPt As TVECTOR)
            _Characters.Rescale(aScaleFactor, aRefPt)
            'LeadChar.Rescale(aScaleFactor, aRefPt)
        End Sub
        Public Sub Rotate(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressNorm As Boolean = False)
            'LeadChar.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
            _Characters.RotateAbout(aOrigin, aAxis, aAngle, bInRadians, bSuppressNorm)
        End Sub
        Public Function UpdateBounds(Optional bSetCharIndexes As Boolean = False) As TCHARBOX
            Dim rBoundingRectangle As New TPLANE
            Return UpdateBounds(rBoundingRectangle, bSetCharIndexes)
        End Function
        Public Function UpdateBounds(ByRef rBoundingRectangle As TPLANE, Optional bSetCharIndexes As Boolean = False) As TCHARBOX
            Dim myBox As TCHARBOX = CharacterBox
            rBoundingRectangle = myBox.Plane(False)

            If _Characters.Count <= 0 Then Return myBox
            Dim fChar As TCHAR = _Characters.Item(1)
            myBox.Ascent = fChar.Ascent
            myBox.Descent = fChar.Descent
            myBox.Width = myBox.Ascent
            If _Characters.CharacterCount <= 0 Then
                CharacterBox = myBox
                Return myBox
            End If
            Dim aChar As TCHAR = _Characters.Item(1)
            Dim bChar As TCHAR
            aChar.CharBox.CopyDirections(myBox)
            Dim stkSty As dxxCharacterStackStyles
            'there should always be the null char as the lead char
            Dim chrCnt As Integer = Characters.Count
            Dim ci As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim d1 As Double
            Dim aDir As TVECTOR
            Dim aBnds As New TPLANE
            Dim bBnds As New TPLANE
            Dim tBnds As New TPLANE


            Dim bFirstChar As Boolean
            Dim myPl As TPLANE
            Dim baseHt As Double = aChar.CharHeight
            Dim basePt As TVECTOR = myBox.BasePt.Clone
            myBox.Width = 0

            Dim baseLine As TLINE = myBox.BaseLine(aLengthAdder:=10)
            myBox.ObliqueAngle = 0
            Dim dX As Double = 0
            Dim dY As Double = 0

            Dim bFlg As Boolean
            Dim cLine As New TLINE(myBox.Vector(0, 0.5 * baseHt), myBox.Vector(100, 0.5 * baseHt))
            Dim tLine As New TLINE(myBox.Vector(0, baseHt), myBox.Vector(100, baseHt))
            Dim vLine As New TLINE(myBox.Vector(0, 0), myBox.Vector(0, 100, True))
            Dim aIDs As New List(Of Integer)
            Dim aSet As New TCHARS(myBox)
            Dim bIDs As New List(Of Integer)
            Dim bSet As New TCHARS(myBox)

            Dim aBox As TCHARBOX
            Dim bBox As TCHARBOX
            Dim cBox As TCHARBOX
            Dim tBox As TCHARBOX
            Dim eVecs As TVECTORS
            Dim ascntBox As TCHARBOX
            Dim superScript As Boolean
            Dim SubScript As Boolean
            Dim lnidx As Integer = 0
            If CharacterCount > 0 Then
                myBox.Ascent = 0
                myBox.Descent = 0
            Else
                d1 = aChar.CharHeight / aChar.Ascent
                myBox.Ascent = aChar.Ascent * d1
                myBox.Descent = aChar.Descent * d1
            End If
            myPl = myBox.Plane(True)
            'myPl.Origin = myBox.BasePt.Clone
            rBoundingRectangle = myPl.Clone(True, "BoundingRectangle")
            dY = 0
            If Vertical Then
                'determine overall width for vertical strings
                Dim vi As Integer = 0
                For ci = 1 To chrCnt
                    aChar = Characters.Item(ci)
                    If Not aChar.IsFormatCode Then
                        vi += 1
                        aBnds = aChar.BoundingRectangle
                        If aBnds.Width > myBox.Width Then myBox.Width = aBnds.Width
                        If vi = 1 Then
                            v1 = aChar.CharBox.BasePt
                            v2 = dxfProjections.ToLine(v1, baseLine, rOrthoDirection:=aDir, rDistance:=d1)
                            If d1 <> 0 Then
                                If aDir.Equals(myBox.YDirection) Then dY = d1 Else dY = -d1   'put the first char on the baseline
                            End If
                        End If
                    End If
                Next ci
                v1 = myBox.BasePt.Clone
                v1 += myBox.XDirection * myBox.Width / 2
                v2 = v1 + (myBox.YDirection * 100)
                cLine = New TLINE(v1, v2)
            End If

            For ci = 1 To chrCnt
                aChar = _Characters.Item(ci)
                aChar.CharBox.CopyDirections(myBox)
                If Not aChar.IsFormatCode Then
                    lnidx += 1
                    If bSetCharIndexes Then aChar.LineIndex = lnidx
                    aBnds = aChar.BoundingRectangle

                End If
                bFirstChar = lnidx = 1

                If Not Vertical Then

                    '========================================
                    'HORIZONTAL TEXT
                    '========================================
                    'adjust to align relative to baseline
                    If Not aChar.IsFormatCode Then

                        If bFirstChar Then
                            'force first char to the left edge of the text box
                            v1 = aChar.CharBox.Point(dxxRectanglePts.TopLeft)
                            v2 = dxfProjections.ToLine(v1, vLine, rOrthoDirection:=aDir, rDistance:=d1)
                            If d1 <> 0 And aDir.Equals(myPl.XDirection, bCompareInverse:=True, aPrecis:=3, rIsInverseEqual:=bFlg) Then
                                If Not bFlg Then dX = d1 Else dX = -d1
                            End If
                            v1 = aBnds.Point(dxxRectanglePts.TopLeft)
                            v2 = dxfProjections.ToLine(v1, vLine, rOrthoDirection:=aDir, rDistance:=d1)
                            If d1 <> 0 Then
                                aChar.Translate(aDir * d1)
                                aBnds.Translate(aDir * d1)
                                _Characters.SetItem(ci, aChar)
                                myBox.BasePt = aChar.CharBox.BasePt.Clone()
                                CharacterBox = myBox
                                myPl = myBox.Plane(True)
                                rBoundingRectangle = myPl.Clone(True, "BoundingRectangle")
                            End If


                        End If
                        If dX <> 0 Then
                            aChar.Translate(myPl.XDirection * dX)
                            _Characters.SetItem(ci, aChar)
                        End If
                        aIDs.Clear() : aIDs.Add(ci)
                        aSet.Clear() : aSet.Add(aChar)
                        bSet.Clear() : bIDs.Clear()
                        stkSty = dxxCharacterStackStyles.None
                        bFlg = False
                        'collect stacked text to arrange collectively
                        If aChar.StackID <> 0 Then
                            stkSty = aChar.StackStyle
                            superScript = stkSty = dxxCharacterStackStyles.SuperScript
                            SubScript = stkSty = dxxCharacterStackStyles.SubScript
                            If SubScript Then
                                aSet.Clear()
                                bSet.Add(aChar)
                                aIDs.Clear() : bIDs.Add(ci)
                            End If
                            For j = ci + 1 To chrCnt
                                bChar = _Characters.Item(j)
                                If bChar.StackID = aChar.StackID Then
                                    If Not bChar.IsFormatCode Then
                                        lnidx += 1
                                        If bSetCharIndexes Then aChar.LineIndex = lnidx
                                        If SubScript Then
                                            bSet.Add(bChar) : bIDs.Add(j)
                                        Else
                                            aSet.Add(bChar) : aIDs.Add(j)
                                        End If
                                    End If
                                    ci += 1
                                ElseIf bChar.StackID = aChar.StackID + 1 Then
                                    If Not bChar.IsFormatCode Then
                                        lnidx += 1
                                        If bSetCharIndexes Then aChar.LineIndex = lnidx
                                        bSet.Add(bChar) : bIDs.Add(j)
                                    End If
                                    ci += 1
                                Else
                                    Exit For
                                End If
                            Next j
                        End If
                        ' char alignment
                        If stkSty = dxxCharacterStackStyles.SubScript Then
                            aBox = bSet.UpdateBounds(False, aBnds)
                            bBox = aBox.Clone : bBnds = aBnds.Clone
                        Else
                            aBox = aSet.UpdateBounds(False, aBnds)
                            If bSet.CharacterCount > 0 Then
                                bBox = bSet.UpdateBounds(False, bBnds)
                            End If
                        End If
                        tBnds = aBnds.Clone : tBox = aBox.Clone
                        ascntBox = aBox.Clone : ascntBox.Descent = 0 ': ascntBox.Ascent = aBnds.Point(dxxRectanglePts.TopLeft).WithRespectTo(aBox).Y
                        ascntBox.Ascent = aChar.CharHeight
                        'align stacks
                        If stkSty <> dxxCharacterStackStyles.None Then
                            'align the bottom set below the top set of chars
                            v1 = aBnds.Point(dxxRectanglePts.BottomLeft)
                            v2 = bBnds.Point(dxxRectanglePts.TopLeft)
                            v1 += myPl.YDirection * -0.07 * baseHt
                            If stkSty = dxxCharacterStackStyles.Diagonal Then
                                'shift the bottom set right by the length of the top one
                                v1 += myPl.XDirection * aBnds.Width
                            Else
                                'center the bottom on the top one
                                v1 += myPl.XDirection * (0.5 * aBnds.Width - 0.5 * bBnds.Width)
                                If bBnds.Width > aBnds.Width Then
                                    v3 = myPl.XDirection * (0.5 * bBnds.Width - 0.5 * aBnds.Width)
                                    bFlg = True
                                    aSet.Translate(v3) : aBox.BasePt += v3 : aBnds.Origin += v3
                                    bSet.Translate(v3) : bBox.BasePt += v3 : bBnds.Origin += v3
                                    ascntBox.BasePt += v3 : v1 += v3 : v2 += v3
                                    tBnds.Origin += v3 : tBox.BasePt += v3
                                End If
                            End If
                            aDir = v2.DirectionTo(v1, False, rDistance:=d1)
                            v1 = aDir * d1
                            bSet.Translate(v1) : bBnds.Origin += v1 : bBox.BasePt += v1
                            aBnds.ExpandToVectors(bBnds.Corners)
                            ascntBox = bBox.Clone : ascntBox.Descent = 0
                            ascntBox.Ascent = aBnds.Height - bBox.Descent
                            If bFlg Then
                                'make the bBox the aBox by swapping because it is the longer group
                                cBox = aBox : aBox = bBox : bBox = cBox
                            End If
                        End If
                        v2 = basePt.Clone : v1 = aBox.BasePt
                        aDir = v1.DirectionTo(v2, False, rDistance:=d1)

                        If d1 <> 0 Then
                            v1 = aDir * d1
                            aSet.Translate(v1) : aBox.BasePt += v1 : aBnds.Origin += v1
                            ascntBox.BasePt += v1 : tBnds.Origin += v1 : tBox.BasePt += v1
                            If bSet.CharacterCount > 0 Then
                                bSet.Translate(v1) : bBox.BasePt += v1 : bBnds.Origin += v1
                            End If
                        End If


                        'vertical alignment
                        If aChar.CharAlign = dxxCharacterAlignments.Center Then
                            v1 = ascntBox.Center(True) : v2 = v1.ProjectedTo(cLine)
                            aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        ElseIf aChar.CharAlign = dxxCharacterAlignments.Top Then
                            v1 = ascntBox.Point(dxxRectanglePts.TopLeft) : v2 = v1.ProjectedTo(tLine)
                            aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        Else
                            v1 = ascntBox.Point(dxxRectanglePts.BottomLeft) : v2 = v1.ProjectedTo(baseLine)
                            aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        End If
                        If d1 <> 0 Then
                            v1 = aDir * d1
                            aSet.Translate(v1) : aBox.BasePt += v1 : aBnds.Origin += v1
                            ascntBox.BasePt += v1 : tBnds.Origin += v1 : tBox.BasePt += v1
                            If bSet.CharacterCount > 0 Then
                                bSet.Translate(v1) : bBox.BasePt += v1 : bBnds.Origin += v1
                            End If
                        End If
                        If stkSty = dxxCharacterStackStyles.Horizontal Then
                            aChar = aSet.Item(1)
                            cLine = tBox.BaseLine
                            cLine.SPT = cLine.SPT.ProjectedTo(aBnds.Edge(dxxRectangleLines.LeftEdge))
                            cLine.EPT = cLine.SPT + myPl.XDirection * aBnds.Width
                            cLine.Translate(myPl.YDirection * (-0.1 * aChar.CharHeight))
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))
                            aSet.SetItem(1, aChar)
                        ElseIf stkSty = dxxCharacterStackStyles.Diagonal Then
                            aChar = aSet.Item(1)
                            cLine.SPT = tBnds.Point(dxxRectanglePts.TopRight)
                            cLine.EPT = bBnds.Point(dxxRectanglePts.BottomLeft)
                            cLine.Rotate(cLine.MPT, myPl.ZDirection, -45)
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))
                            aSet.SetItem(1, aChar)
                        End If
                        'capture extents
                        eVecs = aBnds.Corners
                        rBoundingRectangle.ExpandToVectors(eVecs)
                        v1 = eVecs.Item(1).WithRespectTo(myPl) 'top left
                        v3 = eVecs.Item(3).WithRespectTo(myPl) 'bottom right
                        'expand ascent
                        If v1.Y > myBox.Ascent Then myBox.Ascent = v1.Y
                        'expand descent
                        If v3.Y < 0 And Math.Abs(v3.Y) > myBox.Descent Then myBox.Descent = Math.Abs(v3.Y)
                        'capture width
                        If v3.X > myBox.Width Then myBox.Width = v3.X
                        basePt = aSet.LastChar.CharBox.EndPt.ProjectedTo(baseLine) '  aBox.Vector(aBox.Width(True), 0).ProjectedTo(baseLine)
                        If bSet.CharacterCount > 0 Then
                            v1 = bSet.LastChar.CharBox.EndPt.ProjectedTo(baseLine) 'bBox.Vector(bBox.Width(True), 0).ProjectedTo(baseLine)
                            If basePt.DirectionTo(v1).Equals(myPl.XDirection, 3) Then basePt = v1
                        End If
                    End If


                Else
                    '========================================
                    'VERTICAL TEXT
                    '========================================
                    'adjust for baseline alignment
                    If dY <> 0 Then
                        v1 = myBox.YDirection * dY
                        aChar.Translate(v1)
                        aBnds.Origin += v1
                    End If
                    If Not aChar.IsFormatCode Then
                        aIDs.Clear() : aIDs.Add(ci)
                        aSet.Clear() : aSet.Add(aChar)
                        bSet.Clear() : bIDs.Clear()
                        stkSty = dxxCharacterStackStyles.None
                        bFlg = False
                        'collect stacked text to arrange collectively
                        If aChar.StackID <> 0 Then
                            stkSty = aChar.StackStyle
                            superScript = stkSty = dxxCharacterStackStyles.SuperScript
                            SubScript = stkSty = dxxCharacterStackStyles.SubScript
                            If SubScript Then
                                aSet.Clear()
                                bSet.Add(aChar)
                                aIDs.Clear() : bIDs.Add(ci)
                            End If
                            For j = ci + 1 To chrCnt
                                bChar = _Characters.Item(j)
                                If bChar.StackID = aChar.StackID Then
                                    If Not bChar.IsFormatCode Then
                                        If bSetCharIndexes Then
                                            lnidx += 1
                                            aChar.LineIndex = lnidx
                                        End If
                                        If SubScript Then
                                            bSet.Add(bChar) : bIDs.Add(j)
                                        Else
                                            aSet.Add(bChar) : aIDs.Add(j)
                                        End If
                                    End If
                                    ci += 1
                                ElseIf bChar.StackID = aChar.StackID + 1 Then
                                    If Not bChar.IsFormatCode Then
                                        If bSetCharIndexes Then
                                            lnidx += 1
                                            aChar.LineIndex = lnidx
                                        End If
                                        bSet.Add(bChar) : bIDs.Add(j)
                                    End If
                                    ci += 1
                                Else
                                    Exit For
                                End If
                            Next j
                        End If
                        ' char alignment
                        If stkSty = dxxCharacterStackStyles.SubScript Then
                            aBox = bSet.UpdateBounds(False, aBnds)
                            bBox = aBox.Clone : bBnds = aBnds.Clone
                        Else
                            aBox = aSet.UpdateBounds(False, aBnds)
                            If bSet.CharacterCount > 0 Then
                                bBox = bSet.UpdateBounds(False, bBnds)
                            End If
                        End If
                        tBnds = aBnds.Clone : tBox = aBox.Clone
                        ascntBox = aBox.Clone : ascntBox.Descent = 0 ': ascntBox.Ascent = aBnds.Point(dxxRectanglePts.TopLeft).WithRespectTo(aBox).Y
                        ascntBox.Ascent = aChar.CharHeight
                        'align stacks
                        If stkSty <> dxxCharacterStackStyles.None Then
                            'align the bottom set below the top set of chars
                            v1 = aBnds.Point(dxxRectanglePts.BottomLeft)
                            v2 = bBnds.Point(dxxRectanglePts.TopLeft)
                            v1 += myPl.YDirection * -0.07 * baseHt
                            If stkSty = dxxCharacterStackStyles.Diagonal Then
                                'shift the bottom set right by the length of the top one
                                v1 += myPl.XDirection * aBnds.Width
                            Else
                                'center the bottom on the top one
                                v1 += myPl.XDirection * (0.5 * aBnds.Width - 0.5 * bBnds.Width)
                                If bBnds.Width > aBnds.Width Then
                                    v3 = myPl.XDirection * (0.5 * bBnds.Width - 0.5 * aBnds.Width)
                                    bFlg = True
                                    aSet.Translate(v3) : aBox.BasePt += v3 : aBnds.Origin += v3
                                    bSet.Translate(v3) : bBox.BasePt += v3 : bBnds.Origin += v3
                                    ascntBox.BasePt += v3 : v1 += v3 : v2 += v3
                                    tBnds.Origin += v3 : tBox.BasePt += v3
                                End If
                            End If
                            aDir = v2.DirectionTo(v1, False, rDistance:=d1)
                            v1 = aDir * d1
                            bSet.Translate(v1) : bBnds.Origin += v1 : bBox.BasePt += v1
                            aBnds.ExpandToVectors(bBnds.Corners)
                            ascntBox = bBox.Clone : ascntBox.Descent = 0
                            ascntBox.Ascent = aBnds.Height - bBox.Descent
                            If bFlg Then
                                'make the bBox the aBox by swapping because it is the longer group
                                cBox = aBox : aBox = bBox : bBox = cBox
                            End If
                        End If
                        'align the chars
                        v2 = basePt.Clone
                        d1 = 0
                        If bFirstChar Then
                            v1 = aBox.BasePt.ProjectedTo(aBnds.Edge(dxxRectangleLines.LeftEdge))
                            v1 = dxfProjections.ToLine(v1, vLine)
                        Else
                            v1 = aBnds.Point(dxxRectanglePts.TopLeft)
                        End If
                        aDir = v1.DirectionTo(v2, False, rDistance:=d1)
                        If d1 <> 0 Then
                            v1 = aDir * d1
                            aSet.Translate(v1) : aBox.BasePt += v1 : aBnds.Origin += v1
                            ascntBox.BasePt += v1 : tBnds.Origin += v1 : tBox.BasePt += v1
                            If bSet.CharacterCount > 0 Then
                                bSet.Translate(v1) : bBox.BasePt += v1 : bBnds.Origin += v1
                            End If
                        End If
                        If stkSty = dxxCharacterStackStyles.Horizontal Then
                            aChar = aSet.Item(1)
                            cLine = tBox.BaseLine
                            cLine.SPT = cLine.SPT.ProjectedTo(aBnds.Edge(dxxRectangleLines.LeftEdge))
                            cLine.EPT = cLine.SPT + myPl.XDirection * aBnds.Width
                            cLine.Translate(myPl.YDirection * (-0.1 * aChar.CharHeight))
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))
                            aSet.SetItem(1, aChar)
                        ElseIf stkSty = dxxCharacterStackStyles.Diagonal Then
                            aChar = aSet.Item(1)
                            cLine.SPT = tBnds.Point(dxxRectanglePts.TopRight)
                            cLine.EPT = bBnds.Point(dxxRectanglePts.BottomLeft)
                            cLine.Rotate(cLine.MPT, myPl.ZDirection, -45)
                            aChar.AccentPaths.AddLine(cLine.WithRespectTo(aChar.CharBox))
                            aSet.SetItem(1, aChar)
                        End If
                        'capture extents
                        eVecs = aBnds.Corners
                        rBoundingRectangle.ExpandToVectors(eVecs)
                        'update the ascent
                        If bFirstChar Then
                            v1 = aBnds.Point(dxxRectanglePts.TopCenter).WithRespectTo(myPl)
                            myBox.Ascent = v1.Y
                        End If
                        v1 = aBnds.Point(dxxRectanglePts.BottomCenter).WithRespectTo(myPl)
                        If v1.Y < 0 And Math.Abs(v1.Y) > myBox.Descent Then myBox.Descent = Math.Abs(v1.Y)
                        v1 = aBnds.Point(dxxRectanglePts.TopRight).WithRespectTo(myPl)
                        If v1.X > myBox.Width Then myBox.Width = v1.X
                        basePt = aBox.Point(dxxRectanglePts.BottomLeft, False) + myPl.YDirection * (-0.07 * baseHt)
                        basePt = dxfProjections.ToLine(basePt, vLine)

                    End If
                End If  'vertical/horizontal
                'save the changes
                For j = 1 To aSet.CharacterCount
                    _Characters.SetItem(aIDs.Item(j - 1), aSet.Item(j))
                Next j
                For j = 1 To bSet.CharacterCount
                    _Characters.SetItem(bIDs.Item(j - 1), bSet.Item(j))
                Next j


            Next ci
            If Not Vertical Then
                'If dX <> 0 Then myBox.BasePt += myPl.XDirection * -dX
                myBox.Width = rBoundingRectangle.Width
                myBox.BasePt = rBoundingRectangle.Point(dxxRectanglePts.TopLeft) + myPl.YDirection * -myBox.Ascent
            End If
            CharacterBox = myBox
            Return myBox
        End Function
        Public Function TextRectangle(Optional bAscentOnly As Boolean = False) As TPLANE
            '^the rectangle containing the characters on the plane that bounds the string (ascent & descent)
            Dim wd As Double = Width
            Dim cBox As TCHARBOX = CharacterBox.Clone

            Dim dscnt As Double = Descent
            If bAscentOnly Then dscnt = 0
            cBox.Descent = dscnt
            'Dim _rVal As New TPLANE(cBox.Center.Projected(cBox.YDirection, -dscnt), cBox.XDirection, cBox.YDirection, Ascent, dscnt, wd, 0, "Text Rectangle [Sub-String " & LineNo & "]")
            Return cBox.Plane(False)
        End Function
        Public Function GetByGroupIndex(aGroupID As Integer, aStartID As Integer, ByRef rLastID As Integer) As TSTRING
            Dim i As Integer
            Dim rStr As TSTRING = Clone(True)
            Dim aChar As TCHAR
            rLastID = 0
            If aGroupID < 0 Then aGroupID = 1
            For i = aStartID To CharacterCount
                aChar = CharacterV(i)
                If aChar.GroupIndex = aGroupID Then
                    rStr.AddChar(aChar)
                    rLastID = i
                Else
                    If aChar.GroupIndex > aGroupID Then
                        Exit For
                    End If
                End If
            Next i
            Return rStr
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSTRING(Me)
        End Function

        Public Sub Clear()
            _Characters.Clear()
        End Sub
        Public Function Clone(Optional bNoChars As Boolean = False) As TSTRING
            Dim _rVal As New TSTRING(Me)
            If bNoChars Then _rVal.Clear()
            Return _rVal
        End Function
        Public Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, bMirrorDirections As Boolean, Optional bSuppressCheck As Boolean = False)

            If Not bSuppressCheck Then
                If dxfProjections.DistanceTo(aSP, aEP) < 0.00001 Then Return
            End If
            'LeadChar.Mirror(aSP, aEP, bMirrorDirections, True)
            _Characters.Mirror(aSP, aEP, bMirrorDirections, True)
        End Sub
#End Region 'Methods

    End Structure 'TSTRING
    Friend Structure TSUBSTRINGS
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Strings() As TSTRING
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            'init ---------------------------------------
            _Init = True
            ReDim _Strings(-1)
            'init ---------------------------------------
            For i As Integer = 1 To aCount
                AddV(New TSTRING(""))
            Next
        End Sub

        Public Sub New(aStrings As TSUBSTRINGS)
            'init ---------------------------------------
            _Init = True
            ReDim _Strings(-1)
            'init ---------------------------------------
            If aStrings._Init Then _Strings = aStrings._Strings.Clone()
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Strings(-1)
                End If
                Return _Strings.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function Clone() As TSUBSTRINGS
            Return New TSUBSTRINGS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSUBSTRINGS(Me)
        End Function
        Public Function CharacterCount(Optional aIndex As Integer = 0) As Integer
            '#1an optional substring index to get the character count for
            'returns the number of visible characters
            Dim _rVal As Integer = 0
            For i As Integer = 1 To Count
                If aIndex <= 0 Or aIndex = i Then _rVal += ItemV(i).CharacterCount
            Next
            Return _rVal
        End Function
        Friend Function ItemV(aIndex As Integer) As TSTRING
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TSTRING("")
            End If
            _Strings(aIndex - 1).LineNo = aIndex
            Return _Strings(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TSTRING)
            If aIndex < 1 Or aIndex > Count Then Return
            _Strings(aIndex - 1) = value
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Strings(-1)
        End Sub
        Friend Sub AddV(aString As TSTRING)

            System.Array.Resize(_Strings, Count + 1)
            _Strings(_Strings.Count - 1) = aString
            _Strings(_Strings.Count - 1).LineNo = _Strings.Count
        End Sub
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return

            Dim aStr As TSTRING
            For li As Integer = 1 To Count
                aStr = _Strings(li - 1)
                aStr.Translate(aTranslation)
                _Strings(li - 1) = aStr
            Next li
        End Sub
        Public Sub Rotate(aAxis As TLINE, aAngle As Double)
            Dim org As TVECTOR = aAxis.SPT
            Dim dir As TVECTOR = aAxis.Direction
            Dim aStr As TSTRING
            For li As Integer = 1 To Count
                aStr = _Strings(li - 1)
                aStr.Rotate(org, dir, aAngle, bSuppressNorm:=True)
                _Strings(li - 1) = aStr
            Next li
        End Sub

        Public Function ToList() As List(Of TSTRING)
            If Count <= 0 Then Return New List(Of TSTRING)
            Return _Strings.ToList()
        End Function
#End Region 'Methods
    End Structure 'TSUBSTRINGS
    Friend Structure TSTRINGS
        Implements ICloneable
#Region "Members"


        Private _Members As TSUBSTRINGS
        Friend CharBox As TCHARBOX

#End Region 'Members
#Region "Constructors"
        Public Sub New(aPlane As TPLANE, aDomain As dxxDrawingDomains, Optional aTextType As dxxTextTypes = dxxTextTypes.Multiline)
            'init -------------------------------------------------------
            Formats = New TCHARFORMAT(dxxCharacterAlignments.Bottom)

            _Alignment = dxxMTextAlignments.BaselineLeft
            _AlignmentPt = TVECTOR.Zero
            _Domain = dxxDrawingDomains.Model
            _BaseCharHeight = 1
            _DTextString = ""
            _FitLength = 0
            _FormatString = ""
            _HasFormats = False
            _LayerName = "0"

            _LineSpacingStyle = dxxLineSpacingStyles.AtLeast
            _LineSpacingFactor = 1

            _LineWeight = dxxLineWeights.ByDefault
            _RecomputeCharPaths = False
            _StrAlignH = dxxTextJustificationsHorizontal.Left
            _StrAlignV = dxxTextJustificationsVertical.Baseline
            _SubStrAlignH = dxxTextJustificationsHorizontal.Left
            _SubStrAlignV = dxxTextJustificationsVertical.Baseline
            CharBox = New TCHARBOX(AlignmentPtV, 0, 0, 0)
            _Members = New TSUBSTRINGS(-1)

            _TextType = dxxTextTypes.Multiline
            'init -------------------------------------------------------
            _TextType = aTextType
            If TPLANE.IsNull(aPlane) Then aPlane = TPLANE.World
            _AlignmentPt = New TVECTOR(aPlane.Origin)
            CharBox.CopyDirections(aPlane)
            If aDomain >= dxxDrawingDomains.Screen And aDomain <= dxxDrawingDomains.Paper Then Domain = aDomain

        End Sub

        Friend Sub New(aStrings As dxfStrings)
            'init -------------------------------------------------------
            Formats = New TCHARFORMAT(dxxCharacterAlignments.Bottom)

            _Alignment = dxxMTextAlignments.BaselineLeft
            _AlignmentPt = TVECTOR.Zero
            _Domain = dxxDrawingDomains.Model
            _BaseCharHeight = 1
            _DTextString = ""
            _FitLength = 0
            _FormatString = ""
            _HasFormats = False
            _LayerName = "0"

            _LineSpacingStyle = dxxLineSpacingStyles.AtLeast
            _LineSpacingFactor = 1

            _LineWeight = dxxLineWeights.ByDefault
            _RecomputeCharPaths = False
            _StrAlignH = dxxTextJustificationsHorizontal.Left
            _StrAlignV = dxxTextJustificationsVertical.Baseline
            _SubStrAlignH = dxxTextJustificationsHorizontal.Left
            _SubStrAlignV = dxxTextJustificationsVertical.Baseline
            CharBox = New TCHARBOX(AlignmentPtV, 0, 0, 0)
            _Members = New TSUBSTRINGS(-1)

            _TextType = dxxTextTypes.Multiline
            'init -------------------------------------------------------
            If aStrings Is Nothing Then Return

            Alignment = aStrings.Alignment
            AlignmentPtV = aStrings.AlignmentPtV
            Domain = aStrings.Domain
            BaseCharHeight = aStrings.BaseCharHeight
            DTextString = aStrings.DTextString
            FitLength = aStrings.FitLength
            FormatString = aStrings.FormatString
            HasFormats = aStrings.HasFormats
            LayerName = aStrings.LayerName

            LineSpacingFactor = aStrings.LineSpacingFactor
            LineSpacingStyle = aStrings.LineSpacingStyle

            LineWeight = aStrings.LineWeight
            RecomputeCharPaths = aStrings.RecomputeCharPaths
            StrAlignH = aStrings.StrAlignH
            StrAlignV = aStrings.StrAlignV
            SubStrAlignH = aStrings.SubStrAlignH
            SubStrAlignV = aStrings.SubStrAlignV


            For Each str As dxoString In aStrings
                _Members.AddV(str.Structure_Get())
            Next
        End Sub

        Public Sub New(aStrings As TSTRINGS)

            'init -------------------------------------------------------
            Formats = New TCHARFORMAT(aStrings.Formats)

            _Alignment = aStrings._Alignment
            _AlignmentPt = New TVECTOR(aStrings._AlignmentPt)
            _Domain = aStrings._Domain
            _BaseCharHeight = aStrings._BaseCharHeight
            _DTextString = aStrings._DTextString
            _FitLength = aStrings._FitLength
            _FormatString = aStrings._FormatString
            _HasFormats = aStrings._HasFormats
            _LayerName = aStrings.LayerName

            _LineSpacingStyle = aStrings.LineSpacingStyle
            _LineSpacingFactor = aStrings._LineSpacingFactor

            _LineWeight = aStrings._LineWeight
            _RecomputeCharPaths = aStrings._RecomputeCharPaths
            _StrAlignH = aStrings._StrAlignH
            _StrAlignV = aStrings._StrAlignV
            _SubStrAlignH = aStrings._SubStrAlignH
            _SubStrAlignV = aStrings._SubStrAlignV
            CharBox = New TCHARBOX(aStrings.CharBox)
            _Members = New TSUBSTRINGS(aStrings._Members)

            _TextType = aStrings._TextType

            'init -------------------------------------------------------
        End Sub

#End Region 'Constructors

#Region "Properties"
        Private _LineWeight As dxxLineWeights
        Public Property LineWeight As dxxLineWeights
            Get
                Return _LineWeight
            End Get
            Friend Set(value As dxxLineWeights)
                _LineWeight = value
            End Set
        End Property

        Private _LayerName As String
        Public Property LayerName As String
            Get
                Return _LayerName
            End Get
            Friend Set(value As String)
                _LayerName = value
            End Set
        End Property



        Private _SubStrAlignV As dxxTextJustificationsVertical
        Public Property SubStrAlignV As dxxTextJustificationsVertical
            Get
                Return _SubStrAlignV
            End Get
            Friend Set(value As dxxTextJustificationsVertical)
                _SubStrAlignV = value
            End Set
        End Property

        Private _SubStrAlignH As dxxTextJustificationsHorizontal
        Public Property SubStrAlignH As dxxTextJustificationsHorizontal
            Get
                Return _SubStrAlignH
            End Get
            Friend Set(value As dxxTextJustificationsHorizontal)
                _SubStrAlignH = value
            End Set
        End Property

        Private _StrAlignV As dxxTextJustificationsVertical
        Public Property StrAlignV As dxxTextJustificationsVertical
            Get
                Return _StrAlignV
            End Get
            Friend Set(value As dxxTextJustificationsVertical)
                _StrAlignV = value
            End Set
        End Property

        Private _StrAlignH As dxxTextJustificationsHorizontal
        Public Property StrAlignH As dxxTextJustificationsHorizontal
            Get
                Return _StrAlignH
            End Get
            Friend Set(value As dxxTextJustificationsHorizontal)
                _StrAlignH = value
            End Set
        End Property

        Private _BaseCharHeight As Double
        Public Property BaseCharHeight As Double
            Get
                Return _BaseCharHeight
            End Get
            Friend Set(value As Double)
                _BaseCharHeight = value
            End Set
        End Property

        Private _Domain As dxxDrawingDomains
        Public Property Domain As dxxDrawingDomains
            Get
                Return _Domain
            End Get
            Friend Set(value As dxxDrawingDomains)
                _Domain = value
            End Set
        End Property

        Private _RecomputeCharPaths As Boolean
        Public Property RecomputeCharPaths As Boolean
            Get
                Return _RecomputeCharPaths
            End Get
            Friend Set(value As Boolean)
                _RecomputeCharPaths = value
            End Set
        End Property
        Private _AlignmentPt As TVECTOR
        Friend Property AlignmentPtV As TVECTOR
            Get
                Return _AlignmentPt
            End Get
            Set(value As TVECTOR)
                _AlignmentPt = value
            End Set
        End Property

        Private _Alignment As dxxMTextAlignments
        Public Property Alignment As dxxMTextAlignments
            '^the Alignment of the text object with respect to it's insertion vector
            '~default = dxfBaselineLeft
            Get
                Return _Alignment
            End Get
            Friend Set(value As dxxMTextAlignments)
                If value < 1 Or value > 14 Then value = dxxMTextAlignments.BaselineLeft
                _Alignment = value

            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return _Members.Count
            End Get
        End Property

        Private _DTextString As String
        Public Property DTextString As String
            Get
                Return _DTextString
            End Get
            Friend Set(value As String)
                _DTextString = value
            End Set
        End Property

        Private _FitLength As Double
        Public Property FitLength As Double
            Get
                Return _FitLength
            End Get
            Friend Set(value As Double)
                _FitLength = value
            End Set
        End Property

        Friend Formats As TCHARFORMAT

        Private _FormatString As String
        Public Property FormatString As String
            Get
                Return _FormatString
            End Get
            Friend Set(value As String)
                _FormatString = value
            End Set
        End Property

        Private _HasFormats As Boolean
        Public Property HasFormats As Boolean
            Get
                Return _HasFormats
            End Get
            Friend Set(value As Boolean)
                _HasFormats = value
            End Set
        End Property

        Private _LineSpacingFactor As Double
        Public Property LineSpacingFactor As Double
            Get
                If _LineSpacingFactor <= 0 Then _LineSpacingFactor = 1
                Return LineSpacingFactor
            End Get
            Friend Set(value As Double)
                If value < 0.25 Then value = 0.25
                If value > 4 Then value = 4
                _LineSpacingFactor = Math.Round(value, 4)
            End Set
        End Property

        Private _LineSpacingStyle As dxxLineSpacingStyles
        Public Property LineSpacingStyle As dxxLineSpacingStyles
            Get
                If _LineSpacingStyle <> dxxLineSpacingStyles.AtLeast And _LineSpacingStyle <> dxxLineSpacingStyles.Exact Then
                    _LineSpacingStyle = dxxLineSpacingStyles.AtLeast
                End If
                Return _LineSpacingStyle
            End Get
            Friend Set(value As dxxLineSpacingStyles)
                If value <> dxxLineSpacingStyles.AtLeast And value <> dxxLineSpacingStyles.Exact Then Return
                _LineSpacingStyle = value
            End Set
        End Property


        Friend ReadOnly Property PlaneV As TPLANE
            Get
                Return CharBox.Plane(False)
            End Get
        End Property
        Public Property Rotation As Double
            Get
                Return Formats.Rotation
            End Get
            Set(value As Double)
                Formats.Rotation = value
            End Set
        End Property
        Public ReadOnly Property BoundingRectangleV As TPLANE
            Get
                Dim _rVal As TPLANE = CharBox.Plane
                Dim aStr As TSTRING
                _rVal.SetDimensions(0, 0)
                _rVal.Descent = 0
                For i As Integer = 1 To Count
                    aStr = SubString(i)
                    _rVal.ExpandToVectors(aStr.BoundingRectangleV.Corners)
                Next
                _rVal.Name = "String Array Bounds [" & Count & "]"
                Return _rVal
            End Get
        End Property

        Private _TextBoxes As Boolean
        Public Property TextBoxes As Boolean
            Get
                Return _TextBoxes
            End Get
            Friend Set(value As Boolean)
                _TextBoxes = value
            End Set
        End Property
        Friend ReadOnly Property TextRectangleV As TPLANE
            '^the rectangle containing al the characters on the plane that bounds the string collection (ascent & descent)
            Get

                Return CharBox.Plane(False)
            End Get
        End Property

        Private _TextType As dxxTextTypes
        Public Property TextType As dxxTextTypes
            Get
                If _TextType < dxxTextTypes.DText Or _TextType > dxxTextTypes.Multiline Then _TextType = dxxTextTypes.Multiline
                Return _TextType
            End Get
            Friend Set(value As dxxTextTypes)
                If value < dxxTextTypes.DText Or value > dxxTextTypes.Multiline Then Return
                _TextType = value
            End Set
        End Property
        Public ReadOnly Property Characters As dxoCharacters
            Get
                Dim _rVal As New dxoCharacters()

                Dim aStr As TSTRING
                Dim j As Integer
                Dim aChr As TCHAR
                Dim lidx As Integer
                lidx = 1
                For i As Integer = 1 To Count
                    aStr = SubString(i)
                    For j = 1 To aStr.CharacterCount
                        aChr = aStr.CharacterV(j)
                        aChr.LineIndex = lidx
                        lidx += 1
                        _rVal.AddV(aChr)
                    Next j
                    lidx = 1
                Next i
                Return _rVal
            End Get
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function Clone() As TSTRINGS
            Return New TSTRINGS(Me)
        End Function

        Public Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSTRINGS(Me)
        End Function

        Public Function Character(aCharIndex As Integer, Optional aLineNo As Integer = 0) As TCHAR
            Dim _rVal As TCHAR = TCHAR.Null
            'On Error Resume Next
            Dim aStr As TSTRING = Nothing
            Dim cnt As Long
            Dim i As Integer
            Dim idx As Integer
            If aCharIndex <= 0 Then Return Nothing
            If aLineNo > 0 Then
                If aLineNo > Count Then Return Nothing
                aStr = SubString(aLineNo)
                If aCharIndex > 0 And aCharIndex <= aStr.CharacterCount Then
                    _rVal = aStr.CharacterV(aCharIndex)
                    _rVal.LineIndex = aCharIndex
                    _rVal.LineNo = aLineNo
                    aStr.SetChar(aCharIndex, _rVal)
                End If
                SetItem(aLineNo, aStr)
            Else
                cnt = 0
                idx = -1
                For i = 1 To Count
                    aStr = SubString(i)
                    If aCharIndex <= cnt + aStr.CharacterCount Then
                        idx = aCharIndex - cnt
                        Exit For
                    Else
                        cnt += aStr.CharacterCount
                    End If
                Next i
                If idx > 0 And idx <= aStr.CharacterCount Then
                    _rVal = aStr.CharacterV(idx)
                    _rVal.LineIndex = aCharIndex
                    _rVal.LineNo = aStr.LineNo
                End If
            End If
            Return _rVal
        End Function
        Public Function CharacterCount(Optional aIndex As Integer = 0) As Integer
            '#1an optional substring index to get the character count for
            'returns the number of visible characters
            If _Members.Count <= 0 Then Return 0
            Return _Members.CharacterCount(aIndex)
        End Function
        Public Function SubString(aLineNo As Integer) As TSTRING
            If aLineNo < 1 Or aLineNo > _Members.Count Then Return Nothing
            Dim aStr As TSTRING = _Members.ItemV(aLineNo)
            aStr.Vertical = Formats.Vertical
            aStr.CopyDirections(CharBox)
            _Members.SetItem(aLineNo, aStr)
            Return aStr
        End Function
        Public Sub SetItem(aLineNo As Integer, value As TSTRING)
            If aLineNo < 0 Or aLineNo > _Members.Count Then Return
            _Members.SetItem(aLineNo, value)
        End Sub
        Public Overrides Function ToString() As String
            Return $"TSTRINGS[{ Count }] { DTextString }"

        End Function


        Public Sub Clear()
            _Members = New TSUBSTRINGS(0)
        End Sub



#End Region 'Methods
#Region "transformation methods"
        Public Sub RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False)
            _AlignmentPt.RotateAbout(aOrigin, aAxis, aAngle, True, True)
            Dim aStr As TSTRING
            For i = 1 To Count
                aStr = SubString(i)
                aStr.Rotate(aOrigin, aAxis, aAngle, True, True)
                SetItem(i, aStr)
            Next i
        End Sub
        Public Sub Scale(aScaleX As Double, aReference As TVECTOR, Optional aScaleY As Double = 0.0, Optional aScaleZ As Double = 0)
            FitLength *= Math.Abs(aScaleX)
            CharBox.Rescale(aScaleX, aReference)
            Dim aStr As TSTRING
            For i = 1 To Count
                aStr = SubString(i)
                aStr.Rescale(aScaleX, aReference)
                SetItem(i, aStr)
            Next i
        End Sub
        Public Sub TranslateComponents(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return
            CharBox.BasePt += aTranslation
            _Members.Translate(aTranslation)
            'CharBox.BasePt = SubString(1).CharacterBox.BasePt
        End Sub
        Public Sub RotateComponents(aAxis As TLINE, aAngle As Double)
            CharBox.BasePt.RotateAbout(aAxis, aAngle:=aAngle)
            _Members.Rotate(aAxis, aAngle)
            'CharBox.BasePt = SubString(1).CharacterBox.BasePt
        End Sub
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return

            _AlignmentPt += aTranslation
            CharBox.BasePt += aTranslation
            _Members.Translate(aTranslation)
        End Sub
        Public Sub Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False)

            If aSP = aEP Then Return
            _AlignmentPt.Mirror(New TLINE(aSP, aEP), bSuppressCheck:=True)
            Dim aStr As TSTRING
            For i = 1 To Count
                aStr = SubString(i)
                aStr.Mirror(aSP, aEP, bMirrorDirections, True)
                SetItem(i, aStr)
            Next i
            CharBox.Mirror(aSP, aEP, True, bMirrorDirections, True)
        End Sub
        Public Sub Mirror(aLine As TLINE, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False)
            Mirror(aLine.SPT, aLine.EPT, bMirrorOrigin, bMirrorDirections, bSuppressCheck)
        End Sub
#End Region 'Transformation Methods
    End Structure 'TSTRINGS
#End Region 'Structures


End Namespace
