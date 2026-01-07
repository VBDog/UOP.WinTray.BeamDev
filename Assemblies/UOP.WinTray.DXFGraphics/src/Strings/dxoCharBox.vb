Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoCharBox
        Inherits dxfRectangle
        Implements ICloneable

#Region "Members"


        Public VerticalAlignment As Boolean
#End Region 'Members
#Region "Constructors"

        Public Sub New()
            Init()
        End Sub

        Friend Sub New(aBasePt As TVECTOR, aWidth As Double, aAscent As Double, aDescent As Double)
            Init()
            BasePt.Strukture = aBasePt
            Ascent = aAscent
            Descent = aDescent
            Width = aWidth

        End Sub

        Friend Sub New(aCharBox As TCHARBOX)
            Init()
            BasePtV = aCharBox.BasePt
            MyBase.Define(BasePtV, aCharBox.XDirection, aCharBox.YDirection, True, aCharBox.Height, aCharBox.Width)
            Ascent = aCharBox.Ascent
            Descent = aCharBox.Descent
            ObliqueAngle = aCharBox.ObliqueAngle
            VerticalAlignment = aCharBox.VerticalAlignment

        End Sub

        Friend Sub New(aCharBox As dxoCharBox)
            Init()
            If aCharBox Is Nothing Then Return
            BasePtV = aCharBox.BasePtV
            MyBase.Define(BasePtV, aCharBox.XDirectionV, aCharBox.YDirectionV, True, aCharBox.Height, aCharBox.Width)
            Ascent = aCharBox.Ascent
            Descent = aCharBox.Descent
            ObliqueAngle = aCharBox.ObliqueAngle
            VerticalAlignment = aCharBox.VerticalAlignment


        End Sub

        Private Sub Init()
            _BasePt = dxfVector.Zero
            _ObliqueAngle = 0
            VerticalAlignment = False
            _Ascent = 0
            _Descent = 0
        End Sub
