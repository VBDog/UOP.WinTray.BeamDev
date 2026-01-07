Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoDisplay
        Implements idxfSettingsObject
#Region "Members"
        Private _Struc As TDISPLAY
        Friend ImagePtr As WeakReference
        Private vOldVal As Object
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage)
            If aImage IsNot Nothing Then
                _Struc = aImage.obj_DISPLAY
                _Struc.ImageGUID = aImage.GUID
                ImagePtr = New WeakReference(aImage)
            End If
        End Sub
        Friend Sub New(aStucture As TDISPLAY)
            _Struc = aStucture.Clone
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Properties As dxoProperties Implements idxfSettingsObject.Properties
            Get
                Return New dxoProperties(_Struc.Properties)
            End Get
            Set(value As dxoProperties)
                _Struc.Properties.CopyValues(value)
            End Set
        End Property

        Public ReadOnly Property SettingType As dxxReferenceTypes Implements idxfSettingsObject.SettingType
            Get
                Return dxxReferenceTypes.DISPLAYSETTINGS
            End Get
        End Property
        Public ReadOnly Property Name As String Implements idxfSettingsObject.Name
            Get
                Return dxfEnums.DisplayName(SettingType)
            End Get
        End Property
        Public ReadOnly Property AspectRatio As Double
            Get
                '^returns the aspect ratio of the current bitmap
                '~aspect is Width / Height
                Return _Struc.AspectRatio
            End Get
        End Property
        Public Property Size As System.Drawing.Size
            Get
                Return _Struc.Size
            End Get
            Set(value As System.Drawing.Size)

                If Not value.IsEmpty Then ResizeImage(value.Width, value.Height)
            End Set
        End Property
        '^controls if the image is redrawn when new entities are added or view changes occur
        Public Property AutoRedraw As Boolean
            Get
                Return _Struc.AutoRedraw
            End Get
            Set(value As Boolean)
                vOldVal = _Struc.AutoRedraw
                If vOldVal <> value Then
                    _Struc.AutoRedraw = value
                    _Struc.UpdateImage(Nothing, TPROPERTIES.OneProp("AutoRedraw", value, vOldVal), value)
                End If
            End Set
        End Property
        Public Property BackgroundColor As Color
            Get
                '^the curent background color of the image
                Return _Struc.BackGroundColor
            End Get
            Set(value As Color)
                '^the curent background color of the image

                If value <> _Struc.BackGroundColor Then
                    UpdateStructure(_Struc.pln_VIEW, "BackgroundColor", False, aBackColor:=value)
                End If

            End Set
        End Property
        Public Property BackColor As dxxColors
            Get
                '^the curent background color of the image
                Return _Struc.BackColor
            End Get
            Set(value As dxxColors)
                '^the curent background color of the image
                If Not value.IsLogical() Then
                    UpdateStructure(_Struc.pln_VIEW, "BackColor", False, aBackColor:=value)
                End If
            End Set
        End Property
        Public WriteOnly Property BackGround As dxfBitmap
            Set(value As dxfBitmap)
                Dim aImage As dxfImage = Image
                If aImage Is Nothing Then Return
                aImage.Background = value
            End Set
        End Property
        Public Property ColorMode As dxxColorModes
            Get
                '^controls how the current image is drawn to the current output device
                Return _Struc.ColorMode
            End Get
            Set(value As dxxColorModes)
                '^controls how the current image is drawn to the current output device
                vOldVal = _Struc.ColorMode
                If value >= 0 And value <= 2 Then
                    If vOldVal <> value Then
                        _Struc.ColorMode = value
                        _Struc.UpdateImage(Nothing, TPROPERTIES.OneProp("ColorMode", value, vOldVal), True)
                    End If
                End If
            End Set
        End Property
        Public ReadOnly Property DeviceHeight As Double
            Get
                '^the height of the current device window in pixels
                Return _Struc.Height
            End Get
        End Property
        Public ReadOnly Property DeviceHwnd As IntPtr
            Get
                '^the window handle of the current output device
                Return _Struc.DeviceHwnd
            End Get
        End Property
        Public ReadOnly Property DevicePlane As dxfPlane
            Get
                '^returns a plane which describes the current bitmaps dimensions
                Return _Struc.pln_DEVICE.ToPlane
            End Get
        End Property
        Public ReadOnly Property DeviceRectangle As dxfRectangle
            Get
                '^returns a rectangle which describes the current bitmaps dimensions
                Return _Struc.pln_DEVICE.ToRectangle
            End Get
        End Property
        Public ReadOnly Property DeviceWidth As Double
            Get
                '^the width of the current device window in pixels
                Return _Struc.pln_DEVICE.Width
            End Get
        End Property
        Public ReadOnly Property Extent_LL As dxfVector
            Get
                '^returns the lower left corner of the current extents rectangle
                '~the extent rectangle may or may not be up to date if the
                '~drawing has not been redrawn after a change to the current entities or layers
                Return New dxfVector(_Struc.rec_EXTENTS.Point(dxxRectanglePts.BottomLeft))
            End Get
        End Property
        Public ReadOnly Property Extent_LR As dxfVector
            Get
                '^returns the lower right corner of the current extents rectangle
                '~the extent rectangle may or may not be up to date if the
                '~drawing has not been redrawn after a change to the current entities or layers
                Return New dxfVector(_Struc.rec_EXTENTS.Point(dxxRectanglePts.BottomRight))
            End Get
        End Property
        Public ReadOnly Property Extent_UL As dxfVector
            Get
                '^returns the upper left corner of the current extents rectangle
                Return CType(_Struc.rec_EXTENTS.Point(dxxRectanglePts.TopLeft), dxfVector)
            End Get
        End Property
        Public ReadOnly Property Extent_UR As dxfVector
            Get
                '^returns the upper right corner of the current extents rectangle
                '~the extent rectangle may or may not be up to date if the
                '~drawing has not been redrawn after a change to the current entities or layers
                Return CType(_Struc.rec_EXTENTS.Point(dxxRectanglePts.TopRight), dxfVector)
            End Get
        End Property
        Public ReadOnly Property ExtentCenter As dxfVector
            Get
                '^returns the center of the current extents rectangle
                '~the extent rectangle may or may not be up to date if the
                '~drawing has not been redrawn after a change to the current entities or layers
                Return New dxfVector(_Struc.rec_EXTENTS.Origin)
            End Get
        End Property
        Public ReadOnly Property ExtentRectangle As dxfRectangle
            Get
                '^the rectangle which lies on the view plane and encloses all the visible entities
                '~the extent rectangle may or may not be up to date if the
                '~drawing has not been redrawn after a change to the current entities or layers
                Return New dxfRectangle(_Struc.rec_EXTENTS)
            End Get
        End Property
        Friend ReadOnly Property Image As dxfImage
            Get
                If Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then
                    Dim myimage As dxfImage = Nothing
                    If ImagePtr IsNot Nothing Then
                        If ImagePtr.IsAlive Then
                            myimage = TryCast(ImagePtr.Target, dxfImage)
                            If myimage IsNot Nothing Then
                                Return myimage
                            End If
                        End If
                    End If
                    myimage = dxfEvents.GetImage(_Struc.ImageGUID)
                    If myimage IsNot Nothing Then
                        ImagePtr = New WeakReference(myimage)
                    End If
                    Return myimage
                Else
                    Return Nothing
                End If
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
        Public Property IsDirty As Boolean
            Get
                Return _Struc.IsDirty
            End Get
            Set(value As Boolean)
                _Struc.IsDirty = value
            End Set
        End Property
        Public Property PaperScale As Double
            Get
                Return _Struc.PaperScale
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                vOldVal = _Struc.PaperScale
                If vOldVal <> value Then
                    _Struc.PaperScale = value
                    _Struc.UpdateImage(Nothing, TPROPERTIES.OneProp("PaperScale", value, vOldVal))
                End If
            End Set
        End Property
        Public ReadOnly Property Pitch As Double
            Get
                '^the angle between the y axis and the global y axis
                '~rotation about gobal Y axis
                Return _Struc.pln_VIEW.Pitch
            End Get
        End Property
        Public ReadOnly Property Roll As Double
            Get
                '^the angle between the x axis and the global x axis
                '~rotation about gobal X axis
                Return _Struc.pln_VIEW.Roll
            End Get
        End Property
        Public ReadOnly Property Rotation As Double
            Get
                '^the angle between the z axis and the global z axis
                '~rotation about gobal Z axis
                Return _Struc.pln_VIEW.Yaw
            End Get
        End Property
        Friend Property Strukture As TDISPLAY
            Get
                Return _Struc
            End Get
            Set(value As TDISPLAY)
                _Struc = value
            End Set
        End Property
        Public ReadOnly Property ToInches As Double
            Get
                '^returns the factor to multiply a value of the current units to convert it to inches
                Return dxfUtils.FactorFromTo(Units, dxxDeviceUnits.Inches, PixelsPerInch)
            End Get
        End Property
        Public ReadOnly Property ToPixels As Double
            Get
                '^returns the factor to multiply a value of the current units to convert it to pixels
                Return _Struc.PixelsPerUnit
            End Get
        End Property
        Public ReadOnly Property UnitName As String
            Get
                '^the name of the units assigned to the current image plane
                Return dxfEnums.Description(_Struc.Units)
            End Get
        End Property
        Public Property Units As dxxDeviceUnits
            Get
                Return _Struc.Units
            End Get
            Set(value As dxxDeviceUnits)
                If value < 1 Or value > 7 Then Return
                If _Struc.Units <> value Then
                    UpdateStructure(_Struc.pln_VIEW, "Units", False, aUnits:=value)
                End If
            End Set
        End Property
        Public ReadOnly Property View_LL As dxfVector
            Get
                Return ViewRectangle.BottomLeft
            End Get
        End Property
        Public ReadOnly Property View_LR As dxfVector
            Get
                Return ViewRectangle.BottomRight
            End Get
        End Property
        Public ReadOnly Property View_UL As dxfVector
            Get
                Return ViewRectangle.TopLeft
            End Get
        End Property
        Public ReadOnly Property View_UR As dxfVector
            Get
                Return ViewRectangle.TopRight
            End Get
        End Property
        Public ReadOnly Property ViewAngles As dxfVector
            Get
                '^returns the Euler Angles of the plane
                Return New dxfVector(_Struc.pln_VIEW.EulerAngles)
            End Get
        End Property
        Public Property ViewCenter As dxfVector
            Get
                Return CType(_Struc.pln_VIEW.Origin, dxfVector) ', Viewport, World))
            End Get
            Set(value As dxfVector)
                Dim aPln As TPLANE = _Struc.pln_VIEW
                If aPln.Origin.Update(value.X, value.Y, value.Z) Then
                    UpdateStructure(aPln, "ViewCenter")
                End If
            End Set
        End Property
        Public Property ViewDirection As dxfDirection
            Get
                Return New dxfDirection(_Struc.pln_VIEW.ZDirection)
            End Get
            Set(value As dxfDirection)
                If value Is Nothing Then Return
                Dim aPln As TPLANE = _Struc.pln_VIEW
                aPln.AlignTo(value.Strukture * -1, dxxAxisDescriptors.Z)
                UpdateStructure(aPln, "ViewDirection")
            End Set
        End Property
        Public ReadOnly Property ViewHeight As Double
            Get
                Return _Struc.pln_VIEW.Height
            End Get
        End Property
        Public Property ViewPlane As dxfPlane
            Get
                '^the plane which describes the directions, focal point and dimensions of the display
                Return _Struc.pln_VIEW.ToPlane
            End Get
            Set(value As dxfPlane)
                '^the plane which describes the directions, focal point and dimensions of the display
                If value Is Nothing Then Return
                Dim vPln As TPLANE = _Struc.pln_VIEW.Clone
                vPln.Define(value.OriginV, value.XDirectionV, value.YDirectionV)
                UpdateStructure(vPln, "ViewPlane")
            End Set
        End Property
        Public ReadOnly Property ViewRectangle As dxfRectangle
            Get
                '^the rectangle which describes the directions, focal point and dimensions of the display
                Return _Struc.pln_VIEW.ToRectangle
            End Get
        End Property
        Public ReadOnly Property ViewWidth As Double
            Get
                Return _Struc.pln_VIEW.Width
            End Get
        End Property
        Public Property ZoomDiagonal As dxeLine
            Get
                '^the line from the lower left corner of the current view plane to then upper right corner of the current view plane expressed in device coordinates
                Return New dxeLine(_Struc.WorldToDevice(_Struc.pln_VIEW.Point(dxxRectanglePts.BottomLeft)), _Struc.WorldToDevice(_Struc.pln_VIEW.Point(dxxRectanglePts.TopRight)))
            End Get
            Set(value As dxeLine)
                '^the line from the lower left corner of the current view plane to then upper right corner of the current view plane expressed in device coordinates (pixels)
                If value Is Nothing Then Return
                Dim vPlane As New TPLANE("")
                Dim ZF As Double
                Dim LL As New TVECTOR(value.StartPt.X, value.StartPt.Y)
                Dim UR As New TVECTOR(value.EndPt.X, value.EndPt.Y)
                Dim org As TVECTOR
                vPlane = _Struc.pln_VIEW
                If LL.DistanceTo(UR) <= 0 Then Return
                org = _Struc.DeviceToView(LL.Interpolate(UR, 0.5))
                LL = _Struc.DeviceToView(LL)
                UR = _Struc.DeviceToView(UR)
                vPlane.Origin = org
                vPlane.Width = Math.Abs(LL.X - UR.X)
                vPlane.Height = Math.Abs(UR.Y - LL.Y)
                ZF = _Struc.FactorToView(vPlane)
                UpdateStructure(vPlane, "ZoomDiagonal", True, aZoomFactor:=ZF)
            End Set
        End Property
        Public Property ZoomFactor As Double
            Get
                Return _Struc.ZoomFactor
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                'If Image.UsingDxfViewer Then
                '    Image.RaiseZoomEvent(False, (_Struc.ZoomFactor - value) / _Struc.ZoomFactor)
                'End If
                If _Struc.ZoomFactor <> value Then
                    UpdateStructure(_Struc.pln_VIEW, "ZoomFactor", aZoomFactor:=value)
                End If
            End Set
        End Property
        Public ReadOnly Property ZoomRectangle As dxfRectangle
            Get
                '^returns the rectangle of the current view scaled by the current zoom factor
                Return _Struc.pln_VIEW.ToRectangle(_Struc.ZoomFactor)
            End Get
        End Property
        Public Property LimitsRectangle As dxfRectangle
            Get
                '^returns the rectangle of the current limits rectangle
                If HasLimits Then
                    Return _Struc.pln_LIMITS.ToRectangle()
                Else
                    Return Nothing
                End If
            End Get
            Set(value As dxfRectangle)
                _Struc.pln_LIMITS = New TPLANE(value)
            End Set
        End Property
        Public ReadOnly Property HasLimits As Boolean
            Get
                Return _Struc.HasLimits
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function SetLimits(aLowerLeft As iVector, aUpperRight As iVector) As Boolean
            Dim _rVal As Boolean = _Struc.SetLimits(New TVECTOR(aLowerLeft), New TVECTOR(aUpperRight))
            If _rVal Then
                Dim myImage As dxfImage = Image
                If myImage IsNot Nothing Then myImage.obj_DISPLAY = _Struc
            End If
            Return _rVal
        End Function
        Public Function SetLimits(aRectangle As dxfRectangle) As Boolean
            If aRectangle Is Nothing Then Return False
            Return SetLimits(aRectangle.BottomLeft, aRectangle.TopRight)
        End Function
        Public Sub SetAspectRatio(aSize As System.Drawing.Size?)
            If aSize Is Nothing Then Return
            If Not aSize.HasValue Then Return

            If aSize.Value.Width <= 0 Or aSize.Value.Height <= 0 Then Return
            SetAspectRatio(aSize.Value.Width / aSize.Value.Height)
        End Sub
        Public Sub SetAspectRatio(aRatio As Double)
            If aRatio <= 0 Then Return
            If aRatio = Double.NaN Then
                Return
            End If
            Dim devPlane As TPLANE = _Struc.pln_DEVICE
            Dim aspct As Double = devPlane.AspectRatio(False)
            If aspct <= 0 Or aspct = aRatio Then Return
            Dim wd As Double = devPlane.Width
            Dim ht As Double = devPlane.Height
            If aspct < 1 And aRatio < 1 Then
                wd = aRatio * ht
            ElseIf aspct < 1 And aRatio > 1 Then
                ht = aRatio / wd
            ElseIf aspct > 1 And aRatio > 1 Then
                ht = aRatio / wd
            Else
                wd = aRatio * ht
            End If
            If (wd = Double.NaN Or ht = Double.NaN) Then
                Return
            End If
            ResizeImage(Convert.ToInt32(wd), Convert.ToInt32(ht))
        End Sub
        Public Function BlackOrWhite(Optional aBackgroundColor As Color? = Nothing) As Integer
            Dim _rVal As Long = 0
            '#1a Background color to use instead of the current background color
            '^returns 16777215 'vbWhite (16777215) if the background is very dark or vbBlack (0) if it is not
            Dim bColor As COLOR_HSB
            If aBackgroundColor.HasValue Then
                bColor = dxfColors.Win64ToHSB(aBackgroundColor.Value)
            Else
                bColor = dxfColors.Win64ToHSB(BackgroundColor)
            End If
            If bColor.B < dxfGlobals.BlackWhiteBrightness Then
                Return 16777215 'vbWhite
            Else
                Return 0 'vbBlack
            End If
        End Function
        Friend Function Clone() As dxoDisplay
            '^returns an exact copy of the object and it's current properties
            Return New dxoDisplay(_Struc)
        End Function
        Public Function ConvertUnits(aValue As Object, aFromUnits As dxxDeviceUnits, aToUnits As dxxDeviceUnits) As Object
            Dim _rVal As Object = aValue
            '#1the value to convert
            '#2the units of the passed value
            '#3the units to convert the value to
            '^returns the value converted from the current display units to the requested unit
            'On Error Resume Next
            If TVALUES.IsNumber(aValue) Then _rVal = aValue * dxfUtils.FactorFromTo(aFromUnits, aToUnits, PixelsPerInch)
            Return _rVal
        End Function
        Public Function ConvertVector(aVector As dxfVector, aFromDomain As dxxDomains, aToDomain As dxxDomains) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the vector to convert
            '#2the domain that the passed vector is currently in
            '#3the domain to convert the ector to
            '^converts the passed vector from its current domain to the requested domain
            If aVector Is Nothing Then Return _rVal
            _rVal = aVector.Clone
            _rVal.Strukture = _Struc.ConvertVector(aVector.Strukture, aFromDomain, aToDomain)
            Return _rVal
        End Function
        Public Function DeviceVector(Optional aWorldX As Double? = Nothing, Optional aWorldY As Double? = Nothing, Optional aWorldZ As Double? = Nothing) As dxfVector
            '#1the world X ordinate
            '#1the world Y ordinate
            '#1the world Z ordinate
            '^returns a device vector based on the passed worl coordinates
            Dim x As Double = 0
            Dim y As Double = 0
            Dim z As Double = 0
            If aWorldX.HasValue Then x = aWorldX.Value
            If aWorldY.HasValue Then y = aWorldY.Value
            If aWorldZ.HasValue Then z = aWorldZ.Value
            Return New dxfVector(_Struc.ConvertVector(New TVECTOR(x, y, z), dxxDomains.World, dxxDomains.Device))
        End Function
        Public Function WorldVector(Optional aDeviceX As Double? = Nothing, Optional aDeviceY As Double? = Nothing, Optional aDeviceZ As Double? = Nothing) As dxfVector
            '#1the device X ordinate
            '#1the device Y ordinate
            '#1the device Z ordinate
            '^returns a world vector based on the passed device coordinates (Pixels)
            Dim x As Double = 0
            Dim y As Double = 0
            Dim z As Double = 0
            If aDeviceX.HasValue Then x = aDeviceX.Value
            If aDeviceY.HasValue Then y = aDeviceY.Value
            If aDeviceZ.HasValue Then z = aDeviceZ.Value
            Return New dxfVector(_Struc.ConvertVector(New TVECTOR(x, y, z), dxxDomains.Device, dxxDomains.World))
        End Function
        Friend Function DeviceIsDefined() As Boolean
            Return _Struc.DeviceIsDefined()
        End Function
        Public Sub DeviceToUCS(ByRef rXPixels As Double, ByRef rYPixels As Double, ByRef rZPixels As Double, Optional aPrecis As Integer = -1)
            '#1the X coordinate to convert (in pixels)
            '#2the Y coordinate to convert (in pixels)
            '#3the Z coordinate to convert (in pixels)
            '^converts the passed device coordinates to world space in the current view units
            '~the arguments are assumed to be with reference to the current devices coordinate system in pixels!
            Dim v1 As TVECTOR = _Struc.DeviceToUCS(New TVECTOR(rXPixels, rYPixels, 0))
            If aPrecis < 0 Then
                rXPixels = v1.X
                rYPixels = v1.Y
                rZPixels = v1.Z
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                rXPixels = Math.Round(v1.X, aPrecis)
                rYPixels = Math.Round(v1.X, aPrecis)
                rZPixels = Math.Round(v1.X, aPrecis)
            End If
        End Sub
        Public Sub DeviceToView(ByRef rXPixels As Double, ByRef rYPixels As Double, ByRef rZPixels As Double, Optional aPrecis As Integer = -1)
            '#1the X coordinate to convert (in pixels)
            '#2the Y coordinate to convert (in pixels)
            '#3the Z coordinate to convert (in pixels)
            '#4flag to convert to the current ucs
            '^converts the passed device coordinates to view plane space in the current view units
            '~the arguments are assumed to be with reference to the current devices coordinate system in pixels!
            Dim v1 As TVECTOR = _Struc.DeviceToView(New TVECTOR(rXPixels, rYPixels, 0))
            If aPrecis < 0 Then
                rXPixels = v1.X
                rYPixels = v1.Y
                rZPixels = v1.Z
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                rXPixels = Math.Round(v1.X, aPrecis)
                rYPixels = Math.Round(v1.X, aPrecis)
                rZPixels = Math.Round(v1.X, aPrecis)
            End If
        End Sub
        Public Sub DeviceToWorld(ByRef rXPixels As Double, ByRef rYPixels As Double, ByRef rZPixels As Double, Optional aPrecis As Integer = -1)
            '#1the X coordinate to convert (in pixels)
            '#2the Y coordinate to convert (in pixels)
            '#3the Z coordinate to convert (in pixels)
            '^converts the passed device coordinates to world space in the current view units
            '~the arguments are assumed to be with reference to the current devices coordinate system in pixels!
            Dim v1 As TVECTOR = _Struc.DeviceToWorld(New TVECTOR(rXPixels, rYPixels, 0))
            If aPrecis < 0 Then
                rXPixels = v1.X
                rYPixels = v1.Y
                rZPixels = v1.Z
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                rXPixels = Math.Round(v1.X, aPrecis)
                rYPixels = Math.Round(v1.X, aPrecis)
                rZPixels = Math.Round(v1.X, aPrecis)
            End If
        End Sub
        Public Function DrawBitmapToDevice(Optional aHwnd As Long = 0, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft) As Boolean
            Dim aImage As dxfImage = Image
            If aImage Is Nothing Then Return False
            Dim rBitmap As dxfBitmap = aImage.GetBitmap(False)
            '#1the destination device context. 0 means to draw to the current output device defined by Display.SetDevice
            '#2the bitmap to draw. Nothing means to draw the current image
            '#3the alignment to apply
            '^draw the current or passed bitmap to the to the destination device context
            Return rBitmap.DrawToDevice(aHwnd)
        End Function
        Public Function DrawToDevice(aDevice As Object, Optional bZoomExtents As Boolean = False, Optional aZoomScale As Double = 0.0, Optional aZoomBuffer As Double = 0.0, Optional aBackColor As Integer = -1, Optional bSuppressNoPlotLayers As Boolean = False, Optional bSuppressScreenEnts As Boolean = True) As dxfBitmap
            Dim _rVal As dxfBitmap = Nothing
            '#1the device to draw the current image to
            '#2the icon mode to apply
            '#3flag to show line weights
            '#4a color mode to apply
            '#5flag to zoom the image to the extents of the device
            '#6a zoom factor to use
            '#7a line weight scale to apply
            '#8a zoom buffer factor to apply
            '#9a background color to use
            '^redraws the current entities to the passed device context
            'On Error Resume Next
            Dim aImage As dxfImage = Image
            If aImage IsNot Nothing Then
                aImage.RenderToDevice(aDevice, aImage.obj_DISPLAY.pln_VIEW, dxxUCSIconModes.None, aImage.Header.LineWeightDisplay, dxxColorModes.ByImage, bZoomExtents, aZoomScale, aImage.Header.LineWeightScale, aZoomBuffer, aBackColor, _rVal, bSuppressNoPlotLayers, bSuppressScreenEnts)
            End If
            Return _rVal
        End Function
        Public Function Erasum(aEntities As colDXFEntities, Optional aHandles As List(Of String) = Nothing, Optional bRemoveFromImage As Boolean = True, Optional aTagString As String = "", Optional aDelimiter As String = ",") As Boolean
            Dim _rVal As Boolean = False
            '#1the entities to Erase
            '#2a collection of string handles to erase
            '#3flag to remove the entities form the image if they are members
            '#4if passed any image entities with matching tags will be erased
            '#5the delimiter for the tag string
            '^draws over the passed entities with the current background color. If
            '^handles are passed then the entities are located fom the current image and erased and
            'On Error Resume Next
            Dim aImage As dxfImage = Nothing
            aImage = Image
            If aImage IsNot Nothing Then
                _rVal = dxfImageTool.Entities_Erase(aImage, aEntities, aHandles, bRemoveFromImage, aTagString, aDelimiter)
            End If
            Return _rVal
        End Function
        Public Function Pan(Optional aPanFractionX As Double = 0.0, Optional aPanFractionY As Double = 0.0) As Boolean
            Dim _rVal As Boolean = False
            '#1the fraction of the device width to pan in the x direction
            '#2the fraction of the device width to pan in the y direction
            '^pans the camara as a fraction of the current device dimension
            If aPanFractionX = 0 And aPanFractionY = 0 Then Return _rVal
            Dim dRec As TPLANE = _Struc.pln_VIEW
            If aPanFractionX <> 0 Or aPanFractionY <> 0 Then
                dRec.Origin += New TVECTOR(aPanFractionX * dRec.Width, aPanFractionY * dRec.Height, 0)
                _rVal = UpdateStructure(dRec, "Pan")
            End If
            Return _rVal
        End Function
        Public Function PixelsPerInch(Optional bIncludeZoom As Boolean = False) As Double
            Dim _rVal As Double = _Struc.DPI
            If bIncludeZoom Then _rVal *= ZoomFactor
            Return _rVal
        End Function
        Public Sub Redraw()
            Dim aImage As dxfImage = Image
            If aImage IsNot Nothing Then aImage.Render()
        End Sub
        Public Sub Refresh(Optional bOutputDevice As Boolean = False)
            Dim aImage As dxfImage = Image
            If aImage Is Nothing Then Return
            If Not aImage.IsDirty Or bOutputDevice Then
                If _Struc.DeviceHwnd = 0 Then Return
                Dim aRec As New Vanara.PInvoke.RECT
                APIWrapper.GetClientRect(_Struc.DeviceHwnd, aRec)
                APIWrapper.InvalidateRect(_Struc.DeviceHwnd, aRec, 1)
            Else
                aImage.Render()
            End If
        End Sub
        Public Sub Regen()
            Dim aImage As dxfImage = Image
            If aImage IsNot Nothing Then aImage.Render(, True)
            If aImage.UsingDxfViewer Then
                aImage.RaiseViewRegenerateEvent()
            End If
        End Sub
        Public Function Reset(Optional bResetFocalPoint As Boolean = True, Optional bResetDirections As Boolean = True, Optional aZoomFactor As Double = 0.0, Optional aFocalPoint As iVector = Nothing) As Boolean
            Dim _rVal As Boolean = False
            Try
                Dim fctr As Double = 1
                Dim vPlane As TPLANE = _Struc.pln_VIEW
                If aZoomFactor > 0 And ZoomFactor <> 0 Then fctr = ZoomFactor / aZoomFactor
                If bResetFocalPoint Or bResetDirections Or fctr <> 1 Then
                    If bResetFocalPoint Then
                        vPlane.Origin = New TVECTOR
                    Else
                        If aFocalPoint IsNot Nothing Then vPlane.Origin = New TVECTOR(aFocalPoint)
                    End If
                    If bResetDirections Then
                        vPlane.Define(vPlane.Origin, TVECTOR.WorldX, TVECTOR.WorldY)
                    End If
                    If fctr = 1 Then fctr = ZoomFactor
                    _rVal = UpdateStructure(vPlane, "Reset", aZoomFactor:=fctr)
                End If
                Return _rVal
            Catch ex As Exception
                Dim aImage As dxfImage = Image
                If aImage IsNot Nothing Then aImage.HandleError("Reset", "Display", ex.Message.ToString)
            End Try
            Return _rVal
        End Function
        Public Function ResizeImage(aWidth As Integer, aHeight As Integer, Optional aImage As dxfImage = Nothing) As Boolean
            Try
                If aImage Is Nothing Then aImage = Image
                If aImage Is Nothing Then Return False
                Dim _rVal As Boolean = Not _Struc.Resize(aImage, aWidth, aHeight)
                aImage.obj_DISPLAY = _Struc
                Return _rVal
            Catch ex As Exception
                aImage.HandleError("ResizeImage", "Display", ex.Message.ToString)
                Return False
            End Try
        End Function
        Public Function ResetViewDirections(Optional aNewViewCenter As iVector = Nothing) As Boolean
            Return Reset(aNewViewCenter IsNot Nothing, True, aFocalPoint:=aNewViewCenter)
        End Function
        Public Function Rotate(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = False) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Rotate(aAngle, bInRadians, bLocal, rAxis)
        End Function
        Public Function Rotate(aAngle As Double, bInRadians As Boolean, bLocal As Boolean, ByRef rAxis As dxeLine) As Boolean
            Dim _rVal As Boolean = False
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '#3flag to use the global axis or the current viewport axis
            '^rotates the image about its z axis
            Dim vPln As TPLANE = _Struc.pln_VIEW
            rAxis = New dxeLine With {
                .StartPtV = vPln.Origin
            }
            If bLocal Then
                rAxis.EndPtV = vPln.Origin + vPln.ZDirection
            Else
                rAxis.EndPtV = vPln.Origin + TVECTOR.WorldZ
            End If
            vPln.RotateAbout(vPln.Origin, rAxis.DirectionV, aAngle, bInRadians, False, True)
            _rVal = UpdateStructure(vPln, "Rotate")
            If Image.UsingDxfViewer Then
                Image.RaiseViweRotateEvent(aAngle)
            End If
            Return _rVal
        End Function
        Public Function ScaleFactor(Optional aFromUnits As dxxDeviceUnits = dxxDeviceUnits.Undefined, Optional aToUnits As dxxDeviceUnits = dxxDeviceUnits.Undefined) As Double
            Dim _rVal As Double = 0.0
            Dim aToPixs As Double
            Dim aFromPixs As Double
            If aFromUnits = dxxDeviceUnits.Undefined Then aFromUnits = Units
            If aToUnits = dxxDeviceUnits.Undefined Then aToUnits = Units
            aToPixs = dxfUtils.FactorToPixels(aFromUnits)
            aFromPixs = 1 / dxfUtils.FactorToPixels(aToUnits)
            _rVal = aToPixs * aFromPixs
            Return _rVal
        End Function
        Public Function SetDevice(aDevice As Object, Optional aBackColor As Integer = -1, Optional aViewRectangle As dxfRectangle = Nothing, Optional aBackGround As dxfBitmap = Nothing, Optional bNoRedraw As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            Dim sErr As String = String.Empty
            Try
                _rVal = _Struc.SetDevice(aDevice, aBackColor, False, Nothing, aBackGround, "", bNoRedraw, sErr)
                If Not _rVal Then
                    If sErr <> "" Then Throw New Exception(sErr)
                End If
                If aViewRectangle IsNot Nothing Then
                    SetDisplayRectangle(aViewRectangle, bNoRedraw:=bNoRedraw)
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return False
            End Try
        End Function
        Public Function SetDisplayRectangle(aRectangle As dxfRectangle, Optional aScaleFactor As Double? = Nothing, Optional bSetFeatureScales As Boolean = False, Optional bNoRedraw As Boolean = False, Optional bSaveAsLimits As Boolean = False) As Boolean
            '#1the rectangle describing the desired zoom rectangle
            '#2a scale factor to apply to the desired zoom rectangle
            '^used to set the scale width and zoom center of the image
            '~moves the zoom center to the center of the rectangle and zooms the image to show the height and width of the recangle.
            If aRectangle Is Nothing Then Return False
            Dim aRec As TPLANE = aRectangle.Strukture.Clone
            If aScaleFactor IsNot Nothing Then
                If aScaleFactor.HasValue Then
                    Dim scl As Double = aScaleFactor.Value
                    If scl > 0 And scl <> 1 Then aRec.ScaleDimensions(scl, scl)
                End If

            End If
            Dim _rVal As Boolean = _Struc.SetDisplayWindow(Nothing, aRec.Width, aRectangle.Center, aRec.Height, bSetFeatureScales, bNoRedraw)
            If bSaveAsLimits Then SetLimits(New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.BottomLeft)), New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.TopLeft)))
            Return _rVal
        End Function
        Public Function SetDisplayWindow(aRectangle As dxfRectangle, Optional bSetFeatureScales As Boolean = False, Optional bNoRedraw As Boolean = False, Optional bSaveAsLimits As Boolean = False) As Boolean
            '[]Me~Controling the Display~HelpDocs\DisplayControl.htm~file
            '#1the display rectangle
            '^used to set the scale width and zoom center of the image
            '~moves the zoom center to the passed center and zooms the image to show the requested width.
            '~the height is dependent on the image's aspect ratio. if  the display height is passed the
            '~displayed width will be increased to show at least the requested height
            If aRectangle Is Nothing Then Return False
            Dim _rVal As Boolean = _Struc.SetDisplayWindow(Nothing, aRectangle.Width, aRectangle.Center, aRectangle.Height, bSetFeatureScales, bNoRedraw)
            If bSaveAsLimits Then SetLimits(New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.BottomLeft)), New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.TopLeft)))
            Return _rVal
        End Function
        Public Function SetDisplayWindow(aDisplayWidth As Double, Optional ZCenter As iVector = Nothing, Optional aDisplayHeight As Double = 0.0, Optional bSetFeatureScales As Boolean = False, Optional bNoRedraw As Boolean = False, Optional bSaveAsLimits As Boolean = False) As Boolean
            '[]Me~Controling the Display~HelpDocs\DisplayControl.htm~file
            '#1the display width to set for the image
            '#2the point to center in the display
            '#3the minimum height to display
            '^used to set the scale width and zoom center of the image
            '~moves the zoom center to the passed center and zooms the image to show the requested width.
            '~the height is dependent on the image's aspect ratio. if  the display height is passed the
            '~displayed width will be increased to show at least the requested height
            Dim _rVal As Boolean = _Struc.SetDisplayWindow(Nothing, aDisplayWidth, ZCenter, aDisplayHeight, bSetFeatureScales, bNoRedraw)
            If bSaveAsLimits Then SetLimits(New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.BottomLeft)), New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.TopLeft)))
            Return _rVal
        End Function
        Public Sub SetFeatureScales(aPaperScale As Double, Optional bSetDimStyle As Boolean = True, Optional bSetTextHeight As Boolean = True)
            '#1the feature scale to apply
            '#2flag to pass the feature scale to all the current dim styles
            '#3flag to pass the feature scale to all the current text styles
            '^assigns the passed feature scale to all the scaled settings objects
            Dim aImage As dxfImage = Image
            If aImage IsNot Nothing Then aImage.SetFeatureScales(aPaperScale, bSetDimStyle, bSetTextHeight)
        End Sub
        Public Function ShowInfoWindow(aOwnerForm As Object) As Boolean
            Dim _rVal As Boolean = False
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            'Dim aFrm As frmDisplayInfo
            'aFrm = New frmDisplayInfo
            'aFrm.ShowDisplayInfo(aOwnerForm, Image)
            'Unload(aFrm)
            'aFrm = Nothing
            Return _rVal
        End Function
        Public Function Spin(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = False) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Spin(aAngle, bInRadians, bLocal, rAxis)
        End Function
        Public Function Spin(aAngle As Double, bInRadians As Boolean, bLocal As Boolean, ByRef rAxis As dxeLine) As Boolean
            '#1the angle to spin
            '#2flag indicating if the passed angle is in radians
            '#3flag to use the global axis or the current viewport axis
            '^rotates the image about its y axis
            rAxis = New dxeLine
            Dim vPln As TPLANE = _Struc.pln_VIEW
            rAxis.StartPtV = vPln.Origin
            If bLocal Then
                rAxis.EndPtV = vPln.Origin + vPln.YDirection
            Else
                rAxis.EndPtV = vPln.Origin + TVECTOR.WorldY
            End If
            vPln.RotateAbout(vPln.Origin, rAxis.DirectionV, aAngle, bInRadians, False, True, True)
            Return UpdateStructure(vPln, "Spin")
        End Function
        Public Function Tip(aAngle As Double, Optional bInRadians As Boolean = False, Optional bLocal As Boolean = False) As Boolean
            Dim rAxis As dxeLine = Nothing
            Return Tip(aAngle, bInRadians, bLocal, rAxis)
        End Function
        Public Function Tip(aAngle As Double, bInRadians As Boolean, bLocal As Boolean, ByRef rAxis As dxeLine) As Boolean
            '#1the angle to tip
            '#2flag indicating if the passed angle is in radians
            '#3flag to use the global axis or the current local viewport axis
            '^rotates the image about its x axis
            rAxis = New dxeLine
            Dim vPln As TPLANE = _Struc.pln_VIEW.Clone
            rAxis.StartPtV = vPln.Origin
            If bLocal Then
                rAxis.EndPtV = vPln.Origin + vPln.XDirection
            Else
                rAxis.EndPtV = vPln.Origin + TVECTOR.WorldX
            End If
            vPln.RotateAbout(vPln.Origin, rAxis.DirectionV, aAngle, bInRadians, False, True, True)
            Return UpdateStructure(vPln, "Tip")
        End Function
        Public Function Transform_DeviceToView(aVector As dxfVector) As dxfVector
            Dim _rVal As New dxfVector
            '#1the vector to transform (in pixels)
            '^returns the vector converted fron pixels to a vector on the current view plane
            If aVector IsNot Nothing Then
                _rVal.Strukture = _Struc.DeviceToView(aVector.Strukture)
            End If
            Return _rVal
        End Function
        Public Function Transform_WorldToDevice(aVector As dxfVector) As dxfVector
            Dim _rVal As New dxfVector
            '#1the vector to transform
            '#2returns the vector with respect to the current view plane in current units
            '^returns the passed vector converted from world units to those of the current device (in pixels)
            If aVector IsNot Nothing Then
                _rVal.Strukture = _Struc.WorldToDevice(aVector.Strukture)
            End If
            Return _rVal
        End Function
        Public Function Transform_WorldToDevice(aVector As dxfVector, aViewVector As dxfVector) As dxfVector
            Dim _rVal As New dxfVector
            '#1the vector to transform
            '#2returns the vector with respect to the current view plane in current units
            '^returns the passed vector converted from world units to those of the current device (in pixels)
            If aVector IsNot Nothing Then
                _rVal.Strukture = _Struc.WorldToDevice(aVector.Strukture, aViewVector)
            End If
            Return _rVal
        End Function
        Public Function UpdateEntity(aEntity As dxfEntity, Optional bEraseFlag As Boolean = True) As Boolean
            '#1the handle of the entity to be update
            '#2flag to earse the enity bewfore redrawing it
            '^redraws the entity is its handle is part of the current images entities collection
            Return dxfGlobals.goEvents.UpdateEntityGraphic(aEntity, bEraseFlag)
        End Function
        Friend Function UpdateStructure(aViewPlane As TPLANE, Optional aCaller As String = "", Optional bForceRedraw As Boolean = False, Optional bSuppressEvnts As Boolean = False, Optional aZoomFactor As Double? = Nothing, Optional aBackColor As Object = Nothing, Optional aUnits As dxxDeviceUnits? = Nothing) As Boolean
            Dim _rVal As Boolean = False
            If aCaller <> "" Then bForceRedraw = True
            _Struc = TDISPLAY.UpdateStructure(_Struc, aViewPlane, rChangeProps:=Nothing, aZoomFactor:=aZoomFactor, aBackColor:=aBackColor, aUnits:=aUnits, rChanged:=_rVal)
            Return _rVal
        End Function
        Public Function UpdateView(Optional aViewPlane As dxfPlane = Nothing, Optional aViewWidth As Double? = Nothing, Optional aViewHeight As Double? = Nothing, Optional aZoomFactor As Double? = Nothing, Optional aBackColor As Object = Nothing, Optional bForceRedraw As Boolean = False) As Boolean
            Dim aPln As TPLANE
            Dim aSng As Double
            If aViewPlane IsNot Nothing Then
                aPln = aViewPlane.Strukture
            Else
                aPln = _Struc.pln_VIEW
            End If
            If aViewWidth IsNot Nothing Then
                If aViewWidth.HasValue Then
                    aSng = aViewWidth.Value
                    If aSng > 0 Then aPln.Width = aSng

                End If
            End If
            If aViewHeight IsNot Nothing Then
                If aViewHeight.HasValue Then
                    aSng = aViewHeight.Value
                    If aSng > 0 Then aPln.Height = aSng

                End If

            End If
            Return UpdateStructure(aPln, "UpdateView", bForceRedraw, False, aZoomFactor, aBackColor)
        End Function
        Public Function ViewLast() As Boolean
            '^resets the display to the last rendered display settings
            If _Struc.ZoomLast <= 0 Then
                _Struc.ZoomLast = _Struc.ZoomFactor
            End If
            If TVECTOR.IsNull(_Struc.pln_VIEWLAST.ZDirection) Then
                _Struc.pln_VIEWLAST = _Struc.pln_VIEW
            End If
            Return UpdateStructure(_Struc.pln_VIEWLAST, "ViewLast", aZoomFactor:=_Struc.ZoomLast)
        End Function
        Public Function Zoom(aFraction As Double, Optional aZoomCenter As dxfVector = Nothing) As Boolean
            Dim _rVal As Boolean = False
            '#1a scaler to apply to the current zoom factor
            '#2a new zoom center to apply
            '^applies the passed zoom fraction and center
            Dim aPln As TPLANE = _Struc.pln_VIEW
            Dim ZF As Double
            ZF = _Struc.ZoomFactor
            If Math.Abs(aFraction) > 0 Then
                ZF = Math.Abs(aFraction) * ZF
            End If
            If aZoomCenter IsNot Nothing Then aPln.Origin = aZoomCenter.Strukture
            _rVal = UpdateStructure(aPln, "Zoom", aZoomFactor:=ZF)
            'If Image.UsingDxfViewer Then
            '    Image.RaiseZoomEvent(False, ZF)
            'End If
            Return _rVal
        End Function
        Public Sub ZoomBox(aX1 As Double, aY1 As Double, aX2 As Double, aY2 As Double)
            '#1the X of the start pt of the zoom diagonal in device coordinates (pixels)
            '#2the Y of the start pt of the zoom diagonal in device coordinates (pixels)
            '#3the X of the end pt of the zoom diagonal in device coordinates (pixels)
            '#4the Y of the end pt of the zoom diagonal in device coordinates (pixels)
            '^zooms the image to encompass the rectangle described by the passed diagonal
            Dim vPlane As TPLANE = _Struc.pln_VIEW.Clone
            Dim ZF As Double
            Dim LL As TVECTOR
            Dim UR As TVECTOR
            Dim org As TVECTOR
            LL = New TVECTOR(aX1, aY1, 0)
            UR = New TVECTOR(aX2, aY2, 0)
            org = LL.Interpolate(UR, 0.5)
            If LL.DistanceTo(UR, 0) <= 0 Then Return
            LL = _Struc.DeviceToWorld(LL)
            UR = _Struc.DeviceToWorld(UR)
            org = _Struc.DeviceToWorld(org)
            vPlane.Width = Math.Abs(LL.X - UR.X)
            vPlane.Height = Math.Abs(UR.Y - LL.Y)
            ZF = _Struc.FactorToView(vPlane)
            vPlane.Origin = vPlane.WorldVector(org)
            vPlane.Origin = org
            UpdateStructure(vPlane, "ZoomBox", True, aZoomFactor:=ZF)
        End Sub
        Public Function ZoomExtents(Optional aMargin As Double = 1.05, Optional bSetFeatureScale As Boolean = False, Optional bRegeneratePaths As Boolean = False, Optional bDefaultToLimits As Boolean = False) As Double
            Dim aImage As dxfImage = Image
            If aImage Is Nothing Then Return 0
            '#1a scale factor to apply to the current extents
            '#2flag to the feature scales to the zoom factor that encompasses the current extent
            '#3flag to force the regeneration of all entity paths
            '^zooms the display to encompass the entire current extents of the drawing
            Dim _rVal As Double = aImage.Render(aMargin, bRegeneratePaths, bZoomExtents:=True, bSetFeatureScale:=bSetFeatureScale)
            'If aImage.UsingDxfViewer Then aImage.RaiseZoomEvent(True, aMargin)
            If bSetFeatureScale Then _rVal = aImage.Display.PaperScale
            Return _rVal
        End Function
        Public Function ZoomToLimits(Optional aWidthBuffer As Double? = Nothing, Optional aHeightBuffer As Double? = Nothing, Optional bSetFeatureScales As Boolean = False, Optional bSuppressAutoRedraw As Boolean = False, Optional bForceRedraw As Boolean = False) As Double
            If Not _Struc.HasLimits Then Return 1 Else Return ZoomOnRectangle(New dxfRectangle(_Struc.pln_LIMITS), aWidthBuffer, aHeightBuffer, bSetFeatureScales, bSuppressAutoRedraw, bForceRedraw)
        End Function
        Public Function ZoomOnRectangle(aRectangle As iRectangle, Optional aWidthBuffer As Double? = Nothing, Optional aHeightBuffer As Double? = Nothing, Optional bSetFeatureScales As Boolean = False, Optional bSuppressAutoRedraw As Boolean = False, Optional bForceRedraw As Boolean = False, Optional bSaveAsLimits As Boolean = False) As Double
            Dim _rVal As Double = 0.0
            If aRectangle Is Nothing Then Return _rVal
            _rVal = ZoomFactor

            Dim wuz As Boolean = _Struc.AutoRedraw
            Dim vPln As TPLANE = _Struc.pln_VIEW

            If bForceRedraw Then bSuppressAutoRedraw = False
            If bSuppressAutoRedraw Then _Struc.AutoRedraw = False

            Dim aPln As TPLANE = _Struc.pln_DEVICE
            'to inches
            aPln.Width /= _Struc.DPI
            aPln.Height /= _Struc.DPI
            vPln.Origin = New TVECTOR(aRectangle.Center)
            _rVal = 1 / aPln.BestFitScale(aRectangle.Width, aRectangle.Height, False, False, aWidthBuffer, aHeightBuffer)
            Dim bNewView As Boolean = UpdateStructure(vPln, "ZoomOnRectangle", bForceRedraw, False, _rVal)
            If bSetFeatureScales Then SetFeatureScales(1 / _rVal)
            If bSaveAsLimits Then SetLimits(New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.BottomLeft)), New dxfVector(_Struc.pln_DEVICE.Point(dxxRectanglePts.TopLeft)))
            _Struc.AutoRedraw = wuz

            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Sub AlignControlToEntity(aControl As Object, aEntity As dxfEntity, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft, Optional bSizeToRectangle As Boolean = False, Optional aRectangleScaleFactor As Double = 0.0, Optional aXOffset As Long = 0, Optional aYOffset As Long = 0, Optional aWidthAdder As Long = 0, Optional aHeightAdder As Long = 0)
            '#1the control to align
            '#2the entity to align the control to
            '#3 the aligment to apply
            '#4flag to size the control to equal the bounds of the entity
            '#5a scale factor to apply to the bounding rectangle of the entity
            '#6an additional amount to add to the Left property of the control
            '#7an additional amount to add to the Top property of the control
            '#6an additional amount to add to the width of the control
            '#7an additional amount to add to the height of the control
            '^aligns the passed control to the passed entity
            'On Error Resume Next
            If aEntity Is Nothing Then Return
            If aControl Is Nothing Then Return
            aEntity.UpdatePath()
            AlignControlToRectangle(aControl, aEntity.BoundingRectangle, aAlignment, bSizeToRectangle, aRectangleScaleFactor, aXOffset, aYOffset, aWidthAdder, aHeightAdder)
        End Sub
        Public Shared Sub AlignControlToRectangle(aControl As Object, aRect As dxfRectangle, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft, Optional bSizeToRectangle As Boolean = False, Optional aRectangleScaleFactor As Double = 0.0, Optional aXOffset As Long = 0, Optional aYOffset As Long = 0, Optional aWidthAdder As Long = 0, Optional aHeightAdder As Long = 0)
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            '#1the control to align
            '#2the rectangle to align the control to
            '#3 the aligment to apply
            '#4flag to size the control to equal the size of the rectangle
            '#5a scale factor to apply to the rectangle
            '#6an additional amount to add to the Left property of the control
            '#7an additional amount to add to the Top property of the control
            '#6an additional amount to add to the width of the control
            '#7an additional amount to add to the height of the control
            '^aligns the passed control to the passed rectangle
            ''On Error Resume Next
            'If aRect Is Nothing Then Return
            'If aControl Is Nothing Then Return
            'On Error GoT2 Err
            'Dim aCntrl As Control
            'Dim bRec As dxfRectangle
            'Dim wd1 As Object
            'Dim wd2 As Object
            'Dim ht1 As Object
            'Dim ht2 As Object
            'Dim aCnt As Vanara.PInvoke.RECT
            'Dim tL1 As TVECTOR
            'Dim tL2 As TVECTOR
            'Dim br1 As TVECTOR
            'Dim br2 As TVECTOR
            'Dim xoset As Double
            'Dim yoset As Double
            'Dim hAlign As dxxTextJustificationsHorizontal
            'Dim vAlign As dxxTextJustificationsVertical
            'Dim aHwnd As Long
            'Dim tname As String = String.Empty
            'Dim aShape As Shape
            'Dim smode As Long
            'aCntrl = aControl
            'tname = UCase(TypeName(aCntrl))
            ''================================================================
            ''get the entities top left and bottom right corners
            'bRec = aRect
            'If aRectangleScaleFactor > 0 And aRectangleScaleFactor <> 1 Then
            '    bRec = bRec.Expanded(aRectangleScaleFactor)
            'End If
            ''transform the corners of the entity to the curent view plane in pixels
            'tL1 = _Struc.WorldToDevice( bRec.TopLeftV)
            'br1 = _Struc.WorldToDevice( bRec.BottomRightV)
            ''get the width and height of the entity rounded up to the nearest pixel
            'wd1 = Fix(Math.Abs(br1.X - tL1.X)) + 1
            'ht1 = Fix(Math.Abs(tL1.Y - br1.Y)) + 1
            ''================================================================================
            ''apply the requested alignment
            'xoset = 0
            'yoset = 0
            'TFONT.EncodeAlignment(aAlignment, vAlign, hAlign)
            'If vAlign = dxxTextJustificationsVertical.Bottom Then
            '    yoset = ht1
            'ElseIf vAlign = dxxTextJustificationsVertical.Middle Then
            '    yoset = 0.5 * ht1
            'End If
            'If hAlign = dxxTextJustificationsHorizontal.Center Then
            '    xoset = 0.5 * wd1
            'ElseIf hAlign = dxxTextJustificationsHorizontal.Right Then
            '    xoset = wd1
            'End If
            ''align the control
            'If tname = "SHAPE" Then
            '    aShape = aCntrl
            '        smode = aShape.Container.ScaleMode
            '        aShape.Left = aShape.Container.ScaleX(tL1.X + xoset + aXOffset, vbPixels, smode)
            '        aShape.Top = aShape.Container.ScaleY(tL1.Y + yoset + aYOffset, vbPixels, smode)
            '        If bSizeToRectangle Then
            '            aShape.Width = aShape.Container.ScaleX(wd1 + aWidthAdder, vbPixels, smode)
            '            aShape.Height = aShape.Container.ScaleY(ht1 + aHeightAdder, vbPixels, smode)
            '        End If
            'Else
            '    'On Error Resume Next
            '    aHwnd = aCntrl.hWnd
            '    '================================================================
            '    'get the bounds of the window of the passed control
            '    'assuming that its containing DC is our images picturebox, form etc.
            '    GetWindowRect(aHwnd, aCnt)
            '    tL2.X = aCnt.Left
            '    tL2.Y = aCnt.Top
            '    br2.X = aCnt.Right
            '    br2.Y = aCnt.Bottom
            '    wd2 = Math.Abs(br2.X - tL2.X)
            '    ht2 = Math.Abs(br2.Y - tL2.Y)
            '    'size the control to cover the entities bounding rectangle by
            '    'setting the dimensions
            '    If bSizeToRectangle Then
            '        wd2 = wd1
            '        ht2 = ht1
            '    End If
            '    MoveWindow(aHwnd, TVALUES.To_LNG(tL1.X + xoset + aXOffset), TVALUES.To_LNG(L1.Y + yoset + aYOffset), wd2 + Math.Abs(aWidthAdder), ht2 + Math.Abs(aHeightAdder), 1)
            'End If
            Return
Err:
        End Sub
#End Region 'Shared Methods
    End Class 'dxoDisplay
End Namespace
