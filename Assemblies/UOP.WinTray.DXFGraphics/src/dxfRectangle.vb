
Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.dxfGlobals

Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxfRectangle
        Implements iVector
        Implements iShape
        Implements iRectangle
        Implements ICloneable
#Region "Members"
        Private _Struc As TPLANE
#End Region 'Members
#Region "Events"
        Public Event RectangleChange(aType As dxxCoordinateSystemEventTypes)
#End Region 'Events
#Region "Constructors"
        Public Sub New()
            Init()
        End Sub
        Public Sub New(aWidth As Double, aHeight As Double, Optional aRotation As Double = 0)

            Init(aWidth, aHeight, aRotation)
        End Sub
        Public Sub New(aCenter As iVector, aWidth As Double, aHeight As Double, Optional aRotation As Double = 0, Optional aTag As String = Nothing, Optional aFlag As String = Nothing)
            Init(aWidth, aHeight, aRotation)
            _Struc.Origin = New TVECTOR(aCenter)


            If Not String.IsNullOrWhiteSpace(aTag) Then Tag = aTag
            If Not String.IsNullOrWhiteSpace(aFlag) Then Flag = aFlag
        End Sub
        Friend Sub New(aStructure As TPLANE)
            _Struc = aStructure
        End Sub
        Public Sub New(aRectangle As iRectangle, Optional aWidth As Double? = Nothing, Optional aHeight As Double? = Nothing)
            Init()
            If aRectangle IsNot Nothing Then
                _Struc = New TPLANE(aRectangle)
            End If
            
            If aWidth.HasValue Then Width = aWidth.Value
            If aHeight.HasValue Then Height = aHeight.Value
        End Sub


        Public Sub New(aPlane As dxfPlane, Optional aWidth As Double? = Nothing, Optional aHeight As Double? = Nothing)
            Init()
            If aPlane IsNot Nothing Then
                _Struc = New TPLANE(aPlane)
            End If
            If aWidth.HasValue Then Width = aWidth.Value
            If aHeight.HasValue Then Height = aHeight.Value

        End Sub

        Public Sub New(DefVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing, Optional bSuppressProjection As Boolean = False)
            Init()
            If DefVectors Is Nothing Then Return
            DefineByVectors(DefVectors, aPlane, bSuppressProjection)

        End Sub

        Private Sub Init(Optional aWidth As Double = 0, Optional aHeight As Double = 0, Optional aRotation As Double = 0)
            _Struc = New TPLANE("") With {.DisplayProps = New TDISPLAYVARS("0"), .Height = Math.Abs(aHeight), .Width = Math.Abs(aWidth)}
            If aRotation <> 0 Then _Struc.Revolve(aRotation)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Overridable ReadOnly Property Ascent As Double
            Get
                Return _Struc.Height - _Struc.Descent
            End Get
        End Property
        Public ReadOnly Property Area As Double
            Get
                Return Width * Height
            End Get
        End Property
        Public ReadOnly Property AspectRatio As Double
            Get
                '^the ratio of the width to the height (w/h)
                Return _Struc.AspectRatio(False)
            End Get
        End Property
        Public Overridable ReadOnly Property BaseLine As dxeLine
            Get
                Return New dxeLine(BaseLineV, aDisplaySettings:=New dxfDisplaySettings(_Struc.DisplayProps))
            End Get
        End Property

        Friend Overridable ReadOnly Property BaseLineV As TLINE
            Get
                Return EdgeV(dxxRectangleLines.Baseline)
            End Get
        End Property

        Public ReadOnly Property BaselineCenter As dxfVector
            Get
                Return New dxfVector(BaselineCenterV)
            End Get
        End Property
        Friend ReadOnly Property BaselineCenterV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.BaselineCenter, True)
            End Get
        End Property
        Public ReadOnly Property BaselineLeft As dxfVector
            Get
                Return New dxfVector(BaselineLeftV)
            End Get
        End Property
        Friend ReadOnly Property BaselineLeftV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.BaselineLeft, True)
            End Get
        End Property
        Public ReadOnly Property BaselineRight As dxfVector
            Get
                Return New dxfVector(BaselineRightV)
            End Get
        End Property
        Friend ReadOnly Property BaselineRightV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.BaselineRight, True)
            End Get
        End Property
        Public ReadOnly Property Bottom As Double Implements iRectangle.Bottom
            '^the Y ordinate of the bottom of the rectangle
            Get
                Return Y - 0.5 * Height
            End Get
        End Property
        Public ReadOnly Property BottomEdge As dxeLine
            '^the edge from the bottom left to the bottom right corner
            Get
                Return New dxeLine(BottomEdgeV(), aDisplaySettings:=DisplaySettings)
            End Get
        End Property
        Public ReadOnly Property BottomCenter As dxfVector
            Get
                Return New dxfVector(BottomCenterV)
            End Get
        End Property
        Friend ReadOnly Property BottomCenterV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.BottomCenter, True)
            End Get
        End Property
        Friend ReadOnly Property BottomEdgeV As TLINE
            Get
                '^the edge from the bottom left to the bottom right corner
                Return EdgeV(dxxRectangleLines.BottomEdge, True)
            End Get
        End Property
        Public ReadOnly Property BottomLeft As dxfVector
            Get
                Return New dxfVector(BottomLeftV)
            End Get
        End Property
        Friend ReadOnly Property BottomLeftV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.BottomLeft, True)
            End Get
        End Property
        Public ReadOnly Property BottomRight As dxfVector
            Get
                Return New dxfVector(BottomRightV)
            End Get
        End Property
        Friend ReadOnly Property BottomRightV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.BottomRight, True)
            End Get
        End Property
        Public Overridable Property Center As dxfVector
            '^returns a new vector located at the current center
            '~applying translations etc. to this point does not affect the rectangles center.
            '~the only way to move the rectangle is through it's Move, Project, Translate etc. functions or by using .Center = {new vector}.
            Get
                Return New dxfVector(_Struc.Origin) With {.Tag = Tag, .Flag = Flag, .Value = Value}
            End Get
            Set(value As dxfVector)
                CenterV = New TVECTOR(value)
            End Set
        End Property
        Friend Overridable Property CenterV As TVECTOR
            Get
                Return _Struc.Origin
            End Get
            Set(value As TVECTOR)
                Define(value, _Struc.XDirection, _Struc.YDirection)
            End Set
        End Property
        Public Property Color As dxxColors
            '^the color of the entity
            Get
                Return _Struc.DisplayProps.Color
            End Get
            Set(value As dxxColors)
                _Struc.DisplayProps.Color = value
            End Set
        End Property
        Public Property Suppressed As Boolean
            '^a flag indicating the suppression state of the rectangle
            Get
                Return _Struc.DisplayProps.Suppressed
            End Get
            Set(value As Boolean)
                _Struc.DisplayProps.Suppressed = value
            End Set
        End Property
        Public Overridable Function Corners(Optional bIncludeBaseLinePts As Boolean = False, Optional bSuppressTags As Boolean = True) As colDXFVectors
            '^returns corners of the retangle
            '~if the rectangle has a descent defined and the baseline is requested, the baseline pts are included
            Dim _rVal As New colDXFVectors
            If Not bIncludeBaseLinePts Or Descent <= 0 Then
                If Not bSuppressTags Then
                    _rVal.Add(TopLeft, aTag:="Top Left")
                    _rVal.Add(BottomLeft, aTag:="Bottom Left")
                    _rVal.Add(BottomRight, aTag:="Bottom Right")
                    _rVal.Add(TopRight, aTag:="Top Right")
                Else
                    _rVal.Add(TopLeft)
                    _rVal.Add(BottomLeft)
                    _rVal.Add(BottomRight)
                    _rVal.Add(TopRight)
                End If
            Else
                If Not bSuppressTags Then
                    _rVal.Add(TopLeft, aTag:="Top Left")
                    _rVal.Add(TopRight, aTag:="Top Right")
                    _rVal.Add(BaselineRight, aTag:="Baseline Right")
                    _rVal.Add(BaselineLeft, aTag:="Baseline Left")
                    _rVal.Add(BottomLeft, aTag:="Bottom Left")
                    _rVal.Add(BottomRight, aTag:="Bottom Right")
                    _rVal.Add(BaselineRight, aTag:="Baseline Right")
                    _rVal.Add(BaselineLeft, aTag:="Baseline Left")
                Else
                    _rVal.Add(TopLeft)
                    _rVal.Add(TopRight)
                    _rVal.Add(BaselineRight)
                    _rVal.Add(BaselineLeft)
                    _rVal.Add(BottomLeft)
                    _rVal.Add(BottomRight)
                    _rVal.Add(BaselineRight)
                    _rVal.Add(BaselineLeft)
                End If
            End If
            Return _rVal
        End Function
        Friend Overridable ReadOnly Property CornersV As TVECTORS
            Get
                Return _Struc.Corners(True)
            End Get
        End Property
        Public Overridable Property Descent As Double
            '^the distance above the bottom that is used to define the position of the rectangles baseline
            '~used for text definitions
            Get
                Return _Struc.Descent
            End Get
            Set(value As Double)
                _Struc.Descent = Math.Abs(value)
            End Set
        End Property

        Friend Property ShearAngle As Double
            Get
                Return _Struc.ShearAngle
            End Get
            Set(value As Double)
                _Struc.ShearAngle = value
            End Set
        End Property

        Public Function Descriptor(Optional bIncludeAspect As Boolean = False, Optional bIncludeName As Boolean = True, Optional aPrecis As Integer = 3) As String
            Return _Struc.Descriptor(bIncludeAspect, bIncludeName, aPrecis)
        End Function
        Public ReadOnly Property Diagonal As Double
            '^the length of the diagonal of the rectangle
            Get
                If _Struc.Width <> 0 Or _Struc.Height <> 0 Then Return Math.Sqrt(_Struc.Width ^ 2 + _Struc.Height ^ 2) Else Return 0
            End Get
        End Property
        Public Function Diagonal1(Optional aLengthAdder As Double = 0) As dxeLine
            Return New dxeLine(EdgeV(dxxRectangleLines.Diagonal1, True, aLengthAdder), aDisplaySettings:=DisplaySettings)
        End Function
        Public Function Diagonal2(Optional aLengthAdder As Double = 0) As dxeLine
            Return New dxeLine(EdgeV(dxxRectangleLines.Diagonal2, True, aLengthAdder), aDisplaySettings:=DisplaySettings)
        End Function
        Public Property DisplaySettings As dxfDisplaySettings
            '^the object which carries display style information for an entity
            Get
                Return New dxfDisplaySettings(_Struc.DisplayProps)
            End Get
            Set(value As dxfDisplaySettings)
                If value IsNot Nothing Then _Struc.DisplayProps = value.Strukture
            End Set
        End Property
        Friend Property DisplayStructure As TDISPLAYVARS
            '^the structure of the object which carries display style information for an entity
            Get
                Return _Struc.DisplayProps
            End Get
            Set(value As TDISPLAYVARS)
                _Struc.DisplayProps = value
            End Set
        End Property
        Public Property Flag As String Implements iVector.Flag
            '^a user asignable string for the rectangle
            Get
                Return _Struc.Flag
            End Get
            Set(value As String)
                _Struc.Flag = value
            End Set
        End Property
        Public Function GripPoint(aAlignm As dxxRectangularAlignments) As dxfVector
            Return New dxfVector(GripPointV(aAlignm))
        End Function
        Friend Function GripPointV(aAlignm As dxxRectangularAlignments) As TVECTOR
            Return _Struc.GripPoint(aAlignm)
        End Function
        Public Property Handle As String
            '^a user asignable string for the rectangle
            Get
                Return _Struc.Handle
            End Get
            Set(value As String)
                _Struc.Handle = value
            End Set
        End Property
        Public Overridable Property Height As Double Implements iRectangle.Height
            Get
                Return _Struc.Height
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                If _Struc.Height <> value Then
                    _Struc.Height = value
                    RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Dimensions)
                End If
            End Set
        End Property
        Public Property Index As Integer
            Get
                Return _Struc.Index
            End Get
            Set(value As Integer)
                _Struc.Index = value
            End Set
        End Property
        Public Function IsDefined(Optional bBothDims As Boolean = True) As Boolean
            If bBothDims Then
                Return (Height > 0 And Width > 0) And _Struc.DirectionsAreDefined
            Else
                Return (Height > 0 Or Width > 0) And _Struc.DirectionsAreDefined
            End If
        End Function

        Public ReadOnly Property DirectionsAreDefined As Boolean
            Get

                Return _Struc.DirectionsAreDefined
            End Get
        End Property

        Public Property IsDirty As Boolean
            Get
                Return _Struc.IsDirty
            End Get
            Set(value As Boolean)
                _Struc.IsDirty = value
            End Set
        End Property
        Public Property LayerName As String
            '^the layer name associated to the entity
            '~this layer is used for color and linetype info for ByLayer values and the visiblity of the enity in an image
            Get
                Return _Struc.DisplayProps.LayerName
            End Get
            Set(value As String)
                _Struc.DisplayProps.LayerName = value
            End Set
        End Property
        Public ReadOnly Property Left As Double Implements iRectangle.Left
            '^the X ordinate of the left side of the rectangle
            Get
                Return X - 0.5 * Width
            End Get
        End Property
        Public ReadOnly Property LeftEdge As dxeLine
            '^the edge from the top left to the bottm left corner
            Get
                Return New dxeLine(LeftEdgeV, aDisplaySettings:=DisplaySettings)
            End Get
        End Property
        Friend ReadOnly Property LeftEdgeV As TLINE
            '^the edge from the top left to the bottm left corner
            Get
                Return EdgeV(dxxRectangleLines.LeftEdge, True)
            End Get
        End Property
        Public Property Linetype As String
            '^the linetype name assigned to the entity
            Get
                Return _Struc.DisplayProps.Linetype
            End Get
            Set(value As String)
                _Struc.DisplayProps.Linetype = value
            End Set
        End Property
        Public Property LTScale As Double
            '^the linetype scale factor of the entity
            '~affects the dispaly of non-continuous lines
            Get
                Return _Struc.DisplayProps.LTScale
            End Get
            Set(value As Double)
                If value > 0 Then _Struc.DisplayProps.LTScale = value
            End Set
        End Property
        Public Function MiddleCenter(Optional bAllowForDescent As Boolean = False) As dxfVector
            Return New dxfVector(MiddleCenterV(bAllowForDescent))
        End Function
        Friend Function MiddleCenterV(Optional bAllowForDescent As Boolean = False) As TVECTOR
            Return PointV(dxxRectanglePts.MiddleCenter, True, bAllowForDescent)
        End Function
        Public Function MiddleLeft(Optional bAllowForDescent As Boolean = False) As dxfVector
            Return New dxfVector(MiddleLeftV(bAllowForDescent))
        End Function
        Friend Function MiddleLeftV(Optional bAllowForDescent As Boolean = False) As TVECTOR
            Return PointV(dxxRectanglePts.MiddleLeft, True, bAllowForDescent)
        End Function
        Public Function MiddleRight(Optional bAllowForDescent As Boolean = False) As dxfVector
            Return New dxfVector(MiddleRightV(bAllowForDescent))
        End Function
        Friend Function MiddleRightV(Optional bAllowForDescent As Boolean = False) As TVECTOR
            Return PointV(dxxRectanglePts.MiddleRight, True, bAllowForDescent)
        End Function
        Public Property Name As String
            '^a user assigned name for the rectangle
            Get
                Return _Struc.Name
            End Get
            Set(value As String)
                _Struc.Name = value
            End Set
        End Property
        Public ReadOnly Property Normal As dxfDirection
            Get
                Return ZDirection
            End Get
        End Property
        Public ReadOnly Property PixelHeight As Double
            '^a height in pixels based on the current units
            Get
                Return _Struc.Height * ToPixels
            End Get
        End Property
        Public ReadOnly Property PixelWidth As Double
            '^a width in pixels based on the current units
            Get
                Return _Struc.Width * ToPixels
            End Get
        End Property
        Public Overridable Property Plane As dxfPlane Implements iShape.Plane
            Get
                Return New dxfPlane(_Struc)
            End Get
            Set(value As dxfPlane)
                If value IsNot Nothing Then
                    Define(value.OriginV, value.XDirectionV, value.YDirectionV)
                Else
                    Define(TVECTOR.Zero, TVECTOR.WorldX, TVECTOR.WorldY)
                End If
            End Set
        End Property
        Public ReadOnly Property Right As Double Implements iRectangle.Right
            '^the X ordinate of the left side of the rectangle
            Get
                Return X + 0.5 * Width
            End Get
        End Property
        Public ReadOnly Property RightEdge As dxeLine
            Get
                '^the edge from the bottom right to the top right corner
                Return New dxeLine(RightEdgeV, aDisplaySettings:=DisplaySettings)
            End Get
        End Property
        Friend ReadOnly Property RightEdgeV As TLINE
            Get
                '^the edge from the bottom right to the top right corner
                Return EdgeV(dxxRectangleLines.RightEdge, True)
            End Get
        End Property
        Friend Property Strukture As TPLANE
            Get
                Return _Struc
            End Get
            Set(value As TPLANE)
                If TPLANE.IsNull(value) Then Return
                _Struc = value
            End Set
        End Property
        Public Property Tag As String Implements iVector.Tag
            '^a user asignable string for the rectangle
            Get
                Return _Struc.Tag
            End Get
            Set(value As String)
                _Struc.Tag = value
            End Set
        End Property
        Public ReadOnly Property Top As Double Implements iRectangle.Top
            '^the Y ordinate of the top of the rectangle
            Get
                Return Y + 0.5 * Height
            End Get
        End Property
        Public ReadOnly Property TopEdge As dxeLine
            '^the edge from the top left to the top right corner
            Get
                Return New dxeLine(TopEdgeV, aDisplaySettings:=DisplaySettings)
            End Get
        End Property
        Public ReadOnly Property TopCenter As dxfVector
            Get
                Return New dxfVector(TopCenterV)
            End Get
        End Property
        Friend ReadOnly Property TopCenterV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.TopCenter, True)
            End Get
        End Property
        Friend ReadOnly Property TopEdgeV As TLINE
            Get
                '^the edge from the top left to the top right corner
                Return EdgeV(dxxRectangleLines.TopEdge, True)
            End Get
        End Property
        Public ReadOnly Property TopLeft As dxfVector
            Get
                Return New dxfVector(TopLeftV())
            End Get
        End Property
        Friend ReadOnly Property TopLeftV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.TopLeft, True)
            End Get
        End Property
        Public ReadOnly Property TopRight As dxfVector
            Get
                Return New dxfVector(TopRightV)
            End Get
        End Property
        Friend ReadOnly Property TopRightV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.TopRight, True)
            End Get
        End Property
        Public ReadOnly Property ToPixels As Double
            Get
                Return dxfUtils.FactorToPixels(_Struc.Units)
            End Get
        End Property
        Public ReadOnly Property UnitName As String
            Get
                Return dxfEnums.Description(_Struc.Units)
            End Get
        End Property
        Friend Property Units As dxxDeviceUnits
            Get
                Return _Struc.Units
            End Get
            Set(value As dxxDeviceUnits)
                If value < 1 Or value > 7 Then Return
                If _Struc.Units <> value Then
                    Dim uscale As Double
                    Dim cp As TVECTOR
                    cp = _Struc.Origin
                    uscale = ToPixels
                    _Struc.Width *= uscale
                    _Struc.Height *= uscale
                    _Struc.Descent *= uscale
                    cp *= uscale
                    _Struc.Units = value
                    uscale = ToPixels
                    _Struc.Width /= uscale
                    _Struc.Height /= uscale
                    _Struc.Descent /= uscale
                    cp *= uscale
                    _Struc = _Struc.MovedTo(cp)
                    RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Units)
                End If
            End Set
        End Property
        Public ReadOnly Property UnderlineLeft As dxfVector
            Get
                Return New dxfVector(UnderlineLeftV)
            End Get
        End Property
        Friend ReadOnly Property UnderlineLeftV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.UnderlineLeft, True)
            End Get
        End Property
        Public ReadOnly Property UnderlineRight As dxfVector
            Get
                Return New dxfVector(UnderlineRightV)
            End Get
        End Property
        Friend ReadOnly Property UnderlineRightV As TVECTOR
            Get
                Return PointV(dxxRectanglePts.UnderlineRight, True)
            End Get
        End Property
        Public Property Value As Double
            '^a user asignable double for the rectangle
            Get
                Return _Struc.Value
            End Get
            Set(value As Double)
                _Struc.Value = value
            End Set
        End Property

        Friend ReadOnly Property VerticalCenterLineV As TLINE
            '^a line running vertically through the center of the rectangle
            Get
                Return New TLINE(TopCenterV, BottomCenterV)
            End Get
        End Property
        Public Property Width As Double Implements iRectangle.Width
            Get
                Return _Struc.Width
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                If _Struc.Width <> value Then
                    _Struc.Width = value
                    RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Dimensions)
                End If
            End Set
        End Property
        Public Property X As Double Implements iVector.X
            Get
                Return _Struc.Origin.X
            End Get
            Set(value As Double)
                SetCoordinates(value)
            End Set
        End Property
        Public Overridable ReadOnly Property XDirection As dxfDirection
            Get
                '^the X direction of the plane
                Return New dxfDirection(_Struc.XDirection) With {.RotationAxis = _Struc.ZDirection}
            End Get
        End Property
        Friend ReadOnly Property XDirectionV As TVECTOR
            '^the X direction of the plane
            Get
                Return _Struc.XDirection
            End Get



        End Property
        Public Property Y As Double Implements iVector.Y
            Get
                Return _Struc.Origin.Y
            End Get
            Set(value As Double)
                SetCoordinates(NewY:=value)
            End Set
        End Property
        Public Overridable ReadOnly Property YDirection As dxfDirection
            Get
                '^the Y direction of the plane
                Return New dxfDirection(_Struc.YDirection) With {.RotationAxis = _Struc.ZDirection}
            End Get
        End Property
        Friend ReadOnly Property YDirectionV As TVECTOR
            '^the Y direction of the plane
            Get
                Return _Struc.YDirection
            End Get
        End Property
        Public Property Z As Double Implements iVector.Z
            Get
                Return _Struc.Origin.Z
            End Get
            Set(value As Double)
                SetCoordinates(NewZ:=value)
            End Set
        End Property
        Public ReadOnly Property ZDirection As dxfDirection
            '^the Z direction of the plane
            Get
                Return New dxfDirection(_Struc.ZDirection) With {.RotationAxis = _Struc.XDirection}
            End Get
        End Property
        Friend ReadOnly Property ZDirectionV As TVECTOR
            Get
                Return _Struc.ZDirection
            End Get
        End Property

        Public ReadOnly Property Vertices As IEnumerable(Of iVector) Implements iShape.Vertices
            Get
                Return Corners()
            End Get
        End Property



        Public ReadOnly Property Closed As Boolean Implements iShape.Closed
            Get
                Return True
            End Get

        End Property