#End Region 'Constructors
#Region "Properties"

        Private _BasePt As dxfVector
        Public Property BasePt As dxfVector
            Get
                Return _BasePt
            End Get
            Set(value As dxfVector)
                If value Is Nothing Then _BasePt = dxfVector.Zero Else _BasePt.Strukture = value.Strukture
            End Set
        End Property

        Friend Property BasePtV As TVECTOR
            Get
                Return _BasePt.Strukture
            End Get
            Set(value As TVECTOR)
                _BasePt.Strukture = value
            End Set
        End Property

        Private _Ascent As Double
        Public Overloads Property Ascent As Double
            Get
                Return _Ascent
            End Get
            Set(value As Double)
                If Double.IsNaN(value) Then value = 0
                _Ascent = value
            End Set
        End Property

        Public Overloads ReadOnly Property Height As Double
            Get
                Return Ascent + Descent
            End Get
        End Property
        Public Overrides Property Center As dxfVector
            Get
                Return BasePt + XDirection * (0.5 * Width) + YDirection * (-_Descent + 0.5 * Height)

            End Get
            Set(value As dxfVector)
                Dim v1 As TVECTOR = BasePtV + XDirectionV * (0.5 * Width) + YDirectionV * (-_Descent + 0.5 * Height)
                Dim v2 As New TVECTOR(0, 0, 0)
                If Not value Is Nothing Then v2 = value.Strukture
                Translate(v2 - v1)
            End Set
        End Property


        Friend Overrides Property CenterV As TVECTOR
            Get
                Return BasePtV + XDirectionV * (0.5 * Width) + YDirectionV * (-_Descent + 0.5 * Height)
            End Get
            Set(value As TVECTOR)
                Dim v1 As TVECTOR = BasePtV + XDirectionV * (0.5 * Width) + YDirectionV * (-_Descent + 0.5 * Height)
                Translate(value - v1)
            End Set
        End Property

        Friend Property AscentCenterV As TVECTOR
            Get
                Return BasePtV + XDirectionV * (0.5 * Width) + YDirectionV * (Ascent * 0.5)
            End Get
            Set(value As TVECTOR)
                Dim v1 As TVECTOR = BasePtV + XDirectionV * (0.5 * Width) + YDirectionV * (Ascent * 0.5)
                Translate(value - v1)
            End Set
        End Property

        Public Function AscentRectangle(Optional aAscent As Double? = Nothing)


            If Not aAscent.HasValue Then
                aAscent = Ascent

            End If
            Dim ctr As TVECTOR = BasePtV + XDirectionV * (0.5 * Width) + YDirectionV * (aAscent.Value * 0.5)

            Return New dxfRectangle(New TPLANE(ctr, XDirectionV, YDirectionV, Width, aAscent.Value))

        End Function

        Private _ObliqueAngle As Double
        Public Property ObliqueAngle As Double
            '^the text's oblique angle
            '~-85 to 85
            Get
                Return _ObliqueAngle
            End Get
            Set(value As Double)
                _ObliqueAngle = TVALUES.ObliqueAngle(value)

            End Set
        End Property

        Public Overrides Property Plane As dxfPlane
            Get

                Return New dxfPlane(PlaneV(True))
            End Get
            Set(value As dxfPlane)
                'ignore this
            End Set

        End Property

        Public Overrides ReadOnly Property BaseLine As dxeLine
            Get
                Return New dxeLine(BaseLineV, aDisplaySettings:=DisplaySettings)
            End Get
        End Property

        Friend Overrides ReadOnly Property BaseLineV As TLINE
            Get
                Return New TLINE(BasePtV, BasePtV + XDirectionV * Width)
            End Get
        End Property


        Public ReadOnly Property MidLine As dxeLine
            Get
                Return New dxeLine(MidLineV, aDisplaySettings:=DisplaySettings)
            End Get
        End Property

        Friend ReadOnly Property MidLineV As TLINE
            Get
                Return New TLINE(BasePtV + YDirectionV * 0.5 * Ascent, BasePtV + YDirectionV * 0.5 * Ascent + XDirectionV * Width)
            End Get
        End Property

        Friend Shadows Function PlaneV(Optional bCenterOnBasePt As Boolean = True) As TPLANE
            Dim _rVal As New TPLANE()
            If Not bCenterOnBasePt Then
                _rVal.Name = "CHAR BOX PLANE"
                _rVal.Define(CenterV, XDirectionV, YDirectionV, aHeight:=0, aWidth:=0)
            Else
                _rVal.Name = "CHAR BOX"
                _rVal.Define(BasePtV, XDirectionV, YDirectionV, aHeight:=Height, aWidth:=Width)
                _rVal.Descent = _Descent
            End If

            _rVal.ShearAngle = 0
            Return _rVal
        End Function

        Friend Overrides ReadOnly Property CornersV As TVECTORS
            '^returns corners of the retangle
            Get
                Dim Cnrs As New TVECTORS(0)
                Cnrs.Add(PointV(dxxRectanglePts.TopLeft, False))
                Cnrs.Add(PointV(dxxRectanglePts.TopRight, False))
                Cnrs.Add(PointV(dxxRectanglePts.BottomRight, False))
                Cnrs.Add(PointV(dxxRectanglePts.BottomLeft, False))
                Return Cnrs
            End Get

        End Property

        Friend Overrides Function VecV(aX As Double, aY As Double, Optional aZ As Double = 0, Optional aShearXAngle As Double = 0, Optional bWorldOrigin As Boolean = False, Optional aOrigin As dxfVector = Nothing, Optional aVertexType As dxxVertexStyles = dxxVertexStyles.UNDEFINED) As TVECTOR
            '#1the X coordinate for the new vector
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '#4an X shear angle to apply
            '#5flag to return the vector with resplect to 0,0,0
            '#6the origin to use if the world origin flag is false (if not passed or Nothing the system origin is used)
            '^used to create a new vector with respect to the origin of the system

            Dim _rVal As TVECTOR = BasePtV
            Dim xFctrT As Double = 0
            If Not bWorldOrigin Then
                If aOrigin IsNot Nothing Then _rVal = aOrigin.Strukture.Clone
            Else
                _rVal = TVECTOR.Zero
            End If

            Dim trans As New TVECTOR(0, 0, 0)

            If aY <> 0 Then trans += YDirectionV * aY
            If aShearXAngle <> 0 And aY <> 0 Then

                xFctrT = Math.Abs(aY) * Math.Tan(aShearXAngle * Math.PI / 180)
                If aY < 0 Then
                    xFctrT *= -1
                End If


            End If
            Dim dX As Double = aX + xFctrT

            If dX <> 0 Then trans += XDirectionV * dX
            If aZ <> 0 Then trans += ZDirectionV * aZ
            _rVal += trans
            _rVal.Code = TVALUES.ToByte(aVertexType)

            Return _rVal


        End Function

        Public Overrides Function Corners(Optional bIncludeBaseLinePts As Boolean = False, Optional bSuppressTags As Boolean = True) As colDXFVectors
            '^returns corners of the retangle
            '~if the rectangle has a descent defined and the baseline is requested, the baseline pts are included
            Dim _rVal As New colDXFVectors
            If Not bIncludeBaseLinePts Or Descent <= 0 Then
                If Not bSuppressTags Then
                    _rVal.Add(Point(dxxRectanglePts.TopLeft), aTag:="Top Left")
                    _rVal.Add(Point(dxxRectanglePts.BottomLeft), aTag:="Bottom Left")
                    _rVal.Add(Point(dxxRectanglePts.BottomRight), aTag:="Bottom Right")
                    _rVal.Add(Point(dxxRectanglePts.TopRight), aTag:="Top Right")
                Else
                    _rVal.Add(Point(dxxRectanglePts.TopLeft))
                    _rVal.Add(Point(dxxRectanglePts.BottomLeft))
                    _rVal.Add(Point(dxxRectanglePts.BottomRight))
                    _rVal.Add(Point(dxxRectanglePts.TopRight))
                End If
            Else
                If Not bSuppressTags Then
                    _rVal.Add(Point(dxxRectanglePts.TopLeft), aTag:="Top Left")
                    _rVal.Add(Point(dxxRectanglePts.TopRight), aTag:="Top Right")
                    _rVal.Add(Point(dxxRectanglePts.BaselineRight), aTag:="Baseline Right")
                    _rVal.Add(Point(dxxRectanglePts.BaselineLeft), aTag:="Baseline Left")
                    _rVal.Add(Point(dxxRectanglePts.BottomLeft), aTag:="Bottom Left")
                    _rVal.Add(Point(dxxRectanglePts.BottomRight), aTag:="Bottom Right")
                    _rVal.Add(Point(dxxRectanglePts.BaselineRight), aTag:="Baseline Right")
                    _rVal.Add(Point(dxxRectanglePts.BaselineLeft), aTag:="Baseline Left")
                Else
                    _rVal.Add(Point(dxxRectanglePts.TopLeft))
                    _rVal.Add(Point(dxxRectanglePts.TopRight))
                    _rVal.Add(Point(dxxRectanglePts.BaselineRight))
                    _rVal.Add(Point(dxxRectanglePts.BaselineLeft))
                    _rVal.Add(Point(dxxRectanglePts.BottomLeft))
                    _rVal.Add(Point(dxxRectanglePts.BottomRight))
                    _rVal.Add(Point(dxxRectanglePts.BaselineRight))
                    _rVal.Add(Point(dxxRectanglePts.BaselineLeft))
                End If
            End If
            Return _rVal
        End Function


        Public Function ToRectangle() As dxfRectangle
            Return New dxfRectangle(Center, Width, Height, 0, Tag, Flag) With {.Descent = Descent}
        End Function

        Private _Descent As Double
        Public Overloads Property Descent As Double
            Get
                Return _Descent
            End Get
            Set(value As Double)
                If Double.IsNaN(value) Then value = 0

                _Descent = value
            End Set
        End Property
        Public Overrides Function Point(aAlignment As dxxRectanglePts, Optional bSuppressShear As Boolean = False, Optional aXOffset As Double = 0, Optional aYOffset As Double = 0) As dxfVector

            Return New dxfVector(PointV(aAlignment, bSuppressShear, aXOffset:=aXOffset, aYOffset:=aYOffset))
        End Function

        Friend Overrides Function PointV(aAlignment As dxxRectanglePts, Optional bSuppressShear As Boolean = False, Optional bAllowForDescent As Boolean = False, Optional aCode As Byte = 0, Optional aXOffset As Double = 0, Optional aYOffset As Double = 0) As TVECTOR
            Dim _rVal As TVECTOR
            Dim dX As Double
            Dim dY As Double
            Dim xFctrT As Double = 0
            Dim xFctrB As Double = 0
            Dim wd As Double = Width
            If ObliqueAngle <> 0 And Not bSuppressShear Then
                xFctrT = Ascent * Math.Tan(ObliqueAngle * Math.PI / 180)
                xFctrB = Descent * -Math.Tan(ObliqueAngle * Math.PI / 180)
            End If
            Select Case aAlignment
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
                    dY = -Descent
                Case dxxRectanglePts.BottomLeft
                    dX = 0 + xFctrB
                    dY = -Descent
                Case dxxRectanglePts.BottomRight
                    dX = wd + xFctrB
                    dY = -Descent
                Case dxxRectanglePts.MiddleCenter
                    dX = 0.5 * wd
                    dY = -Descent + 0.5 * Height
                    If ObliqueAngle <> 0 And Not bSuppressShear Then
                        If dY > 0 Then
                            dX += dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        Else
                            dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        End If
                    End If
                Case dxxRectanglePts.MiddleLeft
                    dX = 0
                    dY = -Descent + 0.5 * Height
                    If ObliqueAngle <> 0 And Not bSuppressShear Then
                        If dY > 0 Then
                            dX += dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        Else
                            dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        End If
                    End If
                Case dxxRectanglePts.MiddleRight
                    dX = wd
                    dY = -Descent + 0.5 * Height
                    If ObliqueAngle <> 0 And Not bSuppressShear Then
                        If dY > 0 Then
                            dX += dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        Else
                            dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                        End If
                    End If
                Case dxxRectanglePts.TopCenter
                    dX = 0.5 * wd + xFctrT
                    dY = Ascent
                Case dxxRectanglePts.TopLeft
                    dX = 0 + xFctrT
                    dY = Ascent
                Case dxxRectanglePts.TopRight
                    dX = wd + xFctrT
                    dY = Ascent
                Case dxxRectanglePts.UnderlineLeft
                    dX = 0
                    dY = -0.2 * Descent
                    If ObliqueAngle <> 0 And Not bSuppressShear Then
                        dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case dxxRectanglePts.UnderlineRight
                    dX = wd
                    dY = -0.2 * Descent
                    If ObliqueAngle <> 0 And Not bSuppressShear Then
                        dX -= dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case dxxRectanglePts.OverlineLeft
                    dX = 0
                    dY = Ascent + 0.2 * Descent
                    If ObliqueAngle <> 0 And Not bSuppressShear Then
                        dX = dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case dxxRectanglePts.OverlineRight
                    dX = wd
                    dY = Ascent + 0.2 * Descent
                    If ObliqueAngle <> 0 And Not bSuppressShear Then
                        dX = dY * Math.Tan(ObliqueAngle * Math.PI / 180)
                    End If
                Case Else
                    dX = 0
                    dY = 0
            End Select
            _rVal = BasePtV
            dX += aXOffset
            dY += aYOffset
            If dY <> 0 Then _rVal += YDirectionV * dY
            If dX <> 0 Then _rVal += XDirectionV * dX
            _rVal.Code = aCode
            Return _rVal
        End Function
        Public Shadows ReadOnly Property IsDefined As Boolean
            Get
                Return MyBase.DirectionsAreDefined
            End Get
        End Property
        Public ReadOnly Property IsWorld As Boolean
            Get
                If Math.Round(XDirection.X, 6) <> 1 Then Return False
                If Math.Round(YDirection.Y, 6) <> 1 Then Return False
                Return True
            End Get
        End Property

        Public ReadOnly Property StartPt As dxfVector
            Get
                Return New dxfVector(StartPtV)
            End Get
        End Property
        Friend ReadOnly Property StartPtV As TVECTOR
            Get
                If Not VerticalAlignment Then
                    Return BasePtV
                Else
                    Return PointV(dxxRectanglePts.TopCenter, True)
                End If
            End Get
        End Property
        Public ReadOnly Property EndPt As dxfVector
            Get
                Return New dxfVector(EndPtV)
            End Get
        End Property
        Friend ReadOnly Property EndPtV As TVECTOR
            Get
                If Not VerticalAlignment Then
                    Return VectorV(Width, 0, True)

                Else
                    Return PointV(dxxRectanglePts.BottomCenter, True)
                End If
            End Get
        End Property