#Region "iRectangle Implementation"

        Private Property iRectangle_Plane As dxfPlane Implements iRectangle.Plane
            Get
                Return Plane
            End Get
            Set(value As dxfPlane)
                Plane = value
            End Set
        End Property




        Private Property iRectangle_Center As iVector Implements iRectangle.Center
            Get
                Return Center
            End Get
            Set(value As iVector)
                Center = New dxfVector(value)
            End Set
        End Property

#End Region 'iRectangle Implementation

#End Region 'Properties
#Region "Methods"

        Public Function VerticalCenterLine(Optional aExtend As Double = 0) As dxeLine

            '^the edge from the top left to the bottm left corner
            Return New dxeLine(VerticalCenterLineV, bInvert:=False, aExtend:=aExtend, DisplaySettings)

        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String = $" {Name} [ { XDirectionV.ToString().Replace("TVECTOR", "XDIR") } { YDirectionV.ToString().Replace("TVECTOR", "YDIR") } { CenterV.ToString().Replace("TVECTOR", "ORIGIN") }]"
            _rVal += $" {Width:0.0#} Wx{Height:0.0#}H"
            If Not String.IsNullOrWhiteSpace(Tag) Then
                _rVal += $" :: {Tag}"
            End If
            Return _rVal.Trim()

        End Function
        Public Function AlignToOCS(aOCS As dxfPlane, Optional bAlignOrigin As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the coordinate system to align to
            '#2flag to center the planes origin to the passed systems origin
            '^aligns the plane to the xy plane of the passed coordinate system
            If aOCS IsNot Nothing Then
                If bAlignOrigin Then
                    Define(aOCS.OriginV, aOCS.XDirectionV, aOCS.YDirectionV)
                Else
                    Define(_Struc.Origin, aOCS.XDirectionV, aOCS.YDirectionV)
                End If
            End If
            Return _rVal
        End Function
        Public Overridable Function Point(aPointEnum As dxxRectanglePts, Optional bSuppressShear As Boolean = False, Optional aXOffset As Double = 0, Optional aYOffset As Double = 0) As dxfVector

            Return New dxfVector(PointV(aPointEnum, bSuppressShear, aXOffset:=aXOffset, aYOffset:=aYOffset))
        End Function

        Friend Overridable Function PointV(aAlignment As dxxRectanglePts, Optional bSuppressShear As Boolean = False, Optional bAllowForDescent As Boolean = False, Optional aCode As Byte = 0, Optional aXOffset As Double = 0, Optional aYOffset As Double = 0) As TVECTOR
            Return _Struc.Point(aAlignment, bSuppressShear, bAllowForDescent, aCode, aXOffset, aYOffset)
        End Function

        Public Function AsSolid(Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aColor As dxxColors = dxxColors.Undefined, Optional aLayerName As String = "") As dxeSolid
            Dim _rVal As New dxeSolid(Corners(), aDisplaySettings, New dxfPlane(Strukture))
            If aDisplaySettings Is Nothing Then _rVal.CopyDisplayValues(DisplayStructure)
            If aLayerName <> "" Then _rVal.LayerName = aLayerName
            If aColor <> dxxColors.Undefined Then _rVal.Color = aColor
            Return _rVal
        End Function
        Public Function BestFitScale(aFitWidth As Double, aFitHeight As Double, Optional bWholeNumsOnly As Boolean = False, Optional bReturnANSIStandard As Boolean = False, Optional aWidthBuffer As Object = Nothing, Optional aHeightBuffer As Object = Nothing, Optional bMetricScales As Boolean = False) As Double
            Return _Struc.BestFitScale(aFitWidth, aFitHeight, bWholeNumsOnly, bReturnANSIStandard, aWidthBuffer, aHeightBuffer, bMetricScales)
        End Function
        Public Function BestFitScale(aFitWidth As Double, aFitHeight As Double, bWholeNumsOnly As Boolean, bReturnANSIStandard As Boolean, ByRef rScaleString As String, Optional aWidthBuffer As Object = Nothing, Optional aHeightBuffer As Object = Nothing, Optional bMetricScales As Boolean = False) As Double
            Return _Struc.BestFitScale(aFitWidth, aFitHeight, bWholeNumsOnly, bReturnANSIStandard, rScaleString, aWidthBuffer, aHeightBuffer, bMetricScales)
        End Function
        Public Function BorderLines(Optional aLineWidth As Double = 0.0, Optional aLineType As String = "", Optional bIncludeBaseline As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If Not IsDefined(False) Then Return _rVal
            Dim aDSP As TDISPLAYVARS = DisplayStructure
            If aDisplaySettings IsNot Nothing Then aDSP = aDisplaySettings.Strukture
            If aLineType <> "" Then aDSP.Linetype = aLineType
            _rVal = Corners().ConnectingLines(True, aLineWidth:=aLineWidth, aDisplaySettings:=New dxfDisplaySettings(aDSP))
            If bIncludeBaseline Then
                If _Struc.Descent <> 0 And _Struc.Width <> 0 Then _rVal.Add(BaseLine)
            End If
            Return _rVal
        End Function
        Public Function Borders(Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            If Not IsDefined(False) Then Return New colDXFEntities
            Return New colDXFEntities(_Struc.Borders, aDisplaySettings)
        End Function
        Friend Function BorderLinesV() As TLINES
            Return _Struc.Borders
        End Function
        Public Function Bounds(Optional bIncludeBaseline As Boolean = False) As dxePolygon
            If _Struc.Descent = 0 Then bIncludeBaseline = False
            Dim _rVal As New dxePolygon With {
                .SuppressEvents = True,
                .Plane = Plane,
                .Closed = Not bIncludeBaseline,
                .DisplayStructure = _Struc.DisplayProps
            }
            If Not bIncludeBaseline Then
                _rVal.Vertices.Add(TopLeft)
                _rVal.Vertices.Add(BottomLeft)
                _rVal.Vertices.Add(BottomRight)
                _rVal.Vertices.Add(TopRight)
            Else
                _rVal.Vertices.Add(BaselineRight)
                _rVal.Vertices.Add(TopRight)
                _rVal.Vertices.Add(TopLeft)
                _rVal.Vertices.Add(BottomLeft)
                _rVal.Vertices.Add(BottomRight)
                _rVal.Vertices.Add(BaselineRight)
                _rVal.Vertices.Add(BaselineLeft)
                _rVal.SuppressEvents = False
            End If
            Return _rVal
        End Function
        Public Function ClearDimensions() As Boolean
            Dim _rVal As Boolean
            If _Struc.Height <> 0 Then _rVal = True
            If _Struc.Width <> 0 Then _rVal = True
            _Struc.Height = 0
            _Struc.Width = 0
            _Struc.Descent = 0
            If _rVal Then RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Dimensions)
            Return _rVal
        End Function

        Friend Sub SetDirections(aXDirection As TVECTOR, aYDirection As TVECTOR)
            _Struc._XDirection = aXDirection
            _Struc._YDirection = aYDirection

        End Sub
        Public Function Clone() As dxfRectangle
            '^returns a new object with properties matching those of the cloned object
            Return New dxfRectangle(_Struc)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxfRectangle(_Struc)
        End Function

        Public Function ContainsVector(aVector As iVector, Optional aFudgeFactor As Double = 0.001, Optional bSuppressProjection As Boolean = False) As Boolean
            Dim rIsonBound As Boolean = False
            Dim rIsCorner As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsonBound, rIsCorner, bSuppressProjection)
        End Function
        Public Function ContainsVector(aVector As iVector, aFudgeFactor As Double, ByRef rIsonBound As Boolean, ByRef rIsCorner As Boolean, Optional bSuppressProjection As Boolean = False) As Boolean
            '#1the vector to dxeDimension
            '#2a fudge factor to apply
            '#3returns true if the passed vector lies on the bounds of the rectangle
            '#4returns true if the passed vector lies on the corner of the rectangle
            '^returns true if the passed vector lies within the bounds of the rectangle
            rIsonBound = False
            rIsCorner = False
            If aVector Is Nothing Then Return False
            Return ContainsVector(New TVECTOR(aVector), aFudgeFactor, rIsonBound, rIsCorner, bSuppressPlaneTest:=bSuppressProjection, False, False)
        End Function
        Friend Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False, Optional bSimpleTest As Boolean = False) As Boolean
            Dim rIsonBound As Boolean = False
            Dim rIsCorner As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsonBound, rIsCorner, bSuppressPlaneTest, bSuppressEdgeTest, bSimpleTest)
        End Function
        Friend Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsonBound As Boolean, ByRef rIsCorner As Boolean, Optional bSuppressPlaneTest As Boolean = False, Optional bSuppressEdgeTest As Boolean = False, Optional bSimpleTest As Boolean = False) As Boolean
            '#1the vector to test
            '#2a fudge factor to apply
            '#3returns true if the passed vector lies on the bounds of the rectangle
            '#4returns true if the passed vector lies on the corner of the rectangle
            '^returns true if the passed vector lies within the bounds of the rectangle
            rIsonBound = False
            rIsCorner = False
            Return _Struc.Contains(aVector, aFudgeFactor, rIsonBound, rIsCorner, bSuppressPlaneTest, bSuppressEdgeTest, bSimpleTest)
        End Function
        Public Function ContainsVectors(aVectors As colDXFVectors, Optional aFudgeFactor As Double = 0.001, Optional bContainsAll As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the vector to dxeDimension
            '#2a fudge factor to apply
            '#3returns true if all the passed vectors are contained
            '^returns true if any of the passed vectors lie within the bounds of the rectangle
            bContainsAll = False
            If aVectors Is Nothing Then Return _rVal
            If aVectors.Count <= 0 Then Return _rVal
            Dim i As Integer
            Dim v1 As dxfVector
            '**UNUSED VAR** Dim bFlag As Boolean
            Dim f1 As Double
            f1 = Math.Abs(aFudgeFactor)
            If f1 > 0.1 Then f1 = 0.1
            If f1 < 0.000001 Then f1 = 0.000001
            bContainsAll = True
            For i = 1 To aVectors.Count
                v1 = aVectors.Item(i)
                If ContainsVector(v1.Strukture, f1) Then
                    _rVal = True
                Else
                    bContainsAll = False
                End If
            Next i
            Return _rVal
        End Function
        Public Function CopyDisplayValues(aEntitySet As dxfDisplaySettings, Optional aMatchLayer As String = "", Optional aMatchColor As dxxColors = dxxColors.Undefined, Optional aMatchLineType As String = "") As Boolean
            Dim _rVal As Boolean
            '#1the entity settings to copy
            '#2a layer name that if passed the entities layer name will not be changed unless it currently matches this string
            '#3a color that if defined the entities color will not be changed unless it currently matches this value
            '#4a linetype name that if passed the entities linetype name will not be changed unless it currently matches this string
            '^copies the values of the passed display settings to this entities display settings
            Dim aSettings As dxfDisplaySettings
            aSettings = DisplaySettings
            _rVal = aSettings.CopyDisplayValues(aEntitySet, aMatchLayer, aMatchColor, aMatchLineType)
            _Struc.DisplayProps = aSettings.Strukture
            Return _rVal
        End Function
        Friend Overridable Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, Optional bSuppressEvnts As Boolean = False, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim rOriginChange As Boolean = False
            Dim rDirectionChange As Boolean = False
            Dim rDimChange As Boolean = False
            Return Define(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimChange, bSuppressEvnts, aHeight, aWidth)
        End Function
        Friend Overridable Function Define(aOrigin As TVECTOR, aXDir As TVECTOR, aYDir As TVECTOR, rOriginChange As Boolean, rDirectionChange As Boolean, ByRef rDimensionChange As Boolean, Optional bSuppressEvnts As Boolean = False, Optional aHeight As Double? = Nothing, Optional aWidth As Double? = Nothing) As Boolean
            Dim _rVal As Boolean
            _Struc = _Struc.ReDefined(aOrigin, aXDir, aYDir, rOriginChange, rDirectionChange, rDimensionChange, _rVal, aHeight, aWidth)
            Dim aType As dxxCoordinateSystemEventTypes
            If _rVal And Not bSuppressEvnts Then
                If rOriginChange Then aType = dxxCoordinateSystemEventTypes.Origin
                If rDirectionChange Then aType += dxxCoordinateSystemEventTypes.Orientation
                If rDimensionChange Then aType += dxxCoordinateSystemEventTypes.Dimensions
                RaiseEvent RectangleChange(aType)
            End If
            Return _rVal
        End Function
        Public Function DefineByLine(diag As dxeLine) As Boolean
            Dim _rVal As Boolean
            '#1the subject line
            '^the line to use to define the rectangle
            '~the line is assumed to run from the lower left to the upper right and be on the current plane
            If diag IsNot Nothing Then _rVal = DefineByLineV(New TLINE(diag))
            Return _rVal
        End Function
        Friend Function DefineByLineV(diag As TLINE) As Boolean

            '#1the subject line
            '^the line to use to define the rectangle
            '~the line is assumed to run from the lower left to the upper right and be on the current plane

            Dim oldStruc As TPLANE = _Struc

            DefineByVectors(New colDXFVectors(New dxfVector(diag.SPT), New dxfVector(diag.SPT)))
            If _Struc.Width <> oldStruc.Width Then Return True
            If _Struc.Height <> oldStruc.Height Then Return True
            If oldStruc.Origin <> _Struc.Origin Then Return True


            Return False
        End Function

        Public Sub DefineByRectangleCol(aRectCol As List(Of dxfRectangle), Optional aAngle As Double = 0.0, Optional aPlane As dxfPlane = Nothing)

            If aRectCol Is Nothing Then Return
            Dim Crns As New colDXFVectors
            For Each rec As dxfRectangle In aRectCol

                Crns.Append(rec.Corners(), False)
            Next
            DefineByVectors(Crns, aPlane)
            If aAngle <> 0 Then Rotate(aAngle, False)
        End Sub
        Public Sub DefineByRectangles(aRect As dxfRectangle, bRect As dxfRectangle)
            If aRect Is Nothing And bRect Is Nothing Then Return
            If aRect IsNot Nothing And bRect Is Nothing Then
                Define(aRect.CenterV, aRect.XDirectionV, aRect.YDirectionV, False, aRect.Height, aRect.Width)
            ElseIf aRect Is Nothing And bRect IsNot Nothing Then
                Define(bRect.CenterV, bRect.XDirectionV, bRect.YDirectionV, False, bRect.Height, bRect.Width)
            Else
                Dim aCol As New List(Of dxfRectangle)({aRect, bRect})

                DefineByRectangleCol(aCol)
            End If
        End Sub
        Public Function DefineByVectors(DefVectors As IEnumerable(Of iVector), Optional aPlane As dxfPlane = Nothing, Optional bSuppressProjection As Boolean = False) As Boolean


            Dim vecs As New TVECTORS(DefVectors)
            Dim newStruc As TPLANE = vecs.BoundingRectangle(_Struc, aPlane, bSuppressProjection)
            Return Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, False, newStruc.Height, newStruc.Width)

        End Function
        Friend Function DefineByVectorsV(DefVectors As TVECTORS) As Boolean

            Dim newStruc As TPLANE = DefVectors.Bounds(_Struc)
            Return Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, False, newStruc.Height, newStruc.Width)

        End Function
        Public Sub DefineCorners(aTopLeft As iVector, aBottomLeft As iVector, aBottomRight As iVector, aTopRight As iVector, Optional aPlane As dxfPlane = Nothing)


            DefineByVectors(New colDXFVectors(New dxfVector(aTopLeft), New dxfVector(aTopRight), New dxfVector(aBottomLeft), New dxfVector(aBottomRight)), aPlane)
        End Sub
        Public Function DefineLWTH(aLeft As Double, aWidth As Double, aTop As Double, aHeight As Double, Optional aCS As dxfPlane = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean

            If aWidth = 0 Then aWidth = 1
            If aHeight = 0 Then aHeight = 1
            aWidth = Math.Abs(aWidth)
            aHeight = Math.Abs(aHeight)
            Dim newStruc As New TPLANE(_Struc)

            If aCS IsNot Nothing Then
                newStruc.Define(aCS.OriginV, aCS.XDirectionV, aCS.YDirectionV)
            Else
                If Not dxfPlane.IsNull(aPlane) Then
                    newStruc.Define(aPlane.OriginV, aPlane.XDirectionV, aCS.YDirectionV)
                End If
            End If
            Dim cp As New TVECTOR(aLeft, aTop, _Struc.Origin.Z)
            cp += newStruc.XDirection * (0.5 * aWidth) + newStruc.YDirection * (-0.5 * aHeight)
            Return Define(cp, newStruc.XDirection, newStruc.YDirection, False, aHeight, aWidth)
        End Function
        Public Sub DefineSimple(aTopY As Double, aLeftX As Double, aBotY As Double, aRightX As Double)
            DefineLWTH(aLeftX, aRightX - aLeftX, aTopY, aTopY - aBotY)
        End Sub
        Public Function DefineWHC(Optional aWidth As Double = 0.0, Optional aHeight As Double = 0.0, Optional aCenter As iVector = Nothing, Optional bNoEvents As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim v1 As TVECTOR
            If aWidth <> _Struc.Width Then _rVal = True
            If aHeight <> _Struc.Height Then _rVal = True
            v1 = _Struc.Origin
            If aCenter IsNot Nothing Then
                v1 = New TVECTOR(aCenter)
                If v1 <> _Struc.Origin Then _rVal = True
            End If
            If Not _rVal Then Return _rVal
            If Not bNoEvents Then
                Height = aHeight
                Width = aWidth
                _Struc.Origin = v1
            Else
                _Struc.Height = aHeight
                _Struc.Width = aWidth
                _Struc.Origin = v1
            End If
            Return _rVal
        End Function
        Friend Function DefineWHCV(aWidth As Double, aHeight As Double, aCenter As TVECTOR, Optional aScaler As Double = 1, Optional bResetDirections As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim newStruc As TPLANE = _Struc.Clone
            newStruc.Width = Math.Abs(aWidth) * aScaler
            newStruc.Height = Math.Abs(aHeight) * aScaler
            newStruc.Origin = aCenter
            If bResetDirections Then
                newStruc.Define(aCenter, TVECTOR.WorldX, TVECTOR.WorldY)
            End If
            _rVal = Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, False, newStruc.Height, newStruc.Width)
            Return _rVal
        End Function
        Public Function DefinitionPts(Optional aProjectionPlane As dxfPlane = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors

            If Height = 0 And Width = 0 Then
                _rVal.AddV(_Struc.Origin)
            Else

                If _Struc.Height = 0 Or _Struc.Width = 0 Then
                    _rVal.AddV(VecV(-0.5 * _Struc.Width, 0.5 * _Struc.Height))
                    If _Struc.Height = 0 Then
                        _rVal.AddV(VecV(0.5 * _Struc.Width, 0.5 * _Struc.Height))
                    Else
                        _rVal.AddV(VecV(-0.5 * _Struc.Width, 0.5 * _Struc.Height))
                    End If
                Else
                    _rVal.AddV(VecV(-0.5 * _Struc.Width, 0.5 * _Struc.Height))
                    _rVal.AddV(VecV(-0.5 * _Struc.Width, -0.5 * _Struc.Height))
                    _rVal.AddV(VecV(0.5 * _Struc.Width, -0.5 * _Struc.Height))
                    _rVal.AddV(VecV(0.5 * _Struc.Width, 0.5 * _Struc.Height))
                End If
            End If
            _rVal.ProjectToPlane(aProjectionPlane)
            Return _rVal
        End Function
        Public Function Dimensions(Optional aUnits As dxxDeviceUnits = dxxDeviceUnits.Undefined) As String
            Dim _rVal As String = String.Empty
            Dim aSF As Double
            If aUnits <= 0 Then
                aUnits = _Struc.Units
                aSF = 1
            Else
                aSF = dxfUtils.FactorFromTo(_Struc.Units, aUnits)
            End If
            If aUnits = dxxDeviceUnits.Pixels Or aUnits = dxxDeviceUnits.Characters Then
                _rVal = TVALUES.To_INT(_Struc.Width * aSF) & "W x " & TVALUES.To_INT(_Struc.Height * aSF) & "H " & dxfEnums.Description(aUnits)
            ElseIf aUnits = dxxDeviceUnits.Points Or aUnits = dxxDeviceUnits.Centimeters Or aUnits = dxxDeviceUnits.Millimeters Then
                _rVal = Format(_Struc.Width * aSF, "0.0") & "W x " & Format(_Struc.Height * aSF, "0.0") & "H " & dxfEnums.Description(aUnits)
            Else
                _rVal = Format(_Struc.Width * aSF, "0.00") & "W x " & Format(_Struc.Height * aSF, "0.00") & "H " & dxfEnums.Description(aUnits)
            End If
            Return _rVal
        End Function

        Public Function Edge(aEdge As dxxRectangleLines, Optional bSuppressShear As Boolean = True, Optional aLengthAdder As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeLine
            Return New dxeLine(EdgeV(aEdge, bSuppressShear, aLengthAdder), aDisplaySettings:=aDisplaySettings)
        End Function

        Friend Overridable Function EdgeV(aEdge As dxxRectangleLines, Optional bSuppressShear As Boolean = True, Optional aLengthAdder As Double = 0.0) As TLINE
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
            v1 = PointV(P1, bSuppressShear)
            v2 = PointV(P2, bSuppressShear)
            If aLengthAdder <> 0 Then
                Dim aDir As TVECTOR
                aDir = v1.DirectionTo(v2)
                v1 += aDir * (-0.5 * aLengthAdder)
                v2 += aDir * (0.5 * aLengthAdder)
            End If
            Return New TLINE(v1, v2)
        End Function
        Friend Function EdgesV() As TLINES

            Dim _rVal As New TLINES(0)
            _rVal.Add(EdgeV(dxxRectangleLines.LeftEdge))
            _rVal.Add(EdgeV(dxxRectangleLines.TopEdge))
            _rVal.Add(EdgeV(dxxRectangleLines.RightEdge))
            _rVal.Add(EdgeV(dxxRectangleLines.BottomEdge))
            Return _rVal

        End Function

        Public Function Edges(Optional aDisplaySettings As dxfDisplaySettings = Nothing) As List(Of dxeLine)

            Return New List(Of dxeLine)({Edge(dxxRectangleLines.LeftEdge, aDisplaySettings:=aDisplaySettings), Edge(dxxRectangleLines.TopEdge, aDisplaySettings:=aDisplaySettings), Edge(dxxRectangleLines.RightEdge, aDisplaySettings:=aDisplaySettings), Edge(dxxRectangleLines.BottomEdge, aDisplaySettings:=aDisplaySettings)})

        End Function

        Public Sub Expand(aExpansion As Double, Optional bExpandWidth As Boolean = True, Optional bExpandHeight As Boolean = True)
            '#1the factor to multiply by
            '#2flag to apply the expansion to the width
            '#3flag to apply the expansion to the height
            '^multiplies the current dimensions by the passed factor
            Dim fctr As Double
            If bExpandHeight And _Struc.Height > 0 Then
                fctr = Descent / _Struc.Height
                Height = _Struc.Height * aExpansion
                Descent = fctr * _Struc.Height
            End If
            If bExpandWidth Then Width = _Struc.Width * aExpansion
        End Sub
        Friend Function ExpandToRectangle(aRectangle As dxfRectangle) As Boolean
            If aRectangle Is Nothing Then Return False
            Return ExpandToVectors(aRectangle.Corners())
        End Function
        Friend Function ExpandToVectors(aVectors As IEnumerable(Of iVector)) As Boolean
            'On Error Resume Next
            If aVectors Is Nothing Then Return False
            If aVectors.Count <= 0 Then Return False
            Return ExpandToVectorsV(New TVECTORS(aVectors))
        End Function
        Friend Function ExpandToVectorsV(aVectors As TVECTORS) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If aVectors.Count <= 0 Then Return _rVal
            Dim newStruc As New TPLANE("")
            newStruc = _Struc.Clone()
            newStruc.ExpandToVectors(aVectors)
            _rVal = Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, False, newStruc.Height, newStruc.Width)
            Return _rVal
        End Function
        Public Function Expanded(aExpansion As Double, Optional bExpandWidth As Boolean = True, Optional bExpandHeight As Boolean = True) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            '#1the factor to multiply by
            '#2flag to apply the expansion to the width
            '#3flag to apply the expansion to the height
            '^multiplies the current dimensions by the passed factor
            _rVal = Clone()
            _rVal.Expand(aExpansion, bExpandWidth, bExpandHeight)
            Return _rVal
        End Function

        Public Sub GetDimensions(ByRef rWidth As Double, ByRef rHeight As Double, Optional aMultiplier As Double = 1)
            '#1returns the width
            '#2returns the height
            '^returns the dimensions of the rectangle
            rWidth = _Struc.Width * Math.Abs(aMultiplier)
            rHeight = _Struc.Height * Math.Abs(aMultiplier)
        End Sub
        Public Sub GetLimits(ByRef rMinX As Double, ByRef rMaxX As Double, ByRef rMinY As Double, ByRef rMaxY As Double)
            '^simply returns the 2D ordinate limits of the rectangle
            Dim UL As TVECTOR = TopLeftV
            Dim LR As TVECTOR = BottomRightV

            rMinX = UL.X
            rMaxX = LR.X
            rMinY = LR.Y
            rMaxY = UL.Y
        End Sub
        Public Function IntersectionPts(aLine As dxeLine, Optional aLineIsInfinite As Boolean = False) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If (_Struc.Height = 0 And _Struc.Width = 0) Or aLine Is Nothing Then Return New colDXFVectors()
            Return New colDXFVectors(IntersectionPts(New TLINE(aLine), aLineIsInfinite))

        End Function
        Friend Function IntersectionPts(aLine As TLINE, Optional aLineIsInfinite As Boolean = False) As TVECTORS
            If _Struc.Height = 0 And _Struc.Width = 0 Then Return New TVECTORS(0)
            If aLine.Length <= 0 Then Return New TVECTORS(0)
            Return aLine.IntersectionPts(BorderLinesV, aLineIsInfinite, False, True)
        End Function
        Friend Function IntersectionPts(aArc As TARC, Optional aArcIsInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True, Optional bSuppressPlaneCheck As Boolean = True, Optional aCurveDivisions As Integer = 1000) As TVECTORS
            If _Struc.Height = 0 And _Struc.Width = 0 Then Return New TVECTORS(0)
            Return BorderLinesV.IntersectionPts(aArc, False, aArcIsInfinite, bMustBeOnBoth, bSuppressPlaneCheck, aCurveDivisions)
        End Function
        Public Function Intersects(aEntity As dxfEntity, Optional aEntityIsInfinite As Boolean = False, Optional aContainsCenter As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If aEntity Is Nothing Then Return _rVal
            Dim aRec As TPLANE = aEntity.Bounds
            Dim diag As Double = 0.5 * Diagonal
            Dim d1 As Double
            If (aRec.Width <= 0 Or aRec.Height <= 0) Or diag <= 0 Then Return _rVal
            d1 = dxfProjections.DistanceTo(_Struc.Origin, aRec.Origin)
            If d1 > diag + 0.5 * Math.Sqrt(aRec.Width ^ 2 + aRec.Height ^ 2) Then Return _rVal
            If aContainsCenter Then
                If _Struc.Contains(aRec.Origin, bSuppressPlaneTest:=True) Then
                    _rVal = True
                    Return _rVal
                End If
            End If
            _rVal = Perimeter.Intersections(aEntity, aEntityIsInfinite, bMustBeOnBoth:=True).Count > 0
            Return _rVal
        End Function
        Public Function IsEqual(aRectangle As dxfRectangle, Optional bCompareCenter As Boolean = False, Optional aPrecis As Integer = 4, Optional bCompareDirections As Boolean = True) As Boolean
            If aRectangle IsNot Nothing Then
                Return TPLANES.Compare2(_Struc, aRectangle.Strukture, aPrecis, bCompareCenter, True, bCompareDirections)
            Else
                Return False
            End If
        End Function
        Public Function LiesOnPlane(aPlane As dxfPlane, Optional aPrecision As Integer = 1) As Boolean
            Dim _rVal As Boolean
            If Not dxfPlane.IsNull(aPlane) Then
                aPrecision = TVALUES.LimitedValue(aPrecision, 1, 6, 1)
                If aPlane.ZDirectionV.Equals(_Struc.ZDirection, True, aPrecision) Then
                    Dim fudge As Double
                    fudge = (1 / TVALUES.To_DBL(1 & New String("0", aPrecision)))
                    _rVal = _Struc.Origin.LiesOn(aPlane.Strukture, fudge)
                End If
            End If
            Return _rVal
        End Function
        Public Function LineSegments(Optional LineDivisions As Integer = 1) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            '^returns the entity as a collection of lines
            _rVal = BorderLines()
            Return _rVal
        End Function

        Public Function Mirror(aMirrorAxis As iLine, Optional bMirrorCenter As Boolean = False, Optional bMirrorDirections As Boolean = True) As Boolean
            '#1the line to mirror across
            '#2flag to mirror the center
            '^mirrors the rectangle across the passed line
            '~returns True if the rectangle actually moves from this process
            If aMirrorAxis Is Nothing Then Return False
            Return Mirror(New TLINE(aMirrorAxis), bMirrorCenter, bMirrorDirections, False)
        End Function
        Friend Function Mirror(aMirrorAxis As TLINE, Optional bMirrorCenter As Boolean = False, Optional bMirrorDirections As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            '#1the line to mirror across
            '#2flag to mirror the center
            '^mirrors the rectangle across the passed line
            '~returns True if the rectangle actually moves from this process
            Dim _rVal As Boolean
            Dim newStruc As TPLANE = _Struc.Mirrored(aMirrorAxis.SPT, aMirrorAxis.EPT, bMirrorCenter, bMirrorDirections, False, _rVal)
            If _rVal Then Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, bSuppressEvnts)
            Return _rVal

        End Function


        Public Function MirrorPlanar(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean

            If Not aMirrorX.HasValue And Not aMirrorY.HasValue Then Return False

            Dim _rVal As Boolean = False
            '#1the x coordinate to mirror across
            '#2the y coordinate to mirror across
            '^mirrors the current coordinates to a vector mirrored across the passed values
            '~only allows orthogonal mirroring.
            Dim aPl As TPLANE
            Dim aLn As TLINE
            If TPLANE.IsNull(aPlane) Then aPl = _Struc Else aPl = New TPLANE(aPlane)
            If aMirrorX.HasValue Then
                aLn = aPl.LineV(aMirrorX.Value, 10)
                If Mirror(aLn, True, True, True) Then _rVal = True
            End If
            If aMirrorY.HasValue Then
                aLn = aPl.LineH(aMirrorY.Value, 10)
                If Mirror(aLn, True, True, True) Then _rVal = True
            End If
            If _rVal Then RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Orientation)
            Return _rVal
        End Function




        Public Function Mirrored(aLineObj As iLine, Optional bMirrorOrigin As Boolean = False, Optional bMirrorDirections As Boolean = True) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            '#1the line to mirror across
            '#4returns the actual mirror axis
            '^returns a copy of the entity mirrored across then passed line
            _rVal = Clone()
            _rVal.Mirror(aLineObj, bMirrorOrigin, bMirrorDirections)
            Return _rVal
        End Function
        Public Function Move(Optional ChangeX As Double = 0.0, Optional ChangeY As Double = 0.0, Optional ChangeZ As Double = 0.0, Optional aCS As dxfPlane = Nothing) As Boolean

            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Return Translate(New TVECTOR(ChangeX, ChangeY, ChangeZ), aCS, False)

        End Function
        Public Function MoveFromTo(BasePointXY As iVector, DestinationPointXY As iVector) As Boolean

            '^used to move the object from one reference vector to another
            Return MoveFromToV(New TVECTOR(BasePointXY), New TVECTOR(DestinationPointXY))
        End Function
        Friend Function MoveFromToV(BasePoint As TVECTOR, DestinationPoint As TVECTOR) As Boolean

            '^used to move the object from one reference vector to another
            Return Translate(DestinationPoint - BasePoint)
        End Function
        Public Function MoveTo(aDestinationXY As iVector, Optional aChangeX As Double = 0.0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0) As Boolean
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the entity from its current insertion point to the passed point
            '~returns True if the entity actually moves from this process
            Dim v1 As TVECTOR = New TVECTOR(aDestinationXY) + New TVECTOR(aChangeX, aChangeY, aChangeZ) - _Struc.Origin
            Return Translate(v1 - _Struc.Origin)
        End Function
        Friend Function Translate(aTranslation As TVECTOR, Optional aCS As dxfPlane = Nothing, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the displacement to apply
            '#2a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the entity by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Dim newStruc As New TPLANE(_Struc)
            newStruc.Translate(aTranslation, aCS, _rVal)
            If _rVal Then Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, bSuppressEvnts:=bSuppressEvnts)
            Return _rVal
        End Function
        Public Function Perimeter(Optional bIncludeBaseline As Boolean = False, Optional aColor As dxxColors = dxxColors.Undefined, Optional aSegmentWidth As Double = 0.0, Optional aLayer As String = "", Optional aLineType As String = "") As dxePolyline
            Dim _rVal As New dxePolyline
            If _Struc.Descent = 0 Then bIncludeBaseline = False
            _rVal.PlaneV = _Struc
            _rVal.Closed = True
            _rVal.DisplayStructure = _Struc.DisplayProps
            _rVal.VectorsV = RectanglePts(bIncludeBaseline, True)
            If aColor <> dxxColors.Undefined Then _rVal.Color = aColor
            If aLayer <> "" Then _rVal.LayerName = aLayer
            If aLineType <> "" Then _rVal.Linetype = aLineType
            _rVal.SegmentWidth = aSegmentWidth
            _rVal.Tag = Tag
            _rVal.Flag = Flag
            Return _rVal
        End Function

        Friend Overridable Function RectanglePts(bIncludeBaseline As Boolean, Optional bSuppressShear As Boolean = True) As TVECTORS
            Return _Struc.RectanglePts(bIncludeBaseline, bSuppressShear)
        End Function

        Friend Function PhantomPoints(Optional aCurveDivisions As Integer = 20, Optional aLineDivision As Integer = 1) As colDXFVectors
            Dim _rVal As colDXFVectors
            '^a collection of phantom vertices along the bounds
            _rVal = BorderLines.PhantomPoints(aLineDivision)
            Return _rVal
        End Function
        Public Function Pointer() As dxePolyline
            Dim _rVal As New dxePolyline With {
                .SuppressEvents = True,
                .PlaneV = _Struc,
                .Closed = False,
                .DisplayStructure = _Struc.DisplayProps
            }
            _rVal.Vertices.AddV(_Struc.Vector(-0.1626312 * _Struc.Width, 0.40288 * _Struc.Height, aShearXAngle:=0))
            _rVal.Vertices.AddV(_Struc.Vector(0.5 * _Struc.Width, 0, aShearXAngle:=0))
            _rVal.Vertices.AddV(TopRightV)
            _rVal.Vertices.AddV(TopLeftV)
            _rVal.Vertices.AddV(BottomLeftV)
            _rVal.Vertices.AddV(BottomRightV)
            _rVal.Vertices.AddV(_Struc.Vector(0.5 * _Struc.Width, 0, aShearXAngle:=0))
            _rVal.Vertices.AddV(_Struc.Vector(-0.1626312 * _Struc.Width, -0.40288 * _Struc.Height, aShearXAngle:=0))
            _rVal.SuppressEvents = False
            Return _rVal
        End Function
        Public Function Project(aDirectionObj As iVector, aDistance As Double) As Boolean
            Dim _rVal As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the rectangles
            _rVal = ProjectV(New TVECTOR(aDirectionObj), aDistance)
            Return _rVal
        End Function
        Public Function Project(aDirection As dxfDirection, aDistance As Double) As Boolean
            Dim _rVal As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the rectangles
            _rVal = ProjectV(New TVECTOR(aDirection), aDistance)
            Return _rVal
        End Function

        Public Sub ProjectTo(aPtXY As iVector)
            '#1the vector to project to
            '^projects the entity in the object
            If aPtXY Is Nothing Then Return
            Dim P1 As New TVECTOR(aPtXY)
            Dim cp As TVECTOR = _Struc.Origin
            ProjectV(cp.DirectionTo(P1), dxfProjections.DistanceTo(cp, P1))
        End Sub
        Friend Function ProjectV(aDirection As TVECTOR, aDistance As Double, Optional bSuppressEvnts As Boolean = False, Optional bSuppressNormalize As Boolean = False) As Boolean
            If aDistance = 0 Then Return False
            '#1the direction to project in
            '#2the distance to project
            If Not bSuppressNormalize Then aDirection.Normalize()
            '^projects the rectangles in the passed direction the requested distance
            Return Define(_Struc.Origin + aDirection * aDistance, _Struc.XDirection, _Struc.YDirection, bSuppressEvnts:=bSuppressEvnts)
        End Function
        Public Function ProjectedToPlane(aPlane As dxfPlane) As dxfRectangle
            Dim _rVal As dxfRectangle = Nothing
            _rVal = Clone()
            If Not dxfPlane.IsNull(aPlane) Then
                _rVal.CenterV = _Struc.Origin.ProjectedTo(aPlane.Strukture)
                _rVal.DefineByVectors(Corners())
            End If
            Return _rVal
        End Function
        Public Overridable Function Rescale(aScaleFactor As Double, Optional aReference As dxfVector = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the factor to scale the entity by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the entity in space and dimension by the passed factor
            Dim ref As TVECTOR
            If aReference Is Nothing Then ref = _Struc.Origin Else ref = aReference.Strukture
            _rVal = RescaleV(aScaleFactor, ref)
            If _rVal Then RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Dimensions)
            Return _rVal
        End Function
        Friend Function RescaleV(aScaleFactor As Double, aReference As TVECTOR, Optional aCS As dxfPlane = Nothing, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the factor to scale the entity by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the entity in space and dimension by the passed factor
            Dim newStruc As TPLANE = _Struc.Clone
            _rVal = newStruc.Rescale(aScaleFactor, aReference, aCS)
            If _rVal Then
                Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, bSuppressEvnts, newStruc.Height, newStruc.Width)
                _Struc.Descent = newStruc.Descent
            End If
            Return _rVal
        End Function
        Public Sub Resize(Optional aWidthAdder As Double = 0.0, Optional aHeightAdder As Double = 0.0, Optional bRecenter As Boolean = False)
            If aWidthAdder = 0 And aHeightAdder = 0 Then Return
            Dim wd As Object = Nothing
            Dim ht As Object = Nothing
            Dim org As TVECTOR = CenterV
            If aWidthAdder <> 0 Then
                wd = Math.Abs(Width + aWidthAdder)
                If bRecenter Then org += XDirectionV * (0.5 * aWidthAdder)
            End If
            If aHeightAdder <> 0 Then
                ht = Math.Abs(Height + aHeightAdder)
                If bRecenter Then
                    org += YDirectionV * (0.5 * aHeightAdder)
                End If
            End If
            Define(org, XDirectionV, YDirectionV, False, ht, wd)
        End Sub
        Public Function Rotate(aAngle As Double, Optional bInRadians As Boolean = False, Optional aAxis As dxeLine = Nothing, Optional bLocal As Boolean = True) As Boolean
            '#1the angle to rotate
            '#2flag indicating if the passed angle is in radians
            '#3returns the line used for the rotation axis
            '^rotates the plane about its z axis
            If bLocal Then aAxis = ZAxis() Else aAxis = New dxeLine(dxfVector.Zero, New dxfVector(0D, 0D, 10))
            Return RotateAbout(aAxis, aAngle, bInRadians, False, True)
        End Function
        Public Function RotateAbout(aPoint As iVector, aAngle As Double, Optional bInRadians As Boolean = False, Optional aAxis As dxeLine = Nothing, Optional aPlane As dxfPlane = Nothing, Optional bRotateCenter As Boolean = True, Optional bRotateDirections As Boolean = True) As Boolean
            '#1the line or vector to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '#4the OCS which is used as the Z axis of rotation if the paased object is a vector
            '^rotates the entity about the passed axis the requested angle
            '~if the passed object is a vector the entity is rotated about the Z axis of the passed coordinated system
            aAxis = dxfPlane.CreateAxis(aPlane, aPoint)
            If aAxis Is Nothing Then Return False
            Return RotateAbout(aAxis, aAngle, bInRadians, bRotateCenter, bRotateDirections)
        End Function
        Public Function RotateAbout(aLine As iLine, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateCenter As Boolean = False, Optional bRotateDirections As Boolean = True) As Boolean

            '#1the line object to rotate about
            '#2the angle to rotate
            '#3flag indicating if the passed angle is in radians
            '#4flag to rotate the origin of the plane about the axis
            '#5flag to rotate the X,Y and Z directions about the axis
            '#6returns the line used for the rotation axis
            '#7flag to raise no change events
            '^used to change the orientation and/or the origin of the plane by rotating the it about the passed axis
            If aLine Is Nothing Then Return False
            Dim axis As New TLINE(aLine)
            If axis.Length <= 0 Then Return False
            Return RotateAbout(axis.SPT, axis.Direction, aAngle, bInRadians, bRotateCenter, bRotateDirections, False)

        End Function
        Friend Function RotateAbout(aOrigin As TVECTOR, aDirection As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False, Optional bRotateCenter As Boolean = False, Optional bRotateDirections As Boolean = True, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the line start pt to rotate about
            '#2the line direction rotate about
            '#3the angle to rotate
            '#4flag indicating if the passed angle is in radians
            '#5flag to rotate the origin of the plane about the axis
            '#6flag to rotate the X,Y and Z directions about the axis
            '#7returns the line used for the rotation axis
            '#8flag to raise no change events
            '^used to change the orientation and/or the origin of the plane by rotating the it about the passed axis
            Dim newStruc = New TPLANE(Strukture)
            _rVal = newStruc.RotateAbout(aOrigin, aDirection, aAngle, bInRadians, bRotateCenter, bRotateDirections)
            If _rVal Then Define(newStruc.Origin, newStruc.XDirection, newStruc.YDirection, bSuppressEvnts:=bSuppressEvnts)
            Return _rVal
        End Function
        Public Function ScaleDimensions(Optional aWidthScaler As Double? = Nothing, Optional aHeightScaler As Double? = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the scale factor to apply to the width
            '#2the scale factor to apply to the height
            '^scales the dimensions of the rectangle and returns True if it changes
            _rVal = _Struc.ScaleDimensions(aWidthScaler, aHeightScaler)
            If _rVal Then RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Dimensions)
            Return _rVal
        End Function
        Public Sub ScaleUp(aScaleX As Double, Optional aScaleY As Double = 0.0, Optional aReference As iVector = Nothing)
            If aReference Is Nothing Then ScaleUp(aScaleX, aScaleY, _Struc.Origin) Else ScaleUp(aScaleX, aScaleY, New TVECTOR(aReference))
        End Sub
        Friend Function ScaleUp(aScaleX As Double, aScaleY As Double, aReference As TVECTOR, Optional bSuppressEvnts As Boolean = True, Optional aScaleZ As Double = 0.0) As Boolean
            Dim _rVal As Boolean
            If aScaleX <= 0 Then aScaleX = 1
            If aScaleY <= 0 Then aScaleY = aScaleX
            If aScaleZ <= 0 Then aScaleZ = 1
            If aScaleX = 1 And aScaleY = 1 And aScaleZ = 1 Then Return _rVal
            If (_Struc.Width <> 0 Or _Struc.Width <> 0 Or _Struc.Height <> 0 Or _Struc.Descent <> 0) Then _rVal = True
            _Struc.Width = aScaleX * _Struc.Width
            _Struc.Height = aScaleY * _Struc.Height
            If _Struc.Descent <> 0 Then
                _Struc.Descent = aScaleY * _Struc.Descent
            End If
            Dim v0 As TVECTOR = _Struc.Origin.Scaled(aScaleX, aReference, aScaleY, aScaleZ)
            If Define(v0, _Struc.XDirection, _Struc.YDirection, bSuppressEvnts:=bSuppressEvnts) Then _rVal = True
            Return _rVal
        End Function
        Public Function SetCoordinates(Optional NewX As Object = Nothing, Optional NewY As Object = Nothing, Optional NewZ As Object = Nothing) As Boolean
            '#1the value to set the X coordinate to
            '#2the value to set the Y coordinate to
            '#3the value to set the Z coordinate to
            '^moves the rectangle to a plane with the passed coordinates at its center
            '~unpassed or non-numeric values are ignored and the current ordinates are used
            Dim v1 As TVECTOR = _Struc.Origin
            v1.Update(NewX, NewY, NewZ)
            Return Define(v1, _Struc.XDirection, _Struc.YDirection)
        End Function
        Public Function SetDimensions(Optional aWidth As Double? = Nothing, Optional aHeight As Double? = Nothing, Optional aMultiplier As Double = 1) As Boolean
            Dim _rVal As Boolean
            '#1the new width
            '#2the new height
            '#3a scale factor to apply
            '^sets the dimensions of the rectangle and returns True if it changes
            _rVal = _Struc.SetDimensions(aWidth, aHeight, aMultiplier)
            If _rVal Then RaiseEvent RectangleChange(dxxCoordinateSystemEventTypes.Dimensions)
            Return _rVal
        End Function
        Public Function Spin(aAngle As Double, Optional bInRadians As Boolean = False, Optional aAxis As dxeLine = Nothing, Optional bLocal As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its y axis
            If bLocal Then aAxis = YAxis() Else aAxis = g_WCS.YAxis
            _rVal = RotateAbout(aAxis, aAngle, bInRadians, False, True)
            Return _rVal
        End Function
        Friend Function SpinV(aAngle As Double, Optional bInRadians As Boolean = False, Optional aAxis As dxeLine = Nothing, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its y axis
            If bLocal Then aAxis = YAxis() Else aAxis = g_WCS.YAxis(, Center)
            _rVal = RotateAbout(aAxis.StartPtV, aAxis.DirectionV, aAngle, bInRadians, False, True, bSuppressEvnts)
            Return _rVal
        End Function
        Public Sub Stretch(aDist As Double, Optional aStretchWidth As Boolean = True, Optional aStretchHeight As Boolean = True, Optional bMaintainBaseline As Boolean = False, Optional bMaintainOrigin As Boolean = True)
            _Struc.Stretch(aDist, aStretchWidth, aStretchHeight, bMaintainBaseline, bMaintainOrigin)
        End Sub
        Public Function Stretched(aDist As Double, Optional StretchWidth As Boolean = True, Optional StretchHeight As Boolean = True, Optional bMaintainBaseline As Boolean = True, Optional bMaintainOrigin As Boolean = True) As dxfRectangle
            Dim _rVal As dxfRectangle = Clone()
            _rVal.Stretch(aDist, StretchWidth, StretchHeight, bMaintainBaseline, bMaintainOrigin)
            Return _rVal
        End Function
        Public Function Tip(aAngle As Double, Optional bInRadians As Boolean = False, Optional aAxis As dxeLine = Nothing, Optional bLocal As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its x axis
            If bLocal Then aAxis = XAxis() Else aAxis = g_WCS.XAxis(aStartPt:=Center)
            _rVal = RotateAbout(aAxis, aAngle, bInRadians, False, True)
            Return _rVal
        End Function
        Friend Function TipV(aAngle As Double, Optional bInRadians As Boolean = False, Optional aAxis As dxeLine = Nothing, Optional bLocal As Boolean = True, Optional bSuppressEvnts As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '^rotates the plane about its x axis
            If bLocal Then aAxis = XAxis() Else aAxis = g_WCS.XAxis(aStartPt:=Center)
            _rVal = RotateAbout(aAxis.StartPtV, aAxis.DirectionV, aAngle, bInRadians, False, True, bSuppressEvnts)
            Return _rVal
        End Function
        Public Function Translate(aTranslation As iVector, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean
            If aTranslation IsNot Nothing Then _rVal = Translate(New TVECTOR(aTranslation), aPlane)
            Return _rVal
        End Function
        Public Sub Trim(aSide As dxxSides, aLength As Double, Optional bMaintainCenter As Boolean = False)
            '#1the side to trim
            '#2the length to trim
            '#3flag to maintain the current center
            '^used to trim the height or width dimensions of the rectangle
            '~if the side is left or right the trim length is limited to the curent width
            '~if the side is top or bottom the trim length is limited to the curent height
            Dim trm As Double
            Dim atp As dxxCoordinateSystemEventTypes
            trm = Math.Abs(aLength)
            If trm = 0 Then Return
            Select Case aSide
                Case dxxSides.Left, dxxSides.Right
                    If _Struc.Width = 0 Then Return
                    If trm > _Struc.Width Then trm = _Struc.Width
                    atp = dxxCoordinateSystemEventTypes.Dimensions
                    _Struc.Width -= trm
                    If Not bMaintainCenter Then
                        If aSide = dxxSides.Left Then
                            _Struc.Origin += _Struc.XDirection * (0.5 * trm)
                        Else
                            _Struc.Origin += _Struc.XDirection * (-0.5 * trm)
                        End If
                        atp += dxxCoordinateSystemEventTypes.Origin
                    End If
                    RaiseEvent RectangleChange(atp)
                Case dxxSides.Top, dxxSides.Bottom
                    If _Struc.Height = 0 Then Return
                    If trm > _Struc.Height Then trm = _Struc.Height
                    atp = dxxCoordinateSystemEventTypes.Dimensions
                    _Struc.Height -= trm
                    If Not bMaintainCenter Then
                        If aSide = dxxSides.Bottom Then
                            _Struc.Origin += _Struc.YDirection * (0.5 * trm)
                        Else
                            _Struc.Origin += _Struc.YDirection * (-0.5 * trm)
                        End If
                        atp += dxxCoordinateSystemEventTypes.Origin
                    End If
                    RaiseEvent RectangleChange(atp)
            End Select
        End Sub
        Friend Function VecR(aVector As TVECTOR, aDisplacement As TVECTOR) As TVECTOR
            '^used to create a new vector with respect to the origin of the system
            Return _Struc.VectorRelative(aVector, aDisplacement.X, aDisplacement.Y, aDisplacement.Z)
        End Function
        Friend Overridable Function VecV(aX As Double, aY As Double, Optional aZ As Double = 0, Optional aShearXAngle As Double = 0, Optional bWorldOrigin As Boolean = False, Optional aOrigin As dxfVector = Nothing, Optional aVertexType As dxxVertexStyles = dxxVertexStyles.UNDEFINED) As TVECTOR
            '#1the X coordinate for the new vector
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '#4an X shear angle to apply
            '#5flag to return the vector with resplect to 0,0,0
            '#6the origin to use if the world origin flag is false (if not passed or Nothing the system origin is used)
            '^used to create a new vector with respect to the origin of the system
            Return _Struc.Vector(aX, aY, aZ, aShearXAngle, bWorldOrigin, aOrigin, aVertexType)
        End Function
        Public Function Vector(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aOrigin As dxfVector = Nothing, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "") As dxfVector
            '#1the X coordinate for the new vector
            '#2the Y coordinate for the new vector
            '#3the Z coordinate for the new vector
            '^used to create a new vector with respect to the origin of the system
            Dim _rVal As New dxfVector(VecV(aX, aY, aZ, aOrigin:=aOrigin))
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.Tag = aTag
            _rVal.Flag = aFlag
            Return _rVal
        End Function
        Public Function VectorRel(aVectorObj As iVector, Optional aXDisplacement As Double = 0.0, Optional aYDisplacement As Double = 0.0, Optional aZDisplacement As Double = 0.0, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aTag As String = "", Optional aFlag As String = "") As dxfVector
            '^used to create a new vector with respect to the origin of the system
            Dim _rVal As dxfVector = New dxfVector(VecR(New TVECTOR(aVectorObj), New TVECTOR(aXDisplacement, aYDisplacement, aZDisplacement)))
            _rVal.LCLSet(aLayerName, aColor, aLineType)
            _rVal.Tag = aTag
            _rVal.Flag = aFlag
            Return _rVal
        End Function

        Public Function XAxis(Optional aLength As Double = 1, Optional aStartPt As Object = Nothing, Optional aColor As dxxColors = dxxColors.Red, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1the length for the axis
            '#2an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes X direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Return _Struc.Axis(dxxAxisDescriptors.X, aLength, aStartPt, aColor, aXOffset, aYOffset, aZOffset)
        End Function

        Public Function YAxis(Optional aLength As Double = 1, Optional aStartPt As Object = Nothing, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1the length for the axis
            '#2an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes Y direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Return _Struc.Axis(dxxAxisDescriptors.Y, aLength, aStartPt, aXOffset:=aXOffset, aYOffset:=aYOffset, aZOffset:=aZOffset)
        End Function

        Public Function ZAxis(Optional aLength As Double = 1, Optional aStartPt As Object = Nothing, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aZOffset As Double = 0.0) As dxeLine
            '#1the length for the axis
            '#2an optional origin for the axis
            '^returns a line beginning at the planes origin extending in the planes normal (Z) direction
            '~if the passed length is negative then the axis extends in the opposite direction
            Return _Struc.Axis(dxxAxisDescriptors.Z, aLength, aStartPt, aXOffset:=aXOffset, aYOffset:=aYOffset, aZOffset:=aZOffset)
        End Function

        Friend Overridable Function LineV(aXOrdinate As Double, Optional aLength As Double = 0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Return LineV(VecV(aXOrdinate, 0, 0), aLength, aInvert, bByStartPt)
        End Function
        Friend Overridable Function LineV(aVector As TVECTOR, Optional aLength As Double = 0.0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
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

        Friend Overridable Function LineH(aYOrdinate As Double, Optional aLength As Double = 0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
            Return LineH(VecV(0, aYOrdinate, 0), aLength, aInvert, bByStartPt)
        End Function
        Friend Overridable Function LineH(aVector As TVECTOR, Optional aLength As Double = 0.0, Optional aInvert As Boolean = False, Optional bByStartPt As Boolean = False) As TLINE
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
#End Region 'Methods
#Region "Operators"
        Public Shared Widening Operator CType(aRectangle As dxfRectangle) As dxePolygon
            If aRectangle Is Nothing Then Return Nothing
            Return New dxePolygon(aRectangle.Corners(), aRectangle.Center, True, aRectangle.Name, aRectangle.DisplaySettings, aRectangle.Plane)
        End Operator
        Public Shared Widening Operator CType(aRectangle As dxfRectangle) As dxePolyline
            If aRectangle Is Nothing Then Return Nothing
            Return New dxePolyline(aRectangle.Corners(), True, aRectangle.DisplaySettings, aRectangle.Plane)
        End Operator
        Public Shared Operator +(A As dxfRectangle, B As dxfRectangle) As dxfRectangle
            If A Is Nothing And B Is Nothing Then Return Nothing
            If A IsNot Nothing And B Is Nothing Then Return New dxfRectangle(A)
            Dim _rVal As New dxfRectangle(A)
            _rVal.ExpandToRectangle(B)
            Return _rVal
        End Operator
#End Region 'Operators
    End Class 'dxfRectangle
End Namespace