#End Region 'Properties
#Region "Methods"
        Friend Overrides Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional bSuppressEvnts As Boolean = False, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim rOriginChange As Boolean = False
            Dim rDirectionChange As Boolean = False
            Dim rDimChange As Boolean = False
            Return Define(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimChange, bSuppressEvnts, aHeight, aWidth)
        End Function

        Friend Overrides Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, rOriginChange As Boolean, rDirectionChange As Boolean, ByRef rDimensionChange As Boolean, Optional bSuppressEvnts As Boolean = False, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean

            Dim plane As TPLANE = PlaneV(False)
            Dim ht As Double = Height
            Dim wd As Double = Width
            If Not aHeight.HasValue Then aHeight = ht
            If Not aWidth.HasValue Then aWidth = wd

            Dim _rVal As Boolean = plane.Define(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimensionChange, aHeight, aWidth)

            Dim d1 As Double = 0
            Dim dir As TVECTOR = BasePtV.DirectionTo(aOrigin, rDistance:=d1)
            If d1 <> 0 Then BasePtV += dir * d1
            MyBase.SetDirections(plane.XDirection, plane.YDirection)
            Width = plane.Width

            If ht <> plane.Height And plane.Height <> 0 Then
                Dim f1 As Double = ht / plane.Height
                Ascent *= f1
                Descent *= f1
            End If


            Return _rVal
        End Function

        Friend Overloads Function Translate(aTranslation As TVECTOR, Optional aCS As dxfPlane = Nothing, Optional bSuppressEvnts As Boolean = True) As Boolean

            '#1the displacement to apply
            '#2a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Dim baspt As TVECTOR = BasePtV
            Dim _rVal As Boolean = baspt.Translate(aTranslation, aCS)
            If _rVal Then Define(baspt, XDirectionV, YDirectionV, bSuppressEvnts)
            Return _rVal
        End Function
        Public Overloads Sub Stretch(aDist As Double, Optional aStretchWidth As Boolean = True, Optional aStretchHeight As Boolean = True)
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

        Public Overloads Function Clone() As dxoCharBox
            Return New dxoCharBox(New TCHARBOX(BasePtV, Width, Ascent, Descent) With {
                .XDirection = XDirectionV,
                .YDirection = YDirectionV,
                .ObliqueAngle = ObliqueAngle,
                .VerticalAlignment = VerticalAlignment
                                  })
        End Function
        Friend Function VectorV(aX As Double, aY As Double, Optional bSuppressOblique As Boolean = False) As TVECTOR
            Dim _rVal As TVECTOR = BasePtV
            Dim xFctrT As Double = 0
            Dim YDir As TVECTOR = YDirectionV
            If ObliqueAngle <> 0 And Not bSuppressOblique And aY <> 0 Then
                xFctrT = Math.Abs(aY) * Math.Tan(ObliqueAngle * Math.PI / 180)
                If aY < 0 Then xFctrT *= -1
            End If
            Dim dX As Double = aX + xFctrT
            If dX <> 0 Then _rVal += XDirectionV * dX
            If aY <> 0 Then _rVal += YDirectionV * aY
            Return _rVal
        End Function


        Friend Overrides Function EdgeV(aEdge As dxxRectangleLines, Optional bSuppressOblique As Boolean = True, Optional aLengthAdder As Double = 0.0) As TLINE
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
            v1 = PointV(P1, bSuppressOblique)
            v2 = PointV(P2, bSuppressOblique)
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
        Public Shadows Sub Rescale(aScaleFactor As Single, aRefPt As dxfVector)
            Ascent *= aScaleFactor
            Descent *= aScaleFactor
            Width *= aScaleFactor

            BasePt.Rescale(aScaleFactor, aRefPt)

        End Sub
        Friend Overloads Function Define2(aBasePt As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional aAscent As Double? = Nothing, Optional aDescent As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim rOriginChange As Boolean = False
            Dim rOrientationChange As Boolean = False
            Dim rDimensionChange As Boolean = False
            Return Define2(aBasePt, aXDir, aYDir, rOriginChange, rOrientationChange, rDimensionChange, aAscent, aDescent, aWidth)
        End Function
        Friend Overloads Function Define2(aBasePt As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, ByRef rOriginChange As Boolean, ByRef rOrientationChange As Boolean, ByRef rDimensionChange As Boolean, Optional aAscent As Double? = Nothing, Optional aDescent As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            rDimensionChange = False
            rOrientationChange = False
            rOriginChange = Not BasePtV.Equals(aBasePt, 6)
            BasePtV = aBasePt
            If TVECTOR.IsNull(aXDir) Then aXDir = XDirectionV
            If TVECTOR.IsNull(aYDir) Then aYDir = YDirectionV
            Dim zDir As TVECTOR = ZDirectionV
            Dim xDir As TVECTOR
            Dim yDir As TVECTOR
            Dim xErr As Boolean
            Dim yErr As Boolean
            Dim wd As Double = Width
            Dim mAscent As Double = Ascent
            Dim mDescent As Double = Descent
            If aWidth.HasValue Then wd = aWidth.Value
            If wd <> Width Then rDimensionChange = True
            If aAscent.HasValue Then mAscent = aAscent.Value
            If mAscent <> Ascent Then rDimensionChange = True
            If aDescent.HasValue Then mDescent = aDescent.Value
            If mDescent <> _Descent Then rDimensionChange = True
            xDir = TVECTOR.ToDirection(aXDir, xErr)
            yDir = TVECTOR.ToDirection(aYDir, yErr)
            If xErr And yErr Then
                xDir = XDirectionV
                yDir = YDirectionV
                zDir = ZDirectionV
            Else
                If xErr And Not yErr Then
                    zDir = ZDirectionV
                    xDir = yDir.CrossProduct(zDir, True)
                    xErr = False
                ElseIf yErr And Not xErr Then
                    zDir = ZDirectionV
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
            If Not XDirectionV.Equals(xDir, 6) Then rOrientationChange = True
            If Not YDirectionV.Equals(yDir, 6) Then rOrientationChange = True
            Ascent = mAscent
            Descent = mDescent
            Width = wd
            BasePtV = aBasePt
            MyBase.Define(BasePtV, xDir, yDir, True, Height, Width)

            Return rOriginChange Or rOrientationChange Or rDimensionChange
        End Function

        Friend Function Reset() As Boolean
            Return Define(TVECTOR.Zero, New TVECTOR(1, 0, 0), New TVECTOR(0, 1, 0))
        End Function
        Friend Function CopyDirections(aPlane As TPLANE) As Boolean
            If TPLANE.IsNull(aPlane) Then Return False
            Return Define(BasePtV, aPlane.XDirection, aPlane.YDirection)
        End Function
        Friend Function CopyDirections(aCharBox As TCHARBOX, Optional bMoveTo As Boolean = False, Optional bResetOnNull As Boolean = False) As Boolean

            If TPLANE.IsNull(aCharBox) Then
                If bResetOnNull Then
                    Return Reset()
                End If
                Return False
            End If
            Dim v1 As TVECTOR = BasePtV
            If bMoveTo Then v1 = aCharBox.BasePt
            Return Define(v1, aCharBox.XDirection, aCharBox.YDirection)
        End Function

        Public Function CopyDirections(aCharBox As dxoCharBox, Optional bMoveTo As Boolean = False, Optional bResetOnNull As Boolean = False) As Boolean

            If TPLANE.IsNull(aCharBox) Then
                If bResetOnNull Then
                    Return Reset()
                End If
                Return False
            End If
            Dim v1 As TVECTOR = BasePtV
            If bMoveTo Then v1 = aCharBox.BasePtV
            Return Define(v1, aCharBox.XDirectionV, aCharBox.YDirectionV)
        End Function
        Friend Overloads Function Mirror(aSP As TVECTOR, aEP As TVECTOR, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressCheck As Boolean = False) As Boolean

            Dim _rVal As Boolean
            '#2the start pt of the line to mirror across
            '#3the end pt of the line to mirror across
            '#4flag to mirror the origin
            '#5flag to mirror the directions
            '^mirrors the system across the passed line
            '~returns True if the system actually moves from this process
            If Not bMirrorOrigin And Not bMirrorOrigin Then Return False
            Dim mline As New TLINE(aSP, aEP)
            If Not bSuppressCheck Then
                If mline.Length < 0.00001 Then Return False
            End If
            Dim v0 As TVECTOR = TVECTOR.Zero
            If bMirrorOrigin Then
                If BasePt.Mirror(mline) Then _rVal = True
            End If
            If bMirrorDirections Then
                MyBase.Mirror(mline, False, True, True)
            End If
            Return _rVal
        End Function
        Public Function Revolve(aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            If aAngle = 0 Then Return False
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '^used to change the orientation and/or the origin of the system by rotating the it about its z axis
            Dim newx As TVECTOR = XDirectionV
            Dim newy As TVECTOR = YDirectionV
            Dim myz As TVECTOR = ZDirectionV
            newx.RotateAbout(myz, aAngle, bInRadians, bSuppressNorm:=True)
            newy.RotateAbout(myz, aAngle, bInRadians, bSuppressNorm:=True)

            Return Define(BasePtV, newx, newy)
        End Function
        Friend Overloads Function RotateAbout(aOrigin As TVECTOR, aAxis As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateOrigin As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressNorm As Boolean = False) As Boolean


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
            Dim xDir As TVECTOR = XDirectionV
            Dim yDir As TVECTOR = YDirectionV
            Dim v0 As TVECTOR = BasePtV
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

        Friend Overrides Function RectanglePts(bIncludeBaseline As Boolean, Optional bSuppressShear As Boolean = True) As TVECTORS
            Dim _rVal As New TVECTORS
            If Descent = 0 Then bIncludeBaseline = False
            _rVal = New TVECTORS
            If Width > 0 Or Height > 0 Then
                If Not bIncludeBaseline Then
                    _rVal.Add(PointV(dxxRectanglePts.TopLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    _rVal.Add(PointV(dxxRectanglePts.BottomLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.BottomRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.TopRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.TopLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                Else
                    _rVal.Add(PointV(dxxRectanglePts.BaselineLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    _rVal.Add(PointV(dxxRectanglePts.BaselineRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.TopRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.TopLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.BottomLeft, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.BottomRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    _rVal.Add(PointV(dxxRectanglePts.BaselineRight, bSuppressShear), TVALUES.ToByte(dxxVertexStyles.LINETO))
                End If
            End If
            Return _rVal
        End Function

        Friend Overrides Function LineV(aXOrdinate As Double, Optional aLength As Double = 0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Return LineV(VecV(aXOrdinate, 0, 0), aLength, aInvert, bByStartPt)
        End Function
        Friend Overrides Function LineV(aVector As TVECTOR, Optional aLength As Double = 0.0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Dim l1 As Double = aLength
            If l1 = 0 Then l1 = Height
            If l1 = 0 Then l1 = 1
            Dim f1 As Integer = 1
            If l1 < 0 Then f1 = -1
            l1 = Math.Abs(l1)
            Dim v1 As New TVECTOR(aVector)


            If Not bByStartPt Then
                Return New TLINE(v1 + (YDirectionV * (-f1 * 0.5 * l1)), v1 + (YDirectionV * (f1 * 0.5 * l1)))
            Else
                Return New TLINE(v1, v1 + (YDirectionV * (f1 * l1)))
            End If
        End Function

        Friend Overrides Function LineH(aYOrdinate As Double, Optional aLength As Double = 0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Return LineH(VecV(0, aYOrdinate, 0), aLength, aInvert, bByStartPt)
        End Function
        Friend Overrides Function LineH(aVector As TVECTOR, Optional aLength As Double = 0.0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Dim l1 As Double = aLength
            If l1 = 0 Then l1 = Height
            If l1 = 0 Then l1 = 1
            Dim f1 As Integer = 1
            If l1 < 0 Then f1 = -1
            l1 = Math.Abs(l1)
            Dim v1 As New TVECTOR(aVector)
            If Not bByStartPt Then
                Return New TLINE(v1 + (XDirectionV * (-f1 * 0.5 * l1)), v1 + (XDirectionV * (f1 * 0.5 * l1)))
            Else
                Return New TLINE(v1, v1 + (XDirectionV * (f1 * l1)))
            End If
        End Function

        Public Overrides Function ToString() As String
            Dim _rVal As String = "TextBox [" & BasePt.ToString & "] W:" & Format(Width, "0.0#") & " A:" & Format(_Ascent, "0.0#") & "D:" & Format(_Descent, "0.0#")
            If XDirection.X <> 1 Then _rVal += "X:" & XDirection.ToString
            Return _rVal
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#End Region 'Methods
    End Class
End Namespace
